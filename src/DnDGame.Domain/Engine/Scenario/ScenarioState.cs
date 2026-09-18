using DnDGame.Domain.Entities.Game;

namespace DnDGame.Domain.Engine.Scenario;

/// <summary>
/// Runtime navigation state for one interactive scenario run, layered over the
/// persistent <see cref="ScenarioProgress"/> entity. The progress record owns the
/// counters (AshClock, Corruption, WarScore, loyalty, quest progress, flags) and
/// the current scene; this wrapper additionally remembers which scenes were
/// already visited and which choices were already selected.
/// </summary>
public sealed class ScenarioState
{
    public ScenarioState(ScenarioProgress progress)
    {
        ArgumentNullException.ThrowIfNull(progress);
        Progress = progress;
    }

    /// <summary>The persistent scenario outcome record that this state navigates.</summary>
    public ScenarioProgress Progress { get; }

    /// <summary>Ids of every scene the run has entered, in no particular order.</summary>
    public HashSet<int> VisitedSceneIds { get; } = new();

    /// <summary>Ids of every choice already taken in this run.</summary>
    public HashSet<int> SelectedChoiceIds { get; } = new();

    /// <summary>True once the story has run to its end.</summary>
    public bool IsCompleted => Progress.IsCompleted;
}