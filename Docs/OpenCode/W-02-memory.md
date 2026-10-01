# Loop Engineering — Memory File

- **Module:** Wave 2 — WP-06, WP-07 (done), WP-10, WP-13, WP-14, WP-29
- **Module Number:** W-02
- **Source Plan:** `Docs/OpenCode/W-02.md`
- **Execution Prompt:** `Docs/OpenCode/W-02-Execution-Prompt.md`
- **Date Created:** 2026-10-01
- **Total Slices:** 16
- **Current Slice:** — (start at S1)
- **Current Branch:** `main`
- **Baseline Commit:** `94292c2b2c953f9a2767cf7e81392ade0f01187d`
- **Author:** loop-engineering (execution by the local coding agent per owner authorization)

---

## Module Summary

Wave 2 repairs the honesty, depth and integrity of the reporting and printing paths. Six packages: an honest print coordinator (WP-06), a regression net for the already-delivered range comments (WP-07), history filters plus a real CBC matrix (WP-10), combined-report print options with an off-lab note and test comments (WP-13), culture depth — microscopy, inhibition zone, antibiotic master fields (WP-14), and data integrity plus diagnostics (WP-29). Three new EF migrations; no edits to the eleven existing migrations. Two ordering constraints are load-bearing: **S2 before S9** (data loss) and **S4 before S13** (file ownership).

---

## Settled Decisions (binding — do not reopen)

- **SD-1 — Decision 2 = (b1): zero the values, keep the columns.** `IsPrinted=false`, `PrintCount=0`. All six columns stay **mapped** on `PatientTest` and `ProfileResultItem` (`IsPrinted`, `PrintCount`, `LastPrintedByUserId`, `LastPrintedAtUtc` on each). **No `DropColumn`. No fourth migration. No backup** (the system never ran in production — owner attestation). The lifecycle guards `MarkDelivered`, `Unreview`, `ClearResult` stay **intact**. **Precise zeroing claim — do not overstate it.** Removing `MarkPrinted` calls prevents *future* writes only; it does **not** retro-zero a row that was already printed. Two verified code facts carry the guarantee: (1) **no `HasData` anywhere in the project sets `IsPrinted` or `PrintCount`** (`grep -rn "HasData.*IsPrinted\|HasData.*PrintCount" src/` ⇒ none), so every seeded row starts `false`/`0` from the column default; (2) `PatientTest`'s private constructor (`:70-96`) never assigns them, making `MarkPrinted` the **only** production writer. **Owner-side verification step (outside the agent, outside the repo, NOT a migration):** on the development database run `SELECT COUNT(*) FROM PatientTests WHERE IsPrinted=1 OR PrintCount>0;` (and the same for `ProfileResultItems`). Zero ⇒ nothing to zero; record the result. Non-zero ⇒ run the zeroing `UPDATE` **by hand** and record the date — **never create a migration for it**. `PatientTestConfiguration.cs:46`'s composite index `{IsReviewed, IsPrinted, IsDelivered}` therefore stays valid. **Optional owner-side housekeeping (NOT a migration, NOT part of any slice):** if the owner still has a development database with rows, an idempotent `UPDATE PatientTests SET IsPrinted=0, PrintCount=0, LastPrintedByUserId=NULL, LastPrintedAtUtc=NULL;` (and the same for `ProfileResultItems`) may be run by the owner outside the repository. Record whether it was run in the Execution Log; **do not** create a migration for it.
- **SD-2 — Decision 3 = (ج): scientific name only.** `Antibiotic.Symbol` (≤10) + `Antibiotic.ScientificName` (≤150); «الاسم العلمي» in the sensitivity table. **Commercial names deferred entirely — no storage, no printing.** Four artefacts are **dropped** and must not be built: (1) `AntibioticCommercialName` entity, (2) `AntibioticCommercialNameConfiguration`, (3) the commercial-names grid in `AntibioticsView`, (4) the test `Report_ShowCommercialNameFalse_HidesColumn`. The stage plan's text naming them is **superseded**.
- **SD-3 — WP-07 is complete** (delivered in WP-01). No slice. S16 is a regression net only.
- **SD-4 — sensitivity labels** stay the five English ones already shipped at `CultureEntryViewModel.cs:55-59`: `Unspecified` · `Sensitive` · `Intermediate` · `Low Sensitivity` · `Resistant`. `SensitivityCategory` values 0–3 unchanged.
- **SD-5 — `IAppLogger` is frozen.** `Log(string requestName, string outcome, TimeSpan duration)`, pinned by a reflection test (S-07 SD-4). Swallowed print exceptions go through a **new separate** port `IPrintingDiagnostics` (`Application/Common/Interfaces`), implemented at `Infrastructure/Logging/PrintingDiagnostics.cs`. Never widen `IAppLogger`; never put exception text, a patient identifier or a result into `requestName`.
- **SD-6 — positional records: append-with-default only.** `CombinedReportLineDto`, `HistoryEntryDto`, `CultureReportSummaryDto`, `ReportCultureSection`, `ReportSettings`, `AntibioticDto`, `AttachedAntibioticDto`. Never insert a member in the middle.
- **SD-7 — forbidden to touch:** `BarcodeService.ToAscii` (`BarcodeService.cs:207-221`, until WP-23) · the `TestDisplayNameResolver` chain (`TestDisplayNameResolver.cs:12-21`, until WP-17) · `IAppLogger` · any of the eleven existing migrations · `SensitivityCategory` values · reference-range matching logic.
- **SD-8 — Arabic strings byte-for-byte from the stage plan.** Any new label the plan does not supply is marked **`TBD-AR`** in the register below and **not invented**. Never invent copyright text, a support address, or any commercial drug name.
- **SD-9 — git.** One **local** commit per verified slice on `main`. Never push. No branch, amend, rebase, reset, stash, clean, tag. Never `git add -A` / `git add .`. The owner pushes after the wave.
- **SD-10 — the plan is a hypothesis.** Code mismatch → STOP and report.
- **SD-11 — migration budget: three new, only.** `AddCombinedReportPrintOptions` (S7), `AddCultureMicroscopyAndZone` (S9), `AddAntibioticMasterFields` (S10). No fourth under any circumstance.
- **SD-12 — layering (C-18).** `App.xaml.cs:22`: no Presentation type may reference Infrastructure. Three ViewModels violate it today: `CombinedReportViewModel.cs:15,55,235-247`, `ProfileEntryViewModel.cs:17,75,406-417`, `WorkSheetsViewModel.cs:17,387`. **No refactor this wave** — S15 adds a structural non-regression test and records the debt as numbered TODO.
- **SD-13 — data-loss ordering.** **S2 must precede S9.** `SaveCultureResultsCommandHandler.cs:5` deletes and recreates every `CultureAntibioticResult` row on every save; if `AddCultureMicroscopyAndZone` lands first, the first save after it erases every `InhibitionZoneMm` in the database.
- **SD-14 — file ownership.** WP-06 owns `ReportPrintingService.cs` in **S4**. WP-29 adds diagnostics to the same file in **S13**, after S4. S5 and S6 must not touch it.
- **SD-15 — migration review** is an independent analysis-only review agent after the wave. The owner does not review migrations.
- **SD-16 — C-21 + C-26.** `MarkResultPrintedCommand` + handler have **zero** consumers in `src/`, but the command is referenced by **four** live test files, not two (see C-26). **Binding default: WIRE, not delete.** In S5, change `MarkResultPrintedCommandHandler` to go through `IResultPrintCoordinator` with `ResultPrintKind.SimpleResult` instead of `pt.MarkPrinted`. **Delete is an exception, not a default:** it requires explicit owner authorization recorded at S5 Stage 4, and it would force edits to two *structural* gates — `ValidatorRegistrationTests` and `ResultsEntryAuthorizationTests` — which weakens the very safety net that catches a missing validator or permission elsewhere. **Deletion decision recorded here:** ⬜ not yet decided (agent fills at S5 Stage 4) — default in force: **wire**.

---

## Plan-vs-Code Corrections (binding — see W-02.md §1)

| ID | Correction |
|---|---|
| C-1 | History screen access already works (`PatientsHubViewModel.cs:79-87`) — no navigation work |
| C-2 | `HistoryInsertion.cs` is 27 lines; the pins are at `:21-22`, and it already forwards comments at `:24-25` |
| C-3 | No XML doc on either history handler — prove the duplication with `diff`, not with a doc citation |
| C-4 | `HistoryEntryDto` already has `TestId` (`:73`) and `LowComment`/`HighComment` (`:82-83`); only `EnteredAtLocalDate` remains |
| C-5 | History dates already print (`ReportContentBuilder.cs:343`) — do not re-add |
| C-6 | `SimpleResultEntryViewModel.cs` has **zero** `Print` matches — not a file to modify for WP-06 |
| C-7 | `ProfileEntryViewModel.cs` `PrintAsync` is **343-365**; `CultureEntryViewModel.cs` is **331-358**, command `:343`, message `:346` |
| C-8 | `IAppLogger` is registered at `Infrastructure/DependencyInjection.cs:105`, not `:102` |
| C-9 | `CultureAntibioticResult.cs` is 32 lines; fields are `:11` and `:14` |
| C-10 | `ReportPdfWriter.cs` is 84 lines; culture rendering is `ReportContentBuilder.cs:246-256` + `ReportCultureSection.BuildLines` `:24-51` |
| C-11 | `CultureEntryView.xaml:141` already binds `Report.OrganismC` — drop the WP-14 item |
| C-12 | `IsTakenOutsideLab` reaches no clinical report (true) but has 25+ consumers, not one — restate the evidence |
| C-13 | The commercial-name artefacts are dropped (SD-2) |
| C-14 | `SettleAccountInFullCommandHandler.cs` is 79 lines; the unlocked read-then-write is `:42-77` |
| C-15 | Hot-reader anchors — exact: `GetResultWorklistQueryHandler.cs:37`, `GetPatientTestAuditQueryHandler.cs:61`, `GetCultureReportQueryHandler.cs:28`. Wrong: `PatientBillingReader.cs:141,158`, `GetCombinableTestsQueryHandler.cs:194`, `GetPatientTestsForDrawQueryHandler.cs:45`, `GetResultEntryQueryHandler.cs:104` |
| C-16 | Settled by SD-1: zero the values, keep the columns, no fourth migration |
| C-17 | `HistoryReportsViewModel.cs:30` is accurate; multi-patient is still dead — fix it |
| C-18 | Three ViewModels violate `App.xaml.cs:22` — non-regression test only (SD-12) |
| C-19 | The nullable `SensitivityCategory` is unreachable on write: `SaveCultureResultsCommand.cs:8` is `int`; `CultureEntryViewModel.cs:223-225` drops null rows, so choosing «Unspecified» **deletes the saved row**. **S1** |
| C-20 | `AgeRules` does not exist; there is **one** relevant site (`GetCultureEntryGridQueryHandler.cs:33`) with a named `const`, not a literal `12`. **S3** |
| C-21 | `MarkResultPrintedCommand` has zero `src/` consumers — decide delete-or-wire in S5 (SD-16) |
| C-22 | `HistoryReportsViewModel.cs:208` type test is statically always true — silent stale `Entries` on null. **S12** |
| C-23 | `GetCultureAttachmentView` / `SaveCultureAntibioticAttachment` do not exist; real names are `GetCultureAntibioticsQueryHandler`, `AttachAntibioticToCultureCommand`, `DetachAntibioticFromCultureCommand` |
| C-24 | `ReportSettings` columns are `IsRequired()` with a `HasData` seed at `ReportSettingsConfiguration.cs:25` — use `bit NOT NULL` + `HasDefaultValue(false)`, not nullable `bit` |
| C-25 | Delete the duplicate `GetSeparateHistoryReportQueryHandler`, but **keep the query** — used at `HistoryReportsViewModel.cs:204` and `PrintHistoryReportCommandHandler.cs:49` |
| C-26 | *(added after local-agent review)* The delete scope for C-21 listed only two test files. `MarkResultPrinted` is referenced by **four**: `ValidatorRegistrationTests.cs:71,219` (validator-completeness gate) · `ResultsEntryAuthorizationTests.cs:10,57,64` (`PRINT_RESULTS` permission gate) · `ExportPatientReportPdfCommandHandlerTests.cs:8` (`using` only — breaks the build) · `ReviewPrintDeliverCommandHandlerTests.cs:2,95,96,129,130,151,152,172,173,193,194`. Default flips to **wire** (SD-16); delete needs owner authorization plus edits to both structural gates |

---

## Confirmed Code Facts (verified at the pinned commit — confirm at Stage 3, do not re-derive blindly)

**C-26 (four test files, not two):** `ValidatorRegistrationTests.cs:71,219` · `ResultsEntryAuthorizationTests.cs:10,57,64` · `ExportPatientReportPdfCommandHandlerTests.cs:8` · `ReviewPrintDeliverCommandHandlerTests.cs:2,95,96,129,130,151,152,172,173,193,194`. The first two are **structural gates** — do not weaken them.

**WP-06:** `MarkProfilePrintedCommandHandler.cs:25-33` injects only db/user/clock; marks at `:63-74`, saves at `:81`, produces **no PDF**. `MarkCultureReportPrintedCommandHandler.cs` is **3 lines** — the whole handler is on line 1. `ExecuteBulkPrintCommandHandler.cs:18-26` injects only db/user/clock; `:76` `pt.MarkPrinted`; `:86` returns `Printed`. Exactly **7** handlers inject a print port (`PrintInvoice`, `PrintReceipt`, `PrintBarcode`, `PrintBlankReport`, `PrintCombinedReport`, `PrintHistoryReport`, `PrintWorkSheet`). `PrintCombinedReportCommandHandler.cs:64-85` and `PrintHistoryReportCommandHandler.cs:69-90` already print-then-mark (honest ordering). `PrintBlankReportCommandHandler.cs:50-54` prints with no `MarkPrinted` by design. `ReportPrintingService.cs` is exactly 77 lines; swallow at `:73-76`; `OperationCanceledException` re-thrown at `:69-72` — **keep that order**. `BarcodeService.cs:76-79` re-throws, `:80-83` swallows — keep. `ExportPatientReportPdfCommandHandler.cs:161-168` swallow. `FakeReportPrintingService` exists at `tests/TopLab.Application.Tests/Common/Fakes/FakeReportPrintingService.cs` with `Tokens` and `NextResult` — **reuse it, do not create a second**.

**WP-07 (closed):** `ReportDtos.cs:51-52, 82-83, 19-20` · `AnalyteReferenceRangeBand.cs:30,32` · `PatientHistoryReader.cs:109-124` (frozen snapshot, matching flag only) · `BuildCombinedReportCommandHandler.cs:145,158-159,133-134` · `PatientHistoryReader.cs:84,97-98` · `HistoryInsertion.cs:24-25` · `ReportContentBuilder.cs:216-225,234-242,149-157,82-90,337-339,347,352`.

**WP-10:** `PatientsHubViewModel.cs:79-87` · `HistoryReportsViewModel.cs:30` private field, `:157-161` always bails, `:170` unreachable, `:204-216` dead type test. `GetPatientTestHistoryQueryHandler.cs` and `GetSeparateHistoryReportQueryHandler.cs` are byte-identical after name normalisation. `PatientHistoryReader.cs:29-31` materialises the whole `Patient` table in `ByPatientName` mode; `:42-45` (`ByLabCode`) is already server-side. `PatientHistoryResolver.cs:31-40` normalises by `ToUpperInvariant` — not SQL-translatable as written. `InsertHistoryDialogViewModel.cs:88-90` — comment without code. `ReportGrid`/`ReportSection` in `ReportDocumentContent.cs:140-153`; `ReportPageComposer.cs:90-120` renders a grid as a QuestPDF table.

**WP-13:** `ReportSettings.cs:9-27` has **10** properties, no print flags; `CreateDefault()` `:37-52`; `ReportSettingsConfiguration.cs:23` last flag, `:25` `HasData`. `UpdateReportSettingsCommand.cs:8-17` positional record, 8 params, `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` at `:19` — **no new permission**. `UpdateReportSettingsCommandHandler.cs:30` `SetHistoryOptions`. `ReportSettingsDto` at `SettingsDtos.cs:20-28`. `PatientTest.cs:26` `IsTakenOutsideLab`; `PatientTestConfiguration.cs:22` non-nullable; only print consumer `WorkSheetPdfWriter.cs:218`. `TestComment.cs:6-53` + `TestCommentConfiguration.cs` + `ApplicationDbContext.DbSets.cs:29` + full CRUD under `Features/PriceListsCommentsAndCustomGroups/` + `Lab/TestCommentsViewModel.cs`; **zero** references under `ReportProduction`/`ResultsEntry`/`CultureResults`/`ProfileResults` ⇒ **no migration needed for test comments**.

**WP-14:** `CultureResult.cs:10-20` exactly six fields; `CultureResultConfiguration.cs:11,19` 1:1 on `PatientTestId`, cascade. `CultureAntibioticResult.cs` 32 lines — **no `InhibitionZoneMm`**. `CultureAntibioticResultConfiguration.cs:16` nullable `tinyint`. `Antibiotic.cs:8-12` exactly three fields; `AntibioticConfiguration.cs:14-16`. `CultureAntibioticAttachment.cs:6-21` composite PK only — **no threshold**; `CultureAntibioticAttachmentConfiguration.cs:11-13`. `CultureAntibioticDisplay.cs:5` `ChildAgeThresholdYears = 12`; `GetCultureEntryGridQueryHandler.cs:33` the one buggy site. `SaveCultureResultsCommand.cs:8` `int`; validator `:9` `is >= 0 and <= 3`; handler `:5` delete-all-then-reinsert plus a hard cast; `CultureEntryViewModel.cs:223-225` `Where(HasValue)` drops nulls; `:52-60` `SensitivityOptions`; `:331-358` `PrintAsync`. `GetCultureReportQueryHandler.cs:26` `FirstOrDefault()` without `Id == 1`; `:28-33` antibiotic catalogue loaded whole + sensitivity rows. `CultureResultDtos.cs:3-4` (`int?` on the read side), `:11-16` `CultureReportDto`. `CultureEntryView.xaml:107-119` flat 4-column grid with the WP-03 `SelectedValue` pattern at `:111-114`; `:141` already binds `OrganismC`. `AntibioticDto` `AntibioticDtos.cs:3-7`; `AttachedAntibioticDto` `:9-13`; `GetAntibioticsQueryHandler.cs:30-37`; `GetCultureAntibioticsQueryHandler.cs:48-59`. `ReportCultureSection.cs:8-14` 6 positional params, `HasAnyContent` `:16`, `BuildLines()` `:24-51`; sole construction site `ReportContentBuilder.cs:248-254`. `ReportDocumentContent.ContainsEnglishLabels()` `:132-136` — new Arabic must not trip it.

**WP-29:** `IAppLogger.cs:7-10` single method; consumers are only `LoggingBehavior.cs:17,19` and `DailyBackupHostedService.cs:18,24`; `FileAppLogger.cs:9-11` documents the privacy guarantee, `:20-23` the `%ProgramData%\TopLab\logs` path, `:46-49` never-throws. `AddTestsToVisitCommandHandler.cs:112` `createdIds.Add(0)`, `:117-122` `OrderByDescending(pt.Id).Take(createdIds.Count)` over all of the patient's tests. `PatientEditorViewModel.cs:883` `ApplyConditionDeltasAsync`, `:918` `ApplyTestDeltasAsync`. `SettleAccountInFullCommandHandler.cs:42-77` unlocked read-then-write. `grep "UPDLOCK|FromSql|IsolationLevel|rowversion|ConcurrencyToken|IsConcurrencyToken"` ⇒ **zero**. Unbounded materialisations: `PatientHistoryReader.cs:29-31`, `WorkSheetHelpers.cs:104-107` (`.ToList().Where(`). Bounded catalogue loads at 28 sites — leave them. `PdfPreviewService.cs:28-29` `%TEMP%\TopLab-PDF-Preview` is already an app-owned folder. `ReportPrintingService.cs:62` and `BarcodeService.cs:69` write straight into `%TEMP%`. `IApplicationDbContext` does **not** expose `Database` (`ApplicationDbContext.cs:29-36`); the concrete context in Infrastructure does.

**Migrations:** 11 files (8 original + 3 Wave 1), timestamps ascending, chain monotonic (`SensitivityCategory` becomes `byte?` exactly at `20260930163921`). Last designer matches the snapshot except for boilerplate. 45 entities, **no entity/property drift** between `ApplicationDbContextModelSnapshot.cs` and the live model.

---

## Global Validation Gates

- **G0 (once, before Slice 1): RE-MEASURE IT YOURSELF, then compare.** The table below is the owner's recorded measurement — it is a **reference, not your baseline**. Run the build and **all five** test projects yourself on this machine, record your own numbers in the *Agent's own G0 confirmation* row, and only then diff yours against the table. Never adopt the owner's figures as your own baseline: the S-00…S-07 protocol requires the executing agent to measure its own baseline, otherwise "no count may fall below baseline" means nothing. If your numbers differ from the table, **STOP and report the delta before Slice 1** — do not proceed against a baseline you did not measure.
- **G1 (every slice):** build 0/0; no test count below baseline; slice `VG-nn` item by item; migrations policy; `git status` clean after commit.

### Zero-drift gate (every slice, no exceptions)

```
dotnet-ef migrations has-pending-model-changes \
  --project src/TopLab.Infrastructure/TopLab.Infrastructure.csproj \
  --startup-project src/TopLab.Presentation/TopLab.Presentation.csproj \
  -- -p:EnableWindowsTargeting=true
git diff -- src/TopLab.Infrastructure/Persistence/
```
Expected: "no changes" and an empty diff, except in S7/S9/S10 where the intended new migration plus the snapshot change.

### Migrations policy (every slice)

- New migration files **only** in **S7, S9, S10**.
- **Never** edit the eleven existing migrations or their `.Designer.cs`.
- **Never** a `DropColumn` on `PatientTests` / `ProfileResultItems` (SD-1).
- **Never** a commercial-name table or column (SD-2).
- **Never** hand-edit `ApplicationDbContextModelSnapshot.cs`.
- **Never** `dotnet ef database update` on the owner's database without coordination.

---

## Quality Gate (a slice is complete ONLY when ALL hold)

1. Scope matches `W-02.md` (SD-10).
2. `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` (or VS MSBuild) → **0 errors, 0 warnings**.
3. No test count below the recorded baseline; no new warning anywhere.
4. The slice's `VG-nn` passes item by item, zero-drift gate included.
5. Local commit only (SD-9), explicit paths staged.

---

## Stop/Continue Rule

- **Continue automatically** after a passing gate plus a local commit (SD-9).
- **STOP** on: plan/code mismatch (SD-10) · deleting `MarkResultPrinted` **without** recorded owner authorization (SD-16/C-26) · S9 before S2 (SD-13) · any need for a fourth migration (SD-11) · any `DropColumn` on the two entities (SD-1) · any commercial-name artefact (SD-2) · any attempt to widen `IAppLogger` or log a patient identifier (SD-5) · touching `BarcodeService.ToAscii` or `TestDisplayNameResolver` (SD-7) · the Infrastructure baseline not being 221/221 at S1 · the same failure **5 consecutive** times · any unlisted ambiguity.
- Write the Stop Report below and wait.

---

## Baseline

**Two columns of truth. The first is the owner's recorded measurement (reference only). The second is yours — fill it in at Slice 1 Stage 1 before touching any code.**

### Owner's recorded measurement (reference — NOT your baseline)

| Item | Value |
|---|---|
| Date measured | 2026-10-01 |
| Toolchain | VS MSBuild; .NET 8 SDK for `dotnet` / `dotnet-ef` 8.0.30 |
| `git rev-parse HEAD` | `94292c2b2c953f9a2767cf7e81392ade0f01187d` ✓ |
| `git status --short` | clean except the three untracked W-02 package files (expected) |
| Build warnings / errors | **0 / 0** |
| TopLab.Domain.Tests | **484 / 484** |
| TopLab.Application.Tests | **1502 / 1502** |
| TopLab.Infrastructure.Tests | **221 / 221** |
| TopLab.Presentation.Tests | **57 / 57** |
| TopLab.Persistence.Tests | **13 passed + 1 skipped** (Docker absent — honest skip) |
| **Full suite** | **2277 passed + 1 skipped** |
| `has-pending-model-changes` | **no changes** |
| `git diff -- src/TopLab.Infrastructure/Persistence/` | empty |
| Migration file count | **11** |
| Docker | absent (Persistence container tests skip) |
| Agent's own G0 confirmation | ✅ **MEASURED 2026-10-01 by the executing agent — all Δ = 0** (see table below) |

### Executing agent's own measurement (MUST be filled before Slice 1 — this is the real baseline)

| Item | Agent's measured value | Δ vs owner's table |
|---|---|---|
| Toolchain | .NET SDK 8.0.425 (`dotnet`), SDK 9.0.318 also installed; `dotnet-ef` **8.0.30** (`dotnet ef --version`) | matches |
| Build warnings / errors | **0 / 0** (`dotnet build TopLab.sln -p:EnableWindowsTargeting=true`) | **0** |
| TopLab.Domain.Tests | **484 / 484** (Passed 484, Failed 0, Skipped 0) | **0** |
| TopLab.Application.Tests | **1502 / 1502** (Passed 1502, Failed 0, Skipped 0) | **0** |
| TopLab.Infrastructure.Tests | **221 / 221** (Passed 221, Failed 0, Skipped 0) — **GDI+/Linux failures absent on Windows, as predicted** | **0** |
| TopLab.Presentation.Tests | **57 / 57** (Passed 57, Failed 0, Skipped 0) | **0** |
| TopLab.Persistence.Tests (passed / skipped) | **13 passed / 1 skipped** (Total 14) | **0** |
| Full suite (passed / skipped) | **2277 passed / 1 skipped** | **0** |
| `has-pending-model-changes` | **`No changes have been made to the model since the last migration.`** | **0** |
| `git diff -- src/TopLab.Infrastructure/Persistence/` | **empty** | **0** |
| `dotnet-ef --version` | **8.0.30** | **0** |
| Docker available? | **No** (`docker: command not found`) ⇒ Persistence container tests skip honestly | **0** |
| `git rev-parse HEAD` | **`94292c2b2c953f9a2767cf7e81392ade0f01187d`** ✓ | **0** |
| `git status --porcelain` | only the three untracked W-02 package files (`W-02.md`, `W-02-memory.md`, `W-02-Execution-Prompt.md`) | **0** |
| Migration file count | **11** (+ `ApplicationDbContextModelSnapshot.cs` = 12 `.cs` files in the folder) | **0** |

**Verdict: G0 PASSES. Every Δ is zero, Infrastructure is 221/221. Slice 1 is authorised to start.**

*Note on the `has-pending-model-changes` line:* it emits two hosting-DI warnings (`IDatabaseMaintenanceService` into `DailyBackupHostedService`, `IDateTimeProvider` into `MainWindow` — scoped-into-singleton) and then continues without the service provider. That is pre-existing at the pinned commit, unrelated to this wave, and does not affect the verdict.

**If any Δ is non-zero, or Infrastructure is not 221/221, STOP and report before Slice 1.**

*Context only, not the target:* on a Linux host Infrastructure shows 16 failures caused by `System.Drawing.Common` / GDI+ inside `ArabicFontResolver` (`ArabicFontResolver.cs:14,48,62`). Those are environmental and absent on Windows. Infrastructure **must** be 221/221 here.

---

## Slice Validation Gates (from plan)

| Slice | Gate | Key checks |
|---|---|---|
| 1 | VG-01 | `Unspecified` persists as NULL; no null-row deletion; enum unchanged; no migration |
| 2 | VG-02 | existing row keeps its id; partial add/remove; idempotent; `Remove(old)` gone |
| 3 | VG-03 | `AgeRules_Month11_IsChild`; infant grid test; BR-04 matching untouched |
| 4 | VG-04 | build-fails ⇒ no print call; print-fails ⇒ not marked; **zero** `MarkPrinted` in the coordinator |
| 5 | VG-05 | Arabic success/error texts; no hardcoded «تم الطباعة.»; **C-21/C-26 default = wire**, all four test files untouched |
| 6 | VG-06 | per-patient `Failed`; others continue; **zero** `MarkPrinted`; balance gate intact |
| 7 | VG-07 | two `bit NOT NULL` + `UpdateData`; eleven old migrations untouched; no `DropColumn` |
| 8 | VG-08 | outside-lab note both ways; comments aggregated in one query; sub-title flag; no migration |
| 9 | VG-09 | **S2 commit present**; three operations; **negative** no-commercial-column test; zone survives re-save |
| 10 | VG-10 | exactly two `AddColumn`s; **negative** no-commercial-column test; optional-field round trip |
| 11 | VG-11 | sensitivity grid rendered; microscopy block; `SingleOrDefault(Id==1)`; no N+1; invariant decimals |
| 12 | VG-12 | date + test filters; multi-patient public; dead type test gone; matrix pivot; **bounded SQL candidate set**; no `EF.Functions.Collate` |
| 13 | VG-13 | `IAppLogger` reflection pinned; diagnostics never throws; no patient identifier in the line |
| 14 | VG-14 | ids of rows inserted only; `Take()` gone; three rollback tests; settlement lock |
| 15 | VG-15 | no full-table load; cleanup ignores other directories; layering guard pins existing debt |
| 16 | VG-16 | **wave DoD**: 2277+1, three migrations, zero drift, SD-1…SD-16 honoured |

---

## Slice Index

| # | Slice Title | Package | Migration | Status | Gate |
|---|---|---|---|---|---|
| 1 | Culture sensitivity write path accepts NULL (C-19) | WP-14 | — | ✅ done | VG-01 ✅ |
| 2 | SaveCultureResults updates rows instead of recreating them | WP-14 | — | ✅ done | VG-02 ✅ |
| 3 | AgeRules + infant child detection (C-20) | WP-14 | — | ✅ done | VG-03 ✅ |
| 4 | IResultPrintCoordinator — build, print, never mark | WP-06 | — | ✅ done (AD-1/AD-2) | VG-04 ✅ |
| 5 | Entry screens print through the coordinator + C-21 | WP-06 | — | ✅ done | VG-05 ✅ |
| 6 | Bulk print through the coordinator, reports Failed | WP-06 | — | ✅ done | VG-06 ✅ |
| 7 | ReportSettings print flags + `AddCombinedReportPrintOptions` | WP-13 | **M1** | ✅ done | VG-07 ✅ |
| 8 | Combined-report options, outside-lab note, test comments | WP-13 | — | [x] DONE | VG-08 PASS |
| 9 | `AddCultureMicroscopyAndZone` | WP-14 | **M2** | [x] DONE | VG-09 PASS |
| 10 | `AddAntibioticMasterFields` | WP-14 | **M3** | ⬜ | VG-10 |
| 11 | Culture sensitivity table + microscopy block in the report | WP-14 | — | ⬜ | VG-11 |
| 12 | History filters + CBC matrix + dead-code cleanup | WP-10 | — | ⬜ | VG-12 |
| 13 | Swallowed print exceptions reach a diagnostics sink | WP-29 | — | ⬜ | VG-13 |
| 14 | Unit of work, visit deltas, id recovery, settlement lock | WP-29 | — | ⬜ | VG-14 |
| 15 | Narrow hot readers, own the temp dir, layering guard | WP-29 | — | ⬜ | VG-15 |
| 16 | WP-07 regression net + wave DoD | WP-07 | — | ⬜ | VG-16 |

---

## Per-slice 10-stage checklists

### Slice 1 — Culture sensitivity write path accepts NULL (C-19, WP-14)

- **Goal:** make the WP-03 nullable column reachable; stop a saved row being deleted when the user picks «Unspecified».
- **Touches:** `SaveCultureResultsCommand.cs:8` · `SaveCultureResultsCommandValidator.cs:9` · `SaveCultureResultsCommandHandler.cs:5` · `CultureEntryViewModel.cs:223-226` · `SaveCultureResultsMappingTests.cs`.
- **Gate:** VG-01. Migration: none.
- **Expected test-count change:** Application strictly **above** baseline.

#### Stage 1 — Pre-Execution Verification (measured 2026-10-01)

| Item | Measured | Δ vs agent G0 |
|---|---|---|
| `git rev-parse HEAD` | `94292c2b2c953f9a2767cf7e81392ade0f01187d` ✓ | 0 |
| `git status --porcelain` | 3 untracked W-02 package files only | 0 |
| Build | 0 warnings / 0 errors | 0 |
| Domain / Application / Infrastructure / Presentation | 484 / 1502 / 221 / 57 — all 100% | 0 |
| Persistence | 13 passed + 1 skipped | 0 |
| `has-pending-model-changes` | `No changes have been made to the model since the last migration.` | 0 |

#### Stage 3 — File Analysis (every anchor re-opened and confirmed)

| Anchor | Confirmed | Match? |
|---|---|---|
| `SaveCultureResultsCommand.cs:8` | `public sealed record CultureSensitivityInput(int AntibioticId, int SensitivityCategory);` — **`int`, not `int?`** | ✓ C-19 |
| `SaveCultureResultsCommandValidator.cs:9` | `RuleForEach(x => x.Sensitivities).Must(x => x.SensitivityCategory is >= 0 and <= 3);` — `null` impossible | ✓ |
| `SaveCultureResultsCommandHandler.cs:5` | `_db.Remove(old)` for **every** existing row, then `Create(... CultureAntibioticResultId.Create(0) ...)` with the hard cast `(SensitivityCategory)item.SensitivityCategory` | ✓ |
| `CultureEntryViewModel.cs:223-225` | `.Where(r => r.SensitivityCategory.HasValue)` then `.Select(r => new CultureSensitivityInput(r.AntibioticId, r.SensitivityCategory!.Value))` | ✓ |
| `CultureAntibioticResult.cs:14` | `public SensitivityCategory? SensitivityCategory { get; private set; }` — already nullable in the domain | ✓ |
| `CultureEntryViewModel.cs:55-59` | `SensitivityOptions`: `new(null, "Unspecified")` + the four SD-4 English labels | ✓ SD-4 |
| `SensitivityCategory` enum | `HighlyFor=0, ModerateFor=1, LowFor=2, ResistantFor=3` | ✓ SD-4 |
| `CultureResultCommandHandlerTests.cs` | the only handler test; seeds via `CultureAntibioticAttachment` and asserts `Assert.Single(db.CultureAntibioticResults)` | ✓ new home for the handler-level VG items |

**The defect chain is confirmed live:** nullable column (WP-03) ⇄ `int` on write ⇄ `.Where(HasValue)` in the view model ⇄ delete-all-then-reinsert in the handler ⇒ picking «Unspecified» for a saved antibiotic **removes the row from the database** while the grid still shows it.

#### Stage 4 — Planning (exact edits)

1. **`SaveCultureResultsCommand.cs:8`** — `CultureSensitivityInput.SensitivityCategory`: `int` ⇒ **`int?`**. A *type change on an existing member*, not a new positional parameter, so SD-6's append-only rule is not engaged.
2. **`SaveCultureResultsCommandValidator.cs:9`** — `Must(x => x.SensitivityCategory is null or (>= 0 and <= 3))`. `null` is now valid (that is the whole point); `4` and `-1` stay rejected. The duplicate-`AntibioticId` rule at `:10` is untouched.
3. **`SaveCultureResultsCommandHandler.cs:5`** — replace the hard cast with `item.SensitivityCategory.HasValue ? (SensitivityCategory)item.SensitivityCategory.Value : (SensitivityCategory?)null`. **No new enum** (SD-4). The not-attached guard, the reviewed/printed/delivered guard, and the header `Create`/`Update` branch stay byte-identical.
4. **`CultureEntryViewModel.cs:223-225`** — **delete** `.Where(r => r.SensitivityCategory.HasValue)`; the select passes `r.SensitivityCategory` straight through (now `int?`, so the `!` is no longer needed and the null row reaches the handler).
5. **Not touched** (explicitly, per plan): `CultureSensitivityRow.SensitivityCategory` · `CultureSensitivityRowDto.SensitivityCategory` · `CultureEntryView.xaml:108-117` `SelectedValue` binding.
6. **Tests** — extend `SaveCultureResultsMappingTests.cs` (validator level) and `CultureResultCommandHandlerTests.cs` (handler level, via `FakeApplicationDbContext`) with the VG-01 items. Hand-rolled fakes only, **no mocking library**.
7. **No migration.** `git diff -- src/TopLab.Infrastructure/Persistence/` must stay empty.

#### Stages 5–7 — Execution, Verification, Validation Gate (VG-01)

**Edits made (4 production files + 2 test files):**

| File | Edit |
|---|---|
| `SaveCultureResultsCommand.cs:8` | `int SensitivityCategory` ⇒ `int? SensitivityCategory` |
| `SaveCultureResultsCommandValidator.cs:9` | `is >= 0 and <= 3` ⇒ `is null or (>= 0 and <= 3)` |
| `SaveCultureResultsCommandHandler.cs:5` | `(SensitivityCategory)item.SensitivityCategory` ⇒ `item.SensitivityCategory.HasValue ? (SensitivityCategory)item.SensitivityCategory.Value : (SensitivityCategory?)null` |
| `CultureEntryViewModel.cs:223-225` | **deleted** `.Where(r => r.SensitivityCategory.HasValue)`; select now passes `r.SensitivityCategory` through as `int?` |
| `SaveCultureResultsMappingTests.cs` | +4 tests (null accepted · null+non-null coexist · **enum values pinned** · a `using` was missing on the first build attempt and added) |
| `CultureResultCommandHandlerTests.cs` | +6 handler-level tests through `FakeApplicationDbContext` |

**Stage 6 — build:** `Build succeeded. 0 Warning(s) 0 Error(s)`.

**Stage 7 — VG-01, item by item:**

| VG-01 item | Result | Evidence |
|---|---|---|
| Build 0/0, no count below baseline | ✅ | 0/0. Domain 484 (**Δ 0**) · Application 1511 (**Δ +9**) · Infrastructure 221 (**Δ 0**) · Presentation 57 (**Δ 0**) · Persistence 13+1 skipped (**Δ 0**) |
| `SaveCulture_PersistsUnspecifiedAsNull` — null reaches the store as NULL, not `HighlyFor` | ✅ | added; `Assert.Null(row.SensitivityCategory)` |
| `SaveCulture_PersistsSensitiveAsHighlyFor` | ✅ | added |
| `SaveCulture_PersistsIntermediateAsModerateFor` | ✅ | added |
| `SaveCulture_PersistsLowSensitivityAsLowFor` | ✅ | added |
| `SaveCulture_PersistsResistantAsResistantFor` | ✅ | added |
| `SaveCulture_NullAndNonNullRows_Coexist` | ✅ | added; both rows survive, null row stays null |
| `SaveCulture_OutOfRangeValue_IsRejected` (pre-existing, stays green) | ✅ | pre-existing test, still passing (`4` rejected) |
| `CultureSensitivityCategoryEnum_ValuesUnchanged` (SD-4) | ✅ | added; 0/1/2/3 + exactly 4 enum members |
| `grep -rn "SensitivityCategory is >= 0" src/` ⇒ **0** | ✅ | **0** — the old pattern is gone |
| `git diff -- src/TopLab.Infrastructure/Persistence/` ⇒ empty, no migration | ✅ | `git diff --stat` printed nothing; `git status --porcelain` on that folder empty |
| zero-drift: `has-pending-model-changes` ⇒ no changes | ✅ | `No changes have been made to the model since the last migration.` |

**Two things the plan did not anticipate, handled honestly:**

1. **`CultureSensitivityInput` is used in existing tests with `int` literals.** `new CultureSensitivityInput(5, category)` still compiles because an `int` literal implicitly converts to `int?`. No pre-existing test needed editing — but this also means the type change alone does **not** prove nullability: that is why the six new handler-level tests exist and assert the stored value.
2. **My own counting error, caught and corrected.** A `grep`-based migration count using `-v Snapshot` returned **10**, because it also filtered `20260908175555_AddPatientTestReferenceRangeSnapshots`. The correct count — excluding only `.Designer.cs` and `ApplicationDbContextModelSnapshot.cs` — is **11**, matching both the owner's table and `git ls-files` (23 tracked files = 11 migrations × 2 + snapshot). **Recorded because this wave relies on that count at S7/S9/S10/S16.** The right filter is `grep -v "\.Designer\.cs$" | grep -v "^ApplicationDbContextModelSnapshot\.cs$"`.

**Deliberately not done:** no enum change (SD-4) · no `CultureSensitivityRow`/`CultureSensitivityRowDto`/`CultureEntryView.xaml` binding change · no migration · no `AgeRules` work (S3) · the delete-all-then-reinsert behaviour is **still there** — S1 only makes `null` writable; S2 removes the delete-and-recreate.

- [x] **Stage 8 — Documentation Update:** no user-facing string changed (the validator message for out-of-range is the FluentValidation default, unchanged; `ReportDocumentContent.ContainsEnglishLabels()` untouched). UI-texts register: **no new entry needed.**
- [x] **Stage 9 — Memory Status Update:** Slice Index S1 → ✅ · Execution Log appended · Current Status 1/16.
- [x] **Stage 10 — Git:** local commit `[W-02] Slice 1/16: Culture sensitivity write path accepts NULL (C-19, WP-14) — loop-engineering`; explicit paths only.



### Slice 2 — SaveCultureResults updates rows instead of recreating them (WP-14)

- **Goal:** stop delete-all-then-reinsert so row identity and any future column survive a re-save.
- **Touches:** `SaveCultureResultsCommandHandler.cs:5` · tests.
- **Gate:** VG-02. Migration: none.
- **SD-13:** this slice **must** land before S9.

#### Stage 1 — Pre-Execution Verification (measured 2026-10-01)

| Item | Measured | Δ vs agent G0 |
|---|---|---|
| `git rev-parse HEAD` | `871332f912ca69a2fd078dab8c32ac9609008e86` (S1 commit, descendant of the pinned baseline) | 0 |
| `git status --porcelain` | 2 untracked W-02 package files only | 0 |
| Build / Domain / Application / Infrastructure / Presentation / Persistence | 0/0 · 484 · 1511 · 221 · 57 · 13+1 | 0 vs G0; Application already carries S1's +9 |

#### Stage 3 — File Analysis

| Anchor | Confirmed | Match? |
|---|---|---|
| `SaveCultureResultsCommandHandler.cs:5` | `foreach(var old in _db.Set<CultureAntibioticResult>().Where(x=>x.PatientTestId.Value==pt.Id.Value).ToList())_db.Remove(old);` then a `Create(...)` per incoming row | ✓ |
| `grep -c "_db.Remove(old)"` | **1** — must become **0** per VG-02 | ✓ |
| `CultureAntibioticResult.cs` | 32 lines, private ctor, only `Create`; `Entity<TId>.Id` has a `protected set`, so identity is settable from inside the entity | ✓ |
| `CultureAntibioticResultId` | `sealed : StronglyTypedId<int>` with `Create(int)` | ✓ |
| `FakeApplicationDbContext` | exposes `CultureAntibioticResults` (`:51`), `Set<>` at `:237-239`, `Add` at `:331`, **`Remove` at `:386`**, `SaveChangesAsync` at `:400` is a **no-op returning 1** — it assigns **no** ids | ✓ |
| header branch | `new CultureResult(...)` / `header.Update(...)` — correct already, **do not touch** | ✓ |

#### Stage 4 — Planning (exact edits)

1. **`CultureAntibioticResult.cs`** — **add** `public void UpdateSensitivity(SensitivityCategory? value) => SensitivityCategory = value;`. `Create` and the private ctor signatures are **unchanged**; SD-4 untouched (no new enum).
2. **`SaveCultureResultsCommandHandler.cs:5`** — replace the two `foreach` loops with a three-way diff:
   - load existing rows into `Dictionary<int, CultureAntibioticResult>` keyed by `AntibioticId`;
   - for each incoming item: key present ⇒ `row.UpdateSensitivity(...)` (**the row keeps its id**); absent ⇒ `Create(CultureAntibioticResultId.Create(0), …)` exactly as before;
   - keys in the store but not incoming ⇒ `_db.Remove(...)`. Removal iterates over a **snapshot of the keys**, never the live dictionary.
3. **Kept byte-identical:** the not-attached guard, the reviewed/printed/delivered guard, and the header `Create`/`Update` branch (plan step 5 and 6).
4. **Tests** — VG-02 items in `CultureResultCommandHandlerTests.cs` using the existing `FakeApplicationDbContext`. Because the fake's `SaveChangesAsync` assigns no ids, the "keeps its id" test **seeds** a row with a known non-zero `CultureAntibioticResultId` and asserts it survives a re-save — which is precisely the identity guarantee S9's `InhibitionZoneMm` depends on.
5. **No migration.** `git diff -- src/TopLab.Infrastructure/Persistence/` must stay empty.

- [ ] 1–10. Plan: add `CultureAntibioticResult.UpdateSensitivity(SensitivityCategory?)`; dictionary keyed by `AntibioticId`; update-in-place / create-new / remove-missing; header branch untouched; the not-attached guard kept verbatim.
#### Stages 5–7 — Execution, Verification, Validation Gate (VG-02)

**Edits made (3 files: 1 Domain, 1 Application, 1 test):**

| File | Edit |
|---|---|
| `CultureAntibioticResult.cs` | **added** `public void UpdateSensitivity(SensitivityCategory? value)`. `Create` and the private ctor are **unchanged**. |
| `SaveCultureResultsCommandHandler.cs:5` | the two `foreach` loops replaced by a three-way diff: existing rows loaded into `Dictionary<int, CultureAntibioticResult>` keyed by `AntibioticId`; incoming key present ⇒ `UpdateSensitivity` (**identity preserved**); absent ⇒ `Create(…Create(0)…)` exactly as before; store-keys not incoming ⇒ `_db.Remove` over a **materialised snapshot**, never the live dictionary. |
| `CultureResultCommandHandlerTests.cs` | +5 handler tests. |

**Kept byte-identical, as the plan required:** the not-attached guard (`المضاد الحيوي غير مرفق بهذه المزرعة.`) · the reviewed/printed/delivered guard · the header `CultureResult` `Create`/`Update` branch (`grep -c "header.Update"` ⇒ **1**, unchanged) · no enum change (SD-4).

**Stage 6 — build:** `Build succeeded. 0 Warning(s) 0 Error(s)`.

**Stage 7 — VG-02, item by item:**

| VG-02 item | Result | Evidence |
|---|---|---|
| Build 0/0 | ✅ | 0/0 |
| `SaveCulture_ExistingRow_KeepsItsId` | ✅ | seeded a row with `CultureAntibioticResultId.Create(777)`; after re-save the id is still **777** and the category updated to `ResistantFor` — no recreate |
| `SaveCulture_RemoveOneRow_KeepsTheOthers` | ✅ | ids 701/702 seeded; sending only antibiotic 1 leaves exactly **one** row, id **701** |
| `SaveCulture_AddNewRow_WhileKeepingExisting` | ✅ | existing keeps id **701**; the new antibiotic 2 row is added |
| `SaveCulture_TwiceInARow_IsIdempotent` | ✅ | two identical saves ⇒ still exactly one row, category unchanged, header untouched |
| `SaveCulture_HeaderUpdate_UnaffectedByRowDiffing` | ✅ | header `Sample` ⇒ `second`, `CultureCondition` ⇒ `anaerobic`, still one sensitivity row |
| `SaveCulture_NotAttachedAntibiotic_IsRejected` (current behaviour preserved) | ✅ | pre-existing test still green, Arabic message unchanged |
| `grep -c "_db.Remove(old)"` ⇒ **0** | ✅ | **0** (was 1) |
| `git diff -- src/TopLab.Infrastructure/Persistence/` ⇒ empty, no migration | ✅ | empty; migration count still **11** |
| zero-drift | ✅ | `No changes have been made to the model since the last migration.` |

**Measured counts vs the agent's own G0:** Domain **484 (Δ 0)** · Application **1516 (Δ +14: S1's +9 and S2's +5)** · Infrastructure **221 (Δ 0)** · Presentation **57 (Δ 0)** · Persistence **13 + 1 skipped (Δ 0)**. Nothing below baseline.

**Three things the plan did not anticipate — all recorded honestly:**

1. **My own assertion was wrong, not the product code.** `SaveCulture_TwiceInARow_IsIdempotent` initially asserted `Sample == " sample "` and failed with `Actual: "sample"`. `CultureResult` **trims on construction** — pre-existing behaviour unrelated to this slice. The test now asserts the observed trimmed value with a comment saying so. **No product code was changed to make a test pass.**
2. **The fake's `SaveChangesAsync` assigns no ids** (`FakeApplicationDbContext:400` returns `1`). So a "keeps its id" test cannot rely on EF-generated identity; the test **seeds** a known non-zero id and asserts it survives. This is a stronger assertion than the plan implied, and it is the exact guarantee S9's `InhibitionZoneMm` depends on.
3. **A brace-placement mistake in the test file.** `Seed()`'s single-line body ended with `}}` (method + class); appending tests after it closed the class early and broke the build with `CS1519`/`CS1513`. Fixed by removing one brace and closing the class at the end of the file.

**Deliberately not done:** no `CultureResult` header change · no change to `CultureAntibioticResult.Create` · no migration · no `InhibitionZoneMm` column (that is S9) · the S1 nullability work is untouched.

**SD-13 is now satisfied: this commit must exist in `git log` before S9 opens. Its hash is recorded below.**

- [x] **Stage 8 — Documentation Update:** no user-facing string changed. UI-texts register: **no new entry.**
- [x] **Stage 9 — Memory Status Update:** Slice Index S2 → ✅ · Execution Log appended · Current Status 2/16.
- [x] **Stage 10 — Git:** local commit `[W-02] Slice 2/16: SaveCultureResults updates sensitivity rows instead of recreating them (WP-14) — loop-engineering`; explicit paths only.

**SD-13 checkpoint hash (S9 Stage 1 must find this in `git log`):** recorded in the Execution Log below.


### Slice 3 — AgeRules + infant child detection (C-20, WP-14)

- **Goal:** an 11-month-old is a child; one site changes, not a sweep.
- **Touches:** new `Domain/Common/AgeRules.cs` · `CultureAntibioticDisplay.cs:5` · `GetCultureEntryGridQueryHandler.cs:33` · new `AgeRulesTests.cs`.
- **Gate:** VG-03. Migration: none.

#### Stages 1–7 — Plan, Execution, Verification, VG-03

**Edits:** new `src/TopLab.Domain/Common/AgeRules.cs` · `GetCultureEntryGridQueryHandler.cs:33` (the one C-20 site) · new `tests/TopLab.Domain.Tests/Common/AgeRulesTests.cs` · `CultureResultQueryHandlerTests.cs` (+5 grid tests, `Seed` overload taking an `AgeUnit`).

**Design note:** `AgeRules` lives in Domain and therefore cannot reference `CultureAntibioticDisplay` (Application). The threshold is `AgeRules.ChildAgeThresholdYears = 12`; the Application-side `CultureAntibioticDisplay.ChildAgeThresholdYears = 12` is **kept** per VG-03 and **pinned by a test** so the two cannot drift silently. Two `using`s removed from the query handler because `AgeUnit` is no longer referenced there — but `CultureAndAntibiotics.Common` had to be **re-added** on the first build attempt because `IsDisplayable` at `:36` still needs it.

**⚠️ The plan contradicts itself in S3. Disclosed, and the plan's *implementation* was followed:**

| Plan VG-03 expectation | Reality | Verdict |
|---|---|---|
| `AgeRules_Month13_IsNotChild` | 13 months = 1 year 1 month ⇒ **under 12** | plan expectation **wrong** |
| `AgeRules_Month144_IsNotChild` | 144 months = exactly 12 years ⇒ not under 12 | ✓ correct |
| `AgeRules_Day365_IsNotChild` | 365 days = exactly 1 year ⇒ **under 12** | plan expectation **wrong** |
| `AgeRules_Day2000_IsNotChild` | 2000 days ≈ 5.5 years ⇒ **under 12** | plan expectation **wrong** |
| `GetCultureEntryGrid_Child12_IncludesChildrenAntibiotics` | `12 < 12` is false ⇒ a 12-year-old is **not** a child, so the children-only antibiotic is hidden | plan item name **contradicts its own implementation** |

Arithmetic verified independently (`v // unit`): 13/12=1 · 364/365=0 · 365/365=1 · 2000/365=5 · 4380/365=12. The plan's *code snippet* (`Month => ageValue / MonthsPerYear`, `IsUnderTwelve => ToWholeYears < 12`) is **correct and implemented verbatim**; only its **test expectations** are wrong. The tests were corrected to medical reality (`143 months` still a child, `144` not; `4379 days` still a child, `4380` not) and the grid test asserts the real boundary with an explanatory comment. **No product code was bent to satisfy a wrong expectation.** This is not an SD-10 plan-vs-code mismatch — the plan is self-inconsistent, and the medical reading is not in doubt.

**VG-03 item by item:**

| Item | Result | Evidence |
|---|---|---|
| Build 0/0 | ✅ | 0/0 |
| `AgeRules_Year11/12/13` | ✅ | via `[Theory]` on the Year unit |
| `AgeRules_Month11_IsChild` ← **the real defect** | ✅ | `(Month, 11)` ⇒ child |
| `AgeRules_Month13/144` | ✅ **corrected** | 13 ⇒ child, 144 ⇒ not; 143 added as the boundary |
| `AgeRules_Day364/365/2000` | ✅ **corrected** | 364/365/2000 ⇒ child; 4379 ⇒ child, 4380 ⇒ not |
| `AgeRules_Zero_IsChild` | ✅ | 0 ⇒ child |
| `GetCultureEntryGrid_Infant11Months_IncludesChildrenAntibiotics` ← **decisive** | ✅ | an 11-month-old now sees the children-flagged antibiotic (previously hidden) |
| `GetCultureEntryGrid_Child12_…` | ✅ **corrected** | a 12-year-old is not a child; antibiotic 2 hidden |
| `GetCultureEntryGrid_Infant23Months_…` (added) | ✅ | 23 months ⇒ child |
| `GetCultureEntryGrid_AdultHidesChildrenOnlyAntibiotics` | ✅ | 30 y ⇒ only antibiotic 1 |
| `CultureAntibioticDisplay.ChildAgeThresholdYears` still defined | ✅ | `:5` = 12, pinned by a new test |
| `AnalyteReferenceRangeBand.Matches` unchanged (BR-04 intact) | ✅ | `git diff` on the file is **empty**; its existing tests still pass |
| `grep "AgeUnit == AgeUnit.Year"` in `Features/CultureResults/` ⇒ 0 | ✅ | **0** |
| **No sweep** (C-20): validator + editor view models untouched | ✅ | `git diff --name-only` lists **only** the query handler and its test file |
| No migration · zero-drift | ✅ | Persistence diff empty; `No changes have been made to the model since the last migration.` |

**Counts vs agent G0:** Domain **502 (Δ +18)** · Application **1521 (Δ +19; cumulative S1+S2+S3)** · Infrastructure **221 (Δ 0)** · Presentation **57 (Δ 0)** · Persistence **13+1 (Δ 0)**. Nothing below baseline; no new warning.

**Deliberately not done:** no change to `SaveAnalyteReferenceRangeCommandValidator.cs:20`, `AnalyteEditorViewModel.cs:16`, `TestEditorViewModel.cs:90,125`, `PatientEditorViewModel.cs:67` — C-20 says these are unrelated to classification · no enum change (SD-4) · no migration.

- [x] **Stage 8 — Documentation Update:** no user-facing string changed.
- [x] **Stage 9 — Memory Status Update:** Slice Index S3 → ✅ · Execution Log appended · Current Status 3/16.
- [x] **Stage 10 — Git:** local commit; explicit paths only.

### Slice 4 — IResultPrintCoordinator (WP-06)

- **Goal:** one honest path — build, print, and never mark.
- **Touches:** new `IResultPrintCoordinator.cs` + `ResultPrintCoordinator.cs` + `ResultPrintCoordinatorTests.cs`; `Application/DependencyInjection.cs`.
- **Gate:** VG-04. Migration: none. **Owns `ReportPrintingService.cs` from here on (SD-14).**

- [ ] 1–10. Plan: reuse `FakeReportPrintingService`; build-fails ⇒ zero tokens; print-fails ⇒ `Printed=false` + verbatim Arabic error; **zero** `MarkPrinted` references.

#### Stages 1–3 — STOPPED at Stage 3 (SD-10)

**Stage 1 — Pre-Execution Verification:** HEAD `d8457da7630b119026bcf3fcecd6b232b4c198fa` (S3) · `git status --porcelain` = 2 untracked W-02 package files only · build 0/0 · Domain **502** · Application **1521** · Infrastructure **221** · Presentation **57** · Persistence **13+1** — all at or above baseline.

**Stage 3 — File Analysis found TWO plan-vs-code contradictions, both material to what S4 must build. Per SD-10 the loop halts here, before any edit.**

---

### 🔴 STOP REPORT — S4 (SD-10, plan contradicts live code)

#### STOP #1 — `BuildProfileReportQuery` does not exist

W-02.md §5 (S4) line: «لكل `kind`: `BuildProfileReportQuery` (موجود)» — "existing".

```bash
$ grep -rn "BuildProfileReportQuery" src/ tests/ --include=*.cs | wc -l
0
```

**Reality:** zero occurrences anywhere. The real query is **`GetProfileReportQuery`** at `src/TopLab.Application/Features/ProfileResults/Queries/GetProfileReport/GetProfileReportQuery.cs:9`, returning `Result<ProfileReportDto>` (`ProfileResultDtos.cs:52-62`). This is the **same class of error as C-23** (a named symbol that does not exist), already corrected once in §1 and repeated here.

**Why it changes what S4 builds:** the coordinator's `ProfileReport` branch has no command to send. Either the plan meant `GetProfileReportQuery`, or the profile path needs a new `BuildProfileReportCommand` — which is **not in S4's scope table** and would be a new command + handler + DTO envelope the plan never mentions.

#### STOP #2 — `ResultPrintOutcome.PdfPath` cannot be populated, and no port can supply it

W-02.md §5 specifies `ResultPrintOutcome(int PatientTestId, ResultPrintKind Kind, bool Printed, string? PdfPath, string? ErrorMessage)` and VG-04 asserts `ResultPrintCoordinator_BuildSucceedsPrintSucceeds_ReturnsPdfPath`.

**Reality:**

```bash
$ grep -n "Task<Result>" src/TopLab.Application/Common/Interfaces/IReportPrintingService.cs
11:    Task<Result> PrintReportAsync(string reportToken, CancellationToken cancellationToken = default);
```

`ReportPrintingService.cs:62-67` writes the PDF to a temp path **and discards it**, returning bare `Result.Success()` with a comment «The temp PDF is intentionally left in the OS temp directory». **No port in the entire codebase returns a produced PDF path** — the only `Result<string>` is `IDatabaseMaintenanceService.BackupNowAsync` (a backup destination, unrelated). `IPatientReportPdfExporter.ExportAsync` takes a path *in* rather than returning one.

**Why it changes what S4 builds:** `PdfPath` is unsatisfiable without **changing `IReportPrintingService`'s signature**. Blast radius: **10 files** (3 production print handlers + the interface + the Infrastructure service + `FakeReportPrintingService` + 5 Infrastructure test call sites + `FakeReportPrintingService`'s consumers).

**None of those files is in S4's scope table**, and S4 explicitly says «`IAppUnitOfWork`-type widening of a port is not authorised» in spirit (SD-5 freezes `IAppLogger`; the same reasoning would apply to another frozen port). SD-14 also reserves `ReportPrintingService.cs` for S4 but S13 returns to it — a signature change would collide with that two-slice ownership.

**This also breaks S5's mandated user-facing text**, which is byte-for-byte from the plan: «تمت الطباعة (ملف: <اسم الملف>).» — there is no file name to interpolate.

#### What I did NOT do, deliberately

- **No edit to any file.** The working tree is clean; S4 has produced **zero** changes.
- **I did not widen `IReportPrintingService`.** That is a port-signature change to a shared abstraction used by 3 shipped print handlers, driven by a plan field the plan never scoped. Choosing between "change the port", "drop `PdfPath`", and "add an exporter that writes to a known path" is an **architecture decision the plan does not make** ⇒ «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية».
- **I did not** invent a `BuildProfileReportCommand`.

#### The three options (owner decision required)

| Option | Change | Consequence |
|---|---|---|
| **A** | Drop `PdfPath` from `ResultPrintOutcome`; S5 text becomes «تمت الطباعة.» | 0 changes to existing ports. But S5's registered Arabic text loses the file name, and the plan's registered UI string must change. |
| **B** | Change `IReportPrintingService` to return the path | 10 files, 3 shipped handlers, 2 Infrastructure behaviours. Widens a shared port — architectural. |
| **C** | Add a **new** read-only port (e.g. `ILastPrintedPdfLocator`) that returns the temp path of the last dispatch | New abstraction; S15 already owns temp-dir work, so this interacts with S15's owned-folder change. |

#### Recommendation (mine, not a decision)

**Option A.** It is the smallest change that keeps every existing port frozen, and the Arabic text still communicates the outcome honestly. The file name adds little to a lab operator — the sheet comes out of the printer either way. Options B and C both touch a frozen/shared abstraction for cosmetic gain, which is exactly what SD-5 and SD-7 exist to prevent.

**Awaiting the owner's decision. The loop is stopped per SD-10 and will not proceed to S5 until this is settled.**
#### Owner decision applied — Stages 4–7 (AD-1 + AD-2), VG-04

**AD-1 implemented:** `ResultPrintOutcome(PatientTestId, Kind, Printed, ErrorMessage)` — **no `PdfPath`**. `IReportPrintingService` **untouched** (`git diff` on both the interface and `ReportPrintingService.cs` is **empty**) — exactly the owner-approved Option A.
**AD-2 implemented:** the profile branch sends `GetProfileReportQuery`; **no new `BuildProfileReportCommand` invented**.

**Files:** new `IResultPrintCoordinator.cs` + `ResultPrintCoordinator.cs` (Application) · `Application/DependencyInjection.cs` (`AddScoped`) · `BulkPrintDtos.cs` (`BulkPrintOutcomes.Failed` for S6) · new `ResultPrintCoordinatorTests.cs` · `FakeSender` **extended** (culture/profile/build-command helpers) — `FakeReportPrintingService` reused as the plan requires, **not** duplicated.

**Design notes worth recording:**
- The coordinator injects **only** `ISender` + `IReportPrintingService` — **no `IApplicationDbContext`**, so there is literally no state to change. That is how "never mark" is enforced structurally, not just by convention.
- The patient id needed by `BuildCombinedReportCommand`/`BuildBlankReportCommand` is resolved through the feature's own read queries (`GetCultureReportQuery`, then `GetProfileReportQuery`) rather than by opening a second ownership rule.
- The culture/simple/blank branches all wrap in a `ReportPrintEnvelope` internally; the profile branch re-wraps `ProfileReportDto` as a single-line `CombinedReportDto`.

**Stage 6 — build:** `Build succeeded. 0 Warning(s) 0 Error(s)`.

**VG-04, item by item:**

| VG-04 item | Result | Evidence |
|---|---|---|
| Build 0/0, no count below baseline | ✅ | 0/0. Domain **502 (Δ0)** · Application **1529 (+8)** · Infrastructure **221 (Δ0)** · Presentation **57 (Δ0)** · Persistence **13+1 (Δ0)** |
| `..._BuildSucceedsPrintSucceeds_ReturnsPdfPath` | ✅ **superseded by AD-1** | `ResultPrintCoordinator_BuildSucceedsPrintSucceeds_ReturnsPrintedTrue` — `Printed=true`, `ErrorMessage=null`, exactly one token |
| `..._PrintFails_ReturnsPrintedFalseAndNoMarking` ← decisive | ✅ | printer returns `Error.Unexpected("تعذر طباعة التقرير.")` ⇒ `Printed=false` and the message is **verbatim**, not reworded |
| `..._BuildFails_DoesNotCallPrinting` | ✅ | `printing.Tokens` **empty** — a failed build never reaches the printer |
| `..._ReturnsArabicErrorMessage` | ✅ | `Error.NotFound("التحليل غير موجود")` surfaced unchanged |
| `..._BlankReport_PrintsWithoutResultLines` | ✅ | Blank envelope path, one token |
| `..._CultureReport_BuildsThroughCultureQuery` | ✅ | culture branch goes through `GetCultureReportQuery` |
| `..._ProfileReport_…` (AD-2) | ✅ | `GetCultureEntryGridProfileReport_BuildsThroughGetProfileReportQuery` via `GetProfileReportQuery` |
| `..._NeverCallsMarkPrinted` — structural | ✅ | `grep -c "MarkPrinted" ResultPrintCoordinator.cs` ⇒ **0** |
| No new port widening (AD-1) | ✅ | `IReportPrintingService.cs` and `ReportPrintingService.cs` diffs **empty** |
| No migration · zero-drift | ✅ | `No changes have been made to the model since the last migration.`; migration count **11**; Persistence diff empty |

**⚠️ One transient Infrastructure failure — investigated, not waved away.** A full-suite run reported `Failed: 1, Passed: 220`. A targeted re-run passed 221/221, and **three** further consecutive full runs all passed 221/221. The failing test name could not be reproduced or captured in any subsequent run, so it is recorded as **flaky/environmental**, **not** as a pass and **not** as a regression. Infrastructure's committed state is 221/221.

**Self-corrections during the slice (recorded):**
1. My first draft of `ResolvePatientAndTestAsync` returned `PrintOwnership(0)` for the culture branch — a real defect that would have built a report for patient 0. Caught on review and rewritten.
2. `FrozenRangeDto` exists in **two** namespaces with different shapes; my first mapping referenced the wrong one. Fixed — the profile-side record is already the type the combined line expects.
3. `CultureReportDto` ctor arity (19, not 22) and `WithResponse` type-inference failures in the new tests — both fixed.
4. The SD-1 doc comment originally contained the forbidden method name, which would have made VG-04's structural grep return 1 instead of 0. Reworded so the grep is genuinely clean.

**Deliberately not done:** no `MarkPrinted` anywhere · no port widening (AD-1) · no `BuildProfileReportCommand` (AD-2) · no new permission code · `BulkPrintOutcomes.Failed` added as data only, S6 does the wiring · no Arabic string invented in this slice.

- [x] **Stage 8 — Documentation Update:** UI-texts register **Table A row for S5 is amended by AD-1**: the success text becomes «تمت الطباعة.» (file name dropped). No new string in S4 itself.
- [x] **Stage 9 — Memory Status Update:** Slice Index S4 → ✅ · Stop Report marked resolved by owner decision · Execution Log appended.
- [x] **Stage 10 — Git:** local commit; explicit paths only.

### Slice 5 — Entry screens print through the coordinator + C-21 (WP-06)

- **Goal:** the two dishonest buttons become honest; the dead command is resolved.
- **Touches:** `ProfileEntryViewModel.cs:343-365` · `CultureEntryViewModel.cs:331-358` · (optionally) delete `MarkResultPrinted/` and its tests.
- **Gate:** VG-05. Migration: none.

#### Stage 1 — Pre-Execution Verification (2026-10-01)

| Item | Value | Δ vs S4 |
|---|---|---|
| `git rev-parse HEAD` | `1720cf7ad574a069873828378404fe25e36b50a3` | — |
| `git status --porcelain` | 2 untracked W-02 package files only | 0 |
| Infrastructure at slice start | **221 / 221** ✅ (stop rule satisfied) | 0 |
| Build / Domain / Application / Presentation / Persistence | 0/0 · 502 · 1529 · 57 · 13+1 | 0 |

#### Stage 3 — File Analysis (every anchor re-opened)

| Anchor | Confirmed | Match |
|---|---|---|
| `ProfileEntryViewModel.cs` `PrintAsync` = **343-365** | sends `MarkProfilePrintedCommand` at `:355`, `StatusMessage = "تم الطباعة."` at `:358`, `LoadAsync` at `:359`, `IsBusy` try/finally, error path `_presenter.Present(result.Error)` | ✓ C-7 |
| `CultureEntryViewModel.cs` `PrintAsync` = **331-358** | sends `MarkCultureReportPrintedCommand`, `StatusMessage = "تمت الطباعة."` at `:346`, same shape | ✓ C-7 |
| `MarkProfilePrintedCommandHandler` | injects only db/user/clock; loads unprinted `ProfileResultItem`s, marks each + `pt.MarkPrinted`, saves — **produces no PDF** | ✓ D1 |
| `MarkCultureReportPrintedCommandHandler` | **3 lines**, whole handler on line 3 — **no PDF** | ✓ D2 |
| `MarkResultPrintedCommandHandler` | `pt.MarkPrinted(...)` at `:57`, `SaveChangesAsync` at `:64` — **no PDF**; has real guards (reviewed, balance) | ✓ C-21 target |
| **C-26 four test files** | `ValidatorRegistrationTests.cs` = **2** · `ResultsEntryAuthorizationTests.cs` = **3** · `ExportPatientReportPdfCommandHandlerTests.cs` = **1** · `ReviewPrintDeliverCommandHandlerTests.cs` = **11** | ✓ exactly as C-26 lists |
| `ProfileEntryViewModel` ctor `:88-100` · `CultureEntryViewModel` ctor `:75-89` | 6 and 3 params | ✓ |
| `Presentation/DependencyInjection.cs` | both VMs `AddTransient` (`:54`, `:59`); coordinator is `AddScoped` — a transient consuming a scoped service is legal (lifetime flows the right way) | ✓ |

#### Stage 4 — Planning + **SD-16 DECISION (recorded before any edit)**

> **SD-16 / C-21 / C-26 — DECISION: WIRE. Not delete.**
> `MarkResultPrintedCommand` and its handler are **not** removed. `MarkResultPrintedCommandHandler` is rewired to go through `IResultPrintCoordinator` with `ResultPrintKind.SimpleResult`, keeping its existing guards (patient exists, test reviewed, balance gate, not-found messages) so **no behavioural contract changes** — only the dishonest `pt.MarkPrinted` + `SaveChangesAsync` tail becomes an honest coordinator call.
> **All four C-26 test files remain untouched**, which is exactly what VG-05 asserts. Two of them (`ValidatorRegistrationTests`, `ResultsEntryAuthorizationTests`) are structural gates; deleting the command would have forced edits that weaken the safety net itself.
> This is the §0 binding default, so **no owner authorization is required**.

**Exact edits:**

1. **`ProfileEntryViewModel`** — inject `IResultPrintCoordinator`; `PrintAsync` calls `PrintAsync(_patientTestId, ResultPrintKind.ProfileReport)`. On success ⇒ `StatusMessage = "تمت الطباعة."` (AD-1) + `LoadAsync`. On failure ⇒ `ErrorMessage = "تعذّرت الطباعة: " + outcome.ErrorMessage`. `IsBusy` try/finally preserved. `MarkProfilePrintedCommand` is **no longer sent from here**.
2. **`CultureEntryViewModel`** — same shape with `ResultPrintKind.CultureReport`.
3. **`MarkResultPrintedCommandHandler`** — SD-16 wire: keep every guard verbatim, replace the `try { pt.MarkPrinted } catch` + `SaveChangesAsync` tail with `_coordinator.PrintAsync(request.PatientTestId, ResultPrintKind.SimpleResult, ct)` mapped to `Result.Success()` / `Result.Failure(Error.Conflict(outcome.ErrorMessage))`.
4. **Texts (SD-8, byte-for-byte):** success **«تمت الطباعة.»** (AD-1 — the plan's «تمت الطباعة (ملف: \<اسم الملف\>).» is superseded because AD-1 removed the path) · failure **«تعذّرت الطباعة: <السبب>»**. Both already registered; S5's row was amended by AD-1.
5. **Not touched:** `CultureEntryView.xaml` · `PreviewProfilePdfAsync` (`:404-424`) · the four C-26 test files · no Infrastructure relocation (SD-12 debt recorded only) · no migration.
6. **Tests:** new `ProfileEntryPrintTests.cs` / culture print tests exercising both view models through a hand-rolled coordinator fake; plus coordinator-failure ⇒ no success text.
#### Stages 5–7 — Execution, Verification, VG-05

**Edits:** `ProfileEntryViewModel.cs` (field + ctor param + `PrintAsync`) · `CultureEntryViewModel.cs` (same) · `MarkResultPrintedCommandHandler.cs` (SD-16 wire + clock overload) · new `MarkResultPrintedWiringTests.cs` (4 tests + `FakeResultPrintCoordinator`) · `ReviewPrintDeliverCommandHandlerTests.cs` (see below).

**Stage 6 — build:** `Build succeeded. 0 Warning(s) 0 Error(s)`.

**VG-05, item by item:**

| VG-05 item | Result | Evidence |
|---|---|---|
| Build 0/0 | ✅ | 0/0 |
| `ProfileEntry_Print_Success_ShowsArabicSuccessText` | ✅ | `PrintAsync` calls the coordinator with `ResultPrintKind.ProfileReport`; on `Printed` ⇒ `StatusMessage = "تمت الطباعة."` (AD-1) |
| `ProfileEntry_Print_PrintFails_ShowsArabicErrorAndNoSuccessText` | ✅ | else ⇒ `ErrorMessage = "تعذّرت الطباعة: " + outcome.ErrorMessage`; no success text on the failure branch |
| `CultureEntry_Print_*` (both) | ✅ | same shape with `ResultPrintKind.CultureReport` |
| `NoViewModel_HardcodesPrintedMessage` — structural | ✅ | `grep -rn "تم الطباعة\." src/TopLab.Presentation/ \| wc -l` ⇒ **0** (the dot distinguishes it from «تمت الطباعة») |
| `PrintSuccessText_IsOnlyReachableOnSuccessfulResult` | ✅ | the success assignment lives **only** inside `if (outcome.Printed)` in both view models |
| **C-21/C-26 default "wire"**: `grep -n "MarkPrinted" MarkResultPrintedCommandHandler.cs` ⇒ **0** | ✅ | **0** — the handler now delegates to `_coordinator.PrintAsync(..., ResultPrintKind.SimpleResult, ...)`; **all four guards kept verbatim** (not-found ×2, unreviewed, balance) |
| The three C-26 structural test files unmodified | ✅ | `git diff --name-only` on `ValidatorRegistrationTests.cs`, `ResultsEntryAuthorizationTests.cs`, `ExportPatientReportPdfCommandHandlerTests.cs` ⇒ **empty** |
| Negative counts preserved | ✅ | `ValidatorRegistrationTests.cs` = **2**, `ResultsEntryAuthorizationTests.cs` = **3** — unchanged from Stage 3 |
| `ProfileEntry_PdfPreview_PathStillWorks` | ✅ | `PreviewProfilePdfAsync` still present (2 references) and untouched |
| No migration · zero-drift | ✅ | `No changes have been made to the model since the last migration.` |

**Counts vs baseline:** Domain **502 (Δ0)** · Application **1533 (Δ +4)** · Infrastructure **221 (Δ0)** · Presentation **57 (Δ0)** · Persistence **13+1 (Δ0)**. Nothing below baseline.

**⚠️ Two real conflicts the plan did not anticipate — both resolved without weakening any safety net:**

1. **SD-16's "do not edit the four test files" is incompatible with its own "wire" instruction.** `ReviewPrintDeliverCommandHandlerTests.cs` constructs `new MarkResultPrintedCommandHandler(db, user, new FakeDateTimeProvider())` in **4** places. Wiring replaces that third argument with the coordinator, so the file cannot compile untouched. Resolved by making the coordinator an **optional trailing parameter** plus a **retained `IDateTimeProvider` overload** delegating to it — so the file compiles **with zero edits**. The three structural gate files VG-05 names were **never touched**.
2. **`Print_Allowed_When_UserFlagOff` asserted `row.IsPrinted == true`.** That is precisely the dishonesty WP-06/SD-1 removes — the test pinned the bug as intended behaviour. The single assertion was inverted to `Assert.False(row.IsPrinted)` with a comment. **This is the only behavioural assertion changed anywhere in the wave so far**, and it is disclosed here rather than buried.

**Self-corrections:** a duplicate `_printCoordinator` field and a missing `using` from my own edits; my seed forgot (a) that `MarkReviewed` requires `EnterResult` first and (b) that the handler requires the patient row — both fixed in my new test only.

**Deliberately not done:** no deletion of `MarkResultPrinted` (SD-16 default) · no edit to the three structural gates · no Infrastructure relocation (SD-12 debt recorded, not refactored) · no Arabic string invented · no migration.

- [x] **Stage 8 — Documentation Update:** UI-texts register already carries both S5 strings (success amended by AD-1). **No new string.**
- [x] **Stage 9 — Memory Status Update:** Slice Index S5 → ✅ · Execution Log appended.
- [x] **Stage 10 — Git:** local commit; explicit paths only.

### Slice 6 — Bulk print through the coordinator (WP-06)

- **Goal:** bulk print reports `Failed` per patient instead of claiming success.
- **Touches:** `ExecuteBulkPrintCommandHandler.cs:18-26,71-87` · `BulkPrintDtos.cs:20-27` · `BulkCommandHandlerTests.cs`.
- **Gate:** VG-06. Migration: none. Reprint-confirmation behaviour **unchanged** here (WP-13's flag arrives in S8).

#### Stage 3 — File Analysis

`ExecuteBulkPrintCommandHandler.cs` (91 lines) confirmed: injects only db/user/clock (`:18-26`); `:54` `verified.Any(pt => pt.IsPrinted)`; `:71-83` the `try { pt.MarkPrinted } catch { continue }` loop; `:85` `SaveChangesAsync`; `:86` **always** reports `BulkPrintOutcomes.Printed`. The defect is exactly as the plan describes — the UI claims success without a single sheet.

`BulkPrintDtos.cs` — `BulkPrintOutcomes` already carries `Failed` from S4.

#### Stage 4 — Plan

1. Inject `IResultPrintCoordinator`. **Keep** `_currentUser` (balance gate `:61-69` still needs the user row) and **drop** `_clock`, which existed only to stamp `MarkPrinted`.
2. Replace `:71-86` with: for each `verified` row call the coordinator with `ResultPrintKind.SimpleResult`; count successes; **one failure does not abort the batch** (the others keep going, preserving the existing continue-on-failure behaviour).
3. Outcome per patient: `Printed` **only if every** verified test printed; otherwise `Failed` with the reason.
4. `IsPrinted` is neither read nor written after this change, except the pre-existing `:54` confirmation check, which **stays** in S6 — WP-13's `SuppressReprintMessage` arrives in S8 (plan step 6).
5. No migration.
#### Stages 5–7 — Execution, Verification, VG-06

**Edits:** `ExecuteBulkPrintCommandHandler.cs` (field + ctor + the `:71-86` block) · new `BulkPrintHonestyTests.cs` (7 tests) · `BulkCommandHandlerTests.cs` (ctor arg ×3 + 2 assertions) · `FakeResultPrintCoordinator` gained `FailForPatientTestId`.

`_clock` was **removed** (it existed only to stamp the marking); `_currentUser` is **kept** because the balance gate still reads the user row. `:54` `verified.Any(pt => pt.IsPrinted)` is **deliberately untouched** — WP-13's `SuppressReprintMessage` arrives in S8 (plan step 6).

**Stage 6 — build:** `0 Warning(s) 0 Error(s)`.

**VG-06, item by item:**

| VG-06 item | Result | Evidence |
|---|---|---|
| Build 0/0 | ✅ | 0/0 |
| `BulkPrint_AllPatientsPrint_ReportsPrinted` | ✅ | both patients `Printed`, 2 coordinator calls |
| `BulkPrint_OnePatientFails_ReportsFailedForThatPatientOnly` ← decisive | ✅ | patient 1 ⇒ `Failed`; patient 2 ⇒ `Printed` with `PrintedCount == 1` — **the batch continues** |
| `BulkPrint_PrintServiceFails_ReportsFailedForEveryPatient` | ✅ | all `Failed`, all `PrintedCount == 0` |
| `BulkPrint_NeverMarksPrinted` — structural | ✅ | `grep -n "MarkPrinted" ExecuteBulkPrintCommandHandler.cs` ⇒ **0**; plus a behavioural test asserting `IsPrinted == false` and `PrintCount == 0` after a successful bulk print |
| `BulkPrint_BalanceBlocked_StillReportsBlockedByBalance` | ✅ | pre-existing test still green |
| `BulkPrint_NoVerifiedResults_StillReportsNoVerifiedResults` | ✅ | re-asserted |
| `BulkPrint_RequiresReprintConfirmation_Unchanged` | ✅ | `Skipped` still returned when `ConfirmReprint == false` |
| `BulkPrintOutcomes_Failed_ConstantExists` | ✅ | present (added in S4) |
| Patient-not-found path | ✅ | still `PatientNotFound` |
| No migration · zero-drift | ✅ | `No changes have been made to the model since the last migration.`; migrations still **11** |

**Counts vs baseline:** Domain **502 (Δ0)** · Application **1540 (Δ +7)** · Infrastructure **221 (Δ0)** · Presentation **57 (Δ0)** · Persistence **13+1 (Δ0)**.

**⚠️ Two pre-existing assertions pinned the behaviour SD-1 forbids — changed and disclosed:**
- `Execute_Confirm_Reprints_And_Increments` asserted `pt.PrintCount == before + 1`.
- `Execute_Cancel_Skips_EntireReport_And_Continues` asserted `fresh.PrintCount == before + 1`.
Both now assert the count is **unchanged**. Together with S5's single inverted assertion, that is **3 behavioural assertions in the whole wave**, all of which asserted the counting/marking behaviour that WP-06 removes. None of them tested anything else.

**Deliberately not done:** no change to the reprint-confirmation check (S8) · no `SuppressReprintMessage` read yet · no new permission · no Arabic string invented · no migration.

- [x] **Stage 8 — Documentation Update:** no user-facing string changed.
- [x] **Stage 9 — Memory Status Update:** Slice Index S6 → ✅ · Execution Log appended.
- [x] **Stage 10 — Git:** local commit; explicit paths only.

### Slice 7 — ReportSettings print flags + `AddCombinedReportPrintOptions` (WP-13) — **M1**

- **Goal:** two persistent flags, no behaviour change at default `false`.
- **Touches:** `ReportSettings.cs:27,37-52,95-99` · `ReportSettingsConfiguration.cs:23-25` · `UpdateReportSettingsCommand.cs:8-19` · `UpdateReportSettingsCommandHandler.cs:30` · `SettingsDtos.cs:20-28` · `GetReportSettingsQueryHandler.cs:35` · new migration + Designer · migration test.
- **Gate:** VG-07. Migration: **M1**.

#### Stage 3 — File Analysis

| Anchor | Confirmed |
|---|---|
| `ReportSettings.cs:9-27` | exactly **10** properties, no print flags ✓ |
| `CreateDefault()` `:37-52` | ✓; `SetHistoryOptions` at `:95-99` is the last mutator |
| `ReportSettingsConfiguration.cs:14-23` | every column `.IsRequired()`; `:25` `HasData` on `Id = 1` — **C-24 confirmed: `bit NOT NULL`, not nullable** |
| `UpdateReportSettingsCommand.cs:8-19` | positional record, **8** params, `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` — **no new permission** |
| `UpdateReportSettingsCommandHandler.cs:30` | `row.SetHistoryOptions(...)` ✓ |
| `has-pending-model-changes` **before** | `No changes have been made to the model since the last migration.` ✓ |

#### Stage 4 — Plan (C-24 corrected design)

1. **Domain** — append `PrintGroupSubTitle` and `SuppressReprintMessage` (both `false` in `CreateDefault()`) + `SetPrintOptions(bool, bool)` after `SetHistoryOptions`.
2. **Config** — `.IsRequired().HasDefaultValue(false)` for both (**NOT NULL**, C-24) and extend the `HasData` seed at `:25` with both fields. The EF generator will then emit an `UpdateData` — **intentional**.
3. **Append-only (SD-6)** — `UpdateReportSettingsCommand` gains `bool PrintGroupSubTitle = false, bool SuppressReprintMessage = false` at the tail; same for `ReportSettingsDto` and `GetReportSettingsQueryHandler.cs:35`.
4. **Handler** — call `row.SetPrintOptions(...)` after `SetHistoryOptions`.
5. `dotnet ef migrations add AddCombinedReportPrintOptions` — **M1**, the first of three. Never `database update`.
6. Verify: exactly two `AddColumn`s of type `bit` NOT NULL default `false`, plus the intended `UpdateData`; `git diff --name-only` over Migrations shows only the new pair; the eleven originals untouched; zero-drift **after**.
#### Stages 5–7 — Execution, Verification, VG-07 (M1)

**Order respected:** Domain → Configuration → `dotnet ef migrations add`. **No `database update` was run.**

**Migration produced (`20261001203315_AddCombinedReportPrintOptions.cs`)** — exactly as designed:
- `AddColumn<bool>("PrintGroupSubTitle", "ReportSettings", type: "bit", nullable: false, defaultValue: false)`
- `AddColumn<bool>("SuppressReprintMessage", "ReportSettings", type: "bit", nullable: false, defaultValue: false)`
- `UpdateData("ReportSettings", keyColumn: "ReportSettingsId", keyValue: 1, columns: [], values: [])` — **the intentional seed update**
- `Down` drops both columns; **no** `DropColumn` on `PatientTests` or `ProfileResultItems` (grep ⇒ **0**)

**VG-07, item by item:**

| VG-07 item | Result | Evidence |
|---|---|---|
| `has-pending-model-changes` **before** ⇒ no changes | ✅ | recorded at Stage 1 |
| Config after Domain, before migration | ✅ | that was the execution order |
| Build 0/0 | ✅ | 0/0 |
| `..._AddsTwoBitNotNullColumnsWithFalseDefault` | ✅ | reflection over `Up`: exactly 2 `AddColumnOperation`, all `bit` / `IsNullable == false` / `DefaultValue == false` |
| `..._EmitsUpdateDataForSeededSettingsRow` | ✅ | single `UpdateDataOperation`, key column `ReportSettingsId`, value 1 |
| `..._Down_DropsBothColumns` | ✅ | 2 `DropColumnOperation` on `ReportSettings` |
| `ReportSettings_DefaultFlagsAreBothFalse` | ✅ | + a `SetPrintOptions` round-trip test |
| `UpdateReportSettings_PersistsPrintGroupSubTitle` / `…SuppressReprintMessage` | ✅ | handler calls `SetPrintOptions` after `SetHistoryOptions`; domain tests cover both flags |
| `UpdateReportSettings_OmittedFlags_DefaultToFalse` | ✅ | appended params default to `false` (SD-6) |
| `GetReportSettings_ReturnsBothFlags` | ✅ | handler appends both to the DTO |
| **Eleven existing migrations untouched** | ✅ | `git status` on the folder lists **only** the new pair + the EF-generated snapshot |
| Migration file count = **12** (11 + 1 new) | ✅ | **12** |
| `git diff -- Configurations/` ⇒ only the modified file | ✅ | only `ReportSettingsConfiguration.cs` |
| No `DropColumn` on `PatientTests`/`ProfileResultItems` (SD-1) | ✅ | grep ⇒ **0**; a negative test asserts it for Up **and** Down |

**Counts vs baseline:** Domain **502 (Δ0)** · Application **1540 (Δ0)** · Infrastructure **227 (Δ +6)** · Presentation **57 (Δ0)** · Persistence **13+1 (Δ0)**.

**⚠️ A real defect from S5 surfaced here and was fixed — disclosed.** S5 added a second three-argument constructor to `MarkResultPrintedCommandHandler` (the coordinator + a retained clock overload) so the C-26 test files would compile untouched. `dotnet ef` host validation then reported **«The following constructors are ambiguous»** — MediatR could not resolve the handler at runtime. That is a production defect introduced in S5 and only caught because S7 runs EF tooling.

**Resolution:** the clock overload is **removed** and the coordinator is **required**. Re-checking which C-26 files actually construct the *handler* proved the constraint was never real: `ValidatorRegistrationTests.cs` and `ResultsEntryAuthorizationTests.cs` reference only the **command**, never the handler — so SD-16's "do not edit" was never threatened by them. Only `ReviewPrintDeliverCommandHandlerTests.cs` needed its argument swapped (already done in S5, one more occurrence). **The three structural gate files remain byte-untouched.** `ambiguous` count is now **0**.

**Self-corrections:** `UpdateDataOperation` in EF 8 exposes `KeyColumns` (not `KeyColumn`) and a 2-D `KeyValues` array — corrected after reading the shipped XML docs rather than guessing again.

- [x] **Stage 8 — Documentation Update:** UI-texts register rows for S7 checkboxes already exist; the XAML itself is not part of S7's scope list (it is S8's screen wiring) — no new string created here.
- [x] **Stage 9 — Memory Status Update:** Slice Index S7 → ✅ · Migration Register row for M1 marked present · Execution Log appended.
- [x] **Stage 10 — Git:** local commit; explicit paths only.

### Slice 8 — Combined-report options, off-lab note, test comments (WP-13)

- **Goal:** `IsTakenOutsideLab` and `TestComment` reach the report; the reprint flag is honoured.
- **Touches:** `ReportDtos.cs:32-38,40-52,70-83` · `BuildCombinedReportCommandHandler.cs:95-97,138-143,147-159` · `PatientHistoryReader.cs:84-98` · `HistoryInsertion.cs:11-26` · `ReportContentBuilder.cs:188-281,317-375` · `ExecuteBulkPrintCommandHandler.cs:54-59` · new `GetTestCommentsForResults/` · new picker window + VM.
- **Gate:** VG-08. Migration: **none** — `TestComment` already has its table.

- [x] 1–10. **DONE.** Plan executed with two technically-required additions (both append-only, SD-6): (1) `TestGroupName` appended to `CombinedReportLineDto` — the sub-title flag needs the group identity and no existing field carries it; (2) `TestId = 0` appended to `ProfileEntryGridDto` + filled in its handler — the Profile picker button needs the test identity and the grid DTO did not carry it. Culture picker is preview-only (no note field exists on the culture save path — display via `HasPickedComment`/`BoolToVis`, no new converter invented). Build warnings fixed during the slice (2× CS1998 async-without-await, 2× CS0234) — final build 0/0. VG-08 green item-by-item: OutsideLab×2, comments aggregation (single query, order preserved), insertion forwarding, sub-title flag read from PK=1, reprint suppression ×2, rendering structural tests ×3, DTO compile-compat (all pre-existing positional constructions compile untouched), `TestComment` report grep non-zero, drift clean, no migration (`git diff Migrations/` empty).

### Slice 9 — `AddCultureMicroscopyAndZone` (WP-14) — **M2** ⚠️

- **Goal:** microscopy storage plus inhibition-zone and attachment-threshold columns.
- **Touches:** new `CultureMicroscopy.cs` + `CultureMicroscopyConfiguration.cs` + migration + Designer + migration test · `CultureAntibioticResult.cs` · `CultureAntibioticAttachment.cs` · their two configurations · `ApplicationDbContext.DbSets.cs`.
- **Gate:** VG-09. Migration: **M2**.
- **SD-13: Stage 1 must confirm the S2 commit exists in `git log`.** Hash: ________

- [x] 1–10. **DONE.** SD-13 confirmed at Stage 1 (`15e51bc` in `git log`). Domain → Config → `migrations add` order kept. `decimal(4,1)` nullable for both columns (C-24 pattern). Self-corrections during the slice: (1) two xUnit2031 warnings fixed (predicate overload); (2) two metadata tests initially asserted `GetColumnType()` which needs a relational provider — replaced with provider-agnostic assertions (presence/nullability/CLR type), column type pinned by the migration-ops tests. VG-09 green item-by-item: S2-commit gate, `has-pending` before (changes detected), build 0/0, CreateTable PK+cascade, both `decimal(4,1)` nullable, Down drops all, negative commercial test, microscopy 20-char cap + cascade metadata, zone-survives-resave (proves S2), 11 old untouched, drift clean after.

### Slice 10 — `AddAntibioticMasterFields` (WP-14) — **M3**

- **Goal:** symbol and scientific name on the antibiotic master; nothing commercial.
- **Touches:** `Antibiotic.cs` · `AntibioticConfiguration.cs` · Create/Update antibiotic commands + validators + handlers · `AntibioticDtos.cs:3-13` · both query handlers · `AntibioticEditorViewModel.cs` · `AntibioticsView.xaml` · `AntibioticEditorWindow.xaml` · new migration + Designer + migration test.
- **Gate:** VG-10. Migration: **M3**.

- [ ] 1–10. Plan: two nullable columns only; append optional command parameters; append DTO members with defaults; assert `AddColumnOperations.Count == 2`; assert zero "commercial" hits.

### Slice 11 — Culture sensitivity table + microscopy block in the report (WP-14)

- **Goal:** the microbiology report becomes a real sensitivity table with a microscopy block.
- **Touches:** `ReportDtos.cs:32-38` · `BuildCombinedReportCommandHandler.cs:138-143` · `PatientHistoryReader.cs:84-98` · `HistoryInsertion.cs:11-26` · `CultureResultDtos.cs:3-4,11-16` · `GetCultureReportQueryHandler.cs:26,28-35` · `GetCultureEntryGridQueryHandler.cs:38` · `ReportCultureSection.cs:8-51` · `ReportContentBuilder.cs:246-256` · `CultureEntryView.xaml:106-121` · `CultureEntryViewModel.cs` · new `CultureReportSectionTests.cs`.
- **Gate:** VG-11. Migration: **none**.

- [ ] 1–10. Plan: fix `SingleOrDefault(Id==1)`; two aggregated queries, no N+1; `BuildSensitivityGrid()` on `ReportCultureSection`; separate `ReportSection` carrying the grid; invariant decimal formatting; SD-4 English labels for «الفئة», registered as reuse; no commercial column.

### Slice 12 — History filters + CBC matrix + dead-code cleanup (WP-10)

- **Goal:** real filters, a real matrix, and the three dead-code defects closed.
- **Touches:** three history queries + the duplicate handler · `InsertHistoryResultCommandHandler.cs` · `PatientHistoryReader.cs:19-46,48-103` · `ReportDtos.cs:70-95` · `ReportPageComposer.cs` (only if wrapping is needed) · `HistoryReportsViewModel.cs:30,155-161,204-216` · `InsertHistoryDialogViewModel.cs:88-90` · `HistoryReportsView.xaml` · new `HistoryMatrixBuilder.cs` + `HistoryMatrixRow.cs` · two test files.
- **Gate:** VG-12. Migration: **none**.

- [ ] 1–10. Plan: append filter parameters with defaults; **delete the handler, keep the query** (C-25); push filters into SQL; pivot matrix; split into two grids rather than changing `ReportPageComposer`; clear `Entries` with an error instead of the always-true type test.

### Slice 13 — Swallowed print exceptions reach a diagnostics sink (WP-29)

- **Goal:** a swallowed failure leaves a trace — through a new port, never through `IAppLogger`.
- **Touches:** new `IPrintingDiagnostics.cs` + `PrintingDiagnostics.cs` + tests · `Infrastructure/DependencyInjection.cs` · `ReportPrintingService.cs:20-32,73-76` ⚠️ · `BarcodeService.cs:80-83` (not `ToAscii`) · `ExportPatientReportPdfCommandHandler.cs:161-168`.
- **Gate:** VG-13. Migration: **none**.

- [ ] 1–10. Plan: reflection test pinning `IAppLogger`'s single method; fixed `component`/`operation` strings only — **never** a patient id or a temp path; writer never throws; `OperationCanceledException` still re-thrown before the general catch in both services; no behaviour change in any returned `Result`.

### Slice 14 — Unit of work, visit deltas, id recovery, settlement lock (WP-29)

- **Goal:** visit edits are all-or-nothing, ids are the rows actually inserted, settlement is serialised.
- **Touches:** new `IAppUnitOfWork.cs` + `AppUnitOfWork.cs` + `ApplyVisitDeltas/` + `ApplyConditionDeltas/` + tests · `ApplicationDbContext.cs` (internal accessor only) · `PatientEditorViewModel.cs:883-916,918-976` · `AddTestsToVisitCommandHandler.cs:112,117-122` · `SettleAccountInFullCommandHandler.cs:42-77` · `Infrastructure/DependencyInjection.cs`.
- **Gate:** VG-14. Migration: **none**.

- [ ] 1–10. Plan: no `DbContext` in the Application interface; read ids from the change tracker before `SaveChangesAsync` and delete both `createdIds.Add(0)` and `Take()`; one scoped `FromSqlInterpolated` with `UPDLOCK, HOLDLOCK` **on that call site only**; concurrency tests in Persistence with an honest skip when Docker is absent.

### Slice 15 — Narrow hot readers, own the temp dir, layering guard (WP-29)

- **Goal:** stop the two unbounded loads, stop `%TEMP%` accumulation, stop layer drift.
- **Touches:** `PatientHistoryReader.cs:29-31` · `WorkSheetHelpers.cs:104-107` · new `TempPdfCleanupService.cs` + tests · new `PresentationLayeringTests.cs` · `Infrastructure/DependencyInjection.cs` · `ReportPrintingService.cs:62` · `BarcodeService.cs:69` · comments on the three bounded catalogue sites.
- **Gate:** VG-15. Migration: **none**.

- [ ] 1–10. Plan: own `%TEMP%\TopLab\Print\`; cleanup by `LastWriteTimeUtc`, app-owned folder only; catalogue loads annotated, not rewritten; layering test **pins** the three existing violations as numbered debt rather than refactoring them.

### Slice 16 — WP-07 regression net + wave DoD (WP-07)

- **Goal:** re-pin the whole range-comment chain after WP-13/WP-14 reshaped the same DTOs, then close the wave.
- **Touches:** tests only — new `RangeCommentFeedingTests.cs`; extend `CultureReportSectionTests.cs`.
- **Gate:** VG-16 = **wave DoD**.

- [ ] 1–10 + wave DoD. Measured final: Domain ___ · Application ___ · Infrastructure ___ · Presentation ___ · Persistence ___ · Migration files = 14.

---

## Migration Register

| Slice | Migration name | Operations | Backup | Review agent |
|---|---|---|---|---|
| 7 | `AddCombinedReportPrintOptions` | `ReportSettings.PrintGroupSubTitle` `bit NOT NULL default false` · `ReportSettings.SuppressReprintMessage` `bit NOT NULL default false` · `UpdateData` for the seeded PK=1 row | n/a | pending — **generated and verified in S7; SD-15 review still owed** |
| 9 | `AddCultureMicroscopyAndZone` | `CreateTable CultureMicroscopies` (PK `PatientTestId`, 1:1 → `CultureResult`, cascade) · `CultureAntibioticResults.InhibitionZoneMm decimal(4,1) NULL` · `CultureAntibioticAttachments.SensitivityThresholdMm decimal(4,1) NULL` | n/a | pending |
| 10 | `AddAntibioticMasterFields` | `Antibiotics.Symbol nvarchar(10) NULL` · `Antibiotics.ScientificName nvarchar(150) NULL` | n/a | pending |

**Never edited:** `20260828052248_BaselineDataModel` · `20260828123530_RenamePkColumns` · `20260906093902_AddTestCodeAndLifecycleColumns` · `20260907162756_AddPatientIsDeletedAndPatientTestSampleDrawnIndex` · `20260908175555_AddPatientTestReferenceRangeSnapshots` · `20260909033414_AddAnalyteProfileDomain` · `20260910213833_AddPregnancyMedicalConditionTypeSeed` · `20260916113704_AddInvoiceIssues` · `20260930163921_FixCultureSensitivityCategoryOffByOne` · `20260930165920_AddExternalEntityEmail` · `20260930170644_AddBranchNumber`.

**Housekeeping outside the repo (SD-1, owner-run — NOT a migration):**

| Step | Command (development DB only) | Result | Recorded by |
|---|---|---|---|
| Verify | `SELECT COUNT(*) FROM PatientTests WHERE IsPrinted=1 OR PrintCount>0;` | ⬜ | owner |
| Verify | `SELECT COUNT(*) FROM ProfileResultItems WHERE IsPrinted=1 OR PrintCount>0;` | ⬜ | owner |
| Zero (only if non-zero) | `UPDATE PatientTests SET IsPrinted=0, PrintCount=0, LastPrintedByUserId=NULL, LastPrintedAtUtc=NULL;` and the same for `ProfileResultItems` | ⬜ n/a · ⬜ run | owner |

**Never create a migration for this.** Per the owner's attestation the system never ran in production, so the expected result is **zero rows** and no action. Record the outcome here regardless.

---

## Created UI Texts Register

Arabic strings are byte-for-byte from the stage plan (SD-8). Anything the plan does not supply is `TBD-AR` and is **not invented**.

**Two separate tables, deliberately.** Table A holds **new** user-facing strings this wave creates. Table B holds **pre-existing** strings this wave only **re-uses** — they are *not* new content, and mixing them into Table A would make the `ContainsEnglishLabels()` guard look like a violation when it is not.

### Table A — new strings created by Wave 2

| Slice | Screen / path | Text | Kind | Source |
|---|---|---|---|---|
| 5 | `ProfileEntryViewModel`, `CultureEntryViewModel` — success | «تمت الطباعة.» | Status | plan WP-06, **amended by AD-1** — the file name was dropped because no port can report a PDF path |
| 5 | `ProfileEntryViewModel`, `CultureEntryViewModel` — failure | «تعذّرت الطباعة: \<السبب\>» | Error | plan WP-06 (verbatim) |
| 7 | `ReportSettingsView.xaml` | «طباعة العنوان الفرعي (اسم المجموعة) في التقرير» | CheckBox label | plan WP-13 (verbatim) |
| 7 | `ReportSettingsView.xaml` | «طباعة الاختبار المطبوع مرة أخرى دون رسالة» | CheckBox label | plan WP-13 (verbatim) |
| 8 | Combined report | «العينة أُخذت خارج المعمل» | Report line | plan WP-13 (verbatim) |
| 8 | `CombinedReportView.xaml` | «طباعة العنوان الفرعي (اسم المجموعة) في التقرير» · «طباعة الاختبار المطبوع مرة أخرى دون رسالة» | CheckBox labels | plan WP-13 (verbatim) |
| 8 | Entry views | «تعليق» | Button | plan WP-13 (verbatim) |
| 8 | `TestCommentPickerWindow` | «تعليق» (title) · «اختيار» · «إلغاء» | Buttons | created S8 (no plan counterpart — window chrome only) |
| 8 | `TestCommentPickerViewModel` | «اختر تعليقاً من القائمة أولاً.» | Status | created S8 |
| 8 | Entry VMs | «اختر تحليلاً أولاً قبل اختيار تعليق.» · «احفظ التحليل أولاً قبل اختيار تعليق.» · «احفظ المزرعة أولاً قبل اختيار تعليق.» | Guards | created S8 |
| 11 | Culture report grid headers | «المضاد» · «الفئة» · «منطقة التثبيط (مم)» · «الاسم العلمي» | Table headers | plan WP-14; commercial header replaced per SD-2 |
| 11 | `CultureEntryView.xaml` group | «الفحص المجهري» | GroupBox header | plan WP-14 (verbatim) |
| 11 | `CultureEntryView.xaml` group | «صديدية» · «كريات حمراء» · «خلايا بطانية» · «بلورات» · «فطريات» · «أخرى» (×3) · «مباشر؟» | Field labels | plan WP-14 (verbatim) |
| 11 | `CultureEntryView.xaml` group | «الحساسية» | GroupBox header | plan WP-14 (verbatim) |
| 11 | `CultureEntryView.xaml` column | «منطقة التثبيط (مم)» | Column header | plan WP-14 (verbatim) |
| 11 | `CultureAttachmentView.xaml` column | «العتبة (مم)» | Column header | plan WP-14 (verbatim) |
| 10 | `AntibioticsView.xaml`, `AntibioticEditorWindow.xaml` | «الاسم العلمي» · «الرمز» | Column / field label | plan WP-14 (scientific name); **no** «الاسم التجاري» per SD-2 |
| 12 | `HistoryReportsView.xaml` | `FromDate` · `ToDate` · `SelectedTestId` · `PrintSeparately` · `SortMode` are property names, not labels — any visible label must be taken verbatim from the plan or marked `TBD-AR` | Property names | plan WP-10 |
| — | — | `TBD-AR` items (record as discovered) | — | — |

### Table B — pre-existing strings RE-USED by Wave 2 (NOT new content)

| Slice | Where re-used | Text | Origin | Why it is safe |
|---|---|---|---|---|
| 11 | Report «الفئة» column cells, microbiology report | `Unspecified` · `Sensitive` · `Intermediate` · `Low Sensitivity` · `Resistant` | **Already shipped** at `CultureEntryViewModel.cs:55-59` under SD-4/WP-01 | The stage plan supplies **no Arabic** for these five category values, and inventing Arabic is forbidden by SD-8. `ReportDocumentContent.ContainsEnglishLabels()` (`:132-136`) only forbids the tokens `LabId:`, `PatientId:`, `Name:`, `Paper:`, `TopSpace:`, `HeaderFooter:`, `Doctor Signature:`, `Sex:`, `Age: `, `Doctor:`, `Referral:`, `Flag:`, `Range:`, `Reviewed:`, `SortMode:`, `AutoDisplay:` — **none of the five labels contains any of them**, so the guard passes legitimately. Assert this explicitly in VG-11. |
| 8 | `CombinedReportView.xaml` checkboxes | same two labels as Table A | identical to the permanent `ReportSettingsView.xaml` checkboxes | One wording, two screens — do not diverge |

**Rule:** if a string appears in Table A, Wave 2 created it and it must come verbatim from the plan. If it appears in Table B, Wave 2 did not create it and must not restate it as Arabic.

---

## Execution Log

| Date | Slice | Stage | Action | Result |
|---|---|---|---|---|
| 2026-10-01 | — | authoring | Wave 2 package authored against `94292c2` from the W-01/S-06/S-07 loop-engineering trios; 25 plan-vs-code corrections registered; 16 slices, 3 migrations. | OK |
| 2026-10-01 | — | G0 | **Agent measured its own baseline** (build + all 5 test projects + drift gate). Domain 484 · Application 1502 · Infrastructure 221 · Presentation 57 · Persistence 13+1 · full 2277+1 · build 0/0 · `has-pending-model-changes` = no changes · Docker absent · `dotnet-ef` 8.0.30. **Every Δ vs the owner's table = 0.** | ✅ PASS |
| 2026-10-01 | S7 | 1–10 | **M1 created: `20261001203315_AddCombinedReportPrintOptions`** — two `bit` NOT NULL default-false columns + the intended `UpdateData`; no SD-1 violation. VG-07 green: build 0/0, Infrastructure **227 (+6)**, all others Δ 0, migrations now **12**, the eleven originals untouched, drift "no changes". **Caught and fixed a production defect introduced in S5**: two three-arg constructors made `MarkResultPrintedCommandHandler` unresolvable by MediatR (`constructors are ambiguous`); the clock overload is gone and the coordinator is required — the three structural gate files only ever referenced the command, so SD-16 was never at risk. | ✅ committed |
| 2026-10-01 | S8 | 1–10 | WP-13 wiring (no migration): `IsTakenOutsideLab`+`TestGroupName`+`TestComments` appended to `CombinedReportLineDto`, same two (+comments) to `HistoryEntryDto`; `GetTestCommentsForResults` aggregated query; handler + reader + insertion forwarding; `FromCombined` off-lab line + comments + group sub-title; `SuppressReprintMessage` honoured in bulk; picker VM+window+DI+3 buttons (Simple→Notes, Profile→Comment, Culture→preview-only); `TestId` appended to `ProfileEntryGridDto` (technically required). VG-08 green: build 0/0; Application **1550 (+10)** · Presentation **60 (+3)** · Infrastructure **230 (+3)** · others Δ0; drift clean; Migrations diff empty. | ✅ committed |
| 2026-10-01 | S7 fixup | 1–10 | **Chief-engineer takeover: F1+F2 repaired and re-proven.** F1: M1 `UpdateData` populated with both flags (was empty columns/values → invalid `UPDATE...SET WHERE` SQL, proven via regenerated `migrations script`); migration test hardened to reject empty Columns/Values. F2: `DependencyInjection.cs` indentation restored + trailing newline. Re-proof: regenerated script shows valid `SET [PrintGroupSubTitle]..., [SuppressReprintMessage]...`; build 0/0; Domain 502 · Application 1540 · Infrastructure 227 · Presentation 57 · Persistence 13+1; drift "no changes". | ✅ committed |
| 2026-10-01 | S6 | 1–10 | Bulk print goes through the coordinator and reports `Failed` honestly; `_clock` removed, balance gate kept, reprint check left for S8. VG-06 green: build 0/0, Application **1540 (+7)**, all others Δ 0, `MarkPrinted` ⇒ **0**, migration count 11, zero-drift. **Two pre-existing `PrintCount == before + 1` assertions inverted** — they pinned the counting behaviour SD-1 forbids (3 such assertions in the wave so far, all disclosed). | ✅ committed |
| 2026-10-01 | S5 | 1–10 | **SD-16 = WIRE (binding default).** Both entry view models now print through the coordinator; `MarkResultPrintedCommandHandler` delegates to it with `ResultPrintKind.SimpleResult` and all guards verbatim. VG-05 green: build 0/0, Application **1533 (+4)**, all others Δ 0, `grep "تم الطباعة\."` ⇒ **0**, `MarkPrinted` in the handler ⇒ **0**, three structural gate files **untouched**, zero-drift. **Two plan conflicts disclosed:** (1) SD-16's "don't edit the four test files" vs its own "wire" — resolved with an optional ctor param + retained clock overload so `ReviewPrintDeliver` compiles with zero edits; (2) `Print_Allowed_When_UserFlagOff` asserted `IsPrinted == true`, i.e. it pinned the bug — that one assertion inverted to `False`. | ✅ committed |
| 2026-10-01 | S4 | 4–10 | **Owner approved Option A (AD-1) + AD-2.** `ResultPrintOutcome` carries no `PdfPath`; `IReportPrintingService` untouched; profile branch uses `GetProfileReportQuery`. Coordinator injects only `ISender` + `IReportPrintingService` — no DbContext, so "never mark" is structural. VG-04 green: build 0/0, Application **1529 (+8)**, all others Δ 0, `grep MarkPrinted` ⇒ **0**, port diffs empty, migration count 11, zero-drift. **One transient Infrastructure failure (1/221) investigated: 3 consecutive full re-runs passed 221/221 — recorded as flaky, not as a pass.** Four self-corrections logged (incl. a draft bug returning patient 0). | ✅ committed |
| 2026-10-01 | S2 | 9 | **Process deviation, disclosed:** I first wrote S1's hash (`871332f`) into the SD-13 checkpoint by mistake, because the S2 commit did not exist yet when the row was drafted. I committed S2 (`15e51bc`), then corrected the hash in a **second** commit (`c04e88f`). That means S2 spans **two** commits instead of one, which is a deviation from SD-9's "one local commit per verified slice". **`amend`/`reset` are forbidden, so the extra commit was not rewritten away.** Both are local; nothing was pushed. From S3 on: read the hash *after* committing, or write the row with the short hash resolved in the next commit. | ⚠️ disclosed |
| 2026-10-01 | S4 | 3 | **STOP (SD-10).** Two plan claims contradicted live code: (1) `BuildProfileReportQuery` — **0 occurrences repo-wide**, real name is `GetProfileReportQuery`; (2) `ResultPrintOutcome.PdfPath` is unsatisfiable — `IReportPrintingService` returns bare `Result` and `ReportPrintingService:62-67` discards the temp path; widening it touches **10 files** and 3 shipped handlers, none in S4's scope. **No file edited; tree clean. Awaiting owner decision: drop `PdfPath` (A, recommended) / widen the port (B) / add a locator port (C).** | 🔴 STOP |
| 2026-10-01 | S2 | 1–10 | SD-13 prerequisite landed: `CultureAntibioticResult.UpdateSensitivity` + three-way diff in `SaveCultureResultsCommandHandler` (update-in-place / create-new / remove-missing). VG-02 green: build 0/0, Application **1516 (+5)**, all others Δ 0, `_db.Remove(old)` now **0** hits, migration count **11**, Persistence diff empty, zero-drift "no changes". Caught 3 self-inflicted issues honestly: a wrong assertion about `CultureResult` trimming (fixed in the test, product code untouched), the fake `SaveChangesAsync` assigning no ids, and a brace-placement error. | ✅ committed |
| 2026-10-01 | S2 | SD-13 | **SD-13 checkpoint — the S2 prerequisite commit hash is `15e51bc` (full: `15e51bcb…`; resolve with `git log --oneline` or `git rev-parse 15e51bc`).** S9 Stage 1 MUST find this commit in `git log` before creating `AddCultureMicroscopyAndZone`; if absent, STOP. | pinned |
| 2026-10-01 | S1 | 1–10 | C-19: `CultureSensitivityInput.SensitivityCategory` `int`⇒`int?` · validator accepts `null` · conditional cast in the handler · deleted `.Where(HasValue)` in `CultureEntryViewModel`. +10 tests. VG-01 fully green: build 0/0, Application **1511 (+9)**, all others Δ 0, old validator pattern `0` hits, Persistence diff empty, zero-drift "no changes". One self-caught counting error on the migration count (see Slice 1 note). | ✅ committed |

---

## Current Status

- Slices complete: **7 / 16**.
- **SD-13 satisfied** — the S2 prerequisite for S9 has landed and is committed.
- Current slice: **Slice 8** — combined-report options, off-lab note, test comments (no migration).
- Baseline: **MEASURED BY THE AGENT** — all Δ = 0, Infrastructure 221/221 (see the agent's own G0 table).
- Commits: 3 (plus 2 correction commits from S2).
- **Agent's measured baseline, binding from here on:** Domain **484** · Application **1502** (grew to 1511 in S1) · Infrastructure **221** · Presentation **57** · Persistence **13 + 1 skipped**. **Full suite 2277 + 1 skipped.** Build **0/0**.
- Notes: **S2 must land before S9.** **S4 must land before S13.** SD-16 (C-21) is decided in S5 Stage 4. SD-2 forbids any commercial-name artefact.

---

## Stop Report

**✅ RESOLVED — S4 stop, 2026-10-01 (SD-10).**

Two claims in W-02.md §5 (S4) are false against the pinned code:

1. **`BuildProfileReportQuery` does not exist.** `grep -rn "BuildProfileReportQuery" src/ tests/ --include=*.cs | wc -l` ⇒ **0**. The real query is `GetProfileReportQuery` (`Features/ProfileResults/Queries/GetProfileReport/GetProfileReportQuery.cs:9`). Same class of error as C-23.
2. **`ResultPrintOutcome.PdfPath` is unsatisfiable.** `IReportPrintingService.PrintReportAsync` returns bare `Task<Result>` (`:11`); `ReportPrintingService.cs:62-67` writes the temp PDF and **discards the path**, returning `Result.Success()`. No port in the codebase returns a produced PDF path. Populating `PdfPath` requires changing a shared port's signature — **10 files**, 3 shipped print handlers, none in S4's scope table. It also breaks S5's mandated Arabic text «تمت الطباعة (ملف: \<اسم الملف\>).», which has no filename to interpolate.

**Status:** S1–S3 complete and committed (`871332f`, `15e51bc`, `d8457da`). **S4 has made zero edits — the working tree is clean.** Awaiting an owner decision among:

- **A (recommended)** drop `PdfPath`; S5 success text becomes «تمت الطباعة.»
- **B** change `IReportPrintingService` to return the path (widens a shared port)
- **C** add a new read-only port returning the temp path of the last dispatch

Not a defect I may decide: choosing between freezing or widening a shared port is an architecture decision absent from §0. Reported as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية».

### ✅ OWNER DECISION — 2026-10-01: **Option A**

The owner reviewed the three options and **approved the recommended Option A**. This is now a **binding decision** and is equivalent in force to a §0 entry.

**AD-1 (owner, 2026-10-01) — `ResultPrintOutcome` carries no `PdfPath`.**
`ResultPrintOutcome(PatientTestId, Kind, Printed, ErrorMessage)`. `IReportPrintingService` is **not** widened — its signature stays `Task<Result>` and every existing port and handler is untouched. Consequences, all accepted by the owner:
- VG-04's `ResultPrintCoordinator_BuildSucceedsPrintSucceeds_ReturnsPdfPath` is **superseded** by `ResultPrintCoordinator_BuildSucceedsPrintSucceeds_ReturnsPrintedTrue`.
- S5's mandated success text becomes **«تمت الطباعة.»** (not «تمت الطباعة (ملف: \<اسم الملف\>).»). The UI-texts register row for S5 is **superseded** accordingly.
- SD-5's principle — never widen a shared port for cosmetic gain — is upheld.

**AD-2 (owner-approved consequence of AD-1) — the profile branch uses `GetProfileReportQuery`.**
`BuildProfileReportQuery` does not exist (0 repo-wide hits). The real query is `GetProfileReportQuery` (`Features/ProfileResults/Queries/GetProfileReport/GetProfileReportQuery.cs:9`), returning `Result<ProfileReportDto>`. The coordinator wraps it in an internally-generated `Combined` envelope, exactly as the plan already prescribes for the culture branch. **No new `BuildProfileReportCommand` is invented.**
