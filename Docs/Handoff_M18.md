# Top-Lab — Handoff Document M-18

## نظام توب لاب — تسليم جلسة عمل (Module 18 — Attendance & Time Tracking)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-15_M18-attendance-time-tracking` |
| Session date (UTC) | 2026-09-15 |
| Session start (UTC) | 2026-09-15 |
| Session end (UTC) | 2026-09-15 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-18 |
| Module name | Attendance & Time Tracking |
| Wave | 8 |
| Feature folder(s) touched | `src/TopLab.Domain/Attendance/**`, `src/TopLab.Application/Features/Attendance/**`, `tests/TopLab.Domain.Tests/Attendance/`, `tests/TopLab.Application.Tests/Features/Attendance/`, `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs`, `tests/TopLab.Infrastructure.Tests/Persistence/AttendanceRecordPersistenceTests.cs` |
| Layers touched | Domain + Application + Infrastructure-proof tests (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `c25352b` (live `main` HEAD after M-10) |
| Final commit at session end | `[M-18] Slice 4/4: Tests + Infrastructure proof + close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 18 **Attendance & Time Tracking** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-18.md` (Domain guards + calculator → Application write surface → Application read surface → persistence proof and close-out). The work is **backend only** — no Presentation content. S1 adds additive break-sequencing/double-check-out guards to the factory-light `AttendanceRecord` and ships the single-sourced static `AttendanceCalculator` (lateness at check-in, overtime at check-out vs the user's configured hours, worked minutes excluding the break span). S2 ships the four ungated session-bound self-service write commands (check-in with single-open-record guard + computed lateness, break start/end, check-out with computed overtime). S3 ships the two manager-only reads (period records with optional user filter, per-user summary with calculator totals) gated by a handler-level `IsAbsolutePermission` check. S4 proves zero drift (no migration), pins Cascade + index + not-auditable, appends ADR-0044, flips the tracking sheet, creates this handoff, and confirms full-suite green. Build must be 0 errors / 0 warnings; full suite green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain: guards on AttendanceRecord + the AttendanceCalculator** — Implementation Complete — additive guards in `StartBreak` (open break → "Break is already started.", post-checkout → "Already checked out."), `EndBreak` (no open break → "No open break to end."), `CheckOut` (open break → "Cannot check out with an open break.", double → "Already checked out."), all English `InvalidOperationException` (`PatientTest` precedent); new pure static `AttendanceCalculator` (`LatenessMinutes`/`OvertimeMinutes` via `ToLocalTime` wall-clock, null when unconfigured, never negative; `WorkedMinutes` subtracting the break span when both ends set, clamped ≥ 0); 2 test classes (23 tests: every guard negative-pathed + happy paths; late/on-time/early/boundary/null, overtime zero/positive/boundary/null, worked with/without/partial break + cross-midnight documentation test). Commit `[M-18] Slice 1/4: Domain guards on AttendanceRecord + the AttendanceCalculator — loop-engineering` (`3070c6e`).
- **S2 — Application write surface: check-in + break + check-out** — Implementation Complete — `AttendanceRecordDto` (`FromRecord` with null-until-checkout `WorkedMinutes`); `DomainFailureTranslator` (four arms + fallback); four parameterless commands with no `IAuthorizedRequest` (SD-18-2), unauthenticated → shared Forbidden, `Create(0)` IDENTITY sentinel, single save; `CheckIn` (single-open-record Conflict + lateness via calculator, null when unconfigured or user row missing); `StartBreak`/`EndBreak` (open-record NotFound + guard translation); `CheckOut` (overtime via calculator + open-break translation); `FakeApplicationDbContext` verify-only (no gap — `AttendanceRecords` + `Users` routing already present); 3 test classes (27 tests: happy paths, computed + null lateness/overtime, on-time zero, missing-user null, open-record Conflict, NotFound, all four translator arms + fallback, DTO mapping, unauthenticated Forbidden, validators valid). Commit `[M-18] Slice 2/4: Application write surface check-in + break + check-out — loop-engineering` (`908f05c`).
- **S3 — Application read surface: manager-only period records + per-user summary** — Implementation Complete — `GetAttendanceRecords` (handler-level absolute gate with verbatim shared message, DateTime half-open bounds = inclusive UTC calendar days, default-today-UTC via `IDateTimeProvider`, optional user filter, Skip/Take, dictionary user-name resolution with empty-string fallback, no `Include`) + validator (`From ≤ To`, paging bounds); `UserAttendanceSummaryDto` in Common + `GetUserAttendanceSummary` (absolute gate, unknown user → `NotFound("المستخدم غير موجود.")`, same period semantics, days-present distinct count + records count + calculator totals) + validator (`UserId > 0` mirroring NotFound text, `From ≤ To`); 2 test classes (13 tests: verbatim Forbidden ×2, inclusive-both-ends boundary instants, user filter, empty-set/zero-totals, default-today, unknown-name fallback, totals number-for-number vs the calculator incl. an open record contributing lateness but zero worked, validator matrices incl. null-period arms). Zero `IAuthorizedRequest` under Attendance (grep-pinned). Commit `[M-18] Slice 3/4: Application read surface manager-only period records + per-user summary — loop-engineering` (`e686dfa`).
- **S4 — Tests + Infrastructure proof + close-out** — Implementation Complete — `AttendanceRecordPersistenceTests` (5 InMemory-on-real-`ApplicationDbContext` tests: full-lifecycle store/retrieve, behavioral Cascade delete user→records, model-level Cascade FK pin on `UserId`, `UserId` index pin, not-auditable pin asserting no audit columns are mapped); M18 validator-registration theory (6 cases); zero-drift gate clean (`has-pending-model-changes` → no changes, snapshot untouched); Release build 0/0; full Release suite green 1789 (`-m:1`); ADR-0044 appended; tracking sheet M18 row flipped to Done + dated change-log row; `Handoff_M18.md` created; zero Presentation content confirmed.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Release).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -c Release -m:1`): **1789 green** = 413 Domain.Tests + 1222 Application.Tests + 154 Infrastructure.Tests.
- New tests added across S1–S4: S1 +23 Domain; S2 +27 Application (3 handler-test classes); S3 +13 Application (2 query-test classes); S4 +5 Infrastructure (`AttendanceRecordPersistenceTests`) + 6 validator-registration cases.
- Tests currently failing: none.
- Coverage: per-slice gates passed (VG-01 Domain Attendance scope: Record line 0.9318/branch 1.0, Calculator line 1.0/branch 0.9285; VG-02 Application S2 footprint: handlers 1.0, translator 1.0/1.0, DTO 0.903; VG-03 Application S3 footprint: handlers 1.0/1.0, validators 1.0 lines, DTOs ≥0.93; VG-04 persistence 5/5 green). Whole-project floors are inapplicable for M-18 (the module touches a subset of files); waiver recorded per the M-11/M-14 precedent.

### 4.3 Migrations

- New EF Core migration(s) added: **None** (`AttendanceRecords` table, Cascade FK to `User`, `UserId` index, and DbSet all exist at the F5 baseline).
- `has-pending-model-changes` at close-out: No changes — zero drift. Snapshot unchanged.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: none (handlers take the existing `IApplicationDbContext`/`ICurrentUserService`/`IDateTimeProvider`; MediatR + validator assembly scan cover the new types).
- Validators: 6 new validators (`CheckInCommandValidator`, `StartBreakCommandValidator`, `EndBreakCommandValidator`, `CheckOutCommandValidator`, `GetAttendanceRecordsQueryValidator`, `GetUserAttendanceSummaryValidator`) resolve via the existing assembly scan; resolution pinned by the M18 validator-registration theory; no DI wiring change.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the `PermissionConfiguration.cs` seed: **none** — no attendance code exists in the 13-code catalog and none was added (SD-18-2/3).

---

## 5. Work In Progress (Required — mark "None" if none)

None. All four slices reached a terminal state; module closed out in the Master Tracking Sheet (§4 M18 row + §9 change-log row), ADR-0044, and this handoff. Deferred scope (out of this plan): edit/correct/void of attendance records (SD-18-9 — needs a mutator + owner decision); per-day breakdown rows in the summary (U2 — totals only shipped); Presentation-layer attendance screens consuming the queries/commands; M-19 consumption of the read semantics.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

All were settled autonomously in the plan (Finality Assessment) and are recorded in ADR-0044 (no re-derivation):

- **Decision (zero storage change — SD-18-1):** Entity + configuration + DbSet + baseline table verified complete at HEAD; zero migrations.
  - **Scope of impact:** Whole M-18 diff (no `Persistence/**` change by construction; zero-drift gate).
  - **Follow-up required:** No.
- **Decision (ungated session-bound writes — SD-18-2):** No attendance code in the 13-code catalog; PRD restricts viewing only; M-03 ungated-writes precedent.
  - **Scope of impact:** 4 write commands (no `IAuthorizedRequest`; `_currentUser.UserId` only).
  - **Follow-up required:** No.
- **Decision (handler-level absolute gate on reads — SD-18-3):** Data Model §9.4 mandates application-layer manager-only enforcement; no code exists to reference, so the gate lives in the handler with the verbatim shared message.
  - **Scope of impact:** 2 query handlers; verbatim-message tests.
  - **Follow-up required:** No.
- **Decision (event-time lateness/overtime vs configured hours; local wall-clock — SD-18-4):** Data Model "computed at check-in/check-out"; factory signatures already accept computed values; single-site LAN. Consistency-based choice, ADR-recorded.
  - **Scope of impact:** `AttendanceCalculator` + check-in/check-out handlers; S1 boundary tests.
  - **Follow-up required:** No (consumer note: overnight-shift overtime is measured against the same calendar day's `WorkEndTime`).
- **Decision (single open record, logical uniqueness — SD-18-5):** M-16 D40 precedent; no new DB unique index.
  - **Scope of impact:** `CheckInCommandHandler` + Conflict test.
  - **Follow-up required:** No.
- **Decision (single-sourced calculator — SD-18-7):** M-16 D39 precedent; worked minutes exclude the recorded break span.
  - **Scope of impact:** All time math; single-source grep gate.
  - **Follow-up required:** No.
- **Decision (no edit/delete of records — SD-18-9):** No requirement; M-16 OD-16-B deferral precedent.
  - **Scope of impact:** Whole M-18 diff (no modification path by construction; grep-pinned absence).
  - **Follow-up required:** Later owner decision if corrections are ever required.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** None. `FakeApplicationDbContext` needed no extension (`AttendanceRecords` + `Users` List/Set/Add/Remove routing already present — verify-only as the plan allowed). The S4 plan file name `GetUserAttendanceSummaryValidator.cs` (without `Query`) was followed exactly. No test-environment workarounds were needed (single-add `Create(0)` per context; saves between adds).
- **Waiver:** Whole-project coverage floors (Domain ≥90%, Application ≥80%, Infrastructure ≥70%) are inapplicable for M-18 because the module touches a subset of files in each project. Per-slice footprint coverage gates were verified (VG-01 through VG-04 all PASS). This waiver follows the M-11/M-14 precedent where whole-project floors are replaced by footprint posture.
- **Slopwatch:** Zero issues in M-18 files (2 pre-existing findings in untouched files: a `#pragma` notice in the F5 baseline migration and an empty-catch error in a Presentation service — both out of scope, untouched).
- **Consumer note (M-19):** `GetAttendanceRecords` (period + optional user filter, resolved names) and `GetUserAttendanceSummary` (days/records/worked/lateness/overtime totals) are the consumption surface; attendance correlation remains optional per M-19's own plan.

---

## 8. Required Reading (Required — mark "None" if none)

- **M-18 Implementation Plan** — `Docs/OpenCode/M-18.md` (this session's source of truth; §5–§7, Appendix A).
- **M-18 Loop-Engineering Memory** — `Docs/OpenCode/M-18-memory.md` (slice index, gates VG-01..VG-04, per-slice 10-stage checklists, execution log).
- **ADR-0044** — `Docs/Source/Top_Lab_ADR.md` (zero-storage, ungated writes, absolute-gated reads, computation rules, single open record, no-edit, zero-drift).
- **M-17 Handoff** — for `User` working-hours/break configuration (`WorkStartTime`/`WorkEndTime` + `SetWorkingHours`/`SetBreakPeriod`) consumed read-only.
- **M-16 Handoff** — `Docs/Handoff_M16.md` (for the guard/calculator/translator/half-open-bounds/dictionary-resolution precedents emulated by M-18).
- **M-10 Handoff** — `Docs/Handoff_M10.md` (for the close-out convention: ADR numbering, tracking flip, handoff structure).

---

*End of document.*
