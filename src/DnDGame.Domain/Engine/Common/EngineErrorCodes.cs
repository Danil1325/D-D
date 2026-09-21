namespace DnDGame.Domain.Engine.Common;

/// <summary>
/// Stable error codes returned by game-engine operations.
/// </summary>
public static class EngineErrorCodes
{
    public const string BattleNotFound = "BATTLE_NOT_FOUND";
    public const string BattleAlreadyFinished = "BATTLE_ALREADY_FINISHED";
    public const string NotPlayerTurn = "NOT_PLAYER_TURN";
    public const string InvalidAction = "INVALID_ACTION";
    public const string PlayerDead = "PLAYER_DEAD";
    public const string EnemyDead = "ENEMY_DEAD";
    public const string MissingCombatRule = "MISSING_COMBAT_RULE";
    public const string InvalidDice = "INVALID_DICE";
    public const string ConsequenceAlreadyApplied = "CONSEQUENCE_ALREADY_APPLIED";
    public const string RewardsAlreadyGranted = "REWARDS_ALREADY_GRANTED";

    // Scenario engine (Domain.Engine.Scenario)
    public const string ScenarioInvalidState = "SCENARIO_INVALID_STATE";
    public const string ScenarioSceneNotFound = "SCENARIO_SCENE_NOT_FOUND";
    public const string ScenarioChoiceNotFound = "SCENARIO_CHOICE_NOT_FOUND";
    public const string ScenarioChoiceAlreadySelected = "SCENARIO_CHOICE_ALREADY_SELECTED";
    public const string ScenarioRequirementNotMet = "SCENARIO_REQUIREMENT_NOT_MET";
    public const string ScenarioStoryCompleted = "SCENARIO_STORY_COMPLETED";

    // Location unlock engine (Domain.Engine.Locations)
    public const string LocationInvalidContext = "LOCATION_INVALID_CONTEXT";
    public const string LocationRequirementNotMet = "LOCATION_REQUIREMENT_NOT_MET";
    public const string LocationAlreadyUnlocked = "LOCATION_ALREADY_UNLOCKED";
    public const string LocationNotUnlocked = "LOCATION_NOT_UNLOCKED";
}
