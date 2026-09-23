using DnDGame.Domain.Entities.Achievements;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the achievements catalog. Reference data, so every row gets an explicit
/// hardcoded Id (see InMemoryGameDataStore's seed-id convention) and a stable
/// Code that clients/tests can rely on. Id ranges are namespaced by event type
/// (1xx creation, 2xx quests, 3xx battles, 4xx locations) to keep them readable;
/// the codes are the canonical identifiers.
///
/// Only the four events available today are represented — the catalog is meant to
/// grow, not to be exhaustive.
/// </summary>
internal static class AchievementSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        var achievements = new[]
        {
            new Achievement
            {
                Id = 101,
                Code = "A_HERO_IS_BORN",
                Title = "A Hero Is Born",
                Description = "Head out on your first journey by creating a character.",
                Type = AchievementType.CharacterCreated,
                TargetAmount = 1
            },
            new Achievement
            {
                Id = 201,
                Code = "FIRST_STEPS",
                Title = "First Steps",
                Description = "Complete your very first quest.",
                Type = AchievementType.QuestsCompleted,
                TargetAmount = 1
            },
            new Achievement
            {
                Id = 202,
                Code = "QUEST_CONQUEROR",
                Title = "Quest Conqueror",
                Description = "Complete 5 quests.",
                Type = AchievementType.QuestsCompleted,
                TargetAmount = 5
            },
            new Achievement
            {
                Id = 301,
                Code = "FIRST_BLOOD",
                Title = "First Blood",
                Description = "Win your first battle.",
                Type = AchievementType.BattlesWon,
                TargetAmount = 1
            },
            new Achievement
            {
                Id = 302,
                Code = "VICTORIOUS_WARRIOR",
                Title = "Victorious Warrior",
                Description = "Win 10 battles.",
                Type = AchievementType.BattlesWon,
                TargetAmount = 10
            },
            new Achievement
            {
                Id = 401,
                Code = "WANDERER",
                Title = "Wanderer",
                Description = "Unlock your first location.",
                Type = AchievementType.LocationsUnlocked,
                TargetAmount = 1
            },
            new Achievement
            {
                Id = 402,
                Code = "EXPLORER",
                Title = "Explorer",
                Description = "Unlock 5 locations.",
                Type = AchievementType.LocationsUnlocked,
                TargetAmount = 5
            }
        };

        store.Achievements.AddRange(achievements);
    }
}