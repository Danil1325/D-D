using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockScenarioProgressRepository : IScenarioProgressRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockScenarioProgressRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<ScenarioProgress?> GetByGameSessionAsync(int gameSessionId)
    {
        return Task.FromResult(_store.ScenarioProgresses.FirstOrDefault(
            sp => sp.GameSessionId == gameSessionId));
    }

    public Task<ScenarioProgress> AddAsync(ScenarioProgress progress)
    {
        progress.Id = _store.GetNextScenarioProgressId();
        _store.ScenarioProgresses.Add(progress);
        return Task.FromResult(progress);
    }

    public Task UpdateAsync(ScenarioProgress progress)
    {
        // Same reasoning as MockCharacterRepository.UpdateAsync — the object is
        // already the one held in memory.
        return Task.CompletedTask;
    }
}