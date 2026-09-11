using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;

namespace DnDGame.Domain.Engine.Effects;

/// <summary>
/// Damage-over-time effect whose value is applied once for each stack when an
/// effect engine resolves it.
/// </summary>
public sealed class PoisonEffect : ActiveEffect
{
    public const string EffectType = "Poison";

    public PoisonEffect(int value, int duration, EffectTarget target, int stackCount = 1)
        : base(EffectType, value, duration, stackCount, target)
    {
    }
}
