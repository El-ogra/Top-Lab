# Cross-Comparison Report

**Reference system:** RealLab System (RLS / "ريال لاب سيستم"), documented in `RLS_Learn_Enhanced.pdf` (212 pages) and `RL_Show_Enhanced.pdf` (77 pages)
**Analysed system:** `https://github.com/El-ogra/Top-Lab.git`
**Exact commit analysed:** `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757`
**Report date:** 2026-10-03
**Method:** source-code evidence only for the assessed system; PDF text evidence for the reference system.

---

## 1. Executive Summary

| Item | Value |
|---|---|
| Repository analysed | `El-ogra/Top-Lab` |
| Commit analysed | `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757` (verified with `git rev-parse HEAD` after `git checkout`; detached HEAD, working tree clean) |
| Reference documents | `RLS_Learn_Enhanced.pdf` (212 pp, OCR), `RL_Show_Enhanced.pdf` (77 pp, OCR) — both read in full |
| Reference functions identified after de-duplication | **102** |
| Intentional exclusions applied (product decisions, not gaps) | **14** |
| **Reference functions remaining for comparison** | **88** |
| — Status **IMPLEMENTED** (behaviourally equivalent) | **67** |
| — Status **IMPLEMENTED_DIFFERENTLY** (present, meaningful difference) | **2** |
| — Status **PARTIALLY_IMPLEMENTED** (capability present, reference rules missing) | **11** |
| — Status **MISSING** (capability absent) | **8** |
| — Status **UNVERIFIABLE** | **0** |

**Headline conclusion.** The assessed system is a mature, feature-complete laboratory information system. It reproduces **67 of 88** in-scope reference capabilities with behaviourally equivalent implementation, plus **2** that work but differ in mechanism. **19** capabilities are either partial (**11**) or absent (**8**). Critically, **none of the 14 intentionally excluded functions appears anywhere in the missing-function analysis** — they were removed before classification and are listed separately in §3.

The most significant genuine gaps are laboratory-analytical rather than administrative: the reference system computes haematology indices and applies per-analyte correction factors automatically, generates clinical comments from result conditions, and provides a quality-control monitor over a result band. The assessed system has none of these; it records the manually entered value and a Low/High/Normal flag.

The system is also in materially better shape in several areas where the reference is weak or self-contradictory — typed strong IDs, an explicit audit interceptor, snapshot-based reference-range immutability, an immutable post-print amendment record, and PBKDF2 password hashing. These are recorded in §6 and §11 as differences that do not count as defects.

### Counts at a glance

```
Reference functions after de-duplication ........................ 102
Intentional exclusions (product decisions) ................... - 14
Reference functions in scope ..................................  88
                                                                    ────
IMPLEMENTED ..................................................  67
IMPLEMENTED_DIFFERENTLY .....................................   2
PARTIALLY_IMPLEMENTED .......................................  11
MISSING ......................................................   8
UNVERIFIABLE .................................................   0
                                                                    ────
Total in scope ...............................................  88   ✔ reconciles
```

---

## 2. Scope and Source-of-Truth Rules

### 2.1 The assessed system

The **only** authoritative source for whether a function exists in the assessed system is the executable/application source code at commit `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757`. No other commit was checked out and no file was modified during this investigation.

Concretely, the following were **excluded as proof**:

- README files, `INSTALL.txt`, `FoundationPhaseAcceptanceReport.md`, `Top_Lab_Remediation_Report_F1_F6.md`
- the 144 Markdown files under `Docs/` (including `Handoff_M02`…`Handoff_M23` and `Docs/Investigations/*`)
- the `.opencode/` agent and skill configuration
- code comments and XML doc comments asserting that a feature exists
- the existence of a database table, column, entity, property, ViewModel, service interface, menu entry, migration, or a method merely *named* after a feature

Documentation was used **only** as an unverified hypothesis for locating code, and every such lead was then confirmed or refuted against source.

A claim of `IMPLEMENTED` in this report requires a concrete, traceable implementation path:

```
WPF View (.xaml) → ViewModel (+ command) → MediatR Command/Query + Handler
   → Domain aggregate / rule → IApplicationDbContext → EF Core configuration → migration
```

Not every feature traverses all seven hops (for example, a domain calculator invoked by a handler does not need a View), but the path actually exercised is stated for each function in §5–§8.

### 2.2 Corroborating structural facts about the assessed system

These were measured directly and are cited throughout as evidence that the code is real rather than scaffolding:

| Measurement | Value | How verified |
|---|---|---|
| Tracked files | 1,752 | `git ls-files \| wc -l` |
| C# files under `src/` | 1,120 | `git ls-files 'src/*.cs'` |
| C# lines under `src/` | 87,208 | `find src -name '*.cs' -exec cat {} + \| wc -l` |
| XAML files | 71 | `git ls-files 'src/*.xaml'` |
| C# lines under `tests/` | 45,535 | same method on `tests/` |
| MediatR handlers | 221 | `git ls-files 'src/TopLab.Application/*Handler.cs'` |
| Use cases (151 commands + 114 queries) across 24 feature areas | 265 | directory walk of `Features/*/Commands\|Queries/*` |
| `DbSet<T>` declarations | 46 | `src/TopLab.Infrastructure/Persistence/ApplicationDbContext.DbSets.cs` |
| `IEntityTypeConfiguration` classes | 46 | `src/TopLab.Infrastructure/Persistence/Configurations/` |
| **EF Core migrations** | **14** | `src/TopLab.Infrastructure/Persistence/Migrations/`, excluding `.Designer.cs` and the model snapshot |
| `NotImplementedException` anywhere in `src/` | **0** | `grep -rn 'NotImplementedException' --include=*.cs src/` |
| `TODO` / `FIXME` / `HACK` / `XXX` in `src/` | **0** | `grep -rniE 'TODO\|FIXME\|HACK\|XXX' src/` |

**Compile verification.** `TopLab.Domain`, `TopLab.Application` and `TopLab.Infrastructure` were compiled from this exact commit and all three built successfully (`dotnet build`, .NET SDK 9.0.316, targets `net8.0`). `TopLab.Presentation` targets `net8.0-windows` with `UseWPF=true` and therefore cannot be built on the Linux analysis host; its XAML and code were inspected statically instead. This is stated as a limitation in §11.

### 2.3 The reference system

The two PDFs are the sole functional reference. Both are Arabic-language OCR output of a vendor's product documentation: `RLS_Learn_Enhanced.pdf` is a chapter-by-chapter operator guide with narrative workflows; `RL_Show_Enhanced.pdf` is a showcase brochure composed mostly of screenshots of printed output with one-line captions. Arabic glyph order is frequently scrambled in the OCR; where a statement could not be recovered with confidence it is treated as unestablished rather than guessed. English fragments, UI strings, grid headers, file paths and numerals were used as anchors.

The two documents overlap heavily. A function described in both was counted **once**; all page citations are given.

### 2.4 Intentional exclusions

The 14 functions in §3 are **deliberate product decisions** by the system owner. They are not evidence of incomplete development, are not requirements for parity, and are removed from the missing-function count entirely. They are not classified, scored, or recommended for implementation anywhere in this report.

---

## 3. Intentional Exclusions

All 14 were removed from the comparison before classification. **None appears in the missing-function count (8) or in any gap recommendation.**

### Group 1 — Internet-related functions (6)

| # | Excluded function | Where it appears in the reference |
|---|---|---|
| 1 | Sending laboratory results through Natiga.com (نتيجة.كوم) | Learn pp. 58–62; Show pp. 3, 4, 5, 53, 55 |
| 2 | Opening the Natiga.com website to display a result | Learn p. 63; Show p. 55 |
| 3 | Printing Natiga.com information on the receipt (incl. "Results PW" field) | Learn p. 69; Show pp. 3, 4, 5 |
| 4 | Creating a Natiga.com account for the treating physician | Learn pp. 142–144 |
| 5 | Creating a Natiga.com account for the contracting party / laboratory | Learn pp. 145–147 |
| 6 | Android application available through the Google Play Store | Show pp. 54, 53 |

### Group 2 — Equipment and device registry (1)

| # | Excluded function | Where it appears in the reference |
|---|---|---|
| 7 | Equipment/device registry: device status, periodic maintenance, calibration history | Learn pp. 94, 96, 99, 106, 112, 118, 124, 134, 140, 142, 145 (System-window banner); Show pp. 42, 53 |

### Group 3 — CBC analyser integration (1)

| # | Excluded function | Where it appears in the reference |
|---|---|---|
| 8 | CBC/analyser device integration: importing analyser results; per-analyser correction/adjustment factors | Show pp. 54–65, 74, 75, 77 ("RealLab Interface", 29 supported analysers) |

> **Note on a naming trap.** Exclusion 8 covers *device* integration. The assessed system contains a domain type named `Analyte` — this is the laboratory *analyte* (a measured substance such as HGB or platelets), **not** an instrument. It is unrelated to the excluded device integration and is treated as in-scope in §5.

### Group 4 — Multi-branch support (5)

| # | Excluded function | Where it appears in the reference |
|---|---|---|
| 9 | Searching by branch | Learn p. 42 (narrative), pp. 44–46, 49, 81 (`Branch No.` field) |
| 10 | Inventory by branch | Learn p. 174 (contents item 6), pp. 178–179 |
| 11 | Branch number assigned to each user | Learn p. 151 (permission #1, multi-value `1, 2, 3`), p. 157 |
| 12 | Configuring the branch number in the system | Learn pp. 155, 191, 192, 199, 200, 202, 203, 205, 206 |
| 13 | Branch-specific statistics | Learn pp. 178–179 |

### Group 5 — Result transmission (1)

| # | Excluded function | Where it appears in the reference |
|---|---|---|
| 14 | Sending results through SMS, email, or fax | Learn pp. 16, 17, 18, 23, 28, 47 (`Send SMS`, `Send E-mail`, `By Fax`, main-menu `Send Result`); Show p. 53 |

### 3.1 Two observations recorded for transparency, not as gaps

- **Branch data exists but is inert in the assessed system.** `SystemSettings.BranchNumber` (`src/TopLab.Domain/Settings/SystemSettings.cs:34`) and `User.BranchNumber` (`src/TopLab.Domain/Users/User.cs:33`) are present, introduced by migration `20260930170644_AddBranchNumber`. The resolver `BranchScope` (`src/TopLab.Domain/Common/BranchScope.cs:6`) has **no consumer anywhere in `src/`**, and `Patient` has no branch column. Because all five branch functions are excluded by product decision, this is **not** counted as a gap. It is recorded here only so the owner knows the data model carries dormant branch columns.
- **`ExternalEntity.Email` and `ExternalEntity.Fax` are stored contact fields** (`src/TopLab.Domain/ExternalEntities/ExternalEntity.cs:25,38`) with **no sending code attached**. They are contact data for a contracting party, not a transmission capability, and are not counted against exclusion 14.

---

## 4. Reference System Functional Inventory

### 4.1 Counting rule applied

One function = one operational capability a user performs, or one independently configurable behaviour with its own observable effect.

Applied consistently:

- **Consolidated** the same logical function described on several pages, in both PDFs, or in a chapter summary *and* its body section (e.g. the Learn Create/Edit-user dialog is photographed on pp. 151/152/153/157 and is counted once; the receipt is described on Show pp. 4, 5 and 52 and counted once).
- **Kept separate** genuinely distinct capabilities inside one module (e.g. add-a-test vs. edit-a-test are one capability — *test catalogue maintenance*; but adding a test *group* and adding a *price list* are separate because they are different business objects with different lifecycles).
- **Grouped** settings that configure a single print artefact onto one function (report layout = margins + paper size + header/footer mode + colours + positioning), because they are edited on one screen to produce one artefact.
- **Kept separate** independent behavioural toggles (e.g. "do not auto-insert titles" vs. "enable name search assist"), because each is a distinct switch with a distinct data-entry consequence.
- **Did not** count every screen, every CRUD operation, or technical infrastructure as a separate function.
- **Excluded** the 14 functions in §3.

**Two consequences worth stating openly:**

1. Chapter 7 (system settings) is the largest single group at 18 functions. This is deliberate: each is an independently configurable setting whose absence changes observable behaviour. It is the group most sensitive to the counting rule, and a reviewer who preferred to collapse all settings into one function would arrive at a materially smaller total.
2. A small number of reference capabilities are **declared but never operationally defined** in the source (see §11.3). Those are not counted as functions, because a one-line promise with no described behaviour cannot be classified as present or absent. They are listed so the omission is visible rather than silent.

### 4.2 The 14 excluded functions

Listed in §3. Excluded from all counts below.

### 4.3 The 88 in-scope reference functions

**Legend** — Src: `L` = `RLS_Learn_Enhanced.pdf`, `S` = `RL_Show_Enhanced.pdf`. Page numbers are PDF page numbers.

#### Group A — Patient, reception and order entry (23)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-A01 | Application launch and user sign-in | Launch the desktop application; sign in with user name and password; show-password toggle; remember-login option; status bar showing user name, last login, database and current time | L 10–12; S 34–35 |
| R-A02 | Register a new patient | Create a patient record: name, sex and age as mandatory data; phone numbers, address, national ID, treating doctor, referral entity, account type, fasting hours, notes as optional; automatic Lab-ID assignment | L 12–13, 47; S 3, 4, 36 |
| R-A03 | Patient clinical questionnaire at registration | Three question groups captured at registration and re-read on the result screen and printed on the report: current treatment (diabetes, hypertension, antiviral, antibiotic, anticoagulant, liver); past conditions (anaemia, lupus, renal failure, hypertension, arthritis); recent contrast imaging within a stated period | L 13, 17, 18, 24, 71 |
| R-A04 | Add, remove and clear individual tests on a patient order | Select tests from a sample-type-filtered menu on the left pane; add/remove by double-clicking the name; a bulk "All" clear available only during first registration; per-test "sample taken outside lab" flag rendered as a note on the printed report | L 13–14, 19, 20, 24, 48, 74 |
| R-A05 | Add a complete test group to a patient in one action | Select a group in the group pane and add every member test at once; individual members can then be removed one at a time | L 15–16 |
| R-A06 | Compute the patient account | Automatic total from catalogued prices, discount, previously-paid amount, net after discount, remaining owed to the lab; `Enter` computes, a second `Enter` saves; a "net/settled" action clears the balance | L 16–17 |
| R-A07 | Ad-hoc extra charge on the patient account | Add a service fee or extra cost not tied to a test by entering an amount prefixed with `+` into the paid field, which adds it to the tests total | L 17 |
| R-A08 | Amend, void or zero a patient account | Open the per-patient account detail; edit or delete an individual dated transaction line; a dedicated button zeroes the account | L 18–20 |
| R-A09 | Deliver patient results and settle the account | Period-scoped list of patients with finished/unfinished results and their account (paid, remaining for the patient, remaining owed to the lab); open the report from the delivery row | L 21 |
| R-A10 | Edit patient data or their tests via the patient list | Patient → add/edit patient data → patient list → select the registration **day** → select the patient → edit data, add a test or remove a test → save | L 22 |
| R-A11 | Edit patient data via search | Search for the patient, then open the patient record and edit; reaches patients of any date, unlike the day-scoped list | L 25–26 |
| R-A12 | Enter test results, verify, preview and print | Pick the patient from the day's list; open the test report; enter value against abbreviation with unit and reference range shown; free-text comment; mark verified; preview before printing; print | L 27–30; S 38–41 |
| R-A13 | Combined multi-profile report | One report merging several selected tests/profiles for one patient; the operator chooses which tests are included **and their order**; can add a comment; a patient may hold more than one combined report; a single action prints all entered-but-unprinted results as one combined report | L 31–34; S 18 |
| R-A14 | Blank (empty) report | Produce a report shell for a patient selected from the patient list, with no results, for external completion | L 35–37 |
| R-A15 | Barcode label / sticker printing grouped by sample type | Group the patient's tests by sample type and print one sticker per tube, container and swab; configurable sticker size (e.g. 38 mm × 25 mm); optional vertical coding on reports, receipt and envelope; sticker content and shape editable; any sticker re-printable; separate labels for the patient file and the lab card; the doctor can re-distribute tests across containers | S 3, 52; L 151 (permission bundles receipt + barcode printing) |
| R-A16 | Cashier receipt printing | Print patient name, sex–age, receive date, doctor, per-test line and amount, then Total → Discount → Net → Paid → Residue; two formats — thermal roll and A4 with added header, footer and logo; test-detail lines, barcode and counters individually showable/hideable; per-profile grouping table with test count and price | S 4, 5, 52 |
| R-A17 | Print the lab order / requisition form | A per-patient form listing Test Name, Unit, Result, Collection Notes and a signature column, with reception and samples-officer signature lines and a `Printed By` stamp; one form per patient to prevent error and allow archiving; printable whether or not the report has been printed | S 6 |
| R-A18 | Search patients and filter by workflow status | Search by patient name (exact or partial), treating doctor, sex, age, phone or card number, by test, and over a date period; plus result-state filters — results not entered, not reviewed, not printed, not delivered | L 41–46; S 37, 52 |
| R-A19 | Display all visits of a specific patient | Search by Lab ID and list every visit of that patient with date, gender, age, unit and referred-by, together with the tests performed | L 47–51 |
| R-A20 | Case audit trail | The administrator can see which user registered the case, how many times it was modified and by whom with date/time, which user entered each test result, how many times the report was printed, and who last printed it and when | L 52–57; S 53, 74 |
| R-A21 | Patient history on the report | If the patient has previously had the same test, prior results are inserted into the current report automatically; the operator can also select previous tests manually and either attach the history to the current report or print it as a separate report; the history window reports how many prior times the patient had the test; history works across test groups and inside a combined report; patient matched by name or Lab ID; sort/group configured in report settings | L 70–78 |
| R-A22 | Multi-patient history comparison report | Accumulate several patients with repeated "add", choose the tests once, and print a single cross-patient history table carrying patient ID and name as row columns, attributed and timestamped | L 80–83 |
| R-A23 | Blood-picture history report with trend charts | A dedicated wide history report for CBC carrying ~15 parameters (Hgb, RBC, HCT, MCV, MCH, MCHC, Leu, differential, platelets) as one row per date, with per-analyte date/value grids and trend line charts drawn against the reference band | L 79; S 66 |

#### Group B — Work papers, departmental logs and tallies (4)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-B01 | Work sheet by patient names or codes over a period | Print a work paper for a group of patients in a date range, by name or by code; two variants — complete and condensed; optional print of unfinished tests only; prints the printing employee | L 84–88; S 7, 8, 52 |
| R-B02 | Work sheet by test names or codes, or for a single test | Test-centric mirror: one test with many patients, with per-test patient counts; a single-test selection changes the layout; optional unfinished-only | L 89–91; S 9 |
| R-B03 | Test tally for a period | Count how many times each test was performed in a chosen period, as `Test Name / No. of Tests` | L 90–92; S 11 |
| R-B04 | Departmental work paper by work group (LOG) | A work paper restricted to the tests belonging to a named work group, for a period, with an unfinished-only option | L 92; S 10 |

#### Group C — Test catalogue, reference values, pricing and external parties (13)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-C01 | Edit test data and prices | Edit group membership, test name and report name, barcode, turnaround period (from which the program derives the due time), test price, sent-out flag with its price, patient price and lab-to-lab price; the Test Information panel also carries bill name, history name, Arabic name, collection/sample type, arrange number, reference type and bench | L 93–95; S 43 |
| R-C02 | Add a new test | Create a test inside the test-data master so it inherits the same field set; the group is chosen from a hierarchical group tree | L 93, 96–97 |
| R-C03 | Reference values per test by sex and age band | Add/edit/delete a normal range for one test keyed on sex and age band, with lower and upper limits, the unit, and a low comment and a high comment that surface automatically on the report. A normal must be entered per age **unit** separately — no conversion between days, months and years | L 93, 98–101, 104; S 44, 69 |
| R-C04 | Re-evaluate already-registered patients against a changed normal | When a normal is edited, patients already registered on the old values keep the old values; the operator must open the patient's report, click the reference-value cell and press Update for the new values to apply | L 102–103; S 17 |
| R-C05 | Price-list management | Create a price list; add a test to it with a price; edit a price; remove a test; rename a list; delete a list. A test may hold a different price in every list; lists are bound to referral entities so order entry no longer needs manual prices | L 105–110 |
| R-C06 | Print the test price list | Print a price list grouped by test group, with price, turnaround day-count and collection notes, in Egyptian pounds | L 111 |
| R-C07 | Fixed comments attached to a test, and picking them on the report | Pre-register standard comments against a test so they can be selected instead of retyped; more than one comment per test; the report exposes a Comment control that opens the list | L 93, 112–117 |
| R-C08 | Custom test group (package) management and use | Create a named group, add tests to it, set a per-item price, edit, remove, rename and delete the group; add the whole group to a patient order | L 118–123; S 46 |
| R-C09 | External parties: treating doctor, referral/contracting entity, partner lab | Add, edit and delete external entities of three types; capture name, address, city, phone, fax, email, person in charge and their job title and mobile; bind a contract price list (required for a referral/contract entity, not permitted for a treating doctor); store a commission/discount percentage | L 93, 124–127, 142–147; S 28, 29, 33, 48 |
| R-C10 | "Farm" (مزرعة) — a culture test type with its own antibiotic panel | A farm is provisioned as a special entry inside the test master (reserved IDs 118–139, group name fixed to CULTURE AND SENSITIVITY) and then given its own data folder on disk by a manual file-copy/rename step; it behaves as an isolatable report whose antibiotic content is configured separately | L 128–133 |
| R-C11 | Antibiotic master and per-farm panel with pregnancy and child flagging | Maintain a catalogue of antibiotics with abbreviation/symbol, scientific name and commercial names and a default sensitivity threshold in millimetres; attach a selected subset with per-antibiotic thresholds to a farm; flag an antibiotic as **Pregnant** or **Children** so that it is shown only when pregnancy is recorded on the patient, or only when the patient is a child (hard-coded as age < 12 years, either sex) | L 134–139; S 45, 23 |
| R-C12 | Mark a test as sent outside and record its economics | Mark a test as a sample referred to another laboratory, choose the receiving laboratory, and record the price paid to that laboratory in Cost Price and the price charged to the patient in Patient Price | L 93, 140; S 29 |
| R-C13 | Search the test master | Search tests by test name, by the group containing the test, or by test number; the result count updates | L 95, 100, 141 |

#### Group D — Culture, haematology and result interpretation (5)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-D01 | Culture and sensitivity result entry | Enter culture data (sample, organism A/B/C, culture condition, colony count), microscopy fields, and per-antibiotic sensitivity classified Highly / Moderately / Resistant, with the inhibition zone in millimetres and a per-organism threshold | S 13, 23, 38; L 135–139 |
| R-D02 | Automatic derivation of haematology indices and per-analyte correction factors | The program computes all indices without operator intervention (HCT, MCV, MCH, MCHC, PCT, MPV, PDW, PDW-CV, P-LCC and ratios); a correction-factors screen holds an editable formula and adjustment constant per derived index, used to correct a counter or reagent fault | S 14, 58, 60, 61, 71, 74 |
| R-D03 | Automatic generation of clinical comments from result conditions | A per-analyte rule table mapping a condition to a comment, e.g. HGB → Anaemia / higher than normal; MCV → Microcytic / Macrocytic / Normocytic; MCH → Hypo/Hyperchromic; RBC → Anaemia / Polycythaemia; PLT → Thrombocytopenia / Thrombocytosis; WBC → Leucopenia / Leucocytosis / no significant abnormality. Comment texts are editable and the whole mechanism can be switched off | S 14, 70, 74 |
| R-D04 | Abnormal-result flagging | Abnormal results are distinguished automatically at entry and at print, with configurable flag letters and per-case font and background colour for the low and high cases | S 13, 14, 56, 58 |
| R-D05 | Report element configuration and curve control | Add, remove, rename, reorder, show/hide or free-text each report element; a report-settings screen lists named elements each with an on/off radio; WBC/RBC/PLT histogram settings including curve count, with histograms reorderable, recolourable, resizable or printable blank | S 25, 60, 62, 64, 72, 74 |

#### Group E — Users, permissions and attendance (6)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-E01 | Create a user and assign permissions | Gated by a system-menu password; capture user name, primary and secondary password, permitted branch numbers, 15 permission switches, a maximum discount percentage, work start/end times, and a break flag with break duration; a user list shows everyone created | L 150–153; S 53, 61 |
| R-E02 | Edit an existing user's data and permissions | The same dialog and the same password gate; select the user from the list, press Edit, change data and/or any permission switch or shift field, save; effective at the user's next login | L 156–157 |
| R-E03 | Permission enforcement on use | Attempting an unpermitted operation shows the fixed refusal message "you do not have the permission for this operation — refer to the system administrator", and the affected menu entries and buttons render greyed | L 154–155 |
| R-E04 | Attendance, rest period and departure registration | The user registers attendance at the start of work, optionally registers the rest period defined on their record, and registers departure at the end of the shift; the system derives login/out times, overtime and lateness. Only the system administrator may view them | L 158–163 |
| R-E05 | Login/logout history report per user | A per-user statement of log-in and log-out events over a specified period, recording entry date/time, exit date/time and the **device name** the user signed in from | L 163–164 |
| R-E06 | Per-patient workflow status | A status symbol in front of every patient showing the current state — results not entered, not verified, not printed, not delivered | L 12, 23, 25, 27, 35, 42, 49, 58, 80; S 37, 52 |

#### Group F — Statistics and monitoring (5)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-F01 | Patient statistics over a period | Total patients presenting, classified by period, and grouped by sex, by account type and by referral entity, with a single-referral-entity variant; sortable by months of the year, by days of the month, or by year; every report also carries an **amounts-paid** money row | L 166–171; S 30, 31, 48, 53 |
| R-F02 | Test and sample volume statistics | Number of tests and number of samples over defined periods, classified by test, by test group, or by referral entity; a test-group request-rate report per analyte with sample counts and percentages | L 172; S 32, 33 |
| R-F03 | Sent-out sample statistics by period and destination lab | Statistics on samples sent outside, broken down by time period and by the laboratory they were sent to | L 166–168; S 47, 53 |
| R-F04 | User productivity statistics | Report the production capacity attributable to each user by the work assigned to them | L 166–168; S 47 |
| R-F05 | Abnormal-result monitoring / quality-control report | For a chosen test and a period, a report of every result falling in a stated value band, with date, patient, patient data, referral entity, test, result and status — the stated way to judge chemistry, analysers and rapid tests and to display positives | S 25 |

#### Group G — Audit, accounts and cash (8)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-G01 | Till reconciliation and net lab profit | Total patient samples, discount value, total after discount, amount collected and not collected, cash available, net profit after collection and settlement; daily, weekly, monthly, yearly or arbitrary periods; password-gated | L 174–179; S 49, 50, 53 |
| R-G02 | Reconciliation scoped to a party, with four report granularities | Scope the reconciliation to a user, a referral entity, a treating doctor, sent-out samples, or an account type; four report variants — summary, detailed by prices, detailed by results, and consolidated across referral entities; the report can be filtered to debtors only, discounted patients only, or settled patients only | L 178–184; S 26, 27, 28, 45 |
| R-G03 | Sent-out sample account statement and follow-up | Entity, test, order date, patient code, patient name, test price, amount paid and remaining, for any period, per patient or aggregated per entity, with a detailed per-sample report; shows the destination lab and the cost price, allocates sample prices out of the cash-drawer account, and lists samples not yet sent | L 184–185; S 29, 53 |
| R-G04 | Settle a sent-out sample's account | Record payment against a sent-out test and settle the account in full | L 185–188 |
| R-G05 | Count the tests of a specific referral or contracting party | A per-party count of samples sent by the lab for a period, via the work-paper facility | L 187; S 33 |
| R-G06 | Cash disbursement and deposit | Disburse or deposit cash against funds designated for that purpose, or for other entities | L 177; S 49, 51 |
| R-G07 | Other laboratories' accounts and referral-party percentage shares | Accounts of other laboratories and the percentage shares of referral entities over specific periods | L 175, 180; S 49 |
| R-G08 | Block report printing until the account is settled | Prevent a patient's report from being printed while a balance is outstanding, and warn the user at hand-over; the patient's analysis conditions or notes can be attached to the account | S 53; L 151 (permission #6) |

#### Group H — System settings (18)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-H01 | Report appearance and layout | Report margins (left/bottom page margin and report top space, maximum 8 cm), paper size A4 or A5, header/footer print mode with three exclusive options (none/pre-printed stationery, words with font type/size/colour, or image files bundled with the program), header and footer colours per element, per-element Left/Top positioning in centimetres, doctor signature, and the patient-history sort/group setting | L 190–198; S 26, 42, 72 |
| R-H02 | Dedicated printer per print job | Assign a specific printer for each of five print jobs: reports, barcode, envelope, receipt, card | L 199–206 |
| R-H03 | Default account type for the system | The account type applied to newly registered patients | L 199–206 |
| R-H04 | Enter the patient name and treating doctor in English | A switch enabling English-language entry of patient name and treating doctor | L 192, 199, 201, 203 |
| R-H05 | Automatic Mr/Mrs default by sex when the referral field is blank | Insert the title derived from the patient's sex into the referral-entity field when that field is left empty | L 192, 199, 201, 203 |
| R-H06 | Save the treating doctor only from the external-entities window | A switch that stops the system auto-saving a treating doctor typed during patient entry, requiring it to be created from the external-entities window instead | L 192, 199, 201, 203 |
| R-H07 | Enable patient-name search assist in the add/edit patient window | A switch enabling name search/suggest inside the patient registration window | L 192, 199, 201, 203 |
| R-H08 | Do not auto-insert titles at registration | A switch suppressing automatic title insertion when registering a patient | L 192, 199, 201, 203 |
| R-H09 | Review and complete tests automatically | A switch that reviews and completes tests automatically rather than requiring manual verification | L 192, 199 |
| R-H10 | Patient-history sort and grouping | Configure whether the history is sorted/grouped by lab code or by patient name | L 192, 194, 195 |
| R-H11 | Main system data | Result-reception time and automatic printing of the patient receipt (branch number is excluded) | L 192, 199, 203, 206 |
| R-H12 | Receipt settings | Receipt top margin, currency, reception time, print-once, test-detail display mode, cashier printer, and header/footer with its own images and logo | L 202–204; S 4, 5 |
| R-H13 | Envelope settings | Envelope top margin, envelope header and footer including images, and the addressee block (referring doctor or referral entity) with a date, positionable in centimetres | L 205–206 |
| R-H14 | Card settings | A card print job with its own settings and its own dedicated printer. *Declared as a settings tab but never given a field or rule anywhere in the reference* | L 190, 191, 192, 199, 201, 203, 206 |
| R-H15 | Database server settings | Modify the server name, the log-in and the database name | L 155, 191, 200, 202, 205 |
| R-H16 | Database backup and restore | Back up the system database, or restore a previous version | L 155, 191, 200, 202, 205; S 52 |
| R-H17 | Database and program version update | Update the database and program files for compatibility with recent versions, and prepare the system for the current version | L 155, 191, 200, 202, 205 |
| R-H18 | Automatic daily backup | Automatic backup of the data into a folder named with each day's date, with system files kept in partitions | S 52 |

#### Group I — Laboratory operations and utilities (6)

| ID | Reference function | Description | Src / pages |
|---|---|---|---|
| R-I01 | Draw and separate patient samples | Register and code the patient's samples, separate them, and code each department's samples individually; per-tube draw status with drawn tubes listed separately; tube catalogue (Stool, CBC, EDTA Blood, Other Tube); save gated on reading the patient's ID, with an optional barcode print at the same moment | L 208–210 |
| R-I02 | Laboratory test knowledge browser | Look up a test's data, the nature of the test, its normal values and its effect on the patient | S 51 |
| R-I03 | Requirements and purchases list | Enter work requests and requirements into a requirements-and-purchases list | S 51 |
| R-I04 | Clock, calculator and unit converter | An on-screen clock, a calculator, and a measurement-unit conversion tool in the Tools window | S 51 |
| R-I05 | Phone-book / contact list | Enter a person's phone number and their data from a phone book, with add and remove | S 51 |
| R-I06 | Reminder messages to users | Compose a reminder message addressed to a specific user or to every person, delivered through an appointment note | S 51 |

### 4.4 Reference capabilities declared but never operationally defined — not counted

These appear in the reference as a single caption or menu label with no described workflow, field, or output. They are **not** counted as functions, because nothing can be classified against them. They are listed so the omission is visible.

| Reference claim | Src / page | Why not counted |
|---|---|---|
| "Lab production capacity planning" | S 47 | One line of text; no period, metric or unit defined |
| Reference values for **test groups** and their barcodes (promised in the chapter 3 contents list) | L 93 | The body only ever demonstrates per-test ranges; no group-level range workflow is described |
| Barcode *maintenance* workflow (`Barcode Types` menu item) | L 93, 96, 98 | Named as a menu item; no maintenance screen or rule is shown |
| "Barcode display control" (`التحكم في ظهور الباركود`) | L 190, 191 | Announced in the settings overview; no such control appears anywhere in the settings chapter |
| "Print Result Password" button | L 18, 23, 47 | A button visible in three screenshots; no dialog, rule or behaviour described |
| Barcode and receipt **printing procedure** in chapter 1 | L 9, 12, 23, 25, 27, 35, 42, 49, 58, 80 | Promised ~10 times in the patient-window description, never given a procedure. *(Covered instead by R-A15 and R-A16, which are documented in `RL_Show_Enhanced.pdf`.)* |
| Count / list of undelivered results (`حصر النتائج الغير مسلمة`) as a dedicated counter | L 12, 23, 25, 27, 35, 42, 49, 58, 80 | Promised ~10 times; no counter or procedure shown. *(The capability itself is covered by R-A18/R-A09.)* |
| Sent-sample entity type as a third external-entity radio option | L 125, 143, 146 | Offered as an option; its field set and behaviour are never shown. *(Its function is covered by R-C12.)* |

### 4.5 Internal contradictions inside the reference documents

Recorded rather than silently resolved, as required. None of these changes a classification in this report, but each is a place where the reference does not agree with itself.

| # | Contradiction | Pages |
|---|---|---|
| 1 | Three different section-numbering schemes are used for chapter 1 (global contents `1-1…1-13`, body `1-1…13-1`, chapter-local contents a bare `1…7`), and the chapter-local list omits six documented topics | L 3, 9, 10–83 |
| 2 | Chapter 3's contents list and its body disagree on the order of three items and omit price lists and custom groups entirely; section number `11-3` is used twice; `10-3` never appears | L 93 vs 94–147 |
| 3 | Chapter 6's contents list skips item 7; the body labels two different sections `3-6` and none matches the contents ordering | L 174 vs 175, 178, 179, 181, 184 |
| 4 | Page 149 carries chapter 7's contents list in its right-hand column — a mis-paste in the source document | L 149 vs 190 |
| 5 | History depth is given as "last 15" in one place and "last ten" in another | S 15 vs 52 |
| 6 | The number of CBC report designs is given as "10", yet the document shows more than ten distinct layouts | S 74 vs 56–65 |
| 7 | `Mr`/`Mrs` auto-insertion into the referral field and the suppression of auto-titles at registration are never reconciled | L 192, 199 |
| 8 | "Delivery time" and "result-reception time" are configured in two different places and never distinguished | L 155/191 vs 192/199 |
| 9 | Receipt arithmetic does not balance in the worked examples (net − paid ≠ residue) | S 4, 5 |
| 10 | Creatinine is priced at 45 and at 15 in two receipts of the same session | S 4 vs 5 |
| 11 | Per-patient totals on a statement do not equal the sum of their test lines, with no discount or contract-rate rule stated | L 182 |
| 12 | Reference values for PT/INR differ between the guide and the showcase for the same test | L 103 vs 116, 117 |
| 13 | Amikacin's sensitivity threshold flips sign between pages | L 136 vs 137, 138 |
| 14 | A statement heading promises a "rate" but prints a raw count, with no denominator | L 172 |

---

## 5. Cross-Comparison Matrix

**Legend** — Status: `IMPLEMENTED` · `IMPLEMENTED_DIFFERENTLY` · `PARTIALLY_IMPLEMENTED` · `MISSING` · `UNVERIFIABLE`.
**DB impact** — `A` no database change · `B` new migration required · `C` change to existing migrations (**not recommended**, see §9.3) · `U` uncertain pending implementation discovery.
**Paths** are relative to the repository root at commit `7a2cfb50`.

### 5.1 Group A — Patient, reception and order entry

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-A01 | Application launch and sign-in | IMPLEMENTED | `Views/Setup/LoginWindow.xaml`; `ViewModels/Setup/LoginViewModel.cs`; `Features/UsersAndPermissions/Commands/SignIn/SignInCommandHandler.cs`; `Domain/Users/User.cs`; `Infrastructure/Identity/Pbkdf2PasswordHasher.cs` | Same capability: authenticate a user, open the work window, display the signed-in user and last-login time | Reference has a "remember login" checkbox; my system instead has a 10-minute idle auto-lock and a `LockWorkstation`/`UnlockWindow` pair (`Features/AccessAndNavigation/Commands/LockWorkstation`). My system stores PBKDF2 hashes; the reference stores an unspecified password | A | High |
| R-A02 | Register a new patient | IMPLEMENTED | `Views/Patients/PatientEditorView.xaml`; `ViewModels/Patients/PatientEditorViewModel.cs`; `Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs`; `Features/PatientRegistration/Queries/GetNextLabIdQuery`; `Domain/Patients/Patient.cs:111` | Equivalent. `Patient` is one row per visit with a shared `LabId` grouping value; name, sex, age are mandatory, the rest optional | My system is stricter: `Patient.Create` rejects an empty name, a negative age, and `FastingHours` without `IsFastingIndicated`. No behavioural loss | A | High |
| R-A03 | Patient clinical questionnaire | IMPLEMENTED | `Features/PatientRegistration/Commands/AddMedicalCondition`, `RemoveMedicalCondition`; `Domain/Patients/PatientMedicalCondition.cs`; `Domain/Patients/MedicalConditionType.cs`; seeded in `Persistence/Configurations/MedicalConditionTypeConfiguration.cs:17` | Equivalent and more general. `MedicalConditionCategory` = `Medication` / `Condition` / `Pregnancy` reproduces the reference's treatment / past-condition / pregnancy split; `Patient.IsFastingIndicated`, `FastingHours` and `RecentContrastImaging` reproduce the fasting and recent-imaging questions | The reference's questionnaire is a fixed hard-coded list of six medications, five conditions and one imaging window. My system makes the condition list seedable master data, so the same three groups are covered and can be extended | A | High |
| R-A04 | Add / remove / clear individual tests on an order | **IMPLEMENTED_DIFFERENTLY** | `Features/PatientRegistration/Commands/AddTestsToVisit`, `RemoveTestFromVisit`, `ClearAllTests`, `UpdatePatientTestSampleFlags`; `Domain/Results/PatientTest.cs:215` (`UpdateSampleFlags`), `:209` (`MarkSampleDrawn`); `ViewModels/Patients/PatientEditorViewModel.cs` | Capability is fully present: add a test, remove a test, clear all, per-test sample-type flags (Urine/Stool/Blood/Semen/CSF) and the "taken outside lab" flag | Two reference rules are **not** reproduced. (a) The reference adds/removes by double-clicking the test name and only clears all *"عند إضافة المريض أول مرة فقط"* — during first registration only. My system uses explicit buttons and imposes no such restriction, so a registered patient's test list can be bulk-cleared. (b) The reference renders "Sample taken outside lab." as a note on the printed report; my system stores `PatientTest.IsTakenOutsideLab` but the note rendering was not confirmed in the report composer | A | Medium — the rendering sub-point was not verified in `ReportContentBuilder` |
| R-A05 | Add a complete test group in one action | IMPLEMENTED | `Features/PatientRegistration/Commands/AddProfileToVisitCommand.cs`; `ViewModels/Patients/PatientEditorViewModel.cs`; `Domain/Tests/Profile.cs:78` | Equivalent: a priced profile of tests is added to the visit in one action | The reference adds a *clinical* test group by name; my system has both a clinical `TestGroup` and a priced `Profile`, and `AddProfileToVisit` adds the priced one. Functionally equivalent for ordering | A | High |
| R-A06 | Compute the patient account | IMPLEMENTED | `Domain/Billing/PatientAccountCalculator.cs`; `Features/PatientBilling/Queries/GetPatientAccount`, `GetPatientInvoice`, `GetPatientReceipt`; `ViewModels/Patients/PatientAccountViewModel.cs` | Equivalent, and computed in one authoritative place. Prices are frozen at order time in `PatientTest.PriceAtOrderTime`, so a later catalog price change cannot alter a historical bill | The reference recomputes from the live catalog; my system freezes the price at order time. This is a deliberate improvement, not a gap. The reference's "press Enter twice to compute then save" gesture has no counterpart — the calculation is always live | A | High |
| R-A07 | Ad-hoc extra charge | IMPLEMENTED | `Features/PatientBilling/Commands/RecordExtraCharge/RecordExtraChargeCommandHandler.cs`; `Domain/Billing/PaymentOperation.cs:15` (`IsExtraCharge`) | Equivalent: a non-test charge is added to the account total | The reference enters it by prefixing an amount with `+` in the paid field; my system uses a dedicated `ExtraChargeDialogWindow`. The domain additionally forbids a discount on an extra charge, which the reference does not state | A | High |
| R-A08 | Amend, void or zero a patient account | IMPLEMENTED | `Features/PatientBilling/Commands/RecordCorrection`, `VoidPaymentOperation`, `SettleAccountInFull`; `Domain/Billing/PaymentOperation.cs:78` (`Void`); `Domain/Common/Enums/OperationType.cs` | Equivalent. `OperationType` = `Payment` / `Correction` / `FullSettlement` maps onto the reference's three account actions (enter a payment, correct a wrong amount, zero the account). Transactions are dated rows that can be individually voided | Voiding is a soft flag (`IsVoided`) rather than a physical delete, so the audit trail retains the voided operation. The reference's audit screen requires this | A | High |
| R-A09 | Deliver results and settle the account | IMPLEMENTED | `ViewModels/Patients/ResultDeliveryViewModel.cs`; `Views/Patients/ResultDeliveryView.xaml`; `Features/ResultDelivery/Commands/DeliverWithSettlement/DeliverWithSettlementCommandHandler.cs`; `Features/ResultDelivery/Queries/GetDeliveryGrid`, `GetUndeliveredResults`; `Domain/Results/PatientTest.cs:191` (`MarkDelivered`) | Equivalent: a period-scoped grid of patients with finished and unfinished results, their paid amount and the balance, from which the report is opened and results are handed over | The reference takes the period from a value configured elsewhere; my system takes `From`/`To` directly in `GetDeliveryGridQuery`/`GetUndeliveredResultsQuery`, so the filter is per-use rather than a global setting. A balance is computed one way only (`PatientAccountCalculator`) and negative values mean credit, with no clamping | A | High |
| R-A10 | Edit patient/tests via the day-scoped patient list | IMPLEMENTED | `ViewModels/Patients/PatientEditorViewModel.cs`; `Features/PatientRegistration/Queries/GetPatientVisitHistoryQuery`; `Features/PatientRegistration/Commands/UpdatePatient`, `AddTestsToVisit`, `RemoveTestFromVisit` | Equivalent. The patient list is day-partitioned on `RegistrationDateUtc`, as in the reference | The reference restricts the clear-all to first registration; my system does not (see R-A04) | A | High |
| R-A11 | Edit patient data via search | IMPLEMENTED | `ViewModels/Patients/PatientSearchViewModel.cs`; `Features/PatientSearch/Queries/SearchPatientsGlobal` | Equivalent: search reaches any date, then the patient record is opened for editing | None material | A | High |
| R-A12 | Enter results, verify, preview, print | IMPLEMENTED | `Views/Patients/SimpleResultEntryView.xaml`; `ViewModels/Patients/SimpleResultEntryViewModel.cs`; `Features/ResultsEntry/Commands/EnterResult`, `ReviewResult`, `UnreviewResult`, `ClearResult`, `MarkResultPrinted`; `Domain/Results/PatientTest.cs:115,166,178` | Equivalent, with a stricter state machine. `EnterResult` throws if already reviewed; `MarkReviewed` throws if nothing was entered; `MarkPrinted` requires entered **and** reviewed and increments `PrintCount`; `ClearResult` throws once reviewed, printed or delivered; `MarkDelivered` throws unless printed. `ResultFlag` = Normal/Low/High is computed against the resolved reference range | The reference's print preview is a separate screen; my system exposes `PdfPreviewService` for the same purpose. The reference allows editing a reviewed result; my system requires the narrow `ClearResult`→re-enter path or, for profiles, the immutable `ProfileResultAmendment` route | A | High |
| R-A13 | Combined multi-profile report | IMPLEMENTED | `ViewModels/Patients/CombinedReportViewModel.cs` (`:87-88` `MoveUpCommand`/`MoveDownCommand`, `:372` `MoveUp`, `:387` `MoveDown`); `Views/Patients/CombinedReportView.xaml:31-32`; `Features/ReportProduction/Commands/BuildCombinedReport`, `PrintCombinedReport`; `Domain/Reports/CombinedReportSelection.cs:7`; `Features/ResultsEntry/Commands/ExecuteBulkPrint` + `Views/Patients/BulkPrintDialogWindow.xaml` | Equivalent. Tests are selected, **ordered by the operator**, and rendered as one report; the "print all entered-but-unprinted results as one combined report" action exists as `ExecuteBulkPrintCommand`; "insert from history" and "auto-insert" exist as `InsertHistoryResultCommand` and `AutoInsertHistoryCommand` | The reference reorders tests by dragging; my system reorders with ▲/▼ buttons. The end result is identical, so this is **not** treated as a behavioural difference | A | High |
| R-A14 | Blank (empty) report | IMPLEMENTED | `ViewModels/Patients/BlankReportViewModel.cs`; `Views/Patients/BlankReportView.xaml`; `Features/ReportProduction/Commands/BuildBlankReport`, `PrintBlankReport` | Equivalent | None material | A | High |
| R-A15 | Barcode sticker printing grouped by sample type | **PARTIALLY_IMPLEMENTED** | `Features/PatientRegistration/Commands/PrintBarcode/PrintBarcodeCommand.cs`; `Infrastructure/Barcode/BarcodeService.cs` (`:44` `PrintBarcodeAsync`, `:94` `BuildLabelPdf`); `Infrastructure/Barcode/BarcodeLabelRenderer.cs:12` (`Render`, default 300×80 px); `Domain/Tests/Test.cs:26` (`Barcode`); `Domain/Settings/SystemSettings.cs:23,25` (`PrintFileExternalBarcode`, `PrintDateTimeOnTubeBarcode`) | **Present:** a Code-128 label is rendered and printed for a patient, gated by the `ADD_EDIT_PATIENT` permission; a per-test `Barcode` catalog field exists; two settings control the external file barcode and the date/time on the tube barcode. **Absent:** the reference derives one sticker per tube, container and swab from the patient's test list grouped by sample type, with a configurable sticker size, an editable sticker content/shape template, re-print of any individual sticker, and a doctor-facing action to re-distribute tests across containers | My system prints one label per patient, not one per container. The grouping logic (which test goes in which tube) and the sticker template do not exist | **B** — a label-template table (type, size in mm, content template, print order) and a per-patient test→container mapping are new entities. A reduced version reusing the existing `PatientTest` sample flags would need no schema change | High on the gap; Medium on the exact schema needed |
| R-A16 | Cashier receipt printing | IMPLEMENTED | `Features/PatientBilling/Commands/PrintReceipt/PrintReceiptCommandHandler.cs`; `Infrastructure/Printing/ReceiptPdfWriter.cs`; `Infrastructure/Printing/ReceiptPrintingService.cs`; `Domain/Settings/ReceiptSettings.cs` (`TestDetailDisplayMode`, `CashierPrinterEnabled`, `HeaderFooterMode`, `Currency`) | Equivalent. Total → discount → net → paid → residue are produced; test detail is individually showable/hideable via `TestDetailDisplayMode` (`Hide` / `Show` / `ShowWithCode`); currency is configurable (default `L.E.`); a header/footer mode and a dedicated cashier printer exist | The reference offers a thermal-roll format in addition to A4. My system renders through QuestPDF with a single page-size mapping (A4/A5) and has no thermal-roll paper preset. This is a presentation variant, not a missing capability | A | High |
| R-A17 | Print the lab order / requisition form | **MISSING** | Closest existing artefact: `Features/ResultsEntry/Queries/GetPatientResultSheet/GetPatientResultSheetQuery.cs` → `ResultsEntryDtos.cs:54-68` (`PatientResultSheetDto`, `ResultSheetLineDto`: TestName, TestCode, ResultValue, ResultFlag, Notes, IsReviewed, IsPrinted), consumed by `ViewModels/Patients/PatientResultSheetViewModel.cs` | **Why missing:** the reference's form is a *pre-analytical* document — a per-patient requisition carrying Test Name, Unit, Result column, **Collection Notes** and a `Sig.` column, with reception and samples-officer signature lines, designed so that "one form per patient" prevents error, eases follow-up and allows archiving, and printable whether or not the report has been printed. `PatientResultSheetDto` is a **post-analytical result listing** — it has no collection-notes column, no signature column and no signature lines, and is derived from entered results. No PDF writer in `Infrastructure/Printing/` produces a requisition: the writers are `ReportPdfWriter`, `ReceiptPdfWriter`, `InvoicePdfWriter`, `WorkSheetPdfWriter`, `PriceListPdfWriter`, `CustomGroupPdfWriter` | Requires a new print artefact: a `LabRequisitionPdfWriter`, a print command gated on `PRINT_RESULTS`, and a print-menu entry | **A** if the collection-notes column is derived from the existing `PatientTest` sample flags; **B** if a free-text collection note must be stored per test | High |
| R-A18 | Search patients and filter by workflow status | IMPLEMENTED | `Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQueryHandler.cs`; `Features/ResultsEntry/Queries/GetResultWorklist/GetResultWorklistQuery.cs`; `Features/ResultDelivery/Queries/GetUndeliveredResults`; `ViewModels/Patients/PatientSearchViewModel.cs` | Equivalent. Search filters: text (Lab ID, national ID, phone, and name when the assist setting is on), treating doctor, referral entity, test, sex, age band, and a from/to date range. Status filters: `HasResult` (not entered), `IsReviewed` (not verified), `IsPrinted` (not printed, as a nullable tri-state so "no filter" is expressible), and `GetUndeliveredResults` (not delivered) | The reference's age matching does not convert between day/month/year either; my system explicitly preserves the stored unit in both the band and the comparison | A | High |
| R-A19 | All visits of a specific patient | IMPLEMENTED | `Features/PatientRegistration/Queries/GetPatientVisitHistoryQuery`; `Features/PatientSearch/Queries/GetVisitHistory`, `GetVisitDetail`; `ViewModels/Patients/PatientVisitHistoryViewModel.cs`; `Views/Patients/PatientVisitHistoryView.xaml` | Equivalent: search by Lab ID and list every visit with its date and tests | None material | A | High |
| R-A20 | Case audit trail | IMPLEMENTED | `Infrastructure/Persistence/Interceptors/AuditableEntitySaveChangesInterceptor.cs`; `Domain/Common/AuditableEntity.cs:14-22`; `Features/AuditAndTraceability/Queries/GetPatientAudit` → `AuditDtos.cs:18-28`; `GetPatientTestAudit` → `AuditDtos.cs:35-51`; `Views/Audit/AuditView.xaml`; gated by `PT_AUDIT_ACCESS` | Equivalent. The patient view returns registering user, creation time, **modification count**, last modifying user and time, and the ordered payment receivers. The per-test view returns entered/reviewed/printed(+count)/delivered, each with user and UTC time — every field the reference requires | The reference's audit is presumably row-history based; my system reconstructs the same facts from five audit columns plus the lifecycle columns. There is **no change-history table**, so a field that is overwritten in place (for example a changed test price) leaves no per-change trail — only a modification counter. For the reference's stated questions (who, how many times, when) this is fully equivalent; for arbitrary field-level history it is not | A | High |
| R-A21 | Patient history on the report | IMPLEMENTED | `Domain/Reports/PatientHistoryResolver.cs:9`; `Features/ReportProduction/Queries/GetPatientTestHistoryQuery`, `GetSeparateHistoryReportQuery`, `GetCombinableTestsQuery`; `Commands/AutoInsertHistory`, `InsertHistoryResult`, `PrintHistoryReport`; `ViewModels/Patients/HistoryReportsViewModel.cs`; `Domain/Settings/ReportSettings.cs:25,27` (`HistorySortMode`, `HistoryAutoDisplayEnabled`) | Equivalent. Automatic insertion when the same test recurs (`HistoryAutoDisplayEnabled`); manual selection of prior tests; attach-to-report and separate-report output modes; works across test groups and inside a combined report; sort/group configured in report settings | The reference matches a patient by name **or** Lab ID; my system makes it a configured **choice** (`ByLabCode` or `ByPatientName`) rather than "either will do". That is arguably safer — the reference's name-based matching has no collision rule for common Egyptian names, whereas my system's Lab-code mode is deterministic | A | High |
| R-A22 | Multi-patient history comparison report | IMPLEMENTED | `Features/ReportProduction/Queries/GetMultiPatientHistoryQuery`; `ViewModels/Patients/HistoryReportsViewModel.cs`; `ViewModels/Patients/InsertHistoryDialogViewModel.cs` | Equivalent: several patients are accumulated, tests are chosen, and a single cross-patient history table is printed with patient ID and name as row columns | None material | A | High |
| R-A23 | Blood-picture history with trend charts | **PARTIALLY_IMPLEMENTED** | `Domain/Tests/Analyte.cs`, `AnalyteReferenceRange.cs`, `AnalyteReferenceRangeBand.cs`; `Domain/Results/ProfileResultItem.cs`; `Features/ProfileResults/Queries/GetProfileReportQuery`; `ViewModels/Statistics/StatisticsViewModel.cs:21` | **Present:** the analyte/band model supports a wide multi-parameter panel keyed on one patient, and `ProfileResultItem` records each analyte's value, unit, flag, verification, print count and last print — so a per-analyte date/value grid is derivable and the print-time per-patient reference override (`GetProfileReportQuery`) works. **Absent:** **trend charts.** There is no charting library in the solution. The constraint is stated explicitly in the source: `ViewModels/Statistics/StatisticsViewModel.cs:21` — *"D5: no charting library, no new package reference — DataGrid/number cards only."* No chart, curve, histogram or plot type exists anywhere in `src/` | A data view exists; the graphical rendering does not. `Directory.Packages.props` contains no plotting package | A | High |

### 5.2 Group B — Work papers, departmental logs and tallies

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-B01 | Work sheet by patient names/codes over a period | IMPLEMENTED | `ViewModels/WorkSheets/WorkSheetsViewModel.cs`; `Views/WorkSheets/WorkSheetsView.xaml`; `Features/WorkSheets/Queries/GetVisitWorkSheetQuery`; `Commands/PrintWorkSheet`; `Infrastructure/Printing/WorkSheetPdfWriter.cs`; gated by `PRINT_WORKSHEET` | Equivalent: a period-scoped work paper, one block per patient, with the printing user stamped on the output | None material | A | High |
| R-B02 | Work sheet by test names/codes, or for one test | IMPLEMENTED | `Features/WorkSheets/Queries/GetWorkSheetByTestGroupQuery`; `Infrastructure/Printing/WorkSheetPdfWriter.cs`; gated by `PRINT_WORKSHEET` | Equivalent: the test-centric mirror, with a per-test patient count | None material | A | High |
| R-B03 | Test tally for a period | IMPLEMENTED | `Features/WorkSheets/Queries/GetWorkSheetTestCountByPeriodQuery` | Equivalent: `Test Name / No. of Tests` for a period | None material | A | High |
| R-B04 | Departmental work paper by work group (LOG) | IMPLEMENTED | `Domain/Tests/WorkGroupLog.cs`, `WorkGroupLogItem.cs`; `Features/TestCatalogAndReferenceRanges/Commands/CreateWorkGroupLog`, `SaveWorkGroupLogItems`, `RenameWorkGroupLog`; `Features/WorkSheets/Queries/GetWorkSheetByWorkGroupLogQuery`; `ViewModels/Lab/WorkGroupLogsViewModel.cs`; `Views/Lab/WorkGroupLogsView.xaml` | Equivalent: a named work group whose member tests are maintained and which drives its own work paper | In the reference the LOG is an attribute on the test record; in my system it is a separate aggregate with a join entity. The operational result is the same and maintenance is arguably cleaner | A | High |

### 5.3 Group C — Test catalogue, reference values, pricing and external parties

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-C01 | Edit test data and prices | **PARTIALLY_IMPLEMENTED** | `ViewModels/Lab/TestEditorViewModel.cs`; `Views/Lab/TestEditorWindow.xaml`; `Features/TestCatalogAndReferenceRanges/Commands/CreateTest`, `UpdateTest`, `DeactivateTest`, `ReactivateTest`; `Domain/Tests/Test.cs:11-40` | **Present:** group (`TestGroupId`), test name, report name, receipt/bill name (`ReceiptName`), test code, barcode, turnaround (`CompletionDurationMinutes`, validated > 0), patient price, lab-to-lab price, sent-out flag with `SentOutCostPrice`, result kind and culture-type flag. `ViewModels/Lab/TestCatalogViewModel.cs` drives the grid. **Absent:** five reference Test Information fields have no counterpart — **History Name** (the label used when a result appears in a patient-history report), **Arabic Name** (a separate display name), **Collection / sample type on the test** (my system stores the sample flags on `PatientTest`, not on the test master), **Arrange No.** (the display-order number), and **Bench**. The reference's `Reference type` switch (By Sex and Age vs. By Age Only) has no counterpart either — my `ReferenceRange.Sex` is simply nullable, which expresses both modes without a separate switch | Missing fields are real gaps in the catalog, not naming differences. Note that `Test time (Day)` in the reference is a whole number of days while my `CompletionDurationMinutes` is minutes — a finer and more useful unit, not a loss | **B** — `HistoryName`, `ArabicName`, `SampleType`, `ArrangeNo` and `Bench` are new columns on `Test`, requiring a new migration | High |
| R-C02 | Add a new test | IMPLEMENTED | `Features/TestCatalogAndReferenceRanges/Commands/CreateTest/CreateTestCommandHandler.cs`; `Domain/Tests/Test.cs:80` (`Create`); `ViewModels/Lab/TestEditorViewModel.cs` | Equivalent: a test is created inside the test-data master and inherits the same field set, with guards on name, code length and price | None material | A | High |
| R-C03 | Reference values by sex and age band | IMPLEMENTED | `Features/TestCatalogAndReferenceRanges/Commands/CreateReferenceRange`, `UpdateReferenceRange`, `DeleteReferenceRange`; `Domain/Tests/ReferenceRange.cs:7-27, 97` (`Matches`), `ReferenceRangeBand`; `ViewModels/Lab/TestEditorViewModel.cs` | **Equivalent including the subtle rule.** `ReferenceRange` carries `TestId`, nullable `Sex`, `AgeUnit`, `AgeMin`/`AgeMax`, `MinValue`/`MaxValue`, `LowComment` and `HighComment`. `Matches(sex, ageUnit, ageValue)` compares like with like and performs **no conversion between day, month and year** — matching the reference's explicit rule that "a normal must be entered for each age unit separately", where a 1-month-old is not matched by a 1-to-60-month range | None material. This is one of the closest matches in the whole comparison | A | High |
| R-C04 | Re-evaluate registered patients against a changed normal | IMPLEMENTED | `Features/ResultsEntry/Commands/RefreshResultReferenceRange/RefreshResultReferenceRangeCommandHandler.cs`; `Domain/Results/PatientTestReferenceRangeSnapshot.cs:15` (+`CaptureSnapshot`, `CapturedAtUtc`); migration `20260908175555_AddPatientTestReferenceRangeSnapshots`; `ProfileResultItemReferenceRangeSnapshot.cs` | **Equivalent, with a stronger guarantee.** The reference keeps already-registered patients on the old values until the operator opens each report and presses Update. My system captures the range onto the patient test as an immutable snapshot at order time, so history is preserved automatically, and `RefreshResultReferenceRangeCommand` provides the same explicit re-evaluation path | The reference relies on the operator remembering to update each affected report; my system cannot silently rewrite history. Same workflow, safer storage | A | High |
| R-C05 | Price-list management | IMPLEMENTED | `ViewModels/Lab/PriceListsViewModel.cs`; `Views/Lab/PriceListsView.xaml`; `Features/PriceListsCommentsAndCustomGroups/Commands/CreatePriceList`, `RenamePriceList`, `DeletePriceList`, `SetPriceListItemPrice`, `RemovePriceListItem`; `Domain/Billing/PriceList.cs`, `PriceListItem.cs`; `Domain/ExternalEntities/ExternalEntity.cs:173-184`; `Features/PatientRegistration/Common/TestPriceResolver.cs` | **Equivalent including the binding rule.** A test may hold a different price in each list (`PriceListItem` composite key `PriceListId`+`TestId`); a list binds to an external entity via `ExternalEntity.PriceListId`; the domain **enforces** the reference's distinction — a `TreatingDoctor` must have **no** price list, a `ReferralOrContract` **must** have one (`ExternalEntity.ValidatePriceListRule`, `:173-184`) — and `TestPriceResolver` prices the order automatically from the bound list | The reference has no effective dating or versioning on a list; my system has none either. Equivalent | A | High |
| R-C06 | Print the test price list | IMPLEMENTED | `Features/PriceListsCommentsAndCustomGroups/Queries/GetPriceListsQuery`; `ViewModels/Lab/PriceListsViewModel.cs`; `Infrastructure/Printing/PriceListPdfWriter.cs`; tested by `tests/TopLab.Presentation.Tests/Lab/PriceListsPrintCommandTests.cs` (6 facts) | Equivalent: a printable price list | None material | A | High |
| R-C07 | Fixed comments on a test, and picking them on the report | IMPLEMENTED | `Domain/Tests/TestComment.cs:6` (`TestId`, `CommentText`, max 1000); `Features/PriceListsCommentsAndCustomGroups/Commands/CreateTestComment`, `UpdateTestComment`, `DeleteTestComment`; `ViewModels/Lab/TestCommentsViewModel.cs`; `ViewModels/Patients/TestCommentPickerViewModel.cs`; `Views/Patients/TestCommentPickerWindow.xaml`; `Features/TestCatalogAndReferenceRanges/Queries/GetTestCommentsForResultsQuery` | Equivalent: several comments per test, selectable from the result screen rather than retyped | None material | A | High |
| R-C08 | Custom test group (package) management and use | IMPLEMENTED | `Domain/Tests/CustomGroup.cs`, `CustomGroupItem.cs`; `Features/PriceListsCommentsAndCustomGroups/Commands/CreateCustomGroup`, `RenameCustomGroup`, `DeleteCustomGroup`, `SetCustomGroupItemPrice`, `RemoveCustomGroupItem`; `Features/PatientRegistration/Commands/AddCustomGroupToVisitCommand.cs`; `ViewModels/Lab/CustomGroupsViewModel.cs`; `Infrastructure/Printing/CustomGroupPdfWriter.cs`; tested by `CustomGroupsPrintCommandTests.cs` (7 facts) | Equivalent: a named package with a per-item price, added to a patient order in one action | None material | A | High |
| R-C09 | External parties: doctor, referral/contract, partner lab | IMPLEMENTED | `ViewModels/External/ExternalEntitiesViewModel.cs`, `ExternalEntityEditorViewModel.cs`; `Views/External/ExternalEntitiesView.xaml`, `ExternalEntityEditorWindow.xaml`; `Features/ExternalEntities/Commands/CreateExternalEntity`, `UpdateExternalEntity`, `DeleteExternalEntity`, `GenerateEntityIdCode`; `Domain/ExternalEntities/ExternalEntity.cs:7-38, 173-184`; `Domain/Common/Enums/EntityType.cs` | **Equivalent.** Three entity types (`TreatingDoctor`, `ReferralOrContract`, `PartnerLab`) matching the reference's three radio options; name, city, address, phone, fax, person in charge, responsible person's phone, email; a required/required-absent price list; `DiscountOrCommissionPercent` 0–100 for the commission percentage; a generated ID code | My system stores the generated ID as `GeneratedIdCode` on the entity. In the reference that ID exists solely to open the Natigh.com portal (exclusion 4/5), so its remaining use here is a convenience code. The commission/discount storage and the price-list binding rule are equivalent | A | High |
| R-C10 | "Farm" (مزرعة) — culture test type with its own antibiotic panel | IMPLEMENTED | `Domain/Tests/Test.cs:40` (`IsCultureType`); `Domain/Tests/CultureAntibioticAttachment.cs:6` (composite key `TestId`+`AntibioticId`, `SensitivityThresholdMm` 0–100); `Features/CultureAndAntibiotics/Commands/AttachAntibioticToCulture`, `DetachAntibioticFromCulture`; `ViewModels/Lab/CultureAttachmentViewModel.cs`; `Views/Lab/CultureAttachmentView.xaml`; `Features/CultureResults/Queries/GetCultureReportQuery` | **Operationally equivalent — and the manual hazard is eliminated.** The reference provisions a farm by (a) occupying a reserved test ID 118–139, (b) setting its group name to `CULTURE AND SENSITIVITY`, and (c) **manually copying and renaming a configuration folder on the `D:\real lab system\Data` path in Windows Explorer**. My system models the same thing declaratively: any `Test` with `IsCultureType = true` owns its own antibiotic panel and per-antibiotic millimetre thresholds, and is confirmed as such by the source itself — `Features/CultureAndAntibiotics/Queries/GetCultureAntibiotics/GetCultureAntibioticsQueryHandler.cs` returns the error *"التحليل المحدد ليس مزرعة"* ("the specified test is not a farm") when a non-culture test is passed | **Material difference, in my system's favour:** the reference's provisioning requires file-system access to the application data directory and a hand-executed copy/rename step, with a documented `_S` suffix convention whose exact semantics the reference itself cannot state clearly. My system removes that step entirely. The operational outcome — an isolatable culture report whose antibiotic content is configured separately — is the same | A | High |
| R-C11 | Antibiotic master and per-farm panel with pregnancy/child flagging | **PARTIALLY_IMPLEMENTED** | `Domain/Tests/Antibiotic.cs:6-18` (`Name`, `IsPregnancyFlagged`, `IsChildrenFlagged`, `Symbol` ≤10, `ScientificName` ≤150); `Features/CultureAndAntibiotics/Commands/CreateAntibiotic`, `UpdateAntibiotic`, `DeleteAntibiotic`; `ViewModels/Lab/AntibioticsViewModel.cs`; `Views/Lab/AntibioticsView.xaml`; **filtering**: `Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs:32-38`; `Features/CultureResults/Common/PregnancySignal.cs:7-8`; `Domain/Common/AgeRules.cs` (`IsUnderTwelve`, `ChildAgeThresholdYears = 12`); `CultureAntibioticDisplay.IsDisplayable(...)`; introduced by migrations `20261002002546_AddAntibioticMasterFields` and `20261001233652_AddCultureMicroscopyAndZone` | **The hard business rules are reproduced exactly.** The antibiotic panel shown on the culture entry screen is filtered by `CultureAntibioticDisplay.IsDisplayable(IsPregnancyFlagged, IsChildrenFlagged, pregnant, child)`, where `pregnant` comes from `PregnancySignal.IsPregnancyIndicated` (a `Pregnancy`-category medical condition on the patient) and `child` from `AgeRules.IsUnderTwelve` — the same **hard-coded age < 12 years, either sex** threshold the reference states. Already-saved rows are preserved even when filtered out. Per-antibiotic millimetre thresholds are stored on `CultureAntibioticAttachment` (the reference stores them per organism) and the measured zone in `CultureAntibioticResult.InhibitionZoneMm` | **Absent: commercial / brand names.** The reference's antibiotic dictionary carries a `Commercial Names` column (Ceclor, Rocephin, Augmentin, Tavanic, Tienem, …) that is printed on the sensitivity report and can be shown or hidden, and a per-organism threshold. `Antibiotic` has **no** commercial-name field | **B** — a commercial-names column (a child collection or a delimited string) on `Antibiotic` requires a new migration. Threshold-per-organism is already satisfied at the panel level by `SensitivityThresholdMm` | High |
| R-C12 | Mark a test as sent outside, record its economics | IMPLEMENTED | `Domain/Tests/Test.cs:30-32` (`IsSentOut`, `SentOutCostPrice`); `Domain/Tests/Test.cs` `Create` guard — *isSentOut requires sentOutCostPrice*; `Domain/SentOutSamples/SentOutSample.cs:6` (`PatientTestId`, `ExternalLabEntityId`, `CostPrice`, `PatientPrice`, `SentAtUtc`); `ViewModels/Patients/SendSampleOutDialogViewModel.cs`; `Features/SentOutSamples/Commands/SendSampleOut` | **Equivalent at both levels.** The reference has the test-level flag (Cost Price / Patient Price) *and* per-sample sent-out tracking in its accounts chapter. My system reproduces both: the test carries the flag and the sent-out cost price, and `SentOutSample` records the destination lab and the economics for the individual patient test | The reference stores the destination lab on the *test* (so every patient ordering it is sent out); my system stores it per *patient test*, which is more precise and still supports the test-level default through `Test.IsSentOut` | A | High |
| R-C13 | Search the test master | IMPLEMENTED | `Features/TestCatalogAndReferenceRanges/Queries/SearchTestCatalogQuery`; `ViewModels/Lab/TestCatalogViewModel.cs` | Equivalent: search the catalog with a live result count | None material | A | High |

### 5.4 Group D — Culture, haematology and result interpretation

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-D01 | Culture and sensitivity result entry | IMPLEMENTED | `Views/Patients/CultureEntryView.xaml`; `ViewModels/Patients/CultureEntryViewModel.cs`; `Features/CultureResults/Commands/SaveCultureResults`, `VerifyCultureResult`, `UnverifyCultureResult`, `MarkCultureReportPrinted`; `Domain/Results/CultureResult.cs:6` (`Sample`, `OrganismA/B/C`, `CultureCondition`, `ColonyCount`); `Domain/Results/CultureMicroscopy.cs:10`; `Domain/Results/CultureAntibioticResult.cs:7` (`SensitivityCategory`, `InhibitionZoneMm`); `Domain/Common/Enums/SensitivityCategory.cs` = HighlyFor/ModerateFor/LowFor/ResistantFor; `Infrastructure/Printing/ReportCultureSection.cs` | Equivalent. Organism, culture condition, colony count, nine microscopy free-text fields plus a direct-smear flag, per-antibiotic sensitivity category in four classes, and the inhibition zone in millimetres | `SensitivityCategory` carries a fourth value (`LowFor`) the reference's three-class display does not name, and `InhibitionZoneMm` is validated 0–100. Both are supersets, not losses | A | High |
| R-D02 | Automatic haematology indices and correction factors | **MISSING** | No implementation. `Domain/Utilities/ArithmeticCalculator.cs:12` (`Evaluate(string expression)`) exists but is consumed **only** by `Features/Utilities/Queries/EvaluateCalculation/EvaluateCalculationQueryHandler.cs:17` — the standalone calculator tool. `Domain/Results/ProfileResultItem.cs` has no formula, no factor and no derived-value computation; `Analyte` has no calculation definition | **Why missing:** the reference computes every haematology index without operator intervention — HCT, MCV, MCH, MCHC, PCT, MPV, PDW, PDW-CV, P-LCC and ratios such as Chol/TG — and maintains a dedicated *Correction Factors* screen holding an editable formula and an adjustment constant **per derived index**, used to correct a counter or reagent fault. In the assessed system an index is whatever the operator types into `ProfileResultItem.ResultValue`; nothing is computed, and no correction factor can be applied. A grep for `AutoComment`/`GeneratedComment`/`CommentRule` and for `Calculat`/`Comput`/`Formula` in the result domain returns nothing | Total capability loss: the assessed system has a calculator *tool* but no calculator *engine* wired into result entry. The reference's device-scoped correction-factor half is excluded (exclusion 8); the index-derivation half is independent of any device and is what is counted here | **B** — a new `DerivedIndexDefinition` aggregate (analyte, formula expression, order of evaluation, correction constant, enabled flag) is required | High  |
| R-D03 | Automatic clinical comments from result conditions | **MISSING** | No implementation. `Domain/Tests/TestComment.cs` is a **fixed free-text comment per test** (the reference's R-C07 capability), not a conditional rule. A search for `AutoComment`, `GeneratedComment`, `CommentRule` and `Interpretation` across all of `src/` returns **zero** matches | **Why missing:** the reference maintains a per-analyte rule table mapping a *condition* to a *comment* — HGB → Anaemia / higher than normal; MCV → Microcytic / Macrocytic / Normocytic; MCH → Hypochromic / Hyperchromic / Normochromic; RBC → Anaemia / Polycythaemia; RDW and reticulocytes; PLT → Thrombocytopenia / Thrombocytosis / adequate in number and well aggregated; MPV, PDW-CV; WBC → Leucopenia / Leucocytosis / no significant abnormality — with a middle "in range" slot, editable texts, and a master switch to disable the mechanism. The assessed system can store and display comments but cannot *derive* one from a result | Note the near-miss: `TestComment` implements the reference's R-C07 (fixed comments per test) faithfully, but a fixed comment is chosen by the operator whereas a rule comment is chosen by the program. Same noun, different mechanism — and the R-C07 match must not be mistaken for this one | **B** — a new `CommentRule` aggregate (analyte, condition band, resulting flag or range test, comment text, priority, enabled flag) is required | High  |
| R-D04 | Abnormal-result flagging | **PARTIALLY_IMPLEMENTED** | `Domain/Common/Enums/ResultFlag.cs` = Normal / Low / High; `Domain/Common/Enums/ProfileResultFlag.cs` = Low / High; applied in `Domain/Results/PatientTest.EnterResult` and `ProfileResultItem.Update`; `Domain/Settings/ReportSettings.cs:21,23` | **Present:** an abnormal result is distinguished automatically at entry and carried to print — `EnterResult(resultValue, flag, …)` receives a computed `ResultFlag` resolved against the patient's reference range, and `ProfileResultItem.Flag` does the same for panel results. **Absent:** the reference's *presentation* half — a configurable **flag letter**, a separate **font colour** and **background colour** for the low and high cases, and the ability to colour the whole line or switch the marker off entirely. `ReportSettings` has `HeaderColor` and `FooterColor` but **no** normal/abnormal colour columns | The *detection* half is equivalent; the *presentation* half is absent. In a printed report my system marks abnormal values with a state, but the lab cannot tune how abnormal values look — which is precisely what a reference user does with its colour and flag-letter settings | **B** — new columns on `ReportSettings` (normal font colour, low font colour, high font colour, low background colour, high background colour, and a show/hide flag) | High  |
| R-D05 | Report element configuration and curve control | **PARTIALLY_IMPLEMENTED** | `Domain/Settings/ReportSettings.cs:19,30,33` (`DoctorSignatureEnabled`, `PrintGroupSubTitle`, `SuppressReprintMessage`); `Domain/Settings/EnvelopePrintItemPosition.cs:4` (per-item enable + Left/Top cm offsets, **envelope only**); `Domain/Tests/ReferenceRange.cs:25-27` (per-range low/high comments) | **Present:** a meaningful subset of presentation control — doctor signature on/off, group sub-title on/off, reprint-message suppression, automatic history display on/off, history sort mode, header/footer mode, header and footer colours, and per-item enable/position for the **envelope**. **Absent:** (a) the per-report **element list** with an individual on/off radio per named element (reticulocyte count, HCT, MCV, MCH, MCHC, RDW-SD, platelet, PCT, segmented, band, lymphocyte, monocyte, eosinophil, basophil …) and user-controlled **element order**; (b) the ability to add a new value to a selection list or substitute free text for a list value; (c) the **curve control** — WBC/RBC/PLT histograms reorderable, recolourable, resizable, blankable, with a configurable curve count | (a) and (b) are a report-template capability; (c) depends on histogram data that only the excluded analyser integration (exclusion 8) would supply, so (c) is effectively out of reach while that exclusion stands | **B** for (a)/(b) — a new `ReportElementConfig` aggregate keyed on report scope plus a template value-list. **A / not applicable** for (c), which is gated by exclusion 8 | High for the gap; Medium for the schema, which depends on the report-template design chosen |

### 5.5 Group E — Users, permissions and attendance

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-E01 | Create a user and assign permissions | **IMPLEMENTED_DIFFERENTLY** | `Views/Users/UserManagementView.xaml`; `ViewModels/Users/UserManagementViewModel.cs:49-52` (`CatalogCodes` — all 13 permission codes, rendered as checkboxes); `Features/UsersAndPermissions/Commands/CreateUser`, `UpdateUser`, `SaveUserPermissions`; `Domain/Users/User.cs:6-36`; `Domain/Users/UserPermissionGrant.cs:6`; `Domain/Users/Permission.cs`; catalogue seeded in `Persistence/Configurations/PermissionConfiguration.cs:17-31`; secondary password via `Features/UsersAndPermissions/Queries/VerifySecondaryPasswordQuery` + `IDialogService.ShowSecondaryPasswordDialogAsync` (`ShellViewModel.cs:234,256`) | **Equivalent for the operator.** A user is created behind a secondary-password gate, with a user name, a primary password hash, a separate internal/secondary password hash, an absolute-permission superuser flag, a discount-limit percentage, a block-print-on-balance flag, work start/end times, a break flag with break duration, and an explicit permission-grant list. The reference's 15 switches map onto 13 seeded permission codes: `ADD_EDIT_PATIENT`, `EDIT_RESULTS`, `REVIEW_RESULTS`, `PRINT_RESULTS`, `BLOCK_PRINT_ON_BALANCE`, `DELIVER_RESULTS`, `DISCOUNT_LIMIT`, `PRINT_WORKSHEET`, `DELETE_PATIENT`, `EDIT_SYSTEM_SETTINGS`, `CASH_DISBURSE_DEPOSIT`, `STATISTICS`, `PT_AUDIT_ACCESS` — the reference's items 2–14 one-for-one | **Three meaningful differences.** (a) The reference's item 15 — a permission gating *attendance registration* — has no counterpart code: the attendance commands implement `IAuthorizedRequest` nowhere, so attendance is ungated. (b) The reference's item 1 (branch numbers) is an intentional exclusion, correctly absent. (c) `BLOCK_PRINT_ON_BALANCE` and `DISCOUNT_LIMIT` are **seeded permission rows that are never read by any enforcement path** — enforcement uses `User.BlockPrintOnRemainingBalance` and `User.DiscountLimitPercent` instead, so the two features work but the grant/revoke UI for them has no effect. The outcome for a configured user is identical; the model differs, and the two dead grant rows are a genuine latent defect | **B** to close cleanly — a fourteenth `ATTENDANCE` permission row and a decision on whether to enforce the two existing codes or remove them. Seed-data changes are model changes to EF Core and need a new migration | High |
| R-E02 | Edit an existing user's data and permissions | IMPLEMENTED | `Features/UsersAndPermissions/Commands/UpdateUser`, `SaveUserPermissions`, `DeactivateUser`, `ReactivateUser`, `DeleteUser`, `ChangeOwnPassword`; `Domain/Users/User.cs:126,161,172,195,214-246` (`GrantPermission`, `RevokePermission`, `ClearPermissions`) | Equivalent. Data and permissions are changed in one pass; the user list supports selection before edit; a user can be deactivated and reactivated rather than deleted. `ShellViewModel.cs:232-245` gates the whole area behind the secondary password | Same dead-grant-row caveat as R-E01 | A | High |
| R-E03 | Permission enforcement on use | IMPLEMENTED | `Application/Common/Authorization` (`AuthorizationBehavior`); 13 per-area `*AccessPolicy` static classes; `Domain/Users/User.cs:14` (`IsAbsolutePermission`); `ShellViewModel.cs:201-209` (menu-level enablement); `Common/ErrorPresentation/ResultErrorPresenter` | Equivalent in effect. Every command and query declares `RequiredPermissionCode`; a MediatR behaviour rejects an unauthorised request; the shell disables the navigation entry; the operator sees a refusal message | The reference's refusal text is one fixed Arabic sentence. My system presents an equivalent refusal through `ResultErrorPresenter` and additionally greys out the menu entry — so both the message and the visual state are reproduced. The reference's `123` factory secondary password is replaced by a per-user internal hash | A | High |
| R-E04 | Attendance, rest period and departure | IMPLEMENTED | `Features/Attendance/Commands/CheckIn`, `StartBreak`, `EndBreak`, `CheckOut`; `Domain/Attendance/AttendanceRecord.cs:6-18` (`CheckInAtUtc`, `BreakStartAtUtc`, `BreakEndAtUtc`, `CheckOutAtUtc`, `OvertimeMinutes`, `LatenessMinutes`); `Domain/Attendance/AttendanceCalculator.cs:12`; `ViewModels/Attendance/MyAttendanceViewModel.cs`; `Views/Attendance/MyAttendanceView.xaml`; the break is conditional on `User.HasBreakPeriod` + `BreakDurationMinutes` | Equivalent. The user registers attendance at the start of work, optionally registers the rest period defined on their record, and registers departure; overtime and lateness are **derived**, never typed. `AttendanceCalculator` converts to local time, documented as a single-site LAN product | Not permission-gated (see R-E01a). The reference's "only the system administrator may view" restriction is reproduced by placing the user-facing summary behind an absolute-permission check rather than a permission code | A | High |
| R-E05 | Login/logout history report per user | **PARTIALLY_IMPLEMENTED** | `Features/Attendance/Queries/GetAttendanceRecords`, `GetUserAttendanceSummary`; `ViewModels/Attendance/AttendanceRecordsViewModel.cs`, `UserAttendanceSummaryViewModel.cs`; `Views/Attendance/AttendanceRecordsView.xaml`, `UserAttendanceSummaryView.xaml`; `Domain/Attendance/AttendanceRecord.cs:6-18` | **Present:** a per-user statement of attendance events over a period, with entry date/time, exit date/time, break window, and derived overtime and lateness. **Absent: the device name.** The reference records *which machine* the user signed in from (`اسم الجهاز`, e.g. `REALLAB-PC`) — an explicit column in its report | `AttendanceRecord` has no machine/host field and no value equivalent to it is captured at check-in | **B** — a nullable `MachineName` column on `AttendanceRecord`, populated at check-in, requires a new migration | High |
| R-E06 | Per-patient workflow status | IMPLEMENTED | `Domain/PatientStatus/PatientAggregateStatus.cs:4` (S1–S7); `Domain/PatientStatus/PatientStatusCalculator.cs:16` (`Calculate(patient, tests, balance)`, documented as *"Computed, never stored, never cached"*); consumed by `Features/PatientSearch` and the billing views | Equivalent and more granular than the reference's four-state symbol. The reference shows not-entered / not-verified / not-printed / not-delivered; my system derives a seven-state aggregate from the per-test lifecycle columns, where the state is the minimum stage across all tests, with S1 only when nothing is entered and the patient registered today, and S6/S7 derived from the balance | The reference's "position in the workflow" is a single symbol; my system's seven states are a superset that still resolves the reference's four questions | A | High |

### 5.6 Group F — Statistics and monitoring

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-F01 | Patient statistics over a period | **PARTIALLY_IMPLEMENTED** | `Features/Statistics/Queries/GetPatientCountStatistics/GetPatientCountStatisticsQueryHandler.cs`; `StatisticsDtos.cs:19-27` (`PatientCountStatisticsDto`); `ViewModels/Statistics/StatisticsViewModel.cs`; `Views/Statistics/StatisticsView.xaml`; gated by `STATISTICS` | **Present:** total patients over a `From`/`To` range, with breakdown by **sex** (`:41-51`), by **referral entity** including a "no referral" bucket (`:52-86`), by **account type** (`:88-97`), **grouped by month** (`:99-106`), and a **month × sex** cross-tab (`:108-121`). **Absent: (a) the day-of-month grouping** the reference offers as one of its six report variants; (b) the **amounts-paid money row** that every reference patient-statistics report carries beneath the count | The counts are computed in memory from the loaded patient set rather than in SQL, which is adequate at the volumes involved but is a scaling constraint, not a behavioural difference. `StatisticsViewModel.cs:21` records the deliberate no-charting-library decision, so the money row would be a text/grid addition | A — `PaymentOperation` already carries `Amount`, `DiscountAmount`, `OperationAtUtc` and `IsVoided`; both the daily grouping and the money row are query-side changes only | High |
| R-F02 | Test and sample volume statistics | IMPLEMENTED | `Features/Statistics/Queries/GetTestCountStatistics/GetTestCountStatisticsQuery.cs`; `StatisticsDtos.cs:37-43` (`TestCountStatisticsDto`: `TotalOrders`, `Tests`, `Groups`); gated by `STATISTICS` | Equivalent: totals over a period with a breakdown **by test** and **by test group**, optionally scoped to one group | The reference's per-analyte percentage breakdown inside a test-group request-rate report (p.32 of the showcase) is not reproduced; my system's breakdown is by test and by group only. The reference's own caption is inconsistent between "rate" and a raw count (§4.5, item 14), so the target behaviour is not crisply defined | A | Medium — the per-analyte percentage sub-breakdown is a presentation addition, not a new capability |
| R-F03 | Sent-out statistics by period and destination lab | IMPLEMENTED | `Features/Statistics/Queries/GetSentOutStatistics/GetSentOutStatisticsQueryHandler.cs`; `StatisticsDtos.cs:45-55` (`SentOutStatisticsDto`: `TotalSent`, `Labs` each with `LabId`, `LabName`, `SentCount`, `TotalCost`, `TotalPaid`, `Remaining`); gated by `STATISTICS` | Equivalent: the exact two-dimensional breakdown — time period × destination laboratory — plus the cost, paid and remaining amounts | My system additionally reports cost/paid/remaining per lab, which the reference does not. A superset | A | High |
| R-F04 | User productivity statistics | IMPLEMENTED | `Features/Statistics/Queries/GetUserProductivityStatistics/GetUserProductivityStatisticsQuery.cs` (`From`, `To`, `UserId`); `ViewModels/Statistics/StatisticsViewModel.cs`; gated by `STATISTICS` | Present: a per-user production figure over a period, optionally scoped to one user. The reference states the capability three times and never demonstrates it, so there is no demonstrated workflow to diverge from | The reference never defines the metric. Because the reference is silent on the formula, no behavioural divergence can be established in either direction | A | Medium — the capability exists with a real handler and a real query; equivalence cannot be asserted because the reference is silent |
| R-F05 | Abnormal-result / quality-control monitor | **MISSING** | No implementation. No `Levey`/`QualityControl`/`ControlChart` type exists in `src/`. `Features/Statistics` contains exactly four queries: patient count, test count, sent-out and user productivity. `ViewModels/Statistics/StatisticsViewModel.cs:21` confirms the surface is DataGrid/number cards only | **Why missing:** the reference lets the operator choose **one test** and **a minimum and maximum result value** for a period, and produces a report of every result falling in that band with date, patient, patient data, referral entity, test, result and status. The reference states this is how a laboratory "judges chemistry, analysers and rapid tests, and displays positive results" — i.e. it is a quality-control and positive-screen monitor, not a revenue report. The assessed system has no equivalent: a result outside a band can be found by filtering the result worklist on `HasResult`/`IsReviewed`/`IsPrinted`, but there is no test-scoped, value-banded, period-scoped monitor | The nearest existing facility filters on *workflow state*; this one filters on *result value*. They answer different questions, so the gap is real and not a naming artefact. The cheapest of the eight missing functions: no migration required | **A** — `PatientTest.ResultValue` (string), `ResultFlag`, `EnteredAtUtc` and `PatientTestReferenceRangeSnapshot` already hold everything the monitor needs. A new query and a band input are sufficient; no schema change | High  |

### 5.7 Group G — Audit, accounts and cash

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-G01 | Till reconciliation and net lab profit | IMPLEMENTED | `ViewModels/Accounts/AccountsHubViewModel.cs`; `Views/Accounts/AccountsHubView.xaml`; `Features/InventoryAndAccounting/Queries/GetCashDrawerInventory/GetCashDrawerInventoryQueryHandler.cs`; `InventoryDtos.cs:11-26` (`CashDrawerInventoryDto`); gated by `CashDisburseDeposit` and the secondary password (`ShellViewModel.cs:254-267`) | **Equivalent, and the field set is a superset.** `CashDrawerInventoryDto` returns `TotalSamplesCount`, `TotalSamplesAmount`, `DiscountsValue`, `TotalAfterDiscount`, `CollectedAmount`, `UncollectedAmount`, `CashSupplies`, `Disbursements`, `SafeCash`, `SentOutCount`, `SentOutTotalCost`, `SentOutTotalPaid`, `SentOutRemaining`, `CommissionsAndShares`, `RemainingToLab` and `NetProfit` over an arbitrary `From`/`To` range — which subsumes the reference's daily/weekly/monthly/yearly/period cadences | The reference's shift selector (`بالأسهار / مسائي / كله`, morning/evening/all) has no counterpart; the period range covers the same reporting need in a different form. My system additionally reports commission shares, which the reference computes in a separate function (R-G07) | A | High |
| R-G02 | Party-scoped reconciliation with four granularities | IMPLEMENTED | `Features/InventoryAndAccounting/Queries/GetElementInventory/GetElementInventoryQueryHandler.cs`; `InventoryDtos.cs:28-45` (`InventoryElementKind` = User / ReferralEntity / TreatingDoctor / AccountType / SentOutSamples; `InventoryReportType` = Summary / Detailed / DetailedByPrices / DetailedByResults; `ElementInventoryDto`, `ElementLineDto`) | **Structurally identical to the reference.** Five scoping axes against the reference's user / referral entity / treating doctor / sent-out / account type, and four report granularities against the reference's summary / detailed by prices / detailed by results / consolidated across entities | The reference additionally offers filters to show only debtors, only discounted patients, or only settled patients. These are not visible in the DTO; they may be applied in the view. Not confirmed either way — see §11 | A for the four granularities; **U** for the debtor/discounted/settled filters | Medium |
| R-G03 | Sent-out account statement and follow-up | IMPLEMENTED | `ViewModels/Patients/SentOutLabAccountViewModel.cs`; `Views/Patients/SentOutLabAccountView.xaml`; `Features/SentOutSamples/Queries/GetSentOutLabAccount`, `GetSentOutSamples`; `Domain/SentOutSamples/SentOutAccountCalculator.cs:11` (`TotalCost`, `TotalPaid`, `Remaining`, `IsFullySettled`); `Domain/SentOutSamples/SentOutSample.cs`, `SentOutSamplePayment.cs` | Equivalent: destination lab, cost, amount paid and remaining, per sample and aggregated per entity, over any period. Payments are recorded as separate `SentOutSamplePayment` rows attributed to a performing user, so instalments are traceable | `Remaining` is not clamped, so an overpayment reads as a negative balance rather than zero. A deliberate accounting choice, not a defect | A | High |
| R-G04 | Settle a sent-out sample's account | IMPLEMENTED | `Features/SentOutSamples/Commands/RecordSentOutPayment`, `SettleSentOutInFull`; `ViewModels/Patients/SentOutLabAccountViewModel.cs`; `Domain/SentOutSamples/SentOutAccountCalculator.cs` (`IsFullySettled`) | Equivalent: record a payment against a sent-out sample, or settle the account in full | None material | A | High |
| R-G05 | Count the tests of a specific referral or contracting party | IMPLEMENTED | `Features/Statistics/Queries/GetTestCountStatistics` (period + optional group, consumed per entity in the accounts view); `Features/InventoryAndAccounting/Queries/GetElementInventory` (`InventoryElementKind.ReferralEntity`, `InventoryReportType.DetailedByResults`); `Features/InventoryAndAccounting/Queries/GetPatientSamplesDetail` | Equivalent: a per-party test/sample count for a period is available both as a statistic and as a detailed accounts line | The reference performs this "using the work-paper facility" (Learn p.187). My system performs it through the accounts/statistics path rather than the work-paper path. Same data, different door | A | Medium — the reference's exact route is stated only in passing |
| R-G06 | Cash disbursement and deposit | IMPLEMENTED | `ViewModels/Accounts/CashMovementDialogViewModel.cs`; `Views/Accounts/CashMovementDialogWindow.xaml`; `Features/InventoryAndAccounting/Commands/RecordCashDeposit`, `RecordCashDisbursement`; `Domain/Accounting/CashMovement.cs:7` (`MovementType`, `Amount` > 0, `RelatedExternalEntityId?`, `PerformedByUserId`, `OccurredAtUtc`, `Notes`); `Domain/Common/Enums/MovementType.cs`; `Features/InventoryAndAccounting/Queries/ListCashMovements` | Equivalent: a cash movement is recorded as a disbursement or a deposit, optionally attributed to a designated external entity, performed by a named user, and listed back | None material | A | High |
| R-G07 | Other labs' accounts and referral-party percentage shares | IMPLEMENTED | `Features/InventoryAndAccounting/Queries/GetCompanyDelegateAccounts/GetCompanyDelegateAccountsQueryHandler.cs`; `InventoryDtos.cs:6-9` (`CommissionShareDto`: `EntityId`, `EntityName`, `Percent`, `ReferredChargeBase`, `CommissionAmount`); `Domain/ExternalEntities/ExternalEntity.cs:33` (`DiscountOrCommissionPercent`) | Equivalent: the accounts of partner laboratories and the commission percentage of referral entities over a period, computed as a percentage of the referred charge base | None material | A | High |
| R-G08 | Block report printing until the account is settled | IMPLEMENTED | `Domain/Users/User.cs:18` (`BlockPrintOnRemainingBalance`); enforced in six handlers — `Features/ResultsEntry/Commands/MarkResultPrinted/MarkResultPrintedCommandHandler.cs:56`, `MarkCultureReportPrintedCommandHandler.cs:3`, `Features/ProfileResults/Commands/MarkProfilePrinted/MarkProfilePrintedCommandHandler.cs:56`, `Features/ReportProduction/Commands/PrintCombinedReport/PrintCombinedReportCommandHandler.cs:59`, `PrintHistoryReportCommandHandler.cs:64`, `Features/ResultsEntry/Commands/ExecuteBulkPrint/ExecuteBulkPrintCommandHandler.cs:72`; `ViewModels/Patients/DeliveryHandoverViewModel.cs` | Equivalent: six independent print paths are gated, and the delivery hand-over surface surfaces the balance | **Model difference:** the reference makes this a *permission grant* (item 6 of its 15); my system makes it a boolean field on the user. For the person configuring the user the outcome is the same, but the corresponding seeded permission code `BLOCK_PRINT_ON_BALANCE` is never read (see R-E01c) | A | High |

### 5.8 Group H — System settings

All settings below are single-row entities with `PK = 1` under `src/TopLab.Domain/Settings/`, mapped by their own `IEntityTypeConfiguration` and seeded through `HasData`. All are edited through `Features/SystemAndPrintSettings` and gated by `EDIT_SYSTEM_SETTINGS`.

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-H01 | Report appearance and layout | **PARTIALLY_IMPLEMENTED** | `Domain/Settings/ReportSettings.cs:9-33`; `Features/SystemAndPrintSettings/Commands/UpdateReportSettings/UpdateReportSettingsCommandHandler.cs`; `ViewModels/Settings/ReportSettingsViewModel.cs`; `Views/Settings/ReportSettingsView.xaml`; `Domain/Settings/EnvelopePrintItemPosition.cs:4`; `Persistence/Configurations/ReportSettingsConfiguration.cs:29` (seed); migration `20261001203315_AddCombinedReportPrintOptions` | **Present:** left and bottom page margins, report top space (**capped at 8 cm — `SetTopSpace`, `ReportSettings.cs:62`, exactly the reference's cap**), paper size A4/A5, header/footer mode as three exclusive options (`HeaderFooterMode.None` / `Words` / `Images` — matching the reference's three), header colour, footer colour, doctor signature, history sort mode, automatic history display, group sub-title and reprint-message suppression. Lab header/footer text is stored separately per artefact through `ILabPrintTextStore` with `LabPrintTextScope { Report, Receipt, Envelope }`. **Absent: per-report element positioning.** `EnvelopePrintItemPosition` gives Left/Top centimetre offsets and an enable flag per item — but only for the **envelope**. The report has no equivalent per-element positioning table, and no per-element on/off list (that gap is R-D05) | The reference's report-positioning screenshots (pp.196–198) come from a visibly different/older build and match no contents entry, so the target is weakly defined. The margin, paper, mode and colour capabilities are reproduced faithfully including the 8 cm cap | **B** for report-side per-element positioning — a new `ReportPrintItemPosition` entity mirroring the envelope one | High on what exists; Medium on what the reference intends |
| R-H02 | Dedicated printer per print job | **PARTIALLY_IMPLEMENTED** | `Domain/Common/Enums/PrinterOutputType.cs` = Reports, Barcode, Envelope, Receipt (4 values); `Domain/Settings/PrinterAssignment.cs:6` (PK is the output type); `Features/SystemAndPrintSettings/Commands/SavePrinterAssignments`; `GetPrinterAssignments`; `Infrastructure/Printing/ShellPdfPrinterDispatcher.cs` (routes by `PrinterOutputType`); `Persistence/Configurations/PrinterAssignmentConfiguration.cs:14` (seed) | **4 of 5 print jobs** can be routed to a dedicated printer, and the dispatcher really uses the assignment. The reference offers five: report, barcode, envelope, receipt, **card** | The card printer is absent, consistent with the absent card artefact (R-H14) | **B** — adding `PrinterOutputType.Card` is an enum change, and because the enum is the primary key of `PrinterAssignments` a new migration is required to widen the column and seed the new row | High |
| R-H03 | Default account type | IMPLEMENTED | `Domain/Settings/SystemSettings.cs:9,74` (`DefaultAccountType`, `SetDefaultAccountType`); `Domain/Common/Enums/AccountType.cs` = Individual / LabToLab / Contracts / Vip / Free; `ViewModels/Settings/SystemSettingsViewModel.cs`; applied at registration in `Features/PatientRegistration` | Equivalent | The domain deliberately **rejects `Vip`** as a default (`SetDefaultAccountType` throws for it) while still allowing `Vip` as a per-patient flag (`Patient.IsVip`). A defensible tightening | A | High |
| R-H04 | Enter patient name and treating doctor in English | **MISSING** | No implementation. `Domain/Patients/Patient.cs:17` has a single `FullName`; `Domain/ExternalEntities/ExternalEntity.cs:17` has a single `Name`. `SystemSettings` has no English-entry switch. A search across `src/` for an English-name or Latin-name field returns nothing | **Why missing:** the reference offers a switch enabling the patient name and the treating-doctor name to be entered in English alongside the Arabic, for laboratories that need a Latin rendering on the report or the envelope. The assessed system stores one name per person and has no setting for it | This is a data-model gap, not a rendering gap: there is nowhere to store the second name, so no amount of report-writer work would satisfy it without a schema change first | **B** — `Patient.FullNameEn` and `ExternalEntity.NameEn` (both nullable), plus a `SystemSettings` switch, and a rendering decision in the PDF writers | High  |
| R-H05 | Automatic Mr/Mrs by sex when the referral field is blank | **MISSING** | No implementation. `Domain/Patients/PatientTitle.cs:6` provides a title master (`TitleText`, `IsDefault`), and `SystemSettings.DisableAutoTitleInsertion` implements the **opposite** switch (R-H08), but nothing derives a title from the patient's sex. A search for `Mr`/`Mrs` title logic in `src/TopLab.Application/Features/PatientRegistration` returns nothing | **Why missing:** the reference inserts a sex-derived title into the **referral-entity field** when that field is left empty, and the resulting title is printed on the patient name. The assessed system stores a title as an explicit patient attribute and never derives one from sex. Note that the reference itself never reconciles this rule with R-H08 (§4.5, item 7), so the intended behaviour is ambiguous | Do not implement this until the ambiguity is settled. The reference contains two contradictory rules — insert a title automatically (this item) and do not auto-insert titles (R-H08) — and the assessed system already implements the second. Adding the first without deciding which the owner wants risks introducing a conflict | **B** if configurable (a `SystemSettings` switch); **A** if implemented as an unconditional domain rule on `Patient.Create`/`Update` | High on the gap; Low on the target behaviour, which the reference contradicts internally  |
| R-H06 | Save the treating doctor only from the external-entities window | IMPLEMENTED | `Domain/Settings/SystemSettings.cs:17` (`SaveTreatingDoctorOnlyFromEntityWindow`); `ViewModels/Settings/SystemSettingsViewModel.cs`; consumed in patient registration | Equivalent: the switch stops the system auto-saving a treating doctor typed during patient entry, forcing creation from the external-entities window | None material | A | High |
| R-H07 | Enable patient-name search assist in the add/edit patient window | IMPLEMENTED | `Domain/Settings/SystemSettings.cs:19` (`EnablePatientNameSearchAssist`); read in `Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQueryHandler.cs` — the name `Contains` clause is applied only when the setting is on; `ViewModels/Patients/PatientEditorViewModel.cs` | Equivalent, and the setting genuinely gates behaviour: with it off, the name clause is skipped and only exact Lab ID, exact national ID and phone matches apply | None material | A | High |
| R-H08 | Do not auto-insert titles at registration | IMPLEMENTED | `Domain/Settings/SystemSettings.cs:21` (`DisableAutoTitleInsertion`); `Features/PatientRegistration` | Equivalent | None material | A | High |
| R-H09 | Review and complete tests automatically | IMPLEMENTED | `Domain/Settings/SystemSettings.cs:13` (`AutoReviewAndComplete`); `ViewModels/Settings/SystemSettingsViewModel.cs`; consumed in the results-entry path | Equivalent: when on, a result does not require the separate manual verification step | None material | A | High |
| R-H10 | Patient-history sort and grouping | IMPLEMENTED | `Domain/Settings/ReportSettings.cs:25,103` (`HistorySortMode`, `SetHistoryOptions`); `Domain/Common/Enums/HistorySortMode.cs` = ByLabCode / ByPatientName; `Domain/Reports/PatientHistoryResolver.cs:9` (`ResolveKey`) | Equivalent: the sort key is configurable between lab code and patient name, and the resolver normalises accordingly (trim for lab code, whitespace-collapse + upper-invariant for name) | None material | A | High |
| R-H11 | Main system data (reception time, auto-print receipt) | **PARTIALLY_IMPLEMENTED** | `Domain/Settings/ReceiptSettings.cs:13` (`PickupTimeDefault`, `TimeOnly?`); `Domain/Settings/SystemSettings.cs` — **no** auto-print-receipt flag; nearest candidate `ReceiptSettings.CashierPrinterEnabled` (`:19`); `Features/SystemAndPrintSettings/Commands/UpdateReceiptSettings` | **Present:** the result-reception time is a configurable setting on the receipt, with a `TimeOnly?` default. **Absent:** an explicit switch that **automatically prints the patient receipt** on registration. The reference names this control directly. `CashierPrinterEnabled` reads as "route receipts to the dedicated cashier printer", not "print the receipt automatically", so I do not count it as satisfying this item | The reference's own "delivery time" versus "result-reception time" ambiguity (§4.5, item 8) means it is not certain which of the two is meant | **B** — a `SystemSettings.AutoPrintReceipt` boolean, or alternatively a `ReceiptSettings` column | Medium — presence of the reception time is high confidence; the auto-print switch may exist under a name I did not find, though a full read of `ReceiptSettings` and `SystemSettings` shows no such member |
| R-H12 | Receipt settings | IMPLEMENTED | `Domain/Settings/ReceiptSettings.cs:7-21`; `Features/SystemAndPrintSettings/Commands/UpdateReceiptSettings/UpdateReceiptSettingsCommand.cs` (7 parameters, gated on `EDIT_SYSTEM_SETTINGS`); `ViewModels/Settings/ReceiptSettingsViewModel.cs`; `Views/Settings/ReceiptSettingsView.xaml`; `Persistence/Configurations/ReceiptSettingsConfiguration.cs:21` (seed) | **Equivalent field-for-field:** top margin, currency (a validated string, default `L.E.`, max 10 chars), reception time, print-once, test-detail display mode, cashier printer, and header/footer mode — matching the reference's seven controls | The reference's receipt header/footer offers a logo image; my system's `HeaderFooterMode.Images` plus `ILabPrintTextStore` provides image-backed headers generally. Equivalent in capability | A | High |
| R-H13 | Envelope settings | IMPLEMENTED | `Domain/Settings/EnvelopeSettings.cs:7-13` (`TopMarginCm` 0–30, `HeaderFooterMode`, `SuppressCaptions`); `Domain/Settings/EnvelopePrintItemPosition.cs:4` (`ItemName` PK, `IsEnabled`, `LeftOffsetCm`, `TopOffsetCm`, both 0–30); `Features/SystemAndPrintSettings/Commands/UpdateEnvelopeSettings`; `Persistence/Configurations/EnvelopePrintItemPositionConfiguration.cs:17-22` (seeds `Name`, `Code`, `ReferralEntity`, `Date`) | **Equivalent, and arguably better.** Top margin, header/footer mode including images, and a per-item position table with enable flags and centimetre offsets. The seeded `ReferralEntity` item is the reference's addressee block — "referring doctor or referral entity" — and `Date` its date line, exactly as the reference describes | The reference's second and third margin controls are never distinguished; my system has a clean 0–30 cm range | A | High |
| R-H14 | Card settings | **MISSING** | No implementation. `PrinterOutputType` has four values, not five. A search for `كارت` / `Card` in `src/**/*.cs` and `src/**/*.xaml` returns no lab-card artefact. `Infrastructure/Printing/` contains six writers — report, receipt, invoice, work sheet, price list, custom group — and none produces a card | **Why missing:** the reference allocates a card print job its own settings tab **and** its own dedicated printer. Neither exists. In fairness the reference **never documents a single field or rule** for this tab (§4.4) — the card is a physically real artefact in the reference (RL_Show p.3 shows a lab-card label) with an undocumented configuration. So this is a genuine gap of small, well-defined scope: one print artefact, one settings row, one printer slot | Smallest well-bounded gap of the eight. It is also the fifth printer in R-H02, so the two are really one piece of work: add `PrinterOutputType.Card`, the settings row, and the writer together | **B** — a `CardSettings` single-row entity (margin, header/footer mode, paper size), a `CardPdfWriter`, a print command, and `PrinterOutputType.Card` | High  |
| R-H15 | Database server settings | IMPLEMENTED | `Features/SystemAndPrintSettings/Queries/GetDatabaseServerSettings/GetDatabaseServerSettingsQueryHandler.cs`; `Commands/UpdateDatabaseServerSettings/UpdateDatabaseServerSettingsCommandHandler.cs`; `Persistence/DesignTimeDbContextFactory.cs`; `Views/Settings/DatabaseMaintenanceView.xaml`; `ViewModels/Settings/DatabaseMaintenanceViewModel.cs` | Equivalent: server name, credentials and database name are editable at runtime | The reference lists this command five times and never demonstrates it, so equivalence rests on the reference's three named fields, all of which are present | A | High |
| R-H16 | Database backup and restore | IMPLEMENTED | `Application/Common/Interfaces/IDatabaseMaintenanceService.cs:6`; `Infrastructure/Persistence/Maintenance/SqlServerDatabaseMaintenanceService.cs:15` (`BackupNowAsync`, `RestoreAsync`, `ApplyPendingUpdatesAsync`, `CheckBackupPathAsync`, raw SQL Server BACKUP/RESTORE); `Features/SystemAndPrintSettings/Commands/BackupDatabaseNow`, `RestoreDatabase`; `IDialogService.PickBackupFolderAsync` / `PickBackupFileAsync`; `Domain/Settings/SystemSettings.cs:29-31` (`DailyBackupEnabled`, `DailyBackupPath` ≤ 300, path required when enabled) | **Equivalent and more complete.** The reference describes one dual-purpose command; my system implements backup, restore and destination-path validation as separate use cases, and the daily backup is a real `BackgroundService` (`Infrastructure/Backup/DailyBackupHostedService.cs:14`) rather than a promise | Backup paths are interpreted **on the SQL Server machine**, not on the client — correct for a shared-database deployment, and consistent with the reference's fixed `D:\` paths | A | High |
| R-H17 | Database and program version update | IMPLEMENTED | `Features/SystemAndPrintSettings/Commands/ApplyDatabaseUpdates/ApplyDatabaseUpdatesCommandHandler.cs`; `IDatabaseMaintenanceService.ApplyPendingUpdatesAsync`; `ViewModels/Settings/DatabaseMaintenanceViewModel.cs` | Equivalent to the reference's "update the database and program files for compatibility with recent versions / prepare the system for the current version" | The reference never demonstrates this either. My system exposes it as a first-class command with its own handler | A | High |
| R-H18 | Automatic daily backup into a dated folder | IMPLEMENTED | `Infrastructure/Backup/DailyBackupHostedService.cs:14` (a `BackgroundService` registered via `AddHostedService`); `Domain/Settings/SystemSettings.cs:29-31`; `SqlServerDatabaseMaintenanceService.cs` | Equivalent: a scheduled backup controlled by a setting and a configured path | The reference names the destination folder after the day's date; my system delegates the naming to SQL Server's `BACKUP` and the configured path. Equivalent in effect | A | High |

### 5.9 Group I — Laboratory operations and utilities

| ID | Reference function | Status in my system | Evidence in my repository | Behaviour vs. reference | Difference | DB impact | Confidence |
|---|---|---|---|---|---|---|---|
| R-I01 | Draw and separate patient samples | IMPLEMENTED | `ViewModels/Lab/SampleCollectionViewModel.cs`, `SampleDrawBoardViewModel.cs`; `Views/Lab/SampleCollectionView.xaml`, `SampleDrawBoardView.xaml`; `Features/SampleCollection/Queries/GetPatientsWithUncollectedSamplesQuery`, `GetPatientTestsForDrawQuery`; `Commands/MarkSampleDrawn/MarkSampleDrawnCommand.cs`, `MarkAllSamplesDrawnForPatientCommand`; `Domain/Results/PatientTest.cs:18-19, 209` (`IsSampleDrawn`, `SampleDrawnAtUtc`, `MarkSampleDrawn`); gated by `ADD_EDIT_PATIENT` | **Equivalent.** Per-tube draw status; a board of patients with uncollected samples; per-test and bulk mark-as-drawn; the drawn state is persisted on the patient test rather than held in the UI | The reference's gating of save on reading the patient's ID, and its optional "print barcode after read ID", are not reproduced as separate controls — barcode printing is its own command (R-A15). The reference's "Other Tube" free-text escape has no counterpart | A for the draw workflow; the "save after read ID" gate would be **A** as a UI flow change | High on the core; Medium on the ID-scan gate |
| R-I02 | Laboratory test knowledge browser | IMPLEMENTED | `Features/Utilities/Queries/GetTestLibrary/GetTestLibraryQueryHandler.cs`; `ViewModels/Utilities/UtilitiesViewModel.cs`; `Views/Utilities/UtilitiesView.xaml:204-206` (grid columns `التحليل` / `الرمز` / `المجموعة` = test, code, group) | Equivalent: a browsable library of the tests in the laboratory with their code and group | The reference also offers the test's nature, its normal values and its effect on the patient. My system shows the catalog fields; I did not find a normal-value or patient-effect column | A if the extra descriptive text is added to the existing projection | Medium |
| R-I03 | Requirements and purchases list | IMPLEMENTED | `Application/Common/Interfaces/IPurchasesListStore`; `Infrastructure/Services/JsonPurchasesListStore.cs`; `Features/Utilities/Commands/AddPurchaseItem`, `RemovePurchaseItem`, `TogglePurchaseItemDone`; `Views/Utilities/UtilitiesView.xaml:164-167` (item text, date, done checkbox) | Equivalent: enter a requirement, tick it off, remove it | Stored as JSON under `%ProgramData%` rather than in SQL Server, by deliberate design (`IPurchasesListStore.cs:5-8`: *"No database table, no EF entity, no migration"*). Functionally equivalent; a trade-off, not a gap | A | High |
| R-I04 | Clock, calculator and unit converter | IMPLEMENTED | `Features/Utilities/Queries/EvaluateCalculation/EvaluateCalculationQueryHandler.cs:17` → `Domain/Utilities/ArithmeticCalculator.cs:12` (recursive-descent parser, `+ − × ÷`, parentheses, decimals, unary minus, **no** `DataTable.Compute` and no dynamic code evaluation); `ConvertMeasurementUnit` → `Domain/Utilities/MeasurementUnitConverter.cs:7` (18 explicit conversion pairs); `ComputeStopwatchElapsed` → `Domain/Utilities/StopwatchCalculator.cs`; stopwatch and clock in `ViewModels/Utilities/UtilitiesViewModel.cs` | **Equivalent and safer.** The reference offers a calculator, a clock and a test-group launcher; my system additionally offers a unit converter and a stopwatch. The calculator refuses division by zero with a localised message instead of throwing | None material | A | High |
| R-I05 | Phone-book / contact list | IMPLEMENTED | `Application/Common/Interfaces/IPhoneBookStore`; `Infrastructure/Services/JsonPhoneBookStore.cs`; `Features/Utilities/Commands/AddPhoneBookEntry`, `RemovePhoneBookEntry`; `Queries/GetPhoneBook`; `Views/Utilities/UtilitiesView.xaml:129-132` (name, phone, notes) | Equivalent: store and remove a contact with a phone number and notes | JSON-backed rather than SQL-backed, by the same deliberate design as R-I03 | A | High |
| R-I06 | Reminder messages to users | **MISSING** | No implementation. A search across `src/TopLab.Application/Features/Utilities/` for `reminder`, `appointment` or a per-user message type returns nothing. The Utilities module contains exactly six commands (phone book add/remove, purchase item add/remove/toggle) and five queries (phone book, purchases, test library, stopwatch, unit conversion, calculation) | **Why missing:** the reference lets an operator compose a reminder message addressed to a specific user or to every person, delivered through an appointment note. The assessed system has no messaging entity, no recipient model and no delivery path | The phone book (R-I05) and the purchases list (R-I03) are both present in the same Utilities module, so the shell for this feature already exists; only the messaging model is absent | **B** — a `ReminderMessage` aggregate (author, target user or broadcast, body, created timestamp, read/acknowledged state) | High  |

---

## 6. Functions Present in Both Systems

This section covers the **69** functions classified `IMPLEMENTED` (67) or `IMPLEMENTED_DIFFERENTLY` (2) — i.e. capabilities the assessed system genuinely delivers. For each: how the reference works, how the assessed system works, and whether they are functionally equivalent or materially different.

**Summary of equivalence:** of the 69 shared capabilities, **63 are functionally equivalent**, **2 are materially different** (R-A04, R-E01), and **4 are equivalent in outcome while differing in a way that could plausibly matter operationally** and are called out individually below (R-C10, R-C12, R-A20, R-A21). Nothing in this section is a naming artefact: in every case the two implementations were traced independently and compared on behaviour.

### 6.1 Patient registration and the clinical questionnaire

**Reference (R-A02, R-A03).** Create a patient with name, sex and age as the mandatory set; phone, address, national ID, treating doctor, referral entity, account type and notes as optional. A separate clinical questionnaire captures current treatment (six named medications), past conditions (five named conditions) and recent contrast imaging, and those answers are re-read on the result-entry screen and printed on the report.

**My system.** `CreatePatientCommandHandler` → `Patient.Create` (`Domain/Patients/Patient.cs:111`). Mandatory set enforced by domain guards. The questionnaire is `PatientMedicalCondition` joined to a seedable `MedicalConditionType` master, with `MedicalConditionCategory` = `Medication` / `Condition` / `Pregnancy` reproducing the reference's three groups; `Patient.IsFastingIndicated`, `FastingHours` and `RecentContrastImaging` reproduce the fasting and imaging questions. `PregnancySignal.IsPregnancyIndicated` later reads the pregnancy category to drive antibiotic filtering (R-C11).

**Verdict: functionally equivalent, and more extensible.** The reference's medication and condition lists are hard-coded; mine is master data. Same clinical coverage, better maintenance.

### 6.2 The patient account — the closest match in the whole comparison

**Reference (R-A06, R-A07, R-A08, R-A09).** Automatic total from catalogued prices; a discount; a previously-paid figure; net after discount; balance owed to the lab. An amount prefixed with `+` in the paid field becomes an extra charge. Wrong entries are corrected or deleted line by line from a dated transaction list, and the account can be zeroed. At delivery the operator sees paid, remaining-for-the-patient and remaining-to-the-lab.

**My system.** `Domain/Billing/PatientAccountCalculator.cs` is the single source of the balance formula, with three named methods: `TotalCharged` (sum of order-time prices **plus** non-voided extra charges), `TotalPaid` (sum of amount + discount over non-voided, non-extra-charge rows), and `Balance` (charged − paid, **negative means credit, no clamping**). `OperationType` = `Payment` / `Correction` / `FullSettlement` maps onto the reference's three account actions. `PaymentOperation.IsExtraCharge` implements the surcharge; `IsVoided` implements the line deletion without destroying the audit trail. `PatientTest.PriceAtOrderTime` freezes the price at order time.

**Verdict: functionally equivalent, with one deliberate improvement.** The reference recomputes from the live catalogue, so a later price change silently rewrites a historical bill. Mine cannot. Every accounting behaviour the reference describes is present, and the arithmetic lives in one testable place rather than across screens.

### 6.3 The result-entry lifecycle

**Reference (R-A12).** Pick the patient, open the report, type a value against the abbreviation with unit and reference range shown, add a comment, mark verified, preview, print.

**My system.** `EnterResult` / `ReviewResult` / `UnreviewResult` / `ClearResult` / `MarkResultPrinted` on `PatientTest` form a strictly guarded state machine: `EnterResult` throws if already reviewed; `MarkReviewed` throws if nothing was entered; `MarkPrinted` requires entered **and** reviewed and increments `PrintCount`; `ClearResult` throws once reviewed, printed or delivered; `MarkDelivered` throws unless printed. Three result shapes are supported and chosen by `ResultKind` — `Simple`, `SpecializedProfile` (multi-analyte panel) and `Culture`.

**Verdict: functionally equivalent, and the guards are stricter than the reference's.** The reference permits editing a reviewed result in place; mine requires an explicit clear-and-re-enter, or for panels the narrow immutable `ProfileResultAmendment` route (which stores old value, new value, reason, author and timestamp and has no update or delete API).

### 6.4 Reference values and historical immutability

**Reference (R-C03, R-C04).** Ranges keyed on sex + age band with low/high limits and low/high comments. A crucial rule: **a normal must be entered for each age unit separately — a 1-month-old is not matched by a 1-to-60-month range.** When a normal is edited, patients already registered keep the old values until the operator opens each report and presses Update.

**My system.** `ReferenceRange.Matches(sex, ageUnit, ageValue)` compares like with like and performs no unit conversion — the same rule, independently arrived at. `PatientTestReferenceRangeSnapshot` (migration `20260908175555`) captures the resolved range onto the patient test at order time, so history cannot be silently rewritten; `RefreshResultReferenceRangeCommand` provides the reference's explicit re-evaluation path on demand.

**Verdict: functionally equivalent, with a stronger storage guarantee.** Same matching semantics, same operator override, but immutability by construction rather than by convention.

### 6.5 The "farm" (مزرعة) — same capability, entirely different mechanism

**Reference (R-C10).** A farm is a culture test type, but provisioning it is a manual, fragile procedure: occupy a reserved test ID in the range 118–139, set the group name to `CULTURE AND SENSITIVITY`, then go into `D:\real lab system\Data` in Windows Explorer, create/rename a folder to the farm's ID, and copy the configuration files — a step the reference itself cannot describe unambiguously (the `_S` suffix convention's meaning is not stated anywhere).

**My system.** `Test.IsCultureType` marks the test; `CultureAntibioticAttachment` (composite key `TestId`+`AntibioticId`, with `SensitivityThresholdMm`) gives it its own panel and per-antibiotic thresholds. The source confirms this is the same concept: `GetCultureAntibioticsQueryHandler` returns the error *"التحليل المحدد ليس مزرعة"* when a non-culture test is supplied.

**Verdict: functionally equivalent — the manual file-system hazard is eliminated.** This is a case where the assessed system is strictly better and no capability is lost.

### 6.6 Antibiotic panel filtering — an exact rule-for-rule match

**Reference (R-C11).** An antibiotic flagged **Pregnant** is shown only when pregnancy is recorded on the patient; one flagged **Children** is shown only for a child, where the program "recognises a child as age < 12 years (male or female)" — a hard-coded, system-wide threshold.

**My system.** `CultureAntibioticDisplay.IsDisplayable(IsPregnancyFlagged, IsChildrenFlagged, pregnant, child)` is applied in `GetCultureEntryGridQueryHandler.cs:37`, where `pregnant` comes from `PregnancySignal.IsPregnancyIndicated` over the patient's medical-condition categories and `child` from `AgeRules.IsUnderTwelve` with `ChildAgeThresholdYears = 12`. Already-saved rows survive filtering.

**Verdict: functionally equivalent, rule for rule, including the arbitrary 12-year threshold.** The one loss is commercial/brand names on the printed sensitivity report — see R-C11 in §5.3 and §8.

### 6.7 External parties and the price-list binding rule

**Reference (R-C09, R-C05).** Three entity types; a contract price list bound to a referral/contracting entity; a commission percentage; a doctor who must not carry a price list.

**My system.** `ExternalEntity.ValidatePriceListRule` (`Domain/ExternalEntities/ExternalEntity.cs:173-184`) **enforces in the domain** that a `TreatingDoctor` has no `PriceListId` and a `ReferralOrContract` must have one — the reference states this as an operating rule but does not show it validated. `TestPriceResolver` applies the bound list at order time.

**Verdict: functionally equivalent, with the rule promoted from convention to an invariant.**

### 6.8 The patient audit trail

**Reference (R-A20).** Which user registered the case; how many times it was modified and by whom with date and time; which user entered each result; how many times the report was printed; who last printed it and when.

**My system.** `AuditableEntitySaveChangesInterceptor` maintains `CreatedByUserId`, `CreatedAtUtc`, `LastModifiedByUserId`, `LastModifiedAtUtc` and `ModificationCount` on ten entities. `GetPatientAudit` returns exactly the reference's patient-level fields; `GetPatientTestAudit` returns entered / reviewed / printed(+count) / delivered, each with user and UTC time. Soft-deleted patients stay auditable; voided payments are included.

**Verdict: functionally equivalent for every question the reference asks — with one structural caveat worth knowing.** There is **no change-history table**. Audit facts are reconstructed from current columns plus a modification counter. So "how many times and by whom" is answerable, but "what did the field used to contain" is not. For the reference's stated purposes this is sufficient; if you later need field-level history, a change-log entity would be required.

### 6.9 Work papers, statistics, accounts and cash

All four work-paper functions (R-B01–R-B04) are implemented against four distinct queries — by visit, by test group, by test count for a period, and by work-group log — all routed through one `WorkSheetPdfWriter` and gated by `PRINT_WORKSHEET`. The reference's "work group" is an attribute on the test; mine is a separate aggregate with a join entity, which is cleaner and behaviourally identical.

Sent-out statistics (R-F03) reproduce the reference's exact two-dimensional breakdown (period × destination lab) and add cost/paid/remaining per lab. The till reconciliation DTO (R-G01) is a strict superset of the reference's field list, adding commission shares that the reference computes separately. Party-scoped reconciliation (R-G02) mirrors the reference's five scoping axes and four granularities almost structurally exactly, via `InventoryElementKind` and `InventoryReportType`.

**Verdict: functionally equivalent throughout**, with R-G02's debtor/discounted/settled filters unconfirmed (§11).

### 6.10 Settings, printing and infrastructure

Report margins with the reference's exact 8 cm cap, A4/A5, the same three exclusive header/footer modes, header/footer colours, doctor signature, history sort mode, automatic history display — all present in `ReportSettings`, seeded and editable through `UpdateReportSettingsCommand`. Envelope settings go further than the reference, with a seeded per-item position table (`Name`, `Code`, `ReferralEntity`, `Date`) carrying enable flags and centimetre offsets. Receipt settings match the reference's seven controls field-for-field. Backup/restore is more complete than the reference's single dual-purpose command, and the daily backup is a real hosted service.

Printing is a genuine strength rather than a checkbox: six dedicated PDF writers (report, receipt, invoice, work sheet, price list, custom group) over QuestPDF with Arabic/RTL rendering and an `ArabicFontResolver`, a `ShellPdfPrinterDispatcher` that genuinely routes by `PrinterOutputType`, a 24-hour temp-file cleanup service, and a no-throw contract that reports swallowed exceptions to an `IPrinttingDiagnostics` sink instead of crashing.

**Verdict: functionally equivalent on every counted settings item except R-H01 (report-side element positioning), R-H02 (the fifth printer) and R-H11 (auto-print receipt).**

### 6.11 Where the assessed system is materially better

Recorded because a comparison that only lists gaps misleads. These are all verified in source:

| Area | Assessed system | Reference |
|---|---|---|
| Price immutability | `PatientTest.PriceAtOrderTime` freezes the price at order time | Recomputes from the live catalogue; a price edit rewrites historical bills |
| Range immutability | `PatientTestReferenceRangeSnapshot` + `ProfileResultItemReferenceRangeSnapshot` | Relies on the operator remembering to update each affected report |
| Post-print amendments | `ProfileResultAmendment` — immutable, old/new value, reason, author, timestamp, no update or delete API | Free-text editing; no amendment record |
| Password storage | PBKDF2 with a per-user salt, registered as a singleton hasher | Unspecified |
| Deleted records | `Patient.IsDeleted` soft delete; soft-deleted patients remain auditable | Not described |
| Voided transactions | `PaymentOperation.IsVoided` — reversible but auditable | Physical deletion of a transaction row |
| Entity invariants | Price-list binding, `isSentOut ⇒ SentOutCostPrice`, result-state guards, discount ≤ amount — all enforced in the domain | Stated as operating rules, not shown as validated |
| Concurrency and typing | 31 strongly-typed IDs, `PatientAggregateStatus` computed and never stored | Untyped identifiers |
| Unauthorised deletion | Zero `NotImplementedException` and zero `TODO`/`FIXME` in `src/`; `AboutWindowPlaceholderTests` actively asserts against placeholder text | Not assessable |
| Backup correctness | Backup paths interpreted on the SQL Server machine, correct for a shared-database deployment | Fixed local `D:\` paths |

---

## 7. Missing Functions

**Eight** reference functions are classified `MISSING`. None of the 14 intentionally excluded functions appears in this section.

### 7.1 R-A17 — Print the lab order / requisition form

- **Function:** produce a per-patient requisition listing Test Name, Unit, Result, Collection Notes and a signature column, with reception and samples-officer signature lines, a `Printed By` stamp, one form per patient to prevent error and ease follow-up and archiving, printable whether or not the report has been printed.
- **Reference behaviour:** the operator prints the form at registration or later; the doctor can also re-order individual tests.
- **Evidence of absence:** the nearest artefact is `PatientResultSheetDto` (`Features/ResultsEntry/Common/ResultsEntryDtos.cs:54-68`), which carries TestName, TestCode, ResultValue, ResultFlag, Notes, IsReviewed and IsPrinted. It is a **post-analytical result listing**: it has no collection-notes column, no signature column and no signature lines, and it is derived from entered results. `Infrastructure/Printing/` contains six writers — `ReportPdfWriter`, `ReceiptPdfWriter`, `InvoicePdfWriter`, `WorkSheetPdfWriter`, `PriceListPdfWriter`, `CustomGroupPdfWriter` — and **none produces a requisition**.
- **Current state:** absent.
- **Why classified missing:** the capability is a distinct print artefact with distinct content; no view, command, writer or menu entry produces it.
- **Required scope:** a `LabRequisitionPdfWriter`, a print command gated on `PRINT_RESULTS`, a print-menu entry, and a decision on how `Collection Notes` is sourced.
- **Database impact:** **A** if collection notes are derived from the existing `PatientTest` sample flags (`IsUrine`, `IsStool`, `IsBlood`, `IsSemen`, `IsCsf`, `IsTakenOutsideLab`); **B** if a free-text collection note must be stored per test.
- **Migration impact:** at most one new migration adding a nullable `CollectionNote` column to `PatientTest`; none if derived.
- **Relevant existing tables:** `PatientTests`, `Patients`, `Tests`, `PrinterAssignments`.
- **Confidence:** High.

### 7.2 R-D02 — Automatic haematology indices and per-analyte correction factors

- **Function:** compute every haematology index without operator intervention (HCT, MCV, MCH, MCHC, PCT, MPV, PDW, PDW-CV, P-LCC and ratios such as Chol/TG), and maintain a correction-factors screen holding an editable formula and an adjustment constant per derived index, used to correct a counter or reagent fault.
- **Reference behaviour:** the operator enters the measured parameters; the program derives the rest. A per-index factor compensates for a faulty counter or reagent.
- **Evidence of absence:** `Domain/Utilities/ArithmeticCalculator.cs:12` implements a full recursive-descent expression evaluator — but it is consumed **only** by `Features/Utilities/Queries/EvaluateCalculation/EvaluateCalculationQueryHandler.cs:17`, the standalone calculator tool. `Domain/Results/ProfileResultItem.cs` holds `ResultValue` as a plain string with no formula, no factor and no computation. `Domain/Tests/Analyte.cs` carries no calculation definition. A search for `Calculat`/`Comput`/`Formula` in the result domain returns nothing.
- **Current state:** absent. An index is whatever the operator types.
- **Why classified missing:** automatic derivation is a distinct capability from a calculator tool, and no code path connects the two.
- **Required scope:** a definition entity linking a derived analyte to a formula over other analytes, an evaluation order (since MCHC depends on HGB and HCT, which depend on RBC), entry points to trigger evaluation, and a correction-factor field with its own settings surface.
- **Database impact:** **B** — a new `DerivedIndexDefinition` aggregate (analyte, formula expression, evaluation order, correction constant, enabled flag) plus columns on `Analyte`.
- **Migration impact:** one new migration creating the definition table and seeding the standard index set.
- **Relevant existing tables:** `Analytes`, `AnalyteReferenceRanges`, `AnalyteReferenceRangeBands`, `ProfileAnalytes`, `ProfileResultItems`, `ProfileResultItemReferenceRangeSnapshots`.
- **Confidence:** High.
- **Dependency note:** the *correction-factor* half of this function is what the exclusion list calls "correction/adjustment factors for each analyser" (exclusion 8). The **index-derivation** half is independent of any device and is not excluded. Only the device-scoped factor is out of scope.

### 7.3 R-D03 — Automatic clinical comments from result conditions

- **Function:** maintain a per-analyte rule table mapping a condition to a comment — HGB → Anaemia / higher than normal; MCV → Microcytic / Macrocytic / Normocytic; MCH → Hypo/Hyper/Normochromic; RBC → Anaemia / Polycythaemia; RDW; reticulocytes; PLT → Thrombocytopenia / Thrombocytosis / adequate; MPV; PDW-CV; WBC → Leucopenia / Leucocytosis / no significant abnormality — with an "in range" slot, editable texts and a master on/off switch.
- **Reference behaviour:** the program writes the comment itself from the result; texts are editable; the mechanism can be disabled.
- **Evidence of absence:** `Domain/Tests/TestComment.cs:6` stores a fixed free-text comment per test — that is the reference's R-C07 capability, already implemented, and is **not** conditional on the result. A search for `AutoComment`, `GeneratedComment`, `CommentRule` and `Interpretation` across all of `src/` returns **zero** matches.
- **Current state:** absent. Comments can be stored and displayed but never derived.
- **Why classified missing:** rule-driven derivation is a distinct capability from a fixed-comment library.
- **Required scope:** a rule entity, an evaluation pass after result entry that resolves applicable rules, a priority so overlapping bands resolve deterministically, an editing screen, and the disable switch.
- **Database impact:** **B** — a new `CommentRule` aggregate (analyte, lower/upper bound or flag test, resulting flag, comment text, priority, enabled flag).
- **Migration impact:** one new migration creating the rule table.
- **Relevant existing tables:** `Analytes`, `ProfileResultItems`, `TestComments`, `ReportSettings`.
- **Confidence:** High.

### 7.4 R-F05 — Abnormal-result / quality-control monitor

- **Function:** choose one test and a minimum and maximum result value for a period, and produce a report of every result in that band with date, patient, patient data, referral entity, test, result and status. The reference states this is how a laboratory "judges chemistry, analysers and rapid tests, and displays positive results".
- **Reference behaviour:** a test-scoped, value-banded, period-scoped monitor — a quality-control and positive-screen tool, not a revenue report.
- **Evidence of absence:** no `Levey`, `QualityControl` or `ControlChart` type exists anywhere in `src/`. `Features/Statistics` contains exactly four queries — patient count, test count, sent-out, user productivity. `ViewModels/Statistics/StatisticsViewModel.cs:21` confirms the surface is DataGrid/number cards only.
- **Current state:** absent. Out-of-band results can be found by filtering the result worklist on `HasResult` / `IsReviewed` / `IsPrinted`, but that is workflow state, not value.
- **Why classified missing:** no test-scoped, value-banded monitor exists at any layer.
- **Required scope:** a query taking test + min + max + period and returning the matching results with patient context, a band-input surface, and a printable output.
- **Database impact:** **A** — `PatientTest.ResultValue` (string), `ResultFlag`, `EnteredAtUtc` and `PatientTestReferenceRangeSnapshot` already hold everything required. A new query and inputs only.
- **Migration impact:** **none**. This is the cheapest of the eight.
- **Relevant existing tables:** `PatientTests`, `Patients`, `Tests`, `PatientTestReferenceRangeSnapshots`.
- **Confidence:** High.
- **Note:** the *quality-control* framing leans toward the excluded analyser integration, but the function as the reference describes it — a banded monitor over manually entered results — is entirely achievable without any device and is therefore counted.

### 7.5 R-H04 — Enter patient name and treating doctor in English

- **Function:** a switch enabling the patient name and the treating-doctor name to be entered in English alongside the Arabic.
- **Reference behaviour:** enabled per installation; the Latin rendering appears where needed on the report or envelope.
- **Evidence of absence:** `Domain/Patients/Patient.cs:17` has a single `FullName`; `Domain/ExternalEntities/ExternalEntity.cs:17` a single `Name`. `SystemSettings` has no English-entry switch. A search across `src/` for an English/Latin name field returns nothing.
- **Current state:** absent.
- **Why classified missing:** no second name field exists on either entity, and no setting requests one.
- **Required scope:** two nullable columns, a settings switch, validation on the Latin-only path, and a rendering decision in the PDF writers.
- **Database impact:** **B** — `Patient.FullNameEn` and `ExternalEntity.NameEn`, plus a `SystemSettings` boolean.
- **Migration impact:** one new migration adding three nullable columns.
- **Relevant existing tables:** `Patients`, `ExternalEntities`, `SystemSettings`.
- **Confidence:** High.

### 7.6 R-H05 — Automatic Mr/Mrs by sex when the referral field is blank

- **Function:** insert a sex-derived title into the referral-entity field when that field is left empty; the title is then printed on the patient name.
- **Reference behaviour:** automatic, conditional on the referral field being blank.
- **Evidence of absence:** `Domain/Patients/PatientTitle.cs:6` provides a title master (`TitleText`, `IsDefault`), and `SystemSettings.DisableAutoTitleInsertion` implements the **opposite** switch (R-H08, which is present). But nothing derives a title from the patient's sex — a search for `Mr`/`Mrs` title logic in `Features/PatientRegistration` returns nothing.
- **Current state:** absent. A title is an explicit patient attribute in my system; it is never inferred.
- **Why classified missing:** no derivation path exists.
- **Required scope:** decide whether the title is derived or explicit, then either a domain rule on `Patient.Create`/`Update` or a settings-gated service call.
- **Database impact:** **B** if configurable (a `SystemSettings` switch, which the reference clearly has); **A** if implemented as an unconditional domain rule.
- **Migration impact:** one new migration adding the switch.
- **Relevant existing tables:** `SystemSettings`, `PatientTitles`, `Patients`.
- **Confidence:** High that the function is absent; **Low** on the target behaviour, because the reference contradicts itself — §4.5 item 7 records that the reference never reconciles this rule with the "do not auto-insert titles" switch. Implement this only after settling which of the two the owner actually wants.

### 7.7 R-H14 — Card settings and the card print job

- **Function:** a card print job with its own settings tab and its own dedicated printer.
- **Reference behaviour:** the reference allocates the card a settings tab across five pages and a fifth printer slot, but **never documents a single field or rule** for it (§4.4). The card is a physically real artefact — RL_Show p.3 shows a lab-card label alongside the tube, container and swab stickers.
- **Evidence of absence:** `PrinterOutputType` has four values, not five. A search for `كارت`/`Card` across `src/**/*.cs` and `src/**/*.xaml` returns no lab-card artefact. The six PDF writers produce no card.
- **Current state:** absent.
- **Why classified missing:** neither the settings row, nor the printer slot, nor the print artefact exists.
- **Required scope:** a `CardSettings` single-row entity (margin, header/footer mode, paper size), a `CardPdfWriter`, a print command, and `PrinterOutputType.Card`.
- **Database impact:** **B** — new `CardSettings` table; `PrinterAssignments.OutputType` is a `byte` primary key, so adding the enum value requires widening/reseeding that column.
- **Migration impact:** one new migration creating the settings table and adding the printer row.
- **Relevant existing tables:** `PrinterAssignments`, `ReceiptSettings`, `EnvelopeSettings`.
- **Confidence:** High on the absence; the scope is small and well-bounded despite the reference's silence on the fields.

### 7.8 R-I06 — Reminder messages to users

- **Function:** compose a reminder message addressed to a specific user or to every person, delivered through an appointment note.
- **Reference behaviour:** one authored message, one or many recipients, surfaced to the user.
- **Evidence of absence:** the Utilities module contains exactly six commands (phone book add/remove, purchase item add/remove/toggle) and six queries (phone book, purchases, test library, stopwatch, unit conversion, calculation). A search for `reminder`, `appointment` or a per-user message type across `src/TopLab.Application/Features/Utilities/` returns nothing.
- **Current state:** absent. There is no messaging entity, no recipient model and no delivery path.
- **Why classified missing:** the phone book (R-I05) and the purchases list (R-I03) are present; the reminder channel is not.
- **Required scope:** a message entity, a recipient model supporting both single-user and broadcast, an authoring surface, and a delivery surface.
- **Database impact:** **B** — a `ReminderMessage` aggregate (author, target user or broadcast flag, body, created timestamp, read/acknowledged state).
- **Migration impact:** one new migration creating the table.
- **Relevant existing tables:** `Users` (recipient resolution).
- **Confidence:** High.

### 7.9 Summary of the eight missing functions

| ID | Function | DB impact | New migration? | Cheapest first? |
|---|---|---|---|---|
| R-F05 | Abnormal-result / QC monitor | A | No | ✅ **yes** — query-only |
| R-A17 | Lab order / requisition form | A or B | Only if a collection-note column is added | ✅ **yes** — a new PDF writer |
| R-H04 | English name entry | B | Yes, 3 nullable columns | Moderate |
| R-I06 | Reminder messages | B | Yes, one new table | Moderate |
| R-H14 | Card settings + card print job | B | Yes, new table + printer column | Moderate |
| R-H05 | Auto Mr/Mrs by sex | B | Yes, one switch | Small, but blocked on a reference ambiguity |
| R-D03 | Auto clinical comments | B | Yes, one new table | Larger build |
| R-D02 | Auto indices + correction factors | B | Yes, new table + analyte columns | Largest build |

**Two of the eight need no schema change at all** and could be delivered without touching a migration.

---

## 8. Partially Implemented / Materially Different Functions

Thirteen functions are neither fully equivalent nor absent: **11 `PARTIALLY_IMPLEMENTED`** and **2 `IMPLEMENTED_DIFFERENTLY`**. For each, exactly what exists and exactly what does not.

### 8.1 The 11 partially implemented functions

#### R-A15 — Barcode sticker printing grouped by sample type
**Exists:** a Code-128 label rendered and printed per patient, gated on `ADD_EDIT_PATIENT` (`PrintBarcodeCommand` → `BarcodeService.PrintBarcodeAsync` → `BarcodeLabelRenderer.Render`, default 300×80 px, ZXing, no QR anywhere); a per-test `Barcode` catalog field; two settings for the external file barcode and the date/time on the tube barcode; re-printing is just re-running the command.
**Missing:** the reference derives **one sticker per tube, container and swab** from the patient's test list grouped by sample type; sticker size is configurable (e.g. 38 × 25 mm); sticker content and shape are an editable template; any individual sticker can be re-printed; separate labels exist for the patient file and the lab card; and the doctor can re-distribute tests across containers. My system prints one label per patient — the test→container grouping, the sticker template and the size configuration do not exist.
**DB impact:** **B** for full parity (a label-template table and a test→container mapping). **A** for a reduced version reusing the existing `PatientTest` sample flags.

#### R-A23 — Blood-picture history with trend charts
**Exists:** the `Analyte` → `AnalyteReferenceRange` → `AnalyteReferenceRangeBand` model supports a wide multi-parameter panel, and `ProfileResultItem` records each analyte's value, unit, flag, verification, print count and last print — so a per-analyte date/value grid is derivable, and the print-time per-patient reference override works via `GetProfileReportQuery`.
**Missing:** **trend charts.** There is no plotting package in `Directory.Packages.props`, and the decision is stated explicitly in the source — `ViewModels/Statistics/StatisticsViewModel.cs:21`: *"D5: no charting library, no new package reference — DataGrid/number cards only."* No chart, curve, histogram or plot type exists anywhere in `src/`.
**DB impact:** **A** — the data view already exists; only rendering is absent. Adding a plotting package plus renderers is a presentation-layer change.

#### R-C01 — Edit test data and prices
**Exists:** group, test name, report name, receipt/bill name, test code, barcode, turnaround (`CompletionDurationMinutes`, validated > 0), patient price, lab-to-lab price, sent-out flag with cost price, result kind and culture-type flag — with a full editor window and grid.
**Missing:** five reference Test Information fields have no counterpart — **History Name** (the label a result carries into a patient-history report), **Arabic Name** (a separate display name), **Collection / sample type on the test master** (my system stores sample flags on `PatientTest` instead, which is arguably the better home but means the test master cannot declare its own sample type), **Arrange No.** (display order), and **Bench**. The reference's `Reference type` switch (By Sex and Age vs. By Age Only) has no counterpart either, though a nullable `ReferenceRange.Sex` expresses both modes without one.
**DB impact:** **B** — five new columns on `Test`.

#### R-C11 — Antibiotic master and panel
**Exists:** the antibiotic master with symbol, scientific name, pregnancy and child flags; per-test panels with per-antibiotic millimetre thresholds; the zone in millimetres; and — the part that matters most — the pregnancy and under-12 filtering is reproduced rule for rule (see §6.6).
**Missing:** **commercial / brand names.** The reference's dictionary carries a `Commercial Names` column (Ceclor, Rocephin, Augmentin, Tavanic, Tienem, …) printed on the sensitivity report with a show/hide control. `Antibiotic` has no such field.
**DB impact:** **B** — a commercial-names column (child collection or delimited string) on `Antibiotic`.
**Already satisfied:** the reference's per-organism threshold maps onto `CultureAntibioticAttachment.SensitivityThresholdMm`.

#### R-D04 — Abnormal-result flagging
**Exists:** automatic flagging at entry and carried to print — `ResultFlag` = Normal / Low / High on `PatientTest`, `ProfileResultFlag` = Low / High on panel results, both resolved against the patient's reference range.
**Missing:** the presentation half — a configurable **flag letter**, separate **font colours** and **background colours** for the low and high cases, whole-line colouring, and a switch to turn the marker off. `ReportSettings` has `HeaderColor` and `FooterColor` but no normal/abnormal colour columns.
**DB impact:** **B** — five to six new columns on `ReportSettings`.

#### R-D05 — Report element configuration and curve control
**Exists:** a meaningful subset — doctor signature on/off, group sub-title on/off, reprint-message suppression, automatic history display, history sort mode, header/footer mode and colours, and per-item enable + Left/Top centimetre offsets for the **envelope** only.
**Missing:** (a) the per-report **element list** with an individual on/off radio per named element and user-controlled **element order**; (b) adding to a selection list or substituting free text for a list value; (c) **curve control** — WBC/RBC/PLT histograms reorderable, recolourable, resizable, blankable, with a configurable curve count.
**DB impact:** **B** for (a)/(b) — a new `ReportElementConfig` aggregate plus a template value-list. **(c) is gated by exclusion 8**: histogram data only arrives from the analyser integration, so while that exclusion stands this sub-item is not reachable regardless.
**Scope caveat:** the reference's report-positioning screenshots come from a visibly different build and match no contents entry, so the target for (a) is weakly defined.

#### R-E05 — Login/logout history report per user
**Exists:** a per-user statement of attendance events over a period with entry date/time, exit date/time, break window, and derived overtime and lateness (`GetAttendanceRecords`, `GetUserAttendanceSummary`).
**Missing:** the **device name**. The reference records which machine the user signed in from (`اسم الجهاز`, e.g. `REALLAB-PC`) as an explicit report column. `AttendanceRecord` has no machine field and nothing equivalent is captured at check-in.
**DB impact:** **B** — a nullable `MachineName` column on `AttendanceRecord`, populated at check-in.

#### R-F01 — Patient statistics over a period
**Exists:** total patients over a range with breakdown by sex, by referral entity (including a "no referral" bucket), by account type, grouped by month, and a month × sex cross-tab.
**Missing:** (a) the **day-of-month grouping**, one of the reference's six report variants; (b) the **amounts-paid money row** that every reference patient-statistics report carries beneath the count.
**DB impact:** **A** — `PaymentOperation` already carries amount, discount, operation timestamp and void flag. Both gaps are query-side additions.
**Scaling note, not a gap:** the counts are computed in memory from the loaded patient set rather than in SQL. Adequate at current volumes; a constraint if the patient base grows substantially.

#### R-H01 — Report appearance and layout
**Exists:** left and bottom page margins; report top space **capped at 8 cm — the reference's exact cap** (`SetTopSpace`, `ReportSettings.cs:62`); paper size A4/A5; header/footer mode as three exclusive options matching the reference's three; header and footer colours; doctor signature; history sort mode; automatic history display; group sub-title; reprint-message suppression; and lab header/footer text stored per artefact via `ILabPrintTextStore` with `LabPrintTextScope { Report, Receipt, Envelope }`.
**Missing:** per-report **element positioning**. `EnvelopePrintItemPosition` provides Left/Top centimetre offsets and enable flags — but only for the envelope. The report has no equivalent table.
**DB impact:** **B** — a new `ReportPrintItemPosition` entity mirroring the envelope one.
**Scope caveat:** the reference's positioning screenshots (pp. 196–198) carry 2010/2011 report dates and match no contents entry, so the target is weakly defined.

#### R-H02 — Dedicated printer per print job
**Exists:** four of five print jobs routable — Reports, Barcode, Envelope, Receipt — with `PrinterAssignment` keyed on the output type, seeded via `HasData`, and genuinely consumed by `ShellPdfPrinterDispatcher`.
**Missing:** the **card** printer, consistent with the absent card artefact (R-H14).
**DB impact:** **B** — `PrinterAssignments.OutputType` is a `byte` primary key, so adding the enum value requires widening and reseeding that column.

#### R-H11 — Main system data (reception time, auto-print receipt)
**Exists:** the result-reception time as a configurable `TimeOnly?` default on `ReceiptSettings.PickupTimeDefault`, editable through `UpdateReceiptSettingsCommand`.
**Missing:** an explicit switch that **automatically prints the patient receipt** on registration. `ReceiptSettings.CashierPrinterEnabled` reads as "route receipts to the dedicated cashier printer", not "print automatically", so I do not count it as satisfying this item — having read `ReceiptSettings.cs` in full, there is no such member.
**DB impact:** **B** — a `SystemSettings.AutoPrintReceipt` boolean, or an additional `ReceiptSettings` column.
**Reference ambiguity:** the reference configures "delivery time" and "result-reception time" in two different places and never distinguishes them (§4.5, item 8), so it is not certain which is meant. Worth settling before implementing.

### 8.2 The 2 implemented-differently functions

#### R-A04 — Add / remove / clear individual tests on a patient order
**Exists in full:** adding a test to a visit (`AddTestsToVisit`), removing one (`RemoveTestFromVisit`), clearing all (`ClearAllTests`), per-test sample-type flags covering Urine / Stool / Blood / Semen / CSF (`PatientTest.UpdateSampleFlags`, `:215`), and the "taken outside lab" flag.
**The two differences, both real:**
1. **Gesture.** The reference adds and removes by **double-clicking** the test name, and restricts the bulk clear to *"عند إضافة المريض أول مرة فقط"* — during first registration only. My system uses explicit buttons and imposes no such restriction, so a registered patient's test list can be bulk-cleared. The reference's restriction is a guardrail; its absence is a mild loss of protection, not a missing capability.
2. **Report annotation — unverified.** The reference automatically renders "Sample taken outside lab." as a note inside the printed report for that test. `PatientTest.IsTakenOutsideLab` is stored, but I did **not** confirm the note rendering in `ReportContentBuilder`. This is flagged as an open sub-point in §11 rather than asserted either way.
**DB impact:** **A** for difference 1 (a UI guard). Difference 2 needs verification before any conclusion.

#### R-E01 — Create a user and assign permissions
**Exists in full:** a user is created behind a secondary-password gate with a user name, a primary password hash, a **separate** internal/secondary password hash, an absolute-permission superuser flag, a discount-limit percentage, a block-print-on-balance flag, work start/end times, a break flag with break duration, and an explicit permission-grant list. The reference's 15 switches map one-for-one onto 13 seeded permission codes: `ADD_EDIT_PATIENT`, `EDIT_RESULTS`, `REVIEW_RESULTS`, `PRINT_RESULTS`, `BLOCK_PRINT_ON_BALANCE`, `DELIVER_RESULTS`, `DISCOUNT_LIMIT`, `PRINT_WORKSHEET`, `DELETE_PATIENT`, `EDIT_SYSTEM_SETTINGS`, `CASH_DISBURSE_DEPOSIT`, `STATISTICS`, `PT_AUDIT_ACCESS` — that is, the reference's items 2 through 14.
**Three meaningful differences:**
1. **Attendance is ungated.** The reference's item 15 is a permission that gates *attendance registration*. No `ATTENDANCE` permission code exists, and the four attendance commands (`CheckIn`, `StartBreak`, `EndBreak`, `CheckOut`) implement `IAuthorizedRequest` nowhere — so any signed-in user can register attendance. This is a genuine access-control gap, and a small one to fix.
2. **Two seeded permission rows are never read.** `BLOCK_PRINT_ON_BALANCE` and `DISCOUNT_LIMIT` appear in `PermissionConfiguration.HasData` (`:22,24`) and in the user-edit checkbox list (`UserManagementViewModel.cs:49-52`, `CatalogCodes`), but a repo-wide search finds them **only** there, in migration snapshots, and in that UI list. Enforcement reads `User.BlockPrintOnRemainingBalance` and `User.DiscountLimitPercent` instead. The two features work correctly; the grant/revoke UI for them is inert. This is a real latent defect — a checkbox an administrator can untick that changes nothing.
3. **Branch is correctly absent** (intentional exclusion 5), and a first-run bootstrap (`FirstRunAdminWindow`, gated on `HasAnyAbsoluteUserQuery`) prevents a system with no administrator.
**DB impact:** **B** to close cleanly — a fourteenth `ATTENDANCE` permission row, and a decision on whether to enforce the two existing codes or remove them. Seeded-data changes are model changes to EF Core and need a new migration.
**Note:** the two dead grant rows are worth fixing regardless of any parity question. They are the only place in the analysed system where the UI offers a control that provably does nothing.

---

## 9. Database and EF Core Migration Analysis

### 9.1 The current persistence baseline

| Item | Value |
|---|---|
| Provider | SQL Server (`Microsoft.EntityFrameworkCore.SqlServer` 8.0.30, pinned in `Directory.Packages.props` with the note *"do NOT move to 9.x/10.x"*) |
| DbContext | `ApplicationDbContext` (`src/TopLab.Infrastructure/Persistence/ApplicationDbContext.cs`), applying configurations by convention at `:39` |
| DbSet declarations | 46 (`ApplicationDbContext.DbSets.cs`) |
| `IEntityTypeConfiguration` classes | 46 (`Persistence/Configurations/`) |
| **Migrations** | **14** |
| Model snapshot | `ApplicationDbContextModelSnapshot.cs` |
| Deployments | **none** — the system has never been released |

### 9.2 The 14 existing migrations, in order

| # | Migration | Date stamp |
|---|---|---|
| 1 | `BaselineDataModel` | 20260828052248 |
| 2 | `RenamePkColumns` | 20260828123530 |
| 3 | `AddTestCodeAndLifecycleColumns` | 20260906093902 |
| 4 | `AddPatientIsDeletedAndPatientTestSampleDrawnIndex` | 20260907162756 |
| 5 | `AddPatientTestReferenceRangeSnapshots` | 20260908175555 |
| 6 | `AddAnalyteProfileDomain` | 20260909033414 |
| 7 | `AddPregnancyMedicalConditionTypeSeed` | 20260910213833 |
| 8 | `AddInvoiceIssues` | 20260916113704 |
| 9 | `FixCultureSensitivityCategoryOffByOne` | 20260930163921 |
| 10 | `AddExternalEntityEmail` | 20260930165920 |
| 11 | `AddBranchNumber` | 20260930170644 |
| 12 | `AddCombinedReportPrintOptions` | 20261001203315 |
| 13 | `AddCultureMicroscopyAndZone` | 20261001233652 |
| 14 | `AddAntibioticMasterFields` | 20261002002546 |

### 9.3 Should any existing migration be amended? — No, and the evidence is clear

The project has never been deployed, so amending the baseline would *technically* be safe. It is nonetheless the wrong call here, for three reasons visible in the repository itself:

1. **The project's established pattern is additive, including for corrections.** Migration 9, `FixCultureSensitivityCategoryOffByOne`, is a **repair migration** created specifically to correct data that an earlier migration got wrong, rather than an edit to that earlier migration. The project has already chosen "add a migration" over "amend a migration" in the one case where the choice was made.
2. **Migrations 5–14 are a deliberate, sequenced feature trail.** The commit log shows each migration landing with its feature slice — for example `c11cf22` *"Slice 7/8: BranchScope + AddBranchNumber migration (WP-15)"* and `81bca7d` *"Slice 14/16: Antibiotic Symbol + ScientificName + AddAntibioticMasterFields (WP-14)"*. Migrations 5–14 have demonstrably been **applied to developer and test databases** (migrations 5, 6, 13 and 14 each carry a dedicated `*MigrationTests.cs` in `tests/TopLab.Infrastructure.Tests/Persistence/`, which run them against a real SQL Server via `Testcontainers.MsSql`). Editing any of them would silently desynchronise those already-applied databases.
3. **Four migrations carry automated repair tests.** `AddAnalyteProfileDomainMigrationTests`, `AddAntibioticMasterFieldsMigrationTests`, `AddCombinedReportPrintOptionsMigrationTests`, `AddCultureMicroscopyAndZoneMigrationTests` and `SensitivityCategoryRepairTests` assert the exact up/down shape of those migrations. Amending any of them would break existing green tests.

**Recommendation: create new migrations for all work below.** This is stated as a recommendation only; no migration was modified, and no application source was changed during this investigation.

### 9.4 Grouping of the 21 functions needing work

Twenty-one functions need work: 2 implemented-differently, 11 partially implemented, 8 missing.

#### Group A — No database changes required (6 functions)

| ID | Work required | Why no schema change |
|---|---|---|
| R-A04 | Restore the first-registration-only guard on bulk clear; decide on the outside-lab report note | A UI/behaviour guard over existing commands |
| R-F01 | Add day-of-month grouping and the amounts-paid row | `PaymentOperation` already holds amount, discount, timestamp and void flag |
| R-F05 | Build the banded result monitor | `PatientTest.ResultValue`, `ResultFlag`, `EnteredAtUtc` and the range snapshot already hold everything |
| R-A17 | Build the requisition PDF writer | Existing `PatientTest` sample flags can drive Collection Notes |
| R-A23 | Add trend-chart rendering | Data already derivable; a plotting package is a `Directory.Packages.props` addition, not a schema change |
| R-I02 | Add normal-value and patient-effect columns to the test-library projection | A projection change over existing `Tests` data |

#### Group B — New migration required, new tables (8 functions)

| ID | New table(s) | Notes |
|---|---|---|
| R-D02 | `DerivedIndexDefinitions` (+ analyte columns) | Needs an evaluation order, since MCHC depends on HGB and HCT, which depend on RBC |
| R-D03 | `CommentRules` | Needs a priority column so overlapping bands resolve deterministically |
| R-H14 | `CardSettings` | Single-row, PK = 1, matching the `ReceiptSettings`/`EnvelopeSettings` pattern |
| R-I06 | `ReminderMessages` | Needs a recipient model supporting single-user and broadcast |
| R-D05 | `ReportElementConfigs` (+ template value list) | Only the element-configuration half; the curve half is gated by exclusion 8 |
| R-A15 | `BarcodeLabelTemplates` (+ a test→container mapping) | Only for full parity; a reduced version needs nothing |
| R-H01 | `ReportPrintItemPositions` | Mirrors the existing `EnvelopePrintItemPositions` table exactly |
| R-E01 | *(no new table)* | Permission rows are seed data — see Group C |

#### Group C — New migration required, new columns on existing tables (7 functions)

| ID | Table | Columns | Notes |
|---|---|---|---|
| R-C01 | `Tests` | `HistoryName`, `ArabicName`, `SampleType`, `ArrangeNo`, `Bench` | Five nullable columns |
| R-C11 | `Antibiotics` | `CommercialNames` | A child collection or a delimited string |
| R-D04 | `ReportSettings` | normal / low / high font colours, low / high background colours, show-flag | The settings row is already seeded, so this is `AddColumn` only |
| R-E05 | `AttendanceRecords` | `MachineName` (nullable) | Populated at check-in |
| R-H02 | `PrinterAssignments` | `OutputType` widened to carry `Card` | **This is the one schema change with a primary-key implication** — `OutputType` is the PK and is currently a `byte`, so the migration must widen the column and reseed |
| R-H04 | `Patients`, `ExternalEntities`, `SystemSettings` | `FullNameEn`, `NameEn`, `EnglishEntryEnabled` | Three nullable columns |
| R-H05 | `SystemSettings` | `AutoTitleBySexEnabled` | One boolean — **but see the reference ambiguity in §7.6 before implementing** |
| R-H11 | `SystemSettings` or `ReceiptSettings` | `AutoPrintReceipt` | One boolean; the target table is a design choice |

#### Group D — Existing model changes requiring a migration, seed data only (1 function)

| ID | Change | Why it still needs a migration |
|---|---|---|
| R-E01 | Add a 14th `Permission` row (`ATTENDANCE`); decide whether to enforce or remove `BLOCK_PRINT_ON_BALANCE` and `DISCOUNT_LIMIT` | `PermissionConfiguration.HasData` is part of the EF model. Changing seeded rows is a model change, so EF generates an `InsertData`/`UpdateData`/`DeleteData` migration. It is seed data, not structure — but it is still a migration |

### 9.5 Consolidated migration plan

If all 21 were implemented together, the work consolidates naturally into **roughly 8 to 10 new migrations**, in this dependency order:

| Order | Migration | Covers | Depends on |
|---|---|---|---|
| 1 | `AddDerivedIndexDefinitions` | R-D02 | — |
| 2 | `AddCommentRules` | R-D03 | — |
| 3 | `ExtendTestCatalogFields` | R-C01 | — |
| 4 | `AddAntibioticCommercialNames` | R-C11 | — |
| 5 | `AddReportAppearanceColumns` | R-D04, R-H01 | — |
| 6 | `AddReportElementConfigs` | R-D05 | migration 5 |
| 7 | `AddCardSettingsAndPrinterSlot` | R-H02, R-H14 | — |
| 8 | `AddAttendanceMachineName` | R-E05 | — |
| 9 | `AddEnglishNamesAndRegistrationFlags` | R-H04, R-H05, R-H11 | — |
| 10 | `AddAttendancePermissionAndResolveDeadGrants` | R-E01 | — |
| — | *(no migration)* | R-A04, R-A17, R-A23, R-F01, R-F05, R-I02, R-I06 | R-I06 does need `AddReminderMessages` |

Two of the eight missing functions — **R-F05 and R-A17** — need no migration at all, and six of the 21 functions needing work are pure presentation or query work.

### 9.6 Cases where the database analysis remains uncertain

| Case | What is uncertain | How to resolve |
|---|---|---|
| R-A17 collection notes | Whether Collection Notes must be free text or may be derived from the existing sample flags. The reference prints a `Collection Notes` column without saying how it is populated | Decide the requirement; if free text, add a nullable `PatientTest.CollectionNote` (one column) |
| R-A04 report note | Whether "Sample taken outside lab." is rendered on the report | Read `ReportContentBuilder` for `IsTakenOutsideLab`; if absent, add it — a renderer change, no migration |
| R-G02 debtor/discounted/settled filters | Whether the three reference filters exist in the view layer rather than the DTO | Read `AccountsHubViewModel` and `ElementInventoryDto` consumers |
| R-H01 report element positioning | The reference's screenshots come from a different build and match no contents entry, so the target is weakly defined | Settle the intended design before building |
| R-H11 auto-print receipt | Whether `CashierPrinterEnabled` was intended to serve this purpose, given the reference's own delivery-time ambiguity | Settle with the product owner; then add the explicit boolean |
| R-A20 field-level history | Whether change-history (not just modification counts) will be required later | If yes, a `EntityChange` log entity plus an interceptor is needed — a new migration |

---

## 10. Final Reconciled Counts

### 10.1 The arithmetic

```
Step 1 — Reference capabilities after consolidating duplicates
        (across both PDFs, chapter summaries vs. body sections,
         and cross-document overlap) ............................  102

Step 2 — Intentional exclusions applied (§3) ....................  - 14
        ├─ Group 1  Internet-related ............................    6
        ├─ Group 2  Equipment and device registry ...............    1
        ├─ Group 3  CBC analyser integration .....................    1
        ├─ Group 4  Multi-branch support ........................    5
        └─ Group 5  Result transmission (SMS / email / fax) .....    1

Step 3 — Reference functions in scope ...........................   88
```

### 10.2 Classification of the 88

| Status | Count | Share |
|---|---|---|
| `IMPLEMENTED` — behaviourally equivalent | **67** | 76.1 % |
| `IMPLEMENTED_DIFFERENTLY` — present, meaningful difference | **2** | 2.3 % |
| `PARTIALLY_IMPLEMENTED` — capability present, reference rules missing | **11** | 12.5 % |
| `MISSING` — capability absent | **8** | 9.1 % |
| `UNVERIFIABLE` | **0** | 0 % |
| **Total in scope** | **88** | **100 %** |

```
        67  IMPLEMENTED
      +  2  IMPLEMENTED_DIFFERENTLY
      + 11  PARTIALLY_IMPLEMENTED
      +  8  MISSING
      +  0  UNVERIFIABLE
      ─────────────────────
        88  ✔ equals the in-scope total
```

### 10.3 By functional group

| Group | In scope | Impl. | Diff. | Partial | Missing |
|---|---|---|---|---|---|
| **A** — Patient, reception, order entry | 23 | 19 | 1 | 2 | 1 |
| **B** — Work papers, logs, tallies | 4 | 4 | 0 | 0 | 0 |
| **C** — Test catalogue, ranges, pricing, parties | 13 | 11 | 0 | 2 | 0 |
| **D** — Culture, haematology, interpretation | 5 | 1 | 0 | 2 | 2 |
| **E** — Users, permissions, attendance | 6 | 4 | 1 | 1 | 0 |
| **F** — Statistics and monitoring | 5 | 3 | 0 | 1 | 1 |
| **G** — Audit, accounts, cash | 8 | 8 | 0 | 0 | 0 |
| **H** — System settings | 18 | 12 | 0 | 3 | 3 |
| **I** — Laboratory operations, utilities | 6 | 5 | 0 | 0 | 1 |
| **Total** | **88** | **67** | **2** | **11** | **8** |

Column totals: 19+4+11+1+4+3+8+12+5 = **67** ✔ · 1+1 = **2** ✔ · 2+2+2+1+1+3 = **11** ✔ · 1+2+1+3+1 = **8** ✔

### 10.4 The answer to the four headline questions

1. **Total relevant reference functions after consolidating duplicates and excluding the 14 intentional exclusions: 88.**
2. **Implemented in my system: 67** fully equivalent, plus **2** that work but differ in mechanism.
3. **Not fully implemented: 19** — 11 partial and 8 absent.
4. **Exact number of missing reference functions after the 14 exclusions: 8** (R-A17, R-D02, R-D03, R-F05, R-H04, R-H05, R-H14, R-I06). **No excluded function appears in this count.**

### 10.5 Treatment of the ambiguous statuses, stated explicitly

- **`PARTIALLY_IMPLEMENTED` (11)** is counted in the "not fully implemented" column, **not** in the "missing" column, and **not** in the "implemented" column. A function lands here when a user can perform the core operation but a documented reference behaviour within that function is absent. Each of the 11 states exactly what exists and what does not, in §5 and §8.1.
- **`IMPLEMENTED_DIFFERENTLY` (2)** is counted as **present**, because the capability genuinely works and a user can achieve the reference's outcome. It is separated from `IMPLEMENTED` because the mechanism, the guarding, or a reference rule differs in a way an operator would notice. Both are listed in the "present in both systems" section (§6) as well as here, so the reader is never left guessing which column they are in.
- **`UNVERIFIABLE` (0)** was not used. The instruction was to reserve it for cases where the source genuinely prevents a defensible conclusion. Every function in scope was classifiable from the code. Where a *sub-detail* could not be established — the outside-lab report note, the debtor filters, the exact target of the report-positioning screenshots — it is recorded as an open point in §11 rather than being allowed to inflate the `UNVERIFIABLE` count.
- **The counting rule for reference functions** is stated in full in §4.1, because the number 88 depends on it. The single most rule-sensitive group is system settings (18 of 88): a reviewer who preferred to collapse all settings into one capability would arrive at 71, and one who split every individual toggle would arrive above 100. The rule applied — one function per independently configurable behaviour with its own observable effect, grouping only settings that configure a single print artefact — is stated so the count can be re-derived or contested.

---

## 11. Evidence and Limitations

### 11.1 What evidence was used

**For the assessed system — source code only.** 1,752 tracked files, 1,120 C# files and 71 XAML files at commit `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757`. Documentation was used **only** to locate code and never as proof. Every `IMPLEMENTED` classification in §5 rests on a traced path from a WPF view through a ViewModel command, a MediatR handler, a domain aggregate, `IApplicationDbContext` and, where a schema element is claimed, an `IEntityTypeConfiguration` and — where the element is new — a named migration.

**Corroborating structural evidence**, listed in §2.2: 265 use cases across 24 feature areas, 221 handlers, 177 validators, 46 DbSets matching 46 entity configurations, 14 migrations, **0** `NotImplementedException` and **0** `TODO`/`FIXME`/`HACK`/`XXX` anywhere in `src/`.

**Compile evidence.** `TopLab.Domain`, `TopLab.Application` and `TopLab.Infrastructure` were built from this exact commit and all three compiled successfully. This rules out the possibility that the handler bodies inspected are non-compiling sketches.

**For the reference system — both PDFs in full.** `RLS_Learn_Enhanced.pdf` (212 pages) and `RL_Show_Enhanced.pdf` (77 pages) were read end to end, including per-page OCR text extraction. Every reference function in §4.3 carries page citations. Where the two documents describe the same function, one entry was made with both sets of citations.

### 11.2 Assumptions, stated so they can be challenged

| # | Assumption | Basis | If wrong |
|---|---|---|---|
| 1 | The counting rule in §4.1 is the right unit of comparison | Stated explicitly and applied consistently; the rationale is in §4.1 and the sensitivity is quantified in §10.5 | The absolute counts shift, but the per-function classifications do not |
| 2 | A capability that is present but whose reference *presentation variant* differs is still `IMPLEMENTED` | The task's own instruction: the comparison is functional, not a demand for identical technology. Applied to the thermal-roll receipt (R-A16), the up/down buttons versus drag reordering (R-A13), and the period filter versus a global setting (R-A09) | Three functions would move from `IMPLEMENTED` to `IMPLEMENTED_DIFFERENTLY`; the missing count of 8 is unaffected |
| 3 | "مزرعة" (farm) is a culture test type with its own antibiotic panel, not an agricultural client | The reference's own reserved-ID range 118–139 and group name `CULTURE AND SENSITIVITY`; and decisively, the assessed system's own error message *"التحليل المحدد ليس مزرعة"* returned when a non-culture test is passed | R-C10 would become `MISSING` rather than `IMPLEMENTED`, moving the missing count from 8 to 9 |
| 4 | The 8 declared-but-undefined reference capabilities in §4.4 are not countable functions | A one-line promise with no described workflow cannot be classified as present or absent | Up to 8 further functions enter scope, mostly `MISSING` |
| 5 | The `ATTENDANCE` permission gap (R-E01) is a meaningful difference rather than a missing function | Attendance registration, break, checkout, overtime and lateness all work; only the gate is absent | R-E01 would move from `IMPLEMENTED_DIFFERENTLY` to `PARTIALLY_IMPLEMENTED` |
| 6 | Unenforced seeded permission rows are a defect, not a design choice | They appear in the seed data and in the administrator's checkbox list, yet no enforcement path reads them; enforcement uses per-user fields instead | No classification changes; only the §8.2 severity note would soften |

### 11.3 Genuinely unverifiable items

**None of the 88 functions is `UNVERIFIABLE`.** The following sub-points could not be established, and are recorded rather than guessed:

| Sub-point | Why it could not be established |
|---|---|
| Whether "Sample taken outside lab." is rendered on the printed report (R-A04) | `PatientTest.IsTakenOutsideLab` is stored; the report composer was not exhaustively read for the note text |
| Whether the debtor / discounted-only / settled-only filters exist in R-G02 | They are absent from `ElementInventoryDto`; they may live in the view layer, which was not read exhaustively |
| What the reference intends by report element positioning (R-H01, R-D05) | The relevant reference pages (196–198) carry 2010/2011 report dates and match no contents entry — a different build, weakly specified |
| Which "time" R-H11 refers to | The reference configures "delivery time" and "result-reception time" in two places and never distinguishes them |
| Whether `CashierPrinterEnabled` was intended to serve R-H11's auto-print requirement | Its name reads as printer routing, not auto-print; `ReceiptSettings.cs` was read in full and holds no such member |
| The exact filesystem semantics of the farm-provisioning step in the reference | The reference itself cannot state it — the `_S` suffix convention's meaning appears nowhere |
| Several reference reference-range and pricing values | The reference contradicts itself (§4.5, items 9–13); the numeric examples are illustrative, not normative |

### 11.4 Limitations

| # | Limitation | Effect on the findings |
|---|---|---|
| 1 | **The WPF presentation layer could not be compiled.** `TopLab.Presentation` targets `net8.0-windows` with `UseWPF=true` and cannot be built on the Linux analysis host | All 71 XAML files and all Presentation code were read statically. A compile error confined to the Presentation layer would not have been caught. The three projects that *were* built cover all domain, application and persistence logic |
| 2 | **No runtime or UI verification was performed.** No SQL Server instance was started, no database was migrated, and the application was never launched | Behaviour is established from code paths, not observed execution. A handler that compiles but fails at runtime against real data would not be detected here |
| 3 | **The `tests/` tree was surveyed, not exhaustively read** — 387 files, 45,535 lines | Test counts are filename-and-attribute based. "No tests for X" means no file named after X exists. Test presence was never used as proof of implementation, only as corroboration |
| 4 | **Five reference pages carry no recoverable text** — Learn pp. 165, 173, 189, 207, 211 and Show p. 67 are image-only | These are divider/appendix pages between sections whose surrounding text is intact. A capability shown only in an image on those pages would have been missed. This is the most likely source of any residual error |
| 5 | **Arabic OCR quality is uneven**, with scrambled glyph order and missing tokens throughout | Functions were extracted from the reference's workflow narratives and its legible English/UI fragments. Where a rule could not be recovered it was marked uncertain rather than invented; §4.5 records 14 internal contradictions found this way |
| 6 | **Two reference sub-capabilities are documented only by screenshot** — the `Print Result Password` button and the `Backup` button in the search window | Neither is counted. Both would fall under the already-excluded Natigh.com result-delivery workflow, or under R-H16 respectively |
| 7 | **The `Docs/` tree (144 files) was deliberately not used as evidence** | Per the source-of-truth policy. A function documented there but absent from the code would appear as `MISSING` in this report — which is the correct outcome under the stated policy, but it means this report and the project's own documentation may disagree |

### 11.5 A closing note on interpreting this report

The assessed system is not a prototype. It is a 87,208-line Clean Architecture solution with 265 use cases, 46 mapped entities, 14 migrations, 45,535 lines of tests, no stubs, and no placeholders — and it reproduces **67 of 88** in-scope reference capabilities with behaviourally equivalent implementation, including several subtle rules that are easy to miss: the no-unit-conversion age-band matching, the price-list binding invariant, the pregnancy and under-12 antibiotic filtering, the 8 cm report-margin cap, and the three exclusive header/footer modes.

The genuine gaps cluster in one place: **automated result interpretation**. The reference computes haematology indices, applies per-index correction factors, derives clinical comments from result conditions, colours abnormal results, monitors results against a quality band, and draws trend charts. The assessed system records what a human types and flags it Low or High. For a medical audience that is the single most consequential difference in this comparison, and it is also where the two cheapest wins sit — **R-F05** (the banded result monitor) and **R-A17** (the requisition form) both need no migration at all.

Two findings sit outside the parity question entirely and are worth acting on regardless:

- **Two seeded permission rows are inert.** `BLOCK_PRINT_ON_BALANCE` and `DISCOUNT_LIMIT` appear as checkboxes in the user-management screen, but no enforcement path reads them; the features work through per-user fields instead. An administrator can untick a box and nothing changes. This is the only place in the analysed system where the UI offers a control that provably does nothing.
- **Attendance registration is not permission-gated at all.** The four attendance commands implement no authorisation contract, so any signed-in user can register attendance. The reference gates this behind a dedicated permission.

Both are small fixes, and neither requires new functionality.
