using DnDGame.Domain.Engine.Enums;
using DnDGame.Domain.Engine.Models;
using DnDGame.Domain.Entities.Cards;

namespace DnDGame.Domain.Engine.Battle;

/// <summary>
/// Mutable snapshot of all state that belongs to one battle.
/// </summary>
public class BattleState
{
    public Guid BattleId { get; set; } = Guid.Empty;

    public int PlayerHealth { get; set; }
    public int PlayerMaxHealth { get; set; }
    public int PlayerEnergy { get; set; }
    public int PlayerMaxEnergy { get; set; }
    public int PlayerBlock { get; set; }

    public int EnemyHealth { get; set; }
    public int EnemyMaxHealth { get; set; }
    public int EnemyBlock { get; set; }

    public int TurnNumber { get; set; } = 1;
    public TurnType CurrentTurn { get; set; } = TurnType.Player;
    public BattleStatus BattleStatus { get; set; } = BattleStatus.PlayerTurn;

    public IList<CardInstance> Hand { get; set; } = new List<CardInstance>();
    public IList<CardInstance> DrawPile { get; set; } = new List<CardInstance>();
    public IList<CardInstance> DiscardPile { get; set; } = new List<CardInstance>();
    public IList<ActiveEffect> ActiveEffects { get; set; } = new List<ActiveEffect>();
    public List<BattleLogEntry> BattleLog { get; set; } = new();

    public bool RewardsGranted { get; set; } = false;
}
