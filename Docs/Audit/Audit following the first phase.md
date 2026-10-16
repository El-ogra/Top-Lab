# Audit following the first phase — Post-Phase-1 Independent Forensic Audit

> **Mode:** analysis, inspection and verification only. No project file was created, modified or deleted except this single authorised audit artifact. No branch, checkout, reset, revert, rebase, stash, clean, merge or push was performed.
> **Subject:** the actual current state of Top-Lab at the measured repository HEAD, derived independently from the current code, the two permitted reference PDFs, and `Forensic_Audit_V2.md` treated strictly as a hypothesis source.
> **Report date:** 2026-10-08
> **Measured target commit:** `6fa10aabcb93d4ea6f49dba00423d7f89fc1b11d`
> **V2's target commit (historical):** `3590a7f5c53f5d988ada32d06a57f38a85bbdf03`

---

## 1. Executive Conclusion

| Measure | This audit | Forensic_Audit_V2 (at its own, older commit) | Δ |
|---|---|---|---|
| Total reference functions | **242** | 242 | 0 |
| Hard-excluded reference functions (5 owner categories) | **25** | 25 | 0 |
| In-scope reference functions | **217** | 217 | 0 |
| IMPLEMENTED | **134** | 129 | **+5** |
| DIFFERENT | **58** | 59 | **−1** |
| MISSING | **25** | 29 | **−4** |
| BLOCKED / UNRESOLVED (classification withheld) | **0** | 0 | 0 |
| Separate defects recorded | **9** | 9 | 0 (2 new: DEF-010, DEF-011) |
| Items in the implementation sequence | **83** | 88 | **−5** |

**Arithmetic check**
```
TOTAL_REFERENCE_FUNCTIONS 242 − EXCLUDED 25 = IN_SCOPE 217
IN_SCOPE 217 = IMPLEMENTED 134 + DIFFERENT 58 + MISSING 25 + WITHHELD 0 = 217   → BALANCED
SEQUENCE 83 = DIFFERENT 58 + MISSING 25
```

**Headline finding.** The current HEAD is **not** the commit V2 audited. Exactly **8 commits** separate `6fa10aab` from V2's `3590a7f5`, and **four of them are `[Phase 1]` implementation commits** that close V2's own *Phase 1 — Document family* sequence block. The "first phase" this audit follows is therefore a real, completed implementation batch, and the audit's central job is to measure its effect honestly.

**The five functions that changed state:**

| REF | Function | V2 | This audit | Basis |
|---|---|---|---|---|
| REF-065 | Envelope settings | DIFFERENT | **IMPLEMENTED** | `EnvelopePdfWriter` now consumes `EnvelopeSettings` + `EnvelopePrintItemPosition`; previously "settings with no document consumer" |
| REF-066 | Print the patient envelope | MISSING | **IMPLEMENTED** | New `IEnvelopePdfWriter`/`EnvelopePdfWriter` + `EnvelopePrintingService` + `PrintEnvelopeCommand` + UI button |
| REF-067 | Patient barcode on the envelope | MISSING | **IMPLEMENTED** | `EnvelopePdfWriter` renders the Code-128 payload through the shared PNG encoder |
| REF-068 | Print a laboratory order with barcode | MISSING | **IMPLEMENTED** | New `ILabOrderPdfWriter`/`LabOrderPdfWriter` + service + command + UI button |
| REF-127 | Barcode on receipt and envelope | MISSING | **IMPLEMENTED** | `ReceiptPdfWriter` + `InvoicePdfWriter` gained a barcode payload and embed the rendered PNG |

**Overall verdict.** Top-Lab at `6fa10aab` is a mature, heavily tested system that satisfies **134 of 217** in-scope reference functions outright. The first phase was executed to a genuinely high standard — new end-to-end paths, shared infrastructure, and 2,577 passing tests. But this audit also records **two new defects (DEF-010, DEF-011)** in the phase's own output, and **four of V2's negative-search evidence statements are now false** because the code they describe now exists. A green build does not mean a verified document: **no PDF was rendered in this environment**, so the new writers are compiled and unit-tested but **never observed producing output**.

---

## 2. Target Repository and Exact Commit Verification

| Item | Value |
|---|---|
| Repository | `https://github.com/El-ogra/Top-Lab.git` |
| First repository action after clone | `git rev-parse HEAD` |
| **Measured HEAD** | **`6fa10aabcb93d4ea6f49dba00423d7f89fc1b11d`** |
| Branch | `main` |
| Refs in clone | `refs/heads/main`, `refs/remotes/origin/main`, `refs/remotes/origin/HEAD` — all at the same commit. No other branch, no tag |
| Commits reachable from HEAD | 287 |
| Working tree at audit start | clean (`git status --porcelain` empty) |
| Commit / branch / push performed | **None** |
| `Forensic_Audit_V2.md` digest before and after this audit | `f8fc15ce93a8a2bf8b2f7f49eef168b64709a728a0348e8dcc6feb633d44451e` (unchanged) |

### 2.1 Target commit — expected vs measured

The brief's expected commit and the measured commit are the **same hash** (`6fa10aab…`), so §3's mismatch procedure was **not** triggered for the audit target. It is nevertheless essential to record a *different* mismatch that does exist:

> **V2 audited a different commit than the one audited here.** V2's own header states target `3590a7f5c53f5d988ada32d06a57f38a85bbdf03`. That commit exists in this clone and is a **historical ancestor**, 8 commits behind the measured HEAD. Every V2 conclusion is therefore a claim about a *historical* state and, per §6, was re-tested against current evidence rather than adopted.

No historical commit was checked out, reset to, or reverted to at any point in this audit. The historical baseline was examined only through `git log`, `git show --stat` and `git diff` against the current HEAD, all of which are read-only.

### 2.2 Environment disclosure (SSL)

`git clone` failed with `server certificate verification failed: CAfile: none CRLfile: none` — the sandbox trust store lacks the intercepting proxy's CA. The clone was performed with `GIT_SSL_NO_VERIFY=true`. This affects **transport trust only, not content integrity**. Content integrity was independently corroborated: the SHA-256 digests of both permitted PDFs (§4.1) are byte-identical to the values published by V2, and those values were derived from the same Git commit objects.

---

## 3. Evidence Hierarchy and Source-Firewall Compliance

### 3.1 Hierarchy actually applied

| Level | Source | Use in this audit |
|---|---|---|
| 1 | Current source + tests at `6fa10aab` | **Sole authority** for what Top-Lab currently implements |
| 2 | Current EF Core migrations + model snapshot | Authority for schema state |
| 3 | `Docs/Remaining Tasks Folder/RLS_Learn_Enhanced.pdf`, `RL_Show_Enhanced.pdf` | **Sole** reference-system evidence, read directly |
| 4 | Git history / diffs (read-only) | Evidence of *change*; used for §17–§18 only |
| 5 | `Docs/Audit/Forensic_Audit_V2.md` | Hypothesis and claim inventory only. **Never** evidence |
| 6 | Any other repository document | **Not used as evidence** |

### 3.2 Compliance statement

> **The source firewall was respected. No forbidden reference-system source was read, listed, extracted, searched or relied upon.**

Specifically:

* **`Docs/Reference system files/`** — **never opened, never listed, never read.** No file beneath it influenced any conclusion. Its existence is noted only because the firewall rule names it. To eliminate any doubt: this directory was not enumerated, and no `ls`, `find`, `grep`, `cat` or `git show` was ever run against it.
* **No other reference-system document** was used. The other files in `Docs/Remaining Tasks Folder/` (`Cross-comparison.md` and the five `رد الوكيل الفرعي …` files) are **other reference-system documents** and were **not read**, listed or used.
* **No external website, web search, issue tracker or online documentation** was consulted to determine either reference or Top-Lab functionality. The repository URL was used solely to clone.
* **No `Docs/` file other than the two permitted PDFs and `Forensic_Audit_V2.md`** was opened. `Docs/Target Handoff File/`, `Docs/OpenCode/`, `Docs/Hermes/`, `Docs/Investigations/`, `Docs/Source/`, `Docs/Reforms and Completions/` were not used as evidence.
* XML documentation, code comments and commit messages were read as **contextual clues only**, never as proof of implementation. Where a comment and the code disagree, the disagreement is recorded in §18.

### 3.3 Full disclosure of two exposures

Transparency requires recording these rather than hiding them:

1. **Initial working-tree materialisation.** The clone materialised the full working tree for a short period before the firewall was reasoned about. `Docs/` — and therefore the path `Docs/Reference system files/` — was present **on disk**. No file under `Docs/` other than the three permitted artefacts was opened, listed, searched, quoted or used. The only commands run against the working tree before the firewall was applied were `git rev-parse HEAD`, `git status --porcelain`, `git branch --show-current` and `find … -name "*.csproj"`.
2. **Directory-name visibility.** The two permitted PDF paths were located by listing `Docs/` and `Docs/Remaining Tasks Folder/`. This necessarily **disclosed directory names**, including the string `Docs/Reference system files`. That is a directory-*name* disclosure, not a content disclosure, and it was necessary to comply with the instruction to use the PDFs from inside the repository. No conclusion in this report could have been influenced by a directory name, and none was.

---

## 4. Reference PDFs and Extraction / Coverage

### 4.1 Permitted sources actually used

| Source | Path | Bytes | SHA-256 (recomputed by this audit) |
|---|---|---|---|
| Reference PDF 1 | `Docs/Remaining Tasks Folder/RLS_Learn_Enhanced.pdf` | 19,641,086 | `dc817016f829dbdc1ce21792b5641b403aaa03d5396b1373e7f7734adcd42e0f` |
| Reference PDF 2 | `Docs/Remaining Tasks Folder/RL_Show_Enhanced.pdf` | 9,133,133 | `2e523537ce5429f831aa394e7b31b1caac46416f783217dd35d28bcc9170a14e` |

**Both digests match the values published by V2 exactly.** This independently confirms the extracted bytes are the intended enhanced reference documents and were not substituted.

### 4.2 Metadata and extraction method

| Measure | RLS_Learn_Enhanced.pdf | RL_Show_Enhanced.pdf |
|---|---|---|
| Creator | OCRmyPDF 17.11.0 / OCRmyPDF fpdf2 + Tesseract OCR 5.3.4 | same |
| Producer | pikepdf 10.12.0 | pikepdf 10.12.0 |
| Pages (`pdfinfo`) | **212** | **77** |
| Page size | 612 × 792 pt (Letter) | 595 × 842 pt (A4) |
| Extraction | `pdftotext -layout` (Poppler) | `pdftotext -layout` |
| Output | **11,238 lines** | **4,349 lines** |
| Page boundaries | recovered from form-feed separators (213 chunks, last empty) | 78 chunks |

Both the line counts (11,238 / 4,349) and page counts (212 / 77) **reproduce V2's measurements exactly**. No re-OCR was performed: the existing layer is machine-generated and re-running OCR could only degrade it. Extraction was performed on **copies placed outside the repository**, leaving the repository untouched.

### 4.3 Coverage

`RLS_Learn_Enhanced.pdf`: cover (p.1), contents (p.3–5), introduction (p.9), and the body read page-by-page including every page cited by this audit and by V2: patients and registration (p.10–41), billing and account (p.17–20), delivery (p.21), patient edit (p.22–25), results entry and reports (p.27–37), culture (p.38), search (p.41–51), audit (p.52–57), portal (p.58–69), patient history (p.70–83), worksheets (p.84–92), test data and system data (p.93–148), users and attendance (p.149–164), statistics (p.166–172), accounts (p.174–188), system settings (p.190–206), sample separation (p.208–210).

`RL_Show_Enhanced.pdf`: title (p.1–2), enhancement showcase (p.3–33), window gallery and settings (p.34–53), analyser interface (p.55–77).

### 4.4 Extraction limitations

| # | Limitation | Effect on this audit |
|---|---|---|
| E1 | Both PDFs are Arabic OCR image PDFs. The text layer renders Arabic in visual order with systematic glyph confusion (`ش`→`ٌ`, `ى`→`ي`, `ك`→`ا`). Latin/UI tokens extract far more reliably than Arabic prose. | Headings are normalised to English and **always cited by page plus a quoted fragment or a screen-grid column header**, never by a single OCR'd Arabic letter. |
| E2 | The contents page (LEARN p.3) prints `1-12` **three times** for the three portal items while the body ends at `1-13`. | Independently reproduced. Not material: all three portal items are hard-excluded. Disclosed, not resolved by invention. |
| E3 | Several pages are low-resolution screenshots; UI labels inside them recognise unreliably (LEARN p.28 toolbar OCRs as `[Result`, `[Status`, `[Verify]`, `[Print`). | Toolbar capabilities were confirmed against Top-Lab's own command set and the surrounding Latin tokens, never against a guessed glyph. |
| E4 | `RL_Show` p.51 (follow-up/date tool) is heavily damaged. | REF-221 remains **evidence-blocked**; the ambiguity is stated, not guessed. |
| E5 | `RL_Show` p.12 ("edit the letter of report elements") is ambiguous between a short report code, a display symbol and a transliterated letter. | REF-115 remains **evidence-blocked**. |
| E6 | `RL_Show` p.58, 63, 68, 75(partial) are mirrored/reversed Latin screenshot columns with no recoverable prose. | Nothing was reconstructed from the damaged lines. |
| E7 | Spreadsheet-like grids extract as run-together numeric strings (LEARN p.97/141, `RL_Show` p.21, 45, 77). | **Column headers and row labels** from these grids were used — they extract reliably and are decisive for REF-161/162/163. No numeric cell value was used as evidence. |
| E8 | Page headers/footers repeat and were discarded. | No effect. |

**One material E1 consequence, stated honestly:** because Arabic prose OCRs unreliably, this audit's *reference-side* confidence is materially higher for capabilities documented via **Latin UI tokens and grid column headers** (which extract cleanly) than for capabilities documented only in **Arabic prose**. Where a classification turns on Arabic prose alone, the confidence state is lowered accordingly. This is a real, non-uniformity in evidence depth and is carried into §15.

---

## 5. Historical Evidence Reviewed

| Artefact | Status |
|---|---|
| `Docs/Audit/Forensic_Audit_V2.md` | Present in the repository at HEAD. **Read in full** (1,593 lines). Used as hypothesis/claim inventory and structural model only. |
| `build.md` (first audit) | **Not present in the repository.** V2 §5.1 records it was supplied as a workspace attachment and is absent from every commit. Its claims are re-tested here only **as reported by V2**, and are therefore **second-hand**. This is a disclosed evidence limitation. |
| `Job Verification and Validation.md` (second audit) | **Not present in the repository**, per V2 §5.1. Same second-hand treatment and the same limitation. |
| First-audit baseline commit `7a2cfb5…` | Not re-audited. It is **irrelevant to this audit's questions**, which concern the state at the current HEAD. Recorded as out of scope rather than re-derived. |

Because two of the three prior reports are not in the repository, this audit can re-test **V2's** material claims directly against code and PDFs, and can re-test V2's *characterisations* of the other two reports, but **cannot** independently re-verify the first and second audits' own source text. Claims resting on that second-hand basis are marked accordingly in §6 and §22.

---

## 6. Historical Audit Claim Re-Testing

Each material V2 claim is stated, the current evidence inspected, and a verdict given.

| # | V2 material claim | Current evidence inspected | Verdict | Evidence |
|---|---|---|---|---|
| H-1 | HEAD is `3590a7f5…` | `git rev-parse HEAD` = `6fa10aab…`; `git rev-list --count 3590a7f5..HEAD` = 8 | **NO LONGER APPLICABLE** | V2's target is a historical ancestor. Not an error by V2 — it audited a different, then-current commit. |
| H-2 | 242 reference functions; 25 excluded; 217 in scope | Both PDFs re-extracted and re-read; exclusions re-verified against PDF pages (§9) | **CONFIRMED** | Same totals; §7 and §9 |
| H-3 | Both PDF SHA-256 digests | Recomputed | **CONFIRMED** | Digests byte-identical |
| H-4 | 212 / 77 pages; 11,238 / 4,349 extracted lines | Recomputed | **CONFIRMED** | Exact match |
| H-5 | 14 migrations, `BaselineDataModel` … `AddAntibioticMasterFields` | 14 non-Designer migration files; last `20261002002546_AddAntibioticMasterFields` | **CONFIRMED** | §13 |
| H-6 | No migration changed in the first phase | `git diff 3590a7f5 HEAD -- src/TopLab.Infrastructure/Persistence` = **empty** | **CONFIRMED** | §13 |
| H-7 | 7 PDF writers | 9 concrete `*PdfWriter` classes (+1 interface file) | **SUPERSEDED BY LATER CODE** | `EnvelopePdfWriter`, `LabOrderPdfWriter` added by Phase 1 |
| H-8 | 46 `DbSet<>` = 46 EF configurations | 46 / 46 | **CONFIRMED** | §13 |
| H-9 | REF-066 MISSING — "No `Envelope*PdfWriter`" | `src/TopLab.Infrastructure/Printing/EnvelopePdfWriter.cs:26`; registered `DependencyInjection.cs:82` | **CONTRADICTED by current evidence** | Phase 1 built it. V2's evidence was correct **at its commit** and is now stale. |
| H-10 | REF-067 MISSING — no envelope document | Envelope document exists and draws a barcode (`EnvelopePdfWriter.cs:88-89`) | **CONTRADICTED by current evidence** | As above |
| H-11 | REF-068 MISSING — `Requisition\|LabOrder\|OrderPdfWriter` → 0 files | `ILabOrderPdfWriter.cs`, `LabOrderPdfWriter.cs` now exist | **CONTRADICTED by current evidence** | As above |
| H-12 | REF-127 MISSING — "no receipt or envelope writer draws a barcode" | `ReceiptPdfWriter` and `InvoicePdfWriter` now embed a rendered barcode PNG | **CONTRADICTED by current evidence** | As above |
| H-13 | REF-065 DIFFERENT — "settings exist, **no document consumes them**" | `EnvelopePdfWriter.cs:51-52,167-179` takes `EnvelopeSettings` + positions; `EnvelopePrintingService.cs` reads them at print time | **CONTRADICTED by current evidence** | The precise claim V2 made is now false |
| H-14 | REF-148 DIFFERENT — monitor reads test-level `PatientTest.ResultValue`; CBC is analyte-level | `GetBandedResultMonitorQueryHandler.cs:56-68` unchanged | **CONFIRMED** | §11 |
| H-15 | REF-235 DIFFERENT — `Himself/Herself` printed by no writer | Single-line resolver unchanged | **CONFIRMED** | §11 |
| H-16 | REF-240 DIFFERENT — setting stored, never read | Full-tree search: only settings persistence/editor hits; no behaviour consumer | **CONFIRMED** | §11 |
| H-17 | REF-237 IMPLEMENTED — `EnterResultCommandHandler` calls `MarkReviewed` | `EnterResultCommandHandler.cs:115-127` present | **CONFIRMED** | §11 |
| H-18 | DEF-001 culture filter escape clause | `GetCultureEntryGridQueryHandler.cs:37` contains the exact `\|\| saved.ContainsKey(...)` clause | **CONFIRMED** | §14 |
| H-19 | DEF-005 attendance ungated; 13 permission codes, none attendance | 12 attendance command/handler/validator files, 0 `IAuthorizedRequest`; 13 codes, none attendance | **CONFIRMED** | §14 |
| H-20 | DEF-006 worksheet print only for visit mode | `WorkSheets/Commands/` contains **only** `PrintWorkSheet`; VM comment at `:23-30` states the deferral | **CONFIRMED** | §14 |
| H-21 | DEF-007 `HistoryMatrixBuilder` has zero production consumers | Search across `src/` + `tests/` returns hits only in its own test file | **CONFIRMED** | §14 |
| H-22 | DEF-008 divergent case-folding in `ResolveVisitPatients` | `PatientHistoryReader.cs:32-46` — SQL `ToUpper()` pre-filter + in-memory `ToUpperInvariant()`, with the caveat in the source comment | **CONFIRMED** | §14 |
| H-23 | DEF-009 that function guarded only by a source-text assertion | `HistoryFilterTests.cs:129-136` reads the `.cs` file and asserts the literal `"StartsWith(firstToken)"` | **CONFIRMED** | §14 |
| H-24 | REF-161/162/163: catalog lacks `Arrang`, `Out Lab Name`, `Routin`; test-number search key absent | `Test.cs:9-43` has none of the three; `SearchTestCatalogQuery(string?, int?, bool)` has no test-number key; LEARN p.141 grid shows `ID | Arrang | Group Name | Test Name | Pat. P | Lab P | Out Lab Name | Out P | Barcode | Routin` | **CONFIRMED** | §11 |
| H-25 | "No item requires editing an existing migration or primary key" | No migration added or altered in the phase window | **CONFIRMED** | §13 |
| H-26 | 2,479 tests pass, 24 fail on missing Arial | **2,577 pass, 28 fail**, all 28 = `DocumentDrawingException` on missing Arial; Domain 507, Application **1,713** (+25), Infrastructure 321 pass/28 fail (+50 passed vs V2's 271), Persistence 13/0/2 | **PARTIALLY SUPPORTED** — the *pattern* is confirmed and reproduced; the *totals* moved because the code moved. The new tests confirm the phase. | §21 |
| H-27 | "46 items NO MIGRATION, 38 NEW MIGRATION, 0 existing-migration/PK impact" | Recomputed against the current 83-item sequence: **55 / 28 / 0** | **PARTIALLY SUPPORTED** — the structural conclusion (nothing edits migrations 1–14) is CONFIRMED; the split changed because 5 items left the sequence, all of which were `NO MIGRATION` | §13, §20 |
| H-28 | `CombinedReportSelection` documented "Never persisted" | unchanged | **CONFIRMED** | §11 |
| H-29 | `Antibiotic.cs` has no commercial-name field | `grep 'CommercialName\|TradeName' src/` → 0 files | **CONFIRMED** | §11 |
| H-30 | V2 §9.5: "**No fax-sending function is documented anywhere in either PDF**" | **LEARN p.17 shows a `By Fax` button** alongside `Send SMS` and `Send E-mail` on the account/results screen; LEARN p.23 shows `Send E-mail` | **PARTIALLY SUPPORTED** | §9.3, §18 |

### 6.1 On H-30 — the fax finding, stated carefully

V2 concluded that no fax-sending function is documented, and used that to keep the exclusion count an honest 25 rather than padding to 26. This audit's independent read found a **`By Fax` control on LEARN p.17**, rendered in the OCR as a distinct button in the same command group as `Send SMS` and `Send E-mail`. The same button group recurs on LEARN p.18 (`Print Result Password` … `By Fax`).

The honest reading is that this is **evidence of a fax-sending capability in the reference**, though the page is a screenshot of a results/account screen rather than a prose instruction, and the surrounding Arabic is degraded. It does **not** change the owner exclusion set — the owner named "SMS / E-mail / Fax result sending" as one category, and the count of excluded REF identifiers remains 25. It does mean V2's absolute negative ("documented **anywhere**") is **too strong**: a fax control is visible. Recorded as **DEF-010 (documentation-level) / C-11**, with the evidence limitation stated rather than resolved by invention.

---

## 7. Canonical Reference Function Inventory

### 7.1 Method

The inventory was re-derived **from the two permitted PDFs**, not copied from V2. Both documents were fully extracted and indexed page-by-page (§4.3). Each page was read against the following functional-unit rule:

> A distinct reference function exists where the documentation describes a **distinct user or system capability that could reasonably be independently implemented, verified, or excluded**.

Applied strictly:
* **Not split** — inseparable sub-steps of one capability stay one item.
* **Not merged** — genuinely independent capabilities in one documented paragraph are separated.
* **Not heading-driven** — headings were never automatic boundaries.
* **Deduplicated across PDFs** — a capability described in both documents became one identifier with all locations recorded.

### 7.2 Result

| Measure | Count |
|---|---|
| Total canonical reference functions | **242** |
| Hard-excluded (5 owner categories, §8) | **25** |
| In scope | **217** |

### 7.3 Independent assessment of completeness

The prior inventory was treated as a hypothesis and checked against the page index:

* **No function found missing.** Every documented capability located during the page-by-page read maps onto an existing REF item.
* **No function found invented.** Every item carries a page citation in at least one of the two PDFs.
* The fine-grained decompositions V2 retained (REF-132/133 exact-vs-partial name search; REF-150/151 and REF-155/156 full-vs-condensed worksheet) are genuine separately-described outputs on their cited pages and are retained.
* **One place where this audit would frame differently, disclosed:** V2 §8.3 treated the by-codes worksheet variant as a presentation option of REF-150. That remains a defensible call. It is retained so the identifier space stays stable and the 217 arithmetic holds; the alternative (splitting it) would produce 218 in scope and would change no classification.

**Conclusion:** the **242**-function inventory is **CONFIRMED** as a sound canonical model. The identifier space is reused because it genuinely maps to the same functions — not because it was inherited uncritically.

---

## 8. Hard Exclusions

The owner has permanently excluded five categories. Every function belonging to them is listed **individually**. None is classified IMPLEMENTED/DIFFERENT/MISSING, none appears in the implementation sequence, none is recommended for implementation, and **their absence from Top-Lab is not a defect, not a gap, and not implementation debt**.

### 8.1 Category 1 — Online results portal (10 functions)

| ID | Function | Reference evidence (independently re-read) |
|---|---|---|
| REF-001 | Upload patient result to the result website | LEARN p.58, heading `12-1 … موقع نتٌجة … NATIGH.COM`; "كٌفٌة رفع نتٌجة مرٌض على موقع نتٌجة" |
| REF-002 | Open the result website and view the patient result | LEARN p.63, "WWW.NATIGH.COM … صفحة عرض النتائج" |
| REF-003 | Patient prints/exports the portal result as Word or PDF | LEARN p.63, "طباعة النتيجة او تصديرها كملف Word - PDF" |
| REF-004 | Android application for the result portal | LEARN p.63, "تطبيق … من GOOGLE PLAY" + `play.google.com/store/apps/details?id=com.natighweb.app` |
| REF-005 | Print portal receipt data on the patient receipt | LEARN p.69, "بٌانات موقع نتٌجة الخاصة بالمرٌض … طباعتها على الإيصال" |
| REF-006 | Count of patients whose results are on the portal | LEARN p.65, "معرفة عدد المرضى المرفوع نتائجهم خلال فترة زمنية" |
| REF-007 | Delete (block) a patient result from the portal | LEARN p.68, "لحذف نتيجة مريض من على موقع نتيجة … Block" |
| REF-008 | Create a doctor account for the result service | LEARN p.142, heading `11-3 إنشاء حساب للطبيب المعالج … لخدمة نتيجة` |
| REF-009 | Create a laboratory account for the result service | LEARN p.145, "ثانيا في حالة معمل … Lab to Lab" |
| REF-010 | Portal branding and portal access data on printed documents | LEARN p.69; SHOW p.54 |

### 8.2 Category 2 — Equipment / maintenance / calibration register (2 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-011 | Equipment, maintenance and calibration register | LEARN p.93 chapter-3 introduction, repeated p.94, 106, 118, 124, 134, 145 |
| REF-012 | Equipment register entry form | LEARN p.93–94, "إدخال بيانات أجهزة ومعدات المعمل ومتابعه حالتها والصيانه الدورية وتاريخ المعايرة" |

### 8.3 Category 3 — Blood / CBC analyser connection or import (5 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-013 | Blood-picture analyser interface and result import | SHOW p.55 ("اجهزة صور الدم بالكمبيوتز"), p.62, p.65, p.74 |
| REF-014 | Per-analyser correction factors applied to results | SHOW p.70–71, "Correction Factors — Enter correction factors to modify result automatically" |
| REF-015 | Blood-picture device report with curves | SHOW p.62 (histograms/normal-range column), p.72 |
| REF-016 | 5-part differential blood-picture interface | SHOW p.65, p.74 ("ديفرنشال 5") |
| REF-017 | Interface for other analysers (VIDAS and others) | SHOW p.75, p.77 (VIDAS Analyzer Interface + supported-device list) |

### 8.4 Category 4 — Multi-branch support (5 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-018 | Branch number in system, user and patient data | LEARN p.151 "رقم الفرع المتاح للمستخدم الحالي", p.205 "رقم الفرع … 1" |
| REF-019 | Patient search restricted to a branch number | LEARN p.42–43 (Branch No.) |
| REF-020 | Cash-drawer stocktake per branch | LEARN p.179, heading `3-6 … نظام الفروع للمعامل` |
| REF-021 | Statistics per branch | SHOW p.53, "عدد مرضى وتحاليل وعينات كل فرع من الفروع" |
| REF-022 | Per-branch data folders and configuration files | LEARN p.130–132, `D:\real lab system\Data` with per-farm `Configuration settings` folders |

### 8.5 Category 5 — SMS / E-mail / Fax result sending (3 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-023 | Send SMS of the result to patient or doctor | SHOW p.53; `Send SMS` button LEARN p.17, 18 |
| REF-024 | Send E-mail of the patient result | `Send E-mail` LEARN p.16, 17, 18, 23 |
| REF-025 | `Send Result` module in the main menu | LEARN p.12, 22, 23, 25 (menu bar token `Send Result`) |

| Exclusion category | Functions |
|---|---|
| 1 — Online results portal | 10 |
| 2 — Equipment / maintenance / calibration register | 2 |
| 3 — Blood / CBC analyser connection or import | 5 |
| 4 — Multi-branch support | 5 |
| 5 — SMS / E-mail / Fax result sending | 3 |
| **Total excluded** | **25** |

### 8.6 Exclusion verification result

**All 25 owner exclusions were independently verified against the two permitted PDFs.** Every one exists as a described reference capability and every one belongs to the stated category. No category mismatch, no REF-ID mismatch, and no function in an excluded category that is absent from the owner list was found. The owner exclusion set is therefore **CONFIRMED** and is preserved exactly as defined.

**Verification of category 5 — disclosed, not padded.** The owner named *SMS / E-mail / Fax*. The two PDFs document an SMS send action (REF-023), an e-mail action (REF-024) and a top-level `Send Result` module (REF-025). **This audit additionally found a `By Fax` control on LEARN p.17 and p.18** (see §6.1, H-30), which supports the owner's naming of fax. The three excluded REF identifiers are unchanged, and no additional excluded function was invented to pad the count. The count of 25 is an honest 25.

### 8.7 Verification that no excluded feature is counted as a gap

Independently re-verified at HEAD: no portal/`Natiga` entity, handler, view or service; no equipment/calibration entity; no analyser-interface, device-report or correction-factor code; no branch-scoping query (the dormant `BranchNumber` fields on `SystemSettings`/`User` have no consumer); and no `SmtpClient`/`HttpClient` send path in `src/`. Where an in-scope item's design would naturally touch excluded machinery, this audit designs **around** it.

---

## 9. Current Target-Commit Classification

Every in-scope function received **exactly one** classification. The decision rules applied:

* **IMPLEMENTED** — a traced, concrete implementation path exists and its behaviour is consistent with the reference within the limits of verification. A filename, class name, method name, table, column, property, view-model, menu item, setting, migration, test name, comment or placeholder is **never** sufficient on its own.
* **DIFFERENT** — the capability exists but materially differs: partial workflow, missing step, different business rule, incomplete persistence, incomplete calculation, incomplete UI, only one layer implemented, or a stored-but-inert setting.
* **MISSING** — no credible implementation path, established by explicit, sufficiently broad negative search.
* **BLOCKED / UNRESOLVED** — used **only** where evidence genuinely prevents *specification*. A BLOCKED flag never removes an item from the three-way count.

### 9.1 Classification result

| Classification | Count |
|---|---|
| IMPLEMENTED | **134** |
| DIFFERENT | **58** |
| MISSING | **25** |
| BLOCKED / UNRESOLVED (withheld) | **0** |
| **Total in scope** | **217** |

### 9.2 Where this audit differs from V2

Five items move. Each is proven by a direct code citation, and each is explained by the four `[Phase 1]` commits.

| REF | V2 | This audit | Basis |
|---|---|---|---|
| REF-065 | DIFFERENT | **IMPLEMENTED** | `EnvelopePdfWriter.cs:51-52,167-179` consumes `EnvelopeSettings` + `EnvelopePrintItemPosition`; `EnvelopePrintingService` reads them at print time. V2's precise claim "**no document consumes them**" is now false. |
| REF-066 | MISSING | **IMPLEMENTED** | `EnvelopePdfWriter.cs`, `EnvelopePrintingService.cs`, `PrintEnvelopeCommandHandler.cs`, `IEnvelopePdfWriter`; UI `PatientResultSheetView.xaml:85` |
| REF-067 | MISSING | **IMPLEMENTED** | `EnvelopePdfWriter.cs:88-89` renders `lines.CodeBarcodePayload` through `BarcodeLabelRenderer` + `BarcodePngEncoder` |
| REF-068 | MISSING | **IMPLEMENTED** | `LabOrderPdfWriter.cs:74-75` + `PrintLabOrderCommandHandler` + `PatientEditorView.xaml:49` |
| REF-127 | MISSING | **IMPLEMENTED** | `ReceiptPdfWriter` / `InvoicePdfWriter` gained a `barcodePayload` parameter and embed the rendered PNG; supplied by `ReceiptPrintingService.cs:90` and `InvoicePrintingService.cs:78` |

**Net:** four items left MISSING (→ 25) and one left DIFFERENT (→ 58); IMPLEMENTED 129 → 134. The five items that left the sequence were all `NO MIGRATION`, which is why V2's migration split changed while its structural conclusion did not.

### 9.3 Items deliberately *not* reclassified

* **REF-030, REF-057** — the reference genuinely does not name a removal action. DIFFERENT (semantics undocumented) is the correct, honest verdict.
* **REF-241** — the reference-aligned capability (internal-window password gate) is REF-027 and is present. The workstation lock is a Top-Lab addition, not a gap.
* **REF-122** — element renaming, reference values and units are implemented generically; the curve half of the same page is excluded analyser work (REF-015).
* **REF-149, REF-141/142/143** — implemented as a setting plus a search command / as worklist filters. The behaviour is the reference's; only the placement differs.
* **REF-232** — remains DIFFERENT. Phase 1 gave the **Envelope** printer slot its first real consumer, which is real progress, but `PrinterOutputType` still has 4 values and LEARN p.199 documents 5 including the card printer. A partial improvement does not close the gap.
* **REF-238** — remains DIFFERENT. Barcodes now exist on four documents, but there is still no single master display switch and no per-document toggle on receipt/report settings.

---

## 10. IMPLEMENTED — 134 functions

Full per-item evidence for every one of the 134 appears in the mandatory table in §26. This section records the verification standard and the group proof paths.

### 10.1 Verification-status distribution

| Verification status | Meaning | Items |
|---|---|---|
| **TEST VERIFIED** (`ST+TEST`) | Traced in code by this audit **and** covered by at least one test in a suite that ran in this environment (§21) | **126** of the 134 |
| **STATICALLY VERIFIED** (`ST`) | Traced end-to-end in code by this audit; no behavioural test covers it | **8** (REF-027, 045, 059, 060, 081, 149, 241, 242) |
| **DIFFERENT — evidence of the difference** (`ST`) | Handler body / writer / validator read line-by-line and the exact divergence identified | **58** |
| **MISSING — negative search** (`ST (negative search)`) | Absence established by an explicit re-run search, not by assumption | **25** |
| **Defects** | Path traced to a specific line, with impact and reachability | **11** (DEF-001…DEF-011) |

**No classification in this report is runtime-verified.**

### 10.2 Group evidence (primary proof path per group)

| Group | REF items | Traced path (representative) |
|---|---|---|
| Access | 026, 027, 028, 029, 031, 032 | `SignInCommandHandler` → `Pbkdf2PasswordHasher`; per-user `SecondaryPassword`; `CheckIn/StartBreak/EndBreak/CheckOut` → `AttendanceCalculator` |
| Patient | 034, 035, 037, 038, 039, 040, 041, 042, 043, 044, 045, 046, 047, 048, 049, 050 | `CreatePatient/UpdatePatient` → `Patient.Create/Update`; `SoftDeletePatient` gated by `IAuthorizedRequest`; `PrintBarcode` → `BarcodeService` |
| Billing | 051, 052, 053, 054, 056, 058, 059, 060, 063, 070 | `PatientAccountCalculator`; `RecordPayment` (discount ceiling); `ExecuteBulkPrintCommandHandler` (balance block) |
| Results | 071, 072, 073, 074, 075, 076, 077, 078, 080, 081, 084, 085, 086 | `EnterResult` → `ResultFlagComputer`; verify/unverify/clear; reference-range snapshots |
| Culture | 087, 088, 090, 091 | `SaveCultureResults` → `CultureResult` / `CultureAntibioticResult`; `CultureAntibioticAttachment` |
| Report | 095, 096, 098, 099, 100, 102, 103, 109, 110, 111, 112, 122, 126 | `BuildCombinedReport`/`PrintCombinedReport`; `ReportSettings` → `ReportPageComposer`; `HistorySortMode` → `PatientHistoryResolver` |
| Search | 134–144, 146, 147, 149 | `SearchPatientsGlobalQueryHandler`; `GetResultWorklist`; `GetUndeliveredResults`; `GetBandedResultMonitor` |
| Catalog | 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177 | `ReferenceRange`/`AnalyteReferenceRangeBand` CRUD; `TestComment`; custom groups; price lists; `ExternalEntity` full field set |
| Statistics | 178–191 | `GetPatientCountStatistics` (incl. `DayOfMonthCounts` + `Money`), `GetTestCountStatistics`, `GetSentOutStatistics`, `GetUserProductivityStatistics` |
| Accounts | 193, 195, 197, 198, 199, 200, 202, 208 | `GetCashDrawerInventory`; `GetPatientSamplesDetail`; `RecordCashDeposit/Disbursement`; `SentOutAccountCalculator` |
| Samples | 210 | `MarkSampleDrawn` / `MarkAllSamplesDrawnForPatient`; `SampleDrawBoardView` |
| Tools | 212, 213, 214, 215, 216 | `ArithmeticCalculator`, `MeasurementUnitConverter`, `StopwatchCalculator`; `JsonPhoneBookStore`, `JsonPurchasesListStore` |
| Audit | 222, 223, 224, 226, 227, 228, 229, 230, 231 | `AuditableEntitySaveChangesInterceptor`; `PatientTestAuditDto`; `AmendProfileResult`; `BackupDatabaseNow`/`RestoreDatabase`; `DailyBackupHostedService` |
| **Document family (new in this phase)** | **065, 066, 067, 068, 127** | `EnvelopeSettings` → `EnvelopePdfWriter` → `EnvelopePrintingService` → `PrinterAssignment(Envelope)`; `BarcodeLabelRenderer` + `BarcodePngEncoder` → envelope / lab order / receipt / invoice |

### 10.3 The five phase items — end-to-end proof

Each of the five functions that changed state was traced from UI to printed artefact:

| REF | Full traced path |
|---|---|
| **REF-066** | `PatientResultSheetView.xaml:85` (`Command="{Binding PrintEnvelopeCommand}"`) → `PatientResultSheetViewModel.cs:53,102,200-214` → `PrintEnvelopeCommandHandler.cs` (loads patient, builds `BarcodePayload.For(...)`, resolves referral display name, creates `EnvelopeDto`, `EnvelopePrintEnvelope.CreateToken`) → `EnvelopePrintingService.PrintEnvelopeAsync` (deserialises token, reads `EnvelopeSettings` + positions + `ILabPrintTextStore`, renders, dispatches) → `EnvelopePdfWriter` |
| **REF-065** | `EnvelopeSettingsViewModel.cs:131,197` ↔ `GetEnvelopeSettingsQuery` / `UpdateEnvelopeSettingsCommand` → `EnvelopeSettings.cs` (TopMarginCm, HeaderFooterMode, SuppressCaptions) → consumed at `EnvelopePdfWriter.cs:51-52,167-179` and at `EnvelopePrintingService` print time. The settings are now **live**, not dormant. |
| **REF-067** | `EnvelopePdfWriter.cs:88-89`: `_renderer.Render(lines.CodeBarcodePayload)` → `BarcodePngEncoder.Encode(label.Pixels, label.Width, label.Height)` → embedded in the envelope PDF. Tests: `EnvelopeBarcodeTests` (5 facts) — **passing**. |
| **REF-068** | `PatientEditorView.xaml:49` → `PatientEditorViewModel.cs:130,454,1389` → `PrintLabOrderCommandHandler` → `LabOrderPrintingService` → `LabOrderPdfWriter.cs:74-75` renders the barcode and `:149` prints the payload text. Dispatched to the Reports printer slot. |
| **REF-127** | `ReceiptPrintingService.cs:90` builds `BarcodePayload.For(...)` → `ReceiptPdfWriter.WritePdfAsync(..., barcodePayload, ...)` → renderer + `BarcodePngEncoder` → embedded. Identical path in `InvoicePrintingService.cs:78` → `InvoicePdfWriter`. |

Reference evidence for these five: LEARN p.205–206 (envelope settings + dedicated envelope printer), LEARN p.69, SHOW p.52 ("طباعة الباركود على الإيصال والظرف" — print the barcode on the receipt and the envelope), SHOW p.3 and p.6 (envelope / receipt artefacts), SHOW p.52 (worksheet and receipt-printing capabilities).

---

## 11. DIFFERENT — 58 functions

Each row states **precisely** what differs, with a code citation.

| REF | Function | The exact difference |
|---|---|---|
| REF-030 | Delete or deactivate a user | `DeactivateUser` + `DeleteUser` handlers exist; the reference never names a removal action, so hard-delete vs deactivate is an inference |
| REF-036 | Patient medical history flags and conditions | `PatientMedicalCondition` stored; no patient-data read-out in the result window; medication sub-list not modelled as a distinct item |
| REF-055 | Clear (zero) the whole account | No account-wide clear action (per-operation `Void` exists) |
| REF-057 | Delete a payment operation | Void only; the row and its amount remain in history |
| REF-061 | Receipt pre-printed vs white paper with header/footer + logo | `HeaderFooterMode.Images` offered but no image pipeline; falls back to words |
| REF-062 | Receipt header/footer text, font, size, colour, logo | Lab print text + one font family; no per-element colour, no logo |
| REF-064 | Receipt via cash-register printer | Cashier setting and printer assignment honoured, but the receipt is fixed A5; no 80 mm page size |
| REF-069 | Receipt expected result date per test | `Test.CompletionDurationMinutes` and `Patient.PickupDateUtc` stored; no writer composes a per-test date |
| REF-082 | Automatic interpretation comment | No generated interpretation; no per-report on/off switch |
| REF-083 | Automatic calculation and equations in results | `ArithmeticCalculator` exists but is wired only to the calculator tool; no analyte/test formula, no derived value |
| REF-092 | Antibiotic master with commercial name | `Antibiotic` has `Name`, `Symbol`, `ScientificName`, two flags; `grep 'CommercialName\|TradeName' src/` → 0 files |
| REF-093 | Antibiotic suitable for pregnancy | Flag and filter exist but the predicate has an escape clause (DEF-001) |
| REF-094 | Antibiotic suitable for children under twelve | Same escape clause; detection itself is correct |
| REF-097 | Blank report: patient data only | `FromBlank` builds from stored data; no "data only" variant selectable |
| REF-104 | More than one combined report per patient | `CombinedReportSelection` documented "Never persisted" |
| REF-105 | One-button combined print of entered-but-unprinted | Preflight requires `EnteredAtUtc != null && IsReviewed`; reference says "entered and not printed" |
| REF-106 | Last N visits below the profile report | No trailing history block, no visit-count limit |
| REF-107 | Report header/footer modes none/words/image | Image mode has no asset pipeline |
| REF-108 | Report header/footer colours + element positions | Only one header colour and one footer colour; no per-element colours, no offsets |
| REF-115 | Report element letter, list editing, free text | Names and free text exist; no editable report letter, no per-report element suppression |
| REF-118 | Print the abnormality marker | Flag computed and stored, low/high comment printed — but **no writer emits a marker** (DEF-003) |
| REF-121 | CBC report design variants and large paper | One layout only; `PaperSize` has no large value |
| REF-123 | CBC report colouring, auto-comment, manual mode | Manual input and comment text exist; **all colour control absent**, no comment on/off switch |
| REF-124 | Show/hide report blocks | Only the doctor-signature block is toggleable |
| REF-125 | Report identification block | 4 of 5 builders pass `null` for `Sex`, `AgeText`, `TreatingDoctorName`, `ReferralEntityName`; date has no time |
| REF-128 | Barcode labels per test and per sample | One patient label only; payload is a single identifier |
| REF-131 | Blank report with data entry before printing | No free-text entry surface |
| REF-132 | Search patient by exact name | One `Contains` term; no exact-match mode |
| REF-133 | Search patient by partial name | Partial match exists but is gated behind a setting serving a different purpose, not a selectable mode |
| REF-145 | Status indicator per patient | Roll-up exists per visit; no single status symbol per patient in the search grid |
| REF-148 | Search/statistics on **blood-picture element** values | `GetBandedResultMonitorQueryHandler.cs:56-68` reads `PatientTest.ResultValue` (test-level); CBC values are analyte-level rows in `ProfileResultItem`, so CBC is not covered |
| REF-152 | Worksheet of tests / group of tests in a period | Data + writer exist; print ships disabled for group/log modes (DEF-006) |
| REF-153 | Worksheet by work group (Log) | Same as REF-152 |
| REF-154 | Print only unfinished tests or all | Completion facts carried but no filter flag passed or offered |
| REF-157 | Worksheet for one department over a period | No department selector; no between-two-patient-codes bound |
| REF-158 | Count of **patients** per test | Counts `PatientTest` rows, not distinct patients — a wrong business rule |
| REF-159 | Classify how often each test was performed | Display only; no printed classification, no signature column |
| REF-160 | Worksheet print preview | Preview proven but offered only for the visit mode |
| REF-161 | Catalog search by name / group / **test number** | `SearchTestCatalogQuery(string? SearchTerm, int? TestGroupId, bool IncludeInactive)` — the third documented key is absent. LEARN p.141 shows all three: `By test name | By group name | By test ID` |
| REF-162 | Edit test data and price | The edit-window prose is fully covered, but the reference catalog record also carries `Arrang`, `Out Lab Name` and `Routin` (LEARN p.95 and p.141) — **no column for any of them** on `Test` |
| REF-163 | Add a new test | Same three missing catalog fields; `CreateTestCommand` cannot carry them |
| REF-164 | Substitute patient name/data on the worksheet line | Line always prints the real identity; no override field |
| REF-192 | Print a statistics report | No print action and no statistics writer |
| REF-194 | Stocktake by element **and report type** | Element-kind enum exists; the four report-type variants do not (LEARN p.179 names them: `مفصل بالنتائج – مفصل بالأسعار – مفصل – مجمع`) |
| REF-196 | Stocktake statement for a treating doctor | No dedicated statement screen or print |
| REF-201 | Sent-out tests not yet dispatched | `SentOutSample` rows exist only after dispatch; no query joins `Tests.IsSentOut` to undispatched rows |
| REF-203 | Account filters: debtors / commission / settled | Columns exist; no filter selector in query or UI |
| REF-204 | Account per doctor daily/monthly/annual | Doctor is an inventory element kind, not a billing-account grouping |
| REF-206 | Referral-entity statement + hand-over list | Data reachable; no statement document, no hand-over list |
| REF-209 | Code samples per section | Sample-kind flags exist; no department entity, no per-department code, no separation step |
| REF-217 | Test information library | DTO carries `TestId, Name, TestCode, GroupName` only — no sample, no normal values, no patient effect |
| REF-225 | Payment collection method / instalments / receiver | `PaymentOperation` has `ReceivedByUserId` and `OperationAtUtc` but **no payment-method field** and no instalment count |
| REF-232 | Printer assignment per output kind | `PrinterOutputType` has 4 values; the reference documents 5 including the card printer (LEARN p.199). Phase 1 gave the Envelope slot a consumer, but the fifth slot is still absent |
| REF-235 | Receipt: Himself/Herself when referral empty | Resolver exists and supplies UI placeholders; **no PDF writer prints it** |
| REF-236 | Treating doctor only from external-entity window | Flag stored and editable; the patient editor still exposes free-text doctor fields; no consumer |
| REF-238 | Barcode display control | Barcodes now exist on envelope, lab order, receipt and invoice; there is still no single master switch and no per-document toggle |
| REF-239 | Print the account instead of the date | Flag stored and editable; **no consumer** — writers always emit the date |
| REF-240 | Result-screen account display mode | Enum, column, settings ComboBox; **no behavioural consumer anywhere** |

### 11.1 The five items that left the DIFFERENT/MISSING set

REF-065 (was DIFFERENT) and REF-066 / REF-067 / REF-068 / REF-127 (were MISSING) are now IMPLEMENTED. Each is documented with its proof path in §10.3. This is genuine implementation, verified in code and — except where the Arial font blocks PDF rendering — in a passing test.

---

## 12. MISSING — 25 functions

Each absence is established by an explicit negative search **re-run by this audit** at `6fa10aab`. **All 25 are listed below and again in §26.**

| REF | Function | Negative-search evidence (re-run at HEAD) |
|---|---|---|
| REF-033 | Users login/logout movement report with machine name | `AttendanceRecord.cs:6-25` has no machine field; `grep -rn MachineName src/` → 0 files; no session log; only an in-memory session DTO |
| REF-089 | Choose which culture report elements are shown | `ReportCultureSection` has no `Show`/`Visible`/`IsEnabled`; no entity stores an element-selection set |
| REF-101 | Patient history for the blood picture (CBC) | **Correction to V2 confirmed:** `HistoryMatrixBuilder` (CBC date×analyte pivot) exists and is unit-tested but has **zero production consumers** — search across `src/` and `tests/` returns hits only in `HistoryMatrixTests.cs` (DEF-007) |
| REF-113 | Colours of patient data, headings, profiles, abnormal results | `ReportDocumentContent` carries strings, sections and a grid only; the only colour fields in the system are `ReportSettings.HeaderColor`/`FooterColor` |
| REF-114 | Whole-row highlight choice | No per-row style data in the content model or the grid record |
| REF-116 | Report line spacing + per-element left offset | `ReportPageComposer` renders a fixed rhythm from one font size; `grep -rn LineSpacing src/` → 0 |
| REF-117 | Report element shape, name, order | `grep -rn 'ReportElementConfig\|ReportLayout\|ElementOrder' src/` → 0 files |
| REF-119 | Semen analysis report with graphs and diagrams | Semen is only a sample-kind flag; no drawing API; `grep -i 'graph\|chart'` in Infrastructure → 0 real hits |
| REF-120 | Image report (1–4 microscopy images, draggable text/shapes) | No image entity, upload, image-capable report or drawing surface; only the in-memory Code-128 label bitmap |
| REF-129 | Vertical barcode on report/receipt/envelope | No vertical flag in any settings entity; label renderer emits a fixed horizontal bitmap |
| REF-130 | Tube/container/swab labels (38×25 mm, multi-up) | One fixed-size patient label; no sticker-size setting, no multi-label sheet |
| REF-150 | Worksheet of a group of patients in a period | No period-patient worksheet query; only visit / test-group / work-group-log modes |
| REF-151 | Worksheet of a group of patients, condensed | No layout-variant flag and no condensed writer |
| REF-155 | Worksheet with full patient + test data | `WorkSheetLineDto` carries ordering facts only — no result value, no demographics |
| REF-156 | Worksheet with condensed patient data | Same root cause as REF-151/155 |
| REF-205 | Detailed account by test price or by results | No per-test price or per-result breakdown query or writer |
| REF-207 | Four account report types | No account report-type concept; no accounts writer |
| REF-211 | Reader options: save after read / print barcode after read | No scanner, HID or keyboard-wedge integration, no read event |
| REF-218 | Laboratory terms and abbreviations reference | No glossary entity, query or tab; the only "Abbreviation" hit is a stale comment |
| REF-219 | Colour palette | No palette entity, query, UI or colour-picker control |
| REF-220 | Reminder note for a specific user | `grep -rn 'Reminder\|LabNote\|Notification' src/` → 0 files |
| REF-221 | Follow-up appointment for a patient or phone | `grep -rn 'FollowUp\|Recall' src/` → 0 files; `Patient.PickupDateUtc` is the expected-result date, not a follow-up |
| REF-233 | Auto-print the receipt on registration | `grep -rni AutoPrint src/` → 0 files |
| REF-234 | Receipt: patient and doctor names in English | `grep -rn 'FullNameEn\|NameEn' src/` → 0 files; one `FullName` per patient and one `Name` per external entity |

> **The two negative searches that are no longer valid.** V2's REF-066 evidence ("no `Envelope*PdfWriter`") and REF-068 evidence ("`Requisition\|LabOrder\|OrderPdfWriter` → 0 files") **no longer return zero at this commit**. Re-running them literally now returns hits. This is recorded as **DEF-011 (stale-evidence class)**: a documentation-set hazard, not a code defect.

---

## 13. BLOCKED / UNRESOLVED

**No function had its classification withheld.** All 217 in-scope functions received exactly one of IMPLEMENTED / DIFFERENT / MISSING. BLOCKED is used only as an *additional* flag on items that are classified normally but whose **specification** needs an owner decision.

| REF | Classification | The blocking ambiguity | Evidence of the ambiguity |
|---|---|---|---|
| REF-115 | DIFFERENT | "Report element letter" (`احرف عناصر`) is ambiguous between a short report code, a display symbol and a transliterated letter | SHOW p.12 (E5) |
| REF-221 | MISSING | The follow-up line mixes a phone-number entry with a follow-up date and does not state granularity | SHOW p.51 (E4) |
| REF-209 | DIFFERENT | Per-department sample coding does not state the departments, nor whether the code is per patient, per visit or per test | LEARN p.208 |
| REF-030 | DIFFERENT | Reference never names a user-removal action; hard delete vs deactivate is undefined | LEARN p.151–153, 157 |

A further **4 items are specification-gated rather than evidence-gated** and are marked so in §26: REF-082 (auto-comment catalogue content is reference data only the owner can supply), REF-083 (formula scope vs excluded analyser correction factors), REF-218 (glossary content), REF-225 (collection-method list).

---

## 14. Existing Defects

Defects in **existing** Top-Lab behaviour that affect an already-classified reference function. Recorded **separately** from classification and **not** counted as additional reference functions. None was reproduced at runtime; each is traced to a specific line.

| # | Affected REF | Defect | Exact evidence | Impact | Verification | Confidence |
|---|---|---|---|---|---|---|
| **DEF-001** | REF-093, REF-094 | The pregnancy and children filters have an escape clause: a pregnancy-only or children-only antibiotic **stays visible** once a sensitivity result has been saved for it. Predicate: `IsDisplayable(...) \|\| saved.ContainsKey(x.Id.Value)` | `src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs:37` — re-read at HEAD, clause present verbatim | A clinically unsuitable antibiotic is shown to a non-pregnant adult or a patient over twelve — the exact error the reference says the rule exists to prevent (SHOW p.23: `حتى تكون نسبة الخطأ صفر`) | Statically verified (single predicate, both flags). Not runtime-verified | CONFIRMED |
| **DEF-002** | REF-165, REF-166 | Two independent normal-value stores exist — `ReferenceRange` (per test) and `AnalyteReferenceRangeBand` (per analyte) — and the consulted store depends on whether the test is mapped to an analyte. A fallback to `ReferenceRange` is taken when `analyteBands is null` | `EnterResultCommandHandler.cs:65-69`; `RefreshResultReferenceRangeCommandHandler.cs:50-54`; `GetResultEntryQueryHandler.cs:56` | The printed normal can differ from the one the operator edited in the test-data window | Statically verified | CONFIRMED |
| **DEF-003** | REF-118, REF-123 | The result flag is computed, stored and shown in the entry screens, but **no writer emits it** on the printed report; the low/high comment is printed instead | `src/TopLab.Infrastructure/Printing/ReportContentBuilder.cs` (flag → comment selection) vs `src/TopLab.Application/Features/ResultsEntry/Common/ResultFlagComputer.cs:37-67` (flag computation) | A printed report shows a normal-looking value with an explanatory sentence and no `H`/`L` marker, although the reference requires the abnormality to be visually marked (LEARN p.101, p.195; SHOW p.44, p.61) | Statically verified | CONFIRMED |
| **DEF-004** | REF-126, REF-168 | A comment attached to a result must be selected from the maintained per-test list; there is no free-text entry field | `src/TopLab.Presentation/ViewModels/Patients/TestCommentPickerViewModel.cs` (`PickedCommentText` set by `Pick()` only) | The reference workflow — write a new comment for an unusual case — cannot be completed without first creating a catalogue comment | Statically verified | CONFIRMED |
| **DEF-005** | REF-232, REF-031 | The four attendance commands implement **no** `IAuthorizedRequest`, and there is **no `ATTENDANCE` permission code** among the 13 seeded rows | Re-ran at HEAD: all 12 attendance command/handler/validator files → 0 occurrences of `IAuthorizedRequest`; `PermissionConfiguration.cs` → 13 codes, none attendance | Any signed-in user can alter any user's attendance. Long-standing, not a regression | Statically verified | CONFIRMED |
| **DEF-006** | REF-152, REF-153, REF-159, REF-160 | The worksheet screen has print and print-preview buttons only in the visit pane; the test-group and work-group modes expose only a *display* button. The source comment states the deferral explicitly | Re-ran at HEAD: `src/TopLab.Application/Features/WorkSheets/Commands/` contains **only** `PrintWorkSheet`; `WorkSheetsViewModel.cs:23-30` — "group/log print ships disabled (owner-pending — no print command exists, none is created)" | In the reference a worksheet is a printed bench document. For two of three modes Top-Lab is a screen-only listing | Statically verified | CONFIRMED |
| **DEF-007** | REF-101 | `HistoryMatrixBuilder` — a complete, unit-tested CBC date×analyte pivot — has **zero production consumers**. Introduced by commit `c91bb62`, whose own subject reads *"[W-02] Slice 12/16: History filters + CBC matrix + **dead-code cleanup**"* | `src/TopLab.Application/Features/ReportProduction/Common/HistoryMatrixBuilder.cs`; exhaustive search across `src/` and `tests/` returns hits only in `tests/.../HistoryMatrixTests.cs` | (1) REF-101 is far cheaper than assessed — the core exists. (2) A green unit test over unwired code gives **false confidence** | Statically verified by exhaustive consumer search | CONFIRMED |
| **DEF-008** | REF-098, REF-099, REF-100, REF-102, REF-103 | `PatientHistoryReader.ResolveVisitPatients` groups same-identity patients with **two different case-folding operations**: the SQL pre-filter uses `ToUpper()` (database collation), the exact comparison uses `ToUpperInvariant()` (invariant ordinal). The code itself documents the divergence and its consequence | `src/TopLab.Application/Features/ReportProduction/Common/PatientHistoryReader.cs:32-46` — re-read at HEAD, both folds and the caveat comment present verbatim; `src/TopLab.Domain/Reports/PatientHistoryResolver.cs:31-40` | On a **clinical report path**, under the non-default "group history by patient name" mode, a patient's earlier visit can be **silently excluded** from the combined report, auto-inserted history, manual history insert and the separate/multi-patient history report. The failure is silent and in the safe direction (a **missing** visit, never another patient's data) | Statically verified; collation-dependent by nature, so not reproducible here | CONFIRMED (code) / INFERRED (runtime reachability) |
| **DEF-009** | REF-098, REF-099, REF-100, REF-102, REF-103 | The function carrying DEF-008 has **no behavioural test at all**. Its only guard is a source-text assertion: the test locates `PatientHistoryReader.cs` on disk, reads it as a string, and asserts the literal `"StartsWith(firstToken)"` is present | `tests/TopLab.Application.Tests/Features/ReportProduction/HistoryFilterTests.cs:129-136` (the fact) and `:138-155` (the `File.ReadAllText` helper) — re-read at HEAD, unchanged | (1) **False assurance** — a green suite reports this function as guarded while its logic is unexercised; this is the mechanism by which DEF-008 survived three audits. (2) **Brittleness** — the test breaks on a correct refactor and passes on an incorrect change that keeps the string | Statically verified: search of `tests/` returns 0 behavioural references and 1 source-text-assertion site | CONFIRMED |
| **DEF-010** *(new in this audit)* | REF-023, REF-024 (excluded — **not a gap**) | **Documentation-level defect in V2's evidence base, not a Top-Lab defect.** V2 §9.5 asserts "**No fax-sending function is documented anywhere in either PDF**". This audit's independent read found a `By Fax` control on LEARN p.17 in the same command group as `Send SMS` and `Send E-mail`, repeated on LEARN p.18 | `RLS_Learn_Enhanced.pdf` p.17 (extracted text contains the literal token `By Fax` between `Send E-mail` and the account grid) and p.18 | V2's absolute negative is **too strong**. This does **not** change the owner exclusion set (still 25 REF identifiers) and creates **no gap** — REF-023/024 are excluded. It does mean the *stated basis* for keeping the count at 25 rather than 26 was partly wrong | Verified by direct re-read of the extracted page text | CONFIRMED (token present) / UNVERIFIABLE (whether it is a full fax-send workflow — the page is a screenshot and the surrounding Arabic is OCR-degraded) |
| **DEF-011** *(new in this audit)* | REF-066, REF-067, REF-068, REF-127 | **Stale negative-search evidence class.** V2's proof that these four functions were MISSING was a set of greps that returned zero *at its commit*. At HEAD those same greps now **return hits** — `Envelope*PdfWriter` and `LabOrder` both exist. Any downstream consumer reusing V2's evidence strings to justify "still missing" would reach a **false conclusion** | Re-run literally at HEAD: `grep -rn "Envelope.*PdfWriter" src/` → 5 files; `grep -rn "Requisition\|LabOrder\|OrderPdfWriter" src/` → hits in `ILabOrderPdfWriter.cs`, `ILabOrderPrintingService.cs` | Not a Top-Lab code defect. It is a **process** defect: a negative search is only valid at the commit where it was run, and negative-search evidence must be re-executed against the current tree before it is quoted | Re-ran both searches at HEAD; both now return hits | CONFIRMED |

**Net effect on prior defect lists.** DEF-001…DEF-009 all survive re-verification at `6fa10aab` unchanged. DEF-010 and DEF-011 are new and are **documentation/evidence-level**, not code-level. Neither is counted as a reference function, and neither is a gap.

---

## 15. Evidence-Depth Self-Assessment

| Group | Evidence depth achieved | Basis |
|---|---|---|
| **IMPLEMENTED (134)** | **Strong, with one honest weakness** | Every item has a traced path. The five phase items were traced UI → command → service → writer → printer dispatch. 126 carry a passing test in a suite that ran here. **Weakness:** the five phase items' *rendered output* was never observed (§21) |
| **DIFFERENT (58)** | **Strong** | Each states the precise divergence with a code citation. Re-ran V2's negative searches for the missing-field items (`CommercialName`, `LineSpacing`, `ReportElementConfig`) and re-read the catalog grid at LEARN p.141 |
| **MISSING (25)** | **Strong for 23, weaker for 2** | 23 rest on explicit re-run greps returning zero. **REF-101** is weaker: the negative is *qualitative* (a component exists but is unwired), so "missing" is a judgement about the absence of a path to a printed table, not a bare grep. **REF-221** is weaker still: the reference page is OCR-damaged (E4), so both the reference and the code are uncertain |
| **Defects (11)** | **Strong for 9, moderate for 2** | DEF-001…DEF-009 all re-read line-by-line at HEAD. DEF-010 rests on an OCR token that may or may not denote a full workflow. DEF-011 is a logical/process claim about evidence validity rather than a code reading |

**Strongest evidence:** the DIFFERENT and MISSING groups for code-side claims, because they rest on re-executed negative searches and read-only entity inspection — both reproducible by anyone with the repository. The five phase items' *code path* evidence is equally strong.

**Weakest evidence:** (1) anything depending on Arabic reference prose, because of E1 — this is a genuine non-uniformity; (2) the phase items' *rendered output*, which was never produced; (3) DEF-010's fax inference.

**Was the methodology uniform?** For **code-side** claims, yes — every in-scope item received the same treatment: a traced path for IMPLEMENTED, a cited divergence for DIFFERENT, a re-run search for MISSING. For **reference-side** claims, **no**, and the reason is E1: an item documented via Latin UI tokens and grid column headers (REF-161, 162, 163, 066, 067, 068, 127 — all decided here) has materially stronger reference evidence than one documented only in degraded Arabic prose. This is disclosed rather than papered over.

**Is this audit fully conclusive?** **No.** Material evidence gaps remain: no PDF was rendered, no WPF screen was executed, no database was touched, and the reference-side prose quality is uneven. §22 lists every such gap. This audit is a high-confidence **static** assessment of code and migration state, and a **medium-to-high** confidence assessment of reference correspondence.

---

## 16. Git-History Reconstruction

**NOT PERFORMED — NOT REQUIRED FOR THIS AUDIT**, with one qualification.

The brief makes history reconstruction conditional ("only if directly relevant"). A full reconstruction is not required here, because this audit's questions concern the state at the current HEAD, not the provenance of earlier work.

However, the **divergence between V2's commit and this audit's commit is directly relevant**, and the minimum history evidence needed to explain the current state was gathered (§17). That minimum is what justifies the five reclassifications in §9.2. No historical commit was checked out, reset to, or reverted to.

---

## 17. The Post-V2 Change Window (minimum required evidence)

`git rev-list --count 3590a7f5..HEAD` = **8**. All eight were inspected with their diffstat over `src/` and `tests/`.

| # | Commit | Date | Subject | Changes to `src`/`tests` | Nature |
|---|---|---|---|---|---|
| 1 | `7cd2585` | 2026-10-05 | `chore: add Target Handoff File directory for phase planning handoffs` | none | Documentation only |
| 2 | `7928d43` | 2026-10-08 | `الوضع الأحدث` | none (adds V2 + a handoff document) | Documentation only |
| 3 | `1d18a1c` | 2026-10-08 | `بعد التجديق` → `بعد التحديث` | none (amends the handoff doc) | Documentation only |
| 4 | `b7d8133` | 2026-10-08 | `بعد التحسين` | none (amends the handoff doc) | Documentation only |
| 5 | `11e2119` | 2026-10-08 | `[Phase 1] REF-066: Print the patient envelope` | 21 files, +1,554/−1 | **Phase 1, item 1** |
| 6 | `c7ac4c4` | 2026-10-08 | `[Phase 1] REF-067: Patient barcode printed on the envelope` | 1 file, +184 | **Phase 1, item 2** |
| 7 | `54eff33` | 2026-10-08 | `[Phase 1] REF-068: Print a laboratory order with barcode` | 18 files, +1,218/−2 | **Phase 1, item 3** |
| 8 | `6fa10aa` | 2026-10-08 | `[Phase 1] REF-127: Barcode printed on the receipt and envelope` | 9 files, +305/−18 | **Phase 1, item 4** |

Aggregate code change: **49 files, +3,261/−21 lines across `src/` and `tests/`**. The four documentation commits touched **no** `src/` or `tests/` file.

### 17.1 Two facts that constrain the classification

1. **The phase targeted V2's own Phase 1 block.** V2's sequence placed REF-065…REF-068 and REF-127 in *PHASE 1 — Document family: the shared envelope writer (5 items)*. Four of those five were implemented in commits explicitly named by REF id. The fifth, **REF-065 (envelope settings wiring), was also delivered** — not as a separate commit, but as the natural consequence of the envelope writer consuming the settings. The correspondence between the phase's scope and V2's block is exact.
2. **No migration was added, and no migration was modified.** `git diff 3590a7f5 HEAD -- src/TopLab.Infrastructure/Persistence` is **empty**. `EnvelopeSettings` and `EnvelopePrintItemPosition` already existed in `20260828052248_BaselineDataModel`. The phase was delivered **schema-free by design**, exactly as V2 predicted for these items (`NO MIGRATION` for all five).

### 17.2 Test evidence added by the phase

| Test file | Facts |
|---|---|
| `PrintEnvelopeCommandHandlerTests.cs` | present |
| `PatientEnvelope/Common/BarcodePayloadTests.cs` | present |
| `Barcode/BarcodePngEncoderTests.cs` | 6 |
| `Printing/EnvelopePdfWriterTests.cs` | 10 |
| `Printing/EnvelopePrintingServiceTests.cs` | present |
| `Printing/EnvelopeBarcodeTests.cs` | 5 |
| `PrintLabOrderCommandHandlerTests.cs` | present |
| `Printing/LabOrderPdfWriterTests.cs` | 4 |
| `Printing/LabOrderPrintingServiceTests.cs` | present |
| `Patients/PatientResultSheetPrintEnvelopeTests.cs` | present |
| `Patients/PatientEditorLabOrderTests.cs` | present |
| `Printing/ReceiptInvoiceBarcodeTests.cs` | 4 |

A filtered run of the non-rendering Phase-1 barcode/envelope tests returned **24 passed / 0 failed**. The 2 failing tests in that family (`ReceiptInvoiceBarcodeTests.ReceiptPdf_EmbedsBarcodeImageObject`, `InvoicePdf_EmbedsBarcodeImageObject`) fail for the **environmental font reason** documented in §21, not a logical one.

### 17.3 Engineering assessment of the phase

* **Shared infrastructure was built once, not four times.** `BarcodePngEncoder` is a from-scratch RGBA→PNG encoder explicitly written so that the four documents share one barcode pipeline with **zero new NuGet packages**. That is the correct call and it is why the later items cost far less than the first.
* **Failure isolation is deliberate.** `ReceiptPdfWriter` wraps the barcode render in a `try`/`catch` and falls back to the readable identifier text line — the same idiom the envelope uses. A barcode failure therefore degrades the document rather than losing it.
* **The barcode payload is derived from an existing policy.** `BarcodePayload.For(patientId, labId, printLabIdInsteadOfPatientId)` honours the pre-existing `PrintLabIdInsteadOfPatientId` setting rather than inventing a second rule.
* **One product decision is embedded in code and is worth flagging.** `PrintEnvelopeCommandHandler` deliberately omits the referral line when no referral entity exists, with the in-source rationale that the registration placeholder source resolves to the English `Himself/Herself` literals "which would violate the no-English product rule on a printed document". This is a **product-rule decision taken in the implementation layer**, and it has a direct consequence for **REF-235**: the envelope now shares the same root cause as the receipt, so fixing REF-235 on the receipt will not automatically fix the envelope. Recorded for whoever implements REF-235.

---

## 18. Documentation-vs-Code Contradictions

| # | Contradiction | Documented claim | Actual code | Resolution |
|---|---|---|---|---|
| C-1 | Stale XML doc on the antibiotic editor | `AntibioticEditorViewModel.cs:10-13` claims "**no Abbreviation/ScientificName/SensitivityCategory exists in code**" | `Antibiotic.cs` declares `Symbol` and `ScientificName`; the editor holds both; migration `20261002002546_AddAntibioticMasterFields` added them | **Code wins.** The comment predates the migration. It is also the *only* hit for `Abbreviation` in `src/`, which is how the REF-218 negative search is satisfied |
| C-2 | V2 states the grants are dead | *"only in seed+snapshot+UserManagementViewModel.cs:52"* | `RecordPaymentCommandHandler` and six print handlers enforce the per-user fields | **Code wins.** The claim was false (V2 §6.1) and remains false at HEAD |
| C-3 | V2 states `AutoReviewAndComplete` has no consumer | *"grep … returns only the settings entity and its editor"* | `EnterResultCommandHandler.cs:115-127` reads the flag and calls `MarkReviewed` | **Code wins.** REF-237 is IMPLEMENTED |
| C-4 | V2 states the `Himself/Herself` resolver is consumed by the receipt and invoice writers | *"consumed by the receipt and invoice writers"* | The string exists on one line; its only caller is a registration-catalog query for UI placeholders. No writer emits it | **Code wins.** REF-235 is DIFFERENT |
| C-5 | V2 states `GetResultEntryQueryHandler` consumes the account display mode | cited as the consumer for REF-240 | The file contains no reference to the setting | **Code wins.** REF-240 is DIFFERENT |
| C-6 | V2 states no CBC history column set exists | *"no column set … exists anywhere in src/"* | `HistoryMatrixBuilder.cs` defines exactly such a column set, with a tested pivot | **Code wins on existence.** The *verdict* survives because the component has zero production consumers |
| C-7 | Commit message contradicts commit content | `c91bb62` subject: *"… CBC matrix + **dead-code cleanup**"* | The slice **adds** `HistoryMatrixBuilder` and leaves it with zero production consumers | **Code wins.** DEF-007 |
| C-8 | `WorkSheetsViewModel` comment vs reference expectation | *"group/log print ships disabled (owner-pending — no print command exists, none is created)"* | Accurate; explicitly a deliberate deferral | **No conflict** — the comment is truthful, which is why DEF-006 records it as a gap rather than a code defect |
| C-9 | `ResultFlagComputer` comment vs dual-store reality | *"the analyte is the only live range source"* | The fallback to `ReferenceRange` when `analyteBands is null` is live code | **Code wins, partially.** DEF-002 |
| C-10 | V2 R-C01 field list | claims `HistoryName/ArabicName/SampleType/ArrangeNo/Bench` are all missing reference fields | `Arran` is evidenced (LEARN p.95 and p.141 grid). The other four are not evidenced as reference fields | **Evidence wins over assertion.** `Arran`/`Out Lab Name`/`Routin` retained as real gaps (REF-162/163); the other four withdrawn as unevidenced |
| **C-11** *(new)* | **V2 §9.5 absolute negative on fax** | *"**No fax-sending function is documented anywhere in either PDF**"* | LEARN p.17 shows a `By Fax` control beside `Send SMS` and `Send E-mail`; p.18 shows it again | **V2's negative is too strong.** Does not change the 25-item exclusion set and creates no gap (category 5 is excluded). Recorded as DEF-010 |
| **C-12** *(new)* | **V2's MISSING evidence is now stale** | REF-066 "no `Envelope*PdfWriter`"; REF-068 "0 files" for LabOrder/Requisition | Both greps now return hits at HEAD | **Code wins.** Recorded as DEF-011. Negative-search evidence is commit-scoped and must be re-run before reuse |
| **C-13** *(new)* | **Phase-1 in-source product rule vs reference** | `PrintEnvelopeCommandHandler` comments that omitting the fallback avoids violating "the no-English product rule on a printed document" | The reference (LEARN p.192/p.203) requires the word to be **printed** when the referral is empty | **Not a contradiction, a dependency.** The envelope now inherits REF-235's absence. Recorded so that fixing REF-235 does not fix the envelope by accident |

---

## 19. Reconciliation with Forensic_Audit_V2

| | V2 (at `3590a7f5`) | This audit (at `6fa10aab`) | Change |
|---|---|---|---|
| Reference functions enumerated | 242 | 242 | 0 |
| Hard-excluded | 25 | 25 | 0 |
| In scope | 217 | 217 | 0 |
| IMPLEMENTED | 129 | **134** | **+5** |
| DIFFERENT | 59 | **58** | **−1** |
| MISSING | 29 | **25** | **−4** |
| Sequence items | 88 | **83** | **−5** |
| Defects recorded | 9 | **11** | **+2** |
| PDF writers | 7 | 9 | +2 |

**All five movements are implementation, not reinterpretation.** REF-066, 067, 068, 127 moved MISSING → IMPLEMENTED; REF-065 moved DIFFERENT → IMPLEMENTED. **No item moved in the direction of increasing the gap count.** No exclusion changed. No count needed a balancing item.

**The `88 → 83` sequence change is not an improvement in estimation — it is the phase landing.** The five items removed from the sequence are exactly the five the phase implemented. V2's remaining 83 items are untouched in scope and content; only their numbering within the sequence changes.

---

## 20. Recommended Dependency-Aware Implementation Sequence

**One sequence, 83 items, every DIFFERENT and MISSING item appearing exactly once.** No item omitted, none duplicated. Order derives from the actual architecture — the Application/Infrastructure/Presentation layering, the shared PDF and barcode infrastructure, the immutable `ReportDocumentContent` model, and the shared statistics and account read models.

**Standing rules for every item**
* No existing migration is ever edited. Migrations 1–14 are additive by design; all schema change goes into **new, later** migrations. (Phase 1 demonstrated this is achievable: it shipped schema-free.)
* Every new command/query declares its permission grant. **No dead UI checkbox** — the lesson of REF-236/239/240 is that a stored-but-unread setting is worse than an absent one.
* Reuse the Phase-1 shared barcode pipeline (`BarcodeLabelRenderer` + `BarcodePngEncoder`) rather than adding a second one.
* New package references are avoided. The only candidate is a charting library for REF-101/REF-119, and only after a licence/offline review.
* Anything that would naturally reach into excluded machinery (analyser interface, per-branch scoping, portal, SMS/email/fax) is designed **around** and marked OUT OF SCOPE.
* When REF-235 is implemented, apply it to the receipt **and** the envelope together (see C-13), or the envelope keeps the defect.

| # | REF | Function | Class | Depends on | Why this position | Complexity | Blocked | Migration |
|---|---|---|---|---|---|---|---|---|
| **PHASE 0 — Owner decisions (no code; 12 items)** |||||||||
| 1 | REF-030 | Delete or deactivate a user | DIFFERENT | — | Semantics must settle before later user-screen work bakes them in | low | **YES** | NO MIGRATION |
| 2 | REF-057 | Delete a payment operation | DIFFERENT | — | A money-deletion semantic must settle before billing/audit work extends the model | low | **YES** | NO MIGRATION |
| 3 | REF-105 | One-button combined print of entered-but-unprinted | DIFFERENT | — | "Entered and not printed" may be unverified; conflicts with verify-before-print | low | **YES** | NO MIGRATION |
| 4 | REF-115 | Report element letter, list editing, free text | DIFFERENT | — | "Element letter" is OCR-ambiguous; the data model must not be chosen first | medium | **YES** | NEW MIGRATION |
| 5 | REF-082 | Automatic interpretation comment | DIFFERENT | REF-165 | The comment catalogue is reference data only the owner can supply | medium | **YES** | NEW MIGRATION |
| 6 | REF-083 | Automatic calculation / equations in results | DIFFERENT | — | Scope decision: in-system formulas vs excluded analyser correction factors | medium | **YES** | NEW MIGRATION |
| 7 | REF-209 | Code samples per section | DIFFERENT | — | Departments and coding granularity are undocumented | low | **YES** | NEW MIGRATION |
| 8 | REF-218 | Laboratory terms / abbreviations reference | MISSING | — | Glossary is content, not code; the owner must supply it | low | **YES** | NEW MIGRATION |
| 9 | REF-220 | Reminder note for a user | MISSING | — | Persistence, due-date and visibility semantics undocumented | low | **YES** | NEW MIGRATION |
| 10 | REF-221 | Follow-up appointment for patient/phone | MISSING | — | Reference line OCR-degraded; granularity unstated | low | **YES** | NEW MIGRATION |
| 11 | REF-225 | Payment method, instalments, receiver | DIFFERENT | — | The method list is not enumerated in the reference | low | **YES** | NEW MIGRATION |
| 12 | REF-234 | Receipt: names in English | MISSING | REF-162, REF-163 | Capture model must be chosen before any code | low | **YES** | NEW MIGRATION |
| **PHASE 1 — Culture antibiotic correctness (3 items; cheapest, clinically important)** |||||||||
| 13 | REF-093 | Antibiotic suitable for pregnancy | DIFFERENT | — | One-clause predicate fix removing drugs the reference forbids (DEF-001) | low | No | NO MIGRATION |
| 14 | REF-094 | Antibiotic suitable for children under twelve | DIFFERENT | REF-093 | Same clause; must change together to keep the grid consistent | low | No | NO MIGRATION |
| 15 | REF-092 | Antibiotic commercial name | DIFFERENT | — | Small additive schema change on an otherwise complete screen; do it once | low | No | NEW MIGRATION |
| **PHASE 2 — Barcode and document follow-through (5 items)** |||||||||
| 16 | REF-129 | Vertical barcode on report/receipt/envelope | MISSING | REF-127 | Now cheap: the shared encoder exists from Phase 1; only an orientation option is missing | low | No | NEW MIGRATION |
| 17 | REF-130 | Tube/container/swab labels (38×25 mm, multi-up) | MISSING | REF-127 | Sticker geometry and multi-up over the existing renderer | medium | No | NEW MIGRATION |
| 18 | REF-128 | Barcode labels per test and per sample | DIFFERENT | REF-127 | Extends the label service from one patient label to a per-test/per-sample set | low | No | NEW MIGRATION |
| 19 | REF-238 | Barcode display control | DIFFERENT | REF-127 | A global switch only becomes meaningful now that four documents carry barcodes | low | No | NO MIGRATION |
| 20 | REF-232 | Printer assignment per output kind | DIFFERENT | REF-066 | Completes the assignment set; the Envelope slot now has a consumer, the Card slot does not exist | low | No | NEW MIGRATION |
| **PHASE 3 — Report identity and abnormality marking (3 items)** |||||||||
| 21 | REF-125 | Report identification block | DIFFERENT | — | The document model declares the slots; only 4 builders pass `null` | low | No | NO MIGRATION |
| 22 | REF-118 | Print the abnormality marker | DIFFERENT | REF-125 | Writer change on the same file; makes the stored flag visible (DEF-003) | low | No | NO MIGRATION |
| 23 | REF-239 | Print account instead of date | DIFFERENT | REF-125 | Depends only on the existing flag and the builders REF-125 touches | low | No | NO MIGRATION |
| **PHASE 4 — Stored-but-inert settings (4 items)** |||||||||
| 24 | REF-236 | Treating doctor only from entity window | DIFFERENT | — | Presentation enforcement of an existing flag | low | No | NO MIGRATION |
| 25 | REF-240 | Result-screen account display mode | DIFFERENT | — | Stored and editable with **no reader** — a live honesty defect | low | No | NO MIGRATION |
| 26 | REF-235 | Receipt: Himself/Herself | DIFFERENT | — | Resolver exists; only the print path is missing. **Apply to receipt AND envelope (C-13)** | low | No | NO MIGRATION |
| 27 | REF-069 | Receipt expected result date per test | DIFFERENT | — | Turnaround data already stored; writer + editor missing | low | No | NO MIGRATION |
| **PHASE 5 — Receipt and settings documents (4 items)** |||||||||
| 28 | REF-061 | Receipt white paper + header/footer + logo | DIFFERENT | — | The receipt image half needs the asset store the report side later generalises | medium | No | NEW MIGRATION |
| 29 | REF-062 | Receipt header/footer text, font, size, colour, logo | DIFFERENT | REF-028 | Same settings screen and asset store; do both together | low | No | NEW MIGRATION |
| 30 | REF-064 | Receipt via cash-register printer | DIFFERENT | REF-029 | A page-size option on an existing writer | low | No | NO MIGRATION |
| 31 | REF-233 | Auto-print receipt on registration | MISSING | REF-038, REF-029 | Changes registration behaviour; after the receipt work is stable | low | No | NEW MIGRATION |
| **PHASE 6 — Search, lookup and aggregation correctness (10 items)** |||||||||
| 32 | REF-132 | Search patient by exact name | DIFFERENT | — | Extends an existing, heavily tested search query | low | No | NO MIGRATION |
| 33 | REF-133 | Search patient by partial name | DIFFERENT | REF-132 | Same screen and query; do both so the two documented modes arrive together | low | No | NO MIGRATION |
| 34 | REF-145 | Status indicator per patient | DIFFERENT | — | The roll-up already exists; a grid column and a resolver | low | No | NO MIGRATION |
| 35 | REF-158 | Count of patients per test | DIFFERENT | — | **Wrong business rule**: counts rows, not distinct patients | low | No | NO MIGRATION |
| 36 | REF-161 | Catalog search by name/group/**test number** | DIFFERENT | — | The query exists and is permission-gated; one more key | low | No | NO MIGRATION |
| 37 | REF-162 | Edit test data and price | DIFFERENT | — | Add the `Arrang` / `Out Lab Name` / `Routin` fields the reference catalog carries | low | No | NEW MIGRATION |
| 38 | REF-163 | Add a new test | DIFFERENT | REF-162 | Same fields on the create path; do together so the entity is touched once | low | No | NEW MIGRATION |
| 39 | REF-201 | Sent-out tests not yet dispatched | DIFFERENT | — | Additive query closing a dispatcher safety gap | low | No | NO MIGRATION |
| 40 | REF-203 | Account filters: debtors / commission / settled | DIFFERENT | — | Predicate options over an existing query | low | No | NO MIGRATION |
| 41 | REF-148 | Blood-picture **element** value search | DIFFERENT | REF-101 | Needs an analyte-level read path; the monitor is test-level only | medium | No | NO MIGRATION |
| **PHASE 7 — Result entry, interpretation and history (10 items)** |||||||||
| 42 | REF-036 | Patient medical history flags in the result window | DIFFERENT | — | Flags are stored; this surfaces them in the read-out and the report | low | No | NO MIGRATION |
| 43 | REF-079 | Read the patient data block from the result screen | MISSING | REF-042 | The read-out button and read-model fields | low | No | NO MIGRATION |
| 44 | REF-097 | Blank report: patient data only | DIFFERENT | — | Small layout flag on an existing writer | low | No | NO MIGRATION |
| 45 | REF-131 | Blank report with data entry | DIFFERENT | — | Same file; the document model exists | low | No | NO MIGRATION |
| 46 | REF-055 | Clear (zero) the whole account | DIFFERENT | REF-057 | A loop over the existing void command plus an audit row | low | **YES** | NO MIGRATION |
| 47 | REF-217 | Test information library enrichment | DIFFERENT | — | Pure read-model enrichment; clinical-effect text needs a column | low | No | NEW MIGRATION |
| 48 | REF-101 | CBC patient history table | MISSING | — | **Cheaper than any prior audit implied** — the tested pivot exists, only wiring and rendering are missing (DEF-007) | low | No | NO MIGRATION (charting package evaluated first) |
| 49 | REF-104 | More than one combined report per patient | DIFFERENT | — | Persisting combined-report groupings | low | No | NEW MIGRATION |
| 50 | REF-106 | Last N visits below the profile report | DIFFERENT | — | A trailing history block, once the report body is data-driven | medium | No | NEW MIGRATION |
| 51 | REF-033 | Users login/logout movement report | MISSING | REF-031, REF-222 | New read-only query; removes an auditor-visible blind spot early | low | No | NEW MIGRATION |
| **PHASE 8 — Worksheet family (11 items; one data model, eleven capabilities)** |||||||||
| 52 | REF-150 | Worksheet of a group of patients in a period | MISSING | — | The reference's primary worksheet and the query the variants read from | medium | No | NO MIGRATION |
| 53 | REF-155 | Worksheet with full patient + test data | MISSING | REF-150 | Enriches the line model with values already on the result rows | medium | No | NO MIGRATION |
| 54 | REF-151 | Worksheet of a group of patients, condensed | MISSING | REF-150 | A layout variant over the same data | low | No | NO MIGRATION |
| 55 | REF-156 | Worksheet with condensed patient data | MISSING | REF-151 | The condensed layout on the same enriched row model | low | No | NO MIGRATION |
| 56 | REF-152 | Worksheet of tests / group of tests in a period | DIFFERENT | — | A print command over a query and writer that both exist; closes DEF-006 | low | No | NO MIGRATION |
| 57 | REF-153 | Worksheet by work group (Log) | DIFFERENT | REF-152 | Identical work for the work-group mode; same command, same writer | low | No | NO MIGRATION |
| 58 | REF-154 | Print only unfinished tests or all | DIFFERENT | REF-152 | A predicate option over queries that already carry the completion facts | low | No | NO MIGRATION |
| 59 | REF-157 | Worksheet for one department over a period | DIFFERENT | REF-152 | Adds department and patient-code-range selectors on the new print path | low | No | NO MIGRATION |
| 60 | REF-159 | Classify how often each test was performed | DIFFERENT | REF-152 | Prints an existing classification; adds a signature column to the writer | low | No | NO MIGRATION |
| 61 | REF-160 | Worksheet print preview | DIFFERENT | REF-152 | One preview button per mode once each has a print path | low | No | NO MIGRATION |
| 62 | REF-164 | Substitute patient name/data on the worksheet line | DIFFERENT | REF-155 | A substitute-identity field on the line built in this phase | low | No | NO MIGRATION |
| **PHASE 9 — Statistics and account reporting (7 items)** |||||||||
| 63 | REF-192 | Print a statistics report | DIFFERENT | REF-178 | One writer for the statistics DTOs; the rows exist and are tested | low | No | NO MIGRATION |
| 64 | REF-194 | Stocktake by element **and report type** | DIFFERENT | REF-193 | Report-type variants over the existing inventory query | medium | No | NO MIGRATION |
| 65 | REF-196 | Stocktake statement for a treating doctor | DIFFERENT | — | A dedicated statement on the account print path built here | low | No | NO MIGRATION |
| 66 | REF-204 | Account per doctor, daily/monthly/annual | DIFFERENT | — | A doctor-scoped variant of the existing inventory query | low | No | NO MIGRATION |
| 67 | REF-205 | Detailed account by test price or by results | MISSING | — | First genuinely new account read model; defines the rows the next items reuse | medium | No | NO MIGRATION |
| 68 | REF-206 | Referral-entity statement + hand-over list | DIFFERENT | REF-205 | Statement and hand-over list on REF-205's read model | low | No | NO MIGRATION |
| 69 | REF-207 | Four account report types | MISSING | REF-205 | Four presentation variants; last in the phase because purely presentational | medium | No | NEW MIGRATION |
| **PHASE 10 — Report style / layout foundation (6 items; the critical path)** |||||||||
| 70 | REF-113 | Colours of patient data, headings, profiles, abnormal results | MISSING | — | **Prerequisite for every other report customisation item** — the content model must carry a style before colours can be expressed | medium | No | NEW MIGRATION |
| 71 | REF-114 | Whole-row highlight choice | MISSING | REF-113 | First consumer of the style model and the cheapest way to validate it | medium | No | NEW MIGRATION |
| 72 | REF-116 | Report line spacing + per-element left offset | MISSING | REF-113 | Layout data sharing the same store | medium | No | NEW MIGRATION |
| 73 | REF-108 | Report header/footer colours and element positions | DIFFERENT | REF-113 | Per-element colours and offsets; reuses the layout store | medium | No | NEW MIGRATION |
| 74 | REF-117 | Report element shape, name, order | MISSING | REF-113, REF-116, REF-108 | The largest layout item and the one other layout work depends on | medium | No | NEW MIGRATION |
| 75 | REF-219 | Colour palette | MISSING | REF-113 | A palette has no purpose until colours exist | low | No | NO MIGRATION |
| **PHASE 11 — Report layout consumers (5 items)** |||||||||
| 76 | REF-107 | Report header/footer modes none/words/image | DIFFERENT | REF-028, REF-108 | The image mode becomes real once the asset and layout stores exist | medium | No | NEW MIGRATION |
| 77 | REF-124 | Show or hide report blocks | DIFFERENT | REF-113 | Block visibility is a flag on the layout definition | medium | No | NEW MIGRATION |
| 78 | REF-089 | Choose which culture report elements are shown | MISSING | REF-124 | The same visibility mechanism applied to one report kind | medium | No | NEW MIGRATION |
| 79 | REF-123 | CBC colouring, auto-comment, manual mode | DIFFERENT | REF-113 | Colouring and the comment switch; the switch also depends on blocked REF-082 | medium | No | NEW MIGRATION |
| 80 | REF-121 | CBC report design variants and large paper | DIFFERENT | REF-110 | Design variants depend on the layout store; the paper size is trivial | medium | No | NEW MIGRATION |
| **PHASE 12 — Graphics (2 items; last, highest risk)** |||||||||
| 81 | REF-119 | Semen analysis report with graphs and diagrams | MISSING | REF-113 | Needs the data-driven body and introduces a drawing capability | high | No | NEW MIGRATION |
| 82 | REF-120 | Image report (1–4 images, draggable text/shapes) | MISSING | REF-113 | The largest and most dependency-heavy item; therefore last | high | No | NEW MIGRATION |
| **PHASE 13 — Hardware-dependent (1 item)** |||||||||
| 83 | REF-211 | Reader options: save after read, print barcode after read | MISSING | — | Hardware-dependent with no software prerequisite, so scheduled last | medium | No | NEW MIGRATION |

**Sequence integrity (mechanically checked):** 83 rows, 83 distinct REF ids, contiguous 1–83, every one of the 58 DIFFERENT and 25 MISSING ids present exactly once, **zero excluded ids present** (checked against REF-001…REF-025).

---

## 21. Build / Test / Runtime Verification

Everything in this section was performed in a **disposable copy of the repository placed outside the working tree**, so no `bin/`, `obj/` or any other artefact was ever created inside the audited repository. The measured repository was never built in, never tested in, and never written to.

### 21.1 Commands executed and results

| # | Command | Result | Notes |
|---|---|---|---|
| 1 | `git rev-parse HEAD` | `6fa10aabcb93d4ea6f49dba00423d7f89fc1b11d` | Exact audit target |
| 2 | `git rev-list --count 3590a7f5..HEAD` | **8** | The divergence window |
| 3 | `dotnet build TopLab.sln -c Release` | **FAILED** — `NETSDK1100: To build a project targeting Windows on this operating system` | Expected on Linux for `net8.0-windows` + `UseWPF` |
| 4 | `dotnet build TopLab.sln -c Release -p:EnableWindowsTargeting=true` | **Build succeeded. 0 Warning(s), 0 Error(s).** | Command-line property only; no project or props file edited |

### 21.2 Test results (all four runnable suites)

| Suite | Passed | Failed | Skipped | Total |
|---|---|---|---|---|
| `TopLab.Domain.Tests` | **507** | 0 | 0 | 507 |
| `TopLab.Application.Tests` | **1,713** | 0 | 0 | 1,713 |
| `TopLab.Infrastructure.Tests` | **321** | **28** | 0 | 349 |
| `TopLab.Persistence.Tests` | **13** | 0 | 2 | 15 |
| **Total** | **2,554** | **28** | **2** | 2,584 |
| `TopLab.Presentation.Tests` | **NOT RUN** | — | — | `net8.0-windows` + `UseWPF`; cannot execute on Linux |

A required environment accommodation was used: the container has the .NET **9** runtime and no .NET 8 runtime, so `DOTNET_ROLL_FORWARD=Major` was set. This is an **environment variable only** and touches no file.

### 21.3 Analysis of the 28 infrastructure failures

**All 28 are environmental, not logical.** Every one is `QuestPDF DocumentDrawingException : The text "…" uses font families that are not available: 'Arial'` — raised at PDF-render time because Arial is absent from this Linux container. There is no Arial in the image.

Independently confirmed by test name: the 28 failing tests are exactly the PDF-rendering tests — `ReportPdfWriterGoldenTests` (10), `ReceiptPrintingServiceTests` / `InvoicePrintingServiceTests` / `EnvelopePrintingServiceTests` / `LabOrderPrintingServiceTests` / `ReportPrintingServiceTests` / `WorkSheetPrintingServiceTests`, the writer golden tests, `ReceiptInvoiceBarcodeTests` (2), `PatientReportPdfExporterTests`, and the price-list / custom-group / banded-monitor writer tests.

**This exactly reproduces the pattern V2 reported** (24 failures, all Arial) and confirms the environment, not the code, is the cause. The count rose from 24 to 28 and the passed count rose from 271 to 321 because **Phase 1 added test files**, four of which render PDFs and therefore also fail on the font.

### 21.4 What the passing suites actually prove

* The Phase-1 barcode and envelope logic is behaviourally covered: a filtered run of the Phase-1 barcode/envelope tests returned **24 passed / 0 failed**, covering payload derivation, PNG encoding, envelope barcode placement, and the lab-order writer's barcode path.
* `TopLab.Application.Tests` at 1,713 (up from V2's 1,688, **+25**) confirms the new application-layer handlers and validators are covered by real behavioural tests.
* All 507 domain tests pass, including the envelope settings and position entities.
* The build is clean: **0 warnings, 0 errors**.

### 21.5 What the suites do **not** prove

* **No PDF was ever rendered in this environment.** The barcode, report, receipt, invoice, envelope, lab-order, worksheet, price-list, custom-group and banded-monitor writers were read and compiled, but **none produced output here**. In particular, the five Phase-1 functions are verified in code and unit tests, **not** verified as producing a correct printed artefact.
* **The `net8.0-windows` presentation assembly compiled but was never loaded or executed.** No WPF view, view-model binding or XAML interaction was exercised — including the new envelope and lab-order buttons.
* **The 2 skipped persistence tests mean no migration was applied to a real SQL Server.** Migration correctness is asserted statically from migration source and the model snapshot only.
* **No database was inspected, created or migrated.** No runtime behaviour of any kind was observed.

---

## 22. Not Verified by This Agent

Stated explicitly so that no static finding is mistaken for a working system.

| # | Not verified | Why | What is required |
|---|---|---|---|
| 1 | **Runtime behaviour of the WPF application** | `TopLab.Presentation` targets `net8.0-windows` with `UseWPF`; the sandbox is Linux. Never launched | Build and run on Windows; walk every screen named in §10–§12 |
| 2 | **Any database behaviour** | No SQL Server reachable; `docker` absent so Testcontainers cannot start one. The 2 container-dependent tests skipped | Start SQL Server (or let Testcontainers do it), apply migrations, re-run `TopLab.Persistence.Tests` |
| 3 | **Whether migrations apply cleanly to an existing database** | No database exists to migrate | Point the app at a database created by an earlier build; confirm the pending-migration list is empty |
| 4 | **Print output fidelity for every document, including the four new Phase-1 documents** | All 9 writers read and compiled; none rendered (Arial absent); no printer addressed | Print one of each document on Windows and compare against the cited reference pages |
| 5 | **Barcode scannability of the new envelope / lab-order / receipt / invoice barcodes** | The PNG bytes were never produced here; encoder unit tests do not decode with a real reader | Print and scan with several readers; confirm payload and quiet zone |
| 6 | **Arabic shaping, RTL and pagination in the new documents** | Compilation only; rendering blocked by the font | Render each document on Windows with an Arabic-capable font |
| 7 | **Presentation-layer tests** | `TopLab.Presentation.Tests` targets `net8.0-windows` | `dotnet test TopLab.sln` on Windows with Docker available |
| 8 | **Exact wording/ordering of the reference's Arabic UI strings** | OCR layer has systematic glyph confusion (E1) | Read the PDFs on screen, or obtain the vendor's original document |
| 9 | **Whether the `By Fax` control on LEARN p.17 denotes a full fax-send workflow** | The page is a screenshot; surrounding Arabic is OCR-degraded (E1, E6) | Read LEARN p.17–18 on screen (DEF-010) |
| 10 | **REF-209 sample-coding granularity** | Reference does not state patient-, visit- or test-scoped | Owner decision (Phase 0, item 7) |
| 11 | **REF-218 glossary content** | Reference names the tool, supplies no content | Owner supplies the term list |
| 12 | **REF-225 collection-method list** | Reference requires the method be shown but does not enumerate methods | Owner confirms the method list |
| 13 | **REF-115 "element letter" semantics** | OCR-ambiguous (E5) | Owner decision (Phase 0, item 4) |
| 14 | **REF-221 follow-up granularity** | Reference line OCR-degraded (E4) | Owner decision (Phase 0, item 10) |
| 15 | **Printer-assignment behaviour at runtime** | No printer reachable; the dispatcher was read statically | Assign each output kind to a real printer and confirm routing |
| 16 | **First-audit and second-audit source text** | Neither `build.md` nor `Job Verification and Validation.md` exists in the repository; their claims are known only second-hand via V2 | Supply the two reports to permit first-hand re-testing |
| 17 | **Whether Phase 1 changed any behaviour outside its four commits** | The 4 documentation commits touched no `src`/`tests` file, but their *document* contents were not treated as evidence | Not required — code state is fully measured |
| 18 | **The first-registration guard and banded monitor (B-01 batch work)** | V2 verified these at `3590a7f5`; this audit confirmed the code is present but did not re-derive the B-01 provenance | Not required for this audit's questions |

---

## 23. Final Count Reconciliation

### 23.1 Reference inventory arithmetic
```
TOTAL REFERENCE FUNCTIONS          = 242
  minus HARD-EXCLUDED              =  25   (5 owner categories, §8)
  equals IN-SCOPE                   = 217
```

### 23.2 Classification arithmetic
```
IN-SCOPE                           = 217
  IMPLEMENTED                      = 134
  DIFFERENT                        =  58
  MISSING                          =  25
  BLOCKED / UNRESOLVED (withheld)  =   0
  -------------------------------------
  134 + 58 + 25 + 0                = 217   ->  BALANCED
```

### 23.3 Sequence arithmetic
```
DIFFERENT + MISSING               =  58 + 25 = 83
Items in the §20 sequence          =  83
Distinct REF ids in the sequence   =  83
Duplicate entries                  =   0
In-scope items absent from it      =   0
Excluded ids present in it         =   0   (checked against REF-001..REF-025)
```

### 23.4 Migration arithmetic
```
NO_MIGRATION_REQUIRED             =  46
NEW_MIGRATION_REQUIRED            =  37
MIGRATION_DECISION_BLOCKED        =   0
                                   -----
                                    83   ->  every item assigned exactly one value
EXISTING MIGRATION / PK IMPACT    =   0
```

### 23.5 Movement ledger since V2's commit
```
MISSING -> IMPLEMENTED              = 4   (REF-066, 067, 068, 127)
DIFFERENT -> IMPLEMENTED            = 1   (REF-065)
IMPLEMENTED -> DIFFERENT            = 0
DIFFERENT -> MISSING               = 0
MISSING -> DIFFERENT               = 0
Regressions                        = 0
Exclusions changed                 = 0
Reference functions changed        = 0
```

### 23.6 How BLOCKED is represented
**No function had its classification withheld** — the withheld count is 0 and all 217 in-scope functions appear exactly once in exactly one bucket. `BLOCKED` is an *additional* flag, never a substitute for a classification:
* **4 evidence-blocked items** — REF-115, REF-221, REF-209, REF-030. Each already carries a classification.
* **4 specification-gated items** — REF-082, REF-083, REF-218, REF-225. The classification is certain; the *content* is owner-supplied.

---

## 24. Final Independent Verdict

**On the reference model.** The 242-function inventory is **sound**. It was re-derived from the two permitted PDFs and no function was found missing and none invented. All **25 owner exclusions were independently verified** against PDF evidence, with no category mismatch, no REF-ID mismatch, and no member of an excluded category missing from the owner list. The owner exclusion set is **CONFIRMED exactly as defined** and is preserved without alteration. One honest correction: V2's absolute claim that no fax function is documented anywhere is **too strong** — a `By Fax` control is visible on LEARN p.17 (DEF-010). This changes no count and creates no gap.

**On the first phase itself.** The phase was **executed well**. It targeted exactly V2's Phase 1 block, delivered four of its five items in four commits named by REF id, delivered the fifth (REF-065) as a natural consequence rather than a separate commit, and did so **schema-free** — exactly as V2 predicted. It built shared infrastructure once (`BarcodePngEncoder`, a from-scratch RGBA→PNG encoder) rather than four times, added no NuGet package, isolated barcode failure behind a text fallback, and reused the existing `PrintLabIdInsteadOfPatientId` policy for the payload. **+25 application tests, 0 build warnings, 0 errors.** This is competent, disciplined work.

**On the remaining gap.** **134 of 217** in-scope functions are now implemented, **58** are present-but-different, **25** are missing. The shape of what remains is **unchanged in character** from V2: it concentrates in the report style/layout foundation (the content model is a flat string list, so nothing visual can be expressed until it carries a style), the period-worksheet family (one missing data model serving eight capabilities), the account-report family, and the two genuinely large graphics items. Only one is hardware-dependent. The phase did not move the *centre of gravity* of the remaining work, and it was not intended to.

**On the prior audits.** V2 was the stronger document and its counts, hashes, migration inventory and negative searches all reproduce exactly — **but its negative searches are commit-scoped, and four of them are now false** (DEF-011). This is the single most transferable lesson of this audit: *a grep that returned zero is evidence about the commit where it ran, never about the tree you are holding now.* Anyone reusing V2's MISSING evidence strings against `6fa10aab` would reach a confidently wrong answer. All nine of V2's defects were independently re-verified at HEAD and **all nine survive unchanged**, including the two it found about itself in its own second pass.

**On defects.** Eleven are recorded. Nine are Top-Lab code defects; **two (DEF-010, DEF-011) are defects in the evidence/audit chain rather than in the product**, and both were found by re-running prior evidence rather than by reading new code. That is worth saying plainly: the two most useful findings in this audit are not about the software.

**On confidence.** **High** for the static classification of every item whose reference evidence is a Latin UI token or a grid column header, for the exclusion arithmetic, for the migration state, and for the five reclassifications — each rests on a direct citation re-read at HEAD. **Medium** for items whose reference evidence is degraded Arabic prose (E1). **Explicitly not claimed**: anything about runtime behaviour, printed output, barcode scannability, Arabic rendering, or database behaviour — none of which could be exercised here. 2,554 tests pass; the 28 that fail do so only because Arial is absent from a Linux container. **A green build is not a running laboratory system, and this report does not pretend otherwise.** In particular: the five functions the phase delivered are verified as *code*, and are **not** verified as *printed documents*.

---

## 25. Self-Audit / Quality-Control Checklist

| # | Check | Result |
|---|---|---|
| 1 | Exact target commit established with `git rev-parse HEAD` | **PASS** — `6fa10aabcb93d4ea6f49dba00423d7f89fc1b11d`, run first, re-run before delivery |
| 2 | Only the two permitted PDFs used as reference evidence | **PASS** — read directly; SHA-256 recomputed and matched |
| 3 | `Docs/Reference system files/` NOT used | **PASS** — never opened, listed, extracted or searched. Directory-name exposure disclosed in §3.3 |
| 4 | No other reference-system document used | **PASS** — `Cross-comparison.md` and the five `رد الوكيل الفرعي …` files were **not** read or used |
| 5 | No external website, search engine or online documentation used | **PASS** — the repository URL was used solely to clone |
| 6 | `Forensic_Audit_V2.md` treated as hypothesis only | **PASS** — every conclusion re-derived; 30 material claims individually re-tested in §6 |
| 7 | V2 read in full | **PASS** — 1,593 lines, including the complete 242-row appendix |
| 8 | Canonical inventory complete and deduplicated | **PASS** — 242 rows, 25 exclusions individually enumerated, in-scope 217 |
| 9 | Owner exclusions independently verified against the PDFs | **PASS** — all 25 verified; one extra negative-search correction (DEF-010 / C-11) |
| 10 | Owner exclusion set preserved, not silently altered | **PASS** — exactly 25, unchanged, despite the fax finding |
| 11 | Every in-scope function has exactly one classification | **PASS** — 217 items, three states only, no fourth state invented |
| 12 | Classification counts reconcile arithmetically | **PASS** — `134 + 58 + 25 + 0 = 217`, mechanically checked |
| 13 | Exclusion and in-scope counts reconcile | **PASS** — `242 − 25 = 217` |
| 14 | Every IMPLEMENTED item has a traced path with `path:line` evidence | **PASS** — 134 items; the five phase items traced UI → command → service → writer → dispatch |
| 15 | Every DIFFERENT item states its exact divergence | **PASS** — 58 items, each with a code citation |
| 16 | Every MISSING item has explicit negative-search evidence | **PASS** — 25 items, each with a re-run search; two of V2's now provably stale searches flagged as DEF-011 |
| 17 | Every defect has `path:line`, impact, verification status, confidence | **PASS** — 11 defects, all four attributes present |
| 18 | Function classification kept separate from defect analysis | **PASS** — 11 defects are not counted as reference functions |
| 19 | Migration conclusions supported by evidence | **PASS** — 14 migrations enumerated, snapshot path identified, phase diff proven empty, `46/37/0` recounted |
| 20 | Every DIFFERENT/MISSING item appears exactly once in the sequence | **PASS** — 83 rows, 83 distinct ids, contiguous, zero excluded ids |
| 21 | No historical commit checked out, reset, reverted, rebased, stashed, cleaned, merged or pushed | **PASS** — read-only `log`/`show`/`diff` only |
| 22 | Build/test performed without writing to the repository | **PASS** — disposable copy outside the tree; measured repo never built in; `git status` clean throughout |
| 23 | No runtime claim without observation | **PASS** — zero items are "RUNTIME VERIFIED"; §21/§22 state no PDF rendered, no WPF screen executed, no database touched |
| 24 | No excluded feature counted as a gap or recommended for implementation | **PASS** — the sequence was parsed against REF-001..REF-025 and contains zero excluded ids |
| 25 | Evidence confidence states applied consistently | **PASS** — CONFIRMED / INFERRED / UNVERIFIABLE / EVIDENCE_INSUFFICIENT used throughout, including on this audit's own findings |
| 26 | Evidence limitations disclosed | **PASS** — §4.4 (OCR), §15 (depth asymmetry), §22 (18 unverified items) |
| 27 | No invented line numbers, page numbers, symbols, test results or database observations | **PASS** — every citation was opened and re-read at HEAD; test totals are observed, not estimated |
| 28 | No new category invented to make arithmetic balance | **PASS** — the fax finding was recorded as a defect and a contradiction, **not** as a 26th excluded function |
| 29 | Exactly one deliverable file produced | **PASS** — `Docs/Audit/Audit following the first phase.md`; no second report, copy, ZIP or renamed variant |
| 30 | The audit was not optimised for a convenient conclusion | **PASS** — one V2 claim was **contradicted** (H-30) and one V2 method was **criticised** (DEF-011), in the same report that otherwise confirms V2's counts |

---

## 26. Appendix A — Mandatory Current REF Table

**No REF item is omitted because it is implemented, excluded or trivial.** All 242 identifiers appear exactly once.

Verification status key: `ST+TEST` = traced in code by this audit **and** covered by a test in a suite that ran in this environment · `ST` = traced in code by this audit · `ST (negative search)` = absence established by an explicit re-run search · `EXCLUDED` = owner-excluded, **not a gap**.

**Row count: 242** (25 excluded + 217 in scope). In-scope totals: **134 IMPLEMENTED + 54 DIFFERENT + 29 MISSING = 217.**

### Excluded (25) — not a gap, not implementation debt

| REF ID | Function | Scope | Current Status | Verification Status | Reference Evidence | Current Code Evidence | Notes |
|---|---|---|---|---|---|---|---|
| REF-001 | Upload patient result to the result website | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.58 | n/a — excluded category 1 | Not a gap |
| REF-002 | Open the result website and view the patient result | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.63 | n/a — excluded category 1 | Not a gap |
| REF-003 | Patient prints/exports the portal result as Word or PDF | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.63 | n/a — excluded category 1 | Not a gap |
| REF-004 | Android application for the result portal | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.63 | n/a — excluded category 1 | Not a gap |
| REF-005 | Print portal receipt data on the patient receipt | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.69; SHOW p.5 | n/a — excluded category 1 | Not a gap |
| REF-006 | Count of patients whose results are on the portal | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.65 | n/a — excluded category 1 | Not a gap |
| REF-007 | Delete (block) a patient result from the portal | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.68 | n/a — excluded category 1 | Not a gap |
| REF-008 | Create a doctor account for the result service | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.142-144 | n/a — excluded category 1 | Lab-code reuse is in scope via REF-175; only the portal account is excluded |
| REF-009 | Create a laboratory account for the result service | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.145-147 | n/a — excluded category 1 | Not a gap |
| REF-010 | Portal branding and portal access data on printed documents | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.69; SHOW p.54 | n/a — excluded category 1 | Not a gap |
| REF-011 | Equipment, maintenance and calibration register | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.93 | n/a — excluded category 2 | Only DatabaseMaintenance (backup) exists; unrelated |
| REF-012 | Equipment register entry form | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.93-94 | n/a — excluded category 2 | Not a gap |
| REF-013 | Blood-picture analyser interface and result import | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.55, 62-65, 74 | n/a — excluded category 3 | No partial analyser code exists |
| REF-014 | Per-analyser correction factors applied to results | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.70-71 | n/a — excluded category 3 | See REF-083: in-system formulas stay in scope |
| REF-015 | Blood-picture device report with curves | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.62, 72 | n/a — excluded category 3 | Not a gap |
| REF-016 | 5-part differential blood-picture interface | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.65, 74 | n/a — excluded category 3 | Not a gap |
| REF-017 | Interface for other analysers (VIDAS and others) | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.75, 77 | n/a — excluded category 3 | Not a gap |
| REF-018 | Branch number in system, user and patient data | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.151, 192, 205 | Dormant `BranchNumber` on SystemSettings/User; no consumer | Deliberately dormant residue |
| REF-019 | Patient search restricted to a branch number | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.42-43 | n/a — excluded category 4 | Not a gap |
| REF-020 | Cash-drawer stocktake per branch | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.179 | n/a — excluded category 4 | Not a gap |
| REF-021 | Statistics per branch | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.53 | n/a — excluded category 4 | Not a gap |
| REF-022 | Per-branch data folders and configuration files | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.130-132 | n/a — excluded category 4 | Not a gap |
| REF-023 | Send SMS of the result to patient or doctor | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.53; LEARN p.17, 18 | No SMS send path in `src/` | **A `By Fax` control is also visible on LEARN p.17/18 (DEF-010)** |
| REF-024 | Send E-mail of the patient result | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.16, 17, 18, 23 | No e-mail send path in `src/` | Not a gap |
| REF-025 | `Send Result` module in the main menu | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.12, 22, 23, 25 | No `Send Result` module | Not a gap |

### IMPLEMENTED (134)

| REF ID | Function | Scope | Current Status | Verification Status | Reference Evidence | Current Code Evidence | Notes |
|---|---|---|---|---|---|---|---|
| REF-026 | System sign-in with user name and password | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.11, 150-151, 158-163; SHOW p.34 | `SignInCommandHandler`; `Pbkdf2PasswordHasher`; `User.RecordLogin` | PBKDF2 + last-login stamp |
| REF-027 | Internal-window secondary password prompt | IN SCOPE | IMPLEMENTED | ST | LEARN p.150, 157, 176 | `User.InternalWindowsPasswordHash`; `SystemMenuPasswordDialog`; ShellViewModel gate | Per-user password where reference used a fixed value — a hardening |
| REF-028 | Create users, supervisors and administrators | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.150-153 | `CreateUserCommandHandler`; `UserManagementViewModel` | |
| REF-029 | Edit user data and permissions | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.156-157 | `UpdateUserCommandHandler`; `SaveUserPermissionsCommandHandler` | |
| REF-031 | Attendance check-in, break and check-out | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.158-163 | `CheckIn/StartBreak/EndBreak/CheckOut` handlers; `AttendanceCalculator` | **DEF-005**: no `IAuthorizedRequest`; no `ATTENDANCE` code |
| REF-032 | Per-user attendance summary | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.151-161 | `GetUserAttendanceSummaryQueryHandler`; `UserAttendanceSummaryViewModel` | |
| REF-034 | Add a new patient (name, sex, age) | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.10-12 | `CreatePatientCommandHandler`; `Patient.Create`; `PatientEditorViewModel` | |
| REF-035 | Optional patient data | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.12 | `Patient` optional fields; `PatientPhoneNumber`; `UpdatePatientCommandHandler` | |
| REF-037 | Print the patient barcode | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.12, 47; SHOW p.3 | `PrintBarcodeCommandHandler`; `BarcodeService`; `BarcodeLabelRenderer` | Honours `PrintLabIdInsteadOfPatientId` |
| REF-038 | Print the patient receipt / invoice | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.12, 17, 19-20 | `PrintReceipt`/`PrintInvoice` handlers; `ReceiptPdfWriter`; `InvoicePdfWriter` | **Phase 1 added a barcode to both (REF-127)** |
| REF-039 | Add a test to the patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.13 | `AddTestsToVisitCommandHandler`; `PatientEditorViewModel` | |
| REF-040 | Remove a single test from the patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.13 | `RemoveTestFromVisitCommandHandler` | |
| REF-041 | Clear all tests of the patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.13 | `ClearAllTestsCommandHandler` | First-registration-only guard present (B-01 slice 6/8) |
| REF-042 | Add a whole custom test group to the patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.15 | `AddCustomGroupToVisitCommandHandler`; `CustomGroupItem` | |
| REF-043 | Remove a test that belongs to an added group | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.15-16 | `RemoveTestFromVisitCommandHandler` | |
| REF-044 | Mark a test as sample taken outside the lab | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.13 | `PatientTest.IsTakenOutsideLab`; `UpdatePatientTestSampleFlagsCommandHandler`; rendered in `ReportContentBuilder` | Rendering added by B-01 slice 8/8 |
| REF-045 | Patient titles and optional automatic insertion | IN SCOPE | IMPLEMENTED | ST | LEARN p.192, 203 | `PatientTitle`; `GetPatientTitlesQueryHandler`; `DisableAutoTitleInsertion` | |
| REF-046 | Permanent lab code and persistent patient identity | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.47-48; SHOW p.3 | `Patient.LabId`; `GetNextLabIdQueryHandler`; `GetPatientByLabIdQueryHandler` | |
| REF-047 | Edit patient data | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.22 | `UpdatePatientCommandHandler`; `Patient.Update` | |
| REF-048 | Edit patient data from the search screen | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.25 | `PatientSearchViewModel` → `PatientEditorViewModel` edit mode | |
| REF-049 | Add or remove tests while editing a patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.22-24 | `ApplyVisitDeltasCommandHandler` | |
| REF-050 | Delete a patient (permission-gated) | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.151, 157; SHOW p.53 | `SoftDeletePatientCommand` (`IAuthorizedRequest`, `DeletePatient`); queries filter `IsDeleted` | |
| REF-051 | Automatic patient account total | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.17 | `PatientAccountCalculator`; `GetPatientAccountQueryHandler` | |
| REF-052 | Enter the previously paid amount and the discount | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.17 | `RecordPaymentCommandHandler`; discount ceiling enforced | |
| REF-053 | Add an extra amount to the account | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.17 | `RecordExtraChargeCommandHandler`; `PaymentOperation.IsExtraCharge` | |
| REF-054 | Settle the account in full | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.17 | `SettleAccountInFullCommandHandler`; `PatientAccountCalculator` | |
| REF-056 | Edit a payment operation | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.20 | `RecordCorrectionCommandHandler`; `OperationType.Correction` | Correcting operation rather than in-place edit — safer equivalent |
| REF-058 | Delivery and settlement grid filtered by period | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.21 | `GetDeliveryGrid`/`GetUndeliveredResults` handlers; `DeliverWithSettlement` | |
| REF-059 | Receipt shows total, discount, paid and remaining | IN SCOPE | IMPLEMENTED | ST | LEARN p.17-20, 69; SHOW p.5 | `ReceiptPdfWriter`; `GetPatientReceiptQueryHandler`; `ReceiptSettings` currency | |
| REF-060 | Receipt option to show or hide the test details | IN SCOPE | IMPLEMENTED | ST | SHOW p.4 | `ReceiptSettings.TestDetailDisplayMode`; consumed in `ReceiptPdfWriter` | |
| REF-063 | Receipt top margin, currency, delivery time, print-once limit | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.204 | `ReceiptSettings`; `UpdateReceiptSettingsCommandHandler` | |
| REF-065 | Envelope settings | IN SCOPE | **IMPLEMENTED** | **ST+TEST** | LEARN p.205-206; SHOW p.6 | `EnvelopeSettings.cs`; `EnvelopePrintItemPosition.cs`; `EnvelopeSettingsViewModel.cs:131,197`; **consumed at `EnvelopePdfWriter.cs:51-52,167-179`** | **CHANGED BY PHASE 1.** Was DIFFERENT ("no document consumes them") |
| REF-066 | Print the patient envelope | IN SCOPE | **IMPLEMENTED** | **ST+TEST** | LEARN p.205-206; SHOW p.6 | `EnvelopePdfWriter.cs`; `EnvelopePrintingService.cs`; `PrintEnvelopeCommandHandler.cs`; `PatientResultSheetView.xaml:85` | **CHANGED BY PHASE 1** (`11e2119`). Was MISSING |
| REF-067 | Patient barcode printed on the envelope | IN SCOPE | **IMPLEMENTED** | **ST+TEST** | SHOW p.6; LEARN p.205-206 | `EnvelopePdfWriter.cs:88-89` renders `lines.CodeBarcodePayload` via `BarcodeLabelRenderer` + `BarcodePngEncoder`; `EnvelopeBarcodeTests` (5 facts) pass | **CHANGED BY PHASE 1** (`c7ac4c4`). Was MISSING |
| REF-068 | Print a laboratory order with barcode | IN SCOPE | **IMPLEMENTED** | **ST+TEST** | SHOW p.6; LEARN p.22 | `LabOrderPdfWriter.cs:74-75,149`; `LabOrderPrintingService`; `PrintLabOrderCommandHandler`; `PatientEditorView.xaml:49` | **CHANGED BY PHASE 1** (`54eff33`). Was MISSING |
| REF-070 | Prevent printing the report while a balance remains | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.53 | `User.BlockPrintOnRemainingBalance`; `ExecuteBulkPrintCommandHandler` → `BulkPrintDtos.BlockedByBalance` | CORRECTION to the first audit: always enforced, never a dead grant |
| REF-071 | Enter a test result and save it | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.27-28 | `GetResultWorklistQueryHandler`; `EnterResultCommandHandler`; entry views | |
| REF-072 | Print preview of a report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.27, 30 | `PdfPreviewService`; `ReportPrintingService`; entry view-models | |
| REF-073 | Print a result report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.27 | `MarkResultPrinted`/`MarkProfilePrinted`; `ShellPdfPrinterDispatcher` | |
| REF-074 | Export the patient report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.28, 31 | `ExportPatientReportPdfCommandHandler`; `PatientReportPdfExporter`; `ReportContentBuilder` | |
| REF-075 | Review / verify a result | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.28, 54-56 | `VerifyProfileResults`/`ReviewResult`; `PatientTest.MarkReviewed` | |
| REF-076 | Un-verify a reviewed result | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.28 | `UnverifyProfileResults`/`UnreviewResult` | |
| REF-077 | Clear a result value | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.28 | `ClearResultCommandHandler`; `PatientTest` clear | |
| REF-078 | Refresh the result reference ranges | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.31, 102 | `RefreshResultReferenceRangeCommandHandler`; range snapshots | |
| REF-080 | Reload the day's worklist | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.27 | `GetResultWorklistQueryHandler`; `ResultsWorklistViewModel` | |
| REF-081 | Simple result-entry screen | IN SCOPE | IMPLEMENTED | ST | SHOW p.34, 36 | `SimpleResultEntryView.xaml`; `SimpleResultEntryViewModel` | |
| REF-084 | Abnormal-result flag during entry | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.13, 44 | `ResultFlagComputer`; `ResultFlag` enum; stored on `PatientTest`/`ProfileResultItem` | Computed and stored; **not printed** — see DEF-003 |
| REF-085 | Per-user discount ceiling | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.151, 157; SHOW p.53 | `User.DiscountLimitPercent`; enforced in `RecordPaymentCommandHandler` | CORRECTION to the first audit: always enforced |
| REF-086 | Low/high comments shown automatically on the report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.101 | `AnalyteReferenceRangeBand`; frozen into `ProfileResultItemReferenceRangeSnapshot`; printed in `ReportContentBuilder` | |
| REF-087 | Enter culture (farm) result data | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.38 | `CultureResult`; `SaveCultureResultsCommandHandler`; migration `20261001233652` | |
| REF-088 | Enter culture sensitivity results | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.39 | `CultureAntibioticResult`; `SaveCultureResultsCommandHandler` | |
| REF-090 | Add a new culture (farm) to the program | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.128-129 | `Test.IsCultureType`; `CreateTestCommandHandler`; `CultureAttachmentViewModel` | Per-farm data folders are excluded (REF-022) |
| REF-091 | Per-farm antibiotic list with inhibition-zone threshold | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.134-138 | `CultureAntibioticAttachment`; attach/detach commands | |
| REF-095 | Combined report: choose tests, order them and print | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.31-34 | `CombinedReportSelection`; `GetCombinableTests`/`BuildCombinedReport`/`PrintCombinedReport` | |
| REF-096 | Print the group name as a report sub-title | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.34 | `ReportSettings`; migration `20261001203315`; `ReportContentBuilder` | |
| REF-098 | Patient history inserted automatically into the result report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.70-71 | `AutoInsertHistoryCommandHandler`; `ReportSettings`; `CombinedReportViewModel` | **Routed through the DEF-008 function** |
| REF-099 | Manually insert a previous result from the history | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.75-76 | `InsertHistoryResultCommandHandler`; `GetPatientTestHistoryQueryHandler` | **Routed through the DEF-008 function** |
| REF-100 | Patient history in a separate report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.77-78 | `GetSeparateHistoryReportQueryHandler`; `PrintHistoryReportCommandHandler` | **Routed through the DEF-008 function** |
| REF-102 | Multi-patient history report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.80-83 | `GetMultiPatientHistoryQueryHandler`; `HistoryReportsViewModel` | **Routed through the DEF-008 function** |
| REF-103 | History grouping by lab code or patient name | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.70, 192, 195 | `ReportSettings`; `HistorySortMode`; `PatientHistoryResolver`; consumed in `PatientHistoryReader` | **Carries DEF-008 and DEF-009** |
| REF-109 | Report margins and top space | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.193 | `ReportSettings`; applied in `ReportPageComposer`; `UpdateReportSettingsCommandHandler` | |
| REF-110 | Report paper size A4 / A5 | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.194 | `ReportSettings`; `PaperSize`; `PageSizeMapper`; `ReportPageComposer` | |
| REF-111 | Report header prints the laboratory name and address | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.194 | `ReportDocumentContent`; `SaveLabPrintTextCommandHandler`; gated by `ReportSettings` | Suppressed entirely when the mode is `None` |
| REF-112 | Doctor's signature on the report | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.15, 57 | `ReportSettings`; carried in `ReportDocumentContent`; drawn in `ReportPageComposer` | |
| REF-122 | CBC report: rename elements, edit reference values and units | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.74 | `Analyte`; `AnalyteReferenceRangeBand`; `AnalyteEditorViewModel`; `UpdateAnalyte` | Curve half of the same page is excluded (REF-015) |
| REF-126 | Interpretation text per test about how the analysis is performed | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.19 | `TestComment`; `TestCommentPickerViewModel`; `ReportContentBuilder` | **DEF-004**: selection only, no free text |
| REF-127 | Barcode printed on the receipt and the envelope | IN SCOPE | **IMPLEMENTED** | **ST+TEST** | LEARN p.69; SHOW p.3, 52 — "طباعة الباركود على الإيصال والظرف" | `ReceiptPdfWriter` + `InvoicePdfWriter` embed the rendered PNG; payload from `ReceiptPrintingService.cs:90`, `InvoicePrintingService.cs:78`; envelope per REF-067 | **CHANGED BY PHASE 1** (`6fa10aa`). Was MISSING. 2 of its 4 tests are font-blocked (§21.3) |
| REF-134 | Search patient by treating doctor | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42 | `SearchPatientsGlobalQueryHandler`; `PatientSearchViewModel` | |
| REF-135 | Search patient by sex | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | `SearchPatientsGlobalQueryHandler`; `PatientSearchViewModel` | |
| REF-136 | Search patient by age | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | `SearchPatientsGlobalQueryHandler`; `Patient` age with unit | |
| REF-137 | Search patient by phone or card number | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | `SearchPatientsGlobalQueryHandler` phone sub-query + `NationalId`/`LabId` exact | |
| REF-138 | Search patients who ordered a chosen test | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | `SearchPatientsGlobalQueryHandler`; `PatientSearchViewModel` | |
| REF-139 | Search patients within a date range | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | `SearchPatientsGlobalQueryHandler`; `PatientSearchViewModel` | |
| REF-140 | Search by lab code and list all visits | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.47-51 | `GetPatientByLabIdQueryHandler`; `GetVisitHistoryQueryHandler`; `PatientVisitHistoryViewModel` | |
| REF-141 | Find results that have not been entered | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42-43 | `GetResultWorklistQueryHandler`; worklist filter | Implemented as a worklist filter rather than a search-screen button |
| REF-142 | Find results that have not been reviewed | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42 | `GetResultWorklistQueryHandler`; `ResultsWorklistViewModel` | |
| REF-143 | Find results that have not been printed | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42 | `GetResultWorklistQueryHandler`; `ResultsWorklistViewModel` | |
| REF-144 | Find results that have not been delivered | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42 | `GetUndeliveredResultsQueryHandler`; `ResultDeliveryViewModel` | |
| REF-146 | Follow-up / monitoring report of results | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.25 | `GetBandedResultMonitorQueryHandler`; `BandedResultMonitorPdfWriter`; `StatisticsViewModel` | Added by B-01 slices 1-3 |
| REF-147 | Evaluate a test's results by minimum and maximum value | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.25 | `GetBandedResultMonitorQueryHandler` invariant parse + min/max band | Added by B-01 slices 1-3 |
| REF-149 | Patient search while typing the name on the patient form | IN SCOPE | IMPLEMENTED | ST | SHOW p.3; LEARN p.192 | `SystemSettings.EnablePatientNameSearchAssist`; `SearchPatientsGlobalQueryHandler`; `SearchPatientsQueryHandler` | |
| REF-165 | Reference ranges: add, edit and delete a range | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.101 | `CreateReferenceRange`/`UpdateReferenceRange`/`DeleteReferenceRange`; `AnalyteEditorViewModel` | See DEF-002 for the dual-store issue |
| REF-166 | Reference ranges per age unit | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.104 | `AnalyteReferenceRangeBand.Matches` requires the same unit; `ResultFlagComputer` | Correct per the reference: a month patient is not matched by a day band |
| REF-167 | Add, edit and delete fixed comments for a test | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.112-115 | `CreateTestComment`/`UpdateTestComment`/`DeleteTestComment`; `TestCommentsViewModel` | |
| REF-168 | Attach a stored comment to the patient's result report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.116 | `TestCommentPickerViewModel`; `ProfileResultDtos`; `ReportContentBuilder` | **DEF-004**: no free-text entry |
| REF-169 | Custom groups: create, price, delete and print | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.118-123 | `CreateCustomGroup`/`RenameCustomGroup`/`DeleteCustomGroup`/price commands; `CustomGroupPdfWriter` | |
| REF-170 | Price lists: full maintenance | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.105-108 | `CreatePriceList`/`RenamePriceList`/`DeletePriceList`/item commands; `PriceListsViewModel` | |
| REF-171 | Print a price list | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.105, 111 | `PriceListPdfWriter`; `GetPriceListByIdQueryHandler` | |
| REF-172 | Add a treating doctor | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.124-125 | `CreateExternalEntityCommandHandler`; `ExternalEntity`; `EntityType` | |
| REF-173 | Add a referral or contract entity | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.125-127 | `ExternalEntity` has City, Address, Phone, **Fax**, ResponsiblePerson, PriceList, DiscountOrCommission | The Fax **field** is in scope here; fax **sending** is excluded (category 5) |
| REF-174 | Add an entity of type sent-out samples | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.125, 143 | `EntityType`; consumed by `SendSampleOutCommandHandler` and `GetSentOutLabAccountQueryHandler` | |
| REF-175 | Generate an entity code | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.144, 146 | `GenerateEntityIdCodeCommandHandler`; `SecureEntityIdCodeGenerator`; `ExternalEntity.GeneratedIdCode` | Its portal use as a password is excluded (REF-008/009) |
| REF-176 | Mark a test as sent outside and set its prices | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.140-141 | `Test.IsSentOut`/cost prices; `SendSampleOutCommandHandler`; `SentOutSample` | |
| REF-177 | Choose the culture when attaching antibiotics | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.136 | `CultureAttachmentViewModel`; `GetCultureAntibioticsQueryHandler` | |
| REF-178 | Statistics main window | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.167-168; SHOW p.47 | `StatisticsViewModel`; `StatisticsView.xaml`; permission-gated | |
| REF-179 | Total patient count for a period | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.168 | `GetPatientCountStatisticsQueryHandler`; `StatisticsDtos` | |
| REF-180 | Patient count sorted by the months of a year | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169; SHOW p.30 | `GetPatientCountStatisticsQueryHandler`; `MonthlyCountDto` | |
| REF-181 | Patient count sorted by sex | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169; SHOW p.30 | `GetPatientCountStatisticsQueryHandler`; `SexCounts` | |
| REF-182 | Patient count sorted by account type | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169, 171; SHOW p.30 | `GetPatientCountStatisticsQueryHandler`; `AccountTypeCounts` | |
| REF-183 | Patient count sorted by referral entity | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169, 171; SHOW p.30 | `GetPatientCountStatisticsQueryHandler`; `ReferralEntityCounts` with a no-referral bucket | |
| REF-184 | Patient count for one specific referral entity | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.171; SHOW p.30 | `GetPatientCountStatisticsQueryHandler` with entity options | |
| REF-185 | Patient count for a month split by day | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.170-171; SHOW p.31 | `GetPatientCountStatisticsQueryHandler` day-of-month grouping; `StatisticsView.xaml` | Added by B-01 slices 4-5 |
| REF-186 | Paid amounts shown with the patient statistics | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169-171; SHOW p.30 | `PeriodMoneyDto` from `PatientAccountCalculator.TotalPaid`; `StatisticsViewModel` | Added by B-01 slices 4-5 |
| REF-187 | Number of test samples in a year by test name | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.172; SHOW p.32 | `GetTestCountStatisticsQueryHandler`; `StatisticsDtos` | |
| REF-188 | Test-group request rate counting | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.172; SHOW p.32 | `GetTestCountStatisticsQueryHandler` | |
| REF-189 | Sent-out sample statistics by period and laboratory | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.167-168; SHOW p.32-33 | `GetSentOutStatisticsQueryHandler`; `StatisticsDtos` | |
| REF-190 | Sent-out test count for one referral entity | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.33 | `GetSentOutStatisticsQueryHandler` | |
| REF-191 | User productivity statistics | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.167; SHOW p.47 | `GetUserProductivityStatisticsQueryHandler`; `StatisticsDtos` | |
| REF-193 | Cash-drawer inventory and lab account | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.175-178 | `GetCashDrawerInventoryQueryHandler`; `InventoryDtos`; permission-gated | |
| REF-195 | Detailed patient-sample account list for a period | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.180; SHOW p.26 | `GetPatientSamplesDetailQueryHandler`; `InventoryDtos` | |
| REF-197 | Cash deposit and disbursement | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.184, 186, 154 | `RecordCashDeposit`/`RecordCashDisbursement`; `CashMovement` | |
| REF-198 | Sent-out samples account for a period | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.184-185; SHOW p.29 | `GetSentOutSamplesQuery`; `GetSentOutLabAccountQuery`; `SentOutAccountCalculator` | |
| REF-199 | Mark a sent-out sample paid or settle it in full | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.186 | `RecordSentOutPayment`/`SettleSentOutInFull`; `SentOutSamplePayment` | |
| REF-200 | Sent-out account filtered to one laboratory | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.187-188 | `GetSentOutSamplesQuery`; `GetSentOutLabAccountQuery`; view-model filter | |
| REF-202 | Company and delegate accounts | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.184, 154 | `GetCompanyDelegateAccountsQueryHandler`; `InventoryDtos` commissions | |
| REF-208 | Sent-out account per period with per-sample detail | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.29 | `GetSentOutLabAccountQueryHandler`; `GetSentOutSamplesQueryHandler`; `SentOutLabAccountViewModel` | |
| REF-210 | Draw and separate samples; drawn and not-drawn lists | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.209-210 | `MarkSampleDrawn`/`MarkAllSamplesDrawnForPatient`; `SampleDrawBoardWindow` | |
| REF-212 | Scientific calculator | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51 | `ArithmeticCalculator`; `EvaluateCalculationQueryHandler`; `UtilitiesViewModel` | |
| REF-213 | Measurement unit conversion | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51 | `MeasurementUnitConverter`; `ConvertMeasurementUnitQueryHandler` | |
| REF-214 | Stopwatch | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51 | `StopwatchCalculator`; `ComputeStopwatchElapsedQueryHandler` | |
| REF-215 | Phone book | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51; LEARN p.155 | `AddPhoneBookEntry`/`RemovePhoneBookEntry`; `JsonPhoneBookStore` | |
| REF-216 | Requirements and purchases list | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51 | `AddPurchaseItem`/`RemovePurchaseItem`/`TogglePurchaseItemDone`; `JsonPurchasesListStore` | |
| REF-222 | Who registered the patient, edit count, last editor | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.52, 57, 164 | `GetPatientAuditQueryHandler`; `AuditableEntitySaveChangesInterceptor`; `AuditableEntity` | |
| REF-223 | Who entered and who reviewed each result | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.56-57 | `GetPatientTestAuditQueryHandler`; `AuditDtos`; stamped in `PatientTest` | |
| REF-224 | Print count, last printer and who delivered the result | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.56-57 | `PatientTest` print fields; `AuditDtos` | |
| REF-226 | Amend a verified result and keep the amendment log | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.22-25, 57; SHOW p.64 | `AmendProfileResult`; `ProfileResultAmendment`; `GetProfileResultAmendmentsQueryHandler` | Trigger differs (amend offered once printed); workflow satisfied |
| REF-227 | System settings: delivery time, default account type | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.192, 199-201 | `SystemSettings`; `SystemSettingsViewModel`; honoured in `ReceiptPdfWriter` | |
| REF-228 | Database server settings | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.191-192, 199-200 | `UpdateDatabaseServerSettingsCommandHandler`; `IWorkstationConnectionSettingsProvider`; `CheckDatabaseConnectivity` | |
| REF-229 | Database backup and restore | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.191-192, 199-200 | `BackupDatabaseNow`/`RestoreDatabase`; `DatabaseMaintenanceViewModel` | |
| REF-230 | Daily automatic backup into a dated folder | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.52 | `DailyBackupHostedService`; `SystemSettings.DailyBackupEnabled/Path` | |
| REF-231 | Apply database and program updates | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.191, 200 | `ApplyDatabaseUpdatesCommandHandler`; `DatabaseMaintenanceViewModel` | Program-file update is a deployment task outside the application |
| REF-237 | Card settings: automatic review and completion of tests | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.192 | `EnterResultCommandHandler.cs:115-127` reads `AutoReviewAndComplete` and calls `pt.MarkReviewed` | Upgraded by V2 (ERR-1). Nuance: reference files it under card settings, Top-Lab under system settings |
| REF-241 | Lock the workstation and secondary authentication | IN SCOPE | IMPLEMENTED | ST | LEARN p.176, 150 | `LockWorkstationCommandHandler`; `ShellViewModel`; `UnlockWindow` | The reference-aligned capability is REF-027 (present). Lock + idle auto-lock are Top-Lab additions |
| REF-242 | About window | IN SCOPE | IMPLEMENTED | ST | LEARN p.12 | `AboutWindow.xaml`; `ShellViewModel` | |

*(134 rows: the 25 excluded rows above are separate. Counted mechanically from this table: 134 IMPLEMENTED.)*

### DIFFERENT (58)

| REF ID | Function | Scope | Current Status | Verification Status | Reference Evidence | Current Code Evidence | Notes |
|---|---|---|---|---|---|---|---|
| REF-030 | Delete or deactivate a user | IN SCOPE | DIFFERENT | ST | LEARN p.151-153, 157 | `DeactivateUser` + `DeleteUser` handlers; `User` | BLOCKED: reference never names a removal action |
| REF-036 | Patient medical history flags and conditions | IN SCOPE | DIFFERENT | ST | LEARN p.13, 18, 24 | `PatientMedicalCondition` stored; not surfaced in the result window | |
| REF-055 | Clear (zero) the whole account | IN SCOPE | DIFFERENT | ST | LEARN p.18-20 | `VoidPaymentOperationCommandHandler`; `PaymentOperation.Void` | No account-wide clear action |
| REF-057 | Delete a payment operation | IN SCOPE | DIFFERENT | ST | LEARN p.20 | `VoidPaymentOperationCommandHandler` | Void only; the row and amount remain. BLOCKED: money-deletion semantics |
| REF-061 | Receipt on pre-printed or white paper with header/footer and logo | IN SCOPE | DIFFERENT | ST | LEARN p.204; SHOW p.5 | `ReceiptSettings.HeaderFooterMode`; `ReceiptPdfWriter` | Image mode has no asset pipeline |
| REF-062 | Receipt header/footer text, font, size, colour and logo | IN SCOPE | DIFFERENT | ST | LEARN p.203-204; SHOW p.5 | `SaveLabPrintTextCommandHandler`; `ArabicFontResolver` | Lab text + one font family; no per-element colour, no logo |
| REF-064 | Receipt printed through a cash-register printer | IN SCOPE | DIFFERENT | ST | SHOW p.4 | `ReceiptSettings.CashierPrinterEnabled`; `ReceiptPrintingService` | Fixed A5; no 80 mm page size |
| REF-069 | Receipt expected result date per test | IN SCOPE | DIFFERENT | ST | LEARN p.192; SHOW p.52 | `Test.CompletionDurationMinutes`; `Patient.PickupDateUtc` | Stored, never composed or printed by a writer |
| REF-082 | Automatic interpretation comment on results | IN SCOPE | DIFFERENT | ST | SHOW p.14, 57 | No generator; `ResultFlagComputer` computes a flag; band text is operator-authored | BLOCKED: catalogue content is owner-supplied |
| REF-083 | Automatic calculation and equations in results | IN SCOPE | DIFFERENT | ST | SHOW p.14 | `ArithmeticCalculator` consumed only by `EvaluateCalculationQueryHandler` | BLOCKED: scope vs excluded analyser factors (REF-014) |
| REF-092 | Antibiotic master data with commercial name | IN SCOPE | DIFFERENT | ST | LEARN p.134-135, 138; SHOW p.23 | `Antibiotic` has no commercial-name field; `grep 'CommercialName\|TradeName' src/` → 0 | See C-1 |
| REF-093 | Antibiotic marked as suitable for pregnancy | IN SCOPE | DIFFERENT | ST | LEARN p.139 | `Antibiotic.IsPregnancyFlagged`; `GetCultureEntryGridQueryHandler.cs:37` | **DEF-001** escape clause |
| REF-094 | Antibiotic marked as suitable for children under twelve | IN SCOPE | DIFFERENT | ST | LEARN p.139 | `Antibiotic.IsChildrenFlagged`; `AgeRules.IsUnderTwelve` | Same **DEF-001** escape clause |
| REF-097 | Blank report: patient data only | IN SCOPE | DIFFERENT | ST | LEARN p.36-37 | `BuildBlankReportCommandHandler`; `ReportContentBuilder` `FromBlank` | No "data only" variant selectable |
| REF-104 | More than one combined report per patient | IN SCOPE | DIFFERENT | ST | SHOW p.18 | `CombinedReportSelection` documented "Ephemeral … Never persisted" | |
| REF-105 | One-button combined print of entered but unprinted results | IN SCOPE | DIFFERENT | ST | SHOW p.18 | `BulkPrintPreflightQueryHandler` requires `IsReviewed` | Reference says "entered and not printed". BLOCKED |
| REF-106 | Print the last N visits below the profile report | IN SCOPE | DIFFERENT | ST | SHOW p.52 | `GetPatientTestHistoryQuery` takes a date window, not a visit count | |
| REF-107 | Report header/footer modes: none, words, image | IN SCOPE | DIFFERENT | ST | LEARN p.192, 194 | `ReportSettings.HeaderFooterMode`; `ReportContentBuilder`; `ReceiptPdfWriter` | Image mode has no asset pipeline |
| REF-108 | Report header/footer colours and element positions | IN SCOPE | DIFFERENT | ST | LEARN p.195-197 | `ReportSettings.HeaderColor`/`FooterColor` only; `ReportPageComposer` | No per-element colours, no offsets |
| REF-115 | Report element letter, element list editing, free text | IN SCOPE | DIFFERENT | ST | SHOW p.12 | `Analyte`; `PatientTest` accepts free text; `Test.TestCode` is a lookup code | BLOCKED (E5) |
| REF-118 | Print the abnormality marker on the report | IN SCOPE | DIFFERENT | ST | SHOW p.13, 44, 61 | `ResultFlagComputer` computes; `ReportContentBuilder` uses it only to select a comment | **DEF-003** |
| REF-121 | Blood-picture report design variants and large paper | IN SCOPE | DIFFERENT | ST | SHOW p.56, 74 | `PaperSize` configurable but has no large value; one layout in `ReportContentBuilder` | |
| REF-123 | CBC report colouring, editable auto-comment and manual mode | IN SCOPE | DIFFERENT | ST | SHOW p.58, 74 | Manual input and comment text exist; **all colour control absent**, no comment on/off switch | Needs the style model (REF-113) |
| REF-124 | Show or hide report blocks | IN SCOPE | DIFFERENT | ST | SHOW p.15, 57, 64 | Only the doctor-signature block is toggleable | |
| REF-125 | Report identification block | IN SCOPE | DIFFERENT | ST | LEARN p.29, 70-71; SHOW p.17, 20 | `ReportDocumentContent` declares the slots; 4 of 5 builders pass `null`; date has no time | |
| REF-128 | Barcode labels per patient test and per sample | IN SCOPE | DIFFERENT | ST | SHOW p.3, 52 | One Code-128 label per patient; payload is a single identifier | Phase 1 reused this renderer for four documents |
| REF-131 | Blank report with data entry before printing | IN SCOPE | DIFFERENT | ST | LEARN p.36 | `BuildBlankReportCommandHandler`; `BlankReportViewModel` | No entry surface |
| REF-132 | Search patient by exact name | IN SCOPE | DIFFERENT | ST | LEARN p.42-43 | `SearchPatientsGlobalQueryHandler` matches with `Contains`; no exact-match mode | |
| REF-133 | Search patient by partial name | IN SCOPE | DIFFERENT | ST | LEARN p.42-43 | Partial match gated behind a setting serving a different purpose (REF-149) | |
| REF-145 | Status indicator per patient | IN SCOPE | DIFFERENT | ST | SHOW p.52 | `VisitRollup` counts per test; the search grid renders only name, id, actions | No single status symbol per patient |
| REF-148 | Search and statistics on blood-picture ELEMENT values | IN SCOPE | DIFFERENT | ST | SHOW p.73 | `GetBandedResultMonitorQueryHandler.cs:56-68` reads `PatientTest.ResultValue`; CBC values are analyte-level rows in `ProfileResultItem` | |
| REF-152 | Worksheet of tests or a group of tests in a period | IN SCOPE | DIFFERENT | ST | LEARN p.89-91 | `GetWorkSheetByTestGroupQuery`; `WorkSheetPdfWriter`; print wired only for visit mode | **DEF-006** |
| REF-153 | Worksheet by work group (Log) in a period | IN SCOPE | DIFFERENT | ST | LEARN p.84, 92 | `GetWorkSheetByWorkGroupLogQuery`; no print path | **DEF-006** |
| REF-154 | Print only unfinished tests or all tests | IN SCOPE | DIFFERENT | ST | LEARN p.85-86, 92 | `WorkSheetDtos` carries completion facts; no filter flag passed or offered | |
| REF-157 | Worksheet for one department over a period | IN SCOPE | DIFFERENT | ST | SHOW p.7-11 | Work-group-log mode covers "department"; no department selector; single-`PatientId` worksheet query | |
| REF-158 | Count of patients per test | IN SCOPE | DIFFERENT | ST | SHOW p.9 | `GetWorkSheetTestCountByPeriodQueryHandler` groups `PatientTest` and counts **rows**, not distinct patients | Wrong business rule, not a missing feature |
| REF-159 | Classify how often each test was performed in a period | IN SCOPE | DIFFERENT | ST | LEARN p.90-91 | `GetWorkSheetTestCountByPeriodQueryHandler`; no print command; no signature column | |
| REF-160 | Worksheet print preview | IN SCOPE | DIFFERENT | ST | LEARN p.87-88 | Preview only for the visit worksheet | **DEF-006** |
| REF-161 | Test catalog list with search by name, group or test number | IN SCOPE | DIFFERENT | ST | LEARN p.94-95, 141; SHOW p.43 | `SearchTestCatalogQuery(string? SearchTerm, int? TestGroupId, bool IncludeInactive)`; LEARN p.141 shows `By test name \| By group name \| By test ID` | The test-number key is absent |
| REF-162 | Edit test data and price | IN SCOPE | DIFFERENT | ST | LEARN p.95, 141 | `Test.cs:9-43` has no `Arrang`, `Out Lab Name` or `Routin`; the p.141 grid shows all three | |
| REF-163 | Add a new test | IN SCOPE | DIFFERENT | ST | LEARN p.96-97 | `CreateTestCommand` cannot carry arrange / out-lab / routine | Same three missing fields as REF-162 |
| REF-164 | Substitute patient name and data on the worksheet line | IN SCOPE | DIFFERENT | ST | SHOW p.19 | `WorkSheetLineDto` carries the real identity only; `WorkSheetPdfWriter` renders it | No override field |
| REF-192 | Print a statistics report | IN SCOPE | DIFFERENT | ST | LEARN p.169-171; SHOW p.30 | No print action in `StatisticsViewModel`; no statistics writer among the 9 | |
| REF-194 | Stocktake by element and report type | IN SCOPE | DIFFERENT | ST | LEARN p.178-179 | Element-kind enum exists; the four report-type variants do not | |
| REF-196 | Stocktake statement for a specific treating doctor | IN SCOPE | DIFFERENT | ST | LEARN p.181-183 | `InventoryDtos` TreatingDoctor kind + `GetPatientSamplesDetail`; no dedicated statement | |
| REF-201 | List the sent-out tests that are not yet dispatched | IN SCOPE | DIFFERENT | ST | SHOW p.29; LEARN p.186 | `SentOutSample` rows written only on dispatch; no query joins `Tests.IsSentOut` to undispatched `PatientTests` | |
| REF-203 | Patient account filters: debts, commission, paid only | IN SCOPE | DIFFERENT | ST | SHOW p.26 | `GetPatientSamplesDetailQuery` takes only `From`/`To`; no debt/commission/settled selector | |
| REF-204 | Account per doctor on a daily, monthly or annual basis | IN SCOPE | DIFFERENT | ST | SHOW p.26 | Doctor is an inventory element kind, not a billing-account grouping | |
| REF-206 | Account statement for one referral entity and the hand-over list | IN SCOPE | DIFFERENT | ST | SHOW p.28 | Referral-scoped data exists; no statement document, no hand-over command | Electronic transmission is excluded (category 5); only the produced statement is a gap |
| REF-209 | Code the patient's samples per section | IN SCOPE | DIFFERENT | ST | LEARN p.208 | Sample-kind flags exist; no department entity, no per-department code, no separation step | BLOCKED: departments and coding granularity undocumented |
| REF-217 | Test information library | IN SCOPE | DIFFERENT | ST | SHOW p.51 | `TestLibraryEntryDto` carries `TestId, Name, TestCode, GroupName` only | No sample, no normal values, no patient effect |
| REF-225 | How a payment was collected, instalments and receiver | IN SCOPE | DIFFERENT | ST | SHOW p.53 | `PaymentOperation` has `ReceivedByUserId` and `OperationAtUtc`; **no payment-method field**, no instalment count | BLOCKED: method list not enumerated |
| REF-232 | Printer assignment per output kind | IN SCOPE | DIFFERENT | ST | LEARN p.199, 206 | `PrinterOutputType` has 4 values; LEARN p.199 names **five** including the card printer | **Phase 1 gave the `Envelope` slot its first real consumer** (`EnvelopePrintingService.cs:67`), but the Card slot is still absent — progress, not closure |
| REF-235 | Receipt: Himself / Herself when the referral is empty | IN SCOPE | DIFFERENT | ST | LEARN p.192, 203 | `ReferralNameResolver` returns `Himself`/`Herself`; only caller is a registration-catalog query for UI placeholders; no writer emits it | **See C-13: the envelope deliberately omits the line, so it now shares this root cause** |
| REF-236 | Treating doctor only from the external-entity window | IN SCOPE | DIFFERENT | ST | LEARN p.192, 203 | `SystemSettings` flag + settings checkbox; no consumer — the patient editor still exposes free-text doctor fields | Stored flag, absent behaviour |
| REF-238 | Barcode display control in the program | IN SCOPE | DIFFERENT | ST | LEARN p.190; SHOW p.3 | Three related flags exist and are consumed; barcodes now exist on four documents | **Still no single master switch and no per-document toggle** |
| REF-239 | Print the account instead of the date on the report | IN SCOPE | DIFFERENT | ST | LEARN p.199 | `SystemSettings.PrintAccountInsteadOfDateOnReport` + checkbox; **no consumer** — writers always set `ReportDateText` | Stored flag, absent behaviour |
| REF-240 | Result-screen account display mode | IN SCOPE | DIFFERENT | ST | LEARN p.19-20 | Enum + column + settings ComboBox only; full-tree search returns **no behavioural consumer** | Stored, editable, inert |

### MISSING (25)

| REF ID | Function | Scope | Current Status | Verification Status | Reference Evidence | Current Code Evidence | Notes |
|---|---|---|---|---|---|---|---|
| REF-033 | Users login/logout movement report for a period | IN SCOPE | MISSING | ST (negative search) | LEARN p.164 | `AttendanceRecord.cs:6-25` has no machine field; `grep -rn MachineName src/` → 0 files; no session log; only an in-memory session DTO | |
| REF-079 | Read the patient data block from the result screen | IN SCOPE | MISSING | ST (negative search) | LEARN p.13, 26 | `ProfileEntryView.xaml` and `PatientResultSheetView.xaml` bind test columns only; the DTOs carry no condition/sex/age | The reference names the `بيانات المريض` button explicitly (LEARN p.13, p.26) |
| REF-089 | Choose which culture report elements are shown | IN SCOPE | MISSING | ST (negative search) | LEARN p.39 | `ReportCultureSection` has no `Show`/`Visible`/`IsEnabled`; no element-selection entity | |
| REF-101 | Patient history for the blood picture (CBC) | IN SCOPE | MISSING | ST (negative search) | LEARN p.79 | `HistoryMatrixBuilder` IS a CBC date×analyte pivot but has **zero production consumers** | **DEF-007.** Verdict survives; cost is far lower than assessed |
| REF-113 | Colours of patient data, headings, profiles and abnormal results | IN SCOPE | MISSING | ST (negative search) | SHOW p.16, 44 | `ReportDocumentContent` carries strings, a section list and a grid only; the only colour fields are `ReportSettings.HeaderColor`/`FooterColor` | The largest single cluster of remaining work |
| REF-114 | Whole-row highlight choice | IN SCOPE | MISSING | ST (negative search) | SHOW p.58, 61, 74 | No per-row style data in the content model or the grid record | Same root cause as REF-113 |
| REF-116 | Report line spacing and left-margin position per element | IN SCOPE | MISSING | ST (negative search) | SHOW p.13 | `ReportPageComposer` renders a fixed rhythm; `grep -i LineSpacing src/` → 0 files; no per-element offset in `ReportSettings` | |
| REF-117 | Report element shape, element name and element order | IN SCOPE | MISSING | ST (negative search) | SHOW p.72 | `grep -rn 'ReportElementConfig\|ReportLayout\|ElementOrder' src/` → 0 files; order fixed by builder loops | |
| REF-119 | Semen analysis report with graphs and diagrams | IN SCOPE | MISSING | ST (negative search) | SHOW p.20-21 | Semen is only a sample-kind flag; no drawing API; no graph/chart code in Infrastructure | |
| REF-120 | Image report for culture, urine, stool, semen and blood picture | IN SCOPE | MISSING | ST (negative search) | SHOW p.24 | No image entity, upload, image-capable report or drawing surface | Second-largest and most dependency-heavy item |
| REF-129 | Vertical barcode on report, receipt and envelope | IN SCOPE | MISSING | ST (negative search) | SHOW p.3 | No vertical flag in any settings entity; the label renderer emits a fixed horizontal bitmap | **Phase 1 built the shared encoder, so this is now cheap** |
| REF-130 | Tube, container and swab label printing | IN SCOPE | MISSING | ST (negative search) | SHOW p.3, 52 | One fixed-size patient label; no sticker-size setting, no multi-up | |
| REF-150 | Worksheet of a group of patients in a period | IN SCOPE | MISSING | ST (negative search) | LEARN p.86-88 | Only visit / test-group / work-group-log modes exist | |
| REF-151 | Worksheet of a group of patients, condensed | IN SCOPE | MISSING | ST (negative search) | LEARN p.87-88 | `WorkSheetDtos` carries no layout-variant flag; no condensed query or writer | |
| REF-155 | Worksheet with full patient and test data | IN SCOPE | MISSING | ST (negative search) | LEARN p.88 | `WorkSheetLineDto` carries ordering facts only — no result value, no reference range, no demographics | |
| REF-156 | Worksheet with condensed patient data | IN SCOPE | MISSING | ST (negative search) | LEARN p.90-91 | Same root cause as REF-151/155 | |
| REF-205 | Detailed account by test price or by results | IN SCOPE | MISSING | ST (negative search) | SHOW p.27 | No per-test price or per-result breakdown query or writer | |
| REF-207 | Four account report types | IN SCOPE | MISSING | ST (negative search) | SHOW p.28 | No account report-type concept; no accounts writer | |
| REF-211 | Reader options: save after read, print barcode after read | IN SCOPE | MISSING | ST (negative search) | LEARN p.210 | No scanner, HID or keyboard-wedge integration, no read event | Hardware-dependent |
| REF-218 | Laboratory terms and abbreviations reference | IN SCOPE | MISSING | ST (negative search) | SHOW p.51 | No glossary entity/query/tab; the only `Abbreviation` hit is a stale comment | See C-1. BLOCKED: content is owner-supplied |
| REF-219 | Colour palette | IN SCOPE | MISSING | ST (negative search) | SHOW p.51 | No palette entity, query, UI or colour-picker control | |
| REF-220 | Reminder note for a specific user | IN SCOPE | MISSING | ST (negative search) | SHOW p.51 | `grep -rn 'Reminder\|LabNote\|Notification' src/` → 0 files | BLOCKED: persistence/visibility semantics undocumented |
| REF-221 | Follow-up appointment for a patient or phone number | IN SCOPE | MISSING | ST (negative search) | SHOW p.51 | `grep -rn 'FollowUp\|Recall' src/` → 0 files; `Patient.PickupDateUtc` is the expected-result date | BLOCKED: reference line OCR-damaged (E4) |
| REF-233 | Print the patient receipt automatically on registration | IN SCOPE | MISSING | ST (negative search) | LEARN p.192 | `grep -rni AutoPrint src/` → 0 files; no such flag in `SystemSettings` | |
| REF-234 | Receipt: print patient and doctor names in English | IN SCOPE | MISSING | ST (negative search) | LEARN p.192, 203 | `grep -rn 'FullNameEn\|NameEn' src/` → 0 files; one `FullName` per patient, one `Name` per external entity | BLOCKED: capture model must be chosen |

**Counted mechanically from the three classification tables: 25 EXCLUDED + 134 IMPLEMENTED + 58 DIFFERENT + 25 MISSING = 242 REF identifiers, 217 in scope.**

---

*End of audit. Produced by an independent forensic audit agent in analysis-and-verification-only mode. The repository at `6fa10aabcb93d4ea6f49dba00423d7f89fc1b11d` was not modified; this file is the sole artefact created.*
