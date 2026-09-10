using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Deck;
using DnDGame.Domain.Engine.Effects;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Turn;
using Xunit;

namespace DnDGame.Tests.Engine.Turn;

public class TurnEngineTests
{
    private readonly ITurnEngine _turnEngine = new TurnEngine(
        new DeckEngineStub(),
        new EffectEngineStub());

    [Fact]
    public void TurnNumberIncreasesCorrectly()
    {
        var battleState = CreateBattleState(turnNumber: 1);

        var enemyTurn = _turnEngine.EndPlayerTurn(battleState);
        var nextPlayerTurn = _turnEngine.EndEnemyTurn(battleState);

        Assert.True(enemyTurn.Success);
        Assert.True(nextPlayerTurn.Success);
        Assert.Equal(2, battleState.TurnNumber);
    }

    [Fact]
    public void StartPlayerTurnResetsEnergy()
    {
        var battleState = CreateBattleState(playerEnergy: 1, playerMaxEnergy: 3);

        var result = _turnEngine.StartPlayerTurn(battleState);

        Assert.True(result.Success);
        Assert.Equal(3, battleState.PlayerEnergy);
    }

    [Fact]
    public void EndPlayerTurnChangesCurrentTurnToEnemy()
    {
        var battleState = CreateBattleState();

        var result = _turnEngine.EndPlayerTurn(battleState);

        Assert.True(result.Success);
        Assert.Equal(TurnType.Enemy, battleState.CurrentTurn);
    }

    [Fact]
    public void TurnTransitionsUpdateBattleStatus()
    {
        var battleState = CreateBattleState();

        var enemyResult = _turnEngine.EndPlayerTurn(battleState);
        var playerResult = _turnEngine.EndEnemyTurn(battleState);

        Assert.True(enemyResult.Success);
        Assert.Equal(BattleStatus.EnemyTurn, enemyResult.Data!.BattleStatus);
        Assert.True(playerResult.Success);
        Assert.Equal(BattleStatus.PlayerTurn, playerResult.Data!.BattleStatus);
    }

    private static BattleState CreateBattleState(
        int turnNumber = 1,
        int playerEnergy = 0,
        int playerMaxEnergy = 3)
    {
        return new BattleState
        {
            TurnNumber = turnNumber,
            CurrentTurn = TurnType.Player,
            BattleStatus = BattleStatus.PlayerTurn,
            PlayerEnergy = playerEnergy,
            PlayerMaxEnergy = playerMaxEnergy
        };
    }

    private sealed class DeckEngineStub : IDeckEngine
    {
        public void DrawCardsForPlayerTurn(BattleState battleState)
        {
        }

        public void DiscardHand(BattleState battleState)
        {
        }
    }

    private sealed class EffectEngineStub : IEffectEngine
    {
        public void ApplyStartOfTurnEffects(BattleState battleState, TurnType turn)
        {
        }

        public void ApplyEndOfTurnEffects(BattleState battleState, TurnType turn)
        {
        }
    }
}
