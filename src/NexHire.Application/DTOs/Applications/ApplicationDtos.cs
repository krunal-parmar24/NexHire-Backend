using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NexHire.Application.DTOs.Applications
{
    public sealed record AnswerDto(
        [Required] [property: JsonPropertyName("questionId")] string QuestionId,
        [Required] [property: JsonPropertyName("value")] string Value
    );

    public sealed record SubmitApplicationRequest(
        [Required] Guid JobId,
        [Required] List<AnswerDto> Answers
    );

    public sealed record SubmitApplicationResponse(
        Guid ApplicationId,
        string Status
    );
    
    public sealed record WithdrawApplicationResponse(
        string Status
    );
    
    public sealed record ApplicationDto(
        Guid Id,
        Guid JobId,
        string JobTitle,
        string CompanyName,
        string Status,
        DateTime SubmittedAt
    );

    public sealed record ApplicantDto(
        Guid ApplicationId,
        string ApplicantName,
        string Status,
        List<AnswerDto> Answers,
        string? ResumeUrl,
        string? ProfileSummary
    );
}
