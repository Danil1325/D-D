using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Deck;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Entities.Cards;

namespace DnDGame.Tests.Engine.Deck;

public class DeckEngineTests
{
    [Fact]
    public void StartOfTurnDrawsFiveCardsIntoTheHand()
    {
        var battleState = State(drawPile: 12);
        var deckEngine = new DeckEngine();

        deckEngine.DrawCardsForPlayerTurn(battleState);

        Assert.Equal(5, battleState.Hand.Count);
        Assert.Equal(7, battleState.DrawPile.Count);
    }

    [Fact]
    public void DrawsFewerWhenTheDrawPileRunsOut()
    {
        var battleState = State(drawPile: 3);
        var deckEngine = new DeckEngine();

        deckEngine.DrawCardsForPlayerTurn(battleState);

        Assert.Equal(3, battleState.Hand.Count);
        Assert.Empty(battleState.DrawPile);
    }

    [Fact]
    public void ReshufflesTheDiscardPileWhenTheDrawPileIsEmpty()
    {
        var battleState = State(drawPile: 0, discard: 6);
        var deckEngine = new DeckEngine();

        deckEngine.DrawCardsForPlayerTurn(battleState);

        Assert.Equal(5, battleState.Hand.Count);
        Assert.Empty(battleState.DiscardPile);
    }

    [Fact]
    public void DrawsNothingWhenNoCardsAreAvailable()
    {
        var battleState = State(drawPile: 0, discard: 0);
        var deckEngine = new DeckEngine();

        deckEngine.DrawCardsForPlayerTurn(battleState);

        Assert.Empty(battleState.Hand);
        Assert.Empty(battleState.DrawPile);
    }

    [Fact]
    public void ExistingHandCardsArePreserved()
    {
        var battleState = State(drawPile: 5);
        battleState.Hand.Add(Instance());
        var deckEngine = new DeckEngine();

        deckEngine.DrawCardsForPlayerTurn(battleState);

        Assert.Equal(6, battleState.Hand.Count);
    }

    [Fact]
    public void UsesTheInjectedRandomSourceWhenProvided()
    {
        var battleState = State(drawPile: 1);
        var deckEngine = new DeckEngine(new FixedRandomNumberSource());

        deckEngine.DrawCardsForPlayerTurn(battleState);

        Assert.Single(battleState.Hand);
    }

    [Fact]
    public void DrawCardsForPlayerTurnThrowsOnNullState()
    {
        Assert.Throws<ArgumentNullException>(() => new DeckEngine().DrawCardsForPlayerTurn(null!));
    }

    private static BattleState State(int drawPile = 0, int discard = 0) => new()
    {
        DrawPile = Enumerable.Range(0, drawPile).Select(_ => Instance()).ToList(),
        DiscardPile = Enumerable.Range(0, discard).Select(_ => Instance()).ToList()
    };

    private static CardInstance Instance() => new()
    {
        InstanceId = Guid.NewGuid(),
        CardId = 1,
        Card = new Card { Name = "Test", BaseCost = 1 }
    };

    private sealed class FixedRandomNumberSource : IRandomNumberSource
    {
        public int Next(int minimumInclusive, int maximumExclusive) => minimumInclusive;
    }
}