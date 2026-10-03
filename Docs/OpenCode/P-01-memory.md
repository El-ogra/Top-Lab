# Loop Engineering — Memory File

- **Module:** Parity Wave 1 — PP-01 patient-discovery filters, PP-02 worklist status filters, PP-03 lab reference-list printing
- **Module Number:** P-01
- **Source Plan:** `Docs/OpenCode/P-01.md`
- **Execution Prompt:** `Docs/OpenCode/P-01-Execution-Prompt.md`
- **Date Created:** 2026-10-03
- **Total Slices:** 6
- **Current Slice:** — (start at S1)
- **Current Branch:** `main`
- **Baseline Commit:** `9d042d06e56093e480d1c04375fd6b10d5caf23c`
- **Author:** loop-engineering (execution by the local coding agent per owner authorization)
- **Functions in scope:** **9** (F1 … F9)

---

## Module Summary

Parity Wave 1 brings **nine** reference-system functions to Top-Lab with **zero schema change, zero migrations and zero new packages**. The wave has three shapes:

- **PP-01 (F1–F5)** — six patient-search filter controls produced by five functions, because F1 yields **two** controls: treating doctor and referral entity. Every required column already exists on `Patient`; this is query-predicate and XAML work.
- **PP-02 (F6–F7)** — two worklist status checkboxes. F6 is real backend work (`IsPrinted` is projected but never filtered); **F7 is XAML only**, because the `IsReviewed` property and its dispatch already exist.
- **PP-03 (F8–F9)** — two printable lab reference lists, each needing a new Application-layer port, a new QuestPDF writer, a print command and a control.

**Wave 1 consumes nothing from any later wave.** The two new `I*` ports become the pattern later work follows, but nothing here waits on that.

**The safety spine of this wave is S1.** The search query is widened by six filters, so its guardrails are pinned by characterisation tests **before** the first filter is added. A regression in paging or the display cap must be distinguishable from one introduced by S2.

---

## Settled Decisions (binding — do not reopen)

- **SD-1 — the plan is a hypothesis.** Every `file:line` below is an unverified claim about the code as it was written. Verify at Stage 3 before editing. Code mismatch ⇒ **STOP and report**; do not silently adapt.
- **SD-2 — git.** One **local** commit per verified slice on `main`. **Never push.** No branch, amend, rebase, reset, stash, clean, tag, force-push. **Never `git add -A` / `git add .`** — stage explicit paths only. Never commit on a red build or failed gate. The owner pushes after the wave.
- **SD-3 — migration budget: ZERO.** `Migrations/` and `ApplicationDbContextModelSnapshot.cs` are read-only for this wave. A slice that appears to need a migration is a STOP condition.
- **SD-4 — package budget: ZERO.** `Directory.Packages.props` and every `.csproj` stay untouched. QuestPDF and ZXing.Net are already pinned and sufficient. Never write `Version=` in a `.csproj`.
- **SD-5 — search query guardrails (binding, all five clauses).** (1) Every new filter predicate stays inside its own guard clause; no new parameter is applied outside one. (2) No new parameter may replace, widen, or remove the existing `Text` guard. (3) Pagination is preserved exactly as it is today — no edit to `Page`, `PageSize`, the ordering, `Skip` or `Take`. (4) The display cap is preserved exactly as it is today. (5) Every new predicate narrows. Tests must prove each filter narrows and that the cap still holds.
- **SD-6 — six search filters, not five (binding).** `TreatingDoctorId` and `ReferralEntityId` are two different columns and two different concepts. A treating doctor is an individual physician; a referral entity is an institution that employs or hosts a physician who may sign the examination while not owning it. Both reference manuals are correct; both fields exist on `Patient`. **Two separate filter controls. Never collapse them into one control, one parameter, or one lookup.**
- **SD-7 — the cap is `PageSize`, not a literal 100.** The reference caps a page at 100 patients. **Top-Lab has no 100 cap** — its cap is `SearchPatientsGlobalQuery.PageSize`, default **50**. This wave changes nothing: do not raise it, lower it, introduce a 100 literal, or add a new cap clause.
- **SD-8 — `EnablePatientNameSearchAssist`.** Do not change its meaning, default, or gating. The name predicate stays gated exactly as it is today.
- **SD-9 — Arabic strings.** No new backend error message is introduced by this wave. New **UI-only** strings (filter labels, print column headers) are created once and appended to the register below. Never invent copyright text, a support address, or a commercial name.
- **SD-10 — layering.** `PresentationLayeringTests.PresentationLayering_NoInfrastructureReferenceOutsideAppXaml` (line 28) forbids any Presentation type referencing Infrastructure. PP-03 writers are reached through new `I*` ports in `Application/Common/Interfaces`, exactly as `IReportPrintingService` and `IWorkSheetPdfWriter` already are.
- **SD-11 — scope ceiling.** Nine functions, six slices, no migration, no schema change, no package, no permission code.

---

## Plan-vs-Code Corrections (binding — see P-01 §1)

| ID | Correction |
|---|---|
| **C-1** | Top-Lab has **no 100-patient cap**. The cap is `PageSize` (default 50). Preserve it; do not introduce 100. |
| **C-2** | **Six** search controls result from **five** search functions — F1 yields two columns. `Patient.TreatingDoctorId:29`, `Patient.ReferralEntityId:31`. |
| **C-3** | The results-not-verified filter is **not** missing: `ResultsWorklistViewModel.IsReviewed:64` exists and dispatches at `:161`. Only the XAML binding is absent (`ResultsWorklistView.xaml:11` binds `HasResult` alone; `IsReviewed` at `:80` is a **grid column**). F7 is XAML-only. |
| **C-4** | The results-not-printed filter is **not** a one-line binding. `GetResultWorklistQuery` (7–14) has no `IsPrinted`; `pt.IsPrinted` at `GetResultWorklistQueryHandler.cs:117` is a **DTO projection only**. |
| **C-5** | Price-list printing needs **two** artefacts, not one: no print command among `PriceListsViewModel.cs:76-82`, **and** no writer in `Infrastructure/Printing/`. |
| **C-6** | Custom-group printing has the same shape but different data (`CustomGroupDtos.cs` vs `PriceListDtos.cs`). Do not share a writer or DTO. |
| **C-7** | `MainWindow_Xaml_Bindings_ResolveToPublicProperties` (line 90) and `EveryView_IsRightToLeft` (line 168) gate every new binding. |
| **C-8** | Text search runs a **separate** `ToList()` of phone-matched ids *before* the main `Where` (handler 34–40), then ORs `phonePatientIds.Contains(p.Id)`. Preserve that structure. |

---

## Confirmed Code Facts (verified at the pinned commit — confirm at Stage 3, do not re-derive blindly)

**PP-01 — search:**
- `SearchPatientsGlobalQuery.cs:7-10` — `SearchPatientsGlobalQuery(string? Text, int Page = 1, int PageSize = 50)`. Four members. No filter parameters.
- `SearchPatientsGlobalQueryHandler.cs:28-29` — base query `.Where(p => !p.IsDeleted)`.
- Handler `:31-45` — one guard clause `if (!string.IsNullOrWhiteSpace(term))` wraps every current predicate. Name is gated on `nameAssist` (line 25).
- Handler `:34-40` — phone ids resolved by a separate `ToList()` **before** the patient `Where`.
- Handler `:47-50` — `OrderByDescending(RegistrationDateUtc)` then `Skip((Page-1)*PageSize).Take(PageSize)`.
- `Patient.cs` — `Sex:19`, `AgeValue:21`, `AgeUnit:23`, `TreatingDoctorId:29`, `ReferralEntityId:31`, `RegistrationDateUtc:37`. `AgeRules.cs` documents **no unit conversion**; bands compare like with like.
- `PatientSearchViewModel.cs:46` — `BranchFilterNoticeCommand`; it sets the honest branch notice. **Preserve.**
- `PatientSearchView.xaml` — current controls: `بحث:`, `كود المعمل:`, `جلب بالكود`, `الفرع`, `رجوع`, `السابق`, `التالي`. Grid columns include `الاسم`, `Lab ID`, `اللقب`, `الجنس`, `العمر`, `وحدة العمر`, `الرقم الوطني`, `نوع الحساب`, `VIP`, `الحالة`, `التاريخ`, `عدد التحاليل`, `فتح`.

**PP-02 — worklist:**
- `GetResultWorklistQuery.cs:7-14` — `Day`, `HasResult`, `IsReviewed`, `TestGroupId`, `ResultKind`, `Page`, `PageSize`. **No `IsPrinted`.**
- `GetResultWorklistQueryHandler.cs:117` — `pt.IsPrinted` is a DTO projection.
- `ResultsWorklistViewModel.cs:52` `HasResult` · `:64` `IsReviewed` · `:161` the query dispatch passing both.
- `ResultsWorklistView.xaml:11` — the single filter CheckBox bound to `HasResult`. `:80` — `IsReviewed` is a `DataGridTextColumn`, **not** a control.
- `ResultsWorklistView.xaml:5-13` — the filter `StackPanel`: `اليوم:` DatePicker, `لدي نتيجة:` CheckBox, `تحديث` button.

**PP-03 — printing:**
- `PriceListsViewModel.cs:76-82` — seven commands (`LoadLists`, `New`, `SaveList`, `DeleteList`, `AddItem`, `SaveItemPrice`, `RemoveItem`). **No print command.**
- `CustomGroupsViewModel.cs:76-82` — seven commands, parallel shape. **No print command.**
- `Infrastructure/Printing/` — `ReportPdfWriter`, `ReceiptPdfWriter`, `InvoicePdfWriter`, `WorkSheetPdfWriter`, plus `PdfPreviewService`, `PageSizeMapper`, `ArabicFontResolver`, `ShellPdfPrinterDispatcher`. **No price-list or group writer.**
- `WorkSheetPdfWriter` static ctor sets `Settings.License = LicenseType.Community` and `Settings.UseSystemFonts = true`; it resolves the font through `ArabicFontResolver.Resolve(labText.FontFamily)`. This is the pattern to imitate.
- `IWorkSheetPdfWriter.WritePdfAsync(absolutePath, VisitWorkSheetDto, LabPrintTextDto, ct)` — the port shape to mirror, in `Application/Common/Interfaces/`.
- `IReportPrintingService` is declared in Application so Presentation depends only on the abstraction (ADR-0005, Architecture §4.3).
- `LabPrintTextDto(LabName, Address, Phone, FontFamily, FontSizePt)`.
- `LabPrintTextStore` is the existing `ILabPrintTextStore` — reuse it for the new writers' header text.

**Structural gates that will fail this wave if ignored:**
- `PresentationStructuralTests.MainWindow_Xaml_Bindings_ResolveToPublicProperties` (line 90)
- `PresentationStructuralTests.EveryWindow_HasCreationSite` (line 117)
- `PresentationStructuralTests.EveryView_IsRightToLeft` (line 168)
- `PresentationLayeringTests.PresentationLayering_NoInfrastructureReferenceOutsideAppXaml` (line 28)
- `NeverConnectGuardTests` (persistence tests; N/A this wave — no DB work)

**Counting traps this wave contains:**
- `grep -rn "IsPrinted"` matches migration designers and the model snapshot as well as production code. Strip `Migrations/` before reasoning about blast radius.
- `SearchPatientsGlobalQuery` is a **positional record** — appended members must be defaulted and must never be inserted in the middle.
- The literal string `100` appears nowhere in `SearchPatientsGlobalQuery`. Do not "restore" a cap that was never there (C-1, SD-7).

---

## Global Validation Gates

- **G0 (once, before Slice 1):** measure the baseline on your own machine — build 0/0, each test project's pass/total, `git rev-parse HEAD`, `git status --porcelain`, `has-pending-model-changes`, and the migration file count. Record it in §Baseline below and report it.
- **G1 (every slice):** build 0/0; no test count below your measured baseline; the slice's `VG-nn` item by item; `git diff -- src/TopLab.Infrastructure/Persistence/Migrations/` empty; `git diff -- Directory.Packages.props` empty.

### Migrations policy (every slice)
- **Zero** new migrations. The folder is read-only.
- **Never** edit, delete or rename any existing migration or its `.Designer.cs`.
- **Never** touch `ApplicationDbContextModelSnapshot.cs` — not even via tooling.
- **Never** run `dotnet ef database update`.

---

## Quality Gate (a slice is complete ONLY when ALL hold)

1. Scope matches `P-01.md` (SD-1).
2. `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` → **0 errors, 0 warnings**.
3. No test count below the measured baseline.
4. The slice's `VG-nn` passes item by item, with evidence recorded here.
5. `git diff` over `Migrations/` **and** over `Directory.Packages.props` is empty.
6. Local commit only, message `[P-01] Slice N/6: <title> — loop-engineering` (SD-2).

---

## Stop/Continue Rule

- **Continue automatically** after a passing gate and a local commit (SD-2). The only normal stop is S6 plus the wave DoD.
- **STOP** on: plan/code mismatch (SD-1) · any need for a migration, schema change, `HasData` row, permission code or package (SD-3, SD-4) · collapsing the doctor and referral-entity filters (SD-6) · writing a predicate outside its guard clause or touching the paging chain (SD-5) · red build or tests not restorable within slice scope · the same failure **5 consecutive** times · about to launch the application or touch a migration · any ambiguity not settled in §0, reported as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية» without deciding it.
- Write the Stop Report in §Stop Report below and wait.

---

## Baseline (fill in G0 on the owner's Windows machine before Slice 1)

Measured by the executing agent on the owner's machine. **These are the gate numbers.**

| Item | Value |
|---|---|
| Date measured | 2026-10-03 |
| .NET SDK / VS MSBuild | .NET SDK **9.0.318** (`dotnet build`; .NET 8 SDK 8.0.425 also installed). VS MSBuild not used. |
| `git rev-parse HEAD` | `9d042d06e56093e480d1c04375fd6b10d5caf23c` ✓ (exact match, not a descendant) |
| `git status --porcelain` | only `?? Docs/OpenCode/P-01-Execution-Prompt.md`, `?? Docs/OpenCode/P-01-memory.md`, `?? Docs/OpenCode/P-01.md` — otherwise clean ✓ |
| Build warnings / errors | **0 warnings / 0 errors** (`dotnet build TopLab.sln -p:EnableWindowsTargeting=true`) |
| TopLab.Domain.Tests | **507 / 507 passed, 0 failed, 0 skipped** |
| TopLab.Application.Tests | **1589 / 1589 passed, 0 failed, 0 skipped** |
| TopLab.Infrastructure.Tests | **265 / 265 passed, 0 failed, 0 skipped** |
| TopLab.Presentation.Tests | **67 / 67 passed, 0 failed, 0 skipped** |
| TopLab.Persistence.Tests | **13 passed, 0 failed, 2 skipped, 15 total** — the 2 skipped are reported as skipped, never as passes |
| **Full suite** | **2441 passed, 0 failed, 2 skipped, 2443 total** |
| `has-pending-model-changes` | `No changes have been made to the model since the last migration.` (run as `dotnet ef migrations has-pending-model-changes --project src/TopLab.Infrastructure --startup-project src/TopLab.Presentation` — EF rejects `-p:` on `dotnet ef`, so the flag cannot be passed to the tooling directly; the build of both projects succeeded first) |
| Migration files | **14 migrations + 14 Designer.cs + `ApplicationDbContextModelSnapshot.cs` = 29 files** in `src/TopLab.Infrastructure/Persistence/Migrations/` |
| `Directory.Packages.props` unchanged | yes — `git diff -- Directory.Packages.props` empty ✓ |

**Baseline note for S2:** `SearchPatientsGlobalQueryValidator.cs` **exists** (`PageSize` rule is `InclusiveBetween(1, 500)`). The plan's S2 scope listed it as "if it exists — verify, C-1 pattern"; it does, and it is verified against the live code. It is left unchanged in this wave — no new validation rule is needed for optional filter parameters, and SD-9 forbids new backend messages.

---

## Stage 3 — File Analysis confirmations (S1)

Every `file:line` cited in P-01 §3 and §4 was opened and confirmed at the pinned commit:

| Claim | Confirmed? | Evidence |
|---|---|---|
| `SearchPatientsGlobalQuery.cs:7-10` — `SearchPatientsGlobalQuery(string? Text, int Page = 1, int PageSize = 50)` | ✅ | File is 9 lines; the record declaration is lines 7-9 with exactly `Text`, `Page = 1`, `PageSize = 50`. **Three members, no filter parameter.** |
| Handler `:28-29` — base query `.Where(p => !p.IsDeleted)` | ✅ | `.Where(p => !p.IsDeleted);` on line 28, `IQueryable<Patient> query = _db.Set<Patient>()` on line 27. |
| Handler `:31-45` — single guard `if (!string.IsNullOrWhiteSpace(term))` | ✅ | Guard opens line 30, closes line 45. Phone `ToList()` on lines 34-38, patient `Where` on 40-44. |
| Handler `:47-50` — ordering then `Skip`/`Take` | ✅ | `OrderByDescending(p => p.RegistrationDateUtc)` line 48, `.Skip((request.Page - 1) * request.PageSize)` line 49, `.Take(request.PageSize)` line 50. |
| `Patient.cs` `Sex:19`, `AgeValue:21`, `AgeUnit:23`, `TreatingDoctorId:29`, `ReferralEntityId:31`, `RegistrationDateUtc:37` | ✅ | All six line numbers are exact in `src/TopLab.Domain/Patients/Patient.cs`. `TreatingDoctorId` and `ReferralEntityId` are two distinct nullable `ExternalEntityId?` columns — SD-6 is confirmed live. |
| `nameAssist` gate on line 25 | ✅ | `_db.Set<SystemSettings>().SingleOrDefault()?.EnablePatientNameSearchAssist ?? false` — line 25, used at line 41. SD-8 confirmed. |
| No 100 literal in the query | ✅ | The whole file is 9 lines and contains only `1` and `50`. C-1 confirmed. |
| Test context is LINQ-to-objects | ✅ | `FakeApplicationDbContext.Set<T>()` returns `Patients.AsQueryable()` etc. So `Skip`/`Take`/`Any` all execute in memory — characterisation tests are valid and assertions on narrowing are meaningful. |

**No plan/code contradiction found.** Execution continues.

**Measure it yourself. The executing agent's own numbers are the gate, not anyone else's.**

---

## Slice Validation Gates (from plan)

| Slice | Gate | Key checks |
|---|---|---|
| 1 | VG-01 | Guardrail characterisation tests green; **`git diff` over `src/` empty** — no production change |
| 2 | VG-02 | Six filters narrow; doctor ≠ referral entity; all S1 guardrails still green; paging and cap untouched |
| 3 | VG-03 | **Six** distinct controls bound; doctor/referral distinct; branch notice preserved; binding + RTL gates green |
| 4 | VG-04 | `IsPrinted=false` selects unprinted (negative case); `IsReviewed` control added without touching the `:80` column |
| 5 | VG-05 | Price-list PDF is real output; layering gate green; no package change |
| 6 | VG-06 | Group PDF is real output; **zero migrations**; wave DoD |

---

## Slice Index

| # | Slice Title | Status | Gate |
|---|---|---|---|
| 1 | Search guardrail net (no production change) | ✅ `8ff279b` | VG-01 PASS |
| 2 | Six patient-search filters in query + handler | ✅ `a968364` | VG-02 PASS |
| 3 | Search screen — the six filter controls | ✅ `e26b8e9` | VG-03 PASS |
| 4 | Worklist `IsPrinted` predicate + both status checkboxes | ✅ `88b91c9` | VG-04 PASS |
| 5 | Price-list print — port, writer, command | ✅ `76f3631` | VG-05 PASS |
| 6 | Custom-group list print + wave DoD | ✅ `f89968e` | VG-06 PASS |

---

## Per-slice 10-stage checklists

### Slice 1 — Search guardrail net (no production change)
**Gate:** VG-01. **Status:** ✅ COMPLETE — commit `8ff279b`
- [x] 1. Pre-Execution Verification — build 0/0; tests ≥ measured baseline; HEAD pinned; tree clean apart from the three P-01 package files
- [x] 2. Deep Understanding — P-01 §3; C-1, C-8
- [x] 3. File Analysis — `SearchPatientsGlobalQuery.cs` and the handler opened end to end; **all cited lines confirmed exact** — see the Stage 3 table above
- [x] 4. Planning — recorded below (Stage 4 note)
- [x] 5. Execution — characterisation tests only; **no production file touched**
- [x] 6. Post-Execution Verification — build 0/0; suite green; `git diff --stat` over `src/` **empty**
- [x] 7. Validation Gate — VG-01 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-01] Slice 1/6: …`

#### Stage 4 — S1 planning note

**One file added, zero production files touched:**
`tests/TopLab.Application.Tests/Features/PatientSearch/SearchGuardrailTests.cs` — 19 tests in 6 guardrail groups.

Fixture idiom copied from the sibling `SearchPatientsGlobalQueryHandlerTests.cs`: hand-rolled `FakeApplicationDbContext` (no mocking library), `Patient.Create` via the domain factory, and the same `WithNameAssist(bool)` settings helper. No new fakes were needed.

| Group | Tests | What it pins |
|---|---|---|
| 1 — query shape | 3 | The record has exactly `Text`, `Page`, `PageSize`; positional order `Text, Page, PageSize`; `PageSize` default is **50** and `Page` default is **1**; no member default is 100; no property name contains "Cap". **This is the VG-01 "parameter list unchanged" gate — it is written so that S2's six added members make it fail loudly rather than pass silently.** |
| 2 — the display cap | 2 | 120 live patients with the default query return **exactly 50** rows, and `50 < 120` so the cap demonstrably bounds; a supplied `PageSize` of 5 and 7 are honoured exactly against 40 rows. **No test asserts the number 100** (C-1, SD-7). |
| 3 — paging | 3 | 25 patients with `PageSize` 10 give page 1 = 10 rows, page 3 = 5 rows, and the two pages **do not intersect**; a page beyond the end returns empty rather than wrapping; the default query and an explicit `(1, 50)` return the identical id sequence, proving page 1 has no offset. |
| 4 — ordering | 2 | Three out-of-order registrations come back `2, 3, 1` on a `RegistrationDateUtc` descending order; 20 patients over 3 pages of 7 concatenate to 20 distinct ids in full descending order — **no row lost or duplicated**. |
| 5 — soft delete | 2 | A deleted patient is excluded with no text supplied; and with the **60 newest rows deleted and 100 live rows behind them**, the default query returns **50 live rows and no deleted id** — proving the cap is applied *after* `!p.IsDeleted`, not before. |
| 6 — text paths | 7 | Name gated by `EnablePatientNameSearchAssist` both on and off, plus the **default-off when no settings row exists** (SD-8 pinned); `LabId` and `NationalId` are exact-trimmed-match **and their partial forms return empty** (narrowing, not broadening); a phone term narrows to its one owning patient out of three (C-8); any stored number of a patient matches; null / empty / whitespace text all return the full set. |

**Design decision recorded:** the S1 cap tests use **120 and 160-row fixtures**, deliberately larger than both the default cap (50) and the reference system's 100, so that a cap which silently grew to 100 or became unbounded would fail the assertion. A fixture of ≤50 rows would satisfy the same test with no cap at all, which is precisely the failure mode guardrail clause 5 forbids.

**Second design decision recorded:** every narrowing assertion pairs a positive case with a **negative** case (partial term returns empty; a second patient excluded; deleted rows never returned). A test that only asserts the matching row can be satisfied by a predicate that returns everything.

---

## VG-01 — evidence (Slice 1)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` → `Build succeeded. 0 Warning(s) 0 Error(s)` |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1608/1608** (Δ**+19**) · Infrastructure **265/265** (Δ0) · Presentation **67/67** (Δ0) · Persistence **13 passed / 2 skipped / 15** (Δ0). Full suite **2460 passed, 0 failed, 2 skipped, 2462 total** (Δ**+19**). |
| 3 | `SearchGuardrail_*` all green | ✅ | `Failed: 0, Passed: 19, Skipped: 0, Total: 19` |
| 4 | `git diff --stat` over `src/` **empty** | ✅ | empty — the slice added one test file and nothing else |
| 5 | Migrations folder unchanged | ✅ | `git diff --stat -- src/TopLab.Infrastructure/Persistence/Migrations/` → empty |
| 6 | Query parameter list unchanged | ✅ | Asserted by 3 shape tests in group 1, not by inspection |
| 7 | `Directory.Packages.props` unchanged | ✅ | `git diff --stat -- Directory.Packages.props` → empty |
| 8 | No `.csproj` touched | ✅ | `git status --porcelain` shows no `.csproj` modification |

---

## Pre-existing defect observed at S1 (NOT introduced by this wave — owner decision needed)

**`TopLab.Infrastructure.Tests.Services.PatientReportPdfExporterTests.Export_Creates_ValidPdf` is an order-dependent flake.** Observed once at the VG-01 full-suite run and then 6 times out of 6 when run under `--filter`.

- **Symptom:** `System.Exception : Please configure the QuestPDF license by setting 'QuestPDF.Settings.License' at application startup.` (QuestPDF `LicenseChecker`).
- **Mechanism (verified in source):** QuestPDF's `Settings.License` is **process-global static state**. Four writers set it in their **static constructors** — `InvoicePdfWriter.cs:23`, `ReceiptPdfWriter.cs:25`, `ReportPdfWriter.cs:20`, `WorkSheetPdfWriter.cs:24`. `PatientReportPdfExporter` (`src/TopLab.Infrastructure/Services/PatientReportPdfExporter.cs:14`) has **no static constructor** and does not set the license itself. It therefore only passes when some *other* writer's static constructor happened to run first in the same process. Under a filter that excludes those writers, or under an unlucky xUnit collection/parallelism ordering, it fails.
- **It is not caused by P-01.** `TopLab.Infrastructure.Tests.csproj` references only `src/TopLab.Infrastructure`, `src/TopLab.Application` and `src/TopLab.Domain` — **it has no `ProjectReference` to `TopLab.Application.Tests`**, so the S1 test file is not in its compilation or execution graph. S1 touched no production file and no Infrastructure test file.
- **Why it does not stop the wave:** the Infrastructure **count is unchanged at 265**; the failure is a pre-existing latent ordering bug in production code, not a regression, and it is **outside P-01's scope and DoD** (F8/F9 are two *new* writers; this is an existing exporter). The wave's gate is that no count may fall below the measured baseline, and none did.
- **Deliberately NOT fixed in this wave** — fixing it means adding a static constructor to `PatientReportPdfExporter`, a production file outside every slice's scope, and it would break VG-01's "`git diff` over `src/` empty". Recorded for the owner as: **«بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»**.
- **Forward consequence for S5/S6:** the two new writers **must** set `Settings.License = LicenseType.Community` and `Settings.UseSystemFonts = true` in their own **static constructors**, exactly as `WorkSheetPdfWriter` does. P-01 §7 already specifies this. A new writer that merely *relies* on another writer's static constructor would inherit this same flake — the new writer tests must therefore be run under `--filter` as well as in the full suite, and must not be accepted as "passing" on a full-suite run alone.

### Slice 2 — Six search filters
**Gate:** VG-02. **Status:** ✅ COMPLETE — commit `see Execution Log`

**Stage 3 — file analysis confirmations (all cited lines verified before editing):**

| Claim | Confirmed? | Evidence |
|---|---|---|
| `SearchPatientsGlobalQuery.cs:7-10`, three members | ✅ | `SearchPatientsGlobalQuery(string? Text, int Page = 1, int PageSize = 50)`, lines 7-9. |
| Handler `:28-29` base query | ✅ | unchanged in this slice |
| Handler `:31-45` single term guard | ✅ | unchanged; the six filters were **inserted after line 45**, i.e. after the guard closes |
| Handler `:47-50` ordering + Skip/Take | ✅ | unchanged — see the mechanical proof below |
| `SearchPatientsGlobalQueryValidator.cs` exists | ✅ | 17 lines; `PageSize` rule `InclusiveBetween(1, 500)`. **Left untouched** — optional filter parameters need no new rule and SD-9 forbids a new backend message. |
| `Patient.cs` six column line numbers | ✅ | all exact (see S1 table) |
| `AgeRules` documents no unit conversion | ✅ | **Path corrected:** it is `src/TopLab.Domain/Common/AgeRules.cs`, **not** `src/TopLab.Domain/Patients/AgeRules.cs` as P-01's Confirmed Code Facts implies. The *substance* is correct and stronger than the plan states: the file header explicitly cites **BR-04 ("no conversion between age units")** and states it is **NOT a reference-range matcher**. Its `ToWholeYears` is documented as belonging to a *different* rule. So "bands compare like with like" is the documented Domain rule, and using `ToWholeYears` here would have been the defect. |
| `PatientTest` key idiom — "verify the exact idiom in the codebase before writing it" | ✅ | `PatientTest.PatientId` and `.TestId` are both `StronglyTypedId<int>` subclasses (`PatientTest.cs:10,12`), mapped with `HasConversion(v => v.Value, v => …Create(v))` in `PatientTestConfiguration.cs:14-15`. Existing handler code uses `.Value` (`GetResultWorklistQueryHandler.cs:62`). The filter therefore compares on `.Value`, which is translatable to SQL. |

**`AgeValueBand` did not exist.** `grep -rn "AgeValueBand" src/ tests/` → **zero hits**. P-01 §4 step 1 names it as part of the target signature and **AS-5 settles its shape** (unit + from/to, no conversion), so creating it is executing the plan, not deciding an open question. Created as `public sealed record AgeValueBand(AgeUnit Unit, int? From = null, int? To = null)` in the same file as the query, in `TopLab.Application` (not Domain — it is a query input, and Domain is read-only under SD-11's "no Domain entity changes"). **No migration:** it is not an entity and is never persisted.

- [x] 1. Pre-Execution Verification — S1's commit `8ff279b` confirmed present in `git log` **before** starting (required by the slice warning); build 0/0; HEAD valid; tree clean apart from package files
- [x] 2. Deep Understanding — P-01 §4; C-1, C-2, C-8
- [x] 3. File Analysis — query, handler, validator, `Patient`, `AgeRules`, `PatientTest`, `StronglyTypedId` and both EF configurations opened and confirmed
- [x] 4. Planning — recorded below
- [x] 5. Execution — six defaulted members appended at the tail; **six independent guard clauses**; `AgeUnit`-aware band; doctor ≠ referral entity; every predicate narrows; paging, ordering, cap and the `Text` clause untouched
- [x] 6. Post-Execution Verification — build 0/0; suite green
- [x] 7. Validation Gate — VG-02 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-01] Slice 2/6: …`

#### Stage 4 — S2 planning note

**Record discipline.** Seven members appended at the **tail**, all defaulted, none inserted between `Text`, `Page` and `PageSize`. `AgeValueBand?` is a **reference type**, so the existing `new SearchPatientsGlobalQuery(null, 1, 3)` call sites are unaffected — no signature break. `Sex?` is `Nullable<Sex>` and the parameter `Sex` deliberately shadows the enum type name inside the record declaration only; the handler uses `Sex.Male`-style access via `p.Sex`, so no ambiguity arises in production code.

**Guard shapes — all six, each independently guarded and inert when null:**

| Fn | Guard | Predicate | Narrows because |
|---|---|---|---|
| F1a | `if (request.TreatingDoctorId is not null)` | `p.TreatingDoctorId != null && p.TreatingDoctorId.Value == id` | conjunct |
| F1b | `if (request.ReferralEntityId is not null)` | `p.ReferralEntityId != null && p.ReferralEntityId.Value == id` | conjunct, **separate column** |
| F2 | `if (request.TestId is not null)` | `_db.Set<PatientTest>().Any(pt => pt.PatientId.Value == p.Id.Value && pt.TestId.Value == testId)` | `Any` — an existential subquery can only exclude |
| F3 | `if (request.Sex is not null)` | `p.Sex == sex` | equality |
| F4 | `if (request.Age is not null)` | `p.AgeUnit == age.Unit && (from == null \|\| p.AgeValue >= from) && (to == null \|\| p.AgeValue <= to)` | unit equality **always** applies, so a supplied band can never match another unit |
| F5 | `if (request.From is not null)` / `if (request.To is not null)` — **two** guards | `>= fromUtc` / `<= toUtc` where the day bound is widened to `TimeOnly.MinValue` / `TimeOnly.MaxValue` | inclusive on both ends |

**Seven `if` clauses for six filters** is deliberate: F5 is one function producing a `From`/`To` pair, and each bound must be independently inert so an open-ended range works (covered by `Search_FilterByDateRange_OpenEndedBoundsWorkIndependently`).

**Decision recorded — the F5 bound conversion.** `DateOnly` has no time component, so a naive `>= new DateTime(y,m,d)` would silently drop every patient registered later on the `From` day. The bounds are widened to the start and end of the day. This is a faithful reading of "restrict to a period", not a new rule.

**Decision recorded — no `||` anywhere in a filter.** Guardrail clause 4 names a disjunction on a patient column as a defect. F4's predicate contains `||` **only** between `age.From == null` and the bound comparison — a null-check on the *parameter*, never on a patient column, and it cannot widen a supplied band. Recorded explicitly because it is the one place the pattern could be misread.

**Mechanical proof that paging and the cap were not touched:**
```
git diff HEAD -- …/SearchPatientsGlobalQueryHandler.cs | grep -E "^[-+].*(Skip|Take|OrderBy|phonePatientIds|nameAssist|IsDeleted|lowerTerm|NationalId|LabId)"
→ (no output)
```
Zero changed lines match any of those tokens, so the `Text` guard, the phone pre-query, `!p.IsDeleted`, the ordering and the `Skip`/`Take` chain are byte-identical. The handler diff is **pure addition** — the six guards sit between line 45 (end of the term guard) and the unchanged paging block.

**17 new tests** in `SearchPatientsGlobalFilterTests.cs`, each with a multi-row fixture and an exact-id assertion, so removing a predicate fails the test:

| VG-02 requirement | Test(s) |
|---|---|
| doctor narrows (2 patients / 1 patient / none) | `Search_FilterByTreatingDoctor_Narrows` |
| referral narrows | `Search_FilterByReferralEntity_Narrows` |
| **doctor ≠ referral** | `Search_DoctorAndReferralAreDistinct_SameValueDifferentResults` — the **same id 7** as doctor gives `{1,2}`, as referral gives `{3,4}`; and doctor=7 **AND** referral=8 gives `{1}` where an OR would give all four. Plus `Search_DoctorAndReferralAreSeparateQueryParameters` (reflection: both properties exist and are distinct). |
| test narrows, unknown test → empty | `Search_FilterByTest_Narrows_AndUnknownTestReturnsEmpty` |
| gender narrows | `Search_FilterByGender_Narrows` |
| age like-with-like | `Search_FilterByAge_ComparesLikeWithLike_DayUnitDoesNotMatchYearValues` — a 400-**Day** and a 400-**Year** patient; a `Day 0..500` band returns only the Day row, the same numeric `Year 0..500` band returns only the Year rows. A unit-blind implementation fails both. |
| age bounds inclusive | `Search_FilterByAge_BoundsAreInclusive` |
| date range incl. both boundaries | `Search_FilterByDateRange_Narrows_IncludingBothBoundaries` (rows at 23:59:59 the day before, 00:00:00 on `From`, mid, 23:59:59 on `To`, 00:00:00 the day after) |
| open-ended date bounds | `Search_FilterByDateRange_OpenEndedBoundsWorkIndependently` |
| filters AND-combine | `Search_FiltersCombineWithAnd` — four filters, five rows, exactly one survives; each wrong row is excluded by a different filter |
| filter with empty text | `Search_FilterWithEmptyText_StillFilters` — null, `""` and `"   "` all filter identically |
| null parameters inert | `Search_NullFilterParameters_ApplyNoFilterAtAll` |
| filter + text AND | `Search_FilterAndTextCombineWithAnd` |
| soft delete still excluded under filters | `Search_SoftDeletedRowsStayExcludedWhenFiltersAreApplied` |
| cap survives filtering | `Search_FilterStillHonoursPageSizeCap` — 120 male + 120 female rows, gender filter returns **exactly 50** male rows |

---

## VG-02 — evidence (Slice 2)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)` |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1625/1625** (Δ**+17**) · Infrastructure **265/265** (Δ0) · Presentation **67/67** (Δ0) · Persistence **13 / 2 skipped / 15** (Δ0). Full suite **2477 passed, 0 failed, 2 skipped, 2479 total** (Δ**+17** vs S1) |
| 3 | `Search_FilterByTreatingDoctor_Narrows` | ✅ | `{1,2}` for the 2-patient doctor, `{3}` for the 1-patient doctor, `{}` for an unknown doctor |
| 4 | `Search_FilterByReferralEntity_Narrows` | ✅ | `{1}` and `{2}`; unknown → `{}` |
| 5 | **`Search_DoctorAndReferralAreDistinct`** | ✅ | id 7 as doctor → `{1,2}`; id 7 as referral → `{3,4}`; **different sets**, asserted with `NotEqual` |
| 6 | `Search_FilterByTest_Narrows` | ✅ | test 100 → `{1,2}`, test 101 → `{1}`, **test 999 → empty, not everything** |
| 7 | `Search_FilterByGender_Narrows` | ✅ | Male `{1,3}`, Female `{2,4}` |
| 8 | `Search_FilterByAge_Narrows_ComparesLikeWithLike` | ✅ | a `Day` band never matches a `Year` value; the same numeric band under each unit returns disjoint sets |
| 9 | `Search_FilterByDateRange_Narrows` incl. both boundaries | ✅ | `{2,3,4}` — the `From`-day 00:00:00 row and the `To`-day 23:59:59 row are both inside |
| 10 | `Search_FiltersCombineWithAnd` | ✅ | exactly one row of five survives four filters |
| 11 | `Search_FilterWithEmptyText_StillFilters` | ✅ | null / `""` / `"   "` all yield `{1}` |
| 12 | **All S1 guardrail tests still green** | ✅ | all 19 pass unchanged in substance; the two shape tests were **re-aimed** (see below) and still pass |
| 13 | `git diff` over `Migrations/` empty | ✅ | empty |
| 14 | `Directory.Packages.props` unchanged | ✅ | empty; **no `.csproj` touched either** |
| 15 | Paging/ordering/cap untouched | ✅ | the `git diff | grep` above returns no matches on any paging/ordering/phone/`IsDeleted` token |
| 16 | No new backend error message | ✅ | SD-9 respected — the validator was not given a new rule; only comments and code were added |

**Two S1 tests were re-aimed, deliberately and with the reason recorded.** `SearchGuardrail_Query_HasNoFilterParameter_BeforeSlice2` and `SearchGuardrail_Query_PositionalShapeIsTextPagePageSize_InThatOrder` both asserted the *complete* parameter list and so failed the moment S2 appended a member — which is exactly what P-01 §3 step 2 asked for ("so that S2's addition is visibly a change"). They were **not deleted and not weakened**: the durable property behind them is SD-5 clause 2 (the existing positional surface must survive), and they now assert that property directly — the first three members are still `Text, Page, PageSize`, **in that order, at the head**, and `PageSize` is still index 2 with default 50. Inserting a filter member in the middle, renaming `Page`, or moving `PageSize` still fails them. The one-shot "no filter exists yet" observation is preserved here in the Execution Log rather than as a live assertion.

### Slice 3 — Six filter controls
**Gate:** VG-03. **Status:** ✅ COMPLETE — commit `see Execution Log`

**Stage 3 — file analysis confirmations:**

| Claim | Confirmed? | Evidence |
|---|---|---|
| `PatientSearchViewModel.cs:46` `BranchFilterNoticeCommand` | ✅ | It is at **line 46** in the constructor and sets `ErrorMessage = "البحث بفرع غير متاح دون مبيعات موزّعة على الفروع."` — **preserved byte-for-byte**; `BranchFilterNoticeCommand_Preserved` asserts the exact Arabic string. |
| Single `SearchAsync` call site at `:138-139` | ✅ | `new SearchPatientsGlobalQuery(text, Page, PageSize)` — one call site, now forwarding all six filters. |
| `PatientSearchView.xaml` current controls | ✅ | `بحث:`, `كود المعمل:`, `جلب بالكود`, `الفرع`, `رجوع`, `السابق`, `التالي` and all 13 grid columns — asserted still present by `PatientSearch_PreExistingControlsAndColumns_Survive`. |
| "labelled lookups only, if required" | ✅ | **Required.** S2's filters take `ExternalEntityId` and `int` ids, which no human can type. The codebase already had a lookup idiom: `SentOutSamplesViewModel.cs:121-139` loads `SearchExternalEntitiesQuery(EntityType.PartnerLab, null, 1, 100)` into a ComboBox, and `PriceListsViewModel.cs:145` loads `SearchTestCatalogQuery(null, null, false)`. **Both idioms were followed exactly** — no new query was created. |
| **SD-6 corroborated by live code** | ✅ **new** | `EntityType` (`src/TopLab.Domain/Common/Enums/EntityType.cs`) is `TreatingDoctor = 0, ReferralOrContract = 1, PartnerLab = 2`. **The Domain enum itself already models the two concepts as distinct**, so the two lookups are issued with two different `EntityType` values and cannot mix. |

- [x] 1. Pre-Execution Verification — HEAD = S2 commit `a968364`; tree clean; build 0/0
- [x] 2. Deep Understanding — P-01 §5; C-2, C-7, SD-6, SD-11
- [x] 3. File Analysis — ViewModel (209 lines) and view (89 lines) read end to end; `SentOutSamplesViewModel` and `PriceListsViewModel` read for the lookup idiom; both structural gates read
- [x] 4. Planning — recorded below
- [x] 5. Execution — six public bindable properties; six controls; Arabic labels registered; `BranchFilterNoticeCommand` preserved; RTL and binding gates green
- [x] 6. Post-Execution Verification — build 0/0; suite green
- [x] 7. Validation Gate — VG-03 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist + UI Texts Register
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-01] Slice 3/6: …`

#### Stage 4 — S3 planning note

**Six controls, one per filter, each bound to its own public property:**

| Fn | Control | Bound property | Options source |
|---|---|---|---|
| F1a | ComboBox «الطبيب المعالج» | `SelectedTreatingDoctor` | `TreatingDoctorOptions` ← `SearchExternalEntitiesQuery(EntityType.TreatingDoctor, …)` |
| F1b | ComboBox «جهة الإحالة» | `SelectedReferralEntity` | `ReferralEntityOptions` ← `SearchExternalEntitiesQuery(EntityType.ReferralOrContract, …)` |
| F2 | ComboBox «التحليل» | `SelectedTest` | `TestOptions` ← `SearchTestCatalogQuery(null, null, false)` |
| F3 | ComboBox «الجنس» | `SelectedSex` | `SexOptions` (ذكر / أنثى / الكل) |
| F4 | 2 TextBoxes + unit ComboBox «العمر» | `AgeFrom`, `AgeTo`, `SelectedAgeUnit` | `AgeUnitOptions` (يوم / شهر / سنة) |
| F5 | 2 DatePickers | `FromDate`, `ToDate` | — |

**Two design decisions recorded.**

1. **The age band is only sent when a bound exists.** `AgeFrom` and `AgeTo` both null ⇒ `Age` is sent as **null**, not as an empty band. An empty band would still carry `AgeUnit == Year`, which would exclude every patient recorded in days or months — a silent *narrowing* that looks like a working filter. Sending null keeps the unit ComboBox inert until the user actually enters a bound. Pinned by `PatientSearch_EmptyAgeBandIsNotSent`.
2. **A "الكل" sentinel with a null id heads each lookup**, following `SentOutSamplesViewModel`'s `new(null, "الكل")` idiom. A null id forwards no filter, so the sentinel is inert (SD-5) rather than a magic "all" value the query would have to special-case.

**A third decision — `ClearFiltersCommand`.** P-01 §5 step 6 requires clearing a filter to widen back to the unfiltered-by-that-filter state. A plain reset would fire **nine** reloads (one per setter). A `_suspendFilterReload` flag suppresses the setters' reloads and exactly one runs at the end. This is why the flag exists and it is the only mutable state added to this ViewModel.

**Change notification** follows the existing `Page` idiom: each setter calls `SetProperty`, and on change resets `Page = 1` then reloads. Pinned by `PatientSearch_FilterChangeResetsToFirstPage`.

**13 new tests.** The six-control and merge-detection tests read the **real XAML file** and the **real ViewModel type** by reflection, so they cannot pass against a stub:

- `PatientSearch_HasSixFilterControls_EachBoundToItsOwnProperty` — asserts a **six**-entry table (not five, C-2); each row's label, bound property and options source must all appear in the XAML.
- `PatientSearch_DoctorAndReferralControlsAreDistinct_FailsIfEverMerged` — the required merge test. Five independent assertions: two labels, two `SelectedItem` bindings, two `ItemsSource` bindings, two public properties, and **exactly one occurrence of each label** (a merged or duplicated control shows up as a count mismatch).
- `PatientSearch_DoctorAndReferralAreLoadedFromSeparateLookups` — a `RecordingSender` captures the `EntityType` of every `SearchExternalEntitiesQuery`; asserts exactly 2 lookups with **different** types, and that the two populated lists share **no** id.
- `PatientSearch_SettingDoctorDoesNotSetReferral_AndViceVersa` — setting one leaves the other null, and setting the referral entity does **not** overwrite the doctor.
- Plus: all bindings public (C-7), each filter forwarded, all-null inert, empty band not sent, clear widens (asserting `Page == 1` and `PageSize == 50` afterwards), filter resets to page 1, branch notice preserved with the exact Arabic string, RTL preserved, pre-existing controls and columns survive.

---

## VG-03 — evidence (Slice 3)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)` — four warnings were introduced and all four eliminated (duplicate `using`, two CS8625, xUnit1031) |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1625/1625** (Δ0) · Infrastructure **265/265** (Δ0) · Presentation **80/80** (Δ**+13**) · Persistence **13 / 2 skipped** (Δ0). Full suite **2490 passed, 0 failed, 2 skipped, 2492 total** (Δ**+13**) |
| 3 | `MainWindow_Xaml_Bindings_ResolveToPublicProperties` | ✅ | green (Presentation 80/80 includes `PresentationStructuralTests`) |
| 4 | `EveryView_IsRightToLeft` | ✅ | green; also asserted directly by `PatientSearch_ViewStaysRightToLeft` |
| 5 | **Six** distinct controls, separately bound | ✅ | `Assert.Equal(6, controls.Length)` over a six-row table; each row's label + property + options source present in the XAML |
| 6 | **`PatientSearch_DoctorAndReferralControlsAreDistinct`** | ✅ | five independent assertions incl. exactly-one-occurrence of each label |
| 7 | `BranchFilterNoticeCommand` present and unchanged | ✅ | command still at line 46 of the constructor; message asserted string-equal |
| 8 | All S1 + S2 tests still green | ✅ | Application **1625/1625**, unchanged — the guardrail and filter suites were untouched by this slice |
| 9 | `git diff` over `Migrations/` empty | ✅ | empty |
| 10 | No package / `.csproj` change | ✅ | `git diff --stat -- Directory.Packages.props '*.csproj'` → empty |
| 11 | No new backend error message | ✅ | SD-9 respected — UI-only labels only |

### Slice 4 — Worklist filters
**Gate:** VG-04. **Status:** ✅ COMPLETE — commit `see Execution Log`

**Stage 3 — file analysis confirmations (C-3 and C-4 both verified before editing):**

| Claim | Confirmed? | Evidence |
|---|---|---|
| **C-3** `ResultsWorklistViewModel.IsReviewed:64` exists and dispatches at `:161` | ✅ **exactly** | `IsReviewed` is at **line 64-74** (property) and the dispatch is at **line 161-162**: `new GetResultWorklistQuery(Day, HasResult, IsReviewed, TestGroupId, ResultKind, Page, PageSize)`. **F7 needed no backend change at all** — confirmed by reading, not assumed. |
| **C-4** `GetResultWorklistQuery` has **no** `IsPrinted` | ✅ **exactly** | The record had exactly `Day, HasResult, IsReviewed, TestGroupId, ResultKind, Page, PageSize`. No `IsPrinted`. |
| **C-4** `pt.IsPrinted` at handler `:117` is a **projection only** | ✅ | Line 117 is inside the DTO construction lambda. **No `Where` clause anywhere referenced `IsPrinted`** — confirmed by reading the full handler. F6 was therefore genuine backend work, not a binding. |
| `GetResultWorklistQueryHandler` guard idiom | ✅ **new, and decisive** | The four existing filters all use `if (request.X.HasValue) { rows = rows.Where(...).ToList(); }` (lines 47-72). The `IsPrinted` filter follows **that exact shape** — this is what makes `false` ≠ absent structurally rather than by care. |
| `ResultsWorklistView.xaml:11` is the only filter CheckBox | ✅ | Confirmed: only `IsChecked="{Binding HasResult, …}"` existed in the filter row. |
| `ResultsWorklistView.xaml:80` `IsReviewed` is a **grid column** | ✅ | `<DataGridTextColumn Header="مُراجعة" Binding="{Binding IsReviewed}" Width="80" />` at line 80, adjacent to the `مطبوعة` and `مُسلمة` columns. **Preserved.** |

- [x] 1. Pre-Execution Verification — HEAD = S3 commit `e26b8e9`; tree clean; build 0/0
- [x] 2. Deep Understanding — P-01 §6; C-3, C-4
- [x] 3. File Analysis — query, handler (full), ViewModel, view XAML read; the four existing filter guards read and used as the template
- [x] 4. Planning — recorded below
- [x] 5. Execution — `bool? IsPrinted = null` appended; `if (request.IsPrinted.HasValue)` narrowing predicate; `IsReviewed` CheckBox added **only**; the `:80` grid column retained; labels registered
- [x] 6. Post-Execution Verification — build 0/0; suite green
- [x] 7. Validation Gate — VG-04 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist + UI Texts Register
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-01] Slice 4/6: …`

#### Stage 4 — S4 planning note

**F6 (real backend work).** `bool? IsPrinted = null` appended at the tail of the positional record, then:

```csharp
if (request.IsPrinted.HasValue)
{
    var isPrinted = request.IsPrinted.Value;
    rows = rows.Where(pt => pt.IsPrinted == isPrinted).ToList();
}
```

**The `false` case is handled structurally, not by care.** Three things together make "false means unprinted" impossible to get wrong:

1. **The type is `bool?`, not `bool`.** A non-nullable `bool` could not express "no filter", so `false` would be indistinguishable from absent. Pinned by `ResultWorklist_IsPrinted_IsATriStateParameter`, which asserts the property type *and* that the constructor default is null.
2. **`.Value` is read only inside the guard.** Writing `pt.IsPrinted == request.IsPrinted` outside the guard would not compile against a `bool?`, and would be wrong anyway.
3. **The predicate shape is copied from the four existing filters**, so it is the codebase's own idiom rather than a new pattern.

**F7 (XAML only).** One CheckBox bound to the existing `IsReviewed`. **No new query parameter, no handler branch** — the warning in the package ("if you find yourself writing a new `IsReviewed` query parameter or handler branch, you have misread the plan") was actively avoided, and `ResultWorklist_IsReviewedFilter_Narrows` exercises the *pre-existing* parameter to prove it works.

**A decision the plan did not spell out — the printed checkbox is `IsThreeState="True"`.** The query parameter is a tri-state, so the control must be too, or the UI could never express "no filter":

| CheckBox state | Bound `bool?` | Meaning |
|---|---|---|
| indeterminate | `null` | no filter |
| unchecked | `false` | **only unprinted** |
| checked | `true` | only printed |

This is the UI half of C-4's requirement, and it is pinned by `ResultsWorklist_PrintCheckboxIsTriState_SoFalseIsDistinguishableFromAbsent`, which also asserts the `IsThreeState` flag appears **before** the `IsPrinted` binding in document order (so the flag cannot be applied to the wrong checkbox).

**A domain fact discovered while writing the fixtures.** `MarkPrinted` throws `Result not reviewed.` and `MarkReviewed` throws `Result not entered.` — the result lifecycle is enforced in the Domain. Any fixture wanting a printed row must go `EnterResult → MarkReviewed → MarkPrinted`. Recorded because it is why the "printed" fixture looks longer than expected, and because it means **printed ⇒ reviewed** always holds in production — so `IsPrinted = false` and `IsReviewed = false` are genuinely different filters, which is what the three distinguishable checkbox labels assert.

**17 new tests** (9 Application + 8 Presentation):

| VG-04 requirement | Test | Key assertion |
|---|---|---|
| **`IsPrinted=false` returns only unprinted** | `ResultWorklist_FilterIsPrintedFalse_ReturnsOnlyUnprinted` | fixture has rows 101 (unprinted), 102 (**printed**), 103 (reviewed, unprinted) ⇒ result is exactly `{101,103}`, and **explicitly `DoesNotContain(102)`** |
| `IsPrinted=true` returns only printed | `ResultWorklist_FilterIsPrintedTrue_ReturnsOnlyPrinted` | exactly `{102}` |
| `IsPrinted=null` applies no filter | `ResultWorklist_IsPrintedNull_AppliesNoFilter` | all three rows; explicit null ≡ absent |
| **false ≠ no filter** | `ResultWorklist_FilterIsPrintedFalse_IsNotTheSameAsNoFilter` | the negative filter's set is **strictly smaller** and `NotEqual` to the no-filter set |
| negative case at scale | `ResultWorklist_FilterIsPrintedFalse_ExcludesEveryPrintedRow` | 6 rows, 3 printed ⇒ **exactly 3** unprinted, all with `IsPrinted == false` |
| tri-state type pinned | `ResultWorklist_IsPrinted_IsATriStateParameter` | property type is `bool?`; ctor default is null |
| **F7 narrows through the existing parameter** | `ResultWorklist_IsReviewedFilter_Narrows` | `{101}` / `{102}` / `{101,102}` for false / true / null |
| the two filters combine | `ResultWorklist_PrintAndReviewFiltersCombineWithAnd` | reviewed **and** unprinted ⇒ exactly `{103}` — the row that is reviewed but not printed exists precisely to make this non-trivial |
| three separate parameters | `ResultWorklist_HasResultFilter_IsUnaffected` | `HasResult`, `IsReviewed`, `IsPrinted` all present and distinct |
| **`:80` column still present** | `ResultsWorklist_ReviewedGridColumn_StillPresent` | the `مُراجعة` `DataGridTextColumn` **and** its `مطبوعة`/`مُسلمة` neighbours all still in the XAML |
| three filter checkboxes | `ResultsWorklist_HasThreeFilterCheckBoxes` | `HasResult`, `IsReviewed`, `IsPrinted` each bound |
| tri-state control | `ResultsWorklist_PrintCheckboxIsTriState_…` | `IsThreeState` precedes the `IsPrinted` binding |
| distinct labels | `ResultsWorklist_FilterLabelsAreDistinct` | `لدي نتيجة:` / `مُراجعة:` / `مطبوعة:` |
| public tri-state properties | `ResultsWorklist_FilterPropertiesArePublicAndTriState` | all three are public `bool?` |
| RTL + preserved controls | two tests | `FlowDirection`, `اليوم:` / `تحديث` / `النتيجة` |

---

## VG-04 — evidence (Slice 4)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)` |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1634/1634** (Δ**+9**) · Infrastructure **265/265** (Δ0) · Presentation **88/88** (Δ**+8**) · Persistence **13 / 2 skipped** (Δ0). Full suite **2507 passed, 0 failed, 2 skipped, 2509 total** (Δ**+17**) |
| 3 | **`ResultWorklist_FilterIsPrintedFalse_ReturnsOnlyUnprinted`** | ✅ | `{101,103}` of `{101,102,103}`; `DoesNotContain(102)` |
| 4 | `ResultWorklist_FilterIsPrintedTrue_ReturnsOnlyPrinted` | ✅ | `{102}` |
| 5 | `ResultWorklist_IsPrintedNull_AppliesNoFilter` | ✅ | all three rows |
| 6 | `ResultWorklist_IsReviewedFilter_Narrows` (F7) | ✅ | `{101}` / `{102}` / `{101,102}` through the **pre-existing** parameter |
| 7 | **`ResultsWorklistView.xaml:80` column still present** | ✅ | `grep` confirms `Header="مُراجعة" Binding="{Binding IsReviewed}"` still in the file (now at line 96, after the filter row grew by 11 lines). Also asserted in-test. |
| 8 | `MainWindow_Xaml_Bindings_ResolveToPublicProperties` green | ✅ | Presentation **88/88** |
| 9 | All S1–S3 tests still green | ✅ | Application Δ+9 and Presentation Δ+8 are **purely new tests**; no pre-existing count fell |
| 10 | `git diff` over `Migrations/` empty | ✅ | empty |
| 11 | No package / `.csproj` change | ✅ | empty |

### Slice 5 — Price-list print
**Gate:** VG-05. **Status:** ✅ COMPLETE — commit `see Execution Log`

**Stage 3 — file analysis confirmations:**

| Claim | Confirmed? | Evidence |
|---|---|---|
| **C-5** `PriceListsViewModel.cs:76-82` — seven commands, none prints | ✅ **exactly** | The constructor held exactly `LoadListsCommand, NewCommand, SaveListCommand, DeleteListCommand, AddItemCommand, SaveItemPriceCommand, RemoveItemCommand`. **No print command.** The class doc-comment even said *"No print control exists (unresolved owner decision)"* — now resolved and replaced. |
| **C-5** no price-list writer in `Infrastructure/Printing/` | ✅ | 20 files, none for price lists or groups. |
| `WorkSheetPdfWriter` pattern to imitate | ✅ | Read in full (235 lines): **static ctor** setting `Settings.License = LicenseType.Community` + `Settings.UseSystemFonts = true`; `ArabicFontResolver.Resolve(labText.FontFamily)`; `LabPrintTextDto` header; A4 portrait; `DirectionFromRightToLeft`; never-overwrite `IOException`; and a **pure `BuildTextLines` static** for content testing. All of it followed. |
| `IWorkSheetPdfWriter` port shape to mirror | ✅ | `WritePdfAsync(string absolutePath, VisitWorkSheetDto, LabPrintTextDto, CancellationToken = default)` in `Application/Common/Interfaces/`. `IPriceListPdfWriter` mirrors it exactly over `PriceListDetailDto`. |
| DI registration point | ✅ | `DependencyInjection.cs:80-81` registers `IWorkSheetPdfWriter` → `WorkSheetPdfWriter` scoped. `IPriceListPdfWriter` added beside it. |
| `PriceListDtos.cs` must not be widened | ✅ | `PriceListSummaryDto`, `PriceListItemDto(TestId, TestName, TestCode, Price)`, `PriceListDetailDto(Id, Name, Items)` — **untouched**. The writer consumes it as-is. |
| Save-path dialog already exists | ✅ **new, saves work** | `IDialogService.PickPdfSavePathAsync(string? suggestedFileName)` already existed — no new dialog API needed. |
| `ILabPrintTextStore` scope enum | ✅ **new** | `LabPrintTextScope { Report = 0, Receipt = 1, Envelope = 2 }`. **No new scope was added** — `Report` is used for the price list, which avoids touching a shared enum. |

- [x] 1. Pre-Execution Verification — HEAD = S4 commit `88b91c9`; tree clean; build 0/0
- [x] 2. Deep Understanding — P-01 §7; C-5, SD-10
- [x] 3. File Analysis — `WorkSheetPdfWriter` read in full; `IWorkSheetPdfWriter` read; `PriceListDtos`, `PriceListsViewModel`, `PriceListsView.xaml`, `IDialogService`, `ILabPrintTextStore` and the DI file read
- [x] 4. Planning — recorded below
- [x] 5. Execution — port in Application; writer in Infrastructure with its **own** static ctor; DI registration; `PrintListCommand` disabled with no selection; `طباعة` control; layering respected
- [x] 6. Post-Execution Verification — build 0/0 (verified with `--no-incremental`); suite green
- [x] 7. Validation Gate — VG-05 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist + UI Texts Register
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-01] Slice 5/6: …`

#### Stage 4 — S5 planning note

**The owner's two binding rules were implemented as follows.**

**Rule 1 — licence in the writer's own static constructor.** `PriceListPdfWriter` has its own `static PriceListPdfWriter()` setting both `Settings.License = LicenseType.Community` and `Settings.UseSystemFonts = true`. It does not depend on any other writer.

**Rule 2 — the test must pass under `--filter` in isolation.** This is the check that would have caught the pre-existing `PatientReportPdfExporter` flake, and it was run explicitly:

```
dotnet test tests/TopLab.Infrastructure.Tests --filter "FullyQualifiedName~PriceListPdfWriterTests"
→ Passed!  - Failed: 0, Passed: 8, Skipped: 0, Total: 8
```

In that run **no other writer's static constructor executes**, so the licence can only have come from `PriceListPdfWriter`'s own. Had the licence been inherited, this run would have failed with the same `LicenseChecker` exception seen in S1. A full-suite pass alone would not have proven anything here.

**Three artefacts, because C-5 was right that two were missing:** the port (`IPriceListPdfWriter`), the writer (`PriceListPdfWriter`), and the command (`PrintListCommand`) + its `طباعة` button.

**Decision recorded — how the ViewModel reaches the writer.** P-01 §7 step 3 said to follow whichever pattern `WorkSheetPrintingService` uses. It was followed in spirit but **not** by adding a printing *service*: `PriceListsViewModel` takes `IPriceListPdfWriter` and `ILabPrintTextStore` directly, opens the save dialog itself, and calls the port. Rationale: a service would only add an indirection with no behaviour, and the DTO the writer needs is one mediator call away (`GetPriceListByIdQuery`). This keeps Presentation dependent only on Application types (SD-10) while adding no layer the plan does not ask for.

**Decision recorded — the command's CanExecute.** `new AsyncRelayCommand(_ => PrintListAsync(), () => SelectedList is not null)`, and the `SelectedList` setter calls `PrintListCommand.RaiseCanExecuteChanged()`. Without that raise, the button would stay greyed out forever after a selection — the classic WPF half-wired state the plan forbids. Pinned from both directions: disabled with no selection, enabled after one, disabled again when cleared.

**Decision recorded — `LabPrintTextScope`.** No new enum member. The existing `Report` scope is used for the price-list header. Adding a `PriceList` scope would have meant editing a shared enum for a print-only concern, and `Report` is exactly the workstation-local lab identification text (ADR-0027) the writer needs. **The enum is untouched.**

**The writer is asserted on real output, not "did not throw".** `BuildTextLines` is a pure static that returns every rendered line, and the tests assert the lab name, the three Arabic column headers, all three item rows (name + code + price) and the totals line. Beyond that, `PriceListPdfWriter_ProducesNonEmptyPdf` checks the file exists, is non-empty, carries the `%PDF` header **and** a `%%EOF` trailer, and `PriceListPdfWriter_RenderedPdfIsLargerThanAnEmptyDocument` proves the item rows were genuinely laid out by showing a 3-item PDF is materially larger than the same list with no items.

---

## VG-05 — evidence (Slice 5)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `dotnet build TopLab.sln -p:EnableWindowsTargeting=true --no-incremental` → `Build succeeded. 0 Warning(s) 0 Error(s)`. Four intermediate warnings (duplicate `using`, 3× CS8625) and four errors were introduced and all eliminated. |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1634/1634** (Δ0) · Infrastructure **273/273** (Δ**+8**) · Presentation **94/94** (Δ**+6**) · Persistence **13 / 2 skipped** (Δ0). Full suite **2521 passed, 0 failed, 2 skipped, 2523 total** (Δ**+14**) |
| 3 | `PriceListPdfWriter_ProducesNonEmptyPdf` | ✅ | file exists, `bytes.Length > 0`, starts `%PDF`, tail contains `%%EOF` |
| 4 | `PriceListPdfWriter_RendersListAndItems` | ✅ | header + the 3 Arabic column headers + all 3 rows with name/code/price + `عدد التحاليل: 3` |
| 5 | `PriceListPdfWriter_RejectsEmptyPath` / `RejectsNullArgument` | ✅ | `""` and `"   "` → `ArgumentException`; null list / null labText / null to `BuildTextLines` → `ArgumentNullException`. Also: never-overwrites → `IOException`, missing directory → `DirectoryNotFoundException` |
| 6 | `PriceListsViewModel_PrintCommand_DisabledWithNoSelection` | ✅ | `CanExecute(null)` is false with no selection, true after selecting, false again when cleared |
| 7 | **`PresentationLayering_NoInfrastructureReferenceOutsideAppXaml`** | ✅ | layering filter run in isolation → `Passed! 2/2`. The ViewModel names only `IPriceListPdfWriter` / `ILabPrintTextStore`, both in Application. |
| 8 | `Directory.Packages.props` unchanged | ✅ | `git diff -- Directory.Packages.props` empty; **no `.csproj` touched** |
| 9 | `git diff` over `Migrations/` empty | ✅ | empty |
| 10 | **Owner rule: writer test under `--filter` in isolation** | ✅ | `Passed! 8/8` under `--filter "FullyQualifiedName~PriceListPdfWriterTests"` — no other writer's static ctor runs, so the licence came from this writer alone |
| 11 | `PriceListDtos.cs` not widened | ✅ | the file is unmodified; the writer consumes the existing three records as-is |

### Slice 6 — Custom-group print + wave DoD
**Gate:** VG-06. **Status:** ✅ COMPLETE — commit `see Execution Log`

**Stage 3 — file analysis confirmations:**

| Claim | Confirmed? | Evidence |
|---|---|---|
| **C-6** `CustomGroupsViewModel.cs:76-82` — seven commands, parallel shape, no print | ✅ **exactly** | `LoadGroupsCommand, NewCommand, SaveGroupCommand, DeleteGroupCommand, AddItemCommand, SaveItemPriceCommand, RemoveItemCommand` at lines 76-82. **No print command.** |
| **C-6** the DTOs are unrelated | ✅ **confirmed** | `CustomGroupDtos.cs` holds `CustomGroupSummaryDto(int, string, int)`, `CustomGroupItemDto(int TestId, string TestName, string TestCode, decimal Price)`, `CustomGroupDetailDto(int, string, IReadOnlyList<CustomGroupItemDto>)` — **structurally parallel to `PriceListDtos.cs` but distinct types.** This is why a shared writer would have forced a DTO to be widened (AS-6). |
| No group writer existed | ✅ | confirmed in S5's file listing; `Infrastructure/Printing/` gained exactly one new file this slice. |

- [x] 1. Pre-Execution Verification — HEAD = S5 commit `76f3631`; tree clean; build 0/0
- [x] 2. Deep Understanding — P-01 §8 + §13; C-6, SD-10
- [x] 3. File Analysis — `CustomGroupsViewModel`, `CustomGroupsView.xaml`, `CustomGroupDtos.cs` read; S5's writer re-read as the shape to mirror
- [x] 4. Planning — recorded below
- [x] 5. Execution — `ICustomGroupPdfWriter` + `CustomGroupPdfWriter` with its **own** static ctor; DI; `PrintGroupCommand` disabled with no selection; `طباعة` control; **no shared writer or DTO**; then the full wave DoD
- [x] 6. Post-Execution Verification — build 0/0 (`--no-incremental`); suite green
- [x] 7. Validation Gate — VG-06 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist + UI Texts Register
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-01] Slice 6/6: …`

#### Stage 4 — S6 planning note

**S5's shape was mirrored exactly, and nothing was shared.** The two print paths share **no class, no port and no DTO**:

| | F8 price list | F9 custom group |
|---|---|---|
| Port | `IPriceListPdfWriter` | `ICustomGroupPdfWriter` |
| Writer class | `PriceListPdfWriter` | `CustomGroupPdfWriter` |
| Text-line records | `PriceListTextLines` / `PriceListTextRow` | `CustomGroupTextLines` / `CustomGroupTextRow` |
| DTO in | `PriceListDetailDto` / `PriceListItemDto` | `CustomGroupDetailDto` / `CustomGroupItemDto` |
| Command | `PriceListsViewModel.PrintListCommand` | `CustomGroupsViewModel.PrintGroupCommand` |

**C-6 is asserted negatively, not just by inspection.** Four tests would fail if anyone merged the two paths:

- `CustomGroupWriter_IsADistinctTypeFromThePriceListWriter` — the writer types are different and neither is assignable to the other.
- `CustomGroupWriter_DeclaresItsOwnTextLineTypes` — each writer owns its content-mapping records.
- `CustomGroupPort_IsADistinctTypeFromThePriceListPort` — two distinct interfaces, and **each port's `WritePdfAsync` second parameter is its own DTO type**, asserted by reflection.
- `CustomGroups_UsesItsOwnPort_NotThePriceListPort` — each ViewModel's constructor takes its own port and **not** the other's.

A refactor that collapsed these into one generic "list writer" would fail all four, which is the point: C-6 names that collapse a defect, so it needed a test that notices it.

**The owner's two binding rules were applied here exactly as in S5**: `CustomGroupPdfWriter` sets `Settings.License` and `Settings.UseSystemFonts` in its **own** static constructor, and its tests were run **under `--filter` in isolation** — `Passed! 11/11` — where no other writer's static initialiser runs.

**One flake re-appeared during the S6 full-suite run and was triaged, not ignored.** `PatientReportPdfExporterTests.Export_Creates_ValidPdf` failed with the same QuestPDF `LicenseChecker` exception recorded at S1. Re-run of the whole project gave **284/284**, and the two new writers passed **19/19** both isolated and together. It is the same pre-existing defect, in the same pre-existing class, untouched by this wave — and this time it is *evidence for* the rule the owner imposed: had `CustomGroupPdfWriter` relied on `PriceListPdfWriter`'s static constructor, its isolated run would have failed exactly the way this one does.

---

## VG-06 — evidence (Slice 6)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `--no-incremental` → `Build succeeded. 0 Warning(s) 0 Error(s)` |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1634/1634** (Δ0) · Infrastructure **284/284** (Δ**+19** vs S5's 265… see note) · Presentation **101/101** (Δ**+7**) · Persistence **13 / 2 skipped** (Δ0). Full suite **2539 passed, 0 failed, 2 skipped, 2541 total** (Δ**+18** vs S5's 2521) |
| 3 | `CustomGroupPdfWriter_ProducesNonEmptyPdf` | ✅ | file exists, non-empty, `%PDF` header, `%%EOF` trailer |
| 4 | `CustomGroupPdfWriter_RendersGroupAndItems` | ✅ | header + 3 Arabic column headers + all 3 rows + `عدد التحاليل: 3` |
| 5 | `CustomGroupsViewModel_PrintCommand_DisabledWithNoSelection` | ✅ | disabled with no selection, enabled after selecting, disabled again when cleared |
| 6 | `PresentationLayering_NoInfrastructureReferenceOutsideAppXaml` green | ✅ | layering filter → `Passed! 2/2` |
| 7 | `Directory.Packages.props` unchanged | ✅ | `git diff 9d042d0 HEAD -- Directory.Packages.props` **empty** |
| 8 | **`git diff --stat` over `Migrations/` empty** | ✅ | `git diff 9d042d0 HEAD -- src/TopLab.Infrastructure/Persistence/Migrations/` **empty** — zero migrations this wave |
| 9 | Every `.csproj` unchanged | ✅ | `git diff 9d042d0 HEAD -- '*.csproj'` **empty** |
| 10 | **Owner rule: writer test under `--filter` in isolation** | ✅ | `Passed! 11/11` for `CustomGroupPdfWriterTests` alone; `19/19` for both new writers together |
| 11 | All S1–S5 tests still green | ✅ | Domain and Application counts unchanged from S5; the only Infrastructure failure was the pre-existing flake, which passed on re-run |
| 12 | **No shared writer / DTO between F8 and F9** | ✅ | four dedicated negative tests (see the table above) |

**Baseline delta note for this slice.** Infrastructure's count is **284** against the pre-wave baseline of **265** — that is **+19**: +8 from `PriceListPdfWriterTests` and +11 from `CustomGroupPdfWriterTests`. The count did not fall.

---

## Wave DoD (P-01 §13) — final verification

| # | DoD item | Result | Evidence |
|---|---|---|---|
| 1 | S1–S6 complete, every `VG-nn` green item by item | ✅ | 8 + 16 + 11 + 11 + 11 + 12 = **69 gate items**, all recorded with evidence above |
| 2 | **Zero migrations** | ✅ | `git diff 9d042d0 HEAD -- src/TopLab.Infrastructure/Persistence/Migrations/` → **empty**. Still 29 files, 14 migrations, none created, none edited, snapshot untouched. |
| 3 | **Zero package changes** | ✅ | `git diff 9d042d0 HEAD -- Directory.Packages.props` → **empty**; `git diff 9d042d0 HEAD -- '*.csproj'` → **empty** |
| 4 | Build **0 errors / 0 warnings** | ✅ | `dotnet build TopLab.sln -p:EnableWindowsTargeting=true --no-incremental` → `0 Warning(s) 0 Error(s)` |
| 5 | Full suite at or above the measured baseline | ✅ | baseline **2441 passed / 2 skipped / 2443** → final **2539 passed / 2 skipped / 2541**. **Δ +98 passed, +0 failed, +0 new skips.** No project fell below baseline at any slice. |
| 6 | All **six** search filter controls present and independent | ✅ | `PatientSearch_HasSixFilterControls_EachBoundToItsOwnProperty` asserts **six**; `PatientSearch_DoctorAndReferralControlsAreDistinct_FailsIfEverMerged` + the separate-lookup test guard the doctor/referral split |
| 7 | Both worklist checkboxes present; the `IsReviewed` grid column still present | ✅ | `ResultsWorklist_HasThreeFilterCheckBoxes` (three filters) and `ResultsWorklist_ReviewedGridColumn_StillPresent` (the `مُراجعة` `DataGridTextColumn` survives) |
| 8 | Two PDF print paths working, each behind its own Application-layer port | ✅ | `IPriceListPdfWriter`/`PriceListPdfWriter` and `ICustomGroupPdfWriter`/`CustomGroupPdfWriter`; each writer's isolated test run green; layering 2/2 green |
| 9 | `PresentationStructuralTests` and `PresentationLayeringTests` green | ✅ | structural filter → `Passed! 4/4`; layering filter → `Passed! 2/2` |
| 10 | **Six local commits only** — the owner pushes | ✅ | `git log --oneline 9d042d0..HEAD` → exactly 6 commits, all local. **Never pushed. No branch, amend, rebase, reset, stash, clean or tag. No `git add -A` / `git add .`** |

**Functions delivered — all nine:** F1 treating doctor ✅ · F2 test ✅ · F3 gender ✅ · F4 age ✅ · F5 date range ✅ (six controls) · F6 results-not-printed ✅ (backend + checkbox) · F7 results-not-verified ✅ (XAML only, as C-3 required) · F8 price-list print ✅ · F9 custom-group-list print ✅.

---

## Ruling Log and Recorded Assumptions

**AS-1 is CLOSED by owner ruling (2026-10-03).** The owner has ruled on it; it is no longer an open matter and must not be re-opened by the executing agent. Its verbatim clarification is appended to the end of `P-01-Execution-Prompt.md` at issue time.

**AS-2 … AS-8** were encountered while preparing the P-01 package. They are recorded here as required, each resolved with the reasoning given, and each remains available for the owner's review — none blocked delivery, and none was decided silently.

| # | Item | Reasoning |
|---|---|---|
| **AS-1** ✅ **CLOSED — owner ruling 2026-10-03** | **The display cap is preserved exactly as it is. No literal 100 is introduced anywhere; the reference system's 100 is not adopted.** | **Owner ruling (binding):** *"The project has no 100-patient cap. The existing display cap is the PageSize parameter of SearchPatientsGlobalQuery, whose default is 50, and the project currently holds no patient data at all. Preserve the existing PageSize value exactly as it is. Do not raise it, do not lower it, and do not introduce a literal 100 anywhere. The reference system's 100 is not adopted. Reading AS-1 as 'keep the existing cap unchanged' is correct."* This confirms the reading originally proposed, so **C-1, SD-7, guardrail clause 3, the counting trap and VG-01 are unchanged and remain accurate as written.** The owner's attestation that the project holds no patient data also removes any residual data-exposure concern. **Do not re-open.** |
| **AS-2** | The six search filters are exposed as **six independent filter clauses ANDed together**, each inert when null, rather than as the reference's discrete mode tabs. | The reference describes constraining a result (REF3 p.34 §1) rather than switching modes, and REF3 p.35 describes identifier boxes alongside it. The codebase's existing `SearchPatientsGlobalQuery` is a single record with one text parameter. Six ANDed optional parameters extend that shape without a UI-mode concept the project does not have. |
| **AS-3** | Search **by test** matches patients who **have** a `PatientTest` for the chosen test; it does not additionally require a recorded result or a date window. | RLS p.42 names the mode without qualifying it. "Has the test" is the minimal literal reading; adding a result or date condition would be inference. |
| **AS-4** | The date-range filter bounds `RegistrationDateUtc`. | It is the only patient-level date in the entity that a search would bound naturally, and it is already the worklist's ordering key. Any-visit-date was rejected as the more complex reading. |
| **AS-5** | The age filter is a **band** (`AgeUnit` + from/to) rather than an exact age, because the reference restricts to an "age stage" (REF3 p.34 §1) and `Patient` stores `AgeValue` with an `AgeUnit`. | An exact-age match would make the filter nearly useless across three age units, and `AgeRules` already forbids unit conversion. |
| **AS-6** | The two printable lists (F8, F9) get **two separate ports and two separate writers**, not one generic "list" writer. | C-6. The DTOs are unrelated, and `SD-10` requires Presentation to reach Infrastructure through an Application port; a shared writer would have to widen one DTO to fit both and would blur the two functions the owner listed separately. |
| **AS-7** | S1 is a **test-only** slice that changes no production file. | The wave widens one `IQueryable` by six filters. Without pinned characterisation tests, a paging or cap regression in S2 could not be distinguished from one introduced by the filters. This follows the W-01 "VG before change" discipline. |
| **AS-8** | Filter control labels are **UI-only strings** created once and registered, not drawn from a frozen message table. | This wave introduces no backend error message (SD-9), so there is no frozen table to copy from. Each label is registered so the owner can review the wording. |

---

## Migration Register

| Slice | Migration name | Tables/columns | Backup | Review agent |
|---|---|---|---|---|
| — | **NONE. This wave creates zero migrations.** | — | — | — |

---

## Created UI Texts Register (append at execution time)

| Slice | Screen | Text | Kind |
|---|---|---|---|
| 3 | PatientSearchView | «الطبيب المعالج:» | Filter label (F1a, treating doctor) — created once |
| 3 | PatientSearchView | «جهة الإحالة:» | Filter label (F1b, **separate control** — SD-6) — created once |
| 3 | PatientSearchView | «التحليل:» | Filter label (F2) — created once |
| 3 | PatientSearchView | «الجنس:» | Filter label (F3) — created once |
| 3 | PatientSearchView | «العمر:» | Filter label (F4) — created once |
| 3 | PatientSearchView | «إلى» | Age-band inner label (F4) — created once |
| 3 | PatientSearchView | «من تاريخ:» / «إلى تاريخ:» | Filter labels (F5) — created once |
| 3 | PatientSearchView | «مسح الفلاتر» | Clear-filters button (P-01 §5 step 6) — created once |
| 3 | PatientSearchView | «الكل» | "All" sentinel heading each of the 3 lookups; forwards a null id = no filter |
| 3 | PatientSearchView | «ذكر» / «أنثى» | Gender choices (F3) — created once |
| 3 | PatientSearchView | «يوم» / «شهر» / «سنة» | Age-unit choices (F4) — created once |
| 4 | ResultsWorklistView | «مُراجعة» (filter) | Checkbox label (F7) |
| 4 | ResultsWorklistView | «مطبوعة» | Checkbox label (F6) |
| 5 | Price-list print | «طباعة» | Print button (F8) — created once |
| 5 | Price-list print | «التحليل» / «الكود» / «السعر» / «عدد التحاليل: N» | Print column headers + totals line (F8) — created once |
| 6 | Group-list print | «طباعة» | Print button (F9) — created once |
| 6 | Group-list print | «التحليل» / «الكود» / «السعر» / «عدد التحاليل: N» | Print column headers + totals line (F9) — created once |

**Not created, and why:** no copyright text, no support address and no commercial name was invented (SD-9). The «الفرع» branch-notice string was **not** created in this wave — it is pre-existing text on `PatientSearchView.xaml:15` and `PatientSearchViewModel`, left byte-for-byte unchanged (SD-11).

Any label not listed here is marked **`TBD-AR`** and **not invented** (SD-9).

---

## Execution Log

| Slice | Commit | Notes |
|---|---|---|
| 1 | `8ff279b` | 19 characterisation tests, **no production file touched**. Build 0/0. Application Δ**+19**. `git diff` over `src/`, `Migrations/` and `Directory.Packages.props` all empty. All 8 VG-01 items pass. Pre-existing QuestPDF-licence flake in `PatientReportPdfExporterTests` found and recorded (not caused by this wave). |
| 2 | `a968364` | Six defaulted members appended at the tail; **seven** guard clauses (F5's two bounds guarded independently); `AgeValueBand` **created** (did not exist — AS-5 settles its shape); build 0/0; Application Δ**+17**; full suite **2477 passed / 2 skipped**. Paging, ordering, cap and the `Text` clause proven untouched by a `git diff | grep` returning no matches. All 16 VG-02 items pass. |
| 3 | `e26b8e9` | Six public bindable properties + six controls; three lookups reuse existing `SearchExternalEntitiesQuery` / `SearchTestCatalogQuery` idioms (no new query); `ClearFiltersCommand` added per §5 step 6 with a suspend flag so one reload fires, not nine; build 0/0; Presentation Δ**+13**; full suite **2490 passed / 2 skipped**. All 11 VG-03 items pass, incl. the six-control count and the merge-detection test. |
| 4 | `88b91c9` | F6 = real backend work (`bool? IsPrinted = null` + `if (request.IsPrinted.HasValue)` predicate copied from the four existing filters); F7 = **XAML only**, no new `IsReviewed` parameter or handler branch; printed checkbox is `IsThreeState` so false ≠ absent; the `مُراجعة` grid column survives (now line 96); build 0/0; Application Δ**+9**, Presentation Δ**+8**; full suite **2507 passed / 2 skipped**. All 11 VG-04 items pass. |
| 5 | `76f3631` | `IPriceListPdfWriter` (Application) + `PriceListPdfWriter` (Infrastructure) + DI + `PrintListCommand` + `طباعة` button; **own static ctor sets the licence**; writer test passes **8/8 under `--filter` in isolation** (owner rule 2 — this is the check that proves the licence is not inherited); PDF asserted on real output incl. `%%EOF` and a larger-than-empty size comparison; layering 2/2 green; no printing *service* added; `LabPrintTextScope` untouched; build 0/0; Infrastructure Δ**+8**, Presentation Δ**+6**; full suite **2521 passed / 2 skipped**. All 11 VG-05 items pass. |
| 6 | `f89968e` | `ICustomGroupPdfWriter` + `CustomGroupPdfWriter` with its **own** static ctor; DI; `PrintGroupCommand` + `طباعة`; **C-6 asserted negatively by 4 tests** (writer types, text-line records, port types + each port's own DTO parameter, and each ViewModel taking only its own port); writer test **11/11 under `--filter` in isolation**, both new writers 19/19 together; build 0/0; Infrastructure Δ+11, Presentation Δ+7; full suite **2539 passed / 2 skipped**. All 12 VG-06 items pass, plus the 10-item **wave DoD**. |

---

## Current Status

**WAVE P-01 COMPLETE.** All six slices committed locally; every `VG-nn` green item by item; the wave DoD satisfied.

| | Baseline (G0) | Final (S6) | Δ |
|---|---|---|---|
| Domain | 507 / 507 | 507 / 507 | 0 |
| Application | 1589 / 1589 | 1634 / 1634 | **+45** |
| Infrastructure | 265 / 265 | 284 / 284 | **+19** |
| Presentation | 67 / 67 | 101 / 101 | **+34** |
| Persistence | 13 passed / 2 skipped | 13 passed / 2 skipped | 0 |
| **Full suite** | **2441 passed, 0 failed, 2 skipped (2443)** | **2539 passed, 0 failed, 2 skipped (2541)** | **+98 passed, +0 failed** |

Build 0 warnings / 0 errors. Zero migrations, zero schema changes, zero packages, zero `.csproj` edits. Six local commits on `main`; never pushed.

**The nine functions are delivered.** Next work is the owner's to schedule — the owner pushes these six commits.

---

## Stop Report

(none — no stop rule triggered)

**Two items are open for the owner and neither blocked the wave:**

1. **Pre-existing flake — `PatientReportPdfExporterTests.Export_Creates_ValidPdf`.** Observed at S1 (1 failure) and again at S6 (1 failure); passed on re-run both times. QuestPDF's `Settings.License` is process-global, set in four writers' static constructors, and `PatientReportPdfExporter` has none — it only passes when another writer ran first. Untouched by this wave, and fixing it would mean editing a production file outside every slice's scope. **«بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»**. The two new P-01 writers do **not** inherit it: each sets the licence in its own static constructor, and each was verified under `--filter` in isolation (8/8 and 11/11).
2. **`LabPrintTextScope`** has no `PriceList`/`CustomGroup` member — both print paths reuse the existing `Report` scope rather than editing a shared enum. Recorded as a deliberate decision, not an oversight. **«بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»** if the owner prefers dedicated scopes later.
