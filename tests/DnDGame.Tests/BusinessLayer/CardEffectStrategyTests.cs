using DnDGame.BusinessLayer.Effects;
using DnDGame.BusinessLayer.Effects.Interfaces;
using DnDGame.BusinessLayer.Effects.Strategies;
using DnDGame.BusinessLayer.Engines;
using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using Xunit;

namespace DnDGame.Tests.BusinessLayer;

public class CardEffectStrategyTests
{
    [Fact]
    public void DamageEffect_AppliesCalculatedDamage()
    {
        var context = Context(baseDamage: 6, targetType: TargetType.SingleEnemy, target: new Target(99), enemyHealth: 20);
        var effect = new DamageEffect(new FixedDamageCalculator(7));

        var description = effect.Apply(context);

        Assert.Equal(13, context.Battle.EnemyCurrentHealth);
        Assert.Contains("Dealt 7 damage", description);
    }

    [Fact]
    public void HealEffect_CapsHealthAtMaximum()
    {
        var context = Context(baseDamage: 8, targetType: TargetType.Self, playerHealth: 18, playerMaxHealth: 20);

        var description = new HealEffect().Apply(context);

        Assert.Equal(20, context.Battle.PlayerCurrentHealth);
        Assert.Contains("Healed self for 2 HP", description);
    }

    [Fact]
    public void DrawEffect_DrawsOnlyAvailableHandSpaceWithoutDuplicateCards()
    {
        var drawCard = Instance(2, "Draw target");
        var context = Context(baseDamage: 3, targetType: TargetType.Self, hand: [Instance(10, "Held")], draw: [drawCard]);
        context.HandEngine = new HandEngine(new HandRules(2));
        context.DeckEngine = new DeckEngine();
        context.HandRules = new HandRules(2);

        var description = new DrawEffect().Apply(context);

        Assert.Equal(2, context.PlayerBattleDeck.Hand.Count);
        Assert.Single(context.PlayerBattleDeck.Hand, card => card.InstanceId == drawCard.InstanceId);
        Assert.Empty(context.PlayerBattleDeck.DrawPile);
        Assert.Contains("Drew 1 card(s)", description);
    }

    [Fact]
    public void DamageEffect_ReducesBlockBeforeHealth()
    {
        var context = Context(baseDamage: 10, targetType: TargetType.SingleEnemy, target: new Target(1), enemyHealth: 20, enemyBlock: 4);
        var effect = new DamageEffect(new FixedDamageCalculator(10));

        var description = effect.Apply(context);

        Assert.Equal(10, context.Battle.EnemyCurrentHealth);
        Assert.Equal(0, context.Battle.EnemyBlock);
        Assert.Contains("Dealt 10 damage", description);
    }

    [Fact]
    public void DamageEffect_ClampsEnemyHealthAtZero()
    {
        var context = Context(baseDamage: 15, targetType: TargetType.SingleEnemy, target: new Target(1), enemyHealth: 5);
        var effect = new DamageEffect(new FixedDamageCalculator(15));

        var description = effect.Apply(context);

        Assert.Equal(0, context.Battle.EnemyCurrentHealth);
        Assert.Contains("Dealt 15 damage", description);
    }

    [Fact]
    public void DamageEffect_CanApplyIsFalseWithoutPositiveDamage()
    {
        var context = Context(baseDamage: 0, targetType: TargetType.SingleEnemy, target: new Target(1));
        var effect = new DamageEffect(new FixedDamageCalculator(7));

        Assert.False(effect.CanApply(context));
        Assert.Contains("Cannot apply", effect.Apply(context));
    }

    [Fact]
    public void DamageEffect_CanApplyIsFalseForSingleTargetWithoutTarget()
    {
        var context = Context(baseDamage: 5, targetType: TargetType.SingleEnemy, target: null);

        var canApply = new DamageEffect(new FixedDamageCalculator(7)).CanApply(context);

        Assert.False(canApply);
    }

    [Fact]
    public void HealEffect_DoesNotOverhealWhenAlreadyAtMaximumHealth()
    {
        var context = Context(baseDamage: 8, targetType: TargetType.Self, playerHealth: 20, playerMaxHealth: 20);

        var description = new HealEffect().Apply(context);

        Assert.Equal(20, context.Battle.PlayerCurrentHealth);
        Assert.Contains("No healing applied", description);
    }

    [Fact]
    public void HealEffect_CanApplyIsFalseWithoutPositiveHealValue()
    {
        var context = Context(baseDamage: 0, targetType: TargetType.Self, playerHealth: 10, playerMaxHealth: 20);

        Assert.False(new HealEffect().CanApply(context));
    }

    [Fact]
    public void DrawEffect_CanApplyIsFalseWhenHandIsFull()
    {
        var context = Context(baseDamage: 1, targetType: TargetType.Self, hand: [Instance(10, "Held"), Instance(11, "Held 2")]);
        context.HandEngine = new HandEngine(new HandRules(2));
        context.DeckEngine = new DeckEngine();
        context.HandRules = new HandRules(2);

        Assert.False(new DrawEffect().CanApply(context));
    }

    [Fact]
    public void DrawEffect_CanApplyIsFalseWhenNoCardsAreAvailable()
    {
        var context = Context(baseDamage: 1, targetType: TargetType.Self);
        context.HandEngine = new HandEngine(new HandRules(5));
        context.DeckEngine = new DeckEngine();
        context.HandRules = new HandRules(5);

        Assert.False(new DrawEffect().CanApply(context));
    }

    [Fact]
    public void DrawEffect_CanApplyIsFalseWithoutRequiredEngines()
    {
        var context = Context(baseDamage: 1, targetType: TargetType.Self, draw: [Instance(3, "Available")]);

        Assert.False(new DrawEffect().CanApply(context));
    }

    [Fact]
    public void DrawEffect_DrawsOnlyUpToAvailableHandSpace()
    {
        var drawCards = new List<CardInstance> { Instance(2, "Drawable 1"), Instance(3, "Drawable 2"), Instance(4, "Drawable 3") };
        var context = Context(baseDamage: 5, targetType: TargetType.Self, hand: [Instance(10, "Held")], draw: drawCards);
        context.HandEngine = new HandEngine(new HandRules(3));
        context.DeckEngine = new DeckEngine();
        context.HandRules = new HandRules(3);

        var description = new DrawEffect().Apply(context);

        Assert.Equal(3, context.PlayerBattleDeck.Hand.Count);
        Assert.Single(context.PlayerBattleDeck.DrawPile);
        Assert.Contains("Drew 2 card(s)", description);
    }

    private static CardEffectContext Context(
        int baseDamage,
        TargetType targetType,
        ICardTarget? target = null,
        int enemyHealth = 0,
        int enemyBlock = 0,
        int playerHealth = 0,
        int playerMaxHealth = 0,
        IList<CardInstance>? hand = null,
        IList<CardInstance>? draw = null)
    {
        var card = new Card { Id = 1, BaseDamage = baseDamage, TargetType = targetType };
        var instance = new CardInstance { CardId = card.Id, Card = card };
        var battle = new Battle
        {
            EnemyCurrentHealth = enemyHealth,
            EnemyBlock = enemyBlock,
            PlayerCurrentHealth = playerHealth,
            PlayerMaxHealth = playerMaxHealth
        };
        return new CardEffectContext(
            battle,
            instance,
            new BattleDeck { Hand = hand ?? new List<CardInstance>(), DrawPile = draw ?? new List<CardInstance>() },
            playerId: 1,
            card,
            target);
    }

    private static CardInstance Instance(int id, string name) => new()
    {
        CardId = id,
        Card = new Card { Id = id, Name = name }
    };

    private sealed class Target(int targetId) : ICardTarget
    {
        public int TargetId { get; } = targetId;
        public string TargetType => "Enemy";
    }

    private sealed class FixedDamageCalculator(int result) : IDamageCalculator
    {
        public int CalculateDamage(int baseDamage, int playerStrength, int enemyDefense, int enemyBlock) => result;
    }
}
