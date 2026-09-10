namespace DnDGame.Domain.Engine.Enums;

/// <summary>
/// Represents the current lifecycle state of a battle.
/// </summary>
public enum BattleStatus
{
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}
