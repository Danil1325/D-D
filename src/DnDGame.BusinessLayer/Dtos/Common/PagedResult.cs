namespace DnDGame.BusinessLayer.Dtos.Common;

/// <summary>
/// Generic paged response envelope. Wraps whatever DTO a feature returns (e.g. a
/// future CardResponseDto) so every paginated endpoint in the API has the same
/// shape: the page of items, plus enough metadata for the frontend to build
/// pagination controls without a second request.
/// </summary>
/// <typeparam name="T">The DTO type being paged (never a Domain entity).</typeparam>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount) =>
        new()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
}
