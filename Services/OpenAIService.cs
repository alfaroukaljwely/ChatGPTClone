using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChatGPTClone.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ChatGPTClone.Services
{
    public class OpenAIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        // 🔥 هنا بالظبط مكان الـ Constructor
        public OpenAIService(IConfiguration config, HttpClient httpClient)
        {
            _apiKey = config["OpenAI:ApiKey"];  // قراءة الـ API Key
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        // send method: simple retry on 429
        public async Task<string> SendMessageAsync(List<Message> messages)
        {
            var body = JsonSerializer.Serialize(new { model = "gpt-4o-mini", messages = messages.Select(m => new { role = m.Role, content = m.Content }) });
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            int tries = 0;
            while (true)
            {
                var response = await _httpClient.PostAsync("chat/completions", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
                }

                if (response.StatusCode == (HttpStatusCode)429 && ++tries <= 3)
                {
                    if (response.Headers.TryGetValues("Retry-After", out var vals) && int.TryParse(vals.FirstOrDefault(), out var seconds))
                        await Task.Delay(TimeSpan.FromSeconds(seconds));
                    else
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, tries)));
                    continue;
                }

                // Log headers & body for diagnosis
                var headers = string.Join("; ", response.Headers.Select(h => h.Key + "=" + string.Join(",", h.Value)));
                throw new InvalidOperationException($"OpenAI error {(int)response.StatusCode} {response.ReasonPhrase}. Headers: {headers}. Body: {responseBody}");
            }
        }
    }
}
