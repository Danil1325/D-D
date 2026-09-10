using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Turn;

/// <summary>
/// Snapshot of a battle immediately after a turn-boundary operation.
/// </summary>
public sealed record TurnResult(
    BattleState BattleState,
    TurnType CurrentTurn,
    BattleStatus BattleStatus,
    int TurnNumber);
