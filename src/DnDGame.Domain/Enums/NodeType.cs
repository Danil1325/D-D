namespace DnDGame.Domain.Enums;

/// <summary>
/// What kind of beat a StoryNode represents in an adventure's branching story graph.
/// </summary>
public enum NodeType
{
    /// <summary>A narrative beat presenting one or more Choices to the player.</summary>
    Narrative,

    /// <summary>A combat beat — the node's EnemyId identifies who the player fights.</summary>
    Combat,

    /// <summary>A terminal node — reaching it ends the GameSession.</summary>
    Ending
}
