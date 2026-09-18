using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>All populated conditions must hold; null conditions impose no restriction.</summary>
public class QuestPrerequisite
{
    /// <summary>Quest whose player progress must have RequiredQuestStatus in the same session.</summary>
    public int? RequiredQuestId { get; set; }

    /// <summary>Only checked when RequiredQuestId is populated.</summary>
    public QuestStatus RequiredQuestStatus { get; set; } = QuestStatus.Completed;

    public int? MinimumLevel { get; set; }

    /// <summary>Flag checked in ScenarioProgress.StoryFlags. Missing flags count as false.</summary>
    public string? RequiredFlag { get; set; }

    public bool RequiredFlagValue { get; set; } = true;
}
