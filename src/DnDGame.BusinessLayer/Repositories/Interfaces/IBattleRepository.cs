using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

public interface IBattleRepository
{
    Task<Battle?> GetByIdAsync(int id);

    /// <summary>
    /// The in-progress battle for a game session, if one exists. A session has at
    /// most one active battle at a time.
    /// </summary>
    Task<Battle?> GetActiveByGameSessionIdAsync(int gameSessionId);

    /// <summary>Every battle (any status) recorded for a game session, including finished ones.</summary>
    Task<IReadOnlyList<Battle>> GetByGameSessionIdAsync(int gameSessionId);

    Task<Battle> AddAsync(Battle battle);

    /// <summary>
    /// A no-op in MockData — the battle passed in is already the exact instance held
    /// in memory, so mutating it is enough. Kept on the interface so BusinessLayer's
    /// calling code doesn't need to change once DataAccessLayer's real
    /// SaveChangesAsync replaces this in Phase 7.
    /// </summary>
    Task UpdateAsync(Battle battle);
}
