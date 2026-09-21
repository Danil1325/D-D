namespace DnDGame.Domain.Entities.Locations;

/// <summary>Location state that belongs to one player.</summary>
public class LocationProgress
{
    public int PlayerId { get; set; }
    public LocationId LocationId { get; set; }
    public LocationStatus Status { get; set; } = LocationStatus.Locked;

    /// <summary>Player level at which this location became available, if unlocked.</summary>
    public int? UnlockedAtLevel { get; set; }

    public bool Visited { get; set; }
    public bool Completed { get; set; }

    /// <summary>Story state relevant to this location. A missing flag is false.</summary>
    public Dictionary<string, bool> StoryFlags { get; set; } = new();
}
