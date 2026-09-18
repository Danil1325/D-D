using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

// The Crown of Ash: SIDE QUESTS - MISTHAVEN PORT.
internal static partial class ScenarioSideQuestSeedData
{
    private static void SeedMisthavenPort(InMemoryGameDataStore store)
    {
        AddQuest(store, 1004, "SQ-MP-01", "The Lighthouse Below",
            "Captain Odessa Vray", "The lighthouse beam is being copied beneath the harbor, guiding ships onto hidden rocks. Dive into the flooded foundation, disable the false beacon, and learn who paid for it.",
            3, "misthaven-port", "Slimes in flooded chambers; Wraith keeper.",
            110, "Odessa loyalty +1; fleet route clue.",
            new[]
            {
                Objective("Enter the flooded lighthouse foundation.", ObjectiveType.Travel, "sq-mp-01-objective-1"),
                Objective("Disable the false beacon.", ObjectiveType.CompleteScene, "sq-mp-01-objective-2"),
                Objective("Learn who paid for the beacon.", ObjectiveType.CompleteScene, "sq-mp-01-objective-3")
            },
            new[]
            {
                Enemy(store, "Slime"),
                Enemy(store, "Wraith", count: 1)
            },
            new[]
            {
                Outcome("beacon-disabled", "Disable the beacon and uncover the fleet route clue.",
                    resultFlags: Flags(("false_beacon_disabled", true), ("fleet_route_clue", true)),
                    loyalty: Values(("odessa-vray", 1)))
            });

        AddQuest(store, 1005, "SQ-MP-02", "Nine Rings, No Champion",
            "Arena Master Kael", "A Hobgoblin challenger has defeated every Guild fighter without killing anyone. Discover that he seeks protection from Kregg's enemies, then fight, recruit, or expose him.",
            2, "misthaven-port", "Optional Hobgoblin duel; Guild enforcers if protected.",
            100, "arena reputation; Hobgoblin scout ally.",
            new[]
            {
                Objective("Discover why the Hobgoblin challenger seeks protection.", ObjectiveType.Talk, "sq-mp-02-objective-1"),
                Objective("Fight, recruit, or expose the challenger.", ObjectiveType.MakeChoice, "sq-mp-02-objective-2")
            },
            new[]
            {
                Enemy(store, "Hobgoblin", count: 1, optional: true, requiredFlag: "hobgoblin_duel_chosen"),
                Enemy(store, "Guild enforcers", optional: true, requiredFlag: "hobgoblin_protected")
            },
            new[]
            {
                Outcome("recruited", "Protect and recruit the Hobgoblin scout.",
                    requiredFlags: Flags(("hobgoblin_recruited", true)),
                    resultFlags: Flags(("arena_reputation", true)),
                    allies: Flags(("hobgoblin-scout", true))),
                Outcome("not-recruited", "Resolve the challenge without recruiting the scout.",
                    requiredFlags: Flags(("hobgoblin_recruited", false)),
                    resultFlags: Flags(("arena_reputation", true)))
            });

        AddQuest(store, 1006, "SQ-MP-03", "Lira's Forbidden Chapter",
            "Lira Sunstroke", "Recover a missing grimoire page from the restricted archive before its unfinished spell summons a Dread Wraith.",
            3, "misthaven-port", "Animated books, Phantoms, ritual timer.",
            130, "Lira loyalty +1; arcane item.",
            new[]
            {
                Objective("Enter the restricted archive.", ObjectiveType.Travel, "sq-mp-03-objective-1"),
                Objective("Recover the missing grimoire page before its unfinished spell summons a Dread Wraith.", ObjectiveType.CollectItems, "sq-mp-03-objective-2")
            },
            new[]
            {
                Enemy(store, "Animated books"),
                Enemy(store, "Phantom"),
                Enemy(store, "Dread Wraith", count: 1, optional: true, requiredFlag: "grimoire_ritual_expired")
            },
            new[]
            {
                Outcome("page-recovered", "Recover the page before the ritual finishes.",
                    resultFlags: Flags(("grimoire_page_recovered", true), ("grimoire_ritual_expired", false)),
                    loyalty: Values(("lira-sunstroke", 1)),
                    items: Values(("arcane-item", 1)))
            });

        AddQuest(store, 1007, "SQ-MP-04", "The Price of Passage",
            "Dockworkers' Union", "The Guild has seized medicine as “expedition supplies.” Decide whether to expose the order, steal the crates, or negotiate their release.",
            3, "misthaven-port", "Can be completed without combat; otherwise Guild guards.",
            90, "medicine for Oakheaven; Corvane evidence.",
            new[]
            {
                Objective("Investigate the Guild's seizure of medicine.", ObjectiveType.CompleteScene, "sq-mp-04-objective-1"),
                Objective("Expose the order, steal the crates, or negotiate their release.", ObjectiveType.MakeChoice, "sq-mp-04-objective-2")
            },
            new[]
            {
                Enemy(store, "Guild guards", optional: true, requiredFlag: "medicine_combat_chosen")
            },
            new[]
            {
                Outcome("medicine-released", "Release the medicine and retain evidence against Corvane.",
                    resultFlags: Flags(("oakheaven_medicine_available", true), ("corvane_evidence", true)))
            });

        AddQuest(store, 1008, "SQ-MP-05", "A Ship with Two Names",
            "Captain Odessa Vray", "Odessa's ship appears in the registry under the name of a vessel that sank seven years ago. Search the wreck and recover the original captain's log.",
            5, "misthaven-port", "Drowned Phantoms and one Great Chimera near the wreck.",
            150, "Odessa loyalty +2; Odessa's fleet available later.",
            new[]
            {
                Objective("Search the wreck of the ship that sank seven years ago.", ObjectiveType.Travel, "sq-mp-05-objective-1"),
                Objective("Recover the original captain's log.", ObjectiveType.CollectItems, "sq-mp-05-objective-2")
            },
            new[]
            {
                Enemy(store, "Drowned Phantoms"),
                Enemy(store, "Great Chimera", count: 1)
            },
            new[]
            {
                Outcome("log-recovered", "Recover the log; Odessa's fleet is available later.",
                    resultFlags: Flags(("original_captain_log_recovered", true)),
                    loyalty: Values(("odessa-vray", 2)),
                    allies: Flags(("odessa-fleet", true)))
            });

    }
}
