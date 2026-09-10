namespace DnDGame.BusinessLayer.Repositories;

using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

/// <summary>
/// Validator for deck composition against configurable rules.
/// Ensures decks meet minimum/maximum size requirements, card copy limits, and contain only valid cards.
/// </summary>
public class DeckValidator : IDeckValidator
{
    private readonly DeckRules _deckRules;

    /// <summary>
    /// Creates a new instance of DeckValidator with the specified deck rules.
    /// </summary>
    /// <param name="deckRules">The configurable rules to validate decks against.</param>
    /// <exception cref="ArgumentNullException">Thrown when deckRules is null.</exception>
    public DeckValidator(DeckRules deckRules)
    {
        _deckRules = deckRules ?? throw new ArgumentNullException(nameof(deckRules));
    }

    /// <summary>
    /// Validates a deck against the configured deck rules.
    /// Performs the following checks:
    /// 1. Validates deck object is not null
    /// 2. Validates deck contains cards collection
    /// 3. Validates all cards in the deck are valid (not null)
    /// 4. Validates deck size meets minimum requirement
    /// 5. Validates deck size does not exceed maximum
    /// 6. Validates no card exceeds the maximum copies limit
    /// </summary>
    /// <param name="deck">The deck to validate.</param>
    /// <returns>
    /// An EngineResult indicating success (IsSuccess=true) or failure with appropriate error code:
    /// - DECK_INVALID: Deck, cards collection, or a card is null/invalid
    /// - DECK_TOO_SMALL: Deck size is below minimum
    /// - DECK_TOO_LARGE: Deck size exceeds maximum
    /// - CARD_COPY_LIMIT_REACHED: A card appears more than the allowed copies
    /// </returns>
    public EngineResult ValidateDeck(Deck deck)
    {
        // Validate deck object
        if (deck == null)
        {
            return EngineResult.Failure(
                ErrorCode.DECK_INVALID,
                "Deck cannot be null."
            );
        }

        // Validate deck has cards collection
        if (deck.Cards == null)
        {
            return EngineResult.Failure(
                ErrorCode.DECK_INVALID,
                "Deck cards collection cannot be null."
            );
        }

        // Validate all cards are valid (not null)
        var invalidCards = deck.Cards.Where(c => c == null).ToList();
        if (invalidCards.Any())
        {
            return EngineResult.Failure(
                ErrorCode.DECK_INVALID,
                $"Deck contains {invalidCards.Count} invalid (null) card(s)."
            );
        }

        var deckSize = deck.Cards.Count;

        // Validate minimum deck size
        if (deckSize < _deckRules.MinimumDeckSize)
        {
            return EngineResult.Failure(
                ErrorCode.DECK_TOO_SMALL,
                $"Deck contains {deckSize} card(s) but requires a minimum of {_deckRules.MinimumDeckSize}."
            );
        }

        // Validate maximum deck size
        if (deckSize > _deckRules.MaximumDeckSize)
        {
            return EngineResult.Failure(
                ErrorCode.DECK_TOO_LARGE,
                $"Deck contains {deckSize} card(s) but the maximum allowed is {_deckRules.MaximumDeckSize}."
            );
        }

        // Validate card copy limits
        var cardCopyCounts = new Dictionary<int, int>();
        foreach (var card in deck.Cards)
        {
            if (!cardCopyCounts.ContainsKey(card.Id))
            {
                cardCopyCounts[card.Id] = 0;
            }

            cardCopyCounts[card.Id]++;

            // Check if this card exceeds the copy limit
            if (cardCopyCounts[card.Id] > _deckRules.MaximumCopiesPerCard)
            {
                return EngineResult.Failure(
                    ErrorCode.CARD_COPY_LIMIT_REACHED,
                    $"Card ID {card.Id} ('{card.Name}') appears {cardCopyCounts[card.Id]} time(s) " +
                    $"but the maximum allowed is {_deckRules.MaximumCopiesPerCard}."
                );
            }
        }

        // All validations passed
        return EngineResult.Success();
    }
}
