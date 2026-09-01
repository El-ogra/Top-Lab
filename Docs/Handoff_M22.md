# Top-Lab — Handoff Document M22

## نظام توب لاب — تسليم جلسة عمل (Module 22 — System & Print Settings)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-02_M22_system-and-print-settings` |
| Session date (UTC) | 2026-09-02 |
| Session start (UTC) | 2026-09-02 |
| Session end (UTC) | 2026-09-02 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M22 |
| Module name | System & Print Settings |
| Wave | 1 |
| Feature folder(s) touched | `Features/SystemAndPrintSettings/`, `Presentation/ViewModels/Settings/`, `Presentation/Views/Settings/`, `Presentation/Services/` |
| Layers touched | Domain / Application / Infrastructure / Presentation |
| Branch name | (local-only commits; no branch switching) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `797892f` (pre-M22 baseline, "التخطيط لإستكمال الموجة الأولى") |
| Final commit at session end | `ac77cc8` (pre-doc finalize; docs finalize commit appended by S8) |

---

## 2. Session Objective (Required)

Implement Module 22 **System & Print Settings** end-to-end in the eight slices S1–S8 of `Docs/Module 22 Implementation Plan.MD` / `Docs/M-22Loop.MD`, satisfying FR-M22-001 … FR-M22-016 and BR-10. This covers the six settings aggregates (`SystemSettings`, `ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`, `EnvelopePrintItemPosition`, `PrinterAssignment`), the read/write Application surface (queries, commands, validators, authorization via `EDIT_SYSTEM_SETTINGS`), the Infrastructure maintenance/connection/lab-text services, and the Presentation screens S-27 (Settings dashboard), S-28 (System), S-29 (Report), S-30 (Receipt), S-31 (Envelope), S-32 (Database Maintenance, secondary-password gated). Build must be 0 errors / 0 warnings; all tests green; local-only commits; no remote pushes; no new NuGet packages beyond pinned set; no database migration added.

---

## 3. Achievements This Session (Required)

- **S1 — Domain behaviors** — Implementation Complete — `src/TopLab.Domain/` aggregates gained `ApplyX`/mutator and invariant-guard methods (top-space 8 cm clamp, currency ≤ 10, art-keys, position/offset counters). Commit `d1bfebf`; 114 Domain tests green.
- **S2 — Application read surface** — Implementation Complete — read queries + DTOs (`SystemSettingsDto`, `ReportSettingsDto`, `ReceiptSettingsDto`, `EnvelopeSettingsDto`, `EnvelopePrintItemPositionDto`, `PrinterAssignmentDto`, `LabPrintTextDto`, `DatabaseServerSettingsDto`) + test fakes. Commit `14ba0f1`.
- **S3 — Application write surface** — Implementation Complete — update commands + validators + permission gating (`Update*SettingsCommand`, `SavePrinterAssignmentsCommand`, `ApplyDatabaseUpdatesCommand`, `BackupDatabaseNowCommand`, `RestoreDatabaseCommand`, `SaveLabPrintTextCommand`, `UpdateDatabaseServerSettingsCommand`). Commit `a8cbf5a`; 123 Application tests green.
- **S4 — Infrastructure** — Implementation Complete — `IDatabaseMaintenanceService` + SQL-backed maintenance, `WorkstationConnectionSettingsProvider`, `JsonLabPrintTextStore`, `DailyBackupHostedService`, seed-repair, DI wiring (added already-pinned `Microsoft.Extensions.Hosting` reference). Commit `3f41c8e`; 31 Infrastructure tests green.
- **S5 — Presentation: Settings dashboard + System settings** — Implementation Complete — `SettingsDashboardViewModel/View`, `SystemSettingsViewModel/View`, `IPrinterCatalogService` + `PrinterCatalogService`, shell navigation for "الإعدادات", DI + DataTemplates. Commit `c267944`.
- **S6 — Presentation: Report / Receipt / Envelope screens** — Implementation Complete — `ReportSettingsViewModel/View`, `ReceiptSettingsViewModel/View`, `EnvelopeSettingsViewModel/View`, dashboard nav wiring. Commit `9d99eae`.
- **S7 — Presentation: Database Maintenance (gated)** — Implementation Complete — `DatabaseMaintenanceViewModel/View`, secondary-password gate via `DialogService.ShowSecondaryPasswordDialogAsync`, backup folder/file pickers added to `IDialogService`. Commit `ac77cc8`.
- **S8 — Hardening, docs & handoff** — In Progress at session end (build/tests green, docs updated, handoff created) — see §5.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes.
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite: 268 green = 114 Domain + 123 Application + 31 Infrastructure.
- Tests currently failing: none.

### 4.3 Migrations

- New EF Core migration(s) added: none (M22 intentionally adds no migration; print text is workstation-local JSON, settings aggregates are single-row existing tables).
- Migration applied to a local database during the session: No.
- Any manual schema change made outside a migration: No (seed-repair routine re-asserts existing seed rows at runtime; no schema change).

### 4.4 Dependency Injection wiring

- New services registered (all `TopLab.Presentation.DependencyInjection.AddPresentation` and Infrastructure DI): `IDatabaseMaintenanceService` → SQL + fake, `IWorkstationConnectionSettingsProvider`, `ILabPrintTextStore` (JSON), `IPrinterCatalogService`, plus Transient ViewModels `SettingsDashboardViewModel`, `SystemSettingsViewModel`, `ReportSettingsViewModel`, `ReceiptSettingsViewModel`, `EnvelopeSettingsViewModel`, `DatabaseMaintenanceViewModel`.
- Composition-root changes: none beyond DI registration and `MainWindow.xaml` DataTemplates for the six settings ViewModels.

### 4.5 Configuration

- New application configuration keys added: none. `lab-print-text.json` defaults to workstation configuration location; `appsettings.json` unchanged.

---

## 5. Work In Progress (Required — mark "None" if none)

- **What is in progress:** Slice S8 (final) documentation finalization and this handoff record.
- **Current state:** S1–S7 code committed (`d1bfebf`…`ac77cc8`); build 0/0; tests 268 green; Master Tracking Sheet §4/§5/§6/§9 and `Top_Lab_ADR.md` (ADR-0027) updated in the working tree; `Docs/Handoff_M22.md` created.
- **What remains:** Stage and commit the two modified tracked docs (`Top_Lab_Master_Tracking_Sheet.md`, `Top_Lab_ADR.md`) and the new `Handoff_M22.md` with message `docs(m22): finalize module 22 documentation, ADR-0027, tracking sheet, and handoff`; update the M-22Loop §1 progress narrative to the final Arabic line; mark S8 checklist `[x]`. The planning docs (`Docs/M-22.md`, `Docs/M-22Loop.MD`, `Docs/Module 22 Implementation Plan.MD`, `Docs/RLS_Learn_M-22.pdf`) are untracked and must NOT be committed.
- **Estimated effort remaining:** < 30 minutes.
- **Location:** `Docs/Source/`, `Docs/M-22Loop.MD`, `Docs/Handoff_M22.md`.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

- **Decision:** Lab print text and font are workstation-local, file-backed JSON per scope (Report/Receipt/Envelope) and are excluded from the business database to avoid a migration.
  - **Reason:** Machine-specific print output matches the ADR-0021 locality rule and avoids schema churn for ephemeral display content.
  - **Scope of impact:** Presentation + Infrastructure; affects report/receipt/envelope provisioning.
  - **Follow-up required:** Yes — recorded as **ADR-0027** in `Top_Lab_ADR.md`.
- **Decision:** No image and no color print-configuration controls are implemented (the PRD's FR-M22-004/005 image/color options are excluded).
  - **Reason:** Text/font configuration replaces them; avoiding binary assets keeps DTOs and storage simple (ADR-0027).
  - **Scope of impact:** Report/Receipt/Envelope settings screens and DTOs.
  - **Follow-up required:** No (documented in ADR-0027; a future decision may add images without schema changes).
- **Decision:** Database Maintenance navigation is gated by the M17 secondary-password dialog (`ShowSecondaryPasswordDialogAsync`), reusing the existing gate rather than introducing a new one.
  - **Reason:** Consistency with the M17 gate and the settled permission model.
  - **Scope of impact:** Presentation.
  - **Follow-up required:** No.

---

## 7. Open Issues, Bugs and Risks (Required — mark "None" if none)

- None known at session end. Remaining S8 work is documentation finalization only (see §5); it is not a code defect.

---

## 8. Deviations and Waivers (Required — mark "None" if none)

- **Convention departed from:** None. The only execution-mechanics improvisation (none model-driven) was adding two folder/file picker methods to `IDialogService` to support the Database Maintenance backup/restore flows, which is a presentation-layer service method addition consistent with the existing dialog service pattern; no domain/application design deviation was introduced.

---

## 9. Pending Reviews and Audits (Required)

- **Code review status:** Not started (local-only commits; no reviewer assigned).
- **Audit acceptance status:** Not started.
- **Blocking findings from review or audit:** None.

---

## 10. Next Session Objective (Required)

Complete Slice S8 by committing the documentation finalization: stage and commit `Docs/Source/Top_Lab_Master_Tracking_Sheet.md`, `Docs/Source/Top_Lab_ADR.md`, and `Docs/Handoff_M22.md` with `docs(m22): finalize module 22 documentation, ADR-0027, tracking sheet, and handoff`; update the M-22Loop §1 narrative to the final Arabic completion line. Then mark Slice S8 fully `[x]` in `Docs/M-22Loop.MD`, confirming all eight slices are Done. Expected end-state: Module 22 closed in the Master Tracking Sheet (§4 M22 row and §5 Wave 1 🟩), ADR-0027 appended, and handoff record on disk. No further Module 22 implementation work is outstanding.

---

## 11. Required Reading Before Continuing (Required)

- Coding Standards & Conventions (`Top_Lab_Coding_Standards.md`).
- Architecture & Folder Structure Blueprint (`Top_Lab_Architecture_Blueprint.md`).
- Data Model / Database Schema Blueprint (`Top_Lab_Data_Model_Blueprint.md`).
- Product Requirements Document — §5 M22 (FR-M22-001…016), §14, §15, §16.
- Test Strategy & Audit Acceptance Criteria — §7.2 M22 checklist.
- UI/UX & ViewModel Blueprint — §5.10 (S-27…S-32), §4.4/§4.7.
- Reporting & Printing Blueprint — §6 Settings Consumption Matrix, §10 Printer Assignment, §11 exclusions.
- Module Dependency & Execution Order Map.
- Master Tracking Sheet.
- Architecture Decision Records — ADR-0020, ADR-0021, ADR-0025, ADR-0027.
- Prior handoff documents for adjacent modules — `Handoff_M17` (M17 user & permission management).

---

## 12. Environment and Tooling Notes (Optional)

- Windows (win32), PowerShell (pwsh). `rg` and `2>/dev/null` are unavailable; `Select-String` used instead.
- Build/test commands run: `dotnet build TopLab.sln` (0 errors/0 warnings), `dotnet test TopLab.sln -m:1` (268 green).
- No non-standard scripts executed; no scratch files left outside the repo.

---

## 13. Artifacts Produced (Required — mark "None" if none)

- **Name:** `Handoff_M22.md`
- **Location:** `Docs/Handoff_M22.md`
- **Purpose:** Session handoff following `Top_Lab_Handoff_Template.md`.
- **Persistence:** Kept.

- **Name (untracked planning, not committed):** `M-22Loop.MD`, `Module 22 Implementation Plan.MD`, `M-22.md`, `RLS_Learn_M-22.pdf`
- **Location:** `Docs/`
- **Purpose:** Slice checklists, implementation plan, reference spec.
- **Persistence:** Kept in working tree; must not be committed.

---

## 14. Signature Block (Required)

| Role | Name | Date (UTC) | Confirmation |
|---|---|---|---|
| Outgoing agent | Local coding agent (Top-Lab) | 2026-09-02 | I confirm this handoff document accurately reflects the state of the work at session end. |
| Reviewer (if any) |  |  | I have reviewed this handoff for completeness. |
| Incoming agent (on acceptance) |  |  | I confirm I have read and understood this handoff and accept it as my starting context. |

---

## 15. Attachments (Optional)

- `Module 22 Implementation Plan.MD` — `Docs/Module 22 Implementation Plan.MD` — primary implementation plan (untracked).
- `M-22Loop.MD` — `Docs/M-22Loop.MD` — slice-by-slice checklist loop (untracked; final updates in S8).
- `Top_Lab_Master_Tracking_Sheet.md` — `Docs/Source/` — module board, wave summary, per-module block, change log.
- `Top_Lab_ADR.md` — `Docs/Source/` — appended ADR-0027.

---

*End of handoff document.*