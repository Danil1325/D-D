using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Effects;

/// <summary>Temporary positive modifier to the target's strength.</summary>
public sealed class StrengthEffect : StatModifierEffect
{
    public const string EffectType = "Strength";

    public StrengthEffect(int value, int duration, EffectTarget target, int stackCount = 1)
        : base(EffectType, value, duration, target, stackCount)
    {
    }
}
