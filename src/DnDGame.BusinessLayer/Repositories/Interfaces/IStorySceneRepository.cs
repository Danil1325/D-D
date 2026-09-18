using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Read access to the scenario scene catalog (dialogue + choices).</summary>
public interface IStorySceneRepository
{
    Task<IReadOnlyList<StoryScene>> GetAllAsync();
    Task<StoryScene?> GetByIdAsync(int id);
}