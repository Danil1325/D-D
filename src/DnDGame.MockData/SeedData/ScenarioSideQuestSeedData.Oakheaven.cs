using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

// The Crown of Ash: SIDE QUESTS - OAKHEAVEN.
internal static partial class ScenarioSideQuestSeedData
{
    private static void SeedOakheaven(InMemoryGameDataStore store)
    {
        AddQuest(store, 1017, "SQ-OH-01", "The Mill Race",
            "Mirelle Ashcroft", "Slimes have blocked the mill channel, stopping the only clean water source. Clear the channel without destroying the old floodgate.",
            3, "oakheaven", "Slimes and one King Slime if Ash Clock is 4+.",
            110, "clean water; militia health +1.",
            new[]
            {
                Objective("Clear the Slimes from the mill channel.", ObjectiveType.CompleteScene, "sq-oh-01-objective-1"),
                Objective("Preserve the old floodgate and restore clean water.", ObjectiveType.CompleteScene, "sq-oh-01-objective-2")
            },
            new[]
            {
                Enemy(store, "Slime"),
                Enemy(store, "King Slime", count: 1, minimumAshClock: 4)
            },
            new[]
            {
                Outcome("water-restored", "Restore clean water without destroying the floodgate.",
                    resultFlags: Flags(("oakheaven_clean_water", true)),
                    counters: Values(("militia_health", 1)))
            });

        AddQuest(store, 1018, "SQ-OH-02", "Kregg's Stolen Banner",
            "Kregg the Sundered", "A Demon patrol stole Kregg's banner to provoke Goblins into attacking the town. Recover it and reveal the deception.",
            4, "oakheaven", "Hobgoblins under possession; Demons.",
            130, "Kregg loyalty +1; goblin alliance route.",
            new[]
            {
                Objective("Recover Kregg's banner from the Demon patrol.", ObjectiveType.CollectItems, "sq-oh-02-objective-1"),
                Objective("Reveal the deception to Kregg and the Goblins.", ObjectiveType.Talk, "sq-oh-02-objective-2")
            },
            new[]
            {
                Enemy(store, "Possessed Hobgoblins"),
                Enemy(store, "Demon")
            },
            new[]
            {
                Outcome("banner-returned", "Return the banner and open the goblin alliance route.",
                    resultFlags: Flags(("goblin_alliance_route", true)),
                    loyalty: Values(("kregg", 1)))
            });

        AddQuest(store, 1019, "SQ-OH-03", "Eleven Empty Beds",
            "Mirelle Ashcroft", "Eleven townsfolk vanish every night and return at dawn with no memory. Follow them into the orchard and break the possession circle.",
            4, "oakheaven", "Phantoms and a Greater Demon ritualist.",
            150, "civilians restored; Ash Clock does not advance after resting here.",
            new[]
            {
                Objective("Follow the eleven missing townsfolk into the orchard.", ObjectiveType.Travel, "sq-oh-03-objective-1"),
                Objective("Break the possession circle and restore the civilians.", ObjectiveType.CompleteScene, "sq-oh-03-objective-2")
            },
            new[]
            {
                Enemy(store, "Phantom"),
                Enemy(store, "Greater Demon ritualist", count: 1)
            },
            new[]
            {
                Outcome("civilians-restored", "Restore civilians; resting in Oakheaven no longer advances the Ash Clock.",
                    resultFlags: Flags(("oakheaven_civilians_restored", true), ("oakheaven_rest_without_ash_clock", true)))
            });

        AddQuest(store, 1020, "SQ-OH-04", "The Mayor's Real Voice",
            "Former clerk Tomas", "Find recordings hidden in the town hall proving when the mayor was possessed. The evidence can convince frightened townsfolk to cooperate.",
            4, "oakheaven", "Possessed guards; stealth or non-lethal route.",
            100, "easier Oakheaven climax; civilian losses reduced.",
            new[]
            {
                Objective("Find the mayor's hidden recordings in the town hall.", ObjectiveType.CollectItems, "sq-oh-04-objective-1"),
                Objective("Use the evidence to convince the frightened townsfolk to cooperate.", ObjectiveType.Talk, "sq-oh-04-objective-2")
            },
            new[]
            {
                Enemy(store, "Possessed guards", optional: true, requiredFlag: "town_hall_combat_chosen")
            },
            new[]
            {
                Outcome("evidence-shared", "Use the evidence to ease Oakheaven's climax and reduce civilian losses.",
                    resultFlags: Flags(("mayor_possession_evidence", true), ("oakheaven_climax_easier", true), ("civilian_losses_reduced", true)))
            });

    }
}
