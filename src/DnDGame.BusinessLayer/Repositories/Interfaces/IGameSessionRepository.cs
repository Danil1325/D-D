using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

public interface IGameSessionRepository
{
    Task<GameSession?> GetByIdAsync(int id);
    Task<IReadOnlyList<GameSession>> GetByCharacterIdAsync(int characterId);
    Task<GameSession> AddAsync(GameSession session);
    Task UpdateAsync(GameSession session);
    Task AddLogEntryAsync(SessionLogEntry entry);
}