# Loop Engineering — Memory File

- **Module:** Attendance & Time Tracking (M-18)
- **Module Number:** M-18
- **Source Plan:** Docs/OpenCode/M-18.md
- **Date Created:** 2026-09-15
- **Total Slices:** 4
- **Current Slice:** S3 — pending
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Delivers attendance & time tracking over the already-complete physical schema (zero migration): additive Domain guards on `AttendanceRecord` (break sequencing, double check-out) plus the single-sourced static `AttendanceCalculator` (lateness at check-in, overtime at check-out vs the user's configured hours, worked minutes excluding the break span); four session-bound self-service write commands (`CheckIn` with single-open-record guard, `StartBreak`, `EndBreak`, `CheckOut`); two manager-only reads (period records, per-user summary) gated by a handler-level `IsAbsolutePermission` check (no attendance permission code exists — none added). No edit/delete of records in v1. Zero Presentation content. Done means: S1 Domain plus tests, S2 writes plus tests, S3 reads plus tests, S4 persistence proof (Cascade pin) plus zero-drift gate plus ADR-0044 plus tracking flip plus handoff plus full-suite green plus coverage floors.

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
- Stop threshold: 5 consecutive failures for the same reason.
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/model-assertion) replaces any standard UI journey — M-18 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-18] Slice N/4: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-18's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain: `src/TopLab.Domain` builds zero/zero; `AttendanceRecordTests` + `AttendanceCalculatorTests` plus all Domain tests green; every guard negative-pathed; calculator branches covered (late/on-time/boundary, overtime, worked-minutes with/without break); Domain `Attendance` coverage ≥ 90%. **Migration: none required by this slice** | `dotnet build src/TopLab.Domain`; `dotnet test tests/TopLab.Domain.Tests`; coverlet module filter |
| 2 | VG-02 | Application writes: `src/TopLab.Application` builds zero/zero; all S2 handler/validator tests green (single-open-record Conflict, session-bound recording, lateness/overtime computed + null-when-unconfigured, guard-to-message translation, unauthenticated Forbidden); grep gate: no time math outside `AttendanceCalculator`; zero `Persistence/**` diff; Application S2 footprint coverage ≥ 80%. **Migration: none required by this slice** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 3 | VG-03 | Application reads: Application builds zero/zero; S3 tests green (non-absolute Forbidden verbatim, period inclusive both ends, user filter, empty-set, default-today, `From > To` validator, summary number-for-number vs calculator, unknown user NotFound); grep gates: absolute check present in both query handlers; no formula restatement; Application S3 footprint coverage ≥ 80%. **Migration: none required by this slice** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 4 | VG-04 | Persistence + close-out: Release build zero/zero; full suite green (`-m:1`); `AttendanceRecordPersistenceTests` green (store/retrieve + Cascade record→user pin); `dotnet ef migrations has-pending-model-changes` → no changes; snapshot unchanged; validator-registration extension green; coverage floors met or waived; ADR-0044 appended; M18 tracking row flipped; `Handoff_M18.md` per template; zero edit/delete path (grep gate); zero Presentation content (grep gate) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain: guards on AttendanceRecord + the AttendanceCalculator | [x] Done | VG-01 |
| 2 | Application write surface: check-in + break + check-out | [x] Done | VG-02 |
| 3 | Application read surface: manager-only period records + per-user summary | [ ] Pending | VG-03 |
| 4 | Tests + Infrastructure proof + close-out | [ ] Pending | VG-04 |

---

## Slice 1: Domain: guards on AttendanceRecord + the AttendanceCalculator

- **Goal:** Add the additive guards to `StartBreak`/`EndBreak`/`CheckOut` and create the pure static `AttendanceCalculator` (the single source of the time math) — no breaking change.
- **Touches:** `src/TopLab.Domain/Attendance/AttendanceRecord.cs` (modify: additive guards only); `src/TopLab.Domain/Attendance/AttendanceCalculator.cs` (create); `tests/TopLab.Domain.Tests/Attendance/AttendanceRecordTests.cs` (create); `tests/TopLab.Domain.Tests/Attendance/AttendanceCalculatorTests.cs` (create)
- **Validation Gate:** VG-01 — Domain build zero/zero; all Domain tests green; every guard negative-pathed; coverage ≥ 90%. Migration: none.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` 0/0; `dotnet test TopLab.sln` full suite green before touching anything. (390 Domain + 149 Infra + 1176 App passed.)
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1 re-read; SD-18-1…SD-18-9; English `InvalidOperationException` precedent.
- [x] **Stage 3 — File Analysis:** `AttendanceRecord.cs` (6-53), `User.cs:20-26` (work-hours config), `PatientTest.cs` guard precedent, `SentOutAccountCalculator.cs` style precedent, xUnit conventions.
- [x] **Stage 4 — Planning:** Guards → calculator (3 static members) → 2 test classes, encoded in this checklist.
- [x] **Stage 5 — Execution:** Implement per plan. Guards: StartBreak (open-break "Break is already started." / post-checkout "Already checked out."); EndBreak (no-open-break "No open break to end."); CheckOut (open-break "Cannot check out with an open break." / double "Already checked out."). Calculator: LatenessMinutes/OvertimeMinutes (null when unconfigured, max(0, local-config) via ToLocalTime wall-clock) + WorkedMinutes (raw span minus break span when both set, clamped ≥0).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build src/TopLab.Domain` 0/0; Domain tests green (413 passed = 390 + 23 new).
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — Domain build 0/0; 413 Domain tests green; every guard negative-pathed (double-start, end-without-start, double-end, checkout-with-open-break, double-checkout, start-after-checkout); calculator branches covered (late/on-time/boundary/early/null, overtime zero/positive/boundary/null, worked with/without/partial break + cross-midnight doc test); coverlet Attendance scope: Calculator line 1.0/branch 0.9285, Record line 0.9318/branch 1.0 (≥90%); zero `Persistence/**` diff (grep gate clean). Migration: none.
- [x] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 2: Application write surface: check-in + break + check-out

- **Goal:** Ship the four session-bound commands with the single-open-record guard, computed lateness/overtime via the calculator, and the full test matrix.
- **Touches:** `src/TopLab.Application/Features/Attendance/Common/AttendanceDtos.cs` (create); `.../Common/DomainFailureTranslator.cs` (create); `.../Commands/CheckIn/` (3 files, create); `.../Commands/StartBreak/` (3 files, create); `.../Commands/EndBreak/` (3 files, create); `.../Commands/CheckOut/` (3 files, create); `tests/TopLab.Application.Tests/Features/Attendance/CheckInCommandHandlerTests.cs` (create); `.../BreakCommandHandlerTests.cs` (create); `.../CheckOutCommandHandlerTests.cs` (create); `FakeApplicationDbContext` (verify-only / extend on proven gap)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green; calculator-single-source grep gate; coverage ≥ 80%. Migration: none.

### 10-Stage Progress (Slice 2)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` 0/0; `dotnet test TopLab.sln` full suite green (413 + 149 + 1176).
- [x] **Stage 2 — Deep Understanding:** Plan §6 S2 re-read; FR-M18-001; EC-01…EC-08; verbatim Appendix A messages.
- [x] **Stage 3 — File Analysis:** `AttendanceRecordConfiguration.cs` (`Create(0)` IDENTITY sentinel), `ICurrentUserService` (+`AuthorizationBehavior` shared Forbidden literal), `FakeCurrentUserService`/`FakeDateTimeProvider`/`FakeApplicationDbContext` (AttendanceRecords+Users already supported — verify-only, no extension), `SendSampleOut`/`RecordSentOutPayment`/`DeliverWithSettlement` handler precedents, `Error`/`Result` API, `InternalsVisibleTo` for translator tests.
- [x] **Stage 4 — Planning:** DTOs → translator → CheckIn → StartBreak/EndBreak → CheckOut → 3 test classes.
- [x] **Stage 5 — Execution:** Per plan. All 4 commands parameterless, no `IAuthorizedRequest` (SD-18-2); unauthenticated → shared Forbidden; CheckIn enforces single-open-record Conflict + computes lateness; Start/EndBreak resolve open record (NotFound) + translate Domain guards; CheckOut computes overtime; single `SaveChanges`; `AttendanceRecordDto.FromRecord` (WorkedMinutes null until checkout, else via calculator).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build src/TopLab.Application` 0/0; S2 filter 27 green; full solution build 0/0; full suite green (413 + 149 + 1203).
- [x] **Stage 7 — Validation Gate:** VG-02 PASS — App build 0/0; 27 S2 tests green (Conflict, session-bound UserId, lateness/overtime computed + null-when-unconfigured + null-when-missing-user, guard-to-message translation incl. direct translator arm test, unauthenticated Forbidden, validators valid, DTO mapping); grep gate: no time math outside `AttendanceCalculator` (3 refs only); zero `Persistence/**` diff (0 files); coverlet S2 footprint: handlers 1.0 lines, translator 1.0/1.0, DTO 0.903/1.0 (≥80%). Migration: none.
- [x] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 3: Application read surface: manager-only period records + per-user summary

- **Goal:** Ship the two absolute-gated reads: `GetAttendanceRecords` (period, optional user filter, resolved names) and `GetUserAttendanceSummary` (totals via the calculator).
- **Touches:** `src/TopLab.Application/Features/Attendance/Queries/GetAttendanceRecords/` (3 files, create); `.../Queries/GetUserAttendanceSummary/` (3 files, create); `tests/TopLab.Application.Tests/Features/Attendance/GetAttendanceRecordsQueryHandlerTests.cs` (create); `.../GetUserAttendanceSummaryQueryHandlerTests.cs` (create)
- **Validation Gate:** VG-03 — Application build zero/zero; S3 tests green; absolute-check + calculator grep gates; coverage ≥ 80%. Migration: none.

### 10-Stage Progress (Slice 3)

- [ ] **Stage 1 — Pre-Execution Verification:** build + full suite green.
- [ ] **Stage 2 — Deep Understanding:** Plan §6 S3 re-read; FR-M18-002; half-open bounds + default-today precedent.
- [ ] **Stage 3 — File Analysis:** `GetSentOutSamplesQueryHandler` (period + dictionary name resolution precedent), `GetSentOutSamplesQueryValidator` (paging messages), `User.UserName` verified.
- [ ] **Stage 4 — Planning:** `GetAttendanceRecords` (+validator) → `GetUserAttendanceSummary` (+validator) → 2 test classes.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Application build 0/0; Attendance filter green; full Application suite green.
- [ ] **Stage 7 — Validation Gate:** VG-03.
- [ ] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [ ] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 4: Tests + Infrastructure proof + close-out

- **Goal:** Prove the module against the real model (Cascade pin), run the zero-drift gate, extend validator registration, pass coverage/slopwatch, and close out (ADR-0044, tracking flip, handoff).
- **Touches:** `tests/TopLab.Infrastructure.Tests/Persistence/AttendanceRecordPersistenceTests.cs` (create); `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend); `Docs/Source/Top_Lab_ADR.md` (append ADR-0044 — reconfirm max ADR at execution; M-10 consumes 0043 when it executes first); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M18 row + dated change-log row); `Docs/Handoff_M18.md` (create per template)
- **Validation Gate:** VG-04 — Release build zero/zero; full suite green; zero-drift proven; persistence pins green; coverage floors or waivers; docs committed per convention; zero edit/delete path (grep); zero Presentation content (grep gate).

### 10-Stage Progress (Slice 4)

- [ ] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln -c Release` 0/0 before touching anything.
- [ ] **Stage 2 — Deep Understanding:** Plan §7 S4 re-read; drift → stop + addendum (never silent migration); ADR-0044 contents; close-out convention.
- [ ] **Stage 3 — File Analysis:** `AttendanceRecordConfiguration.cs` (Cascade line 21, index line 22); `InMemoryContextFactory` + `SentOutSamplePersistenceTests` pattern; `ValidatorRegistrationTests` per-module theory; ADR max reconfirmed; M18 tracking row + §9 log format; `Handoff_M16.md` structure precedent.
- [ ] **Stage 4 — Planning:** Drift gate first → persistence tests → validator-reg extension → Release full suite → ADR → tracking → handoff.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Release build 0/0; Release full suite green `-m:1`; drift gate → no changes; snapshot untouched.
- [ ] **Stage 7 — Validation Gate:** VG-04.
- [ ] **Stage 8 — Documentation Update:** ADR-0044 appended; M18 row flipped 🟩 Done + dated §9 row; `Docs/Handoff_M18.md` created per template; this checklist recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Current Status

- Overall: 2/4 slices done — S2 complete, S3 in progress
- Slice 1 — Domain: guards + calculator: [x] Done (VG-01 pass, committed)
- Slice 2 — Application write surface: [x] Done (VG-02 pass, committed)
- Slice 3 — Application read surface: [ ] Pending
- Slice 4 — Tests + Infrastructure proof + close-out: [ ] Pending

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-15 | 0 | — | Memory file created | OK | — |
| 2026-09-15 | S1 | 1–10 | Domain guards + AttendanceCalculator; VG-01 pass (413 Domain green; Record 0.9318/Calc 1.0; zero Persistence diff) | OK | [M-18] Slice 1/4 |
| 2026-09-15 | S2 | 1–10 | Write surface (DTOs/translator/4 commands) + 27 tests; VG-02 pass (1203 App green; single-source grep clean; zero Persistence diff) | OK | [M-18] Slice 2/4 |

## Stop Report (append only if a stop condition triggers)
