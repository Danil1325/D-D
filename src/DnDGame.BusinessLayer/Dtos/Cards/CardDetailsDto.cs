using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Dtos.Cards;

/// <summary>Mirrors DnDGame.Domain.Entities.Cards.CardDetails — a visibility-filtered card projection.</summary>
public class CardDetailsDto
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

    public static CardDetailsDto FromDomain(CardDetails details) => new()
    {
        CardId = details.CardId,
        Unlocked = details.Unlocked,
        Quantity = details.Quantity,
        Name = details.Name,
        Description = details.Description,
        CardType = details.CardType,
        Category = details.Category,
        Rarity = details.Rarity,
        BaseCost = details.BaseCost,
        BaseDamage = details.BaseDamage,
        TargetType = details.TargetType,
        EffectType = details.EffectType
    };
}
