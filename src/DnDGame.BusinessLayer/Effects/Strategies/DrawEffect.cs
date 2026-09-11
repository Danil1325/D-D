namespace DnDGame.BusinessLayer.Effects.Strategies;

using DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Effect that draws cards from the player's deck.
/// Respects maximum hand size - will not draw if hand is full.
/// </summary>
public class DrawEffect : ICardEffect
{
    public string EffectName => "Draw";

    /// <summary>
    /// Draw effect can apply if:
    /// - Hand engine is available
    /// - Hand rules are available
    /// - There's room in the hand for at least one card
    /// - Deck has cards available
    /// </summary>
    public bool CanApply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Need hand engine to draw cards
        if (context.HandEngine == null)
        {
            return false;
        }

        // Need hand rules to check size limit
        if (context.HandRules == null)
        {
            return false;
        }

        // Need deck engine to draw from
        if (context.DeckEngine == null)
        {
            return false;
        }

        // Check if there's room in hand
        int currentHandSize = context.HandEngine.GetHandSize(context.PlayerBattleDeck);
        if (!context.HandRules.CanAddCard(currentHandSize))
        {
            return false; // Hand is full
        }

        // Check if deck has cards (draw pile + discard pile)
        if (context.PlayerBattleDeck.DrawPile.Count == 0 && context.PlayerBattleDeck.DiscardPile.Count == 0)
        {
            return false; // No cards available
        }

        return true;
    }

    /// <summary>
    /// Draws cards from the player's deck, respecting hand size limits.
    /// Uses the card's BaseDamage value as number of cards to draw (default 1).
    /// </summary>
    public string Apply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!CanApply(context))
        {
            return "Cannot draw: hand is full or no cards available.";
        }

        // Number of cards to draw (using BaseDamage as card count for now)
        int cardsToDraw = context.EffectiveCardDamage;
        if (cardsToDraw <= 0)
        {
            cardsToDraw = 1; // Default to 1 card if cost is 0
        }

        int currentHandSize = context.HandEngine!.GetHandSize(context.PlayerBattleDeck);
        int spaceInHand = context.HandRules!.MaxHandSize - currentHandSize;

        // Draw only as many cards as fit in hand
        int actualCardsToDraw = Math.Min(cardsToDraw, spaceInHand);

        // Draw cards from deck
        var drawnCards = context.DeckEngine!.DrawCards(context.PlayerBattleDeck, actualCardsToDraw);

        if (drawnCards == null || drawnCards.Count == 0)
        {
            return "No cards could be drawn.";
        }

        // DeckEngine.DrawCards already moves each drawn card into Hand. The space
        // calculation above guarantees this does not exceed the hand limit.
        return $"Drew {drawnCards.Count} card(s) (hand size: {context.HandEngine.GetHandSize(context.PlayerBattleDeck)}/{context.HandRules.MaxHandSize}).";
    }
}
