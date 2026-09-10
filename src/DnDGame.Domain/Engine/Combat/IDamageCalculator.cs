using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Calculates damage from a complete, already-resolved combat input.
/// </summary>
public interface IDamageCalculator
{
    EngineResult<DamageResult> Calculate(DamageRequest request);
}
