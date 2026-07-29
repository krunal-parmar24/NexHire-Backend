using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexHire.Application.DTOs.Onboarding;
using NexHire.Application.Interfaces;

namespace NexHire.Infrastructure.Llm
{
    /// <summary>
    /// <see cref="ILlmClient"/> implementation backed by the GitHub Models (Azure AI Inference)
    /// chat completions API. Used for resume field extraction and ATS semantic title matching.
    /// </summary>
    public class GitHubModelsClient : ILlmClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _githubToken;
        private readonly ILogger<GitHubModelsClient> _logger;

        public GitHubModelsClient(HttpClient httpClient, IConfiguration configuration, ILogger<GitHubModelsClient> logger)
        {
            _httpClient = httpClient;
            _githubToken = configuration["GITHUB_TOKEN"] ?? configuration["LLM_API_KEY"] ?? string.Empty;
            _logger = logger;
        }

        /// <summary>
        /// Extracts structured resume fields (name, title, skills, etc.) from raw resume text via the LLM.
        /// Returns an empty <see cref="ParsedFieldsDto"/> when the API key is missing or the call fails.
        /// </summary>
        public async Task<ParsedFieldsDto> ParseResumeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_githubToken))
            {
                return new ParsedFieldsDto();
            }

            var requestBody = new
            {
                model = LlmConstants.ModelName,
                messages = new[]
                {
                    new { role = "system", content = LlmConstants.ResumeParsingSystemPrompt },
                    new { role = "user", content = text }
                },
                response_format = new { type = "json_object" }
            };

            var contentString = await SendChatCompletionAsync(requestBody);
            if (string.IsNullOrEmpty(contentString))
            {
                return new ParsedFieldsDto();
            }

            try
            {
                return JsonSerializer.Deserialize<ParsedFieldsDto>(contentString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new ParsedFieldsDto();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize parsed resume fields from LLM response");
            }

            return new ParsedFieldsDto();
        }

        /// <summary>
        /// Scores the semantic alignment (0-100) between a candidate's current title and a job's title via the LLM.
        /// Returns 0 when the API key or either title is missing, or when the call fails.
        /// </summary>
        public async Task<int> GetSemanticTitleMatchAsync(string candidateTitle, string jobTitle)
        {
            if (string.IsNullOrWhiteSpace(_githubToken) || string.IsNullOrWhiteSpace(candidateTitle) || string.IsNullOrWhiteSpace(jobTitle))
            {
                return 0; // Default or fallback
            }

            var requestBody = new
            {
                model = LlmConstants.ModelName,
                messages = new[]
                {
                    new { role = "system", content = LlmConstants.SemanticTitleMatchSystemPrompt },
                    new { role = "user", content = $"Candidate Title: {candidateTitle}\nTarget Job Title: {jobTitle}" }
                }
            };

            var contentString = await SendChatCompletionAsync(requestBody);
            if (string.IsNullOrEmpty(contentString))
            {
                return 0;
            }

            try
            {
                if (int.TryParse(contentString.Trim(), out int score))
                {
                    return Math.Clamp(score, 0, 100);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse semantic title match score from LLM response");
            }

            return 0;
        }

        /// <summary>
        /// Sends a chat-completion request to the GitHub Models endpoint and returns the
        /// assistant message content, or <c>null</c> when the call fails or the response is malformed.
        /// </summary>
        private async Task<string?> SendChatCompletionAsync(object requestBody)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, LlmConstants.ChatCompletionsEndpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _githubToken);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            try
            {
                return document.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Malformed chat completion response from LLM provider");
                return null;
            }
        }
    }
}
