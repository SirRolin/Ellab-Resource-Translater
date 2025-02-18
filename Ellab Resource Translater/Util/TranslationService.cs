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
    }

}
