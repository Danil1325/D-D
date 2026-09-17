# DnD Game Backend — Architecture & Component Reference

This document describes the backend as it stands on branch `catalina` (Persoana 2).
It covers the folder structure, every class and interface (with its responsibility and
owner), the integration surface with Persoana 1 (battle/turn systems) and Persoana 3
(API infrastructure), the Result Pattern, error codes, Dependency Injection, and the
assumptions made so far.

> Companion to `README.md` (phase tracker) and `DnD-Game-Backend-Plan.md` (design doc).

---

## 1. Solution layout

```
DnDGame.sln
├── src/
│   ├── DnDGame.Domain/           Pure C# domain: entities, enums, configuration, game engine.
│   ├── DnDGame.BusinessLayer/    Game logic: card-battle engines, effects, models, validation, infra contracts.
│   ├── DnDGame.MockData/         In-memory store, seed data, Mock* repositories, current-player service.
│   └── DnDGame.API/              ASP.NET Core Web API entry point: Program.cs, composition root, middleware.
└── tests/
    └── DnDGame.Tests/            xUnit test suite (229 tests, see §12).
```

Project reference chain:

```
DnDGame.API  →  DnDGame.BusinessLayer  →  DnDGame.Domain
DnDGame.API  →  DnDGame.MockData       →  DnDGame.Domain
DnDGame.Tests (references Domain, BusinessLayer, MockData, API)
```

- `DnDGame.Domain` has zero external dependencies — entities, enums and the game engine are plain C#.
- `DnDGame.BusinessLayer` depends only on the Domain project (and .NET base class libraries).
- `DnDGame.MockData` implements the BusinessLayer repository interfaces against `InMemoryGameDataStore`.
- `DnDGame.API` is the only project with framework/DI dependencies.

---

## 2. Branch and ownership map

The work is split across three people on separate branches, merged into `catalina`:

| Branch        | Person | Scope                                                                  | Integration |
|---------------|--------|------------------------------------------------------------------------|-------------|
| `catalina`    | P2     | Card-battle business layer + card domain + DI composition root        | current branch |
| `battle_models` | P1   | Domain game engine: dice, combat, initiative, saving throws, enemy, turn, battle | merged via PR #3 |
| `alexandru`    | P3     | API infrastructure: middleware, DTOs, validation, error mapping, mock bootstrapper | merged via PR #4 |

Ownership is inferred from branch/PR history; classes created in earlier phases (before
branching) are marked **baseline**. The merge of P1 (`battle_models`) means the Domain
engine files physically exist on `catalina` today — but several of their dependencies are
deliberately still unimplemented (see §10, Person 1 seams).

---

## 3. DnDGame.Domain

### 3.1 Entities

`/Entities/Cards` — card domain (Persoana 2):

| Class | Responsibility |
|-------|----------------|
| `Card` | Blueprint/definition of a card: name, description, `CardType`/`Category`/`Rarity`, `BaseCost` (= `ResourceCost`), `BaseDamage`, `TargetType`, `EffectType`, class/level requirements, active/passive effects, lore, and navigation to `CardInstance`. |
| `CardInstance` | A single runtime copy of a `Card` during a session. Tracks `TemporaryCostModifier`/`TemporaryDamageModifier` and exposes `GetEffectiveCost()` / `GetEffectiveDamage()`. |
| `PlayerCard` | A card owned by a player: `CardId`, `Unlocked` state, `Quantity`, and a `CardVisibilityRule` used while locked. `Unlock(rule, character)` and `AddCopies(n)` enforce invariants (valid rule, matching `CardId`, unlocked before copies). |
| `CardCollection` | Everything one `PlayerCharacter` owns or can unlock. Adds locked entries, `GetCardDetails()` (visibility-filtered projection), `SearchCards()` (filter/sort, read-only), and collection-level `UnlockCard()`. |
| `CardDetails` | Safe, nullable display projection of a `Card` produced by `CardCollection`. Locked cards expose `null` for fields their visibility rule hides. |
| `CardSearchOptions` | Value object for `CardCollection.SearchCards`: optional `Rarity`/`Category`/`Type`/`Unlocked` filters plus `SortBy`/`SortOrder`. |
| `ICardUnlockRule` | Contract for a verifiable card-unlock condition (`CardId`, `IsValid`, `IsSatisfiedBy(character)`); open to new unlock sources. |
| `LevelCardUnlockRule` | Concrete unlock rule: satisfied when the character reaches `RequiredLevel`. |

`/Entities/Game` — battle-flow runtime state (shared, living on `catalina`):

| Class | Responsibility |
|-------|----------------|
| `Battle` | The active battle: session link, status, whose turn it is, round number, player/enemy health, energy pool, `EnemyBlock`. Consumed by the BusinessLayer engines (P2) and mirrored into P1's `BattleState` via `BattleContext`. |
| `BattleDeck` | Runtime state of a deck during battle: `DrawPile`, `DiscardPile`, `Hand`, draw/discard counters, `CurrentBlock`. |
| `Deck` | A player's constructed deck (source for `CreateBattleDeck`). |
| `Adventure`, `StoryNode`, `Choice`, `GameSession`, `SessionLogEntry` | Campaign/branching-story domain (baseline, Phase 1/2). |

`/Entities/Characters` (`PlayerCharacter`, `CharacterTalent`), `/Entities/Enemies`
(`Enemy`), `/Entities/Races`, `/Entities/Classes`, `/Entities/Talents`, `/Entities/Portraits`
— baseline Phase 1 entities from the design document; `PlayerCharacter` doubles as the
"player" concept for unlock rules and ability use.

`Common/BaseEntity` — numeric `Id` base; `Common/CardInformationVisibility` record lives
with the visibility configuration (see §3.3).

### 3.2 Configuration (Persoana 2)

| Class | Responsibility | DI defaults (see §9) |
|-------|----------------|----------------------|
| `DeckRules` | Deck build/validation limits and `ValidateDeck(size, counts)`. | Min 20, max 40, 4 copies per card |
| `HandRules` | `MaxHandSize` + `CanAddCard`/`CanAddCards`. | 10 |
| `PlayRules` | `MaxEnergyPerTurn`, `AllowOverdraft`, `CanAffordCard`, `CalculateRemainingEnergy`. | 5 energy, no overdraft |
| `CardVisibilityRules` + `CardInformationVisibility` | Field-level projection presets (`HideAll`, `ShowNameOnly`, `HideStatistics`, `ShowBasicInformation`, `Complete`) resolved from a `CardVisibilityRule`. | defaults |

These carry parameterless constructors for DI/serialization but are registered with
explicit placeholder values in the composition root.

### 3.3 Enums

- Card enums (P2): `CardType`, `CardCategory`, `CardRarity`, `CardSortBy`, `CardVisibilityRule`,
  `EffectType`, `TargetType`, `SortOrder`.
- Engine enums (P1): `TurnType`, `EffectTarget`, `BattleStatus` (under `/Engine/Enums`).
- Baseline: `AttributeType`, `EnemyTier`, `EnemyFamily`, `ChoiceOutcomeType`, `NodeType`,
  `GameSessionStatus`, `SessionLogEntryType`.
- `ErrorCode` (P2) — engine error codes used by the BusinessLayer `EngineResult` (see §8).

### 3.4 Game engine (`/Engine`, Persoana 1)

`Dice`
- `IRandomNumberSource` / `CryptographicRandomNumberSource` — injectable randomness (crypto-strong).
- `IDiceEngine` / `DiceEngine` — rolls standard `DiceType`s, returns `DiceResult`.
- `DiceResult`, `DiceType`.

`Combat`
- `IDamageCalculator` / `DamageCalculator` — resolves a `DamageRequest` into a `DamageResult`;
  the *domain* damage contract (takes a `DamageRequest`). This is NOT the same as the
  BusinessLayer `IDamageCalculator` (see §10).
- `IDamageRule` / `AdditiveDamageRule` — pluggable damage formula seam.
- `IDodgeCalculator` / `DodgeCalculator`, `IDodgeRule` — dodge chance/resolution.
- `ICriticalCalculator` / `CriticalCalculator` + `CriticalRules` — critical-hit resolution.
- Supporting models: `DamageRequest/Result`, `DodgeRequest/Result/Outcome`, `CriticalResult/Outcome`.

`Initiative`
- `IInitiativeEngine` / `InitiativeEngine` — rolls and orders turn initiative.
- `IInitiativeRule` — modifier policy seam. `InitiativeScores`, `InitiativeResult`.

`SavingThrows`
- `ISavingThrowEngine` / `SavingThrowEngine` — resolved as `SavingThrowResult` from a
  `SavingThrowRequest`.

`EnemyActions` (files live under `/Engine/Enemy`, namespace `DnDGame.Domain.Engine.EnemyActions`)
- `IEnemyActionSelector` / `EnemyActionSelector` — delegates to a configurable `IEnemyActionRule`;
  returns `MISSING_COMBAT_RULE` while no rule is wired.
- `IEnemyActionRule`, `IEnemyDefenseRule` — policy seams. `EnemyAction`, `EnemyActionType`.

`Turn` / `Battle`
- `ITurnEngine` / `TurnEngine` — `StartPlayerTurn`/`EndPlayerTurn` over a `BattleState`;
  delegates start-of-turn effects and drawing to the (unimplemented) domain engines.
- `IBattleEngine` / `BattleEngine` — full battle lifecycle; orchestrates turn, initiative,
  card, enemy action, dodge/damage/critical and logging. **Not resolvable until its
  card/effect/turn dependencies exist** (bonus: `EnemyActionType` selection occurs via the
  action selector seam).
- `BattleContext` — immutable per-battle hand-off (`PlayerCharacter`, `Enemy`, `BattleState`).
  Created on demand; **not a DI service**.
- `BattleState`, `BattleResult`, `IBattleLogWriter` / `BattleLogWriter`, `BattleLogEntry`.

`Deck` / `Hand` / `Cards` / `Effects` (turn-boundary contracts — currently *unimplemented*)
- `DnDGame.Domain.Engine.Deck.IDeckEngine` — `DrawCardsForPlayerTurn(state)`.
- `DnDGame.Domain.Engine.Hand.IHandEngine` — `DiscardHand(state)`.
- `DnDGame.Domain.Engine.Cards.ICardEngine` — `PlayCard(BattleContext, Engine.Models.CardInstance)`.
- `DnDGame.Domain.Engine.Effects.IEffectEngine` — `ApplyStartOfTurnEffects` / `ApplyEndOfTurnEffects`.

`Effects` model (implemented, P1)
- `ActiveEffect` — abstract effect descriptor (`Type`, `Value`, `Duration`, `StackCount`,
  `EffectTarget`) with `ConsumeDuration()`/`SetStackCount()`; the turn engine decrements its
  own lifetime independently of concrete types.
- Concrete effects: `BurnEffect`, `PoisonEffect`, `DefenseUpEffect`, `StrengthEffect`,
  `VulnerableEffect`, `WeakEffect`, plus `StatModifierEffect` base.

`Models` (P1)
- `Engine.Models.CardInstance` — minimal battle placeholder (`InstanceId`, `CardId`). Distinct
  from `Entities.Cards.CardInstance` (see §13, assumption 7).
- `ActiveEffect`, `BattleLogEntry`.

`Common` (P1)
- `EngineResult<T>` — domain result type (see §7).
- `EngineErrorCodes` — stable string code constants (see §8).

---

## 4. DnDGame.BusinessLayer

### 4.1 Card-battle engines (Persoana 2)

Implemented in `/Engines`, contracted in `/Engines/Interfaces`.

| Interface | Responsibility | Implementation |
|-----------|----------------|----------------|
| `IDeckEngine` | Battle-deck lifecycle: `CreateBattleDeck`, `ShuffleDeck`, `DrawCard`/`DrawCards` (auto-reshuffle from discard), `DiscardCard`, `ReshuffleDiscardPile`. | `DeckEngine` |
| `IHandEngine` | Hand management: `AddCard(s)` (respects `HandRules`), `RemoveCard`, `FindCard`/`FindCardByDefinitionId`, `DiscardHand`, `GetHandSize`. Returns `EngineResult`. | `HandEngine` |
| `IPlayEngine` | Play validation **without** state change: battle active, player turn, card in hand, energy, valid target. Also defines `ICardTarget` (`TargetId`, `TargetType`). | `PlayEngine` |
| `IEffectEngine` | Applies a card's effects through `CardEffectRegistry`; `CalculateDamage` / `ApplyStatusEffect`. | `EffectEngine` |
| `ICardEngine` | Orchestrates a full play: validate → consume energy → remove from hand → apply effects → discard. Atomic (no state change on invalid play). | `CardEngine` |
| `IAbilityUseEngine` | Validates ability/spell requirements (unlock, class, level) and pays the resource cost via `AbilityUseContext.Consume()`; returns `AbilityUseResult`. Effect application is deferred to the battle system. | `AbilityUseEngine` |

`CardEngine` flow (documented in its XML comments): `IPlayEngine.CanPlayCard` → energy
consumption → hand removal → `IEffectEngine.ApplyCardEffect` → discard. The hand is emptied
through `IDeckEngine.DiscardCard` (a past double-removal bug was fixed by removing the
`_handEngine` field; the constructor parameter is kept for compatibility).

### 4.2 Effects (Persoana 2) — Strategy pattern

| Type | Responsibility |
|------|----------------|
| `ICardEffect` | Strategy contract: `EffectName`, `CanApply(CardEffectContext)`, `Apply(CardEffectContext) -> string`. |
| `CardEffectContext` | Rich context handed to strategies: `Battle`, `PlayedCard`, `PlayerBattleDeck`, `PlayerId`, `CardDefinition`, `ICardTarget?`, optional `IHandEngine`/`IDeckEngine`/`HandRules`, and `ActiveEffects` (P1 effect sink). Exposes `EffectiveCardCost`/`EffectiveCardDamage`, `HasTarget`, `TargetsSelf`, `TargetsArea`. Must target the `Domain.Engine.Models.ActiveEffect` type via alias. |
| `CardEffectRegistry` | Name-keyed registry of `ICardEffect` strategies; add strategies without touching the engine (Open/Closed). |
| `ActiveEffectCardEffect<TActiveEffect>` | Abstract base for strategies that create turn-boundary effects (`DefenseUp`, `Strength`, `Vulnerable`, `Weak`); concrete strategies supply only an effect factory + duration. |
| `DamageEffect` | Deals damage via the injected **BusinessLayer** `IDamageCalculator`. `CanApply` requires damage value + valid target; area/single-target handling. **Currently uses placeholder `playerStrength = 10`, `enemyDefense = 0`.** |
| `HealEffect` | Heals (respects max-health cap). |
| `DrawEffect` | Draws cards (hand-size aware). |
| `EnergyEffect` | Restores/spends energy. |
| `BlockEffect` | Adds block. |
| `DefenseUpCardEffect`, `StrengthCardEffect`, `VulnerableCardEffect`, `WeakCardEffect` | Concrete active-effect strategies (duration defaults to 1 turn). |

`Effects/Interfaces/IDamageCalculator` — **the BusinessLayer damage contract**
(`CalculateDamage(int baseDamage, int playerStrength, int enemyDefense, int enemyBlock)`).
There is **no production implementation** — `DamageEffect` depends on it and it is the
blocker that keeps `CardEffectRegistry`/`IEffectEngine`/`ICardEngine` from resolving in the
full personas architecture today (see §10). It is documented as "Implementation provided by
Persoana 1" in the source.

### 4.3 Models (Persoana 2)

| Type | Responsibility |
|------|----------------|
| `EngineResult<T>` / `EngineResult` | BusinessLayer result type (see §7). |
| `CardPlayResult` | Play outcome: played card, `EnergyCost`, `RemainingEnergy`, effects applied, `EffectDescription`, `PlayedAt`. |
| `AbilityUseContext` | Input + state for `UseAbility` (`AvailableResource`, `Target`, internal `Consume()`). |
| `AbilityUseResult` | Record: `ResourceCost`, `RemainingResource`, `TargetType`, `EffectType`. |

### 4.4 Repositories (interfaces: baseline Phase 2; `DeckValidator`: Persoana 2)

`Repositories/Interfaces`: `IEnemyRepository`, `IRaceRepository`, `IClassRepository`,
`ICharacterRepository`, `ICharacterPortraitRepository`, `IAdventureRepository`,
`ITalentRepository`, `IGameSessionRepository`, `IStoryNodeRepository`, and `IDeckValidator`.

`Repositories/DeckValidator` — validates a deck's card composition against `DeckRules`;
returns `EngineResult` with `ErrorCode` values like `DECK_TOO_SMALL`, `DECK_TOO_LARGE`,
`CARD_COPY_LIMIT_REACHED`.

### 4.5 API infrastructure contracts (Persoana 3)

| Type | Responsibility |
|------|----------------|
| `Common/Errors/ErrorCodes` | Generic infra-level string codes: `VALIDATION_ERROR`, `NOT_FOUND`, `CONFLICT`, `INTERNAL_ERROR`. |
| `Common/Errors/IErrorCodeHttpMapper` + `ErrorCodeHttpMapper` | Maps a string `ErrorCode` to an HTTP status; additive `Register(code, status)`; unknown → 500 by design. |
| `Common/Exceptions/DomainException` | Expected business-level failure carrying a string `ErrorCode`; consumed by the middleware. |
| `Dtos/Common/ApiErrorResponse` | Single error response shape (`Success=false`, `ErrorCode`, `Message`); never leaks internals. |
| `Dtos/Common/PagedRequest` / `PagedResult` | Pagination request (`Page`, `PageSize`, `MaxPageSize`, `Normalize()`) and paged response container. |
| `Validation/ValidationResult` + `RequestValidationHelpers` | Feature-agnostic request-shape checks: `RequireNonEmpty`, `RequireValidGuid`, `RequirePositiveId`, `ValidatePagination`. |
| `Services/Interfaces/ICurrentPlayerService` | Resolves the current player for a request context. |

---

## 5. DnDGame.MockData

| Type | Responsibility |
|------|----------------|
| `MockDataBootstrapper` | Creates a fully seeded `InMemoryGameDataStore` (P3). |
| `InMemoryGameDataStore` | Singleton in-memory data store shared by every `Mock*` repository (state survives between requests). |
| `SeedData/GameDataSeeder` | Runs the individual seeders (baseline + card catalogue). |
| `SeedData/CardSeedData` | The 60-card catalogue (weapons, artifacts, consumables/potions) as plain `Card` definitions — no repository. (P2) |
| `SeedData/{Race,Class,Portrait,Talent,Enemy,Adventure}SeedData` | Baseline Phase 2 catalogue data. |
| `Repositories/Mock*Repository` | All nine repositories implementing the BusinessLayer interfaces against the store. |
| `Repositories/GraphHydrator` | Rebuilds entity navigation graphs after store loads. |
| `Services/MockCurrentPlayerService` | `ICurrentPlayerService` returning the seeded current player. (P3) |

---

## 6. DnDGame.API

| File | Responsibility |
|------|----------------|
| `Program.cs` | Thin entry point: controllers, Swagger, the three DI extensions (§9), middleware, `MapControllers`. `partial class Program` exposes a public type for test hosting. |
| `CompositionRoot/DependencyInjection.cs` | Single composition root, split into one extension method per owner (see §9). (P2) |
| `Middleware/ExceptionHandlingMiddleware.cs` | Global exception handling: `DomainException` → mapped status + `ApiErrorResponse`; anything else → logged fully server-side, generic `INTERNAL_ERROR` response only. Registered first in the pipeline. (P3) |
| `Middleware/ExceptionHandlingMiddlewareExtensions.cs` | `UseGlobalExceptionHandling()` helper. (P3) |
| `Controllers/PingController.cs` | `GET /api/ping` health endpoint. (P3) |

---

## 7. Result Pattern

There are **two** result types, one per layer side, intentionally kept separate until the
Persona 1/2 integration lands:

### BusinessLayer — `DnDGame.BusinessLayer.Models.EngineResult<T>`

```csharp
EngineResult<T> result = EngineResult<T>.Success(data);          // IsSuccess = true, ErrorCode = null
EngineResult<T> result = EngineResult<T>.Failure(ErrorCode.HAND_FULL, "Hand is full.");
//   IsSuccess = false, ErrorCode = HAND_FULL, ErrorMessage = "..."
```

- `IsSuccess`, `ErrorCode?` (**`Domain.Enums.ErrorCode` enum**), `ErrorMessage`, `Data`.
- Private constructor — callers must use the static factories `Success()` / `Failure(ErrorCode, string)`.
- A non-generic `EngineResult` exists for operations that return no data.

### Domain engine — `DnDGame.Domain.Engine.Common.EngineResult<T>`

```csharp
EngineResult<T> ok  = EngineResult<T>.Ok(data, message);
EngineResult<T> err = EngineResult<T>.Fail("...", EngineErrorCodes.NotPlayerTurn);
```

- `Success`, `Message`, `ErrorCode?` (**plain string**), `Data`.
- Sealed; factories `Ok(data, message = "")` / `Fail(message, errorCode)`.
- Intended for expected gameplay failures **without exceptions**; string codes come from
  `EngineErrorCodes`.

Both types are immutable after construction and skip exceptions for expected failures —
exceptions are reserved for genuinely unexpected conditions.

---

## 8. Error codes

Four surfaces exist, deliberately decoupled by layer:

| Surface | Location | Purpose | Examples |
|---------|----------|---------|----------|
| `ErrorCode` enum | `Domain/Enums/ErrorCode.cs` | Card-battle engine failures (BusinessLayer `EngineResult`) | `DECK_TOO_SMALL`, `HAND_FULL`, `CARD_NOT_IN_HAND`, `NOT_PLAYER_TURN`, `INSUFFICIENT_ENERGY`, `ABILITY_LOCKED`, `CLASS_REQUIREMENT_NOT_MET`, `LEVEL_REQUIREMENT_NOT_MET` (0–13) |
| `EngineErrorCodes` | `Domain/Engine/Common/EngineErrorCodes.cs` (P1) | Domain engine result codes (string constants) | `BATTLE_NOT_FOUND`, `NOT_PLAYER_TURN`, `MISSING_COMBAT_RULE`, `PLAYER_DEAD`, `CONSEQUENCE_ALREADY_APPLIED`, ... |
| `ErrorCodes` | `BusinessLayer/Common/Errors/ErrorCodes.cs` (P3) | Generic infrastructure codes | `VALIDATION_ERROR`, `NOT_FOUND`, `CONFLICT`, `INTERNAL_ERROR` |
| `DomainException.ErrorCode` | `BusinessLayer/Common/Exceptions/DomainException.cs` (P3) | Thrown by services for HTTP-bound failures; mapped by `ErrorCodeHttpMapper` | any registered string code |

### HTTP mapping

`ErrorCodeHttpMapper` (registered as a singleton) seeds:

| Code | HTTP status |
|------|-------------|
| `VALIDATION_ERROR` | 400 |
| `NOT_FOUND` | 404 |
| `CONFLICT` | 409 |
| `INTERNAL_ERROR` | 500 (also the fallback for any unregistered code) |

Mapping is **additive**: once Person 1/2 confirm their engine codes, a registration step can
call `Register(code, status)` (e.g. `BATTLE_NOT_FOUND → 404`, `NOT_PLAYER_TURN → 409`)
without touching the middleware or the interface.

Response shape (`ApiErrorResponse`, camelCase JSON):

```json
{ "success": false, "errorCode": "CARD_NOT_IN_HAND", "message": "Card is not available in the current hand." }
```

---

## 9. Dependency Injection

`Program.cs` calls three extensions grouped by owner in
`DnDGame.API/CompositionRoot/DependencyInjection.cs`.

### `AddCardBattleServices()` — Persoana 2

- **Config singletons** (placeholder values, move to `appsettings` when balances finalise):
  `DeckRules(min: 20, max: 40, maxCopies: 4)`, `HandRules(10)`,
  `PlayRules(maxEnergyPerTurn: 5, allowOverdraft: false)`, `CardVisibilityRules`.
- **Engines (scoped, per-request)**: `IDeckEngine → DeckEngine`, `IHandEngine → HandEngine`,
  `IPlayEngine → PlayEngine`, `IEffectEngine → EffectEngine`, `ICardEngine → CardEngine`,
  `IAbilityUseEngine → AbilityUseEngine`.
- `IDeckValidator → DeckValidator` (scoped), `IErrorCodeHttpMapper → ErrorCodeHttpMapper` (singleton).
- **All 9 `ICardEffect` strategies registered** (scoped), then a singleton
  `CardEffectRegistry` built from `provider.GetServices<ICardEffect>()` — new strategies are
  picked up by the container without editing the registry.

### `AddBattleTurnSystemServices()` — Persoana 1 (what is implemented)

- Dice: `IRandomNumberSource → CryptographicRandomNumberSource` (singleton),
  `IDiceEngine → DiceEngine` (singleton).
- Combat: `IDamageRule → AdditiveDamageRule`, `DomainDamageCalculator →
  DamageCalculator` (the **domain** contract, aliased to avoid name clash), `IDodgeCalculator`,
  `ICriticalCalculator` (singletons).
- Orchestration: `IInitiativeEngine`, `ISavingThrowEngine`, `IEnemyActionSelector`,
  `IBattleLogWriter` (singletons).
- Policy seams (`IDodgeRule`, `IInitiativeRule`, `IEnemyActionRule`, `IEnemyDefenseRule`)
  are only wired if implementations exist.

### `AddMockData()` — mock-data phase

- One shared `InMemoryGameDataStore` singleton (from `MockDataBootstrapper.CreateSeededStore()`).
- All nine `Mock*` repositories (scoped) + `MockCurrentPlayerService` (scoped).

### Container behavior

The container is intentionally **lazy** (no `ValidateOnBuild`): anything whose dependency
graph Persoana 1 has not completed still lets the app **boot** — it only throws if actually
resolved. Every known gap is marked as a **"Person 1 seam"** in the composition root rather
than implemented or stubbed there.

---

## 10. Integration with Persoana 1 (battle / turn systems)

Hand-off contract between the two layers:

```
P2 card flow  →  Battle / BattleDeck / Entities.Cards.CardInstance
                         │
                         ▼
       BattleContext (PlayerCharacter, Enemy, BattleState)   ← created on demand
                         │
                         ▼
       P1 engines: Initiative → Turn → (Card → Combat → EnemyAction) → log
```

**Registered because implemented on `catalina`**: dice, damage/dodge/critical calculators,
initiative, saving throws, enemy action selector, battle log writer.

**Person 1 seams — deliberately NOT registered/implemented here:**

1. `DnDGame.BusinessLayer.Effects.Interfaces.IDamageCalculator` — consumed by `DamageEffect`.
   The moment it is registered, `CardEffectRegistry`/`IEffectEngine`/`ICardEngine` resolve
   (verified by `DiRegistrationTests`). `DamageEffect` currently falls back to placeholder
   stats (`strength = 10`, `defense = 0`) and cannot be resolved until P1 supplies it.
2. Domain turn-boundary engines `IDeckEngine`, `IHandEngine`, `ICardEngine`, `IEffectEngine`
   (under `DnDGame.Domain.Engine.{Deck,Hand,Cards,Effects}`). These are the concrete seams
   consumed by `TurnEngine` and `BattleEngine` (`DrawCardsForPlayerTurn`, `DiscardHand`,
   `PlayCard`, `ApplyStartOfTurnEffects`/`ApplyEndOfTurnEffects`).
3. `ITurnEngine` / `IBattleEngine` (implemented classes exist but depend on the seams above,
   so they are not resolvable/registered yet).
4. `BattleContext` — a per-battle data holder created on demand; explicitly **not** a DI service.
5. Policy rules (`IEnemyActionRule`, `IEnemyDefenseRule`, `IDodgeRule`, `IInitiativeRule`) —
   optional; `EnemyActionSelector` returns `MISSING_COMBAT_RULE` until one is wired.

Naming note: two `IDamageCalculator` contracts exist. The composition root disambiguates with
`using DomainDamageCalculator = DnDGame.Domain.Engine.Combat.IDamageCalculator;`.

---

## 11. Integration with Persoana 3 (API infrastructure)

P3's merged work (`alexandru`, PR #4) provides the HTTP-facing scaffolding that P2's logic
will plug into:

- `ExceptionHandlingMiddleware` + `UseGlobalExceptionHandling()` — the single catch-all:
  `DomainException` → `ApiErrorResponse`, else generic `INTERNAL_ERROR`. P2's engines return
  `EngineResult` (not exceptions), so controllers will translate failures into
  `DomainException` at the boundary, or the mapping grows via `Register()`.
- `ApiErrorResponse` / `PagedRequest` / `PagedResult` — response + pagination contracts.
- `ErrorCodes` + `IErrorCodeHttpMapper`/`ErrorCodeHttpMapper` — HTTP code registry that P2/P1
  codes plug into additively.
- `RequestValidationHelpers`/`ValidationResult` — request-shape checks reusable by future
  controllers; deeper business rules stay in services/engines.
- `ICurrentPlayerService` / `MockCurrentPlayerService` — request-level current-player access.
- `MockDataBootstrapper` — seeds the store P2's card catalogue extends (`CardSeedData`).

No controllers beyond `PingController` exist yet; per the task contract, P2 does not create
controllers.

---

## 12. Tests

`tests/DnDGame.Tests` — xUnit, `ImplicitUsings` + `global using Xunit`. **229/229 passing**,
solution builds with 0 warnings / 0 errors. `DnDGame.Tests.csproj` references the Domain,
BusinessLayer, MockData and API projects (and `Microsoft.Extensions.DependencyInjection`
for the DI tests).

| Area | Test files |
|------|-----------|
| DI composition root | `API/DiRegistrationTests.cs` (29) — P2 engines resolve, all 9 effects registered, chain resolves with a stub damage calculator, seam failure pinned, P1 implemented components + mock data resolve. |
| BusinessLayer engines | `BusinessLayer/DeckAndHandEngineTests.cs`, `PlayAndCardEngineTests.cs`, `CardEffectStrategyTests.cs`, `Effects/Strategies/ActiveEffectCardEffectTests.cs`, `Engine/AbilityUseEngineTests.cs`. |
| Card domain | `Domain/Cards/CardCollectionTests.cs`, `CardSearchTests.cs`, `CardVisibilityTests.cs`, `MockData/CardSeedDataTests.cs`. |
| P1 engine | `Engine/Dice/DiceEngineTests.cs`, `Engine/Combat/{DamageCalculator,DodgeCalculator,CriticalCalculator}Tests.cs`, `Engine/Initiative/InitiativeEngineTests.cs`, `Engine/SavingThrows/SavingThrowEngineTests.cs`, `Engine/Turn/TurnEngineTests.cs`, `Engine/Enemy/EnemyTurnTests.cs`, `Engine/Battle/{BattleStatus,BattleLog}Tests.cs`, `Engine/Effects/ActiveEffectTests.cs`, `Engine/Common/EngineResultTests.cs`. |
| Infrastructure (P3) | `Infrastructure/{ErrorCodeHttpMapper,RequestValidationHelpers,PagedRequestAndResult,MockCurrentPlayerService,MockEnemyRepositoryReuse}Tests.cs`. |

---

## 13. Assumptions & design decisions

1. **Two `IDamageCalculator` contracts** are intentionally distinct: the BusinessLayer one
   (raw numeric inputs, used by `DamageEffect`) is a P1 TODO; the Domain one
   (`DamageRequest`-based) is implemented. The alias in the composition root keeps both usable.
2. **Placeholder combat stats**: `DamageEffect` currently assumes `playerStrength = 10`,
   `enemyDefense = 0`; real wiring comes from `PlayerCharacter`/`Enemy` via P1's calculator.
3. **Player model pending**: `PlayerCharacter` is used as "the player" for unlocks/abilities;
   the player-vs-player-character distinction is not finalised.
4. **Config values are placeholder tunings**: `DeckRules(20,40,4)`, `HandRules(10)`,
   `PlayRules(5, no overdraft)` and the visibility presets should move to `appsettings` once
   balancing is finalised. The parameterless ctors exist for DI/serialization but are not
   used for runtime defaults.
5. **Lazy container is deliberate**: unresolved Person 1 seams throw only at resolve-time,
   never at startup; each is documented in `AddBattleTurnSystemServices`.
6. **`BattleContext` is not a DI service**: it is a per-battle value assembled by whichever
   layer initiates a battle, then handed to `IBattleEngine`.
7. **Two `CardInstance` types coexist**: `Entities.Cards.CardInstance` (P2 runtime instances
   with modifiers) vs `Engine.Models.CardInstance` (P1 battle-facing placeholder). Reconciliation
   is part of the P1/P2 integration.
8. **`CardEngine` atomicity**: an invalid play changes nothing; the hand is emptied via
   `DeckEngine.DiscardCard` (a past double-removal bug is fixed — the `_handEngine` field was
   removed, the ctor parameter retained for compatibility).
9. **`DrawEffect` semantics**: it currently uses `EffectiveCardDamage` (i.e. `BaseDamage`) as
   the number of cards to draw and clamps to free hand space; the `Cards`-count convention is
   a placeholder to revisit.
10. **Unknown error codes map to 500 on purpose** (`ErrorCodeHttpMapper`): an unregistered
    code is treated as a bug to register, not guessed at.
11. **`EnemyActionSelector` returns `MISSING_COMBAT_RULE`** until a concrete `IEnemyActionRule`
    is supplied.
12. **Mock data is catalogue-only for cards**: `CardSeedData` defines 60 cards with no backing
    repository; player collection/unlock is exercised via `CardCollection` in tests.
13. **No new controllers** in this slice (P2 contract); endpoints arrive with the controllers
    phase, consuming the engines via the DI container.

---

## 14. Person 3 follow-up: Dice, Card, Deck features

Added on `alexandru` after this document was written, per the remaining Person 3 scope:

- **Dice**: `DiceRequestDto`/`DiceResultDto`, `IDiceService`/`DiceService` (wraps `IDiceEngine`),
  `DiceController` (`POST /api/dice/roll`). `EngineErrorCodes.InvalidDice` mapped to 400.
- **Card**: `ICardCollectionRepository`/`MockCardCollectionRepository` (lazily creates an empty
  `CardCollection` per player character), `CardDetailsDto`/`CardSearchRequestDto`,
  `ICardService`/`CardService` (thin wrapper over `CardCollection.SearchCards`/`GetCardDetails`,
  no filtering/sorting logic duplicated), `CardController` (`GET /api/card`, `GET /api/card/{id}`).
- **Deck**: `IDeckRepository`/`MockDeckRepository` (CRUD + catalogue card lookup by id — no
  dedicated card-catalogue repository exists yet, so lookup lives here), `DeckResponseDto`/
  `DeckSaveRequestDto`/`DeckValidationResultDto`, `IDeckService`/`DeckService` (ownership-checked
  CRUD over the current player's decks, plus validation via the existing `IDeckValidator`),
  `DeckController` (`GET/POST /api/deck`, `GET/PUT/DELETE /api/deck/{id}`,
  `POST /api/deck/{id}/validate`). `DECK_TOO_SMALL`/`DECK_TOO_LARGE`/`CARD_COPY_LIMIT_REACHED`
  mapped to 400.

None of the §10 Person 1 seams were touched. `InMemoryGameDataStore` gained two additive lists
(`CardCollections`, `Decks`); no existing store field was removed or restructured. Test count
grew from 229 to 262 (see individual PRs for the breakdown).

## 15. Person 3 follow-up: Battle data-access layer

Added on `alexandru`, ahead of the Battle API (`BattleService`/`BattleController` not yet
built): `IBattleRepository`/`MockBattleRepository` and `IBattleDeckRepository`/
`MockBattleDeckRepository`, following the existing `Mock*Repository` pattern.
`InMemoryGameDataStore` gained two additive lists (`Battles`, `BattleDecks`). `BattleDeck`
gained a `BattleId` FK (1:1 — one battle has exactly one battle deck) since none existed
before, matching the `BattleDeck.DeckId`/`Deck.CharacterId` scalar-FK style already used
elsewhere in this file — no navigation property was added, consistent with `Deck.CharacterId`
having none either. `Battle` was left unchanged; the relationship only needs a FK on one side.
`BattleState`'s `ActiveEffects`, structured `BattleLog`, `RewardsGranted`, and its
PlayerTurn/EnemyTurn `BattleStatus` still had no persisted equivalent on `Battle`/`BattleDeck`
at this point — closed out in §16 below.

## 16. Person 3 follow-up: Battle API (complete)

Added on `alexandru`, completing the Battle API on top of §15's repositories. The Battle
data model, DTOs, service, and controller are now fully built and wired — see §7's conflict
table and PROJECT_CONTEXT.md's original blocker list, both now superseded by this section.

**Battle/BattleDeck persistence gaps closed** (all additive, reusing existing Engine types —
no new types invented):
- `Battle.RewardsGranted` (`bool`) — mirrors `BattleState.RewardsGranted`.
- `Battle.CurrentTurn` (`Domain.Engine.Enums.TurnType`) — turn ownership, reused as-is from
  the Engine. Kept distinct from `Battle.Status` (`GameSessionStatus`): `Status` is the
  terminal outcome (in progress/victory/defeat/abandoned), `CurrentTurn` is whose turn it is
  — two different concerns that happened to be conflated into one enum
  (`Domain.Engine.Enums.BattleStatus`) on the transient `BattleState` side.
- `Battle.BattleLog` (`List<Domain.Engine.Models.BattleLogEntry>`) and `Battle.ActiveEffects`
  (`IList<Domain.Engine.Models.ActiveEffect>`) — both already-public Engine types, reused
  directly rather than duplicated.
- `Battle.EnemyId` (+ `Enemy?` nav) — `Battle` previously had `EnemyCurrentHealth`/`EnemyBlock`
  but no link to *which* `Enemy`. Resolved once at `StartBattle` from the game session's
  current `StoryNode.EnemyId`/`Choice.EnemyId` and persisted, so later requests
  (`GetBattleState`/`PlayCard`/`EndTurn`) can re-resolve the same `Enemy` without re-walking
  the story graph.

**DTOs** (`BusinessLayer/Dtos/Battles/`): `CardInstanceDto`, `ActiveEffectDto`,
`BattleLogEntryDto` (reuses the existing `DiceResultDto` for its nested dice roll),
`BattleStateDto` (composed from **both** `Battle` and `BattleDeck` — see §15 for why both
models exist; `DrawPile`/`DiscardPile` exposed only as counts, not contents), `StartBattleRequestDto`
(`GameSessionId` + `DeckId` — no `EnemyId`, since the enemy is resolved from the story graph,
not chosen by the client), `PlayCardRequestDto` (`CardInstanceId` only — `IBattleEngine.PlayCard`
takes no target parameter).

**`IBattleService`/`BattleService`** (`BusinessLayer/Services/`): `StartBattleAsync`,
`GetBattleStateAsync`, `PlayCardAsync`, `EndTurnAsync`, `GetBattleLogAsync`. Reuses
`BusinessLayer.Engines.IDeckEngine.CreateBattleDeck` (Persoana 2's, already implemented)
rather than reimplementing battle-deck construction. Two private helpers,
`BuildState`/`ApplyState`, are the sole two-way translation between persisted
(`Battle`+`BattleDeck`) and runtime (`BattleContext`/`BattleState`) — every mutating method
routes through them, so the reconciliation logic exists in exactly one place. `EndTurnAsync`
auto-chains into `ExecuteEnemyTurn` when the turn passes to the enemy, since there is no
separate endpoint for the client to trigger that step. `IBattleEngine`'s `EngineResult`
failures convert straight to `DomainException(result.ErrorCode, result.Message)` at the
boundary (its codes are already strings, unlike Persona 2's enum-coded `EngineResult`).

**`BattleController`** (`API/Controllers/`): `POST /api/battle/start`,
`GET /api/battle/{battleId}`, `POST /api/battle/{battleId}/play-card`,
`POST /api/battle/{battleId}/end-turn`, `GET /api/battle/{battleId}/log`. Thin, same shape as
`DiceController`/`DeckController`.

**HTTP error mapping**: `Domain.Engine.Common.EngineErrorCodes` — the codes `IBattleEngine`'s
`EngineResult` failures actually carry — registered in `AddCardBattleServices()`'s
`IErrorCodeHttpMapper` alongside the Dice/Deck codes: `BattleNotFound`→404,
`BattleAlreadyFinished`→409, `NotPlayerTurn`→409, `InvalidAction`→400, `PlayerDead`→409,
`EnemyDead`→409, `MissingCombatRule`→500, `RewardsAlreadyGranted`→409.
`ConsequenceAlreadyApplied` (also in `EngineErrorCodes`) was deliberately left unregistered —
it belongs to the unrelated adventure/story-node flow, not anything this feature produces.

**Was a Person 1 seam, now closed**: at the time this section was written,
`Domain.Engine.Battle.IBattleEngine` was *not* registered in
`AddBattleTurnSystemServices()` because its constructor requires
`Domain.Engine.{Cards,Deck,Hand,Effects}` implementations and `IEnemyDefenseRule`, none of
which existed on this branch yet (see §10). `BattleService`/`BattleController` were real,
complete code, not stubs — they simply didn't resolve/function until Persona 1 delivered
those seams. Persona 1 landed all five (`DeckEngine`, `HandEngine`, `CardEngine`,
`EffectEngine`, `EnemyDefenseRule`) on `catalina` (commit `1e2724d`, "Implement battle API",
merged via PR #10), so `IBattleEngine`/`ITurnEngine`/`IBattleService` now resolve from the
composition root. The pinning test was renamed and flipped accordingly:
`DiRegistrationTests.Persona1_Seam_BattleServiceResolvesNowThatBattleEngineLands`, same
convention as the still-open `IEffectEngine` seam test (see below).

**Tests**: `BattleServiceTests` (service-layer orchestration, using a hand-written
`IBattleEngine` test double — same technique as `DiRegistrationTests.StubDamageCalculator`,
kept even after the real engine became resolvable so the test doesn't depend on Persona 1's
internals), plus `MockBattleRepositoryTests`/`MockBattleDeckRepositoryTests` and additional
`DiRegistrationTests`/`ErrorCodeHttpMapperTests` cases. No controller-level tests were added —
there's no precedent for that in this codebase (`DiceController`/`DeckController` have none
either); coverage lives entirely at the service layer the controller thinly wraps. Test count
grew from 262 (§14) to 297 at the time this section was written.

## 17. Update: Person 1 battle-engine seam closed

As of `main` commit `62422f2` (merge of PR #10, `catalina` → `main`), the seam described
above and in §10 is closed: `Domain.Engine.{Deck,Hand,Cards,Effects}` and `IEnemyDefenseRule`
are implemented and registered in `AddBattleTurnSystemServices()`, so `IBattleEngine`,
`ITurnEngine`, and (in turn) `IBattleService`/`BattleController` all resolve end-to-end. This
supersedes the "not yet resolvable" framing in §10 point 2–3 and §16 above — no further
Person 3 action was needed, the existing `BattleService`/`BattleController` code just started
working. Full solution test count is now 359/359 passing (`dotnet build`: 0 warnings/0 errors).

**Still open, and distinct from the seam above** — do not conflate the two: the
*BusinessLayer* `Effects.Interfaces.IDamageCalculator` (consumed by `DamageEffect`, part of
Persona 2's card-effect chain) remains unregistered. `CardEffectRegistry`/`IEffectEngine`/
`ICardEngine` **on the BusinessLayer side** still won't resolve until Persona 1 supplies it;
`DamageEffect` still uses placeholder stats (`strength = 10`, `defense = 0`). Pinned by
`DiRegistrationTests.Persona1_Seam_EffectEngineNotResolvableUntilDamageCalculatorLands`,
unchanged.