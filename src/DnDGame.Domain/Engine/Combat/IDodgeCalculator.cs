using DnDGame.Domain.Engine.Battle;

namespace DnDGame.Domain.Engine.Combat;

/// <summary>
/// Contract for evaluating whether an attack is dodged.
/// </summary>
public interface IDodgeCalculator
{
    bool IsDodged(BattleContext battleContext);
}
