using DnDGame.BusinessLayer.Engines;
using DnDGame.BusinessLayer.Engines.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using Xunit;

namespace DnDGame.Tests.BusinessLayer;

public class PlayAndCardEngineTests
{
    private readonly PlayRules _rules = new(maxEnergyPerTurn: 5);

    [Fact]
    public void CanPlayCard_ReturnsSuccessForAffordableCardInHand()
    {
        var card = Instance(cost: 3, targetType: TargetType.Self);
        var deck = DeckWith(card);
        var battle = ActiveBattle(energy: 3);

        var result = new PlayEngine(_rules).CanPlayCard(battle, deck, card, playerId: 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CanPlayCard_ReturnsEngineResultWhenCardIsAbsentOrEnergyIsInsufficient()
    {
        var card = Instance(cost: 4, targetType: TargetType.Self);
        var absentResult = new PlayEngine(_rules).CanPlayCard(ActiveBattle(5), DeckWith(), card, 1);
        var energyResult = new PlayEngine(_rules).CanPlayCard(ActiveBattle(3), DeckWith(card), card, 1);

        Assert.False(absentResult.IsSuccess);
        Assert.Equal(ErrorCode.CARD_NOT_IN_HAND, absentResult.ErrorCode);
        Assert.False(energyResult.IsSuccess);
        Assert.Equal(ErrorCode.INSUFFICIENT_ENERGY, energyResult.ErrorCode);
    }

    [Fact]
    public void PlayCard_ConsumesEnergyOnlyAfterValidationAndReturnsEngineResult()
    {
        var card = Instance(cost: 3, targetType: TargetType.Self);
        var deck = DeckWith(card);
        var battle = ActiveBattle(energy: 5);
        var cardEngine = new CardEngine(new PlayEngine(_rules), new HandEngine(new HandRules(5)), new DeckEngine(), _rules);

        var result = cardEngine.PlayCard(battle, deck, card, playerId: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, battle.CurrentPlayerEnergy);
        Assert.Equal(3, result.Data!.EnergyCost);
        Assert.Empty(deck.Hand);
        Assert.Contains(card, deck.DiscardPile);
    }

    [Fact]
    public void CanPlayCard_ReturnsEngineResultWhenBattleIsNotActive()
    {
        var card = Instance(cost: 1, targetType: TargetType.Self);
        var deck = DeckWith(card);
        var battle = new Battle
        {
            Status = GameSessionStatus.Victory,
            CompletedAt = DateTime.UtcNow,
            CurrentPlayerTurnId = 1,
            CurrentPlayerEnergy = 5
        };

        var result = new PlayEngine(_rules).CanPlayCard(battle, deck, card, playerId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.BATTLE_NOT_ACTIVE, result.ErrorCode);
    }

    [Fact]
    public void CanPlayCard_ReturnsEngineResultWhenItIsNotThePlayersTurn()
    {
        var card = Instance(cost: 1, targetType: TargetType.Self);
        var deck = DeckWith(card);
        var battle = ActiveBattle(energy: 5);
        battle.CurrentPlayerTurnId = 2;

        var result = new PlayEngine(_rules).CanPlayCard(battle, deck, card, playerId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NOT_PLAYER_TURN, result.ErrorCode);
    }

    [Fact]
    public void CanPlayCard_ReturnsEngineResultWhenTargetIsInvalid()
    {
        var card = Instance(cost: 1, targetType: TargetType.SingleEnemy);
        var deck = DeckWith(card);

        var result = new PlayEngine(_rules).CanPlayCard(ActiveBattle(5), deck, card, playerId: 1, target: null);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.INVALID_TARGET, result.ErrorCode);
    }

    [Fact]
    public void CanPlayCard_ReturnsEngineResultWhenCardDefinitionIsMissing()
    {
        var orphan = new CardInstance { InstanceId = Guid.NewGuid(), CardId = 99, Card = null! };
        var deck = DeckWith(orphan);

        var result = new PlayEngine(_rules).CanPlayCard(ActiveBattle(5), deck, orphan, playerId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.CARD_CANNOT_BE_PLAYED, result.ErrorCode);
    }

    [Fact]
    public void CanPlayCard_ReturnsSuccessForTargetedCardWithValidTarget()
    {
        var card = Instance(cost: 2, targetType: TargetType.SingleEnemy);
        var deck = DeckWith(card);

        var result = new PlayEngine(_rules).CanPlayCard(ActiveBattle(5), deck, card, playerId: 1, new Target(42));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PlayCard_ReturnsEngineResultForAbsentCardWithoutConsumingEnergy()
    {
        var card = Instance(cost: 3, targetType: TargetType.Self);
        var deck = DeckWith();
        var battle = ActiveBattle(energy: 5);
        var cardEngine = NewCardEngine();

        var result = cardEngine.PlayCard(battle, deck, card, playerId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.CARD_NOT_IN_HAND, result.ErrorCode);
        Assert.Equal(5, battle.CurrentPlayerEnergy);
    }

    [Fact]
    public void PlayCard_ReturnsEngineResultForInsufficientEnergyWithoutConsumingEnergy()
    {
        var card = Instance(cost: 5, targetType: TargetType.Self);
        var deck = DeckWith(card);
        var battle = ActiveBattle(energy: 3);
        var cardEngine = NewCardEngine();

        var result = cardEngine.PlayCard(battle, deck, card, playerId: 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.INSUFFICIENT_ENERGY, result.ErrorCode);
        Assert.Equal(3, battle.CurrentPlayerEnergy);
        Assert.Contains(card, deck.Hand);
    }

    [Fact]
    public void PlayCard_ConsumesEffectiveCostIncludingTemporaryModifiers()
    {
        var card = Instance(cost: 3, targetType: TargetType.Self);
        card.TemporaryCostModifier = -1;
        var deck = DeckWith(card);
        var battle = ActiveBattle(energy: 5);
        var cardEngine = NewCardEngine();

        var result = cardEngine.PlayCard(battle, deck, card, playerId: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, battle.CurrentPlayerEnergy);
        Assert.Equal(2, result.Data!.EnergyCost);
        Assert.Equal(3, result.Data.RemainingEnergy);
    }

    [Fact]
    public void PlayCard_ReturnsSuccessWhenEnergyBalanceReachesZero()
    {
        var card = Instance(cost: 5, targetType: TargetType.Self);
        var deck = DeckWith(card);
        var battle = ActiveBattle(energy: 5);
        var cardEngine = NewCardEngine();

        var result = cardEngine.PlayCard(battle, deck, card, playerId: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, battle.CurrentPlayerEnergy);
        Assert.Equal(0, result.Data!.RemainingEnergy);
    }

    [Fact]
    public void PlayCard_OverdraftRulesAllowPlayingBeyondZeroEnergy()
    {
        var overdraft = new PlayRules(maxEnergyPerTurn: 5, allowOverdraft: true);
        var card = Instance(cost: 3, targetType: TargetType.Self);
        var deck = DeckWith(card);
        var battle = ActiveBattle(energy: 1);
        var cardEngine = new CardEngine(new PlayEngine(overdraft), new HandEngine(new HandRules(5)), new DeckEngine(), overdraft);

        var result = cardEngine.PlayCard(battle, deck, card, playerId: 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(-2, battle.CurrentPlayerEnergy);
    }

    private static CardEngine NewCardEngine(PlayRules? rules = null)
    {
        rules ??= new PlayRules(5);
        return new CardEngine(new PlayEngine(rules), new HandEngine(new HandRules(5)), new DeckEngine(), rules);
    }

    private static Battle ActiveBattle(int energy) => new()
    {
        Status = GameSessionStatus.InProgress,
        CurrentPlayerTurnId = 1,
        CurrentPlayerEnergy = energy
    };

    private static BattleDeck DeckWith(params CardInstance[] cards) => new() { Hand = cards.ToList() };

    private static CardInstance Instance(int cost, TargetType targetType) => new()
    {
        CardId = 1,
        Card = new Card { Id = 1, Name = "Test", BaseCost = cost, TargetType = targetType }
    };

    private sealed class Target(int targetId) : ICardTarget
    {
        public int TargetId { get; } = targetId;
        public string TargetType => "Enemy";
    }
}
