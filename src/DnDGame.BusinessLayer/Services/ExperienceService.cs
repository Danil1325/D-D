using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Characters;

namespace DnDGame.BusinessLayer.Services;

public sealed class ExperienceService : IExperienceService
{
    private readonly LevelProgressionRules _rules;

    public ExperienceService(LevelProgressionRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules;
    }

    public ExperienceResult AddExperience(PlayerCharacter player, int experience)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentOutOfRangeException.ThrowIfNegative(experience);
        if (player.Level < 1 || player.Level > _rules.MaxLevel ||
            player.CurrentXp < 0 || player.SkillPoints < 0)
            throw new ArgumentException("Character progression values are invalid.", nameof(player));

        var previousLevel = player.Level;
        var totalExperience = (int)Math.Min(int.MaxValue, (long)player.CurrentXp + experience);
        var currentLevel = previousLevel;

        while (currentLevel < _rules.MaxLevel &&
               totalExperience >= _rules.ExperienceThresholds[currentLevel])
        {
            currentLevel++;
        }

        var pointsGained = (currentLevel - previousLevel) * _rules.SkillPointsPerLevel;
        // Calculate before changing the character so an invalid points total cannot partially apply a reward.
        var totalSkillPoints = checked(player.SkillPoints + pointsGained);
        var result = new ExperienceResult(previousLevel, currentLevel,
            totalExperience - player.CurrentXp, totalExperience, pointsGained);

        player.CurrentXp = totalExperience;
        player.Level = currentLevel;
        player.SkillPoints = totalSkillPoints;
        return result;
    }
}
