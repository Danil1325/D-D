using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Deck;
using DnDGame.Domain.Engine.Effects;
using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Turn;

/// <summary>
/// Coordinates turn transitions while delegating deck and effect behavior to their
/// respective engines.
/// </summary>
public sealed class TurnEngine : ITurnEngine
{
    private readonly IDeckEngine _deckEngine;
    private readonly IEffectEngine _effectEngine;

    public TurnEngine(IDeckEngine deckEngine, IEffectEngine effectEngine)
    {
        _deckEngine = deckEngine;
        _effectEngine = effectEngine;
    }

    public EngineResult<TurnResult> StartPlayerTurn(BattleState battleState)
    {
        var failure = EnsureBattleIsActive(battleState);
        if (failure is not null)
        {
            return failure;
        }

        if (battleState.TurnNumber == 0)
        {
            battleState.TurnNumber = 1;
        }

        battleState.CurrentTurn = TurnType.Player;
        battleState.BattleStatus = BattleStatus.PlayerTurn;
        battleState.PlayerEnergy = battleState.PlayerMaxEnergy;
        battleState.PlayerBlock = 0;

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Player);
        _deckEngine.DrawCardsForPlayerTurn(battleState);

        return Succeed(battleState);
    }

    public EngineResult<TurnResult> EndPlayerTurn(BattleState battleState)
    {
        var failure = EnsureBattleIsActive(battleState);
        if (failure is not null)
        {
            return failure;
        }

        if (battleState.CurrentTurn != TurnType.Player)
        {
            return EngineResult<TurnResult>.Fail(
                "The player turn is not active.",
                EngineErrorCodes.NotPlayerTurn);
        }

        _effectEngine.ApplyEndOfTurnEffects(battleState, TurnType.Player);
        _deckEngine.DiscardHand(battleState);

        return NextTurn(battleState);
    }

    public EngineResult<TurnResult> StartEnemyTurn(BattleState battleState)
    {
        var failure = EnsureBattleIsActive(battleState);
        if (failure is not null)
        {
            return failure;
        }

        battleState.CurrentTurn = TurnType.Enemy;
        battleState.BattleStatus = BattleStatus.EnemyTurn;

        _effectEngine.ApplyStartOfTurnEffects(battleState, TurnType.Enemy);

        return Succeed(battleState);
    }

    public EngineResult<TurnResult> EndEnemyTurn(BattleState battleState)
    {
        var failure = EnsureBattleIsActive(battleState);
        if (failure is not null)
        {
            return failure;
        }

        if (battleState.CurrentTurn != TurnType.Enemy)
        {
            return EngineResult<TurnResult>.Fail(
                "The enemy turn is not active.",
                EngineErrorCodes.InvalidAction);
        }

        _effectEngine.ApplyEndOfTurnEffects(battleState, TurnType.Enemy);

        return NextTurn(battleState);
    }

    public EngineResult<TurnResult> NextTurn(BattleState battleState)
    {
        var failure = EnsureBattleIsActive(battleState);
        if (failure is not null)
        {
            return failure;
        }

        if (battleState.CurrentTurn == TurnType.Player)
        {
            return StartEnemyTurn(battleState);
        }

        battleState.TurnNumber++;
        return StartPlayerTurn(battleState);
    }

    private static EngineResult<TurnResult>? EnsureBattleIsActive(BattleState battleState)
    {
        return battleState.BattleStatus is BattleStatus.Victory or BattleStatus.Defeat
            ? EngineResult<TurnResult>.Fail(
                "The battle has already finished.",
                EngineErrorCodes.BattleAlreadyFinished)
            : null;
    }

    private static EngineResult<TurnResult> Succeed(BattleState battleState)
    {
        return EngineResult<TurnResult>.Ok(
            new TurnResult(
                battleState,
                battleState.CurrentTurn,
                battleState.BattleStatus,
                battleState.TurnNumber));
    }
}
