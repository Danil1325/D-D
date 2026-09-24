namespace DnDGame.MockData.SeedData;

/// <summary>
/// Runs every seed step in dependency order. Order matters here: later steps
/// reference ids created by earlier ones (portraits reference races/classes, the
/// adventure references enemies), so this is not just organizational.
/// </summary>
internal static class GameDataSeeder
{
    public static void Seed(InMemoryGameDataStore store)
    {
        RaceSeedData.Seed(store);
        ClassSeedData.Seed(store);
        PortraitSeedData.Seed(store);
        TalentSeedData.Seed(store);
        CardSeedData.Seed(store);
        AchievementSeedData.Seed(store);
        SkillSeedData.Seed(store);
        EnemySeedData.Seed(store);
LocationSeedData.Seed(store);
        LocationEncounterSeedData.Seed(store);
        new ScenarioStorySceneSeedData().Seed(store);
        ScenarioMainQuestSeedData.Seed(store);
        ScenarioSideQuestSeedData.Seed(store);
        LocationSeedData.SeedDefinitions(store);
        AdventureSeedData.Seed(store);
    }
}
