using DnDGame.Domain.Entities.Locations;

namespace DnDGame.Tests.Domain.Locations;

public class LocationModelsTests
{
    [Fact]
    public void LocationDefinition_InitializesMutableCollections()
    {
        var definition = new LocationDefinition();

        definition.MainQuestIds.Add(101);
        definition.SideQuestIds.Add(201);
        definition.EncounterIds.Add(301);

        Assert.Equal(new[] { 101 }, definition.MainQuestIds);
        Assert.Equal(new[] { 201 }, definition.SideQuestIds);
        Assert.Equal(new[] { 301 }, definition.EncounterIds);
    }

    [Fact]
    public void LocationProgress_StartsLockedWithEmptyStoryFlags()
    {
        var progress = new LocationProgress();

        Assert.Equal(LocationStatus.Locked, progress.Status);
        Assert.Empty(progress.StoryFlags);
        Assert.False(progress.Visited);
        Assert.False(progress.Completed);
        Assert.Null(progress.UnlockedAtLevel);
    }

    [Fact]
    public void RouteStep_CanExpressTypedLocationRequirements()
    {
        var step = new LocationRouteStep
        {
            Order = 2,
            LocationId = LocationId.WhisperingWoods,
            UnlockRequirement = new LocationUnlockRequirement
            {
                MinimumLevel = 3,
                RequiredRace = 2,
                RequiredFragmentCount = 1,
                RequiredPreviousLocationIds = new List<LocationId> { LocationId.Oakheaven }
            }
        };

        Assert.Equal(LocationId.WhisperingWoods, step.LocationId);
        Assert.Equal(3, step.UnlockRequirement.MinimumLevel);
        Assert.Contains(LocationId.Oakheaven, step.UnlockRequirement.RequiredPreviousLocationIds);
    }
}
