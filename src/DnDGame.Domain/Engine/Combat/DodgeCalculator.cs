using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Delegates deterministic dodge evaluation to a configured combat rule.
/// </summary>
public sealed class DodgeCalculator : IDodgeCalculator
{
    private readonly IDodgeRule? _dodgeRule;

    public DodgeCalculator(IDodgeRule? dodgeRule = null)
    {
        _dodgeRule = dodgeRule;
    }

    public EngineResult<DodgeResult> Calculate(DodgeRequest request)
    {
        if (_dodgeRule is null)
        {
            return EngineResult<DodgeResult>.Fail(
                "No dodge rule has been configured.",
                EngineErrorCodes.MissingCombatRule);
        }

        var outcomeResult = _dodgeRule.Resolve(request);
        return outcomeResult.Success
            ? EngineResult<DodgeResult>.Ok(new DodgeResult(outcomeResult.Data))
            : EngineResult<DodgeResult>.Fail(
                outcomeResult.Message,
                outcomeResult.ErrorCode ?? EngineErrorCodes.MissingCombatRule);
    }
}
