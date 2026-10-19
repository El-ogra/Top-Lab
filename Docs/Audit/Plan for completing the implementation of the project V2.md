# Plan for completing the implementation of the project V2

---

## DOCUMENT HEADER

| Item | Value |
|---|---|
| Document | Implementation completion plan for the Top-Lab clinical laboratory information system |
| Date of preparation | 2026-10-19 |
| Written against | Repository state identified by target commit `9d90f499e95082ec40c417bd4910f1cfc62daec3` (branch `main`), working tree unmodified apart from one pre-existing untracked audit document |
| Plan identifier scheme | This plan uses its own identifiers throughout. Reference requirements are cited **only** in the forms `reference guide, page N` and `reference showcase, page N`. Implementation evidence is cited by repository file path and line range. |
| Environment — host | Windows 10 (10.0.19045), x64 |
| Environment — SDK | .NET SDKs installed: 8.0.426 and 9.0.318. Build executed with the default SDK; runtimes present include `Microsoft.NETCore.App 8.0.32` and `Microsoft.WindowsDesktop.App 8.0.32` |
| Environment — tooling used | `git` (read-only), PowerShell 7, Python 3.13.1 with PyMuPDF 1.28.2 and Pillow 12.2.0 for PDF text extraction, page rasterisation and contact sheets, `dotnet build`, `dotnet test` |
| Environment — database | **No database was reached.** No SQL Server or LocalDB instance was contacted. No EF Core tooling command (`migrations add/remove/script/list`, `database update`, `has-pending-model-changes`) was executed. No migration was applied, generated or altered. |
| Environment — application | The application GUI was never launched |
| Repository preservation | The repository working tree, index, references, worktrees and stashes were left exactly as found. All builds and tests were executed against a plain file copy of `src/`, `tests/` and the solution/props files made outside the solution folder. |

### What was actually verified

* The whole solution **builds** — `dotnet build TopLab.sln -c Debug` completed with **0 errors and 0 warnings** across all nine projects, including the WPF presentation project and its test project.
* The whole test suite **runs** — `dotnet test TopLab.sln` executed **3 184** tests: **3 182 passed, 0 failed, 2 skipped**. The two skips are relational tests gated on an unavailable container engine; they are reported as skipped and are never counted as passed.
* Both authorised reference PDFs were extracted page by page (212 + 77 pages) and were rendered and inspected — individually at high resolution for the pages that carry function evidence, and through contact sheets for the remainder.
* Every classification in this plan was re-derived from the actual code at the target commit by reading the files, by systematic negative search with recorded term counts, and — where it changes a conclusion — by executing the tests that pin the behaviour.

### What was not verified

* No runtime observation of any screen, dialog, print job or PDF. The application was never started, so **no** claim in this plan carries a runtime verification status.
* No live database. Persistence behaviour is established from EF Core configuration, the migration sources, the model snapshot and tests that use the in-memory provider or model-only contexts. `MigrationChain_AppliesToEmptyDatabase` — the only test that would prove the migration chain applies to an empty SQL Server database — was **skipped** because no container engine is installed on this host.
* Visual correctness of produced documents (Arabic shaping, right-to-left ordering, pagination, colour rendering, barcode legibility, font resolution) was not observed. No document was generated and examined.
* Barcode scanning, thermal label printers and any other hardware were not exercised.
* Reference requirements were taken from the two authorised PDFs only. No external site, manual or domain knowledge was used to establish a requirement.

### Build and test limitations that affect confidence

1. Two relational tests cannot run here. Any conclusion that depends on the migration chain actually applying to a fresh database is **not** verified.
2. The in-memory EF provider does not enforce relational constraints. Tests that use it prove mapping and behaviour, not DDL correctness.
3. Presentation tests exercise view models and navigation wiring; they do not open windows.

### Overall confidence limitations

* Classification confidence is **high** for every negative (MISSING) row: each rests on a documented search whose scope and terms are recorded, on a file listing of the whole `src` tree, and on the absence of any entity, command, query, DTO, view or view-model that could carry the capability.
* Classification confidence is **high** for the DIFFERENT rows, which rest on the actual configuration and rendering code.
* Classification confidence is **medium-high** for IMPLEMENTED rows. Each was traced from a user-facing entry point to a handler and to a persistence or rendering terminus, and the existence of every handler was checked mechanically. No IMPLEMENTED row was accepted on the strength of a class name, a column, a comment or a test name alone.
* Confidence is **low** for the three BLOCKED rows, by construction: the reference does not describe an in-scope data route for them.

---

## SECTION 1 — VERIFICATION CONTEXT

### 1.1 Baseline

The investigation began from a verified baseline. `git rev-parse HEAD` returned the required target commit. `git status --porcelain=v1 --untracked-files=all` returned exactly one entry, an untracked audit document. `git diff --stat` returned nothing. A reproducible fingerprint was computed with read-only commands only: target tree object, SHA-256 over `git ls-files -s`, SHA-256 over `git ls-tree -r HEAD`, the SHA-256 of the permitted untracked document, and the complete reference, worktree, stash and remote listings. No command that writes Git objects or metadata was run at any point.

### 1.2 Reference material actually inspected

Both authorised documents were processed in full:

| Attribute | Reference guide | Reference showcase |
|---|---|---|
| Page count (confirmed) | 212 | 77 |
| Pages whose text layer was extracted | 212 / 212 | 77 / 77 |
| Pages rendered to an image | 212 / 212 | 77 / 77 |
| Pages **visually inspected individually** at 190 dpi | 193, 195, 197, 199, 206 (5 pages) | 3, 13, 51, 53, 73 (5 pages) |
| Pages **visually inspected through contact sheets** | none | 1–26 (2 sheets) |
| Pages inspected **only as extracted text** | 207 of 212 | 51 of 77 |
| Pages not inspected at all | 0 | 0 |

Ten pages were opened individually at high resolution — the pages that carry the function evidence this plan depends on — and a further 26 pages were opened as low-magnification contact sheets. The remaining 258 pages were read only through their extracted text layer. That is a real limitation: it establishes page structure, chapter contents and the presence of a capability, but it does not establish fine field text on a page that was never opened as an image. Where a claim in this plan depends on fine text, the page is one of the ten individually inspected pages.

The showcase document's pages 56–66 carry a mirrored Latin text layer inside their screenshots, so only the rendered image was relied upon for them; the individual figures below were read from those page images.

Both documents are Arabic, right-to-left and screenshot-heavy. The text layer shows orthographic corruption and, inside screenshots, reversed Latin. Where a field label, a number or a caption mattered, the rendered page governed.

### 1.3 Corrections applied to the prior assessment

The following prior conclusions were tested against primary evidence and are **not** carried into this plan.

| Prior conclusion | Finding | Evidence |
|---|---|---|
| 189 reference functions, of which 21 owner-excluded and 168 in scope | **Corrected.** 192 reference functions; 23 owner-excluded; 169 in scope | Reference guide, page 174 does not evidence a year-end settlement function. Reference guide, page 199 does not evidence the six print switches claimed for it — the switches actually on that page are all implemented. Reference guide, page 206 evidences per-item font, enable and left/top offsets on the envelope but no before/after ordering control. Four in-scope functions present in the PDFs were absent from the inventory: database backup and restore, database/server connection settings, database and program update, and the pictured report. Two further owner-excluded functions appear in the PDFs. |
| Year-end account settlement is a reference function | **Withdrawn** | Reference guide, page 174 chapter contents list eight items: lab inventory, period inventory, sent-out-sample inventory, doctor inventory, contract-entity inventory, branch inventory (excluded), sent-out account settlement. Annual inventory is present and is already covered by the period inventory function. No annual *settlement* is evidenced anywhere in either document. |
| Six specific report print-option switches (print with the inventory account; delivery report with the account; cheque without test value; print so far; all tests with/without invoice; entered tests only) | **Withdrawn** | Reference guide, page 199 was rendered and inspected. The switches on that screen are: default referral text, save sample types to the entity memory, name search assist, disable automatic numbering insertion, print the patient barcode, print date and time on the barcode, print the lab code instead of the patient code, automatic review/completion, print the patient account in the results report, and daily database backup. All are implemented. No screen in either document shows the six claimed switches. |
| Envelope item ordering ("place the item after / before") | **Withdrawn** | Reference guide, page 206 was rendered and inspected. The envelope item table exposes a Font selector, an enable checkbox and left/top offset spinners. No ordering control exists. |
| No comment/code divergence exists in the receipt writer | **Corrected to the opposite.** The receipt writer's comment is accurate | `src/TopLab.Infrastructure/Printing/ReceiptPdfWriter.cs:104-105` computes `renderWordsHeader = logoPath is null` and passes it into the text-line builder, and `:286-302` suppresses the words header while a logo is present. The comment block at `:156-169` describes exactly that. There is no divergence. |
| Removing a test from a visit after results exist is an unconfirmed defect | **Refuted.** The guard exists and is executed | `src/TopLab.Application/Features/PatientRegistration/Commands/RemoveTestFromVisit/RemoveTestFromVisitCommandHandler.cs:25-28` returns a conflict when `EnteredAtUtc`, `ResultValue` or `ResultFlag` is set. `.../ClearAllTests/ClearAllTestsCommandHandler.cs:80-83` applies the same guard, plus a first-registration-only guard at `:39-72`. The 3 184-test suite exercises this area and passed. |
| Combined, separate-history and bulk print paths may not increment the print count | **Refuted.** A single recorder performs the transition on every path | `src/TopLab.Application/Features/ResultsEntry/Common/IPrintedStateRecorder.cs:82-133` is the only place that calls `PatientTest.MarkPrinted` / `ProfileResultItem.MarkPrinted` for the shared paths; it is consumed by `PrintCombinedReportCommandHandler`, `PrintHistoryReportCommandHandler` and `ExecuteBulkPrintCommandHandler`. `PatientTest.MarkPrinted` (`src/TopLab.Domain/Results/PatientTest.cs:186-188`) increments `PrintCount` and stamps the last printer. |
| `EnableAbnormalResultColor` does not gate the per-profile normal colours (a defect) | **Refuted as a defect.** The asymmetry is the specified behaviour and is pinned by an executed test | `src/TopLab.Infrastructure/Printing/ReportContentBuilder.cs:656-673`. `tests/TopLab.Infrastructure.Tests/Printing/ProfileResultStylingPrecedenceTests.cs` contains `Combined_NormalWithProfileStyling_AppliesEvenWhenAbnormalColouringDisabled` with the stated rationale, and the test suite passed. |
| Permission granularity and print-element positions are "noted, not reclassified" | **Reclassified as DIFFERENT** | Reference guide, page 151 lists a distinct grant "access to the cash drawer account and other accounts", separate from the cash deposit/disbursement grant. `src/TopLab.Application/Features/InventoryAndAccounting/Common/InventoryAndAccountingAccessPolicy.cs` gates every inventory query on `CASH_DISBURSE_DEPOSIT`, so the two grants cannot be separated. Reference guide, page 197 shows per-header-element enable, left and top offsets plus long-date, show-time, width and height options; `src/TopLab.Domain/Settings/ReportItemTemplate.cs` keys body lines only and `ReportItemTemplateConfiguration.cs:11-21` maps no font or offset-beyond-left attribute. |
| The picture report was not inventoried | **Added as a MISSING function** | Reference showcase, page 24 evidences a picture report for CBC, culture, semen, stool and urine samples: from one to four images per report, changeable image position and size, draggable text and shapes, selectable element font colour/size/type, and shapes, numbers and words placed on and off the images with control of position, size and colour. No image entity, command, query, DTO, view or view-model exists in `src`. |
| Database backup/restore, connection settings and program update were not inventoried | **Added as IMPLEMENTED functions** | Reference guide, pages 190, 191, 200, 202 and 205. `src/TopLab.Application/Features/SystemAndPrintSettings/Commands/BackupDatabaseNow`, `.../RestoreDatabase`, `.../UpdateDatabaseServerSettings` and `.../ApplyDatabaseUpdates` all exist, are registered, and are surfaced by `src/TopLab.Presentation/Views/Settings/DatabaseMaintenanceView.xaml`. |

### 1.4 Claims that could not be verified

| Claim | Status | Why |
|---|---|---|
| The migration chain applies cleanly to an empty database | **NOT VERIFIED** | The only test that proves it is gated on a container engine that is not installed on this host; it was skipped. |
| EF Core reports no pending model changes | **NOT VERIFIED** | No EF Core tooling command was run and no database was reached. |
| Printed documents render correctly | **NOT VERIFIED** | Nothing was printed and no document was generated and examined. |
| Every IMPLEMENTED function behaves correctly at runtime | **NOT VERIFIED** | The application was never launched. |
| Whether a screenshot-only reference behaviour is a requirement or an example | **NOT VERIFIED — owner decision** | The reference gives no prose for several capabilities that are evidenced only by a caption beneath a screen capture. |

---

## SECTION 2 — SCOPE AND EXCLUSIONS

### 2.1 The five permanent owner exclusions, in plain language

1. **Online results portal and its ecosystem.** The website where patients, doctors and partner laboratories read a result, the desktop uploader, the Android application, portal receipt printing, portal-side administration and portal user accounts are all out of scope.
2. **Equipment management.** Registering laboratory equipment and recording its condition, its periodic maintenance and its calibration dates are out of scope.
3. **Analyser integration.** Connecting to a blood-count analyser, importing its results, the analyser interface and server settings, the automatic per-element correction factors, the supported-hardware list and the analyser device login are out of scope.
4. **Multi-branch operation.** The application is single-branch. Branch master data, a branch selector on any screen and any branch-scoped reporting dimension are out of scope.
5. **External result delivery.** Sending a result by SMS, e-mail or fax, and any laboratory-branded SMS notice, are out of scope.

### 2.2 How the boundary was enforced

The exclusions were applied at four points and nowhere else:

* **Inventory.** A reference capability was admitted to the inventory only after its page evidence was read. Anything whose page shows a portal, an analyser, equipment, a branch or an external delivery channel was recorded as excluded and carried no implementation work.
* **Classification.** An excluded function never received an implementation classification and never appeared in the gap register as work.
* **Phase design.** No phase proposes a table, column, command, screen or setting whose purpose is excluded. Where an in-scope capability shares a screen or table with an excluded one — patient search and the branch selector; the cash-drawer inventory and the branch selector; external entities and their portal account code — only the in-scope portion was planned, and the excluded portion is named so that a coding agent does not reintroduce it.
* **Migration decisions.** No migration decision in this plan depends on an excluded data source.

### 2.3 The 23 owner-excluded functions

`G` = reference guide, `S` = reference showcase.

| # | Excluded function | Evidence | Category |
|---|---|---|---|
| 1 | Upload a patient result to the online results website | G, pages 58–62 | 1 |
| 2 | Open the results website and view the patient's result | G, page 63 | 1 |
| 3 | Print the portal data on the thermal receipt | G, page 69 | 1 |
| 4 | Android application through which the patient prints or saves a result | G, page 63; S, page 53 | 1 |
| 5 | Create a treating-doctor account for the results service | G, pages 142–144 | 1 |
| 6 | Create a partner-laboratory account for the results service | G, pages 145–147 | 1 |
| 7 | Count the patients whose results were uploaded in a period | G, pages 65–67 | 1 |
| 8 | Block or delete a patient result from the results website | G, page 68 | 1 |
| 9 | Follow up a referral entity's portal cases (lab to lab) | S, page 53 | 1 |
| 10 | Show the portal delivery data on the printed receipt | G, page 204 | 1 |
| 11 | Equipment register with condition, periodic maintenance and calibration date | S, page 42; G, pages 142, 145 | 2 |
| 12 | Connect to a haematology analyser and pull its results into the lab record | S, pages 67, 74 | 3 |
| 13 | Analyser interface and server settings | S, page 72 | 3 |
| 14 | Automatic analyser result-correction factors | S, pages 70, 74 | 3 |
| 15 | Supported analyser and hardware list | S, pages 75, 77 | 3 |
| 16 | Haematology analyser interface device login | S, page 67 | 3 |
| 17 | Cash-drawer inventory for a selected branch number | G, pages 178, 179 | 4 |
| 18 | Search for patients within a specified branch | G, pages 42, 45 | 4 |
| 19 | Laboratory branches master-data window | G, pages 142, 145 | 4 |
| 20 | Send the result by SMS | G, pages 17, 47 | 5 |
| 21 | Send the result by e-mail | G, pages 17, 18, 23, 47 | 5 |
| 22 | Send the result by fax | G, page 17 | 5 |
| 23 | Laboratory-branded SMS notice (congratulation, result, completion, marketing) | S, page 53 | 5 |

Two of these — #10 and #19 — were not previously separated out. Both are recorded here because their evidence is unambiguous and their category is not in doubt; neither creates an implementation obligation.

### 2.4 Boundary notes carried into the phases

| Boundary | Where it appears | Treatment |
|---|---|---|
| Patient search carries a branch predicate in the reference (G, page 42) | Search predicate family | The in-scope predicates are planned; the branch predicate is deliberately absent and must stay absent. |
| Cash-drawer inventory carries a branch selector (G, page 179) | Period inventory | Same. |
| Statistics carry a per-branch grouping dimension (S, page 53) | Statistics module | The per-branch dimension is excluded; the classification dimensions that are in scope are already implemented and are not re-planned. |
| External entities carry a portal-account code (G, pages 143–147) | External-entity master data | The entity CRUD and its price list are in scope; the generated portal identifier and password must not be reintroduced. |
| CBC report artefacts whose data source is the analyser (S, pages 56–66) | Blocked rows BLOCK-001…003 | Recorded as blocked pending an owner decision. No in-scope data route was invented. |
| The reference's chart and scatter-plot rendering on a laboratory report is evidenced in scope for the semen-analysis report (S, pages 20–21) | DEC-001 | This precedent is cited so that the owner can judge the CBC boundary against a lab-system precedent rather than against the analyser product alone. |

---

## SECTION 3 — FUNCTIONAL GAP REGISTER

Twenty entries. Each is either **DIFFERENT** (the capability exists and must be brought to the reference) or **MISSING** (the capability is absent). Blocked rows are listed separately in §3.2 and are **not** work items.

### 3.1 Gap register

| GAP | Ref | Function | Class | Verified current behaviour | Reference evidence | Page |
|---|---|---|---|---|---|---|
| GAP-001 | — | Per-user permission catalogue granularity | DIFFERENT | 13 seeded permissions in `src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs:17-31`. All inventory and cash-account queries are gated on the single code `CASH_DISBURSE_DEPOSIT` (`InventoryAndAccountingAccessPolicy.cs`), so a grant to move money is inseparable from a grant to read the cash-drawer and other accounts. `DISCOUNT_LIMIT` is seeded but never used as a gate. | The user window lists "cash deposit/disbursement and accounting of patients and external entities" and, as a **separate** grant, "access to the cash-drawer account and other accounts (daily/weekly/monthly/annual inventory)". | G, pages 151–153, 157 |
| GAP-002 | — | Per-header/footer print-element enable and position, and top-line options | DIFFERENT | `ReportItemTemplate` keys **body lines** only (24 keys, `src/TopLab.Domain/Settings/ReportItemKeys.cs`). `ReportItemTemplateConfiguration.cs:11-21` maps a single `LeftOffsetCm`; there is no top offset, no enable/disable and no per-element date format. `ReportDocumentContent.cs:8-30` carries one document-wide `HeaderFooterMode`, one `DrawHeader` and one `DrawFooter`. `ReportPageComposer.cs:62-74` applies marker and left offset to body lines only. The envelope half of the same reference capability **is** implemented (`EnvelopePrintItemPosition` with enable, left and top). | The default-layout window shows per-element enable checkboxes (Patient Name, Age/Sex, Referred By, Patient Id, Request Date, Printed In), a per-element Left and Top offset, `Show top line`, `Show bottom line`, `Long date`, `Show time`, `Width` and `Height` for the top line. | G, pages 193, 196, 197, 206 |
| GAP-003 | — | Configurable barcode sticker size | MISSING | `BarcodeLabelRenderer.Render(payload)` is called with its defaults; the label page is a compile-time constant of 288 × 144 pt (`BarcodeService.cs:69, 96-98`). No settings row, command, DTO or view exposes a label dimension. Term search `LabelWidth`, `LabelHeight`, `StickerSize` across `src`: **0 matches**. | "Sticker size 38 mm wide × 25 mm long", with the label catalogue for tubes and samples shown at that size. | S, page 3 |
| GAP-004 | — | Sequential sample number printed on the report | MISSING | `ReportPageComposer.cs:134-138` prints the file/patient number and the patient name; `ReportDocumentContent` carries no sequence field. Term search `SampleNumber`, `SampleNo`, `SequenceNumber` across `src`: **0 matches** outside migrations. | CBC report pages carry an auto-incrementing sample number and a separate `Sample ID`. | S, pages 56, 61, 62, 65 |
| GAP-005 | — | Patient name printed in Arabic and English on the report | MISSING | `ReportPageComposer.cs:138` emits exactly one string, `اسم المريض: {PatientFullName}`. `Patient` carries a single `FullName`. Term search `FullNameEn`, `EnglishName`, `NameEn`, `Transliterat` across `src`: **0 matches**. | The default-layout preview shows `Patient Name = Eng. Ahmed Ibrahem Saleh` beside the Arabic name; the CBC report prints the name in both scripts. | G, page 197; S, page 60 |
| GAP-006 | — | Colour versus black-and-white report printing | MISSING | Every PDF writer calls `page.PageColor(Colors.White)` unconditionally, including `ReportPageComposer.cs:361`. No output-mode switch exists anywhere. Term search `BlackAndWhite`, `Grayscale`, `ColourMode`, `ColorMode`, `Monochrome` across `src`: **0 matches**. | The product description states that, in the colour-printing case, printing is also possible on plain white or pre-printed paper. | S, page 59; S, page 61 |
| GAP-007 | — | A3 paper and A4 at 85 × 55 | MISSING | `src/TopLab.Domain/Common/Enums/PaperSize.cs` contains exactly `A4 = 0` and `A5 = 1`. `ReportSettings.PaperSize` is persisted as an int-converted enum. Term search `A3` across `src`: **0 matches**. | "and can be printed on large paper A3 or A4 85×55". | S, page 74 |
| GAP-008 | — | Folded / multi-port report printing | MISSING | No folding, port or imposition construct exists in the PDF pipeline. Term search `Folded`, `Fold`, `Port` across `src`: **0 matches**. | "3-port and 5-port folded output" is offered as one of the ten report layouts. | S, page 64 |
| GAP-009 | — | Font family and size per report line | MISSING | The report content line carries `MarkerSymbol`, `LeftOffsetCm` and `LineSpacing` only (`ReportContentBuilder.cs:737`, `ReportPageComposer.cs:62-74`, `:319`). Font size is document-wide. Term search `LineFontSize`, `ReportLineFont`, `ItemFontSize`, `ItemColor` across `src`: **0 matches** — the only font attributes in the codebase are receipt header/footer fonts and the single lab print-text font. | The header/footer item table gives each line its own **Font** selector (0, 0.5, 1, 1.2, 1.5, 1.8, 2, 3, 9, 9.5); the doctor's-signature block gives each of its three lines its own **Font** button. | G, pages 193, 195 |
| GAP-010 | — | Per-user (doctor) report element toggle | MISSING | `ReportItemTemplateConfiguration.cs:11` declares `HasKey(e => e.ItemKey)` — one global key and no user column. `ReportSettings` is a single row. Term search `UserReportSettings`, `ReportPreference`, `UserLayout` across `src`: **0 matches**. | A side menu by which a doctor shows or hides report elements — a per-user preference, distinct from the global "show this line" checkbox. | S, page 15 |
| GAP-011 | — | Font size per envelope item | MISSING | `EnvelopePrintItemPosition` carries exactly `ItemName`, `IsEnabled`, `LeftOffsetCm`, `TopOffsetCm`; its constructor and `Update` accept only those four (`src/TopLab.Domain/Settings/EnvelopePrintItemPosition.cs:6-41`). `EnvelopePdfWriter.cs:70-72` resolves one document-wide font family and size. Term search for an envelope font attribute: **0 matches**. | The envelope window's item table gives each envelope item its own **Font** selector. | G, page 206 |
| GAP-012 | — | Optional vertical coding on receipt, envelope and labels | MISSING | No rotation or vertical-text mode exists. Term search `VerticalCode`, `VerticalText` across `src`: **0 matches**. The only barcode rendering path emits a horizontal label (`BarcodeService.cs:69`). | "Printing the barcode on the receipt and the envelope to ease handling"; the barcode-label page adds vertical coding for handing results over and reprints. | S, pages 3, 52; G, page 206 |
| GAP-013 | — | Barcode labels per sample material, with a doctor-editable test distribution | MISSING | `IBarcodeService.PrintBarcodeAsync(string value)` accepts exactly one identifier (`BarcodeService.cs:44`) and its only caller passes the patient id or the file number. No sample-material entity, command, DTO, view or view-model exists. Term search `SampleMaterial`, `ContainerLabel`, `SwabLabel`, `TubeLabel` across `src`: **0 matches**. | Seven labels for one patient, one per drawn material (serum fasting, serum postprandial, EDTA blood, citrated blood, urine, semen, stool), each listing the tests assigned to that material, with the ability to change the distribution and to paste the file on a chosen label. | S, pages 3, 52 |
| GAP-014 | — | Picture report for a sample (one to four images) | MISSING | No image, attachment, microscopy-photo or picture-report entity, command, query, DTO, view or view-model exists. `src/TopLab.Domain/Results` contains only text-valued microscopy findings. `ReportPageComposer` emits text and tables only (`ComposeItemLine` at `:280-320`). Term search `SampleMaterial`, `Image`, `Attachment`, `Picture` narrowed to result and report code: **0 matches** for a stored image. | "A picture report can be made for the following tests — CBC, culture, semen, stool, urine. From one image to four images can be placed in the report; the image place and size can be changed; texts and shapes can be moved by dragging; the colour, size and type of the font used in the elements can be chosen; shapes, numbers and words can be placed on and off the images and their places, dimensions and colours controlled." A second caption states the picture report can also be produced on a separate sheet from inside five of the tests. | S, page 24 |
| GAP-015 | — | Glossary of analysis abbreviations | MISSING | `Test.Short` / `TestCode` are catalogue fields used as headings and by the receipt display mode. There is a `GetTestLibrary` query that lists tests; it is a test list, not a glossary. Term search `Glossary`, `Terminology`, `Abbreviation` across `src`: **0 matches**. | The utilities window carries a "glossary of the abbreviations used in the field of medical analyses" button. | S, page 51 |
| GAP-016 | — | Sample-nature interpretation panel | MISSING | `Test.Collection` plus `LogGroup`/`LabGroup` labels are used as report headings and `ReferenceRange` supplies the normal interval, but there is no interpretation entity, panel, command, query, DTO, view or view-model that presents a sample's nature together with its normal ranges and their clinical meaning. Term search `SampleNature`, `Interpretation` across `src`: **0 matches**. | The utilities window carries a "sample interpretation" button, described as "reading the nature of the samples, their normal rates and their effect on the patient". | S, page 51 |
| GAP-017 | — | Banded result monitor over blood-picture element values | MISSING | `GetBandedResultMonitorQueryHandler.cs:56-68` reads `PatientTest.ResultValue` and requires `ResultFlagComputer.TryParse` to succeed; it never touches `ProfileResultItem`, where every blood-picture analyte value lives. Term search for an analyte-level band query across `src`: **0 matches**. The pivot that would make it cheap, `ReportProduction/Common/HistoryMatrixBuilder.Pivot`, is wired only to patient history. | A search and statistics window over blood-picture elements in which each element (HGB, RBCs, WBCs, PLT, RETICS, HCT, MCV, MCH, MCHC, PCT, MPV, LYM%, MON%, NEU%, EDS%, BAS%, Band%, the absolute counts, and leukaemia cells) has its own check box and its own minimum and maximum. | S, page 73 |
| GAP-018 | — | Charted blood-count history report | MISSING | `HistoryMatrixBuilder.Pivot` produces the **tabular** date × analyte matrix only. No charting component is referenced anywhere; `src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs:30` explicitly records that there is no charting library. Term search `Chart`, `Plot`, `BarSeries`, `LineSeries` across `src`: **0 matches** other than that comment. | A "COMPLETE BLOOD COUNT HISTORY REPORT" with a bar chart per analyte and trend lines for the RBC indices and the differential across visits. | S, page 66 |
| GAP-019 | — | Auto-generated remark composed from result flags | MISSING | The report emits `LowComment` / `HighComment` text sourced from the reference range (`ReportContentBuilder.cs:82-87, 149-154, 233-238`), which is a stored comment, not a remark composed from the abnormal findings. `ProfileResultItem` carries a flag but no remark field. Term search `Remark` across `src`: **0 matches** other than XML documentation. | CBC report pages carry an automatically generated remark line. | S, pages 56, 61, 63 |
| GAP-020 | — | Appointment-note reminder to a user or contact | MISSING | The utilities area contains a phone book and a purchases list and nothing else of this kind. Term search `Reminder`, `Appointment`, `Notification`, `Scheduling` across `src`: **0 matches** for any of the first three; the single `Scheduling` hit is a comment about operating-system task scheduling. | The utilities window carries a "reminder message for a specified user or person through the appointments note" button. | S, page 51 |

### 3.2 Blocked rows — not work items

| ID | Ref | Function | Why blocked |
|---|---|---|---|
| BLOCK-001 | — | CBC report histograms beside the results | The reference evidence is the analyser-interface report family, whose provenance line states the values are "done on fully automatic haematology analyzer". Acquisition of those images is an excluded capability. The lab system has no in-scope source for a histogram image. The reference does not describe an in-scope data route, so one must not be invented. Evidence: S, pages 56–65. |
| BLOCK-002 | — | Two-dimensional scattergram for the differential | Same boundary as BLOCK-001. Evidence: S, page 65. |
| BLOCK-003 | — | Five-part and three-part differential reporting | The reference's relative/absolute pairs and its `LYM# / MID# / GRA#` grouping are the analyser's own output. Acquisition is excluded. Evidence: S, pages 64, 65. |

All three are released by a single decision, DEC-001, recorded in Section 6. They are not counted as gaps, are not sequenced, and carry no migration.

---

## SECTION 4 — ORDERED IMPLEMENTATION PHASES

Twenty phases. Phase 1 is the phase that should be executed next.

---

### PHASE 1 — Separate the "view accounts" grant from the "move money" grant

**A. Phase identification.** Phase 1. Gap **GAP-001**, class **DIFFERENT**. Estimated size **low**. Estimated risk **low**. Size is low because the change is a data-seed addition plus two gate constants and one view-model change; risk is low because it widens the grant model without removing any existing capability, and the only behavioural change is that some operators who could previously read the cash-drawer account will no longer be able to unless granted.

**B. Dependency position.** This is first because it is the only phase that changes an authorization seed, and every later phase that adds a screen or command must be gated by a code that already exists. Nothing else depends on it and nothing blocks it: `PermissionConfiguration` is a seed list, the authorization pipeline is `src/TopLab.Application/Common/Behaviors/AuthorizationBehavior.cs` and every feature already references a seeded code through its own `*AccessPolicy` constant class. Downstream phases affected: Phase 2 (element settings are edited from the accounts-adjacent settings surface) and every phase that registers a view — the new grants must be selectable there.

**C. Required reference behaviour.** The user-management window offers the administrator a list of grants. Two of them are separate check boxes and must stay separate:

* cash deposit and disbursement, and the accounting of patients and of external entities;
* **access to the cash-drawer account and the other accounts (daily, weekly, monthly and annual inventory)**.

Reference guide, pages 151–153 and 157.

**D. Current Top-Lab behaviour.** `src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs:17-31` seeds 13 permission rows. `src/TopLab.Application/Features/InventoryAndAccounting/Common/InventoryAndAccountingAccessPolicy.cs` exposes a single constant `CashDisburseDeposit = "CASH_DISBURSE_DEPOSIT"` and every inventory and cash-account query is gated on it. Verified statically; the whole suite builds and passes.

**E. Exact behaviour delta.**

| | Reference | Current | Delta |
|---|---|---|---|
| Grant list | cash movement **and** account visibility are two grants | one grant covers both | an operator cannot be given read-only account access |
| Discount limit | a grant ("the discount percentage the user may give the patient") | seeded as `DISCOUNT_LIMIT` but enforced only as a numeric ceiling on the user, never as a gate | a seeded row is not selectable in the grant model |

Changes required: introduce a distinct view-accounts code, gate the inventory/account **read** queries on it and the cash-movement **write** commands on the existing code, and decide explicitly (see DEC-008) whether `DISCOUNT_LIMIT` becomes a real gate or is removed from the catalogue. Regression risk: operators who relied on a single grant lose read access until re-granted — mitigated by seeding the new grant as **granted** for the administrator and for any existing role during the migration back-fill.

**F. Exact implementation changes by layer.**

* **Domain** — modify `src/TopLab.Domain/Users/Permission.cs` only if a helper is needed; no new entity.
* **Application** — modify `src/TopLab.Application/Features/InventoryAndAccounting/Common/InventoryAndAccountingAccessPolicy.cs` to add `ViewAccounts`; change the `RequiredPermissionCode` on the inventory **read** queries (`GetCashDrawerInventoryQuery`, `GetElementInventoryQuery`, `GetPatientSamplesDetailQuery`, `GetCompanyDelegateAccountsQuery`, `ListCashMovementsQuery`) from the cash-movement code to `ViewAccounts`. Leave the write commands (`RecordCashDepositCommandHandler`, `RecordCashDisbursementCommandHandler`, `RecordSentOutPaymentCommandHandler`, `SettleSentOutInFullCommandHandler`) on the cash-movement code. Also `src/TopLab.Application/Features/PatientBilling/Common/PatientBillingAccessPolicy.cs` and `.../SentOutSamples/Common/SentOutSamplesAccessPolicy.cs` for the sent-out account reads.
* **Infrastructure** — modify `src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` to add the new seeded row(s).
* **Presentation** — modify `src/TopLab.Presentation/ViewModels/Users/UserManagementViewModel.cs` only if the grant list is hard-coded there; otherwise no change.
* **Tests** — extend `tests/TopLab.Application.Tests/Features/UsersAndPermissions/` with an authorization-behaviour test asserting that a user holding only the cash-movement code is refused the inventory queries, and that a user holding only the view-accounts code is refused the cash-movement commands.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddAccountVisibilityPermission`. Additive only: one row inserted into the `Permissions` seed (`Code` `VIEW_ACCOUNTS`, max length 50 per `PermissionConfiguration.cs:14-15`, `Description` max 300). No column is added, altered or dropped; no primary key, foreign key or unique index changes. Back-fill: because seeded rows are applied by the migration, the new row must also be granted in `UserPermissionGrants` for every user that currently holds `CASH_DISBURSE_DEPOSIT`, so no operator loses access — this is the single required data transition. Evidence the state is not already persisted: `PermissionConfiguration.cs:17-31` contains exactly thirteen `HasData` entries and no row with a view-accounts code. The migration is appended after `20261018144852_AddProfileResultStyling`; no earlier migration is edited.

**H. Business rules and message text.** The grant names shown to the administrator must match the meaning of the two reference lines; the exact Arabic label is the implementing agent's to choose and must be recorded. No reference message text governs the refusal, because the reference does not describe a refusal message; reuse the existing authorization-failure message so that no new string is invented.

**I. Objective acceptance criteria.**

1. Given a user granted only the cash deposit/disbursement permission, when that user opens the cash-drawer inventory screen, then no inventory total, discount, commission or net-profit figure is returned and no patient drill-down row is returned.
2. Given a user granted only the account-visibility permission, when that user attempts to record a cash deposit or disbursement, then the command is refused.
3. Given a user granted both, when that user records a movement and then reads the period inventory, then both succeed and the movement appears in the inventory.
4. Given a user with absolute permission, when that user performs any of the above, then every action succeeds (the existing bypass is unchanged).
5. Given the migration has been applied, when the permissions table is read, then the new code exists exactly once and every user who previously held the cash-movement code also holds the new code.
6. Given the administrator edits grants and saves, when the user is reloaded, then the saved grant set is returned unchanged.

**J. Test plan.** Application tests: authorization behaviour for each of the six acceptance cases above. Domain tests: none required. Persistence test: a configuration test asserting the new seeded row exists and is unique, following the style of `tests/TopLab.Infrastructure.Tests/Persistence/InvoiceIssueConfigurationTests.cs`. Migration test: a migration test asserting the new row is inserted and that the back-fill grants it to existing holders, following the style of `tests/TopLab.Infrastructure.Tests/Persistence/Migrations/AddAntibioticMasterFieldsMigrationTests.cs`. Presentation test: the user-management view model returns the new grant in its list. No Windows-runtime or printer dependency.

**K. Phase-end verification gate.** Build the whole solution; run the whole test suite; run the migration against a disposable empty database and confirm the seed and the back-fill; confirm every existing migration is byte-identical to its prior state; confirm every acceptance criterion above; inspect the complete set of changed files and confirm that no excluded capability — no portal screen, no analyser setting, no branch field, no delivery channel — was introduced; record every command, exit code and failure.

**L. Risks and rollback.** Risk: an existing operator loses account visibility if the back-fill is omitted or wrong; this is a data-integrity risk on an authorization table and must be covered by the back-fill. Risk: two grant rows with overlapping meaning confuse administrators. Rollback: revert the changes made by the phase, restoring the affected files to their prior state, and remove the appended migration so that a fresh database is created from the pre-phase state. The safest rollback is before any real database has been migrated; after that, the grant back-fill must be reversed explicitly.

**M. Phase status.** **FINAL**, subject only to DEC-008 (whether `DISCOUNT_LIMIT` becomes a gate or is withdrawn), which is confined to one seeded row and does not block the grant separation itself.

---

### PHASE 2 — Per-header/footer element enable, offsets and top-line options

**A. Phase identification.** Phase 2. Gap **GAP-002**, class **DIFFERENT**. Estimated size **medium**. Estimated risk **medium**. Size is medium because the report header/footer must become addressable per element across the settings entity, the command, the query, the DTO, the content model and the composer. Risk is medium because the report header is drawn by shared code used by every report shape; a regression would misprint every report.

**B. Dependency position.** Second because the report settings surface (`src/TopLab.Presentation/Views/Settings/ReportSettingsView.xaml`) and the settings commands are already used by Phase 1's screen neighbourhood and are touched again by Phases 5, 6, 7 and 9. Doing it before those phases means each of them adds its column to a table whose shape is already settled, instead of all of them altering it. Prerequisites: none beyond the baseline. Downstream phases affected: 5, 6, 7, 8, 9, 12, 13.

**C. Required reference behaviour.** A default-layout window shows the composed report and, for each header/footer element — patient name, age/sex, referred by, patient id, request date, printed in — an enable check box and a **Left** and **Top** offset in centimetres. It also carries `Show top line`, `Show bottom line`, `Long date`, `Show time`, `Width` and `Height` for the top line, and per-test `Bold` check boxes in the history block. Reference guide, page 197; the same per-item font table is shown on pages 193 and 195 and the envelope counterpart on page 206.

**D. Current Top-Lab behaviour.** `src/TopLab.Domain/Settings/ReportItemKeys.cs` defines 24 keys, all body lines. `src/TopLab.Infrastructure/Persistence/Configurations/ReportItemTemplateConfiguration.cs:11-21` maps nine properties, of which only `LeftOffsetCm` is positional and none is a header element. `src/TopLab.Infrastructure/Printing/ReportDocumentContent.cs:8-30` carries one document-wide `HeaderFooterMode`, one `DrawHeader`, one `DrawFooter`. `src/TopLab.Infrastructure/Printing/ReportPageComposer.cs:77-118` and `:220-265` draw the header and footer from whole-document fields; `:58-75` applies marker and left offset to body lines only; `:291-320` positions one body line. Verified statically; the composer and the settings commands build and their tests pass.

**E. Exact behaviour delta.** The reference addresses header/footer elements individually and positions them in two axes. The current model addresses them as a single mode plus three whole-document fields and positions them not at all. Required changes: introduce a persisted, per-element record for the header and the footer carrying enable, left offset and top offset; introduce settings for `ShowTopLine`, `ShowBottomLine`, `UseLongDate`, `ShowTime`, `TopLineWidth` and `TopLineHeight`; consume them in the composer so that a disabled element is omitted and an enabled element is placed at its offset. Regression risk: an existing installation that relies on the current always-on header must keep the same output when no per-element rows exist — therefore the lookup must fall back to "all elements enabled at offset zero" when no rows are present.

**F. Exact implementation changes by layer.**

* **Domain** — NEW `src/TopLab.Domain/Settings/ReportHeaderElement.cs`: an entity keyed by `(HeaderFooterZone, ElementName)` carrying `IsEnabled`, `LeftOffsetCm`, `TopOffsetCm`, with bounded setters (0–30 cm, mirroring `EnvelopePrintItemPosition.cs:26-41`). Extend `ReportSettings.cs` with the six top-line options and their setters.
* **Application** — NEW `Features/SystemAndPrintSettings/Commands/UpdateReportHeaderElements` and `Queries/GetReportHeaderElements`, following the existing `UpdateEnvelopeSettings` / `GetEnvelopeSettings` shape; extend `UpdateReportSettingsCommand` / `GetReportSettingsQuery` / `SettingsDtos` with the six options.
* **Infrastructure** — NEW configuration for `ReportHeaderElement`; extend `ReportSettingsConfiguration.cs:17-35` with the six columns and the seeded defaults; the composer consumes the new DTO.
* **Presentation** — NEW `Views/Settings/ReportHeaderElementsView.xaml` plus its view model, registered in `src/TopLab.Presentation/DependencyInjection.cs` and opened from `ViewModels/Settings/SettingsDashboardViewModel.cs`.
* **Tests** — domain tests for the bounded setters; infrastructure tests for the new columns; printing tests asserting that a disabled element is absent from `ReportDocumentContent` and that offsets reach `ReportItemLayout`.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddReportHeaderElements`. Additive: one new table `ReportHeaderElements` (`HeaderFooterZone` `nvarchar(20)` not null, `ElementName` `nvarchar(50)` not null, composite primary key over both, `IsEnabled` `bit` not null default 1, `LeftOffsetCm` `decimal(5,2)` not null default 0, `TopOffsetCm` `decimal(5,2)` not null default 0); six nullable columns on `ReportSettings` (`ShowTopLine` `bit`, `ShowBottomLine` `bit`, `UseLongDate` `bit`, `ShowTime` `bit`, `TopLineWidth` `decimal(6,2)`, `TopLineHeight` `decimal(6,2)`) plus a `HasData` update for row `Id = 1`. No back-fill is needed beyond the seed because the reference's defaults are "enabled at offset zero", which is what the absence of rows already produces; the six new settings default to the values already hard-coded in `ReportSettings.CreateDefault()` (`ReportSettings.cs:58-81`). Evidence the state is not already persisted: `ReportSettingsConfiguration.cs:17-35` maps no such columns and no `ReportHeaderElement` type exists. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference names six elements explicitly (patient name, age/sex, referred by, patient id, request date, printed in) and six top-line options. Use exactly those names as the persisted `ElementName` values so the reference mapping is unambiguous. No reference message text governs an out-of-range offset; reuse the existing `ArgumentOutOfRangeException`-to-`Error` translation used by `EnvelopePrintItemPosition` so no new string is introduced.

**I. Objective acceptance criteria.**

1. Given a header element is disabled, when any report is composed, then that element appears in no header line of `ReportDocumentContent`.
2. Given a header element is enabled with left 1.5 cm and top 2 cm, when the report is composed, then the corresponding layout carries both offsets and the rendered page places the element accordingly.
3. Given no header-element rows exist, when a report is composed, then every element is emitted at offset zero and the output is byte-for-byte the layout the phase found.
4. Given `ShowTopLine` is false, when the report is composed, then the header rule line is omitted; given it is true, the line is emitted with the configured width and height.
5. Given `ShowTime` is false, when a date is rendered, then the time component is omitted; given `UseLongDate` is true, the long form is used.
6. Given an offset outside 0–30 cm, when it is saved, then the save is refused with the existing out-of-range conflict and no row is written.
7. Given the administrator saves the element grid, when it is reloaded, then every enabled flag and offset is returned unchanged.

**J. Test plan.** Domain tests for the bounded setters and the default-constructor state. Application tests for the update and query commands, including the refusal case. Infrastructure tests for the column types and the composite key. Printing tests asserting the four composition cases. A migration test asserting the new table and columns and the untouched seed. No Windows-runtime dependency; rendering assertions are made against the content model, not against produced pixels — a rendering check is listed in K.

**K. Phase-end verification gate.** Build the whole solution; run the whole test suite; apply the migration to a disposable empty database and confirm it; confirm existing migrations are unchanged; confirm all acceptance criteria; **generate one report on a Windows workstation with an Arabic-capable font and confirm the header element positions, the top line, the long-date and show-time switches visually** — this is the one phase in which a purely content-level assertion is not sufficient; inspect the changed file set for excluded capability; record commands and outputs.

**L. Risks and rollback.** Risk: the header is drawn by code shared with every report shape, so a mistake misprints every document; mitigate by asserting `BuildDisplayLines()` equality for the no-rows case. Risk: RTL placement with a negative or positive offset may place an element off the page; the 0–30 cm bound alone does not prevent that, so the composer must clamp to the page box. Rollback: revert the changes made by the phase and restore the affected files to their prior state; the appended migration is dropped so a fresh database reproduces the pre-phase schema.

**M. Phase status.** **FINAL.**

---

### PHASE 3 — Configurable barcode sticker size

**A. Phase identification.** Phase 3. Gap **GAP-003**, class **MISSING**. Size **low**. Risk **low**. Size is low: two settings columns and one pass-through. Risk is low: the label path is currently a constant and the change is bounded to two validation ranges.

**B. Dependency position.** Third because Phase 13 rewrites the label payload and Phase 12 adds a label orientation; both must reuse a configurable size rather than re-introduce a constant. Doing the size first means those phases never touch `SystemSettings` for label geometry again.

**C. Required reference behaviour.** "Sticker size 38 mm wide × 25 mm long". Reference showcase, page 3.

**D. Current Top-Lab behaviour.** `src/TopLab.Infrastructure/Barcode/BarcodeService.cs:69` calls `BarcodeLabelRenderer.Render(payload)` with its defaults; `:96-98` fixes the label page at 288 × 144 pt; `:104-113` fits the bitmap with fixed 12 pt margins and a fixed 20 pt text reserve. Term search `LabelWidth`, `LabelHeight`, `StickerSize` across `src` returned **0 matches**.

**E. Exact behaviour delta.** Reference: the operator chooses the stock size. Current: the stock size is a compile-time constant. Required change: expose width and height in millimetres, convert to points at print time, and remove the hard-coded page and margins in favour of values derived from the setting. Regression risk: the identity barcode is used by the invoice and the group-slip paths, so a bad default would change every label.

**F. Exact implementation changes by layer.** Domain: extend `src/TopLab.Domain/Settings/SystemSettings.cs` with `BarcodeLabelWidthMm`, `BarcodeLabelHeightMm` and a bounded setter (10–300 mm, 5–300 mm). Application: extend `UpdateSystemSettingsCommand`, its validator, its handler and `GetSystemSettings`. Infrastructure: extend `SystemSettingsConfiguration` with the two columns and the seeded defaults of 38 and 25; change `BarcodeService.BuildLabelPdf` to derive the page box and margins from the settings it already loads at `:53`. Presentation: extend `Views/Settings/SystemSettingsView.xaml` and its view model. Tests: domain, validator, configuration and a `BuildLabelPdf` test asserting the page box for a known setting.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddBarcodeLabelSizeSettings`. Additive: two nullable decimal columns on `SystemSettings` (`BarcodeLabelWidthMm` `decimal(6,2)`, `BarcodeLabelHeightMm` `decimal(6,2)`) with a `HasData` update for row `Id = 1` setting 38.00 and 25.00, which reproduces today's physical output exactly. Evidence the state is not already persisted: `SystemSettings.cs:9-34` has no such property and `BarcodeService.cs:96-98` is a constant. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference states 38 mm × 25 mm. Use those as the seeded defaults; do not invent a different stock. No reference refusal message for an out-of-range size; reuse the existing out-of-range validation message.

**I. Objective acceptance criteria.** 1. Given the default settings, when a label is produced, then the page box is 38 mm × 25 mm and the output matches the pre-phase layout. 2. Given a size of 50 × 30 mm is saved, when a label is produced, then the page box is 50 mm × 30 mm and the human-readable line remains below the barcode. 3. Given a width of 5 mm is submitted, then the save is refused. 4. Given no setting row has been seeded, then the service falls back to 38 × 25 rather than failing.

**J. Test plan.** Domain tests for the setter bounds; validator tests; a configuration test for the two column types; a rendering test over `BuildLabelPdf` asserting the media box for two settings values; a regression test asserting the seeded default equals the pre-phase constant. No printer or Windows dependency.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm all acceptance criteria; **print one label on a Windows workstation and confirm the physical sticker size and barcode legibility**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: a size too small to hold the barcode produces an unscannable label — mitigate by bounding the minimum at 10 mm wide. Risk: the seeded default must reproduce today's geometry exactly, or every existing installation's labels change. Rollback: revert the phase's changes and restore the affected files; drop the appended migration.

**M. Phase status.** **FINAL.**

---

### PHASE 4 — Sequential sample number on the report

**A. Phase identification.** Phase 4. Gap **GAP-004**, class **MISSING**. Size **medium**. Risk **medium**. Size is medium: a numbering source, a settings field and a report line. Risk is medium: a numbering sequence must be unique under concurrent registration.

**B. Dependency position.** Fourth because it adds a report content line and therefore touches `ReportDocumentContent` and the composer, which Phase 2 also touches; placing it after Phase 2 avoids two edits of the same content record in adjacent phases. It precedes Phase 8 because both affect the composed page, and it is independent of Phases 5–7. Prerequisite: Phase 2's element machinery, so the new line can be hidden like any other.

**C. Required reference behaviour.** The CBC report pages carry an auto-incrementing sample number and a separate sample identifier. Reference showcase, pages 56, 61, 62, 65.

**D. Current Top-Lab behaviour.** `src/TopLab.Infrastructure/Printing/ReportPageComposer.cs:134-138` emits the file/patient number and the patient name only. `src/TopLab.Infrastructure/Printing/ReportDocumentContent.cs:8-30` carries no sequence field. Term search `SampleNumber`, `SampleNo`, `SequenceNumber` across `src` returned **0 matches** outside migrations.

**E. Exact behaviour delta.** Reference: an automatically generated, monotonically increasing sample number is printed. Current: no such number exists. The reference does not state the numbering scope, so DEC-004 governs; the phase must not guess it. Regression risk: a sequence allocated per registration must be allocated inside the same transaction as the registration, or two concurrent registrations can collide.

**F. Exact implementation changes by layer.** Domain: NEW `src/TopLab.Domain/Results/SampleNumberSequence.cs` — a single-row counter with a scope key and a next value; the scope key's shape is decided by DEC-004. Application: NEW `Features/PatientRegistration/Commands/AllocateSampleNumber` invoked by the registration handler after the patient row is created; extend `ReportDocumentContent` consumers (`ResultPrintCoordinator`, the combined/history/profile/culture DTOs) to carry `SampleNumber`. Infrastructure: configuration for the new table; `ReportPageComposer` emits the line. Presentation: a read-only column on the result screen. Tests: concurrency test for the allocator; printing test for the line; registration test that the number is allocated exactly once.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddSampleNumberSequences`. Additive: one new table `SampleNumberSequences` (`ScopeKey` `nvarchar(50)` primary key, `NextValue` `bigint` not null default 1, `LastAllocatedDate` `date2` nullable) with one seeded row per configured scope; one nullable `SampleNumber` `bigint` column on `PatientTests`. No back-fill of historical rows — a null means "not allocated before this phase" and the composer omits the line for those rows. Evidence the state is not already persisted: no such column exists in `PatientTestConfiguration.cs` and no sequence type exists in the domain. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** No reference text governs the number's format. The format is DEC-004. Do not invent a prefix.

**I. Objective acceptance criteria.** 1. Given two patients registered in the same scope, when both are saved, then their sample numbers are distinct and increasing. 2. Given twenty concurrent registrations, then no two share a number. 3. Given a registration is rolled back, then its allocated number is not reused. 4. Given a report is printed for a patient with a number, then the number appears once; given a patient without one, the line is omitted. 5. Given the report-header element for the sample number is disabled in Phase 2's grid, then the line is omitted even when a number exists.

**J. Test plan.** Domain test for the counter transition. Application test for the allocator including a concurrency test. Printing test for the line's presence and absence. Migration test for the new table and column. No Windows dependency.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database and exercise concurrent registration there; confirm existing migrations unchanged; confirm the acceptance criteria; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: non-unique numbers under concurrency — mitigate with a database-side identity or an update-guarded increment, not an in-memory counter. Risk: a per-day scope that resets silently produces duplicates across a year — DEC-004 must settle this. Rollback: revert the phase and restore the affected files; the appended column and table are dropped on a fresh database.

**M. Phase status.** **NOT FINAL.** Blocked by **DEC-004** (numbering scope). The condition to finalise is a single owner answer fixing the scope — per day, per visit, or per accession — and the numeric format.

---

### PHASE 5 — Patient name printed in Arabic and English on the report

**A. Phase identification.** Phase 5. Gap **GAP-005**, class **MISSING**. Size **low**. Risk **low**. Two columns, a registration field and a report line; risk is low because the change is additive to a string field and to one composition line.

**B. Dependency position.** Fifth because it must land before Phase 4's report line if both are added to the same patient header block, and before Phase 9, which gives that block a per-line font. It is independent of Phase 2's element machinery except that the new field must be a hideable element, which Phase 2 provides.

**C. Required reference behaviour.** The default-layout preview shows `Patient Name` and, beside it, `Eng. Ahmed Ibrahem Saleh`; the CBC report prints the patient name in both scripts. Reference guide, page 197; reference showcase, page 60.

**D. Current Top-Lab behaviour.** `src/TopLab.Infrastructure/Printing/ReportPageComposer.cs:138` emits one string. `src/TopLab.Domain/Patients/Patient.cs` carries a single `FullName`. Term search `FullNameEn`, `EnglishName`, `NameEn`, `Transliterat` across `src` returned **0 matches**.

**E. Exact behaviour delta.** Reference: two names on one line. Current: one. Required changes: add an optional English name to the patient, expose it on the registration and edit screens, carry it on the report DTOs, and emit it on the same line. Regression risk: right-to-left composition of an Arabic run followed by a Latin run — the composer must keep both runs on one visual line and must not reverse the Latin run.

**F. Exact implementation changes by layer.** Domain: extend `Patient.cs` with `FullNameEn` and a validating setter (nullable, length bounded by the existing name column length). Application: extend `CreatePatient`, `UpdatePatient`, `GetPatientById`, `SearchPatients`, `SearchPatientsGlobal`, `GetVisitDetail`, `GetResultEntry` and the report DTOs. Infrastructure: `PatientConfiguration` gains the column; `ReportPageComposer` emits the pair. Presentation: `Views/Patients/PatientEditorView.xaml` and `ViewModels/Patients/PatientEditorViewModel.cs`. Tests: domain, application and printing tests including an RTL assertion that the Latin substring appears in the expected order in the emitted string.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddPatientEnglishName`. Additive: one nullable `nvarchar(200)` column on `Patients`. No back-fill: null means absent and the composer then emits exactly today's single-name line. Evidence the state is not already persisted: `Patient.cs` has no such property. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference labels the second field `Eng.`. Use that exact prefix for the rendered label. No other reference text governs it.

**I. Objective acceptance criteria.** 1. Given an English name is absent, when a report is composed, then the emitted line is identical to today's. 2. Given an English name is present, when a report is composed, then one line carries both names and the Latin run is not reversed. 3. Given a name longer than the column length, when saved, then the save is refused. 4. Given a patient registered before this phase, when their report is printed, then no error occurs and the line carries only the Arabic name.

**J. Test plan.** Domain test for the setter; application tests for create and update carrying the field; a printing test for both branches; a migration test for the column. No Windows dependency.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **print one report containing an Arabic and a Latin name on a Windows workstation and confirm the visual order**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: RTL/LTR mixed-run rendering — the reason for the visual check in K. Risk: search and sorting by name would need a second index if the English name becomes searchable; that is out of this phase's scope and must not be added implicitly. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **FINAL.**

---

### PHASE 6 — Colour versus black-and-white report printing

**A. Phase identification.** Phase 6. Gap **GAP-006**, class **MISSING**. Size **low**. Risk **medium**. One setting and one conditional, but the conditional touches every drawn element.

**B. Dependency position.** Sixth because it modifies page setup that Phase 7 and Phase 8 also change, so it goes before them and keeps the page-configuration code in one place. Independent of the label phases.

**C. Required reference behaviour.** Printing is offered both in colour and on plain or pre-printed white paper. Reference showcase, pages 59 and 61; the setting is part of the report settings screen shown on reference guide, page 193.

**D. Current Top-Lab behaviour.** `src/TopLab.Infrastructure/Printing/ReportPageComposer.cs:361` calls `page.PageColor(Colors.White)` unconditionally, and every other writer does the same. No output-mode switch exists. Term search `BlackAndWhite`, `Grayscale`, `ColourMode`, `ColorMode`, `Monochrome` across `src` returned **0 matches**.

**E. Exact behaviour delta.** Reference: the operator chooses colour or monochrome output. Current: colour-only, on a hard-coded white page. Required change: an output mode on `ReportSettings`, a monochrome rendering path in the composer, and — because a greyscale page alone does not make a coloured text run monochrome — a colour-to-grey conversion for every font and background colour the composer applies (`ReportPageComposer.cs:84, 104, 110, 222, 228, 255, 261, 298, 316`). Regression risk: if only the page colour is changed, a monochrome report still prints coloured text — the acceptance criteria forbid that.

**F. Exact implementation changes by layer.** Domain: extend `ReportSettings` with `ReportOutputMode` (a new enum in `src/TopLab.Domain/Common/Enums/`) and a setter. Application: extend the report settings command, validator, handler, query and DTO. Infrastructure: add the column and its conversion in `ReportSettingsConfiguration`; add a shared colour-downgrade helper next to the existing `ParseColor` in the printing namespace and call it from every colour-consuming site in the composer. Presentation: a radio pair on the report settings view. Tests: printing tests asserting that in monochrome mode every emitted colour is a grey and that colour mode leaves them untouched; configuration and validator tests.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddReportOutputMode`. Additive: one non-nullable `tinyint` column on `ReportSettings` with a `HasData` update for row `Id = 1` set to the colour value, reproducing today's behaviour exactly. Evidence the state is not already persisted: `ReportSettingsConfiguration.cs:17-35` has no such property. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** No reference message governs this switch. Reuse existing validation messages.

**I. Objective acceptance criteria.** 1. Given colour mode, when a report is composed and rendered, then every emitted colour is unchanged from today. 2. Given monochrome mode, when a report is composed, then no emitted font or background colour has a non-zero channel spread. 3. Given monochrome mode, when a colour setting is unset, then the renderer uses the same default as colour mode. 4. Given the mode is toggled and saved, when reloaded, then the stored value is returned unchanged. 5. Given a report is printed in monochrome, then the barcode image remains legible.

**J. Test plan.** Printing tests over `ReportDocumentContent` and the composer helpers; configuration test for the conversion; validator test. Migration test for the column and seed. No printer dependency, but K includes a physical print.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **print the same report in both modes on a Windows workstation and confirm visually that the monochrome output is entirely grey**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: partial downgrade — a colour site missed in the composer produces a half-coloured monochrome report; mitigate by centralising the conversion and by a test that asserts every emitted colour, not only the ones named in the acceptance criteria. Risk: a bitmap logo keeps its colour in monochrome mode; the reference does not say, so the safest behaviour is to keep the logo as-is and record it. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **FINAL.**

---

### PHASE 7 — A3 and A4 at 85 × 55 paper

**A. Phase identification.** Phase 7. Gap **GAP-007**, class **MISSING**. Size **low**. Risk **low**. An enum member, a mapper entry and a settings option; risk is low because the composer is already parameterised by page size.

**B. Dependency position.** Seventh because Phase 8's imposition maths is defined against a page size and must follow it rather than invent its own geometry. Independent of the patient and label phases.

**C. Required reference behaviour.** "Can be printed on large paper A3, or A4 at 85 × 55." Reference showcase, page 74. The paper dropdown on the report settings screen shows `A4 (Large)`. Reference guide, page 195.

**D. Current Top-Lab behaviour.** `src/TopLab.Domain/Common/Enums/PaperSize.cs:3-7` contains `A4 = 0` and `A5 = 1`. `ReportSettingsConfiguration.cs:17` persists the enum as an int-converted `tinyint`. `PageSizeMapper` maps the two existing values. Term search `A3` across `src` returned **0 matches**.

**E. Exact behaviour delta.** Reference: two further stocks — A3, and A4 at 85 × 55. Current: two stocks. Required change: add the members, map them to exact millimetre dimensions, seed the default as `A4` so nothing changes, and expose them in the settings combo. Regression risk: none for existing rows; a non-default stock that the composer lays out badly is a rendering question handled in K.

**F. Exact implementation changes by layer.** Domain: extend `PaperSize.cs` with `A3 = 2` and `A4_85x55 = 3`. Infrastructure: extend `src/TopLab.Infrastructure/Printing/PageSizeMapper.cs` with the two exact dimensions. Application: the settings validator must reject an undefined enum value. Presentation: add the two entries to the paper combo on the report settings view. Tests: mapper tests asserting the exact millimetre dimensions; a validator test for an out-of-range value.

**G. Migration decision.** **NO MIGRATION**, conditionally. The persisted column is already an int conversion of the enum (`ReportSettingsConfiguration.cs:17`), so adding enum members changes no table and no column. The coding agent must confirm this by running the EF pending-model-change check against the disposable database before deciding. If that check reports a change, this decision becomes **NEW MIGRATION** and Section 5 must be updated before the phase is accepted. The decision is recorded as conditional rather than guessed, and the gate in K enforces it.

**H. Business rules and message text.** Use the reference's own stock names `A3` and `85×55`. No reference message governs an invalid selection.

**I. Objective acceptance criteria.** 1. Given A3 is selected, when a report is rendered, then the page box equals A3 in millimetres. 2. Given the 85 × 55 stock is selected, the page box is 210 mm × 55 mm. 3. Given the default, the page box is unchanged from today. 4. Given an undefined enum value is submitted, the save is refused. 5. Given the setting is persisted and reloaded, the same member is returned.

**J. Test plan.** Mapper unit tests; validator test; a configuration test asserting the persisted representation. No migration test unless the pending-change check reports a change. No Windows dependency, but K includes a physical print.

**K. Phase-end verification gate.** Build; run the whole suite; run the EF pending-model-change check against the disposable database and record the result; confirm the migration decision from that result; confirm existing migrations unchanged; confirm the acceptance criteria; **print one report on A3 and one on the 85 × 55 stock on a Windows workstation and confirm the content is neither clipped nor split**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: content that fits A4 may overflow the 55 mm band — the acceptance criteria in K are the mitigation. Risk: an empty migration would be noise in the history, which is why the decision is gated on an executed check. Rollback: revert the phase and restore the affected files.

**M. Phase status.** **FINAL.**

---

### PHASE 8 — Folded and multi-port report printing

**A. Phase identification.** Phase 8. Gap **GAP-008**, class **MISSING**. Size **medium**. Risk **medium**. The imposition maths is contained but affects every page.

**B. Dependency position.** Eighth because it consumes Phase 7's page geometry. It must not precede it.

**C. Required reference behaviour.** "3-port and 5-port folded output" is one of the offered report layouts. Reference showcase, page 64.

**D. Current Top-Lab behaviour.** No folding, port or imposition construct exists; term search `Folded`, `Fold`, `Port` across `src` returned **0 matches**. `ReportPageComposer` emits a single flat page per report page.

**E. Exact behaviour delta.** Reference: a report can be imposed so that one physical sheet folds into three or five panels. Current: one panel per page. Required changes: a layout setting, a panel-splitting calculation derived from the page box, a fold guide drawn on the panel seams, and content clipped to its panel. Regression risk: content that does not fit a panel must be clipped deterministically and the clipping must be visible in a test, otherwise a long report silently loses rows.

**F. Exact implementation changes by layer.** Domain: NEW `src/TopLab.Domain/Settings/ReportFoldLayout.cs` with the enum and the panel-split calculation. Application: extend the report settings command and DTO. Infrastructure: apply the split in the composer after the content is composed, and draw the guide. Presentation: the settings control and a fold preview. Tests: unit tests for the split calculation at A4 and A3; a printing test asserting panel clipping and guide presence.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddReportFoldLayout`. Additive: one non-nullable `tinyint` column on `ReportSettings` (`ReportFoldLayout`, default `0` = single panel), which reproduces today's flat output exactly. Evidence the state is not already persisted: no such property exists. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference names two folded layouts: 3-port and 5-port. Use exactly those names. No reference message governs an invalid combination.

**I. Objective acceptance criteria.** 1. Given the single-panel layout, when a report is rendered, the output is identical to today's. 2. Given the 3-port layout on A4, the page is divided into three equal panels and a fold guide is drawn on both seams. 3. Given content exceeding a panel, it is clipped at the panel boundary and never spills into the neighbouring panel. 4. Given a 5-port layout on the 55 mm stock, the calculation is rejected as not representable rather than producing a degenerate page. 5. Given the layout is changed and saved, it is returned unchanged on reload.

**J. Test plan.** Domain unit tests for the split calculation; printing tests for guide presence and clipping; validator tests; a migration test for the column and default. No printer dependency; K includes a physical fold check.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **print a folded report on a Windows workstation, fold it, and confirm the panels read in the correct order**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: silent content loss at panel edges — mitigated by making clipping explicit and tested. Risk: a folded report combined with a Phase 9 per-line font could overflow a panel; the two phases are separately verifiable, which is why they are not combined. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **FINAL.**

---

### PHASE 9 — Font family and size per report line

**A. Phase identification.** Phase 9. Gap **GAP-009**, class **MISSING**. Size **medium**. Risk **medium**. Size is medium: two attributes on an existing entity plus composer plumbing and a settings grid. Risk is medium: a per-line font changes pagination, so a font larger than the page can abort PDF generation — the same class of failure already guarded for logos.

**B. Dependency position.** Ninth because it modifies `ReportItemTemplates` and `ReportPageComposer`, both of which Phase 10 also extends. Running it first means Phase 10 only adds a scope column to a table whose columns are settled. Phase 2 has already moved header/footer elements to their own table, so this phase covers body lines and the doctor-signature block.

**C. Required reference behaviour.** The header/footer item table gives each line its own **Font** selector, with values shown as 0, 0.5, 1, 1.2, 1.5, 1.8, 2, 3, 9 and 9.5. The doctor's-signature block gives each of its three lines its own **Font** button. Reference guide, pages 193 and 195.

**D. Current Top-Lab behaviour.** `src/TopLab.Domain/Settings/ReportItemTemplate.cs` carries `LineSpacing`, `LeftOffsetCm` and `MarkerSymbol` and no font attribute. `ReportItemTemplateConfiguration.cs:11-21` maps nine properties. `ReportPageComposer.cs:319` applies one document-wide `fontSize`. Term search `LineFontSize`, `ReportLineFont`, `ItemFontSize`, `ItemColor` across `src` returned **0 matches**; the only font attributes in the codebase are `ReceiptSettings.HeaderFontFamily` / `HeaderFontSizePt` / `FooterFontFamily` / `FooterFontSizePt` and the single `LabPrintTextDto.FontFamily` / `FontSizePt`.

**E. Exact behaviour delta.** Reference: each report line has its own font selection. Current: one font for the whole document. Required changes: add a nullable font family and a nullable font size to the item template; resolve them per line in the composer, falling back to the document font when unset; expose them in the template grid. The doctor-signature block, which today reuses `ReportSettings.DoctorSignatureEnabled`, must gain per-line fonts through the same mechanism. Regression risk: a line whose font size exceeds the page height aborts PDF generation — the composer must bound the size and fall back to the document font when the resulting line would not fit, exactly as `PageSizeMapper.FitHeaderFooterImage` bounds an image.

**F. Exact implementation changes by layer.** Domain: extend `ReportItemTemplate` with `FontFamily` and `FontSizePt` plus bounded setters, following the existing `MaxMarkerSymbolLength` pattern. Application: extend `UpdateReportItemTemplatesCommand`, its validator, `GetReportItemTemplatesQuery` and the template DTOs; extend `ViewModels/Settings/ReportItemTemplatesViewModel.cs`. Infrastructure: extend `ReportItemTemplateConfiguration`; extend `ReportContentLine` and the composer; add the fit check. Presentation: extend `Views/Settings/ReportItemTemplatesView.xaml` and the doctor-signature block on the report settings view. Tests: domain bounds tests; validator tests; printing tests for the fallback and the override; a fit-rejection test.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddReportItemFonts`. Additive: two nullable columns on `ReportItemTemplates` (`FontFamily` `nvarchar(100)`, `FontSizePt` `decimal(5,2)`), both null for the 24 seeded rows so nothing changes until an operator edits a row. Evidence the state is not already persisted: `ReportItemTemplateConfiguration.cs:18-20` maps only spacing, offset and marker. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference exposes a numeric font scale, not named families; expose both and default the family to the document font. No reference message governs an out-of-range size.

**I. Objective acceptance criteria.** 1. Given all 24 seeded rows have null fonts, when a report is rendered, the output is identical to today's. 2. Given one row has a font family and size, when the report is rendered, only that line's font changes. 3. Given a size that would make the line taller than the page, when the report is rendered, the line falls back to the document font rather than aborting. 4. Given a row's font is cleared, when the report is rendered, the line returns to the document font. 5. Given the grid is saved and reloaded, every family and size is returned unchanged.

**J. Test plan.** Domain bounds tests; validator tests; printing tests for override, fallback and fit rejection; configuration tests for the column types; a migration test. No printer dependency; K includes a physical print.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **print one report with three different per-line fonts on a Windows workstation and confirm each line renders in its own font and that pagination is unchanged**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: pagination changes — mitigated by keeping the default null and by asserting the display-line sequence is unchanged in the default case. Risk: a font family that cannot shape Arabic silently substitutes; mitigate by resolving through the existing `ArabicFontResolver` and by treating an unresolved family as "use the document font". Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **FINAL**, subject to **DEC-005** (whether the reference's per-line Font control also governs the header/footer item table that Phase 2 moved to its own entity). If the answer is "yes" this phase must also extend `ReportHeaderElement` and its migration. The coding agent must read DEC-005 before starting.

---

### PHASE 10 — Per-user (doctor) report element toggle

**A. Phase identification.** Phase 10. Gap **GAP-010**, class **MISSING**. Size **medium**. Risk **medium**. The change is one nullable scope column and a lookup precedence rule; risk is medium because it alters the primary key of an existing table with 24 seeded rows.

**B. Dependency position.** Tenth, immediately after Phase 9, because it adds a column to the same table Phase 9 extends. Doing it after Phase 9 means one migration carries both changes and the composite-key alteration happens once, against a settled column set. It must not be moved earlier.

**C. Required reference behaviour.** A side menu by which a doctor shows or hides report elements — a **per-user** preference, distinct from the global "show this line" checkbox in the report settings. Reference showcase, page 15.

**D. Current Top-Lab behaviour.** `src/TopLab.Infrastructure/Persistence/Configurations/ReportItemTemplateConfiguration.cs:11` declares `HasKey(e => e.ItemKey)` — one global key, no user column. `ReportSettings` is a single row. Term search `UserReportSettings`, `ReportPreference`, `UserLayout` across `src` returned **0 matches**.

**E. Exact behaviour delta.** Reference: element visibility can differ per user. Current: visibility is global. Required changes: add a nullable scope column, promote the key to include it, seed only the global rows, and resolve the effective template as "the user's row if one exists, otherwise the global row". The precedence rule must be written down in code and pinned by a test, because getting it wrong silently shows one doctor another doctor's layout.

**F. Exact implementation changes by layer.** Domain: extend `ReportItemTemplate` with a nullable user reference and adjust the identity used by the entity's factory; expose the resolution rule as a small domain service. Application: extend the update and query commands so the caller can supply either a global update or a user-scoped update; enforce that a non-administrator may only write their own scope, reusing `AuthorizationBehavior`. Infrastructure: configuration, key change, and the lookup in `ReportContentBuilder.BuildTemplateMap` (`ReportContentBuilder.cs:675-680`). Presentation: a per-user grid and a route to reach it. Tests: domain resolution tests, application authorization tests, and printing tests proving that a user override wins and that the absence of an override falls back.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddUserScopedReportItemTemplates`. This is the single highest-migration-risk item in the plan. Additive plus one key alteration: a nullable `int` foreign-key column `UserId` on `ReportItemTemplates`, and the primary key changed from `ItemKey` to `(ItemKey, UserId)`. Because the column is nullable, the safe shape — adding the column as non-null with a `0` sentinel meaning "global", then making it part of the key, versus keeping it nullable in the key — depends on the deployed engine and is to be decided at implementation time against a disposable database, not guessed here. No existing migration may be edited; the change is appended after `20261018144852_AddProfileResultStyling` and after Phase 9's migration. Back-fill: existing rows keep the global meaning and existing reports are unaffected. Evidence the state is not already persisted: `ReportItemTemplateConfiguration.cs:11-20` maps a single `ItemKey` and no user column.

**H. Business rules and message text.** The reference gives no message text. Use the existing authorization-failure message; do not introduce a new one.

**I. Objective acceptance criteria.** 1. Given no user-scoped rows exist, when any report is rendered, the output is identical to today. 2. Given a user-scoped row for the result-value line exists, when that user's report is rendered, then that line follows the user's row while every other line follows the global row. 3. Given a user-scoped row is deleted, when the report is rendered, the global row governs again. 4. Given a non-administrator submits a user-scoped update for another user, the request is refused. 5. Given the migration has been applied to a database that already holds the 24 seeded rows, then all 24 rows survive with global scope and the count is unchanged. 6. Given two users each override the same element, when each renders a report, each sees their own value.

**J. Test plan.** Domain tests for the resolution precedence; application tests for authorization and persistence; printing tests for override, fallback and independence between users; a configuration test for the composite key; a migration test asserting the key alteration and the row count. This is the one phase whose migration must additionally be executed against a disposable database already populated by every pre-phase migration.

**K. Phase-end verification gate.** Build; run the whole suite; apply every existing migration and then this one to a disposable database and confirm the row count and the key; confirm existing migrations unchanged; confirm the acceptance criteria; **on a Windows workstation, override one element for one user and confirm only that user's report changes**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: the key alteration loses rows — mitigate by asserting the row count after the migration on a populated database. Risk: precedence ambiguity between a global row and a user row — mitigate by a single documented rule and a test for every branch. Risk: a user row left behind after a user is deleted — the foreign-key behaviour is decided at implementation time and pinned by a test. Rollback: revert the phase and restore the affected files; the safest rollback is before any real database has been migrated.

**M. Phase status.** **FINAL**, subject to **DEC-005** only in the sense that if the answer extends header/footer elements into the same table, the key alteration must cover both tables and the phase grows. The coding agent must read DEC-005 first.

---

### PHASE 11 — Font size per envelope item

**A. Phase identification.** Phase 11. Gap **GAP-011**, class **MISSING**. Size **low**. Risk **medium**. Two columns and a per-item lookup; risk is medium because the envelope has its own writer and its own font resolution, and a font that cannot shape Arabic silently substitutes.

**B. Dependency position.** Eleventh, after the report-template phases, because the reference presents the report-line font control and the envelope-item font control on adjacent screens and an agent working both should not interleave two migrations on two different tables in adjacent phases. The envelope side of the element-position work is already complete, so this phase is confined to styling.

**C. Required reference behaviour.** The envelope window's item table gives each envelope item its own **Font** selector. Reference guide, page 206.

**D. Current Top-Lab behaviour.** `src/TopLab.Domain/Settings/EnvelopePrintItemPosition.cs:6-41` carries exactly `ItemName`, `IsEnabled`, `LeftOffsetCm`, `TopOffsetCm`; its constructor and `Update` accept only those four. `src/TopLab.Infrastructure/Printing/EnvelopePdfWriter.cs:70-72` resolves one document-wide font family and size. Term search for an envelope font attribute: **0 matches**.

**E. Exact behaviour delta.** Reference: each envelope item has its own font selection. Current: one font for the whole envelope. Required changes: add a nullable font family and font size to the position entity, resolve them per item in the envelope writer, and expose them on the envelope settings grid. Regression risk: an unresolved font family silently substitutes; the acceptance criteria require an explicit fallback.

**F. Exact implementation changes by layer.** Domain: extend `EnvelopePrintItemPosition` with `FontFamily` and `FontSizePt` and bounded setters, keeping the existing 0–30 cm bounds untouched. Application: extend `UpdateEnvelopeSettingsCommand`, its validator and `GetEnvelopeSettingsQuery`. Infrastructure: extend `EnvelopePrintItemPositionConfiguration`; extend `EnvelopePdfWriter.BuildTextLines` and its emission loop. Presentation: extend `Views/Settings/EnvelopeSettingsView.xaml` and its view model. Tests: domain bounds tests; validator tests; printing tests for override, fallback and per-item independence.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddEnvelopeItemFonts`. Additive: two nullable columns on `EnvelopePrintItemPositions` (`FontFamily` `nvarchar(100)`, `FontSizePt` `decimal(5,2)`), both null for existing rows so the envelope is unchanged until an operator edits it. Evidence the state is not already persisted: `EnvelopePrintItemPositionConfiguration` maps exactly the four properties listed above. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference's Font column shows a numeric scale, as on the report side. Expose both a numeric size and an optional family. No reference message governs an out-of-range value.

**I. Objective acceptance criteria.** 1. Given every row has null fonts, when an envelope is written, the output is identical to today's. 2. Given one row sets a size, only that item's font changes. 3. Given a family that cannot be resolved, that item falls back to the document font rather than failing. 4. Given an out-of-range size is submitted, the save is refused. 5. Given the grid is saved and reloaded, all values are returned unchanged.

**J. Test plan.** Domain bounds tests; validator tests; printing tests for override, fallback and independence between items; configuration tests for the column types; a migration test. No printer dependency; K includes a physical print.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **print one envelope with two items in different fonts on a Windows workstation and confirm both shapes correctly**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: an Arabic item rendered in a Latin-only font produces unshaped glyphs — mitigated by the fallback rule and the visual check. Risk: a large size pushing an item off the envelope — mitigated by bounding the size to the envelope's usable height, decided at implementation time and pinned by a test. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **FINAL.**

---

### PHASE 12 — Optional vertical coding on receipt, envelope and labels

**A. Phase identification.** Phase 12. Gap **GAP-012**, class **MISSING**. Size **low**. Risk **medium**. One setting and a rotation path on three documents; risk is medium because rotation interacts with page geometry and with barcode legibility.

**B. Dependency position.** Twelfth, after Phase 3 established a configurable sticker size and after Phase 11 completed the envelope styling. It must precede Phase 13 because Phase 13 emits several labels per patient and the orientation must be a property of the label, not hard-coded per call site.

**C. Required reference behaviour.** "Printing the barcode on the receipt and the envelope to ease handling"; the barcode-label page adds vertical coding for handing results over and for reprints, and the envelope window carries a control for the vertical barcode's contents. Reference showcase, pages 3 and 52; reference guide, page 206.

**D. Current Top-Lab behaviour.** No rotation or vertical-text mode exists; term search `VerticalCode`, `VerticalText` across `src` returned **0 matches**. `BarcodeService.cs:69` renders a horizontal label. `ReceiptPdfWriter.cs:146` and `EnvelopePdfWriter.cs:107` both call `page.PageColor(Colors.White)` on a flat page and place the barcode horizontally.

**E. Exact behaviour delta.** Reference: the operator can choose a vertical orientation for the code printed on the receipt, the envelope and the labels. Current: horizontal only, everywhere. Required changes: an orientation setting, a rotated placement path in each of the three writers, and a rotation that does not clip the code. Regression risk: a rotated label placed with the fixed 12 pt margins currently used in `BarcodeService.BuildLabelPdf` will not fit; the placement maths must swap width and height when rotated.

**F. Exact implementation changes by layer.** Domain: NEW enum `BarcodeOrientation` in `src/TopLab.Domain/Common/Enums/`; a property on `SystemSettings` with a setter. Application: extend the system settings command, validator, handler, query and DTO; expose the value to the three printing services. Infrastructure: `BarcodeLabelRenderer` and `BarcodeService` gain a rotated layout path; `ReceiptPdfWriter` and `EnvelopePdfWriter` place the code rotated. Presentation: a check box on the system settings view. Tests: renderer tests asserting the rotated bitmap dimensions and placement for both orientations; printing tests for receipt and envelope placement.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddBarcodeOrientation`. Additive: one non-nullable `tinyint` column on `SystemSettings` with a `HasData` update for row `Id = 1` set to horizontal, reproducing today's output exactly. Evidence the state is not already persisted: `SystemSettings.cs:9-34` has no such property. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference describes vertical coding without a message. Use a plain setting label; reuse existing validation messages.

**I. Objective acceptance criteria.** 1. Given horizontal orientation, the receipt, envelope and label output is identical to today's. 2. Given vertical orientation, each of the three places the code rotated ninety degrees, fully inside the printable area. 3. Given vertical orientation, the human-readable line beneath the code remains horizontal and legible. 4. Given vertical orientation and a sticker size whose long edge is shorter than the rotated code's length, the request is refused rather than silently scaling the code below the scanner's minimum. 5. Given the setting is saved and reloaded, it is returned unchanged.

**J. Test plan.** Renderer tests for both orientations; printing tests for placement inside the page box; validator tests; a migration test for the column and seed. No scanner hardware; K includes a physical print.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **print a receipt, an envelope and a label in vertical orientation on a Windows workstation, scan them, and confirm each is read correctly**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: a rotated code that is scaled below the scanner minimum becomes unreadable — acceptance criterion 4 is the mitigation. Risk: rotation breaks the Arabic human-readable line — that line must stay horizontal, which acceptance criterion 3 pins. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **FINAL.**

---

### PHASE 13 — Barcode labels per sample material with a doctor-editable distribution

**A. Phase identification.** Phase 13. Gap **GAP-013**, class **MISSING**. Size **high**. Risk **high**. Size is high: two new tables, an assignment editor, and a multi-label payload replacing a single-identifier call. Risk is high: the single-identifier path is load-bearing for the invoice and the group-slip barcodes, which reuse the same renderer and the same service.

**B. Dependency position.** Thirteenth, after Phase 3 (size) and Phase 12 (orientation) so that the label pipeline is complete before its payload changes. This is the last label phase; nothing else depends on it.

**C. Required reference behaviour.** Seven labels for one patient, one per drawn material — serum fasting, serum postprandial, EDTA blood, citrated blood, urine, semen, stool — each listing the tests assigned to that material, with the ability for the doctor to change the distribution; the file barcode can be pasted onto a chosen label; a label can be re-printed more than once. Reference showcase, pages 3 and 52.

**D. Current Top-Lab behaviour.** `src/TopLab.Infrastructure/Barcode/BarcodeService.cs:44` — `PrintBarcodeAsync(string value)` — accepts exactly one identifier, and its only caller passes the patient id or the file number. There is no sample-material entity, command, DTO, view or view-model. Term search `SampleMaterial`, `ContainerLabel`, `SwabLabel`, `TubeLabel` across `src` returned **0 matches**. `Test.Collection` carries a free-text sample name but is not a master record and has no assignment.

**E. Exact behaviour delta.**

| | Reference | Current |
|---|---|---|
| Number of labels per patient | one per drawn material | one identity label |
| Assignment of tests to a material | shown on each label, editable by the doctor | no assignment exists |
| Default distribution | the system sorts tests across the drawn materials | n/a |
| Reprint | any label may be re-printed | the identity label may be re-printed |
| File barcode placement | can be pasted onto a chosen label | printed only on the identity label and on the invoice |

Changes required: a sample-material master, a per-test assignment, an editor that sorts tests across the materials and lets the doctor move them, a print command that emits one label per non-empty material, and the existing identity label preserved unchanged. Regression risk is the highest in the plan: the invoice and group-slip paths call the same service and must keep working exactly as before — this is why the phase must not change the existing single-identifier method signature, but add a new one.

**F. Exact implementation changes by layer.**

* **Domain** — NEW `src/TopLab.Domain/Tests/SampleMaterial.cs` (name, sort order, active flag, optional default barcode prefix). NEW `src/TopLab.Domain/Tests/PatientTestSampleMaterial.cs` (the assignment). The seven reference materials are seeded, not hard-coded in logic.
* **Application** — NEW `Features/PatientRegistration/Commands/AssignSampleMaterials` (bulk assignment with the doctor's overrides), `Features/PatientRegistration/Queries/GetSampleMaterialLabels` (one DTO per non-empty material with its tests), and `Features/PatientRegistration/Commands/PrintSampleMaterialLabels`. Extend `AddTestsToVisitCommandHandler` and `AddProfileToVisitCommandHandler` to create the default assignment when a test is added.
* **Infrastructure** — configurations for both new entities; seed the seven materials; add a new `PrintSampleMaterialLabelsAsync` to `IBarcodeService` and its implementation, leaving `PrintBarcodeAsync` untouched.
* **Presentation** — a materials editor on the patient editor screen and a print action on the sample collection view; register the view models.
* **Tests** — domain assignment tests; application tests for default sorting and for a doctor override; infrastructure tests for both tables; printing tests asserting one label per non-empty material and that the identity label is unchanged.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddSampleMaterialLabels`. Additive: a new table `SampleMaterials` (`Id` identity, `Name` `nvarchar(100)` not null unique, `SortOrder` `int` not null, `IsActive` `bit` not null) seeded with the seven reference materials; a new table `PatientTestSampleMaterials` (`PatientTestId` `int` not null, `SampleMaterialId` `int` not null, composite primary key over both, foreign keys to `PatientTests` and `SampleMaterials`). Back-fill: assigning every historical `PatientTest` to a default material is **not** attempted — historical visits have no assignment, so their material labels are simply not offered, and this must be stated on the screen rather than guessed at print time. Evidence the state is not already persisted: no such table, column or type exists. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** Use the reference's own material names exactly: serum fasting, serum postprandial, EDTA blood, citrated blood, urine, semen, stool. The reference names no error message; reuse the existing "no tests assigned" validation message for an empty selection.

**I. Objective acceptance criteria.** 1. Given a patient with tests across three distinct `Test.Collection` values, when the material labels are requested, three labels are returned and every test appears on exactly one of them. 2. Given the doctor moves a test to another material, when the labels are re-requested, the test follows the doctor. 3. Given a patient registered before this phase, when material labels are requested, an empty result is returned and the screen says so rather than guessing a distribution. 4. Given a material is printed, the barcode payload is the file number and the label's size and orientation come from the Phase 3 and Phase 12 settings. 5. Given the existing identity-label action is invoked, the output is byte-identical to the pre-phase output. 6. Given two labels are printed for one patient, two distinct PDFs are produced and dispatched to the barcode printer. 7. Given a material has no tests, no label is produced for it.

**J. Test plan.** Domain tests for assignment invariants; application tests for sorting, override and the empty-visit case; infrastructure tests for both configurations and the seed; a service test asserting the identity-label path is unchanged; migration test for both tables and the seed. No printer dependency; K includes a physical print.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database and exercise a seeded visit there; confirm existing migrations unchanged; confirm the acceptance criteria; **print a real set of material labels on a Windows workstation and scan each one**; inspect the changed file set and confirm the invoice and group-slip barcode output is unchanged; record commands and outputs.

**L. Risks and rollback.** Risk: a change to the shared renderer would alter the invoice and group-slip barcodes — mitigated by the acceptance criterion that pins the identity path to its pre-phase output. Risk: a distribution guessed for historical visits would be clinically wrong — mitigated by returning nothing and saying so. Risk: a default assignment created on add-test could change the worksheet output if the worksheet starts filtering by material — it must not, and that is a regression check in K. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **FINAL**, provided the reference's own material list is used verbatim and no historical distribution is invented.

---

### PHASE 14 — Picture report for a sample (one to four images)

**A. Phase identification.** Phase 14. Gap **GAP-014**, class **MISSING**. Size **high**. Risk **high**. Size is high: image storage, an editor with drag positioning, and image composition inside the PDF pipeline. Risk is high: images embedded in a report can exceed the page box and abort generation, and binary storage has a data-integrity dimension no other phase in this plan has.

**B. Dependency position.** Fourteenth. It is the only phase that embeds raster images in a report, so it must follow the page-geometry phases (7, 8) and the per-line font phase (9), because image placement depends on both. Nothing later depends on it.

**C. Required reference behaviour.** "A picture report can be made for the following tests — complete blood count, culture, semen, stool and urine. From one image to four images can be placed in the report; the image place and size can be changed; texts and shapes can be moved by dragging; the colour, size and type of the font used in the elements can be chosen; shapes, numbers and words can be placed on and off the images, and their places, dimensions and colours controlled." A second caption states the picture report can also be produced on a separate sheet from inside five of these tests, with shapes, numbers and words on and off the images. Reference showcase, page 24.

**D. Current Top-Lab behaviour.** No image, attachment, microscopy-photo or picture-report entity, command, query, DTO, view or view-model exists anywhere in `src`. `src/TopLab.Domain/Results/CultureMicroscopy.cs` holds only text-valued microscopic findings. `ReportPageComposer.ComposeItemLine` (`ReportPageComposer.cs:280-320`) emits text and tables only. Term search across `src` for a stored image attached to a result: **0 matches**.

**E. Exact behaviour delta.** Reference: one to four images per report, with position, size and overlaid annotations under operator control. Current: no images at all. Required changes: an image record attached to a patient test, an editor that captures or imports up to four images and lets the operator place, size, annotate and order them, a print path that embeds them, and — because the reference also allows a separate sheet — a layout option. The reference does not state the capture modality, the storage location, the file formats, the maximum pixel dimensions or the retention policy; those are DEC-006 and the phase must not invent them.

**F. Exact implementation changes by layer.**

* **Domain** — NEW `src/TopLab.Domain/Results/SampleImage.cs`: file name, relative path, sort order (1–4), width and height in points, left and top offset, an optional caption, and a `ContentHash` for integrity.
* **Application** — NEW `Features/ResultsEntry/Commands/AddSampleImage`, `RemoveSampleImage`, `UpdateSampleImageLayout`, and `Features/ResultsEntry/Queries/GetSampleImages`; NEW `Features/ReportProduction/Commands/PrintPictureReport`. Reuse the existing `IReportImageAssetStore` pattern already used for the report header/footer images so storage lives outside the database.
* **Infrastructure** — configuration for the new entity; extend `ReportDocumentContent` with an image section and the composer with a bounded image emitter, mirroring `PageSizeMapper.FitHeaderFooterImage`.
* **Presentation** — an image editor window with drag positioning, reachable from the culture and result screens; register the view model.
* **Tests** — domain layout tests including the four-image limit; application tests for add, remove and reorder; infrastructure tests for the configuration and the asset store; printing tests asserting the emitter clamps an oversized image and that zero images produces today's output unchanged.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddSampleImages`. Additive: a new table `SampleImages` (`Id` identity, `PatientTestId` `int` not null with a foreign key to `PatientTests` and cascade delete, `FileName` `nvarchar(200)` not null, `RelativePath` `nvarchar(400)` not null, `SortOrder` `tinyint` not null constrained to 1–4, `WidthPt` `decimal(7,2)` not null, `HeightPt` `decimal(7,2)` not null, `LeftPt` `decimal(7,2)` not null, `TopPt` `decimal(7,2)` not null, `Caption` `nvarchar(300)` nullable, `ContentHash` `char(64)` nullable, `CreatedAtUtc`/`CreatedByUserId` from the auditable base). Binary data is **not** stored in the database; the column stores a path into the existing asset-store location. No back-fill: historical visits have no images. Evidence the state is not already persisted: no such table or column exists. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference allows one to four images; enforce the maximum as a domain invariant and reuse the existing capacity-failure message rather than writing new text. The reference gives no image caption wording; a caption is operator-supplied free text.

**I. Objective acceptance criteria.** 1. Given no images are attached, when any report is printed, the output is identical to today's. 2. Given one to four images are attached, when the picture report is printed, each image appears once, at its stored position and size, and in its stored order. 3. Given a fifth image is submitted, the request is refused. 4. Given an image whose stored size exceeds the printable area, when the report is printed, it is scaled to fit rather than aborting generation. 5. Given an image file is missing from the asset store, when the report is printed, the report is produced without it and the omission is reported rather than failing silently. 6. Given the image is moved or resized in the editor, the stored offsets change and the printed position follows. 7. Given the patient test is deleted, its images are deleted with it and no orphaned file remains.

**J. Test plan.** Domain tests for the four-image limit and the layout invariants; application tests for the CRUD and the missing-file path; infrastructure tests for the configuration, the foreign key and the asset store; printing tests for fit-clamping and for the no-image regression; migration test for the table. The drag-positioning behaviour needs a Windows-hosted UI test or a manual check in K — it cannot be asserted headlessly. No scanner or camera hardware.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **on a Windows workstation, attach four images, drag and resize them, print, and confirm the placement and that the images are legible**; confirm no excluded capability was introduced — a microscope-camera capture route must not be added; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: binary data on the workstation file system can be lost or duplicated across machines — mitigate by storing under the existing asset-store root with a content hash and by an integrity test. Risk: an oversized image aborts PDF generation — acceptance criterion 4 is the mitigation and the emitter must clamp. Risk: patient data embedded in an image file leaves the database's protection — this is the highest security consideration in the plan; the storage location and access permissions must be decided under DEC-006 and recorded. Rollback: revert the phase and restore the affected files, delete the appended migration, and remove the asset-store sub-directory the phase created; the safest rollback is before any image has been stored.

**M. Phase status.** **NOT FINAL.** Blocked by **DEC-006** (capture and storage model, file formats, size limits and retention). The condition to finalise is a single owner answer fixing those four points; everything else in this phase is already specified.

---

### PHASE 15 — Glossary of analysis abbreviations

**A. Phase identification.** Phase 15. Gap **GAP-015**, class **MISSING**. Size **low**. Risk **low**. One small table, CRUD and a read screen; risk is low because nothing else consumes it.

**B. Dependency position.** Fifteenth, grouped with the two reference-content utilities and placed after all report phases so it cannot be interrupted by report work. Independent of every other phase.

**C. Required reference behaviour.** The utilities window carries a "glossary of the abbreviations used in the field of medical analyses" button. Reference showcase, page 51.

**D. Current Top-Lab behaviour.** `Test.Short` and `TestCode` are catalogue fields used as report headings and by the receipt display mode. `GetTestLibraryQuery` (`src/TopLab.Application/Features/Utilities/Queries/GetTestLibrary/`) lists tests for the utilities screen; it is a test list, not a glossary. Term search `Glossary`, `Terminology`, `Abbreviation` across `src` returned **0 matches**.

**E. Exact behaviour delta.** Reference: a maintainable list of analysis abbreviations and their meanings. Current: none. Required changes: an entity, full CRUD, a searchable read screen inside the existing utilities window, and an explicit statement of who supplies the content. The reference supplies no sample entries, so none may be invented — the phase must ship the mechanism with an empty glossary and a clear "no entries yet" state, and the content is DEC-007.

**F. Exact implementation changes by layer.** Domain: NEW `src/TopLab.Domain/Utilities/GlossaryTerm.cs` (abbreviation, expansion, optional category, optional note). Application: NEW `Features/Utilities/Commands/{CreateGlossaryTerm,UpdateGlossaryTerm,DeleteGlossaryTerm}` and `Queries/GetGlossaryTerms` (searchable, paged). Infrastructure: configuration and a migration seed of zero rows. Presentation: a new tab or dialog inside `Views/Utilities/UtilitiesView.xaml`. Tests: domain uniqueness tests; application CRUD and search tests; configuration test; migration test.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddGlossaryTerms`. Additive: a new table `GlossaryTerms` (`Id` identity, `Abbreviation` `nvarchar(50)` not null with a unique index, `Expansion` `nvarchar(300)` not null, `Category` `nvarchar(100)` nullable, `Note` `nvarchar(500)` nullable). No back-fill; the seed is empty by design. Evidence the state is not already persisted: no such table exists. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** No reference text governs the entries. Reuse existing validation messages. The empty-state text is operator-facing and must be plain, not an invented clinical statement.

**I. Objective acceptance criteria.** 1. Given the glossary is empty, when the screen opens, a clear empty state is shown and no invented entry is displayed. 2. Given a term is created, then it appears in the list and is searchable by abbreviation and by expansion. 3. Given a duplicate abbreviation is submitted, the save is refused. 4. Given a term is deleted, it disappears and no row remains. 5. Given a term with the same expansion but a different abbreviation exists, both are kept.

**J. Test plan.** Domain uniqueness test; application CRUD and search tests; a configuration test for the unique index; a migration test asserting the table exists and holds zero rows. No Windows dependency.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: an empty glossary ships as an apparently broken feature — mitigated by the explicit empty state. Risk: clinical content authored without review — mitigated by DEC-007 requiring an owner for content review. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **NOT FINAL** for the content question only (DEC-007). The mechanism is fully specified; the phase may be implemented and shipped empty, and the decision only affects whether entries are authored.

---

### PHASE 16 — Sample-nature interpretation panel

**A. Phase identification.** Phase 16. Gap **GAP-016**, class **MISSING**. Size **medium**. Risk **high**. The code is small; the risk is high because the content is clinical and the phase ships a mechanism that a non-clinician must populate correctly.

**B. Dependency position.** Sixteenth, directly after Phase 15 because the two are the same kind of work — a reference-content utility with an authoring surface — and are best built by one agent in sequence. Phase 16 additionally reads the test catalogue and the reference ranges, both of which already exist and are not modified.

**C. Required reference behaviour.** The utilities window carries a "sample interpretation" button, described as reading "the nature of the samples, their normal rates and their effect on the patient". Reference showcase, page 51.

**D. Current Top-Lab behaviour.** `Test` carries `Collection` plus `LogGroup`/`LabGroup` labels used as report headings, and `ReferenceRange` supplies the normal interval, but there is no interpretation entity, panel, command, query, DTO, view or view-model that presents a sample's nature together with its normal ranges and their clinical meaning. Term search `SampleNature`, `Interpretation` across `src` returned **0 matches**.

**E. Exact behaviour delta.** Reference: for a chosen sample, show its nature, its normal rates and their clinical effect. Current: the sample name exists and the ranges exist, but nothing joins them into a clinical statement. Required changes: an interpretation record keyed to the sample, carrying its normal range and its clinical note; a read panel that resolves the live range from `ReferenceRange` when no override is stored and shows the stored note; and an authoring screen. The reference supplies no sample entries, so none may be invented — the panel ships empty for un-documented samples, and the content is DEC-007.

**F. Exact implementation changes by layer.** Domain: NEW `src/TopLab.Domain/Utilities/SampleInterpretation.cs` (`TestId` foreign key, optional `MinValue`/`MaxValue` override, `ClinicalNote` required, sort order). Application: NEW `Features/Utilities/Commands/{CreateSampleInterpretation,UpdateSampleInterpretation,DeleteSampleInterpretation}` and `Queries/GetSampleInterpretation`. Infrastructure: configuration; the query joins `ReferenceRange` so the panel always shows the current range unless an override is stored. Presentation: a panel inside the utilities window plus an authoring screen. Tests: domain tests for the override rule; application CRUD tests; a query test proving the live range is used when no override exists; configuration and migration tests.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddSampleInterpretations`. Additive: a new table `SampleInterpretations` (`Id` identity, `TestId` `int` not null with a foreign key to `Tests`, `MinOverride` `decimal(12,4)` nullable, `MaxOverride` `decimal(12,4)` nullable, `ClinicalNote` `nvarchar(1000)` not null, `SortOrder` `int` not null default 0) with a unique index on `TestId`; the seed is empty. No back-fill. Evidence the state is not already persisted: no such table or type exists. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The clinical note is free text authored by a qualified person. No reference wording exists for any entry; none may be invented. The empty state must say plainly that no interpretation has been recorded for the sample.

**I. Objective acceptance criteria.** 1. Given a sample has no interpretation, the panel shows the sample name and its live range from `ReferenceRange` and states that no clinical note is recorded. 2. Given an interpretation exists with no override, the panel shows the live range and the stored note. 3. Given an interpretation exists with an override, the panel shows the override and labels it as an override. 4. Given the range in `ReferenceRange` changes, the panel follows it for samples with no override and does not change for samples with one. 5. Given a duplicate interpretation is submitted for the same sample, the save is refused. 6. Given no interpretation exists for any sample, the utilities screen shows an explicit empty state and no invented clinical text.

**J. Test plan.** Domain tests for the override rule; application CRUD and uniqueness tests; a query test proving the live-range join; configuration and migration tests. No Windows dependency.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **have a qualified reviewer sign off any clinical content that the owner supplies under DEC-007, and record that sign-off**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Clinical-content risk is the dominant one: an inaccurate note is a patient-safety risk, which is why DEC-007 requires a named owner and why the phase ships empty by default. Data risk is low; the table is additive with a unique index. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **NOT FINAL** pending **DEC-007** (clinical content ownership and review). The condition to finalise is a named content owner and a review path; the mechanism is fully specified.

---

### PHASE 17 — Banded result monitor over blood-picture element values

**A. Phase identification.** Phase 17. Gap **GAP-017**, class **MISSING**. Size **low**. Risk **low**. A second data source inside an existing query and an output shape that already exists.

**B. Dependency position.** Seventeenth. It needs nothing from the earlier phases and it is the first phase in the plan with no migration, which makes it a good candidate to run in parallel with Phase 18 once the report phases are done.

**C. Required reference behaviour.** A search and statistics window over blood-picture elements in which each element — HGB, RBCs, WBCs, PLT, RETICS, HCT, MCV, MCH, MCHC, PCT, MPV, LYM%, MON%, NEU%, EDS%, BAS%, Band%, the corresponding absolute counts, and leukaemia cells — has its own check box and its own minimum and maximum, over a from/to date range. Reference showcase, page 73.

**D. Current Top-Lab behaviour.** `src/TopLab.Application/Features/Statistics/Queries/GetBandedResultMonitor/GetBandedResultMonitorQueryHandler.cs:56-68` reads `PatientTest` filtered by `TestId` and requires `ResultFlagComputer.TryParse` to succeed. Every blood-picture analyte value lives in `ProfileResultItem`, which that handler never reads. Term search for an analyte-level band query across `src`: **0 matches**. `ReportProduction/Common/HistoryMatrixBuilder.Pivot` produces a date × analyte matrix but is wired only to patient history.

**E. Exact behaviour delta.** Reference: band ranges can be applied to individual blood-picture elements, including both relative and absolute forms. Current: a band range applies only to a scalar `PatientTest.ResultValue`, so no profile analyte can be monitored. Required changes: an optional analyte-level mode on the existing query, reading `ProfileResultItem` and its `ProfileAnalyte`, with the same band semantics, the same lifecycle-status label and the same referral-name resolution. Regression risk: the existing single-test mode must keep returning exactly what it returns today.

**F. Exact implementation changes by layer.** Domain: none required. Application: extend `GetBandedResultMonitorQuery` with an optional analyte selector and an analyte-level band list; extend `GetBandedResultMonitorQueryHandler` with a second branch that reads `ProfileResultItem` and reuses the existing flag computation; extend the DTO. Infrastructure: no change to persistence; `BandedResultMonitorPdfWriter` gains the analyte rows. Presentation: extend `Views/Statistics/StatisticsView.xaml` and its view model with the analyte grid; register nothing new. Tests: application tests for both branches, including that the single-test branch is unchanged; printing tests for the analyte rows.

**G. Migration decision.** **NO MIGRATION.** `ProfileResultItem` already exists (`src/TopLab.Domain/Results/ProfileResultItem.cs:30-34` carries the value, the flag and the print columns) and every analyte is already stored. A second read path over an existing table requires no schema change, so no column, index or constraint is needed. Evidence: the analyte table is created by the existing `20260909033414_AddAnalyteProfileDomain` migration and no new persisted state is introduced. This decision does not depend on any row being absent; it depends on the columns already existing.

**H. Business rules and message text.** The reference shows the element names in Latin script and the existing monitor already uses the Arabic status labels `تم التسليم`, `تمت الطباعة`, `معتمد` and `غير معتمد`. Reuse them unchanged. No new message is introduced.

**I. Objective acceptance criteria.** 1. Given the single-test mode with no analyte selected, the result is identical to today's for the same inputs. 2. Given an analyte with a band, when the monitor runs, only patients whose value for that analyte lies inside the band are returned. 3. Given several analytes with bands, every matching analyte row is returned for a patient who matches more than one. 4. Given a patient has no value for the selected analyte, no row is returned for that analyte. 5. Given a patient is soft-deleted, no row is returned. 6. Given the output is printed, each row carries its analyte name, its value and the same lifecycle label as the single-test mode.

**J. Test plan.** Application tests for both branches and for every acceptance criterion; a regression test asserting the single-test branch's result for a fixed input; printing tests for the analyte rows; a configuration test asserting no new column is needed. No migration test, because there is no migration.

**K. Phase-end verification gate.** Build; run the whole suite; confirm no new migration exists and no existing migration changed; confirm the acceptance criteria; **run the monitor against a disposable database seeded with profile results and print the output on a Windows workstation**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: the two branches drift so that a future change fixes one and not the other — mitigate by extracting the shared band and label logic rather than duplicating it. Risk: a performance regression from loading profile items in memory — the existing handler already materialises `PatientTest` rows, so the pattern is unchanged, but a query with many analytes and a wide date range should be bounded. Rollback: revert the phase and restore the affected files; there is no schema to unwind.

**M. Phase status.** **FINAL.**

---

### PHASE 18 — Charted blood-count history report

**A. Phase identification.** Phase 18. Gap **GAP-018**, class **MISSING**. Size **high**. Risk **high**. Size is high because it introduces a charting capability the codebase has never had. Risk is high because charting inside a PDF generator is the most failure-prone rendering work in the plan.

**B. Dependency position.** Eighteenth. It depends on the existing pivot only, so it is technically independent, but it is placed here because it is the second phase with no migration and it should not be interleaved with the schema-carrying phases.

**C. Required reference behaviour.** A "COMPLETE BLOOD COUNT HISTORY REPORT" with a bar chart per analyte and trend lines for the RBC indices and the differential across visits, drawn over the same dates. Reference showcase, page 66.

**D. Current Top-Lab behaviour.** `HistoryMatrixBuilder.Pivot` produces the **tabular** date × analyte matrix. No charting component is referenced anywhere; `src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs:30` explicitly records that there is no charting library and that the screen uses data grids and number cards. Term search `Chart`, `Plot`, `BarSeries`, `LineSeries` across `src` returned **0 matches` other than that comment. `ReportPageComposer` emits text and tables only.

**E. Exact behaviour delta.** Reference: the history is presented as charts. Current: the same data is presented as a table. Required changes: a chart model (series, axes, categories), a chart renderer that draws into the report, and a report layout that places a bar panel per analyte and a trend panel per index and differential group. The tabular report must keep working. Regression risk: a chart larger than its allotted panel overflows the page; the emitter must scale, and the acceptance criteria pin it.

**F. Exact implementation changes by layer.** Domain: NEW `src/TopLab.Domain/Reports/HistoryChartSpec.cs` describing panel kind, title, categories and series. Application: extend the separate-history query to return a chart specification alongside the existing rows; the tabular path is untouched. Infrastructure: extend `ReportDocumentContent` with a chart section; add a chart renderer in `src/TopLab.Infrastructure/Printing/` that draws the axes, bars and lines with explicit bounds; the composer places it inside the existing panel machinery from Phase 8. Presentation: a chart/no-chart toggle on the history report screen. Tests: chart-model tests; printing tests for panel scaling and for the no-chart default producing today's output.

**G. Migration decision.** **NO MIGRATION.** Every value the chart needs already exists in `ProfileResultItem` across visits, and the pivot already produces the matrix. The chart is a presentation of existing data, so no column, index or constraint is required. Evidence: `ProfileResultItem` is created by the existing `20260909033414_AddAnalyteProfileDomain` migration and the pivot reads only its existing columns. If the owner later wants chart options such as axis ranges or series selection persisted per user, that becomes a **NEW MIGRATION** and a separate phase — it must not be added inside this one.

**H. Business rules and message text.** The reference panel titles are the analyte names already in the catalogue. The reference chart legend and axis titles are not legible enough to transcribe; do not invent axis titles, and use the analyte names and the reference-range text already persisted on the analyte.

**I. Objective acceptance criteria.** 1. Given the chart toggle is off, when the history report is printed, the output is identical to today's table. 2. Given the toggle is on, each analyte with more than one recorded value gets a bar panel whose categories are the visit dates in the resolved sort order. 3. Given the RBC indices or the differential group is selected, it is drawn as a line panel across the same dates. 4. Given a panel's content exceeds its allotted area, it is scaled to fit and never crosses a page boundary. 5. Given an analyte has a single recorded value, it is rendered as a single-category panel rather than omitted silently. 6. Given no values exist for a selected analyte, the panel shows the analyte name with an empty axis rather than failing.

**J. Test plan.** Chart-model tests covering the panel kinds; printing tests for scaling, single-value and empty cases; a regression test asserting the tabular default; a configuration assertion that no new column is required. No Windows dependency; K includes a visual print check.

**K. Phase-end verification gate.** Build; run the whole suite; confirm no new migration exists and no existing migration changed; confirm the acceptance criteria; **print a charted history report on a Windows workstation with a dataset spanning several visits and confirm the bars, the axes and the trend lines are legible and inside the page**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** Risk: a charting dependency that does not support right-to-left or Arabic labels — the safest default is Latin analyte names, which is what the reference shows. Risk: chart generation aborts PDF generation on malformed data — the emitter must validate its inputs and fall back to the table. Risk: a new package reference changes the dependency footprint — it must be recorded in the changed-file inspection. Rollback: revert the phase, restore the affected files and remove the package reference; there is no schema to unwind.

**M. Phase status.** **FINAL**, on the explicit condition that persisted chart options are **not** part of this phase.

---

### PHASE 19 — Auto-generated remark composed from result flags

**A. Phase identification.** Phase 19. Gap **GAP-019**, class **MISSING**. Size **low**. Risk **high**. The code is a few lines; the risk is high because the content is clinical and the reference does not define the rule.

**B. Dependency position.** Nineteenth, after Phase 5 and Phase 9 so that the remark sits among the other patient-header lines and can be hidden and styled like one of them. It is placed late because its content rule is an open decision and it should not block earlier work.

**C. Required reference behaviour.** The CBC report pages carry an automatically generated remark line. Reference showcase, pages 56, 61 and 63. The reference does **not** state the composition rule, the wording or the trigger.

**D. Current Top-Lab behaviour.** The report emits `LowComment` / `HighComment` text sourced from the reference range (`src/TopLab.Infrastructure/Printing/ReportContentBuilder.cs:82-87, 149-154, 233-238`), which is a stored comment, not a remark composed from the abnormal findings. `ProfileResultItem` carries a flag but no remark field. Term search `Remark` across `src` returned **0 matches** other than XML documentation comments.

**E. Exact behaviour delta.** Reference: a remark is generated automatically from the abnormal findings. Current: the stored low/high comments are printed instead. Because the reference does not define the rule, the phase may only be specified as far as the evidence allows: a remark block composed from the flagged results, where the composition rule and the wording are DEC-003. Do not invent a template.

**F. Exact implementation changes by layer.** Domain: NEW `src/TopLab.Domain/Results/ResultRemark.cs` — an optional per-test remark override, so that an operator can correct or suppress a generated remark without changing the generator. Application: NEW `Features/ReportProduction/Commands/UpdateResultRemark`. Infrastructure: a remark builder in `src/TopLab.Infrastructure/Printing/` that consumes the flagged results and the stored comments under the rule DEC-003 selects; the composer emits it as a new body line registered in `ReportItemKeys`. Presentation: an editable remark field on the result entry screens. Tests: builder tests for each branch of the selected rule; printing tests asserting the line appears only when a remark exists.

**G. Migration decision.** **NO MIGRATION**, conditional on DEC-003. If the owner selects a rule that composes the remark entirely from data that already exists — the flag, the value and the stored low/high comment — then no new persisted state is needed and the decision stands. If the owner selects a rule that requires stored remark text, this becomes **NEW MIGRATION** and the table above becomes the schema. The coding agent must not assume either answer; the phase gate in K records the decision actually implemented and Section 5 must then be updated.

**H. Business rules and message text.** **There is no reference wording for the remark and none may be invented.** The rule and the template are DEC-003. Whatever is implemented must be recorded in the phase's own change log with its exact wording so it can be reviewed as clinical content.

**I. Objective acceptance criteria.** 1. Given every result is normal, no remark line is emitted. 2. Given at least one result is flagged, a remark line is emitted containing that result under the rule DEC-003 selects. 3. Given several results are flagged, all of them appear, in the report's line order. 4. Given an operator has stored an override remark, the override is printed instead of the generated one. 5. Given the operator clears the override, the generated remark returns. 6. Given the remark line is disabled in the item-template grid, no remark is printed.

**J. Test plan.** Builder tests for each branch of the selected rule; application tests for the override; printing tests for presence, absence and override; configuration and migration tests only if the conditional migration is taken. No Windows dependency.

**K. Phase-end verification gate.** Build; run the whole suite; record whether a migration was required and update Section 5 accordingly; confirm existing migrations unchanged; confirm the acceptance criteria; **have a qualified reviewer approve the exact remark wording before the phase is accepted, and record that approval**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** The dominant risk is clinical: an automatically generated clinical statement that has not been reviewed. That is why the phase ships with a per-test override and why qualified approval is a gate. Rollback: revert the phase and restore the affected files; if the conditional migration was taken, drop it.

**M. Phase status.** **NOT FINAL.** Blocked by **DEC-003** (the composition rule and the wording of the generated remark) and gated on qualified clinical approval. The condition to finalise is a single owner answer giving the rule and the wording, plus a named reviewer.

---

### PHASE 20 — Appointment-note reminder to a user or contact

**A. Phase identification.** Phase 20. Gap **GAP-020**, class **MISSING**. Size **medium**. Risk **high**. Size is medium: a small table, CRUD and a scheduler hook. Risk is high because it is the only phase in the plan whose implementation depends on a delivery channel that the exclusions constrain.

**B. Dependency position.** Twentieth and last. It reuses the existing phone book and needs nothing from any earlier phase, but it is sequenced last deliberately: its size is driven by an open design decision and it must never delay work that is fully specified. If DEC-002 is decided quickly this phase can be executed at any point; the position here reflects sequencing prudence, not a code dependency.

**C. Required reference behaviour.** The utilities window carries a button described as "a reminder message for a specified user or a person, through the appointments note". Reference showcase, page 51. The reference does **not** state the delivery channel.

**D. Current Top-Lab behaviour.** The utilities area contains a phone book (`AddPhoneBookEntry`, `RemovePhoneBookEntry`, `GetPhoneBookQuery`) and a purchases list, and a unit converter and a stopwatch. There is no reminder, appointment, note-scheduling or notification entity, command, query, DTO, view or view-model anywhere in `src`. Term search `Reminder`, `Appointment`, `Notification` across `src` returned **0 matches** each; the single `Scheduling` hit is a comment about operating-system task scheduling.

**E. Exact behaviour delta.** Reference: a reminder addressed to a specified user or person, carried on an appointments note. Current: no such capability. **The reference does not establish a delivery channel, and the exclusions forbid SMS, e-mail and fax result delivery.** The phase must therefore be specified only as far as the evidence allows: an appointments-note record, addressed either to a system user or to a phone-book contact, with a due date, and a delivery channel that stays entirely inside the application. The channel itself is DEC-002 and must not be selected by the implementing agent.

**F. Exact implementation changes by layer.**

* **Domain** — NEW `src/TopLab.Domain/Utilities/AppointmentNote.cs` (`Title`, `Body`, `DueAtUtc`, `IsCompleted`, `TargetUserId` nullable, `PhoneBookEntryId` nullable, and a constraint that exactly one target is set).
* **Application** — NEW `Features/Utilities/Commands/{CreateAppointmentNote,UpdateAppointmentNote,CompleteAppointmentNote,DeleteAppointmentNote}` and `Queries/GetDueAppointmentNotes`. A due-note query surfaces notes inside the application only — a list on the utilities screen and, if the owner wants, a count in the shell status area. **No external sender is added under any decision of DEC-002 other than "in-application only".**
* **Infrastructure** — configuration; no messaging infrastructure is introduced, because none exists and adding one would exceed the phase's scope.
* **Presentation** — a notes list and editor inside `Views/Utilities/UtilitiesView.xaml`.
* **Tests** — domain constraint tests; application CRUD and due-query tests; configuration and migration tests.

**G. Migration decision.** **NEW MIGRATION.** Name pattern `AddAppointmentNotes`. Additive: a new table `AppointmentNotes` (`Id` identity, `Title` `nvarchar(200)` not null, `Body` `nvarchar(1000)` not null, `DueAtUtc` `datetime2` not null, `IsCompleted` `bit` not null default 0, `TargetUserId` `int` nullable with a foreign key to `Users` and `null` cascade, `PhoneBookEntryId` `int` nullable, plus the auditable columns). No back-fill; the seed is empty. Evidence the state is not already persisted: no such table exists. Appended after `20261018144852_AddProfileResultStyling`.

**H. Business rules and message text.** The reference gives no wording. The note's title and body are operator-authored free text; no reminder template may be invented. The list view must show the due date and the addressee.

**I. Objective acceptance criteria.** 1. Given a note addressed to a system user, when it is saved, then the due query returns it and the addressee is displayed as that user. 2. Given a note addressed to a phone-book contact, the due query returns it and the addressee is displayed as that contact. 3. Given a note with both targets or neither, the save is refused. 4. Given a note is completed, it no longer appears in the due query but remains in the full list. 5. Given a note is deleted, no row and no file remains. 6. Given the reminder is shown, it appears **only** inside the application and no external channel is contacted. 7. Given the current user lacks the relevant permission, the notes list is not reachable.

**J. Test plan.** Domain constraint tests; application CRUD, due-query and authorization tests; configuration and migration tests. No external-channel test exists and none may be added. No Windows dependency.

**K. Phase-end verification gate.** Build; run the whole suite; apply the migration to a disposable database; confirm existing migrations unchanged; confirm the acceptance criteria; **confirm by inspecting the complete changed-file set that no SMS, e-mail or fax sender was introduced, and that the reminder surfaces only in-application**; inspect the changed file set; record commands and outputs.

**L. Risks and rollback.** The dominant risk is scope creep into an external delivery channel, which is permanently out of scope; acceptance criterion 6 and the gate in K exist specifically to catch it. Secondary risk: a reminder addressed to the wrong contact is an operational annoyance rather than a data risk. Rollback: revert the phase and restore the affected files; drop the appended migration.

**M. Phase status.** **NOT FINAL.** Blocked by **DEC-002** (the delivery channel). The condition to finalise is a single owner answer; the only answer consistent with the permanent exclusions is "in-application only", and if the owner prefers an external channel the function must be reclassified as an owner exclusion and dropped from scope rather than implemented.

---

## SECTION 5 — MIGRATION SUMMARY

The existing migration chain at the target commit is 22 migrations, from `20260828052248_BaselineDataModel` through `20261018144852_AddProfileResultStyling`, plus `ApplicationDbContextModelSnapshot.cs`. That count was confirmed by listing the migration directory: exactly 22 `*.cs` migration classes plus their designer files and one snapshot.

**Rules applied.** Every change below is additive. No existing migration is edited. Every new migration is appended after the latest existing one and, where phases stack on one table, after the previous phase's migration. No phase introduces more than one migration.

| # | Phase | Gap | Migration | Additive change | Tables and columns | Types, nullability, defaults, constraints | Back-fill | Evidence the state is not already persisted |
|---|---|---|---|---|---|---|---|---|
| 1 | 1 — Separate the account-visibility grant | GAP-001 | **YES** | One seeded permission row, plus a grant back-fill | `Permissions` (seed), `UserPermissionGrants` (back-fill) | `Code` `nvarchar(50)` not null, unique; `Description` `nvarchar(300)` not null | **Required**: every holder of the cash-movement code is granted the new code | `PermissionConfiguration.cs:17-31` holds exactly thirteen seeded rows and none is an account-visibility code |
| 2 | 2 — Per-header/footer element control | GAP-002 | **YES** | New table plus six columns | New `ReportHeaderElements`; `ReportSettings` | `HeaderFooterZone` `nvarchar(20)` not null, `ElementName` `nvarchar(50)` not null, composite PK; `IsEnabled` `bit` not null default 1; `LeftOffsetCm` `decimal(5,2)` not null default 0; `TopOffsetCm` `decimal(5,2)` not null default 0. Six nullable columns on `ReportSettings`: `ShowTopLine` `bit`, `ShowBottomLine` `bit`, `UseLongDate` `bit`, `ShowTime` `bit`, `TopLineWidth` `decimal(6,2)`, `TopLineHeight` `decimal(6,2)` | Seed update for `ReportSettings` row `Id = 1`; no element rows, which is equivalent to "all enabled at offset zero" | `ReportSettingsConfiguration.cs:17-35` maps no such columns; no `ReportHeaderElement` type exists |
| 3 | 3 — Configurable sticker size | GAP-003 | **YES** | Two columns | `SystemSettings` | `BarcodeLabelWidthMm` `decimal(6,2)` nullable, `BarcodeLabelHeightMm` `decimal(6,2)` nullable; seed 38.00 and 25.00 | Seed only; 38 × 25 reproduces today's constant exactly | `BarcodeService.cs:96-98` is a compile-time constant |
| 4 | 4 — Sequential sample number | GAP-004 | **YES** | New table plus one column | New `SampleNumberSequences`; `PatientTests` | `ScopeKey` `nvarchar(50)` PK, `NextValue` `bigint` not null default 1, `LastAllocatedDate` `date2` nullable; `PatientTests.SampleNumber` `bigint` nullable | None — null means "not allocated before this phase" and the line is omitted | `PatientTestConfiguration.cs` has no `SampleNumber`; no sequence type exists |
| 5 | 5 — English patient name on the report | GAP-005 | **YES** | One column | `Patients` | `FullNameEn` `nvarchar(200)` nullable | None — null reproduces today's single-name line | `Patient.cs` has a single `FullName` |
| 6 | 6 — Colour versus monochrome | GAP-006 | **YES** | One column | `ReportSettings` | `ReportOutputMode` `tinyint` not null, default the colour value | Seed update for row `Id = 1` | `ReportSettingsConfiguration.cs:17-35` has no such property |
| 7 | 7 — A3 and A4 85 × 55 | GAP-007 | **NO** (conditional) | None expected | — | The persisted column is already an int conversion of the enum (`ReportSettingsConfiguration.cs:17`), so adding enum members changes no table | None | Confirmed by the fact that the column type does not depend on the enum's membership; must be re-confirmed by an executed pending-change check |
| 8 | 8 — Folded / multi-port printing | GAP-008 | **YES** | One column | `ReportSettings` | `ReportFoldLayout` `tinyint` not null default 0 (single panel) | Seed default reproduces today's flat output | No such property exists |
| 9 | 9 — Font per report line | GAP-009 | **YES** | Two columns | `ReportItemTemplates` | `FontFamily` `nvarchar(100)` nullable, `FontSizePt` `decimal(5,2)` nullable | None — all 24 seeded rows stay null, reproducing today's output | `ReportItemTemplateConfiguration.cs:18-20` maps only spacing, offset and marker |
| 10 | 10 — Per-user report toggle | GAP-010 | **YES** | One column plus a key alteration | `ReportItemTemplates` | `UserId` `int` nullable, foreign key to `Users`; primary key changed from `ItemKey` to `(ItemKey, UserId)` | Existing 24 rows retain global scope; no row is lost | `ReportItemTemplateConfiguration.cs:11` declares a single-column key |
| 11 | 11 — Font per envelope item | GAP-011 | **YES** | Two columns | `EnvelopePrintItemPositions` | `FontFamily` `nvarchar(100)` nullable, `FontSizePt` `decimal(5,2)` nullable | None — null reproduces today's envelope | `EnvelopePrintItemPosition.cs:6-41` accepts exactly four properties |
| 12 | 12 — Vertical coding | GAP-012 | **YES** | One column | `SystemSettings` | `BarcodeOrientation` `tinyint` not null, default horizontal | Seed default reproduces today's horizontal output | `SystemSettings.cs:9-34` has no such property |
| 13 | 13 — Labels per sample material | GAP-013 | **YES** | Two new tables | New `SampleMaterials`, new `PatientTestSampleMaterials` | `SampleMaterials`: `Id` identity, `Name` `nvarchar(100)` not null unique, `SortOrder` `int` not null, `IsActive` `bit` not null, seeded with the seven reference materials. `PatientTestSampleMaterials`: `PatientTestId` `int` not null and `SampleMaterialId` `int` not null, composite PK, foreign keys to `PatientTests` and `SampleMaterials` | **Deliberately none** — historical visits get no distribution and the screen says so | No such table or type exists |
| 14 | 14 — Picture report | GAP-014 | **YES** | New table | New `SampleImages` | `Id` identity; `PatientTestId` `int` not null, FK cascade; `FileName` `nvarchar(200)` not null; `RelativePath` `nvarchar(400)` not null; `SortOrder` `tinyint` not null constrained to 1–4; `WidthPt`, `HeightPt`, `LeftPt`, `TopPt` `decimal(7,2)` not null; `Caption` `nvarchar(300)` nullable; `ContentHash` `char(64)` nullable; auditable columns | None — historical visits have no images | No image entity, column or type exists |
| 15 | 15 — Glossary | GAP-015 | **YES** | New table | New `GlossaryTerms` | `Id` identity; `Abbreviation` `nvarchar(50)` not null unique; `Expansion` `nvarchar(300)` not null; `Category` `nvarchar(100)` nullable; `Note` `nvarchar(500)` nullable | None — the seed is empty by design | No such table exists |
| 16 | 16 — Sample interpretation | GAP-016 | **YES** | New table | New `SampleInterpretations` | `Id` identity; `TestId` `int` not null, FK to `Tests`, unique; `MinOverride`, `MaxOverride` `decimal(12,4)` nullable; `ClinicalNote` `nvarchar(1000)` not null; `SortOrder` `int` not null default 0 | None — the seed is empty by design | No such table or type exists |
| 17 | 17 — Banded monitor over analytes | GAP-017 | **NO** | — | — | `ProfileResultItem` already stores the value, the flag and the print columns | None | The analyte table is created by the existing `20260909033414_AddAnalyteProfileDomain` migration; a second read path needs no schema change |
| 18 | 18 — Charted history | GAP-018 | **NO** | — | — | All charted values already exist in `ProfileResultItem` across visits; the chart is presentation only | None | Same as row 17; persisted chart options are explicitly excluded from this phase |
| 19 | 19 — Generated remark | GAP-019 | **NO** (conditional) | — | — | If DEC-003 selects a rule composed from the existing flag, value and stored comments, no new persisted state is needed | None | Same as row 17; if a stored remark text is required the decision becomes **YES** with the `ResultRemark` table above and Section 5 is updated before the phase is accepted |
| 20 | 20 — Appointment-note reminder | GAP-020 | **YES** | New table | New `AppointmentNotes` | `Id` identity; `Title` `nvarchar(200)` not null; `Body` `nvarchar(1000)` not null; `DueAtUtc` `datetime2` not null; `IsCompleted` `bit` not null default 0; `TargetUserId` `int` nullable with a null-cascade foreign key to `Users`; `PhoneBookEntryId` `int` nullable; auditable columns | None — the seed is empty | No reminder, appointment or notification entity exists |

### Reconciled migration total

| Measure | Value |
|---|---|
| Phases in the plan | 20 |
| Phases with **NEW MIGRATION** | **16** (phases 1, 2, 3, 4, 5, 6, 8, 9, 10, 11, 12, 13, 14, 15, 16, 20) |
| Phases with **NO MIGRATION** | **4** (phases 7, 17, 18, 19) |
| Phases whose migration decision is **conditional** | 2 (phase 7 — expected none, to be confirmed by an executed pending-change check; phase 19 — expected none, to be confirmed by DEC-003) |
| Phases requiring an edit to an existing migration | **0** |
| Phases introducing more than one migration | **0** |
| Migrations appended after `20261018144852_AddProfileResultStyling` | 16 |
| Of those, migrations that alter an existing table's primary key | **1** (phase 10, on `ReportItemTemplates`) |
| Of those, migrations that add only seeded rows plus a grant back-fill and no new column | **1** (phase 1) |
| Of those, migrations that are purely additive new tables or new nullable / defaulted columns | **14** |

**Migration ordering constraints the sequence respects.** Phases 9 then 10 must apply in that order: they touch one table and phase 10 alters its key. Phase 2 precedes phases 4, 5, 6, 9 and 12 conceptually but not in the migration order, because those phases touch different tables. Phases 3, 12 and 13 all touch label geometry and are ordered 3 → 12 → 13. Phase 20's migration is independent and may be applied at any point after the earlier ones. If either conditional decision resolves to **NEW MIGRATION**, the totals above become 18 NEW and 2 NO, and this table must be updated before the affected phase is accepted.

---

## SECTION 6 — OPEN OWNER DECISIONS

### DEC-001 — In-scope data route for analyser-derived CBC report artefacts

* **Question.** Should the CBC report histograms, the two-dimensional scattergram and the five-part / three-part differential be implemented in this project, and if so, where does their data come from without building an analyser connection?
* **Evidence and its limits.** Reference showcase, pages 56–66 show these three outputs on CBC report pages whose provenance line states the values are produced on a fully automatic haematology analyser. Acquisition of analyser results is an owner exclusion. Reference showcase, pages 20–21 show the **same laboratory system** printing bar charts and scatter plots for a semen-analysis report, with an explicit option to show or hide them — so charting in a lab report is an in-scope capability in the reference. The reference nowhere describes how the lab system obtains a histogram image.
* **Options.** (a) Classify all three as owner exclusions, consistent with the analyser boundary, and close them. (b) Implement the rendering only, against an image supplied manually by the operator for each visit. (c) Implement the rendering only, against a data route the owner specifies.
* **Consequences.** (a) closes three rows at zero cost but leaves a visible report family unimplemented. (b) is implementable today and keeps the boundary intact, at the cost of a manual data-entry step the reference does not describe. (c) is implementable only once a route exists, and any route the owner names must not amount to an analyser connection.
* **Operational impact, security, compatibility.** Under (b) an operator-supplied image is stored on the workstation and embedded in a patient document, which carries the same data-protection consideration as the picture report in Phase 14. Under (c) the risk depends entirely on the route chosen.
* **Recommendation.** Option (a) for the histogram and the scattergram, whose only reference evidence is the analyser product, and option (b) for the differential, whose paired relative and absolute counts are ordinary result values that the lab system already stores in `ProfileResultItem` and could accept as two extra analyte values. Note that this recommendation is a recommendation, not a decision.
* **Blocks.** It blocks BLOCK-001, BLOCK-002 and BLOCK-003. It does not block any phase in Section 4.

### DEC-002 — Delivery channel for the appointment-note reminder

* **Question.** Through which channel is the reminder delivered?
* **Evidence and its limits.** Reference showcase, page 51 says only "a reminder message for a specified user or a person, through the appointments note". No channel is named. Reference showcase, page 53 describes SMS as a separate capability, which is an owner exclusion.
* **Options.** (a) In-application only — a due-notes list on the utilities screen and an optional count in the shell. (b) An external channel, which would require the function to be reclassified as an owner exclusion first.
* **Consequences.** (a) delivers no external reminder, which is a real operational shortfall against a laboratory that expects a phone call. (b) cannot be implemented without breaching the permanent exclusions.
* **Operational impact.** Under (a) staff must open the application to see the reminder.
* **Recommendation.** (a). If the owner judges that an external reminder is essential, the correct action is to record the function as an owner exclusion rather than to implement it.
* **Blocks.** Phase 20.

### DEC-003 — Composition rule and wording of the generated remark

* **Question.** What text does the automatically generated remark contain, and what triggers it?
* **Evidence and its limits.** Reference showcase, pages 56, 61 and 63 show a remark line on CBC reports. The reference gives no wording and no rule.
* **Options.** (a) Concatenate the stored low and high comments of every flagged result. (b) Concatenate `analyte: value (flag)` for every flagged result. (c) A template with a fixed sentence plus the flagged items.
* **Consequences.** (a) needs no new persisted state; (b) needs none either; (c) needs none but produces a sentence that reads like clinical prose and therefore needs review.
* **Operational impact, security, maintenance.** Under any option the remark is patient-facing clinical text. (c) is the most readable and the most exposed if the rule is wrong.
* **Recommendation.** (a), because it reuses text a clinician already authored and reviewed, and it requires no new persisted state — which is also what keeps Phase 19's migration decision at NO MIGRATION.
* **Blocks.** Phase 19, and with it the conditional row in Section 5.

### DEC-004 — Numbering scope and format for the sequential sample number

* **Question.** Is the sample number allocated per day, per visit, or per accession, and what format does it print?
* **Evidence and its limits.** Reference showcase, pages 56, 61, 62 and 65 show a number and a sample identifier on CBC reports without stating how either is scoped.
* **Options.** (a) A single monotonic counter for the installation. (b) A counter reset each day. (c) A counter per accession.
* **Consequences.** (a) is simplest and guarantees uniqueness but is meaningless across years. (b) is readable and is the convention in most laboratories, but a reset must be atomic. (c) matches laboratory practice but requires an accession concept the application does not have.
* **Operational impact.** (b) is what staff expect to read aloud over the telephone.
* **Recommendation.** (b), with an atomic reset, and a plain numeric format with no prefix.
* **Blocks.** Phase 4.

### DEC-005 — Does the per-line Font control also govern header/footer items?

* **Question.** Phase 2 moves header and footer elements to their own table. Does the reference's per-line Font selector apply to those items as well as to body lines?
* **Evidence and its limits.** Reference guide, pages 193 and 195 show the Font selector inside the header/footer item table. Body-item fonts are not shown separately on those pages; the body-line layout controls appear on reference showcase, page 13.
* **Options.** (a) One font mechanism covering both zones. (b) Two mechanisms, matching the two screens the reference shows.
* **Consequences.** (a) is simpler but merges two tables that the reference keeps visually distinct. (b) matches the evidence but is more work.
* **Recommendation.** (b), which is what Phase 2 already assumes; under (a) Phase 9 must additionally extend `ReportHeaderElement`.
* **Blocks.** It does not block Phase 9 or Phase 10, but it changes their scope and must be answered before either starts.

### DEC-006 — Capture and storage model for sample images

* **Question.** How are the one to four images obtained, where are they stored, which formats are accepted, and how large may they be?
* **Evidence and its limits.** Reference showcase, page 24 describes placement, size, dragging and annotation. It is silent on capture, storage, format and limits.
* **Options.** (a) Import from a file, stored in the existing asset-store location. (b) Capture from a microscope camera or a scanner. (c) Both.
* **Consequences.** (a) is implementable today and keeps the phase inside its scope. (b) introduces a device dependency and would collide with the analyser exclusion if the device is an analyser. (c) inherits (b)'s risk.
* **Operational and security implications.** Patient images are patient data. The storage location, access permissions and retention period must be stated, and the phase's own acceptance criteria already require the asset store's content hash for integrity.
* **Recommendation.** (a) in this project, with (b) deferred.
* **Blocks.** Phase 14.

### DEC-007 — Clinical content ownership for the interpretation panel and the glossary

* **Question.** Who authors and who approves the clinical notes and the abbreviation expansions?
* **Evidence and its limits.** Reference showcase, page 51 names both utilities but supplies no entries and no ownership.
* **Options.** (a) A named laboratory clinician authors and approves, and the features ship empty until content exists. (b) A content vendor supplies it. (c) Derive the glossary from the existing test catalogue.
* **Consequences.** (a) is the only option that is clinically safe. (c) produces abbreviations without expansions and does not satisfy the reference.
* **Operational, compliance and maintenance implications.** Under (a) the features are useful only after a content effort that must be budgeted. An unreviewed clinical note is a patient-safety exposure.
* **Recommendation.** (a), with the phases shipping the mechanism and an explicit empty state.
* **Blocks.** It blocks nothing technically. It determines whether Phases 15 and 16 are accepted as complete or as delivered-empty.

### DEC-008 — Should the seeded discount-limit permission become a real gate?

* **Question.** `DISCOUNT_LIMIT` is seeded but never enforced as a gate; the discount ceiling is enforced as a numeric limit on the user. Should the permission become a gate, or should the seeded row be withdrawn?
* **Evidence and its limits.** Reference guide, page 151 lists "the discount percentage the user may give the patient" among the grants. The reference does not distinguish a grant from a ceiling.
* **Options.** (a) Make it a gate on the discount-bearing command. (b) Withdraw the seeded row and keep the numeric ceiling.
* **Consequences.** (a) matches the reference's presentation but duplicates a control that already exists. (b) is simpler but leaves the grant list short of the reference by one row.
* **Recommendation.** (b), and record the divergence, because the observable behaviour — a bounded per-user discount — is already correct and enforced in `RecordPaymentCommandHandler`.
* **Blocks.** Nothing. It is decided inside Phase 1.

---

## SECTION 7 — RISK REGISTER AND CROSS-PHASE DEPENDENCIES

| # | Risk | Type | Likelihood | Impact | Affected phase or decision | Mitigation |
|---|---|---|---|---|---|---|
| R-01 | The migration chain has never been executed against a real database in this investigation; the only test that would prove it was skipped. A migration defect could surface for the first time when a real database is migrated. | Confirmed fact | Medium | High | All migration phases | Every migration phase applies to a disposable empty database in its gate; the first phase that touches an existing table (Phase 10) must apply to a database already populated by all prior migrations |
| R-02 | Printed documents have never been visually inspected. Arabic shaping, right-to-left ordering, pagination, colour and barcode legibility are unverified. | Confirmed fact | High | Medium | Phases 2, 3, 5, 6, 7, 8, 9, 11, 12, 13, 14, 18 | Each phase's gate requires a physical print on a Windows workstation; content-model assertions alone are explicitly not accepted as sufficient |
| R-03 | The picture-report phase stores patient images on the workstation file system rather than in the database, which moves patient data outside the database's protection. | Confirmed fact | High | High | Phase 14 / DEC-006 | Store under the existing asset-store root with a content hash; decide location, permissions and retention before the phase starts; never add a capture route that would reach an analyser |
| R-04 | Phase 10 alters the primary key of a table that already holds 24 seeded rows. | Confirmed fact | Medium | High | Phase 10 | Decide the nullable-key shape against a disposable database, assert the row count after migration, and roll back before any real database is migrated |
| R-05 | Phase 13 changes a barcode path that the invoice and the group slip also use. | Confirmed fact | Medium | High | Phase 13 | Keep the existing single-identifier method signature untouched and add a new method; acceptance criterion pins the identity-label output to its pre-phase bytes |
| R-06 | Three reference functions (BLOCK-001…003) may in fact be owner exclusions rather than gaps. Implementing them would build excluded functionality. | Possible risk | Medium | High | DEC-001 | They are classified BLOCKED and carry no phase; no in-scope data route is invented; the decision is put to the owner before any scheduling |
| R-07 | The reference's inventory is a reconstruction from a table of contents, per-chapter prose and screen captures; no consolidated requirements list exists. A differently decomposed inventory would give different totals. | Confirmed fact | Medium | Medium | Whole plan | Every inventory row cites a page; the decomposition rules are stated; an owner-confirmed requirements baseline would raise confidence |
| R-08 | Several reference behaviours are evidenced only by a caption beneath a screen capture, with no prose. Whether each is a requirement or an example is unknown. | Confirmed fact | High | Low | Phases 3, 5, 6, 8, 13, 14, 18, 19, 20 | Each such row is flagged in Section 3 and its phase records the evidence-strength caveat |
| R-09 | Combining a Phase 9 per-line font with a Phase 8 folded panel could overflow a panel. | Possible risk | Medium | Medium | Phases 8 and 9 | They are deliberately not combined; the folded-panel clipping test must be re-run after Phase 9 lands |
| R-10 | An auto-generated clinical remark that has not been reviewed is a patient-safety exposure. | Confirmed fact | Medium | High | Phase 19 / DEC-003 | Ship a per-test override; require qualified approval of the exact wording as a phase gate; prefer a rule that reuses clinician-authored text |
| R-11 | Sample images and generated remarks both enlarge the patient record and could be retained after the clinical need passes. | Possible risk | Medium | Medium | Phases 14 and 19 | Retention is part of DEC-006; the override remark in Phase 19 is auditable through the existing `AuditableEntity` columns |
| R-12 | The reminder phase could drift toward an external channel, which is permanently out of scope. | Possible risk | Medium | High | Phase 20 / DEC-002 | Acceptance criterion 6 and an explicit changed-file inspection in the phase gate |
| R-13 | Two phases change patient-facing rendering (5 and 9) and a third changes page geometry (7). A rendering regression could be blamed on the wrong phase. | Possible risk | Medium | Medium | Phases 5, 7, 9 | Each phase is independently verifiable and its gate requires a print before the next begins |
| R-14 | The reference's English patient name and the per-line fonts both affect Arabic text layout and could interact. | Possible risk | Low | Medium | Phases 5 and 9 | The per-phase print checks must include an Arabic and a Latin run |
| R-15 | Content authored for the interpretation panel or the glossary could be clinically wrong. | Confirmed fact | Medium | High | Phases 15 and 16 / DEC-007 | Both ship empty with an explicit empty state; qualified sign-off is a phase gate |

### Cross-phase dependency map

* **Report settings surface** — Phase 1 (grant catalogue) → Phase 2 (header elements) → Phases 4, 5, 6, 7, 8 (report content and page geometry) → Phase 9 (per-line fonts) → Phase 10 (per-user scope). This is the longest chain in the plan and the reason the first ten phases are ordered as they are.
* **Report item template table** — Phase 9 then Phase 10, strictly in that order.
* **Label pipeline** — Phase 3 (size) → Phase 12 (orientation) → Phase 13 (multi-label payload).
* **Envelope** — Phase 11 (item fonts) is independent of Phase 2 but shares the envelope writer with Phase 12.
* **Patient record** — Phase 5 (English name) and Phase 4 (sample number) both extend `Patients`/`PatientTests`; they are ordered so that the report content record is extended once per field rather than twice in adjacent phases.
* **No migrations** — Phases 17 and 18 are independent of every other phase and may be executed in parallel once Phases 1–10 are complete.
* **Owner decisions** — DEC-005 gates Phases 9 and 10; DEC-004 gates Phase 4; DEC-006 gates Phase 14; DEC-002 gates Phase 20; DEC-003 gates Phase 19; DEC-001 gates nothing but determines whether three further functions enter or leave scope; DEC-007 gates acceptance of Phases 15 and 16 but not their implementation.

---

## SECTION 8 — SELF-AUDIT CHECKLIST

| # | Item | Result | Basis |
|---|---|---|---|
| 1 | Completeness of the reference inventory | **PARTIALLY VERIFIED** | Every page of both documents was text-extracted and rendered. Ten pages were opened individually at high resolution — the pages carrying the function evidence this plan relies on — and a further 26 pages as contact sheets. The remaining 258 pages were read as extracted text only. Three functions were added and three withdrawn against the prior assessment, so the decomposition is defensible but not proven unique. |
| 2 | Verification of all identified MISSING functions | **PASS** | 18 MISSING rows, each with a documented negative search naming the terms searched and the match count, each confirmed against a full file listing of `src`, and each confirmed by the absence of any entity, command, query, DTO, view or view-model that could carry it. |
| 3 | Verification of all identified DIFFERENT functions | **PASS** | 2 DIFFERENT rows, each traced from the reference page to the exact configuration or composer line that produces the deviation, and each re-checked against the actual source at the target commit. |
| 4 | Excluded-feature isolation | **PASS** | 23 excluded functions listed with page and category. No excluded function appears as a gap, a phase, a migration item or a recommendation. Boundary notes are recorded for the four shared screens. No excluded capability is introduced by any phase, and each phase gate requires an explicit check. |
| 5 | Source restrictions | **PASS** | Only the two authorised PDFs were used for reference requirements. No external site, manual or domain knowledge was used to establish a requirement. No file in the reference-system documentation directory was listed, opened, searched, read or cited, and no search was rooted there. Prose documentation in the repository was not treated as evidence of either reference requirements or implementation status. |
| 6 | Source-path and line-number accuracy | **PASS** | Every file path and line range quoted in this plan was produced by reading the file at the target commit during this investigation. The one line-number claim that could not be reproduced — a page-colour line in the report composer — was re-derived by locating the call and is quoted at the actual line, 361. |
| 7 | PDF visual inspection coverage | **PARTIALLY VERIFIED** | 289 of 289 pages were text-extracted and rendered. 10 pages were opened individually at 190 dpi and 26 pages were opened as contact sheets. The remaining 258 pages were read only as extracted text and are **not** claimed as visually inspected. No page is claimed as individually inspected that was not. |
| 8 | Build coverage | **PASS** | The complete solution builds: 9 projects, 0 errors, 0 warnings, including the WPF presentation project and its test project. Executed on Windows with the .NET 8 desktop runtime present. |
| 9 | Test coverage and count reconciliation | **PASS** | 3 184 tests discovered and 3 184 executed: 3 182 passed, 0 failed, 2 skipped. Per project: domain 585, application 1 846, infrastructure 519, persistence 13 passed and 2 skipped, presentation 219. Discovered equals executed; the two skips are reported as skipped and are never counted as passed. |
| 10 | Test safety gate | **PASS** | Every test project's configuration, fixtures and base classes were inspected before running. The domain, application and presentation projects use fakes only. The infrastructure project uses the in-memory provider with generated database names, plus two files that build a context with the SQL Server provider **for model metadata only** and never open a connection — both were read in full before running. The persistence project uses an ephemeral container or model-only contexts and skips cleanly when the engine is unavailable. No test could reach a database holding application data. |
| 11 | Migration inspection | **PARTIALLY VERIFIED** | The ordered migration set, the EF configurations, the entity definitions and the model snapshot were inspected. 22 migrations confirmed. That the chain applies cleanly to an empty database was **not** verified: the single test that proves it was skipped because no container engine is installed. Every migration decision in Section 5 is therefore justified from configuration and snapshot evidence, not from execution. |
| 12 | Phase dependency ordering | **PASS** | Every ordering decision is justified by a named code dependency — a shared table, a shared writer, a shared content record or a page-geometry prerequisite — and the dependency map in Section 7 states each chain. No ordering is justified by conceptual similarity or by the order in which the gaps were found. |
| 13 | One-migration-per-phase compliance | **PASS** | 20 phases, at most one migration each. Phases 9 and 10 are kept separate precisely so that the table they share is altered once and its key altered once. |
| 14 | Acceptance criteria | **PASS** | Every phase carries numbered, observable Given/When/Then criteria covering normal operation, invalid input, boundary conditions, permissions, persistence and state transitions, UI reachability and regression protection. |
| 15 | Open owner decisions | **PASS** | 8 decisions recorded, each with the question, the evidence and its limits, the options, the consequences, the operational and security implications, a recommendation, and whether it blocks a phase. No decision is left unstated and no decision is taken on the owner's behalf. |
| 16 | IMPLEMENTED-function coverage | **PARTIALLY VERIFIED** | 146 IMPLEMENTED rows. Tier-level check: every handler class cited as evidence was resolved mechanically against the whole `src` tree and every one of them exists; the whole solution builds and the whole suite passes, which establishes reachability of the compilation and registration surfaces. Deep trace: 20 functions were traced end to end from a user-facing entry point through the handler, validation, domain logic and persistence or rendering terminus — sign-in and sign-out, secondary-password gating, patient creation and its medical history, test removal and clear-all, the payment and extra-charge commands, culture result persistence with antibiotic filtering, the reference-range match and print-time refresh, the printed-state recorder across all print paths, the banded result monitor, the profile-result colour precedence, the receipt writer's logo and words-header branch, the barcode service, the report composer, the permission catalogue and the inventory access policy, the settings configurations, the database maintenance commands, the utility converters and the backup service. The remaining 126 rest on the mechanical existence check, the build and the suite; no runtime observation was possible. |
| 17 | Blocked items | **PASS** | 3 blocked rows, each with evidence, the boundary reason, and the single decision that would release it. None appears as a phase and none carries a migration. |
| 18 | Cleanliness of the plan | **PASS** | No Git commit hash appears in the body. The header states the baseline commit once. No prior audit report, forensic report, inventory or execution plan is named or cited. Only this plan's own identifiers are used. Reference citations use only the two permitted forms. Rollback and change-inspection instructions are written generically and name no version-control operation, command, commit, branch or history. |
| 19 | Honesty about unverified claims | **PASS** | The header states what was not verified and why. No runtime status is claimed. The two skipped tests are reported as skipped. The build is reported as establishing buildability only. The visual-inspection coverage distinguishes individual renders from contact sheets. The conditional migration decisions are stated as conditional and gated on an executed check rather than guessed. |
| 20 | Repository preservation | **PASS** | Exactly one file was created inside the solution folder: this plan. No existing file was modified, renamed, moved or deleted. No commit, push, branch, tag, worktree, remote or stash operation was performed. No database was reached and the application was never launched. Builds and tests ran against a copy outside the solution folder. |

**Summary: 17 PASS, 3 PARTIALLY VERIFIED, 0 FAIL.**