namespace DnDGame.Domain.Entities.Locations;

/// <summary>
/// Conditions that gate a single EncounterEnemyDefinition. All populated conditions
/// must hold; an empty/null/default field means no restriction of that kind. Mirrors
/// the pattern used by ChoiceRequirement/LocationUnlockRequirement (BACK-LOC-06).
/// </summary>
public class EncounterAvailabilityRequirement
{
    /// <summary>
    /// Sub-areas of the location this encounter is restricted to (e.g. "arena",
    /// "archives", "port", "wrecks" at Misthaven Port). Empty means available
    /// anywhere in the location.
    /// </summary>
    public ICollection<string> RequiredSubLocations { get; set; } = new List<string>();

    /// <summary>
    /// All entry flags must match ScenarioProgress.StoryFlags; an empty set imposes
    /// no restriction. Used both to require a flag (e.g. an alliance having failed)
    /// and to exclude an enemy once it has become an ally (require the ally/restore
    /// flag to be false).
    /// </summary>
    public Dictionary<string, bool> RequiredStoryFlags { get; set; } = new();

    /// <summary>Inclusive lower bound on ScenarioProgress.AshClock; null imposes no restriction.</summary>
    public int? MinimumAshClock { get; set; }

    /// <summary>The referenced quest must be Active for the player's session; null imposes no restriction.</summary>
    public int? RequiredActiveQuestId { get; set; }

    /// <summary>FK to Race.Id the player's character must have; null imposes no restriction. Matches LocationUnlockRequirement.RequiredRace.</summary>
    public int? RequiredRace { get; set; }
}
