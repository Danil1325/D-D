using DnDGame.BusinessLayer.Dtos.Locations;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Read-only location catalog surface for frontend location progression work.
/// </summary>
public interface ILocationProgressionService
{
    Task<IReadOnlyList<LocationSummaryDto>> GetAllLocationsAsync();
    Task<LocationDetailsDto> GetLocationDetailsAsync(int locationId);
    Task<TravelToLocationResultDto> TravelToLocationAsync(TravelToLocationRequestDto request);
    Task<IReadOnlyList<LocationRouteStepDto>> GetPlayerRouteAsync(int playerId);
}
