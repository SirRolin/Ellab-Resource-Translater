using Ellab_Resource_Translater.objects;
using Ellab_Resource_Translater.Util;
using Microsoft.Data.SqlClient;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Resources;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;

namespace Ellab_Resource_Translater.Translators
{
    internal class EMSuite(TranslationService translationService, DbConnection DBCon)
    {
        private const int MAX_THREADS = 32;
        private const int SYSTEM_ENUM = 1; // For the Database
        private readonly Config config = Config.Get();

        private readonly TranslationService TranslationService = translationService;
        private readonly DbConnection DBCon = DBCon;
        internal void Run(string path, ListView view, Label progresText)
        {
            // Tracks resources done - Starts at -1 so that we can call it to get the right format to start with.
            int currentProcessed = -1;

            //// Regex 
            /// .*\\\\Resources\\\\ means it's in a Resources folder
            /// .* means anything in there
            /// (?<!\\...) means there shouldn't be a . followed by 2 other characters
            /// \\.resx means it has to end with .resx
            /// 
            Regex regex = new(".*\\\\Resources\\\\.*(?<!\\...)\\.resx");
            var allResources = Directory.GetFiles(path, "*.resx", SearchOption.AllDirectories).ToHashSet();
            var englishQueuedFiles = new ConcurrentQueue<string>(allResources.Where(x => regex.IsMatch(x)).Take(1)); // TODO:

            int maxProcesses = englishQueuedFiles.Count;

            // Doing this so we don't have to pass both a int ref and a Label ref
            void updateProgresText()
            {
                Interlocked.Increment(ref currentProcessed);
                progresText.Invoke(() => progresText.Text = currentProcessed + " out of " + maxProcesses);
            }
            updateProgresText();

            Task[] tasks = new Task[MAX_THREADS];
            for (int i = 0; i < MAX_THREADS; i++)
            {
                tasks[i] = Task.Run(() => ProcessQueue(pathLength: path.Length,
                                                       queue: englishQueuedFiles,
                                                       existingResources: allResources,
                                                       update: updateProgresText,
                                                       listView: view));
            }

            Task.WhenAll(tasks).Wait();
        }

        private void ProcessQueue(int pathLength, ConcurrentQueue<string> queue, HashSet<string> existingResources, Action update, ListView listView)
        {
            ListViewItem listViewItem;
            while (true)
            {
                if (queue.TryDequeue(out var resource))
                {
                    string shortenedPath = resource.Substring(pathLength + 1);
                    listViewItem = listView.Invoke(() => listView.Items.Add(shortenedPath));
                    TranslateResource(existingResources, resource);
                    listView.Invoke(() => listView.Items.Remove(listViewItem));
                    update.Invoke();
                } 
                else
                    break;
            }
        }

        private async void TranslateResource(HashSet<string> existing, string resource)
        {
            // To store the Data in each language.
            Dictionary<string, Dictionary<string, Translation>> translations = [];

            // Retrieve the English information
            translations.Add("en", ReadResource(resource));

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
                foreach (string entry in translations["en"].Keys)
                {
                    if (!translations[lang].TryGetValue(entry, out Translation? trans) || string.IsNullOrEmpty(trans.value))
                    {
                        var value = aiTrans ? string.Empty : translations["en"][entry].value;
                        var comment = translations["en"][entry].comment;

                        // In Case of only the value is empty
                        translations[lang].Remove(entry);
                        trans = new Translation(entry, value, comment);
                        translations[lang].Add(entry, trans);
                    }
                }

                // AI Translation
                if (aiTrans)
                    TranslateMissingValues(translations, lang);
            }

            DataTable dataTable = new();
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

                    dataTable.Rows.Add(comment,
                                        key,
                                        language,
                                        resourceName,
                                        text,                           
                                        false,                                                           
                                        SYSTEM_ENUM                                                      
                                        );
                }
            }
            // Upload to the Database
            if (DBCon.CanCreateBatch && DBCon is SqlConnection sqlCon)
            {
                using SqlBulkCopy dbBatch = new(sqlCon);
                dbBatch.DestinationTableName = "Translation";
                await dbBatch.WriteToServerAsync(dataTable);
            }
            else
            {

                foreach (DataRow row in dataTable.Rows)
                {
                    using DbCommand command = DBCon.CreateCommand();
                    command.CommandText = @"
                        INSERT INTO Translation 
                            (Comment, Key, LanguageCode, ResourceName, Text, IsTranlatedInValSuite, SystemEnum) 
                        VALUES 
                            (@Comment, @Key, @LanguageCode, @ResourceName, @Text, @IsTranlatedInValSuite, @SystemEnum)";

                    // Comment
                    var paramComment = command.CreateParameter();
                    paramComment.ParameterName = "@Comment";
                    paramComment.Value = row["Comment"] ?? DBNull.Value;
                    command.Parameters.Add(paramComment);

                    // Key
                    var paramKey = command.CreateParameter();
                    paramKey.ParameterName = "@Key";
                    paramKey.Value = row["Key"];
                    command.Parameters.Add(paramKey);

                    // languageCode
                    var paramLanguageCode = command.CreateParameter();
                    paramLanguageCode.ParameterName = "@LanguageCode";
                    paramLanguageCode.Value = row["LanguageCode"];
                    command.Parameters.Add(paramLanguageCode);

                    // ResourceName
                    var paramResourceName = command.CreateParameter();
                    paramResourceName.ParameterName = "@ResourceName";
                    paramResourceName.Value = row["ResourceName"];
                    command.Parameters.Add(paramResourceName);

                    // Text
                    var paramText = command.CreateParameter();
                    paramText.ParameterName = "@Text";
                    paramText.Value = row["Text"];
                    command.Parameters.Add(paramText);

                    // IsTranlatedInValSuite
                    var paramIsTranlatedInValSuite = command.CreateParameter();
                    paramIsTranlatedInValSuite.ParameterName = "@IsTranlatedInValSuite";
                    paramIsTranlatedInValSuite.Value = row["IsTranlatedInValSuite"];
                    command.Parameters.Add(paramIsTranlatedInValSuite);

                    // SystemEnum
                    var paramSystemEnum = command.CreateParameter();
                    paramSystemEnum.ParameterName = "@SystemEnum";
                    paramSystemEnum.Value = row["SystemEnum"];
                    command.Parameters.Add(paramSystemEnum);

                    // Executing Time
                    await command.ExecuteNonQueryAsync();
                }
            }

            // Save Translations
            foreach (var item in translations)
            {
                if(!item.Key.Equals("en"))
                    WriteResource(Path.ChangeExtension(resource, $".{item.Key.ToLower()}.resx"), item.Value);
            }
        }

        private void TranslateMissingValues(Dictionary<string, Dictionary<string, Translation>> translations, string lang)
        {
            // Find missing translation keys
            List<string> emptyTranslations = translations[lang].Values.Where(x => x.value == string.Empty).Select(x => x.key).ToList();

            // Get missing translation values in english as a Reverse Dictionary
            Dictionary<string, string> kvp = translations["en"].Where(x => emptyTranslations.Contains(x.Key))
                .Select(x => new KeyValuePair<string, string>(x.Value.value ?? "", x.Key))
                .ToDictionary();
            string[] textsToTranslate = [.. kvp.Keys];

            var response = TranslationService.TranslateTextAsync(textsToTranslate, lang).Result;
            foreach (var item in response.Value)
            {
                var itemST = item.SourceText.Text;
                Translation transItem = translations[lang][kvp[itemST]];
                transItem.value = item.Translations[0]?.Text ?? translations["en"][itemST].value;
                transItem.comment = String.Join("\n", transItem.comment, "Ai Translated.");
            };
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
            using (ResXResourceWriter resxWriter = new(path))
            {
                foreach (var entry in trans)
                {
                    var dataInfo = new ResXDataNode(entry.Key, entry.Value.value);
                    resxWriter.AddResource(entry.Key, entry.Value.value);
                }
            }
        }
    }
}
