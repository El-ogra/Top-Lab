# Plan for completing the implementation of the project

## Document header

- Date of preparation: 17 October 2026.
- Environment used: Windows 11, .NET SDK 8.0.425 with .NET SDK 9.0.318 also installed, Windows Desktop runtime 8.0.31, Python 3.13 with pypdf and PyMuPDF 1.28.2 for reference-page handling, `dotnet-ef` 8.0.30, LocalDB instance control (`sqllocaldb`) for availability probing only.
- What was verified: the full solution builds project-by-project with zero warnings and zero errors (all four source projects and all five test projects); the full test suite was executed on the .NET 8 runtime (2787 passed, 0 failed, 2 skipped across 2789 total); the ordered migration set (15 migrations, latest `20261008135847_AddAntibioticCommercialName`) was inspected as source together with entity definitions, EF configurations and the model snapshot file; every in-scope reference function was traced to its execution path (entry point, command or query, handler, domain rule, persistence, presentation) or established as absent by systematic negative search; the two authoritative reference PDFs were confirmed at 212 and 77 pages and 55 pages were inspected visually at 150 DPI with the remainder covered through text extraction.
- What was not verified: runtime behaviour of the graphical interface (the application interface was never launched, which is also a standing safety rule for this work); application of migrations to a live database (no database connection could be opened from the available tooling, so migration conclusions rest on source inspection and model metadata, not on an applied schema); visual correctness of printed output on paper or on a physical printer; Arabic glyph shaping in generated files beyond what the passing printing tests assert.
- Build limitations: a single parallel full-solution build was attempted and did not complete (a build-node failure under parallel load); every project was then built individually in dependency order, and each build succeeded with zero warnings and zero errors.
- Test limitations: two tests in `TopLab.Persistence.Tests` skip cleanly (one requires a container runtime that is not installed, one is a concurrency test marked to skip); no test project was excluded for safety reasons because every database-touching test uses an in-memory provider with a generated database name, model-only metadata reads that open no connection, or an ephemeral container that skips when its runtime is absent.
- Overall confidence limitations: classifications of IMPLEMENTED rest on static traces plus passing tests, not on observed runtime behaviour; MISSING classifications rest on systematic negative searches, which cannot prove the absence of a capability that lives outside the repository; several reference pages were read through text extraction only; a small number of reference details are genuinely ambiguous and are recorded as open owner decisions rather than invented requirements.

---

## SECTION 1 — Verification Context

### 1.1 Date, environment and tools

Prepared 17 October 2026 on Windows 11. Toolchain: .NET SDK 8.0.425 (with 9.0.318 present), Windows Desktop runtime 8.0.31, `dotnet-ef` 8.0.30, Python 3.13 with pypdf and PyMuPDF for reference-page rendering at 150 DPI, `sqllocaldb` for availability probing, standard source-search tooling. No external websites, search engines or general domain knowledge were used to establish reference requirements.

### 1.2 Scope of verification

The verification covered: solution structure (4 source projects, 5 test projects, 15 ordered migrations, model snapshot file, WPF views and view models, printing pipeline, authorization behaviour); per-project builds; the complete test suite on the .NET 8 runtime; migration and persistence source inspection; a 141-point Tier 1 reachability sweep over the implemented behaviour plus targeted Tier 2 deep traces for every flagged item and every report-layout, barcode, permission, inventory-naming and monitor concern; visual inspection of 55 reference pages at 150 DPI; and text-layer inspection of the remaining pages.

### 1.3 Verification status summary (honest)

- Build: PASS for all 9 projects individually (0 warnings, 0 errors each). A single parallel full-solution build attempt failed on build-node resources and is recorded as a limitation, not as a project failure.
- Tests: 2787 passed, 0 failed, 2 skipped (2789 total). The 2 skips are by design (container runtime absent; one concurrency test marked to skip). Per-project: Domain 513/0/0, Application 1733/0/0, Infrastructure 353/0/0, Persistence 13 passed with 2 skipped, Presentation 175/0/0. All 28 printing tests pass on Windows; the font-dependent failures seen on other operating systems do not occur here.
- Reference inventory: 152 distinct in-scope functions inventoried from the two authoritative PDFs; 21 excluded functions inventoried separately. Two reference rows required special handling: a showcase-side restatement of the group-patient slip is merged into a single function, and a supposed year-end settlement entry has no reference evidence (the cited chapter covers yearly drawer accounting and sent-out settlement only) and is therefore not carried as a function.
- Classifications: IMPLEMENTED 127, DIFFERENT 11, MISSING 14. Every MISSING and DIFFERENT function has a gap-register entry and at least one implementation phase. BLOCKED / UNRESOLVED: 0. Open owner decisions: 6, none of which blocks the first phase.
- Migration verification: source-level only (PASS with stated limits); no live-database application was possible, so migration conclusions are marked accordingly and every new migration must be proven against a disposable database by the implementing agent.
- IMPLEMENTED-function coverage: Tier 1 reachability checks covered every IMPLEMENTED function (141 verification points, 132 passing, 9 flagged); Tier 2 deep traces covered all 9 flagged points plus the permission, naming, monitor, deviation, settlement, appointment-note and vertical-coding concerns (18 deep investigations); functions covered by neither tier: 0.

---

## SECTION 2 — Scope and Exclusions

Five areas are permanently excluded from the required scope. They were applied as a hard boundary during inventory, classification, dependency analysis, phase design and migration decisions: no excluded function appears in the gap register, in any phase, in the migration summary, or as a recommendation.

1. Online results portal and associated ecosystem. The online result website, portal receipt printing, doctor and laboratory portal accounts, Android applications, uploader utilities and portal administration are excluded. Where a reference window combines portal accounts with in-scope master data (treating doctors, sent-out laboratories, referring entities), only the master-data portion is carried; the portal-account portion is not planned.
2. Equipment management. Equipment registration, maintenance records and calibration records are excluded, including the single system-window capability line that names all three together.
3. Analyser integration. Blood/CBC analyser connections and result imports are excluded, including correction factors, hardware lists and interface settings. Manual entry of CBC results, CBC history matrices and statistics over results already stored in the laboratory database remain in scope.
4. Multi-branch support. The application is single-branch only. Branch-number columns that exist in storage (`Users.BranchNumber`, `SystemSettings.BranchNumber` with a single-branch default) are dormant data with no branch-scoped behaviour and are left untouched; no phase introduces branch dimensions.
5. External result delivery. Sending results by SMS, e-mail or fax is excluded, including laboratory-branded SMS notices. Contact-data columns (such as an e-mail address field on external entities) are dormant data, not delivery behaviour. Every reminder or notification in this document stays inside the application; no phase introduces an external delivery channel.

Enforcement evidence: the inventory keeps 21 excluded functions in a separate excluded population; the gap register contains zero excluded functions; every phase-end gate requires the implementing agent to confirm that no excluded functionality or external delivery channel was introduced; the migration summary contains no table or column serving an excluded purpose.

---

## SECTION 3 — Functional Gap Register

One entry per distinct in-scope MISSING or DIFFERENT function. IMPLEMENTED and OWNER-EXCLUDED functions are not listed here. Identifiers GAP-001…GAP-025 are stable within this file.

| Gap ID | Function | Classification | Reference evidence | Current implementation evidence | Verification status | Required outcome | Dependencies | Open decision, if any |
|---|---|---|---|---|---|---|---|---|
| GAP-001 | Verify / unverify simple single-value results | DIFFERENT | reference guide, pages 28, 54 (review and reverse-review of entered results) | `ReviewResultCommandHandler.cs`, `UnreviewResultCommandHandler.cs` and `MarkAllPatientResultsReviewedCommandHandler.cs` exist under `src/TopLab.Application/Features/ResultsEntry/Commands/`; no sender in `src/TopLab.Presentation` (verified by exhaustive search; `SimpleResultEntryViewModel.cs` exposes only save, clear and comment-picker). Profile and culture verify paths are wired (`ProfileEntryViewModel.cs:303,340`, `CultureEntryViewModel.cs:404`). | STATICALLY VERIFIED | Wire the existing simple-result review commands to the result-entry surface with permission checks, or implement the surface action through them, so entered simple results can be verified and unverified exactly like profile and culture results. | None (uses existing commands, entities and permissions). | OD-01 (single-print recording rule interacts with the review-before-print chain). |
| GAP-002 | Control display of Sensitivity / Reference / Commercial Name columns in the culture report | DIFFERENT | reference guide, page 39 (toggle which of the three columns appears) | Master data present (`Antibiotic.Symbol/ScientificName/CommercialName`, `AntibioticConfiguration.cs:19`, latest migration adds `CommercialName`); report grid fixed at four columns with no commercial-name column and no toggle (`ReportCultureSection.cs:113-124`, comment records no commercial-name column). | STATICALLY VERIFIED | Persist three column-visibility flags and render the sensitivity grid with exactly the selected columns, including the commercial name when enabled. | Culture report query and writer (shared with GAP-012). | None. |
| GAP-003 | Temporarily override a reference value at print time for one patient | DIFFERENT | reference guide, pages 101-102 (open the reference cell from the report, edit, refresh, accept for that patient) | Per-patient frozen-range snapshot infrastructure exists (`ResultsEntryDtos.cs:24-52`, `ResultReferenceRangeSource.cs`); `RefreshResultReferenceRangeCommand` handler exists but has no sender in `src/TopLab.Presentation`. | STATICALLY VERIFIED | Make the print-time refresh reachable from the report surface for a single patient test, re-resolving and re-freezing only that patient's snapshot. | Result-entry snapshot model (exists). | None. |
| GAP-004 | Print the worksheet restricted to unfinished work, or complete | DIFFERENT | reference guide, pages 86, 89 (unfinished-only switch on the worksheet) | Worksheet queries exist (`GetVisitWorkSheetQuery`, `GetWorkSheetByTestGroupQuery`, `GetWorkSheetByWorkGroupLogQuery`) with no unfinished-only parameter; print command is per-patient (`PrintWorkSheetCommand(PatientId)`); only the outside-laboratory exclusion exists, which is a different filter. | STATICALLY VERIFIED | Add an unfinished-only option to the worksheet queries and surface it on the worksheet view as a switch between unfinished-only and complete output. | Worksheet queries and writer (exist). | None. |
| GAP-005 | Report header/footer from attached image files | DIFFERENT | reference guide, page 194 (header/footer modes: none, words, image files) | `HeaderFooterMode.Images` is offered (`HeaderFooterMode.cs`, settings view models) but `ReportContentBuilder.cs:32,68,138` branches only on `!= None`; receipt and envelope writers fall back to the words header with an explicit code comment; no image column, picker or upload path exists. | STATICALLY VERIFIED | Persist header/footer image references, provide a file picker with validation, render the images in the report header/footer, and define the fallback when a file is missing. | Shared with GAP-006, GAP-011, GAP-018 (same aggregate and writer region). | None. |
| GAP-006 | Report header/footer colours | DIFFERENT | reference guide, pages 195-198 (colour of individual header/footer elements) | `ReportSettings.HeaderColor/FooterColor` declared and mapped (`ReportSettings.cs:21,23`, `ReportSettingsConfiguration.cs:20-21`, seeded null) but with no setter, no command field, no view-model property and no writer read (7 code hits total, one test asserting null). | STATICALLY VERIFIED | Add mutators, command fields, settings-screen controls and writer application for both colours. | Same aggregate and writer region as GAP-005 (implement together). | None. |
| GAP-007 | Receipt font type, size and colour plus laboratory logo | DIFFERENT | reference guide, pages 202-204 (receipt header/footer font controls and logo) | `ReceiptSettings.cs` models margins, currency, pickup time, print-once, detail mode, cashier-printer flag and header/footer mode; no font family/size/colour and no logo reference; writer falls back to words for the Images mode (`ReceiptPdfWriter.cs:200-202`). | STATICALLY VERIFIED | Persist receipt header/footer font family, size and colour plus a logo image reference; expose them on the receipt-settings screen; render them in the receipt writer. | Image-asset pattern shared with GAP-005. | None. |
| GAP-008 | Patient receivable account summary with balance filter | DIFFERENT | reference showcase, page 26 (period receivable summary filtered to unpaid, discounted or paid-only) | `GetPatientSamplesDetailQuery(From, To)` returns per-patient charged/paid/balance with no filter parameter; `PaymentOperation.OperationType` distinguishes payment, correction, extra charge and void, so the data needed for the filter already exists. | STATICALLY VERIFIED | Add the three-way balance filter (unpaid-only, discounted-only, paid-only) to the patient-level account query and surface and its writer. | Payment-operation model (exists). | None. |
| GAP-009 | Barcode labels per sample material with doctor-editable test distribution | DIFFERENT | reference showcase, page 3 (tube/container/swab labels; program distributes tests across drawn materials; doctor may change the distribution; 38 mm x 25 mm stock; reprintable) | `IBarcodeService.PrintBarcodeAsync(string value)` renders one fixed-size Code-128 label for the patient or Lab Id (`BarcodeService.cs:44`, `BarcodeLabelRenderer.Render(content, width = 300, height = 80)`); no sample-material entity, no per-material assignment, no distribution UI. Identity-barcode printing itself works. | STATICALLY VERIFIED | Introduce sample materials and per-test material assignment, a doctor-editable distribution surface, and multi-label printing; keep the existing single-identifier path working. | GAP-021 (label size parameters should be configurable first). | None. |
| GAP-010 | Receipt header/footer logo on pre-printed or plain receipt paper | DIFFERENT | reference showcase, page 4 (receipt paper choice with logo branding) | Words half implemented (`ReceiptPdfWriter.cs:200-205`); logo half inert (Images mode falls back to words; no logo asset). | STATICALLY VERIFIED | Render the persisted logo on the receipt for both paper choices. | GAP-007 (reuses its logo column; no new schema). | None. |
| GAP-011 | Report on plain or pre-printed paper with a logo | DIFFERENT | reference showcase, pages 5, 16 (paper choice and logo branding for reports) | Plain-paper path is the default and works; pre-printed choice approximated by words mode; logo half inert (`ReportContentBuilder.cs:32,68,138` never loads an image). | STATICALLY VERIFIED | Render the persisted header/footer images on the report for both paper choices. | GAP-005 (reuses its image columns; no new schema). | None. |
| GAP-012 | Select the antibiotic of choice for a culture | MISSING | reference guide, page 39 (`Antibiotic of choice` on the culture report) | No choice field, command, query or view-model property exists (exhaustive search for choice markers returns only unrelated gender/age/font usages; `CultureAntibioticDisplay.cs` holds only the pregnancy/children filter). | STATICALLY VERIFIED | Persist one antibiotic-of-choice selection per culture result, expose selection in culture entry, and mark it on the report. | Culture entry grid and report section (shared with GAP-002). | None. |
| GAP-013 | Group-patient report slip (one slip per patient for a whole group) | MISSING | reference guide, pages 52-57; reference showcase, page 6 (per-patient slips for a group of patients, archive-safe; envelope/position control on the same page) | No command, query, DTO, writer, view or view model produces a one-line-per-patient slip (systematic search returned no implementation; nearest artefacts are the bench worksheet and the collection sheet, which are different documents). | STATICALLY VERIFIED | Add a group-slip query and writer producing one slip per patient with the reference layout, reachable from the follow-up/delivery area. | Patient, visit-test and test data (exist; no schema change). | None. |
| GAP-014 | Editable report item template (letters, reference values, show/hide, extensible lists, free text) | MISSING | reference showcase, page 12 (report item layout configuration) | `ReportContentBuilder.cs` emits a fixed section sequence for every report; no item-template entity, command, DTO or show/hide/free-text construct exists. | STATICALLY VERIFIED | Introduce an item-template aggregate with management surface and drive the builder from it. | Report builder shared with GAP-005…GAP-011, GAP-015, GAP-016 (implement last among report-visual items). | None. |
| GAP-015 | Line spacing, item distance from the margin, and symbol before each item | MISSING | reference showcase, page 13 (layout and marker control per report item) | Only page margins and top space are persisted (`PageMarginLeftCm`, `PageMarginBottomCm`, `ReportTopSpaceCm`); no line-spacing field, no per-item report offset (per-item offsets exist only for the envelope, a different surface), no marker configuration. | STATICALLY VERIFIED | Persist per-item layout (line spacing, left offset, marker symbol) and apply it in the report composer. | Report composer region shared with GAP-005 (implement after the branding foundation). | None. |
| GAP-016 | Shade or colour abnormal results, or disable the shading | MISSING | reference showcase, page 13 (highlight out-of-range results with an off switch) | `ResultFlagComputer` computes Low/High and the builder prints low/high comment text; no colour or background is ever applied to a result cell (writer search returns only page-background and footer greys). | STATICALLY VERIFIED | Add a global enable flag plus abnormal-result colours and apply them to flagged result cells. | Result-cell rendering shared with GAP-017 (implement before the per-profile override). | None. |
| GAP-017 | Per-profile control of normal/abnormal result font and background colour | MISSING | reference showcase, page 16 (per-profile result styling) | No per-profile styling attribute, command, DTO or writer branch exists on `Profile`, `ProfileAnalyte` or `ProfileResultItem`. | STATICALLY VERIFIED | Add per-profile styling attributes with a documented precedence rule against the global colours, and apply them in the writers. | GAP-016 (implement after the base colouring). | None. |
| GAP-018 | Edit the character used to mark the normal range | MISSING | reference showcase, page 17 (normal-range marker control) | The marker text is a fixed string in the report content (`ReportDocumentContent.cs:129` area); no setting, command or view-model property exposes it. | STATICALLY VERIFIED | Persist one nullable marker string and substitute it in the writer. | Same aggregate and writer file as GAP-005 (implement with the branding foundation). | None. |
| GAP-019 | Toggle report elements from a doctor-facing control | MISSING | reference showcase, page 15 (show or hide any report element on demand, e.g. side address/title, signature, sample block) | `ReportSettings` is a single global row with no per-user layout entity; the only per-item switches in the product belong to the envelope; the dialogs folder holds only the secondary-password dialog. | STATICALLY VERIFIED | Persist per-user report-element visibility preferences and enforce them at report build time. | Report item taxonomy (stabilised by GAP-014; implement after it). | None. |
| GAP-020 | Optional vertical identity coding on the result report, receipt and envelope | MISSING | reference showcase, page 3 (vertical coding shown on the three documents; explicitly optional) | No setting, command or writer branch for vertical or rotated identity coding; the only barcode path renders horizontally on a single label. | STATICALLY VERIFIED | Add an optional vertical-coding rendering mode applied to the identity mark on all three documents, governed by a persisted toggle. | GAP-009 (same identity mark and label primitive; implement after it). | OD-03 (global versus per-document toggle). |
| GAP-021 | Sticker size configuration for barcode labels | MISSING | reference showcase, page 3 (label stock 38 mm x 25 mm) | `BarcodeLabelRenderer.Render` takes dimensions only from call-site constants (service invokes it with defaults); no size setting, command, DTO or view model exists. | STATICALLY VERIFIED | Persist label width and height and pass them through the barcode service into the renderer. | Label pipeline (exists); must precede GAP-009. | None. |
| GAP-022 | Sample-nature interpretation panel (nature, normal ranges, effect on the patient) | MISSING | reference showcase, page 51 (sample-nature interpretation entry) | `Test` carries collection and group labels and `ReferenceRange` supplies intervals, but no interpretation entity, command, query or panel presents a sample nature with its ranges and clinical meaning. | STATICALLY VERIFIED | Introduce sample-nature interpretation records with a read surface combining nature, ranges and interpretation text. | Catalogue and range data (exist). | None (editorial content risk recorded in phase). |
| GAP-023 | Glossary of abbreviations used in medical analysis | MISSING | reference showcase, page 51 (terminology lookup entry) | `Test.Short`/test codes are catalogue heading fields; no glossary entity, command, query or definition view exists. | STATICALLY VERIFIED | Introduce glossary records (abbreviation, full term, notes) with create/read/update/delete and a lookup surface. | None. | None. |
| GAP-024 | Appointment-note reminder inside the application | MISSING | reference showcase, page 51 (reminder note to a user or phone-book contact through the appointment notebook; in-application delivery, no external channel) | Phone-book storage and queries exist, but no reminder, appointment, note-scheduling or notification entity, command, query, DTO, view or view model exists. | STATICALLY VERIFIED | Introduce internal reminders (target, message, due date, state) with management and an in-application pending-reminder surface; no external channel. | Phone book (exists, unaffected). | OD-04 (scheduling specifics). |
| GAP-025 | Search and statistics on blood-picture element values in time periods | MISSING | reference showcase, page 73 (element-value band search over CBC analytes: HGB/HCT/MCV/MCH/MCHC/RETICS/PLT/PCT/MPV/LYM/MON/NEU/EOS/BAS/BAND with from/to bands and a statistics action) | The only banded monitor filters `PatientTest.ResultValue` scalars (`GetBandedResultMonitorQueryHandler.cs:57-105`); CBC results live as analyte-level rows in `ProfileResultItem`, which that query never reads. | STATICALLY VERIFIED | Extend banded search from test-level scalars to profile-analyte rows with per-element band inputs over a period, reusing the existing history pivot helper. | `ProfileResultItem` and `HistoryMatrixBuilder.Pivot` (exist; no schema change). | OD-06 (hosting note). |

Unresolved inventory items: none. Every inventoried in-scope function carries exactly one classification above or in the IMPLEMENTED population (127 functions, each covered by at least a Tier 1 reachability check and recorded in the verification working material).

---

## SECTION 4 — Ordered Implementation Phases

Phases are ordered by real code dependency. The phase that must be executed immediately next is Phase 1. Each phase introduces at most one new migration. Phases marked FINAL have no unresolved owner decision; phases marked NOT FINAL name the blocking decision.

### Phase 1 — Report settings foundation: header/footer images and normal-range marker

A. Phase identification
- Phase number: 1
- Descriptive title: Report settings foundation — image header/footer and normal-range marker
- Gap IDs: GAP-005, GAP-006, GAP-018
- Each covered function marked: GAP-005 DIFFERENT, GAP-006 DIFFERENT, GAP-018 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: size medium because three related settings share one aggregate and one writer region; risk medium because the Images mode is currently offered but silently falls back to words, so a wrong image-path policy could produce silently broken branding.

B. Dependency position
- Why now: This is the report-visual foundation. `ReportSettings` is a single global row and `ReportContentBuilder` decides header/footer rendering in one place (`ReportContentBuilder.cs:32,68,138`). All later report-visual phases edit the same file, so the image primitive and color mutators must land first to avoid repeated rework.
- Prerequisite phases or existing conditions: none. The `ReportSettings` entity and its configuration already exist; `HeaderColor`/`FooterColor` columns already exist and are seeded null.
- Exact code dependencies: `Domain/Settings/ReportSettings.cs`, `Infrastructure/Persistence/Configurations/ReportSettingsConfiguration.cs`, `Infrastructure/Printing/ReportContentBuilder.cs`, `Presentation/ViewModels/Settings/ReportSettingsViewModel.cs`, `Application/Features/SystemAndPrintSettings/Commands/UpdateReportSettings/UpdateReportSettingsCommandHandler.cs`.
- Downstream phases that depend on it: Phases 2, 3, 7, 8, 9, 10, 11 all touch `ReportContentBuilder` or `ReportSettings`.
- Shared state or execution paths affected: the single `ReportSettings` row (PK = 1) and every report shape built by `ReportContentBuilder`.

C. Required reference behaviour
- GAP-005: header/footer from attached image files (reference guide, page 194): the user can select an image file for the report header or footer; the three modes are none / words / image files.
- GAP-006: header/footer colours (reference guide, pages 195-198): the colour of individual header/footer elements can be changed.
- GAP-018: edit the character used to mark the normal range (reference showcase, page 17): the normal-range marker character can be changed.
- User workflow: open report settings, choose header/footer mode, pick image file or set colour, set marker character, save; reports render accordingly.

D. Current Top-Lab behaviour
- `HeaderFooterMode.Images` is offered in `ReportSettingsViewModel` but `ReportContentBuilder.cs:32,68,138` branches only on `!= None`; receipt and envelope writers fall back to the words header with an explicit code comment. No image column, file picker, or upload path exists (static inspection).
- `ReportSettings.HeaderColor`/`FooterColor` are declared and mapped but have no setter, no command field, no view-model property, and no writer read (7 code hits total; static inspection).
- GAP-018: the marker text is a fixed string in report content; no setting, command, or view-model exposes it (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- GAP-005: reference offers image mode; Top-Lab offers the mode but silently produces words. Delta: add persisted image references, file picker, image load/scaling in QuestPDF, fallback on missing file.
- GAP-006: reference allows per-element colour; Top-Lab stores null, inert columns. Delta: add mutators, command fields, settings controls, writer application.
- GAP-018: reference allows marker edit; Top-Lab hard-codes the marker. Delta: add nullable marker column and writer substitution.

F. Exact implementation changes by layer
- Domain: `ReportSettings` — add `HeaderImagePath` (nullable string), `FooterImagePath` (nullable string), `NormalRangeMarker` (nullable string).
- Application: `UpdateReportSettingsCommand` — add image path fields and marker field; `UpdateReportSettingsCommandHandler` — persist them.
- Infrastructure: `ReportSettingsConfiguration` — map the three new nullable columns; `ReportContentBuilder` — render header/footer images and apply marker substitution.
- Presentation: `ReportSettingsViewModel` — image picker and colour/marker controls; view (XAML) bindings.
- Tests: settings unit tests for image/color/marker persistence; regression tests for report rendering in Words mode (existing) unchanged.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add `HeaderImagePath`, `FooterImagePath`, `NormalRangeMarker` (all nullable strings) to `ReportSettings`.
- Back-fill: none required (nullable, default null).
- Compatibility: additive only; existing migrations unchanged.
- Evidence state not already persisted: the three columns do not exist in any migration or the model snapshot.
- One migration only for this phase.

H. Business rules and message text
- No new user-visible message text established by the reference pages beyond the mode selection. Validation: image file must be a supported image type and within size limits; colour values stored as strings consistent with existing `HasMaxLength(9)` column convention.

I. Objective acceptance criteria
- Given a report settings screen, when the user selects Images mode and picks a header image, then the saved `HeaderImagePath` is non-null and appears in report header output.
- Given the same screen, when the user sets a header colour, then `HeaderColor` is persisted and applied by the writer.
- Given the same screen, when the user sets the normal-range marker, then `NormalRangeMarker` is persisted and substituted in report output.
- Regression: existing Words-mode reports continue to render unchanged.

J. Test plan
- Domain/Application: settings persistence unit tests for new columns.
- Infrastructure: `ReportContentBuilder` image-rendering test with a bundled test image; fallback test when file missing.
- Presentation: settings view model test for image picker command.
- Regression: existing report golden tests for Words mode must still pass.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite (Domain, Application, Infrastructure, Persistence, Presentation).
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.
Additional Windows check: image rendering on Windows with QuestPDF.

L. Risks and rollback
- Risk: image path stored becomes environment-dependent. Mitigation: document asset-location policy (embed vs content path).
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 2 — Receipt font type, size, colour, and laboratory logo

A. Phase identification
- Phase number: 2
- Descriptive title: Receipt header/footer font controls and logo
- Gap IDs: GAP-007
- Each covered function marked: GAP-007 DIFFERENT
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because six of eight documented receipt controls already exist; only font family/size/colour and logo image are missing. Risk medium because font settings may not be honoured by the font resolver on all Windows configurations, and logo rendering requires a real Images branch in the receipt writer.

B. Dependency position
- Why now: reuses the image-asset primitive and colour primitive introduced in Phase 1. Completing Phase 1 first means this phase is a configuration job on top of shared primitives rather than building a second primitive.
- Prerequisite phases or existing conditions: Phase 1 complete (image-asset pattern, HeaderFooterMode.Images handling). `ReceiptSettings` entity exists with six of eight documented controls.
- Exact code dependencies: `Domain/Settings/ReceiptSettings.cs`, `Infrastructure/Persistence/Configurations/ReceiptSettingsConfiguration.cs`, `Infrastructure/Printing/ReceiptPdfWriter.cs`, `Presentation/ViewModels/Settings/ReceiptSettingsViewModel.cs`, `Application/Features/SystemAndPrintSettings/Commands/UpdateReceiptSettings`.
- Downstream phases that depend on it: Phase 4 (receipt logo reuses these columns).
- Shared state or execution paths affected: the single `ReceiptSettings` row and the receipt printing path (`ReceiptPdfWriter`).

C. Required reference behaviour
- GAP-007: receipt settings font type, size, colour, and logo (reference guide, pages 202-204): choose the font type, font size and font colour for receipt header/footer and choose a laboratory logo image.
- User workflow: open receipt settings, set header/footer font family/size/colour, pick logo image, save; receipts render with chosen fonts and logo.

D. Current Top-Lab behaviour
- `ReceiptSettings` models margins, currency, pickup time, print-once, detail mode, cashier-printer flag, and header/footer mode; no font family/size/colour and no logo reference exist (static inspection).
- `ReceiptPdfWriter.cs:200-202` falls back to the words header when `HeaderFooterMode == Images`; no logo asset column, picker, or upload path exists (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: font type/size/colour and logo on receipts. Current: none of the four controls exist; logo path inert. Delta: persist font family/size/colour and logo path; expose on receipt-settings screen; render fonts and logo in receipt writer with real Images branch.

F. Exact implementation changes by layer
- Domain: `ReceiptSettings` — add `HeaderFontFamily`, `HeaderFontSizePt`, `HeaderFontColor`, `FooterFontFamily`, `FooterFontSizePt`, `FooterFontColor`, `LogoImagePath` (all nullable).
- Application: `UpdateReceiptSettingsCommand` — add the new fields; handler persists them.
- Infrastructure: `ReceiptSettingsConfiguration` — map the seven new nullable columns; `ReceiptPdfWriter` — real Images branch rendering logo and applying fonts/colours.
- Presentation: `ReceiptSettingsViewModel` — font and logo controls; view bindings.
- Tests: receipt writer font/logo rendering test; settings persistence tests; regression: existing receipt Words-mode tests unchanged.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add the seven nullable columns to `ReceiptSettings`.
- Back-fill: none required.
- Compatibility: additive only; existing migrations unchanged.
- Evidence state not already persisted: none of the seven columns exists in any migration or the model snapshot.
- One migration only for this phase.

H. Business rules and message text
- Validation: font family must be a valid installed font name; size within reasonable bounds; colour strings consistent with existing conventions. Logo file must be a supported image type within size limits.

I. Objective acceptance criteria
- Given receipt settings, when the user sets header font family/size/colour, then the values are persisted and applied to rendered receipt output.
- Given receipt settings, when the user picks a logo image, then `LogoImagePath` is persisted and the logo appears on the receipt for both plain and pre-printed paper.
- Regression: existing receipt output without logo unchanged.

J. Test plan
- Domain/Application: receipt settings persistence unit tests.
- Infrastructure: `ReceiptPdfWriter` logo/font rendering test with bundled image and test fonts.
- Presentation: receipt settings view model tests.
- Regression: existing receipt printing tests must still pass.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.
Additional Windows check: receipt font rendering with installed system fonts.

L. Risks and rollback
- Risk: font settings that cannot be honoured by the font resolver silently substitute. Mitigation: document that fonts are resolved at render time; prefer widely available fonts.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 3 — Report on plain or pre-printed paper with a logo

A. Phase identification
- Phase number: 3
- Descriptive title: Report logo on pre-printed or plain paper
- Gap IDs: GAP-011
- Each covered function marked: GAP-011 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because it is pure configuration and rendering of the primitive built in Phase 1; carries no logic of its own. Low risk because once Phase 1 exists this is a writer branch.

B. Dependency position
- Why now: depends on Phase 1 (image primitive) and Phase 1 colour support. Must not be attempted before Phase 1.
- Prerequisite phases or existing conditions: Phase 1 complete. Plain-paper path is the default and works; pre-printed path is approximated by Words mode.
- Exact code dependencies: `Infrastructure/Printing/ReportContentBuilder.cs`, `Infrastructure/Printing/ReportPdfWriter.cs`, `Domain/Settings/ReportSettings.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: report writing path; shares `ReportSettings` row with Phase 1.

C. Required reference behaviour
- GAP-011: print the report on plain or pre-printed paper with a logo (reference showcase, pages 5, 16): paper choice and logo branding for reports.
- User workflow: choose paper type (plain or pre-printed) and logo; reports render with logo and appropriate positioning.

D. Current Top-Lab behaviour
- Plain-paper path is the default and works. Pre-printed path approximated by `HeaderFooterMode.Words`; logo half not implemented (`ReportContentBuilder.cs:32,68,138` never loads an image) (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: logo on report for both paper choices. Current: words branding present; image branding inert. Delta: render persisted header/footer images on the report for both paper choices.

F. Exact implementation changes by layer
- Infrastructure: `ReportContentBuilder` / `ReportPdfWriter` — load and render persisted header/footer images; paper-choice handling.
- No new Domain columns (reuses Phase 1 columns); no new Application fields (reuse existing settings command).
- Tests: report writer logo rendering test; regression: existing report tests unchanged.

G. Migration decision: NO MIGRATION
- Reason: reuses the `HeaderImagePath`/`FooterImagePath` columns added by Phase 1; no new schema required. The change is rendering-only over existing columns.

H. Business rules and message text
- No new message text beyond existing settings wording. Validation: image file must exist at render time; fallback to words when missing.

I. Objective acceptance criteria
- Given report settings with header/footer images configured, when a report is generated on plain paper, then the images appear in header/footer.
- Given the same settings on pre-printed paper, then the images appear positioned per configuration.

J. Test plan
- Infrastructure: report writer image rendering test for both paper choices.
- Regression: existing report golden tests must still pass.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify (NO MIGRATION).
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: logo colliding with pre-printed stationery. Mitigation: configurable positioning.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 4 — Receipt header/footer logo

A. Phase identification
- Phase number: 4
- Descriptive title: Receipt header/footer logo rendering
- Gap IDs: GAP-010
- Each covered function marked: GAP-010 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because it reuses the logo column from Phase 2; same reasoning as Phase 3 on the receipt surface. Low risk.

B. Dependency position
- Why now: depends on Phase 2 (logo column on ReceiptSettings). Same reasoning as Phase 3.
- Prerequisite phases or existing conditions: Phase 2 complete.
- Exact code dependencies: `Infrastructure/Printing/ReceiptPdfWriter.cs`, `Domain/Settings/ReceiptSettings.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: receipt writing path; shares `ReceiptSettings` row with Phase 2.

C. Required reference behaviour
- GAP-010: receipt header/footer logo on pre-printed or plain receipt paper (reference showcase, page 4): receipt paper choice with logo branding.
- User workflow: logo configured on receipt settings; receipts render with the logo for both paper choices.

D. Current Top-Lab behaviour
- Words half implemented; logo half inert (`ReceiptPdfWriter.cs:200-202` falls back to words; no logo asset) (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: logo on receipts. Current: words branding present; logo inert. Delta: render persisted logo on the receipt for both paper choices.

F. Exact implementation changes by layer
- Infrastructure: `ReceiptPdfWriter` — real Images branch rendering persisted logo.
- No new schema (reuses Phase 2 columns).
- Tests: receipt writer logo rendering test; regression: existing receipt tests unchanged.

G. Migration decision: NO MIGRATION
- Reason: reuses the `LogoImagePath` column added by Phase 2; rendering-only change over existing columns.

H. Business rules and message text
- Validation: logo image supported type and size; fallback to words when missing.

I. Objective acceptance criteria
- Given receipt settings with logo configured, when a receipt is printed, then the logo appears on the receipt for both plain and pre-printed paper.

J. Test plan
- Infrastructure: receipt writer logo test.
- Regression: existing receipt tests must still pass.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 5 — Patient receivable account summary with balance filter

A. Phase identification
- Phase number: 5
- Descriptive title: Patient receivable balance filter (unpaid / discounted / paid only)
- Gap IDs: GAP-008
- Each covered function marked: GAP-008 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because the data needed already exists (`PaymentOperation.OperationType` distinguishes payment, correction, extra charge, void); the change is a query predicate only. Low risk.

B. Dependency position
- Why now: purely additive query surface; touches no shared file with other phases; can be done at any point. Placed early because it is small and independent.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/InventoryAndAccounting/Queries/GetPatientSamplesDetail`, `InventoryDtos.cs`, presentation view for account summary.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: patient-level account query; no schema change.

C. Required reference behaviour
- GAP-008: patient receivable account summary for a period with a balance filter (reference showcase, page 26): period receivable summary filtered to unpaid, discounted or paid-only.
- User workflow: open receivable summary for a period, choose balance filter (unpaid-only / discounted-only / paid-only), view filtered patient rows with charged/paid/balance.

D. Current Top-Lab behaviour
- `GetPatientSamplesDetailQuery(From, To)` returns per-patient charged/paid/balance with no filter parameter; no filter for bad-debt-only, discounted-only, or paid-only exists on any patient-level account query (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: three-way balance filter. Current: period summary exists but no filter. Delta: add three optional filter booleans plus predicate to the patient account query and surface on screen.

F. Exact implementation changes by layer
- Application: `GetPatientSamplesDetailQuery` — add `UnpaidOnly`, `DiscountedOnly`, `PaidOnly` parameters; handler applies predicate based on `PaymentOperation` classification.
- Presentation: account summary view — filter controls and bindings.
- Tests: query predicate unit tests; regression: existing account queries unchanged.
- No Domain or Infrastructure schema changes.

G. Migration decision: NO MIGRATION
- Reason: the data required already exists (`PaymentOperation.OperationType`); the filter is a query predicate only. No schema change required.

H. Business rules and message text
- Validation: "discounted" defined consistently with the discount representation already used by `PaymentOperation`.

I. Objective acceptance criteria
- Given a period with mixed payment states, when the user applies the unpaid-only filter, then only patients with unpaid balance appear.
- Given the same period, when the user applies paid-only, then only fully paid patients appear.
- Given the same period, when the user applies discounted-only, then only patients with discount operations appear.

J. Test plan
- Application: filter predicate unit tests covering each balance state.
- Regression: existing account summary tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: inconsistent definition of "discounted". Mitigation: align with existing `PaymentOperation` discount semantics.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 6 — Group-patient report slip

A. Phase identification
- Phase number: 6
- Descriptive title: Group-patient report slip (one slip per patient for a whole group)
- Gap IDs: GAP-013
- Each covered function marked: GAP-013 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because a new report shape with pagination (one slip per patient) and a reuse decision between `ReportPdfWriter` and the worksheet writer. Medium risk because of possible duplication of patient-data rendering rather than sharing it.

B. Dependency position
- Why now: independent of all other phases; touches no entity and no shared file with Phases 1-5. Placed here because it is query-surface work and small enough to proceed early.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/PatientSearch` or `Application/Features/ReportProduction` queries; `Domain/Patients/Patient`, `Domain/Results/PatientTest`, `Domain/Tests/Test`; presentation view for group-slip.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: patient/visit-test read path; no schema change.

C. Required reference behaviour
- GAP-013: group-patient report slip (one slip per patient for a whole group of patients) (reference guide, pages 52-57; reference showcase, page 6): one slip per patient, archive-safe, to prevent errors and keep an archive.
- User workflow: select a group of patients, generate slips (one per patient), print or archive each slip.

D. Current Top-Lab behaviour
- No command, query, DTO, writer, view, or view model produces a one-line-per-patient slip (systematic negative search). Nearest artefacts are the bench worksheet and the collection sheet, which are different documents (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: per-patient slips for a group. Current: absent. Delta: add a group-slip query and writer producing one slip per patient with the reference layout, reachable from the follow-up/delivery area.

F. Exact implementation changes by layer
- Application: new group-slip query and handler; DTO per patient slip.
- Infrastructure: new PDF writer or reuse of existing writer for the slip layout; pagination (one slip per patient).
- Presentation: view/view model to select patients and trigger slip generation; navigation entry.
- Tests: slip generation unit test; regression: existing report tests unchanged.
- No Domain or Infrastructure schema changes.

G. Migration decision: NO MIGRATION
- Reason: reads `Patient`, `PatientTest`, and `Test`, all already persisted. No new column required.

H. Business rules and message text
- No specific message text established by the reference beyond the slip layout. Validation: patient selection must be non-empty.

I. Objective acceptance criteria
- Given a group of selected patients, when the user generates the slip, then one slip per patient is produced with patient data and tests.
- Given the group, when printed/archived, then each slip is archive-safe and separate.

J. Test plan
- Application: group-slip query unit test.
- Infrastructure: slip writer output test.
- Regression: existing report/worksheet tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: duplicating patient-data rendering instead of sharing it. Mitigation: reuse existing patient-data rendering components.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 7 — Editable report item template

A. Phase identification
- Phase number: 7
- Descriptive title: Editable report item template (letters, reference values, show/hide, extensible lists, free text)
- Gap IDs: GAP-014
- Each covered function marked: GAP-014 MISSING
- Estimated size: high
- Estimated risk: high
- Rationale: high size because it introduces an item-template aggregate and rewrites `ReportContentBuilder`'s fixed section sequence, shared by three report shapes. High risk because of regressions in the two report shapes currently correct.

B. Dependency position
- Why now: this is the widest change in the report-visual sequence. It rewrites the very code path Phases 1, 2, 9, 10, 11 modify, so it must be last among the report-visual items to avoid repeated rework.
- Prerequisite phases or existing conditions: Phases 1, 2 complete (branding foundation and image/color primitives applied to `ReportContentBuilder`).
- Exact code dependencies: `Infrastructure/Printing/ReportContentBuilder.cs`, `Infrastructure/Printing/ReportDocumentContent.cs`, new item-template aggregate.
- Downstream phases that depend on it: Phase 8, 9, 10 (per-item layout and colouring build on item template).
- Shared state or execution paths affected: `ReportContentBuilder` fixed section sequence used by three report shapes.

C. Required reference behaviour
- GAP-014: edit report item letters and reference values; hide/show items; extend selection lists; allow free text (reference showcase, page 12): configure the report's item layout.
- User workflow: open report item template management, edit item letters/reference values, toggle show/hide, extend selection lists, allow free text; reports render per template.

D. Current Top-Lab behaviour
- `ReportContentBuilder.cs` emits a fixed section sequence for every report; no item-template entity, command, DTO, or show/hide/free-text construct exists (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: editable item template. Current: fixed section sequence. Delta: introduce item-template aggregate with management surface and drive the builder from it.

F. Exact implementation changes by layer
- Domain: new item-template aggregate (item key, label, reference-value expression, show/hide, list membership, free-text override).
- Application: CRUD commands/queries for item templates; handler to build report sections from templates.
- Infrastructure: EF configuration for item-template table; `ReportContentBuilder` rewritten to emit sections from the template instead of fixed sequence.
- Presentation: item-template management view/view model; bindings.
- Tests: template CRUD tests; builder section-emission tests; regression: existing report tests for current fixed sequence must still pass.
- Migration: one new migration for item-template rows.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add item-template table rows (item key, label, reference-value expression, show/hide, list membership, free-text override).
- Back-fill: seed default templates matching current fixed section sequence so existing reports render identically.
- Compatibility: additive only; existing migrations unchanged.
- Evidence state not already persisted: no item-template table exists in any migration or the model snapshot.
- One migration only for this phase.

H. Business rules and message text
- Validation: item key unique; reference-value expression valid; show/hide toggle boolean.

I. Objective acceptance criteria
- Given the item-template management screen, when the user edits an item letter or reference value, then reports render using the edited values.
- Given the same screen, when the user hides an item, then the item is absent from report output.
- Given the same screen, when the user allows free text for an item, then free text can be entered at report time.
- Regression: with default templates seeded, existing report output unchanged.

J. Test plan
- Domain/Application: item-template CRUD unit tests.
- Infrastructure: builder section-from-template unit test; migration seed test.
- Presentation: template management view model tests.
- Regression: existing report golden tests must still pass with default templates.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database (including seed data).
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: regression in report shapes currently correct. Mitigation: seed default templates reproducing the fixed sequence; run full regression suite.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 8 — Line spacing, item distance from margin, item symbol

A. Phase identification
- Phase number: 8
- Descriptive title: Per-item line spacing, left offset, and marker symbol
- Gap IDs: GAP-015
- Each covered function marked: GAP-015 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because per-item offset storage and marker glyph per item; the envelope already models this (`EnvelopePrintItemPosition`) which can be generalised. Medium risk of diverging from the envelope's proven model.

B. Dependency position
- Why now: `ReportTopSpaceCm`/`PageMarginLeftCm` handling lives in the same `ReportPageComposer` region Phases 1, 2, 6 modify; implement after Phase 7 (item template) so per-item layout is part of the template.
- Prerequisite phases or existing conditions: Phase 7 complete (item template aggregate exists).
- Exact code dependencies: `Infrastructure/Printing/ReportPageComposer.cs`, new per-item layout rows on item template.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: report composer region shared with Phase 1.

C. Required reference behaviour
- GAP-015: control line spacing, item distance from the left margin, and the symbol before each item (reference showcase, page 13): layout and marker control.
- User workflow: open item template settings, set line spacing, left distance, and symbol per item; reports render per item layout.

D. Current Top-Lab behaviour
- Only page margins and top space persisted (`PageMarginLeftCm`, `PageMarginBottomCm`, `ReportTopSpaceCm`); no line-spacing field, no per-item report offset (per-item offsets exist only for the envelope), no marker configuration (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: per-item line spacing, left offset, symbol. Current: none. Delta: persist per-item layout and apply in report composer.

F. Exact implementation changes by layer
- Domain: item-template aggregate — add line spacing, left offset, marker symbol fields.
- Application: extend item-template commands/queries.
- Infrastructure: `ReportPageComposer` — apply per-item offsets and marker glyphs.
- Presentation: item-template view — per-item layout controls.
- Tests: per-item layout rendering test; regression: existing report tests unchanged.
- Migration: additive columns on the item-template table introduced by Phase 7 — can be folded into Phase 7's migration or a separate one; since Phase 7 already introduces the item-template table, Phase 8 can extend it. To keep one migration per phase and avoid re-editing Phase 7's migration, Phase 8 adds columns via its own migration appended after Phase 7's.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after Phase 7's migration; add line-spacing, left-offset, marker-symbol nullable columns to the item-template table.
- Back-fill: none required (nullable).
- Compatibility: additive only.
- Evidence state not already persisted: no per-item layout columns exist.
- One migration only for this phase.

H. Business rules and message text
- Validation: offset values within reasonable bounds; marker symbol single character.

I. Objective acceptance criteria
- Given item template settings, when the user sets line spacing for an item, then report output reflects the spacing.
- Given the same settings, when the user sets left offset, then the item is positioned accordingly.
- Given the same settings, when the user sets a marker symbol, then the symbol appears before the item.

J. Test plan
- Application: item-template layout field tests.
- Infrastructure: composer per-item offset test.
- Regression: existing report tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: diverging from the envelope's proven per-item offset model. Mitigation: generalise the envelope's `EnvelopePrintItemPosition` shape.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 9 — Shade or colour abnormal results

A. Phase identification
- Phase number: 9
- Descriptive title: Shade or colour abnormal results (with disable option)
- Gap IDs: GAP-016
- Each covered function marked: GAP-016 MISSING
- Estimated size: medium
- Estimated risk: low
- Rationale: medium size because it writes into `ReportContentBuilder`'s result-cell rendering; low risk because driven by the already-computed `ResultFlag`.

B. Dependency position
- Why now: the colour of an abnormal result and the layout of its row are the same cell; implement after Phase 8 (per-item layout) so cell composition is stable. Before Phase 10 (per-profile override).
- Prerequisite phases or existing conditions: Phase 8 complete (item-template/layout region stable).
- Exact code dependencies: `Infrastructure/Printing/ReportContentBuilder.cs`, `Domain/Results/ResultFlagComputer.cs`, `Domain/Settings/ReportSettings.cs`.
- Downstream phases that depend on it: Phase 10.
- Shared state or execution paths affected: result-cell rendering shared with Phase 10.

C. Required reference behaviour
- GAP-016: shade or colour abnormal results, or disable it (reference showcase, page 13): highlight out-of-range results with an off switch.
- User workflow: open report settings, enable/disable abnormal-result colouring and choose colours; abnormal results render shaded/coloured.

D. Current Top-Lab behaviour
- `ResultFlagComputer` computes Low/High and the builder prints low/high comment text; no colour or background is ever applied to a result cell (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: abnormal results shaded/coloured with off switch. Current: flag computed but not visually treated. Delta: add global enable flag plus abnormal-result colours; apply to flagged result cells.

F. Exact implementation changes by layer
- Domain: `ReportSettings` — add abnormal-result enable flag and low/background colour columns.
- Application: extend settings command/handler.
- Infrastructure: `ReportContentBuilder` — apply `.FontColor`/`.Background` to result cells driven by `ResultFlag`.
- Presentation: settings view — colour controls.
- Tests: result-cell colouring test driven by flag; regression: existing report tests unchanged.
- Migration: one new migration for ReportSettings colour columns.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration (after Phase 8's migration); add abnormal-result enable flag and low/abnormal-background colour columns to `ReportSettings`.
- Back-fill: default flag off so existing reports unchanged.
- Compatibility: additive only.
- Evidence state not already persisted: no abnormal-result colour columns exist.
- One migration only for this phase.

H. Business rules and message text
- Validation: colour strings consistent with existing conventions; enable flag boolean.

I. Objective acceptance criteria
- Given abnormal-result colouring enabled, when a result is flagged High/Low, then the result cell is coloured/shaded in report output.
- Given the same setting disabled, then flagged results render without colour/shading.

J. Test plan
- Infrastructure: result-cell colouring unit test for each flag state.
- Regression: existing report golden tests unchanged with flag off by default.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: contrast failure in print. Mitigation: choose high-contrast colours; provide disable option.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 10 — Per-profile normal/abnormal result font and background colour

A. Phase identification
- Phase number: 10
- Descriptive title: Per-profile control of normal/abnormal result font and background colour
- Gap IDs: GAP-017
- Each covered function marked: GAP-017 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because per-profile override plus precedence rule (profile override vs global). Main design risk is precedence.

B. Dependency position
- Why now: the per-profile variant is Phase 9 plus a per-profile override; implement after Phase 9 so the base colouring cell-rendering code is not duplicated.
- Prerequisite phases or existing conditions: Phase 9 complete (base colouring exists).
- Exact code dependencies: `Domain/Tests/Profile.cs`, `Domain/Tests/ProfileAnalyte.cs`, `Infrastructure/Printing/ReportContentBuilder.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: result-cell rendering shared with Phase 9.

C. Required reference behaviour
- GAP-017: per-profile control of normal/abnormal result font and background colour (reference showcase, page 16): per-profile result styling.
- User workflow: open profile settings, set normal/abnormal font and background colour per profile; reports render per profile with override precedence.

D. Current Top-Lab behaviour
- No per-profile styling attribute, command, DTO, or writer branch exists on `Profile`, `ProfileAnalyte`, or `ProfileResultItem` (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: per-profile styling with precedence. Current: none. Delta: add per-profile styling attributes with documented precedence against global colours; apply in writers.

F. Exact implementation changes by layer
- Domain: `Profile`/`ProfileAnalyte` — add normal-font, normal-colour, abnormal-font, abnormal-background columns.
- Application: extend profile commands/queries.
- Infrastructure: `ReportContentBuilder` — apply per-profile styling with precedence over global.
- Presentation: profile editor view — colour controls.
- Tests: per-profile styling precedence test; regression: existing profile/report tests unchanged.
- Migration: one new migration for per-profile colour columns.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration (after Phase 9's migration); add normal-font, normal-colour, abnormal-font, abnormal-background columns to `Profile` (or `ProfileAnalyte`).
- Back-fill: null by default so global colours apply.
- Compatibility: additive only.
- Evidence state not already persisted: no per-profile styling columns exist.
- One migration only for this phase.

H. Business rules and message text
- Validation: precedence rule documented (profile override vs global); colour strings consistent with conventions.

I. Objective acceptance criteria
- Given a profile with abnormal colour configured, when a result in that profile is flagged abnormal, then the profile colour overrides the global colour.
- Given a profile without override, then the global colour from Phase 9 applies.

J. Test plan
- Application: profile styling unit tests.
- Infrastructure: precedence rendering test.
- Regression: existing profile tests unchanged with null defaults.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: precedence ambiguity between profile and global. Mitigation: document precedence rule explicitly.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 11 — Verify / unverify simple single-value results

A. Phase identification
- Phase number: 11
- Descriptive title: Wire verify/unverify for simple results to the entry surface
- Gap IDs: GAP-001
- Each covered function marked: GAP-001 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because the commands and handlers already exist (`ReviewResultCommandHandler`, `UnreviewResultCommandHandler`, `MarkAllPatientResultsReviewedCommandHandler`) and profile/culture verify paths are wired; only the simple-result entry surface is missing the action. Low risk.

B. Dependency position
- Why now: uses existing commands, entities, and permissions; no schema change; independent of all other phases. Can be done early.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/ResultsEntry/Commands/ReviewResult`, `Application/Features/ResultsEntry/Commands/UnreviewResult`, `Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: result-entry surface; no shared file with report-visual phases.

C. Required reference behaviour
- GAP-001: verify/unverify results (reference guide, pages 28, 54): mark a result as reviewed/verified and reverse that, for simple single-value results as for profile and culture.
- User workflow: open simple result entry for an entered result, verify it, and later unverify it.

D. Current Top-Lab behaviour
- `ReviewResultCommandHandler`/`UnreviewResultCommandHandler` exist but have no sender in `src/TopLab.Presentation`; `SimpleResultEntryViewModel.cs` exposes only save, clear, and comment-picker (static inspection). Profile and culture verify paths are wired (`ProfileEntryViewModel.cs:303,340`, `CultureEntryViewModel.cs:404`).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: verify/unverify for simple results. Current: command exists but unreachable from the simple-result surface. Delta: wire existing commands to the entry surface with permission checks.

F. Exact implementation changes by layer
- Presentation: `SimpleResultEntryViewModel` — verify/unverify commands bound to `ReviewResultCommand`/`UnreviewResultCommand`; permission check (`REVIEW_RESULTS`).
- Application: no change (commands exist); ensure authorization behaviour applies.
- Tests: handler wiring test; regression: existing profile/culture verify tests unchanged.
- No Domain or Infrastructure changes.

G. Migration decision: NO MIGRATION
- Reason: commands, handlers, entities, and permissions already exist; only UI wiring is added. No schema change required.

H. Business rules and message text
- Validation: only entered results can be verified; verified results can be unverified unless printed/delivered (existing domain locks `PatientTest.cs:129-153`).

I. Objective acceptance criteria
- Given an entered simple result, when the user verifies it, then `IsReviewed` is set and the command succeeds.
- Given a verified simple result, when the user unverifies it, then `IsReviewed` is cleared.
- Given a printed/delivered result, then unverify is rejected by the domain lock.

J. Test plan
- Application: verify/unverify handler tests for simple results.
- Presentation: view model command wiring test.
- Regression: existing verify tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 12 — Culture sensitivity column control and antibiotic of choice

A. Phase identification
- Phase number: 12
- Descriptive title: Culture report column control and antibiotic of choice
- Gap IDs: GAP-002, GAP-012
- Each covered function marked: GAP-002 DIFFERENT, GAP-012 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because both functions touch the culture report section (`ReportCultureSection`) and the sensitivity grid; one migration can add the commercial-name visibility flag and the antibiotic-of-choice field. Medium risk because the grid currently excludes commercial name by design.

B. Dependency position
- Why now: both functions share the culture report section and one migration; independent of report-visual phases. Can proceed after culture entry grid exists (it does).
- Prerequisite phases or existing conditions: none (culture entry grid and report section exist).
- Exact code dependencies: `Application/Features/CultureResults/Queries/GetCultureReport`, `Infrastructure/Printing/ReportCultureSection.cs`, `Domain/Tests/Antibiotic.cs`, new culture-result choice field.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: culture report grid shared with GAP-002/GAP-012.

C. Required reference behaviour
- GAP-002: control display of Sensitivity / Reference / Commercial Name columns (reference guide, page 39): toggle which of the three columns appears in the culture report.
- GAP-012: select the antibiotic of choice (reference guide, page 39): mark the preferred antibiotic for the culture.
- User workflow: open culture entry/report settings, toggle column visibility, select antibiotic of choice; culture report renders selected columns and marks the chosen antibiotic.

D. Current Top-Lab behaviour
- Master data present (`Antibiotic.Symbol/ScientificName/CommercialName`, latest migration adds `CommercialName`); report grid fixed at four columns with no commercial-name column and no toggle (`ReportCultureSection.cs:113-124`). No choice field, command, or view-model property exists (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: three-column toggle and antibiotic-of-choice mark. Current: fixed grid, no choice. Delta: persist three visibility flags and one choice field per culture result; render grid with selected columns and mark the choice.

F. Exact implementation changes by layer
- Domain: `CultureAntibioticResult` or new culture-result choice field — add `IsAntibioticOfChoice`; culture report settings — add three visibility flags.
- Application: culture report query — include selected columns; handler to persist choice.
- Infrastructure: `ReportCultureSection.BuildSensitivityGrid` — render selected columns; EF configuration for new columns/flags.
- Presentation: culture entry/report view — column toggle and choice selection.
- Tests: sensitivity grid rendering test; choice persistence test; regression: existing culture report tests unchanged.
- Migration: one new migration for visibility flags and choice field.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add sensitivity-column visibility flags (nullable booleans) and antibiotic-of-choice field to the culture result/report settings table.
- Back-fill: default flags to show all three columns; choice null.
- Compatibility: additive only.
- Evidence state not already persisted: no visibility flags or choice field exist.
- One migration only for this phase.

H. Business rules and message text
- Validation: at least one column visible; choice refers to an attached antibiotic.

I. Objective acceptance criteria
- Given culture report settings, when the user hides the Reference column, then the grid renders without it.
- Given the same settings, when the user shows the Commercial Name column, then it appears in the grid.
- Given a culture with an antibiotic marked as choice, then the report marks it as the antibiotic of choice.

J. Test plan
- Application: culture report column-visibility tests.
- Infrastructure: grid rendering test for each column combination.
- Regression: existing culture report tests unchanged with default all-visible.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: grid layout breakage when columns toggled. Mitigation: render only selected columns with stable widths.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 13 — Worksheet unfinished-only switch

A. Phase identification
- Phase number: 13
- Descriptive title: Worksheet unfinished-only / complete switch
- Gap IDs: GAP-004
- Each covered function marked: GAP-004 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because worksheet queries exist; only an unfinished-only parameter is missing. Low risk.

B. Dependency position
- Why now: additive query parameter; independent of all other phases. Small and safe to do early.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/WorkSheets/Queries/GetVisitWorkSheetQuery`, `GetWorkSheetByTestGroupQuery`, `GetWorkSheetByWorkGroupLogQuery`, `Presentation/ViewModels/WorkSheets/WorkSheetsViewModel.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: worksheet query path; no schema change.

C. Required reference behaviour
- GAP-004: print the worksheet unfinished only or all (reference guide, pages 86, 89): restrict the worksheet to unfinished work via a switch.
- User workflow: open worksheet view, toggle unfinished-only, view/print restricted or complete worksheet.

D. Current Top-Lab behaviour
- Worksheet queries exist with no unfinished-only parameter; print command is per-patient; only the outside-laboratory exclusion exists (a different filter) (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: unfinished-only switch. Current: no such parameter. Delta: add unfinished-only option to worksheet queries and surface as a switch on the view.

F. Exact implementation changes by layer
- Application: worksheet queries — add `OnlyUnfinished` boolean parameter; handlers filter `PatientTest` where result not entered.
- Presentation: worksheet view — switch control and binding.
- Tests: query filter unit test; regression: existing worksheet tests unchanged.
- No Domain or Infrastructure schema changes.

G. Migration decision: NO MIGRATION
- Reason: query predicate only over existing `PatientTest` columns (`EnteredAtUtc`/`ResultValue`). No schema change required.

H. Business rules and message text
- Validation: unfinished defined as no result entered; consistent with existing outside-laboratory exclusion semantics.

I. Objective acceptance criteria
- Given a period with entered and unentered results, when the user toggles unfinished-only, then only unentered results appear on the worksheet.
- Given the same period with the switch off, then all results appear.

J. Test plan
- Application: worksheet query filter tests for entered/unentered states.
- Regression: existing worksheet tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 14 — Print-time reference value override for one patient

A. Phase identification
- Phase number: 14
- Descriptive title: Print-time reference value override (single patient test)
- Gap IDs: GAP-003
- Each covered function marked: GAP-003 DIFFERENT
- Estimated size: low
- Estimated risk: low
- Rationale: low size because per-patient frozen-range snapshot infrastructure and `RefreshResultReferenceRangeCommand` handler already exist; only the UI sender is missing. Low risk.

B. Dependency position
- Why now: uses existing snapshot and command; no schema change; independent of all other phases.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/ResultsEntry/Commands/RefreshResultReferenceRange`, `Application/Features/ResultsEntry/Common/ResultReferenceRangeSource.cs`, `Presentation/ViewModels/Patients/SimpleResultEntryViewModel.cs` or report view.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: result-entry/report surface; no shared file with report-visual phases.

C. Required reference behaviour
- GAP-003: temporarily override a reference value at print time for one patient (reference guide, pages 101-102): open the reference cell from the report, edit, refresh and accept for that patient only.
- User workflow: open a result's reference cell at print time, edit, refresh to re-resolve the live range, accept; scoped to one patient's test.

D. Current Top-Lab behaviour
- Per-patient frozen-range snapshot infrastructure exists (`ResultsEntryDtos.cs:24-52`, `ResultReferenceRangeSource.cs`); `RefreshResultReferenceRangeCommand` handler exists but has no sender in `src/TopLab.Presentation` (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: print-time refresh scoped to one patient. Current: command exists but unreachable. Delta: wire the command to the report/entry surface for a single patient test.

F. Exact implementation changes by layer
- Presentation: report/entry view — refresh-reference action bound to `RefreshResultReferenceRangeCommand` for the selected patient test.
- Application: no change (command exists).
- Tests: refresh command test scoped to one patient; regression: existing snapshot tests unchanged.
- No Domain or Infrastructure changes.

G. Migration decision: NO MIGRATION
- Reason: snapshot model and command already exist; only UI wiring added. No schema change required.

H. Business rules and message text
- Validation: refresh allowed for entered results; scoped to one patient's test only (existing `PatientTestReferenceRangeSnapshot`).

I. Objective acceptance criteria
- Given an entered result, when the user refreshes its reference range at print time, then the snapshot for that patient's test is re-resolved and re-frozen.
- Given another patient's test, then its snapshot is unaffected.

J. Test plan
- Application: refresh command test verifying single-patient scoping.
- Regression: existing snapshot tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 15 — Barcode label size configuration

A. Phase identification
- Phase number: 15
- Descriptive title: Barcode label size configuration
- Gap IDs: GAP-021
- Each covered function marked: GAP-021 MISSING
- Estimated size: low
- Estimated risk: low
- Rationale: low size because `BarcodeLabelRenderer.Render(content, width, height)` already takes dimensions as parameters; only the configuration surface is missing. Low risk.

B. Dependency position
- Why now: `BarcodeLabelRenderer.Render` already accepts width/height; only settings are missing. Must precede Phase 16 (sample-material labels) so the multi-label phase does not hard-code a second size.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Infrastructure/Barcode/BarcodeLabelRenderer.cs`, `Infrastructure/Barcode/BarcodeService.cs`, `Domain/Settings/SystemSettings.cs` (or a label-settings row).
- Downstream phases that depend on it: Phase 16.
- Shared state or execution paths affected: label rendering path; no schema conflict with existing barcode flags.

C. Required reference behaviour
- GAP-021: sticker size configuration (reference showcase, page 3): label stock size documented as 38 mm x 25 mm.
- User workflow: open label settings, set width and height; labels render at configured size.

D. Current Top-Lab behaviour
- `BarcodeLabelRenderer.Render` takes dimensions only from call-site constants; `BarcodeService.cs` invokes it with defaults; no size setting, command, DTO, or view model exists (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: configurable label size (38 x 25 mm documented). Current: hard-coded defaults. Delta: persist label width/height and pass through the service into the renderer.

F. Exact implementation changes by layer
- Domain: `SystemSettings` (or dedicated label-settings row) — add label width and height columns (nullable).
- Application: settings command/handler for label size.
- Infrastructure: `BarcodeService` — pass configured dimensions to `BarcodeLabelRenderer.Render`; EF configuration for new columns.
- Presentation: settings view — width/height controls.
- Tests: label size pass-through test; regression: existing barcode tests unchanged with defaults.
- Migration: one new migration for label size columns.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add label width/height nullable columns to `SystemSettings` (or a label-settings table).
- Back-fill: default to existing render dimensions (300 x 80) so current labels unchanged.
- Compatibility: additive only.
- Evidence state not already persisted: no label size columns exist.
- One migration only for this phase.

H. Business rules and message text
- Validation: width/height positive numbers within printer capability.

I. Objective acceptance criteria
- Given label settings, when the user sets width/height, then `BarcodeLabelRenderer.Render` is invoked with those dimensions.
- Given defaults, then existing label output unchanged.

J. Test plan
- Application: label settings persistence tests.
- Infrastructure: renderer dimension pass-through test.
- Regression: existing barcode tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: size that does not match real label stock. Mitigation: document measured stock dimensions as default.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 16 — Barcode labels per sample material with doctor-editable test distribution

A. Phase identification
- Phase number: 16
- Descriptive title: Barcode labels per sample material with doctor-editable test distribution
- Gap IDs: GAP-009
- Each covered function marked: GAP-009 DIFFERENT
- Estimated size: high
- Estimated risk: medium
- Rationale: high size because it introduces a new `SampleMaterial` entity, per-test assignment, doctor-editable distribution UI, and multi-label print payload. Medium risk because the existing single-identifier path must keep working.

B. Dependency position
- Why now: depends on Phase 15 (label size parameters). Terminates in `BarcodeLabelRenderer.Render`; its output shape is defined by Phase 15's parameters. Must not be attempted before Phase 15.
- Prerequisite phases or existing conditions: Phase 15 complete. Existing `PrintBarcodeCommand`/`BarcodeService` single-identifier path works (GAP-038-equivalent identity printing).
- Exact code dependencies: `Application/Features/PatientRegistration/Commands/PrintBarcode`, `Infrastructure/Barcode/BarcodeService.cs`, `Infrastructure/Barcode/BarcodeLabelRenderer.cs`, new `SampleMaterial` entity and assignment table.
- Downstream phases that depend on it: Phase 17 (vertical coding reuses the label primitive).
- Shared state or execution paths affected: barcode printing path; existing single-identifier path must remain intact.

C. Required reference behaviour
- GAP-009: barcode labels for tubes, containers and swabs with doctor-editable test distribution (reference showcase, page 3): program distributes the patient's tests across drawn materials; doctor can change distribution; 38 mm x 25 mm stock; reprintable.
- User workflow: open a patient's sample-material board, distribute tests across tubes/containers/swabs, edit distribution, print one label per material; reprintable.

D. Current Top-Lab behaviour
- `IBarcodeService.PrintBarcodeAsync(string value)` renders one fixed-size Code-128 label for the patient or Lab Id; no sample-material entity, no per-material assignment, no distribution UI (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: per-material labels with doctor-editable distribution. Current: one label per patient. Delta: introduce sample materials, per-test assignment, distribution UI, multi-label payload; keep existing single-identifier path working.

F. Exact implementation changes by layer
- Domain: new `SampleMaterial` entity; `PatientTest` reference to material (nullable); assignment aggregate.
- Application: commands to assign tests to materials, set distribution; query to list materials per patient.
- Infrastructure: EF configuration for `SampleMaterial` and assignment table; `BarcodeService` multi-label payload; printer dispatch.
- Presentation: material board view/view model; doctor-editable distribution; print trigger.
- Tests: material assignment tests; multi-label payload test; regression: existing `PrintBarcode` tests unchanged.
- Migration: one new migration for `SampleMaterial` table and assignment table.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add `SampleMaterial` table and `PatientTestSampleMaterial` assignment table (plus optional `SampleDrawnAtUtc` column in the spirit of existing patterns).
- Back-fill: none required (new tables empty).
- Compatibility: additive only; existing `PatientTest` changes are additive nullable.
- Evidence state not already persisted: no sample-material table exists.
- One migration only for this phase.

H. Business rules and message text
- Validation: a test assigned to at most one material at a time; material must exist before assignment.

I. Objective acceptance criteria
- Given a patient with tests, when the user distributes tests across materials, then one label per material is produced with the assigned tests.
- Given the same patient, when the user reprints, then labels are reproducible.
- Regression: existing single-identifier barcode print still works.

J. Test plan
- Domain/Application: material assignment unit tests.
- Infrastructure: multi-label PDF payload test.
- Presentation: material board view model tests.
- Regression: existing barcode printing tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: breaking the existing single-identifier path. Mitigation: keep `PrintBarcodeCommand` unchanged and add a separate multi-material path.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 17 — Optional vertical identity coding

A. Phase identification
- Phase number: 17
- Descriptive title: Optional vertical identity coding on result report, receipt, and envelope
- Gap IDs: GAP-020
- Each covered function marked: GAP-020 MISSING
- Estimated size: low
- Estimated risk: medium
- Rationale: low size because it is a rendering mode of the identity mark. Medium risk because it touches three writers and rotation interacts with page geometry.

B. Dependency position
- Why now: vertical coding is a rendering mode of the identity mark that Phase 16's material labelling also emits. Implement after Phase 16 so it is not redone for the per-material label.
- Prerequisite phases or existing conditions: Phase 16 complete (label primitive and identity coding exist).
- Exact code dependencies: `Infrastructure/Printing/ReportPdfWriter.cs`, `Infrastructure/Printing/ReceiptPdfWriter.cs`, `Infrastructure/Printing/EnvelopePdfWriter.cs`, `Domain/Settings/SystemSettings.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: three writers (report, receipt, envelope).

C. Required reference behaviour
- GAP-020: optional vertical identity coding on the result report, receipt and envelope (reference showcase, page 3): vertical identity coding printed on documents, explicitly optional.
- User workflow: enable vertical coding in settings; identity mark renders vertically/rotated on the three documents.

D. Current Top-Lab behaviour
- No setting, command, or writer branch for vertical/rotated identity coding; the only barcode path renders horizontally on a single label (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: optional vertical coding on three documents. Current: none. Delta: add a persisted toggle and a vertical/rotated rendering mode applied to the identity mark on all three documents.

F. Exact implementation changes by layer
- Domain: `SystemSettings` — add vertical-coding toggle (nullable boolean, default false). Reuses Phase 16's identity-carrying label if applicable.
- Application: settings command/handler for the toggle.
- Infrastructure: three writers — add rotation/vertical-text mode for the identity mark.
- Presentation: settings view — toggle control.
- Tests: vertical rendering test per writer; regression: existing horizontal barcode/report tests unchanged with toggle off.

G. Migration decision: NO MIGRATION
- Reason: reuses the coding/label infrastructure introduced by Phase 16; vertical coding is a rendering mode applied to the existing identity mark. If the owner requires a dedicated persisted toggle column, see OD-03 (would add a migration).

H. Business rules and message text
- Validation: rotation angle supported by QuestPDF; page geometry check.

I. Objective acceptance criteria
- Given vertical coding enabled, when the identity mark is rendered on the report/receipt/envelope, then the mark is rotated vertical.
- Given vertical coding disabled (default), then the mark renders horizontally as today.

J. Test plan
- Infrastructure: vertical rendering test for each of the three writers.
- Regression: existing horizontal rendering tests unchanged with mode off.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: rotation interacting with page geometry. Mitigation: test each writer's page layout.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: NOT FINAL — blocked by OD-03 (global versus per-document vertical-coding toggle). If the owner decides no dedicated toggle column is needed, the phase is FINAL and can proceed.

### Phase 18 — Sample-nature interpretation panel

A. Phase identification
- Phase number: 18
- Descriptive title: Sample-nature interpretation panel (nature, normal ranges, effect on patient)
- Gap IDs: GAP-022
- Each covered function marked: GAP-022 MISSING
- Estimated size: medium
- Estimated risk: medium
- Rationale: medium size because a new entity, read surface, and authored interpretation text. Risk medium because the clinical content is editorial, not technical.

B. Dependency position
- Why now: independent of all other phases; depends only on catalogue and range data that already exist. Can be done at any point.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Domain/Tests/Test.cs`, `Domain/Results/ReferenceRange.cs`, new sample-nature interpretation entity.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: catalogue/range read path; no shared file with other phases.

C. Required reference behaviour
- GAP-022: interpret sample nature, its normal ranges, and its effect on the patient (reference showcase, page 51): sample-nature interpretation panel.
- User workflow: open interpretation panel for a sample, view nature, normal ranges, and clinical meaning.

D. Current Top-Lab behaviour
- `Test` carries collection/group labels and `ReferenceRange` supplies intervals, but no interpretation entity, command, query, or panel presents sample nature with ranges and meaning (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: interpretation panel. Current: absent. Delta: introduce sample-nature interpretation records with a read surface combining nature, ranges, and interpretation text.

F. Exact implementation changes by layer
- Domain: new sample-nature interpretation entity linked to test/sample.
- Application: query to read interpretation for a sample/test.
- Infrastructure: EF configuration for interpretation table.
- Presentation: interpretation panel view/view model.
- Tests: interpretation read test; regression: existing catalogue/range tests unchanged.
- Migration: one new migration for interpretation rows.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add sample-nature interpretation table (nature, normal ranges, interpretation text).
- Back-fill: none required (new table; editorial content added separately).
- Compatibility: additive only.
- Evidence state not already persisted: no interpretation table exists.
- One migration only for this phase.

H. Business rules and message text
- Validation: interpretation text linked to a valid test/sample nature.

I. Objective acceptance criteria
- Given a sample/test, when the user opens the interpretation panel, then the sample nature, normal ranges, and clinical meaning are displayed.
- Editorial content risk: interpretation text must be authored/maintained (recorded in phase risk).

J. Test plan
- Application: interpretation query unit tests.
- Regression: existing catalogue/range tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: clinical content correctness (editorial). Mitigation: content authored by domain experts; code only stores/displays.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 19 — Glossary of analysis abbreviations

A. Phase identification
- Phase number: 19
- Descriptive title: Glossary of analysis abbreviations
- Gap IDs: GAP-023
- Each covered function marked: GAP-023 MISSING
- Estimated size: low
- Estimated risk: medium
- Rationale: low size because new entity/CRUD/lookup surface, same shape as Phase 18. Risk medium because editorial, not technical.

B. Dependency position
- Why now: independent of all other phases; same shape as Phase 18. Can be done at any point.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: new glossary entity; `Domain/Tests/Test.Short`/`TestCode` are catalogue heading fields (read-only reference).
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: catalogue read path; no shared file with other phases.

C. Required reference behaviour
- GAP-023: glossary of abbreviations used in medical analysis (reference showcase, page 51): terminology lookup.
- User workflow: open glossary, look up an abbreviation and its full term/notes.

D. Current Top-Lab behaviour
- `Test.Short`/test codes are catalogue heading fields; no glossary entity, command, query, or definition view exists (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: glossary lookup. Current: absent. Delta: introduce glossary records (abbreviation, full term, notes) with CRUD and a lookup surface.

F. Exact implementation changes by layer
- Domain: new glossary entity.
- Application: CRUD commands/queries.
- Infrastructure: EF configuration for glossary table.
- Presentation: glossary view/view model; lookup bindings.
- Tests: glossary CRUD tests; regression: existing catalogue tests unchanged.
- Migration: one new migration for glossary rows.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add glossary table (abbreviation, full term, notes).
- Back-fill: none required (new table; editorial content added separately).
- Compatibility: additive only.
- Evidence state not already persisted: no glossary table exists.
- One migration only for this phase.

H. Business rules and message text
- Validation: abbreviation unique.

I. Objective acceptance criteria
- Given the glossary, when the user looks up an abbreviation, then the full term and notes are displayed.
- Editorial content risk recorded.

J. Test plan
- Application: glossary CRUD unit tests.
- Regression: existing catalogue tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: editorial content correctness. Mitigation: content authored by domain experts.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: FINAL

### Phase 20 — Appointment-note reminder inside the application

A. Phase identification
- Phase number: 20
- Descriptive title: Appointment-note reminder (in-application only)
- Gap IDs: GAP-024
- Each covered function marked: GAP-024 MISSING
- Estimated size: medium
- Estimated risk: high
- Rationale: medium size because new reminder entity and pending-reminder surface. Risk high because no notification infrastructure exists at all; it is greenfield with an open design decision (OD-04).

B. Dependency position
- Why now: independent of all other phases. Sequenced last because its size is driven by a notification-channel decision that affects nothing else, so it should not block other phases.
- Prerequisite phases or existing conditions: none. Phone book exists (`JsonPhoneBookStore`, `GetPhoneBookQuery`) and is unaffected.
- Exact code dependencies: `Application/Features/Utilities/Queries/GetPhoneBookQuery`, new reminder entity.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: phone book read path (read-only reference); no shared file with other phases.

C. Required reference behaviour
- GAP-024: appointment-note reminder to a user or phone-book contact (reference showcase, page 51): send a reminder note through the appointment notebook; delivery is in-application/internal, no external channel.
- User workflow: create a reminder note for a user or phone-book contact with a due date; view pending reminders inside the application; no SMS/e-mail/fax/portal.

D. Current Top-Lab behaviour
- Phone-book storage and queries exist, but no reminder, appointment, note-scheduling, or notification entity, command, query, DTO, view, or view model exists anywhere in `src/` (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: in-application reminder. Current: absent. Delta: introduce internal reminders (target, message, due date, state) with management and an in-application pending-reminder surface; no external channel.

F. Exact implementation changes by layer
- Domain: new reminder entity.
- Application: reminder CRUD commands/queries; pending-reminder query.
- Infrastructure: persistence for reminder table (JSON store or EF table).
- Presentation: reminder management view/view model; pending-reminder surface.
- Tests: reminder CRUD tests; regression: phone-book tests unchanged.
- Migration: one new migration for reminder rows if EF persistence chosen.

G. Migration decision: NEW MIGRATION
- Intended name: additive migration appended after the latest existing migration; add reminder table (target, message, due date, sent state).
- Back-fill: none required.
- Compatibility: additive only.
- Evidence state not already persisted: no reminder table exists.
- One migration only for this phase.

H. Business rules and message text
- Validation: due date in future; target is a valid user or phone-book contact.
- Constraint: no SMS/e-mail/fax/portal delivery channel (owner exclusion).

I. Objective acceptance criteria
- Given a user/contact, when the user creates a reminder with a due date, then the reminder is stored and appears in the pending-reminder surface.
- Given the due date passed, then the reminder is shown as due.
- No external delivery channel is introduced.

J. Test plan
- Application: reminder CRUD and pending-reminder query tests.
- Presentation: reminder view model tests.
- Regression: phone-book tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. Verify the new migration against a disposable empty database.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: scheduling specifics unresolved. Mitigation: OD-04 records options.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: NOT FINAL — blocked by OD-04 (reminder scheduling specifics: due-date semantics, recurrence, target resolution). If the owner resolves OD-04, the phase is FINAL.

### Phase 21 — Banded monitor over blood-picture element values

A. Phase identification
- Phase number: 21
- Descriptive title: Banded monitor over blood-picture element values in time periods
- Gap IDs: GAP-025
- Each covered function marked: GAP-025 MISSING
- Estimated size: low
- Estimated risk: low
- Rationale: low size because `ProfileResultItem` and `HistoryMatrixBuilder.Pivot` already exist; extend the monitor from test-level scalars to profile-analyte rows. Low risk.

B. Dependency position
- Why now: fully independent of the printing pipeline and every other phase; reuses the existing pivot helper. Can be done at any point.
- Prerequisite phases or existing conditions: none.
- Exact code dependencies: `Application/Features/Statistics/Queries/GetBandedResultMonitorQueryHandler`, `Domain/Results/ProfileResultItem.cs`, `Application/Features/ReportProduction/Common/HistoryMatrixBuilder.cs`.
- Downstream phases that depend on it: none.
- Shared state or execution paths affected: banded monitor query path; reads `ProfileResultItem` which the existing monitor never reads.

C. Required reference behaviour
- GAP-025: search and statistics on blood-picture element values in time periods (reference showcase, page 73): element-value band search over CBC analytes (HGB/HCT/MCV/MCH/MCHC/RETICS/PLT/PCT/MPV/LYM/MON/NEU/EOS/BAS/BAND) with from/to bands and a statistics action.
- User workflow: open element-value search, choose an analyte and from/to band and a period, view matching results; run statistics action.

D. Current Top-Lab behaviour
- The only banded monitor filters `PatientTest.ResultValue` scalars (`GetBandedResultMonitorQueryHandler.cs:57-105`); CBC results live as analyte-level rows in `ProfileResultItem`, which that query never reads (static inspection).
- Verification status: STATICALLY VERIFIED.

E. Exact behaviour delta
- Reference: element-value band search over CBC analytes. Current: test-level scalar monitor only. Delta: extend banded search from `PatientTest.ResultValue` to `ProfileResultItem` analyte rows with per-element band inputs over a period, reusing `HistoryMatrixBuilder.Pivot`.

F. Exact implementation changes by layer
- Application: `GetBandedResultMonitorQuery` — add element/analyte input and per-element band parameters; handler reads `ProfileResultItem`.
- Infrastructure: none (reads existing table).
- Presentation: element-value search view — band inputs and period picker.
- Tests: monitor-over-analyte unit test; regression: existing scalar monitor tests unchanged.
- No schema change.

G. Migration decision: NO MIGRATION
- Reason: reads `ProfileResultItem`, which already exists (migration `20260909033414_AddAnalyteProfileDomain`). No new column required.

H. Business rules and message text
- Validation: band min/max valid; element exists in profile.

I. Objective acceptance criteria
- Given a period and an analyte with a from/to band, when the user runs the search, then matching profile-analyte results are listed.
- Given the same inputs, when the user runs the statistics action, then element statistics are computed.
- Existing scalar monitor behaviour unchanged.

J. Test plan
- Application: banded monitor over analyte rows unit test.
- Regression: existing scalar banded monitor tests unchanged.

K. Phase-end verification gate
1. Build the complete solution.
2. Run the complete relevant test suite.
3. Run the full regression suite.
4. No migration to verify.
5. Confirm existing migrations remain unchanged.
6. Verify reference behaviour matches acceptance criteria.
7. Inspect the complete set of changed files and confirm only intended files changed.
8. Record commands, exit codes, failures, outputs.
9. Verify no excluded functionality or external delivery channel introduced.

L. Risks and rollback
- Risk: none material.
- Rollback: revert the changes made by the phase; restore affected files to prior state.

M. Phase status: NOT FINAL — blocked by OD-06 (hosting decision: where the element-value band search is surfaced relative to the existing scalar monitor). If the owner resolves OD-06, the phase is FINAL.

---

## SECTION 5 — Migration Summary

Consolidated migration table. Totals recomputed from the phase-level migration decisions below; they are not copied from any earlier document.

| Phase | Migration decision | Reason | Exact additive schema changes | Affected tables and columns | Types, nullability, defaults, constraints | Back-fill / data-transition | Evidence data not already persisted / no schema change required |
|---|---|---|---|---|---|---|---|
| 1 | NEW MIGRATION | `HeaderImagePath`, `FooterImagePath`, `NormalRangeMarker` columns do not exist on `ReportSettings` | Add three nullable string columns to `ReportSettings` | `ReportSettings.HeaderImagePath`, `ReportSettings.FooterImagePath`, `ReportSettings.NormalRangeMarker` | string, nullable, default null, no key/index | none (nullable) | Columns absent from all migrations and model snapshot |
| 2 | NEW MIGRATION | Seven receipt font/logo columns do not exist on `ReceiptSettings` | Add seven nullable string/int columns to `ReceiptSettings` | `ReceiptSettings.HeaderFontFamily`, `HeaderFontSizePt`, `HeaderFontColor`, `FooterFontFamily`, `FooterFontSizePt`, `FooterFontColor`, `LogoImagePath` | string/int, nullable, default null | none | Columns absent from all migrations and model snapshot |
| 3 | NO MIGRATION | Reuses Phase 1 image columns; rendering-only over existing columns | — | — | — | — | `HeaderImagePath`/`FooterImagePath` added by Phase 1 |
| 4 | NO MIGRATION | Reuses Phase 2 logo column; rendering-only | — | — | — | — | `LogoImagePath` added by Phase 2 |
| 5 | NO MIGRATION | Query predicate only; `PaymentOperation.OperationType` already distinguishes operation types | — | — | — | — | Data already persisted |
| 6 | NO MIGRATION | Reads `Patient`/`PatientTest`/`Test`; no new column | — | — | — | — | Data already persisted |
| 7 | NEW MIGRATION | Item-template table does not exist | Add item-template table | New table: item key, label, reference-value expression, show/hide, list membership, free-text override | string/boolean, nullable per design, PK on item key | Seed default templates matching current fixed section sequence | Table absent from all migrations and model snapshot |
| 8 | NEW MIGRATION | Per-item layout columns do not exist on item-template table | Add line-spacing, left-offset, marker-symbol nullable columns to item-template table | Item-template table: line spacing, left offset, marker symbol | string/decimal, nullable, default null | none | Columns absent (table created by Phase 7) |
| 9 | NEW MIGRATION | Abnormal-result colour columns do not exist on `ReportSettings` | Add enable flag and colour columns to `ReportSettings` | `ReportSettings` abnormal-result enable, low colour, background colour | boolean/string, nullable, default off/null | Default flag off | Columns absent from all migrations and model snapshot |
| 10 | NEW MIGRATION | Per-profile styling columns do not exist on `Profile`/`ProfileAnalyte` | Add normal/abnormal font and background columns to `Profile` | `Profile` normal-font, normal-colour, abnormal-font, abnormal-background | string, nullable, default null | Null default (global colours apply) | Columns absent from all migrations and model snapshot |
| 11 | NO MIGRATION | Commands/handlers exist; UI wiring only | — | — | — | — | No schema change |
| 12 | NEW MIGRATION | Sensitivity visibility flags and antibiotic-of-choice field do not exist | Add flags and choice field to culture result/report settings table | Culture settings: three visibility booleans; choice field | boolean/string, nullable, default show-all/null | Default flags show all three columns | Columns absent from all migrations and model snapshot |
| 13 | NO MIGRATION | Query predicate only over `PatientTest.EnteredAtUtc`/`ResultValue` | — | — | — | — | Data already persisted |
| 14 | NO MIGRATION | Snapshot model and command exist; UI wiring only | — | — | — | — | No schema change |
| 15 | NEW MIGRATION | Label width/height columns do not exist | Add width/height nullable columns to `SystemSettings` | `SystemSettings.LabelWidth`, `SystemSettings.LabelHeight` | int/decimal, nullable, default existing render dimensions | Default to current render dimensions | Columns absent from all migrations and model snapshot |
| 16 | NEW MIGRATION | `SampleMaterial` and assignment tables do not exist | Add `SampleMaterial` table and `PatientTestSampleMaterial` assignment table | `SampleMaterial` (name, etc.); `PatientTestSampleMaterial` (patient test id, material id) | string/int, PK/FK, nullable assignment | none | Tables absent from all migrations and model snapshot |
| 17 | NO MIGRATION | Reuses Phase 16 coding field; rendering mode (see OD-03 if a toggle column is wanted) | — | — | — | — | No new schema (OD-03 open) |
| 18 | NEW MIGRATION | Sample-nature interpretation table does not exist | Add interpretation table | New table: sample nature, normal ranges, interpretation text | string, nullable per design, PK | none (editorial content separate) | Table absent from all migrations and model snapshot |
| 19 | NEW MIGRATION | Glossary table does not exist | Add glossary table | New table: abbreviation, full term, notes | string, nullable per design, PK | none | Table absent from all migrations and model snapshot |
| 20 | NEW MIGRATION | Reminder table does not exist | Add reminder table | New table: target, message, due date, sent state | string/datetime/boolean, PK | none | Table absent from all migrations and model snapshot |
| 21 | NO MIGRATION | Reads `ProfileResultItem` (already exists) | — | — | — | — | `ProfileResultItem` persisted by `20260909033414_AddAnalyteProfileDomain` |

Reconciled migration total:
- NEW MIGRATION phases: 1, 2, 7, 8, 9, 10, 12, 15, 16, 18, 19, 20 = 12 migrations.
- NO MIGRATION phases: 3, 4, 5, 6, 11, 13, 14, 17, 21 = 9 phases.
- Total phases: 21. Total new migrations: 12.
The number of migrations (12) agrees with the phase-level migration decisions. Every new migration is additive and appended after the latest existing migration; no existing migration is edited. No owner decision or unresolved possibility is counted as a confirmed migration. OD-03 (Phase 17 toggle column) and OD-06 (Phase 21 hosting) are open decisions and are not counted as confirmed migrations.

## SECTION 6 — Open Owner Decisions

Each entry records an unresolved decision that the owner must make. None of these decisions was resolved by assuming a preferred design.

### OD-01 — Single-result print recording rule (print count)

- Question: Should a successful single-result print set `IsPrinted` and increment `PrintCount` on the `PatientTest` row (and thereby permit delivery, which requires `IsPrinted`), matching the bulk, combined, history, and entry-screen print paths; or should single-result print continue to never mark the row (current behaviour)?
- Reference evidence and limitations: the reference describes printing and delivery but does not settle whether every print route increments the same persisted print count. The current code marks `PrintCount` in bulk/combined/history/entry paths but not in single-result `MarkResultPrinted` (static inspection of `MarkResultPrintedCommandHandler.cs` and `ResultPrintCoordinator.cs`). No live database was available to observe the persisted count.
- Viable options: (a) record the print on success in the single-result path, making all print routes consistent and enabling delivery after a single print; (b) keep the current never-mark behaviour for single-result print.
- Consequences: option (a) makes print-count state consistent across routes and unblocks delivery on the single-result path; option (b) leaves a single-printed result unable to satisfy delivery preconditions and leaves the audit count under-reported for that route.
- Operational impact: option (a) changes behaviour for an existing print route; existing users relying on "print without marking" would see a change. Option (b) preserves the status quo but retains an inconsistency.
- Recommendation: option (a), record on success, for consistency with every other print route and with the delivery precondition.
- Blocks a phase: no phase is strictly blocked, but it affects the correctness framing of Phase 11 (verify/unverify) and Phase 14 (print-time override) and should be resolved before those phases are verified as conformant.

### OD-02 — Test removal/clear guard after results exist or after delivery

- Question: Should removal or clearing of tests from a visit be blocked once any result has been entered, reviewed, printed, or delivered? Currently the entered-result predicate exists on every removal path, but there is no explicit `IsReviewed`/`IsPrinted`/`PrintCount > 0`/`IsDelivered` predicate; after-delivery blocking is only transitive through the domain locks (`ClearResult`/`Unreview`/`MarkPrinted`/`MarkDelivered`), which was not covered by tests.
- Reference evidence and limitations: the reference describes removing tests at first registration but does not state whether removal is permitted after results exist or after delivery. Static inspection did not settle the guard either way; no database was available to observe the outcome.
- Viable options: (a) add an explicit `IsReviewed || IsPrinted || PrintCount > 0 || IsDelivered` guard to all three removal paths (single-remove via deltas, clear-all, apply-visit-deltas); (b) keep the current entered-result-only predicate with transitive after-delivery blocking.
- Consequences: option (a) prevents removal after printing/delivery so printed reports remain reconcilable with the visit; option (b) preserves current behaviour but leaves the delivery case unproven by tests and the culture-draft case (saved culture result with unverified parent) potentially removable.
- Operational impact: option (a) hardens data integrity for printed/delivered visits; may affect workflows that currently remove tests after entry.
- Recommendation: option (a), with tests for removal-after-printed and removal-after-delivered.
- Blocks a phase: none; recorded as an observation/hardening task, not a gap.

### OD-03 — Vertical identity coding toggle scope

- Question: Should optional vertical coding be governed by a single global toggle, or by a per-document toggle (separate flags on the result report, receipt, and envelope)?
- Reference evidence and limitations: reference showcase, page 3 shows vertical coding on the three documents and describes it as optional, but does not state whether the option is global or per-document.
- Viable options: (a) one global boolean toggle governing all three documents; (b) three per-document boolean flags.
- Consequences: option (a) is simpler and matches the singular "(optional)" wording; option (b) offers finer control but requires three columns and more UI.
- Operational impact: option (b) gives operators more control but increases settings surface; option (a) is simpler to implement and explain.
- Recommendation: option (a), a single global toggle, unless the owner demonstrates a need for per-document control.
- Blocks a phase: yes — Phase 17 is marked NOT FINAL pending this decision. If option (a) is chosen, no new migration is needed beyond a toggle column (or reuse of Phase 16 infrastructure); if option (b), a migration with three columns is required.

### OD-04 — Appointment-note reminder scheduling specifics

- Question: What are the reminder scheduling specifics: due-date semantics (one-time vs recurring), target resolution (user vs phone-book contact), and how pending reminders are surfaced and cleared?
- Reference evidence and limitations: reference showcase, page 51 describes an appointment-note reminder to a user or phone-book contact through an in-application notebook, with no SMS/e-mail/fax/portal channel. It does not state due-date recurrence, target resolution rules, or the pending-reminder surface behaviour.
- Viable options: (a) one-time reminder with a due date, targeted at a user or phone-book contact, shown in a pending-reminder list until marked done; (b) recurring reminder with interval; (c) reminder integrated with the calendar/appointment view rather than a standalone list.
- Consequences: option (a) is simplest and matches the "note" wording; option (b) adds scheduling complexity; option (c) requires a calendar surface that does not exist.
- Operational impact: option (a) adds a small greenfield capability; option (c) is a larger UI investment.
- Recommendation: option (a).
- Blocks a phase: yes — Phase 20 is marked NOT FINAL pending this decision.

### OD-05 — Permission granularity

- Question: Should the 13 seeded coarse permissions be split into finer per-activity permission codes matching the reference's longer per-activity list?
- Reference evidence and limitations: reference guide, page 155 enumerates a longer per-activity permission list (patient add/edit, result enter/review/print, print-blocked-on-balance, deliver, discount limit, worksheet print, patient delete, system settings, cash, statistics, audit, plus catalogue/reference-range/unit/group/barcode activities). Top-Lab seeds 13 coarse permissions enforced uniformly by `AuthorizationBehavior.cs`. The reference does not state that fine-grained codes are mandatory; it describes activities.
- Viable options: (a) keep the 13 coarse permissions (current); (b) split into per-activity codes with per-handler gates.
- Consequences: option (b) is a large change across all handlers and the authorization behaviour; option (a) retains the coarse model noted as a coarser model, not a missing capability.
- Operational impact: option (b) touches nearly every command/handler and its permission check; option (a) requires no change.
- Recommendation: option (a); classify the coarse model as acceptable unless the owner requires per-activity codes.
- Blocks a phase: none.

### OD-06 — Banded monitor hosting for blood-picture element values

- Question: Where should the element-value band search over blood-picture analytes be surfaced relative to the existing single-test banded monitor: as an extension of the same monitor screen, or as a separate screen?
- Reference evidence and limitations: reference showcase, page 73 shows element-value search/statistics over CBC analytes with from/to bands and a statistics action. The existing monitor reads `PatientTest.ResultValue` scalars; CBC values live in `ProfileResultItem` analyte rows. The reference does not state the hosting/placement.
- Viable options: (a) extend the existing banded-monitor screen to support element/analyte input; (b) add a separate element-value search screen.
- Consequences: option (a) reuses the existing screen and pivot helper; option (b) adds a new screen but keeps the scalar monitor unchanged.
- Operational impact: option (a) is smaller; option (b) avoids changing the existing monitor behaviour.
- Recommendation: option (a), extending the existing monitor, reusing `HistoryMatrixBuilder.Pivot`.
- Blocks a phase: yes — Phase 21 is marked NOT FINAL pending this decision.

---

## SECTION 7 — Risk Register and Cross-Phase Dependencies

| Risk | Likelihood | Impact | Mitigation | Phase or decision affected |
|---|---|---|---|---|
| Incorrect classification: a function labelled IMPLEMENTED is actually not fully implemented at runtime | Medium | Medium | Tier 1 reachability was performed for all IMPLEMENTED functions; Tier 2 deep traces covered all flagged items; classifications rest on static traces plus passing tests, not runtime observation | All IMPLEMENTED functions; reported honestly as not runtime-verified |
| Unverified runtime behaviour: WPF layer never launched; no database observed | Medium | High | No phase claims runtime verification; every phase-end gate requires a build and test run on the implementation host; runtime confirmation left to the implementing agent on Windows | All phases; noted as a standing limitation |
| Database/migration uncertainty: migrations source-inspected only, not applied to a database | Medium | High | Every new migration must be verified against a disposable empty database by the implementing agent; existing migrations never edited | Phases with NEW MIGRATION (1, 2, 7, 8, 9, 10, 12, 15, 16, 18, 19, 20) |
| WPF and printing differences: fonts, Arabic shaping, RTL layout not visually verified on paper | Medium | Medium | All printing tests pass on Windows; font-dependent failures seen elsewhere do not occur here; no physical printer or Arabic shaping test executed | Phases touching report/receipt/envelope writers (1, 2, 3, 4, 7, 9, 10, 11, 16, 17) |
| Font and rendering dependencies: `ArabicFontResolver` is Windows-only and depends on installed fonts | Medium | Medium | Fonts resolved at render time; document preference for widely available fonts; rendering tests use bundled images/fonts | Phases 1, 2, 7, 9, 10, 16, 17 |
| Reference ambiguity: some reference pages read through text extraction only; visual degradation on some pages | Medium | Medium | 55 pages visually inspected at 150 DPI; ambiguous pages cross-checked against render; ambiguous details recorded as owner decisions rather than invented | All phases citing ambiguous pages; OD-03, OD-04, OD-06 |
| Data migration/back-fill risk: new columns on settings tables need sensible defaults | Low | Low | All new columns nullable with documented defaults; seed default templates for Phase 7; back-fill specified per phase | Phases 1, 2, 7, 8, 9, 10, 12, 15, 18, 19, 20 |
| Dependencies between phases: report-visual phases share `ReportContentBuilder`/`ReportSettings` | Medium | Medium | Phase 1 (branding foundation) precedes all report-visual phases; Phase 7 (item template) precedes Phase 8/9/10; order enforced in Section 4 | Phases 1-10 |
| Unresolved inventory completeness: decomposition is a reconstruction; a different auditor could decompose composite screens differently | Low | Medium | Inventory derived independently from both PDFs; duplicates merged; excluded functions separated; totals reconciled from rows | Gap register; Section 8 item 1 |

Confirmed facts are distinguished from possible risks: all risks above are possible, not confirmed, except the build/test results which are confirmed.

Cross-phase dependency summary: Phase 1 (report settings foundation) is the prerequisite for Phases 2, 3, 7, 8, 9, 10, 11. Phase 15 (label size) precedes Phase 16 (sample materials). Phase 16 precedes Phase 17 (vertical coding). Phase 7 (item template) precedes Phases 8, 9, 10. No circular dependencies exist.

---

## SECTION 8 — Self-Audit Checklist

Each item carries an explicit PASS / FAIL / PARTIALLY VERIFIED result supported by evidence.

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Completeness of reference inventory | PASS | 152 distinct in-scope functions reconstructed from both PDFs; 21 excluded functions separated; totals recomputed from rows (152 in-scope = 127 IMPLEMENTED + 11 DIFFERENT + 14 MISSING); duplicates merged; sub-steps not double-counted |
| 2 | Verification of all identified MISSING functions | PASS | All 14 MISSING functions have a gap-register entry (GAP-013, GAP-014, GAP-015, GAP-016, GAP-017, GAP-018, GAP-019, GAP-020, GAP-021, GAP-022, GAP-023, GAP-024, GAP-025) and at least one implementation phase; each supported by systematic negative search and page evidence |
| 3 | Verification of all identified DIFFERENT functions | PASS | All 11 DIFFERENT functions have a gap-register entry (GAP-001…GAP-012) and at least one implementation phase; each with reference behaviour, current behaviour, delta, and file/line evidence |
| 4 | Excluded-feature isolation | PASS | 21 excluded functions listed separately; zero excluded functions in the gap register, phases, migration summary, or recommendations; the five exclusions applied consistently |
| 5 | Source restrictions | PASS | Only the two authorized PDFs used for reference requirements; `Docs/Reference system files/` never opened; no external websites or domain knowledge used for requirements |
| 6 | Source-path and line-number accuracy | PARTIALLY VERIFIED | File paths and line ranges cited from files actually read; line numbers re-checked after inspection; a small number of Tier 1 rows relied on grep results rather than full file reads |
| 7 | PDF visual inspection coverage | PARTIALLY VERIFIED | 55 pages visually inspected at 150 DPI (guide pages 3-5, 151-153, 155, 193-212; showcase pages 3, 15, 51, 56-66); remaining pages covered through text extraction; no visual inspection claimed for text-only pages |
| 8 | Build coverage | PASS | All 9 projects built individually with zero warnings and zero errors; exact commands and exit codes recorded |
| 9 | Test coverage and count reconciliation | PASS | 2787 passed, 0 failed, 2 skipped (2789 total) across all 5 test projects; per-project counts recorded; both skips are by design; discrepancy with earlier totals explained |
| 10 | Migration inspection | PARTIALLY VERIFIED | 15 ordered migrations and model snapshot inspected as source; no live database available to apply migrations; conclusions marked source-level only |
| 11 | Phase dependency ordering | PASS | Phases ordered by real code dependency; Phase 1 first; one migration per phase; no circular dependencies; prerequisite relationships documented per phase |
| 12 | One-migration-per-phase compliance | PASS | Each phase states exactly one migration decision; 12 phases NEW MIGRATION, 9 phases NO MIGRATION; total 12 reconciles with phase decisions |
| 13 | Acceptance criteria | PASS | Every phase defines Given/When/Then acceptance criteria traceable to the reference requirement |
| 14 | Open owner decisions | PASS | Six owner decisions documented (OD-01…OD-06) with question, reference evidence, options, consequences, operational impact, recommendation, and blocking status |
| 15 | IMPLEMENTED-function coverage (Tier 1 and Tier 2) | PARTIALLY VERIFIED | Tier 1 reachability performed for all IMPLEMENTED functions (141 points); Tier 2 deep traces covered all 9 flagged items plus 9 cross-cutting concerns; coverage rests on static traces plus passing tests, not runtime observation |
| 16 | Cleanliness of the plan | PASS | Programmatic check found no commit hash, no commit/baseline/snapshot/HEAD/branch/push in the version-control sense, no prior-document references, no REF-xxx/D-xx/K-xx/F-xx/L-xx/U-xx identifiers, and no PDF file name; permitted exceptions (multi-branch phrase, existing source names) retained |
| 17 | Honesty about unverified claims | PASS | Unverified items (runtime behaviour, live migration application, printed-output visual correctness, Arabic shaping) are stated as limitations, not claimed as verified |

A PASS is supported by evidence. FAIL is used where a requirement is not satisfied. PARTIALLY VERIFIED is used where meaningful evidence exists but a material limitation remains. PASS is not used as a synonym for "attempted."

<!--PART-8-->
