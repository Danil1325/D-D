using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Locations;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services.Interfaces;

namespace DnDGame.BusinessLayer.Services;

/// <summary>
/// Provides read-only location catalog data without duplicating scenario rules.
/// </summary>
public class LocationProgressionService : ILocationProgressionService
{
    private readonly ILocationRepository _locationRepository;

    public LocationProgressionService(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
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
}
