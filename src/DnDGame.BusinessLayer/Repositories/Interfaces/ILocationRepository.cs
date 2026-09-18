using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to the world map of locations.</summary>
public interface ILocationRepository
{
    Task<IReadOnlyList<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
}