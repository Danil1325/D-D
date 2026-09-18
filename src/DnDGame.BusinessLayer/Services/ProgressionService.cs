using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Progression;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Configuration;
using DnDGame.Domain.Entities.Characters;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Character progression surface. Grants experience through
/// <see cref="IExperienceService"/> and reports level, cumulative experience,
/// level thresholds and unspent skill points. No skill tree.
/// </summary>
public sealed class ProgressionService : IProgressionService
{
    private readonly ICharacterRepository _characterRepository;
    private readonly IExperienceService _experienceService;
    private readonly LevelProgressionRules _rules;

    public ProgressionService(
        ICharacterRepository characterRepository,
        IExperienceService experienceService,
        LevelProgressionRules rules)
    {
        _characterRepository = characterRepository;
        _experienceService = experienceService;
        _rules = rules;
    }

    public async Task<CharacterProgressionDto> GetProgressionAsync(int playerId)
    {
        var character = await RequireCharacterAsync(playerId);
        return ToDto(character);
    }

    public async Task<CharacterProgressionDto> GrantExperienceAsync(int playerId, int experience)
    {
        if (experience <= 0)
        {
            throw new DomainException(ErrorCodes.ValidationError, "experience must be a positive integer.");
        }

        var character = await RequireCharacterAsync(playerId);
        _experienceService.AddExperience(character, experience);
        await _characterRepository.UpdateAsync(character);

        return ToDto(character);
    }

    private async Task<PlayerCharacter> RequireCharacterAsync(int playerId)
    {
        var characters = await _characterRepository.GetAllAsync();
        return characters.FirstOrDefault(character => character.OwnerId == playerId.ToString(CultureInfo.InvariantCulture))
            ?? throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {playerId}.");
    }

    private CharacterProgressionDto ToDto(PlayerCharacter character)
    {
        var thresholds = _rules.ExperienceThresholds;
        var levelIndex = Math.Clamp(character.Level - 1, 0, thresholds.Count - 1);
        var currentThreshold = thresholds[levelIndex];

        var hasNextTier = character.Level < _rules.MaxLevel;
        var nextThreshold = hasNextTier ? thresholds[levelIndex + 1] : currentThreshold;
        var percentage = hasNextTier && nextThreshold > currentThreshold
            ? Math.Round(
                (double)(character.CurrentXp - currentThreshold) / (nextThreshold - currentThreshold) * 100,
                1,
                MidpointRounding.AwayFromZero)
            : 0;

        return new CharacterProgressionDto
        {
            Level = character.Level,
            CurrentExperience = character.CurrentXp,
            ExperienceForCurrentLevel = currentThreshold,
            ExperienceForNextLevel = hasNextTier ? nextThreshold : null,
            ExperienceProgressPercentage = Math.Clamp(percentage, 0, 100),
            AvailableSkillPoints = character.SkillPoints
        };
    }
}