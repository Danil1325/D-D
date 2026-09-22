using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to the location-progression catalogue (recommended level, special flags, unlock requirements).</summary>
public interface ILocationDefinitionRepository
{
    Task<IReadOnlyList<LocationDefinition>> GetAllAsync();
    Task<LocationDefinition?> GetByIdAsync(LocationId locationId);
}
