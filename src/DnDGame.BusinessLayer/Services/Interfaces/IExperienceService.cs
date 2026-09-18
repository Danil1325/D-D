using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Characters;

namespace DnDGame.BusinessLayer.Services.Interfaces;

public interface IExperienceService
{
    /// <summary>
    /// Applies a non-negative EXP reward from a quest or battle to the character.
    /// EXP is cumulative and saturates at int.MaxValue; ExperienceGained reports
    /// the amount actually stored. The caller owns reward deduplication and persistence.
    /// </summary>
    ExperienceResult AddExperience(PlayerCharacter player, int experience);
}
