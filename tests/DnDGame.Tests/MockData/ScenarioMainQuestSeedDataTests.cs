using DnDGame.Domain.Enums;
using DnDGame.MockData;

namespace DnDGame.Tests.MockData;

public class ScenarioMainQuestSeedDataTests
{
    [Fact]
    public void MainQuestChain_HasFifteenLinkedQuestsWithoutHardLevelGates()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var quests = store.Quests.OrderBy(quest => quest.Id).ToList();

        Assert.Equal(15, quests.Count);
        Assert.Equal(15, quests.Select(quest => quest.Code).Distinct().Count());
        foreach (var quest in quests)
        {
            Assert.Equal(QuestType.Main, quest.QuestType);
            Assert.Equal($"MQ-{quest.Id:00}", quest.Code);
            Assert.NotEmpty(quest.Title);
            Assert.NotEmpty(quest.Description);
            Assert.NotEmpty(quest.QuestGiver);
            Assert.NotEmpty(quest.Objectives);
            Assert.NotEmpty(quest.RewardDescription);
            Assert.InRange(quest.RecommendedLevel, 1, 10);
            Assert.InRange(quest.RecommendedMaximumLevel!.Value, quest.RecommendedLevel, 10);
            if (quest.Id == 1)
                Assert.Empty(quest.Prerequisites);
            else
            {
                var prerequisite = Assert.Single(quest.Prerequisites);
                Assert.Equal(quest.Id - 1, prerequisite.RequiredQuestId);
                Assert.Equal(QuestStatus.Completed, prerequisite.RequiredQuestStatus);
                Assert.Null(prerequisite.MinimumLevel);
            }
            Assert.Equal(quest.Id == 15 ? (int?)null : quest.Id + 1, quest.NextQuestId);
        }
    }

    [Fact]
    public void SceneAndLocationReferences_ResolveAndPreserveFrontendImageKeys()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        Assert.Equal(store.StoryScenes.Count, store.StoryScenes.Select(scene => scene.Id).Distinct().Count());

        foreach (var quest in store.Quests)
        {
            var locations = quest.LocationId is int id ? new[] { id } : quest.PossibleLocationIds.ToArray();
            Assert.NotEmpty(locations);
            Assert.NotEmpty(quest.AssociatedSceneIds);
            foreach (var locationId in locations)
            {
                var location = Assert.Single(store.Locations, location => location.Id == locationId);
                Assert.Contains(quest.Id, location.AvailableMainQuestIds);
            }

            foreach (var sceneId in quest.AssociatedSceneIds)
            {
                var scene = Assert.Single(store.StoryScenes, scene => scene.Id == sceneId);
                var location = Assert.Single(store.Locations, location => location.Id == scene.LocationId);
                Assert.Equal(location.BackgroundImage, scene.BackgroundImage);
                Assert.Contains(scene.LocationId, locations);
            }
        }

        foreach (var location in store.Locations)
        {
            Assert.Equal(location.AvailableMainQuestIds.Count, location.AvailableMainQuestIds.Distinct().Count());
            Assert.All(location.AvailableMainQuestIds,
                id => Assert.Contains(store.Quests, quest => quest.Id == id && quest.QuestType == QuestType.Main));
        }
    }

    [Fact]
    public void ObjectiveTargets_AreUniqueAndUseRealIdsOrExplicitNarrativeKeys()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var objectives = store.Quests.SelectMany(quest => quest.Objectives).ToList();
        Assert.Equal(objectives.Count, objectives.Select(objective => objective.Id).Distinct().Count());
        foreach (var objective in objectives)
        {
            Assert.True(objective.RequiredAmount > 0);
            Assert.NotEmpty(objective.Description);
            Assert.True(objective.TargetId.HasValue ^ !string.IsNullOrWhiteSpace(objective.TargetCode));
            if (objective.TargetId is int id)
            {
                if (objective.ObjectiveType == ObjectiveType.Travel)
                    Assert.Contains(store.Locations, location => location.Id == id);
                else
                {
                    Assert.Equal(ObjectiveType.CompleteScene, objective.ObjectiveType);
                    Assert.Contains(store.StoryScenes, scene => scene.Id == id);
                }
            }
        }
    }

    [Fact]
    public void FragmentQuests_AllowEveryRegionalOrderAndCountDistinctFragmentsCumulatively()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var regionIds = store.Locations
            .Where(location => location.Slug is "ashtonia" or "whispering-woods" or "the-bone-peaks")
            .Select(location => location.Id).OrderBy(id => id).ToArray();

        foreach (var quest in store.Quests.Where(quest => quest.Id is >= 7 and <= 9))
        {
            Assert.Null(quest.LocationId);
            Assert.Equal(regionIds, quest.PossibleLocationIds.OrderBy(id => id));
            var collection = Assert.Single(quest.Objectives, objective => objective.TargetCode == "crown-fragments");
            Assert.Equal(quest.Id - 6, collection.RequiredAmount);
            Assert.All(quest.Rewards, reward => Assert.Empty(reward.StoryFlags));
        }
    }

    [Fact]
    public void NineGates_HaveSeparateOncePerObjectiveRewardsAndCompletionBonus()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        var quest = Assert.Single(store.Quests, quest => quest.Code == "MQ-11");

        Assert.Equal(9, quest.Objectives.Count);
        Assert.All(quest.Objectives, objective =>
        {
            Assert.False(objective.IsOptional);
            Assert.Equal(1, objective.RequiredAmount);
            Assert.Equal(40, Assert.Single(objective.Rewards).Experience);
        });
        Assert.Equal(180, Assert.Single(quest.Rewards).Experience);
        Assert.Equal(540, quest.Rewards.Sum(reward => reward.Experience) +
            quest.Objectives.SelectMany(objective => objective.Rewards).Sum(reward => reward.Experience));
    }

    [Theory]
    [InlineData("MQ-01", 100)]
    [InlineData("MQ-02", 120)]
    [InlineData("MQ-03", 150)]
    [InlineData("MQ-04", 140)]
    [InlineData("MQ-05", 180)]
    [InlineData("MQ-06", 250)]
    [InlineData("MQ-07", 220)]
    [InlineData("MQ-08", 260)]
    [InlineData("MQ-09", 320)]
    [InlineData("MQ-10", 220)]
    [InlineData("MQ-12", 250)]
    [InlineData("MQ-13", 300)]
    [InlineData("MQ-14", 300)]
    [InlineData("MQ-15", 0)]
    public void QuestExperience_MatchesSourceWithoutExtraObjectiveGrants(string code, int experience)
    {
        var quest = Assert.Single(MockDataBootstrapper.CreateSeededStore().Quests, quest => quest.Code == code);
        Assert.Equal(experience, quest.Rewards.Sum(reward => reward.Experience));
        Assert.Empty(quest.Objectives.SelectMany(objective => objective.Rewards));
    }

    [Fact]
    public void SourceExceptions_ArePreservedWithoutForcingChoiceDependentRewards()
    {
        var store = MockDataBootstrapper.CreateSeededStore();
        Assert.Contains("throne room", store.Quests.Single(quest => quest.Code == "MQ-10").ScenarioNotes);
        Assert.Contains("The Crown Given", store.Quests.Single(quest => quest.Code == "MQ-12").ScenarioNotes);
        var final = store.Quests.Single(quest => quest.Code == "MQ-15");
        Assert.Contains("Ash Clock", final.ScenarioNotes);
        Assert.Null(final.NextQuestId);
        Assert.All(store.Quests.SelectMany(quest => quest.Rewards), reward =>
        {
            Assert.Equal(0, reward.WarScore);
            Assert.Empty(reward.CompanionLoyalty);
        });
    }

    [Fact]
    public void FreshStores_DoNotShareMutableQuestData()
    {
        var first = MockDataBootstrapper.CreateSeededStore();
        var second = MockDataBootstrapper.CreateSeededStore();
        first.Quests[0].Objectives.Clear();
        first.Locations[0].AvailableMainQuestIds.Clear();

        Assert.NotEmpty(second.Quests[0].Objectives);
        Assert.NotEmpty(second.Locations[0].AvailableMainQuestIds);
    }
}
