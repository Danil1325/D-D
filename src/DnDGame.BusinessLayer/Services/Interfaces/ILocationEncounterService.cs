using DnDGame.BusinessLayer.Models;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Resolves which enemy encounters are currently selectable at a location for one
/// player's game session. This only computes and returns configuration (which
/// EnemyId/Tier are on offer) — it never creates or advances a battle itself; that
/// remains the BattleEngine/BattleService's job (see §7 of the Person 3 handoff).
///
/// An EncounterEnemyDefinition slot is included only when ALL of the following hold:
///   - The location's LocationDefinition.RecommendedMinimumLevel is met by the character.
///   - The slot is not a Boss the player has already defeated at this location in
///     this session (a Battle row for that EnemyId and LocationId with
///     Status == Victory) — Boss encounters are one-time per location; Normal/Elite
///     encounters remain repeatable regardless of history. Scoped by LocationId
///     because the same EnemyId can be a Boss at one location and a repeatable
///     Normal/Elite encounter at another (e.g. Dread Wraith).
///   - The slot's EncounterAvailabilityRequirement (if any) is satisfied:
///     RequiredSubLocations (the caller's EncounterSelectionContext.SubLocation must
///     be one of them), RequiredStoryFlags (all must match ScenarioProgress.StoryFlags,
///     a missing flag counting as false), MinimumAshClock, RequiredActiveQuestId
///     (must be Active for this session), RequiredRace (must match the character's RaceId).
/// </summary>
public interface ILocationEncounterService
{
    Task<EncounterSelectionResult> GetAvailableEncountersAsync(EncounterSelectionContext context);
}
