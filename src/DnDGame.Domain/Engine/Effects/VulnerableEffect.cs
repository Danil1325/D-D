using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Effects;

/// <summary>Temporary modifier that increases damage received by the target.</summary>
public sealed class VulnerableEffect : StatModifierEffect
{
    public const string EffectType = "Vulnerable";

    public VulnerableEffect(int value, int duration, EffectTarget target, int stackCount = 1)
        : base(EffectType, value, duration, target, stackCount)
    {
    }
}
