namespace DnDGame.Domain.Engine.Common;

/// <summary>
/// Stable error codes returned by game-engine operations.
/// </summary>
public static class EngineErrorCodes
{
    public const string BattleNotFound = "BATTLE_NOT_FOUND";
    public const string BattleAlreadyFinished = "BATTLE_ALREADY_FINISHED";
    public const string NotPlayerTurn = "NOT_PLAYER_TURN";
    public const string InvalidAction = "INVALID_ACTION";
    public const string PlayerDead = "PLAYER_DEAD";
    public const string EnemyDead = "ENEMY_DEAD";
    public const string MissingCombatRule = "MISSING_COMBAT_RULE";
    public const string InvalidDice = "INVALID_DICE";
}
