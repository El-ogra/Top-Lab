---
title: "Plan for completing the implementation of the project"
date: 2026-10-17
---

# Plan for completing the implementation of the project

## Document header

- Date of preparation: 17 October 2026.
- Environment used: Windows 11, .NET SDK 8.0.425 with .NET SDK 9.0.318 also installed, Windows Desktop runtime 8.0.31, Python 3.13 with pypdf and PyMuPDF 1.28.2 for reference-page handling, `dotnet-ef` 8.0.30, LocalDB instance control (`sqllocaldb`) for availability probing only.
- Prior-audit results recorded in this plan (not rerun during this document review): the original audit reports successful individual builds of all four source projects and five test projects with zero warnings/errors, and 2788 passed, 0 failed, 2 skipped tests out of
  2790. It also reports 15 ordered migration classes, with latest `20261008135847_AddAntibioticCommercialName`, and a source-level investigation of the application. These are inherited claims from the original audit, not fresh build/test results produced while revising this document.
- Reference evidence recorded by the prior audit: 152 distinct in-scope functions, 21 excluded functions, two authoritative PDFs listed as 212 and 77 pages, 55 pages reportedly inspected visually at 150 DPI, with the remaining pages covered through text extraction.
- What this document review independently checked: the 22-phase structure, arithmetic of the gap and migration totals, consistency of phase dependencies and owner-decision status, and selected claims against the supplied read-only response files. This merged revision preserves the prior audit's verification context, 25-item gap register, Tier 1/Tier 2 coverage summary, phase-by-phase evidence, migration handoff protocol, risk register, and self-audit checklist; it also applies the plan-level corrections recorded below. The reported build/test totals remain inherited audit results, not fresh execution results from this document review.
- What was not independently verified during this document review: build/test execution; runtime behaviour of the graphical interface; visual correctness of printed output on paper or a physical printer; and Arabic glyph shaping beyond existing tests. The owner reports that, after reopening the IDE, running `Get-Migration -Project TopLab.Infrastructure -StartupProject TopLab.Presentation` showed all 15 existing migrations, from `BaselineDataModel` through `20261008135847_AddAntibioticCommercialName`, with `applied = True`. This is owner-reported evidence and was not independently re-run by this document review. New migrations must be handed off as artefacts only; the owner applies them and confirms their status as set out in Section 5.1.
- Build limitations: a single parallel full-solution build was attempted and did not complete (a build-node failure under parallel load); every project was then built individually in dependency order, and each build succeeded with zero warnings and zero errors.
- Test limitations: two tests in `TopLab.Persistence.Tests` skip cleanly (both skip because a container runtime is not installed on the verification host: the relational migration-chain integration test and the concurrent-settlement test); no test project was excluded for safety reasons because every database-touching test uses an in-memory provider with a generated database name, model-only metadata reads that open no connection, or an ephemeral container that skips when its runtime is absent.
- Overall confidence limitations: classifications of IMPLEMENTED rest on static traces plus passing tests, not on observed runtime behaviour; MISSING classifications rest on systematic negative searches, which cannot prove the absence of a capability that lives outside the repository; several reference pages were read through text extraction only; a small number of reference details are genuinely ambiguous and are recorded as resolved owner decisions rather than invented requirements.

------------------------------------------------------------------------

## SECTION 1 --- Verification Context

### 1.1 Date, environment and tools

Prepared 17 October 2026 on Windows 11. Toolchain: .NET SDK 8.0.425 (with 9.0.318 present), Windows Desktop runtime 8.0.31, `dotnet-ef` 8.0.30, Python 3.13 with pypdf and PyMuPDF for reference-page rendering at 150 DPI, `sqllocaldb` for availability probing, standard source-search tooling. No external websites, search engines or general domain knowledge were used to establish reference requirements.

### 1.2 Scope of the original audit (as reported)

The original audit report states that it covered: solution structure (4 source projects, 5 test projects, 15 ordered migrations, model snapshot file, WPF views and view models, printing pipeline, authorization behaviour); per-project builds; the complete test suite on the .NET 8 runtime; migration and persistence source inspection; a 141-point Tier 1 reachability sweep over implemented behaviour plus targeted Tier 2 deep traces for every flagged item and report-layout, barcode, permission, inventory-naming and monitor concerns; visual inspection of 55 reference pages at 150 DPI; and text-layer inspection of the remaining pages. The present revision did not rerun that full audit; it reviewed the plan and checked selected claims against the supplied response files.

### 1.3 Verification status summary (honest)

- Prior-audit build result (not rerun in this document review): PASS for all 9 projects individually (0 warnings, 0 errors each). A single parallel full-solution build attempt reportedly failed on build-node resources and is recorded as a limitation, not as a project failure.
- Prior-audit test result (not rerun in this document review): 2788 passed, 0 failed, 2 skipped (2790 total). The 2 skips occur because a container runtime is unavailable on the verification host (one requires Docker for the relational integration test; one is a live-connection concurrency test). No test project was excluded for safety reasons because every database-touching test uses an in-memory provider with a generated database name, model-only metadata reads that open no connection, or an ephemeral container that skips when its runtime is absent. All 28 printing tests pass on Windows; the font-dependent failures seen on other operating systems do not occur here.
- Reference inventory: 152 distinct in-scope functions inventoried from the two authoritative PDFs; 21 excluded functions inventoried separately. Two reference rows required special handling: a showcase-side restatement of the group-patient slip is merged into a single function, and a supposed year-end settlement entry has no reference evidence (the cited chapter covers yearly drawer accounting and sent-out settlement only) and is therefore not carried as a function.
- Classifications: IMPLEMENTED 127, DIFFERENT 11, MISSING 14. Every MISSING and DIFFERENT function has a gap-register entry and at least one implementation phase. BLOCKED / UNRESOLVED: 0. Open owner decisions: 0, all RESOLVED.
- Migration verification: the owner reports that, after reopening the IDE, `Get-Migration -Project TopLab.Infrastructure -StartupProject     TopLab.Presentation` showed all 15 existing migrations from `BaselineDataModel` through `20261008135847_AddAntibioticCommercialName` with `applied = True`. This is owner-reported evidence, not a result independently re-run by this document review. The automated migration-chain test still cannot run without a container runtime, so the full chain on an empty database is not proven by that test. New migrations follow Section 5.1: the implementing agent creates artefacts only; the owner applies each migration and confirms it with `Get-Migration`.
- IMPLEMENTED-function coverage: Tier 1 reachability checks covered every IMPLEMENTED function (141 verification points, 132 passing, 9 flagged); Tier 2 deep traces covered all 9 flagged points plus the permission, naming, monitor, deviation, settlement, appointment-note and vertical-coding concerns (18 deep investigations); functions covered by neither tier: 0.

------------------------------------------------------------------------

## SECTION 2 --- Scope and Exclusions

Five areas are permanently excluded from the required scope. They were applied as a hard boundary during inventory, classification, dependency analysis, phase design and migration decisions: no excluded function appears in the gap register, in any phase, in the migration summary, or as a recommendation.

1.  Online results portal and associated ecosystem. The online result website, portal receipt printing, doctor and laboratory portal accounts, Android applications, uploader utilities and portal administration are excluded. Where a reference window combines portal accounts with in-scope master data (treating doctors, sent-out laboratories, referring entities), only the master-data portion is carried; the portal-account portion is not planned.
2.  Equipment management. Equipment registration, maintenance records and calibration records are excluded, including the single system-window capability line that names all three together.
3.  Analyser integration. Blood/CBC analyser connections and result imports are excluded, including correction factors, hardware lists and interface settings. Manual entry of CBC results, CBC history matrices and statistics over results already stored in the laboratory database remain in scope.
4.  Multi-branch support. The application is single-branch only. Branch-number columns that exist in storage (`Users.BranchNumber`, `SystemSettings.BranchNumber` with a single-branch default) are dormant data with no branch-scoped behaviour and are left untouched; no phase introduces branch dimensions.
5.  External result delivery. Sending results by SMS, e-mail or fax is excluded, including laboratory-branded SMS notices. Contact-data columns (such as an e-mail address field on external entities) are dormant data, not delivery behaviour. Every reminder or notification in this document stays inside the application; no phase introduces an external delivery channel.

Enforcement evidence: the inventory keeps 21 excluded functions in a separate excluded population; the gap register contains zero excluded functions; every phase-end gate requires the implementing agent to confirm that no excluded functionality or external delivery channel was introduced; the migration summary contains no table or column serving an excluded purpose.

------------------------------------------------------------------------

## SECTION 3 --- Functional Gap Register

One entry per distinct in-scope MISSING or DIFFERENT function. IMPLEMENTED and OWNER-EXCLUDED functions are not listed here. Identifiers GAP-001...GAP-025 are stable within this file.

#### GAP-001

- **Function:** Verify / unverify simple single-value results
- **Classification:** DIFFERENT
- **Reference evidence:** reference guide, pages 28, 54 (review and reverse-review of entered results)
- **Current implementation evidence:** `ReviewResultCommandHandler.cs`, `UnreviewResultCommandHandler.cs` and `MarkAllPatientResultsReviewedCommandHandler.cs` exist under `src/TopLab.Application/Features/ResultsEntry/Commands/`; no sender in `src/TopLab.Presentation` (verified by exhaustive search; `SimpleResultEntryViewModel.cs` exposes only save, clear and comment-picker). Profile and culture verify paths are wired (`ProfileEntryViewModel.cs:303,340`, `CultureEntryViewModel.cs:404`).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Wire the existing simple-result review commands to the result-entry surface with permission checks, or implement the surface action through them, so entered simple results can be verified and unverified exactly like profile and culture results.
- **Dependencies:** None (uses existing commands, entities and permissions).
- **Open decision, if any:** OD-01 (single-print recording rule interacts with the review-before-print chain).

#### GAP-002

- **Function:** Control display of Sensitivity / Reference / Commercial Name columns in the culture report
- **Classification:** DIFFERENT
- **Reference evidence:** reference guide, page 39 (toggle which of the three columns appears)
- **Current implementation evidence:** Master data present (`Antibiotic.Symbol/ScientificName/CommercialName`, `AntibioticConfiguration.cs:19`, latest migration adds `CommercialName`); report grid fixed at four columns with no commercial-name column and no toggle (`ReportCultureSection.cs:113-124`, comment records no commercial-name column).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Persist three column-visibility flags and writer (shared with GAP-012). render the sensitivity grid with exactly the selected columns, including the commercial name when enabled.
- **Dependencies:** Culture report query and
- **Open decision, if any:** None.

#### GAP-003

- **Function:** Temporarily override a reference value at print time for one patient
- **Classification:** DIFFERENT
- **Reference evidence:** reference guide, pages 101-102 (open the reference cell from the report, edit, refresh, accept for that patient)
- **Current implementation evidence:** Per-patient frozen-range snapshot infrastructure exists (`ResultsEntryDtos.cs:24-52`, `ResultReferenceRangeSource.cs`); `RefreshResultReferenceRangeCommand` handler exists but has no sender in `src/TopLab.Presentation`.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Make the print-time refresh Result-entry snapshot model reachable from the report surface for a single patient test, re-resolving and re-freezing only that patient's snapshot.
- **Dependencies:** (exists).
- **Open decision, if any:** None.

#### GAP-004

- **Function:** Print the worksheet restricted to unfinished work, or complete
- **Classification:** DIFFERENT
- **Reference evidence:** reference guide, pages 86, 89 (unfinished-only switch on the worksheet)
- **Current implementation evidence:** Worksheet queries exist (`GetVisitWorkSheetQuery`, `GetWorkSheetByTestGroupQuery`, `GetWorkSheetByWorkGroupLogQuery`) with no unfinished-only parameter; print command is per-patient (`PrintWorkSheetCommand(PatientId)`); only the outside-laboratory exclusion exists, which is a different filter.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Add an unfinished-only option to the worksheet queries and surface it on the worksheet view as a switch between unfinished-only and complete output.
- **Dependencies:** Worksheet queries and writer (exist).
- **Open decision, if any:** None.

#### GAP-005

- **Function:** Report header/footer from attached image files
- **Classification:** DIFFERENT
- **Reference evidence:** reference guide, page 194 (header/footer modes: none, words, image `HeaderFooterMode.Images` is offered (`HeaderFooterMode.cs`, files)
- **Current implementation evidence:** settings view models) but `ReportContentBuilder.cs:32,68,138` branches only on `!= None`; receipt and envelope writers fall back to the words header with an explicit code comment; no image column, picker or upload path exists.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Persist header/footer image Shared with GAP-006, GAP-011, references, provide a file picker with validation, render the images in the report header/footer, and define the fallback when a file is missing.
- **Dependencies:** GAP-018 (same aggregate and writer region).
- **Open decision, if any:** None.

#### GAP-006

- **Function:** Report header/footer colours
- **Classification:** DIFFERENT
- **Reference evidence:** reference guide, pages 195-198 (colour of individual header/footer `ReportSettings.HeaderColor/FooterColor` declared and mapped elements)
- **Current implementation evidence:** (`ReportSettings.cs:21,23`, `ReportSettingsConfiguration.cs:20-21`, seeded null) but with no setter, no command field, no view-model property and no writer read (7 code hits total, one test asserting null).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Add mutators, command fields, settings-screen controls and writer application for both colours.
- **Dependencies:** Same aggregate and writer region as GAP-005 (implement together).
- **Open decision, if any:** None.

#### GAP-007

- **Function:** Receipt font type, size and colour plus laboratory logo
- **Classification:** DIFFERENT
- **Reference evidence:** reference guide, pages 202-204 (receipt header/footer font controls and logo)
- **Current implementation evidence:** `ReceiptSettings.cs` models margins, currency, pickup time, print-once, detail mode, cashier-printer flag and header/footer mode; no font family/size/colour and no logo reference; writer falls back to words for the Images mode (`ReceiptPdfWriter.cs:200-202`).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Persist receipt header/footer font family, size and colour plus a logo image reference; expose them on the receipt-settings screen; render them in the receipt writer.
- **Dependencies:** Image-asset pattern shared with GAP-005.
- **Open decision, if any:** None.

#### GAP-008

- **Function:** Patient receivable account summary with balance filter
- **Classification:** DIFFERENT
- **Reference evidence:** reference showcase, page 26 (period receivable summary filtered to `GetPatientSamplesDetailQuery(From, To)` returns per-patient unpaid, discounted or paid-only)
- **Current implementation evidence:** charged/paid/balance with no filter parameter; `PaymentOperation.OperationType` distinguishes payment, correction, extra charge and void, so the data needed for the filter already exists.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Add the three-way balance filter (unpaid-only, discounted-only, paid-only) to the patient-level account query and surface and its writer.
- **Dependencies:** Payment-operation model (exists).
- **Open decision, if any:** None.

#### GAP-009

- **Function:** Barcode labels per sample material with doctor-editable test distribution
- **Classification:** DIFFERENT
- **Reference evidence:** reference showcase, page 3 (tube/container/swab labels; program distributes tests across drawn materials; doctor may change the distribution; 38 mm x 25 mm stock; reprintable)
- **Current implementation evidence:** `IBarcodeService.PrintBarcodeAsync(string value)` renders one fixed-size Code-128 label for the patient or Lab Id (`BarcodeService.cs:44`, `BarcodeLabelRenderer.Render(content, width = 300, height = 80)`); no sample-material entity, no per-material assignment, no distribution UI. Identity-barcode printing itself works.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Introduce sample materials and per-test material assignment, a doctor-editable distribution surface, and multi-label printing; keep the existing single-identifier path working.
- **Dependencies:** GAP-021 (label size parameters None. should be configurable first).
- **Open decision, if any:** —

#### GAP-010

- **Function:** Receipt header/footer logo on pre-printed or plain receipt paper
- **Classification:** DIFFERENT
- **Reference evidence:** reference showcase, page 4 (receipt paper choice with logo branding)
- **Current implementation evidence:** Words half implemented (`ReceiptPdfWriter.cs:200-205`); logo half inert (Images mode falls back to words; no logo asset).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Render the persisted logo on the receipt for both paper choices.
- **Dependencies:** GAP-007 (reuses its logo column; no new schema).
- **Open decision, if any:** None.

#### GAP-011

- **Function:** Report on plain or pre-printed paper with a logo
- **Classification:** DIFFERENT
- **Reference evidence:** reference showcase, pages 5, 16 (paper choice and logo branding for reports)
- **Current implementation evidence:** Plain-paper path is the default and works; pre-printed choice approximated by words mode; logo half inert (`ReportContentBuilder.cs:32,68,138` never loads an image).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Render the persisted header/footer images on the columns; no new schema). report for both paper choices.
- **Dependencies:** GAP-005 (reuses its image
- **Open decision, if any:** None.

#### GAP-012

- **Function:** Select the antibiotic of choice for a culture
- **Classification:** MISSING
- **Reference evidence:** reference guide, page 39 (`Antibiotic of choice` on the culture report)
- **Current implementation evidence:** No choice field, command, query or view-model property exists (exhaustive search for choice markers returns only unrelated gender/age/font usages; `CultureAntibioticDisplay.cs` holds only the pregnancy/children filter).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Persist one antibiotic-of-choice selection per culture result, expose selection in culture entry, and mark it on the report.
- **Dependencies:** Culture entry grid and report section (shared with GAP-002).
- **Open decision, if any:** None.

#### GAP-013

- **Function:** Group-patient report slip (one slip per patient for a whole group)
- **Classification:** MISSING
- **Reference evidence:** reference guide, pages 52-57; reference showcase, page 6 (per-patient slips for a group of patients, archive-safe; envelope/position control on the same page)
- **Current implementation evidence:** No command, query, DTO, writer, view or view model produces a one-line-per-patient slip (systematic search returned no implementation; nearest artefacts are the bench worksheet and the collection sheet, which are different documents).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Add a group-slip query and writer producing one slip per patient with the reference layout, reachable from the follow-up/delivery area.
- **Dependencies:** Patient, visit-test and test data (exist; no schema change).
- **Open decision, if any:** None.

#### GAP-014

- **Function:** Editable report item template (letters, reference values, show/hide, extensible lists, free text)
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 12 (report item layout configuration)
- **Current implementation evidence:** `ReportContentBuilder.cs` emits narrative lines and grids without per-line template controls; it does not emit a fixed named-section sequence report; no item-template entity, command, DTO or show/hide/free-text VERIFIED construct exists.
- **Verification status:** —
- **Required outcome:** aggregate with management surface and drive the builder from it.
- **Dependencies:** Phase 7 precedes Phases 8-10 and 22.
- **Open decision, if any:** STATICALLY Introduce an item-template Phase 1 only; Phase 2 is not a prerequisite.

#### GAP-015

- **Function:** Line spacing, item distance from the margin, and symbol before each item
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 13 (layout and marker control per report item)
- **Current implementation evidence:** Only page margins and top space are persisted (`PageMarginLeftCm`, `PageMarginBottomCm`, `ReportTopSpaceCm`); no line-spacing field, no VERIFIED per-item report offset (per-item offsets exist only for the envelope, a different surface), no marker configuration.
- **Verification status:** STATICALLY
- **Required outcome:** Persist per-item layout (line spacing, left offset, with GAP-005 (implement after marker symbol) and apply it the branding foundation). in the report composer.
- **Dependencies:** Report composer region shared
- **Open decision, if any:** None.

#### GAP-016

- **Function:** Shade or colour abnormal results, or disable the shading
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 13 (highlight out-of-range results with an off switch)
- **Current implementation evidence:** `ResultFlagComputer` computes Low/High and the builder prints low/high comment text; no colour or background is ever applied to a result cell (writer search returns only page-background and footer greys).
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Add a global enable flag plus abnormal-result colours and apply them to flagged result cells.
- **Dependencies:** Result-cell rendering shared with GAP-017 (implement before the per-profile override).
- **Open decision, if any:** None.

#### GAP-017

- **Function:** Per-profile control of normal/abnormal result font and background colour
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 16 (per-profile result styling)
- **Current implementation evidence:** No per-profile styling attribute, command, DTO or writer branch exists on `Profile`, `ProfileAnalyte` or `ProfileResultItem`.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Add per-profile styling attributes with a documented precedence rule against the global colours, and apply them in the writers.
- **Dependencies:** GAP-016 (implement after the base colouring).
- **Open decision, if any:** None.

#### GAP-018

- **Function:** Edit the character used to mark the normal range
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 17 (normal-range marker control)
- **Current implementation evidence:** The marker text is a fixed string in the report content (`ReportDocumentContent.cs:129` area); no setting, command or view-model property exposes it.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Persist one nullable marker Same aggregate and writer file None. string and substitute it in as GAP-005 (implement with the the writer.
- **Dependencies:** branding foundation).
- **Open decision, if any:** —

#### GAP-019

- **Function:** Toggle report elements from a doctor-facing control
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 15 (show or hide any report element on demand, e.g. side address/title, signature, sample block)
- **Current implementation evidence:** `ReportSettings` is a single global row with no per-user layout entity; the only per-item switches in the product belong to the envelope; the dialogs folder holds only the secondary-password dialog.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Persist per-user report-element visibility preferences and enforce them at report build time.
- **Dependencies:** Report item taxonomy (stabilised by GAP-014; implement after it).
- **Open decision, if any:** None.

#### GAP-020

- **Function:** Optional vertical identity coding on the result report, receipt and envelope
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 3 (vertical coding shown on the three documents; explicitly optional)
- **Current implementation evidence:** No setting, command or writer branch for vertical or rotated identity coding; the only barcode path renders horizontally on a single label.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Add an optional vertical-coding rendering mode applied to the identity mark on all three documents, governed by a persisted toggle.
- **Dependencies:** GAP-009 (same identity mark and label primitive; implement per-document toggle). after it).
- **Open decision, if any:** OD-03 (global versus

#### GAP-021

- **Function:** Sticker size configuration for barcode labels
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 3 (label stock 38 mm x 25 mm)
- **Current implementation evidence:** `BarcodeLabelRenderer.Render` takes dimensions only from call-site constants (service invokes it with defaults); no size setting, command, DTO or view model exists.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Persist label width and height and pass them through the barcode service into the renderer.
- **Dependencies:** Label pipeline (exists); must precede GAP-009.
- **Open decision, if any:** None.

#### GAP-022

- **Function:** Sample-nature interpretation panel (nature, normal ranges, effect on the patient)
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 51 (sample-nature interpretation entry)
- **Current implementation evidence:** `Test` carries collection and group labels and `ReferenceRange` supplies intervals, but no interpretation entity, command, query or panel presents a sample nature with its ranges and clinical meaning.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Introduce sample-nature interpretation records with (exist). a read surface combining nature, ranges and interpretation text.
- **Dependencies:** Catalogue and range data
- **Open decision, if any:** None (editorial content risk recorded in phase).

#### GAP-023

- **Function:** Glossary of abbreviations used in medical analysis
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 51 (terminology lookup entry)
- **Current implementation evidence:** `Test.Short`/test codes are catalogue heading fields; no glossary entity, command, query or definition view exists.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Introduce glossary records (abbreviation, full term, notes) with create/read/update/delete and a lookup surface.
- **Dependencies:** None.
- **Open decision, if any:** None.

#### GAP-024

- **Function:** Appointment-note reminder inside the application
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 51 (reminder note to a user or phone-book Phone-book storage and queries exist, but no reminder, appointment, contact through the appointment notebook; in-application delivery, note-scheduling or notification entity, command, query, DTO, view or VERIFIED no external channel)
- **Current implementation evidence:** view model exists.
- **Verification status:** STATICALLY
- **Required outcome:** Introduce internal reminders (target, message, unaffected). due date, state) with management and an in-application pending-reminder surface; no external channel.
- **Dependencies:** Phone book (exists,
- **Open decision, if any:** OD-04 (scheduling specifics).

#### GAP-025

- **Function:** Search and statistics on blood-picture element values in time periods
- **Classification:** MISSING
- **Reference evidence:** reference showcase, page 73 (element-value band search over CBC analytes: HGB/HCT/MCV/MCH/MCHC/RETICS/PLT/PCT/MPV/LYM/MON/NEU/EOS/BAS/BAND with from/to bands and a statistics action)
- **Current implementation evidence:** The only banded monitor filters `PatientTest.ResultValue` scalars (`GetBandedResultMonitorQueryHandler.cs:57-105`); CBC results live as analyte-level rows in `ProfileResultItem`, which that query never reads.
- **Verification status:** STATICALLY VERIFIED
- **Required outcome:** Extend banded search from test-level scalars to profile-analyte rows with per-element band inputs over a period, reusing the existing history pivot helper.
- **Dependencies:** `ProfileResultItem` and `HistoryMatrixBuilder.Pivot` (exist; no schema change).
- **Open decision, if any:** OD-06 (hosting note).

Unresolved inventory items: none. Every inventoried in-scope function carries exactly one classification above or in the IMPLEMENTED population (127 functions, each covered by at least a Tier 1 reachability check and recorded in the verification working material).

------------------------------------------------------------------------

## SECTION 4 --- Ordered Implementation Phases

Phases are ordered by code dependency. Phase 1 is the next phase to implement. Each phase introduces at most one new migration. All six owner decisions are resolved, so all phases are ready for implementation. In this document, planning readiness does NOT mean a phase has already been implemented: a phase is complete only after its acceptance checks pass and, when applicable, the owner confirms the migration is applied.

### Phase 1 --- Report settings foundation: header/footer images and normal-range marker

**A. Phase identification**

- Phase number: 1
- Descriptive title: Report settings foundation --- image header/footer and normal-range marker
- Gap IDs: GAP-005, GAP-006, GAP-018
- Each covered function marked: GAP-005 DIFFERENT, GAP-006 DIFFERENT, GAP-018 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: size medium because three related settings share one aggregate and one writer region; risk medium because the Images mode is currently offered but silently falls back to words, so a wrong image-path policy could produce silently broken branding.

**B. Dependency position**

- Why now: This establishes the report-settings and branding foundation. Phases 7, 9, and 10 extend report-content rendering, while Phase 8 extends the item-template layout introduced by Phase 7. Phase 2 is a separate receipt-settings path using `ReceiptPdfWriter`; it does not depend on this phase's report-image columns or `ReportContentBuilder`.
- Prerequisite phases or existing conditions: none. The `ReportSettings` entity and its configuration already exist; `HeaderColor`/`FooterColor` columns already exist and are seeded null.
- Exact code dependencies: `Domain/Settings/ReportSettings.cs`, `Infrastructure/Persistence/Configurations/ReportSettingsConfiguration.cs`, `Infrastructure/Printing/ReportContentBuilder.cs`, `Presentation/ViewModels/Settings/ReportSettingsViewModel.cs`, `Application/Features/SystemAndPrintSettings/Commands/UpdateReportSettings/UpdateReportSettingsCommandHandler.cs`.
- Downstream phases that depend on it: Phase 3 uses the report-branding foundation; Phase 7 adds report item templates; Phases 9 and 10 extend report rendering; Phase 8 extends the template from Phase 7. Phase 2 is a separate receipt-settings path using `ReceiptPdfWriter`, not `ReportContentBuilder`, and is not a code dependency of this phase.
- Shared state or execution paths affected: the single `ReportSettings` row (PK = 1) and every report shape built by `ReportContentBuilder`.

**C. Required reference behaviour**

- GAP-005: header/footer from attached image files (reference guide, page 194): the user can select an image file for the report header or footer; the three modes are none / words / image files.
- GAP-006: header/footer colours (reference guide, pages 195-198): the colour of individual header/footer elements can be changed.
- GAP-018: edit the character used to mark the normal range (reference showcase, page 17): the normal-range marker character can be changed.
- User workflow: open report settings, choose header/footer mode, pick image file or set colour, set marker character, save; reports render accordingly.

**D. Current Top-Lab behaviour**

- `HeaderFooterMode.Images` is offered in `ReportSettingsViewModel` but `ReportContentBuilder.cs:32,68,138` branches only on `!= None`; receipt and envelope writers fall back to the words header with an explicit code comment. No image column, file picker, or upload path exists (static inspection).
- `ReportSettings.HeaderColor`/`FooterColor` are declared and mapped but have no setter, no command field, no view-model property, and no writer read (7 code hits total; static inspection).
- GAP-018: the marker text is a fixed string in report content; no setting, command, or view-model exposes it (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- GAP-005: reference offers image mode; Top-Lab offers the mode but silently produces words. Delta: add persisted image references, file picker, image load/scaling in QuestPDF, fallback on missing file.
- GAP-006: reference allows per-element colour; Top-Lab stores null, inert columns. Delta: add mutators, command fields, settings controls, writer application.
- GAP-018: reference allows marker edit; Top-Lab hard-codes the marker. Delta: add nullable marker column and writer substitution.

**F. Exact implementation changes by layer**

- Domain: `ReportSettings` --- add `HeaderImagePath` and `FooterImagePath` (nullable strings, max 300 each), and `NormalRangeMarker` (nullable string, max 10). Null/empty means use the existing default marker. The reference describes a marker character but does not explicitly require a single-character column; max 10 follows the existing `Antibiotic.Symbol` precedent.
- Application: `UpdateReportSettingsCommand` --- add image path fields and marker field; `UpdateReportSettingsCommandHandler` --- persist them.
- Infrastructure: `ReportSettingsConfiguration` --- map nullable `HeaderImagePath` and `FooterImagePath` as nullable strings with `HasMaxLength(300).IsRequired(false)`, and `NormalRangeMarker` as a nullable string with `HasMaxLength(10).IsRequired(false)`; `ReportContentBuilder` --- render header/footer images and apply marker substitution.
- Presentation: `ReportSettingsViewModel` --- image picker and colour/marker controls; view (XAML) bindings.
- Tests: settings unit tests for image/color/marker persistence; regression tests for report rendering in Words mode (existing) unchanged.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add `HeaderImagePath` and `FooterImagePath` (nullable strings, max 300 each) and `NormalRangeMarker` (nullable string, max 10) to `ReportSettings`. Max 10 follows `Antibiotic.Symbol` because the cited reference evidence does not explicitly specify a single-character storage limit.

- Back-fill: none required (nullable, default null).

- Compatibility: additive only; existing migrations unchanged.

- Evidence state not already persisted: the three columns do not exist in any migration or the model snapshot.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

- Settings seed convention: `SystemSettings`, `ReportSettings`, and `ReceiptSettings` seed their single row with `HasData` using an anonymous object that lists every property; every non-nullable column added to these entities must also be added to that seed object (nullable columns may be listed as `null`), and the matching `...SettingsConfiguration.cs` must be updated in the same phase, otherwise the EF model fails validation. For Phase 1, all three added columns are nullable and may be included in the seed object as `null`.

**H. Business rules and message text**

- No new user-visible message text established by the reference pages beyond the mode selection. Validation: image file must be a supported image type and within size limits; colour values stored as strings consistent with existing `HasMaxLength(9)` column convention.

**I. Objective acceptance criteria**

- Given a report settings screen, when the user selects Images mode and picks a header image, then the saved `HeaderImagePath` is non-null and appears in report header output.
- Given the same screen, when the user sets a header colour, then `HeaderColor` is persisted and applied by the writer.
- Given the same screen, when the user sets the normal-range marker, then `NormalRangeMarker` is persisted and substituted in report output.
- Regression: existing Words-mode reports continue to render unchanged.

**J. Test plan**

- Domain/Application: settings persistence unit tests for new columns.
- Infrastructure: `ReportContentBuilder` image-rendering test with a bundled test image; fallback test when file missing.
- Presentation: settings view model test for image picker command.
- Regression: existing report golden tests for Words mode must still pass.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Complete the following phase-specific platform check before handoff: Additional Windows check: image rendering on Windows with QuestPDF.
10. Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
11. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: a machine-local absolute image path may not resolve for other users or workstations because report settings are shared. Mitigation: before implementation, inspect the actual deployment and asset-storage pattern; choose one durable strategy that works for every supported user. Because the planned schema stores paths, prefer an application-managed asset store and persist a stable managed relative key/path rather than an arbitrary absolute path from the file picker. If the supported deployment has no asset location accessible to every user who reads the shared settings, stop and revise the design to store the image bytes instead of shipping a machine-local path. Phase 2 must reuse the same strategy for the receipt logo.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 2 --- Receipt font type, size, colour, and laboratory logo

**A. Phase identification**

- Phase number: 2
- Descriptive title: Receipt header/footer font controls and logo
- Gap IDs: GAP-007
- Each covered function marked: GAP-007 DIFFERENT
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because six of eight documented receipt controls already exist; only font family/size/colour and logo image are missing. Risk medium because font settings may not be honoured by the font resolver on all Windows configurations, and logo rendering requires a real Images branch in the receipt writer.

**B. Dependency position**

- Why now: this is a separate receipt-settings feature implemented in `ReceiptPdfWriter`; it does not use `ReportContentBuilder` or `ReportSettings`. It follows Phase 1 in the plan's sequence, but it does not depend on Phase 1's report-image columns or report-rendering path.
- Prerequisite phases or existing conditions: none. `ReceiptSettings` exists with the currently implemented receipt controls.
- Exact code dependencies: `Domain/Settings/ReceiptSettings.cs`, `Infrastructure/Persistence/Configurations/ReceiptSettingsConfiguration.cs`, `Infrastructure/Printing/ReceiptPdfWriter.cs`, `Presentation/ViewModels/Settings/ReceiptSettingsViewModel.cs`, `Application/Features/SystemAndPrintSettings/Commands/UpdateReceiptSettings`.
- Downstream phases that depend on it: Phase 4 (receipt logo reuses these columns).
- Shared state or execution paths affected: the single `ReceiptSettings` row and the receipt printing path (`ReceiptPdfWriter`).

**C. Required reference behaviour**

- GAP-007: receipt settings font type, size, colour, and logo (reference guide, pages 202-204): choose the font type, font size and font colour for receipt header/footer and choose a laboratory logo image.
- User workflow: open receipt settings, set header/footer font family/size/colour, pick logo image, save; receipts render with chosen fonts and logo.

**D. Current Top-Lab behaviour**

- `ReceiptSettings` models margins, currency, pickup time, print-once, detail mode, cashier-printer flag, and header/footer mode; no font family/size/colour and no logo reference exist (static inspection).
- `ReceiptPdfWriter.cs:200-202` falls back to the words header when `HeaderFooterMode == Images`; no logo asset column, picker, or upload path exists (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: font type/size/colour and logo on receipts. Current: none of the four controls exist; logo path inert. Delta: persist font family/size/colour and logo path; expose on receipt-settings screen; render fonts and logo in receipt writer with real Images branch.

**F. Exact implementation changes by layer**

- Domain: `ReceiptSettings` --- add `HeaderFontFamily` string? (nullable, max 100; DESIGN), `HeaderFontSizePt` decimal(4,1) nullable (DESIGN), `HeaderFontColor` string? (nullable, max 9), `FooterFontFamily` string? (nullable, max 100; DESIGN), `FooterFontSizePt` decimal(4,1) nullable (DESIGN), `FooterFontColor` string? (nullable, max 9), and `LogoImagePath` string? (nullable, max 300). Font-family length 100 and point-size decimal(4,1) are design choices without direct repository precedent; configure both decimal columns with explicit `HasColumnType` and `HasPrecision`.
- Application: `UpdateReceiptSettingsCommand` --- add the new fields; handler persists them.
- Infrastructure: `ReceiptSettingsConfiguration` --- map the seven new nullable columns; `ReceiptPdfWriter` --- real Images branch rendering logo and applying fonts/colours.
- Presentation: `ReceiptSettingsViewModel` --- font and logo controls; view bindings.
- Tests: receipt writer font/logo rendering test; settings persistence tests; regression: existing receipt Words-mode tests unchanged.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add seven nullable columns to `ReceiptSettings`: `HeaderFontFamily` string? max 100 (DESIGN), `HeaderFontSizePt` nullable decimal(4,1) (DESIGN), `HeaderFontColor` string? max 9, `FooterFontFamily` string? max 100 (DESIGN), `FooterFontSizePt` nullable decimal(4,1) (DESIGN), `FooterFontColor` string? max 9, and `LogoImagePath` string? max 300. All seven are nullable. Configure decimal columns with explicit `HasColumnType` and `HasPrecision`.

- Back-fill: none required.

- Compatibility: additive only; existing migrations unchanged.

- Evidence state not already persisted: none of the seven columns exists in any migration or the model snapshot.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

- Settings seed convention: `SystemSettings`, `ReportSettings`, and `ReceiptSettings` seed their single row with `HasData` using an anonymous object that lists every property; every non-nullable column added to these entities must also be added to that seed object (nullable columns may be listed as `null`), and the matching `...SettingsConfiguration.cs` must be updated in the same phase, otherwise the EF model fails validation.

**H. Business rules and message text**

- Validation: font family must be a valid installed font name; size within reasonable bounds; colour strings consistent with existing conventions. Logo file must be a supported image type within size limits.

**I. Objective acceptance criteria**

- Given receipt settings, when the user sets header font family/size/colour, then the values are persisted and applied to rendered receipt output.
- Given receipt settings, when the user picks a logo image, then `LogoImagePath` is persisted and the logo appears on the receipt for both plain and pre-printed paper.
- Regression: existing receipt output without logo unchanged.

**J. Test plan**

- Domain/Application: receipt settings persistence unit tests.
- Infrastructure: `ReceiptPdfWriter` logo/font rendering test with bundled image and test fonts.
- Presentation: receipt settings view model tests.
- Regression: existing receipt printing tests must still pass.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Complete the following phase-specific platform check before handoff: Additional Windows check: receipt font rendering with installed system fonts.
10. Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
11. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: font settings that cannot be honoured by the font resolver silently substitute. Mitigation: document that fonts are resolved at render time; prefer widely available fonts.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 3 --- Report on plain or pre-printed paper with a logo

**A. Phase identification**

- Phase number: 3
- Descriptive title: Report logo on pre-printed or plain paper
- Gap IDs: GAP-011
- Each covered function marked: GAP-011 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because it is pure configuration and rendering of the primitive built in Phase 1; carries no logic of its own. Low risk because once Phase 1 exists this is a writer branch.

**B. Dependency position**

- Why now: depends on Phase 1 (image primitive) and Phase 1 colour support. Must not be attempted before Phase 1.
- Prerequisite phases or existing conditions: Phase 1 complete. Plain-paper path is the default and works; pre-printed path is approximated by Words mode.
- Exact code dependencies: `Infrastructure/Printing/ReportContentBuilder.cs`, `Infrastructure/Printing/ReportPdfWriter.cs`, `Domain/Settings/ReportSettings.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: report writing path; shares `ReportSettings` row with Phase 1.

**C. Required reference behaviour**

- GAP-011: print the report on plain or pre-printed paper with a logo (reference showcase, pages 5, 16): paper choice and logo branding for reports.
- User workflow: choose paper type (plain or pre-printed) and logo; reports render with logo and appropriate positioning.

**D. Current Top-Lab behaviour**

- Plain-paper path is the default and works. Pre-printed path approximated by `HeaderFooterMode.Words`; logo half not implemented (`ReportContentBuilder.cs:32,68,138` never loads an image) (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: logo on report for both paper choices. Current: words branding present; image branding inert. Delta: render persisted header/footer images on the report for both paper choices.

**F. Exact implementation changes by layer**

- Infrastructure: `ReportContentBuilder` / `ReportPdfWriter` --- load and render persisted header/footer images; paper-choice handling.
- No new Domain columns (reuses Phase 1 columns); no new Application fields (reuse existing settings command).
- Tests: report writer logo rendering test; regression: existing report tests unchanged.

**G. Migration decision: NO MIGRATION**

- Reason: reuses the `HeaderImagePath`/`FooterImagePath` columns added by Phase 1; no new schema required. The change is rendering-only over existing columns.

**H. Business rules and message text**

- No new message text beyond existing settings wording. Validation: image file must exist at render time; fallback to words when missing.

**I. Objective acceptance criteria**

- Given report settings with header/footer images configured, when a report is generated on plain paper, then the images appear in header/footer.
- Given the same settings on pre-printed paper, then the images appear positioned per configuration.

**J. Test plan**

- Infrastructure: report writer image rendering test for both paper choices.
- Regression: existing report golden tests must still pass.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: logo colliding with pre-printed stationery. Mitigation: configurable positioning.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 4 --- Receipt header/footer logo

**A. Phase identification**

- Phase number: 4
- Descriptive title: Receipt header/footer logo rendering
- Gap IDs: GAP-010
- Each covered function marked: GAP-010 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because it reuses the logo column from Phase 2; same reasoning as Phase 3 on the receipt surface. Low risk.

**B. Dependency position**

- Why now: depends on Phase 2 (logo column on ReceiptSettings). Same reasoning as Phase 3.
- Prerequisite phases or existing conditions: Phase 2 complete.
- Exact code dependencies: `Infrastructure/Printing/ReceiptPdfWriter.cs`, `Domain/Settings/ReceiptSettings.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: receipt writing path; shares `ReceiptSettings` row with Phase 2.

**C. Required reference behaviour**

- GAP-010: receipt header/footer logo on pre-printed or plain receipt paper (reference showcase, page 4): receipt paper choice with logo branding.
- User workflow: logo configured on receipt settings; receipts render with the logo for both paper choices.

**D. Current Top-Lab behaviour**

- Words half implemented; logo half inert (`ReceiptPdfWriter.cs:200-202` falls back to words; no logo asset) (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: logo on receipts. Current: words branding present; logo inert. Delta: render persisted logo on the receipt for both paper choices.

**F. Exact implementation changes by layer**

- Infrastructure: `ReceiptPdfWriter` --- real Images branch rendering persisted logo.
- No new schema (reuses Phase 2 columns).
- Tests: receipt writer logo rendering test; regression: existing receipt tests unchanged.

**G. Migration decision: NO MIGRATION**

- Reason: reuses the `LogoImagePath` column added by Phase 2; rendering-only change over existing columns.

**H. Business rules and message text**

- Validation: logo image supported type and size; fallback to words when missing.

**I. Objective acceptance criteria**

- Given receipt settings with logo configured, when a receipt is printed, then the logo appears on the receipt for both plain and pre-printed paper.

**J. Test plan**

- Infrastructure: receipt writer logo test.
- Regression: existing receipt tests must still pass.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 5 --- Patient receivable account summary with balance filter

**A. Phase identification**

- Phase number: 5
- Descriptive title: Patient receivable balance filter (unpaid / discounted / paid only)
- Gap IDs: GAP-008
- Each covered function marked: GAP-008 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because the data needed already exists (`PaymentOperation.OperationType` distinguishes payment, correction, extra charge, void); the change is a query predicate only. Low risk.

**B. Dependency position**

- Why now: purely additive query surface; touches no shared file with other phases; can be done at any point. Placed early because it is small and independent.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/InventoryAndAccounting/Queries/GetPatientSamplesDetail`, `InventoryDtos.cs`, presentation view for account summary.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: patient-level account query; no schema change.

**C. Required reference behaviour**

- GAP-008: patient receivable account summary for a period with a balance filter (reference showcase, page 26): period receivable summary filtered to unpaid, discounted or paid-only.
- User workflow: open receivable summary for a period, choose balance filter (unpaid-only / discounted-only / paid-only), view filtered patient rows with charged/paid/balance.

**D. Current Top-Lab behaviour**

- `GetPatientSamplesDetailQuery(From, To)` returns per-patient charged/paid/balance with no filter parameter; no filter for bad-debt-only, discounted-only, or paid-only exists on any patient-level account query (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: three-way balance filter. Current: period summary exists but no filter. Delta: add three optional filter booleans plus predicate to the patient account query and surface on screen.

**F. Exact implementation changes by layer**

- Application: `GetPatientSamplesDetailQuery` --- add `UnpaidOnly`, `DiscountedOnly`, `PaidOnly` parameters; handler applies predicate based on `PaymentOperation` classification.
- Presentation: account summary view --- filter controls and bindings.
- Tests: query predicate unit tests; regression: existing account queries unchanged.
- No Domain or Infrastructure schema changes.

**G. Migration decision: NO MIGRATION**

- Reason: the data required already exists (`PaymentOperation.OperationType`); the filter is a query predicate only. No schema change required.

**H. Business rules and message text**

- Validation: "discounted" defined consistently with the discount representation already used by `PaymentOperation`.

**I. Objective acceptance criteria**

- Given a period with mixed payment states, when the user applies the unpaid-only filter, then only patients with unpaid balance appear.
- Given the same period, when the user applies paid-only, then only fully paid patients appear.
- Given the same period, when the user applies discounted-only, then only patients with discount operations appear.

**J. Test plan**

- Application: filter predicate unit tests covering each balance state.
- Regression: existing account summary tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: inconsistent definition of "discounted". Mitigation: align with existing `PaymentOperation` discount semantics.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 6 --- Group-patient report slip

**A. Phase identification**

- Phase number: 6
- Descriptive title: Group-patient report slip (one slip per patient for a whole group)
- Gap IDs: GAP-013
- Each covered function marked: GAP-013 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because a new report shape with pagination (one slip per patient) and a reuse decision between `ReportPdfWriter` and the worksheet writer. Medium risk because of possible duplication of patient-data rendering rather than sharing it.

**B. Dependency position**

- Why now: independent of all other phases; touches no entity and no shared file with Phases 1-5. Placed here because it is query-surface work and small enough to proceed early.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/PatientSearch` or `Application/Features/ReportProduction` queries; `Domain/Patients/Patient`, `Domain/Results/PatientTest`, `Domain/Tests/Test`; presentation view for group-slip.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: patient/visit-test read path; no schema change.

**C. Required reference behaviour**

- GAP-013: group-patient report slip (one slip per patient for a whole group of patients) (reference guide, pages 52-57; reference showcase, page 6): one slip per patient, archive-safe, to prevent errors and keep an archive.
- User workflow: select a group of patients, generate slips (one per patient), print or archive each slip.

**D. Current Top-Lab behaviour**

- No command, query, DTO, writer, view, or view model produces a one-line-per-patient slip (systematic negative search). Nearest artefacts are the bench worksheet and the collection sheet, which are different documents (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: per-patient slips for a group. Current: absent. Delta: add a group-slip query and writer producing one slip per patient with the reference layout, reachable from the follow-up/delivery area.

**F. Exact implementation changes by layer**

- Application: new group-slip query and handler; DTO per patient slip.
- Infrastructure: new PDF writer or reuse of existing writer for the slip layout; pagination (one slip per patient).
- Presentation: view/view model to select patients and trigger slip generation; navigation entry.
- Tests: slip generation unit test; regression: existing report tests unchanged.
- No Domain or Infrastructure schema changes.

**G. Migration decision: NO MIGRATION**

- Reason: reads `Patient`, `PatientTest`, and `Test`, all already persisted. No new column required.

**H. Business rules and message text**

- No specific message text established by the reference beyond the slip layout. Validation: patient selection must be non-empty.

**I. Objective acceptance criteria**

- Given a group of selected patients, when the user generates the slip, then one slip per patient is produced with patient data and tests.
- Given the group, when printed/archived, then each slip is archive-safe and separate.

**J. Test plan**

- Application: group-slip query unit test.
- Infrastructure: slip writer output test.
- Regression: existing report/worksheet tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: duplicating patient-data rendering instead of sharing it. Mitigation: reuse existing patient-data rendering components.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 7 --- Editable report item template

**A. Phase identification**

- Phase number: 7
- Descriptive title: Editable report item template (letters, reference values, show/hide, extensible lists, free text)
- Gap IDs: GAP-014
- Each covered function marked: GAP-014 MISSING
- Estimated size: high
- Estimated risk: high
- Rationale: high size because it introduces an item-template aggregate across five report shapes and multiple narrative/grid line kinds. High risk because template-driven visibility and layout could regress report output; document chrome remains in `ReportPageComposer`.

**B. Dependency position**

- Why now: Phase 7 introduces the item template that Phases 8 and 22 extend. Phases 9 and 10 colour result lines produced by this same builder and are sequenced after Phase 7 so they colour template-driven output rather than being reworked. `ReportContentBuilder` is used by report PDF/export/view-model paths, not by receipt or Phase 11 simple result-review paths.
- Prerequisite phases or existing conditions: Phase 1 complete (report-settings foundation); the existing `ReportSettings` and `ReportContentBuilder` code are present. Phase 2 is not a prerequisite.
- Exact code dependencies: `Infrastructure/Printing/ReportContentBuilder.cs`, `Infrastructure/Printing/ReportDocumentContent.cs`, and the new item-template aggregate. Verified consumers are `ReportPdfWriter.cs:64`, `PatientReportPdfExporter.cs:41`, `CombinedReportViewModel.cs:246`, and `ProfileEntryViewModel.cs:443`. `ReceiptPdfWriter`, `EnvelopePdfWriter`, `InvoicePdfWriter`, and `LabOrderPdfWriter` do not use this builder.
- Downstream phases that depend on it: Phases 8, 9, 10, 22 (per-item layout, colouring, and per-user visibility build on the item template).
- Shared state or execution paths affected: five report shapes built by `ReportContentBuilder`: `FromCombined` (L188-333, combined analysis report), `FromBlank` (L335-367, no sections), `FromHistory` (L369-427, one grid), `FromPatientExport` (L54-122, one section per test line), and `FromProfileReport` (L124-186). Public entry points include `FromEnvelope` (L15), `FromPatientExport` (L54), and `FromProfileReport` (L124). The builder emits narrative lines with a null section heading and history/sensitivity grids; it does not emit a fixed sequence of named sections. Header, title, patient block, signature, and footer are drawn by `ReportPageComposer.cs`.

**C. Required reference behaviour**

- GAP-014: edit report item letters and reference values; hide/show items; extend selection lists; allow free text (reference showcase, page 12): configure the report's item layout.
- User workflow: open report item template management, edit item letters/reference values, toggle show/hide, extend selection lists, allow free text; reports render per template.

**D. Current Top-Lab behaviour**

- `ReportContentBuilder.cs` emits narrative lines and selected grids, not a fixed sequence of named sections. It supports `FromCombined` (lines and culture-sensitivity grid), `FromBlank` (no sections), `FromHistory` (one history grid), `FromPatientExport` (one section per test line), and `FromProfileReport`. Public entry points include `FromEnvelope`, `FromPatientExport`, and `FromProfileReport`. Current line kinds include group subtitle (when enabled), test name/result, off-lab note, reference range, low/high comments, test comments, profile analyte/unit/range/comment lines, culture and microscopy lines, the `جدول الحساسية` sensitivity grid, and history-grid columns for date, test, result, reviewed, and comment. Document chrome is drawn by `ReportPageComposer.cs`. No item-template entity, command, DTO, or show/hide/free-text construct exists (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: editable item template. Current: narrative lines and grids are emitted without configurable per-line template controls. Delta: introduce an item-template aggregate with management surface and let templates control the supported line kinds without moving document chrome out of `ReportPageComposer`.

**F. Exact implementation changes by layer**

- Domain: new `ReportItemTemplate` aggregate with `ItemKey` string PK (required, max 100; configure `IsRequired()`), `Label` string (required, max 200; configure `IsRequired()`), `ReferenceValueExpression` string? (nullable, max 500; configure `IsRequired(false)`), `ShowHide` bool (non-null, default true), `ListMembership` string? (nullable, max 100; configure `IsRequired(false)`), and `FreeTextOverride` bool (non-null, default false).
- Application: CRUD commands/queries for item templates; the save path validates template keys and values and the builder uses template data to control supported narrative/grid line kinds.
- Infrastructure: EF configuration for `ReportItemTemplates` table; `ReportContentBuilder` updated to use templates for line visibility, labels/reference expressions, lists, and free-text overrides. Keep header/title/patient/signature/footer composition in `ReportPageComposer.cs`.
- Presentation: item-template management view/view model; bindings.
- Tests: template CRUD tests; tests for each supported line kind and grid, including visibility, label/reference expression, list and free-text behaviour; regression tests for all five report shapes and unchanged document chrome.
- Migration: one new migration for `ReportItemTemplates` table with seeded default rows.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add `ReportItemTemplates` with `ItemKey` string PK required max 100 (`IsRequired()`); `Label` string required max 200 (`IsRequired()`); `ReferenceValueExpression` string? nullable max 500 (`IsRequired(false)`); `ShowHide` bool non-null default true; `ListMembership` string? nullable max 100 (`IsRequired(false)`); and `FreeTextOverride` bool non-null default false.

- Back-fill: seed one row per template-controllable line kind emitted by the builder: group subtitle, test name/result, off-lab note, reference range, low/high comments, test comments, profile analyte/unit/range/ low/high-comment lines, culture lines, microscopy lines, sensitivity grid (`جدول الحساسية`), and history-grid columns (date, test, result, reviewed, comment). PROPOSED `ItemKey` values: `GroupSubtitle`, `TestName`, `ResultValue`, `OffLabNote`, `ReferenceRange`, `LowComment`, `HighComment`, `TestComment`, `ProfileAnalyte`, `ProfileUnit`, `ProfileReferenceRange`, `ProfileLowComment`, `ProfileHighComment`, `CultureLine`, `MicroscopyLine`, `CultureSensitivityGrid`, `HistoryDate`, `HistoryTest`, `HistoryResult`, `HistoryReviewed`, `HistoryComment`. The implementing agent must confirm this proposed key list against the actual builder before coding. Seed rows should preserve current output by default. Document chrome is not a builder line kind and is not seeded here.

- Compatibility: additive only; existing migrations unchanged.

- Evidence state not already persisted: no `ReportItemTemplates` table exists in any migration or the model snapshot.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

**H. Business rules and message text**

- Validation: item key unique; required labels are nonblank; nullable reference expressions and list membership are validated when present; show/hide and free-text flags are booleans. Confirm the proposed key list against builder branches before implementing seed rows.

**I. Objective acceptance criteria**

- Given the item-template management screen, when the user edits a template label or reference expression, then the corresponding line kind renders using the edited value.
- Given the same screen, when the user hides an item, then the item is absent from report output.
- Given the same screen, when the user allows free text for an item, then the relevant free-text workflow is enabled for that item kind.
- Regression: default template seeds preserve all five report shapes and document chrome, except for explicitly intended template-controlled visibility or label changes.

**J. Test plan**

- Domain/Application: item-template CRUD unit tests.
- Infrastructure: line-kind-to-template tests for the listed builder outputs; migration seed test; verify every proposed `ItemKey` against the current builder branches.
- Presentation: template management view model tests.
- Regression: existing report golden tests must still pass with default templates.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: regression in any of the five report shapes or narrative/grid line kinds. Mitigation: seed one template per controllable line kind, preserve document chrome in `ReportPageComposer`, and run full report regression tests.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 8 --- Line spacing, item distance from margin, item symbol

**A. Phase identification**

- Phase number: 8
- Descriptive title: Per-item line spacing, left offset, and marker symbol
- Gap IDs: GAP-015
- Each covered function marked: GAP-015 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because per-item offset storage and marker glyph per item; the envelope already models this (`EnvelopePrintItemPosition`) which can be generalised. Medium risk of diverging from the envelope's proven model.

**B. Dependency position**

- Why now: `ReportTopSpaceCm`/`PageMarginLeftCm` handling lives in the same `ReportPageComposer` region Phases 1, 2, 6 modify; implement after Phase 7 (item template) so per-item layout is part of the template.
- Prerequisite phases or existing conditions: Phase 7 complete (item template aggregate exists).
- Exact code dependencies: `Infrastructure/Printing/ReportPageComposer.cs`, new per-item layout rows on item template.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: report composer region shared with Phase 1.

**C. Required reference behaviour**

- GAP-015: control line spacing, item distance from the left margin, and the symbol before each item (reference showcase, page 13): layout and marker control.
- User workflow: open item template settings, set line spacing, left distance, and symbol per item; reports render per item layout.

**D. Current Top-Lab behaviour**

- Only page margins and top space persisted (`PageMarginLeftCm`, `PageMarginBottomCm`, `ReportTopSpaceCm`); no line-spacing field, no per-item report offset (per-item offsets exist only for the envelope), no marker configuration (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: per-item line spacing, left offset, symbol. Current: none. Delta: persist per-item layout and apply in report composer.

**F. Exact implementation changes by layer**

- Domain: `ReportItemTemplate` --- add nullable `LineSpacing` decimal(4,2), nullable `LeftOffsetCm` decimal(5,2), and nullable `MarkerSymbol` string? max 10. `MarkerSymbol` must map with `IsRequired(false)`; both decimal properties must have explicit `HasColumnType` and `HasPrecision` configuration.
- Application: extend item-template commands/queries.
- Infrastructure: `ReportPageComposer` --- apply per-item offsets and marker glyphs.
- Presentation: item-template view --- per-item layout controls.
- Tests: nullable decimal persistence and configuration tests for `LineSpacing` decimal(4,2) and `LeftOffsetCm` decimal(5,2), marker string max-10/nullability tests, per-item layout rendering tests, and regression tests for existing report output.
- Migration: additive columns on the item-template table introduced by Phase 7 --- can be folded into Phase 7's migration or a separate one; since Phase 7 already introduces the item-template table, Phase 8 can extend it. To keep one migration per phase and avoid re-editing Phase 7's migration, Phase 8 adds columns via its own migration appended after Phase 7's.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after Phase 7's migration; add nullable `LineSpacing` decimal(4,2), nullable `LeftOffsetCm` decimal(5,2), and nullable `MarkerSymbol` string? max 10 to `ReportItemTemplates`. Configure `MarkerSymbol` with `IsRequired(false)` and both decimal columns with explicit `HasColumnType` and `HasPrecision`.

- Back-fill: none required (nullable).

- Compatibility: additive only.

- Evidence state not already persisted: no per-item layout columns exist.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

**H. Business rules and message text**

- Validation: offset values within reasonable bounds; marker symbol single character.

**I. Objective acceptance criteria**

- Given item template settings, when the user sets line spacing for an item, then report output reflects the spacing.
- Given the same settings, when the user sets left offset, then the item is positioned accordingly.
- Given the same settings, when the user sets a marker symbol, then the symbol appears before the item.

**J. Test plan**

- Application: item-template layout field tests.
- Infrastructure: composer per-item offset test.
- Regression: existing report tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: diverging from the envelope's proven per-item offset model. Mitigation: generalise the envelope's `EnvelopePrintItemPosition` shape.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 9 --- Shade or colour abnormal results

**A. Phase identification**

- Phase number: 9
- Descriptive title: Shade or colour abnormal results (with disable option)
- Gap IDs: GAP-016
- Each covered function marked: GAP-016 MISSING
- Estimated size: medium
- Estimated risk: low
- Rationale: medium size because it writes into `ReportContentBuilder`'s result-cell rendering; low risk because driven by the already-computed `ResultFlag`.

**B. Dependency position**

- Why now: the colour of an abnormal result and the layout of its row are the same cell; implement after Phase 8 (per-item layout) so cell composition is stable. Before Phase 10 (per-profile override).
- Prerequisite phases or existing conditions: Phase 8 complete (item-template/layout region stable).
- Exact code dependencies: `Infrastructure/Printing/ReportContentBuilder.cs`, `Domain/Results/ResultFlagComputer.cs`, `Domain/Settings/ReportSettings.cs`.
- Downstream phases that depend on it: Phase 10.
- Shared state or execution paths affected: result-cell rendering shared with Phase 10.

**C. Required reference behaviour**

- GAP-016: shade or colour abnormal results, or disable it (reference showcase, page 13): highlight out-of-range results with an off switch.
- User workflow: open report settings, enable/disable abnormal-result colouring and choose colours; abnormal results render shaded/coloured.

**D. Current Top-Lab behaviour**

- `ResultFlagComputer` computes Low/High and the builder prints low/high comment text; no colour or background is ever applied to a result cell (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: abnormal results shaded/coloured with off switch. Current: flag computed but not visually treated. Delta: add global enable flag plus abnormal-result colours; apply to flagged result cells.

**F. Exact implementation changes by layer**

- Domain: `ReportSettings` --- add `EnableAbnormalResultColor` (non-nullable boolean, default false), `AbnormalResultFontColor` (nullable string, max length 9), and `AbnormalResultBackgroundColor` (nullable string, max length 9). Apply both colours to results whose `ResultFlag` is Low or High when enabled.
- Application: extend settings command/handler.
- Infrastructure: `ReportContentBuilder` --- apply `.FontColor`/`.Background` to result cells driven by `ResultFlag`.
- Presentation: settings view --- colour controls.
- Tests: result-cell colouring test driven by flag; regression: existing report tests unchanged.
- Migration: one new migration for ReportSettings colour columns.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration (after Phase 8's migration); add non-nullable `EnableAbnormalResultColor` (boolean, default false) and nullable `AbnormalResultFontColor`/`AbnormalResultBackgroundColor` strings (max length 9) to `ReportSettings`.

- Back-fill: default `EnableAbnormalResultColor` to false so existing reports remain unchanged; colour columns are nullable and fall back to the existing uncoloured rendering when absent.

- Compatibility: additive only.

- Evidence state not already persisted: no abnormal-result colour columns exist.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

- Settings seed convention: `SystemSettings`, `ReportSettings`, and `ReceiptSettings` seed their single row with `HasData` using an anonymous object that lists every property; every non-nullable column added to these entities must also be added to that seed object (nullable columns may be listed as `null`), and the matching `...SettingsConfiguration.cs` must be updated in the same phase, otherwise the EF model fails validation.

**H. Business rules and message text**

- Validation: colour strings must follow the existing colour-string convention and be at most 9 characters; the enable flag is required and defaults to false.

**I. Objective acceptance criteria**

- Given abnormal-result colouring enabled, when a result is flagged High/Low, then the result cell is coloured/shaded in report output.
- Given the same setting disabled, then flagged results render without colour/shading.

**J. Test plan**

- Infrastructure: result-cell colouring unit test for each flag state.
- Regression: existing report golden tests unchanged with flag off by default.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: contrast failure in print. Mitigation: choose high-contrast colours; provide disable option.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 10 --- Per-profile normal/abnormal result font and background colour

**A. Phase identification**

- Phase number: 10
- Descriptive title: Per-profile control of normal/abnormal result font and background colour
- Gap IDs: GAP-017
- Each covered function marked: GAP-017 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because per-profile override plus precedence rule (profile override vs global). Main design risk is precedence.

**B. Dependency position**

- Why now: the per-profile variant is Phase 9 plus a per-profile override; implement after Phase 9 so the base colouring cell-rendering code is not duplicated.
- Prerequisite phases or existing conditions: Phase 9 complete (base colouring exists).
- Exact code dependencies: `Domain/Tests/Profile.cs`, `Domain/Tests/ProfileAnalyte.cs`, `Infrastructure/Printing/ReportContentBuilder.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: result-cell rendering shared with Phase 9.

**C. Required reference behaviour**

- GAP-017: per-profile control of normal/abnormal result font and background colour (reference showcase, page 16): per-profile result styling.
- User workflow: open profile settings, set normal/abnormal font and background colour per profile; reports render per profile with override precedence.

**D. Current Top-Lab behaviour**

- No per-profile styling attribute, command, DTO, or writer branch exists on `Profile`, `ProfileAnalyte`, or `ProfileResultItem` (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: per-profile styling with precedence. Current: none. Delta: add per-profile styling attributes with documented precedence against global colours; apply in writers.

**F. Exact implementation changes by layer**

- Domain: `Profile` --- add `NormalFont` (nullable string, max length 50), `NormalColor` (nullable string, max length 9), `AbnormalFont` (nullable string, max length 50), and `AbnormalBackgroundColor` (nullable string, max length 9).
- Application: extend profile commands/queries.
- Infrastructure: `ReportContentBuilder` --- apply per-profile styling with precedence over global.
- Presentation: profile editor view --- colour controls.
- Tests: per-profile styling precedence test; regression: existing profile/report tests unchanged.
- Migration: one new migration for per-profile colour columns.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration (after Phase 9's migration); add `NormalFont` (nullable string, max 50), `NormalColor` (nullable string, max 9), `AbnormalFont` (nullable string, max 50), and `AbnormalBackgroundColor` (nullable string, max 9) columns to the `Profiles` table.

- Back-fill: null by default so global colours apply.

- Compatibility: additive only.

- Evidence state not already persisted: no per-profile styling columns exist on `Profiles`.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

**H. Business rules and message text**

- Validation: precedence rule documented (profile override vs global); colour strings consistent with conventions.

**I. Objective acceptance criteria**

- Given a profile with abnormal colour configured, when a result in that profile is flagged abnormal, then the profile colour overrides the global colour.
- Given a profile without override, then the global colour from Phase 9 applies.

**J. Test plan**

- Application: profile styling unit tests.
- Infrastructure: precedence rendering test.
- Regression: existing profile tests unchanged with null defaults.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: precedence ambiguity between profile and global. Mitigation: document precedence rule explicitly.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 11 --- Verify / unverify simple single-value results

**A. Phase identification**

- Phase number: 11
- Descriptive title: Wire verify/unverify for simple results to the entry surface, and guard test removal after review, printing, or delivery
- Gap IDs: GAP-001
- Each covered function marked: GAP-001 DIFFERENT
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because the phase has two parts. Part one is user-interface wiring over verify/unverify commands that already exist (`ReviewResultCommandHandler`, `UnreviewResultCommandHandler`, `MarkAllPatientResultsReviewedCommandHandler`); profile and culture verify paths are already wired, and only the simple-result entry surface lacks the action. Part two is the OD-02 backend hardening: a guard added to three existing test-removal handlers, which changes existing registration behaviour. Risk is medium because of that behaviour change; there is no schema change.

**B. Dependency position**

- Why now: uses existing commands, entities, and permissions; no schema change; independent of all other phases. Can be done early. The OD-02 hardening is placed here because it needs no schema change and touches only the patient-registration removal handlers.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/ResultsEntry/Commands/ReviewResult`, `Application/Features/ResultsEntry/Commands/UnreviewResult`, `Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs`; for the OD-02 hardening, `Application/Features/PatientRegistration/Commands/ClearAllTests/ClearAllTestsCommandHandler.cs`, `Application/Features/PatientRegistration/Commands/RemoveTestFromVisit/RemoveTestFromVisitCommandHandler.cs`, and `Application/Features/PatientRegistration/Commands/ApplyVisitDeltas/ApplyVisitDeltasCommandHandler.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: result-entry surface; the three patient-registration test-removal paths; no shared file with report-visual phases.

**C. Required reference behaviour**

- GAP-001: verify/unverify results (reference guide, pages 28, 54): mark a result as reviewed/verified and reverse that, for simple single-value results as for profile and culture.
- User workflow: open simple result entry for an entered result, verify it, and later unverify it.

**D. Current Top-Lab behaviour**

- `ReviewResultCommandHandler`/`UnreviewResultCommandHandler` exist but have no sender in `src/TopLab.Presentation`; `SimpleResultEntryViewModel.cs` exposes only save, clear, and comment-picker (static inspection). Profile and culture verify paths are wired (`ProfileEntryViewModel.cs:303,340`, `CultureEntryViewModel.cs:404`).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: verify/unverify for simple results. Current: command exists but unreachable from the simple-result surface. Delta: wire existing commands to the entry surface with permission checks.

**F. Exact implementation changes by layer**

- Presentation: `SimpleResultEntryViewModel` --- verify/unverify commands bound to `ReviewResultCommand`/`UnreviewResultCommand`; permission check (`REVIEW_RESULTS`).
- Application: no change (commands exist); ensure authorization behaviour applies.
- Hardening task (OD-02, Option A): add explicit `IsReviewed || IsPrinted || PrintCount > 0 || IsDelivered` guard to all three test-removal paths:
  - `Application/Features/PatientRegistration/Commands/ClearAllTests/ClearAllTestsCommandHandler.cs` --- at lines 80-83, the test-level guard checks `tests.Any(t => t.EnteredAtUtc.HasValue || t.ResultValue is not null || t.ResultFlag.HasValue)` and returns `Error.Conflict`; extend that predicate to include `t.IsReviewed || t.IsPrinted || t.PrintCount > 0 || t.IsDelivered`. Keep the two earlier guards unchanged: first-registration-only and the 24-hour window.
  - `Application/Features/PatientRegistration/Commands/RemoveTestFromVisit/RemoveTestFromVisitCommandHandler.cs` --- lines 25-28; extend the existing guard `pt.EnteredAtUtc.HasValue || pt.ResultValue is not null || pt.ResultFlag.HasValue` with `|| pt.IsReviewed || pt.IsPrinted || pt.PrintCount > 0 || pt.IsDelivered`.
  - `Application/Features/PatientRegistration/Commands/ApplyVisitDeltas/ApplyVisitDeltasCommandHandler.cs` --- lines 58-61; extend the existing guard with `|| pt.IsReviewed || pt.IsPrinted || pt.PrintCount > 0 || pt.IsDelivered`. The corresponding deletion is at line 121.
- No Domain or Infrastructure changes.
- Migration: NO MIGRATION. `IsReviewed`, `IsPrinted`, `PrintCount`, and `IsDelivered` are existing mapped columns of `PatientTest`; the guard is a query predicate over existing columns. No other code removes a `PatientTest`.

**G. Migration decision: NO MIGRATION**

- Reason: commands, handlers, entities, and permissions already exist; only UI wiring and query-predicate guards in three existing handlers are added. No schema change required.

**H. Business rules and message text**

- Validation: only entered results can be verified; verified results can be unverified unless printed/delivered (existing domain locks `PatientTest.cs:129-153`). For the ClearAllTests guard, return `Error.Conflict` with the message: "لا يمكن مسح تحاليل تم تسجيل نتائج لها أو اعتمادها أو طباعتها أو تسليمها." For RemoveTestFromVisit and ApplyVisitDeltas, return `Error.Conflict` with the message: "لا يمكن حذف تحليل تم تسجيل نتيجته أو اعتماده أو طباعته أو تسليمه." Update existing tests that assert the old message text only where they assert that text; do not alter unrelated tests.

**I. Objective acceptance criteria**

- Given an entered simple result, when the user verifies it, then `IsReviewed` is set and the command succeeds.
- Given a verified simple result, when the user unverifies it, then `IsReviewed` is cleared.
- Given a printed/delivered result, then unverify is rejected by the domain lock.
- Hardening (OD-02): given a reviewed/printed/delivered test, when the user attempts to remove it, then the command is rejected with a conflict error.
- Hardening (OD-02): given an entered-but-not-reviewed test, when the user removes it, then the command succeeds.

**J. Test plan**

- Application: verify/unverify handler tests for simple results.
- Presentation: view model command wiring test.
- Hardening (OD-02): removal-guard tests for ClearAllTests, RemoveTestFromVisit, and ApplyVisitDeltas covering reviewed/printed/delivered states.
- Regression: existing verify tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: the removal guard changes existing registration behaviour. Workflows that currently allow removing a reviewed, printed, or delivered test will be rejected. Existing removal tests must be re-run, and changed only where they relied on the old behaviour.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 12 --- Culture sensitivity column control and antibiotic of choice

**A. Phase identification**

- Phase number: 12
- Descriptive title: Culture report column control and antibiotic of choice
- Gap IDs: GAP-002, GAP-012
- Each covered function marked: GAP-002 DIFFERENT, GAP-012 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because both functions touch the culture report section (`ReportCultureSection`) and the sensitivity grid; one migration can add the commercial-name visibility flag and the antibiotic-of-choice field. Medium risk because the grid currently excludes commercial name by design.

**B. Dependency position**

- Why now: both functions share the culture report section and one migration; independent of report-visual phases. Can proceed after culture entry grid exists (it does).
- Prerequisite phases or existing conditions: none (culture entry grid and report section exist).
- Exact code dependencies: `Application/Features/CultureResults/Queries/GetCultureReport`, `Application/Features/CultureResults/Common/CultureResultDtos.cs` (`CultureSensitivityRowDto`), `SaveCultureResultsCommand` and its handler, `Domain/Results/CultureAntibioticResult`, `Presentation/ViewModels/CultureEntryViewModel.cs` and its `CultureSensitivityRow` class, `Domain/Tests/Antibiotic.cs`, `Infrastructure/Printing/ReportCultureSection.cs` and its `ReportCultureSensitivityRow` (including the grid).
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: culture report grid shared with GAP-002/GAP-012.

**C. Required reference behaviour**

- GAP-002: control display of Sensitivity / Reference / Commercial Name columns (reference guide, page 39): toggle which of the three columns appears in the culture report.
- GAP-012: select the antibiotic of choice (reference guide, page 39): mark the preferred antibiotic for the culture.
- User workflow: open culture entry/report settings, toggle column visibility, select antibiotic of choice; culture report renders selected columns and marks the chosen antibiotic.

**D. Current Top-Lab behaviour**

- `CultureAntibioticResult` currently has `PatientTestId`, `AntibioticId`, nullable `SensitivityCategory`, and nullable `InhibitionZoneMm` decimal(4,1); its `PatientTestId` index is plain and non-unique. The application layer currently enforces one row per antibiotic per culture. Master data exists (`Antibiotic.Symbol/ScientificName/     CommercialName`; latest migration adds `CommercialName`), but the report grid is fixed at four columns with no commercial-name column or toggle (`ReportCultureSection.cs:113-124`). There is no choice/preferred concept in `CultureSensitivityRowDto`, `CultureSensitivityRow`, or `ReportCultureSensitivityRow`, and no per-row command in `CultureEntryViewModel` (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: three-column toggle and antibiotic-of-choice mark. Current: fixed grid, no choice. Delta: persist three visibility flags and one choice field per culture result; render grid with selected columns and mark the choice.

**F. Exact implementation changes by layer**

- Domain: `CultureAntibioticResult` --- add non-nullable `IsAntibioticOfChoice` bool (default false); `ReportSettings` --- add non-nullable `ShowSensitivityColumn`, `ShowReferenceColumn`, and `ShowCommercialNameColumn` bools (default true).
- Application: update `CultureSensitivityRowDto`, `SaveCultureResultsCommand` and its handler. The handler currently updates rows keyed by `AntibioticId`, inserts new rows, removes stale rows, and performs one `SaveChangesAsync`; add a domain method on `CultureAntibioticResult` and handler logic that clears the choice on the other rows of the same culture before setting the selected row, within that existing save.
- Infrastructure: update `ReportCultureSensitivityRow` and `ReportCultureSection` (including its four-column grid) to render the choice and selected columns. Do not add a unique filtered database index by default: a unique index is checked per statement, so moving the flag between rows in one save can fail depending on update order; comparable invariants are enforced in this codebase at application level. DESIGN default: application-layer invariant, no filtered index.
- Presentation: update the `CultureSensitivityRow` view-model class and `CultureEntryViewModel` with a row selection control; there is no existing per-row command, so the existing `SaveCommand` saves the whole culture.
- Tests: DTO/command-handler and domain tests for selecting a choice, clearing the previous choice in the same culture, and one-save update; view-model selection tests; report grid tests for selected columns and choice marker; regression: existing culture report tests unchanged.
- Migration: one new migration covering both `CultureAntibioticResults` and `ReportSettings` table additions.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add non-nullable `IsAntibioticOfChoice` bool (default false) to `CultureAntibioticResults`; add non-nullable `ShowSensitivityColumn`, `ShowReferenceColumn`, and `ShowCommercialNameColumn` bools (default true) to `ReportSettings`. DESIGN default: no unique filtered index; no `HasFilter` precedent exists in the configurations folder and a unique index could fail during a choice switch within one save.

- Back-fill: defaults preserve existing behaviour (show all three columns and no antibiotic selected).

- Compatibility: additive only.

- Evidence state not already persisted: no `IsAntibioticOfChoice` column on `CultureAntibioticResults`; no visibility flag columns on `ReportSettings`.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

- Settings seed convention: `SystemSettings`, `ReportSettings`, and `ReceiptSettings` seed their single row with `HasData` using an anonymous object that lists every property; every non-nullable column added to these entities must also be added to that seed object (nullable columns may be listed as `null`), and the matching `...SettingsConfiguration.cs` must be updated in the same phase, otherwise the EF model fails validation.

**H. Business rules and message text**

- Validation: at least one column must remain visible; a choice must refer to an antibiotic attached to the same `PatientTest`; selecting a new choice clears the previous choice for that culture in the application layer before `SaveChangesAsync`. No filtered unique index is used by the DESIGN default; if an owner later chooses an index, document a safe two-step write order and a rollback test.

**I. Objective acceptance criteria**

- Given culture report settings, when the user hides the Reference column, then the grid renders without it.
- Given the same settings, when the user shows the Commercial Name column, then it appears in the grid.
- Given a culture with an antibiotic marked as choice, then the report marks it as the antibiotic of choice.
- Given a user switches the choice to another antibiotic in the same culture, then the prior choice is cleared and the new choice is saved through the existing single `SaveChangesAsync` operation.

**J. Test plan**

- Application: test `CultureSensitivityRowDto`, `SaveCultureResultsCommand` and its handler; verify the handler updates existing rows keyed by `AntibioticId`, inserts new rows, removes stale rows, and clears the prior choice before its single `SaveChangesAsync`.
- Presentation: test `CultureSensitivityRow` and `CultureEntryViewModel` selection control (there is no existing per-row command; `SaveCommand` saves the whole culture).
- Infrastructure: test `ReportCultureSensitivityRow` and `ReportCultureSection` rendering for each column combination and the choice marker; migration/model test proving no filtered unique index is configured. Test the application-layer one-choice invariant and a choice switch with one `SaveChangesAsync`.
- Regression: existing culture report tests unchanged with default all-visible and no choice selected.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: grid layout breakage when columns toggle, or choice switching violates the one-choice rule. Mitigation: test all column combinations and enforce the invariant in the domain/handler before the existing single save. A unique filtered index is deliberately omitted by the DESIGN default because statement-level uniqueness can conflict with update order.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 13 --- Worksheet unfinished-only switch

**A. Phase identification**

- Phase number: 13
- Descriptive title: Worksheet unfinished-only / complete switch
- Gap IDs: GAP-004
- Each covered function marked: GAP-004 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because worksheet queries exist; only an unfinished-only parameter is missing. Low risk.

**B. Dependency position**

- Why now: additive query parameter; independent of all other phases. Small and safe to do early.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/WorkSheets/Queries/GetVisitWorkSheetQuery`, `GetWorkSheetByTestGroupQuery`, `GetWorkSheetByWorkGroupLogQuery`, `Presentation/ViewModels/WorkSheets/WorkSheetsViewModel.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: worksheet query path; no schema change.

**C. Required reference behaviour**

- GAP-004: print the worksheet unfinished only or all (reference guide, pages 86, 89): restrict the worksheet to unfinished work via a switch.
- User workflow: open worksheet view, toggle unfinished-only, view/print restricted or complete worksheet.

**D. Current Top-Lab behaviour**

- Worksheet queries exist with no unfinished-only parameter; print command is per-patient; only the outside-laboratory exclusion exists (a different filter) (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: unfinished-only switch. Current: no such parameter. Delta: add unfinished-only option to worksheet queries and surface as a switch on the view.

**F. Exact implementation changes by layer**

- Application: worksheet queries --- add `OnlyUnfinished` boolean parameter; handlers filter `PatientTest` where result not entered.
- Presentation: worksheet view --- switch control and binding.
- Tests: query filter unit test; regression: existing worksheet tests unchanged.
- No Domain or Infrastructure schema changes.

**G. Migration decision: NO MIGRATION**

- Reason: query predicate only over existing `PatientTest` columns (`EnteredAtUtc`/`ResultValue`). No schema change required.

**H. Business rules and message text**

- Validation: unfinished defined as no result entered; consistent with existing outside-laboratory exclusion semantics.

**I. Objective acceptance criteria**

- Given a period with entered and unentered results, when the user toggles unfinished-only, then only unentered results appear on the worksheet.
- Given the same period with the switch off, then all results appear.

**J. Test plan**

- Application: worksheet query filter tests for entered/unentered states.
- Regression: existing worksheet tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 14 --- Print-time reference value override for one patient

**A. Phase identification**

- Phase number: 14
- Descriptive title: Print-time reference value override (single patient test)
- Gap IDs: GAP-003
- Each covered function marked: GAP-003 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because per-patient frozen-range snapshot infrastructure and `RefreshResultReferenceRangeCommand` handler already exist; only the UI sender is missing. Low risk.

**B. Dependency position**

- Why now: uses existing snapshot and command; no schema change; independent of all other phases.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/ResultsEntry/Commands/RefreshResultReferenceRange`, `Application/Features/ResultsEntry/Common/ResultReferenceRangeSource.cs`, `Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs` or report view.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: result-entry/report surface; no shared file with report-visual phases.

**C. Required reference behaviour**

- GAP-003: temporarily override a reference value at print time for one patient (reference guide, pages 101-102): open the reference cell from the report, edit, refresh and accept for that patient only.
- User workflow: open a result's reference cell at print time, edit, refresh to re-resolve the live range, accept; scoped to one patient's test.

**D. Current Top-Lab behaviour**

- Per-patient frozen-range snapshot infrastructure exists (`ResultsEntryDtos.cs:24-52`, `ResultReferenceRangeSource.cs`); `RefreshResultReferenceRangeCommand` handler exists but has no sender in `src/TopLab.Presentation` (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: print-time refresh scoped to one patient. Current: command exists but unreachable. Delta: wire the command to the report/entry surface for a single patient test.

**F. Exact implementation changes by layer**

- Presentation: report/entry view --- refresh-reference action bound to `RefreshResultReferenceRangeCommand` for the selected patient test.
- Application: no change (command exists).
- Tests: refresh command test scoped to one patient; regression: existing snapshot tests unchanged.
- No Domain or Infrastructure changes.

**G. Migration decision: NO MIGRATION**

- Reason: snapshot model and command already exist; only UI wiring added. No schema change required.

**H. Business rules and message text**

- Validation: refresh allowed for entered results; scoped to one patient's test only (existing `PatientTestReferenceRangeSnapshot`).

**I. Objective acceptance criteria**

- Given an entered result, when the user refreshes its reference range at print time, then the snapshot for that patient's test is re-resolved and re-frozen.
- Given another patient's test, then its snapshot is unaffected.

**J. Test plan**

- Application: refresh command test verifying single-patient scoping.
- Regression: existing snapshot tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 15 --- Barcode label size configuration

**A. Phase identification**

- Phase number: 15
- Descriptive title: Barcode label size configuration and the global vertical-coding setting
- Gap IDs: GAP-021
- Each covered function marked: GAP-021 MISSING
- Estimated size: low to medium
- Estimated risk: medium
- Rationale: the settings surface is small, but the dedicated label PDF currently uses a 288 x 144 point page (4 x 2 inches = 101.6 x 50.8 mm), while the reference stock is 38 x 25 mm. The implementation must change the physical page geometry and verify fitting, margins, text legibility, and printer output. The same migration adds the `EnableVerticalCoding` setting used by Phase 17 (OD-03). The shared bitmap renderer is also used by other PDFs, so those outputs must be protected by regression tests.

**B. Dependency position**

- Why now: `BarcodeLabelRenderer.Render` accepts internal width/height parameters; configurable physical dimensions must be introduced before Phase 16 (sample-material labels), so the multi-label phase uses the same measured stock-size mapping rather than hard-coded dimensions.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Infrastructure/Barcode/BarcodeLabelRenderer.cs`, `Infrastructure/Barcode/BarcodeService.cs` (`BuildLabelPdf`), `Application/Features/PatientRegistration/Commands/PrintBarcode/PrintBarcodeCommandHandler.cs`, `Domain/Settings/SystemSettings.cs`, and `Infrastructure/Persistence/Configurations/SystemSettingsConfiguration.cs`.
- Downstream phases that depend on it: Phases 16 and 17.
- Shared state or execution paths affected: dedicated label printing and `SystemSettings`; `BarcodeLabelRenderer.Render` is also used by `EnvelopePdfWriter`, `InvoicePdfWriter`, `LabOrderPdfWriter`, and `ReceiptPdfWriter` to embed barcodes. Physical sticker dimensions must apply only to the dedicated label PDF, not to those embedded barcode images.

**C. Required reference behaviour**

- GAP-021: sticker size configuration (reference showcase, page 3): label stock size documented as 38 mm x 25 mm.
- User workflow: open label settings, set width and height; labels render at configured size.

**D. Current Top-Lab behaviour**

- `BarcodeService.BuildLabelPdf` currently creates a 288 x 144 point PDF page (4 x 2 inches = 101.6 x 50.8 mm), with a 12 point margin, a 20 point bottom text strip, and a 9 point identifier line. The `/MediaBox` defines the physical PDF page size.
- `BarcodeLabelRenderer.Render(content, width = 300, height = 80)` produces a bitmap in pixels and has no DPI setting; its 300 x 80 dimensions are not physical millimetres. The renderer is also called at its defaults by the envelope, invoice, lab-order, and receipt PDF writers to embed barcodes.
- No physical label-size setting, command, DTO, or view model exists in the inspected source. The reference specifies 38 x 25 mm stock; physical millimetres, PDF points, and bitmap pixels must remain distinct units.
- Verification status: STATICALLY VERIFIED in the prior audit outputs; this document review did not re-run source inspection or build/tests.

**E. Exact behaviour delta**

- Reference: configurable label size (38 x 25 mm documented). Current: a fixed 101.6 x 50.8 mm dedicated-label PDF page and a bitmap renderer whose pixel dimensions have no physical DPI. Delta: persist width/height in millimetres, calculate the dedicated PDF page in points using `mm * 72 / 25.4`, and fit the barcode/text into the page without changing the embedded-barcode output of other PDFs.

**F. Exact implementation changes by layer**

- Domain: `SystemSettings` --- add non-nullable `LabelWidthMm` and `LabelHeightMm` columns of type `decimal(4,1)` (physical millimetres, defaults 38.0 and 25.0; DESIGN choice, matching existing millimetre decimal(4,1) columns and supporting values up to 999.9) and non-nullable `EnableVerticalCoding` boolean (default false). Keep physical millimetres distinct from renderer pixels and PDF points.
- Application: settings command/handler for label size and vertical-coding toggle.
- Infrastructure: `BarcodeService.BuildLabelPdf` --- use `LabelWidthMm` and `LabelHeightMm` to calculate the dedicated label PDF page `MediaBox` in points (`millimetres * 72 / 25.4`), then fit the existing barcode bitmap and human-readable text within the physical label without distortion. Keep the shared `BarcodeLabelRenderer.Render` defaults and embedded-barcode path unchanged for envelope, invoice, lab-order, and receipt PDFs; those documents use the renderer too and must not inherit the physical sticker-page dimensions. If renderer changes are required, introduce a separate explicit path for dedicated labels rather than silently changing shared embedded output. Changing the dedicated label page from 4 x 2 inches to the reference's 38 x 25 mm is intentional; preserve barcode content and horizontal orientation, not the old physical page size. Update the EF configuration for new settings.
- Presentation: settings view --- width/height controls and vertical-coding toggle.
- Tests: label-size settings persistence; vertical-toggle persistence; PDF page-size conversion, bitmap fit, and aspect-ratio tests. Update tests that assert the old 4 x 2 inch dedicated-label PDF geometry; retain regression tests for barcode content and horizontal orientation, and add output-regression tests proving embedded barcodes in envelope, invoice, lab-order, and receipt PDFs are unchanged.
- Migration: one new migration for label size columns and vertical-coding toggle.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add non-nullable `LabelWidthMm` (`decimal(4,1)`, default 38.0), `LabelHeightMm` (`decimal(4,1)`, default 25.0), and `EnableVerticalCoding` (boolean, default false) to `SystemSettings`. The defaults must be applied to the existing singleton settings row by the migration.

- Back-fill: set the existing singleton settings row to 38.0 × 25.0 mm and `EnableVerticalCoding = false`. The current renderer defaults (300 × 80 pixels) and PDF page (288 × 144 points = 4 × 2 inches) are different units and must not be interpreted as millimetres. Compute the PDF page dimensions from the configured physical millimetres; independently choose sufficient bitmap pixel dimensions and fit the barcode/text without distortion. The physical page size intentionally changes to the reference default, while barcode content and horizontal orientation remain unchanged.

- Compatibility: additive only.

- Evidence state not already persisted: no label size columns or vertical-coding toggle column exist on `SystemSettings`.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

- Settings seed convention: `SystemSettings`, `ReportSettings`, and `ReceiptSettings` seed their single row with `HasData` using an anonymous object that lists every property; every non-nullable column added to these entities must also be added to that seed object (nullable columns may be listed as `null`), and the matching `...SettingsConfiguration.cs` must be updated in the same phase, otherwise the EF model fails validation.

**H. Business rules and message text**

- Validation: physical width/height must be positive millimetre values within printer capability; convert them to internal renderer dimensions in one documented mapping, preserving aspect ratio.

**I. Objective acceptance criteria**

- Given label settings, when the user sets physical width/height in millimetres, then the generated PDF `MediaBox` corresponds to those physical dimensions (within rounding tolerance), and the barcode/text fit without distortion.
- Given default settings, the generated PDF page is 38.0 × 25.0 mm, not the current 4 × 2 inches; the barcode content and horizontal orientation remain unchanged. Tests explicitly assert the new physical page size, unit conversion, decimal(4,1) setting values/defaults, aspect-ratio preservation, and sensible margins/text fit for the minimum supported size.

**J. Test plan**

- Application: label settings persistence tests, including `decimal(4,1)` width/height persistence and the 38.0 / 25.0 defaults.
- Infrastructure: test millimetre-to-point page sizing, renderer pixel sizing, aspect-ratio preservation, and content fit.
- Regression: update tests that assert the old 4 × 2 inch PDF geometry; preserve tests for barcode content and horizontal orientation.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: the default physical page changes from 101.6 x 50.8 mm to the reference's 38 x 25 mm; incorrect fit, unreadable text, or printer scaling could make labels unusable. Mitigation: use the reference dimensions as defaults, validate supported ranges, assert PDF page geometry in tests, and require a physical-printer sample check by the owner before operational rollout. This document review did not run a physical printer test.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 16 --- Barcode labels per sample material with doctor-editable test distribution

**A. Phase identification**

- Phase number: 16
- Descriptive title: Barcode labels per sample material with doctor-editable test distribution
- Gap IDs: GAP-009
- Each covered function marked: GAP-009 DIFFERENT
- Estimated size: high
- Estimated risk: medium
- Rationale: high size because it introduces a new `SampleMaterial` entity, per-test assignment, doctor-editable distribution UI, and multi-label print payload. Medium risk because the existing single-identifier path must keep working.

**B. Dependency position**

- Why now: depends on Phase 15 (label size parameters). Terminates in `BarcodeLabelRenderer.Render`; its output shape is defined by Phase 15's parameters. Must not be attempted before Phase 15.
- Prerequisite phases or existing conditions: Phase 15 complete. Existing `PrintBarcodeCommand`/`BarcodeService` single-identifier barcode-printing path works.
- Exact code dependencies: `Application/Features/PatientRegistration/Commands/PrintBarcode`, `Infrastructure/Barcode/BarcodeService.cs`, `Infrastructure/Barcode/BarcodeLabelRenderer.cs`, new `SampleMaterial` entity and assignment table.
- Downstream phases that depend on it: Phase 17 (vertical coding reuses the label primitive).
- Shared state or execution paths affected: barcode printing path; existing single-identifier path must remain intact.

**C. Required reference behaviour**

- GAP-009: barcode labels for tubes, containers and swabs with doctor-editable test distribution (reference showcase, page 3): program distributes the patient's tests across drawn materials; doctor can change distribution; 38 mm x 25 mm stock; reprintable.
- User workflow: open a patient's sample-material board, distribute tests across tubes/containers/swabs, edit distribution, print one label per material; reprintable.

**D. Current Top-Lab behaviour**

- `IBarcodeService.PrintBarcodeAsync(string value)` renders one fixed-size Code-128 label for the patient or Lab Id; no sample-material entity, no per-material assignment, no distribution UI (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: per-material labels with doctor-editable distribution. Current: one label per patient. Delta: introduce sample materials, per-test assignment, distribution UI, multi-label payload; keep existing single-identifier path working.

**F. Exact implementation changes by layer**

- Domain: new `SampleMaterial` entity (`Name` required string max 100; `Description` nullable string? max 500); assignment entity `PatientTestSampleMaterial` (`PatientTestId` FK, `SampleMaterialId` FK, `AssignedAtUtc` required `datetime2`) is the single source of truth for the current assignment. Configure `PatientTestId` with `HasOne<PatientTest>().WithMany().HasForeignKey(e => e.PatientTestId)     .OnDelete(DeleteBehavior.Cascade)` and `SampleMaterialId` with `HasOne<SampleMaterial>().WithMany().HasForeignKey(e => e.SampleMaterialId)     .OnDelete(DeleteBehavior.Restrict)`. Add a unique index with `b.HasIndex(e => e.PatientTestId).IsUnique()`. Do not also add `SampleMaterialId` directly to `PatientTest`, because that would duplicate the same relationship in two places and could allow conflicting state.
- Application: commands to assign tests to materials, set distribution; query to list materials per patient.
- Infrastructure: EF configuration for `SampleMaterials` and `PatientTestSampleMaterials` tables; `BarcodeService` multi-label payload; printer dispatch.
- Presentation: material board view/view model; doctor-editable distribution; print trigger.
- Tests: material assignment tests; multi-label payload test; regression: existing `PrintBarcode` tests unchanged.
- Migration: one new migration for `SampleMaterials` and `PatientTestSampleMaterials` tables.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add `SampleMaterials` (`SampleMaterialId` int PK, `Name` required string max 100, `Description` nullable string? max

  500) and `PatientTestSampleMaterials` (`PatientTestSampleMaterialId` int PK, `PatientTestId` int FK Cascade, `SampleMaterialId` int FK Restrict, `AssignedAtUtc` required `datetime2`). Configure both FKs with explicit `HasOne<T>().WithMany().HasForeignKey(...).OnDelete(...)`. Add a unique index using `b.HasIndex(e => e.PatientTestId).IsUnique()`. Do not add a `SampleMaterialId` column to `PatientTests`.

- Back-fill: none required; both tables are new and start empty.

- Compatibility: additive only; this phase adds two new tables and does not alter the existing `PatientTests` schema.

- Evidence state not already persisted: no sample-material tables exist.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

**H. Business rules and message text**

- Validation: a test is assigned to at most one material at a time, enforced by the unique `PatientTestId` index; material must exist before assignment. `AssignedAtUtc` is a required system-set `datetime2` UTC timestamp. The assignment table is the only source of truth for the current assignment.

**I. Objective acceptance criteria**

- Given a patient with tests, when the user distributes tests across materials, then one label per material is produced with the assigned tests.
- Given the same patient, when the user reprints, then labels are reproducible.
- Regression: existing single-identifier barcode print still works.

**J. Test plan**

- Domain/Application: material assignment unit tests.
- Infrastructure: multi-label PDF payload test.
- Presentation: material board view model tests.
- Regression: existing barcode printing tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: breaking the existing single-identifier path. Mitigation: keep `PrintBarcodeCommand` unchanged and add a separate multi-material path.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 17 --- Optional vertical identity coding

**A. Phase identification**

- Phase number: 17
- Descriptive title: Optional vertical identity coding on result report, receipt, and envelope
- Gap IDs: GAP-020
- Each covered function marked: GAP-020 MISSING
- Estimated size: low
- Estimated risk: medium
- Rationale: low size because it is a rendering mode of the identity mark. Medium risk because it touches three writers and rotation interacts with page geometry.

**B. Dependency position**

- Why now: vertical coding is a rendering mode of the identity mark that Phase 16's material labelling also emits. Implement after Phase 16 so it is not redone for the per-material label.
- Prerequisite phases or existing conditions: Phase 15 complete (`SystemSettings.EnableVerticalCoding` and configurable physical label size); Phase 16 complete (sample-material label primitive and material distribution exist).
- Exact code dependencies: `Infrastructure/Printing/ReportPdfWriter.cs`, `Infrastructure/Printing/ReceiptPdfWriter.cs`, `Infrastructure/Printing/EnvelopePdfWriter.cs`, `Domain/Settings/SystemSettings.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: three writers (report, receipt, envelope).

**C. Required reference behaviour**

- GAP-020: optional vertical identity coding on the result report, receipt and envelope (reference showcase, page 3): vertical identity coding printed on documents, explicitly optional.
- User workflow: enable vertical coding in settings; identity mark renders vertically/rotated on the three documents.

**D. Current Top-Lab behaviour**

- No setting, command, or writer branch for vertical/rotated identity coding; the only barcode path renders horizontally on a single label (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: optional vertical coding on three documents. Current: none. Delta: add a persisted toggle and a vertical/rotated rendering mode applied to the identity mark on all three documents.

**F. Exact implementation changes by layer**

- Domain: none (vertical-coding toggle column `EnableVerticalCoding` already added to `SystemSettings` by Phase 15).
- Application: settings command/handler for the toggle.
- Infrastructure: three writers --- add rotation/vertical-text mode for the identity mark.
- Presentation: settings view --- toggle control.
- Tests: vertical rendering test per writer; regression: existing horizontal barcode/report tests unchanged with toggle off.

**G. Migration decision: NO MIGRATION**

- Reason: the `EnableVerticalCoding` toggle column on `SystemSettings` was added by Phase 15; vertical coding is a rendering mode applied to the existing identity mark. No new schema required for this phase.

**H. Business rules and message text**

- Validation: rotation angle supported by QuestPDF; page geometry check.

**I. Objective acceptance criteria**

- Given vertical coding enabled, when the identity mark is rendered on the report/receipt/envelope, then the mark is rotated vertical.
- Given vertical coding disabled (default), then the mark renders horizontally as today.

**J. Test plan**

- Infrastructure: vertical rendering test for each of the three writers.
- Regression: existing horizontal rendering tests unchanged with mode off.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: rotation interacting with page geometry. Mitigation: test each writer's page layout.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — OD-03 Option A is resolved (one global identity-coding toggle); implementation is not yet complete.**

### Phase 18 --- Sample-nature interpretation panel

**A. Phase identification**

- Phase number: 18
- Descriptive title: Sample-nature interpretation panel (nature, normal ranges, effect on patient)
- Gap IDs: GAP-022
- Each covered function marked: GAP-022 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because a new entity, read surface, and authored interpretation text. Risk medium because the clinical content is editorial, not technical.

**B. Dependency position**

- Why now: independent of all other phases; depends only on catalogue and range data that already exist. Can be done at any point.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Domain/Tests/Test.cs`, `Domain/Results/ReferenceRange.cs`, new sample-nature interpretation entity.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: catalogue/range read path; no shared file with other phases.

**C. Required reference behaviour**

- GAP-022: interpret sample nature, its normal ranges, and its effect on the patient (reference showcase, page 51): sample-nature interpretation panel.
- User workflow: open interpretation panel for a sample, view nature, normal ranges, and clinical meaning.

**D. Current Top-Lab behaviour**

- `Test` carries collection/group labels and `ReferenceRange` supplies intervals, but no interpretation entity, command, query, or panel presents sample nature with ranges and meaning (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: interpretation panel. Current: absent. Delta: introduce sample-nature interpretation records with a read surface combining nature, ranges, and interpretation text.

**F. Exact implementation changes by layer**

- Domain: new `SampleNatureInterpretation` entity (`Nature` required string max 100, `NormalRanges` required string max 500, `InterpretationText` required string max 1000); linked to `Test` via `TestId` foreign key configured as `b.HasOne<Test>().WithMany().HasForeignKey(e => e.TestId)     .OnDelete(DeleteBehavior.Restrict)`. All three string columns are non-nullable and configured with `IsRequired()`.
- Application: query to read interpretation for a sample/test.
- Infrastructure: EF configuration for `SampleNatureInterpretations` table.
- Presentation: interpretation panel view/view model.
- Tests: interpretation read test; regression: existing catalogue/range tests unchanged.
- Migration: one new migration for `SampleNatureInterpretations` table.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add `SampleNatureInterpretations` table (`SampleNatureInterpretationId` int PK, `TestId` int FK Restrict, `Nature` required string max 100, `NormalRanges` required string max 500, `InterpretationText` required string max 1000). Configure the FK with `b.HasOne<Test>().WithMany().HasForeignKey(e => e.TestId)     .OnDelete(DeleteBehavior.Restrict)` and all three strings with `IsRequired()`.

- Back-fill: none required (new table; editorial content added separately).

- Compatibility: additive only.

- Evidence state not already persisted: no interpretation table exists.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

**H. Business rules and message text**

- Validation: interpretation text linked to a valid test/sample nature.

**I. Objective acceptance criteria**

- Given a sample/test, when the user opens the interpretation panel, then the sample nature, normal ranges, and clinical meaning are displayed.
- Editorial content risk: interpretation text must be authored/maintained (recorded in phase risk).

**J. Test plan**

- Application: interpretation query unit tests.
- Regression: existing catalogue/range tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: clinical content correctness (editorial). Mitigation: content authored by domain experts; code only stores/displays.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 19 --- Glossary of analysis abbreviations

**A. Phase identification**

- Phase number: 19
- Descriptive title: Glossary of analysis abbreviations
- Gap IDs: GAP-023
- Each covered function marked: GAP-023 MISSING
- Estimated size: low
- Estimated risk: medium
- Rationale: low size because new entity/CRUD/lookup surface, same shape as Phase 18. Risk medium because editorial, not technical.

**B. Dependency position**

- Why now: independent of all other phases; same shape as Phase 18. Can be done at any point.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: new glossary entity; `Domain/Tests/Test.Short`/`TestCode` are catalogue heading fields (read-only reference).
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: catalogue read path; no shared file with other phases.

**C. Required reference behaviour**

- GAP-023: glossary of abbreviations used in medical analysis (reference showcase, page 51): terminology lookup.
- User workflow: open glossary, look up an abbreviation and its full term/notes.

**D. Current Top-Lab behaviour**

- `Test.Short`/test codes are catalogue heading fields; no glossary entity, command, query, or definition view exists (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: glossary lookup. Current: absent. Delta: introduce glossary records (abbreviation, full term, notes) with CRUD and a lookup surface.

**F. Exact implementation changes by layer**

- Domain: new `GlossaryEntry` entity (`Abbreviation` string max 100, PK; `FullTerm` string max 200; `Notes` string max 1000 nullable).
- Application: CRUD commands/queries.
- Infrastructure: EF configuration for `GlossaryEntries` table.
- Presentation: glossary view/view model; lookup bindings.
- Tests: glossary CRUD tests; regression: existing catalogue tests unchanged.
- Migration: one new migration for `GlossaryEntries` table.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add `GlossaryEntries` table (`Abbreviation` string max 100 PK, `FullTerm` string max 200, `Notes` string max 1000 nullable).

- Back-fill: none required (new table; editorial content added separately).

- Compatibility: additive only.

- Evidence state not already persisted: no glossary table exists.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

**H. Business rules and message text**

- Validation: abbreviation unique.

**I. Objective acceptance criteria**

- Given the glossary, when the user looks up an abbreviation, then the full term and notes are displayed.
- Editorial content risk recorded.

**J. Test plan**

- Application: glossary CRUD unit tests.
- Regression: existing catalogue tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: editorial content correctness. Mitigation: content authored by domain experts.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

### Phase 20 --- Appointment-note reminder inside the application

**A. Phase identification**

- Phase number: 20
- Descriptive title: Appointment-note reminder (in-application only)
- Gap IDs: GAP-024
- Each covered function marked: GAP-024 MISSING
- Estimated size: medium
- Estimated risk: high
- Rationale: medium size because new reminder entity and pending-reminder surface. Risk high because no notification infrastructure exists; this is a greenfield feature, although the scheduling decision (OD-04) has been resolved.

**B. Dependency position**

- Why now: independent of all other phases. Sequenced last as a self-contained in-application feature that can be implemented after the other planned functionality. This ordering does not depend on an unresolved notification-channel decision; OD-04 is resolved and external notification channels are explicitly excluded. The phone-book contract is `PhoneBookEntryDto(int Id, string Name, string Phone,     string? Notes)`; IDs are assigned as `Max(Id)+1`, can be reused after deletion, and the JSON store is workstation-local rather than per-user. `IPhoneBookStore` exposes only `GetAllAsync` and `SaveAllAsync`; it does not provide a stable relational identity or per-user store.
- Prerequisite phases or existing conditions: none. Phone book exists (`JsonPhoneBookStore`, `GetPhoneBookQuery`) and is unaffected.
- Exact code dependencies: `Application/Features/Utilities/Queries/GetPhoneBookQuery`, new reminder entity.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: phone book read path (read-only reference); no shared file with other phases.

**C. Required reference behaviour**

- GAP-024: appointment-note reminder to a user or phone-book contact (reference showcase, page 51): send a reminder note through the appointment notebook; delivery is in-application/internal, no external channel.
- User workflow: create a reminder note for a user or phone-book contact with a due date; view pending reminders inside the application; no SMS/e-mail/fax/portal.

**D. Current Top-Lab behaviour**

- Phone-book storage and queries exist, but no reminder, appointment, note-scheduling, or notification entity, command, query, DTO, view, or view model exists anywhere in `src/` (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: in-application reminder. Current: absent. Delta: introduce internal reminders (target, message, due date, state) with management and an in-application pending-reminder surface; no external channel.

**F. Exact implementation changes by layer**

- Domain: new auditable `Reminder` entity with a strongly typed ID. Persist `OwnerUserId` (required `UserId`-typed property with value conversion and required FK to `Users`), `TargetType` (required string max 20, values `User` or `PhoneBookContact`), nullable `TargetContactName` (string? max 200) and `TargetContactPhone` (string? max 30) as snapshots for a phone-book contact, `Message` (required string max 500), `DueDateUtc` (required `datetime2`, system-set UTC), and required `IsDone` bool default false. Configure the user relationship in the `AttendanceRecord` form: `HasOne<User>().WithMany().HasForeignKey(e =>     e.OwnerUserId).OnDelete(DeleteBehavior.Cascade)`, matching the existing AttendanceRecord FK form and required-FK default. Add a check constraint so the contact snapshot fields are null for `User` targets and `TargetContactName` is nonblank for `PhoneBookContact` targets. For a `User` target, `OwnerUserId` is the target user; no duplicate `TargetUserId` column is needed. Do not store a generic integer `TargetId`: the phone book is a workstation-local JSON store at `%ProgramData%\TopLab\phone-book.json`, shared per workstation rather than per user. `PhoneBookEntryDto` uses an `int Id` assigned as `Max(Id)+1`, so an identifier can be reused after deletion. The contact snapshot design is required because phone-book IDs are unstable and workstation-local.
- Ownership rule: if the target is an application user, that selected user is `OwnerUserId` and sees/manages the reminder in their pending list. The audit field `CreatedByUserId` records who created it. If the target is a phone-book contact, the currently signed-in creator is `OwnerUserId`; persist the contact name/phone snapshot so the reminder remains understandable if the local JSON entry changes or is unavailable on another workstation. A contact is context only; no external message is sent. This ownership/visibility rule is an explicit implementation choice added because the reference describes a pending-reminder list but does not specify its user-scoping details; it does not claim that the PDF dictated these exact database fields.
- Application: create/update/mark-done commands and owner-scoped pending query. Enforce the future-due-date rule in the command handler using `IDateTimeProvider.UtcNow`, not in a validator; reject a due date that is not later than the current UTC time with the message `يجب أن يكون تاريخ الاستحقاق في المستقبل.` Commands must verify current-user ownership. DESIGN default: use `ADD_EDIT_PATIENT` as the existing permission code because this reminder is tied to the patient appointment-note workflow; do not introduce new seeded permission types (OD-05).
- Infrastructure: EF configuration for `Reminders`, FK/delete rules, check constraint, and audit inheritance; do not create an FK to the workstation-local JSON phone book.
- Presentation: reminder management view/view model; pending-reminder surface scoped to the current owner.
- Tests: reminder CRUD and ownership/authorization; target-type constraint; command-handler test using a controlled `IDateTimeProvider` for future/past/equal due dates and the exact Arabic rejection message; pending/done query tests; regression: phone-book tests unchanged.
- Migration: one new migration for the `Reminders` table.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add `Reminders` table with the fields and constraints listed in Phase 20.F: generated integer PK stored through a strongly typed domain ID; required `OwnerUserId` FK; `TargetType`; nullable `TargetContactName`/`TargetContactPhone` snapshots; required `Message`; required `DueDateUtc datetime2`; and required `IsDone` boolean default false. Include inherited audit columns because `Reminder` is planned as `AuditableEntity<TId>`.

- Back-fill: none required.

- Compatibility: additive only.

- Evidence state not already persisted: no reminder table exists.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

**H. Business rules and message text**

- Validation: the command handler enforces that `DueDateUtc` is later than `IDateTimeProvider.UtcNow`; do not put this rule in a validator. If invalid, return the Arabic message `يجب أن يكون تاريخ الاستحقاق     في المستقبل.` For a `User` target, the `OwnerUserId` FK identifies the target; a `PhoneBookContact` target must have a nonblank contact-name snapshot (phone optional). Pending queries are scoped to `OwnerUserId`; only that owner may mark the reminder done or edit it, subject to the existing permission model.
- Constraint: no SMS/e-mail/fax/portal delivery channel (owner exclusion). Phone-book contacts are stored as snapshots because the current phone book is workstation-local JSON and has no relational FK.

**I. Objective acceptance criteria**

- Given a reminder targeted at a user, when it is created, then it is visible only in that target user's pending list.
- Given a reminder targeted at a phone-book contact, when it is created, then it appears in the creating user's pending list with the stored contact snapshot, even if the workstation-local phone-book entry later changes or is absent.
- Given the due date has passed, the reminder is shown as due; it remains pending until its owner marks it done.
- A different user cannot read or mark done another owner's reminder.
- No external delivery channel is introduced.

**J. Test plan**

- Application: reminder CRUD, target validation, owner-scoping, authorization, due-date, and pending/done query tests.
- Infrastructure: EF constraint and migration-model tests.
- Presentation: reminder view model tests for owner-scoped lists and mark-done behaviour.
- Regression: phone-book tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: the new reminder workflow has no existing notification infrastructure. Mitigation: follow resolved OD-04 Option A: a one-time reminder with a due date, shown in-app until marked done.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — OD-04 Option A is resolved (one-time reminder with a due date); implementation is not yet complete.**

### Phase 21 --- Banded monitor over blood-picture element values

**A. Phase identification**

- Phase number: 21
- Descriptive title: Banded monitor over blood-picture element values in time periods
- Gap IDs: GAP-025
- Each covered function marked: GAP-025 MISSING
- Estimated size: low
- Estimated risk: low
- Rationale: low size because `ProfileResultItem` and `HistoryMatrixBuilder.Pivot` already exist; extend the monitor from test-level scalars to profile-analyte rows. Low risk.

**B. Dependency position**

- Why now: fully independent of the printing pipeline and every other phase; reuses the existing pivot helper. Can be done at any point.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/Statistics/Queries/GetBandedResultMonitorQueryHandler`, `Domain/Results/ProfileResultItem.cs`, `Application/Features/ReportProduction/Common/HistoryMatrixBuilder.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: banded monitor query path; reads `ProfileResultItem` which the existing monitor never reads.

**C. Required reference behaviour**

- GAP-025: search and statistics on blood-picture element values in time periods (reference showcase, page 73): element-value band search over CBC analytes (HGB/HCT/MCV/MCH/MCHC/RETICS/PLT/PCT/MPV/LYM/MON/NEU/EOS/BAS/BAND) with from/to bands and a statistics action.
- User workflow: open element-value search, choose an analyte and from/to band and a period, view matching results; run statistics action.

**D. Current Top-Lab behaviour**

- The only banded monitor filters `PatientTest.ResultValue` scalars (`GetBandedResultMonitorQueryHandler.cs:57-105`); CBC results live as analyte-level rows in `ProfileResultItem`, which that query never reads (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: element-value band search over CBC analytes. Current: test-level scalar monitor only. Delta: extend banded search from `PatientTest.ResultValue` to `ProfileResultItem` analyte rows with per-element band inputs over a period, reusing `HistoryMatrixBuilder.Pivot`.

**F. Exact implementation changes by layer**

- Application: `GetBandedResultMonitorQuery` --- add element/analyte input and per-element band parameters; handler reads `ProfileResultItem`.
- Infrastructure: none (reads existing table).
- Presentation: element-value search view --- band inputs and period picker.
- Tests: monitor-over-analyte unit test; regression: existing scalar monitor tests unchanged.
- No schema change.

**G. Migration decision: NO MIGRATION**

- Reason: reads `ProfileResultItem`, which already exists (migration `20260909033414_AddAnalyteProfileDomain`). No new column required.

**H. Business rules and message text**

- Validation: band min/max valid; element exists in profile.

**I. Objective acceptance criteria**

- Given a period and an analyte with a from/to band, when the user runs the search, then matching profile-analyte results are listed.
- Given the same inputs, when the user runs the statistics action, then element statistics are computed.
- Existing scalar monitor behaviour unchanged.

**J. Test plan**

- Application: banded monitor over analyte rows unit test.
- Regression: existing scalar banded monitor tests unchanged.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Confirm that no migration artefact or database-schema change is required or introduced by this phase.
5.  Confirm existing migration files remain unchanged.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Verify that no excluded functionality or external delivery channel was introduced.

**L. Risks and rollback**

- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — OD-06 Option A is resolved (extend the existing monitor with element/analyte bands); implementation is not yet complete.**

### Phase 22 --- Per-user report-element visibility toggle

**A. Phase identification**

- Phase number: 22
- Descriptive title: Toggle report elements from a doctor-facing control (per-user visibility)
- Gap IDs: GAP-019
- Each covered function marked: GAP-019 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because a new per-user layout-preferences table is required, and the report builder must consult it at render time. Depends on GAP-014's item-template taxonomy (Phase 7).

**B. Dependency position**

- Why now: depends on GAP-014 (Phase 7) which introduces the item-template taxonomy. Per-user visibility preferences reference report items, so the template must exist first. Placed last because it extends the report builder after the item-template foundation is stable.
- Prerequisite phases or existing conditions: Phase 7 complete (item-template aggregate exists).
- Exact code dependencies: `Infrastructure/Printing/ReportContentBuilder.cs`, new `UserReportLayoutPreferences` table, `Domain/Users/User.cs` (existing).
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: report builder render path; per-user preferences consulted at report build time.

**C. Required reference behaviour**

- GAP-019: toggle report elements from a doctor-facing control (reference showcase, page 15): show or hide any report element on demand (e.g. side address/title, signature, sample block).
- User workflow: open report element visibility settings, show/hide elements; reports render per user preferences.

**D. Current Top-Lab behaviour**

- `ReportSettings` is a single global row with no per-user layout entity; the only per-item switches in the product belong to the envelope; the dialogs folder holds only the secondary-password dialog (static inspection).
- Verification status: STATICALLY VERIFIED.

**E. Exact behaviour delta**

- Reference: per-user report-element visibility toggle. Current: none. Delta: persist per-user report-element visibility preferences and enforce them at report build time.

**F. Exact implementation changes by layer**

- Domain: new `UserReportLayoutPreference` entity with `UserId`-typed property (value conversion to the integer database column), required user relationship, `ReportItemKey` string required max 100 (`IsRequired()`), and `IsVisible` bool non-null default true; composite key (`UserId`, `ReportItemKey`). DESIGN default: `ReportItemKey` remains a plain string column with no database FK to `ReportItemTemplates.ItemKey`, because the repository has no precedent for string-key foreign keys.
- Application: commands/queries to set and read per-user layout preferences; validate keys against `ReportItemTemplates` when saving, and ignore orphan keys during rendering.
- Infrastructure: EF configuration for `UserReportLayoutPreferences` table with explicit `User` FK/Cascade; `ReportContentBuilder` consults preferences when emitting supported line kinds and ignores orphan `ReportItemKey` values at render time.
- Presentation: report element visibility view/view model; toggle controls per element.
- Tests: preference persistence and key-validation tests; render-time filtering test proving orphan `ReportItemKey` values are ignored; regression: existing report tests unchanged with default all-visible.
- Migration: one new migration for `UserReportLayoutPreferences` table.

**G. Migration decision: NEW MIGRATION**

- Intended name: additive migration appended after the latest existing migration; add `UserReportLayoutPreferences` with `UserId`-typed property converted to an integer column and explicit `b.HasOne<User>().WithMany().HasForeignKey(e => e.UserId)     .OnDelete(DeleteBehavior.Cascade)` relationship; `ReportItemKey` string required max 100 (`IsRequired()`); `IsVisible` bool non-null default true; composite PK on (`UserId`, `ReportItemKey`). DESIGN default: no database FK from `ReportItemKey` to `ReportItemTemplates.ItemKey` because there is no string-FK precedent in the codebase.

- Back-fill: none required (new table; defaults to all visible).

- Compatibility: additive only.

- Evidence state not already persisted: no per-user layout preference table exists.

- One migration only for this phase. See the migration handoff protocol in Section 5.1 for the artefact creation and owner hand-off procedure.

**H. Business rules and message text**

- Validation: `UserId` must reference an existing user. The application layer validates `ReportItemKey` against `ReportItemTemplates` when saving a preference; because no string-key FK is used, orphan keys encountered at render time are ignored rather than emitted.

- Default: all items visible unless explicitly hidden by the user.

**I. Objective acceptance criteria**

- Given a user and a report item, when the user hides the item, then subsequent reports for that user exclude the item.
- Given the same user, when the user shows the item again, then subsequent reports include the item.
- Given default preferences, then all report items are visible.

**J. Test plan**

- Application: preference CRUD and query tests.
- Infrastructure: render-time filtering test (preference hides/shows item).
- Regression: existing report tests unchanged with default all-visible.

**K. Phase-end verification gate**

1.  Build the complete solution.
2.  Run the complete relevant test suite.
3.  Run the full regression suite.
4.  Verify the new migration artefacts WITHOUT touching any database: the solution builds with zero warnings and zero errors; the migration, Designer file, and updated model snapshot were generated by EF Core tooling; the snapshot diff contains only the intended phase changes; `Up` and `Down` reverse each other correctly; and no existing migration file was edited.
5.  Confirm no excluded functionality or external delivery channel was introduced.
6.  Verify that the phase acceptance criteria and reference behaviour are satisfied.
7.  Inspect all changed files and confirm that only intended files changed.
8.  Record commands, exit codes, failures, and outputs.
9.  Hand off to the owner and STOP: report the three artefact paths, migration name, schema-change summary, `Up`/`Down` review, snapshot-diff review, and exact Package Manager Console and CLI commands. Do not apply the migration and do not begin the next phase.
10. Owner-only acceptance gate: the owner applies the migration to the real database and confirms with `Get-Migration` that the new migration has `applied = True`. The phase is accepted, and the next phase may start, only after that confirmation.

**L. Risks and rollback**

- Risk: report render performance with many per-user preferences. Mitigation: cache preferences per user session.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

**M. Planning status: READY FOR IMPLEMENTATION — owner decisions resolved; implementation is not yet complete.**

------------------------------------------------------------------------

### 4.1 --- Verified implementation conventions for new code

The following conventions were confirmed by the read-only source inspection performed against the current implementation. New phases must follow them unless a phase explicitly documents why a natural or composite key is the better fit.

1.  **Generated integer keys and domain IDs.** Ordinary domain entities use strongly typed ID wrappers from `src/TopLab.Domain/Common/Ids/`: each ID is a sealed class derived from `StronglyTypedId<TValue>`, has a private constructor, and exposes a public static `Create(...)` factory. For a new generated integer-key entity, add the corresponding strongly typed ID and use `Entity<ThatId>` or `AuditableEntity<ThatId>` as appropriate; the database column can still be SQL `int`. In EF configuration, follow the existing value-conversion pattern and name the PK column `<EntityName>Id`. Do not interpret a database `int` PK in this plan as permission to expose a raw `int` ID throughout the domain.

2.  **Natural and composite keys are deliberate exceptions.** Existing examples include `EnvelopePrintItemPosition` with its string `ItemName` key and link entities configured with composite keys. For `ReportItemTemplate`, `GlossaryEntry`, or `UserReportLayoutPreference`, preserve the planned natural/composite key only if it is intentional and enforce its uniqueness in EF configuration; do not add a redundant surrogate ID merely for uniformity. Match the closest existing key pattern and document the choice.

3.  **Entity construction and validation.** For ordinary aggregate entities, follow the existing pattern illustrated by `Antibiotic`: private parameterless constructor for EF materialization; private constructor for valid state; public static factory; validation/normalization in the factory and update methods; private property setters. Apply the same pattern to new generated-key entities unless the existing domain shape provides a better direct precedent.

4.  **Audit inheritance is a domain decision.** Audit columns are supplied through `AuditableEntity<TId>` / `IAuditableEntity` and `AuditableEntitySaveChangesInterceptor`; they are not individually configured in each EF configuration file. Choose `AuditableEntity<TId>` only where creation/modification audit is appropriate; otherwise use `Entity<TId>` or the documented natural/composite-key pattern. Do not assume that every current entity is auditable.

5.  **DbSet and configuration registration.** Register each new entity in `src/TopLab.Infrastructure/Persistence/ApplicationDbContext.DbSets.cs` using the existing `DbSet<Entity> PluralName => Set<Entity>()` pattern. EF configurations are discovered automatically by `ApplyConfigurationsFromAssembly(...)`, so no separate registration in `ApplicationDbContext.OnModelCreating` is needed. `IApplicationDbContext` exposes generic `Set<TEntity>()` operations and does not require a new entity-specific DbSet declaration.

6.  **Relational configuration, nullability, and settings seeds.** Pair non-nullable C# `string` properties with `IsRequired()` and nullable `string?` properties with `IsRequired(false)`; give strings explicit maximum lengths. Every decimal property must have both explicit `HasColumnType(...)` and `HasPrecision(...)` configuration. System-set UTC timestamps use required `datetime2` and a `Utc` suffix. Two delete behaviour styles exist: use explicit `HasOne<T>().WithMany().HasForeignKey(...).OnDelete(...)` when a non-default behaviour such as `Restrict` is intended; convention-based foreign keys are also used, including in join entities and in `PatientPhoneNumber`, `InvoiceIssue`, and `UserPermissionGrant` (the default for required FKs is Cascade). Configure strongly typed ID conversion and the database PK column name. `SystemSettings`, `ReportSettings`, and `ReceiptSettings` are existing `Entity<int>` exceptions whose singleton rows use `ValueGeneratedNever()` and seeded `Id = 1`. Seed their single row with `HasData` using an anonymous object that lists every property; every non-nullable column added to these entities must also be added to that seed object (nullable columns may be listed as `null`), and the matching `...SettingsConfiguration.cs` must be updated in the same phase, otherwise EF model validation fails.

7.  **Migration discipline.** Existing migration files follow the timestamp-plus-PascalCase naming convention and have paired Designer files; the model snapshot must be updated with each new migration. Follow Section 5.1's artefact handoff protocol: never edit an existing migration, never apply a migration as the implementing agent, and stop after handing the generated migration artefacts to the owner.

8.  **Phase 16 assignment invariant.** `PatientTestSampleMaterials` is the single source of truth for current test-to-material assignment. The unique index on `PatientTestId` enforces at most one current material per test. Do not store the same current assignment again as `PatientTest.SampleMaterialId`; if the product later requires assignment history or multiple simultaneous materials, that is a separate business-rule change requiring an explicit design decision.

9.  **Phase 20 reminder state.** The resolved owner decision defines a one-time reminder that remains pending until a user marks it done. The persisted state must therefore be named `IsDone` consistently in the domain model, EF configuration, migration, commands/queries, UI and tests. `Sent` is not the intended state because this phase does not send external notifications.

10. **Index and constraint precedent.** The inspected configuration folder contains no `HasFilter` index configuration and no string-key foreign-key precedent. Exactly one check constraint exists, on `ReportSettings`, configured with `b.ToTable(t => t.HasCheckConstraint(...))`; new filtered indexes or string foreign keys therefore require an explicit, separately reviewed design justification rather than being treated as established pattern.

11. **User-reference forms.** `AuditableEntity` audit columns and `InvoiceIssue.IssuedByUserId` are plain `int` values. By contrast, `AttendanceRecord` uses a `UserId`-typed property with value conversion and an explicit `HasOne<User>().WithMany().HasForeignKey(...)     .OnDelete(DeleteBehavior.Cascade)` relationship. Follow the relevant existing form instead of assuming all user references share one type.

These conventions are based on the source-inspection results supplied with the original audit, not on a new build or test run during this plan review. The prior build/test totals remain reported as inherited results; Sections 1 and 8 explicitly distinguish them from checks independently repeated in this revision.

------------------------------------------------------------------------

## SECTION 5 --- Migration Summary

Consolidated migration table. Totals recomputed from the phase-level migration decisions below; they are not copied from any earlier document.

#### Phase 1

- **Migration decision:** NEW MIGRATION
- **Reason:** `HeaderImagePath`, `FooterImagePath`, `NormalRangeMarker` columns do not exist on `ReportSettings`
- **Exact additive schema changes:** Add three nullable string columns to `ReportSettings`
- **Affected tables and columns:** `ReportSettings.HeaderImagePath`, `ReportSettings.FooterImagePath`, `ReportSettings.NormalRangeMarker`
- **Types, nullability, defaults, constraints:** string? nullable max 300 / 300 / 10; no key/index `NormalRangeMarker` max 10 follows `Antibiotic.Symbol` precedent
- **Back-fill / data-transition:** none (nullable)
- **Evidence data not already persisted / no schema change required:** Columns absent from all migrations and model snapshot

#### Phase 2

- **Migration decision:** NEW MIGRATION
- **Reason:** Seven receipt font/logo columns do not exist on `ReceiptSettings`
- **Exact additive schema changes:** Add seven nullable columns
- **Affected tables and columns:** `ReceiptSettings.HeaderFontFamily` string? max 100 (DESIGN), `HeaderFontSizePt` decimal(4,1) (DESIGN), `HeaderFontColor` string? max 9, `FooterFontFamily` string? max 100 (DESIGN), `FooterFontSizePt` decimal(4,1) (DESIGN), `FooterFontColor` string? max 9, `LogoImagePath` string? max 300; all nullable; decimal type/precision explicit
- **Types, nullability, defaults, constraints:** none
- **Back-fill / data-transition:** Columns absent from all migrations and model snapshot
- **Evidence data not already persisted / no schema change required:** —

#### Phase 3

- **Migration decision:** NO MIGRATION
- **Reason:** Reuses Phase 1 image columns; rendering-only over existing columns
- **Exact additive schema changes:** ---
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** `HeaderImagePath`/`FooterImagePath` added by Phase 1

#### Phase 4

- **Migration decision:** NO MIGRATION
- **Reason:** Reuses Phase 2 logo column; rendering-only ---
- **Exact additive schema changes:** —
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** `LogoImagePath` added by Phase 2

#### Phase 5

- **Migration decision:** NO MIGRATION
- **Reason:** Query predicate only; `PaymentOperation.OperationType` already distinguishes operation types
- **Exact additive schema changes:** ---
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** Data already persisted

#### Phase 6

- **Migration decision:** NO MIGRATION
- **Reason:** Reads `Patient`/`PatientTest`/`Test`; no new column
- **Exact additive schema changes:** ---
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** Data already persisted

#### Phase 7

- **Migration decision:** NEW
- **Reason:** Item-template table does not exist
- **Exact additive schema changes:** Add item-template table
- **Affected tables and columns:** `ReportItemTemplates`: `ItemKey` string PK required max 100; `Label` string required max 200; `ReferenceValueExpression` string? nullable max 500; `ShowHide` bool non-null default true; `ListMembership` string? nullable max 100; `FreeTextOverride` bool non-null default false
- **Types, nullability, defaults, constraints:** Seed one row per builder line kind; PROPOSED keys require agent confirmation
- **Back-fill / data-transition:** Table absent from all migrations and model snapshot
- **Evidence data not already persisted / no schema change required:** —

#### Phase 8

- **Migration decision:** NEW MIGRATION
- **Reason:** Per-item layout columns do not exist on item-template table
- **Exact additive schema changes:** Add line-spacing, left-offset, marker-symbol nullable columns
- **Affected tables and columns:** `ReportItemTemplates.LineSpacing` decimal(4,2) nullable (DESIGN), `LeftOffsetCm` decimal(5,2) nullable, `MarkerSymbol` string? nullable max 10; explicit decimal type/precision
- **Types, nullability, defaults, constraints:** none
- **Back-fill / data-transition:** Columns absent (table created by Phase 7)
- **Evidence data not already persisted / no schema change required:** —

#### Phase 9

- **Migration decision:** NEW MIGRATION
- **Reason:** Abnormal-result colour columns do not exist on `ReportSettings`
- **Exact additive schema changes:** Add enable flag and colour columns to `ReportSettings`
- **Affected tables and columns:** `ReportSettings` abnormal-result EnableAbnormalResultColor; font/background colours
- **Types, nullability, defaults, constraints:** boolean non-null false; colour strings nullable max 9
- **Back-fill / data-transition:** Columns absent from all migrations and
- **Evidence data not already persisted / no schema change required:** model snapshot

#### Phase 10

- **Migration decision:** NEW MIGRATION
- **Reason:** Per-profile styling columns do not exist on `Profiles`
- **Exact additive schema changes:** Add four nullable styling font/background columns
- **Affected tables and columns:** `Profiles.NormalFont`, `Profiles.NormalColor`, `Profiles.AbnormalFont`, `Profiles.AbnormalBackgroundColor` nullable strings; max lengths 50 / 9 / 50 / 9
- **Types, nullability, defaults, constraints:** Null default (global colours apply)
- **Back-fill / data-transition:** —
- **Evidence data not already persisted / no schema change required:** Columns absent from all migrations and model snapshot

#### Phase 11

- **Migration decision:** NO MIGRATION
- **Reason:** Commands/handlers exist; UI wiring plus guard predicates in three existing handlers
- **Exact additive schema changes:** ---
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** No schema change

#### Phase 12

- **Migration decision:** NEW MIGRATION
- **Reason:** Sensitivity visibility flags and antibiotic-of-choice field do not exist
- **Exact additive schema changes:** Add three visibility flags and choice flag
- **Affected tables and columns:** `CultureAntibioticResults.IsAntibioticOfChoice`; `ReportSettings` three visibility flags
- **Types, nullability, defaults, constraints:** booleans non-null; choice false; flags true; no filtered index
- **Back-fill / data-transition:** Show all columns; no choice
- **Evidence data not already persisted / no schema change required:** Columns absent from migrations and model snapshot

#### Phase 13

- **Migration decision:** NO MIGRATION
- **Reason:** Query predicate only over `PatientTest.EnteredAtUtc`/`ResultValue`
- **Exact additive schema changes:** ---
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** Data already persisted

#### Phase 14

- **Migration decision:** NO MIGRATION
- **Reason:** Snapshot model and command exist; UI wiring only
- **Exact additive schema changes:** ---
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** No schema change

#### Phase 15

- **Migration decision:** NEW
- **Reason:** Label size and vertical-coding columns do
- **Exact additive schema changes:** Add label size and
- **Affected tables and columns:** `SystemSettings.LabelWidthMm`, `LabelHeightMm`, `EnableVerticalCoding`; `decimal(4,1)` / `decimal(4,1)` / bool, non-null; defaults 38.0 / 25.0 / false (DESIGN for decimal(4,1))
- **Types, nullability, defaults, constraints:** Default 38 x 25 mm; vertical coding off; PDF page geometry follows mm
- **Back-fill / data-transition:** Columns absent from all migrations and model snapshot
- **Evidence data not already persisted / no schema change required:** —

#### Phase 16

- **Migration decision:** NEW MIGRATION
- **Reason:** `SampleMaterial` and assignment tables do not exist
- **Exact additive schema changes:** Add `SampleMaterials` and `PatientTestSampleMaterials`
- **Affected tables and columns:** `SampleMaterials.Name` required string max 100; `Description` string? nullable max 500; `PatientTestSampleMaterials.AssignedAtUtc` required datetime2; PatientTestId FK Cascade; SampleMaterialId FK Restrict; unique index on PatientTestId; no duplicate FK on PatientTests
- **Types, nullability, defaults, constraints:** none
- **Back-fill / data-transition:** Tables absent from all migrations and model snapshot
- **Evidence data not already persisted / no schema change required:** —

#### Phase 17

- **Migration decision:** NO MIGRATION
- **Reason:** Reuses `SystemSettings.EnableVerticalCoding` introduced by Phase 15; rendering-only change over existing setting and identity-mark pipeline
- **Exact additive schema changes:** ---
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** No new schema; OD-03 is RESOLVED in Section 6 and selects one global toggle for report, receipt, and envelope

#### Phase 18

- **Migration decision:** NEW
- **Reason:** Sample-nature interpretation table does
- **Exact additive schema changes:** Add interpretation table
- **Affected tables and columns:** `SampleNatureInterpretations`: `Nature` required string max 100; `NormalRanges` required string max 500; `InterpretationText` required string max 1000; `TestId` FK Restrict
- **Types, nullability, defaults, constraints:** none (editorial content separate)
- **Back-fill / data-transition:** Table absent from all migrations and model snapshot
- **Evidence data not already persisted / no schema change required:** —

#### Phase 19

- **Migration decision:** NEW MIGRATION
- **Reason:** Glossary table does not exist
- **Exact additive schema changes:** Add glossary table
- **Affected tables and columns:** New table: abbreviation, full term, notes
- **Types, nullability, defaults, constraints:** string, nullable per design, PK
- **Back-fill / data-transition:** none
- **Evidence data not already persisted / no schema change required:** Table absent from all migrations and model snapshot

#### Phase 20

- **Migration decision:** NEW
- **Reason:** Reminder table does not exist
- **Exact additive schema changes:** Add `Reminders` table
- **Affected tables and columns:** OwnerUserId UserId-typed converted FK with explicit User FK Cascade; TargetType required string max 20; TargetContactName string? max 200; TargetContactPhone string? max 30; Message required string max 500; DueDateUtc required datetime2; IsDone bool default false; check constraint per Phase 20.F
- **Types, nullability, defaults, constraints:** none; one-time reminder remains pending until done
- **Back-fill / data-transition:** Table absent from all migrations and model snapshot
- **Evidence data not already persisted / no schema change required:** —

#### Phase 21

- **Migration decision:** NO MIGRATION
- **Reason:** Reads `ProfileResultItem` (already exists) ---
- **Exact additive schema changes:** —
- **Affected tables and columns:** ---
- **Types, nullability, defaults, constraints:** ---
- **Back-fill / data-transition:** ---
- **Evidence data not already persisted / no schema change required:** `ProfileResultItem` persisted by `20260909033414_AddAnalyteProfileDomain`

#### Phase 22

- **Migration decision:** NEW
- **Reason:** Per-user layout preference table does not
- **Exact additive schema changes:** Add `UserReportLayoutPreferences`
- **Affected tables and columns:** `UserId`-typed property converted to int; explicit User FK Cascade; `ReportItemKey` string required max 100 with no database FK (DESIGN); `IsVisible` bool non-null default true; composite PK (`UserId`, `ReportItemKey`)
- **Types, nullability, defaults, constraints:** none
- **Back-fill / data-transition:** Table absent from all migrations and model snapshot
- **Evidence data not already persisted / no schema change required:** —

Reconciled migration total:

- NEW MIGRATION phases: 1, 2, 7, 8, 9, 10, 12, 15, 16, 18, 19, 20, 22 = 13 migrations.
- NO MIGRATION phases: 3, 4, 5, 6, 11, 13, 14, 17, 21 = 9 phases.
- Total phases: 22. Total new migrations: 13. The number of migrations (13) agrees with the phase-level migration decisions. Every new migration is additive and appended after the latest existing migration; no existing migration is edited. All owner decisions are RESOLVED (Section 6); none are counted as confirmed migrations.

### 5.1 --- Migration handoff protocol

The following protocol applies to every phase whose migration decision is NEW MIGRATION (currently Phases 1, 2, 7, 8, 9, 10, 12, 15, 16, 18, 19, 20, 22). Phases with NO MIGRATION do not create any migration artefact.

a\) The implementing agent CREATES the complete migration artefact set for the phase, using the project's standard EF Core tooling generated from the finished model (never hand-written): (1) the migration file, (2) its Designer file, (3) the updated model snapshot file. All three are mandatory. A phase with a new migration is not complete without all three.

b\) The migration must be created in the same project and folder as the existing migrations, with a timestamped name later than the latest existing migration, named meaningfully and consistently with existing naming (the existing latest is `20261008135847_AddAntibioticCommercialName`; confirm against the repository when executing).

c\) The implementing agent must NEVER apply the migration to any database: no `dotnet ef database update`, no application launch that auto-migrates, no execution of the migration SQL against any database. The owner alone applies it to the real database.

d\) The agent must verify the artefacts WITHOUT touching a database: the solution builds with zero warnings and zero errors; the Designer file and snapshot were produced by the tooling; the snapshot diff contains only the changes intended by that phase (no unrelated drift); the migration contains an Up and a correct Down that reverse exactly what Up does; no existing migration file was edited; generating the migration SQL script offline (read-only, output to a location outside the repository tree or to the console only) may be used as a review aid where the tooling supports it without a database connection.

e\) After creating and verifying the artefacts, the agent must STOP and hand off to the owner with a report containing: the three file paths, the migration name, a summary of the schema changes, the Up/Down review result, the snapshot-diff review result, and the exact command the owner will run (the Package Manager Console command and the CLI equivalent). The agent must not begin the next phase.

f\) The owner then applies the migration to the real database and confirms with `Get-Migration` that the new migration shows `applied = True`. Only after this confirmation is the phase considered accepted, and only then may the next phase start. Write this as an explicit gate in item K of every phase with a migration.

g\) Honest limitation: the automated test `RelationalIntegrationTests.MigrationChain_AppliesToEmptyDatabase` cannot run without a container runtime, so the full migration chain on an empty database is not proven by the automated suite. The owner's application of each migration to the real database, confirmed with `Get-Migration`, is the acceptance evidence for that phase. Running that test, and `ConcurrentSettleTests.Settle_ConcurrentCalls_InsertOnePayment`, on a host with a container runtime remains a recommended extra check.

## SECTION 6 --- Owner Decisions (all resolved)

All six owner decisions were finalized by the owner choosing Option A for each. Each is marked RESOLVED below with the chosen option and its effect on the plan.

### OD-01 --- Single-result print recording rule (print count) --- RESOLVED: Option A

- Decision: Record the print on success in the single-result path, making all print routes consistent and enabling delivery after a single print.
- Effect on plan: Phase 11 and Phase 14 include the print-count-recording rule. No new migration is required. No phase was blocked; both phases already had resolved owner decisions.
- Status: RESOLVED --- Option A chosen.

### OD-02 --- Test removal/clear guard after results exist or after delivery --- RESOLVED: Option A

- Decision: Add an explicit IsReviewed \|\| IsPrinted \|\| PrintCount \> 0 \|\| IsDelivered guard to all three removal paths (single-remove via deltas, clear-all, apply-visit-deltas).
- Effect on plan: implemented as a hardening task in Phase 11 (verify/unverify simple results), adding the guard to all three removal paths. No phase blocked. No migration required.
- Status: RESOLVED --- Option A chosen.

### OD-03 --- Vertical identity coding toggle scope --- RESOLVED: Option A

- Decision: Single global boolean toggle governing all three documents (result report, receipt, envelope).
- Effect on plan: Phase 17 is ready for implementation now that the decision is resolved. Its migration decision remains NO MIGRATION (the toggle reuses the setting added by Phase 15; no per-document columns). This planning status does not mean the phase is implemented.
- Status: RESOLVED --- Option A chosen.

### OD-04 --- Appointment-note reminder scheduling specifics --- RESOLVED: Option A

- Decision: One-time reminder with a due date, targeted at a user or phone-book contact, shown in a pending-reminder list until marked done.
- Effect on plan: Phase 20 is ready for implementation now that the one-time due-date behaviour is resolved. This planning status does not mean the phase is implemented.
- Status: RESOLVED --- Option A chosen.

### OD-05 --- Permission granularity --- RESOLVED: Option A

- Decision: Keep the 13 coarse seeded permissions (current behaviour).
- Effect on plan: no change to any phase or migration. The coarser model is classified as a coarser model, not a missing capability.
- Status: RESOLVED --- Option A chosen.

### OD-06 --- Banded monitor hosting for blood-picture element values --- RESOLVED: Option A

- Decision: Extend the existing banded-monitor screen to support element/analyte input (reusing HistoryMatrixBuilder.Pivot).
- Effect on plan: Phase 21 is ready for implementation now that OD-06 is resolved. The element-value band search extends the existing scalar monitor screen; planning readiness does not mean it is already implemented.
- Status: RESOLVED --- Option A chosen.

------------------------------------------------------------------------

All six owner decisions are RESOLVED. All 22 phases are ready for implementation; none is implemented merely by appearing in this plan.

## SECTION 7 --- Risk Register and Cross-Phase Dependencies

| Risk | Likelihood | Impact | Mitigation | Phase or decision affected |
| --- | --- | --- | --- | --- |
| Incorrect classification: a function labelled IMPLEMENTED is actually not fully implemented at runtime | Medium | Medium | Tier 1 reachability was performed for all IMPLEMENTED functions; Tier 2 deep traces covered all flagged items; classifications rest on static traces plus passing tests, not runtime observation | All IMPLEMENTED functions; reported honestly as not runtime-verified |
| Unverified runtime behaviour: WPF layer never launched; no database observed | Medium | High | No phase claims runtime verification; every phase-end gate requires a build and test run on the implementation host; runtime confirmation left to the implementing agent on Windows | All phases; noted as a standing limitation |
| Database/migration uncertainty: applied status of the 15 existing migrations is owner-reported evidence, not independently re-run | Medium | High | The owner reports that, after reopening the IDE, the command `Get-Migration -Project TopLab.Infrastructure -StartupProject TopLab.Presentation` showed all 15 migrations from `BaselineDataModel` through `20261008135847_AddAntibioticCommercialName` with `applied = True`. This is owner-reported evidence and was not independently re-run during this document review. The automated migration-chain test cannot run without a container runtime, so the full chain on an empty database remains unproven. New migrations follow Section 5.1: the agent creates artefacts only; the owner applies and confirms them. | All phases; owner-reported status recorded, not independently re-run |
| Two automated tests cannot run on the verification host: `RelationalIntegrationTests.MigrationChain_AppliesToEmptyDatabase` and `ConcurrentSettleTests.Settle_ConcurrentCalls_InsertOnePayment` skip because no container runtime is installed | Medium | High | The automated suite cannot currently prove that the full migration chain (existing plus new) applies to an empty database, nor the concurrent-settlement behaviour; the owner applies each new migration to the real database and confirms it with `Get-Migration` (Section 5.1); both tests should be run on a host with a container runtime as a recommended extra check | Phases with NEW MIGRATION; settlement-related phases |
| WPF and printing differences: fonts, Arabic shaping, RTL layout not visually verified on paper | Medium | Medium | All printing tests pass on Windows; font-dependent failures seen elsewhere do not occur here; no physical printer or Arabic shaping test executed | Phases touching report/receipt/envelope writers (1, 2, 3, 4, 7, 9, 10, 16, 17) |
| Font and rendering dependencies: `ArabicFontResolver` is Windows-only and depends on installed fonts | Medium | Medium | Fonts resolved at render time; document preference for widely available fonts; rendering tests use bundled images/fonts | Phases 1, 2, 7, 9, 10, 16, 17 |
| Reference ambiguity: some reference pages read through text extraction only; visual degradation on some pages | Medium | Medium | 55 pages visually inspected at 150 DPI; ambiguous pages cross-checked against render; ambiguous details recorded as owner decisions rather than invented | All phases citing ambiguous pages; OD-03, OD-04, OD-06 |
| Data migration/back-fill risk: new settings columns and tables need sensible defaults | Low | Medium | Each phase defines required/nullability/default rules and any seed/back-fill explicitly; Phase 15 applies 38 x 25 mm defaults to the existing singleton settings row | Phases 1, 2, 7, 8, 9, 10, 12, 15, 18, 19, 20 |
| Dependencies between phases: report builder/settings are distinct code paths; receipt and simple-result review are separate | Medium | Medium | Phase 1 precedes report-settings-dependent work; Phase 7 introduces the item template; Phase 8 and 22 extend it; Phases 9/10 colour template-driven result lines. Phase 2 uses ReceiptPdfWriter; Phase 11 does not modify ReportContentBuilder | Phases 1-10, 22 |
| Unresolved inventory completeness: decomposition is a reconstruction; a different auditor could decompose composite screens differently | Low | Medium | Inventory derived independently from both PDFs; duplicates merged; excluded functions separated; totals reconciled from rows | Gap register; Section 8 item 1 |

Confirmed facts are distinguished from possible risks: risks above are possible, not confirmed. Build/test results are reported by the original audit as confirmed, but were not rerun during this document review.

Cross-phase dependency summary: Phase 1 (report-settings foundation) precedes Phase 2's separate receipt image-asset pattern and report- settings-dependent work in Phases 3, 7, 9, and 10. Phase 7 introduces the item template; Phase 8 extends its layout fields, Phases 9 and 10 colour template-driven result lines, and Phase 22 uses its keys for per-user visibility. Phase 2 uses `ReceiptPdfWriter`; Phase 11 does not modify `ReportContentBuilder`. Phase 15 (label size and vertical-coding setting) precedes Phase 16 (sample materials) and Phase 17 (vertical coding). Phase 16 precedes Phase 17. No circular dependencies exist.

------------------------------------------------------------------------

## SECTION 8 --- Self-Audit Checklist

Each item carries an explicit PASS / FAIL / PARTIALLY VERIFIED result supported by evidence.

| \# | Item | Result | Evidence |
| --- | --- | --- | --- |
| 1 | Completeness of reference inventory | PASS | 152 distinct in-scope functions reconstructed from both PDFs; 21 excluded functions separated; totals recomputed from rows (152 in-scope = 127 IMPLEMENTED + 11 DIFFERENT + 14 MISSING); duplicates merged; sub-steps not double-counted |
| 2 | Verification of all identified MISSING functions | PASS | All 14 MISSING functions have a gap-register entry (GAP-012, GAP-013, GAP-014, GAP-015, GAP-016, GAP-017, GAP-018, GAP-019, GAP-020, GAP-021, GAP-022, GAP-023, GAP-024, GAP-025) and at least one implementation phase; each supported by systematic negative search and page evidence |
| 3 | Verification of all identified DIFFERENT functions | PASS | All 11 DIFFERENT functions have a gap-register entry (GAP-001...GAP-011) and at least one implementation phase; each with reference behaviour, current behaviour, delta, and file/line evidence |
| 4 | Excluded-feature isolation | PASS | 21 excluded functions listed separately; zero excluded functions in the gap register, phases, migration summary, or recommendations; the five exclusions applied consistently |
| 5 | Source restrictions | PASS | Only the two authorized PDFs used for reference requirements; `Docs/Reference system files/` never opened; no external websites or domain knowledge used for requirements |
| 6 | Source-path and line-number accuracy | PARTIALLY VERIFIED | File paths and line ranges cited from files actually read; line numbers re-checked after inspection; a small number of Tier 1 rows relied on grep results rather than full file reads |
| 7 | PDF visual inspection coverage | PARTIALLY VERIFIED | 55 pages visually inspected at 150 DPI (guide pages 3-5, 151-153, 155, 193-212; showcase pages 3, 15, 51, 56-66); remaining pages covered through text extraction; no visual inspection claimed for text-only pages |
| 8 | Build coverage | PASS | All 9 projects built individually with zero warnings and zero errors; exact commands and exit codes recorded |
| 9 | Test coverage and count reconciliation | PASS | 2788 passed, 0 failed, 2 skipped (2790 total) across all 5 test projects; per-project counts recorded; both skips occur because a container runtime is unavailable on the verification host; discrepancy with earlier totals explained |
| 10 | Migration inspection | PARTIALLY VERIFIED | The owner reports that, after reopening the IDE, `Get-Migration -Project TopLab.Infrastructure -StartupProject TopLab.Presentation` showed all 15 migrations from `BaselineDataModel` through `20261008135847_AddAntibioticCommercialName` with `applied = True`. This is owner-reported evidence and was not independently re-run during this document review. The automated migration-chain test cannot run without a container runtime, so the full chain on an empty database is not proven by that test. New migrations follow Section 5.1; the owner applies them and confirms status with `Get-Migration`. |
| 11 | Phase dependency ordering | PASS | Phases ordered by real code dependency; Phase 1 first; one migration per phase; no circular dependencies; prerequisite relationships documented per phase |
| 12 | One-migration-per-phase compliance | PASS | Each phase states exactly one migration decision; 13 phases NEW MIGRATION, 9 phases NO MIGRATION; total 13 reconciles with phase decisions |
| 13 | Acceptance criteria | PASS | Every phase defines Given/When/Then acceptance criteria traceable to the reference requirement |
| 14 | Open owner decisions | PASS | Six owner decisions documented (OD-01...OD-06), all RESOLVED --- see Section 6 for chosen options and their effect on the plan. |
| 15 | IMPLEMENTED-function coverage (Tier 1 and Tier VERIFIED 2) | PARTIALLY | Tier 1 reachability performed for all IMPLEMENTED functions (141 points); Tier 2 deep traces covered all 9 flagged items plus 9 cross-cutting concerns; coverage rests on static traces plus passing tests, not runtime observation |
| 16 | Cleanliness of the plan | PASS | No commit hash or version-control instructions were introduced; no obsolete REF-xxx/D-xx/K-xx/F-xx/L-xx/U-xx identifiers or PDF filenames are used as implementation dependencies. A brief prior-audit provenance note is intentionally included to distinguish inherited build/test claims from checks rerun during this document review. |
| 17 | Honesty about unverified claims | PASS | Unverified items (runtime behaviour, live migration application, printed-output visual correctness, Arabic shaping) are stated as limitations, not claimed as verified |

A PASS is supported by evidence. FAIL is used where a requirement is not satisfied. PARTIALLY VERIFIED is used where meaningful evidence exists but a material limitation remains. PASS is not used as a synonym for "attempted."
