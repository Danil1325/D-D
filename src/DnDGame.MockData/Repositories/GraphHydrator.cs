using DnDGame.Domain.Entities.Game;

namespace DnDGame.MockData.Repositories;

/// <summary>
/// Shared logic for assembling a StoryNode (and its Choices) from the flat lists in
/// InMemoryGameDataStore. Used by both MockStoryNodeRepository and
/// MockAdventureRepository/MockGameSessionRepository so this "join" logic exists
/// in exactly one place.
///
/// Deliberately shallow: when a Choice or StoryNode points at another StoryNode
/// (NextNode/FailureNode), that target node is attached as-is, without recursively
/// hydrating ITS OWN Choices/NextNode too. Without this limit, loading one node
/// could cascade into loading the entire story graph (and could recurse forever if
/// the graph ever looped back on itself).
/// </summary>
internal static class GraphHydrator
{
    public static StoryNode HydrateNode(InMemoryGameDataStore store, StoryNode node)
    {
        node.Choices = store.Choices
            .Where(c => c.StoryNodeId == node.Id)
            .Select(c => HydrateChoice(store, c))
            .ToList();

        if (node.EnemyId.HasValue)
        {
            node.Enemy = store.Enemies.FirstOrDefault(e => e.Id == node.EnemyId.Value);
        }

        if (node.NextNodeId.HasValue)
        {
            node.NextNode = FindShallow(store, node.NextNodeId.Value);
        }

        return node;
    }

    private static Choice HydrateChoice(InMemoryGameDataStore store, Choice choice)
    {
        if (choice.EnemyId.HasValue)
        {
            choice.Enemy = store.Enemies.FirstOrDefault(e => e.Id == choice.EnemyId.Value);
        }

        if (choice.NextNodeId.HasValue)
        {
            choice.NextNode = FindShallow(store, choice.NextNodeId.Value);
        }

        if (choice.FailureNodeId.HasValue)
        {
            choice.FailureNode = FindShallow(store, choice.FailureNodeId.Value);
        }

        return choice;
    }

    private static StoryNode? FindShallow(InMemoryGameDataStore store, int id) =>
        store.StoryNodes.FirstOrDefault(n => n.Id == id);
}
