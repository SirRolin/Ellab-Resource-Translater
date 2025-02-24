using Ellab_Resource_Translater.Objects;
using Ellab_Resource_Translater.Objects.Extensions;
using System.Collections;
using System.Linq;
using System.Resources;

namespace Ellab_Resource_Translater.Util
{
    public static class ResourceHandler
    {
        /// <summary>
        /// Reads the resource file (.resx) and returns a dictionary of the entries with the key as the dictionary key.
        /// </summary>
        /// <typeparam name="Type">Type of value, if you plan to write back to the resource file, this should be <see cref="object"/>?, otherwise it filters to only the correct types.</typeparam>
        /// <param name="path">path of the resource</param>
        /// <returns>Dictionary with key, <see cref="MetaData"/>, which is a (key, value, comment) object, that can implicitly be converted to a <see cref="ResXDataNode"/>.</returns>
        public static Dictionary<string, MetaData<Type>> ReadResource<Type>(string path)
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

                        if (entry.Value is Type value)
                            trans.Add(key, new MetaData<Type>(key, value, comment));
                    }
                }
                catch
                {
                }
            }
            return trans;
        }

        /// <summary>
        /// Writes the <paramref name="data"/> into <paramref name="path"/> resource file.
        /// </summary>
        /// <remarks>
        /// this might throw an IO error if path is incorrect or access is blocked.
        /// </remarks>
        /// <param name="path">File Path.</param>
        /// <param name="data"></param>
        public static void WriteResource(string path, Dictionary<string, MetaData<object?>> data)
        {
            using ResXResourceWriter resxWriter = new(path);
            foreach (var entry in data)
            {
                entry.Value.WriteToResourceWriter(resxWriter);
            }
        }
        /// <summary>
        /// Reads resource files of the english and the languages in <paramref name="langs"/>.
        /// Then fills the missing entried of <paramref name="langs"/> out with the english once.
        /// </summary>
        /// <remarks>
        /// If you want to also translate the missing entries use the other overloaded function, where you include a <see cref="TranslationService"/> as the last parameter.
        /// </remarks>
        /// <param name="existing">a HashSet of all the resource files considered "existing" in the root folder.</param>
        /// <param name="resource">Full Path to the english resource.</param>
        /// <param name="langs">Languagues other than english to also read and prepare.<br/>Upper Case national short form. ex: "EN", "DE", "ZH".</param>
        /// <param name="langsToAi">if the entry doesn't exist or is empty, and language doesn't exist in this array, it'll fill in the english entry for it.</param>
        /// <returns>Level 1 Key is the language, level 2 Key is the Entries Key.</returns>
        public static Dictionary<string, Dictionary<string, MetaData<object?>>> GetAllLangResources(HashSet<string> existing, string resource, string[] langs, IEnumerable<string> langsToAi)
        {
            // To store the Data of each language.
            Dictionary<string, Dictionary<string, MetaData<object?>>> translations = [];
            // Retrieve the English information
            translations.Add("EN", ResourceHandler.ReadResource<object?>(resource));
            foreach (var lang in langs)
            {
                bool aiTrans = langsToAi.Contains(lang);
                string langPath = Path.ChangeExtension(resource, $".{lang.ToLower()}.resx");
                // Setup the Translations
                if (!existing.Contains(langPath))
                    translations.Add(lang, []);
                else
                    translations.Add(lang, ResourceHandler.ReadResource<object?>(langPath));

                // Add all missing translations
                /*foreach (string entry in translations["EN"].Keys)
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
                }*/
            }

            return translations;
        }

        /// <summary>
        /// Reads resource files of the english and the languages in <paramref name="langs"/>.
        /// Then fills the missing entried of <paramref name="langs"/> out with the english once.
        /// </summary>
        /// <remarks>
        /// If you want to also translate the missing entries use the other overloaded function, where you include a <see cref="TranslationService"/> as the last parameter.
        /// </remarks>
        /// <param name="existing">a HashSet of all the resource files considered "existing" in the root folder.</param>
        /// <param name="resource">Full Path to the english resource.</param>
        /// <param name="langs">Languagues other than english to also read and prepare.<br/>Upper Case national short form. ex: "EN", "DE", "ZH".</param>
        /// <param name="langsToAi">if the entry doesn't exist or is empty, and language doesn't exist in this array, it'll fill in the english entry for it.
        /// <br/>If the entry just doesn't exist or is empty, it'll be translated with the TranslationService.</param>
        /// <param name="translationService">The service that uses ai to translate the entry values.</param>
        /// <returns>Level 1 Key is the language, level 2 Key is the Entries Key.</returns>
        public static Dictionary<string, Dictionary<string, MetaData<object?>>> GetAllLangResources(HashSet<string> existing, string resource, string[] langs, IEnumerable<string> langsToAi, TranslationService? translationService)
        {
            var output = GetAllLangResources(existing, resource, langs, langsToAi);
            // Only get the once that are in both arrays/enumerables.
            var translatelangs = langs.Intersect(langsToAi);
            foreach (var lang in translatelangs)
            {
                // AI Translation
                ResourceHandler.TranslateMissingValuesToLang(output, lang, translationService);
            }
            return output;
        }


        /// <summary>
        /// Translates missing entries of the language provided.
        /// Outputs it into the same Dictionary.
        /// </summary>
        /// <remarks>
        /// If you already are using GetAllLangResources, consider adding the <see cref="TranslationService"/> to that call instead of doing it manually.
        /// </remarks>
        /// <param name="translations">Level 1 Key is the language, level 2 Key is the Entries Key.</param>
        /// <param name="lang">Which Language should we translate?</param>
        /// <param name="TranslationService">The service that uses ai to translate the entry values.</param>
        public static void TranslateMissingValuesToLang(Dictionary<string, Dictionary<string, MetaData<object?>>> translations, string lang, TranslationService? TranslationService)
        {
            // Find missing translation keys
            List<MetaData<string>> missingTranslations = [];
            foreach (string entry in translations["EN"].Keys)
            {
                bool NotAlreadyTranslated = !translations[lang].TryGetValue(entry, out MetaData<object?>? trans) || (trans.value is string strVal && string.IsNullOrEmpty(strVal));
                if (translations["EN"][entry].value is string enValue && NotAlreadyTranslated)
                {
                    var value = enValue;
                    var comment = translations["EN"][entry].comment;

                    // Add it to the Languages Dictionary
                    missingTranslations.Add(new MetaData<string>(entry, value, comment));
                }
            }
            ////This is from before we wanted untranslated resources not to get saved as english in other language files.
            //List<MetaData<object?>> emptyTranslations = [.. translations[lang].Values.Where(x => x.value is string str && str == string.Empty)];

            // Nothing to translate? return
            if (missingTranslations.Count == 0 || TranslationService != null)
                return;

            // Get missing translation values in english as a Reverse Dictionary
            // Filter so we don't get errors
            // GroupBy so that dublicate values doesn't break as it becomes a key
            // Another Filter to remove the once that doesn't have a text in english (can't translate empty string)
            Dictionary<string, MetaData<string>[]> kvp = missingTranslations
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
                            transItem.comment = String.Join("\n", transItem.comment, "#AI");
                        }
                        else if (translations["EN"][itemST].value is string englishValue) // Shouldn't ever be false, but if it is, we avoid the error.
                        {
                            transItem.value = englishValue;
                            transItem.comment = String.Join("\n", transItem.comment, "Attempted Ai Translation Failed.");
                        }

                        translations[lang].Add(transItem.key, new MetaData<object?>(transItem.key, transItem.value, transItem.comment));
                    }
                }
            }
        }
    }
}
