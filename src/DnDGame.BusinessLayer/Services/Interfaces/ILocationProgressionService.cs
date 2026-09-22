using DnDGame.BusinessLayer.Dtos.Locations;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Read-only location catalog surface for frontend location progression work.
/// </summary>
public interface ILocationProgressionService
{
    Task<IReadOnlyList<LocationSummaryDto>> GetAllLocationsAsync();
    Task<LocationDetailsDto> GetLocationDetailsAsync(int locationId);
    Task<IReadOnlyList<LocationEnemyDto>> GetLocationEnemiesAsync(int locationId, int playerId);
    Task<TravelToLocationResultDto> TravelToLocationAsync(TravelToLocationRequestDto request);
    Task<IReadOnlyList<LocationRouteStepDto>> GetPlayerRouteAsync(int playerId);
    Task<IReadOnlyList<LocationStatusDto>> GetPlayerLocationStatusesAsync(int playerId);
}
