using DnDGame.Domain.Engine.Cards;
using DnDGame.Domain.Engine.Combat;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;
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
        IDiceEngine diceEngine)
    {
        ArgumentNullException.ThrowIfNull(turnEngine);
        ArgumentNullException.ThrowIfNull(initiativeEngine);
        ArgumentNullException.ThrowIfNull(cardEngine);
        ArgumentNullException.ThrowIfNull(effectEngine);
        ArgumentNullException.ThrowIfNull(damageCalculator);
        ArgumentNullException.ThrowIfNull(dodgeCalculator);
        ArgumentNullException.ThrowIfNull(criticalCalculator);
        ArgumentNullException.ThrowIfNull(diceEngine);

        _turnEngine = turnEngine;
        _initiativeEngine = initiativeEngine;
        _cardEngine = cardEngine;
        _effectEngine = effectEngine;
        _damageCalculator = damageCalculator;
        _dodgeCalculator = dodgeCalculator;
        _criticalCalculator = criticalCalculator;
        _diceEngine = diceEngine;
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

        var firstTurn = _initiativeEngine.DetermineFirstTurn(battleContext);
        var turnResult = firstTurn == TurnType.Player
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

    public EngineResult<BattleStatus> CheckBattleStatus(BattleContext battleContext)
    {
        if (CheckDefeat(battleContext.BattleState))
        {
            battleContext.BattleState.BattleStatus = BattleStatus.Defeat;
        }
        else if (CheckVictory(battleContext.BattleState))
        {
            battleContext.BattleState.BattleStatus = BattleStatus.Victory;
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
        if (CheckDefeat(battleContext.BattleState))
        {
            battleContext.BattleState.BattleStatus = BattleStatus.Defeat;
            return EngineResult<BattleState>.Fail("The player is defeated.", EngineErrorCodes.PlayerDead);
        }

        if (CheckVictory(battleContext.BattleState))
        {
            battleContext.BattleState.BattleStatus = BattleStatus.Victory;
            return EngineResult<BattleState>.Fail("The enemy is defeated.", EngineErrorCodes.EnemyDead);
        }

        return battleContext.BattleState.BattleStatus is BattleStatus.Victory or BattleStatus.Defeat
            ? BattleFinished()
            : null;
    }

    private static EngineResult<BattleState> BattleFinished()
    {
        return EngineResult<BattleState>.Fail(
            "The battle has already finished.",
            EngineErrorCodes.BattleAlreadyFinished);
    }
}
