using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.SavingThrows;

/// <summary>
/// Resolves saving throws from backend-generated dice data and guards one-time
/// consequence application.
/// </summary>
public interface ISavingThrowEngine
{
    EngineResult<SavingThrowResult> Resolve(SavingThrowRequest request);

    EngineResult<SavingThrowResult> ApplyConsequence(SavingThrowResult result);
}
