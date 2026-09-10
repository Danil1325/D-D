using DnDGame.Domain.Engine.Dice;
using Xunit;

namespace DnDGame.Tests.Engine.Dice;

public class DiceEngineTests
{
    [Fact]
    public void DiceD20Returns1To20()
    {
        var engine = new DiceEngine(new FixedRandomNumberSource(20));

        var result = engine.Roll(DiceType.D20);

        Assert.True(result.Success);
        Assert.InRange(result.Data!.BaseRoll, 1, 20);
    }

    [Fact]
    public void DiceD4Returns1To4()
    {
        var engine = new DiceEngine(new FixedRandomNumberSource(1));

        var result = engine.Roll(DiceType.D4);

        Assert.True(result.Success);
        Assert.InRange(result.Data!.BaseRoll, 1, 4);
    }

    [Fact]
    public void DiceD6Returns1To6()
    {
        var engine = new DiceEngine(new FixedRandomNumberSource(6));

        var result = engine.Roll(DiceType.D6);

        Assert.True(result.Success);
        Assert.InRange(result.Data!.BaseRoll, 1, 6);
    }

    [Fact]
    public void ModifierIsApplied()
    {
        var engine = new DiceEngine(new FixedRandomNumberSource(10));

        var result = engine.RollWithModifier(DiceType.D20, -3);

        Assert.True(result.Success);
        Assert.Equal(10, result.Data!.BaseRoll);
        Assert.Equal(-3, result.Data.Modifier);
        Assert.Equal(7, result.Data.FinalResult);
    }

    [Fact]
    public void FinalResultIsAlwaysDerivedFromBaseRollAndModifier()
    {
        var result = new DiceResult(DiceType.D20, 14, 3);

        Assert.Equal(17, result.FinalResult);
    }

    private sealed class FixedRandomNumberSource : IRandomNumberSource
    {
        private readonly int _value;

        public FixedRandomNumberSource(int value)
        {
            _value = value;
        }

        public int Next(int minimumInclusive, int maximumExclusive)
        {
            return _value;
        }
    }
}
