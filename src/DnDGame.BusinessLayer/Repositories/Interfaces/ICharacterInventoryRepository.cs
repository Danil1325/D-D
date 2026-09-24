using DnDGame.Domain.Entities.Shop;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>
/// Per-player owned-item quantities, composite-keyed (PlayerId, ShopItemId) —
/// the same shape IAchievementProgressRepository has for achievements.
/// </summary>
public interface ICharacterInventoryRepository
{
    Task<IReadOnlyList<CharacterInventoryEntry>> GetByPlayerIdAsync(int playerId);
    Task<CharacterInventoryEntry?> GetAsync(int playerId, int shopItemId);
    Task<CharacterInventoryEntry> AddAsync(CharacterInventoryEntry entry);

    /// <summary>
    /// A no-op in MockData — the row passed in is already the exact instance held
    /// in memory, so mutating it is enough. Kept on the interface so
    /// BusinessLayer's calling code doesn't change once DataAccessLayer's real
    /// SaveChangesAsync replaces this in Phase 7.
    /// </summary>
    Task UpdateAsync(CharacterInventoryEntry entry);
}
