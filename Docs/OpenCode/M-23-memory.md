# Loop Engineering — Memory File

- **Module:** Utilities (Tools) (M-23, backend only)
- **Module Number:** M-23
- **Source Plan:** Docs/OpenCode/M-23.md
- **Date Created:** 2026-09-15
- **Total Slices:** 4
- **Current Slice:** S2 — completed; S3 next
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Delivers the M-23 Utilities backend surface with zero business-schema change and zero EF migration: S1 ships three pure Domain computation services (`MeasurementUnitConverter` with an explicit fail-closed conversion-pair table, `ArithmeticCalculator` as a safe recursive-descent four-function parser — no dynamic eval, `StopwatchCalculator.Elapsed`) plus Domain tests; S2 ships the ungated Application queries over those services (`ConvertMeasurementUnit`, `EvaluateCalculation`, `ComputeStopwatchElapsed` via `IDateTimeProvider`) plus the read-only `GetTestLibraryQuery` catalog projection (dictionary group names, unknown group → «مجموعة التحاليل غير موجودة.»), with DTOs/validators/handler tests; S3 ships the two workstation-local JSON-backed lists — `IPurchasesListStore`/`IPhoneBookStore` ports (ILabPrintTextStore precedent), add/remove/toggle commands, list queries, `JsonPurchasesListStore`/`JsonPhoneBookStore` Infrastructure implementations under `%ProgramData%\TopLab`, two DI registrations, and handler + JSON round-trip tests; S4 is the zero-drift proof, validator-registration extension, coverage roll-up, ADR-0047, tracking flip, and `Handoff_M23.md`. Everything is ungated (CheckDatabaseConnectivity precedent); `PermissionConfiguration` untouched; the Tools Phone Book never references patient data; Image Library and Shortcut Library backends are settled exclusions (ADR-0027 image precedent + Presentation/OS concerns). Zero Presentation content. No M-20 coupling of any kind.

## Global Validation Gates

- Gate G0 (pre-execution): `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- Gate G1 (post-execution per slice): same as G0 plus the slice-specific gate listed in the table below.

## Stop/Continue Rule

After a slice completes (all 10 stages done), verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice, with no pause and no human confirmation required.
If any one fails → retry. If the SAME failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs 5 CONSECUTIVE times, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do NOT proceed past this point without owner review. Ordinary expected test failures caused by the current slice and resolved within the same correction cycle do NOT count as five separate failures.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: 5 consecutive failures for the same reason.
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/model-assertion) replaces any standard UI journey — M-23 has no UI in scope.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote, NEVER force-push, NEVER modify or rewrite remote history. Commit message format: `[M-23] Slice N/4: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-23's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain computation services: solution build zero/zero; Domain tests green (every conversion pair number-for-number, unknown-pair throw; parser precedence/parentheses/decimals/malformed/division-by-zero; elapsed + inverted bounds); zero `Persistence/**` diff (grep gate); **Migration: NONE — zero-drift gate** (`has-pending-model-changes` → no changes) | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Domain.Tests`; grep gate; ef drift check |
| 2 | VG-02 | Computation queries + test library: Application build zero/zero; S2 tests green (happy paths; error surfaces verbatim; provider-supplied stopwatch end via fake clock; catalog projection/filters/NotFound/empty); zero `Persistence/**` + zero unexpected Domain diff (grep gates); coverage ≥ 80%. **Migration: NONE — zero-drift gate** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gates; ef drift check |
| 3 | VG-03 | Purchases list + phone book: Application + Infrastructure build zero/zero; S3 tests green (handler behavior over fake stores; validators; JSON round-trip incl. missing-file-empty and malformed-file → `Error.Unexpected`); zero `Persistence/**` diff (grep gate — stores live in `Infrastructure/Services/`); coverage App ≥ 80% / Infra ≥ 70%. **Migration: NONE — zero-drift gate** | `dotnet build src/TopLab.Application`; `dotnet build src/TopLab.Infrastructure`; `dotnet test` both test projects; grep gates; ef drift check |
| 4 | VG-04 | Zero-drift + close-out: Release build zero/zero; full suite green (`-m:1`); `dotnet ef migrations has-pending-model-changes` → no changes; snapshot unchanged; validator-registration extension green; coverage floors met or waived; ADR-0047 appended; M23 tracking row flipped; `Handoff_M23.md` per template; zero schema diff / zero Presentation content (grep gates) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain computation services + tests | [x] Done | VG-01 PASS |
| 2 | Application computation queries + Test Library | [x] Done | VG-02 PASS |
| 3 | Purchases list + phone book (workstation-local stores) | [ ] Pending | VG-03 |
| 4 | Tests + zero-drift proof + close-out | [ ] Pending | VG-04 |

---

## Settled Decisions (from plan — binding)

- **SD-23-1:** Pure computation utilities live as stateless Domain services in `src/TopLab.Domain/Utilities/` with thin Application query wrappers. Precedent-based (Domain calculators).
- **SD-23-2:** Purchases List + Phone Book persist as workstation-local JSON behind Application ports — no business table, no EF migration. Precedent-based (`ILabPrintTextStore`/ADR-0027; data model has no utility tables).
- **SD-23-3:** Entire surface ungated; `PermissionConfiguration` untouched. Precedent-based (`CheckDatabaseConnectivityQuery`; 13-row catalog immutable).
- **SD-23-4:** Feature folder `Features/Utilities/`. Precedent-based (dependency map §3).
- **SD-23-5:** Test Library = read-only catalog query over `Test`/`TestGroup` with dictionary group names. Precedent-based (M-19).
- **SD-23-6:** Image Library backend excluded (ADR-0027 no-images precedent; Presentation concern). Precedent-based exclusion — recorded, not open.
- **SD-23-7:** Shortcut Library backend excluded (OS/Presentation concern; no business data). Consistency-based exclusion — recorded, not open.
- **SD-23-8:** Converter uses an explicit pair table; unknown pair throws. Consistency-based decision — no direct precedent found.
- **SD-23-9:** Calculator is a safe recursive-descent parser; division by zero → «لا يمكن القسمة على صفر.». Consistency-based decision — no direct precedent found (ADR-0008 error mapping).
- **SD-23-10:** Stopwatch = pure elapsed computation; omitted end → `IDateTimeProvider` now. Precedent-based (F4 time provider).
- **SD-23-11:** DTO shapes fixed; Tools Phone Book is fully separate from patient phone numbers (FR-M23-002 binding). Precedent-based.

---

## Slice 1: Domain computation services + tests

- **Goal:** Ship `MeasurementUnitConverter`, `ArithmeticCalculator`, `StopwatchCalculator` with full Domain test coverage; prove zero persistence drift.
- **Touches:** `src/TopLab.Domain/Utilities/` (3 files, create); `tests/TopLab.Domain.Tests/Utilities/` (3 test classes, create)
- **Validation Gate:** VG-01 — build zero/zero; Domain tests green; zero-Persistence grep gate; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite 1894/1894 green.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1 + SD-23-1/8/9/10.
- [x] **Stage 3 — File Analysis:** Domain calculator style; DomainException abstract base.
- [x] **Stage 4 — Planning:** Converter → parser → stopwatch + tests.
- [x] **Stage 5 — Execution:** Implemented MeasurementUnitConverter, ArithmeticCalculator (+ CalculatorException), StopwatchCalculator + 3 test classes (47 tests).
- [x] **Stage 6 — Post-Execution Verification:** solution build 0/0.
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — Utilities 47/47; Domain 467/467; zero Persistence; drift clean.
- [x] **Stage 8 — Documentation Update:** checklist recorded.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 2: Application computation queries + Test Library

- **Goal:** Ship the four ungated queries with DTOs, validators, and handler tests, translating Domain guard failures into `Error.Validation` per ADR-0008.
- **Touches:** `src/TopLab.Application/Features/Utilities/Common/UtilitiesDtos.cs` (create); `.../Queries/ConvertMeasurementUnit/` (3 files, create); `.../Queries/EvaluateCalculation/` (3 files, create); `.../Queries/ComputeStopwatchElapsed/` (3 files, create); `.../Queries/GetTestLibrary/` (3 files, create); four test classes under `tests/TopLab.Application.Tests/Features/Utilities/` (create)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green; grep gates; coverage ≥ 80%; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 2)

- [x] **Stage 1 — Pre-Execution Verification:** Domain 467/467 after S1.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S2 + SD-23-3/5; Appendix A.
- [x] **Stage 3 — File Analysis:** CheckDatabaseConnectivityQuery ungated shape; Error.Validation; Test/TestGroup columns.
- [x] **Stage 4 — Planning:** DTOs → 4 queries + validators → handlers → 4 test classes.
- [x] **Stage 5 — Execution:** Implemented conversion/calculation/stopwatch/test-library queries + tests (15). Also stabilized a pre-existing midnight-UTC flake in SampleCollection tests (unrelated to M-23; required for G0).
- [x] **Stage 6 — Post-Execution Verification:** Application build 0/0.
- [x] **Stage 7 — Validation Gate:** VG-02 PASS — Utilities 15/15; full App 1335/1335; zero Persistence; drift clean.
- [x] **Stage 8 — Documentation Update:** checklist recorded.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 3: Purchases list + phone book (workstation-local stores)

- **Goal:** Ship the two JSON-backed list surfaces: ports, commands, queries, Infrastructure stores, DI, and tests.
- **Touches:** `src/TopLab.Application/Common/Interfaces/IPurchasesListStore.cs` + `IPhoneBookStore.cs` (create); five command folders + two query folders under `Features/Utilities/` (create); `src/TopLab.Infrastructure/Services/JsonPurchasesListStore.cs` + `JsonPhoneBookStore.cs` (create); `src/TopLab.Infrastructure/DependencyInjection.cs` (modify — two registrations); Application test classes (create); `tests/TopLab.Infrastructure.Tests/Services/JsonPurchasesListStoreTests.cs` + `JsonPhoneBookStoreTests.cs` (create)
- **Validation Gate:** VG-03 — builds zero/zero; S3 tests green; zero-Persistence grep gate; coverage floors; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 3)

- [ ] **Stage 1 — Pre-Execution Verification:** Full suite green after S2.
- [ ] **Stage 2 — Deep Understanding:** Plan §5 S3 + SD-23-2/11; Appendix A messages.
- [ ] **Stage 3 — File Analysis:** `ILabPrintTextStore.cs` port shape, M-22 `JsonLabPrintTextStore` (Handoff_M22 §3), `src/TopLab.Infrastructure/DependencyInjection.cs` registration style.
- [ ] **Stage 4 — Planning:** Ports → DTO usage → commands/queries → stores → DI → tests.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Application + Infrastructure builds 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-03 (incl. zero-drift).
- [ ] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-23] Slice 3/4: Purchases list + phone book stores — loop-engineering`.

---

## Slice 4: Tests + zero-drift proof + close-out

- **Goal:** Prove solution-wide zero drift, extend validator registration, roll up coverage, append ADR-0047, flip tracking, write `Handoff_M23.md`, finish full-suite green.
- **Touches:** `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (M23 row + change log); `Docs/Handoff_M23.md` (create); `Docs/Source/Top_Lab_ADR.md` (append ADR-0047 — reconfirm next free number at execution)
- **Validation Gate:** VG-04 — Release build 0/0; full suite green (`-m:1`); zero-drift proven; ADR + tracking + handoff present; grep gates clean. Migration: none (proven).

### 10-Stage Progress (Slice 4)

- [ ] **Stage 1 — Pre-Execution Verification:** Full suite green after S3.
- [ ] **Stage 2 — Deep Understanding:** Plan §6; handoff template; ADR numbering re-checked.
- [ ] **Stage 3 — File Analysis:** `ValidatorRegistrationTests.cs`, tracking sheet rows 85/103, ADR tail, `Docs/Source/Top_Lab_Handoff_Template.md`.
- [ ] **Stage 4 — Planning:** Validator extension → gates → docs → final run.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln -c Release` 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-04 (zero-drift proof binding).
- [ ] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated → MODULE COMPLETE.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-23] Slice 4/4: Tests + zero-drift proof + close-out — loop-engineering`.

---

## Current Status

- Slices complete: 2/4. Next action: begin S3 Stage 1.
- Dependency posture: M-23 ⇄ M-20 — no code-level dependency (verified); M-23's only code dependency (M-01 pipeline) is satisfied at the audited commit.
- S1 evidence: VG-01 PASS; commit `1fd8263`.
- S2 evidence (2026-09-15): VG-02 PASS; App 1335/1335; drift clean; midnight-UTC flake stabilized in SampleCollection tests.

## Execution Log

| Date | Slice | Stage | Action | Result |
|---|---|---|---|---|
| 2026-09-15 | 1 | 1-10 | Domain computation services + tests; VG-01 PASS | OK |
| 2026-09-15 | 2 | 1-10 | Computation queries + test library; VG-02 PASS | OK |

## Stop Report

(None — no stop condition has triggered.)
