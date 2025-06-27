using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ModelViews.AIModelViews;
using Microsoft.Extensions.Configuration;


namespace Services.Service
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey; // Replace with your actual API key

        public GeminiService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["GeminiAPIKey"];
        }

        public async Task<string> ChatWithHistoryAsync(List<GeminiContent> contents)
        {
            return await SendToGeminiAsync(contents);
        }

        /// <summary>
        /// Hàm gửi thực tế đến Gemini
        /// </summary>
        private async Task<string> SendToGeminiAsync(IEnumerable<GeminiContent> contents)
        {
            var request = new { contents };
            var requestJson = JsonSerializer.Serialize(request);

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}");

            httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API Error: {response.StatusCode} - {error}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<GeminiModels.GeminiResponse>(content);

            return result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text
                   ?? "No response from Gemini.";
        }
    }
}
