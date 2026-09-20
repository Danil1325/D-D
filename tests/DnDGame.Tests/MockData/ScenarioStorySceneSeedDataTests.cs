using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;
using DnDGame.MockData;

namespace DnDGame.Tests.MockData;

public class ScenarioStorySceneSeedDataTests
{
    private static InMemoryGameDataStore CreateStore() => MockDataBootstrapper.CreateSeededStore();

    [Fact]
    public void AllScenes_AreUniqueAndKeepLocationRenderingKeys()
    {
        var store = CreateStore();
        var scenes = store.StoryScenes;

        Assert.Equal(scenes.Count, scenes.Select(scene => scene.Id).Distinct().Count());
        Assert.All(scenes, scene =>
        {
            Assert.NotEmpty(scene.Title);
            Assert.InRange(scene.Act, 0, 6);
            var location = Assert.Single(store.Locations, location => location.Id == scene.LocationId);
            Assert.Equal(location.BackgroundImage, scene.BackgroundImage);
        });
    }

    [Fact]
    public void EveryChoice_RoutesToAnExistingSceneWhereTheFlowContinues()
    {
        var store = CreateStore();
        var scenes = store.StoryScenes;

        foreach (var scene in scenes)
        {
            foreach (var choice in scene.Choices)
            {
                Assert.NotNull(choice.NextSceneId);
                Assert.NotEmpty(choice.Text);
                Assert.Contains(store.StoryScenes, target => target.Id == choice.NextSceneId);
                Assert.NotEqual(scene.Id, choice.NextSceneId.Value);
            }
        }
    }

    [Fact]
    public void RacialOpeningChoices_RequireTheExpectedRaceForEachRoute()
    {
        var scene = Assert.Single(CreateStore().StoryScenes, scene => scene.Id == 1205);
        var expectedRaceByTarget = new Dictionary<int, int>
        {
            [1201] = 2,
            [1202] = 3,
            [1203] = 1,
            [1204] = 4
        };

        var routeChoices = scene.Choices
            .Where(choice => choice.NextSceneId is int target && expectedRaceByTarget.ContainsKey(target))
            .ToList();

        Assert.Equal(expectedRaceByTarget.Count, routeChoices.Count);
        foreach (var choice in routeChoices)
        {
            var requirement = Assert.Single(choice.Requirements);
            Assert.Equal(expectedRaceByTarget[choice.NextSceneId!.Value], requirement.RaceId);
        }
    }

    [Fact]
    public void EveryNonFinalScene_CanAdvanceAndEveryForkTargetsTheSameChapterOrLater()
    {
        var store = CreateStore();
        foreach (var scene in store.StoryScenes.Where(scene => !scene.IsFinalScene))
        {
            // Reference-only travel scenes (e.g. The Silent Grain Road) are legitimately bare.
            if (scene.Id == 3005)
                continue;
            Assert.NotEmpty(scene.Choices);
        }
    }

    [Fact]
    public void DialogueLines_AreWithinTheBoxAndOrderedPerScene()
    {
        var store = CreateStore();
        foreach (var scene in store.StoryScenes)
        {
            var dialogues = scene.Dialogues.OrderBy(dialogue => dialogue.DialogueOrder).ToList();
            Assert.Equal(Enumerable.Range(1, dialogues.Count), dialogues.Select(dialogue => dialogue.DialogueOrder));
            Assert.All(dialogues, dialogue =>
            {
                Assert.NotEmpty(dialogue.Text);
                Assert.True(dialogue.Text.Length <= 350, $"Dialogue {dialogue.Id} in scene {scene.Id} exceeds 350 chars.");
            });
        }
    }

    [Fact]
    public void DialogueIdsAndChoiceIds_AreGloballyUnique()
    {
        var store = CreateStore();
        var dialogueIds = store.StoryScenes.SelectMany(scene => scene.Dialogues).Select(dialogue => dialogue.Id).ToList();
        var choiceIds = store.StoryScenes.SelectMany(scene => scene.Choices).Select(choice => choice.Id).ToList();

        Assert.Equal(dialogueIds.Count, dialogueIds.Distinct().Count());
        Assert.Equal(choiceIds.Count, choiceIds.Distinct().Count());
    }

    [Fact]
    public void SpeakerLabels_MapToTheExpectedDialogueTypes()
    {
        var store = CreateStore();
        Assert.All(store.StoryScenes.SelectMany(scene => scene.Dialogues), dialogue =>
        {
            switch (dialogue.DialogueType)
            {
                case DialogueType.Player:
                    Assert.Equal("{user_nickname}", dialogue.Speaker);
                    break;
                case DialogueType.System:
                    Assert.Equal("GAME", dialogue.Speaker);
                    break;
                case DialogueType.NPC:
                    Assert.NotEmpty(dialogue.Speaker);
                    Assert.NotEqual("GAME", dialogue.Speaker);
                    Assert.NotEqual("{user_nickname}", dialogue.Speaker);
                    break;
                case DialogueType.Narration:
                    Assert.Equal(string.Empty, dialogue.Speaker);
                    break;
            }
        });
    }

    [Fact]
    public void OnlyFinalImage_IsMarkedAsTheFinalScene()
    {
        var store = CreateStore();
        var final = Assert.Single(store.StoryScenes, scene => scene.IsFinalScene);
        Assert.Equal(6108, final.Id);
        Assert.Equal("Final Image", final.Title);
    }

    [Theory]
    [InlineData(6101, 6, 18, 3, "The Crown Broken")]
    [InlineData(6102, 6, 18, 2, "The Crown Worn")]
    [InlineData(6103, 6, 18, 3, "The Crown Sung")]
    [InlineData(6104, 6, 18, 6, "The Crown Reforged")]
    [InlineData(6105, 6, 18, 3, "The Crown Given")]
    [InlineData(6106, 6, 18, 3, "The Crown Carried")]
    [InlineData(6107, 6, 18, 3, "The Ash Falls")]
    public void EpilogueScenes_KeepTheirActChapterLocationAndEndingTitle(int id, int act, int chapter, int locationId, string ending)
    {
        var scene = Assert.Single(CreateStore().StoryScenes, scene => scene.Id == id);
        Assert.Equal(act, scene.Act);
        Assert.Equal(chapter, scene.Chapter);
        Assert.Equal(locationId, scene.LocationId);
        Assert.Contains(ending, scene.Title);
    }

    [Fact]
    public void FinalDecisionChoices_AreRepeatedVerbatimForRouting()
    {
        var store = CreateStore();
        var contract = store.StoryScenes.Single(scene => scene.Id == 6016);
        var ash = store.StoryScenes.Single(scene => scene.Id == 6017);

        Assert.Equal(6, contract.Choices.Count);
        Assert.Equal(contract.Choices.Select(choice => choice.Text),
            ash.Choices.Select(choice => choice.Text));
    }

    [Fact]
    public void BranchChoices_KeepTheirBracketGuidanceVerbatim()
    {
        var store = CreateStore();
        var elf = store.StoryScenes.Single(scene => scene.Id == 1211);
        Assert.Contains(elf.Choices, choice => choice.Text ==
            "Free the fragment with a WITS ritual. [Success: second_fragment=true; Sil'Vaneth becomes an ally.]");

        var human = store.StoryScenes.Single(scene => scene.Id == 1231);
        Assert.Contains(human.Choices, choice => choice.Text ==
            "Inspect the expedition ledger. [WITS check; discover missing reports were altered.]");

        var gates = store.StoryScenes.Single(scene => scene.Id == 5101);
        Assert.Contains(gates.Choices, choice => choice.Text == "release them through a funerary ritual.");
    }

    [Fact]
    public void AllRoutedScenes_AreReachableFromTheOpening()
    {
        var store = CreateStore();
        var choiceTargets = store.StoryScenes
            .SelectMany(scene => scene.Choices)
            .Where(choice => choice.NextSceneId.HasValue)
            .Select(choice => choice.NextSceneId!.Value)
            .ToHashSet();
        var inboundTargets = store.StoryScenes.Select(scene => scene.Id).ToHashSet();

        var queue = new Queue<int>();
        queue.Enqueue(900);
        var visited = new HashSet<int>();
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!visited.Add(id))
                continue;
            var scene = store.StoryScenes.Single(scene => scene.Id == id);
            foreach (var choice in scene.Choices.Where(choice => choice.NextSceneId.HasValue))
                queue.Enqueue(choice.NextSceneId!.Value);
        }

        // Deliberately not routed by choices in the linear seed graph:
        //  - 3005 (The Silent Grain Road): the game inserts the road travel.
        //  - 6107 (Ash Falls): the Ash Clock triggers the ending.
        //  - 4112/4113 and their payoffs 4115/4116: parallel Chapter 11 variants for
        //    the Woods/Karag-Dur "last fragment" cases; the engine picks whichever
        //    region supplied the final fragment, so only 4111 is on the linear route.
        var unreachable = inboundTargets.Except(visited)
            .Except(new[] { 3005, 6107, 4112, 4113, 4115, 4116 }).ToList();
        Assert.Empty(unreachable);
        Assert.Contains(choiceTargets, target => target == 6101);
    }
}
