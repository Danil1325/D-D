using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockStorySceneRepository : IStorySceneRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockStorySceneRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<StoryScene>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<StoryScene>>(_store.StoryScenes.ToList());
    }

    public Task<StoryScene?> GetByIdAsync(int id)
    {
        return Task.FromResult(_store.StoryScenes.FirstOrDefault(scene => scene.Id == id));
    }
}