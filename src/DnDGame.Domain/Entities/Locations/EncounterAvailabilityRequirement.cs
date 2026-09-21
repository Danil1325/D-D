namespace DnDGame.Domain.Entities.Locations;

/// <summary>
/// Conditions that gate a single EncounterEnemyDefinition. All populated conditions
/// must hold; an empty/null field means no restriction of that kind. Mirrors the
/// pattern used by ChoiceRequirement/LocationUnlockRequirement.
/// </summary>
public class EncounterAvailabilityRequirement
{
    /// <summary>
    /// Sub-areas of the location this encounter is restricted to (e.g. "arena",
    /// "archives", "port", "wrecks" at Misthaven Port). Empty means available
    /// anywhere in the location.
    /// </summary>
    public ICollection<string> RequiredSubLocations { get; set; } = new List<string>();

    /// <summary>Story flag that must be set (e.g. an alliance having failed) for this encounter to be available.</summary>
    public string? RequiredFlag { get; set; }

    public bool RequiredFlagValue { get; set; } = true;
}
