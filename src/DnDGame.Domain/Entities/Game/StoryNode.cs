using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// One node in an adventure's branching story graph. What it means depends on
/// NodeType: Narrative nodes present Choices, Combat nodes point at an Enemy,
/// Ending nodes terminate the GameSession.
/// </summary>
public class StoryNode : BaseEntity
{
    public int AdventureId { get; set; }
    public Adventure? Adventure { get; set; }

    public string Title { get; set; } = string.Empty;
    public string NarrativeText { get; set; } = string.Empty;

    public NodeType NodeType { get; set; }

    /// <summary>Only meaningful when NodeType is Combat — identifies who the player fights here.</summary>
    public int? EnemyId { get; set; }
    public Enemy? Enemy { get; set; }

    /// <summary>Only meaningful for Narrative nodes — the options presented to the player.</summary>
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
}
