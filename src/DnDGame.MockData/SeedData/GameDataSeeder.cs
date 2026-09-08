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
        EnemySeedData.Seed(store);
        AdventureSeedData.Seed(store);
    }
}
