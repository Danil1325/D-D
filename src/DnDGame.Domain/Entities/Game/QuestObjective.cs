using DnDGame.Domain.Common;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

public class QuestObjective : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public ObjectiveType ObjectiveType { get; set; }

    /// <summary>Number of matching actions required; one for a single interaction.</summary>
    public int RequiredAmount { get; set; } = 1;

    /// <summary>
    /// Identifier of the NPC, location, enemy, item, StoryChoice or StoryScene,
    /// as determined by ObjectiveType.
    /// </summary>
    public int? TargetId { get; set; }

    /// <summary>
    /// Narrative event, item or NPC key when no numeric entity exists.
    /// Use either TargetId or TargetCode, not both.
    /// </summary>
    public string? TargetCode { get; set; }

    /// <summary>Rewards granted once when this objective completes, separate from quest completion rewards.</summary>
    public ICollection<QuestReward> Rewards { get; set; } = new List<QuestReward>();

    public bool IsOptional { get; set; }
}
