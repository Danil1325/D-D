using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.SavingThrows;

/// <summary>
/// Deterministically resolves saving throws and exposes an idempotency guard for
/// a future effect-processing flow.
/// </summary>
public sealed class SavingThrowEngine : ISavingThrowEngine
{
    public EngineResult<SavingThrowResult> Resolve(SavingThrowRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return EngineResult<SavingThrowResult>.Ok(
            new SavingThrowResult(
                request.DiceResult,
                request.DifficultyClass,
                request.Modifier));
    }

    public EngineResult<SavingThrowResult> ApplyConsequence(SavingThrowResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.ConsequenceApplied)
        {
            return EngineResult<SavingThrowResult>.Fail(
                "The saving throw consequence has already been applied.",
                EngineErrorCodes.ConsequenceAlreadyApplied);
        }

        result.MarkConsequenceApplied();
        return EngineResult<SavingThrowResult>.Ok(result);
    }
}
