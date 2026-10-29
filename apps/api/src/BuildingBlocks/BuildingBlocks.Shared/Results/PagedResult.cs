namespace BuildingBlocks.Shared.Results;

/// <summary>
/// Kết quả phân trang (giống QMS). Bản thuần, không phụ thuộc EF Core;
/// phân trang ở DB dùng extension <c>ToPagedResultAsync</c> trong Bms.Application.
/// </summary>
public sealed class PagedResult<T>
{
    public const int UpperPageSize = 100;
    public const int DefaultPageSize = 10;
    public const int DefaultPageIndex = 1;

    private PagedResult(IReadOnlyList<T> items, int pageIndex, int pageSize, int totalCount)
    {
        Items = items;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int PageIndex { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public bool HasNextPage => PageIndex * PageSize < TotalCount;

    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>Tạo từ một trang đã được truy vấn sẵn (items đã Skip/Take).</summary>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int pageIndex, int pageSize, int totalCount)
        => new(items, pageIndex, pageSize, totalCount);

    /// <summary>Chuẩn hóa pageIndex (>= 1).</summary>
    public static int NormalizePageIndex(int pageIndex) => pageIndex <= 0 ? DefaultPageIndex : pageIndex;

    /// <summary>Chuẩn hóa pageSize trong khoảng [1, UpperPageSize].</summary>
    public static int NormalizePageSize(int pageSize) =>
        pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, UpperPageSize);
}
