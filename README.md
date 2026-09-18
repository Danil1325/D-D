# D&D University Web Game — Backend

Single-player, D&D-inspired web game. React + TypeScript frontend (separate repo,
`TheCrownofAsh_FrontEnd`), ASP.NET Core Web API backend, PostgreSQL (introduced in
Phase 6), layered architecture.

See `docs/ARCHITECTURE.md` for the full component reference — every class/interface,
the DI composition root, the Result Pattern, error-code surfaces, and the current
Person 1/2/3 integration state. This README only tracks where the code currently stands.
(`DnD-Game-Backend-Plan.md`, the original design doc these phases were named after, is
no longer in the repo.)

## Current status: Phase 4 in progress (API / controllers)

- [x] Phase 0 — Solution & project scaffolding
- [x] Phase 1 — Domain models and enums
- [x] Phase 2 — Mock data
- [x] Phase 3 — Business/game logic
  - Card-battle engines (deck/hand/play/card/effect/ability-use), combat
    calculators (damage/dodge/critical), dice, initiative (`AdditiveInitiativeRule`),
    enemy AI (`WeightedEnemyActionRule`), turn/battle orchestration, and deck
    validation are all implemented and wired into DI. A full battle
    (start → play a card → end turn → resolve the enemy's turn) completes
    end-to-end through the real engine.
  - Card damage on the live path (`Domain.Engine.Cards.CardEngine`, used by
    `BattleService`) now uses the player's real `Strength` and the enemy's real
    `Defense`/`AttackBonus` instead of hardcoded zeros. Note: the separate
    BusinessLayer `ICardEngine`/`DamageEffect`/`AdditiveDamageCalculator` chain
    is real and tested but is **not** on this live path — see
    `docs/ARCHITECTURE.md` §18 for why two independent card-battle systems
    exist and which one the API actually uses.
- [ ] Phase 4 — API / controllers
  - Done: Dice, Card, Deck, and Battle controllers, each with DTOs, request
    validation, and error-code-to-HTTP mapping through the shared middleware.
  - Remaining: no controllers yet for Character, Adventure/story-graph, or
    GameSession.
- [ ] Phase 5 — Full backend test pass via mock data + Swagger
- [ ] Phase 6 — DataAccessLayer + EF Core + PostgreSQL
- [ ] Phase 7 — Replace mock data with database repositories
- [ ] Phase 8 — Database-backed test pass
- [ ] Phase 9 — React frontend
- [x] Phase 10 — Connect frontend to API *(partial — see Authentication below;
  the rest of the frontend/API integration is still pending)*
- [ ] Phase 11 — Final testing / polish

**Out-of-band addition — Authentication (cookie-based)**: not part of the original
phase list above, but implemented end-to-end (backend + frontend) ahead of it. See
the "Authentication" section below and `docs/ARCHITECTURE.md` §19.

## Solution layout

```
DnDGame.sln
├── src/
│   ├── DnDGame.Domain/          Entities, enums, configuration, and the game engine
│   │                            (dice, combat, initiative, saving throws, enemy
│   │                            actions, turn/battle orchestration, effects).
│   │                            Plain C#, zero external dependencies.
│   ├── DnDGame.BusinessLayer/   Card-battle engines (deck/hand/play/card/effect/
│   │                            ability-use), card-effect strategies, DTOs,
│   │                            application services, repository interfaces,
│   │                            validation, error handling.
│   ├── DnDGame.MockData/        In-memory data store, seed data, Mock* repositories,
│   │                            current-player service. No database yet.
│   └── DnDGame.API/             ASP.NET Core Web API — controllers, DI composition
│                                 root, middleware, Program.cs, Swagger.
└── tests/
    └── DnDGame.Tests/           xUnit test suite (401 tests as of this writing).
```

`DnDGame.Domain` currently contains 26 entity classes and 23 enums. `DnDGame.MockData`
seeds 4 races, 4 classes, 16 portraits, 12 talents, 21 enemies, a 60-card catalogue,
and one small branching adventure ("The Whispering Crypt") exercising all 7
`ChoiceOutcomeType` values. See `docs/ARCHITECTURE.md` for the full breakdown, or
browse `src/DnDGame.Domain/` and `src/DnDGame.MockData/` directly.

`DnDGame.DataAccessLayer` is planned as a sibling to `MockData` for Phase 6, implementing
the same `BusinessLayer` repository interfaces via EF Core + PostgreSQL, so mock data can
be swapped out without touching the API or business layer.

## API surface

Controllers currently registered (see `docs/ARCHITECTURE.md` §6/§11 for DTOs and error
mapping):

- `GET /api/ping` — health check.
- `POST /api/dice/roll` — roll a die via the domain dice engine.
- `GET /api/card`, `GET /api/card/{id}` — search/browse the current player's card collection.
- `GET/POST /api/deck`, `GET/PUT/DELETE /api/deck/{id}`, `POST /api/deck/{id}/validate` — deck CRUD + validation.
- `POST /api/battle/start`, `GET /api/battle/{battleId}`, `POST /api/battle/{battleId}/play-card`,
  `POST /api/battle/{battleId}/end-turn`, `GET /api/battle/{battleId}/log` — battle lifecycle.
  The full flow now works end-to-end through the real engine (see `docs/ARCHITECTURE.md` §18).
- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`,
  `GET /api/auth/me` — cookie-based authentication (see "Authentication" below and
  `docs/ARCHITECTURE.md` §19).

No controllers exist yet for Character, Adventure/story-graph, or GameSession.

## Authentication

Cookie-based, via ASP.NET Core's built-in cookie authentication — deliberately not
JWT (see `docs/ARCHITECTURE.md` §19 for the reasoning: single first-party browser
client, no token storage/XSS exposure on the frontend, real server-side logout).

- `Account` (Domain), `IAccountRepository`/`MockAccountRepository`, and
  `IAccountService`/`AccountService` follow the same Entity → Repository → Service
  → Controller pattern as every other feature (`Deck`, `Card`, `Battle`).
- Passwords are hashed with `Microsoft.AspNetCore.Identity.PasswordHasher<Account>`
  (via `Microsoft.Extensions.Identity.Core`) — never stored in plaintext, never
  hand-rolled salting.
- The session cookie is `HttpOnly`, with environment-conditional `SameSite`/`Secure`
  (`Lax`/`SameAsRequest` in dev, `None`/`Always` otherwise), and CORS is restricted
  to an explicit, configuration-driven frontend origin with `AllowCredentials()` —
  required because the frontend lives in a separate repo/origin.
- **Deliberately unrelated to `ICurrentPlayerService`**: there is still no
  Account-to-`PlayerCharacter` link. `ICurrentPlayerService` (used by
  Deck/Card/Battle) is untouched and still resolves the mock fixed id — this was an
  explicit decision, not an oversight, to avoid inventing an account/character
  relationship no one has designed yet.
- The React frontend (`TheCrownofAsh_FrontEnd`, branch `LogareRegistrareCookies`)
  is already wired to all four endpoints via `src/api/authApi.ts`, including a
  session check on load (`GET /api/auth/me`) so a login survives a page refresh.

## Running it locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet build
dotnet run --project src/DnDGame.API
```

This opens Swagger at `/swagger` with the endpoints listed above.

```bash
dotnet test
```

Runs the full xUnit suite (401 tests, 0 failures as of this writing).
