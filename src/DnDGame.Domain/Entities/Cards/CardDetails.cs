using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Cards;

/// <summary>Safe, visibility-filtered representation of a card for display.</summary>
public sealed class CardDetails
{
    public int CardId { get; init; }
    public bool Unlocked { get; init; }
    public int? Quantity { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public CardType? CardType { get; init; }
    public CardCategory? Category { get; init; }
    public CardRarity? Rarity { get; init; }
    public int? BaseCost { get; init; }
    public int? BaseDamage { get; init; }
    public TargetType? TargetType { get; init; }
    public EffectType? EffectType { get; init; }
}
