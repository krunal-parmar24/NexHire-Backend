using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NexHire.Application.DTOs.Onboarding;
using NexHire.Application.Interfaces;

namespace NexHire.Infrastructure.Llm
{
    public class GitHubModelsClient : ILlmClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _githubToken;

        public GitHubModelsClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _githubToken = configuration["GITHUB_TOKEN"] ?? configuration["LLM_API_KEY"] ?? string.Empty;
        }

        public async Task<ParsedFieldsDto> ParseResumeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_githubToken))
            {
                return new ParsedFieldsDto();
            }

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a resume parsing assistant. Extract the following fields from the resume text into a JSON object matching this schema: FullName (string), Phone (string), CurrentTitle (string), TotalExperienceYears (number), Skills (array of strings), PreferredJobType (string), PreferredLocation (string), Certifications (array of strings), PortfolioLinks (array of strings), ExpectedSalaryRange (string). Ensure the output is strictly valid JSON."
                    },
                    new
                    {
                        role = "user",
                        content = text
                    }
                },
                response_format = new { type = "json_object" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://models.inference.ai.azure.com/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return new ParsedFieldsDto();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            try
            {
                var contentString = document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (!string.IsNullOrEmpty(contentString))
                {
                    return JsonSerializer.Deserialize<ParsedFieldsDto>(contentString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ParsedFieldsDto();
                }
            }
            catch
            {
                // Ignored parsing failures
            }

            return new ParsedFieldsDto();
        }
    }
}
