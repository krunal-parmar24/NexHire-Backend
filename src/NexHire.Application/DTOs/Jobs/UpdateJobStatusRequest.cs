using System.ComponentModel.DataAnnotations;

namespace NexHire.Application.DTOs.Jobs
{
    public class UpdateJobStatusRequest
    {
        [Required]
        public string Status { get; set; } = null!;
    }
}
