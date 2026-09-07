# Loop Engineering — Memory File

- **Module:** Application Access & Main Navigation (M-01, backend only)
- **Module Number:** M-01
- **Source Plan:** Docs/OpenCode/M-01.md
- **Date Created:** 2026-09-07
- **Total Slices:** 2
- **Current Slice:** Not started — 0/2 slices done
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Implements the small M-01 backend surface (the Presentation layer — login window, status bar, navigation-menu wiring, lock-workstation UI — is out of scope for this plan and is the entire remaining M-01 surface for a future Presentation plan). S1 extends the existing `CheckDatabaseConnectivityQuery` to return a `CheckDatabaseConnectivityDto` (`bool IsConnected`, `string? ServerName`, `string? DatabaseName`, `DateTime CheckedAtUtc`) backed by a new `IDbConnectionDescriptor` port and an Infrastructure `SqlServerConnectionDescriptor` (redacts the password from the connection string); S1 also seeds the M-01 test fakes. S2 adds the only M-01-owned Application-layer write surface: `LockWorkstationCommand` — a thin in-memory `ClearSession` re-export gated on `EDIT_SYSTEM_SETTINGS` (id 10, the M17/M22 permission pattern). No new Domain mutators (M01's domain grouping `Users` is fully delivered in M17). No new migration. M01's tracking-sheet row is **not** flipped to "🟩 Done" by S1 + S2 alone — the Presentation layer is what M-01's "Done" criterion requires; the plan declares M-01 backend work complete.

## Global Validation Gates

- Gate G0 (pre-execution): `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- Gate G1 (post-execution per slice): same as G0 plus the slice-specific gate listed in the table below.

## Stop/Continue Rule

After a slice completes (all 10 stages done), verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice, with no pause and no human confirmation required.
If any one fails → retry. If the SAME failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs 5 CONSECUTIVE times, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do NOT proceed past this point without owner review.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: 5 consecutive failures for the same reason (not 4).
- Execution order: strictly sequential S1 -> S2, no parallel slices. **M-01 ships after M-15 — M-15's slices must have closed and the M-15 code must be deployed in the codebase before S1 starts** (this is the binding execution order for the sequence 13 → 15 → 1 → 2 → 21).
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/inspection) replaces any standard UI journey — M-01 has no UI in scope here.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-01] Slice N/Total: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-01's plan. M-01 backend is then "done" in the sense defined by the plan; the tracking-sheet flip for the M-01 row is a separate concern handled outside this plan (the M-01 module closure requires the out-of-scope Presentation layer).

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | M-01 S1: extended connectivity DTO + tests: build zero/zero; new tests cover connected / not connected / exception path / descriptor surfaces correct server and database / credentials redaction asserted (no `secret` substring in DTO); 529-test baseline green; diff touches only M-01's Application feature folder, Infrastructure (port + DI registration), and the test project; no Presentation; no other Domain; no M17/M22/M12/M14 files | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Application.Tests` |
| 2 | VG-02 | M-01 S2: `LockWorkstationCommand` + tests: build zero/zero; new tests cover handler calls `ClearSession` exactly once / does not touch `IApplicationDbContext` / does not throw on already-cleared session / returns `Result.Success()`; authorization test asserts `LockWorkstationCommand.RequiredPermissionCode == "EDIT_SYSTEM_SETTINGS"` and standard denial message; 529-baseline + S1 tests green; diff is local to M-01's feature folder + test project; no DI change (ICurrentUserService already registered) | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Application.Tests` |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | M-01 application: extended connectivity DTO and tests | [x] Done | VG-01 |
| 2 | M-01 application: `LockWorkstationCommand` and tests | [x] Done | VG-02 |

---

## Slice 1: M-01 application: extended connectivity DTO and tests

- **Goal:** Add the new `CheckDatabaseConnectivityDto` (with `bool IsConnected`, `string? ServerName`, `string? DatabaseName`, `DateTime CheckedAtUtc`) and update the `CheckDatabaseConnectivityQuery` response. Capture the server and database name from a new `IDbConnectionDescriptor` port. Establish the M-01 test fake set.
- **Touches:** `src/TopLab.Application/Features/AccessAndNavigation/Common/ConnectivityDto.cs` (create — `public sealed record CheckDatabaseConnectivityDto(bool IsConnected, string? ServerName, string? DatabaseName, DateTime CheckedAtUtc);`); `src/TopLab.Application/Features/AccessAndNavigation/Common/Interfaces/IDbConnectionDescriptor.cs` (create — `public interface IDbConnectionDescriptor { string? ServerName { get; } string? DatabaseName { get; } }`); `src/TopLab.Application/Features/AccessAndNavigation/Queries/CheckDatabaseConnectivity/CheckDatabaseConnectivityQuery.cs` (modify — change response type from `IRequest<Result<bool>>` to `IRequest<Result<CheckDatabaseConnectivityDto>>`); `src/TopLab.Application/Features/AccessAndNavigation/Queries/CheckDatabaseConnectivity/CheckDatabaseConnectivityQueryHandler.cs` (modify — depend on `IDbConnectionDescriptor` + `IDateTimeProvider` in addition to `IApplicationDbContext`; catch `Exception` and return success DTO with `IsConnected=false` and the descriptor's server/database populated; success on `CanConnectAsync` true); `src/TopLab.Infrastructure/Persistence/SqlServerConnectionDescriptor.cs` (create — `public sealed class SqlServerConnectionDescriptor : IDbConnectionDescriptor`; parses `IConfiguration["ConnectionStrings:TopLab"]`; returns `Server=` and `Database=` segments with the password and any `Password=…;` segment redacted to `Password=***`); `src/TopLab.Infrastructure/DependencyInjection.cs` (modify — register `IDbConnectionDescriptor` as `Singleton`; `IWorkstationConnectionSettingsProvider` registration unchanged); `tests/TopLab.Application.Tests/Common/Fakes/FakeDbConnectionDescriptor.cs` (create — stub descriptor for tests); `tests/TopLab.Application.Tests/Features/AccessAndNavigation/CheckDatabaseConnectivityQueryHandlerTests.cs` (create — tests: connected, not connected, exception path, descriptor surfaces the right server/database, never surfaces the password)
- **Validation Gate:** VG-01 — build 0/0 over the full solution; new tests + 529-test baseline green; new tests cover success (DTO with `IsConnected=true` and the descriptor's server/database), failure (`CanConnectAsync` returns false -> DTO with `IsConnected=false` but descriptor still populated), exception (DBC throws -> DTO with `IsConnected=false`, no exception bubbles), credentials redaction (`Password=secret;` -> DTO has no `secret` substring anywhere); diff confined to M-01's feature folder, Infrastructure port+DI, test project; no Presentation; no other Domain; no M17/M22/M12/M14 files.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln` -> 0 warning, 0 error. `dotnet test TopLab.sln` -> 256 Domain + 549 Application + 70 Infrastructure = 875 tests, all passing.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §5.2 (M01-S1); the existing `IWorkstationConnectionSettingsProvider` returns the full connection string with credentials — the new `IDbConnectionDescriptor` is a smaller, redacted port; redaction rule: password and any `Password=…;` segment -> `Password=***`; the `CheckDatabaseConnectivityQuery` response type change from `IRequest<Result<bool>>` to `IRequest<Result<CheckDatabaseConnectivityDto>>` is the only public-shape change in this slice; the handler catches `Exception` and returns a `Result<...>.Success` DTO with `IsConnected=false` (no exception bubbles to the caller). Forced compile-fix at the existing Presentation consumer `ShellViewModel.cs:202` (one line) was needed to follow the documented public-shape change.
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `IApplicationDbContext` (read in full), existing `CheckDatabaseConnectivityQuery` + `CheckDatabaseConnectivityQueryHandler` (read in full), `IWorkstationConnectionSettingsProvider` (read — to be left unchanged), `IDateTimeProvider`, `Result/Error`, `FakeApplicationDbContext` + wrapping-fake pattern, `tests/TopLab.Application.Tests/Common/Fakes/`, `src/TopLab.Infrastructure/DependencyInjection.cs` (read — to be extended with the new port), `appsettings.json` (confirmed connection string shape: `Server=...;Database=...;...`), `ShellViewModel.cs:202` (existing Presentation consumer of the old `Result<bool>` shape — required adapter line for the documented public-shape change), `IAuthorizedRequest`, `PermissionConfiguration.cs:27` (EDIT_SYSTEM_SETTINGS at id 10), `FakeCurrentUserService`, `AuthorizationBehavior`.
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: (1) `ConnectivityDto.cs`; (2) `IDbConnectionDescriptor.cs`; (3) `CheckDatabaseConnectivityQuery.cs` signature change; (4) `CheckDatabaseConnectivityQueryHandler.cs` inject descriptor + datetime, catch Exception, return DTO; (5) `SqlServerConnectionDescriptor.cs` parse + redact; (6) `DependencyInjection.cs` Singleton registration; (7) `FakeDbConnectionDescriptor.cs`; (8) `CheckDatabaseConnectivityQueryHandlerTests.cs` (replacing the pre-existing `Result<bool>`-typed test that would otherwise fail to compile); (9) forced compile-fix at `ShellViewModel.cs:202` to follow the documented public-shape change.
- [x] **Stage 5 — Execution:** Slice implemented per plan. All 8 new files + 2 file modifications created.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`. Evidence: `dotnet build TopLab.sln` -> 0/0. `dotnet test TopLab.sln` -> 256 Domain + 553 Application (was 549; +4: replaced 1 pre-existing test with 5 new ones) + 70 Infrastructure = 879 tests, all passing.
- [x] **Stage 7 — Validation Gate:** VG-01 passed. Evidence: build/test output above; success / failure / exception / credentials-redaction / descriptor-surfaces-parsed-server-database cases all green.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (authorized local):** `[M-01] Slice 1/2: M-01 application: extended connectivity DTO and tests — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.` — on `main`, never push.

---

## Slice 2: M-01 application: `LockWorkstationCommand` and tests

- **Goal:** Add the `LockWorkstationCommand` — a thin in-memory command that clears the current session and gates on `EDIT_SYSTEM_SETTINGS` (id 10, the M17/M22 permission pattern). This is the only M-01-owned write surface. No new Domain mutator is needed (the in-memory state lives in `CurrentUserService`, which already has `ClearSession()`).
- **Touches:** `src/TopLab.Application/Features/AccessAndNavigation/Commands/LockWorkstation/LockWorkstationCommand.cs` (create — `public sealed record LockWorkstationCommand : IRequest<Result>, IAuthorizedRequest { public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"; }`); `src/TopLab.Application/Features/AccessAndNavigation/Commands/LockWorkstation/LockWorkstationCommandHandler.cs` (create — depends on `ICurrentUserService`, calls `_currentUser.ClearSession()`, returns `Result.Success()`); `src/TopLab.Application/Features/AccessAndNavigation/Common/AccessAndNavigationAccessPolicy.cs` (create — `public const string EditSystemSettings = "EDIT_SYSTEM_SETTINGS";`); `tests/TopLab.Application.Tests/Features/AccessAndNavigation/LockWorkstationCommandHandlerTests.cs` (create — handler calls `ClearSession` exactly once; does not touch `IApplicationDbContext`; does not throw on already-cleared session; returns `Result.Success()`); `tests/TopLab.Application.Tests/Features/AccessAndNavigation/AccessAndNavigationAuthorizationTests.cs` (create — `LockWorkstationCommand.RequiredPermissionCode == "EDIT_SYSTEM_SETTINGS"`; standard denial message returned when user lacks the permission; `IsAbsolutePermission = true` bypasses the gate; user with `EDIT_SYSTEM_SETTINGS` grant passes)
- **Validation Gate:** VG-02 — build 0/0; new tests + 529-baseline + S1 tests green; handler is purely in-memory (no `IApplicationDbContext` injection) — verified by test inspection; authorization test asserts `IsAbsolutePermission = true` bypass, `EDIT_SYSTEM_SETTINGS` grant pass, no-grant denial.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln` -> 0/0. `dotnet test TopLab.sln` -> 256 Domain + 553 Application + 70 Infrastructure = 879 tests, all passing.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §5.2 (M01-S2); no `LockWorkstationCommandValidator` because the command has no fields to validate (M17 `SignOutCommand` precedent); `IAuthorizedRequest` -> `EDIT_SYSTEM_SETTINGS` (id 10); the in-memory state lives in `CurrentUserService.ClearSession()`; no DI change (ICurrentUserService already registered); permission-gate bypass for `IsAbsolutePermission = true` (M17 floor precedent).
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `ICurrentUserService` (read — has `ClearSession()`), `CurrentUserService.cs` (read — production implementation), `SignOutCommand` + handler (read — no-validator precedent for thin in-memory command), `IAuthorizedRequest` (read — marker interface), `SamplePipeline/Commands/EchoName/` (read — `IAuthorizedRequest` template), `PermissionConfiguration.cs:27` (read — `EDIT_SYSTEM_SETTINGS` at id 10), `ExternalEntitiesAuthorizationTests.cs` (read — auth theory class precedent), `AuthorizationBehavior` (read — enforces `IsAbsolutePermission || HasPermission` then returns standard denial), `Result/Error`.
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: (1) `LockWorkstationCommand.cs` (record implementing `IRequest<Result>` + `IAuthorizedRequest` with `EDIT_SYSTEM_SETTINGS`); (2) `LockWorkstationCommandHandler.cs` — ctor injects `ICurrentUserService`, `Handle` calls `ClearSession()` exactly once then returns `Result.Success()`; (3) `AccessAndNavigationAccessPolicy.cs` constants class; (4) `LockWorkstationCommandHandlerTests.cs` — 4 tests: `ClearSession` once, no `IApplicationDbContext`, idempotent on already-cleared session, returns `Result.Success()`; (5) `AccessAndNavigationAuthorizationTests.cs` — Theory asserts `LockWorkstationCommand.RequiredPermissionCode == "EDIT_SYSTEM_SETTINGS"`; standard denial message returned when user lacks the permission; `IsAbsolutePermission = true` bypasses the gate; user with the `EDIT_SYSTEM_SETTINGS` grant passes. No `LockWorkstationCommandValidator` (M17 `SignOutCommand` precedent — no fields to validate). No DI change (`ICurrentUserService` already registered).
- [x] **Stage 5 — Execution:** Slice implemented per plan. All 3 new files + 2 new test files created; no DI change.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`. Evidence: `dotnet build TopLab.sln` -> 0/0. `dotnet test TopLab.sln` -> 256 Domain + 561 Application (was 553; +8: 4 handler tests + 4 authorization tests) + 70 Infrastructure = 887 tests, all passing.
- [x] **Stage 7 — Validation Gate:** VG-02 passed. Evidence: build/test output above; `ClearSession` called exactly once; no `IApplicationDbContext` injection verified by reflection on constructor parameters; idempotent on already-cleared session; authorization theory class passes for the absolute-user bypass, the granted-permission pass, and the no-grant denial.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated; M-01 backend work declared complete (Presentation layer remains out of scope).
- [x] **Stage 10 — Git Commit (authorized local):** `[M-01] Slice 2/2: M-01 application: LockWorkstationCommand and tests — loop-engineering` + `Stages 1-10 verified. Gate VG-02 passed.` — on `main`, never push.

---

## Current Status

- Overall: 2/2 slices done — **M-01 backend work is complete** (per plan §5.1 / §5.2 / §8). The Presentation layer (login window, status-bar wiring, navigation-menu wiring, lock-workstation UI) remains out of scope and is what the M-01 "Done" criterion requires; the plan declares M-01 backend work complete.
- Slice 1 — M-01 application: extended connectivity DTO and tests: [x] Done (committed)
- Slice 2 — M-01 application: `LockWorkstationCommand` and tests: [x] Done (committed)

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-07 | 0 | — | Memory file created | OK | — |
| 2026-09-07 | 1 | 1-10 | Slice 1 implemented; VG-01 passed | OK | [M-01] Slice 1/2 commit |
| 2026-09-07 | 2 | 1-10 | Slice 2 implemented; VG-02 passed | OK | [M-01] Slice 2/2 commit |

## Stop Report (append only if a stop condition triggers)
