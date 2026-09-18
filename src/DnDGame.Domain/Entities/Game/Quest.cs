using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>Definition of a main or side quest, independent of a player's progress.</summary>
public class Quest : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestType QuestType { get; set; }
    public int LocationId { get; set; }
    public int RecommendedLevel { get; set; } = 1;
    public string QuestGiver { get; set; } = string.Empty;

    /// <summary>Objectives in progression order, indexed by PlayerQuest.CurrentObjectiveIndex.</summary>
    public IList<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();

    public ICollection<QuestReward> Rewards { get; set; } = new List<QuestReward>();

    /// <summary>All prerequisites must hold. An empty collection imposes no restrictions.</summary>
    public ICollection<QuestPrerequisite> Prerequisites { get; set; } = new List<QuestPrerequisite>();

    /// <summary>Next quest in the chain, or null when there is no follow-up quest.</summary>
    public int? NextQuestId { get; set; }
}
