namespace DnDGame.BusinessLayer.Dtos.Progression;

/// <summary>
/// Response shape for GET /api/progression/{playerId} and
/// POST /api/progression/{playerId}/experience.
///
/// Experience thresholds come from LevelProgressionRules. At the maximum level
/// there is no next tier, so <see cref="ExperienceForNextLevel"/> is null and the
/// progress percentage is reported as 0.
/// </summary>
public class CharacterProgressionDto
{
    public int Level { get; init; }
    public int CurrentExperience { get; init; }

    /// <summary>Cumulative experience required to have reached the current level.</summary>
    public int ExperienceForCurrentLevel { get; init; }

    /// <summary>Cumulative experience required to reach the next level (null at the maximum level).</summary>
    public int? ExperienceForNextLevel { get; init; }

    /// <summary>Percentage of the way from the current level thresholds to the next one (0 at the maximum level).</summary>
    public double ExperienceProgressPercentage { get; init; }

    /// <summary>Unspent skill points accumulated from level-ups.</summary>
    public int AvailableSkillPoints { get; init; }
}