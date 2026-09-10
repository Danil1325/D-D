using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Configurable damage rule responsible for raw damage and modifier stages.
/// </summary>
public interface IDamageRule
{
    EngineResult<int> CalculateRawDamage(DamageRequest request);

    EngineResult<int> ApplyModifiers(int rawDamage, DamageRequest request);
}
