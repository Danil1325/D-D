using DndGame.Domain.Entities;

namespace DndGame.BusinessLayer.Interfaces;

public interface ICharacterRepository
{
    Task<IReadOnlyList<Character>> GetAllAsync(CancellationToken cancellationToken);
    Task<Character?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(Character character, CancellationToken cancellationToken);
    void Update(Character character);
    void Delete(Character character);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
