using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Effects;

/// <summary>Temporary positive modifier to the target's defense.</summary>
public sealed class DefenseUpEffect : StatModifierEffect
{
    public const string EffectType = "DefenseUp";

    public DefenseUpEffect(int value, int duration, EffectTarget target, int stackCount = 1)
        : base(EffectType, value, duration, target, stackCount)
    {
    }
}
