using DnDGame.Domain.Common;
using DnDGame.Domain.Entities.Enemies;
using DnDGame.Domain.Enums;

namespace DnDGame.Domain.Entities.Game;

/// <summary>
/// One option a player can pick at a Narrative StoryNode. Which of the fields below
/// are meaningful depends on OutcomeType:
///
///   DiceCheck          -> RequiredAttribute, DifficultyValue, NextNodeId (success), FailureNodeId
///   StartCombat        -> EnemyId (an immediate fight right here), NextNodeId (after victory)
///   TriggerEncounter   -> NextNodeId (branch to another node, which may itself be Combat)
///   GiveReward         -> RewardXp, RewardText, NextNodeId
///   CauseDamage        -> DamageAmount, NextNodeId
///   ProvideInformation -> InfoText, NextNodeId
///   ChangeStoryPath    -> NextNodeId
///
/// Design decision (Phase 1): this is one table with nullable columns per outcome
/// type, rather than a class hierarchy per outcome — simpler to seed, query, and
/// explain, at the cost of a few unused columns per row depending on OutcomeType.
///
/// Scope note: the first mock adventure (Phase 2) only needs DiceCheck, StartCombat,
/// and GiveReward to have real logic behind them. The other four outcome types exist
/// here so the shape is complete, and get real handling in a later phase.
/// </summary>
public class Choice : BaseEntity
{
    public int StoryNodeId { get; set; }
    public StoryNode? StoryNode { get; set; }

    public string ChoiceText { get; set; } = string.Empty;
    public ChoiceOutcomeType OutcomeType { get; set; }

    // --- DiceCheck fields ---
    public AttributeType? RequiredAttribute { get; set; }
    public int? DifficultyValue { get; set; }
    public int? FailureNodeId { get; set; }
    public StoryNode? FailureNode { get; set; }

    // --- StartCombat field ---
    public int? EnemyId { get; set; }
    public Enemy? Enemy { get; set; }

    // --- GiveReward fields ---
    public int? RewardXp { get; set; }
    public string? RewardText { get; set; }

    // --- CauseDamage field ---
    public int? DamageAmount { get; set; }

    // --- ProvideInformation field ---
    public string? InfoText { get; set; }

    /// <summary>
    /// The node to go to next. Used as the "success" destination for DiceCheck; as
    /// the destination reached immediately for TriggerEncounter, GiveReward,
    /// CauseDamage, ProvideInformation, and ChangeStoryPath; and, for StartCombat, as
    /// where the story continues once the enemy identified by EnemyId is defeated.
    /// </summary>
    public int? NextNodeId { get; set; }
    public StoryNode? NextNode { get; set; }
}
