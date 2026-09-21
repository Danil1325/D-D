using DnDGame.Domain.Entities.Locations;
using DnDGame.MockData;

namespace DnDGame.Tests.MockData;

public class LocationSeedDataTests
{
    [Fact]
    public void LocationDefinitions_ContainEachScenarioLocationWithUniqueIds()
    {
        var definitions = MockDataBootstrapper.CreateSeededStore().LocationDefinitions;

        Assert.Equal(7, definitions.Count);
        Assert.Equal(7, definitions.Select(location => location.Id).Distinct().Count());
        Assert.Equal(new[]
        {
            LocationId.HerosOverlook,
            LocationId.WhisperingWoods,
            LocationId.Ashtonia,
            LocationId.MisthavenPort,
            LocationId.Oakheaven,
            LocationId.TheBonePeaks,
            LocationId.DarkstormKeep
        }, definitions.Select(location => location.Id));
    }

    [Fact]
    public void LocationDefinitions_UseSpecifiedBackgroundImagesAndSpecialRules()
    {
        var definitions = MockDataBootstrapper.CreateSeededStore().LocationDefinitions;
        var overlook = definitions.Single(location => location.Id == LocationId.HerosOverlook);
        var bonePeaks = definitions.Single(location => location.Id == LocationId.TheBonePeaks);
        var port = definitions.Single(location => location.Id == LocationId.MisthavenPort);
        var darkstorm = definitions.Single(location => location.Id == LocationId.DarkstormKeep);

        Assert.Equal("Heros_Overlook", overlook.BackgroundImage);
        Assert.Contains("PrologueUnlocked", overlook.SpecialFlags);
        Assert.Contains("FinaleUnlocked", overlook.SpecialFlags);
        Assert.Equal("The_Bone_Peaks", bonePeaks.BackgroundImage);
        Assert.Contains("ExteriorUnlocked", bonePeaks.SpecialFlags);
        Assert.Contains("KaragDurUnlocked", bonePeaks.SpecialFlags);
        Assert.True(port.IsSafeLocation);
        Assert.Equal("Misthaven_Port", port.BackgroundImage);
        Assert.Equal(3, darkstorm.UnlockRequirement.RequiredFragmentCount);
        Assert.Contains(10, darkstorm.UnlockRequirement.RequiredQuestIds);
    }
}
