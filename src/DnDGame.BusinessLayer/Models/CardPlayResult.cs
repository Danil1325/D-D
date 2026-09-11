namespace DnDGame.BusinessLayer.Models;

using DnDGame.Domain.Entities.Cards;

/// <summary>
/// Represents the result of a successful card play.
/// Contains information about what happened during the card's execution.
/// </summary>
public class CardPlayResult
{
    /// <summary>
    /// The card that was played.
    /// </summary>
    public CardInstance PlayedCard { get; set; } = null!;

    /// <summary>
    /// The energy consumed by playing this card.
    /// </summary>
    public int EnergyCost { get; set; }

    /// <summary>
    /// The remaining energy after playing the card.
    /// </summary>
    public int RemainingEnergy { get; set; }

    /// <summary>
    /// Whether the card's effects were successfully applied.
    /// </summary>
    public bool EffectsApplied { get; set; }

    /// <summary>
    /// Details or description of what the card did.
    /// </summary>
    public string? EffectDescription { get; set; }

    /// <summary>
    /// Timestamp when the card was played.
    /// </summary>
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a new successful card play result.
    /// </summary>
    /// <param name="card">The card that was played.</param>
    /// <param name="energyCost">The energy consumed.</param>
    /// <param name="remainingEnergy">Energy after the play.</param>
    /// <param name="effectsApplied">Whether effects were applied.</param>
    /// <param name="effectDescription">Optional description of effects.</param>
    public CardPlayResult(CardInstance card, int energyCost, int remainingEnergy, bool effectsApplied, string? effectDescription = null)
    {
        PlayedCard = card;
        EnergyCost = energyCost;
        RemainingEnergy = remainingEnergy;
        EffectsApplied = effectsApplied;
        EffectDescription = effectDescription;
    }
}
