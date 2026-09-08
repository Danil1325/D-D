using DnDGame.Domain.Entities.Talents;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to Talent reference data.</summary>
public interface ITalentRepository
{
    Task<IReadOnlyList<Talent>> GetAllAsync();
    Task<Talent?> GetByIdAsync(int id);
}
