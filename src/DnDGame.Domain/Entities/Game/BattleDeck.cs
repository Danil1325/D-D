namespace DnDGame.Domain.Entities.Game;

using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Cards;

/// <summary>
/// Represents the runtime state of a deck during battle.
/// Tracks DrawPile (cards available to draw), DiscardPile (cards that have been played/discarded),
/// and Hand (cards currently held by the player).
/// </summary>
public class BattleDeck : BaseEntity
{
    /// <summary>
    /// The ID of the original deck this battle deck is based on.
    /// </summary>
    public int DeckId { get; set; }

    /// <summary>
    /// Navigation property to the original deck.
    /// </summary>
    public virtual Deck Deck { get; set; } = null!;

    /// <summary>
    /// The pile of cards available to draw from.
    /// When empty, the DiscardPile will be shuffled and moved here.
    /// </summary>
    public IList<CardInstance> DrawPile { get; set; } = new List<CardInstance>();

    /// <summary>
    /// The pile of cards that have been played or discarded.
    /// Used to reshuffle back into the draw pile when it's empty.
    /// </summary>
    public IList<CardInstance> DiscardPile { get; set; } = new List<CardInstance>();

    /// <summary>
    /// The cards currently held in the player's hand.
    /// </summary>
    public IList<CardInstance> Hand { get; set; } = new List<CardInstance>();

    /// <summary>
    /// The total number of cards drawn from this deck in the current game session.
    /// Useful for tracking deck statistics.
    /// </summary>
    public int TotalCardsDrawn { get; set; }

    /// <summary>
    /// The total number of cards discarded from this deck in the current game session.
    /// </summary>
    public int TotalCardsDiscarded { get; set; }
}
