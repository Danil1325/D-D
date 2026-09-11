using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Effects;

/// <summary>Temporary negative modifier to the target's outgoing damage.</summary>
public sealed class WeakEffect : StatModifierEffect
{
    public const string EffectType = "Weak";

    public WeakEffect(int value, int duration, EffectTarget target, int stackCount = 1)
        : base(EffectType, value, duration, target, stackCount)
    {
    }
}
