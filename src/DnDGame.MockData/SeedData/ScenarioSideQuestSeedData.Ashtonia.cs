using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

// The Crown of Ash: SIDE QUESTS - ASHTONIA.
internal static partial class ScenarioSideQuestSeedData
{
    private static void SeedAshtonia(InMemoryGameDataStore store)
    {
        AddQuest(store, 1013, "SQ-AS-01", "Names in the Furnace",
            "Mother Sereh", "Demons have carved the names of living Orcs into cooling iron. Break the plates before the volcano's next pulse binds those people to the seal.",
            4, "ashtonia", "Demons and one Greater Demon smith.",
            130, "Mother Sereh loyalty +1; fire resistance supply.",
            new[]
            {
                Objective("Find the cooling iron plates bearing living Orcs' names.", ObjectiveType.Travel, "sq-as-01-objective-1"),
                Objective("Break the plates before the volcano's next pulse binds the named Orcs to the seal.", ObjectiveType.CompleteScene, "sq-as-01-objective-2")
            },
            new[]
            {
                Enemy(store, "Demon"),
                Enemy(store, "Greater Demon smith", count: 1)
            },
            new[]
            {
                Outcome("names-freed", "Break the nameplates before the binding.",
                    resultFlags: Flags(("orc_names_freed", true)),
                    loyalty: Values(("mother-sereh", 1)),
                    items: Values(("fire-resistance-supply", 1)))
            });

        AddQuest(store, 1014, "SQ-AS-02", "The Last Clan Drum",
            "Orc refugee Dorga", "Recover a ceremonial drum from a lava-cut shrine. Its rhythm keeps lesser Demons from approaching, but sounding it also alerts Karnyx.",
            5, "ashtonia", "Demon packs; timed escape.",
            120, "safe camp in Ashtonia; War Score +1 if preserved.",
            new[]
            {
                Objective("Recover the ceremonial drum from the lava-cut shrine.", ObjectiveType.CollectItems, "sq-as-02-objective-1"),
                Objective("Escape with the drum and decide how to use its warning rhythm.", ObjectiveType.MakeChoice, "sq-as-02-objective-2")
            },
            new[]
            {
                Enemy(store, "Demon")
            },
            new[]
            {
                Outcome("drum-preserved", "Preserve the drum and establish a safe camp.",
                    requiredFlags: Flags(("clan_drum_preserved", true)),
                    resultFlags: Flags(("ashtonia_safe_camp", true)),
                    warScore: 1),
                Outcome("drum-not-preserved", "Resolve the escape without preserving the drum's finale advantage.",
                    requiredFlags: Flags(("clan_drum_preserved", false)),
                    resultFlags: Flags(("ashtonia_safe_camp", true)))
            });

        AddQuest(store, 1015, "SQ-AS-03", "A Bargain with No Signature",
            "Bound Demon Irix", "A lesser Demon claims it was forced into Vharruk's service without signing the contract. Find its true name and decide whether to free, banish, or recruit it as an informant.",
            5, "ashtonia", "Ritual encounter; Greater Demon jailer.",
            150, "hidden gate information; corruption varies by choice.",
            new[]
            {
                Objective("Find Irix's true name.", ObjectiveType.CollectItems, "sq-as-03-objective-1"),
                Objective("Free, banish, or recruit Irix as an informant.", ObjectiveType.MakeChoice, "sq-as-03-objective-2")
            },
            new[]
            {
                Enemy(store, "Greater Demon jailer", count: 1)
            },
            new[]
            {
                Outcome("free", "Resolve Irix's bargain; corruption varies by choice, with no numeric delta specified in the source.",
                    requiredFlags: Flags(("irix_free", true)),
                    resultFlags: Flags(("hidden_gate_information", true)),
                    corruption: null),
                Outcome("banish", "Resolve Irix's bargain; corruption varies by choice, with no numeric delta specified in the source.",
                    requiredFlags: Flags(("irix_banish", true)),
                    resultFlags: Flags(("hidden_gate_information", true)),
                    corruption: null),
                Outcome("recruit", "Resolve Irix's bargain; corruption varies by choice, with no numeric delta specified in the source.",
                    requiredFlags: Flags(("irix_recruit", true)),
                    resultFlags: Flags(("hidden_gate_information", true), ("irix_informant", true)),
                    allies: Flags(("irix-informant", true)),
                    corruption: null)
            });

        AddQuest(store, 1016, "SQ-AS-04", "The Ember That Refused",
            "Mother Sereh", "An ember from the original sealing still burns beneath the shrine. Protect it from a Chimera drawn to its magic and carry it to Hero's Overlook.",
            6, "ashtonia", "Chimera boss.",
            170, "Sealing Ember; advantage in final Phase Three.",
            new[]
            {
                Objective("Protect the original sealing ember from the Chimera.", ObjectiveType.CompleteScene, "sq-as-04-objective-1"),
                Objective("Carry the ember to Hero's Overlook.", ObjectiveType.Travel, "sq-as-04-objective-2")
            },
            new[]
            {
                Enemy(store, "Chimera", count: 1)
            },
            new[]
            {
                Outcome("ember-delivered", "Deliver the Sealing Ember for an advantage in final Phase Three.",
                    resultFlags: Flags(("sealing_ember_preserved", true), ("final_phase_three_advantage", true)),
                    items: Values(("sealing-ember", 1)))
            });

    }
}
