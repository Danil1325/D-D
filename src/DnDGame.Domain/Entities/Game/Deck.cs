namespace DnDGame.Domain.Entities.Game;

using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Cards;

/// <summary>
/// Represents a player's deck in the game system.
/// A deck contains a collection of cards that can be played during gameplay.
/// </summary>
public class Deck : BaseEntity
{
    /// <summary>
    /// The name of the deck (e.g., "Fire Mage Deck", "Knight's Arsenal").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The collection of cards in this deck.
    /// </summary>
    public ICollection<Card> Cards { get; set; } = new List<Card>();

    /// <summary>
    /// The ID of the character that owns this deck.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// Optional description of the deck's strategy or purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
