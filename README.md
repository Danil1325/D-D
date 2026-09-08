# D&D University Web Game — Backend

Single-player, D&D-inspired web game. React + TypeScript frontend (not built yet),
ASP.NET Core Web API backend, PostgreSQL (introduced in Phase 6), layered architecture.

See `DnD-Game-Backend-Plan.md` for the full design document — architecture rationale,
entity list, enums, dice/combat rules, story/choice system, and the complete phase-by-phase
roadmap. This README only tracks where the code currently stands.

## Current status: Phase 1 complete (Domain models and enums)

- [x] Phase 0 — Solution & project scaffolding
- [x] Phase 1 — Domain models and enums
- [ ] Phase 2 — Mock data
- [ ] Phase 3 — Business/game logic
- [ ] Phase 4 — API / controllers
- [ ] Phase 5 — Full backend test pass via mock data + Swagger
- [ ] Phase 6 — DataAccessLayer + EF Core + PostgreSQL
- [ ] Phase 7 — Replace mock data with database repositories
- [ ] Phase 8 — Database-backed test pass
- [ ] Phase 9 — React frontend
- [ ] Phase 10 — Connect frontend to API
- [ ] Phase 11 — Final testing / polish

## Solution layout

```
DnDGame.sln
├── src/
│   ├── DnDGame.Domain/          Entities + enums. Plain C#, zero dependencies. (Phase 1 ✓)
│   ├── DnDGame.BusinessLayer/   Services, DTOs, game rules, repository interfaces.
│   ├── DnDGame.MockData/        In-memory implementations of those interfaces (Phases 2-5).
│   └── DnDGame.API/             ASP.NET Core Web API — controllers, Program.cs, Swagger.
└── tests/
    └── DnDGame.Tests/           xUnit tests for BusinessLayer.
```

`DnDGame.Domain` currently contains 15 entities and 7 enums — see
`DnD-Game-Backend-Plan.md` for the full breakdown, or just browse
`src/DnDGame.Domain/Entities/` and `src/DnDGame.Domain/Enums/` directly.

`DnDGame.DataAccessLayer` is added in Phase 6 as a sibling to `MockData`, implementing the
same `BusinessLayer` interfaces via EF Core + PostgreSQL. See the plan document's
"mock-to-database swap" diagram for why the project is shaped this way.

## Running it locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet restore
dotnet build
dotnet run --project src/DnDGame.API
```

This opens Swagger at `/swagger`, where you'll currently find one endpoint —
`GET /api/ping` — just to confirm the host is running. Real endpoints arrive in Phase 4.

```bash
dotnet test
```

Runs the (currently placeholder) test project.
