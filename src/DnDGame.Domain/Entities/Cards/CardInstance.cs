namespace DnDGame.Domain.Entities.Cards;

using DnDGame.Domain.Common;

/// <summary>
/// Represents a specific instance of a card in a game session.
/// This tracks runtime state like temporary modifiers that apply to a specific card instance.
/// </summary>
public class CardInstance : BaseEntity
{
    /// <summary>
    /// A unique identifier for this specific card instance.
    /// Allows tracking of individual cards across game sessions and state changes.
    /// </summary>
    public Guid InstanceId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to the Card definition this instance represents.
    /// </summary>
    public int CardId { get; set; }

    /// <summary>
    /// Navigation property to the Card definition.
    /// </summary>
    public virtual Card Card { get; set; } = null!;

    /// <summary>
    /// Temporary modifier to the card's cost for this instance.
    /// Applied on top of the card's BaseCost. Can be negative to reduce cost.
    /// </summary>
    public int TemporaryCostModifier { get; set; } = 0;

    /// <summary>
    /// Temporary modifier to the card's damage for this instance.
    /// Applied on top of the card's BaseDamage. Can be negative to reduce damage.
    /// </summary>
    public int TemporaryDamageModifier { get; set; } = 0;

    /// <summary>
    /// Calculates the effective cost of this card instance.
    /// </summary>
    public int GetEffectiveCost() => Math.Max(0, Card.BaseCost + TemporaryCostModifier);

    /// <summary>
    /// Calculates the effective damage of this card instance.
    /// </summary>
    public int GetEffectiveDamage() => Math.Max(0, Card.BaseDamage + TemporaryDamageModifier);
}
