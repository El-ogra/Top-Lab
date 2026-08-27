# Top-Lab — Reporting & Printing Blueprint

## نظام توب لاب — مخطط التقارير والطباعة

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Reporting & Printing Blueprint |
| Status | **Final** — approved reporting & printing baseline |
| Document purpose | This document defines every printed, previewed, or barcode-rendered artifact in Top-Lab: the layout of each document, the settings that drive its appearance, the barcode symbology and placement, the preview/print execution flow, and the Application-layer service contracts behind which all rendering is abstracted. It is the single source of truth for the Reporting & Printing concern and must be implemented exactly as specified. |

---

## 1. Purpose and Scope

This document specifies **what is printed, how each document looks, and how printing is executed** at a level precise enough to build the `IReportPrintingService`, `IReceiptPrintingService`, `IEnvelopePrintingService`, and `IBarcodeService` implementations and their consuming screens without further interpretation.

It answers:
- Which documents Top-Lab produces (result report, combined report, blank report, history report, receipt, envelope, barcode labels, work sheet, price list).
- The exact layout of each document (page, margins, header, body, footer).
- Which configuration settings (Report / Receipt / Envelope / System / Printer Assignment) drive each artifact.
- The barcode symbology, data content, and placement.
- The preview → print execution flow and how it binds to the Presentation layer.

This document does **not** specify:
- Screen layout for non-printing screens — see the UI/UX & Screen Blueprint.
- Result data "Export" (`ExportResultCommand`, FR-M04) — that is a local machine-readable file export handled by a separate export facility, not a printed or previewed document.
- Patient card (كارنيه) printing — **excluded** from the product (see §11).

Every artifact defined here is produced **offline, on the LAN**, against the single shared SQL Server database, with no Internet dependency, per the Architecture & Folder Structure Blueprint (ADR-0010, ADR-0021).

---

## 2. Conformance & Source Documents

This blueprint is derived from and must remain consistent with the following approved documents in the `Source` folder:

| Source document | What this blueprint inherits from it |
|---|---|
| Top_Lab_PRD.md | Functional requirements for reporting, receipts, envelopes, barcodes, printer assignment, and the excluded-feature set (M07, M22, §17). |
| Top_Lab_Architecture_Blueprint.md | Clean Architecture layering; `IReportPrintingService` / `IBarcodeService` as Application ports; offline-only constraint; single shared database; Infrastructure-implements-Application rule. |
| Top_Lab_Data_Model_Blueprint.md | `ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`, `EnvelopePrintItemPosition`, `PrinterAssignment` schemas; `decimal(18,2)` money; `nvarchar` text; `datetime2` UTC. |
| Top_Lab_UI_UX_Blueprint.md | Screen bindings (S-05, S-07, S-08, S-09, S-12, S-18, S-25, S-28–S-31, S-33); §4.8 print/preview action contract; RTL requirement. |
| Top_Lab_Module_Dependency_Map.md | M07 (Wave 7) and M22 (Wave 1) own the printing surfaces; printing services are delivered by Foundation F4/F6. |
| Top_Lab_Coding_Standards.md | Infrastructure-implements-interface rule; no Internet dependency; services return `Result`/`Result<T>`; testability through interfaces. |
| Top_Lab_Test_Strategy.md | §3.3 printing/barcode service test contract; migration cleanliness. |
| Top_Lab_ADR.md | ADR-0010 (offline), ADR-0020 (single-row config), ADR-0021 (connection settings local). |

Conflict resolution: where this document and any source document disagree, the source document is authoritative and this blueprint must be corrected to match.

---

## 3. Architecture & Layering

### 3.1 Application-layer ports (interfaces)

Printing and barcode generation are defined as Application-layer ports so that the Presentation layer and Domain logic never depend on a concrete rendering library. The following interfaces live in `TopLab.Application` (per the Application port naming convention in the Coding Standards, §4.1):

| Interface | Responsibility |
|---|---|
| `IReportPrintingService` | Render and print/preview the patient result report (single, combined, blank, history variants). |
| `IReceiptPrintingService` | Render and print the cashier receipt. |
| `IEnvelopePrintingService` | Render and print the sample-envelope label using `EnvelopeSettings` positions. |
| `IBarcodeService` | Generate a barcode image (`BitmapSource` / `UIElement`) for a given data string and symbology. |

Each interface method returns `Result` or `Result<T>`; rendering failures surface as `Error` of type `Unexpected` (per Coding Standards §6.1), never as a raw exception to the UI.

### 3.2 Infrastructure-layer implementations

Concrete implementations live in `TopLab.Infrastructure` under `Printing/` and `Barcode/`, registered in `DependencyInjection.cs` (Scoped lifetime). They:
- Depend only on Application interfaces and Domain/Data-Model types; never on WPF directly for business logic.
- Translate library-specific exceptions into `Error` of type `Unexpected` at the Infrastructure boundary (Coding Standards §5.3, §6.1).
- Read configuration rows (`ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`, `PrinterAssignment`) via read-only queries, never by reaching into the `DbContext` from Presentation.

### 3.3 Rendering technology (finalized choice)

Per the UI/UX Blueprint §4.8, the concrete rendering surface is finalized here:

| Concern | Finalized technology | Rationale |
|---|---|---|
| Document model | **WPF `FixedDocument` / `FixedPage`** | Native to WPF; no third-party dependency for preview or print; fully offline. |
| On-screen preview | `DocumentViewer` bound to the `FixedDocument` | Satisfies the "preview region placeholder" contract in UI/UX §4.8. |
| Physical print | `PrintDialog` + `XpsDocumentWriter` (or the system print path for the assigned printer) | Standard WPF print pipeline; respects the `PrinterAssignment` selection. |
| File export (optional) | A **local** PDF library (e.g., PDFsharp) referenced at build time, no runtime Internet | Permitted only if a PDF file must be produced for LAN file sharing; not required for core printing. |
| Barcode | A **local** 1-D barcode rendering (Code 128) behind `IBarcodeService`; a local NuGet library such as ZXing.Net is permissible as an implementation detail | Offline; symbology chosen for alphanumeric patient/lab identifiers. |

No package used here may introduce an Internet or external-API dependency (Architecture ADR-0010; Coding Standards §2).

### 3.4 Document composition model

Printing services consume a presentation-neutral **report model** built by Application-layer queries (not by passing entities directly). The report model carries:
- Patient identification (per §5.3),
- The selected set of result rows (test, result value, unit, reference range, flag),
- Header/footer content resolved from `ReportSettings`,
- Signature flag.

This keeps the rendering layer decoupled from the domain and testable with fakes (Test Strategy §3.2).

---

## 4. Printing Artifacts Catalog

| ID | Artifact | Triggering screen(s) | Owner module | Output printer (per `PrinterAssignment`) |
|---|---|---|---|---|
| P-01 | Patient Result Report (single visit) | S-07 (from S-05, S-06, S-10/S-11) | M07 | Reports |
| P-02 | Combined Report | S-08 | M07 | Reports |
| P-03 | Blank Report | S-09 | M07 | Reports |
| P-04 | History Report (single / multi-patient) | S-05/S-06 patient-history action, S-07 | M07 | Reports |
| P-05 | Receipt | S-03 billing panel, S-25 cashier | M03 / M20 | Receipt |
| P-06 | Envelope Label | Sample collection / ordering flow | M21 / M02 | Envelope |
| P-07 | Barcode Label (tube / lab) | S-03 ordering, sample draw | M02 / M21 | Barcode |
| P-08 | Work Sheet | S-33 | M11 | Reports (or default) |
| P-09 | Price List | S-18 | M13 | Reports (or default) |

Print eligibility and lifecycle rules for P-01…P-04 are in §9.

---

## 5. Report Layout Specification (Patient Result Report — P-01)

This is the canonical result document; P-02/P-03/P-04 are variants built on the same layout engine with differing content sets (see §5.7).

### 5.1 Page & margins

| Aspect | Source | Value |
|---|---|---|
| Paper size | `ReportSettings.PaperSize` | A4 or A5 |
| Left margin | `ReportSettings.PageMarginLeftCm` | cm |
| Bottom margin | `ReportSettings.PageMarginBottomCm` | cm |
| Top space (above header) | `ReportSettings.ReportTopSpaceCm` | **Maximum 8 cm — enforced in UI (S-29) and validated in code** |
| RTL flow | UI/UX §7 | Right-to-left; numeric/date fields remain LTR internally but positioned within RTL |

### 5.2 Header block

Controlled by `ReportSettings.HeaderFooterMode` (None / Words / Images):
- **Words:** laboratory name (Arabic), address line, phone line, rendered as text.
- **Images:** laboratory logo / header image (JPG, up to 720×140 per the Data Model envelope convention analog) and an organization logo (up to 720×50) sourced from configured image files on the workstation.
- `HeaderColor` applies to the header band when present.
- No branch name, no online/portal reference (single-branch, offline — §11).

### 5.3 Patient identification block

Fields shown, per `SystemSettings`:
- Patient name (always).
- **Identifier:** `PatientId` (barcode) **or** `LabId` according to `SystemSettings.PrintLabIdInsteadOfPatientId`.
- Age + sex.
- Request date.
- Treating doctor / referral entity (when present).
- **Account/balance line:** shown **only** when `SystemSettings.PrintAccountInsteadOfDateOnReport` is off; when that setting is on, the account/balance summary replaces the print date line (per UI/UX S-07 note).
- Print timestamp (date/time of printing) — shown normally, **unless** `PrintAccountInsteadOfDateOnReport` substitutes the account line; `PrintDateTimeOnTubeBarcode` governs the tube barcode only (§7).

### 5.4 Results table

| Column | Content | Notes |
|---|---|---|
| Test abbreviation / name | From `Test` | Arabic label authoritative |
| Result value | `PatientTest.ResultValue` (simple) or aggregated analytes (profile) | |
| Unit | `ProfileResultItem.Unit` / test unit | |
| Reference range | From `ReferenceRange` matched by sex + age unit | Low/High comments (`LowComment`/`HighComment`) appended at boundary when set |
| Flag | Normal / Low / High (`ResultFlag`) | Color or symbol per house style |

Culture results (P-01 variant) append organism(s) and the antibiotic-sensitivity table (`CultureAntibioticResult.SensitivityCategory`).

### 5.5 Footer & signature

- `ReportSettings.DoctorSignatureEnabled` → a signature area is rendered.
- `FooterColor` applies to the footer band.
- Footer may carry clinic name / page number per `HeaderFooterMode`.

### 5.6 RTL / Arabic rendering

All visible text is Arabic (UI/UX §7). Flow direction is RTL; the results table reads right-to-left. Barcode and numeric fields are LTR glyphs positioned within the RTL flow. No English is surfaced in the product.

### 5.7 Variants

| Variant | Difference from P-01 |
|---|---|
| P-02 Combined | Multiple selected tests aggregated into one document via S-08 builder; same header/footer/patient block. |
| P-03 Blank | Patient-data-only sheet, no results; used when results are entered later or on pre-printed forms. |
| P-04 History | Prior-visit test list per `GetPatientTestHistoryQuery` / `GetMultiPatientHistoryQuery`; sort order per `ReportSettings.HistorySortMode` (by lab code / by patient name); auto-display per `HistoryAutoDisplayEnabled`. |

---

## 6. Settings Consumption Matrix

| Setting (source) | Affects artifact(s) |
|---|---|
| `ReportSettings.PaperSize` | P-01, P-02, P-03, P-04, P-08 |
| `ReportSettings.PageMarginLeftCm` / `PageMarginBottomCm` / `ReportTopSpaceCm` | P-01…P-04 |
| `ReportSettings.HeaderFooterMode` / `HeaderColor` / `FooterColor` | P-01…P-04 |
| `ReportSettings.DoctorSignatureEnabled` | P-01…P-04 footer |
| `ReportSettings.HistorySortMode` / `HistoryAutoDisplayEnabled` | P-04 |
| `SystemSettings.PrintLabIdInsteadOfPatientId` | P-01…P-04, P-07 barcode data |
| `SystemSettings.PrintAccountInsteadOfDateOnReport` | P-01…P-04 identifier/date line |
| `SystemSettings.PrintFileExternalBarcode` | P-01…P-04 external barcode rendering |
| `SystemSettings.PrintDateTimeOnTubeBarcode` | P-07 tube barcode content |
| `ReceiptSettings.*` (TopMarginCm, Currency, PickupTimeDefault, PrintOnce, TestDetailDisplayMode, CashierPrinterEnabled, HeaderFooterMode) | P-05 |
| `EnvelopeSettings.*` (TopMarginCm, HeaderFooterMode, SuppressCaptions) + `EnvelopePrintItemPosition` (Name/Code/ReferralEntity/Date, IsEnabled, LeftOffsetCm, TopOffsetCm) | P-06 |
| `PrinterAssignment.OutputType` → `PrinterName` | Routes P-01…P-09 to Reports / Barcode / Envelope / Receipt |

All settings are read at print time (not cached), consistent with the Data Model Blueprint's "computed/derived, live" principle and UI/UX §8.

---

## 7. Barcode Specification (P-07)

| Aspect | Specification |
|---|---|
| Symbology | **Code 128** (1-D), rendered through `IBarcodeService` |
| Data content | `PatientId` or `LabId` per `SystemSettings.PrintLabIdInsteadOfPatientId` |
| Tube barcode | When `PrintDateTimeOnTubeBarcode` is set, the tube label barcode encodes the identifier plus the print date/time; otherwise identifier only |
| Placement | Printed on the result report (when `PrintFileExternalBarcode`), on the tube label, and on the envelope when `EnvelopePrintItemPosition` "Code" item is enabled |
| Output printer | `PrinterAssignment` OutputType = Barcode |
| Rendering | Local library (e.g., ZXing.Net); no Internet dependency |

---

## 8. Preview & Print Execution Flow

Per UI/UX §4.8, every Print/Preview control sends a Command carrying the relevant identifiers and awaits a `Result`. The flow:

1. ViewModel issues the print/preview Command (e.g., `PrintReportCommand`, `BuildCombinedReportCommand`) via `IMediator`.
2. The handler loads the report model via queries and invokes the appropriate printing service (`IReportPrintingService`, etc.).
3. The service builds a `FixedDocument` from the report model + current settings.
4. **Preview:** the `FixedDocument` is returned/bound to the screen's preview region (`DocumentViewer`); no print spool yet.
5. **Print:** the service sends the `FixedDocument` to the `PrinterAssignment`-selected printer via `PrintDialog`/`XpsDocumentWriter`. The result (`Success`/`Unexpected`) propagates back as `Result`.
6. On `Unexpected`, `ResultErrorPresenter` shows a non-technical retry message (UI/UX §4.3); raw exception detail is never shown.

Receipt printing (P-05) honors `ReceiptSettings.PrintOnce` (one physical receipt per operation) and `CashierPrinterEnabled`.

---

## 9. Print Eligibility & Lifecycle Rules

| Rule | Specification |
|---|---|
| Result printability | A result row may be printed once it has reached at least the **Finished** lifecycle stage (`PatientTest.IsReviewed` / finish flags per the Data Model). Unfinished rows are excluded from P-01 by the query. |
| Print-block-on-balance | When the operating user's `BlockPrintOnRemainingBalance` is set and a balance remains, the Deliver/print path is refused with a `Forbidden`/`Conflict` result before printing (Data Model BR-07; UI/UX S-12). |
| Print count | `PatientTest.PrintCount` and `LastPrintedByUserId`/`AtUtc` are updated on each physical print (the T audit surface, Data Model §6.1, Architecture ADR-0014). |
| Re-print | Re-printing an already-printed result is permitted (it is not a new lifecycle stage); the print count increments. |
| Export vs print | `ExportResultCommand` produces a local file and does **not** advance any lifecycle stage and does **not** increment print count (Data Model §6.1). |

---

## 10. Printer Assignment

`PrinterAssignment` (Data Model §12.6) holds exactly four output types, each mapped to a `PrinterName`:
- **Reports** → P-01, P-02, P-03, P-04, P-08, P-09
- **Barcode** → P-07
- **Envelope** → P-06
- **Receipt** → P-05

There is **no card printer** (patient card printing excluded — §11). The assignment row is edited via S-28 (System Settings) and stored as a single row in `PrinterAssignment`. The physical printer name is a workstation-local string; selection is honored by the print service at execution time.

---

## 11. Out of Scope (Exclusions — must not appear)

Consistent with the PRD (§17) and the Architecture Blueprint:
- **Patient card (كارنيه) printing** — excluded by product-owner decision; no card artifact, no card printer, no card layout.
- **Online / web / portal delivery** of any report — prohibited (ADR-0010).
- **SMS / E-mail / Fax** result or receipt delivery — prohibited (PRD §17.2; Data Model BR-08).
- **Multi-branch** differentiation on any document — single-branch only (ADR-0022; Data Model BR-09).
- **Equipment / inventory tracking** documents — no such artifact.
- Any third-party cloud print service or external API — prohibited.

---

## 12. Acceptance Criteria (ties to Test Strategy)

The Reporting & Printing concern is accepted when:
1. Every printing service implements its Application interface and is registered in `DependencyInjection.cs` (Coding Standards §3, §5.3).
2. P-01 renders header (Words/Images), patient block (Lab ID or Patient ID per setting), results table with reference ranges and flags, and footer/signature per `ReportSettings`.
3. `ReportTopSpaceCm` exceeding 8 cm is rejected at the settings screen (S-29) and validated in the service.
4. `PrintLabIdInsteadOfPatientId` and `PrintAccountInsteadOfDateOnReport` each change the printed identifier/date line and the barcode data (UI/UX S-07; Data Model §12.1).
5. Envelope label (P-06) positions each enabled item (Name/Code/ReferralEntity/Date) at its `LeftOffsetCm`/`TopOffsetCm` from `EnvelopePrintItemPosition`.
6. Barcode (P-07) encodes the correct identifier (Code 128) and honors `PrintDateTimeOnTubeBarcode`.
7. Print-block-on-balance refuses printing with a `Forbidden`/`Conflict` result when a balance remains and the user's flag is set (Test Strategy §7.2 M09).
8. Print count and last-printed actor are updated on each physical print (Test Strategy §7.2 M04 / M10 T-view).
9. No printing path introduces an Internet dependency (Test Strategy §7.3; Architecture ADR-0010).
10. Printing/barcode services are testable through their interfaces with a physical or virtual target, and surface `Result.Failure` of type `Unexpected` when no target is available (Test Strategy §3.3).

---

## 13. Implementation Ownership

| Foundation / Module | Delivers |
|---|---|
| F4 / F6 (Foundations) | Printing service interfaces, `IBarcodeService`, DI registration, settings queries |
| M22 — System & Print Settings (Wave 1) | `PrinterAssignment`, `ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`, `EnvelopePrintItemPosition` editors (S-28–S-31) |
| M07 — Combined, Blank & History Reports (Wave 7) | P-01…P-04 rendering and preview (S-07–S-09) |
| M02 / M21 | P-05 receipt trigger, P-06 envelope trigger, P-07 barcode trigger |
| M11 / M13 | P-08 work sheet, P-09 price list printing |

---

*End of document.*
