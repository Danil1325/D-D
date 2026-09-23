using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Characters;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.BusinessLayer.Validation;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Races;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Character creation for New Game (Task: Character Creation). Every service that
/// keys game progress by player id (ProgressionService, ScenarioService, ...)
/// assumes exactly one character per owner, so a second character for the same
/// player is rejected before it can silently shadow the first one.
/// </summary>
public class CharacterService : ICharacterService
{
    private const int MinimumNameLength = 2;
    private const int MaximumNameLength = 40;

    private readonly ICharacterRepository _characterRepository;
    private readonly IRaceRepository _raceRepository;
    private readonly IClassRepository _classRepository;
    private readonly ICharacterPortraitRepository _portraitRepository;
    private readonly ICurrentPlayerService _currentPlayerService;
    private readonly IAchievementService _achievementService;

    public CharacterService(
        ICharacterRepository characterRepository,
        IRaceRepository raceRepository,
        IClassRepository classRepository,
        ICharacterPortraitRepository portraitRepository,
        ICurrentPlayerService currentPlayerService,
        IAchievementService achievementService)
    {
        _characterRepository = characterRepository;
        _raceRepository = raceRepository;
        _classRepository = classRepository;
        _portraitRepository = portraitRepository;
        _currentPlayerService = currentPlayerService;
        _achievementService = achievementService;
    }

    public async Task<CharacterResponseDto> CreateNewGameAsync(NewGameCharacterRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ThrowIfInvalid(ValidateNewGame(request));

        var ownerId = _currentPlayerService.GetCurrentPlayerId().ToString(CultureInfo.InvariantCulture);
        await EnsurePlayerHasNoCharacterAsync(ownerId);

        var race = await RequireRaceAsync((int)request.Race);
        var characterClass = await RequireClassAsync(request.ClassId);

        var character = new PlayerCharacter
        {
            OwnerId = ownerId,
            Name = request.Name.Trim(),
            RaceId = race.Id,
            Race = race,
            ClassId = characterClass.Id,
            CharacterClass = characterClass,
            Level = 1,
            CurrentXp = 0,
            SkillPoints = 0,
            CreatedAt = DateTime.UtcNow
        };
        RollStartingAttributes(character, race);

        var created = await _characterRepository.AddAsync(character);
        var portrait = await _portraitRepository.GetByRaceAndClassAsync(race.Id, characterClass.Id);

        // Character creation is itself an achievement event (Type.CharacterCreated).
        await _achievementService.RegisterCharacterCreatedAsync(created.Id);
        return CharacterResponseDto.FromDomain(created, portrait?.ImagePath);
    }

    public async Task<CharacterOptionsDto> GetOptionsAsync()
    {
        var races = await _raceRepository.GetAllAsync();
        var classes = await _classRepository.GetAllAsync();

        return new CharacterOptionsDto
        {
            Races = races.Select(RaceOptionDto.FromDomain).ToList(),
            Classes = classes.Select(ClassOptionDto.FromDomain).ToList()
        };
    }

    public async Task<CharacterResponseDto?> GetByIdAsync(int id)
    {
        var character = await _characterRepository.GetByIdAsync(id);
        if (character is null)
        {
            return null;
        }

        var portrait = await _portraitRepository.GetByRaceAndClassAsync(character.RaceId, character.ClassId);
        return CharacterResponseDto.FromDomain(character, portrait?.ImagePath);
    }

    public async Task<CharacterResponseDto> GetCurrentAsync()
    {
        var playerId = _currentPlayerService.GetCurrentPlayerId();
        var ownerId = playerId.ToString(CultureInfo.InvariantCulture);
        var characters = await _characterRepository.GetAllAsync();
        var character = characters.FirstOrDefault(candidate => candidate.OwnerId == ownerId);
        if (character is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {playerId}.");
        }

        var portrait = await _portraitRepository.GetByRaceAndClassAsync(character.RaceId, character.ClassId);
        return CharacterResponseDto.FromDomain(character, portrait?.ImagePath);
    }

    private async Task EnsurePlayerHasNoCharacterAsync(string ownerId)
    {
        var characters = await _characterRepository.GetAllAsync();
        if (characters.Any(character => character.OwnerId == ownerId))
        {
            throw new DomainException(ErrorCodes.Conflict, "The player already has a character. Only one character per player is allowed.");
        }
    }

    private async Task<Race> RequireRaceAsync(int raceId)
    {
        return await _raceRepository.GetByIdAsync(raceId)
            ?? throw new DomainException(CharacterErrorCodes.RaceNotFound, $"Race {raceId} was not found.");
    }

    private async Task<DnDGame.Domain.Entities.Classes.CharacterClass> RequireClassAsync(int classId)
    {
        return await _classRepository.GetByIdAsync(classId)
            ?? throw new DomainException(CharacterErrorCodes.ClassNotFound, $"Class {classId} was not found.");
    }

    private static void RollStartingAttributes(PlayerCharacter character, Race race)
    {
        character.MaxHealth = RollAttribute(race, AttributeType.Health);
        character.CurrentHealth = character.MaxHealth;
        character.Strength = RollAttribute(race, AttributeType.Strength);
        character.Dexterity = RollAttribute(race, AttributeType.Dexterity);
        character.Intelligence = RollAttribute(race, AttributeType.Intelligence);
        character.Charisma = RollAttribute(race, AttributeType.Charisma);
    }

    private static int RollAttribute(Race race, AttributeType attribute)
    {
        var range = race.AttributeRanges.FirstOrDefault(range => range.Attribute == attribute);
        if (range is null)
        {
            throw new DomainException(
                ErrorCodes.InternalError,
                $"Race {race.Id} has no attribute range for {attribute}.");
        }

        return Random.Shared.Next(range.MinValue, range.MaxValue + 1);
    }

    private static ValidationResult ValidateNewGame(NewGameCharacterRequestDto request)
    {
        return ValidationResult.Combine(
            RequestValidationHelpers.RequireNonEmpty(request.Name, "name"),
            ValidateNameLength(request.Name),
            ValidateRace(request.Race),
            RequestValidationHelpers.RequirePositiveId(request.ClassId, "classId"));
    }

    private static ValidationResult ValidateNameLength(string? name)
    {
        var trimmedLength = name?.Trim().Length ?? 0;
        return trimmedLength is >= MinimumNameLength and <= MaximumNameLength
            ? ValidationResult.Success()
            : ValidationResult.Failure($"name must be between {MinimumNameLength} and {MaximumNameLength} characters.");
    }

    private static ValidationResult ValidateRace(RaceType race)
    {
        return Enum.IsDefined(race)
            ? ValidationResult.Success()
            : ValidationResult.Failure($"race must be one of: {string.Join(", ", Enum.GetNames<RaceType>())}.");
    }

    private static void ThrowIfInvalid(ValidationResult validation)
    {
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }
    }
}