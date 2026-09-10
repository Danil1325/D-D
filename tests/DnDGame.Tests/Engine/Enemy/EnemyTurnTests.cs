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

namespace DnDGame.Tests.Engine.EnemyActions;

public class EnemyTurnTests
{
    [Fact]
    public void AttackReducesHp()
    {
        var turnEngine = new TurnEngineStub();
        var battleEngine = CreateBattleEngine(new EnemyAction(EnemyActionType.Attack), 0, turnEngine);
        var context = CreateEnemyTurnContext(playerHealth: 10, enemyDamage: 4);

        var result = battleEngine.ExecuteEnemyTurn(context);

        Assert.True(result.Success);
        Assert.Equal(6, context.BattleState.PlayerHealth);
    }

    [Fact]
    public void DefendAddsBlock()
    {
        var battleEngine = CreateBattleEngine(new EnemyAction(EnemyActionType.Defend), 5, new TurnEngineStub());
        var context = CreateEnemyTurnContext(playerHealth: 10, enemyDamage: 4, enemyBlock: 2);

        var result = battleEngine.ExecuteEnemyTurn(context);

        Assert.True(result.Success);
        Assert.Equal(7, context.BattleState.EnemyBlock);
    }

    [Fact]
    public void EnemyKillingPlayerCausesDefeat()
    {
        var battleEngine = CreateBattleEngine(new EnemyAction(EnemyActionType.Attack), 0, new TurnEngineStub());
        var context = CreateEnemyTurnContext(playerHealth: 3, enemyDamage: 5);

        var result = battleEngine.ExecuteEnemyTurn(context);

        Assert.True(result.Success);
        Assert.Equal(0, context.BattleState.PlayerHealth);
        Assert.Equal(BattleStatus.Defeat, context.BattleState.BattleStatus);
    }

    [Fact]
    public void NoNewTurnStartsAfterDefeat()
    {
        var turnEngine = new TurnEngineStub();
        var battleEngine = CreateBattleEngine(new EnemyAction(EnemyActionType.Attack), 0, turnEngine);
        var context = CreateEnemyTurnContext(playerHealth: 3, enemyDamage: 5);

        battleEngine.ExecuteEnemyTurn(context);

        Assert.Equal(0, turnEngine.EndEnemyTurnCalls);
        Assert.Equal(TurnType.Enemy, context.BattleState.CurrentTurn);
        Assert.Equal(BattleStatus.Defeat, context.BattleState.BattleStatus);
    }

    private static BattleEngine CreateBattleEngine(
        EnemyAction action,
        int defendBlock,
        TurnEngineStub turnEngine)
    {
        return new BattleEngine(
            turnEngine,
            new InitiativeEngineStub(),
            new CardEngineStub(),
            new EffectEngineStub(),
            new DamageCalculator(new AdditiveDamageRule()),
            new DodgeCalculatorStub(),
            new CriticalCalculatorStub(),
            new DiceEngineStub(),
            new EnemyActionSelector(new FixedEnemyActionRule(action)),
            new FixedEnemyDefenseRule(defendBlock),
            new BattleLogWriter());
    }

    private static BattleContext CreateEnemyTurnContext(int playerHealth, int enemyDamage, int enemyBlock = 0)
    {
        return new BattleContext(
            new PlayerCharacter { CurrentHealth = playerHealth, MaxHealth = playerHealth },
            new Enemy { DamageAmount = enemyDamage, Health = 10 },
            new BattleState
            {
                PlayerHealth = playerHealth,
                PlayerMaxHealth = playerHealth,
                EnemyHealth = 10,
                EnemyMaxHealth = 10,
                EnemyBlock = enemyBlock,
                CurrentTurn = TurnType.Enemy,
                BattleStatus = BattleStatus.EnemyTurn
            });
    }

    private sealed class FixedEnemyActionRule : IEnemyActionRule
    {
        private readonly EnemyAction _action;

        public FixedEnemyActionRule(EnemyAction action)
        {
            _action = action;
        }

        public EngineResult<EnemyAction> SelectAction(BattleContext battleContext)
        {
            return EngineResult<EnemyAction>.Ok(_action);
        }
    }

    private sealed class FixedEnemyDefenseRule : IEnemyDefenseRule
    {
        private readonly int _block;

        public FixedEnemyDefenseRule(int block)
        {
            _block = block;
        }

        public EngineResult<int> CalculateBlock(BattleContext battleContext)
        {
            return EngineResult<int>.Ok(_block);
        }
    }

    private sealed class TurnEngineStub : ITurnEngine
    {
        public int EndEnemyTurnCalls { get; private set; }

        public EngineResult<TurnResult> StartPlayerTurn(BattleState battleState) => Result(battleState);
        public EngineResult<TurnResult> EndPlayerTurn(BattleState battleState) => Result(battleState);
        public EngineResult<TurnResult> StartEnemyTurn(BattleState battleState) => Result(battleState);

        public EngineResult<TurnResult> EndEnemyTurn(BattleState battleState)
        {
            EndEnemyTurnCalls++;
            battleState.CurrentTurn = TurnType.Player;
            battleState.BattleStatus = BattleStatus.PlayerTurn;
            battleState.TurnNumber++;
            return Result(battleState);
        }

        public EngineResult<TurnResult> NextTurn(BattleState battleState) => Result(battleState);

        private static EngineResult<TurnResult> Result(BattleState battleState)
        {
            return EngineResult<TurnResult>.Ok(
                new TurnResult(
                    battleState,
                    battleState.CurrentTurn,
                    battleState.BattleStatus,
                    battleState.TurnNumber));
        }
    }

    private sealed class InitiativeEngineStub : IInitiativeEngine
    {
        public EngineResult<InitiativeResult> DetermineFirstTurn(BattleContext battleContext) =>
            EngineResult<InitiativeResult>.Fail("Not used by this test.", EngineErrorCodes.InvalidAction);
    }

    private sealed class CardEngineStub : ICardEngine
    {
        public EngineResult<BattleState> PlayCard(BattleContext battleContext, CardInstance card) =>
            EngineResult<BattleState>.Fail("Not used by this test.", EngineErrorCodes.InvalidAction);
    }

    private sealed class EffectEngineStub : IEffectEngine
    {
        public void ApplyStartOfTurnEffects(BattleState battleState, TurnType turn) { }
        public void ApplyEndOfTurnEffects(BattleState battleState, TurnType turn) { }
    }

    private sealed class DodgeCalculatorStub : IDodgeCalculator
    {
        public EngineResult<DodgeResult> Calculate(DodgeRequest request) =>
            EngineResult<DodgeResult>.Ok(new DodgeResult(DodgeOutcome.Hit));
    }

    private sealed class CriticalCalculatorStub : ICriticalCalculator
    {
        public EngineResult<CriticalResult> Calculate(DiceResult diceResult) =>
            EngineResult<CriticalResult>.Ok(new CriticalResult(CriticalOutcome.NormalHit, diceResult));
    }

    private sealed class DiceEngineStub : IDiceEngine
    {
        public EngineResult<DiceResult> Roll(DiceType dice) =>
            EngineResult<DiceResult>.Ok(new DiceResult(dice, 1, 0));

        public EngineResult<DiceResult> RollWithModifier(DiceType dice, int modifier) =>
            EngineResult<DiceResult>.Ok(new DiceResult(dice, 1, modifier));
    }
}
