# Loop Engineering — Memory File

- **Module:** Sample Collection & Separation (M-21, backend only)
- **Module Number:** M-21
- **Source Plan:** Docs/OpenCode/M-21.md
- **Date Created:** 2026-09-07
- **Total Slices:** 2
- **Current Slice:** All slices done — 2/2 slices done. Module M-21 complete.
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Implements the small M-21 backend surface (the Presentation phlebotomist screen is out of scope). S1 builds the `SampleCollection` Application feature folder with 2 queries (`GetPatientsWithUncollectedSamplesQuery` returning `PatientWithUndrawnTestsDto` ordered by `RegistrationDateUtc asc` with an optional `DateOnly? Day` filter, excluding soft-deleted patients and outside-drawn tests; `GetPatientTestsForDrawQuery` returning Drawn / NotDrawn lists for a patient) and 2 commands (`MarkSampleDrawnCommand` and `MarkAllSamplesDrawnForPatientCommand`, both `IAuthorizedRequest` -> `SampleCollectionAccessPolicy.AddEditPatient` = `"ADD_EDIT_PATIENT"`, settled OD-8 reuse). The `MarkSampleDrawn` handler enforces three pre-conditions: not soft-deleted `PatientTest` (`Conflict`), not soft-deleted `Patient` (`Conflict`), and `!IsTakenOutsideLab` (FR-M21-001 — `Conflict` with the message `"تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب"`). No new Domain mutator (the `PatientTest.MarkSampleDrawn(DateTime)` mutator is the F5 schema baseline; M-02-S1's soft-delete guard on it is the only schema-related change M-21 depends on). No new migration. S2 is the pure-documentation close-out: flip the M-21 row on the tracking sheet, append ADR-0033 (or next free) recording the M-21 permission-gate reuse, and produce `Handoff_M21.md` per the template.

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
- Execution order: strictly sequential S1 -> S2, no parallel slices. **M-21 ships after M-02 — M-02's slices must have closed and the M-02 code (including the M-02-S1 migration `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs` adding the `IsDeleted` column on `Patients` and the `(PatientId, IsSampleDrawn)` non-unique index on `PatientTests`) must be deployed in the codebase before S1 starts** (real-world code-deployment dependency). M-21 does not call any M-02 command; it only depends on the M-02-S1 schema additions, the M-02 access-policy constant shape (mirrored, not imported), and the M-02 reuse of the `ADD_EDIT_PATIENT` permission code.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/inspection) replaces any standard UI journey — M-21 has no UI in scope here.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-21] Slice N/Total: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-21's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | M-21 S1: queries and `MarkSampleDrawnCommand` + tests: build zero/zero; new tests + 529-baseline + M-01 + M-02-S1/S2/S3 green; `GetPatientsWithUncollectedSamples` returns patients with at least one un-drawn in-lab test, excludes patients with only outside-drawn tests, excludes patients with only drawn tests, excludes soft-deleted patients, optional `Day` filter works; `GetPatientTestsForDraw` returns the two groups correctly and resolves the test name; `MarkSampleDrawn` happy path (sets `IsSampleDrawn=true`, `SampleDrawnAtUtc=now`), unknown test -> `NotFound`, test on soft-deleted patient -> `Conflict`, test that is `IsTakenOutsideLab=true` -> `Conflict` with the specific message, double-draw idempotent success; `MarkAllSamplesDrawnForPatient` happy path, only `!IsTakenOutsideLab` and `!IsSampleDrawn` rows touched, saves exactly once; one authorization theory class asserts every write carries `RequiredPermissionCode => "ADD_EDIT_PATIENT"`; no new Domain or Infrastructure changes; diff touches only M-21's feature folder, the test project, and the test fake; no new DI registrations; no Presentation | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Application.Tests` |
| 2 | VG-02 | M-21 S2: close-out: tracking sheet M-21 row flipped to 🟩 Done with a dated change-log row; `Handoff_M21.md` produced per `Top_Lab_Handoff_Template.md`; ADR-0033 (or next free) recorded in `Docs/Source/Top_Lab_ADR.md` for the M-21 permission-gate reuse decision; no code changes; diff touches only `Docs/Source/Top_Lab_Master_Tracking_Sheet.md`, `Docs/Handoff_M21.md` (new), `Docs/Source/Top_Lab_ADR.md` | diff inspection; `dotnet build TopLab.sln` (no code change expected to break); `dotnet test TopLab.sln` still green |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | M-21 application: queries and `MarkSampleDrawnCommand` + tests | [x] Done | VG-01 |
| 2 | M-21 close-out (tracking flip + handoff + ADR-0033) | [x] Done | VG-02 |

---

## Slice 1: M-21 application: queries and `MarkSampleDrawnCommand` + tests

- **Goal:** Build the M-21 Application feature folder with the two queries and the two commands the phlebotomist screen will consume. The Domain `PatientTest.MarkSampleDrawn(DateTime)` mutator already exists (F5 baseline, with the M-02-S1 soft-delete guard); the Application layer is the new surface. M-21 reuses the M-02 `ADD_EDIT_PATIENT` permission code (settled OD-8) via its own `SampleCollectionAccessPolicy.AddEditPatient` constant (mirrored, not imported). No new Domain or Infrastructure changes.
- **Touches:** `src/TopLab.Application/Features/SampleCollection/Common/SampleCollectionDtos.cs` (create — `PatientWithUndrawnTestsDto`, `PatientTestDrawDto`, `SampleDrawBoardDto`); `src/TopLab.Application/Features/SampleCollection/Common/SampleCollectionAccessPolicy.cs` (create — `public const string AddEditPatient = "ADD_EDIT_PATIENT";`); `src/TopLab.Application/Features/SampleCollection/Queries/GetPatientsWithUncollectedSamples/` (create — Query + Handler, returns list of patients with at least one `PatientTest` where `!IsSampleDrawn && !IsTakenOutsideLab` and the patient is not soft-deleted, ordered by `RegistrationDateUtc asc`, optional `DateOnly? Day` filter defaulting to today UTC, page size ≤ 500, no `IAuthorizedRequest`); `src/TopLab.Application/Features/SampleCollection/Queries/GetPatientTestsForDraw/` (create — Query + Handler, returns Drawn and NotDrawn lists for a patient with `Test.Name` resolved via M12 `Test` set, no `IAuthorizedRequest`); `src/TopLab.Application/Features/SampleCollection/Commands/MarkSampleDrawn/` (create — Command + Handler + Validator, `MarkSampleDrawnCommand(int PatientTestId, DateTime DrawnAtUtc)`, rejects `NotFound`/`Conflict` per the three pre-conditions, calls `patientTest.MarkSampleDrawn(_dateTime.UtcNow)`, `IAuthorizedRequest` -> `SampleCollectionAccessPolicy.AddEditPatient`); `src/TopLab.Application/Features/SampleCollection/Commands/MarkAllSamplesDrawnForPatient/` (create — Command + Handler, iterates the patient's `NotDrawn` list, calls `MarkSampleDrawn` on each, saves once, same `AddEditPatient` permission); `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` (extend if missing `Remove<PatientTest>(...)` branch); `tests/TopLab.Application.Tests/Features/SampleCollection/` (create — `GetPatientsWithUncollectedSamplesQueryHandlerTests`, `GetPatientTestsForDrawQueryHandlerTests`, `MarkSampleDrawnCommandHandlerTests`, `MarkAllSamplesDrawnForPatientCommandHandlerTests`, `SampleCollectionAuthorizationTests`)
- **Validation Gate:** VG-01 — build 0/0; new + 529-baseline + M-01 + M-02-S1/S2/S3 green; FR-M21-001 invariant asserted explicitly (outside-drawn test excluded from "un-drawn" list and `MarkSampleDrawn` refuses to draw it); no new Domain/Infrastructure changes; diff touches only M-21's feature folder, the test project, the test fake; no new DI registrations; no Presentation.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln -m:1` 0/0; `dotnet test TopLab.sln -m:1` 270 + 647 + 79 green. **Precondition verified: M-02 code present** — `Patient.IsDeleted`/`SoftDelete`/`Restore`, `PatientTest.UpdateSampleFlags`, `PatientRegistrationAccessPolicy.AddEditPatient = "ADD_EDIT_PATIENT"` all exist in the codebase. Recorded M02 deviation applies (Handoff_M02): `PatientTest` has no `IsDeleted` flag, so the soft-delete guard lives in the M21 handler via the owning `Patient`.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §5.1 (M21-S1) + §3.1 (M02 inlined contracts); two-query split is settled OD-7 (distinct from M-02's `SearchPatientsQuery` which is name/phone search); `MarkSampleDrawn` pre-conditions enforced: owning `Patient` not soft-deleted (`Conflict`), `!IsTakenOutsideLab` (`Conflict` with the specific FR-M21-001 message); `IsTakenOutsideLab` is set at order time via M-02's `UpdatePatientTestSampleFlags` and is not changed from the M-21 screen; double-draw is idempotent (M17 `Re-issuing commands` precedent); `MarkAllSamplesDrawnForPatient` saves exactly once; reads are unauthorized; writes are `IAuthorizedRequest` -> `SampleCollectionAccessPolicy.AddEditPatient` (reuses M-02's `ADD_EDIT_PATIENT`, settled OD-8).
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `IApplicationDbContext` (read), `FakeApplicationDbContext` (read — `Remove<PatientTest>` branch already present, no fake change needed), M-02-S1 `Patient.cs` (read — `IsDeleted`, `SoftDelete`, `Restore`), M-02-S1 `PatientTest.cs` (read — `UpdateSampleFlags`, `MarkSampleDrawn` with NO soft-delete guard; deviation recorded in Handoff_M02 confirmed), M-12 `Test` (read — `Name` for `PatientTestDrawDto` resolution), M-02 `PatientRegistrationAccessPolicy` (read — `AddEditPatient = "ADD_EDIT_PATIENT"`), M-17 `DeactivateUserCommand` / `ReactivateUserCommand` (idempotent-mutator precedent noted), `PatientRegistrationAuthorizationTests` (read — authorization theory class precedent), `Result`/`Error` (read — `Result<bool>`/`Result<int>`/`Result<T>` + `NotFound`/`Conflict`/`Forbidden`), `IDateTimeProvider` + `FakeDateTimeProvider` (read), `SearchPatientsQueryHandler` (query precedent), `ClearAllTestsCommandHandler` (iterate + save-once + `المريض محذوف.` precedent), `StronglyTypedId` (value equality confirmed for GroupBy/Contains), `DependencyInjection` (validators auto-registered, no DI change needed).
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: (1) DTOs; (2) `SampleCollectionAccessPolicy.cs`; (3) 2 query files; (4) 2 command files (commands/handlers/validators); (5) fake extension if needed (not needed); (6) 4 handler test classes + 1 authorization theory class.
- [x] **Stage 5 — Execution:** Slice implemented per plan. Created 14 production files under `SampleCollection/` + 5 test files (20 tests). Design notes: `GetPatientsWithUncollectedSamplesQuery(DateOnly? Day, Page, PageSize)` defaults Day to today UTC, orders `RegistrationDateUtc asc`; `GetPatientTestsForDraw` groups ALL rows into Drawn (`IsSampleDrawn`) / NotDrawn (`!IsSampleDrawn`, incl. outside-drawn which M21 refuses to draw); `MarkSampleDrawn` uses `_clock.UtcNow` per plan, idempotent double-draw returns success with no save; `MarkAllSamplesDrawnForPatient` returns the marked count, saves exactly once, `Success(0)` with no save when nothing pending.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`. Evidence: `dotnet build TopLab.sln -m:1` 0/0; SampleCollection filter 20/20; full suite 270 + 667 + 79 green.
- [x] **Stage 7 — Validation Gate:** VG-01 passed. Evidence: build/test output above; FR-M21-001 invariant asserted (`Excludes_PatientWithOnlyOutsideDrawnTests_FR_M21_001`, `OutsideDrawnTest_ReturnsConflict_WithSpecificMessage_FR_M21_001`); `MarkAllSamplesDrawnForPatient` saves exactly once asserted (`HappyPath_MarksOnlyEligibleRows_SavesExactlyOnce`); auth theory asserts both writes carry `ADD_EDIT_PATIENT`; diff touches only `src/TopLab.Application/Features/SampleCollection/` + `tests/TopLab.Application.Tests/Features/SampleCollection/` (new files only; no Domain/Infrastructure/fake/DI/Presentation changes).
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-21] Slice 1/2: M-21 application: queries and MarkSampleDrawnCommand + tests — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.` — on `main`, never push.

---

## Slice 2: M-21 close-out (tracking flip + handoff + ADR-0033)

- **Goal:** Mark the M-21 row in the tracking sheet as "🟩 Done" with a dated change-log row; produce `Handoff_M21.md` per the template; append ADR-0033 (or next free) recording the M-21 permission-gate reuse decision. No new code; pure documentation.
- **Touches:** `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (modify — flip M-21 row to "🟩 Done" with a §9 change-log row dated 2026-09-07); `Docs/Handoff_M21.md` (create per `Top_Lab_Handoff_Template.md` — sections: Session Header, Objective, Achievements (M-21-S1 + M-21-S2), State of the Codebase (build 0/0, full suite green, no new migrations, no new DI), Decisions (settled OD-7 two-query split, settled OD-8 reuse of `ADD_EDIT_PATIENT` for M-21 writes — no new code added, "outside-drawn tests are read-only from the M-21 screen" — encoded as the FR-M21-001 invariant), Work in Progress (none), Deviations and Waivers (none), Required Reading (`Handoff_M02.md` for the integrated M-02 schema and access-policy shape; `Handoff_M14.md` for `ExternalEntity` reference)); `Docs/Source/Top_Lab_ADR.md` (modify — append ADR-0033 or next free: M-21 permission-gate reuse of `ADD_EDIT_PATIENT` rather than introducing a new code; the re-use pattern that settled OD-8 applied to M-02's `SoftDeletePatient → DELETE_PATIENT` is applied here to M-21's `MarkSampleDrawn → ADD_EDIT_PATIENT`).
- **Validation Gate:** VG-02 — handoff document produced; tracking sheet row updated; ADR-0033 (or next free) recorded; no code changes in S2; build + tests still green.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln -m:1` 0/0; `dotnet test TopLab.sln -m:1` 270 + 667 + 79 green (S1 commit `ea382dc` verified clean).
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §5.2 (M21-S2); pure documentation — no Application/Domain/Infrastructure code changes; S1's diff stays as it landed (`ea382dc`); ADR-0033 is for traceability of the permission-gate reuse, not a behavioral change; handoff "Required Reading" cites `Handoff_M02.md` + `Handoff_M14.md` + ADR-0033; change-log/tracking dates use the factual execution date 2026-09-08 (the plan's 2026-09-07 was written assuming same-day execution).
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `Top_Lab_Master_Tracking_Sheet.md` (read — §4 M21 row line 71, §5 Wave 4 row line 97, §9 log format line 369; §6 blocks left untouched per M02 close-out precedent), `Handoff_M02.md` (read in full — handoff structure precedent), `Top_Lab_ADR.md` (read — ADR-0032 tail, ADR-0033 is next free), `ValidatorRegistrationTests.cs` (read — explicit per-module InlineData lists; M21 validators NOT added there because VG-02 mandates no code changes in S2; assembly scanning covers them).
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: (1) flip tracking-sheet §4 M21 row to 🟩 Done + §5 Wave 4 row to 🟩 Done (both members Done, per the sheet's own wave rule) + §9 change-log row; (2) write `Docs/Handoff_M21.md` per the Handoff_M02 structure (incl. one honest deviation entry: `PatientTest` has no `IsDeleted`, guard enforced via owning `Patient` per the recorded M02 deviation); (3) append ADR-0033; (4) verify build + tests still green.
- [x] **Stage 5 — Execution:** Slice implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings` (docs only, nothing to break). Evidence: build 0/0; full suite 667 + 79 (+ 270 Domain, unchanged) green.
- [x] **Stage 7 — Validation Gate:** VG-02 passed. Evidence: `Docs/Handoff_M21.md` produced; tracking-sheet §4/§5/§9 updated; ADR-0033 recorded; `git diff --stat` shows only the 2 modified docs (+ `Handoff_M21.md` new); zero `src/`/`tests/` changes in S2.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-21] Slice 2/2: M-21 close-out (tracking flip + handoff + ADR-0033) — loop-engineering` + `Stages 1-10 verified. Gate VG-02 passed.` — on `main`, never push.

---

## Current Status

- Overall: 2/2 slices done. Module M-21 complete.
- Slice 1 — M-21 application: queries and `MarkSampleDrawnCommand` + tests: [x] Done (VG-01 passed, commit `ea382dc`)
- Slice 2 — M-21 close-out (tracking flip + handoff + ADR-0033): [x] Done (VG-02 passed)

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-07 | 0 | — | Memory file created | OK | — |
| 2026-09-08 | 1 | 1–9 | S1 implemented: 14 production files + 5 test files (20 tests); build 0/0; full suite 270+667+79 green; VG-01 passed | OK | — |
| 2026-09-08 | 1 | 10 | S1 committed locally on `main` | OK | `ea382dc` |
| 2026-09-08 | 2 | 1–9 | S2 executed: tracking §4/§5/§9 updated, `Handoff_M21.md` created, ADR-0033 appended; build 0/0; full suite green; VG-02 passed | OK | — |

## Stop Report (append only if a stop condition triggers)
