using DnDGame.Domain.Entities.Achievements;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>The seeded achievements catalog (reference data).</summary>
public interface IAchievementRepository
{
    Task<IReadOnlyList<Achievement>> GetAllAsync();
    Task<IReadOnlyList<Achievement>> GetByTypeAsync(AchievementType type);
}