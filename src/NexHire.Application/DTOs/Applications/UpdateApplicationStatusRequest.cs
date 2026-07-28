using System.ComponentModel.DataAnnotations;

namespace NexHire.Application.DTOs.Applications
{
    public class UpdateApplicationStatusRequest
    {
        [Required]
        public string Status { get; set; } = null!;
    }
}
