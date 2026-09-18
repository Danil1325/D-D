namespace DnDGame.Domain.Configuration;

/// <summary>Immutable, configurable cumulative EXP thresholds for levels 1 through 10.</summary>
public sealed class LevelProgressionRules
{
    public int MaxLevel => 10;
    public IReadOnlyList<int> ExperienceThresholds { get; }
    public int SkillPointsPerLevel { get; }

    public LevelProgressionRules()
        : this(new[] { 0, 120, 300, 550, 850, 1200, 1650, 2150, 2750, 3450 }, 3)
    {
    }

    /// <param name="experienceThresholds">Exactly ten strictly increasing thresholds, starting at zero.</param>
    /// <param name="skillPointsPerLevel">Points awarded for each gained level.</param>
    public LevelProgressionRules(IEnumerable<int> experienceThresholds, int skillPointsPerLevel = 3)
    {
        ArgumentNullException.ThrowIfNull(experienceThresholds);
        var thresholds = experienceThresholds.ToArray();
        if (thresholds.Length != MaxLevel || thresholds[0] != 0)
            throw new ArgumentException("Provide ten thresholds starting at zero.", nameof(experienceThresholds));

        for (var index = 1; index < thresholds.Length; index++)
        {
            if (thresholds[index] <= thresholds[index - 1])
                throw new ArgumentException("Thresholds must be strictly increasing.", nameof(experienceThresholds));
        }

        if (skillPointsPerLevel < 0 || skillPointsPerLevel > int.MaxValue / (MaxLevel - 1))
            throw new ArgumentOutOfRangeException(nameof(skillPointsPerLevel));

        ExperienceThresholds = Array.AsReadOnly(thresholds);
        SkillPointsPerLevel = skillPointsPerLevel;
    }
}
