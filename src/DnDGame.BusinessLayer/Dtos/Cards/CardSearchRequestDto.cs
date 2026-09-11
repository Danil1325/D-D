using DnDGame.BusinessLayer.Dtos.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Cards;

/// <summary>
/// Query-string-bound request for GET /api/card — paging (inherited from
/// PagedRequest) plus the filter/sort options CardCollection.SearchCards already
/// supports.
/// </summary>
public class CardSearchRequestDto : PagedRequest
{
    public string? Query { get; set; }
    public CardRarity? Rarity { get; set; }
    public CardCategory? Category { get; set; }
    public CardType? Type { get; set; }
    public bool? Unlocked { get; set; }
    public CardSortBy SortBy { get; set; } = CardSortBy.Name;
    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;
}
