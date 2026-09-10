namespace DnDGame.BusinessLayer.Dtos.Common;

/// <summary>
/// Generic "give me page N of size M" request shape, meant to be reused by any
/// future paginated endpoint (Cards, Deck, etc.) once those features exist.
///
/// This class intentionally has no feature-specific fields (no CardId, no filter
/// text) — those belong in a feature's own request DTO, which can inherit from or
/// contain a PagedRequest instead of duplicating Page/PageSize itself.
/// </summary>
public class PagedRequest
{
    /// <summary>1-based page number. Values below 1 are treated as 1 by Normalize().</summary>
    public int Page { get; set; } = 1;

    /// <summary>How many items per page. Clamped to [1, MaxPageSize] by Normalize().</summary>
    public int PageSize { get; set; } = DefaultPageSize;

    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>
    /// Returns a corrected copy with Page/PageSize clamped to sane bounds. Controllers
    /// should call this before handing the request to a service, so services never have
    /// to re-check for zero/negative/huge values themselves.
    /// </summary>
    public PagedRequest Normalize()
    {
        var page = Page < 1 ? 1 : Page;
        var pageSize = PageSize < 1 ? DefaultPageSize : Math.Min(PageSize, MaxPageSize);
        return new PagedRequest { Page = page, PageSize = pageSize };
    }
}
