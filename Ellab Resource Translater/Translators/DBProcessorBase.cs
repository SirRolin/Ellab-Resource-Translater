using Ellab_Resource_Translater.Interfaces;
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
            // FetchData from Database that has entries in ChangedTranslations.
            UpdateLocalFiles(path, view, progresText);

            // setup a transaction handler so that we can abort
            dth = new(
                    source: source,
                    // Delete All associated entities - In transaction, so if cancelled it'll not go through
                    onTransactionStart: (dbc, trans) =>
                    {
                        progresText.Invoke(() => progresText.Text = "Clearing Database.");
                        var c = dbc.CreateCommand();
                        c.Transaction = trans;
                        c.CommandType = CommandType.Text;
                        c.CommandText = $@"DELETE a FROM Translation a where a.SystemEnum = { systemEnum } AND NOT a.ID in (Select ct.TranslationID from ChangedTranslation ct);";
                        c.ExecuteNonQuery();
                        c.Dispose();
                    },
                    // Key have to have quotes as "Key" is a keyword used in SQL
                    commandText: @"
                              INSERT INTO Translation 
                                  (Comment, ""Key"", LanguageCode, ResourceName, Text, IsTranlatedInValSuite, SystemEnum) 
                              VALUES 
                                  (@Comment, @Key, @LanguageCode, @ResourceName, @Text, @IsTranlatedInValSuite, @SystemEnum);",

                    addParameters: (row, paramable) =>
                    {
                        paramable.AddParam(row, "Comment", DbType.String);
                        paramable.AddParam(row, "Key", DbType.String);
                        paramable.AddParam(row, "LanguageCode", DbType.String);
                        paramable.AddParam(row, "ResourceName", DbType.String);
                        paramable.AddParam(row, "Text", DbType.String);
                        paramable.AddParam(row, "IsTranlatedInValSuite", DbType.Boolean);
                        paramable.AddParam(row, "SystemEnum", DbType.Int32);
                    },
                    inserters: Config.Get().insertersToUse);

            // Read, Translate, Write & prepare dth.
            SetupTasks(path, view, progresText, regex);

            // Starts transfer to Database
            if (connProv != null && !token.IsCancellationRequested)
            {
                dth.StartCommands(connProv, progresText, view, (dt) =>
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
                command.CommandText = $@"SELECT a.""Key"", a.ResourceName, a.LanguageCode, a.Comment, b.ChangedText, b.ID as ""ChangedID"" FROM Translation a JOIN ChangedTranslation b ON a.ID = b.TranslationID WHERE a.SystemEnum = {systemEnum};";
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


                    // Merging the Tables with column data so we can fetch it later.
                    // Reasoning for this is so we can multithread
                    int currentProgress = 0;
                    int maxProgresses = dataTables.Count;

                    void myUpdate(string pretext)
                    {
                        Interlocked.Increment(ref currentProgress);
                        FormUtils.LabelTextUpdater(progresText, pretext, currentProgress, " out of ", maxProgresses);
                    }

                    ConcurrentQueue<(int dataTNum, DataRow row)> dataRows = [];
                    ConcurrentDictionary<int, (DataColumn resource, DataColumn key, DataColumn value, DataColumn comment)> dataColumns = [];
                    void processTable(DataTable dt)
                    {
                        if (dt.Columns["ResourceName"] is DataColumn resourceColumn
                            && dt.Columns["Key"] is DataColumn keyColumn
                            && dt.Columns["ChangedText"] is DataColumn textColumn
                            && dt.Columns["Comment"] is DataColumn commentValue) {
                            dataColumns.TryAdd(currentProgress, (resourceColumn, keyColumn, textColumn, commentValue));
                            foreach (DataRow row in dt.Rows)
                            {
                                dataRows.Enqueue((currentProgress, row));
                            }
                        }
                    }
                    ExecutionHandler.Execute(maxProgresses, maxThreads, (i) =>
                    {
                        while (dataTables.TryDequeue(out DataTable? table))
                        {
                            FormUtils.HandleProcess(
                                update: () => myUpdate("Merging tables: "),
                                listView: view,
                                resourceName: i + ") Fetching Data...",
                                process: () => processTable(table));
                        }
                    });

                    // Splits it into a Dictionary so that we can open update local files in groups instead of one change at a time.
                    currentProgress = 0;
                    maxProgresses = dataRows.Count;
                    ConcurrentDictionary<string, List<MetaData<string>>> changesToRegister = [];
                    void processRow(DataRow row, int dataTNumber)
                    {
                        if (row[dataColumns[dataTNumber].resource]   is string resourceValue
                            && row[dataColumns[dataTNumber].key]     is string keyValue
                            && row[dataColumns[dataTNumber].value]   is string valueValue
                            && row[dataColumns[dataTNumber].comment] is string commentValue)
                        {
                            changesToRegister.AddOrUpdate(resourceValue, [new MetaData<string>(keyValue, valueValue, commentValue)], 
                                (key, orgList) => {
                                    orgList.Add(new MetaData<string>(keyValue, valueValue, commentValue));
                                    return orgList;
                                });
                        }
                    }
                    ExecutionHandler.Execute(maxThreads, maxProgresses, (i) =>
                    {
                        while (dataRows.TryDequeue(out var rowData))
                        {
                            FormUtils.HandleProcess(
                                update: () => myUpdate("Grouping Data on same Resource Files: "),
                                listView: view,
                                resourceName: i + ") Fetching Data...",
                                process: () => processRow(rowData.row, rowData.dataTNum));
                            Interlocked.Increment(ref currentProgress);
                        }
                    });

                    // ConcurrentDictionary can't be popped, so we have to have a queue of it's keys
                    ConcurrentQueue<string> files = new(changesToRegister.Keys);

                    // Process Changes by the ResourceFile
                    currentProgress = 0;
                    maxProgresses = files.Count;
                    void processChanges(string rootPath, string transDictKey)
                    {
                        // Load Local data so we don't lose data that wasn't overriden
                        var translations = ReadResource(string.Concat(rootPath, rootPath.EndsWith('/') ? "": '/', transDictKey));

                        // Override in Memory
                        var changes = changesToRegister[transDictKey];
                        changes.ForEach(trans => {
                            if(translations.TryGetValue(trans.key, out MetaData<string>? oldtrans))
                            {
                                oldtrans.value = trans.value;
                            } else
                            {
                                translations.Add(trans.key, trans);
                            }
                        });

                        // Save to Local Data
                        WriteResource(rootPath, translations);
                    }

                    ExecutionHandler.Execute(maxThreads, maxProgresses, (i) =>
                    {
                        while (files.TryDequeue(out var resourceName))
                        {
                            FormUtils.HandleProcess(
                                pathLength: -1, // -1 to disable 
                                update: () => FormUtils.LabelTextUpdater(progresText, "Saving Changes: ", currentProgress, " out of ", maxProgresses),
                                listView: view,
                                resourceName: i + ") Fetching Data...",
                                process: () => processChanges(path, resourceName));
                            Interlocked.Increment(ref currentProgress);
                        }
                    });
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

            // Execution Time
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
            while (!source.IsCancellationRequested)
            {
                if (queue.TryDequeue(out var resource))
                {
                    void process()
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
                    FormUtils.HandleProcess(rootPathLength, update, listView, resource, process);
                }
                else
                    break;
            }
        }

        private static DataTable CreateDataTable(int pathLength, string resource, Dictionary<string, Dictionary<string, MetaData<string>>> translations, int systemEnum)
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

        private static Dictionary<string, MetaData<string>> ReadResource(string path)
        {
            Dictionary<string, MetaData<string>> trans = [];
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
                        if (entry.Value is not string)
                            continue;

                        string value = entry.Value?.ToString() ?? string.Empty;

                        trans.Add(key, new MetaData<string>(key, value, comment));
                    }
                }
                catch
                {
                }
            }
            return trans;
        }

        private static void WriteResource(string path, Dictionary<string, MetaData<string>> trans)
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

        private void TranslateMissingValues(Dictionary<string, Dictionary<string, MetaData<string>>> translations, string lang)
        {
            // Find missing translation keys
            List<MetaData<string>> emptyTranslations = translations[lang].Values.Where(x => x.value is string str && str == string.Empty).ToList();

            // Nothing to translate? return
            if (emptyTranslations.Count == 0 || TranslationService != null)
                return;

            // Get missing translation values in english as a Reverse Dictionary
            // Filter so we don't get errors
            // GroupBy so that dublicate values doesn't break as it becomes a key
            // Another Filter to remove the once that doesn't have a text in english (can't translate empty string)
            Dictionary<string, MetaData<string>[]> kvp = emptyTranslations.Where(x => translations["EN"].ContainsKey(x.key))
                .GroupBy(x => translations["EN"][x.key].value, x => x)
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
                        string? text = translation[0] ?? null;
                        if (text != null)
                        {
                            transItem.value = text;
                            transItem.comment = String.Join("\n", transItem.comment, "Ai Translated.");
                        }
                        else
                        {
                            transItem.value = translations["EN"][itemST].value;
                            transItem.comment = String.Join("\n", transItem.comment, "Attempted Ai Translation Failed.");
                        }
                    }
                };
            }
        }

        private void TranslateResource(HashSet<string> existing, string resource, int pathLength)
        {
            // To store the Data in each language.
            Dictionary<string, Dictionary<string, MetaData<string>>> translations = [];

            // Retrieve the English information
            translations.Add("EN", ReadResource(resource));

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
                    translations.Add(lang, ReadResource(langPath));

                // Add all missing translations
                foreach (string entry in translations["EN"].Keys)
                {
                    if (!translations[lang].TryGetValue(entry, out MetaData<string>? trans) || string.IsNullOrEmpty(trans.value))
                    {
                        var value = aiTrans ? string.Empty : translations["EN"][entry].value;
                        var comment = translations["EN"][entry].comment;

                        // In Case of only the value is empty
                        translations[lang].Remove(entry);

                        // Add it to the Languages Dictionary
                        trans = new MetaData<string>(entry, value, comment);
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

        private void WriteToDatabase(int pathLength, string resource, Dictionary<string, Dictionary<string, MetaData<string>>> translations)
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