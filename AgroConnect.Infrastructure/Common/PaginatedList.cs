using Microsoft.EntityFrameworkCore;

namespace AgroConnect.Infrastructure.Common
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalCount = count;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 1;
            AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // EF Core IQueryable üçün (DB səviyyəsində Skip/Take)
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;

            var count = await source.CountAsync();
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        // Yaddaşdakı List üçün (məsələn rol filtri kimi DB-də edilə bilməyən hallarda)
        public static PaginatedList<T> Create(List<T> source, int pageIndex, int pageSize)
        {
            if (pageIndex < 1) pageIndex = 1;

            var count = source.Count;
            var items = source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}