using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.DTO.Pagination
{
    public class PaginationQueryDto
    {
        [Range(1, 1000000, ErrorMessage = "Page number must be between 1 and 1000000.")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }
}
