using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

// The Crown of Ash: SIDE QUESTS - DARKSTORM KEEP.
internal static partial class ScenarioSideQuestSeedData
{
    private static void SeedDarkstormKeep(InMemoryGameDataStore store)
    {
        AddQuest(store, 1026, "SQ-DK-01", "Prisoners of the Seventh Year",
            "Whisper from the walls", "Find adventurers kept inside magical portraits, defeat the Wraith curators, and choose which memory each prisoner may keep when freed.",
            8, "darkstorm-keep", "Wraiths and one Dread Wraith.",
            190, "two survivors join the final defense.",
            new[]
            {
                Objective("Find the adventurers imprisoned in magical portraits.", ObjectiveType.Travel, "sq-dk-01-objective-1"),
                Objective("Defeat the Wraith curators.", ObjectiveType.DefeatEnemies, "sq-dk-01-objective-2"),
                Objective("Choose which memory each freed prisoner may keep.", ObjectiveType.MakeChoice, "sq-dk-01-objective-3")
            },
            new[]
            {
                Enemy(store, "Wraith"),
                Enemy(store, "Dread Wraith", count: 1)
            },
            new[]
            {
                Outcome("prisoners-freed", "Free two survivors who join the final defense.",
                    counters: Values(("final_defense_survivors", 2)),
                    allies: Flags(("portrait-survivors", true)))
            });

        AddQuest(store, 1027, "SQ-DK-02", "The Chimera's Three Hearts",
            "Divine Chimera", "Recover the three heart-seals removed by Vharruk. The beast can be defeated for its power or restored as an ally.",
            9, "darkstorm-keep", "Three Great Chimera aspects.",
            220, "Divine Chimera ally or legendary material.",
            new[]
            {
                Objective("Recover the three heart-seals removed by Vharruk.", ObjectiveType.CollectItems, "sq-dk-02-objective-1", 3),
                Objective("Defeat the Divine Chimera for its power or restore it as an ally.", ObjectiveType.MakeChoice, "sq-dk-02-objective-2")
            },
            new[]
            {
                Enemy(store, "Great Chimera aspects", count: 3),
                Enemy(store, "Divine Chimera", count: 1, optional: true, requiredFlag: "divine_chimera_defeated")
            },
            new[]
            {
                Outcome("restored", "Restore the Divine Chimera as an ally.",
                    requiredFlags: Flags(("divine_chimera_restored", true)),
                    allies: Flags(("divine-chimera", true))),
                Outcome("defeated", "Defeat the Divine Chimera and receive legendary material.",
                    requiredFlags: Flags(("divine_chimera_restored", false)),
                    items: Values(("legendary-material", 1)))
            });

        AddQuest(store, 1028, "SQ-DK-03", "Karnyx's Debt",
            "Karnyx", "Destroy the ledger that binds Karnyx and the Greater Demons to Vharruk. Mercy weakens the enemy army more than simply defeating Karnyx.",
            9, "darkstorm-keep", "Greater Demons; optional Karnyx boss.",
            210, "Gate of Fire weakened; War Score +1 through truce.",
            new[]
            {
                Objective("Destroy the ledger binding Karnyx and the Greater Demons to Vharruk.", ObjectiveType.CompleteScene, "sq-dk-03-objective-1"),
                Objective("Decide whether to offer Karnyx mercy and a truce.", ObjectiveType.MakeChoice, "sq-dk-03-objective-2")
            },
            new[]
            {
                Enemy(store, "Greater Demon"),
                Enemy(store, "Karnyx", count: 1, optional: true, requiredFlag: "karnyx_combat_chosen")
            },
            new[]
            {
                Outcome("truce", "Free Karnyx through a truce and weaken the Gate of Fire.",
                    requiredFlags: Flags(("karnyx_truce", true)),
                    resultFlags: Flags(("gate_of_fire_weakened", true)),
                    warScore: 1),
                Outcome("no-truce", "Destroy the ledger and weaken the Gate of Fire without the truce bonus.",
                    requiredFlags: Flags(("karnyx_truce", false)),
                    resultFlags: Flags(("gate_of_fire_weakened", true)))
            });

        AddQuest(store, 1029, "SQ-DK-04", "The Statue That Never Was",
            "Fellowship echo", "Collect four fragments of the erased fifth statue and assemble them in the Vault. The completed image proves the traitor's identity.",
            9, "darkstorm-keep", "Echo fights mirroring the four original heroes.",
            230, "decisive traitor evidence; special dialogue in MQ-12.",
            new[]
            {
                Objective("Collect the fragments of the erased fifth statue.", ObjectiveType.CollectItems, "sq-dk-04-objective-1", 4),
                Objective("Assemble the statue in the Fellowship Vault.", ObjectiveType.CompleteScene, "sq-dk-04-objective-2")
            },
            new[]
            {
                Enemy(store, "Fellowship echoes", count: 4)
            },
            new[]
            {
                Outcome("statue-assembled", "Assemble the proof of the traitor's identity and unlock special MQ-12 dialogue.",
                    resultFlags: Flags(("decisive_traitor_evidence", true), ("mq12_statue_dialogue", true)))
            });

    }
}
