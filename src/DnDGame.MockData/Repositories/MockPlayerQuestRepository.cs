using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockPlayerQuestRepository : IPlayerQuestRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockPlayerQuestRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<PlayerQuest>> GetByGameSessionAsync(int gameSessionId)
    {
        return Task.FromResult<IReadOnlyList<PlayerQuest>>(
            _store.PlayerQuests.Where(pq => pq.GameSessionId == gameSessionId).ToList());
    }

    public Task<PlayerQuest?> GetByQuestAndSessionAsync(int questId, int gameSessionId)
    {
        return Task.FromResult(_store.PlayerQuests.FirstOrDefault(
            pq => pq.QuestId == questId && pq.GameSessionId == gameSessionId));
    }

    public Task<PlayerQuest> AddAsync(PlayerQuest playerQuest)
    {
        playerQuest.Id = _store.GetNextPlayerQuestId();
        _store.PlayerQuests.Add(playerQuest);
        return Task.FromResult(playerQuest);
    }

    public Task UpdateAsync(PlayerQuest playerQuest)
    {
        // Same reasoning as MockCharacterRepository.UpdateAsync — the object is
        // already the one held in memory.
        return Task.CompletedTask;
    }
}