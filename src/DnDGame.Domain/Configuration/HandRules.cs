namespace DnDGame.Domain.Configuration;

/// <summary>
/// Configurable rules for hand management during gameplay.
/// These rules define constraints on hand size and should not be hardcoded in the engine.
/// </summary>
public class HandRules
{
    /// <summary>
    /// The maximum number of cards allowed in a player's hand.
    /// </summary>
    public int MaxHandSize { get; set; }

    /// <summary>
    /// Creates a new instance of HandRules with the specified configuration.
    /// </summary>
    /// <param name="maxHandSize">The maximum number of cards allowed in a hand.</param>
    public HandRules(int maxHandSize)
    {
        MaxHandSize = maxHandSize;
    }

    /// <summary>
    /// Parameterless constructor for dependency injection/serialization purposes.
    /// </summary>
    public HandRules()
    {
        MaxHandSize = 10; // Default value
    }

    /// <summary>
    /// Determines whether a hand can accept more cards.
    /// </summary>
    /// <param name="currentHandSize">The current number of cards in the hand.</param>
    /// <returns>True if the hand can accept more cards, false if it's full.</returns>
    public bool CanAddCard(int currentHandSize)
    {
        return currentHandSize < MaxHandSize;
    }

    /// <summary>
    /// Determines whether a hand can accept a specific number of cards.
    /// </summary>
    /// <param name="currentHandSize">The current number of cards in the hand.</param>
    /// <param name="cardsToAdd">The number of cards intended to add.</param>
    /// <returns>True if the hand can accept all cards, false if adding them would exceed max size.</returns>
    public bool CanAddCards(int currentHandSize, int cardsToAdd)
    {
        return (currentHandSize + cardsToAdd) <= MaxHandSize;
    }
}
