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
    public int TargetId { get; set; }

    public bool IsOptional { get; set; }
}
