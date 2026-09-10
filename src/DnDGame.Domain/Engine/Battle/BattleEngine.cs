using DnDGame.Domain.Engine.Cards;
using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.EnemyActions;
using DnDGame.Domain.Engine.Effects;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Initiative;
using DnDGame.Domain.Engine.Models;
using DnDGame.Domain.Engine.Turn;

namespace DnDGame.Domain.Engine.Battle;

/// <summary>
/// Coordinates the lifecycle of a battle without owning combat, card, dice, or
/// effect rules.
/// </summary>
public sealed class BattleEngine : IBattleEngine
{
    private readonly ITurnEngine _turnEngine;
    private readonly IInitiativeEngine _initiativeEngine;
    private readonly ICardEngine _cardEngine;
    private readonly IEnemyActionSelector _enemyActionSelector;
    private readonly IEnemyDefenseRule _enemyDefenseRule;
    private readonly IBattleLogWriter _battleLogWriter;

    // Retained as injected collaborators for future action-resolution orchestration.
    private readonly IEffectEngine _effectEngine;
    private readonly IDamageCalculator _damageCalculator;
    private readonly IDodgeCalculator _dodgeCalculator;
    private readonly ICriticalCalculator _criticalCalculator;
    private readonly IDiceEngine _diceEngine;

    public BattleEngine(
        ITurnEngine turnEngine,
        IInitiativeEngine initiativeEngine,
        ICardEngine cardEngine,
        IEffectEngine effectEngine,
        IDamageCalculator damageCalculator,
        IDodgeCalculator dodgeCalculator,
        ICriticalCalculator criticalCalculator,
        IDiceEngine diceEngine,
        IEnemyActionSelector enemyActionSelector,
        IEnemyDefenseRule enemyDefenseRule,
        IBattleLogWriter battleLogWriter)
    {
        ArgumentNullException.ThrowIfNull(turnEngine);
        ArgumentNullException.ThrowIfNull(initiativeEngine);
        ArgumentNullException.ThrowIfNull(cardEngine);
        ArgumentNullException.ThrowIfNull(effectEngine);
        ArgumentNullException.ThrowIfNull(damageCalculator);
        ArgumentNullException.ThrowIfNull(dodgeCalculator);
        ArgumentNullException.ThrowIfNull(criticalCalculator);
        ArgumentNullException.ThrowIfNull(diceEngine);
        ArgumentNullException.ThrowIfNull(enemyActionSelector);
        ArgumentNullException.ThrowIfNull(enemyDefenseRule);
        ArgumentNullException.ThrowIfNull(battleLogWriter);

        _turnEngine = turnEngine;
        _initiativeEngine = initiativeEngine;
        _cardEngine = cardEngine;
        _effectEngine = effectEngine;
        _damageCalculator = damageCalculator;
        _dodgeCalculator = dodgeCalculator;
        _criticalCalculator = criticalCalculator;
        _diceEngine = diceEngine;
        _enemyActionSelector = enemyActionSelector;
        _enemyDefenseRule = enemyDefenseRule;
        _battleLogWriter = battleLogWriter;
    }

    public EngineResult<BattleState> StartBattle(BattleContext battleContext)
    {
        if (battleContext.BattleState.BattleStatus is BattleStatus.Victory or BattleStatus.Defeat)
        {
            return BattleFinished();
        }

        InitialiseState(battleContext);

        var statusResult = CheckBattleStatus(battleContext);
        if (battleContext.BattleState.BattleStatus is BattleStatus.Victory or BattleStatus.Defeat)
        {
            return EngineResult<BattleState>.Ok(battleContext.BattleState, statusResult.Message);
        }

        var initiativeResult = _initiativeEngine.DetermineFirstTurn(battleContext);
        if (!initiativeResult.Success || initiativeResult.Data is null)
        {
            return EngineResult<BattleState>.Fail(
                initiativeResult.Message,
                initiativeResult.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
        }

        var turnResult = initiativeResult.Data.StartingTurn == TurnType.Player
            ? _turnEngine.StartPlayerTurn(battleContext.BattleState)
            : _turnEngine.StartEnemyTurn(battleContext.BattleState);

        return turnResult.Success
            ? EngineResult<BattleState>.Ok(battleContext.BattleState)
            : EngineResult<BattleState>.Fail(turnResult.Message, turnResult.ErrorCode ?? EngineErrorCodes.InvalidAction);
    }

    public EngineResult<BattleState> PlayCard(BattleContext battleContext, CardInstance? card)
    {
        var failure = EnsureActionAllowed(battleContext);
        if (failure is not null)
        {
            return failure;
        }

        if (battleContext.BattleState.CurrentTurn != TurnType.Player ||
            battleContext.BattleState.BattleStatus != BattleStatus.PlayerTurn)
        {
            return EngineResult<BattleState>.Fail(
                "Cards can only be played during the player turn.",
                EngineErrorCodes.NotPlayerTurn);
        }

        if (card is null)
        {
            return EngineResult<BattleState>.Fail(
                "A card must be provided.",
                EngineErrorCodes.InvalidAction);
        }

        var cardResult = _cardEngine.PlayCard(battleContext, card);
        if (!cardResult.Success)
        {
            return cardResult;
        }

        CheckBattleStatus(battleContext);
        return EngineResult<BattleState>.Ok(battleContext.BattleState);
    }

    public EngineResult<BattleState> EndTurn(BattleContext battleContext)
    {
        var failure = EnsureActionAllowed(battleContext);
        if (failure is not null)
        {
            return failure;
        }

        if (battleContext.BattleState.CurrentTurn != TurnType.Player ||
            battleContext.BattleState.BattleStatus != BattleStatus.PlayerTurn)
        {
            return EngineResult<BattleState>.Fail(
                "Only the active player turn can be ended.",
                EngineErrorCodes.NotPlayerTurn);
        }

        var turnResult = _turnEngine.EndPlayerTurn(battleContext.BattleState);
        if (!turnResult.Success)
        {
            return EngineResult<BattleState>.Fail(
                turnResult.Message,
                turnResult.ErrorCode ?? EngineErrorCodes.InvalidAction);
        }

        CheckBattleStatus(battleContext);
        return EngineResult<BattleState>.Ok(battleContext.BattleState);
    }

    public EngineResult<BattleState> ExecuteEnemyTurn(BattleContext battleContext)
    {
        var failure = EnsureActionAllowed(battleContext);
        if (failure is not null)
        {
            return failure;
        }

        if (battleContext.BattleState.CurrentTurn != TurnType.Enemy ||
            battleContext.BattleState.BattleStatus != BattleStatus.EnemyTurn)
        {
            return EngineResult<BattleState>.Fail(
                "The enemy turn is not active.",
                EngineErrorCodes.InvalidAction);
        }

        var actionResult = _enemyActionSelector.SelectAction(battleContext);
        if (!actionResult.Success || actionResult.Data is null)
        {
            return EngineResult<BattleState>.Fail(
                actionResult.Message,
                actionResult.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
        }

        var executionResult = actionResult.Data.Type switch
        {
            EnemyActionType.Attack => ExecuteEnemyAttack(battleContext),
            EnemyActionType.Defend => ExecuteEnemyDefend(battleContext),
            _ => EngineResult<BattleState>.Fail(
                "The selected enemy action is invalid.",
                EngineErrorCodes.InvalidAction)
        };

        if (!executionResult.Success)
        {
            return executionResult;
        }

        CheckBattleStatus(battleContext);
        if (CheckDefeat(battleContext.BattleState))
        {
            return EngineResult<BattleState>.Ok(battleContext.BattleState);
        }

        var turnResult = _turnEngine.EndEnemyTurn(battleContext.BattleState);
        return turnResult.Success
            ? EngineResult<BattleState>.Ok(battleContext.BattleState)
            : EngineResult<BattleState>.Fail(
                turnResult.Message,
                turnResult.ErrorCode ?? EngineErrorCodes.InvalidAction);
    }

    public EngineResult<bool> MarkRewardsGranted(BattleContext battleContext)
    {
        var battleState = battleContext.BattleState;

        if (battleState.RewardsGranted)
        {
            return EngineResult<bool>.Fail(
                "Rewards have already been granted for this battle.",
                EngineErrorCodes.RewardsAlreadyGranted);
        }

        if (battleState.BattleStatus != BattleStatus.Victory)
        {
            return EngineResult<bool>.Fail(
                "Rewards can only be granted after victory.",
                EngineErrorCodes.InvalidAction);
        }

        battleState.RewardsGranted = true;
        return EngineResult<bool>.Ok(true);
    }

    public EngineResult<BattleStatus> CheckBattleStatus(BattleContext battleContext)
    {
        if (IsBattleFinished(battleContext.BattleState))
        {
            return EngineResult<BattleStatus>.Ok(battleContext.BattleState.BattleStatus);
        }

        if (CheckDefeat(battleContext.BattleState))
        {
            FinalizeBattle(battleContext.BattleState, BattleStatus.Defeat);
        }
        else if (CheckVictory(battleContext.BattleState))
        {
            FinalizeBattle(battleContext.BattleState, BattleStatus.Victory);
        }

        return EngineResult<BattleStatus>.Ok(battleContext.BattleState.BattleStatus);
    }

    public bool CheckVictory(BattleState battleState)
    {
        return battleState.EnemyHealth <= 0;
    }

    public bool CheckDefeat(BattleState battleState)
    {
        return battleState.PlayerHealth <= 0;
    }

    private EngineResult<BattleState> ExecuteEnemyAttack(BattleContext battleContext)
    {
        var battleState = battleContext.BattleState;
        var damageCalculation = _damageCalculator.Calculate(
            new DamageRequest(
                baseDamage: battleContext.Enemy.DamageAmount,
                strength: 0,
                defense: 0,
                block: battleState.PlayerBlock));
        if (!damageCalculation.Success || damageCalculation.Data is null)
        {
            return EngineResult<BattleState>.Fail(
                damageCalculation.Message,
                damageCalculation.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
        }

        var damageResult = damageCalculation.Data;

        battleState.PlayerBlock = damageResult.RemainingBlock;
        battleState.PlayerHealth = Math.Max(0, battleState.PlayerHealth - damageResult.FinalDamage);

        return EngineResult<BattleState>.Ok(battleState);
    }

    private EngineResult<BattleState> ExecuteEnemyDefend(BattleContext battleContext)
    {
        var blockResult = _enemyDefenseRule.CalculateBlock(battleContext);
        if (!blockResult.Success)
        {
            return EngineResult<BattleState>.Fail(
                blockResult.Message,
                blockResult.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
        }

        battleContext.BattleState.EnemyBlock += blockResult.Data;
        return EngineResult<BattleState>.Ok(battleContext.BattleState);
    }

    private static void InitialiseState(BattleContext battleContext)
    {
        var battleState = battleContext.BattleState;

        battleState.BattleId = battleState.BattleId == Guid.Empty
            ? Guid.NewGuid()
            : battleState.BattleId;
        battleState.PlayerHealth = battleContext.Player.CurrentHealth;
        battleState.PlayerMaxHealth = battleContext.Player.MaxHealth;
        battleState.EnemyHealth = battleContext.Enemy.Health;
        battleState.EnemyMaxHealth = battleContext.Enemy.Health;
        battleState.PlayerBlock = 0;
        battleState.EnemyBlock = 0;
        battleState.TurnNumber = 1;
        battleState.RewardsGranted = false;
    }

    private EngineResult<BattleState>? EnsureActionAllowed(BattleContext battleContext)
    {
        if (IsBattleFinished(battleContext.BattleState))
        {
            return BattleFinished();
        }

        CheckBattleStatus(battleContext);

        return IsBattleFinished(battleContext.BattleState)
            ? BattleFinished()
            : null;
    }

    private void FinalizeBattle(BattleState battleState, BattleStatus terminalStatus)
    {
        battleState.BattleStatus = terminalStatus;

        var action = terminalStatus == BattleStatus.Victory ? "Victory" : "Defeat";
        var result = terminalStatus == BattleStatus.Victory
            ? "Enemy health reached zero."
            : "Player health reached zero.";

        _battleLogWriter.Write(
            battleState,
            new BattleLogEntry(
                battleState.TurnNumber,
                actor: "Battle",
                action: action,
                result: result));
    }

    private static bool IsBattleFinished(BattleState battleState)
    {
        return battleState.BattleStatus is BattleStatus.Victory or BattleStatus.Defeat;
    }

    private static EngineResult<BattleState> BattleFinished()
    {
        return EngineResult<BattleState>.Fail(
            "The battle has already finished.",
            EngineErrorCodes.BattleAlreadyFinished);
    }
}
