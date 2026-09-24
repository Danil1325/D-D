using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Skills;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Skills;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Application-service implementation of the skill-tree catalog and the
/// per-player unlock state tracked against it. See <see cref="ISkillService"/>
/// for why no level/prerequisite gating exists yet.
/// </summary>
public class SkillService : ISkillService
{
    private readonly ISkillDefinitionRepository _skillDefinitionRepository;
    private readonly ICharacterSkillRepository _characterSkillRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly ICurrentPlayerService _currentPlayerService;

    public SkillService(
        ISkillDefinitionRepository skillDefinitionRepository,
        ICharacterSkillRepository characterSkillRepository,
        ICharacterRepository characterRepository,
        ICurrentPlayerService currentPlayerService)
    {
        _skillDefinitionRepository = skillDefinitionRepository;
        _characterSkillRepository = characterSkillRepository;
        _characterRepository = characterRepository;
        _currentPlayerService = currentPlayerService;
    }

    public async Task<CharacterSkillsDto> GetSkillTreeForCurrentPlayerAsync()
    {
        var character = await ResolveCurrentCharacterAsync();
        return await BuildCharacterSkillsAsync(character);
    }

    public async Task<CharacterSkillsDto> UnlockSkillForCurrentPlayerAsync(int skillDefinitionId)
    {
        var character = await ResolveCurrentCharacterAsync();

        var skill = await _skillDefinitionRepository.GetByIdAsync(skillDefinitionId);
        if (skill is null || skill.RaceId != character.RaceId || skill.ClassId != character.ClassId)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Skill {skillDefinitionId} was not found for this character.");
        }

        var existing = await _characterSkillRepository.GetAsync(character.Id, skillDefinitionId);
        if (existing is not null)
        {
            throw new DomainException(SkillErrorCodes.SkillAlreadyUnlocked, $"Skill {skillDefinitionId} is already unlocked.");
        }

        if (character.SkillPoints < skill.Cost)
        {
            throw new DomainException(SkillErrorCodes.InsufficientSkillPoints, $"Character {character.Id} does not have enough skill points to unlock skill {skillDefinitionId}.");
        }

        character.SkillPoints -= skill.Cost;
        await _characterRepository.UpdateAsync(character);

        await _characterSkillRepository.AddAsync(new CharacterSkillUnlock
        {
            PlayerId = character.Id,
            SkillDefinitionId = skillDefinitionId,
            UnlockedAt = DateTime.UtcNow
        });

        return await BuildCharacterSkillsAsync(character);
    }

    private async Task<PlayerCharacter> ResolveCurrentCharacterAsync()
    {
        var currentPlayerId = _currentPlayerService.GetCurrentPlayerId();
        var ownerId = currentPlayerId.ToString(CultureInfo.InvariantCulture);
        var characters = await _characterRepository.GetAllAsync();
        var character = characters.FirstOrDefault(candidate => candidate.OwnerId == ownerId);
        if (character is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {currentPlayerId}.");
        }

        return character;
    }

    private async Task<CharacterSkillsDto> BuildCharacterSkillsAsync(PlayerCharacter character)
    {
        var skills = await _skillDefinitionRepository.GetByRaceAndClassAsync(character.RaceId, character.ClassId);
        var unlocks = await _characterSkillRepository.GetByPlayerIdAsync(character.Id);
        var unlocksBySkillId = unlocks.ToDictionary(unlock => unlock.SkillDefinitionId);

        var skillDtos = skills
            .OrderBy(skill => skill.Id)
            .Select(skill => PlayerSkillDto.FromDomain(skill, unlocksBySkillId.GetValueOrDefault(skill.Id)))
            .ToList();

        return new CharacterSkillsDto
        {
            PlayerId = character.Id,
            AvailableSkillPoints = character.SkillPoints,
            Skills = skillDtos
        };
    }
}
