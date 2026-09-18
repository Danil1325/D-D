using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// One alternative quest resolution. Select exactly one eligible outcome, once per player quest.
/// This is configuration only; applying rewards, flags and effects belongs to quest logic.
/// </summary>
public class QuestOutcome
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QuestStatus Status { get; set; } = QuestStatus.Completed;
    public Dictionary<string, bool> RequiredFlags { get; set; } = new();
    public Dictionary<string, bool> ResultFlags { get; set; } = new();

    /// <summary>Percentage of Quest.ExperienceReward: 100 on success, 40 on meaningful failure, otherwise 0.</summary>
    public int ExperienceRewardPercentage { get; set; } = 100;

    public int WarScoreChange { get; set; }
    public int AshClockChange { get; set; }

    /// <summary>Signed delta; null means the source leaves the choice-dependent amount unspecified.</summary>
    public int? CorruptionChange { get; set; } = 0;

    /// <summary>Signed loyalty deltas by stable companion code; chosen-companion is resolved at runtime.</summary>
    public Dictionary<string, int> CompanionLoyaltyChanges { get; set; } = new();

    /// <summary>Other source-defined counters, such as trust_aldwyn or militia_health.</summary>
    public Dictionary<string, int> CounterChanges { get; set; } = new();

    /// <summary>Items with source-defined quantities, keyed by narrative item code.</summary>
    public Dictionary<string, int> Items { get; set; } = new();

    /// <summary>Finale ally availability by stable code. Unlocking an alliance route is not recruiting an ally.</summary>
    public Dictionary<string, bool> Allies { get; set; } = new();
}
