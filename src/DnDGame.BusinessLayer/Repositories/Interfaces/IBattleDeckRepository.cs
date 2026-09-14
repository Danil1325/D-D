using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

public interface IBattleDeckRepository
{
    Task<BattleDeck?> GetByIdAsync(int id);

    /// <summary>
    /// The runtime battle-deck for a given battle. One battle has exactly one
    /// battle deck (the human player's).
    /// </summary>
    Task<BattleDeck?> GetByBattleIdAsync(int battleId);

    Task<BattleDeck> AddAsync(BattleDeck battleDeck);

    /// <summary>
    /// A no-op in MockData — the battle deck passed in is already the exact instance
    /// held in memory, so mutating it is enough. Kept on the interface so
    /// BusinessLayer's calling code doesn't need to change once DataAccessLayer's
    /// real SaveChangesAsync replaces this in Phase 7.
    /// </summary>
    Task UpdateAsync(BattleDeck battleDeck);
}
