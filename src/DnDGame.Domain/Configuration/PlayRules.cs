namespace DnDGame.Domain.Configuration;

/// <summary>
/// Configurable rules for card play during battles.
/// These rules define game mechanics that should not be hardcoded in the engine.
/// </summary>
public class PlayRules
{
    /// <summary>
    /// The maximum energy pool available to a player per turn.
    /// </summary>
    public int MaxEnergyPerTurn { get; set; }

    /// <summary>
    /// Whether cards can be played without sufficient energy remaining.
    /// If false, all card plays must have enough energy available.
    /// </summary>
    public bool AllowOverdraft { get; set; }

    /// <summary>
    /// Creates a new instance of PlayRules with the specified configuration.
    /// </summary>
    /// <param name="maxEnergyPerTurn">The maximum energy pool per turn.</param>
    /// <param name="allowOverdraft">Whether to allow playing cards without sufficient energy.</param>
    public PlayRules(int maxEnergyPerTurn, bool allowOverdraft = false)
    {
        MaxEnergyPerTurn = maxEnergyPerTurn;
        AllowOverdraft = allowOverdraft;
    }

    /// <summary>
    /// Parameterless constructor for dependency injection/serialization purposes.
    /// </summary>
    public PlayRules()
    {
        MaxEnergyPerTurn = 5;
        AllowOverdraft = false;
    }

    /// <summary>
    /// Determines whether a player has enough energy to play a card.
    /// </summary>
    /// <param name="currentEnergy">The current energy pool of the player.</param>
    /// <param name="cardCost">The cost to play the card.</param>
    /// <returns>True if the player can afford the card, false otherwise.</returns>
    public bool CanAffordCard(int currentEnergy, int cardCost)
    {
        if (AllowOverdraft)
        {
            return true; // Can always play if overdraft is allowed
        }

        return currentEnergy >= cardCost;
    }

    /// <summary>
    /// Calculates the remaining energy after playing a card.
    /// </summary>
    /// <param name="currentEnergy">The current energy pool.</param>
    /// <param name="cardCost">The cost to play the card.</param>
    /// <returns>The remaining energy after the card is played.</returns>
    public int CalculateRemainingEnergy(int currentEnergy, int cardCost)
    {
        int remaining = currentEnergy - cardCost;
        
        // Don't allow negative energy unless overdraft is enabled
        if (!AllowOverdraft && remaining < 0)
        {
            return 0;
        }

        return remaining;
    }
}
