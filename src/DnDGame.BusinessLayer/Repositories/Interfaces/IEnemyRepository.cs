using DnDGame.Domain.Entities.Enemies;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to Enemy reference data (the 21 seeded monsters).</summary>
public interface IEnemyRepository
{
    Task<IReadOnlyList<Enemy>> GetAllAsync();
    Task<Enemy?> GetByIdAsync(int id);
}
