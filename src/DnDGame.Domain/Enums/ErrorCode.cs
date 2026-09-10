namespace DnDGame.Domain.Enums;

/// <summary>
/// Error codes used throughout the engine for consistent error handling.
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// Deck validation failed for a general reason.
    /// </summary>
    DECK_INVALID = 0,

    /// <summary>
    /// The deck contains fewer cards than the minimum required.
    /// </summary>
    DECK_TOO_SMALL = 1,

    /// <summary>
    /// The deck contains more cards than the maximum allowed.
    /// </summary>
    DECK_TOO_LARGE = 2,

    /// <summary>
    /// A card appears more times in the deck than the maximum copies allowed per card.
    /// </summary>
    CARD_COPY_LIMIT_REACHED = 3
}
