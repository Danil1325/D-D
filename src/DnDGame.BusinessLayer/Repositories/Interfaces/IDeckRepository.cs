using DnDGame.Domain.Entities.Cards;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

public interface IDeckRepository
{
    Task<IReadOnlyList<Deck>> GetAllByCharacterIdAsync(int characterId);

    Task<Deck?> GetByIdAsync(int id);

    Task<Deck> AddAsync(Deck deck);

    /// <summary>
    /// A no-op in MockData — the deck passed in is already the exact instance held
    /// in memory, so mutating it is enough. Kept on the interface so BusinessLayer's
    /// calling code doesn't need to change once DataAccessLayer's real
    /// SaveChangesAsync replaces this in Phase 7.
    /// </summary>
    Task UpdateAsync(Deck deck);

    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Resolves card catalogue entries by id, for building/updating a deck's card
    /// list from the ids a request supplies. There is no dedicated card-catalogue
    /// repository yet, so this lives here rather than being guessed at elsewhere.
    /// </summary>
    Task<IReadOnlyList<Card>> GetCardsByIdsAsync(IEnumerable<int> cardIds);
}
