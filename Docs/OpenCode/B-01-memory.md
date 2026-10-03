# Loop Engineering — Memory File

- **Batch:** B-01 — R-F05 (banded result monitor), R-F01 (patient statistics extension), R-A04 (test-order edit gestures)
- **Batch Number:** B-01
- **Source Plan:** `Docs/OpenCode/B-01.md`
- **Date Created:** 2026-10-03
- **Total Slices:** **8**
- **Current Slice:** none — Slice 1 not started
- **Current Branch:** `main` (the branch that is checked out; **never switch it**)
- **Author:** loop-engineering / module-execution (execution to be carried out by the executing agent per owner authorization; **stage-10 local commit is authorized — see SD-1**)
- **Plan status:** **FINAL.** All owner decisions (OD-1, OD-2, OD-3, OD-4, OD-4b) are closed. Nothing in this batch is awaiting an owner decision.

---

## Module Summary

B-01 closes three capability gaps in the Top-Lab medical-laboratory management system across the Application, Infrastructure and Presentation layers, in **eight ordered slices**:

- **R-F05 — banded result monitor.** A new `GetBandedResultMonitorQuery` under the existing `STATISTICS` gate lists every result of one chosen test, in one period, whose numeric value lies in an inclusive `[Min, Max]` band, with the patient context columns the reference specifies, plus a printable PDF through a dedicated port and writer.
- **R-F01 — patient statistics extension.** Two opt-in additions to the existing patient-count statistics query: grouping by day-of-month, and a money row reporting **cash received in the period** via `PatientAccountCalculator.TotalPaid`.
- **R-A04 — test-order edit gestures.** (a) A first-registration-only guard on the bulk "clear all tests" action, enforced in the Application handler; (b) the taken-outside-lab note closed on the specialised-profile **printed** report path, which today silently renders `false`.

**Zero database migrations.** Every slice is a read query, a DTO, a port, a writer, a ViewModel property, a XAML section, or a call-site correction over columns that already exist. The `Persistence/` directory is **read-only for the whole batch** and is asserted empty by every slice gate.

**Owner-delegated decisions already made — do not re-open, do not re-decide, do not raise as open questions:** OD-1 (reuse `SearchTestCatalogQuery`; no new query), OD-2 (money = received in the period, anchored on `OperationAtUtc`, soft-deleted patients' payments **included**), OD-3 (first-registration guard = earliest non-deleted row in the `LabId` group, tie-broken by `PatientId`; **the 24-hour and result-entered rules are kept**; the new guard runs before the 24-hour check), OD-4 (close the profile printed path only; slice 3 is unconditional), OD-4b (min/max inputs accept a **dot only**; a **comma is rejected** with a clear Arabic message).

---

## Global Validation Gates

- **Gate G0 (pre-execution, runs once, before Slice 1):** the agent records **its own baseline on the owner's machine** — the exact build result and the exact pass/fail counts of **all five** test projects — into the Baseline table below. **Until that table is filled in, no slice may start.**
- **Gate G1 (post-execution, every slice):** the same quality gate as G0, plus the slice-specific gate in the table below, plus the persistence-diff assertion.

### Persistence assertion (every slice, no exceptions)

```
git diff --stat src/TopLab.Infrastructure/Persistence/
```

Expected, every time: **empty**. This is the proof that no slice created, edited or applied a migration, and that `ApplicationDbContextModelSnapshot.cs` is untouched.

```
git diff --name-only <pinned-commit>
```

Expected at the final gate: **exactly the 28 files** listed in `B-01.md` §C, and nothing else.

> `dotnet-ef migrations has-pending-model-changes` is **not** a gate of any slice. Nothing in this batch touches the EF model, so the gate would be testing nothing. If `dotnet-ef` 8.0.30 happens to be installed you may run it once at G0 for information; a failure to run is not a slice failure.

### No-regression rule

**The anchor is the agent's own G0 measurement, not any number printed in this file or in the plan.** A new build warning is a regression exactly like a failing test. No test project's pass count may fall below its recorded baseline.

---

## Quality Gate (non-negotiable — a slice may be marked complete ONLY when ALL FOUR hold)

1. The slice's implementation is fully complete per `Docs/OpenCode/B-01.md`.
2. The **entire solution** builds with **zero errors AND zero warnings** (`dotnet build TopLab.sln -p:EnableWindowsTargeting=true`).
3. **No test count is lower than the recorded baseline**, in any of the five test projects — except where the plan explicitly predicts a rise, which is the norm here because most slices add tests.
4. The slice's own specific `VG-nn` gate passes **item by item**.

`Directory.Build.props` sets `Nullable=enable`, `LangVersion=latest`, `ImplicitUsings=enable` and `ManagePackageVersionsCentrally=true`. The build is currently clean and must stay clean.

---

## Stop/Continue Rule

After a slice completes, verify success via ALL THREE of:

- (a) the whole solution builds with **zero errors and zero warnings**;
- (b) no test count is below the recorded baseline;
- (c) that slice's specific `VG-nn` gate passes item by item.

**If all three hold → make the Stage-10 local commit and begin Stage 1 of the next slice immediately. No pause, no request for human confirmation.** The owner reviews each commit after the fact; that review is not a gate the agent waits on.

If any one of (a), (b), (c) fails → retry within the slice's own scope. If the **same** single failure (a specific build error, a specific file-edit failure, a specific test that will not pass, any other one repeated failure) occurs **5 consecutive times**, STOP entirely and emit a Stop Report naming what failed, at which slice and stage, and the evidence from each of the five attempts. Ordinary expected failures caused by the current slice and resolved within the same correction cycle do **not** count as five separate failures.

---

## Settled Decisions (binding — do not re-open during execution)

- **SD-1 — Git: ONE LOCAL COMMIT PER VERIFIED SLICE, on the current branch. The agent NEVER pushes.** Stage 10 is authorized exactly as in S-00…S-07. Commit message: `[B-01] Slice N/8: <slice title> — loop-engineering`. Stage **explicit paths only** — never `git add -A`, never `git add .`. **The owner pushes personally after the whole batch finishes.** Never push, never create or switch a branch, never amend, rebase, reset, force-push, tag or rewrite history. Never commit on a red build or a failed gate. Read-only git commands (`rev-parse`, `status`, `diff`, `log`, `ls-files`, `show`) are encouraged in every report. **This supersedes and resolves the contradictory commit policy in the draft plan, which had the slices committing while its closing paragraph said the agent commits nothing.**
- **SD-2 — Proceed immediately to the next slice.** After a passing gate: record the result, commit, and start the next slice's Stage 1 with no pause. The only normal stopping point is Slice 8 completing its gate.
- **SD-3 — The baseline is the agent's own measurement on the owner's machine**, recorded at G0 before Slice 1. The figures in the verification-environment table below are **context only, not the target**.
- **SD-4 — Zero migrations.** No creating, editing, applying or deleting any file under `src/TopLab.Infrastructure/Persistence/`, and no change to `ApplicationDbContextModelSnapshot.cs`, for the entire batch. Every slice's gate asserts the persistence diff is empty.
- **SD-5 — The application must never be launched.** `App.xaml.cs` applies EF migrations to the configured database at startup, so launching it could migrate the owner's real database. Building and running automated tests is allowed and expected.
- **SD-6 — Only the 28 files in `B-01.md` §C may be created or modified.** No file outside that inventory may be opened for writing. The single exception is **this memory file**, which the agent updates as its progress record. It must not edit `B-01.md` or anything else under `Docs/`.
- **SD-7 — R-F05 owns the `ResultFlagComputer.TryParse` widening, and it is unconditional.** `ResultFlagComputer.cs:69` goes from `private static` to `internal static`, performed in **Slice 1**. It is **not** an R-A04 file and **not** conditional on OD-4. It exists so the monitor reuses the identical parsing rule instead of inventing a second one. **Do not change the parsing behaviour itself** — see SD-8.
- **SD-8 — `ResultFlagComputer.TryParse`'s existing behaviour is NOT fixed in this batch.** With `InvariantCulture` it treats `,` as a thousands separator, so `"3,5"` parses to **35**. This is a known pre-existing defect, reported in `B-01.md` §L.3 as finding F-1, and it is **explicitly out of scope**. Widening the method to `internal` must not change a single character of its body.
- **SD-9 — The monitor's min/max inputs are DOT-ONLY and reject a comma** (BR-F05-17). Do **not** use `NumberStyles.Any` and do **not** use `NumberStyles.Number`: both include `AllowThousands` and read `3,5` as `35`. Reject any input containing `,` with a clear Arabic message, then parse with `AllowDecimalPoint` (add `AllowLeadingSign` only if a signed band is genuinely wanted). This is deliberately stricter than both existing repository patterns.
- **SD-10 — The money row is cash RECEIVED in the period.** `PaymentOperation.OperationAtUtc` inside `[From 00:00, To+1 00:00)`, summed through `PatientAccountCalculator.TotalPaid`, **regardless of when the patient registered**, and **including payments of soft-deleted patients** (BR-F01-9). The handler must **not** route the money through the period's `patients` list. A test pins the soft-delete behaviour so a later tidy-up cannot silently change the figure.
- **SD-11 — The 24-hour and result-entered guards in `ClearAllTestsCommandHandler` are KEPT unchanged,** with their existing Arabic messages. The new first-registration guard is inserted **between** the soft-delete check (`:28-31`) and the 24-hour check (`:33-36`). Nothing existing is removed or loosened.
- **SD-12 — R-A04-S3 passes `IsTakenOutsideLab` as the THIRTEENTH argument of `CombinedReportLineDto`,** which has fifteen members (`ReportDtos.cs:58-73`). After the existing `Culture: null`, add `null, null, report.IsTakenOutsideLab`. **Do not "add the 10th positional argument"** — position 10 is `CultureReportSummaryDto?` and a `bool` there does not compile. This is the single most likely mistake in the batch.
- **SD-13 — The new `ProfileReportDto.IsTakenOutsideLab` member is OPTIONAL with a default value.** That is what keeps `tests/.../Common/Fakes/FakeSender.cs:105-106` compiling untouched. If the agent makes it required, that file must be added to the inventory — **stop and report** instead.
- **SD-14 — `tests/TopLab.Presentation.Tests/Common/Fakes.cs` is NOT modified.** New Presentation tests use **hand-rolled nested fakes inside the test class**, following `Lab/PriceListsPrintCommandTests.cs:26-49`. The shared `FakeSender` throws at `:152` for an unrecognised request, and the nested-fake pattern sidesteps it entirely.
- **SD-15 — The new PDF writer must wrap `ArabicFontResolver.Resolve` in the same `#pragma warning disable CA1416` / `restore` pair** as `PriceListPdfWriter.cs:60-62`. `ArabicFontResolver` is `[SupportedOSPlatform("windows")]` (`ArabicFontResolver.cs:14`); without the pragma the build gate fails.
- **SD-16 — The new PDF writer sets `Settings.License` and `Settings.UseSystemFonts` in its OWN static constructor,** copying `PriceListPdfWriter.cs:25-36` and `CustomGroupPdfWriter.cs:23-32`. It must never depend on another writer's static initialiser having run.
- **SD-17 — The Arabic message `لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة.` is one literal,** reused byte-for-byte in the handler, in the ViewModel short-circuit, and in the tests. The two pre-existing Arabic messages are likewise never reworded.
- **SD-18 — `PatientEditorViewModel` refreshes `CanClearAllVisitTests` INSIDE `LoadVisitTestsAsync` (`:604`).** That single insertion point covers all six existing call sites (`:567, :840, :880, :1134, :1171, :1208`). Do not add six separate refresh calls.
- **SD-19 — Presentation-layer UI behaviour is verified manually by the owner on Windows.** The repository has no UI test harness and none may be invented. The agent's Presentation tests must be **structural and ViewModel-level only — no test may instantiate a WPF element.**
- **SD-20 — Stop threshold: 5 consecutive failures of the same single cause.** Execution order is strictly S1 → S8. No parallel slices, no reordering, no merging, no skipping ahead.

---

## Additional user-authorized execution parameters (override skill defaults)

- **Git:** automatic LOCAL commit after each verified slice (no confirmation pause), on the **current** branch. **NEVER** create or switch a branch, **NEVER** push, **NEVER** force-push, **NEVER** amend, rebase or reset, **NEVER** modify or rewrite remote history. Stage explicit paths only. Never commit on a red build or a failed gate. **The owner pushes after the whole batch.**
- **Pacing:** strictly sequential S1 → S8, **no pause between slices**, no request for human confirmation.
- **Stage-7 gate:** the plan's textual exit criteria replace any standard UI journey.
- **The application must never be launched** (SD-5).
- **Migrations are untouchable** (SD-4).
- **Docs:** the agent updates **only this memory file**.
- **The only normal stopping point** is Slice 8 completing its gate.

---

## Baseline (to be filled in by the executing agent on the owner's machine — **EMPTY AT CREATION**)

### Verification-environment figures (CONTEXT ONLY — NOT the owner's baseline)

Measured on Debian Linux 12 / .NET SDK 9.0.316 building the `net8.0` targets, at the pinned commit. Recorded so the agent knows what to expect, **not** as a target.

```
dotnet build TopLab.sln -p:EnableWindowsTargeting=true
    Build succeeded.  0 Warning(s)  0 Error(s)

TopLab.Domain.Tests          : 507 / 507
TopLab.Application.Tests     : 1638 / 1638
TopLab.Infrastructure.Tests  : 262 / 284   (22 Printing tests fail)
TopLab.Persistence.Tests     : 13 passed, 2 skipped (Docker unavailable)
TopLab.Presentation.Tests    : CANNOT EXECUTE (net8.0-windows + UseWPF)
```

The 22 Infrastructure failures are all in `Printing` test classes — `ReportPdfWriterGoldenTests` (9), `ReportPrintingServiceTests` (4), `PriceListPdfWriterTests` (2), `CustomGroupPdfWriterTests` (2), and one each in `PatientReportPdfExporterTests`, `WorkSheetPrintingServiceTests`, `ReportPdfWriterTests`, `ReceiptPrintingServiceTests`, `InvoicePrintingServiceTests`. They are caused by the Arabic font family not being resolvable on a Linux host. **On the owner's Windows machine these very likely pass, so the Windows Infrastructure baseline is expected to be 284/284.** The agent must measure and record what is actually true; the recorded value — not this table — is what every slice is judged against.

> **Do not hard-code any number from this table as an expectation.** Also note the build **requires `-p:EnableWindowsTargeting=true`** on any non-Windows host (`TopLab.Presentation` targets `net8.0-windows`, uses WPF and sets `RuntimeIdentifier=win-x64`; without the flag MSBuild fails with `NETSDK1100`).

### Agent-measured baseline (**FILL IN BEFORE SLICE 1**)

| Item | Value |
|---|---|
| Date measured | 2026-10-03 (session start; **measurement NOT completed — loop halted at Step 0**) |
| OS | Windows 10 (`LAP LINK`, Hermes Desktop, bash/MSYS shell) |
| .NET SDK version | 8.0.425 present; **9.0.318 is the default selected SDK** (no `global.json` in the repo) |
| `dotnet --version` | `9.0.318` — ⚠ **must be pinned to 8.0.425 via `DOTNET_ROOT`/a local `global.json`; the repo has none, so a bare `dotnet build` silently uses 9.x against the required .NET 8 SDK** |
| `git rev-parse HEAD` | `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757` ✓ |
| `git status --short` | ✗ **NOT EMPTY — STOP RULE #1 TRIGGERED.** Tracked tree is pristine (`git diff HEAD --stat` empty), but 12 untracked files exist under `Docs/`: `Docs/Hermes/Batch-1-Plan.md`, `Docs/OpenCode/B-01-Execution-Prompt.md`, `Docs/OpenCode/B-01-memory.md`, `Docs/OpenCode/B-01.md`, `Docs/Remaining Tasks Folder/` (8 files). `bin/`+`obj/` are correctly gitignored, so this is not build residue. See Stop Report below. |
| Build warnings | |
| Build errors | |
| `TopLab.Domain.Tests` passed / total | |
| `TopLab.Application.Tests` passed / total | |
| `TopLab.Infrastructure.Tests` passed / total | |
| `TopLab.Infrastructure.Tests` failing test names | |
| `TopLab.Persistence.Tests` passed / skipped / total | |
| `TopLab.Persistence.Tests` skipped names, if any | |
| `TopLab.Presentation.Tests` passed / total | |
| `TopLab.Presentation.Tests` failing test names | |
| Docker available | |
| Migration files present (expect 14 + 14 + 1 = 29) | |
| Arabic-capable font families present | |

---

## Slice Validation Gates

| Slice | Gate ID | Gate Description | How to Verify |
|---|---|---|---|
| 1 | **VG-01** | **R-F05-S1 — Application query and DTOs.** Build 0/0; no project below baseline; **persistence diff empty**; `ResultFlagComputer.TryParse` is `internal` **and its body is byte-identical to before** (SD-7, SD-8); the new query carries `IAuthorizedRequest → "STATISTICS"`; the validator rejects `Min > Max` and `From > To` with the exact Arabic messages; handler tests cover inclusive bounds, non-numeric exclusion, period bounds by `EnteredAtUtc`, unentered rows, profile rows with a null value, soft-deleted patients, referral resolution and the no-referral label, `NotFound` for an unknown test, the four status labels and deterministic ordering; the new query is in `StatisticsAuthorizationTests.ModuleQueries`; the new validator is in `ValidatorRegistrationTests`; **no file under `Features/Statistics/` other than the three new query files and the new DTO file was touched**. **Migration: NONE** | `dotnet build`; `dotnet test`; `git diff --stat src/TopLab.Infrastructure/Persistence/`; `git diff -- src/TopLab.Application/Features/ResultsEntry/Common/ResultFlagComputer.cs` (must be the single keyword `private`→`internal`); the greps and file list above |
| 2 | **VG-02** | **R-F05-S2 — PDF writer.** Build 0/0; Infrastructure not below baseline; persistence diff empty; the port and the writer are **separate types over a separate DTO**, sharing nothing with `ICustomGroupPdfWriter`/`IPriceListPdfWriter`; the writer sets `Settings.License`/`Settings.UseSystemFonts` in its **own** static constructor; `ArabicFontResolver.Resolve` sits inside the `CA1416` pragma pair; `File.Exists` → `IOException`; missing directory → `DirectoryNotFoundException`; a successful write to a temp path produces a non-empty file; `BuildTextLines` yields the criteria line and the nine grid headers; the empty-rows case still writes header and criteria; exactly one new line in `Infrastructure/DependencyInjection.cs`; **no new `ReportKind` constant and no `PrinterOutputType` value**. **Migration: NONE** | `dotnet build`; `dotnet test tests/TopLab.Infrastructure.Tests`; persistence diff; `git diff -- src/TopLab.Infrastructure/DependencyInjection.cs`; `git diff --stat -- src/TopLab.Infrastructure/Persistence/` |
| 3 | **VG-03** | **R-F05-S3 — Presentation.** Build 0/0; no project below baseline; persistence diff empty; the picker is populated from `SearchTestCatalogQuery` and **no new Application query/validator/authorization entry was added for it**; **a comma in min or max is rejected with the Arabic message and the mediator is NOT called** (SD-9); a dot parses correctly; the print command short-circuits on a `null` save path and otherwise writes exactly once with the loaded `BandStats`; `StatisticsView.xaml` still declares `FlowDirection="RightToLeft"` (**`PresentationStructuralTests.EveryView_IsRightToLeft` scans every `Views/**/*.xaml`**); **`StatisticsViewModel.cs` does not contain the string `TopLab.Infrastructure`** (`PresentationLayeringTests` asserts an exact three-file offender list); **`tests/TopLab.Presentation.Tests/Common/Fakes.cs` is unmodified**; every new `{Binding …}` in the XAML names a real public property (no shipped test checks this — the agent must). **Migration: NONE** | `dotnet build`; `dotnet test`; persistence diff; `git diff --name-only` must not list `Fakes.cs`; `grep -c "TopLab.Infrastructure" src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs` → 0; `grep -c 'FlowDirection="RightToLeft"' src/TopLab.Presentation/Views/Statistics/StatisticsView.xaml` ≥ 1 |
| 4 | **VG-04** | **R-F01-S1 — DTO, query, handler.** Build 0/0; no project below baseline; persistence diff empty; the positional record grew 6 → 8 and **the three call sites at `StatisticsAuthorizationTests.cs:20/:41/:57` were fixed** (hard compile break, fixed in this slice); the day grouping is on `RegistrationDateUtc.Day` only, ordered, with zero-count days omitted and empty when the flag is false; the money row is filtered on `OperationAtUtc` alone and summed via `PatientAccountCalculator.TotalPaid`; **payments of soft-deleted patients are included**; `Money` is `null` and no `PaymentOperation` is read when the flag is false; all five pre-existing classifications and `TotalCount` are unchanged when both flags are false. **Migration: NONE** | `dotnet build`; `dotnet test`; persistence diff; `git diff -- src/TopLab.Application/Features/Statistics/`; the `Money_IncludesPaymentsOfSoftDeletedPatients` fact must exist and pass |
| 5 | **VG-05** | **R-F01-S2 — Presentation.** Build 0/0; no project below baseline; persistence diff empty; two new checkboxes bound to `GroupByDayOfMonth` and `IncludeMoneyRow`; the day grid shows only when the flag is true; the money line renders with invariant formatting; `StatisticsView.xaml` still RTL; no `TopLab.Infrastructure` reference in the ViewModel. **Migration: NONE** | `dotnet build`; `dotnet test`; persistence diff; the greps from VG-03 |
| 6 | **VG-06** | **R-A04-S1 — Application guard.** Build 0/0; no project below baseline; persistence diff empty; the guard sits **between** the soft-delete check and the 24-hour check; a non-first visit yields `Error.Conflict` with `لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة.`; **the 24-hour and result-entered messages are unchanged and still fire**; a soft-deleted prior visit does not block; a tie on `RegistrationDateUtc` is broken by the lower `PatientId`; `Clear_HappyPath_RemovesAll` (`:14-30`) and `Clear_TestWithResult_Conflict` (`:32-47`) still pass unchanged; the guard performs **no write** on the refusal path. **Migration: NONE** | `dotnet build`; `dotnet test tests/TopLab.Application.Tests`; persistence diff; `git diff -- src/TopLab.Application/Features/PatientRegistration/Commands/ClearAllTests/` |
| 7 | **VG-07** | **R-A04-S2 — UI guard.** Build 0/0; no project below baseline; persistence diff empty; `CanClearAllVisitTests` is refreshed **inside** `LoadVisitTestsAsync` and nowhere else; the `مسح الكل` button binds `IsEnabled`; a stale command invocation yields the same Arabic message; **the ViewModel and the handler use the same literal**; `PatientEditorView.xaml` still RTL. **Migration: NONE** | `dotnet build`; `dotnet test`; persistence diff; `git diff -- src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` |
| 8 | **VG-08** | **R-A04-S3 — Profile outside-lab note (UNCONDITIONAL).** Build 0/0; no project below baseline; persistence diff empty; `ProfileReportDto` gained the member **as optional with a default**; `GetProfileReportQueryHandler` populates it from the `PatientTest` it already loads; `ResultPrintCoordinator` passes it as the **13th** argument of `CombinedReportLineDto` (**thirteen, not ten**); with the flag `true` the produced envelope's single line carries `IsTakenOutsideLab == true`, and with `false` it still does; **`ReportContentBuilder.cs` is unmodified**; **`ReportDtos.cs` is unmodified**; **`PatientReportPdfPort.cs` is unmodified**; **`FromHistory` is unmodified**; **`tests/.../Common/Fakes/FakeSender.cs` is unmodified**. **Migration: NONE** | `dotnet build`; `dotnet test`; persistence diff; `git diff --name-only` must list only `ProfileResultDtos.cs`, `GetProfileReportQueryHandler.cs`, `ResultPrintCoordinator.cs` and `ResultPrintCoordinatorTests.cs` among the production/test files of this slice |

---

## Slice Index

| # | Slice | Item | Files touched | Status | Gate |
|---|---|---|---|---|---|
| 1 | R-F05-S1 — Banded-monitor Application query + DTOs | R-F05 | create 4 · modify 3 | [ ] NOT STARTED | VG-01 |
| 2 | R-F05-S2 — Banded-monitor PDF port + writer | R-F05 | create 2 · modify 1 | [ ] NOT STARTED | VG-02 |
| 3 | R-F05-S3 — Banded-monitor VM + XAML | R-F05 | create 1 · modify 2 | [ ] NOT STARTED | VG-03 |
| 4 | R-F01-S1 — Day-of-month + money row in the Application layer | R-F01 | modify 3 · modify 1 test | [ ] NOT STARTED | VG-04 |
| 5 | R-F01-S2 — Day-of-month + money row in the UI | R-F01 | modify 2 | [ ] NOT STARTED | VG-05 |
| 6 | R-A04-S1 — First-registration guard in the Application handler | R-A04 | modify 1 · modify 1 test | [ ] NOT STARTED | VG-06 |
| 7 | R-A04-S2 — First-registration guard in the UI | R-A04 | modify 2 · create 1 test | [ ] NOT STARTED | VG-07 |
| 8 | R-A04-S3 — Outside-lab note on the profile printed report | R-A04 | modify 3 · modify 1 test | [ ] NOT STARTED | VG-08 |

**Cross-slice file overlap (expected, and the reason for this order):** `StatisticsViewModel.cs` and `StatisticsView.xaml` are modified by slices 3 and 5; `StatisticsAuthorizationTests.cs` by slices 1 and 4. No two slices are ever open at the same time, because execution is strictly sequential.

---

## Per-slice 10-stage loop

Every slice runs the same ten stages. Record the outcome of each in the Execution Log.

1. **Pre-execution verification** — build 0/0; no test count below the G0 baseline; `git status --short` empty; HEAD is the previous slice's commit.
2. **Deep understanding** — re-read that slice's section of `B-01.md` and the rules it cites (BR-\*, VG-\*).
3. **File analysis** — open every file the slice touches, at the cited lines, before editing anything. **Re-confirm every line number: this plan was verified against one commit and the code may have moved.**
4. **Planning** — write the exact step-by-step plan into this file's slice note.
5. **Execution** — implement, using only patterns already established in this repository. No new framework, no new package, no new test library.
6. **Post-execution verification** — `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` → 0 errors, **0 warnings**.
7. **Validation gate** — evaluate that slice's `VG-nn` item by item, including the persistence diff. Record the evidence.
8. **Documentation update** — tick the stage checkboxes; append any new user-facing string to the register below.
9. **Memory status update** — mark the slice complete in the Slice Index and in Current Status; set the next slice as current.
10. **Git commit** — one local commit, message `[B-01] Slice N/8: <slice title> — loop-engineering`, explicit paths only. **Then begin Stage 1 of the next slice with no pause.** Never push.

---

## Per-slice stage checklists

*(The agent copies the relevant block into the Execution Log and ticks each line.)*

- [ ] **Slice 1** — Stage 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10
- [ ] **Slice 2** — Stage 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10
- [ ] **Slice 3** — Stage 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10
- [ ] **Slice 4** — Stage 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10
- [ ] **Slice 5** — Stage 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10
- [ ] **Slice 6** — Stage 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10
- [ ] **Slice 7** — Stage 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10
- [ ] **Slice 8** — Stage 1 · 2 · 3 · 4 · 5 · 6 · 7 · 8 · 9 · 10

---

## Created UI texts register

Every Arabic string introduced by this batch. **Backend strings are frozen and must be copied byte-for-byte from `B-01.md`; UI-only strings are created once and listed here.** Do not invent a copyright line, a support address or a link — that is the owner's content and is out of scope.

| # | String | Layer | Slice | Source |
|---|---|---|---|---|
| 1 | `لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة.` | Application + Presentation (shared literal) | 6, 7 | BR-A04-2 — frozen |
| 2 | `الحد الأدنى يجب ألا يتجاوز الحد الأقصى.` | Application validator | 1 | BR-F05-6 — frozen |
| 3 | `بداية الفترة يجب ألا تتجاوز نهايتها.` | Application validator | 1 | BR-F05-7 — frozen (verbatim from `GetPatientCountStatisticsQueryValidator.cs:11`) |
| 4 | `التحليل غير موجود.` | Application handler | 1 | BR-F05-1 |
| 5 | *(comma-rejection message for min/max)* | Presentation | 3 | **TO BE CREATED ONCE** — must be a clear Arabic sentence naming the problem; record the exact text here in Slice 3 and reuse it verbatim in the ViewModel and its test |
| 6 | `نطاق النتائج` (monitor section title) | Presentation | 3 | **TO BE CREATED ONCE** — record here |
| 7 | `الحد الأدنى` / `الحد الأقصى` | Presentation | 3 | **TO BE CREATED ONCE** — record here |
| 8 | `عرض` / `طباعة` | Presentation | 3 | **TO BE CREATED ONCE** — record here |
| 9 | `التاريخ` / `المريض` / `الرقم` / `الجنس` / `العمر` / `جهة الإحالة` / `التحليل` / `النتيجة` / `الحالة` | Presentation + PDF | 1, 2, 3 | **TO BE CREATED ONCE** — record here |
| 10 | `تم التسليم` / `تمت الطباعة` / `معتمد` / `غير معتمد` | Presentation + PDF | 1, 2, 3 | BR-F05-10 — **TO BE CREATED ONCE** — record here |
| 11 | `بدون جهة إحالة` | Application + Presentation + PDF | 1, 2, 3 | BR-F05-12 — **frozen**, copied from `GetPatientCountStatisticsQueryHandler.cs:14` |
| 12 | `المدفوعات: {AmountsPaid}` | Presentation | 5 | BR-F01-12 — **TO BE CREATED ONCE** — record here |
| 13 | `حسب يوم الشهر` / `اليوم` / `العدد` | Presentation | 5 | BR-F01-5 — **TO BE CREATED ONCE** — record here |

**Pre-existing messages that must NOT be reworded:** `المريض غير موجود.` · `المريض محذوف.` · `لا يمكن مسح التحاليل من مريض أضيف قبل أكثر من 24 ساعة.` · `لا يمكن مسح تحاليل تم تسجيل نتائج لها.` · `احفظ بيانات المريض أولًا قبل مسح التحاليل.` · `أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام` · `العينة أُخذت خارج المعمل` · `تعذّر تحميل بيانات المعمل للطباعة.` · `الملف موجود مسبقًا؛ لم يتم الكتابة فوقه.`

---

## Execution Log

*(One block per slice. The agent fills these in as it goes.)*

### Slice 1 — R-F05-S1 — Banded-monitor Application query + DTOs
- **Date / agent:** 
- **Files created:** 
- **Files modified:** 
- **Build result (warnings / errors):** 
- **Test results vs baseline (per project, as a delta):** 
- **Persistence diff:** 
- **VG-01 item-by-item result:** 
- **New UI strings:** 
- **Deviation from the plan (and why):** 
- **Commit hash:** 
- **`git status --short` after the commit:** 
- **Anything noticed that the plan did not anticipate:** 

### Slice 2 — R-F05-S2 — Banded-monitor PDF port + writer
*(same fields)*

### Slice 3 — R-F05-S3 — Banded-monitor VM + XAML
*(same fields; must record the exact comma-rejection message created)*

### Slice 4 — R-F01-S1 — Day-of-month + money row in the Application layer
*(same fields)*

### Slice 5 — R-F01-S2 — Day-of-month + money row in the UI
*(same fields)*

### Slice 6 — R-A04-S1 — First-registration guard in the Application handler
*(same fields)*

### Slice 7 — R-A04-S2 — First-registration guard in the UI
*(same fields)*

### Slice 8 — R-A04-S3 — Outside-lab note on the profile printed report
*(same fields)*

---

## Current Status

| Field | Value |
|---|---|
| Slices complete | **0 of 8** |
| Current slice | none — Slice 1 not started |
| Baseline recorded at G0 | **NO — blocking** |
| Last commit made by the agent | none |
| Pushes made by the agent | **0 (and it must stay 0 — SD-1)** |
| Migrations created / edited / applied | **0** |
| Files outside the `B-01.md` §C inventory modified | **0** |
| Blocking issues | none |

---

## Stop Report template

*(Fill in and emit if any Stop Rule triggers. Then wait.)*

### STOP REPORT — B-01 halted at **Step 0**, before Slice 1

- **Slice / stage where it stopped:** **Step 0 (pin the commit) — pre-Slice-1 gate. No slice was started. Zero source files were read for implementation, zero were modified, zero commits were made.**
- **What failed:** the second Step 0 acceptance check. The prompt requires `git status --short` to **print nothing**; it prints 12 untracked paths. Stop Rule #1 («`git status --short` is not clean at the start of a slice») therefore halts the loop before the Baseline table can be completed.
- **How many consecutive times the same failure occurred:** **1** — this is a deliberate, single halt, not a retry loop. No corrective attempt was made, because the only fixes available all require a forbidden git write (`add`/`commit`/`clean`/`stash` are all outside the one-commit-per-slice authorization, and this is not even a slice).
- **Evidence from each attempt:**

```
$ cd "/c/Users/LAP LINK/source/repos/Top-Lab" && git rev-parse HEAD
7a2cfb505acd8f6bdac4e0b49c8059d95d19a757          ← PASS

$ git log --oneline -1
7a2cfb5  الإستعداد للرحلة                          ← PASS (subject matches the pin)

$ git status --short
?? Docs/Hermes/Batch-1-Plan.md
?? Docs/OpenCode/B-01-Execution-Prompt.md
?? Docs/OpenCode/B-01-memory.md
?? Docs/OpenCode/B-01.md
?? "Docs/Remaining Tasks Folder/Cross-comparison.md"
?? "Docs/Remaining Tasks Folder/RLS_Learn_Enhanced.pdf"
?? "Docs/Remaining Tasks Folder/RL_Show_Enhanced.pdf"
?? "Docs/Remaining Tasks Folder/باقي ..." (5 further Arabic-named .md files)   ← FAIL

$ git diff HEAD --stat
(empty)                          ← every TRACKED file is byte-identical to the pin

$ git stash list
(empty)                          ← no hidden stashed work

$ git check-ignore -v src/TopLab.Domain/bin/x
.gitignore:4:[Bb]in/             ← bin/ and obj/ ARE ignored, so the ?? are not build residue
```

- **What the plan assumed, and what the code actually shows:** the plan assumed a freshly cloned repository, so `git status --short` would be empty. The actual working copy is a **pre-existing, long-lived local clone** at the correct pin with correct `origin/main` tracking, `main == HEAD == 7a2cfb5`. The tracked tree is pristine, so **no tracked file is wrong and no slice's starting state is compromised.** The only deviation is 12 **untracked documentation files** under `Docs/` — the working material of the owner and the prior agents (`B-01.md` and `B-01-memory.md` themselves live here, and the prompt instructs me to *place them in the clone at `Docs/OpenCode/`*, which is exactly where they already are). These are inputs, not defects.
- **What is blocked as a result:** Slice 1 Stage 1 cannot start, so the **Agent-measured baseline is intentionally left unfilled** — build warning/error counts and all five test projects' passed/failed/total remain blank in the table above. The prompt states «Until that table is filled in, no slice may start», so measuring now would not unblock me either; the halt is upstream of the measurement. I did **not** run a build or any test project: a `dotnet build` writes `bin/`+`obj/` across the solution and is not part of Step 0, and doing it while the tree is already in violation would add noise to the very check that failed.
- **What the agent recommends, without doing it:** the owner picks **one** of these three, all of which require a git write I am forbidden to perform:
  1. **Treat the untracked `Docs/` files as pre-existing and in-scope for the batch.** The cleanest reading, because the prompt itself orders me to place `B-01.md`/`B-01-memory.md` at `Docs/OpenCode/` — they are already there. The rule would need restating as «`git status --short` must show no changes to **tracked** files, and the only permitted untracked paths are the `Docs/` inputs and the memory file I am told to update».
  2. **Commit or ignore the `Docs/` material** (e.g. add `Docs/Remaining Tasks Folder/` and the plan drafts to `.gitignore`, or make one owner commit), after which the tree is genuinely clean and Step 0 passes verbatim.
  3. **Proceed on a fresh clone** where `Docs/OpenCode/B-01.md` and `B-01-memory.md` are added on top of the pinned commit, accepting that `git status --short` will then still show those two files as untracked until the owner commits them — i.e. this option still needs rule (1) or (2) to be workable.
  
  I recommend **option 1**, and additionally that the owner create a `global.json` pinning `8.0.425`, because of the unrelated blocker below.

### Secondary finding — the required .NET 8 SDK is not the default SDK (not yet a Stop Rule, but it will break Stage 6)

```
$ dotnet --version
9.0.318
$ dotnet --list-sdks
8.0.425 [C:\Program Files\dotnet\sdk]
9.0.318 [C:\Program Files\dotnet\sdk]
$ cat global.json
(none — the repository does not pin an SDK)
```

The environment rules require the **.NET 8 SDK**, explicitly «not 9.x». There is **no `global.json`**, so every `dotnet build`/`restore`/`test` in this batch would resolve to **9.0.318**, and `TopLab.Presentation` targets `net8.0-windows`. Pinning `8.0.425` requires either setting `DOTNET_ROOT` to an 8.x-only install or adding a `global.json` — and a `global.json` is a **new file outside the `B-01.md` §C inventory of 28 files**, which the prompt forbids me from touching. This must be resolved by the owner (or by explicit authorization to add the one file), otherwise Stage 6 of Slice 1 is a guaranteed `NETSDK`/`NU` failure that the loop's own rules would have me report rather than fix.

**Other environment facts recorded for the Baseline table** (measured, read-only, not yet a baseline):

| Item | Value |
|---|---|
| Docker | **unavailable** → `TopLab.Persistence.Tests` will self-skip via `DockerFactAttribute`; to be reported as **skipped, never as a pass** |
| Arabic-capable fonts present | `arial.ttf` `arialbi.ttf` (Arabic), `tahoma`, `segoeui`, `calibri`, `cour`, `times` (full families present) — the Linux-host font failures in the verification env. are **not expected here**; Infrastructure/Presentation are expected to pass fully, to be **measured** |

- **Slice / stage where it stopped:** 
- **What failed:** 
- **How many consecutive times the same failure occurred:** 
- **Evidence from each attempt:** 
- **What the plan assumed, and what the code actually shows:** 
- **What is blocked as a result:** 
- **What the agent recommends, without doing it:** 

**Stop Rules — any one halts the loop immediately. Record it here and wait.**

- The pinned commit cannot be reached, or `git status --short` is not clean at the start of a slice.
- The code differs from the plan in a way that changes what the slice should do.
- A slice appears to require a migration, a schema change, or an edit to any file outside the `B-01.md` §C inventory.
- The build or the tests go red and cannot be restored within the slice's scope.
- The same failure occurs **5 consecutive times**.
- `ResultPrintCoordinator` is about to receive a `bool` in the 10th position of `CombinedReportLineDto` — that is a compile error; stop and re-read SD-12.
- `FakeSender.cs` would have to be modified because the new `ProfileReportDto` member was made required — stop and re-read SD-13.
- The application is about to be launched, or a migration is about to be created, edited or applied. Both are forbidden.
- Any ambiguity not already settled in the plan — record it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية» and do **not** decide it yourself.

---

## Owner manual checks (after the batch — the agent cannot perform these)

- [ ] Launch the application and open **الإحصائيات → نطاق النتائج**: the picker populates, a band returns rows, `3.5` works, `3,5` is rejected with a readable message, and the PDF lands where chosen.
- [ ] Confirm the disabled `مسح الكل` button on a returning patient's second visit, and its tooltip-free but clear behaviour on a first visit.
- [ ] Print a specialised-profile report for a sample marked "خارج المعمل" and confirm the note `العينة أُخذت خارج المعمل` now appears.
- [ ] Confirm the combined clinical report, the history report and the patient PDF export are **unchanged**.
- [ ] Review the **eight** commits, then push.
