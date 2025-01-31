using Azure.AI.Translation.Text;
using Azure.Core;

namespace Ellab_Resource_Translater.Util
{

    public class TranslationService(TextTranslationClient credential)
    {
        private readonly TextTranslationClient _client = credential;

        public async Task<Azure.Response<IReadOnlyList<TranslatedTextItem>>> TranslateTextAsync(string[] texts, string targetLanguage)
        {
            return await _client.TranslateAsync(targetLanguage: targetLanguage, content: texts, sourceLanguage: "en");
        }
    }

}
