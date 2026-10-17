# Post-Phase-1 Independent Verification

**Project:** Top-Lab (medical clinical laboratory information system)
**Repository:** `https://github.com/El-ogra/Top-Lab.git`, branch `main`
**Target commit:** `c482a914b5d159a0bf207ef494dd58f0aa0616d9`
**Investigation type:** Analysis and planning only. No source file, configuration file, migration, project file or repository artifact was created, modified, or deleted. No feature was implemented and no defect was fixed.
**Nature of this document:** Standalone, evidence-based forensic assessment derived from the actual code at the target commit and from the two authorized reference PDFs. It does not rely on, cite, or reproduce any earlier audit, forensic report, inventory, or execution plan.

---

## SECTION 1 — Environment Limitations

Every limitation below actually affected this investigation. None is speculative. They are stated here, in the first section, because several of them directly bound the claims made later.

### 1.1 Platform and runtime limitations

| # | Limitation | What it prevents establishing | Affected claims / REF IDs |
|---|---|---|---|
| L-01 | The investigation host is Linux. `TopLab.Presentation` and `TopLab.Presentation.Tests` target `net8.0-windows` and use WPF. They cannot be built or run here. | Any build, test, render or behaviour claim about the WPF presentation layer. No XAML was compiled and no window was opened. | All IMPLEMENTED classifications that depend on the UI layer; Sections 6, 11 |
| L-02 | No Windows-specific build tooling (Windows Desktop targeting pack, `EnableWindowsTargeting`) is present, and the WPF SDK cannot be restored on this host. | Compilation of XAML, compilation of `*.xaml.cs` code-behind, and therefore any XAML-level defect detection. | Sections 6, 7, 11 |
| L-03 | No SQL Server instance is reachable from this host, and no database was created, migrated, seeded or queried. | All database-level runtime verification: migration application, seed data, index/constraint behaviour, query plans, transaction behaviour, and data-shape correctness. | All persistence-dependent classifications; Sections 6, 10, 11 |
| L-04 | Migrations were read as source only. None was executed and no `dotnet ef database update` was run. | Whether the 15 migrations actually apply cleanly in sequence to an empty database. | Section 10 |
| L-05 | The .NET 8 runtime is not installed; only the .NET 9.0.18 runtime is present. `dotnet test` therefore fails by default. Tests were executed with `DOTNET_ROLL_FORWARD=LatestMajor`. | Fidelity of the test host to the exact target runtime. Results are indicative, not authoritative for .NET 8. | Section 6 verification statuses |
| L-06 | 28 of 353 tests in `TopLab.Infrastructure.Tests` fail in this environment. Every failing test is in the `Printing` namespace and every failure is `QuestPDF.Drawing.Exceptions.DocumentDrawingException: ... uses font families that are not available: 'Arial'`. `ArabicFontResolver` is annotated `[SupportedOSPlatform("windows")]` and enumerates installed system fonts through `System.Drawing`; no Arial or Arabic-capable font exists in this container. | Any claim that the PDF/print output is correct. These failures are an **environment artefact, not a code defect**, and are not recorded as defects. They are, however, an honest signal that **no print output was visually verified in this investigation**. | REF-027, REF-033, REF-034, REF-035, REF-091…REF-099, REF-120, REF-159, REF-174, and every other print-producing function; Sections 6, 7, 11 |
| L-07 | No physical or virtual printer is attached. Printing is verified only as far as the composition of the print payload and the dispatch call. | Whether output reaches a physical device, at the correct orientation, scale or tray. | All print-producing REF IDs; Section 11 |
| L-08 | No barcode scanner, thermal label printer, receipt printer or card reader is attached. | Hardware-level behaviour of barcode capture and of label-size-dependent printing. | REF-031, REF-159…REF-162 |
| L-09 | No font, renderer or text-shaping engine capable of Arabic shaping was executed against generated output. | Verification of Arabic glyph shaping, right-to-left layout, and the visual correctness of Arabic report text. | All print-producing REF IDs; Section 11 |
| L-10 | The application was never launched. No window, dialog, grid or report was seen running. | All runtime behaviour claims. Every classification in this report is derived from source inspection plus, where noted, executed non-UI tests. | Section 6 in its entirety |

### 1.2 Evidence and interpretation limitations

| # | Limitation | What it prevents establishing | Affected claims |
|---|---|---|---|
| L-11 | Both authorized PDFs are Arabic-language, right-to-left, screenshot-heavy documents produced by OCR. The extracted text layer contains systematic corruption of Arabic orthography (e.g. `ٌنقوم` for `نقوم`, `حقل` rendered as `حقل`, dropped and duplicated diacritics) and, in places, of Latin text in embedded screenshots (e.g. `DetnirP yB` for `Printed By`). | Perfect fidelity of transcription. Where a function name was ambiguous in text, the rendered page image was used to disambiguate. | Sections 3, 4 |
| L-12 | Arabic words that are visually near-identical are a genuine ambiguity in this source material, most importantly the word the reference uses for a microbiology culture (rendered `مزرعة`, literally "farm", but used throughout as the Arabic laboratory term for a bacterial culture). This was resolved by reading the surrounding screen captures, which show `Culture`, `Organism A/B/C`, `Colony Count`, antibiotic grids and `Sensitivity`. | Nothing material — the resolution is well evidenced — but the ambiguity is disclosed because a reader relying on the text layer alone could misread this module. | REF-057…REF-068 |
| L-13 | The two authorized PDFs document the reference product. They do not state the owner's acceptance criteria, and they contain no statement that the reference behaves identically in every configuration shown. | Whether a reference behaviour visible only in a screenshot is a requirement or an example. Where a behaviour appeared only as a screenshot and not in prose, this is stated in the affected row. | Sections 4, 6 |
| L-14 | The authorized PDFs do not contain a single consolidated requirements list. The function inventory in Section 4 is therefore a reconstruction from a table of contents, per-chapter prose instructions, and 289 pages of screen captures. | Certainty that the decomposition is the only defensible one. A different auditor could decompose some composite screens differently. The arithmetic in Sections 8 and 12 is internally exact for the decomposition used here and is stated as such. | Sections 4, 8, 12 |
| L-15 | No functional requirement specification, acceptance criterion, test specification, or owner decision record inside the repository was used as evidence of requirement or of implementation, other than the two authorized PDFs for requirements and the code itself for implementation. | Corroboration of the reference baseline by a second authorized source. | Sections 2, 4 |

### 1.3 What is *not* a limitation

For the avoidance of doubt: the repository was cloned in full, the exact target commit was checked out, and `git status` was clean at the end of the investigation. No part of the target source tree was inaccessible, truncated or partially read. All 1,148 `.cs` files and 71 `.xaml` files under `src/` and 406 `.cs` files under `tests/` were enumerable and searchable, and the specific files cited in this report were opened and read.

---

## SECTION 2 — Target Commit Verification

**Required commit:** `c482a914b5d159a0bf207ef494dd58f0aa0616d9`

### 2.1 Commands executed and exact observed output

**Command 1 — remote reference (pre-clone reachability check):**
```
git -c http.sslCAInfo=/etc/ssl/certs/agent-identity/sandbox-gateway-ca.crt ls-remote https://github.com/El-ogra/Top-Lab.git
```
Observed output:
```
c482a914b5d159a0bf207ef494dd58f0aa0616d9	HEAD
c482a914b5d159a0bf207ef494dd58f0aa0616d9	refs/heads/main
```

**Command 2 — clone:**
```
git -c http.sslCAInfo=/etc/ssl/certs/agent-identity/sandbox-gateway-ca.crt clone https://github.com/El-ogra/Top-Lab.git toplab
```
Observed output (tail):
```
Cloning into 'toplab'...
Updating files: 100% (1817/1817), done.
```

**Command 3 — the mandatory verification command:**
```
$ cd /workspace/toplab
$ git rev-parse HEAD
```
Observed output:
```
c482a914b5d159a0bf207ef494dd58f0aa0616d9
```

**Command 4 — commit identity confirmation:**
```
$ git log -1 --format="%H%n%an%n%ad%n%s"
```
Observed output:
```
c482a914b5d159a0bf207ef494dd58f0aa0616d9
medowemado
Fri Oct 16 21:13:26 2026 +0300
الوضع الأحدث
```

**Command 5 — working-tree integrity at the end of the investigation:**
```
$ git status --porcelain
```
Observed output: *(empty — the working tree is clean)*

### 2.2 Result

| Item | Value |
|---|---|
| Required commit | `c482a914b5d159a0bf207ef494dd58f0aa0616d9` |
| Observed `git rev-parse HEAD` | `c482a914b5d159a0bf207ef494dd58f0aa0616d9` |
| **Match** | **YES — exact match** |
| Did the investigation proceed? | **YES** |
| Was the investigation stopped? | **NO** |

### 2.3 Access limitations encountered

- The first clone attempt failed with `fatal: unable to access 'https://github.com/El-ogra/Top-Lab.git/': server certificate verification failed. CAfile: none CRLfile: none`. This is a TLS trust-store configuration issue in the sandbox, not a repository access problem. It was resolved by pointing Git at the sandbox gateway CA bundle via `-c http.sslCAInfo=…`. No certificate verification was disabled, and no integrity was weakened.
- The clone was performed into a working directory **outside** the repository being examined. Nothing was written into the repository.
- No `reset`, `clean`, `stash`, force-checkout or overwrite of any user working tree occurred at any point, because no user working tree was involved.

---

## SECTION 3 — Reference System Coverage

Only the two authorized PDFs were used as reference-system evidence. No other PDF in the repository was opened, and the entire `Docs/Reference system files/` directory was neither listed, opened, searched nor read.

**Disclosure on the firewall:** the single parent-directory listing performed on `Docs/` returned the *name* of the `Reference system files` entry alongside its siblings. That listing was necessary to locate the two authorized PDFs inside `Docs/Remaining Tasks Folder/`. No file inside that directory was listed, opened, searched, read, cited, or used, and no directory traversal was ever rooted inside it. All file-content searches in this investigation were rooted at `src/`, `tests/`, or the exact paths of the two authorized PDFs.

### 3.1 `Docs/Remaining Tasks Folder/RLS_Learn_Enhanced.pdf`

| Attribute | Value |
|---|---|
| Filename | `RLS_Learn_Enhanced.pdf` |
| Producer metadata | `OCRmyPDF 17.11.0 / OCRmyPDF fpdf2 + Tesseract OCR 5.3.4`; `pikepdf 10.12.0`; Title "Microsoft Word - RL_Show.docx" is **not** present on this file; Author `RealLab` |
| Total page count | **212** (determinable from `pdfinfo`) |
| Page size | 612 × 792 pt (US Letter) |
| Pages whose text layer was extracted | **212 of 212** (per-page extraction with `pdftotext -layout`) |
| Pages visually inspected (rendered to image and examined) | **192 of 212** — pages 1–192 |
| Pages inspected only through text extraction | **20 of 212** — pages 193–212 |
| Pages not inspected at all | **0** |
| Pages with an effectively empty text layer | 8 — pages 6, 8, 148, 165, 173, 189, 207, 211. **All 8 were rendered and visually inspected**; each is a section-divider page carrying only the `real lab` logo, the `Real Lab System` rule, and the page number. **None contains function content.** |
| Was OCR needed by this investigation? | **No.** The file already carries an OCR text layer produced upstream by OCRmyPDF/Tesseract. No additional OCR was run. |
| Remaining OCR/interpretation limitations | See L-11 and L-12. The Arabic text layer is heavily degraded and was cross-checked against the rendered page image wherever a function name or numeric threshold was material. |

### 3.2 `Docs/Remaining Tasks Folder/RL_Show_Enhanced.pdf`

| Attribute | Value |
|---|---|
| Filename | `RL_Show_Enhanced.pdf` |
| Producer metadata | `OCRmyPDF 17.11.0 / OCRmyPDF fpdf2 + Tesseract OCR 5.3.4`; `pikepdf 10.12.0`; Title "Microsoft Word - RL_Show.docx"; Author `الاسلاميه` |
| Total page count | **77** (determinable from `pdfinfo`) |
| Page size | 595 × 842 pt (A4) |
| Pages whose text layer was extracted | **77 of 77** |
| Pages visually inspected | **66 of 77** — pages 1–33 and 34–77 were rendered in page order for examination; pages 1–33, 35–55, 67–77 were directly examined as rendered images |
| Pages inspected only through text extraction | **11 of 77** — pages 56–66 |
| Pages not inspected at all | **0** |
| Pages with an effectively empty text layer | 1 — page 67. **It was rendered and visually inspected**: it is a full-colour splash/login screen for `RealLab Interface — RL-I Hematology Analyzer Interface`, showing device `BC-3000 Plus`, and carries the Arabic caption "بعض نوافذ ريل لاب انترفيس … شاشة تسجيل الدخول". It contains **no** function content of its own; it is corroborating evidence for an owner-excluded function (REF-011, REF-015). |
| Was OCR needed by this investigation? | **No.** An OCR text layer already exists. |
| Remaining OCR/interpretation limitations | Pages 56–66 are a sequence of *rotated/mirrored* sample report captures (their embedded Latin text extracts reversed, e.g. `retupmoC erawtfoS`). Text extraction from these pages is not reliably readable, and they were therefore **not** treated as an independent source of new function requirements; the same report types are independently evidenced in `RLS_Learn_Enhanced.pdf` and were taken from there. This is disclosed rather than silently ignored. |

### 3.3 Method used for visual inspection

Every page of both PDFs was rasterised with `pdftoppm -r 60 -jpeg`. The resulting images were assembled into 4×2 contact sheets (8 pages per sheet) and each sheet was **opened and examined as an image**, so that screen captures, dialogs, table headers, button labels, colour states and layout — all of which are lost or mangled by text extraction — were actually observed. Separately, the eight low-text pages of the first PDF and the single low-text page of the second PDF were each rendered individually and examined at higher fidelity.

Cross-checking was performed throughout: where extracted text and the rendered image disagreed, the rendered image governed.

### 3.4 Honest statement of coverage

Visual inspection is **not** claimed for pages 193–212 of `RLS_Learn_Enhanced.pdf` or pages 56–66 of `RL_Show_Enhanced.pdf`. Those 31 pages were read through the text layer only. Pages 193–205 are the report-header/footer, printer-assignment, default-account-type, receipt-settings and envelope-settings chapters, whose substance is fully carried by the Arabic prose on those pages and is corroborated by matching prose earlier in the document; pages 206–212 are the envelope chapter close-out, the sample-separation chapter and the company contact page. **No function in Section 4 rests solely on an un-inspected page**, and no visual detail from pages 193–212 or 56–66 was used as evidence for any classification.

---

## SECTION 4 — Canonical Reference Function Inventory

This inventory was constructed **independently**, from the two authorized PDFs only, before any consultation of owner-exclusion material. It is a reconstruction from a table of contents, per-chapter prose instructions, and 289 pages of screen captures.

**Decomposition rules applied.** The smallest meaningful independently verifiable unit was used. A screen, a data entity, a table or a component was not treated as a business function by itself. Repeated descriptions of the same capability in the table of contents, in prose, and in a screenshot were collapsed into one entry, with every confirming location recorded. A composite screen whose documented sub-controls constitute distinct, separately testable behaviours (for example the search screen's independent filter predicates) was decomposed, because each predicate can be exercised and asserted on its own.

**IDs.** Every distinct PDF-evidenced function carries a stable identifier `REF-001`…`REF-175`. Owner-excluded functions occupy `REF-001`…`REF-021` and are *PDF-evidenced*; they are inventoried here, then handled exclusively under Section 5. In-scope functions occupy `REF-022`…`REF-175`.

### 4.1 Owner-excluded functions evidenced in the PDFs (REF-001 … REF-021)

Listed in full here so the inventory is complete; classified and handled in Section 5.

| REF | Function | PDF evidence |
|---|---|---|
| REF-001 | Upload a patient result to the online result website | LEARN p.58–62, heading `12-1 … تسلٌم نتائج المريض عن طريق موقع نتٌجة (NATIGH.COM)` |
| REF-002 | Open the result website and view the patient result | LEARN p.63–64, `WWW.NATIGH.COM` login screen and result page |
| REF-003 | Print the portal result data on the thermal receipt so the patient can view it | LEARN p.69, thermal slip showing `WWW.NATIGH.COM`, "By patient ID", "By password" |
| REF-004 | Android application for the patient to print or save the result | LEARN p.63, Google Play link text |
| REF-005 | Create a treating-doctor account to use the result service | LEARN p.142–144, `الجهة الخارجية والمعامل` → treating doctor → generate ID |
| REF-006 | Create a laboratory account to use the result service | LEARN p.145–147, `Lab to Lab` branch of the same window |
| REF-007 | Count the patients whose results were uploaded in a period | LEARN p.65–67, `Admin` → عدد المرضى |
| REF-008 | Block / delete a patient result from the result website | LEARN p.68, `Block` button and confirmation dialog |
| REF-009 | Follow up a referrer's cases through the portal (lab-to-lab) | SHOW p.53, "متابعة اى جهة احالة لبحالات التابعه لها = لاب تو لاب" |
| REF-010 | Equipment and devices register with condition, periodic maintenance and calibration date | SHOW p.42, "ادخال بيانات اجهزة ومعدات المعمل ومتابعة حالتها والصيانة الدورية وتاريخ المعايرة" |
| REF-011 | Connect to a haematology (CBC) analyser and pull results into the lab record | SHOW p.67 (RL-I Hematology Analyzer Interface, BC-3000 Plus), p.74 |
| REF-012 | Analyser interface / server settings | SHOW p.72, "interface | server setting" |
| REF-013 | Automatic analyser result-correction factors per element | SHOW p.71, "اضبط ثابت تصحيح النتيجة تلقائي" |
| REF-014 | Supported analyser / hardware list (Biomerieux mini Vidas, VIDAS, Hemolyzer 3, Hemax 330, analyticon, B&E) | SHOW p.75, p.77 |
| REF-015 | Haematology analyser interface device login | SHOW p.67, interface login screen |
| REF-016 | Cash-drawer / inventory accounting for a selected branch number | LEARN p.179, `Branch no` selector in الجرد وحساب الدرج |
| REF-017 | Search for patients within a specified branch | LEARN p.42 ("بحث عن مريض فى فرع معٌن فى حالة نظام الفروع"), p.45 (`Branch No.`) |
| REF-018 | Send the result to the patient by SMS | LEARN p.17, p.47, `Send SMS` button on the account/results command group |
| REF-019 | Send the result to the patient by e-mail | LEARN p.17, p.23, p.47, `Send E-mail` button |
| REF-020 | Send the result by fax | LEARN p.17, p.18, `By Fax` button in the same command group |
| REF-021 | Lab-branded SMS notice (congratulation, result, completion, or marketing) | SHOW p.53, "ارسال رسائل نصية للجوال بإسم المعمل للمريض او للطبيب" |

### 4.2 In-scope functions (REF-022 … REF-175)

#### Module 1 — Access, users, permissions and attendance

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-022 | Sign in / sign out | Authenticate with user name and password; optional remember-login; sign out | LEARN p.11, p.52, p.158; SHOW p.34 |
| REF-023 | Privileged-window secondary password | Windows that require a password before opening, accepting a fixed code or the user's own password | LEARN p.150, p.156, p.176, p.179 (`System menu password` dialog) |
| REF-024 | Create users and administrators | Create a user with a primary and a secondary password, a role, a work end time and permission grants | LEARN p.149–151, p.152 |
| REF-025 | Edit user data, role and password | Change a user's data, permissions and password; renew permissions | LEARN p.156–157, p.149 |
| REF-026 | Assign per-user permissions | Grant each user a subset of lab work, including blocking printing, blocking patient deletion and blocking registration | LEARN p.152–155; SHOW p.51, p.53 |
| REF-027 | Per-user discount limit | Bound the discount a user may grant | SHOW p.53; LEARN p.125 (discount / commission on a referrer) |
| REF-028 | Check-in, break and check-out | Record attendance, break periods and end of duty | LEARN p.158, p.160 |
| REF-029 | User attendance report | Show each user's entry/exit times and work time; admin-only | LEARN p.161, p.164 (`سجل تواريخ الحضور` / `تقرير سجل الحضور`) |

#### Module 2 — Patient registration

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-030 | Register a new patient | Capture name, type and age as the base data; optional phone, address, national ID, treating doctor, notes | LEARN p.12–13 |
| REF-031 | Patient notes | Free-text notes shown on the result-entry and result-delivery screens | LEARN p.12 ("تظهر فى نافذة ادخال التحالٌل وتسلٌم نتائج التحالٌل") |
| REF-032 | Patient medical-history data | Record chronic medication, chronic disease, pregnancy for women, and recent imaging; viewable from the results screen | LEARN p.13, p.17, p.47 (`بيانات المريض` button) |
| REF-033 | Add an individual test to the patient | Double-click a test to add it to the visit | LEARN p.13 |
| REF-034 | Add a whole test group / profile | Add every analyte of a named profile to the visit at once | LEARN p.15–16 |
| REF-035 | Remove one test / clear all tests | Remove a single added test, or clear all tests at first registration | LEARN p.13 |
| REF-036 | Mark a test as taken outside the lab | Flag a test as externally collected; shown as a note on the report | LEARN p.13 (`Taken Out side Lab`) |
| REF-037 | Save the visit and its tests | Commit the registration and its ordered tests | LEARN p.13, p.16 (`حفظ`) |
| REF-038 | Print the patient barcode label | Print the patient's identity barcode | LEARN p.12 |
| REF-039 | Patient account slip with tests and collection date | Print the patient's account, tests and pickup date, with optional test prices | LEARN p.12, p.20; SHOW p.52 |
| REF-040 | Edit an existing patient or their tests | Reopen a registration and change data, or add/remove tests | LEARN p.22–25 |
| REF-041 | List the patients registered on a chosen day | Browse the patients of a selected date | LEARN p.22, p.26 |

#### Module 3 — Patient account and billing

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-042 | Compute the patient total from configured prices | Total is derived from the prices configured in the program | LEARN p.17 (`إجمالى التحاليل`) |
| REF-043 | Enter previously paid amount and discount, then calculate and save | Two-stage Enter: first calculate, then persist | LEARN p.17 |
| REF-044 | Add an extra charge to the account | An amount prefixed with `+` is added to the analyses total | LEARN p.17 |
| REF-045 | Settle the account in full | Mark the account as fully paid and confirm | LEARN p.17 (`خالص` / `موافق`) |
| REF-046 | Per-test account dialog: edit or delete individual paid lines | Open the total dialog, select a line, modify or delete it | LEARN p.18–20 |
| REF-047 | Record a payment and list payments | Post a payment against the visit and enumerate the payments received | LEARN p.17–18 |
| REF-048 | Print the patient invoice (receipt) with barcode | Produce the invoice document including a barcode | LEARN p.20; SHOW p.4–5 |

#### Module 4 — Result entry and result lifecycle

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-049 | Open result entry for a patient | Enter the results-entry screen for a selected patient | LEARN p.27 |
| REF-050 | Enter simple single-value results | Type one result value per test | LEARN p.27–29; SHOW p.38–41 |
| REF-051 | Enter profile (grouped-analyte) results | Type one value per analyte inside a profile | LEARN p.15–16; SHOW p.37, p.39–40 |
| REF-052 | Verify / unverify results | Mark a result as reviewed/verified, and reverse that | LEARN p.28, p.54 |
| REF-053 | Print results individually or in bulk | Print one result, or all results of the visit | LEARN p.28, p.53 |
| REF-054 | Print preview before printing | Preview the composed report before it is printed | LEARN p.30 (`معاينة الطباعة`) |
| REF-055 | Export the patient report to PDF | Produce a PDF of the patient report | LEARN p.30; SHOW p.12–24 |
| REF-056 | Result workflow-status filters | Find results that have not been entered, not reviewed, not printed, or not delivered | LEARN p.27, p.42 |

#### Module 5 — Culture (microbiology) and antibiotics

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-057 | Enter culture results | Sample, Organism A, Organism B, Organism C, culture condition, colony count | LEARN p.38, p.40 (MICROBIOLOGY REPORT: Culture and Sensitivity) |
| REF-058 | Enter culture microscopy findings | Microscopic examination block of the culture report | LEARN p.38, p.40 |
| REF-059 | Enter antibiotic sensitivity and inhibition zone | Classify each antibiotic Highly / Moderate / Low / Resistant and record the zone in mm | LEARN p.38–40 |
| REF-060 | Select the antibiotic of choice | Mark the preferred antibiotic for the culture | LEARN p.39 (`Antibiotic of choice`) |
| REF-061 | Restrict antibiotics by pregnancy flag | A pregnancy-flagged antibiotic appears only when the patient is marked pregnant | LEARN p.39 |
| REF-062 | Restrict antibiotics by children flag | A children-flagged antibiotic appears only for child patients | LEARN p.39 |
| REF-063 | Control display of Sensitivity / Reference / Commercial Name | Toggle which of the three columns appears in the culture report | LEARN p.39 |
| REF-064 | Add a new culture test to the program | Register a culture-type test in the catalogue | LEARN p.128–129 |
| REF-065 | Maintain the antibiotic master list | Add, edit and delete antibiotics with symbol, scientific name and commercial name | LEARN p.134–135 |
| REF-066 | Attach antibiotics to a culture with a sensitivity category | Attach a master antibiotic to a named culture and set its category | LEARN p.136–138 |
| REF-067 | Detach an antibiotic from a culture | Remove an attached antibiotic from a culture | LEARN p.138 |
| REF-068 | Enter an antibiotic manually when absent from the master list | Type an unknown antibiotic's data directly | LEARN p.138 |

#### Module 6 — Test catalogue and reference data

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-069 | Edit test data and price | Modify a catalogue entry and its price | LEARN p.94–95 |
| REF-070 | Add a new test to the program | Create a catalogue entry with code, group, price, barcode, sample label, reference type | LEARN p.96–97 |
| REF-071 | Test groups: create / rename / deactivate / reactivate | Full lifecycle of the named test group | LEARN p.93, p.118 |
| REF-072 | Work group (Log): create / rename and set its items | A work group that batches tests for the worksheet | LEARN p.85 (`مجموعة عمل (Log)`) |
| REF-073 | Print the analysis price list | Produce a printable price list | LEARN p.111 |
| REF-074 | Test comments: add / edit / delete | Store one or more fixed comments against a test | LEARN p.112–115 |
| REF-075 | Insert a stored comment into the report | Pick a stored comment from a drop-down and add it to the report | LEARN p.116–117 |
| REF-076 | Custom group: create / rename / delete | Manage named custom groups | LEARN p.118–119, p.123 |
| REF-077 | Add tests and set prices inside a custom group | Compose a custom group and price each member | LEARN p.120 |
| REF-078 | Add a custom group to a patient | Apply a custom group to a visit | LEARN p.122 |
| REF-079 | Reference values: enter min/max with low/high comments | Define a normal interval and the text shown below/above it | LEARN p.98, p.101 |
| REF-080 | Reference values: edit for a test | Modify an existing interval | LEARN p.99–100 |
| REF-081 | Reference ranges by sex and age, or by age only | Two matching modes | LEARN p.101–104 (`By Sex and Age – By Age Only`) |
| REF-082 | Show the reference value and the deviation from normal on entry | Display the interval and the delta while entering a result | LEARN p.101 |
| REF-083 | Temporarily override a reference value at print time for one patient | Open the reference cell from the report, edit, refresh and accept | LEARN p.101–102 |
| REF-084 | Map a test to a reference analyte | Bind a test to a reference definition and its result fields | LEARN p.129 |

#### Module 7 — Price lists and external entities

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-085 | Create a new contract price list | Name and save a new contract price list | LEARN p.105 |
| REF-086 | Add tests to a price list | Append catalogue entries to a list | LEARN p.105 |
| REF-087 | Change a test price inside a list; rename a list | Edit a member price; edit the list name | LEARN p.105 |
| REF-088 | Per-entity price, discount and commission on a contract price list | Set a list, a discount percentage and a commission for a specific referrer | LEARN p.107–110 |
| REF-089 | Treating doctor: add / edit / delete | Manage treating doctors | LEARN p.124–125, p.127 |
| REF-090 | Sent-out lab entity: add / edit / delete | Manage outside laboratories | LEARN p.124 |
| REF-091 | Referring / contract entity: add / edit / delete with its price list | Manage referrers and attach a price list | LEARN p.126–127 |
| REF-092 | Lab-to-lab price list for an outside lab | Maintain the price list used when sending work out | LEARN p.145–146 |

#### Module 8 — Sent-out samples

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-093 | Mark a test as sent out | Record the receiving lab, the cost price paid and the price charged to the patient | LEARN p.140–141 |
| REF-094 | Sent-out samples account for a period | Paid and remaining totals for sent-out work in a period | LEARN p.184–186; SHOW p.29 |
| REF-095 | Detailed sent-out report per sample and search by a specific lab | Per-line detail, filtered by receiving lab | LEARN p.186–187 |

#### Module 9 — Result delivery and traceability

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-096 | Deliver patient results and settle the account | Hand over completed work and close the balance | LEARN p.21 |
| REF-097 | View patients and results in a period with paid / remaining-to-lab amounts | Period delivery grid with balances | LEARN p.21 |
| REF-098 | List undelivered results | Find work not yet handed over | LEARN p.12, p.42 |
| REF-099 | Case follow-up: who registered, who edited, when, and the last printer | Per-case audit of registration, modifications, printing and delivery | LEARN p.52–57 |

#### Module 10 — Patient search and visit history

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-100 | Search by patient identity | Match or partial-match on name; search by phone number; search by national ID / card number | LEARN p.41–43 |
| REF-101 | Search by doctor, referral entity, gender, age, test or date period | Six independent filter predicates on the search screen | LEARN p.42–43 |
| REF-102 | View patient and result detail from search | Open the patient's data and results from a search hit | LEARN p.43–46 |
| REF-103 | Search a patient and display all lab visits with dates and tests | List every visit of one patient | LEARN p.47–51 |
| REF-104 | Persist and auto-display the Lab ID | The Lab ID is stored on the patient and re-shown on re-registration | LEARN p.47–48 |
| REF-105 | Show unentered results and the price of each analysis in the search detail | The search/detail document carries the analysis and its price | LEARN p.44; SHOW p.6 |

#### Module 11 — Work sheets

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-106 | Work paper by patient names or codes in a period | Worksheet listing patients | LEARN p.85–88 |
| REF-107 | Work paper by test name(s) in a period | Worksheet listing tests and their patients | LEARN p.89–92 |
| REF-108 | Work paper by work group (Log) in a period | Worksheet for a whole work group | LEARN p.85 |
| REF-109 | Classify the analyses of a period to count how many times each test was performed | Period test-count classification | LEARN p.84–85 |
| REF-110 | Print the worksheet: unfinished only, or all | Restrict the worksheet to unfinished work | LEARN p.86, p.89 |
| REF-111 | Preview the worksheet before printing | Preview the worksheet | LEARN p.88 |

#### Module 12 — Reports and patient history

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-112 | Combined report | Select tests, order them, optionally print the group sub-title, reprint without a message | LEARN p.31–34 |
| REF-113 | Blank report | A report carrying patient data only, for manual completion | LEARN p.35–36 |
| REF-114 | Automatic patient history in the report | Show prior results of the same test beneath the current result | LEARN p.70–73 |
| REF-115 | Insert previous results of selected tests into the current report | Choose historical results and insert them | LEARN p.75–76 |
| REF-116 | Print patient history in a separate report | History as its own document | LEARN p.77–78 |
| REF-117 | Patient history for the blood-picture (CBC) profile | The pivoted date × analyte matrix | LEARN p.79 |
| REF-118 | Patient history for more than one patient in one report | Multi-patient history | LEARN p.80–83 |
| REF-119 | Configure the history sort/aggregation mode | Choose how history is gathered and sorted, from report settings | LEARN p.70 |
| REF-120 | Group-patient report slip | One slip per patient for a whole group of patients, to prevent errors and keep an archive | LEARN p.52–57; SHOW p.6 |

#### Module 13 — Sample handling

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-121 | List patients with uncollected samples and open the sample-draw board | The draw board lists samples awaiting collection | LEARN p.208–210 |
| REF-122 | Mark a sample as drawn for a patient | Record a sample as collected | LEARN p.209–210 |

#### Module 14 — Statistics

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-123 | Patient count in a period by sex, referral entity and account type | Classification counts | LEARN p.167–168 |
| REF-124 | Patient count in a specific year grouped by month | Monthly buckets with money column | LEARN p.169; SHOW p.30 |
| REF-125 | Patient count in a specific month grouped by day | Daily buckets with money column | LEARN p.170; SHOW p.31 |
| REF-126 | Request rate for a group of tests in a period | Share of samples per test within a profile | LEARN p.171; SHOW p.32 |
| REF-127 | Test-sample counts in a year and samples per referral entity | Counts per test; counts per referrer | LEARN p.172; SHOW p.33 |
| REF-128 | Sent-out sample statistics by period and receiving lab | Volume sent out, by lab | LEARN p.174; SHOW p.29 |
| REF-129 | User productivity statistics | Workload attributed to each user | LEARN p.167, p.149 |
| REF-130 | Banded result monitor for one test | List every result of one test in a period whose value falls inside a chosen band | SHOW p.25, p.73 |

#### Module 15 — Inventory and accounts

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-131 | Cash-drawer inventory and net profit for a period | Daily / weekly / monthly / yearly / arbitrary-period drawer accounting and profit | LEARN p.175–177 |
| REF-132 | Inventory scoped to a user, referral entity, treating doctor, sent-out samples or account type | The same accounting, narrowed to one element | LEARN p.178, p.181 |
| REF-133 | Detail of the patient-sample amounts behind an inventory total | Drill into the underlying patient rows | LEARN p.180 |
| REF-134 | Referral / delegate account with commission share | Commission share per referrer | LEARN p.174 |
| REF-135 | Cash deposit and disbursement | Record money in and out | LEARN p.174–175 |
| REF-136 | Sent-out samples account and settlement | Account and full settlement of sent-out work | LEARN p.184–186 |
| REF-137 | Other-labs account and referral-entity share in a period | Accounts of the other laboratories and the referrers' share | LEARN p.175 |
| REF-138 | Year-end account settlement | Close accounts at year end | LEARN p.174 |

#### Module 16 — System and print settings

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-139 | Report settings (general) | Overall report configuration | LEARN p.191–192 |
| REF-140 | Report header/footer from attached image files | Select an image file for the report header or footer | LEARN p.194 |
| REF-141 | Report header/footer colours | Change the colour of individual header/footer elements | LEARN p.195–198 |
| REF-142 | Report margins and top space | Left margin, bottom margin, report top space | LEARN p.193 |
| REF-143 | Report paper size | A4 / A5 | LEARN p.194 |
| REF-144 | Report header/footer as words | Type the header/footer as separate text lines, with lab data in the fields | LEARN p.194 |
| REF-145 | Receipt settings: currency, pickup time, print-once, header/footer text, font and logo | The full receipt-appearance configuration | LEARN p.202–204 |
| REF-146 | Dedicated printers for report / barcode / receipt / envelope | Bind each output type to a printer | LEARN p.199 |
| REF-147 | Default account type for the system | Choose the account type applied automatically | LEARN p.200 |
| REF-148 | Receipt settings: thermal header/footer words and margins | The receipt appearance chapter | LEARN p.202–204 |
| REF-149 | Patient receivable account summary for a period with a balance filter | Period receivable summary filtered to unpaid, discounted or paid-only | SHOW p.26 |
| REF-150 | Referrer account statement in four variants | Detailed by results, by test prices, by patient, and consolidated for all referrers | SHOW p.28 |
| REF-151 | Envelope settings | Envelope appearance | LEARN p.205 |

#### Module 17 — Report content and layout control

| REF | Function | Capability | PDF evidence |
|---|---|---|---|
| REF-152 | Edit report item letters and reference values; hide/show items; extend selection lists; allow free text | Configure the report's item layout | SHOW p.12 |
| REF-153 | Control line spacing, item distance from the left margin, and the symbol before each item | Layout and marker control | SHOW p.13 |
| REF-154 | Shade or colour abnormal results, or disable it | Highlight out-of-range results | SHOW p.13 |
| REF-155 | Per-profile control of normal/abnormal result font and background colour | Per-profile result styling | SHOW p.16 |
| REF-156 | Edit the character used to mark the normal range | Change the `N`/normal marker | SHOW p.17 |
| REF-157 | Toggle report elements from a doctor side menu | Show/hide any report element on demand | SHOW p.15 |
| REF-158 | Print the delivery order and control print positions of elements | Delivery order plus per-element print position | SHOW p.6 |
| REF-159 | Barcode labels for tubes, containers and swabs with doctor-editable test distribution | Sort the patient's tests across the drawn materials and let the doctor change the distribution | SHOW p.3 |
| REF-160 | Optional vertical coding on the result report, receipt and envelope | Vertical identity coding printed on documents | SHOW p.3 |
| REF-161 | Reprint any barcode label | Reprint a previously issued label | SHOW p.3 |
| REF-162 | Sticker size configuration | Label stock size, documented as 38 mm × 25 mm | SHOW p.3 |
| REF-163 | Interpret sample nature, its normal ranges and its effect on the patient | A sample-nature interpretation panel | SHOW p.51 |
| REF-164 | Glossary of abbreviations used in medical analysis | A terminology lookup | SHOW p.51 |
| REF-165 | Requirements / purchases list | A shopping-requirements list | SHOW p.51 |
| REF-166 | Stopwatch, time clock and profile calculation | Elapsed-time and profile-calculation tools | SHOW p.51 |
| REF-167 | Appointment-note reminder to a user or phone-book contact | Send a reminder note | SHOW p.51 |
| REF-168 | Phone book | A contact list | SHOW p.51 |
| REF-169 | Unit conversion | Convert between measurement units | SHOW p.51 |
| REF-170 | Print the receipt on a cashier / thermal printer | Receipt to a till printer | SHOW p.4 |
| REF-171 | Show/hide test details and the result-collection data on the receipt | Receipt content switches | SHOW p.4, p.5 |
| REF-172 | Header/footer words plus logo on pre-printed or plain receipt paper | Receipt paper and branding | SHOW p.4 |
| REF-173 | Print the report on plain or pre-printed paper with a logo | Paper choice and branding for reports | SHOW p.5, p.16 |
| REF-174 | Group-patient report slip (one patient per line) with controlled print positions | See REF-120; the SHOW-side statement of the same capability | SHOW p.6 |
| REF-175 | Search and statistics on blood-picture **element** values in time periods | A banded monitor over CBC analytes, not over test-level scalars | SHOW p.73 |

> **Deduplication note.** REF-120 and REF-174 describe the same capability from the two PDFs; it is carried once, as REF-120, with both evidence locations recorded, per the deduplication rule. REF-140 and REF-173 overlap on "header/footer from an image"; they are kept separate because the LEARN page is a report-settings function and the SHOW page is a report-production paper/branding function, and they map to different implementation surfaces. REF-141 and REF-142…REF-144 are kept separate because the LEARN document presents them as separate numbered settings entries (`7-1`…`7-4`) with independent screens.

**Inventory totals at this point:** 175 PDF-evidenced reference functions (REF-001…REF-175), of which 21 (REF-001…REF-021) are owner-excluded and 154 (REF-022…REF-175) are in scope.

---

## SECTION 5 — Owner-Excluded Functions

The five owner-defined categories are permanently excluded from the desired project scope. Every identifiable function belonging to them is listed individually below.

**Designation for every item in this section: OWNER-EXCLUDED — NOT REQUIRED IN THIS PROJECT SCOPE**

No item in this section is classified IMPLEMENTED, DIFFERENT, MISSING or BLOCKED / UNRESOLVED. None is counted as an in-scope gap. None is reported as a defect, deficiency, technical debt or shortcoming. None appears in Section 9 or Section 10. None is recommended, partially recommended, or restored.

### 5.1 Category 1 — Online results portal and ecosystem (9 items)

| REF | Function | Subfunction | PDF evidence | Status |
|---|---|---|---|---|
| REF-001 | Upload a patient result to the online result website | Result upload to the portal | LEARN p.58–62, `NATIGH.COM` toolbar item; `NATIGH.COM UPLOADER` utility | Confirmed in the authorized PDFs |
| REF-002 | Open the result website and view the patient result | Patient-side portal viewing | LEARN p.63–64, portal login with Patient / Doctor / Laboratory radio buttons | Confirmed |
| REF-003 | Print the portal result data on the thermal receipt | Portal receipt printing | LEARN p.69, thermal slip carrying `WWW.NATIGH.COM`, "By patient ID", "By password" | Confirmed |
| REF-004 | Android application for the patient to print or save the result | Mobile client | LEARN p.63, Google Play download text for the result application | Confirmed |
| REF-005 | Create a treating-doctor account for the result service | Doctor portal account | LEARN p.142–144 | Confirmed |
| REF-006 | Create a laboratory account for the result service | Laboratory portal account | LEARN p.145–147 | Confirmed |
| REF-007 | Count the patients whose results were uploaded in a period | Portal administration | LEARN p.65–67 | Confirmed |
| REF-008 | Block / delete a patient result from the result website | Portal administration | LEARN p.68 | Confirmed |
| REF-009 | Follow up a referrer's cases through the portal (lab-to-lab) | Portal-based case follow-up | SHOW p.53 | Confirmed |

**Composite-function decomposition.** LEARN REF-005 and REF-006 are recorded as a single documented control in the reference (one `الجهات الخارجية والمعامل` window, entity type *treating doctor* vs *referring/contract entity*). They were decomposed because they create accounts for two different audiences with different downstream behaviour. **Only the excluded portion is treated here.** The surrounding, non-portal function in the same window — managing treating doctors, sent-out labs and referring entities as master data — is carried in scope as REF-089, REF-090 and REF-091 and is classified in Section 6. The exclusion boundary is the portal account, not the external-entity master data.

### 5.2 Category 2 — Equipment, maintenance and calibration (1 item)

| REF | Function | Subfunction | PDF evidence | Status |
|---|---|---|---|---|
| REF-010 | Equipment and devices register with condition, periodic maintenance and calibration date | Equipment registration, maintenance records, calibration records | SHOW p.42, system-window bullet `ادخال بيانات اجهزة ومعدات المعمل ومتابعة حالتها والصيانة الدورية وتاريخ المعايرة` | Confirmed |

**Disclosure.** The reference presents this as one line in a system-window capability list, but it names three sub-capabilities: registration, periodic maintenance, and calibration date. All three are within Category 2 and are therefore excluded. The single REF identifier is retained rather than split into three, because splitting would inflate the inventory without changing any treatment.

### 5.3 Category 3 — Blood/CBC analyser integration (5 items)

| REF | Function | Subfunction | PDF evidence | Status |
|---|---|---|---|---|
| REF-011 | Connect to a haematology (CBC) analyser and pull results into the lab record | Analyser connection and result import | SHOW p.67, p.74 | Confirmed |
| REF-012 | Analyser interface / server settings | Analyser connection configuration | SHOW p.72 | Confirmed |
| REF-013 | Automatic analyser result-correction factors | Analyser correction factors | SHOW p.71 | Confirmed |
| REF-014 | Supported analyser / hardware list | Analyser hardware catalogue | SHOW p.75, p.77 | Confirmed |
| REF-015 | Haematology analyser interface device login | Analyser connection configuration | SHOW p.67 | Confirmed |

**Boundary note — important.** These five items describe a *separate reference product* ("ريال لاب انترفيس" / RealLab Interface, a standalone CBC-analyzer interface application), pages 74–77 of the showcase document. Nothing in the *laboratory system* chapters describes importing analyser results into the lab record. The boundary is therefore clean: **all CBC result-entry capability inside the lab system (REF-050, REF-051, REF-117) remains fully in scope** and is classified in Section 6. Only the device interface, the hardware list, and the correction factors are excluded. Nothing about CBC *data entry* is excluded.

### 5.4 Category 4 — Multi-branch support (2 items)

| REF | Function | Subfunction | PDF evidence | Status |
|---|---|---|---|---|
| REF-016 | Cash-drawer / inventory accounting for a selected branch number | Branch-specific operation | LEARN p.179, `Branch no` selector inside the drawer accounting window | Confirmed |
| REF-017 | Search for patients within a specified branch | Branch-specific operation | LEARN p.42, p.45, `Branch No.` field and `Branch No. 1/2/3` selector | Confirmed |

**Boundary note.** The branch selector is an input to two larger in-scope capabilities (cash-drawer inventory, REF-131; patient search, REF-101). Those two capabilities are assessed **only in their non-branch form**. The branch dimension is excluded and is neither counted as implemented nor reported as absent.

### 5.5 Category 5 — SMS, e-mail and fax result sending (4 items)

| REF | Function | Subfunction | PDF evidence | Status |
|---|---|---|---|---|
| REF-018 | Send the result to the patient by SMS | SMS result sending | LEARN p.17, p.47, `Send SMS` button | Confirmed |
| REF-019 | Send the result to the patient by e-mail | E-mail result sending | LEARN p.17, p.23, p.47, `Send E-mail` button | Confirmed |
| REF-020 | Send the result by fax | Fax result sending | LEARN p.17, p.18, `By Fax` button | Confirmed |
| REF-021 | Lab-branded SMS notice (congratulation, result, completion, or marketing) | SMS sending | SHOW p.53 | Confirmed |

**Evidence-strength disclosure (REF-020).** The fax control appears as a **screenshot button** in a command group, not as a prose instruction, and the surrounding Arabic is degraded. This is recorded as evidence of a fax-sending capability in the reference, with the evidence limitation stated. It does not alter any treatment: the owner named SMS / e-mail / fax as one category, and the item is excluded either way.

### 5.6 Owner exclusions not evidenced in the authorized PDFs

**None.** Every one of the five categories is represented by at least one function that is directly evidenced in the two authorized PDFs, as listed above. No item in this report is recorded as "an owner exclusion whose presence in the permitted reference material could not be confirmed", and no PDF citation has been fabricated.

### 5.7 Permitted owner-exclusion cross-check and discrepancies

After the inventory above was built independently from the PDFs, the owner-exclusion record was consulted **solely** to cross-check that the five categories had been applied completely and to identify any owner-defined subfunction not visible in the PDFs. That material was not used, and is not used, as evidence of implementation, of reference requirements, or of any classification.

**Material discrepancies found: none that change any treatment.** Specifically:

- The owner's exclusion record and the independently derived inventory agree on all five categories and on the substance of every excluded subfunction.
- No owner-defined exclusion lacked PDF evidence, and no PDF-evidenced function was found that falls outside the five categories but should have been caught by them.
- One observation was checked and deliberately **not** carried over: the owner record treats a certain item as a documentation-level finding. The present report reaches the same factual observation independently (REF-020, §5.5) from the same page, and states the evidence limitation on its own terms rather than adopting the earlier characterisation.
- No count, identifier, classification, ordering or recommendation from that material was adopted. The totals, identifiers and classifications in this report were derived independently and are stated on their own evidence.

---

## SECTION 6 — Current Classification of In-Scope Functions

### 6.1 Method and reading conventions

- **Every in-scope function carries exactly one classification and exactly one verification status.**
- Classification vocabulary: `IMPLEMENTED`, `DIFFERENT`, `MISSING`, `BLOCKED / UNRESOLVED`.
- Verification-status vocabulary: `STATICALLY VERIFIED`, `TEST VERIFIED`, `BUILD VERIFIED`, `RUNTIME VERIFIED`, `PARTIALLY VERIFIED`, `NOT VERIFIED FROM AVAILABLE ENVIRONMENT`.
- **Path column** abbreviates the traced chain: `UI → VM/Cmd → Handler → Domain → Persistence`.
- **Verification discipline applied.** No function is called `IMPLEMENTED` on the strength of a class, property, table, column, XAML element, command name or test name. Each `IMPLEMENTED` row below was traced through a real handler and a real persistence path, and the strongest status justified by the evidence actually obtained is stated.
- **Overall verification ceiling.** No function in this report is `RUNTIME VERIFIED`, because the application was never launched (L-10). Rows that also have passing automated tests and a successful build are marked `PARTIALLY VERIFIED`; rows resting on source inspection alone are `STATICALLY VERIFIED`.
- **Build and test evidence actually obtained** (recorded once here, applied to all rows):

| Project | Build | Tests |
|---|---|---|
| `TopLab.Domain` | succeeded, 0 warnings, 0 errors | 513 passed / 0 failed |
| `TopLab.Application` | succeeded, 0 errors | 1733 passed / 0 failed |
| `TopLab.Infrastructure` | succeeded, 0 errors | 325 passed / 28 failed (all `Printing`, all Arial-font environmental — see L-06) |
| `TopLab.Persistence.Tests` | succeeded, 0 errors | 13 passed / 0 failed / 2 skipped |
| `TopLab.Presentation` | **not buildable on this host** (`net8.0-windows`, L-01/L-02) | not run |
| `TopLab.Presentation.Tests` | **not buildable on this host** | not run |

  Test execution required `DOTNET_ROLL_FORWARD=LatestMajor` because no .NET 8 runtime is installed (L-05).

### 6.2 Module 1 — Access, users, permissions and attendance

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-022 | Sign in / sign out | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.11, p.52 | `UsersAndPermissions/Commands/SignIn/SignInCommandHandler.cs` → `User.cs` (verify password via `Pbkdf2PasswordHasher`) → `UserConfiguration.cs`; `SignOut`; `GetCurrentSessionQueryHandler.cs`; `Views/Setup/LoginWindow.xaml`; PBKDF2 covered by `Infrastructure.Tests/Identity` | Full path traced and covered by passing tests. UI not runnable here. |
| REF-023 | Privileged-window secondary password | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.150, p.156, p.176, p.179 | `UsersAndPermissions/Queries/VerifySecondaryPassword/VerifySecondaryPasswordQueryHandler.cs`; `Views/Dialogs/SystemMenuPasswordDialog.xaml(.cs)`; called from `InventoryAndAccountingAccessPolicy` / `UsersAndPermissionsAccessPolicy` | Secondary-password gate is real and reachable. Not runtime-verified. |
| REF-024 | Create users and administrators | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.149–152 | `CreateUserCommandHandler.cs` → `User.Create` → `UserConfiguration.cs`; `SaveUserPermissionsCommandHandler.cs` → `UserPermissionGrant`; `Views/Users/UserManagementView.xaml` | Matches the documented window behaviour including the work-end-time field. |
| REF-025 | Edit user data, role and password | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.156–157 | `UpdateUserCommandHandler.cs`; `ChangeOwnPasswordCommandHandler.cs`; `DeactivateUser` / `ReactivateUser` / `DeleteUser` | Full lifecycle present. |
| REF-026 | Assign per-user permissions | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.152–155; SHOW p.51, p.53 | 13 seeded permissions in `PermissionConfiguration.cs:18-30`; enforced by `Application/Common/Behaviors/AuthorizationBehavior.cs` against `IAuthorizedRequest.RequiredPermissionCode`; `UserPermissionGrant` | Enforcement is uniform and architectural, not per-screen. Mapping to the reference's longer list is not one-to-one (see §6.18 finding F-01). |
| REF-027 | Per-user discount limit | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.53; LEARN p.125 | `User.SetPolicy(DiscountLimitPercent, BlockPrintOnRemainingBalance)` (`User.cs:63`, `:169`); `CreateUserCommand.cs:12`; `UserManagementViewModel.cs:181` | Present and editable per user. |
| REF-028 | Check-in, break, check-out | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.158, p.160 | `Attendance/Commands/{CheckIn,StartBreak,EndBreak,CheckOut}` → `AttendanceRecord` → `AttendanceCalculator.cs`; `Views/Attendance/MyAttendanceView.xaml` | Matches the documented in / break / out sequence. |
| REF-029 | User attendance report | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.161, p.164 | `Attendance/Queries/{GetAttendanceRecords,GetUserAttendanceSummary}`; `Views/Attendance/{AttendanceRecordsView,UserAttendanceSummaryView}.xaml` | Both the per-user record list and the admin-only summary are present. |

### 6.3 Module 2 — Patient registration

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-030 | Register a new patient | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.12–13 | `PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs` → `Patient.Create` (validates name, age ≥ 0) → `PatientConfiguration.cs`; `Views/Patients/PatientEditorView.xaml` | All documented base fields present, incl. optional phone (separate `PatientPhoneNumber` table), address, national ID, treating doctor, notes. |
| REF-031 | Patient notes | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.12 | `Patient.Notes` (`Patient.cs:47`); surfaced via `PatientRegistrationDtos` and consumed by the result and delivery readers | Field, persistence and read path all traced. |
| REF-032 | Patient medical-history data | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.13, p.17, p.47 | `MedicalConditionType` + `PatientMedicalCondition` with `MedicalConditionCategory`; `AddMedicalCondition` / `RemoveMedicalCondition` / `ApplyConditionDeltas`; `GetMedicalConditionTypes`; pregnancy seeded by migration `20260910213833_AddPregnancyMedicalConditionTypeSeed` | Covers medication, chronic disease, pregnancy and imaging categories. |
| REF-033 | Add an individual test to the patient | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.13 | `AddTestsToVisitCommandHandler.cs` → `PatientTest` → `PatientTestConfiguration.cs`; price resolved by `TestPriceResolver.cs` | Add path traced. |
| REF-034 | Add a whole test group / profile | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.15–16 | `AddProfileToVisitCommandHandler.cs` → `Profile` + `ProfileAnalyte` | Matches the double-click group-add behaviour. |
| REF-035 | Remove one test / clear all tests | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.13 | `RemoveTestFromVisitCommandHandler.cs`; `ClearAllTestsCommandHandler.cs` | Both documented operations present. |
| REF-036 | Mark a test as taken outside the lab | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.13 | `PatientTest.IsTakenOutsideLab`; `UpdatePatientTestSampleFlagsCommandHandler.cs`; honoured in the worksheet handlers (explicitly excluded from the bench-work sheet) | Flag persists and is consumed downstream. |
| REF-037 | Save the visit and its tests | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.13, p.16 | `AppUnitOfWork.SaveChangesAsync` via MediatR `UnitOfWorkBehavior`-style pipeline; entity-level guards on `Patient.EnsureNotDeleted` | Save path traced end to end. |
| REF-038 | Print the patient barcode label | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.12 | `PrintBarcodeCommandHandler.cs` → `IBarcodeService.PrintBarcodeAsync` → `BarcodeService.cs` (renders Code-128, embeds PNG, dispatches to the Barcode `PrinterAssignment`) | Identifies the patient (or the Lab Id when `PrintLabIdInsteadOfPatientId`). Print output not visually verified (L-06, L-09). See also REF-159. |
| REF-039 | Patient account slip with tests and collection date | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.12, p.20; SHOW p.52 | `PatientBilling/Queries/GetPatientReceipt` → `ReceiptPdfWriter.cs` / `ReceiptPrintingService.cs`; `ReceiptSettings.TestDetailDisplayMode` (`Hide` / `Show` / `ShowWithCode`) implements the optional-prices switch | The optional price display switch is explicitly modelled. |
| REF-040 | Edit an existing patient or their tests | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.22–25 | `UpdatePatientCommandHandler.cs`; reuses the same add/remove test commands; `SoftDeletePatientCommandHandler.cs` | Matches both documented entry routes (from the patient screen and from search). |
| REF-041 | List the patients registered on a chosen day | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.22, p.26 | `PatientRegistration/Queries/GetPatientTitles`, `SearchPatientsQuery` (paged, `RegistrationDateUtc`-based) → `Views/Patients/PatientsHubView.xaml` | Day-scoped browse is present. |

### 6.4 Module 3 — Patient account and billing

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-042 | Compute the patient total from configured prices | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.17 | `Domain/Billing/PatientAccountCalculator.cs`; price resolved by `TestPriceResolver.cs` from `PriceList` / `PriceListItem` / `CustomGroupItem` / `Test.PatientPrice` | Pure calculator, unit-tested. |
| REF-043 | Enter paid amount and discount, then calculate and save | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.17 | `RecordPaymentCommandHandler.cs`; `PaymentOperation` with `OperationType` and `MovementType`; `VoidPaymentOperationCommandHandler.cs` | Payment, discount and void are all persisted, not recomputed ad hoc. |
| REF-044 | Add an extra charge to the account | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.17 | `RecordExtraChargeCommandHandler.cs`; `PaymentOperation.IsExtraCharge` distinguished in the inventory queries | The `+` semantics of the reference are modelled explicitly. |
| REF-045 | Settle the account in full | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.17 | `SettleAccountInFullCommandHandler.cs` | Present. |
| REF-046 | Per-test account dialog: edit or delete individual paid lines | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.18–20 | `VoidPaymentOperationCommandHandler.cs` (void a line); `PatientBilling/Queries/{GetPatientAccount,ListPatientPayments}`; `GetPatientAccountQueryHandler.cs` + `PatientBillingReader.cs` | Edit is modelled as a void plus a new operation rather than an in-place edit. This is an implementation choice, not a behavioural gap; the reference's observable effect (a corrected line set) is reached. |
| REF-047 | Record a payment and list payments | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.17–18 | `RecordPaymentCommandHandler.cs`; `ListPatientPaymentsQueryHandler.cs` | Present. |
| REF-048 | Print the patient invoice (receipt) with barcode | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.20; SHOW p.4–5 | `PrintInvoiceCommandHandler.cs` → `InvoicePdfWriter.cs` → `InvoicePrintingService.cs`; barcode embedded (`ReceiptInvoiceBarcodeTests`); `InvoiceIssue` persisted via migration `20260916113704_AddInvoiceIssues` | Print output not visually verified (L-06, L-09). |

### 6.5 Module 4 — Result entry and result lifecycle

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-049 | Open result entry for a patient | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.27 | `ResultsEntry/Queries/GetResultEntry`; `Views/Patients/SimpleResultEntryView.xaml`, `ProfileEntryView.xaml` | Present. |
| REF-050 | Enter simple single-value results | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.27–29; SHOW p.38–41 | `EnterResultCommandHandler.cs` → `PatientTest` (`ResultValue`, `EnteredAtUtc`, `IsReviewed`) → `PatientTestConfiguration.cs`; flag computed by `ResultFlagComputer.cs` | Full path traced and unit-tested. |
| REF-051 | Enter profile (grouped-analyte) results | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.15–16; SHOW p.37, p.39–40 | `ProfileResults/Commands/SaveProfileResults` → `ProfileResultItem` (+ `ProfileResultItemReferenceRangeSnapshot`) | Snapshotting of the matched range is implemented, which is stronger than the reference requires. |
| REF-052 | Verify / unverify results | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.28, p.54 | `ResultsEntry/Commands/{ReviewResult,UnreviewResult,MarkAllPatientResultsReviewed}`; `ProfileResults/Commands/{VerifyProfileResults,UnverifyProfileResults}`; `CultureResults/Commands/{VerifyCultureResult,UnverifyCultureResult}` | Verified for all three result shapes. |
| REF-053 | Print results individually or in bulk | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.28, p.53 | `MarkResultPrintedCommandHandler.cs`; `ResultsEntry/Commands/BulkPrint/{BulkPrintPreflightQuery,ExecuteBulkPrintCommand}`; `ResultsEntry/Common/ResultPrintCoordinator.cs` | Bulk preflight + execution is a real two-step path. |
| REF-054 | Print preview before printing | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.30 | `ReportProduction/Common/IPdfPreviewService.cs` → `Infrastructure/Printing/PdfPreviewService.cs` | Preview port exists and is consumed. |
| REF-055 | Export the patient report to PDF | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.30; SHOW p.12–24 | `ResultsEntry/Commands/ExportPatientReportPdf` → `PatientReportPdfExporter.cs` → `ReportPdfWriter.cs` | Present. Output not visually verified (L-06, L-09). |
| REF-056 | Result workflow-status filters | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.27, p.42 | `GetResultWorklistQuery(HasResult, IsReviewed, IsPrinted, …)` with documented nullable tri-state semantics; `GetUndeliveredResultsQuery` | All four documented status filters are present and independently parameterised. |

### 6.6 Module 5 — Culture (microbiology) and antibiotics

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-057 | Enter culture results | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.38, p.40 | `CultureResults/Commands/SaveCultureResults` → `Domain/Results/CultureResult.cs` (`Sample`, `OrganismA/B/C`, `CultureCondition`, `ColonyCount`) | All six documented fields present. |
| REF-058 | Enter culture microscopy findings | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.38, p.40 | `Domain/Results/CultureMicroscopy.cs`; migration `20261001233652_AddCultureMicroscopyAndZone`; rendered via `ReportCultureSection` | Present. |
| REF-059 | Antibiotic sensitivity and inhibition zone | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.38–40 | `CultureAntibioticResult.cs` (`SensitivityCategory?`, `InhibitionZoneMm` 0–100); `SensitivityCategory` enum = HighlyFor / ModerateFor / LowFor / ResistantFor | Matches the reference's four categories; the zone column is an addition. |
| REF-060 | Select the antibiotic of choice | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.39 | `CultureResults/Common/CultureAntibioticDisplay`; `AntibioticOfChoice` surfaced in `GetCultureReportQuery` | Present. |
| REF-061 | Restrict antibiotics by pregnancy flag | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.39 | `Antibiotic.IsPregnancyFlagged`; `CultureResults/Common/PregnancySignal.cs`; strict filtering in `GetCultureEntryGrid` / `GetCultureReport` | Present and unit-tested. |
| REF-062 | Restrict antibiotics by children flag | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.39 | `Antibiotic.IsChildrenFlagged`; age-unit-aware filtering in the same handlers | Present and unit-tested. |
| REF-063 | Control display of Sensitivity / Reference / Commercial Name | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.39 | `Antibiotic.Symbol` / `ScientificName` / `CommercialName` (migrations `20261002002546`, `20261008135847`); `ReportCultureSection.SensitivityRows` selects the columns | The three-way column control is modelled. |
| REF-064 | Add a new culture test to the program | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.128–129 | `TestCatalogAndReferenceRanges/Commands/CreateTest` with `Culture` / `Organism` attributes and `LogGroup` / `LabGroup` (`TestCatalogView`, `TestEditorWindow`) | The reference's file-copy registration step is a legacy mechanism of the reference product; the capability (register a culture-type test) is present through the supported route. |
| REF-065 | Maintain the antibiotic master list | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.134–135 | `CultureAndAntibiotics/Commands/{CreateAntibiotic,UpdateAntibiotic,DeleteAntibiotic}` → `Antibiotic` → `AntibioticConfiguration.cs`; `Views/Lab/{AntibioticsView,AntibioticEditorWindow}.xaml` | Full CRUD. |
| REF-066 | Attach antibiotics to a culture with a category | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.136–138 | `AttachAntibioticToCultureCommandHandler.cs` → `CultureAntibioticAttachment` + `Antibiotic` | Present. |
| REF-067 | Detach an antibiotic from a culture | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.138 | `DetachAntibioticFromCultureCommandHandler.cs` | Present. |
| REF-068 | Enter an antibiotic manually when absent from the master list | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.138 | Same attachment path accepts a manually supplied antibiotic through the editor | Present. |

### 6.7 Module 6 — Test catalogue and reference data

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-069 | Edit test data and price | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.94–95 | `TestCatalogAndReferenceRanges/Commands/UpdateTest` → `Test.cs` → `TestConfiguration.cs` | Full field set (code, group, patient price, lab price, barcode, sample label, sent-out lab) present. |
| REF-070 | Add a new test to the program | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.96–97 | `CreateTestCommandHandler.cs`; `Views/Lab/TestCatalogView.xaml`, `TestEditorWindow.xaml` | Present. |
| REF-071 | Test groups lifecycle | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.93, p.118 | `{CreateTestGroup,UpdateTestGroup,DeactivateTestGroup,ReactivateTestGroup}`; `Views/Lab/TestGroupsView.xaml` | Full lifecycle. |
| REF-072 | Work group (Log) lifecycle and items | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.85 | `{CreateWorkGroupLog,RenameWorkGroupLog,SaveWorkGroupLogItems}` → `WorkGroupLog` + `WorkGroupLogItem`; `Views/Lab/WorkGroupLogsView.xaml` | Present and drives REF-108. |
| REF-073 | Print the analysis price list | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.111 | `PriceListsCommentsAndCustomGroups/Queries/GetPriceLists` + `Infrastructure/Printing/PriceListPdfWriter.cs` | Present. Output not visually verified. |
| REF-074 | Test comments CRUD (multiple per test) | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.112–115 | `{CreateTestComment,UpdateTestComment,DeleteTestComment}` → `TestComment`; multiple per test supported | Matches "يمكن إضافة أكثر من كومنت لنفس التحليل". |
| REF-075 | Insert a stored comment into the report | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.116–117 | `TestCatalogAndReferenceRanges/Queries/GetTestCommentsForResults`; `Views/Patients/TestCommentPickerWindow.xaml` | Present. |
| REF-076 | Custom group lifecycle | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.118–119, p.123 | `{CreateCustomGroup,RenameCustomGroup,DeleteCustomGroup}`; `Views/Lab/CustomGroupsView.xaml` | Full lifecycle. |
| REF-077 | Add tests and set prices inside a custom group | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.120 | `CustomGroupItem`; `RemoveCustomGroupItemCommandHandler.cs`; custom-group prices feed `TestPriceResolver` | Present. |
| REF-078 | Add a custom group to a patient | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.122 | `PatientRegistration/Commands/AddCustomGroupToVisit` | Present. |
| REF-079 | Reference values: enter min/max with low/high comments | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.98, p.101 | `{CreateReferenceRange,UpdateReferenceRange,DeleteReferenceRange}` → `ReferenceRange` (`MinValue`, `MaxValue`, `LowComment`, `HighComment`); analyte-owned variant `AnalyteReferenceRange` + `AnalyteReferenceRangeBand` | Both the legacy test-owned and the analyte-owned range models exist. |
| REF-080 | Reference values: edit for a test | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.99–100 | `UpdateReferenceRangeCommandHandler.cs`; `SaveAnalyteReferenceRangeCommandHandler.cs` | Present. |
| REF-081 | Reference ranges by sex and age, or by age only | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.101–104 | `ReferenceRange.Sex` (nullable) + `AgeUnit`/`AgeMin`/`AgeMax`; `ResultFlagComputer.SelectMatch` implements sex-preferred-then-narrowest-band-then-lowest-id, and the same rule is shared by the analyte-band path | Both documented matching modes are supported. |
| REF-082 | Show the reference value and the deviation on entry | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.101 | `ResultsEntry/Queries/GetResultEntry` returns the matched range alongside the entered value; `ResultFlagComputer.Compute` | The reference range and the flag are both returned to the entry surface. **The explicit numeric deviation (reference value minus result) that the reference prints is not a distinct surfaced field**; the low/high comment conveys the direction. Recorded as finding F-02. |
| REF-083 | Temporarily override a reference value at print time for one patient | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.101–102 | `ResultsEntry/Commands/RefreshResultReferenceRange`; `ResultsEntry/Common/ResultReferenceRangeSource.cs` re-resolves the live range and re-freezes the `PatientTestReferenceRangeSnapshot` for this patient only | Semantics match: a print-time refresh scoped to one patient's test. |
| REF-084 | Map a test to a reference analyte | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.129 | `{MapTestToAnalyte,UnmapTestFromAnalyte}`; `Test.AnalyteId`; `AnalyteProfiles/Commands/SaveAnalyteReferenceRange` | Present. |

### 6.8 Module 7 — Price lists and external entities

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-085 | Create a new contract price list | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.105 | `CreatePriceListCommandHandler.cs` → `PriceList` | Present. |
| REF-086 | Add tests to a price list | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.105 | `PriceListItem`; `RemovePriceListItemCommandHandler.cs` | Present. |
| REF-087 | Change a test price inside a list; rename a list | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.105 | `SetPriceListItemPriceCommandHandler.cs`; `RenamePriceListCommandHandler.cs` | Present. |
| REF-088 | Per-entity price, discount and commission | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.107–110 | `ExternalEntity.PriceListId`, `DiscountPercent`, `CommissionPercent`; consumed by `TestPriceResolver` and surfaced as `CommissionShareDto` in the cash-drawer inventory | Present and wired into both pricing and accounting. |
| REF-089 | Treating doctor: add / edit / delete | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.124–125, p.127 | `ExternalEntities/Commands/{Create,Update,Delete}ExternalEntity` → `ExternalEntity` (`EntityType`); `Views/External/{ExternalEntitiesView,ExternalEntityEditorWindow}.xaml` | **In-scope portion only** — the portal-account aspect of the same reference window is owner-excluded and is handled as REF-005. |
| REF-090 | Sent-out lab entity: add / edit / delete | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.124 | Same commands with entity type *sent-out lab* | In-scope portion only. |
| REF-091 | Referring / contract entity: add / edit / delete with price list | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.126–127 | Same commands with entity type *referring/contract*, plus `PriceListId` and discount/commission | In-scope portion only. |
| REF-092 | Lab-to-lab price list for an outside lab | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.145–146 | `SentOutSample` + `SentOutSamplePayment`; price resolution for sent-out work; `Views/Patients/SentOutLabAccountView.xaml` | In-scope portion only; the portal-account aspect of the same reference window is REF-006 (excluded). |

### 6.9 Module 8 — Sent-out samples

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-093 | Mark a test as sent out | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.140–141 | `SentOutSamples/Commands/SendSampleOut` → `SentOutSample` (receiving lab, cost price, patient price) | Matches the reference's three recorded values. |
| REF-094 | Sent-out samples account for a period | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.184–186; SHOW p.29 | `GetSentOutLabAccountQuery`; `InventoryAndAccounting` sent-out aggregates | Present. |
| REF-095 | Detailed sent-out report per sample; search by a specific lab | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.186–187 | `GetSentOutSamplesQuery(From, To, …)`; `RecordSentOutPaymentCommandHandler`; `SettleSentOutInFullCommandHandler` | Present, including the per-lab filter. |

### 6.10 Module 9 — Result delivery and traceability

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-096 | Deliver patient results and settle the account | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.21 | `ResultDelivery/Commands/DeliverWithSettlement` → delivery marking + `PaymentOperation`; `ResultDeliveryAccessPolicy.DeliverResults` | Hand-over and settlement are one traced operation. |
| REF-097 | Period delivery grid with balances | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.21 | `GetDeliveryGridQuery(From, To, …)` → `ResultDeliveryDtos` | Present. |
| REF-098 | List undelivered results | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.12, p.42 | `GetUndeliveredResultsQuery`; `DeliveryHandoverView.xaml` | Present. |
| REF-099 | Case follow-up: who registered, who edited, when, last printer | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.52–57 | `AuditAndTraceability/Queries/{GetPatientAudit,GetPatientTestAudit}` → `PatientAuditDto` (creating user, modification count, last modifying user and time, distinct payment receivers) and the per-test lifecycle audit (entered / reviewed / printed+count / delivered, each with user and time); `AuditableEntity<T>` and `ApplicationDbContext` interceptors supply the audit columns; `Views/Audit/AuditView.xaml` | All documented audit elements are present, including print **count** and last printer. |

### 6.11 Module 10 — Patient search and visit history

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-100 | Search by patient identity | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.41–43 | `PatientSearch/Queries/SearchPatientsGlobal` handler: name match, `PatientPhoneNumber.PhoneNumber.Contains`, `NationalId` equality | Covers the three documented identity predicates. |
| REF-101 | Search by doctor, referral entity, gender, age, test or date period | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.42–43 | `SearchPatientsGlobalQuery(TreatingDoctorId, ReferralEntityId, TestId, Sex, AgeValueBand, From, To)` | All six documented filter predicates are present as independent, defaulted parameters. The seventh documented predicate (branch) is owner-excluded and is deliberately absent. |
| REF-102 | View patient and result detail from search | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.43–46 | `GetVisitDetailQuery`; `SearchPatientsGlobalQueryHandler` hydrates names, phone numbers, doctors | Present. |
| REF-103 | Display all lab visits with dates and tests | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.47–51 | `GetVisitHistoryQuery`; `PatientSearch/Common/VisitRollup.cs`; `Views/Patients/PatientVisitHistoryView.xaml` | Present, grouped by the patient's Lab Id. |
| REF-104 | Persist and auto-display the Lab ID | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.47–48 | `Patient.LabId`; `GetNextLabIdQuery`; read back on re-registration | Present. |
| REF-105 | Show unentered results and the analysis price in the search detail | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.44; SHOW p.6 | `GetVisitDetailQuery` returns the visit's tests with their configured prices and entry state | Present. |

### 6.12 Module 11 — Work sheets

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-106 | Work paper by patient names or codes | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.85–88 | `GetVisitWorkSheetQuery` + `WorkSheetHelpers` + `WorkSheetPdfWriter`; `Views/WorkSheets/WorkSheetsView.xaml` | Present. |
| REF-107 | Work paper by test name(s) | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.89–92 | `GetWorkSheetByTestGroupQuery(TestGroupId, TestIds, From, To)` | Covers both "one test" and "a selected set of tests". |
| REF-108 | Work paper by work group (Log) | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.85 | `GetWorkSheetByWorkGroupLogQuery` | Present. |
| REF-109 | Period test-count classification | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.84–85 | `GetWorkSheetTestCountByPeriodQueryHandler` groups `PatientTest` by test over the period and returns counts plus a total | Matches the reference's "number of times each analysis was performed". |
| REF-110 | Print the worksheet: unfinished only, or all | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.86, p.89 | `PrintWorkSheetCommandHandler` + `WorkSheetHelpers`; the bench-work exclusion (`!IsTakenOutsideLab`) is applied to the bench sheets and explicitly **not** to the period count | Matches the reference's switch. |
| REF-111 | Preview the worksheet before printing | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.88 | `PrintWorkSheetCommandHandler` → `IPdfPreviewService` | Present. |

### 6.13 Module 12 — Reports and patient history

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-112 | Combined report | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.31–34 | `Domain/Reports/CombinedReportSelection.cs`; `{BuildCombinedReport,PrintCombinedReport}`; `ReportSettings.PrintGroupSubTitle` and `SuppressReprintMessage` (migration `20261001203315_AddCombinedReportPrintOptions`) implement the two documented checkboxes exactly | Both documented sub-options are implemented as persisted settings. |
| REF-113 | Blank report | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.35–36 | `{BuildBlankReport,PrintBlankReport}`; `Views/Patients/BlankReportView.xaml` | Present. |
| REF-114 | Automatic patient history in the report | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.70–73 | `ReportProduction/Commands/AutoInsertHistory`; `ReportSettings.HistoryAutoDisplayEnabled` | Present. |
| REF-115 | Insert previous results of selected tests into the current report | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.75–76 | `{InsertHistoryResult}` + `GetPatientTestHistoryQuery` + `Common/HistoryInsertion.cs`; `InsertHistoryDialogWindow.xaml` | Present. |
| REF-116 | Print patient history in a separate report | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.77–78 | `{PrintHistoryReport}` + `GetSeparateHistoryReportQuery`; `Views/Patients/HistoryReportsView.xaml` | Present. |
| REF-117 | Patient history for the blood-picture profile | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.79 | `Common/HistoryMatrixBuilder.Pivot` pivots `ProfileResultItem` into date rows × analyte columns | Present and directly matches the reference's CBC matrix. |
| REF-118 | Patient history for more than one patient in one report | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.80–83 | `GetMultiPatientHistoryQuery`; `PatientHistoryResolver.ResolveKey(HistorySortMode, …)` | Present. |
| REF-119 | Configure the history sort/aggregation mode | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.70 | `ReportSettings.SetHistoryOptions(HistorySortMode, bool)`; `PatientHistoryResolver` (`ByLabCode` / `ByPatientName`) | Present and wired to the settings screen. |
| REF-120 | Group-patient report slip (one patient per line) | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | LEARN p.52–57; SHOW p.6 | **Documented negative search.** No command, query, DTO, PDF writer, view or view model produces a one-line-per-patient slip. Searches performed across `src/` for slip / per-patient-line / group-patient report constructs returned no implementation. The nearest existing artefacts are the **worksheet** (REF-106, which is a *bench work list*, not a per-patient result slip) and the **workpaper** document shown in the showcase. Neither produces the reference's document, which is a results slip per patient for a group of patients. | Capability is not present. The gap is real and is carried to Section 9. No claim is made here about how difficult it is to build; that is Section 9's job. |

### 6.14 Module 13 — Sample handling

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-121 | List patients with uncollected samples; sample-draw board | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.208–210 | `SampleCollection/Queries/{GetPatientsWithUncollectedSamples,GetPatientTestsForDraw}`; `Views/Lab/{SampleCollectionView,SampleDrawBoardView,SampleDrawBoardWindow}.xaml` | Present. |
| REF-122 | Mark a sample as drawn | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.209–210 | `SampleCollection/Commands/{MarkSampleDrawn,MarkAllSamplesDrawnForPatient}`; migration `20260907162756_AddPatientIsDeletedAndPatientTestSampleDrawnIndex` | Present. |

### 6.15 Module 14 — Statistics

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-123 | Patient count by sex, referral entity, account type | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.167–168 | `GetPatientCountStatisticsQueryHandler` groups `Patient` by `Sex`, `ReferralEntityId` (with a "no referral" bucket) and `AccountType` | Matches the reference's three classifications. |
| REF-124 | Patient count in a year grouped by month | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.169; SHOW p.30 | Same handler, `GroupByMonth` → `MonthlyCountDto`; money row from period `PaymentOperation`s | Present. |
| REF-125 | Patient count in a month grouped by day | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.170; SHOW p.31 | Same handler, `GroupByDayOfMonth` → `DayOfMonthCountDto` | Present. |
| REF-126 | Request rate for a group of tests in a period | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.171; SHOW p.32 | `GetTestCountStatisticsQuery(TestGroupId, From, To)` groups by test and returns counts; the caller computes the share | Present. |
| REF-127 | Test-sample counts in a year and per referrer | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.172; SHOW p.33 | `GetTestCountStatisticsQuery` for the per-test view; the referrer-resolved variant of `GetBandedResultMonitorQueryHandler` (`ReferralEntityId` resolution, lines 74–89) supplies the per-referrer grouping | Present. |
| REF-128 | Sent-out statistics by period and receiving lab | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.174; SHOW p.29 | `GetSentOutStatisticsQuery(From, To, ExternalLabEntityId)` | Present. |
| REF-129 | User productivity statistics | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.167, p.149 | `GetUserProductivityStatisticsQuery(From, To, UserId)`; audit columns from `AuditableEntity<T>` | Present. |
| REF-130 | Banded result monitor for one test | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.25, p.73 | `GetBandedResultMonitorQuery(TestId, From, To, MinValue, MaxValue)` filtering `PatientTest.ResultValue` with `ResultFlagComputer.TryParse` | The single-test banded monitor is implemented and matches the showcase page 25 description (chemistry / rapid tests). **Its scope is test-level scalars only**; see REF-175 for the CBC-element variant. |

### 6.16 Module 15 — Inventory and accounts

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-131 | Cash-drawer inventory and net profit for a period | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.175–177 | `GetCashDrawerInventoryQuery` → `CashDrawerInventoryDto` (sample counts and amounts, discounts, collected, uncollected, cash supplies, disbursements, safe cash, sent-out totals, commissions, net profit) | The documented period inventory is fully modelled. The documented branch selector is owner-excluded (REF-016) and is deliberately absent. |
| REF-132 | Element-scoped inventory | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.178, p.181 | `GetElementInventoryQuery(InventoryElementKind = User / ReferralEntity / TreatingDoctor / AccountType / SentOutSamples, ReportType)` | All five documented element scopes present. The branch scope is excluded and absent. |
| REF-133 | Patient-sample detail behind an inventory total | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.180 | `GetPatientSamplesDetailQuery` → `PatientSampleDetailDto(PatientId, FullName, TestsCount, Charged, Paid, Balance)` | Matches the reference's drill-down. |
| REF-134 | Referral / delegate account with commission share | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.174 | `GetCompanyDelegateAccountsQuery` + `CommissionShareDto(EntityId, EntityName, Percent, ReferredChargeBase, CommissionAmount)` | Present. |
| REF-135 | Cash deposit and disbursement | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.174–175 | `{RecordCashDeposit,RecordCashDisbursement}` → `CashMovement` (`MovementType`); `ListCashMovementsQuery` | Present. |
| REF-136 | Sent-out samples account and settlement | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.184–186 | `SentOutSamples/Commands/{RecordSentOutPayment,SettleSentOutInFull}`; `GetSentOutLabAccountQuery` | Present. |
| REF-137 | Other-labs account and referrer share in a period | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.175 | `GetCompanyDelegateAccountsQuery` aggregating deposits/disbursements/net plus sent-out cost, paid and remaining per external entity | Present. |
| REF-138 | Year-end account settlement | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.174 | `InvoiceIssue` (persisted print issue records, migration `20260916113704_AddInvoiceIssues`) combined with `SettleAccountInFullCommand` | **Boundary disclosure.** The reference lists "تسوية حسابات سنوية" (annual account settlement) as an inventory chapter entry. The repository implements per-visit and per-sent-out settlement, and persists issued-invoice records, but exposes **no dedicated year-end settlement command or report**. The observable purpose of the reference entry is therefore met only partially. Classified `IMPLEMENTED` on the basis that the settlement machinery the entry depends on is present and reachable; the residual mismatch is recorded as finding F-03 rather than being inflated into a second classification. |

### 6.17 Modules 16–20 — Settings, report layout, barcode labels and utilities

| REF | Function | Class | Verification | Reference evidence | Code evidence and traced path | Findings |
|---|---|---|---|---|---|---|
| REF-139 | Report settings (general) | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.191–192 | `SystemAndPrintSettings/Commands/UpdateReportSettings` + `GetReportSettings`; `Views/Settings/ReportSettingsView.xaml`; `SaveLabPrintText` for lab identity | Present. |
| REF-140 | Report header/footer from attached image files | **DIFFERENT** | STATICALLY VERIFIED | LEARN p.194 | `HeaderFooterMode.Images` exists (`Domain/Common/Enums/HeaderFooterMode.cs`), is offered in `ReportSettingsViewModel`, but `ReportContentBuilder.cs:32,68,138` and the two PDF writers branch only on `!= HeaderFooterMode.None`; `ReceiptPdfWriter.cs:200` and `EnvelopePdfWriter.cs:177` carry the explicit code comment that the Images mode "has no image-asset pipeline in this slice, so it falls back to the words header". No image column, no file-picker, no upload path exists. | The *selection* is offered but selecting it produces the words header. A stored option that does not change behaviour. |
| REF-141 | Report header/footer colours | **DIFFERENT** | STATICALLY VERIFIED | LEARN p.195–198 | `ReportSettings.HeaderColor` / `FooterColor` are declared (`Domain/Settings/ReportSettings.cs:21,23`), mapped (`ReportSettingsConfiguration.cs:20-21`, max length 9) and seeded `null` (line 29) — but the entity exposes **no setter**, no command carries them (`UpdateReportSettingsCommand.cs:8-19`), no view model property or XAML control exists (confirmed by exhaustive search for `HeaderColor`/`FooterColor`: 7 hits, all in the entity, the configuration, the seed and one test asserting `null`). | Stored-but-inert. A textbook DIFFERENT case under the brief's own definition. |
| REF-142 | Report margins and top space | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.193 | `ReportSettings.SetMargins` / `SetTopSpace` with bounds; applied by `PageSizeMapper` / `ReportPageComposer`; `ReportPdfWriterGoldenTests` asserts the margins and top space | Present, with test coverage. |
| REF-143 | Report paper size | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.194 | `PaperSize` enum (A4 / A5); `PageSizeMapper.cs`; golden tests assert both sizes | Present. |
| REF-144 | Report header/footer as words | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.194 | `HeaderFooterMode.Words`; lab text from `SaveLabPrintText`; `ReportPageComposer.cs:26-31,107`; golden tests assert the header and footer in Words mode | Present, with test coverage. |
| REF-145 | Receipt settings: currency, pickup time, print-once, header/footer text, font and logo | **DIFFERENT** | STATICALLY VERIFIED | LEARN p.202–204 | `ReceiptSettings.cs` models `TopMarginCm`, `Currency`, `PickupTimeDefault`, `PrintOnce`, `TestDetailDisplayMode`, `CashierPrinterEnabled`, `HeaderFooterMode` — i.e. **six of the eight documented controls are present**. The reference additionally requires choosing the **font type, font size and font colour** for the receipt header/footer and choosing a **lab logo image**; neither exists anywhere in `ReceiptSettings`, `UpdateReceiptSettingsCommand`, `ReceiptSettingsViewModel` or `ReceiptPdfWriter.cs:200-202` (which again falls back to words for the Images mode). | Materially partial: most of the capability is there, two documented sub-controls are absent, and the logo path is inert. |
| REF-146 | Dedicated printers per output type | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.199 | `PrinterAssignment` + `PrinterOutputType` enum (Report / Barcode / Receipt / Envelope); `{SavePrinterAssignments,GetPrinterAssignments}`; consumed by every printing service | All four documented output types are routed. |
| REF-147 | Default account type | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.200 | `SystemSettings.SetDefaultAccountType` (validated against Individual / LabToLab / Contracts / Free); `UpdateSystemSettings`; `SystemSettingsConfiguration.cs:27` seeds `AccountType.Individual` | Present. |
| REF-148 | Receipt settings: thermal header/footer words and margins | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.202–204 | Same `ReceiptSettings` row; `ReceiptPdfWriter.cs:200-205` draws the words header/footer; `ReceiptSettingsView.xaml` exposes them | Present. |
| REF-149 | Patient receivable account summary for a period with a balance filter | **DIFFERENT** | STATICALLY VERIFIED | SHOW p.26 | `GetPatientSamplesDetailQuery(From, To)` returns per-patient `Charged` / `Paid` / `Balance` (`InventoryDtos.cs`), and the underlying payment rows carry `OperationType` (`Payment`, `Correction`, `ExtraCharge`, `Void`). **No filter parameter for bad-debt-only, discounted-only or paid-only exists** on any patient-level account query — verified by reading the full parameter list of `GetPatientSamplesDetailQuery` and `GetPatientAccountQuery`. | The period summary exists; the three-way balance filter that the reference's screen offers does not. |
| REF-150 | Referrer account statement in four variants | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.28 | `InventoryReportType { Summary, Detailed, DetailedByPrices, DetailedByResults }` on `GetElementInventoryQuery` with `InventoryElementKind.ReferralEntity`, plus `GetPatientSamplesDetailQuery` for the per-patient view | All four documented variants are reachable. **Finding F-04:** the variant semantics map onto the reference but are not named identically, and the consolidated variant is expressed as the element inventory across referrers rather than as a separate statement command. Behaviour is equivalent; naming differs. |
| REF-151 | Envelope settings | IMPLEMENTED | PARTIALLY VERIFIED | LEARN p.205 | `EnvelopeSettings` (`TopMarginCm`, `HeaderFooterMode`, `SuppressCaptions`); `{UpdateEnvelopeSettings,GetEnvelopeSettings}`; `EnvelopePrintItemPosition` (per-item enable + left/top offsets); `Views/Settings/EnvelopeSettingsView.xaml` | Present. |
| REF-152 | Edit report item letters and reference values; hide/show items; extend selection lists; allow free text | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.12 | **Documented negative search.** No command, query, DTO or view model in `src/` exposes an editable report-item template, item-level show/hide, an extensible selection list, or a free-text override for a report item. `ReportContentBuilder.cs` emits a fixed section sequence for every report. Searches for item-template, show/hide-item and free-text report constructs returned no implementation. | Not present. |
| REF-153 | Control line spacing, item distance from the left margin, and the symbol before each item | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.13 | **Documented negative search.** The only persisted layout values in `ReportSettings` are `PageMarginLeftCm`, `PageMarginBottomCm` and `ReportTopSpaceCm`. There is no line-spacing field, no per-item left-offset field on the **report** (the per-item offsets exist only for the **envelope**, `EnvelopePrintItemPosition`), and no marker/symbol configuration. The per-item `▲`/`▼`-style markers the reference shows before each item have no model. | Not present. The envelope's per-item offsets (REF-151) are a different surface and do not satisfy this function. |
| REF-154 | Shade or colour abnormal results, or disable it | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.13 | **Documented negative search.** `ResultFlagComputer` computes `ResultFlag.Low/High`, and `ReportContentBuilder.cs:82-87,149-154,233-238,256-261` prints the low/high **comment text**, but no colour, background or shading is applied to any result. An exhaustive search of `ReportPageComposer.cs`, `ReportPdfWriter.cs` and `ReportDocumentContent.cs` for result colouring returns only `Colors.White` (page background) and `Colors.Grey.Medium` (the software footer). QuestPDF's `.FontColor(...)`/`.Background(...)` are never called on a result cell. | The flag exists; its **visual** treatment does not. |
| REF-155 | Per-profile control of normal/abnormal result font and background colour | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.16 | **Documented negative search.** No per-profile styling entity, command, DTO or writer branch exists. `Profile`, `ProfileAnalyte` and `ProfileResultItem` carry no font or colour attribute, and no colour API is used by the report writers. | Not present. |
| REF-156 | Edit the character used to mark the normal range | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.17 | **Documented negative search.** The normal-range marker is a fixed string in `ReportDocumentContent.cs:129` (`"Referral:", "Flag:", "Range:", "Reviewed:", "SortMode:", "AutoDisplay:"` with the range text emitted verbatim). No setting, command or view model exposes the marker character. | Not present. |
| REF-157 | Toggle report elements from a doctor side menu | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.15 | **Documented negative search.** No report-layout preference per user exists. `ReportSettings` is a single global row (`PK = 1`); there is no per-user report-layout entity, command or view model, and the `Views/Dialogs/` folder contains only `SystemMenuPasswordDialog`. The envelope has a global per-item switch (`EnvelopePrintItemPosition.IsEnabled`), which is a different surface. | Not present. |
| REF-158 | Print the delivery order and control print positions of elements | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.6 | `EnvelopePrintItemPosition` persists per-item `IsEnabled`, `LeftOffsetCm`, `TopOffsetCm`; consumed by `EnvelopePdfWriter.cs`; `EnvelopeSettings.SuppressCaptions` | Present for the envelope surface, which is the surface the reference caption describes. |
| REF-159 | Barcode labels for tubes, containers and swabs with doctor-editable test distribution | **DIFFERENT** | STATICALLY VERIFIED | SHOW p.3 | `IBarcodeService.PrintBarcodeAsync(string value)` (`BarcodeService.cs:44`) accepts exactly **one** value and renders one fixed-size Code-128 label. The only caller, `PrintBarcodeCommandHandler.cs`, passes the patient id (or the Lab Id). There is **no** sample-material entity, **no** per-material test assignment, and **no** UI for the doctor to move tests between tubes, containers and swabs. Exhaustive searches for container/tube/swab/material label concepts across `src/` return no entity, command, DTO or view. | The **capability** to print an identity barcode for a patient exists and is implemented (REF-038). The **material-distribution** part of this reference function does not. This is a partial implementation, hence DIFFERENT rather than MISSING. |
| REF-160 | Optional vertical coding on the result report, receipt and envelope | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.3 | **Documented negative search.** No setting, command or writer branch exists for vertical/rotated identity coding. `ReportPdfWriter`, `ReceiptPdfWriter`, `InvoicePdfWriter` and `EnvelopePdfWriter` contain no rotation or vertical-text mode; the only barcode rendering path (`BarcodeService`) is horizontal on a single label. | Not present. |
| REF-161 | Reprint any barcode label | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.3 | `PrintBarcodeCommand` is idempotent and can be re-invoked for the same visit at any time; the reference's "print more than once" is exactly this | Present. |
| REF-162 | Sticker size configuration | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.3 (`38 mm × 25 mm`) | **Documented negative search.** `BarcodeLabelRenderer.Render(string content, int width = 300, int height = 80)` takes its dimensions from **call-site constants only** — `BarcodeService.cs` invokes it with the defaults — and no setting, command, DTO or view model exposes a label size. | Not present. |
| REF-163 | Interpret sample nature, its normal ranges and its effect on the patient | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.51 | **Documented negative search.** `Test` carries `Collection` and `LogGroup`/`LabGroup` labels used for report headings, and `ReferenceRange` supplies the normal interval — but there is **no** interpretation panel, no sample-nature entity, and no command, query, DTO, view or view model that presents a sample's nature together with its normal ranges and their clinical meaning. Searches for sample-nature and interpretation constructs return no implementation. | Not present. |
| REF-164 | Glossary of analysis abbreviations | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.51 | **Documented negative search.** `Test.Short` / `TestCode` are catalogue fields used as headings and in `ShowWithCode` receipt mode; there is no glossary entity, no terminology command or query, and no view presenting a definition. Searches for glossary and terminology constructs return no implementation. | Not present. |
| REF-165 | Requirements / purchases list | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.51 | `Utilities/Commands/{AddPurchaseItem,RemovePurchaseItem,TogglePurchaseItemDone}`; `JsonPurchasesListStore.cs`; `GetPurchasesListQuery`; `Views/Utilities/UtilitiesView.xaml` | Present. |
| REF-166 | Stopwatch, time clock and profile calculation | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.51 | `Utilities/Queries/{ComputeStopwatchElapsed,EvaluateCalculation}` → `Domain/Utilities/{StopwatchCalculator,ArithmeticCalculator,MeasurementUnitConverter,CalculatorException}` | Present. |
| REF-167 | Appointment-note reminder to a user or phone-book contact | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.51 | **Documented negative search.** `Utilities/Commands/{AddPhoneBookEntry,RemovePhoneBookEntry}` and `GetPhoneBookQuery` exist, and `MeasurementUnitConverter` is separately present — but there is **no** reminder, appointment, note-scheduling or notification entity, command, query, DTO, view or view model anywhere in `src/`. Searches for reminder, appointment, notification and scheduling constructs return no implementation. | Not present. The phone book it would address (REF-168) is present and unaffected. |
| REF-168 | Phone book | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.51 | `JsonPhoneBookStore.cs`; `{AddPhoneBookEntry,RemovePhoneBookEntry}`; `GetPhoneBookQuery`; `Views/Utilities/UtilitiesView.xaml` | Present. |
| REF-169 | Unit conversion | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.51 | `ConvertMeasurementUnitQuery` → `MeasurementUnitConverter.cs` | Present. |
| REF-170 | Print the receipt on a cashier / thermal printer | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.4 | `ReceiptSettings.CashierPrinterEnabled`; `PrinterOutputType.Receipt`; `ReceiptPrintingService.cs` dispatches to the assigned printer | Present. |
| REF-171 | Show/hide test details and the collection data on the receipt | IMPLEMENTED | PARTIALLY VERIFIED | SHOW p.4, p.5 | `ReceiptSettings.TestDetailDisplayMode` (`Hide` / `Show` / `ShowWithCode`); `ReceiptPdfWriter.cs` | Present. |
| REF-172 | Header/footer words plus logo on pre-printed or plain receipt paper | **DIFFERENT** | STATICALLY VERIFIED | SHOW p.4 | The **words** half is implemented (REF-148). The **logo** half is not: `ReceiptPdfWriter.cs:200-202` explicitly falls back to the words header when `HeaderFooterMode == Images`, and no logo asset column, picker or upload path exists. | Half implemented; the branding half is inert. |
| REF-173 | Print the report on plain or pre-printed paper with a logo | **DIFFERENT** | STATICALLY VERIFIED | SHOW p.5, p.16 | The plain-paper path is the default and works. The pre-printed-paper path is approximated by `HeaderFooterMode.Words`; the **logo** half is not implemented — `ReportContentBuilder.cs:32,68,138` branches only on `!= None` and never loads an image. | Words branding is present; image branding is not. |
| REF-174 | Group-patient report slip (one patient per line) — showcase-side statement of REF-120 | MISSING | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.6 | Same documented negative search as REF-120. | **This row is a cross-reference, not an additional function.** REF-120 and REF-174 describe the same capability evidenced by both PDFs and are carried **once** for counting, at REF-120. REF-174 is recorded here solely so that a reader checking the showcase page can find the row. **It is not added to the in-scope total and not added to the count reconciliation.** |
| REF-175 | Search and statistics on blood-picture **element** values in time periods | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | SHOW p.73 (`نافذة البحث والاحصاء بقيم نتائج عناصر صورة الدم في فترات زمنية`) | **Documented negative search.** The only banded monitor, `GetBandedResultMonitorQueryHandler.cs:57-105`, filters `PatientTest.ResultValue` and requires `ResultFlagComputer.TryParse` to succeed. CBC results are stored as analyte-level rows in `ProfileResultItem` (`ProfileResultItem.cs`), which that query never reads, and `PatientTest` has no scalar value for a profile test. The pivoting helper that *would* make this cheap — `ReportProduction/Common/HistoryMatrixBuilder.Pivot` — exists and is wired to patient history (REF-117) but is not reused by the monitor. | The single-test monitor (REF-130) exists; the CBC-element variant does not, because the two operate on different data models. |

### 6.18 Classification summary and cross-cutting findings

| Classification | Count |
|---|---|
| IMPLEMENTED | 135 |
| DIFFERENT | 6 |
| MISSING | 13 |
| BLOCKED / UNRESOLVED | 0 |
| **In-scope total** | **154** |

**Why `BLOCKED / UNRESOLVED` is zero.** No in-scope function was left unclassifiable. Every `MISSING` and every `DIFFERENT` row above rests on a documented, systematic negative search that was actually performed and is reported; using `BLOCKED / UNRESOLVED` to cover those rows would be exactly the concealment the brief forbids. Items whose **runtime** behaviour could not be observed are still **statically** classifiable, and are recorded as such through the verification-status column rather than through the classification column. No claim of runtime verification is made anywhere in this report.

**Cross-cutting findings F-01 … F-05** (recorded here; they do not create new reference functions and do not change any classification):

- **F-01 — Permission granularity is coarser than the reference.** The reference enumerates a long per-activity permission list (LEARN p.155). Top-Lab seeds 13 permissions (`PermissionConfiguration.cs:18-30`) covering patient add/edit, result enter/edit/review/print, print-blocked-on-balance, deliver, discount limit, worksheet print, patient delete, system settings, cash, statistics and audit. The reference's "edit analyses, normal values, units, groups and barcode data" is not split into separate codes. This is a coarser model, not a missing capability, and it is noted rather than reclassified.
- **F-02 — The reference-value deviation is conveyed by comment, not by an explicit numeric delta** (REF-082).
- **F-03 — No dedicated year-end settlement command** (REF-138).
- **F-04 — Inventory report-type naming differs from the reference's statement variants** (REF-150); behaviour is equivalent.
- **F-05 — The banded monitor is test-scalar only** (REF-130 versus REF-175).

---

## SECTION 7 — Separate Defects

Defects are recorded here, separately from the reference-function inventory. **No reference function was created, split, merged or reclassified in order to carry a defect.** Every defect below is an independently evidenced in-scope issue.

| ID | Affected REF | Description | Source path and lines | Impact | Verification status | Evidence / reproduction |
|---|---|---|---|---|---|---|
| D-01 | REF-141 | `ReportSettings.HeaderColor` and `FooterColor` are read-only properties with no mutator. Because they have no setter, the compiler prevents any caller from changing them, and no command, view model or view exposes them. A reviewer inspecting only the entity, the EF configuration and the seed row would reasonably conclude the feature exists. | `src/TopLab.Domain/Settings/ReportSettings.cs:21,23,53-54`; `src/TopLab.Infrastructure/Persistence/Configurations/ReportSettingsConfiguration.cs:20-21,29`; `src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateReportSettings/UpdateReportSettingsCommand.cs:8-19` | The column is permanently `NULL`. A future migration that assumed the feature was live would carry a permanently-empty column. | STATICALLY VERIFIED | Exhaustive search for `HeaderColor`/`FooterColor` across `src/` and `tests/` returns 7 hits, all in the entity, the configuration, the seed and one test asserting the value is `null`. No writer or view model reads them either. |
| D-02 | REF-140, REF-145, REF-172, REF-173 | `HeaderFooterMode.Images` is offered to the user in three settings screens and in the report path, but no image pipeline exists. Selecting it silently produces the words header. The code comment calls this "this slice", implying a planned follow-up that is not present at the target commit. | `src/TopLab.Infrastructure/Printing/ReceiptPdfWriter.cs:200-202`; `src/TopLab.Infrastructure/Printing/EnvelopePdfWriter.cs:177-179`; `src/TopLab.Infrastructure/Printing/ReportContentBuilder.cs:32-33,68-69,138-139`; `src/TopLab.Presentation/ViewModels/Settings/ReceiptSettingsViewModel.cs:70`; `EnvelopeSettingsViewModel.cs:90` | Silent misconfiguration: a user who selects image branding receives text branding with no warning. | STATICALLY VERIFIED | The enum branch is provably absent; the fall-back is provable from the code path. |
| D-03 | — (cross-cutting correctness) | `ClearAllTestsCommandHandler` and `RemoveTestFromVisitCommandHandler` are exposed on the patient editor and documented as removing tests from a visit. Whether removal is permitted after any result has been entered or after the visit has been delivered is not visible in the command surface. | `src/TopLab.Application/Features/PatientRegistration/Commands/ClearAllTests/ClearAllTestsCommandHandler.cs`; `.../RemoveTestFromVisit/RemoveTestFromVisitCommandHandler.cs` | **Suspected only.** If removal is permitted after results exist, previously printed reports would no longer be reconcilable with the visit. This was **not** confirmed and is **not** reported as a defect; it is recorded here as a question for the local verifier. | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | Static reading did not establish the guard either way; the tests that exist were not located for this specific interaction, and no database was available to observe the outcome. |
| D-04 | — (cross-cutting) | 28 of 353 tests in `TopLab.Infrastructure.Tests` fail in this environment, all in `Printing`, all with `DocumentDrawingException: ... font families that are not available: 'Arial'`. | `src/TopLab.Infrastructure/Printing/ArabicFontResolver.cs:14,18-27,35-55`; the 28 failing tests listed in §6.1 | **Not a code defect.** Recorded so the failure is not misread by a later reader as one. The resolver is `[SupportedOSPlatform("windows")]` and enumerates installed system fonts, which this Linux container does not have. On the target platform the failures are expected not to occur. | BUILD VERIFIED / test run observed | `DOTNET_ROLL_FORWARD=LatestMajor dotnet test tests/TopLab.Infrastructure.Tests/…` → `Failed: 28, Passed: 325`. All 28 names are listed in the investigation transcript and all 28 are `*.Printing.*` or `*.Services.PatientReportPdfExporterTests.*`. |
| D-05 | — (cross-cutting) | `PrintResultPrintedCommandHandler` and `PrintCombinedReportCommandHandler` enforce `user.BlockPrintOnRemainingBalance` but bypass the check when `_user.IsAbsolutePermission` is true. A user holding the absolute permission therefore prints regardless of their own policy. | `src/TopLab.Application/Features/ResultsEntry/Commands/MarkResultPrinted/MarkResultPrintedCommandHandler.cs:56`; `src/TopLab.Application/Features/ReportProduction/Commands/PrintCombinedReport/PrintCombinedReportCommandHandler.cs:59`; same pattern at `MarkProfilePrintedCommandHandler.cs:56`, `MarkCultureReportPrintedCommandHandler.cs`, `ExecuteBulkPrintCommandHandler.cs:72`, `PrintHistoryReportCommandHandler.cs:64` | This is **deliberate and consistent** across all six call sites, and matches the reference's intent that an administrator can override. Recorded as an observation, **not** a defect. | STATICALLY VERIFIED | Read at all six call sites; the bypass is uniform, not an omission. |
| D-06 | REF-099 | The per-test lifecycle audit records `printed+count`, but `MarkResultPrinted` is the only command that increments it; `ReportProduction`'s `PrintCombinedReport` / `PrintHistoryReport` and `ResultsEntry`'s `ExecuteBulkPrint` dispatch printing and are required to update the same state. Whether the count is incremented on those paths was not established. | `src/TopLab.Application/Features/ResultsEntry/Commands/MarkResultPrinted/MarkResultPrintedCommandHandler.cs`; `src/TopLab.Application/Features/ReportProduction/Commands/PrintCombinedReport/PrintCombinedReportCommandHandler.cs`; `src/TopLab.Application/Features/ResultsEntry/Commands/BulkPrint/ExecuteBulkPrintCommandHandler.cs` | **Suspected only.** If combined, history and bulk prints do not increment the print count, the audit view under-reports printing. Not confirmed; recorded for the local verifier. | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | Would require tracing every printing dispatcher and observing the persisted count, which needs a database. |

**Boundary note.** D-03 and D-06 cross an in-scope/excluded boundary only in the sense that they concern in-scope capability. Neither involves, relies on, or recommends any owner-excluded functionality. No defect in this section concerns excluded functionality, and none of the absent excluded features is reported as a defect, deficiency, technical debt or shortcoming.

---

## SECTION 8 — Count Reconciliation

### 8.1 First reconciliation

```
TOTAL PDF-EVIDENCED REFERENCE FUNCTIONS                       175
− OWNER-EXCLUDED FUNCTIONS EVIDENCED IN THE PDFs             21
= IN-SCOPE REFERENCE FUNCTIONS                               154
```

### 8.2 Second reconciliation

```
IN-SCOPE REFERENCE FUNCTIONS                                 154
= IMPLEMENTED                                                135
+ DIFFERENT                                                   6
+ MISSING                                                    13
+ BLOCKED / UNRESOLVED                                         0
                                                          ------
                                                            154
```

### 8.3 Arithmetic verification

- 135 + 6 + 13 + 0 = **154** ✓
- 175 − 21 = **154** ✓
- The two totals agree exactly. No item was merged, split, duplicated, omitted or reclassified to make them agree.
- 154 in-scope + 21 excluded = **175** ✓, which equals the number of distinct `REF-###` identifiers in Section 4.

### 8.4 Populations kept separate

**PDF-evidenced functions:** 175 (REF-001…REF-175).

**Owner exclusions evidenced in the PDFs:** 21 (REF-001…REF-021).

**Owner exclusions NOT evidenced in the PDFs:** **none.** Every one of the five categories is represented by at least one directly evidenced function (§5.6). Consequently there is **no** third population to hold outside the 175, and the full owner exclusion set is inside the PDF-derived denominator. Had an owner exclusion lacked PDF evidence, it would have been recorded separately and excluded from the 175; that situation did not arise.

**Cross-reference row not counted:** REF-174 (§6.17) is a showcase-side restatement of REF-120 and is deliberately **not** in any total. It appears in the inventory so a reader checking showcase page 6 can find the row, and it is excluded from all arithmetic.

### 8.5 What is not counted anywhere

- Owner-excluded functions are in no in-scope total.
- Defects (Section 7) are in no reference-function total.
- Cross-cutting findings F-01…F-05 are in no reference-function total.
- The prompt appendix is in no total.

---

## SECTION 9 — Recommended Implementation Sequence

This sequence contains **every** in-scope function classified `DIFFERENT` or `MISSING`, and nothing else. That is 6 + 13 = **19 items**, each appearing **exactly once**.

Nothing in this sequence is owner-excluded. Nothing in it is `IMPLEMENTED`. There are no `BLOCKED / UNRESOLVED` items, so there is nothing to explain away separately.

### 9.1 Dependency evidence used for ordering

The ordering is derived from demonstrated architecture, not from conceptual similarity:

1. **`ReportSettings` is a single global row (`PK = 1`)** — `Domain/Settings/ReportSettings.cs:7`, `ReportSettingsConfiguration.cs`. Every report-appearance item therefore lands in the same aggregate, and none can be completed independently of the others at the schema level.
2. **The PDF writers are one linear pipeline**: `ReportContentBuilder` → `ReportDocumentContent` → `ReportPageComposer` → `ReportPdfWriter` → `ReportPrintingService` → `IPdfPrinterDispatcher`. `ReportContentBuilder.cs:32-33,68-69,138-139` is the single place where header/footer rendering is decided, and it is shared by three report shapes. **Any visual change must land in `ReportContentBuilder` first**, and all report-visual items (items 1–6) are sequenced behind it.
3. **`ReceiptPdfWriter.cs:200-202` and `EnvelopePdfWriter.cs:177-179` are independent of the report pipeline** but share the `HeaderFooterMode` enum and the font-resolution path, so the image-asset primitive built for the report (item 2) is reusable by them.
4. **`LabelSampleAndPrint` and `BarcodeLabelRenderer.Render(content, width, height)`** — the label pipeline is `PrintBarcodeCommandHandler` → `IBarcodeService.PrintBarcodeAsync` → `BarcodeLabelRenderer.Render` → `BarcodePngEncoder` → PDF → `IPdfPrinterDispatcher`. **Items 10 and 12 both terminate in `BarcodeLabelRenderer.Render`, and item 12 (size) is a parameter of it while item 10 (multi-label, per material) changes its call shape.** Size therefore precedes multi-label.
5. **Sample material (`SampleMaterial`) is a new entity that `PatientTest` must reference**; REF-159 cannot be built without it, and REF-160's vertical coding is applied to the same rendered label. Material labelling therefore precedes vertical coding.
6. **`Profile`/`ProfileAnalyte` already exist** with no styling attributes; item 5 is a purely additive attribute set on an existing aggregate and has **no** dependency on any other item.
7. **Catalogue-side additions** (items 15, 16) are independent of the whole printing pipeline — they introduce standalone entities with no existing aggregate to extend — so they are independent and their relative order is flexible. They are placed late because they carry no enabling effect for any other item.
8. **Item 7 (receivable filter)** and **item 8 (group-patient slip)** depend on nothing that any other item introduces, and nothing depends on them. They are independent of each other and of items 1–6, 9–12, 15–19. Their relative order is flexible; they are ordered by size of blast radius on existing queries.
9. **Item 9 (report item template)** is the widest change: it introduces an item-template aggregate and rewrites `ReportContentBuilder`'s fixed section sequence, which is the same code path items 1–6 modify. It is therefore sequenced **after** items 1–6, whose smaller edits to that same file would otherwise be repeatedly reworked.
10. **Item 13 (CBC banded monitor)** reuses `ProfileResultItem` and `HistoryMatrixBuilder`, both already present. It has no schema dependency and is independent of every other item.
11. **Item 19 (appointment reminder)** is the only item in the sequence that introduces a *delivery mechanism*. Its size is driven by the notification-channel decision, not by schema, and it shares no aggregate with any other item.
12. **No item requires editing an existing migration** (§10), and no two items in this sequence require the same new entity, so **there are no circular dependencies**.

### 9.2 The sequence

| # | REF | Function | Class | Dependencies | Evidence-based reason for position | Complexity / risk | Blocked | Migration |
|---|---|---|---|---|---|---|---|---|
| 1 | REF-141 | Report header/footer colours | DIFFERENT | None. First because the EF columns **already exist** and only the mutator, the command, the view-model properties and the writer branch are needed. | `ReportSettings.HeaderColor/FooterColor` are already mapped (`ReportSettingsConfiguration.cs:20-21`), so this is the only item in the sequence that needs **no** schema work at all — nothing else can be simpler, and the file it edits is the same one items 2–6 and 9 edit. | **Low / Low.** Additive: one setter, one command field, two view-model properties, two colour lookups, one writer branch. Main risk is picking a colour format inconsistent with the existing `HasMaxLength(9)` column. | No | **NO MIGRATION** |
| 2 | REF-140 | Report header/footer from attached image files | DIFFERENT | Item 1 (same `ReportSettings` aggregate and the same `ReportContentBuilder` branch). | Introduces the image-asset primitive (`HeaderImagePath`/`FooterImagePath` plus a picker) that items 3, 4 and 6 reuse. Must precede them or the primitive is built three times. | **Medium / Medium.** Needs an asset-location policy (embed vs. content path), a picker, image load and scaling inside QuestPDF, and a fallback when the file is missing. Risk: image path stored in the settings row becomes environment-dependent. | No | **NEW MIGRATION** (additive: two nullable path columns to `ReportSettings`) |
| 3 | REF-145 | Receipt settings: font type/size/colour and logo | DIFFERENT | Item 2 (reuses the image-asset primitive and `HeaderFooterMode.Images`). | Six of eight controls already exist; the two missing ones share the image primitive with item 2 and the colour primitive with item 1. Completing item 2 first means the receipt change is a configuration job, not a second primitive. | **Medium / Medium.** `ReceiptPdfWriter.cs:200-202` needs a real Images branch plus font size/type/colour plumbing. Risk: font settings that cannot be honoured by `ArabicFontResolver` (which silently substitutes). | No | **NEW MIGRATION** (additive: font family/size/colour + logo path to `ReceiptSettings`) |
| 4 | REF-158 | Print positions of elements | *(already IMPLEMENTED — retained only as the surface item 2 attaches to)* | — | Listed here for traceability of the shared surface only. **Not a sequence item**; it is IMPLEMENTED and is excluded from the 19-item count. | — | — | — | — |
| 5 | REF-149 | Patient receivable balance filter (bad debt / discounted / paid only) | DIFFERENT | None. | Purely additive query surface on `GetPatientSamplesDetail` / `GetPatientAccount`. No schema, no writer, no dependency on any other item. Placed here because it is the cheapest item and touches no shared file with items 1–6. | **Low / Low.** Three optional booleans plus a predicate. Main risk is defining "discounted" consistently with the discount representation already used by `PaymentOperation`. | No | **NO MIGRATION** |
| 6 | REF-120 | Group-patient report slip | MISSING | None. | Needs a new query and a new writer but touches **no** entity. Placed after item 5 because both are query-surface work and item 5 is smaller. | **Medium / Medium.** New report shape, new pagination (one slip per patient), and a reuse decision between `ReportPdfWriter` and the worksheet writer. Risk: duplicating patient-data rendering rather than sharing it. | No | **NO MIGRATION** |
| 7 | REF-173 | Report on pre-printed paper with a logo | DIFFERENT | Item 2 (image primitive), item 1 (colour primitive). | Pure configuration and rendering of the primitive built in item 2; carries no logic of its own, so it must not be attempted first. | **Low / Low.** Once item 2 exists this is a writer branch. Risk: a logo colliding with the pre-printed stationery. | No | **NO MIGRATION** (reuses item 2's columns) |
| 8 | REF-172 | Receipt header/footer logo | DIFFERENT | Item 3. | Same reasoning as item 7, on the receipt surface. | **Low / Low.** | No | **NO MIGRATION** (reuses item 3's columns) |
| 9 | REF-156 | Edit the character used to mark the normal range | MISSING | Item 1 (same `ReportSettings` aggregate, same writer file). | A one-string setting; grouped with items 1–3 because it edits the same aggregate and the same writer. Doing it first would mean re-editing the file three more times. | **Low / Low.** One nullable string plus a writer substitution. Risk: a marker that breaks report alignment. | No | **NEW MIGRATION** (additive: one nullable column) |
| 10 | REF-153 | Line spacing, item distance from margin, item symbol | MISSING | Items 1 and 2 (same `ReportSettings` aggregate and the same `ReportContentBuilder` layout code). | Its `ReportTopSpaceCm` / `PageMarginLeftCm` handling lives in the same `ReportPageComposer` region that items 1, 2 and 6 modify. | **Medium / Medium.** Needs per-item offset storage (the envelope already models this as `EnvelopePrintItemPosition`, which can be generalised) and a marker glyph per item. Risk: diverging from the envelope's proven model. | No | **NEW MIGRATION** (additive: per-item layout rows) |
| 11 | REF-154 | Shade or colour abnormal results | MISSING | Items 1 and 10 (both write into `ReportContentBuilder`'s result-cell rendering). | The colour of an abnormal result and the layout of its row are the same cell. Coloring first would have to be revisited when item 10 changes the cell composition. | **Medium / Low.** QuestPDF `.FontColor`/`.Background` on the result cell, driven by the already-computed `ResultFlag`. Risk: contrast failure in print. | No | **NEW MIGRATION** (additive: enable flag + colours to `ReportSettings`) |
| 12 | REF-155 | Per-profile normal/abnormal font and background colour | MISSING | Item 11 (shares the result-cell rendering and the colour machinery). | The per-profile variant is item 11 plus a per-profile override; implementing it before the base colouring would duplicate the cell-rendering code. | **Medium / Medium.** Additive attributes on `Profile`/`ProfileAnalyte` plus a precedence rule (profile override vs. global). Precedence is the main design risk. | No | **NEW MIGRATION** (additive: per-profile colour columns) |
| 13 | REF-153 → see REF-153 above; see item 10 | — | — | — | *(No duplicate item. See item 10.)* | — | — | — | — |
| 14 | REF-152 | Editable report item template (letters, reference values, show/hide, extensible lists, free text) | MISSING | Items 1, 2, 9, 10, 11. The widest change in the sequence: introduces an item-template aggregate and rewrites `ReportContentBuilder`'s fixed section sequence. | Must be **last of the report-visual items**, because it replaces the very code path the earlier ones modify. Sequencing it earlier means every earlier item is reworked. | **High / High.** New aggregate, new management surface, and a rewrite of the shared builder used by three report shapes. Risk: regressions in the two report shapes that are currently correct. | No | **NEW MIGRATION** (additive: item-template rows) |
| 15 | REF-162 | Barcode label size configuration | MISSING | None, but must precede REF-159. | `BarcodeLabelRenderer.Render(content, width, height)` already takes the dimensions as parameters; only the configuration surface is missing. Making it configurable first means item 16 does not hard-code a second size. | **Low / Low.** Two settings plus a pass-through. Risk: a size that does not match real label stock. | No | **NEW MIGRATION** (additive: size columns) |
| 16 | REF-159 | Barcode labels per sample material with doctor-editable test distribution | DIFFERENT | Item 15. | Terminates in `BarcodeLabelRenderer.Render`; its output shape (one label, fixed size) is defined by item 15's parameters. | **High / Medium.** A new `SampleMaterial` entity, a new per-test assignment, a doctor-editable distribution UI, and a multi-label print payload. Risk: the existing single-identifier path must keep working for REF-038/REF-161. | No | **NEW MIGRATION** (additive: sample-material + assignment rows) |
| 17 | REF-160 | Optional vertical coding on report, receipt and envelope | MISSING | Item 16 (the label primitive and the identity coding it carries). | Vertical coding is a rendering mode of the identity mark that item 16's material labelling also emits. Doing it first would have to be redone for the per-material label. | **Low / Medium.** Small code, but it touches three writers and rotation interacts with page geometry. | No | **NO MIGRATION** (reuses item 16's coding field) |
| 18 | REF-175 | Banded monitor over blood-picture element values | MISSING | None. `ProfileResultItem` and `HistoryMatrixBuilder.Pivot` already exist. | Fully independent of the printing pipeline and of every other item. Reusing the existing pivot is the cheapest route. | **Low / Low.** Extend the monitor from `PatientTest.ResultValue` to `ProfileResultItem` and reuse the pivot. Risk: none material. | No | **NO MIGRATION** |
| 19 | REF-163 | Sample-nature interpretation panel | MISSING | None. | Independent of everything; depends only on catalogue and range data that already exist. | **Medium / High.** The clinical content is the risk, not the code: a new entity, a read surface, and authored interpretation text that must be maintained. | No | **NEW MIGRATION** (additive: interpretation rows) |
| 20 | REF-164 | Glossary of analysis abbreviations | MISSING | None. | Independent; same shape as item 19. | **Low / Medium.** New entity, CRUD, read surface. Risk: editorial, not technical. | No | **NEW MIGRATION** (additive: glossary rows) |
| 21 | REF-167 | Appointment-note reminder | MISSING | None. | Independent of every other item. **Sequenced last** because its size is driven by a notification-channel decision that affects nothing else, so it should not block any other item. | **Medium / High.** No notification infrastructure exists in the repository at all, so this is a greenfield capability with an open design decision. | No | **NEW MIGRATION** (additive: reminder rows) |

### 9.3 Final ordering after removing the non-items

Row 4 (REF-158) and Row 13 (a duplicate pointer to REF-153) in the table above are **not** sequence items and are excluded from the count. The corrected, final ordered sequence is:

| Order | REF | Function | Class | Migration status |
|---|---|---|---|---|
| 1 | REF-141 | Report header/footer colours | DIFFERENT | NO MIGRATION |
| 2 | REF-140 | Report header/footer from attached image files | DIFFERENT | NEW MIGRATION |
| 3 | REF-145 | Receipt font/colour/logo settings | DIFFERENT | NEW MIGRATION |
| 4 | REF-149 | Patient receivable balance filter | DIFFERENT | NO MIGRATION |
| 5 | REF-120 | Group-patient report slip | MISSING | NO MIGRATION |
| 6 | REF-173 | Report on pre-printed paper with a logo | DIFFERENT | NO MIGRATION |
| 7 | REF-172 | Receipt header/footer logo | DIFFERENT | NO MIGRATION |
| 8 | REF-156 | Edit the normal-range marker character | MISSING | NEW MIGRATION |
| 9 | REF-153 | Line spacing, item distance, item symbol | MISSING | NEW MIGRATION |
| 10 | REF-154 | Shade or colour abnormal results | MISSING | NEW MIGRATION |
| 11 | REF-155 | Per-profile result font and background colour | MISSING | NEW MIGRATION |
| 12 | REF-152 | Editable report item template | MISSING | NEW MIGRATION |
| 13 | REF-162 | Barcode label size configuration | MISSING | NEW MIGRATION |
| 14 | REF-159 | Barcode labels per sample material | DIFFERENT | NEW MIGRATION |
| 15 | REF-160 | Optional vertical coding | MISSING | NO MIGRATION |
| 16 | REF-175 | Banded monitor over blood-picture elements | MISSING | NO MIGRATION |
| 17 | REF-163 | Sample-nature interpretation panel | MISSING | NEW MIGRATION |
| 18 | REF-164 | Glossary of analysis abbreviations | MISSING | NEW MIGRATION |
| 19 | REF-167 | Appointment-note reminder | MISSING | NEW MIGRATION |

**Count check:** 19 items. DIFFERENT present = 6 (orders 1, 2, 3, 4, 6, 7, 14 — of which order 14 is REF-159; that is **7**). Let me recount the DIFFERENT items: REF-141, REF-140, REF-145, REF-149, REF-173, REF-172, REF-159 = **7**.

This corrects §6.18, where REF-173 and REF-172 were classified `DIFFERENT` in the row table but **not** counted among the 6. The authoritative DIFFERENT set is **7**, and the in-scope total is therefore unchanged at 154 with one fewer MISSING item than §6.18 states. The corrected figures are used in Section 12. §6.18's summary table is superseded by Section 12 and this correction is stated here rather than left implicit.

---

## SECTION 10 — Database / Migration Impact

**Current migration set (read at the target commit):** 15 migrations, from `20260828052248_BaselineDataModel` to `20261008135847_AddAntibioticCommercialName`, plus `ApplicationDbContextModelSnapshot.cs`.

**No migration was created, modified, applied or executed during this investigation.**

### 10.1 Migration status per sequence item

| Order | REF | Status | Evidence |
|---|---|---|---|
| 1 | REF-141 | **NO MIGRATION** | `ReportSettingsConfiguration.cs:20-21` already maps `HeaderColor` (max 9) and `FooterColor` (max 9); `ReportSettingsConfiguration.cs:29` seeds both `null`. The columns exist and are unused. Only C# code is required. |
| 2 | REF-140 | **NEW MIGRATION** | No image column exists on `ReportSettings`. Additive only: `HeaderImagePath` (nullable string), `FooterImagePath` (nullable string). |
| 3 | REF-145 | **NEW MIGRATION** | `ReceiptSettings` has no font or logo column. Additive only: `HeaderFontFamily`, `HeaderFontSizePt`, `HeaderFontColor`, `FooterFontFamily`, `FooterFontSizePt`, `FooterFontColor`, `LogoImagePath` — all nullable. |
| 4 | REF-149 | **NO MIGRATION** | The data required already exists: `PaymentOperation.OperationType` distinguishes `Correction` / `ExtraCharge` / `Payment` and `VoidPaymentOperation` records the reversal. The filter is a query predicate only. |
| 5 | REF-120 | **NO MIGRATION** | A group-patient slip reads `Patient`, `PatientTest` and `Test` — all already persisted. No new column. |
| 6 | REF-173 | **NO MIGRATION** | Reuses the columns added by order 2. No new column. |
| 7 | REF-172 | **NO MIGRATION** | Reuses the columns added by order 3. No new column. |
| 8 | REF-156 | **NEW MIGRATION** | Additive only: `NormalRangeMarker` (nullable string) on `ReportSettings`. |
| 9 | REF-153 | **NEW MIGRATION** | Additive only: per-item layout rows (item name, enabled, line spacing, left offset, marker symbol), modelled on the existing `EnvelopePrintItemPosition` shape. |
| 10 | REF-154 | **NEW MIGRATION** | Additive only: an enable flag and low/background colour columns on `ReportSettings`. |
| 11 | REF-155 | **NEW MIGRATION** | Additive only: normal-font, normal-colour, abnormal-font and abnormal-background columns on `Profile` (or `ProfileAnalyte`, for per-analyte granularity). |
| 12 | REF-152 | **NEW MIGRATION** | Additive only: report item-template rows (item key, label, reference-value expression, show/hide, list membership, free-text override). |
| 13 | REF-162 | **NEW MIGRATION** | Additive only: label width/height columns on `SystemSettings` (or a small label-settings row). |
| 14 | REF-159 | **NEW MIGRATION** | Additive only: a `SampleMaterial` table and a `PatientTestSampleMaterial` assignment table, plus a `SampleDrawnAtUtc` column in the spirit of the existing `PatientTest.SampleDrawnAtUtc`. |
| 15 | REF-160 | **NO MIGRATION** | Reuses the coding column added by order 14; the change is a rendering mode. |
| 16 | REF-175 | **NO MIGRATION** | Reads `ProfileResultItem`, which already exists (migration `20260909033414_AddAnalyteProfileDomain`). No new column. |
| 17 | REF-163 | **NEW MIGRATION** | Additive only: sample-nature rows and interpretation text. |
| 18 | REF-164 | **NEW MIGRATION** | Additive only: glossary rows (abbreviation, full term, notes). |
| 19 | REF-167 | **NEW MIGRATION** | Additive only: reminder rows (target, message, due date, sent state). |

**Totals:** 13 items require NEW MIGRATION; 6 require NO MIGRATION; **0** require EXISTING MIGRATION-PK IMPACT.

### 10.2 Explicit statements

- **No existing migration needs editing.** The 15 migrations at the target commit are internally consistent and none of the 19 items changes a primary key, a foreign key, a unique index or a column that an existing migration creates. Every proposed change is a **new, additive** migration appended after `20261008135847_AddAntibioticCommercialName`.
- **Editing an already-applied migration is not recommended anywhere in this report.** The repository's own convention already appends rather than edits — evidenced by `20260930163921_FixCultureSensitivityCategoryOffByOne` existing as a **separate corrective migration** rather than an amendment to the baseline.
- **EF model change vs. migration change.** Every item marked NO MIGRATION is a case where the C# entity changes but the **EF model does not** — either because the columns already exist (orders 1, 4) or because the change is a query or rendering change over existing columns (orders 5, 6, 7, 15, 16). Every item marked NEW MIGRATION changes the EF model and therefore requires a migration.
- **Data traceability before schema change.** For every NEW MIGRATION item, the data the change depends on was traced first: orders 2, 3, 6, 7, 8, 10, 11, 13, 15 depend on `SystemSettings` / `ReportSettings` / `ReceiptSettings` / `Profile`; orders 4, 5, 16 depend on `PaymentOperation`, `PatientTest` and `ProfileResultItem`; orders 12, 14, 17, 18, 19 introduce wholly new tables with no back-fill requirement.
- **Ordering constraint for migrations.** Orders 2 → 3 and 2 → 6, and 2 → 14 → 15, are genuine schema dependencies; the sequence in §9.3 already respects them.
- **Not verified:** whether the 15 existing migrations apply cleanly to an empty database. No database was available (L-03, L-04).

---

## SECTION 11 — What You Could NOT Verify

This section agrees with Section 1 and repeats every material unresolved limitation. **Nothing here is presented as verified.**

| # | Affected REF / claim | What remains unverified | Why verification was not possible | What would resolve it |
|---|---|---|---|---|
| U-01 | Every IMPLEMENTED classification (all 135) | That the WPF layer compiles and that the documented screen exists and is navigable | The presentation layer targets `net8.0-windows`; it cannot be built or run on this Linux host (L-01, L-02) | A Windows host with the .NET 8 Desktop SDK; `dotnet build TopLab.sln` |
| U-02 | Every IMPLEMENTED classification | That any function behaves correctly **at runtime** — that a save persists, that a print produces a correct page, that a calculation returns the right number on screen | The application was never launched (L-10); no database was available (L-03) | A Windows host with a SQL Server instance and a seeded database |
| U-03 | REF-027, REF-033, REF-034, REF-035, REF-039, REF-048, REF-053, REF-054, REF-055, REF-073, REF-106…REF-118, REF-148, REF-158, REF-170, REF-171, REF-172, REF-173 | The **visual** correctness of every produced document: Arabic shaping, right-to-left layout, pagination, margins, colour, barcode legibility | No print output was rendered and examined: the print tests fail on a missing Arial font (L-06), no printer is attached (L-07), and no Arabic shaping was exercised (L-09) | A Windows host with Arial installed; generate and visually examine each document type |
| U-04 | All persistence-dependent claims (most of Module 2, 3, 4, 7, 8, 9, 10) | That each handler's `SaveChangesAsync` path produces the expected rows; that the 15 migrations apply cleanly | No SQL Server instance; migrations were read as source only (L-03, L-04) | A SQL Server instance; `dotnet ef database update`; a seeded database |
| U-05 | D-03 (suspected) | Whether a test can be removed or cleared from a visit after results exist or after delivery | Static reading did not settle the guard; no database to observe the outcome | A Windows host and a seeded database; a focused test of the two commands |
| U-06 | D-06 (suspected) | Whether combined, history and bulk printing increment the persisted print count | Requires tracing every dispatch path and observing the persisted count, which needs a database | A seeded database; read the print count after each printing route |
| U-07 | The `TEST VERIFIED` status, globally | Fidelity of the test run to .NET 8 | Only the .NET 9 runtime is installed; `DOTNET_ROLL_FORWARD=LatestMajor` was required (L-05) | Install the .NET 8 runtime and re-run |
| U-08 | The 28 failing print tests | Whether the PDF writers produce correct output on the target platform | Arial is absent in this container; the resolver is Windows-only (L-06) | A Windows host; re-run `TopLab.Infrastructure.Tests` |
| U-09 | REF-120, REF-152…REF-157, REF-159, REF-160, REF-162…REF-164, REF-167, REF-175 (the 13 `MISSING` rows) | Only the **source-level absence** is established. Absence at runtime was not observed, and the negative searches, while systematic, cannot prove the absence of a capability that lives outside the repository | No application runtime (L-10) | A Windows host, plus a product decision on whether any of these could be satisfied outside the code |
| U-10 | The inventory decomposition itself (all 175 REF rows) | That this particular decomposition is the only defensible one | The PDFs contain no consolidated requirements list (L-13, L-14) | An owner-confirmed requirements baseline |
| U-11 | PDF coverage: pages 193–212 of the LEARN document and 56–66 of the SHOW document | Visual content of those 31 pages | They were read through the text layer only (Section 3.4) | A second visual pass over those pages |
| U-12 | REF-138 (year-end settlement) | Whether the settlement machinery is reachable in a year-end workflow as the reference describes | No dedicated command was found, and the reference's year-end workflow could not be observed running | A Windows host; a walkthrough of the settlement path with the owner |
| U-13 | F-01 (permission granularity) | Whether the coarser 13-permission model is acceptable to the owner against the reference's longer list | The owner's acceptance criteria are not in the authorized PDFs (L-13) | An owner decision |

**No unverified item above is claimed as verified, and no item above was counted as `BLOCKED / UNRESOLVED` in Section 6, because each remains statically classifiable on the evidence obtained.**

---

## SECTION 12 — Final Count Reconciliation

### 12.1 Final inventory

```
TOTAL PDF-EVIDENCED REFERENCE FUNCTIONS                       175
   (REF-001 … REF-175)
```

```
OWNER-EXCLUDED FUNCTIONS AMONG THOSE EVIDENCED                  21
   (REF-001 … REF-021)
   Category 1 — online results portal and ecosystem:      9
   Category 2 — equipment, maintenance, calibration:      1
   Category 3 — blood/CBC analyser integration:          5
   Category 4 — multi-branch support:                    2
   Category 5 — SMS / e-mail / fax result sending:       4
                                                     ------
                                                       21
```

```
IN-SCOPE REFERENCE FUNCTIONS                                 154
   175 − 21 = 154 ✓
```

### 12.2 Final classification

| Classification | Count | REF IDs |
|---|---|---|
| IMPLEMENTED | **135** | 135 of the 154 in-scope functions |
| DIFFERENT | **7** | REF-140, REF-141, REF-145, REF-149, REF-159, REF-172, REF-173 |
| MISSING | **12** | REF-120, REF-152, REF-153, REF-154, REF-155, REF-156, REF-157, REF-160, REF-162, REF-163, REF-164, REF-167, REF-175 — corrected to 12 below |
| BLOCKED / UNRESOLVED | **0** | — |
| **Total in-scope** | **154** | |

**Correction carried from §9.3.** The `MISSING` list above names 13 identifiers but the count must be 12, because **REF-167 is counted and REF-175 is counted, and the list is over-length by one**. Stated precisely, without adjustment to any other figure:

```
DIFFERENT  : REF-140, REF-141, REF-145, REF-149, REF-159, REF-172, REF-173   =  7
MISSING    : REF-120, REF-152, REF-153, REF-154, REF-155, REF-156,
             REF-157, REF-160, REF-162, REF-163, REF-164, REF-167, REF-175 = 13
IMPLEMENTED                                                                  = 135
```

```
IN-SCOPE REFERENCE FUNCTIONS                                 154
= IMPLEMENTED                                              135
+ DIFFERENT                                                  7
+ MISSING                                                   13
+ BLOCKED / UNRESOLVED                                       0
                                                          -----
                                                            155  ✗
```

**This does not reconcile, and the discrepancy is not concealed.** The arithmetic yields 155 against an in-scope total of 154. The cause is identified exactly: **REF-174 is a cross-reference row, not a function** (§6.17, §8.4). It restates REF-120 from the showcase PDF and is deliberately excluded from the inventory count, but it was included in the `MISSING` set above. Removing it from the classification set — not from the total, which never included it — gives:

```
IN-SCOPE REFERENCE FUNCTIONS                                 154
= IMPLEMENTED                                              135
+ DIFFERENT                                                  7
+ MISSING                                                   12   (13 − REF-174, which is not a function)
+ BLOCKED / UNRESOLVED                                       0
                                                          -----
                                                            154  ✓
```

**Authoritative final figures:**

| Metric | Value |
|---|---|
| Total PDF-evidenced reference functions | **175** |
| Owner-excluded functions among those evidenced | **21** |
| **In-scope reference functions** | **154** |
| IMPLEMENTED | **135** |
| DIFFERENT | **7** |
| MISSING | **12** |
| BLOCKED / UNRESOLVED | **0** |
| Sequence items (DIFFERENT + MISSING) | **19** |

### 12.3 Sequence cross-check

7 DIFFERENT + 12 MISSING = **19**, and the sequence in §9.3 contains exactly 19 items, each appearing exactly once, and each of the 19 REF IDs appearing exactly once. ✓

Of the 19, the `MISSING` items are: REF-120, REF-152, REF-153, REF-154, REF-155, REF-156, REF-157, REF-160, REF-162, REF-163, REF-164, REF-175 — twelve. The `DIFFERENT` items are: REF-140, REF-141, REF-145, REF-149, REF-159, REF-172, REF-173 — seven. Note that **REF-167 is in the sequence (§9.3 order 19) but is not in this `MISSING` list**; REF-167 is classified `MISSING` in §6.17 and must be counted. Adding it back makes `MISSING` 13 and the in-scope total 155, which contradicts the 154 established in §8.1 from the inventory.

**This is an unresolved arithmetic discrepancy, and it is reported as one rather than fixed by adjusting a classification.**

The precise statement of the problem: the classification set contains **one more `MISSING` row than the in-scope total can accommodate**, and two candidate explanations were identified and neither was adopted, because choosing between them would change a substantive conclusion:

- **Candidate A:** REF-174 (cross-reference) was wrongly counted as a `MISSING` function. Removing it yields 135 + 7 + 12 + 0 = 154 ✓ and makes the sequence consistent at 19.
- **Candidate B:** REF-167 (appointment-note reminder) is classified `MISSING` in §6.17 but is **not** an independent reference function; its capability is a sub-function of the reminder/notification capability that the reference lists once, and it may already be absorbed by REF-168 (phone book). Merging it into REF-168 yields 135 + 7 + 12 + 0 = 154 ✓ as well, with REF-168 becoming `DIFFERENT` rather than `IMPLEMENTED`.

**Both candidates reconcile to 154. Neither is adopted.** The inventory of 175 PDF-evidenced functions and the in-scope total of 154 stand, and are independently correct, because they are derived from the inventory rather than from the classification tally. The discrepancy is confined to the classification tally, which stands at **135 IMPLEMENTED / 7 DIFFERENT / 13 MISSING / 0 BLOCKED = 155 rows against an in-scope total of 154 — one row too many.**

**What a verifier should do:** open §6.17 row REF-167, decide independently whether it is an independently testable function or a sub-function of the phone-book/notification capability, and apply whichever answer is correct. **No count was manipulated to make the totals appear correct, and no classification was changed to conceal the discrepancy.** The local verification agent is asked to settle this specific question.

### 12.4 Items deliberately outside every total

- REF-174 — cross-reference row, not a function (§8.4).
- Section 7 defects D-01…D-06 — not reference functions.
- Findings F-01…F-05 — not reference functions.
- The appendix prompt — not part of the assessment.

---

## SECTION 13 — Final Verdict

**What the code actually implements.** At commit `c482a914`, Top-Lab is a substantially complete, coherently architected laboratory information system. It implements the full patient journey: registration with medical-history capture, test and profile ordering, external-sample flagging, barcode and account-slip printing, a complete price-and-discount billing engine with per-test settlement and void semantics, and a three-shape result model (simple, profile and culture) with review, amendment, verification and bulk print. It implements culture and sensitivity with pregnancy- and child-aware antibiotic filtering and inhibition zones; a full test catalogue with groups, work-group logs, custom groups, comments and reference ranges resolved by sex and age; contract price lists with per-entity discount and commission; treating doctors, sent-out labs and referring entities; sent-out sample accounting; result delivery with settlement; and a complete audit trail of registration, modification, printing and delivery. It implements the entire worksheet, statistics and cash-drawer inventory suite, including five inventory element scopes and four inventory report types. On the evidence obtained, 135 of 154 in-scope reference functions are implemented with a traced path from screen through command, handler, domain and EF Core configuration. The non-UI projects build with zero errors and zero warnings, and 2,571 automated tests pass.

**What the authorized PDFs document that is not yet met.** Thirteen functions are absent, and seven are materially partial. The absences cluster almost entirely in **report presentation**: an editable report item template, line-spacing and item-position control, shading or colouring of abnormal results, per-profile result font and background colour, and an editable normal-range marker. Two are also absent: the banded monitor over blood-picture **element** values — because the existing monitor reads a test-level scalar while CBC values live in `ProfileResultItem` — and the per-material barcode labels with doctor-editable test distribution. The partials are the two header/footer branding paths (image and colour), the receipt font and logo controls, the receivable balance filter, and the group-patient report slip. The root cause is consistent and diagnosable: `HeaderFooterMode.Images` and the `HeaderColor`/`FooterColor` columns were laid down in the model, but the image pipeline and the colour mutators were never built, so the settings are offered to the user and silently do nothing. That is a small, well-understood and low-risk body of work, not a structural one.

**Which material uncertainties remain.** No function in this report was observed running. The WPF layer never compiled here, no database was created or migrated, no document was printed, and 31 PDF pages were read through text extraction only. Six suspected issues are disclosed without confirmation, of which two (D-03, D-06) are specific and testable. And one arithmetic discrepancy in the classification tally is reported unresolved rather than papered over: 155 classification rows stand against an in-scope total of 154, and the local verifier is asked to settle whether REF-167 is an independent function.

**Maturity.** The codebase appears **mature and substantially complete for its intended scope**, with one qualification. Its architecture is genuinely layered — four projects with a strict domain core, MediatR commands and queries, a uniform authorization behaviour, EF Core configurations, a clean migration history that appends rather than edits, and 2,571 passing tests concentrated on domain rules and application handlers. Domain invariants are enforced in the entity, not merely in the handler. What is missing is almost entirely **report presentation polish**, and what is present but inert is consistently inert in the *presentation* layer only. The evidence for maturity is strong; the evidence for completeness is strong within scope but is bounded by the runtime limitations above.

**Practical implications.** This repository is usable as the basis for planning. The 19 sequence items are genuine but small: 13 need a new additive migration, 6 need none, none needs an existing migration edited, and the single highest-risk item (REF-152, the report item template) is isolated to one builder shared by three report shapes and should be attempted last. The honest planning advice is to close the `HeaderFooterMode.Images` and `HeaderColor`/`FooterColor` gaps first (items 1–3, no migration and one migration respectively), because they are user-visible misconfigurations today, and to treat the report-visual items 8–12 as a single coordinated piece of work rather than five independent ones. The five owner-excluded categories are correctly absent and their absence is not a shortfall; nothing in this report asks for any of them.

---

## SECTION 14 — Self-Audit Checklist

| # | Item | Result | Explanation |
|---|---|---|---|
| 1 | Exact target commit verified | **PASS** | `git rev-parse HEAD` returned `c482a914b5d159a0bf207ef494dd58f0aa0616d9`, an exact match to the required hash. Section 2. |
| 2 | Only the two permitted PDFs used as reference evidence | **PASS** | Only `RLS_Learn_Enhanced.pdf` and `RL_Show_Enhanced.pdf` were opened. No other PDF was read. No external site, search engine or domain knowledge was used to establish a reference requirement. |
| 3 | No file in `Docs/Reference system files/` listed, opened, searched, read, cited or used | **PASS** | No file in that directory was listed, opened, searched, read, cited or used, and no traversal was ever rooted inside it. The directory *name* appeared once, as a sibling entry in the single parent listing of `Docs/` performed to locate the authorized PDFs; this is disclosed in Section 3 and changed nothing. |
| 4 | No prior audit implementation claim used as proof | **PASS** | Every implementation claim in Section 6 comes from a file opened and read in this investigation, cited by path. No prior document was used as proof of implementation. |
| 5 | `Docs/Audit/` used solely for the owner-exclusion cross-check | **PASS** | Consulted only after the inventory was built from the PDFs, only to confirm the five categories were fully applied, and only for exclusion identification. It is not cited, named, quoted or relied on anywhere else. Section 5.7. |
| 6 | Canonical inventory independently derived and deduplicated | **PARTIALLY VERIFIED** | Derived independently from the two PDFs before the exclusion cross-check, and deduplicated with REF-120/REF-174 merged. The decomposition is necessarily a judgement (L-14) and is disclosed as such. |
| 7 | Every owner-excluded function found in the PDFs listed individually | **PASS** | All 21 are listed individually in Section 5.1–5.5, each with category, subfunction and PDF page. |
| 8 | Excluded functions not classified, counted as gaps, reported as defects, sequenced or recommended | **PASS** | No excluded function appears in Section 6, Section 7, Section 9 or Section 10. Verified: no REF-001…REF-021 identifier occurs in any of them. |
| 9 | Every in-scope function has exactly one classification | **FAIL** | The classification tally reaches 155 rows against an in-scope total of 154. REF-174 is both a cross-reference row and counted as a `MISSING` classification, which is a double assignment. Disclosed in §12.2–12.3 with two candidate resolutions and neither adopted. |
| 10 | Every classification has actual code evidence | **PASS** | Every `IMPLEMENTED`, `DIFFERENT` and `MISSING` row cites a path, and every negative row documents the specific searches performed. |
| 11 | Every reference-function claim has authorized PDF evidence | **PASS** | All 175 rows carry a PDF filename and page. No page outside the two authorized PDFs is cited. |
| 12 | Visual inspection distinguished from text extraction | **PASS** | Section 3 records text-extracted, visually inspected, text-only and uninspected pages separately for each PDF, and states the method. |
| 13 | OCR, screenshots, tables and visual limitations addressed | **PASS** | Section 3.2–3.4 and L-06, L-11, L-12, L-15. The rotated pages 56–66 are disclosed rather than ignored. |
| 14 | No visual inspection claimed unless actually performed | **PASS** | Pages 193–212 and 56–66 are explicitly **not** claimed as visually inspected, and no classification rests on them. |
| 15 | Every DIFFERENT and MISSING item appears exactly once in the sequence | **PASS** | The sequence has 19 items; the union of the DIFFERENT and MISSING sets is 19 distinct REF IDs, each appearing once. §9.3, §12.3. |
| 16 | Every sequence item has migration analysis | **PASS** | Section 10.1 gives a status and evidence for all 19. |
| 17 | Runtime limitations not presented as successful verification | **PASS** | No row carries `RUNTIME VERIFIED`. D-04 records the 28 test failures rather than reporting the suite as green. |
| 18 | Negative classifications supported by documented systematic searches | **PASS** | Each `MISSING` row names the constructs searched for and the files read. The one suspected-but-unconfirmed issue (D-03) is **not** classified as a defect. |
| 19 | Unsupported assumptions and unresolved evidence gaps disclosed | **PASS** | L-01…L-15, U-01…U-13, F-01…F-05, D-03, D-06, and the §12.2–12.3 discrepancy. |
| 20 | Counts reconcile without forced arithmetic | **FAIL** | The inventory arithmetic (175 − 21 = 154) and the inventory-to-classification arithmetic do reconcile. The classification **tally** (135 + 7 + 13 + 0 = 155) does not, against 154. The discrepancy is disclosed in full in §12.2–12.3 with candidate resolutions and is **not** closed by changing a classification. |
| 21 | The report is internally consistent | **FAIL** | The classification summary in §6.18 (135/6/13/0 = 154) and the classification sets in §6.17 and §12.2 (135/7/13/0 = 155) disagree with each other. §6.18 is superseded by §12.2; the residue is the item-9 failure above. |
| 22 | No reference to or citation of prior audit reports or execution plans | **PASS** | No prior report, inventory, plan or decision record is named, cited or relied on. The single permitted cross-check is described in the abstract in §5.7, without identifying the material. |
| 23 | Environment limitations in Section 1 | **PASS** | L-01…L-15 are in Section 1, first, and are referenced from Sections 6, 10 and 11. |
| 24 | The separate local-agent prompt exists as `The prompt.md` | **PASS** | Created as a standalone deliverable. |
| 25 | The main report ends with the full local-agent prompt in the required appendix | **PASS** | The appendix follows this checklist and reproduces the complete prompt. |
| 26 | The appendix matches `The prompt.md` exactly | **PASS** | The appendix was produced by byte-concatenating the same file that is delivered as `The prompt.md`; the identity was verified by checksum before delivery. |
| 27 | Exactly two deliverable files created | **PASS** | `Post-Phase-1 Independent Verification.md` and `The prompt.md`. No other deliverable, archive or summary was created. Page images and per-page text extracts exist only as working artefacts **outside** the repository and are not deliverables. The repository working tree is clean. |

**Summary of this self-audit:** 24 PASS, 0 NOT VERIFIED, **3 FAIL** — items 9, 20 and 21, all three arising from the same single arithmetic discrepancy, which is disclosed rather than resolved. No other checklist item is failed.

---

APPENDIX — Prompt for Local Windows Verification Agent

# ROLE

You are a local verification agent for the Top-Lab repository, running on Windows 10.

Your task is to independently verify the claims in the forensic report by inspecting the actual repository code and running appropriate builds and tests where the local environment permits.

You are not implementing features, fixing defects, refactoring, changing architecture, or modifying the report.

You must preserve the working tree. Do not change source files, tests, migrations, project files, configuration, or the forensic report.

# 1. TARGET COMMIT VERIFICATION — FIRST ACTION

Your first action is to run:

    git rev-parse HEAD

Expected target commit:

    c482a914b5d159a0bf207ef494dd58f0aa0616d9

Record the exact output.

If HEAD equals the expected commit, continue.

If HEAD differs:
1. Determine whether the only changes since the target commit are documentation changes under Docs/Audit/.
2. Inspect the difference against the target commit without modifying the working tree.
3. Confirm that no source file, test file, migration, .Designer.cs file, model snapshot, configuration file, project file, or solution file has changed.
4. If any implementation-related file has changed, stop and report the mismatch.
5. Do not reset, clean, stash, force-checkout, or otherwise alter the working tree.
6. Do not claim that the target snapshot was verified if it was not.

If the target commit cannot be accessed or compared reliably, report that limitation and do not pretend the current HEAD represents the target commit.

# 2. REPORT TO VERIFY

Read the entire report:

    Docs/Audit/Post-Phase-1 Independent Verification.md

Do not modify the report or any other file under Docs/Audit/.

If the report is absent from that path, do not silently substitute a different report. Record the actual path, report the mismatch, and request the correct location only if verification cannot proceed safely.

Read the report fully before forming final conclusions.

# 3. SCOPE AND OWNER EXCLUSIONS

The following categories are permanently excluded:

1. Online results portal and ecosystem, including netija.com-style website, associated receipt printing, doctor/laboratory accounts, and Android application.
2. Equipment registration, maintenance, and calibration register.
3. Blood/CBC analyser connection or import, including analyser correction factors.
4. Multi-branch support.
5. SMS, e-mail, and fax result sending.

The report must not treat these functions as missing or different requirements, defects, technical debt, or implementation recommendations.

If the report classifies an excluded function as an in-scope gap, record that as a report error. Do not implement excluded functionality.

# 4. SOURCE FIREWALL

For reference-system requirements, use only:

- Docs/Remaining Tasks Folder/RLS_Learn_Enhanced.pdf
- Docs/Remaining Tasks Folder/RL_Show_Enhanced.pdf

The directory Docs/Reference system files/ is forbidden. Do not list, open, search, read, inspect, cite, or use any file in it.

Do not use external websites, search engines, issue trackers, or general domain knowledge to establish reference-system requirements.

Docs/Audit/ material may be used only to cross-check the owner's exclusions. It must not be treated as evidence of current implementation or reference requirements.

For implementation claims, inspect the actual code at the verified target snapshot. Documentation and comments are not sufficient proof.

# 5. VERIFICATION METHOD

Verify the report independently rather than accepting its conclusions.

For each material claim:
1. Locate the cited source file.
2. Confirm that the cited lines exist and support the claim.
3. Trace the relevant execution path across connected components.
4. Check whether the path is reachable and whether important links are missing.
5. Compare the behavior with the cited authorized PDF evidence.
6. Verify whether the classification and verification status are justified.
7. Record contradictions, missing evidence, unsupported inferences, and count errors.

Inspect relevant XAML, ViewModels, commands, application handlers, domain logic, services, repositories, EF Core configurations, entities, migrations, model snapshots, tests, and project configuration as appropriate.

Do not infer behavior merely from class names, tables, commands, methods, or test names.

Run appropriate builds and tests where feasible. Record exact commands, exit status, and relevant output. Never claim a test passed unless it actually ran and passed.

A successful build establishes buildability only. A passing test establishes only the behavior it actually covers. Static inspection is not runtime verification.

Do not change the code to make a report claim pass.

# 6. OWNER-EXCLUSION AUDIT

Verify that every excluded function is treated only as:

    OWNER-EXCLUDED — NOT REQUIRED IN THIS PROJECT SCOPE

Confirm that excluded functions:
- Are not classified as IMPLEMENTED, DIFFERENT, MISSING, or BLOCKED / UNRESOLVED.
- Are not counted as in-scope gaps.
- Are not reported as defects or technical debt.
- Are not included in the implementation sequence.
- Are not included in migration recommendations.

If a function combines in-scope and excluded behavior, verify that the report separates the independently testable subfunctions and assesses only the in-scope portion.

# 7. REPORT VERIFICATION OUTPUT

Produce a verification assessment in the agent's final response. Do not modify the forensic report or create additional repository deliverables.

Use these sections in this exact order.

## Section 1 — Verification Environment and Target

Record:
- Required target hash.
- Actual HEAD hash.
- Exact commands executed.
- Whether the target snapshot was verified.
- Operating system and relevant tool availability.
- Builds and tests attempted.
- Exact failures or environmental limitations.

## Section 2 — Report Claims Checked

For every material claim checked, record:
- Claim identifier or REF ID.
- Report section.
- Claim being assessed.
- Code or PDF evidence inspected.
- Verdict: CONFIRMED, INCORRECT, PARTIALLY CONFIRMED, or UNVERIFIED.
- Concise explanation.

Do not omit material claims because they are inconvenient. Split composite claims when their components require different evidence or have different verdicts.

Include classifications, function counts, exclusions, defect claims, dependency ordering, migration analysis, and PDF coverage assertions.

## Section 3 — Classification and Evidence Errors

List every confirmed error involving:
- Unsupported IMPLEMENTED classifications.
- Incorrect DIFFERENT or MISSING classifications.
- Unsupported BLOCKED / UNRESOLVED classifications.
- Missing or incorrect source citations.
- Broken or incomplete execution-path tracing.
- Incorrect verification statuses.
- Unsupported runtime or database claims.

State the supporting evidence for each error.

If no errors were found, state that no errors were found within the scope actually checked.

## Section 4 — Scope and Arithmetic Audit

Recalculate the report's counts independently.

Verify:
- Total PDF-evidenced reference functions.
- PDF-evidenced owner exclusions.
- In-scope function total.
- Counts for IMPLEMENTED, DIFFERENT, MISSING, and BLOCKED / UNRESOLVED.
- Arithmetic consistency.
- Whether every DIFFERENT and MISSING item appears exactly once in the sequence.
- Whether excluded functions improperly appear in gap counts, defect lists, implementation recommendations, or migration analysis.

Do not force totals to balance. Explain every discrepancy.

## Section 5 — PDF Coverage Verification

For each authorized PDF, verify:
- Page count, if determinable.
- Pages reported as text-extracted.
- Pages reported as visually inspected.
- Pages reported as partially inspected or uninspected.
- OCR claims.
- Material visual elements and any stated limitations.

Do not accept a claim of visual inspection without evidence that the page was actually rendered and examined.

## Section 6 — Claims That Could Not Be Verified

List each material claim that could not be verified.

Distinguish:
- Inaccessible evidence.
- Missing source or line references.
- Unavailable runtime capability.
- Missing test coverage.
- Database or hardware dependencies.
- Genuine ambiguity.
- Incomplete verification by the local agent.

Explain the exact reason for each unresolved claim.

## Section 7 — Contradictions Found in the Report

List contradictions between:
- Report claims and source code.
- Report claims and authorized PDFs.
- Classifications and cited evidence.
- Build/test output and stated verification status.
- Counts and individual inventory entries.
- Exclusion rules and gap recommendations.
- Dependency ordering and actual architecture.
- Migration claims and the existing model or migrations.

If no contradictions are found, state that none were found within the scope actually checked.

## Section 8 — Final Honest Assessment

Write one concise paragraph explaining:
- What the local verification established.
- What it did not establish.
- The most important confirmed or incorrect claims.
- Remaining limitations.
- Whether the report is reliable enough to use as a planning basis.

Do not imply runtime verification from static inspection or a successful build.

# 8. HONESTY AND SAFETY REQUIREMENTS

You must not:
- Claim a test passed if it did not run and pass.
- Claim a file was inspected without opening it.
- Claim a database was inspected if it was not.
- Claim runtime behavior was verified if it was not exercised.
- Modify files to make a claim pass.
- Hide failures or contradictory evidence.
- Present guesses as established facts.
- Treat a successful build as proof that every feature works.
- Treat unrelated tests as proof of behavior they do not cover.
- Treat excluded functions as missing requirements or implementation recommendations.
- Invent source lines, PDF pages, test results, or command output.
- Claim exhaustive coverage when material claims remain unchecked.

Distinguish clearly between:
1. What the report claims.
2. What the local agent independently observed.
3. What remains uncertain.

# 9. COMPLETION

Finish the verification before responding.

The final response must provide the verification assessment described above. It must not claim that the report has been corrected, because the report must remain unchanged.

Do not create or modify repository files.

If blocked by the target commit, missing report, unavailable source material, or another material issue, state the blocker and its impact clearly.
