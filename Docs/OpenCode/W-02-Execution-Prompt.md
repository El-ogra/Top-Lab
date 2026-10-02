# Loop-Engineering Execution Prompt — Workstream W-02 (Wave 2)

> Convention: standalone prompt per workstream (S-05/S-06/S-07/W-01 pattern). **Copy everything below the line verbatim to the local executing coding agent.**

---

You are the local executing coding agent for Top-Lab workstream **W-02** (Wave 2), under the loop-engineering protocol used by S-00…S-07 and W-01.

**Repository:** work inside the local clone of Top-Lab (`main`).
**Pinned commit:** `94292c2b2c953f9a2767cf7e81392ade0f01187d` («[W-01] Slice 8/8: Branch UI + shell status (WP-15)»). `main == origin/main`.

## Step 0 — Pin baseline

```
git rev-parse HEAD
git status --porcelain
```

`git rev-parse HEAD` must print `94292c2b2c953f9a2767cf7e81392ade0f01187d`, or a descendant whose diff is **only** the W-02 package files under `Docs/OpenCode/`: `W-02.md`, `W-02-memory.md`, `W-02-Execution-Prompt.md`. `git status --porcelain` must otherwise be empty. **If not → STOP and report.**

At the start of every slice, re-run both and include them in your report. At the end of every slice, re-run `git status --porcelain` (empty after committing) and `git log --oneline -1`, so the owner can see exactly what that slice committed.

## Source documents (read both fully before any code)

1. **`Docs/OpenCode/W-02.md`** — the wave plan: §0 binding decisions SD-1…SD-16, §1 plan-vs-code corrections C-1…C-26, §2 the dependency-ordered slice map, §3–§18 the sixteen slices each with a scope file table, a technical approach and a validation gate `VG-01`…`VG-16`, §19 general gates, §20 baseline, §21 stop rules, §23 wave DoD.
2. **`Docs/OpenCode/W-02-memory.md`** — the living memory: SD/C/facts, the G0 baseline table, the slice index, per-slice 10-stage checklists, the migration register, the created-UI-texts register, the execution log and the stop report. **You update this file as you go.**

**No other module file is required.** If a fact is not in these two files and not in live code, stop and report. Do not go hunting through `Docs/` for other audit reports; they are unverified inputs, not sources.

## Mission

Implement the complete Wave 2 remediation — six packages, sixteen slices:

- **WP-06** honest print coordinator (no migration)
- **WP-07** range low/high comments — **already done in WP-01**; no slice, regression net only (S16)
- **WP-10** history reports + CBC matrix (no migration)
- **WP-13** combined-report options + off-lab note + test comments (**1** new migration)
- **WP-14** culture depth (**2** new migrations)
- **WP-29** data integrity and diagnostics (no migration)

**Exactly three new migrations**, with these exact names: `AddCombinedReportPrintOptions` (S7), `AddCultureMicroscopyAndZone` (S9), `AddAntibioticMasterFields` (S10). **There is no fourth migration under any circumstances.** **Never edit, delete or rename any of the eleven existing migrations**, and never edit `ApplicationDbContextModelSnapshot.cs` except through EF tooling inside S7, S9 and S10.

## Binding decisions — do NOT reopen (W-02.md §0)

- **SD-1 — Decision 2 = (b1).** Zero the values, keep the columns. `IsPrinted=false`, `PrintCount=0`. **All six columns stay mapped** on `PatientTest` and `ProfileResultItem` (`IsPrinted`, `PrintCount`, `LastPrintedByUserId`, `LastPrintedAtUtc` on each). **No `DropColumn`. No fourth migration. No backup** — the system never ran in production (owner attestation). **The lifecycle guards stay intact**: `MarkDelivered`, `Unreview` and `ClearResult` are not touched. **Precise claim — do not overstate it.** Removing `MarkPrinted` calls prevents *future* writes; it does **not** retro-zero a row that was already printed. Two verified facts carry the guarantee: no `HasData` anywhere sets either field, so every seeded row starts `false`/`0`; and `PatientTest`'s private constructor never assigns them, making `MarkPrinted` the only production writer. **Owner-side verification, outside your scope:** on the development database the owner runs a `COUNT(*)` read; only if it is non-zero does a hand-written `UPDATE` run. **Never create a migration for that.**
- **SD-2 — Decision 3 = (ج).** Scientific name only. `Antibiotic.Symbol` + `Antibiotic.ScientificName`; «الاسم العلمي» appears in the sensitivity table. **Commercial names deferred entirely: no storage, no printing.** Four artefacts are dropped and must not be built: (1) `AntibioticCommercialName` entity, (2) `AntibioticCommercialNameConfiguration`, (3) the commercial-names grid in `AntibioticsView`, (4) the test `Report_ShowCommercialNameFalse_HidesColumn`. The stage plan's text naming them is **superseded**.
- **SD-3 — WP-07** is complete. `CombinedReportLineDto.LowComment`/`HighComment` exist and are fed from the frozen `PatientTestReferenceRangeSnapshot`. No slice; S16 is a regression net only.
- **SD-4** sensitivity labels stay the five English ones already shipped in `CultureEntryViewModel.cs:55-59`; `SensitivityCategory` values 0–3 do not change.
- **SD-5** `IAppLogger` is **frozen** — `Log(string requestName, string outcome, TimeSpan duration)`, pinned by a reflection test. Swallowed print exceptions go through a **new, separate** port `IPrintingDiagnostics` in `Application/Common/Interfaces`, implemented at `Infrastructure/Logging/PrintingDiagnostics.cs`. **Never** put an exception message, a patient identifier or a result into `requestName`.
- **SD-6** positional records are **append-with-default only**. Never insert a member in the middle.
- **SD-7 — forbidden:** `BarcodeService.ToAscii` (until WP-23) · the `TestDisplayNameResolver` chain (until WP-17) · `IAppLogger` · any of the eleven existing migrations · `SensitivityCategory` values · reference-range matching logic.
- **SD-8** Arabic strings are **byte-for-byte from the stage plan**. Any new label the plan does not supply is marked **`TBD-AR`** in the UI-texts register and **not invented**. **Never** invent copyright text, a support address, or any commercial drug name.
- **SD-9** git: one **local** commit per verified slice. Never push. No branch, amend, rebase, reset. Never `git add -A` / `git add .`. The owner pushes after the wave.
- **SD-10** the plan is a hypothesis: code mismatch → STOP and report.
- **SD-11** migration budget: 3 new, only. No fourth.
- **SD-12** layering: `App.xaml.cs:22` forbids any Presentation type referencing Infrastructure. Three ViewModels violate it today. **No refactor inside this wave** — add a structural non-regression test and record the debt.
- **SD-13 — data-loss ordering:** **S2 must precede S9.** `SaveCultureResultsCommandHandler.cs:5` deletes and recreates every sensitivity row on every save; if the zone migration lands first, the first save after it erases every `InhibitionZoneMm` in the database.
- **SD-14 — file ownership:** WP-06 owns `ReportPrintingService.cs` in S4. WP-29 adds diagnostics to it in S13, **after** S4.
- **SD-15** migrations get an independent analysis-only review agent after the wave; the owner does not review them.
- **SD-16 — C-21 + C-26:** `MarkResultPrintedCommand` has **zero** consumers in `src/`, but it is referenced by **four** live test files, not two: `ValidatorRegistrationTests.cs:71,219` · `ResultsEntryAuthorizationTests.cs:10,57,64` · `ExportPatientReportPdfCommandHandlerTests.cs:8` · `ReviewPrintDeliverCommandHandlerTests.cs:2,95,96,129,130,151,152,172,173,193,194`. **Binding default: WIRE, not delete** — change `MarkResultPrintedCommandHandler` to go through `IResultPrintCoordinator` with `ResultPrintKind.SimpleResult` instead of `pt.MarkPrinted`. **Delete is an exception** requiring explicit owner authorization recorded at S5 Stage 4, plus edits to all four test files — and the first two are *structural* gates, so deleting weakens the safety net itself. Record the decision in `W-02-memory.md` Stage 4 before editing.

## Plan-vs-code corrections you must honour (W-02.md §1)

The stage plan's line numbers are frequently wrong. **Use the corrected anchors.** Among the ones that change what you build:

- **C-1** history screen access already works (`PatientsHubViewModel.cs:79-87`) — do not "fix" navigation.
- **C-5** history dates already print (`ReportContentBuilder.cs:343`) — do not re-add.
- **C-6** `SimpleResultEntryViewModel.cs` has no print path at all — not a file to modify.
- **C-7** `ProfileEntryViewModel.cs` `PrintAsync` is **343-365** (not 331-358); `CultureEntryViewModel.cs` is **331-358** with the command at **343**.
- **C-8** `IAppLogger` is registered at `Infrastructure/DependencyInjection.cs:105`, not `:102`.
- **C-11** `OrganismC` is already in the culture preview (`CultureEntryView.xaml:141`) — drop that item.
- **C-13** the commercial-name artefacts are dropped (SD-2).
- **C-19** the nullable `SensitivityCategory` column is **unreachable on write**: `SaveCultureResultsCommand.cs:8` is `int`, and `CultureEntryViewModel.cs:223-225` drops null rows, so choosing «Unspecified» **deletes the row**. This is slice S1 and it is the most important correctness fix in the wave.
- **C-20** there is exactly **one** production site for the child-age rule (`GetCultureEntryGridQueryHandler.cs:33`), and `AgeRules` does not exist yet. Do not "sweep" every `AgeUnit == AgeUnit.Year` match — the others are unrelated.
- **C-23** the plan names `GetCultureAttachmentView` / `SaveCultureAntibioticAttachment`; neither exists. The real names are `GetCultureAntibioticsQueryHandler`, `AttachAntibioticToCultureCommand`, `DetachAntibioticFromCultureCommand`.
- **C-24** `ReportSettings` columns are `IsRequired()` (NOT NULL) with a `HasData` seed at `ReportSettingsConfiguration.cs:25` — use `bit NOT NULL` + `HasDefaultValue(false)`, not nullable `bit`.
- **C-26** the deletion scope for C-21 listed only two test files; there are **four** (full list in W-02.md C-26). Two of them are structural gates. **Default is wire, not delete** (SD-16).
- **C-25** delete the duplicate `GetSeparateHistoryReportQueryHandler` but **keep the query** — it is used at `HistoryReportsViewModel.cs:204` and `PrintHistoryReportCommandHandler.cs:49`.

## Slice loop (strictly S1 → S16)

Each slice runs the full 10-stage cycle from `W-02-memory.md`:

1. **Pre-Execution Verification** — build 0/0, no test count below baseline, HEAD still valid, tree clean apart from your own package files.
2. **Deep Understanding** — re-read that slice's section in `W-02.md` and every correction it cites.
3. **File Analysis** — open every file the slice touches, at the cited lines, **before** editing anything.
4. **Planning** — write the exact step-by-step plan into the slice's Stage 4 note in the memory file, including any decision you are taking (notably SD-16 in S5).
5. **Execution** — implement only this slice's scope. Patterns: existing MediatR + FluentValidation + hand-rolled MVVM; `Result`/`Error` with Arabic-first messages; `ArgumentException.ThrowIfNullOrWhiteSpace`; `NavigateTo<T>()` + `CurrentViewModel is T` + `LoadAsync`; hand-rolled xUnit fakes, **no mocking library**.
6. **Post-Execution Verification** — `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` (or VS MSBuild) ⇒ **0 errors / 0 warnings**.
7. **Validation Gate** — `VG-nn` **item by item**, including the zero-drift gate. Every item must pass. Record the evidence in the memory file.
8. **Documentation Update** — tick the slice's stage checkboxes; append every new user-facing string to the created-UI-texts register.
9. **Memory Status Update** — Slice Index, Current Status, Migration Register and Execution Log updated.
10. **Git Commit** — **LOCAL only**, on `main`, message `[W-02] Slice N/16: <slice title> — loop-engineering`. Stage **explicit paths only**.

**Upon a passing gate: commit, then start the next slice immediately** — no human pause. The only normal stop is S16 plus the wave DoD.

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
- **`dotnet-ef` must be 8.0.30**, matching `Directory.Packages.props`. Arguments after `--` go to MSBuild:
  ```
  dotnet-ef migrations has-pending-model-changes \
    --project src/TopLab.Infrastructure/TopLab.Infrastructure.csproj \
    --startup-project src/TopLab.Presentation/TopLab.Presentation.csproj \
    -- -p:EnableWindowsTargeting=true
  ```
- **Package versions live only in `Directory.Packages.props`.** `ManagePackageVersionsCentrally` is on. **Wave 2 adds no package.** Never write `Version=` in a `.csproj`.
- **The build is warning-free. Treat any new warning as a regression**, exactly like a failing test.
- **Never launch the application.** `App.xaml.cs` applies EF migrations to the configured database during startup, so running it could migrate the owner's real database. Building and running automated tests is fine.
- **A skipped test is reported as skipped, never as a pass.** `TopLab.Persistence.Tests` skips its container-dependent paths when Docker is absent; that is the expected, honest baseline.
- Infrastructure test failures seen on a **Linux** host (`System.Drawing.Common` / GDI+ via `ArabicFontResolver`) are environmental and do not exist on Windows. On your machine Infrastructure must be **221/221**. If it is not, stop and report before starting S1.

## Baseline (G0 — the owner's recorded measurement)

**This table is a REFERENCE, not your baseline. Measure it yourself before Slice 1.** Run the build and all five test projects on your machine, record your own numbers in the *Executing agent's own measurement* table in `W-02-memory.md`, and only then compare. The S-00…S-07 protocol requires the executing agent to measure its own baseline — adopting the owner's numbers makes "no count may fall below baseline" meaningless. **If any delta is non-zero, or Infrastructure is not 221/221, STOP and report before Slice 1.**

| Item | Value |
|---|---|
| `git rev-parse HEAD` | `94292c2b2c953f9a2767cf7e81392ade0f01187d` ✓ |
| Build (VS MSBuild) | **0 warnings / 0 errors** |
| TopLab.Domain.Tests | **484 / 484** |
| TopLab.Application.Tests | **1502 / 1502** |
| TopLab.Infrastructure.Tests | **221 / 221** |
| TopLab.Presentation.Tests | **57 / 57** |
| TopLab.Persistence.Tests | **13 passed + 1 skipped** (Docker absent) |
| **Full suite** | **2277 passed + 1 skipped** |
| `has-pending-model-changes` | **no changes** · `git diff -- src/TopLab.Infrastructure/Persistence/` empty |
| Migration files | **11** |

**No count may fall below *your own measured* table in any slice, and no build warning may appear.** The owner's table is the expected value; yours is the gate.

## Migrations policy (absolute)

- New migration classes **only**: `AddCombinedReportPrintOptions` (S7), `AddCultureMicroscopyAndZone` (S9), `AddAntibioticMasterFields` (S10). Three in total, no more.
- **Never** edit, delete or rename the eleven existing migrations or their `.Designer.cs` files.
- Touch `ApplicationDbContextModelSnapshot.cs` **only** via EF tooling, and only inside S7, S9, S10.
- **No `DropColumn` on `PatientTests` or `ProfileResultItems`** — SD-1.
- **No commercial-name table or column anywhere** — SD-2.
- **Configuration before migration**, always: Domain → EF configuration → `dotnet ef migrations add` → verify.
- Run `has-pending-model-changes` **before and after** every migration slice, and `git diff -- src/TopLab.Infrastructure/Persistence/Migrations/` to prove only the new file changed.
- **Never** run `dotnet ef database update` against the owner's database without explicit coordination.
- Follow `W-02.md` for the exact column shapes: `PrintGroupSubTitle bit NOT NULL default false`, `SuppressReprintMessage bit NOT NULL default false`; `CultureMicroscopies` 1:1 on `PatientTestId`; `InhibitionZoneMm decimal(4,1) NULL`; `SensitivityThresholdMm decimal(4,1) NULL`; `Symbol nvarchar(10) NULL`; `ScientificName nvarchar(150) NULL`.

## Git policy (absolute)

- One **local** commit per verified slice on `main`, message `[W-02] Slice N/16: <slice title> — loop-engineering`.
- **Never push. Never** create or switch a branch. **Never** amend, rebase, reset, force-push, stash, clean, tag, or rewrite history.
- **Never** `git add -A` / `git add .` — stage explicit paths only.
- Never commit on a red build or a failed gate.
- The owner reviews each commit afterwards; that review is **not** a gate you wait on. Proceed immediately.
- Read-only git commands are encouraged in every report: `rev-parse`, `status`, `diff`, `log`, `show`, `ls-files`.

## Forbidden, without exception

- Any git write command beyond the one authorised local commit per slice.
- Creating, editing, deleting or applying migrations outside S7/S9/S10; changing the model snapshot by hand; `dotnet ef database update`; `--force`.
- Launching the application.
- Editing any document except `W-02-memory.md`. Do not edit `W-02.md`.
- Any new permission code, any new `HasData` row beyond the one seed extension the plan authorises in S7, any package addition, any stack/TFM/licence/architecture change.
- Touching `IAppLogger`'s signature, `BarcodeService.ToAscii`, or the `TestDisplayNameResolver` chain.
- Inventing Arabic strings, copyright text, a support address, or any commercial drug name.
- Deciding anything the plan leaves open. Record what you observed and report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية».

## The plan is a hypothesis — re-verify before you edit

W-02.md §1 already corrects twenty-five of the stage plan's claims, but **re-verify at Stage 3 anyway**. Open each file and confirm the cited symbol, line and behaviour. If the code differs from W-02.md — file moved, line changed, count differs, a described call is gone — **STOP and report the difference.** Do not silently adapt, do not "fix the number in your head", do not assume the plan is roughly right.

Counting traps this wave contains:
- `grep -rn "IsPrinted"` hits the eleven migration designers and the snapshot as well as production code — 229 occurrences across 62 files, of which only **20 are test files**. Strip migrations before you reason about blast radius.
- `grep -rn "AgeUnit == AgeUnit.Year" src/` returns **one** relevant production site plus unrelated validator and UI-default hits. The validator and the three editor view models are **not** in scope (C-20).
- `grep -rni "CBC"` in `src/` returns **zero** production hits; every match is a test fixture using "CBC" as a literal test name. Do not mistake a fixture for a feature.
- `TestComment` appears in 50+ places across `PriceListsCommentsAndCustomGroups` and the Lab UI, and in **zero** places under `ReportProduction`, `ResultsEntry`, `CultureResults` or `ProfileResults`. Count only the report features when you verify WP-13's isolation claim.

## Slice-specific warnings you must not skip

- **S1 before S9.** The nullable sensitivity column is the whole point of the WP-03 repair migration and nothing can write `NULL` to it until S1 lands.
- **S2 before S9 — SD-13, data loss.** If S9 runs before S2, the first culture save after the migration deletes every inhibition-zone value in the database. Verify the S2 commit exists in `git log` at the start of S9's Stage 1; if not, **STOP**.
- **S4 owns `ReportPrintingService.cs`; S13 consumes it.** Do not touch that file in S5 or S6.
- **S7's seed row is intentional.** EF will emit an `UpdateData` for `ReportSettings` because `HasData` at `ReportSettingsConfiguration.cs:25` changes. That is correct, not an accident.
- **S8 needs no migration.** `TestComment` already has its table and a full CRUD slice. The single WP-13 migration buys the two `bit` columns and nothing else. Do not add a fourth.
- **S5 defaults to wiring, not deleting.** Four test files reference `MarkResultPrinted`; do not touch any of them on the default path. VG-05 asserts they are unmodified.
- **S11 reuses SD-4's English labels for the «الفئة» column** because the stage plan supplies no Arabic for the five category values. **Do not invent Arabic for them.** Register the reuse in the UI-texts register.
- **S12 does not use `EF.Functions.Collate`.** There is **zero** precedent for it in this codebase (`grep -rn "EF.Functions" src/` ⇒ none) and **no collation is configured anywhere** (`grep -rni "collation|UseCollation" src/` ⇒ none), while `PatientHistoryResolver.ResolveKey` normalises with `.ToUpperInvariant()` — a .NET function that cannot be reproduced in SQL against an unknown collation. The binding approach is a **bounded SQL candidate set** (`Where(... p.FullName.StartsWith(firstToken))`) followed by the exact `ResolveKey` comparison in memory, with the case-sensitivity caveat documented in XML doc. If the owner explicitly asks for `Collate`, that is an owner decision, not your suggestion.
- **S14 introduces the first raw SQL in the codebase** (`WITH (UPDLOCK, HOLDLOCK)` in `SettleAccountInFullCommandHandler`). Scope it to that one call site. Its concurrency tests need a real database, so they live in `TopLab.Persistence.Tests` and skip honestly when Docker is absent.
- **S15 is a non-regression test, not a refactor.** The three layer-violating ViewModels stay exactly where they are; record them as numbered debt.
- **S16 is the wave gate.** Nothing is done until all sixteen `VG-nn` are green, the three migrations exist under their exact names, the eleven originals are untouched, and the suite is at or above 2277 + 1 skipped.

## After each slice: report, commit, continue

When a slice's gate passes, produce a report containing:

- the slice number and title, and the gate result **item by item**;
- the exact commands you ran and their key output — build warnings/errors, each test project's passed/failed/total, the `has-pending-model-changes` line, and for migration slices `git diff --name-only` over the Migrations folder;
- the measured counts **compared against the baseline**, stated as a delta;
- the commit hash you just made, and `git status --porcelain` **after** committing — it must be empty;
- anything the plan did not anticipate, and anything you deliberately did not do;
- any new user-facing string (appended to the register), and any open owner decision the slice surfaced.

**Then commit (Stage 10) and begin Stage 1 of the next slice immediately — no pause, no request for human confirmation.**

## Stop rules — any one halts the loop immediately

Record it in `W-02-memory.md`'s Stop Report and wait.

- Live code contradicts `W-02.md` (SD-10), or a `file:line` in W-02.md turns out to be wrong in a way that changes what the slice should do.
- **S9 would run before S2** (SD-13).
- Any slice appears to require a fourth migration, a `DropColumn` on `PatientTests`/`ProfileResultItems`, a commercial-name artefact, a new package, a new permission code, or any stack/TFM/licence change.
- The build or the tests go red and cannot be restored within the slice's scope.
- The Infrastructure baseline is not 221/221 at the start of S1, **or any delta between your own G0 measurement and the owner's table is non-zero**.
- `MarkResultPrinted` is about to be **deleted** without recorded owner authorization (SD-16/C-26) — the default is wire.
- The same failure occurs **5 consecutive** times.
- Any ambiguity not already settled in §0 — report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية» and do **not** decide it yourself.
- The application is about to be launched, or a migration is about to be edited or deleted. Stop; both are forbidden.

## Slice map (details in W-02.md)

| # | Title | Package | Migration | Depends on |
|---|---|---|---|---|
| 1 | Culture sensitivity write path accepts NULL (C-19) | WP-14 | — | — |
| 2 | SaveCultureResults updates rows instead of recreating them | WP-14 | — | — |
| 3 | AgeRules + infant child detection (C-20) | WP-14 | — | — |
| 4 | IResultPrintCoordinator — build, print, never mark | WP-06 | — | — |
| 5 | Entry screens print through the coordinator + C-21 | WP-06 | — | S4 |
| 6 | Bulk print through the coordinator, reports Failed | WP-06 | — | S4 |
| 7 | ReportSettings print flags + `AddCombinedReportPrintOptions` | WP-13 | **M1** | — |
| 8 | Combined-report options, outside-lab note, test comments | WP-13 | — | S7, S6 |
| 9 | `AddCultureMicroscopyAndZone` | WP-14 | **M2** | **S2** |
| 10 | `AddAntibioticMasterFields` | WP-14 | **M3** | — |
| 11 | Culture sensitivity table + microscopy block in the report | WP-14 | — | S9, S10 |
| 12 | History filters + CBC matrix + dead-code cleanup | WP-10 | — | S11 |
| 13 | Swallowed print exceptions reach a diagnostics sink | WP-29 | — | S4 |
| 14 | Unit of work, visit deltas, id recovery, settlement lock | WP-29 | — | — |
| 15 | Narrow hot readers, own the temp dir, layering guard | WP-29 | — | S13, S14 |
| 16 | WP-07 regression net + wave DoD | WP-07 | — | S1…S15 |

**Begin at Step 0, confirm the G0 baseline, then go straight into Slice 1, Stage 1. Run all sixteen slices in sequence, commit each one, and stop only when Slice 16's gate and the wave DoD pass, or a Stop Rule triggers.**
