using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Main Quest Chain from The_Crown_of_Ash_Interactive_Scenario.docx.
/// See docs/TheCrownOfAshMainQuestSeed.md for source mapping and technical conventions.
/// </summary>
internal static class ScenarioMainQuestSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        SeedSceneReferences(store);
        store.Quests.AddRange(new[]
        {
            CreateQuest(1, "Ash on the Overlook",
                "Inspect the cracked statue, receive the Guild summons, and decide whether to trust Aldwyn.",
                "Brother Aldwyn", 1, 1, new[] { 3 }, new[] { 1001 },
                100, "Guild Summons; trust flag.",
                "",
                new[]
                {
                    Objective(101, "Inspect the cracked statue and read the inscription if the inspection succeeds.",
                        ObjectiveType.MakeChoice, targetCode: "inspect-cracked-statue", isOptional: true),
                    Objective(102, "Receive the Guild summons from Brother Aldwyn.",
                        ObjectiveType.CollectItems, targetCode: "guild-summons"),
                    Objective(103, "Decide whether to trust Aldwyn or follow your racial instinct.",
                        ObjectiveType.MakeChoice, targetCode: "aldwyn-trust")
                }),
            CreateQuest(2, "The Road That Is Yours",
                "Follow the Elf, Orc, Human, or Dwarf opening and discover the first evidence connected to the Crown.",
                "Racial route", 1, 2, new[] { 7, 1, 4, 6 }, new[] { 1201, 1202, 1203, 1204 },
                120, "Regional ally or early fragment opportunity.",
                "Only the selected race's opening is required; every route later converges on Misthaven Port.",
                new[]
                {
                    Objective(201, "Follow the opening for the selected race: Whispering Woods, Ashtonia, Misthaven Port, or Karag-Dur.",
                        ObjectiveType.CompleteScene, targetCode: "racial-opening"),
                    Objective(202, "Discover the first evidence connected to the Crown on the chosen route.",
                        ObjectiveType.CollectItems, targetCode: "opening-crown-evidence")
                }),
            CreateQuest(3, "The Lighthouse Guild",
                "Reach Misthaven Port, complete the class registration trial, and sign the Oakheaven contract.",
                "Guildmaster Corvane", 2, 2, new[] { 4 }, new[] { 2003, 2004 },
                150, "Guild rank; first companion slot.",
                "Humans may register without the usual class trial. The 150 EXP registration reward and the quest reward are the same reward, not two grants.",
                new[]
                {
                    Objective(301, "Reach the Adventurers' Guild in Misthaven Port.",
                        ObjectiveType.Travel, targetId: 4),
                    Objective(302, "Register with the Guild through the class trial, or the Human admission route.",
                        ObjectiveType.MakeChoice, targetCode: "guild-registration"),
                    Objective(303, "Sign the contract to investigate Oakheaven.",
                        ObjectiveType.MakeChoice, targetCode: "oakheaven-contract")
                }),
            CreateQuest(4, "Pages That Should Be Missing",
                "Enter the Guild archive and recover the altered expedition report without losing the evidence.",
                "Chosen companion", 3, 3, new[] { 4 }, new[] { 2005 },
                140, "investigation_evidence +1.",
                "",
                new[]
                {
                    Objective(401, "Enter the Guild archive with the chosen companion.",
                        ObjectiveType.Travel, targetCode: "guild-archive"),
                    Objective(402, "Recover the altered expedition report and preserve the evidence.",
                        ObjectiveType.CollectItems, targetCode: "altered-expedition-report"),
                    Objective(403, "Choose how to handle Corvane and the recovered report.",
                        ObjectiveType.MakeChoice, targetCode: "archive-report-response")
                }),
            CreateQuest(5, "The Silent Grain Road",
                "Travel from Misthaven to Oakheaven, inspect the abandoned caravans, and keep at least one survivor alive.",
                "Corvane Ashford", 3, 4, new[] { 5 }, new[] { 3005 },
                180, "Supplies; Oakheaven entry route.",
                "",
                new[]
                {
                    Objective(501, "Travel from Misthaven Port toward Oakheaven.",
                        ObjectiveType.Travel, targetId: 5),
                    Objective(502, "Inspect the abandoned grain caravans.",
                        ObjectiveType.CompleteScene, targetCode: "abandoned-caravans"),
                    Objective(503, "Keep at least one caravan survivor alive.",
                        ObjectiveType.CompleteScene, targetCode: "grain-road-survivor")
                }),
            CreateQuest(6, "The Unburned Town",
                "Discover the possession network and choose the Mage, Warrior, Bard, or Healer solution for Oakheaven.",
                "Mirelle Ashcroft", 4, 5, new[] { 5 }, new[] { 3006, 3007 },
                250, "oakheaven_result; militia 0-3 War Score.",
                "",
                new[]
                {
                    Objective(601, "Discover the possession sigils with Mirelle Ashcroft.",
                        ObjectiveType.Talk, targetCode: "mirelle-ashcroft"),
                    Objective(602, "Resolve Oakheaven through the Mage, Warrior, Bard, or Healer solution.",
                        ObjectiveType.MakeChoice, targetCode: "oakheaven-solution"),
                    Objective(603, "Hear the final demon's warning and establish Oakheaven's narrative result.",
                        ObjectiveType.CompleteScene, targetId: 3007)
                }),
            CreateQuest(7, "The First Crown Fragment",
                "Recover whichever Crown Fragment the player chooses first.",
                "Open objective", 4, 6, new[] { 1, 7, 6 }, new[] { 4008, 4009, 4010 },
                220, "first_fragment; +1 investigation clue.",
                "MQ-07 through MQ-09 count collection order, not a fixed region order. Regional fragment flags still identify Ashtonia, Whispering Woods and Karag-Dur respectively. Previously recovered fragments count.",
                new[]
                {
                    Objective(701, "Recover one distinct Crown Fragment from any of the three regions; an opening-route fragment counts.",
                        ObjectiveType.CollectItems, targetCode: "crown-fragments")
                }),
            CreateQuest(8, "The Second Crown Fragment",
                "Reach a second fragment location and resolve its guardian's moral demand.",
                "Open objective", 6, 7, new[] { 1, 7, 6 }, new[] { 4008, 4009, 4010 },
                260, "second_fragment; companion loyalty opportunity.",
                "Choose an uncollected regional fragment. The guardian can be resolved without combat; loyalty depends on the player's choice.",
                new[]
                {
                    Objective(801, "Resolve the moral demand of the guardian at a second fragment location.",
                        ObjectiveType.MakeChoice, targetCode: "second-fragment-guardian"),
                    Objective(802, "Have recovered two distinct Crown Fragments, in any regional order.",
                        ObjectiveType.CollectItems, targetCode: "crown-fragments", requiredAmount: 2)
                }),
            CreateQuest(9, "The Third Crown Fragment",
                "Claim the final fragment and survive the memory released when the Crown becomes complete.",
                "Open objective", 7, 8, new[] { 1, 7, 6 }, new[] { 4008, 4009, 4010, 4111, 4112, 4113 },
                320, "third_fragment; traitor evidence completed.",
                "The completed Crown's memory occurs in whichever region supplies the last fragment.",
                new[]
                {
                    Objective(901, "Have recovered all three distinct Crown Fragments, including any obtained during the opening.",
                        ObjectiveType.CollectItems, targetCode: "crown-fragments", requiredAmount: 3),
                    Objective(902, "Survive the memory released when the Crown becomes complete.",
                        ObjectiveType.CompleteScene, targetCode: "completed-crown-memory")
                }),
            CreateQuest(10, "The Traitor's Errand",
                "Compare the evidence from Aldwyn, Corvane, the Guild records, and the fragments; identify the living traitor.",
                "Story revelation", 8, 8, new[] { 1, 7, 6 }, new[] { 4111, 4112, 4113 },
                220, "traitor set; hidden route into Darkstorm Keep.",
                "If both investigation trails were ignored, the document defers the traitor's identity to the throne room; do not block the campaign waiting for a name here.",
                new[]
                {
                    Objective(1001, "Compare the evidence from Aldwyn, Corvane, the Guild records, and the fragments.",
                        ObjectiveType.CompleteScene, targetCode: "traitor-investigation"),
                    Objective(1002, "Establish the traitor outcome: Corvane, Aldwyn, or revelation deferred to the throne room if both trails were ignored.",
                        ObjectiveType.MakeChoice, targetCode: "traitor-outcome")
                }),
            CreateQuest(11, "The Nine Gates",
                "Pass all nine gates by force, guile, ritual, or mercy. Every third gate creates a checkpoint.",
                "Darkstorm Keep", 8, 9, new[] { 2 }, new[] { 5101, 5102, 5103, 5104, 5105, 5106, 5107, 5108, 5109 },
                180, "40 EXP per gate; 180 EXP completion bonus; Fellowship Vault access.",
                "Award each gate's objective reward once. The quest completion reward is only 180 EXP, for a combined maximum of 540 EXP before combat rewards.",
                new[]
                {
                    Objective(1101, "Pass Gate 1: The Gate of Rust by an available method.",
                        ObjectiveType.CompleteScene, targetId: 5101, experience: 40),
                    Objective(1102, "Pass Gate 2: The Gate of Names by an available method.",
                        ObjectiveType.CompleteScene, targetId: 5102, experience: 40),
                    Objective(1103, "Pass Gate 3: The Gate of Weight by an available method. Reach the checkpoint.",
                        ObjectiveType.CompleteScene, targetId: 5103, experience: 40),
                    Objective(1104, "Pass Gate 4: The Gate of Reach by an available method.",
                        ObjectiveType.CompleteScene, targetId: 5104, experience: 40),
                    Objective(1105, "Pass Gate 5: The Gate of Sorrow by an available method.",
                        ObjectiveType.CompleteScene, targetId: 5105, experience: 40),
                    Objective(1106, "Pass Gate 6: The Gate of Teeth by an available method. Reach the checkpoint.",
                        ObjectiveType.CompleteScene, targetId: 5106, experience: 40),
                    Objective(1107, "Pass Gate 7: The Gate of Fire by an available method.",
                        ObjectiveType.CompleteScene, targetId: 5107, experience: 40),
                    Objective(1108, "Pass Gate 8: The Gate of Stone by an available method.",
                        ObjectiveType.CompleteScene, targetId: 5108, experience: 40),
                    Objective(1109, "Pass Gate 9: The Fellowship Vault by an available method. Reach the checkpoint.",
                        ObjectiveType.CompleteScene, targetId: 5109, experience: 40)
                }),
            CreateQuest(12, "The Fifth Pedestal",
                "Open the Fellowship Vault, read the original contract, and decide what to do with the traitor.",
                "The traitor", 9, 9, new[] { 2 }, new[] { 5012 },
                250, "Ending routes unlocked.",
                "Handing the Crown to the traitor unlocks The Crown Given. NextQuestId describes the standard continuation, not an obligation to override an ending.",
                new[]
                {
                    Objective(1201, "Read the original contract and the Guild death records in the Fellowship Vault.",
                        ObjectiveType.CompleteScene, targetCode: "original-contract"),
                    Objective(1202, "Condemn the traitor, ask for help, conceal another plan, or hand over the Crown.",
                        ObjectiveType.MakeChoice, targetCode: "traitor-judgment")
                }),
            CreateQuest(13, "The Crown's Real Offer",
                "Enter the throne room, hear Vharruk's offer, and escape Darkstorm Keep with the joined Crown.",
                "Vharruk Ashmourn", 9, 9, new[] { 2 }, new[] { 5013 },
                300, "Return route; final decision recorded.",
                "",
                new[]
                {
                    Objective(1301, "Hear Vharruk's offer in the throne room.",
                        ObjectiveType.Talk, targetCode: "vharruk-ashmourn"),
                    Objective(1302, "Reject the seat, accept it, conceal your plan, or invoke a class-specific solution.",
                        ObjectiveType.MakeChoice, targetCode: "crown-offer"),
                    Objective(1303, "Leave Darkstorm Keep with the joined Crown.",
                        ObjectiveType.Travel, targetCode: "darkstorm-keep-exit")
                }),
            CreateQuest(14, "Return to Hero's Overlook",
                "Escort the Crown to the stone table while allies hold the closing roads.",
                "Chosen companion", 9, 10, new[] { 3 }, new[] { 6014 },
                300, "All recruited factions added to War Score.",
                "",
                new[]
                {
                    Objective(1401, "Resolve three escalating encounters selected from unresolved regional enemies.",
                        ObjectiveType.CompleteScene, targetCode: "return-road-encounters", requiredAmount: 3),
                    Objective(1402, "Escort the Crown to the stone table on Hero's Overlook.",
                        ObjectiveType.Travel, targetId: 3)
                }),
            CreateQuest(15, "The Crown of Ash",
                "Defeat the Herald, resolve the Contract phase, confront the Ash form, and choose one of the seven endings.",
                "Final quest", 10, 10, new[] { 3 }, new[] { 6015, 6016, 6017 },
                0, "Campaign ending; epilogue based on choices, allies, Ash Clock, and corruption.",
                "No EXP is awarded for the finale. The Ash Falls ending can start wherever the player stands when Ash Clock reaches 10, even without all fragments; ending logic can bypass the standard quest chain and phases.",
                new[]
                {
                    Objective(1501, "Defeat the Herald, protect allies, and prevent three summoning circles from completing.",
                        ObjectiveType.CompleteScene, targetId: 6015),
                    Objective(1502, "Resolve the Contract phase through the selected final decision.",
                        ObjectiveType.CompleteScene, targetId: 6016),
                    Objective(1503, "Create three safe zones, expose Vharruk's core, and resolve the Ash phase.",
                        ObjectiveType.CompleteScene, targetId: 6017),
                    Objective(1504, "Resolve the campaign ending from the player's choices, allies, Ash Clock, and corruption.",
                        ObjectiveType.MakeChoice, targetCode: "campaign-ending")
                })
        });

        foreach (var quest in store.Quests.Where(quest => quest.QuestType == QuestType.Main))
        {
            IEnumerable<int> locationIds = quest.LocationId is int locationId
                ? new[] { locationId }
                : quest.PossibleLocationIds;
            foreach (var id in locationIds)
            {
                var location = store.Locations.Single(location => location.Id == id);
                if (!location.AvailableMainQuestIds.Contains(quest.Id))
                    location.AvailableMainQuestIds.Add(quest.Id);
            }
        }
    }

    private static Quest CreateQuest(
        int id, string title, string description, string giver,
        int minimumLevel, int maximumLevel, int[] locationIds, int[] sceneIds,
        int experience, string rewardDescription, string notes, QuestObjective[] objectives) => new()
    {
        Id = id,
        Code = $"MQ-{id:00}",
        Title = title,
        Description = description,
        QuestType = QuestType.Main,
        QuestGiver = giver,
        LocationId = locationIds.Length == 1 ? locationIds[0] : null,
        PossibleLocationIds = locationIds.Length == 1 ? new List<int>() : locationIds.ToList(),
        RecommendedLevel = minimumLevel,
        RecommendedMaximumLevel = maximumLevel,
        AssociatedSceneIds = sceneIds.ToList(),
        Prerequisites = id == 1
            ? new List<QuestPrerequisite>()
            : new List<QuestPrerequisite> { new() { RequiredQuestId = id - 1, RequiredQuestStatus = QuestStatus.Completed } },
        Objectives = objectives.ToList(),
        Rewards = new List<QuestReward> { new() { Experience = experience } },
        RewardDescription = rewardDescription,
        ScenarioNotes = notes,
        NextQuestId = id == 15 ? null : id + 1
    };

    private static QuestObjective Objective(
        int id, string description, ObjectiveType type, int? targetId = null,
        string? targetCode = null, int requiredAmount = 1, bool isOptional = false,
        int experience = 0) => new()
    {
        Id = id,
        Description = description,
        ObjectiveType = type,
        TargetId = targetId,
        TargetCode = targetCode,
        RequiredAmount = requiredAmount,
        IsOptional = isOptional,
        Rewards = experience == 0
            ? new List<QuestReward>()
            : new List<QuestReward> { new() { Experience = experience } }
    };

    private static void SeedSceneReferences(InMemoryGameDataStore store)
    {
        // Reference catalog only: no dialogue/choice graph is invented here.
        // Chapter 0 denotes an unnumbered source section.
        // Chapter 11 has three variants because the last fragment's location is not fixed.
        AddScene(store, 1001, 1, 1, "Ash at Hero's Overlook", 3);
        AddScene(store, 1201, 1, 2, "The Wood That Remembers", 7);
        AddScene(store, 1202, 1, 2, "Ash Beneath Ashtonia", 1);
        AddScene(store, 1203, 1, 2, "The Lighthouse Guild", 4);
        AddScene(store, 1204, 1, 2, "The Silence of Karag-Dur", 6);
        AddScene(store, 2003, 2, 3, "The Lighthouse with No Sea-Light", 4);
        AddScene(store, 2004, 2, 4, "Companions in the Lantern Hall", 4);
        AddScene(store, 2005, 2, 5, "The Missing Reports", 4);
        AddScene(store, 3005, 3, 0, "The Silent Grain Road", 5);
        AddScene(store, 3006, 3, 6, "A Town Being Consumed", 5);
        AddScene(store, 3007, 3, 7, "The Voice Behind the Face", 5);
        AddScene(store, 4008, 4, 8, "Ashtonia's Sealed Chamber", 1);
        AddScene(store, 4009, 4, 9, "The Heart-Tree's Fifth Shadow", 7);
        AddScene(store, 4010, 4, 10, "Karag-Dur, Where Nothing Lives", 6);
        AddScene(store, 4111, 4, 11, "The Traitor's Errand", 1);
        AddScene(store, 4112, 4, 11, "The Traitor's Errand", 7);
        AddScene(store, 4113, 4, 11, "The Traitor's Errand", 6);
        AddScene(store, 5101, 5, 0, "The Gate of Rust", 2);
        AddScene(store, 5102, 5, 0, "The Gate of Names", 2);
        AddScene(store, 5103, 5, 0, "The Gate of Weight", 2);
        AddScene(store, 5104, 5, 0, "The Gate of Reach", 2);
        AddScene(store, 5105, 5, 0, "The Gate of Sorrow", 2);
        AddScene(store, 5106, 5, 0, "The Gate of Teeth", 2);
        AddScene(store, 5107, 5, 0, "The Gate of Fire", 2);
        AddScene(store, 5108, 5, 0, "The Gate of Stone", 2);
        AddScene(store, 5109, 5, 0, "The Fellowship Vault", 2);
        AddScene(store, 5012, 5, 12, "The Fellowship Vault", 2);
        AddScene(store, 5013, 5, 13, "The Throne and the Offer", 2);
        AddScene(store, 6014, 6, 14, "The Road Closing Behind You", 3);
        AddScene(store, 6015, 6, 15, "Phase One: The Herald", 3);
        AddScene(store, 6016, 6, 16, "Phase Two: The Contract", 3);
        AddScene(store, 6017, 6, 17, "Phase Three: The Ash", 3);
    }

    private static void AddScene(
        InMemoryGameDataStore store, int id, int act, int chapter, string title, int locationId)
    {
        var location = store.Locations.Single(location => location.Id == locationId);
        store.StoryScenes.Add(new StoryScene
        {
            Id = id,
            Act = act,
            Chapter = chapter,
            Title = title,
            LocationId = locationId,
            BackgroundImage = location.BackgroundImage
        });
    }
}
