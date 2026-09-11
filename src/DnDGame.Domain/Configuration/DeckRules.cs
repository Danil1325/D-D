namespace DnDGame.Domain.Configuration;

/// <summary>
/// Configurable rules for deck building and validation.
/// These rules define constraints on deck composition and should not be hardcoded in the engine.
/// </summary>
public class DeckRules
{
    /// <summary>
    /// The minimum number of cards required in a deck.
    /// </summary>
    public int MinimumDeckSize { get; set; }

    /// <summary>
    /// The maximum number of cards allowed in a deck.
    /// </summary>
    public int MaximumDeckSize { get; set; }

    /// <summary>
    /// The maximum number of copies of a single card allowed in a deck.
    /// </summary>
    public int MaximumCopiesPerCard { get; set; }

    /// <summary>
    /// Creates a new instance of DeckRules with the specified configuration.
    /// </summary>
    /// <param name="minimumDeckSize">The minimum number of cards in a deck.</param>
    /// <param name="maximumDeckSize">The maximum number of cards in a deck.</param>
    /// <param name="maximumCopiesPerCard">The maximum copies of a single card allowed.</param>
    public DeckRules(int minimumDeckSize, int maximumDeckSize, int maximumCopiesPerCard)
    {
        MinimumDeckSize = minimumDeckSize;
        MaximumDeckSize = maximumDeckSize;
        MaximumCopiesPerCard = maximumCopiesPerCard;
    }

    /// <summary>
    /// Parameterless constructor for dependency injection/serialization purposes.
    /// </summary>
    public DeckRules()
    {
    }

    /// <summary>
    /// Validates a deck's card composition against these rules.
    /// </summary>
    /// <param name="deckSize">The total number of cards in the deck.</param>
    /// <param name="cardCounts">A dictionary where keys are card IDs and values are the count of each card in the deck.</param>
    /// <returns>A tuple containing (isValid, errorMessage). errorMessage is null if valid.</returns>
    public (bool IsValid, string? ErrorMessage) ValidateDeck(int deckSize, Dictionary<int, int> cardCounts)
    {
        // Validate deck size
        if (deckSize < MinimumDeckSize)
        {
            return (false, $"Deck size {deckSize} is below the minimum of {MinimumDeckSize}.");
        }

        if (deckSize > MaximumDeckSize)
        {
            return (false, $"Deck size {deckSize} exceeds the maximum of {MaximumDeckSize}.");
        }

        // Validate card copies
        foreach (var (cardId, count) in cardCounts)
        {
            if (count > MaximumCopiesPerCard)
            {
                return (false, $"Card ID {cardId} appears {count} times, exceeding the maximum of {MaximumCopiesPerCard} copies per card.");
            }
        }

        return (true, null);
    }
}
