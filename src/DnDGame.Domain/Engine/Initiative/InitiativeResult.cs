using DnDGame.Domain.Engine.Enums;

namespace DnDGame.Domain.Engine.Initiative;

/// <summary>
/// The result of determining which combatant starts a battle.
/// </summary>
public sealed record InitiativeResult(
    TurnType StartingTurn,
    int? PlayerScore,
    int? EnemyScore,
    int? PlayerDiceResult,
    int? EnemyDiceResult);
