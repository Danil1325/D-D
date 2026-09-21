using DnDGame.BusinessLayer.Common.Errors;
using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Scenarios;
using DnDGame.BusinessLayer.Repositories.Interfaces;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Engine.Scenario;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using Xunit;

namespace DnDGame.Tests.BusinessLayer;

public class ScenarioServiceTests
{
    private const int PlayerId = 1;
    private const string OwnerId = "1";
    private const int EntrySceneId = 900;

    // --- Locations ---

    [Fact]
    public async Task GetLocations_ReturnsAllSeededLocations()
    {
        var scenario = CreateScenario();

        var locations = await scenario.Service.GetLocationsAsync();

        Assert.Equal(7, locations.Count);
        Assert.All(locations, location => Assert.False(string.IsNullOrEmpty(location.Name)));
    }

    [Fact]
    public async Task GetLocation_ReturnsTheRequestedLocation()
    {
        var scenario = CreateScenario();

        var location = await scenario.Service.GetLocationAsync(3);

        Assert.Equal(3, location.Id);
        Assert.False(string.IsNullOrEmpty(location.Slug));
    }

    [Fact]
    public async Task GetLocation_UnknownLocation_FailsWithNotFound()
    {
        var scenario = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.GetLocationAsync(999));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    // --- Starting a run ---

    [Fact]
    public async Task Start_CreatesARunAtTheEntryScene()
    {
        var scenario = CreateScenario();

        var result = await scenario.Service.StartAsync(PlayerId);

        Assert.Equal(EntrySceneId, result.CurrentSceneId);
        Assert.False(result.IsCompleted);
        Assert.Single(scenario.Store.ScenarioProgresses);
        Assert.Single(scenario.Store.GameSessions);
    }

    [Fact]
    public async Task Start_WhileARunIsInProgress_Conflicts()
    {
        var scenario = CreateScenario();
        await scenario.Service.StartAsync(PlayerId);

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.StartAsync(PlayerId));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task Start_AfterTheStoryEnds_RestartsWithACleanState()
    {
        var scenario = CreateScenario();
        await scenario.Service.StartAsync(PlayerId);

        var progress = scenario.Store.ScenarioProgresses.Single();
        progress.CurrentSceneId = 999;
        progress.IsCompleted = true;
        progress.AshClock = 5;
        progress.Corruption = 7;
        progress.WarScore = 3;
        progress.StoryFlags["some_flag"] = true;

        var result = await scenario.Service.StartAsync(PlayerId);

        Assert.Equal(EntrySceneId, result.CurrentSceneId);
        Assert.False(result.IsCompleted);
        Assert.Equal(0, result.AshClock);
        Assert.Equal(0, result.Corruption);
        Assert.Equal(0, result.WarScore);
        Assert.Empty(result.StoryFlags);
    }

    [Fact]
    public async Task Start_UnknownPlayer_FailsWithNotFound()
    {
        var scenario = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.StartAsync(99));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    // --- Reading the current scene ---

    [Fact]
    public async Task GetCurrent_ReturnsTheSceneWithOnlyAvailableChoices()
    {
        var scenario = CreateScenario();
        await scenario.Service.StartAsync(PlayerId);

        var scene = await scenario.Service.GetCurrentAsync(PlayerId);

        Assert.Equal(EntrySceneId, scene.Id);
        Assert.Equal("How to Use This Script", scene.Title);
        Assert.NotEmpty(scene.Dialogues);
        Assert.Single(scene.Choices);
    }

    [Fact]
    public async Task GetCurrent_BeforeAnyRun_FailsWithNotFound()
    {
        var scenario = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.GetCurrentAsync(PlayerId));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    [Fact]
    public async Task GetCurrent_UnknownPlayer_FailsWithNotFound()
    {
        var scenario = CreateScenario();

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.GetCurrentAsync(99));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    // --- Selecting a choice ---

    [Fact]
    public async Task SelectChoice_AdvancesTheRunAlongTheChoice()
    {
        var scenario = CreateScenario();
        await scenario.Service.StartAsync(PlayerId);
        var scene = await scenario.Service.GetCurrentAsync(PlayerId);
        var choice = Assert.Single(scene.Choices);

        var result = await scenario.Service.SelectChoiceAsync(new SelectChoiceRequest
        {
            PlayerId = PlayerId,
            SceneId = scene.Id,
            ChoiceId = choice.Id
        });

        Assert.Equal(choice.NextSceneId, result.CurrentSceneId);
    }

    [Fact]
    public async Task SelectChoice_WithExplicitLocationUnlock_ReturnsNewLocationIdAndPersistsProgress()
    {
        var scenario = CreateScenario();
        var (scene, choice) = await StartAndGetOnlyChoiceAsync(scenario);
        choice.Consequences.Add(new ChoiceConsequence
        {
            NewLocationIds = new List<int> { (int)LocationId.MisthavenPort }
        });

        var result = await scenario.Service.SelectChoiceAsync(new SelectChoiceRequest
        {
            PlayerId = PlayerId,
            SceneId = scene.Id,
            ChoiceId = choice.Id
        });

        Assert.Equal(new[] { (int)LocationId.MisthavenPort }, result.NewLocationIds);
        var progress = Assert.Single(scenario.Store.LocationProgresses, progress =>
            progress.PlayerId == PlayerId &&
            progress.LocationId == LocationId.MisthavenPort);
        Assert.Equal(LocationStatus.Available, progress.Status);
    }

    [Fact]
    public async Task SelectChoice_WithAlreadyUnlockedLocation_ReturnsNoNewLocationIds()
    {
        var scenario = CreateScenario();
        scenario.Store.LocationProgresses.Add(new LocationProgress
        {
            PlayerId = PlayerId,
            LocationId = LocationId.MisthavenPort,
            Status = LocationStatus.Available,
            UnlockedAtLevel = 1
        });
        var (scene, choice) = await StartAndGetOnlyChoiceAsync(scenario);
        choice.Consequences.Add(new ChoiceConsequence
        {
            NewLocationIds = new List<int> { (int)LocationId.MisthavenPort }
        });

        var result = await scenario.Service.SelectChoiceAsync(new SelectChoiceRequest
        {
            PlayerId = PlayerId,
            SceneId = scene.Id,
            ChoiceId = choice.Id
        });

        Assert.Empty(result.NewLocationIds);
        Assert.Single(scenario.Store.LocationProgresses, progress =>
            progress.PlayerId == PlayerId &&
            progress.LocationId == LocationId.MisthavenPort);
    }

    [Fact]
    public async Task SelectChoice_WithoutExplicitLocationUnlock_ReturnsEmptyNewLocationIds()
    {
        var scenario = CreateScenario();
        var (scene, choice) = await StartAndGetOnlyChoiceAsync(scenario);

        var result = await scenario.Service.SelectChoiceAsync(new SelectChoiceRequest
        {
            PlayerId = PlayerId,
            SceneId = scene.Id,
            ChoiceId = choice.Id
        });

        Assert.Empty(result.NewLocationIds);
        Assert.Empty(scenario.Store.LocationProgresses);
    }

    [Fact]
    public async Task SelectChoice_WhenChoiceIsRejected_DoesNotUnlockLocations()
    {
        var scenario = CreateScenario();
        var (scene, choice) = await StartAndGetOnlyChoiceAsync(scenario);
        choice.Consequences.Add(new ChoiceConsequence
        {
            NewLocationIds = new List<int> { (int)LocationId.MisthavenPort }
        });

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.SelectChoiceAsync(
            new SelectChoiceRequest
            {
                PlayerId = PlayerId,
                SceneId = scene.Id,
                ChoiceId = 999999
            }));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
        Assert.Empty(scenario.Store.LocationProgresses);
    }

    [Fact]
    public async Task SelectChoice_WithAStaleSceneId_Conflicts()
    {
        var scenario = CreateScenario();
        await scenario.Service.StartAsync(PlayerId);

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.SelectChoiceAsync(
            new SelectChoiceRequest { PlayerId = PlayerId, SceneId = 9999, ChoiceId = 1 }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task SelectChoice_OnACompletedStory_Conflicts()
    {
        var scenario = CreateScenario();
        await scenario.Service.StartAsync(PlayerId);
        scenario.Store.ScenarioProgresses.Single().IsCompleted = true;

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.SelectChoiceAsync(
            new SelectChoiceRequest { PlayerId = PlayerId, SceneId = EntrySceneId, ChoiceId = 1 }));

        Assert.Equal(ErrorCodes.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task SelectChoice_WithAnUnknownChoice_FailsWithNotFound()
    {
        var scenario = CreateScenario();
        await scenario.Service.StartAsync(PlayerId);

        var exception = await Assert.ThrowsAsync<DomainException>(() => scenario.Service.SelectChoiceAsync(
            new SelectChoiceRequest { PlayerId = PlayerId, SceneId = EntrySceneId, ChoiceId = 999999 }));

        Assert.Equal(ErrorCodes.NotFound, exception.ErrorCode);
    }

    // --- Scenario helpers ---

    private static (IScenarioService Service, InMemoryGameDataStore Store) CreateScenario()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        store.Characters.Add(new PlayerCharacter
        {
            Id = 1,
            OwnerId = OwnerId,
            Name = "Hero",
            Level = 1,
            CurrentXp = 0,
            SkillPoints = 0,
            MaxHealth = 20,
            CurrentHealth = 20,
            RaceId = 1,
            ClassId = 1
        });

        var service = CreateService(store);
        return (service, store);
    }

    private static async Task<(StorySceneDto Scene, StoryChoice Choice)> StartAndGetOnlyChoiceAsync(
        (IScenarioService Service, InMemoryGameDataStore Store) scenario)
    {
        await scenario.Service.StartAsync(PlayerId);
        var scene = await scenario.Service.GetCurrentAsync(PlayerId);
        var choiceDto = Assert.Single(scene.Choices);
        var seededScene = scenario.Store.StoryScenes.Single(candidate => candidate.Id == scene.Id);
        var choice = seededScene.Choices.Single(candidate => candidate.Id == choiceDto.Id);
        return (scene, choice);
    }

    private static IScenarioService CreateService(InMemoryGameDataStore store)
    {
        return new ScenarioService(
            new ScenarioEngine(),
            new MockCharacterRepository(store),
            new MockGameSessionRepository(store),
            new MockScenarioProgressRepository(store),
            new MockStorySceneRepository(store),
            new MockQuestRepository(store),
            new MockPlayerQuestRepository(store),
            new MockLocationRepository(store),
            new ExplicitLocationUnlockService(
                new MockLocationDefinitionRepository(store),
                new MockLocationProgressRepository(store)));
    }
}
