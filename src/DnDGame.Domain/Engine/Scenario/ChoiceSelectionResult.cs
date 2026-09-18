using DnDGame.Domain.Entities.Game;

namespace DnDGame.Domain.Engine.Scenario;

/// <summary>
/// The outcome of selecting a choice: the updated navigation state, the scene
/// the run moved into (null when the choice ended the story), and a snapshot of
/// the consequences that were applied.
/// </summary>
public sealed class ChoiceSelectionResult
{
    public ChoiceSelectionResult(
        ScenarioState state,
        StoryScene? nextScene,
        IReadOnlyList<ChoiceConsequence> appliedConsequences)
    {
        State = state;
        NextScene = nextScene;
        AppliedConsequences = appliedConsequences;
    }

    /// <summary>The same state instance that was passed in, mutated by the selection.</summary>
    public ScenarioState State { get; }

    /// <summary>
    /// The scene the run lands in after the choice, or null when the choice, or
    /// the scene it points to, ends the story.
    /// </summary>
    public StoryScene? NextScene { get; }

    /// <summary>True when the choice ends the run (final scene reached or no next scene).</summary>
    public bool StoryEnded => State.IsCompleted;

    /// <summary>The consequences that were applied, in the order they appear on the choice.</summary>
    public IReadOnlyList<ChoiceConsequence> AppliedConsequences { get; }
}