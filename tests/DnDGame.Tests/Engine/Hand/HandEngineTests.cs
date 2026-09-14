using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Hand;
using DnDGame.Domain.Entities.Cards;

namespace DnDGame.Tests.Engine.Hand;

public class HandEngineTests
{
    private readonly IHandEngine _handEngine = new HandEngine();

    [Fact]
    public void DiscardHandMovesAllCardsToTheDiscardPile()
    {
        var battleState = new BattleState
        {
            Hand = new List<CardInstance> { Instance(), Instance(), Instance() }
        };

        _handEngine.DiscardHand(battleState);

        Assert.Empty(battleState.Hand);
        Assert.Equal(3, battleState.DiscardPile.Count);
    }

    [Fact]
    public void DiscardHandWithAnEmptyHandIsNoOp()
    {
        var battleState = new BattleState();

        _handEngine.DiscardHand(battleState);

        Assert.Empty(battleState.DiscardPile);
    }

    [Fact]
    public void DiscardHandLeavesTheDrawPileUntouched()
    {
        var battleState = new BattleState
        {
            Hand = new List<CardInstance> { Instance() },
            DrawPile = new List<CardInstance> { Instance(), Instance() }
        };

        _handEngine.DiscardHand(battleState);

        Assert.Equal(2, battleState.DrawPile.Count);
        Assert.Single(battleState.DiscardPile);
    }

    [Fact]
    public void DiscardHandAccumulatesAcrossTurns()
    {
        var battleState = new BattleState
        {
            Hand = new List<CardInstance> { Instance() },
            DiscardPile = new List<CardInstance> { Instance() }
        };

        _handEngine.DiscardHand(battleState);

        Assert.Equal(2, battleState.DiscardPile.Count);
    }

    [Fact]
    public void DiscardHandThrowsOnNullState()
    {
        Assert.Throws<ArgumentNullException>(() => _handEngine.DiscardHand(null!));
    }

    private static CardInstance Instance() => new()
    {
        InstanceId = Guid.NewGuid(),
        CardId = 1,
        Card = new Card { Name = "Test", BaseCost = 1 }
    };
}