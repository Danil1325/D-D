using DndGame.Domain.Entities;

namespace DndGame.BusinessLayer.Interfaces;

public interface ICharacterService
{
    Task<IReadOnlyList<Character>> GetAllAsync(CancellationToken cancellationToken);
    Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Character> CreateAsync(Character character, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(int id, Character character, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
