\# DnDGame — Claude Code Instructions



\## Project



This is a university D\&D-style web game.



Architecture:



React/TypeScript frontend

→ ASP.NET Core Web API

→ BusinessLayer

→ Domain



MockData provides the current in-memory repositories/data.



There is currently no database.



\## Important architecture rules



\- Follow the existing project architecture.

\- Keep Domain independent from BusinessLayer, MockData, and API.

\- Use Dependency Injection rather than manually constructing services.

\- Keep controllers thin.

\- Put business/application logic in services or the appropriate existing engine.

\- Use DTOs at the API boundary.

\- Reuse existing validation, error handling, pagination, and logging infrastructure.

\- Do not duplicate logic that already exists in Domain or the Game Engines.



\## Team ownership



Person 1 owns the battle/game engine work.



Person 2 owns the card/deck/hand/effect engine work.



Person 3 owns the API/application-service layer.



Do not modify another person's engine implementation unless explicitly instructed.



\## Current Person 3 work



Person 3 is currently implementing:



\- repositories

\- application services

\- DTOs

\- controllers

\- validation

\- logging

\- API integration



See `PROJECT\_CONTEXT.md` for the detailed current state and remaining work.



\## Critical rule



Do not create fake, stub, or placeholder implementations of missing game-engine interfaces just to make the project compile.



If an implementation is blocked by another team's engine, stop and report the dependency instead of inventing an implementation.



\## Git



\- Never commit directly to `main`.

\- Work only on the current feature branch unless explicitly instructed otherwise.

\- Keep commits small and logically separated.

\- Before committing, run:

&#x20; - `dotnet build`

&#x20; - `dotnet test`

\- Do not push to `main`.

\- Do not rewrite or squash existing team history unless explicitly instructed.



\## Before making changes



For a new task:



1\. Read `PROJECT\_CONTEXT.md`.

2\. Inspect the relevant existing source code.

3\. Verify assumptions against the actual repository.

4\. Explain the first implementation step before making changes if requested by the user.



If the architecture is ambiguous and cannot be resolved from the code or project documentation, ask the user instead of silently choosing an architecture.

