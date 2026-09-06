# Top-Lab — Handoff Document M12

## نظام توب لاب — تسليم جلسة عمل (Module 12 — Test Catalog & Reference Ranges)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-06_M12_test-catalog-and-reference-ranges` |
| Session date (UTC) | 2026-09-06 |
| Session start (UTC) | 2026-09-06 |
| Session end (UTC) | 2026-09-06 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M12 |
| Module name | Test Catalog & Reference Ranges |
| Wave | 2 |
| Feature folder(s) touched | `src/TopLab.Domain/Tests/`, `src/TopLab.Application/Features/TestCatalogAndReferenceRanges/`, `src/TopLab.Infrastructure/Persistence/Configurations/`, `src/TopLab.Infrastructure/Persistence/Migrations/` |
| Layers touched | Domain / Application / Infrastructure |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `0278fb3` (`a30f99e` docs commit) |
| Final commit at session end | `(filled at commit time)` |

---

## 2. Session Objective (Required)

Implement Module 12 **Test Catalog & Reference Ranges** end-to-end in the five slices S1–S5 of `Docs/OpenCode/M-12.md` (Domain behaviors → Application read surface → Application write surface → Infrastructure + migration → hardening/docs/close-out), satisfying FR-M12-001 … FR-M12-006 and BR-04/BR-05/BR-13. This covers the `Test`/`TestGroup`/`ReferenceRange`/`WorkGroupLog`/`WorkGroupLogItem` lifecycle (soft deactivate/reactivate with atomic TestGroup cascade, reference-range BR-04 age/sex matching), the read/write Application surface with `EDIT_SYSTEM_SETTINGS` authorization and FluentValidation, the sanctioned schema change (`Tests.TestCode` unique, `Tests/TestGroups.IsActive` default 1) via one EF migration, and ADR-0028/0029. Build must be 0 errors / 0 warnings; all tests green; coverage floors met for M-12 scope; local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain behaviors** — Implementation Complete — `src/TopLab.Domain/Tests/`: `Test` (TestCode, `MaxTestCodeLength = 50`, `Deactivate`/`Reactivate`, unique-code guard), `TestGroup` (lifecycle + `Rename`), `ReferenceRange` (+ `ReferenceRangeSnapshot` value object with BR-04 age-unit-sensitive matching), `WorkGroupLog`/`WorkGroupLogItem` (row-level `Create`, no public constructor). Commit `234f98d`; 169 Domain tests green (114 baseline + 55 new).
- **S2 — Application read surface** — Implementation Complete — 5 queries (`GetTests`, `GetTestGroups`, `GetReferenceRanges`, `GetWorkGroupLogs`, `GetWorkGroupLogItems`) + 6 DTOs + `FakeApplicationDbContext` extensions. Commit `5a641d0`; 145 Application tests green (+30).
- **S3 — Application write surface** — Implementation Complete — 14 command use-cases (`CreateTestCommand` … `DeleteReferenceRangeCommand`) each with command + handler + validator; all implement `IAuthorizedRequest` with `RequiredPermissionCode = "EDIT_SYSTEM_SETTINGS"`; Arabic canonical error/message strings; no `DeleteTest`/`DeleteTestGroup` (soft-deactivate instead); `SaveWorkGroupLogItems` produces exactly one `SaveChanges`. **R-1 fix landed** (pre-approved): `FluentValidation.DependencyInjectionExtensions` 12.1.1 + `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` in `src/TopLab.Application/DependencyInjection.cs` (cross-cutting, affects M-17/M-22 — see §8). Commit `d44bf18`; 280 Application tests green (+135).
- **S4 — Infrastructure + migration** — Implementation Complete — `TestConfiguration.cs` (TestCode nvarchar(50) required + unique `IX_Tests_TestCode`, IsActive default true), `TestGroupConfiguration.cs` (IsActive default true); migration `20260906093902_AddTestCodeAndLifecycleColumns` generated, **applied, rolled back, and re-applied against LocalDB** (`Server=(localdb)\mssqllocaldb;Database=TopLab`); ADR-0028/0029 appended; `F5ConfigurationTests` + 3 new tests; `TestDeletionCascadeTests.cs` created. Commit `9758466`; 38 Infrastructure tests green (+7).
- **S5 — Hardening / docs / close-out** — Implementation Complete — Master Tracking Sheet M12 row → 🟩 Done (+ wave summary + change-log row); Data Model Blueprint §5.1/§5.2 updated; this handoff created; Release build 0/0; full suite 487/487; coverage measured (see §4.2 for waiver details). Commits: this S5 code/docs commit + memory-file docs delta.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Debug and Release).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (Release, `dotnet test TopLab.sln -m:1`): **487 green** = 169 Domain + 280 Application + 38 Infrastructure.
- New tests added: +269 net since baseline (185 at session start → 487).
- Tests currently failing: none.

**Coverage (coverlet, measured per project in Release):**

| Scope | Whole-project lines | M-12 footprint lines | Floor | Result |
|---|---|---|---|---|
| Domain | 64.1% | **93.8%** (Test 96.9%, TestGroup 89.5%, ReferenceRange 96.6%, ReferenceRangeSnapshot 81.8%, WorkGroupLog 92.3%, WorkGroupLogItem 84.6%) | ≥ 90% | **Waiver for whole-project; M-12 scope meets floor** |
| Application | 91.1% | **97.2%** (`Features.TestCatalogAndReferenceRanges.*` + DI) | ≥ 80% | Pass |
| Infrastructure | 5.4% | **100%** (Test/TestGroup/ReferenceRange/WorkGroupLog/WorkGroupLogItem configurations) | ≥ 70% | **Waiver for whole-project; M-12 scope meets floor** |

### 4.3 Migrations

- New EF Core migration(s) added: `src/TopLab.Infrastructure/Persistence/Migrations/20260906093902_AddTestCodeAndLifecycleColumns.cs` (+ `.Designer.cs`), `ApplicationDbContextModelSnapshot.cs` regenerated.
- Migration scope (reviewed, locked): `Tests + TestCode nvarchar(50) NOT NULL`, `Tests + IsActive bit NOT NULL DEFAULT 1`, `TestGroups + IsActive bit NOT NULL DEFAULT 1`, `CREATE UNIQUE INDEX IX_Tests_TestCode` — nothing else (§5.5 gate).
- Migration applied to a local database during the session: **Yes** — applied to LocalDB `TopLab`, rolled back to `20260828123530_RenamePkColumns`, re-applied; idempotent script inspected (only the four statements above).
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: `FluentValidation.DependencyInjectionExtensions` **12.1.1** (package added in `Directory.Packages.props` + `TopLab.Application.csproj`); `DependencyInjection.cs` gains `using FluentValidation;` and `services.AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>();` (before `AddMediatR`).
- Composition-root changes (`App.xaml.cs`): none (validators resolved through the Application DI `AddApplication()` extension already called there).
- Note: the `FluentValidation` namespace (not `FluentValidation.DependencyInjectionExtensions`) hosts the `ServiceCollectionExtensions` class in v12.1.1.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none beyond `Directory.Packages.props` package pin.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All five slices reached a terminal state; module closed out in the Master Tracking Sheet (§4/§5/§6/§9) and this handoff.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

- **Decision:** Add a user-facing `Tests.TestCode nvarchar(50)` with a unique index as the "test number" search key (FR-M12-001), distinct from the surrogate `TestId`.
  - **Reason:** FR-M12-001 requires search by test number; the surrogate PK is internal and not display-stable across imports.
  - **Scope of impact:** Domain + Application validators + Infrastructure migration + downstream display (M-04).
  - **Follow-up required:** Yes — recorded as **ADR-0028**.
- **Decision:** Introduce `IsActive bit NOT NULL DEFAULT 1` on `Tests` and `TestGroups` as the soft-lifecycle flag; **no hard delete** of Test/TestGroup in M-12.
  - **Reason:** Retiring tests/groups must not orphan reference ranges, work-group items, or future patient-test rows.
  - **Scope of impact:** Deactivate/Reactivate commands, read-side `IncludeInactive` filtering, migration, downstream M-02 ordering surfaces.
  - **Follow-up required:** Yes — recorded as **ADR-0029** (includes the deliberate asymmetry: TestGroup deactivation cascades atomically, reactivation does not).
- **Decision (R-1 fix):** Register validators via the fluent assembly scan (`AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()`).
  - **Reason:** Pre-flight confirmed validators were implemented but never registered, so the ValidationBehavior had no validators to run.
  - **Scope of impact:** Cross-cutting — benefits M-17 and M-22 request validation too.
  - **Follow-up required:** No (regression tests `ValidatorRegistrationTests` in place).

---

## 7. Open Issues, Bugs and Risks (Required — mark "None" if none)

- **Symptom:** `WorkGroupLogItem` has no DB FK to `Test` in the F5 baseline.
- **Reproduction steps:** Inspect `WorkGroupLogItemConfiguration.cs` (comment deliberately suppresses the relationship) and the `BaselineDataModel` migration (only a `WorkGroupLogs` FK exists).
- **Suspected cause / area of code:** Pre-existing baseline design decision, not introduced by M-12.
- **Severity:** Low (M-12 offers no hard delete of tests; a future module adding delete must add the FK).
- **Suggested next investigation step:** Add the `Test → WorkGroupLogItem` cascade FK in the module that first introduces a hard-delete path, under its own migration.

---

## 8. Deviations and Waivers (Required — mark "None" if none)

- **Convention departed from:** M-12 plan §5.2 anticipated verifying a "by-convention FK cascade from `Test` → `WorkGroupLogItem`"; the baseline has no such FK.
  - **Nature:** The cascade test now asserts the real modeled behaviors instead: `Test → ReferenceRange` Cascade, `WorkGroupLog → WorkGroupLogItem` Cascade, `PatientTest → Test` Restrict, and the documented absence of the `Test → WorkGroupLogItem` FK.
  - **Justification:** Adding the FK would exceed the locked migration scope (§5.5, R-7 stop-gate). **Owner-approved** ("Keep scope; document deviation"). Recorded in ADR-0029 and §7.
  - **Temporary?** No — permanent scope decision recorded in ADR-0029.
- **Convention departed from:** Whole-project coverage floors (Domain ≥ 90%, Infrastructure ≥ 70%).
  - **Nature:** Whole-project line coverage is 64.1% (Domain) / 5.4% (Infrastructure) because both projects contain large bodies of code for not-yet-implemented modules (Patients, Results, Billing, Sent-Out, ~36 declarative EF configurations, maintenance services). M-12's own footprint meets the floors: **93.8%** Domain / **100%** Infrastructure configs / **97.2%** Application (project-wide 91.1% already above the 80% floor).
  - **Justification:** Per plan §6.3, uncovered members are recorded here as waivers rather than padded with ad-hoc tests; the under-test code belongs to future modules, not M-12.
  - **Temporary?** Yes — the floors become fully reachable per project when the remaining modules' tests land; revisit at each module closure.
- **Convention departed from:** Test-plan migration gate asked to "apply and roll back against an ephemeral SQL Server".
  - **Nature:** Executed against **LocalDB** fallback (`Server=(localdb)\mssqllocaldb;Database=TopLab`) per the owner-authorized execution parameter; apply → rollback → re-apply all successful.
  - **Temporary?** No — LocalDB is the established local development database for this repo (see F4/F5 tracking notes).

---

## 9. Pending Reviews and Audits (Required)

- **Code review status:** Not started (local-only commits; no reviewer assigned).
- **Audit acceptance status:** Not started.
- **Blocking findings from review or audit:** none.

---

## 10. Next Session Objective (Required)

- **Most important task:** Begin **Wave 2 continuation / M-14 — External Entities** (or the next module per the dependency map), which depends on M17/M22 and is the module the M-12 write surface will integrate with for test catalog screens.
- **Prerequisites:** M-12 requires a Presentation layer to expose the catalog screens (out of M-12 scope); when M-02 or M-14 builds those screens it consumes the M-12 queries/commands and `EDIT_SYSTEM_SETTINGS` gate already in place.
- **Expected end-state:** The next module reads `Docs/Handoff_<module>.md`, the Master Tracking Sheet `M12` row (should read 🟩 Done), and `Docs/OpenCode/M-12.md` exit criteria to verify, then proceeds with its own slice plan.

---

## 11. Required Reading Before Continuing (Required)

- Coding Standards & Conventions.
- Architecture & Folder Structure Blueprint.
- Data Model / Database Schema Blueprint — §5.1 `TestGroup`, §5.2 `Test`, §5.3 `ReferenceRange`, §5.6 `WorkGroupLog`/`WorkGroupLogItem` (now includes `TestCode` + `IsActive`).
- Product Requirements Document — §M12 (FR-M12-001 … FR-M12-006), BR-04/BR-05/BR-13.
- Test Strategy & Audit Acceptance Criteria — coverage-floor rules.
- Module Dependency & Execution Order Map — M12 relations (M17, M22, M02, M04, M14).
- Master Tracking Sheet — §4 row M12 and §9 change log.
- ADR-0028, ADR-0029 (this module), ADR-0018, ADR-0021 (context).
- Prior handoff documents for the same module: none (first delivery).
- In-repo execution record: `Docs/OpenCode/M-12.md`, `Docs/OpenCode/M-12-memory.md`, `Docs/OpenCode/M-12-Loop-Engineering-Execution-Mapping.md`.

---

## 12. Environment and Tooling Notes (Optional)

- .NET SDK 8; `dotnet-ef` global tool 8.0.30; SQL Server Express LocalDB `(localdb)\mssqllocaldb`, database `TopLab`.
- Commands executed: `dotnet ef migrations add AddTestCodeAndLifecycleColumns --project src/TopLab.Infrastructure`; `dotnet ef database update` / `dotnet ef database update 20260828123530_RenamePkColumns`; `dotnet ef migrations script … --idempotent` (scope inspection); coverage with `dotnet test … --collect:"XPlat Code Coverage"` per project.
- **Tooling gotcha (important):** `dotnet ef … --no-build` uses whatever assembly is on disk; after editing EF configurations or generating a migration you must rebuild before running `database update`/`migrations list` or EF operates on a stale model (observed during S4: first generated migration had `nvarchar(max)`, no index, `defaultValue: false`; rebuilt and regenerated to the correct scope).
- `rg` is not installed on this machine; use whiprg/ripgrep alternatives or PowerShell/`Select-String`.

---

## 13. Artifacts Produced (Required — mark "None" if none)

- **Name:** M-12 execution memory file — **Location:** `Docs/OpenCode/M-12-memory.md` — **Purpose:** slice-by-slice loop-engineering trace (Stages 1–10 per slice) — **Persistence:** Kept (in-repo working record).
- **Name:** M-12 execution mapping — **Location:** `Docs/OpenCode/M-12-Loop-Engineering-Execution-Mapping.md` — **Purpose:** Arabic slice-map of the plan — **Persistence:** Kept.
- Non-source artifacts: none produced outside source control (coverage XML live under the temporary directory `%Temp%\opencode\m12cov` — scratch, deleted after review).

---

## 14. Signature Block (Required)

| Role | Name | Date (UTC) | Confirmation |
|---|---|---|---|
| Outgoing agent | Local coding agent (Top-Lab) | 2026-09-06 | I confirm this handoff document accurately reflects the state of the work at session end. |
| Reviewer (if any) | TBD |  | I have reviewed this handoff for completeness. |
| Incoming agent (on acceptance) | TBD |  | I confirm I have read and understood this handoff and accept it as my starting context. |

---

## 15. Attachments (Optional)

- `Docs/OpenCode/M-12.md` — execution plan with exit criteria.
- `Docs/OpenCode/M-12-memory.md` — living loop-engineering trace (per-slice Stages 1–10 evidence).
- `Docs/Source/Top_Lab_ADR.md` — ADR-0028, ADR-0029.

---

*End of handoff document.*