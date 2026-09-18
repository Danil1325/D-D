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
    /// <summary>Fixed primary location, or null when the region depends on the player's route.</summary>
    public int? LocationId { get; set; }

    /// <summary>Alternative regions for a quest without a fixed primary location.</summary>
    public ICollection<int> PossibleLocationIds { get; set; } = new List<int>();

    /// <summary>Source scenes associated with this quest; alternatives need not all be completed.</summary>
    public ICollection<int> AssociatedSceneIds { get; set; } = new List<int>();

    /// <summary>Upper end of a recommended level range, when specified.</summary>
    public int? RecommendedMaximumLevel { get; set; }

    /// <summary>Conditional or narrative rewards that cannot be granted as fixed numeric values.</summary>
    public string RewardDescription { get; set; } = string.Empty;

    /// <summary>Source-specific route and completion rules for the future quest evaluator.</summary>
    public string ScenarioNotes { get; set; } = string.Empty;
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
