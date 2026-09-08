using DnDGame.Domain.Entities.Game;
using DnDGame.Domain.Enums;

namespace DnDGame.MockData.SeedData;

/// <summary>
/// Seeds one small branching adventure, "The Whispering Crypt", built specifically
/// to exercise all 7 ChoiceOutcomeType values at least once, per the approved plan.
///
/// Graph shape:
///
///   N1 (Narrative, start)
///     |-- DiceCheck (Dex vs 8) --success--> N2 --GiveReward--> N5
///     |                        \-failure--> N3 --CauseDamage--> N5
///     `-- ChangeStoryPath -------------------------------------> N4 --ProvideInformation--> N5
///
///   N5 (Narrative) --StartCombat (Skeleton)--> N6
///   N6 (Narrative) --TriggerEncounter--------> N7 (Combat, Hobgoblin) --> N8 (Ending)
///
/// Every branch converges at N5 before the adventure continues linearly to its end,
/// keeping the graph small while still genuinely branching and reconverging.
/// </summary>
internal static class AdventureSeedData
{
    public static void Seed(InMemoryGameDataStore store)
    {
        var adventure = new Adventure
        {
            Id = 1,
            Name = "The Whispering Crypt",
            Description = "A short introductory adventure built to exercise every kind of story choice the game supports.",
            RecommendedLevel = 1,
            StartingNodeId = 1
        };
        store.Adventures.Add(adventure);

        store.StoryNodes.AddRange(new[]
        {
            new StoryNode
            {
                Id = 1,
                AdventureId = adventure.Id,
                Title = "Crypt Entrance",
                NarrativeText = "You stand before a crumbling crypt entrance. A narrow gap in the collapsed rubble leads one way; a wide, torch-lit passage leads another.",
                NodeType = NodeType.Narrative
            },
            new StoryNode
            {
                Id = 2,
                AdventureId = adventure.Id,
                Title = "Through the Gap",
                NarrativeText = "You slip through the narrow gap unnoticed and find a small alcove tucked into the rock.",
                NodeType = NodeType.Narrative
            },
            new StoryNode
            {
                Id = 3,
                AdventureId = adventure.Id,
                Title = "Scraped and Bruised",
                NarrativeText = "You get wedged in the rubble and have to force your way through, scraping yourself badly in the process.",
                NodeType = NodeType.Narrative
            },
            new StoryNode
            {
                Id = 4,
                AdventureId = adventure.Id,
                Title = "The Lit Passage",
                NarrativeText = "Torches line the walls here. A faded inscription is carved into the stone beside a heavy door.",
                NodeType = NodeType.Narrative
            },
            new StoryNode
            {
                Id = 5,
                AdventureId = adventure.Id,
                Title = "The Crypt Heart",
                NarrativeText = "Both paths converge here, before a heavy stone door. Something stirs in the shadows, blocking the way.",
                NodeType = NodeType.Narrative
            },
            new StoryNode
            {
                Id = 6,
                AdventureId = adventure.Id,
                Title = "Beyond the Guardian",
                NarrativeText = "With the way forward clear, you find a fork leading toward a second, deeper chamber.",
                NodeType = NodeType.Narrative
            },
            new StoryNode
            {
                Id = 7,
                AdventureId = adventure.Id,
                Title = "The Deeper Chamber",
                NarrativeText = "A tougher guardian awaits in the chamber beyond.",
                NodeType = NodeType.Combat,
                EnemyId = 5, // Hobgoblin
                NextNodeId = 8
            },
            new StoryNode
            {
                Id = 8,
                AdventureId = adventure.Id,
                Title = "Crypt Cleared",
                NarrativeText = "You emerge from the crypt, treasure and story in hand. Your adventure here is complete.",
                NodeType = NodeType.Ending
            }
        });

        store.Choices.AddRange(new[]
        {
            // N1 -- DiceCheck and ChangeStoryPath
            new Choice
            {
                Id = 1,
                StoryNodeId = 1,
                ChoiceText = "Try to slip through the narrow gap",
                OutcomeType = ChoiceOutcomeType.DiceCheck,
                RequiredAttribute = AttributeType.Dexterity,
                DifficultyValue = 8,
                NextNodeId = 2,   // success
                FailureNodeId = 3 // failure
            },
            new Choice
            {
                Id = 2,
                StoryNodeId = 1,
                ChoiceText = "Take the wide, torch-lit passage instead",
                OutcomeType = ChoiceOutcomeType.ChangeStoryPath,
                NextNodeId = 4
            },

            // N2 -- GiveReward
            new Choice
            {
                Id = 3,
                StoryNodeId = 2,
                ChoiceText = "Search the alcove",
                OutcomeType = ChoiceOutcomeType.GiveReward,
                RewardXp = 10,
                RewardText = "You find a small pouch of old coins.",
                NextNodeId = 5
            },

            // N3 -- CauseDamage
            new Choice
            {
                Id = 4,
                StoryNodeId = 3,
                ChoiceText = "Force your way through the rubble",
                OutcomeType = ChoiceOutcomeType.CauseDamage,
                DamageAmount = 3,
                NextNodeId = 5
            },

            // N4 -- ProvideInformation
            new Choice
            {
                Id = 5,
                StoryNodeId = 4,
                ChoiceText = "Read the inscription",
                OutcomeType = ChoiceOutcomeType.ProvideInformation,
                InfoText = "The inscription warns of a guardian bound to protect the crypt's heart.",
                NextNodeId = 5
            },

            // N5 -- StartCombat
            new Choice
            {
                Id = 6,
                StoryNodeId = 5,
                ChoiceText = "Draw your weapon and fight",
                OutcomeType = ChoiceOutcomeType.StartCombat,
                EnemyId = 1, // Skeleton
                NextNodeId = 6 // where the story continues after victory
            },

            // N6 -- TriggerEncounter
            new Choice
            {
                Id = 7,
                StoryNodeId = 6,
                ChoiceText = "Press deeper into a second, harder chamber",
                OutcomeType = ChoiceOutcomeType.TriggerEncounter,
                NextNodeId = 7 // a Combat-type node
            }
        });
    }
}
