# Batch 1 Plan — R-F05, R-F01, R-A04

**Target commit (HEAD of working tree):** `7a2cfb505acd8f6bdac4e0b49c8059d95d19a757`
**Commit message:** `الإستعداد للرحلة` — 2026-10-03 14:57:48 +0300
**Working-tree state:** clean, with one untracked directory: `Docs/Remaining Tasks Folder/` (contains the comparison report and reference PDFs). No tracked file is modified; no staged change. Verified with:
```
git rev-parse HEAD   -> 7a2cfb505acd8f6bdac4e0b49c8059d95d19a757
git status --porcelain -> ?? "Docs/Remaining Tasks Folder/"
```
**Baseline verified by build at this commit:** `dotnet build TopLab.sln` → *Build succeeded. 0 Warning(s) 0 Error(s)*.

**Producer:** Agent 1 (Hermes). Produced independently; no file under `Docs/OpenCode/` or `Docs/Hermes/` other than this one was read, and no `build.md` was read. `Docs/Remaining Tasks Folder/Cross-comparison.md` was read **only** as a lead-generator for locating code; every behavioural claim below was re-verified in source and cited by file and line.

**Isolation note:** this plan deliberately plans Presentation/ViewModel/XAML work, which the `module-planning` skill forbids. That skill was therefore not applied.

---

## A. Verified current state of each item

### A.0 The lead, verified: Statistics contains exactly four count-style queries

`src/TopLab.Application/Features/Statistics/` contains exactly four query folders and nothing else:

| Query folder | Handler | Verified behaviour |
|---|---|---|
| `GetPatientCountStatistics` | `GetPatientCountStatisticsQueryHandler.cs:25-134` | Loads patients in `[From, To)` in memory (`:35-39`), then groups by sex (`:41-50`), referral entity with a no-referral bucket (`:52-86`), account type (`:88-97`), year+month (`:99-106`), and year+month×sex (`:108-121`). |
| `GetTestCountStatistics` | `GetTestCountStatisticsQueryHandler.cs:23-112` | Counts `PatientTest` rows per test and per group. |
| `GetSentOutStatistics` | `GetSentOutStatisticsQueryHandler.cs:22-91` | Counts sent-out samples per partner lab with cost/paid/remaining. |
| `GetUserProductivityStatistics` | `GetUserProductivityStatisticsQueryHandler.cs:22-109` | Per-user entered/reviewed/printed/delivered counters. |

No fifth query exists. The four query records all carry `IAuthorizedRequest` → `"STATISTICS"` (`StatisticsAccessPolicy.Statistics = "STATISTICS"`, `StatisticsAccessPolicy.cs:10`), confirmed by `StatisticsAuthorizationTests.cs:18-32`.

The presentation surface is a single screen: `src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs` (368 lines, `SelectedSection` 0..3 at `:55`, `:75-88`) and `src/TopLab.Presentation/Views/Statistics/StatisticsView.xaml` (292 lines, four `DockPanel` sections). There is **no print button and no PDF path anywhere in the Statistics feature** — verified by reading both files end to end.

There is **no `StatisticsViewModel` test file** — `tests/TopLab.Presentation.Tests/` contains no file under a `Statistics/` folder.

---

### A.1 R-F05 — Banded result monitor

**Confirmed absent.**

- `grep -rniE "Levey|QualityControl|ControlChart"` across `src/` → zero matches (no quality-control / banded-monitor type at any layer).
- The four Statistics queries above are the entire feature.
- The nearest existing facility is the result worklist, which filters on **workflow state**, not value: `GetResultWorklistQueryHandler.cs:124` projects `pt.IsTakenOutsideLab`, and `Views/Patients/ResultsWorklistView.xaml:102` renders it as a column. There is no result-value band filter anywhere.
- **Data the monitor needs already exists (no migration):**
  - `PatientTest.ResultValue` is a `string?` (`src/TopLab.Domain/Results/PatientTest.cs:32`).
  - `PatientTest.ResultFlag` is `ResultFlag?` (`:34`), enum `Normal=0, Low=1, High=2` (`Domain/Common/Enums/ResultFlag.cs`).
  - `PatientTest.EnteredAtUtc` (`:40`), `IsReviewed` (`:42`), `IsPrinted` (`:48`), `PrintCount` (`:50`), `IsDelivered` (`:56`).
  - `Patient.RegistrationDateUtc` (`Domain/Patients/Patient.cs:37`), `FullName` (`:17`), `Sex` (`:19`), `AgeValue`/`AgeUnit` (`:21`,`:23`), `LabId` (`:13`), `ReferralEntityId` (`:31`), `TreatingDoctorId` (`:29`).
  - `ExternalEntity.Name` resolved for the referral entity — the pattern is proven at `GetPatientCountStatisticsQueryHandler.cs:65-67` and `GetPatientByIdQueryHandler.cs:55-80`.
  - `Test.Name` / `Test.TestCode` / `Test.ResultKind` (`Domain/Tests/Test.cs:11`,`:17`,`:38`).
- **Value parsing precedent exists** but is **not reusable**: `ResultFlagComputer.TryParse` (`src/TopLab.Application/Features/ResultsEntry/Common/ResultFlagComputer.cs:69-78`) is `private static`, uses `decimal.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out _)`, and returns `false` for null/whitespace. `ResultFlagComputer` itself is `internal` (`ResultFlagComputer.cs:15`), i.e. reachable from anywhere in `TopLab.Application`, so widening `TryParse` to `internal` is a one-word change that lets the monitor reuse the **identical** parsing rule rather than inventing a second one.
- **Printable-report precedent exists but does not fit.** `ReportDocumentContent` (`src/TopLab.Infrastructure/Printing/ReportDocumentContent.cs:86-105`) requires a **non-nullable** `PatientFullName` and `int PatientId`, and `ReportPageComposer.ComposeBody` (`Printing/ReportPageComposer.cs:47-55`) unconditionally prints `اسم المريض: {PatientFullName}` and the id. A multi-patient banded monitor has no single patient. Adding a fifth `ReportKind` to `ReportPrintEnvelope` (`ReportProduction/Common/ReportPrintEnvelope.cs:14-18`) and a fifth case to `ReportContentBuilder.FromEnvelope` (`:36-51`) would force a fake patient row into the document. Two precedents exist for the alternative shape, each with its **own port and its own DTO**, sharing nothing: `ICustomGroupPdfWriter`/`CustomGroupPdfWriter` (`Application/Common/Interfaces/ICustomGroupPdfWriter.cs:16`, `Printing/CustomGroupPdfWriter.cs:22`) and `IPriceListPdfWriter`/`PriceListPdfWriter` (`Printing/PriceListPdfWriter.cs:23`). Both are invoked from a ViewModel with `IDialogService.PickPdfSavePathAsync` (`Presentation/Common/Dialogs/IDialogService.cs:10`; usage proven at `ViewModels/Lab/PriceListsViewModel.cs:207-221`) and resolve lab header text via `ILabPrintTextStore.GetAsync(LabPrintTextScope.Report)` (same file, `:212`).

**Not confirmed / open:** which test-picker data source the UI should use (`SearchTestCatalogQuery` returns `IReadOnlyList<TestSummaryDto>` — `SearchTestCatalogQuery.cs:7`; `GetTestGroupsQuery` returns `IReadOnlyList<TestGroupDto>` — `GetTestGroupsQuery.cs:6`). Both are un-paged. See OD-1.

---

### A.2 R-F01 — Patient statistics extension

**(a) Day-of-month grouping — confirmed absent.**

`GetPatientCountStatisticsQuery` has exactly six parameters (`GetPatientCountStatisticsQuery.cs:8-15`): `From`, `To`, `BySex`, `ByReferralEntity`, `ByAccountType`, `GroupByMonth`. There is no day-of-month flag. `GroupByMonth` groups on the **(Year, Month)** tuple (`GetPatientCountStatisticsQueryHandler.cs:101`) — that is month-of-year grouping, not day-of-month. `PatientCountStatisticsDto` (`StatisticsDtos.cs:20-28`) has `MonthlyCounts` (`MonthlyCountDto(int Year,int Month,int Count)`, `:8-11`) and `MonthlySexCounts` (`:13-18`) but **no** day-shaped DTO.

The UI exposes `تجميع شهري` bound to `GroupByMonth` (`StatisticsView.xaml:64`, `:130-146`, columns السنة/الشهر/العدد) and has **no** day-of-month control.

**(b) Amounts-paid money row — confirmed absent.**

`GetPatientCountStatisticsQueryHandler.Handle` (`:25-134`) touches **only** `_db.Set<Patient>()` (`:35`). It never reads `PaymentOperation`, and no money figure appears in `PatientCountStatisticsDto`.

**Data the money row needs already exists (no migration):** `PaymentOperation` carries `PatientId` (`Domain/Billing/PaymentOperation.cs:9`), `Amount` (`:11`), `DiscountAmount` (`:13`), `IsExtraCharge` (`:15`), `OperationType` (`:17`), `OperationAtUtc` (`:21`), `IsVoided` (`:23`).

**The amount formula is already settled and pure** — `PatientAccountCalculator.TotalPaid` (`Domain/Billing/PatientAccountCalculator.cs:28-33`) sums `Amount + (DiscountAmount ?? 0)` over operations that are `!IsVoided && !IsExtraCharge`. The identical rule is duplicated verbatim in two existing probes (`PatientSearch/Common/BalanceProbe.cs:20` and `CultureResults/Common/BalanceProbe.cs`). The money row must reuse `PatientAccountCalculator.TotalPaid`; it must not invent a third rule.

**Open:** which period anchor the money uses — see OD-2.

---

### A.3 R-A04 — Test-order edit gestures

#### (a) "Clear all tests" guard — confirmed missing; the reference rule is *not* implemented.

`ClearAllTestsCommandHandler.Handle` (`src/TopLab.Application/Features/PatientRegistration/Commands/ClearAllTests/ClearAllTestsCommandHandler.cs:20-56`) enforces, in order:
1. patient exists (`:22-26`);
2. patient not soft-deleted (`:28-31`);
3. **the patient row was created less than 24 hours ago** (`:33-36`, message `لا يمكن مسح التحاليل من مريض أضيف قبل أكثر من 24 ساعة.`);
4. no test has an entered result (`:44-47`).

There is **no** notion of "first registration" anywhere in the codebase. Verified:
- `grep -rn "VisitNumber|VisitIndex|FirstVisit|IsFirstRegistration|VisitCount"` across `src/` → **zero matches**.
- `Patient` (`Domain/Patients/Patient.cs`) has no visit-sequence or first-visit flag; rows 13-49 are the full property list.
- `PatientStatusCalculator.IsRegistrationToday` (`Domain/PatientStatus/PatientStatusCalculator.cs:93-96`) uses `registrationDateUtc.Date == DateTime.UtcNow.Date` for state S1, i.e. "registered today" — a **wall-clock** rule, not a first-registration rule.

A patient **is** a registration (one `Patient` row per visit — `Domain/Patients/Patient.cs:7-9`), and visits of one patient identity are grouped by `LabId` (`PatientSearch/Queries/GetVisitHistory/GetVisitHistoryQueryHandler.cs:40-44`). So "first registration" is computable at the Application layer today: this row is the earliest non-deleted visit in its `LabId` group (or the only row when `LabId` is null).

UI side: the "مسح الكل" button is bound unconditionally (`Views/Patients/PatientEditorView.xaml:410`); the ViewModel guard is only `IsEditMode && _patientId.HasValue` (`ViewModels/Patients/PatientEditorViewModel.cs:1183-1188`) plus a confirmation dialog (`:1194`). Nothing disables or explains the button.

The exact definition of "first registration" is a product decision → **OD-3**.

#### (b) Taken-outside-lab note on the printed report — **VERIFIED. The note IS rendered, but only on one of the four report paths.**

`ReportContentBuilder.cs` was read in full (448 lines). Every composition path:

| Path | Entry point | Renders the outside-lab note? | Evidence |
|---|---|---|---|
| **Combined clinical report** (the ordinary result report) | `FromEnvelope` → `FromCombined` | **YES** | `ReportContentBuilder.cs:222-225` — `if (line.IsTakenOutsideLab) body.Add("العينة أُخذت خارج المعمل");`, placed immediately after `النتيجة:` (`:218`) and before the frozen range (`:229`) |
| **Patient PDF export** (`ExportPatientReportPdf`) | `FromPatientExport` | **NO** | `:71-100` iterates `data.Lines` and emits name/value, range, profile items, culture summary — no branch reads a flag. `PatientReportPdfLine` (`ResultsEntry/Common/PatientReportPdfPort.cs:5-14`) has **no** `IsTakenOutsideLab` member at all, and `ExportPatientReportPdfCommandHandler.cs:146-155` does not pass one |
| **Specialised-profile report** | `FromProfileReport`, and `ResultPrintCoordinator.BuildProfileTokenAsync` | **NO** | `FromProfileReport` `:141-159` has no flag branch. `ResultPrintCoordinator.cs:101-133` constructs `CombinedReportLineDto` with **nine** positional arguments and therefore leaves `IsTakenOutsideLab` at its default `false` (`ReportDtos.cs:71`) — the flag is silently dropped |
| **Standalone history report** | `FromEnvelope` → `FromHistory` | **NO** | `:382-405` builds a 5-column grid التاريخ/التحليل/النتيجة/معتمد/التعليق. `HistoryEntryDto` *does* carry `IsTakenOutsideLab` (`ReportDtos.cs:105`) and `PatientHistoryReader.cs:132` populates it, so the data reaches the composer and is discarded |
| Culture report print | routed to `FromCombined` | YES (incidentally) | `ResultPrintCoordinator.cs:75-77` maps `CultureReportKind` → `BuildCombinedTokenAsync` → `BuildCombinedReportCommand`, whose handler passes `pt.IsTakenOutsideLab` (`ReportProduction/Commands/BuildCombinedReport/BuildCombinedReportCommandHandler.cs:232`) |

The data source of truth for the combined path is therefore genuine and traced end to end: `PatientTest.IsTakenOutsideLab` (`Domain/Results/PatientTest.cs:26`, set by `UpdateSampleFlags` `:215-229`) → `BuildCombinedReportCommandHandler.cs:232` → `CombinedReportLineDto.IsTakenOutsideLab` (`ReportDtos.cs:71`) → `ReportContentBuilder.cs:222-225` → `ReportSection` body lines → `ReportDocumentContent.BuildDisplayLines()` (`ReportDocumentContent.cs:161-178`), which is the golden-test surface (`ReportDocument.cs:56-57`).

Separately, the **work sheet** prints `خارج` as a sample-kind marker (`Printing/WorkSheetPdfWriter.cs:218`), fed by `WorkSheetHelpers.cs:74`. That is the bench sheet, not the report.

**Conclusion for (b):** the reference requirement *"the taken-outside-lab flag renders a note on the printed report"* is **already satisfied on the primary combined clinical report**. It is **not** satisfied on the patient PDF export, on the specialised-profile report, or on the history report. Whether to close those three gaps is **OD-4**; the profile path is a genuine one-line defect (a real `IsTakenOutsideLab` silently renders as `false`), the export and history paths are scope additions.

---

## B. Target behaviour and business rules

### B.1 R-F05 — Banded result monitor

**Behaviour.** The operator chooses one test, a minimum value, a maximum value, and a period; the screen lists every result of that test, in that period, whose numeric value falls inside `[Min, Max]`, with the patient context columns the reference specifies: **date, patient, patient data, referral entity, test, result, status**. A print action writes the same grid to a PDF at a user-chosen path.

**Business rules.**

| # | Rule |
|---|---|
| BR-F05-1 | `TestId` is **required** and must exist in `Test`; otherwise `Error.NotFound("التحليل غير موجود.")`. |
| BR-F05-2 | `From`/`To` are `DateOnly?`, defaulting to today from `IDateTimeProvider` (mirrors `GetPatientCountStatisticsQueryHandler.cs:29-31`). Period is half-open on UTC day boundaries: `>= From 00:00:00` and `< To+1 00:00:00` — the exact pattern at `:32-33`. |
| BR-F05-3 | The date column filters on **`PatientTest.EnteredAtUtc`** (the result-entry instant), not `CreatedAtUtc` (order time) and not `RegistrationDateUtc`. The monitor is about results. Rows with `EnteredAtUtc == null` are excluded by construction. |
| BR-F05-4 | Band comparison is **inclusive on both ends**: `Min <= value <= Max`. |
| BR-F05-5 | A value is included only if `decimal.TryParse(value.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out v)` succeeds. Non-numeric results (`"Negative"`, `"Not detected"`, `null`, whitespace) are **excluded** and are **not** an error — the grid simply does not contain them. This mirrors `ResultFlagComputer.TryParse` (`:69-78`) exactly. |
| BR-F05-6 | `Min > Max` is `Error.Validation` from the validator, message `الحد الأدنى يجب ألا يتجاوز الحد الأقصى.` |
| BR-F05-7 | `From > To` is `Error.Validation`, message `بداية الفترة يجب ألا تتجاوز نهايتها.` — verbatim the existing wording at `GetPatientCountStatisticsQueryValidator.cs:11`. |
| BR-F05-8 | The query is gated on `StatisticsAccessPolicy.Statistics` (`"STATISTICS"`), exactly like the other four. |
| BR-F05-9 | Patient rows that are **soft-deleted** (`Patient.IsDeleted`, `Domain/Patients/Patient.cs:49`) are excluded — same guard as `GetPatientCountStatisticsQueryHandler.cs:36` and `GetTestCountStatisticsQueryHandler.cs:45`. |
| BR-F05-10 | "Status" column = the row's own lifecycle, rendered from existing columns only. Planned mapping: `IsDelivered` → `تم التسليم`; else `IsPrinted` → `تمت الطباعة`; else `IsReviewed` → `معتمد`; else `غير معتمد`. No new persisted state. |
| BR-F05-11 | Ordering: `EnteredAtUtc` ascending, then `PatientTestId` ascending (deterministic). |
| BR-F05-12 | Referral entity resolves through `ExternalEntity.Name`; a null `ReferralEntityId` renders `بدون جهة إحالة`, reusing the existing constant wording from `GetPatientCountStatisticsQueryHandler.cs:14`. |
| BR-F05-13 | The PDF writer is a **separate port over a separate DTO** (the `ICustomGroupPdfWriter` / `IPriceListPdfWriter` precedent). It is **not** added to `ReportPrintEnvelope`'s `ReportKind` and **not** routed through `PrinterAssignment`, so no new `PrinterOutputType` value and no migration. |
| BR-F05-14 | The PDF is written with `FileMode.CreateNew` semantics — it throws `IOException` when the target exists — copied from `PriceListPdfWriter.cs:56-59`. |
| BR-F05-15 | The PDF carries the lab header from `ILabPrintTextStore.GetAsync(LabPrintTextScope.Report)` and uses `ArabicFontResolver.Resolve(labText.FontFamily)`, exactly as `PriceListPdfWriter.cs:59-67` does. |
| BR-F05-16 | All Arabic labels only. The writer's QuestPDF `Settings.License = LicenseType.Community` and `Settings.UseSystemFonts = true` are set in the writer's **own static constructor** — the owner ruling recorded at `PriceListPdfWriter.cs:26-28` and `CustomGroupPdfWriter.cs:27-29`. |

### B.2 R-F01 — Patient statistics extension

**(a) Day-of-month grouping**

| # | Rule |
|---|---|
| BR-F01-1 | A new query flag `GroupByDayOfMonth` is added to `GetPatientCountStatisticsQuery`, defaulting to `false` so no existing caller changes behaviour. |
| BR-F01-2 | Grouping key is `RegistrationDateUtc.Day` — the **day of the month**, in the same in-memory projection style as the existing month grouping (`:99-106`). It is deliberately **not** a `(Year, Month, Day)` tuple: the reference's variant groups by the day ordinal across the whole period. |
| BR-F01-3 | Output is `IReadOnlyList<DayOfMonthCountDto>(int Day, int Count)>`, ordered by `Day` ascending. Days with zero patients in the period are **not** emitted (same convention as `MonthlyCounts`). |
| BR-F01-4 | The new list is populated **only** when `GroupByDayOfMonth` is true; otherwise it is an empty list — identical to how `MonthlyCounts` behaves at `:99-106`. |
| BR-F01-5 | A new UI checkbox `حسب يوم الشهر` bound to `GroupByDayOfMonth`, and a grid with columns `اليوم` / `العدد`, shown only when the flag is true — mirroring `StatisticsView.xaml:130-146`. |

**(b) Amounts-paid money row**

| # | Rule |
|---|---|
| BR-F01-6 | A new query flag `IncludeMoneyRow`, defaulting to `false`. |
| BR-F01-7 | The money figure is `PatientAccountCalculator.TotalPaid(ops)` (`Domain/Billing/PatientAccountCalculator.cs:28-33`) over the operations of **the patients already selected by the period filter** (the `patients` list loaded at `:35-39`). No third copy of the paid formula is introduced. |
| BR-F01-8 | The operation set is `!IsVoided && !IsExtraCharge` — this is exactly what `TotalPaid` filters on; the handler must not pre-filter differently. |
| BR-F01-9 | Period anchor is **OD-2** (recommendation: `PaymentOperation.OperationAtUtc` inside `[From 00:00, To+1 00:00)`, i.e. money *received in the period*). |
| BR-F01-10 | Output is a single `PeriodMoneyDto(decimal AmountsPaid, int PaymentCount)` — the count is included so the operator can tell zero money from zero payments. |
| BR-F01-11 | With `IncludeMoneyRow == false` the DTO carries `null`, and no `PaymentOperation` row is read. |
| BR-F01-12 | The row is displayed as one line under the grids: `المدفوعات: {AmountsPaid}` with invariant formatting. |

### B.3 R-A04 — Test-order edit gestures

**(a) First-registration-only guard on bulk clear**

| # | Rule |
|---|---|
| BR-A04-1 | `ClearAllTestsCommandHandler` gains a first-registration guard whose definition is **OD-3**. |
| BR-A04-2 | On violation it returns `Error.Conflict` with the message `لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة.` (exact string in slice A04-S1; the plan mandates one Arabic literal reused verbatim in the handler, the ViewModel and the test). |
| BR-A04-3 | The guard runs **after** the existence and soft-delete checks and **before** the existing 24-hour check, so the user sees the specific reason the reference cares about. |
| BR-A04-4 | The existing 24-hour check and the "result already entered" check are **kept** unless OD-3 resolves to "replace the 24-hour rule". |
| BR-A04-5 | Presentation: `PatientEditorViewModel` exposes a read-only `CanClearAllVisitTests` (true only when the guard passes) and sets it on load, after add/remove/clear. The XAML button binds `IsEnabled="{Binding CanClearAllVisitTests}"`. The ViewModel also short-circuits with the same Arabic message when a stale command fires. |

**(b) Outside-lab report note**

| # | Rule |
|---|---|
| BR-A04-6 | **No change to the combined clinical report.** It already emits `العينة أُخذت خارج المعمل` (`ReportContentBuilder.cs:222-225`). This item is a *verification*, and the verification result is recorded in A.3(b). |
| BR-A04-7 | The specialised-profile report path is closed **only if OD-4 resolves to "yes"** (`ResultPrintCoordinator.cs:101-133`). |
| BR-A04-8 | No change to the history report or the patient PDF export unless OD-4 resolves to "yes, all". |

---

## C. Complete file inventory

### C.1 R-F05

**CREATE — Application (queries)**
| Path | Purpose |
|---|---|
| `src/TopLab.Application/Features/Statistics/Queries/GetBandedResultMonitor/GetBandedResultMonitorQuery.cs` | `record GetBandedResultMonitorQuery(DateOnly? From, DateOnly? To, int TestId, decimal? MinValue, decimal? MaxValue) : IRequest<Result<BandedResultMonitorDto>>, IAuthorizedRequest` → `StatisticsAccessPolicy.Statistics`. |
| `src/TopLab.Application/Features/Statistics/Queries/GetBandedResultMonitor/GetBandedResultMonitorQueryHandler.cs` | The banded query. Loads `PatientTest` rows for the test in the period, applies `ResultFlagComputer.TryParse` + `>= Min` / `<= Max`, projects patient + referral + test + status, orders deterministically. |
| `src/TopLab.Application/Features/Statistics/Queries/GetBandedResultMonitor/GetBandedResultMonitorQueryValidator.cs` | `TestId > 0`; `From <= To`; `Min <= Max`; `Min`/`Max` non-negative not required (a negative band is legal for some analytes) — validate only ordering and presence. |

**CREATE — Application (port + DTO)**
| Path | Purpose |
|---|---|
| `src/TopLab.Application/Common/Interfaces/IBandedResultMonitorPdfWriter.cs` | `Task WritePdfAsync(string absolutePath, BandedResultMonitorDto report, LabPrintTextDto labText, CancellationToken ct = default)`. Separate port, mirroring `ICustomGroupPdfWriter` (`ICustomGroupPdfWriter.cs:16`). |
| `src/TopLab.Application/Features/Statistics/Common/BandedResultMonitorDtos.cs` | `BandedResultMonitorDto(DateOnly From, DateOnly To, int TestId, string TestName, decimal? MinValue, decimal? MaxValue, int TotalCount, IReadOnlyList<BandedResultRowDto> Rows)`, `BandedResultRowDto(int PatientTestId, DateTime EnteredAtUtc, int PatientId, string PatientName, string? LabId, string Sex, int AgeValue, string AgeUnit, string ReferralEntityName, string TestName, string? ResultValue, int? ResultFlag, string StatusText)`, `BandedResultStatus` static label map. |

**CREATE — Infrastructure**
| Path | Purpose |
|---|---|
| `src/TopLab.Infrastructure/Printing/BandedResultMonitorPdfWriter.cs` | QuestPDF A4 portrait, RTL, lab header, criteria line (test + band + period), then a table over the rows. Own static ctor sets `License`/`UseSystemFonts`. Never overwrites. Text lines exposed via an `internal static BuildTextLines(...)` so tests assert on real strings, the `CustomGroupPdfWriter.BuildTextLines` pattern. |

**MODIFY**
| Path | Purpose |
|---|---|
| `src/TopLab.Infrastructure/DependencyInjection.cs` | One line beside `:85-90`: `services.AddScoped<IBandedResultMonitorPdfWriter, BandedResultMonitorPdfWriter>();` |
| `src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs` | New section index 4 = banded monitor. Add `SelectedTestId`, `MonitorMinText`, `MonitorMaxText` (strings, invariant-parsed — same pattern as `PatientEditorViewModel.cs:1228`), `TestItems`, `BandStats`, `HasBandStats`, `ShowBandEmpty`, `LoadMonitorCommand`, `PrintMonitorCommand`. Inject `ILabPrintTextStore` and `IBandedResultMonitorPdfWriter`. Extend `LoadFilterItemsAsync` (`:216-260`) with the test list. |
| `src/TopLab.Presentation/Views/Statistics/StatisticsView.xaml` | Fifth `RadioButton` in the section selector (`:15-21`), a fifth `DockPanel` section, a criteria row (test ComboBox + min + max + عرض + طباعة), and a `DataGrid` with columns التاريخ / المريض / الرقم / الجنس / العمر / جهة الإحالة / التحليل / النتيجة / الحالة. |

**MODIFY (R-F05 tests — see §F)**
`tests/TopLab.Application.Tests/Features/Statistics/GetBandedResultMonitorQueryHandlerTests.cs` (new), `StatisticsAuthorizationTests.cs`, `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs`, `tests/TopLab.Infrastructure.Tests/Printing/BandedResultMonitorPdfWriterTests.cs` (new), `tests/TopLab.Presentation.Tests/Common/Fakes.cs`, `tests/TopLab.Presentation.Tests/Statistics/StatisticsViewModelMonitorTests.cs` (new).

**DELETE:** none.

### C.2 R-F01

**CREATE:** none.

**MODIFY**
| Path | Purpose |
|---|---|
| `src/TopLab.Application/Features/Statistics/Common/StatisticsDtos.cs` | Add `DayOfMonthCountDto(int Day, int Count)` and `PeriodMoneyDto(decimal AmountsPaid, int PaymentCount)`; append `IReadOnlyList<DayOfMonthCountDto> DayOfMonthCounts` and `PeriodMoneyDto? Money` to `PatientCountStatisticsDto` (`:20-28`). |
| `src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/GetPatientCountStatisticsQuery.cs` | Append `bool GroupByDayOfMonth, bool IncludeMoneyRow` to the positional record. |
| `src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/GetPatientCountStatisticsQueryHandler.cs` | Compute `dayOfMonthCounts` next to `monthlyCounts` (`:99-106`); compute `money` via `PatientAccountCalculator.TotalPaid` when `IncludeMoneyRow`; extend the DTO construction at `:123-131`. |
| `src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs` | Add `GroupByDayOfMonth` and `IncludeMoneyRow` properties; pass them at `:269-271`; add `ShowMoneyRow` / money text. |
| `src/TopLab.Presentation/Views/Statistics/StatisticsView.xaml` | Two checkboxes in the patients section (`:60-66`) and one `DataGrid` (اليوم/العدد) plus one money `TextBlock`. |
| `tests/TopLab.Application.Tests/Features/Statistics/GetPatientCountStatisticsQueryHandlerTests.cs` | New facts (see §F). |
| `tests/TopLab.Application.Tests/Features/Statistics/StatisticsAuthorizationTests.cs` | **Three** existing `new GetPatientCountStatisticsQuery(null, null, true, true, true, false)` call sites (`:20`, `:41`, `:57`) must gain the two new arguments. |

**DELETE:** none.

### C.3 R-A04

**CREATE:** none.

**MODIFY**
| Path | Purpose |
|---|---|
| `src/TopLab.Application/Features/PatientRegistration/Commands/ClearAllTests/ClearAllTestsCommandHandler.cs` | Insert the first-registration guard after the soft-delete check (`:28-31`) and before the 24-hour check (`:33-36`), per OD-3. |
| `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` | Add `CanClearAllVisitTests`; refresh it in `LoadVisitTestsAsync` (`:604`), after add/remove/clear; short-circuit in `ClearAllVisitTestsAsync` (`:1182`) with the same Arabic message. |
| `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` | `:410` — add `IsEnabled="{Binding CanClearAllVisitTests}"` to the `مسح الكل` button. |
| `src/TopLab.Application/Features/ResultsEntry/Common/ResultPrintCoordinator.cs` | **Only if OD-4 = yes for the profile path**: add the 10th positional argument `pt.IsTakenOutsideLab` — which requires resolving the flag in `BuildProfileTokenAsync`, whose `GetProfileReportQuery` result must then expose it (`GetProfileReportQueryHandler` / `ProfileReportDto`). **Note:** this widens into `ProfileResults`, so it is gated, not assumed. |
| `src/TopLab.Application/Features/ResultsEntry/Common/ResultFlagComputer.cs` | **Only if OD-4 = yes**: widen `private static bool TryParse` (`:69`) to `internal static`. This is what lets R-F05 reuse the identical rule. |
| `tests/TopLab.Application.Tests/Features/PatientRegistration/Commands/ClearAllTestsCommandHandlerTests.cs` | New guard facts; the existing happy-path test (`:14-30`) creates a patient with no `LabId` and no competing visits, so it passes under the recommended OD-3 definition — **verified, not assumed**. |

**DELETE:** none.

### C.4 Batch totals

| | Create | Modify | Delete |
|---|---|---|---|
| Production `.cs` / `.xaml` | 5 | 8 | **none** |
| Test `.cs` | 4 | 3 | **none** |
| Migrations | **none** | **none** | **none** |

---

## D. Explicit database decision per item

| Item | Migration | Evidence |
|---|---|---|
| **R-F05** | **NOT REQUIRED.** | Every field the monitor reads already exists: `PatientTest.ResultValue` (`Domain/Results/PatientTest.cs:32`), `.ResultFlag` (`:34`), `.EnteredAtUtc` (`:40`), `.IsReviewed/.IsPrinted/.IsDelivered` (`:42`,`:48`,`:56`), `.TestId` (`:12`), `.PatientId` (`:10`); `Patient.RegistrationDateUtc/FullName/Sex/AgeValue/AgeUnit/LabId/ReferralEntityId/IsDeleted` (`Domain/Patients/Patient.cs:37,17,19,21,23,13,31,49`); `Test.Name/TestCode` (`Domain/Tests/Test.cs:11,17`); `ExternalEntity.Name`. The work is a new read query, a new DTO, and a new PDF writer. No new column, no index, no seed. |
| **R-F01** | **NOT REQUIRED.** | Day-of-month grouping derives from `Patient.RegistrationDateUtc`, already loaded by the handler at `GetPatientCountStatisticsQueryHandler.cs:37`. The money row derives from `PaymentOperation.Amount` (`Domain/Billing/PaymentOperation.cs:11`), `.DiscountAmount` (`:13`), `.IsExtraCharge` (`:15`), `.IsVoided` (`:23`), `.PatientId` (`:9`), `.OperationAtUtc` (`:21`) — all present since the baseline migration `20260828052248_BaselineDataModel.cs` and mapped in `ApplicationDbContextModelSnapshot.cs`. Both are query-side additions only. |
| **R-A04** | **NOT REQUIRED.** | (a) is a guard over rows already loaded (`ClearAllTestsCommandHandler.cs:22`, `:38`) plus one `Patient` existence/ordering read. (b) is a **verified no-op on the primary report path**: `ReportContentBuilder.cs:222-225` already renders the note from data that already flows (`BuildCombinedReportCommandHandler.cs:232` → `ReportDtos.cs:71`). The optional profile/export/history closures are DTO and coordinator changes over existing columns. |

**Batch verdict: zero migrations.** No file under `src/TopLab.Infrastructure/Persistence/Migrations/` is created, modified, or deleted. The 13 existing migrations and `ApplicationDbContextModelSnapshot.cs` are untouched, and `dotnet ef migrations has-pending-model-changes` is not part of any slice gate because nothing in this batch touches the EF model.

---

## E. Step-by-step workflow (ordered slices)

Every slice ends with the same two gates, run from the repo root:
```
dotnet build TopLab.sln -v q --nologo      # must report 0 Warning(s) 0 Error(s)
dotnet test  TopLab.sln --nologo           # must report 0 failed
```
Baseline at HEAD is green (verified: build 0/0). `TopLab.Persistence.Tests` self-skip when Docker is unavailable (`DockerFactAttribute`, `Persistence.Tests/RelationalIntegrationTests.cs:11-18`) — a skip is not a failure, and no slice depends on SQL Server.

Per the owner's F4–F6 rule, **each slice ends with a local commit and a stop.** Do not chain slices unprompted.

---

### Slice R-F05-S1 — Domain-free Application query (no UI)

1. Create `Statistics/Common/BandedResultMonitorDtos.cs`.
2. Create `GetBandedResultMonitorQuery.cs`, `.cs` validator, `.cs` handler.
3. Widen `ResultFlagComputer.TryParse` from `private` to `internal` (`ResultFlagComputer.cs:69`) so the handler reuses the identical parsing rule.
4. Add `tests/.../Features/Statistics/GetBandedResultMonitorQueryHandlerTests.cs`.
5. Add the new query to `StatisticsAuthorizationTests.ModuleQueries` (`:18-24`) and to `ValidatorRegistrationTests` (its `TheoryData` list, following the `GetTestCountStatisticsQuery` entry at `:32`).
6. **Gate:** build 0/0 + `dotnet test` green. Commit: `بعد تنفيذ F R-F05-S1`. **Stop.**

### Slice R-F05-S2 — PDF writer (Application port + Infrastructure)

1. Create `Application/Common/Interfaces/IBandedResultMonitorPdfWriter.cs`.
2. Create `Infrastructure/Printing/BandedResultMonitorPdfWriter.cs` following `PriceListPdfWriter.cs` line for line in structure (own static ctor `:26-35`, directory/file guards `:44-60`, `ArabicFontResolver` `:59-67`, A4 RTL table).
3. Register in `Infrastructure/DependencyInjection.cs` beside `:85-90`.
4. Add `tests/TopLab.Infrastructure.Tests/Printing/BandedResultMonitorPdfWriterTests.cs`: assert the text lines contain the criteria (test name, band, period) and the grid headers; assert `IOException` when the target exists; assert a successful write produces a non-empty file under a temp path.
5. **Gate:** build 0/0 + tests green. Commit `بعد تنفيذ F R-F05-S2`. **Stop.**

### Slice R-F05-S3 — Presentation (VM + XAML)

1. Extend `StatisticsViewModel` with the monitor section, commands, test items, and print (injecting `IDialogService`, `ILabPrintTextStore`, `IBandedResultMonitorPdfWriter`).
2. Extend `StatisticsView.xaml` with the fifth section.
3. Extend `tests/TopLab.Presentation.Tests/Common/Fakes.cs`: `FakeSender` currently **throws** `NotSupportedException` for an unrecognised request (`:152`) — add canned responses for `SearchTestCatalogQuery`, `GetTestGroupsQuery`, and `GetBandedResultMonitorQuery`. Add a settable `PdfSavePath` to `FakeDialogService` (`:66` currently hard-returns `null`) and a fake `ILabPrintTextStore` / `IBandedResultMonitorPdfWriter` recording the call.
4. Add `tests/TopLab.Presentation.Tests/Statistics/StatisticsViewModelMonitorTests.cs`.
5. **Gate:** build 0/0 + tests green. Commit `بعد تنفيذ F R-F05-S3`. **Stop.**

---

### Slice R-F01-S1 — DTO + query + handler

1. `StatisticsDtos.cs`: add `DayOfMonthCountDto`, `PeriodMoneyDto`, and the two new members of `PatientCountStatisticsDto`.
2. `GetPatientCountStatisticsQuery.cs`: append `GroupByDayOfMonth`, `IncludeMoneyRow`.
3. `GetPatientCountStatisticsQueryHandler.cs`: day grouping beside `:99-106`; money row after the existing groupings, using `PatientAccountCalculator.TotalPaid`.
4. Fix the **three** existing construction sites in `StatisticsAuthorizationTests.cs` (`:20`, `:41`, `:57`) to pass the new arguments. **This is a required compile fix, not an optional tidy.**
5. Add facts to `GetPatientCountStatisticsQueryHandlerTests.cs`.
6. **Gate:** build 0/0 + tests green. Commit `بعد تنفيذ F R-F01-S1`. **Stop.**

### Slice R-F01-S2 — Presentation

1. `StatisticsViewModel`: two properties, pass-through at `:269-271`, money text.
2. `StatisticsView.xaml`: two checkboxes in the patients section, one day grid, one money line.
3. Reuse/extend the `StatisticsViewModelMonitorTests` fakes added in R-F05-S3.
4. **Gate:** build 0/0 + tests green. Commit `بعد تنفيذ F R-F01-S2`. **Stop.**

---

### Slice R-A04-S1 — Guard in the Application handler

1. `ClearAllTestsCommandHandler.cs`: insert the OD-3 guard between `:31` and `:33`.
2. Add facts to `ClearAllTestsCommandHandlerTests.cs`, including the regression fact that the existing `Clear_HappyPath_RemovesAll` (`:14-30`) still passes.
3. **Gate:** build 0/0 + tests green. Commit `بعد تنفيذ F R-A04-S1`. **Stop.**

### Slice R-A04-S2 — Guard in the UI

1. `PatientEditorViewModel`: `CanClearAllVisitTests` + refresh + short-circuit.
2. `PatientEditorView.xaml:410`: `IsEnabled` binding.
3. Add a Presentation test for the enable/disable state and the message.
4. **Gate:** build 0/0 + tests green. Commit `بعد تنفيذ F R-A04-S2`. **Stop.**

### Slice R-A04-S3 — Outside-lab note (conditional on OD-4)

**Runs only if OD-4 resolves to a change.** Otherwise this slice is skipped and recorded as "verified, no change required" (the verified result stands as written in A.3(b)).
1. Profile path: extend the profile report DTO with `IsTakenOutsideLab`, read it in `GetProfileReportQueryHandler`, pass it as the 10th positional argument in `ResultPrintCoordinator.cs:101-133`.
2. Optional, only if OD-4 = "all": add the flag to `PatientReportPdfLine` and to the `FromHistory` grid.
3. **Gate:** build 0/0 + tests green. Commit `بعد تنفيذ F R-A04-S3`. **Stop.**

### Final gate

```
dotnet build TopLab.sln -v q --nologo
dotnet test  TopLab.sln --nologo
git status --porcelain     # expect only Docs/Hermes/Batch-1-Plan.md to be new
git log --oneline -7       # expect the six slice commits, none pushed
```
**Owner performs `git commit` and `git push` personally. The executing agent does neither.**

---

## F. Tests to add or change

### R-F05

**NEW `tests/TopLab.Application.Tests/Features/Statistics/GetBandedResultMonitorQueryHandlerTests.cs`**

| Test | Scenario | Expected |
|---|---|---|
| `Band_MinMaxInclusive_ReturnsOnlyMatchingRows` | Values 3.0, 5.0, 7.0 in band [3,7] | 3 rows (both bounds inclusive) |
| `Band_NonNumericResultValue_ExcludedNotErrored` | `"Negative"`, `null`, `"  "` alongside numeric rows | non-numeric absent; result `IsSuccess` true |
| `Band_ResultOutsidePeriod_Excluded` | row with `EnteredAtUtc` on `From-1` and on `To+1` | both excluded |
| `Band_UnenteredRow_Excluded` | `EnteredAtUtc == null` | excluded |
| `Band_DeletedPatient_Excluded` | matching row whose patient has `IsDeleted = true` | excluded |
| `Band_ReferralNull_RendersNoReferralLabel` | row with `ReferralEntityId == null` | `ReferralEntityName == "بدون جهة إحالة"` |
| `Band_ReferralResolvedFromExternalEntity` | row with a referral id present in `ExternalEntities` | the entity's `Name` |
| `Band_UnknownTestId_NotFound` | `TestId` absent from `Test` | `ErrorType.NotFound` |
| `Band_OtherTestRows_Excluded` | rows for a different test in the same period | excluded |
| `Status_Text_MapsLifecycle` | four rows at delivered / printed / reviewed / entered-only | `تم التسليم` / `تمت الطباعة` / `معتمد` / `غير معتمد` |
| `Order_Deterministic` | two rows with the same `EnteredAtUtc` | ascending `PatientTestId` |
| `From_AfterTo_ValidationFailure` | validator directly | fails, message `بداية الفترة يجب ألا تتجاوز نهايتها.` |
| `Min_GreaterThanMax_ValidationFailure` | validator directly | fails, message `الحد الأدنى يجب ألا يتجاوز الحد الأقصى.` |
| `Defaults_ToToday_WhenDatesOmitted` | `From`/`To` null, `FakeDateTimeProvider` fixed | query runs over that UTC day |

**MODIFY `StatisticsAuthorizationTests.cs`** — add the new query to `ModuleQueries` (`:18-24`); add a denial-without-permission fact and an absolute-permission bypass fact, mirroring `:34-60`.

**MODIFY `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs`** — add `GetBandedResultMonitorQueryValidator` to the validator-resolution theory data, next to the `GetTestCountStatisticsQuery` entry (`:32`). Without this the validator is silently never resolved at runtime.

**NEW `tests/TopLab.Infrastructure.Tests/Printing/BandedResultMonitorPdfWriterTests.cs`** — text-line assertions on the criteria and the nine grid headers; `IOException` on an existing target; a successful write produces a non-empty PDF; empty-rows case still writes the header and criteria.

**NEW `tests/TopLab.Presentation.Tests/Statistics/StatisticsViewModelMonitorTests.cs`** — test picker populates; min/max parse with invariant culture (`,` and `.`); a non-numeric min sets `ErrorMessage` and does not call the mediator; the print command short-circuits when `PickPdfSavePathAsync` returns `null`; the print command writes once with the loaded `BandStats`.

### R-F01

**MODIFY `tests/.../GetPatientCountStatisticsQueryHandlerTests.cs`**

| Test | Scenario | Expected |
|---|---|---|
| `DayOfMonth_GroupsByDayOrdinal` | registrations on the 1st, 1st and 15th | `[{1,2},{15,1}]`, ordered by `Day` |
| `DayOfMonth_FlagFalse_ReturnsEmpty` | flag false | `DayOfMonthCounts` empty |
| `DayOfMonth_RespectsPeriodFilter` | day 5 before `From`, day 5 inside | only the in-period day appears |
| `DayOfMonth_ExcludesDeletedPatients` | deleted row on the same day | not counted |
| `Money_UsesPatientAccountCalculatorFormula` | operations incl. a voided one and an extra-charge one | only `!IsVoided && !IsExtraCharge` `Amount + DiscountAmount` are summed |
| `Money_CountsPaymentOperations` | three qualifying operations | `PaymentCount == 3` |
| `Money_FlagFalse_ReturnsNull` | flag false | `Money is null` |
| `Money_ZeroPayments_ZeroNotNull` | period with patients but no operations | `Money` non-null with `0` |
| `ExistingBehaviour_UnchangedWhenBothFlagsFalse` | full existing classification set | identical to pre-change expectations — the regression guard for the six-to-eight-parameter record |

**MODIFY `StatisticsAuthorizationTests.cs`** — fix the three call sites (`:20`, `:41`, `:57`); the two theory facts themselves need no change.

### R-A04

**MODIFY `tests/.../ClearAllTestsCommandHandlerTests.cs`**

| Test | Scenario | Expected |
|---|---|---|
| `Clear_NonFirstVisit_Conflict` | patient has an earlier visit in the same `LabId` group | `ErrorType.Conflict`, message `لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة.` |
| `Clear_FirstVisit_Succeeds` | earliest visit, fresh tests | success, count returned |
| `Clear_OnlyVisitWithoutLabId_Succeeds` | single row, `LabId` null | success (regression guard for the existing happy path) |
| `Clear_SoftDeletedPatient_StillNotFoundOrConflict` | soft-deleted patient | the pre-existing soft-delete conflict, not the new one |
| `Clear_ResultEntered_Conflict` | first visit but a result entered | the pre-existing message, not the new one |
| `Clear_DeletedPriorVisitDoesNotBlock` | the only *earlier* visit is soft-deleted | success |
| `Clear_GuardRunsBeforeTwentyFourHourRule` | non-first visit **and** older than 24 h | the new message |

**MODIFY `tests/TopLab.Presentation.Tests/Common/Fakes.cs`** — as described in R-F05-S3; the `FakeSender` throw at `:152` is a real blocker for any Statistics ViewModel test and must be handled.

**NEW Presentation test** — `CanClearAllVisitTests` false on a non-first visit, true on a first visit, refreshed after add/remove/clear, and the Arabic message on a stale command.

### Regression risks to existing tests

| Risk | Why it is real | Mitigation |
|---|---|---|
| `StatisticsAuthorizationTests` `:20/:41/:57` | positional record grows from 6 to 8 parameters — **hard compile break** | fixed inside R-F01-S1, before the build gate |
| `ClearAllTestsCommandHandlerTests.Clear_HappyPath_RemovesAll` (`:14-30`) | a new guard sits between the soft-delete check and the load | verified by reading the test: one patient, no `LabId`, no competing visits → passes under the recommended OD-3 definition |
| `PresentationLayeringTests.PresentationLayering_NoInfrastructureReferenceOutsideAppXaml` | `StatisticsViewModel` must **not** name `TopLab.Infrastructure`; the test asserts an **exact** offender list of three files (`Layering/PresentationLayeringTests.cs:38-40`) | the port-based design keeps this clean; do not "simplify" by constructing the writer directly |
| `ValidatorRegistrationTests` | a new validator that is not in the theory data is never resolved by FluentValidation, and the failure is silent | add it in the same slice |
| `OrphanedViewModelTests` (`:8-50`) | every registered entry screen must have a `NavigateTo<>` caller | `StatisticsViewModel` is already registered and already navigated; the monitor is a **section inside** it, not a new screen, so no new caller is needed — do not split it into a new ViewModel |
| `PresentationStructuralTests` (`PresentationStructuralTests.cs:12-13`) | resolves `x:Type`/binding names against real CLR members | every new `{Binding ...}` in `StatisticsView.xaml` must name a real public property or the test fails |
| `ReportPdfWriterGoldenTests` / `ReportPrintingServiceTests` | they assert on `ReportContentBuilder` output | untouched by R-F05 and R-F01; R-A04-S3 changes profile-export output only if OD-4 = "all" |

---

## G. Acceptance criteria

### R-F05
1. Entering a test, min, max and period returns a grid whose rows are exactly the `PatientTest` rows for that test with `EnteredAtUtc` in the period whose parsed `ResultValue` lies in `[Min, Max]`, inclusive.
2. Non-numeric, null and whitespace results never appear and never produce an error.
3. Soft-deleted patients' rows never appear.
4. Each row shows date, patient name, patient number, sex, age, referral entity, test name, result and status; a null referral renders `بدون جهة إحالة`.
5. `Min > Max` and `From > To` are rejected with the exact Arabic messages in BR-F05-6/7.
6. The query declares `IAuthorizedRequest` → `"STATISTICS"` and is denied without the grant, exactly like the other four statistics queries.
7. With the `STATISTICS` grant absent, the MediatR pipeline denies the request with `أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام`.
8. The print action writes a PDF containing the lab header, the criteria (test, band, period) and the same nine columns, at a path the user chooses.
9. The writer refuses to overwrite an existing file.
10. **No file under `src/TopLab.Infrastructure/Persistence/Migrations/` is added or changed, and no `PrinterOutputType` value is added.**

### R-F01
1. With `GroupByDayOfMonth` on, the response carries day-of-month buckets ordered by day, each counting only registrations inside the period and excluding soft-deleted patients.
2. With the flag off, the collection is empty and no behaviour elsewhere changes.
3. With `IncludeMoneyRow` on, the response carries one money row whose figure equals `PatientAccountCalculator.TotalPaid` over the period patients' non-voided, non-extra-charge operations.
4. The money row carries a payment count, so zero money and zero payments are distinguishable.
5. With the flag off, the money row is `null` and no `PaymentOperation` row is read.
6. All five pre-existing classifications (sex, referral incl. no-referral bucket, account type, month, month×sex) and `TotalCount` produce byte-identical results to HEAD when both new flags are off.
7. The screen offers both new controls and renders the day grid and the money line.
8. **No migration; no EF model change.**

### R-A04
1. **Verified result, recorded as delivered:** the combined clinical report emits `العينة أُخذت خارج المعمل` on exactly those lines whose `PatientTest.IsTakenOutsideLab` is true, positioned immediately after the result line and before the frozen range — evidence `ReportContentBuilder.cs:222-225`, data path `BuildCombinedReportCommandHandler.cs:232` → `ReportDtos.cs:71` → `ReportContentBuilder.cs:222`.
2. **Verified result, recorded as delivered:** the note is **absent** from the patient PDF export (`ReportContentBuilder.cs:71-100`; `PatientReportPdfPort.cs:5-14` carries no flag), from the history report (`:382-405`), and from the specialised-profile report (`:141-159` plus `ResultPrintCoordinator.cs:101-133`, which passes nine positional arguments and leaves the flag at its `false` default).
3. Bulk clear is refused with `Error.Conflict` and the exact Arabic message whenever the visit is not the patient's first registration, per OD-3.
4. The refusal happens in the **Application handler**, so it holds for any caller, not only the button.
5. The `مسح الكل` button is disabled when the guard would refuse, and a stale command invocation still produces the same Arabic message.
6. Pre-existing guards still fire: not-found, soft-deleted, 24-hour, and result-already-entered.
7. Adding and removing individual tests is untouched — the double-click gesture of the reference is explicitly **out of scope** for Batch 1 (it is a UI-gesture preference, not a missing capability; see §J).
8. **No migration.**

---

## H. Risks and edge cases

| # | Risk / edge case | Handling |
|---|---|---|
| H-1 | `PatientTest.ResultValue` is a **`string?`**, so a band query is a numeric-parse-and-filter, not a SQL range | parse per BR-F05-5 using the existing rule. At current volumes the in-memory style of the other statistics handlers is adequate; a SQL-side `TRY_CONVERT` would be a separate, later decision and is **out of scope** |
| H-2 | Arabic-Indic digits or a comma decimal separator typed by the operator | the VM parses with `NumberStyles.Number, CultureInfo.InvariantCulture` — the existing pattern at `PatientEditorViewModel.cs:1228`. `ResultFlagComputer` itself uses `NumberStyles.Any, InvariantCulture`, so `","` **is** accepted there. **This is a real inconsistency**: a value typed with an Arabic-Indic separator can pass the flag computer yet be excluded from the monitor. Recommendation: use `NumberStyles.Any` in the VM parse to match `ResultFlagComputer` exactly, and raise this with the owner as **OD-4b** |
| H-3 | Band matching on culture or profile tests | `ResultValue` is null for `ResultKind.SpecializedProfile`/`Culture` (`:32` is only written by `EnterResult` on simple results), so they are silently excluded. Document this in the screen help text; do not attempt to band analyte values in this batch |
| H-4 | Empty result set | return a success with `Rows == []` and `TotalCount == 0`; the VM shows the empty-state text. Never an error |
| H-5 | Very wide band over a long period | could return a large grid. No paging is planned (matches every other statistics query). Flagged, not solved |
| H-6 | A `PaymentOperation` whose `OperationAtUtc` falls in the period but whose patient registered earlier | the patient set is defined by the period filter; the money is scoped to that set. Whether that is the intended business meaning is **OD-2** |
| H-7 | `PaymentOperation.DiscountAmount` semantics | `TotalPaid` adds `Amount + DiscountAmount` (`PatientAccountCalculator.cs:32`). Reusing `TotalPaid` verbatim is the point — the money row must equal what the balance probe would report, or the two screens will disagree |
| H-8 | Voiding a payment after the fact | historical statistics recompute, so a voided payment silently disappears from a past period's money row. This is correct-by-definition for a query, but must not surprise the operator |
| H-9 | The bulk-clear guard denies a legitimate correction | that is the point of a guardrail, but if OD-3 resolves to "first **visit** in the `LabId` group", a returning patient with a `LabId` gets a clear message rather than silent behaviour change. The UI disabling the button prevents a dead end |
| H-10 | The first-registration guard depends on `RegistrationDateUtc`, which no update path can change | **resolved:** `grep -n "RegistrationDateUtc" UpdatePatientCommand.cs UpdatePatientCommandHandler.cs` → zero matches, and `Patient.Update` (`Domain/Patients/Patient.cs:158-205`) has no date parameter. The registration timestamp is immutable after creation, so the guard's answer is stable for the life of a visit |
| H-11 | The profile-report path silently renders `IsTakenOutsideLab` as `false` today | a real defect. If OD-4 = no-change, this must be recorded as an accepted known gap, not forgotten |
| H-12 | New PDF writer and QuestPDF static initialisation | every writer sets `License`/`UseSystemFonts` in its **own** static ctor — the owner ruling at `PriceListPdfWriter.cs:26-28`. Follow it exactly |
| H-13 | `PresentationLayeringTests` asserts an exact offender file list | adding an Infrastructure `using` to `StatisticsViewModel` fails a shipped test |
| H-14 | Growing the `GetPatientCountStatisticsQuery` positional record breaks three call sites | handled inside R-F01-S1, before the gate |

---

## I. Open decisions

**This plan is marked NOT FINAL.** Three open decisions are material and one is minor.

---

### OD-1 — Test picker data source for the R-F05 monitor screen

**Options**

| Option | Consequence | Operational impact |
|---|---|---|
| **A. `SearchTestCatalogQuery(null, null, IncludeInactive: false)`** — reuse the un-paged catalog query (`SearchTestCatalogQuery.cs:7`) | Returns **every** active `TestSummaryDto`. No new query needed. `TestSummaryDto` (`TestCatalogAndReferenceRanges/Common/…:5-17`) has everything the monitor needs | For a lab with hundreds of tests the ComboBox is a long unwrapped list. No search-as-you-type. Acceptable at current catalog size; degrades with growth |
| **B. Add a new paged `SearchTestsForMonitorQuery`** with a term and a cap | Cleanest UX; two queries for one screen | A new query, validator, authorization entry and handler tests — more surface than the feature warrants |
| **C. Reuse `GetTestGroupsQuery` for a group filter + A** | Group-then-test narrowing with no new query | Two dropdowns on a screen that wants one input |

**Recommendation: A.** One requirement is one query. A paged test picker is a UX improvement, not part of the missing capability, and the whole point of this batch is to close the capability gap with the least new surface. If the catalog grows past a few hundred tests, B is the correct follow-up.

**Reasoning:** the reference's function is "choose a test and a band" — nothing about search. A implies a new Application query, a new validator, a new authorization test entry and a new handler test file; C adds a control the reference does not have. A delivers the specified behaviour with zero new read paths.

---

### OD-2 — Period anchor for the R-F01 money row

**Options**

| Option | Consequence | Operational impact |
|---|---|---|
| **A. `PaymentOperation.OperationAtUtc` in `[From, To+1)`** — money **received** during the period | Semantically the standard cash-collection statistic. Directly filterable in SQL. Simple to explain: "what did we collect in March?" | A payment collected in March for a December visit lands in March. Cross-period figures will not tie to the visit list |
| **B. Payments of the period's patients, regardless of payment date** — all operations of patients registered in the period | Ties to the patient list on screen; no date filter on operations at all | A December visit paid in March shows under December forever. Not a "period" statistic in the ordinary sense |
| **C. Both: a received-in-period figure plus a lifetime-to-date figure for the period's patients** | Maximum information; two numbers on one screen | Two numbers invite misreading in a clinical-billing context; exceeds what the reference specifies (one money row) |

**Recommendation: A.** "Amounts paid **for the period**" most naturally means money taken in the period, and it is the only option that is a genuine period statistic. It also mirrors how the other three statistics queries already scope by their own timestamp (`GetSentOutStatistics` on `SentAtUtc`, `GetUserProductivityStatistics` on `EnteredAtUtc`/`ReviewedAtUtc`/…).

**Reasoning:** B is not a period statistic — it is a cohort statistic wearing a period label, and it would make the money row disagree with any other money figure the lab computes. C is scope creep. If the owner wants B, the change is localized to the handler's operation filter and nothing else in the plan moves.

---

### OD-3 — Definition of "first registration" for the R-A04 bulk-clear guard

**Options**

| Option | Consequence | Operational impact |
|---|---|---|
| **A. First visit of the patient identity.** The row is the earliest non-deleted `Patient` sharing its `LabId` group (or the only row when `LabId` is null). Comparison on `RegistrationDateUtc`, tie-broken by `PatientId` | Matches the reference phrase `عند إضافة المريض أول مرة فقط`. Deterministic, wall-clock-independent. Computable today from existing rows — no migration | For a walk-in with no `LabId`, only the single row qualifies. For a returning patient with a `LabId`, every later visit is blocked — which is the guardrail's purpose |
| **B. Registered today.** `RegistrationDateUtc.Date == UtcNow.Date`, reusing `PatientStatusCalculator.IsRegistrationToday` (`:93-96`) | Trivial to implement; consistent with the existing S1 rule | A patient registered yesterday and corrected this morning can no longer be bulk-cleared even though nothing has happened to their tests; and the behaviour silently changes at midnight |
| **C. Replace the 24-hour rule with A** | One rule instead of two; the 24-hour window was a proxy for "same session" | **Removes a guard that exists today.** The 24-hour check (`ClearAllTestsCommandHandler.cs:33-36`) currently blocks clearing a same-day order at 23:00 that was created at 01:00 only under A too — but it also blocks a *first* visit from being corrected the next morning. Removing it is a net loosening |
| **D. Keep 24 h **and** add A** | Both rules; the tightest wins | Strictest option: only a same-day **first** visit is clearable. The most likely to frustrate reception staff |

**Recommendation: A, and keep the existing 24-hour rule (i.e. option A added on top of today's behaviour — which is D's rule set but justified by A's definition).** A is the faithful reading of the reference. Keeping the 24-hour rule is the conservative choice: it is shipped behaviour with an existing test and an existing Arabic message, and removing it (C) is a loosening the owner did not ask for.

**Reasoning:** B conflates "first visit for this patient" with "today", which are different facts and diverge exactly when it matters. A is computable without touching the schema — `Patient` rows for the `LabId` group are already loaded by the pattern at `GetVisitHistoryQueryHandler.cs:40-44`. The cost of A over B is one extra indexed-by-`LabId` read; the cost of the alternatives is either a midnight behaviour cliff (B) or a silent removal of a shipped guard (C). If the owner wants the pure reference rule with nothing else, C is a defensible one-line follow-up.

---

### OD-4 — Scope of the outside-lab report note (R-A04(b))

The **verification is complete and is not a decision**: the note renders on the combined clinical report and nowhere else. What remains is how much of the gap to close in this batch.

| Option | Consequence | Operational impact |
|---|---|---|
| **A. No code change.** Record the verified result and stop | Smallest batch. Zero risk to any report path. The three gaps stand as documented known limitations | The profile report — which a lab uses for every profile test — never shows the note, even when the flag is set. That is a genuine clinical-documentation gap left open |
| **B. Close the profile path only** | `ResultPrintCoordinator.cs:101-133` must pass the flag; `ProfileReportDto` and `GetProfileReportQueryHandler` must expose it (widening into `Features/ProfileResults`). One new member on one DTO, no new entity | Fixes the case that actually matters operationally. Leaves export and history unchanged |
| **C. Close all three** (profile + patient PDF export + history grid column) | Adds a member to `PatientReportPdfLine` (`PatientReportPdfPort.cs:5-14`), a parameter in `ExportPatientReportPdfCommandHandler.cs:146-155`, and a sixth column in `FromHistory` (`:403-405`). The history column also changes the history report's visual layout, which may affect a golden test | Complete parity. Widest blast radius of the three options; touches three features for one sub-item |

**Recommendation: B.** The profile path is not a parity nicety — it is a silent data loss on the report a lab prints for every profile test, and the fix is a handful of lines with no layout change. The PDF export and the history grid are genuine additions rather than defects, and the export path is a *saved file* rather than *the printed report* the reference item names.

**Reasoning:** A leaves a known defect undocumented-as-defect. C violates the batch's own scope discipline for a sub-item that is one line of a much larger question. B closes the defect and stops.

**Consequence for the plan:** slice R-A04-S3 runs only under B or C. Under A it is skipped and the verification stands as delivered.

---

### OD-4b (minor) — Numeric input parsing culture in the R-F05 band inputs

`ResultFlagComputer` parses stored results with `NumberStyles.Any, InvariantCulture` (`ResultFlagComputer.cs:77`), so a comma decimal separator is accepted when the result is entered. If the monitor's min/max inputs parse with the narrower `NumberStyles.Number` (the pattern at `PatientEditorViewModel.cs:1228`), an operator who types `3,5` gets a validation error for a band that is perfectly legitimate in the stored data. **Recommendation: parse the band inputs with `NumberStyles.Any, InvariantCulture`, matching `ResultFlagComputer` exactly.** This is a one-line implementation detail rather than a product decision, but it is flagged so it is not decided silently.

---

## J. Completeness check

**The three items in Batch 1 are fully covered:**

| Item | Sub-requirement | Covered by |
|---|---|---|
| **R-F05** | banded result monitor over a test + min + max + period | BR-F05-1…16; slices S1–S3; file inventory C.1 |
| **R-F05** | grid with date, patient, patient data, referral, test, result, status | BR-F05-5, 10, 12; `BandedResultRowDto`; AC 2–5 |
| **R-F05** | printable report | BR-F05-13…16; slice S2; AC 8–9 |
| **R-F01** | (a) grouping by day of the month | BR-F01-1…5; slice R-F01-S1/S2; AC 1–2 |
| **R-F01** | (b) amounts-paid money row | BR-F01-6…12; slice R-F01-S1/S2; AC 3–5 |
| **R-A04** | (a) first-registration-only guard on clear-all | BR-A04-1…5; slices A04-S1/S2; AC 3–6 |
| **R-A04** | (b) taken-outside-lab note on the printed report | **verified** in §A.3(b) from `ReportContentBuilder.cs` read in full (448 lines); AC 1–2 record the verified result; slice A04-S3 conditional on OD-4 |

**No excluded function appears anywhere in this plan.** Checked explicitly:

1. **Online results portal** (netija.com-style site, receipts, doctor/lab accounts, Android app) — **absent**. No plan slice touches `ResultDelivery`, no account entity, no web or mobile surface.
2. **Equipment / maintenance / calibration register** — **absent**. No slice references equipment, maintenance, or calibration. R-F05 is explicitly a monitor over **manually entered** results (`PatientTest.ResultValue`), consistent with the classification note in `Cross-comparison.md:664`.
3. **Blood / CBC analyser connection or import** — **absent**. No slice references a device, HL7/ASTP protocol, serial port, or file import. R-F05 reads only rows a human entered.
4. **Multi-branch support** — **absent**. No slice touches `Patient.BranchNumber` (added by `20260930170644_AddBranchNumber`) or any branch filter. The R-F05 monitor is scoped by test and period only, **not** by branch.
5. **SMS / E-mail / Fax result sending** — **absent**. No slice touches `Features/ResultDelivery`, and no slice sends anything anywhere. The only output R-F05 produces is a PDF written to a path the user picks.

**Also confirmed out of scope, deliberately:** the reference's double-click-to-add/remove gesture for individual tests. A gesture is a UI preference over a capability that is already fully present and working (`AddTestsToVisit`, `RemoveTestFromVisit`, `UpdatePatientTestSampleFlags` — all verified present). R-A04 is planned as the two items named in the lead: the clear-all guard and the report note. Splitting it out would require new plumbing (hit-testing, a selected-row concept) for no missing behaviour.

**Isolation confirmed:** this plan was produced without reading any file under `Docs/OpenCode/`, no other file under `Docs/Hermes/`, and no `build.md`.

---

## Execution order for the coding agent

`R-F05-S1 → R-F05-S2 → R-F05-S3 → R-F01-S1 → R-F01-S2 → R-A04-S1 → R-A04-S2 → R-A04-S3 (conditional)`

Each slice: implement → `dotnet build` 0 warnings / 0 errors → `dotnet test` green → local commit `بعد تنفيذ F <slice>` → **stop and wait for an explicit "go"**.

**The owner performs every `git commit` and `git push`. The executing agent commits nothing and pushes nothing without an explicit instruction.**
