using DnDGame.Domain.Entities.Races;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds the 4 races, their flavor traits (transcribed from the reference cards),
/// and one RaceAttributeRange row per attribute per race.
///
/// A few trait descriptions were reworded from the original cards where the text
/// referenced Constitution, Wisdom, advantage, or skill checks — none of which exist
/// in this system. The traits stay as pure flavor text (per the Phase 1 decision
/// that RaceTrait carries no mechanical effect); only the wording changed, not the
/// intent. See the Phase 2 summary for the specific reworded lines.
/// </summary>
internal static class RaceSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        var human = new Race
        {
            Id = 1,
            Name = "Human",
            Description = "Versatile and ambitious, people adapt and overcome any challenge.",
            Quote = "Courage, will and determination defined our destiny.",
            CardNumber = 1,
            ImageFrontPath = "races/human_front.png",
            ImageBackPath = "races/human_back.png"
        };

        var elf = new Race
        {
            Id = 2,
            Name = "Elf",
            Description = "Graceful and timeless beings, children of the forest and the stars, guardians of ancient magic.",
            Quote = "Time flows differently for those who live in harmony with nature.",
            CardNumber = 2,
            ImageFrontPath = "races/elf_front.png",
            ImageBackPath = "races/elf_back.png"
        };

        var orc = new Race
        {
            Id = 3,
            Name = "Orc",
            Description = "Strong and unyielding, orcs live for battle, honor, and the clan.",
            Quote = "Strength is the law. Clan is everything.",
            CardNumber = 3,
            ImageFrontPath = "races/orc_front.png",
            ImageBackPath = "races/orc_back.png"
        };

        var dwarf = new Race
        {
            Id = 4,
            Name = "Dwarf",
            Description = "Sturdy and stout, dwarves are masters of craftsmanship, stone, and tradition.",
            Quote = "Stone and steel. Honor and toil. That is how the world has stood since the beginning.",
            CardNumber = 4,
            ImageFrontPath = "races/dwarf_front.png",
            ImageBackPath = "races/dwarf_back.png"
        };

        store.Races.AddRange(new[] { human, elf, orc, dwarf });

        store.RaceTraits.AddRange(new[]
        {
            new RaceTrait { Id = 1, RaceId = human.Id, Name = "Adaptable", Description = "People excel in many roles, growing steadily no matter the path they choose.", IconKey = "star" },
            new RaceTrait { Id = 2, RaceId = human.Id, Name = "Additional Talent", Description = "You get an additional talent at level 1.", IconKey = "badge" },
            new RaceTrait { Id = 3, RaceId = human.Id, Name = "Additional Languages", Description = "You know an additional language of your choice.", IconKey = "book" },
            new RaceTrait { Id = 4, RaceId = human.Id, Name = "Resilient", Description = "Additional hit points: +1 per level.", IconKey = "heart" },
            new RaceTrait { Id = 5, RaceId = human.Id, Name = "Versatility", Description = "People excel in many roles and professions.", IconKey = "trophy" },

            new RaceTrait { Id = 6, RaceId = elf.Id, Name = "Superior Reflexes", Description = "Elves possess superior reflexes and agility.", IconKey = "wind" },
            new RaceTrait { Id = 7, RaceId = elf.Id, Name = "Keen Intuition", Description = "Elves possess perceptive minds and a deep understanding of the world around them.", IconKey = "eye" },
            new RaceTrait { Id = 8, RaceId = elf.Id, Name = "Darkvision", Description = "See in the dark up to a distance of 18 meters.", IconKey = "moon" },
            new RaceTrait { Id = 9, RaceId = elf.Id, Name = "Elven Step", Description = "You can hide even when only lightly obscured by mist, rain, foliage, or snow.", IconKey = "leaf" },
            new RaceTrait { Id = 10, RaceId = elf.Id, Name = "Weapon Proficiency", Description = "Skill with bows, swords, and longswords.", IconKey = "sword" },
            new RaceTrait { Id = 11, RaceId = elf.Id, Name = "Immunities", Description = "Immunity to magical sleep effects.", IconKey = "shield" },
            new RaceTrait { Id = 12, RaceId = elf.Id, Name = "Racial Trait", Description = "Elves do not require sleep; instead, they meditate for 4 hours.", IconKey = "moon" },

            new RaceTrait { Id = 13, RaceId = orc.Id, Name = "Powerful Muscles", Description = "An orc's muscles are powerful, and their blows are devastating.", IconKey = "muscle" },
            new RaceTrait { Id = 14, RaceId = orc.Id, Name = "Unusual Resistance", Description = "Orcs are naturally resistant to poison.", IconKey = "flask" },
            new RaceTrait { Id = 15, RaceId = orc.Id, Name = "Wild Angry", Description = "When enraged, an orc's attacks become especially fierce.", IconKey = "fire" },
            new RaceTrait { Id = 16, RaceId = orc.Id, Name = "Fearless", Description = "Little intimidates an orc — they stare down danger without flinching.", IconKey = "mask" },
            new RaceTrait { Id = 17, RaceId = orc.Id, Name = "Speed", Description = "An orc's movement speed is 30 feet.", IconKey = "boot" },

            new RaceTrait { Id = 18, RaceId = dwarf.Id, Name = "Robust Body", Description = "Dwarves possess unwavering physical resilience.", IconKey = "shield" },
            new RaceTrait { Id = 19, RaceId = dwarf.Id, Name = "Poison Resistance", Description = "Dwarves shrug off poison more easily than most.", IconKey = "flask" },
            new RaceTrait { Id = 20, RaceId = dwarf.Id, Name = "Stone Knowledge", Description = "Dwarves recognize masonry and stonework at a glance, drawing on generations of craft.", IconKey = "book" },
            new RaceTrait { Id = 21, RaceId = dwarf.Id, Name = "Darkvision", Description = "You can see in the dark up to 18 meters.", IconKey = "moon" },
            new RaceTrait { Id = 22, RaceId = dwarf.Id, Name = "Axe and Armor Master", Description = "You are proficient with battleaxes, handaxes, and medium armor.", IconKey = "axe" }
        });

        // Placeholder attribute ranges — flavor-consistent (Orc highest Strength, Elf
        // highest Dexterity, Dwarf highest Health, Human balanced) but the exact
        // numbers are TBD, to be tuned once mock data is actually in use.
        var nextId = 1;
        nextId = AddAttributeRanges(store, nextId, human.Id, health: (15, 20), strength: (8, 12), dexterity: (8, 12), intelligence: (8, 12), charisma: (8, 12));
        nextId = AddAttributeRanges(store, nextId, elf.Id, health: (12, 16), strength: (6, 10), dexterity: (12, 18), intelligence: (9, 14), charisma: (8, 13));
        nextId = AddAttributeRanges(store, nextId, orc.Id, health: (16, 22), strength: (13, 18), dexterity: (7, 11), intelligence: (5, 9), charisma: (6, 10));
        AddAttributeRanges(store, nextId, dwarf.Id, health: (18, 24), strength: (10, 15), dexterity: (6, 10), intelligence: (7, 11), charisma: (6, 10));
    }

    private static int AddAttributeRanges(
        InMemoryGameDataStore store,
        int startId,
        int raceId,
        (int min, int max) health,
        (int min, int max) strength,
        (int min, int max) dexterity,
        (int min, int max) intelligence,
        (int min, int max) charisma)
    {
        var id = startId;

        store.RaceAttributeRanges.Add(new RaceAttributeRange { Id = id++, RaceId = raceId, Attribute = AttributeType.Health, MinValue = health.min, MaxValue = health.max });
        store.RaceAttributeRanges.Add(new RaceAttributeRange { Id = id++, RaceId = raceId, Attribute = AttributeType.Strength, MinValue = strength.min, MaxValue = strength.max });
        store.RaceAttributeRanges.Add(new RaceAttributeRange { Id = id++, RaceId = raceId, Attribute = AttributeType.Dexterity, MinValue = dexterity.min, MaxValue = dexterity.max });
        store.RaceAttributeRanges.Add(new RaceAttributeRange { Id = id++, RaceId = raceId, Attribute = AttributeType.Intelligence, MinValue = intelligence.min, MaxValue = intelligence.max });
        store.RaceAttributeRanges.Add(new RaceAttributeRange { Id = id++, RaceId = raceId, Attribute = AttributeType.Charisma, MinValue = charisma.min, MaxValue = charisma.max });

        return id;
    }
}
