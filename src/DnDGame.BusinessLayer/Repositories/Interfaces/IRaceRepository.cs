using DnDGame.Domain.Entities.Races;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>
/// Read access to Race reference data. Implemented by MockData now, and later by
/// DataAccessLayer (Phase 6+) via EF Core — BusinessLayer only ever depends on this
/// interface, never on either implementation directly.
/// </summary>
public interface IRaceRepository
{
    Task<IReadOnlyList<Race>> GetAllAsync();
    Task<Race?> GetByIdAsync(int id);
}
