using DnDGame.BusinessLayer.Effects.Interfaces;

namespace DnDGame.BusinessLayer.Effects;

/// <summary>
/// Same MVP/additive formula as the Domain-side combat calculator
/// (<see cref="DnDGame.Domain.Engine.Combat.DamageCalculator"/> +
/// <see cref="DnDGame.Domain.Engine.Combat.AdditiveDamageRule"/>), adapted to this
/// interface's flat-int signature: base damage plus strength forms raw damage,
/// defense reduces it (floor 0), then block absorbs what's left (floor 0).
/// </summary>
public sealed class AdditiveDamageCalculator : IDamageCalculator
{
    public int CalculateDamage(int baseDamage, int playerStrength, int enemyDefense, int enemyBlock)
    {
        var rawDamage = baseDamage + playerStrength;
        var damageAfterDefense = Math.Max(0, rawDamage - Math.Max(0, enemyDefense));
        var availableBlock = Math.Max(0, enemyBlock);
        var blockedDamage = Math.Min(damageAfterDefense, availableBlock);

        return damageAfterDefense - blockedDamage;
    }
}
