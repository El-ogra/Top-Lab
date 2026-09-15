# Top-Lab — Handoff Document M-07

## نظام توب لاب — تسليم جلسة عمل (Module 7 — Combined, Blank & History Reports)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-15_M07-combined-blank-history-reports` |
| Session date (UTC) | 2026-09-15 |
| Session start (UTC) | 2026-09-15 |
| Session end (UTC) | 2026-09-15 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-07 |
| Module name | Combined, Blank & History Reports |
| Wave | 7 |
| Feature folder(s) touched | `src/TopLab.Domain/Reports/**`, `src/TopLab.Application/Features/ReportProduction/**`, `src/TopLab.Infrastructure/Printing/**`, `tests/TopLab.Domain.Tests/Reports/`, `tests/TopLab.Application.Tests/Features/ReportProduction/`, `tests/TopLab.Infrastructure.Tests/Printing/` |
| Layers touched | Domain + Application + Infrastructure (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `664ed34` (live `main` HEAD after Slice 3) |
| Final commit at session end | `[M-07] Slice 5/5: Hardening / close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 7 **Combined, Blank & History Reports** end-to-end in the five slices S1–S5 of `Docs/OpenCode/M-07.md` (Domain rules + Application surface + Infrastructure printing + Application history insertion + close-out). The work is **backend only** — no Presentation content. S1 ships `CombinedReportSelection` (ephemeral, user-ordered, reviewed-only, deduplicated) and `PatientHistoryResolver` (ByLabCode / exact-normalized-name identity per `HistorySortMode`). S2 ships the Application core surface: combinable-tests list, combined/blank report builders, patient-history queries, and DTOs. S3 ships the Infrastructure `ReportPrintingService` (PDF-first, settings-at-print-time, temp-file dispatch) plus the three `PRINT_RESULTS`-gated print commands with the BR-07 balance gate on combined+history only (blank exempt) and `MarkPrinted` audit per printed line. S4 ships auto/manual history insertion (DTO copies, no stored-row mutation) and the separate history report assembly. S5 proves zero drift (no migration), appends ADR-0040, flips the tracking sheet, creates this handoff, and confirms full-suite green. Build must be 0 errors / 0 warnings; full suite green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain rules: combined-report selection + patient-history resolver** — Implementation Complete — `CombinedReportSelection` (ephemeral ValueObject: `Add(patientTestId, isReviewed)` rejects non-reviewed + duplicate; `MoveUp`/`MoveDown` keep contiguous 1..n order; `OrderedIds` projection); `PatientHistoryResolver` static (`ResolveKey(mode, labId, fullName)`: ByLabCode → trimmed LabId key (rejects null/empty); ByPatientName → trimmed + case-folded full name; whitespace name rejected). Tests: `CombinedReportSelectionTests` (non-reviewed rejected, duplicate rejected, moves keep sequence, empty-selection) + `PatientHistoryResolverTests` (both modes, LabId-less rejected, whitespace name rejected, extra-spaced names normalize). Commit `[M-07] Slice 1/5: Domain rules — loop-engineering`.
- **S2 — Application core surface** — Implementation Complete — DTOs (`ReportDtos.cs`: `CombinableTestDto`, `CombinedReportLineDto`, `BlankReportDto`, `HistoryEntryDto`, `PatientHistoryDto`, `MultiPatientHistoryDto`); access policy (`ReportProductionAccessPolicy.PrintResults`); `DomainFailureTranslator`; shared `PatientHistoryReader` (resolver + entries + identity ordering); 3 open queries (`GetCombinableTests`, `GetPatientTestHistory`, `GetMultiPatientHistory`) + 2 persistence-free build commands (`BuildCombinedReport`, `BuildBlankReport`), each with handler + validator; 5 handler-test classes (30 tests). Commit `[M-07] Slice 2/5: Application core surface — loop-engineering`.
- **S3 — Infrastructure printing + print commands** — Implementation Complete — `ReportPrintingService` (PDF-first, settings-at-call-time, temp-file dispatch, `PrinterAssignment` routing); `ReportPdfWriter` (kind titles, `LabId:` vs `PatientId:` per setting, Paper/TopSpace/HeaderFooter/DoctorSignature); `ShellPdfPrinterDispatcher` (OS print association); 3 print command triplets (`PrintCombinedReport`, `PrintBlankReport`, `PrintHistoryReport`) each with command/validator/handler; BR-07 gate on combined+history only (blank exempt); `MarkPrinted` audit per printed line; DI registration (3 Scoped); `ReportPrintEnvelope` (JSON token); additive `DomainFailureTranslator` overload for `InvalidOperationException`; authorization theory tests + validator registration tests. Commit `[M-07] Slice 3/5: Infrastructure printing — loop-engineering`.
- **S4 — Application history insertion** — Implementation Complete — `HistoryInsertion` helper (persistence-free model line from DTO); `AutoInsertHistoryCommand` (switch-false → empty lines EC-09; switch-true → copies prior results for same TestId under resolved identity); `InsertHistoryResultCommand` (manual, unconditional, source must belong to resolved identity else Conflict EC-10); `GetSeparateHistoryReportQuery` (standalone P-04 history DTO); re-pointed `PrintHistoryReportCommandHandler` to `GetSeparateHistoryReportQuery`; 5 test classes covering switch-false pin, no-mutation pin, wrong-identity rejection, happy paths. Commit `[M-07] Slice 4/5: Application history insertion — loop-engineering`.
- **S5 — Close-out** — Implementation Complete — Release build 0/0; full suite green 1581; migration-scope gate clean (zero drift, snapshot unchanged); ADR-0040 appended; tracking sheet M07 row flipped to Done + dated change-log row; `Handoff_M07.md` created; zero Presentation content confirmed.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Release, `-m:1` posture).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -m:1`): **1581 green** = 378 Domain.Tests + 1061 Application.Tests + 142 Infrastructure.Tests.
- New tests added across S1–S4: S1 +18 Domain (CombinedReportSelectionTests + PatientHistoryResolverTests); S2 +30 Application (5 handler-test classes); S3 +16 Infrastructure (ReportPdfWriterTests + ReportPrintingServiceTests + DI resolution) + 32 Application (3 print handler tests + authorization tests + validator registration tests); S4 +23 Application (5 test classes for AutoInsert/InsertHistoryResult/GetSeparateHistoryReport + PrintHistoryReport re-point).
- Tests currently failing: none.
- Coverage: per-slice gates passed (VG-01 Domain Reports ≥90%, VG-02 Application S2 ≥80%, VG-03 Infra S3 ≥70% + App S3 ≥80%, VG-04 Application S4 ≥80%). Whole-project floors are inapplicable for M-07 (the module touches a subset of files); waiver recorded per the M-11/M-14 precedent.

### 4.3 Migrations

- New EF Core migration(s) added: **None** (OD-07-B ephemeral selection; history lines are copies; print audit uses existing `PatientTest` columns).
- `has-pending-model-changes` at close-out: No changes — zero drift. Snapshot unchanged.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: 3 Scoped (`IReportPrintingService → ReportPrintingService`, `IReportPdfWriter → ReportPdfWriter`, `IPdfPrinterDispatcher → ShellPdfPrinterDispatcher`) in `src/TopLab.Infrastructure/DependencyInjection.cs`.
- Validators: 8 new validators (`AutoInsertHistoryCommandValidator`, `InsertHistoryResultCommandValidator`, `PrintCombinedReportCommandValidator`, `PrintBlankReportCommandValidator`, `PrintHistoryReportCommandValidator`, `BuildCombinedReportCommandValidator`, `BuildBlankReportCommandValidator`, `GetMultiPatientHistoryQueryValidator`) resolve via the existing assembly scan; no DI wiring change.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the `PermissionConfiguration.cs` seed: **none** — `PRINT_RESULTS` (id=4) and `BLOCK_PRINT_ON_BALANCE` (id=5) already seeded.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All five slices reached a terminal state; module closed out in the Master Tracking Sheet (§4 M07 row + §9 change-log row), ADR-0040, and this handoff. Natural next steps (Presentation-layer report screens consuming the DTOs/PDF, M-09 reporting/delivery) are out of scope of this plan.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

All were owner-confirmed verbatim before/during execution and are recorded in ADR-0040 (no re-derivation):

- **Decision (PDF-first printing — OD-07-A):** `IReportPrintingService` is implemented as PDF generation + printer dispatch. Settings read at print time, never cached. Tested `PatientReportPdfExporter` precedent.
  - **Scope of impact:** `ReportPrintingService`, `ReportPdfWriter`, `ShellPdfPrinterDispatcher`, DI registration.
  - **Follow-up required:** No.
- **Decision (ephemeral combined selection — OD-07-B):** `CombinedReportSelection` is an in-memory ValueObject; nothing is persisted. Zero migration.
  - **Scope of impact:** Domain `Reports/CombinedReportSelection.cs`; verified by `BuildCombinedReportCommandHandler` tests.
  - **Follow-up required:** No.
- **Decision (exact-normalized-name history identity — OD-07-C):** `ByPatientName` resolves by exact trimmed + case-folded full name only; no fuzzy matching. `ByLabCode` is the default.
  - **Scope of impact:** `PatientHistoryResolver`; ADR-0040 records the patient-safety rationale.
  - **Follow-up required:** No.
- **Decision (BR-07 scope — OD-07-E):** Balance gate applies to combined + history printing; blank report exempt (no results). Verbatim M-06 message reused.
  - **Scope of impact:** `PrintCombinedReportCommandHandler`, `PrintHistoryReportCommandHandler`; `PrintBlankReportCommandHandler` has no gate.
  - **Follow-up required:** No.
- **Decision (history lines are copies):** Auto/manual insertion copies values into report DTO; no stored `PatientTest` row is mutated by any report operation. Pinned by tests re-reading source rows.
  - **Scope of impact:** `AutoInsertHistoryCommandHandler`, `InsertHistoryResultCommandHandler`.
  - **Follow-up required:** No.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** None.
- **Waiver:** Whole-project coverage floors (Domain ≥90%, Application ≥80%, Infrastructure ≥70%) are inapplicable for M-07 because the module touches a subset of files in each project. Per-slice footprint coverage gates were verified (VG-01 through VG-04 all PASS). This waiver follows the M-11/M-14 precedent where whole-project floors are replaced by footprint posture.

---

## 8. Required Reading (Required — mark "None" if none)

- **M-07 Implementation Plan** — `Docs/OpenCode/M-07.md` (this session's source of truth; §5–§7).
- **M-07 Loop-Engineering Memory** — `Docs/OpenCode/M-07-memory.md` (slice index, gates VG-01..VG-05, per-slice 10-stage checklists, execution log).
- **ADR-0040** — `Docs/Source/Top_Lab_ADR.md` (PDF-first, ephemeral selection, exact-normalized-name, BR-07 scope, settings-at-print-time, zero-drift).
- **M-04 Handoff** — `Docs/Handoff_M04.md` (for `MarkPrinted` mutator, `BalanceProbe` delegation, lifecycle guards).
- **M-05 Handoff** — `Docs/Handoff_M05.md` (for frozen snapshot reads, `ProfileResultItemReferenceRangeSnapshot` pattern).
- **M-06 Handoff** — `Docs/Handoff_M06.md` (for BR-07 balance gate precedent, `MarkCultureReportPrinted` verbatim message).
- **M-08 Handoff** — `Docs/Handoff_M08.md` (for `VisitRollup` LabId rollup pattern, settings echo).
- **M-22 Handoff** — `Docs/Handoff_M22.md` (for `ReportSettings`/`SystemSettings` shapes, `HistorySortMode`, `HistoryAutoDisplayEnabled`).

---

*End of document.*
