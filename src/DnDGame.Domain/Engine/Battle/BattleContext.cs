using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Enemies;

namespace DnDGame.Domain.Engine.Battle;

/// <summary>
/// Groups the source combatants with the mutable state for a single battle.
/// </summary>
public class BattleContext
{
    public required PlayerCharacter Player { get; init; }

    public required Enemy Enemy { get; init; }

    public required BattleState State { get; init; }
}
