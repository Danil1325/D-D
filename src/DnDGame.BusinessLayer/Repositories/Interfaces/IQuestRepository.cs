using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to the quest catalog.</summary>
public interface IQuestRepository
{
    Task<IReadOnlyList<Quest>> GetAllAsync();
    Task<Quest?> GetByIdAsync(int id);
}