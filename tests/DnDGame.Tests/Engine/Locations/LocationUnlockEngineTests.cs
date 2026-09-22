using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Locations;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.Tests.Engine.Locations;

public class LocationUnlockEngineTests
{
    private readonly ILocationUnlockEngine _engine = new LocationUnlockEngine();

    public static IEnumerable<object[]> RaceRoutes()
    {
        yield return new object[] { RaceType.Human, LocationId.MisthavenPort };
        yield return new object[] { RaceType.Elf, LocationId.WhisperingWoods };
        yield return new object[] { RaceType.Orc, LocationId.Ashtonia };
        yield return new object[] { RaceType.Dwarf, LocationId.TheBonePeaks };
    }

    [Theory]
    [MemberData(nameof(RaceRoutes))]
    public void GetAvailableLocations_UnlocksRaceSpecificFirstLocationAfterPrologue(
        RaceType race, LocationId expectedFirstLocation)
    {
        var context = CreateContext(race);

        var start = _engine.GetAvailableLocations(context);
        Assert.True(start.Success);
        Assert.Equal(new[] { LocationId.HerosOverlook }, start.Data);

        context.UnlockedLocationIds.Add(LocationId.HerosOverlook);
        context.CompletedLocationIds.Add(LocationId.HerosOverlook);
        var afterPrologue = _engine.GetAvailableLocations(context);

        Assert.True(afterPrologue.Success);
        Assert.Contains(expectedFirstLocation, afterPrologue.Data!);
    }

    [Theory]
    [MemberData(nameof(RaceRoutes))]
    public void MisthavenPort_IsAvailableAfterEachRaceSpecificOpening(
        RaceType race, LocationId firstRaceLocation)
    {
        var context = CreateContext(race);
        context.UnlockedLocationIds.Add(LocationId.HerosOverlook);
        context.CompletedLocationIds.Add(LocationId.HerosOverlook);
        context.UnlockedLocationIds.Add(firstRaceLocation);
        context.CompletedLocationIds.Add(firstRaceLocation);

        var available = _engine.GetAvailableLocations(context);

        Assert.True(available.Success);
        if (race == RaceType.Human)
            Assert.Contains(LocationId.MisthavenPort, context.UnlockedLocationIds);
        else
            Assert.Contains(LocationId.MisthavenPort, available.Data!);
    }

    [Fact]
    public void UnlockLocation_DoesNotUnlockTheSameLocationTwice()
    {
        var context = CreateContext(RaceType.Human);

        Assert.True(_engine.UnlockLocation(context, LocationId.HerosOverlook).Success);
        var secondUnlock = _engine.UnlockLocation(context, LocationId.HerosOverlook);

        Assert.False(secondUnlock.Success);
        Assert.Equal(EngineErrorCodes.LocationAlreadyUnlocked, secondUnlock.ErrorCode);
    }

    [Fact]
    public void Oakheaven_RequiresGuildRegistration()
    {
        var context = CreateContext(RaceType.Human);

        var locked = _engine.CanUnlockLocation(context, LocationId.Oakheaven);
        context.CompletedQuestIds.Add(3);
        var unlocked = _engine.CanUnlockLocation(context, LocationId.Oakheaven);

        Assert.False(locked.Success);
        Assert.True(unlocked.Success);
    }

    [Fact]
    public void DarkstormKeep_RequiresThreeFragmentsAndMq10()
    {
        var context = CreateContext(RaceType.Elf);

        Assert.False(_engine.CanUnlockLocation(context, LocationId.DarkstormKeep).Success);
        context.CrownFragmentCount = 3;
        context.CompletedQuestIds.Add(10);

        Assert.True(_engine.CanUnlockLocation(context, LocationId.DarkstormKeep).Success);
    }

    [Fact]
    public void HeroOverlookFinale_BecomesRecommendedAfterMq13()
    {
        var context = CreateContext(RaceType.Orc);
        context.UnlockedLocationIds.Add(LocationId.HerosOverlook);
        context.CompletedLocationIds.Add(LocationId.HerosOverlook);
        context.CompletedQuestIds.Add(13);

        var next = _engine.GetRecommendedNextLocation(context);
        var available = _engine.GetAvailableLocations(context);

        Assert.True(next.Success);
        Assert.Equal(LocationId.HerosOverlook, next.Data);
        Assert.Contains(LocationId.HerosOverlook, available.Data!);
    }

    private static LocationUnlockContext CreateContext(RaceType race) => new()
    {
        PlayerId = 1,
        Race = race,
        Level = 1
    };
}
