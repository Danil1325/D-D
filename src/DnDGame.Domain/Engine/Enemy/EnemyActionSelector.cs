using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;

namespace DnDGame.Domain.Engine.EnemyActions;

/// <summary>
/// Delegates action selection to a configurable enemy policy.
/// </summary>
public sealed class EnemyActionSelector : IEnemyActionSelector
{
    private readonly IEnemyActionRule? _actionRule;

    public EnemyActionSelector(IEnemyActionRule? actionRule = null)
    {
        _actionRule = actionRule;
    }

    public EngineResult<EnemyAction> SelectAction(BattleContext battleContext)
    {
        return _actionRule is null
            ? EngineResult<EnemyAction>.Fail(
                "No enemy action rule has been configured.",
                EngineErrorCodes.MissingCombatRule)
            : _actionRule.SelectAction(battleContext);
    }
}
