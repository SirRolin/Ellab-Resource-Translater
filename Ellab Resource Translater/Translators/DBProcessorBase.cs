using Azure.AI.Translation.Text;
using Ellab_Resource_Translater.objects;
using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Util;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Resources;
using System.Text.RegularExpressions;

namespace Ellab_Resource_Translater.Translators
{
    internal class DBProcessorBase(TranslationService? translationService, DbConnectionExtension? DBCon, int systemEnum, int maxThreads = 32)
    {
        private readonly int maxThreads = maxThreads;
        private readonly int systemEnum = systemEnum; // For the Database
        private readonly DbConnectionExtension? DBCon = DBCon;

        private readonly TranslationService? TranslationService = translationService;
        private readonly Config config = Config.Get();

        internal void Run(string path, ListView view, Label progresText, Regex regex)
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
                progresText.Invoke(() => progresText.Text = currentProcessed + " out of " + maxProcesses);
            }
            updateProgresText();

            // If we want to run it syncronous, we can set maxThreads to 0 or less. Otherwise it's asyncronous.
            if (maxThreads > 0)
            {
                Task[] tasks = new Task[maxThreads];
                for (int i = 0; i < maxThreads; i++)
                {
                    tasks[i] = Task.Run(() => ProcessQueue(pathLength: path.Length,
                                                           queue: englishQueuedFiles,
                                                           existingResources: allResources,
                                                           update: updateProgresText,
                                                           listView: view));
                }
                Task.WhenAll(tasks).Wait();
            } 
            else
            {
                ProcessQueue(pathLength: path.Length,
                                                           queue: englishQueuedFiles,
                                                           existingResources: allResources,
                                                           update: updateProgresText,
                                                           listView: view);
            }
        }

        private void ProcessQueue(int pathLength, ConcurrentQueue<string> queue, HashSet<string> existingResources, Action update, ListView listView)
        {
            ListViewItem listViewItem;
            while (true)
            {
                if (queue.TryDequeue(out var resource))
                {
                    string shortenedPath = resource[(pathLength + 1)..]; // Remove the root path
                    listViewItem = listView.Invoke(() => listView.Items.Add(shortenedPath));
                    TranslateResource(existingResources, resource);
                    listView.Invoke(() => listView.Items.Remove(listViewItem));
                    update.Invoke();
                }
                else
                    break;
            }
        }

        private static void AddParam(DataRow row, DbBatchCommand c, string name, DbType dbType)
        {
            var paramComment = c.CreateParameter();
            paramComment.ParameterName = "@" + name;
            paramComment.Value = row[name];
            paramComment.DbType = dbType;
            c.Parameters.Add(paramComment);
        }

        private static void AddParam(DataRow row, DbCommand c, string name, DbType dbType)
        {
            var paramComment = c.CreateParameter();
            paramComment.ParameterName = "@" + name;
            paramComment.Value = row[name];
            paramComment.DbType = dbType;
            c.Parameters.Add(paramComment);
        }

        private static DataTable CreateDataTable(string resource, Dictionary<string, Dictionary<string, Translation>> translations, int systemEnum)
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
                string resourceName = Path.GetFileName(Path.ChangeExtension(resource, $".{item.Key.ToLower()}.resx"));
                foreach (var value in item.Value)
                {
                    string comment = value.Value.comment;
                    string key = value.Key;
                    string text = value.Value.value; // Stonks

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

        private static Dictionary<string, Translation> ReadResource(string path)
        {
            Dictionary<string, Translation> trans = [];
            using (ResXResourceReader resxReader = new(path))
            {
                using ResXResourceReader resxCommentReader = new(path);
                // Switches to reading metaData instead of values, can't have both, which we need for comments
                resxCommentReader.UseResXDataNodes = true;
                var enumerator = resxCommentReader.GetEnumerator();

                foreach (DictionaryEntry entry in resxReader)
                {
                    string key = entry.Key.ToString() ?? string.Empty;
                    string value = entry.Value?.ToString() ?? string.Empty;
                    string comment;

                    // Since we have 2 readers of the same File, we can iterate over them synced by calling MoveNext only once per loop
                    if (enumerator.MoveNext())
                    {
                        ResXDataNode? current = (ResXDataNode?)((DictionaryEntry)enumerator.Current).Value;
                        comment = current?.Comment ?? string.Empty;
                    }
                    else 
                        comment = string.Empty; 

                    trans.Add(key, new Translation(key, value, comment));
                }
            }
            return trans;
        }

        private static void WriteResource(string path, Dictionary<string, Translation> trans)
        {
            using ResXResourceWriter resxWriter = new(path);
            foreach (var entry in trans)
            {
                resxWriter.AddResource(entry.Key, entry.Value.value);
            }
        }

        private void TranslateMissingValues(Dictionary<string, Dictionary<string, Translation>> translations, string lang)
        {
            // Find missing translation keys
            List<Translation> emptyTranslations = translations[lang].Values.Where(x => x.value == string.Empty).ToList();

            // Nothing to translate? return
            if (emptyTranslations.Count == 0)
                return;

            // Get missing translation values in english as a Reverse Dictionary
            // Filter so we don't get errors
            // GroupBy so that dublicate values doesn't break as it becomes a key
            Dictionary<string, Translation[]> kvp = emptyTranslations.Where(x => translations["EN"].ContainsKey(x.key))
                .GroupBy(x => translations["EN"][x.key].value, x => x)
                .ToDictionary(g => g.Key, g => g.ToArray());


            // To Do Fix the random empty strings that's introduced by some strings


            string[] textsToTranslate = [.. kvp.Keys];
            if (textsToTranslate.Length > 0 && TranslationService != null)
            {
                var response = TranslationService.TranslateTextAsync(textsToTranslate, lang).Result;
                foreach (var item in response)
                {
                    var itemST = item.source;
                    foreach (var s in kvp.Where(x => itemST.Equals(x.Key)).Select(x => x.Value).First())
                    {
                        Translation transItem = translations[lang][s.key];
                        string? text = item.translation[0] ?? null;
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

        private async void TranslateResource(HashSet<string> existing, string resource)
        {
            // To store the Data in each language.
            Dictionary<string, Dictionary<string, Translation>> translations = [];

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
                    if (!translations[lang].TryGetValue(entry, out Translation? trans) || string.IsNullOrEmpty(trans.value))
                    {
                        var value = aiTrans ? string.Empty : translations["EN"][entry].value;
                        var comment = translations["EN"][entry].comment;

                        // In Case of only the value is empty
                        translations[lang].Remove(entry);

                        // Add it to the Languages Dictionary
                        trans = new Translation(entry, value, comment);
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
            await WriteToDatabase(resource, translations);
        }

        private async Task WriteToDatabase(string resource, Dictionary<string, Dictionary<string, Translation>> translations)
        {
            if (DBCon != null)
            {
                DataTable dataTable = CreateDataTable(resource, translations, systemEnum);

                bool batchFailed = false;
                // Upload to the Database
                if (DBCon.connection.CanCreateBatch)
                {
                    var s = DBCon.connection.CreateBatch();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        var c = s.CreateBatchCommand();
                        // Key have to have quotes as "Key" is a keyword used in SQL
                        c.CommandText = @"
                            INSERT INTO Translation 
                                (Comment, ""Key"", LanguageCode, ResourceName, Text, IsTranlatedInValSuite, SystemEnum) 
                            VALUES 
                                (@Comment, @Key, @LanguageCode, @ResourceName, @Text, @IsTranlatedInValSuite, @SystemEnum)";

                        AddParam(row, c, "Comment", DbType.String);
                        AddParam(row, c, "Key", DbType.String);
                        AddParam(row, c, "LanguageCode", DbType.String);
                        AddParam(row, c, "ResourceName", DbType.String);
                        AddParam(row, c, "Text", DbType.String);
                        AddParam(row, c, "IsTranlatedInValSuite", DbType.Boolean);
                        AddParam(row, c, "SystemEnum", DbType.Int32);
                        s.BatchCommands.Add(c);
                    }
                    try
                    {
                        await DBCon.ThreadSafeAsyncFunction((_) => {
                            s.ExecuteNonQuery();
                        });
                    }
                    catch (Exception)
                    {
                        batchFailed = true;
                    }
                }
                else
                {
                    batchFailed = true;
                }
                if (batchFailed)
                {
                    using DbCommand command = DBCon.connection.CreateCommand();
                    // Key have to have quotes as "Key" is a keyword used in SQL
                    command.CommandText = @"
                            INSERT INTO Translation 
                                (Comment, ""Key"", LanguageCode, ResourceName, Text, IsTranlatedInValSuite, SystemEnum) 
                            VALUES 
                                (@Comment, @Key, @LanguageCode, @ResourceName, @Text, @IsTranlatedInValSuite, @SystemEnum)";

                    foreach (DataRow row in dataTable.Rows)
                    {
                        // Preparing the Values
                        AddParam(row, command, "Comment", DbType.String);
                        AddParam(row, command, "Key", DbType.String);
                        AddParam(row, command, "LanguageCode", DbType.String);
                        AddParam(row, command, "ResourceName", DbType.String);
                        AddParam(row, command, "Text", DbType.String);
                        AddParam(row, command, "IsTranlatedInValSuite", DbType.Boolean);
                        AddParam(row, command, "SystemEnum", DbType.Int32);

                        // Executing Time
                        int tries = 0;
                        while (tries < 2)
                        {
                            try
                            {
                                await DBCon.ThreadSafeAsyncFunction((s) =>
                                {
                                    // Sometimes the connection closes
                                    if (DBCon.connection.State != ConnectionState.Open)
                                        DBCon.connection.Open();

                                    command.ExecuteNonQuery();
                                });
                                break;
                            }
                            catch
                            {
                                tries++;
                                Task.Delay(500).Wait(); // Wait half a second.
                            }
                            if (tries == 2)
                            {
                                MessageBox.Show("failed to upload to the database:" + resource);
                            }
                        }
                    }
                }
            }
        }
    }
}