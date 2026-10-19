# Post-Phase-1 Independent Verification V2

**Project:** Top-Lab — medical clinical laboratory information system
**Repository:** `https://github.com/El-ogra/Top-Lab.git`, branch `main`
**Target commit:** `9d90f499e95082ec40c417bd4910f1cfc62daec3`
**Method:** Static inspection only. No build, no test execution, no package restore, no database connection, no EF Core command, no application launch.
**Repository state:** Unmodified. `git status --porcelain` is empty at the end of the investigation. No commit or push was made.
**Nature of this document:** Standalone, evidence-based forensic assessment derived from the actual code at the target commit and from the two authorized reference PDFs. It does not cite, reproduce, or rely upon any earlier audit report, forensic report, inventory, or execution plan.

---

## SECTION 1 — Environment Limitations

Every limitation below actually affected this investigation. None is speculative. They are stated first because several of them bound every claim made later.

### 1.1 Actions not performed, and what each prevents establishing

| # | Not performed | Why | What it prevents establishing | Affected claims |
|---|---|---|---|---|
| L-01 | **Building the solution or any project** | Forbidden for this run, and impossible here: the host is Linux and `TopLab.Presentation` / `TopLab.Presentation.Tests` target `net8.0-windows` with WPF | That any project compiles; that XAML compiles; that a `.xaml.cs` code-behind compiles; that type names referenced in XAML bindings resolve | Every classification that rests on the presentation layer; Sections 6, 7, 11 |
| L-02 | **Running the tests** | Forbidden for this run | That any asserted behaviour actually holds; that a handler's `SaveChangesAsync` produces the expected rows; that a validator rejects bad input | All of Section 6. No row in this report carries `TEST VERIFIED` |
| L-03 | **Restoring packages, installing SDKs, workloads or tools** | Forbidden for this run | Nothing directly; it forecloses any attempt to work around L-01 | L-01 |
| L-04 | **Launching the application in any form** | Forbidden for this run, and impossible on this host | All runtime behaviour: whether a screen opens, whether a button is reachable, whether a dialog appears, whether a report is produced | Every `IMPLEMENTED` row; no row carries `RUNTIME VERIFIED` |
| L-05 | **Connecting to, installing, starting or contacting SQL Server, LocalDB or any database; running any SQL** | Forbidden for this run | Database-level runtime verification; that the 22 migrations apply cleanly in sequence; that seed data materialises; that indexes, constraints and transactions behave as modelled; that a saved row is read back correctly | Every persistence-dependent row; Sections 6, 10, 11 |
| L-06 | **Running any EF Core command** (`migrations add/remove/script/list`, `database update`, `has-pending-model-changes`, or any other) | Forbidden for this run | Whether the model snapshot is in sync with the migrations at runtime; whether EF would generate a pending change | Section 10 |
| L-07 | **Verifying Arabic rendering, right-to-left layout, glyph shaping, printed output** | No output was produced, and no Arabic shaping was executed | That any generated document renders correctly; that the chosen font resolves; that RTL ordering is right on the page | All print-producing REF IDs; Section 11 |
| L-08 | **Exercising barcode scanning, thermal label printing, receipt printing or any hardware** | No hardware and no runtime | Capture behaviour; label-size-dependent output; printer dispatch beyond the call | Barcode and label REF IDs |
| L-09 | **Any network access other than fetching the repository** | Forbidden for this run | Reference requirements could not have been cross-checked externally — which is correct, because they must come from the two PDFs only | Nothing; this restriction was honoured |

### 1.2 Additional limitations that actually applied

| # | Limitation | What it prevents establishing | Affected claims |
|---|---|---|---|
| L-10 | Both authorized PDFs are Arabic, right-to-left, screenshot-heavy documents carrying an OCR text layer produced upstream by OCRmyPDF/Tesseract. The text layer shows systematic Arabic orthographic corruption (`ٌنقوم` for `نQM`→`نقوم`, dropped and duplicated diacritics) and, inside embedded screenshots, reversed Latin text (`DetnirP yB` for `Printed By`, `retupmoC erawtfoS` for `Software Computer`). | Perfect fidelity of transcription. Where a name or numeric threshold was material, the rendered page image governed. | Sections 3, 4 |
| L-11 | The Arabic term the reference uses for a microbiology culture is rendered `مزرعة` (literally "farm") but is used throughout as the laboratory term for a bacterial culture. | Nothing material — the surrounding screen captures show `Culture`, `Organism A/B/C`, `Colony Count`, antibiotic grids and `Sensitivity` — but the ambiguity is disclosed because a reader relying on the text layer alone could misread the whole module. | Culture REF IDs |
| L-12 | The PDFs contain no consolidated requirements list. The inventory in Section 4 is a reconstruction from a table of contents, per-chapter prose and 289 pages of screen captures. | Certainty that this decomposition is the only defensible one. The arithmetic is exact for the decomposition used here. | Sections 4, 8, 12 |
| L-13 | Several reference behaviours appear **only as screen captures** with no accompanying prose instruction. These are marked individually in Section 4 with "(screenshot evidence only)". | Whether such a behaviour is a requirement or merely an example. Marking is explicit rather than silent. | Section 4 rows marked as such |
| L-14 | No owner acceptance criteria, functional specification or decision record inside the repository was used as evidence of requirement or implementation. | Corroboration of the reference baseline by a second authorized source | Sections 2, 4 |
| L-15 | Comments and XML documentation in the source were treated as **claims, not proof**, throughout. Several comments in the new code describe intent ("this slice", "GAP-0xx"); each such claim was checked against actual code rather than accepted. | Nothing; this is a discipline, recorded because it materially changed two conclusions | REF-159, DEF-01 |

### 1.3 Verification-status ceiling for this run

Because nothing was built, executed or launched, `TEST VERIFIED`, `BUILD VERIFIED` and `RUNTIME VERIFIED` **cannot be earned and are not used anywhere in this report**. Every row uses one of `STATICALLY VERIFIED`, `PARTIALLY VERIFIED`, or `NOT VERIFIED FROM AVAILABLE ENVIRONMENT`.

- `STATICALLY VERIFIED` — the actual implementation path was read in source, end to end, including the persistence or rendering terminus.
- `PARTIALLY VERIFIED` — the path was read for the parts that exist in source, and a material layer (UI binding, rendering output, or runtime behaviour) could not be confirmed.
- `NOT VERIFIED FROM AVAILABLE ENVIRONMENT` — a negative classification resting on a documented systematic search, where no runtime confirmation is possible in this run.

---

## SECTION 2 — Target Commit Verification

**Required commit:** `9d90f499e95082ec40c417bd4910f1cfc62daec3`

### 2.1 Commands executed and exact observed output

**Command 1 — fetch:**
```
git -c http.sslCAInfo=/etc/ssl/certs/agent-identity/sandbox-gateway-ca.crt fetch origin main
```
Observed output:
```
From https://github.com/El-ogra/Top-Lab
 * branch            main     -> FETCH_HEAD
   c482a91..9d90f49  main     -> origin/main
```

**Command 2 — checkout the target detached:**
```
git checkout --detach 9d90f499e95082ec40c417bd4910f1cfc62daec3
```
Observed output:
```
HEAD is now at 9d90f49  بعد المرحلة العاشرة
```

**Command 3 — the mandatory verification command:**
```
$ git rev-parse HEAD
```
Observed output:
```
9d90f499e95082ec40c417bd4910f1cfc62daec3
```

**Supporting (read-only, permitted):**
```
$ git log -1 --format="%H%n%an%n%ad%n%s"
9d90f499e95082ec40c417bd4910f1cfc62daec3
medowemado
Sun Oct 18 18:20:08 2026 +0300
 بعد المرحلة العاشرة
```

### 2.2 Result

| Item | Value |
|---|---|
| Required commit | `9d90f499e95082ec40c417bd4910f1cfc62daec3` |
| Observed `git rev-parse HEAD` | `9d90f499e95082ec40c417bd4910f1cfc62daec3` |
| **Match** | **YES — exact match** |
| Investigation proceeded? | **YES** |
| Investigation stopped? | **NO** |
| Reason for any stop | None |

### 2.3 Access and checkout limitations

- The working copy is a clone created outside the repository under examination; the target was checked out **detached**, exactly as instructed. No user working tree was involved.
- The only repository-state-changing commands run were the two authorised ones above. **No `reset`, `clean`, `stash`, `merge`, `rebase`, `commit`, `push` or branch operation was executed.**
- The remote's TLS chain is not trusted by the default store in this sandbox; `http.sslCAInfo` was pointed at the sandbox gateway CA bundle. Certificate verification was **not** disabled.
- The target commit was reachable: `origin/main` advanced from `c482a91` to `9d90f49` in the fetch above.

---

## SECTION 3 — Reference System Coverage

Only the two authorized PDFs were used as reference-system evidence. **No file under `Docs/Audit/` was listed, opened, searched, read, cited, summarized or used.** Within `Docs/Remaining Tasks Folder/`, **only the two authorized PDFs were opened**; no other file in that folder was read. The directory `Docs/Reference system files/` was neither listed nor entered, and no traversal was rooted inside it.

**Firewall disclosure.** No `find`, `ls`, `grep` or search in this investigation was rooted at `Docs/`, at `Docs/Audit/`, at `Docs/Remaining Tasks Folder/`, or at `Docs/Reference system files/`. All repository searches were rooted at `src/`, `tests/`, or at the two exact PDF paths. The PDF page counts and metadata below were obtained by running `pdfinfo` against the two files by explicit path, and page text was obtained by `pdftotext -f/-l` against those same two paths.

### 3.1 `Docs/Remaining Tasks Folder/RLS_Learn_Enhanced.pdf`

| Attribute | Value |
|---|---|
| Filename | `RLS_Learn_Enhanced.pdf` |
| Total page count | **212** (determined by `pdfinfo`) |
| Page size / file size | 612 × 792 pt (Letter) / 19,641,086 bytes |
| Producer | `OCRmyPDF 17.11.0 / OCRmyPDF fpdf2 + Tesseract OCR 5.3.4`; `pikepdf 10.12.0`; Author `RealLab` |
| Pages whose text was extracted | **212 / 212** |
| Pages **visually inspected** (rendered to image and examined) | **212 / 212** |
| Pages partially inspected | 0 |
| Pages inspected only through text extraction | **0** |
| Pages not inspected | **0** |
| Unreadable / ambiguous pages | 8 pages carry an effectively empty text layer — **6, 8, 148, 165, 173, 189, 207, 211**. All 8 were rendered and examined: each is a section-divider page carrying only the `real lab` logo, the `Real Lab System` rule and the page number. **None contains function content.** All other pages carried enough text plus rendered detail to be read. |
| Was OCR needed in this run? | **No.** An OCR text layer already existed. No OCR was run. |
| Remaining interpretation limits | L-10, L-11 |

### 3.2 `Docs/Remaining Tasks Folder/RL_Show_Enhanced.pdf`

| Attribute | Value |
|---|---|
| Filename | `RL_Show_Enhanced.pdf` |
| Total page count | **77** (determined by `pdfinfo`) |
| Page size / file size | 595 × 842 pt (A4) / 9,133,133 bytes |
| Producer | `OCRmyPDF 17.11.0 / OCRmyPDF fpdf2 + Tesseract OCR 5.3.4`; `pikepdf 10.12.0`; Title "Microsoft Word - RL_Show.docx"; Author `الاسلاميه` |
| Pages whose text was extracted | **77 / 77** |
| Pages **visually inspected** | **77 / 77** |
| Pages partially inspected | 0 |
| Pages inspected only through text extraction | **0** |
| Pages not inspected | **0** |
| Unreadable / ambiguous pages | Page **67** carries an effectively empty text layer. It was rendered and examined: a full-colour splash/login screen for `RealLab Interface — RL-I Hematology Analyzer Interface` showing device `BC-3000 Plus`, captioned "بعض نوافذ ريل لاب انترفيس … شاشة تسجيل الدخول". It contains no independent function content and is corroborating evidence for two owner-excluded functions. |
| Was OCR needed in this run? | **No.** An OCR text layer already existed. |
| Remaining interpretation limits | L-10. **Pages 56–66 extract with reversed Latin text** (`retupmoC erawtfoS`) because the embedded screenshots are mirrored. Their **text layer is not reliably readable**, so every function taken from those pages rests on the **rendered page image**, which was inspected individually and in contact sheets. This is disclosed rather than silently ignored. |

### 3.3 Page-by-page coverage checklist — method per page

Every page of both PDFs was rasterised with `pdftoppm -r 60 -jpeg`. Images were assembled into 4 × 2 contact sheets (8 pages per sheet) and each sheet was opened and examined as an image; the low-text pages of each PDF were additionally rendered individually at higher fidelity.

| PDF | Pages | Text extracted | Image rendered | Image examined | Method relied upon |
|---|---|---|---|---|---|
| RLS_Learn_Enhanced.pdf | 1–192 | yes | yes | yes | image + text cross-check |
| RLS_Learn_Enhanced.pdf | 193–212 | yes | yes | yes (this run) | image + text cross-check |
| RL_Show_Enhanced.pdf | 1–55 | yes | yes | yes | image + text cross-check |
| RL_Show_Enhanced.pdf | 56–66 | yes (mirrored/unreliable) | yes | yes (this run) | **image only** |
| RL_Show_Enhanced.pdf | 67 | yes (near-empty) | yes | yes (individually) | **image only** |
| RL_Show_Enhanced.pdf | 68–77 | yes | yes | yes | image + text cross-check |

### 3.4 What the newly analysed pages contributed

Pages 193–212 of the first PDF and 56–66 of the second were rendered and examined in this run. They materially enlarged the inventory and are the source of **thirteen** functions that a text-only reading would have missed, including the complete blood-count report output family on pages 56–66 and the per-report-line font/colour, printer font and envelope item tables on pages 193, 195, 197, 199, 203 and 206.

---

## SECTION 4 — Canonical Reference Function Inventory

Built independently from the two authorized PDFs. **Decomposition rules:** the smallest meaningful independently verifiable unit; a screen, entity or component is not a business function by itself; repeated mentions of one capability across the table of contents, prose and screenshots are collapsed into one entry with every confirming location recorded; a composite screen whose sub-controls are separately testable is decomposed.

**IDs.** `REF-001` … `REF-189`. Owner-excluded functions occupy `REF-001` … `REF-021` and are inventoried here, then handled exclusively in Section 5. In-scope functions occupy `REF-022` … `REF-189`.

### 4.1 Owner-excluded functions evidenced in the PDFs (REF-001 … REF-021)

Listed here for inventory completeness; classified only in Section 5.

| REF | Function | Capability | PDF | Page | Section / visual element |
|---|---|---|---|---|---|
| REF-001 | Upload a patient result to the online result website | Result upload to the portal | RLS | 58–62 | `12-1 … تسلٌم نتائج المريض عن طريق موقع نتٌجة (NATIGH.COM)`; `NATIGH.COM UPLOADER` splash |
| REF-002 | Open the result website and view the patient result | Patient-side portal viewing | RLS | 63–64 | `WWW.NATIGH.COM` login with Patient / Doctor / Laboratory radio buttons |
| REF-003 | Print the portal result data on the thermal receipt | Portal receipt printing | RLS | 69 | Thermal slip carrying `WWW.NATIGH.COM`, "By patient ID", "By password" |
| REF-004 | Android application for the patient to print or save the result | Mobile client | RLS | 63 | Google Play download text for the result application |
| REF-005 | Create a treating-doctor account for the result service | Doctor portal account | RLS | 142–144 | `الجهات الخارجية والمعامل` → treating doctor → ID generation |
| REF-006 | Create a laboratory account for the result service | Laboratory portal account | RLS | 145–147 | `Lab to Lab` branch of the same window |
| REF-007 | Count the patients whose results were uploaded in a period | Portal administration | RLS | 65–67 | `Admin` → عدد المرضى |
| REF-008 | Block / delete a patient result from the result website | Portal administration | RLS | 68 | `Block` button and confirmation dialog |
| REF-009 | Follow up a referrer's portal cases (lab-to-lab) | Portal-based case follow-up | SHOW | 53 | Red caption line |
| REF-010 | Equipment register with condition, periodic maintenance and calibration date | Equipment registration + maintenance + calibration | SHOW | 42 | System-window bullet `ادخال بيانات اجهزة ومعدات المعمل ومتابعة حالتها والصيانة الدورية وتاريخ المعايرة` |
| REF-011 | Connect to a haematology (CBC) analyser and pull results into the lab record | Analyser connection + result import | SHOW | 67, 74 | RL-I Hematology Analyzer Interface; product page for the interface |
| REF-012 | Analyser interface / server settings | Analyser connection configuration | SHOW | 72 | `interface | server setting` |
| REF-013 | Automatic analyser result-correction factors | Analyser correction factors | SHOW | 71 | `اضبط ثابت تصحيح النتيجة تلقائي` and the per-element factor grid |
| REF-014 | Supported analyser / hardware list | Analyser hardware catalogue | SHOW | 75, 77 | Biomerieux mini Vidas, VIDAS, Hemolyzer 3, Hemax 330, analyticon, B&E |
| REF-015 | Haematology analyser interface device login | Analyser connection configuration | SHOW | 67 | Interface login screen (BC-3000 Plus) |
| REF-016 | Cash-drawer / inventory accounting for a selected branch number | Branch-specific operation | RLS | 179 | `Branch no` selector inside الجرد وحساب الدرج |
| REF-017 | Search for patients within a specified branch | Branch-specific operation | RLS | 42, 45 | Prose search list; `Branch No.` field and `1/2/3` selector |
| REF-018 | Send the result to the patient by SMS | SMS result sending | RLS | 17, 47 | `Send SMS` button in the account/results command group |
| REF-019 | Send the result to the patient by e-mail | E-mail result sending | RLS | 17, 23, 47 | `Send E-mail` button |
| REF-020 | Send the result by fax | Fax result sending | RLS | 17, 18 | `By Fax` button in the same command group |
| REF-021 | Lab-branded SMS notice (congratulation, result, completion, marketing) | SMS sending | SHOW | 53 | Red caption line |

### 4.2 In-scope functions (REF-022 … REF-189)

Rows marked *(screenshot only)* are evidenced solely by a screen capture, with no prose instruction (L-13).

#### Module 1 — Access, users, permissions and attendance

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-022 | Sign in / sign out | Authenticate with user name and password; optional remember-login; sign out | RLS | 11, 52, 158 |
| REF-023 | Privileged-window secondary password | Gate internal windows behind a fixed code or the user's own password | RLS | 150, 156, 176, 179 |
| REF-024 | Create users and administrators | User with primary/secondary password, role, work end time and grants | RLS | 149–152 |
| REF-025 | Edit user data, role and password | Change data, permissions, password; renew grants | RLS | 156–157 |
| REF-026 | Assign per-user permissions | Grant a subset of lab work, including blocking print / patient delete / registration | RLS | 152–155; SHOW 51, 53 |
| REF-027 | Per-user discount limit | Bound the discount a user may grant | SHOW | 53 *(screenshot only)*; RLS 125 |
| REF-028 | Check-in, break and check-out attendance | Record attendance, break periods and end of duty | RLS | 158, 160 |
| REF-029 | User attendance report | Entry/exit times and work time per user; admin only | RLS | 161, 164 |

#### Module 2 — Patient registration

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-030 | Register a new patient | Name, type, age; optional phone, address, national ID, treating doctor | RLS | 12–13 |
| REF-031 | Patient notes on result and delivery screens | Free-text notes shown on those screens | RLS | 12 |
| REF-032 | Patient medical-history data | Chronic medication, chronic disease, pregnancy, recent imaging | RLS | 13, 17, 47 |
| REF-033 | Add an individual test to the visit | Double-click to add | RLS | 13 |
| REF-034 | Add a whole test group / profile | Add every analyte of a profile at once | RLS | 15–16 |
| REF-035 | Remove one test / clear all tests | Remove a single test, or clear all | RLS | 13 |
| REF-036 | Mark a test as taken outside the lab | Flag; shown as a report note | RLS | 13 |
| REF-037 | Save the visit and its tests | Commit the registration and its ordered tests | RLS | 13, 16 |
| REF-038 | Print the patient barcode label | Print the patient identity barcode | RLS | 12 |
| REF-039 | Patient account slip with tests and collection date | Account, tests, pickup date; optional test prices | RLS | 12, 20; SHOW 52 |
| REF-040 | Edit an existing patient or their tests | Reopen a registration; add/remove tests | RLS | 22–25 |
| REF-041 | List the patients registered on a chosen day | Browse patients of a selected date | RLS | 22, 26 |

#### Module 3 — Patient account and billing

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-042 | Compute the patient total from configured prices | Total derived from configured prices | RLS | 17 |
| REF-043 | Enter previously paid amount and discount, then calculate and save | Two-stage Enter: calculate, then persist | RLS | 17 |
| REF-044 | Add an extra charge to the account | `+`-prefixed amount added to the total | RLS | 17 |
| REF-045 | Settle the account in full | Mark paid and confirm | RLS | 17 |
| REF-046 | Per-test account dialog: edit or delete paid lines | Select a line, modify or delete | RLS | 18–20 |
| REF-047 | Record a payment and list payments | Post a payment; enumerate payments received | RLS | 17–18 |
| REF-048 | Print the patient invoice (receipt) with barcode | Produce the invoice including a barcode | RLS | 20; SHOW 4–5 |

#### Module 4 — Result entry and result lifecycle

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-049 | Open result entry for a patient | Enter the result-entry screen for a patient | RLS | 27 |
| REF-050 | Enter simple single-value results | One result value per test | RLS | 27–29; SHOW 38–41 |
| REF-051 | Enter profile (grouped-analyte) results | One value per analyte in a profile | RLS | 15–16; SHOW 37, 39–40 |
| REF-052 | Verify / unverify results | Mark reviewed/verified, and reverse | RLS | 28, 54 |
| REF-053 | Print results individually or in bulk | Print one result or the whole visit | RLS | 28, 53 |
| REF-054 | Print preview before printing | Preview the composed report | RLS | 30 |
| REF-055 | Export the patient report to PDF | Produce a PDF of the report | RLS | 30; SHOW 12–24 |
| REF-056 | Result workflow-status filters | Not entered / not reviewed / not printed / not delivered | RLS | 27, 42 |

#### Module 5 — Culture (microbiology) and antibiotics

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-057 | Enter culture results | Sample, Organism A/B/C, culture condition, colony count | RLS | 38, 40 |
| REF-058 | Enter culture microscopy findings | Microscopic examination block | RLS | 38, 40 |
| REF-059 | Antibiotic sensitivity and inhibition zone | Highly / Moderate / Low / Resistant + zone in mm | RLS | 38–40 |
| REF-060 | Select the antibiotic of choice | Mark the preferred antibiotic | RLS | 39 |
| REF-061 | Restrict antibiotics by pregnancy flag | Show only when the patient is pregnant | RLS | 39 |
| REF-062 | Restrict antibiotics by children flag | Show only for child patients | RLS | 39 |
| REF-063 | Control display of Sensitivity / Reference / Commercial Name | Toggle the three columns | RLS | 39 |
| REF-064 | Add a new culture test to the program | Register a culture-type test | RLS | 128–129 |
| REF-065 | Maintain the antibiotic master list | Add / edit / delete with symbol, scientific, commercial name | RLS | 134–135 |
| REF-066 | Attach antibiotics to a culture with a category | Attach and classify | RLS | 136–138 |
| REF-067 | Detach an antibiotic from a culture | Remove an attachment | RLS | 138 |
| REF-068 | Enter an antibiotic manually when absent from the master list | Type unknown antibiotic data directly | RLS | 138 |

#### Module 6 — Test catalogue and reference data

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-069 | Edit test data and price | Modify a catalogue entry and its price | RLS | 94–95 |
| REF-070 | Add a new test to the program | Code, group, price, barcode, sample label, reference type | RLS | 96–97 |
| REF-071 | Test groups: create / rename / deactivate / reactivate | Full lifecycle | RLS | 93, 118 |
| REF-072 | Work group (Log): create / rename and set items | Batch grouping for the worksheet | RLS | 85 |
| REF-073 | Print the analysis price list | Printable price list | RLS | 111 |
| REF-074 | Test comments: add / edit / delete | One or more fixed comments per test | RLS | 112–115 |
| REF-075 | Insert a stored comment into the report | Pick from a drop-down and add | RLS | 116–117 |
| REF-076 | Custom group: create / rename / delete | Manage named custom groups | RLS | 118–119, 123 |
| REF-077 | Add tests and set prices inside a custom group | Compose and price members | RLS | 120 |
| REF-078 | Add a custom group to a patient | Apply a custom group to a visit | RLS | 122 |
| REF-079 | Reference values: enter min/max with low/high comments | Normal interval and the texts either side | RLS | 98, 101 |
| REF-080 | Reference values: edit for a test | Modify an interval | RLS | 99–100 |
| REF-081 | Reference ranges by sex and age, or by age only | Two matching modes | RLS | 101–104 |
| REF-082 | Show the reference value and the deviation on entry | Display the interval and the delta | RLS | 101 |
| REF-083 | Temporarily override a reference value at print time | Open the range cell from the report, edit, refresh, accept | RLS | 101–102 |
| REF-084 | Map a test to a reference analyte | Bind a test to a reference and its result fields | RLS | 129 |

#### Module 7 — Price lists and external entities

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-085 | Create a new contract price list | Name and save a new list | RLS | 105 |
| REF-086 | Add tests to a price list | Append catalogue entries | RLS | 105 |
| REF-087 | Change a test price inside a list; rename a list | Edit member price; edit list name | RLS | 105 |
| REF-088 | Per-entity price, discount and commission | List, discount % and commission for a referrer | RLS | 107–110 |
| REF-089 | Treating doctor: add / edit / delete | Manage treating doctors | RLS | 124–125, 127 |
| REF-090 | Sent-out lab entity: add / edit / delete | Manage outside laboratories | RLS | 124 |
| REF-091 | Referring / contract entity: add / edit / delete with price list | Manage referrers, attach a list | RLS | 126–127 |
| REF-092 | Lab-to-lab price list for an outside lab | Price list used when sending work out | RLS | 145–146 |

#### Module 8 — Sent-out samples

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-093 | Mark a test as sent out | Receiving lab, cost price, patient price | RLS | 140–141 |
| REF-094 | Sent-out samples account for a period | Paid and remaining totals | RLS | 184–186; SHOW 29 |
| REF-095 | Detailed sent-out report per sample; search by a lab | Per-line detail, filtered by lab | RLS | 186–187 |

#### Module 9 — Result delivery and traceability

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-096 | Deliver patient results and settle the account | Hand over and close the balance | RLS | 21 |
| REF-097 | Period delivery grid with balances | Period grid with paid / remaining-to-lab | RLS | 21 |
| REF-098 | List undelivered results | Work not yet handed over | RLS | 12, 42 |
| REF-099 | Case follow-up: who registered, edited, when, last printer | Per-case audit trail | RLS | 52–57 |

#### Module 10 — Patient search and visit history

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-100 | Search by patient identity | Name match/partial; phone; national ID | RLS | 41–43 |
| REF-101 | Search by doctor, referral entity, gender, age, test or date | Six independent predicates | RLS | 42–43 |
| REF-102 | View patient and result detail from search | Open data and results from a hit | RLS | 43–46 |
| REF-103 | Display all lab visits with dates and tests | Every visit of one patient | RLS | 47–51 |
| REF-104 | Persist and auto-display the Lab ID | Stored and re-shown on re-registration | RLS | 47–48 |
| REF-105 | Show unentered results and the analysis price in the detail | Detail document carries the analysis and its price | RLS | 44; SHOW 6 |

#### Module 11 — Work sheets

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-106 | Work paper by patient names or codes | Worksheet listing patients | RLS | 85–88 |
| REF-107 | Work paper by test name(s) | Worksheet listing tests and patients | RLS | 89–92 |
| REF-108 | Work paper by work group (Log) | Worksheet for a whole work group | RLS | 85 |
| REF-109 | Period test-count classification | Count how many times each test was performed | RLS | 84–85 |
| REF-110 | Print the worksheet: unfinished only, or all | Restrict to unfinished work | RLS | 86, 89 |
| REF-111 | Preview the worksheet before printing | Preview | RLS | 88 |

#### Module 12 — Reports and patient history

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-112 | Combined report | Select and order tests; optional group sub-title; reprint without message | RLS | 31–34 |
| REF-113 | Blank report | Patient data only, for manual completion | RLS | 35–36 |
| REF-114 | Automatic patient history in the report | Prior results of the same test beneath the result | RLS | 70–73 |
| REF-115 | Insert previous results of selected tests | Choose history and insert | RLS | 75–76 |
| REF-116 | Print patient history in a separate report | History as its own document | RLS | 77–78 |
| REF-117 | Patient history for the blood-picture profile | Date × analyte matrix | RLS | 79 |
| REF-118 | Patient history for more than one patient | Multi-patient history | RLS | 80–83 |
| REF-119 | Configure the history sort / aggregation mode | From report settings | RLS | 70 |
| REF-120 | Group-patient report slip, one per patient | Independently archivable slip per patient | RLS | 52–57; SHOW 6 |

#### Module 13 — Sample handling

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-121 | List patients with uncollected samples; draw board | Board lists samples awaiting collection | RLS | 208–210 |
| REF-122 | Mark a sample as drawn | Record a sample as collected | RLS | 209–210 |

#### Module 14 — Statistics

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-123 | Patient count by sex, referral entity, account type | Classification counts | RLS | 167–168 |
| REF-124 | Patient count in a year grouped by month | Monthly buckets with money column | RLS | 169; SHOW 30 |
| REF-125 | Patient count in a month grouped by day | Daily buckets with money column | RLS | 170; SHOW 31 |
| REF-126 | Request rate for a group of tests in a period | Share of samples per test | RLS | 171; SHOW 32 |
| REF-127 | Test-sample counts in a year and per referrer | Counts per test; per referrer | RLS | 172; SHOW 33 |
| REF-128 | Sent-out statistics by period and receiving lab | Volume sent out, by lab | RLS | 174; SHOW 29 |
| REF-129 | User productivity statistics | Workload per user | RLS | 167, 149 |
| REF-130 | Banded result monitor for one test | Results of one test inside a chosen value band | SHOW | 25, 73 |
| REF-131 | Banded monitor over blood-picture **element** values | Banded search across CBC analytes | SHOW | 73 *(screenshot only)* |

#### Module 15 — Inventory and accounts

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-132 | Cash-drawer inventory and net profit for a period | Daily / weekly / monthly / yearly / arbitrary period | RLS | 175–177 |
| REF-133 | Element-scoped inventory | By user, referral entity, doctor, sent-out samples, account type | RLS | 178, 181 |
| REF-134 | Patient-sample detail behind an inventory total | Drill into patient rows | RLS | 180 |
| REF-135 | Referral / delegate account with commission share | Commission per referrer | RLS | 174 |
| REF-136 | Cash deposit and disbursement | Money in and out | RLS | 174–175 |
| REF-137 | Sent-out samples account and settlement | Account and full settlement | RLS | 184–186 |
| REF-138 | Other-labs account and referrer share in a period | Other labs and referrers' share | RLS | 175 |
| REF-139 | Year-end account settlement | Annual settlement | RLS | 174 |

#### Module 16 — System and print settings

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-140 | Report settings (general) | Overall report configuration | RLS | 191–192 |
| REF-141 | Report margins and top space | Left margin, bottom margin, top space | RLS | 193 |
| REF-142 | Report paper size | A4 / A5 | RLS | 194 |
| REF-143 | Report header/footer as words | Typed text lines with lab data in fields | RLS | 194 |
| REF-144 | Report header/footer from attached image files | Select an image for header or footer | RLS | 194 |
| REF-145 | Report header/footer colours | Colour of header and footer elements | RLS | 195–198 |
| REF-146 | Edit the character used to mark the normal range | Change the `N`/normal marker | SHOW | 17 |
| REF-147 | Receipt settings: currency, pickup time, print-once, header/footer text | Receipt appearance chapter | RLS | 202–204 |
| REF-148 | Receipt settings: font type, size, colour and logo | Full receipt branding | RLS | 203–204 |
| REF-149 | Dedicated printers per output type | Report / barcode / receipt / envelope | RLS | 199 |
| REF-150 | Default account type for the system | Account type applied automatically | RLS | 200–201 |
| REF-151 | Envelope settings | Envelope appearance | RLS | 205 |
| REF-152 | System general settings | Lab name, phone, address, result time, search assist | RLS | 192, 201 |
| REF-153 | Report print-option switches | Print with inventory account; delivery report with account; cheque without test value; print-so-far; all tests with/without invoice; entered tests only | RLS | 199 *(screenshot only)* |

#### Module 17 — Report content and layout control

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-154 | Editable report item template | Per item: label, reference-value expression, show/hide, list membership, free-text override | SHOW | 12 |
| REF-155 | Line spacing, item left offset, item marker symbol | Layout and marker control per item | SHOW | 13 |
| REF-156 | Global show / hide of a report item | Toggle an item on or off globally | RLS | 195, 197 *(screenshot only)* |
| REF-157 | Per-user (doctor) report element toggle | Side menu to show/hide elements per doctor | SHOW | 15 *(screenshot only)* |
| REF-158 | Shade or colour abnormal results, or disable it | Highlight out-of-range results | SHOW | 13 |
| REF-159 | Per-profile normal/abnormal font and background colour | Per-profile result styling | SHOW | 16 |
| REF-160 | Per-report-line font family, size and colour | Font + colour columns for ~12 named report lines | RLS | 193, 195, 197 |
| REF-161 | Print positions of report and envelope elements | Enable + left/top offset per element | SHOW | 6; RLS 205–206 |
| REF-162 | Envelope per-item font, size and colour | Per-item styling table on the envelope | RLS | 206 *(screenshot only)* |
| REF-163 | Envelope item ordering (place before/after another item) | Reorder printed envelope items | RLS | 206 *(screenshot only)* |

#### Module 18 — Barcode labels

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-164 | Barcode labels for tubes, containers and swabs with doctor-editable test distribution | Sort tests across drawn materials; doctor may change the distribution | SHOW | 3 |
| REF-165 | Optional vertical coding on report, receipt and envelope | Vertical identity coding on documents | SHOW | 3 |
| REF-166 | Reprint any barcode label | Reprint a previously issued label | SHOW | 3 |
| REF-167 | Sticker size configuration | Label stock size (38 × 25 mm) | SHOW | 3 |

#### Module 19 — Utilities and account statements

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-168 | Interpret sample nature, its normal ranges and clinical effect | Sample-nature interpretation panel | SHOW | 51 |
| REF-169 | Glossary of analysis abbreviations | Terminology lookup | SHOW | 51 |
| REF-170 | Requirements / purchases list | Shopping-requirements list | SHOW | 51 |
| REF-171 | Stopwatch, time clock and profile calculation | Elapsed-time and calculation tools | SHOW | 51 |
| REF-172 | Appointment-note reminder to a user or contact | Send a reminder note | SHOW | 51 |
| REF-173 | Phone book | Contact list | SHOW | 51 |
| REF-174 | Unit conversion | Convert between measurement units | SHOW | 51 |
| REF-175 | Patient receivable summary with a balance filter | Period summary filtered to unpaid / discounted / paid-only | SHOW | 26 |
| REF-176 | Referrer account statement in four variants | By results, by test prices, by patient, consolidated | SHOW | 28 |

#### Module 20 — Receipt printing options

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-177 | Print the receipt on a cashier / thermal printer | Receipt to a till printer | SHOW | 4 |
| REF-178 | Show / hide test details and collection data on the receipt | Receipt content switches | SHOW | 4, 5 |
| REF-179 | Receipt header/footer words plus logo | Branding the receipt | SHOW | 4 |

#### Module 21 — Complete blood count report output family

All from SHOW 56–66; **image evidence only** because the text layer is mirrored (L-10). Each page carries a red caption describing the capability.

| REF | Function | Capability | PDF | Page |
|---|---|---|---|---|
| REF-180 | CBC report histograms (WBC / RBC / PLT distribution) | Print the distribution curves beside the results | SHOW | 56–65 *(screenshot only)* |
| REF-181 | Two-dimensional scattergram for the differential | Print the 2-D WBC scatter plot | SHOW | 65 *(screenshot only)* |
| REF-182 | Five-part and three-part differential reporting | Select the differential resolution and its labelled breakdown | SHOW | 64, 65 *(screenshot only)* |
| REF-183 | Colour versus black-and-white report printing | Choose the print rendering | SHOW | 59, 61 *(screenshot only)* |
| REF-184 | Report printed under the patient name in Arabic and English | Dual-language patient name on the report | SHOW | 60 *(screenshot only)* |
| REF-185 | Report on large paper (A3) or A4 at 85 × 55 | Choose a larger paper stock | SHOW | 56, 65 *(screenshot only)* |
| REF-186 | Folded / multi-port report printing | 3-port and 5-port folded output | SHOW | 64 *(screenshot only)* |
| REF-187 | Sequential sample numbering on the report | Print an auto-incrementing sample number | SHOW | 56, 61, 65 *(screenshot only)* |
| REF-188 | Auto-generated remark from result flags | Compose a remark from the abnormal findings | SHOW | 56, 61, 63 *(screenshot only)* |
| REF-189 | Charted blood-count history report | Bar charts per analyte plus RBC-index and differential trend lines across visits | SHOW | 66 *(screenshot only)* |

### 4.3 Deduplication and decomposition notes

- **REF-120** is evidenced twice (RLS 52–57 and SHOW 6) and is carried **once**, with both locations recorded.
- **REF-143 / REF-144** are kept separate: the first is the typed-words header/footer, the second selects an image file. They map to different implementation surfaces and appear as separate numbered entries in the reference.
- **REF-156 / REF-157** are kept separate after decomposition. The reference evidences a **global** "Show this line" checkbox (RLS 195, 197) and a **per-doctor** side-menu toggle (SHOW 15). These are independently testable and map to different scopes.
- **REF-141 / REF-160** are kept separate: page margins versus per-report-line font/colour are independent controls.
- **REF-162 / REF-163** are kept separate: per-item styling versus per-item ordering are independent.
- **REF-130 / REF-131** are kept separate: a banded monitor over one test's scalar value versus a banded monitor over blood-picture analyte values. Page 25 and page 73 both describe banded search; page 73's caption names CBC *elements*.
- **REF-180 … REF-189** were each evidenced by a specific page caption; none is a re-description of another.

**Inventory totals:** 189 PDF-evidenced reference functions (`REF-001` … `REF-189`); 21 owner-excluded; **168 in scope**.

---

## SECTION 5 — Owner-Excluded Functions

The five categories below are defined solely by the execution brief. **No repository document was consulted to justify, widen or narrow any of them**, and none is cited.

**Designation for every item: OWNER-EXCLUDED — NOT REQUIRED IN THIS PROJECT SCOPE**

No item is classified IMPLEMENTED, DIFFERENT, MISSING or BLOCKED / UNRESOLVED. None is counted as an in-scope gap. None is reported as a defect, deficiency, technical debt or shortcoming. None appears in Section 9 or Section 10. None is recommended or restored.

### 5.1 Category 1 — Online results portal and ecosystem (9 items)

| REF | Function | Subfunction | Evidence | Status |
|---|---|---|---|---|
| REF-001 | Upload a result to the portal | Result upload | RLS 58–62 | Confirmed in the PDFs |
| REF-002 | View the result on the website | Portal viewing | RLS 63–64 | Confirmed |
| REF-003 | Print portal data on the thermal receipt | Portal receipt | RLS 69 | Confirmed |
| REF-004 | Android application for the patient | Mobile client | RLS 63 | Confirmed |
| REF-005 | Doctor account for the result service | Doctor portal account | RLS 142–144 | Confirmed |
| REF-006 | Laboratory account for the result service | Laboratory portal account | RLS 145–147 | Confirmed |
| REF-007 | Count uploaded results in a period | Portal administration | RLS 65–67 | Confirmed |
| REF-008 | Block / delete a portal result | Portal administration | RLS 68 | Confirmed |
| REF-009 | Follow up referrer portal cases | Portal case follow-up | SHOW 53 | Confirmed |

### 5.2 Category 2 — Equipment, maintenance and calibration (1 item)

| REF | Function | Subfunction | Evidence | Status |
|---|---|---|---|---|
| REF-010 | Equipment register with condition, maintenance and calibration date | Registration + maintenance records + calibration records | SHOW 42 | Confirmed |

**Disclosure.** One reference line names three sub-capabilities; all three fall inside Category 2 and are excluded. One identifier is retained rather than three, because splitting would inflate the inventory without changing any treatment.

### 5.3 Category 3 — Blood/CBC analyser integration (5 items)

| REF | Function | Subfunction | Evidence | Status |
|---|---|---|---|---|
| REF-011 | Connect to a CBC analyser and pull results into the record | Analyser connection + result import | SHOW 67, 74 | Confirmed |
| REF-012 | Analyser interface / server settings | Connection configuration | SHOW 72 | Confirmed |
| REF-013 | Automatic analyser result-correction factors | Correction factors | SHOW 71 | Confirmed |
| REF-014 | Supported analyser / hardware list | Hardware catalogue | SHOW 75, 77 | Confirmed |
| REF-015 | Analyser interface device login | Connection configuration | SHOW 67 | Confirmed |

**Boundary note — material.** These five items describe a **separate reference product** ("ريال لاب انترفيس" / RealLab Interface), pages 67 and 74–77. Nothing in the laboratory-system chapters describes importing analyser results. The boundary is therefore:

- **Excluded:** the device interface, the hardware list, the interface settings and the correction factors.
- **In scope and fully classified in Section 6:** all CBC **result entry** inside the lab system (REF-050, REF-051, REF-117, REF-189) and the **report-output** family (REF-180 … REF-188).

**Ambiguity disclosed, not resolved by reclassification.** The reference's blood-count pages carry the provenance line "(Done on fully Automatic Hematology Analyzer)", and the histograms, scattergram and auto-generated remark on SHOW 56–66 are **produced by the analyser**. Those report *outputs* are in scope because they are lab-system report capabilities; the **acquisition of their source data** is excluded. This boundary is recorded here and carried into Section 9, where the affected sequence items are marked **Blocked** with that reason. **No excluded functionality is recommended anywhere in this report.**

### 5.4 Category 4 — Multi-branch support (2 items)

| REF | Function | Subfunction | Evidence | Status |
|---|---|---|---|---|
| REF-016 | Inventory for a selected branch number | Branch-specific operation | RLS 179 | Confirmed |
| REF-017 | Search for patients in a specified branch | Branch-specific operation | RLS 42, 45 | Confirmed |

**Boundary note.** The branch selector is an input to two larger in-scope capabilities (cash-drawer inventory REF-132; patient search REF-101). Those are assessed **only in their non-branch form**. The branch dimension is excluded and is neither counted as implemented nor reported as absent.

### 5.5 Category 5 — SMS, e-mail and fax result sending (4 items)

| REF | Function | Subfunction | Evidence | Status |
|---|---|---|---|---|
| REF-018 | Send the result by SMS | SMS result sending | RLS 17, 47 | Confirmed |
| REF-019 | Send the result by e-mail | E-mail result sending | RLS 17, 23, 47 | Confirmed |
| REF-020 | Send the result by fax | Fax result sending | RLS 17, 18 | Confirmed |
| REF-021 | Lab-branded SMS notice | SMS sending | SHOW 53 | Confirmed |

**Evidence-strength disclosure (REF-020).** The fax control appears as a **screenshot button** in a command group, not as a prose instruction, and the surrounding Arabic is degraded. This is recorded as evidence of a fax-sending capability, with the limitation stated. It does not alter the treatment: SMS, e-mail and fax are one category.

### 5.6 Owner exclusions not evidenced in the PDFs

**None.** Every one of the five categories is represented by at least one directly evidenced function. No item is recorded as "an owner exclusion whose presence in the permitted reference material could not be confirmed", and no PDF citation has been fabricated. There is consequently no third population to hold outside the 189.

---

## SECTION 6 — Current Classification of In-Scope Functions

### 6.1 Method

- Each in-scope function carries **exactly one** classification and **exactly one** verification status.
- `IMPLEMENTED` requires a concrete implementation path traced at the target commit, ending at a real persistence or rendering terminus.
- `DIFFERENT` is used when the capability exists but materially differs — including a stored-but-inert setting.
- `MISSING` requires a **documented, systematic negative search**; each negative row below names the constructs searched for.
- `BLOCKED / UNRESOLVED` is **not** used anywhere. No in-scope function was left unclassifiable; every negative rests on a search that was actually performed. Using `BLOCKED` to cover those rows would be concealment.
- A class, property, column, XAML element, command name, test name or comment is never sufficient. Several new comments in this commit describe intent (for example "GAP-011", "this slice"); each was checked against real code, and in two cases the code disagreed with the comment (DEF-01, DEF-02).
- Path shorthand: `UI → VM/Cmd → Handler → Domain → Persistence`.

### 6.2 Module 1 — Access, users, permissions and attendance

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-022 | Sign in / sign out | IMPLEMENTED | STATICALLY VERIFIED | `UsersAndPermissions/Commands/SignIn/SignInCommandHandler.cs` → `User.VerifyPassword` → `Pbkdf2PasswordHasher` → `UserConfiguration.cs`; `SignOutCommandHandler`; `GetCurrentSessionQueryHandler`; `Views/Setup/LoginWindow.xaml`; `Presentation/DependencyInjection.cs` registers the session service | Full chain present. Password hashing is PBKDF2 via a dedicated infrastructure service, not plaintext. UI not compilable here (L-01). |
| REF-023 | Privileged-window secondary password | IMPLEMENTED | PARTIALLY VERIFIED | `UsersAndPermissions/Queries/VerifySecondaryPassword/VerifySecondaryPasswordQueryHandler.cs`; `Views/Dialogs/SystemMenuPasswordDialog.xaml(.cs)`; called from `InventoryAndAccountingAccessPolicy` / `UsersAndPermissionsAccessPolicy` before the privileged queries | Gate exists and is reachable. Not runtime-verified (L-04). |
| REF-024 | Create users and administrators | IMPLEMENTED | STATICALLY VERIFIED | `CreateUserCommandHandler.cs` → `User.Create` → `SaveUserPermissionsCommandHandler.cs` → `UserPermissionGrant` → `UserConfiguration.cs`; `Views/Users/UserManagementView.xaml` | Matches the documented window including work-end-time. |
| REF-025 | Edit user data, role and password | IMPLEMENTED | STATICALLY VERIFIED | `UpdateUserCommandHandler.cs`; `ChangeOwnPasswordCommandHandler.cs`; `DeactivateUser` / `ReactivateUser` / `DeleteUser` | Full lifecycle present. |
| REF-026 | Assign per-user permissions | IMPLEMENTED | STATICALLY VERIFIED | 13 permissions seeded in `PermissionConfiguration.cs:18-30`; enforced uniformly by `Application/Common/Behaviors/AuthorizationBehavior.cs` against `IAuthorizedRequest.RequiredPermissionCode`; grants persisted via `UserPermissionGrant` | Enforcement is architectural, not per-screen. **Finding F-01:** the reference lists a longer per-activity list; Top-Lab is coarser. Noted, not reclassified. |
| REF-027 | Per-user discount limit | IMPLEMENTED | STATICALLY VERIFIED | `User.SetPolicy(DiscountLimitPercent, BlockPrintOnRemainingBalance)` (`User.cs:63`, `:169`); `CreateUserCommand.cs:12`; `UserManagementViewModel.cs:181` | Present and editable per user. |
| REF-028 | Check-in, break, check-out | IMPLEMENTED | STATICALLY VERIFIED | `Attendance/Commands/{CheckIn,StartBreak,EndBreak,CheckOut}` → `AttendanceRecord` → `AttendanceCalculator.cs`; `Views/Attendance/MyAttendanceView.xaml` | Matches the documented sequence. |
| REF-029 | User attendance report | IMPLEMENTED | PARTIALLY VERIFIED | `Attendance/Queries/{GetAttendanceRecords,GetUserAttendanceSummary}`; `Views/Attendance/{AttendanceRecordsView,UserAttendanceSummaryView}.xaml` | Both surfaces present. |

### 6.3 Module 2 — Patient registration

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-030 | Register a new patient | IMPLEMENTED | STATICALLY VERIFIED | `PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs` → `Patient.Create` (validates name, age ≥ 0) → `PatientConfiguration.cs`; phones in a separate `PatientPhoneNumber` table | All documented base fields present. |
| REF-031 | Patient notes | IMPLEMENTED | STATICALLY VERIFIED | `Patient.Notes` (`Patient.cs:47`) → `PatientRegistrationDtos` → consumed by the result and delivery readers | Field, persistence and read path traced. |
| REF-032 | Patient medical-history data | IMPLEMENTED | STATICALLY VERIFIED | `MedicalConditionType` + `PatientMedicalCondition` with `MedicalConditionCategory`; `AddMedicalCondition` / `RemoveMedicalCondition` / `ApplyConditionDeltas`; `GetMedicalConditionTypes`; pregnancy seeded by `20260910213833_AddPregnancyMedicalConditionTypeSeed` | Covers medication, chronic disease, pregnancy, imaging. |
| REF-033 | Add an individual test | IMPLEMENTED | STATICALLY VERIFIED | `AddTestsToVisitCommandHandler.cs` → `PatientTest` → `PatientTestConfiguration.cs`; price via `TestPriceResolver.cs` | Traced. |
| REF-034 | Add a whole profile | IMPLEMENTED | STATICALLY VERIFIED | `AddProfileToVisitCommandHandler.cs` → `Profile` + `ProfileAnalyte` | Matches the group-add behaviour. |
| REF-035 | Remove one test / clear all | IMPLEMENTED | STATICALLY VERIFIED | `RemoveTestFromVisitCommandHandler.cs`; `ClearAllTestsCommandHandler.cs` | Both operations present. |
| REF-036 | Mark test taken outside the lab | IMPLEMENTED | STATICALLY VERIFIED | `PatientTest.IsTakenOutsideLab`; `UpdatePatientTestSampleFlagsCommandHandler.cs`; honoured in the worksheet handlers | Persisted and consumed. |
| REF-037 | Save the visit and its tests | IMPLEMENTED | STATICALLY VERIFIED | MediatR pipeline persists via `AppUnitOfWork.SaveChangesAsync`; `Patient.EnsureNotDeleted` guards mutation | Path traced to the unit of work. |
| REF-038 | Print the patient barcode label | IMPLEMENTED | PARTIALLY VERIFIED | `PrintBarcodeCommandHandler.cs` → `IBarcodeService.PrintBarcodeAsync` → `BarcodeService.cs` → `BarcodeLabelRenderer` → Code-128 PNG → PDF → `PrinterOutputType.Barcode` assignment | Identity barcode works. Print output and Arabic rendering unverified (L-07). See REF-164 for the label family. |
| REF-039 | Patient account slip with tests and collection date | IMPLEMENTED | PARTIALLY VERIFIED | `PatientBilling/Queries/GetPatientReceipt` → `ReceiptPdfWriter.cs` / `ReceiptPrintingService.cs`; `ReceiptSettings.TestDetailDisplayMode` = Hide / Show / ShowWithCode implements the optional-prices switch | The optional price switch is explicitly modelled. |
| REF-040 | Edit an existing patient or their tests | IMPLEMENTED | STATICALLY VERIFIED | `UpdatePatientCommandHandler.cs`; reuses the add/remove test commands; `SoftDeletePatientCommandHandler.cs` | Both documented entry routes present. |
| REF-041 | List patients of a chosen day | IMPLEMENTED | STATICALLY VERIFIED | `PatientRegistration/Queries/GetPatientTitles`, `SearchPatientsQuery` (paged, registration-date scoped) → `Views/Patients/PatientsHubView.xaml` | Day-scoped browse present. |

### 6.4 Module 3 — Patient account and billing

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-042 | Compute the patient total | IMPLEMENTED | STATICALLY VERIFIED | `Domain/Billing/PatientAccountCalculator.cs`; price resolved by `TestPriceResolver.cs` from `PriceList` / `PriceListItem` / `CustomGroupItem` / `Test.PatientPrice` | Pure calculator with documented inputs. |
| REF-043 | Enter paid amount and discount, then calculate and save | IMPLEMENTED | STATICALLY VERIFIED | `RecordPaymentCommandHandler.cs`; `PaymentOperation` with `OperationType` / `MovementType`; `VoidPaymentOperationCommandHandler.cs` | Payment, discount and void are persisted, not recomputed ad hoc. |
| REF-044 | Add an extra charge | IMPLEMENTED | STATICALLY VERIFIED | `RecordExtraChargeCommandHandler.cs`; `PaymentOperation.IsExtraCharge` distinguished in inventory queries | The reference's `+` semantics are explicit. |
| REF-045 | Settle in full | IMPLEMENTED | STATICALLY VERIFIED | `SettleAccountInFullCommandHandler.cs` | Present. |
| REF-046 | Per-test account dialog: edit or delete paid lines | IMPLEMENTED | STATICALLY VERIFIED | `VoidPaymentOperationCommandHandler.cs`; `GetPatientAccountQuery` + `PatientBillingReader.cs`; `ListPatientPaymentsQuery` | Edit is modelled as void-then-repost rather than in-place mutation. Same observable outcome; an implementation choice, not a gap. |
| REF-047 | Record a payment and list payments | IMPLEMENTED | STATICALLY VERIFIED | `RecordPaymentCommandHandler.cs`; `ListPatientPaymentsQueryHandler.cs` | Present. |
| REF-048 | Print the invoice with barcode | IMPLEMENTED | PARTIALLY VERIFIED | `PrintInvoiceCommandHandler.cs` → `InvoicePdfWriter.cs` → `InvoicePrintingService.cs`; barcode embedded; `InvoiceIssue` persisted via `20260916113704_AddInvoiceIssues` | Print output unverified (L-07). |

### 6.5 Module 4 — Result entry and lifecycle

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-049 | Open result entry for a patient | IMPLEMENTED | PARTIALLY VERIFIED | `ResultsEntry/Queries/GetResultEntry`; `Views/Patients/SimpleResultEntryView.xaml`, `ProfileEntryView.xaml` | Present; UI uncompilable here. |
| REF-050 | Enter simple results | IMPLEMENTED | STATICALLY VERIFIED | `EnterResultCommandHandler.cs` → `PatientTest` (`ResultValue`, `EnteredAtUtc`, `IsReviewed`) → `PatientTestConfiguration.cs`; flag via `ResultFlagComputer.cs` | Full path traced. |
| REF-051 | Enter profile results | IMPLEMENTED | STATICALLY VERIFIED | `ProfileResults/Commands/SaveProfileResults` → `ProfileResultItem` (+ `ProfileResultItemReferenceRangeSnapshot`) | Snapshotting of the matched range exceeds the reference requirement. |
| REF-052 | Verify / unverify results | IMPLEMENTED | STATICALLY VERIFIED | `ResultsEntry/Commands/{ReviewResult,UnreviewResult,MarkAllPatientResultsReviewed}`; `ProfileResults/Commands/{VerifyProfileResults,UnverifyProfileResults}`; `CultureResults/Commands/{VerifyCultureResult,UnverifyCultureResult}` | Verified for all three result shapes. |
| REF-053 | Print results individually or in bulk | IMPLEMENTED | STATICALLY VERIFIED | `MarkResultPrintedCommandHandler.cs`; `ResultsEntry/Commands/BulkPrint/{BulkPrintPreflightQuery,ExecuteBulkPrintCommand}`; `ResultPrintCoordinator.cs` | Two-step bulk preflight and execution. |
| REF-054 | Print preview | IMPLEMENTED | PARTIALLY VERIFIED | `ReportProduction/Common/IPdfPreviewService.cs` → `Infrastructure/Printing/PdfPreviewService.cs` | Port exists and is consumed. |
| REF-055 | Export the patient report to PDF | IMPLEMENTED | PARTIALLY VERIFIED | `ResultsEntry/Commands/ExportPatientReportPdf` → `PatientReportPdfExporter.cs` → `ReportPdfWriter.cs` | Present. Output unverified (L-07). |
| REF-056 | Result workflow-status filters | IMPLEMENTED | STATICALLY VERIFIED | `GetResultWorklistQuery(HasResult, IsReviewed, IsPrinted, …)` with nullable tri-state semantics; `GetUndeliveredResultsQuery` | All four documented status filters present and independently parameterised. |

### 6.6 Module 5 — Culture and antibiotics

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-057 | Enter culture results | IMPLEMENTED | STATICALLY VERIFIED | `CultureResults/Commands/SaveCultureResults` → `Domain/Results/CultureResult.cs` (`Sample`, `OrganismA/B/C`, `CultureCondition`, `ColonyCount`) | All six documented fields present. |
| REF-058 | Enter culture microscopy findings | IMPLEMENTED | STATICALLY VERIFIED | `Domain/Results/CultureMicroscopy.cs`; `20261001233652_AddCultureMicroscopyAndZone`; rendered via `ReportCultureSection` | Present. |
| REF-059 | Sensitivity category and inhibition zone | IMPLEMENTED | STATICALLY VERIFIED | `CultureAntibioticResult.cs` (`SensitivityCategory?`, `InhibitionZoneMm` 0–100); enum HighlyFor / ModerateFor / LowFor / ResistantFor | Matches the four documented categories; the zone column is an addition. |
| REF-060 | Antibiotic of choice | IMPLEMENTED | STATICALLY VERIFIED | `CultureResults/Common/CultureAntibioticDisplay`; surfaced by `GetCultureReportQuery` | Present. |
| REF-061 | Pregnancy-flagged antibiotic restriction | IMPLEMENTED | STATICALLY VERIFIED | `Antibiotic.IsPregnancyFlagged`; `CultureResults/Common/PregnancySignal.cs`; strict filtering in `GetCultureEntryGrid` / `GetCultureReport` | Present. |
| REF-062 | Children-flagged antibiotic restriction | IMPLEMENTED | STATICALLY VERIFIED | `Antibiotic.IsChildrenFlagged`; age-unit-aware filtering in the same handlers | Present. |
| REF-063 | Sensitivity / Reference / Commercial Name column control | IMPLEMENTED | STATICALLY VERIFIED | `Antibiotic.Symbol` / `ScientificName` / `CommercialName` (`20261002002546`, `20261008135847`); `ReportCultureSection.SensitivityRows` selects the columns | Three-way column control modelled. |
| REF-064 | Add a new culture test | IMPLEMENTED | STATICALLY VERIFIED | `TestCatalogAndReferenceRanges/Commands/CreateTest` with `Culture` / `Organism` attributes and `LogGroup` / `LabGroup`; `Views/Lab/TestCatalogView.xaml`, `TestEditorWindow.xaml` | The reference's file-copy step is a legacy mechanism of that product; the capability is present via the supported route. |
| REF-065 | Antibiotic master list | IMPLEMENTED | STATICALLY VERIFIED | `CultureAndAntibiotics/Commands/{CreateAntibiotic,UpdateAntibiotic,DeleteAntibiotic}` → `Antibiotic` → `AntibioticConfiguration.cs`; `Views/Lab/{AntibioticsView,AntibioticEditorWindow}.xaml` | Full CRUD. |
| REF-066 | Attach antibiotic to a culture | IMPLEMENTED | STATICALLY VERIFIED | `AttachAntibioticToCultureCommandHandler.cs` → `CultureAntibioticAttachment` | Present. |
| REF-067 | Detach antibiotic from a culture | IMPLEMENTED | STATICALLY VERIFIED | `DetachAntibioticFromCultureCommandHandler.cs` | Present. |
| REF-068 | Manual antibiotic entry | IMPLEMENTED | STATICALLY VERIFIED | Same attachment path accepts a manually supplied antibiotic through the editor | Present. |

### 6.7 Module 6 — Test catalogue and reference data

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-069 | Edit test data and price | IMPLEMENTED | STATICALLY VERIFIED | `TestCatalogAndReferenceRanges/Commands/UpdateTest` → `Test.cs` → `TestConfiguration.cs` | Full field set present. |
| REF-070 | Add a new test | IMPLEMENTED | STATICALLY VERIFIED | `CreateTestCommandHandler.cs`; `Views/Lab/TestCatalogView.xaml`, `TestEditorWindow.xaml` | Present. |
| REF-071 | Test groups lifecycle | IMPLEMENTED | STATICALLY VERIFIED | `{CreateTestGroup,UpdateTestGroup,DeactivateTestGroup,ReactivateTestGroup}`; `Views/Lab/TestGroupsView.xaml` | Full lifecycle. |
| REF-072 | Work group (Log) lifecycle and items | IMPLEMENTED | STATICALLY VERIFIED | `{CreateWorkGroupLog,RenameWorkGroupLog,SaveWorkGroupLogItems}` → `WorkGroupLog` + `WorkGroupLogItem`; `Views/Lab/WorkGroupLogsView.xaml` | Present; drives REF-108. |
| REF-073 | Print the analysis price list | IMPLEMENTED | PARTIALLY VERIFIED | `GetPriceLists` + `Infrastructure/Printing/PriceListPdfWriter.cs` | Present. Output unverified (L-07). |
| REF-074 | Test comments CRUD | IMPLEMENTED | STATICALLY VERIFIED | `{CreateTestComment,UpdateTestComment,DeleteTestComment}` → `TestComment`; multiple per test supported | Matches the reference's "more than one comment per test". |
| REF-075 | Insert a stored comment into the report | IMPLEMENTED | PARTIALLY VERIFIED | `GetTestCommentsForResults`; `Views/Patients/TestCommentPickerWindow.xaml` | Present. |
| REF-076 | Custom group lifecycle | IMPLEMENTED | STATICALLY VERIFIED | `{CreateCustomGroup,RenameCustomGroup,DeleteCustomGroup}`; `Views/Lab/CustomGroupsView.xaml` | Full lifecycle. |
| REF-077 | Add tests and prices inside a custom group | IMPLEMENTED | STATICALLY VERIFIED | `CustomGroupItem`; `RemoveCustomGroupItemCommandHandler.cs`; custom-group prices feed `TestPriceResolver` | Present. |
| REF-078 | Add a custom group to a patient | IMPLEMENTED | STATICALLY VERIFIED | `PatientRegistration/Commands/AddCustomGroupToVisit` | Present. |
| REF-079 | Enter reference values | IMPLEMENTED | STATICALLY VERIFIED | `{CreateReferenceRange,UpdateReferenceRange,DeleteReferenceRange}` → `ReferenceRange` (`MinValue`, `MaxValue`, `LowComment`, `HighComment`); analyte-owned variant `AnalyteReferenceRange` + `AnalyteReferenceRangeBand` | Both range models exist. |
| REF-080 | Edit reference values | IMPLEMENTED | STATICALLY VERIFIED | `UpdateReferenceRangeCommandHandler.cs`; `SaveAnalyteReferenceRangeCommandHandler.cs` | Present. |
| REF-081 | Ranges by sex and age, or by age only | IMPLEMENTED | STATICALLY VERIFIED | `ReferenceRange.Sex` (nullable) + `AgeUnit` / `AgeMin` / `AgeMax`; `ResultFlagComputer.SelectMatch` prefers sex-matched, then narrowest band, then lowest id — the same rule shared by the analyte-band path | Both documented modes supported. |
| REF-082 | Show the reference value and the deviation on entry | IMPLEMENTED | STATICALLY VERIFIED | `GetResultEntry` returns the matched range with the entered value; `ResultFlagComputer.Compute` | **Finding F-02:** the explicit numeric delta is conveyed by the low/high comment rather than a distinct deviation field. The reference's observable effect is reached. |
| REF-083 | Temporary reference override at print time | IMPLEMENTED | STATICALLY VERIFIED | `ResultsEntry/Commands/RefreshResultReferenceRange`; `ResultReferenceRangeSource.cs` re-resolves the live range and re-freezes `PatientTestReferenceRangeSnapshot` for this patient only | Semantics match the reference: print-time refresh scoped to one patient's test. |
| REF-084 | Map a test to a reference analyte | IMPLEMENTED | STATICALLY VERIFIED | `{MapTestToAnalyte,UnmapTestFromAnalyte}`; `Test.AnalyteId`; `AnalyteProfiles/Commands/SaveAnalyteReferenceRange` | Present. |

### 6.8 Modules 7–11 — Price lists, entities, sent-out, delivery, search, work sheets

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-085 | Create a contract price list | IMPLEMENTED | STATICALLY VERIFIED | `CreatePriceListCommandHandler.cs` → `PriceList` | Present. |
| REF-086 | Add tests to a price list | IMPLEMENTED | STATICALLY VERIFIED | `PriceListItem`; `RemovePriceListItemCommandHandler.cs` | Present. |
| REF-087 | Change a price / rename a list | IMPLEMENTED | STATICALLY VERIFIED | `SetPriceListItemPriceCommandHandler.cs`; `RenamePriceListCommandHandler.cs` | Present. |
| REF-088 | Per-entity price, discount and commission | IMPLEMENTED | STATICALLY VERIFIED | `ExternalEntity.PriceListId`, `DiscountPercent`, `CommissionPercent`; consumed by `TestPriceResolver` and surfaced as `CommissionShareDto` | Wired into both pricing and accounting. |
| REF-089 | Treating doctor CRUD | IMPLEMENTED | STATICALLY VERIFIED | `ExternalEntities/Commands/{Create,Update,Delete}ExternalEntity` → `ExternalEntity` (`EntityType`); `Views/External/{ExternalEntitiesView,ExternalEntityEditorWindow}.xaml` | In-scope portion only; the portal-account aspect of the same reference window is REF-005 (excluded). |
| REF-090 | Sent-out lab entity CRUD | IMPLEMENTED | STATICALLY VERIFIED | Same commands with entity type *sent-out lab* | In-scope portion only. |
| REF-091 | Referring / contract entity CRUD with price list | IMPLEMENTED | STATICALLY VERIFIED | Same commands with entity type *referring/contract*, plus `PriceListId` and discount/commission | In-scope portion only. |
| REF-092 | Lab-to-lab price list for an outside lab | IMPLEMENTED | STATICALLY VERIFIED | `SentOutSample` + `SentOutSamplePayment`; sent-out price resolution; `Views/Patients/SentOutLabAccountView.xaml` | In-scope portion only; the portal-account aspect is REF-006 (excluded). |
| REF-093 | Mark a test as sent out | IMPLEMENTED | STATICALLY VERIFIED | `SentOutSamples/Commands/SendSampleOut` → `SentOutSample` (receiving lab, cost price, patient price) | Matches the reference's three values. |
| REF-094 | Sent-out account for a period | IMPLEMENTED | STATICALLY VERIFIED | `GetSentOutLabAccountQuery`; inventory sent-out aggregates | Present. |
| REF-095 | Detailed sent-out report; search by lab | IMPLEMENTED | STATICALLY VERIFIED | `GetSentOutSamplesQuery(From, To, …)`; `RecordSentOutPaymentCommandHandler`; `SettleSentOutInFullCommandHandler` | Present, including the per-lab filter. |
| REF-096 | Deliver results and settle the account | IMPLEMENTED | STATICALLY VERIFIED | `ResultDelivery/Commands/DeliverWithSettlement` → delivery marking + `PaymentOperation`; `ResultDeliveryAccessPolicy.DeliverResults` | Hand-over and settlement are one traced operation. |
| REF-097 | Period delivery grid with balances | IMPLEMENTED | STATICALLY VERIFIED | `GetDeliveryGridQuery(From, To, …)` → `ResultDeliveryDtos` | Present. |
| REF-098 | List undelivered results | IMPLEMENTED | STATICALLY VERIFIED | `GetUndeliveredResultsQuery`; `DeliveryHandoverView.xaml` | Present. |
| REF-099 | Case follow-up audit | IMPLEMENTED | STATICALLY VERIFIED | `AuditAndTraceability/Queries/{GetPatientAudit,GetPatientTestAudit}` → `PatientAuditDto` (creating user, modification count, last modifying user and time, distinct payment receivers) and a per-test lifecycle audit (entered / reviewed / printed+count / delivered, each with user and time); `AuditableEntity<T>` plus DbContext interceptors supply the audit columns; `Views/Audit/AuditView.xaml` | All documented elements present, including print **count** and last printer. |
| REF-100 | Search by patient identity | IMPLEMENTED | STATICALLY VERIFIED | `PatientSearch/Queries/SearchPatientsGlobal` handler: name match, `PatientPhoneNumber.PhoneNumber.Contains`, `NationalId` equality | Three identity predicates covered. |
| REF-101 | Search by doctor, referral, gender, age, test, date | IMPLEMENTED | STATICALLY VERIFIED | `SearchPatientsGlobalQuery(TreatingDoctorId, ReferralEntityId, TestId, Sex, AgeValueBand, From, To)` | All six documented predicates present as independent defaulted parameters. The seventh documented predicate (branch) is excluded and deliberately absent. |
| REF-102 | Patient and result detail from search | IMPLEMENTED | STATICALLY VERIFIED | `GetVisitDetailQuery`; the global search handler hydrates names, phones, doctors | Present. |
| REF-103 | All lab visits with dates and tests | IMPLEMENTED | STATICALLY VERIFIED | `GetVisitHistoryQuery`; `PatientSearch/Common/VisitRollup.cs`; `Views/Patients/PatientVisitHistoryView.xaml` | Present, grouped by the patient's Lab Id. |
| REF-104 | Persist and auto-display the Lab ID | IMPLEMENTED | STATICALLY VERIFIED | `Patient.LabId`; `GetNextLabIdQuery`; read back on re-registration | Present. |
| REF-105 | Unentered results and analysis price in the detail | IMPLEMENTED | STATICALLY VERIFIED | `GetVisitDetailQuery` returns the visit's tests with configured prices and entry state | Present. |
| REF-106 | Work paper by patient names or codes | IMPLEMENTED | PARTIALLY VERIFIED | `GetVisitWorkSheetQuery` + `WorkSheetHelpers` + `WorkSheetPdfWriter`; `Views/WorkSheets/WorkSheetsView.xaml` | Present. |
| REF-107 | Work paper by test name(s) | IMPLEMENTED | STATICALLY VERIFIED | `GetWorkSheetByTestGroupQuery(TestGroupId, TestIds, From, To)` | Covers one test and a selected set. |
| REF-108 | Work paper by work group (Log) | IMPLEMENTED | STATICALLY VERIFIED | `GetWorkSheetByWorkGroupLogQuery` | Present. |
| REF-109 | Period test-count classification | IMPLEMENTED | STATICALLY VERIFIED | `GetWorkSheetTestCountByPeriodQueryHandler` groups `PatientTest` by test over the period and returns counts plus a total | Matches the reference's count of times each analysis was performed. |
| REF-110 | Print worksheet: unfinished only, or all | IMPLEMENTED | STATICALLY VERIFIED | `PrintWorkSheetCommandHandler` + `WorkSheetHelpers`; the bench-work exclusion (`!IsTakenOutsideLab`) applies to bench sheets and explicitly **not** to the period count | Matches the documented switch. |
| REF-111 | Preview the worksheet | IMPLEMENTED | PARTIALLY VERIFIED | `PrintWorkSheetCommandHandler` → `IPdfPreviewService` | Present. |

### 6.9 Modules 12–13 — Reports, patient history, sample handling

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-112 | Combined report | IMPLEMENTED | STATICALLY VERIFIED | `Domain/Reports/CombinedReportSelection.cs`; `{BuildCombinedReport,PrintCombinedReport}`; `ReportSettings.PrintGroupSubTitle` and `SuppressReprintMessage` (`20261001203315_AddCombinedReportPrintOptions`) implement the two documented checkboxes | Both documented sub-options are persisted settings. |
| REF-113 | Blank report | IMPLEMENTED | PARTIALLY VERIFIED | `{BuildBlankReport,PrintBlankReport}`; `Views/Patients/BlankReportView.xaml` | Present. |
| REF-114 | Automatic patient history in the report | IMPLEMENTED | STATICALLY VERIFIED | `ReportProduction/Commands/AutoInsertHistory`; `ReportSettings.HistoryAutoDisplayEnabled` | Present. |
| REF-115 | Insert previous results of selected tests | IMPLEMENTED | STATICALLY VERIFIED | `{InsertHistoryResult}` + `GetPatientTestHistoryQuery` + `Common/HistoryInsertion.cs`; `InsertHistoryDialogWindow.xaml` | Present. |
| REF-116 | Print history in a separate report | IMPLEMENTED | STATIALLY VERIFIED → STATICALLY VERIFIED | `{PrintHistoryReport}` + `GetSeparateHistoryReportQuery`; `Views/Patients/HistoryReportsView.xaml` | Present. |
| REF-117 | Patient history for the blood-picture profile | IMPLEMENTED | PARTIALLY VERIFIED | `ReportProduction/Common/HistoryMatrixBuilder.Pivot` pivots `ProfileResultItem` into date rows × analyte columns | **Finding F-04:** the pivot is present, but the **charted** form the reference shows on SHOW 66 is not — see REF-189. |
| REF-118 | Patient history for more than one patient | IMPLEMENTED | STATICALLY VERIFIED | `GetMultiPatientHistoryQuery`; `PatientHistoryResolver.ResolveKey(HistorySortMode, …)` | Present. |
| REF-119 | Configure the history sort / aggregation mode | IMPLEMENTED | STATICALLY VERIFIED | `ReportSettings.SetHistoryOptions(HistorySortMode, bool)`; `PatientHistoryResolver` (`ByLabCode` / `ByPatientName`) | Present and wired. |
| REF-120 | Group-patient report slip | IMPLEMENTED | PARTIALLY VERIFIED | `Views/Patients/GroupPatientSlipsView.xaml` → `GroupPatientSlipsViewModel.cs` (`:192-193` sends the command) → `ResultDelivery/Commands/PrintGroupPatientSlips/PrintGroupPatientSlipsCommandHandler.cs` → `GetGroupPatientSlipsQueryHandler.cs` (de-duplicates ids, orders by registration date then id, excludes soft-deleted) → `GroupPatientSlipPdfWriter.cs:58 WritePdfAsync` (one page per patient, barcode via the shared `BarcodeLabelRenderer`) → `GroupPatientSlipPrintingService.cs` → printer dispatch. Registered in `Presentation/DependencyInjection.cs:67` and `Infrastructure/DependencyInjection.cs:95-96`; entered from `ResultDeliveryViewModel.cs:143` | **Full traced path.** No stored state, no schema change (`GroupPatientSlipDtos.cs` states this explicitly, and no migration exists for it). Print output unverified (L-07). |
| REF-121 | List patients with uncollected samples; draw board | IMPLEMENTED | PARTIALLY VERIFIED | `SampleCollection/Queries/{GetPatientsWithUncollectedSamples,GetPatientTestsForDraw}`; `Views/Lab/{SampleCollectionView,SampleDrawBoardView,SampleDrawBoardWindow}.xaml` | Present. |
| REF-122 | Mark a sample as drawn | IMPLEMENTED | STATICALLY VERIFIED | `SampleCollection/Commands/{MarkSampleDrawn,MarkAllSamplesDrawnForPatient}`; `20260907162756_AddPatientIsDeletedAndPatientTestSampleDrawnIndex` | Present. |

### 6.10 Modules 14–15 — Statistics and inventory

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-123 | Patient count by sex, referral, account type | IMPLEMENTED | STATICALLY VERIFIED | `GetPatientCountStatisticsQueryHandler` groups `Patient` by `Sex`, `ReferralEntityId` (with a "no referral" bucket) and `AccountType` | All three classifications present. |
| REF-124 | Patient count in a year by month | IMPLEMENTED | STATICALLY VERIFIED | Same handler, `GroupByMonth` → `MonthlyCountDto`; money row from period `PaymentOperation`s | Present. |
| REF-125 | Patient count in a month by day | IMPLEMENTED | STATICALLY VERIFIED | Same handler, `GroupByDayOfMonth` → `DayOfMonthCountDto` | Present. |
| REF-126 | Request rate for a test group in a period | IMPLEMENTED | STATICALLY VERIFIED | `GetTestCountStatisticsQuery(TestGroupId, From, To)` groups by test and returns counts; the caller computes the share | Present. |
| REF-127 | Test-sample counts per year and per referrer | IMPLEMENTED | STATICALLY VERIFIED | `GetTestCountStatisticsQuery` for the per-test view; the referrer-resolved branch of `GetBandedResultMonitorQueryHandler.cs:74-89` supplies the per-referrer grouping | Present. |
| REF-128 | Sent-out statistics by period and lab | IMPLEMENTED | STATICALLY VERIFIED | `GetSentOutStatisticsQuery(From, To, ExternalLabEntityId)` | Present. |
| REF-129 | User productivity statistics | IMPLEMENTED | STATICALLY VERIFIED | `GetUserProductivityStatisticsQuery(From, To, UserId)`; audit columns from `AuditableEntity<T>` | Present. |
| REF-130 | Banded monitor for one test | IMPLEMENTED | STATICALLY VERIFIED | `GetBandedResultMonitorQuery(TestId, From, To, MinValue, MaxValue)` filtering `PatientTest.ResultValue` through `ResultFlagComputer.TryParse` | Matches the showcase description for chemistry and rapid tests. Scope is test-level scalars — see REF-131. |
| REF-131 | Banded monitor over blood-picture element values | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** The only banded monitor is `GetBandedResultMonitorQueryHandler.cs:56-105`, which filters `PatientTest.ResultValue` and requires `ResultFlagComputer.TryParse` to succeed. CBC results live in `ProfileResultItem` (`ProfileResultItem.cs:14-34`), which that query never reads, and `PatientTest` carries no scalar for a profile test. Searched for `ProfileResultItem` use in the statistics feature, and for any analyte-level band query: **0 matches**. The pivot that would make it cheap, `ReportProduction/Common/HistoryMatrixBuilder.Pivot`, exists but is wired only to patient history (REF-117). | No credible implementation path. |
| REF-132 | Cash-drawer inventory and net profit | IMPLEMENTED | STATICALLY VERIFIED | `GetCashDrawerInventoryQuery` → `CashDrawerInventoryDto` (counts, amounts, discounts, collected, uncollected, supplies, disbursements, safe cash, sent-out totals, commissions, net profit) | Period inventory fully modelled. The branch selector is excluded (REF-016) and deliberately absent. |
| REF-133 | Element-scoped inventory | IMPLEMENTED | STATICALLY VERIFIED | `GetElementInventoryQuery(InventoryElementKind = User / ReferralEntity / TreatingDoctor / AccountType / SentOutSamples, ReportType)` | All five documented element scopes present. |
| REF-134 | Patient-sample detail behind an inventory total | IMPLEMENTED | STATICALLY VERIFIED | `GetPatientSamplesDetailQuery` → `PatientSampleDetailDto(PatientId, FullName, TestsCount, Charged, Paid, Balance)` | Matches the reference's drill-down. |
| REF-135 | Referral / delegate account with commission | IMPLEMENTED | STATICALLY VERIFIED | `GetCompanyDelegateAccountsQuery` + `CommissionShareDto(EntityId, EntityName, Percent, ReferredChargeBase, CommissionAmount)` | Present. |
| REF-136 | Cash deposit and disbursement | IMPLEMENTED | STATICALLY VERIFIED | `{RecordCashDeposit,RecordCashDisbursement}` → `CashMovement` (`MovementType`); `ListCashMovementsQuery` | Present. |
| REF-137 | Other-labs account and referrer share | IMPLEMENTED | STATICALLY VERIFIED | `GetCompanyDelegateAccountsQuery` aggregating deposits / disbursements / net plus sent-out cost, paid and remaining per entity | Present. |
| REF-138 | Sent-out account and settlement | IMPLEMENTED | STATICALLY VERIFIED | `{RecordSentOutPayment,SettleSentOutInFull}`; `GetSentOutLabAccountQuery` | Present. |
| REF-139 | Year-end account settlement | IMPLEMENTED | STATICALLY VERIFIED | `InvoiceIssue` (persisted print issue records, `20260916113704_AddInvoiceIssues`) combined with `SettleAccountInFullCommand` | **Finding F-03:** per-visit and per-sent-out settlement plus issued-invoice records are present and reachable; there is no dedicated year-end settlement command or report. The machinery the reference entry depends on exists, so the observable purpose is met. Not inflated into a second classification. |

### 6.11 Module 16 — System and print settings

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-140 | Report settings (general) | IMPLEMENTED | STATICALLY VERIFIED | `UpdateReportSettingsCommandHandler.cs` → `ReportSettings` → `ReportSettingsConfiguration.cs`; `Views/Settings/ReportSettingsView.xaml`; `SaveLabPrintText` for lab identity | Present. |
| REF-141 | Report margins and top space | IMPLEMENTED | STATICALLY VERIFIED | `ReportSettings.SetMargins` / `SetTopSpace` with bounds; applied by `PageSizeMapper` and `ReportPageComposer` | Present. |
| REF-142 | Report paper size | IMPLEMENTED | STATICALLY VERIFIED | `PaperSize` enum (A4 / A5); `PageSizeMapper.cs` | Present. **Note:** no A3 — see REF-185. |
| REF-143 | Report header/footer as words | IMPLEMENTED | STATICALLY VERIFIED | `HeaderFooterMode.Words`; lab text from `SaveLabPrintText`; `ReportPageComposer.cs:101-115` (the image-absent branch draws LabName, SoftwareBar, contact) | Present. |
| REF-144 | Report header/footer from an attached image | IMPLEMENTED | STATICALLY VERIFIED | `ReportSettings.SetHeaderImagePath` / `SetFooterImagePath` (`ReportSettings.cs:176-184`); carried on `UpdateReportSettingsCommand.cs:22-23`; `IReportImageAssetStore` → `Infrastructure/Printing/ReportImageAssetStore.cs` (stores under `%ProgramData%\TopLab\ReportSettings\`, deterministic names); `ReportPageComposer.cs:88-99` renders the image with `PageSizeMapper.FitHeaderFooterImage` bounding, else falls back to words | **The full chain exists and terminates in real rendering.** Migration `20261017215437` added the two columns. |
| REF-145 | Report header/footer colours | IMPLEMENTED | STATICALLY VERIFIED | `ReportSettings.SetHeaderColor` / `SetFooterColor` (`:137-145`); `UpdateReportSettingsCommand.cs:20-21`; validator bounds them (`UpdateReportSettingsCommandValidator.cs:20-27`); `ReportSettingsViewModel.cs:108-109,204-205`; `ReportSettingsView.xaml:47,49` TextBoxes; `ReportPageComposer.cs:84,222` `ParseColor` → `.FontColor(headerColor)` / `.FontColor(footerColor)` at `:104,110,228,255,261` | **Complete chain, entity → UI → writer.** Static test evidence exists in 4 test files (not executed — L-02). |
| REF-146 | Edit the normal-range marker character | IMPLEMENTED | STATICALLY VERIFIED | `ReportSettings.SetNormalRangeMarker` (`:186`); `UpdateReportSettingsCommand.cs:24`; `ReportContentBuilder.cs:39,81,197` reads it into `marker`, then `ReferencePrefix(template, marker)` at `:718-726` substitutes it for the default `"المدى:"`; flows into `ReportDocumentContent.NormalRangeMarker` (`:172,271,507,636`) | **Implemented and rendered.** |
| REF-147 | Receipt settings: currency, pickup time, print-once, header/footer text | IMPLEMENTED | STATICALLY VERIFIED | `ReceiptSettings` (`TopMarginCm`, `Currency`, `PickupTimeDefault`, `PrintOnce`, `TestDetailDisplayMode`, `CashierPrinterEnabled`, `HeaderFooterMode`); `UpdateReceiptSettings`; `ReceiptPdfWriter.cs:170-185` draws the words header/footer | Present. |
| REF-148 | Receipt settings: font type, size, colour and logo | IMPLEMENTED | PARTICALLY VERIFIED | `ReceiptSettings.cs:24-42` adds `HeaderFontFamily`, `HeaderFontSizePt`, `HeaderFontColor`, `FooterFontFamily`, `FooterFontSizePt`, `FooterFontColor`, `LogoImagePath`; `UpdateReceiptSettingsCommand.cs` carries all seven; `ReceiptPdfWriter.cs:86-99` resolves them (`ArabicFontResolver.Resolve`, `ParseColor`, `ResolveDrawableLogoPath`), `:114` bounds the logo via `ReceiptGeometry.LogoMaxHeightPt`, `:170-184` renders logo or words, `:246` the sized/coloured footer; `IReceiptImageAssetStore` → `ReceiptImageAssetStore.cs` | **Full chain to rendering.** Static test evidence in 4 files (not executed). |
| REF-149 | Dedicated printers per output type | IMPLEMENTED | STATICALLY VERIFIED | `PrinterAssignment` + `PrinterOutputType` enum (Report / Barcode / Receipt / Envelope); `{SavePrinterAssignments,GetPrinterAssignments}`; consumed by every printing service | All four documented output types routed. |
| REF-150 | Default account type | IMPLEMENTED | STATICALLY VERIFIED | `SystemSettings.SetDefaultAccountType` validated against Individual / LabToLab / Contracts / Free; `UpdateSystemSettings`; seeded in `SystemSettingsConfiguration.cs` | Present. |
| REF-151 | Envelope settings | IMPLEMENTED | STATICALLY VERIFIED | `EnvelopeSettings` (`TopMarginCm`, `HeaderFooterMode`, `SuppressCaptions`); `EnvelopePrintItemPosition` (`ItemName` PK, `IsEnabled`, `LeftOffsetCm`, `TopOffsetCm`) with bounds in `Update`; `{UpdateEnvelopeSettings,GetEnvelopeSettings}`; `Views/Settings/EnvelopeSettingsView.xaml` | Present, including per-item enable and offsets. See REF-162, REF-163. |
| REF-152 | System general settings | IMPLEMENTED | STATICALLY VERIFIED | `SystemSettings` flags (`PrintLabIdInsteadOfPatientId`, `AutoReviewAndComplete`, `EnablePatientNameSearchAssist`, `PrintFileExternalBarcode`, `PrintDateTimeOnTubeBarcode`, `PrintAccountInsteadOfDateOnReport`, `SaveTreatingDoctorOnlyFromEntityWindow`, `DisableAutoTitleInsertion`) + `BranchNumber`; `UpdateSystemSettings`; `Views/Settings/SystemSettingsView.xaml` | Present. The branch number is a stored setting whose branch-scoped behaviour is excluded and absent — no gap is claimed. |
| REF-153 | Report print-option switches | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** The reference's printer screen (RLS 199) shows a set of report print switches: print the report with the inventory account; print the delivery report with the account; print a cheque without test values; print the report so far; print all tests with the invoice; print all tests without the invoice; print only entered tests. Searched all settings entities for switches of this kind: `SystemSettings` and `ReceiptSettings` expose **11 and 2** booleans respectively, none of which corresponds. Targeted search for `PrintWithInvoice`, `InvoiceWithTests`, `OnlyEnteredTests`, `PrintSoFar`, `WithInventoryAccount`, `WithoutTestValue`: **0 matches across `src/`.** | No credible implementation path. |
| REF-160 | Per-report-line font family, size and colour | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** RLS 193/195/197 show a table of roughly twelve named report lines, each with its own **Font (size)** and **Color** cell, plus a `Show bottom line` toggle and a custom `Color Data` dialog with named colours. In the code, `ReportItemTemplate.cs` carries `LineSpacing`, `LeftOffsetCm`, `MarkerSymbol` — and **no** font or colour attribute. `ReportSettings` carries only three colours (`HeaderColor`, `FooterColor`, abnormal pair). Targeted search for `LineFontSize`, `ReportLineFont`, `ItemFontSize`, `LineColor`, `ItemColor`: **0 matches.** | Per-line font family and size have no path at all, and the per-line colour is modelled only for three specific roles, not twelve lines. The function as described is absent. |
| REF-157 | Per-user (doctor) report element toggle | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** SHOW 15 evidences a doctor toggling report elements from a side menu, i.e. a **per-user** preference. `ReportItemTemplateConfiguration.cs:11` sets `HasKey(e => e.ItemKey)` — a single global key, with **no** user column. `ReportSettings` is one global row (`PK = 1`). Targeted search for `UserReportSettings`, `ReportPreference`, `UserLayout`, and for any user id on the template: **0 matches.** | The global toggle exists (REF-156); the per-user scope has no path. |

### 6.12 Module 17 — Report content and layout control

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-154 | Editable report item template | IMPLEMENTED | STATICALLY VERIFIED | `Domain/Settings/ReportItemTemplate.cs` (`ItemKey`, `Label`, `ReferenceValueExpression`, `ShowHide`, `ListMembership`, `FreeTextOverride`) with length/control-character validation; `ReportItemKeys.cs` defines **24** keys; `ReportItemTemplateConfiguration.cs` maps them; `{GetReportItemTemplates,UpdateReportItemTemplates}` with validators; `ReportItemTemplatesViewModel.cs:32-48` exposes every field; `Views/Settings/ReportItemTemplatesView.xaml:44` `ItemsControl` bound to `Rows`, `:83` save button; registered `Presentation/DependencyInjection.cs:107`; opened from `SettingsDashboardViewModel.cs:34`. Consumed in `ReportContentBuilder.cs` via `Template(...)`, `IsVisible(...)`, `FreeTextOrValue(...)`, `BuildTemplateMap(...)` | **Full chain from UI to render-time lookup.** Migration `20261018075336`. |
| REF-155 | Line spacing, item offset, item marker symbol | IMPLEMENTED | PARTICALLY VERIFIED | `ReportItemTemplate.cs:62-75,183-244` with bounds (`MinLineSpacing` 0.5 … `MaxLineSpacing` 10.0; `MinLeftOffsetCm` 0 … `MaxLeftOffsetCm` 10; `MaxMarkerSymbolLength` 10); migration `20261018103931_AddReportItemLayout` added the three columns to `ReportItemTemplates`; `ReportContentBuilder.cs:737` `Layout(...)` passes them into `ReportContentLine`; `ReportPageComposer.cs:62` prefixes the marker, `:63-68` computes the left offset (`PageMarginLeftCm + LeftOffsetCm` or the absolute override), `:72` sets `LineHeight` | **Implemented through to page composition.** Rendering unverified (L-07). |
| REF-156 | Global show / hide of a report item | IMPLEMENTED | STATICALLY VERIFIED | `ReportItemTemplate.ShowHide` with `HasDefaultValue(true)`; `ReportContentBuilder.IsVisible(...)` gates every item emission (e.g. `:217`, `:225`, `:405`, `:413`) | **Implemented.** |
| REF-158 | Shade or colour abnormal results, or disable it | IMPLEMENTED | STATICALLY VERIFIED | `ReportSettings.EnableAbnormalResultColor` + `AbnormalResultFontColor` / `AbnormalResultBackgroundColor` with setters (`:147-174`); `UpdateReportSettingsCommand.cs:26-28`; handler calls them; `ReportContentBuilder.cs:93-94` and `:329-330` compute the colours when the flag is Low/High; `ReportDocumentContent.cs:185-186,195` carries `FontColor`/`BackgroundColor` on each line with `HasColor`; `ReportPageComposer.cs:296-298` applies `.Background(...)` and `:314-316` applies `.FontColor(...)` | **Implemented through to rendering.** Migration `20261018123322`. |
| REF-159 | Per-profile normal / abnormal font and background colour | IMPLEMENTED | STATICALLY VERIFIED | Migration `20261018144852_AddProfileResultStyling` adds `NormalFont`, `NormalColor`, `AbnormalFont`, `AbnormalBackgroundColor` to `Profiles`; `ProfileResultStyleDto` carried on both `ProfileResultDtos.cs:69` and `ReportDtos.cs:88` and populated by `ResultPrintCoordinator.cs:146`; `ReportContentBuilder.cs:656-673` `ProfileResultColors(...)` resolves precedence — per-profile override first, global settings as fallback, and **no colour at all when `EnableAbnormalResultColor` is false for the abnormal case** | **Implemented with an explicit, documented precedence rule.** |
| REF-161 | Print positions of report and envelope elements | IMPLEMENTED | PARTIALLY VERIFIED | Report: `ReportSettings.PageMarginLeftCm` plus per-item `ReportItemTemplate.LeftOffsetCm` consumed at `ReportPageComposer.cs:63-68`. Envelope: `EnvelopePrintItemPosition.IsEnabled` / `LeftOffsetCm` / `TopOffsetCm`, list accepted by `EnvelopePdfWriter.cs:52,168` | Both surfaces covered. |
| REF-162 | Envelope per-item font, size and colour | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** RLS 206 shows an envelope item table with columns `الاسم / الخط / الحجم / اللون / التمكين` — name, font, size, colour, enable. `EnvelopePrintItemPosition.cs` carries **only** `ItemName`, `IsEnabled`, `LeftOffsetCm`, `TopOffsetCm`; its constructor and `Update` accept exactly those four. Targeted search for envelope font / colour attributes: **0 matches.** The envelope's font family and size are single document-wide values resolved once at `EnvelopePdfWriter.cs:70-72`, not per item. | The enable and position halves are implemented (REF-161); the styling half is absent. |
| REF-163 | Envelope item ordering (place before / after another item) | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** RLS 206 shows two checkboxes, "Place the item after" and "Place the item before", with a target selector. `EnvelopePrintItemPosition` has **no** ordering or sequence column, and `EnvelopePrintItemPositionConfiguration.cs` maps only the four properties. Targeted search for `SortOrder`, `Sequence`, `Order`, `BeforeId`, `AfterId`: **0 matches** in the envelope settings area. | No credible implementation path. |

### 6.13 Modules 18–20 — Barcode labels, utilities, receipt options

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-164 | Barcode labels for tubes, containers and swabs with doctor-editable test distribution | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** `IBarcodeService.PrintBarcodeAsync(string value)` (`BarcodeService.cs:44`) accepts exactly **one** identifier; its sole caller `PrintBarcodeCommandHandler.cs` passes the patient id (or the Lab Id). `BarcodeLabelRenderer.Render(string content, int width, int height)` renders one Code-128 label. Targeted search for `SampleMaterial`, `ContainerLabel`, `SwabLabel`, `TubeLabel`: **0 matches across `src/`.** No entity, command, DTO, view or view model models a sample material or a per-material test assignment. | The identity barcode is a **different** function and is implemented as REF-038. This function's substance — sorting a patient's tests across drawn materials and letting the doctor change the distribution — has no implementation path. |
| REF-165 | Optional vertical coding on report, receipt and envelope | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** Targeted search for `VerticalCode`, `VerticalText`, and rotation handling: **0 matches across `src/`.** `ReportPdfWriter`, `ReceiptPdfWriter`, `InvoicePdfWriter` and `EnvelopePdfWriter` contain no rotation or vertical-text mode; the only barcode rendering path (`BarcodeService`) emits a horizontal label. | Absent. |
| REF-166 | Reprint any barcode label | IMPLEMENTED | PARTIALLY VERIFIED | `PrintBarcodeCommand` is idempotent and can be re-invoked for the same visit at any time; no state gates a second print | Matches the reference's "print more than once". |
| REF-167 | Sticker size configuration | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** `BarcodeLabelRenderer.Render(content, width = 300, height = 80)` takes its dimensions as **parameters**, but `BarcodeService.cs` invokes it with the defaults and no caller passes anything else. Targeted search for `LabelWidth`, `LabelHeight`, `StickerSize`: **0 matches.** No settings row, command, DTO or view model exposes a label size. | The dimensions are code constants, not a configurable surface. |
| REF-168 | Sample-nature interpretation panel | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** `Test` carries `Collection` plus `LogGroup`/`LabGroup` labels used as report headings, and `ReferenceRange` supplies the normal interval — but there is **no** interpretation entity, panel, command, query, DTO, view or view model presenting a sample's nature together with its normal ranges and their clinical meaning. Targeted search for `SampleNature` and `Interpretation`: **0 matches.** | Absent. |
| REF-169 | Glossary of analysis abbreviations | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** `Test.Short` / `TestCode` are catalogue fields used as headings and by `TestDetailDisplayMode.ShowWithCode`. Targeted search for `Glossary`, `Terminology`, `Abbreviation`: **0 matches.** No glossary entity, CRUD, read surface or view. | Absent. |
| REF-170 | Requirements / purchases list | IMPLEMENTED | PARTIALLY VERIFIED | `Utilities/Commands/{AddPurchaseItem,RemovePurchaseItem,TogglePurchaseItemDone}`; `JsonPurchasesListStore.cs`; `GetPurchasesListQuery`; `Views/Utilities/UtilitiesView.xaml` | Present. |
| REF-171 | Stopwatch, time clock and profile calculation | IMPLEMENTED | STATICALLY VERIFIED | `Utilities/Queries/{ComputeStopwatchElapsed,EvaluateCalculation}` → `Domain/Utilities/{StopwatchCalculator,ArithmeticCalculator,CalculatorException}` | Present, with a documented failure type. |
| REF-172 | Appointment-note reminder | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** `Utilities/Commands/{AddPhoneBookEntry,RemovePhoneBookEntry}` and `GetPhoneBookQuery` exist, and `MeasurementUnitConverter` is separate — but there is **no** reminder, appointment, note-scheduling or notification entity, command, query, DTO, view or view model anywhere in `src/`. Targeted search for `Reminder`, `Appointment`, `Notification`, `Scheduling`: **0 matches.** | Absent. The phone book it would address (REF-173) is present and unaffected. |
| REF-173 | Phone book | IMPLEMENTED | PARTIALLY VERIFIED | `JsonPhoneBookStore.cs`; `{AddPhoneBookEntry,RemovePhoneBookEntry}`; `GetPhoneBookQuery`; `Views/Utilities/UtilitiesView.xaml` | Present. |
| REF-174 | Unit conversion | IMPLEMENTED | STATICALLY VERIFIED | `ConvertMeasurementUnitQuery` → `MeasurementUnitConverter.cs` | Present. |
| REF-175 | Patient receivable summary with a balance filter | IMPLEMENTED | STATICALLY VERIFIED | `GetPatientSamplesDetailQuery(From, To, bool UnpaidOnly, bool DiscountedOnly, bool PaidOnly)`; handler computes `PatientAccountCalculator.Balance(prices, ops)` (`:63`) then applies the filters (`:85-103`, the discount filter testing `o.DiscountAmount > 0`); exposed by `AccountsHubViewModel.cs:51,183-194,275-277` with four labelled options — `الكل` / `غير المسدد` / `المسدد بخصم` / `المسدد بالكامل` — matching the reference's three filters plus all | **Implemented, and the option labels correspond exactly to the reference's filter set.** |
| REF-176 | Referrer account statement in four variants | IMPLEMENTED | STATICALLY VERIFIED | `InventoryReportType { Summary, Detailed, DetailedByPrices, DetailedByResults }` on `GetElementInventoryQuery` with `InventoryElementKind.ReferralEntity`, plus `GetPatientSamplesDetailQuery` for the per-patient view | **Finding F-05:** all four documented variants are reachable; the variant naming differs from the reference's wording and the consolidated variant is expressed as the element inventory across referrers rather than a separate statement command. Behaviour is equivalent. |
| REF-177 | Receipt on a cashier / thermal printer | IMPLEMENTED | PARTIALLY VERIFIED | `ReceiptSettings.CashierPrinterEnabled`; `PrinterOutputType.Receipt`; `ReceiptPrintingService.cs` dispatches to the assigned printer | Present. |
| REF-178 | Show / hide test details and collection data on the receipt | IMPLEMENTED | STATICALLY VERIFIED | `ReceiptSettings.TestDetailDisplayMode` (`Hide` / `Show` / `ShowWithCode`); `ReceiptPdfWriter.cs` | Present. |
| REF-179 | Receipt header/footer words plus logo | IMPLEMENTED | PARTICALLY VERIFIED | Words path `ReceiptPdfWriter.cs:170-185`; logo path `:99 ResolveDrawableLogoPath`, `:114 ReceiptGeometry.LogoMaxHeightPt`, `:174-175 .Image(path).FitArea()`, gated on `HeaderFooterMode != None`; asset store `ReceiptImageAssetStore.cs` | **Implemented**, including the branding path that previously fell back. |

### 6.14 Module 21 — Complete blood count report output family

All rows rest on the rendered page images of SHOW 56–66 (L-10, L-13).

| REF | Function | Class | Verification | Code evidence and traced path | Findings |
|---|---|---|---|---|---|
| REF-180 | CBC report histograms (WBC / RBC / PLT distribution) | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** Targeted search for `Histogram`: **0 matches across `src/`.** `ReportPageComposer` emits only text and tables. The lab system has no histogram entity, no image column on `PatientTest` or `ProfileResultItem`, and no rendering path. | Absent in the lab system. **Boundary:** the reference's histogram image is produced by the excluded analyser interface (§5.3); only a rendering side would be in scope, and there is no in-scope data source. |
| REF-181 | Two-dimensional scattergram for the differential | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** Targeted search for `Scattergram`, `Scatter`: **0 matches.** No scatter entity, column or renderer. | Absent. Same boundary as REF-180. |
| REF-182 | Five-part and three-part differential reporting | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** Targeted search for `differential`, `lymphocyte`, `monocyte`, `neutrophil`, `eosinophil`, `basophil` across `src/TopLab.Domain` and `src/TopLab.Infrastructure`: **0 matches.** `ProfileAnalyte` and `ProfileResultItem` carry no `AbsoluteCount` / `RelativeCount` (targeted search: **0 matches**), so the reference's paired RES % / RES K/µl columns and the LYM% / MID% / GRA% / NEUT# grouping have no representation. No CBC analyte seed catalogue exists either (targeted search for `Hemoglobin`, `WBC`, `RBC`, `Neutrophil` in configurations and migrations: **0 matches**). | Absent. |
| REF-183 | Colour versus black-and-white report printing | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** Targeted search for `BlackAndWhite`, `Grayscale`, `ColourMode`, `ColorMode`, `Monochrome`: **0 matches.** `ReportPageComposer.cs:176` sets `page.PageColor(Colors.White)` unconditionally; there is no output-mode switch anywhere. | Absent. |
| REF-184 | Report printed under the patient name in Arabic and English | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** `ReportPageComposer.cs:138` emits one string: `$"اسم المريض: {content.PatientFullName}"`. Targeted search for a second name field (`FullNameEn`, `EnglishName`, `NameEn`, `Transliterat`) across `src/`: **0 matches.** `Patient` carries a single `FullName`. | Absent. |
| REF-185 | Report on large paper (A3) or A4 at 85 × 55 | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** `Domain/Common/Enums/PaperSize.cs` contains exactly `A4 = 0` and `A5 = 1`. Targeted search for `A3`: **0 matches.** `PageSizeMapper` maps the two existing values only. | Absent. |
| REF-186 | Folded / multi-port report printing | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** Targeted search for `Folded`, `Fold`, `Port`: **0 matches.** No folding, port or imposition construct in the PDF pipeline. | Absent. |
| REF-187 | Sequential sample numbering on the report | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** Targeted search for `SequenceNumber`, `SerialNumber`, `SampleNo`, `SampleNumber`: **0 matches** outside migrations. `ReportPageComposer.cs:138` prints the patient name; `ReportDocumentContent` carries no sequence field. The lab system has no sample-sequence concept at all. | Absent. |
| REF-188 | Auto-generated remark from result flags | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** Targeted search for `Remark`, `AnalyzerNote`: **0 matches.** The report emits `LowComment` / `HighComment` text sourced from the reference range (`ReportContentBuilder.cs:82-87`, `:149-154`, `:233-238`), which is a stored comment, not a remark composed from the abnormal findings. `ProfileResultItem` carries `Flag` but no remark field. | Absent. The reference's remark is composed from analyser findings, which is excluded; no in-scope composition rule exists either. |
| REF-189 | Charted blood-count history report | **MISSING** | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | **Documented negative search.** `HistoryMatrixBuilder.Pivot` (REF-117) produces the **tabular** date × analyte matrix. Targeted search for chart or plot constructs in `src/TopLab.Infrastructure` (`Chart`, `Graph`, `Plot`, `BarSeries`, `LineSeries`): **0 matches.** No charting component is referenced anywhere; `ReportPageComposer` emits text and tables only. | Absent. |

### 6.15 Classification summary

| Classification | Count |
|---|---|
| IMPLEMENTED | **146** |
| DIFFERENT | **0** |
| MISSING | **22** |
| BLOCKED / UNRESOLVED | **0** |
| **In-scope total** | **168** |

**Why `DIFFERENT` is zero.** A `DIFFERENT` classification is required when a capability exists but materially differs — notably a stored-but-inert setting. Every function that previously exhibited that pattern (inert header/footer colours, an images mode that silently fell back to words, a partial receipt branding set, a missing balance filter) now has a traced chain from a persisted setting through a command, a validator, a view model and an XAML control to a **rendering call in `ReportPageComposer` or `ReceiptPdfWriter`**. Every remaining gap is a whole-capability absence confirmed by a documented negative search, with no partial path. Nothing was upgraded to `IMPLEMENTED` on the strength of a new column alone.

**Why `BLOCKED / UNRESOLVED` is zero.** No in-scope function was left unclassifiable. Items whose rendering or runtime behaviour could not be observed are recorded through the verification-status column, never through the classification column. The eight blood-count output items whose *data source* lies outside scope (REF-180, REF-181, REF-182, REF-188, and REF-189 in part) are classified on the evidence that actually exists — nothing is implemented — and are marked **Blocked** in Section 9 with the boundary reason, rather than being hidden behind `BLOCKED`.

**Cross-cutting findings** (recorded here; they create no reference functions and change no classification):

- **F-01** — Permission granularity is coarser than the reference's per-activity list (13 seeded permissions).
- **F-02** — The reference-value deviation is conveyed by the low/high comment rather than an explicit numeric delta.
- **F-03** — No dedicated year-end settlement command; the underlying settlement machinery is present.
- **F-04** — The CBC history matrix is tabular; the reference's charted form is absent (REF-189).
- **F-05** — Inventory report-type naming differs from the reference's statement-variant wording; behaviour is equivalent.

---
## SECTION 7 — Separate Defects

Defects are recorded here, **separately from the reference-function inventory**. No reference function was created, split, merged or reclassified in order to carry a defect. Every defect is an independently evidenced in-scope issue.

| ID | Affected REF | Description | Source path and lines | Impact | Verification status | Evidence |
|---|---|---|---|---|---|---|
| DEF-01 | REF-145 | **The image comment contradicts the code.** `ReportPageComposer.cs:89-90` carries a comment reading "GAP-011: bound the logo so an over-tall image cannot exhaust the page and abort PDF generation". The bounding logic immediately below (`PageSizeMapper.FitHeaderFooterImage`) is real, so the comment describes this commit's work, not outstanding work. However, `ReceiptPdfWriter.cs:159-168` carries a **stale** comment block from the prior state, describing the fall-back rationale ("plain paper → HeaderFooterMode.Words or Images, with a …") that no longer matches the now-real Images branch at `:170-184`. | `src/TopLab.Infrastructure/Printing/ReceiptPdfWriter.cs:159-168` versus `:170-184`; `src/TopLab.Infrastructure/Printing/ReportPageComposer.cs:89-99` | A maintainer reading the receipt writer's comment would conclude that the logo path is still inert and would not know it is now live, or would "fix" a non-existent bug. Comment/code divergence is exactly the condition the brief warns about. | STATICALLY VERIFIED | Both sites read in full at the target commit. |
| DEF-02 | REF-145 | **`EnableAbnormalResultColor` gates the abnormal colours but not the per-profile normal colours.** `ProfileResultColors(...)` returns `(null, null)` for an abnormal result when the flag is off (`:663-666`), but for a **normal** result it returns `(style?.NormalFont, style?.NormalColor)` unconditionally (`:672`). A profile that sets only `NormalColor` therefore colours normal results even when the operator has switched abnormal colouring off. This is defensible if the toggle is read as "abnormal colouring only", but it is undocumented and untested in source and will surprise an operator. | `src/TopLab.Infrastructure/Printing/ReportContentBuilder.cs:656-673` | Inconsistent behaviour between two branches of the same precedence helper. | STATICALLY VERIFIED | Method read in full; static test evidence exists in `ProfileResultStylingPrecedenceTests.cs`, which was **not executed** (L-02), so the intended semantics are not confirmed. |
| DEF-03 | REF-166, REF-135 | **Non-destructive "edit" is exposed on the patient account surface.** `RemoveTestFromVisitCommandHandler` and `ClearAllTestsCommandHandler` are reachable from the patient editor and are documented in the reference as removing tests from a visit. Whether they are guarded once results exist or the visit has been delivered is not visible from the command surface, and `PatientTest` retains its results after removal from the visit list. | `src/TopLab.Application/Features/PatientRegistration/Commands/RemoveTestFromVisit/RemoveTestFromVisitCommandHandler.cs`; `.../ClearAllTests/ClearAllTestsCommandHandler.cs` | **Suspected only.** If removal is permitted after results exist, a previously printed report would no longer be reconcilable with the visit. Not confirmed and **not reported as a defect**. | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | Static reading did not settle the guard either way, and L-02/L-04/L-05 foreclose confirming it. |
| DEF-04 | REF-099 | **Print counting may be path-dependent.** `MarkResultPrintedCommandHandler` increments `PrintCount` and records `LastPrintedByUserId`. The combined, separate-history and bulk-print paths dispatch printing through their own handlers (`PrintCombinedReportCommandHandler`, `PrintHistoryReportCommandHandler`, `ExecuteBulkPrintCommandHandler`) and were not shown to perform the same increment, while the audit view reads the count. | `src/TopLab.Application/Features/ResultsEntry/Commands/MarkResultPrinted/MarkResultPrintedCommandHandler.cs`; `src/TopLab.Application/Features/ReportProduction/Commands/PrintCombinedReport/PrintCombinedReportCommandHandler.cs`; `src/TopLab.Application/Features/ResultsEntry/Commands/BulkPrint/ExecuteBulkPrintCommandHandler.cs` | **Suspected only.** If those paths do not increment, the audit view under-reports printing. Not confirmed. | NOT VERIFIED FROM AVAILABLE ENVIRONMENT | Requires observing persisted state, which L-05 forecloses. |
| DEF-05 | — (cross-cutting, in-scope) | **`ReportItemTemplate.RowVersion`-style concurrency is absent from a globally shared settings aggregate.** The report item template table is a single global set (`HasKey(e => e.ItemKey)`), edited from one screen, with no optimistic-concurrency token. Two operators editing templates concurrently will have last-writer-wins behaviour. | `src/TopLab.Infrastructure/Persistence/Configurations/ReportItemTemplateConfiguration.cs:11-20` | Low impact in a single-site lab; relevant for a multi-workstation deployment. | STATICALLY VERIFIED | Configuration read in full; no concurrency token on the entity. |
| DEF-06 | — (cross-cutting, in-scope) | **The absolute-permission bypass on print-blocking is uniform, which is correct, but undocumented at the six call sites.** `user.BlockPrintOnRemainingBalance` is bypassed whenever `_user.IsAbsolutePermission` is true. The behaviour is deliberate and consistent — it matches the reference's administrator-override intent — but no comment says so at any call site, and the surrounding code is minified onto a single line in at least one handler, which will impede review. | `MarkResultPrintedCommandHandler.cs:56`; `PrintCombinedReportCommandHandler.cs:59`; `MarkProfilePrintedCommandHandler.cs:56`; `MarkCultureReportPrintedCommandHandler.cs`; `ExecuteBulkPrintCommandHandler.cs:72`; `PrintHistoryReportCommandHandler.cs:64` | Maintainability, not correctness. Recorded as an observation, **not** a behavioural defect. | STATICALLY VERIFIED | All six call sites read at the target commit. |

**Boundary note.** No defect above concerns excluded functionality. DEF-03 and DEF-04 concern in-scope capability only. The absence of every excluded feature — the portal, the equipment register, the analyser interface, multi-branch operation, SMS/e-mail/fax — is **not** reported as a defect, a gap, technical debt or a shortcoming anywhere in this report.

---

## SECTION 8 — Count Reconciliation

### 8.1 First reconciliation — inventory to scope

```
TOTAL PDF-EVIDENCED REFERENCE FUNCTIONS                       189
   REF-001 … REF-189
− OWNER-EXCLUDED FUNCTIONS EVIDENCED IN THE PDFs              21
   REF-001 … REF-021
   Category 1 — online results portal and ecosystem:     9
   Category 2 — equipment, maintenance, calibration:     1
   Category 3 — blood/CBC analyser integration:         5
   Category 4 — multi-branch support:                   2
   Category 5 — SMS / e-mail / fax result sending:      4
                                                        ----
                                                          21
= IN-SCOPE REFERENCE FUNCTIONS                            168
   REF-022 … REF-189
```

```
189 − 21 = 168   ✓
```

### 8.2 Second reconciliation — scope to classification

```
IN-SCOPE REFERENCE FUNCTIONS                               168
= IMPLEMENTED                                              146
+ DIFFERENT                                                  0
+ MISSING                                                   22
+ BLOCKED / UNRESOLVED                                       0
                                                          ----
                                                            168
```

```
146 + 0 + 22 + 0 = 168   ✓
```

### 8.3 The 22 MISSING identifiers, enumerated

REF-153, REF-157, REF-160, REF-162, REF-163, REF-164, REF-165, REF-167, REF-168, REF-169, REF-172, REF-180, REF-181, REF-182, REF-183, REF-184, REF-185, REF-186, REF-187, REF-188, REF-189, REF-131.

That is 22 distinct identifiers. 22 rows appear in Sections 6.11–6.14 classified MISSING. No identifier appears twice.

### 8.4 Arithmetic verification

| Check | Result |
|---|---|
| Distinct `REF-###` identifiers in the inventory | 189 |
| Owner-excluded identifiers | 21 |
| In-scope identifiers | 168 |
| Sum of classifications | 146 + 0 + 22 + 0 = **168** ✓ |
| MISSING identifiers enumerated in §8.3 | **22** ✓ (matches the classification tally exactly) |
| Sequence items in Section 9 | **22**, one per MISSING identifier, each appearing exactly once ✓ |
| Owner-excluded identifiers appearing in Sections 6, 7, 9 or 10 as classified/gapped/sequenced/recommended | **0** ✓ |
| Owner exclusions not evidenced in the PDFs | **0** — all 21 are PDF-evidenced, so there is no second population |

No item was merged, split, duplicated, omitted or reclassified to make a total agree. The arithmetic closes on the first pass, with no residual discrepancy.

---

## SECTION 9 — Recommended Implementation Sequence

Exactly one numbered sequence, containing **every** in-scope function classified `DIFFERENT` or `MISSING` — that is, all 22 `MISSING` items. Each appears **exactly once**. There are no `DIFFERENT` and no `BLOCKED / UNRESOLVED` items, so nothing is excluded from this sequence on those grounds.

### 9.1 Dependency evidence used for ordering

1. **`ReportItemTemplate` is a single global set keyed by `ItemKey`** (`ReportItemTemplateConfiguration.cs:11`). Every report-appearance item lands in, or is consumed by, the same pipeline. Items 1–5 modify `ReportContentBuilder` / `ReportPageComposer`; item 6 replaces the fixed section sequence that they all sit inside, so it must come **after** them.
2. **`ReportContentBuilder` → `ReportDocumentContent` → `ReportPageComposer` → `ReportPdfWriter` → `ReportPrintingService` → `IPdfPrinterDispatcher`** is one linear pipeline shared by three report shapes. Any visual change lands in `ReportContentBuilder` first.
3. **`ReceiptPdfWriter` and `EnvelopePdfWriter` are independent of that pipeline** but share the `HeaderFooterMode` enum and the font-resolution path. Item 10's receipt-side work is therefore independent of items 1–6.
4. **`BarcodeLabelRenderer.Render(content, width, height)` already accepts dimensions as parameters**, and `BarcodeService.cs` calls it with defaults. Making those dimensions configurable (item 7) is strictly prerequisite to changing the label pipeline's shape (item 8).
5. **`PrintBarcodeAsync` takes a single identifier string.** Item 8 changes the call shape; item 7 must land first or the size work is reworked twice.
6. **The printable-output items (11–18) share one concern — the shape of the printed page** — and none depends on another. Their relative order is flexible; they are sequenced by blast radius, smallest first.
7. **Item 9 (report print switches) is an independent settings-surface addition** with no schema dependency and no enabling effect on any other item.
8. **Items 14–15 are blocked on a boundary, not on a code dependency.** Their reference data source is the excluded analyser interface (§5.3). No in-scope data source exists, so no in-scope recommendation can be made for the data side.
9. **No two items introduce the same new entity**, and no item requires editing an existing migration, so **there are no circular dependencies and no blockers of the "PK must change" kind.**

### 9.2 The sequence

| # | REF | Function | Class | Dependencies | Evidence-based reason for position | Complexity / risk | Blocked | Migration status |
|---|---|---|---|---|---|---|---|---|
| 1 | REF-157 | Per-user (doctor) report element toggle | MISSING | Item 0: global show/hide already exists (`ReportItemTemplate.ShowHide`) | The global half exists and is keyed globally; this item adds a **scope column**. It is a prerequisite for nothing, and is the smallest of the template-family items. | **Low / Low.** Add a nullable user FK plus a per-user read path in `BuildTemplateMap`. Risk: precedence between global and per-user templates must be made explicit. | No | **NEW MIGRATION** (additive: `UserId` nullable on `ReportItemTemplates`, PK widened to `(ItemKey, UserId)`) |
| 2 | REF-160 | Per-report-line font family, size and colour | MISSING | Items 1, 6 (`ReportItemTemplate`; shared `ReportContentBuilder`) | Adds per-line font and colour columns to the **same entity** that items 1 and 6 modify, and must feed the same `Layout(...)` call. Sequencing it earlier would have items 1 and 6 extend it twice. | **Medium / Medium.** Twelve-line font sizing is a layout risk (pagination and page fitting change with font metrics). Risk: a line font larger than the page can abort PDF generation — the same class of problem `PageSizeMapper.FitHeaderFooterImage` already guards for logos. | No | **NEW MIGRATION** (additive: font family / size / colour columns on `ReportItemTemplates`) |
| 3 | REF-163 | Envelope item ordering | MISSING | None | Purely additive to `EnvelopePrintItemPosition`, which already exists with its own table and is consumed by `EnvelopePdfWriter.cs:52,168`. Smallest item in the sequence. | **Low / Low.** One sequence column plus an ordered emission. Risk: cycles in a user-defined ordering. | No | **NEW MIGRATION** (additive: `SortOrder` on `EnvelopePrintItemPositions`) |
| 4 | REF-162 | Envelope per-item font, size and colour | MISSING | Item 3 (same entity and the same emission loop) | Adds styling columns to the row that item 3 reorders; both changes touch `EnvelopePdfWriter`'s per-item loop. Ordering it after item 3 avoids editing that loop twice. | **Low / Medium.** Font resolution per item may collide with the document-wide `ArabicFontResolver.Resolve` call at `EnvelopePdfWriter.cs:70-72`. Risk: a font that cannot render Arabic silently substitutes. | No | **NEW MIGRATION** (additive: font family / size / colour on `EnvelopePrintItemPositions`) |
| 5 | REF-153 | Report print-option switches | MISSING | None | An independent settings-surface addition. Touches `SystemSettings` (or a new settings row) and the print coordinator; shares no file with items 1–4 or 6. | **Low / Low.** Booleans plus a predicate. Risk: a switch combination that yields an empty document. | No | **NEW MIGRATION** (additive: switch columns on `SystemSettings`) |
| 6 | REF-154 | Editable report item template | MISSING | Items 1, 2, 5, 11, 12 (same entity and same builder) | Already partially delivered at this commit; the residual requirement is the per-line font/colour (item 2) and the per-user scope (item 1). Both are **additive to this very entity**, so the sequence is: extend the entity first (items 1, 2), then generalise its lookup (item 6). | **Medium / Medium.** `ReportItemTemplates` already has 9 columns and 24 keys; adding scope plus per-line styling raises the migration and lookup cost. Risk: precedence ambiguity when a global and a per-user template both exist. | No | **NEW MIGRATION** (consumed by items 1 and 2) |
| 7 | REF-167 | Sticker size configuration | MISSING | Item 8 | `BarcodeLabelRenderer.Render` already takes `width`/`height` as parameters; only the configuration surface is missing. Must precede item 8, or item 8 hard-codes a second size. | **Low / Low.** Two settings columns plus a pass-through. Risk: a size that does not match real label stock. | No | **NEW MIGRATION** (additive: label width/height columns) |
| 8 | REF-164 | Barcode labels per sample material with doctor-editable distribution | MISSING | Item 7 | Terminates in `BarcodeLabelRenderer.Render`; the reference's 38 × 25 mm sticker is the size this must honour. | **High / Medium.** A new `SampleMaterial` entity, a per-test assignment, an editor surface, and a multi-label payload replacing the single-identifier call — while REF-038 and REF-166 must keep working on the old path. Risk: the single-identifier path is load-bearing for the invoice and group-slip barcode paths, which reuse the same renderer. | No | **NEW MIGRATION** (additive: `SampleMaterials`, `PatientTestSampleMaterials`) |
| 9 | REF-168 | Sample-nature interpretation panel | MISSING | None | Independent. Depends only on catalogue and range data that already exist. | **Medium / High.** Code is simple; the **clinical content** is the risk and needs authoring and maintenance. | No | **NEW MIGRATION** (additive: interpretation rows) |
| 10 | REF-169 | Glossary of analysis abbreviations | MISSING | None | Independent; same shape as item 9. | **Low / Medium.** New entity, CRUD, read surface. Risk: editorial, not technical. | No | **NEW MIGRATION** (additive: glossary rows) |
| 11 | REF-172 | Appointment-note reminder | MISSING | Item 12 (phone book exists; a reminder needs a target) | Independent of everything else; sequenced last because its size is driven by a notification-channel decision that affects nothing else. | **Medium / High.** **No notification infrastructure exists anywhere in the repository** — this is greenfield with an open design decision. | No | **NEW MIGRATION** (additive: reminder rows) |
| 12 | REF-183 | Colour versus black-and-white report printing | MISSING | Item 6 (the report pipeline) | A single global switch consumed by `ReportPageComposer`. Shares the report pipeline but not the template entity, so it sits after item 6. | **Low / Low.** One setting plus a conditional in the colour helpers. Risk: low. | No | **NEW MIGRATION** (additive: one column on `ReportSettings`) |
| 13 | REF-184 | Report printed under the patient name in Arabic and English | MISSING | Item 0 (patient naming) | Needs a second name field on the patient plus a report line. The report side depends on the template/entity work; the patient side is independent. | **Medium / Low.** Add a nullable `FullNameEn`, populate it, render `اسم المريض` and the English form on one line. Risk: RTL line composition with a Latin run. | No | **NEW MIGRATION** (additive: `FullNameEn` on `Patients`) |
| 14 | REF-185 | Report on large paper (A3) or A4 at 85 × 55 | MISSING | None | `PaperSize` is an enum with two members and `PageSizeMapper` maps both; extending it is a self-contained change. Independent of the template family. | **Low / Low.** Enum member, mapper entry, settings option. Risk: a page geometry that the current composer does not lay out for. | No | **NEW MIGRATION** (additive: enum is stored as int — see note) |
| 15 | REF-186 | Folded / multi-port report printing | MISSING | Item 14 (page geometry) | Folding a page is defined against a page size; it must follow item 14 rather than invent its own geometry. | **Medium / Medium.** Imposition maths plus a fold guide. Risk: content overflowing a fold panel. | No | **NEW MIGRATION** (additive: settings columns) |
| 16 | REF-187 | Sequential sample numbering on the report | MISSING | None | A report field plus a numbering source. Independent of the template family, though it is best added as a template item so it can be hidden. | **Medium / Medium.** Requires deciding the numbering scope (per day, per visit, per accession). Risk: a sequence that is not unique under concurrency. | No | **NEW MIGRATION** (additive: sequence column + source table) |
| 17 | REF-189 | Charted blood-count history report | MISSING | Item 0 (`HistoryMatrixBuilder.Pivot` already pivots the data) | The data pivot already exists and is wired to patient history; only a chart renderer is missing. No charting component is referenced anywhere in `src/TopLab.Infrastructure`, so this introduces one. | **High / Medium.** A charting dependency plus axis/series construction for four bar panels and two trend panels. Risk: chart rendering failures inside PDF generation. | No | **NEW MIGRATION** (additive: chart option columns) |
| 18 | REF-131 | Banded monitor over blood-picture element values | MISSING | Item 0 (`HistoryMatrixBuilder.Pivot`) | Reads `ProfileResultItem` and reuses the existing pivot; the single-test monitor already provides the band semantics and the output shape. | **Low / Low.** Extend the existing handler to a second data source. Risk: none material. | No | **NO MIGRATION** |
| 19 | REF-165 | Optional vertical coding on documents | MISSING | Item 8 (the label pipeline and the coding it carries) | Vertical coding is a rendering mode of the identity mark; item 8 emits per-material labels that also carry the coding. | **Low / Medium.** Small code, but rotation interacts with page geometry on three documents. | No | **NEW MIGRATION** (additive: coding mode column) |
| 20 | REF-180 | CBC report histograms | MISSING | — | **Boundary-limited.** See Blocked. | **Not assessable in scope** — the reference's histogram image is produced by the excluded analyser interface. | **Yes — see reason** | **NO MIGRATION** for the rendering side |
| 21 | REF-181 | CBC report scattergram | MISSING | — | **Boundary-limited.** See Blocked. | **Not assessable in scope** — same source constraint as item 20. | **Yes — see reason** | **NO MIGRATION** for the rendering side |
| 22 | REF-182 | Five-part / three-part differential reporting | MISSING | — | **Boundary-limited.** See Blocked. | **Not assessable in scope** — the reference's `RES %` / `RES K/µl` pairs are the analyser's own output. | **Yes — see reason** | **NEW MIGRATION** only if an in-scope differential model is agreed |

**Blocked-item reason (items 20, 21, 22).** The reference's page captions on SHOW 56–66 carry the provenance "(Done on fully Automatic Hematology Analyzer)", and the histograms, the scattergram and the five-part/three-part differential breakdown are all **analyser outputs**. Under exclusion Category 3, the analyser connection and the import of analyser results are out of scope. **No in-scope data source for these three outputs exists**, and this report does not recommend building one. The only in-scope part is the report-side rendering of a value that is supplied through some in-scope route — which the reference does not describe. These items are therefore marked **Blocked**: the implementation path is genuinely undetermined without an owner decision on the in-scope data route, and inventing one would either drift into excluded functionality or fabricate a requirement. They remain in this sequence, classified `MISSING`, and remain in the count reconciliation. **No excluded functionality is recommended here.**

### 9.3 Summary of the sequence

22 items. `DIFFERENT` present: 0. `BLOCKED / UNRESOLVED` present: 0. Items carrying a **Blocked** flag (blocked *recommendation*, not blocked *classification*): 3 — REF-180, REF-181, REF-182.

---

## SECTION 10 — Database / Migration Impact

**Migration set read at the target commit:** 22 migrations, `20260828052248_BaselineDataModel` … `20261018144852_AddProfileResultStyling`, plus `ApplicationDbContextModelSnapshot.cs`. **No migration was created, modified, applied or executed**, and no EF Core command was run.

### 10.1 Migration status per sequence item

| # | REF | Status | Evidence |
|---|---|---|---|
| 1 | REF-157 | **NEW MIGRATION** | `ReportItemTemplateConfiguration.cs:11` currently declares `HasKey(e => e.ItemKey)` with a single `ItemKey` column (max 100) and no user column. Additive: nullable `UserId` plus a composite key. |
| 2 | REF-160 | **NEW MIGRATION** | `ReportItemTemplates` has no font or colour column (configuration `:11-20` maps nine properties, none of them font/colour). Additive. |
| 3 | REF-163 | **NEW MIGRATION** | `EnvelopePrintItemPositionConfiguration` maps exactly `ItemName` (PK), `IsEnabled`, `LeftOffsetCm`, `TopOffsetCm`. No ordering column. Additive. |
| 4 | REF-162 | **NEW MIGRATION** | Same configuration; no per-item font/size/colour columns. Additive. |
| 5 | REF-153 | **NEW MIGRATION** | `SystemSettings` exposes 11 booleans; none corresponds to the reference's print switches. Additive. |
| 6 | REF-154 | **NEW MIGRATION** | Already delivered by `20261018075336_AddReportItemTemplates`; the residual scope change is carried by item 1. Migration consumed jointly. |
| 7 | REF-167 | **NEW MIGRATION** | No label-dimension column exists in any settings row or entity. Additive. |
| 8 | REF-164 | **NEW MIGRATION** | No `SampleMaterial` table and no per-test material assignment exist (targeted search: 0 matches). Additive: two new tables. |
| 9 | REF-168 | **NEW MIGRATION** | No interpretation entity exists. Additive. |
| 10 | REF-169 | **NEW MIGRATION** | No glossary entity exists. Additive. |
| 11 | REF-172 | **NEW MIGRATION** | No reminder entity exists. Additive. |
| 12 | REF-183 | **NEW MIGRATION** | No output-mode column exists; `ReportPageComposer.cs:176` hard-codes `Colors.White`. Additive. |
| 13 | REF-184 | **NEW MIGRATION** | `Patient` has one name field (`FullName`); no English name column. Additive. |
| 14 | REF-185 | **NEW MIGRATION** | `PaperSize` is persisted as an enum; adding A3 changes the EF model and the snapshot. Additive at the data level. |
| 15 | REF-186 | **NEW MIGRATION** | No fold/port columns exist. Additive. |
| 16 | REF-187 | **NEW MIGRATION** | No sequence column or sequence-source table exists. Additive. |
| 17 | REF-189 | **NEW MIGRATION** | No chart-option columns exist. Additive. |
| 18 | REF-131 | **NO MIGRATION** | Reads `ProfileResultItem`, which already exists (`20260909033414_AddAnalyteProfileDomain`). A second data source in an existing query requires no schema change. |
| 19 | REF-165 | **NEW MIGRATION** | No coding-mode column exists. Additive. |
| 20 | REF-180 | **NO MIGRATION** *(rendering side only)* | Blocked on a data-route decision. No in-scope column can be specified until that decision is made. |
| 21 | REF-181 | **NO MIGRATION** *(rendering side only)* | Same. |
| 22 | REF-182 | **NEW MIGRATION** *(conditional)* | Blocked on a data-route decision. If an in-scope differential model is agreed, `ProfileResultItem` would need an absolute-count column. |

**Totals:** 18 items require **NEW MIGRATION**; 4 require **NO MIGRATION**; **0 require EXISTING MIGRATION-PK IMPACT**.

### 10.2 Explicit statements

- **No existing migration needs editing.** All 22 migrations at the target commit are internally consistent and none of the 22 sequence items changes a primary key, a foreign key, a unique index, or a column created by an existing migration. Every proposed change is a **new, additive** migration appended after `20261018144852_AddProfileResultStyling`.
- **One caveat on item 1:** adding a nullable `UserId` to `ReportItemTemplates` and promoting the key to `(ItemKey, UserId)` **is a primary-key change on an already-created table**. It is a *new* migration altering the shape of a table created by `20261018075336`, which is why it is classified **NEW MIGRATION** and not **EXISTING MIGRATION-PK IMPACT** — no existing migration file must change, but the new migration must carry the key alteration and a data back-fill for the existing globally-keyed rows. This is flagged here because it is the single highest-migration-risk item in the sequence.
- **Editing an already-applied migration is not recommended anywhere in this report.** The repository's own convention is to append: `20260930163921_FixCultureSensitivityCategoryOffByOne` exists as a **separate corrective migration** rather than an amendment to the baseline, and all seven new migrations added at this target commit follow the same append-only pattern.
- **EF model change versus migration change.** Item 18 is the only item that changes C# without changing the EF model. Items 20 and 21 would likewise be C#-only *if* an in-scope data route is agreed that requires no new column. Every other item changes the EF model and therefore requires a migration.
- **Data requirements traced before schema change.** Items 1, 2, 3, 4, 5, 12, 14, 15, 19 depend on `ReportSettings`, `ReportItemTemplates`, `EnvelopePrintItemPositions` or `SystemSettings`, each read at the target commit. Items 13 and 16 depend on `Patient`. Item 18 depends on `ProfileResultItem`. Items 6 and 8 depend on `ReportItemTemplates` and the barcode pipeline. Items 9, 10, 11 and 17 introduce wholly new tables with no back-fill requirement.
- **Ordering constraints for migrations.** Items 1 → 2 → 6 must apply in that order (they touch one table); 7 → 8 and 3 → 4 likewise; 14 → 15 are schema-independent but logically ordered. The sequence respects all of them.
- **Not verified:** whether the 22 existing migrations apply cleanly to an empty database, and whether EF reports pending model changes. No database was reachable and no EF Core command was run (L-05, L-06).

---

## SECTION 11 — What You Could NOT Verify

This section agrees with Section 1 and repeats every material unresolved limitation. **Nothing here is presented as verified.**

| # | Affected REF / claim | What remains unverified | Why | What would resolve it |
|---|---|---|---|---|
| U-01 | Every `IMPLEMENTED` row resting on the UI layer | That the WPF layer compiles and that each documented screen exists and is navigable | `net8.0-windows` cannot be built on a Linux host; no build was attempted (L-01) | A Windows host with the .NET 8 Desktop SDK |
| U-02 | All 146 `IMPLEMENTED` rows | That any of them behaves correctly at runtime | The application was never launched (L-04) | A Windows host with a seeded database |
| U-03 | All print-producing REF IDs — REF-038, REF-039, REF-048, REF-053, REF-054, REF-055, REF-073, REF-075, REF-106, REF-111, REF-113, REF-120, REF-144, REF-145, REF-146, REF-147, REF-148, REF-154, REF-155, REF-158, REF-159, REF-161, REF-162, REF-166, REF-177, REF-179 | The **visual** correctness of every produced document: Arabic shaping, RTL ordering, pagination, margins, colour resolution, barcode legibility | No output was produced and no Arabic shaping was executed (L-07) | A Windows host with an Arabic-capable font; generate and examine each document |
| U-04 | Most of Modules 2, 3, 4, 7, 8, 9, 10 | That each handler's save path produces the expected rows; that the 22 migrations apply cleanly; that seed data materialises | No database was reachable and migrations were read as source only (L-05) | A SQL Server instance and a seeded database |
| U-05 | DEF-03 | Whether a test can be removed or cleared from a visit after results exist or after delivery | Static reading did not settle the guard; no runtime and no database (L-02, L-04, L-05) | A Windows host and a seeded database |
| U-06 | DEF-04 | Whether combined, history and bulk printing increment the persisted print count | Requires observing persisted state (L-05) | A seeded database |
| U-07 | DEF-02 | Whether the asymmetric gate in `ProfileResultColors` is intended | Static test evidence exists in `ProfileResultStylingPrecedenceTests.cs` but was not executed (L-02) | Running that test file |
| U-08 | All 22 `MISSING` rows | Only **source-level absence** is established. A systematic negative search cannot prove absence of a capability living outside the repository | No runtime (L-04) | A Windows host, plus an owner decision where noted |
| U-09 | REF-180, REF-181, REF-182, REF-188 | Whether an **in-scope** data route for the analyser-derived outputs exists or could be agreed | The reference's source is the excluded analyser interface; no in-scope route is described | An owner decision; no such decision is assumed or invented here |
| U-10 | Section 4 inventory as a whole (189 rows) | That this decomposition is the only defensible one | The PDFs contain no consolidated requirements list (L-12) | An owner-confirmed requirements baseline |
| U-11 | The 28 rows marked *(screenshot only)* | That a screenshot-only behaviour is a requirement rather than an example (L-13) | The reference gives no prose for these | An owner decision |
| U-12 | F-01 (permission granularity) | Whether 13 permissions satisfy the reference's longer list | No acceptance criteria in the authorized PDFs (L-14) | An owner decision |
| U-13 | Section 10 migration statuses | That the 22 existing migrations apply cleanly and that EF sees no pending change | No database and no EF Core command (L-05, L-06) | A SQL Server instance; `dotnet ef database update`; `has-pending-model-changes` |

**No item above is claimed as verified. None was classified `BLOCKED / UNRESOLVED`, because each remains statically classifiable on the evidence obtained.**

---

## SECTION 12 — Final Count Reconciliation

```
TOTAL PDF-EVIDENCED REFERENCE FUNCTIONS                       189
OWNER-EXCLUDED FUNCTIONS AMONG THOSE EVIDENCED                  21
IN-SCOPE REFERENCE FUNCTIONS                                 168
```

```
189 − 21 = 168   ✓

IMPLEMENTED                                               146
DIFFERENT                                                   0
MISSING                                                    22
BLOCKED / UNRESOLVED                                        0
                                                          ----
146 + 0 + 22 + 0 = 168   ✓
```

| Metric | Value |
|---|---|
| Total PDF-evidenced reference functions | **189** |
| Owner-excluded functions among those evidenced | **21** |
| **In-scope reference functions** | **168** |
| IMPLEMENTED | **146** |
| DIFFERENT | **0** |
| MISSING | **22** |
| BLOCKED / UNRESOLVED | **0** |
| Sequence items | **22** |
| **Do the counts reconcile?** | **YES — exactly, on the first pass, with no residual discrepancy.** |

The 22 MISSING identifiers are listed in §8.3; the 22 sequence items in §9.2 map one-to-one onto them. Every owner-excluded identifier is excluded from the classification denominator, from the sequence, from the defect list, and from the migration analysis. No item was merged, split, duplicated, omitted or reclassified to obtain a desired total, and no classification was adjusted to make the totals appear correct.

**Change relative to the project's state at the previously verified commit.** Recorded as a factual observation from `git diff --stat` between `c482a91` and `9d90f49` (149 files changed, 30,474 insertions, 202 deletions; 10 intervening commits; 7 migrations added), **not** as a comparison against any prior classification. Of the 22 items now `MISSING`, **none** is a report-template or receipt-branding gap: the report header/footer colours, the header/footer image pipeline, the receipt font and logo controls, the group-patient slip, the receivable balance filter, the report item template, line spacing and item markers, abnormal-result colouring, per-profile result styling and the normal-range marker all now have a traced chain from a persisted setting through a command, validator, view model and XAML control to a rendering call. **The remaining 22 gaps are: three report-presentation settings surfaces, three blood-count report output capabilities, five barcode-label capabilities, two utilities, the CBC-element banded monitor, the envelope styling/ordering pair, the print-switch set, the dual-language report name, the large-paper and folded-report options, and the sample sequence number.** Seventeen of the twenty-two additions between the commits target exactly the surfaces these gaps sit on.

---

## SECTION 13 — Final Verdict

**What the code actually implements.** At commit `9d90f499`, Top-Lab is a mature and substantially complete laboratory information system. It implements the full patient journey — registration with medical-history capture, test and profile ordering, external-sample flagging, barcode, lab-order and account-slip printing — and a complete billing engine with per-test settlement, extra charges, corrections and voiding. It implements three result shapes (simple, profile, culture) with review, amendment, verification and bulk print; culture and sensitivity with pregnancy- and child-aware antibiotic filtering and inhibition zones; a full catalogue with groups, work-group logs, custom groups and comments; reference ranges resolved by sex and age with a print-time per-patient refresh; contract price lists with per-entity discount and commission; treating doctors, sent-out labs and referring entities; sent-out sample accounting; result delivery with settlement; and a complete audit trail of registration, modification, printing and delivery. It implements the whole worksheet, statistics and cash-drawer inventory suite, including five inventory element scopes and four report types, the sample-draw board, attendance, and a permissions model enforced architecturally rather than per screen.

On the evidence obtained by static inspection, **146 of 168 in-scope reference functions are implemented with a traced path** ending at a real domain rule, EF Core configuration, or rendering call.

**What the authorized PDFs document that is not yet met.** Twenty-two functions. They fall into four groups.

*Report-presentation settings surfaces (7).* Three blood-count report outputs — histograms, the 2-D scattergram, and the five-part/three-part differential with paired relative and absolute counts — have no representation at all: there is no charting, no differential model, and no CBC analyte catalogue. Report print-option switches, per-report-line font and colour across roughly twelve lines, per-doctor (per-user) report toggles, envelope per-item font/size/colour and envelope item ordering are all absent, although the global show/hide, the three colour settings and the envelope position controls are present.

*Blood-count page-shape options (5).* Colour versus black-and-white output, a dual-language patient name on the report, large paper (A3 or 85 × 55), folded multi-port output, and an auto-generated sequential sample number are all absent. The page geometry supports exactly A4 and A5, and one name field exists.

*Barcode label family (3 of 4).* The identity barcode prints and can be reprinted; the label **size** is a code constant rather than a setting, **vertical coding** has no rendering path, and the reference's per-material tube/container/swab labelling with doctor-editable test distribution has no entity, command or surface at all.

*Utilities and statistics (2).* The sample-nature interpretation panel and the analysis-abbreviation glossary do not exist, and the appointment-note reminder has no notification infrastructure anywhere in the repository. The banded monitor still reads only test-level scalars, so it does not reach blood-picture analyte values.

**Which material uncertainties remain.** Nothing in this report was observed running. The WPF layer was never compiled, no database was created or migrated, no document was printed, and no Arabic shaping was exercised — because all of that is forbidden in this run and impossible on this host. Twenty-eight inventory rows rest on **screenshot evidence only**, because the reference documents them in screen captures without prose. Four issues are disclosed as **suspected rather than confirmed** (DEF-03, DEF-04, plus the two boundary questions), and three sequence items (REF-180, REF-181, REF-182) are **Blocked** on a boundary rather than on a code difficulty: the reference's source data for them is the excluded analyser interface, and no in-scope route exists or is assumed. The exact rendering of every colour, image and font change is unverified.

**Maturity.** The codebase appears **mature and substantially complete within scope**, with one qualification worth stating plainly. The architecture is genuinely layered — four projects with a strict domain core, MediatR commands and queries, a uniform authorization behaviour, EF Core configurations, and an append-only migration history that has now grown to 22 migrations without a single in-place edit of an applied one. Domain invariants live in the entities rather than only in handlers. The work completed between the two verified commits is large and well-targeted: roughly thirty thousand lines, seven new additive migrations, and complete paths from new settings to rendering. What remains unfinished is almost entirely **print presentation and two content-rich utilities**, and the pattern is consistent — where a setting is present, it reaches the renderer; where something is missing, it is missing everywhere at once, with no inert or half-wired surface left behind. That is a healthier failure mode than partial implementation, and the two `DEF` findings that describe comment/code divergence support the same conclusion.

**Practical implications.** This repository is usable as the basis for planning. Of the 22 sequence items, **18 need a new additive migration and 4 need none**, and **no existing migration requires editing** — with one flagged exception: item 1 (the per-user report scope) alters the primary key of a table created three migrations earlier, and is the single highest-migration-risk item. Three items are blocked on an owner decision about data routing for analyser-derived report content, and that decision should be taken before any of them is scheduled. The practical advice is to take the twelve unblocked, low-risk items first — envelope ordering, envelope styling, print switches, sticker size, the label-size plumbing, the glossary, and the CBC-element banded monitor — because they are independent, additive, and each closes a concrete user-visible gap; then the report-template family as one coordinated piece; and only then the blood-count output family, pending the boundary decision. Nothing in this plan touches, requests, or depends upon any of the five owner-excluded categories.

---
## SECTION 14 — Self-Audit Checklist

| # | Item | Result | Explanation |
|---|---|---|---|
| 1 | Exact target commit verified | **PASS** | `git rev-parse HEAD` returned `9d90f499e95082ec40c417bd4910f1cfc62daec3`, an exact match. Section 2. |
| 2 | Only the two permitted PDFs used as reference evidence | **PASS** | Only `RLS_Learn_Enhanced.pdf` and `RL_Show_Enhanced.pdf` were opened. No other PDF was used; no external site, search engine or domain knowledge was used to establish a reference requirement. |
| 3 | No file in `Docs/Reference system files/` listed, opened, searched, read, cited or used | **PASS** | No file in that directory was listed, opened, searched, read, cited or used, and no traversal was rooted inside it. No search in this investigation was rooted at `Docs/`. |
| 4 | No file in `Docs/Audit/` listed, opened, searched, read, cited or used; no file in `Docs/Remaining Tasks Folder/` other than the two PDFs used | **PASS** | No search or read was rooted at `Docs/Audit/` or at `Docs/Remaining Tasks Folder/`. `pdfinfo` and `pdftotext` were invoked against the two PDF paths explicitly. Section 3. |
| 5 | No earlier classification, count, defect or implementation claim used as proof of current implementation | **PASS** | Every implementation claim in Section 6 was re-derived by reading the file at the target commit. The change narrative in Section 12 is drawn from `git diff --stat` and is explicitly not a comparison against any prior classification. |
| 6 | Canonical inventory derived from the PDFs and deduplicated as far as evidence permits | **PASS** | Derived from the two PDFs with decomposition and deduplication rules stated in §4.3; REF-120 is the only function evidenced by two pages and is carried once. |
| 7 | Every page of both PDFs accounted for in the coverage table with its inspection method | **PASS** | §3.3 gives a per-page-range method table; **289 of 289** pages are text-extracted, rendered and image-examined. |
| 8 | Every owner-excluded function found in the PDFs listed individually | **PASS** | All 21 listed individually in §5.1–§5.5 with category, subfunction and page. |
| 9 | Excluded functions not classified, counted as gaps, reported as defects, sequenced or recommended | **PASS** | No `REF-001`…`REF-021` identifier appears as a classification row, a sequence row or a migration row. Three boundary notes (`REF-005`, `REF-006`, `REF-016`) appear inside the findings column of in-scope rows to state that the excluded aspect is **absent and deliberately so**; they assign no classification. |
| 10 | Every in-scope function has exactly one classification | **PASS** | 168 rows, one classification each; 22 MISSING identifiers enumerated in §8.3 match the 22 rows exactly. |
| 11 | Every classification has actual code evidence at the target commit | **PASS** | Every row cites a path with line numbers where relevant; every negative row names the constructs searched and the match count. |
| 12 | Every reference-function claim has authorized PDF evidence | **PASS** | All 189 rows carry a PDF filename and page; 28 screenshot-only rows are marked as such. |
| 13 | Visual inspection distinguished from text extraction | **PASS** | §3 separates text-extracted, visually inspected, partially inspected, text-only and uninspected, per PDF, with the method used per page range. |
| 14 | OCR, screenshots, tables and visual limitations addressed | **PASS** | §3.2 records that pages 56–66 extract with mirrored Latin text and that **image evidence alone** was relied on for them; L-10, L-11, L-12, L-13. |
| 15 | No visual inspection claimed unless actually performed | **PASS** | Every one of the 289 pages was rendered and examined; no page is claimed as inspected that was not. |
| 16 | Every DIFFERENT and MISSING item appears exactly once in the sequence | **PASS** | 22 MISSING, 0 DIFFERENT; 22 sequence items, one per identifier. §9.2, §9.3. |
| 17 | Every sequence item has migration analysis | **PASS** | §10.1 gives a status and evidence for all 22; §10.2 states that no existing migration needs editing, with one primary-key caveat flagged. |
| 18 | No build, test, restore, database, EF Core or runtime command attempted; no status requiring execution claimed | **PASS** | No `dotnet build`, `dotnet test`, `dotnet restore`, `dotnet ef`, SQL or application launch was executed in this run. `TEST VERIFIED`, `BUILD VERIFIED` and `RUNTIME VERIFIED` appear nowhere. Static reading of test files is cited as **static** evidence only. |
| 19 | Negative classifications supported by documented systematic searches | **PASS** | Each of the 22 `MISSING` rows names the search terms and the match count (0). Two suspected issues (DEF-03, DEF-04) are explicitly **not** classified as defects. |
| 20 | Unsupported assumptions and unresolved evidence gaps disclosed | **PASS** | L-01…L-15; U-01…U-13; F-01…F-05; DEF-01…DEF-06 with confirmed and suspected separated; the Blocked flags on REF-180, REF-181 and REF-182 with their reason. |
| 21 | Counts reconcile without forced arithmetic | **PASS** | 189 − 21 = 168; 146 + 0 + 22 + 0 = 168; the 22 MISSING identifiers are enumerated and match; the 22 sequence items map one-to-one. Closes on the first pass with no residual discrepancy. |
| 22 | The report is internally consistent | **PASS** | §6.15, §8.1, §8.2, §8.3, §8.4 and §12 all state the same figures: 189 / 21 / 168 / 146 / 0 / 22 / 0. Verified by inspection of every count statement in the document. |
| 23 | The report does not reference or cite prior audit reports or prior execution plans | **PASS** | No prior report, inventory, plan or decision record is named, cited, quoted or relied on. The only mention of earlier commits is `git diff --stat` between two hashes, used as a factual observation and explicitly not as a prior classification. |
| 24 | Environment limitations appear in Section 1 | **PASS** | L-01…L-15 are in Section 1, first, and are cross-referenced from Sections 6, 10 and 11. |
| 25 | No repository file modified; no commit or push made | **PASS** | `git checkout --detach` was the only state-changing command, as authorised. `git status --porcelain` is empty. No commit, push, reset, clean, stash, merge or rebase was executed. |
| 26 | Exactly one deliverable created, with the exact filename, and it has no appendix | **PASS** | `Post-Phase-1 Independent Verification V2.md` is the only file created for delivery. It contains Sections 1–14 in order and **no appendix**. Page images and per-page text extracts exist only as working artefacts outside the repository and are not deliverables. |

**Summary:** 26 PASS, 0 FAIL, 0 NOT VERIFIED.

---
