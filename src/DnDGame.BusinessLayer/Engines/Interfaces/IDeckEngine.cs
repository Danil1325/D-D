namespace DnDGame.BusinessLayer.Engines.Interfaces;

using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

/// <summary>
/// Interface for the deck engine that manages deck operations during battle.
/// Handles creating, shuffling, drawing, and discarding cards.
/// </summary>
public interface IDeckEngine
{
    /// <summary>
    /// Creates a BattleDeck from a regular Deck, initializing it for battle.
    /// Shuffles all cards from the deck into the DrawPile.
    /// </summary>
    /// <param name="deck">The deck to convert to a battle deck.</param>
    /// <returns>A new BattleDeck ready for battle with cards shuffled into DrawPile.</returns>
    /// <exception cref="ArgumentNullException">Thrown when deck is null.</exception>
    BattleDeck CreateBattleDeck(Deck deck);

    /// <summary>
    /// Shuffles a list of cards randomly.
    /// </summary>
    /// <param name="cards">The list of cards to shuffle. Modified in place.</param>
    void ShuffleDeck(IList<CardInstance> cards);

    /// <summary>
    /// Draws a single card from the battle deck's DrawPile.
    /// If DrawPile is empty, reshuffles the DiscardPile into the DrawPile before drawing.
    /// If no cards are available at all, returns null.
    /// </summary>
    /// <param name="battleDeck">The battle deck to draw from.</param>
    /// <returns>A CardInstance drawn from the deck, or null if no cards are available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    CardInstance? DrawCard(BattleDeck battleDeck);

    /// <summary>
    /// Draws multiple cards from the battle deck's DrawPile.
    /// If DrawPile is exhausted during drawing, reshuffles the DiscardPile and continues.
    /// </summary>
    /// <param name="battleDeck">The battle deck to draw from.</param>
    /// <param name="count">The number of cards to draw.</param>
    /// <returns>A list of CardInstances drawn from the deck. May contain fewer cards than requested if insufficient cards available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    /// <exception cref="ArgumentException">Thrown when count is less than or equal to 0.</exception>
    IList<CardInstance> DrawCards(BattleDeck battleDeck, int count);

    /// <summary>
    /// Discards a card from the player's hand to the DiscardPile.
    /// </summary>
    /// <param name="battleDeck">The battle deck being played.</param>
    /// <param name="card">The card to discard.</param>
    /// <returns>True if the card was successfully discarded from hand, false if card not found in hand.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck or card is null.</exception>
    bool DiscardCard(BattleDeck battleDeck, CardInstance card);

    /// <summary>
    /// Reshuffles the DiscardPile back into the DrawPile.
    /// Called when DrawPile becomes empty and we need more cards to draw.
    /// </summary>
    /// <param name="battleDeck">The battle deck to reshuffle.</param>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    void ReshuffleDiscardPile(BattleDeck battleDeck);
}
