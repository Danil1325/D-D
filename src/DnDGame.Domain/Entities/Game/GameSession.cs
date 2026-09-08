using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// One character's run through one adventure.
/// </summary>
public class GameSession : BaseEntity
{
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
