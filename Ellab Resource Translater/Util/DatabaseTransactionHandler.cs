using Ellab_Resource_Translater.Enums;
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
    internal class DatabaseTransactionHandler(CancellationTokenSource source, Action<DbConnection, DbTransaction?> onTransactionStart, string commandText, Action<DataRow, IDBparameterable> addData, int maxThreads = 32)
    {
        private readonly ConcurrentQueue<DataTable> insertToDatabaseTasks = [];

        private readonly object lockObj = new();

        private readonly CancellationToken token = source.Token;


        public void AddInsert(DataTable dt)
        {
            lock (lockObj)
            {
                insertToDatabaseTasks.Enqueue(dt);
            }
        }

        public void StartCommands(ConnectionProvider connProv, Label progresText, ListView listView, int pathLength, Func<DataTable, string> getResourceName)
        {
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

            void execution(DbTransaction? transAct)
            {
                onTransactionStart.Invoke(dce.conn, transAct);
                ExecutionHandler.Execute(maxThreads, insertToDatabaseTasks.Count, (i) =>
                {
                    while (insertToDatabaseTasks.TryDequeue(out var dt))
                    {
                        if (token.IsCancellationRequested)
                            break;
                        string resource = getResourceName.Invoke(dt);
                        FormUtils.HandleProcess(updateProgresText, listView, resource, () => process(dce, transAct, dt));
                    }
                });
            }

            bool process(DbConnectionExtension dce, DbTransaction? transAct, DataTable item)
            {
                if (token.IsCancellationRequested)
                    return false;
                else
                    BuildAndExecute(dce, item, transAct);
                return true;
            }

            dce.WaitForOpen(source.Cancel);
            TryScopedTransaction(token: token,
                                dce: dce,
                                execution: execution);

            dce.WaitForFinish();
        }

        private void BuildAndExecute(DbConnectionExtension DBCon, DataTable dataTable, DbTransaction? trans)
        {
            bool batchFailed = false;
            Task PreviousTask = Task.CompletedTask;
            // Upload to the Database
            if (DBCon.conn.CanCreateBatch)
            {
                var s = DBCon.conn.CreateBatch();
                if(trans != null)
                    s.Transaction = trans;

                foreach (DataRow row in dataTable.Rows)
                {
                    var c = s.CreateBatchCommand();
                    c.CommandText = commandText;
                    DBBatchCommandWrapper cwrapped = new(c);
                    addData.Invoke(row, cwrapped);
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
                using DbCommand command = DBCon.conn.CreateCommand();
                command.CommandText = commandText;
                if (trans != null)
                    command.Transaction = trans;

                DBCommandWrapper wrappedCommand = new(command);

                foreach (DataRow row in dataTable.Rows)
                {
                    addData.Invoke(row, wrappedCommand);

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

        private void CommandExecuteElseMessage(DataTable dataTable, DbConnectionExtension dce, DbCommand command)
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

        public static bool TryGetTransactionScope(DbConnectionExtension connection, out TransactionScope? transactionScope)
        {
            ConnType dbType = DBStringHandler.DetectType(connection.conn.ConnectionString);
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
        /// <param name="token"><see cref="CancellationToken"/> to cancel with the process.</param>
        /// <param name="dce">connection wrapped in my extension class.</param>
        /// <param name="execution"></param>
        /// <returns>DTC supported? <see langword="true"/>, otherwise <see langword="false"/></returns>
        public static bool TryScopedTransaction(CancellationToken token, DbConnectionExtension dce, Action<DbTransaction?> execution)
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
                using DbTransaction dbTrans = dce.conn.BeginTransaction();
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
