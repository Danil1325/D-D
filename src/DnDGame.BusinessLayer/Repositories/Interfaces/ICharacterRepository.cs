using DnDGame.Domain.Entities.Characters;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

public interface ICharacterRepository
{
    Task<IReadOnlyList<PlayerCharacter>> GetAllAsync();
    Task<PlayerCharacter?> GetByIdAsync(int id);
    Task<PlayerCharacter> AddAsync(PlayerCharacter character);

    /// <summary>
    /// A no-op in MockData — the returned object from GetByIdAsync is already the
    /// exact instance held in memory, so mutating it is enough. Kept on the interface
    /// so BusinessLayer's calling code (character.Level++; await repo.UpdateAsync(character);)
    /// doesn't need to change once DataAccessLayer's real SaveChangesAsync replaces this
    /// in Phase 7.
    /// </summary>
    Task UpdateAsync(PlayerCharacter character);
}
