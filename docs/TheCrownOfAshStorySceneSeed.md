# The Crown of Ash: story scene seed

Source: `The_Crown_of_Ash_Interactive_Scenario.docx` (the interactive narrative "How to Use This Script", Prologue, Acts I–VI, and the seven endings). This step turns that narrative into seeded `StoryScene` records (dialogues + choices) so a story engine can present it. Typographic apostrophes and quotes are normalized to ASCII, and the literal source token `user_nickname` becomes the exact placeholder `{user_nickname}`.

## Scope

Only the interactive narrative is seeded: How-to-Use page, Prologue + Character Awakening + Racial Memory / Class Instinct, Acts I–VI scene by scene, and the seven endings plus the Final Image.

Excluded (documented source sections, not game dialogue): the "Core Story Variables" table, the "Quest Book / Experience and Level Progression / EXP tables", the War Score table rows (the concluding `GAME: War Score 12+...` line is kept as a System line), and the appendix/design material. Structural markers (`PLAYER CHOICE`, `WAYS THROUGH`, `FINAL DECISION`, `CLASS TRIAL`, `RACE MEMORY`, `CLASS INSTINCT`, act/chapter headings) are not emitted as dialogue; act/chapter headings instead become scene titles.

## Dialogue conversion

One source paragraph becomes one or more `StoryDialogue` records:

| Source line prefix | DialogueType | Speaker |
| --- | --- | --- |
| `Author: ...` | Narration | *empty* |
| `user_nickname: ...` | Player | `{user_nickname}` |
| `GAME: ...` | System | `GAME` |
| any character label, e.g. `Sil'Vaneth: ...` | NPC | the label verbatim |
| unlabeled descriptive lines (racial memory, class call, `Cost:`, `Reward:`, `Success:`, `Revelation:`, path headings) | Narration | *empty* |

No scenario sentence is reworded. `DialogueOrder` is 1..n in reading order. Lines longer than 350 characters are split on sentence boundaries into multiple narration boxes, also under 350 characters each.

## Choices

Every authored option becomes a `StoryChoice` whose `Text` keeps the option **including its bracketed guidance verbatim** (e.g. `Free the fragment with a WITS ritual. [Success: second_fragment=true; Sil'Vaneth becomes an ally.]`). Every choice carries a `NextSceneId`. `Requirements` and `Consequences` are intentionally left empty: the bracketed guidance realises them, and resolving it into typed effects belongs to the game engine, not this seed.

Linear beats still route through a single `Continue` choice so every non-final scene has a connected flow. `Continue` is a UI affordance only and is not authored scenario text.

## Scene IDs

- Up to 899: not used; scene ids below 900 are the prologue (none exist there now).
- 900–903: Act 0 framing — 900 How to Use, 901 Prologue, 902 Awakening, 903 Racial Memory and Calling.
- 1001–1004 + 1205: Act I chapter 1 (1001 dialogue, 1002 choice hub, 1003/1004 statue/question outcomes) and the chapter 2 crossroads.
- 1201/1202/1203/1204: the Elf/Orc/Human/Dwarf routes (ids kept from the quest scene catalog). 1211/1212, 1221/1222, 1231/1232, 1241/1242 are their choice/payoff splits.
- 2003/2004/2005: Act II chapters 3/4/5 (ids kept). 2013, 2023, 2033, 2014, 2015 are the registration-trial, companion-choice and report-response splits.
- 3005: Act III "The Silent Grain Road" — a reference scene only (the source has no authored dialogue for it).
- 3006/3007: Act III chapters 6/7 (ids kept). 3016 is the solution hub; 3026/3027, 3036/3037, 3046/3047, 3056/3057 are the four solution paths; 3017 is the Oakheaven payoff.
- 4100: Act IV hub. 4008/4009/4010: regional chapters 8/9/10 (ids kept), with choice/payoff splits 4018/4028, 4019/4029, 4020/4030.
- 4111/4112/4113: chapter 11 "Traitor's Errand" as location variants (ids kept) for the last-fragment region; each routes to its payoff 4114/4115/4116. The linear seed graph enters 4111 only; the engine selects the variant matching the region that supplied the final fragment.
- 5101–5109: the nine gates (ids kept), four methods each (force, guile, ritual, mercy). 5012/5014 + 5013: chapters 12/13.
- 6014–6017: Act VI chapters 14–17 (ids kept). 6017 repeats the six `FINAL DECISION` options verbatim from 6016 and routes each to its ending:
  BREAK → 6101, WEAR → 6102, SING → 6103, REFORGE → 6104, GIVE → 6105, CARRY → 6106.
- 6101–6107: the seven endings (Act 6, chapter 18), each ending in `Continue` → 6108 "Final Image" (`IsFinalScene = true`). 6107 "The Ash Falls" is the only ending with no inbound choice: the Ash Clock reaching 10 triggers it.
- Dialogue and choice IDs are assigned from 90001 upward; the seed is now per-instance (no static mutable state) so parallel seeding is safe.

## Migration

Scene creation moved out of `ScenarioMainQuestSeedData.SeedSceneReferences` into `ScenarioStorySceneSeedData`. The reference-only catalog was dropped; the same scene IDs are now created here with dialogues/choices, so all quest `AssociatedSceneIds`/`CompleteScene` targets still resolve. `GameDataSeeder` invokes `ScenarioStorySceneSeedData.Seed(store)` before the quest seed.

## Generated paragraphs

`ScenarioStorySceneSeedData.Paragraphs.cs` is an auto-generated partial that holds the extracted source paragraphs in `P[]` (array index = source paragraph index). It is regenerated from the tab-separated `paragraphs.txt` extraction of the docx; normalize only apostrophes/quotes and the `{user_nickname}` placeholder, never the wording.