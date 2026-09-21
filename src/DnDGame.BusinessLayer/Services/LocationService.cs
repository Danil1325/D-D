using System.Globalization;
using System.Text;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.BusinessLayer.Validation;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Locations;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Application-service implementation of the location-progression catalogue. See
/// <see cref="ILocationService"/> for the enforced rules and error conventions.
/// </summary>
public sealed class LocationService : ILocationService
{
    // Mirrors QuestService's own private constants (see its "Location unlocks
    // (BACK-LOC-07)" section remarks on why they're duplicated here rather than
    // exposed from the engine: it keeps both services callers of
    // ILocationUnlockEngine instead of depending on each other).
    private static readonly int[] CrownFragmentQuestIds = { 7, 8, 9 };
    private const int HeroOverlookFinaleQuestId = 13;

    private readonly ILocationDefinitionRepository _locationDefinitionRepository;
    private readonly ILocationProgressRepository _locationProgressRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IPlayerQuestRepository _playerQuestRepository;
    private readonly IScenarioProgressRepository _scenarioProgressRepository;
    private readonly ILocationEncounterService _locationEncounterService;
    private readonly ILocationUnlockEngine _locationUnlockEngine;
    private readonly ILocationRouteProvider _locationRouteProvider;
    private readonly ICurrentPlayerService _currentPlayerService;

    public LocationService(
        ILocationDefinitionRepository locationDefinitionRepository,
        ILocationProgressRepository locationProgressRepository,
        ICharacterRepository characterRepository,
        IGameSessionRepository gameSessionRepository,
        IPlayerQuestRepository playerQuestRepository,
        IScenarioProgressRepository scenarioProgressRepository,
        ILocationEncounterService locationEncounterService,
        ILocationUnlockEngine locationUnlockEngine,
        ILocationRouteProvider locationRouteProvider,
        ICurrentPlayerService currentPlayerService)
    {
        _locationDefinitionRepository = locationDefinitionRepository;
        _locationProgressRepository = locationProgressRepository;
        _characterRepository = characterRepository;
        _gameSessionRepository = gameSessionRepository;
        _playerQuestRepository = playerQuestRepository;
        _scenarioProgressRepository = scenarioProgressRepository;
        _locationEncounterService = locationEncounterService;
        _locationUnlockEngine = locationUnlockEngine;
        _locationRouteProvider = locationRouteProvider;
        _currentPlayerService = currentPlayerService;
    }

    public async Task<IReadOnlyList<LocationSummaryDto>> GetAllLocationsAsync()
    {
        var definitions = await _locationDefinitionRepository.GetAllAsync();

        return definitions
            .OrderBy(definition => definition.Id)
            .Select(ToSummaryDto)
            .ToList();
    }

    public async Task<LocationDetailsDto> GetLocationByIdAsync(LocationId locationId)
    {
        RequireDefinedLocation(locationId);
        var definition = await RequireDefinitionAsync(locationId);

        return ToDetailsDto(definition);
    }

    public async Task<IReadOnlyList<LocationStatusDto>> GetProgressForPlayerAsync(int playerId)
    {
        var character = await RequireCharacterForPlayerAsync(playerId);
        var (_, progressByLocation) = await BuildUnlockContextAsync(character);
        var definitions = await _locationDefinitionRepository.GetAllAsync();

        return definitions
            .OrderBy(definition => definition.Id)
            .Select(definition => ToStatusDto(definition, progressByLocation))
            .ToList();
    }

    public async Task<IReadOnlyList<LocationRouteDto>> GetRouteForPlayerAsync(int playerId)
    {
        var character = await RequireCharacterForPlayerAsync(playerId);
        var race = RequireDefinedRace(character);
        var (_, progressByLocation) = await BuildUnlockContextAsync(character);
        var route = _locationRouteProvider.GetRecommendedRoute(race);
        var definitions = (await _locationDefinitionRepository.GetAllAsync()).ToDictionary(d => d.Id);

        return route.Steps
            .OrderBy(step => step.Order)
            .Select(step =>
            {
                progressByLocation.TryGetValue(step.LocationId, out var progress);
                definitions.TryGetValue(step.LocationId, out var definition);
                return new LocationRouteDto
                {
                    Order = step.Order,
                    LocationId = (int)step.LocationId,
                    LocationName = definition?.Name ?? step.LocationId.ToString(),
                    Status = StatusText(progress),
                    RecommendedLevel = definition?.RecommendedMinimumLevel ?? 1,
                    IsCurrent = progress?.Status == LocationStatus.Current,
                    IsCompleted = progress?.Completed ?? false
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<LocationEnemyDto>> GetAvailableEnemiesAsync(
        LocationId locationId, int playerId, string? subLocation = null)
    {
        RequireDefinedLocation(locationId);
        var character = await RequireCharacterForPlayerAsync(playerId);
        var sessionId = await ResolveSessionIdForCharacterAsync(character);

        var selection = await _locationEncounterService.GetAvailableEncountersAsync(
            new EncounterSelectionContext(locationId, sessionId, subLocation));

        return selection.AvailableEncounters
            .GroupBy(option => option.EnemyId)
            .Select(group => group.First())
            .Select(option => new LocationEnemyDto
            {
                Id = option.EnemyId,
                Name = option.EnemyName
            })
            .ToList();
    }

    public async Task<TravelToLocationResultDto> TravelToLocationAsync(TravelToLocationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequirePositiveId(request.PlayerId, "playerId");
        RequireDefinedLocation(request.LocationId);

        var character = await RequireCharacterForPlayerAsync(request.PlayerId);
        var (context, progressByLocation) = await BuildUnlockContextAsync(character);
        var definition = await RequireDefinitionAsync(request.LocationId);

        LocationUnlockResult outcome;
        if (context.UnlockedLocationIds.Contains(request.LocationId))
        {
            // Already unlocked: this is a plain re-visit, not a fresh unlock.
            // ILocationUnlockEngine.UnlockLocation would reject it (already unlocked),
            // so ask the engine only for the advisory data (available/next set) and
            // move CurrentLocationId ourselves.
            context.CurrentLocationId = request.LocationId;

            var availableResult = _locationUnlockEngine.GetAvailableLocations(context);
            if (!availableResult.Success || availableResult.Data is null)
            {
                throw new DomainException(
                    availableResult.ErrorCode ?? EngineErrorCodes.LocationInvalidContext, availableResult.Message);
            }

            var nextResult = _locationUnlockEngine.GetRecommendedNextLocation(context);

            outcome = new LocationUnlockResult
            {
                PlayerId = character.Id,
                LocationId = request.LocationId,
                Status = LocationStatus.Current,
                HeroOverlookFinaleUnlocked = context.CompletedQuestIds.Contains(HeroOverlookFinaleQuestId),
                AvailableLocationIds = availableResult.Data,
                RecommendedNextLocationId = nextResult.Data
            };
        }
        else
        {
            var unlockResult = _locationUnlockEngine.UnlockLocation(context, request.LocationId);
            if (!unlockResult.Success || unlockResult.Data is null)
            {
                throw new DomainException(
                    unlockResult.ErrorCode ?? EngineErrorCodes.LocationRequirementNotMet, unlockResult.Message);
            }

            outcome = unlockResult.Data;
        }

        await SetCurrentLocationAsync(progressByLocation, character, request.LocationId);

        return new TravelToLocationResultDto
        {
            PlayerId = outcome.PlayerId,
            LocationId = outcome.LocationId,
            LocationName = definition.Name,
            Status = outcome.Status,
            HeroOverlookFinaleUnlocked = outcome.HeroOverlookFinaleUnlocked,
            AvailableLocationIds = outcome.AvailableLocationIds,
            RecommendedNextLocationId = outcome.RecommendedNextLocationId
        };
    }

    // --- Context building ---

    /// <summary>
    /// Builds the LocationUnlockContext ILocationUnlockEngine needs, sourced from the
    /// player's already-persisted LocationProgress (never re-derived from quest
    /// completions here — that mapping is QuestService's job, see BACK-LOC-07) plus
    /// the session's completed quests/story flags, which the engine's own gating
    /// rules (guild registration, Crown Fragments, the finale) still need directly.
    /// </summary>
    private async Task<(LocationUnlockContext Context, Dictionary<LocationId, LocationProgress> ProgressByLocation)>
        BuildUnlockContextAsync(PlayerCharacter character)
    {
        var sessionId = await ResolveSessionIdForCharacterAsync(character);
        var playerQuests = await _playerQuestRepository.GetByGameSessionAsync(sessionId);
        var completedQuestIds = playerQuests
            .Where(pq => pq.Status == QuestStatus.Completed)
            .Select(pq => pq.QuestId)
            .ToHashSet();
        var scenarioProgress = await _scenarioProgressRepository.GetByGameSessionAsync(sessionId);

        var locationProgress = await _locationProgressRepository.GetByPlayerIdAsync(character.Id);
        var progressByLocation = locationProgress.ToDictionary(p => p.LocationId);

        var unlockedLocationIds = progressByLocation.Values
            .Where(p => p.Status != LocationStatus.Locked)
            .Select(p => p.LocationId)
            .ToList();
        var completedLocationIds = progressByLocation.Values
            .Where(p => p.Completed)
            .Select(p => p.LocationId)
            .ToList();
        var currentLocationId = progressByLocation.Values
            .FirstOrDefault(p => p.Status == LocationStatus.Current)?.LocationId;

        var context = new LocationUnlockContext
        {
            PlayerId = character.Id,
            Race = RequireDefinedRace(character),
            Level = character.Level,
            CompletedQuestIds = completedQuestIds.ToList(),
            StoryFlags = scenarioProgress?.StoryFlags ?? new Dictionary<string, bool>(),
            CrownFragmentCount = CrownFragmentQuestIds.Count(completedQuestIds.Contains),
            CurrentLocationId = currentLocationId,
            CompletedLocationIds = completedLocationIds,
            UnlockedLocationIds = unlockedLocationIds
        };

        return (context, progressByLocation);
    }

    private async Task<LocationId?> RecommendedNextLocationAsync(LocationUnlockContext context)
    {
        var result = _locationUnlockEngine.GetRecommendedNextLocation(context);
        if (!result.Success)
        {
            throw new DomainException(result.ErrorCode ?? EngineErrorCodes.LocationInvalidContext, result.Message);
        }

        return await Task.FromResult(result.Data);
    }

    /// <summary>
    /// Persists the outcome of a travel action: the previous Current location (if
    /// any) goes back to Available, and the destination becomes Current. Never
    /// touches LocationProgress.Completed — that stays QuestService's concern.
    /// </summary>
    private async Task SetCurrentLocationAsync(
        Dictionary<LocationId, LocationProgress> progressByLocation, PlayerCharacter character, LocationId newCurrentLocationId)
    {
        foreach (var existing in progressByLocation.Values.Where(p => p.Status == LocationStatus.Current && p.LocationId != newCurrentLocationId))
        {
            existing.Status = LocationStatus.Available;
            await _locationProgressRepository.UpdateAsync(existing);
        }

        if (progressByLocation.TryGetValue(newCurrentLocationId, out var target))
        {
            var changed = target.Status != LocationStatus.Current || !target.Visited;
            target.Status = LocationStatus.Current;
            target.Visited = true;
            if (changed)
            {
                await _locationProgressRepository.UpdateAsync(target);
            }

            return;
        }

        var created = new LocationProgress
        {
            PlayerId = character.Id,
            LocationId = newCurrentLocationId,
            Status = LocationStatus.Current,
            UnlockedAtLevel = character.Level,
            Visited = true
        };
        await _locationProgressRepository.AddAsync(created);
        progressByLocation[newCurrentLocationId] = created;
    }

    // --- Projection helpers ---

    private static LocationSummaryDto ToSummaryDto(LocationDefinition definition)
    {
        return new LocationSummaryDto
        {
            Id = (int)definition.Id,
            Slug = ToSlug(definition.Name),
            Name = definition.Name,
            RecommendedMinimumLevel = definition.RecommendedMinimumLevel,
            BackgroundImage = definition.BackgroundImage,
            IsSafeLocation = definition.IsSafeLocation
        };
    }

    private static LocationDetailsDto ToDetailsDto(LocationDefinition definition)
    {
        return new LocationDetailsDto
        {
            Id = (int)definition.Id,
            Slug = ToSlug(definition.Name),
            Name = definition.Name,
            Description = definition.Description,
            RecommendedMinimumLevel = definition.RecommendedMinimumLevel,
            BackgroundImage = definition.BackgroundImage,
            IsSafeLocation = definition.IsSafeLocation
        };
    }

    private static LocationStatusDto ToStatusDto(
        LocationDefinition definition,
        Dictionary<LocationId, LocationProgress> progressByLocation)
    {
        progressByLocation.TryGetValue(definition.Id, out var progress);
        return new LocationStatusDto
        {
            LocationId = (int)definition.Id,
            LocationName = definition.Name,
            Status = StatusText(progress),
            RecommendedLevel = definition.RecommendedMinimumLevel,
            IsCurrent = progress?.Status == LocationStatus.Current,
            IsCompleted = progress?.Completed ?? false
        };
    }

    private static string StatusText(LocationProgress? progress)
    {
        return (progress?.Status ?? LocationStatus.Locked).ToString();
    }

    private static string ToSlug(string name)
    {
        var slug = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in name.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                slug.Append(character);
                previousWasSeparator = false;
                continue;
            }

            if ((char.IsWhiteSpace(character) || character is '-' or '_') &&
                slug.Length > 0 &&
                !previousWasSeparator)
            {
                slug.Append('-');
                previousWasSeparator = true;
            }
        }

        if (slug.Length > 0 && slug[^1] == '-')
        {
            slug.Length--;
        }

        return slug.ToString();
    }

    // --- Resource resolution ---

    private async Task<PlayerCharacter> RequireCurrentCharacterAsync()
    {
        var currentPlayerId = _currentPlayerService.GetCurrentPlayerId();
        var character = await _characterRepository.GetByIdAsync(currentPlayerId);
        if (character is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Character {currentPlayerId} was not found.");
        }

        return character;
    }

    private async Task<PlayerCharacter> RequireCharacterForPlayerAsync(int playerId)
    {
        RequirePositiveId(playerId, "playerId");

        var characters = await _characterRepository.GetAllAsync();
        var character = characters.FirstOrDefault(c => c.OwnerId == playerId.ToString(CultureInfo.InvariantCulture));
        if (character is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {playerId}.");
        }

        return character;
    }

    private async Task<int> ResolveSessionIdForCharacterAsync(PlayerCharacter character)
    {
        var sessions = await _gameSessionRepository.GetByCharacterIdAsync(character.Id);
        var inProgressSessionId = sessions.FirstOrDefault(s => s.Status == GameSessionStatus.InProgress)?.Id;
        if (inProgressSessionId.HasValue)
        {
            return inProgressSessionId.Value;
        }

        var anySessionId = sessions.FirstOrDefault()?.Id;
        if (anySessionId.HasValue)
        {
            return anySessionId.Value;
        }

        throw new DomainException(ErrorCodes.NotFound, $"No game session was found for character {character.Id}.");
    }

    private async Task<LocationDefinition> RequireDefinitionAsync(LocationId locationId)
    {
        var definition = await _locationDefinitionRepository.GetByIdAsync(locationId);
        if (definition is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Location {locationId} was not found.");
        }

        return definition;
    }

    private static RaceType RequireDefinedRace(PlayerCharacter character)
    {
        if (!Enum.IsDefined((RaceType)character.RaceId))
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Character {character.Id} has an invalid or unset race and cannot be evaluated for location progress.");
        }

        return (RaceType)character.RaceId;
    }

    private static void RequireDefinedLocation(LocationId locationId)
    {
        if (!Enum.IsDefined(locationId))
        {
            throw new DomainException(ErrorCodes.ValidationError, $"Location {locationId} does not exist.");
        }
    }

    private static void RequirePositiveId(int id, string fieldName)
    {
        var validation = RequestValidationHelpers.RequirePositiveId(id, fieldName);
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }
    }
}
