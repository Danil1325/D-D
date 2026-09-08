using DnDGame.Domain.Entities.Talents;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds a small curated pool of talents. Each grants a flat bonus either to checks
/// using one specific attribute, or (when AppliesToAttribute is null) to any check.
/// Values are placeholders, consistent with everything else tagged TBD in this phase.
/// </summary>
internal static class TalentSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        store.Talents.AddRange(new[]
        {
            new Talent { Id = 1, Name = "Iron Will", Description = "A small, steady bonus to any check you attempt.", MinLevel = 1, AppliesToAttribute = null, BonusValue = 1 },
            new Talent { Id = 2, Name = "Strong Arm", Description = "Your raw power shows whenever strength is put to the test.", MinLevel = 1, AppliesToAttribute = AttributeType.Strength, BonusValue = 2 },
            new Talent { Id = 3, Name = "Quick Reflexes", Description = "You react faster than most in tense moments.", MinLevel = 1, AppliesToAttribute = AttributeType.Dexterity, BonusValue = 2 },
            new Talent { Id = 4, Name = "Sharp Mind", Description = "Careful reasoning gives you an edge in mental challenges.", MinLevel = 1, AppliesToAttribute = AttributeType.Intelligence, BonusValue = 2 },
            new Talent { Id = 5, Name = "Silver Tongue", Description = "People tend to listen when you talk.", MinLevel = 1, AppliesToAttribute = AttributeType.Charisma, BonusValue = 2 },
            new Talent { Id = 6, Name = "Enduring Spirit", Description = "Your resilience helps you push through exhausting ordeals.", MinLevel = 1, AppliesToAttribute = AttributeType.Health, BonusValue = 2 },
            new Talent { Id = 7, Name = "Linguist", Description = "A well-traveled mind helps in more situations than you'd expect.", MinLevel = 4, AppliesToAttribute = null, BonusValue = 1 },
            new Talent { Id = 8, Name = "Battle Hardened", Description = "Years of combat have sharpened your physical instincts.", MinLevel = 4, AppliesToAttribute = AttributeType.Strength, BonusValue = 3 },
            new Talent { Id = 9, Name = "Keen Senses", Description = "You notice what others miss.", MinLevel = 8, AppliesToAttribute = AttributeType.Dexterity, BonusValue = 3 },
            new Talent { Id = 10, Name = "Arcane Insight", Description = "A deeper understanding of the arcane sharpens your judgment.", MinLevel = 8, AppliesToAttribute = AttributeType.Intelligence, BonusValue = 3 },
            new Talent { Id = 11, Name = "Inspiring Presence", Description = "Your confidence rubs off on everyone nearby.", MinLevel = 8, AppliesToAttribute = AttributeType.Charisma, BonusValue = 3 },
            new Talent { Id = 12, Name = "Unbreakable", Description = "Nothing seems to wear you down anymore.", MinLevel = 12, AppliesToAttribute = AttributeType.Health, BonusValue = 4 }
        });
    }
}
