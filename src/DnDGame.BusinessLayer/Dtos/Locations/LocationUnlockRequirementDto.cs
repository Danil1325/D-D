using DnDGame.Domain.Entities.Locations;

namespace DnDGame.BusinessLayer.Dtos.Locations;

/// <summary>Read-only projection of <see cref="LocationUnlockRequirement"/> for the API boundary.</summary>
public class LocationUnlockRequirementDto
{
    public int? MinimumLevel { get; init; }
    public int? RequiredRace { get; init; }
    public IReadOnlyCollection<int> RequiredQuestIds { get; init; } = Array.Empty<int>();
    public IReadOnlyDictionary<string, bool> RequiredStoryFlags { get; init; } = new Dictionary<string, bool>();
    public int? RequiredFragmentCount { get; init; }
    public IReadOnlyCollection<LocationId> RequiredPreviousLocationIds { get; init; } = Array.Empty<LocationId>();

    public static LocationUnlockRequirementDto FromDomain(LocationUnlockRequirement requirement) => new()
    {
        MinimumLevel = requirement.MinimumLevel,
        RequiredRace = requirement.RequiredRace,
        RequiredQuestIds = requirement.RequiredQuestIds.ToList(),
        RequiredStoryFlags = new Dictionary<string, bool>(requirement.RequiredStoryFlags),
        RequiredFragmentCount = requirement.RequiredFragmentCount,
        RequiredPreviousLocationIds = requirement.RequiredPreviousLocationIds.ToList()
    };
}
