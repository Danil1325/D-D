using DnDGame.Domain.Entities.Classes;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to CharacterClass reference data.</summary>
public interface IClassRepository
{
    Task<IReadOnlyList<CharacterClass>> GetAllAsync();
    Task<CharacterClass?> GetByIdAsync(int id);
}
