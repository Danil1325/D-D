using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Models;

/// <summary>
/// Input to ILocationEncounterService.GetAvailableEncountersAsync. Everything else
/// the filtering rules need (character level/race, active quests, ScenarioProgress
/// story flags/Ash Clock, battle history) is resolved server-side from the game
/// session — this only carries what the caller alone knows.
/// </summary>
/// <param name="LocationId">The location whose encounter pool to filter.</param>
/// <param name="GameSessionId">The player's game session, used to resolve character/quest/scenario state.</param>
/// <param name="SubLocation">
/// The specific area within the location the player is currently at (e.g. "arena",
/// "archives", "port", "wrecks" at Misthaven Port). Null means the location's
/// general/central area, which yields no encounters wherever a location restricts
/// encounters to named sub-locations.
/// </param>
public sealed record EncounterSelectionContext(
    LocationId LocationId,
    int GameSessionId,
    string? SubLocation = null);
