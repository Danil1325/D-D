using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Application-service surface for the location-progression catalogue
/// (BACK-LOC-08). Read endpoints project LocationDefinition + the requesting
/// player's LocationProgress; the travel endpoint is the first caller that
/// actually persists ILocationUnlockEngine's unlock/current-location outcome.
/// Every method resolves its own player/character — controllers never take one
/// as a raw id beyond what the route already carries.
/// </summary>
public interface ILocationService
{
    /// <summary>The full catalogue, projected for the current player (ICurrentPlayerService).</summary>
    Task<IReadOnlyList<LocationSummaryDto>> GetAllLocationsAsync();

    /// <summary>One location's full detail, projected for the current player.</summary>
    Task<LocationDetailsDto> GetLocationByIdAsync(LocationId locationId);

    /// <summary>A specific player's whole progression snapshot.</summary>
    Task<LocationProgressDto> GetProgressForPlayerAsync(int playerId);

    /// <summary>A specific player's race-recommended route, with their status for each step.</summary>
    Task<LocationRouteDto> GetRouteForPlayerAsync(int playerId);

    /// <summary>
    /// The enemies a specific player can currently fight at a location. Delegates to
    /// ILocationEncounterService, so the same filtering (level gate, one-time bosses,
    /// availability requirements) is enforced — never reimplemented here.
    /// </summary>
    Task<IReadOnlyList<LocationEnemyDto>> GetAvailableEnemiesAsync(LocationId locationId, int playerId, string? subLocation = null);

    /// <summary>
    /// Moves a player to a location: unlocks it first via ILocationUnlockEngine if it
    /// isn't already unlocked, then records it as the player's current location.
    /// </summary>
    Task<TravelToLocationResultDto> TravelToLocationAsync(TravelToLocationRequest request);
}
