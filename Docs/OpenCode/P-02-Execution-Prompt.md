# Loop-Engineering Execution Prompt — Workstream P-02 (Parity Wave 2, Revised)

> Convention: standalone prompt per workstream (S-05 / S-06 / S-07 / W-01 / W-02 / P-01 pattern). **Copy everything below the line verbatim to the local executing coding agent.**

---

You are the local executing coding agent for Top-Lab workstream **P-02** (Parity Wave 2, revised), under the loop-engineering protocol used by S-00…S-07, W-01, W-02 and P-01.

**Repository:** work inside the local clone of Top-Lab (`main`).
**Pinned commit:** `51095e597d2a8b9c8e43f594f70e99bb155d803c`.

## Step 0 — Pin the baseline

```
git rev-parse HEAD
git status --porcelain
git diff -- src/TopLab.Infrastructure/Persistence/
```

`git rev-parse HEAD` must print `51095e597d2a8b9c8e43f594f70e99bb155d803c`, or a descendant whose diff is **only** the three P-02 package files under `Docs/OpenCode/`: `P-02.md`, `P-02-memory.md`, `P-02-Execution-Prompt.md`. `git status --porcelain` must otherwise be empty, and the third command must print **nothing**. **If not → STOP and report.**

At the start of every slice, re-run the first two and include them in your report. At the end of every slice, re-run `git status --porcelain` (empty after committing) and `git log --oneline -1`.

## Source documents (read both fully before any code)

1. **`Docs/OpenCode/P-02.md`** — the wave plan: §0 the migration boundary rule and the per-function boundary test, §1 binding decisions SD-1…SD-9, §2 plan-vs-code corrections C-1…C-8, §3 the slice map, §4–§7 the four slices each with a scope file table, an implementation approach and a validation gate `VG-01`…`VG-04`, §8 the **Wave 3 register** (described, not packaged), §9 general gates, §10 baseline, §11 wave DoD.
2. **`Docs/OpenCode/P-02-memory.md`** — the living memory: SD/C tables, confirmed code facts, the recorded decisions RD-1…RD-8, the G0 baseline table, the slice index, per-slice 10-stage checklists, the Wave 3 register, the created-UI-texts register, the execution log and the stop report. **You update this file as you go.**

**No other module file is required.** If a fact is not in these two files and not in the live code, stop and report. Do not go hunting through `Docs/` for audit reports; they are unverified inputs, not sources.

## The migration boundary — the rule that defines this wave

> **A function belongs to Wave 2 if and only if it can be executed, exactly as originally specified, without creating any new migration file and without editing any existing migration file. A function that requires a migration belongs to Wave 3. The boundary is the migration, not the behaviour.**

Applied to the original candidate set, this moved **four of five items** to Wave 3:

| Item | Column needed? | Where it went |
|---|---|---|
| Three-code patient identity (13-digit) | Yes — the three codes are persistent identifiers | **Wave 3** |
| Barcode symbology catalogue | Yes — a catalogue is stored rows | **Wave 3** |
| Per-test tube & container labels | Yes — the reference specifies a **saved** label position | **Wave 3** |
| Barcode on the receipt and envelope | Yes — no barcode member on either settings entity | **Wave 3** |
| Independent barcode reprint | **No** — the command exists; only an entry point is missing | **Wave 2, S4** |

**You are not to argue this boundary, widen it, or re-litigate it.** If you believe a Wave 2 slice needs a migration, that is a **STOP**, not a redesign.

**Never redesign a function to avoid a migration.** An earlier draft of this package kept four migration-requiring functions inside Wave 2 by changing where their state lives — deriving the codes instead of storing them, moving settings to a workstation-local file. That was a scope change, it was withdrawn, and it must not be repeated. If a function needs a migration, it goes to Wave 3.

## Mission

**Four slices.**

**S1–S3 correct the four defects the Wave 1 verification found.** The owner has accepted all four as real.

| Ref | Defect |
|---|---|
| **D-1** | `PatientReportPdfExporter` never sets the QuestPDF licence, so "export patient report to PDF" succeeds or throws depending on what the user did first in the session. A **real production defect**, pre-existing. |
| **D-2** | The «لدي نتيجة» and «مُراجعة» status checkboxes are two-state bound to `bool?`, so "no filter" is unreachable for them. |
| **D-3** | Filter reloads are fire-and-forget and un-sequenced, so a database fault is silently lost and a burst of filter changes can display a stale result set. |
| **D-4** | The age filter's text boxes cannot be cleared by deleting the text; only `مسح الفلاتر` clears them. |

**S4 delivers the independent barcode reprint.** Reference (REF3 p.16 §14): on a **lost patient card**, search the patient and *"طباعة الباركود الخاص به مرة اخري وال يعطى رقم اخر جديد"* — print his barcode again, and **do not issue a new number**.

**The Linux font failures are not yours to fix** and get no slice: 18 of them pre-date Wave 1 and 4 are Wave 1's, all from a missing `Arial` on a Linux host. Your Windows run reports none.

**Not in this wave.** Every Wave 3 function listed in the plan §8 and the memory's Wave 3 Register. Do not build, stub or scaffold any of them.

## Binding decisions — do NOT reopen (P-02.md §1)

- **SD-1** the plan is a hypothesis: live code contradicting it ⇒ **STOP and report**.
- **SD-2** git: one **local** commit per verified slice. **Never push.** No branch, amend, rebase, reset, stash, clean, tag, force-push. **Never `git add -A` / `git add .`** — stage explicit paths only. Never commit on a red build or failed gate.
- **SD-3** **zero migrations.** No new migration class; no edit, delete or rename of any existing migration or `.Designer.cs`; **no** `ApplicationDbContextModelSnapshot.cs` change by hand **or** by tooling; **no** `dotnet ef database update`; **no** `HasData` row; **no** new column. Every slice proves it with `git diff -- src/TopLab.Infrastructure/Persistence/` returning **empty**. A slice that appears to need a migration is a **STOP**, never a licence to improvise.
- **SD-4** **zero packages.** `Directory.Packages.props` and every `.csproj` untouched. Never write `Version=` in a `.csproj`.
- **SD-5** **do not redesign to avoid a migration.** Changing a function's behaviour so it fits this wave is forbidden. Moving it to Wave 3 is the correct action.
- **SD-6** **a reprint must never mint a new identifier.** `GetNextLabId` and `Patient.LabId` are untouched by the reprint path.
- **SD-7** Arabic strings: no new backend error message. New **UI-only** strings are created once and appended to the register. Never invent copyright text, a support address, or a commercial name.
- **SD-8** layering: Presentation may not reference Infrastructure. The reprint goes through the existing `IBarcodeService` port.
- **SD-9** scope ceiling: four slices. Three Wave 1 corrections plus one functional slice.

## Decisions already taken for you (P-02-memory RD-1…RD-8 — binding, do not re-open)

- **RD-1** A-12 is the **patient-card reprint** of REF3 p.16 §14 — search, print his barcode again, no new number. It is **not** the per-container-label print, which sits inside the label window and belongs to A-3 in Wave 3.
- **RD-2** S2 fixes **both** two-state checkboxes — Wave 1's `IsReviewed` **and** the pre-existing `HasResult`.
- **RD-3** S3 fixes the filter path only. The six pre-existing `_ = SomethingAsync()` sites elsewhere are **not** refactored.
- **RD-4** The Linux font failures get **no slice**.
- **RD-5** A-3 moved to Wave 3: the reference specifies a **saved** label position, so keeping it here would mean dropping the save or moving state to a file. Both are forbidden.
- **RD-6** A-18 moved to Wave 3: no barcode member on `ReceiptSettings` or `EnvelopeSettings`, so the toggle is a new column on both.
- **RD-7** A-13 moved to Wave 3 even though it needs no column of its own: it reads B-6's column and cannot execute before B-6.
- **RD-8** The contract (lab-to-lab) case follow-up stays deferred and is in **neither** wave.

## Plan-vs-code corrections you must honour (P-02.md §2)

**Use them, then confirm them at Stage 3.** Among the ones that change what you build:

- **C-1** `PatientReportPdfExporter` is the only QuestPDF entry point with **no static constructor**. Run alone it throws *"Please configure the QuestPDF license"*. S1 adds the static constructor — **two lines, one file, nothing else changes.**
- **C-2** the 22 Linux Infrastructure failures are **environmental**, not a Wave 1 regression. **Do not fix the writers.**
- **C-3** `HasResult` (line 11) and `IsReviewed` (line 16) are two-state; `IsPrinted` (line 25) is `IsThreeState="True"`. S2 makes all three tri-state — **including the pre-existing one.**
- **C-4** the age fields bind `TextBox` to `int?`; WPF cannot convert `""`, so the source keeps its old value. S2 makes them clearable.
- **C-5** `OnFilterChanged` discards a task whose `SearchAsync` has **no `catch`**. S3 sequences the reloads and surfaces errors.
- **C-6** `_ = SomethingAsync()` is **house style** in six other ViewModels. It is not a defect introduced here, and S3 does **not** refactor them.
- **C-7** the barcode reprint command **already exists** and is already tested; its only production caller is `PatientEditorViewModel:1322`. The gap is an **entry point from the search window**. S4 adds the entry point and nothing else.
- **C-8** the reference forbids minting a new number on reprint. SD-6.

## Slice loop (strictly S1 → S4)

Each slice runs the full 10-stage cycle from `P-02-memory.md`:

1. **Pre-Execution Verification** — build 0/0, no test count below your measured baseline, HEAD still valid, tree clean apart from your own package files, `git diff -- src/TopLab.Infrastructure/Persistence/` empty.
2. **Deep Understanding** — re-read that slice's section in `P-02.md` and every correction it cites.
3. **File Analysis** — open every file the slice touches, at the cited lines, **before** editing anything.
4. **Planning** — write the exact step-by-step plan into the slice's Stage 4 note in the memory file, including any decision you take (notably the S3 sequencing choice).
5. **Execution** — implement only this slice's scope. Patterns: MediatR + FluentValidation; hand-rolled MVVM (`ViewModelBase.SetProperty`, `RelayCommand` / `AsyncRelayCommand`); `Result`/`Error` with Arabic-first messages; hand-rolled xUnit fakes, **no mocking library**; reuse the existing `IBarcodeService` port and the existing `PrintBarcodeCommand`.
6. **Post-Execution Verification** — `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` (or VS MSBuild) ⇒ **0 errors / 0 warnings**.
7. **Validation Gate** — `VG-nn` **item by item**, including the zero-migration and zero-package gates. Every item must pass. Record the evidence in the memory file.
8. **Documentation Update** — tick the slice's stage checkboxes; append every new user-facing string to the Created UI Texts Register.
9. **Memory Status Update** — Slice Index, Current Status and Execution Log updated.
10. **Git Commit** — **LOCAL only**, on `main`, message `[P-02] Slice N/4: <slice title> — loop-engineering`. Stage **explicit paths only**.

**Upon a passing gate: commit, then start the next slice immediately** — no human pause. The only normal stop is S4 plus the wave DoD.

## Environment rules (this repository is fussy about these)

- **Every** `dotnet` command needs `-p:EnableWindowsTargeting=true` — `restore`, `build`, `test`, `dotnet-ef`. `TopLab.Presentation` targets `net8.0-windows`, uses WPF and sets `RuntimeIdentifier=win-x64`; without the flag MSBuild fails with `NETSDK1100`.
- **Never run `dotnet test` on the solution.** Run each test project on its own:
  ```
  dotnet test tests/TopLab.Domain.Tests          -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Application.Tests     -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Infrastructure.Tests  -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Presentation.Tests    -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Persistence.Tests     -p:EnableWindowsTargeting=true
  ```
- **Use the .NET 8 SDK** for `dotnet` and `dotnet-ef`; set `DOTNET_ROOT` if you installed it to a custom path, or `libhostfxr` will not be found.
- **Package versions live only in `Directory.Packages.props`.** `ManagePackageVersionsCentrally` is on. **This wave adds no package.**
- **The build is warning-free. Treat any new warning as a regression**, exactly like a failing test.
- **Never launch the application.** `App.xaml.cs` applies EF migrations to the configured database at startup, so running it could migrate the owner's real database. Building and running automated tests is fine.
- **A skipped test is reported as skipped, never as a pass.**

## Baseline (G0 — measure it yourself before Slice 1)

**The plan's numbers are a reference, not your gate.** Run the build and all five test projects, record your own numbers in the Baseline table in `P-02-memory.md`, and only then compare. If `has-pending-model-changes` reports changes at G0, **STOP before Slice 1**.

Record: the .NET SDK version, `git rev-parse HEAD`, `git status --porcelain`, `git diff -- src/TopLab.Infrastructure/Persistence/`, build warning and error counts, the passed/total count of each test project, the `has-pending-model-changes` result, and the migration file count.

**No count may fall below *your own measured* table in any slice, and no build warning may appear.**

## Git policy (absolute)

- One **local** commit per verified slice on `main`, message `[P-02] Slice N/4: <slice title> — loop-engineering`.
- **Never push. Never** create or switch a branch. **Never** amend, rebase, reset, force-push, stash, clean, or tag.
- **Never** `git add -A` / `git add .` — stage explicit paths only.
- Never commit on a red build or a failed gate.
- The owner reviews each commit afterwards; that review is **not** a gate you wait on.
- Read-only git commands are encouraged in every report: `rev-parse`, `status`, `diff`, `log`, `show`, `ls-files`.

## Forbidden, without exception

- Any git write command beyond the one authorised local commit per slice.
- **Creating, editing, deleting or applying any migration; changing the model snapshot by hand or via tooling; `dotnet ef database update`; `--force`.**
- **Adding a database column, a `HasData` row, or a permission code.**
- Adding a package, editing `Directory.Packages.props`, or writing `Version=` in a `.csproj`.
- **Redesigning any function so it avoids a migration** (SD-5) — no deriving codes instead of storing them, no moving settings into a file instead of the database, no making a persisted value transient.
- Building, stubbing or scaffolding **any** Wave 3 function: the three patient codes, the symbology catalogue, per-test container labels, barcode on receipt/envelope, the turnaround-unit change, the unfinished state, the antibiotic commercial name, the range-band unit, the per-result range override, abnormal-result colouring, or the pickup-date calculation.
- Modifying `PrintBarcodeCommand`, its handler, or its validator in S4 — you are adding an entry point, not changing the command.
- Widening `IBarcodeService` or `BarcodeLabelRenderer` in S4 — the symbology catalogue is Wave 3.
- Letting a reprint mint a new `LabId` or call `GetNextLabId` (SD-6).
- Refactoring the six pre-existing `_ = SomethingAsync()` sites outside `PatientSearchViewModel` (RD-3 / C-6).
- Naming an Infrastructure type from a Presentation file.
- Launching the application.
- Editing any document except `P-02-memory.md`. Do not edit `P-02.md`.
- Inventing Arabic strings, copyright text, a support address, or any commercial name.
- Deciding anything the plan leaves open. Record what you observed and report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية».

## The plan is a hypothesis — re-verify before you edit

**The plan's line numbers, counts and descriptions are unverified claims. Check every one against the live code before you change anything.** Confirm the cited line really contains what the plan quotes, the count is still what the plan says, and the pattern you are about to copy really exists where the plan points to. **If the code differs — the file moved, the line changed, the count differs, something the plan depends on is not there — STOP and report the difference.** Do not silently adapt and do not proceed on the assumption that the plan is roughly right.

Counting traps this wave contains:
- `grep -rn "Settings.License"` matches six writers plus migration files; **six writers, and `PatientReportPdfExporter` is not one of them.** Strip migrations before reasoning.
- `grep -rn "_ = "` across the Presentation layer returns many sites. Only `PatientSearchViewModel` is in scope (RD-3).
- `PrintBarcodeCommand` appears in five test files. Those tests are an **asset** — S4 reuses the command and must leave them green.

## Slice-specific warnings you must not skip

- **S1 is the most important slice in the wave.** It is a two-line fix to a **production** defect, and the test that proves it is `PatientReportPdfExporterTests` run **in isolation**. Running the whole suite will not prove it, because another writer's static constructor may mask the fault. **Run it alone.**
- **S1 before S2 before S3 before S4.** Do not reorder.
- **S3 must not become a codebase-wide refactor.** Six other ViewModels use the same fire-and-forget idiom; that is house style and out of scope. Fix the filter path only.
- **S4 adds an entry point and nothing else.** Do not create a new command, do not modify the existing handler, do not widen the barcode port, and do not introduce a symbology setting. All of those belong to Wave 3.
- **S4's control is a patient-card reprint.** The reference's flow is: the patient lost his card, you search for him, you print his barcode again, and you do **not** give him a new number. Present it that way.
- **S4's disabled state is mandatory.** No patient selected ⇒ command disabled. No half-wired state.

## After each slice: report, commit, continue

When a slice's gate passes, produce a report containing:

- the slice number and title, and the gate result **item by item**;
- the exact commands you ran and their key output — build warnings/errors, each test project's passed/failed/total, the `has-pending-model-changes` line, and **`git diff -- src/TopLab.Infrastructure/Persistence/` plus `git diff -- Directory.Packages.props '*.csproj'`**;
- the measured counts **compared against your measured baseline**, stated as a delta;
- for S1, the isolated `PatientReportPdfExporterTests` result;
- for S4, the `LabId`-unchanged assertion and proof that `PrintBarcodeCommand`'s folder is untouched;
- the commit hash you just made, and `git status --porcelain` **after** committing — it must be empty;
- anything you noticed that the plan did not anticipate, and anything you deliberately did not do;
- any new user-facing string (appended to the register), and any open owner decision the slice surfaced.

**Then commit (Stage 10) and begin Stage 1 of the next slice immediately — no pause, no request for human confirmation.**

## Stop rules — any one halts the loop immediately

Record it in `P-02-memory.md`'s Stop Report and wait.

- Live code contradicts `P-02.md` (SD-1), or a `file:line` turns out to be wrong in a way that changes what the slice should do.
- **Any slice appears to require a migration, a column, a `HasData` row, a permission code, or a package** (SD-3, SD-4).
- **You find yourself redesigning a function to avoid a migration** (SD-5). Stop — that is the exact failure this wave exists to prevent.
- A reprint is about to mint a new `LabId`, or to call `GetNextLabId` (SD-6).
- The build or the tests go red and cannot be restored within the slice's scope.
- `has-pending-model-changes` reports changes at G0 or at any slice boundary.
- The same failure occurs **5 consecutive** times.
- Any ambiguity not already settled in §0 — report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية» and do **not** decide it yourself.
- The application is about to be launched, or a migration is about to be created, edited or applied. Stop; both are forbidden.

## Slice map (details in P-02.md)

| # | Title | **Migration** | Depends on |
|---|---|---|---|
| 1 | QuestPDF licence in `PatientReportPdfExporter` (D-1) | **NONE** | — |
| 2 | Tri-state checkboxes + clearable age field (D-2, D-4) | **NONE** | — |
| 3 | Sequenced, error-surfacing filter reload (D-3) | **NONE** | S2 |
| 4 | Independent barcode reprint from search (A-12) | **NONE** | — |

**Wave 3 — described in the plan §8, NOT to be built in this wave:** the three-code patient identity, the barcode symbology catalogue, per-test tube & container labels, barcode on the receipt and envelope, the turnaround-unit change, the unfinished state and its filter, the antibiotic commercial name, the unit on the range band, the per-result range override, abnormal-result colouring, and the estimated pickup date. Every one of them requires a migration, and every one of them is executed **as originally specified**, with its migration, in Wave 3.

**Begin at Step 0, measure and report the G0 baseline, then go straight into Slice 1, Stage 1. Run all four slices in sequence, committing each one, and stop only when Slice 4's gate and the wave DoD pass, or a Stop Rule triggers.**
