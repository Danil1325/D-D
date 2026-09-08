using DnDGame.Domain.Entities.Portraits;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds all 16 Race x Class portrait combinations. File-name fragments below match
/// the actual project image assets exactly (e.g. "orc_healer.png").
/// </summary>
internal static class PortraitSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        var races = new (int Id, string FileName)[]
        {
            (1, "human"),
            (2, "elf"),
            (3, "orc"),
            (4, "dwarf")
        };

        var classes = new (int Id, string FileName)[]
        {
            (1, "healer"),
            (2, "warrior"),
            (3, "magician"),
            (4, "bard")
        };

        var id = 1;
        foreach (var race in races)
        {
            foreach (var characterClass in classes)
            {
                store.Portraits.Add(new CharacterPortrait
                {
                    Id = id++,
                    RaceId = race.Id,
                    ClassId = characterClass.Id,
                    ImagePath = $"portraits/{race.FileName}_{characterClass.FileName}.png"
                });
            }
        }
    }
}
