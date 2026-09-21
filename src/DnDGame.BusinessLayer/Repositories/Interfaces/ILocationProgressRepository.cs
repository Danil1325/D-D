using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Per-player location unlock/completion state (BACK-LOC-07).</summary>
public interface ILocationProgressRepository
{
    Task<IReadOnlyList<LocationProgress>> GetByPlayerIdAsync(int playerId);
    Task<LocationProgress?> GetAsync(int playerId, LocationId locationId);
    Task<LocationProgress> AddAsync(LocationProgress progress);

    /// <summary>
    /// A no-op in MockData — the row passed in is already the exact instance held in
    /// memory, so mutating it is enough. Kept on the interface so BusinessLayer's
    /// calling code doesn't need to change once DataAccessLayer's real
    /// SaveChangesAsync replaces this in Phase 7.
    /// </summary>
    Task UpdateAsync(LocationProgress progress);
}
