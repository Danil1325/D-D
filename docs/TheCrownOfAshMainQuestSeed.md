# The Crown of Ash: main quest seed

Source: `The_Crown_of_Ash_Interactive_Scenario.docx`, “Quest Book - Main and Side Quests / Main Quest Chain”, with scene associations from Acts I–VI. English descriptions and rewards follow that document; typographic apostrophes are normalized to ASCII.

## Data conventions

- Quest IDs 1–15 correspond to MQ-01–MQ-15. Objective IDs are quest ID × 100 plus the objective number. These are technical identifiers assigned here, not IDs printed in the document.
- The previous main quest is the standard prerequisite, and the next numbered quest is the standard continuation. Recommended levels are not hard prerequisites. MQ-01 has none; MQ-15 has no next quest.
- `RecommendedLevel` and `RecommendedMaximumLevel` preserve the source's ranges.
- `LocationId` is null when the location depends on the race or fragment order. `PossibleLocationIds` lists the actual alternatives. MQ-05 and MQ-14 use the journey's destination as their primary location.
- Scene records are a reference catalog for source sections, not a playable dialogue/choice graph. No dialogue or choices have been invented. Chapter 0 means the source section is unnumbered (the grain road journal entry and the nine gates).
- `TargetId` refers to an existing location for Travel or scene for CompleteScene. `TargetCode` is a stable narrative event/item/NPC key where no numeric entity exists; a future quest evaluator must resolve those keys, not interpret them as database IDs.
- `crown-fragments` measures the cumulative number of **distinct** fragments recovered (1, 2, 3), including any recovered during the racial opening. It is not an instruction to obtain 1 + 2 + 3 new fragments.
- MQ-07–MQ-09 follow acquisition order. All three can use Ashtonia, Whispering Woods or Karag-Dur. The regional first/second/third fragment flags in the narrative must not be assigned automatically from the quest number.
- `RewardDescription` preserves non-EXP rewards, including conditional outcomes. No fixed loyalty, militia War Score, trust, items or traitor identity is awarded by this seed.
- MQ-03's trial reward and quest reward are the same 150 EXP.
- MQ-11 has nine objective rewards of 40 EXP and a completion reward of 180 EXP: 540 EXP total, excluding combat. Objective reward application and deduplication belong to future quest logic.
- MQ-15 awards 0 EXP: the reward is the ending/epilogue.
- The standard chain does not override early/alternative endings. MQ-10 can defer identification to the throne room; MQ-12 can unlock The Crown Given; Ash Clock 10 can trigger The Ash Falls anywhere, bypassing normal prerequisites and remaining phases. These source rules are recorded in `ScenarioNotes`; no quest/ending engine is introduced here.

## Scene associations

| Quest | Source section / seeded scene IDs |
| --- | --- |
| MQ-01 | 1001 |
| MQ-02 | 1201, 1202, 1203, 1204 |
| MQ-03 | 2003, 2004 |
| MQ-04 | 2005 |
| MQ-05 | 3005 |
| MQ-06 | 3006, 3007 |
| MQ-07 | 4008, 4009, 4010 |
| MQ-08 | 4008, 4009, 4010 |
| MQ-09 | 4008, 4009, 4010, 4111, 4112, 4113 |
| MQ-10 | 4111, 4112, 4113 |
| MQ-11 | 5101, 5102, 5103, 5104, 5105, 5106, 5107, 5108, 5109 |
| MQ-12 | 5012 |
| MQ-13 | 5013 |
| MQ-14 | 6014 |
| MQ-15 | 6015, 6016, 6017 |

Scene IDs 1001 and 1201–1204 cover Act I chapter 1 and the four chapter 2 race routes. 2003–2005 cover Act II chapters 3–5. 3005 is the grain road journal entry; 3006–3007 cover Act III chapters 6–7. 4008–4010 cover the regional chapters 8–10, and 4111–4113 are location variants of chapter 11. 5101–5109 cover the nine gates, followed by 5012–5013 for chapters 12–13. 6014–6017 cover Act VI chapters 14–17.

The location image keys are copied from existing location seed data without changing their spelling.
