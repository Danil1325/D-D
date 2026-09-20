using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Dtos.Scenarios;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.BusinessLayer.Validation;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Provides read-only location catalog data without duplicating scenario rules.
/// </summary>
public class LocationProgressionService : ILocationProgressionService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IScenarioService _scenarioService;
    private readonly IStorySceneRepository _sceneRepository;

    public LocationProgressionService(
        ILocationRepository locationRepository,
        IScenarioService scenarioService,
        IStorySceneRepository sceneRepository)
    {
        _locationRepository = locationRepository;
        _scenarioService = scenarioService;
        _sceneRepository = sceneRepository;
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
}
