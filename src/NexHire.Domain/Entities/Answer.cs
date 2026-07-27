using System.Text.Json.Serialization;

namespace NexHire.Domain.Entities
{
    public class Answer
    {
        [JsonPropertyName("questionId")]
        public string QuestionId { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;
    }
}
