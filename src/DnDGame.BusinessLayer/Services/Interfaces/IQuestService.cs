using DnDGame.BusinessLayer.Models;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Manages quest progress for one player's game session: offering, starting,
/// advancing objectives, completing and failing quests.
///
/// Rules enforced here:
/// - Main quests are offered and started strictly in chain order (a main quest
///   whose prerequisite quest is not yet completed stays unavailable).
/// - Side quests are optional and never block the main chain.
/// - Offerings are filtered by location, character level and prerequisites.
/// - Quest EXP is granted exactly once; a cleared quest cannot be started again.
/// - Level-ups gained through quest EXP are reported as SkillPointsGained.
///
/// Errors are reported by throwing DomainException with the generic ErrorCodes
/// (NotFound for missing resources, Conflict for state violations, ValidationError
/// for invalid amounts). Expect your caller to map them to HTTP statuses.
/// </summary>
public interface IQuestService
{
    /// <summary>
    /// Quests the character may currently start in the session: prerequisites met,
    /// level in the recommended range and (when a location is given) offered at that
    /// location. Excludes quests already active, completed or failed. Completed main
    /// quests unlock the next one in the chain.
    /// </summary>
    Task<IReadOnlyList<QuestView>> GetAvailableQuestsAsync(int gameSessionId, int? locationId = null);

    /// <summary>Quests currently in progress (Active) for the session.</summary>
    Task<IReadOnlyList<QuestProgressView>> GetActiveQuestsAsync(int gameSessionId);

    /// <summary>Quests already cleared (Completed) for the session.</summary>
    Task<IReadOnlyList<QuestProgressView>> GetCompletedQuestsAsync(int gameSessionId);

    /// <summary>One quest from the catalog, regardless of the session's progress.</summary>
    Task<QuestView> GetQuestByIdAsync(int questId);

    /// <summary>Activates an offered quest. Fails if prerequisites are unmet or the quest is already active/completed/failed.</summary>
    Task<QuestProgressView> StartQuestAsync(int gameSessionId, int questId);

    /// <summary>
    /// Advances the current objective of an active quest. Only the current objective
    /// can be updated. Completing the last objective completes the quest and grants
    /// Experience exactly once (also granting per-objective experience along the way).
    /// </summary>
    Task<UpdateObjectiveResult> UpdateObjectiveAsync(int gameSessionId, int questId, int objectiveId, int amount = 1);

    /// <summary>
    /// Completes an active quest: grants its experience once, applies its story
    /// flags and reports the level-up (SkillPointsGained) if any.
    /// </summary>
    Task<QuestCompletionResult> CompleteQuestAsync(int gameSessionId, int questId);

    /// <summary>Fails an active quest. No experience or success flags are granted.</summary>
    Task FailQuestAsync(int gameSessionId, int questId);

    // --- Player-based conveniences (resolve the player's own session) ---
    // These exist so the API can be keyed by player id, keeping the session-level
    // methods above as the shared implementation.

    /// <summary>The quests the player's character may currently start. See <see cref="GetAvailableQuestsAsync"/>.</summary>
    Task<IReadOnlyList<QuestView>> GetAvailableQuestsForPlayerAsync(int playerId, int? locationId = null);

    /// <summary>The player's quests currently in progress.</summary>
    Task<IReadOnlyList<QuestProgressView>> GetActiveQuestsForPlayerAsync(int playerId);

    /// <summary>The player's quests already cleared.</summary>
    Task<IReadOnlyList<QuestProgressView>> GetCompletedQuestsForPlayerAsync(int playerId);

    /// <summary>Starts an offered quest for the player. See <see cref="StartQuestAsync"/>.</summary>
    Task<QuestProgressView> StartQuestForPlayerAsync(int playerId, int questId);

    /// <summary>Completes an active quest for the player. See <see cref="CompleteQuestAsync"/>.</summary>
    Task<QuestCompletionResult> CompleteQuestForPlayerAsync(int playerId, int questId);
}