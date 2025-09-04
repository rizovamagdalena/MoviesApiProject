using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MoviesAPI.Service
{
    public class OpenAIService : IOpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public OpenAIService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> AskQuestionAsync(string question)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "user", content = question }
        },
                max_tokens = 200
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            }
            catch (Exception ex)
            {
                return $"Error sending request: {ex.Message}";
            }

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return "Unauthorized: API key is invalid or missing.";

                if (response.StatusCode == (System.Net.HttpStatusCode)429)
                    return "Rate limit exceeded: Too many requests. Try again later.";

                return $"OpenAI API returned error: {response.StatusCode}";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var answer = doc.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

            return answer;
        }

    }
}
