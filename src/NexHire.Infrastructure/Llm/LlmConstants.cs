namespace NexHire.Infrastructure.Llm
{
    /// <summary>
    /// Centralized configuration literals for the GitHub Models (Azure AI Inference) LLM client.
    /// Keeping these in one place avoids duplicated magic strings across the client's methods.
    /// </summary>
    internal static class LlmConstants
    {
        /// <summary>Chat completions endpoint for the GitHub Models inference API.</summary>
        public const string ChatCompletionsEndpoint = "https://models.inference.ai.azure.com/chat/completions";

        /// <summary>Model identifier used for resume parsing and semantic title matching.</summary>
        public const string ModelName = "gpt-4o-mini";

        /// <summary>System prompt instructing the model to extract structured resume fields as JSON.</summary>
        public const string ResumeParsingSystemPrompt =
            "You are a resume parsing assistant. Extract the following fields from the resume text into a JSON object matching this schema: FullName (string), Phone (string), CurrentTitle (string), TotalExperienceYears (number), Skills (array of strings), PreferredJobType (string), PreferredLocation (string), Certifications (array of strings), PortfolioLinks (array of strings), ExpectedSalaryRange (string). Ensure the output is strictly valid JSON.";

        /// <summary>System prompt instructing the model to score semantic title alignment.</summary>
        public const string SemanticTitleMatchSystemPrompt =
            "You are an ATS semantic matching engine. Compare the candidate's job title with the target job title. Return ONLY a single integer from 0 to 100 representing their semantic alignment and similarity, where 100 is an exact match and 0 is completely unrelated. Do not return any other text.";
    }
}
