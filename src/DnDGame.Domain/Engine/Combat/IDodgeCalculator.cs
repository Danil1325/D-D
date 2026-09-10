using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Calculates the dodge outcome from a complete, already-resolved combat input.
/// </summary>
public interface IDodgeCalculator
{
    EngineResult<DodgeResult> Calculate(DodgeRequest request);
}
