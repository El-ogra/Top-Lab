# Top-Lab — Handoff Document M-21

## نظام توب لاب — تسليم جلسة عمل (Module 21 — Sample Collection & Separation)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-08_M21_sample-collection-separation` |
| Session date (UTC) | 2026-09-08 |
| Session start (UTC) | 2026-09-08 |
| Session end (UTC) | 2026-09-08 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-21 |
| Module name | Sample Collection & Separation (backend only) |
| Wave | 4 |
| Feature folder(s) touched | `src/TopLab.Application/Features/SampleCollection/`, `tests/TopLab.Application.Tests/Features/SampleCollection/` |
| Layers touched | Application (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `34ae9cc` (after M-02; the M-21 plan's reference commit `e7ca616` is an ancestor — the M02 code surface the plan depends on is present) |
| Final commit at session end | `(filled at commit time)` |

---

## 2. Session Objective (Required)

Implement Module 21 **Sample Collection & Separation** end-to-end in the two slices S1–S2 of `Docs/OpenCode/M-21.md` (Application queries + `MarkSampleDrawnCommand` + tests → documentation close-out), satisfying FR-M21-001 ("selecting a patient from the list on the right shows the samples to be drawn; clicking a drawn sample registers it as drawn and displays it at the bottom of the window"). The work is **backend only** — Application layer; no Domain change, no Infrastructure change, no migration, no Presentation content. S1 ships the two queries (`GetPatientsWithUncollectedSamplesQuery`, `GetPatientTestsForDrawQuery`) and the two commands (`MarkSampleDrawnCommand`, `MarkAllSamplesDrawnForPatientCommand`). Both writes are `IAuthorizedRequest` with `ADD_EDIT_PATIENT` (id 1, already seeded) — the M02 code is reused, no new permission row is added (settled OD-8, ADR-0033). Build must be 0 errors / 0 warnings; all tests green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — M21 application: queries and `MarkSampleDrawnCommand` + tests** — Implementation Complete — `Common/SampleCollectionDtos.cs` (`PatientWithUndrawnTestsDto` with `UndrawnCount`, `PatientTestDrawDto` with resolved `TestName`, `SampleDrawBoardDto` grouping `Drawn`/`NotDrawn` under a patient header); `Common/SampleCollectionAccessPolicy.cs` (`AddEditPatient = "ADD_EDIT_PATIENT"` — mirrors the M02 shape, settled OD-8); `Queries/GetPatientsWithUncollectedSamples/` (Query + Handler + Validator — optional `DateOnly? Day` defaulting to today UTC, `RegistrationDateUtc asc`, excludes soft-deleted patients / drawn tests / outside-drawn tests, page size ≤ 500, no `IAuthorizedRequest`); `Queries/GetPatientTestsForDraw/` (Query + Handler + Validator — all rows for a patient grouped into `Drawn` (`IsSampleDrawn`) / `NotDrawn` (`!IsSampleDrawn`, including outside-drawn rows which M21 refuses to draw), `Test.Name` resolved via the M12 `Test` set, unknown patient → empty board echoing `PatientId`, no `IAuthorizedRequest`); `Commands/MarkSampleDrawn/` (Command + Handler + Validator — `NotFound` for unknown test/patient, `Conflict` for soft-deleted owning patient, `Conflict` with the frozen FR-M21-001 message for outside-drawn rows, idempotent double-draw returning success with no save, time via `IDateTimeProvider`, `IAuthorizedRequest` → `AddEditPatient`); `Commands/MarkAllSamplesDrawnForPatient/` (Command + Handler + Validator — marks only `!IsSampleDrawn && !IsTakenOutsideLab` rows, single `SaveChangesAsync`, returns the marked count, `Success(0)` with no save when nothing is pending, same permission); no `FakeApplicationDbContext` change was needed (`Remove<PatientTest>` branch already present); 5 new test files, 20 tests. Commit `[M-21] Slice 1/2: ...`.

- **S2 — M21 close-out (tracking flip + handoff + ADR-0033)** — Implementation Complete — Master Tracking Sheet §4 M21 row → 🟩 Done with §9 change-log row dated 2026-09-08; §5 Wave 4 row → 🟩 Done (M02 and M21 both Done, per the sheet's own wave rule); ADR-0033 appended to `Docs/Source/Top_Lab_ADR.md` recording the permission-gate reuse; this handoff created. No code changes in S2. Commit `[M-21] Slice 2/2: ...`.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Debug, `-m:1` posture).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -m:1`): **1016 green** = 270 Domain.Tests + 667 Application.Tests + 79 Infrastructure.Tests.
- New tests added: +20 (all under `tests/TopLab.Application.Tests/Features/SampleCollection/`).
- Tests currently failing: none.

### 4.3 Migrations

- New EF Core migration(s) added: **None.** M21 has no schema dependency beyond the M02-S1 migration (`IsDeleted` column on `Patients`, `(PatientId, IsSampleDrawn)` index on `PatientTests`), which already exists in the codebase.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: None. The 5 new validators resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (assembly scanning, no DI wiring change).
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the 13-row `PermissionConfiguration.cs` seed: **none** — settled OD-8: `git diff src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` is empty.

---

## 5. Work In Progress (Required — mark "None" if none)

None. Both slices reached a terminal state; module closed out in the Master Tracking Sheet (§4/§5/§9) and this handoff. The natural next step (Presentation phlebotomist screen consuming the S1 queries/commands) is out of scope of this plan.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

The two formerly-open decisions (OD-7, OD-8) are settled by explicit owner confirmation of the V2 plan's own recommended options in every case (see M-21.md §2).

- **Decision (settled OD-7):** Two queries — `GetPatientsWithUncollectedSamplesQuery` (patient list, distinct from M02's name/phone `SearchPatientsQuery`) and `GetPatientTestsForDrawQuery` (per-patient Drawn/NotDrawn board).
  - **Reason:** The owner-approved V2 option is faithful to `RLS_Learn.pdf` §8 ("selecting a patient from the list on the right shows the samples to be drawn ... displays it at the bottom of the window").
  - **Scope of impact:** The two S1 query handlers; no M02 change.
  - **Follow-up required:** No.

- **Decision (settled OD-8):** M21 reuses the existing `ADD_EDIT_PATIENT` (id 1) code for both write commands. No new permission row is added.
  - **Reason:** The owner-approved V2 option validates the M17 seed design — the phlebotomist's draw-marking write is the same operational write as the registrator's.
  - **Scope of impact:** Both S1 write commands + `SampleCollectionAccessPolicy.AddEditPatient`; recorded in ADR-0033.
  - **Follow-up required:** No (encoded in the commands; the `git diff` against `PermissionConfiguration.cs` is empty).

- **Decision (FR-M21-001 invariant):** Outside-drawn tests are read-only from the M21 screen — excluded from the "un-drawn" patient list and refused by `MarkSampleDrawn` with the frozen message `"تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب"`. `IsTakenOutsideLab` is set at order time via M02's `UpdatePatientTestSampleFlags` and is never changed from the M21 screen.
  - **Reason:** The reference system's outside-drawn flag is an order-time fact, not a draw-time state.
  - **Scope of impact:** `GetPatientsWithUncollectedSamplesQueryHandler` filter + `MarkSampleDrawnCommandHandler` guard; pinned by the two `FR_M21_001` tests.
  - **Follow-up required:** No.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** The M-21 plan §5.1 describes the handler as asserting "the patient-test is not soft-deleted (rejects `Conflict`)" alongside the patient check — but `PatientTest` carries no `IsDeleted` flag (only `Patient` does, per the M02-S1 migration scope; recorded deviation in `Handoff_M02.md` §7). The soft-delete guard is therefore enforced once, via the owning `Patient.IsDeleted` lookup before calling `MarkSampleDrawn`.
  - **Waiver:** No waiver required — the implementation follows the as-built M02 baseline, consistent with the recorded M02 deviation.
  - **Pinned by:** `MarkSampleDrawnCommandHandlerTests.TestOnSoftDeletedPatient_ReturnsConflict` and `MarkAllSamplesDrawnForPatientCommandHandlerTests.SoftDeletedPatient_ReturnsConflict`.
- **Waiver:** None.

---

## 8. Required Reading (Required — mark "None" if none)

- **M-21 Implementation Plan** — `Docs/OpenCode/M-21.md` (this session's source of truth; §3.1 inlines the complete M02 surface M21 depends on).
- **M-02 Handoff** — `Docs/Handoff_M02.md` (for the integrated M02 schema and access-policy shape; see §3.1 of the M-21 plan for the inlined contracts).
- **M-14 Handoff** — `Docs/Handoff_M14.md` (for `ExternalEntity` reference).
- **ADR-0033** — `Docs/Source/Top_Lab_ADR.md` (M-21 permission-gate reuse of `ADD_EDIT_PATIENT`).

---

*End of document.*
