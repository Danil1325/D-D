using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Engine.Locations;

/// <summary>All player state needed to evaluate location progression.</summary>
public class LocationUnlockContext
{
    public int PlayerId { get; set; }
    public RaceType Race { get; set; }
    public int Level { get; set; } = 1;
    public ICollection<int> CompletedQuestIds { get; set; } = new List<int>();
    public Dictionary<string, bool> StoryFlags { get; set; } = new();
    public int CrownFragmentCount { get; set; }
    public LocationId? CurrentLocationId { get; set; }
    public ICollection<LocationId> CompletedLocationIds { get; set; } = new List<LocationId>();

    /// <summary>Persistent location availability for this player.</summary>
    public ICollection<LocationId> UnlockedLocationIds { get; set; } = new List<LocationId>();
}
