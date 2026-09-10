namespace DnDGame.BusinessLayer.Engines;

using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

/// <summary>
/// Engine for managing deck operations during battle.
/// Handles creating, shuffling, drawing, and discarding cards.
/// Implements automatic reshuffle logic when DrawPile is exhausted.
/// </summary>
public class DeckEngine : IDeckEngine
{
    private readonly Random _random;

    /// <summary>
    /// Creates a new instance of DeckEngine.
    /// Uses a new Random instance for shuffling operations.
    /// </summary>
    public DeckEngine()
    {
        _random = new Random();
    }

    /// <summary>
    /// Creates a BattleDeck from a regular Deck, initializing it for battle.
    /// Converts all Card entities to CardInstance entities and shuffles them into the DrawPile.
    /// </summary>
    /// <param name="deck">The deck to convert to a battle deck.</param>
    /// <returns>A new BattleDeck ready for battle with cards shuffled into DrawPile.</returns>
    /// <exception cref="ArgumentNullException">Thrown when deck is null.</exception>
    public BattleDeck CreateBattleDeck(Deck deck)
    {
        if (deck == null)
        {
            throw new ArgumentNullException(nameof(deck), "Deck cannot be null.");
        }

        var battleDeck = new BattleDeck
        {
            DeckId = deck.Id,
            Deck = deck,
            DrawPile = new List<CardInstance>(),
            DiscardPile = new List<CardInstance>(),
            Hand = new List<CardInstance>(),
            TotalCardsDrawn = 0,
            TotalCardsDiscarded = 0
        };

        // Convert each Card to a CardInstance and add to DrawPile
        foreach (var card in deck.Cards)
        {
            var cardInstance = new CardInstance
            {
                CardId = card.Id,
                Card = card
            };
            battleDeck.DrawPile.Add(cardInstance);
        }

        // Shuffle the DrawPile
        ShuffleDeck(battleDeck.DrawPile);

        return battleDeck;
    }

    /// <summary>
    /// Shuffles a list of cards randomly using the Fisher-Yates shuffle algorithm.
    /// Modifies the list in place.
    /// </summary>
    /// <param name="cards">The list of cards to shuffle.</param>
    public void ShuffleDeck(IList<CardInstance> cards)
    {
        if (cards == null || cards.Count <= 1)
        {
            return;
        }

        // Fisher-Yates shuffle algorithm
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = _random.Next(i + 1);

            // Swap
            (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
        }
    }

    /// <summary>
    /// Draws a single card from the battle deck's DrawPile.
    /// If DrawPile is empty, automatically reshuffles the DiscardPile into the DrawPile before drawing.
    /// If no cards are available at all, returns null.
    /// </summary>
    /// <param name="battleDeck">The battle deck to draw from.</param>
    /// <returns>A CardInstance drawn from the deck, or null if no cards are available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    public CardInstance? DrawCard(BattleDeck battleDeck)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        // If DrawPile is empty, try to reshuffle the DiscardPile
        if (battleDeck.DrawPile.Count == 0)
        {
            ReshuffleDiscardPile(battleDeck);
        }

        // If still no cards, return null
        if (battleDeck.DrawPile.Count == 0)
        {
            return null;
        }

        // Draw the top card from the DrawPile (last card for efficiency)
        var drawnCard = battleDeck.DrawPile[battleDeck.DrawPile.Count - 1];
        battleDeck.DrawPile.RemoveAt(battleDeck.DrawPile.Count - 1);

        // Add to hand and update statistics
        battleDeck.Hand.Add(drawnCard);
        battleDeck.TotalCardsDrawn++;

        return drawnCard;
    }

    /// <summary>
    /// Draws multiple cards from the battle deck's DrawPile.
    /// If DrawPile is exhausted during drawing, automatically reshuffles the DiscardPile and continues.
    /// Continues until the requested count is reached or no more cards are available.
    /// </summary>
    /// <param name="battleDeck">The battle deck to draw from.</param>
    /// <param name="count">The number of cards to draw.</param>
    /// <returns>A list of CardInstances drawn from the deck. May contain fewer cards than requested if insufficient cards available.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    /// <exception cref="ArgumentException">Thrown when count is less than or equal to 0.</exception>
    public IList<CardInstance> DrawCards(BattleDeck battleDeck, int count)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        if (count <= 0)
        {
            throw new ArgumentException("Count must be greater than 0.", nameof(count));
        }

        var drawnCards = new List<CardInstance>();

        for (int i = 0; i < count; i++)
        {
            var card = DrawCard(battleDeck);
            if (card == null)
            {
                // No more cards available
                break;
            }

            drawnCards.Add(card);
        }

        return drawnCards;
    }

    /// <summary>
    /// Discards a card from the player's hand to the DiscardPile.
    /// Removes the card from the Hand and moves it to DiscardPile.
    /// </summary>
    /// <param name="battleDeck">The battle deck being played.</param>
    /// <param name="card">The card to discard.</param>
    /// <returns>True if the card was successfully discarded from hand, false if card not found in hand.</returns>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck or card is null.</exception>
    public bool DiscardCard(BattleDeck battleDeck, CardInstance card)
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
            return false;
        }

        // Add to discard pile and update statistics
        battleDeck.DiscardPile.Add(card);
        battleDeck.TotalCardsDiscarded++;

        return true;
    }

    /// <summary>
    /// Reshuffles the DiscardPile back into the DrawPile.
    /// Moves all cards from DiscardPile to DrawPile and shuffles them.
    /// Called automatically when DrawPile becomes empty and we need more cards to draw.
    /// If DiscardPile is also empty, no action is taken.
    /// </summary>
    /// <param name="battleDeck">The battle deck to reshuffle.</param>
    /// <exception cref="ArgumentNullException">Thrown when battleDeck is null.</exception>
    public void ReshuffleDiscardPile(BattleDeck battleDeck)
    {
        if (battleDeck == null)
        {
            throw new ArgumentNullException(nameof(battleDeck), "Battle deck cannot be null.");
        }

        // If DiscardPile is empty, nothing to reshuffle
        if (battleDeck.DiscardPile.Count == 0)
        {
            return;
        }

        // Move all cards from DiscardPile to DrawPile
        while (battleDeck.DiscardPile.Count > 0)
        {
            var card = battleDeck.DiscardPile[0];
            battleDeck.DiscardPile.RemoveAt(0);
            battleDeck.DrawPile.Add(card);
        }

        // Shuffle the newly populated DrawPile
        ShuffleDeck(battleDeck.DrawPile);
    }
}
