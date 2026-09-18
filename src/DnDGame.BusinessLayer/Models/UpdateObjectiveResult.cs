namespace DnDGame.BusinessLayer.Models;

/// <summary>
/// Outcome of advancing one quest objective. When <see cref="Completed"/> is set,
/// the update completed the whole quest and the grant details are reported there.
/// </summary>
public sealed record UpdateObjectiveResult(
    int QuestId,
    int ObjectiveId,
    int CurrentProgress,
    int RequiredAmount,
    bool ObjectiveCompleted,
    int? NextObjectiveIndex,
    QuestCompletionResult? Completed);