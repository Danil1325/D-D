using DnDGame.BusinessLayer.Dtos.Scenarios;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Application service over <see cref="DnDGame.Domain.Engine.Scenario.IScenarioEngine"/>.
/// It loads the player's character/session and the scenario catalog, delegates the
/// state transitions to the engine, persists the mutated entities and maps the
/// results to DTOs. Expected failures throw DomainException with BusinessLayer
/// ErrorCodes so the global middleware can map them to correct HTTP status codes.
/// </summary>
public interface IScenarioService
{
    /// <summary>
    /// The current scene for the player's in-progress run, with only the choices
    /// that are currently available to them.
    /// </summary>
    Task<StorySceneDto> GetCurrentAsync(int playerId);

    /// <summary>Starts a fresh scenario run at the catalog's entry scene (id 900).</summary>
    Task<ScenarioProgressDto> StartAsync(int playerId);

    /// <summary>Selects a choice in the given scene and advances the run.</summary>
    Task<ScenarioProgressDto> SelectChoiceAsync(SelectChoiceRequest request);

    /// <summary>All locations of the world map.</summary>
    Task<IReadOnlyList<LocationDto>> GetLocationsAsync();

    /// <summary>A single location.</summary>
    Task<LocationDto> GetLocationAsync(int locationId);
}