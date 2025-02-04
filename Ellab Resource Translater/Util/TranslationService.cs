using Azure;
using Azure.AI.Translation.Text;
using Azure.Core;

namespace Ellab_Resource_Translater.Util
{

    public class TranslationService(AzureKeyCredential creds, Uri uri, string region)
    {
        private readonly TextTranslationClient _client = new(creds, uri, region);
        private readonly Uri _uri = uri;

        public async Task<List<(string source, string[] translation)>> TranslateTextAsync(string[] texts, string targetLanguage)
        {
            var response =  await _client.TranslateAsync(targetLanguage: targetLanguage, content: texts, sourceLanguage: "en");
            return response.Value
                .Select((translation, index) => (texts[index], translation.Translations.Select((x) => x.Text).ToArray())) // Pair source with translation
                .ToList();
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
    }

}
