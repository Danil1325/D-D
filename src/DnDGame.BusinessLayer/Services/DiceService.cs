using DnDGame.BusinessLayer.Common.Exceptions;
using DnDGame.BusinessLayer.Dtos.Dice;
using DnDGame.BusinessLayer.Services.Interfaces;
using DnDGame.Domain.Engine.Dice;

namespace DnDGame.BusinessLayer.Services;

public class DiceService : IDiceService
{
    private readonly IDiceEngine _diceEngine;

    public DiceService(IDiceEngine diceEngine)
    {
        ArgumentNullException.ThrowIfNull(diceEngine);
        _diceEngine = diceEngine;
    }

    public DiceResultDto Roll(DiceRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = _diceEngine.RollWithModifier(request.DiceType, request.Modifier);
        if (!result.Success)
        {
            throw new DomainException(result.ErrorCode!, result.Message);
        }

        return DiceResultDto.FromDomain(result.Data!);
    }
}
