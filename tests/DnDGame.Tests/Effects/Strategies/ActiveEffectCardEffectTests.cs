using DnDGame.BusinessLayer.Effects;
using DnDGame.BusinessLayer.Effects.Interfaces;
using DnDGame.BusinessLayer.Effects.Strategies;
using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Enums;
using Xunit;
using ActiveEffect = DnDGame.Domain.Engine.Models.ActiveEffect;

namespace DnDGame.Tests.Effects.Strategies;

public class ActiveEffectCardEffectTests
{
    [Theory]
    [MemberData(nameof(StatusEffects))]
    public void Apply_CreatesTheExpectedActiveEffect(ICardEffect cardEffect, string type)
    {
        var activeEffects = new List<ActiveEffect>();
        var context = CreateContext(activeEffects);

        var result = cardEffect.Apply(context);

        var activeEffect = Assert.Single(activeEffects);
        Assert.Equal(type, activeEffect.Type);
        Assert.Equal(3, activeEffect.Value);
        Assert.Equal(2, activeEffect.Duration);
        Assert.Equal(EffectTarget.Player, activeEffect.Target);
        Assert.Contains(type, result);
    }

    public static IEnumerable<object[]> StatusEffects =>
    [
        [new StrengthCardEffect(2), "Strength"],
        [new WeakCardEffect(2), "Weak"],
        [new VulnerableCardEffect(2), "Vulnerable"],
        [new DefenseUpCardEffect(2), "DefenseUp"]
    ];

    [Fact]
    public void CanApply_IsFalseWhenNoActiveEffectDestinationIsProvided()
    {
        var context = CreateContext(activeEffects: null);

        Assert.False(new StrengthCardEffect().CanApply(context));
    }

    [Theory]
    [MemberData(nameof(ResolvedTargets))]
    public void Apply_ResolvesTargetByCardTargetType(TargetType targetType, EffectTarget expected)
    {
        var activeEffects = new List<ActiveEffect>();
        var context = CreateContext(activeEffects, targetType, target: new Target(5));

        var result = new StrengthCardEffect(duration: 1).Apply(context);

        var effect = Assert.Single(activeEffects);
        Assert.Equal(expected, effect.Target);
        Assert.Contains(expected.ToString(), result);
    }

    public static IEnumerable<object[]> ResolvedTargets =>
    [
        [TargetType.SingleEnemy, EffectTarget.Enemy],
        [TargetType.AllEnemies, EffectTarget.Enemy],
        [TargetType.Self, EffectTarget.Player],
        [TargetType.SingleAlly, EffectTarget.Player],
        [TargetType.AllAllies, EffectTarget.Player]
    ];

    [Fact]
    public void Apply_CreatesEffectThenConsumingDurationExpiresIt()
    {
        var activeEffects = new List<ActiveEffect>();
        var context = CreateContext(activeEffects);

        new StrengthCardEffect(duration: 1).Apply(context);

        var effect = Assert.Single(activeEffects);
        Assert.False(effect.IsExpired);

        effect.ConsumeDuration();

        Assert.True(effect.IsExpired);
        Assert.Equal(0, effect.Duration);
    }

    [Fact]
    public void CanApply_IsFalseWhenCardHasNoPositiveDamage()
    {
        var card = new Card { BaseDamage = 0, TargetType = TargetType.Self };
        var context = new CardEffectContext(
            new Battle(),
            new CardInstance { Card = card },
            new BattleDeck(),
            playerId: 1,
            cardDefinition: card,
            activeEffects: new List<ActiveEffect>());

        Assert.False(new StrengthCardEffect().CanApply(context));
    }

    [Fact]
    public void CanApply_IsFalseForSingleTargetCardsWithoutATarget()
    {
        var card = new Card { BaseDamage = 3, TargetType = TargetType.SingleEnemy };
        var context = new CardEffectContext(
            new Battle(),
            new CardInstance { Card = card },
            new BattleDeck(),
            playerId: 1,
            cardDefinition: card,
            activeEffects: new List<ActiveEffect>());

        Assert.False(new WeakCardEffect().CanApply(context));
    }

    [Fact]
    public void CanApply_IsFalseForUnsupportedTargetType()
    {
        var card = new Card { BaseDamage = 3, TargetType = TargetType.None };
        var context = new CardEffectContext(
            new Battle(),
            new CardInstance { Card = card },
            new BattleDeck(),
            playerId: 1,
            cardDefinition: card,
            activeEffects: new List<ActiveEffect>());

        Assert.False(new DefenseUpCardEffect().CanApply(context));
    }

    private static CardEffectContext CreateContext(
        ICollection<ActiveEffect>? activeEffects,
        TargetType targetType = TargetType.Self,
        ICardTarget? target = null)
    {
        var card = new Card
        {
            BaseDamage = 3,
            TargetType = targetType,
            EffectType = EffectType.Strength
        };

        return new CardEffectContext(
            new Battle(),
            new CardInstance { Card = card },
            new BattleDeck(),
            playerId: 1,
            cardDefinition: card,
            target,
            activeEffects: activeEffects);
    }

    private sealed class Target(int targetId) : ICardTarget
    {
        public int TargetId { get; } = targetId;
        public string TargetType => "Enemy";
    }
}
