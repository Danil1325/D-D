using System.Globalization;
using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Dtos.Scenarios;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.BusinessLayer.Validation;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Provides read-only location catalog data without duplicating scenario rules.
/// </summary>
public class LocationProgressionService : ILocationProgressionService
{
    private const string CompletedStatus = "completed";
    private const string CurrentStatus = "current";
    private const string UpcomingStatus = "upcoming";
    private const int OpeningOrder = 1;
    private const int RaceSpecificOrder = 2;
    private const int MisthavenOrder = 3;
    private const int OakheavenOrder = 4;
    private const int AshtoniaFragmentOrder = 5;
    private const int WhisperingWoodsFragmentOrder = 6;
    private const int BonePeaksFragmentOrder = 7;
    private const int DarkstormKeepOrder = 8;
    private const int FinalReturnOrder = 9;

    private readonly ILocationRepository _locationRepository;
    private readonly IScenarioService _scenarioService;
    private readonly IStorySceneRepository _sceneRepository;
    private readonly ICharacterRepository _characterRepository;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IScenarioProgressRepository _progressRepository;

    public LocationProgressionService(
        ILocationRepository locationRepository,
        IScenarioService scenarioService,
        IStorySceneRepository sceneRepository,
        ICharacterRepository characterRepository,
        IGameSessionRepository gameSessionRepository,
        IScenarioProgressRepository progressRepository)
    {
        _locationRepository = locationRepository;
        _scenarioService = scenarioService;
        _sceneRepository = sceneRepository;
        _characterRepository = characterRepository;
        _gameSessionRepository = gameSessionRepository;
        _progressRepository = progressRepository;
    }

    public async Task<IReadOnlyList<LocationSummaryDto>> GetAllLocationsAsync()
    {
        var locations = await _locationRepository.GetAllAsync();
        return locations.Select(LocationSummaryDto.FromDomain).ToList();
    }

    public async Task<LocationDetailsDto> GetLocationDetailsAsync(int locationId)
    {
        var location = await _locationRepository.GetByIdAsync(locationId)
            ?? throw new DomainException(ErrorCodes.NotFound, $"Location {locationId} was not found.");

        return LocationDetailsDto.FromDomain(location);
    }

    public async Task<TravelToLocationResultDto> TravelToLocationAsync(TravelToLocationRequestDto request)
    {
        if (request is null)
        {
            throw new DomainException(ErrorCodes.ValidationError, "Travel request is required.");
        }

        var validation = ValidationResult.Combine(
            RequestValidationHelpers.RequirePositiveId(request.PlayerId, nameof(request.PlayerId)),
            RequestValidationHelpers.RequirePositiveId(request.LocationId, nameof(request.LocationId)));

        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }

        if (await _locationRepository.GetByIdAsync(request.LocationId) is null)
        {
            throw new DomainException(ErrorCodes.NotFound, $"Location {request.LocationId} was not found.");
        }

        var currentScene = await _scenarioService.GetCurrentAsync(request.PlayerId);
        if (request.LocationId == currentScene.LocationId)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Location {request.LocationId} is already the current location.");
        }

        var scenes = await _sceneRepository.GetAllAsync();

        var matchingChoices = currentScene.Choices
            .Where(choice => choice.NextSceneId.HasValue)
            .Select(choice => new
            {
                Choice = choice,
                Destination = scenes.FirstOrDefault(scene => scene.Id == choice.NextSceneId.GetValueOrDefault())
            })
            .Where(transition => transition.Destination?.LocationId == request.LocationId)
            .ToList();

        if (matchingChoices.Count == 0)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Location {request.LocationId} is not currently reachable from scene {currentScene.Id}.");
        }

        if (matchingChoices.Count > 1)
        {
            throw new DomainException(
                ErrorCodes.Conflict,
                $"Location {request.LocationId} is reachable through multiple current scenario choices.");
        }

        await _scenarioService.SelectChoiceAsync(new SelectChoiceRequest
        {
            PlayerId = request.PlayerId,
            SceneId = currentScene.Id,
            ChoiceId = matchingChoices[0].Choice.Id
        });

        var resultingScene = await _scenarioService.GetCurrentAsync(request.PlayerId);
        var currentLocation = await _locationRepository.GetByIdAsync(resultingScene.LocationId)
            ?? throw new DomainException(ErrorCodes.NotFound, $"Location {resultingScene.LocationId} was not found.");

        return new TravelToLocationResultDto
        {
            CurrentLocation = LocationDetailsDto.FromDomain(currentLocation),
            CurrentScene = resultingScene
        };
    }

    public async Task<IReadOnlyList<LocationRouteStepDto>> GetPlayerRouteAsync(int playerId)
    {
        var validation = RequestValidationHelpers.RequirePositiveId(playerId, nameof(playerId));
        if (!validation.IsValid)
        {
            throw new DomainException(ErrorCodes.ValidationError, string.Join(" ", validation.Errors));
        }

        var character = await RequireCharacterAsync(playerId);
        var session = await RequireSessionAsync(character.Id);
        var progress = await RequireProgressAsync(session.Id);
        var scenes = await _sceneRepository.GetAllAsync();
        var currentScene = scenes.FirstOrDefault(scene => scene.Id == progress.CurrentSceneId)
            ?? throw new DomainException(
                ErrorCodes.NotFound,
                $"Scene {progress.CurrentSceneId} was not found in the catalog.");

        var routeLocationIds = CreateRouteLocationIds(character.RaceId);
        var currentPosition = ResolveCurrentRoutePosition(currentScene);
        var locations = await _locationRepository.GetAllAsync();
        var locationsById = locations.ToDictionary(location => location.Id);

        return routeLocationIds
            .Select((locationId, index) =>
            {
                if (!locationsById.TryGetValue(locationId, out var location))
                {
                    throw new DomainException(ErrorCodes.NotFound, $"Location {locationId} was not found.");
                }

                var order = index + 1;
                var isCurrent = order == currentPosition.CurrentOrder;
                var isCompleted = order <= currentPosition.CompletedThroughOrder;

                return new LocationRouteStepDto
                {
                    Order = order,
                    LocationId = location.Id,
                    LocationName = location.Name,
                    Status = isCurrent ? CurrentStatus : isCompleted ? CompletedStatus : UpcomingStatus,
                    RecommendedLevel = location.RecommendedMinimumLevel,
                    IsCurrent = isCurrent,
                    IsCompleted = isCompleted
                };
            })
            .ToList();
    }

    private async Task<PlayerCharacter> RequireCharacterAsync(int playerId)
    {
        var characters = await _characterRepository.GetAllAsync();
        return characters.FirstOrDefault(character => character.OwnerId == playerId.ToString(CultureInfo.InvariantCulture))
            ?? throw new DomainException(ErrorCodes.NotFound, $"No character was found for player {playerId}.");
    }

    private async Task<GameSession> RequireSessionAsync(int characterId)
    {
        var sessions = await _gameSessionRepository.GetByCharacterIdAsync(characterId);
        return sessions.FirstOrDefault(session => session.Status == GameSessionStatus.InProgress)
            ?? sessions.FirstOrDefault()
            ?? throw new DomainException(ErrorCodes.NotFound, $"No game session was found for character {characterId}.");
    }

    private async Task<ScenarioProgress> RequireProgressAsync(int gameSessionId)
    {
        return await _progressRepository.GetByGameSessionAsync(gameSessionId)
            ?? throw new DomainException(ErrorCodes.NotFound, "No scenario has been started for this player yet.");
    }

    private static IReadOnlyList<int> CreateRouteLocationIds(int raceId)
    {
        return new[]
        {
            3,
            RaceSpecificLocationId(raceId),
            4,
            5,
            1,
            7,
            6,
            2,
            3
        };
    }

    private static int RaceSpecificLocationId(int raceId) => raceId switch
    {
        1 => 4,
        2 => 7,
        3 => 1,
        4 => 6,
        _ => throw new DomainException(ErrorCodes.Conflict, $"Race {raceId} does not have an authored route location.")
    };

    private static RoutePosition ResolveCurrentRoutePosition(StoryScene currentScene)
    {
        if (currentScene.Id is >= 900 and <= 1004)
        {
            return CurrentRouteStep(OpeningOrder);
        }

        if (currentScene.Id == 1205)
        {
            return BetweenRouteSteps(OpeningOrder);
        }

        if (currentScene.Id is >= 2003 and < 3005)
        {
            return CurrentRouteStep(MisthavenOrder);
        }

        if (currentScene.Id is >= 1201 and <= 1242)
        {
            return CurrentRouteStep(RaceSpecificOrder);
        }

        if (currentScene.Id is >= 3005 and < 4008)
        {
            return CurrentRouteStep(OakheavenOrder);
        }

        if (currentScene.Id == 4100)
        {
            return BetweenRouteSteps(OakheavenOrder);
        }

        if (currentScene.Id is >= 4008 and < 5000)
        {
            return CurrentRouteStep(OrderOfFragmentLocation(currentScene.LocationId));
        }

        if (currentScene.Id is >= 5012 and < 6014)
        {
            return CurrentRouteStep(DarkstormKeepOrder);
        }

        if (currentScene.Id is >= 6014 and <= 6017)
        {
            return CurrentRouteStep(FinalReturnOrder);
        }

        if (currentScene.Id is 6101 or 6103 or 6105 or 6106 or 6107 or 6108)
        {
            return CurrentRouteStep(FinalReturnOrder);
        }

        if (currentScene.Id == 6102)
        {
            return CurrentRouteStep(DarkstormKeepOrder);
        }

        if (currentScene.Id == 6104)
        {
            return CurrentRouteStep(BonePeaksFragmentOrder);
        }

        throw new DomainException(ErrorCodes.Conflict, $"Scene {currentScene.Id} is not part of the authored route.");
    }

    private static RoutePosition CurrentRouteStep(int order) => new(order, order - 1);

    private static RoutePosition BetweenRouteSteps(int completedThroughOrder) => new(null, completedThroughOrder);

    private static int OrderOfFragmentLocation(int locationId) => locationId switch
    {
        1 => AshtoniaFragmentOrder,
        7 => WhisperingWoodsFragmentOrder,
        6 => BonePeaksFragmentOrder,
        _ => throw new DomainException(ErrorCodes.Conflict, $"Location {locationId} is not part of the authored fragment route.")
    };

    private sealed record RoutePosition(int? CurrentOrder, int CompletedThroughOrder);
}
