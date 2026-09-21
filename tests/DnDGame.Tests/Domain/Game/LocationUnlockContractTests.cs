using DnDGame.BusinessLayer.Dtos.Scenarios;
using DnDGame.BusinessLayer.Models;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.Tests.Domain.Game;

public class LocationUnlockContractTests
{
    [Fact]
    public void ExplicitUnlockContract_DefaultsToEmptyCollections()
    {
        Assert.Empty(new ChoiceConsequence().NewLocationIds);
        Assert.Empty(new QuestReward().NewLocationIds);
        Assert.Empty(new ScenarioProgressDto().NewLocationIds);

        var objectiveResult = new UpdateObjectiveResult(
            QuestId: 1,
            ObjectiveId: 2,
            CurrentProgress: 0,
            RequiredAmount: 1,
            ObjectiveCompleted: false,
            NextObjectiveIndex: 0,
            Completed: null);

        Assert.Empty(objectiveResult.NewLocationIds);
    }

    [Fact]
    public void ScenarioProgressDto_FromDomain_KeepsNewLocationIdsEmptyByDefault()
    {
        var dto = ScenarioProgressDto.FromDomain(new ScenarioProgress
        {
            GameSessionId = 1,
            CurrentSceneId = 900
        });

        Assert.NotNull(dto.NewLocationIds);
        Assert.Empty(dto.NewLocationIds);
    }
}
