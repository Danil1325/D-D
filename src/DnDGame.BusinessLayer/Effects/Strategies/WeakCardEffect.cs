using DnDGame.Domain.Engine.Effects;

namespace DnDGame.BusinessLayer.Effects.Strategies;

public sealed class WeakCardEffect : ActiveEffectCardEffect<WeakEffect>
{
    public WeakCardEffect(int duration = 1)
        : base(WeakEffect.EffectType, duration, (value, turns, target) => new WeakEffect(value, turns, target)) { }
}
