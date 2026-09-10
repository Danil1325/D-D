using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Configurable rule that resolves a dodge outcome from a DodgeRequest.
/// </summary>
public interface IDodgeRule
{
    EngineResult<DodgeOutcome> Resolve(DodgeRequest request);
}
