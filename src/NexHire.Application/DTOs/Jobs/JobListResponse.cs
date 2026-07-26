using System.Collections.Generic;

namespace NexHire.Application.DTOs.Jobs
{
    public class JobListResponse
    {
        public List<JobListItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
