using BuildingBlocks.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Bms.Application.Common.Paging;

/// <summary>Phân trang ở DB (CountAsync + Skip/Take) — thay cho PagedResult.CreateAsync của QMS.</summary>
public static class QueryablePagingExtensions
{
    /// <summary>
    /// Đếm tổng + lấy 1 trang ngay tại DB. Gọi SAU khi đã Where/OrderBy/Select (projection)
    /// để SQL chỉ lấy đúng cột cần. Luôn OrderBy trước khi phân trang để kết quả ổn định.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageIndex = PagedResult<T>.NormalizePageIndex(pageIndex);
        pageSize = PagedResult<T>.NormalizePageSize(pageSize);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<T>.Create(items, pageIndex, pageSize, totalCount);
    }
}
