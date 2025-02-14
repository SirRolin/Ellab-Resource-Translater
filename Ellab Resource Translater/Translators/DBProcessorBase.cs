using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Objects.Extensions;
using Ellab_Resource_Translater.Util;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Resources;
using System.Text.RegularExpressions;

namespace Ellab_Resource_Translater.Translators
{
    internal class DBProcessorBase(TranslationService? TranslationService, ConnectionProvider? connProv, CancellationTokenSource source, int systemEnum, int maxThreads = 32)
    {
        private readonly Config config = Config.Get();

        private readonly CancellationToken token = source.Token;

        public DatabaseTransactionHandler? dth;

        internal void Run(string path, ListView view, Label progresText, Regex regex)
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
                var translations = ReadResource<object?>(resourcePath);

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
                WriteResource(resourcePath, translations);
            }
            ExecutionHandler.Execute(maxThreads, fileCount, (i) =>
            {
                while (files.TryDequeue(out var resourceName))
                {
                    FormUtils.ShowOnListWhileProcessing(
                        pathLength: -1, // -1 to disable 
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
            ExecutionHandler.Execute(maxThreads, rowCount, (i) =>
            {
                while (dataRows.TryDequeue(out var rowData))
                {
                    FormUtils.ShowOnListWhileProcessing(
                        update: () => myUpdate(TITLE, currentProgress, rowCount),
                        listView: view,
                        resourceName: i + ") Fetching Data...",
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
            ExecutionHandler.Execute(tableCount, maxThreads, (i) =>
            {
                while (dataTables.TryDequeue(out DataTable? table))
                {
                    FormUtils.ShowOnListWhileProcessing(
                        update: () => myUpdate(TITLE, currentProgress, tableCount),
                        listView: view,
                        resourceName: i + ") Fetching Data...",
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

        private static DataTable CreateDataTable(int pathLength, string resource, Dictionary<string, Dictionary<string, MetaData<object?>>> translations, int systemEnum)
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
                string resourceName = (item.Key.Equals("en", StringComparison.OrdinalIgnoreCase) ? resource : Path.ChangeExtension(resource, $".{item.Key.ToLower()}.resx"))[(pathLength + 1)..];
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

        private static Dictionary<string, MetaData<Type>> ReadResource<Type>(string path)
        {
            Dictionary<string, MetaData<Type>> trans = [];
            using (ResXResourceReader resxReader = new(path))
            {
                using ResXResourceReader resxCommentReader = new(path);
                // Switches to reading metaData instead of values, can't have both, which we need for comments
                resxCommentReader.UseResXDataNodes = true;

                // Found out that some files are simply broken which will cause this to throw an error when it reaches the end of the file.
                try
                {
                    var enumerator = resxCommentReader.GetEnumerator();
                    foreach (DictionaryEntry entry in resxReader)
                    {
                        string key = entry.Key.ToString() ?? string.Empty;
                        string comment;

                        // Since we have 2 readers of the same File, we can iterate over them synced by calling MoveNext only once per loop
                        if (enumerator.MoveNext())
                        {
                            ResXDataNode? current = (ResXDataNode?)((DictionaryEntry)enumerator.Current).Value;
                            comment = current?.Comment ?? string.Empty;
                        }
                        else 
                            comment = string.Empty;

                        if(entry.Value is Type value)
                            trans.Add(key, new MetaData<Type>(key, value, comment));
                    }
                }
                catch
                {
                }
            }
            return trans;
        }

        private static void WriteResource(string path, Dictionary<string, MetaData<object?>> trans)
        {
            using ResXResourceWriter resxWriter = new(path);
            foreach (var entry in trans)
            {
                ResXDataNode node = new(entry.Key, entry.Value.value)
                {
                    Comment = entry.Value.comment
                };
                resxWriter.AddResource(node);
            }
        }

        private void TranslateMissingValues(Dictionary<string, Dictionary<string, MetaData<object?>>> translations, string lang)
        {
            // Find missing translation keys
            List<MetaData<object?>> emptyTranslations = translations[lang].Values.Where(x => x.value is string str && str == string.Empty).ToList();

            // Nothing to translate? return
            if (emptyTranslations.Count == 0 || TranslationService != null)
                return;

            // Get missing translation values in english as a Reverse Dictionary
            // Filter so we don't get errors
            // GroupBy so that dublicate values doesn't break as it becomes a key
            // Another Filter to remove the once that doesn't have a text in english (can't translate empty string)
            Dictionary<string, MetaData<string>[]> kvp = emptyTranslations
                .Where(x => x.value is string).Select(x => new MetaData<string>(x.key, x.value?.ToString() ?? string.Empty, x.comment))
                .Where(x => translations["EN"].ContainsKey(x.key))
                .GroupBy(keySelector: x => translations["EN"][x.key].value as string ?? string.Empty, x => x)
                .Where(k => !k.Key.Equals(string.Empty))
                .ToDictionary(g => g.Key, g => g.ToArray());


            string[] textsToTranslate = [.. kvp.Keys];
            if (textsToTranslate.Length > 0 && TranslationService != null)
            {
                var response = TranslationService.TranslateTextAsync(textsToTranslate, lang).Result;
                foreach (var (source, translation) in response)
                {
                    var itemST = source;
                    var transes = kvp[itemST];
                    foreach (MetaData<string> transItem in transes)
                    {
                        string? text = translation[0];
                        if (text != null)
                        {
                            transItem.value = text;
                            transItem.comment = String.Join("\n", transItem.comment, "Ai Translated.");
                        }
                        else if(translations["EN"][itemST].value is string englishValue) // Shouldn't ever be false, but if it is, we avoid the error.
                        {
                            transItem.value = englishValue;
                            transItem.comment = String.Join("\n", transItem.comment, "Attempted Ai Translation Failed.");
                        }
                    }
                };
            }
        }

        private void TranslateResource(HashSet<string> existing, string resource, int pathLength)
        {
            // To store the Data in each language.
            Dictionary<string, Dictionary<string, MetaData<object?>>> translations = [];

            // Retrieve the English information
            translations.Add("EN", ReadResource<object?>(resource));

            // Translations work
            var langs = config.languagesToTranslate.ToArray();
            foreach (var lang in langs)
            {
                bool aiTrans = config.languagesToAiTranslate.Contains(lang);
                string langPath = Path.ChangeExtension(resource, $".{lang.ToLower()}.resx");
                // Setup the Translations
                if (!existing.Contains(langPath))
                    translations.Add(lang, []);
                else
                    translations.Add(lang, ReadResource<object?>(langPath));

                // Add all missing translations
                foreach (string entry in translations["EN"].Keys)
                {
                    if (!translations[lang].TryGetValue(entry, out MetaData<object?>? trans) || (trans.value is string strVal && string.IsNullOrEmpty(strVal)))
                    {
                        var value = aiTrans ? string.Empty : translations["EN"][entry].value;
                        var comment = translations["EN"][entry].comment;

                        // In Case of only the value is empty
                        translations[lang].Remove(entry);

                        // Add it to the Languages Dictionary
                        trans = new MetaData<object?>(entry, value, comment);
                        translations[lang].Add(entry, trans);
                    }
                }

                // AI Translation
                if (aiTrans)
                    TranslateMissingValues(translations, lang);
            }

            // Save Translations
            foreach (var item in translations)
            {
                if (!item.Key.Equals("EN"))
                    WriteResource(Path.ChangeExtension(resource, $".{item.Key.ToLower()}.resx"), item.Value);
            }

            // Try to write to the Database
            WriteToDatabase(pathLength, resource, translations);
        }

        private void WriteToDatabase(int pathLength, string resource, Dictionary<string, Dictionary<string, MetaData<object?>>> translations)
        {
            if (dth != null)
            {
                DataTable dataTable = CreateDataTable(pathLength, resource, translations, systemEnum);
                if(dataTable.Rows.Count > 0)
                    dth.AddInsert(dataTable);
            }
        }
    }
}