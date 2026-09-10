namespace DnDGame.BusinessLayer.Engines;

using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

/// <summary>
/// Engine for managing hand operations during gameplay.
/// Handles adding, removing, finding, and discarding cards from the player's hand.
/// Enforces hand size limits defined in HandRules.
/// </summary>
public class HandEngine : IHandEngine
{
    private readonly HandRules _handRules;

    /// <summary>
    /// Creates a new instance of HandEngine with the specified hand rules.
    /// </summary>
    /// <param name="handRules">The configurable rules for hand management.</param>
    /// <exception cref="ArgumentNullException">Thrown when handRules is null.</exception>
    public HandEngine(HandRules handRules)
    {
        _handRules = handRules ?? throw new ArgumentNullException(nameof(handRules));
    }

    /// <summary>
    /// Adds a single card to the player's hand, respecting the maximum hand size limit.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="card">The card to add to the hand.</param>
    /// <returns>An EngineResult indicating success or HAND_FULL error if hand is at capacity.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck or card is null.</exception>
    public EngineResult AddCard(BattleDeck battleDeck, CardInstance card)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        if (card == null)
        {
            throw new ArgumentNullException(nameof(card), "Card cannot be null.");
        }

        // Check if hand is full
        if (!_handRules.CanAddCard(battleDeck.Hand.Count))
        {
            return EngineResult.Failure(
                ErrorCode.HAND_FULL,
                $"Hand is full. Current hand size: {battleDeck.Hand.Count}, Maximum allowed: {_handRules.MaxHandSize}."
            );
        }

        // Add card to hand
        battleDeck.Hand.Add(card);
        return EngineResult.Success();
    }

    /// <summary>
    /// Adds multiple cards to the player's hand.
    /// All cards must fit within the hand size limit; partial additions are not performed.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="cards">The cards to add to the hand.</param>
    /// <returns>An EngineResult indicating success or HAND_FULL error if not all cards fit.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck or cards is null.</exception>
    public EngineResult AddCards(BattleDeck battleDeck, IList<CardInstance> cards)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        if (cards == null)
        {
            throw new ArgumentNullException(nameof(cards), "Cards collection cannot be null.");
        }

        // Check if all cards can fit
        if (!_handRules.CanAddCards(battleDeck.Hand.Count, cards.Count))
        {
            return EngineResult.Failure(
                ErrorCode.HAND_FULL,
                $"Cannot add {cards.Count} card(s). Hand would exceed maximum. " +
                $"Current hand size: {battleDeck.Hand.Count}, Maximum allowed: {_handRules.MaxHandSize}."
            );
        }

        // Add all cards to hand
        foreach (var card in cards)
        {
            if (card != null)
            {
                battleDeck.Hand.Add(card);
            }
        }

        return EngineResult.Success();
    }

    /// <summary>
    /// Removes a card from the player's hand.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="card">The card to remove from the hand.</param>
    /// <returns>An EngineResult indicating success or CARD_NOT_IN_HAND error if card is not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck or card is null.</exception>
    public EngineResult RemoveCard(BattleDeck battleDeck, CardInstance card)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        if (card == null)
        {
            throw new ArgumentNullException(nameof(card), "Card cannot be null.");
        }

        // Try to remove the card from hand
        if (!battleDeck.Hand.Remove(card))
        {
            return EngineResult.Failure(
                ErrorCode.CARD_NOT_IN_HAND,
                $"Card with instance ID '{card.InstanceId}' and definition ID '{card.CardId}' is not in the hand."
            );
        }

        return EngineResult.Success();
    }

    /// <summary>
    /// Finds a card in the player's hand by card instance ID.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="cardInstanceId">The instance ID of the card to find.</param>
    /// <returns>The CardInstance if found, null otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    public CardInstance? FindCard(BattleDeck battleDeck, Guid cardInstanceId)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        return battleDeck.Hand.FirstOrDefault(c => c.InstanceId == cardInstanceId);
    }

    /// <summary>
    /// Finds a card in the player's hand by card definition ID.
    /// Returns the first card found with the matching card ID.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <param name="cardId">The card definition ID to search for.</param>
    /// <returns>The first CardInstance with matching card ID, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    public CardInstance? FindCardByDefinitionId(BattleDeck battleDeck, int cardId)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        return battleDeck.Hand.FirstOrDefault(c => c.CardId == cardId);
    }

    /// <summary>
    /// Discards all cards from the player's hand back to the discard pile.
    /// Clears the hand and moves all cards to the battle deck's discard pile.
    /// </summary>
    /// <param name="battleDeck">The battle deck to discard from.</param>
    /// <returns>An EngineResult indicating success with the number of cards discarded.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    public EngineResult DiscardHand(BattleDeck battleDeck)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        int cardsDiscarded = battleDeck.Hand.Count;

        // Move all cards from hand to discard pile
        while (battleDeck.Hand.Count > 0)
        {
            var card = battleDeck.Hand[0];
            battleDeck.Hand.RemoveAt(0);
            battleDeck.DiscardPile.Add(card);
            battleDeck.TotalCardsDiscarded++;
        }

        return EngineResult.Success();
    }

    /// <summary>
    /// Gets the current size of the player's hand.
    /// </summary>
    /// <param name="battleDeck">The battle deck containing the hand.</param>
    /// <returns>The number of cards currently in the hand.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    public int GetHandSize(BattleDeck battleDeck)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        return battleDeck.Hand.Count;
    }
}
