namespace DnDGame.BusinessLayer.Effects.Interfaces;

/// <summary>
/// Contract for calculating damage in combat.
/// Implementation provided by Persoana 1 with full combat rules.
/// </summary>
public interface IDamageCalculator
{
    /// <summary>
    /// Calculates final damage considering all modifiers, defense, and block.
    /// This interface allows effects to delegate damage calculation to a configurable strategy.
    /// </summary>
    /// <param name="baseDamage">The base damage value from the card</param>
    /// <param name="playerStrength">The attacker's strength attribute</param>
    /// <param name="enemyDefense">The defender's defense attribute</param>
    /// <param name="enemyBlock">The defender's temporary block/shield</param>
    /// <returns>The final damage to apply, after all calculations</returns>
    int CalculateDamage(int baseDamage, int playerStrength, int enemyDefense, int enemyBlock);
}
