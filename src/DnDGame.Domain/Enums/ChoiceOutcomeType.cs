namespace DnDGame.Domain.Enums;

/// <summary>
/// What happens when a player picks a Choice. See the Choice entity's comments for
/// which of its fields are meaningful for each outcome type.
///
/// Phase 1 note: all 7 values exist here so the shape of the system is complete, but
/// the first mock adventure (Phase 2) only exercises DiceCheck, StartCombat, and
/// GiveReward. The remaining four are wired up with real logic in a later phase.
/// </summary>
public enum ChoiceOutcomeType
{
    DiceCheck,
    StartCombat,
    GiveReward,
    CauseDamage,
    ProvideInformation,
    ChangeStoryPath,
    TriggerEncounter
}
