# Handoff: Person 3 backend work — DnD Game (card-battle API)

You are continuing backend work on a university D&D web game (ASP.NET Core + C#,
React/TypeScript frontend, no database yet — mock data only, no SignalR, no
multiplayer). I am Person 3. Three people work on separate branches, merged into
`main`. This handoff is based on a direct inspection of the actual merged code —
not on the original task spec, which is now partly outdated. Where the two
disagree, trust the code and this handoff, not the task spec.

If you encounter an architectural ambiguity that cannot be resolved from the
existing code or this handoff, STOP and ask me before making the decision.
Do not silently choose an architecture just to keep coding.

## Step 0 — before touching anything

1. Read `docs/ARCHITECTURE.md` in full. It was written by a teammate and I
   verified its key claims against the actual source during my review — it is
   accurate as of the last merge. Treat it as a reliable map of the codebase.
2. Re-verify anything you're about to depend on by opening the actual file —
   the architecture doc and this handoff describe the state as of the last
   merge; don't assume it hasn't drifted.
3. Run a full solution build and full test run *before* making any change, so
   you have a known-good baseline to compare against:
   ```
   dotnet build
   dotnet test
   ```
   Expected baseline: solution builds with 0 warnings/0 errors; test suite
   passes (was 229/229 at last merge — confirm the current number and record
   it before you start).

## 1. Solution structure

```
DnDGame.sln
├── src/
│   ├── DnDGame.Domain/           Pure C#, zero external deps: entities, enums, configuration, game engine.
│   ├── DnDGame.BusinessLayer/    Card-battle engines, effects, models, validation, infra contracts.
│   ├── DnDGame.MockData/         In-memory store, seed data, Mock* repositories, current-player service.
│   └── DnDGame.API/              ASP.NET Core entry point: Program.cs, composition root, middleware, controllers.
└── tests/
    └── DnDGame.Tests/            xUnit.
```

Reference chain: `API → BusinessLayer → Domain`, `API → MockData → Domain`. Domain
has zero external dependencies. Never introduce a reference that violates this
chain (e.g. Domain must never reference BusinessLayer or MockData).

## 2. Ownership map (for context only — don't reorganize by this)

| Branch | Person | Scope |
|---|---|---|
| `battle_models` | P1 | Domain game engine: dice, combat, initiative, saving throws, enemy actions, turn, battle |
| `catalina` | P2 | Card-battle business layer, card domain entities, DI composition root |
| `alexandru` (me, P3) | P3 | API infrastructure: middleware, DTOs, validation, error mapping, mock bootstrapper |

## 3. What Person 1 actually implemented (`src/DnDGame.Domain/Engine/*`)

**Fully implemented and resolvable via DI today:**
- `IDiceEngine`/`DiceEngine` — `EngineResult<DiceResult> Roll(DiceType, int modifier)`.
- `Domain.Engine.Combat.IDamageCalculator`/`DamageCalculator` — `EngineResult<DamageResult> Calculate(DamageRequest)`.
- `IDodgeCalculator`/`DodgeCalculator`, `ICriticalCalculator`/`CriticalCalculator`.
- `IInitiativeEngine`/`InitiativeEngine`, `ISavingThrowEngine`/`SavingThrowEngine`.
- `IEnemyActionSelector`/`EnemyActionSelector`, `IBattleLogWriter`/`BattleLogWriter`.

**Implemented but NOT resolvable via DI (missing dependencies — do not implement these yourself, see §7):**
- `ITurnEngine`/`TurnEngine` — needs `Domain.Engine.Deck.IDeckEngine`, `Domain.Engine.Hand.IHandEngine`, `Domain.Engine.Effects.IEffectEngine` (none implemented).
- `IBattleEngine`/`BattleEngine` — needs the same three plus `IEnemyDefenseRule` (also unimplemented).
- Data types: `BattleContext` (wraps `PlayerCharacter`, `Enemy`, `BattleState` — NOT a DI service, created on demand), `BattleState` (transient snapshot: PlayerHealth/EnemyHealth/Hand/DrawPile/DiscardPile of `Engine.Models.CardInstance`/ActiveEffects/BattleLog).
- `Domain.Engine.Models.CardInstance` — explicitly commented in the source as **"a placeholder"**. Do not treat it as the real card-instance type (see §6).
- Result type: `Domain.Engine.Common.EngineResult<T>` — `Success`/`Message`/`ErrorCode` (**string**, nullable)/`Data`; factories `Ok(data, message)`/`Fail(message, errorCode)`. String codes in `Domain.Engine.Common.EngineErrorCodes`.

## 4. What Person 2 actually implemented (`src/DnDGame.BusinessLayer/Engines/*`, `src/DnDGame.Domain/Entities/Cards`, `src/DnDGame.Domain/Entities/Game`)

**Fully implemented and resolvable via DI today** (register via `AddCardBattleServices()` in `src/DnDGame.API/CompositionRoot/DependencyInjection.cs`):
- `IDeckEngine`/`DeckEngine` — `CreateBattleDeck(Deck)`, `ShuffleDeck(IList<CardInstance>)`, `DrawCard(BattleDeck)`, `DrawCards(BattleDeck, count)`, `DiscardCard(BattleDeck, CardInstance)`, `ReshuffleDiscardPile(BattleDeck)`.
- `IHandEngine`/`HandEngine` — `AddCard(s)(BattleDeck, ...)`, `RemoveCard`, `FindCard(BattleDeck, Guid instanceId)`, `FindCardByDefinitionId(BattleDeck, int cardId)`, `DiscardHand(BattleDeck)`, `GetHandSize(BattleDeck)`. Returns `BusinessLayer.Models.EngineResult` (non-generic).
- `IPlayEngine`/`PlayEngine` — `CanPlayCard(Battle, BattleDeck, CardInstance, int playerId, ICardTarget?)`, `IsBattleActive(Battle)`, `IsPlayerTurn(Battle, int playerId)`, `IsValidTarget(CardInstance, ICardTarget?)`.
- `IAbilityUseEngine`/`AbilityUseEngine` — `EngineResult<AbilityUseResult> UseAbility(PlayerCharacter, PlayerCard, AbilityUseContext)`.
- `IDeckValidator`/`DeckValidator` — `EngineResult ValidateDeck(Deck)` (checks `DeckRules`: min/max size, copy limits).

**Declared but NOT resolvable via DI today** (missing dependency — do not implement yourself, see §7):
- `ICardEngine`/`CardEngine` — `EngineResult<CardPlayResult> PlayCard(Battle, BattleDeck, CardInstance, int playerId, ICardTarget?)`. Depends on `IEffectEngine`.
- `IEffectEngine`/`EffectEngine` — depends on `CardEffectRegistry`, which depends on all 9 `ICardEffect` strategies including `DamageEffect`, which depends on `BusinessLayer.Effects.Interfaces.IDamageCalculator` — **no production implementation exists**. This is the single blocker for the whole `ICardEngine`/`IEffectEngine` chain.

**Card domain (fully implemented, no blockers)** — `src/DnDGame.Domain/Entities/Cards/`:
- `Card` (definition/catalogue entry), `CardInstance` (the **real** one — `InstanceId`, `CardId`, `Card` nav, `TemporaryCostModifier`/`TemporaryDamageModifier`, `GetEffectiveCost()`/`GetEffectiveDamage()`).
- `PlayerCard` (owned card: `CardId`, `Unlocked`, `Quantity`, `VisibilityRule`, `Unlock(rule, character)`, `AddCopies(n)`).
- `CardCollection` (everything one `PlayerCharacter` owns/can unlock) — **already implements**:
  - `GetCardDetails(int cardId, CardVisibilityRules?)` → visibility-filtered `CardDetails`.
  - `SearchCards(string? query, CardSearchOptions?, CardVisibilityRules?)` → filter by `Rarity`/`Category`/`Type`/`Unlocked`, sort by `CardSortBy`/`SortOrder`, returns `IReadOnlyList<CardDetails>`.
  - `UnlockCard(cardId, ICardUnlockRule, PlayerCharacter)`, `AddLockedCard(cardId, CardVisibilityRule)`.
- `CardDetails` (nullable display projection — locked fields are `null` per visibility rule).
- `CardSearchOptions` (`Rarity?`, `Category?`, `Type?`, `Unlocked?`, `SortBy`, `SortOrder`).
- `ICardUnlockRule`/`LevelCardUnlockRule`.
- **`PlayerCharacter` now has a `CardCollection? CardCollection` property directly** — the team has informally settled on `PlayerCharacter` being "the player" for card-battle purposes. No separate `Player` entity exists. (The architecture doc still calls this "not finalised" — treat it as settled enough to build on, not as fully closed.)

**Battle-flow entities (`src/DnDGame.Domain/Entities/Game/`)** — a **second, separate battle model** from Person 1's `BattleContext`/`BattleState` (see §7 conflict):
- `Battle` (persisted-entity style: `GameSessionId`, `Status`, `CurrentPlayerTurnId`, `CurrentRound`, `CurrentPlayerEnergy`/`MaxPlayerEnergyPerTurn`, `EnemyBlock`, `EnemyCurrentHealth`, `PlayerCurrentHealth`/`PlayerMaxHealth`, timestamps).
- `BattleDeck` (`DeckId`, `DrawPile`/`DiscardPile`/`Hand` of `Entities.Cards.CardInstance`, `TotalCardsDrawn`/`Discarded`, `CurrentBlock`).
- `Deck` (`Name`, `ICollection<Card> Cards`, `CharacterId`, `Description`).

**Result type** — `BusinessLayer.Models.EngineResult<T>` / non-generic `EngineResult` — `IsSuccess`/`ErrorCode` (**`Domain.Enums.ErrorCode` enum**, nullable)/`ErrorMessage`/`Data`; factories `Success(data)`/`Failure(ErrorCode, message)`. **This is a different type, with different property names and a different error-code type, from Person 1's `Domain.Engine.Common.EngineResult<T>`. Never confuse the two — they are not interchangeable and both exist in the codebase on purpose.**

`Domain.Enums.ErrorCode` (enum, Person 2's codes): `DECK_INVALID`, `DECK_TOO_SMALL`, `DECK_TOO_LARGE`, `CARD_COPY_LIMIT_REACHED`, `HAND_FULL`, `CARD_NOT_IN_HAND`, `BATTLE_NOT_ACTIVE`, `NOT_PLAYER_TURN`, `INSUFFICIENT_ENERGY`, `INVALID_TARGET`, `CARD_CANNOT_BE_PLAYED`, `ABILITY_LOCKED`, `CLASS_REQUIREMENT_NOT_MET`, `LEVEL_REQUIREMENT_NOT_MET`.

## 5. What I (Person 3) have already implemented — do not recreate these

All merged into `main` already:
- `BusinessLayer/Dtos/Common/{ApiErrorResponse,PagedRequest,PagedResult}.cs`
- `BusinessLayer/Common/Exceptions/DomainException.cs` — `(string ErrorCode, string Message)`. Services should throw this for business-level failures; the middleware converts it to `ApiErrorResponse`.
- `BusinessLayer/Common/Errors/{ErrorCodes,IErrorCodeHttpMapper,ErrorCodeHttpMapper}.cs` — generic infra codes (`VALIDATION_ERROR`→400, `NOT_FOUND`→404, `CONFLICT`→409, `INTERNAL_ERROR`→500, unknown codes→500 by design). `Register(code, status)` is additive — **use this to register Person 1/2's codes, don't add new mapping mechanisms**.
- `BusinessLayer/Validation/{ValidationResult,RequestValidationHelpers}.cs` — `RequireNonEmpty`, `RequireValidGuid`, `RequirePositiveId`, `ValidatePagination`.
- `BusinessLayer/Services/Interfaces/ICurrentPlayerService.cs` + `MockData/Services/MockCurrentPlayerService.cs` — `int GetCurrentPlayerId()`, always returns a fixed mock id (`MockCurrentPlayerService.MockPlayerId`). **Use this in every controller instead of hard-coding a player id.**
- `API/Middleware/ExceptionHandlingMiddleware.cs` + `UseGlobalExceptionHandling()` — registered first in the pipeline. Already handles `DomainException` → mapped status + `ApiErrorResponse`; anything else → logged fully, generic `INTERNAL_ERROR` to the client.
- `MockData/MockDataBootstrapper.cs` — `CreateSeededStore()`, the only public entry point to build/seed `InMemoryGameDataStore` (the individual seeders are `internal`).
- `API/CompositionRoot/DependencyInjection.cs` — `AddCardBattleServices()`, `AddBattleTurnSystemServices()`, `AddMockData()`, called from `Program.cs`. **Add new registrations into the correct one of these three, don't create a fourth or inline registrations in `Program.cs`.**
- Infra tests under `tests/DnDGame.Tests/Infrastructure/`.

## 6. Remaining Person 3 scope — build this now (nothing else)

Only the following. Do not touch Battle/BattleEngine/TurnEngine work (§7).

1. **Dice feature (fully unblocked):**
   - `DiceRequestDto` (dice type + modifier), `DiceResultDto` (mirrors `DiceResult`).
   - `IDiceService`/`DiceService` wrapping `IDiceEngine.Roll(DiceType, modifier)`.
   - `DiceController` — one endpoint, e.g. `POST /api/dice/roll`.

2. **Card feature (fully unblocked):**
   - Extend `InMemoryGameDataStore` **additively** with a `List<CardCollection>` (or equivalent keyed store) — do not remove or restructure the existing `List<Card>` catalogue.
   - `ICardCollectionRepository` (BusinessLayer interface) + `MockCardCollectionRepository` (MockData impl), following the exact pattern of the existing `Mock*Repository` classes (constructor takes `InMemoryGameDataStore`).
   - DTOs mirroring `CardDetails` (`CardDetailsDto`) + a paged list DTO using the existing `PagedRequest`/`PagedResult<T>`.
   - `ICardService`/`CardService` — thin wrapper calling `CardCollection.SearchCards(...)`/`GetCardDetails(...)` directly. **Do not reimplement filtering/sorting/visibility logic — it already exists on `CardCollection`.**
   - `CardController` — list/search endpoint (paged, with filter/sort query params) and a get-by-id endpoint. Resolve the current player via `ICurrentPlayerService`.

3. **Deck feature (fully unblocked):**
   - Extend `InMemoryGameDataStore` additively with a `List<Deck>`.
   - `IDeckRepository` (BusinessLayer) + `MockDeckRepository` (MockData), same pattern as above.
   - DTOs: `DeckResponseDto`, `DeckValidationResultDto` (translate `Domain.Enums.ErrorCode` + `ErrorMessage` from `EngineResult` into your DTO/`DomainException`).
   - `IDeckService`/`DeckService` — CRUD over `IDeckRepository`, plus a `ValidateDeck` method that calls the existing, resolvable `IDeckValidator.ValidateDeck(Deck)`.
   - `DeckController` — CRUD endpoints + `POST /api/deck/{id}/validate`.
   - Register `IDeckEngine`'s already-existing DI registration is untouched; only add your new repository/service registrations into `AddMockData()`/a new method as appropriate.

4. **Cross-cutting:**
   - Wherever a Person 1/2 engine returns its own result type, convert failures into `DomainException` at the service boundary (not in the controller), using the existing `ErrorCodes/IErrorCodeHttpMapper` machinery. For `BusinessLayer.Models.EngineResult`'s `Domain.Enums.ErrorCode` enum, use `errorCode.ToString()` as the string code, and register HTTP mappings for the ones you actually hit (e.g. `DECK_TOO_SMALL`→400, `DECK_TOO_LARGE`→400, `CARD_COPY_LIMIT_REACHED`→400) via `IErrorCodeHttpMapper.Register(...)` in the composition root — do not hardcode a switch statement elsewhere.
   - Add integration/unit tests for everything above, following the existing test project's structure and conventions (see `tests/DnDGame.Tests/Infrastructure/` and `BusinessLayer/` for style).

## 7. Conflicts/blockers you must NOT try to resolve

There are two independently-built, non-interoperable battle systems merged into `main`:

| | Person 1 | Person 2 |
|---|---|---|
| Battle state | `BattleContext` + `BattleState` (transient) | `Battle` + `BattleDeck` (persisted-style) |
| Card instance | `Engine.Models.CardInstance` (placeholder) | `Entities.Cards.CardInstance` (real) |
| Result type | `Domain.Engine.Common.EngineResult<T>` (string code) | `BusinessLayer.Models.EngineResult<T>`/`EngineResult` (enum code) |
| Orchestration | `IBattleEngine.StartBattle/PlayCard/EndTurn/ExecuteEnemyTurn` | none — no top-level battle lifecycle exists on this side |

Consequence: **there is currently no working path to resolve an enemy's turn at all** — that logic exists only inside Person 1's `BattleEngine.ExecuteEnemyTurn`, which cannot be constructed (missing `Domain.Engine.{Cards,Deck,Hand,Effects}` seam implementations + `IEnemyDefenseRule`). This is a genuine cross-team integration gap, not something for you to patch over.

**Do not, under any circumstances:**
- Implement `Domain.Engine.Cards.ICardEngine`, `Domain.Engine.Deck.IDeckEngine`, `Domain.Engine.Hand.IHandEngine`, `Domain.Engine.Effects.IEffectEngine`, `IEnemyDefenseRule`, or `BusinessLayer.Effects.Interfaces.IDamageCalculator`. These are Person 1's contracts to fulfill (a couple are arguably Person 2's — either way, not mine/yours).
- Write an adapter/bridge between `BattleContext`/`BattleState` and `Battle`/`BattleDeck`. That's a team architecture decision, not a Person 3 task.
- Build `BattleController`/`BattleService`'s start/play/end-turn/enemy-turn lifecycle, or any `Battle`/`BattleDeck` persistence store. This is explicitly out of scope until the team resolves the conflict above.
- Create any "fake" or "stub" implementation of an engine interface just to make something compile or to demonstrate an endpoint. If a real dependency is genuinely missing, leave that endpoint/feature unbuilt and say so — don't simulate it.
- Touch the adventure/story-graph system (`Adventure`, `StoryNode`, `Choice`, `GameSession`) — unrelated to this scope, not blocked, just not mine to change.
- Rename, restructure, or "clean up" existing Person 1/2 files, namespaces, or the two parallel `EngineResult`/`CardInstance`/battle-model pairs described above. They're intentionally decoupled for now — that's documented in `docs/ARCHITECTURE.md` §13, not an accident to fix.
- Modify `docs/ARCHITECTURE.md` beyond appending a short note about what you added (if anything) — don't rewrite sections describing Person 1/2's work.

If you find yourself needing any of the above to make a Section 6 feature work, stop and flag it instead of working around it.

## 8. Git / commit workflow
Github: https://github.com/Danil1325/D-D/tree/alexandru
My branch: Alexandru

- Work on my branch (alexandru). Do not commit directly to `main`. Work on my current Person 3 feature branch. Before making changes, run git branch --show-current and confirm the current branch. Do not switch branches or create a new branch unless I explicitly ask you to. Never commit directly to main and never push to main.
- After completing each logical step, stop and show me:
        - what changed
	- files changed
	- build/test results
	- proposed commit message
Then, after I approve, i will commit.
Do NOT automatically move on to the next feature after committing.
E.G:
1. Dice implementation + tests → one commit
2. Card implementation + tests → one commit
3. Deck implementation + tests → one commit
4. Shared wiring/tests only if genuinely separate
- Do not squash-merge away this history — each commit should build and pass tests on its own (see §9), so `git bisect` stays useful.
- Do not touch files outside `src/DnDGame.BusinessLayer`, `src/DnDGame.MockData`, `src/DnDGame.API` (Controllers/DTOs/CompositionRoot only), and `tests/DnDGame.Tests`, except the one-line `docs/ARCHITECTURE.md` addition mentioned above.
- Open a PR back into `main` when done; do not merge it yourself — I'll review.

## 9. Build/test requirements before every commit

Before every single commit:
```
dotnet build     # must succeed, 0 errors — warnings should not increase from baseline
dotnet test      # every existing test must still pass; new tests you added must pass
```
If a test that passed at baseline (§0) now fails, do not commit until you've fixed it or determined it's an intentional, justified change — and if it's the latter, say so explicitly in the commit message and to me, don't silently let it fail.

Do not leave any temporary `NuGet.Config`, local package source overrides, or other machine-specific workarounds in the repo when you commit.

## 10. Definition of done for this handoff

- Dice, Card, and Deck features are implemented per §6, each with passing tests.
- No file touched outside the boundaries in §8.
- No engine interface from §7's "do not implement" list has a new implementation.
- `dotnet build` and `dotnet test` both pass on the final branch state.
- A short summary of what was built, and an explicit list of anything you deliberately left unbuilt because it depends on the §7 conflict, ready for me to relay back to the team.
