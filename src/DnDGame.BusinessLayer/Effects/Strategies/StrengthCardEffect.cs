using DnDGame.Domain.Engine.Effects;

namespace DnDGame.BusinessLayer.Effects.Strategies;

public sealed class StrengthCardEffect : ActiveEffectCardEffect<StrengthEffect>
{
    public StrengthCardEffect(int duration = 1)
        : base(StrengthEffect.EffectType, duration, (value, turns, target) => new StrengthEffect(value, turns, target)) { }
}
