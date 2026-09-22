namespace DnDGame.Domain.Entities.Locations;

/// <summary>
/// Requirements that must all be met before a location can be unlocked.
/// Null scalar values and empty collections impose no requirement.
/// </summary>
public class LocationUnlockRequirement
{
    public int? MinimumLevel { get; set; }
    public int? RequiredRace { get; set; }
    public ICollection<int> RequiredQuestIds { get; set; } = new List<int>();
    public Dictionary<string, bool> RequiredStoryFlags { get; set; } = new();
    public int? RequiredFragmentCount { get; set; }
    public ICollection<LocationId> RequiredPreviousLocationIds { get; set; } = new List<LocationId>();
}
