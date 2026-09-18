using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// One character's run through one adventure.
/// </summary>
public class GameSession : BaseEntity
{
    /// <summary>Battle IDs whose EXP reward has already been processed in this session.</summary>
    public HashSet<int> ExperienceRewardedBattleIds { get; set; } = new();

    /// <summary>Summoner instances for which a summoned opponent has already earned EXP.</summary>
    public HashSet<Guid> ExperienceRewardedSummonerIds { get; set; } = new();

    public int CharacterId { get; set; }
    public PlayerCharacter? PlayerCharacter { get; set; }

    public int AdventureId { get; set; }
    public Adventure? Adventure { get; set; }

    public int CurrentNodeId { get; set; }
    public StoryNode? CurrentNode { get; set; }

    public GameSessionStatus Status { get; set; } = GameSessionStatus.InProgress;

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<SessionLogEntry> LogEntries { get; set; } = new List<SessionLogEntry>();
}
