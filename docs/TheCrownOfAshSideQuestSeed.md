# The Crown of Ash: side quest seed

Source: `The_Crown_of_Ash_Interactive_Scenario.docx`, “Side Quests” and “Quest Completion Rules”.

The 29 quests are stored in `InMemoryGameDataStore.Quests`, following the main quests. `ScenarioSideQuestSeedData` is a partial class with one file per region. Each quest is linked from its location's `AvailableSideQuestIds`.

## Source and configuration boundaries

Descriptions, quest givers, encounter summaries, additional rewards and EXP follow the document; apostrophes are normalized to ASCII. The document does not provide per-side-quest levels. The levels below are initial balance configuration, not source quotations.

| Code | Recommended level (configuration) | EXP (source) |
| --- | --- | --- |
| SQ-HO-01 | 1 | 80 |
| SQ-HO-02 | 1 | 100 |
| SQ-HO-03 | 2 | 90 |
| SQ-MP-01 | 3 | 110 |
| SQ-MP-02 | 2 | 100 |
| SQ-MP-03 | 3 | 130 |
| SQ-MP-04 | 3 | 90 |
| SQ-MP-05 | 5 | 150 |
| SQ-WW-01 | 3 | 120 |
| SQ-WW-02 | 3 | 100 |
| SQ-WW-03 | 4 | 140 |
| SQ-WW-04 | 6 | 170 |
| SQ-AS-01 | 4 | 130 |
| SQ-AS-02 | 5 | 120 |
| SQ-AS-03 | 5 | 150 |
| SQ-AS-04 | 6 | 170 |
| SQ-OH-01 | 3 | 110 |
| SQ-OH-02 | 4 | 130 |
| SQ-OH-03 | 4 | 150 |
| SQ-OH-04 | 4 | 100 |
| SQ-BP-01 | 6 | 140 |
| SQ-BP-02 | 6 | 160 |
| SQ-BP-03 | 7 | 180 |
| SQ-BP-04 | 7 | 170 |
| SQ-BP-05 | 7 | 190 |
| SQ-DK-01 | 8 | 190 |
| SQ-DK-02 | 9 | 220 |
| SQ-DK-03 | 9 | 210 |
| SQ-DK-04 | 9 | 230 |

- IDs 1001–1029 and objective IDs (quest ID × 100 + objective ordinal) are technical identifiers, not source IDs.
- All quests have `QuestType.Side`, so `IsOptional` is true. Their internal objectives are still required for their chosen resolution. None is made a prerequisite of the main chain.
- `ExperienceReward` derives from the existing `Rewards` collection. `AdditionalRewards` preserves narrative rewards without inventing quantities for plural supplies or numeric effects absent from the document.
- Entry `RequiredFlags` are empty unless a specific narrative dependency is configured. SQ-WW-04 uses `elf_scout_freed`, inferred from its quest giver and SQ-WW-01; this is not an explicit numbered prerequisite printed in the source.
- Objective target codes are narrative event keys for future quest logic; no NPC, item or choice database IDs are invented.
- `Enemies` reuses existing enemy templates. Guild guards, hunters, animated books and fellowship echoes remain narrative-only encounters with null `EnemyId`. Null `Count` means unspecified; two Wraith ambushes do not imply exactly two Wraiths.
- Conditional encounters retain required flags or minimum Ash Clock. The original encounter summary also retains timers, puzzles and non-combat options.

## Outcomes

This change configures outcomes; it does not introduce a quest execution service.

Select **one** outcome per resolved player quest, not all matching entries. Check the quest's entry flags separately from the outcome's choice flags. Apply the outcome's EXP percentage once through the existing experience service. Grant successful quest flags only on success; failure outcomes contain their own failed/unresolved flags.

- Success: 100% of the source EXP.
- Failure that continues the story with a meaningful consequence: 40%, as prescribed by the document.
- Failure without that condition: 0%.
- A per-quest `sq_*_meaningful_failure` flag records that distinction. Failure does not grant completion items, companion loyalty, allies or success flags.
- The document gives no per-quest numeric failure penalty. Failure marks the quest unresolved for later narrative consequences; no arbitrary War Score, Ash Clock or corruption penalties have been added.
- `WarScoreChange`, `AshClockChange`, `CorruptionChange`, companion loyalty changes, counters, item quantities and ally availability are structured outcome data. A future evaluator must apply them to runtime state.
- Companion and ally codes are stable narrative keys. `chosen-companion` means the player's selected companion; these are not fictitious numeric entity IDs.
- The source does not give a numeric corruption delta for Irix's choices (SQ-AS-03). Those three outcomes deliberately retain null `CorruptionChange` and their narrative explanation; balancing must supply a value before automatic application.
- SQ-OH-03 grants a safe-rest flag. It does not reduce historical Ash Clock.
- SQ-AS-02 grants +1 War Score only when the drum is preserved; SQ-DK-03 grants +1 only through the truce.
- SQ-WW-03 and SQ-BP-03 grant -1 corruption.
- SQ-DK-02 separates the Divine Chimera ally from legendary material into alternative outcomes.
- Potential alliances and recruitment are distinct. SQ-BP-02 unlocks a troll alliance route; SQ-OH-02 unlocks the goblin alliance route. Neither immediately recruits an army.
- Allies such as Odessa's fleet, the Elf scout and restored Divine Chimera are made available for the finale; their eventual War Score is not added a second time by this seed.

The seven location image keys and the existing main quest descriptions/rewards remain unchanged.
