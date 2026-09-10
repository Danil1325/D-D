using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Configurable MVP rule: base damage, strength, and any pre-rolled dice result
/// form raw damage; flat buffs and debuffs are then applied additively.
/// </summary>
public sealed class AdditiveDamageRule : IDamageRule
{
    public EngineResult<int> CalculateRawDamage(DamageRequest request)
    {
        var diceDamage = request.DiceResult?.FinalResult ?? 0;
        return EngineResult<int>.Ok(request.BaseDamage + request.Strength + diceDamage);
    }

    public EngineResult<int> ApplyModifiers(int rawDamage, DamageRequest request)
    {
        var buffs = request.BuffModifiers.Sum();
        var debuffs = request.DebuffModifiers.Sum();

        return EngineResult<int>.Ok(rawDamage + buffs - debuffs);
    }
}
