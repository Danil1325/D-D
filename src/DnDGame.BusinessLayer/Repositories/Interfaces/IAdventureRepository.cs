using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>
/// Read access to Adventures. Implementations are expected to return an Adventure
/// with its full StoryNode/Choice graph already attached (an aggregate load), similar
/// to what an EF Core .Include() chain would produce.
/// </summary>
public interface IAdventureRepository
{
    Task<IReadOnlyList<Adventure>> GetAllAsync();
    Task<Adventure?> GetByIdAsync(int id);
}
