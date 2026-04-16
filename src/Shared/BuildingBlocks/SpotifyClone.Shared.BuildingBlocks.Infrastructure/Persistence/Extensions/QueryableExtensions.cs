using Microsoft.EntityFrameworkCore;
using SpotifyClone.Shared.BuildingBlocks.Application.Pagination;

namespace SpotifyClone.Shared.BuildingBlocks.Infrastructure.Persistence.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        PaginationParams pagination,
        CancellationToken cancellationToken = default)
    {
        // 1. Спочатку рахуємо загальну кількість (виконує SELECT COUNT)
        int count = await source.CountAsync(cancellationToken);

        // 2. Витягуємо тільки потрібну сторінку (виконує LIMIT / OFFSET)
        List<T> items = await source
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, count, pagination.Page, pagination.PageSize);
    }
}
