namespace DnDGame.Domain.Entities.Cards;

using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

/// <summary>
/// Represents a card definition in the game system.
/// This is the blueprint for a card; each instance of the card in gameplay is represented by CardInstance.
/// </summary>
public class Card : BaseEntity
{
    /// <summary>
    /// The unique name of the card.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A detailed description of what the card does.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The type of the card (Spell, Weapon, Armor, etc.).
    /// </summary>
    public CardType CardType { get; set; }

    /// <summary>
    /// The category of the card (Offensive, Defensive, etc.).
    /// </summary>
    public CardCategory Category { get; set; }

    /// <summary>
    /// The rarity level of the card.
    /// </summary>
    public CardRarity Rarity { get; set; }

    /// <summary>
    /// The base cost to play this card (e.g., mana, resources, action points).
    /// </summary>
    public int BaseCost { get; set; }

    /// <summary>
    /// Resource consumed when this card is used as an ability or spell. This is
    /// an alias for <see cref="BaseCost"/>, so gameplay has one source of truth
    /// for card cost.
    /// </summary>
    public int ResourceCost
    {
        get => BaseCost;
        set => BaseCost = value;
    }

    /// <summary>Optional class restriction for an ability or spell.</summary>
    public int? RequiredClassId { get; set; }

    /// <summary>Minimum character level needed to use an ability or spell.</summary>
    public int RequiredLevel { get; set; } = 1;

    /// <summary>
    /// The base damage value of the card (if applicable).
    /// </summary>
    public int BaseDamage { get; set; }

    /// <summary>
    /// The type of target this card can affect.
    /// </summary>
    public TargetType TargetType { get; set; }

    /// <summary>
    /// The primary effect type this card produces.
    /// </summary>
    public EffectType EffectType { get; set; }

    /// <summary>
    /// Indicates whether this card is playable in the current game state.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for all instances of this card in play.
    /// </summary>
    public virtual ICollection<CardInstance> Instances { get; set; } = new List<CardInstance>();
}
