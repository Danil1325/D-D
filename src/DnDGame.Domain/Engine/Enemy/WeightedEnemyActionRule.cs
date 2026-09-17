using DnDGame.Domain.Engine.Battle;
using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Dice;

namespace DnDGame.Domain.Engine.EnemyActions;

/// <summary>
/// MVP enemy AI policy: each turn rolls a weighted coin using the same injectable
/// <see cref="IRandomNumberSource"/> the dice engine uses (so tests stay
/// deterministic), attacking most of the time and occasionally defending instead.
/// </summary>
public sealed class WeightedEnemyActionRule : IEnemyActionRule
{
    /// <summary>Chance (0-100) that the enemy attacks rather than defends.</summary>
    public const int AttackChancePercent = 75;

    private readonly IRandomNumberSource _randomNumberSource;

    public WeightedEnemyActionRule(IRandomNumberSource randomNumberSource)
    {
        ArgumentNullException.ThrowIfNull(randomNumberSource);
        _randomNumberSource = randomNumberSource;
    }

    public EngineResult<EnemyAction> SelectAction(BattleContext battleContext)
    {
        ArgumentNullException.ThrowIfNull(battleContext);

        var roll = _randomNumberSource.Next(0, 100);
        var actionType = roll < AttackChancePercent
            ? EnemyActionType.Attack
            : EnemyActionType.Defend;

        return EngineResult<EnemyAction>.Ok(new EnemyAction(actionType));
    }
}
