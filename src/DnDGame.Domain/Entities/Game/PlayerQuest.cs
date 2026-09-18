using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>Progress for one quest in a GameSession, which identifies the character and adventure.</summary>
public class PlayerQuest : BaseEntity
{
    public int GameSessionId { get; set; }
    public int QuestId { get; set; }
    public QuestStatus Status { get; set; } = QuestStatus.Locked;

    /// <summary>
    /// Zero-based index in Quest.Objectives. A value equal to the objective count
    /// means there is no remaining current objective.
    /// </summary>
    public int CurrentObjectiveIndex { get; set; }

    /// <summary>Current amounts keyed by QuestObjective.Id. Missing entries mean zero progress.</summary>
    public Dictionary<int, int> ObjectiveProgress { get; set; } = new();

    /// <summary>UTC start time; null until the quest is started.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>UTC completion time; null until the quest is completed.</summary>
    public DateTime? CompletedAt { get; set; }
}
