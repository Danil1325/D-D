namespace DnDGame.Domain.Engine.Models;

/// <summary>
/// Minimal battle-facing representation of a card in a deck.
/// This placeholder deliberately contains no card behavior; the complete card model
/// can extend it when the card domain is introduced.
/// </summary>
public class CardInstance
{
    public Guid InstanceId { get; set; }

    /// <summary>
    /// Identifies the card definition represented by this instance.
    /// </summary>
    public int CardId { get; set; }
}
