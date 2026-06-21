using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EnglishLearningApp.Services
{
    public class YouGlishService
    {
        /// <summary>
        /// Generates a YouGlish URL for pronunciation lookup
        /// </summary>
        public string GenerateUrl(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return string.Empty;

            // Clean sentence for URL
            var cleanSentence = sentence.Trim();
            // Remove extra spaces
            cleanSentence = Regex.Replace(cleanSentence, @"\s+", " ");
            // URL encode
            var encoded = Uri.EscapeDataString(cleanSentence);

            return $"https://youglish.com/pronounce/{encoded}/english";
        }

        /// <summary>
        /// Opens YouGlish in the default browser
        /// </summary>
        public void OpenInBrowser(string sentence)
        {
            var url = GenerateUrl(sentence);
            if (!string.IsNullOrEmpty(url))
            {
                OpenUrl(url);
            }
        }

        /// <summary>
        /// Opens a specific URL in the default browser
        /// </summary>
        public void OpenUrl(string url)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception)
            {
                // Fallback for older Windows versions
                try
                {
                    Process.Start("explorer.exe", url);
                }
                catch { }
            }
        }

        /// <summary>
        /// Pre-generates YouGlish URLs for all sentences without one
        /// </summary>
        public async Task UpdateMissingUrlsAsync(IEnumerable<Models.Sentence> sentences)
        {
            foreach (var sentence in sentences)
            {
                if (string.IsNullOrEmpty(sentence.YouGlishUrl) && !string.IsNullOrWhiteSpace(sentence.EnglishSentence))
                {
                    sentence.YouGlishUrl = GenerateUrl(sentence.EnglishSentence);
                }
            }
            await Task.CompletedTask;
        }
    }
}
