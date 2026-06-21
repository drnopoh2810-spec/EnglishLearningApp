using System.Net.Http.Json;
using System.Text.Json;

namespace EnglishLearningApp.Services
{
    public class TranslationService
    {
        private readonly HttpClient _httpClient;

        public TranslationService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "EnglishLearningApp/1.0");
        }

        /// <summary>
        /// Translates English text to Arabic using MyMemory API (free, no key required)
        /// </summary>
        public async Task<string> TranslateToArabicAsync(string englishText)
        {
            if (string.IsNullOrWhiteSpace(englishText))
                return string.Empty;

            try
            {
                var encodedText = Uri.EscapeDataString(englishText);
                var url = $"https://api.mymemory.translated.net/get?q={encodedText}&langpair=en|ar";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<TranslationResponse>();

                if (result?.ResponseStatus == 200 && !string.IsNullOrEmpty(result.ResponseData?.TranslatedText))
                {
                    return result.ResponseData.TranslatedText;
                }

                return string.Empty;
            }
            catch (HttpRequestException)
            {
                // Fallback: return empty string - UI will show message
                return string.Empty;
            }
            catch (TaskCanceledException)
            {
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Batch translate multiple sentences (with rate limiting)
        /// </summary>
        public async Task<Dictionary<string, string>> BatchTranslateAsync(IEnumerable<string> sentences)
        {
            var results = new Dictionary<string, string>();
            var delay = TimeSpan.FromMilliseconds(500); // Rate limiting

            foreach (var sentence in sentences)
            {
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    var translation = await TranslateToArabicAsync(sentence);
                    results[sentence] = translation;
                    await Task.Delay(delay);
                }
            }

            return results;
        }

        // Internal models for MyMemory API
        private class TranslationResponse
        {
            public int ResponseStatus { get; set; }
            public TranslationData? ResponseData { get; set; }
        }

        private class TranslationData
        {
            public string TranslatedText { get; set; } = string.Empty;
        }
    }
}
