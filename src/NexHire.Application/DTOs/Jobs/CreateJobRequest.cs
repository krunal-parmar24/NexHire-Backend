using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexHire.Application.DTOs.Jobs
{
    public class CreateJobRequest
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public string Requirements { get; set; } = null!;

        [Required]
        public string Location { get; set; } = null!;

        [Required]
        public string JobType { get; set; } = null!;

        public string? SalaryRange { get; set; }

        [Required]
        public string RemoteType { get; set; } = null!;

        public List<ScreeningQuestionDto> ScreeningQuestions { get; set; } = new();
    }
}
