using DnDGame.BusinessLayer.Effects;
using DnDGame.BusinessLayer.Effects.Interfaces;
using DnDGame.BusinessLayer.Effects.Strategies;
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

    private static CardEffectContext CreateContext(ICollection<ActiveEffect>? activeEffects)
    {
        var card = new Card
        {
            BaseDamage = 3,
            TargetType = TargetType.Self,
            EffectType = EffectType.Strength
        };

        return new CardEffectContext(
            new Battle(),
            new CardInstance { Card = card },
            new BattleDeck(),
            playerId: 1,
            cardDefinition: card,
            activeEffects: activeEffects);
    }
}
