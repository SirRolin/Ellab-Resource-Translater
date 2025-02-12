using Ellab_Resource_Translater.Enums;
using Ellab_Resource_Translater.Interfaces;
using Ellab_Resource_Translater.Objects;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Azure;
using Mysqlx.Crud;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Ellab_Resource_Translater.Util
{
    /// <summary>
    /// Starts a Transaction and calls <paramref name="onTransactionStart"/>,<br/>
    /// <see cref="DataRow"/>s in the <see cref="DataTable"/> provided with <see cref="AddInsert(DataTable)"/><br/>
    /// are fed into <paramref name="addParameters"/> to give commands with <paramref name="commandText"/> it's parameters<br/>
    /// This uses <paramref name="inserters"/> threads. runs in parallel if not positive.
    /// </summary>
    /// <param name="source">Token to cancel with.</param>
    /// <param name="onTransactionStart">Action Run right after transaction start.</param>
    /// <param name="commandText">SQL Query</param>
    /// <param name="addParameters">How we add the parameters to the command.</param>
    /// <param name="inserters">How many threads to use, runs in parallel if not positive.</param>
    internal class DatabaseTransactionHandler(CancellationTokenSource source, Action<DbConnection, DbTransaction?> onTransactionStart, string commandText, Action<DataRow, IDBparameterable> addParameters, int inserters = 4)
    {
        private readonly ConcurrentQueue<DataTable> insertToDatabaseTasks = [];
        private readonly object lockObj = new();
        private readonly CancellationToken token = source.Token;
        private int insertsPending = 0;
        private bool _waitTillStopped = false;

        /// <summary>
        /// Threadsafely inserts the table <paramref name="dt"/> to the internal queue.<br/>
        /// use <see cref="StartCommands(ConnectionProvider, Label, ListView, Func{DataTable, string}, bool)"/>.
        /// </summary>
        /// <param name="dt"><see cref="DataTable"/> to extract rows from and create the parameters for the command.</param>
        public void AddInsert(DataTable dt)
        {
            Interlocked.Increment(ref insertsPending);
            lock (lockObj)
            {
                insertToDatabaseTasks.Enqueue(dt);
            }
            Interlocked.Decrement(ref insertsPending);
        }

        /// <summary>
        /// Starts processing everything feeded into it.
        /// </summary>
        /// <param name="connProv">to generate a <see cref="DbConnection"/> and automatically close it.</param>
        /// <param name="progresText"><see cref="Label"/> of which to update progress.</param>
        /// <param name="listView"><see cref="ListView"/> of which to show current processes</param>
        /// <param name="getResourceName">Function that takes the <see cref="DataTable"/> and returns a the process shown.</param>
        /// <param name="waitTillStopped">When <see langword="false"/> will only continue until there's no current or pending inserts.<br/>
        /// When <see langword="true"/> will also wait for <see cref="NoMoreInserts"/> to be called.</param>
        public void StartCommands(ConnectionProvider connProv, Label progresText, ListView listView, Func<DataTable, string> getResourceName, bool waitTillStopped = false)
        {
            _waitTillStopped = waitTillStopped;
            int currentProcessed = -1;
            int maxProcesses = insertToDatabaseTasks.Count;
            // Doing this so we don't have to pass both a int ref and a Label ref
            void updateProgresText()
            {
                Interlocked.Increment(ref currentProcessed);
                FormUtils.LabelTextUpdater(progresText, "Inserting ", currentProcessed, " out of ", maxProcesses, " into database.");
            }
            updateProgresText();
            using var dce = connProv.Get();

            bool process(DbConnection dce, DbTransaction? transAct, DataTable item)
            {
                if (token.IsCancellationRequested)
                    return false;
                else
                    BuildAndExecute(dce, item, transAct);
                return true;
            }

            void execution(DbTransaction? transAct)
            {
                onTransactionStart.Invoke(dce, transAct);
                ExecutionHandler.Execute(inserters, insertToDatabaseTasks.Count, (i) =>
                {
                    while (insertToDatabaseTasks.TryDequeue(out var dt) || insertsPending > 0 || _waitTillStopped)
                    {
                        // if more are pending
                        if (dt != null)
                        {
                            if (token.IsCancellationRequested)
                                break;
                            string resource = getResourceName.Invoke(dt);
                            FormUtils.HandleProcess(updateProgresText, listView, resource, () => process(dce, transAct, dt));
                        } else
                        {
                            Task.Delay(100).Wait();
                        }
                    }
                });
            }

            dce.WaitForOpen(source.Cancel);
            TryScopedTransaction(dce: dce,
                                execution: execution,
                                token: token);

            dce.WaitForFinish();
        }

        /// <summary>
        /// Only does something if <see cref="StartCommands(ConnectionProvider, Label, ListView, Func{DataTable, string}, bool)"/> was called with <see langword="true"/> for <see langword="waitTillStopped"/><br/>
        /// when no more <see cref="DataTable"/>s are available to insert.
        /// </summary>
        /// <returns></returns>
        public bool NoMoreInserts()
        {
            bool output = _waitTillStopped;
            _waitTillStopped = false;
            return output;
        }

        private void BuildAndExecute(DbConnection DBCon, DataTable dataTable, DbTransaction? trans)
        {
            bool batchFailed = false;
            Task PreviousTask = Task.CompletedTask;
            // Upload to the Database
            if (DBCon.CanCreateBatch)
            {
                var s = DBCon.CreateBatch();
                if(trans != null)
                    s.Transaction = trans;

                foreach (DataRow row in dataTable.Rows)
                {
                    var c = s.CreateBatchCommand();
                    c.CommandText = commandText;
                    DBBatchCommandWrapper cwrapped = new(c);
                    addParameters(row, cwrapped);
                    s.BatchCommands.Add(c);
                }

                // Sometimes there's nothing to upload
                if (s.BatchCommands.Count > 0)
                {
                    PreviousTask.Wait();
                    if (!token.IsCancellationRequested)
                    {
                        PreviousTask = Task.Run(() =>
                        {
                            try
                            {
                                s.ExecuteNonQuery();

                                // Manually Disposing of it instead of using, well using, cause otherwise it gets disposed off before it gets to execute.
                                s.Dispose();
                            }
                            catch (Exception)
                            {
                                batchFailed = true;
                            }
                        });
                    }
                }
                else
                {
                    s.Cancel();
                }
            }
            else
            {
                batchFailed = true;
            }
            if (batchFailed)
            {
                PreviousTask = Task.CompletedTask;
                using DbCommand command = DBCon.CreateCommand();
                command.CommandText = commandText;
                if (trans != null)
                    command.Transaction = trans;

                DBCommandWrapper wrappedCommand = new(command);

                foreach (DataRow row in dataTable.Rows)
                {
                    addParameters(row, wrappedCommand);

                    // We run it Asyncroniously so that we can prepare the next Task before we run it.
                    PreviousTask.Wait();
                    if (!token.IsCancellationRequested)
                    {
                        PreviousTask = Task.Run(() =>
                        {
                            DBCon.WaitForOpen();
                            CommandExecuteElseMessage(dataTable, DBCon, command);
                        });
                    }
                }
            }
            PreviousTask.Wait();
        }

        private void CommandExecuteElseMessage(DataTable dataTable, DbConnection dce, DbCommand command)
        {
            int tries = 0;
            while (tries < 2)
            {
                try
                {
                    dce.WaitForOpen();

                    command.ExecuteNonQuery();
                    break;
                }
                catch
                {
                    tries++;
                    Task.Delay(500).Wait(); // Wait half a second.
                }
                if (tries == 2)
                {
                    Cancel();
                    MessageBox.Show("failed to upload to the database:" + dataTable.ToString());
                }
            }
        }

        public void Cancel()
        {
            source.Cancel();
        }

        public static bool TryGetTransactionScope(DbConnection connection, out TransactionScope? transactionScope)
        {
            ConnType dbType = DBStringHandler.DetectType(connection.ConnectionString);
            switch (dbType)
            {
                case ConnType.MSSql:
                    transactionScope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
                        TransactionScopeAsyncFlowOption.Enabled);
                    break;

                case ConnType.PostgreSql:
                    transactionScope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
                        TransactionScopeAsyncFlowOption.Enabled);
                    break;

                default:
                    transactionScope = null;
                    return false;
            }
            return true;
        }

        /// <summary>
        /// if the connection allows transaction to share multiple connections (using DTC) <paramref name="execution"/> will be called with <see langword="null"/>.<br/>
        /// if DTC is not supported or it fails, the <see cref="DbTransaction"/> which only supports 1 connection.
        /// </summary>
        /// <param name="dce">connection wrapped in my extension class.</param>
        /// <param name="execution"></param>
        /// <param name="token"><see cref="CancellationToken"/> to cancel with the process.</param>
        /// <returns>DTC supported? <see langword="true"/>, otherwise <see langword="false"/></returns>
        public static bool TryScopedTransaction(DbConnection dce, Action<DbTransaction?> execution, CancellationToken token)
        {
            try
            {
                if (TryGetTransactionScope(dce, out TransactionScope? scope))
                {
                    using var scop = scope;
                    execution(null);
                    if (!token.IsCancellationRequested)
                        scop?.Complete();
                }
                else
                {
                    throw new Exception("Transcope don't work, using DbTransaction instead.");
                }
                return true;
            }
            catch (Exception)
            {
                using DbTransaction dbTrans = dce.BeginTransaction();
                try
                {
                    execution(dbTrans);
                    
                    dce.WaitForFinish();

                    if (token.IsCancellationRequested)
                        dbTrans.Rollback();
                    else
                        dbTrans.Commit();
                }
                catch (Exception)
                {
                    dbTrans.Rollback();
                }
            }
            return false;
        }
    }
}
