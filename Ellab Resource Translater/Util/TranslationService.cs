using Azure;
using Azure.AI.Translation.Text;
using Azure.Core;
using Ellab_Resource_Translater.Objects;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Ellab_Resource_Translater.Util
{

    public class TranslationService(AzureKeyCredential creds, Uri uri, string region)
    {
        private readonly TextTranslationClient _client = new(creds, uri, region);
        private readonly Uri _uri = uri;
        public int msWaitTime = 100;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<List<(string source, string[] translation)>> TranslateTextAsync(string[] texts, string targetLanguage)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await TranslateText(texts, targetLanguage);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<List<(string source, string[] translation)>> TranslateText(string[] texts, string targetLanguage)
        {
            // Due to limit of Azure, you can only translate a set amount at a time.
            // It would surprise me if 10 lines hits the limit.
            List<(string source, string[] translation)> outputList = [];
            for (int i = 0; i < texts.Length; i += 10)
            {
                var smallTexts = texts.Skip(i).Take(10).ToArray();
                var response = await _client.TranslateAsync(targetLanguage: targetLanguage, content: smallTexts, sourceLanguage: "en");
                outputList.AddRange(response.Value
                    .Select((translation, index) => (smallTexts[index], translation.Translations.Select((x) => x.Text).ToArray())) // Pair source with translation
                    .ToList());

                // Waiting between each call to hopefully avoid being being denied due to DDoS security
                Task.Delay(msWaitTime).Wait();
            }
            return outputList;
        }

        public async Task<bool> CanReachAzure()
        {
            try
            {
                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(_uri);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to reach Azure: {ex.Message}");
                return false;
            }
        }


        public static Dictionary<string, Dictionary<string, MetaData<object?>>> TranslateResource(this TranslationService? transService, HashSet<string> existingFiles, string resource, string[] langs, List<string> translationLangs)
        {
            // To store the Data in each language.
            Dictionary<string, Dictionary<string, MetaData<object?>>> translations = [];

            // Retrieve the English information
            translations.Add("EN", ReadResource<object?>(resource));

            foreach (var lang in langs)
            {
                bool aiTrans = translationLangs.Contains(lang);
                string langPath = Path.ChangeExtension(resource, $".{lang.ToLower()}.resx");
                // Setup the Translations
                if (!existingFiles.Contains(langPath))
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
                if (aiTrans && transService != null)
                    transService.TranslateMissingValues(translations, lang);
            }

            return translations;
        }


        public static void TranslateMissingValues(this TranslationService transservice, Dictionary<string, Dictionary<string, MetaData<object?>>> translations, string lang)
        {
            // Find missing translation keys
            List<MetaData<object?>> emptyTranslations = [.. translations[lang].Values.Where(x => x.value is string str && str == string.Empty)];

            // Nothing to translate? return
            if (emptyTranslations.Count == 0)
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
            if (textsToTranslate.Length > 0)
            {
                var response = transservice.TranslateTextAsync(textsToTranslate, lang).Result;
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
                        else if (translations["EN"][itemST].value is string englishValue) // Shouldn't ever be false, but if it is, we avoid the error.
                        {
                            transItem.value = englishValue;
                            transItem.comment = String.Join("\n", transItem.comment, "Attempted Ai Translation Failed.");
                        }
                    }
                }
                ;
            }
        }
    }

}
