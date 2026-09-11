using DnDGame.Domain.Engine.Effects;

namespace DnDGame.BusinessLayer.Effects.Strategies;

public sealed class DefenseUpCardEffect : ActiveEffectCardEffect<DefenseUpEffect>
{
    public DefenseUpCardEffect(int duration = 1)
        : base(DefenseUpEffect.EffectType, duration, (value, turns, target) => new DefenseUpEffect(value, turns, target)) { }
}
