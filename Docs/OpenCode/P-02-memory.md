# Loop Engineering — Memory File

- **Module:** Parity Wave 2 (revised) — P-02.1 Wave 1 corrections · P-02.2 barcode reprint
- **Module Number:** P-02
- **Source Plan:** `Docs/OpenCode/P-02.md`
- **Execution Prompt:** `Docs/OpenCode/P-02-Execution-Prompt.md`
- **Date Created:** 2026-10-03
- **Total Slices:** 4
- **Current Slice:** — (start at S1)
- **Current Branch:** `main`
- **Baseline Commit:** `51095e597d2a8b9c8e43f594f70e99bb155d803c`
- **Author:** loop-engineering (execution by the local coding agent per owner authorization)
- **Migrations required by this wave: ZERO**
- **Supersedes:** the previous P-02 package, withdrawn — see SD-5

---

## Module Summary

Wave 2 is bounded by one rule: **a function is in this wave if and only if it can be executed, exactly as originally specified, without a migration.** Everything else is Wave 3.

Applying that rule to the original Wave 2 candidate set removed **four of its five items**. The three patient codes, the symbology catalogue, the per-test label generation with its saved position, and the receipt/envelope barcode toggle each need persistent storage, so each moved to Wave 3 with its migration. Only **A-12 — the independent barcode reprint** survives, because the command it needs already exists and the gap is an entry point.

What remains is four slices: the three Wave 1 defect corrections, then the reprint. This is a small wave, and that is the honest consequence of the boundary rather than a gap in the analysis.

**Wave 3 holds eleven functions** — described in the plan §8, not packaged here. None was redesigned to avoid a migration; none was deferred silently.

---

## The boundary rule (binding)

> **A function belongs to Wave 2 if and only if it can be executed, exactly as originally specified, without creating any new migration file and without editing any existing migration file. A function that requires a migration belongs to Wave 3. The boundary is the migration, not the behaviour.**

| Original Wave 2 item | Column needed, as specified? | Result |
|---|---|---|
| EN-1 three-code patient identity | Yes — the three codes are persistent identifiers (REF3 p.18) | **Wave 3** |
| B-4 symbology catalogue | Yes — a catalogue is stored rows | **Wave 3** |
| A-3 per-test container labels | Yes — REF3 p.20 §5 specifies a **saved** label position; nothing stores it today | **Wave 3** |
| A-18 barcode on receipt / envelope | Yes — no barcode member on `ReceiptSettings` or `EnvelopeSettings` | **Wave 3** |
| A-12 independent reprint | **No** — `PrintBarcodeCommand` exists and is tested; only an entry point is missing | **Wave 2 (S4)** |

---

## Settled Decisions (binding — do not reopen)

- **SD-1 — the plan is a hypothesis.** Every `file:line` below is an unverified claim. Verify at Stage 3. Code mismatch ⇒ **STOP and report**.
- **SD-2 — git.** One **local** commit per verified slice on `main`. **Never push.** No branch, amend, rebase, reset, stash, clean, tag, force-push. **Never `git add -A` / `git add .`**. Never commit on a red build or failed gate. The owner pushes after the wave.
- **SD-3 — migration budget: ZERO. Absolute.** No new migration class. No edit, delete or rename of any existing migration or `.Designer.cs`. **No** `ApplicationDbContextModelSnapshot.cs` change, by hand **or** by tooling. **No** `dotnet ef database update`. **No** `HasData` row. **No** new column. Every slice proves it with `git diff -- src/TopLab.Infrastructure/Persistence/` returning **empty**. A slice that appears to need a migration is a **STOP**, never a licence to improvise.
- **SD-4 — package budget: ZERO.** `Directory.Packages.props` and every `.csproj` untouched. No new dependency.
- **SD-5 — do not redesign to avoid a migration.** If a function needs a migration it is not in this wave. Moving it to Wave 3 is correct; **changing its behaviour so it fits here is not.** This is a prohibition, not a preference. It is why the previous P-02 package was withdrawn.
- **SD-6 — a reprint must never mint a new identifier.** The reference is explicit (REF3 p.16 §14). `GetNextLabId` and `Patient.LabId` are untouched by the reprint path.
- **SD-7 — Arabic strings.** No new backend error message. New **UI-only** strings are created once and appended to the register. Never invent copyright text, a support address, or a commercial name.
- **SD-8 — layering.** Presentation may not reference Infrastructure. The reprint goes through the existing `IBarcodeService` port.
- **SD-9 — scope ceiling.** Four slices. Three Wave 1 corrections plus one functional slice. Everything else is Wave 3.

---

## Recorded Decisions (taken by the planning agent under Part Five — binding)

| # | Decision | Reasoning |
|---|---|---|
| **RD-1** | **A-12 is scoped to the patient-card reprint** described at REF3 p.16 §14 — search the patient, print his barcode again, do not issue a new number — **not** the per-container-label print. | p.16 §14 is explicitly about a **lost patient card** and the **search window**. The "print any single label any number of times" wording at p.18 sits **inside the label window**, which is A-3's subject and is in Wave 3. Reading A-12 as the lost-card reprint is what makes it migration-free without changing its behaviour: `PrintBarcodeCommand(int PatientId)` already exists and is already tested. |
| **RD-2** | S2 fixes **both** two-state checkboxes — the Wave 1 `IsReviewed` one **and** the pre-existing `HasResult` one. | Leaving one working and two broken tri-states in the same toolbar row would be indefensible, and the pre-existing fix is a one-attribute XAML change with no migration. It is a correction of a defect the owner has already accepted as real, not new scope. |
| **RD-3** | S3 fixes the filter path only; the six pre-existing `_ = SomethingAsync()` sites elsewhere are **not** refactored. | That idiom is house style across `AccountsHubViewModel`, `CultureAttachmentViewModel`, `PriceListsViewModel`, `CustomGroupsViewModel` and `PatientsHubViewModel`. Only the filter path is exposed to the burst-of-input ordering risk. Keeping the change surgical respects the wave boundary. |
| **RD-4** | The 22 Linux Infrastructure failures get **no slice**. | Proved environmental: at `9d042d0` the same host fails 18, at `51095e5` it fails 22 — the same 18 plus 4, all the same missing-`Arial` cause. The owner instructed that this item requires no correction and must not be given a slice. |
| **RD-5** | **A-3 is moved to Wave 3**, not kept in Wave 2 with the position adjustment made transient. | REF3 p.20 §5 specifies adjusting the barcode position on the label and pressing **حفظ** — Save. A save with nowhere to persist is not the specified function, and no label layout or position is stored anywhere in the code today. The only ways to keep A-3 here would be to drop the save or to move the state to a file — and the first is a scope change and the second is the redesign that caused the withdrawal. Moving it to Wave 3 is the correct action. |
| **RD-6** | **A-18 is moved to Wave 3.** | `ReceiptSettings` has no barcode member and `EnvelopeSettings` has no barcode member; the optional toggle the reference specifies is a new boolean column on both. It also consumes the EN-1 file code. |
| **RD-7** | **A-13 is moved to Wave 3** even though it needs no column of its own. | It computes the pickup date from the turnaround field, which is B-6's column. A function blocked by a migration-requiring predecessor cannot execute in a migration-free wave. Moving it is explicit, not silent. |
| **RD-8** | The contract (lab-to-lab) case follow-up stays deferred and is placed in **neither** wave. | The owner deferred it from Wave 1 on the grounds that its evidentiary basis is too weak. No new evidence has arrived. Re-adding it would be an unauthorised scope change. |

---

## Plan-vs-Code Corrections (binding — see P-02 §2)

| ID | Correction |
|---|---|
| **C-1** | `PatientReportPdfExporter` is the only QuestPDF entry point with **no static constructor**. Run alone it throws *"Please configure the QuestPDF license"*. A **real, pre-existing, production-affecting** defect. S1. |
| **C-2** | The 22 Infrastructure failures are **environmental**, not a Wave 1 regression. 18 before Wave 1, the same 18 plus 4 after, all missing `Arial`. **No fix, no slice.** |
| **C-3** | `HasResult` (line 11) and `IsReviewed` (line 16) are two-state bound to `bool?`; `IsPrinted` (line 25) is `IsThreeState="True"`. "No filter" is unreachable for two of the three. S2. |
| **C-4** | `AgeFrom` / `AgeTo` are `TextBox` → `int?`; WPF cannot convert `""`, so the source keeps its old value. Only `مسح الفلاتر` clears it, and nothing tests it. S2. |
| **C-5** | `OnFilterChanged` discards a task whose `SearchAsync` has **no `catch`**. Errors lost; completion unsequenced. S3. |
| **C-6** | `_ = SomethingAsync()` is **house style** in six other ViewModels. Not introduced here. Fix the filter path only. |
| **C-7** | The barcode reprint command **already exists** and is already tested. The only production caller is `PatientEditorViewModel:1322`. The gap is an **entry point from search**. S4 adds the entry point and nothing else. |
| **C-8** | The reference forbids minting a new number on reprint. SD-6. |

---

## Confirmed Code Facts (verified at the pinned commit — confirm at Stage 3, do not re-derive blindly)

**S1 — the licence defect:**
- `src/TopLab.Infrastructure/Services/PatientReportPdfExporter.cs` — the only QuestPDF entry point with no static constructor. `grep "Settings.License"` across `Infrastructure/` matches six writers and **not** this file.
- The six that do: `CustomGroupPdfWriter` · `InvoicePdfWriter` · `PriceListPdfWriter` · `ReceiptPdfWriter` · `ReportPdfWriter` · `WorkSheetPdfWriter`.
- `PriceListPdfWriter`'s XML comment records the lesson: *"each writer sets the licence in its OWN static constructor and must not rely on another writer running first."*

**S2 — the two-state checkboxes and the age field:**
- `ResultsWorklistView.xaml:11` `HasResult` two-state · `:16` `IsReviewed` two-state · `:25` `IsPrinted` `IsThreeState="True"` · `:96` the `مُراجعة` grid column (display-only — must survive).
- `PatientSearchView.xaml` — `AgeFrom` / `AgeTo` `TextBox` bound to `int?`, `UpdateSourceTrigger=LostFocus`.

**S3 — the un-sequenced reload:**
- `PatientSearchViewModel.OnFilterChanged()` → `_ = ReloadFromFirstPageAsync()`. `SearchAsync` has `try` (offset 6) and `finally` (offset 49) and **no `catch`**.
- Six pre-existing `_ =` sites **out of scope** (RD-3): `AccountsHubViewModel:77,85` · `CultureAttachmentViewModel:57` · `PriceListsViewModel:113` · `CustomGroupsViewModel:108` · `PatientsHubViewModel:31,42`.

**S4 — the reprint surface:**
- `PrintBarcodeCommand(int PatientId)` — `IRequest<Result>`, `IAuthorizedRequest`, gated on `PatientRegistrationAccessPolicy.AddEditPatient`. Exists, implemented, tested (`PrintBarcodeCommandHandlerTests`, lines 15–166).
- Only production caller: `PatientEditorViewModel:1322`.
- `PatientSearchViewModel` has **no** barcode command. `SelectedItem` is `PatientSearchHitDto?` and already carries `Id`.
- `IBarcodeService.PrintBarcodeAsync(string value, ct)` prints one Code-128 label to the `Barcode` printer slot (`BarcodeService.cs:59`).
- `GetNextLabId` and `Patient.LabId` are the identifiers SD-6 protects.

**Wave 3 evidence (why these are Wave 3, not fixed here):**
- `ReceiptSettings` members: `TopMarginCm`, `Currency`, `PickupTimeDefault`, `PrintOnce`, `TestDetailDisplayMode`, `CashierPrinterEnabled`, `HeaderFooterMode` — **no barcode member**.
- `EnvelopeSettings` members: `TopMarginCm`, `HeaderFooterMode`, `SuppressCaptions` — **no barcode member**.
- `grep -rniE "LabelPosition|LabelLayout|LabelX|LabelY|barcodePosition" src/` → **no hits**. No label layout is stored.
- `BarcodeLabelRenderer` hard-codes `BarcodeFormat.CODE_128`; `ToAscii` in `BarcodeService` is frozen until WP-23.

**Structural gates:**
- `PresentationStructuralTests.MainWindow_Xaml_Bindings_ResolveToPublicProperties` (line 90) · `EveryWindow_HasCreationSite` (line 117) · `EveryView_IsRightToLeft` (line 168).
- `PresentationLayeringTests.PresentationLayering_NoInfrastructureReferenceOutsideAppXaml` (line 28).

**Counting traps this wave contains:**
- `grep -rn "Settings.License"` matches six writers plus migration files; **six writers, and `PatientReportPdfExporter` is not one of them.** Strip migrations before reasoning.
- `grep -rn "_ = "` across the Presentation layer returns many sites. Only `PatientSearchViewModel` is in scope (RD-3).
- `PrintBarcodeCommand` appears in five test files. Those tests are an **asset here** — S4 reuses the command and must leave them green.

---

## Global Validation Gates

- **G0 (once, before Slice 1):** measure your own baseline — build 0/0, each test project's pass/total, `git rev-parse HEAD`, `git status --porcelain`, `has-pending-model-changes`, the migration file count, and `git diff -- src/TopLab.Infrastructure/Persistence/`. Record and report it.
- **G1 (every slice):** build 0/0; no test count below your measured baseline; the slice's `VG-nn` item by item; `git diff -- src/TopLab.Infrastructure/Persistence/` **empty**; `git diff -- Directory.Packages.props '*.csproj'` **empty**.

### Migrations policy (every slice — absolute)
- **Zero** new migrations. The folder is read-only for this entire wave.
- **Never** edit, delete or rename any migration or its `.Designer.cs`.
- **Never** touch `ApplicationDbContextModelSnapshot.cs` — not even via tooling.
- **Never** run `dotnet ef database update`.
- **Never** add a column, a `HasData` row, or a permission code.
- **Never** redesign a function to make it fit this wave (SD-5).
- If a slice appears to need any of the above, that is a **STOP condition**.

---

## Quality Gate (a slice is complete ONLY when ALL hold)

1. Scope matches `P-02.md` (SD-1).
2. `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` → **0 errors, 0 warnings**.
3. No test count below your measured baseline.
4. The slice's `VG-nn` passes item by item, with evidence recorded here.
5. **`git diff -- src/TopLab.Infrastructure/Persistence/` is EMPTY.**
6. `git diff -- Directory.Packages.props '*.csproj'` is **empty**.
7. Local commit only, message `[P-02] Slice N/4: <title> — loop-engineering`.

---

## Stop/Continue Rule

- **Continue automatically** after a passing gate and a local commit. The only normal stop is S4 plus the wave DoD.
- **STOP** on: plan/code mismatch (SD-1) · any need for a migration, column, `HasData` row, permission code or package (SD-3, SD-4) · any temptation to redesign a function to avoid a migration (SD-5) · a reprint that mints a new `LabId` (SD-6) · red build or tests not restorable within slice scope · the same failure **5 consecutive times** · about to launch the application or touch a migration · any ambiguity not settled in §0, reported as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية».
- Write the Stop Report in §Stop Report below and wait.

---

## Baseline (fill in G0 on the owner's Windows machine before Slice 1)

| Item | Value |
|---|---|
| Date measured | 2026-10-03 |
| .NET SDK / VS MSBuild | .NET SDK **9.0.318** (`dotnet build`; .NET 8 SDK 8.0.425 also installed). VS MSBuild not used. |
| `git rev-parse HEAD` | `51095e597d2a8b9c8e43f594f70e99bb155d803c` ✓ **exact match** |
| `git status --porcelain` | only the three P-02 package files untracked (`P-02.md`, `P-02-memory.md`, `P-02-Execution-Prompt.md`) plus the two leftover P-01 package files `P-01.md` / `P-01-Execution-Prompt.md` — **otherwise clean** ✓ |
| `git diff -- src/TopLab.Infrastructure/Persistence/` | **empty** ✓ (Step 0) |
| Build warnings / errors | **0 warnings / 0 errors** |
| TopLab.Domain.Tests | **507 / 507 passed, 0 failed, 0 skipped** |
| TopLab.Application.Tests | **1634 / 1634 passed, 0 failed, 0 skipped** |
| TopLab.Infrastructure.Tests | **284 / 284 passed, 0 failed, 0 skipped** |
| TopLab.Presentation.Tests | **101 / 101 passed, 0 failed, 0 skipped** |
| TopLab.Persistence.Tests | **13 passed, 0 failed, 2 skipped, 15 total** — the 2 skipped are reported as skipped, never as passes |
| **Full suite** | **2539 passed, 0 failed, 2 skipped, 2541 total** |
| `has-pending-model-changes` | `No changes have been made to the model since the last migration.` — **no STOP condition**. (EF rejects `-p:`, so the flag cannot be passed to the tooling; both projects build first.) |
| Migration file count | **29** (14 migrations + 14 Designer.cs + `ApplicationDbContextModelSnapshot.cs`) — must be unchanged at the end of the wave |

**No Linux font failures on this Windows host** — Infrastructure is 284/284. The 22 failures described in C-2 are environmental to that host and are not reproducible here, so RD-4 (no slice) is moot in practice but remains recorded.

---

## Stage 3 — File Analysis confirmations (S1)

| Claim | Confirmed? | Evidence |
|---|---|---|
| `PatientReportPdfExporter.cs` has **no static constructor** | ✅ **exactly** | The file is 41 lines: `public sealed class PatientReportPdfExporter : IPatientReportPdfExporter` at line 14 is immediately followed by `public Task ExportAsync(...)` at line 16. **No static constructor anywhere.** |
| It is the **only** QuestPDF entry point without one | ✅ | `grep -rn "Settings.License" src/ --include=*.cs` (migrations excluded) matches exactly **six writers**: `CustomGroupPdfWriter`, `InvoicePdfWriter`, `PriceListPdfWriter`, `ReceiptPdfWriter`, `ReportPdfWriter`, `WorkSheetPdfWriter`. The exporter was the seventh QuestPDF entry point and had none. |
| Run alone it throws the licence error | ✅ **reproduced** | `dotnet test … --filter PatientReportPdfExporterTests` before the fix → `Failed: 1, Passed: 2` with `System.Exception : Please configure the QuestPDF license by setting 'QuestPDF.Settings.License' at application startup.` |
| The fix is two lines in one file, nothing else | ✅ | One static constructor; the three `using` lines the six writers already have were also required — see the note below. |

**A detail the plan did not state: the two "two lines" need three `using` directives.** `PatientReportPdfExporter.cs` imported only `QuestPDF.Fluent`, whereas the six writers also import `QuestPDF` (for `Settings`) and `QuestPDF.Infrastructure` (for `LicenseType`). Without those the fix does not compile — `Settings` and `LicenseType` were unresolved, producing 3 errors. This is still "one production file, two behaviour lines" and matches the plan's intent, but the plan's count of changed lines was incomplete.

---

**Measure it yourself. On a Linux host, 22 Infrastructure printing tests fail for a missing `Arial` font** — environmental, and **not yours to fix** (RD-4). Record your real numbers; a skipped test is **skipped**, never a pass.

---

## Slice Validation Gates (from plan)

| Slice | Gate | Key checks |
|---|---|---|
| 1 | VG-01 | `PatientReportPdfExporterTests` pass **in isolation**; no new Infrastructure failures; zero drift |
| 2 | VG-02 | All three checkboxes tri-state; the `:96` column survives; the age field is clearable |
| 3 | VG-03 | A throwing handler surfaces `ErrorMessage`; a burst shows the last filter; the six other `_ =` sites untouched |
| 4 | VG-04 | `PrintBarcodeCommand` and its handler **unmodified**; three reprints leave `LabId` unchanged; layering green; zero drift |

---

## Slice Index

| # | Slice Title | **Migration** | Status | Gate |
|---|---|---|---|---|
| 1 | QuestPDF licence in `PatientReportPdfExporter` (D-1) | **NONE** | ✅ `e041def` | VG-01 PASS |
| 2 | Tri-state checkboxes + clearable age field (D-2, D-4) | **NONE** | ✅ `8e11bf8` | VG-02 PASS |
| 3 | Sequenced, error-surfacing filter reload (D-3) | **NONE** | ✅ `777b08b` | VG-03 PASS |
| 4 | Independent barcode reprint from search (A-12) | **NONE** | ⬜ | VG-04 |

---

## Per-slice 10-stage checklists

### Slice 1 — QuestPDF licence (D-1)
**Gate:** VG-01. **Status:** ✅ COMPLETE — commit `see Execution Log`
- [x] 1. Pre-Execution Verification — build 0/0; tests ≥ measured baseline; HEAD pinned exactly; `git diff -- src/TopLab.Infrastructure/Persistence/` empty
- [x] 2. Deep Understanding — P-02 §4; C-1
- [x] 3. File Analysis — `PatientReportPdfExporter.cs` opened; **no static constructor confirmed**; `WorkSheetPdfWriter`'s static ctor read as the template; the six-writer count re-derived
- [x] 4. Planning — recorded below
- [x] 5. Execution — the static constructor **only**; no behaviour, signature or structural change; the other six writers untouched
- [x] 6. Post-Execution Verification — build 0/0; **`PatientReportPdfExporterTests` run alone ⇒ 3/3 passed**
- [x] 7. Validation Gate — VG-01 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-02] Slice 1/4: …`

#### Stage 4 — S1 planning note

**The defect was reproduced before it was fixed.** That ordering is the whole point of this slice, so both runs are recorded:

```
# BEFORE the fix — the negative proof
dotnet test tests/TopLab.Infrastructure.Tests --filter "FullyQualifiedName~PatientReportPdfExporterTests"
→ Failed!  - Failed: 1, Passed: 2, Total: 3
   System.Exception : Please configure the QuestPDF license by setting
   'QuestPDF.Settings.License' at application startup.
```

The plan is right that a full-suite run would **not** have proved anything: in a full run some other writer's static constructor has usually already set the process-global licence, which masks the fault. The isolated run is the only honest test of this defect.

**The change is one static constructor in one file:**

```csharp
static PatientReportPdfExporter()
{
    Settings.License = LicenseType.Community;
    Settings.UseSystemFonts = true;
}
```

Exactly the two lines `WorkSheetPdfWriter` uses, for the reason its XML comment records. `Settings.UseSystemFonts = true` is included because the exporter renders Arabic through the shared `ReportDocument` path, and without it QuestPDF would fall back to its embedded Lato and lose Arabic shaping — the same reason the six writers set it.

**Three `using` directives were also required** (`QuestPDF`, `QuestPDF.Infrastructure`, and `QuestPDF.Fluent` was already present). The file previously had only `QuestPDF.Fluent`, so `Settings` and `LicenseType` were unresolved. Recorded above as a plan inaccuracy; it did not change the slice's scope, which remains one production file.

**What was deliberately NOT done:** no shared licence helper, no base class, no refactor of the other six writers, no change to `IPatientReportPdfExporter` or `ExportAsync`'s signature, and **no new test file** — `PatientReportPdfExporterTests` already covered the behaviour and needed no extension, so the fix is proven by an existing test that was previously failing.

---

## VG-01 — evidence (Slice 1)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 errors / 0 warnings | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)` |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1634/1634** (Δ0) · Infrastructure **284/284** (Δ0) · Presentation **101/101** (Δ0) · Persistence **13 / 2 skipped** (Δ0). Full suite **2539 passed, 0 failed, 2 skipped, 2541 total** (Δ**0**) — no new test was needed, which is the expected result for a two-line fix |
| 3 | **`PatientReportPdfExporterTests` run alone ⇒ 0 failed** | ✅ | **`Passed! - Failed: 0, Passed: 3, Skipped: 0, Total: 3`** — and the same filter gave `Failed: 1, Passed: 2` **before** the change. Negative→positive proven in isolation |
| 4 | Full Infrastructure suite: no **new** failures vs baseline | ✅ | 284/284 — identical to the measured baseline |
| 5 | `grep -c "Settings.License"` on the exporter ≥ 1 | ✅ | **1** |
| 6 | **No migration** — `git diff -- src/TopLab.Infrastructure/Persistence/` | ✅ | **empty** |
| 7 | **No package** — `git diff -- Directory.Packages.props '*.csproj'` | ✅ | **empty**; no `.csproj` touched |
| 8 | The other six writers untouched | ✅ | `git status --porcelain` lists exactly one modified source file |

### Slice 2 — Tri-state checkboxes + clearable age field (D-2, D-4)
**Gate:** VG-02. **Status:** ✅ COMPLETE — commit `see Execution Log`

**Stage 3 — file analysis confirmations:**

| Claim | Confirmed? | Evidence |
|---|---|---|
| `ResultsWorklistView.xaml:11` `HasResult` two-state | ✅ **exactly** | `<CheckBox IsChecked="{Binding HasResult, UpdateSourceTrigger=PropertyChanged}" Margin="0,0,8,0" />` — **no** `IsThreeState`. |
| `:16` `IsReviewed` two-state | ✅ **exactly** | `<CheckBox IsChecked="{Binding IsReviewed, UpdateSourceTrigger=PropertyChanged}" Margin="0,0,8,0" />` — **no** `IsThreeState`. |
| `:25` `IsPrinted` `IsThreeState="True"` | ✅ **exactly** | `<CheckBox IsThreeState="True"` at line 25, bound to `IsPrinted`. The correct idiom already existed. |
| `:96` the `مُراجعة` grid column | ✅ | `<DataGridTextColumn Header="مُراجعة" Binding="{Binding IsReviewed}" Width="80" />` — **display-only, untouched.** |
| `PatientSearchView.xaml` binds `AgeFrom`/`AgeTo` as `TextBox` → `int?` | ✅ **exactly** | `Text="{Binding AgeFrom, UpdateSourceTrigger=LostFocus}"` at line 47 and the same for `AgeTo` at line 49; the ViewModel declared `public int? AgeFrom` / `public int? AgeTo`. **D-4 confirmed as a real defect.** |

- [x] 1. Pre-Execution Verification — HEAD = S1 commit `e041def`; build 0/0; tests ≥ baseline; persistence diff empty
- [x] 2. Deep Understanding — P-02 §5; C-3, C-4, RD-2
- [x] 3. File Analysis — both views and the ViewModel opened at the cited lines; all five claims confirmed
- [x] 4. Planning — recorded below
- [x] 5. Execution — `IsThreeState="True"` on `HasResult` **and** `IsReviewed` (RD-2: both); age properties `string`-backed with a public `ParseAgeBound`; empty and unparseable map to `null`; `:96` preserved
- [x] 6. Post-Execution Verification — build 0/0; Presentation 118/118
- [x] 7. Validation Gate — VG-02 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist + UI Texts Register
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-02] Slice 2/4: …`

#### Stage 4 — S2 planning note

**D-2 — two attributes, and RD-2 is why it is two and not one.** `IsThreeState="True"` was added to the `HasResult` box **and** the `IsReviewed` box, so all three status boxes now behave identically. RD-2 settles this: leaving one tri-state and two two-state boxes in the same toolbar row would be indefensible, and the pre-existing `HasResult` fix is a one-attribute change with no migration.

**D-4 — the type change *is* the fix.** A `TextBox` bound to `int?` cannot convert `""`, so WPF leaves the source value untouched and the field is unclearable by typing. Changing the property to `string?` makes the binding lossless in both directions; the parse happens once, in `SearchAsync`:

```csharp
public static int? ParseAgeBound(string? text)
{
    if (string.IsNullOrWhiteSpace(text)) return null;
    return int.TryParse(text.Trim(), out var value) ? value : null;
}
```

It is `public static` so it is directly testable — the WPF conversion failure itself cannot be exercised from xUnit, so the **parsing contract** and the resulting **query shape** are what the tests assert. Fourteen `[Theory]` cases cover null, empty, whitespace, four unparseable inputs and four valid ones.

**Unparseable text widens rather than throws.** VG-02 requires "a non-numeric entry maps to `null` and does not throw", so a stray character is treated as *no bound* instead of raising a validation error the user cannot see. Recorded as a deliberate choice: the alternative (a visible message) would be a new user-facing string and a behaviour the plan does not specify.

**The band stays inert when both boxes are empty.** `ageFrom is null && ageTo is null` ⇒ `Age` is still sent as `null`, so an empty pair of boxes does not narrow by unit. That SD-5 property from Wave 1 survives the type change.

**One consequence worth naming:** three Wave 1 tests assigned `vm.AgeFrom = 6` and were updated to `vm.AgeFrom = "6"`. This is a **source-level** signature change to a public bindable property, not a behaviour regression — the forwarded query values are unchanged and all 13 pre-existing `PatientSearchFiltersTests` still pass, including `PatientSearch_ForwardsEachFilterIntoTheQuery`, which asserts `Age.From == 6`.

**No new user-facing string was created** in this slice — the fix removes the need for a clear affordance rather than adding one, so the register's S2 row stays empty and is marked as such below.

---

## VG-02 — evidence (Slice 2)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)`. Three intermediate CS0029 errors (Wave 1 tests assigning `int` to the now-`string` properties) were resolved by updating those three call sites. |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1634/1634** (Δ0) · Infrastructure **284/284** (Δ0) · Presentation **118/118** (Δ**+17**) · Persistence **13 / 2 skipped** (Δ0). Full suite **2556 passed, 0 failed, 2 skipped, 2558 total** (Δ**+17**) |
| 3 | All three worklist CheckBoxes carry `IsThreeState="True"` | ✅ | `ResultsWorklist_AllThreeCheckBoxesCarryIsThreeState` checks each of `HasResult`, `IsReviewed`, `IsPrinted` has the flag **preceding its own binding** with no intervening `CheckBox`/`TextBlock` element; `ResultsWorklist_HasExactlyThreeTriStateCheckBoxes` asserts the count is exactly 3 |
| 4 | `ResultsWorklistView.xaml:96` grid column still present | ✅ | `ResultsWorklist_ReviewedGridColumn_StillPresent` — both the `مُراجعة` and `مطبوعة` `DataGridTextColumn` elements intact |
| 5 | Clearing the age field sets the source back to `null` and widens | ✅ | `ParseAgeBound_EmptyMeansNoBound` (null / `""` / `"   "`), `AgeFields_CanBeCleared` (set `"12"` → `string.Empty` → `null`, each parse to no bound), `AgeFromAndAgeTo_AreStringBacked` |
| 6 | A non-numeric entry maps to `null` and does not throw | ✅ | `ParseAgeBound_UnparseableMeansNoBound_AndDoesNotThrow` — `"abc"`, `"12abc"`, `"-"`, `"!@#"` all → `null`, no exception |
| 7 | `ClearFiltersCommand` still clears every filter in one reload | ✅ | `PatientSearch_ClearFilters_WidensBackToTheUnfilteredState` (Wave 1) still green — it sets `AgeFrom = null`, which is valid against the new `string?` type, and asserts the unit resets to `Year` with paging intact |
| 8 | `EveryView_IsRightToLeft` green | ✅ | `EveryView_IsRightToLeft` green in the 118/118 run; also asserted directly by `ResultsWorklist_ViewStaysRightToLeft` |
| 9 | **No migration** — `git diff -- src/TopLab.Infrastructure/Persistence/` | ✅ | **empty** |
| 10 | **No package** — `git diff -- Directory.Packages.props '*.csproj'` | ✅ | **empty** |
| 11 | No new backend error message (SD-7) | ✅ | no string added; the unparseable case is silent by design and documented above |

### Slice 3 — Sequenced filter reload (D-3)
**Gate:** VG-03. **Status:** ✅ COMPLETE — commit `see Execution Log`

**Stage 3 — file analysis confirmations:**

| Claim | Confirmed? | Evidence |
|---|---|---|
| `OnFilterChanged()` does `_ = ReloadFromFirstPageAsync()` | ✅ **exactly** | `private void OnFilterChanged()` → `_ = ReloadFromFirstPageAsync();`, and `ReloadFromFirstPageAsync` called `SearchAsync(CancellationToken.None)`. The task was discarded. |
| `SearchAsync` has **no `catch`** | ✅ **exactly** | `try { … } finally { IsBusy = false; }` — a `finally` only, no `catch` clause anywhere in the method. |
| `PatientSearchFiltersTests` asserts `Age.From == 6` | ✅ | that assertion survived S2's type change, which is how I know the forwarded values are unchanged |

- [x] 1. Pre-Execution Verification — HEAD = S2 commit `8e11bf8`; build 0/0; tests ≥ baseline; persistence diff empty
- [x] 2. Deep Understanding — P-02 §6; C-5, C-6, RD-3
- [x] 3. File Analysis — `OnFilterChanged`, `ReloadFromFirstPageAsync` and the whole of `SearchAsync` read; the six out-of-scope sites enumerated
- [x] 4. Planning — recorded below, including the sequencing choice
- [x] 5. Execution — generation counter **and** `CancellationTokenSource`; `try`/`catch` surfacing `ErrorMessage` with the list left intact; `OperationCanceledException` swallowed as cancellation; the six other `_ =` sites untouched
- [x] 6. Post-Execution Verification — build 0/0; Presentation 125/125
- [x] 7. Validation Gate — VG-03 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist
- [x] 9. Memory Status Update — Slice Index + Execution Log
- [x] 10. Git Commit — local, `[P-02] Slice 3/4: …`

#### Stage 4 — S3 planning note

**The sequencing choice — recorded as the plan requires: a generation counter `AND` a `CancellationTokenSource`, not the counter alone.**

| Mechanism | What it does | Why it is not enough alone |
|---|---|---|
| `CancellationTokenSource` | cancels the superseded query early, where the provider honours the token | a query that has **already completed** ignores the token, so a stale result can still land after the newer one |
| `_filterReloadGeneration` (`long`, `Interlocked.Increment`) | `SearchAsync` refuses to write to the view unless its generation is still the current one | correct but wasteful — it would let the stale query run to completion |

Keeping both closes the window: the token saves work, the counter guarantees correctness. The guard is a single early-return in `SearchAsync`:

```csharp
if (generation.HasValue && generation.Value != Volatile.Read(ref _filterReloadGeneration))
{
    return;   // superseded: never write to the view
}
```

**A note on honesty of the guarantee.** `OnFilterChanged` still calls `_ = RunFilterReloadAsync()`. It cannot await — it is a property setter invoked synchronously from a binding. What changed is that **every failure is now caught inside `RunFilterReloadAsync` itself**, so the discarded task can no longer carry an unobserved exception, and the generation check makes the completion order irrelevant. The plan's "never discard a task whose failure can be lost" is satisfied at the point where the failure occurs, not by removing the discard.

**Error handling shape.** `catch (OperationCanceledException)` first — cancellation is not an error and must not surface. Then `catch (Exception ex)` → `ErrorMessage = _presenter.Present(Error.Unexpected(ex.Message))`, deliberately leaving `Items` untouched so the user keeps the last good result set instead of an empty grid.

**A finding worth recording: the presenter does not leak exception text.** My first test asserted `ErrorMessage` contains `"database is unreachable"`; it failed with `"حدث خطأ غير متوقع. حاول مرة أخرى."`. `ResultErrorPresenter` maps `Error.Unexpected` to that generic Arabic string by design. The test now asserts the generic message **and** explicitly asserts the exception text is *absent* — which is the better test, because it pins the non-leaking behaviour as well as the surfacing.

**The sequencing tests were proven to fail without the fix.** Rather than trusting that they exercise the guard, I temporarily removed the generation check and re-ran: **`Failed: 2, Passed: 5`** — `BurstOfChanges_DisplaysTheLastRequestedFilterSet` and `BurstOfChanges_SupersededSearchDoesNotOverwriteALaterOne` both failed. The file was then restored and the build and all 7 tests re-verified green. Recorded because a test that passes both with and without the fix proves nothing.

**RD-3 respected.** Only `PatientSearchViewModel.cs` and the new test file changed. `git diff --name-only` over the six out-of-scope files returns **nothing**, and the slice's whole production footprint is one ViewModel.

---

## VG-03 — evidence (Slice 3)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)`. One intermediate CS0103 (`Error` unresolved — needed `using TopLab.Application.Common.Results;`) and four CS4016 in the new test file were resolved. |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1634/1634** (Δ0) · Infrastructure **284/284** (Δ0) · Presentation **125/125** (Δ**+7**) · Persistence **13 / 2 skipped** (Δ0). Full suite **2563 passed, 0 failed, 2 skipped, 2565 total** (Δ**+7**) |
| 3 | A handler that throws ⇒ `ErrorMessage` populated, previous list survives | ✅ | `ThrowingHandler_PopulatesErrorMessage` (asserts the exact Arabic message and that the exception text is **not** leaked) and `ThrowingHandler_PreviousListSurvives` (3 male rows still shown, `IsBusy` cleared) |
| 4 | A rapid burst displays the **last** requested filter set | ✅ | `BurstOfChanges_DisplaysTheLastRequestedFilterSet`, `BurstOfChanges_SupersededSearchDoesNotOverwriteALaterOne` (a stale 2-row set must not clobber a current 3-row set), `RapidBurst_AllReloadsAreIssuedAndTheLastOneWins`. **Proved to fail** when the generation guard is removed (`Failed: 2, Passed: 5`). |
| 5 | `OperationCanceledException` does **not** surface as an error | ✅ | `OperationCanceledException_DoesNotSurfaceAsAnError` — `ErrorMessage` empty and `IsBusy` false after a cancelled reload followed by a successful one |
| 6 | S2's age-field tests and `PatientSearch_FilterChangeResetsToFirstPage` still green | ✅ | all 17 `AgeFieldClearingTests` and all 13 `PatientSearchFiltersTests` pass in the 125/125 run |
| 7 | The six pre-existing `_ =` sites untouched | ✅ | `git diff --name-only` over `AccountsHubViewModel`, `CultureAttachmentViewModel`, `PriceListsViewModel`, `CustomGroupsViewModel`, `PatientsHubViewModel` → **no output**. The slice's production footprint is exactly one file. |
| 8 | **No migration** — `git diff -- src/TopLab.Infrastructure/Persistence/` | ✅ | **empty** |
| 9 | **No package** — `git diff -- Directory.Packages.props '*.csproj'` | ✅ | **empty** |
| 10 | No new backend error message (SD-7) | ✅ | the surfaced message is the presenter's existing `Error.Unexpected` text; no new string was added |

### Slice 4 — Independent barcode reprint (A-12)
**Gate:** VG-04. **Status:** ✅ COMPLETE — commit `see Execution Log`

**Stage 3 — file analysis confirmations:**

| Claim | Confirmed? | Evidence |
|---|---|---|
| **C-7** `PrintBarcodeCommand(int PatientId)` exists, implemented, tested | ✅ **exactly** | `record PrintBarcodeCommand(int PatientId) : IRequest<Result>, IAuthorizedRequest` gated on `PatientRegistrationAccessPolicy.AddEditPatient`; handler loads the patient, returns `Error.NotFound("المريض غير موجود.")` for missing/deleted, reads `GetSystemSettingsQuery`, then picks `patient.LabId.Value` or `patient.Id.Value` by `PrintLabIdInsteadOfPatientId` and calls `_barcodeService.PrintBarcodeAsync`. Validator rules `PatientId > 0`. |
| Its only production caller is `PatientEditorViewModel:1322` | ✅ **exactly** | `grep -rn "PrintBarcodeCommand" src/` outside the command's own folder returns exactly three hits, all in `PatientEditorViewModel.cs`: line 124 (command construction), 399 (property), **1322** (the dispatch). |
| `PatientSearchViewModel` has **no** barcode command | ✅ | confirmed — the only patient action was `OpenPatientCommand`. |
| `SelectedItem` already carries the id | ✅ | `PatientSearchHitDto(int PatientId, string? LabId, …)`. No new DTO needed. |
| `IBarcodeService.PrintBarcodeAsync(string value, ct)` | ✅ | the port has exactly one method; **not widened** (B-4 is Wave 3). |
| `GetNextLabId` exists and is untouched | ✅ | `GetNextLabIdQuery.cs` / `GetNextLabIdQueryHandler.cs` — neither file appears in any S4 diff. |

- [x] 1. Pre-Execution Verification — HEAD = S3 commit `777b08b`; build 0/0; tests ≥ baseline; persistence diff empty
- [x] 2. Deep Understanding — P-02 §7; C-7, C-8, SD-6, SD-8
- [x] 3. File Analysis — the command, handler, validator, its tests, `PatientEditorViewModel:1322`, `IBarcodeService` and `PatientSearchHitDto` all read
- [x] 4. Planning — recorded below
- [x] 5. Execution — one command + one button added to `PatientSearchViewModel`; the **existing** `PrintBarcodeCommand` dispatched; handler, validator, command, port and `GetNextLabId` all untouched
- [x] 6. Post-Execution Verification — build 0/0; Application 1638/1638; Presentation 133/133
- [x] 7. Validation Gate — VG-04 PASS (item by item, below)
- [x] 8. Documentation Update — this checklist + UI Texts Register
- [x] 9. Memory Status Update — Slice Index + Execution Log + Wave DoD
- [x] 10. Git Commit — local, `[P-02] Slice 4/4: …`

#### Stage 4 — S4 planning note

**Only an entry point was added.** The production footprint is one ViewModel property, one command, one private method and one button:

```csharp
ReprintBarcodeCommand = new AsyncRelayCommand(
    async (_, ct) => await ReprintBarcodeAsync(ct),
    _ => SelectedItem is not null);   // no half-wired state
```

and the dispatch itself is a single line — `_mediator.Send(new PrintBarcodeCommand(patientId.Value))`.

**SD-6 is satisfied structurally, not by discipline.** The handler only ever *reads* `patient.LabId` (or `patient.Id`) and passes it to the barcode port; nothing in the path can mint an identifier because no minting code is reachable from it. Two tests pin that:
- `PatientSearchViewModel_SourceNeverMentionsAnIdentifierMintingQuery` — scans the ViewModel's **code** (comments stripped, so the XML comment explaining that `GetNextLabId` is *not* called does not trip it) for `GetNextLabId`, `SetLabId`, `AssignLabId`.
- `PrintBarcodeReprintDoesNotMintIdentifierTests` (Application project) — drives the **real** handler three times against a capturing barcode port and asserts `LabId` is unchanged and the same value printed each time.

**Why the SD-6 proof is split across two test projects.** I first wrote the three-reprint test inside `Presentation.Tests`, which cannot compile: that project has no reference to `TopLab.Application.Tests`, so `FakeApplicationDbContext`, `FakeSender` and the settings DTO are unavailable. Rather than add a project reference or duplicate four hand-rolled fakes, the handler-level property test went to the Application project beside the fakes it needs, and the entry-point behaviour stayed in Presentation. Each test now sits where its dependencies already exist.

**A decision recorded — `StatusMessage` was added to `PatientSearchViewModel`.** The ViewModel had only `ErrorMessage`; there was nowhere to report successful completion without misusing the error channel. `StatusMessage` follows the existing house shape (`PriceListsViewModel` and `CustomGroupsViewModel` both have one): a public read-only property over a private field, set through `SetProperty`.

**The control is presented as the lost-card reprint, per the reference.** Label «إعادة طباعة الباركود» — "reprint the barcode" — not a generic "print". REF3 p.16 §14 is about a patient who lost his card; the button sits beside the existing «رجوع» in the actions row and is disabled until a row is selected.

**Deliberately NOT done:** no new command, no handler or validator edit, no `IBarcodeService` widening, no symbology setting (B-4 is Wave 3), no per-container label work (A-3 is Wave 3), and `GetNextLabId` untouched.

---

## VG-04 — evidence (Slice 4)

| # | Item | Result | Evidence |
|---|---|---|---|
| 1 | Build 0 warnings / 0 errors | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)`. Intermediate errors resolved: CS0103 (`Error` unqualified — `using TopLab.Application.Common.Results;` added), CS0246/CS0234 (`Presentation.Tests` cannot reference `Application.Tests` — split the SD-6 test across projects as described), CS1061 (`sender.ReprintSent` is already a `Task`). |
| 2 | No test count below baseline | ✅ | Domain **507/507** (Δ0) · Application **1638/1638** (Δ**+4**) · Infrastructure **284/284** (Δ0) · Presentation **133/133** (Δ**+8**) · Persistence **13 / 2 skipped** (Δ0). Full suite **2575 passed, 0 failed, 2 skipped, 2577 total** (Δ**+12**) |
| 3 | **`PrintBarcodeCommandHandler` unmodified** | ✅ | `git diff --name-only -- src/TopLab.Application/Features/PatientRegistration/Commands/PrintBarcode/` → **no output**. The folder holds exactly the same three files it did before S4 (Command, Handler, Validator), asserted by `ReprintCommand_UsesTheExistingCommandType_NotANewOne`. |
| 4 | **Reprinting three times leaves `Patient.LabId` unchanged** | ✅ | `PrintBarcodeReprintDoesNotMintIdentifierTests.ReprintingThreeTimes_LeavesLabIdUnchanged` — three real handler invocations, `LabId` still `"LAB-7"`, and `barcodes.Values == ["LAB-7","LAB-7","LAB-7"]`. Also `Reprinting_PrintsThePatientId_NotANewNumber_WhenLabIdPrintingIsOff` (`["9","9"]`), `Reprinting_DoesNotChangeThePatientsIdentityFields`, and `Reprinting_SoftDeletedPatient_IsRefusedWithoutPrinting`. |
| 5 | The command is disabled with no patient selected | ✅ | `ReprintCommand_IsDisabledWithNoPatientSelected` and `ReprintCommand_IsEnabledWithAPatientSelected_AndDisabledAgainWhenCleared` |
| 6 | Goes through `IBarcodeService`, never directly to `BarcodeService` (layering) | ✅ | the ViewModel references **no** Infrastructure type at all — it sends a MediatR command. `PresentationLayering_NoInfrastructureReferenceOutsideAppXaml` → **2/2 green**. `IBarcodeService` was not widened (still one method). |
| 7 | `PatientSearch_ViewStaysRightToLeft` and `MainWindow_Xaml_Bindings_ResolveToPublicProperties` green | ✅ | `EveryView_IsRightToLeft` green in the 133/133 run; structural filter → **4/4**. `ReprintCommand_UsesTheExistingCommandType_NotANewOne` uses reflection on the ViewModel, so a non-public command would fail it. |
| 8 | **No migration** — `git diff -- src/TopLab.Infrastructure/Persistence/` | ✅ | **empty** |
| 9 | **No package** — `git diff -- Directory.Packages.props '*.csproj'` | ✅ | **empty** |
| 10 | The existing `PrintBarcodeCommand` tests still green | ✅ | `--filter PrintBarcode` → `Passed! 13/13` (9 pre-existing + 4 new). The pre-existing `PrintBarcodeCommandHandlerTests` were not modified. |
| 11 | No new backend error message (SD-7) | ✅ | the surfaced failure is the existing presenter mapping; only the UI status string is new (registered below) |

---

## Wave DoD (P-02 §11) — final verification

| # | DoD item | Result | Evidence |
|---|---|---|---|
| 1 | S1–S4 complete, every `VG-nn` green item by item | ✅ | 8 + 11 + 10 + 11 = **40 gate items**, all evidenced above |
| 2 | **Zero migrations**, migration file count unchanged | ✅ | `git diff 51095e5 HEAD -- src/TopLab.Infrastructure/Persistence/` **empty**; folder still **29** files (14 migrations + 14 Designers + snapshot), identical to the G0 count |
| 3 | **Zero columns, zero `HasData` rows, zero permission codes** | ✅ | no entity, no EF configuration and no `HasData` touched in any slice; `has-pending-model-changes` reported *no changes* at G0 and no slice created a migration |
| 4 | **Zero package changes** | ✅ | `git diff 51095e5 HEAD -- Directory.Packages.props` **empty**; `git diff 51095e5 HEAD -- '*.csproj'` **empty** |
| 5 | Build **0 errors / 0 warnings** | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)` on every slice |
| 6 | Full suite at or above the measured baseline | ✅ | baseline **2539 passed / 2 skipped (2541)** → final **2575 passed / 2 skipped (2577)**. **Δ +36 passed, +0 failed, +0 new skips.** No project ever fell below baseline. |
| 7 | `PatientReportPdfExporterTests` pass **in isolation** | ✅ | `Passed! 3/3` under `--filter`, and the same filter gave `Failed: 1, Passed: 2` before S1 — the negative/positive proof that D-1 is fixed |
| 8 | All three worklist checkboxes tri-state; the age field clearable; filter reloads sequenced and error-surfacing | ✅ | 3× `IsThreeState="True"` pinned; `ParseAgeBound` maps empty and unparseable text to `null`; generation counter + `CancellationTokenSource` with the guard proven necessary (`Failed: 2` without it) |
| 9 | A barcode reprint leaves `LabId` unchanged and creates no new command | ✅ | `ReprintingThreeTimes_LeavesLabIdUnchanged`; `git diff` over the `PrintBarcode` folder is empty; exactly three files still exist there |
| 10 | `PresentationStructuralTests` and `PresentationLayeringTests` green | ✅ | structural → **4/4**; layering → **2/2** |
| 11 | **Four local commits only** — the owner pushes | ✅ | `git log --oneline 51095e5..HEAD` → 4 commits. **Never pushed; no branch, amend, rebase, reset, stash, clean or tag; no `git add -A` / `git add .`** |

**Defects delivered — all four:** D-1 QuestPDF licence ✅ · D-2 tri-state checkboxes ✅ · D-3 sequenced, error-surfacing reload ✅ · D-4 clearable age field ✅. Plus **A-12**, the independent barcode reprint.

---

## Wave 3 Register (described in the plan §8 — NOT packaged for execution)

| # | Function | Migration required | Depends on |
|---|---|---|---|
| EN-1 | Three-code patient identity, 13-digit, branch fixed | New patient-code columns | — |
| B-4 | Barcode symbology catalogue, configurable 1-D | New symbology catalogue table | — |
| A-3 | Per-test tube & container labels, saved position | New label layout / position storage | EN-1, B-4 |
| A-18 | Barcode on receipt and envelope, optional toggle | New `ReceiptSettings` **and** `EnvelopeSettings` booleans | EN-1 |
| B-6 | Turnaround unit → days | `Test.CompletionDurationMinutes` → days, with data conversion | — |
| A-9 | "Unfinished" state + filter on every work-sheet output | New `PatientTest.IsFinished` column | — |
| B-2 | Antibiotic commercial name | New `Antibiotics.CommercialName` column | — |
| A-17 | Unit on the normal-range band | New `Unit` on `ReferenceRange` **and** `AnalyteReferenceRangeBand` | — |
| A-4 | Per-result range override + global propagation | New override columns on `PatientTest`, `ProfileResultItem` | A-17 |
| A-14 | Colour-code abnormal results | New report-style storage | A-17, A-4 |
| A-13 | Estimated pickup date | None of its own; blocked by B-6's column | **B-6** |

**Deferred from both waves:** the contract (lab-to-lab) case follow-up (RD-8).

---

## Migration Register

| Slice | Migration name | Tables/columns | Backup | Review agent |
|---|---|---|---|---|
| — | **NONE. This wave creates zero migrations, zero columns, zero `HasData` rows.** | — | — | — |

---

## Created UI Texts Register (append at execution time)

| Slice | Screen | Text | Kind |
|---|---|---|---|
| 2 | PatientSearchView | **none created** — the D-4 fix makes the existing box clearable, so no clear-affordance label was needed. The planned *TBD-AR* row is therefore **not used**. | — |
| 4 | PatientSearchView | «إعادة طباعة الباركود» | Reprint-barcode button label (A-12) — created once; presented as the lost-card reprint of REF3 p.16 §14 |
| 4 | PatientSearchView | «تم إرسال الباركود للطباعة.» | Success status after a reprint (A-12) — created once; required because the ViewModel had no `StatusMessage` before |

Any label not listed here is marked **`TBD-AR`** and **not invented** (SD-7).

---

## Execution Log

| Slice | Commit | Notes |
|---|---|---|
| 1 | `e041def` | **D-1 fixed.** Negative proof first: isolated run gave `Failed: 1, Passed: 2` with the QuestPDF licence error; after the static constructor, the same filter gave `Passed! 3/3`. One production file; the other six writers untouched; **no new test needed** (the existing test was already failing). Build 0/0; all five projects Δ0; persistence and package diffs empty. All 8 VG-01 items pass. |
| 2 | `8e11bf8` | **D-2 and D-4 fixed.** `IsThreeState="True"` added to **both** two-state boxes (`HasResult` and `IsReviewed`, per RD-2) so all three match; the `:96` `مُراجعة` grid column untouched. `AgeFrom`/`AgeTo` are now `string?` with a public `ParseAgeBound` mapping empty **and** unparseable text to `null`; band still inert when both boxes are empty. Three Wave 1 tests updated `AgeFrom = 6` → `"6"` (source-level type change, behaviour unchanged — `Age.From == 6` still asserted). Build 0/0; Presentation Δ**+17**; full suite **2556 passed / 2 skipped**. All 11 VG-02 items pass. No new user-facing string needed. |
| 3 | `777b08b` | **D-3 fixed.** Sequencing uses a **generation counter *and* a `CancellationTokenSource`** (recorded choice): the token saves work, the counter guarantees correctness — `SearchAsync` refuses to write when its generation is stale. Failures caught inside the reload itself, so the discarded task can no longer lose one; `OperationCanceledException` swallowed as cancellation; `Items` left intact on error. **Sequencing tests proven to fail without the guard** (`Failed: 2, Passed: 5` when the generation check was temporarily removed, then restored). Production footprint: **one** ViewModel — the six out-of-scope `_ =` sites verified unmodified. Build 0/0; Presentation Δ**+7**; full suite **2563 passed / 2 skipped**. All 10 VG-03 items pass. |
| 4 | `cb002ef` | **A-12 delivered.** Entry point only: `ReprintBarcodeCommand` on `PatientSearchViewModel`, disabled with no selection, dispatching the **existing** `PrintBarcodeCommand`. `git diff` over the `PrintBarcode` folder is **empty** (C-7) and `IBarcodeService` was not widened (SD-8). SD-6 proven two ways: three real handler invocations leave `LabId == "LAB-7"` and print the same value each time, and the ViewModel's code contains no `GetNextLabId`/`SetLabId`/`AssignLabId`. `StatusMessage` added (the ViewModel had only `ErrorMessage`). The SD-6 test lives in the Application project because `Presentation.Tests` cannot reference `Application.Tests`. Build 0/0; Application Δ+4, Presentation Δ+8; full suite **2575 passed / 2 skipped**. All 11 VG-04 items pass, plus the 11-item wave DoD. |

---

## Current Status

**WAVE P-02 COMPLETE.** All four slices committed locally; every `VG-nn` green item by item; the wave DoD satisfied.

| | Baseline (G0) | Final (S4) | Δ |
|---|---|---|---|
| Domain | 507 / 507 | 507 / 507 | 0 |
| Application | 1634 / 1634 | 1638 / 1638 | **+4** |
| Infrastructure | 284 / 284 | 284 / 284 | 0 |
| Presentation | 101 / 101 | 133 / 133 | **+32** |
| Persistence | 13 passed / 2 skipped | 13 passed / 2 skipped | 0 |
| **Full suite** | **2539 passed, 0 failed, 2 skipped (2541)** | **2575 passed, 0 failed, 2 skipped (2577)** | **+36 passed, +0 failed** |

Build 0 warnings / 0 errors on every slice. **Zero migrations** (folder still 29 files), **zero columns**, **zero `HasData` rows**, **zero packages**, **zero `.csproj` edits**. Four local commits on `main`; never pushed.

All four Wave 1 defects are corrected and A-12 is delivered. Wave 3's eleven functions remain untouched.

---

## Stop Report

(no stop rule triggered)

**Open for the owner — neither blocked the wave:**

1. **`PatientSearchViewModel` gained a `StatusMessage` property** in S4. The ViewModel previously had only `ErrorMessage`, so a successful reprint had nowhere to report itself without misusing the error channel. The new property follows the shape `PriceListsViewModel` and `CustomGroupsViewModel` already use. Recorded as a deliberate addition; **«بانتظار قرار المالك»** if the owner would rather report success through a different channel.
2. **The unparseable age entry is silent.** A non-numeric value in the age box maps to "no bound" (widening the result set) rather than raising a visible message, because VG-02 requires it not to throw and any message would be a new user-facing string the plan does not specify. **«بانتظار قرار المالك»** if the owner prefers an inline validation message.
3. **The 22 Linux font failures were not reproducible on this Windows host** — Infrastructure measured 284/284 at G0. RD-4's "no slice" instruction was therefore not exercised in practice, and remains recorded.
