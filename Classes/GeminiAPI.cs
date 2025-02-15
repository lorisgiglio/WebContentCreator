using Serilog;
using System.Text.Json.Serialization;

namespace WebContentCreator.Classes
{
    public class GeminiAPI
    {
        private static readonly string API_KEY = "your_api_key";
        private static readonly string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" + API_KEY;

        public record GeminiRequest(string Prompt);
        public class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public Candidate[]? Candidates { get; set; }
        }

        public class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }
        }

        public class Content
        {
            [JsonPropertyName("parts")]
            public Part[]? Parts { get; set; }
        }

        public class Part
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }

        public static async Task<string?> GetGeminiTextResponse(HttpClient httpClient, GeminiRequest request)
        {
            var requestBody = new
            {
                contents = new[]
        {
            new
            {
                parts = new[]
                {
                    new { text = request.Prompt }
                }
            }
        }
            };

            Random random = new Random();
            int delayTime = random.Next(1000, 3001); // da 1 a 3 secondi
            await Task.Delay(delayTime);

            Log.Information("Richiesta a Gemini API");
            var response = await httpClient.PostAsJsonAsync(API_URL, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Errore nella richiesta a Gemini API: {0}", response.ReasonPhrase);
                return null!;
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            return result?.Candidates?[0]?.Content?.Parts?[0]?.Text;
        }

    }
}