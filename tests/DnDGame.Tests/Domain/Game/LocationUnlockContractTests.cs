using DnDGame.BusinessLayer.Dtos.Scenarios;
using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Game;
using DnDGame.MockData;
using DnDGame.MockData.Repositories;
using Xunit;

namespace DnDGame.Tests.Domain.Game;

public class LocationUnlockContractTests
{
    [Fact]
    public void ScenarioProgress_UnlockedLocationIds_StoresDistinctLocationIds()
    {
        var progress = new ScenarioProgress();

        progress.UnlockedLocationIds.Add(4);
        progress.UnlockedLocationIds.Add(4);
        progress.UnlockedLocationIds.Add(7);

        Assert.Equal(new[] { 4, 7 }, progress.UnlockedLocationIds.OrderBy(id => id));
    }

    [Fact]
    public async Task ScenarioProgressRepository_RetainsUnlockedLocationIds()
    {
        var store = new InMemoryGameDataStore();
        var repository = new MockScenarioProgressRepository(store);
        var progress = new ScenarioProgress { GameSessionId = 11 };
        progress.UnlockedLocationIds.Add(4);

        await repository.AddAsync(progress);
        var stored = await repository.GetByGameSessionAsync(11);

        Assert.NotNull(stored);
        Assert.Contains(4, stored.UnlockedLocationIds);
    }

    [Fact]
    public void ExplicitUnlockContracts_DefaultToEmptyForExistingSeedData()
    {
        var store = MockDataBootstrapper.CreateSeededStore();

        Assert.Empty(new ChoiceConsequence().NewLocationIds);
        Assert.Empty(new QuestReward().NewLocationIds);
        Assert.All(
            store.StoryScenes.SelectMany(scene => scene.Choices).SelectMany(choice => choice.Consequences),
            consequence => Assert.Empty(consequence.NewLocationIds));
        Assert.All(
            store.Quests.SelectMany(quest => quest.Rewards),
            reward => Assert.Empty(reward.NewLocationIds));
        Assert.All(
            store.Quests.SelectMany(quest => quest.Objectives).SelectMany(objective => objective.Rewards),
            reward => Assert.Empty(reward.NewLocationIds));
    }

    [Fact]
    public void ResponseContracts_DefaultNewLocationIdsToEmpty()
    {
        var progressDto = ScenarioProgressDto.FromDomain(new ScenarioProgress { GameSessionId = 1 });
        var questCompletion = new QuestCompletionResult(
            QuestId: 1,
            QuestTitle: "Quest",
            ExperienceGained: 0,
            PreviousLevel: 1,
            CurrentLevel: 1,
            SkillPointsGained: 0);

        Assert.Empty(progressDto.NewLocationIds);
        Assert.Empty(questCompletion.NewLocationIds);
    }

    [Fact]
    public void ScenarioProgressDto_FromDomain_NormalizesDuplicateNewLocationIds()
    {
        var dto = ScenarioProgressDto.FromDomain(
            new ScenarioProgress { GameSessionId = 1 },
            new[] { 4, 4, 7 });

        Assert.Equal(new[] { 4, 7 }, dto.NewLocationIds);
    }
}
