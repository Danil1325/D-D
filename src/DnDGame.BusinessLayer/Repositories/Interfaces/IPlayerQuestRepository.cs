using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>Per-session quest progress.</summary>
public interface IPlayerQuestRepository
{
    Task<IReadOnlyList<PlayerQuest>> GetByGameSessionAsync(int gameSessionId);
    Task<PlayerQuest?> GetByQuestAndSessionAsync(int questId, int gameSessionId);
    Task<PlayerQuest> AddAsync(PlayerQuest playerQuest);
    Task UpdateAsync(PlayerQuest playerQuest);
}