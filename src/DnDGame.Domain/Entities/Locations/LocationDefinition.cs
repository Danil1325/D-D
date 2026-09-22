namespace DnDGame.Domain.Entities.Locations;

/// <summary>
/// Static location catalogue data. It is shared by all players and is suitable
/// for in-memory seed data while the application has no database.
/// </summary>
public class LocationDefinition
{
    public LocationId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundImage { get; set; } = string.Empty;
    public int RecommendedMinimumLevel { get; set; } = 1;
    public int RecommendedMaximumLevel { get; set; } = 1;
    public bool IsSafeLocation { get; set; }

    public ICollection<int> MainQuestIds { get; set; } = new List<int>();
    public ICollection<int> SideQuestIds { get; set; } = new List<int>();
    public ICollection<int> EncounterIds { get; set; } = new List<int>();

    /// <summary>Named story states exposed by this location's scenario content.</summary>
    public ICollection<string> SpecialFlags { get; set; } = new List<string>();

    /// <summary>Conditions required before the location may be made available.</summary>
    public LocationUnlockRequirement UnlockRequirement { get; set; } = new();
}
