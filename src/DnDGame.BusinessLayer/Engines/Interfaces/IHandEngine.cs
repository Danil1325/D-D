namespace DnDGame.BusinessLayer.Engines.Interfaces;

using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

/// <summary>
/// Interface for the hand engine that manages hand operations during gameplay.
/// Handles adding, removing, finding, and discarding cards from the player's hand.
/// </summary>
public interface IHandEngine
{
    /// <summary>
    /// Adds a card to the player's hand, respecting the maximum hand size limit.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="card">The card to add to the hand.</param>
    /// <returns>An EngineResult indicating success or HAND_FULL error if hand is at capacity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck or card is null.</exception>
    EngineResult AddCard(BattleDeck battleDeck, CardInstance card);

    /// <summary>
    /// Adds multiple cards to the player's hand.
    /// All cards must fit within the hand size limit; partial additions are not performed.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="cards">The cards to add to the hand.</param>
    /// <returns>An EngineResult indicating success or HAND_FULL error if not all cards fit.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck or cards is null.</exception>
    EngineResult AddCards(BattleDeck battleDeck, IList<CardInstance> cards);

    /// <summary>
    /// Removes a card from the player's hand.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="card">The card to remove from the hand.</param>
    /// <returns>An EngineResult indicating success or CARD_NOT_IN_HAND error if card is not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck or card is null.</exception>
    EngineResult RemoveCard(BattleDeck battleDeck, CardInstance card);

    /// <summary>
    /// Finds a card in the player's hand by card ID and instance ID.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="cardInstanceId">The instance ID of the card to find.</param>
    /// <returns>The CardInstance if found, null otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    CardInstance? FindCard(BattleDeck battleDeck, Guid cardInstanceId);

    /// <summary>
    /// Finds a card in the player's hand by card definition ID.
    /// Returns the first card found with the matching card ID.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="cardId">The card definition ID to search for.</param>
    /// <returns>The first CardInstance with matching card ID, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    CardInstance? FindCardByDefinitionId(BattleDeck battleDeck, int cardId);

    /// <summary>
    /// Discards all cards from the player's hand back to the discard pile.
    /// Clears the hand and moves all cards to the battle deck's discard pile.
    /// </summary>
    /// <param name="battleDeck">The battle deck to discard from.</param>
    /// <returns>An EngineResult indicating success or failure. Returns the number of cards discarded.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    EngineResult DiscardHand(BattleDeck battleDeck);

    /// <summary>
    /// Gets the current size of the player's hand.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <returns>The number of cards currently in the hand.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    int GetHandSize(BattleDeck battleDeck);
}
