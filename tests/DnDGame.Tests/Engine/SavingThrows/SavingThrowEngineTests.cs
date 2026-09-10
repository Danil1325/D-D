using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;
using DnDGame.Domain.Engine.SavingThrows;
using Xunit;

namespace DnDGame.Tests.Engine.SavingThrows;

public class SavingThrowEngineTests
{
    private readonly ISavingThrowEngine _engine = new SavingThrowEngine();

    [Fact]
    public void SavingThrowSuccessWorks()
    {
        var result = _engine.Resolve(CreateRequest(baseRoll: 12, modifier: 3, difficultyClass: 14));

        Assert.True(result.Success);
        Assert.True(result.Data!.Success);
        Assert.False(result.Data.Failure);
        Assert.Equal(15, result.Data.FinalResult);
    }

    [Fact]
    public void SavingThrowFailureWorks()
    {
        var result = _engine.Resolve(CreateRequest(baseRoll: 10, modifier: 2, difficultyClass: 14));

        Assert.True(result.Success);
        Assert.True(result.Data!.Failure);
        Assert.False(result.Data.Success);
    }

    [Fact]
    public void ModifierIsApplied()
    {
        var result = _engine.Resolve(CreateRequest(baseRoll: 12, modifier: 3, difficultyClass: 15));

        Assert.Equal(15, result.Data!.FinalResult);
        Assert.True(result.Data.Success);
    }

    [Fact]
    public void ConsequenceCannotBeAppliedTwice()
    {
        var savingThrow = _engine.Resolve(CreateRequest(12, 3, 14)).Data!;

        var firstApplication = _engine.ApplyConsequence(savingThrow);
        var secondApplication = _engine.ApplyConsequence(savingThrow);

        Assert.True(firstApplication.Success);
        Assert.True(savingThrow.ConsequenceApplied);
        Assert.False(secondApplication.Success);
        Assert.Equal(EngineErrorCodes.ConsequenceAlreadyApplied, secondApplication.ErrorCode);
    }

    private static SavingThrowRequest CreateRequest(int baseRoll, int modifier, int difficultyClass)
    {
        return new SavingThrowRequest(
            new DiceResult(DiceType.D20, baseRoll, 0),
            difficultyClass,
            modifier);
    }
}
