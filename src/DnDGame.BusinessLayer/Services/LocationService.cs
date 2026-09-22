using System.Globalization;
using System.Text;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Dtos.Scenarios;
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
    private const int OpeningRouteOrder = 1;
    private const int RaceSpecificRouteOrder = 2;
    private const int MisthavenRouteOrder = 3;
    private const int OakheavenRouteOrder = 4;
    private const int AshtoniaFragmentRouteOrder = 5;
    private const int WhisperingWoodsFragmentRouteOrder = 6;
    private const int BonePeaksFragmentRouteOrder = 7;
    private const int DarkstormKeepRouteOrder = 8;
    private const int FinalReturnRouteOrder = 9;

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
    private readonly IScenarioService _scenarioService;
    private readonly IStorySceneRepository _storySceneRepository;

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
        ICurrentPlayerService currentPlayerService,
        IScenarioService scenarioService,
        IStorySceneRepository storySceneRepository)
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
        _scenarioService = scenarioService;
        _storySceneRepository = storySceneRepository;
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
        var currentScene = await GetCurrentStorySceneAsync(character);
        var routePhase = currentScene is null ? null : ResolveRoutePhase(currentScene);

        return route.Steps
            .OrderBy(step => step.Order)
            .Select(step =>
            {
                progressByLocation.TryGetValue(step.LocationId, out var progress);
                definitions.TryGetValue(step.LocationId, out var definition);
                var isCurrent = routePhase?.CurrentOrder == step.Order;
                var isCompleted = routePhase is not null && step.Order < routePhase.ProgressOrder;

                return new LocationRouteDto
                {
                    Order = step.Order,
                    LocationId = (int)step.LocationId,
                    LocationName = definition?.Name ?? step.LocationId.ToString(),
                    Status = RouteStatusText(step, routePhase, progress),
                    RecommendedLevel = definition?.RecommendedMinimumLevel ?? 1,
                    IsCurrent = isCurrent,
                    IsCompleted = isCompleted
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
        RequirePositiveId((int)request.LocationId, "locationId");
        RequireDefinedLocation(request.LocationId);

        var character = await RequireCharacterForPlayerAsync(request.PlayerId);
        await RequireDefinitionAsync(request.LocationId);

        var currentScene = await _scenarioService.GetCurrentAsync(request.PlayerId);
        if (currentScene.LocationId == (int)request.LocationId)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Player {request.PlayerId} is already at location {(int)request.LocationId}.");
        }

        var matchingChoices = await FindChoicesLeadingToLocationAsync(currentScene, request.LocationId);
        if (matchingChoices.Count == 0)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Location {(int)request.LocationId} is not reachable from the current scene.");
        }

        if (matchingChoices.Count > 1)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Location {(int)request.LocationId} is reachable through more than one current choice.");
        }

        await _scenarioService.SelectChoiceAsync(new SelectChoiceRequest
        {
            PlayerId = request.PlayerId,
            SceneId = currentScene.Id,
            ChoiceId = matchingChoices[0].Id
        });

        var resultingScene = await _scenarioService.GetCurrentAsync(request.PlayerId);
        var resultingLocationId = ToLocationId(resultingScene.LocationId);
        var currentLocation = await GetLocationByIdAsync(resultingLocationId);

        var progressByLocation = (await _locationProgressRepository.GetByPlayerIdAsync(character.Id))
            .ToDictionary(progress => progress.LocationId);
        await SetCurrentLocationAsync(progressByLocation, character, resultingLocationId);

        return new TravelToLocationResultDto
        {
            CurrentLocation = currentLocation,
            CurrentScene = resultingScene
        };
    }

    // --- Context building ---

    private async Task<IReadOnlyList<ChoiceDto>> FindChoicesLeadingToLocationAsync(
        StorySceneDto currentScene,
        LocationId locationId)
    {
        var matches = new List<ChoiceDto>();
        foreach (var choice in currentScene.Choices.Where(choice => choice.NextSceneId.HasValue))
        {
            var destinationScene = await _storySceneRepository.GetByIdAsync(choice.NextSceneId!.Value)
                ?? throw new DomainException(
                    ErrorCodes.NotFound,
                    $"Scene {choice.NextSceneId.Value} was not found in the catalog.");

            if (destinationScene.LocationId == (int)locationId)
            {
                matches.Add(choice);
            }
        }

        return matches;
    }

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

    private async Task<StoryScene?> GetCurrentStorySceneAsync(PlayerCharacter character)
    {
        var sessionId = await ResolveSessionIdForCharacterAsync(character);
        var scenarioProgress = await _scenarioProgressRepository.GetByGameSessionAsync(sessionId);
        if (scenarioProgress is null || scenarioProgress.CurrentSceneId <= 0)
        {
            return null;
        }

        return await _storySceneRepository.GetByIdAsync(scenarioProgress.CurrentSceneId)
            ?? throw new DomainException(
                ErrorCodes.NotFound,
                $"Scene {scenarioProgress.CurrentSceneId} was not found in the catalog.");
    }

    private static RoutePresentationPhase ResolveRoutePhase(StoryScene currentScene)
    {
        return currentScene.Id switch
        {
            >= 900 and <= 1004 => CurrentRouteStep(OpeningRouteOrder),
            1205 => BetweenRouteSteps(OpeningRouteOrder),
            >= 1201 and <= 1242 => CurrentRouteStep(RaceSpecificRouteOrder),
            >= 2003 and < 3005 => CurrentRouteStep(MisthavenRouteOrder),
            >= 3005 and < 4008 => CurrentRouteStep(OakheavenRouteOrder),
            4100 => BetweenRouteSteps(OakheavenRouteOrder),
            >= 4008 and < 5000 => CurrentRouteStep(OrderOfFragmentLocation((LocationId)currentScene.LocationId)),
            >= 5012 and < 6014 => CurrentRouteStep(DarkstormKeepRouteOrder),
            >= 6014 and <= 6017 => CurrentRouteStep(FinalReturnRouteOrder),
            6101 or 6103 or 6105 or 6106 or 6107 or 6108 => CurrentRouteStep(FinalReturnRouteOrder),
            6102 => CurrentRouteStep(DarkstormKeepRouteOrder),
            6104 => CurrentRouteStep(BonePeaksFragmentRouteOrder),
            _ => throw new DomainException(ErrorCodes.Conflict, $"Scene {currentScene.Id} is not part of the authored route.")
        };
    }

    private static RoutePresentationPhase CurrentRouteStep(int order) => new(order, order);

    private static RoutePresentationPhase BetweenRouteSteps(int completedThroughOrder) => new(completedThroughOrder + 1, null);

    private static int OrderOfFragmentLocation(LocationId locationId)
    {
        return locationId switch
        {
            LocationId.Ashtonia => AshtoniaFragmentRouteOrder,
            LocationId.WhisperingWoods => WhisperingWoodsFragmentRouteOrder,
            LocationId.TheBonePeaks => BonePeaksFragmentRouteOrder,
            _ => throw new DomainException(
                ErrorCodes.Conflict,
                $"Location {locationId} is not part of the authored fragment route.")
        };
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

    private static string RouteStatusText(
        LocationRouteStep step,
        RoutePresentationPhase? routePhase,
        LocationProgress? progress)
    {
        if (routePhase is null)
        {
            var fallbackStatus = progress?.Status ?? LocationStatus.Locked;
            return fallbackStatus is LocationStatus.Current or LocationStatus.Completed
                ? LocationStatus.Available.ToString()
                : fallbackStatus.ToString();
        }

        if (routePhase.CurrentOrder == step.Order)
        {
            return LocationStatus.Current.ToString();
        }

        if (step.Order < routePhase.ProgressOrder)
        {
            return LocationStatus.Completed.ToString();
        }

        var status = progress?.Status ?? LocationStatus.Locked;
        return status is LocationStatus.Current or LocationStatus.Completed
            ? LocationStatus.Available.ToString()
            : status.ToString();
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

    private static LocationId ToLocationId(int locationId)
    {
        if (!Enum.IsDefined(typeof(LocationId), locationId))
        {
            throw new DomainException(ErrorCodes.ValidationError, $"Location {locationId} does not exist.");
        }

        return (LocationId)locationId;
    }

    private static void RequirePositiveId(int id, string fieldName)
    {
        var validation = RequestValidationHelpers.RequirePositiveId(id, fieldName);
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }
    }

    private sealed record RoutePresentationPhase(int ProgressOrder, int? CurrentOrder);
}
