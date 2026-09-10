using DnDGame.MockData.SeedData;

namespace DnDGame.MockData;

/// <summary>
/// The one public entry point for wiring up mock data from outside this assembly.
/// GameDataSeeder and the individual *SeedData classes are internal on purpose
/// (they're seeding implementation detail, not something API/BusinessLayer should
/// call step-by-step) — this class is the deliberate, narrow seam the API project's
/// composition root (Program.cs) uses instead.
///
/// Note: this only builds and seeds the store. Registering it (and the Mock*
/// repositories) into a DI container is Program.cs's job, not this class's — see
/// Program.cs for why DI composition lives there rather than in an AddMockData()
/// extension method on IServiceCollection.
/// </summary>
public static class MockDataBootstrapper
{
    public static InMemoryGameDataStore CreateSeededStore()
    {
        var store = new InMemoryGameDataStore();
        GameDataSeeder.Seed(store);
        return store;
    }
}
