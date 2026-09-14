using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Cards;
using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Effects;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using DomainEnums = DnDGame.Domain.Enums;

namespace DnDGame.Tests.Engine.Cards;

public class CardEngineTests
{
    private readonly ICardEngine _cardEngine = new CardEngine(new DamageCalculator(new AdditiveDamageRule()));

    [Fact]
    public void PlayCardConsumesEnergyAndMovesTheCardToTheDiscardPile()
    {
        var card = DamageCard(damage: 5, cost: 3);
        var context = Context(card, playerEnergy: 5, enemyHealth: 20);

        var result = _cardEngine.PlayCard(context, card);

        Assert.True(result.Success);
        Assert.Equal(2, context.BattleState.PlayerEnergy);
        Assert.Empty(context.BattleState.Hand);
        Assert.Contains(card, context.BattleState.DiscardPile);
    }

    [Fact]
    public void PlayCardDealsDamageToTheEnemy()
    {
        var card = DamageCard(damage: 5, cost: 1);
        var context = Context(card, playerEnergy: 3, enemyHealth: 20);

        _cardEngine.PlayCard(context, card);

        Assert.Equal(15, context.BattleState.EnemyHealth);
    }

    [Fact]
    public void PlayCardDamageRespectsEnemyBlock()
    {
        var card = DamageCard(damage: 5, cost: 1);
        var context = Context(card, playerEnergy: 3, enemyHealth: 20);
        context.BattleState.EnemyBlock = 2;

        _cardEngine.PlayCard(context, card);

        Assert.Equal(17, context.BattleState.EnemyHealth);
        Assert.Equal(0, context.BattleState.EnemyBlock);
    }

    [Fact]
    public void PlayCardFailsWhenTheCardIsNotInHand()
    {
        var card = DamageCard(damage: 5, cost: 1);
        var context = Context(card, playerEnergy: 3, enemyHealth: 20);
        context.BattleState.Hand.Clear();

        var result = _cardEngine.PlayCard(context, card);

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.InvalidAction, result.ErrorCode);
        Assert.Equal(3, context.BattleState.PlayerEnergy);
    }

    [Fact]
    public void PlayCardFailsWhenEnergyIsInsufficient()
    {
        var card = DamageCard(damage: 5, cost: 4);
        var context = Context(card, playerEnergy: 3, enemyHealth: 20);

        var result = _cardEngine.PlayCard(context, card);

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.InvalidAction, result.ErrorCode);
        Assert.Equal(3, context.BattleState.PlayerEnergy);
        Assert.Contains(card, context.BattleState.Hand);
    }

    [Fact]
    public void PlayCardFailsWhenTheBattleHasFinished()
    {
        var card = DamageCard(damage: 5, cost: 1);
        var context = Context(card, playerEnergy: 3, enemyHealth: 20);
        context.BattleState.BattleStatus = BattleStatus.Victory;

        var result = _cardEngine.PlayCard(context, card);

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.BattleAlreadyFinished, result.ErrorCode);
    }

    [Fact]
    public void PlayCardFailsWhenItIsNotThePlayerTurn()
    {
        var card = DamageCard(damage: 5, cost: 1);
        var context = Context(card, playerEnergy: 3, enemyHealth: 20);
        context.BattleState.CurrentTurn = TurnType.Enemy;
        context.BattleState.BattleStatus = BattleStatus.EnemyTurn;

        var result = _cardEngine.PlayCard(context, card);

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.NotPlayerTurn, result.ErrorCode);
    }

    [Fact]
    public void PlayCardFailsWhenTheCardDefinitionIsMissing()
    {
        var orphan = new CardInstance { CardId = 99, Card = null! };
        var context = Context(orphan, playerEnergy: 3, enemyHealth: 20);

        var result = _cardEngine.PlayCard(context, orphan);

        Assert.False(result.Success);
        Assert.Equal(EngineErrorCodes.InvalidAction, result.ErrorCode);
    }

    [Fact]
    public void PlayCardCostIncludesTemporaryCostModifiers()
    {
        var card = DamageCard(damage: 5, cost: 3);
        card.TemporaryCostModifier = -1;
        var context = Context(card, playerEnergy: 5, enemyHealth: 20);

        _cardEngine.PlayCard(context, card);

        Assert.Equal(3, context.BattleState.PlayerEnergy); // 5 - (3 - 1)
    }

    [Fact]
    public void HealingCardRestoresPlayerHealth()
    {
        var card = HealingCard(heal: 5, cost: 1);
        var context = Context(card, playerEnergy: 3, playerHealth: 8, playerMaxHealth: 20, enemyHealth: 20);

        _cardEngine.PlayCard(context, card);

        Assert.Equal(13, context.BattleState.PlayerHealth);
    }

    [Fact]
    public void HealingCardCannotExceedMaximumHealth()
    {
        var card = HealingCard(heal: 30, cost: 1);
        var context = Context(card, playerEnergy: 3, playerHealth: 15, playerMaxHealth: 20, enemyHealth: 20);

        _cardEngine.PlayCard(context, card);

        Assert.Equal(20, context.BattleState.PlayerHealth);
    }

    [Fact]
    public void StrengthCardAppliesAStrengthEffectToThePlayer()
    {
        var card = EffectCard(DomainEnums.EffectType.Strength, value: 2, targetType: DomainEnums.TargetType.Self);
        var context = Context(card, playerEnergy: 3);

        _cardEngine.PlayCard(context, card);

        var effect = Assert.Single(context.BattleState.ActiveEffects);
        Assert.IsType<StrengthEffect>(effect);
        Assert.Equal(EffectTarget.Player, effect.Target);
        Assert.Equal(2, effect.Value);
        Assert.Equal(CardEngine.DefaultEffectDuration, effect.Duration);
    }

    [Fact]
    public void WeakCardAppliesAWeakEffectToTheEnemy()
    {
        var card = EffectCard(DomainEnums.EffectType.Weak, value: 2, targetType: DomainEnums.TargetType.SingleEnemy);
        var context = Context(card, playerEnergy: 3);

        _cardEngine.PlayCard(context, card);

        var effect = Assert.Single(context.BattleState.ActiveEffects);
        Assert.IsType<WeakEffect>(effect);
        Assert.Equal(EffectTarget.Enemy, effect.Target);
    }

    [Fact]
    public void VulnerableCardAppliesAVulnerableEffectToTheEnemy()
    {
        var card = EffectCard(DomainEnums.EffectType.Vulnerable, value: 2, targetType: DomainEnums.TargetType.SingleEnemy);
        var context = Context(card, playerEnergy: 3);

        _cardEngine.PlayCard(context, card);

        var effect = Assert.Single(context.BattleState.ActiveEffects);
        Assert.IsType<VulnerableEffect>(effect);
        Assert.Equal(EffectTarget.Enemy, effect.Target);
    }

    [Fact]
    public void PoisonStatusCardAppliesAPoisonEffectToTheEnemy()
    {
        var card = EffectCard(DomainEnums.EffectType.StatusEffect, value: 0, targetType: DomainEnums.TargetType.SingleEnemy, active: "Venom Coil");
        var context = Context(card, playerEnergy: 3);

        _cardEngine.PlayCard(context, card);

        var effect = Assert.Single(context.BattleState.ActiveEffects);
        Assert.IsType<PoisonEffect>(effect);
        Assert.Equal(EffectTarget.Enemy, effect.Target);
    }

    [Fact]
    public void BurnStatusCardAppliesABurnEffectToTheEnemy()
    {
        var card = EffectCard(DomainEnums.EffectType.StatusEffect, value: 0, targetType: DomainEnums.TargetType.SingleEnemy, active: "Burning Soul");
        var context = Context(card, playerEnergy: 3);

        _cardEngine.PlayCard(context, card);

        var effect = Assert.Single(context.BattleState.ActiveEffects);
        Assert.IsType<BurnEffect>(effect);
        Assert.Equal(EffectTarget.Enemy, effect.Target);
    }

    [Fact]
    public void ProtectCardAddsPlayerBlock()
    {
        var card = EffectCard(DomainEnums.EffectType.Protect, value: 4, targetType: DomainEnums.TargetType.Self);
        var context = Context(card, playerEnergy: 3);

        _cardEngine.PlayCard(context, card);

        Assert.Equal(4, context.BattleState.PlayerBlock);
    }

    [Fact]
    public void DrawCardDrawsFromTheDrawPileIntoTheHand()
    {
        var card = EffectCard(DomainEnums.EffectType.Draw, value: 2, targetType: DomainEnums.TargetType.Self);
        var context = Context(card, playerEnergy: 3);
        context.BattleState.DrawPile = new List<CardInstance>
        {
            DamageCard(damage: 1, cost: 0),
            DamageCard(damage: 1, cost: 0)
        };

        _cardEngine.PlayCard(context, card);

        Assert.Equal(2, context.BattleState.Hand.Count);
        Assert.Empty(context.BattleState.DrawPile);
    }

    [Fact]
    public void DrawCardReshufflesTheDiscardPileWhenTheDrawPileIsEmpty()
    {
        var card = EffectCard(DomainEnums.EffectType.Draw, value: 2, targetType: DomainEnums.TargetType.Self);
        var context = Context(card, playerEnergy: 3);
        context.BattleState.DrawPile = new List<CardInstance>();
        context.BattleState.DiscardPile = new List<CardInstance>
        {
            DamageCard(damage: 1, cost: 0),
            DamageCard(damage: 1, cost: 0)
        };

        _cardEngine.PlayCard(context, card);

        Assert.Equal(2, context.BattleState.Hand.Count);
        Assert.Single(context.BattleState.DrawPile); // 3 reshuffled, 2 drawn
        Assert.Empty(context.BattleState.DiscardPile);
    }

    [Fact]
    public void DiscardCardDiscardsAdditionalCardsFromTheHand()
    {
        var played = EffectCard(DomainEnums.EffectType.Discard, value: 2, targetType: DomainEnums.TargetType.Self);
        var context = Context(played, playerEnergy: 3);
        context.BattleState.Hand.Add(DamageCard(damage: 1, cost: 0));
        context.BattleState.Hand.Add(DamageCard(damage: 1, cost: 0));

        _cardEngine.PlayCard(context, played);

        Assert.Empty(context.BattleState.Hand);
        Assert.Equal(3, context.BattleState.DiscardPile.Count);
    }

    [Fact]
    public void PlayCardWritesToTheBattleLog()
    {
        var card = DamageCard(damage: 5, cost: 1);
        var context = Context(card, playerEnergy: 3, enemyHealth: 20);

        _cardEngine.PlayCard(context, card);

        var entry = Assert.Single(context.BattleState.BattleLog);
        Assert.Equal("Player", entry.Actor);
        Assert.Equal("Played", entry.Action);
        Assert.Equal(card.Card.Name, entry.Card);
    }

    [Fact]
    public void PlayCardThrowsOnNullArguments()
    {
        Assert.Throws<ArgumentNullException>(() => _cardEngine.PlayCard(null!, DamageCard(damage: 1, cost: 1)));
        Assert.Throws<ArgumentNullException>(() => _cardEngine.PlayCard(Context(DamageCard(damage: 1, cost: 1), playerEnergy: 1), null!));
    }

    private static BattleContext Context(
        CardInstance card,
        int playerEnergy,
        int playerHealth = 10,
        int playerMaxHealth = 10,
        int enemyHealth = 10)
    {
        return new BattleContext(
            new PlayerCharacter { CurrentHealth = playerHealth, MaxHealth = playerMaxHealth },
            new Enemy { Health = enemyHealth, Defense = 8, DamageAmount = 3 },
            new BattleState
            {
                PlayerHealth = playerHealth,
                PlayerMaxHealth = playerMaxHealth,
                PlayerEnergy = playerEnergy,
                PlayerMaxEnergy = playerEnergy,
                EnemyHealth = enemyHealth,
                EnemyMaxHealth = enemyHealth,
                CurrentTurn = TurnType.Player,
                BattleStatus = BattleStatus.PlayerTurn,
                Hand = new List<CardInstance> { card }
            });
    }

    private static CardInstance DamageCard(int damage, int cost) => new()
    {
        InstanceId = Guid.NewGuid(),
        CardId = 1,
        Card = new Card
        {
            Id = 1,
            Name = "Attack",
            BaseCost = cost,
            BaseDamage = damage,
            EffectType = DomainEnums.EffectType.Damage,
            TargetType = DomainEnums.TargetType.None
        }
    };

    private static CardInstance HealingCard(int heal, int cost) => new()
    {
        InstanceId = Guid.NewGuid(),
        CardId = 2,
        Card = new Card
        {
            Id = 2,
            Name = "Bandage",
            BaseCost = cost,
            BaseDamage = heal,
            EffectType = DomainEnums.EffectType.Healing,
            TargetType = DomainEnums.TargetType.Self
        }
    };

    private static CardInstance EffectCard(
        DomainEnums.EffectType effectType,
        int value,
        DomainEnums.TargetType targetType,
        string active = "") => new()
    {
        InstanceId = Guid.NewGuid(),
        CardId = 3,
        Card = new Card
        {
            Id = 3,
            Name = effectType.ToString(),
            BaseCost = 1,
            BaseDamage = value,
            EffectType = effectType,
            TargetType = targetType,
            ActiveEffect = active
        }
    };
}