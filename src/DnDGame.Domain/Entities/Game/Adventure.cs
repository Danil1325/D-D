using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// A playable adventure — a branching graph of StoryNodes reachable by following
/// Choices from one another.
/// </summary>
public class Adventure : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecommendedLevel { get; set; }

    /// <summary>
    /// Which StoryNode a new GameSession begins at. Design decision (Phase 1): the
    /// approved plan described the story graph itself but not how a session finds its
    /// entry point — this field is that entry point. Nullable only because it can't be
    /// set until at least one StoryNode exists for this adventure.
    /// </summary>
    public int? StartingNodeId { get; set; }
    public StoryNode? StartingNode { get; set; }

    public ICollection<StoryNode> Nodes { get; set; } = new List<StoryNode>();
}
