using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Entities.Locations;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the seven scenario locations with stable IDs and exact frontend image keys.
/// Levels, encounter families and safety are initial mock balance values.
/// Quest lists are populated by the subsequent scenario quest seed.
/// </summary>
internal static class LocationSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        store.Locations.AddRange(new[]
        {
            new Location
            {
                Id = 1,
                Slug = "ashtonia",
                Name = "Ashtonia",
                Description = "A fortified town where adventurers gather supplies, hear local news and begin their journey.",
                RecommendedMinimumLevel = 1,
                BackgroundImage = "Ashtonia",
                AvailableMainQuestIds = new List<int>(),
                AvailableSideQuestIds = new List<int>(),
                PossibleEnemyTypes = new List<EnemyFamily> {  },
                IsSafeLocation = true
            },
            new Location
            {
                Id = 2,
                Slug = "darkstorm-keep",
                Name = "Darkstorm Keep",
                Description = "A ruined fortress beneath gathering storm clouds, occupied by demons and restless undead.",
                RecommendedMinimumLevel = 9,
                BackgroundImage = "Darkstorm_Keep",
                AvailableMainQuestIds = new List<int>(),
                AvailableSideQuestIds = new List<int>(),
                PossibleEnemyTypes = new List<EnemyFamily> { EnemyFamily.Demon, EnemyFamily.Skeleton, EnemyFamily.Wraith },
                IsSafeLocation = false
            },
            new Location
            {
                Id = 3,
                Slug = "heros-overlook",
                Name = "Hero's Overlook",
                Description = "A sheltered lookout and memorial above the valley, offering a place to rest and plan the next journey.",
                RecommendedMinimumLevel = 5,
                BackgroundImage = "Heros_Overlook",
                AvailableMainQuestIds = new List<int>(),
                AvailableSideQuestIds = new List<int>(),
                PossibleEnemyTypes = new List<EnemyFamily> {  },
                IsSafeLocation = true
            },
            new Location
            {
                Id = 4,
                Slug = "misthaven-port",
                Name = "Misthaven Port",
                Description = "A guarded harbor wrapped in sea mist, where traders, sailors and travelers exchange supplies and rumors.",
                RecommendedMinimumLevel = 3,
                BackgroundImage = "Misthaven_Port",
                AvailableMainQuestIds = new List<int>(),
                AvailableSideQuestIds = new List<int>(),
                PossibleEnemyTypes = new List<EnemyFamily> {  },
                IsSafeLocation = true
            },
            new Location
            {
                Id = 5,
                Slug = "oakheaven",
                Name = "Oakheaven",
                Description = "A peaceful woodland settlement beneath ancient oaks, offering shelter and provisions to passing travelers.",
                RecommendedMinimumLevel = 2,
                BackgroundImage = "Oakheaven",
                AvailableMainQuestIds = new List<int>(),
                AvailableSideQuestIds = new List<int>(),
                PossibleEnemyTypes = new List<EnemyFamily> {  },
                IsSafeLocation = true
            },
            new Location
            {
                Id = 6,
                Slug = "the-bone-peaks",
                Name = "The Bone Peaks",
                Description = "Jagged mountains scattered with ancient remains, where trolls, chimeras and undead stalk the passes.",
                RecommendedMinimumLevel = 7,
                BackgroundImage = "The_Bone_Peaks",
                AvailableMainQuestIds = new List<int>(),
                AvailableSideQuestIds = new List<int>(),
                PossibleEnemyTypes = new List<EnemyFamily> { EnemyFamily.Skeleton, EnemyFamily.Troll, EnemyFamily.Chimera },
                IsSafeLocation = false
            },
            new Location
            {
                Id = 7,
                Slug = "whispering-woods",
                Name = "Whispering Woods",
                Description = "A tangled forest of murmuring trees, hidden trails and abandoned camps haunted by raiders and spirits.",
                RecommendedMinimumLevel = 2,
                BackgroundImage = "Wispering_Woods",
                AvailableMainQuestIds = new List<int>(),
                AvailableSideQuestIds = new List<int>(),
                PossibleEnemyTypes = new List<EnemyFamily> { EnemyFamily.Goblin, EnemyFamily.Slime, EnemyFamily.Wraith },
                IsSafeLocation = false
            }
        });
    }

    /// <summary>
    /// Seeds the location catalogue used by the dedicated location-progression
    /// model. This is intentionally run after quest seed data so every quest ID
    /// is copied from the scenario's authoritative location catalogue.
    /// Encounter IDs refer to EnemySeedData IDs until a separate encounter model
    /// is introduced.
    /// </summary>
    public static void SeedDefinitions(InMemoryGameDataStore store)
    {
        store.LocationDefinitions.AddRange(new[]
        {
            Definition(store, LocationId.HerosOverlook, "Hero's Overlook",
                "A sheltered memorial above the valley where travelers rest and plan the next journey.",
                "Heros_Overlook", 1, 10, true, Array.Empty<int>(),
                new[] { "PrologueUnlocked", "FinaleUnlocked" }),
            Definition(store, LocationId.WhisperingWoods, "Whispering Woods",
                "A tangled forest of murmuring trees, hidden trails, raiders and restless spirits.",
                "Wispering_Woods", 1, 8, false, new[] { 4, 7, 19 }),
            Definition(store, LocationId.Ashtonia, "Ashtonia",
                "A fortified town of guild business, old clan grudges and the first Crown fragment route.",
                "Ashtonia", 1, 8, true, Array.Empty<int>()),
            Definition(store, LocationId.MisthavenPort, "Misthaven Port",
                "A guarded harbor and safe Guild hub where sailors, merchants and adventurers trade news.",
                "Misthaven_Port", 2, 5, true, Array.Empty<int>()),
            Definition(store, LocationId.Oakheaven, "Oakheaven",
                "A woodland settlement beneath ancient oaks, threatened by a hidden possession network.",
                "Oakheaven", 3, 6, true, Array.Empty<int>()),
            Definition(store, LocationId.TheBonePeaks, "The Bone Peaks",
                "Jagged mountain passes, ancient remains and the sealed halls of Karag-Dur.",
                "The_Bone_Peaks", 1, 8, false, new[] { 1, 10, 13 },
                new[] { "ExteriorUnlocked", "KaragDurUnlocked" }),
            Definition(store, LocationId.DarkstormKeep, "Darkstorm Keep",
                "A storm-battered fortress of demons and undead, reached only after the Crown is restored.",
                "Darkstorm_Keep", 8, 10, false, new[] { 1, 16, 19 },
                unlockRequirement: new LocationUnlockRequirement
                {
                    RequiredFragmentCount = 3,
                    RequiredQuestIds = new List<int> { 10 }
                })
        });
    }

    private static LocationDefinition Definition(
        InMemoryGameDataStore store, LocationId id, string name, string description, string backgroundImage,
        int minimumLevel, int maximumLevel, bool isSafeLocation, int[] encounterIds,
        string[]? specialFlags = null, LocationUnlockRequirement? unlockRequirement = null)
    {
        var scenarioLocation = storeLocation(id);
        return new LocationDefinition
        {
            Id = id,
            Name = name,
            Description = description,
            BackgroundImage = backgroundImage,
            RecommendedMinimumLevel = minimumLevel,
            RecommendedMaximumLevel = maximumLevel,
            IsSafeLocation = isSafeLocation,
            MainQuestIds = scenarioLocation.AvailableMainQuestIds.ToList(),
            SideQuestIds = scenarioLocation.AvailableSideQuestIds.ToList(),
            EncounterIds = encounterIds.ToList(),
            SpecialFlags = specialFlags?.ToList() ?? new List<string>(),
            UnlockRequirement = unlockRequirement ?? new LocationUnlockRequirement()
        };

        Location storeLocation(LocationId locationId) =>
            store.Locations.Single(location => location.Id == (int)locationId);
    }
}
