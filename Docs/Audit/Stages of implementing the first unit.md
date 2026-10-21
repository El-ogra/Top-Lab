# Top-Lab — First Functional Unit: Dependency-Ordered Implementation Stages

> **Document type:** implementation-stage plan (planning artefact only).
> **Execution contract:** a coding agent executes this document stage by stage, in the order given, obeying each stage's completion gate before starting the next stage.
> **Baseline authority:** the current source code at the measured baseline below. Every implementation-level statement in this plan was re-derived from that code and is cited to a verified path.
> **Nothing was executed while producing this plan:** no source file, test file, project file, configuration file, migration, model snapshot, or database was created, modified, or deleted. The only file written is this plan.

---

## A. Executive Overview

### A.1 Objective

Bring the **first functional unit** of Top-Lab — patient registration, test registration, patient account, result entry, report production, patient search, case follow-up and result delivery — to full functional parity with the approved functional baseline for that unit, **subject only to the five owner-approved exclusions in Section C**.

This plan does not implement anything. It defines *what* must change, *in what order*, *what each change depends on*, *which database change each stage requires*, and *how each stage is proven complete*.

### A.2 Plan identity

| Field | Value |
|---|---|
| Plan ID | `TL-FU1-STAGES` |
| File name | `Stages of implementing the first unit.md` |
| Placement | `Docs/Audit/` |
| Baseline HEAD | `f1e5011e9010b2f2e2c45d882b55bc32dac23e8f` |
| Branch | `main` |
| Working-tree state at authoring | Clean except untracked input material under `Docs/The reviewer/`; **no tracked file modified** |
| Solution | `TopLab.sln` — 4 production projects (`TopLab.Domain`, `TopLab.Application`, `TopLab.Infrastructure`, `TopLab.Presentation`) + 5 test projects |
| Target frameworks | `net8.0` (Domain, Application, Infrastructure), `net8.0-windows` (Presentation); test projects mirror these |
| EF Core | 8.0.30 (SqlServer / InMemory / Tools / Design) — pinned in `Directory.Packages.props` |
| Other pinned packages | MediatR 12.5.0, FluentValidation 12.1.1, QuestPDF 2026.9.0, ZXing.Net 0.16.11, Testcontainers.MsSql 3.10.0, xUnit 2.9.3 |
| Current migration set | 21 migrations, `20260828052248_BaselineDataModel` … `20261018144852_AddProfileResultStyling`, plus `ApplicationDbContextModelSnapshot.cs` |
| Model snapshot | `src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` |
| Planned stages | **15** (STAGE-01 … STAGE-15) |
| Authoring date | 2026-10-20 |

### A.3 Verified scope of this plan

The functional baseline for this unit contains **491 identified elements** across 15 groups (1.1 … 1.15). Baseline conformance classification, as established by the unit's conformance audit and independently re-verified against the code while authoring this plan:

| Baseline class | Count | Share | Meaning |
|---|---|---|---|
| A — functionally matching | **146** | 29.7 % | Behaviour already correct; **no reimplementation scheduled** |
| B — partially present | 107 | 21.8 % | Present but incomplete or in the wrong place |
| C — present with different behaviour | 66 | 13.4 % | Present but observably different from the baseline |
| D — not present | 172 | 35.0 % | Absent; must be built |
| E — unverifiable | 0 | 0 % | No element is classified unverifiable at baseline |
| **Total** | **491** | 100 % | |

The previously reported headline "146 functionally matching functions" **is corroborated** and is used as the basis of this plan: the 146 figure is reproduced independently from the per-element evidence (139 class-A elements outside group 1.15 + 7 class-A elements inside group 1.15 = 146), and every class-A identifier is listed with disposition `PRESERVE` in Section G so that no working element is scheduled for redundant reimplementation.

**One material qualification on the B/C/D split is recorded in Section B.5 and as verification task `VER-01`.** The persisted per-element audit evidence covers 455 of 491 identifiers; the 36 identifiers of group 1.15 that follow `1.15.1.2` are not persisted, and the two available reconstructions of that group's B/C/D split disagree on 5 elements. The **class-A total of 146 is unaffected** by that disagreement. This plan schedules group-1.15 work on the best available element-level evidence and gates it behind `VER-01`.

### A.4 The five owner-approved exclusions (permanent, for this and all later units)

These five capability categories are **out of Top-Lab's required functionality**. Their absence is never a defect, never a gap, and never a parity failure. No stage may implement them, schedule them, or list them as an optional enhancement.

1. **Public online patient-results portal** — no portal, no NATIGH.COM-style functionality, no public result publishing, no public patient access, no portal accounts, no portal URLs, no portal authentication, no portal integrations, no portal-oriented database fields or workflows.
2. **Equipment inventory, maintenance and calibration** — no equipment register, no equipment maintenance or calibration workflow.
3. **Analyzer connectivity and automated result import** — no direct connection to laboratory analyzers (including CBC analyzers), no automated analyzer-result import.
4. **Multi-branch support** — Top-Lab supports one laboratory branch only.
5. **External result delivery** — no result delivery by SMS, e-mail or fax.

**Precise application to this unit** (Section C and per identifier in Section G):

- Exclusion 1 removes the external-service delivery group and its portal-facing children, the site-upload/site-reference elements of the registration screen, and the site-access label. The **local** capabilities that the same baseline requires — printing, local PDF export, local result viewing, manual result entry, specimen handling — **remain in scope and are scheduled**.
- Exclusions 2 and 3 **remove no identifier of this unit**: the unit contains no equipment-register element and no analyzer-integration element. Manual result entry (1.6.1.5 / 1.6.1.6) and specimen/sample workflows (1.12.1.9, 1.14) remain fully in scope.
- Exclusion 4 removes the two branch-scope identifiers of the search screen; the surrounding scope block keeps its non-branch child (the source selector).
- Exclusion 5 removes the SMS/e-mail/fax fields and send actions from the registration screen, the test-selection block and the delivery screen.

### A.5 Offline-first constraint

Top-Lab must remain **fully usable with no public Internet access and no external cloud service**. No stage may introduce a remote API, an online service, a cloud dependency, or a runtime Internet requirement.

Verified supporting facts: the solution contains **zero `HttpClient` usages anywhere under `src/`**; all rendering is local (`QuestPDF` 2026.9.0 with `QuestPdfSkia.dll` / `qpdf.dll` native binaries for `win-x64` present in the package `runtimes/` folder); the only datastore is a locally reachable SQL Server instance; the persistence test suite is deliberately pinned to ephemeral containers with a "never connect to a configured database" guard (`tests/TopLab.Persistence.Tests/NeverConnectGuardTests.cs`).

Design consequences applied throughout this plan: the search screen's "Backup" source is implemented as a **locally configured secondary database**, not as a restore-from-`.bak` service; local PDF/print generation is the only delivery mechanism; no stage proposes a package that needs a network call at runtime.

### A.6 Principles used to determine implementation status

| Principle | Application in this plan |
|---|---|
| Externally observable behaviour decides | A different internal design is **not** a defect. Elements are scheduled only where observable behaviour deviates from the baseline. |
| Already-correct elements are preserved, not rewritten | All 146 class-A identifiers carry disposition `PRESERVE` and appear in no work item. Regression coverage is added only where a later stage touches their surface. |
| Container and leaf identifiers are distinct | Every nested identifier is dispositioned individually; none is hidden behind its parent's disposition. |
| Mixed requirements are split | Where one identifier mixes excluded and in-scope behaviour, the split is stated on the identifier itself (disposition `SPLIT`). |
| Unestablished behaviour is verified, not invented | 20 identifiers whose intended behaviour is not established by the baseline carry a `VERIFY` disposition with a named decision owner, a documented default, and a conditional path. |
| Migrations are decided from inspected state | Every migration decision names the affected table/columns, the data-preservation rule, the rollback, and what must be inspected before the decision is final. |
| No destructive change | Additive columns with explicit defaults, append-only seed inserts, no rewriting of applied migrations, no column drops in this plan. |
| One identifier, one owning stage | Each in-scope identifier has exactly one owning stage, so no two stages can implement the same behaviour. |

---

## B. Verified Current-State Assessment

### B.1 How the assessment was produced

1. The functional baseline for this unit (element hierarchy, element definitions, screen map, workflows, validation rules, outputs, uncertainty list) was read in full and used as the authority on *required behaviour*.
2. The unit's conformance audit was read for *baseline classification* and then cross-checked against the code.
3. The code was re-inspected directly for every element whose implementation state materially affects the plan: the four production projects, the EF configurations and model snapshot, the domain entities, the 21 migrations, the seeded configuration rows, the five test projects, and the printing/barcode infrastructure.
4. The supplementary session transcript was used **only as a lead generator**. Every material claim it carried was re-checked against the code before being accepted, corrected, or rejected (Section B.5).

### B.2 Verified architecture of the relevant surface

| Area | Verified fact | Evidence |
|---|---|---|
| Layering | Domain (entities + pure services) → Application (MediatR commands/queries/handlers, FluentValidation validators, DTOs) → Infrastructure (EF Core 8, QuestPDF printing, ZXing barcode, PDF preview) → Presentation (WPF, one ViewModel + View per screen) | `src/TopLab.Domain`, `src/TopLab.Application`, `src/TopLab.Infrastructure`, `src/TopLab.Presentation` |
| Feature slicing | 25 feature folders under `src/TopLab.Application/Features` (`PatientRegistration`, `PatientBilling`, `ResultsEntry`, `ResultDelivery`, `PatientSearch`, `ReportProduction`, `CultureResults`, `SampleCollection`, `AuditAndTraceability`, …) | directory listing |
| Persistence | Single `ApplicationDbContext`, one explicit `DbSet` per aggregate, value-converted `int` identifiers, no lazy loading; auditable entities stamped by `AuditableEntitySaveChangesInterceptor` | `src/TopLab.Infrastructure/Persistence/ApplicationDbContext.DbSets.cs`, `src/TopLab.Domain/Common/AuditableEntity.cs` |
| Seeded configuration | `HasData` rows exist for `MedicalConditionType` (**1 row only**), `SystemSettings`, `ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`, `ReportItemTemplate`, `PrinterAssignment`, `Permission`, `EnvelopePrintItemPosition` | `src/TopLab.Infrastructure/Persistence/Configurations/*.cs` |
| Patient aggregate | `Patient` = **one row per visit**; `LabId` is the cross-visit grouping key. Fields: `FullName`, `Sex`, `AgeValue`/`AgeUnit`, `NationalId`, `Address`, `TreatingDoctorId`, `ReferralEntityId`, `AccountType`, `IsVip`, `RegistrationDateUtc`, `PickupDateUtc` (**always null**), `IsFastingIndicated`, `FastingHours`, `RecentContrastImaging` (**contrast only**), `Notes`, `IsDeleted`. **No card number, no patient-level sample type, no print-result password** | `src/TopLab.Domain/Patients/Patient.cs:11-300` |
| Lab-ID reuse | `CreatePatientCommandHandler` **rejects** a LabId already held by any row, and `GetNextLabIdQueryHandler` always returns *max+1* — so a returning patient can never be shown their previous identifier | `src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs:48-58`; `…/GetNextLabId/GetNextLabIdQueryHandler.cs:20-33` |
| Result aggregate | `PatientTest` = one order/result line. Fields: `PriceAtOrderTime`, five sample-type booleans, `IsTakenOutsideLab`, `IsSampleDrawn`, `ResultValue`, `ResultFlag?`, `Notes`, entered/reviewed/printed/delivered/exported flags and counters. **No unit, no finished state, no analysis date** | `src/TopLab.Domain/Results/PatientTest.cs:8-230` |
| Flag model | `ResultFlag { Normal=0, Low=1, High=2 }` (stored `tinyint` via int conversion); `ProfileResultFlag { Low=0, High=1 }` (nullable) | `src/TopLab.Domain/Common/Enums/ResultFlag.cs`, `ProfileResultFlag.cs`; `PatientTestConfiguration.cs` |
| Range model | Ranges live on `Analyte` → `AnalyteReferenceRange` → `AnalyteReferenceRangeBand` (sex/age band, Min/Max, Low/High comments). **No unit, no critical band.** Each result freezes `PatientTestReferenceRangeSnapshot` | `src/TopLab.Domain/Tests/AnalyteReferenceRangeBand.cs`; `src/TopLab.Domain/Results/PatientTestReferenceRangeSnapshot.cs` |
| Profile results | `ProfileResultItem` = one analyte line with `ResultValue`, `Unit`, `Flag?`, `IsVerified`, `IsPrinted`. **No comment, no control value, no second unit, no qualitative marker** | `src/TopLab.Domain/Results/ProfileResultItem.cs` |
| Money model | `PaymentOperation` with `Amount` (>= 0, `decimal(18,2)`), `DiscountAmount?`, `IsExtraCharge`, `OperationType {Payment, Correction, FullSettlement}` (int→tinyint), `IsVoided`. `PatientAccountCalculator` computes TotalCharged / TotalPaid / Balance; voided rows contribute nothing | `src/TopLab.Domain/Billing/PaymentOperation.cs`, `PatientAccountCalculator.cs`; `PaymentOperationConfiguration.cs` |
| Culture model | `CultureResult`, `CultureMicroscopy` (free-text fields, single `IsDirect` boolean), `CultureAntibioticResult` (**nullable** `SensitivityCategory`, `InhibitionZoneMm?`), `CultureAntibioticAttachment` (test↔antibiotic), `Antibiotic` (already carries `CommercialName`) | `src/TopLab.Domain/Results/Culture*.cs`, `src/TopLab.Domain/Tests/Antibiotic.cs` |
| Report settings | `ReportSettings` is a **single row, PK = 1**; it already holds margins, paper size, header/footer mode, abnormal colours, `HistorySortMode`, `HistoryAutoDisplayEnabled`, `PrintGroupSubTitle`, `SuppressReprintMessage`, `NormalRangeMarker`. `ReportItemTemplate` holds per-item label, symbol, spacing, offset, show/hide, list membership | `src/TopLab.Domain/Settings/ReportSettings.cs`, `ReportItemTemplate.cs` |
| Report-settings write path | `UpdateReportSettingsCommandHandler` writes margins/top-space/paper/header-footer/doctor-signature/history/print-options/colour/images/marker. **`SetPrintOptions` is called with the request values**, and `ReportSettingsViewModel` overwrites them with `false` on save | `src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateReportSettings/UpdateReportSettingsCommandHandler.cs:17-47` |
| Printing | 33 printing/barcode classes: `ReportPdfWriter`, `ReportContentBuilder`, `ReportPageComposer`, `ReportCultureSection`, `ReceiptPdfWriter`, `InvoicePdfWriter`, `EnvelopePdfWriter`, `WorkSheetPdfWriter`, `GroupPatientSlipPdfWriter`, `LabOrderPdfWriter`, `PdfPreviewService`, `BarcodeService` (one generic Code-128 label, 288×144 px), `BarcodeLabelRenderer` | `src/TopLab.Infrastructure/Printing`, `src/TopLab.Infrastructure/Barcode` |
| Result worklist | `GetResultWorklistQueryHandler` returns **one row per test** (not per patient), loads the whole catalogue, computes one aggregate status per patient, and pages in memory | `src/TopLab.Application/Features/ResultsEntry/Queries/GetResultWorklist/GetResultWorklistQueryHandler.cs:22-150` |
| History | `GetPatientTestHistoryQuery`, `GetMultiPatientHistoryQuery`, `GetSeparateHistoryReportQuery`, `HistoryMatrixBuilder` exist; **`PrintHistoryReportCommand` carries only `PatientId`**; the hub opens the history screen with `patientId = 0` | `src/TopLab.Application/Features/ReportProduction/**`; `src/TopLab.Presentation/ViewModels/Patients/PatientsHubViewModel.cs:79-87` |
| Combined report | `BuildCombinedReportCommandHandler` rebuilds the report from the **current** tests only; inserted history lines never reach the printed artefact | `…/Commands/BuildCombinedReport/BuildCombinedReportCommandHandler.cs:33-61` |
| Delivery | Two screens: `ResultDeliveryView` (filter + undelivered list) and `DeliveryHandoverView` (account + per-test grid + settlement). `GetDeliveryGridQuery` returns per-test rows with `IsEntered/IsReviewed/IsPrinted/IsDelivered` but **no finish or export column** | `src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml`, `DeliveryHandoverView.xaml`; `src/TopLab.Application/Features/ResultDelivery/Queries/GetDeliveryGrid/*` |
| Tests | 5 xUnit 2.9.3 projects. `TopLab.Persistence.Tests` uses `Testcontainers.MsSql` (ephemeral container, skips cleanly without Docker) plus `NeverConnectGuardTests`, which forbids `localdb`, `ConfigurationBuilder` and `GetConnectionString(` in persistence tests | `tests/**` |
| Offline posture | 0 `HttpClient` usages under `src/`; QuestPDF ships `win-x64` Skia natives; no cloud SDK | repository scan |

### B.3 Correctly implemented functions (no reimplementation scheduled)

All **146** class-A identifiers, listed in full in Section G with disposition `PRESERVE`. They cluster in:

- **Shell and sign-in (19)** — ribbon items (exit, about, settings, system, users, statistics, accounts, tools, work sheet, laboratory, patients), status-bar user name and today fields, sign-in window fields, sign-in action.
- **Registration identity and header (13 of 1.2.2.1/1.2.2.2)** — patient code, VIP action, age value/unit, mobile/telephone, account type, address, referring entity, treating doctor, fasting toggle and hours, and the two registration actions that already work (save, undo, receipt, invoice, work sheet).
- **Test selection and group lists (14)** — master test list, add/select, delete, all-tests, patient's tests list, taken-outside flag, the five sample-type options, and the "add all tests in group" action.
- **Account panel and account screen (12)** — account panel, total of tests, discount value, paid amount, account summary, account screen, transaction grid, paid date/money/user-name columns, pay, add-amount.
- **Delivery grid and actions (8)** — delivery date filter, delivery result block, test abbreviation, result, verify, print columns, refresh, main-list action.
- **Modification and lookup paths (5)** — patient icon, edit patient, patient selection, modification save, search action.
- **Result entry, report entry, culture, search, visit history, audit (remaining)** — the double-click action, the already-correct report-entry columns, the culture data block and its six fields, culture verified flag, antibiotic list and its four classes, culture save, the search criteria already satisfied, visit history, and eight of the thirteen audit entries.

**Regression rule applied:** these identifiers receive **no work item**. Where a later stage changes the surface that renders them (for example the result grid, the delivery grid, the report settings dialog), the stage's test list includes an explicit "preserved behaviour" assertion so a regression is caught. No stage may re-implement them.

### B.4 Partially implemented, differently implemented, and missing functions

| Baseline class | Volume | Where the work concentrates | Notes from the code |
|---|---|---|---|
| B — partially present | 107 | Registration screen (7), account panel/screen (12), delivery (5), result entry (21+9), report settings (10), combined/empty report (5), culture (10), search (11), visit history (1), modification paths (5), history (3+19) | Most B items are *present in the wrong place* (e.g. the envelope-print action sits on the result-sheet screen instead of the registration action band; the account-summary action sits on the patient screen; result-view actions sit on the result-screen instead of the search screen). |
| C — present, different behaviour | 66 | Registration (3), account (7), delivery (3), result entry (14), report settings (17), culture (0), search (5), visit history (1), audit (1), combined (0) | Representative defects: the result grid shows a **row count** where the baseline requires the *abnormal flag*, with no `Critical` state; the report-entry flag column stores `Low` for an empty selection and cannot express `Normal`; the report settings dialog is unreachable from the report screen and splits across three screens; `UpdateReportSettings` resets the two print options to `false` on every save; the settlement action closes the account with a zero balance and no confirmation. |
| D — not present | 172 | Registration (18), account (7), delivery (21), result entry (21), report entry (18), report settings (7), culture (6), search (20), history (10), labels (23), audit (3), shell (9) | Representative absences: no patient card-number concept; no patient-level sample type; no print-result password; no *finished* test state; no `Critical` flag; no delivery department/name/status filters; no patient-scope block; no specimen-container concept at all; no tube label kind; no microbiology report artefact; no multi-patient history panel; no audit samples block. |
| E — unverifiable | 0 | — | No identifier is classified unverifiable at baseline. The 20 identifiers carrying a baseline uncertainty are **not** an E class: each has a definite requirement, and it is a specific *detail* of each that the baseline leaves open. They are dispositioned `VERIFY` (Section C.3). |

### B.5 Corrections to the baseline evidence, and what remains unverified

Five findings from re-verification, recorded so the plan does not rest on unexamined claims:

1. **The class-A total of 146 is confirmed** from the per-element rows (139 outside group 1.15 + 7 inside). Both the audit's summary and the per-element rows agree. The headline is used in this plan.
2. **The B/C/D split of group 1.15 is not fully determined.** The persisted per-element evidence stops inside `1.15.1.2` (455 of 491 identifiers persisted). The audit's own summary implies group 1.15 = A 7 / B 4 / C 14 / D 14, while the only element-level reconstruction available implies A 7 / B 3 / C 19 / D 10. The two agree on A = 7 and on the total of 39, and disagree on **5 identifiers** (B/C/D only). Nothing in the code contradicts either. → `VER-01`: re-derive the 36 unpersisted group-1.15 identifiers from the code before STAGE-09 starts. The A set is unaffected, so no preserved element is at risk.
3. **The audit's narrative line "the medical-history block cannot answer 12 of 15 questions" is confirmed** by direct inspection, and its cause is now stated precisely. `MedicalConditionTypeConfiguration` seeds exactly one row (id 1, pregnancy), and **twelve** of the fifteen questions of `1.2.2.3` are answered through catalogue rows — so **eleven** catalogue rows are missing; the twelfth unanswerable question is the imaging question, whose ultrasound half is missing. The other three questions (fasting toggle, fasting hours, pregnancy) are answerable today, and two of those three are answered by direct `Patient` properties rather than by catalogue rows. The gap is therefore a *seed-data* gap of **eleven rows** plus one missing column, not a UI gap — the block is already data-driven.
4. **The audit's narrative line "the search screen has no branch selector" is confirmed** and, additionally, `Domain/Common/BranchScope.cs` and `SystemSettings.BranchNumber` are branch-deployment artefacts. Under Exclusion 4 they are **out of scope** and are not to be wired, extended, or removed by any stage.
5. **The supplementary transcript's claim that the audit totals are 146/108/61/176 is only partly supported.** Its 146 and its per-unit totals for groups 1.1–1.14 are corroborated; its group-1.15 distribution is not (see item 2). Its *stage decomposition* was treated as a proposal only: every stage boundary in this plan was re-derived from the code, and several boundaries differ (see Section D.2). One transcript proposal was **rejected** outright: it placed the local Word/Image export formats in scope, which would re-introduce portal-adjacent work; the audit's own evidence places those two formats inside the excluded site-result-view block (Section C.2, `DEC-04`).

**Unverified items carried into the plan as `VERIFY` dispositions** (20 identifiers) are listed in Section C.3. **Not verified at all, because the task forbids execution:** the applied state of the 21 migrations on any real database; the runtime behaviour of QuestPDF line spacing, offsets and shadows; and the seeded values of `MedicalConditionType`, `PrinterAssignment`, `ReportItemTemplate` and `SystemSettings` in a live database. Each of these is named as an inspection step in the stage that depends on it (`VER-02`, `VER-03`).

---

## C. Scope Decisions

### C.1 Disposition vocabulary

Every identifier in Section G carries exactly one disposition from this closed set. Dispositions are **not** interchangeable, and each implies a different kind of work.

| Disposition | Meaning | Work implied |
|---|---|---|
| `IMPLEMENT` | The capability is absent (baseline class D). | Build it, wire it, and prove it. |
| `COMPLETE` | Present but incomplete or in the wrong place (baseline class B). | Finish or relocate it; do not rebuild it. |
| `CORRECT` | Present but observably different (baseline class C). | Change the behaviour so the observable result matches the baseline. |
| `PRESERVE` | Already matches the baseline (baseline class A). | **No implementation work.** Regression-only. |
| `VERIFY` | A specific detail of the requirement is not established by the baseline. | Implement the documented default **and** record the decision request; never invent a clinical meaning. |
| `EXCLUDED` | Outside Top-Lab's required functionality by one of the five owner-approved exclusions. | **No work, ever.** Not a gap, not a defect, not a parity failure. |

`VERIFY` never replaces a class-derived disposition; it is recorded as an **additional** attribute on the same row, so an uncertain element still carries its `IMPLEMENT` / `COMPLETE` / `CORRECT` obligation plus the decision request.

### C.2 The five exclusions, applied element by element

**Exclusion 1 — public online patient-results portal.** The baseline places the external-service delivery capability in group **1.13**, whose definitions are the signed-in *site* user's view, upload screen, site sign-in, site account types, and site access data printed on the receipt. All 24 portal-facing identifiers are excluded: `1.13.1`–`1.13.1.7`, `1.13.2`, `1.13.3`, `1.13.4`–`1.13.4.3`, `1.13.5`–`1.13.5.3`, `1.13.6`, `1.13.6.1`, `1.13.7`, plus `1.2.2.4.7` (site upload action), `1.2.2.4.8` (site reference field) and `1.14.1.4` (site access label).

Three identifiers in group 1.13 are **retained** because they are local document-generation capabilities rather than portal capabilities, and because the baseline separately requires them in scope:

| Identifier | Why retained | Where delivered |
|---|---|---|
| `1.13.6.2.1` Print Format | Printing a result is required locally by `1.6.1.3.3` / `1.6.1.7.2`; the gap is only that the print choice is not offered at the export action. | STAGE-08 |
| `1.13.6.2.3` PDF Format | Already implemented locally by `ExportPatientReportPdfCommandHandler`; the gap is that the exporter ignores the saved report settings. | STAGE-08 |
| `1.13.6.2.2` Word Format, `1.13.6.2.4` Image Format | Additional **local** export formats of the same result document. Implemented offline: the image format through the renderer already shipped in the pinned QuestPDF package, the Word format through a Word-readable document writer that needs no new package. | STAGE-08 |

`1.13.6.2` (the container) is retained as `COMPLETE`: the local export action gains the full format set.

**Exclusion 4 — multi-branch support.** `1.10.1.1.10` (search within branch) and `1.10.1.2.1` (branch selector) are excluded. The surrounding container `1.10.1.2` (search scope block) is retained and its **source** selector `1.10.1.2.2` is implemented; only the branch half of the block is dropped. The pre-existing branch artefacts in the code (`Domain/Common/BranchScope.cs`, `SystemSettings.BranchNumber`, the `20260930170644_AddBranchNumber` migration) are **not** to be extended, surfaced, or removed by any stage; they are inert to this unit.

**Exclusion 5 — external result delivery.** `1.2.2.4.3` (send-SMS field), `1.2.2.4.4` (send-e-mail field), `1.2.2.4.5` (fax field), `1.2.2.4.6` (send action), `1.2.2.5.9` (group send e-mail), `1.4.1.4.12` (send-SMS action) are excluded. The container `1.2.2.4` (notes and communication block) is retained for its **notes** and **print-result-password** children only.

**Exclusions 2 and 3 — equipment inventory/maintenance/calibration, and analyzer connectivity/automated result import.** Neither maps to any identifier of this unit: the unit contains no equipment-register element and no analyzer-integration element. Nothing is excluded on their account, and nothing in this plan implements either capability. Manual result entry (`1.6.1.5`, `1.6.1.6`) and specimen/sample workflows (`1.12.1.9`, `1.14`) remain **fully in scope** and are scheduled.

**Total excluded: 32 identifiers** (24 under Exclusion 1, 2 under Exclusion 4, 6 under Exclusion 5). In scope: **459 identifiers** — 146 `PRESERVE`, 84 `COMPLETE`, 88 `CORRECT`, 141 `IMPLEMENT`, of which 20 additionally carry `VERIFY`.

**Exclusion audit evidence.** Every one of the 32 excluded identifiers is marked `EXCLUDED` in the Section G.3 matrix, carries no stage, no acceptance criterion and no migration, and is not reported as a gap: `1.2.2.4.3`–`1.2.2.4.6`, `1.2.2.4.7`, `1.2.2.4.8`, `1.2.2.5.9` (Exclusion 5: 6); `1.13.1`–`1.13.1.7`, `1.13.2`, `1.13.3`–`1.13.5.3`, `1.13.6`, `1.13.6.1`, `1.13.7`, `1.2.2.4.7`, `1.2.2.4.8`, `1.14.1.4` (Exclusion 1: 24); `1.10.1.1.10`, `1.10.1.2.1` (Exclusion 4: 2). No stage work item, acceptance criterion, traceability row or migration schedules any of these as required implementation work, under any wording. The local capabilities adjacent to the exclusions — printing, local PDF/Word/image export, local result viewing, manual result entry (`1.6.1.5`, `1.6.1.6`), and specimen/sample workflows (`1.12.1.9`, `1.14`) — remain in scope and are scheduled (STAGE-03, STAGE-08, STAGE-12, STAGE-14 respectively). No replacement portal, "lite" portal, or partial variant of an excluded capability is proposed anywhere in this plan.

### C.3 The 20 `VERIFY` items, their defaults, and their decision owners

Each item is implemented at the **documented default** below so that the stage is never blocked, and each is simultaneously raised as a decision request. No default encodes a clinical meaning that the baseline does not establish.

| # | Identifier | Open question | Documented default implemented | Decision owner |
|---|---|---|---|---|
| 1 | `1.1.5.2` | Meaning of the quick-search mode codes TM / TG / CG | Three modes over the patient module search: **T** = by test, **G** = by group, **C** = by clinical/medical-history condition | Laboratory owner |
| 2 | `1.2.1.11` | The test-registration statistics screen is never shown | Reuse the existing statistics module and its `GetTestCountStatisticsQuery`; the hub action opens it pre-filtered to the current period | Laboratory owner |
| 3 | `1.2.2.2.8` | Relation between the identity-block sample type and the per-test radio group | Identity-block sample type is a **read-only summary** of the sample types on the visit's registered tests; the per-test radio group remains the only editable source | Laboratory owner |
| 4 | `1.2.2.5.1.2` | Meaning of the test-filter mode codes TM / TG / CG | **T** = filter by test name/code, **G** = filter by test group, **C** = filter by custom group | Laboratory owner |
| 5 | `1.2.2.6.12` | The identification card is never shown | A patient identification card carrying the patient code, name, sex/age and the visit's laboratory identifier, printed from the registration action band | Laboratory owner |
| 6 | `1.3.1.13` | Behaviour of the custody action | Opens a read-only list of the visit's open (unsettled) account operations with the receiving user and date | Laboratory owner |
| 7 | `1.3.2.1.5` | Which colours the colour-code column carries | Colour derived from the operation type and void state: payment, correction, refund, settlement, void | Laboratory owner |
| 8 | `1.4.1.1.4` | Which "department" the delivery filter selects | Filter by the patient's **referring entity** — the only organisational attribute the data model carries | Laboratory owner |
| 9 | `1.4.1.3.1.2` | Content of the delivery grid's P column | Same semantic as `1.6.1.2.2` | Laboratory owner |
| 10 | `1.4.1.3.1.3` | Content of the delivery grid's T column | Same semantic as `1.6.1.2.3` | Laboratory owner |
| 11 | `1.6.1.2.2` | Content of the result grid's P column | **P** = the test is part of a **P**rofile (multi-analyte) test | Laboratory owner |
| 12 | `1.6.1.2.3` | Content of the result grid's T column | **T** = the test is part of a **T**est group | Laboratory owner |
| 13 | `1.6.1.2.6` | Relation between the abnormal-flag column and the low-and-high column | The flag is **derived from** the same snapshot band that the low-and-high column shows; the two always agree | Laboratory owner |
| 14 | `1.6.1.5.4` | The exact rule that sets the low-or-high flag | Flag is **computed** from the analyte band for the patient's sex and age, with manual override allowed only before print | Laboratory owner |
| 15 | `1.10.1.2.2` | What "the backup" source physically is | A **separately configured secondary database** (the nightly copy), reached through a second connection string in the workstation configuration file. Restoring on demand is rejected: it is destructive and needs elevated rights | Laboratory owner + IT |
| 16 | `1.10.1.4.6` | Behaviour of the restrict-to-VIP action | Toggles the search result to VIP patients only, and clears itself when pressed again | Laboratory owner |
| 17 | `1.10.1.4.8` | Behaviour of the collected-results action | Opens the combined report for the selected patient's registered tests | Laboratory owner |
| 18 | `1.10.1.4.9` | Behaviour of the modify-group action | Changes the selected patient's account type | Laboratory owner |
| 19 | `1.14.2.7` | Exact composition of the file barcode number | Leading digit = `1`, then the laboratory code, then a zero-padded per-visit sequence | Laboratory owner |
| 20 | `1.1.3.1.1` | File-database option at startup | Offered as a locally attached file-backed SQL Server instance (same engine, one schema); presented but unavailable unless the deployment confirms a file-attached database (Outcome B) — decided under `VER-03` before STAGE-15 is approved | Database owner + Laboratory owner |

### C.4 Recording decisions

Every `VERIFY` item requires a decision record in the stage's handoff note: the item, the default implemented, the date, and the outcome once the owner answers. If an owner's answer contradicts the default, the change is a **follow-up edit inside the same stage** — never a new stage, and never a silent reinterpretation of the baseline. Items 9, 10, 11 and 12 form one decision (the meaning of the P and T columns); items 2 and 4 form one decision (the TM/TG/CG codes). Item 20 (file-database option) is decided separately under `VER-03` before STAGE-15 is approved. The effective number of distinct decisions is therefore **16**.

---

## D. Stage Sequence and Dependency Map

### D.1 The fifteen stages

| Order | Stage | Title | Primary surface | Migration decision |
|---|---|---|---|---|
| 1 | **STAGE-01** | Patient master data, registration header and medical history | `PatientRegistration` | `NEW MIGRATION REQUIRED` |
| 2 | **STAGE-02** | Registration action band and screen navigation | `PatientRegistration` | `NO MIGRATION REQUIRED` |
| 3 | **STAGE-03** | Test selection: filter bar, groups, counts, sample-type control | `PatientRegistration` | `NO MIGRATION REQUIRED` |
| 4 | **STAGE-04** | Patient account panel and account screen | `PatientBilling` | `NEW MIGRATION REQUIRED` |
| 5 | **STAGE-05** | Laboratory identifier, patient search, visit history, modification paths | `PatientSearch` | `NO MIGRATION REQUIRED` |
| 6 | **STAGE-06** | Result entry: patient panel, result grid, entry action block | `ResultsEntry` | `NEW MIGRATION REQUIRED` |
| 7 | **STAGE-07** | Report entry grid, entry variants, report screen actions | `ProfileResults` | `NEW MIGRATION REQUIRED` |
| 8 | **STAGE-08** | Report settings screen and rendering controls | `SystemAndPrintSettings` | `NEW MIGRATION REQUIRED` |
| 9 | **STAGE-09** | Combined report, empty report, patient data sheet | `ReportProduction` | `NO MIGRATION REQUIRED` |
| 10 | **STAGE-10** | Results delivery, delivery-time settlement, delivery outputs | `ResultDelivery` | `NO MIGRATION REQUIRED` |
| 11 | **STAGE-11** | Culture entry, sensitivity classification, microbiology report | `CultureResults` | `NEW MIGRATION REQUIRED` |
| 12 | **STAGE-12** | Specimen containers, barcode label set, test distribution | `SampleCollection` + printing | `NEW MIGRATION REQUIRED` |
| 13 | **STAGE-13** | Medical history insertion and the history report | `ReportProduction` | `NO MIGRATION REQUIRED` |
| 14 | **STAGE-14** | Case follow-up, audit trail, patient samples | `AuditAndTraceability` | `NO MIGRATION REQUIRED` |
| 15 | **STAGE-15** | Application shell, sign-in, startup data source, ribbon navigation | `AccessAndNavigation` | `CONDITIONAL — VERIFY BEFORE DECIDING` |

**Stage identifiers are stable labels, not execution order.** The `Order` column above records each stage's stable identifier (STAGE-01 … STAGE-15) and is used for all cross-references in Sections E, F and G. It must not be renumbered, because every work item, acceptance criterion and traceability row cites the stable ID.

The **executable sequence** below respects every documented dependency (Section D.2) and is the order in which stages must be started. Each stage's completion gate must be met before the next stage in the sequence begins.

| Exec. seq. | Stage | Must complete after | Notes |
|---|---|---|---|
| 1 | STAGE-01 | — | First stage; no prerequisite. |
| 2 | STAGE-02 | STAGE-01 | Needs card number + condition rows. |
| 3 | STAGE-03 | STAGE-01 | Needs the medical-history condition catalogue. |
| 4 | STAGE-04 | STAGE-01 | Independent of 02/03; may run in parallel once 01 is done. |
| 5 | STAGE-05 | STAGE-01, STAGE-02 | Needs card number (search criterion) and the registration navigation targets. |
| 6 | STAGE-06 | STAGE-01, STAGE-03, STAGE-05 | Needs registered tests and the patient-panel data. |
| 7 | STAGE-07 | STAGE-06 | Needs the frozen band, unit and flag columns. |
| 8 | STAGE-08 | STAGE-06, STAGE-07 | Needs the report-entry grid and the status flag/band the settings print. |
| 9 | STAGE-09 | STAGE-08 | Needs the saved report settings and the fixed print options. |
| 10 | STAGE-11 | STAGE-08 | Needs the report settings that shape the microbiology report. Runs in parallel with STAGE-09. |
| 11 | STAGE-12 | STAGE-03, STAGE-08 | Needs the registered tests and label rendering settings. Runs in parallel with STAGE-09/11. |
| 12 | STAGE-13 | STAGE-07, STAGE-08, STAGE-09 | Needs the combined-report pipeline (separate-report mode) and report settings. |
| 13 | STAGE-10 | STAGE-04, STAGE-05, STAGE-08, STAGE-13 | Needs the account panel (04), search/patient data (05), export formats (08) and the history report (13). |
| 14 | STAGE-14 | STAGE-12 | Needs the container model (patient samples block). Runs in parallel with STAGE-10/15 once its prerequisite is met. |
| 15 | STAGE-15 | STAGE-10, STAGE-05 | Needs the delivery screen (Send Result) and the search queries (quick-search toolbar). |

**Hard ordering constraints** (violating any of these breaks a later stage):

1. `STAGE-01` before `STAGE-02`, `STAGE-03`, `STAGE-05` — the card number, print-result password and the medical-history condition rows are produced there.
2. `STAGE-06` before `STAGE-07` and `STAGE-10` — the finished/unit/critical columns are produced there.
3. `STAGE-07` before `STAGE-08` — the report-settings dialog renders the report-entry grid's flag and range columns.
4. `STAGE-08` before `STAGE-09`, `STAGE-10`, `STAGE-11`, `STAGE-13` — every report artefact reads the persisted rendering settings.
5. `STAGE-09` before `STAGE-13` — the history report's separate-report mode reuses the combined-report pipeline.
6. `STAGE-12` before `STAGE-14` — the patient-samples block lists containers.
7. `STAGE-10` before `STAGE-15` — the Send Result ribbon item opens the delivery screen.
8. `STAGE-05` before `STAGE-15` — the patient-module quick-search toolbar is built on the search queries.

**Resolved conflict:** STAGE-13 (history report) must precede STAGE-10 (delivery), because STAGE-10's history action depends on STAGE-13's dialog and report. The stable numbering keeps STAGE-10 before STAGE-13 in the label sequence, but execution follows the table above: STAGE-13 runs at exec. seq. 12, before STAGE-10 at exec. seq. 13. No stage ID is renumbered.

**Stages that may run in parallel** once their prerequisites are met: STAGE-02 ∥ STAGE-03 ∥ STAGE-04 ∥ STAGE-05 (after STAGE-01); STAGE-06 ∥ STAGE-12 (after their prerequisites); STAGE-09 ∥ STAGE-11 (after STAGE-08); STAGE-14 runs after STAGE-12 in parallel with STAGE-10/15.

### D.2 Dependency map

```
STAGE-01  patient master data, medical history
    │  (card-number column, print-password columns, condition-type seed)
    ├──────────────► STAGE-02  action band & navigation ──┐
    │                 (needs card number + medical-history rows)│
    ├──────────────► STAGE-03  test selection / filter / groups│
    │                                                       │
    ├──────────────────────────────────────────► STAGE-05  lab id, search, visit history
    │                                                       │  (needs card number)
    ├──────────────────────────────────────────► STAGE-06  result entry grid
    │                                                       │  (needs finished/unit/critical columns)
    │                                                       │
    ├──────────────────────────────────────────► STAGE-04  account panel & screen
    │                                                       │
    │                                     ┌─────────────────► STAGE-07  report entry grid & variants
    │                                     │                    (needs STAGE-06 columns)
    │                                     │                          │
    │                                     │                          ▼
    │                                     │                    STAGE-08  report settings & rendering
    │                                     │                          │
    │                                     │                          ▼
    │                                     │                    STAGE-09  combined & empty report
    │                                     │                    STAGE-13  medical history & history report
    │                                     │                          │
    │                                     ▼                          ▼
    │                                  STAGE-10  results delivery ────┘
    │                                  (needs account STAGE-04, export formats STAGE-08,
    │                                   history STAGE-13, delivery-list writer)
    │
    ├──────────────────────────────────────────► STAGE-11  culture & microbiology report
    │                                           (needs STAGE-08 rendering controls)
    │
    ├──────────────────────────────────────────► STAGE-12  containers & barcode labels
    │                                           (needs STAGE-03 test catalogue)
    │                                                 │
    │                                                 ▼
    │                                            STAGE-14  case follow-up & patient samples
    │                                            (needs STAGE-12 container model)
    │
    └──────────────────────────────────────────► STAGE-15  shell & sign-in
                                                (needs STAGE-10 delivery screen for Send Result,
                                                 STAGE-05 for the quick-search toolbar)
```

**Hard ordering constraints** (violating any of these breaks a later stage):

1. `STAGE-01` before `STAGE-02`, `STAGE-03`, `STAGE-05` — the card number, print-result password and the medical-history condition rows are produced there.
2. `STAGE-06` before `STAGE-07` and `STAGE-10` — the finished/unit/critical columns are produced there.
3. `STAGE-07` before `STAGE-08` — the report-settings dialog renders the report-entry grid's flag and range columns.
4. `STAGE-08` before `STAGE-09`, `STAGE-10`, `STAGE-11`, `STAGE-13` — every report artefact reads the persisted rendering settings.
5. `STAGE-09` before `STAGE-13` — the history report's separate-report mode reuses the combined-report pipeline.
6. `STAGE-12` before `STAGE-14` — the patient-samples block lists containers.
7. `STAGE-10` before `STAGE-15` — the Send Result ribbon item opens the delivery screen.
8. `STAGE-05` before `STAGE-15` — the patient-module quick-search toolbar is built on the search queries.

**Stages that may run in parallel** once their prerequisites are met: `STAGE-02` ∥ `STAGE-03` ∥ `STAGE-04` ∥ `STAGE-05`; then `STAGE-06` ∥ `STAGE-12`; then `STAGE-07` → `STAGE-08` → {`STAGE-09`, `STAGE-11`} → `STAGE-13` → `STAGE-10` → `STAGE-15`, and `STAGE-12` → `STAGE-14` (STAGE-14 runs after STAGE-12, in parallel with STAGE-10/15 once STAGE-12 is done).

### D.3 Boundary decisions that differ from a naive grouping

Three groupings were considered and rejected, with reasons recorded so the sequence is not re-litigated:

- **Shell last vs. shell first.** The shell (ribbon, status bar, splash, sign-in) is sequenced **last** because `1.1.1.1.3` (Send Result) and `1.1.5` (patient-module quick-search toolbar) both point at surfaces that only exist once their owning stage has run. Sequencing the shell first would force placeholder navigation targets.
- **Medical history inside the registration stage.** The medical-history toggles (`1.2.2.3.x`) are data-driven from `MedicalConditionType`, so they are a **seed-data** change and belong with the schema stage, not with the screen work of STAGE-02. Keeping them in STAGE-01 avoids a second migration in the same area and puts the condition catalogue and its screen in one reviewable change.
- **Delivery vs. culture ordering.** STAGE-11 (culture) needs only STAGE-08, so culture can proceed as soon as STAGE-08 lands. STAGE-10 (delivery) needs STAGE-08's export formats and STAGE-13's history report, so delivery is gated behind STAGE-13 (exec. seq. 12) rather than running immediately after STAGE-08. Culture therefore runs in parallel with the 08 → 09 → 13 chain and is not a predecessor or successor of delivery. The longest chain is `01 → {02,03,04,05} → 06 → 07 → 08 → 09 → 13 → 10 → 15`, with `11` and `12 → 14` as parallel branches. This keeps the critical path contiguous while avoiding the earlier mistake of placing STAGE-10 before STAGE-13 despite STAGE-10's dependency on STAGE-13's history.

---

## E. Implementation Stages

Each stage below is written to be executed without further research. Every stage states its objective, prerequisites, verified current state, exact work items, traceability, migration decision and details, acceptance criteria, testing requirements, preservation constraints and its completion gate.

> **Notation.** `1.2.2.5.11` denotes a baseline identifier. `Class A/B/C/D` denotes the baseline conformance class. Work items name concrete files; a file named without a directory is relative to the project root.

### E.1 STAGE-01 — Patient master data, registration header and medical history

**7.1 Objective and scope.** Give the patient record the fields the baseline requires — card number, entry date, receipt date, print-result password — and make the medical-history block answer **all fifteen** of its baseline questions instead of three. Covers baseline identifiers `1.2.2.1`, `1.2.2.1.1`, `1.2.2.1.2`, `1.2.2.2`, `1.2.2.2.6`, `1.2.2.2.8`, `1.2.2.2.10.1`, `1.2.2.2.11.1`, `1.2.2.3`, `1.2.2.3.3`–`1.2.2.3.15`, `1.2.2.4`, `1.2.2.4.1`, `1.2.2.4.2`, `1.2.2.4.2.1`. **Out of scope:** the screen layout of the registration form (STAGE-02), the test-selection block (STAGE-03), the laboratory-identifier lifecycle (STAGE-05), the account panel (STAGE-04).

**The fifteen questions of the medical-history block, and what each one is stored as.** The fifteen questions of `1.2.2.3` are **not** fifteen catalogue rows: twelve of them are answered through `MedicalConditionType` rows and three of them are answered through direct `Patient` properties.

| Baseline question(s) | Baseline Arabic label | Stored as | State at baseline |
|---|---|---|---|
| `1.2.2.3.1` Fasting Toggle | صائم | `Patient.IsFastingIndicated` | works — class A, preserved |
| `1.2.2.3.2` Fasting Hours Field | عدد ساعات الصيام | `Patient.FastingHours` | works — class A, preserved |
| `1.2.2.3.3`–`1.2.2.3.8` (6) | هل يؤخذ علاج للسكر … هل يؤخذ علاج للكبد | `MedicalConditionType` rows, `Category = Medication` | **six catalogue rows missing** |
| `1.2.2.3.9` Pregnancy Toggle | للسيدات هل يوجد حمل | `MedicalConditionType` row id 1 (`حمل`, `Pregnancy`) + `PatientMedicalCondition` join | the row exists; it is offered to every patient — class B |
| `1.2.2.3.10` Imaging Within Two Days Toggle | هل تم عمل اشاعة بالصبغة او الموجات الصوتية خلال يومين | `Patient.RecentContrastImaging` + the ultrasound half added by this stage | the contrast half only — class B |
| `1.2.2.3.11`–`1.2.2.3.15` (5) | مريض بفقر الدم … مريض بالذئبة الحمراء | `MedicalConditionType` rows, `Category = Condition` | **five catalogue rows missing** |

**Totals, and the number of new catalogue rows.** 15 questions = 12 catalogue-backed + 3 patient-property-backed. Of the twelve catalogue-backed questions, **one** row already exists (id 1, pregnancy), so **exactly eleven new `MedicalConditionType` rows are required** — six `Medication` and five `Condition`. The three patient-property-backed questions need **no** catalogue row. The baseline's **notes field (`1.2.2.4.1`, ملاحظات) is not one of the fifteen**: it belongs to the adjacent notes-and-communication block, already exists as `Patient.Notes` (`nvarchar(1000)`), is preserved, and must not be counted as a medical-history question.

**7.2 Prerequisites and dependencies.** None — this is the first stage. It is a hard prerequisite for STAGE-02 (needs the card number and the condition rows), STAGE-03 (needs the condition catalogue), STAGE-05 (needs the card number for the search criterion).

**7.3 Verified current state.**
- `Patient` (`src/TopLab.Domain/Patients/Patient.cs:11-300`) has **no** card-number member, no ultrasound flag and no print-password member. `PickupDateUtc` exists (line 39) but is never written: `CreatePatientCommand` carries it (`CreatePatientCommand.cs:26`), `UpdatePatientCommand` does not, and `PatientEditorViewModel` passes `null` for it on create (`PatientEditorViewModel.cs:876`).
- `Patient.Notes` exists (line 47), is written by `CreatePatientCommandHandler.cs:108`, and is read back into `PatientDetailDto.Notes` (`PatientRegistrationDtos.cs:37`).
- The medical-history block is **data-driven**: `PatientEditorView.xaml:218-224` binds an `ItemsControl` to the view model's `Conditions` collection, which is filled from the catalogue at `PatientEditorViewModel.cs:510-514`; `MedicalConditionTypeConfiguration.cs:17` seeds **exactly one row** (id 1, `حمل`, `Pregnancy`). Consequently **eleven** of the twelve catalogue-backed questions cannot be answered today, and the imaging question (`1.2.2.3.10`) is only half answerable — the block is short of **twelve** of its fifteen questions, and three of the fifteen (fasting toggle, fasting hours, pregnancy) already work.
- `MedicalConditionCategory` already carries `Medication = 0`, `Condition = 1`, `Pregnancy = 2` — the three categories the baseline needs. `PregnancySignal` (`src/TopLab.Application/Features/CultureResults/Common/PregnancySignal.cs`) raises the culture-entry pregnancy indication from the `Pregnancy` category alone, so the eleven new rows must never be given that category.
- `GetMedicalConditionTypesQuery` returns the seeded rows and already supports a category filter; its DTO carries the category (`MedicalConditionTypeDto(int MedicalConditionTypeId, string Name, MedicalConditionCategory Category)`, `PatientRegistrationDtos.cs:52`), and `GetRegistrationCatalogQueryHandler` calls it without a filter. There is no administration screen for the catalogue.
- `Sex` is `Male = 0, Female = 1` — there is **no** "unknown sex" state — and the registration screen's sex control is a single check box labelled `ذكر (غير محدد = أنثى)` (`PatientEditorView.xaml:168`), so the recorded sex is always male or female.
- `PatientMedicalCondition` has **no foreign key** to `MedicalConditionTypes` (`PatientMedicalConditionConfiguration.cs:14` — "removed FK to avoid shadow - convention"), so deleting a catalogue row would leave dangling join rows: the answer would remain stored on the patient but disappear from the block.
- The catalogue is an `IDENTITY` key table (`UseIdentityColumn` in `ApplicationDbContextModelSnapshot.cs:333`) that is nevertheless seeded with explicit identifiers — the pregnancy row (`20260910213833_AddPregnancyMedicalConditionTypeSeed`) and the thirteen `Permissions` rows (`PermissionConfiguration.cs`). Seeding explicit identifiers is therefore the repository's established convention, and identifier 1 is the only identifier `MedicalConditionTypes` currently holds.

**7.4 Exact work items.**

*W1.1 — Card number.* Add `CardNumber` (string, max 30, nullable) to `Patient`; expose it in `CreatePatientCommand`, `UpdatePatientCommand`, `Patient.Create`, `Patient.Update`, `PatientEditorViewModel`, and the registration view's identity block; index it for the STAGE-05 search criterion (`PatientConfiguration`: `b.HasIndex(e => e.CardNumber)`, non-unique, exactly as `NationalId` is indexed). Both patient commands are **positional records with several construction sites** (`PatientEditorViewModel.cs:876` and `:912`, plus the application tests), so a new parameter is **appended** and every construction site is updated in the same change. Baseline `1.2.2.2.6` (class D).

*W1.2 — Identity-block sample type.* The baseline's identity block carries a sample-type field (`1.2.2.2.8`, class C), and the relation between that field and the per-test sample-type radio group is not established (VERIFY #3). The documented default stands and is implemented as stated: the identity-block field is a **read-only summary of the sample types already set on the visit's registered tests** (`PatientTest`'s five booleans `IsUrine`, `IsStool`, `IsBlood`, `IsSemen`, `IsCsf`), and the per-test radio group remains the only editable source. Consequently **no patient-level sample-type column is added and no new sample-type enum is introduced**; the field is derived in the view model from the visit's registered tests and is re-evaluated whenever those tests or their sample flags change. A test row added for the first time therefore starts with no sample type selected — there is no patient-level default for this stage to apply. VERIFY #3 is recorded in the stage handoff.

*W1.3 — Receipt date.* Wire `PickupDateUtc` end to end: add it to `UpdatePatientCommand` and `Patient.Update`, bind it to the registration header, and default it to the registration moment on create. The column **already exists** (`Patient.cs:39`, `PatientConfiguration.cs:34`), so this item needs **no schema change**. Baseline `1.2.2.1.2` (class D).

*W1.4 — Entry date display and correction.* Surface `RegistrationDateUtc` (already stored, `Patient.cs:37`) in the registration header as a value that can be corrected before save; today it is generated (`PatientEditorViewModel.cs:875`) and never shown. Correction means `Patient.Update` accepts a registration date — a domain change, but **no schema change**. Baseline `1.2.2.1.1` (class C).

*W1.5 — Print-result password.* The baseline places a print-result password and an enable option in the patient's communication block (`1.2.2.4.2`, `1.2.2.4.2.1`, class D). The password governs **printing**, which is a station-wide act, so it is implemented as a **system setting**: add `PrintResultPasswordEnabled` (bool) and `PrintResultPasswordHash` (string, max 300, nullable) to `SystemSettings`; edit them from the registration screen's communication block through a new `UpdatePrintResultPasswordCommand` (`src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdatePrintResultPassword/`); and expose one shared verifier (`PrintResultPasswordVerifier`) that STAGE-06's F12, STAGE-08 and STAGE-10 call at the print entry points. **Only a hash is stored**: PBKDF2-HMAC-SHA256, 100 000 iterations, a 16-byte random salt and a 32-byte sub-key, encoded as the single string `pbkdf2$<iterations>$<base64 salt>$<base64 subkey>` (≤ 300 characters). The plain password is never written to the database, to the workstation configuration file or to a log. If a patient-scoped password is required instead, that is a stage-local rework and not a new stage; the decision is recorded in the stage handoff.

*W1.6 — Medical-history catalogue: eleven new rows.* Extend the `HasData` of `MedicalConditionTypeConfiguration` with **exactly eleven** new rows, ids **2–12**, carrying the baseline's own Arabic labels and the baseline's categories, so that the block answers all twelve catalogue-backed questions:

| id | `Name` | `Category` | Baseline question |
|---|---|---|---|
| 1 | حمل | `Pregnancy` | `1.2.2.3.9` — **existing row, untouched** |
| 2 | هل يؤخذ علاج للسكر | `Medication` | `1.2.2.3.3` |
| 3 | هل يؤخذ علاج لسيولة الدم | `Medication` | `1.2.2.3.4` |
| 4 | هل يؤخذ علاج للغدد | `Medication` | `1.2.2.3.5` |
| 5 | هل يؤخذ علاج للفيروسات | `Medication` | `1.2.2.3.6` |
| 6 | هل يؤخذ مضاد حيوي | `Medication` | `1.2.2.3.7` |
| 7 | هل يؤخذ علاج للكبد | `Medication` | `1.2.2.3.8` |
| 8 | مريض بفقر الدم | `Condition` | `1.2.2.3.11` |
| 9 | مريض بالتهاب المفاصل | `Condition` | `1.2.2.3.12` |
| 10 | مريض بالضغط الدم | `Condition` | `1.2.2.3.13` |
| 11 | مريض بالفشل كلوي | `Condition` | `1.2.2.3.14` |
| 12 | مريض بالذئبة الحمراء | `Condition` | `1.2.2.3.15` |

The eleven Arabic names are the baseline's own labels (`Docs/The reviewer/01-patient-registration-and-test-registration.md:608-620`); **no label is invented**. **Why ids 2–12:** id 1 is the only identifier `MedicalConditionTypes` holds today, the key is an identity column, and the repository's seed convention is sequential explicit identifiers from 1 (`Permissions` 1–13 in `PermissionConfiguration.cs`), so 2–12 are the next free identifiers — no identifier is skipped, none collides, and the identity counter advances past 12 exactly as it does for `Permissions`. **Presentation order:** the block shows the questions in the baseline's sequence — the fasting pair, then the six medication toggles (ids 2–7), then the pregnancy toggle, then the single imaging question, then the five condition toggles (ids 8–12); operator-added catalogue rows appear after the seeded ones. The catalogue query's present `Category, Name` ordering does not produce this sequence, so the ordering is applied explicitly rather than left to the query. Baseline `1.2.2.3.3`–`.3.8`, `1.2.2.3.11`–`.3.15` (class D), plus the block `1.2.2.3` (class B) and the pregnancy question `1.2.2.3.9` (class B).

*W1.7 — Pregnancy question shown to female patients only.* The baseline rule states the pregnancy question is asked of female patients only; today the row is offered to everyone. Filter the row out of the block when the recorded sex is `Male`, and re-evaluate the filter whenever the sex control changes. Because `Sex` has only `Male` and `Female`, there is no "sex not recorded yet" state and that case does not arise; when the sex is switched to male while the pregnancy row is selected, the selection is cleared, so a male patient never carries a pregnancy answer. The filter uses the category the catalogue query already returns (`Category == Pregnancy`), so no new query is needed; the view model's `ConditionItem` gains the category. Baseline `1.2.2.3.9` (class B).

*W1.8 — Imaging within two days: one question, two answers.* The baseline question `1.2.2.3.10` is **one** toggle covering "a contrast study **or** an ultrasound within the last two days". `RecentContrastImaging` covers the contrast half only. Add `RecentUltrasoundImaging` to `Patient` as a second persisted boolean and present the baseline's **single** question with two answers — a choice between *contrast*, *ultrasound* and *both* (or an equivalent pair of answer controls under the one question label) — so that the block still presents **fifteen** questions and not sixteen. The stored pair `RecentContrastImaging` / `RecentUltrasoundImaging` satisfies the question when either flag is set. Baseline `1.2.2.3.10` (class B).

*W1.9 — Condition-catalogue administration.* Add a small maintenance screen (Laboratory module) listing `MedicalConditionType` rows with **add** and **rename**, so that the catalogue the baseline depends on is maintainable without a migration. The screen is deliberately limited to those two operations: **no deactivation and no deletion**. Deactivation would need an active flag, which this entity does not carry and which this stage's migration does not add; and `PatientMedicalConditions` has no foreign key to the catalogue, so deleting a row would leave dangling join rows and silently drop a patient's recorded answer. The seeded rows (ids 1–12) are read-only in the screen, so the catalogue cannot drift from the migration's `HasData` snapshot; operator-added rows may be renamed freely. This item is a maintainability enabler for `1.2.2.3`, not a baseline element of its own.

*W1.10 — Additional information of the referring entity and the treating doctor.* Section G assigns `1.2.2.2.10.1` and `1.2.2.2.11.1` to this stage, so the stage discharges them. An entity's additional information is the data the model already carries (`ExternalEntity`: city, address, phone, fax, responsible-person name and phone, e-mail, price list, commission percent — `ExternalEntity.cs`), and it is already editable in `ExternalEntityEditorWindow`. Implement the two identity-block actions as a **read-only** view of the selected entity's additional-information fields, reusing the existing window and view model with editing disabled. **No schema change and no new data.** Baseline `1.2.2.2.10.1`, `1.2.2.2.11.1` (class D).

*W1.11 — Notes field carried into the result-entry and result-delivery patient data.* Section G assigns `1.2.2.4.1` to this stage. The audit's residual for it is that the notes are stored but do not appear in the result-entry and result-delivery windows, because `ResultsEntryDtos` / `ResultDeliveryDtos` carry no notes field. Add `Notes` to the patient-detail DTO of the result-entry and the result-delivery queries — an **additive DTO field only**: the column exists as `Patient.Notes` (`nvarchar(1000)`) and **no schema change** is needed. The **rendering** of the notes in those two windows belongs to the stages that own those screens (STAGE-06 and STAGE-10); this stage delivers the data half and makes the notes a preserved behaviour of both windows. Baseline `1.2.2.4.1` (class B).

**7.5 Traceability.** `1.2.2.1` (B→COMPLETE), `1.2.2.1.1` (C→CORRECT), `1.2.2.1.2` (D→IMPLEMENT), `1.2.2.2` (B→COMPLETE), `1.2.2.2.6` (D→IMPLEMENT), `1.2.2.2.8` (C→CORRECT + VERIFY #3), `1.2.2.2.10.1`/`.11.1` (D→IMPLEMENT), `1.2.2.3` (B→COMPLETE), `1.2.2.3.1`/`.2` (A→PRESERVE), `1.2.2.3.3`–`.3.8` (D→IMPLEMENT — the six `Medication` rows of W1.6), `1.2.2.3.9` (B→CORRECT), `1.2.2.3.10` (B→CORRECT), `1.2.2.3.11`–`.3.15` (D→IMPLEMENT — the five `Condition` rows of W1.6), `1.2.2.4` (B→COMPLETE), `1.2.2.4.1` (B→COMPLETE — the DTO half in W1.11; the rendering in the two windows is discharged by STAGE-06 and STAGE-10), `1.2.2.4.2`/`.2.1` (D→IMPLEMENT).

**7.6 Database migration decision.** **NEW MIGRATION REQUIRED.**

**7.7 Migration details.**
- *Proposed name:* `AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue`.
- *`Patients` table:* add `CardNumber nvarchar(30) NULL` (with the non-unique index `IX_Patients_CardNumber`) and `RecentUltrasoundImaging bit NOT NULL DEFAULT 0`.
- *`SystemSettings` table:* add `PrintResultPasswordEnabled bit NOT NULL DEFAULT 0` and `PrintResultPasswordHash nvarchar(300) NULL`. `SystemSettings` is a single `HasData` row (id 1, `SystemSettingsConfiguration.cs:25`), so the model's seed is extended with the two new values and the migration carries the matching `UpdateData` for key 1 — exactly as `20260930170644_AddBranchNumber` did for `BranchNumber`.
- *Data seed:* insert **11** `MedicalConditionType` rows (ids 2–12) — six `Medication`, five `Condition`, with the Arabic names listed in W1.6 — through the same `HasData` mechanism, so that a fresh database and a migrated database end with the same twelve catalogue rows. **Append-only**: the existing id-1 pregnancy row is not modified, re-categorised or deleted.
- *No patient-level sample-type column is added* (W1.2): the identity-block sample type is derived, so the migration adds no such column.
- *Backfill:* `RecentUltrasoundImaging` defaults to 0 — no historical value exists to backfill; `CardNumber` and `PrintResultPasswordHash` are nullable with no default and `PrintResultPasswordEnabled` defaults to 0 (= the requirement is off), so no existing row is rewritten and no `UPDATE` statement is needed.
- *Data preservation:* every existing `Patient` and `SystemSettings` row keeps all current values. No column is dropped, renamed or retyped; the `Patients` row count is unchanged; `Patient.Notes`, `PickupDateUtc`, `RecentContrastImaging`, `IsFastingIndicated` and `FastingHours` are not touched; the id-1 pregnancy row keeps its name, its category and its identifier.
- *Rollback:* drop the four added columns; delete the eleven seeded rows (ids 2–12) — and, **before** deleting them, delete the `PatientMedicalConditions` join rows that reference them, because there is no foreign key and those rows would otherwise be left dangling. Those join rows can only exist because this migration created the catalogue entries, so no pre-existing patient data is lost. The id-1 pregnancy row, every patient row and every other row survive.
- *Migration-history caveat:* this migration must be appended **after** `20261018144852_AddProfileResultStyling` and must not modify any earlier migration file. The applied state of the 21 existing migrations on the target database is **unverified** (`VER-02`) — confirm with `dotnet ef migrations list` against the target database before generating; the live seeded values of `MedicalConditionType` and `SystemSettings` are likewise unverified (`VER-03`).
- *Test obligation:* a migration test must apply the full chain to an empty database **and** over a database seeded at `20261018144852_AddProfileResultStyling`, asserting that the four new columns exist with their defaults, that `MedicalConditionTypes` holds exactly the twelve seeded rows (ids 1–12) with the expected categories, that the pre-existing patient, settings and pregnancy rows are unchanged, and that the rollback restores the prior shape — including the removal of the eleven rows and of the join rows that referenced them.

**7.8 Acceptance criteria.**
- A1. The identity block carries the card number: it is persisted, displayed on re-open, indexed for the STAGE-05 search, and refused when longer than 30 characters; the two "additional information" actions open the selected entity's existing additional-information data read-only.
- A2. The registration header shows the entry date **with its time** and the receipt date, both correctable before save, and both survive a save/reload round trip.
- A3. The medical-history block presents exactly the **fifteen** baseline questions of `1.2.2.3` — the fasting toggle and hours field, the **twelve** catalogue-backed toggles (six medication, five condition, one pregnancy) and the single imaging question — of which the **eleven** newly seeded toggles become answerable and the imaging question becomes fully answerable through its ultrasound half; the pregnancy question is offered only to female patients, and the notes field is not counted among the fifteen.
- A4. The imaging question is answered by *contrast*, by *ultrasound* and by *both*, and each of the three answers makes the single baseline question true; the block still presents fifteen questions.
- A5. The print-result password can be set and changed from the registration screen's communication block, is stored only as a PBKDF2 hash (no plain text in the database, the configuration file or the logs), and the enable option gates the requirement; the shared verifier refuses a wrong or missing password when the requirement is enabled. The refusal **at the print entry points themselves** is proven by STAGE-06's F12, STAGE-08 and STAGE-10, which call the same verifier.
- A6. The condition catalogue can be extended and renamed from the Laboratory module without a migration; the seeded rows are read-only there and cannot be deleted or deactivated.
- A7. A patient registered before this stage still opens, edits and saves with all pre-existing values intact, and the id-1 pregnancy row is unchanged.
- A8. The patient's notes are carried in the result-entry and the result-delivery patient data, and the registration screen's notes field keeps its current behaviour.
- A9. The identity-block sample-type field shows the sample types set on the visit's registered tests and is read-only; the per-test sample-type control remains the only editable source.

**7.9 Testing requirements.**
- *Domain tests:* `Patient.Create`/`Update` accept and reject the new fields per their rules; `Patient` rejects a card number longer than 30 characters; the imaging rule is true when either flag is set.
- *Application tests:* `CreatePatientCommandHandler` and `UpdatePatientCommandHandler` persist the card number, the receipt date and the ultrasound flag; `ApplyConditionDeltasCommandHandler` accepts every seeded condition id; the catalogue query returns the **twelve seeded rows** (six `Medication`, five `Condition`, one `Pregnancy`) and its category filter still works; the print-password command stores only a hash and never the plain value.
- *Persistence tests:* the migration chain applies to an empty database and over the previous head; the four new columns exist with their defaults; the eleven seeded rows exist with the right categories and the id-1 pregnancy row is unchanged; the pre-existing patient and settings rows are unchanged; the rollback restores the previous shape.
- *UI tests:* the registration header binds the new fields; the block shows the fifteen questions, of which twelve are catalogue toggles; the pregnancy question is hidden for a male patient and its selection is cleared when the sex changes; the imaging question offers both answers; the password fields mask the value.

**7.10 Preservation and compatibility requirements.** No existing `Patient` or `SystemSettings` value may change. `Patient.Notes`, `PickupDateUtc`, `RecentContrastImaging`, `IsFastingIndicated`, `FastingHours`, `IsVip`, `AccountType`, `LabId` and the phone/condition collections keep their current meaning and their current values. `GetMedicalConditionTypesQuery` keeps returning the same row shape, now with more rows, and keeps its category filter. The `1.2.2.3.1` fasting toggle and `1.2.2.3.2` fasting-hours field keep their behaviour exactly. The id-1 pregnancy row keeps its name, its category and its identifier, and `PregnancySignal` keeps raising the culture-entry pregnancy indication from the `Pregnancy` category alone, so none of the eleven new rows can raise it.

**7.11 Completion gate.** The build succeeds with zero new warnings; all STAGE-01 tests pass; the migration applies and rolls back cleanly on an ephemeral container; the catalogue ends with exactly the twelve seeded rows and the id-1 pregnancy row is unchanged; no other stage's file has been modified; and the decision records for VERIFY #3 and for the print-password scope (W1.5) are written in the stage handoff.

### E.2 STAGE-02 — Registration action band and screen navigation

**7.1 Objective and scope.** Rebuild the registration screen's action band so it carries the baseline's actions in the baseline's place, and make the screen navigate where the baseline says. Covers `1.2.2.6`, `1.2.2.6.2`, `1.2.2.6.3`, `1.2.2.6.8`, `1.2.2.6.9`, `1.2.2.6.12`–`1.2.2.6.16`, `1.2.2.1.5`. **Out of scope:** the identity and medical-history content (STAGE-01), the test-selection block (STAGE-03), the envelope document itself (STAGE-08's printing controls).

**7.2 Prerequisites and dependencies.** STAGE-01 (the band hosts the print-result password fields and the identification card needs the patient's identity).

**7.3 Verified current state.** `PatientEditorView.xaml:39-53` carries nine buttons — new, save, delete, undo, barcode, work sheet, receipt, invoice, lab order — and **none** of the baseline's remaining actions. The baseline's action band is: save, **add entry**, **edit entry**, undo, barcode, receipt, invoice, **envelope print**, **modify**, **follow-up**, work sheet, **identification card**, **patient screen**, **main point**, **patient window**, **lab results**. The envelope print exists today but only on the result-sheet screen (`PatientResultSheetView.xaml:84-85`); "add entry" today clears the form (`PatientEditorViewModel.cs:717-764`); "modify" is not a distinct action; "follow-up", "identification card", "patient screen", "main point", "patient window" and "lab results" have no command at all. `1.2.2.6.1` (save) and `1.2.2.6.4` (undo) work and are preserved.

**7.4 Exact work items.**

*W2.1 — Add entry.* `1.2.2.6.2` asks the action to add the current entry as an **independent record**; today it discards the form. Change it to: persist the current form as its own registration and start a fresh form. Baseline `1.2.2.6.2` (class B).

*W2.2 — Edit entry.* Add a distinct action that opens the current entry for editing, separate from modify (`1.2.2.6.3`, class D).

*W2.3 — Modify.* Add a modify action that reopens the patient's saved data for modification, distinct from edit and from the registration flow (`1.2.2.6.9`, class D).

*W2.4 — Envelope print on the registration screen.* Move the existing envelope print (`PatientResultSheetViewModel.cs:201-214`) so the action band carries it, and keep the result-sheet entry point working (`1.2.2.6.8`, class C).

*W2.5 — Identification card.* Add the identification-card action and its document. The card's content is not established by the baseline (VERIFY #5): default = patient code, name, sex/age, and the visit's laboratory identifier, printed as a document. The action belongs on the registration screen; the document writer is added in STAGE-08's printing layer and consumed here.

*W2.6 — Follow-up.* Add the follow-up action opening the case-history screen (STAGE-14 delivers that screen; the command and navigation land here).

*W2.7 — Patient screen, main point, patient window, lab results.* Add the four navigation actions with the baseline's destinations: the main patient screen, the program's main point, the patient list, and the laboratory results screen. Today the registration screen has a *search* strip instead (`PatientEditorView.xaml:66-130`); keep the search strip as a search convenience, and add the four navigation actions alongside it.

*W2.8 — Laboratory identifier action.* `1.2.2.1.5` requires the header's "Lab. ID" control to **generate or show** the current visit's identifier. Today the field is pre-filled with a *new* code at open time (`PatientEditorViewModel.cs:518-525`) and is not carried on update. Change it to: show the stored identifier when one exists, generate one on save when none exists, and preserve it on update. The reuse rule (one identifier per patient across visits) is STAGE-05's work; this stage only makes the control read/write correctly.

**7.5 Traceability.** `1.2.2.6` (B→COMPLETE), `1.2.2.6.1` (A→PRESERVE), `1.2.2.6.2` (B→CORRECT), `1.2.2.6.3` (D→IMPLEMENT), `1.2.2.6.4` (A→PRESERVE), `1.2.2.6.5` (B→COMPLETE — the barcode action itself is STAGE-12's; here it is *placed* on the band and made to print the full label set), `1.2.2.6.6`/`.7`/`.11` (A→PRESERVE), `1.2.2.6.8` (C→CORRECT), `1.2.2.6.9` (D→IMPLEMENT), `1.2.2.6.10` (D→IMPLEMENT), `1.2.2.6.12`–`.16` (D→IMPLEMENT), `1.2.2.1.5` (B→CORRECT).

**7.6 Database migration decision.** **NO MIGRATION REQUIRED.** The band's actions are commands and navigation targets; every field they read or write already exists or is added by STAGE-01. The identification card and the envelope print reuse the printing layer without schema change.

**7.7 Migration details.** Not applicable — no schema change. The stage must not introduce one: if a new column appears to be needed, the design is wrong.

**7.8 Acceptance criteria.**
- B1. The action band carries every baseline action, each enabled in the baseline's circumstances.
- B2. "Add entry" leaves the saved registration intact and starts a clean form.
- B3. "Edit entry" and "modify" produce different, correct outcomes.
- B4. The envelope can be printed from the registration screen and still from the result sheet.
- B5. The identification card prints with the documented default content.
- B6. Each of the four navigation actions reaches its destination and returns.
- B7. The laboratory-identifier control shows the stored identifier for an existing visit and a new one only for a fresh registration.

**7.9 Testing requirements.**
- *Application tests:* add-entry persists the form and clears it; edit-entry and modify issue their distinct commands; the follow-up command targets the case-history screen.
- *ViewModel tests:* each band command is enabled only when its precondition holds; the identifier control is read-only for a saved visit.
- *UI tests:* every baseline action is present on the band; the envelope button is present on the registration screen and the result sheet.

**7.10 Preservation and compatibility requirements.** Save, undo, receipt, invoice, work sheet and the existing search strip keep working exactly as today. The result-sheet envelope button keeps working. The identification card and follow-up actions may depend on STAGE-14's screen and STAGE-08's document writer, and must degrade to a disabled action (not a crash) if those have not landed.

**7.11 Completion gate.** The band carries all baseline actions; STAGE-02 tests pass; no schema change was made; no STAGE-01 behaviour regressed.

### E.3 STAGE-03 — Test selection: filter bar, groups, counts, sample-type control

**7.1 Objective and scope.** Make the registration screen's test-selection block behave as the baseline describes: a filter bar with a filter field and three mode options, a working test-group list, a live count of the patient's tests, and a sample-type control that applies to the registered tests. Covers `1.2.2.5.1`, `1.2.2.5.1.1`, `1.2.2.5.1.2`, `1.2.2.5.1.3`, `1.2.2.5.1.4`, `1.2.2.5.6.1`, `1.2.2.5.7`, `1.2.2.5.10`, `1.2.2.5.10.1`–`1.2.2.5.10.5`. **Out of scope:** the account panel (STAGE-04), the result entry grid (STAGE-06).

**7.2 Prerequisites and dependencies.** STAGE-01 (the medical-history condition catalogue).

**7.3 Verified current state.** The block exists (`PatientEditorView.xaml:387-471`) and the master test list works. Missing: the filter bar (`1.2.2.5.1`, class D) — `GetRegistrationCatalogQueryHandler.cs:30` calls `SearchTestCatalogQuery(null, null)`, so the list is always unfiltered; the filter field and the three mode options (class D); the patient's-test count (class D — `PatientEditorView.xaml:414` shows a static title); the test-group list is loaded into the view model (`PatientEditorViewModel.cs:498-502, 557-568`) but not bound to a visible list (class B). The "complete test" and "test group" menu actions exist as *pickers* (`PatientEditorView.xaml:392-397`, `406-411`) rather than as menus (class B each). The sample-type control is five check boxes per test row (`PatientEditorView.xaml:435-439`); the baseline calls for a row of radio-style options per test, and the baseline rule permits **more than one** sample type to be selected, so the current multi-select behaviour is correct and only the presentation differs (class B).

**7.4 Exact work items.**

*W3.1 — Filter bar.* Add the filter bar above the master test list with a filter field and three mode options. Extend `GetRegistrationCatalogQuery` with a filter term and a mode, and pass it to `SearchTestCatalogQuery` (which already accepts a term). The three modes are the baseline's TM / TG / CG codes, whose meaning is not established (VERIFY #1): default = **T** filter by test name/code, **G** filter by test group, **C** filter by custom group. Baseline `1.2.2.5.1`, `1.2.2.5.1.1`, `1.2.2.5.1.2`.

*W3.2 — Test-group list.* Bind the already-loaded `TestGroups` collection to a visible list from which a whole group can be added, so the baseline's "add every test of the selected group" is reachable. Baseline `1.2.2.5.7` (class B).

*W3.3 — Patient's-test count.* Show the live count of the patient's registered tests in the panel title, replacing the static text. Baseline `1.2.2.5.6.1` (class D).

*W3.4 — Menus.* Convert the complete-test and test-group *pickers* into the baseline's *menus* on the master test list, keeping the picker available. Baseline `1.2.2.5.1.3`, `1.2.2.5.1.4` (class B each).

*W3.5 — Sample-type control.* Present the five sample-type options as one row per test, keeping multi-select (the baseline rule allows more than one). Keep `UpdatePatientTestSampleFlagsCommand` as the write path. Baseline `1.2.2.5.10` and its five options.

*W3.6 — Sample type and the identity-block summary.* A newly added test row starts with no sample type selected. The identity-block sample-type field (STAGE-01 W1.2) is a **read-only summary** of the sample types set on the visit's registered tests, so it follows the per-test control rather than driving it; there is no patient-level default to apply (VERIFY #3).

**7.5 Traceability.** `1.2.2.5` (B→COMPLETE), `1.2.2.5.1` (D→IMPLEMENT), `1.2.2.5.1.1` (D→IMPLEMENT), `1.2.2.5.1.2` (D→IMPLEMENT + VERIFY), `1.2.2.5.1.3` (B→CORRECT), `1.2.2.5.1.4` (B→CORRECT), `1.2.2.5.2`/`.3`/`.4`/`.5`/`.6`/`.8`/`.11` (A→PRESERVE), `1.2.2.5.6.1` (D→IMPLEMENT), `1.2.2.5.7` (B→COMPLETE), `1.2.2.5.10` (B→CORRECT), `1.2.2.5.10.1`–`.10.5` (A→PRESERVE), `1.2.2.5.9` (D→**EXCLUDED**, Exclusion 5).

**7.6 Database migration decision.** **NO MIGRATION REQUIRED.** The filter is a query parameter over the existing catalogue; the group list, the count and the sample-type flags already exist. `1.2.2.5.9` is excluded, so no mail path is added.

**7.7 Migration details.** Not applicable — no schema change. The mode codes are a query parameter, not a column.

**7.8 Acceptance criteria.**
- C1. Typing in the filter field narrows the master test list by the active mode.
- C2. Each of the three mode options is selectable, and its effect is the documented default.
- C3. The test-group list is visible and a whole group can be added from it.
- C4. The panel title shows the live number of the patient's registered tests.
- C5. The complete-test and test-group menus are reachable from the master test list.
- C6. A test row's sample-type options are one row, and more than one may be selected.
- C7. A newly added test starts with no sample type selected, and the identity-block sample-type field reflects the sample types set on the visit's registered tests.

**7.9 Testing requirements.**
- *Application tests:* `GetRegistrationCatalogQueryHandler` honours the filter term and mode; the count reflects the patient's registered tests.
- *ViewModel tests:* mode changes re-query; adding a group adds every test of the group exactly once.
- *UI tests:* the filter bar, mode options, group list and count are present and bound.

**7.10 Preservation and compatibility requirements.** Adding a test, adding a profile, adding a custom group, removing a test, clearing all tests, the taken-outside flag and the sample-type flags keep their current behaviour (`1.2.2.5.3`, `.4`, `.5`, `.8`, `.11` are class A). No e-mail path is introduced.

**7.11 Completion gate.** The filter, the group list, the count, the menus and the sample-type control all behave as above; STAGE-03 tests pass; no schema change was made; no e-mail path exists anywhere in the solution.

### E.4 STAGE-04 — Patient account panel and account screen

**7.1 Objective and scope.** Make the account panel and the account screen carry every baseline figure, action and column, and make the money arithmetic match the baseline's rules. Covers `1.3.1.3`, `1.3.1.4.1`, `1.3.1.4.2`, `1.3.1.5`, `1.3.1.6`, `1.3.1.7`, `1.3.1.8`, `1.3.1.9`, `1.3.1.10`, `1.3.1.11`, `1.3.1.13`, `1.3.1.14`, `1.3.2.1.4`, `1.3.2.1.5`, `1.3.2.2`, `1.3.2.3`, `1.3.2.5`, `1.3.2.7`, `1.3.2.8`. **Out of scope:** the delivery-time settlement (STAGE-10), the registration panel itself (STAGE-02).

**7.2 Prerequisites and dependencies.** STAGE-01 (the patient record and the print-result password are not prerequisites; the account panel is independent) — in fact **STAGE-04 has no hard prerequisite** and may run in parallel with STAGE-02 and STAGE-03.

**7.3 Verified current state.**
- The panel (`PatientEditorView.xaml:19-53`) shows total, discount, paid and balance, and offers "record payment" and "full settlement". It has **no** discount-percentage field, **no** paid-previously figure, **no** total-collected figure, **no** remainder-for-lab figure, and **no** confirmation dialog.
- `PatientEditorViewModel.cs:1292` reads the paid field with a plain decimal parse, so a `+50` surcharge is silently treated as 50. The extra-charge path exists but is unreachable from this field (`RecordExtraChargeCommandHandler.cs:43-51`).
- `RecordPaymentCommandHandler.cs:42-52` stores a discount as an absolute amount only; the panel's second field is a plain amount, not a percentage.
- `SettleAccountInFullCommandHandler` inserts a `FullSettlement` row for the balance and **refuses** a balance of zero or less; there is no confirmation dialog (`PatientEditorViewModel.cs:1331-1358`).
- `1.3.1.4.2` (the two-`Enter` rule) has no keyboard handling at all: no `KeyDown`, `PreviewKeyDown` or `InputBindings` exists under `src/TopLab.Presentation`.
- The account screen (`PatientAccountView.xaml:100-136`) lists transactions with paid date, money and user name. The edit-date column is absent from the DTO (`PatientBillingDtos.cs:22-31`) although `AuditableEntity.LastModifiedAtUtc` exists; the colour-code column is absent entirely; the "modify amount" action creates a **correction row** instead of editing the selected amount (`PatientAccountViewModel.cs:415-432`); the "cancel operation" action voids the row and leaves it visible (`PatientAccountViewModel.cs:459-495`); there is **no** refund action (`OperationType` has only Payment, Correction, FullSettlement); and there is **no** clear-account action.
- `1.3.1.13` (custody) and `1.3.1.14` (account back) have no implementation. The panel's back action is the form-wide undo (`PatientEditorViewModel.cs:1037-1048`).

**7.4 Exact work items.**

*W4.1 — Discount percentage.* Add a percentage input next to the absolute discount. The baseline keeps both: the absolute value (`1.3.1.2`, class A) and the percentage (`1.3.1.3`, class C). Compute the percentage from the charged total for display and convert a percentage entry into the absolute amount on save, so the stored `DiscountAmount` remains the single source of truth. Baseline `1.3.1.3`.

*W4.2 — Surcharge convention.* Implement the `+` rule: a paid-field value prefixed with `+` is an addition to the total of the tests, not a payment. Route it to the existing extra-charge command. Baseline `1.3.1.4.1` (class C).

*W4.3 — The two-`Enter` rule.* Add keyboard handling on the paid field: the first `Enter` computes the account operation, the second saves it; a second `Enter` after a save starts a new compute. The two presses are **not** interchangeable. Baseline `1.3.1.4.2` (class D).

*W4.4 — Paid previously, total collected, amount after discount.* Add the three missing figures as read-only derivations on the panel: the amount already paid on the account before this operation, the total collected on the account, and the total after the discount. Baseline `1.3.1.5` (B), `1.3.1.6` (D), `1.3.1.7` (B).

*W4.5 — Remainder for the laboratory.* Add the laboratory-side remainder beside the customer-side remainder, computed from the same balance with the opposite sign convention the delivery screen already uses (`GetDeliveryAccountQueryHandler.cs:39-40`). Baseline `1.3.1.9` (class C).

*W4.6 — Confirm-payment dialog.* The baseline's confirm action confirms the entered payment in a dialog. Add the dialog and wire it to the existing `RecordPaymentCommand`; the "record payment" button becomes the confirm action. Baseline `1.3.1.10` (class B).

*W4.7 — Net settlement with confirmation.* The baseline's net settlement closes the account when the patient has paid in full and is confirmed in a dialog. Change the handler so a zero balance is a valid settlement (today it is rejected), and add the confirmation dialog. Baseline `1.3.1.11` (class C).

*W4.8 — Custody action.* Add the custody action opening a read-only list of the visit's open (unsettled) operations with the receiving user and date. The baseline's exact behaviour is not established (VERIFY #6). Baseline `1.3.1.13` (class D).

*W4.9 — Account back action.* Add a dedicated back action that leaves the account operation and returns to the registration context, distinct from the form-wide undo. Baseline `1.3.1.14` (class C).

*W4.10 — Edit-date and colour-code columns.* Project `LastModifiedAtUtc` into `PaymentOperationDto` and add the column; add the colour-code column, deriving the colour from the operation type and void state (VERIFY #7). Baseline `1.3.2.1.4` (D), `1.3.2.1.5` (D).

*W4.11 — Modify amount.* Make the modify action **edit the selected transaction's amount** in place, keeping the existing correction behaviour available as a separate path so nothing that depends on it breaks. Baseline `1.3.2.2` (class C).

*W4.12 — Void/archive transaction (never hard-delete).* Make the delete action remove the transaction from the account **view** and record the removal in the case audit trail (actor, timestamp, reason). The ledger row itself is **voided or archived, never hard-deleted**: `PaymentOperation` already carries `IsVoided` and is an auditable entity stamped by `AuditableEntitySaveChangesInterceptor`, so the financial record is preserved for auditability. The baseline's "deletes a transaction from the account" is therefore implemented as removal from the working ledger plus an audit-trail entry, never as destruction of the row. Baseline `1.3.2.3` (class C). This is consistent with the standing rule in Section F.4 ("Never hard-delete a `PaymentOperation`").

*W4.13 — Refund action.* Add a refund action. A refund is a payment returned to the patient, so it must reduce the collected total. `PaymentOperation` currently rejects a negative amount and offers no refund type, so the model must express one. Baseline `1.3.2.5` (class D).

*W4.14 — Clear account.* Add the clear-account action, which removes the patient's account operations, leaving the registered tests and their prices intact. Baseline `1.3.2.7` (class D).

*W4.15 — Account patient list.* The baseline's action returns to the **patient list**; today it returns to the registration editor. Correct the destination. Baseline `1.3.2.8` (class B).

**7.5 Traceability.** `1.3.1` (A→PRESERVE), `1.3.1.1`/`.2`/`.4`/`.12` (A→PRESERVE), `1.3.1.3` (C→CORRECT), `1.3.1.4.1` (C→CORRECT), `1.3.1.4.2` (D→IMPLEMENT), `1.3.1.5` (B→COMPLETE), `1.3.1.6` (D→IMPLEMENT), `1.3.1.7` (B→COMPLETE), `1.3.1.8` (B→COMPLETE), `1.3.1.9` (C→CORRECT), `1.3.1.10` (B→COMPLETE), `1.3.1.11` (C→CORRECT), `1.3.1.13` (D→IMPLEMENT + VERIFY), `1.3.1.14` (C→CORRECT), `1.3.2` (A→PRESERVE), `1.3.2.1`/`.1.1`/`.1.2`/`.1.3`/`.4`/`.6` (A→PRESERVE), `1.3.2.1.4` (D→IMPLEMENT), `1.3.2.1.5` (D→IMPLEMENT + VERIFY), `1.3.2.2` (C→CORRECT), `1.3.2.3` (C→CORRECT), `1.3.2.5` (D→IMPLEMENT), `1.3.2.7` (D→IMPLEMENT), `1.3.2.8` (B→CORRECT).

**7.6 Database migration decision.** **NEW MIGRATION REQUIRED.** Every figure and column added in this stage is either a read-only derivation over existing rows, a command-level behaviour, or a projection of `LastModifiedAtUtc` — **except** the refund, which needs the domain to express a payment returned to the patient. `PaymentOperation` currently has no refund representation: `Amount` is constrained `>= 0`, `OperationType` holds only `Payment`, `Correction`, `FullSettlement`, and there is no refund column. A refund cannot be expressed as a negative `Amount` (that would break the `>= 0` rule and silently alter every existing balance), and it cannot be piggybacked on `Correction` or `IsExtraCharge` without overloading their meaning. The refund therefore requires a new schema element. Verified against the code: no `RefundAmount` column or `Refund` operation type exists anywhere in the solution, and no applied migration introduces one, so this is a genuine new migration, not a change that is already applied.

**7.7 Migration details.**
- *Proposed name:* `AddPaymentOperationRefundAmount`.
- *`PaymentOperation` table:* add `RefundAmount decimal(18,2) NULL`, with a check that it is `> 0` when present. `Amount` keeps its `>= 0` rule and its `decimal(18,2)` type.
- *Domain:* add `OperationType.Refund` as an appended enum member (the column is an `int` conversion of the enum stored in a `tinyint`, so the new member persists as `3` without a schema change), and let `PaymentOperation.Create` accept a refund, which sets `RefundAmount` and leaves `Amount` at zero.
- *Calculator:* `PatientAccountCalculator.TotalPaid` subtracts `RefundAmount` from the collected total; `Balance` follows. A refund therefore increases the balance owed by the laboratory, which is the correct direction.
- *Data preservation:* no existing row is modified; the new column is null on every existing row and every existing operation keeps its current `Amount`, `DiscountAmount`, `OperationType` and void state. Row counts are unchanged. `TotalPaid` for every patient without a refund is **unchanged** — the new term contributes zero — so every stored balance and every printed receipt keeps its current value.
- *Rollback:* drop the column; no operation is lost. Because the column is additive and null on all pre-existing rows, rollback restores the prior schema exactly and no stored financial value is affected.
- *Test obligation:* a persistence test must assert that a patient with a refund has a balance equal to the pre-refund balance plus the refund, and that a patient without refunds has exactly the balance produced before this stage.

**7.7 Migration details.**
- *Proposed name:* `AddPaymentOperationRefundAmount`.
- *`PaymentOperation` table:* add `RefundAmount decimal(18,2) NULL`, with a check that it is `> 0` when present. `Amount` keeps its `>= 0` rule and its `decimal(18,2)` type.
- *Domain:* add `OperationType.Refund` (the column is an `int` conversion of the enum, so the new member persists as `3` without a schema change), and let `PaymentOperation.Create` accept a refund, which sets `RefundAmount` and leaves `Amount` at zero.
- *Calculator:* `PatientAccountCalculator.TotalPaid` subtracts `RefundAmount` from the collected total; `Balance` follows. A refund therefore increases the balance owed by the laboratory, which is the correct direction.
- *Data preservation:* no existing row is modified; the new column is null on every existing row and every existing operation keeps its current `Amount`, `DiscountAmount`, `OperationType` and void state. Row counts are unchanged. `TotalPaid` for every patient without a refund is **unchanged** — the new term contributes zero — so every stored balance and every printed receipt keeps its current value.
- *Rollback:* drop the column; no operation is lost.
- *Test obligation:* a persistence test must assert that a patient with a refund has a balance equal to the pre-refund balance plus the refund, and that a patient without refunds has exactly the balance produced before this stage.

**7.8 Acceptance criteria.**
- D1. The panel shows the discount value **and** the discount percentage, and either may be entered.
- D2. A paid-field value of `+50` adds 50 to the total of the tests and does not reduce the balance.
- D3. The first `Enter` computes; the second saves; the two are not interchangeable.
- D4. The panel shows paid-previously, total-collected and amount-after-discount, and each matches a recomputation from the ledger.
- D5. The panel shows the remainder for the customer and the remainder for the laboratory, and they are consistent with the balance.
- D6. Confirming a payment shows a dialog; cancelling records nothing.
- D7. Net settlement at a zero balance is accepted and asks for confirmation.
- D8. The custody action opens the list of open operations.
- D9. The account's back action returns to the patient list, not to the form.
- D10. The transaction grid shows the edit date and the colour code.
- D11. Modify edits the selected amount; delete voids/archives the transaction (removes it from the account view, records actor/timestamp/reason in the case audit trail, never destroys the ledger row); refund records a refund that raises the balance owed; clear empties the account while leaving the registered tests intact.

**7.9 Testing requirements.**
- *Domain tests:* `PaymentOperation.Create` accepts a refund and rejects an invalid one; `PatientAccountCalculator` subtracts refunds; a pre-refund balance is unchanged for a refund-free account.
- *Application tests:* `RecordPaymentCommandHandler` treats a `+`-prefixed amount as an extra charge; `SettleAccountInFullCommandHandler` accepts a zero balance.
- *ViewModel tests:* the first `Enter` computes and the second saves; the percentage entry converts to an absolute discount; the settlement dialog cancels without a write.
- *Persistence tests:* the migration adds a nullable column, leaves existing rows null, and rolls back.
- *UI tests:* the three dialogs appear and cancel correctly.

**7.10 Preservation and compatibility requirements.** The account formula for refund-free patients is numerically identical to today's. The printed receipt and invoice keep their current totals and layout. `1.3.1.1` (total of tests), `1.3.1.2` (discount value), `1.3.1.4` (paid amount) keep their current behaviour. The correction operation type keeps working, and the extra-charge operation type keeps its meaning.

**7.11 Completion gate.** All eleven criteria hold; the migration applies and rolls back on an ephemeral container; the balance of every refund-free patient is bit-identical to its pre-stage value; the decision record for VERIFY #6 and #7 is written.

### E.5 STAGE-05 — Laboratory identifier, patient search, visit history, modification paths

**7.1 Objective and scope.** Make the laboratory identifier behave as a **patient-level** identifier that survives across visits, give the search screen the baseline's criteria, scope and action bar, and complete the two modification paths. Covers `1.11.1.3`, `1.11.2.2`, `1.10.1.1.1`, `1.10.1.1.2`, `1.10.1.1.5`, `1.10.1.1.6`, `1.10.1.1.7`, `1.10.1.1.11`–`1.10.1.1.15`, `1.10.1.2.2`, `1.10.1.3.1`, `1.10.1.3.2`, `1.10.1.3.3`, `1.10.1.3.4`, `1.10.1.4`, `1.10.1.4.1`–`1.10.1.4.10`, `1.5.1`, `1.5.1.2`, `1.5.1.3`, `1.5.2`, `1.5.2.2`, `1.5.3`. **Out of scope:** the branch criterion and branch selector (excluded); the print-result password (STAGE-01); the ribbon's own search icon (STAGE-15).

**7.2 Prerequisites and dependencies.** STAGE-01 (the card number is a search criterion), STAGE-02 (the registration screen's navigation actions point at the patient list).

**7.3 Verified current state.**
- The identifier is generated as *max + 1* on every registration (`GetNextLabIdQueryHandler.cs:20-33`) and `CreatePatientCommandHandler.cs:48-58` **rejects** any identifier already held by a row, so a returning patient can never be shown their previous identifier, and every visit is a separate patient as far as the identifier is concerned.
- `GetPatientByLabIdQueryHandler.cs:36-55` already resolves **all** visits sharing an identifier and orders them by date, so the visit-history model is in place; `VisitRollup.BuildHistory` composes the summary.
- The search screen (`PatientSearchView.xaml:25-66`) has six criteria: treating doctor, request date, age, gender, plus an identifier field and a name field. Missing: exact-name search (`SearchPatientsGlobalQueryHandler.cs:41` uses `Contains` only), a dedicated phone field (`:34-38, :44` searches inside the name), card-number search, the four result-state criteria, the bounded/free date-range modes, the scope block, the result-count indicator, the full action bar, and the result grid's "referred by" column.
- The name-search assist is disabled by default: `SystemSettingsConfiguration.cs:27` seeds `EnablePatientNameSearchAssist = false`, and `SearchPatientsGlobalQueryHandler.cs:25` reads the flag, so the search screen's name criterion does nothing until an administrator turns it on.
- "Test within period" filters the **visit** date (`:69-74`) instead of the analysis date.
- The result grid shows 7 of the baseline's 8 columns; the count indicator shows a page count rather than a total (`:167-171, :594`).
- "Enter ID" navigates away (`:608-644`) instead of filling the result grid; "New search" clears the filters but not the search term or the identifier.
- The action bar has 7 buttons, none of them the baseline's 10.
- The embedded patient list in the registration screen has no date filter (`SearchPatientsQuery` has none), so the baseline's registration-day selector cannot be honoured.
- The result-screen patient data action (`1.5.3`) does not exist.

**7.4 Exact work items.**

*W5.1 — Patient-level identifier.* Change the identifier from per-visit to **per-patient**. Introduce a patient key: the identifier stays on `Patient`, but the reuse rule changes so that a patient's later visits carry the identifier of their first visit. Implement by matching the returning patient on the identity fields the baseline already carries (national id, name, phone) at registration, and by relaxing `CreatePatientCommandHandler`'s rejection so a re-registration under the same patient reuses the identifier instead of failing. Baseline `1.11.1.3` (class D). This is the change that makes the identifier "shown automatically at the next registration of the same patient".

*W5.2 — Identifier display.* Show the stored identifier in the registration header for a returning patient, and the generated one for a first visit. (The control itself is STAGE-02's `1.2.2.1.5`; the *value* it shows is this stage's work.)

*W5.3 — Exact-name criterion.* Add an exact-match criterion beside the partial one, so the baseline's two name criteria are distinct. Baseline `1.10.1.1.1` (class D).

*W5.4 — Partial-name criterion available by default.* The baseline lists both name criteria as available criteria of the search screen. The assist flag currently gates the name criterion off by default. Change the search screen so both name criteria are always offered, and keep the assist flag for its own purpose (the registration editor's name drop-down). Baseline `1.10.1.1.2` (class C).

*W5.5 — Test within period.* Re-point the "test within period" criterion at the **analysis date** of the patient's tests rather than the visit date. Baseline `1.10.1.1.5` (class C).

*W5.6 — Dedicated phone field.* Add a phone-number criterion with its own field, distinct from the name criterion. Baseline `1.10.1.1.6` (class C).

*W5.7 — Card-number criterion.* Add a criterion backed by the STAGE-01 card-number column. Baseline `1.10.1.1.7` (class D).

*W5.8 — Result-state criteria.* Add the four result-state criteria — results not entered, not verified, not printed, not delivered — to the search screen. These queries exist for the worklist (`GetResultWorklistQuery`) and are reused here. Baseline `1.10.1.1.11`–`1.10.1.1.14` (class B each).

*W5.9 — Bounded / free date range.* Add the two mutually exclusive date-range modes to every dated criterion. Baseline `1.10.1.1.15` (class D).

*W5.10 — Scope block.* Add the scope block with the source selector; the branch half is excluded (Exclusion 4). Baseline `1.10.1.2` (class D), `1.10.1.2.2` (class D); `1.10.1.2.1` → **EXCLUDED**.

*W5.11 — Result grid columns.* Add the "referred by" column so the grid carries all eight baseline columns. Baseline `1.10.1.3.1` (class C).

*W5.12 — Result count.* Show the **total** number of patients the search returned, not the page size. Baseline `1.10.1.3.2` (class D).

*W5.13 — New search.* Clear the search term and the identifier as well as the filters. Baseline `1.10.1.3.3` (class B).

*W5.14 — Enter identifier.* Typing an identifier starts the search and fills the result grid instead of navigating away. Baseline `1.10.1.3.4` (class C).

*W5.15 — Action bar.* Add the baseline's ten actions: patient tests, patient data, laboratory results, delete patient, open account, restrict to VIP, add to selection, collected results, modify group, main list. `1.10.1.4.1`–`1.10.1.4.5` and `1.10.1.4.10` are class B (present, but on the wrong screen — relocate them). `1.10.1.4.6` (restrict to VIP), `1.10.1.4.7` (add to selection), `1.10.1.4.8` (collected results) and `1.10.1.4.9` (modify group) are class D. `1.10.1.4.7` and `1.10.1.4.8` connect to STAGE-13's multi-patient history; `1.10.1.4.9` changes the account type. VERIFY #16/#17/#18 recorded.

*W5.16 — Visit-history grid columns.* Add the baseline's eight columns (referred by, unit, age, gender, date, patient name, laboratory identifier, identifier) to the visit list, which today shows only the date. Baseline `1.11.2.2` (class C).

*W5.17 — Registration-day selector.* Add the date filter to the patient list embedded in the registration screen, and make the registration-day choice a precondition of modification. Baseline `1.5.1.3` (class D), and the rule attached to it.

*W5.18 — Modification path one.* Complete path one end to end: patient icon → add-and-edit screen → patient list → registration day → select patient → modify → change → save. The modify action is STAGE-02's; this stage supplies the list with its day filter and the save that persists the change. Baseline `1.5.1` (class B), `1.5.1.2` (class B).

*W5.19 — Modification path two.* Path two must reach an **editable** patient record, not a read-only visit history. Change the search screen's patient-data action to open the registration screen in edit mode for the selected patient. Baseline `1.5.2` (class B), `1.5.2.2` (class B).

*W5.20 — Patient data from the result screen.* Add the patient-data action to the result-entry screen, reaching the patient's data. Baseline `1.5.3` (class D).

**7.5 Traceability.** `1.11.1`/`.1.1`/`.1.2` (A→PRESERVE), `1.11.1.3` (D→IMPLEMENT), `1.11.2`/`.2.1`/`.2.3` (A→PRESERVE), `1.11.2.2` (C→CORRECT), `1.10.1`/`.1.1`/`.1.3`/`.1.4`/`.1.8`/`.1.9`/`.1.3` (A→PRESERVE), `1.10.1.1.1` (D→IMPLEMENT), `1.10.1.1.2` (C→CORRECT), `1.10.1.1.5` (C→CORRECT), `1.10.1.1.6` (C→CORRECT), `1.10.1.1.7` (D→IMPLEMENT), `1.10.1.1.11`–`1.10.1.1.14` (B→COMPLETE), `1.10.1.1.15` (D→IMPLEMENT), `1.10.1.2` (D→IMPLEMENT), `1.10.1.2.1` (D→**EXCLUDED**, Exclusion 4), `1.10.1.2.2` (D→IMPLEMENT + VERIFY), `1.10.1.3.1` (C→CORRECT), `1.10.1.3.2` (D→IMPLEMENT), `1.10.1.3.3` (B→CORRECT), `1.10.1.3.4` (C→CORRECT), `1.10.1.4` (D→IMPLEMENT), `1.10.1.4.1`–`.4.5` (B→COMPLETE — relocate from the visit-history screen), `1.10.1.4.6` (D→IMPLEMENT + VERIFY), `1.10.1.4.7` (D→IMPLEMENT), `1.10.1.4.8` (D→IMPLEMENT + VERIFY), `1.10.1.4.9` (D→IMPLEMENT + VERIFY), `1.10.1.4.10` (B→CORRECT), `1.5.1` (B→COMPLETE), `1.5.1.1`/`.4`/`.5`/`.6` (A→PRESERVE), `1.5.1.2` (B→CORRECT), `1.5.1.3` (D→IMPLEMENT), `1.5.2` (B→CORRECT), `1.5.2.1` (A→PRESERVE), `1.5.2.2` (B→CORRECT), `1.5.3` (D→IMPLEMENT), `1.1.5`/`.1`/`.2` (D→IMPLEMENT — the patient-module quick-search toolbar).

**7.6 Database migration decision.** **NO MIGRATION REQUIRED.** Every change is a query parameter, a screen composition, a navigation target, or a reuse rule. The identifier reuse is enforced in the application layer against the existing `Patient.LabId` column, which is already a plain (non-unique) index — so no unique constraint has to be created or dropped.

**7.7 Migration details.** Not applicable — no schema change. The identifier reuse rule must **not** be implemented by adding a unique index on `LabId`: the baseline requires one identifier to cover several visits of the same patient, so a unique index would be wrong as well as destructive. The `PatientConfiguration` index on `LabId` stays non-unique.

**7.8 Acceptance criteria.**
- E1. Registering a patient, then registering them again, shows the **same** laboratory identifier on the second registration.
- E2. Searching by that identifier lists every visit of that patient with its date.
- E3. The search screen offers both an exact-name and a partial-name criterion, both usable without an administrator setting.
- E4. "Test within period" selects patients whose **analysis** falls in the period.
- E5. A dedicated phone criterion finds a patient by phone number.
- E6. A card-number criterion finds a patient by card number.
- E7. The four result-state criteria each return the expected patients.
- E8. Each dated criterion offers a bounded and a free mode, and the two are mutually exclusive.
- E9. The source selector switches between the live data and the configured backup source.
- E10. The result grid shows all eight baseline columns, and the count indicator shows the total number of patients returned.
- E11. "New search" clears the term and the identifier as well.
- E12. "Enter identifier" fills the result grid.
- E13. All ten action-bar actions are present and reach their targets.
- E14. The visit list shows all eight baseline columns.
- E15. Path one reaches an editable record after a registration day is chosen; path two reaches an editable record from the search screen.
- E16. The result-entry screen carries a patient-data action.

**7.9 Testing requirements.**
- *Application tests:* the reuse rule reuses an identifier for the same patient and rejects a conflicting one; the exact-name criterion matches exactly; the analysis-date criterion filters on analysis date; each result-state criterion returns the right set; the bounded and free modes are mutually exclusive.
- *ViewModel tests:* the count indicator shows the total; the action bar exposes exactly ten actions; "Enter identifier" populates the grid.
- *Persistence tests:* the identifier query returns every visit of a patient.
- *UI tests:* the scope block, the mode options and the eight grid columns render.

**7.10 Preservation and compatibility requirements.** The existing search criteria, the age and gender criteria, the request-date criterion and the treating-doctor criterion keep working. `1.11.1.1` (identifier creation at save) and `1.11.1.2` (identifier stored as a distinguished patient data item) keep their behaviour, and `1.10.1.1.3`/`.4`/`.8`/`.9` and `1.10.1.3` stay as they are. The `EnablePatientNameSearchAssist` setting keeps its meaning for the registration editor's name drop-down. The branch artefacts stay inert — the branch criterion and branch selector are not implemented and not surfaced.

**7.11 Completion gate.** All sixteen criteria hold; STAGE-05 tests pass; no schema change was made; no branch criterion or branch selector exists anywhere in the solution; the decision records for VERIFY #15, #16, #17 and #18 are written.

### E.6 STAGE-06 — Result entry: patient panel, result grid, entry action block

**7.1 Objective and scope.** Turn the result-entry screen into the baseline's screen: a patient list panel of the day's patients with a count, a result grid carrying the baseline's columns, editable per-test flags, and an action block that closes the screen. Covers `1.6.1.1`, `1.6.1.1.1`, `1.6.1.1.2`, `1.6.1.1.3`, `1.6.1.2`, `1.6.1.2.2`, `1.6.1.2.3`, `1.6.1.2.5`, `1.6.1.2.6`, `1.6.1.2.7`, `1.6.1.2.8`, `1.6.1.2.9`, `1.6.1.2.10`, `1.6.1.2.11`, `1.6.1.2.12`, `1.6.1.2.13`, `1.6.1.2.14`, `1.6.1.3`, `1.6.1.3.1`, `1.6.1.3.2`, `1.6.1.3.3`. **Out of scope:** the report-entry grid and its variants (STAGE-07), the report settings (STAGE-08), the delivery screen (STAGE-10).

**7.2 Prerequisites and dependencies.** STAGE-01 (the patient record and the print-result password gate the print action), STAGE-03 (the registered tests whose results are entered), STAGE-05 (the result screen's patient-data action and the patient-panel data).

**7.3 Verified current state.**
- `GetResultWorklistQueryHandler` returns **one row per test** (`:113-150`), so the screen has no patient panel: the baseline requires the day's *patients* listed, with the patient's tests appearing when the name is clicked. The audit confirms this: the grid's rows are tests (`ResultsWorklistView.xaml:73-111`).
- The grid's columns are 8 of the baseline's 15: test abbreviation, result, and read-only flags for verify and print, plus a column that shows a **row count** where the baseline wants the abnormal flag (`ResultsWorklistView.xaml:104`).
- `ResultFlag` has `Normal`, `Low`, `High` — **no `Critical`** — and `ResultFlagComputer` derives only Low/High from the matched band (`ResultFlagComputer.cs:37-90`). The baseline requires three states: Low, High and Critical.
- `PatientTest` has **no** `Unit` and **no** finished state, so the baseline's unit column, low-and-high column, normal-range column and finish flag cannot be shown. `ProfileResultItem` has a unit, but a simple test has none.
- The verify and print flags are **read-only** in the grid: `ReviewResultCommand` and `MarkResultPrintedCommand` exist in the application layer but are never invoked from the presentation layer.
- The export flag is per-patient, not per-test (`ExportPatientReportPdfCommandHandler.cs:175-181`).
- The per-test comment is stored (`PatientTest.Notes`, max 500) but appears neither in the grid nor in the ordinary printed report (`ReportContentBuilder.cs:373-387`).
- The entry action block is spread over three screens; the worklist screen has no save action, and the preview and print actions exist only for the profile and culture screens.
- The `1.5.3` patient-data action does not exist on this screen.

**7.4 Exact work items.**

*W6.1 — Patient list panel.* Add the panel above the result grid listing the patients registered that day, with the baseline's six columns (code, name, type, laboratory identifier, referring entity, age) and a count of the patients in the list. Clicking a patient's name opens that patient's tests. The current per-test worklist becomes the patient's test list shown after the click. Baseline `1.6.1.1`, `1.6.1.1.1`, `1.6.1.1.2` (class C), `1.6.1.1.3` (class C).

*W6.2 — Result grid columns.* Extend the grid to the baseline's 15 columns: abbreviation, P, T, result, status, abnormal flag, low-and-high band, unit, normal range, finish, verify, print, export, per-test comment, plus the double-click action. Baseline `1.6.1.2` (class B) and the individual columns.

*W6.3 — P and T columns.* Add the two narrow columns with the documented defaults (VERIFY #11: P = the test is part of a profile; VERIFY #12: T = the test is part of a test group). Both are derived from the catalogue, so no schema change is needed. Baseline `1.6.1.2.2`, `1.6.1.2.3` (class D each).

*W6.4 — Status column.* Show the test's status: not entered, entered, verified, printed, delivered — derived from the flags that already exist. Baseline `1.6.1.2.5` (class D).

*W6.5 — Abnormal flag with a Critical state.* Extend `ResultFlag` with `Critical` and compute it. The baseline's rule is that a result is abnormal when it falls outside the reference range, with the states Low, High and Critical. The existing band carries only Min/Max, so the critical threshold must be expressed. Baseline `1.6.1.2.6` (class B) and VERIFY #13.

*W6.6 — Low-and-high band and normal range columns.* Show the band the result was judged against and the test's normal range, both read from the frozen `PatientTestReferenceRangeSnapshot` the entry path already writes (`EnterResultCommandHandler.cs:98`, `ResultReferenceRangeSource.cs:55`). The relation between the flag column and the band column is fixed so they always agree (VERIFY #13). Baseline `1.6.1.2.7`, `1.6.1.2.9` (class D each).

*W6.7 — Unit column.* Show the unit of the result. Baseline `1.6.1.2.8` (class D).

*W6.8 — Finish flag.* Add the per-test finished check box and make the finished state a stored, audited fact. Baseline `1.6.1.2.10` (class D).

*W6.9 — Editable verify, print and export flags.* Make the three flags editable and wire them to the existing `ReviewResultCommand`, `MarkResultPrintedCommand` and export command, so the flags act rather than merely display. Baseline `1.6.1.2.11`, `1.6.1.2.12`, `1.6.1.2.13` (class B each).

*W6.10 — Per-test comment in the grid and in the report.* Show the stored per-test comment in the grid and render it in the printed report (the report item template already has a `TestComment` key, `ReportItemKeys.cs:12-35`). Baseline `1.6.1.2.14` (class B).

*W6.11 — Entry action block.* Give the result-entry screen its own closing block with save, preview and print, matching the baseline; keep the per-variant screens' actions. Baseline `1.6.1.3` (class B), `1.6.1.3.1` (class B), `1.6.1.3.2` (class B), `1.6.1.3.3` (class B).

*W6.12 — Patient-data action.* Add the patient-data action to the result screen (the command itself is STAGE-05's W5.20). Baseline `1.5.3` (class D, cross-referenced).

**7.5 Traceability.** `1.6.1` (A→PRESERVE), `1.6.1.1` (C→CORRECT), `1.6.1.1.1` (C→CORRECT), `1.6.1.1.2` (D→IMPLEMENT), `1.6.1.1.3` (C→CORRECT), `1.6.1.2` (B→COMPLETE), `1.6.1.2.1`/`.4`/`.15` (A→PRESERVE), `1.6.1.2.2`/`.3` (D→IMPLEMENT + VERIFY), `1.6.1.2.5` (D→IMPLEMENT), `1.6.1.2.6` (B→CORRECT + VERIFY), `1.6.1.2.7`/`.8`/`.9`/`.10` (D→IMPLEMENT), `1.6.1.2.11`/`.12`/`.13` (B→COMPLETE), `1.6.1.2.14` (B→COMPLETE), `1.6.1.3`/`.3.1`/`.3.2`/`.3.3` (B→COMPLETE), `1.5.3` (D→IMPLEMENT, cross-referenced to STAGE-05).

**7.6 Database migration decision.** **NEW MIGRATION REQUIRED.**

**7.7 Migration details.**
- *Proposed name:* `AddResultEntryColumnsAndCriticalFlag`.
- *`PatientTest` table:* add `Unit nvarchar(30) NULL`, `IsFinished bit NOT NULL DEFAULT 0`, `FinishedAtUtc datetime2 NULL`, `FinishedByUserId int NULL`, `ResultComparison tinyint NULL` (the comparison the flag was computed from, so the printed flag can be re-derived), `LowValue decimal(18,2) NULL`, `HighValue decimal(18,2) NULL` (the band the flag was judged against, so the grid can show the low-and-high column and the report can print the status flag without recomputing). Index `PatientId, IsFinished`.
- *`AnalyteReferenceRangeBand` table:* add `CriticalMinValue decimal(18,2) NULL`, `CriticalMaxValue decimal(18,2) NULL`.
- *`PatientTestReferenceRangeSnapshot` table:* add the same two critical columns, `Unit nvarchar(30) NULL` and `LowComment`/`HighComment` are already present, so the frozen band can carry the critical band and the unit at entry time.
- *`ResultFlag` enum:* add `Critical = 3`. The column is an `int` conversion of the enum stored in a `tinyint`, so the new state persists without a schema change and **existing rows keep their meaning** (`0` Normal, `1` Low, `2` High are unchanged; the new state is `3`).
- *Data preservation:* every added column is nullable or has a default that reproduces today's behaviour (`IsFinished` defaults to false, which matches today's "not finished" reading; the flag values of existing rows are untouched). No existing row is rewritten; no column is dropped or retyped.
- *Rollback:* drop the added columns; the `Critical` enum member is removed with the code change. Existing data survives.
- *Test obligation:* the migration must apply to an empty database and over `20261018144852_AddProfileResultStyling`; a test must assert that a result with a pre-existing flag still reports the same flag after the migration, and that the band columns are null on pre-existing rows.
- *Dependency note:* the snapshot columns are written by the entry path from STAGE-07 onwards; entries made before this stage simply show an empty band and unit, which is the correct rendering of "no band captured".

**7.8 Acceptance criteria.**
- F1. The result screen lists the day's patients with the baseline's six columns and a count; clicking a name shows that patient's tests.
- F2. The result grid carries all fifteen baseline columns.
- F3. The P and T columns show the documented defaults.
- F4. The status column distinguishes not entered, entered, verified, printed and delivered.
- F5. A numeric result outside its band is flagged Low or High; a result beyond the critical band is flagged Critical; the flag and the band column always agree.
- F6. The low-and-high column and the normal-range column show the band the result was judged against.
- F7. The unit column shows the result's unit.
- F8. The finish check box is editable and its state survives a reload.
- F9. The verify, print and export check boxes are editable and their action takes effect immediately.
- F10. A per-test comment appears in the grid and in the printed report.
- F11. The entry action block closes the screen with save, preview and print.
- F12. The print action is refused without the STAGE-01 print-result password when it is enabled.

**7.9 Testing requirements.**
- *Domain tests:* `ResultFlagComputer` returns `Critical` for a value beyond the critical band and `Low`/`High` otherwise; a result inside the band is `Normal`.
- *Application tests:* the worklist query returns patients grouped with their tests and the correct counts; the review, print and export commands are dispatched from the grid; the patient-data command is reachable.
- *ViewModel tests:* clicking a patient name loads that patient's tests; editing a flag issues its command; the action block's commands are enabled appropriately.
- *Persistence tests:* the migration applies and rolls back; existing flags and rows are unchanged; the new columns default safely.
- *UI tests:* the panel, the count and the fifteen grid columns render; the print action is refused without the password.

**7.10 Preservation and compatibility requirements.** The result screen keeps its existing per-test worklist behaviour as the patient's test list. The variant screens' own actions keep working. `1.6.1.2.1` (abbreviation), `1.6.1.2.4` (result) and `1.6.1.2.15` (double-click) keep their behaviour. The flag values of every existing result keep their meaning. `PatientTest.Notes` keeps its current 500-character limit and existing content.

**7.11 Completion gate.** All twelve criteria hold; the migration applies and rolls back on an ephemeral container; pre-existing results report identical flags after the migration; the decision records for VERIFY #11, #12 and #13 are written.

### E.7 STAGE-07 — Report entry grid, entry variants, report screen actions

**7.1 Objective and scope.** Give every kind of test the baseline's report-entry grid, make the grid's flag and range columns correct, and support the six entry shapes the baseline names. Covers `1.6.1.5`, `1.6.1.5.4`, `1.6.1.5.5`, `1.6.1.5.6`, `1.6.1.5.7`, `1.6.1.5.8`, `1.6.1.5.9`, `1.6.1.5.9.1`, `1.6.1.5.9.2`, `1.6.1.5.9.3`, `1.6.1.6`, `1.6.1.6.1`, `1.6.1.6.2`, `1.6.1.6.3`, `1.6.1.6.4`, `1.6.1.6.5`, `1.6.1.6.6`, `1.6.1.7.2`, `1.6.1.7.3`, `1.6.1.7.4`, `1.6.1.7.5`, `1.6.1.7.6`. **Out of scope:** the report settings dialog (STAGE-08), the history dialog (STAGE-13).

**7.2 Prerequisites and dependencies.** STAGE-06 (the result-entry screen, the frozen band and the unit the grid shows).

**7.3 Verified current state.**
- The report-entry grid exists only for the profile variant (`ProfileEntryView.xaml:60-94`); a simple test gets a single field (`SimpleResultEntryView.xaml:30-33`). The baseline requires the grid for every test.
- The grid's columns are test name, result, unit, a broken flag combo, a read-only verify check box and a read-only print check box. The baseline requires normal ranges, per-row comment, per-row verify and per-row print.
- The flag combo is bound by **index** to a `ProfileResultFlag` with only `Low = 0` and `High = 1` (`ProfileResultFlag.cs:3-7`): the empty item stores `Low`, "مرتفع" falls outside the enum's range, and there is no `Normal` state. The audit confirms the broken semantics (`ProfileEntryView.xaml:83-90`).
- The grid's normal-range column is missing although the query already fetches the range (`GetProfileEntryGridQueryHandler.cs:62-64, 78-92`).
- Per-row verify and per-row print are impossible: `ProfileResultItem` has `IsVerified` and `IsPrinted` per row, but no per-row command exists.
- The per-row comment does not exist: `ProfileResultItem` has no comment member, and `SaveProfileResultsCommandHandler.cs:94` writes a single screen-level comment.
- The urine sub-row panel does not exist anywhere.
- Of the baseline's six entry shapes, only three exist: `ResultKind` drives `ResultsWorklistViewModel.cs:207-231` into simple, profile and culture. Paired control/patient, derived value, multi-unit and qualitative are absent.
- `ProfileResultItem` holds one value, one unit and one flag (`ProfileResultItem.cs:22-25`), so a second unit, a control value and a qualitative marker have nowhere to live.
- The report screen's actions are save, print and preview only (`ProfileEntryView.xaml:131-140`); history, back and main-list are absent, and the preview does not exist for a simple test.

**7.4 Exact work items.**

*W7.1 — Report-entry grid for every test.* Render the baseline's grid (test name, result, unit, low-or-high flag, normal ranges, verified, print, comment) on the simple-test screen as well as the profile screen. Baseline `1.6.1.5` (class B) and `1.6.1.5.1`–`1.6.1.5.3` (class A, preserved).

*W7.2 — Flag column semantics.* Fix the column: bind by value, not by index; add `Normal` to the flag set; store the empty selection as `Normal` rather than `Low`; and let the flag be **computed** from the analyte band for the patient's sex and age, with manual override permitted only before print (VERIFY #14). Baseline `1.6.1.5.4` (class C).

*W7.3 — Normal-ranges column.* Show the normal range the patient's own range where the test defines one, using the frozen snapshot. Baseline `1.6.1.5.5` (class C).

*W7.4 — Per-row verified flag.* Add a per-row verify check box bound to `ProfileResultItem.IsVerified`, with a command that verifies one row. Baseline `1.6.1.5.6` (class B).

*W7.5 — Per-row print flag.* Add a per-row print check box bound to `ProfileResultItem.IsPrinted`. Baseline `1.6.1.5.7` (class B).

*W7.6 — Per-row comment.* Give each row its own comment and carry it into the report. Baseline `1.6.1.5.8` (class B).

*W7.7 — Urine sample sub-row panel.* Add the panel: one row per sample, each with a sub-result, and a sub-result with no corresponding sample marked as having no sample. Baseline `1.6.1.5.9` and its three children (class D each).

*W7.8 — Paired control and patient entry.* Support a sub-test entered as a control value and a patient value, each with its own unit. Baseline `1.6.1.6.3` (class D).

*W7.9 — Derived value.* Support a value derived from other values of the same report — an international normalised ratio from the control time and the patient's prothrombin time, or a ratio between two enzyme results. The derivation is **computed**, never stored, and is shown read-only. Baseline `1.6.1.6.4` (class D).

*W7.10 — Multi-unit entry.* Support one sub-test carrying more than one unit — a bleeding time in minutes and in seconds. Baseline `1.6.1.6.5` (class D).

*W7.11 — Qualitative result.* Support a qualitative result — a value that exceeds a limit, or a result recorded as absent — for both simple and profile entries. Baseline `1.6.1.6.6` (class B).

*W7.12 — Report screen actions.* Add the baseline's report-screen actions: print, preview (including for a simple test), the patient-history action (wired to STAGE-13's dialog), back, and main list. The back action needs a navigation stack that does not exist today (`INavigationService.cs:5-19` has no back stack) → add one. Baseline `1.6.1.7.2`, `1.6.1.7.3` (class B), `1.6.1.7.4`, `1.6.1.7.5`, `1.6.1.7.6` (class D each).

**7.5 Traceability.** `1.6.1.5` (B→COMPLETE), `1.6.1.5.1`/`.2`/`.3` (A→PRESERVE), `1.6.1.5.4` (C→CORRECT + VERIFY), `1.6.1.5.5` (C→CORRECT), `1.6.1.5.6`/`.7`/`.8` (B→COMPLETE), `1.6.1.5.9`/`.9.1`/`.9.2`/`.9.3` (D→IMPLEMENT), `1.6.1.6` (B→COMPLETE), `1.6.1.6.1`/`.2`/`.6` (B→COMPLETE), `1.6.1.6.3`/`.4`/`.5` (D→IMPLEMENT), `1.6.1.7` (B→COMPLETE), `1.6.1.7.1` (A→PRESERVE), `1.6.1.7.2`/`.3` (B→COMPLETE), `1.6.1.7.4`/`.5`/`.6` (D→IMPLEMENT).

**7.6 Database migration decision.** **NEW MIGRATION REQUIRED.**

**7.7 Migration details.**
- *Proposed name:* `AddProfileResultEntryColumns`.
- *`ProfileResultItem` table:* add `Comment nvarchar(500) NULL`, `ControlValue nvarchar(100) NULL`, `ControlUnit nvarchar(30) NULL`, `SecondaryUnit nvarchar(30) NULL`, `SecondaryValue nvarchar(100) NULL`, `IsDerived bit NOT NULL DEFAULT 0`, `DerivedValue nvarchar(100) NULL`, `Comparison tinyint NULL` (the qualitative comparison: none, greater-than, less-than, absent, present), `NormalRangeText nvarchar(200) NULL`.
- *New table `PatientTestSampleResult` (the urine sub-row panel):* `PatientTestSampleResultId int IDENTITY` PK, `PatientTestId int NOT NULL` FK to `PatientTests` (cascade delete), `SampleLabel nvarchar(50) NOT NULL`, `SubResultKey nvarchar(50) NULL`, `SubResultValue nvarchar(100) NULL`, `IsMissingSample bit NOT NULL DEFAULT 0`, `SortOrder int NOT NULL DEFAULT 0`. Index `(PatientTestId, SortOrder)`.
- *`ProfileResultFlag` enum:* add `Normal = 2` **without renumbering** the stored `Low = 0` and `High = 1`; the new state persists as `2` in the existing `tinyint` column, so existing rows keep their meaning. A row whose stored value is out of the new range cannot occur, because the column previously only ever held `0` or `1`.
- *Data preservation:* every added column is nullable or defaults to today's behaviour; no existing row is rewritten; the existing `ProfileResultItems` rows keep their values, flags and printed state.
- *Rollback:* drop the added columns and the new table; existing rows survive.
- *Test obligation:* the migration applies to an empty database and over the previous head; a test asserts that every pre-existing profile result keeps its flag value and that the new table is empty.

**7.8 Acceptance criteria.**
- G1. Every test kind shows the baseline's report-entry grid with all eight of its columns.
- G2. An empty flag selection stores `Normal`; `Low` and `High` store their own values; a value outside the band is flagged automatically before print.
- G3. The normal-ranges column shows the range, with the patient's own range where the test defines one.
- G4. A row's verify check box verifies that row alone.
- G5. A row's print check box marks that row alone.
- G6. Each row carries its own comment, and the comment appears in the printed report.
- G7. A urine test shows one sub-row per sample, and a sub-result with no sample is marked as having no sample.
- G8. A paired sub-test carries a control value and a patient value, each with its own unit.
- G9. A derived value is computed from the report's own values and is read-only.
- G10. A sub-test can carry two units.
- G11. A qualitative result can be recorded as exceeding a limit or as absent.
- G12. The report screen offers print, preview, history, back and main list; preview works for a simple test; back returns to the previous screen.

**7.9 Testing requirements.**
- *Domain tests:* `ProfileResultItem` accepts and rejects the new value shapes (control value, second unit, qualitative comparison); a derived value is not persisted.
- *Application tests:* the profile grid query returns the range, the comment and the per-row flags; the per-row verify and print commands act on one row; the urine sub-rows are saved and loaded in order.
- *ViewModel tests:* an empty flag selection stores `Normal`; the grid computes the flag from the band.
- *Persistence tests:* the migration applies and rolls back; pre-existing profile results keep their values.
- *UI tests:* the grid renders all eight columns on both screens; the urine panel renders one row per sample.

**7.10 Preservation and compatibility requirements.** The profile screen's existing columns and the save command keep working. `1.6.1.5.1`/`.2`/`.3` (test name, result, unit) are class A and keep their rendering. `1.6.1.7.1` (save) keeps working. Existing profile results keep their flag values, their print counts and their printed state. The correction path (`ProfileResultAmendment`) keeps working unchanged.

**7.11 Completion gate.** All twelve criteria hold; the migration applies and rolls back on an ephemeral container; every pre-existing profile result keeps its stored flag value; the decision record for VERIFY #14 is written.

### E.8 STAGE-08 — Report settings screen and rendering controls

**7.1 Objective and scope.** Give the report its settings dialog: a single reachable screen carrying the baseline's title block, display options, typography and output block, and make every report artefact obey the saved settings — including the local export formats. Covers `1.6.1.4`, `1.6.1.4.1`, `1.6.1.4.1.1`–`1.6.1.4.1.10`, `1.6.1.4.2`, `1.6.1.4.2.2`, `1.6.1.4.2.3`, `1.6.1.4.3`, `1.6.1.4.3.1`–`1.6.1.4.3.5`, `1.6.1.4.4`, `1.6.1.4.4.1`, `1.6.1.4.4.3`, `1.6.1.4.4.4`, `1.13.6.2`, `1.13.6.2.1`–`1.13.6.2.4`. **Out of scope:** the report-item template screen (an existing, separate administration screen that stays).

**7.2 Prerequisites and dependencies.** STAGE-06 (the status flag and band the settings print), STAGE-07 (the report-entry grid the settings shape).

**7.3 Verified current state.**
- The settings live on **three** screens: `ReportSettingsView.xaml` (margins, paper, colours, history options, print options, marker), `ReportItemTemplatesView.xaml` (per-item labels, symbols, spacing, offsets) and `SystemSettingsView.xaml:31-33` (the printer). The baseline wants **one** dialog with four blocks. The audit confirms the split (`1.6.1.4`, class B).
- The dialog is **unreachable from the report screens** — nothing in `Views/Patients` navigates to it.
- The title block does not exist: `ReportItemKeys.cs:12-35` has no screen-title, main-title, sub-title or sub-sub-title key, and `ReportContentBuilder.cs:153, 252, 488` writes fixed text. The baseline requires four title fields.
- The reference-range, report-comment and normal-range texts exist only as *per-item* overrides (`ReportItemTemplate.ReferenceValueExpression`, `FreeTextOverride`) and a single global `NormalRangeMarker` (`ReportSettings.cs:48`), not as the baseline's four report-level fields.
- The symbol fields exist only as one global marker per item (`ReportItemTemplate.MarkerSymbol`); the baseline wants a **main** symbol, a **sub** symbol and a **sub-title** symbol, chosen independently.
- The typography block is missing: one font family for all elements (`Printing/ReportDocument.cs:20-34`), no symbol-font selectors, no text-alignment control, no cell-expansion control, and the normal-result font size is fixed at 12 pt (`:24, 33`).
- Vertical spacing exists per item only (`ReportItemTemplate.LineSpacing`); the baseline wants space after the sub title, space between texts, and extra space to the left of the range.
- The print-status-flag option does not exist and there is no `Critical` state to print (STAGE-06 adds it).
- **The print-patient-history option applies globally today**, so a change made in the settings affects every report; the baseline requires the change to apply to **that report only** (`1.6.1.4.2.3`, class B).
- The export action is on the result-sheet screen and **ignores the saved settings** (`PatientReportPdfExporter.cs:42-71` builds from `ReportSettings.CreateDefault()`), and it offers PDF only. The baseline's export action belongs in the output block and offers the format set.
- The close action navigates to the settings dashboard instead of closing, and there is no cancel (`ReportSettingsViewModel.cs:69`).
- `UpdateReportSettingsCommandHandler.cs:30` writes the two print options from the request, but `ReportSettingsViewModel.cs:195-211` sends `false, false` for both — so the baseline's print-sub-title and reprint options are permanently off (this is the audit's finding for `1.7.1.5.1` and `1.7.1.5.2`).

**7.4 Exact work items.**

*W8.1 — One settings dialog.* Build a single dialog reachable from the report screens (and from the settings dashboard) with the baseline's four blocks: title block, display options, typography, output block. The per-item template screen and the system printer selection remain where they are, but the dialog's output block carries the baseline's printing-target selector. Baseline `1.6.1.4` (class B).

*W8.2 — Title block.* Add the four title fields — report screen title, report main title, report sub title, report sub-sub-title — and render them in the report, replacing the fixed text at `ReportContentBuilder.cs:153, 252, 488`. Baseline `1.6.1.4.1` (class C), `1.6.1.4.1.1`–`1.6.1.4.1.4` (class D ×2, C ×2).

*W8.3 — Reference-range, report-comment and normal-range texts.* Add the three report-level text fields and use them in the report. Baseline `1.6.1.4.1.5`/`.1.6`/`.1.7` (class B each).

*W8.4 — Three symbol fields.* Add the main, sub and sub-title symbol fields, independent of the per-item marker. Baseline `1.6.1.4.1.8`/`.1.9`/`.1.10` (class C each).

*W8.5 — Print-status-flag option.* Add the option that prints the low, high and critical status flag, using STAGE-06's `Critical` state. Baseline `1.6.1.4.2.2` (class D).

*W8.6 — Per-report scope of the history option.* Make the print-patient-history option apply to **the report being set**, not to every report. Implement as a per-report override resolved at print time, with the stored setting as the default. Baseline `1.6.1.4.2.3` (class B).

*W8.7 — Typography block.* Add the symbol-font selectors (one for the main elements, one for the sub elements), the vertical spacing fields (space after the sub title, space between texts, extra space to the left of the range), the normal-result font selector, the text-alignment control and the cell-expansion control; and make the selected normal-result font **and size** apply to the report. Baseline `1.6.1.4.3` (class C), `1.6.1.4.3.1`/`.3.2`/`.3.4`/`.3.5` (class D ×2, C ×2), `1.6.1.4.3.3` (class B). The offset-amount control (`1.6.1.4.3.6`) is class A and stays as the per-item offset that already exists.

*W8.8 — Output block: printing target.* Add the printing-target selector to the output block, reusing `PrinterAssignment` and the existing printer dispatch. Baseline `1.6.1.4.4.1` (class B).

*W8.9 — Output block: export action.* Move the export action into the output block and make it obey the saved settings (today it builds from defaults). Add the baseline's format set: print, Word, PDF, image. Baseline `1.6.1.4.4.3` (class C), `1.13.6.2` (class C), `1.13.6.2.1` (class B), `1.13.6.2.2` (class D), `1.13.6.2.3` (class A), `1.13.6.2.4` (class D). The image format uses the renderer already present in the pinned QuestPDF package; the Word format is produced by a local writer — **no new package and no network access**.

*W8.10 — Close action.* Add a close action that closes the dialog, and a cancel that discards unsaved changes. Baseline `1.6.1.4.4.4` (class B).

*W8.11 — Fix the print options.* Stop `ReportSettingsViewModel` from sending `false, false` for the two print options, so the baseline's print-sub-title and reprint options are actually settable. This is the shared root cause of `1.7.1.5.1` and `1.7.1.5.2` in STAGE-09.

**7.5 Traceability.** `1.6.1.4` (B→COMPLETE), `1.6.1.4.1` (C→CORRECT), `1.6.1.4.1.1`/`.1.2` (D→IMPLEMENT), `1.6.1.4.1.3` (C→CORRECT), `1.6.1.4.1.4` (D→IMPLEMENT), `1.6.1.4.1.5`/`.1.6`/`.1.7` (B→COMPLETE), `1.6.1.4.1.8`/`.1.9`/`.1.10` (C→CORRECT), `1.6.1.4.2` (B→COMPLETE), `1.6.1.4.2.1` (A→PRESERVE), `1.6.1.4.2.2` (D→IMPLEMENT), `1.6.1.4.2.3` (B→CORRECT), `1.6.1.4.3` (C→CORRECT), `1.6.1.4.3.1` (D→IMPLEMENT), `1.6.1.4.3.2` (C→CORRECT), `1.6.1.4.3.3` (B→COMPLETE), `1.6.1.4.3.4` (D→IMPLEMENT), `1.6.1.4.3.5` (D→IMPLEMENT), `1.6.1.4.3.6` (A→PRESERVE), `1.6.1.4.4` (C→CORRECT), `1.6.1.4.4.1` (B→COMPLETE), `1.6.1.4.4.2` (A→PRESERVE), `1.6.1.4.4.3` (C→CORRECT), `1.6.1.4.4.4` (B→CORRECT), `1.13.6.2` (C→CORRECT), `1.13.6.2.1` (B→COMPLETE), `1.13.6.2.2` (D→IMPLEMENT), `1.13.6.2.3` (A→PRESERVE), `1.13.6.2.4` (D→IMPLEMENT).

**7.6 Database migration decision.** **NEW MIGRATION REQUIRED.**

**7.7 Migration details.**
- *Proposed name:* `AddReportSettingsTitlesTypographyAndSymbols`.
- *`ReportSettings` table* (single row, id 1): add `ReportScreenTitle nvarchar(200) NULL`, `ReportMainTitle nvarchar(200) NULL`, `ReportSubTitle nvarchar(200) NULL`, `ReportSubSubTitle nvarchar(200) NULL`, `ReferenceRangeText nvarchar(200) NULL`, `ReportCommentText nvarchar(500) NULL`, `NormalRangeText nvarchar(200) NULL`, `MainSymbol nvarchar(10) NULL`, `SubSymbol nvarchar(10) NULL`, `SubTitleSymbol nvarchar(10) NULL`, `SymbolFontFamilyMain nvarchar(100) NULL`, `SymbolFontFamilySub nvarchar(100) NULL`, `NormalResultFontFamily nvarchar(100) NULL`, `NormalResultFontSizePt decimal(4,1) NULL`, `TextAlignment tinyint NULL` (0 = default/right, 1 = left), `CellExpansionEnabled bit NOT NULL DEFAULT 0`, `CellExpansionMaxWidthCm decimal(4,1) NULL`, `SpaceAfterSubTitleCm decimal(4,2) NULL`, `SpaceBetweenTextsCm decimal(4,2) NULL`, `SpaceLeftOfRangeCm decimal(4,2) NULL`, `PrintStatusFlagEnabled bit NOT NULL DEFAULT 0`, `PrintHistoryPerReportOverride bit NOT NULL DEFAULT 0`.
- *No change to `ReportItemTemplate`*: the per-item template screen keeps its own label, symbol, spacing and offset columns, and the new report-level fields sit beside them.
- *Data preservation:* every added column is nullable or defaults to today's rendering (`CellExpansionEnabled = 0` reproduces today's fixed cells; `PrintStatusFlagEnabled = 0` reproduces today's report, which prints no status flag; `PrintHistoryPerReportOverride = 0` reproduces today's global behaviour). The single existing settings row keeps every current value; no row is inserted, deleted or rewritten.
- *Rollback:* drop the added columns; the single row survives with its original values.
- *Test obligation:* the migration applies to an empty database and over the previous head; a test asserts that the existing settings row is unchanged apart from the added null/default columns, and that a report rendered with default settings is byte-comparable in layout to the pre-stage rendering (no status flag, no expanded cells).
- *Dependency note:* `PrintHistoryPerReportOverride` gives the per-report scope; the value chosen for a report is carried in the report request, so no per-report settings table is needed.

**7.8 Acceptance criteria.**
- H1. One settings dialog is reachable from the report screens and carries the title, display, typography and output blocks.
- H2. The four title fields appear in the rendered report, replacing the fixed text.
- H3. The reference-range, report-comment and normal-range texts appear in the report.
- H4. The main, sub and sub-title symbols are set independently and render independently.
- H5. The status-flag option prints the Low, High and Critical flags.
- H6. Turning the history option off for one report leaves other reports unaffected.
- H7. The symbol fonts, the three spacing values, the normal-result font and its size, the alignment and the cell expansion all take effect in the rendered report.
- H8. The output block's printing-target selector chooses the printer.
- H9. The export action offers print, Word, PDF and image, and the exported document obeys the saved settings.
- H10. Closing the dialog closes it; cancelling discards unsaved changes.
- H11. The print-sub-title and reprint options can actually be turned on and stay on.

**7.9 Testing requirements.**
- *Domain tests:* `ReportSettings` accepts and validates the new values (symbol length, font size range, spacing non-negative, alignment in range).
- *Application tests:* the update command persists the new fields; the per-report history override is resolved at print time.
- *Printing tests:* a report rendered with a set title shows that title; a set symbol renders; a set font size changes the rendered text size; the status flag prints when enabled and is absent when disabled; the export honours the saved settings rather than the defaults.
- *Persistence tests:* the migration applies and rolls back; the existing row is unchanged apart from the added columns.
- *UI tests:* the dialog has the four blocks; the export offers the four formats; close and cancel behave differently.

**7.10 Preservation and compatibility requirements.** The per-item template screen keeps its own fields and its existing marker, spacing and offset behaviour, and `1.6.1.4.2.1` (abnormal background) and `1.6.1.4.3.6` (offset amount) are class A and keep working. The **default** rendering of every report is unchanged: no title override, no status flag, no expanded cells, the same fonts, the same spacing, the same alignment. A report produced from an unchanged database looks exactly as before. The printer assignment screen keeps working. No new package is introduced and no network access is required to export.

**7.11 Completion gate.** All eleven criteria hold; the migration applies and rolls back on an ephemeral container; a default report renders identically to the pre-stage rendering; no new package was added to `Directory.Packages.props`; no HTTP client exists in the solution.

### E.9 STAGE-09 — Combined report, empty report, patient data sheet

**7.1 Objective and scope.** Make the combined report compose the way the baseline describes, restore its two print options, and give the empty report a working patient selection and printable content. Covers `1.7.1.2`, `1.7.1.3`, `1.7.1.5`, `1.7.1.5.1`, `1.7.1.5.2`, `1.8.1.1`, `1.8.1.2`. **Out of scope:** the report settings (STAGE-08), the history insertion (STAGE-13).

**7.2 Prerequisites and dependencies.** STAGE-08 (the report settings that the combined report obeys, and the fix to the print options).

**7.3 Verified current state.**
- The combined report screen exists (`CombinedReportView.xaml:1-116`) and the printed artefact exists (`BuildCombinedReportCommandHandler.cs:23-255`). The **available-tests-in-report list** does not exist: the screen shows a preview built by a command instead of a list of the tests already included (`CombinedReportViewModel.cs:194-228`). The audit confirms this (`1.7.1.2`, class B).
- Tests are added by a check box and a build button (`CombinedReportView.xaml:77, 27`); the baseline requires a double press **or** a select-and-arrow action (`1.7.1.3`, class B).
- The reorder actions work (`CombinedReportViewModel.cs:374-402`); the audit notes the `MoveUp`/`MoveDown` helpers are dead code, and `1.7.1.4`/`.4.1`/`.4.2` are class A and are preserved.
- The two print options cannot be enabled because the settings view always sends `false, false` (see STAGE-08 W8.11), and the combined-report path does not check the reprint confirmation (the audit finds it only in the group-print path, `ExecuteBulkPrintCommandHandler.cs:59-68`). The baseline wants the reprint option on the combined report itself.
- The empty report is opened with `patientId = 0` (`PatientsHubViewModel.cs:69-77`), so it always shows the error path. The baseline requires the user to select a patient from the patient list.
- The empty report's content is read-only (`BlankReportView.xaml:66-71`) and the DTO has no editable fields (`ReportDtos.cs:96-104`); the baseline requires data to be entered into the empty report and printed. The patient-data-sheet option (`1.8.1.3`) already works and is class A.

**7.4 Exact work items.**

*W9.1 — Available-tests-in-report list.* Add the list of the tests already included in the combined report, shown beside the patient's own tests, so the user can see the composition before building. Baseline `1.7.1.2` (class B).

*W9.2 — Add-test action.* Support both baseline gestures: a double press on a test and a select-then-arrow action. Baseline `1.7.1.3` (class B).

*W9.3 — Option block.* Add the combined report's option block carrying the print-sub-title and print-printed-again options, wired to the settings STAGE-08 fixed, and honour the reprint option in the combined-report print path (no confirmation when it is on). Baseline `1.7.1.5` (class D), `1.7.1.5.1` (class B), `1.7.1.5.2` (class B).

*W9.4 — Empty report: patient selection.* Open the empty report for a **selected** patient: the action opens a patient picker, and the report then carries that patient's data. Baseline `1.8.1.1` (class B).

*W9.5 — Empty report: enter data and print.* Make the empty report's content editable — the patient data plus free-text entries — and print it with those entries. The entries are **transient** (they belong to the produced document, not to a stored record), so no schema change is needed. Baseline `1.8.1.2` (class D).

**7.5 Traceability.** `1.7.1` (A→PRESERVE), `1.7.1.1` (A→PRESERVE), `1.7.1.2` (B→COMPLETE), `1.7.1.3` (B→CORRECT), `1.7.1.4`/`.4.1`/`.4.2` (A→PRESERVE), `1.7.1.5` (D→IMPLEMENT), `1.7.1.5.1` (B→COMPLETE), `1.7.1.5.2` (B→COMPLETE), `1.8.1` (A→PRESERVE), `1.8.1.1` (B→CORRECT), `1.8.1.2` (D→IMPLEMENT), `1.8.1.3` (A→PRESERVE).

**7.6 Database migration decision.** **NO MIGRATION REQUIRED.** The report composition is session state, the option block reads and writes the settings STAGE-08 already persists, and the empty report's entered content is transient.

**7.7 Migration details.** Not applicable — no schema change. The empty report's entries must be carried in the print request, not persisted; if the implementation wants them stored, that is a design error to be corrected rather than a schema to be added.

**7.8 Acceptance criteria.**
- I1. The combined report screen shows the tests already included, and that list is accurate before and after building.
- I2. A test can be added by a double press and by the arrow action.
- I3. The option block can turn the sub-title print and the reprint-without-message option on, and both take effect.
- I4. Reprinting an already printed test through the combined report raises no confirmation when the option is on.
- I5. The empty report opens for a patient chosen in a picker, and shows that patient's data.
- I6. Free text entered in the empty report is printed with it.
- I7. The patient-data-sheet option still prints the patient's data alone.

**7.9 Testing requirements.**
- *Application tests:* the combined-report build honours the ordered selection and the two options; the empty-report command carries the selected patient and the entered content.
- *ViewModel tests:* the available-tests list mirrors the selection; both add gestures work; the option block reflects and writes the settings.
- *Printing tests:* the empty report prints the entered content; the combined report prints the group sub title when the option is on.
- *UI tests:* the option block, the arrow action and the patient picker render.

**7.10 Preservation and compatibility requirements.** The combined report's printed layout and the reorder behaviour are unchanged (`1.7.1.1`, `1.7.1.4` and its two children are class A). The patient-data sheet keeps printing the patient's data alone. The group-print path keeps its own reprint confirmation.

**7.11 Completion gate.** All seven criteria hold; no schema change was made; the combined report's default rendering is unchanged.

### E.10 STAGE-10 — Results delivery, delivery-time settlement, delivery outputs

**7.1 Objective and scope.** Rebuild the delivery screen to the baseline's shape — filters, patient scope, result grid, and the action block — and make the settlement and the delivery outputs behave as the baseline requires. Covers `1.4.1.1`, `1.4.1.1.2`, `1.4.1.1.3`, `1.4.1.1.4`, `1.4.1.2`, `1.4.1.2.1`, `1.4.1.2.2`, `1.4.1.2.3`, `1.4.1.2.3.1`, `1.4.1.3.1`, `1.4.1.3.1.2`, `1.4.1.3.1.3`, `1.4.1.3.1.5`, `1.4.1.3.1.6`, `1.4.1.3.1.9`, `1.4.1.3.1.10`, `1.4.1.4`, `1.4.1.4.1`, `1.4.1.4.3`, `1.4.1.4.4`, `1.4.1.4.5`, `1.4.1.4.6`, `1.4.1.4.7`, `1.4.1.4.8`, `1.4.1.4.9`, `1.4.1.4.10`, `1.4.1.4.11`, `1.4.1.4.13`. **Out of scope:** the Send-SMS action (excluded), the main patients window's own period list and statistics (STAGE-05).

**7.2 Prerequisites and dependencies.** STAGE-04 (the account panel and screen), STAGE-05 (the search screen's patient data, and the multi-patient history for the medical-history action), STAGE-08 (the export formats and the report settings), STAGE-13 (the medical-history report for the history action).

**7.3 Verified current state.**
- The delivery is two screens: `ResultDeliveryView.xaml` (a date filter and an undelivered list) and `DeliveryHandoverView.xaml` (account, per-test grid, settlement). The baseline wants **one** delivery screen with a filter block, a patient-scope block, a result block and an action block.
- Only a date filter exists (`1.4.1.1.1`, class A). The name filter, the status filter and the department filter are all absent (`1.4.1.1.2`/`.1.3`/`.1.4`, class D).
- The patient-scope block is absent entirely (`1.4.1.2`, class D): there is no source selector (Individual / LabToLab / Contracts — the `AccountType` enum already carries exactly these three plus Vip and Free), no group selector (VIP / Free / All), and no patients-today panel. The `AccountType` enum's existing values mean the two selectors need **no** schema change.
- The delivery patient grid shows the wrong columns (`ResultDeliveryView.xaml:49-71`): the baseline wants code, name, type, laboratory identifier, referring entity and age.
- The delivery result grid has four read-only boxes and one editable delivery box (`DeliveryHandoverView.xaml:68-101`); the baseline wants per-test check boxes for finish, verify, print and export, plus a status column, the P and T columns and a see-report action.
- The **status** value is already carried by the DTO (`ResultDeliveryDtos.cs:31`) but is not bound to a column.
- The export flag is per-patient, so a per-test export check box cannot work yet (STAGE-06 W6.9).
- There is no "finished" state to check (STAGE-06 W6.8).
- The account panel is displayed **permanently**, but the baseline's rule is that it appears only when the patient has settled part of the account and a balance remains with the laboratory. The audit confirms this (`1.4.1.4.1`, class B).
- The delivery-list print action prints **group patient slips** (`GroupPatientSlipPdfWriter`), not a delivery list. There is no delivery-list writer in `Infrastructure/Printing`.
- Preview is not bound (`PdfPreviewService` is not used by the delivery screens); copy, notes, patient data, combined report, medical history, empty report and envelope print have no command on the delivery screen.

**7.4 Exact work items.**

*W10.1 — One delivery screen.* Bring the filter, scope, result and action blocks onto one screen (the handover screen is the natural host), keeping the existing undelivered list as the scope block's content. Baseline `1.4.1` (class B).

*W10.2 — Filters.* Add the name filter, the status filter (delivered / reviewed / printed) and the department filter, alongside the existing date filter. The department's referent is not established (VERIFY #8): default = the patient's **referring entity**, the only organisational attribute the model carries. Baseline `1.4.1.1.2`/`.1.3`/`.1.4` (class D each).

*W10.3 — Patient scope block.* Add the source selector (Individual / LabToLab / Contracts — all three exist in `AccountType`), the group selector (VIP / Free / All) and the patients-today panel with its count. Baseline `1.4.1.2` (class D), `1.4.1.2.1`/`.2.2`/`.2.3` (class D each).

*W10.4 — Delivery patient grid.* Give the panel the baseline's six columns. Baseline `1.4.1.2.3.1` (class C).

*W10.5 — Delivery result grid.* Add the status column (binding the value the DTO already carries), the P and T columns with the same defaults as the result grid (VERIFY #9, #10), the per-test finish, verify, print and export check boxes, and the see-report action opening that test's report. Baseline `1.4.1.3.1` (class B), `1.4.1.3.1.2`/`.1.3` (class D), `1.4.1.3.1.5` (class D), `1.4.1.3.1.6` (class C), `1.4.1.3.1.9` (class D), `1.4.1.3.1.10` (class D).

*W10.6 — Account panel shown conditionally.* Show the account only when the patient has settled part of the account and a balance remains with the laboratory. Baseline `1.4.1.4.1` (class B) and the rule attached to it.

*W10.7 — Delivery list print.* Add a delivery-list writer that prints the delivery list the baseline describes, rather than the group slips. The existing group-slip writer is not modified. Baseline `1.4.1.4.3` (class C).

*W10.8 — Preview.* Bind the preview action to the delivery list. Baseline `1.4.1.4.4` (class D).

*W10.9 — Copy.* Add the copy action that copies the current delivery entry. Baseline `1.4.1.4.5` (class D).

*W10.10 — Notes.* Add the action opening the notes attached to the delivery entry (the patient's notes from STAGE-01). Baseline `1.4.1.4.6` (class D).

*W10.11 — Patient data, combined report, medical history, empty report, envelope print.* Add the five actions, each opening its target, reusing the screens built in STAGE-05, STAGE-09, STAGE-13, STAGE-08 and the existing envelope writer. Baseline `1.4.1.4.7`–`1.4.1.4.11` (class D each).

*W10.12 — Main list.* Keep the existing return to the main list. Baseline `1.4.1.4.13` (class A).

**7.5 Traceability.** `1.4.1` (B→COMPLETE), `1.4.1.1` (B→COMPLETE), `1.4.1.1.1` (A→PRESERVE), `1.4.1.1.2`/`.1.3`/`.1.4` (D→IMPLEMENT + VERIFY), `1.4.1.2` (D→IMPLEMENT), `1.4.1.2.1`/`.2.2`/`.2.3` (D→IMPLEMENT), `1.4.1.2.3.1` (C→CORRECT), `1.4.1.3` (A→PRESERVE), `1.4.1.3.1` (B→COMPLETE), `1.4.1.3.1.1`/`.1.4`/`.1.7`/`.1.8` (A→PRESERVE), `1.4.1.3.1.2`/`.1.3` (D→IMPLEMENT + VERIFY), `1.4.1.3.1.5` (D→IMPLEMENT), `1.4.1.3.1.6` (C→CORRECT), `1.4.1.3.1.9` (D→IMPLEMENT), `1.4.1.3.1.10` (D→IMPLEMENT), `1.4.1.4` (B→COMPLETE), `1.4.1.4.1` (B→CORRECT), `1.4.1.4.2` (A→PRESERVE), `1.4.1.4.3` (C→CORRECT), `1.4.1.4.4`–`1.4.1.4.11` (D→IMPLEMENT), `1.4.1.4.12` (D→**EXCLUDED**, Exclusion 5), `1.4.1.4.13` (A→PRESERVE).

**7.6 Database migration decision.** **NO MIGRATION REQUIRED.** The two scope selectors filter on the existing `AccountType` and `IsVip` columns; the grid columns are projections; the check boxes act on the columns STAGE-06 adds; the delivery list is a printed artefact.

**7.7 Migration details.** Not applicable — no schema change. The delivery-list writer is a new document writer in `Infrastructure/Printing`, not a schema change. The per-test export check box writes `PatientTest.IsExported` per row (STAGE-06 changes it from per-patient to per-test).

**7.8 Acceptance criteria.**
- J1. The delivery screen carries the filter, scope, result and action blocks.
- J2. The name, status and department filters each narrow the delivery list.
- J3. The source selector lists Individual, LabToLab and Contracts; the group selector lists VIP, Free and All; both change the list.
- J4. The patients-today panel lists that day's patients and shows their count.
- J5. The delivery patient grid shows the baseline's six columns.
- J6. The delivery result grid carries the status column, the P and T columns, four per-test check boxes and the see-report action.
- J7. The account appears only when a balance remains with the laboratory after a partial settlement, and is hidden otherwise.
- J8. The delivery-list print produces a delivery list, and the group-slip print is unchanged.
- J9. Preview shows the delivery list before printing.
- J10. Copy, notes, patient data, combined report, medical history, empty report and envelope print each work.
- J11. No SMS, e-mail or fax action exists on the delivery screen.

**7.9 Testing requirements.**
- *Application tests:* each filter narrows the delivery list correctly; the scope selectors filter by `AccountType` and `IsVip`; the delivery list query returns the patients-today set; the account is returned only when a balance remains.
- *ViewModel tests:* the account panel's visibility follows the balance rule; the check boxes dispatch per test; the see-report action carries the test id.
- *Printing tests:* the delivery-list writer produces a document containing the delivery list.
- *UI tests:* the four blocks render; the conditional account hides and shows; no messaging action exists.
- *Regression:* the group-slip print output is unchanged.

**7.10 Preservation and compatibility requirements.** The existing date filter, the undelivered list, the group-slip print and the handover screen's existing settlement actions keep working. `1.4.1.1.1`, `1.4.1.3`, `1.4.1.3.1.1`, `1.4.1.3.1.4`, `1.4.1.3.1.7`, `1.4.1.3.1.8`, `1.4.1.4.2` and `1.4.1.4.13` are class A and are preserved. The delivery settlement continues to use the STAGE-04 account logic and the same `DeliverWithSettlementCommand`.

**7.11 Completion gate.** All eleven criteria hold; no schema change was made; the group-slip output is unchanged; no messaging path exists; the decision records for VERIFY #8, #9 and #10 are written.

### E.11 STAGE-11 — Culture entry, sensitivity classification, microbiology report

**7.1 Objective and scope.** Complete the culture screen's microscopy block, enforce the baseline's sensitivity classification rule, and produce the baseline's microbiology report. Covers `1.9.1.2`, `1.9.1.2.1`, `1.9.1.2.7`, `1.9.1.2.9`, `1.9.1.2.10`, `1.9.1.3`, `1.9.1.3.2`, `1.9.1.4`, `1.9.1.4.2`, `1.9.1.4.3`, `1.9.1.4.4`, `1.9.1.5`, `1.9.1.5.1`, `1.9.1.6`. **Out of scope:** the culture data block and its six fields (class A, preserved), the antibiotic master data (existing).

**7.2 Prerequisites and dependencies.** STAGE-08 (the report settings that shape the microbiology report).

**7.3 Verified current state.**
- The culture data block works and is class A (`CultureEntryView.xaml:26-60`, `CultureResult.cs:10-20`).
- The microscopy block is missing its reaction, bacteria, Direct1/Direct2 and complete fields: `CultureMicroscopy` has `IsDirect` (one boolean) and three fixed "Others" columns, and no reaction or bacteria member (`CultureMicroscopy.cs:16-32`).
- The culture flag block offers two buttons where the baseline wants check boxes: a verified toggle and a print action (`CultureEntryView.xaml:207-214`).
- The sensitivity block lists the antibiotics attached at **test** level (`CultureAttachmentView.xaml:53-64`), not at culture level; the baseline wants the antibiotic added to the **culture**.
- The classification carries a fifth option, "unspecified" (`CultureEntryViewModel.cs:79-86`; `SaveCultureResultsCommandValidator.cs:9`), so "exactly one of the four classes" is not enforced; `CultureAntibioticResult.SensitivityCategory` is nullable (`CultureAntibioticResultConfiguration.cs` maps it `int?`, `IsRequired(false)`).
- The report-element visibility control is a single global show/hide per template item (`ReportItemTemplatesViewModel.cs:19, 37`), with no culture-specific control and no commercial-name control.
- The microbiology report does not exist as a document: the culture section is rendered inside the combined report (`ReportCultureSection.cs:47-124`) with no microscopy reference ranges, no Highly/Moderate/Low blocks, and no commercial-name column (`:109-112`).

**7.4 Exact work items.**

*W11.1 — Microscopy fields.* Add the reaction, bacteria, Direct1 and Direct2 fields, and the complete check box, to the microscopy block; the complete flag is a stored fact. Baseline `1.9.1.2.1` (class D), `1.9.1.2.7` (class D, **disabled** in the baseline's inspected state), `1.9.1.2.9` (class B), `1.9.1.2.10` (class D). The bacteria field is rendered **disabled** and cannot be entered, exactly as the baseline states.

*W11.2 — Culture flag block.* Convert the culture screen's verified and print actions into the baseline's per-test check boxes. Baseline `1.9.1.3` (class B), `1.9.1.3.2` (class B); `1.9.1.3.1` (verified) is class A and keeps its behaviour.

*W11.3 — Antibiotic add action.* Add the antibiotic to the **culture**, not to the test, and list the culture's antibiotics in the sensitivity block. Baseline `1.9.1.4` (class B), `1.9.1.4.2` (class B).

*W11.4 — Exactly one of four classes.* Remove the "unspecified" option and enforce that a saved sensitivity row carries exactly one of Highly For / Moderate For / Low For / Resistant For. The four option identifiers are class A and keep their meaning. Baseline `1.9.1.4.3` (class B).

*W11.5 — Report element visibility control.* Add the culture-specific control over the sensitivity block, the reference ranges and the commercial name, so the microbiology report can be composed for that culture. Baseline `1.9.1.4.4` (class B).

*W11.6 — Sensitivity entry action.* Add the sensitivity-entry action that opens the sensitivity entry for the culture. Baseline `1.9.1.5` (class B), `1.9.1.5.1` (class D).

*W11.7 — Microbiology report.* Produce the baseline's microbiology report as a document of its own: the culture and sensitivity table, the microscopic examination table with its reference ranges, the culture block, and the Highly For / Moderate For / Low For sensitivity blocks, each with a sensitive-inhibition-zone column and a commercial-name column. Baseline `1.9.1.6` (class B). The inhibition zone already exists (`CultureAntibioticResult.InhibitionZoneMm`); the commercial name already exists on `Antibiotic` (`Antibiotic.CommercialName`, added by `20261008135847_AddAntibioticCommercialName`). The **microscopy reference ranges** are new reference data and are seeded in STAGE-11's migration.

**7.5 Traceability.** `1.9.1` (A→PRESERVE), `1.9.1.1` and its six children (A→PRESERVE), `1.9.1.2` (B→COMPLETE), `1.9.1.2.2`–`.2.6`/`.2.8` (A→PRESERVE), `1.9.1.2.1` (D→IMPLEMENT), `1.9.1.2.7` (D→IMPLEMENT, disabled), `1.9.1.2.9` (B→COMPLETE), `1.9.1.2.10` (D→IMPLEMENT), `1.9.1.3` (B→COMPLETE), `1.9.1.3.1` (A→PRESERVE), `1.9.1.3.2` (B→COMPLETE), `1.9.1.4` (B→COMPLETE), `1.9.1.4.1` (A→PRESERVE), `1.9.1.4.2` (B→CORRECT), `1.9.1.4.3` (B→CORRECT), `1.9.1.4.3.1`–`.4` (A→PRESERVE), `1.9.1.4.4` (B→CORRECT), `1.9.1.5` (B→COMPLETE), `1.9.1.5.1` (D→IMPLEMENT), `1.9.1.5.2` (A→PRESERVE), `1.9.1.6` (B→COMPLETE).

**7.6 Database migration decision.** **NEW MIGRATION REQUIRED.**

**7.7 Migration details.**
- *Proposed name:* `AddCultureMicroscopyFieldsAndReferenceRanges`.
- *`CultureMicroscopies` table:* add `Reaction nvarchar(20) NULL`, `Bacteria nvarchar(20) NULL`, `Direct1 nvarchar(20) NULL`, `Direct2 nvarchar(20) NULL`, `IsComplete bit NOT NULL DEFAULT 0`. The existing `IsDirect` column is retained and keeps its meaning; the two Direct entries are additional.
- *`CultureAntibioticResults` table:* `SensitivityCategory` is currently nullable (`byte?`, `IsRequired(false)`), and the requirements do **not** establish how to classify pre-existing `NULL` rows. The default, evidence-based strategy is therefore to **keep the column nullable** and enforce "exactly one of Highly For / Moderate For / Low For / Resistant For" at save time in the domain and the validator (the "unspecified" fifth option is removed). Tightening the column to `tinyint NOT NULL` is an **owner decision, not a default**: it is applied only if a deterministic backfill is confirmed, using the same correction semantics as the prior repair migration `20260930163921_FixCultureSensitivityCategoryOffByOne`, and any row that still cannot be classified deterministically is **reported**, never silently assigned a fabricated clinical value. A fabricated classification on a real clinical result is prohibited.
- *New table `CultureMicroscopyReferenceRange`:* `CultureMicroscopyReferenceRangeId int IDENTITY` PK, `CultureTestId int NOT NULL` FK to the catalogue's culture tests, `FieldKey nvarchar(50) NOT NULL`, `NormalText nvarchar(200) NOT NULL`, `SortOrder int NOT NULL DEFAULT 0`. Seed the default ranges for the standard microscopy fields.
- *Data preservation:* every added column is nullable or defaults to today's behaviour (`IsComplete = 0` matches today's "not complete"); existing microscopy rows keep their current field values; existing culture results keep their sensitivity classifications unchanged. No pre-existing `NULL` is overwritten unless the owner separately approves the NOT NULL backfill.
- *Rollback:* drop the added columns and the new table; `SensitivityCategory` stays at whatever nullability was in force before this stage (nullable under the default strategy). Schema rollback restores the prior shape exactly; it does **not** restore any `NULL` value that an owner-approved backfill may have overwritten — exact restoration of such data requires a database backup taken before the migration is applied.
- *Test obligation:* the migration applies to an empty database and over the previous head; a test asserts that every pre-existing sensitivity classification is unchanged after the migration, and that unclassifiable rows are listed rather than assigned a value. If the owner approves the NOT NULL tightening, an additional test must assert the backfill is deterministic and that no row is silently assigned a fabricated class.

**7.8 Acceptance criteria.**
- K1. The microscopy block carries reaction, bacteria, Direct1, Direct2 and the complete check box.
- K2. The bacteria field is visible and cannot be entered.
- K3. The culture screen's verified and print states are check boxes that act on that culture.
- K4. An antibiotic added from the sensitivity block appears on that culture, not only on its test.
- K5. A saved sensitivity row carries exactly one of the four classes; the "unspecified" option is gone.
- K6. The visibility control decides, for that culture's report, whether the sensitivity block, the reference ranges and the commercial name appear.
- K7. The sensitivity-entry action opens the culture's sensitivity entry.
- K8. A microbiology report can be produced on its own, carrying the culture and sensitivity table, the microscopy table with its reference ranges, the culture block, and the three sensitivity blocks with their zone and commercial-name columns.

**7.9 Testing requirements.**
- *Domain tests:* `CultureMicroscopy` accepts the new fields; a saved sensitivity row with a null or a fifth class is rejected.
- *Application tests:* adding an antibiotic binds it to the culture; the culture grid query returns the culture's antibiotics; the microbiology report query returns the ranges and the commercial names.
- *Persistence tests:* the migration applies and rolls back; existing classifications are unchanged; unclassifiable rows are reported.
- *Printing tests:* the microbiology report contains each of the four required sections.
- *UI tests:* the disabled bacteria field rejects input; the check boxes act per culture.

**7.10 Preservation and compatibility requirements.** The culture data block and its six fields are class A and keep their behaviour. The four classification options are class A and keep their names and order. `1.9.1.3.1` (verified) keeps its current effect. The existing culture rendering inside the combined report keeps working; the microbiology report is **added**, not substituted. Existing sensitivity classifications are never rewritten.

**7.11 Completion gate.** All eight criteria hold; the migration applies and rolls back on an ephemeral container; no pre-existing classification changed; the decision record for the `SensitivityCategory` nullability is written.

### E.12 STAGE-12 — Specimen containers, barcode label set, test distribution

**7.1 Objective and scope.** Introduce the specimen-container concept the baseline needs, produce the four label kinds, and let the laboratory sort a patient's tests across containers and edit that distribution. Covers `1.2.2.6.5`, `1.14.1`, `1.14.1.1`, `1.14.1.2`, `1.14.1.3`, `1.14.2`, `1.14.2.1`–`1.14.2.8`, `1.14.3`, `1.14.3.1`–`1.14.3.7`, `1.14.4`, `1.14.4.1`, `1.14.4.2`, `1.14.4.3`, `1.14.4.4`. **Out of scope:** the site-access label (excluded).

**7.2 Prerequisites and dependencies.** STAGE-03 (the registered tests that are distributed over containers) and STAGE-08 (the report settings that shape the printed label content).

**7.3 Verified current state.**
- `BarcodeService` produces **one** generic Code-128 label (288 × 144 px, `BarcodeService.cs:22, 96-97, 115-121`) carrying a barcode and an identifier. The baseline needs four label kinds and a specified sticker size of 38 mm × 25 mm.
- There is **no container concept** anywhere in the domain: no entity, no enum, no designation. The audit confirms that "Serum Fasting, EDTA Blood, Citrated Blood 0.4 = 0 results in src/".
- The patient's sample types are five booleans on `PatientTest` (`PatientTest.cs:16-24`), which is a *material* statement, not a container.
- The work sheet groups tests by group (`WorkSheetPdfWriter.cs:166-176, 185-199`), which is not the baseline's sorting by material and container.
- The file barcode number has no composition: `BarcodePayload.cs:14-22` carries a bare identifier, and the exact composition (a leading digit, the laboratory code and a sequence) is not established (VERIFY #19).
- The laboratory identifier printed on the label is currently the patient identifier, switchable by a setting (`SystemSettings.PrintLabIdInsteadOfPatientId`, `BarcodeService.cs:118-120`); the baseline wants the **visit's** laboratory identifier.
- The print timestamp on the label exists but only under a setting (`SystemSettings.PrintDateTimeOnTubeBarcode`, `BarcodeService.cs:65-67`).
- Vertical coding on printed documents does not exist (no `vertical`/`rotat` occurrence under `src/`), and it must appear optionally on the result report, the receipt and the envelope.

**7.4 Exact work items.**

*W12.1 — Container designations.* Introduce the seven designations — serum fasting, serum post-prandial, EDTA blood, citrated blood 0.4, urine, stool, semen — as a domain enumeration, and let a test be mapped to the container its material requires. Baseline `1.14.3` (class D) and its seven options (class D each).

*W12.2 — Container allocation.* Allocate each of a patient's registered tests to a container, derived from the test's material by default. Baseline `1.14.4` (class D).

*W12.3 — Automatic sorting.* Sort the patient's tests by material and container, producing one allocation per container. Baseline `1.14.4.1` (class C).

*W12.4 — Container distribution edit.* Let the doctor edit the distribution of the tests over the materials drawn from the patient and the containers. Baseline `1.14.4.2` (class D).

*W12.5 — Specimen-container label.* Produce the specimen-container label: one label per container, 38 mm × 25 mm, carrying the tests allocated to that container. Baseline `1.14.1.1` (class D).

*W12.6 — File barcode label and laboratory-identifier label.* Produce the file barcode label (affixed to the patient's file) and the laboratory-identifier label (affixed to the laboratory card, carrying the laboratory code). Baseline `1.14.1.2`, `1.14.1.3` (class C each).

*W12.7 — Label content.* Add the baseline's label content: patient name, the sex-and-age line, the list of allocated test abbreviations, the barcode, the **visit's** laboratory identifier (not the patient identifier), the container designation, the file barcode number and the print timestamp. The barcode itself (`1.14.2.4`) is class A and is preserved. Baseline `1.14.2` (class D), `1.14.2.1`/`.2.2`/`.2.3` (class D each), `1.14.2.5` (class C), `1.14.2.6` (class D), `1.14.2.7` (class C + VERIFY), `1.14.2.8` (class C).

*W12.8 — File barcode number.* Compose the file barcode number as a leading digit, the laboratory code and a sequence number, with the documented default for the sequence (VERIFY #19). Baseline `1.14.2.7`.

*W12.9 — Print timestamp.* Print the date and time the label was printed under the file and laboratory-identifier labels. Baseline `1.14.2.8` (class C).

*W12.10 — Barcode action on the registration screen.* Make the registration screen's barcode action produce the **whole label set** — specimen-container labels, the file barcode and the laboratory-identifier barcode — not one generic label. Baseline `1.2.2.6.5` (class B).

*W12.11 — Vertical coding.* Add the optional vertical coding to the result report, the receipt and the envelope, each independently switchable, and render it when enabled. Baseline `1.14.4.3` (class D).

*W12.12 — Repeated sticker printing.* Make a sticker printable more than once, recording each print. Baseline `1.14.4.4` (class C).

**7.5 Traceability.** `1.2.2.6.5` (B→COMPLETE), `1.14.1` (C→COMPLETE), `1.14.1.1` (D→IMPLEMENT), `1.14.1.2`/`.1.3` (C→CORRECT), `1.14.1.4` (D→**EXCLUDED**, Exclusion 1), `1.14.2` (D→IMPLEMENT), `1.14.2.1`/`.2.2`/`.2.3` (D→IMPLEMENT), `1.14.2.4` (A→PRESERVE), `1.14.2.5` (C→CORRECT), `1.14.2.6` (D→IMPLEMENT), `1.14.2.7` (C→CORRECT + VERIFY), `1.14.2.8` (C→CORRECT), `1.14.3` (D→IMPLEMENT), `1.14.3.1`–`1.14.3.7` (D→IMPLEMENT each), `1.14.4` (D→IMPLEMENT), `1.14.4.1` (C→CORRECT), `1.14.4.2` (D→IMPLEMENT), `1.14.4.3` (D→IMPLEMENT), `1.14.4.4` (C→CORRECT).

**7.6 Database migration decision.** **NEW MIGRATION REQUIRED.**

**7.7 Migration details.**
- *Proposed name:* `AddSpecimenContainersAndLabelPrintLog`.
- *New table `SpecimenContainer`:* `SpecimenContainerId int IDENTITY` PK, `Designation tinyint NOT NULL` (the seven designations), `Code nvarchar(30) NOT NULL`, `Name nvarchar(100) NOT NULL`, `StickerWidthMm decimal(5,2) NOT NULL`, `StickerHeightMm decimal(5,2) NOT NULL`, `SortOrder int NOT NULL DEFAULT 0`. Unique index on `Code`.
- *New table `TestContainerRule`:* `TestContainerRuleId int IDENTITY` PK, `TestId int NOT NULL` FK to `Tests` (cascade), `SpecimenContainerId int NOT NULL` FK to `SpecimenContainers` (restrict). Unique index `(TestId, SpecimenContainerId)`.
- *New table `PatientTestContainerAllocation`:* `PatientTestContainerAllocationId int IDENTITY` PK, `PatientTestId int NOT NULL` FK to `PatientTests` (cascade), `SpecimenContainerId int NOT NULL` FK to `SpecimenContainers` (restrict), `SortOrder int NOT NULL DEFAULT 0`, `IsManuallyEdited bit NOT NULL DEFAULT 0`. Unique index `(PatientTestId, SpecimenContainerId)`.
- *New table `LabelPrintLog`:* `LabelPrintLogId int IDENTITY` PK, `PatientTestId int NULL` FK (cascade), `SpecimenContainerId int NULL` FK (restrict), `LabelKind tinyint NOT NULL` (container / file / laboratory-identifier), `PrintedAtUtc datetime2 NOT NULL`, `PrintedByUserId int NOT NULL`, `FileBarcodeNumber nvarchar(50) NULL`. Index `(PatientTestId, PrintedAtUtc)`.
- *Seed:* the seven container rows and the default container rules for the seeded test catalogue.
- *Data preservation:* the existing `PatientTest` sample-type booleans are **retained unchanged** and keep their meaning; the new allocation table is the container view of the same facts and is populated by the sorting routine, not by a rewrite of `PatientTest`. No existing row is modified; every new table starts empty except the seeded catalogue rows.
- *Rollback:* drop the four new tables; the sample-type booleans are untouched, so the previous shape is exact.
- *Test obligation:* the migration applies to an empty database and over the previous head; a test asserts that the seeded container catalogue is complete (seven rows) and that no pre-existing `PatientTest` row changed.

**7.8 Acceptance criteria.**
- L1. The seven container designations exist and a test maps to the container its material requires.
- L2. A patient's registered tests are allocated across containers automatically, one label per container.
- L3. The distribution can be edited by the doctor and the edit survives a reload.
- L4. The specimen-container label is 38 mm × 25 mm and carries that container's tests.
- L5. The file barcode label and the laboratory-identifier label are produced and differ from each other.
- L6. The container label carries the patient name, the sex-and-age line, the allocated test abbreviations, the barcode, the **visit's** laboratory identifier, the container designation, the file barcode number and the print timestamp.
- L7. The file barcode number is a leading digit, the laboratory code and a sequence number.
- L8. The registration screen's barcode action produces the whole label set.
- L9. The vertical coding appears on the result report, the receipt and the envelope, each switchable independently, and on none of them when all are off.
- L10. The same sticker can be printed twice, and both prints are recorded.

**7.9 Testing requirements.**
- *Domain tests:* the seven designations are distinct; a test's default container resolves; an allocation is unique per test and container.
- *Application tests:* the sorting routine produces one allocation per container; an edited distribution is preserved; the label print log records each print.
- *Printing tests:* the container label measures 38 × 25 mm and lists the allocated tests; the file and laboratory-identifier labels carry their own content; the vertical coding appears only where enabled.
- *Persistence tests:* the migration applies and rolls back; the seven containers are seeded; no pre-existing `PatientTest` row changed.
- *UI tests:* the distribution editor round-trips; the barcode action yields three kinds of label.

**7.10 Preservation and compatibility requirements.** The `PatientTest` sample-type booleans and their existing command keep working unchanged — the container allocation is additive. The existing generic barcode service and the existing work-sheet grouping keep working. `1.14.2.4` (the barcode) is class A and is preserved. The settings `PrintLabIdInsteadOfPatientId` and `PrintDateTimeOnTubeBarcode` keep their meaning for the workflows that use them; the baseline's label content is driven by the new fields. No site-access label is produced.

**7.11 Completion gate.** All ten criteria hold; the migration applies and rolls back on an ephemeral container; no pre-existing result row changed; the decision record for VERIFY #19 is written.

### E.13 STAGE-13 — Medical history insertion and the history report

**7.1 Objective and scope.** Make the history automatic and complete: insert the earlier results automatically, let the user choose them by hand, and produce the baseline's history report with its matrix, its multi-patient panel and its two test lists. Covers `1.15.1`, `1.15.1.1`, `1.15.1.2`, `1.15.2`, `1.15.2.1`, `1.15.2.2`, `1.15.2.3`, `1.15.2.4`, `1.15.2.5`, `1.15.3`, `1.15.3.1`, `1.15.3.2`, `1.15.3.4`, `1.15.4`, `1.15.4.1`, `1.15.4.1.2`, `1.15.4.1.3`, `1.15.4.1.4`, `1.15.4.2`, `1.15.4.2.2`, `1.15.4.2.3`, `1.15.4.2.4`, `1.15.4.3`, `1.15.4.3.1`, `1.15.4.3.2`, `1.15.4.4`, `1.15.4.4.1`, `1.15.4.4.2`, `1.15.4.5`, `1.15.4.6`, `1.15.4.7`, `1.6.1.7.4`. **Out of scope:** the report settings that govern the history (STAGE-08).

**7.2 Prerequisites and dependencies.** STAGE-07 (the report screen's history action opens this stage's dialog), STAGE-08 (the report settings and the per-report history override), STAGE-09 (the combined-report pipeline, whose separate-report mode reuses it).

**7.3 Verified current state.**
- The history machinery exists: `GetPatientTestHistoryQuery`, `GetMultiPatientHistoryQuery`, `GetSeparateHistoryReportQuery`, `HistoryMatrixBuilder`, `AutoInsertHistoryCommandHandler` and `InsertHistoryResultCommandHandler`.
- The insertion is **manual**: the user presses a button and confirms (`CombinedReportView.xaml:30`, `CombinedReportViewModel.cs:325-372`); the baseline requires the earlier result to be inserted **automatically**.
- `AutoInsertHistoryCommandHandler.cs:66-71` inserts the earlier result for the **same test only**, so the baseline's multi-test insertion is not supported.
- The history lines never reach the printed report: `PrintCombinedReportCommandHandler.cs:51-53` rebuilds the report from the current tests only. This is the audit's headline defect for the group.
- The history screen is opened with `patientId = 0` (`PatientsHubViewModel.cs:79-87`), so the history path always fails.
- `HistoryReportsViewModel` carries `MultiPatientIds` publicly but nothing binds to it (`:94`), so the multi-patient panel does not exist.
- The output-mode block exists in the view model (`SelectedMode`, `IsSingleMode`, `IsMultiMode`, `IsSeparateMode`) but the baseline's two modes — attach to the result report, or print as a report of its own — are not the model's shape, and the confirm and cancel actions of the baseline are not present.
- The report's history grid is not the baseline's matrix: the baseline's history report has **one row per analysis date**, the patient identity, and **one column per selected test**; `HistoryMatrixBuilder` exists but is not wired to the printed report.
- The history report header carries no period, no printing user and no print date; `PrintHistoryReportCommand` carries only a patient id.
- The two test lists (the tests in the lab and the tests already chosen for a history) with their counts do not exist.
- The delete-all action does not exist.
- The history report has no preview.

**7.4 Exact work items.**

*W13.1 — Automatic insertion.* Insert the earlier result into the result report **automatically** when the patient has had the same test before, governed by the report settings' history option, with the manual button retained as an override. Baseline `1.15.1` (class C), `1.15.1.1` (class C).

*W13.2 — Multi-test insertion.* Insert the history of **several** tests into a result report alongside a group of tests, and into a combined report. Baseline `1.15.1.2` (class C).

*W13.3 — History in the printed artefact.* Carry the inserted history lines into the printed report; the report is no longer rebuilt from the current tests alone. This is the single change that makes the whole group work. Baseline `1.15.1.1`/`1.15.1.2` and the printed report of the group.

*W13.4 — Previous-visit test list and selection.* Show the tests the patient had in earlier visits, with the earlier results of the selected test — result, status and analysis date — and support selection by a double press. Baseline `1.15.2` (class C), `1.15.2.1` (class B), `1.15.2.2` (class C), `1.15.2.5` (class C).

*W13.5 — Previous-visit count.* Show how many earlier visits the patient has. Baseline `1.15.2.3` (class D).

*W13.6 — Repeated-test note.* Note that the patient has had a given test more than once. Baseline `1.15.2.4` (class D).

*W13.7 — Output mode and confirm/cancel.* Present the baseline's two output modes — attach the history in the result report, or print it as a report of its own — with the confirm and cancel actions. The confirm action keeps the history mode confirm (class A); the cancel action is added (class B). Baseline `1.15.3` (class C), `1.15.3.1`/`.3.2` (class C each), `1.15.3.4` (class B); `1.15.3.3` (class A) preserved.

*W13.8 — History report header.* Give the history report its period, the user who printed it and the date it was printed. The title is class A and is preserved. Baseline `1.15.4.1` (class C), `1.15.4.1.2` (class C), `1.15.4.1.3` (class D), `1.15.4.1.4` (class C).

*W13.9 — History report matrix.* Produce the history report with one row per analysis date, the patient identity, and one column per selected test. How a test that was not performed on a given date is represented is not established (VERIFY #20): default = the cell is empty. Baseline `1.15.4.2` (class C), `1.15.4.2.1` (class B), `1.15.4.2.2`/`.2.3`/`.2.4` (class C each).

*W13.10 — Multi-patient panel.* Add the panel used to build a history for more than one patient in the same report, with its grid (patient name, identifier, analysis date) and its count, and bind the existing `MultiPatientIds`. Baseline `1.15.4.3` (class C), `1.15.4.3.1`/`.3.2` (class D each).

*W13.11 — History test lists.* Add the two lists — the tests available to be given a history, and the tests already chosen — each with its count. Baseline `1.15.4.4` (class D), `1.15.4.4.1`/`.4.2` (class D each).

*W13.12 — Delete all history.* Add the action that removes every selection from the history report. Baseline `1.15.4.5` (class D).

*W13.13 — History report print and preview.* Print the history report and preview it before printing. Baseline `1.15.4.6` (class C), `1.15.4.7` (class D).

*W13.14 — Report-screen history action.* Wire the report screen's patient-history action to this stage's dialog. Baseline `1.6.1.7.4` (class D, cross-referenced from STAGE-07).

**7.5 Traceability.** `1.15.1` (C→CORRECT), `1.15.1.1` (C→CORRECT), `1.15.1.2` (C→CORRECT), `1.15.1.3`/`.1.3.1`/`.1.3.2` (A→PRESERVE), `1.15.1.4`/`.1.5` (A→PRESERVE), `1.15.2` (C→CORRECT), `1.15.2.1` (B→COMPLETE), `1.15.2.2` (C→CORRECT), `1.15.2.3` (D→IMPLEMENT), `1.15.2.4` (D→IMPLEMENT), `1.15.2.5` (C→CORRECT), `1.15.3` (C→CORRECT), `1.15.3.1`/`.3.2` (C→CORRECT), `1.15.3.3` (A→PRESERVE), `1.15.3.4` (B→COMPLETE), `1.15.4` (C→CORRECT), `1.15.4.1` (C→CORRECT), `1.15.4.1.1` (A→PRESERVE), `1.15.4.1.2` (C→CORRECT), `1.15.4.1.3` (D→IMPLEMENT), `1.15.4.1.4` (C→CORRECT), `1.15.4.2` (C→CORRECT), `1.15.4.2.1` (B→COMPLETE), `1.15.4.2.2`/`.2.3`/`.2.4` (C→CORRECT), `1.15.4.3` (C→CORRECT), `1.15.4.3.1`/`.3.2` (D→IMPLEMENT), `1.15.4.4` (D→IMPLEMENT), `1.15.4.4.1`/`.4.2` (D→IMPLEMENT), `1.15.4.5` (D→IMPLEMENT), `1.15.4.6` (C→CORRECT), `1.15.4.7` (D→IMPLEMENT), `1.6.1.7.4` (D→IMPLEMENT).

**7.6 Database migration decision.** **NO MIGRATION REQUIRED.** The history is derived from the results that already exist; the printed report and the preview are documents; the selections are session state.

**7.7 Migration details.** Not applicable — no schema change. The history's own selections (the chosen tests, the chosen patients) are **session state**, carried in the print request, exactly as the combined report's selection is today; no persistence is required by the baseline. Adding a persisted "history template" table would be scope creep and is explicitly not planned.

**7.8 Acceptance criteria.**
- M1. With the history option on, the earlier result of a repeated test appears in the result report **without** the user pressing anything.
- M2. The history of several tests can be inserted alongside a group of tests, and into a combined report.
- M3. The inserted history lines appear in the **printed** report.
- M4. The dialog lists the patient's earlier-visit tests, and selecting one by a double press shows its earlier results with result, status and analysis date.
- M5. The number of earlier visits is shown.
- M6. A test the patient has had more than once carries the repeated-test note.
- M7. The two output modes are offered and each behaves as described; the confirm action commits and the cancel action abandons.
- M8. The printed history report carries its period, the printing user and the print date.
- M9. The printed history report has one row per analysis date with the patient identity and one column per selected test.
- M10. The multi-patient panel lists the patients with their identifier and analysis date, shows their count, and the history report can be built for several of them.
- M11. Both test lists are shown with their counts.
- M12. The delete-all action removes every selection.
- M13. The history report prints and previews.

**7.9 Testing requirements.**
- *Application tests:* the automatic insertion fires for a repeated test and not for a first-time test; the multi-test insertion selects several tests; the printed report's content includes the inserted history lines.
- *ViewModel tests:* the previous-visit list is populated; the double press selects; the count and the repeated-test note are computed; the multi-patient list and its count are maintained; delete-all empties both lists.
- *Printing tests:* the history report has one row per analysis date and one column per selected test; the header carries the period, the user and the date.
- *UI tests:* the two output modes, the confirm action and the cancel action render and behave.
- *Regression:* the combined report without history prints exactly as before.

**7.10 Preservation and compatibility requirements.** The history identification settings (by name, by laboratory code) keep their meaning and their identifiers `1.15.1.3`/`.1.3.1`/`.1.3.2` are class A and are preserved. The automatic-history display setting and the history sorting method (`1.15.1.4`, `1.15.1.5`) keep their stored values and their effect. The history title (`1.15.4.1.1`) and the analysis-date column (`1.15.4.2.1`) keep their content. The history screen is reachable with a real patient id. The report settings' history options keep their stored meaning.

**7.11 Completion gate.** All thirteen criteria hold; no schema change was made; the combined report without history prints unchanged; the decision records for VERIFY #20 are written; `VER-01` (the 1.15 classification reconciliation) is closed before this stage is signed off.

### E.14 STAGE-14 — Case follow-up, audit trail, patient samples

**7.1 Objective and scope.** Complete the case-history screen: the last printer entry, and the patient's samples with their tube name, their tests and the taking action. Covers `1.12.1.7`, `1.12.1.9`, `1.12.1.9.1`, `1.12.1.9.2`, `1.12.1.9.3`. **Out of scope:** the eight audit entries that already work (class A, preserved).

**7.2 Prerequisites and dependencies.** STAGE-12 (the container model the samples block lists) and STAGE-02 (the registration screen's follow-up action opens this screen).

**7.3 Verified current state.**
- The case-history screen exists (`AuditView.xaml:1-124`, `AuditViewModel.cs:16-188`) and shows eight of the baseline's entries: who registered, the modification count, the last modifier, who entered the results, who reviewed them, the print count, who delivered. Two entries are incomplete: the **last printer** shows the name only — the date and time are loaded into the DTO (`AuditDtos.cs:45-47`) but not bound — and the **patient samples** block does not exist at all (`AuditView.xaml:59-89`).
- The samples block's tube name has no source, because there is no container model until STAGE-12.
- The "sample take" action exists elsewhere as a per-test action on the sample-draw board (`SampleDrawBoardView.xaml:81-84`, `MarkSampleDrawnCommandHandler.cs:22-54`), not as a samples-block action; the baseline wants the action in the samples block.
- The samples block's tests column is one row per test today (`SampleDrawBoardView.xaml:86, 124`), not one row per sample with its tests.

**7.4 Exact work items.**

*W14.1 — Last printer entry.* Bind the last printer's date and time next to the name, using the value the query already loads. Baseline `1.12.1.7` (class B).

*W14.2 — Patient samples block.* Add the block listing the patient's samples with the baseline's three columns — tube name, the tests on that sample, and the taking action. The tube name comes from STAGE-12's container allocation. Baseline `1.12.1.9` (class D), `1.12.1.9.1` (class D), `1.12.1.9.2` (class C).

*W14.3 — Sample take action.* Move the taking action into the samples block, one row per sample, acting on the sample's tests. The existing per-test action on the sample-draw board keeps working. Baseline `1.12.1.9.3` (class B).

**7.5 Traceability.** `1.12.1` (A→PRESERVE), `1.12.1.1`–`1.12.1.6` and `1.12.1.8` (A→PRESERVE), `1.12.1.7` (B→COMPLETE), `1.12.1.9` (D→IMPLEMENT), `1.12.1.9.1` (D→IMPLEMENT), `1.12.1.9.2` (C→CORRECT), `1.12.1.9.3` (B→CORRECT).

**7.6 Database migration decision.** **NO MIGRATION REQUIRED.** The samples block reads STAGE-12's container allocation and the existing sample-draw facts; the last-printer entry binds a value the query already loads.

**7.7 Migration details.** Not applicable — no schema change.

**7.8 Acceptance criteria.**
- N1. The last printer entry shows the name **and** the date and time.
- N2. The patient samples block appears on the case-history screen.
- N3. Each row shows the tube name and the tests on that sample.
- N4. The taking action on a row marks that sample's tests as taken.
- N5. The eight existing audit entries are unchanged.

**7.9 Testing requirements.**
- *Application tests:* the audit query returns the last printer's timestamp; the samples query groups the patient's tests by container.
- *ViewModel tests:* the samples block is populated from the allocation; the take action issues the mark command for the sample's tests.
- *UI tests:* the block renders tube name, tests and the take action; the last-printer entry shows a date and time.
- *Regression:* the eight existing audit entries render unchanged.

**7.10 Preservation and compatibility requirements.** The eight working audit entries and their values are untouched. The existing per-test "take" action on the sample-draw board keeps working. `1.12.1.1`–`1.12.1.6` and `1.12.1.8` are class A and are preserved.

**7.11 Completion gate.** All five criteria hold; no schema change was made; the eight existing audit entries are unchanged.

### E.15 STAGE-15 — Application shell, sign-in, startup data source, ribbon navigation

**7.1 Objective and scope.** Give the application the shell the baseline describes: a ribbon that marks the active module and opens the delivery screen, a status bar that reports the previous sign-in and the data source, a startup splash with a data-source choice and a progress indication, and a sign-in window that can remember the sign-in. Covers `1.1.1`, `1.1.1.1`, `1.1.1.1.3`, `1.1.1.2`, `1.1.2.2`, `1.1.2.3`, `1.1.3`, `1.1.3.1`, `1.1.3.1.1`, `1.1.3.1.2`, `1.1.3.2`, `1.1.4.4`. **Out of scope:** the patient-module quick-search toolbar (STAGE-05).

**7.2 Prerequisites and dependencies.** STAGE-10 (the delivery screen the Send Result item opens), STAGE-05 (the search screen the patient-module toolbar uses).

**7.3 Verified current state.**
- The ribbon carries 12 items and the module of each; there is **no** Send Result item (`ShellViewModel.cs:189`) and **no** active-module state (`ShellViewModel.cs:16-21`; `MainWindow.xaml:41-45` has no trigger).
- The status bar shows the user name, a **connection** indicator and the date. The connection indicator shows connected/disconnected (`ShellViewModel.cs:363-373`) rather than the data source's label; and the last-login indicator shows the **current** sign-in, not the previous one, because `SignInCommandHandler` overwrites `LastLoginAtUtc` before the session query reads it (`GetCurrentSessionQueryHandler.cs:29`).
- There is **no** splash screen: the start sequence runs inside `App.xaml.cs:28-113` (database setup → migrations → admin → login) with no progress indication. The database setup window shows text, not progress (`DatabaseSetupWindow.xaml:44-45`).
- The data-source option group does not exist: the setup window offers one source, SQL Server, through five fields (`DatabaseSetupWindow.xaml:29-42`; `DatabaseSetupViewModel.cs:59-93`). There is no file-database option.
- The sign-in window has no remember-login option (`LoginWindow.xaml:1-46`), and the remembered sign-in must not persist a password.

**7.4 Exact work items.**

*W15.1 — Send Result ribbon item.* Add the ribbon item that opens the result-delivery screen, and mark the item of the currently open module as active. Baseline `1.1.1.1.3` (class D), `1.1.1.2` (class D).

*W15.2 — Active-module indicator.* Track the open module and show its ribbon item as active. Baseline `1.1.1.2` (class D).

*W15.3 — Last login indicator.* Show the **previous** sign-in of that user with its date and time, not the current one. Baseline `1.1.2.2` (class C).

*W15.4 — Database label indicator.* Show the label of the data source in use — the database kind and name — instead of a connection state. Baseline `1.1.2.3` (class C).

*W15.5 — Startup splash screen.* Add a splash screen shown while the program starts, carrying the data-source choice and the progress indication, and move the start sequence into it. Baseline `1.1.3` (class C), `1.1.3.2` (class D).

*W15.6 — Data-source option group.* Offer the two documented data sources at startup. The **file-database option** (class D) is implemented as a locally attached file-backed SQL Server instance (the same provider, a file-attached database) so that the option is real without a second database technology and without any network dependency. The client-server option (class B) keeps today's behaviour. The concrete binding of the file database to the existing schema is a **CONDITIONAL** item: it needs the deployment's SQL Server edition and the attachment mechanism confirmed before it is built (see 7.6). Baseline `1.1.3.1` (class D), `1.1.3.1.1` (class D), `1.1.3.1.2` (class B).

*W15.7 — Startup progress.* Report the progress of the start sequence on the splash screen. Baseline `1.1.3.2` (class D).

*W15.8 — Remember login.* Add the remember-login option. It stores the **user name only** — never a password — in the local workstation configuration file, and pre-fills the sign-in field on the next start. Baseline `1.1.4.4` (class D).

**7.5 Traceability.** `1.1.1` (B→CORRECT), `1.1.1.1` (B→COMPLETE), `1.1.1.1.1`/`.1.2`/`.1.4`–`.1.12` (A→PRESERVE), `1.1.1.1.3` (D→IMPLEMENT), `1.1.1.2` (D→IMPLEMENT), `1.1.2` (A→PRESERVE), `1.1.2.1`/`.2.4` (A→PRESERVE), `1.1.2.2` (C→CORRECT), `1.1.2.3` (C→CORRECT), `1.1.3` (C→CORRECT), `1.1.3.1` (D→IMPLEMENT), `1.1.3.1.1` (D→IMPLEMENT + CONDITIONAL), `1.1.3.1.2` (B→COMPLETE), `1.1.3.2` (D→IMPLEMENT), `1.1.4` (A→PRESERVE), `1.1.4.1`/`.2`/`.3`/`.5` (A→PRESERVE), `1.1.4.4` (D→IMPLEMENT).

**7.6 Database migration decision.** **CONDITIONAL — VERIFY BEFORE DECIDING.**

**7.7 Migration details and the conditional decision.** The stage's own work — the splash screen, the progress indication, the ribbon items, the active-module state, the status-bar semantics and the remember-login option — needs **no** schema change: the remembered user name goes in the local workstation configuration file, and the previous sign-in is read from the sign-in flow before it is overwritten. The **conditional** part is the file-database option (`1.1.3.1.1`).

- *What must be inspected before deciding:* the deployment's SQL Server edition and whether a file-attached database (`.mdf`) is available to it; whether the migration chain runs identically against an attached file database; and whether the customer's data volume allows a file-attached copy of the production database. `VER-03`.
- *Outcome A — file-attached SQL Server is available and approved:* the option is configured as a second connection to an attached file database. **No schema change.** The same migrations apply. This is the recommended outcome, because it keeps one database technology, one schema and one migration chain.
- *Outcome B — file-attached SQL Server is not available or not approved:* the option is presented and disabled with an explanatory label, and the client-server option remains the only usable one. **No schema change.** The option is then recorded as unavailable in this deployment, and the element is dispositioned as delivered-with-a-documented-deployment-limit rather than as a gap.
- *Outcome C — a different, genuinely embedded file technology is required (for example SQLite):* this is **out of scope for this plan** and would need a new package, a second schema and a second migration chain. It is recorded as a **future decision** and is **not** planned. It must not be started as part of this unit.
- *Rollback:* not applicable — no schema change is made under any of the three outcomes.
- *Test obligation:* under Outcome A, a persistence test must apply the migration chain against a file-attached database and assert the same schema as against a server database.

**7.8 Acceptance criteria.**
- O1. The ribbon carries the Send Result item and opens the delivery screen.
- O2. The ribbon item of the open module is shown as active.
- O3. The status bar shows the user's **previous** sign-in with its date and time.
- O4. The status bar shows the data source's label, not a connection state.
- O5. A splash screen is shown while the program starts and reports progress.
- O6. The data-source choice is offered on the splash screen, and the client-server source still works exactly as today.
- O7. The file-database source works against a file-attached database under Outcome A, or is explicitly unavailable under Outcome B, and is never a second database technology.
- O8. Remember-login stores the user name only — no password is written to any file or to the database — and the sign-in field is pre-filled on the next start.
- O9. The eleven existing ribbon items and the sign-in window's existing fields keep working.

**7.9 Testing requirements.**
- *Application tests:* the sign-in flow captures the previous sign-in before overwriting it; the current-session query returns it.
- *ViewModel tests:* the active module is tracked on navigation; the status bar shows the data-source label; the remembered user name is read and written by the local configuration service.
- *UI tests:* the ribbon shows the active item; the splash screen appears during start-up; the remember-login option persists the user name and never a password.
- *Security test:* an automated assertion that **no** password or hash is written to the workstation configuration file.
- *Regression:* the eleven existing ribbon items and the existing sign-in fields are unchanged.

**7.10 Preservation and compatibility requirements.** The eleven existing ribbon items, the sign-in window's three existing fields and the sign-in action are class A and are preserved. The user-name and today indicators in the status bar are preserved. The client-server database setup keeps working exactly as today, including its five fields and its connection test. The start sequence's order (setup → migrations → admin → sign-in) is preserved, only surfaced on a splash screen.

**7.11 Completion gate.** All nine criteria hold; the decision under `VER-03` is recorded; no password is persisted anywhere; no new package was added to `Directory.Packages.props`; no HTTP client exists in the solution.

---

## F. Consolidated Database and Migration Strategy

### F.1 Migration inventory

Seven of the fifteen stages require a migration. All seven are **additive**: a new column, a new table, a seeded row, or a documented nullability tightening on one column with an explicit backfill. No stage drops a column, renames a column, retypes a column, or deletes a row.

| Order | Stage | Proposed migration | Objects touched | Kind |
|---|---|---|---|---|
| 1 | STAGE-01 | `AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue` | `Patient` (+2 columns), `SystemSettings` (+2 columns), `MedicalConditionType` (+11 seeded rows) | Additive |
| 2 | STAGE-04 | `AddPaymentOperationRefundAmount` | `PaymentOperation` (+1 column) | Additive |
| 3 | STAGE-06 | `AddResultEntryColumnsAndCriticalFlag` | `PatientTest` (+7 columns), `AnalyteReferenceRangeBand` (+2), `PatientTestReferenceRangeSnapshot` (+3) | Additive |
| 4 | STAGE-07 | `AddProfileResultEntryColumns` | `ProfileResultItem` (+8 columns), new table `PatientTestSampleResult` | Additive |
| 5 | STAGE-08 | `AddReportSettingsTitlesTypographyAndSymbols` | `ReportSettings` (+21 columns) | Additive |
| 6 | STAGE-11 | `AddCultureMicroscopyFieldsAndReferenceRanges` | `CultureMicroscopies` (+5 columns), `CultureAntibioticResults` (nullability), new table `CultureMicroscopyReferenceRange` | Additive + one nullability tightening |
| 7 | STAGE-12 | `AddSpecimenContainersAndLabelPrintLog` | new tables `SpecimenContainer`, `TestContainerRule`, `PatientTestContainerAllocation`, `LabelPrintLog`; seeded container rows | Additive |

The eight remaining stages — STAGE-02, STAGE-03, STAGE-05, STAGE-09, STAGE-10, STAGE-13, STAGE-14, STAGE-15 — require **no** schema change, and each says so explicitly with the reason.

### F.2 Ordering and dependencies between the migrations

The migrations are strictly appended in the order above. No migration depends on a later one, and the sequence follows the stage order exactly, so the migration chain and the stage order can never diverge.

- STAGE-01 must precede STAGE-02 and STAGE-05 (the card number and the condition catalogue are read by both).
- STAGE-04's refund column is independent of everything else and can be applied at any point after STAGE-01.
- STAGE-06's columns are read by STAGE-07 (the grid's unit, band and flag) and by STAGE-10 (the delivery grid's finish and export boxes) and by STAGE-12 (the container allocation).
- STAGE-07's columns are read by STAGE-08 (the settings that shape the report) and by STAGE-09.
- STAGE-08's settings are read by STAGE-09, STAGE-11 and STAGE-13 — the three stages that print documents.
- STAGE-11's nullability tightening on `CultureAntibioticResults.SensitivityCategory` is the **only** migration in the plan that constrains existing data, and it therefore lands **after** STAGE-01, so the backfill has the corrected semantics of the earlier repair migration available to it and does not have to re-derive them.

### F.3 Data-preservation rules (binding on every stage)

1. **No destructive change.** No column is dropped, renamed or retyped; no row is deleted. Every "remove" in this plan is implemented as a domain or query filter, not as a schema operation.
2. **New columns are nullable or carry a behaviour-preserving default.** A new `bit NOT NULL DEFAULT 0` means "not finished", "not complete", "not expanded", "no status flag" — each of which is exactly today's behaviour.
3. **New enum members are appended, never renumbered.** `ResultFlag.Critical = 3` is appended after `Normal = 0, Low = 1, High = 2`; `ProfileResultFlag.Normal = 2` is appended after `Low = 0, High = 1`. Because the columns are `int` conversions of these enums, the stored values of every existing row remain valid and keep their meaning. **Renumbering either enum would silently corrupt every stored flag**, and is explicitly forbidden.
4. **Seed data is append-only.** The eleven new `MedicalConditionType` rows, the seven `SpecimenContainer` rows and the `CultureMicroscopyReferenceRange` rows are inserted with new identifiers. The existing pregnancy row (id 1) is never modified, re-categorised or deleted.
5. **The one nullability tightening** (STAGE-11) is performed only after a deterministic backfill, and any row that cannot be classified deterministically is **reported**, never silently assigned a fabricated clinical value. If a deterministic backfill cannot be established, the column stays nullable and the invariant is enforced in the domain and the validator instead — the migration decision then records that outcome. A fabricated "Moderate For" on a real clinical result is prohibited.
6. **New tables start empty** except for seeded catalogue rows, so no historical row is reinterpreted.
7. **The model snapshot is regenerated with the migration**, and the `HasData` seed is extended in the same change, so a database created from the migrations and a database migrated forward end with the same seeded rows.
8. **Every migration has a rollback**, and the rollback is tested. A rollback must restore the *previous* schema exactly — that means the "no data loss" property is verified in both directions, and the additive-only rule is what makes that possible.

### F.4 Rules that protect the existing data (a consolidated checklist)

| Rule | Why | How it is checked |
|---|---|---|
| Never edit an applied migration | The audit's migration set is deployed; rewriting history desynchronises every environment | The new migration is appended with a later timestamp; the 21 existing files are untouched (verified by `git diff` on the Migrations folder) |
| Never renumber `ResultFlag` or `ProfileResultFlag` | Both are stored as `int`; renumbering silently reclassifies every stored flag | A persistence test asserts the stored flag of a pre-existing row is unchanged after the migration |
| Never drop `Patient.PickupDateUtc` | It is the receipt date STAGE-01 starts using | The column is populated, not recreated |
| Never drop `PatientTest`'s sample-type booleans | The container model in STAGE-12 is additive to them | A persistence test asserts the booleans of a pre-existing row are unchanged |
| Never drop `ReportItemTemplate` | The per-item template screen keeps working alongside the new report-level settings | The template screen's rows and columns are unchanged; a test asserts the seeded rows are unchanged |
| Never hard-delete a `PaymentOperation` | The account is a financial ledger | STAGE-04's "delete transaction" removes the row from the *account view* and records the deletion in the case audit trail; the ledger row itself is voided or archived, never destroyed |
| Never write a password in plain text | The print-result password and the remembered sign-in | The print-result password is stored only as a hash; the remembered sign-in stores only the user name; an automated test asserts no password material is written to the configuration file |
| Never introduce a second database technology | One schema, one migration chain, offline-first | `Directory.Packages.props` gains no provider; a build check asserts no new EF provider package |

### F.5 Migration verification procedure (executed once, before the first stage)

Because the task forbids execution, this procedure is **specified here** and must be run by the implementing agent as part of STAGE-01.

1. `dotnet build TopLab.sln -c Release` — record the warning and error counts as the baseline.
2. `dotnet test TopLab.sln -c Release` — record the per-project pass/fail/skip counts as the baseline. Any later failure is then attributable to this work.
3. `dotnet ef migrations list` against the target database — record which of the 21 migrations are applied and which are pending. This is `VER-02`; the design assumes all 21 are applied, and the first new migration is appended after `20261018144852_AddProfileResultStyling` regardless.
4. Inspect the **live** seeded values of `MedicalConditionType`, `SystemSettings`, `ReportSettings` and `ReportItemTemplate` — the audit could only read the seed in code, not in a database. This is `VER-03`.
5. Apply the seven new migrations in order to an ephemeral container and to a copy of the target database, and assert the row counts of `Patient`, `PatientTest`, `ProfileResultItem`, `PaymentOperation`, `CultureAntibioticResult` and `ReportSettings` are unchanged by every migration, and that every new column is null or default on pre-existing rows.
6. Roll every new migration back in reverse order and assert the schema matches the pre-stage shape exactly.

---

## G. Complete Traceability Matrix

### G.1 How to read the matrix

One row per baseline identifier, all 491 of them. Columns: **ID** (baseline identifier), **Element** (canonical name), **Class** (baseline conformance class: A matching, B partial, C different, D absent), **Disposition** (the decision from Section C.1), **Stage** (the single owning stage, or `—` for excluded and preserved elements), **Acceptance** (the criterion in the owning stage that proves the element), and **Migration** (the stage's migration decision, or `—`).

`PRESERVE` rows carry the stage whose **regression** covers them. `EXCLUDED` rows carry `—` for both stage and migration: they are never implemented, never tested, and never reported as a gap.

### G.2 Disposition totals (reconciled against the matrix)

| Disposition | Count | Composition |
|---|---|---|
| `IMPLEMENT` | 141 | The capability is absent and is built |
| `COMPLETE` | 84 | Present but incomplete, or present with an incomplete feature set |
| `CORRECT` | 88 | Present but observably different from the baseline |
| `PRESERVE` | 146 | Already matches the baseline — **no implementation work** |
| `EXCLUDED` | 32 | 24 under Exclusion 1, 2 under Exclusion 4, 6 under Exclusion 5 |
| **Total** | **491** | 459 in scope + 32 excluded |

`VERIFY` is an **additional** attribute on 20 of the in-scope rows, marked `+V` in the matrix; it does not add to the totals.

**How the disposition relates to the baseline class.** The baseline class describes *how much* of an element is present; the disposition describes *what must be done*. For class A the two coincide (`PRESERVE`). For classes B, C and D the disposition is driven by the **observed defect**, so the two are not in one-to-one correspondence: 24 class-B elements are dispositioned `CORRECT` because the element exists but behaves observably wrongly (for example an action offered as a picker where the baseline requires a menu, or a value present where the baseline requires a computed figure), and one class-C element (`1.14.1` Barcode Label Kinds) is dispositioned `COMPLETE` because the defect is an incomplete feature set rather than wrong behaviour. 31 class-D elements and one class-C element fall inside the exclusions and are dispositioned `EXCLUDED`. The per-element table in Section G.3 is authoritative over this summary.

**Cross-check against the baseline classes.** Every class-A element carries `PRESERVE` and no other disposition; no class-A element is scheduled for work anywhere in Sections E or F. Every class-D element is either `IMPLEMENT` or `EXCLUDED` — never `COMPLETE` and never `CORRECT`, because neither of those dispositions can apply to an absent capability. The counts in the table above are computed from the matrix rows in Section G.3 (row count, distinct-identifier check, and disposition totals were re-verified by recounting the matrix; see the verification blocker below). The baseline audit's own header summary states A 146 / B 108 / C 61 / D 176; the per-element evidence used in this plan gives A 146 / B 107 / C 66 / D 172 — a shift of 5 elements between C and D and 1 between B and C, with A = 146 agreeing exactly. The 5 disputed elements all sit inside group 1.15 and are governed by `VER-01`; they change no stage boundary. `VER-01` remains open: the 36 unpersisted group-1.15 identifiers must be re-derived from code before STAGE-13 sign-off.

---

## H. Cross-Stage Testing and Regression Strategy

### H.1 What is verified at which level

| Level | Project | What it proves | When it runs |
|---|---|---|---|
| Domain | `TopLab.Domain.Tests` | Entity invariants, the account calculator, the flag computer, the money rules | On every stage's commit |
| Application | `TopLab.Application.Tests` | Command and query behaviour, the DTO shapes, the validation rules | On every stage's commit |
| Infrastructure | `TopLab.Infrastructure.Tests` | The printing writers, the barcode writer, the PDF output, the configuration file service | On every stage's commit |
| Persistence | `TopLab.Persistence.Tests` | The migration chain, the data-preservation rules, the rollback | On every stage that carries a migration |
| Presentation | `TopLab.Presentation.Tests` | View-model behaviour and the view composition | On every stage's commit |

`TopLab.Persistence.Tests` uses `Testcontainers.MsSql`, so the relational tests need a container runtime. The existing `SqlServerFixture` **skips cleanly** when Docker is unavailable, and `NeverConnectGuardTests` forbids the persistence tests from reading a configured or local database. Both behaviours are preserved: the persistence tests must never be pointed at the laboratory's own database.

### H.2 The regression strategy for preserved elements

The 146 preserved elements are protected in three ways, and a stage that touches their surface must satisfy all three.

1. **Assertion-level.** Each stage's test list names the preserved elements it touches and asserts their behaviour explicitly. Examples that this plan requires: STAGE-06 must assert that the abbreviation, result and double-click columns behave as before; STAGE-08 must assert that the abnormal-background option and the per-item offset still work and that a default report renders identically; STAGE-12 must assert that the per-test sample-type booleans and their command are unchanged; STAGE-14 must assert that the eight working audit entries render unchanged; STAGE-15 must assert that the eleven existing ribbon items and the three existing sign-in fields are unchanged.

2. **Report-level.** The three documents that already render — the result report, the receipt and the invoice — must produce an **identical layout** before and after the stage, unless the stage deliberately changes that document. STAGE-08 and STAGE-12 both change the documents, so they carry a **golden-output comparison**: a report rendered from an unchanged database must match the pre-stage rendering element for element.

3. **Data-level.** The row counts of `Patient`, `PatientTest`, `ProfileResultItem`, `PaymentOperation`, `CultureAntibioticResult`, `CultureResult`, `ReportSettings` and `SystemSettings` must be identical before and after every migration, and every added column must be null or at its behaviour-preserving default on every pre-existing row.

### H.3 Migration test obligations (cumulative, per stage)

| Obligation | Applies to | Assertion |
|---|---|---|
| M-T1 | Every migration | The full chain applies to an **empty** database, ending in the expected schema |
| M-T2 | Every migration | The full chain applies **over** the previous head (the incremental path a live database takes) |
| M-T3 | Every migration | No existing row in any of the eight counted tables is modified, deleted or retyped |
| M-T4 | Every migration | Every added column is null, or at its behaviour-preserving default, on pre-existing rows |
| M-T5 | Every migration | The rollback restores the previous schema exactly, and the data survives it |
| M-T6 | STAGE-06, STAGE-07 | The stored flag of a pre-existing result is unchanged after the migration (the enum-append rule) |
| M-T7 | STAGE-11 | Every pre-existing sensitivity classification is unchanged, and unclassifiable rows are **listed** rather than assigned a value |
| M-T8 | STAGE-04 | The balance of a refund-free patient is bit-identical to its pre-stage value |
| M-T9 | STAGE-12 | The seven container rows are seeded, and no pre-existing `PatientTest` row changed |
| M-T10 | STAGE-15 | Under Outcome A, the chain applies to a file-attached database and yields the same schema |

### H.4 The end-to-end scenarios that must pass before the unit is closed

These cross stages and are the real proof of parity. Each is a scripted flow, executed manually and then captured as a test.

| # | Scenario | Stages exercised | Proof |
|---|---|---|---|
| E2E-1 | Register a patient with a card number, sample types and medical-history answers; print the receipt; find the patient by card number; view the visit | 01, 02, 04, 05 | The identifier, the card number, the answers and the receipt agree |
| E2E-2 | Register a **returning** patient; the same laboratory identifier appears; searching it lists both visits | 01, 05 | Requirement 1.11.1.3 and 1.11.2.1 |
| E2E-3 | Register tests, apply a sample type, settle the account with a percentage discount, then a `+` surcharge, then a refund; the balance and the receipt are correct at each step | 01, 03, 04, 08 | The account rules and the two `Enter` presses |
| E2E-4 | Enter results for a simple test and for a profile; the flag, band, unit and comment reach the printed report; the report prints its titles, symbols and status flag | 06, 07, 08 | The result grid, the entry grid and the report |
| E2E-5 | Build a combined report with the sub-title and reprint options on; the sub-title prints and a reprinted test raises no confirmation | 08, 09 | The two combined-report options |
| E2E-6 | Deliver results: filter by name, status and department; settle the balance; print the delivery list; preview it; open the combined report, the medical history, the empty report and the envelope from the delivery screen | 05, 08, 09, 10, 13 | The delivery screen's blocks and actions |
| E2E-7 | Enter a culture with microscopy fields and a sensitivity classification; print the microbiology report with its ranges, zones and commercial names | 08, 11 | The culture screen and the microbiology report |
| E2E-8 | Print the label set: the container labels, the file barcode and the laboratory-identifier barcode, with the baseline's content; print a sticker twice; enable the vertical coding on the report, the receipt and the envelope | 08, 12 | The label set and the vertical coding |
| E2E-9 | Insert a patient's earlier results into a new result report **automatically**, build the history report for two patients, and print it with its matrix | 07, 08, 13 | The history group, and the defect the audit identified |
| E2E-10 | Follow up a case: the samples block lists the tubes and their tests, and a sample is taken; the last printer shows a date and time | 12, 14 | The case-history screen |
| E2E-11 | Start the application, choose the data source, sign in with the remembered user name, and see the previous sign-in and the data source's label in the status bar | 15 | The shell and the sign-in |
| E2E-12 | Open the application with **no network** and complete E2E-1 through E2E-11 | all | The offline-first constraint |

### H.5 What must not be tested, and why

- **The excluded elements are not tested.** There is no test asserting that an SMS is sent, that a portal exists, that a branch selector is present, or that a result is uploaded. Writing such a test would assert the presence of excluded functionality. The **absence** of the excluded paths is instead asserted in the static checks of Section H.6.
- **No test connects to a real laboratory database.** The persistence tests use ephemeral containers only.
- **No test requires Internet access.** The export formats, the label printing and the file-attached database all work locally; if any of them needed the network, the offline-first constraint would be violated and the stage would fail its gate.

### H.6 Static checks that enforce the exclusions and the offline constraint

These are cheap, automated and run on every stage's commit. They are the mechanical proof that the plan's boundary rules hold.

| Check | Assertion |
|---|---|
| S-1 | No source file references an SMS, e-mail or fax **send** path (a `Twilio`, `SmtpClient`, fax, or equivalent). |
| S-2 | No source file contains a portal URL, a portal account type, a portal sign-in, or an upload command. |
| S-3 | No source file contains a **branch selector** in the search screen, and `SystemSettings.BranchNumber` is not read outside the branch-deployment code that already exists. |
| S-4 | No analyzer connectivity: no analyser/analyser device protocol, no HL7/ASTM/LIS listener, no automated result import. |
| S-5 | No `HttpClient`, `WebRequest`, `Socket` or `HttpWebRequest` exists anywhere under `src/`. |
| S-6 | `Directory.Packages.props` gains no package without an explicit, recorded decision. In particular no second EF Core provider. |
| S-7 | The 21 existing migration files are unmodified: `git diff --name-only` over `src/TopLab.Infrastructure/Persistence/Migrations` lists only the **new** files. |
| S-8 | No migration contains `DropColumn`, `DropTable`, `RenameColumn` or `AlterColumn` that retypes or drops an existing column. The one `AlterColumn` in the plan (STAGE-11's nullability) is on a column the plan states explicitly. |
| S-9 | No password, hash or secret is written to the workstation configuration file by the remember-login or print-result-password features. |

---

## I. Risks, Unresolved Decisions and Verification Tasks

### I.1 Verification tasks (must be closed, not decided by assumption)

| ID | Task | Owner | Blocks | How it is closed |
|---|---|---|---|---|
| `VER-01` | Reconcile the 5 elements whose class differs between the baseline audit's summary line and its per-element rows, all inside group 1.15 | Implementing agent + audit author | STAGE-13 sign-off | Re-derive the 36 unpersisted group-1.15 identifiers from the code and record the class per identifier. The stage boundaries do not change either way. |
| `VER-02` | Establish the applied state of the 21 existing migrations on the target database | Implementing agent, with the database owner | STAGE-01 | `dotnet ef migrations list` against the target database; record which are applied and which are pending. The plan assumes all 21 are applied. |
| `VER-03` | Inspect the **live** seeded values of `MedicalConditionType`, `SystemSettings`, `ReportSettings`, `ReportItemTemplate`, `PrinterAssignment`, and confirm the deployment's SQL Server edition and its ability to host a file-attached database | Implementing agent + database owner | STAGE-01, STAGE-15 | Read the live rows; record them in the stage handoff. For the file database, confirm the attachment mechanism, or record Outcome B. |
| `VER-04` | Confirm the 17 open behavioural decisions listed in Section C.3 | Laboratory owner | The acceptance of each affected element (not the stage gate) | A decision record per item. Until then the documented default stands and the element is delivered. |

### I.2 Material risks

| # | Risk | Likelihood | Impact | Mitigation, and the stage that carries it |
|---|---|---|---|---|
| R-1 | The 5 disputed group-1.15 elements are re-classed in a way that moves work between dispositions | Medium | Low | No stage boundary depends on them; `VER-01` closes it before STAGE-13 sign-off |
| R-2 | The patient-level laboratory identifier **changes the identity model**: the identifier becomes a patient key rather than a visit key, and `CreatePatientCommandHandler`'s duplicate rejection must be relaxed | High | High | STAGE-05 W5.1 states the exact rule; the migration is explicitly **not** a unique index on `LabId`; the existing `GetPatientByLabIdQueryHandler` already resolves several visits per identifier, so the model was built for this |
| R-3 | The `Critical` flag is required by the baseline but has **no threshold source** in the model | High | Medium | STAGE-06 adds explicit critical bounds to the band and the frozen snapshot; a result beyond them is `Critical`. Until an owner supplies bounds, no result is flagged `Critical` — the state exists and is correct, it is simply never raised. This is stated rather than invented |
| R-4 | Making the medical-history catalogue data-driven (eleven new rows) changes what the registration screen shows for **every** existing user | Certain | Medium | STAGE-01 states the eleven rows explicitly; the pre-existing pregnancy row is untouched, so the block grows and nothing is rewritten. Acceptance A3 and A7 cover it |
| R-5 | The 21-column report-settings addition is the largest single schema change and touches the document that every other stage prints | Medium | High | STAGE-08 requires a **golden-output** comparison: a default report must render identically to the pre-stage rendering (H.11) |
| R-6 | The refund representation could silently change every existing balance | Medium | High | The refund is a **separate nullable column**, never a negative `Amount`; M-T8 asserts that a refund-free patient's balance is bit-identical |
| R-7 | The "delete transaction" requirement collides with keeping a financial ledger | Medium | High | STAGE-04 deletes the row from the **account view** and records the deletion in the case audit trail; the ledger row is voided or archived, never destroyed (F.4) |
| R-8 | The `CultureAntibioticResults.SensitivityCategory` nullability tightening cannot be made deterministic for every row | Medium | High | The plan forbids fabricating a clinical classification: unclassifiable rows are **listed**, and if no deterministic backfill exists the column stays nullable and the invariant is enforced in the domain and the validator instead. The decision is recorded in the stage handoff (STAGE-11) |
| R-9 | The P and T columns have no established meaning, and a wrong guess would be printed on clinical reports | Medium | Medium | VERIFY #11 and #12 with a documented default; the columns are narrow, derived and free of clinical consequence. The decision is recorded before STAGE-06 sign-off |
| R-10 | The file-database option is read as a demand for a second database technology, which would break the one-schema rule and add a package | Medium | High | STAGE-15 7.7 makes Outcome C explicitly **out of scope** and forbids starting it; the startup check S-6 asserts no second EF provider is introduced |
| R-11 | The "backup" source of the search is read as "restore a `.bak` at query time", which is destructive and needs elevated rights | Medium | High | VERIFY #15's default is a separately configured secondary database; restoring on demand is rejected in the plan (STAGE-05 W5.10) |
| R-12 | The identification card's content is never shown, so the implemented card may not match what the laboratory expects | Certain | Low | VERIFY #5 with a documented default; the card is a single screen action and is cheap to re-cut |
| R-13 | The eleven stage boundaries in this plan are not the ones the previous session proposed, so a reader comparing the two may think work was lost | Certain | Low | Section D.3 records the three grouping decisions that differ and why; the identifiers and dispositions are identical, and the matrix proves every identifier is assigned exactly once |
| R-14 | The printed report changes in several stages (06, 07, 08, 11, 12, 13), so a regression in one could surface much later | Medium | Medium | Each printing stage carries a golden-output comparison; the comparison is run at the stage, not at the end |
| R-15 | A stage lands against a database that is not at the expected migration head | Medium | High | `VER-02` is closed before STAGE-01 and the migration procedure in F.5 records the head; each migration is applied incrementally over the previous head (M-T2), which is the path a live database takes |

### I.3 Decisions deliberately taken, with their reasons

| Decision | Reason | Where it is reversible |
|---|---|---|
| The laboratory identifier becomes a **patient-level** key | Required by 1.11.1.3 and 1.11.2.1; the existing lab-identifier lookup already returns several visits per identifier | STAGE-05, behind a domain rule — not a schema change |
| The print-result password is a **system** setting, not a per-patient one | Printing is a station-wide act; the baseline places the field on the patient screen, but a per-patient print password has no operational meaning | STAGE-01; the alternative is a column on `Patient` |
| The identity-block sample type is a **read-only summary** of the visit's test sample types | The baseline explicitly leaves the relation unestablished; a read-only summary cannot contradict the per-test control | STAGE-01, with VERIFY #3 |
| The P and T columns are **derived** indicators, not stored facts | No established meaning; derived columns cannot hold a wrong clinical value | STAGE-06, with VERIFY #11 and #12 |
| `ResultFlag.Critical` and `ProfileResultFlag.Normal` are **appended**, never renumbered | Both enums are stored as `int`; renumbering would silently reclassify every stored flag | Never — this is a hard rule (F.3) |
| The two "print" options are fixed in the **settings view model** rather than the command | The command already accepts the values (`UpdateReportSettingsCommandHandler.cs:30`); the view model sends `false, false` (`ReportSettingsViewModel.cs:195-211`). The defect is one layer, and fixing it there is smaller | STAGE-08 |
| The "backup" source is a **configured secondary database**, not an on-demand restore | An on-demand restore is destructive and needs elevated rights; the configured copy is read-only and safe | STAGE-05, with VERIFY #15 |
| The file-database option is a **file-attached instance of the same engine** | One schema, one migration chain, no new package, and it stays offline | STAGE-15, with `VER-03` |
| The Word and image export formats are produced **locally** | The offline-first constraint forbids a service, and a new package is avoidable | STAGE-08 |
| The history selections are **session state**, not a persisted template | The baseline describes a composition for a report, not a saved artefact | STAGE-13 |
| The empty report's entered content is **transient** | The baseline requires it to be entered and printed, not stored | STAGE-09 |
| The three TM / TG / CG code sets share **one** decision | The baseline uses the same three codes in two places; two decisions on one vocabulary invites contradiction | STAGE-03, STAGE-05, with VERIFY #1 and #4 |

### I.4 Explicitly out of scope for this unit

Recorded so that nothing in this list is later mistaken for an omission.

1. **The external patient-results portal** and every identifier of it (Section C.2).
2. **Equipment inventory, maintenance and calibration** — no identifier of this unit; no stage introduces it.
3. **Analyzer connectivity and automated result import** — no identifier of this unit; no stage introduces it. Manual result entry and specimen handling are in scope and are scheduled.
4. **Multi-branch support** — the two branch identifiers are excluded; the existing branch artefacts stay inert and are not extended, surfaced or removed.
5. **Result delivery by SMS, e-mail or fax** — the six identifiers are excluded; no stage introduces a send path.
6. **The second unit's functional scope** (the work-sheet module and everything beyond it). This plan covers the first functional unit only; the work-sheet action in 1.2.2.6.11 is preserved, not re-implemented.
7. **A second database technology** (Outcome C of STAGE-15) and any new package.
8. **Persisted history templates, persisted empty-report documents and any other stored composition** — none is required by the baseline.

---

## J. Final Checklist

### J.1 Plan integrity

- [ ] The plan names **15 stages** in a fixed order, each with a unique identifier (STAGE-01 … STAGE-15) and a stated position in the sequence.
- [ ] Every stage has all eleven subsections (7.1 – 7.11) and exactly one migration decision from the four permitted values.
- [ ] The traceability matrix contains **491 rows** covering all 491 baseline identifiers, with no omission, no duplication and no invented identifier.
- [ ] Every in-scope identifier has exactly one owning stage; no two stages implement the same behaviour.
- [ ] The 32 excluded identifiers are marked `EXCLUDED`, carry no stage, no acceptance criterion and no migration, and are not reported as gaps.
- [ ] The 146 preserved identifiers carry `PRESERVE`, appear in no work item, and are covered by regression assertions in the stages that touch their surface.
- [ ] The 20 uncertain identifiers carry `VERIFY`, a documented default, a decision owner, and a conditional path.
- [ ] The 17 open decisions are recorded in one decision log, not scattered across stages.

### J.2 Scope and exclusion compliance

- [ ] The five exclusions are applied element by element, and the 24 / 2 / 6 split is reproduced by the matrix.
- [ ] No stage implements, schedules or lists as an enhancement: a portal, an equipment register, analyzer connectivity, multi-branch support, or SMS / e-mail / fax delivery.
- [ ] The local capabilities adjacent to the exclusions — printing, local PDF export, local result viewing, manual result entry, specimen and sample workflows — are **in scope** and scheduled.
- [ ] No stage proposes a replacement portal, a "lite" portal, or any partial variant of an excluded capability.

### J.3 Offline-first compliance

- [ ] No stage introduces a remote API, an online service, a cloud dependency, a web call, or a runtime Internet requirement.
- [ ] The static check S-5 (`HttpClient`, `WebRequest`, `Socket`, `HttpWebRequest` absent from `src/`) passes on every commit.
- [ ] The static check S-6 (no new package without a recorded decision, and no second EF provider) passes on every commit.
- [ ] E2E-12 completes the whole unit with no network.

### J.4 Migration discipline

- [ ] `VER-02` and `VER-03` are closed before the first migration is authored.
- [ ] The seven new migrations are appended in the order of Section F.1, after `20261018144852_AddProfileResultStyling`.
- [ ] The 21 existing migration files are **unmodified** (static check S-7).
- [ ] Every migration is additive; no column is dropped, renamed or retyped (static check S-8).
- [ ] `ResultFlag` and `ProfileResultFlag` are extended by **appending**, never renumbered.
- [ ] Every new column is nullable or carries a behaviour-preserving default on pre-existing rows.
- [ ] Seed data is append-only; the existing pregnancy condition row and every seeded settings row are untouched.
- [ ] The one nullability tightening (STAGE-11) is applied only after a deterministic backfill, and unclassifiable rows are listed rather than assigned a fabricated clinical value.
- [ ] Every migration passes M-T1 through M-T10 and has a tested rollback.

### J.5 Verification readiness

- [ ] `dotnet build TopLab.sln -c Release` and `dotnet test TopLab.sln -c Release` produce a recorded baseline before the first change.
- [ ] All twelve E2E scenarios pass, each capturing a named acceptance criterion.
- [ ] The three documents that already render produce an identical layout before and after the stages that touch them.
- [ ] The row counts of the eight counted tables are unchanged by every migration, and the balance of every refund-free patient is unchanged.
- [ ] The static checks S-1 through S-9 pass.
- [ ] No password, hash or secret is written to the workstation configuration file.

### J.6 Execution readiness

- [ ] Each stage's completion gate was met before the next stage began; no stage was started on an unmet dependency.
- [ ] The decision log records all 17 open decisions, the `VER-01` reconciliation, and the `VER-03` file-database outcome.
- [ ] The stage handoff for each stage records: the baseline measurement, the migration applied, the acceptance criteria met, and the decision records written.
- [ ] No file outside `Docs/Audit/Stages of implementing the first unit.md` was modified by the **planning** of this plan.

---

*End of plan. 15 stages, 491 baseline identifiers accounted for, 32 excluded under the five owner-approved exclusions, 7 additive migrations, 12 end-to-end scenarios, 17 open decisions, 4 verification tasks.*

> **Reconciliation note.** The baseline audit's own summary line states A 146 / B 108 / C 61 / D 176. The per-element rows, which are the evidence, give A 146 / B 107 / C 66 / D 172 for the 455 persisted rows plus the 36 unpersisted group-1.15 rows. The **A total of 146 agrees exactly**; the B/C/D split differs by a net movement of one element between B, C and D. The matrix below uses the **per-element** classes, because the per-element rows are the evidence and the aggregate line is the summary of it. `VER-01` closes the difference; it changes no stage boundary, because the five affected identifiers all sit inside STAGE-13.

### G.3 Matrix

| ID | Element | Class | Disposition | Stage | Acceptance | Migration |
|---|---|---|---|---|---|---|
| 1.1.1 | Top Ribbon | B | CORRECT | STAGE-15 | O1, O2 | — |
| 1.1.1.1 | Ribbon Item List | B | COMPLETE | STAGE-15 | O1, O9 | — |
| 1.1.1.1.1 | Exit Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.2 | About Us Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.3 | Send Result Item | D | IMPLEMENT | STAGE-15 | O1 | — |
| 1.1.1.1.4 | Setting Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.5 | System Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.6 | Users Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.7 | Statistics Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.8 | Accounts Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.9 | Tools Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.10 | Work Sheet Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.11 | Laboratory Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.1.12 | Patients Item | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.1.2 | Active Module Indicator | D | IMPLEMENT | STAGE-15 | O2 | — |
| 1.1.2 | Status Bar | A | PRESERVE | STAGE-15 | O3, O4, O9 | — |
| 1.1.2.1 | User Name Indicator | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.2.2 | Last Login Indicator | C | CORRECT | STAGE-15 | O3 | — |
| 1.1.2.3 | Database Label Indicator | C | CORRECT | STAGE-15 | O4 | — |
| 1.1.2.4 | Today Indicator | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.3 | Startup Splash Screen | C | CORRECT | STAGE-15 | O5 | — |
| 1.1.3.1 | Data Source Option Group | D | IMPLEMENT | STAGE-15 | O6 | CONDITIONAL |
| 1.1.3.1.1 | File Database Option | D | IMPLEMENT +V | STAGE-15 | O7 | CONDITIONAL |
| 1.1.3.1.2 | Client Server Database Option | B | COMPLETE | STAGE-15 | O6 | — |
| 1.1.3.2 | Startup Progress Indicator | D | IMPLEMENT | STAGE-15 | O5 | — |
| 1.1.4 | Sign-In Window | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.4.1 | Sign-In User Name Field | A | PRESERVE | STAGE-15 | O8, O9 | — |
| 1.1.4.2 | Sign-In Password Field | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.4.3 | Show Password Characters Option | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.4.4 | Remember Login Option | D | IMPLEMENT | STAGE-15 | O8 | — |
| 1.1.4.5 | Application Sign In Action | A | PRESERVE | STAGE-15 | O9 | — |
| 1.1.5 | Patient Search Toolbar | D | IMPLEMENT | STAGE-05 | E16 | — |
| 1.1.5.1 | Quick Search Field | D | IMPLEMENT | STAGE-05 | E16 | — |
| 1.1.5.2 | Quick Search Mode Options | D | IMPLEMENT +V | STAGE-05 | E16 | — |
| 1.2.1 | Main Patients Window | B | COMPLETE | STAGE-05 | E13, E16 | — |
| 1.2.1.1 | Add Patient Action | A | PRESERVE | STAGE-05 | E15 | — |
| 1.2.1.2 | Enter Results Action | A | PRESERVE | STAGE-05 | E13 | — |
| 1.2.1.3 | Find Patient Action | A | PRESERVE | STAGE-05 | E13 | — |
| 1.2.1.4 | Period List Action | B | COMPLETE | STAGE-05 | E13 | — |
| 1.2.1.5 | Unfinished Results View | B | COMPLETE | STAGE-06 | F1 | — |
| 1.2.1.6 | Unverified Results View | B | COMPLETE | STAGE-06 | F1 | — |
| 1.2.1.7 | Unprinted Results View | B | COMPLETE | STAGE-06 | F1 | — |
| 1.2.1.8 | Deliver Results Action | A | PRESERVE | STAGE-10 | J1 | — |
| 1.2.1.9 | Settle Accounts Action | B | COMPLETE | STAGE-10 | J7 | — |
| 1.2.1.10 | Undelivered Results Count | B | COMPLETE | STAGE-10 | J4 | — |
| 1.2.1.11 | Test Registration Statistics Action | B | COMPLETE +V | STAGE-05 | E13 | — |
| 1.2.2 | Add-and-Edit Patient Data Screen | B | COMPLETE | STAGE-02 | B1 | — |
| 1.2.2.1 | Patient Header Block | B | COMPLETE | STAGE-01 | A2 | NEW MIGRATION |
| 1.2.2.1.1 | Entry Date Field | C | CORRECT | STAGE-01 | A2 | NEW MIGRATION |
| 1.2.2.1.2 | Receipt Date Field | D | IMPLEMENT | STAGE-01 | A2 | NEW MIGRATION |
| 1.2.2.1.3 | Patient Code Field | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.1.4 | VIP Action | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.1.5 | Laboratory ID Action | B | CORRECT | STAGE-02 | B7 | — |
| 1.2.2.2 | Identity Block | B | COMPLETE | STAGE-01 | A1 | NEW MIGRATION |
| 1.2.2.2.1 | Patient Name Field | B | COMPLETE | STAGE-01 | A1 | NEW MIGRATION |
| 1.2.2.2.2 | Sex Radio Group | B | COMPLETE | STAGE-01 | A1 | NEW MIGRATION |
| 1.2.2.2.2.1 | Male Option | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.2.2.2 | Female Option | B | COMPLETE | STAGE-01 | A1 | NEW MIGRATION |
| 1.2.2.2.3 | Age Value Field | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.2.4 | Age Unit Selector | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.2.5 | Mobile and Telephone Field | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.2.6 | Card Number Field | D | IMPLEMENT | STAGE-01 | A1 | NEW MIGRATION |
| 1.2.2.2.7 | Account Type Selector | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.2.8 | Sample Type Field | C | CORRECT +V | STAGE-01 | A9 | NEW MIGRATION |
| 1.2.2.2.9 | Patient Address Field | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.2.10 | Referring Entity Selector | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.2.10.1 | Referring Entity Additional Information Action | D | IMPLEMENT | STAGE-01 | A1 | NEW MIGRATION |
| 1.2.2.2.11 | Treating Doctor Selector | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.2.11.1 | Treating Doctor Additional Information Action | D | IMPLEMENT | STAGE-01 | A1 | NEW MIGRATION |
| 1.2.2.3 | Medical History Block | B | COMPLETE | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.1 | Fasting Toggle | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.3.2 | Fasting Hours Field | A | PRESERVE | STAGE-01 | A7 | NEW MIGRATION |
| 1.2.2.3.3 | Diabetes Treatment Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.4 | Blood-Thinning Treatment Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.5 | Gland Treatment Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.6 | Antiviral Treatment Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.7 | Antibiotic Treatment Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.8 | Liver Treatment Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.9 | Pregnancy Toggle | B | CORRECT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.10 | Imaging Within Two Days Toggle | B | CORRECT | STAGE-01 | A4 | NEW MIGRATION |
| 1.2.2.3.11 | Anaemia Condition Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.12 | Joint Inflammation Condition Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.13 | High Blood Pressure Condition Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.14 | Renal Failure Condition Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.3.15 | Lupus Condition Toggle | D | IMPLEMENT | STAGE-01 | A3 | NEW MIGRATION |
| 1.2.2.4 | Notes and Communication Block | B | COMPLETE | STAGE-01 | A5 | NEW MIGRATION |
| 1.2.2.4.1 | Notes Field | B | COMPLETE | STAGE-01 | A8 | NEW MIGRATION |
| 1.2.2.4.2 | Print Result Password Field | D | IMPLEMENT | STAGE-01 | A5 | NEW MIGRATION |
| 1.2.2.4.2.1 | Print Result Password Enable Option | D | IMPLEMENT | STAGE-01 | A5 | NEW MIGRATION |
| 1.2.2.4.3 | Send SMS Field | D | **EXCLUDED** (Excl. 5) | — | — | — |
| 1.2.2.4.4 | Send E-mail Field | D | **EXCLUDED** (Excl. 5) | — | — | — |
| 1.2.2.4.5 | Fax Field | D | **EXCLUDED** (Excl. 5) | — | — | — |
| 1.2.2.4.6 | Send Action | D | **EXCLUDED** (Excl. 5) | — | — | — |
| 1.2.2.4.7 | Site Upload Action | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.2.2.4.8 | Site Reference Field | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.2.2.5 | Test Selection Block | B | COMPLETE | STAGE-03 | C1–C7 | — |
| 1.2.2.5.1 | Test Filter Bar | D | IMPLEMENT | STAGE-03 | C1 | — |
| 1.2.2.5.1.1 | Test Filter Field | D | IMPLEMENT | STAGE-03 | C1 | — |
| 1.2.2.5.1.2 | Test Filter Mode Options | D | IMPLEMENT +V | STAGE-03 | C2 | — |
| 1.2.2.5.1.3 | Complete Test Menu Action | B | CORRECT | STAGE-03 | C5 | — |
| 1.2.2.5.1.4 | Test Group Menu Action | B | CORRECT | STAGE-03 | C5 | — |
| 1.2.2.5.2 | Master Test List | A | PRESERVE | STAGE-03 | C1 | — |
| 1.2.2.5.3 | Add Select Test Action | A | PRESERVE | STAGE-03 | C7 | — |
| 1.2.2.5.4 | Delete Test Action | A | PRESERVE | STAGE-03 | C7 | — |
| 1.2.2.5.5 | All Tests Action | A | PRESERVE | STAGE-03 | C7 | — |
| 1.2.2.5.6 | Patient's Tests List | A | PRESERVE | STAGE-03 | C4, C7 | — |
| 1.2.2.5.6.1 | Patient's Tests Count | D | IMPLEMENT | STAGE-03 | C4 | — |
| 1.2.2.5.7 | Test Group List | B | COMPLETE | STAGE-03 | C3 | — |
| 1.2.2.5.8 | Add All Tests In Group Action | A | PRESERVE | STAGE-03 | C3 | — |
| 1.2.2.5.9 | Group Send E-mail Action | D | **EXCLUDED** (Excl. 5) | — | — | — |
| 1.2.2.5.10 | Sample Type Radio Group | B | CORRECT | STAGE-03 | C6 | — |
| 1.2.2.5.10.1 | Urine Option | A | PRESERVE | STAGE-03 | C6 | — |
| 1.2.2.5.10.2 | Stool Option | A | PRESERVE | STAGE-03 | C6 | — |
| 1.2.2.5.10.3 | Blood Option | A | PRESERVE | STAGE-03 | C6 | — |
| 1.2.2.5.10.4 | Semen Option | A | PRESERVE | STAGE-03 | C6 | — |
| 1.2.2.5.10.5 | CSF Option | A | PRESERVE | STAGE-03 | C6 | — |
| 1.2.2.5.11 | Taken Outside Lab Flag | A | PRESERVE | STAGE-03 | C7 | — |
| 1.2.2.6 | Action Band | B | COMPLETE | STAGE-02 | B1 | — |
| 1.2.2.6.1 | Save Registration Action | A | PRESERVE | STAGE-02 | B1 | — |
| 1.2.2.6.2 | Add Entry Action | B | CORRECT | STAGE-02 | B2 | — |
| 1.2.2.6.3 | Edit Entry Action | D | IMPLEMENT | STAGE-02 | B3 | — |
| 1.2.2.6.4 | Undo Action | A | PRESERVE | STAGE-02 | B1 | — |
| 1.2.2.6.5 | Print Barcode Action | B | COMPLETE | STAGE-12 | L8 | NEW MIGRATION |
| 1.2.2.6.6 | Print Receipt Action | A | PRESERVE | STAGE-02 | B1 | — |
| 1.2.2.6.7 | Patient Invoice Action | A | PRESERVE | STAGE-02 | B1 | — |
| 1.2.2.6.8 | Patient Envelope Print Action | C | CORRECT | STAGE-02 | B4 | — |
| 1.2.2.6.9 | Modify Action | D | IMPLEMENT | STAGE-02 | B3 | — |
| 1.2.2.6.10 | Follow-up Action | D | IMPLEMENT | STAGE-02 | B1 | — |
| 1.2.2.6.11 | Work Sheet Action | A | PRESERVE | STAGE-02 | B1 | — |
| 1.2.2.6.12 | ID Card Action | D | IMPLEMENT +V | STAGE-02 | B5 | — |
| 1.2.2.6.13 | Patient Screen Action | D | IMPLEMENT | STAGE-02 | B6 | — |
| 1.2.2.6.14 | Main Point Action | D | IMPLEMENT | STAGE-02 | B6 | — |
| 1.2.2.6.15 | Patient Window Action | D | IMPLEMENT | STAGE-02 | B6 | — |
| 1.2.2.6.16 | Lab Results Action | D | IMPLEMENT | STAGE-02 | B6 | — |
| 1.3.1 | Account Panel | A | PRESERVE | STAGE-04 | D11 | — |
| 1.3.1.1 | Total of Tests Field | A | PRESERVE | STAGE-04 | D4 | — |
| 1.3.1.2 | Discount Value Field | A | PRESERVE | STAGE-04 | D1 | — |
| 1.3.1.3 | Discount Percentage Field | C | CORRECT | STAGE-04 | D1 | — |
| 1.3.1.4 | Paid Amount Field | A | PRESERVE | STAGE-04 | D2, D3 | — |
| 1.3.1.4.1 | Surcharge Entry Convention | C | CORRECT | STAGE-04 | D2 | — |
| 1.3.1.4.2 | Enter Key Account Operation | D | IMPLEMENT | STAGE-04 | D3 | — |
| 1.3.1.5 | Paid Previously Field | B | COMPLETE | STAGE-04 | D4 | — |
| 1.3.1.6 | Amount After Discount Field | D | IMPLEMENT | STAGE-04 | D4 | — |
| 1.3.1.7 | Total Collected Field | B | COMPLETE | STAGE-04 | D4 | — |
| 1.3.1.8 | Remainder for Customer Field | B | COMPLETE | STAGE-04 | D5 | — |
| 1.3.1.9 | Remainder for Laboratory Field | C | CORRECT | STAGE-04 | D5 | — |
| 1.3.1.10 | Confirm Payment Action | B | COMPLETE | STAGE-04 | D6 | — |
| 1.3.1.11 | Net Settlement Action | C | CORRECT | STAGE-04 | D7 | — |
| 1.3.1.12 | Account Summary Action | A | PRESERVE | STAGE-04 | D11 | — |
| 1.3.1.13 | Custody Action | D | IMPLEMENT +V | STAGE-04 | D8 | — |
| 1.3.1.14 | Account Back Action | C | CORRECT | STAGE-04 | D9 | — |
| 1.3.2 | Patient Account Screen | A | PRESERVE | STAGE-04 | D11 | — |
| 1.3.2.1 | Account Transaction Grid | A | PRESERVE | STAGE-04 | D10, D11 | — |
| 1.3.2.1.1 | Paid Date Column | A | PRESERVE | STAGE-04 | D10 | — |
| 1.3.2.1.2 | Money Column | A | PRESERVE | STAGE-04 | D10 | — |
| 1.3.2.1.3 | User Name Column | A | PRESERVE | STAGE-04 | D10 | — |
| 1.3.2.1.4 | Edit Date Column | D | IMPLEMENT | STAGE-04 | D10 | — |
| 1.3.2.1.5 | Color Code Column | D | IMPLEMENT +V | STAGE-04 | D10 | — |
| 1.3.2.2 | Modify Amount Action | C | CORRECT | STAGE-04 | D11 | NEW MIGRATION |
| 1.3.2.3 | Delete Transaction Action | C | CORRECT | STAGE-04 | D11 | NEW MIGRATION |
| 1.3.2.4 | Pay Action | A | PRESERVE | STAGE-04 | D6 | NEW MIGRATION |
| 1.3.2.5 | Refund Action | D | IMPLEMENT | STAGE-04 | D11 | NEW MIGRATION |
| 1.3.2.6 | Add Amount Action | A | PRESERVE | STAGE-04 | D2 | NEW MIGRATION |
| 1.3.2.7 | Clear Account Action | D | IMPLEMENT | STAGE-04 | D11 | NEW MIGRATION |
| 1.3.2.8 | Account Patient List Action | B | CORRECT | STAGE-04 | D9 | NEW MIGRATION |
| 1.4.1 | Deliver Patient Results Screen | B | COMPLETE | STAGE-10 | J1 | — |
| 1.4.1.1 | Delivery Filter Block | B | COMPLETE | STAGE-10 | J2 | — |
| 1.4.1.1.1 | Delivery Date Filter | A | PRESERVE | STAGE-10 | J2 | — |
| 1.4.1.1.2 | Delivery Name Filter | D | IMPLEMENT | STAGE-10 | J2 | — |
| 1.4.1.1.3 | Delivery Status Filter | D | IMPLEMENT | STAGE-10 | J2 | — |
| 1.4.1.1.4 | Delivery Department Filter | D | IMPLEMENT +V | STAGE-10 | J2 | — |
| 1.4.1.2 | Patient Scope Block | D | IMPLEMENT | STAGE-10 | J3 | — |
| 1.4.1.2.1 | Patient Source Selector | D | IMPLEMENT | STAGE-10 | J3 | — |
| 1.4.1.2.2 | Patient Group Selector | D | IMPLEMENT | STAGE-10 | J3 | — |
| 1.4.1.2.3 | Patients Today Panel | D | IMPLEMENT | STAGE-10 | J4 | — |
| 1.4.1.2.3.1 | Delivery Patient Grid | C | CORRECT | STAGE-10 | J5 | — |
| 1.4.1.3 | Delivery Result Block | A | PRESERVE | STAGE-10 | J6 | — |
| 1.4.1.3.1 | Delivery Result List Grid | B | COMPLETE | STAGE-10 | J6 | — |
| 1.4.1.3.1.1 | Test Abbreviation Column | A | PRESERVE | STAGE-10 | J6 | — |
| 1.4.1.3.1.2 | P Column | D | IMPLEMENT +V | STAGE-10 | J6 | — |
| 1.4.1.3.1.3 | T Column | D | IMPLEMENT +V | STAGE-10 | J6 | — |
| 1.4.1.3.1.4 | Result Column | A | PRESERVE | STAGE-10 | J6 | — |
| 1.4.1.3.1.5 | Status Column | D | IMPLEMENT | STAGE-10 | J6 | — |
| 1.4.1.3.1.6 | Delivery Finish Column | C | CORRECT | STAGE-10 | J6 | NEW MIGRATION |
| 1.4.1.3.1.7 | Delivery Verify Column | A | PRESERVE | STAGE-10 | J6 | — |
| 1.4.1.3.1.8 | Delivery Print Column | A | PRESERVE | STAGE-10 | J6 | — |
| 1.4.1.3.1.9 | Delivery Export Column | D | IMPLEMENT | STAGE-10 | J6 | NEW MIGRATION |
| 1.4.1.3.1.10 | See Report Action | D | IMPLEMENT | STAGE-10 | J6 | — |
| 1.4.1.4 | Delivery Action Block | B | COMPLETE | STAGE-10 | J10 | — |
| 1.4.1.4.1 | Patient Account Action | B | CORRECT | STAGE-10 | J7 | — |
| 1.4.1.4.2 | Refresh Action | A | PRESERVE | STAGE-10 | J1 | — |
| 1.4.1.4.3 | Delivery List Print Action | C | CORRECT | STAGE-10 | J8 | — |
| 1.4.1.4.4 | Preview Action | D | IMPLEMENT | STAGE-10 | J9 | — |
| 1.4.1.4.5 | Copy Action | D | IMPLEMENT | STAGE-10 | J10 | — |
| 1.4.1.4.6 | Notes Action | D | IMPLEMENT | STAGE-10 | J10 | — |
| 1.4.1.4.7 | Delivery Patient Data Action | D | IMPLEMENT | STAGE-10 | J10 | — |
| 1.4.1.4.8 | Combined Report Action | D | IMPLEMENT | STAGE-10 | J10 | — |
| 1.4.1.4.9 | Patient Medical History Action | D | IMPLEMENT | STAGE-10 | J10 | — |
| 1.4.1.4.10 | Delivery Empty Report Action | D | IMPLEMENT | STAGE-10 | J10 | — |
| 1.4.1.4.11 | Delivery Envelope Print Action | D | IMPLEMENT | STAGE-10 | J10 | — |
| 1.4.1.4.12 | Send SMS Action | D | **EXCLUDED** (Excl. 5) | — | — | — |
| 1.4.1.4.13 | Delivery Main List Action | A | PRESERVE | STAGE-10 | J1 | — |
| 1.5.1 | Modification Path One | B | COMPLETE | STAGE-05 | E15 | — |
| 1.5.1.1 | Patient Icon Action | A | PRESERVE | STAGE-05 | E15 | — |
| 1.5.1.2 | Patient List Open Action | B | CORRECT | STAGE-05 | E15 | — |
| 1.5.1.3 | Registration Day Selector | D | IMPLEMENT | STAGE-05 | E15 | — |
| 1.5.1.4 | Edit Patient Action | A | PRESERVE | STAGE-05 | E15 | — |
| 1.5.1.5 | Patient Selection Action | A | PRESERVE | STAGE-05 | E15 | — |
| 1.5.1.6 | Modification Save Action | A | PRESERVE | STAGE-05 | E15 | — |
| 1.5.2 | Modification Path Two | B | CORRECT | STAGE-05 | E15 | — |
| 1.5.2.1 | Search Action | A | PRESERVE | STAGE-05 | E15 | — |
| 1.5.2.2 | Patient Data Entry Action | B | CORRECT | STAGE-05 | E15 | — |
| 1.5.3 | Patient Data Access Action | D | IMPLEMENT | STAGE-05 | E16 (command owned here; placed on the result-entry screen in STAGE-06, F12) | — |
| 1.6.1 | Result Entry Screen | A | PRESERVE | STAGE-06 | F1 | NEW MIGRATION |
| 1.6.1.1 | Patient List Panel | C | CORRECT | STAGE-06 | F1 | NEW MIGRATION |
| 1.6.1.1.1 | Result Entry Patient Grid | C | CORRECT | STAGE-06 | F1 | NEW MIGRATION |
| 1.6.1.1.2 | Patient Name Click Action | D | IMPLEMENT | STAGE-06 | F1 | NEW MIGRATION |
| 1.6.1.1.3 | Patient Count Indicator | C | CORRECT | STAGE-06 | F1 | NEW MIGRATION |
| 1.6.1.2 | Test Results Grid | B | COMPLETE | STAGE-06 | F2 | NEW MIGRATION |
| 1.6.1.2.1 | Result Entry Test Abbreviation Column | A | PRESERVE | STAGE-06 | F2 | NEW MIGRATION |
| 1.6.1.2.2 | Result Entry P Column | D | IMPLEMENT +V | STAGE-06 | F3 | NEW MIGRATION |
| 1.6.1.2.3 | Result Entry T Column | D | IMPLEMENT +V | STAGE-06 | F3 | NEW MIGRATION |
| 1.6.1.2.4 | Result Entry Result Column | A | PRESERVE | STAGE-06 | F2 | NEW MIGRATION |
| 1.6.1.2.5 | Result Entry Status Column | D | IMPLEMENT | STAGE-06 | F4 | NEW MIGRATION |
| 1.6.1.2.6 | Abnormal Flag Column | B | CORRECT +V | STAGE-06 | F5 | NEW MIGRATION |
| 1.6.1.2.7 | Low and High Column | D | IMPLEMENT | STAGE-06 | F6 | NEW MIGRATION |
| 1.6.1.2.8 | Unit Column | D | IMPLEMENT | STAGE-06 | F7 | NEW MIGRATION |
| 1.6.1.2.9 | Normal Range Column | D | IMPLEMENT | STAGE-06 | F6 | NEW MIGRATION |
| 1.6.1.2.10 | Finish Flag | D | IMPLEMENT | STAGE-06 | F8 | NEW MIGRATION |
| 1.6.1.2.11 | Verify Flag | B | COMPLETE | STAGE-06 | F9 | NEW MIGRATION |
| 1.6.1.2.12 | Result Print Flag | B | COMPLETE | STAGE-06 | F9 | NEW MIGRATION |
| 1.6.1.2.13 | Export Flag | B | COMPLETE | STAGE-06 | F9 | NEW MIGRATION |
| 1.6.1.2.14 | Per-Test Comment Field | B | COMPLETE | STAGE-06 | F10 | NEW MIGRATION |
| 1.6.1.2.15 | Double-Click Test Action | A | PRESERVE | STAGE-06 | F1 | NEW MIGRATION |
| 1.6.1.3 | Entry Action Block | B | COMPLETE | STAGE-06 | F11 | NEW MIGRATION |
| 1.6.1.3.1 | Save Results Action | B | COMPLETE | STAGE-06 | F11 | NEW MIGRATION |
| 1.6.1.3.2 | Print Preview Action | B | COMPLETE | STAGE-06 | F11 | NEW MIGRATION |
| 1.6.1.3.3 | Result Report Print Action | B | COMPLETE | STAGE-06 | F11, F12 | NEW MIGRATION |
| 1.6.1.4 | Report Settings Screen | B | COMPLETE | STAGE-08 | H1 | NEW MIGRATION |
| 1.6.1.4.1 | Report Title Block | C | CORRECT | STAGE-08 | H2 | NEW MIGRATION |
| 1.6.1.4.1.1 | Report Screen Title Field | D | IMPLEMENT | STAGE-08 | H2 | NEW MIGRATION |
| 1.6.1.4.1.2 | Report Main Title Field | D | IMPLEMENT | STAGE-08 | H2 | NEW MIGRATION |
| 1.6.1.4.1.3 | Report Sub Title Field | C | CORRECT | STAGE-08 | H2 | NEW MIGRATION |
| 1.6.1.4.1.4 | Report Sub Sub-title Field | D | IMPLEMENT | STAGE-08 | H2 | NEW MIGRATION |
| 1.6.1.4.1.5 | Reference Range Field | B | COMPLETE | STAGE-08 | H3 | NEW MIGRATION |
| 1.6.1.4.1.6 | Report Comment Field | B | COMPLETE | STAGE-08 | H3 | NEW MIGRATION |
| 1.6.1.4.1.7 | Report Normal Range Field | B | COMPLETE | STAGE-08 | H3 | NEW MIGRATION |
| 1.6.1.4.1.8 | Main Symbol Field | C | CORRECT | STAGE-08 | H4 | NEW MIGRATION |
| 1.6.1.4.1.9 | Sub Symbol Field | C | CORRECT | STAGE-08 | H4 | NEW MIGRATION |
| 1.6.1.4.1.10 | Sub-title Symbol Field | C | CORRECT | STAGE-08 | H4 | NEW MIGRATION |
| 1.6.1.4.2 | Report Display Option Block | B | COMPLETE | STAGE-08 | H1, H5 | NEW MIGRATION |
| 1.6.1.4.2.1 | Show Abnormal Background Option | A | PRESERVE | STAGE-08 | H11 | NEW MIGRATION |
| 1.6.1.4.2.2 | Print Status Flag Option | D | IMPLEMENT | STAGE-08 | H5 | NEW MIGRATION |
| 1.6.1.4.2.3 | Print Patient History Automatically Option | B | CORRECT | STAGE-08 | H6 | NEW MIGRATION |
| 1.6.1.4.3 | Report Typography Block | C | CORRECT | STAGE-08 | H7 | NEW MIGRATION |
| 1.6.1.4.3.1 | Symbol Font Selectors | D | IMPLEMENT | STAGE-08 | H7 | NEW MIGRATION |
| 1.6.1.4.3.2 | Vertical Spacing Fields | C | CORRECT | STAGE-08 | H7 | NEW MIGRATION |
| 1.6.1.4.3.3 | Normal Result Font Selector | B | COMPLETE | STAGE-08 | H7 | NEW MIGRATION |
| 1.6.1.4.3.4 | Text Alignment Control | D | IMPLEMENT | STAGE-08 | H7 | NEW MIGRATION |
| 1.6.1.4.3.5 | Cell Expansion Control | D | IMPLEMENT | STAGE-08 | H7 | NEW MIGRATION |
| 1.6.1.4.3.6 | Offset Amount Control | A | PRESERVE | STAGE-08 | H11 | NEW MIGRATION |
| 1.6.1.4.4 | Report Output Block | C | CORRECT | STAGE-08 | H1, H9 | NEW MIGRATION |
| 1.6.1.4.4.1 | Printing Target Selector | B | COMPLETE | STAGE-08 | H8 | NEW MIGRATION |
| 1.6.1.4.4.2 | Save Settings Action | A | PRESERVE | STAGE-08 | H10 | NEW MIGRATION |
| 1.6.1.4.4.3 | Export Data Action | C | CORRECT | STAGE-08 | H9 | NEW MIGRATION |
| 1.6.1.4.4.4 | Close Action | B | CORRECT | STAGE-08 | H10 | NEW MIGRATION |
| 1.6.1.5 | Report Entry Grid | B | COMPLETE | STAGE-07 | G1 | NEW MIGRATION |
| 1.6.1.5.1 | Report Entry Test Name Column | A | PRESERVE | STAGE-07 | G1 | NEW MIGRATION |
| 1.6.1.5.2 | Report Entry Result Column | A | PRESERVE | STAGE-07 | G1 | NEW MIGRATION |
| 1.6.1.5.3 | Report Entry Unit Column | A | PRESERVE | STAGE-07 | G1 | NEW MIGRATION |
| 1.6.1.5.4 | Report Entry Flag Column | C | CORRECT +V | STAGE-07 | G2 | NEW MIGRATION |
| 1.6.1.5.5 | Report Entry Normal Ranges Column | C | CORRECT | STAGE-07 | G3 | NEW MIGRATION |
| 1.6.1.5.6 | Report Entry Verified Flag | B | COMPLETE | STAGE-07 | G4 | NEW MIGRATION |
| 1.6.1.5.7 | Report Entry Print Flag | B | COMPLETE | STAGE-07 | G5 | NEW MIGRATION |
| 1.6.1.5.8 | Report Entry Comment Field | B | COMPLETE | STAGE-07 | G6 | NEW MIGRATION |
| 1.6.1.5.9 | Urine Sample Sub-Row Panel | D | IMPLEMENT | STAGE-07 | G7 | NEW MIGRATION |
| 1.6.1.5.9.1 | Urine Sample Sub-Row | D | IMPLEMENT | STAGE-07 | G7 | NEW MIGRATION |
| 1.6.1.5.9.2 | Sub-Result Row | D | IMPLEMENT | STAGE-07 | G7 | NEW MIGRATION |
| 1.6.1.5.9.3 | Missing Sample Marker | D | IMPLEMENT | STAGE-07 | G7 | NEW MIGRATION |
| 1.6.1.6 | Test Entry Variants | B | COMPLETE | STAGE-07 | G8–G11 | NEW MIGRATION |
| 1.6.1.6.1 | Single Test Entry | B | COMPLETE | STAGE-07 | G1 | NEW MIGRATION |
| 1.6.1.6.2 | Profile Test Entry | B | COMPLETE | STAGE-07 | G1 | NEW MIGRATION |
| 1.6.1.6.3 | Paired Control And Patient Entry | D | IMPLEMENT | STAGE-07 | G8 | NEW MIGRATION |
| 1.6.1.6.4 | Derived Value Entry | D | IMPLEMENT | STAGE-07 | G9 | NEW MIGRATION |
| 1.6.1.6.5 | Multi-Unit Entry | D | IMPLEMENT | STAGE-07 | G10 | NEW MIGRATION |
| 1.6.1.6.6 | Qualitative Result Entry | B | COMPLETE | STAGE-07 | G11 | NEW MIGRATION |
| 1.6.1.7 | Report Screen Actions | B | COMPLETE | STAGE-07 | G12 | — |
| 1.6.1.7.1 | Save Report Action | A | PRESERVE | STAGE-07 | G12 | — |
| 1.6.1.7.2 | Report Screen Print Action | B | COMPLETE | STAGE-07 | G12 | — |
| 1.6.1.7.3 | Report Screen Preview Action | B | COMPLETE | STAGE-07 | G12 | — |
| 1.6.1.7.4 | Report Patient History Action | D | IMPLEMENT | STAGE-13 | M14 | — |
| 1.6.1.7.5 | Report Screen Back Action | D | IMPLEMENT | STAGE-07 | G12 | — |
| 1.6.1.7.6 | Report Screen Main List Action | D | IMPLEMENT | STAGE-07 | G12 | — |
| 1.7.1 | Combined Report Screen | A | PRESERVE | STAGE-09 | I1, I7 | — |
| 1.7.1.1 | Patient Tests In Our Lab List | A | PRESERVE | STAGE-09 | I1 | — |
| 1.7.1.2 | Available Tests In Report List | B | COMPLETE | STAGE-09 | I1 | — |
| 1.7.1.3 | Add Test To Report Action | B | CORRECT | STAGE-09 | I2 | — |
| 1.7.1.4 | Reorder Report Tests Action | A | PRESERVE | STAGE-09 | I2 | — |
| 1.7.1.4.1 | Move Test Up Action | A | PRESERVE | STAGE-09 | I2 | — |
| 1.7.1.4.2 | Move Test Down Action | A | PRESERVE | STAGE-09 | I2 | — |
| 1.7.1.5 | Combined Report Option Block | D | IMPLEMENT | STAGE-09 | I3 | — |
| 1.7.1.5.1 | Print Sub Title Option | B | COMPLETE | STAGE-09 | I3 | — |
| 1.7.1.5.2 | Print Printed Test Again Option | B | COMPLETE | STAGE-09 | I3, I4 | — |
| 1.8.1 | Empty Report Screen | A | PRESERVE | STAGE-09 | I7 | — |
| 1.8.1.1 | Empty Report Open Action | B | CORRECT | STAGE-09 | I5 | — |
| 1.8.1.2 | Enter Data And Print Option | D | IMPLEMENT | STAGE-09 | I6 | — |
| 1.8.1.3 | Patient Data Sheet Option | A | PRESERVE | STAGE-09 | I7 | — |
| 1.9.1 | Culture Screen | A | PRESERVE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.1 | Culture Data Block | A | PRESERVE | STAGE-11 | K9 | NEW MIGRATION |
| 1.9.1.1.1 | Sample Field | A | PRESERVE | STAGE-11 | K9 | NEW MIGRATION |
| 1.9.1.1.2 | Organism A Field | A | PRESERVE | STAGE-11 | K9 | NEW MIGRATION |
| 1.9.1.1.3 | Organism B Field | A | PRESERVE | STAGE-11 | K9 | NEW MIGRATION |
| 1.9.1.1.4 | Organism C Field | A | PRESERVE | STAGE-11 | K9 | NEW MIGRATION |
| 1.9.1.1.5 | Culture Condition Field | A | PRESERVE | STAGE-11 | K9 | NEW MIGRATION |
| 1.9.1.1.6 | Colony Count Field | A | PRESERVE | STAGE-11 | K9 | NEW MIGRATION |
| 1.9.1.2 | Microscopy Block | B | COMPLETE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.1 | Reaction Field | D | IMPLEMENT | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.2 | Pus Cells Field | A | PRESERVE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.3 | Red Blood Cells Field | A | PRESERVE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.4 | Epithelial Cells Field | A | PRESERVE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.5 | Crystal Field | A | PRESERVE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.6 | Fungi Field | A | PRESERVE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.7 | Bacteria Field | D | IMPLEMENT | STAGE-11 | K2 | NEW MIGRATION |
| 1.9.1.2.8 | Additional Microscopy Fields | A | PRESERVE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.9 | Direct Culture Fields | B | COMPLETE | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.2.10 | Microscopy Complete Flag | D | IMPLEMENT | STAGE-11 | K1 | NEW MIGRATION |
| 1.9.1.3 | Culture Flag Block | B | COMPLETE | STAGE-11 | K3 | NEW MIGRATION |
| 1.9.1.3.1 | Culture Verified Flag | A | PRESERVE | STAGE-11 | K3 | NEW MIGRATION |
| 1.9.1.3.2 | Culture Print Flag | B | COMPLETE | STAGE-11 | K3 | NEW MIGRATION |
| 1.9.1.4 | Sensitivity Block | B | COMPLETE | STAGE-11 | K4 | NEW MIGRATION |
| 1.9.1.4.1 | Antibiotic List | A | PRESERVE | STAGE-11 | K4 | NEW MIGRATION |
| 1.9.1.4.2 | Antibiotic Add Action | B | CORRECT | STAGE-11 | K4 | NEW MIGRATION |
| 1.9.1.4.3 | Sensitivity Classification | B | CORRECT | STAGE-11 | K5 | NEW MIGRATION |
| 1.9.1.4.3.1 | Highly For Option | A | PRESERVE | STAGE-11 | K5 | NEW MIGRATION |
| 1.9.1.4.3.2 | Moderate For Option | A | PRESERVE | STAGE-11 | K5 | NEW MIGRATION |
| 1.9.1.4.3.3 | Low For Option | A | PRESERVE | STAGE-11 | K5 | NEW MIGRATION |
| 1.9.1.4.3.4 | Resistant For Option | A | PRESERVE | STAGE-11 | K5 | NEW MIGRATION |
| 1.9.1.4.4 | Report Element Visibility Control | B | CORRECT | STAGE-11 | K6 | NEW MIGRATION |
| 1.9.1.5 | Culture Action Block | B | COMPLETE | STAGE-11 | K7 | NEW MIGRATION |
| 1.9.1.5.1 | Sensitivity Entry Action | D | IMPLEMENT | STAGE-11 | K7 | NEW MIGRATION |
| 1.9.1.5.2 | Save Culture Data Action | A | PRESERVE | STAGE-11 | K7 | NEW MIGRATION |
| 1.9.1.6 | Microbiology Report Output | B | COMPLETE | STAGE-11 | K8 | NEW MIGRATION |
| 1.10.1 | Search Screen | A | PRESERVE | STAGE-05 | E13 | — |
| 1.10.1.1 | Search Criteria Block | A | PRESERVE | STAGE-05 | E3–E8 | — |
| 1.10.1.1.1 | Search by Patient Name Exact | D | IMPLEMENT | STAGE-05 | E3 | — |
| 1.10.1.1.2 | Search by Patient Name Partial | C | CORRECT | STAGE-05 | E3 | — |
| 1.10.1.1.3 | Search by Treating Doctor | A | PRESERVE | STAGE-05 | E13 | — |
| 1.10.1.1.4 | Search by Request Date | A | PRESERVE | STAGE-05 | E13 | — |
| 1.10.1.1.5 | Search by Test Within Period | C | CORRECT | STAGE-05 | E4 | — |
| 1.10.1.1.6 | Search by Phone Number | C | CORRECT | STAGE-05 | E5 | — |
| 1.10.1.1.7 | Search by Card Number | D | IMPLEMENT | STAGE-05 | E6 | — |
| 1.10.1.1.8 | Search by Patient Age | A | PRESERVE | STAGE-05 | E13 | — |
| 1.10.1.1.9 | Search by Patient Gender | A | PRESERVE | STAGE-05 | E13 | — |
| 1.10.1.1.10 | Search Within Branch | D | **EXCLUDED** (Excl. 4) | — | — | — |
| 1.10.1.1.11 | Search for Results Not Entered | B | COMPLETE | STAGE-05 | E7 | — |
| 1.10.1.1.12 | Search for Results Not Verified | B | COMPLETE | STAGE-05 | E7 | — |
| 1.10.1.1.13 | Search for Results Not Printed | B | COMPLETE | STAGE-05 | E7 | — |
| 1.10.1.1.14 | Search for Results Not Delivered | B | COMPLETE | STAGE-05 | E7 | — |
| 1.10.1.1.15 | Date Range Mode Options | D | IMPLEMENT | STAGE-05 | E8 | — |
| 1.10.1.2 | Search Scope Block | D | IMPLEMENT | STAGE-05 | E9 | — |
| 1.10.1.2.1 | Branch Selector | D | **EXCLUDED** (Excl. 4) | — | — | — |
| 1.10.1.2.2 | Source Selector | D | IMPLEMENT +V | STAGE-05 | E9 | — |
| 1.10.1.3 | Search Result Block | A | PRESERVE | STAGE-05 | E10 | — |
| 1.10.1.3.1 | Search Result Grid | C | CORRECT | STAGE-05 | E10 | — |
| 1.10.1.3.2 | Search Result Count | D | IMPLEMENT | STAGE-05 | E10 | — |
| 1.10.1.3.3 | New Search Action | B | CORRECT | STAGE-05 | E11 | — |
| 1.10.1.3.4 | Enter ID Field | C | CORRECT | STAGE-05 | E12 | — |
| 1.10.1.4 | Search Action Bar | D | IMPLEMENT | STAGE-05 | E13 | — |
| 1.10.1.4.1 | Patient Tests Action | B | COMPLETE | STAGE-05 | E13 | — |
| 1.10.1.4.2 | Search Patient Data Action | B | COMPLETE | STAGE-05 | E15 | — |
| 1.10.1.4.3 | Search Lab Results Action | B | COMPLETE | STAGE-05 | E13 | — |
| 1.10.1.4.4 | Delete Patient Action | B | COMPLETE | STAGE-05 | E13 | — |
| 1.10.1.4.5 | Open Account Action | B | COMPLETE | STAGE-05 | E13 | — |
| 1.10.1.4.6 | Restrict To VIP Action | D | IMPLEMENT +V | STAGE-05 | E13 | — |
| 1.10.1.4.7 | Add Action | D | IMPLEMENT | STAGE-05 | E13 (action bar); consumed by STAGE-13 multi-patient history (M10) | — |
| 1.10.1.4.8 | Collected Results Action | D | IMPLEMENT +V | STAGE-05 | E13 | — |
| 1.10.1.4.9 | Modify Group Action | D | IMPLEMENT +V | STAGE-05 | E13 | — |
| 1.10.1.4.10 | Search Main List Action | B | CORRECT | STAGE-05 | E13 | — |
| 1.11.1 | Laboratory ID Lifecycle | A | PRESERVE | STAGE-05 | E1 | — |
| 1.11.1.1 | Laboratory ID Generation | A | PRESERVE | STAGE-05 | E1 | — |
| 1.11.1.2 | Laboratory ID Storage | A | PRESERVE | STAGE-05 | E1 | — |
| 1.11.1.3 | Laboratory ID Display at Next Registration | D | IMPLEMENT | STAGE-05 | E1 | — |
| 1.11.2 | Visit History | A | PRESERVE | STAGE-05 | E2, E14 | — |
| 1.11.2.1 | Search by Laboratory ID | A | PRESERVE | STAGE-05 | E2 | — |
| 1.11.2.2 | Visit List Grid | C | CORRECT | STAGE-05 | E14 | — |
| 1.11.2.3 | Visit Tests List | A | PRESERVE | STAGE-05 | E14 | — |
| 1.12.1 | Case History Screen | A | PRESERVE | STAGE-14 | N5 | — |
| 1.12.1.1 | Registration Audit Entry | A | PRESERVE | STAGE-14 | N5 | — |
| 1.12.1.2 | Modification Count | A | PRESERVE | STAGE-14 | N5 | — |
| 1.12.1.3 | Last Modifier Entry | A | PRESERVE | STAGE-14 | N5 | — |
| 1.12.1.4 | Result Entry Audit Entry | A | PRESERVE | STAGE-14 | N5 | — |
| 1.12.1.5 | Verification Audit Entry | A | PRESERVE | STAGE-14 | N5 | — |
| 1.12.1.6 | Printing Count | A | PRESERVE | STAGE-14 | N5 | — |
| 1.12.1.7 | Last Printer Entry | B | COMPLETE | STAGE-14 | N1 | — |
| 1.12.1.8 | Delivery Audit Entry | A | PRESERVE | STAGE-14 | N5 | — |
| 1.12.1.9 | Patient Samples Block | D | IMPLEMENT | STAGE-14 | N2 | — |
| 1.12.1.9.1 | Tube Name Column | D | IMPLEMENT | STAGE-14 | N3 | — |
| 1.12.1.9.2 | Sample Tests Column | C | CORRECT | STAGE-14 | N3 | — |
| 1.12.1.9.3 | Sample Take Action | B | CORRECT | STAGE-14 | N4 | — |
| 1.13.1 | Upload Screen | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.1.1 | Upload Patient Tests List | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.1.2 | Upload Action | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.1.3 | Select All Action | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.1.4 | Unselect All Action | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.1.5 | Upload Progress Indicator | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.1.6 | Block Result Action | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.1.7 | Mobile Number Requirement | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.2 | Uploaded Patient Count | C | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.3 | Site Access Channels | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.4 | Site Account Types | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.4.1 | Patient Account Type | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.4.2 | Doctor Account Type | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.4.3 | Laboratory Account Type | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.5 | Site Sign-In Block | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.5.1 | Patient Sign-In Data | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.5.2 | Doctor Or Laboratory Sign-In Data | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.5.3 | Site Sign In Action | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.6 | Site Result View | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.6.1 | Result View Action | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.13.6.2 | Result Export Formats | C | CORRECT | STAGE-08 | H9 | NEW MIGRATION |
| 1.13.6.2.1 | Print Format | B | COMPLETE | STAGE-08 | H9 | NEW MIGRATION |
| 1.13.6.2.2 | Word Document Format | D | IMPLEMENT | STAGE-08 | H9 | NEW MIGRATION |
| 1.13.6.2.3 | PDF Format | A | PRESERVE | STAGE-08 | H9 | NEW MIGRATION |
| 1.13.6.2.4 | Image Format | D | IMPLEMENT | STAGE-08 | H9 | NEW MIGRATION |
| 1.13.7 | Site Access Data On Receipt | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.14.1 | Barcode Label Kinds | C | COMPLETE | STAGE-12 | L4, L5 | NEW MIGRATION |
| 1.14.1.1 | Specimen Container Label | D | IMPLEMENT | STAGE-12 | L4 | NEW MIGRATION |
| 1.14.1.2 | File Barcode Label | C | CORRECT | STAGE-12 | L5 | NEW MIGRATION |
| 1.14.1.3 | Laboratory ID Barcode Label | C | CORRECT | STAGE-12 | L5 | NEW MIGRATION |
| 1.14.1.4 | Site Access Label | D | **EXCLUDED** (Excl. 1) | — | — | — |
| 1.14.2 | Specimen Container Label Content | D | IMPLEMENT | STAGE-12 | L6 | NEW MIGRATION |
| 1.14.2.1 | Patient Name on Label | D | IMPLEMENT | STAGE-12 | L6 | NEW MIGRATION |
| 1.14.2.2 | Sex and Age Line on Label | D | IMPLEMENT | STAGE-12 | L6 | NEW MIGRATION |
| 1.14.2.3 | Allocated Test List on Label | D | IMPLEMENT | STAGE-12 | L6 | NEW MIGRATION |
| 1.14.2.4 | Label Barcode | A | PRESERVE | STAGE-12 | L6 | NEW MIGRATION |
| 1.14.2.5 | Laboratory Identifier on Label | C | CORRECT | STAGE-12 | L6 | NEW MIGRATION |
| 1.14.2.6 | Container Designation on Label | D | IMPLEMENT | STAGE-12 | L6 | NEW MIGRATION |
| 1.14.2.7 | File Barcode Number | C | CORRECT +V | STAGE-12 | L7 | NEW MIGRATION |
| 1.14.2.8 | Label Print Timestamp | C | CORRECT | STAGE-12 | L6 | NEW MIGRATION |
| 1.14.3 | Specimen Container Designations | D | IMPLEMENT | STAGE-12 | L1 | NEW MIGRATION |
| 1.14.3.1 | Serum Fasting Container | D | IMPLEMENT | STAGE-12 | L1 | NEW MIGRATION |
| 1.14.3.2 | Serum Postprandial Container | D | IMPLEMENT | STAGE-12 | L1 | NEW MIGRATION |
| 1.14.3.3 | EDTA Blood Container | D | IMPLEMENT | STAGE-12 | L1 | NEW MIGRATION |
| 1.14.3.4 | Citrated Blood Container | D | IMPLEMENT | STAGE-12 | L1 | NEW MIGRATION |
| 1.14.3.5 | Urine Container | D | IMPLEMENT | STAGE-12 | L1 | NEW MIGRATION |
| 1.14.3.6 | Stool Container | D | IMPLEMENT | STAGE-12 | L1 | NEW MIGRATION |
| 1.14.3.7 | Semen Container | D | IMPLEMENT | STAGE-12 | L1 | NEW MIGRATION |
| 1.14.4 | Test Distribution Across Containers | D | IMPLEMENT | STAGE-12 | L1, L2 | NEW MIGRATION |
| 1.14.4.1 | Automatic Test Sorting | C | CORRECT | STAGE-12 | L2 | NEW MIGRATION |
| 1.14.4.2 | Container Distribution Edit | D | IMPLEMENT | STAGE-12 | L3 | NEW MIGRATION |
| 1.14.4.3 | Vertical Coding on Printed Documents | D | IMPLEMENT | STAGE-12 | L9 | NEW MIGRATION |
| 1.14.4.4 | Repeated Sticker Printing | C | CORRECT | STAGE-12 | L10 | NEW MIGRATION |
| 1.15.1 | Automatic History Insertion | C | CORRECT | STAGE-13 | M1, M3 | — |
| 1.15.1.1 | Automatic Previous Result Insertion | C | CORRECT | STAGE-13 | M1, M3 | — |
| 1.15.1.2 | Multi-Test History Insertion | C | CORRECT | STAGE-13 | M2, M3 | — |
| 1.15.1.3 | History Patient Identification | A | PRESERVE | STAGE-13 | M13 | — |
| 1.15.1.3.1 | Identify By Patient Name | A | PRESERVE | STAGE-13 | M13 | — |
| 1.15.1.3.2 | Identify By Laboratory Code | A | PRESERVE | STAGE-13 | M13 | — |
| 1.15.1.4 | Automatic History Display Setting | A | PRESERVE | STAGE-13 | M13 | — |
| 1.15.1.5 | History Sorting Method Setting | A | PRESERVE | STAGE-13 | M13 | — |
| 1.15.2 | History Test Selection | C | CORRECT | STAGE-13 | M4 | — |
| 1.15.2.1 | Previous Visit Test List | B | COMPLETE | STAGE-13 | M4 | — |
| 1.15.2.2 | Previous Test Selection Action | C | CORRECT | STAGE-13 | M4 | — |
| 1.15.2.3 | Previous Visit Count Indicator | D | IMPLEMENT | STAGE-13 | M5 | — |
| 1.15.2.4 | Repeated Test Note | D | IMPLEMENT | STAGE-13 | M6 | — |
| 1.15.2.5 | Previous Result Grid | C | CORRECT | STAGE-13 | M4 | — |
| 1.15.3 | History Output Mode | C | CORRECT | STAGE-13 | M7 | — |
| 1.15.3.1 | Attach History In Result Report Option | C | CORRECT | STAGE-13 | M7 | — |
| 1.15.3.2 | Print History In Separate Report Option | C | CORRECT | STAGE-13 | M7, M13 | — |
| 1.15.3.3 | History Mode Confirm Action | A | PRESERVE | STAGE-13 | M7 | — |
| 1.15.3.4 | History Mode Cancel Action | B | COMPLETE | STAGE-13 | M7 | — |
| 1.15.4 | Patient History Report | C | CORRECT | STAGE-13 | M9 | — |
| 1.15.4.1 | History Report Header | C | CORRECT | STAGE-13 | M8 | — |
| 1.15.4.1.1 | History Report Title | A | PRESERVE | STAGE-13 | M8 | — |
| 1.15.4.1.2 | History Date Range Field | C | CORRECT | STAGE-13 | M8 | — |
| 1.15.4.1.3 | History Printed By Entry | D | IMPLEMENT | STAGE-13 | M8 | — |
| 1.15.4.1.4 | History Printed In Entry | C | CORRECT | STAGE-13 | M8 | — |
| 1.15.4.2 | History Report Grid | C | CORRECT | STAGE-13 | M9 | — |
| 1.15.4.2.1 | History Analysis Date Column | B | COMPLETE | STAGE-13 | M9 | — |
| 1.15.4.2.2 | History Patient ID Column | C | CORRECT | STAGE-13 | M9 | — |
| 1.15.4.2.3 | History Patient Name Column | C | CORRECT | STAGE-13 | M9 | — |
| 1.15.4.2.4 | History Test Result Column | C | CORRECT | STAGE-13 | M9 | — |
| 1.15.4.3 | Multi-Patient History Panel | C | CORRECT | STAGE-13 | M10 | — |
| 1.15.4.3.1 | Multi-Patient History Grid | D | IMPLEMENT | STAGE-13 | M10 | — |
| 1.15.4.3.2 | Multi-Patient History Count | D | IMPLEMENT | STAGE-13 | M10 | — |
| 1.15.4.4 | History Test Lists | D | IMPLEMENT | STAGE-13 | M11 | — |
| 1.15.4.4.1 | History Tests In Lab List | D | IMPLEMENT | STAGE-13 | M11 | — |
| 1.15.4.4.2 | History Tests Available In Report List | D | IMPLEMENT | STAGE-13 | M11 | — |
| 1.15.4.5 | Delete All History Action | D | IMPLEMENT | STAGE-13 | M12 | — |
| 1.15.4.6 | History Report Print Action | C | CORRECT | STAGE-13 | M13 | — |
| 1.15.4.7 | History Report Preview Action | D | IMPLEMENT | STAGE-13 | M13 | — |



