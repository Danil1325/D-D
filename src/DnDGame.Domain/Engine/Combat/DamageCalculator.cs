using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Applies defense and block after delegating raw-damage and modifier stages to a
/// configurable combat rule. It never changes battle health.
/// </summary>
public sealed class DamageCalculator : IDamageCalculator
{
    private readonly IDamageRule? _damageRule;

    public DamageCalculator(IDamageRule? damageRule = null)
    {
        _damageRule = damageRule;
    }

    public EngineResult<DamageResult> Calculate(DamageRequest request)
    {
        if (_damageRule is null)
        {
            return EngineResult<DamageResult>.Fail(
                "No damage rule has been configured.",
                EngineErrorCodes.MissingCombatRule);
        }

        var rawDamageResult = _damageRule.CalculateRawDamage(request);
        if (!rawDamageResult.Success)
        {
            return FailRule(rawDamageResult);
        }

        var modifiedDamageResult = _damageRule.ApplyModifiers(rawDamageResult.Data, request);
        if (!modifiedDamageResult.Success)
        {
            return FailRule(modifiedDamageResult);
        }

        var modifiedDamage = Math.Max(0, modifiedDamageResult.Data);
        var damageAfterDefense = Math.Max(0, modifiedDamage - Math.Max(0, request.Defense));
        var availableBlock = Math.Max(0, request.Block);
        var blockedDamage = Math.Min(damageAfterDefense, availableBlock);
        var finalDamage = damageAfterDefense - blockedDamage;
        var remainingBlock = availableBlock - blockedDamage;

        return EngineResult<DamageResult>.Ok(
            new DamageResult(
                rawDamageResult.Data,
                modifiedDamage,
                damageAfterDefense,
                blockedDamage,
                finalDamage,
                remainingBlock));
    }

    private static EngineResult<DamageResult> FailRule(EngineResult<int> ruleResult)
    {
        return EngineResult<DamageResult>.Fail(
            ruleResult.Message,
            ruleResult.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
    }
}
