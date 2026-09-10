namespace DnDGame.Domain.Engine.Initiative;

/// <summary>
/// Scores supplied by a configured initiative rule before a starting turn is chosen.
/// </summary>
public sealed record InitiativeScores(
    int PlayerScore,
    int EnemyScore,
    int? PlayerDiceResult = null,
    int? EnemyDiceResult = null);
