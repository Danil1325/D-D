using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Achievements;

/// <summary>
/// Reference data — a catalog entry describing one achievement, seeded once at
/// startup like Race/CharacterClass and never created or edited by players.
///
/// <see cref="Code"/> is the stable, human-readable identifier meant to be
/// referenced by clients, localization keys or tests; <see cref="Id"/> stays a
/// plain seed id so progress rows can point at it (same convention as the other
/// reference tables — see InMemoryGameDataStore's remarks on hardcoded seed ids).
///
/// The unlock condition is expressed as "reach TargetAmount of the event Type"
/// (e.g. Type = BattlesWon, TargetAmount = 10 means "win 10 battles").
/// </summary>
public class Achievement
{
    public int Id { get; set; }

    /// <summary>Stable identifier, e.g. "VICTORIOUS_WARRIOR". Never reused.</summary>
    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public AchievementType Type { get; set; }

    /// <summary>How many times the event must happen before the achievement completes.</summary>
    public int TargetAmount { get; set; }
}
