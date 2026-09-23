using DnDGame.BusinessLayer.Dtos.Achievements;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Read + event-side of the achievements feature.
///
/// The Register* methods are the ONLY writers: they are invoked from the
/// application services where the corresponding real game events are persisted
/// (character creation, quest completion, battle victory, location unlock). They
/// are intentionally not part of any HTTP surface — no GET endpoint can advance
/// an achievement, so loading a page grants nothing.
/// </summary>
public interface IAchievementService
{
    /// <summary>The whole catalog, independent of any player.</summary>
    Task<IReadOnlyList<AchievementDto>> GetCatalogAsync();

    /// <summary>
    /// The current player's progress. Resolves the player server-side via
    /// ICurrentPlayerService and keys the result on their character id. Throws
    /// DomainException(NOT_FOUND) if the current player has no character yet.
    /// </summary>
    Task<PlayerAchievementsDto> GetProgressForCurrentPlayerAsync();

    /// <summary>Progress keyed directly by a character id (used by tests and player-keyed endpoints).</summary>
    Task<PlayerAchievementsDto> GetProgressForPlayerAsync(int characterId);

    Task RegisterCharacterCreatedAsync(int characterId);
    Task RegisterQuestCompletedAsync(int characterId);
    Task RegisterBattleVictoryAsync(int characterId);
    Task RegisterLocationUnlockedAsync(int characterId);
}