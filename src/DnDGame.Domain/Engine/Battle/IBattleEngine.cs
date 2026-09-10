using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;

namespace DnDGame.Domain.Engine.Battle;

/// <summary>
/// Orchestrates battle operations by delegating specialised work to game-engine
/// components.
/// </summary>
public interface IBattleEngine
{
    EngineResult<BattleState> StartBattle(BattleContext battleContext);

    EngineResult<BattleState> PlayCard(BattleContext battleContext, CardInstance? card);

    EngineResult<BattleState> EndTurn(BattleContext battleContext);

    EngineResult<BattleStatus> CheckBattleStatus(BattleContext battleContext);

    bool CheckVictory(BattleState battleState);

    bool CheckDefeat(BattleState battleState);
}
