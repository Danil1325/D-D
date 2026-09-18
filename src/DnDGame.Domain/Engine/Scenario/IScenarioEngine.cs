using DnDGame.Domain.Engine.Common;
using DnDGame.Domain.Entities.Characters;
using DnDGame.Domain.Entities.Game;

namespace DnDGame.Domain.Engine.Scenario;

/// <summary>
/// Drives an interactive story scene graph (<see cref="StoryScene"/>/<see cref="StoryChoice"/>)
/// for one character. The engine is a pure state machine: it mutates the provided
/// <see cref="ScenarioProgress"/>, <see cref="PlayerCharacter"/> and
/// <see cref="PlayerQuest"/> instances and never touches persistence. All expected
/// gameplay failures are reported through <see cref="EngineResult{T}"/> instead of
/// exceptions.
///
/// Notes on boundaries:
/// - <see cref="ChoiceRequirement.Stat"/> is evaluated against the domain
///   <see cref="DnDGame.Domain.Enums.AttributeType"/> values on the character.
/// - There is no inventory entity in this codebase yet; item requirements are
///   evaluated against the caller-supplied owned-item-ids, which may be empty.
/// - Quest progress deltas only update <see cref="ScenarioProgress.QuestProgress"/>;
///   objective/outcome evaluation belongs to the future quest evaluator.
/// </summary>
public interface IScenarioEngine
{
    /// <summary>
    /// Begins a run at <paramref name="startSceneId"/>: records the current scene,
    /// marks it visited and starts any quests whose <see cref="Quest.AssociatedSceneIds"/>
    /// include it. <paramref name="progress"/> is expected to be a fresh record.
    /// </summary>
    EngineResult<ScenarioState> StartScenario(
        ScenarioProgress progress,
        int startSceneId,
        IReadOnlyCollection<StoryScene> scenes,
        PlayerCharacter character,
        IReadOnlyCollection<Quest> quests,
        ICollection<PlayerQuest> playerQuests);

    /// <summary>
    /// Restores a persisted run: rebuilds the visited/selected bookkeeping that is
    /// not stored on <see cref="ScenarioProgress"/> itself.
    /// </summary>
    EngineResult<ScenarioState> ResumeScenario(
        ScenarioProgress progress,
        IReadOnlyCollection<int> visitedSceneIds,
        IReadOnlyCollection<int> selectedChoiceIds);

    /// <summary>
    /// Returns the current scene with every <c>{user_nickname}</c> placeholder on
    /// dialogues and choices replaced by <paramref name="character"/>.Name. The
    /// returned scene is a presentation copy; the store is never mutated.
    /// </summary>
    EngineResult<StoryScene> GetCurrentScene(
        ScenarioState state,
        IReadOnlyCollection<StoryScene> scenes,
        PlayerCharacter character);

    /// <summary>
    /// Returns the choices of <paramref name="scene"/> that are still selectable in
    /// this run: not already chosen and meeting every requirement. Texts have
    /// <c>{user_nickname}</c> replaced. Requirement failures silently exclude a
    /// choice; inspect <see cref="CheckChoiceRequirements"/> for the specific reason.
    /// </summary>
    EngineResult<IReadOnlyList<StoryChoice>> GetAvailableChoices(
        ScenarioState state,
        StoryScene scene,
        PlayerCharacter character,
        IReadOnlyCollection<int>? ownedItemIds = null);

    /// <summary>
    /// Validates every populated requirement of <paramref name="choice"/> without
    /// applying anything. Missing quest items and missing flags fail the check.
    /// </summary>
    EngineResult<bool> CheckChoiceRequirements(
        StoryChoice choice,
        PlayerCharacter character,
        ScenarioState state,
        IReadOnlyCollection<int>? ownedItemIds = null);

    /// <summary>
    /// Applies the consequences of <paramref name="choice"/>: EXP, AshClock,
    /// Corruption, WarScore, companion loyalty, quest progress and story flags.
    /// Counter totals are clamped so they never go below zero. This step does not
    /// advance the scene. Use <see cref="SelectChoice"/> for the combined flow.
    /// </summary>
    EngineResult<ChoiceConsequence> ApplyChoiceConsequences(
        ScenarioState state,
        PlayerCharacter character,
        StoryChoice choice);

    /// <summary>
    /// Requires a choice in <paramref name="scene"/> to be selectable (not already
    /// taken, all requirements met), then applies its consequences, records it as
    /// selected and advances along <see cref="StoryChoice.NextSceneId"/>, starting
    /// the quests associated with the destination scene. Choosing into the final
    /// scene, or a choice with no next scene, completes the run.
    /// </summary>
    EngineResult<ChoiceSelectionResult> SelectChoice(
        ScenarioState state,
        StoryScene scene,
        PlayerCharacter character,
        int choiceId,
        IReadOnlyCollection<StoryScene> scenes,
        IReadOnlyCollection<Quest> quests,
        ICollection<PlayerQuest> playerQuests,
        IReadOnlyCollection<int>? ownedItemIds = null);

    /// <summary>
    /// Moves the run to <paramref name="nextSceneId"/>: records the scene as
    /// visited, marks the run completed when the destination is the final scene and
    /// starts the quests associated with the destination scene. Fails when the run
    /// is already completed.
    /// </summary>
    EngineResult<ScenarioState> MoveToNextScene(
        ScenarioState state,
        IReadOnlyCollection<StoryScene> scenes,
        int nextSceneId,
        PlayerCharacter character,
        IReadOnlyCollection<Quest> quests,
        ICollection<PlayerQuest> playerQuests);
}