using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;

namespace DnDGame.Domain.Engine.Effects;

/// <summary>
/// Common base for temporary buffs and debuffs that modify a combat statistic.
/// Resolution is intentionally left to the turn effect engine.
/// </summary>
public abstract class StatModifierEffect : ActiveEffect
{
    protected StatModifierEffect(
        string type,
        int value,
        int duration,
        EffectTarget target,
        int stackCount = 1)
        : base(type, value, duration, stackCount, target)
    {
    }
}
