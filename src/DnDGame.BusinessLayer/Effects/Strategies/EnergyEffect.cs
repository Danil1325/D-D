namespace DnDGame.BusinessLayer.Effects.Strategies;

using DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Effect that modifies player energy during battle.
/// Can increase or decrease current energy pool.
/// </summary>
public class EnergyEffect : ICardEffect
{
    public string EffectName => "Energy";

    /// <summary>
    /// Energy effect can apply if:
    /// - Card has an energy value (using BaseDamage as energy amount)
    /// </summary>
    public bool CanApply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Energy effect always can apply (can be positive or negative)
        // No special conditions needed
        return true;
    }

    /// <summary>
    /// Modifies the player's current energy pool.
    /// Positive values increase energy, negative values decrease it.
    /// Energy is capped at 0 minimum and MaxPlayerEnergyPerTurn maximum.
    /// </summary>
    public string Apply(CardEffectContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!CanApply(context))
        {
            return "Cannot apply energy effect.";
        }

        // Use BaseDamage as energy modifier (can be negative)
        int energyModifier = context.EffectiveCardDamage;
        int previousEnergy = context.Battle.CurrentPlayerEnergy;
        int newEnergy = previousEnergy + energyModifier;

        // Cap energy at minimum 0 and maximum MaxPlayerEnergyPerTurn
        if (newEnergy < 0)
        {
            newEnergy = 0;
        }

        if (newEnergy > context.Battle.MaxPlayerEnergyPerTurn)
        {
            newEnergy = context.Battle.MaxPlayerEnergyPerTurn;
        }

        int actualChange = newEnergy - previousEnergy;
        context.Battle.CurrentPlayerEnergy = newEnergy;

        if (actualChange == 0)
        {
            return "Energy remains unchanged.";
        }
        else if (actualChange > 0)
        {
            return $"Gained {actualChange} energy (current: {newEnergy}/{context.Battle.MaxPlayerEnergyPerTurn}).";
        }
        else
        {
            return $"Lost {Math.Abs(actualChange)} energy (current: {newEnergy}/{context.Battle.MaxPlayerEnergyPerTurn}).";
        }
    }
}
