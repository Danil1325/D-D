namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Applies the base physical-damage and block rules for a single hit.
/// Future combat modifiers belong in dedicated extensions of this calculation,
/// not in battle-state mutation.
/// </summary>
public sealed class DamageCalculator : IDamageCalculator
{
    public DamageResult Calculate(int attack, int defense, int block)
    {
        var baseDamage = Math.Max(1, attack - defense);
        var availableBlock = Math.Max(0, block);
        var blockedDamage = Math.Min(baseDamage, availableBlock);
        var finalDamage = baseDamage - blockedDamage;
        var remainingBlock = availableBlock - blockedDamage;

        return new DamageResult(baseDamage, blockedDamage, finalDamage, remainingBlock);
    }
}
