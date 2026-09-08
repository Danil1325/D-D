namespace DnDGame.Domain.Enums;

/// <summary>
/// The lifecycle state of one character's run (GameSession) through one adventure.
/// </summary>
public enum GameSessionStatus
{
    InProgress,
    Victory,
    Defeat,
    Abandoned
}
