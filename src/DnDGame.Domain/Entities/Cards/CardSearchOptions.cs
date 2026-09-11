using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Cards;

/// <summary>Optional filters and ordering for a read-only card collection search.</summary>
public sealed class CardSearchOptions
{
    public CardRarity? Rarity { get; init; }
    public CardCategory? Category { get; init; }
    public CardType? Type { get; init; }
    public bool? Unlocked { get; init; }
    public CardSortBy SortBy { get; init; } = CardSortBy.Name;
    public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
}
