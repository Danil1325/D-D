using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

// The Crown of Ash: SIDE QUESTS - WHISPERING WOODS.
internal static partial class ScenarioSideQuestSeedData
{
    private static void SeedWhisperingWoods(InMemoryGameDataStore store)
    {
        AddQuest(store, 1009, "SQ-WW-01", "Roots That Speak",
            "Sil'Vaneth", "Three heart-tree roots repeat the voices of lost travelers. Enter their memories, identify the real call for help, and free the trapped Elf scout.",
            3, "whispering-woods", "Phantoms; false-memory puzzles.",
            120, "Elf scout ally; forest shortcut.",
            new[]
            {
                Objective("Enter the memories of the three heart-tree roots.", ObjectiveType.CompleteScene, "sq-ww-01-objective-1", 3),
                Objective("Identify the real call for help.", ObjectiveType.MakeChoice, "sq-ww-01-objective-2"),
                Objective("Free the trapped Elf scout.", ObjectiveType.CompleteScene, "sq-ww-01-objective-3")
            },
            new[]
            {
                Enemy(store, "Phantom")
            },
            new[]
            {
                Outcome("scout-freed", "Free the scout and discover the forest shortcut.",
                    resultFlags: Flags(("elf_scout_freed", true), ("forest_shortcut", true)),
                    allies: Flags(("elf-scout", true)))
            });

        AddQuest(store, 1010, "SQ-WW-02", "The Goblin Who Planted Flowers",
            "Pip Moss-Ear", "A Goblin herbalist is blamed for poisoned flowers, but the poison comes from a corrupted Great Slime. Choose whether to protect Pip from hunters.",
            3, "whispering-woods", "Great Slime; optional conflict with hunters.",
            100, "antidotes; Pip becomes a merchant.",
            new[]
            {
                Objective("Discover that a corrupted Great Slime is poisoning the flowers.", ObjectiveType.CompleteScene, "sq-ww-02-objective-1"),
                Objective("Resolve the Great Slime threat.", ObjectiveType.CompleteScene, "sq-ww-02-objective-2"),
                Objective("Choose whether to protect Pip from the hunters.", ObjectiveType.MakeChoice, "sq-ww-02-objective-3")
            },
            new[]
            {
                Enemy(store, "Great Slime", count: 1),
                Enemy(store, "Hunters", optional: true, requiredFlag: "pip_protected")
            },
            new[]
            {
                Outcome("pip-protected", "Protect Pip, who becomes a merchant.",
                    requiredFlags: Flags(("pip_protected", true)),
                    resultFlags: Flags(("pip_merchant_available", true))),
                Outcome("pip-unprotected", "Resolve the poison without promising Pip's protection.",
                    requiredFlags: Flags(("pip_protected", false)))
            });

        AddQuest(store, 1011, "SQ-WW-03", "Moonlight Without a Moon",
            "Sil'Vaneth", "The forest's moonwell has gone dark. Carry a living flame through Wraith territory without letting it be extinguished.",
            4, "whispering-woods", "Two Wraith ambushes; movement challenge.",
            140, "moonwell blessing; corruption -1.",
            new[]
            {
                Objective("Carry a living flame through Wraith territory without extinguishing it.", ObjectiveType.Travel, "sq-ww-03-objective-1"),
                Objective("Restore the moonwell.", ObjectiveType.CompleteScene, "sq-ww-03-objective-2")
            },
            new[]
            {
                Enemy(store, "Wraith")
            },
            new[]
            {
                Outcome("moonwell-restored", "Restore the moonwell and receive its blessing.",
                    resultFlags: Flags(("moonwell_restored", true), ("moonwell_blessing", true)),
                    corruption: -1)
            });

        AddQuest(store, 1012, "SQ-WW-04", "The Dread Wraith's Name",
            "The forgotten Elf scout", "Collect three memory leaves and speak the Wraith's mortal name. Destroying it is faster; restoring it reveals the Fifth Hero.",
            6, "whispering-woods", "Dread Wraith boss or ritual resolution.",
            170, "traitor clue; Sil'Vaneth loyalty +1.",
            new[]
            {
                Objective("Collect three memory leaves.", ObjectiveType.CollectItems, "sq-ww-04-objective-1", 3),
                Objective("Speak the Wraith's mortal name and decide whether to destroy or restore it.", ObjectiveType.MakeChoice, "sq-ww-04-objective-2")
            },
            new[]
            {
                Enemy(store, "Dread Wraith", count: 1, optional: true, requiredFlag: "dread_wraith_combat_chosen")
            },
            new[]
            {
                Outcome("restored", "Restore the Wraith and reveal the Fifth Hero.",
                    requiredFlags: Flags(("dread_wraith_restored", true)),
                    resultFlags: Flags(("traitor_clue", true), ("fifth_hero_revealed", true)),
                    loyalty: Values(("silvaneth", 1))),
                Outcome("destroyed", "Destroy the Wraith instead of restoring it.",
                    requiredFlags: Flags(("dread_wraith_restored", false)),
                    resultFlags: Flags(("traitor_clue", true)),
                    loyalty: Values(("silvaneth", 1)))
            }, requiredFlags: Flags(("elf_scout_freed", true)));

    }
}
