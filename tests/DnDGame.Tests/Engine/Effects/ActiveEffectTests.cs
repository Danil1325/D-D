using DnDGame.Domain.Engine.Effects;
using DnDGame.Domain.Engine.Enums;
using Xunit;

namespace DnDGame.Tests.Engine.Effects;

public class ActiveEffectTests
{
    [Fact]
    public void BurnEffect_StoresItsEffectData()
    {
        var effect = new BurnEffect(value: 3, duration: 2, target: EffectTarget.Enemy, stackCount: 4);

        Assert.Equal(BurnEffect.EffectType, effect.Type);
        Assert.Equal(3, effect.Value);
        Assert.Equal(2, effect.Duration);
        Assert.Equal(4, effect.StackCount);
        Assert.Equal(EffectTarget.Enemy, effect.Target);
        Assert.False(effect.IsExpired);
    }

    [Fact]
    public void PoisonEffect_CanBeConsumedByATurnEngineIntegration()
    {
        var effect = new PoisonEffect(value: 2, duration: 1, target: EffectTarget.Player);

        effect.ConsumeDuration();

        Assert.True(effect.IsExpired);
        Assert.Equal(0, effect.Duration);
    }
}
