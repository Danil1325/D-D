using DnDGame.Domain.Entities.Achievements;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>
/// Per-player achievement progress, composite-keyed (PlayerId, AchievementId) —
/// the same shape ILocationProgressRepository has for locations.
/// </summary>
public interface IAchievementProgressRepository
{
    Task<IReadOnlyList<AchievementProgress>> GetByPlayerIdAsync(int playerId);
    Task<AchievementProgress?> GetAsync(int playerId, int achievementId);
    Task<AchievementProgress> AddAsync(AchievementProgress progress);

    /// <summary>
    /// A no-op in MockData — the row passed in is already the exact instance held in
    /// memory, so mutating it is enough. Kept on the interface so BusinessLayer's
    /// calling code doesn't change once DataAccessLayer's real SaveChangesAsync
    /// replaces this in Phase 7.
    /// </summary>
    Task UpdateAsync(AchievementProgress progress);
}