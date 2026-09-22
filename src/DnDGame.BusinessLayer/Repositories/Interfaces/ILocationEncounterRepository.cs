using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to each location's configured enemy encounter pool (BACK-LOC-05).</summary>
public interface ILocationEncounterRepository
{
    Task<LocationEncounterDefinition?> GetByLocationIdAsync(LocationId locationId);
}
