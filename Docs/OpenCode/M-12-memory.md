# Loop Engineering — Memory File

- **Module:** Test Catalog & Work Group Lifecycle (M-12)
- **Module Number:** M-12
- **Source Plan:** Docs/OpenCode/M-12.md
- **Date Created:** 2026-09-06
- **Total Slices:** 5
- **Current Slice:** 1 — in progress (Stages 1-7 verified, commit pending)
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Implements the lifecycle states and behaviors for the Test Catalog: Test, TestGroup, ReferenceRange (with freeze snapshots and BR-04 age/sex matching), and WorkGroupLog aggregates, plus their Application query/command surfaces, EF Core persistence (unique TestCode, IsActive columns, soft-delete cascade semantics), EF migration, and handoff documentation. Done means: Domain behaviors + tests, Application read/write surfaces + tests, Infrastructure migration applied to a real database, and a Release build passing the full test suite with mandated coverage thresholds.

## Global Validation Gates

- Gate G0 (pre-execution): `build` passes zero errors + zero warnings, `tests` pass 100%.
- Gate G1 (post-execution per slice): same as G0 plus slice-specific gate below.

## Stop/Continue Rule

After a slice completes (all 10 stages done), verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice.
If any one fails → STOP execution and emit a stop report. Do NOT proceed to the next slice.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: 5 consecutive failures for the same reason (not 4).
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4 -> S5, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/inspection) replaces any standard UI journey — M-12 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), NEVER push.
- R-1 fix pre-approved: `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` in `src/TopLab.Application/DependencyInjection.cs` if pre-flight confirms the gap in S4.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain behaviors + tests: build zero/zero; all Domain tests pass; external `new WorkGroupLogItem(` callers zero | `dotnet build src/TopLab.Domain`; `dotnet test tests/TopLab.Domain.Tests`; grep src+tests |
| 2 | VG-02 | Application read surface: build zero/zero; all Application tests pass (queries, DTOs, fakes) | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests` |
| 3 | VG-03 | Application write surface: build zero/zero; all Application tests pass (14 commands, validators, auth) | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests` |
| 4 | VG-04 | Infrastructure: build zero/zero; Infra tests pass; migration AddTestCodeAndLifecycleColumns generated (.cs + .Designer.cs + ModelSnapshot updated) and applied to a real DB (LocalDB) | `dotnet build src/TopLab.Infrastructure`; `dotnet test tests/TopLab.Infrastructure.Tests`; `dotnet ef migrations add` + `dotnet ef database update` |
| 5 | VG-05 | Close-out: Release build zero/zero over solution; full suite passes `-m:1`; coverage thresholds Domain >= 90, Application >= 80, Infrastructure >= 70; tracking sheet marked Done; handoff written | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain behaviors + tests | [~] In progress (Stages 1-7 done, commit pending) | VG-01 |
| 2 | Application read surface (queries) + DTOs + fakes + tests | [ ] Not started | VG-02 |
| 3 | Application write surface (commands) + validators + auth tests | [ ] Not started | VG-03 |
| 4 | Infrastructure + migration + configs | [ ] Not started | VG-04 |
| 5 | Hardening / docs / close-out | [ ] Not started | VG-05 |

---

## Slice 1: Domain behaviors + tests

- **Goal:** Add TestCode and IsActive lifecycle to Test/TestGroup, Update+snapshot to ReferenceRange, item management to WorkGroupLog, and the private-ctor/Create factory contract to WorkGroupLogItem, with full domain test coverage (incl. BR-04 matrix).
- **Touches:** src/TopLab.Domain/Tests/Test.cs (modify), TestGroup.cs (modify), ReferenceRange.cs (modify), WorkGroupLog.cs (modify), WorkGroupLogItem.cs (modify), ReferenceRangeSnapshot.cs (create); tests/TopLab.Domain.Tests/Tests/TestCatalogTests.cs (modify call sites), TestTests.cs (create), TestGroupTests.cs (create), ReferenceRangeTests.cs (create), WorkGroupLogTests.cs (create), WorkGroupLogItemTests.cs (create)
- **Validation Gate:** VG-01 — Domain build zero/zero; all Domain tests pass; external `new WorkGroupLogItem(` callers zero.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Domain builds zero errors + zero warnings (Build succeeded, 0 Warning(s), 0 Error(s)); `dotnet test tests/TopLab.Domain.Tests` PASS (Failed: 0, Passed: 114, Total: 114). Evidence in S1 execution log (2026-09-06).
- [x] **Stage 2 — Deep Understanding:** Requirements captured from plan sections 3-3.6 and BR-04: TestCode required/trimmed/max 50 (domain guard + tests); IsActive default true on Test/TestGroup; Deactivate/Reactivate; ReferenceRange.Update with guards AgeMin>=0, AgeMin<=AgeMax, MinValue<=MaxValue, comments <=500 on Create AND Update; ReferenceRangeSnapshot is a pure-domain value record (TestId int, Sex?, AgeUnit, AgeMin, AgeMax, MinValue, MaxValue, LowComment?, HighComment?, CapturedAtUtc) — NOT an EF-entity shape (M-04 owns persistence); Matches(sex, ageUnit, ageValue) is age-unit-sensitive with null-Sex serving both sexes and inclusive boundaries; WorkGroupLog item mutations (AddItem duplicate-guard, RemoveItem missing-guard, ClearItems, ContainsTest); WorkGroupLogItem public parameterless ctor only for EF, parameterized ctor private, static Create factory; price-update guard PatientPrice>=0 added to Test.Create/Test.Update to satisfy the plan's price-update guard test (§3.6).
- [x] **Stage 3 — File Analysis:** Inspected all 5 Domain entities + Common base types (StronglyTypedId<TValue> is class-based so `is null` checks are valid; AuditableEntity<TId>, ValueObject), existing TestCatalogTests call sites (lines 13/21/27), csproj (ImplicitUsings+Nullable enabled), test conventions (xUnit, namespace TopLab.Domain.Tests.<Area>). Pre-check grep `new WorkGroupLogItem(` = 0 callers in src+tests (only plan/documentation matches).
- [x] **Stage 4 — Planning:** Step-by-step plan written: (1) Test.cs — TestCode + IsActive + guards + price guard + Deactivate/Reactivate + extended Create/Update; (2) TestGroup.cs — Rename + IsActive + Deactivate/Reactivate + extended Create; (3) ReferenceRange.cs — Update + guards + CaptureSnapshot; (4) WorkGroupLog.cs — Rename/ContainsTest/AddItem/RemoveItem/ClearItems; (5) WorkGroupLogItem.cs — public parameterless ctor + private parameterized ctor + Create factory; (6) ReferenceRangeSnapshot.cs — sealed record; (7) fix TestCatalogTests; (8) 5 new test files.
- [x] **Stage 5 — Execution:** All 7 implementation/edit steps completed; 5 new test files written (55 new tests).
- [x] **Stage 6 — Post-Execution Verification:** Domain builds zero errors + zero warnings (Build succeeded, 0 Warning(s), 0 Error(s)); `dotnet test tests/TopLab.Domain.Tests` PASS (Failed: 0, Passed: 169, Skipped: 0, Total: 169).
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — build zero/zero; 169/169 tests passed; external `new WorkGroupLogItem(` grep in src+tests = 0 callers (only sanctioned factory line inside WorkGroupLogItem.cs — plan-mandated implementation; documented interpretation of R-5 gate).
- [ ] **Stage 8 — Documentation Update:** Memory file and per-slice checkboxes marked; remaining evidence appended to execution log.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated in this file.
- [ ] **Stage 10 — Git Commit (authorized local):** Staged and committed locally per owner authorization (never push). Message per R-commit pattern: `[M-12] Slice 1/5: Domain behaviors + tests — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.`

---

## Slice 2: Application read surface (queries) + DTOs + fakes + tests

- **Goal:** Expose the M-12 query side over the catalog: SearchTestCatalog, GetTestById, GetTestGroups, GetWorkGroupLogs, GetReferenceRanges queries with DTOs and a FakeApplicationDbContext for handler tests.
- **Touches:** src/TopLab.Application/Tests/testCatalogDtos.cs + 5 query use-case folders under Tests/Queries/ (SearchTestCatalog, GetTestById, GetTestGroups, GetWorkGroupLogs, GetReferenceRanges); tests/TopLab.Application.Tests fakes extension (FakeApplicationDbContext) + 5 QueryHandlerTests
- **Validation Gate:** VG-02 — Application build zero/zero; all Application tests pass (queries, DTOs, fakes).

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build src/TopLab.Application` + `dotnet test tests/TopLab.Application.Tests`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan sections 4-4.5; queries must NOT implement IAuthorizedRequest (auth is command-side only); DTOs must map domain shapes incl. TestCode/IsActive and ReferenceRange snapshot-consistent values; fakes must support AsNoTracking query paths.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: testCatalogDtos.cs, 5 query folders, FakeApplicationDbContext, 5 QueryHandlerTests + existing Application conventions.
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: DTOs first, then queries, then fake extension, then handler tests.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-02 passed. Evidence: build + test output.
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-12] Slice 2/5: Application read surface (queries) + DTOs + fakes + tests — loop-engineering` + `Stages 1-10 verified. Gate VG-02 passed.` — never push.

---

## Slice 3: Application write surface (commands) + validators + auth tests

- **Goal:** Implement the 14 M-12 command use-cases with FluentValidation validators and Application-layer auth tests (EDIT_SYSTEM_SETTINGS); cascade decrements to child catalogs; single batch SaveChanges for WorkGroupLog item writes.
- **Touches:** src/TopLab.Application/Tests/{CreateTest, UpdateTest, DeactivateTest, ReactivateTest, CreateTestGroup, UpdateTestGroup, DeactivateTestGroup, ReactivateTestGroup, CreateWorkGroupLog, RenameWorkGroupLog, SaveWorkGroupLogItems, CreateReferenceRange, UpdateReferenceRange, DeleteReferenceRange} commands + validators; tests/TopLab.Application.Tests handlers + auth tests
- **Validation Gate:** VG-03 — Application build zero/zero; all Application tests pass (14 commands, validators, auth).

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build src/TopLab.Application` + `dotnet test tests/TopLab.Application.Tests`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan sections 5-5.6; all handlers implement IAuthorizedRequest with EDIT_SYSTEM_SETTINGS; validators mirror domain guards (PatientPrice>=0, TestCode max 50 required, name required, age/comment rules); no DeleteTest/DeleteTestGroup use-cases (deactivate/soft-delete instead); SaveWorkGroupLogItems must produce ONE SaveChanges (SaveChangesCallCount == 1).
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: 14 command use-case files + 14 validators + DependencyInjection registration (R-1 gap pre-check) + auth test conventions.
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: commands -> validators -> DI wiring -> handler unit tests -> auth tests.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-03 passed. Evidence: build + test output.
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-12] Slice 3/5: Application write surface (commands) + validators + auth tests — loop-engineering` + `Stages 1-10 verified. Gate VG-03 passed.` — never push.

---

## Slice 4: Infrastructure + migration + configs

- **Goal:** Configure persistence for the new lifecycle columns: Tests.TestCode (max 50, unique index IX_Tests_TestCode), default IsActive (true) on Tests and TestGroups; generate + apply migration AddTestCodeAndLifecycleColumns (file + .Designer.cs + ModelSnapshot updated) against LocalDB; add EF configuration and cascade tests.
- **Touches:** src/TopLab.Infrastructure/Persistence/Configurations/TestConfiguration.cs (modify), TestGroupConfiguration.cs (modify), new migration AddTestCodeAndLifecycleColumns (create), ModelSnapshot (update); tests/TopLab.Infrastructure.Tests/Persistence/Configurations/F5ConfigurationTests.cs (extend), TestDeletionCascadeTests.cs (extend); Docs/Source/Top_Lab_ADR.md (ADR-0028/0029); src/TopLab.Application/DependencyInjection.cs (R-1 conditional fix)
- **Validation Gate:** VG-04 — Infrastructure build zero/zero; Infra tests pass; migration applied to LocalDB; snapshot updated.

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build src/TopLab.Infrastructure` + `dotnet test tests/TopLab.Infrastructure.Tests`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan sections 6-6.4; migration scope LOCKED (only Tests.TestCode max-50 + unique IX_Tests_TestCode, Tests.IsActive default true, TestGroups.IsActive default true); soft-delete cascade semantics tested (domain-only isolation already proven in S1; full cascade at Application layer in S3); R-1 pre-approved fix if pre-flight confirms the validator-registration gap.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: TestConfiguration.cs, TestGroupConfiguration.cs, ModelSnapshot, F5ConfigurationTests.cs, TestDeletionCascadeTests.cs, Top_Lab_ADR.md, DependencyInjection.cs.
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: configs -> EF config tests -> migration generate -> apply to LocalDB -> ADR entries -> R-1 fix if confirmed.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-04 passed. Evidence: build/test output + successful `dotnet ef database update` against LocalDB.
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-12] Slice 4/5: Infrastructure + migration + configs — loop-engineering` + `Stages 1-10 verified. Gate VG-04 passed.` — never push.

---

## Slice 5: Hardening / docs / close-out

- **Goal:** Mark the module Done in the master tracking sheet, update the data model blueprint, write Docs/Handoff_M12.md (per Top_Lab_Handoff_Template.md), and prove the whole solution in Release configuration with full-suite tests + coverage thresholds.
- **Touches:** Docs/Source/Top_Lab_Master_Tracking_Sheet.md, Docs/Source/Top_Lab_Data_Model_Blueprint.md, Docs/Handoff_M12.md (create), final Release build + full test run with coverage
- **Validation Gate:** VG-05 — Release build zero/zero over solution; `dotnet test TopLab.sln -m:1` all pass; coverage Domain >= 90, Application >= 80, Infrastructure >= 70.

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln -c Release`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan sections 7-7.4; tracking sheet marks M-12 Done with completed-slice counts; blueprint reflects TestCode/IsActive/snapshot artifact; handoff per template; coverage thresholds are hard gates.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: Top_Lab_Master_Tracking_Sheet.md, Top_Lab_Data_Model_Blueprint.md, Top_Lab_Handoff_Template.md, coverage tooling in test projects.
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: docs updates -> Release build -> full test suite -> coverage measurement -> final Arabic report.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-05 passed. Evidence: Release build output + `dotnet test TopLab.sln -m:1` + coverage numbers.
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-12] Slice 5/5: Hardening / docs / close-out — loop-engineering` + `Stages 1-10 verified. Gate VG-05 passed.` — never push.

---

## Current Status

- Overall: 0/5 slices done (Slice 1 verification passed; commit pending)
- Slice 1 — Domain behaviors + tests: [~] In progress (Stages 1-7 done, commit pending)
- Slice 2 — Application read surface (queries) + DTOs + fakes + tests: [ ] Not started
- Slice 3 — Application write surface (commands) + validators + auth tests: [ ] Not started
- Slice 4 — Infrastructure + migration + configs: [ ] Not started
- Slice 5 — Hardening / docs / close-out: [ ] Not started

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-06 | 0 | — | Memory file created | OK | — |
| 2026-09-06 | 1 | 1 | Pre-Execution Verification (domain) | PASS — build 0/0; tests 114/114 | — |
| 2026-09-06 | 1 | 2 | Deep Understanding | PASS — spec captured from M-12 sections 3-3.6 + BR-04 | — |
| 2026-09-06 | 1 | 3 | File Analysis + pre-check grep | PASS — `new WorkGroupLogItem(` = 0 external callers; call sites enumerated | — |
| 2026-09-06 | 1 | 4 | Planning | PASS — 8-step plan written | — |
| 2026-09-06 | 1 | 5 | Execution — 5 domain files + 1 new snapshot + 5 new test files + TestCatalogTests fix | PASS | — |
| 2026-09-06 | 1 | 6 | Post-Execution Verification (domain) | PASS — build 0/0; tests 169/169 | — |
| 2026-09-06 | 1 | 7 | Validation Gate VG-01 | PASS — build zero/zero, tests 169/169, external `new WorkGroupLogItem(` = 0 | — |
| 2026-09-06 | 1 | 8 | Documentation Update | Pending until Stage 10 | — |
| 2026-09-06 | 1 | 9 | Memory Status Update | Pending until Stage 10 | — |
| 2026-09-06 | 1 | 10 | Git Commit (local, authorized) | Pending | — |

## Stop Report (append only if a stop condition triggers)