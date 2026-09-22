using DnDGame.BusinessLayer.Models;
using DnDGame.BusinessLayer.Services;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using Xunit;

namespace DnDGame.Tests.BusinessLayer;

public class LocationEncounterServiceTests
{
    private const int CurrentPlayerId = 1;

    // --- Level gate (reuses LocationDefinition.RecommendedMinimumLevel) ---

    [Fact]
    public async Task BelowRecommendedMinimumLevel_ReturnsNoEncounters()
    {
        var (service, _, _) = CreateScenario(characterLevel: 1); // Darkstorm Keep requires level 8

        var result = await service.GetAvailableEncountersAsync(
            new EncounterSelectionContext(LocationId.DarkstormKeep, GameSessionId: 1));

        Assert.Empty(result.AvailableEncounters);
    }

    [Fact]
    public async Task AtOrAboveRecommendedMinimumLevel_ReturnsUnrestrictedEncounters()
    {
        var (service, _, _) = CreateScenario(characterLevel: 1); // Whispering Woods requires level 1, no restricted slots

        var result = await service.GetAvailableEncountersAsync(
            new EncounterSelectionContext(LocationId.WhisperingWoods, GameSessionId: 1));

        Assert.Equal(7, result.AvailableEncounters.Count);
        Assert.Contains(result.AvailableEncounters, e => e.EnemyId == 21 && e.Tier == EncounterTier.Boss); // Dread Wraith
    }

    // --- Sub-location gate (Misthaven Port never generates encounters at the center) ---

    [Fact]
    public async Task MisthavenPort_WithNoSubLocation_ReturnsNoEncounters()
    {
        var (service, _, _) = CreateScenario(characterLevel: 2);

        var result = await service.GetAvailableEncountersAsync(
            new EncounterSelectionContext(LocationId.MisthavenPort, GameSessionId: 1));

        Assert.Empty(result.AvailableEncounters);
    }

    [Fact]
    public async Task MisthavenPort_InANamedSubLocation_ReturnsItsEncounters_ExceptTheArenaHobgoblinWithoutItsQuestActive()
    {
        var (service, _, _) = CreateScenario(characterLevel: 2);

        var result = await service.GetAvailableEncountersAsync(
            new EncounterSelectionContext(LocationId.MisthavenPort, GameSessionId: 1, SubLocation: "arena"));

        // The arena's Hobgoblin challenger (Arena Master Kael's SQ-MP-02) only
        // appears while that quest is active; the other 5 slots don't require it.
        Assert.Equal(5, result.AvailableEncounters.Count);
        Assert.DoesNotContain(result.AvailableEncounters, e => e.EnemyId == 5);
        Assert.DoesNotContain(result.AvailableEncounters, e => e.Tier == EncounterTier.Boss);
    }

    [Fact]
    public async Task MisthavenPort_ArenaHobgoblin_OnlyAppearsWhileItsQuestIsActive()
    {
        var (service, store, _) = CreateScenario(characterLevel: 2);
        var context = new EncounterSelectionContext(LocationId.MisthavenPort, GameSessionId: 1, SubLocation: "arena");

        var before = await service.GetAvailableEncountersAsync(context);
        Assert.DoesNotContain(before.AvailableEncounters, e => e.EnemyId == 5);

        store.PlayerQuests.Add(new PlayerQuest { GameSessionId = 1, QuestId = 1005, Status = QuestStatus.Active });

        var after = await service.GetAvailableEncountersAsync(context);
        Assert.Contains(after.AvailableEncounters, e => e.EnemyId == 5);
    }

    // --- Ally exclusion via story flags (an NPC that became an ally is never an enemy) ---

    [Fact]
    public async Task Oakheaven_Goblins_AreEnemiesByDefault_AndDisappearOnceAllianceRouteOpens()
    {
        var (service, store, _) = CreateScenario(characterLevel: 3);
        var context = new EncounterSelectionContext(LocationId.Oakheaven, GameSessionId: 1);

        var before = await service.GetAvailableEncountersAsync(context);
        Assert.Contains(before.AvailableEncounters, e => e.EnemyId == 4); // Goblin
        Assert.Contains(before.AvailableEncounters, e => e.EnemyId == 5); // Hobgoblin

        store.ScenarioProgresses.Add(new ScenarioProgress
        {
            Id = 1,
            GameSessionId = 1,
            StoryFlags = new Dictionary<string, bool> { ["goblin_alliance_route"] = true }
        });

        var after = await service.GetAvailableEncountersAsync(context);
        Assert.DoesNotContain(after.AvailableEncounters, e => e.EnemyId == 4);
        Assert.DoesNotContain(after.AvailableEncounters, e => e.EnemyId == 5);
        // Unrelated normal enemies are unaffected.
        Assert.Contains(after.AvailableEncounters, e => e.EnemyId == 16); // Demon
    }

    [Fact]
    public async Task TheBonePeaks_GrommashVurr_DisappearsOnceTrollAllianceRouteOpens()
    {
        var (service, store, _) = CreateScenario(characterLevel: 1);
        var context = new EncounterSelectionContext(LocationId.TheBonePeaks, GameSessionId: 1);

        var before = await service.GetAvailableEncountersAsync(context);
        Assert.Contains(before.AvailableEncounters, e => e.EnemyId == 26); // Gromash-Vurr

        store.ScenarioProgresses.Add(new ScenarioProgress
        {
            Id = 1,
            GameSessionId = 1,
            StoryFlags = new Dictionary<string, bool> { ["troll_alliance_route"] = true }
        });

        var after = await service.GetAvailableEncountersAsync(context);
        Assert.DoesNotContain(after.AvailableEncounters, e => e.EnemyId == 26);
        Assert.Contains(after.AvailableEncounters, e => e.EnemyId == 25); // Nerath-Dur the Lich is unaffected
    }

    // --- Ash Clock gate ---

    [Fact]
    public async Task Oakheaven_KingSlimeBoss_OnlyAppearsAtAshClockFourOrHigher()
    {
        var (service, store, _) = CreateScenario(characterLevel: 3);
        var context = new EncounterSelectionContext(LocationId.Oakheaven, GameSessionId: 1);

        var before = await service.GetAvailableEncountersAsync(context);
        Assert.DoesNotContain(before.AvailableEncounters, e => e.EnemyId == 9); // King Slime

        store.ScenarioProgresses.Add(new ScenarioProgress { Id = 1, GameSessionId = 1, AshClock = 4 });

        var after = await service.GetAvailableEncountersAsync(context);
        Assert.Contains(after.AvailableEncounters, e => e.EnemyId == 9);
    }

    // --- Prologue gate (final bosses do not appear at Hero's Overlook in the prologue) ---

    [Fact]
    public async Task HerosOverlook_FinalBosses_AreHiddenUntilFinaleUnlocked()
    {
        var (service, store, _) = CreateScenario(characterLevel: 1);
        var context = new EncounterSelectionContext(LocationId.HerosOverlook, GameSessionId: 1);

        var before = await service.GetAvailableEncountersAsync(context);
        Assert.DoesNotContain(before.AvailableEncounters, e => e.EnemyId == 22); // The Herald
        Assert.DoesNotContain(before.AvailableEncounters, e => e.EnemyId == 23); // Vharruk
        Assert.Contains(before.AvailableEncounters, e => e.EnemyId == 19); // Phantom still available

        store.ScenarioProgresses.Add(new ScenarioProgress
        {
            Id = 1,
            GameSessionId = 1,
            StoryFlags = new Dictionary<string, bool> { ["FinaleUnlocked"] = true }
        });

        var after = await service.GetAvailableEncountersAsync(context);
        Assert.Contains(after.AvailableEncounters, e => e.EnemyId == 22);
        Assert.Contains(after.AvailableEncounters, e => e.EnemyId == 23);
    }

    // --- Already-finished (one-time) boss encounters ---

    [Fact]
    public async Task AlreadyDefeatedBoss_IsNeverReturnedAgain_ButNormalEncountersStayRepeatable()
    {
        var (service, store, _) = CreateScenario(characterLevel: 1);
        var context = new EncounterSelectionContext(LocationId.WhisperingWoods, GameSessionId: 1);

        store.Battles.Add(new Battle
        {
            Id = 1,
            GameSessionId = 1,
            EnemyId = 21, // Dread Wraith
            LocationId = LocationId.WhisperingWoods,
            Status = GameSessionStatus.Victory,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        });

        var result = await service.GetAvailableEncountersAsync(context);

        Assert.DoesNotContain(result.AvailableEncounters, e => e.EnemyId == 21);
        Assert.Contains(result.AvailableEncounters, e => e.EnemyId == 4); // Goblin (Normal) still repeatable
    }

    [Fact]
    public async Task DefeatingAnEnemyAsARandomEncounterAtOneLocation_DoesNotHideItAsABossElsewhere()
    {
        // Dread Wraith (21) is a repeatable Elite encounter at Darkstorm Keep but the
        // sole Boss at Whispering Woods — defeating it as the Darkstorm Keep Elite
        // fight must not remove the separate Whispering Woods boss fight.
        var (service, store, _) = CreateScenario(characterLevel: 8);

        store.Battles.Add(new Battle
        {
            Id = 1,
            GameSessionId = 1,
            EnemyId = 21, // Dread Wraith
            LocationId = LocationId.DarkstormKeep,
            Status = GameSessionStatus.Victory,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        });

        var whisperingWoods = await service.GetAvailableEncountersAsync(
            new EncounterSelectionContext(LocationId.WhisperingWoods, GameSessionId: 1));

        Assert.Contains(whisperingWoods.AvailableEncounters, e => e.EnemyId == 21 && e.Tier == EncounterTier.Boss);
    }

    [Fact]
    public async Task BattleWithNoLocationId_DoesNotSuppressAnyBossEncounter()
    {
        // Battles from the existing story-node pipeline (BattleService.StartBattleAsync)
        // don't set LocationId yet — they must never be mistaken for "defeated here".
        var (service, store, _) = CreateScenario(characterLevel: 1);

        store.Battles.Add(new Battle
        {
            Id = 1,
            GameSessionId = 1,
            EnemyId = 21, // Dread Wraith
            LocationId = null,
            Status = GameSessionStatus.Victory,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        });

        var result = await service.GetAvailableEncountersAsync(
            new EncounterSelectionContext(LocationId.WhisperingWoods, GameSessionId: 1));

        Assert.Contains(result.AvailableEncounters, e => e.EnemyId == 21);
    }

    // --- Generic race / active-quest hooks (no concrete seed data uses them yet) ---

    [Fact]
    public async Task RequiredRace_ExcludesCharactersOfADifferentRace()
    {
        var (service, store, _) = CreateScenario(characterLevel: 1, raceId: 2);
        var definition = store.LocationEncounterDefinitions.Single(d => d.LocationId == LocationId.WhisperingWoods);
        definition.Enemies.Add(new EncounterEnemyDefinition
        {
            EnemyId = 1, // Skeleton
            Tier = EncounterTier.Normal,
            Availability = new EncounterAvailabilityRequirement { RequiredRace = 1 }
        });

        var result = await service.GetAvailableEncountersAsync(
            new EncounterSelectionContext(LocationId.WhisperingWoods, GameSessionId: 1));

        Assert.DoesNotContain(result.AvailableEncounters, e => e.EnemyId == 1);
    }

    [Fact]
    public async Task RequiredActiveQuestId_OnlyAvailableWhileThatQuestIsActive()
    {
        var (service, store, _) = CreateScenario(characterLevel: 1);
        var definition = store.LocationEncounterDefinitions.Single(d => d.LocationId == LocationId.WhisperingWoods);
        definition.Enemies.Add(new EncounterEnemyDefinition
        {
            EnemyId = 1, // Skeleton
            Tier = EncounterTier.Normal,
            Availability = new EncounterAvailabilityRequirement { RequiredActiveQuestId = 999 }
        });
        var context = new EncounterSelectionContext(LocationId.WhisperingWoods, GameSessionId: 1);

        var beforeQuestStarted = await service.GetAvailableEncountersAsync(context);
        Assert.DoesNotContain(beforeQuestStarted.AvailableEncounters, e => e.EnemyId == 1);

        store.PlayerQuests.Add(new PlayerQuest { GameSessionId = 1, QuestId = 999, Status = QuestStatus.Active });

        var afterQuestStarted = await service.GetAvailableEncountersAsync(context);
        Assert.Contains(afterQuestStarted.AvailableEncounters, e => e.EnemyId == 1);
    }

    // --- Test scaffolding ---

    private static (ILocationEncounterService Service, InMemoryGameDataStore Store, PlayerCharacter Player) CreateScenario(
        int characterLevel,
        int raceId = 1)
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var player = new PlayerCharacter
        {
            Id = CurrentPlayerId,
            OwnerId = CurrentPlayerId.ToString(),
            Name = "Hero",
            Level = characterLevel,
            MaxHealth = 20,
            CurrentHealth = 20,
            RaceId = raceId,
            ClassId = 1
        };
        store.Characters.Add(player);
        store.GameSessions.Add(new GameSession { Id = 1, CharacterId = CurrentPlayerId, AdventureId = 1 });

        var service = new LocationEncounterService(
            new MockLocationEncounterRepository(store),
            new MockLocationDefinitionRepository(store),
            new MockEnemyRepository(store),
            new MockGameSessionRepository(store),
            new MockCharacterRepository(store),
            new MockScenarioProgressRepository(store),
            new MockPlayerQuestRepository(store),
            new MockBattleRepository(store),
            new FixedCurrentPlayerService(CurrentPlayerId));

        return (service, store, player);
    }

    private sealed class FixedCurrentPlayerService : DnDGame.BusinessLayer.Services.Interfaces.ICurrentPlayerService
    {
        private readonly int _playerId;

        public FixedCurrentPlayerService(int playerId)
        {
            _playerId = playerId;
        }

        public int GetCurrentPlayerId() => _playerId;
    }
}
