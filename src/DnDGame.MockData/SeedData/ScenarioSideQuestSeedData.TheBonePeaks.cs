using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

// The Crown of Ash: SIDE QUESTS - THE BONE PEAKS.
internal static partial class ScenarioSideQuestSeedData
{
    private static void SeedTheBonePeaks(InMemoryGameDataStore store)
    {
        AddQuest(store, 1021, "SQ-BP-01", "A Knight's Last Order",
            "Ser Halbrecht", "Recover the command seal of Halbrecht's company and release three Skeleton Knights still patrolling a battle that ended centuries ago.",
            6, "the-bone-peaks", "Skeleton Knights; mercy ritual available.",
            140, "Halbrecht loyalty +1; undead passage.",
            new[]
            {
                Objective("Recover Halbrecht's command seal.", ObjectiveType.CollectItems, "sq-bp-01-objective-1"),
                Objective("Release the three Skeleton Knights, including through the mercy ritual.", ObjectiveType.CompleteScene, "sq-bp-01-objective-2", 3)
            },
            new[]
            {
                Enemy(store, "Skeleton Knight", count: 3, optional: true)
            },
            new[]
            {
                Outcome("knights-released", "Release the company and obtain undead passage.",
                    resultFlags: Flags(("undead_passage", true)),
                    loyalty: Values(("ser-halbrecht", 1)))
            });

        AddQuest(store, 1022, "SQ-BP-02", "The Troll King's Challenge",
            "Grommash-Vurr", "Complete three trials - endurance, courage, and mercy - to earn the right to challenge the Troll King without fighting his entire warband.",
            6, "the-bone-peaks", "Warrior Troll trial fights.",
            160, "honorable duel; potential troll alliance.",
            new[]
            {
                Objective("Complete the trial of endurance.", ObjectiveType.CompleteScene, "sq-bp-02-objective-1"),
                Objective("Complete the trial of courage.", ObjectiveType.CompleteScene, "sq-bp-02-objective-2"),
                Objective("Complete the trial of mercy.", ObjectiveType.CompleteScene, "sq-bp-02-objective-3")
            },
            new[]
            {
                Enemy(store, "Warrior Troll")
            },
            new[]
            {
                Outcome("trials-completed", "Earn an honorable duel and the possibility of a troll alliance.",
                    resultFlags: Flags(("honorable_troll_duel", true), ("troll_alliance_route", true)))
            });

        AddQuest(store, 1023, "SQ-BP-03", "Seven Cold Shrines",
            "Echo of Nerath-Dur", "Relight seven shrines across Karag-Dur while the Lich's curse sends stronger undead after each flame.",
            7, "the-bone-peaks", "Skeletons, Skeleton Knights, final Wraith.",
            180, "Lich weakened; corruption -1.",
            new[]
            {
                Objective("Relight the cold shrines across Karag-Dur.", ObjectiveType.CompleteScene, "sq-bp-03-objective-1", 7),
                Objective("Survive the increasingly strong undead sent by the Lich's curse.", ObjectiveType.CompleteScene, "sq-bp-03-objective-2")
            },
            new[]
            {
                Enemy(store, "Skeleton"),
                Enemy(store, "Skeleton Knight"),
                Enemy(store, "Wraith", count: 1)
            },
            new[]
            {
                Outcome("shrines-lit", "Relight the shrines and weaken the Lich.",
                    resultFlags: Flags(("lich_weakened", true)),
                    corruption: -1)
            });

        AddQuest(store, 1024, "SQ-BP-04", "The Forge Remembers",
            "Dwarven ancestor stone", "Recover three forge tools from collapsed halls and repair the mechanism that can later reforge the Crown.",
            7, "the-bone-peaks", "Trolls, traps, Great Slime in the cooling cistern.",
            170, "Karag-Dur forge activated.",
            new[]
            {
                Objective("Recover the forge tools from the collapsed halls.", ObjectiveType.CollectItems, "sq-bp-04-objective-1", 3),
                Objective("Repair and activate Karag-Dur's forge.", ObjectiveType.CompleteScene, "sq-bp-04-objective-2")
            },
            new[]
            {
                Enemy(store, "Troll"),
                Enemy(store, "Great Slime", count: 1)
            },
            new[]
            {
                Outcome("forge-repaired", "Activate the forge that can later reforge the Crown.",
                    resultFlags: Flags(("karag_dur_forge", true)))
            });

        AddQuest(store, 1025, "SQ-BP-05", "Silence Has Teeth",
            "Bram Ironjaw", "The unnatural silence is created by a Great Chimera feeding on sound. Track it by vibration and prevent it from consuming Karag-Dur's final lament.",
            7, "the-bone-peaks", "Great Chimera boss.",
            190, "Bram loyalty +1; sonic protection item.",
            new[]
            {
                Objective("Track the sound-feeding Great Chimera by vibration.", ObjectiveType.CompleteScene, "sq-bp-05-objective-1"),
                Objective("Prevent the Chimera from consuming Karag-Dur's final lament.", ObjectiveType.CompleteScene, "sq-bp-05-objective-2")
            },
            new[]
            {
                Enemy(store, "Great Chimera", count: 1)
            },
            new[]
            {
                Outcome("lament-preserved", "Preserve the final lament.",
                    resultFlags: Flags(("karag_dur_lament_preserved", true)),
                    loyalty: Values(("bram-ironjaw", 1)),
                    items: Values(("sonic-protection-item", 1)))
            });

    }
}
