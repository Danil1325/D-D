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
    CARD_COPY_LIMIT_REACHED = 3,

    /// <summary>
    /// The player's hand is full and cannot accept more cards.
    /// </summary>
    HAND_FULL = 4,

    /// <summary>
    /// The specified card is not found in the player's hand.
    /// </summary>
    CARD_NOT_IN_HAND = 5,

    /// <summary>
    /// No battle is currently active or the battle is not in a playable state.
    /// </summary>
    BATTLE_NOT_ACTIVE = 6,

    /// <summary>
    /// It is not the player's turn to play a card.
    /// </summary>
    NOT_PLAYER_TURN = 7,

    /// <summary>
    /// The player does not have enough energy/mana/resources to play the card.
    /// </summary>
    INSUFFICIENT_ENERGY = 8,

    /// <summary>
    /// The target specified for the card is invalid or not allowed.
    /// </summary>
    INVALID_TARGET = 9,

    /// <summary>
    /// The card cannot be played for a general reason.
    /// </summary>
    CARD_CANNOT_BE_PLAYED = 10,

    /// <summary>The requested ability or spell has not been unlocked by the player.</summary>
    ABILITY_LOCKED = 11,

    /// <summary>The player's class does not meet an ability or spell requirement.</summary>
    CLASS_REQUIREMENT_NOT_MET = 12,

    /// <summary>The player's level does not meet an ability or spell requirement.</summary>
    LEVEL_REQUIREMENT_NOT_MET = 13
}
