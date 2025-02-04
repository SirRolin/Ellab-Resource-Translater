using Azure;
using Azure.AI.Translation.Text;
using Azure.Core;

namespace Ellab_Resource_Translater.Util
{

    public class TranslationService(AzureKeyCredential creds, Uri uri, string region)
    {
        private readonly TextTranslationClient _client = new(creds, uri, region);
        private readonly Uri _uri = uri;

        public async Task<Azure.Response<IReadOnlyList<TranslatedTextItem>>> TranslateTextAsync(string[] texts, string targetLanguage)
        {
            return await _client.TranslateAsync(targetLanguage: targetLanguage, content: texts, sourceLanguage: "en");
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
