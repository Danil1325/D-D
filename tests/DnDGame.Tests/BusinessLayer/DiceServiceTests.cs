using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Dice;
using DnDGame.BusinessLayer.Services;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Tests.BusinessLayer;

public class DiceServiceTests
{
    [Fact]
    public void Roll_ValidRequest_ReturnsDtoMirroringEngineResult()
    {
        var service = new DiceService(new DiceEngine(new FixedRandomNumberSource(10)));

        var result = service.Roll(new DiceRequestDto { DiceType = DiceType.D20, Modifier = 3 });

        Assert.Equal(DiceType.D20, result.DiceType);
        Assert.Equal(10, result.BaseRoll);
        Assert.Equal(3, result.Modifier);
        Assert.Equal(13, result.FinalResult);
    }

    [Fact]
    public void Roll_InvalidDiceType_ThrowsDomainExceptionWithEngineErrorCode()
    {
        var service = new DiceService(new DiceEngine(new FixedRandomNumberSource(1)));

        var exception = Assert.Throws<DomainException>(
            () => service.Roll(new DiceRequestDto { DiceType = (DiceType)999, Modifier = 0 }));

        Assert.Equal(EngineErrorCodes.InvalidDice, exception.ErrorCode);
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
