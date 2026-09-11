using DnDGame.Domain.Engine.Effects;

namespace DnDGame.BusinessLayer.Effects.Strategies;

public sealed class VulnerableCardEffect : ActiveEffectCardEffect<VulnerableEffect>
{
    public VulnerableCardEffect(int duration = 1)
        : base(VulnerableEffect.EffectType, duration, (value, turns, target) => new VulnerableEffect(value, turns, target)) { }
}
