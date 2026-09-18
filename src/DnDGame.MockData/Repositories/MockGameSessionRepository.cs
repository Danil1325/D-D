using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

public class MockGameSessionRepository : IGameSessionRepository
{
    private readonly InMemoryGameDataStore _store;

    public MockGameSessionRepository(InMemoryGameDataStore store)
    {
        _store = store;
    }

    public Task<GameSession?> GetByIdAsync(int id)
    {
        var session = _store.GameSessions.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(session is null ? null : Hydrate(session));
    }

    public Task<IReadOnlyList<GameSession>> GetByCharacterIdAsync(int characterId)
    {
        return Task.FromResult<IReadOnlyList<GameSession>>(
            _store.GameSessions
                .Where(session => session.CharacterId == characterId)
                .Select(Hydrate)
                .ToList());
    }

    public Task<GameSession> AddAsync(GameSession session)
    {
        session.Id = _store.GetNextGameSessionId();
        _store.GameSessions.Add(session);
        return Task.FromResult(session);
    }

    public Task UpdateAsync(GameSession session)
    {
        // Same reasoning as MockCharacterRepository.UpdateAsync — nothing to do here
        // since the object is already the one stored in memory.
        return Task.CompletedTask;
    }

    public Task AddLogEntryAsync(SessionLogEntry entry)
    {
        entry.Id = _store.GetNextLogEntryId();
        if (entry.Timestamp == default)
        {
            entry.Timestamp = DateTime.UtcNow;
        }

        _store.SessionLogEntries.Add(entry);
        return Task.CompletedTask;
    }

    private GameSession Hydrate(GameSession session)
    {
        var node = _store.StoryNodes.FirstOrDefault(n => n.Id == session.CurrentNodeId);
        session.CurrentNode = node is null ? null : GraphHydrator.HydrateNode(_store, node);

        session.LogEntries = _store.SessionLogEntries
            .Where(e => e.GameSessionId == session.Id)
            .OrderBy(e => e.Timestamp)
            .ToList();

        return session;
    }
}
