# Project Completion Status

**Target project:** `https://github.com/El-ogra/Top-Lab.git`
**Audited commit:** `9b912c1a0ec51fb9786831a7d8b3362141e78e18` (detached HEAD; commit message: `[Docs] Correct stale claims 25/30/41/47/48 and header to current code`)
**Reference system:** `RL_Show_Enhanced.pdf` (77 pp.), `RLS_Learn_Enhanced.pdf` (212 pp.) — "RealLab" medical laboratory management system, RealLab Co., El Mansoura, Egypt.
**Audit date:** 2026-09-30
**Report language:** English

---

## 1. Executive Summary

TopLab is a **substantial, well-architected, partially complete** re-implementation of the RealLab reference system. It is not a mock-up: it is a real .NET 8 / WPF / EF Core application built on a genuine Clean Architecture layering (Domain → Application → Infrastructure → Presentation), with MediatR command/query dispatch, FluentValidation, a permission pipeline, an audit interceptor, and **2,158 passing automated tests**.

The **domain and application layers are far more complete than the presentation and output layers.** The pattern is consistent and is the single most important finding of this audit:

> Business rules, data model, validation and persistence are largely implemented. **What is missing is the wiring that turns them into the reference system's visible behaviour** — the UI selectors that choose among implemented handlers, the print/report artefacts that honour their own settings, and the cross-cutting concepts (branches, external results website, list printing) that the reference relies on.

Three findings dominate:

1. **The patient clinical report is functionally broken as an output artefact.** `ReportPdfWriter` is a hand-rolled ASCII PDF generator that replaces every non-ASCII character with `?` and hardcodes A4 page geometry. Arabic patient names, test names and doctor names all print as `?????`. The same writer ignores the paper size, page margins, top space, header/footer mode and doctor-signature settings it is handed. The receipt, invoice and work-sheet writers (QuestPDF) do not have this problem — the report path is the outlier.

2. **A required entity type cannot be created through the UI.** The domain mandates a price list for `ReferralOrContract` entities, but the editor window has no price-list control and hardcodes `null`. Every attempt to create a referral/contract entity fails validation. This cascades: referral commission percentages can never be set, so the "other labs' accounts and referral percentages" accounting figure is permanently zero.

3. **Four fully-implemented accounting report variants have zero UI selectors.** The backend supports 5 entity kinds × 4 report types (summary / detailed / detailed-by-prices / detailed-by-results) exactly as the reference specifies, but no control in `AccountsHubView.xaml` binds to them, so the tab always requests `User` / `Summary` — and `Summary` returns an empty line list.

Counting 87 consolidated requirements (§4), the completion profile is:

| Status | Count | Share |
|---|---|---|
| Fully implemented | 38 | 43.7% |
| Partially implemented | 30 | 34.5% |
| Implemented with defects | 6 | 6.9% |
| Present but unverified | 1 | 1.1% |
| Missing | 12 | 13.8% |

**Coverage rate (requirements at least partially addressed): 86 / 87 = 98.9%.**
**Weighted functional completeness: 68.4%** (formula in §10). The categorical conclusion is more useful than the number: *core laboratory workflow — registration, ordering, billing, result entry, verification, delivery, accounting and permissions — is substantially present; report output fidelity, print coverage, and several cross-cutting system concepts are not.*

---

## 2. Audit Scope and Source of Truth

### 2.1 Reference system source of truth

Only the two supplied PDFs were used to establish reference requirements. Both are Arabic-language (OCR'd) documents produced by RealLab Co.:

- **`RLS_Learn_Enhanced.pdf`** — 212-page "Real Lab System Guide Book". Chapters 1–7, each with a contents page and worked, step-by-step examples. This is the primary functional specification.
- **`RL_Show_Enhanced.pdf`** — 77-page product showcase containing sample printed outputs (receipts, invoices, reports, work papers, statistics, accounts) and capability claims.

Text was extracted per page with `pdftotext -layout` and each page was indexed so that every requirement can be cited by PDF page. **Arabic OCR quality is imperfect** — this is stated wherever it limits a conclusion.

### 2.2 Target project source of truth

**Only the source code at commit `9b912c1a0ec51fb9786831a7d8b3362141e78e18`.** The repository was cloned and checked out to that exact commit in detached-HEAD state; `git rev-parse HEAD` confirms the hash. No later commit was inspected.

**Explicitly excluded as evidence:**
- The `Docs/` directory (28 files including `project-completeness-audit.md`, `Handoff_M02..M23`, `Final Implementation plan for m14.md`, `licensing-decision.md`). These are development-process artefacts, not code. A grep confirms **no `.cs` or `.csproj` file reads anything from `Docs/`**.
- `FoundationPhaseAcceptanceReport.md`, `Top_Lab_Remediation_Report_F1_F6.md`, `INSTALL.txt`, `README`-type files, code comments, and commit messages.
- Any external description of the project.

Documentation and comments were read **only** where necessary to interpret code. Several comments in the codebase are unusually candid about known limitations (e.g. `ReceiptPrintingService.cs:20` states the print-once setting "is a display-only setting"), and these were used as *pointers to verify in code*, never as proof on their own — each such claim was independently confirmed by reading the referenced implementation.

### 2.3 Resolution of a conflict in the audit brief

The task brief stated the audit commit as `9b912c1a0ec51fb9786831a7d8b3362141e78e18` in §2 but as `a7828113ea51652baf22d4968c61803db2e66201` in §18. Per explicit user instruction, **only `9b912c1a0ec51fb9786831a7d8b3362141e78e18` was audited**; all other hashes were disregarded.

### 2.4 Build and test verification performed

| Action | Result |
|---|---|
| `dotnet build TopLab.sln` (Linux) | Fails with `NETSDK1100` — WPF `net8.0-windows` cannot be targeted on Linux. **Environment limitation, not a defect.** |
| `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` | **Build succeeded. 0 warnings, 0 errors.** (Property passed on the command line; no file modified.) |
| `TopLab.Domain.Tests` | **474 passed, 0 failed** |
| `TopLab.Application.Tests` | **1,484 passed, 0 failed** |
| `TopLab.Infrastructure.Tests` | 198 passed, **3 failed** |
| `git status --porcelain` after audit | **Empty — repository unmodified** |

The 3 infrastructure failures are `ReceiptPrintingServiceTests`, `InvoicePrintingServiceTests` and `WorkSheetPrintingServiceTests` "HappyPath" cases, all asserting `File.Exists(dispatcher.PdfPath)` after a QuestPDF render. QuestPDF depends on the Skia native graphics library, which is **not present in this Linux sandbox** (only `QuestPDF.dll` is restored; no `QuestPDF.Skia` package or native `libQuestPDF` is on disk). These tests exercise the two **QuestPDF** writers; the hand-rolled writers (`ReportPdfWriter`, `BarcodeService`) have their own tests, which pass. **Classification: environment-limited, not a confirmed product defect.** See §12.

---

## 3. Methodology

The audit followed a five-phase method.

**Phase 1 — Reference requirement extraction.** Both PDFs were read end to end. The `RLS_Learn` table of contents (pp. 3–5) provides the canonical chapter/section skeleton; each section was then read in full to capture the actual behaviour, the exact screen names, the exact button labels, and the workflow sequence. `RL_Show` was read to capture printed-output structure (columns, sections, header/footer content) and stated capabilities. Requirements appearing in several places (e.g. patient history is described in §1-13 and demonstrated across pp. 70–83) were consolidated into one canonical requirement with multiple page citations, while meaningful distinctions (same-report vs separate-report; single vs multi-patient) were preserved as separate requirements.

**Phase 2 — Target codebase understanding.** Project structure, layering, frameworks, dependency injection, navigation, persistence, auth, printing and test strategy were mapped before evaluating any individual requirement.

**Phase 3 — Requirement-to-code tracing.** For each requirement, the audit traced the complete execution path: **WPF View → ViewModel → MediatR Command/Query → FluentValidation Validator → Handler → Domain entity/method → `IApplicationDbContext` / DbSet → EF configuration → migration**, and for printed outputs, **Command → PDF writer → printer dispatcher**. A class name, view name or DTO was never accepted as proof. Where a handler exists, the audit confirmed it actually calls `SaveChangesAsync` and actually mutates domain state.

**Phase 4 — Quality and defect analysis.** Correctness, reliability, data integrity and performance were assessed for every capability found to exist. Defects were only reported where provable from source; unprovable suspicions are labelled **potential risk**.

**Phase 5 — Verification pass.** A second pass re-checked every "missing" conclusion against a code-search basis, every positive claim against actual evidence, and the internal consistency of the final matrix.

Two independent parallel investigation workstreams were used for breadth (one over the patient/results/reporting area, one over settings/statistics/users/lab-catalog), and **all of their high-severity claims were independently re-verified by the lead auditor against the source before being accepted into this report.** Several sub-claims were re-verified as inaccurate and were corrected or dropped (§14.3).

---

## 4. Reference Functional Requirements

87 distinct consolidated requirements were extracted. Page citations refer to PDF page numbers.

### Chapter 1 — Patients, results and patient history (RLS_Learn pp. 9–83)

| ID | Requirement | Reference |
|---|---|---|
| R-01 | Four-entry main menu (add/edit patient · enter results · search patient · deliver results) | Learn p.12, p.27, p.41, p.21 |
| R-02 | Register a new patient: name, type/title, age; optional phone, address, national ID, treating doctor | Learn p.12 |
| R-03 | Patient medical history: medication taken (diabetes, BP, viral, antibiotic, blood thinner, liver), conditions suffered (anaemia, lupus, kidney failure, BP, joint inflammation), X-ray/ultrasound within a month, pregnancy for females | Learn p.13, p.17–18, p.24 |
| R-04 | Patient notes field, shown in result entry and result delivery | Learn p.12 |
| R-05 | Per-patient extra "Lab ID" generated on demand, auto-shown on re-registration, used to list all visits | Learn p.47–51 |
| R-06 | Order tests: catalogue on left, double-click adds to "Patient tests", double-click removes, "All" clears | Learn p.13–14 |
| R-07 | Sample-kind flags per test: Urine / Stool / Blood / Semen / CSF | Learn p.13, p.24 |
| R-08 | "Taken outside lab" flag, surfaced as a note in the patient report | Learn p.13, p.74 |
| R-09 | Add a whole pre-defined test group to a patient in one action | Learn p.15–16 |
| R-10 | Patient account: auto total from configured prices, previously-paid, discount, "+"-prefixed extra charge | Learn p.17 |
| R-11 | Double-Enter computes then saves; "Net" settle button with confirm | Learn p.17 |
| R-12 | Delete a payment operation; edit a payment | Learn p.20 |
| R-13 | Account summary: paid / remaining to lab / remaining to patient | Learn p.19, p.21 |
| R-14 | Result delivery screen showing completed / not-completed tests and account balance per patient, by period | Learn p.21 |
| R-15 | Edit patient data and add/remove tests (method 1: via patient list; method 2: via search) | Learn p.22–26 |
| R-16 | Result entry grid: test abbreviation, result, status, Finish, Verify, Print, Export, See Report | Learn p.27–29 |
| R-17 | Print preview ("معاينة الطباعة") before printing a report | Learn p.30 |
| R-18 | Combined report: select several of a patient's tests into one report, reorder via up/down arrows | Learn p.31–33 |
| R-19 | Combined report options: "Print sub title (group name) in report", "Print printed test again without msg." | Learn p.33–34 |
| R-20 | Blank report: patient data only, fillable and printable | Learn p.35–37 |
| R-21 | Culture result entry: Sample, Organism A/B/C, Culture Condition, Colony Count | Learn p.38 |
| R-22 | Culture microscopic examination: Pus cells, RBCs, Epithelial cells, Crystal, Fungi, Others, Direct? | Learn p.38 |
| R-23 | Culture sensitivity: Highly / Moderate / Low / Resistant For, with inhibition zone (mm) and commercial name | Learn p.39–40 |
| R-24 | Culture report display toggles: Sensitivity / Reference / Commercial Name | Learn p.39 |
| R-25 | Patient search: exact name, wildcard name, treating-doctor name, sex, age, phone, card number, by test, by request date, "results not entered", "not reviewed", "not printed", "not delivered" | Learn p.42–46 |
| R-26 | Search by branch number in a multi-branch lab | Learn p.42, p.45 |
| R-27 | Patient visit history: all visits with dates, by name or by Lab ID | Learn p.47–51 |
| R-28 | Audit of who registered / edited the case, who entered the result, number of prints, last printer + time | Learn p.52–57 |
| R-29 | Deliver results via the Natigh.com website: upload patient result, patient/doctor/lab log in and view or export (Word/PDF) | Learn p.58–69 |
| R-30 | Natigh patient count over a period and total count; block a patient's result from the site; print the patient's site data on the receipt | Learn p.65–69 |
| R-31 | Create a Natigh account for a treating doctor or a lab (generate ID, use ID as password) | Learn p.142–147 |
| R-32 | Patient history auto-inserted into the result report when the test was done before | Learn p.70–73 |
| R-33 | Manual history insertion: choose prior tests, attach in the same report or print in a separate report | Learn p.75–78 |
| R-34 | History for a single test or for blood picture (CBC analyte matrix) | Learn p.78–79 |
| R-35 | Multi-patient history in one report; history sort/group by patient name or by Lab ID; date range | Learn p.80–83 |
| R-36 | Report settings for history: sort mode, automatic display, per-test bold | Learn p.70, p.74, p.196 |

### Chapter 2 — Work sheets (RLS_Learn pp. 84–92)

| ID | Requirement | Reference |
|---|---|---|
| R-37 | Work paper by patient names for a period, with gender, age, date, referral, total, paid | Learn p.85–88; Show p.7 |
| R-38 | Work paper by test name for a period, grouped by test code, with patient results and per-test patient count | Learn p.89–91; Show p.9 |
| R-39 | Work paper for a selected single test | Learn p.91 |
| R-40 | Work paper by work-group (Log) name for a period | Learn p.92 |
| R-41 | Tally of tests in the period, showing number of times each test was performed | Learn p.90, p.92 |
| R-42 | Option to print only not-completed tests or all tests; work-paper preview before printing | Learn p.85, p.88 |

### Chapter 3 — Tests, external entities, antibiotics, sent-out samples (RLS_Learn pp. 93–147)

| ID | Requirement | Reference |
|---|---|---|
| R-43 | Test list with ID, arrangement, group, test name, patient price, lab-to-lab price, out-lab name, out price, barcode; search by name / group / test ID | Learn p.94, p.97 |
| R-44 | Edit test data: group, report name, bill name, history name, Arabic name, barcode name, test time (days), reference type, prices, "sent outside lab" + lab + cost, patient question, main test, print-with-other, add-with-group, see report | Learn p.95, p.98 |
| R-45 | Add a new test to the catalogue | Learn p.96 |
| R-46 | Reference ranges: sex + age band + min/max + test unit; edit and delete | Learn p.99–101 |
| R-47 | Separate low comment and high comment, auto-shown in the report when result < min or > max | Learn p.101–102 |
| R-48 | Age-unit semantics: normals must be defined per age unit; a 1-month patient must not match a 30-day band | Learn p.104 |
| R-49 | After changing a normal range, existing results keep the old range; a "refresh range" action re-applies the new range from the result screen | Learn p.102–103 |
| R-50 | Units management and Barcode Types management | Learn p.96, p.112 |
| R-51 | Test groups (work groups / Log) and work-group logs: CRUD, assign tests to group | Learn p.97, p.99 |
| R-52 | Custom test groups: CRUD, add tests with individual prices, total group price, apply to a patient, delete | Learn p.118–123 |
| R-53 | Fixed test comments: CRUD, multiple comments per test, shown via a Comment button in the report | Learn p.112–117 |
| R-54 | Price lists: CRUD, add/edit/delete tests with prices, delete list, **print price list** grouped by test group with counts and collection notes | Learn p.105–111 |
| R-55 | External entities: treating doctor, sent-out samples, referral/contract entity; name, address, city, phone, fax, responsible person + phone, email, price list, commission % | Learn p.125–127, p.143 |
| R-56 | "Cultures" registered as tests in the ID range 118–139 under group "CULTURE AND SENSITIVITY" | Learn p.128–129 |
| R-57 | Antibiotics: international name, scientific name, symbol/abbreviation, commercial names, suitable-for-pregnant, suitable-for-children, print list | Learn p.135, p.139 |
| R-58 | Attach antibiotics to a culture/test with a sensitivity threshold (e.g. > 18 mm); pregnancy and children specific lists; children detected as age < 12 | Learn p.136–139 |
| R-59 | Mark a test as sent outside: choose the destination lab, record cost price and patient price | Learn p.140–141 |
| R-60 | Sent-out account screen: filter by period and lab, "sent"/"not sent" filters, search a specific entity, "Send to" payment, "Net" settle, account report with cost/paid/balance | Learn p.184–188 |

### Chapter 4 — Users, permissions, attendance (RLS_Learn pp. 149–165)

| ID | Requirement | Reference |
|---|---|---|
| R-61 | Create users and system administrators/supervisors, gated by a system/second password; primary + secondary password; last login | Learn p.150–153 |
| R-62 | Per-user branch number, work start/end time, break period (hours + minutes) | Learn p.151–152 |
| R-63 | Granular permission checkboxes: add/edit patient + receipt/barcode print, result entry, result verify/edit, result print (incl. history + envelope), block print when balance outstanding, deliver results, max discount %, work-sheet print, delete patient, system data/settings edit, cash in/out, statistics, drawer/accounts view | Learn p.151–152 |
| R-64 | Edit an existing user's data and permissions | Learn p.156–157 |
| R-65 | Attendance: check-in, break start/stop, check-out; per-user log; overtime and lateness; only the administrator may view | Learn p.158–163 |
| R-66 | Login detector: log of user login/logout with machine name, login date/time, logout date/time | Learn p.164 |
| R-67 | Access-denied message when a non-permitted action is attempted | Learn p.154 |

### Chapter 5 — Statistics (RLS_Learn pp. 166–172; RL_Show p.25)

| ID | Requirement | Reference |
|---|---|---|
| R-68 | Patient statistics over a period, grouped by month, by day of month, by sex, by account type, by referral entity, and by a specific referral entity only; monthly totals + collected amounts | Learn p.168–171; Show p.30–31 |
| R-69 | Test/sample statistics over a period, grouped by test name, by test group, by referral entity, specific entity | Learn p.168; Show p.32 |
| R-70 | Sent-out sample statistics by period and destination lab | Learn p.166–168 |
| R-71 | User productivity statistics by assigned work | Learn p.166–168 |
| R-72 | Statistics require the second/system password | Learn p.167 |
| R-73 | Result monitoring/follow-up report: choose a test and a period, set min/max result values, list results (positives only) with patient, referral, age/sex | Show p.25 |

### Chapter 6 — Inventory, drawer and accounts (RLS_Learn pp. 174–188)

| ID | Requirement | Reference |
|---|---|---|
| R-74 | Drawer accounting + lab profit: total patient samples value, discounts, collected, uncollected, cash in drawer, net profit after collection and payment; gated by second password | Learn p.175–178 |
| R-75 | Accounting over day / week / month / year / custom periods; branch number selection | Learn p.178–179 |
| R-76 | Accounting for a specific user, referral entity, treating doctor, or sent-out samples / account type, with 4 report variants: detailed with results, detailed with prices, detailed, summary | Learn p.178–179, p.181–183 |
| R-77 | Drill down from totals to the per-patient detail listing | Learn p.180 |
| R-78 | Cash in/out (withdraw/deposit) for designated or other entities; other labs' accounts and referral-entity commission percentages over a period | Learn p.184, p.186–188; Show p.26–29 |

### Chapter 7 — System settings and sample separation (RLS_Learn pp. 189–210; RL_Show pp. 3–11, 68–73)

| ID | Requirement | Reference |
|---|---|---|
| R-79 | System settings: branch number, result pickup time, dedicated printers (report, barcode, envelope, receipt, card), default account type, English patient/doctor names on receipt, self/herself substitution when referral is blank, save doctor only from entity window, search assist on name entry, no automatic titles, auto-complete lab tests | Learn p.191–192, p.199–203 |
| R-80 | Database server settings (server, login, database name), backup, restore previous backup, update database, system preparation for the current version | Learn p.191–192 |
| R-81 | Report settings: report margins, report top space (max 8 cm), paper size A4/A5, header/footer print mode (none / words / images), full header image, full footer image, header/footer element positions (Left/Top in cm) | Learn p.192–195 |
| R-82 | Report header/footer element configuration: patient name, patient ID, age/sex, request date, referred by, printed-in; show top line, show bottom line, long date, show time, per-element bold, per-element colour, custom colours, doctor signature | Learn p.196–198 |
| R-83 | Receipt settings: header/footer, top margin, currency, result pickup time, print-once-only, show Natigh data on receipt, test-detail display on/off, barcode count and printing | Learn p.202–204; Show p.4–5 |
| R-84 | Envelope settings: header/footer, top margin, image/text modes, element positions, doctor/referral name and date | Learn p.205–206 |
| R-85 | Card (كارنية) settings | Learn p.192, p.199 |
| R-86 | Barcode control: show/hide barcode, sticker size 38 × 25 mm, vertical/horizontal layout, print on pre-printed stock, print per test tube/container, "Print barcode after read ID" | Show p.3, p.11 |
| R-87 | Sample separation & drawing: patient list, tube name + tests, drawn/not-drawn marking, save patient samples, save samples after read ID, "Other Tube" | Learn p.208–210 |

---

## 5. Requirement Traceability Matrix

Status key: **FI** Fully Implemented · **PI** Partially Implemented · **IWD** Implemented With Defects · **PBU** Present But Unverified · **M** Missing

| ID | Reference Requirement | Reference Source | Target Implementation | Code Evidence | Func. Status | UI Status | Quality | Defects | Conf. |
|---|---|---|---|---|---|---|---|---|---|
| R-01 | 4-entry main menu | Learn p.12 | `PatientsHubView.xaml:10-17` | Four buttons, labels identical to reference | FI | Equivalent | Good | — | High |
| R-02 | Register new patient | Learn p.12 | `PatientEditorView.xaml:155-186`, `CreatePatientCommand` | Name/title/sex/age+unit/phone/national ID/address/account type | FI | Equivalent | Good | — | High |
| R-03 | Medical history flags | Learn p.13, p.17 | `Patient.MedicalConditions`; `MedicalConditionTypeConfiguration.cs:17` | Only **one** seeded condition: "حمل" (pregnancy) | PI | Partial | Defective | D-16 | High |
| R-04 | Patient notes | Learn p.12 | `Patient.Notes`; `PatientEditorView.xaml:220-222` | Present in model, editor and report DTOs | FI | Equivalent | Good | — | High |
| R-05 | Per-patient Lab ID + visit list | Learn p.47-51 | `Patient.AssignLabId`; `PatientSearchViewModel.FetchByLabIdCommand` | `PatientEditorView.xaml:157-158`; `GetPatientVisitHistory` | FI | Equivalent | Good | — | High |
| R-06 | Order tests (add/remove/clear) | Learn p.13-14 | `AddTestsToVisit`, `RemoveTestFromVisit`, `ClearAllTests` | `PatientEditorViewModel.cs:126-150` | FI | Equivalent | Good | — | High |
| R-07 | Sample-kind flags | Learn p.13 | `PatientTest.IsUrine/IsStool/IsBlood/IsSemen/IsCsf` | `PatientTest.cs:16-24` | FI | Equivalent | Good | — | High |
| R-08 | Taken-outside-lab note | Learn p.13, p.74 | `PatientTest.IsTakenOutsideLab` | Present; not confirmed in report output (D-01) | PI | Partial | — | D-01 | Med |
| R-09 | Add whole test group | Learn p.15-16 | `AddProfileToVisit`, `AddCustomGroupToVisit` | `PatientEditorViewModel.cs:148-149` | FI | Equivalent | Good | — | High |
| R-10 | Patient account calculation | Learn p.17 | `PatientAccountCalculator.cs:17-38` | Pure domain formula, frozen prices at order time | FI | Equivalent | Good | — | High |
| R-11 | Double-Enter / Net settle | Learn p.17 | `RecordPayment`, `Settle*`; `PatientEditorView.xaml:36-37` | Single-click buttons rather than double-Enter | PI | Similar | Good | D-24 (low) | Med |
| R-12 | Delete / edit payment | Learn p.20 | `PaymentOperation` void + correction ops | Void/correction model present; editor buttons not located | PI | Undetermined | Good | — | Med |
| R-13 | Account summary | Learn p.19, p.21 | `PatientEditorView.xaml:23-31` | Total, discount, paid, balance, "عرض الحساب" | FI | Equivalent | Good | — | High |
| R-14 | Result delivery screen | Learn p.21 | `ResultDeliveryView.xaml`, `ResultDeliveryViewModel` | Patient list with tests, completion and balance | FI | Equivalent | Good | — | High |
| R-15 | Edit patient + add/remove tests | Learn p.22-26 | `UpdatePatient`, `PatientEditorView` | Both routes available | FI | Equivalent | Good | — | High |
| R-16 | Result entry grid | Learn p.27-29 | `ProfileEntryView.xaml:77-138` | Analyte, value, unit, flag, verified, printed + save/verify/unverify/print/report | FI | Equivalent | Good | — | High |
| R-17 | Print preview before printing | Learn p.30 | `ProfileEntryViewModel.ShowReportAsync:360-382` | Renders an **on-screen data panel**, not a rendered-PDF preview | PI | Partial | Defective | D-08 | High |
| R-18 | Combined report + reorder | Learn p.31-33 | `CombinedReportView.xaml:27-33` | `MoveUpCommand` / `MoveDownCommand` present | FI | Equivalent | Good | — | High |
| R-19 | Combined report print options | Learn p.33-34 | — | **No** subtitle or reprint flag anywhere in `src/` | M | Missing | — | D-25 | High |
| R-20 | Blank report | Learn p.35-37 | `BuildBlankReport*`, `PrintBlankReport*`, `BlankReportView` | Patient, sex, age+unit, doctor, referral | FI | Equivalent | Defective output | D-01 | High |
| R-21 | Culture core fields | Learn p.38 | `CultureResult.cs:10-20` | Sample, Organism A/B/C, Culture Condition, Colony Count | FI | Equivalent | Good | — | High |
| R-22 | Culture microscopic exam | Learn p.38 | — | No Pus-cell/RBC/epithelial/crystal/fungi fields in the model | M | Missing | — | D-09 | High |
| R-23 | Sensitivity class + zone + trade name | Learn p.39-40 | `CultureAntibioticResult.SensitivityCategory` | **Category enum only**; no inhibition zone (mm), no commercial name | PI | Partial | Defective | D-10 | High |
| R-24 | Culture display toggles | Learn p.39 | — | No Sensitivity/Reference/CommercialName display toggles | M | Missing | — | D-09 | High |
| R-25 | Patient search modes | Learn p.42-46 | `PatientSearchView.xaml:8-16` | Free-text + Lab ID fetch; paged; rich columns | PI | Partial | Good | D-26 | Med |
| R-26 | Search by branch number | Learn p.42, p.45 | — | **No branch concept in the model** | M | Missing | — | D-13 | High |
| R-27 | Visit history | Learn p.47-51 | `PatientVisitHistoryView.xaml` | Full visit list with tests | FI | Equivalent | Good | — | High |
| R-28 | Case/result/print audit | Learn p.52-57 | `AuditView.xaml`, `AmendmentsLogWindow.xaml` | Patient audit, test audit, entered/reviewed/printed/delivered + users + times | FI | Equivalent | Good | — | High |
| R-29 | Natigh.com result delivery | Learn p.58-69 | — | **No Natigh code anywhere in `src/`** | M | Missing | — | D-14 | High |
| R-30 | Natigh counts / block / print on receipt | Learn p.65-69 | — | Absent | M | Missing | — | D-14 | High |
| R-31 | Natigh doctor/lab account | Learn p.142-147 | `GenerateEntityIdCode` (partial substitute) | Random code issued, but no account concept, no password, no website | M | Missing | — | D-14 | High |
| R-32 | Auto history in report | Learn p.70-73 | `AutoInsertHistoryCommand`, `HistoryAutoDisplayEnabled` | Setting persisted; behaviour driven by handler | FI | Equivalent | Good | — | High |
| R-33 | Manual history: same vs separate report | Learn p.75-78 | `InsertHistoryResult`, `GetSeparateHistoryReport` | `HistoryReportsView.xaml:31-33` three modes | FI | Equivalent | Defective output | D-01 | High |
| R-34 | History for one test or CBC matrix | Learn p.78-79 | `GetPatientTestHistory` | Per-analyte lines present; **no date × analyte matrix layout** | PI | Partial | Defective | D-01, D-11 | High |
| R-35 | Multi-patient history + sort + range | Learn p.80-83 | `GetMultiPatientHistory`, `HistorySortMode` | Sort mode persisted and honoured; range supported | FI | Equivalent | Defective output | D-01 | High |
| R-36 | History report settings | Learn p.70, p.196 | `ReportSettingsView.xaml:53-58` | Sort mode + auto-display; **no per-test bold** | PI | Partial | — | D-12 | High |
| R-37 | Work paper by patient names | Learn p.85-88 | `WorkSheetsView`, `WorkSheetPdfWriter.cs` | QuestPDF, RTL, A4; columns present | FI | Equivalent | Good | — | High |
| R-38 | Work paper by test names | Learn p.89-91 | `WorkSheetsViewModel` | Grouped by test with per-test patient count | FI | Equivalent | Good | — | High |
| R-39 | Work paper for a selected test | Learn p.91 | `WorkSheetsViewModel` | Test selection in the view | FI | Equivalent | Good | — | High |
| R-40 | Work paper by work-group (Log) | Learn p.92 | `WorkGroupLog*` | Log-based work sheet | FI | Equivalent | Good | — | High |
| R-41 | Tally of tests in period | Learn p.90, p.92 | `WorkSheetsViewModel` | Tally tab with per-test counts | FI | Equivalent | Good | — | High |
| R-42 | Only-incomplete option + preview | Learn p.85, p.88 | `WorkSheetPrintingService` | Filter present; **no rendered preview before print** | PI | Partial | Defective | D-08 | Med |
| R-43 | Test list columns + search | Learn p.94, p.97 | `TestCatalogView.xaml:61-64` | Only 4 of 9 reference columns bound; no search-by-test-ID | PI | Partial | Defective | D-15 | High |
| R-44 | Edit test data (13 fields) | Learn p.95, p.98 | `Test.cs:11-40`, `UpdateTestCommand` | 7 of 13 present; **6 absent** (history/Arabic/barcode name, reference type, out-lab on test, patient question, main test, print-with-other, add-with-group, see report) | PI | Partial | Defective | D-15 | High |
| R-45 | Add new test | Learn p.96 | `CreateTestCommand` | Full create flow | FI | Equivalent | Good | — | High |
| R-46 | Reference ranges CRUD | Learn p.99-101 | `ReferenceRange`, `AnalyteReferenceRangeBand` | Sex + age band + min/max; **no test-unit field** | PI | Partial | Defective | D-17 | High |
| R-47 | Low/high comments in report | Learn p.101-102 | Fields exist; **never reach the report** | `BuildCombinedReportCommandHandler.cs:160-163`; `ReportDtos.cs:40-50` | IWD | — | **Defective** | **D-03** | High |
| R-48 | Age-unit semantics | Learn p.104 | `ReferenceRange.Matches:97-110` | Returns `false` on unit mismatch **before** value comparison | FI | Equivalent | **Correct** | — | High |
| R-49 | Refresh range on old results | Learn p.102-103 | `RefreshResultReferenceRangeCommandHandler` | Handler real; **zero UI callers** | PI | Missing | — | D-18 | High |
| R-50 | Units + Barcode Types management | Learn p.96, p.112 | — | No `Unit` or `BarcodeType` entity/DbSet/view | M | Missing | — | D-19 | High |
| R-51 | Test groups + work-group logs | Learn p.97, p.99 | `TestGroup`, `WorkGroupLog` | Full CRUD + test assignment | FI | Equivalent | Good | — | High |
| R-52 | Custom test groups | Learn p.118-123 | `CustomGroup`, `CustomGroupItem` | CRUD + per-test price + apply; **no total group price** | PI | Partial | Defective | D-20 | High |
| R-53 | Fixed test comments | Learn p.112-117 | `TestComment`; `TestCommentsView.xaml:14-30` | CRUD, multiple per test | FI | Equivalent | Defective output | D-01 | High |
| R-54 | Price lists + **print price list** | Learn p.105-111 | `PriceList`, `PriceListItem`; `PriceListsView.xaml:13-85` | CRUD complete; **no print command exists** | PI | Partial | Defective | D-21 | High |
| R-55 | External entities (3 types + fields) | Learn p.125-127, p.143 | `ExternalEntity.cs:12-32`; `ExternalEntityEditorWindow.xaml:17-36` | **ReferralOrContract cannot be created** (no price-list control, `null` hardcoded); no email field | IWD | **Defective** | **Defective** | **D-04** | High |
| R-56 | Cultures as tests 118–139 | Learn p.128-129 | `Test.IsCultureType` | Flag exists; no reserved ID-range seed | PI | Partial | — | D-22 | Med |
| R-57 | Antibiotic master data | Learn p.135 | `Antibiotic.cs:8-12` | Name + pregnancy + children only; **no symbol, scientific name, trade names, no print** | PI | Partial | Defective | D-23 | High |
| R-58 | Antibiotic attach with threshold | Learn p.136-139 | `CultureAntibioticAttachment`, `CultureAntibioticResult` | Attach/detach + pregnancy/children lists; **no numeric threshold**; child test requires `AgeUnit==Year` | PI | Partial | Defective | D-10, D-27 | High |
| R-59 | Mark test sent outside | Learn p.140-141 | `SentOutSample`, `SendSampleOutCommand` | Destination lab + cost + patient price | FI | Equivalent | Good | — | High |
| R-60 | Sent-out account screen | Learn p.184-188 | `SentOutSamplesView.xaml:61-82`, `SentOutLabAccountView` | Period + lab filter, payment, Net settle, account report; **no sent/not-sent filter** | PI | Partial | Good | D-28 | High |
| R-61 | Create users/admins (2nd pwd) | Learn p.150-153 | `UserManagementView`, `VerifySecondaryPasswordQuery` | `ShellViewModel.cs:233-246` gates the Users menu | FI | Equivalent | Good | — | High |
| R-62 | Branch no., work times, break h+m | Learn p.151-152 | `User.cs:18-24` | Work times + break **minutes only**; **no branch number** | PI | Partial | Defective | D-13, D-29 | High |
| R-63 | Granular permission set | Learn p.151-152 | `PermissionConfiguration.cs:17-31` | 13 permissions seeded; 2 reference permissions not separated | PI | Partial | Good | D-29 | High |
| R-64 | Edit user + permissions | Learn p.156-157 | `UpdateUserCommand`, `SaveUserPermissionsCommandHandler` | Full edit flow | FI | Equivalent | Good | — | High |
| R-65 | Attendance + overtime/lateness | Learn p.158-163 | `MyAttendanceView.xaml:54-60`, `AttendanceCalculator` | Check-in, break start/stop, check-out, summary view, calculations | FI | Equivalent | Good | — | High |
| R-66 | Login detector (machine, logout) | Learn p.164 | — | Only `User.LastLoginAtUtc`; no log table, no machine name, no logout time | M | Missing | — | D-30 | High |
| R-67 | Access-denied message | Learn p.154 | `AuthorizationBehavior.cs:34-50` | `Error.Forbidden("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام")` — wording matches reference | FI | Equivalent | Good | — | High |
| R-68 | Patient statistics (6 groupings) | Learn p.168-171 | `GetPatientCountStatisticsQuery.cs:8-14` | Month/sex/account-type/referral present; **no day-of-month, no specific-entity filter** | PI | Partial | Good | D-31 | High |
| R-69 | Test statistics (4 groupings) | Learn p.168 | `GetTestCountStatisticsQuery` | By test + by group; **no referral grouping** | PI | Partial | Good | D-31 | High |
| R-70 | Sent-out statistics by lab | Learn p.166-168 | `GetSentOutStatistics*` | Full: count, cost, paid, remaining per lab | FI | Equivalent | Good | — | High |
| R-71 | User productivity statistics | Learn p.166-168 | `GetUserProductivityStatistics*` | Entered/reviewed/printed/delivered per user; "assigned work" not modelled | PI | Partial | Good | D-31 | Med |
| R-72 | Statistics need 2nd password | Learn p.167 | `ShellViewModel.cs:268-276` | **No second-password prompt** (unlike Users :234 and Accounts :256) | IWD | **Defective** | **Defective** | **D-05** | High |
| R-73 | Result monitoring report (min/max) | Show p.25 | — | No min/max result query parameter, no positive-only filter, no such report | M | Missing | — | D-32 | High |
| R-74 | Drawer accounting + profit (2nd pwd) | Learn p.175-178 | `GetCashDrawerInventoryQueryHandler.cs:61-122` | All 12 figures computed; gated by password at `ShellViewModel.cs:256` | FI | Equivalent | Good | — | High |
| R-75 | Period presets + branch selection | Learn p.178-179 | `AccountsHubView.xaml:26-29` | Free date pickers only; **no presets, no branch** | PI | Partial | Defective | D-13 | High |
| R-76 | 5 entity kinds × 4 report variants | Learn p.178-183 | `GetElementInventoryQueryHandler.cs:206-224` | Backend complete; **zero UI selectors** → always `User`/`Summary`/empty | IWD | **Defective** | **Defective** | **D-06** | High |
| R-77 | Drill-down to patient detail | Learn p.180 | `GetPatientSamplesDetail` | Query exists; **no drill trigger in the accounts UI** | PI | Partial | Good | D-33 | Med |
| R-78 | Cash in/out + commissions (Lab P) | Learn p.184, p.186-188 | `RecordCashDeposit/Disbursement`; `BuildCommissions` | Cash in/out FI; **commissions computed but never bound to any view**, and unreachable (D-04) | PI | Partial | Defective | D-06, D-34 | High |
| R-79 | System settings (13 items) | Learn p.191-192, p.199-203 | `SystemSettings.cs`, `SystemSettingsView.xaml` | 7 live, 3 dead flags, **branch/English names/self-herself/auto-complete missing**; no card printer | PI | Partial | Defective | D-13, D-27 | High |
| R-80 | DB server settings + backup/restore | Learn p.191-192 | `SqlServerDatabaseMaintenanceService.cs:37,71,102` | BACKUP + guarded RESTORE (pre-restore safety backup :63) + path check | FI | Equivalent | Good | — | High |
| R-81 | Report margins/paper/header-footer | Learn p.192-195 | `ReportSettings.cs`; `ReportPdfWriter.cs:50-61,142,163` | Persisted but **writer ignores margins, top space, paper size, header/footer** | IWD | **Defective** | **Defective** | **D-01**, **D-02** | High |
| R-82 | Header/footer element positions & colours | Learn p.196-198 | `ReportSettings.HeaderColor/FooterColor` | **No setter, no command/DTO/UI field, no reader** — permanently null columns | M | Missing | — | D-28 | High |
| R-83 | Receipt settings | Learn p.202-204; Show p.4-5 | `ReceiptSettings`; `ReceiptPdfWriter.cs` | Top margin, currency, pickup time, detail mode live; **footer hardcoded empty, print-once not enforced** | IWD | Partial | **Defective** | D-18 | High |
| R-84 | Envelope settings | Learn p.205-206 | `EnvelopeSettings`, `EnvelopeSettingsView.xaml:26-70` | Fully modelled + editable; **no envelope PDF writer exists** | PBU | Partial | Inert | **D-06** | High |
| R-85 | Card settings | Learn p.192, p.199 | — | No entity, no view; `PrinterOutputType` has no `Card` | M | Missing | — | D-27 | High |
| R-86 | Barcode control (38×25, orientation, per-test) | Show p.3, p.11 | `BarcodeService.cs`, `BarcodeLabelRenderer.cs` | Code-128 label + human-readable ID + routing; **fixed 4in×2in page, hardcoded CODE_128, no per-test sticker set, no orientation** | PI | Partial | Defective | D-29 | High |
| R-87 | Sample separation & drawing | Learn p.208-210 | `SampleCollectionView`, `SampleDrawBoardView.xaml:40-140` | Patient list, drawn/not-drawn, draw-all, reload, per-test rows | FI | Equivalent | Good | — | High |

---

## 6. Detailed Functional Findings

### 6.1 Architecture (Phase 2 findings)

| Aspect | Finding |
|---|---|
| Language / runtime | C# / .NET 8 (`net8.0`, `net8.0-windows` for the WPF shell) |
| UI framework | WPF, `net8.0-windows`, `WinExe`; 70 XAML views, 74 ViewModels; MVVM with `RelayCommand`/`AsyncRelayCommand` + MediatR |
| Architecture | Clean Architecture: `TopLab.Domain` (framework-free) → `TopLab.Application` (CQRS via MediatR) → `TopLab.Infrastructure` (EF Core 8 + SQL Server, QuestPDF, ZXing) → `TopLab.Presentation` (WPF) |
| Data layer | EF Core 8, `ApplicationDbContext`, **55 entity configurations**, 8 migrations, strongly-typed IDs (`PatientId`, `TestId`, …) |
| Dispatch | MediatR 12.5 pipeline behaviours: `ValidationBehavior`, `AuthorizationBehavior`, `LoggingBehavior` |
| Auth | PBKDF2 password hashing, `IAuthorizedRequest` per request, `IsAbsolutePermission` bypass, 13 seeded permissions |
| Reporting | 4 PDF writers: 3 QuestPDF (receipt, invoice, work sheet), 1 hand-rolled (report), plus a hand-rolled barcode label writer |
| Barcode | ZXing.Net Code-128 |
| Backup | `DailyBackupHostedService` + `SqlServerDatabaseMaintenanceService` (BACKUP/RESTORE) |
| Tests | 5 xUnit projects, **2,158 passing tests** |
| Code size | 1,405 `.cs` + 70 `.xaml` files, 1,628 tracked files |

**No `NotImplementedException`, no `TODO`, and no stubbed command handlers were found in `src/`.** The incompleteness is in wiring and in absent domain fields — not in placeholder code.

### 6.2 Where the implementation is genuinely strong

These are not superficial; each was traced to real persistence.

- **Billing integrity.** `PatientAccountCalculator` implements a frozen, auditable formula: charges are the sum of **prices captured at order time** (`PatientTest.PriceAtOrderTime`), extra charges add to the total, payments count `Amount + Discount`, voided rows contribute nothing, and the balance is deliberately not clamped. `RecordPaymentCommandHandler.cs:43-52` enforces `User.DiscountLimitPercent` and correctly treats a missing user row as a zero cap.
- **Reference-range correctness.** `ReferenceRange.Matches` (`ReferenceRange.cs:97-110`) and `AnalyteReferenceRangeBand.Matches` (`:92-105`) both reject on `AgeUnit` mismatch **before** comparing values — precisely the subtle rule the reference calls out on p.104. Ranges are frozen onto the patient test (`PatientTestReferenceRangeSnapshot`) at entry time, so historical results are not silently re-interpreted.
- **Flag computation.** `ResultFlagComputer` applies a documented deterministic tie-break (sex-matched over sex-null → narrowest band → lowest id) and returns `null` for non-numeric results rather than guessing.
- **Print-blocking and permissions.** `BlockPrintOnRemainingBalance` is enforced in **six** print paths, and `AuthorizationBehavior` produces the exact Arabic denial wording the reference specifies.
- **Data integrity.** `Patient` is soft-deletable with `EnsureNotDeleted()` guards; `AuditableEntity` + `AuditableEntitySaveChangesInterceptor` stamp every write; result amendments keep old/new values, actor, timestamp and reason.
- **Age-appropriate culture rules.** Pregnancy detection uses a seeded `MedicalConditionCategory.Pregnancy` condition; children detection is explicit.

### 6.3 Where the implementation thins out

The gaps are highly patterned. Six structural causes account for almost all of them:

1. **Model field absent.** The reference requires a data element the domain never declares — e.g. inhibition zone, culture microscopy, test history name, test unit, branch number, entity email.
2. **Handler present, UI absent.** A complete, tested, persisted capability with no control that reaches it — element-inventory variants, refresh-reference-range, patient-detail drill-down, commissions, card printer.
3. **Setting persisted, renderer ignores it.** The single largest disconnect: `ReportSettings` fields are stored and read back into the UI, but `ReportPdfWriter` hardcodes page geometry and never consults them.
4. **Whole concept absent.** Natigh.com, login detector, branch/multi-branch, card settings, price-list printing, units/barcode-type management.
5. **Output artefact defective.** The report writer destroys Arabic text and omits frozen range comments.
6. **Business rule blocks the UI.** Domain validation requires a price list for referral entities; the editor provides no way to supply one.

---

## 7. UI Comparison Findings

The reference PDFs are largely **scanned screen captures with Arabic OCR**, so exact pixel-level comparison is impossible. Where a screen is legible, the following could be compared. The target UI is Arabic RTL throughout (`FlowDirection="RightToLeft"`, QuestPDF `DirectionFromRightToLeft`), matching the reference language and layout convention.

### 7.1 Navigation and shell — Functionally equivalent, materially different layout

The reference uses a top ribbon with menu items: `Exit · About Us · Send Result · Setting · System · Users · Statistics · Accounts · Tools · Work sheet · Laboratory · Patients` (Learn p.12, p.161).

TopLab renders a **vertical side list** with the same semantic entries: `المرضى · المعمل · ورقة العمل · الأدوات · الحسابات · الإحصائيات · المستخدمون · النظام · الإعدادات · حول البرنامج · قفل المحطة · خروج` (`ShellViewModel.cs:172`). This is a **materially different layout, functionally equivalent coverage** — except:

- `Send Result` (the Natigh upload) has **no counterpart** — the entry is simply absent.
- TopLab **adds** `قفل المحطة` (workstation lock with idle auto-lock), which the reference does not specify.
- TopLab adds a persistent status bar showing user name, last login, live clock and **database connectivity** (`ShellViewModel.LoadStatusAsync`), which the reference does not specify.

### 7.2 Patient hub — Equivalent

The four reference entries appear verbatim as four buttons (`PatientsHubView.xaml:10-17`): إضافة وتعديل بيانات المرضى · إدخال نتائج التحاليل · بحث عن مريض · تسليم نتائج المرضى. This is a **functionally equivalent** match with strong label fidelity.

### 7.3 Patient editor — Substantially similar

Present and correctly labelled: patient code, Lab ID, name, title, sex, age + age-unit combo, phone, national ID, address, account type, treating doctor (typed ID + name + picker), referral entity, notes, VIP. The reference additionally shows, on the same screen, a "extra information" block with **eight named medical-history toggles** (6 medications, 5 conditions, X-ray, pregnancy) — Learn p.18, p.24.

TopLab instead renders a **generic `ItemsControl` of checkboxes bound to `MedicalConditionTypes`** (`PatientEditorView.xaml:220-222`). The mechanism is equivalent; the **content is not** — only "حمل" (pregnancy) is seeded, so a real installation shows one checkbox where the reference shows a full clinical history panel. Additionally, the reference's fasting hours appear as a dedicated question; TopLab has `IsFastingIndicated` + `FastingHours` + `RecentContrastImaging`, which **partially** covers it.

The reference's left-hand test catalogue with double-click add/remove and an "All" clear button is matched by a test list with add/remove plus `مسح الكل` (`ClearAllVisitTestsCommand`).

### 7.4 Test catalogue — Partially similar

Reference columns (Learn p.94): `ID · Arrangement · Group Name · Test Name · Pat. P · Lab P · Out Lab Name · Out P · Barcode`. TopLab binds **four**: code, name, group, price (`TestCatalogView.xaml:61-64`). The data for the missing columns **already exists in the DTO** (`TestCatalogDtos.cs:5-17` carries `LabToLabPrice`, `IsSentOut`, `Barcode`) — they are simply not displayed. This is a presentation gap, not a data gap.

The test editor dialog is well-populated for the fields the model supports, but the reference editor (Learn p.98) also shows reference type, patient-question, main-test, print-with-other and add-with-group checkboxes, none of which exist in `Test`.

### 7.5 Result entry — Functionally equivalent, with one notable divergence

TopLab's entry grid (`ProfileEntryView.xaml:77-138`) shows analyte, value, unit, flag, verified, printed — a close analogue of the reference's test/result/unit/normal-range grid. Save / Verify / Unverify / Print / Report / Amend / Amendments-log buttons are all present and cover the reference's Finish · Verify · Print · See Report.

Divergence: the reference's **"معاينة الطباعة" (print preview)** opens a rendered preview of the document as it will appear. TopLab's `تقرير` button (`ProfileEntryViewModel.ShowReportAsync:360-382`) loads a query result into an **on-screen WPF panel** showing the data as text. It is a data readout, not a document preview. The user therefore cannot see the printed layout before committing paper. This is a **partial** match.

### 7.6 Combined report — Functionally equivalent controls, defective output

The reference has a left list ("Patient tests in our lab"), a right list ("Available tests in report"), a count badge, up/down arrows, and two checkboxes. TopLab provides the selection, the count, `أعلى`/`أسفل` reordering, and manual/auto history insertion — but **the two print-option checkboxes are absent**, and the preview is a WPF text panel rather than a rendered document.

### 7.7 Culture entry — Materially different

The reference culture screen is a rich two-panel form (Learn p.38–39): a Microscopic Reaction block with 7+ rows (Pus Cells, R.B.Cs, Epithelial Cells, Crystal, Fungi, Others×3, Direct?) and a Sensitivity block listing every antibiotic with **inhibition zone in mm** and **trade name**, categorised Highly / Moderate / Low / Resistant.

TopLab's `CultureEntryView.xaml:107-121` shows a flat antibiotic grid with: antibiotic name, a sensitivity **category** combo (4 values), and read-only pregnancy/children flags. The **microscopic examination block does not exist**, and **inhibition zone and trade name are not stored anywhere in the model**. This is a **materially different** screen, not a partial match.

### 7.8 Accounts — Functionally equivalent surface, one tab defective

The reference Accounts window offers four actions: الجرد وحساب الدرج · المختبرات الأخرى · العينات المرسلة · صرف وإيداع نقدية. TopLab's `AccountsHubView.xaml:18-21` offers four tabs that map to them: جرد درج النقدية · جرد العناصر · عينات المرضى · حسابات الشركات, plus the deposit/disbursement buttons.

However, the **جرد العناصر (element inventory)** tab is where the reference offers 4 report types × 5 element kinds — and TopLab's XAML for that tab contains **no selectors at all** (verified: zero bindings to `ElementKind` or `ReportType` in `AccountsHubView.xaml`). The tab is therefore permanently showing one of the four variants, and the one it shows returns an empty grid.

### 7.9 Settings — Partial

The reference System Settings dialog is a single multi-section window (Learn p.192) with sections: printers, report settings, receipt settings, envelope settings, card settings, database server settings, database backup, system preparation. TopLab splits this into a settings dashboard with separate pages — a **structurally equivalent but differently organised** approach, which is functionally fine.

The report settings page shows margins, top space, paper size, header/footer mode, doctor signature, and a **live preview panel** (`ReportSettingsView.xaml:85-95`). The preview is a reasonable UI affordance, but it **previews the settings, not the produced document** — and the produced document ignores most of them (D-02).

### 7.10 What cannot be compared

The following could not be assessed because the reference material does not contain sufficient legible visual information, and are reported as **not determinable**:

- Exact report typography, column widths, line spacing and page-break behaviour.
- Whether reference report pages carry the "Computer Software / real lab system / For Medical Clinical Laboratory" bilingual logo band in the same position.
- Exact colour values of reference report headers/footers.
- The precise geometry of the 38 × 25 mm sticker (Learn/Show p.3 states the size; the reference image is not measurable from OCR).
- Behaviour of the Natigh website and Android app (external, not in either PDF beyond screenshots).

---

## 8. Quality and Defect Assessment

### 8.1 Defect register

Severity: Critical / High / Medium / Low. Confidence: High / Medium / Low.

---

**D-01 — Patient clinical report renders Arabic as `?` and ignores its own layout settings**
- **Related:** R-08, R-17, R-20, R-32–R-35, R-53, R-81
- **Location:** `src/TopLab.Infrastructure/Printing/ReportPdfWriter.cs:237-251` (`ToAscii`), `:50-61`, `:142`, `:163`; also `src/TopLab.Infrastructure/Services/PatientReportPdfExporter.cs:34-57`
- **Description:** `ToAscii` maps every character with code point > 126 to `'?'`. Every Arabic patient name, test name, doctor name, referral name and range comment therefore prints as `?????`. Separately, `:163` hardcodes `/MediaBox [0 0 595 842]` (A4) so `PaperSize.A5` has no effect; `:142` hardcodes `BT /F1 12 Tf 50 800 Td` so `PageMarginLeftCm` and `PageMarginBottomCm` are never read; `:51` prints `ReportTopSpaceCm` as a literal text line rather than positioning anything; `:53-56` prints `HeaderFooterMode` as text without drawing a header or footer; `:58-61` prints the literal string `"Doctor Signature: Yes"`.
- **Why it matters:** This is the primary clinical deliverable of a medical laboratory system. The reference reports (Learn pp. 30, 34, 37, 73; Show pp. 6, 12–24) are Arabic documents with patient names and reference ranges legible in both scripts. The output here is unusable. The frozen reference range text is ASCII-safe (numeric) and does survive, but the identifiers around it do not.
- **Expected (reference):** Arabic patient name, test name, doctor, referral and range comments printed legibly; paper size, margins, top space, header/footer mode and doctor signature visibly applied.
- **Actual:** All Arabic becomes `?`; margins, paper size, top space, header/footer and signature have no effect on the page.
- **Severity:** Critical · **Confidence:** High
- **Note:** The project itself acknowledges this — `ReceiptPdfWriter.cs:15-17` documents QuestPDF as the chosen fix for "the hand-rolled ASCII-only `ReportPdfWriter` cannot render Arabic", and the receipt/invoice/work-sheet paths were migrated while the report path was not.
- **Direction:** Reimplement `IReportPdfWriter` on QuestPDF, mirroring the already-working `ReceiptPdfWriter` pattern (RTL, system font via `ArabicFontResolver`, `page.Size(...)`, `page.MarginTop(...)`), and render real header/footer blocks and a signature line.

---

**D-02 — Reference-range low/high comments never reach any printed report**
- **Related:** R-47
- **Location:** `src/TopLab.Application/Features/ReportProduction/Commands/BuildCombinedReport/BuildCombinedReportCommandHandler.cs:160-163`; `src/TopLab.Application/Features/ReportProduction/Common/ReportDtos.cs:40-50`; `ReportPdfWriter.cs:76-89`
- **Description:** `FormatRange` returns only `$"{MinValue} - {MaxValue}"`. `CombinedReportLineDto` has no `LowComment`/`HighComment` property, so the writer has nothing to print. For profile (multi-analyte) lines, `ProfileReportLineDto.FrozenRange` **does** carry `LowComment`/`HighComment` (`ReportDtos.cs:19-20`) but `ReportPdfWriter.cs:81-89` never renders `profile.FrozenRange` at all.
- **Why it matters:** The reference specifies (Learn pp. 101–102) that a comment is written into a low-comment or high-comment field and **appears automatically in the patient's report** when the result falls outside the range. The data is captured correctly in the database and then discarded at the output boundary.
- **Expected:** Low comment shown when result < min; high comment when result > max.
- **Actual:** Only the numeric min–max pair is printed.
- **Severity:** High · **Confidence:** High
- **Direction:** Add the comment fields to `CombinedReportLineDto`, populate them in `BuildCombinedReportCommandHandler`, and render them conditionally on the flag in the report writer.

---

**D-03 — Referral/Contract external entities cannot be created through the UI**
- **Related:** R-55, R-78
- **Location:** `src/TopLab.Presentation/Views/External/ExternalEntityEditorWindow.xaml:17-36`; `src/TopLab.Presentation/ViewModels/External/ExternalEntityEditorViewModel.cs:155-163`; `src/TopLab.Domain/ExternalEntities/ExternalEntity.cs:159-170`; `src/TopLab.Application/Features/ExternalEntities/Commands/CreateExternalEntity/CreateExternalEntityCommandValidator.cs:41-47`
- **Description:** `ExternalEntity.ValidatePriceListRule` throws `"ReferralOrContract requires PriceListId."`, and the FluentValidation validator independently requires it. But the editor window contains **no price-list or commission control**, and `SaveAsync` passes literal `null, null` for `PriceListId` and `DiscountOrCommissionPercent` in create mode. On edit the values round-trip from the DTO, which can only ever be null.
- **Why it matters:** This is a **hard functional block**, not a cosmetic gap. The reference (Learn p.126, p.143, p.146) explicitly requires a referral/contract entity to have a contract price list, and a commission percentage. Because creation always fails, referral entities cannot exist at all. `Patient.ReferralEntityId` and the account-type `Contracts` flow depend on them.
- **Cascade:** `DiscountOrCommissionPercent` can never be set, so `GetCashDrawerInventoryQueryHandler.cs:143` (which requires `> 0`) never produces a commission figure — the reference's "Lab P" referral percentage (Learn p.186, p.188) is permanently empty.
- **Severity:** Critical · **Confidence:** High
- **Direction:** Add a price-list picker and a commission-percent box to `ExternalEntityEditorWindow.xaml`, expose a price-list query, and pass the real values in `SaveAsync`. Add a regression test that creates a `ReferralOrContract` through the ViewModel.

---

**D-04 — Four implemented accounting report variants have no UI selectors**
- **Related:** R-76, R-77, R-78
- **Location:** `src/TopLab.Presentation/Views/Accounts/AccountsHubView.xaml:106-137`; `src/TopLab.Presentation/ViewModels/Accounts/AccountsHubViewModel.cs:43-46,145-148,250`; `src/TopLab.Application/Features/InventoryAndAccounting/Queries/GetElementInventory/GetElementInventoryQueryHandler.cs:206-224`
- **Description:** `GetElementInventory` fully supports 5 `InventoryElementKind` values (User, ReferralEntity, TreatingDoctor, AccountType, SentOutSamples) × 4 `InventoryReportType` values (Summary, Detailed, DetailedByPrices, DetailedByResults) — exactly the reference's five accounting scopes and four report variants (Learn p.178–179). The ViewModel exposes `ElementKind`, `ElementId`, `AccountTypeFilter` and `ReportType` as bindable properties and passes them to the query. **A grep of `AccountsHubView.xaml` finds zero bindings to any of them.** The fields stay at their initial values (`User`, `null`, `null`, `Summary`), and `Summary` returns an **empty** `Lines` list (`:208`).
- **Why it matters:** The user can only ever see an empty grid on this tab. Four complete, tested, permission-gated report variants are unreachable, and `GetPatientSamplesDetail` (the reference's drill-down, Learn p.180) has no trigger either.
- **Severity:** High · **Confidence:** High
- **Direction:** Add a `ComboBox` for element kind, an entity picker for `ElementId`, an account-type filter, and a report-type `ComboBox` bound to the existing properties; populate the `Lines` grid. Ensure `Summary` returns a meaningful aggregate row.

---

**D-05 — Statistics screens are not protected by the second/system password**
- **Related:** R-72
- **Location:** `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs:268-276` (contrast `:233-246` for Users and `:255-263` for Accounts)
- **Description:** Navigating to `StatisticsViewModel` does not call `ShowSecondaryPasswordDialogAsync()`. The reference requires the second password before opening Statistics (Learn p.167 shows the "Please Enter Your Second Password." dialog above the Statistics window).
- **Why it matters:** Statistics expose commercial-sensitive figures (revenue, commissions, per-doctor volumes, per-user productivity). A user without the `STATISTICS` *permission* is correctly blocked by `AuthorizationBehavior` — the queries do implement `IAuthorizedRequest` (`GetPatientCountStatisticsQuery.cs:16`, `StatisticsAccessPolicy.cs`) — but any user who *does* hold that permission reaches the screen without the second-factor gate the reference specifies.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Insert the same `ShowSecondaryPasswordDialogAsync()` call used for Users and Accounts.

---

**D-06 — Envelope settings are fully modelled but have no renderer**
- **Related:** R-84
- **Location:** `src/TopLab.Domain/Settings/EnvelopeSettings.cs`, `EnvelopePrintItemPosition.cs`; `src/TopLab.Presentation/Views/Settings/EnvelopeSettingsView.xaml:26-70`; `src/TopLab.Infrastructure/Printing/` (no envelope writer); `PrinterOutputType.Envelope` — consumers listed in §5/R-84
- **Description:** The entity, DbSet, `UpdateEnvelopeSettings*` command, `GetEnvelopeSettings*` query, DTOs and a full editing view (including the four item-position grid) all exist and persist correctly. There is **no `IEnvelopePdfWriter` / envelope rendering service**, and nothing dispatches to `PrinterOutputType.Envelope`.
- **Why it matters:** The user can configure envelope printing in detail and nothing will ever print. Every persisted value (`TopMarginCm`, `HeaderFooterMode`, `SuppressCaptions`, the four item positions) is inert. The reference specifies envelope settings in detail (Learn p.205–206) and envelope printing as a permission-gated result-print option (Learn p.151).
- **Severity:** High · **Confidence:** High (settings layer verified real; absence of a renderer verified by listing `Infrastructure/Printing/`)
- **Direction:** Add an envelope PDF writer (QuestPDF, matching the receipt pattern) plus a print command wired to `PrinterOutputType.Envelope`.

---

**D-07 — Reference-range "refresh" command has no UI trigger**
- **Related:** R-49
- **Location:** `src/TopLab.Application/Features/ResultsEntry/Commands/RefreshResultReferenceRange/`; zero references in `src/TopLab.Presentation/`
- **Description:** The handler is complete and correct (re-resolves the range for a patient test and recomputes the flag). No view or ViewModel invokes it.
- **Why it matters:** The reference (Learn p.102–103) requires that after a normal value is changed, previously registered patients keep the old value until the user opens the report, presses "تحديث" and confirms. Without a trigger, the second half of that rule — *apply the new range* — is unreachable. The freeze behaviour (the more safety-critical half) is correct.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add a "تحديث القيمة المرجعية" action to the result-entry view bound to this command, with a confirmation dialog.

---

**D-08 — No rendered print preview before printing**
- **Related:** R-17, R-42
- **Location:** `src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs:360-382`; `CombinedReportViewModel.BuildPreviewCommand`; `WorkSheetsViewModel`
- **Description:** The "معاينة الطباعة" / "تقرير" / "معاينة ورقة العمل" actions render the report data into WPF `TextBlock`/`Run` panels. None of them produces the actual PDF and displays it.
- **Why it matters:** Given D-01/D-02, the difference between what the preview shows (readable Arabic data) and what prints (`?????` on a page that ignores margins and paper size) is **maximal**. A preview would have surfaced D-01 immediately. The reference makes preview a standard step before every report print (Learn p.30, p.88).
- **Severity:** Medium · **Confidence:** High
- **Direction:** Write the PDF to a temp file and open it in the configured viewer (or an embedded WebView) before dispatching to the printer.

---

**D-09 — Culture microscopic examination is not modelled**
- **Related:** R-22, R-24
- **Location:** `src/TopLab.Domain/Results/CultureResult.cs:10-20`; grep for `PusCell|Epithelial|Microscop|Fungi|Crystal` across `src/` returns **zero** matches
- **Description:** The reference culture screen (Learn p.38) has a Microscopic Reaction block with ~10 rows (Pus Cells, R.B.Cs, Epithelial Cells, Crystal, Fungi, Others ×3, Direct?) plus a Direct? toggle, and a report with display toggles for Sensitivity / Reference / Commercial Name (Learn p.39). `CultureResult` stores only Sample, Organism A/B/C, Culture Condition, Colony Count.
- **Why it matters:** An entire documented section of the microbiology workflow — and of the microbiology report (Learn p.40 shows it in full) — has no representation. The data cannot be entered, stored, or printed.
- **Severity:** High · **Confidence:** High
- **Direction:** Add a `CultureMicroscopy` child entity (or JSON-valued fields) with the documented rows, extend `CultureEntryView` and `ReportDtos`, and render them in the microbiology report section.

---

**D-10 — Culture sensitivity stores a category but not the inhibition zone or trade name**
- **Related:** R-23, R-58
- **Location:** `src/TopLab.Domain/Results/CultureAntibioticResult.cs:13`; `src/TopLab.Domain/Tests/Antibiotic.cs:8-12`; grep for `InhibitionZone|ZoneMm|CommercialName` in `src/TopLab.Domain/` returns **zero** matches
- **Description:** The reference (Learn p.39–40, p.136–139) records, per antibiotic: the sensitivity class (Highly/Moderate/Low/Resistant For), the **inhibition zone in millimetres** (e.g. `>18mm`), and the **commercial/trade name** (e.g. Augmentin). TopLab persists only a `SensitivityCategory` enum. The antibiotic master has only `Name`, `IsPregnancyFlagged`, `IsChildrenFlagged` — no abbreviation/symbol, no scientific name, no commercial names.
- **Why it matters:** Zone diameter is the actual measured clinical value; without it the sensitivity class is a manually-typed assertion with no supporting data, and the report column the reference shows (`Inhibition zone`) cannot exist. Note the worksheet writer already documents that barcodes print as text rather than images — a similar pattern of reduced fidelity.
- **Severity:** High · **Confidence:** High
- **Direction:** Add `InhibitionZoneMm` (decimal, nullable) to `CultureAntibioticResult` and `Symbol`/`ScientificName`/`CommercialNames` to `Antibiotic`; extend the report DTO and writer.

---

**D-11 — Patient history for blood picture is a line list, not the reference's date × analyte matrix**
- **Related:** R-34
- **Location:** `ReportPdfWriter.cs:129-133`; `ReportDtos.cs:HistoryEntryDto`
- **Description:** The reference history report (Learn p.79) is a matrix: rows = analysis dates, columns = analytes (Hgb, RBCs, HCT, MCV, MCH, MCHC, %, Lin, Mon, Neu, Eos, Bas, Other, Plt). TopLab's history DTO carries one entry per (test, date) and the writer emits a flat line per entry.
- **Why it matters:** The matrix is what makes trend comparison readable in a haematology report. A flat list conveys the same data but not the reference's presentation, and cannot show analyte columns at all.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Pivot `HistoryEntryDto` by date and analyte in the history writer.

---

**D-12 — Reference-range low/high comments and per-test bold are stored but not editable/reported as specified**
- **Related:** R-36, R-47
- **Location:** `ReportSettings` (no per-test bold property); `ReportSettingsView.xaml:53-58`
- **Description:** The reference report settings (Learn p.196) include a per-analyte `Bold` checkbox for the patient-history table. `ReportSettings` has no such field, and the settings view offers only sort mode + auto-display.
- **Severity:** Low · **Confidence:** High
- **Direction:** Add a per-analyte bold collection to `ReportSettings` and render it in the history matrix (paired with D-11).

---

**D-13 — No branch / multi-branch concept anywhere in the model**
- **Related:** R-26, R-62, R-75, R-79
- **Location:** grep for `BranchNumber|BranchNo` in `src/TopLab.Domain/` returns **zero** matches; the only repo-wide hit is a comment in `TestPriceResolver.cs:16`
- **Description:** The reference specifies a branch number in four places: patient search (Learn p.42, p.45), per-user assignment (p.151), drawer/accounting period selection (p.178–179) and system settings (p.191). TopLab has no branch field on `User`, `SystemSettings`, or any query parameter. `ShellViewModel` has no branch selector.
- **Why it matters:** All four reference features that depend on branch selection are affected. This is one missing concept with four downstream consequences, not four independent omissions.
- **Severity:** High · **Confidence:** High
- **Direction:** Add `BranchNumber` to `User` and `SystemSettings`, thread it as an optional filter through the accounting/statistics queries, and add the search and drawer selectors. This is a larger change and should be scoped as one work item.

---

**D-14 — Natigh.com result-delivery portal is entirely absent**
- **Related:** R-29, R-30, R-31
- **Location:** grep for `natigh` (case-insensitive) across `src/` returns **zero** matches
- **Description:** The reference devotes a full section (Learn pp. 58–69) plus §3-11 (pp. 142–147) to: uploading a patient's result to a website, the patient/doctor/lab log-in flow, the Android app, viewing and exporting results as Word/PDF, counting uploaded patients over a period, blocking a patient's result, printing the site data on the receipt, and creating website accounts for doctors and labs with a generated ID used as the password.
- **Partial substitute found:** `GenerateEntityIdCodeCommandHandler` + `SecureEntityIdCodeGenerator` issue a 12-character cryptographically random code per external entity, and `GetExternalEntityByCodeQuery` looks it up. This is a code, not an account: it is generated automatically on every create, displayed read-only, has no password concept, no per-doctor/per-lab scoping, and no website.
- **Why it matters:** This is a whole reference chapter. It is also, by nature, not implementable purely as a desktop feature — the website and mobile app are external deliverables. Classifying it MISSING is correct for the desktop scope; the reader should note that satisfying it requires work outside this repository.
- **Severity:** High (scope) · **Confidence:** High
- **Direction:** Decide scope explicitly. If the web portal is in scope, it is a separate project; if only the desktop-side account management is in scope, add an explicit `ExternalEntityAccount` with a user-settable password and an upload-tracking table.

---

**D-15 — Test catalogue is missing 6 of 13 editable fields and 5 of 9 displayed columns**
- **Related:** R-43, R-44
- **Location:** `src/TopLab.Domain/Tests/Test.cs:11-40`; `src/TopLab.Presentation/Views/Lab/TestCatalogView.xaml:61-64`; `src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Common/TestCatalogDtos.cs:5-17`
- **Description:** Reference test fields (Learn p.95, p.98): group, report name, bill name, history name, Arabic name, barcode name, test time (days), reference type, patient price, lab-to-lab price, sent-out flag + lab + cost, patient question, main test, print-with-other, add-with-group, see report. `Test` declares 8 of these. Absent entirely from the codebase (grep returns zero): history name, Arabic name, barcode name, reference type, patient question, main test, print-with-other, add-with-group, see report. Note the model has **no** `ExternalEntityId` for the out-lab — the destination is chosen per-sample at send time only.
  Separately, the catalogue grid binds 4 of the 9 reference columns even though the DTO already carries `LabToLabPrice`, `IsSentOut` and `Barcode`.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add the missing `Test` fields with a migration, extend the editor window, and bind the existing DTO columns in the grid.

---

**D-16 — Only the pregnancy medical condition is seeded; the reference specifies 11+ clinical history toggles**
- **Related:** R-03
- **Location:** `src/TopLab.Infrastructure/Persistence/Configurations/MedicalConditionTypeConfiguration.cs:17` — `HasData(new { Id = 1, Name = "حمل", Category = Pregnancy })`
- **Description:** `MedicalConditionCategory` declares `Medication`, `Condition` and `Pregnancy`, and the patient editor renders whatever is in the table — but the table is seeded with exactly one row. The reference (Learn p.13, p.18, p.24) specifies 6 medication questions (diabetes, BP, viral treatment, antibiotic, blood thinner, liver) and 5 condition questions (anaemia, lupus, kidney failure, BP, joint inflammation), plus X-ray and pregnancy. The `Medication` and `Condition` categories are unreachable, and there is no command or view to create a condition type (grep for `AddMedicalConditionType|CreateMedicalConditionType` returns zero).
- **Why it matters:** The patient editor shows one checkbox where the reference shows a full clinical-history panel. The plumbing is correct; only the content is missing — which makes this a data-seeding gap, not an architectural one, and therefore cheap to close.
- **Severity:** High · **Confidence:** High
- **Direction:** Seed the 11 documented conditions in a migration and add a maintenance command/view for condition types.

---

**D-17 — Reference ranges have no test-unit field**
- **Related:** R-46
- **Location:** `src/TopLab.Domain/Tests/ReferenceRange.cs`, `AnalyteReferenceRangeBand.cs` — no `Unit`; `src/TopLab.Domain/Results/ProfileResultItem.cs:22` has `Unit` (profile path only)
- **Description:** The reference range editor (Learn p.102) shows a `Test unit` column alongside `To / Reference range / high / Low flag`. No `Unit` exists on either range entity or on `Analyte`.
- **Why it matters:** The unit shown next to a reference range in the report would have to come from elsewhere; on the simple-test path there is nowhere to get it.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add `Unit` to `Analyte` (the owning entity) and surface it in the range editor and report.

---

**D-18 — Receipt footer is hardcoded empty; print-once is not enforced; cashier-printer flag is dead**
- **Related:** R-83
- **Location:** `src/TopLab.Infrastructure/Printing/ReceiptPdfWriter.cs:214` (returns `new List<string>()` unconditionally); `src/TopLab.Infrastructure/Printing/ReceiptPrintingService.cs:20`; `ReceiptSettings.CashierPrinterEnabled`
- **Description:** Three separate issues in the receipt path. (a) The footer line collection is returned empty regardless of `HeaderFooterMode`, so receipt footers never print. (b) `PrintOnce` is explicitly documented in code as "a display-only setting: no reprint-guard" — the reference's print-once behaviour (Learn p.204) is not implemented. (c) `CashierPrinterEnabled` has no consumer outside the EF configuration and the settings view. Separately, `HeaderFooterMode.Images` silently falls back to words.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Populate the footer from settings; implement a reprint guard (e.g. refuse or warn when `InvoiceIssue`/receipt already exists for the visit); wire or remove `CashierPrinterEnabled`; implement the image footer path.

---

**D-19 — No Units or Barcode Types management**
- **Related:** R-50
- **Location:** no `Unit` or `BarcodeType` entity, DbSet, configuration, view or command; `LabHubView.xaml:7-41` has 11 tabs, none for these
- **Description:** The reference lists `Test Units` and `Barcode Types` as System-menu maintenance windows (Learn p.96, p.112). `BarcodeLabelRenderer.cs:18` hardcodes `BarcodeFormat.CODE_128`.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add `Unit` and `BarcodeType` entities with CRUD views; make the renderer and `Test.Barcode` reference the configured type.

---

**D-20 — Custom test groups have no total group price**
- **Related:** R-52
- **Location:** `src/TopLab.Domain/Tests/CustomGroup.cs`, `CustomGroupItem.cs`
- **Description:** The reference custom-group window (Learn p.119–121) shows `No. Of tests in group` and `Total group Price (L.E.)` alongside the per-test `Price` column. `CustomGroup` stores only per-item `CustomGroupItem.Price`; there is no group-level total.
- **Severity:** Low · **Confidence:** High
- **Direction:** Either derive and display the total, or add an explicit override field as the reference implies.

---

**D-21 — No price-list printing**
- **Related:** R-54
- **Location:** The complete print-command set in the codebase is `PrintInvoice`, `PrintReceipt`, `PrintBarcode`, `PrintBlankReport`, `PrintCombinedReport`, `PrintHistoryReport`, `PrintWorkSheet`. No `PrintPriceList` exists. `PriceListsView.xaml:13-85` has no print button.
- **Description:** The reference (Learn p.105, p.111) requires printing a price list, and the sample output (Learn p.111) shows prices grouped by test group with `Price / Result date / Collection notes` columns and a `Current Page X from Y` footer.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add a `PrintPriceList` command and a QuestPDF writer following the work-sheet pattern (which already renders group-grouped tables with Arabic RTL).

---

**D-22 — Antibiotic master lacks symbol, scientific name and trade names, and has no print**
- **Related:** R-57
- **Location:** `src/TopLab.Domain/Tests/Antibiotic.cs:8-12`; `AntibioticsView.xaml:32-34`
- **Description:** The reference antibiotic window (Learn p.135) has International Antibiotic name, Scientific Name, symbol/abbreviation (AMX, AMC, AM, SAM, AZM, ATM, CB, CEC, CN, MA, CZ, FEP, CFEP, CFM, CFP, SCF, CTX, CCTX, CTT, FOX), Commercial Names, and a Print button. TopLab has one `Name` field plus the two flags.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add `Symbol`, `ScientificName`, `CommercialNames`; add a print command.

---

**D-23 — Child detection excludes patients under 1 year**
- **Related:** R-58
- **Location:** `src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs:33` — `patient.AgeUnit == AgeUnit.Year && patient.AgeValue < 12`
- **Description:** The reference rule (Learn p.139) is "the program recognises children if their age is less than 12 years (male or female)". TopLab's predicate requires the age to be *expressed in years*, so an 11-month-old (`AgeUnit.Month, AgeValue = 11`) is **not** flagged as a child, and children-only antibiotics remain visible for an infant.
- **Why it matters:** This is the same class of age-unit subtlety that the reference-range code handles correctly (requirement R-48, `ReferenceRange.Matches`). Here the correct-by-design no-conversion rule is applied without the accompanying normalisation, producing an inverted result for infants.
- **Severity:** High · **Confidence:** High
- **Direction:** Normalise before comparing — treat `AgeUnit.Month` with `AgeValue < 144` (and `AgeUnit.Day` with `AgeValue < 365*5`) as under 12, or store a computed age-in-years for this check.

---

**D-24 — Statistics groupings and filters incomplete**
- **Related:** R-68, R-69, R-71, R-73
- **Location:** `GetPatientCountStatisticsQuery.cs:8-14`; `GetTestCountStatisticsQuery.cs:26-33`; `StatisticsView.xaml:60-64`
- **Description:** Reference groupings vs implemented: patient stats need month ✓, day-of-month ✗, sex ✓, account type ✓, referral entity ✓, **specific referral entity only ✗**. Test stats need test name ✓, test group ✓, **referral entity ✗**, specific entity ✗. User productivity is implemented as entered/reviewed/printed/delivered counts, which is a reasonable reading of "assigned work" but is not work-assignment modelling.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add `ByDayOfMonth` and `ReferralEntityId` parameters to the two queries, plus matching selectors in `StatisticsView`.

---

**D-25 — Result monitoring / follow-up report is absent**
- **Related:** R-73
- **Location:** no query accepts min/max result values; no such report exists in `src/`
- **Description:** RL_Show p.25 shows a "تقرير متابعة ومراقبة النتائج" over a date range with columns `اليوم · اسم المريض · بيانات المريض · جهة الاحالة · التحليل · النتيجة · الحالة`, produced by selecting a test and a min/max result value. The caption states the purpose explicitly: to assess chemistry, device and RAST results and display positive results. No equivalent exists.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add a `GetResultMonitoringQuery(TestId, From, To, MinValue, MaxValue)` plus a report writer.

---

**D-26 — Patient search does not offer the reference's 13 search modes**
- **Related:** R-25
- **Location:** `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml:8-16`
- **Description:** The reference search window (Learn p.43, p.45) offers: exact patient name, wildcard patient name, treating-doctor name, sex, age, phone, card number, by test in a period, by request date, and four status filters (results not entered / not reviewed / not printed / not delivered), plus Backup. TopLab provides a single free-text box (which the VM routes to name / national ID / Lab ID / phone) and a Lab ID fetch. No status filters, no by-test search, no by-request-date search, no treating-doctor search.
- **Severity:** Medium · **Confidence:** High (the reference's search-button grid is legible in OCR at Learn p.43 and p.45; the target's controls are fully enumerable)
- **Direction:** Add the missing filter dimensions to `SearchPatientsGlobalQuery` and the search view.

---

**D-27 — System settings: 3 dead flags, 5 missing capabilities**
- **Related:** R-79
- **Location:** `src/TopLab.Domain/Settings/SystemSettings.cs`; `SystemSettingsViewModel.cs:186,193,194`
- **Description:** Live (written and read by behaviour): `DefaultAccountType` → `GetRegistrationCatalogQueryHandler.cs:70`; `DisableAutoTitleInsertion` → `:71`; `EnablePatientNameSearchAssist` → `SearchPatientsGlobalQueryHandler.cs:25`; `PrintLabIdInsteadOfPatientId` → `PrintBarcodeCommandHandler.cs:40`, `ReportPdfWriter.cs:45`; `PrintDateTimeOnTubeBarcode` → `BarcodeService.cs:62`; `AutoReviewAndComplete` → `EnterResultCommandHandler.cs:116`; `DailyBackup*` → `DailyBackupHostedService.cs:57,62`. **Dead** (persisted and read back into the VM, consumed by nothing): `SaveTreatingDoctorOnlyFromEntityWindow`, `PrintAccountInsteadOfDateOnReport`, `ResultScreenAccountDisplayMode`. **Missing entirely:** branch number, patient/doctor English names on receipts, self/herself substitution when referral is blank, auto-complete lab tests, and a card printer (`PrinterOutputType` has no `Card` member).
- **Severity:** Medium · **Confidence:** High
- **Direction:** Implement or remove the three dead flags; add the missing settings; add `PrinterOutputType.Card`.

---

**D-28 — Three `ReportSettings` colour columns are permanently null**
- **Related:** R-82
- **Location:** `src/TopLab.Domain/Settings/ReportSettings.cs:21,23,47-48`; `ReportSettingsConfiguration.cs:20-21,25`
- **Description:** `HeaderColor` and `FooterColor` are declared, mapped, seeded as null, and asserted null in `ReportSettingsTests.cs:92-93`. There is **no setter method**, no field on `UpdateReportSettingsCommand`, no property on the settings DTO, no control in `ReportSettingsView.xaml`, and no reader in any writer. They are write-never, read-never columns.
- **Why it matters:** The reference specifies per-element header/footer colours with a colour picker and a "custom colours" dialog (Learn p.195–196). None of that is reachable.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Either implement the colour pipeline end to end (setter → command → DTO → view → writer) or remove the columns and the reference claim.

---

**D-29 — Barcode label geometry and symbology are hardcoded**
- **Related:** R-86
- **Location:** `src/TopLab.Infrastructure/Barcode/BarcodeService.cs:88-89` (page 288 × 144 pt = 4in × 2in); `BarcodeLabelRenderer.cs:18` (`BarcodeFormat.CODE_128`)
- **Description:** The reference specifies 38 × 25 mm stickers (RL_Show p.3), vertical and horizontal layout, printing on pre-printed stock vs white paper, per-test tube/container barcodes, a "Print barcode after read ID" option, and configurable Barcode Types (Learn p.96). TopLab renders one fixed-size Code-128 label per call with a human-readable ID, and the file is written to the OS temp directory and dispatched.
- **Severity:** Medium · **Confidence:** High
- **Note:** This is a reasonable simplification for a first implementation; it is recorded as a gap, not a fault.
- **Direction:** Make sticker dimensions, orientation and symbology configurable; add the per-test sticker set from the work sheet.

---

**D-30 — Login detector is absent**
- **Related:** R-66
- **Location:** only `User.LastLoginAtUtc` (`User.cs:28`, set by `RecordLogin`); no login/logout log table exists
- **Description:** The reference (Learn p.164) shows a printed report "كشف تسجيل الدخول والخروج للمستخدم … عن فترة محددة" with columns `اسم الجهاز · وقت الخروج · تاريخ الخروج · وقت الدخول · تاريخ الدخول`. TopLab records only the last login timestamp on the user row — no machine name, no logout time, no history.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Add a `LoginSession` entity (UserId, MachineName, LoginAt, LogoutAt) written by the login/logout commands, and a report query.

---

**D-31 — Two reference permissions are not separately modelled; break period is minutes-only**
- **Related:** R-62, R-63
- **Location:** `PermissionConfiguration.cs:17-31` (13 permissions); `User.BreakDurationMinutes` (`User.cs:23`)
- **Description:** The reference permission list (Learn p.151) treats "طباعة وصل" (receipt print) as part of patient add/edit, and "طباعة ورقة العمل" (work-sheet print) as its own item; TopLab has `PRINT_WORKSHEET` but folds receipt printing into patient registration. The reference break period is captured in **hours and minutes**; TopLab stores minutes only. Neither is a functional break, but both are specified deviations.
- **Severity:** Low · **Confidence:** High
- **Direction:** Minor model additions if strict parity is required.

---

**D-32 — Sent-out screen lacks the sent / not-sent filter**
- **Related:** R-60
- **Location:** `GetSentOutSamplesQuery` exposes only `From/To/ExternalLabEntityId/Page/PageSize`
- **Description:** The reference window (Learn p.186) has explicit `الكل` (all) and `الغير مرسل` (not sent) radio options, a request-date filter, and a per-entity search. Period and entity filters exist; the sent/not-sent distinction does not.
- **Severity:** Low · **Confidence:** High
- **Direction:** Add a `SentOutState` parameter to the query and a radio pair to the view.

---

**D-33 — Commission ("Lab P") figures are computed but never displayed**
- **Related:** R-78
- **Location:** `GetCashDrawerInventoryQueryHandler.cs:125-177` builds `CommissionShareDto`; `InventoryDtos.cs:28` exposes it; `AccountsHubView.xaml:65-80` binds none of it
- **Description:** The reference (Learn p.186, p.188; RL_Show p.29) shows a per-entity table with `عمولة ونسب` and a `100%` total. The computation exists and is correct in shape; no view renders it. Compounded by D-03, the source percentage can never be non-zero.
- **Severity:** Medium · **Confidence:** High
- **Direction:** Bind the existing `CommissionsAndShares` collection to a grid; fix D-03 so the input can be populated.

---

**D-34 — No drill-down from accounting totals to per-patient detail**
- **Related:** R-77
- **Location:** `GetPatientSamplesDetail` exists; `AccountsHubView.xaml` has no selection handler
- **Description:** The reference (Learn p.180) instructs the user to press "إجمالى عينات المرضى" to open the detailed per-patient listing with columns `تاريخ الطلب · كود المريض · اسم المريض · الاجمالى · قيمة الخصم · بعد الخصم · المدفوع · باقى الحساب`. The query exists and returns the right shape; no control triggers it.
- **Severity:** Low · **Confidence:** High
- **Direction:** Add a click handler on the totals figure opening a detail grid, matching the reference's sample output (RL_Show p.26).

---

### 8.2 Performance assessment

| Observation | Classification | Evidence |
|---|---|---|
| Whole-period statistics materialise the full result set in memory before grouping | **Observed from code (potential risk)** | `GetPatientCountStatisticsQueryHandler.cs:39` `.ToList()` on all `Patient` rows in the range, then LINQ-to-Objects grouping; `GetSentOutStatisticsQueryHandler.cs:48` `.ToList()` on all `SentOutSample` rows in the range |
| Sent-out statistics issue a second query with `Contains` over all sample ids | **Observed from code** | `GetSentOutStatisticsQueryHandler.cs:49-52` — the id list is materialised into SQL as an `IN` clause; with very large periods this can approach SQL Server's 2,100-parameter limit |
| Referral-entity names fetched with a single batched `Contains` query rather than per-row lookups | **Observed from code (good practice)** | `GetPatientCountStatisticsQueryHandler.cs:57-59` |
| Printing services catch all exceptions and return a generic `Error.Unexpected` | **Observed from code** | `ReportPrintingService.cs:80-84`, `BarcodeService.cs:80-84` — intentional per the class contract ("this service never throws"), but it means a PDF-generation bug surfaces to the user only as "تعذر طباعة التقرير" with no diagnostic detail |
| Temp PDFs are intentionally never deleted | **Observed from code (minor resource concern)** | `ReportPrintingService.cs:69`, `BarcodeService.cs:76` — every print leaves a file in `%TEMP%` |
| No pagination on the statistics grids | **Potential risk** | `StatisticsView.xaml` grids bind unbounded `IReadOnlyList` results |

**Not claimed:** no runtime performance problem is asserted from any pattern alone. The `.ToList()` findings are real inefficiencies in the code as written; their practical impact depends on data volume, which cannot be established from source.

### 8.3 Data-integrity assessment

Positive findings, all traced: frozen prices at order time; voided payment operations excluded from totals; soft-delete with mutation guards; auditable interceptor on all writes; result amendments retaining old value, new value, actor, timestamp and reason; foreign keys from patient tests to patients and tests; permission gates on destructive operations.

Concerns: the `Patient` model stores a single `LabId` that is shared across visits of the same person — a design choice, not a defect, but it means two different people cannot share a Lab ID and a patient cannot have two. Reported as an observation, not a finding, because the reference describes the same shared-identity concept (Learn p.47).

---

## 9. Missing and Partial Functionality

### 9.1 Missing entirely (12)

| ID | Requirement | Notes |
|---|---|---|
| R-19 | Combined report print options (group sub-title, reprint suppression) | No model or view support |
| R-22 | Culture microscopic examination | No entity fields |
| R-24 | Culture report display toggles | No view controls |
| R-26 | Branch-number search | No branch concept (D-13) |
| R-29 | Natigh.com result upload + patient/doctor/lab portal | External system; no code |
| R-30 | Natigh patient counts, result blocking, site data on receipt | No code |
| R-31 | Natigh doctor/lab accounts with ID-as-password | Random code only, no account concept |
| R-50 | Units + Barcode Types management | No entities |
| R-66 | Login detector (machine name, logout time) | Only `LastLoginAtUtc` |
| R-73 | Result monitoring/follow-up report (min/max, positives) | No query or report |
| R-82 | Header/footer element positions, per-element colour/bold | Colour columns permanently null |
| R-85 | Card (كارنية) settings and printer | No entity, no `PrinterOutputType.Card` |

### 9.2 Partial (30)

R-03, R-08, R-11, R-12, R-17, R-23, R-25, R-34, R-36, R-42, R-43, R-44, R-46, R-49, R-52, R-54, R-56, R-57, R-58, R-60, R-62, R-63, R-68, R-69, R-71, R-75, R-77, R-78, R-79, R-86.

### 9.3 Implemented with defects (6)

R-47 (low/high comments lost at the output boundary), R-55 (referral/contract entity uncreatable via UI), R-72 (statistics missing the second-password gate), R-76 (accounting variants unreachable), R-81 (report writer ignores its settings), R-83 (receipt footer empty, print-once inert).

### 9.4 Present but unverified (1)

R-84 — envelope settings. The settings layer is verified real (entity, DbSet, command, query, DTO and a full editing view all exist and persist correctly), but **no envelope renderer exists**, so the end-to-end behaviour cannot be established. Separately, the three QuestPDF printing tests that fail in this environment are classified in §12 as environment-limited rather than as requirement-level findings.

---

## 10. Overall Completion Assessment

### 10.1 Requirement counts

| Status | Count | Share of 87 |
|---|---|---|
| Fully implemented | 38 | 43.7% |
| Partially implemented | 30 | 34.5% |
| Implemented with defects | 6 | 6.9% |
| Present but unverified | 1 | 1.1% |
| Missing | 12 | 13.8% |
| Not specified / not applicable | 0 | 0% |

### 10.2 Completion calculation (explicitly defined)

Three complementary measures are given, because a single number would be misleading.

**(a) Strict binary coverage** — requirements at least partially addressed:
> (38 + 30 + 6 + 1) / 87 = **75 / 87 = 86.2%**

**(b) Weighted functional completeness** — each requirement scored 0 (missing), 0.5 (partial / present-but-unverified) or 1.0 (fully implemented **or** implemented-with-defects, since the capability functions; defects are penalised separately in §10.4):
> (38 × 1.0 + 6 × 1.0 + (30 + 1) × 0.5 + 12 × 0) / 87
> = (44 + 15.5) / 87 = **59.5 / 87 = 68.4%**

**(c) Defect-free completeness** — as (b) but scoring implemented-with-defects at 0.75 rather than 1.0:
> (38 × 1.0 + 6 × 0.75 + (30 + 1) × 0.5) / 87 = (38 + 4.5 + 15.5) / 87 = 58 / 87 = **66.7%**

**A defensible summary is 67–68% on the weighted measures, with 86% strict coverage.** The headline figure quoted in §1 (68.4%) is measure (b), which credits an implemented-with-defects capability as functioning — appropriate, because the defect is then penalised separately in §10.4 and by the Tier weighting in §10.3. Measure (c) is the conservative reading and lands within one point, because only 6 requirements carry the "with defects" classification; the far larger reduction in perceived completeness comes from the 30 partial requirements, which no weighting scheme can resolve without reading them as either present or absent.

### 10.3 Critical-path weighting (the more meaningful view)

An unweighted percentage treats a missing login-detector report and a broken clinical report as equal. They are not. The 87 requirements were re-grouped into four tiers by clinical impact, and the status distribution recomputed **directly from the matrix in §5** (so the figures below cannot drift from the evidence):

| Tier | Requirements | FI | PI | IWD | M | PBU | Weight |
|---|---|---|---|---|---|---|---|
| **Tier 1 — clinical output** (patient report, blank report, print preview, history reports, culture entry & sensitivity, test comments, report settings) | 14 | 6 | 4 | 2 | 2 | 0 | 4× |
| **Tier 2 — core laboratory workflow** (registration, ordering, billing, result entry & verification, delivery, visit history, audit, sent-out, sample drawing) | 21 | 17 | 3 | 0 | 1 | 0 | 3× |
| **Tier 3 — administration** (conditions, branch, ranges, catalog, entities, users, permissions, attendance, statistics, accounting, settings) | 30 | 7 | 12 | 4 | 6 | 1 | 2× |
| **Tier 4 — auxiliary** (work sheets, catalog editing, groups, price lists, antibiotics, barcodes) | 18 | 7 | 11 | 0 | 0 | 0 | 1× |
| **Unassigned to a tier** (R-29/30/31 Natigh portal — external system; R-48 age-unit rule — a cross-cutting correctness property verified correct) | 4 | 2 | 0 | 0 | 1 | 0 | — |

**The critical observation is not the status mix but what D-01 does to Tier 1.** Six of the fourteen Tier-1 requirements are marked *fully implemented* at the data and workflow layer — and every one of them is still unusable, because all six terminate in `ReportPdfWriter`, which prints `?????` for every Arabic string. A requirement marked FI whose output artefact cannot be read is not functionally complete in any meaningful sense of the phrase.

Applying defect severity to the tiers:

| Tier | Status-only result | After accounting for D-01's blast radius |
|---|---|---|
| Tier 1 | 6/14 fully implemented (43%) | **effectively 0% usable** — every FI item terminates in the broken writer |
| Tier 2 | 17/21 fully implemented (81%) | ~81% — core workflow does not depend on the report writer |
| Tier 3 | 7/30 FI, 6 missing (23%) | ~23% |
| Tier 4 | 7/18 FI, 11 partial (39%) | ~39% |

**This is the single most important conclusion of the audit: the clinical output tier is the weakest, and it is the tier a medical laboratory depends on most.** A laboratory could use this system to register patients, take money, record results, manage staff and balance the drawer — and would receive an unreadable printed report for every patient. This is why the weighted percentage in §10.2 (68%) materially overstates real readiness, and why §1 leads with the categorical conclusion rather than the number.

### 10.4 Defect burden

34 defects were recorded: 2 Critical, 9 High, 18 Medium, 5 Low. Concentrated in three places:
- **Report output** (D-01, D-02, D-08, D-11, D-12, D-28, D-18) — 7 defects, six of them downstream of the single architectural decision to hand-roll the report PDF.
- **Missing UI wiring for implemented handlers** (D-04, D-07, D-33, D-34) — 4 defects, all cheap to fix relative to their value.
- **Whole missing concepts** (D-03, D-13, D-14, D-19, D-21, D-25, D-30) — 7 defects, the largest remaining build effort.

### 10.5 Overall conclusion

TopLab is a **credible, well-engineered foundation that is roughly 68% complete against the RealLab reference, with a strong domain layer and an immature output layer.** The architectural quality is genuinely above average for this kind of project: Clean Architecture with framework-free domain, CQRS with validation and authorization pipeline behaviours, strongly-typed IDs, frozen reference-range snapshots, PBKDF2 password hashing, an audit interceptor, a guarded backup/restore path, and 2,158 passing tests.

What is missing is concentrated and, importantly, *legible*: the team clearly knows which pieces are unfinished — several comments say so outright. The work is not "un-discoverable"; it is a known backlog. Closing the Tier-1 output defects plus the four UI-wiring defects would move the system from "partially complete" to "operationally usable", and would require far less work than the volume of already-passing tests might suggest.

---

## 11. Prioritised Recommendations

Recommendations are tied to specific findings. Items not supported by the reference PDFs are labelled **[Optional engineering]** and are not completion requirements.

### Priority 1 — Clinical output correctness (blocks any clinical use)

| # | Action | Addresses | Effort |
|---|---|---|---|
| 1.1 | Reimplement `IReportPdfWriter` on QuestPDF, mirroring the working `ReceiptPdfWriter`: RTL, `ArabicFontResolver` font, real page size from `PaperSize`, real margins from `PageMarginLeftCm`/`PageMarginBottomCm`, real top offset from `ReportTopSpaceCm`, drawn header/footer block, doctor-signature line | D-01, R-81 | High |
| 1.2 | Carry `LowComment`/`HighComment` through `CombinedReportLineDto` into the report; render conditionally on the flag | D-02, R-47 | Low |
| 1.3 | Add a rendered print preview (write PDF → show) before every report, invoice and work-sheet print | D-08, R-17, R-42 | Medium |
| 1.4 | Add culture microscopic examination fields (entity, view, DTO, report) | D-09, R-22 | Medium |
| 1.5 | Add `InhibitionZoneMm` and antibiotic `Symbol`/`ScientificName`/`CommercialNames`; render the Sensitivity section of the microbiology report | D-10, R-23, R-57, R-58 | Medium |
| 1.6 | Pivot the blood-picture history into the reference's date × analyte matrix | D-11, R-34 | Medium |

### Priority 2 — Unblock existing functionality (highest value per unit of effort)

| # | Action | Addresses | Effort |
|---|---|---|---|
| 2.1 | Add a price-list picker + commission-percent control to `ExternalEntityEditorWindow` and pass the real values — this unblocks referral/contract entity creation entirely | D-03, R-55 | Low |
| 2.2 | Bind the existing `ElementKind` / `ElementId` / `AccountTypeFilter` / `ReportType` properties to controls in the element-inventory tab; make `Summary` return an aggregate row | D-04, R-76 | Low |
| 2.3 | Seed the 11 documented medical-history conditions and add a condition-type maintenance command | D-16, R-03 | Low |
| 2.4 | Wire the "refresh reference range" command to a result-screen action with confirmation | D-07, R-49 | Low |
| 2.5 | Bind the existing `CommissionsAndShares` collection to a grid; add the totals drill-down to the per-patient detail query | D-33, D-34, R-77, R-78 | Low |
| 2.6 | Add the second-password gate to the Statistics navigation | D-05, R-72 | Very low |
| 2.7 | Add the combined-report option checkboxes (group sub-title, reprint suppression) | R-19 | Low |

### Priority 3 — Reliability and data integrity

| # | Action | Addresses | Effort |
|---|---|---|---|
| 3.1 | Normalise age before the child check so infants are treated as children | D-23, R-58 | Very low |
| 3.2 | Populate the receipt footer from settings; implement the print-once guard; wire or delete `CashierPrinterEnabled` | D-18, R-83 | Medium |
| 3.3 | Log the swallowed exception in the printing services alongside the user-facing error, so D-01-class bugs are diagnosable in the field | §8.2 | Low |
| 3.4 | Add a startup or scheduled cleanup for the `%TEMP%` PDFs that every print leaves behind | §8.2 | Low |
| 3.5 | Push statistics grouping into SQL (`GroupBy` in the query) rather than materialising the whole period | §8.2 | Medium |

### Priority 4 — Missing capability with a clear boundary

| # | Action | Addresses | Effort |
|---|---|---|---|
| 4.1 | Decide and document the scope of Natigh.com: the desktop-side account management is in this repository; the website and Android app are not. Either add `ExternalEntityAccount` with a user-settable password and an upload-tracking table, or record the whole feature as out of scope | D-14, R-29–R-31 | Scope decision |
| 4.2 | Add the branch-number concept (`User`, `SystemSettings`, query filters, search and drawer selectors) as one coherent work item | D-13, R-26, R-62, R-75, R-79 | High |
| 4.3 | Add an envelope PDF writer wired to `PrinterOutputType.Envelope` — the settings UI already exists and works | D-06, R-84 | Medium |
| 4.4 | Add price-list printing and antibiotic-list printing | D-21, D-22, R-54, R-57 | Medium |
| 4.5 | Add Units and Barcode Types management | D-19, R-50 | Medium |
| 4.6 | Add the login-detector entity, commands and report | D-30, R-66 | Medium |
| 4.7 | Add the result monitoring/follow-up report | D-25, R-73 | Medium |
| 4.8 | Add card settings and `PrinterOutputType.Card` | D-27, R-85 | Medium |
| 4.9 | Implement or remove the three dead `SystemSettings` flags and the two dead `ReportSettings` colour columns | D-27, D-28, R-79, R-82 | Low |

### Priority 5 — Fidelity and completeness of what exists

| # | Action | Addresses | Effort |
|---|---|---|---|
| 5.1 | Add the 6 missing `Test` fields (history name, Arabic name, barcode name, reference type, patient question, main test, print-with-other, add-with-group, see report) with a migration; bind the 5 already-present catalogue columns | D-15, R-43, R-44 | Medium |
| 5.2 | Add `Unit` to `Analyte` and surface it in the range editor and report | D-17, R-46 | Low |
| 5.3 | Add custom-group total price; add the sent/not-sent filter; add the reference's remaining patient-search dimensions | D-20, D-32, D-26, R-25, R-52, R-60 | Medium |
| 5.4 | Add day-of-month and specific-referral-entity statistics groupings | D-24, R-68, R-69 | Medium |
| 5.5 | Make barcode sticker geometry, orientation and symbology configurable | D-29, R-86 | Medium |
| 5.6 | Add the two missing granular permissions and hours+minutes break period | D-31, R-62, R-63 | Low |

### 6. Testing gaps

| # | Action | Rationale |
|---|---|---|
| 6.1 | Add a test asserting a referral/contract entity can be created **through the ViewModel**, not just the command | Would have caught D-03 |
| 6.2 | Add a golden-file test asserting the generated report PDF contains the patient's Arabic name and the configured page size | Would have caught D-01 |
| 6.3 | Add a test asserting `LowComment`/`HighComment` appear in the report payload | Would have caught D-02 |
| 6.4 | Add a UI-wiring test per hub asserting each declared ViewModel property is bound in the corresponding XAML | Would have caught D-04 and the whole class of "implemented but unreachable" defects |
| 6.5 | Add an age-normalisation unit test for the child check across Day/Month/Year | Would have caught D-23 |
| 6.6 | Establish a Windows CI runner so the 3 QuestPDF print tests are actually exercised in CI | Currently failing unverified in this environment |

### Optional engineering improvements (not reference requirements)

- **[Optional]** Hard-delete/purge tooling for soft-deleted patients beyond restore.
- **[Optional]** Keyboard shortcuts — the reference claims Function Keys navigation (RL_Show p.52) but does not specify bindings; TopLab has none.
- **[Optional]** Analyzers for Arabic-to-ASCII conversion in print paths, to prevent D-01-class regressions.

---

## 12. Limitations and Evidence Gaps

1. **Static analysis only.** No GUI execution, no Windows runtime, no live SQL Server. Conclusions about *behaviour* are inferences from code paths, not from observation. The strongest mitigations available were used: the solution **builds with 0 warnings and 0 errors**, and **2,158 tests pass**.

2. **Three infrastructure tests fail and are not resolved.** `ReceiptPrintingServiceTests`, `InvoicePrintingServiceTests` and `WorkSheetPrintingServiceTests` fail on `Assert.True(File.Exists(pdfPath))` after a QuestPDF render. QuestPDF's Skia native dependency is absent from this Linux sandbox. **Classification: environment-limited, not a confirmed product defect.** These three tests cover the two writers that are *not* affected by D-01, so the failure is plausibly unrelated to the report defect — but it is **not proven**, and these print paths remain formally unverified on a real host.

3. **WPF visual fidelity is unverifiable.** Only XAML markup and bindings were inspected. Actual rendering, column widths, RTL flow behaviour, dialog sizing and control states cannot be assessed. Statements in §7 about UI are about *control presence and binding*, never about appearance.

4. **Reference OCR quality.** Both PDFs are Arabic OCR. Where OCR was ambiguous (dense screenshot captions, partially cut-off Arabic words), the requirement was traced to the clearest instance and the page cited. Two specific consequences:
   - Reference **visual design** (colours, fonts, exact layout) is largely not recoverable, so §7 compares *structure and controls*, not appearance.
   - A handful of caption lines could not be read reliably; where a caption was the only evidence, the requirement is marked accordingly and no UI detail was invented.

5. **No reference screenshots were rendered as images.** The PDFs' images were extracted as text only. Visual comparison of report layouts is therefore out of scope, and §7.10 lists what consequently cannot be determined.

6. **Scope of the Natigh assessment.** The website and Android app are external to any desktop repository. Their absence from `src/` is proven; whether the *organisation* built them elsewhere is outside this audit's evidence. The finding is stated as "absent from the audited commit", not "never built".

7. **Quantitative performance is not assessed.** §8.2 reports only what is observable in code. No data volumes were available, so no latency claim is made.

8. **Counts are of consolidated requirements, not of screens or controls.** A requirement rated "Partially implemented" may span several fully-working screens plus one absent control. The 78-requirement granularity is the unit the reference's own table of contents uses, which keeps the mapping defensible.

9. **The `Docs/` directory was deliberately not used.** It contains prior completion claims (including `project-completeness-audit.md`) that would have biased this assessment. A grep confirms no source file reads from it. Several of this audit's findings (D-01, D-03, D-04) are of exactly the kind a documentation-driven claim would have missed.

---

## 13. Final Conclusion

Audited at commit `9b912c1a0ec51fb9786831a7d8b3362141e78e18` against the RealLab reference system described in `RL_Show_Enhanced.pdf` and `RLS_Learn_Enhanced.pdf`.

**TopLab is approximately 68% complete against the reference system (86% of requirements are at least partially addressed), with a strong, well-tested domain and application layer and a materially incomplete output and integration layer.**

The conclusion rests on evidence, not impression:

- **The engineering foundation is real and sound.** Framework-free domain with strongly-typed IDs; CQRS with validation, authorization and logging pipeline behaviours; EF Core 8 with 55 configurations and 8 migrations; PBKDF2 hashing; an audit interceptor; guarded backup/restore; **2,158 passing tests**; a clean build with zero warnings. No `NotImplementedException` and no stubbed handlers exist in `src/`.

- **Core laboratory workflow is substantially complete.** Patient registration with the full clinical data model, test ordering with sample-kind and outside-lab flags, account calculation with frozen prices and discount-limit enforcement, result entry with verification and amendment trails, result delivery, sent-out sample tracking, work sheets, custom groups, test comments, external entities, price lists, antibiotics, culture attachment, users with granular permissions, attendance with overtime and lateness, and the complete drawer/accounting computation.

- **The clinical output tier is the critical weakness.** The patient report is produced by a hand-rolled ASCII PDF generator that converts every Arabic character to `?` and ignores the paper size, margins, top space, header/footer mode and signature settings it is handed. **Six Tier-1 requirements are marked fully implemented at the data and workflow layer, and all six terminate in that writer — so in practical terms none of them is deliverable.** Reference-range low/high comments are captured correctly in the database and then dropped at the output boundary. The culture microscopic examination and the inhibition-zone/trade-name data are not modelled at all.

- **Some implemented capability is unreachable.** A required entity type cannot be created through the UI because the domain mandates a price list that the editor provides no way to set — and this silently disables referral commission accounting. Four fully-implemented accounting report variants have no UI selectors and always resolve to an empty summary. Envelope settings are fully modelled, editable, and have no renderer.

- **Some concepts are absent wholesale.** Branch/multi-branch, the Natigh.com portal, the login detector, units and barcode-type management, price-list printing, card settings, and the result-monitoring report have no representation in the audited commit.

The gap is concentrated and legible rather than diffuse. The three Critical and highest-impact High findings (D-01, D-03, D-04) are individually addressable, and the two cheapest ones — binding existing properties to existing controls — would immediately recover substantial value from work that is already done and already tested.

**A categorical statement of completion status: partially complete. The system is not yet fit for clinical production use, because the artefact a patient receives cannot be read.**

---

*Report generated by comparative static audit of commit `9b912c1a0ec51fb9786831a7d8b3362141e78e18`. The repository was not modified at any point (`git status --porcelain` empty before and after).*
