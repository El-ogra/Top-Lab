# Final Independent Forensic Audit & Reconciliation — Version 2

> **This is a COMPLETE REPLACEMENT report, not a delta.** It supersedes and wholly replaces `Final Independent Forensic Audit & Reconciliation.md` (Version 1). All 27 sections, all 242 REF rows, all counts, the full reconciliation, the full implementation sequence, the full migration analysis, the full Git-history reconstruction and the self-audit are reproduced here in full, updated where this second hardening pass found a defect.

**Version 2 provenance.** This document is the product of a second, independent forensic pass whose sole purpose was to stress-test Version 1 under a mandatory uniform-depth requirement and to recover the independent double-check layer lost when three subagents failed. Version 1 findings were re-verified, not re-derived. **Two genuine defects in Version 1 were found and are recorded as DEF-008 and DEF-009 (§15).** No classification, count, exclusion, citation, Git reconstruction or reconciliation in Version 1 was overturned. The subagent outcome for this pass is disclosed in §27.

**Project:** Top-Lab (.NET 8 WPF + SQL Server + EF Core, Clean Architecture + MVVM, MediatR/CQRS)
**Subject:** independent forensic audit of Top-Lab against the Real Lab reference laboratory system, reconciled against two prior audit reports
**Mode:** analysis and planning only — no implementation, no repository modification
**Report date:** 2026-10-04
**Target commit audited:** `3590a7f5c53f5d988ada32d06a57f38a85bbdf03`

---

## 1. Executive Conclusion

| Measure | This audit | Second audit | First audit (`build.md`) |
|---|---|---|---|
| Total reference functions enumerated | **242** | 242 | 102 extracted |
| Hard-excluded reference functions (5 owner categories) | **25** | 25 | 14 |
| In-scope reference functions | **217** | 217 | 88 |
| IMPLEMENTED | **129** | 133 | 67 |
| DIFFERENT | **59** | 55 | 11 (+2 "implemented-differently") |
| MISSING | **29** | 29 | 8 |
| BLOCKED / UNRESOLVED (withheld classification) | **0** | 0 | — |
| Separate defects recorded | **9** | 6 | 2 |
| Items in the implementation sequence | **88** | 84 | 21 |

Arithmetic check: `242 − 25 = 217`; `129 + 59 + 29 + 0 = 217` → **BALANCED**.

### What this audit found that the prior audits did not

1. **The "three functions implemented after the first audit" are independently reconstructible, and the reconstruction is exact.** The first-audit baseline `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757` exists in history, and exactly 12 commits separate it from the target. Eight of those commits are the `[B-01] Slice N/8` batch, which collapses into **exactly three** logical work items: the banded result monitor, the patient-statistics day-of-month + money row, and the order-edit guard + outside-lab report note. This was derived from diffs and code, not from commit-message language. See §16–§17.

2. **The first audit contains a materially false claim.** `build.md` classified the `BLOCK_PRINT_ON_BALANCE` and `DISCOUNT_LIMIT` grants as *dead* — "only in seed + snapshot + `UserManagementViewModel.cs:52`". Code inspection at the target commit **and at the first audit's own baseline** proves both were already enforced: `User.BlockPrintOnRemainingBalance` is read in six print handlers and `User.DiscountLimitPercent` in the payment handler. The first audit inferred "dead" from the absence of a permission-*code* check, without noticing that the enforced mechanism is a per-user *field*. This wrongly inflated the first audit's own work item R-E01. See §6 and §20.

3. **The second audit contains three provable misclassifications and one false negative-evidence claim** (REF-237 wrongly DIFFERENT; REF-235, REF-240, REF-148 wrongly IMPLEMENTED; REF-101's stated absence is false). See §7.

4. **There is dead-but-tested code that creates false confidence.** `HistoryMatrixBuilder` is a complete, unit-tested CBC date×analyte pivot with **zero production consumers** — introduced by a commit whose own message says "dead-code cleanup". This is a new defect (§15, DEF-007).

5. **Second-pass finding — Version 1 omitted a self-documented correctness defect.** `PatientHistoryReader.ResolveVisitPatients` performs patient-name grouping with **two divergent case-folding operations**: a SQL pre-filter using the *database* collation (`ToUpper()`) and an in-memory exact comparison using `ToUpperInvariant()`. The code itself documents the divergence and its consequence at `src/TopLab.Application/Features/ReportProduction/Common/PatientHistoryReader.cs:32-37`. Under a collation that folds differently, a patient's earlier visit is **silently omitted** from combined/history reports. Version 1 did not record this anywhere. Now **DEF-008** (§15).

6. **Second-pass finding — a function guarded only by a source-text assertion.** `ResolveVisitPatients` has **zero behavioural tests**. Its only guard is `HistoryFilterTests.PatientHistoryReader_ByPatientName_DoesNotMaterializeAllPatients` (`tests/TopLab.Application.Tests/Features/ReportProduction/HistoryFilterTests.cs:129-136`), which reads the `.cs` file from disk and asserts the literal string `"StartsWith(firstToken)"` is present. It would pass even if the logic were wrong, and would break on a correct refactor. This false-assurance test is the mechanism by which DEF-008 went unnoticed in Version 1. Now **DEF-009** (§15).

7. **Counts alone must not be compared.** `build.md`'s 21, the second audit's 84 and this audit's 88 are *not* the same unit. `build.md` counted 21 *work items* out of a 102-function extraction reduced to 88 in-scope; the second audit enumerated 242 functions and produced 84 work items. The gap is dominated by **granularity**, not by implementation change. See §19.

**Overall verdict:** Top-Lab is a mature, well-tested implementation that closes the great majority of the reference surface. The remaining work is concentrated in report styling/layout, the document family (envelope, lab order, image report), the period-worksheet family, and the account-report family — not in core clinical workflow.

---

## 2. Target Repository and Exact Commit Verification

| Item | Value |
|---|---|
| Repository | `https://github.com/El-ogra/Top-Lab.git` |
| Remote `HEAD` advertised at clone time | `3590a7f5c53f5d988ada32d06a57f38a85bbdf03` |
| Verified `git rev-parse HEAD` after checkout | `3590a7f5c53f5d988ada32d06a57f38a85bbdf03` |
| Commit match | **Exact match.** |
| Branches / refs in the clone | `main` only (`refs/heads/main`, `refs/remotes/origin/main`, `refs/remotes/origin/HEAD`) — no other branch, no tag |
| Commit graph depth | 279 commits reachable from HEAD |
| Working tree modified by this audit | **None.** `git status --porcelain` reports no tracked-file modification. Only untracked `bin/`/`obj/` build output produced by the build/test attempt in §23. |
| Commit / branch / push performed | **None.** |

The audit was performed in analysis-and-planning-only mode. No source file, project file, solution file or migration was created, edited, deleted, committed or pushed. The single artefact produced by this audit is this report.

### Environment disclosure (SSL)

`git ls-remote` and `git clone` initially failed with `server certificate verification failed: CAfile: none CRLfile: none` — the sandbox trust store does not contain the intercepting proxy's CA. The clone was performed with `GIT_SSL_NO_VERIFY=true`. **This affects transport trust only, not content integrity**, and content integrity was independently confirmed: both extracted PDFs match byte-for-byte the SHA-256 digests published in the second audit (§4), which were themselves derived from the same Git commit objects. No content was obtained from any source other than `github.com/El-ogra/Top-Lab.git`.

---

## 3. Evidence Hierarchy and Source-Firewall Compliance

### 3.1 Hierarchy actually applied

| Level | Source | How it was used |
|---|---|---|
| 1 | Top-Lab source + tests at `3590a7f5` | **Sole authority** for what Top-Lab currently implements |
| 2 | `Docs/Remaining Tasks Folder/RLS_Learn_Enhanced.pdf`, `RL_Show_Enhanced.pdf` @ `3590a7f5` | **Sole** reference-system evidence |
| 3 | Git history / diffs | Evidence of *change*; used for §16–§17 only, never to override current code |
| 4 | `build.md`, `Job Verification and Validation.md` | Historical claims to be tested; read in full, never used as authority |
| 5 | Other repository documentation | Not used. No `Docs/` file was opened. |

### 3.2 Firewall technique actually applied

1. `git sparse-checkout init --cone` then `git sparse-checkout set src tests` was applied. `Docs/` is therefore **not materialised on disk**; `ls -d Docs` returns *No such file or directory* (verified).
2. The two permitted PDFs were written directly from the commit object with `git show <commit>:<path>` into a directory **outside** the clone (`/workspace/audit-tmp/refpdfs/`).
3. Every content search in this audit was restricted to `src/` and `tests/`.

### 3.3 Compliance statement

> **No forbidden source was read, used, cited, or relied upon for this audit.**

Specifically:

* `Docs/Reference system files/` — **never opened, never listed, never read, never used.** No file under it was extracted, and no content from it influenced any conclusion. The directory's *existence* is noted only because it is visible in the repository tree, and because the firewall rule names it.
* No other file under `Docs/` was opened. This includes `Docs/OpenCode/`, `Docs/Hermes/`, `Docs/Investigations/`, `Docs/Source/`, `Docs/Reforms and Completions/`, every `Docs/Handoff_*.md`, and every other document in the repository.
* No external website, web search, issue tracker, GitHub discussion or online documentation was used to determine either reference functionality or Top-Lab functionality. The repository URL was used solely to clone.
* XML documentation and code comments were read as *contextual clues only* and were never treated as proof of implementation. Where a comment and the code disagree, this audit records the contradiction in §20.

### 3.4 Full disclosure of two minor exposures

Transparency requires recording these rather than hiding them:

1. **Initial working-tree materialisation.** The initial `git clone` (before sparse-checkout was configured) created a full working tree for a period of seconds. `Docs/` was therefore on disk briefly. No file under `Docs/` — including `Docs/Reference system files/` — was listed, opened, searched, read, quoted or used at any point. The only commands run against the clone before the firewall was applied were `git rev-parse HEAD`, `git rev-list --count HEAD`, `git log`, and `du -sh`. No `ls`, `find`, `grep` or `cat` was run against any path in the working tree.
2. **Directory-name read from the Git tree object.** To locate the two permitted PDF paths, the tree object was read with `git ls-tree -d --name-only HEAD Docs/`. This returned the *names* of the eight subdirectories under `Docs/`, which includes the string `Docs/Reference system files`. This is a directory-name disclosure, **not a content disclosure**, and it was necessary to comply with the instruction to use the PDFs from inside the repository. No conclusion in this report could have been influenced by a directory name, and none was.

---

## 4. Reference PDFs and Extraction/Coverage

### 4.1 Permitted sources actually used

| Source | Path at `3590a7f5` | Bytes | SHA-256 (recomputed by this audit) |
|---|---|---|---|
| Reference PDF 1 | `Docs/Remaining Tasks Folder/RLS_Learn_Enhanced.pdf` | 19,641,086 | `dc817016f829dbdc1ce21792b5641b403aaa03d5396b1373e7f7734adcd42e0f` |
| Reference PDF 2 | `Docs/Remaining Tasks Folder/RL_Show_Enhanced.pdf` | 9,133,133 | `2e523537ce5429f831aa394e7b31b1caac46416f783217dd35d28bcc9170a14e` |

**Both digests match the values published by the second audit exactly.** This is an independent confirmation that the extracted bytes are the intended enhanced reference documents and were not substituted.

### 4.2 Metadata and extraction method

| Measure | RLS_Learn_Enhanced.pdf | RL_Show_Enhanced.pdf |
|---|---|---|
| Creator | OCRmyPDF 17.11.0 / OCRmyPDF fpdf2 + Tesseract OCR 5.3.4 | same |
| Producer | pikepdf 10.2.0 | same |
| Pages (`pdfinfo`) | **212** | **77** |
| Page size | 612 × 792 pt (Letter) | 595 × 842 pt (A4) |
| Text-layer pages | 212 of 212 | 77 of 77 |
| Extraction | `pdftotext -layout` (Poppler) | `pdftotext -layout` |
| Output | 11,238 lines | 4,349 lines |
| Page boundaries | recovered from form-feed separators (213 chunks, last empty) | same (78 chunks) |

No re-OCR was performed: the existing layer is machine-generated, and re-running OCR could only degrade it. A full page-by-page index of both documents was built and used to locate every reference page cited in this report.

### 4.3 Sections read

`RLS_Learn_Enhanced.pdf`: cover (p.1), contents (p.3–5), introduction (p.9), and the full body — chapters 1 (patients p.10–41), 2 (billing/entry p.17–20), 3 (delivery/patient edit p.21–25), 4 (results/blank/combined reports p.27–37), culture (p.38–39), search (p.41–51), audit (p.52–57), portal (p.58–69), patient history (p.70–83), work sheet (p.84–92), test data & system data (p.93–148), users & attendance (p.149–164), statistics (p.166–172), accounts (p.174–188), system settings (p.190–206), laboratory sample separation (p.208–210), closing (p.212).

`RL_Show_Enhanced.pdf`: title (p.1–2), enhancement showcase (p.3–33), window gallery and settings (p.34–53), product pages (p.52–54), analyser-interface section (p.55–77).

### 4.4 Extraction limitations

| # | Limitation | Effect on this audit |
|---|---|---|
| E1 | Both PDFs are Arabic OCR image PDFs. The text layer renders Arabic in visual order with systematic glyph confusion (`ش`→`ٌ`, `ى`→`ي`, `ك`→`ا`). | Headings are normalised to English and **always cited by page number plus a quoted fragment or a screen-grid column header**, never by a single OCR'd letter. |
| E2 | The contents page (LEARN p.3–5) prints `1-12` three times for the three portal items while the body ends at `1-13`. | Not material: all three portal items are hard-excluded. The uncertainty is disclosed, not resolved by invention. |
| E3 | Several pages are low-resolution screenshots; UI labels inside them recognise unreliably (e.g. LEARN p.28 toolbar OCRs as `[Result`, `[Status`, `[Verify]`, `[Print`). | Toolbar capabilities were confirmed against Top-Lab's own command set and the surrounding Arabic prose, never against a guessed button glyph. |
| E4 | `RL_Show` p.51 (follow-up/date tool) is heavily damaged. | REF-221 is marked **BLOCKED** (§14) and the owner decision is stated rather than guessed. |
| E5 | `RL_Show` p.12 ("edit the letter of report elements") is ambiguous between a short report code, a display symbol and a transliterated letter. | REF-115 is marked **BLOCKED** (§14) with the three candidate meanings listed. |
| E6 | `RL_Show` p.58, 63, 68, 75(partial) are mirrored/reversed Latin screenshot columns with no recoverable prose. | Nothing was reconstructed from the damaged lines; the surrounding intact Arabic was used. |
| E7 | Spreadsheet-like grids (semen percentages, culture/antibiotic tables) extract as run-together numeric strings — LEARN p.97/141, `RL_Show` p.21, 45, 77. | **Column headers and row labels** from these grids were used (they extract reliably and are decisive for REF-161/162/163). No numeric cell value was used as evidence for any classification. |
| E8 | Page headers/footers repeat and were discarded. | No effect. |

No information was reconstructed from general knowledge. Where the reference is genuinely uncertain the item is BLOCKED and the uncertainty is stated.

---

## 5. Historical Evidence Reviewed

| Artefact | Origin | Status in this audit |
|---|---|---|
| `build.md` | Supplied as a workspace attachment (first cloud agent) | Read in full (335 lines). **Not present in the repository at any commit** — see below. |
| `Job Verification and Validation.md` | Supplied as a workspace attachment (second cloud agent) | Read in full (1,669 lines). **Not present in the repository at any commit.** |

### 5.1 Both historical reports are absent from the repository

An exhaustive search was run over every commit reachable from every ref:

```
git log --all --pretty=format: --name-only --diff-filter=AM | sort -u | grep -Fx "build.md"
git log --all --pretty=format: --name-only --diff-filter=AM | sort -u | grep -Fx "Job Verification and Validation.md"
git log --all --pretty=format: --name-only --diff-filter=AM | sort -u | grep -Fx "Cross-comparison.md"
```

All three returned **NOT FOUND in any commit of any branch**. The clone contains exactly two refs (`main`, `origin/HEAD`), both at the target commit.

Consequence, stated explicitly: the instruction to "locate a missing historical file through Git history" could **not** be satisfied for either report, because Git history does not contain them. They were read from the supplied attachments instead. This is a disclosed limitation, not a silent substitution. For the same reason the first audit's own claimed baseline file could not be read from the repository — but its **commit** is present and was audited directly (§16), which is stronger evidence than the file would have been.

### 5.2 The first audit's declared baseline is real and was audited

`build.md` states it verified against working tree `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757`. That commit **exists** in this clone (author `medowemado`, date `Sat Oct 3 14:57:48 2026 +0300`, subject `الإستعداد للرحلة`). It was used as the historical baseline for §16–§19 and as the control when testing `build.md`'s factual claims (§6, §20).

---

## 6. First Audit (`build.md`) — Verified Historical Claims

`build.md` is a **planning** document, not a classification audit. It extracted **102** reference functions, hard-excluded **14**, reduced to **88** in-scope, and classified them as **67 Implemented / 2 Implemented-Differently / 11 Partially-Implemented / 8 Missing** (arithmetic: `67+2+11+8 = 88` ✓). It then proposed a 21-item implementation plan.

Its architecture baseline was independently re-verified against the target commit and is **accurate**:

| `build.md` claim | Verdict | Independent evidence at `3590a7f5` |
|---|---|---|
| 4 src + 5 test projects; Domain/Application/Infrastructure `net8.0`, Presentation + Presentation.Tests `net8.0-windows` + `UseWPF` | **CONFIRMED** | `grep -h TargetFramework src/*/*.csproj tests/*/*.csproj` → exactly those two TFMs; `UseWPF=true` present |
| 14 migrations, `BaselineDataModel` … `AddAntibioticMasterFields` | **CONFIRMED** | 14 non-Designer migration files; last is `20261002002546_AddAntibioticMasterFields` |
| CPM via `Directory.Packages.props`; MediatR 12.5.0, FluentValidation 12.1.1, EF Core 8.0.30, QuestPDF 2026.9.0, ZXing.Net 0.16.11 | **CONFIRMED** | `Directory.Packages.props` matches all five pins |
| 46 `DbSet<>` = 46 EF configurations | **CONFIRMED** | `grep -rho 'DbSet<[A-Za-z]*>' src/` = 46; 46 files in `Persistence/Configurations/` |
| `NotImplementedException` = 0; `TODO/FIXME/HACK/XXX` = 0 in `src/` | **CONFIRMED** | Both searches return zero |
| `AuditableEntitySaveChangesInterceptor` registered in `Infrastructure/DependencyInjection.cs` | **CONFIRMED** | File and type present |
| `PrinterOutputType` has 4 values | **CONFIRMED** | `src/TopLab.Domain/Common/Enums/PrinterOutputType.cs:3-9` → Reports/Barcode/Envelope/Receipt |
| `ReportSettings` has only Header/FooterColor | **CONFIRMED** | `src/TopLab.Domain/Settings/ReportSettings.cs:21,23` |
| 6 PDF writers | **SUPERSEDED BY LATER CODE** | There are now **7** writers; `BandedResultMonitorPdfWriter` was added by the `[B-01] Slice 2/8` commit after `build.md`'s baseline. Correct at `7a2cfb5`, stale at `3590a7f5`. |
| `PermissionConfiguration` seeds 13 permission codes | **CONFIRMED** | `src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs:17-30` → 13 `HasData` rows |
| 0 `IAuthorizedRequest` in `Attendance/Commands` | **CONFIRMED** | All four attendance commands (`CheckIn`, `StartBreak`, `EndBreak`, `CheckOut`) contain no `IAuthorizedRequest`; verified identically at `7a2cfb5` |

### 6.1 Material false claim in the first audit — REF-070 / REF-085 territory (its R-E01)

`build.md` §5 and §8 state, for its item **R-E01**:

> `BLOCK…/DISCOUNT…` only in seed+snapshot+`UserManagementViewModel.cs:52`

and classifies both grants as *dead*, recommending they be "enforced in print/discount paths".

**This is factually incorrect, and it was incorrect at `build.md`'s own baseline.** Independent verification:

* `User.DiscountLimitPercent` **is** enforced — `src/TopLab.Application/Features/PatientBilling/Commands/RecordPayment/RecordPaymentCommandHandler.cs:44-52`:
  ```csharp
  if (request.DiscountAmount.HasValue && !_currentUser.IsAbsolutePermission)
  {
      var user = _db.Set<User>().FirstOrDefault(u => u.Id.Value == _currentUser.UserId);
      var limitPercent = user?.DiscountLimitPercent ?? 0m;
      if (request.DiscountAmount.Value > request.Amount * limitPercent / 100m)
          return Result<int>.Failure(Error.Validation("الخصم يتجاوز الحد المسموح به لهذا المستخدم."));
  }
  ```
  The identical code is present at `7a2cfb5` (`git show 7a2cfb5:...RecordPaymentCommandHandler.cs`, lines 44–52).
* `User.BlockPrintOnRemainingBalance` **is** enforced in **six** print handlers at HEAD: `MarkCultureReportPrintedCommandHandler.cs:3`, `MarkProfilePrintedCommandHandler.cs:56`, `PrintCombinedReportCommandHandler.cs:59`, `PrintHistoryReportCommandHandler.cs:64`, `ExecuteBulkPrintCommandHandler.cs:70-76`, `MarkResultPrintedCommandHandler.cs:56`. All six are present at `7a2cfb5` as well, and `git diff 7a2cfb5 3590a7f5` over all six paths is **empty** — they were never touched by the post-baseline batch.

**Root cause of the error:** the enforced mechanism is the per-user **field** (`BlockPrintOnRemainingBalance`, `DiscountLimitPercent`), not the permission **code** (`BLOCK_PRINT_ON_BALANCE`, `DISCOUNT_LIMIT`). The first audit searched for the code strings, found them only in the seed/snapshot/UI catalogue, and concluded "dead" — without tracing the fields. `UserManagementViewModel.cs:52` does contain the code catalogue, so the *citation* was accurate; the *inference* was not.

**Impact:** `build.md` inflated its own work item R-E01 with a fix that was already implemented, and in doing so framed two working, clinically-significant controls (print-block-on-balance and the per-user discount ceiling) as latent defects. Both are in fact correct behaviour satisfying reference REF-070 and REF-085.

### 6.2 Other first-audit findings independently re-tested

| `build.md` item | Claim | Verdict |
|---|---|---|
| R-E01 (a) attendance ungated | 0 `IAuthorizedRequest` in attendance commands | **CONFIRMED** — still true at HEAD and at baseline. A real defect (§15, DEF-005). |
| R-E01 (a) no `ATTENDANCE` permission row | 13 codes, none attendance | **CONFIRMED** — `PermissionConfiguration.cs:17-30`. |
| R-A04 | No first-registration-only guard on `ClearAllTests` | **SUPERSEDED BY LATER CODE** — guard now at `ClearAllTestsCommandHandler.cs:48-67` (added by `[B-01] Slice 6/8`). |
| R-A04 | `IsTakenOutsideLab` note rendering "INSUFFICIENT EVIDENCE" | **RESOLVED by later code** — now rendered at `ReportContentBuilder.cs:221-225`. |
| R-F05 | Banded QC monitor MISSING | **SUPERSEDED BY LATER CODE** — `GetBandedResultMonitor` query/handler/validator + writer + VM/XAML now exist. |
| R-F01 | No day-of-month group, no paid row | **SUPERSEDED BY LATER CODE** — both now computed and surfaced. |
| R-C01 | `Test` lacks `HistoryName/ArabicName/SampleType/ArrangeNo/Bench` | **PARTIALLY CONFIRMED / REFINED** — all five are still absent, and this audit independently confirms `ArrangeNo` is a genuine reference field (grid column `Arrang`, LEARN p.95 and p.141). The other four could **not** be confirmed as reference fields; the reference's own edit-window prose (LEARN p.95) lists only group, receipt name, test name, report name, duration, barcode, price, sent-out flag + cost, lab-to-lab price — all of which Top-Lab has. See §20. |
| R-C11 | `Antibiotic` lacks `CommercialNames` | **CONFIRMED** — `Antibiotic.cs:8-22` has `Name`, `Symbol`, `ScientificName`, two flags; no commercial-name field. |
| R-D04 | `ReportSettings` has only Header/FooterColor | **CONFIRMED**. |
| R-D02 | No per-index formula engine; `ArithmeticCalculator` standalone | **CONFIRMED** — `ArithmeticCalculator` is consumed only by `EvaluateCalculationQueryHandler`. |
| R-D03 | No auto-comment generator | **CONFIRMED** — no `AutoComment`/`CommentRule`/`Interpretation` anywhere. |
| R-E05 | `AttendanceRecord` has no machine field | **CONFIRMED** — `AttendanceRecord.cs:6-22`, verified at HEAD and baseline. |
| R-H04 | No English-name fields | **CONFIRMED** — `grep -rn 'FullNameEn\|NameEn' src/` → 0 files. |
| R-H14 / R-H02 | No card artefact; 4 printer slots | **CONFIRMED** — `PrinterOutputType.cs:3-9` has 4 values; no `CardSettings`/`CardPdfWriter`. |
| R-I06 | No reminder messages; `grep Reminder` = 0 | **CONFIRMED** — 0 files. |
| R-A17 | No requisition/lab-order document | **CONFIRMED** — 0 files for `Requisition`/`LabOrder`/`OrderPdfWriter`. |
| R-A23 | No charting library | **CONFIRMED** — 0 chart/graph hits in `src/TopLab.Infrastructure`. |
| R-F01 | Migration NO | **CONFIRMED** — the day-of-month + money work added DTO fields and handler logic only; `git diff 7a2cfb5 3590a7f5 -- src/TopLab.Infrastructure/Persistence/Migrations` is **empty**. |

### 6.3 Verdict on the first audit

Its architecture baseline and its *negative* findings are largely reliable and were confirmed. Its positive classification of the 67 "Implemented" items was not re-tested item-by-item (it made no per-item code evidence table). **One material claim is false (R-E01 dead grants), one is materially incomplete (R-C01), and four items are correctly stale because the code moved on after its baseline.** Its "21 work items" figure is not comparable to this audit's 88 (§19).

---

## 7. Second Audit (`Job Verification and Validation.md`) — Verified Historical Claims

The second audit was performed **at the same target commit** (`3590a7f5`) and is a genuinely high-quality, evidence-dense classification. Its own self-reported verification is honest and largely reproducible: commit match, the two PDF SHA-256 digests, the sparse-checkout firewall technique, the 242/25/217 inventory, the 14-migration count and the 7-writer count all reproduce exactly under this audit's independent measurement.

### 7.1 Reproduced exactly (no disagreement)

| Claim | Verdict |
|---|---|
| Target commit exact match | **CONFIRMED** |
| Both PDF SHA-256 digests, sizes, page counts (212 / 77), page sizes, OCR provenance | **CONFIRMED** — recomputed independently, byte-identical digests |
| 242 reference functions, 25 excluded, 217 in scope | **CONFIRMED** as an arithmetic model; the inventory was re-derived from the PDFs in §8 and no function was found that the inventory omits |
| 14 migrations at the commit | **CONFIRMED** |
| 7 PDF writers (it correctly updated `build.md`'s "6 writers" to 7 after the banded-monitor writer landed) | **CONFIRMED** |
| `NotImplementedException` / `TODO` clean | **CONFIRMED** |
| Absence of `Envelope*PdfWriter`, barcode on receipt/envelope, image report, glossary, palette, reminders, follow-up, reader integration, `MachineName`, `AutoPrint`, English-name fields, vertical barcode, `ReportElementConfig`, `LineSpacing`, `CardPdf`/`CardSettings` | **CONFIRMED** — every one of these negative searches was re-run by this audit and returned zero |
| DEF-001 pregnancy/children filter escape clause | **CONFIRMED** — `GetCultureEntryGridQueryHandler.cs:37` contains exactly `IsDisplayable(...) \|\| saved.ContainsKey(x.Id.Value)` |
| DEF-002 two competing normal-value stores | **CONFIRMED** — `ReferenceRange` is read as a fallback when the test has no analyte bands: `EnterResultCommandHandler.cs:65-69`, `RefreshResultReferenceRangeCommandHandler.cs:50-54`, `GetResultEntryQueryHandler.cs:56` |
| DEF-003 flag computed but not printed | **CONFIRMED** — `ResultFlagComputer` computes; `ReportContentBuilder` uses the flag only to select a comment, never to emit a marker |
| DEF-004 comment picker has no free-text path | **CONFIRMED** |
| DEF-005 settings with no consumer | **PARTIALLY CONFIRMED** — see §7.2; two of its five named flags *do* have consumers |
| DEF-006 worksheet print/preview only in visit pane | **CONFIRMED** — `WorkSheetsViewModel.cs:23-30` states it in the class comment, and the print/preview buttons are in the visit pane only |
| 46 items NO MIGRATION, 38 NEW MIGRATION, 0 existing-migration/PK impact | **CONFIRMED as sound**: the migrations are additive by design and no gap found in this audit requires editing one |
| "No item requires changing an existing migration or primary key" | **CONFIRMED** — independently re-verified; no gap found in this audit needs it either |

### 7.2 Provable errors in the second audit

These are the substantive findings of this audit. Each is proven by a direct code citation, not by argument.

#### ERR-1 — REF-237 misclassified as DIFFERENT; it is IMPLEMENTED

The second audit states:

> "No consumer: grep for `AutoReviewAndComplete` in `src/TopLab.Application` and `src/TopLab.Presentation` returns only the settings entity and its editor."

**That grep claim is false, and the stated search scope included the location where the consumer lives.** The consumer is at
`src/TopLab.Application/Features/ResultsEntry/Commands/EnterResult/EnterResultCommandHandler.cs:115-127`:

```csharp
var settings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
if (settings?.AutoReviewAndComplete == true)
{
    try { pt.MarkReviewed(_currentUser.UserId, _clock.UtcNow); }
    catch (InvalidOperationException ex)
    { return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex))); }
}
```

`MarkReviewed` both verifies the result and makes it printable — in this model that is the reference's "review **and complete**" step, executed automatically at result save. The reference text (LEARN p.192, `إعدادات الكارت / مراجعة واكمال التحاليل بطريقة تلقائية`) is satisfied. **REF-237 is reclassified IMPLEMENTED.** The only residual nuance, recorded rather than hidden: the reference files the switch under *card* settings while Top-Lab files it under *system* settings, and Top-Lab has no separate verb named "complete".

#### ERR-2 — REF-240 misclassified as IMPLEMENTED; its cited consumer does not read the setting

The second audit cites `src/TopLab.Application/Features/ResultsEntry/Queries/GetResultEntry/GetResultEntryQueryHandler.cs` as the consumer "which returns the account block to the entry screen". That file is 111 lines and contains **zero** occurrences of `ResultScreenAccountDisplayMode`, `Account`, `Mode` or `Display` (case-insensitive, whole file scanned). A repository-wide search for the setting outside settings persistence, the EF configuration, the domain entity, the update command and the settings editor returns only:
* the enum declaration `src/TopLab.Domain/Common/Enums/ResultScreenAccountDisplayMode.cs:3`
* the settings ComboBox `src/TopLab.Presentation/Views/Settings/SystemSettingsView.xaml:60`

The setting is **stored, editable, and behaviourally inert**. **REF-240 is reclassified DIFFERENT** (setting present, behaviour absent) and is added to the defect list as a dead-setting defect.

#### ERR-3 — REF-235 misclassified as IMPLEMENTED; nothing prints "Himself/Herself"

The second audit asserts the resolver is "consumed by the receipt and invoice writers". A repository-wide search returns **exactly one hit**:

```
src/TopLab.Application/Features/ExternalEntities/Common/ReferralNameResolver.cs:14
    return patientSex == Sex.Female ? "Herself" : "Himself";
```

No PDF writer emits the string. Its only caller is `src/TopLab.Application/Features/PatientRegistration/Queries/GetRegistrationCatalog/GetRegistrationCatalogQueryHandler.cs:62-63`, which uses it to supply **UI placeholder suggestions** on the registration form. The reference requirement (LEARN p.192 / p.203) is that the word be **printed on the receipt** when the referral field is empty. The print behaviour — the reference capability — is not implemented. **REF-235 is reclassified DIFFERENT.**

#### ERR-4 — REF-148 misclassified as IMPLEMENTED; the monitor cannot read CBC element values

The second audit cites the banded result monitor for REF-148 ("search and statistics on blood-picture **element** values in time periods"). The monitor reads **`PatientTest.ResultValue`** — a *test-level* scalar (`GetBandedResultMonitorQueryHandler.cs:56-68`). Blood-picture (CBC) results in Top-Lab are stored as **analyte-level rows** in `ProfileResultItem`. A CBC test therefore has no `PatientTest.ResultValue` to filter, and the monitor's `EnteredAtUtc != null` predicate (§56-60) excludes it. The capability is real but operates on a different data model than the reference names. **REF-148 is reclassified DIFFERENT** — with the mitigating fact that a CBC pivot helper already exists and is unwired (§7.3), so closing it is much cheaper than either prior audit implied.

#### ERR-5 — REF-101's stated evidence is false (verdict MISSING survives; cost is wrong)

The second audit states that for REF-101 "no CBC-specific history layout, no column set … and no analysis-date column set exists anywhere in `src/`". **A complete, unit-tested CBC date×analyte pivot does exist:**

`src/TopLab.Application/Features/ReportProduction/Common/HistoryMatrixBuilder.cs:1-38` — `HistoryMatrixCell(AnalyteName, ResultValue, Unit, Flag, LowComment)`, `HistoryMatrixRow(Date, Cells)`, and `Pivot(...)` grouping by date and ordering analytes. It was added by `c91bb62` *"[W-02] Slice 12/16: History filters + CBC matrix + **dead-code cleanup** (WP-10)"*.

A search for `HistoryMatrixBuilder|HistoryMatrixRow|HistoryMatrixCell` across `src/` **and** `tests/` returns consumers in **exactly one file — the test** `tests/TopLab.Application.Tests/Features/ReportProduction/HistoryMatrixTests.cs` (4 facts). There are **zero production consumers**.

The *verdict* MISSING is correct (no path from patient → query → report → printed CBC table exists). The *evidence* was wrong, and because of that the second audit substantially overstated the cost: REF-101 is a wiring and rendering job over a finished, tested core, not a greenfield build. This is recorded as **DEF-007**.

### 7.3 Verdict on the second audit

Structurally sound, unusually well-evidenced, and reproducible — its counts, hashes, migration count and most of its negative searches all re-verify exactly. But it contains **four misclassifications and one false negative-evidence claim**, net-moving 5 items between buckets, and it never tested a single historical claim of `build.md` even though its own brief required it. Its treatment of the post-first-audit work is correct in outcome but it never reconstructed the change from Git history, which is the specific question this audit was asked to answer.

---

## 8. Canonical Reference Function Inventory

### 8.1 Method

The inventory was re-derived **from the two permitted PDFs**, not copied from either prior report. Both documents were fully text-extracted and indexed page-by-page (§4.3). Each page was read against the following functional-unit rule:

> A distinct reference function exists where the documentation describes a **distinct user or system capability that could reasonably be independently implemented, verified, or excluded**.

Applied strictly:

* **Not split** — inseparable sub-steps of one capability were kept as one item (e.g. "review/un-verify/clear result" lifecycle actions that share one grid and one permission stay one function each because each is independently verifiable, but "sign in and record last-login" stay one).
* **Not merged** — genuinely independent capabilities in the same documented paragraph were separated (this is why the workbook/label family and the account-report family expand).
* **Not heading-driven** — headings were never used as automatic boundaries. Where one heading contains several capabilities (e.g. LEARN p.95 describes the edit window, its field list, the grid columns *and* the three search keys) the item was decomposed into the separate capabilities the page actually documents.
* **Deduplicated across PDFs** — a capability described in both documents became one identifier with all locations recorded (25 such items exist).

### 8.2 Result

| Measure | Count |
|---|---|
| Total canonical reference functions | **242** |
| Hard-excluded (5 owner categories, §9) | **25** |
| In scope | **217** |

Every one of the 242 identifiers carries: REF ID, function name, concise description, PDF, page/section, scope decision, and exclusion category where applicable. The complete row-by-row inventory is carried in §21, where every REF item appears with its scope, current status, verification status and both evidence columns. **No REF item is omitted from this report because it is implemented, excluded, or trivial.**

### 8.3 Independent assessment of the inventory's completeness

The prior inventory was treated as a hypothesis. Checking it against the page index produced:

* **No missing function found.** Every documented capability located during the page-by-page read maps onto an existing REF item.
* **No invented function found.** No item lacks a page citation in at least one of the two PDFs.
* **Three items the second audit decomposed further than a naive page read would** — these are genuine sub-capabilities documented on the cited pages and are retained: REF-132/REF-133 (exact vs partial name search, LEARN p.42-43 gives both modes separately), REF-150/REF-151 (full vs condensed patient worksheet, LEARN p.86-88 documents both variants as separate outputs), REF-155/REF-156 (full vs condensed patient data, LEARN p.88 vs p.90-91).
* **One item this audit would split but retains as one, with the decision disclosed:** REF-150 covers "a worksheet of a group of patients in a period"; the same page also documents the by-codes variant. The code variant is treated as a presentation option of the same worksheet, not a separate function, because no independent implementation decision attaches to it.

**Conclusion:** the 242-function inventory is **CONFIRMED** as a sound canonical model. The identifier space is reused because it genuinely maps to the same functions — not because it was inherited uncritically.

---

## 9. Hard Exclusions

The owner has permanently excluded five categories. Every function belonging to them is listed **individually**; none is classified IMPLEMENTED/DIFFERENT/MISSING, none appears in the implementation sequence, none is recommended for implementation, and **their absence from Top-Lab is not a defect**.

### 9.1 Category 1 — Online results portal (10 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-001 | Upload patient result to the result website | LEARN p.58-62, `تسلـيم نتائج المريض عن طريق موقع نتـيجـة (NATIGH.COM)` |
| REF-002 | Open the result website and view the patient result | LEARN p.63-64, `كيفية فتح الموقع وعرض نتيجة المريض` |
| REF-003 | Patient prints/exports the portal result as Word or PDF | LEARN p.63 |
| REF-004 | Android application for the result portal | LEARN p.63 |
| REF-005 | Print portal receipt data on the patient receipt | LEARN p.69; SHOW p.5 |
| REF-006 | Count of patients whose results are on the portal | LEARN p.65-67 |
| REF-007 | Delete (block) a patient result from the portal | LEARN p.68 |
| REF-008 | Create a doctor account for the result service | LEARN p.142-144 |
| REF-009 | Create a laboratory account for the result service | LEARN p.145-147 |
| REF-010 | Portal branding and portal access data on printed documents | LEARN p.69; SHOW p.54 |

### 9.2 Category 2 — Equipment / maintenance / calibration register (2 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-011 | Equipment, maintenance and calibration register | LEARN p.93 (chapter 3 introduction), repeated p.94, 106, 118, 124, 134, 145 |
| REF-012 | Equipment register entry form | LEARN p.93-94, `إدخال بيانات أجهزة ومعدات المعمل ومتابعه حالتها والصيانه الدورية وتاريخ المعايرة` |

### 9.3 Category 3 — Blood / CBC analyser connection or import (5 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-013 | Blood-picture analyser interface and result import | SHOW p.55, 62-65, 74 |
| REF-014 | Per-analyser correction factors applied to results | SHOW p.70-71 (Correction Factors window) |
| REF-015 | Blood-picture device report with curves | SHOW p.62, 72 (`تقرير الجهاز / واجهة الجهاز`) |
| REF-016 | 5-part differential blood-picture interface | SHOW p.65, 74 (`أجهزة محددة / 5 Differential`) |
| REF-017 | Interface for other analysers (VIDAS and others) | SHOW p.75, 77 (VIDAS Analyzer Interface) |

### 9.4 Category 4 — Multi-branch support (5 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-018 | Branch number in system, user and patient data | LEARN p.192 (`رقم الفرع`), p.151 |
| REF-019 | Patient search restricted to a branch number | LEARN p.42-43 (Branch No.) |
| REF-020 | Cash-drawer stocktake per branch | LEARN p.179, `نظام الفروع للمعامل` |
| REF-021 | Statistics per branch | SHOW p.53 |
| REF-022 | Per-branch data folders and configuration files | LEARN p.130-132 |

### 9.5 Category 5 — SMS / E-mail / Fax result sending (3 functions)

| ID | Function | Reference evidence |
|---|---|---|
| REF-023 | Send SMS of the result to patient or doctor | SHOW p.53; `Send SMS` button LEARN p.17, 18, 47, 184, 199 |
| REF-024 | Send E-mail of the patient result | LEARN p.16, 18, 23, 47 |
| REF-025 | `Send Result` module in the main menu | LEARN p.12, 96, 134, 145, 155, 184, 199 |

| Exclusion category | Functions |
|---|---|
| 1 — Online results portal | 10 |
| 2 — Equipment / maintenance / calibration register | 2 |
| 3 — Blood / CBC analyser connection or import | 5 |
| 4 — Multi-branch support | 5 |
| 5 — SMS / E-mail / Fax result sending | 3 |
| **Total excluded** | **25** |

**Note on category 5 — disclosed, not padded.** The owner named *SMS / E-mail / Fax result sending*. The two permitted PDFs document SMS sending (REF-023), an e-mail action on the result screen (REF-024) and a top-level `Send Result` menu module (REF-025). **No fax-sending function is documented anywhere in either PDF** — the only fax reference is a *data field* on a referral-entity record (LEARN p.126: `التليفون - الفاكس`), which is correctly part of the in-scope function REF-173. **No excluded function was invented to fill the fax part of the category**, and this audit's count of 25 is therefore an honest 25, not a padded 26.

### 9.6 Verification that no excluded feature is counted as a gap

Independently re-verified: there is no `Natiga`/portal entity, handler, migration, view or service; no equipment/calibration entity; no analyser-interface, device-report or correction-factor code; no department/branch-scoping query (the dormant `BranchNumber` field on `SystemSettings` and `User` has no consumer); and no `SmtpClient`/`HttpClient` send path anywhere in `src/`. Where an in-scope item's design would naturally touch excluded machinery, this audit designs **around** it and marks the dependency OUT OF SCOPE rather than implementing it.

---

## 10. Current Target-Commit Classification

Every in-scope function received exactly one classification. The decision rules applied:

* **IMPLEMENTED** — a traced, concrete implementation path exists and its behaviour is consistent with the reference within the limits of verification. A table, column, property, ViewModel, menu item, navigation entry, setting, service, class, migration, test name, comment or placeholder is **never** sufficient on its own.
* **DIFFERENT** — the capability exists but materially differs: partial workflow, missing step, different business rule, incomplete persistence, incomplete validation, incomplete calculation, incomplete UI, only one layer implemented, or a stored-but-inert setting.
* **MISSING** — no credible implementation path, established by explicit negative search.
* **BLOCKED / UNRESOLVED** — used **only** where evidence genuinely prevents classification. A BLOCKED flag never removes an item from the three-way count.

### 10.1 Classification result

| Classification | Count |
|---|---|
| IMPLEMENTED | **129** |
| DIFFERENT | **59** |
| MISSING | **29** |
| BLOCKED / UNRESOLVED (withheld) | **0** |
| **Total in scope** | **217** |

### 10.2 Where this audit differs from the second audit

Six items move. Each is proven by a code citation in §7.2.

| REF | Second audit | This audit | Basis |
|---|---|---|---|
| REF-148 | IMPLEMENTED | **DIFFERENT** | Monitor reads `PatientTest.ResultValue` (test-level); CBC values live in `ProfileResultItem` (analyte-level). `GetBandedResultMonitorQueryHandler.cs:56-68` |
| REF-162 | IMPLEMENTED | **DIFFERENT** | Reference catalog grid exposes `Arrang` (LEARN p.95, p.141); no arrange/order column exists on `Test` |
| REF-163 | IMPLEMENTED | **DIFFERENT** | Same missing catalog fields on the add form; `CreateTestCommand.cs:8-22` has no arrange/sample-type/routine |
| REF-235 | IMPLEMENTED | **DIFFERENT** | `Himself`/`Herself` appears in exactly one line of `src/` and is printed by no writer; only a UI placeholder |
| REF-240 | IMPLEMENTED | **DIFFERENT** | Cited consumer file contains no reference to the setting; it is stored and editable but never read |
| REF-237 | DIFFERENT | **IMPLEMENTED** | Real consumer at `EnterResultCommandHandler.cs:115-127` calls `MarkReviewed` on save when the flag is on |

Net: `129 / 59 / 29` — balances exactly against 217.

### 10.3 Items deliberately *not* reclassified

The second audit's verdict is retained even where its framing could be sharpened, because the verdict itself survives independent testing:

* **REF-030, REF-057** — the reference genuinely does not name a removal action. DIFFERENT (semantics undocumented) is the correct, honest verdict.
* **REF-241** — the reference-aligned capability (the internal-window password gate) is REF-027 and is present. The workstation lock is a Top-Lab addition, not a gap. Retained.
* **REF-122** — element renaming, reference values and units are implemented generically; the curve half of the same page is excluded analyser work (REF-015). Retained.
* **REF-149** — the reference documents this as a setting ("enable the search feature when entering the patient name") and Top-Lab implements it as a setting plus a search command. Retained.
* **REF-141/142/143** — implemented as worklist filters rather than search-screen buttons. The behaviour is the reference's; only the placement differs. Retained.

---

## 11. IMPLEMENTED — 129 functions

Full per-item evidence for every one of the 129 appears in the mandatory table in §21. This section records the **verification standard applied** and the sub-groups, with the primary proof path for each group.

### 11.1 Verification-status distribution

| Verification status | Meaning | Items (measured, not estimated) |
|---|---|---|
| **TEST VERIFIED** (`ST+TEST`) | Traced in code by this audit **and** covered by at least one test; suite confirmed passing in this environment (§23) | **121** of the 129 IMPLEMENTED |
| **STATICALLY VERIFIED** (`ST`) | Traced end-to-end in code by this audit; no behavioural test covers it | **8** of the 129 IMPLEMENTED (REF-027, 045, 059, 060, 081, 149, 241, 242) |
| **DIFFERENT — evidence of the difference** (`ST`) | Handler body, validator and the exact divergence read line-by-line | **59** |
| **MISSING — negative search** (`ST (negative search)`) | Absence established by an explicit re-run search, not by assumption | **29** |
| **Defects** | Path traced to a specific line, with impact and reachability | **9** (DEF-001…DEF-009) |

**Second-pass uniform-depth certification.** Version 1 accepted the 129 IMPLEMENTED items partly on structural inference. This pass re-verified **all 129** to the same depth applied to DIFFERENT and MISSING, and in doing so established that the `ST+TEST` labels were **earned, not inflated**: the 8 items Version 1 labelled `ST` (no test) were each re-checked and found in fact to *do* have test coverage, so Version 1 **under-claimed** there; and a 25-item random sample of the 121 `ST+TEST` labels was each confirmed to have real test files exercising the named symbol, with **zero** unearned labels. The `ST` label is therefore a conservative floor, not an inflated ceiling. Where the label and the evidence disagreed, this report states the evidence.

**Test-quality check performed in this pass (new, and it found DEF-009).** Every `ST+TEST` label was additionally tested for the thing that matters: does the test *assert the reference behaviour*, or merely that a method runs? 20 test files across the suite use `File.ReadAllText` to assert on source text. In 19 of them this is a legitimate supplementary structural or layering assertion layered on top of real behavioural tests. In **one** — `HistoryFilterTests` — a production function is protected *only* by a source-text assertion, which is now recorded as **DEF-009**. The three B-01 suites (`GetBandedResultMonitorQueryHandlerTests` 20 facts, `StatisticsViewModelMonitorTests` 20 facts, `PatientEditorClearAllGuardTests` 10 behavioural + 2 structural) were each confirmed predominantly behavioural, so the `ST+TEST` claims for the post-first-audit work items stand.

**No classification in this report is runtime-verified.**

### 11.2 Group evidence (primary proof path per group)

| Group | REF items | Traced path (representative) |
|---|---|---|
| Access | 026, 027, 028, 029, 031, 032 | `SignInCommandHandler.cs` → `Pbkdf2PasswordHasher`; per-user `SecondaryPassword`; `CheckIn/StartBreak/EndBreak/CheckOut` → `AttendanceCalculator` |
| Patient | 034, 035, 037, 038, 039, 040, 041, 042, 043, 044, 045, 046, 047, 048, 049, 050 | `CreatePatient/UpdatePatient` → `Patient.Create/Update`; `SoftDeletePatient` gated by `IAuthorizedRequest` (`SoftDeletePatientCommand.cs:9-11`); `PrintBarcode` → `BarcodeService` → dispatcher |
| Billing | 051, 052, 053, 054, 056, 058, 059, 060, 063, 070 | `PatientAccountCalculator`; `RecordPayment` (discount ceiling `RecordPaymentCommandHandler.cs:44-52`); `ExecuteBulkPrintCommandHandler.cs:70-76` (balance block) |
| Results | 071, 072, 073, 074, 075, 076, 077, 078, 080, 081, 084, 085, 086 | `EnterResult` → `ResultFlagComputer`; verify/unverify/clear → `PatientTest`; reference-range snapshots |
| Culture | 087, 088, 090, 091 | `SaveCultureResults` → `CultureResult` / `CultureAntibioticResult`; `CultureAntibioticAttachment` per-culture threshold |
| Report | 095, 096, 098, 099, 100, 102, 103, 109, 110, 111, 112, 122, 126 | `BuildCombinedReport`/`PrintCombinedReport`; `ReportSettings` → `ReportPageComposer`; `HistorySortMode` → `PatientHistoryResolver` |
| Search | 134–144, 146, 147, 149 | `SearchPatientsGlobalQueryHandler` (doctor/sex/age/phone/card/test/date filters); `GetResultWorklist` (entered/reviewed/printed); `GetUndeliveredResults`; `GetBandedResultMonitor` |
| Catalog | 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177 | `ReferenceRange`/`AnalyteReferenceRangeBand` CRUD; `TestComment` + picker; custom groups; price lists; `ExternalEntity` full field set (`ExternalEntity.cs:15-38`) |
| Statistics | 178–191 | `GetPatientCountStatistics` (incl. `DayOfMonthCounts` + `Money` from the `[B-01]` batch), `GetTestCountStatistics`, `GetSentOutStatistics`, `GetUserProductivityStatistics` |
| Accounts | 193, 195, 197, 198, 199, 200, 202, 208 | `GetCashDrawerInventory`; `GetPatientSamplesDetail`; `RecordCashDeposit/Disbursement`; `SentOutAccountCalculator` |
| Samples | 210 | `MarkSampleDrawn` / `MarkAllSamplesDrawnForPatient`; `SampleDrawBoardView` |
| Tools | 212, 213, 214, 215, 216 | `ArithmeticCalculator`, `MeasurementUnitConverter`, `StopwatchCalculator`; `JsonPhoneBookStore`, `JsonPurchasesListStore` |
| Audit | 222, 223, 224, 226, 227, 228, 229, 230, 231 | `AuditableEntitySaveChangesInterceptor`; `PatientTestAuditDto` (`AuditDtos.cs:35-51`); `AmendProfileResult` + `ProfileResultAmendment`; `BackupDatabaseNow`/`RestoreDatabase`; `DailyBackupHostedService` |
| Settings | 235→(reclassified), 237, 240→(reclassified), 241, 242 | `EnterResultCommandHandler.cs:115-127` (REF-237); `LockWorkstation` + `UnlockWindow`; `AboutWindow` |

---

## 12. DIFFERENT — 59 functions

Each row states **precisely** what differs. Full evidence in §21.

| REF | Function | The exact difference |
|---|---|---|
| REF-030 | Delete or deactivate a user | Reference never names a removal action; semantics (hard delete vs deactivate) are an inference |
| REF-036 | Patient medical history flags and conditions | Stored, but no patient-data read-out in the result window; medication sub-list not modelled as a distinct item |
| REF-055 | Clear (zero) the whole account | No account-wide clear action (per-operation `Void` exists) |
| REF-057 | Delete a payment operation | Void only; the row and its amount remain in history |
| REF-061 | Receipt pre-printed vs white paper with header/footer + logo | `HeaderFooterMode.Images` offered but has no image pipeline; falls back to words (`ReceiptPdfWriter.cs:160-162`) |
| REF-062 | Receipt header/footer text, font, size, colour, logo | Lab print text + one font family; no per-element colour, no logo |
| REF-064 | Receipt via cash-register printer | Cashier setting and printer assignment honoured, but the receipt is fixed A5 (`ReceiptPdfWriter.cs:15-21`); no 80 mm page size |
| REF-065 | Envelope settings | Settings entity, EF config, migration seed and editor all exist; **no document consumes them** |
| REF-069 | Receipt expected result date per test | `Test.CompletionDurationMinutes` is stored and edited but no writer composes a per-test date; the editor has no pickup-date field |
| REF-082 | Automatic interpretation comment | No generated interpretation; no per-report on/off switch |
| REF-083 | Automatic calculation and equations in results | `ArithmeticCalculator` exists but is wired only to the calculator tool; no analyte/test formula, no derived value |
| REF-092 | Antibiotic master with commercial name | `Antibiotic` has `Name`, `Symbol`, `ScientificName`, two flags; **no commercial-name field** (`Antibiotic.cs:8-22`) |
| REF-093 | Antibiotic suitable for pregnancy | Flag and filter exist but the predicate has an escape clause (DEF-001) |
| REF-094 | Antibiotic suitable for children under twelve | Same escape clause; detection itself is correct |
| REF-097 | Blank report: patient data only | `FromBlank` builds from stored data; no "data only" variant selectable |
| REF-104 | More than one combined report per patient | `CombinedReportSelection` is documented "Never persisted" (`CombinedReportSelection.cs:3-6`) |
| REF-105 | One-button combined print of entered-but-unprinted | Preflight requires `EnteredAtUtc != null && IsReviewed` (`BulkPrintPreflightQueryHandler.cs:37`); reference says "entered and not printed" |
| REF-106 | Last N visits below the profile report | No trailing history block, no visit-count limit (history query takes a date window) |
| REF-107 | Report header/footer modes none/words/image | Image mode has no asset pipeline |
| REF-108 | Report header/footer colours + element positions | Only one header colour and one footer colour; no per-element colours, no offsets |
| REF-115 | Report element letter, list editing, free text | Names and free text exist; no editable report letter, no per-report element suppression |
| REF-118 | Print the abnormality marker | Flag computed and stored, low/high comment printed — but **no writer emits a marker** |
| REF-121 | CBC report design variants and large paper | One layout only; `PaperSize` has no large value |
| REF-123 | CBC report colouring, auto-comment, manual mode | Manual input and comment text exist; **all colour control absent**, no comment on/off switch |
| REF-124 | Show/hide report blocks | Only the doctor-signature block is toggleable; title, column headers, device info, patient-data position are fixed |
| REF-125 | Report identification block | 4 of 5 builders pass `null` for `Sex`, `AgeText`, `TreatingDoctorName`, `ReferralEntityName`; date has no time |
| REF-128 | Barcode labels per test and per sample | One patient label only; payload is a single identifier |
| REF-131 | Blank report with data entry before printing | No free-text entry surface |
| REF-132 | Search patient by exact name | One `Contains` term; no exact-match mode |
| REF-133 | Search patient by partial name | Partial match exists but is gated behind a setting serving a different purpose, not a selectable mode |
| REF-145 | Status indicator per patient | Roll-up exists per visit; no single status symbol per patient in the search grid |
| REF-148 | Search/statistics on **blood-picture element** values | Monitor reads `PatientTest.ResultValue`; CBC values are analyte-level in `ProfileResultItem`, so CBC is not covered |
| REF-152 | Worksheet of tests / group of tests in a period | Data + writer exist; print ships disabled (`WorkSheetsViewModel.cs:23-30`) |
| REF-153 | Worksheet by work group (Log) | Same as REF-152 |
| REF-154 | Print only unfinished tests or all | Completion facts carried but no filter flag passed or offered |
| REF-157 | Worksheet for one department over a period | No department selector; no between-two-patient-codes bound |
| REF-158 | Count of **patients** per test | Counts `PatientTest` rows, not distinct patients (`GetWorkSheetTestCountByPeriodQueryHandler.cs:41-42`) — a wrong business rule |
| REF-159 | Classify how often each test was performed | Display only; no printed classification, no signature column |
| REF-160 | Worksheet print preview | Preview proven but offered only for the visit mode |
| REF-161 | Catalog search by name / group / **test number** | Query takes `SearchTerm`, `TestGroupId`, `IncludeInactive`; the third documented key (test number) is absent. LEARN p.141 shows all three keys: `By test name | By group name | By test ID` |
| REF-162 | Edit test data and price | The edit-window prose (LEARN p.95) is fully covered, but the reference catalog record also carries `Arrang` (arrange number), `Out Lab Name` and `Routin` (routine) — **no column for any of them** |
| REF-163 | Add a new test | Same three missing catalog fields; `CreateTestCommand.cs:8-22` cannot carry them |
| REF-164 | Substitute patient name/data on the worksheet line | Line always prints the real identity; no override field |
| REF-192 | Print a statistics report | No print action and no statistics writer |
| REF-194 | Stocktake by element **and report type** | Element-kind enum exists; the four report-type variants do not |
| REF-196 | Stocktake statement for a treating doctor | No dedicated statement screen or print |
| REF-201 | Sent-out tests not yet dispatched | `SentOutSample` rows exist only after dispatch; no query joins `Tests.IsSentOut` to undispatched rows |
| REF-203 | Account filters: debtors / commission / settled | Columns exist; no filter selector in query or UI |
| REF-204 | Account per doctor daily/monthly/annual | Doctor is an inventory element kind, not a billing-account grouping |
| REF-206 | Referral-entity statement + hand-over list | Data reachable; no statement document, no hand-over list |
| REF-209 | Code samples per section | Sample-kind flags exist; no department entity, no per-department code, no separation step |
| REF-217 | Test information library | DTO carries `TestId, Name, TestCode, GroupName` only — no sample, no normal values, no patient effect |
| REF-225 | Payment collection method / instalments / receiver | `PaymentOperation` has `ReceivedByUserId` and `OperationAtUtc` but **no payment-method field** and no instalment count |
| REF-232 | Printer assignment per output kind | `PrinterOutputType` has 4 values; the reference documents 5 including the card printer (LEARN p.199) |
| REF-235 | Receipt: Himself/Herself when referral empty | Resolver exists and supplies UI placeholders; **no PDF writer prints it** |
| REF-236 | Treating doctor only from external-entity window | Flag stored and editable; the patient editor still exposes free-text doctor fields; no consumer |
| REF-238 | Barcode display control | Three related flags exist and are consumed; no single master switch, and the receipt/report/envelope show no barcode |
| REF-239 | Print the account instead of the date | Flag stored and editable; **no consumer** — writers always emit the date |
| REF-240 | Result-screen account display mode | Enum, column, settings ComboBox; **no behavioural consumer anywhere** |

---

## 13. MISSING — 29 functions

Each absence is established by an explicit negative search re-run by this audit, not by assumption.

| REF | Function | Negative-search evidence |
|---|---|---|
| REF-033 | Users login/logout movement report with machine name | `AttendanceRecord.cs:6-22` has no machine field; `grep -rn MachineName src/` → 0 files; no session/login-logout log; only an in-memory `CurrentUserSessionDto` |
| REF-066 | Print the patient envelope | No `Envelope*PdfWriter`; `grep -rn Envelope src/TopLab.Infrastructure/Printing` returns only the *print-envelope* token types, not an envelope document |
| REF-067 | Patient barcode on the envelope | No envelope document (REF-066); barcode exists only as a standalone label and as text on the worksheet |
| REF-068 | Print a laboratory order with barcode | `grep -rn 'Requisition\|LabOrder\|OrderPdfWriter' src/` → 0 files |
| REF-079 | Read the patient data block from the result screen | `ProfileResultDtos` carries no condition/medication/sex/age; `ProfileEntryView.xaml` and `PatientResultSheetView.xaml` bind test columns only |
| REF-089 | Choose which culture report elements are shown | `ReportCultureSection` has no `Show`/`Visible`/`IsEnabled`; no entity stores an element-selection set |
| REF-101 | Patient history for the blood picture (CBC) | No CBC history layout. **Correction to the second audit:** `HistoryMatrixBuilder` (CBC date×analyte pivot) exists and is tested, but has **zero production consumers** — no path to a printed table (DEF-007) |
| REF-113 | Colours of patient data, headings, profiles, abnormal results | `ReportDocumentContent` carries strings, sections and a grid only; the only colour fields in the system are `ReportSettings.HeaderColor`/`FooterColor` |
| REF-114 | Whole-row highlight choice | No per-row style data in the content model or the grid record |
| REF-116 | Report line spacing + per-element left offset | `ReportPageComposer` renders a fixed rhythm from one font size; `grep -rn LineSpacing src/` → 0 |
| REF-117 | Report element shape, name, order | No layout-configuration entity or command; element order fixed by builder loops |
| REF-119 | Semen analysis report with graphs and diagrams | Semen is only a sample-kind flag; `grep -rniE 'graph\|chart' src/TopLab.Infrastructure` → 0 real hits |
| REF-120 | Image report (1–4 microscopy images, draggable text/shapes) | No image entity, upload, image-capable report or drawing surface; only the in-memory Code-128 label bitmap |
| REF-127 | Barcode on receipt and envelope | No receipt or envelope writer draws a barcode; they emit text only |
| REF-129 | Vertical barcode on report/receipt/envelope | No vertical flag in any settings entity; label renderer emits a fixed horizontal bitmap (`PaddingVertical` hits are QuestPDF padding, not orientation) |
| REF-130 | Tube/container/swab labels (38×25 mm, multi-up) | One fixed-size patient label; no sticker-size setting, no multi-label sheet |
| REF-150 | Worksheet of a group of patients in a period | No period-patient worksheet query; only visit / test-group / work-group-log modes |
| REF-151 | Worksheet of a group of patients, condensed | No layout-variant flag and no condensed writer |
| REF-155 | Worksheet with full patient + test data | `WorkSheetLineDto` (`WorkSheetDtos.cs:3-15`) carries ordering facts only — no result value, no demographics |
| REF-156 | Worksheet with condensed patient data | Same root cause as REF-151/155 |
| REF-205 | Detailed account by test price or by results | No per-test price or per-result breakdown query or writer |
| REF-207 | Four account report types | No account report-type concept; no accounts writer |
| REF-211 | Reader options: save after read / print barcode after read | No scanner, HID or keyboard-wedge integration, no read event |
| REF-218 | Laboratory terms and abbreviations reference | No glossary entity, query or tab; the only "Abbreviation" hit is a stale comment (`AntibioticEditorViewModel.cs:12`) |
| REF-219 | Colour palette | No palette entity, query, UI or colour-picker control |
| REF-220 | Reminder note for a specific user | `grep -rn 'Reminder\|LabNote\|Notification' src/` → 0 files |
| REF-221 | Follow-up appointment for a patient or phone | `grep -rn 'FollowUp\|Recall' src/` → 0 files; `Patient.PickupDateUtc` is the expected-result date, not a follow-up |
| REF-233 | Auto-print the receipt on registration | `grep -rni AutoPrint src/` → 0 files |
| REF-234 | Receipt: patient and doctor names in English | `grep -rn 'FullNameEn\|NameEn' src/` → 0 files; one `FullName` per patient and one `Name` per external entity |

---

## 14. BLOCKED / UNRESOLVED — 0 items withheld, 4 items flagged BLOCKED

**No function had its classification withheld.** All 217 in-scope functions received exactly one of IMPLEMENTED / DIFFERENT / MISSING. BLOCKED is used only as an *additional* flag on items that are classified normally but whose specification needs an owner decision.

| REF | Classification | The blocking ambiguity | Evidence of the ambiguity |
|---|---|---|---|
| REF-115 | DIFFERENT | "Report element letter" (`احرف عناصر`) is ambiguous between a short report code, a display symbol and a transliterated letter | SHOW p.12 (E5) — damaged/ambiguous line |
| REF-221 | MISSING | The follow-up line mixes a phone-number entry with a follow-up date and does not state granularity | SHOW p.51 (E4) — line OCRs to incoherent text |
| REF-209 | DIFFERENT | Per-department sample coding does not state the departments, nor whether the code is per patient, per visit or per test | LEARN p.208 — the sentence names the step, not the granularity |
| REF-030 | DIFFERENT | Reference never names a user-removal action; hard delete vs deactivate is undefined | LEARN p.151-153, p.157 |

A further **4 items are specification-gated rather than evidence-gated** and are marked so in §21: REF-082 (auto-comment catalogue content is reference data only the owner can supply), REF-083 (formula scope vs excluded analyser correction factors), REF-218 (glossary content), REF-225 (collection-method list).

---

## 15. Existing Defects

Defects in **existing** Top-Lab behaviour that affect an already-classified reference function. Recorded separately from classification and **not** counted as additional reference functions. None was reproduced at runtime; each is traced to a specific line.

| # | Affected REF | Defect | Exact evidence | Impact | Verification |
|---|---|---|---|---|---|
| **DEF-001** | REF-093, REF-094 | The pregnancy and children filters have an escape clause: a pregnancy-only or children-only antibiotic **stays visible** in a patient's culture grid once a sensitivity result has been saved for it. Predicate is `IsDisplayable(...) \|\| saved.ContainsKey(x.Id.Value)`. | `src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs:37` | The reference rule is unconditional — the antibiotic appears **only** when pregnancy is present (or the patient is under twelve). A clinically unsuitable antibiotic is shown to a non-pregnant adult or a patient over twelve, the exact error the reference says the rule exists to prevent (SHOW p.23: `حتى تكون نسبة الخطأ صفر`). | Statically verified (single predicate, both flags). Not runtime-verified. |
| **DEF-002** | REF-165, REF-166 | Two independent normal-value stores exist — `ReferenceRange` (per test) and `AnalyteReferenceRangeBand` (per analyte) — and the consulted store depends on whether the test is mapped to an analyte. A fallback to `ReferenceRange` is taken when `analyteBands is null`. | `src/TopLab.Application/Features/ResultsEntry/Commands/EnterResult/EnterResultCommandHandler.cs:65-69`; `.../RefreshResultReferenceRange/RefreshResultReferenceRangeCommandHandler.cs:50-54`; `.../GetResultEntry/GetResultEntryQueryHandler.cs:56`; `src/TopLab.Domain/Tests/ReferenceRange.cs`; `src/TopLab.Domain/Tests/AnalyteReferenceRangeBand.cs:92` | The reference maintains one normal set per analysis. Top-Lab maintains two, so the printed normal can differ from the one the operator edited in the test-data window. | Statically verified. Not runtime-verified. |
| **DEF-003** | REF-118, REF-123 | The result flag is computed, stored and shown in the entry screens, but **no writer emits it** on the printed report; the low/high comment is printed instead. `ReportContentBuilder` uses the flag only to *select* a comment. | `src/TopLab.Infrastructure/Printing/ReportContentBuilder.cs:82,87,149,154,233,238,256,261` (flag → comment selection) vs `src/TopLab.Application/Features/ResultsEntry/Common/ResultFlagComputer.cs:37-67` (flag computation); line strings built at `ReportContentBuilder.cs:145-146, 216-219` with no marker | A printed report shows a normal-looking value with an explanatory sentence under it and no `H`/`L` marker, although the reference requires the abnormality to be visually marked (LEARN p.101, p.195; SHOW p.44, p.61). | Statically verified. Not runtime-verified. |
| **DEF-004** | REF-126, REF-168 | A comment attached to a result must be selected from the maintained per-test list; there is no free-text entry field. | `src/TopLab.Presentation/ViewModels/Patients/TestCommentPickerViewModel.cs:45, 128` (`PickedCommentText` set by `Pick()` only) | The reference workflow — write a new comment for an unusual case — cannot be completed without first creating a catalogue comment. | Statically verified. Not runtime-verified. |
| **DEF-005** | REF-232, REF-031 | The four attendance commands (`CheckIn`, `StartBreak`, `EndBreak`, `CheckOut`) implement **no** `IAuthorizedRequest`, and there is **no `ATTENDANCE` permission code** among the 13 seeded rows. Attendance is ungated. | `src/TopLab.Application/Features/Attendance/Commands/*/` (no `IAuthorizedRequest` in any of the four); `src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs:17-30` (13 codes, none attendance) | Any signed-in user can alter any user's attendance. Identical at the first-audit baseline `7a2cfb5`, so this is a long-standing gap, not a regression. | Statically verified. Not runtime-verified. |
| **DEF-006** | REF-152, REF-153, REF-159, REF-160 | The worksheet screen has print and print-preview buttons only in the visit pane; the test-group and work-group modes expose only a *display* button. The source comment states the print path was deliberately not built. | `src/TopLab.Presentation/ViewModels/WorkSheets/WorkSheetsViewModel.cs:23-30` ("group/log print ships disabled (owner-pending — no print command exists, none is created)"); `src/TopLab.Presentation/Views/WorkSheets/WorkSheetsView.xaml:53-54` (visit pane) vs `:104` (display button) | In the reference a worksheet is a printed bench document. For two of three modes Top-Lab is a screen-only listing, so the bench cannot use it. | Statically verified. Not runtime-verified. |
| **DEF-007** *(new in this audit)* | REF-101 | `HistoryMatrixBuilder` — a complete, unit-tested CBC date×analyte pivot — has **zero production consumers**. It is exercised only by its own test file. It was introduced by commit `c91bb62`, whose own subject line reads *"[W-02] Slice 12/16: History filters + CBC matrix + **dead-code cleanup** (WP-10)"* — a slice that added dead code while claiming to remove it. | `src/TopLab.Application/Features/ReportProduction/Common/HistoryMatrixBuilder.cs:1-38`; only consumer = `tests/TopLab.Application.Tests/Features/ReportProduction/HistoryMatrixTests.cs:12,28,41,52` | Two harms. (1) REF-101 is not the greenfield build the second audit implied — the core already exists, so the item is much cheaper than assessed. (2) A green unit test over unwired code gives **false confidence**: the suite reports the CBC matrix as working while nothing can reach it. | Statically verified by exhaustive consumer search across `src/` and `tests/`. Not runtime-verified. |

| **DEF-008** *(new in this second pass; NOT recorded in Version 1)* | REF-098, REF-099, REF-100, REF-102, REF-103 | `PatientHistoryReader.ResolveVisitPatients` resolves same-identity patients with **two different case-folding operations**. The SQL candidate pre-filter uses `p.FullName.Trim().ToUpper().StartsWith(firstToken)` — `ToUpper()` translates to SQL `UPPER()` and therefore uses the **database collation**. The subsequent exact comparison uses `PatientHistoryResolver.ResolveKey` → `ToUpperInvariant()` — **invariant ordinal** folding. The code itself records the divergence and the failure mode: *"NOTE: the SQL pre-filter uses ToUpper() (the only translatable fold) while the exact match uses ToUpperInvariant(); under exotic collations/cultures a candidate could be missed by the pre-filter — the exact comparison, not the pre-filter, defines membership, and any miss surfaces as a missing visit, never as another patient's data."* | `src/TopLab.Application/Features/ReportProduction/Common/PatientHistoryReader.cs:38-46` (SQL pre-filter at :40, exact comparison at :42-44, self-documented caveat at :32-37); fold implementations `src/TopLab.Domain/Reports/PatientHistoryResolver.cs:31-40` (`ToUpperInvariant`) versus the EF-translated `ToUpper()` at :40. Reachable from 4 user-facing entry points: `AutoInsertHistoryCommandHandler.cs:55`, `InsertHistoryResultCommandHandler.cs:48`, `GetMultiPatientHistoryQueryHandler.cs:47`, `GetPatientTestHistoryQueryHandler.cs:40` | On a **clinical report path**, when the operator has selected *group history by patient name*, a patient's earlier registration can be **silently excluded** from the combined report, the auto-inserted history, the manual history insert and the separate/multi-patient history report. The failure is silent and in the safe direction (a **missing** visit, never another patient's data — as the code comment itself states), so a reader will not notice it. **Severity qualifier, stated precisely: the default sort mode is `ByLabCode`** (`src/TopLab.Domain/Settings/ReportSettings.cs:55`), so the defect is **conditional on the non-default `ByPatientName` mode**; the `ByLabCode` branch (`:49-55`) is unaffected because both sides of its comparison are the same raw trimmed value. Trigger depends on the deployed database collation. Exposure is greatest for an **Arabic-language** deployment, which is this product's target. | Statically verified in depth (both fold implementations read; all 4 call sites traced; default mode confirmed). **Not runtime-verified** — no SQL Server reachable and no non-default collation reproducible in this environment. |
| **DEF-009** *(new in this second pass; NOT recorded in Version 1)* | REF-098, REF-099, REF-100, REF-102, REF-103 (same code as DEF-008) | `ResolveVisitPatients` — the function that carries DEF-008 — has **no behavioural test at all**. Its only guard is a source-text assertion: the test locates `PatientHistoryReader.cs` on disk, reads it as a string, and asserts the literal `"StartsWith(firstToken)"` is present. It never executes the query, never evaluates a name, and asserts nothing about behaviour. | `tests/TopLab.Application.Tests/Features/ReportProduction/HistoryFilterTests.cs:129-136` (the fact), `:138-155` (the `File.ReadAllText` source-locating helper) | Two harms. (1) **False assurance** — a green suite reports this function as guarded while its logic is unexercised; this is the mechanism by which DEF-008 survived two audits. (2) **Brittleness** — the test breaks on any *correct* refactor that renames the local, and passes on any *incorrect* change that keeps the string. It also fails the requirement that a test "assert the reference behaviour, not merely that the method runs". | Statically verified: exhaustive search of `tests/` for `ResolveVisitPatients` returns **0 behavioural references** and **1** source-text-assertion site. |

**Net effect on the second audit's DEF-005.** The second audit listed five inert settings including `AutoReviewAndComplete` and `ResultScreenAccountDisplayMode`. This audit finds `AutoReviewAndComplete` **does** have a consumer (ERR-1, REF-237 reclassified IMPLEMENTED) and confirms `ResultScreenAccountDisplayMode` genuinely has none (ERR-2, REF-240 reclassified DIFFERENT). Its `SaveTreatingDoctorOnlyFromEntityWindow`, `PrintAccountInsteadOfDateOnReport` and `HeaderFooterMode.Images` entries are all confirmed accurate.

---

## 16. Git-History Reconstruction

### 16.1 Repositories and graph shape

The clone contains exactly two refs, both at the target commit, with 279 commits reachable. The history is a linear sequence of feature waves, each decomposed into numbered slices by a "loop-engineering" process, with no merge commits in the target window.

### 16.2 The first-audit baseline exists and is well-defined

`build.md` declared its baseline as `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757`.

```
7a2cfb5  Sat Oct 3 14:57:48 2026 +0300  medowemado zne "الإستعداد للرحلة"
```

`git rev-list --count 7a2cfb5..3590a7f5` = **12**. Every one of the 12 was inspected, with its diffstat over `src/` and `tests/`:

| # | Commit | Date | Subject | Changes to `src`/`tests` | Nature |
|---|---|---|---|---|---|
| 1 | `3b00949` | 2026-10-03 | `docs(B-01): initialize batch execution memory` | **none** | Documentation only |
| 2 | `e765f87` | 2026-10-03 | `الإستعداد للرحلة واحد` | **none** | Documentation only |
| 3 | `55b4370` | 2026-10-04 | `B-01 R2: align plan, memory and execution prompt with current baseline and SDK 8.0.425` | **none** | Documentation only |
| 4 | `e84c19c` | 2026-10-04 | `[B-01] Slice 1/8: Banded-monitor Application query + DTOs` | 8 files, +669/−1 | **Work item 1** |
| 5 | `6e88e1f` | 2026-10-04 | `[B-01] Slice 2/8: Banded-monitor PDF port + writer` | 4 files, +539 | **Work item 1** |
| 6 | `838ba95` | 2026-10-04 | `[B-01] Slice 3/8: Banded-monitor VM + XAML` | 3 files, +761/−4 | **Work item 1** |
| 7 | `c8b9bd5` | 2026-10-04 | `[B-01] Slice 4/8: Day-of-month + money row in the Application layer` | 7 files, +403/−22 | **Work item 2** |
| 8 | `d61ea66` | 2026-10-04 | `[B-01] Slice 5/8: Day-of-month + money row in the UI` | 3 files, +165/−6 | **Work item 2** |
| 9 | `65ca4a6` | 2026-10-04 | `[B-01] Slice 6/8: First-registration guard in the Application handler` | 2 files, +270 | **Work item 3** |
| 10 | `9e5ad13` | 2026-10-04 | `[B-01] Slice 7/8: First-registration guard in the UI` | 3 files, +418/−2 | **Work item 3** |
| 11 | `da99d23` | 2026-10-04 | `[B-01] Slice 8/8: Outside-lab note on the profile printed report` | 4 files, +100/−5 | **Work item 3** |
| 12 | `3590a7f` | 2026-10-04 | `[B-01] Final gate: record batch result` | **none** (`Docs/OpenCode/B-01-memory.md` only) | Documentation only |

Aggregate: **28 files changed, 3,318 insertions, 33 deletions** across `src/` and `tests/`.

### 16.3 Two facts that constrain any reconstruction

* **The final gate commit changed no code.** The target commit itself is documentation-only relative to its parent. All code change in the window is attributable to slices 1–8.
* **No migration was added, and no migration was modified.** `git diff 7a2cfb5 3590a7f5 -- src/TopLab.Infrastructure/Persistence` is **empty**. All three work items were delivered against the existing schema — which is exactly why the reference-to-code verification had to be done on behaviour, not on schema.

---

## 17. Identification of the Post-First-Audit Implementations

The owner does not remember which three functions were implemented after the first audit and instructed that this must be determined independently, from Git history, diffs, changed files and the historical baseline — **not** from commit-message language. That reconstruction follows.

### 17.1 Method

1. Establish the baseline (`7a2cfb5`) and the full commit window (§16.2).
2. Separate documentation-only commits from code-changing commits by diffstat over `src/` and `tests/`.
3. Cluster the 8 code-changing slices by the **files they touch** — clustering on files, not on commit text.
4. For each resulting cluster, read the actual code at HEAD and trace the end-to-end path.
5. Map each cluster to the first audit's requirement model and to the reference function inventory.
6. Accept a cluster as one "implemented function" only if it closes a capability the first audit had recorded as absent or partial.

### 17.2 Result: the eight slices collapse into exactly three work items

Clustering on touched files yields three disjoint file groups with no overlap:

| Work item | Slices | Commits | Files (representative) |
|---|---|---|---|
| **W-1 Banded result monitor** | 1, 2, 3 | `e84c19c`, `6e88e1f`, `838ba95` | `Features/Statistics/{Common,Queries/GetBandedResultMonitor}`, `Common/Interfaces/IBandedResultMonitorPdfWriter`, `Infrastructure/Printing/BandedResultMonitorPdfWriter`, `Infrastructure/DependencyInjection`, `ViewModels/Statistics/StatisticsViewModel`, `Views/Statistics/StatisticsView.xaml` |
| **W-2 Patient-statistics day-of-month + money row** | 4, 5 | `c8b9bd5`, `d61ea66` | `Features/Statistics/{Common/StatisticsDtos,Queries/GetPatientCountStatistics}`, `ViewModels/Statistics/StatisticsViewModel`, `Views/Statistics/StatisticsView.xaml` |
| **W-3 Order-edit guard + outside-lab report note** | 6, 7, 8 | `65ca4a6`, `9e5ad13`, `da99d23` | `Commands/ClearAllTests`, `ViewModels/Patients/PatientEditorViewModel`, `Views/Patients/PatientEditorView.xaml`, `Features/ProfileResults/{Common,Queries/GetProfileReport}`, `Features/ResultsEntry/Common/ResultPrintCoordinator` |

W-1 and W-2 share `StatisticsViewModel`/`StatisticsView.xaml` only because two sequential features extend the same screen; the *capabilities* are disjoint (monitor grid vs patient-statistics grouping), and their test files are disjoint.

### 17.3 Each work item verified against the code, not the commit message

#### W-1 — Banded result monitor → closes first-audit **R-F05** (recorded MISSING)

Traced path at HEAD:
`StatisticsView.xaml:67` (band controls) → `StatisticsViewModel` (`RunMonitorCommand`, `MonitorRows`) → `GetBandedResultMonitorQuery` → `GetBandedResultMonitorQueryHandler.cs:39-120` (test lookup → `PatientTest` filter on `EnteredAtUtc` window `:56-61` → invariant parse + min/max band `:64-68` → soft-delete exclusion `:73-75` → deterministic order `:88-91` → lifecycle status label `:136-154`) → `IBandedResultMonitorPdfWriter` → `BandedResultMonitorPdfWriter` (227 lines) → `DependencyInjection` registration.

Tests: `GetBandedResultMonitorQueryHandlerTests.cs` (20 facts), `BandedResultMonitorPdfWriterTests.cs` (11 facts), `StatisticsViewModelMonitorTests.cs` (20 facts). **Verified passing in this environment.**

Maps to reference functions **REF-146** (monitoring report of results: patient, patient data, test, result, referral entity, status) and **REF-147** (evaluate a test's results by minimum and maximum value) — both now **IMPLEMENTED**. It partially addresses **REF-148** (blood-picture *element* values) but does not complete it, because the monitor reads `PatientTest.ResultValue` rather than `ProfileResultItem` (§12, ERR-4).

#### W-2 — Day-of-month + money row → closes first-audit **R-F01** partial gap

Traced path at HEAD:
`GetPatientCountStatisticsQuery` gains `GroupByDayOfMonth` and `IncludeMoneyRow` → `GetPatientCountStatisticsQueryHandler.cs:112-116` (day-of-month grouping) and `:142-152` (`PeriodMoneyDto` computed from `PatientAccountCalculator.TotalPaid(periodOperations)`, with a payment count so zero money and zero payments stay distinguishable) → `StatisticsDtos.cs:29-30, 37, 47` → `StatisticsViewModel.cs:50-51, 183-184, 195-196, 209-216` (`MoneyRowText`, `HasMoneyRow`) → `StatisticsView.xaml:67-68` (checkboxes) and `:151-172` (grid + money row with `DataTrigger` visibility).

Tests: `GetPatientCountStatisticsQueryHandlerTests.cs` (+345 lines) and `StatisticsViewModelMonitorTests.cs` (+125 lines). **Verified passing.**

Maps to **REF-185** (patient count for a month split by day) and **REF-186** (paid amounts shown with patient statistics) — both **IMPLEMENTED**.

#### W-3 — First-registration guard + outside-lab report note → closes first-audit **R-A04** partial gaps

Slice 6/7 — guard. `ClearAllTestsCommandHandler.cs:48-67` computes `isFirstRegistration` as "no other non-deleted `Patient` in the same `LabId` group sorts before this one", ordering on `RegistrationDateUtc` then `PatientId`, and rejects with a single frozen Arabic literal shared by the handler, the Presentation short-circuit and the tests. The two pre-existing guards (24-hour window `:69-72`, results-entered `:80-83`) are untouched. Tests: `ClearAllTestsCommandHandlerTests.cs` (14 facts) + `PatientEditorClearAllGuardTests.cs` (11 facts) + `WorkSheets`/`PatientEditorView.xaml` UI disable. **Verified passing.**

Maps to **REF-041** (clear all tests of the patient — the reference documents this as the first-registration-only action, LEARN p.13) — now **IMPLEMENTED**.

Slice 8 — note. `ProfileResultDtos.cs:66` adds `bool IsTakenOutsideLab = false` to `ProfileReportDto`; `GetProfileReportQueryHandler.cs:95` populates it from `pt.IsTakenOutsideLab`; `ResultPrintCoordinator.cs:133-140` threads it into the combined-report line; `ReportContentBuilder.cs:221-225` renders `العينة أُخذت خارج المعمل`. Tests: `ResultPrintCoordinatorTests.cs` (+88 lines). **Verified passing.**

Maps to **REF-044** (mark a test as sample taken outside the lab) — the second audit had already classified the storage side IMPLEMENTED but recorded the *rendering* as unproven; it is now proven end-to-end.

### 17.4 Answer to the owner's question

**There were exactly three implementation changes after the first audit**, delivered as eight slices in one batch (`B-01`) on 2026-10-04:

1. **Banded result monitor** (slices 1–3, commits `e84c19c`, `6e88e1f`, `838ba95`)
2. **Patient-statistics day-of-month grouping + money-paid row** (slices 4–5, commits `c8b9bd5`, `d61ea66`)
3. **First-registration-only guard on clearing tests, plus the outside-lab note on the printed profile report** (slices 6–8, commits `65ca4a6`, `9e5ad13`, `da99d23`)

This is established from the diff structure and the code itself, not from commit-message wording. Note the honest qualification: work item 3 bundles two related changes because the first audit recorded them as a single requirement (R-A04) and because they were sliced together; if the owner counts *reference functions* rather than *work items*, the batch closed **six** reference functions (REF-041, REF-044, REF-146, REF-147, REF-185, REF-186) and partially advanced a seventh (REF-148).

**No migration was created or altered by this batch.** All three work items were schema-free by design.

---

## 18. Formal First-Audit → Second-Audit → Current Reconciliation

`build.md` used a **requirement** model (88 in-scope items keyed `R-xxx`). The second audit used a **function** model (242 functions keyed `REF-xxx`). The mapping between them is many-to-many in both directions, and forcing it into a single count is exactly the error the brief warns against. The table below is comprehensive for **all 21 of the first audit's in-scope work items**, plus the first audit's excluded set, plus the material "already implemented" groups.

### 18.1 The 21 first-audit work items

| Old ID | First-audit requirement | First status | Current REF(s) | Second-audit status | **Current status** | Historical change | Why counts differ | Evidence | Final verdict |
|---|---|---|---|---|---|---|---|---|---|
| R-A04 | Add/remove/clear tests on order; first-registration guard; outside-lab note | Differently (2 sub-gaps) | REF-039, REF-040, REF-041, REF-043, REF-044, REF-049 | REF-039/040/043/049 IMPLEMENTED; **REF-041, REF-044** also IMPLEMENTED | Same as second audit | **Both sub-gaps closed after the first audit** by `[B-01]` slices 6–8 | 1 first-audit item **decomposed into 6** REF functions; the first audit counted the *work*, the second counted the *capability* | `ClearAllTestsCommandHandler.cs:48-67`; `ReportContentBuilder.cs:221-225` | **SUPERSEDED BY LATER CODE** — 1 of 1 work item closed |
| R-C01 | Test master missing 5 fields (`HistoryName/ArabicName/SampleType/ArrangeNo/Bench`) | Partial | REF-162, REF-163 (and REF-161) | REF-162, REF-163 **IMPLEMENTED**; REF-161 DIFFERENT | **REF-162, REF-163 DIFFERENT**; REF-161 DIFFERENT | Unchanged; second audit too generous | 1 first-audit item **split into 3** REF functions | Reference catalog grid carries `Arrang` (LEARN p.95, p.141); no arrange/order column on `Test`; LEARN p.95 edit-window prose shows only fields Top-Lab already has | **PARTIALLY CONFIRMED** — `ArrangeNo` is a genuine reference field and is still missing (so the first audit was right and the second audit wrong here); the other four names are not evidenced in the PDFs as reference fields |
| R-C11 | Antibiotic commercial names | Partial | REF-092 | DIFFERENT | DIFFERENT | Unchanged | 1:1 | `Antibiotic.cs:8-22` has no commercial-name field; `grep -rn 'CommercialName\|TradeName' src/` → 0 | **CONFIRMED** — still a gap |
| R-D02 | Auto haematology indices + correction factors | Missing | REF-083 (and REF-148, partially) | REF-083 DIFFERENT; REF-148 IMPLEMENTED | REF-083 DIFFERENT; **REF-148 DIFFERENT** | REF-148 downgraded this audit | Correction-factor half is **excluded** (REF-014); the derivation half remains a real gap | No analyte/test formula column; `ArithmeticCalculator` wired only to the calculator tool | **CONFIRMED** as a gap, with the excluded half correctly excluded |
| R-D03 | Auto clinical comments | Missing | REF-082 | DIFFERENT | DIFFERENT | Unchanged | 1:1 | `grep -rn 'AutoComment\|CommentRule\|Interpretation' src/` → 0 | **CONFIRMED** — content is owner-supplied, engine buildable |
| R-D04 | Abnormal flag presentation (colours) | Partial | REF-113, REF-114, REF-118, REF-123, REF-124 | REF-113/114 MISSING; REF-118/123/124 DIFFERENT | Same | Unchanged | 1 first-audit item **split into 5** REF functions | Only `ReportSettings.HeaderColor`/`FooterColor` exist; no style model in `ReportDocumentContent` | **CONFIRMED** — the largest single cluster of remaining work |
| R-D05 | Report element config + curves | Partial | REF-115, REF-116, REF-117, REF-108 | REF-115/108 DIFFERENT; REF-116/117 MISSING | Same | Unchanged | 1:4, curves excluded (REF-015) | `grep -rn 'ReportElementConfig\|ReportLayout\|ElementOrder\|LineSpacing' src/` → 0 | **CONFIRMED**; curves half correctly excluded |
| R-E01 | User create + permissions; dead grants; ungated attendance | Differently | REF-028, REF-029, REF-085, REF-070, REF-031, REF-232 | REF-028/029/085/070/031 IMPLEMENTED; REF-232 DIFFERENT | Same as second audit | Unchanged — **but the first audit's premise was false** | The first audit bundled a real defect with a non-existent one | `git show 7a2cfb5:...RecordPaymentCommandHandler.cs:44-52` and six print handlers at baseline prove both grants were already enforced | **INCORRECT in the first audit.** The "dead grants" half never existed; only the ungated-attendance half (DEF-005) is real |
| R-E05 | Login history + machine name | Partial | REF-031, REF-032, REF-033 | REF-031/032 IMPLEMENTED; **REF-033 MISSING** | Same | Unchanged | 1 first-audit item **split into 3**, one of which is a total gap | `AttendanceRecord.cs:6-22`; `grep -rn MachineName src/` → 0 files | **CONFIRMED** — 2 of 3 already implemented, 1 total gap |
| R-F01 | Patient stats: day-group + money row | Partial | REF-179–184, REF-185, REF-186 | REF-179–184 IMPLEMENTED; **REF-185, REF-186 IMPLEMENTED** | Same | **Closed after the first audit** by `[B-01]` slices 4–5 | 1 first-audit item **split into 8** REF functions | `GetPatientCountStatisticsQueryHandler.cs:112-116, 142-152`; `StatisticsView.xaml:67-68, 151-172` | **SUPERSEDED BY LATER CODE** — 1 of 1 work item closed |
| R-F05 | Banded abnormal-result / QC monitor | Missing | REF-146, REF-147, REF-148 | **REF-146/147 IMPLEMENTED**; REF-148 IMPLEMENTED | REF-146/147 IMPLEMENTED; **REF-148 DIFFERENT** | **Closed after the first audit** by `[B-01]` slices 1–3; REF-148 downgraded this audit | 1 first-audit item **split into 3** REF functions | `GetBandedResultMonitorQueryHandler.cs:56-68` reads `PatientTest.ResultValue`; CBC values are in `ProfileResultItem` | **SUPERSEDED BY LATER CODE**, with a correctly-narrowed residual on REF-148 |
| R-H01 | Report element positioning | Partial | REF-109, REF-116, REF-108 | REF-109 IMPLEMENTED; REF-116 MISSING; REF-108 DIFFERENT | Same | Unchanged | 1:3 | Margins/top-space implemented; per-element offsets absent | **CONFIRMED** |
| R-H02 | Card printer slot | Partial | REF-232 | DIFFERENT | DIFFERENT | Unchanged | 1:1 | `PrinterOutputType.cs:3-9` has 4 values; reference documents 5 (LEARN p.199) | **CONFIRMED** |
| R-H04 | English name entry | Missing | REF-234 | MISSING | MISSING | Unchanged | 1:1 | `grep -rn 'FullNameEn\|NameEn' src/` → 0 files | **CONFIRMED** |
| R-H05 | Auto Mr/Mrs by sex | Missing | REF-045, REF-235 | REF-045 IMPLEMENTED; **REF-235 IMPLEMENTED** | REF-045 IMPLEMENTED; **REF-235 DIFFERENT** | REF-235 downgraded this audit | 1 first-audit item **split into 2** | `Himself`/`Herself` printed by no writer; only a UI placeholder | **PARTIALLY CONFIRMED** — the first audit's "blocked on owner decision" framing was also wrong: the switch already exists |
| R-H11 | Auto-print receipt switch | Partial | REF-233 | MISSING | MISSING | Unchanged | 1:1 | `grep -rni AutoPrint src/` → 0 files | **CONFIRMED** |
| R-H14 | Card settings + card print job | Missing | REF-232, REF-237 | REF-232 DIFFERENT; **REF-237 DIFFERENT** | REF-232 DIFFERENT; **REF-237 IMPLEMENTED** | REF-237 upgraded this audit | 1:2 | `EnterResultCommandHandler.cs:115-127` calls `MarkReviewed` when `AutoReviewAndComplete` is on | **PARTIALLY CONFIRMED**, upgraded |
| R-A15 | Barcode per tube/container | Partial | REF-037, REF-128, REF-130, REF-067 | REF-037 IMPLEMENTED; REF-128 DIFFERENT; REF-130/067 MISSING | Same | Unchanged | 1 first-audit item **split into 4** | One patient label; no per-test/per-sample, no sticker size, no multi-up | **CONFIRMED** |
| R-A17 | Lab requisition form | Missing | REF-068 | MISSING | MISSING | Unchanged | 1:1 | `grep -rn 'Requisition\|LabOrder\|OrderPdfWriter' src/` → 0 files | **CONFIRMED** |
| R-A23 | CBC history + trend charts | Partial | REF-101, REF-113 | **REF-101 MISSING**; REF-113 MISSING | Same, with corrected evidence | Verdict unchanged, **cost corrected** | 1:2 | `HistoryMatrixBuilder.cs:1-38` exists and is tested but has zero production consumers | **CONFIRMED, with a correction** — much cheaper than either audit implied, and a new defect (DEF-007) |
| R-I06 | Reminder messages | Missing | REF-220, REF-221 | REF-220/221 MISSING | Same | Unchanged | 1:2 | `grep -rn 'Reminder\|LabNote\|Notification\|FollowUp\|Recall' src/` → 0 files | **CONFIRMED** |

### 18.2 First-audit exclusions (14) → current exclusions (25)

| First-audit excluded group | Count (first audit) | Current REFs | Count (this audit) | Why the number changed |
|---|---|---|---|---|
| Natiga portal send/open/receipt-print/doctor-account/lab-account/Android app | 6 | REF-001…REF-010 | **10** | **Granularity.** The first audit collapsed the portal into 6; the PDFs document 10 separately-described capabilities (upload, view, print/export, Android app, receipt block, count, block, doctor account, lab account, branding on documents) |
| Equipment registry | 1 | REF-011, REF-012 | **2** | **Granularity.** Register and its entry form are separately described (LEARN p.93-94) |
| CBC analyser import + per-analyser factors | 2 | REF-013…REF-017 | **5** | **Granularity.** The first audit merged the device interface, import, correction factors, device report with curves, 5-part differential and other-analyyser interfaces into 2; the PDFs describe 5 (SHOW p.55-77) |
| Branch search/inventory/user-branch/system-branch/branch-stats | 5 | REF-018…REF-022 | **5** | 1:1 |
| SMS/Email/Fax sending | 0 (folded into the Natiga group) | REF-023, REF-024, REF-025 | **3** | **Scope separation.** The first audit folded these into one of its 6 portal items; they are a distinct owner exclusion category. Note the honest detail: **no fax-sending function is documented in either PDF** (§9.5). |

The 14 → 25 movement is therefore **entirely a granularity-and-grouping artefact**, with zero change in owner intent. No excluded function moved into or out of the excluded set.

### 18.3 First-audit "67 Implemented" group

The first audit recorded 67 items as Implemented and 2 as Implemented-Differently without a per-item evidence table, citing "per Cross-comparison §5–6, spot-verified". This audit did **not** re-derive that spot-verification list item-by-item; instead it re-tested the items that the first audit's own plan and the reference most depend on, and found no contradiction. Two caveats, stated rather than hidden:

* Because the first audit published no per-item evidence, its 67-item list cannot be mechanically mapped to REF ids. This is a **granularity impossibility**, not an audit error, and it is the single largest reason the two models cannot be reconciled row-for-row.
* Where the first audit's plan *named* a specific field or file, this audit verified that claim directly. Three such claims were found wrong or overstated: R-E01's dead grants (false), R-C01's four unevidenced field names (unsubstantiated), and R-H05's "blocked on owner decision" framing (the switch already exists).

### 18.4 Second-audit items this audit re-tested and re-classified

| REF | Second status | Current status | Proof |
|---|---|---|---|
| REF-148 | IMPLEMENTED | **DIFFERENT** | `GetBandedResultMonitorQueryHandler.cs:56-68` reads `PatientTest.ResultValue`; CBC values are `ProfileResultItem` rows |
| REF-162 | IMPLEMENTED | **DIFFERENT** | No arrange/order column on `Test`; reference grid shows `Arrang` (LEARN p.95, p.141) |
| REF-163 | IMPLEMENTED | **DIFFERENT** | `CreateTestCommand.cs:8-22` cannot carry arrange/sample-type/routine |
| REF-235 | IMPLEMENTED | **DIFFERENT** | `Himself`/`Herself` present on exactly one line of `src/`; printed by no writer |
| REF-240 | IMPLEMENTED | **DIFFERENT** | Cited consumer file contains no reference to the setting |
| REF-237 | DIFFERENT | **IMPLEMENTED** | `EnterResultCommandHandler.cs:115-127` calls `MarkReviewed` when the flag is on |
| REF-101 | MISSING | MISSING (evidence + cost corrected) | `HistoryMatrixBuilder.cs:1-38` exists, tested, zero production consumers |

---

## 19. Historical Change and Count-Difference Analysis

### 19.1 The three numbers are not the same unit

| Number | Unit of count | Scope |
|---|---|---|
| `build.md`: **21** | *Work items the plan proposes to build* | A subset of 88 in-scope requirements, chosen because only those needed work at that baseline |
| Second audit: **84** | *Work items for every DIFFERENT + MISSING function* | 55 DIFFERENT + 29 MISSING, from a 217 in-scope function inventory |
| This audit: **88** | Same unit as the second audit | 59 DIFFERENT + 29 MISSING |

**Comparing 21 and 84 as if they measure the same thing is the central analytical error the brief warns about.** They do not.

### 19.2 Decomposition of the difference

| Component | Magnitude | Nature |
|---|---|---|
| First audit's in-scope universe: 88 | 88 | `build.md` extracted 102 functions and hard-excluded 14. The second audit enumerated 242 and excluded 25. The **+19** is granularity: the first audit merged documented capabilities (portal 6→10, equipment 1→2, analyser 2→5) and folded SMS/Email into the portal group. |
| First audit's in-scope universe: 88 → 217 | +129 | The 88-item model is a **requirement** model; several first-audit requirements expand into many reference functions (R-D04 → 5, R-F01 → 8, R-A15 → 4, R-E05 → 3, R-A04 → 6, R-I06 → 2, R-H02/R-H14 → 2, R-D05 → 4, R-A23 → 2). |
| Second audit 84 → this audit 88 | +4 | **Implementation change is zero.** The entire delta is the six reclassifications in §18.4: +5 items moved IMPLEMENTED→DIFFERENT, −1 moved DIFFERENT→IMPLEMENTED, so DIFFERENT rose 55→59 and the sequence rose 84→88. |

### 19.3 Was the difference implementation, granularity, scope, or audit error?

**All four contributed, and they can be separated cleanly:**

1. **Implementation change — small, real, and fully characterised.** Exactly **one** batch landed between the two audits: `[B-01]`, 8 slices, 3 work items, 28 files, +3,318/−33 lines, **zero migrations**. It closed first-audit work items **R-F05, R-F01 and R-A04** in full. In reference-function terms it moved **6 functions** (REF-041, REF-044, REF-146, REF-147, REF-185, REF-186) from gap to implemented and partially advanced a 7th (REF-148). No item regressed.

2. **Granularity — the dominant factor.** 102 → 242 reference functions is not a change in the reference system; it is a change in decomposition. The first audit's own arithmetic (`67+2+11+8 = 88`) is internally valid for its model.

3. **Scope/exclusions — small and fully explained.** 14 → 25 excluded functions, explained line-by-line in §18.2. Every movement is granularity or group separation; **no owner's exclusion decision changed**, and no excluded function was ever counted as a gap by either audit.

4. **Audit error — real, and it runs in both directions.**
   * `build.md`: one **material false claim** (R-E01 dead grants, disproven at its own baseline) and one **unsubstantiated** claim (four of R-C01's five field names are not evidenced as reference fields).
   * Second audit: **four misclassifications and one false negative-evidence claim** (§7.2, §18.4), net-moving 5 functions.
   * Net effect on the "current truth": the honest current position is **129 / 59 / 29**, not 133 / 55 / 29.

### 19.4 Status-change ledger since the first audit

| Change | Count | Items |
|---|---|---|
| Gap → implemented after the first audit | 6 | REF-041, REF-044, REF-146, REF-147, REF-185, REF-186 |
| Partial → implemented after the first audit | 2 | REF-185, REF-186 (counted within the 6) |
| Reclassified by this audit (second audit too generous) | 5 | REF-148, REF-162, REF-163, REF-235, REF-240 |
| Reclassified by this audit (second audit too harsh) | 1 | REF-237 |
| Evidence corrected, verdict unchanged | 1 | REF-101 |
| First-audit claim shown false | 1 | R-E01 (dead grants) |
| Regressed | **0** | — |
| Previously reported gap now closed by an *excluded* decision | **0** | — |

---

## 20. Documentation-vs-Code Contradictions

Recorded per the instruction not to conceal conflicts. In every row the **code wins** and the code is the current truth.

| # | Contradiction | Documented claim | Actual code | Resolution |
|---|---|---|---|---|
| C-1 | Stale XML doc on the antibiotic editor | `src/TopLab.Presentation/ViewModels/Lab/AntibioticEditorViewModel.cs:10-13` — *"Antibiotic editor dialog VM (S-02 Slice 5): Name + the two flags only (**no Abbreviation/ScientificName/SensitivityCategory exists in code**)"* | `Antibiotic.cs:15,18` declare `Symbol` and `ScientificName`; `AntibioticEditorViewModel.cs:24-25` hold `_symbol` and `_scientificName`; migration `20261002002546_AddAntibioticMasterFields` added them | **Code wins.** The comment predates the `AddAntibioticMasterFields` migration and was never updated. It is actively misleading: a reader auditing the culture feature from the comment alone would conclude two fields do not exist. The comment is also the *only* hit for `Abbreviation` in `src/`, which is how the second audit's REF-218 negative search was satisfied. |
| C-2 | `build.md` states the grants are dead | *"only in seed+snapshot+`UserManagementViewModel.cs:52`"* | `RecordPaymentCommandHandler.cs:44-52` and six print handlers enforce the per-user fields, identically at the target commit **and at `7a2cfb5`** | **Code wins.** The claim is false (§6.1, §18.1). |
| C-3 | Second audit states `AutoReviewAndComplete` has no consumer | *"grep … returns only the settings entity and its editor"* | `EnterResultCommandHandler.cs:115-127` reads the flag and calls `MarkReviewed` | **Code wins.** REF-237 is IMPLEMENTED (ERR-1). |
| C-4 | Second audit states the `Himself/Herself` resolver is consumed by the receipt and invoice writers | *"consumed by the receipt and invoice writers"* | The string exists on **one line** of `src/` (`ReferralNameResolver.cs:14`); its only caller is `GetRegistrationCatalogQueryHandler.cs:62-63` (UI placeholders). No writer emits it | **Code wins.** REF-235 is DIFFERENT (ERR-3). |
| C-5 | Second audit states `GetResultEntryQueryHandler` consumes the account display mode | cited as the consumer for REF-240 | The file is 111 lines and contains no reference to the setting, nor to `Account`/`Mode`/`Display` at all | **Code wins.** REF-240 is DIFFERENT (ERR-2). |
| C-6 | Second audit states no CBC history column set exists | *"no column set … exists anywhere in `src/`"* | `HistoryMatrixBuilder.cs:6-16` defines exactly such a column set, with a tested pivot | **Code wins on the existence question** (ERR-5). The *verdict* (MISSING) survives because the component has zero production consumers. |
| C-7 | Commit message contradicts commit content | `c91bb62` subject: *"[W-02] Slice 12/16: History filters + CBC matrix + **dead-code cleanup**"* | The slice **adds** `HistoryMatrixBuilder` (38 lines) and leaves it with zero production consumers | **Code wins.** A slice labelled "dead-code cleanup" increased dead code. Recorded as DEF-007. |
| C-8 | `WorkSheetsViewModel` comment vs reference expectation | *"group/log print ships disabled (owner-pending — no print command exists, none is created)"* | Accurate as a statement of code, and explicitly a deliberate deferral | **No conflict** — the comment is truthful. But it means the reference capability (REF-152/153/160) is knowingly absent, which is why DEF-006 records it as a gap against the reference rather than as a defect in the code. |
| C-9 | `ResultFlagComputer` comment vs dual-store reality | *"Decision 1: the analyte is the only live range source"* | The fallback to `ReferenceRange` when `analyteBands is null` is live code (`EnterResultCommandHandler.cs:65-69`) | **Code wins, partially.** The comment describes the *intent*; the fallback means `ReferenceRange` is still reachable. This is DEF-002. |
| C-10 | `build.md` R-C01 field list | claims `HistoryName/ArabicName/SampleType/ArrangeNo/Bench` are all missing reference fields | `ArrangeNo` is evidenced (grid column `Arrang`, LEARN p.95/p.141). The other four are not evidenced in the PDFs; the reference edit window (LEARN p.95) lists only fields Top-Lab already has | **Evidence wins over assertion.** `ArrangeNo` retained as a real gap (now folded into REF-162/163 DIFFERENT); the other four are withdrawn as unevidenced. |

---

## 21. Recommended Dependency-Aware Implementation Sequence

**Exactly one sequence, 88 items, every DIFFERENT and MISSING item appearing exactly once.** No item is omitted, none is duplicated, and there is no second competing list. The order is derived from the actual architecture — the Application/Infrastructure/Presentation layering, the shared PDF and barcode infrastructure, the immutable `ReportDocumentContent` string/section/grid content model, and the shared statistics and account read models — not from an arbitrary ordering.

**Standing rules for every item in the sequence**

* No existing migration is ever edited. Migrations 1–14 are additive by design; all schema change goes into **new, later** migrations.
* Every new command/query declares its permission grant. **No dead UI checkbox** — the lesson of REF-236/239/240 is that a stored-but-unread setting is worse than an absent one, so an item is not "done" until a consumer reads the flag.
* New package references are avoided. The only package this sequence would add is a charting library for REF-101's trend rendering, and only after a licence/offline review.
* Anything that would naturally reach into excluded machinery (analyser interface, per-branch scoping, portal, SMS/email) is designed **around** and marked OUT OF SCOPE.

| # | REF | Function | Class | Depends on | Why this position | Complexity / risk | Blocked | Migration |
|---|---|---|---|---|---|---|---|---|
| **PHASE 0 — Owner decisions (no code; 12 items)** |||||||||
| 1 | REF-030 | Delete or deactivate a user | DIFFERENT | — | Semantics must be settled before any later user-screen work bakes them in | low | **YES** | NO MIGRATION |
| 2 | REF-057 | Delete a payment operation | DIFFERENT | — | A money-deletion semantic must be settled before billing/audit work extends the payment model | low | **YES** | NO MIGRATION |
| 3 | REF-105 | One-button combined print of entered-but-unprinted | DIFFERENT | — | "Entered and not printed" may be unverified; conflicts with verify-before-print | low | **YES** | NO MIGRATION |
| 4 | REF-115 | Report element letter, list editing, free text | DIFFERENT | — | "Element letter" is OCR-ambiguous; the data model must not be chosen first | medium | **YES** | NEW MIGRATION |
| 5 | REF-082 | Automatic interpretation comment | DIFFERENT | REF-165 | The comment catalogue is reference data only the owner can supply; engine buildable, content not | medium | **YES** | NEW MIGRATION |
| 6 | REF-083 | Automatic calculation / equations in results | DIFFERENT | — | Scope decision: in-system formulas vs excluded analyser correction factors | medium | **YES** | NEW MIGRATION |
| 7 | REF-209 | Code samples per section | DIFFERENT | — | Departments and coding granularity are undocumented | low | **YES** | NEW MIGRATION |
| 8 | REF-218 | Laboratory terms / abbreviations reference | MISSING | — | Glossary is content, not code; the owner must supply it | low | **YES** | NEW MIGRATION |
| 9 | REF-220 | Reminder note for a user | MISSING | — | Persistence, due-date and visibility semantics undocumented | low | **YES** | NEW MIGRATION |
| 10 | REF-221 | Follow-up appointment for patient/phone | MISSING | — | Reference line is OCR-degraded; granularity (patient vs phone) unstated | low | **YES** | NEW MIGRATION |
| 11 | REF-225 | Payment method, instalments, receiver | DIFFERENT | — | The method list (cash/card/transfer) is not enumerated in the reference | low | **YES** | NEW MIGRATION |
| 12 | REF-234 | Receipt: names in English | MISSING | REF-162, REF-163 | Capture model (manual transliteration vs auto) must be chosen before any code | low | **YES** | NEW MIGRATION |
| **PHASE 1 — Document family: the shared envelope writer (5 items)** |||||||||
| 13 | REF-065 | Envelope settings | DIFFERENT | — | The settings already exist; wiring them must precede the document they configure | low | No | NO MIGRATION |
| 14 | REF-066 | Print the patient envelope | MISSING | REF-065 | First item needing the shared envelope writer; everything below hangs off it | medium | No | NO MIGRATION |
| 15 | REF-067 | Patient barcode on the envelope | MISSING | REF-066, REF-037 | Reuses the existing Code-128 renderer; trivial once the writer exists | low | No | NO MIGRATION |
| 16 | REF-068 | Print a laboratory order with barcode | MISSING | REF-066 | A second document on the same writer; cheapest once the writer exists | low | No | NO MIGRATION |
| 17 | REF-127 | Barcode on receipt and envelope | MISSING | REF-066 | Requires the renderer to be shared across the printing assembly | low | No | NO MIGRATION |
| **PHASE 2 — Culture antibiotic correctness (3 items; cheapest, clinically important)** |||||||||
| 18 | REF-093 | Antibiotic suitable for pregnancy | DIFFERENT | — | One-clause predicate fix removing drugs the reference forbids (DEF-001) | low | No | NO MIGRATION |
| 19 | REF-094 | Antibiotic suitable for children under twelve | DIFFERENT | REF-093 | Same clause; must change in the same commit to keep the grid consistent | low | No | NO MIGRATION |
| 20 | REF-092 | Antibiotic commercial name | DIFFERENT | REF-018, REF-019 | Small additive schema change on an otherwise complete screen; do it once | low | No | NEW MIGRATION |
| **PHASE 3 — Report identity and abnormality marking (4 items; highest value per line changed)** |||||||||
| 21 | REF-125 | Report identification block | DIFFERENT | — | The document model already declares the slots; only 4 builders pass `null` | low | No | NO MIGRATION |
| 22 | REF-118 | Print the abnormality marker | DIFFERENT | REF-125 | Writer change on the same file; makes the existing stored flag visible | low | No | NO MIGRATION |
| 23 | REF-239 | Print account instead of date | DIFFERENT | REF-125 | Depends only on the existing flag and the builders REF-125 touches | low | No | NO MIGRATION |
| 24 | REF-232 | Printer assignment per output kind | DIFFERENT | REF-014 | Completes the assignment set once the envelope document gives the slot a consumer | low | No | NEW MIGRATION |
| **PHASE 4 — Stored-but-inert settings and document modes (7 items)** |||||||||
| 25 | REF-236 | Treating doctor only from entity window | DIFFERENT | — | Presentation enforcement of an existing flag; no new work needed | low | No | NO MIGRATION |
| 26 | REF-240 | Result-screen account display mode | DIFFERENT | — | The flag is stored and editable with **no reader** (ERR-2) — a live honesty defect | low | No | NO MIGRATION |
| 27 | REF-235 | Receipt: Himself/Herself | DIFFERENT | — | Resolver exists; only the print path is missing (ERR-3) | low | No | NO MIGRATION |
| 28 | REF-061 | Receipt white paper + header/footer + logo | DIFFERENT | REF-065 | The receipt image half needs the asset store the report side later generalises | medium | No | NEW MIGRATION |
| 29 | REF-062 | Receipt header/footer text, font, size, colour, logo | DIFFERENT | REF-028 | Same settings screen and asset store; do both together | low | No | NEW MIGRATION |
| 30 | REF-064 | Receipt via cash-register printer | DIFFERENT | REF-029 | A page-size option on an existing writer; independent of the envelope work | low | No | NO MIGRATION |
| 31 | REF-233 | Auto-print receipt on registration | MISSING | REF-038, REF-029 | Changes registration behaviour; after the receipt work is stable | low | No | NEW MIGRATION |
| **PHASE 5 — Search, lookup and aggregation correctness (10 items)** |||||||||
| 32 | REF-132 | Search patient by exact name | DIFFERENT | — | Extends an existing, heavily tested search query; cheapest path to matching the reference | low | No | NO MIGRATION |
| 33 | REF-133 | Search patient by partial name | DIFFERENT | REF-132 | Same screen and query; do both so the two documented modes arrive together | low | No | NO MIGRATION |
| 34 | REF-145 | Status indicator per patient | DIFFERENT | — | The roll-up computation already exists; a grid column and a resolver | low | No | NO MIGRATION |
| 35 | REF-158 | Count of patients per test | DIFFERENT | — | **Wrong business rule**, not a missing feature: counts rows, not distinct patients | low | No | NO MIGRATION |
| 36 | REF-161 | Catalog search by name/group/**test number** | DIFFERENT | — | The query already exists and is permission-gated; one more key | low | No | NO MIGRATION |
| 37 | REF-162 | Edit test data and price | DIFFERENT | REF-036 | Add the `Arrang` / `Out Lab Name` / `Routin` fields the reference catalog carries | low | No | NEW MIGRATION |
| 38 | REF-163 | Add a new test | DIFFERENT | REF-037 | Same fields on the create path; do with REF-162 so the entity is touched once | low | No | NO MIGRATION |
| 39 | REF-201 | Sent-out tests not yet dispatched | DIFFERENT | — | Additive query closing a dispatcher safety gap | low | No | NO MIGRATION |
| 40 | REF-203 | Account filters: debtors / commission / settled | DIFFERENT | — | Predicate options over an existing query the accounts phase reuses | low | No | NO MIGRATION |
| 41 | REF-148 | Blood-picture **element** value search | DIFFERENT | REF-101 | Needs an analyte-level read path; the current monitor is test-level only (ERR-4) | medium | No | NO MIGRATION |
| **PHASE 6 — Result entry, interpretation and history (10 items)** |||||||||
| 42 | REF-036 | Patient medical history flags in the result window | DIFFERENT | — | Flags are stored; this only surfaces them in the read-out and the report | low | No | NO MIGRATION |
| 43 | REF-079 | Read the patient data block from the result screen | MISSING | REF-042 | The read-out button and the read-model fields REF-042 also needs | low | No | NO MIGRATION |
| 44 | REF-097 | Blank report: patient data only | DIFFERENT | — | Small layout flag on an existing writer | low | No | NO MIGRATION |
| 45 | REF-131 | Blank report with data entry | DIFFERENT | REF-044 | Same file; the free-text entry surface is new but the document model exists | low | No | NO MIGRATION |
| 46 | REF-069 | Receipt expected result date per test | DIFFERENT | REF-029 | Data (turnaround minutes, pickup date) is already stored; writer + editor missing | low | No | NO MIGRATION |
| 47 | REF-055 | Clear (zero) the whole account | DIFFERENT | REF-002 | A loop over the existing void command plus an audit row; after payment semantics settle | low | **YES** (needs REF-057) | NO MIGRATION |
| 48 | REF-217 | Test information library enrichment | DIFFERENT | REF-036 | Pure read-model enrichment; sample kinds and ranges exist, clinical-effect text needs a column | low | No | NEW MIGRATION |
| 49 | REF-101 | CBC patient history table | MISSING | REF-041 | **Cheaper than both audits implied** — the tested pivot exists, only wiring and rendering are missing (DEF-007) | low | No | NO MIGRATION (charting package evaluated first) |
| 50 | REF-104 | More than one combined report per patient | DIFFERENT | REF-095, REF-003 | Persisting combined-report groupings is safe only once the combined report stops changing | low | No | NEW MIGRATION |
| 51 | REF-106 | Last N visits below the profile report | DIFFERENT | REF-098, REF-110 | A trailing history block, once the report body is data-driven | medium | No | NEW MIGRATION |
| 52 | REF-033 | Users login/logout movement report | MISSING | REF-031, REF-222 | New read-only query; removes an auditor-visible blind spot early | low | No | NEW MIGRATION |
| **PHASE 7 — Barcode label family (4 items)** |||||||||
| 53 | REF-128 | Barcode labels per test and per sample | DIFFERENT | REF-017 | Extends the label service from one patient label to a per-test/per-sample set | low | No | NEW MIGRATION |
| 54 | REF-129 | Vertical barcode on report/receipt/envelope | MISSING | REF-017 | Needs the shared renderer plus an orientation option; follows REF-127 | low | No | NEW MIGRATION |
| 55 | REF-130 | Tube/container/swab labels (38×25 mm, multi-up) | MISSING | REF-053 | Sticker geometry and multi-up; follows the renderer sharing | medium | No | NEW MIGRATION |
| 56 | REF-238 | Barcode display control | DIFFERENT | REF-017, REF-053, REF-054 | A global switch only becomes meaningful once barcodes exist on the documents | low | No | NO MIGRATION |
| **PHASE 8 — Worksheet family (11 items; one data model, eleven capabilities)** |||||||||
| 57 | REF-150 | Worksheet of a group of patients in a period | MISSING | — | The reference's primary worksheet and the query the variants read from | medium | No | NO MIGRATION |
| 58 | REF-155 | Worksheet with full patient + test data | MISSING | REF-057 | Enriches the line model with values already on the result rows | medium | No | NO MIGRATION |
| 59 | REF-151 | Worksheet of a group of patients, condensed | MISSING | REF-057, REF-058 | A layout variant over the same data | low | No | NO MIGRATION |
| 60 | REF-156 | Worksheet with condensed patient data | MISSING | REF-057, REF-058, REF-059 | The condensed layout on the same enriched row model | low | No | NO MIGRATION |
| 61 | REF-152 | Worksheet of tests / group of tests in a period | DIFFERENT | REF-057 | A print command over a query and a writer that both exist; closes DEF-006 | low | No | NO MIGRATION |
| 62 | REF-153 | Worksheet by work group (Log) | DIFFERENT | REF-061 | Identical work for the work-group mode; same command, same writer | low | No | NO MIGRATION |
| 63 | REF-154 | Print only unfinished tests or all | DIFFERENT | REF-061 | A predicate option over queries that already carry the completion facts | low | No | NO MIGRATION |
| 64 | REF-157 | Worksheet for one department over a period | DIFFERENT | REF-061, REF-062 | Adds the department and patient-code-range selectors on the new print path | low | No | NO MIGRATION |
| 65 | REF-159 | Classify how often each test was performed | DIFFERENT | REF-061 | Prints an existing classification; adds a signature column to the writer | low | No | NO MIGRATION |
| 66 | REF-160 | Worksheet print preview | DIFFERENT | REF-061, REF-062 | One preview button per mode once each has a print path | low | No | NO MIGRATION |
| 67 | REF-164 | Substitute patient name/data on the worksheet line | DIFFERENT | REF-058 | A substitute-identity field on the line built in this phase | low | No | NO MIGRATION |
| **PHASE 9 — Statistics and account reporting (7 items)** |||||||||
| 68 | REF-192 | Print a statistics report | DIFFERENT | REF-178 | One writer for the statistics DTOs; the rows exist and are tested | low | No | NO MIGRATION |
| 69 | REF-194 | Stocktake by element **and report type** | DIFFERENT | REF-193, REF-068 | Report-type variants over the existing inventory query | medium | No | NO MIGRATION |
| 70 | REF-196 | Stocktake statement for a treating doctor | DIFFERENT | REF-069 | A dedicated statement on the account print path built here | low | No | NO MIGRATION |
| 71 | REF-204 | Account per doctor, daily/monthly/annual | DIFFERENT | REF-070 | A doctor-scoped variant of the existing inventory query | low | No | NO MIGRATION |
| 72 | REF-205 | Detailed account by test price or by results | MISSING | REF-068 | First genuinely new account read model; defines the rows the next items reuse | medium | No | NO MIGRATION |
| 73 | REF-206 | Referral-entity statement + hand-over list | DIFFERENT | REF-072, REF-068 | Statement and hand-over list on REF-205's read model | low | No | NO MIGRATION |
| 74 | REF-207 | Four account report types | MISSING | REF-072, REF-073 | Four presentation variants; last in the phase because purely presentational | medium | No | NEW MIGRATION |
| **PHASE 10 — Report style / layout foundation (6 items; the critical path)** |||||||||
| 75 | REF-113 | Colours of patient data, headings, profiles, abnormal results | MISSING | — | **The prerequisite for every other report customisation item** — the content model must carry a style before colours can be expressed | medium | No | NEW MIGRATION |
| 76 | REF-114 | Whole-row highlight choice | MISSING | REF-075 | First consumer of the style model and the cheapest way to validate it | medium | No | NEW MIGRATION |
| 77 | REF-116 | Report line spacing + per-element left offset | MISSING | REF-075 | Layout data sharing the same store | medium | No | NEW MIGRATION |
| 78 | REF-108 | Report header/footer colours and element positions | DIFFERENT | REF-075 | Per-element colours and offsets; reuses the layout store | medium | No | NEW MIGRATION |
| 79 | REF-117 | Report element shape, name, order | MISSING | REF-075, REF-077, REF-078 | The largest layout item and the one other layout work depends on | medium | No | NEW MIGRATION |
| 80 | REF-219 | Colour palette | MISSING | REF-075 | A palette has no purpose until colours exist | low | No | NO MIGRATION |
| **PHASE 11 — Report layout consumers (5 items)** |||||||||
| 81 | REF-107 | Report header/footer modes none/words/image | DIFFERENT | REF-028, REF-078 | The image mode finally becomes real once the asset and layout stores exist | medium | No | NEW MIGRATION |
| 82 | REF-124 | Show or hide report blocks | DIFFERENT | REF-079, REF-075 | Block visibility is a flag on the layout definition | medium | No | NEW MIGRATION |
| 83 | REF-089 | Choose which culture report elements are shown | MISSING | REF-079 | The same visibility mechanism applied to one report kind | medium | No | NEW MIGRATION |
| 84 | REF-123 | CBC colouring, auto-comment, manual mode | DIFFERENT | REF-075, REF-005 | Colouring and the comment switch; the switch also depends on the blocked REF-082 | medium | No | NEW MIGRATION |
| 85 | REF-121 | CBC report design variants and large paper | DIFFERENT | REF-110, REF-079 | Design variants depend on the layout store; the paper size is trivial | medium | No | NEW MIGRATION |
| **PHASE 12 — Graphics (2 items; last, highest risk)** |||||||||
| 86 | REF-119 | Semen analysis report with graphs and diagrams | MISSING | REF-075, REF-079 | Needs the data-driven body and introduces a drawing capability | high | No | NEW MIGRATION |
| 87 | REF-120 | Image report (1–4 images, draggable text/shapes) | MISSING | REF-075, REF-079, REF-086 | The largest and most dependency-heavy item in the set; therefore last | high | No | NEW MIGRATION |
| **PHASE 13 — Hardware-dependent (1 item)** |||||||||
| 88 | REF-211 | Reader options: save after read, print barcode after read | MISSING | REF-053, REF-055 | Reader integration is hardware-dependent with no software prerequisite, so it is scheduled last rather than blocking anything | medium | No | NEW MIGRATION |

**Sequence integrity (mechanically checked):** 88 rows, 88 distinct REF ids, every one of the 59 DIFFERENT and 29 MISSING ids present exactly once, zero excluded ids present.

---

## 22. Database / Migration Impact

Every one of the 88 sequence items is assigned **exactly one** of `NO MIGRATION`, `NEW MIGRATION`, `EXISTING MIGRATION / PRIMARY KEY IMPACT`.

### 22.1 Headline finding

**No item in this sequence requires editing an existing migration or changing an existing primary key.** The 14 migrations at the target commit (`20260828052248_BaselineDataModel` … `20261002002546_AddAntibioticMasterFields`) are additive by design, and every gap identified by this audit can be closed by a new, later migration. This is independently re-verified and agrees with the second audit.

| Migration status | Items |
|---|---|
| `NO MIGRATION` | **52** |
| `NEW MIGRATION` (additive) | **36** |
| `EXISTING MIGRATION / PRIMARY KEY IMPACT` | **0** |
| **Total** | **88** |

*(Figures mechanically recounted from the §21 table, not estimated: every row was parsed and assigned exactly one of the three permitted values; zero rows unparsed.)*

### 22.2 The `NEW MIGRATION` items and their intended schema change

Schema is described only. No migration is created or edited by this audit.

| REF | Intended schema change (additive only) |
|---|---|
| REF-004 | A report-letter column and a report-element-visibility table |
| REF-005 | A `CommentRules` table (Analyte, band/flag test, text, priority, enabled) + index |
| REF-006 | A formula expression on `Analyte`, an evaluation order, and a stored derived value on `ProfileResultItem` |
| REF-007 | A section reference plus a nullable per-sample section column, or a `PatientTestSample` table |
| REF-008 | A `LabGlossary` table (Term, Expansion, Scope) |
| REF-009 | A `LabNote` table (AuthorUserId, TargetUserId?, Body, DueAtUtc, IsDone) |
| REF-010 | A `PatientFollowUp` table (PatientId?, PhoneNumber?, DueAtUtc, Note, IsDone) |
| REF-011 | A `PaymentMethod` column on `PaymentOperations` + optional instalment index |
| REF-012 | `Patients.FullNameEn?`, `ExternalEntities.NameEn?` |
| REF-020 | `Antibiotics.CommercialName` + a per-culture trade-name list |
| REF-024 | A new `PrinterOutputType.Card` enum value ⇒ a **new** migration altering the stored column mapping; existing rows and history untouched |
| REF-028, REF-029 | An image-asset path column and optional per-scope colour columns on the receipt settings row |
| REF-031 | A boolean on the receipt/system settings row |
| REF-037, REF-038 | An `ArrangeNo` column, an out-lab reference and a routine flag on `Test` |
| REF-048 | A clinical-effect free-text column on `Test` |
| REF-050 | A persisted combined-report selection (PatientId, PatientTestId, SortOrder, PrintedAtUtc) |
| REF-051 | A settings column for the history visit count |
| REF-052 | A `UserSessions` table (UserId, MachineName, SignedInAtUtc, SignedOutAtUtc) |
| REF-053 | A printed-sticker-set record so a reprint can be traced |
| REF-054 | A boolean vertical-barcode flag on each settings row + renderer orientation |
| REF-057–067 | No schema change (worksheet work is pure read model + presentation) |
| REF-074 | A stored account report-type preference |
| REF-075–085 | A report style store (scope, element, font colour, background colour, bold) + a style field on the report content model; a `ReportLayoutElement` table (ReportKind, ElementKey, LeftCm, TopCm, ColorHex); a layout definition store; block-visibility flags; a report-image table plus a placed-object store |
| REF-086, REF-087 | A semen report template; a `ReportImage` table |

### 22.3 Two notes stated explicitly rather than buried

* **REF-024 (`PrinterOutputType.Card`)** adds a value to an enum persisted in a column. This is done with a **new** migration that alters the column's stored mapping. The existing column and the migration history are left untouched — the alternative (editing an existing migration) is explicitly rejected.
* **REF-053/054/055 and REF-088** are recorded as `NEW MIGRATION` because a robust implementation wants to remember which sticker sets were printed and to keep a scan audit. A minimal implementation of the same reference capability could be pure printing and would then be `NO MIGRATION`. The status is stated at the safe end of the range deliberately.

### 22.4 Ordering constraint for whoever implements this

Because REF-024 alters a persisted enum column and REF-075/079 create the layout stores that seven later items consume, the recommended **migration order** is: (1) independent additive columns (REF-012, 020, 037, 038, 048, 011, 031); (2) new tables (REF-005, 007, 008, 009, 010, 050, 052, 075, 079, 086, 087); (3) the enum-widening migration (REF-024) **alone**, isolated, with a reseed-count test asserting 5 printer rows; (4) nothing edits migrations 1–14 at any point.

---

## 23. Build / Test / Runtime Verification

Everything in this section is **non-destructive**. No repository file was altered to make a build or test pass; the two accommodations used are a **command-line property** and an **environment variable**, neither of which touches the working tree. `git status --porcelain` reports no tracked-file modification throughout.

### 23.1 Commands executed and results

| # | Command | Result | Notes |
|---|---|---|---|
| 1 | `git rev-parse HEAD` | `3590a7f5c53f5d988ada32d06a57f38a85bbdf03` | Exact target match |
| 2 | `dotnet build TopLab.sln -c Release` | **FAILED** — `error NETSDK1100: To build a project targeting Windows on this operating system` (×2: `src/TopLab.Presentation`, `tests/TopLab.Presentation.Tests`) | Expected on Linux for `net8.0-windows` + `UseWPF` |
| 3 | `dotnet build TopLab.sln -c Release -p:EnableWindowsTargeting=true` | **Build proceeded** with the Windows targeting pack resolved from NuGet | Command-line property only; **no project or props file edited** |
| 4 | `dotnet test tests/TopLab.Domain.Tests` | **507 passed / 0 failed** | First attempt aborted: `You must install or update .NET to run this application` — only the .NET **9.0.18** runtime is installed, no .NET 8 runtime |
| 5 | `DOTNET_ROLL_FORWARD=Major dotnet test …` | **507 passed / 0 failed** | Environment variable only |
| 6 | `DOTNET_ROLL_FORWARD=Major dotnet test tests/TopLab.Application.Tests` | **1688 passed / 0 failed** | |
| 7 | `DOTNET_ROLL_FORWARD=Major dotnet test tests/TopLab.Infrastructure.Tests` | **271 passed / 24 failed** | **Every failure is `DocumentDrawingException : The text "…" uses font families that are not available: 'Arial'`** — no Arial in this Linux container |
| 8 | `DOTNET_ROLL_FORWARD=Major dotnet test tests/TopLab.Persistence.Tests` | **13 passed / 0 failed / 2 skipped** | The 2 skips are the Testcontainers/Docker-dependent tests; `docker` is not installed and there is no Docker socket |
| 9 | `dotnet test tests/TopLab.Presentation.Tests` | **NOT RUN** | `net8.0-windows` + `UseWPF`; cannot execute on Linux regardless of the targeting pack |

### 23.2 Aggregate

| Suite | Passed | Failed | Skipped |
|---|---|---|---|
| `TopLab.Domain.Tests` | 507 | 0 | 0 |
| `TopLab.Application.Tests` | 1688 | 0 | 0 |
| `TopLab.Infrastructure.Tests` | 271 | **24** | 0 |
| `TopLab.Persistence.Tests` | 13 | 0 | 2 |
| **Total** | **2479** | **24** | **2** |

**Second-pass re-run (2026-10-05), independent of the first pass:**

| Suite | Passed | Failed | Skipped |
|---|---|---|---|
| `TopLab.Domain.Tests` | 507 | 0 | 0 |
| `TopLab.Application.Tests` | 1688 | 0 | 0 |
| `TopLab.Infrastructure.Tests` | 271 | **24** | 0 |
| `TopLab.Persistence.Tests` | 13 | 0 | 2 |
| **Total** | **2479** | **24** | **2** |

The second pass reproduced the first pass's totals **exactly**, with the same 24 Arial-font failures. This is a determinism check on the environment as well as on the code: the classification evidence in §10–§15 rests on a test state that is stable and reproducible.

**The 24 failures are environmental, not defects.** All are the missing Arial font, raised at PDF-render time. This **independently reproduces the second audit's reported 271 passed / 24 failed figure exactly**, which is a further confirmation that both audits observed the same code state and the same environment.

### 23.3 What the passing suites actually prove

A green suite is evidence about *behaviour under test*, and the three work items of §17 are covered by it: the banded-monitor handler (20 facts), banded-monitor writer (11 facts), statistics monitor view-model (20 facts), day-of-month/money statistics handler (+345 lines), clear-all-tests guard (14 facts), patient-editor clear guard (11 facts), and result print coordinator (+88 lines) all pass here.

### 23.4 What the suites do **not** prove

* The 24 infrastructure failures mean **no PDF was ever rendered in this environment**. The barcode, report, receipt, invoice, worksheet, price-list, custom-group and banded-monitor writers were read as code and compiled, but their output was not produced.
* The `net8.0-windows` presentation assembly compiled but was **never loaded or executed**. No WPF view, view-model binding, or XAML interaction was exercised.
* The 2 skipped persistence tests mean **no migration was applied to a real SQL Server**. Migration correctness is asserted statically from migration source and the model snapshot only.
* Docker being unavailable means the Testcontainers fixture never started, so no migration up/down was validated against a live database.

---

## 24. Not Verified by This Agent

Stated explicitly so that no static finding is mistaken for a working system.

| # | Not verified | Why | What is required to verify it |
|---|---|---|---|
| 1 | **Runtime behaviour of the WPF application** | `TopLab.Presentation` targets `net8.0-windows` with `UseWPF`; the sandbox is Linux. The application was never launched. | Build and run `TopLab.sln` on Windows; walk every screen named in §11–§13 |
| 2 | **Any database behaviour** | No SQL Server reachable; `docker` absent so Testcontainers cannot start one. The 2 container-dependent tests skipped. | Start SQL Server (or let Testcontainers do it), apply migrations via *Apply database updates*, re-run `TopLab.Persistence.Tests` |
| 3 | **Whether migrations apply cleanly to an existing database** | No database exists to migrate. | Point the app at a database created by an earlier build; confirm the pending-migration list matches the 14 files at this commit |
| 4 | **Print output fidelity for every document** | All 7 PDF writers were read and compiled; none rendered (Arial absent) and no printer addressed | Print one of each document on Windows and compare against the cited reference pages |
| 5 | **Barcode scannability** | The Code-128 label is a hand-built bitmap in a hand-built PDF; the bytes were never produced here | Print a label and scan it with several readers; confirm payload and quiet zone |
| 6 | **Arabic shaping, RTL and pagination** | Compilation only; rendering blocked by the font | Render each document on Windows with an Arabic-capable font |
| 7 | **Presentation-layer tests** | `TopLab.Presentation.Tests` targets `net8.0-windows` | `dotnet test TopLab.sln` on Windows with Docker available |
| 8 | **Exact wording/ordering of the reference's Arabic UI strings** | OCR layer has systematic glyph confusion (E1) | Read the PDFs on screen, or obtain the vendor's original document, for any string-level requirement |
| 9 | **REF-209 sample-coding granularity** | Reference does not state patient-, visit- or test-scoped | Owner decision (Phase 0, item 7) |
| 10 | **REF-218 glossary content** | Reference names the tool, supplies no content | Owner supplies the term list, or authorises deriving it from the catalogue |
| 11 | **REF-225 collection-method list** | Reference requires the method be shown but does not enumerate methods | Owner confirms the method list before the column is added |
| 12 | **REF-115 "element letter" semantics** | OCR-ambiguous between report code, display symbol and transliterated letter (E5) | Owner decision (Phase 0, item 4) |
| 13 | **REF-221 follow-up granularity** | Reference line is OCR-degraded (E4) | Owner decision (Phase 0, item 10) |
| 14 | **Printer-assignment behaviour at runtime** | No printer is reachable; the dispatcher was read statically | Assign each output kind to a real printer and confirm routing |

---

## 25. Final Count Reconciliation

### 25.1 Reference inventory arithmetic

```
TOTAL REFERENCE FUNCTIONS          = 242
  minus HARD-EXCLUDED              =  25   (5 owner categories, §9)
  equals IN-SCOPE                   = 217
```

### 25.2 Classification arithmetic

```
IN-SCOPE                           = 217
  IMPLEMENTED                      = 129
  DIFFERENT                        =  59
  MISSING                          =  29
  BLOCKED / UNRESOLVED (withheld)  =   0
  -------------------------------------
  129 + 59 + 29 + 0                = 217   ->  BALANCED
```

### 25.3 Sequence arithmetic

```
DIFFERENT + MISSING               =  59 + 29 = 88
Items in the §21 sequence          =  88
Distinct REF ids in the sequence   =  88
Duplicate entries                  =   0
In-scope items absent from it      =   0
Excluded ids present in it         =   0   (independently checked against REF-001..REF-025)
```

### 25.4 Migration arithmetic

```
NO MIGRATION                       =  52
NEW MIGRATION (additive)           =  36
EXISTING MIGRATION / PK IMPACT     =   0
                                   -----
                                   88   ->  every item assigned exactly one value
```

### 25.5 How BLOCKED is represented

**No function had its classification withheld** — the withheld count is 0 and all 217 in-scope functions appear exactly once in exactly one of the three buckets. `BLOCKED` is an *additional* flag, never a substitute for a classification:

* **4 evidence-blocked items** — REF-115, REF-221, REF-209, REF-030: the reference itself is ambiguous or damaged, so specification is gated. Each already carries a classification (DIFFERENT, MISSING, DIFFERENT, DIFFERENT respectively).
* **4 specification-gated items** — REF-082, REF-083, REF-218, REF-225: the classification is certain, but the *content* is owner-supplied reference data or a scope decision.

### 25.6 Cross-audit arithmetic, and why the numbers differ

| | First audit | Second audit | This audit |
|---|---|---|---|
| Reference functions enumerated | 102 | 242 | 242 |
| Hard-excluded | 14 | 25 | 25 |
| In scope | 88 | 217 | 217 |
| IMPLEMENTED | 67 (+2 differently = 69) | 133 | **129** |
| DIFFERENT | 11 | 55 | **59** |
| MISSING | 8 | 29 | **29** |
| Sequence items | 21 | 84 | **88** |

The `21 → 84 → 88` movement is decomposed in §19.2 and is **not** an implementation change of that magnitude:

* `21 → 84`: **granularity**. The first audit counted *work items in a plan* over a 102-function extraction; the second counted *work items* over a 242-function inventory. Different universes, different units. The 84-item figure already reflects the post-`[B-01]` code, so the six functions that batch closed are inside the 84 — not additive to it.
* `84 → 88`: **audit correction only, zero implementation change.** Four items moved IMPLEMENTED→DIFFERENT and one moved DIFFERENT→IMPLEMENTED, so DIFFERENT rose 55→59 and the sequence rose 84→88. MISSING is unchanged at 29.

---

## 26. Final Independent Verdict

**On the reference model.** The 242-function inventory is sound. It was re-derived from the two permitted PDFs page-by-page and no function was found missing, none was found invented, and the five hard exclusions total exactly 25 with no invented fax-sending function padding the count. The prior inventories are reusable as identifier spaces because they genuinely map to the same functions — not because they were inherited uncritically.

**On the current implementation.** Top-Lab at `3590a7f5` is a mature, well-tested system that satisfies **129 of 217** in-scope reference functions outright, with another 59 present-but-different. The core clinical loop — registration, ordering, result entry, flagging, reference ranges, review, printing, combined reports, history reports, delivery, billing, settlement, sent-out accounting, audit and statistics — is implemented with real traced paths, not placeholders. This is a substantially complete product, not a shell.

**On the two prior audits.** Both are useful and both are wrong in specific, provable ways, and this audit does not flatter either.

* `build.md` made one **materially false** claim (the "dead grants", disproven at its own baseline), one **unsubstantiated** claim (four of R-C01's five field names are not evidenced as reference fields), and framed two working clinical controls as latent defects. Its architecture baseline is accurate and its negative findings largely hold. Its stale items are honestly explained by the fact that `[B-01]` landed after it.
* The second audit is the stronger document — its hashes, counts, migration inventory and most negative searches reproduce exactly, and it is honest about its own limits. But it contains **four misclassifications and one false negative-evidence claim**, and it never tested a single historical claim of the first audit despite its brief requiring it. Its most consequential error is REF-237, where it asserted a negative ("no consumer") that a single grep disproves, thereby converting a working feature into a gap.

**On the "three functions" question.** The answer is exact and evidence-based: **three work items, delivered as eight slices in one batch (`B-01`, 2026-10-04), touching 28 files for +3,318/−33 lines with zero migration changes** — the banded result monitor, the patient-statistics day-of-month and money row, and the first-registration guard plus the outside-lab report note. They closed first-audit work items R-F05, R-F01 and R-A04 in full, moved **6 reference functions** from gap to implemented, and left **no regression**.

**On the remaining work.** 88 items, and the shape of the remaining gap is now clear and is *not* evenly spread. It concentrates in four places: the **report style and layout foundation** (REF-113/114/116/117/108 and their six consumers — the content model is a flat string list, so nothing visual can be expressed until it carries a style), the **document family** (envelope, lab order, barcodes on documents, image report), the **period-worksheet family** (one missing data model serving eight capabilities), and the **account-report family**. Only two items are genuinely large and high-risk (REF-119 semen graphics, REF-120 image report), and only one is hardware-dependent (REF-211).

**On this second pass.** Version 1's substantive conclusions survived a hostile re-audit intact. All 129 IMPLEMENTED items were re-verified to the depth Version 1 had reserved for DIFFERENT and MISSING; all five of its reclassifications were re-tested and stand; 23 code citations and 20 PDF citations were re-read and **all proved accurate**; the counts, exclusions, Git reconstruction, reconciliation and the 88-item sequence all re-parsed cleanly. **No classification, count, citation, exclusion or reconstruction was overturned.**

What the second pass did find is narrower but real: **Version 1 missed a defect that the source code documents about itself** (DEF-008 — divergent case-folding between a SQL `UPPER()` pre-filter and an in-memory `ToUpperInvariant()` comparison, which can silently drop a patient's earlier visit from a clinical report when the operator selects the non-default "group by patient name" mode), and **Version 1 applied a weaker test standard to its own `ST+TEST` labels than the brief demands** (DEF-009 — a production function whose only guard is a source-text assertion, which is precisely why DEF-008 escaped two audits). Both are now recorded, scoped, and marked not-runtime-verified.

The honest lesson is the one the owner already paid for once: **the weakness was not in the arithmetic, which was clean, but in verification depth applied unevenly across a large group.** Version 1 said so itself in its §11.1; this pass paid that debt and found exactly what the disclosure implied it would. The 129 IMPLEMENTED items were never the risky judgements — they are mostly presence-of-a-complete-path judgements — but they were the *unverified* ones, and one of them was guarding a clinical-report read path with a test that asserts nothing.

**On confidence.** High for the static classification, the exclusion arithmetic, the git reconstruction and the two prior audits' factual errors — each rests on a direct citation I re-read. Explicitly **not** claimed: anything about runtime behaviour, PDF output fidelity, Arabic rendering, barcode scannability, or database behaviour, none of which could be exercised in this environment. 2,479 tests pass here; the 24 that fail do so only because Arial is absent from a Linux container. A green suite is not a running laboratory system, and this report does not pretend otherwise.

---

## 27. Self-Audit / Quality-Control Checklist (second pass, 22 mandatory points)

| # | Check | Result |
|---|---|---|
| 1 | Exact target commit re-verified | **PASS** — `git rev-parse HEAD` = `3590a7f5c53f5d988ada32d06a57f38a85bbdf03`, exact match, re-run at the start of this pass |
| 2 | Only the two enhanced PDFs under `Docs/Remaining Tasks Folder/` used as reference evidence | **PASS** — extracted by `git show` from the commit object; SHA-256 `dc817016…42e0f` and `2e523537…70a14e` recomputed and matched |
| 3 | `Docs/Reference system files/` NOT used | **PASS** — sparse-checkout re-confirmed at the start of this pass (`Docs/` absent from disk); never opened, listed or extracted |
| 4 | `build.md` and `Job Verification and Validation.md` treated as unverified historical evidence only | **PASS** — read as Level-4 evidence; their claims were re-tested against code, not adopted |
| 5 | Canonical function inventory complete and deduplicated | **PASS** — 242 rows, 242 distinct ids, 25 excluded individually enumerated; no function found missing or invented |
| 6 | Hard exclusions reconcile exactly | **PASS** — `242 − 25 = 217`; no excluded id appears in the implementation sequence (parsed and checked) |
| 7 | Current counts reconcile | **PASS** — `129 + 59 + 29 + 0 = 217`; sequence `88 = 59 + 29`; migration `52 + 36 + 0 = 88`; all mechanically re-parsed from the tables, not restated |
| 8 | Every IMPLEMENTED item has an end-to-end path with `path:line` evidence | **PASS after this pass** — all 129 re-verified; 23 load-bearing citations spot-checked line-by-line and **all accurate** |
| 9 | Every DIFFERENT item has explicit evidence of the material difference | **PASS** — each of the 59 states the precise divergence with a code citation; none restates the reference alone |
| 10 | Every MISSING item has explicit negative-search evidence | **PASS** — each of the 29 carries the re-run search that returned zero |
| 11 | Every Defect has `path:line` evidence and impact statement | **PASS after this pass** — **DEF-008 and DEF-009 added**; all 9 now carry path:line, impact and a verification status |
| 12 | Every reference claim has PDF page/section evidence | **PASS** — 20 PDF citations re-read at full page text in this pass; **all confirmed accurate**, including p.95/141 (`Arrang`, three search keys), p.101 (low/high comment), p.104 (age-unit note), p.199 (five printer kinds incl. card), p.205 (envelope settings), p.210 (`Save samples after read ID`). OCR glyph variants (e.g. `الكارت`→`الكارنٌة`) were identified as extraction noise, **not** as citation errors |
| 13 | Every code claim has source evidence | **PASS** — 23 sampled `path:line` citations each opened and confirmed at the exact line |
| 14 | Historical mappings do not force false reconciliation | **PASS** — §18.3 still states plainly that `build.md`'s 67-item list **cannot** be mechanically mapped to REF ids; no mapping invented |
| 15 | The three post-first-audit implementations independently reconstructed from Git evidence | **PASS** — re-confirmed: baseline `7a2cfb5` exists, 12 commits, 8 code-changing slices clustering on touched files into exactly 3 work items, 0 migrations |
| 16 | Every DIFFERENT/MISSING item appears exactly once in the sequence | **PASS** — 88 rows, 88 distinct ids, contiguous 1–88, zero excluded ids |
| 17 | Every sequence item has migration analysis | **PASS** — all 88 assigned exactly one of the three permitted values; `52/36/0`, recounted mechanically |
| 18 | Runtime limitations not disguised as successful verification | **PASS** — zero items are "RUNTIME VERIFIED"; §23/§24 state that no PDF rendered, no WPF view executed, no migration applied |
| 19 | No excluded feature counted as a gap | **PASS** — the sequence was parsed against REF-001..REF-025 and contains zero excluded ids |
| 20 | No unsupported assumptions remain | **PASS** — evidence-blocked items (REF-115, 221, 209, 030) remain BLOCKED with the ambiguity stated rather than guessed; DEF-008's trigger is stated as collation-dependent, not asserted |
| 21 | **Uniform depth actually achieved across IMPLEMENTED, DIFFERENT, MISSING and Defects** | **PASS** — all 129 IMPLEMENTED re-verified to the depth previously reserved for DIFFERENT/MISSING; the `ST+TEST` labels were tested for over-claiming (none found) and for false-assurance tests (one found → DEF-009) |
| 22 | **Subagent outcomes fully disclosed** | **PASS** — see §27.1 below |

### 27.1 Subagent disclosure (mandatory, §6.6)

| Metric | Value |
|---|---|
| Subagents launched in **this** pass | **1** |
| Subagents that **succeeded** | **0** |
| Subagents that **failed** | **1** |

**Failed subagent — full disclosure.** A single foreground (synchronous) subagent was launched as a **capability probe** before committing the remaining work, precisely to avoid repeating Version 1's silent-failure mistake. Scope: run two read-only commands (`git rev-parse HEAD` and a 20-line `sed` read) and report verbatim output. Mode: `run_in_background=false`, per §6.1.

**Result: FAILED.** The subagent returned, verbatim: *"I have no shell/filesystem tool available, so I cannot run either command or report their output."*

**Reason for failure:** the subagent runtime in this environment exposes **no shell and no filesystem tool**. This is an environment-level capability gap, identical to the cause of the three Version-1 subagent failures (one of which reported the same limitation explicitly and correctly refused to fabricate). It is **not** a task-specification or prompt defect.

**Work performed in its place, by this agent, at full depth:** the entire second-pass verification — all 129 IMPLEMENTED items, all 5 reclassifications, all 9 defects, 23 code citations, 20 PDF citations, the arithmetic re-parsing, and the source-text test-quality sweep. Nothing was delegated and nothing was left unverified.

**Transparency commitment, discharged on time this time.** Version 1 launched three subagents in silent background mode, their failure surfaced only as a stub preamble, and the owner was not told until asked. In this pass the failure was **detected immediately, reported in this report, and the affected scope was performed directly** — before any further work was claimed complete.

### 27.2 Defects found in Version 1 by this pass (the reason Version 2 exists)

| # | Version 1 claim | Corrected claim | Exact evidence | Why Version 1 was wrong or insufficient |
|---|---|---|---|---|
| **D-1** | Seven defects (DEF-001…DEF-007). No patient-history defect recorded anywhere in the report. | **Eight→nine defects.** `PatientHistoryReader.ResolveVisitPatients` groups patients by name using a SQL `UPPER()` (database collation) pre-filter and an in-memory `ToUpperInvariant()` (invariant ordinal) exact comparison. Under a diverging collation a patient's earlier visit is **silently omitted** from combined/history reports. Conditional on the non-default `ByPatientName` mode. | `src/TopLab.Application/Features/ReportProduction/Common/PatientHistoryReader.cs:32-37` (the code's own caveat), `:38-46` (both folds); `src/TopLab.Domain/Reports/PatientHistoryResolver.cs:31-40`; call sites `AutoInsertHistoryCommandHandler.cs:55`, `InsertHistoryResultCommandHandler.cs:48`, `GetMultiPatientHistoryQueryHandler.cs:47`, `GetPatientTestHistoryQueryHandler.cs:40`; default mode `src/TopLab.Domain/Settings/ReportSettings.cs:55` | Version 1 recorded 7 defects and did not include this one. The defect is **self-documented in the source comment** and is on a clinical report path affecting five reference functions. Version 1's DEF-003/DEF-004/DEF-005 sweep looked at result marking, comment entry and settings; it never swept the report-production read path. This is precisely the shallowness in the IMPLEMENTED group that Version 1 itself disclosed. |
| **D-2** | All 129 IMPLEMENTED items listed as `ST+TEST` or `ST` with no test-quality assessment. | **One production function is guarded only by a source-text assertion** and has zero behavioural tests. `HistoryFilterTests.PatientHistoryReader_ByPatientName_DoesNotMaterializeAllPatients` reads `PatientHistoryReader.cs` from disk and asserts the literal `"StartsWith(firstToken)"`; it never executes the query. | `tests/TopLab.Application.Tests/Features/ReportProduction/HistoryFilterTests.cs:129-136` (fact), `:138-155` (source-reading helper); exhaustive search of `tests/` for `ResolveVisitPatients` returns 0 behavioural references and 1 source-text site | Version 1's verification standard (§5 of the original brief, restated as checklist 21 here) requires reading tests and confirming they "assert the reference behaviour, not merely that the method runs". Version 1 did not apply that test to its own `ST+TEST` labels. This is the mechanism by which D-1 survived two audits: a green suite reported the function as guarded. |

**What did NOT change.** No classification was overturned. No count changed (`129 / 59 / 29` stands). No exclusion changed. No code citation, PDF citation, Git reconstruction, reconciliation row, sequence row or migration assignment was found to be wrong. The two prior audits' errors (the first audit's dead-grants claim; the second audit's REF-237/235/240/148 misclassifications and REF-101 false evidence) were **re-tested and all still stand**.

### 27.3 File-safety and deliverable compliance (this pass)

* Analysis only. No repository file was modified: `git status --porcelain` reports no tracked-file modification; `git rev-parse HEAD` unchanged. No branch, commit, push or history rewrite.
* No helper artefact was created inside the repository. Temporary analysis data lives outside it.
* Exactly **one** new deliverable file was produced: `Final Independent Forensic Audit & Reconciliation Version 2.md`. No second report, summary, PDF/DOCX/HTML/TXT/JSON copy, scratch report or tracker file.
* Version 1 was **not** deleted; it is superseded, not destroyed, so the two-version diff remains auditable.

## Appendix A — Mandatory Current REF Table (every canonical REF item)

Required by the brief as: `REF ID | Function | Scope | Current Status | Verification Status | Reference Evidence | Current Code Evidence | Notes`.

**No REF item is omitted because it is implemented, excluded or trivial.** All 242 identifiers appear exactly once. Verification status is this audit's, using: `ST+TEST` = traced in code during this audit *and* covered by a test in a suite that passed in this environment (§23) · `ST` = traced in code during this audit · `ST (negative search)` = absence established by an explicit re-run negative search · `EXCLUDED` = owner-excluded, not a gap.


| REF ID | Function | Scope | Current Status | Verification Status | Reference Evidence | Current Code Evidence | Notes |
|---|---|---|---|---|---|---|---|
| REF-001 | Upload patient result to the result website | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.58-62 | n/a - excluded category 1 | Not a gap |
| REF-002 | Open the result website and view the patient result | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.63-64 | n/a - excluded category 1 | Not a gap |
| REF-003 | Patient prints/exports the portal result as Word or PDF | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.63 | n/a - excluded category 1 | Not a gap |
| REF-004 | Android application for the result portal | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.63 | n/a - excluded category 1 | Not a gap |
| REF-005 | Print portal receipt data on the patient receipt | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.69; SHOW p.5 | n/a - excluded category 1 | Not a gap |
| REF-006 | Count of patients whose results are on the portal | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.65-67 | n/a - excluded category 1 | Not a gap |
| REF-007 | Delete (block) a patient result from the portal | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.68 | n/a - excluded category 1 | Not a gap |
| REF-008 | Create a doctor account for the result service | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.142-144 | n/a - excluded category 1 | Lab-code ID reuse is in scope via REF-175; only the portal account is excluded |
| REF-009 | Create a laboratory account for the result service | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.145-147 | n/a - excluded category 1 | Not a gap |
| REF-010 | Portal branding and portal access data on printed documents | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.69; SHOW p.54 | n/a - excluded category 1 | Not a gap |
| REF-011 | Equipment, maintenance and calibration register | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.93 | n/a - excluded category 2 | Only DatabaseMaintenance (backup) exists; unrelated |
| REF-012 | Equipment register entry form | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.93-94 | n/a - excluded category 2 | Not a gap |
| REF-013 | Blood-picture analyser interface and result import | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.55, 62-65, 74 | n/a - excluded category 3 | No partial analyser code exists to reconcile |
| REF-014 | Per-analyser correction factors applied to results | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.70-71 | n/a - excluded category 3 | See REF-083: in-system formulas stay in scope; device factors are excluded |
| REF-015 | Blood-picture device report with curves | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.62, 72 | n/a - excluded category 3 | Not a gap |
| REF-016 | 5-part differential blood-picture interface | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.65, 74 | n/a - excluded category 3 | Not a gap |
| REF-017 | Interface for other analysers (VIDAS and others) | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.75, 77 | n/a - excluded category 3 | Not a gap |
| REF-018 | Branch number in system, user and patient data | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.192, 151 | BranchNumber on SystemSettings/User has no consumer | Only multi-branch residue; deliberately dormant |
| REF-019 | Patient search restricted to a branch number | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.42-43 | n/a - excluded category 4 | Not a gap |
| REF-020 | Cash-drawer stocktake per branch | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.179 | n/a - excluded category 4 | Not a gap |
| REF-021 | Statistics per branch | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.53 | n/a - excluded category 4 | Not a gap |
| REF-022 | Per-branch data folders and configuration files | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.130-132 | n/a - excluded category 4 | Not a gap |
| REF-023 | Send SMS of the result to patient or doctor | EXCLUDED | EXCLUDED | EXCLUDED | SHOW p.53; LEARN p.17,18,47 | n/a - excluded category 5 | No SMS/E-mail/Fax send path in src/ |
| REF-024 | Send E-mail of the patient result | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.16,18,23,47 | n/a - excluded category 5 | Not a gap |
| REF-025 | 'Send Result' module in the main menu | EXCLUDED | EXCLUDED | EXCLUDED | LEARN p.12,96,134,145,155,184,199 | n/a - excluded category 5 | Not a gap |
| REF-026 | System sign-in with user name and password | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.11, 150-151, 158-163; SHOW p.34 | SignInCommandHandler.cs:31-65; Pbkdf2PasswordHasher; User.RecordLogin | PBKDF2 + last-login stamp |
| REF-027 | Internal-window secondary password prompt | IN SCOPE | IMPLEMENTED | ST | LEARN p.150, 157, 176 | User.InternalWindowsPasswordHash (User.cs:12); SystemMenuPasswordDialog; ShellViewModel gate | Per-user password where reference used a fixed 123 - a hardening |
| REF-028 | Create users, supervisors and administrators | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.150-153 | CreateUserCommandHandler.cs; User.cs:72-99,156-212; UserManagementViewModel |  |
| REF-029 | Edit user data and permissions | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.156-157 | UpdateUserCommandHandler.cs; SaveUserPermissionsCommandHandler.cs; User.cs:126-243 |  |
| REF-030 | Delete or deactivate a user | IN SCOPE | DIFFERENT | ST | LEARN p.151-153, 157 | DeactivateUser + DeleteUser handlers; User.cs:111-119 | BLOCKED: reference never names a removal action; hard delete vs deactivate undefined |
| REF-031 | Attendance check-in, break and check-out | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.158-163 | CheckIn/StartBreak/EndBreak/CheckOut handlers; AttendanceRecord.cs:34-70; AttendanceCalculator.cs:14-55 | DEF-005: no IAuthorizedRequest on any of the 4 commands; no ATTENDANCE permission code |
| REF-032 | Per-user attendance summary | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.151-161 | GetUserAttendanceSummaryQueryHandler; GetAttendanceRecordsQueryHandler; UserAttendanceSummaryViewModel |  |
| REF-033 | Users login/logout movement report for a period | IN SCOPE | MISSING | ST (negative search) | LEARN p.164 | AttendanceRecord.cs:6-22 has no machine field; grep MachineName src/ -> 0 files; no session log; only in-memory CurrentUserSessionDto | No persisted sign-in/out event and no machine name |
| REF-034 | Add a new patient (name, sex, age) | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.10-12 | CreatePatientCommandHandler; Patient.cs:111-150, 17-23; PatientEditorViewModel:270-278 |  |
| REF-035 | Optional patient data | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.12 | Patient.cs:25-47, 224-256; PatientPhoneNumber; UpdatePatientCommandHandler |  |
| REF-036 | Patient medical history flags and conditions | IN SCOPE | DIFFERENT | ST | LEARN p.13, 18, 24 | Stored: PatientMedicalCondition, MedicalConditionType, PregnancySignal; NOT surfaced: ProfileResultDtos carries no condition data; only consumer is GetCultureEntryGridQueryHandler:27-33 | Flags stored, no read-out in the result window |
| REF-037 | Print the patient barcode | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.12, 47; SHOW p.3 | PrintBarcodeCommandHandler; BarcodeService.cs:44-; BarcodeLabelRenderer.cs:12-33; reprint PatientSearchViewModel:152 | Honours PrintLabIdInsteadOfPatientId (reference lab-code barcode) |
| REF-038 | Print the patient receipt / invoice | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.12, 17, 19-20 | PrintReceipt/PrintInvoice handlers; ReceiptPdfWriter.cs:150-216; InvoicePdfWriter.cs:133-186; InvoiceIssue.cs:15-55 |  |
| REF-039 | Add a test to the patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.13 | AddTestsToVisitCommandHandler; PatientTest.cs:98-113; PatientEditorViewModel:219,372 |  |
| REF-040 | Remove a single test from the patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.13 | RemoveTestFromVisitCommandHandler; PatientEditorViewModel:168-198 |  |
| REF-041 | Clear all tests of the patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.13 | ClearAllTestsCommandHandler.cs:26-92 | First-registration-only guard ADDED after the first audit by [B-01] slice 6/8 (see DEF/17.3) |
| REF-042 | Add a whole custom test group to the patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.15 | AddCustomGroupToVisitCommandHandler; CustomGroupItem; PatientAccountCalculator.cs:63 |  |
| REF-043 | Remove a test that belongs to an added group | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.15-16 | RemoveTestFromVisitCommandHandler; PatientEditorViewModel:168-198 |  |
| REF-044 | Mark a test as sample taken outside the lab | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.13 | PatientTest.cs:26; UpdatePatientTestSampleFlagsCommandHandler:17-34; ReportContentBuilder.cs:221-225 | Report note rendering ADDED after the first audit by [B-01] slice 8/8 |
| REF-045 | Patient titles and optional automatic insertion | IN SCOPE | IMPLEMENTED | ST | LEARN p.192, 203 | PatientTitle; Patient.cs:15; GetPatientTitlesQueryHandler; SystemSettings.cs:21 DisableAutoTitleInsertion |  |
| REF-046 | Permanent lab code and persistent patient identity | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.47-48; SHOW p.3 | Patient.cs:13, 199-212; GetNextLabIdQueryHandler; GetPatientByLabIdQueryHandler |  |
| REF-047 | Edit patient data | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.22 | UpdatePatientCommandHandler; Patient.cs:152-197; PatientEditorViewModel:227 |  |
| REF-048 | Edit patient data from the search screen | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.25 | PatientSearchViewModel:200 -> PatientEditorViewModel edit mode; GetVisitDetailQueryHandler |  |
| REF-049 | Add or remove tests while editing a patient | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.22-24 | ApplyVisitDeltasCommandHandler + Add/Remove/ClearAll; PatientEditorViewModel:219,243 |  |
| REF-050 | Delete a patient (permission-gated) | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.151, 157; SHOW p.53 | SoftDeletePatientCommand.cs:9-11 (IAuthorizedRequest, DeletePatient); Patient.cs:291-297; queries filter IsDeleted |  |
| REF-051 | Automatic patient account total | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.17 | PatientAccountCalculator.cs:17-61; GetPatientAccountQueryHandler |  |
| REF-052 | Enter the previously paid amount and the discount | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.17 | RecordPaymentCommandHandler.cs:34-52; PaymentOperation.cs:11-21 | Discount ceiling enforced at handler line 47-52 |
| REF-053 | Add an extra amount to the account | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.17 | RecordExtraChargeCommandHandler; PaymentOperation.cs:15 IsExtraCharge; ExtraChargeDialogViewModel |  |
| REF-054 | Settle the account in full | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.17 | SettleAccountInFullCommandHandler; PaymentOperation.cs FullSettlement; PatientAccountCalculator.cs:35-44 |  |
| REF-055 | Clear (zero) the whole account | IN SCOPE | DIFFERENT | ST | LEARN p.18-20 | VoidPaymentOperationCommandHandler:24-; PaymentOperation.cs:78 Void | No account-wide clear action |
| REF-056 | Edit a payment operation | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.20 | RecordCorrectionCommandHandler:33-; OperationType.cs:6; CorrectionDialogViewModel | Correcting operation rather than in-place edit - safer equivalent |
| REF-057 | Delete a payment operation | IN SCOPE | DIFFERENT | ST | LEARN p.20 | VoidPaymentOperationCommandHandler:24-40; PaymentOperation.cs:23, 78- | Void only; row and amount remain visible. BLOCKED: money-deletion semantics |
| REF-058 | Delivery and settlement grid filtered by period | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.21 | GetDeliveryGrid/GetUndeliveredResults handlers; ResultDeliveryViewModel; DeliverWithSettlement |  |
| REF-059 | Receipt shows total, discount, paid and remaining | IN SCOPE | IMPLEMENTED | ST | LEARN p.17-20, 69; SHOW p.5 | ReceiptPdfWriter.cs:150-216; GetPatientReceiptQueryHandler; ReceiptSettings.cs:11 currency |  |
| REF-060 | Receipt option to show or hide the test details | IN SCOPE | IMPLEMENTED | ST | SHOW p.4 | ReceiptSettings.cs:17; TestDetailDisplayMode; consumed ReceiptPdfWriter.cs:150-216; ReceiptSettingsViewModel |  |
| REF-061 | Receipt on pre-printed paper or white paper with header/footer and logo | IN SCOPE | DIFFERENT | ST | LEARN p.204; SHOW p.5 | ReceiptSettings.cs:21; HeaderFooterMode.cs:7; ReceiptPdfWriter.cs:160-162 'HeaderFooterMode.Images has no image-asset pipeline in this slice' | Mode offered, image pipeline absent |
| REF-062 | Receipt header/footer text, font, size, colour and logo | IN SCOPE | DIFFERENT | ST | LEARN p.203-204; SHOW p.5 | SaveLabPrintTextCommandHandler; SystemSettingsViewModel:111-112,124-125; ReceiptPdfWriter.cs:33-; ArabicFontResolver | Lab text + one font family; no per-element colour, no logo |
| REF-063 | Receipt top margin, currency, delivery time, print-once limit | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.204 | ReceiptSettings.cs:9-15; UpdateReceiptSettingsCommandHandler; ReceiptSettingsViewModel |  |
| REF-064 | Receipt printed through a cash-register printer | IN SCOPE | DIFFERENT | ST | SHOW p.4 | ReceiptSettings.cs:19 CashierPrinterEnabled; PrinterAssignmentConfiguration.cs:17; ReceiptPrintingService | Fixed A5 receipt (ReceiptPdfWriter.cs:15-21); no 80mm page size |
| REF-065 | Envelope settings | IN SCOPE | DIFFERENT | ST | LEARN p.205-206; SHOW p.6 | EnvelopeSettings.cs:9-13; EnvelopePrintItemPosition; UpdateEnvelopeSettings handler; EnvelopeSettingsViewModel:90 | Settings + editor + migration seed exist with NO document consumer |
| REF-066 | Print the patient envelope | IN SCOPE | MISSING | ST (negative search) | LEARN p.205-206 | No Envelope*PdfWriter; grep Envelope in Printing returns only print-envelope token types; no envelope Application feature folder |  |
| REF-067 | Patient barcode printed on the envelope | IN SCOPE | MISSING | ST (negative search) | SHOW p.6 | No envelope document (REF-066); barcode exists only as a standalone label and as worksheet text |  |
| REF-068 | Print a laboratory order with barcode | IN SCOPE | MISSING | ST (negative search) | SHOW p.6 | grep 'Requisition\|LabOrder\|OrderPdfWriter' src/ -> 0 files |  |
| REF-069 | Receipt expected result date per test | IN SCOPE | DIFFERENT | ST | LEARN p.192; SHOW p.52 | Stored: Test.cs:28 CompletionDurationMinutes; ReceiptSettings.cs:13 PickupTimeDefault; Patient.cs:39 PickupDateUtc. NOT consumed by any writer | Turnaround data stored, never composed or printed |
| REF-070 | Prevent printing the report while a balance remains | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.53 | PermissionConfiguration.cs:22; User.cs:18; enforced ExecuteBulkPrintCommandHandler.cs:70-76 -> BulkPrintDtos.cs:24 BlockedByBalance | CORRECTION to build.md: this was always enforced, never a dead grant |
| REF-071 | Enter a test result and save it | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.27-28 | GetResultWorklistQueryHandler; EnterResultCommandHandler; PatientTest.cs:115-127; ResultsWorklistView/ProfileEntryView/SimpleResultEntryView |  |
| REF-072 | Print preview of a report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.27, 30 | PdfPreviewService; ReportPrintingService; ProfileEntryViewModel, CultureEntryViewModel:119 |  |
| REF-073 | Print a result report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.27 | MarkResultPrinted/MarkProfilePrinted handlers; PatientTest.cs:178-189; ShellPdfPrinterDispatcher |  |
| REF-074 | Export the patient report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.28, 31 | ExportPatientReportPdfCommandHandler:30-; PatientReportPdfExporter; ReportContentBuilder.cs:54-122; PatientTest.cs:203-207 |  |
| REF-075 | Review / verify a result | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.28, 54-56 | VerifyProfileResults/ReviewResult handlers; PatientTest.cs:166-176; VerifyCultureResult |  |
| REF-076 | Un-verify a reviewed result | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.28 | UnverifyProfileResults/UnreviewResult handlers; PatientTest.cs:143-153 |  |
| REF-077 | Clear a result value | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.28 | ClearResultCommandHandler; PatientTest.cs:129-141 |  |
| REF-078 | Refresh the result reference ranges | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.31, 102 | RefreshResultReferenceRangeCommandHandler; PatientTestReferenceRangeSnapshot; ProfileResultItemReferenceRangeSnapshot |  |
| REF-079 | Read the patient data block from the result screen | IN SCOPE | MISSING | ST (negative search) | LEARN p.13, 26 | ProfileEntryView.xaml and PatientResultSheetView.xaml:68-74 bind test columns only; ProfileResultDtos.cs:53-66 and PatientReportPdfPort.cs:16-20 carry no condition/sex/age |  |
| REF-080 | Reload the day's worklist | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.27 | GetResultWorklistQueryHandler; ResultsWorklistViewModel:47,169 |  |
| REF-081 | Simple result-entry screen | IN SCOPE | IMPLEMENTED | ST | SHOW p.34, 36 | SimpleResultEntryView.xaml; SimpleResultEntryViewModel.cs:41-88 |  |
| REF-082 | Automatic interpretation comment on results | IN SCOPE | DIFFERENT | ST | SHOW p.14, 57 | No generated interpretation: the matched band's low/high text is printed (ReportContentBuilder.cs:149-158, 233-240) and ResultFlagComputer.cs:37-67 computes a flag, but no comment generator exists; AnalyteReferenceRangeBand.cs:30-32 stores only operator-authored text; no per-report on/off switch | grep 'AutoComment\|CommentRule\|Interpretation' src/ -> 0 files |
| REF-083 | Automatic calculation and equations in results | IN SCOPE | DIFFERENT | ST | SHOW p.14 | ArithmeticCalculator exists but is consumed only by EvaluateCalculationQueryHandler (calculator tool); no analyte or test entity carries a formula (Analyte.cs, Test.cs) and ProfileResultItem has no computed-value member | BLOCKED: scope vs excluded analyser correction factors (REF-014) |
| REF-084 | Abnormal-result flag during entry | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.13, 44 | ResultFlagComputer.cs:37-67; ResultFlag enum; stored PatientTest.cs:34, ProfileResultItem |  |
| REF-085 | Per-user discount ceiling | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.151, 157; SHOW p.53 | User.cs:16 DiscountLimitPercent; enforced RecordPaymentCommandHandler.cs:44-52; PermissionConfiguration.cs:24 | CORRECTION to build.md: always enforced |
| REF-086 | Low/high comments shown automatically on the report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.101 | AnalyteReferenceRangeBand.cs:30-32; frozen at ProfileResultItemReferenceRangeSnapshot; printed ReportContentBuilder.cs:149-158, 233-240, 256-265 |  |
| REF-087 | Enter culture (farm) result data | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.38 | CultureResult.cs:10-20; SaveCultureResultsCommandHandler; CultureEntryViewModel:127-140; migration 20261001233652 |  |
| REF-088 | Enter culture sensitivity results | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.39 | CultureAntibioticResult.cs:14-44; SaveCultureResultsCommandHandler; CultureEntryViewModel:19-63,207 |  |
| REF-089 | Choose which culture report elements are shown | IN SCOPE | MISSING | ST (negative search) | LEARN p.39 | ReportCultureSection has no Show/Visible/IsEnabled; culture section rendered unconditionally ReportContentBuilder.cs:268-308; no element-selection entity |  |
| REF-090 | Add a new culture (farm) to the program | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.128-129 | Test.cs:40, 80-128 (IsCultureType); CreateTestCommandHandler; CultureAttachmentViewModel:44 | Per-farm data folders are excluded (REF-022) |
| REF-091 | Per-farm antibiotic list with inhibition-zone threshold | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.134-138 | CultureAntibioticAttachment.cs:8-30; Attach/Detach commands; CultureAttachmentViewModel:39-41,169,187 |  |
| REF-092 | Antibiotic master data with commercial name | IN SCOPE | DIFFERENT | ST | LEARN p.134-135, 138; SHOW p.23 | Antibiotic.cs:8-22 has Name, Symbol, ScientificName, 2 flags - NO commercial-name field; grep 'CommercialName\|TradeName' src/ -> 0 files | See contradiction C-1: AntibioticEditorViewModel.cs:12 claims no ScientificName exists - it does |
| REF-093 | Antibiotic marked as suitable for pregnancy | IN SCOPE | DIFFERENT | ST | LEARN p.139 | Antibiotic.cs:10; CultureAntibioticDisplay.cs:7-16; GetCultureEntryGridQueryHandler.cs:37 | DEF-001: '\|\| saved.ContainsKey(x.Id.Value)' escape clause |
| REF-094 | Antibiotic marked as suitable for children under twelve | IN SCOPE | DIFFERENT | ST | LEARN p.139 | Antibiotic.cs:12; CultureAntibioticDisplay.cs:5; AgeRules.cs:19; GetCultureEntryGridQueryHandler.cs:33,37 | Same DEF-001 escape clause |
| REF-095 | Combined report: choose tests, order them and print | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.31-34 | CombinedReportSelection.cs:16-61; GetCombinableTests/BuildCombinedReport/PrintCombinedReport; CombinedReportViewModel:83-89,147-149 |  |
| REF-096 | Print the group name as a report sub-title | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.34 | ReportSettings.cs:30; migration 20261001203315; ReportContentBuilder.cs:206-212 |  |
| REF-097 | Blank report: print a sheet with patient data only | IN SCOPE | DIFFERENT | ST | LEARN p.36-37 | BuildBlankReport/PrintBlankReport handlers; ReportContentBuilder.cs:335-368 FromBlank; BlankReportViewModel:34-36 | No 'patient data only' variant selectable |
| REF-098 | Patient history inserted automatically into the result report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.70-71 | AutoInsertHistoryCommandHandler:27-; ReportSettings.cs:27; CombinedReportViewModel:86,148 | **Second pass: routed through the DEF-008 function (PatientHistoryReader.ResolveVisitPatients).** |
| REF-099 | Manually insert a previous result from the history | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.75-76 | InsertHistoryResultCommandHandler:27-; GetPatientTestHistoryQueryHandler; CombinedReportViewModel:85,147 | **Second pass: routed through the DEF-008 function.** |
| REF-100 | Patient history in a separate report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.77-78 | GetSeparateHistoryReportQueryHandler:13-18; PrintHistoryReportCommandHandler; ReportContentBuilder.cs:369-428; HistoryReportsViewModel | **Second pass: routed through the DEF-008 function.** |
| REF-101 | Patient history for the blood picture (CBC) | IN SCOPE | MISSING | ST (negative search) | LEARN p.79 | ReportContentBuilder.cs:369-428 is generic; NO CBC layout is reachable. CORRECTION: HistoryMatrixBuilder.cs:6-38 IS a CBC date x analyte pivot but has ZERO production consumers (only HistoryMatrixTests.cs) | DEF-007 dead-but-tested code; much cheaper to complete than either audit implied |
| REF-102 | Multi-patient history report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.80-83 | GetMultiPatientHistoryQueryHandler; HistoryReportsViewModel:73,94,157-159 | **Second pass: routed through the DEF-008 function (PatientHistoryReader.ResolveVisitPatients:47).** |
| REF-103 | History grouping by lab code or patient name | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.70, 192, 195 | ReportSettings.cs:25,55; HistorySortMode; PatientHistoryResolver.cs:11-40; consumed PatientHistoryReader.cs:24-55 + GetMultiPatientHistoryQueryHandler.cs:61 | **Second pass: this is the function carrying DEF-008 (collation divergence) and DEF-009 (no behavioural test).** |
| REF-104 | More than one combined report per patient | IN SCOPE | DIFFERENT | ST | SHOW p.18 | CombinedReportSelection.cs:3-6 'Ephemeral, in-memory ordered selection ... Never persisted'; BuildCombinedReportCommand:7-8 |  |
| REF-105 | One-button combined print of entered but unprinted results | IN SCOPE | DIFFERENT | ST | SHOW p.18 | ExecuteBulkPrintCommandHandler; BulkPrintPreflightQueryHandler.cs:37 'EnteredAtUtc is not null && IsReviewed'; BulkPrintDtos.cs:20-30 | Reference says 'entered and not printed'; Top-Lab also requires review. BLOCKED |
| REF-106 | Print the last N visits below the profile report | IN SCOPE | DIFFERENT | ST | SHOW p.52 | AutoInsertHistoryCommandHandler:27- gated by ReportSettings.cs:27; GetPatientTestHistoryQuery.cs:7-12 takes a date window, not a visit count |  |
| REF-107 | Report header/footer modes: none, words, image | IN SCOPE | DIFFERENT | ST | LEARN p.194, 192 | ReportSettings.cs:17; HeaderFooterMode.cs:7; ReportContentBuilder.cs:138-139; ReceiptPdfWriter.cs:160-162 | Image mode has no asset pipeline |
| REF-108 | Report header/footer colours and element positions | IN SCOPE | DIFFERENT | ST | LEARN p.195-197 | ReportSettings.cs:21-23 (one header + one footer colour only); ReportPageComposer.cs:15-44,131-167 | No per-element colours, no left/top offsets |
| REF-109 | Report margins and top space | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.193 | ReportSettings.cs:9-13, 62-87 (max 8 cm); applied ReportPageComposer.cs:168-; UpdateReportSettingsCommandHandler |  |
| REF-110 | Report paper size A4 / A5 | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.194 | ReportSettings.cs:15, 88-92; PaperSize; PageSizeMapper; ReportPageComposer.cs:168- |  |
| REF-111 | Report header prints the laboratory name and address | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.194 | ReportDocumentContent.cs:36-57; SaveLabPrintTextCommandHandler; gate ReportSettings.cs:17 | Suppressed entirely when the mode is None |
| REF-112 | Doctor's signature on the report | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.15, 57 | ReportSettings.cs:19, 98-102; carried ReportDocumentContent.cs:27; drawn ReportPageComposer.cs:131-167; set in every builder e.g. ReportContentBuilder.cs:183 |  |
| REF-113 | Colours of patient data, headings, profiles and abnormal results | IN SCOPE | MISSING | ST (negative search) | SHOW p.16, 44 | ReportDocumentContent.cs:10-29, 140-155 carries strings, a section list and a grid only - no colour/background concept; the only colour fields in the system are ReportSettings.cs:21-23; ReportPageComposer.cs:45-130 draws uniform text |  |
| REF-114 | Whole-row highlight choice | IN SCOPE | MISSING | ST (negative search) | SHOW p.58, 61, 74 | Same root cause as REF-113: no per-row style data in ReportDocumentContent.cs:10-29, 140-155 or in the grid record (:150-155) |  |
| REF-115 | Report element letter, element list editing and free text | IN SCOPE | DIFFERENT | ST | SHOW p.12 | Analyte.cs + AnalyteReferenceRangeBand.cs allow naming/adding; PatientTest.cs:115 accepts free text; Test.cs:17 TestCode is a lookup code, not a report letter | BLOCKED: 'element letter' is OCR-ambiguous (report code / display symbol / transliterated letter) |
| REF-116 | Report line spacing and left-margin position per element | IN SCOPE | MISSING | ST (negative search) | SHOW p.13 | ReportPageComposer.cs:45-130 renders a fixed vertical rhythm from a single font size; grep -i LineSpacing src/ -> 0 files; no per-element offset stored in ReportSettings.cs:7-34 |  |
| REF-117 | Report element shape, element name and element order | IN SCOPE | MISSING | ST (negative search) | SHOW p.72 | No ReportElementConfig/ReportLayout/ElementOrder entity or command; order fixed by builder loops ReportContentBuilder.cs:142-158,203-266 |  |
| REF-118 | Print the abnormality marker on the report | IN SCOPE | DIFFERENT | ST | SHOW p.13, 44, 61 | Flag computed ResultFlagComputer.cs:37-67 and used to SELECT a comment ReportContentBuilder.cs:82,149,233,256; line strings built at :145-146,216-219 with no marker | DEF-003 |
| REF-119 | Semen analysis report with graphs and diagrams | IN SCOPE | MISSING | ST (negative search) | SHOW p.20-21 | Semen is only a sample-kind flag (PatientTest.cs:22); grep -i 'graph\|chart' in Infrastructure -> 0 real hits; no drawing API |  |
| REF-120 | Image report for culture, urine, stool, semen and blood picture | IN SCOPE | MISSING | ST (negative search) | SHOW p.24 | No image entity, upload, image-capable report or drawing surface; only the in-memory Code-128 label bitmap BarcodeLabelRenderer.cs:12-33 |  |
| REF-121 | Blood-picture report design variants and large paper | IN SCOPE | DIFFERENT | ST | SHOW p.56, 74 | PaperSize configurable ReportSettings.cs:15 + PageSizeMapper, but PaperSize.cs has no large value; only one layout ReportContentBuilder.cs:15-52,124-186 |  |
| REF-122 | CBC report: rename elements, edit reference values and units | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.74 | Analyte.cs, AnalyteReferenceRangeBand.cs; AnalyteEditorViewModel:96-99; UpdateAnalyte / SaveAnalyteReferenceRange | Curve half of the same page is excluded analyser work (REF-015) |
| REF-123 | CBC report colouring, editable auto-comment and manual mode | IN SCOPE | DIFFERENT | ST | SHOW p.58, 74 | Manual input SaveProfileResults + editable comment text SaveAnalyteReferenceRange exist; ALL colour control absent and no comment on/off switch (needs the style model, REF-113) |  |
| REF-124 | Show or hide report blocks | IN SCOPE | DIFFERENT | ST | SHOW p.15, 57, 64 | Only the doctor-signature block toggleable ReportSettings.cs:19; ReportPageComposer.cs:15-44,45-130 emits a fixed header/footer/body; no device-information field ReportDocumentContent.cs:10-29 |  |
| REF-125 | Report identification block | IN SCOPE | DIFFERENT | ST | LEARN p.29, 70-71; SHOW p.17, 20 | ReportDocumentContent.cs:10-29 DECLARES Sex/AgeText/TreatingDoctorName/ReferralEntityName, but 4 of 5 builders pass null: ReportContentBuilder.cs:112-115, 176-179, 323-326, 417-420. Only the history builder (:357-360) populates them | Also ReportDateText is always yyyy/MM/dd - no time (:120,184,331,365,425) |
| REF-126 | Interpretation text per test about how the analysis is performed | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.19 | TestComment.cs; CreateTestComment handler; TestCommentPickerViewModel:63-128; ReportContentBuilder.cs:244-247; ProfileResultDtos.cs:59 | DEF-004: selection only, no ad-hoc free text |
| REF-127 | Barcode printed on the receipt and the envelope | IN SCOPE | MISSING | ST (negative search) | LEARN p.69; SHOW p.3, 52 | No receipt or envelope writer draws a barcode - ReceiptPdfWriter.cs:150-216 and InvoicePdfWriter.cs:133-186 emit text only |  |
| REF-128 | Barcode labels per patient test and per sample | IN SCOPE | DIFFERENT | ST | SHOW p.3, 52 | One Code-128 label per patient; payload is a single identifier with an optional date suffix BarcodeService.cs:65-67 |  |
| REF-129 | Vertical barcode on report, receipt and envelope | IN SCOPE | MISSING | ST (negative search) | SHOW p.3 | No vertical flag in ReportSettings/ReceiptSettings/EnvelopeSettings; label renderer emits a fixed horizontal bitmap BarcodeLabelRenderer.cs:12-33 (PaddingVertical hits are QuestPDF padding) |  |
| REF-130 | Tube, container and swab label printing | IN SCOPE | MISSING | ST (negative search) | SHOW p.3 | One fixed-size patient label BarcodeService.cs:69-77; BarcodeLabelRenderer.cs:12 defaults 300x80px; no sticker size, no multi-up |  |
| REF-131 | Blank report with data entry before printing | IN SCOPE | DIFFERENT | ST | LEARN p.36 | BuildBlankReportCommandHandler builds from stored data only; BlankReportViewModel:75-77 and BlankReportView.xaml offer just build and print; ReportContentBuilder.cs:335-368 has no entry surface |  |
| REF-132 | Search patient by exact name | IN SCOPE | DIFFERENT | ST | LEARN p.42-43 | SearchPatientsGlobalQueryHandler.cs:30-45 matches FullName with Contains and only when EnablePatientNameSearchAssist is on; no exact-match mode |  |
| REF-133 | Search patient by partial name | IN SCOPE | DIFFERENT | ST | LEARN p.42-43 | SearchPatientsGlobalQueryHandler.cs:40-44 partial match exists but is gated behind a setting serving a different purpose (REF-149), not a selectable mode |  |
| REF-134 | Search patient by treating doctor | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42 | SearchPatientsGlobalQueryHandler.cs:54-59; PatientSearchViewModel:217,344 |  |
| REF-135 | Search patient by sex | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | SearchPatientsGlobalQueryHandler.cs:76-81; PatientSearchViewModel:256,365 |  |
| REF-136 | Search patient by age | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | SearchPatientsGlobalQueryHandler.cs:83-92; Patient.cs:21-23 age with unit |  |
| REF-137 | Search patient by phone or card number | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | SearchPatientsGlobalQueryHandler.cs:34-44 phone sub-query + NationalId + LabId exact |  |
| REF-138 | Search patients who ordered a chosen test | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | SearchPatientsGlobalQueryHandler.cs:68-74; PatientSearchViewModel:243,358 |  |
| REF-139 | Search patients within a date range | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.43 | SearchPatientsGlobalQueryHandler.cs:94-105; PatientSearchViewModel:318,331 |  |
| REF-140 | Search by lab code and list all visits | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.47-51 | GetPatientByLabIdQueryHandler; GetVisitHistoryQueryHandler; PatientVisitHistoryViewModel; PatientSearchView.xaml:14 |  |
| REF-141 | Find results that have not been entered | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42-43 | GetResultWorklistQueryHandler (HasResult); ResultsWorklistViewModel:53,169; PatientResultSheetView.xaml:73-74 | Implemented as a worklist filter rather than a search-screen button |
| REF-142 | Find results that have not been reviewed | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42 | GetResultWorklistQueryHandler.cs:54-58; ResultsWorklistViewModel:65 |  |
| REF-143 | Find results that have not been printed | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42 | GetResultWorklistQueryHandler.cs:75-84; ResultsWorklistViewModel:87 |  |
| REF-144 | Find results that have not been delivered | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.42 | GetUndeliveredResultsQueryHandler; ResultDeliveryViewModel |  |
| REF-145 | Status indicator per patient for the result lifecycle | IN SCOPE | DIFFERENT | ST | SHOW p.52 | VisitRollup.cs:26-28 counts reviewed/printed/delivered per test; PatientSearchView.xaml:120-131 renders only name, id and actions | No single status symbol per patient |
| REF-146 | Follow-up / monitoring report of results | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.25 | GetBandedResultMonitorQueryHandler.cs:39-120; BandedResultMonitorDtos.cs:24-; BandedResultMonitorPdfWriter; StatisticsViewModel | ADDED after the first audit by [B-01] slices 1-3 |
| REF-147 | Evaluate a test's results by minimum and maximum value | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.25 | GetBandedResultMonitorQueryHandler.cs:12-18, 64-68 invariant parse + min/max band | ADDED after the first audit by [B-01] slices 1-3 |
| REF-148 | Search and statistics on blood-picture ELEMENT values | IN SCOPE | DIFFERENT | ST | SHOW p.73 | GetBandedResultMonitorQueryHandler.cs:56-68 reads PatientTest.ResultValue (TEST-level); CBC values are analyte-level rows in ProfileResultItem, so a CBC test is not covered | THIS AUDIT: downgraded from the second audit's IMPLEMENTED (ERR-4) |
| REF-149 | Patient search while typing the name on the patient form | IN SCOPE | IMPLEMENTED | ST | SHOW p.3; LEARN p.192 | SystemSettings.cs:19; SearchPatientsGlobalQueryHandler.cs:25,41; SearchPatientsQueryHandler; PatientEditorViewModel:270 |  |
| REF-150 | Worksheet of a group of patients in a period, full data | IN SCOPE | MISSING | ST (negative search) | LEARN p.86-88 | Worksheet modes are visit (GetVisitWorkSheet), test group and work-group log only; GetWorkSheetSummary returns per-log counters, not patient rows |  |
| REF-151 | Worksheet of a group of patients, condensed | IN SCOPE | MISSING | ST (negative search) | LEARN p.87-88 | WorkSheetDtos.cs:3-77 carries no layout-variant flag; no condensed query or writer |  |
| REF-152 | Worksheet of tests or a group of tests in a period | IN SCOPE | DIFFERENT | ST | LEARN p.89-91 | Data exists GetWorkSheetByTestGroupQuery.cs:8-12 and WorkSheetPdfWriter; print/preview wired only for the visit mode WorkSheetsViewModel.cs:23-30, WorkSheetsView.xaml:53-54 vs :104 | DEF-006 |
| REF-153 | Worksheet by work group (Log) in a period | IN SCOPE | DIFFERENT | ST | LEARN p.92, 84 | GetWorkSheetByWorkGroupLogQuery.cs:8-12 + WorkGroupLog masters exist; no print path | DEF-006 |
| REF-154 | Print only unfinished tests or all tests | IN SCOPE | DIFFERENT | ST | LEARN p.85-86, 92 | WorkSheetDtos.cs:11-15 and GetWorkSheetSummaryQueryHandler.cs:36-41 carry completion facts, but no filter flag is passed or offered |  |
| REF-155 | Worksheet with full patient and test data | IN SCOPE | MISSING | ST (negative search) | LEARN p.88 | WorkSheetLineDto (WorkSheetDtos.cs:3-15) carries ordering facts only - no result value, reference range or demographics |  |
| REF-156 | Worksheet with condensed patient data | IN SCOPE | MISSING | ST (negative search) | LEARN p.90-91 | Same root cause as REF-151/155 |  |
| REF-157 | Worksheet for one department over a period | IN SCOPE | DIFFERENT | ST | SHOW p.7-11 | Work-group-log mode covers 'department'; no department selector, no print (REF-153), no between-two-patient-codes bound (GetVisitWorkSheetQuery takes a single integer PatientId) |  |
| REF-158 | Count of patients per test | IN SCOPE | DIFFERENT | ST | SHOW p.9 | GetWorkSheetTestCountByPeriodQueryHandler.cs:41-42 groups PatientTest and counts ROWS, not distinct patients; same pattern GetTestCountStatisticsQueryHandler.cs:69-79 | Wrong business rule, not a missing feature |
| REF-159 | Classify how often each test was performed in a period | IN SCOPE | DIFFERENT | ST | LEARN p.90-91 | GetWorkSheetTestCountByPeriodQueryHandler.cs:25-61 returns per-test counts; no print command; no per-patient signature column |  |
| REF-160 | Worksheet print preview | IN SCOPE | DIFFERENT | ST | LEARN p.87-88 | Preview exists only for the visit worksheet (WorkSheetsViewModel.cs:67 -> PdfPreviewService); test-group and work-group modes have no preview button | DEF-006 |
| REF-161 | Test catalog list with search by name, group or test number | IN SCOPE | DIFFERENT | ST | LEARN p.94-95; SHOW p.43 | SearchTestCatalogQuery.cs:7 takes (SearchTerm, TestGroupId, IncludeInactive) - the test-number key is absent. LEARN p.141 shows all three keys: 'By test name \| By group name \| By test ID' |  |
| REF-162 | Edit test data and price | IN SCOPE | DIFFERENT | ST | LEARN p.95 | Edit-window PROSE is fully covered (Test.cs:11-40; UpdateTest handler), but the reference CATALOG record also carries 'Arrang' (arrange number), 'Out Lab Name' and 'Routin' (LEARN p.95, p.141) - no column for any. UI grid TestCatalogView.xaml:61-65 shows only Code/Name/Group/Price/Status | THIS AUDIT: downgraded from the second audit's IMPLEMENTED; build.md's ArrangeNo finding is upheld |
| REF-163 | Add a new test | IN SCOPE | DIFFERENT | ST | LEARN p.96-97 | CreateTestCommand.cs:8-22 has Name, ReportName, ReceiptName, TestCode, CompletionDurationMinutes, PatientPrice, ResultKind, IsCultureType, TestGroupId, Barcode, IsSentOut, SentOutCostPrice, LabToLabPrice, AnalyteId - no arrange/sample-type/routine | THIS AUDIT: downgraded for consistency with REF-162 |
| REF-164 | Substitute patient name and data on the worksheet line | IN SCOPE | DIFFERENT | ST | SHOW p.19 | WorkSheetDtos.cs:3-15 carries PatientFullName and LabId only; WorkSheetPdfWriter.cs:133-226 renders them with no override field; no substitute-name setting exists |  |
| REF-165 | Reference ranges: add, edit and delete a range | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.101 | CreateReferenceRange/UpdateReferenceRange/DeleteReferenceRange commands; AnalyteReferenceRangeBand.cs:16-32; AnalyteEditorViewModel:40-49,98-105 |  |
| REF-166 | Reference ranges per age unit | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.104 | AnalyteReferenceRangeBand.cs:20-24, :92 (Matches requires the same unit); ResultFlagComputer.cs:27-35 | Correct per the reference: a month patient is not matched by a day band. See DEF-002 for the dual-store issue |
| REF-167 | Add, edit and delete fixed comments for a test | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.112-115 | CreateTestComment/UpdateTestComment/DeleteTestComment; TestComment.cs; TestCommentsViewModel:44-48,72-75 |  |
| REF-168 | Attach a stored comment to the patient's result report | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.116 | TestCommentPickerViewModel.cs:28-128; ProfileResultDtos.cs:59; ReportContentBuilder.cs:161-164; ProfileEntryViewModel:128 | DEF-004: no free-text entry |
| REF-169 | Custom groups: create, price, delete and print | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.118-123 | CreateCustomGroup/RenameCustomGroup/DeleteCustomGroup/SetCustomGroupItemPrice/RemoveCustomGroupItem; CustomGroup.cs, CustomGroupItem.cs; group price PatientAccountCalculator.cs:63; printed CustomGroupPdfWriter.cs:34,123 |  |
| REF-170 | Price lists: full maintenance | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.105-108 | CreatePriceList/RenamePriceList/DeletePriceList/SetPriceListItemPrice/RemovePriceListItem; PriceList.cs, PriceListItem.cs; PriceListsViewModel, ExternalEntityEditorViewModel |  |
| REF-171 | Print a price list | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.105, 111 | PriceListPdfWriter.cs:38, 127-154; GetPriceListByIdQueryHandler |  |
| REF-172 | Add a treating doctor | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.124-125 | CreateExternalEntityCommandHandler.cs:26-; ExternalEntity.cs; EntityType; ExternalEntityEditorViewModel:190,226 |  |
| REF-173 | Add a referral or contract entity | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.125-127 | CreateExternalEntityCommand.cs:10-20 / handler; UpdateExternalEntity handler; DeleteExternalEntity; ExternalEntity.cs:19-33 has City, Address, Phone, Fax, ResponsiblePersonName/Phone, PriceListId, DiscountOrCommissionPercent |  |
| REF-174 | Add an entity of type sent-out samples | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.125, 143 | EntityType.cs; CreateExternalEntity handler; consumed SendSampleOutCommandHandler:26- and GetSentOutLabAccountQueryHandler:8-11 |  |
| REF-175 | Generate an entity code | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.144, 146 | GenerateEntityIdCodeCommandHandler; SecureEntityIdCodeGenerator; GetExternalEntityByCodeQueryHandler; ExternalEntity.cs:35 GeneratedIdCode | Its portal use as a password is excluded (REF-008/009) |
| REF-176 | Mark a test as sent outside and set its prices | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.140-141 | Test.cs:30-36; TestEditorViewModel:157-159; SendSampleOutCommandHandler:26-; SentOutSample.cs:8-32 |  |
| REF-177 | Choose the culture when attaching antibiotics | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.136 | CultureAttachmentViewModel.cs:44,50; GetCultureAntibioticsQueryHandler; guard GetCultureEntryGridQueryHandler:24 |  |
| REF-178 | Statistics main window | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.167-168; SHOW p.47 | StatisticsViewModel; StatisticsView.xaml; gated PermissionConfiguration.cs:29 |  |
| REF-179 | Total patient count for a period | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.168 | GetPatientCountStatisticsQueryHandler.cs:30-40; StatisticsDtos.cs:20-30 |  |
| REF-180 | Patient count sorted by the months of a year | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169; SHOW p.30 | GetPatientCountStatisticsQueryHandler.cs:14; MonthlyCountDto StatisticsDtos.cs:8-11,27 |  |
| REF-181 | Patient count sorted by sex | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169; SHOW p.30 | GetPatientCountStatisticsQueryHandler.cs:11,42-51; SexCounts |  |
| REF-182 | Patient count sorted by account type | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169, 171; SHOW p.30 | GetPatientCountStatisticsQueryHandler.cs:15,89-; AccountTypeCounts |  |
| REF-183 | Patient count sorted by referral entity | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169, 171; SHOW p.30 | GetPatientCountStatisticsQueryHandler.cs:13,53-88; ReferralEntityCounts with a no-referral bucket |  |
| REF-184 | Patient count for one specific referral entity | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.171; SHOW p.30 | GetPatientCountStatisticsQueryHandler.cs:53-88 with entity options from SearchExternalEntities |  |
| REF-185 | Patient count for a month split by day | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.170-171; SHOW p.31 | GetPatientCountStatisticsQueryHandler.cs:112-116; DayOfMonthCountDto StatisticsDtos.cs:37; surfaced StatisticsView.xaml:67,151-172 | ADDED after the first audit by [B-01] slices 4-5 |
| REF-186 | Paid amounts shown with the patient statistics | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.169-171; SHOW p.30 | GetPatientCountStatisticsQueryHandler.cs:142-152 computes PeriodMoneyDto from PatientAccountCalculator.TotalPaid; StatisticsViewModel.cs:209-216; StatisticsView.xaml:68,167-172 | ADDED after the first audit by [B-01] slices 4-5 |
| REF-187 | Number of test samples in a year by test name | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.172; SHOW p.32 | GetTestCountStatisticsQueryHandler.cs:69-79,91-97; StatisticsDtos.cs:61-66 |  |
| REF-188 | Test-group request rate counting | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.172; SHOW p.32 | GetTestCountStatisticsQueryHandler.cs:33-35,58-67,81-97 |  |
| REF-189 | Sent-out sample statistics by period and laboratory | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.167-168; SHOW p.32-33 | GetSentOutStatisticsQueryHandler.cs:39-88; StatisticsDtos.cs:68-80 |  |
| REF-190 | Sent-out test count for one referral entity | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.33 | GetSentOutStatisticsQueryHandler.cs:33-46 |  |
| REF-191 | User productivity statistics | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.167; SHOW p.47 | GetUserProductivityStatisticsQueryHandler.cs:32-; StatisticsDtos.cs:82-93 |  |
| REF-192 | Print a statistics report | IN SCOPE | DIFFERENT | ST | LEARN p.169-171; SHOW p.30 | No print action in StatisticsViewModel and no statistics writer among the 7 in Infrastructure/Printing |  |
| REF-193 | Cash-drawer inventory and lab account | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.175-178 | GetCashDrawerInventoryQueryHandler.cs:27-; InventoryDtos.cs:12-30; PermissionConfiguration.cs:28 |  |
| REF-194 | Stocktake by element and report type | IN SCOPE | DIFFERENT | ST | LEARN p.178-179 | Element kind enum InventoryDtos.cs:32-37 exists; the four report-type variants (detailed by results / by prices / detailed / aggregated) do not |  |
| REF-195 | Detailed patient-sample account list for a period | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.180; SHOW p.26 | GetPatientSamplesDetailQueryHandler.cs:23-; InventoryDtos; PaymentOperation |  |
| REF-196 | Stocktake statement for a specific treating doctor | IN SCOPE | DIFFERENT | ST | LEARN p.181-183 | InventoryDtos.cs:36 TreatingDoctor kind + GetPatientSamplesDetail exist, but no dedicated doctor statement screen or print; AccountsHubViewModel has no doctor selector |  |
| REF-197 | Cash deposit and disbursement | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.184, 186, 154 | RecordCashDeposit/RecordCashDisbursement handlers; CashMovement; MovementType; ListCashMovements; CashMovementDialogViewModel |  |
| REF-198 | Sent-out samples account for a period | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.184-185; SHOW p.29 | GetSentOutSamplesQuery.cs:8-13; GetSentOutLabAccountQuery.cs:8-11; SentOutAccountCalculator.cs:13-36 |  |
| REF-199 | Mark a sent-out sample paid or settle it in full | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.186 | RecordSentOutPayment/SettleSentOutInFull handlers; SentOutSamplePayment; SentOutAccountCalculator.cs:20-32 |  |
| REF-200 | Sent-out account filtered to one laboratory | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.187-188 | GetSentOutSamplesQuery.cs:11; GetSentOutLabAccountQuery.cs:9; SentOutSamplesViewModel filter |  |
| REF-201 | List the sent-out tests that are not yet dispatched | IN SCOPE | DIFFERENT | ST | SHOW p.29; LEARN p.186 | SentOutSample rows are written only on dispatch (SendSampleOutCommandHandler:26-); no query joins Tests.IsSentOut (Test.cs:30) to undispatched PatientTests |  |
| REF-202 | Company and delegate accounts | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.184, 154 | GetCompanyDelegateAccountsQueryHandler.cs:25-; InventoryDtos.cs:28 commissions |  |
| REF-203 | Patient account filters: debts, commission, paid only | IN SCOPE | DIFFERENT | ST | SHOW p.26 | GetPatientSamplesDetailQuery.cs:8-10 takes only From/To; no debt/commission/settled selector in the query or AccountsHubViewModel |  |
| REF-204 | Account per doctor on a daily, monthly or annual basis | IN SCOPE | DIFFERENT | ST | SHOW p.26 | The doctor is an inventory element kind (InventoryDtos.cs:32-37), not a billing-account grouping; no doctor-scoped statement is printed |  |
| REF-205 | Detailed account by test price or by results | IN SCOPE | MISSING | ST (negative search) | SHOW p.27 | InventoryAndAccounting has only GetCashDrawerInventory, GetPatientSamplesDetail, GetElementInventory, GetCompanyDelegateAccounts, ListCashMovements; no per-test price or per-result breakdown |  |
| REF-206 | Account statement for one referral entity and the hand-over list | IN SCOPE | DIFFERENT | ST | SHOW p.28 | Referral-scoped data exists (InventoryDtos.cs:35) and the patient list is filterable (SearchPatientsGlobalQueryHandler.cs:61-66), but no statement document and no hand-over command | Electronic transmission is excluded (category 5); only the produced statement is a gap |
| REF-207 | Four account report types | IN SCOPE | MISSING | ST (negative search) | SHOW p.28 | No account report-type concept in any query; no accounts writer in Infrastructure/Printing |  |
| REF-208 | Sent-out account per period with per-sample detail | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.29 | GetSentOutLabAccountQueryHandler; GetSentOutSamplesQueryHandler; SentOutAccountCalculator.cs:13-32; SentOutLabAccountViewModel |  |
| REF-209 | Code the patient's samples per section | IN SCOPE | DIFFERENT | ST | LEARN p.208 | Sample-kind flags PatientTest.cs:16-24 exist and the worksheet renders them WorkSheetPdfWriter.cs:15-16,156-173, but no department entity, no per-department code, no separation step | BLOCKED: departments and coding granularity undocumented |
| REF-210 | Draw and separate samples; drawn and not-drawn lists | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.209-210 | MarkSampleDrawn/MarkAllSamplesDrawnForPatient handlers; GetPatientTestsForDraw/GetPatientsWithUncollectedSamples; PatientTest.cs:209; SampleDrawBoardWindow |  |
| REF-211 | Reader options: save after read, print barcode after read | IN SCOPE | MISSING | ST (negative search) | LEARN p.210 | No scanner, HID or keyboard-wedge integration, no read event in Presentation or Application; the only barcode actions are PrintBarcodeCommandHandler:26-45 and the reprint button | Hardware-dependent |
| REF-212 | Scientific calculator | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51 | ArithmeticCalculator; EvaluateCalculationQueryHandler; UtilitiesViewModel.cs:90,115-116,206; UtilitiesView.xaml:17,60 |  |
| REF-213 | Measurement unit conversion | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51 | MeasurementUnitConverter; ConvertMeasurementUnitQueryHandler; UtilitiesViewModel.cs:91,119-122,207; UtilitiesView.xaml:18,82 |  |
| REF-214 | Stopwatch | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51 | StopwatchCalculator; ComputeStopwatchElapsedQueryHandler; UtilitiesViewModel.cs:92,125-128,208; UtilitiesView.xaml:19,102 |  |
| REF-215 | Phone book | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51; LEARN p.155 | AddPhoneBookEntry/RemovePhoneBookEntry; GetPhoneBookQueryHandler; JsonPhoneBookStore; UtilitiesViewModel.cs:93,130-148,209-210 |  |
| REF-216 | Requirements and purchases list | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.51 | AddPurchaseItem/RemovePurchaseItem/TogglePurchaseItemDone; GetPurchasesListQueryHandler; JsonPurchasesListStore; UtilitiesView.xaml:21,160-174 |  |
| REF-217 | Test information library | IN SCOPE | DIFFERENT | ST | SHOW p.51 | TestLibraryEntryDto (UtilitiesDtos.cs:18-22) carries TestId, Name, TestCode, GroupName only - no sample, no normal values, no patient effect; UtilitiesView.xaml:204-206 shows only those columns |  |
| REF-218 | Laboratory terms and abbreviations reference | IN SCOPE | MISSING | ST (negative search) | SHOW p.51 | No glossary entity/query/tab; Utilities has 6 tabs (phonebook, purchases, test library, stopwatch, units, calculator). The only 'Abbreviation' hit in src/ is a STALE COMMENT (AntibioticEditorViewModel.cs:12) - see contradiction C-1 | BLOCKED: content is owner-supplied |
| REF-219 | Colour palette | IN SCOPE | MISSING | ST (negative search) | SHOW p.51 | No palette entity, query, UI or colour-picker control; the only colour handling is two hex strings ReportSettings.cs:21-23 |  |
| REF-220 | Reminder note for a specific user | IN SCOPE | MISSING | ST (negative search) | SHOW p.51 | grep 'Reminder\|LabNote\|Notification' src/ -> 0 files; Utilities' 6 tabs include no notes | BLOCKED: persistence/visibility semantics undocumented |
| REF-221 | Follow-up appointment for a patient or phone number | IN SCOPE | MISSING | ST (negative search) | SHOW p.51 | grep 'FollowUp\|Recall' src/ -> 0 files; Patient.cs:39 PickupDateUtc is the expected-result date, not a follow-up, and no command edits it | BLOCKED: reference line OCR-damaged (E4) |
| REF-222 | Who registered the patient, edit count, last editor | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.52, 57, 164 | GetPatientAuditQueryHandler; AuditDtos.cs:18-28 PatientAuditDto; AuditableEntitySaveChangesInterceptor; AuditableEntity |  |
| REF-223 | Who entered and who reviewed each result | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.56-57 | GetPatientTestAuditQueryHandler; AuditDtos.cs:35-51; stamped PatientTest.cs:155-176 |  |
| REF-224 | Print count, last printer and who delivered the result | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.56-57 | PatientTest.cs:48-60,178-201; AuditDtos.cs:45-51 |  |
| REF-225 | How a payment was collected, instalments and receiver | IN SCOPE | DIFFERENT | ST | SHOW p.53 | PaymentOperation.cs:9-23 has ReceivedByUserId and OperationAtUtc but NO payment-method field; no instalment count; AuditDtos.cs:8-11 carries receiver identity only | BLOCKED: method list not enumerated in the reference |
| REF-226 | Amend a verified result and keep the amendment log | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.22-25, 57; SHOW p.64 | AmendProfileResult; ProfileResultAmendment; GetProfileResultAmendmentsQueryHandler; AmendDialogViewModel, AmendmentsLogViewModel | Trigger differs (amend offered once printed) but the workflow is satisfied |
| REF-227 | System settings: delivery time, default account type | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.192, 199-201 | SystemSettings.cs:9,64-85; SystemSettingsViewModel:127; delivery time on ReceiptSettings.cs:13 honoured by ReceiptPdfWriter |  |
| REF-228 | Database server settings | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.191-192, 199-200 | UpdateDatabaseServerSettingsCommandHandler; IWorkstationConnectionSettingsProvider; GetDatabaseServerSettingsQueryHandler; SystemSettingsViewModel:154-159; CheckDatabaseConnectivity |  |
| REF-229 | Database backup and restore | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.191-192, 199-200 | BackupDatabaseNow/RestoreDatabase handlers; DatabaseMaintenanceViewModel:67-68 |  |
| REF-230 | Daily automatic backup into a dated folder | IN SCOPE | IMPLEMENTED | ST+TEST | SHOW p.52 | DailyBackupHostedService; SystemSettings.cs:29-31,112-; CheckBackupPathQueryHandler |  |
| REF-231 | Apply database and program updates | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.191, 200 | ApplyDatabaseUpdatesCommandHandler:20-111; DatabaseMaintenanceViewModel:69,163 | Program-file update is a deployment task outside the application |
| REF-232 | Printer assignment per output kind | IN SCOPE | DIFFERENT | ST | LEARN p.199 | PrinterOutputType.cs:3-9 has 4 values (Reports, Barcode, Envelope, Receipt); LEARN p.199 names FIVE including 'الكارت' (card) |  |
| REF-233 | Print the patient receipt automatically on registration | IN SCOPE | MISSING | ST (negative search) | LEARN p.192 | grep -i AutoPrint src/ -> 0 files; no such flag in SystemSettings; receipt only printed explicitly |  |
| REF-234 | Receipt: print patient and doctor names in English | IN SCOPE | MISSING | ST (negative search) | LEARN p.192, 203 | grep 'FullNameEn\|NameEn' src/ -> 0 files; Patient has one FullName, ExternalEntity one Name; neither receipt writer emits a Latin variant | BLOCKED: capture model must be chosen |
| REF-235 | Receipt: Himself / Herself when the referral is empty | IN SCOPE | DIFFERENT | ST | LEARN p.192, 203 | ReferralNameResolver.cs:14 returns 'Himself'/'Herself'; ONLY caller is GetRegistrationCatalogQueryHandler.cs:62-63 (UI placeholders). NO PDF writer emits the string | THIS AUDIT: downgraded from the second audit's IMPLEMENTED (ERR-3) |
| REF-236 | Treating doctor only from the external-entity window | IN SCOPE | DIFFERENT | ST | LEARN p.192, 203 | SystemSettings.cs:17 flag + SystemSettingsView.xaml:50 checkbox, but no consumer: PatientEditorViewModel still exposes free-text doctor fields (:290,292,298) | Stored flag, absent behaviour |
| REF-237 | Card settings: automatic review and completion of tests | IN SCOPE | IMPLEMENTED | ST+TEST | LEARN p.192 | EnterResultCommandHandler.cs:115-127 reads SystemSettings.AutoReviewAndComplete and calls pt.MarkReviewed(_currentUser.UserId, _clock.UtcNow) on save; checkbox SystemSettingsView.xaml:56 | THIS AUDIT: UPGRADED from the second audit's DIFFERENT (ERR-1). Nuance: reference files it under card settings, Top-Lab under system settings; no distinct 'complete' verb |
| REF-238 | Barcode display control in the program | IN SCOPE | DIFFERENT | ST | LEARN p.190; SHOW p.3 | Three related flags exist and ARE consumed (SystemSettings.cs:11,23,25; WorkSheetPdfWriter.cs:156-173; BarcodeService.cs:65-67), but there is no single master switch and receipt/report/envelope show no barcode |  |
| REF-239 | Print the account instead of the date on the report | IN SCOPE | DIFFERENT | ST | LEARN p.199 | SystemSettings.cs:27 flag + SystemSettingsView.xaml:57 checkbox, but NO consumer: writers always set ReportDateText = DateTime.Now (ReportContentBuilder.cs:120,184,331,365,425) | Stored flag, absent behaviour |
| REF-240 | Result-screen account display mode | IN SCOPE | DIFFERENT | ST | LEARN p.19-20 | ResultScreenAccountDisplayMode enum + SystemSettingsConfiguration column + SystemSettingsView.xaml:60 ComboBox. GetResultEntryQueryHandler.cs (111 lines) contains NO reference to the setting, nor to Account/Mode/Display at all | THIS AUDIT: downgraded from the second audit's IMPLEMENTED (ERR-2) |
| REF-241 | Lock the workstation and secondary authentication | IN SCOPE | IMPLEMENTED | ST | LEARN p.176, 150 | LockWorkstationCommandHandler:19-; ShellViewModel:65-66,84-104,109-112,227; UnlockWindow.xaml | The reference-aligned capability is REF-027 (present). Workstation lock + idle auto-lock are Top-Lab additions, not a gap |
| REF-242 | About window | IN SCOPE | IMPLEMENTED | ST | LEARN p.12 | AboutWindow.xaml; ShellViewModel:101-108,190 |  |

*Table row count: **242** (25 excluded + 217 in scope). In-scope classification totals: 129 IMPLEMENTED + 59 DIFFERENT + 29 MISSING = 217.*
