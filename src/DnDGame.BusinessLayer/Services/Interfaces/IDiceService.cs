using DnDGame.BusinessLayer.Dtos.Dice;

namespace DnDGame.BusinessLayer.Services.Interfaces;

/// <summary>
/// Application-service wrapper around IDiceEngine for the API boundary.
/// </summary>
public interface IDiceService
{
    /// <summary>
    /// Rolls the requested dice type with the given modifier.
    /// Throws DomainException (mapped to 400 by IErrorCodeHttpMapper) if the engine
    /// rejects the request, e.g. an out-of-range DiceType value.
    /// </summary>
    DiceResultDto Roll(DiceRequestDto request);
}
