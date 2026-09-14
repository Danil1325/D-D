using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

public interface IBattleDeckRepository
{
    Task<BattleDeck?> GetByIdAsync(int id);

    /// <summary>
    /// The runtime battle-deck built from a given source <see cref="Deck"/>, if one
    /// has been created. There is currently no persisted link from a
    /// <see cref="Battle"/> to its <see cref="BattleDeck"/> — see
    /// docs/ARCHITECTURE.md; callers resolve one via the source deck id instead.
    /// </summary>
    Task<BattleDeck?> GetByDeckIdAsync(int deckId);

    Task<BattleDeck> AddAsync(BattleDeck battleDeck);

    /// <summary>
    /// A no-op in MockData — the battle deck passed in is already the exact instance
    /// held in memory, so mutating it is enough. Kept on the interface so
    /// BusinessLayer's calling code doesn't need to change once DataAccessLayer's
    /// real SaveChangesAsync replaces this in Phase 7.
    /// </summary>
    Task UpdateAsync(BattleDeck battleDeck);
}
