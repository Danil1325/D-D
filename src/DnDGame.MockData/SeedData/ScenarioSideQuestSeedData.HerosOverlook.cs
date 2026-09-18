using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

// The Crown of Ash: SIDE QUESTS - HERO'S OVERLOOK.
internal static partial class ScenarioSideQuestSeedData
{
    private static void SeedHerosOverlook(InMemoryGameDataStore store)
    {
        AddQuest(store, 1001, "SQ-HO-01", "The Fifth Shadow",
            "Brother Aldwyn", "At sunset, a fifth shadow appears between the four statues. Follow it beneath the overlook and recover a bronze nameplate before three Phantoms erase its inscription.",
            1, "heros-overlook", "3 Phantoms; optional Wraith if the inscription is read aloud.",
            80, "Fifth Hero clue; +1 trust_aldwyn or +1 suspicion.",
            new[]
            {
                Objective("Follow the fifth shadow beneath the overlook.", ObjectiveType.Travel, "sq-ho-01-objective-1"),
                Objective("Recover the bronze nameplate before the Phantoms erase its inscription.", ObjectiveType.CollectItems, "sq-ho-01-objective-2"),
                Objective("Decide whether the clue increases trust in Aldwyn or suspicion.", ObjectiveType.MakeChoice, "sq-ho-01-objective-3")
            },
            new[]
            {
                Enemy(store, "Phantom", count: 3),
                Enemy(store, "Wraith", optional: true, requiredFlag: "inscription_read_aloud")
            },
            new[]
            {
                Outcome("trust-aldwyn", "Recover the clue and increase trust in Aldwyn.",
                    requiredFlags: Flags(("trust_aldwyn_chosen", true)),
                    resultFlags: Flags(("fifth_hero_clue", true)),
                    counters: Values(("trust_aldwyn", 1))),
                Outcome("suspect-aldwyn", "Recover the clue and become more suspicious.",
                    requiredFlags: Flags(("trust_aldwyn_chosen", false)),
                    resultFlags: Flags(("fifth_hero_clue", true)),
                    counters: Values(("suspicion", 1)))
            });

        AddQuest(store, 1002, "SQ-HO-02", "Ash in the Well",
            "Caretaker Mara", "The village well tastes of smoke. Descend through an old maintenance tunnel, destroy the Slimes feeding on corrupted ash, and purify the spring.",
            1, "heros-overlook", "3 Slimes and 1 Great Slime.",
            100, "2 healing supplies; villagers aid the finale.",
            new[]
            {
                Objective("Descend through the well's maintenance tunnel.", ObjectiveType.Travel, "sq-ho-02-objective-1"),
                Objective("Destroy the Slimes feeding on corrupted ash.", ObjectiveType.DefeatEnemies, "sq-ho-02-objective-2"),
                Objective("Purify the spring.", ObjectiveType.CompleteScene, "sq-ho-02-objective-3")
            },
            new[]
            {
                Enemy(store, "Slime", count: 3),
                Enemy(store, "Great Slime", count: 1)
            },
            new[]
            {
                Outcome("purified", "Restore the spring; villagers aid the finale.",
                    resultFlags: Flags(("spring_purified", true)),
                    items: Values(("healing-supply", 2)),
                    allies: Flags(("overlook-villagers", true)))
            });

        AddQuest(store, 1003, "SQ-HO-03", "Four Names for the Dead",
            "Families of missing adventurers", "Find four abandoned Guild tokens along the old road and return them to their families. The last token is carried by a Skeleton Knight who still remembers its owner.",
            2, "heros-overlook", "Skeleton group; mercy option for the Knight.",
            90, "memorial charm; companion loyalty +1.",
            new[]
            {
                Objective("Find the abandoned Guild tokens.", ObjectiveType.CollectItems, "sq-ho-03-objective-1", 4),
                Objective("Resolve the encounter with the Skeleton Knight, including the mercy option.", ObjectiveType.MakeChoice, "sq-ho-03-objective-2"),
                Objective("Return the tokens to their families.", ObjectiveType.Talk, "sq-ho-03-objective-3")
            },
            new[]
            {
                Enemy(store, "Skeleton"),
                Enemy(store, "Skeleton Knight", count: 1, optional: true)
            },
            new[]
            {
                Outcome("returned", "Return the tokens and honor the missing adventurers.",
                    resultFlags: Flags(("guild_tokens_returned", true)),
                    loyalty: Values(("chosen-companion", 1)),
                    items: Values(("memorial-charm", 1)))
            });

    }
}
