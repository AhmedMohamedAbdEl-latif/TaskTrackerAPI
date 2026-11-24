using Microsoft.EntityFrameworkCore;

namespace TaskTrackerAPI.DTO.Pagination
{
    public static class PaginationExtensions
    {
        public static async Task<PagedResultDto<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PaginationQueryDto pagination,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        public static bool IsValid(this PaginationQueryDto pagination)
        {
            return pagination.PageNumber is >= 1 and <= 1_000_000 &&
                pagination.PageSize is >= 1 and <= 100 &&
                ((long)pagination.PageNumber - 1) * pagination.PageSize <= int.MaxValue;
        }
    }
}
