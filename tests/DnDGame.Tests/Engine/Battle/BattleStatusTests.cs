using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Cards;
using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.EnemyActions;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Effects;
using DnDGame.Domain.Engine.Initiative;
using DnDGame.Domain.Engine.Models;
using DnDGame.Domain.Engine.Turn;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;
using Xunit;

namespace DnDGame.Tests.Engine.Battle;

public class BattleStatusTests
{
    [Fact]
    public void PlayerDefeatWorks()
    {
        var context = CreateContext(playerHealth: 0, enemyHealth: 10);
        var engine = CreateEngine();

        var result = engine.CheckBattleStatus(context);

        Assert.True(result.Success);
        Assert.Equal(BattleStatus.Defeat, context.BattleState.BattleStatus);
        Assert.Single(context.BattleState.BattleLog);
        Assert.Equal("Defeat", context.BattleState.BattleLog[0].Action);
    }

    [Fact]
    public void EnemyDefeatWorks()
    {
        var context = CreateContext(playerHealth: 10, enemyHealth: 0);
        var engine = CreateEngine();

        var result = engine.CheckBattleStatus(context);

        Assert.True(result.Success);
        Assert.Equal(BattleStatus.Victory, context.BattleState.BattleStatus);
        Assert.Single(context.BattleState.BattleLog);
        Assert.Equal("Victory", context.BattleState.BattleLog[0].Action);
    }

    [Fact]
    public void CannotActAfterBattleEnds()
    {
        var context = CreateContext(playerHealth: 10, enemyHealth: 0);
        var engine = CreateEngine();
        engine.CheckBattleStatus(context);

        var playCard = engine.PlayCard(context, new CardInstance());
        var endTurn = engine.EndTurn(context);
        var enemyAction = engine.ExecuteEnemyTurn(context);

        Assert.Equal(EngineErrorCodes.BattleAlreadyFinished, playCard.ErrorCode);
        Assert.Equal(EngineErrorCodes.BattleAlreadyFinished, endTurn.ErrorCode);
        Assert.Equal(EngineErrorCodes.BattleAlreadyFinished, enemyAction.ErrorCode);
        Assert.Equal(TurnType.Player, context.BattleState.CurrentTurn);
        Assert.Single(context.BattleState.BattleLog);
    }

    private static BattleContext CreateContext(int playerHealth, int enemyHealth)
    {
        return new BattleContext(
            new PlayerCharacter { CurrentHealth = playerHealth, MaxHealth = 10 },
            new Enemy { Health = enemyHealth },
            new BattleState
            {
                PlayerHealth = playerHealth,
                EnemyHealth = enemyHealth,
                CurrentTurn = TurnType.Player,
                BattleStatus = BattleStatus.PlayerTurn
            });
    }

    private static BattleEngine CreateEngine()
    {
        return new BattleEngine(
            new TurnEngineStub(),
            new InitiativeEngineStub(),
            new CardEngineStub(),
            new EffectEngineStub(),
            new DamageCalculator(new AdditiveDamageRule()),
            new DodgeCalculatorStub(),
            new CriticalCalculatorStub(),
            new DiceEngineStub(),
            new EnemyActionSelector(new EnemyActionRuleStub()),
            new EnemyDefenseRuleStub(),
            new BattleLogWriter());
    }

    private sealed class TurnEngineStub : ITurnEngine
    {
        public EngineResult<TurnResult> StartPlayerTurn(BattleState state) => Result(state);
        public EngineResult<TurnResult> EndPlayerTurn(BattleState state) => Result(state);
        public EngineResult<TurnResult> StartEnemyTurn(BattleState state) => Result(state);
        public EngineResult<TurnResult> EndEnemyTurn(BattleState state) => Result(state);
        public EngineResult<TurnResult> NextTurn(BattleState state) => Result(state);

        private static EngineResult<TurnResult> Result(BattleState state) =>
            EngineResult<TurnResult>.Ok(new TurnResult(
                state,
                state.CurrentTurn,
                state.BattleStatus,
                state.TurnNumber));
    }

    private sealed class InitiativeEngineStub : IInitiativeEngine
    {
        public EngineResult<InitiativeResult> DetermineFirstTurn(BattleContext context) =>
            EngineResult<InitiativeResult>.Fail("Not used.", EngineErrorCodes.InvalidAction);
    }

    private sealed class CardEngineStub : ICardEngine
    {
        public EngineResult<BattleState> PlayCard(BattleContext context, CardInstance card) =>
            EngineResult<BattleState>.Fail("Not used.", EngineErrorCodes.InvalidAction);
    }

    private sealed class EffectEngineStub : IEffectEngine
    {
        public void ApplyStartOfTurnEffects(BattleState state, TurnType turn) { }
        public void ApplyEndOfTurnEffects(BattleState state, TurnType turn) { }
    }

    private sealed class DodgeCalculatorStub : IDodgeCalculator
    {
        public EngineResult<DodgeResult> Calculate(DodgeRequest request) =>
            EngineResult<DodgeResult>.Ok(new DodgeResult(DodgeOutcome.Hit));
    }

    private sealed class CriticalCalculatorStub : ICriticalCalculator
    {
        public EngineResult<CriticalResult> Calculate(DiceResult result) =>
            EngineResult<CriticalResult>.Ok(new CriticalResult(CriticalOutcome.NormalHit, result));
    }

    private sealed class DiceEngineStub : IDiceEngine
    {
        public EngineResult<DiceResult> Roll(DiceType dice) =>
            EngineResult<DiceResult>.Ok(new DiceResult(dice, 1, 0));

        public EngineResult<DiceResult> RollWithModifier(DiceType dice, int modifier) =>
            EngineResult<DiceResult>.Ok(new DiceResult(dice, 1, modifier));
    }

    private sealed class EnemyActionRuleStub : IEnemyActionRule
    {
        public EngineResult<EnemyAction> SelectAction(BattleContext context) =>
            EngineResult<EnemyAction>.Ok(new EnemyAction(EnemyActionType.Attack));
    }

    private sealed class EnemyDefenseRuleStub : IEnemyDefenseRule
    {
        public EngineResult<int> CalculateBlock(BattleContext context) => EngineResult<int>.Ok(0);
    }
}
