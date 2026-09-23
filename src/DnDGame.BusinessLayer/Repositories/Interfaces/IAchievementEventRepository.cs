using DnDGame.Domain.Entities.Achievements;
using DnDGame.Domain.Enums;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>
/// Exactly-once ledger of the real game events already counted for a player.
/// AddIfMissingAsync both queries and inserts atomically enough for the in-memory
/// phase, returning whether the event was new.
/// </summary>
public interface IAchievementEventRepository
{
    /// <summary>
    /// Records the event if it has not been recorded for this player before and
    /// returns true; returns false without writing when the same event was already
    /// counted. This is the idempotency backstop for every Register* call.
    /// </summary>
    Task<bool> AddIfMissingAsync(AchievementEvent recordedEvent);

    Task<IReadOnlyList<AchievementEvent>> GetByPlayerIdAsync(int playerId);
}