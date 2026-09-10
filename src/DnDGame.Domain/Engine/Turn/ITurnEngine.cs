using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Turn;

/// <summary>
/// Coordinates transitions between player and enemy turns.
/// </summary>
public interface ITurnEngine
{
    EngineResult<TurnResult> StartPlayerTurn(BattleState battleState);

    EngineResult<TurnResult> EndPlayerTurn(BattleState battleState);

    EngineResult<TurnResult> StartEnemyTurn(BattleState battleState);

    EngineResult<TurnResult> EndEnemyTurn(BattleState battleState);

    EngineResult<TurnResult> NextTurn(BattleState battleState);
}
