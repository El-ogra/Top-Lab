# Loop-Engineering Execution Prompt — Batch B-01 (R-F05, R-F01, R-A04)

> **Convention note.** This file follows the repository's `Docs/OpenCode/Execution-Prompts.md` pattern and the standalone `S-05` / `S-06` / `S-07` / `P-01` / `P-02` execution-prompt form used by every workstream since S-05. Copy everything below the line **verbatim** to the local coding agent.

---

You are the local executing agent for Top-Lab batch **B-01**, operating under the loop-engineering / module-execution protocol already established in this repository by workstreams S-00 through S-07.

**Repository:** https://github.com/El-ogra/Top-Lab.git (clone it yourself if you do not already have it; work inside your clone).
**Pinned commit:** `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757` (branch `main`, commit subject «الإستعداد للرحلة»).

## Step 0 — Pin the commit. Do this before anything else.

```
git clone https://github.com/El-ogra/Top-Lab.git
cd Top-Lab
git checkout 7a2cfb505acd8f6bdac4e0b49c8059d95d19a757
git rev-parse HEAD
git status --short
```

- `git rev-parse HEAD` must print `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757` exactly.
- `git status --short` must print nothing.

**If either check fails, STOP and report.** Do not substitute another commit, do not continue on a different state, do not "fix" the tree yourself.

At Step 0, `HEAD` must be the pinned commit and the tree clean. Thereafter `HEAD` **advances by exactly one commit per slice** — that is expected and correct. At the end of every slice, re-run `git status --short` (must be empty, after committing) and `git log --oneline -1`, and include both in your report.

**Your two source documents (read both in full before writing any code):**

1. **`Docs/OpenCode/B-01.md`** — the execution plan: eight ordered slices, the complete file inventory, business rules `BR-F05-1…18`, `BR-F01-1…12`, `BR-A04-1…9`, validation gates `VG-01…VG-08`, the migration decision, the resolved owner decisions, the unverified items, and the verification log.
2. **`Docs/OpenCode/B-01-memory.md`** — the memory file: the settled decisions `SD-1…SD-20`, the slice validation-gate table, the slice index, the per-slice 10-stage checklists, the baseline table, the created-UI-texts register, the execution log and the stop report. **You update this file as you go — it is the only document you may edit.**

Place both files in the clone at `Docs/OpenCode/` before starting.

**You have no prior context on this project and you do not need any.** Every claim in the plan was re-verified against the code at the pinned commit before the plan was finalised, so the plan is trustworthy — but it is still a document written by someone else, and you must re-check anything you rely on (see "The plan is a hypothesis" below).

## Mission

Execute **eight small, ordered slices** that close three capability gaps across the Application, Infrastructure and Presentation layers:

- **R-F05 — banded result monitor.** A new statistics query lists every result of one chosen test, in one period, whose numeric value lies in an inclusive `[Min, Max]` band, with the patient context columns; a print action writes the same grid to a PDF at a user-chosen path, through its own port and its own writer.
- **R-F01 — patient statistics extension.** Two opt-in additions to the existing patient-count statistics query: grouping by day-of-month, and a money row reporting cash received in the period.
- **R-A04 — test-order edit gestures.** A first-registration-only guard on the bulk "clear all tests" action, enforced in the Application handler; and the taken-outside-lab note closed on the specialised-profile printed report path, which today silently renders `false`.

**Every slice requires no new EF migration, no edit to any existing migration or to the model snapshot, no application of migrations, no `HasData`/seed change, no new permission code, and no database schema change.** The `Persistence/` directory is read-only for the entire batch and every slice gate asserts its diff is empty.

**The five owner decisions are already closed. Do not re-open any of them, and do not raise them as open questions:**

- **OD-1** — the test picker reuses the existing un-paged `SearchTestCatalogQuery`. **No new query, no new validator, no new authorization entry.**
- **OD-2** — the money row is cash **received in the period**: `PaymentOperation.OperationAtUtc` inside `[From 00:00, To+1 00:00)`, summed through `PatientAccountCalculator.TotalPaid`, **regardless of when the patient registered**, and **including payments of soft-deleted patients**. It must **not** be routed through the period's patient list.
- **OD-3** — the first-registration guard is: the visit **is** the earliest non-deleted `Patient` row in its `LabId` group (or the only row when `LabId` is null), compared on `RegistrationDateUtc` with `PatientId` ascending as the tie-breaker. **The existing 24-hour rule and the "result already entered" rule are KEPT.** The new guard runs **after** the existence and soft-delete checks and **before** the 24-hour check.
- **OD-4** — close the **specialised-profile report path only**: pass the real `IsTakenOutsideLab` through the profile report DTO and query handler into `ResultPrintCoordinator`. The patient PDF export and the history report stay unchanged. Slice 3 of R-A04 is therefore **unconditional**.
- **OD-4b** — the monitor's min/max inputs accept a **dot** as the decimal separator **only**. A **comma is rejected** with a clear Arabic message, so a value can never be silently read as a different number.

## Environment rules (this repository is fussy about these)

- **Every** `dotnet` command needs `-p:EnableWindowsTargeting=true`. This applies to `restore`, `build`, `test`. `TopLab.Presentation` targets `net8.0-windows`, uses WPF and sets `RuntimeIdentifier=win-x64`; without the flag MSBuild fails with `NETSDK1100`.
- **The build is currently warning-free. Treat any new warning as a regression**, exactly like a failing test. The gate is `0 Warning(s) 0 Error(s)`, not "no errors".
- **Use the .NET 8 SDK.** Not 9.x. If you installed it to a custom path, set `DOTNET_ROOT` to it, or `dotnet` will fail to find `libhostfxr`.
- **Package versions live only in `Directory.Packages.props`.** `ManagePackageVersionsCentrally` is on. **Never write `Version=` in a `.csproj`.** This batch adds **no package at all**.
- **Run the test projects one at a time** if solution-level `dotnet test` is unusable on your host, and record which projects you ran.
- **`TopLab.Persistence.Tests` self-skips when Docker is unavailable** (`DockerFactAttribute`, `tests/TopLab.Persistence.Tests/RelationalIntegrationTests.cs:12-21`). A skip is not a failure, and no slice depends on SQL Server. **Report a skipped run as skipped, never as a pass.**

## Before Slice 1 — measure your own baseline

Fill in the **"Agent-measured baseline"** table in `B-01-memory.md` by running the build and **all five** test projects **on the owner's machine**, and record the .NET SDK version, `git rev-parse HEAD`, `git status --short`, the build warning and error counts, the passed/total count of every test project, the names of any failing tests, whether Docker is available, the migration file count, and the Arabic-capable font families present. **Until that table is filled in, no slice may start.**

Context, not a target: the verification environment recorded **0 warnings / 0 errors** for the build, `TopLab.Domain.Tests` **507/507**, `TopLab.Application.Tests` **1638/1638**, `TopLab.Infrastructure.Tests` **262/284** (22 `Printing` tests failing for font reasons on a Linux host), `TopLab.Persistence.Tests` 13 passed / 2 skipped, and `TopLab.Presentation.Tests` **not executable** off Windows. **On this Windows machine the Infrastructure and Presentation projects very likely pass fully. Record what is actually true — that recorded value, not this paragraph, is what every slice is judged against.**

## The slice loop

Execute slices **strictly in order 1 → 8**. No parallel slices, no reordering, no merging, no skipping ahead. Each slice runs the full ten-stage cycle defined in the memory file:

1. **Pre-Execution Verification** — build 0/0, no test count below the recorded baseline, HEAD still at the previous slice's commit, tree clean.
2. **Deep Understanding** — re-read that slice's section of `B-01.md` and the rules it cites.
3. **File Analysis** — open every file the slice touches, at the cited lines, before editing anything.
4. **Planning** — write the exact step-by-step plan into the slice's note in the memory file.
5. **Execution** — implement, using only patterns already established in this repository.
6. **Post-Execution Verification** — `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` → 0 errors, 0 warnings.
7. **Validation Gate** — evaluate that slice's `VG-nn` **item by item**, including the `git diff --stat src/TopLab.Infrastructure/Persistence/` assertion. Every item must pass. Record the evidence.
8. **Documentation Update** — tick the stage checkboxes; append any new user-facing string to the created-UI-texts register.
9. **Memory Status Update** — mark the slice complete in the Slice Index and in Current Status; set the next slice as current.
10. **Git Commit** — one local commit for this slice. See the git policy below.

## Git policy (absolute)

**You make ONE LOCAL COMMIT PER VERIFIED SLICE, on the current branch, immediately after that slice's validation gate passes.** Message format: `[B-01] Slice N/8: <slice title> — loop-engineering`.

**You NEVER push to any remote.** **The owner pushes personally after the whole batch finishes.** This is the whole policy — there is no second, contradictory one.

Also absolute: never create or switch a branch, never amend, never rebase, never reset, never revert, never stash, never clean, never force-push, never tag, and never modify or rewrite remote history. Stage **explicit paths only** — never `git add -A`, never `git add .` — so a commit can never sweep in an unrelated file. **Never commit on a red build or a failed gate.**

The owner reviews each commit afterwards. That review is **not** a gate you wait on: you proceed to the next slice immediately.

Read-only git commands are fine and encouraged in every report: `rev-parse`, `status`, `diff`, `log`, `ls-files`, `show`, `cat-file`.

Record the commit hash in the memory file's Execution Log, then begin Stage 1 of the next slice with no pause.

## Forbidden, without exception

- Any git write command beyond the one authorized local commit per slice — specifically `push`, `checkout`, `switch`, `reset`, `revert`, `stash`, `clean`, `amend`, `rebase`, `tag`, and creating a new branch.
- **Creating, editing or applying migrations**, changing the model snapshot, running `dotnet ef database update`, or using `--force`. The migrations folder is read-only for this entire batch.
- **Launching the application.** `App.xaml.cs` applies EF migrations to the configured database during startup, so running the app could migrate the owner's real database. Building and running automated tests is allowed and expected.
- **Touching any file outside the inventory in `B-01.md` §C** — that is 28 files, listed there, with the "explicitly NOT modified" list beside it. In particular: `ReportContentBuilder.cs`, `ReportDtos.cs`, `PatientReportPdfPort.cs`, `tests/TopLab.Presentation.Tests/Common/Fakes.cs` and `tests/TopLab.Application.Tests/Common/Fakes/FakeSender.cs` are **not** yours to change.
- **Editing any document except `B-01-memory.md`.** Do not edit `B-01.md`, and do not touch anything else under `Docs/`.
- **Any new permission code, any `HasData` row, any permission seed, any schema change, any new package.**
- **Deciding anything the owner already decided.** The five decisions above are closed. If the code makes one of them impossible, **stop and report** — do not re-open it.
- **Fixing the pre-existing comma-parsing defect.** `ResultFlagComputer.TryParse` uses `NumberStyles.Any` with `InvariantCulture`, so it reads `"3,5"` as **35**. That is a real defect, it is **reported and out of scope**, and you widen the method to `internal` **without touching its body**.

## The plan is a hypothesis — re-verify before you edit

**The plan's line numbers and counts were verified against the pinned commit, but verify them again before you change anything.** A line that has moved is cheap to check; a wrong *count* can silently change what you build.

For each item in your slice, open the file and confirm the cited symbol, line and behaviour are what the plan says. Concretely, before editing you should have confirmed that the cited line really contains what the plan quotes, that the pattern you are about to copy really exists where the plan says, and that the type or record you are about to extend still has the members the plan lists.

**If the code differs from the plan — the file moved, the line changed, a member is gone, a call the plan describes no longer exists — STOP and report the difference.** Do not silently adapt, do not "fix the plan's number in your head", and do not proceed on the assumption that the plan is roughly right.

## Slice-specific warnings you must not skip

- **`CombinedReportLineDto` has FIFTEEN members, and `IsTakenOutsideLab` is the THIRTEENTH** (`ReportDtos.cs:58-73`). `ResultPrintCoordinator.BuildProfileTokenAsync` (`:101-133`) currently passes **ten** positional arguments. To pass the flag you add **three** more after the existing `null` for `Culture` — `null, null, report.IsTakenOutsideLab` — for thirteen in total. **Passing a `bool` in the 10th position will not compile: that position is `CultureReportSummaryDto?`.** This is the single most likely mistake in the batch. Stop and re-read `SD-12` if you are unsure.
- **Make the new `ProfileReportDto.IsTakenOutsideLab` member OPTIONAL, with a default value.** That is what keeps `tests/.../Common/Fakes/FakeSender.cs:105-106` compiling without being edited. If you make it required, that file becomes a new inventory entry — **stop and report instead of editing it**.
- **Slice 3's numeric parse must reject a comma.** `NumberStyles.Any` **and** `NumberStyles.Number` both include `AllowThousands` and read `"3,5"` as **35** — the plan's rule `BR-F05-17` is deliberately stricter than both existing repository patterns. Reject any input containing `,` with a clear Arabic message first, then parse with `AllowDecimalPoint`. Create that Arabic message **once** and record it in the memory file's UI-texts register.
- **Slice 2's writer must wrap `ArabicFontResolver.Resolve` in a `#pragma warning disable CA1416` / `restore` pair**, exactly as `PriceListPdfWriter.cs:60-62` does. `ArabicFontResolver` is `[SupportedOSPlatform("windows")]`. Without the pragma the build gate fails. And the writer sets `Settings.License` and `Settings.UseSystemFonts` in its **own** static constructor — never rely on another writer's initialiser.
- **Slice 1 widens `ResultFlagComputer.TryParse` to `internal` and changes nothing else in that file.** It is unconditional and belongs to **R-F05**, not to R-A04.
- **Slice 4 must fix three existing call sites** at `StatisticsAuthorizationTests.cs:20`, `:41` and `:57` — the positional record grows from six parameters to eight, which is a hard compile break. This is a required part of the slice, not a tidy-up.
- **Slice 4's money row must NOT be scoped to the period's patients.** Filter on `OperationAtUtc` alone, sum through `PatientAccountCalculator.TotalPaid`, and **include payments of soft-deleted patients**. A test pins that last point on purpose.
- **Slice 6 inserts a guard; it removes nothing.** The 24-hour check and the result-entered check stay exactly as they are, with their existing Arabic messages. The two existing tests `Clear_HappyPath_RemovesAll` and `Clear_TestWithResult_Conflict` must still pass unmodified.
- **Slice 7 refreshes `CanClearAllVisitTests` inside `LoadVisitTestsAsync` (`:604`) only** — that one insertion point covers all six existing call sites. Do not add six separate refresh calls.
- **Presentation tests use hand-rolled nested fakes inside the test class**, following `Lab/PriceListsPrintCommandTests.cs:26-49`. The shared `tests/TopLab.Presentation.Tests/Common/Fakes.cs` is **not** modified, and its `FakeSender` throw at `:152` is therefore never reached.
- **`StatisticsViewModel.cs` must not contain the string `TopLab.Infrastructure`.** `PresentationLayeringTests.PresentationLayering_NoInfrastructureReferenceOutsideAppXaml` asserts an exact offender list of three files, and adding a fourth fails a shipped test. Use the port, never construct the writer directly.
- **`StatisticsView.xaml` must keep `FlowDirection="RightToLeft"`.** `PresentationStructuralTests.EveryView_IsRightToLeft` scans **every** `.xaml` under `src/TopLab.Presentation/Views/`.
- **The monitor is a section inside `StatisticsViewModel`, not a new ViewModel.** It is already registered and navigated; splitting it out would create an orphan screen.

## Established patterns — use these, invent nothing

- Hand-rolled MVVM: `ViewModelBase.SetProperty`, `RelayCommand` / `AsyncRelayCommand`, `INavigationService` + `ContentControl` with `DataTemplate`s in `MainWindow.xaml`. No MVVM framework, no control library.
- Every use case returns `Result` or `Result<T>`; failures are `Error.Validation | NotFound | Conflict | Forbidden | Unexpected` with Arabic-first messages. **Never throw for an expected outcome.** To prove a failure in a test, match on `result.Error!.Type`.
- `ResultErrorPresenter` surfaces backend errors in the UI. `IDialogService` for dialogs and confirmations. `ILabPrintTextStore.GetAsync(LabPrintTextScope.Report)` for the lab header.
- Every Arabic user-facing string that has a backend counterpart is copied **byte-for-byte** from the plan. UI-only strings are created once and appended to the memory file's UI-texts register. **Do not invent copyright text, a support address, or a link** — that is the owner's content and is out of scope.
- All UI is RTL. Identifiers and code are English; messages and UI strings are Arabic.
- Permission checks on the server are the control. A handler does not re-implement the pipeline's gate.

## After each slice: report, commit, continue

When a slice's gate passes, produce a report containing:

- the slice number and title, and the gate result **item by item**;
- the exact commands you ran and their key output — build warnings/errors, each test project's passed/failed/total, and the `git diff --stat src/TopLab.Infrastructure/Persistence/` result;
- the measured counts **compared against your recorded baseline**, stated as a delta;
- the commit hash you just made, and `git status --short` **after** committing — it must be empty;
- anything you noticed that the plan did not anticipate, and anything you deliberately did not do;
- any new user-facing string, and any owner decision the slice surfaced.

**Then commit (Stage 10) and begin Stage 1 of the next slice immediately — no pause, no request for human confirmation.**

## Stop rules — any one halts the loop immediately. Record it in the memory file's Stop Report and wait.

- The pinned commit cannot be reached, or `git status --short` is not clean at the start of a slice.
- The code differs from the plan in a way that changes what the slice should do.
- A slice appears to require a migration, a schema change, or an edit to any file outside the `B-01.md` §C inventory.
- The build or the tests go red and cannot be restored within the slice's scope.
- The same failure occurs **5 consecutive times**.
- `ResultPrintCoordinator` is about to receive a `bool` in the 10th position of `CombinedReportLineDto` — stop and re-read `SD-12`.
- `FakeSender.cs` would have to be modified because the new `ProfileReportDto` member was made required — stop and re-read `SD-13`.
- The application is about to be launched, or a migration is about to be created, edited or applied. Both are forbidden.
- Any ambiguity not already settled in the plan — record it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية» and do **not** decide it yourself.

## Slice map (details in `B-01.md`)

- Slice 1 — R-F05-S1 — Banded-monitor Application query + DTOs → `VG-01`
- Slice 2 — R-F05-S2 — Banded-monitor PDF port + writer → `VG-02`
- Slice 3 — R-F05-S3 — Banded-monitor ViewModel + XAML → `VG-03`
- Slice 4 — R-F01-S1 — Day-of-month + money row in the Application layer → `VG-04`
- Slice 5 — R-F01-S2 — Day-of-month + money row in the UI → `VG-05`
- Slice 6 — R-A04-S1 — First-registration guard in the Application handler → `VG-06`
- Slice 7 — R-A04-S2 — First-registration guard in the UI → `VG-07`
- Slice 8 — R-A04-S3 — Outside-lab note on the specialised-profile printed report → `VG-08`

## Final gate — after Slice 8

```
dotnet build TopLab.sln -p:EnableWindowsTargeting=true
dotnet test  TopLab.sln -p:EnableWindowsTargeting=true
git status --short
git log --oneline -9
git diff --stat 7a2cfb505acd8f6bdac4e0b49c8059d95d19a757 -- src/TopLab.Infrastructure/Persistence/
git diff --name-only 7a2cfb505acd8f6bdac4e0b49c8059d95d19a757
```

Expect: build 0/0; no test project below your recorded baseline; `git status --short` empty; the pinned commit plus **exactly eight** slice commits; the persistence diff **empty**; and `git diff --name-only` returning **exactly the 28 files** of the `B-01.md` §C inventory and nothing else.

**Then stop and report. Do not push. The owner reviews the eight commits and pushes personally.**

**Begin with Step 0 (pin the commit), fill in the Baseline table, report the baseline, and proceed directly into Slice 1, Stage 1. Run all eight slices in sequence, committing each one, and stop only when Slice 8's gate passes or a Stop Rule triggers.**
