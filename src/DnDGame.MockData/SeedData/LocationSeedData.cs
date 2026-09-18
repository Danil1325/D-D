using DnDGame.Domain.Entities.Game;
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
                BackgroundImage = "Whispering_Woods",
                AvailableMainQuestIds = new List<int>(),
                AvailableSideQuestIds = new List<int>(),
                PossibleEnemyTypes = new List<EnemyFamily> { EnemyFamily.Goblin, EnemyFamily.Slime, EnemyFamily.Wraith },
                IsSafeLocation = false
            }
        });
    }
}
