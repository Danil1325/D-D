using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// Conditions for a choice. All populated conditions must hold; null means no restriction.
/// Race and class identifiers refer to the existing Race and CharacterClass entities.
/// </summary>
public class ChoiceRequirement
{
    public int? RaceId { get; set; }
    public int? ClassId { get; set; }
    public int? MinimumLevel { get; set; }

    /// <summary>The attribute to check; populate together with MinimumStat.</summary>
    public AttributeType? Stat { get; set; }

    /// <summary>Inclusive minimum value for Stat; populate together with Stat.</summary>
    public int? MinimumStat { get; set; }

    /// <summary>Quest that must be present in ScenarioProgress.QuestProgress.</summary>
    public int? RequiredQuestId { get; set; }

    /// <summary>Optional inclusive progress threshold for RequiredQuestId.</summary>
    public int? MinimumQuestProgress { get; set; }

    /// <summary>Item that must be present in the character's inventory.</summary>
    public int? RequiredItemId { get; set; }

    /// <summary>Flag to check in ScenarioProgress.StoryFlags. Missing flags count as false.</summary>
    public string? RequiredFlag { get; set; }

    public bool RequiredFlagValue { get; set; } = true;
}
