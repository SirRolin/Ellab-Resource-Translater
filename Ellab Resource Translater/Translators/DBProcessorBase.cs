using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Objects.Extensions;
using Ellab_Resource_Translater.Util;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;

namespace Ellab_Resource_Translater.Translators
{
    public class DBProcessorBase(TranslationService? TranslationService, ConnectionProvider? connProv, CancellationTokenSource source, int systemEnum, int maxThreads = 32)
    {
        private readonly Config config = Config.Get();

        private readonly CancellationToken token = source.Token;

        public DatabaseTransactionHandler? dth;

        public void Run(string path, ListView view, Label progresText, Regex regex)
        {
            /// Local Functions to make the data transaction handler call more readable.
            Action<DbConnection, DbTransaction?> onTransactionStart(int systemEnum, Label progresText)
            {
                return (dbc, trans) =>
                {
                    // FetchData from Database that has entries in ChangedTranslations.
                    UpdateLocalFiles(path, view, progresText);

                    // Removing the relavant translations on the database.
                    progresText.Invoke(() => progresText.Text = "Clearing Database.");
                    var c = dbc.CreateCommand();
                    c.Transaction = trans;
                    c.CommandType = CommandType.Text;
                    // To avoid possible errors I have excluded those that have changed translations, though those should had been deleted in the previous step.
                    // Edge Case: While updating local files someone could add a new translation to the database.
                    c.CommandText = $@"DELETE a FROM Translation a where a.SystemEnum = {systemEnum} AND NOT a.ID in (Select ct.TranslationID from ChangedTranslation ct);";
                    c.ExecuteNonQuery();
                    c.Dispose();

                    // Read, Translate, Write & prepare dth.
                    SetupTasks(path, view, progresText, regex);
                };
            }

            static Action<DataRow, Interfaces.IDBparameterable> addParameters()
            {
                return (row, paramable) =>
                {
                    paramable.AddParam(row, "Comment", DbType.String);
                    paramable.AddParam(row, "Key", DbType.String);
                    paramable.AddParam(row, "LanguageCode", DbType.String);
                    paramable.AddParam(row, "ResourceName", DbType.String);
                    paramable.AddParam(row, "Text", DbType.String);
                    paramable.AddParam(row, "IsTranlatedInValSuite", DbType.Boolean);
                    paramable.AddParam(row, "SystemEnum", DbType.Int32);
                };
            }

            // Setup a transaction handler, this makes it possible to cancel our work and revert back the changes on the database.
            dth = new(
                    source: source,
                    onTransactionStart: onTransactionStart(systemEnum, progresText),
                    // Key have to have quotes as "Key" is a keyword used in SQL
                    commandText: @"
                              INSERT INTO Translation 
                                  (Comment, ""Key"", LanguageCode, ResourceName, Text, IsTranlatedInValSuite, SystemEnum) 
                              VALUES 
                                  (@Comment, @Key, @LanguageCode, @ResourceName, @Text, @IsTranlatedInValSuite, @SystemEnum);",

                    addParameters: addParameters(),
                    inserters: Config.Get().insertersToUse);


            // Starts transfer to Database
            if (connProv != null && !token.IsCancellationRequested)
            {
                dth.StartCommands(connProv, progresText, view, (DataTable dt) =>
                {
                    string output = "<Unknown>";
                    // On Error DataTables are cleared, emptying them, but 
                    if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("ResourceName"))
                        output = dt.Rows[0]["ResourceName"].ToString() ?? "<Unknown>";
                    return output;
                });
            }
        }

        /// <summary>
        /// Fetches Data from the database and updates local resource files.
        /// </summary>
        /// <param name="path">root path</param>
        /// <param name="view">the ViewList that shows which items are being processed atm.</param>
        /// <param name="progresText">The Label that is updated with the progres</param>
        /// <exception cref="NotImplementedException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0306:Simplify collection initialization", Justification = "it's a lie, collection initialization doesn't work for ConcurrentQueues")]
        private void UpdateLocalFiles(string path, ListView view, Label progresText)
        {
            // Update UI
            FormUtils.LabelTextUpdater(progresText, "Fetching changed translations from DB.");

            if (connProv != null && !token.IsCancellationRequested)
            {
                using DbConnection dce = connProv.Get();

                // Read from the DB
                using DbCommand command = dce.CreateCommand();
                command.CommandText = $@"WITH changedTranslationsLatest AS (
                                          SELECT
                                            *,
                                            ROW_NUMBER() OVER(PARTITION BY TranslationID ORDER BY ID DESC) AS ReverseRowNumber
                                          FROM ChangedTranslation
                                        )
                                        SELECT a.""Key"", a.ResourceName, a.LanguageCode, a.Comment, b.ChangedText, b.ID as ""ChangedID""
                                        FROM Translation a JOIN changedTranslationsLatest b ON a.ID = b.TranslationID WHERE b.ReverseRowNumber = 1 and a.SystemEnum = {systemEnum};";
                
                dce.WaitForOpen(() =>
                {
                    source.Cancel();
                    source.Token.ThrowIfCancellationRequested();
                });
                
                using DbDataReader changedTexts = command.ExecuteReader();

                // Load changes
                if (changedTexts.HasRows)
                {
                    // Quickly Extract Data to DataTables, so we can quantify the tables and close the connection.
                    ConcurrentQueue<DataTable> dataTables = [];
                    while (!changedTexts.IsClosed)
                    {
                        DataTable table = new();
                        table.Load(changedTexts);
                        dataTables.Enqueue(table);
                    }

                    dce.TryClose();

                    // local helper function that updates the GUI.
                    void myUpdate(string pretext, Ref<int> currentProgress, int maxProgresses)
                    {
                        Interlocked.Increment(ref currentProgress.value);
                        FormUtils.LabelTextUpdater(progresText, pretext, currentProgress, " out of ", maxProgresses);
                    }

                    // Merging the Tables with column data so we can fetch it later.
                    // Reasoning for this is so we can multi-thread
                    ConcurrentQueue<(int dataTNum, DataRow row)> dataRows = [];
                    ConcurrentDictionary<int, (DataColumn resource, DataColumn key, DataColumn value, DataColumn comment)> dataColumns = [];
                    GetRowsAndColumnsFromDataTable(maxThreads, view, dataTables, dataRows, dataColumns, myUpdate);

                    // Groups the data with the Dictionary.
                    // Reasoning is that if we don't do this we wouldn't be able to multi-thread it, due to opening files with readwrite access blocks other threads from doing the same on the same file.
                    // Even if it wouldn't block, it would likely cause problems or at the very least be less efficient as it would open the same files multiple times.
                    ConcurrentDictionary<string, List<MetaData<object?>>> changesToRegister = [];
                    GroupByResourceFileFromRowsAndColumns(maxThreads, view, dataRows, dataColumns, changesToRegister, myUpdate);

                    // ConcurrentDictionary can't be popped, so we have to have a queue of it's keys
                    ConcurrentQueue<string> files = new(changesToRegister.Keys);

                    // Process Changes by the ResourceFile
                    UpdateLocalFilesFromGroupedData(maxThreads, path, view, changesToRegister, files, myUpdate);
                    using DbCommand deleteCommand = dce.CreateCommand();
                    deleteCommand.CommandText = $@"DELETE FROM ChangedTranslation WHERE ChangedTranslation.TranslationID in (SELECT ID FROM Translation where SystemEnum = {systemEnum});";
                    dce.WaitForOpen(() =>
                    {
                        source.Cancel();
                        source.Token.ThrowIfCancellationRequested();
                    });
                    deleteCommand.ExecuteNonQuery();
                    deleteCommand.Dispose();
                    dce.CloseAsync();
                }
            }
        }

        private static void UpdateLocalFilesFromGroupedData(int maxThreads,
                                                            string path,
                                                            ListView view,
                                                            ConcurrentDictionary<string, List<MetaData<object?>>> changesToRegister,
                                                            ConcurrentQueue<string> files,
                                                            Action<string, Ref<int>, int> myUpdate)
        {
            int currentProgress = -1;
            int fileCount = files.Count;
            const string TITLE = "Saving Changes: ";

            // Initial Update of UI
            myUpdate(TITLE, currentProgress, fileCount);
            void processChanges(string rootPath, string transDictKey)
            {
                var resourcePath = string.Concat(rootPath, rootPath.EndsWith('/') ? "" : '/', transDictKey);

                // Load Local data so we don't lose data that wasn't overriden
                var translations = ResourceHandler.ReadResource<object?>(resourcePath);

                // Override in Memory
                var changes = changesToRegister[transDictKey];
                changes.ForEach(trans =>
                {
                    if (translations.TryGetValue(trans.key, out MetaData<object?>? oldtrans))
                    {
                        oldtrans.value = trans.value;
                    }
                    else
                    {
                        translations.Add(trans.key, trans);
                    }
                });

                // Save to Local Data
                ResourceHandler.WriteResource(resourcePath, translations);
            }
            ExecutionHandler.Execute(maxThreads, fileCount, (int i) =>
            {
                while (files.TryDequeue(out var resourceName))
                {
                    FormUtils.ShowOnListWhileProcessing(
                        update: () => myUpdate(TITLE, currentProgress, fileCount),
                        listView: view,
                        resourceName: i + ") Fetching Data...",
                        process: () => processChanges(path, resourceName));
                    Interlocked.Increment(ref currentProgress);
                }
            });
        }

        private static void GroupByResourceFileFromRowsAndColumns(int maxThreads,
                                                                  ListView view,
                                                                  ConcurrentQueue<(int dataTNum, DataRow row)> dataRows,
                                                                  ConcurrentDictionary<int, (DataColumn resource, DataColumn key, DataColumn value, DataColumn comment)> dataColumns,
                                                                  ConcurrentDictionary<string, List<MetaData<object?>>> changesToRegister,
                                                                  Action<string, Ref<int>, int> myUpdate)
        {
            int currentProgress = 0;
            int rowCount = dataRows.Count;
            const string TITLE = "Grouping Data on same Resource Files: ";

            // Initial Update of UI
            myUpdate(TITLE, currentProgress, rowCount);
            void processRow(DataRow row, int dataTNumber)
            {
                if (row[dataColumns[dataTNumber].resource] is string resourceValue
                    && row[dataColumns[dataTNumber].key] is string keyValue
                    && row[dataColumns[dataTNumber].value] is string valueValue
                    && row[dataColumns[dataTNumber].comment] is string commentValue)
                {
                    changesToRegister.AddOrUpdate(resourceValue, [new MetaData<object?>(keyValue, valueValue, commentValue)],
                        (key, orgList) =>
                        {
                            orgList.Add(new MetaData<object?>(keyValue, valueValue, commentValue));
                            return orgList;
                        });
                }
            }
            ExecutionHandler.Execute(maxThreads, rowCount, (threadNum) =>
            {
                while (dataRows.TryDequeue(out var rowData))
                {
                    FormUtils.ShowOnListWhileProcessing(
                        update: () => myUpdate(TITLE, currentProgress, rowCount),
                        listView: view,
                        resourceName: threadNum + ") Fetching Data...",
                        process: () => processRow(rowData.row, rowData.dataTNum));
                    Interlocked.Increment(ref currentProgress);
                }
            });
        }

        private static void GetRowsAndColumnsFromDataTable(int maxThreads,
                                                           ListView view,
                                                           ConcurrentQueue<DataTable> dataTables,
                                                           ConcurrentQueue<(int dataTNum, DataRow row)> dataRows,
                                                           ConcurrentDictionary<int, (DataColumn resource, DataColumn key, DataColumn value, DataColumn comment)> dataColumns,
                                                           Action<string, Ref<int>, int> myUpdate)
        {

            int currentProgress = -1;
            int tableCount = dataTables.Count;
            const string TITLE = "Merging tables: ";

            // Initial Update of UI
            myUpdate(TITLE, currentProgress, tableCount);
            void processTable(DataTable dt)
            {
                if (dt.Columns["ResourceName"] is DataColumn resourceColumn
                    && dt.Columns["Key"] is DataColumn keyColumn
                    && dt.Columns["ChangedText"] is DataColumn textColumn
                    && dt.Columns["Comment"] is DataColumn commentValue)
                {
                    dataColumns.TryAdd(currentProgress, (resourceColumn, keyColumn, textColumn, commentValue));
                    foreach (DataRow row in dt.Rows)
                    {
                        dataRows.Enqueue((currentProgress, row));
                    }
                }
            }
            ExecutionHandler.Execute(tableCount, maxThreads, (int threadNum) =>
            {
                while (dataTables.TryDequeue(out DataTable? table))
                {
                    FormUtils.ShowOnListWhileProcessing(
                        update: () => myUpdate(TITLE, currentProgress, tableCount),
                        listView: view,
                        resourceName: threadNum + ") Fetching Data...",
                        process: () => processTable(table));
                }
            });
        }

        private void SetupTasks(string path, ListView view, Label progresText, Regex regex)
        {
            // Tracks resources done - Starts at -1 so that we can call it to get the right format to start with.
            int currentProcessed = -1;
            var allResources = Directory.GetFiles(path, "*.resx", SearchOption.AllDirectories).ToHashSet();
            var englishQueuedFiles = new ConcurrentQueue<string>(allResources.Where(x => regex.IsMatch(x)));

            int maxProcesses = englishQueuedFiles.Count;

            // Doing this so we don't have to pass both a int ref and a Label ref
            void updateProgresText()
            {
                Interlocked.Increment(ref currentProcessed);
                FormUtils.LabelTextUpdater(progresText, "Preparing ", currentProcessed, " out of ", maxProcesses);
            }
            updateProgresText();

            void onFailing(AggregateException ae)
            {
                foreach (Exception e in ae.InnerExceptions)
                {
                    if (e is TaskCanceledException)
                        englishQueuedFiles.Clear();
                    else
                        Console.WriteLine("Exception: " + e.GetType().Name);
                }
            }
            // Execution Time
            ExecutionHandler.TryExecute(maxThreads, allResources.Count,
                    action: (i) => ProcessQueue(rootPathLength: path.Length,
                                                               queue: englishQueuedFiles,
                                                               existingResources: allResources,
                                                               update: updateProgresText,
                                                               listView: view),
                    onFailing: (Action<AggregateException>)onFailing
                );
        }

        private void ProcessQueue(int rootPathLength, ConcurrentQueue<string> queue, HashSet<string> existingResources, Action update, ListView listView)
        {
            while (!source.IsCancellationRequested && queue.TryDequeue(out var resource))
            {
                void TryTranslateResource()
                {
                    try
                    {
                        TranslateResource(existingResources, resource, rootPathLength);
                    }
                    catch (AggregateException ae)
                    {
                        if (!source.IsCancellationRequested)
                        {
                            source.Cancel();
                            Debug.WriteLine(ae.Message);
                            MessageBox.Show("Translations used up for now. Try again later.");
                        }
                    }
                }
                FormUtils.ShowOnListWhileProcessing(rootPathLength, update, listView, resource, TryTranslateResource);
            }
        }

        private void TranslateResource(HashSet<string> existingFiles, string resource, int pathLength)
        {
            // Translations work
            var langs = config.languagesToTranslate.ToArray();
            Dictionary<string, Dictionary<string, MetaData<object?>>> translations = ResourceHandler.GetAllLangResources(existingFiles, resource, langs, Config.Get().languagesToAiTranslate, TranslationService);

            // Save Translations
            foreach (var item in translations)
            {
                if (!item.Key.Equals("EN", StringComparison.OrdinalIgnoreCase))
                    ResourceHandler.WriteResource(Path.ChangeExtension(resource, $".{item.Key.ToLower()}.resx"), item.Value);
            }

            // Try to write to the Database
            WriteToDatabase(pathLength, resource, translations);
        }


        private void WriteToDatabase(int pathLength, string resource, Dictionary<string, Dictionary<string, MetaData<object?>>> translations)
        {
            if (dth != null)
            {
                // Filter to only be strings as others don't need translating
                var toUpload = translations.Select(lang => new KeyValuePair<string, Dictionary<string, MetaData<string>>>(lang.Key, MetaData<string>.FilterTo(lang.Value)))
                                           .ToDictionary();
                if (toUpload != null) {
                    DataTable dataTable = CreateDataTable<string>(pathLength, resource, toUpload, systemEnum);
                    if (dataTable.Rows.Count > 0)
                        dth.AddInsert(dataTable);
                }
            }
        }

        private static DataTable CreateDataTable<T>(int pathLength, string resource, Dictionary<string, Dictionary<string, MetaData<T>>> translations, int systemEnum)
        {
            DataTable dataTable = new();
            dataTable.Columns.Add("ID", typeof(long));
            dataTable.Columns.Add("Comment", typeof(string));
            dataTable.Columns.Add("Key", typeof(string));
            dataTable.Columns.Add("LanguageCode", typeof(string));
            dataTable.Columns.Add("ResourceName", typeof(string));
            dataTable.Columns.Add("Text", typeof(string));
            dataTable.Columns.Add("IsTranlatedInValSuite", typeof(bool)); // The Spelling Error is on the Database
            dataTable.Columns.Add("SystemEnum", typeof(int));
            foreach (var item in translations)
            {
                string language = item.Key;
                // add language string (if not english), then cut rootPath away.
                string resourceName = (item.Key.Equals("EN", StringComparison.OrdinalIgnoreCase) ? resource : Path.ChangeExtension(resource, $".{item.Key.ToLower()}.resx"))[(pathLength + 1)..];
                foreach (var value in item.Value)
                {
                    // it's only useful to send strings up
                    if (value.Value.value is not string)
                        continue;
                    string comment = value.Value.comment;
                    string key = value.Key;
                    object text = value.Value.value; // Stonks

                    dataTable.Rows.Add(null, // ID, should autogenerate
                                        comment,
                                        key,
                                        language,
                                        resourceName,
                                        text,
                                        false,
                                        systemEnum
                                        );
                }
            }

            return dataTable;
        }
    }
}