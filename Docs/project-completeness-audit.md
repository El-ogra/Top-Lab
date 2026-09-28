# Project Completeness Audit — Top-Lab (Laboratory Information System)

Audit type: independent, evidence-driven, read-only completeness audit.

---

## 1. Verdict

**Verdict: NOT COMPLETE**

**Verdict Confidence: Limited.** The following checks could not be executed in this environment and are listed as Unverified Items in Section 11: (a) running the WPF application (no Windows/WPF runtime on this Linux audit host); (b) executing EF Core migrations against a real SQL Server (no SQL Server instance or `sqlcmd`/`sqlpackage` tooling available; `dotnet ef` not installed); (c) model-vs-snapshot drift check via `dotnet ef migrations has-pending-model-changes`; (d) interactive UI navigation verification. Environment limits excuse only execution; all layers were still inspected by reading source.

**Justification (evidence-based):** The solution builds cleanly (0 warnings, 0 errors) and 2,085 of 2,088 executed tests pass. However, the audit established two Major gaps with direct code evidence: (1) the Home dashboard's four primary quick-action buttons are wired to nothing (empty `HomeViewModel`, buttons with no `Command`), and (2) for a product described and sold as a *commercial* system, there is no licensing/activation/enforcement implementation anywhere in the codebase. In addition, three printing happy-path tests fail in this environment with the cause not fully isolated (treated as unverified, not as a defect, per the binding instruction on environment limits). These Major gaps prevent the product from being considered complete under the audit's severity model.

- Blocker findings: **0**
- Major findings: **2**
- Minor findings: **4**

---

## 2. Verification Metadata

- **Repository:** https://github.com/El-ogra/Top-Lab
- **Pinned commit (required):** `7a5a814a5ea8a0c143fad1df0f195059db5088a4`
- **Verified HEAD:** `7a5a814a5ea8a0c143fad1df0f195059db5088a4` — exact match. Commit subject: `[S-06] Slice 6/7: Utilities screen + «الأدوات» wiring — loop-engineering`, dated 2026-09-19 00:56:46 +0300 (author-local timezone).
- **git status:** `HEAD detached at 7a5a814`, `nothing to commit, working tree clean` (verified again after all audit activity — tree remained clean; no repository file was modified; the report file was written outside the repository).
- **Audit environment:** Linux x86_64 sandbox (`Linux sbx-002ad584 6.18.15`), no Windows desktop, no WPF runtime, no SQL Server, no `sqlcmd`/`sqlpackage`.
- **Tooling installed for the audit:** .NET SDK **8.0.425** (installed to a private directory for this audit; the base image had no `dotnet`).
- **Commands executed (actual):**
  - `git clone https://github.com/El-ogra/Top-Lab.git`
  - `git checkout 7a5a814a5ea8a0c143fad1df0f195059db5088a4`
  - `git rev-parse HEAD` → `7a5a814a5ea8a0c143fad1df0f195059db5088a4`
  - `git status --short` → (empty)
  - `dotnet restore TopLab.sln` → **FAILED** with `error NETSDK1100: To build a project targeting Windows on this operating system, set the EnableWindowsTargeting property to true` (expected on non-Windows; an SDK/environment property requirement, not a repository defect — `TopLab.Presentation.csproj` targets `net8.0-windows` and does not set `EnableWindowsTargeting`, which is normal for a Windows-only app).
  - `dotnet restore TopLab.sln /p:EnableWindowsTargeting=true` → **exit 0**, all 7 projects restored.
  - `dotnet build TopLab.sln --no-restore /p:EnableWindowsTargeting=true` → **Build succeeded. 0 Warning(s), 0 Error(s).**
  - `dotnet test tests/TopLab.Domain.Tests --no-build` → **Passed! Failed: 0, Passed: 474, Skipped: 0, Total: 474**
  - `dotnet test tests/TopLab.Application.Tests --no-build` → **Passed! Failed: 0, Passed: 1419, Skipped: 0, Total: 1419**
  - `dotnet test tests/TopLab.Infrastructure.Tests --no-build` → **Failed! Failed: 3, Passed: 192, Skipped: 0, Total: 195**
- **Commands that could not be executed:** any `dotnet ef` / migration-against-database command (no SQL Server, `dotnet ef` not installed); launching the WPF app (no Windows desktop). Reasons recorded above.

Solution layout (actual): `TopLab.sln`; projects `src/TopLab.Domain`, `src/TopLab.Application`, `src/TopLab.Infrastructure`, `src/TopLab.Presentation` (WPF, `net8.0-windows`, `win-x64`), and tests `TopLab.Domain.Tests`, `TopLab.Application.Tests`, `TopLab.Infrastructure.Tests`. Central package management via `Directory.Packages.props`. Source scale: 1,361 `.cs` files, 70 `.xaml` files, 219 handler files, 154 validator files, 296 test files containing 1,785 `[Fact]`/`[Theory]` attributes, 8 EF Core migrations + model snapshot.

---

## 3. Scope Definition

The module inventory was derived from **code signals first**: the 25 feature folders under `src/TopLab.Application/Features/`, the 14 domain aggregate folders under `src/TopLab.Domain/`, the View/ViewModel folders under `src/TopLab.Presentation`, the three `DependencyInjection.cs` registration files, the shell navigation in `ShellViewModel.BuildNavigationItems()`, and the 8 migrations. Documentation (`Docs/Source/Top_Lab_PRD.md`, `Docs/Source/Top_Lab_Architecture_Blueprint.md`, `Docs/OpenCode/S-*.md`, handoff files) was used only to cross-check intended scope, never as implementation proof.

**Documentation-derived scope:** a full LIS — patient registration, test catalog with reference ranges, sample pipeline, worksheets, result entry/validation, culture/antibiotics, billing/payment, result delivery, sent-out samples, external entities (doctors/referral labs), users/permissions, attendance, accounting, statistics, audit, printing/reporting, settings, utilities, backup — for LAN multi-device SQL Server deployment.

**Code-derived scope (final module list):** all of the above exist as feature folders with handlers, validators, views and view models; details per module in Section 4.

**Discrepancies identified:**
- A. Documented-but-thin in code: `SamplePipeline` exists as a feature folder but contains only one operation (`EchoName`); no broader pipeline management commands were found there (sample draw workflow actually lives in `SampleCollection` / `PatientRegistration.UpdatePatientTestSampleFlags`). Not a missing feature, but the folder name overstates its content.
- D. Implemented-but-unreachable-from-intended-entry-point: the Home dashboard quick actions (see F-01, Major).
- No fully documented-but-absent module was found; no significant undocumented module was found.

---

## 4. Module Matrix

Layer statuses are based on code inspection of handlers, DI registrations, views/view models, and executed test results. "Complete" here means "no Blocker/Major completeness gap found in inspected evidence" — it is not a claim of exhaustive line-by-line review of all 219 handlers (see Section 11).

| Module | Domain | Application | Infrastructure | UI | Tests | Overall Status | Evidence |
|---|---|---|---|---|---|---|---|
| Patient Registration | Complete | Complete (12+ command/query sets incl. `CreatePatient`, `UpdatePatient`, `SoftDeletePatient`, `AddProfileToVisit`, `RemoveTestFromVisit`) | Complete (Patient entity, migration columns, LabId duplicate rejection) | Complete (`PatientEditorView`, `PatientsHubView`) | Complete (Application suite green) | Complete | `Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs:28-80`; test run 1419/1419 |
| Patient Search | Complete | Complete | Complete | Complete (`PatientSearchView`) | Complete | Complete | `Features/PatientSearch/`; `Views/Patients/PatientSearchView.xaml` |
| Patient Billing / Payments | Complete | Complete (`RecordPayment`, `SettleAccountInFull`, `RecordCorrection`, `RecordExtraCharge`, `VoidPaymentOperation`, `PrintReceipt`, `PrintInvoice`) | Complete (invoice issues migration `20260916113704_AddInvoiceIssues`) | Complete (`PatientAccountView`, dialogs) | Complete | Complete | `Features/PatientBilling/Commands|Queries` listing; `PatientAccountView.xaml` |
| Results Entry | Complete | Complete (`EnterResult`, `ReviewResult`, `UnreviewResult`, `ClearResult`, `MarkResultPrinted/Delivered`, `RefreshResultReferenceRange`, `ExportPatientReportPdf`, `BulkPrint`) | Complete (reference-range snapshot migration `20260908175555`) | Complete (`SimpleResultEntryView`, `ResultsWorklistView`, `PatientResultSheetView`) | Complete | Complete | `Features/ResultsEntry/Commands|Queries` listing |
| Profile / Analyte Results | Complete | Complete | Complete | Complete (`ProfileEntryView`) | Complete | Complete | `Features/ProfileResults/`; `Features/AnalyteProfiles/`; migration `20260909033414_AddAnalyteProfileDomain` |
| Culture Results | Complete | Complete (`SaveCultureResults`, `VerifyCultureResult`, `UnverifyCultureResult`, `MarkCultureReportPrinted`) | Complete | Complete (`CultureEntryView`, `CultureAttachmentView`) | Complete | Complete | `Features/CultureResults/Commands/` listing |
| Culture & Antibiotics catalog | Complete | Complete | Complete | Complete (`AntibioticsView`, `AntibioticEditorWindow`) | Complete | Complete | `Features/CultureAndAntibiotics/` |
| Test Catalog & Reference Ranges | Complete | Complete (incl. `CreateTestCommandValidator` used as DI anchor) | Complete (lifecycle columns migration `20260906093902`) | Complete (`TestCatalogView`, `TestEditorWindow`, `TestGroupsView`) | Complete | Complete | `Application/DependencyInjection.cs:20`; `Views/Lab/` |
| Analyte Profiles | Complete | Complete | Complete | Complete (`AnalytesView`, `ProfilesView`, editors) | Complete | Complete | `Features/AnalyteProfiles/`; migration `20260909033414` |
| Price Lists / Comments / Custom Groups | Complete | Complete | Complete | Complete (`PriceListsView`, `TestCommentsView`, `CustomGroupsView`) | Complete | Complete | `Features/PriceListsCommentsAndCustomGroups/` |
| Sample Collection | Complete | Complete | Complete | Complete (`SampleCollectionView`, `SampleDrawBoardView/Window`) | Complete | Complete | `Features/SampleCollection/`; `Views/Lab/SampleCollectionView.xaml` |
| Sample Pipeline | Partial (single-op folder) | Partial (`EchoName` only) | Not-Verifiable | Not-Verifiable (no dedicated view found) | Not-Verifiable | Partial | `Features/SamplePipeline/` contains only `EchoName/` |
| WorkSheets | Complete | Complete (`GetVisitWorkSheet`, `GetWorkSheetSummary`, `PrintWorkSheet`, counts by period; permission-gated) | Complete (`WorkSheetPdfWriter`, `WorkSheetPrintingService`) | Complete (`WorkSheetsView`) | Partial (printing happy-path test fails in this env — see Section 11) | Complete with test caveat | `Features/WorkSheets/.../*AccessPolicy*`; `Infrastructure/Printing/WorkSheetPdfWriter.cs` |
| Report Production | Complete | Complete (`PrintBlankReport`, `PrintCombinedReport`, `PrintHistoryReport`) | Complete (`PatientReportPdfExporter`, `ReportPrintingService`, `ReportPdfWriter`) | Complete (`BlankReportView`, `CombinedReportView`, `HistoryReportsView`) | Complete | Complete | `Features/ReportProduction/Commands/`; `Infrastructure/DependencyInjection.cs` registrations |
| Result Delivery | Complete | Complete (`DeliverWithSettlement` with single `SaveChanges`; `GetUndeliveredResults`, `GetDeliveryAccount`, `GetDeliveryGrid`) | Complete | Complete (`ResultDeliveryView`, `DeliveryHandoverView`) | Complete | Complete | `Features/ResultDelivery/Commands/DeliverWithSettlement/DeliverWithSettlementCommandHandler.cs:94` |
| Sent-Out Samples | Complete | Complete | Complete | Complete (`SentOutSamplesView`, `SendSampleOutDialogWindow`, `SentOutLabAccountView`) | Complete | Complete | `Features/SentOutSamples/`; `Domain/SentOutSamples/` |
| External Entities | Complete | Complete | Complete (secure code generator `SecureEntityIdCodeGenerator`) | Complete (`ExternalEntitiesView`, editor, picker) | Complete | Complete | `Infrastructure/DependencyInjection.cs` (`IEntityIdCodeGenerator` registration) |
| Users & Permissions | Complete | Complete (`SignIn`, `SignOut`, `ChangeOwnPassword`, `GetCurrentSession`, `HasAnyAbsoluteUser`, `VerifySecondaryPassword`) | Complete (PBKDF2 hasher, session service) | Complete (`LoginWindow`, `UserManagementView`, `ChangeOwnPasswordWindow`) | Complete | Complete | `SignInCommandHandler.cs:40-56`; `Pbkdf2PasswordHasher` registration |
| Access & Navigation | Complete | Complete (`LockWorkstation`) | Complete | Complete (shell + unlock window) | Complete | Complete | `Features/AccessAndNavigation/Commands/LockWorkstation/`; `ShellViewModel.cs:358` |
| Audit & Traceability | Complete | Complete | Complete (`AuditableEntitySaveChangesInterceptor` sets Created/Modified user+UTC) | Complete (`AuditView`) | Complete | Complete | `Infrastructure/Persistence/Interceptors/AuditableEntitySaveChangesInterceptor.cs:64-74` |
| Attendance | Complete | Complete | Complete | Complete (`MyAttendanceView`, `AttendanceRecordsView`, `UserAttendanceSummaryView`) | Complete | Complete | `Features/Attendance/`; `Domain/Attendance/` |
| Inventory & Accounting | Complete | Complete (`RecordCashDeposit`, `RecordCashDisbursement`, etc.) | Complete | Complete (`AccountsHubView`, `CashMovementDialogWindow`) | Complete | Complete | `Features/InventoryAndAccounting/Commands/` |
| Statistics | Complete | Complete (4 query sets: patient count, test count, sent-out, user productivity) | Complete | Complete (`StatisticsView`) | Complete | Complete | `Features/Statistics/Queries/` listing |
| System & Print Settings | Complete | Complete (`GetSystemSettings`, `UpdateDatabaseServerSettings`, print text store) | Complete (maintenance service, backup/restore raw SQL) | Complete (`SettingsDashboardView`, `SystemSettingsView`, `ReportSettingsView`, `ReceiptSettingsView`, `EnvelopeSettingsView`, `DatabaseMaintenanceView`) | Complete | Complete | `Features/SystemAndPrintSettings/.../UpdateDatabaseServerSettingsCommandHandler.cs:42-45` |
| Utilities | Complete | Complete | Complete (JSON stores `JsonPurchasesListStore`, `JsonPhoneBookStore` — by design no DB table) | Complete (`UtilitiesView`) | Complete | Complete | `Infrastructure/DependencyInjection.cs` M-23 registrations |
| Setup / First-Run | n/a | Complete (`HasAnyAbsoluteUserQuery`) | Complete (`Database.MigrateAsync` on startup) | Complete (`DatabaseSetupWindow`, `FirstRunAdminWindow`, `LoginWindow`) | Complete | Complete | `App.xaml.cs:39-104` |
| Home dashboard quick actions | n/a | n/a | n/a | **Missing wiring** | n/a | **Missing** | `MainWindow.xaml:98-101`; `HomeViewModel.cs` (empty class) — Finding F-01 |
| Licensing / Activation | **Missing** | **Missing** | **Missing** | **Missing** | **Missing** | **Missing** | repo-wide grep evidence — Finding F-02 |

---

## 5. Detailed Gap Register

### F-01 — Home dashboard quick-action buttons are inert
- **Module:** Shell / Home
- **Title:** Four primary quick-action buttons on the Home screen have no command and no action
- **Exact description:** The Home dashboard `DataTemplate` in `MainWindow.xaml` renders four buttons — `إضافة وتعديل بيانات المرضى` (add/edit patient), `إدخال نتائج التحاليل` (enter results), `بحث عن مريض` (patient search), `تسليم نتائج المرضى` (result delivery) — with no `Command` attribute and no `Click` handler. The bound `HomeViewModel` is an empty class (`public sealed class HomeViewModel : ViewModelBase { }`) exposing no commands. Clicking these buttons at runtime does nothing. Note: `Docs/OpenCode/S-05.md` itself describes "تفعيل بوابة «بحث عن مريض»" (activating the search gateway) as planned slice work, confirming these buttons were intended to be wired.
- **Evidence:** `src/TopLab.Presentation/MainWindow.xaml:98-101` (buttons without `Command`); `src/TopLab.Presentation/ViewModels/Shell/HomeViewModel.cs` (empty class, verbatim-inspected).
- **Severity:** MAJOR
- **Evidence level:** VERIFIED-BY-CODE-INSPECTION
- **Impact:** The application's landing screen advertises the four most common daily workflows and none of them respond. The same functions remain reachable via the top navigation bar (`المرضى`, `المعمل`, …), so the product is not blocked — but a prominent, intended UI path is dead.
- **Required correction:** Add commands to `HomeViewModel` navigating to `PatientEditorViewModel`/`PatientsHubViewModel`, `ResultsWorklistViewModel`, `PatientSearchViewModel`, and `ResultDeliveryViewModel` via the existing `INavigationService`, and bind the four buttons.
- **Acceptance condition:** Each button invokes its navigation target; covered by a ViewModel-level test or documented manual verification on Windows.

### F-02 — No licensing / activation / enforcement implementation
- **Module:** Cross-cutting (commercial readiness)
- **Title:** A product described as a commercial LIS contains no licensing subsystem of any kind
- **Exact description:** The product description defines this as a *commercial* desktop LIS. A repository-wide search for licensing concepts (`Licen[cs]e`, `Activation`, `Trial` across all `.cs` files; `licen`/`رخصة`/`تفعيل` across documentation) found only QuestPDF library-license comments in three printing files (`WorkSheetPdfWriter.cs:23-24`, `ReceiptPdfWriter.cs:24-25`, `InvoicePdfWriter.cs:22-23`) and NuGet package-licensing notes in docs. There is no license entity, no activation command/handler, no license check at startup (`App.xaml.cs` startup sequence is: config → migrate → first-run admin → login → main window; no license step), no grace/expiry logic, and no documented licensing design to defer to.
- **Evidence:** grep across `src --include="*.cs"` for `Licen[cs]e|Activation|Trial` → only the three QuestPDF comment hits listed above; `App.xaml.cs:30-115` (startup sequence with no license stage).
- **Severity:** MAJOR (impact on the *commercial* completeness claim; per the severity model this is a substantial missing capability for a product intended for sale. If the owner confirms licensing is intentionally out of product scope, this finding downgrades to Minor.)
- **Evidence level:** VERIFIED-BY-CODE-INSPECTION (evidence of absence, repo-wide)
- **Impact:** The software can be copied and run on any machine without restriction; the commercial distribution model has no technical enforcement.
- **Required correction:** Define the licensing model (machine-bound key, offline activation, expiry), implement validation at startup with a defined failure mode, and document it.
- **Acceptance condition:** Startup refuses (or degrades in a defined way) without a valid license; bypass paths reviewed; tests cover valid/invalid/expired states.

### F-03 — No persistent application logging
- **Module:** Cross-cutting (error handling / logging)
- **Title:** The only `IAppLogger` production implementation writes to `Debug.WriteLine` and its own comment says it is a placeholder
- **Exact description:** `WpfAppLogger` (in `src/TopLab.Presentation/DependencyInjection.cs`, bottom of file) implements `IAppLogger.Log` with `System.Diagnostics.Debug.WriteLine(...)` and the comment "Minimal console logging; can be replaced with proper logger later". `Debug.WriteLine` output is dropped in Release builds without a debugger/trace listener, so in production there is effectively no persisted log of request outcomes, failures, or the backup service's skip events (`DailyBackupHostedService` also logs "backup skipped, empty path" through the same sink).
- **Evidence:** `src/TopLab.Presentation/DependencyInjection.cs` (class `WpfAppLogger`); `src/TopLab.Infrastructure/Backup/DailyBackupHostedService.cs:65`.
- **Severity:** MINOR (does not block functional workflows; degrades operability/supportability of a multi-device LAN deployment)
- **Evidence level:** VERIFIED-BY-CODE-INSPECTION
- **Impact:** Failures on a customer workstation leave no durable trace; diagnosing LAN/DB/printing incidents in the field is severely hampered.
- **Required correction:** File-based rolling logger (e.g., under `%ProgramData%\TopLab\logs`) behind the same `IAppLogger` interface.
- **Acceptance condition:** A forced failure produces a persisted log entry on disk in a Release build.

### F-04 — Shell navigation is not permission-filtered
- **Module:** Access & Navigation
- **Title:** All top-level navigation items are created with `IsEnabled = true` for every signed-in user
- **Exact description:** `ShellViewModel.BuildNavigationItems()` constructs all 12 navigation entries (`المرضى`, `المعمل`, `ورقة العمل`, `الأدوات`, `الحسابات`, `الإحصائيات`, `المستخدمون`, `النظام`, `الإعدادات`, `حول البرنامج`, `قفل المحطة`, `خروج`) with `IsEnabled = true` unconditionally. The `المستخدمون` (Users) entry is gated only by a secondary-password dialog (`ShowSecondaryPasswordDialogAsync`), not by the permission system. Server-side enforcement does exist and is sound: `AuthorizationBehavior` rejects unauthorized requests via `IAuthorizedRequest.RequiredPermissionCode` with an `IsAbsolutePermission` bypass, and many requests (e.g., `DeliverWithSettlementCommand`, `PrintWorkSheetCommand`) declare required permission codes. The gap is UI-level only: users see and can open screens whose actions will then be rejected.
- **Evidence:** `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs:158-280` (`IsEnabled = true` for all items); `src/TopLab.Application/Common/Behaviors/AuthorizationBehavior.cs` (verbatim-inspected); `Features/ResultDelivery/Commands/DeliverWithSettlement/DeliverWithSettlementCommand.cs:14`.
- **Severity:** MINOR (application-layer enforcement closes the security hole; this is a usability/expectation gap)
- **Evidence level:** VERIFIED-BY-CODE-INSPECTION
- **Impact:** Non-privileged users can navigate into modules where every action fails with a forbidden error.
- **Required correction:** Filter/disable navigation items from the current session's granted permission codes (already available via `GetCurrentSessionQuery`).
- **Acceptance condition:** A restricted test user sees only permitted modules; app-layer behavior tests remain green.

### F-05 — Test-evidence limitation: no SQL-Server-backed persistence/integration tests
- **Module:** Tests (cross-cutting)
- **Title:** All executed tests run against EF Core InMemory or pure unit seams; nothing exercises the real SQL Server provider, migrations, or constraints
- **Exact description:** The Infrastructure suite constructs contexts via `InMemoryContextFactory.Create()` (e.g., `tests/TopLab.Infrastructure.Tests/Printing/ReceiptPrintingServiceTests.cs:79-84`). The InMemory provider does not enforce relational constraints, indexes, raw SQL (`SqlServerDatabaseMaintenanceService` uses `BACKUP DATABASE ... TO DISK` raw commands), or migration behavior. The 2,088 tests are meaningful at the domain/application level (they assert handler behavior, validation, authorization denials, DTO content mapping — not mock-only tautologies, per inspection of representative test files), but the persistence layer's correctness against SQL Server is unproven by the suite.
- **Evidence:** `tests/TopLab.Infrastructure.Tests/Printing/ReceiptPrintingServiceTests.cs:79-84`; `src/TopLab.Infrastructure/Persistence/Maintenance/SqlServerDatabaseMaintenanceService.cs:61-64`.
- **Severity:** MINOR (test-coverage gap, not a functional defect)
- **Evidence level:** VERIFIED-BY-CODE-INSPECTION
- **Impact:** Migration or provider-specific defects (e.g., in the 8-migration chain) would not be caught by CI.
- **Required correction:** Add a SQL Server (container/Testcontainers) integration test project running `MigrateAsync` on a fresh database plus smoke tests per aggregate.
- **Acceptance condition:** CI job creates a fresh database from migration `20260828052248_BaselineDataModel` through `20260916113704_AddInvoiceIssues` and runs persistence smoke tests green.

### F-06 — `SamplePipeline` feature folder contains only one operation
- **Module:** Sample Pipeline
- **Title:** Feature folder suggests a pipeline-management module; only `EchoName` exists inside
- **Exact description:** `src/TopLab.Application/Features/SamplePipeline/` contains a single subfolder, `EchoName`. Sample draw/collection workflow is actually implemented elsewhere (`SampleCollection`, `UpdatePatientTestSampleFlags`). No view named for a sample pipeline was found. This is an inventory/documentation-accuracy issue rather than a missing workflow.
- **Evidence:** directory listing of `Features/SamplePipeline/`; presence of `SampleCollectionView.xaml` and `UpdatePatientTestSampleFlagsCommand*`.
- **Severity:** MINOR
- **Evidence level:** VERIFIED-BY-CODE-INSPECTION
- **Impact:** Module inventory confusion; risk that a future reader assumes pipeline stages (receive → in-process → complete) are managed here when they are not.
- **Required correction:** Either fold `EchoName` into `SampleCollection` or document the folder's intended scope.
- **Acceptance condition:** Module inventory in docs matches feature folders one-to-one.

---

## 6. Cross-Cutting Findings

1. **Database & migrations.** 8 migrations in a coherent timestamped chain (`20260828052248_BaselineDataModel` → `20260916113704_AddInvoiceIssues`) plus `ApplicationDbContextModelSnapshot.cs` (124 entity configuration entries). Baseline creates 36 tables. `ApplicationDbContext` intentionally exposes no `DbSet<T>` properties — it implements `IApplicationDbContext.Set<TEntity>()` generically and calls `modelBuilder.ApplyConfigurationsFromAssembly(...)` (`ApplicationDbContext.cs:26-42`). Startup runs `db.Database.MigrateAsync()` (`App.xaml.cs:64`). **Applying migrations to a fresh SQL Server database could not be executed (no SQL Server available) — Unverified Item U-01.** Seed data exists via `HasData` (e.g., `ReceiptSettings`, `PrinterAssignment`, pregnancy condition type seed migration `20260910213833`). Model-vs-snapshot drift check unexecuted — U-02.
2. **Authentication.** Verified by inspection: PBKDF2 hashing (`Pbkdf2PasswordHasher`, singleton), `SignInCommandHandler` verifies hash, rejects inactive users, and establishes a session via `ICurrentUserService.SetSession(...)` with granted permission codes (`SignInCommandHandler.cs:40-56`). First-run flow verified: `HasAnyAbsoluteUserQuery` gates `FirstRunAdminWindow` (`App.xaml.cs:74-90`). Workstation lock/unlock exists (`LockWorkstationCommand`, `UnlockWindow`).
3. **Authorization.** Central pipeline enforcement (`AuthorizationBehavior`) returns a structured Forbidden result in Arabic for requests implementing `IAuthorizedRequest`; absolute users bypass. Permission codes are declared per-request (evidence: `ResultDeliveryAccessPolicy.DeliverResults`, `WorkSheetsAccessPolicy.PrintWorksheet`). UI-level filtering gap recorded as F-04. No bypass path was found in inspected handlers — handlers do not re-implement or skip the check.
4. **First-run/startup.** Full chain inspected: optional `%ProgramData%\TopLab\appsettings.json` → `DatabaseSetupWindow` if no connection string → DI → `MigrateAsync` with Arabic error dialog on failure → first-run admin → `LoginWindow` → `MainWindow` (`App.xaml.cs:30-115`). Failure handling present at each stage (shutdown with message).
5. **Configuration.** Connection string is workstation-local, never stored in the DB (comment in `Infrastructure/DependencyInjection.cs` citing ADR-0021); `appsettings.example.json` documents `ConnectionStrings:TopLab` defaulting to `(localdb)\mssqllocaldb`. `UpdateDatabaseServerSettingsCommandHandler` builds both integrated-security and SQL-auth connection strings (`:42-45`). Password exposure is mitigated by `SqlServerConnectionDescriptor` exposing only Server/Database segments. No secrets committed to the repo (only the example file).
6. **LAN multi-device operation.** The architecture supports it by configuration: each workstation points `ConnectionStrings:TopLab` at a shared SQL Server (SQL auth supported, `TrustServerCertificate=True`). No client-side caching of shared mutable state was found in inspected code (DbContext is scoped per operation; only identity/session are singletons, which is correct for a single-user desktop). **Concurrent multi-workstation behavior (locking, optimistic concurrency) was not fully traced — Unverified Item U-06.**
7. **Licensing.** Absent — see Finding F-02 (Major).
8. **Printing & reporting.** Genuinely implemented, not stubbed: QuestPDF writers for receipt/invoice/worksheet (`ReceiptPdfWriter`, `InvoicePdfWriter`, `WorkSheetPdfWriter`) with real RTL Arabic layout code and pure text-mapping functions for unit tests; report pipeline (`PatientReportPdfExporter`, `ReportPrintingService`, `ReportPdfWriter`); barcode rendering (`ZXing.Net`, `BarcodeService`, `BarcodeLabelRenderer`); OS shell dispatch via `ShellPdfPrinterDispatcher`; printer routing via `PrinterAssignment` seed data; envelope/receipt/report settings views. Each writer sets `QuestPDF Settings.License = LicenseType.Community` with a comment that the owner confirmed license fit — a commercial-distribution consideration the owner should re-confirm, but not a code defect. The three printing happy-path tests fail in this Linux environment (see Section 11, U-03); the non-printing Infrastructure tests (192) pass.
9. **Error handling & logging.** Structured `Result`/`Result<T>` with typed errors (`Error.Validation/NotFound/Conflict/Forbidden`) flows from handlers to a `ResultErrorPresenter`. Startup failures surface Arabic MessageBoxes. No broad `catch {}` swallowing was found in inspected handlers. Persistent logging is the weak point — F-03.
10. **Data integrity & audit trail.** `AuditableEntitySaveChangesInterceptor` stamps `CreatedByUserId/CreatedAtUtc/LastModifiedByUserId/LastModifiedAtUtc` on every `IAuditableEntity` from the singleton current-user session (`AuditableEntitySaveChangesInterceptor.cs:64-74`). Amendments and corrections have dedicated UI (`AmendDialogWindow`, `AmendmentsLogWindow`, `CorrectionDialogWindow`) and an `AuditView`. Result lifecycle commands (`ReviewResult`/`UnreviewResult`/`VerifyCultureResult`) exist, and reference ranges are snapshotted per patient test (migration `20260908175555`) — a strong traceability design.

---

## 7. End-to-End Workflow Trace

Traced through actual code (entry point → VM → MediatR request → pipeline → handler → persistence). Runtime execution on Windows/SQL Server not possible in this environment; chains are verified by code inspection.

1. **Patient registration.** Entry: top nav `المرضى` → `ShellViewModel` → `NavigateTo<PatientsHubViewModel>()` (`ShellViewModel.cs:252`) → `PatientEditorView`. Action: `CreatePatientCommand` (with `CreatePatientCommandValidator`) → `ValidationBehavior` → `CreatePatientCommandHandler`: loads system settings, validates `LabId` format and rejects duplicates including soft-deleted rows, validates treating-doctor/referral external entities, persists via `IApplicationDbContext` (`CreatePatientCommandHandler.cs:28-80`). **Chain reaches persistence. Works (by inspection).**
2. **Test ordering.** `AddProfileToVisitCommand` / `AddCustomGroupToVisitCommand` / `RemoveTestFromVisitCommand` + validators under `Features/PatientRegistration/Commands/`; price resolution consults `GetPriceListByIdQuery` inside `CreatePatientCommandHandler`. **Works (by inspection).**
3. **Billing/payment.** `PatientAccountView` → `RecordPaymentCommand`, `SettleAccountInFullCommand`, `RecordExtraChargeCommand`, `RecordCorrectionCommand`, `VoidPaymentOperationCommand` under `Features/PatientBilling/Commands/`, queries `GetPatientAccount`/`ListPatientPayments`; receipt/invoice printing commands (`PrintReceiptCommand`, `PrintInvoiceCommand`) route to the QuestPDF writers. **Works (by inspection); physical print dispatch unverifiable here (U-03/U-04).**
4. **Worksheet/work assignment.** `WorkSheetsView` → `GetVisitWorkSheetQuery`, `GetWorkSheetSummaryQuery`, `GetWorkSheetTestCountByPeriodQuery`, `PrintWorkSheetCommand` — all permission-gated by `WorkSheetsAccessPolicy.PrintWorksheet` (e.g., `PrintWorkSheetCommand.cs:15`). PDF: `WorkSheetPdfWriter` (A4 RTL bench sheet). **Works (by inspection); print test caveat U-03.**
5. **Result entry.** `ResultsWorklistView` → `GetResultWorklistQuery` → `SimpleResultEntryView` → `EnterResultCommand` (handler at `Features/ResultsEntry/Commands/EnterResult/EnterResultCommandHandler.cs`, permission-checked per grep evidence) → persist. Reference-range refresh available (`RefreshResultReferenceRangeCommand`). **Works (by inspection).**
6. **Result validation/approval.** `ReviewResultCommand` / `UnreviewResultCommand` / `MarkAllPatientResultsReviewedCommand`; culture results have a separate verify/unverify pair (`VerifyCultureResultCommand`, `UnverifyCultureResultCommand`). **Works (by inspection).**
7. **Report generation.** `PatientResultSheetView` → `GetPatientResultSheetQuery` / `ExportPatientReportPdfCommand` → `PatientReportPdfExporter`; combined/history/blank reports via `PrintCombinedReportCommand`, `PrintHistoryReportCommand`, `PrintBlankReportCommand`. **Works (by inspection).**
8. **Report printing & delivery.** `ResultDeliveryView` (undelivered list) → `DeliverWithSettlementCommand` — handler performs delivery + financial settlement in one handler with a single `SaveChangesAsync` (`DeliverWithSettlementCommandHandler.cs:94`, comment: "One handler, one SaveChanges"), permission `ResultDeliveryAccessPolicy.DeliverResults` enforced by pipeline; handover view `DeliveryHandoverView`. Print dispatch: `ShellPdfPrinterDispatcher` → OS shell — **unverifiable without Windows (U-04)**.

**Explicit breakpoints/gaps found:** the only broken *entry point* is the Home dashboard (F-01) — the four shortcut buttons never enter any of the chains above, although the same chains are reachable through the top navigation bar. No handler in the traced chains returns hardcoded/placeholder data; no `NotImplementedException`, `TODO`, `FIXME`, or `HACK` marker exists anywhere in `src` or `tests` (repo-wide grep, zero hits).

---

## 8. Documentation vs Code Discrepancies

1. **Claim:** `Docs/OpenCode/S-02-memory.md` records a validation gate "suite 474+195+1419=2088 green". **Code/execution evidence:** at the pinned commit in this environment the Infrastructure suite reports `Failed: 3, Passed: 192` (the three printing happy-path tests). **Observed status:** discrepancy is environment-conditional (the S-02 gate was presumably run on Windows with full fonts; cause here not isolated — U-03). **Conclusion:** documentation is not implementation evidence; the audit reports the executed result, not the documented one.
2. **Claim:** `Docs/OpenCode/S-05.md` scopes "تفعيل بوابة «بحث عن مريض»" (activating the patient-search gateway) and delivery-gateway wiring as slice work. **Code evidence:** `MainWindow.xaml:98-101` buttons still have no commands; `HomeViewModel` is empty. **Observed status:** the home-screen gateways remain inactive at the pinned commit (F-01); the navigation-bar equivalents documented in the same slice notes (e.g., `PatientsHubViewModel.OpenDeliverResultsCommand`) are wired. **Conclusion:** implementation is authoritative — Home shortcuts are dead code.
3. **Claim:** `Docs/OpenCode/S-05.md` describes a "temporary D3-style entry point" for sent-out samples. **Code evidence:** at the pinned commit, `ShellViewModel` comment states the temporary settings-dashboard routes were "absorbed in Slice 4" and ExternalEntities + SentOutSamples live inside the Accounts hub (`ShellViewModel.cs:165-168` comment block). **Observed status:** documentation superseded by later slices; code is consistent. **Conclusion:** no open discrepancy; recorded for traceability.
4. **Claim:** `ApplicationDbContext` XML remark says concrete `DbSet<T>` declarations "are added in F5". **Code evidence:** no `DbSet<T>` properties exist; access is via the generic `IApplicationDbContext.Set<TEntity>()` — the F5 plan changed. **Observed status:** stale comment only; behavior intentional and complete. **Conclusion:** minor stale documentation in a code comment; no functional impact.

---

## 9. Ordered Completion Roadmap

Verdict is NOT COMPLETE; the following dependency-aware sequence closes the two Major findings first, then the Minor items. Steps are sized so another agent can execute them without rediscovering this audit.

### Step R-01 — Wire the Home dashboard quick actions (closes F-01)
- **Objective:** Make the four Home buttons functional.
- **Modules/components:** `TopLab.Presentation` — `ViewModels/Shell/HomeViewModel.cs`, `MainWindow.xaml` (Home `DataTemplate`).
- **Prerequisites:** none (uses existing `INavigationService` and existing target ViewModels).
- **Required implementation work:** Inject `INavigationService` into `HomeViewModel`; add four commands navigating to `PatientEditorViewModel` (new patient) or `PatientsHubViewModel`, `ResultsWorklistViewModel`, `PatientSearchViewModel`, `ResultDeliveryViewModel` (with `LoadAsync` where the target requires it, mirroring the shell's existing patterns at `ShellViewModel.cs:202-265`); bind each button's `Command` in `MainWindow.xaml`.
- **Verification:** unit-test the ViewModel navigation calls (the test project already has a DI/navigation test harness per `tests/TopLab.Application.Tests/DependencyInjection/`); manual Windows click-through of all four buttons.
- **Acceptance condition:** Each button lands on the correct screen with data loaded; application test suite remains green.

### Step R-02 — Define and implement licensing (closes F-02)
- **Objective:** Give the commercial product a defined, enforced licensing story.
- **Modules/components:** new `Features/Licensing` (Application), persistence or signed-file license store (Infrastructure), startup gate in `App.xaml.cs`, activation dialog (Presentation), owner-facing documentation.
- **Prerequisites:** product-owner decision on the model (offline machine-bound key vs. activation server); confirm QuestPDF Community-license eligibility for the intended commercial distribution (see Section 6.8) in the same pass.
- **Required implementation work:** license entity/file format with signature; `ValidateLicenseQuery` executed in `App.xaml.cs` before the login stage; defined failure mode (blocking vs. read-only grace); activation UI; tamper/bypass review.
- **Verification:** unit tests for valid/invalid/expired/tampered licenses; manual first-run test on a clean Windows machine.
- **Acceptance condition:** Unlicensed installation reaches the defined failure mode; licensed installation proceeds to login; owner signs off on the model.

### Step R-03 — Persistent file logging (closes F-03)
- **Objective:** Durable operational logs on every workstation.
- **Modules/components:** `WpfAppLogger` in `TopLab.Presentation/DependencyInjection.cs`; log path under `%ProgramData%\TopLab\logs`.
- **Prerequisites:** none.
- **Required implementation work:** Replace `Debug.WriteLine` with a rolling file writer (size-capped, dated files); keep the `IAppLogger` contract unchanged; ensure backup-skip and migration-failure events are included.
- **Verification:** Release-build run; force a handled failure; confirm a log line on disk.
- **Acceptance condition:** Logs persist across restarts in Release builds.

### Step R-04 — Permission-filtered shell navigation (closes F-04)
- **Objective:** Only permitted modules are enabled in the top bar.
- **Modules/components:** `ShellViewModel.BuildNavigationItems()`; `GetCurrentSessionQuery` result (permission codes already in session DTO).
- **Prerequisites:** none (session DTO already carries `grantedCodes` per `SignInCommandHandler.cs:56`).
- **Required implementation work:** map each navigation title to the module's access-policy permission code(s); set `IsEnabled`/visibility from the session; keep `IsAbsolutePermission` seeing everything.
- **Verification:** test with a restricted user fixture; confirm forbidden modules are disabled and app-layer denials remain as the backstop.
- **Acceptance condition:** Restricted user cannot open screens they cannot act on; suite green.

### Step R-05 — SQL Server integration test project (closes F-05)
- **Objective:** Prove migrations and provider-specific behavior against real SQL Server.
- **Modules/components:** new `tests/TopLab.Integration.Tests` (Testcontainers SQL Server or LocalDB on Windows CI).
- **Prerequisites:** none for authoring; CI runner needs Docker or LocalDB.
- **Required implementation work:** fresh-database `MigrateAsync` test across all 8 migrations; per-aggregate persistence smoke tests; at least one raw-SQL maintenance path test (`BackupNowAsync` against a test instance).
- **Verification:** new project runs green in CI.
- **Acceptance condition:** A schema/model drift or a broken migration fails CI.

### Step R-06 — Reconcile `SamplePipeline` folder (closes F-06)
- **Objective:** Module inventory matches code.
- **Modules/components:** `Features/SamplePipeline/EchoName/`; docs.
- **Prerequisites:** owner confirms intended scope of the folder.
- **Required implementation work:** relocate or document; update module list.
- **Verification:** docs/code one-to-one mapping review.
- **Acceptance condition:** No feature folder misrepresents its content.

### Step R-07 — Windows/SQL-Server verification pass (closes U-01…U-06)
- **Objective:** Convert every Limited-confidence item in Section 11 into verified status.
- **Modules/components:** whole solution on a Windows machine with SQL Server.
- **Prerequisites:** R-01…R-05 complete (so the pass covers the final state).
- **Required implementation work:** `dotnet ef database update` on a fresh instance; `dotnet ef migrations has-pending-model-changes`; full app launch; click-through of the eight workflows in Section 7 including physical/PDF printing; two-workstation LAN concurrency smoke test; re-run the three failing printing tests on Windows.
- **Verification:** this section's checklist executed with recorded outputs.
- **Acceptance condition:** All Section 11 items resolved as verified or converted into tracked defects.

---

## 10. Definition of Done

The project may be considered complete when, in order:

1. **Pinned-commit integrity** — audit base remains `7a5a814a5ea8a0c143fad1df0f195059db5088a4` or a descendant explicitly re-audited. *Verification: `git rev-parse HEAD`.*
2. **Clean build** — `dotnet build TopLab.sln` succeeds with 0 errors (0 warnings as at this audit). *Verification: build output.*
3. **Full test suite green** — Domain 474, Application 1419, Infrastructure 195 with zero failures on the target Windows environment (the 3 printing failures reproduced here must be shown to be environment-only or fixed). *Verification: `dotnet test` on Windows.*
4. **Home dashboard functional** — F-01 acceptance condition met. *Verification: R-01.*
5. **Licensing decided and enforced** — F-02 acceptance condition met (or a documented owner decision removing licensing from product scope, downgrading F-02). *Verification: R-02.*
6. **Fresh database provisionable** — all 8 migrations apply to an empty SQL Server database; `has-pending-model-changes` reports none. *Verification: R-07.*
7. **Eight core workflows execute on Windows** — the Section 7 chains complete end-to-end on a real workstation, including at least one printed/PDF receipt, invoice, worksheet, and patient report. *Verification: R-07 checklist.*
8. **Authorization double-layer confirmed** — app-layer denials (already tested) plus permission-filtered shell (R-04). *Verification: tests + restricted-user walkthrough.*
9. **LAN operation demonstrated** — two workstations against one SQL Server complete concurrent registration/result-entry without errors. *Verification: R-07.*
10. **Operational logging persists** — R-03 acceptance condition. *Verification: Release-build log file.*
11. **Docs/code inventory aligned** — R-06 acceptance condition. *Verification: module inventory diff.*

---

## 11. Limitations

**Environment of the audit:** Linux x86_64, .NET SDK 8.0.425 (installed for the audit), no Windows desktop, no WPF runtime, no SQL Server, no `dotnet ef` tool. Everything reportable by reading source was inspected; the items below could not be *executed* and, per the binding instruction, are **not** classified as defects.

### Unverified Items

- **U-01 — Fresh-database migration execution.** The 8-migration chain and `db.Database.MigrateAsync()` startup call were inspected but not run against a real SQL Server. *Needed:* a SQL Server instance (or container) and `dotnet ef database update` / app first-run.
- **U-02 — Model-vs-snapshot drift.** `dotnet ef migrations has-pending-model-changes` was not run (tool not installed; needs restore context). *Needed:* `dotnet ef` on the pinned tree.
- **U-03 — Three failing printing tests.** `InvoicePrintingServiceTests.PrintInvoiceAsync_HappyPath_*`, `WorkSheetPrintingServiceTests.PrintWorkSheetAsync_HappyPath_*`, `ReceiptPrintingServiceTests.PrintReceiptAsync_HappyPath_*` fail with a bare `Assert.True() Failure` (Expected: True / Actual: False) in this Linux environment. The writers set `QuestPDF Settings.UseSystemFonts = true` and render Arabic text, so a font/shaping dependency is a plausible cause, but the assertion's exact condition was not isolated and the host did have 18 Arabic fonts registered (`fc-list :lang=ar`). *Cause: unconfirmed.* *Needed:* re-run on Windows (the target platform) and, if still failing, inspect the asserted condition at the cited test lines (`ReceiptPrintingServiceTests.cs:102`, `WorkSheetPrintingServiceTests.cs:95`, `InvoicePrintingServiceTests.cs:88`).
- **U-04 — Physical print dispatch.** `ShellPdfPrinterDispatcher` delegates to the OS shell; verifiable only on Windows with printers. *Needed:* Windows workstation with a printer/PDF printer.
- **U-05 — Interactive UI verification.** View wiring was verified statically (DataTemplates for every ViewModel exist in `MainWindow.xaml`; all ViewModels and windows are registered in `AddPresentation()`); actual rendering, binding errors at runtime, and RTL layout were not exercised. *Needed:* run the app on Windows.
- **U-06 — Multi-workstation concurrency.** LAN deployment is configuration-based (per-workstation connection string to a shared server); concurrent-access behavior (deadlocks, concurrency tokens — no `RowVersion` usage was observed in inspected configurations, though not every configuration file was opened) was not tested. *Needed:* two workstations + shared SQL Server.
- **U-07 — Exhaustive per-handler review.** 219 handlers exist; the audit fully read representative handlers across every traced workflow and grepped all handlers for incompleteness markers (zero hits), permission declarations, and placeholder patterns, but did not open all 219 files line-by-line. Residual risk of localized gaps in unopened handlers exists and is not claimed otherwise.

**Other limitations:** The Infrastructure test suite uses the EF Core InMemory provider (F-05), so even the green tests do not prove SQL Server behavior. Documentation was used for scope discovery only. The audit was read-only: `git status --short` was empty before and after; no repository file was created, modified, or deleted (build artifacts under `obj/`/`bin/` are gitignored; the report file lives outside the repository).

---

## 12. Appendix

### A. Pinned-commit gate output
```
$ git checkout 7a5a814a5ea8a0c143fad1df0f195059db5088a4
HEAD is now at 7a5a814 [S-06] Slice 6/7: Utilities screen + «الأدوات» wiring — loop-engineering
$ git rev-parse HEAD
7a5a814a5ea8a0c143fad1df0f195059db5088a4
$ git status --short        # (empty)
$ git status
HEAD detached at 7a5a814
nothing to commit, working tree clean
$ git log -1 --format='%H %ci %s'
7a5a814a5ea8a0c143fad1df0f195059db5088a4 2026-09-19 00:56:46 +0300 [S-06] Slice 6/7: Utilities screen + «الأدوات» wiring — loop-engineering
```

### B. Build & test results (actual)
```
$ dotnet restore TopLab.sln
error NETSDK1100: To build a project targeting Windows on this operating system,
set the EnableWindowsTargeting property to true. [src/TopLab.Presentation/TopLab.Presentation.csproj]

$ dotnet restore TopLab.sln /p:EnableWindowsTargeting=true     # exit 0 (7 projects restored)
$ dotnet build TopLab.sln --no-restore /p:EnableWindowsTargeting=true
Build succeeded.    0 Warning(s)    0 Error(s)

$ dotnet test tests/TopLab.Domain.Tests --no-build
Passed!  - Failed: 0, Passed: 474,  Skipped: 0, Total: 474
$ dotnet test tests/TopLab.Application.Tests --no-build
Passed!  - Failed: 0, Passed: 1419, Skipped: 0, Total: 1419
$ dotnet test tests/TopLab.Infrastructure.Tests --no-build
Failed!  - Failed: 3, Passed: 192,  Skipped: 0, Total: 195
  [FAIL] InvoicePrintingServiceTests.PrintInvoiceAsync_HappyPath_WritesPdfAndDispatchesToReceiptPrinter  (line 88)
  [FAIL] WorkSheetPrintingServiceTests.PrintWorkSheetAsync_HappyPath_WritesPdfAndDispatchesToReportsPrinter (line 95)
  [FAIL] ReceiptPrintingServiceTests.PrintReceiptAsync_HappyPath_WritesPdfAndDispatchesToReceiptPrinter  (line 102)
  Error Message (all three): Assert.True() Failure — Expected: True, Actual: False
```

### C. Repository scale and composition (counted at the pinned commit)
- `.cs` files: 1,361 | `.xaml` files: 70 | commits in history: 179
- Application handlers: 219 files | FluentValidation validators: 154 files
- Test files: 296 | `[Fact]`/`[Theory]` attributes: 1,785
- Feature folders (`src/TopLab.Application/Features/`): AccessAndNavigation, AnalyteProfiles, Attendance, AuditAndTraceability, CultureAndAntibiotics, CultureResults, ExternalEntities, InventoryAndAccounting, PatientBilling, PatientRegistration, PatientSearch, PriceListsCommentsAndCustomGroups, ProfileResults, ReportProduction, ResultDelivery, ResultsEntry, SampleCollection, SamplePipeline, SentOutSamples, Statistics, SystemAndPrintSettings, TestCatalogAndReferenceRanges, UsersAndPermissions, Utilities, WorkSheets
- Migrations: `20260828052248_BaselineDataModel` (36 `CreateTable` calls), `20260828123530_RenamePkColumns`, `20260906093902_AddTestCodeAndLifecycleColumns`, `20260907162756_AddPatientIsDeletedAndPatientTestSampleDrawnIndex`, `20260908175555_AddPatientTestReferenceRangeSnapshots`, `20260909033414_AddAnalyteProfileDomain`, `20260910213833_AddPregnancyMedicalConditionTypeSeed`, `20260916113704_AddInvoiceIssues`, + `ApplicationDbContextModelSnapshot.cs` (124 entity entries)
- Incompleteness-marker grep (`NotImplementedException|TODO|FIXME|HACK` across `src` and `tests`): **0 hits**
- Licensing grep (`Licen[cs]e|Activation|Trial` across `src/**/*.cs`): only QuestPDF `LicenseType.Community` comments in `WorkSheetPdfWriter.cs`, `ReceiptPdfWriter.cs`, `InvoicePdfWriter.cs`

### D. Key files inspected in full or in material part
`App.xaml` / `App.xaml.cs`; `MainWindow.xaml`; `ViewModels/Shell/ShellViewModel.cs`; `ViewModels/Shell/HomeViewModel.cs`; `DependencyInjection.cs` (Application, Infrastructure, Presentation — all three in full); `Common/Behaviors/AuthorizationBehavior.cs`; `SignInCommandHandler.cs`; `CreatePatientCommandHandler.cs`; `DeliverWithSettlementCommandHandler.cs`; `UpdateDatabaseServerSettingsCommandHandler.cs`; `GetDatabaseServerSettingsQueryHandler.cs`; `ApplicationDbContext.cs`; `AuditableEntitySaveChangesInterceptor.cs`; `SqlServerConnectionDescriptor.cs`; `SqlServerDatabaseMaintenanceService.cs`; `DailyBackupHostedService.cs`; `WorkSheetPdfWriter.cs`; `ReceiptPdfWriter.cs`; `InvoicePdfWriter.cs`; `appsettings.example.json`; both `Directory.*.props`; all four `.csproj` heads; `tests/.../ReceiptPrintingServiceTests.cs` (incl. failing test body); feature/command directory listings for all 25 feature folders; Views/ViewModels full listings; `Docs/OpenCode/S-01.md`, `S-02-memory.md`, `S-05.md` (grep-extracted claims only, used per source-of-truth rules).

### E. Environment notes for reproduction
- Host: `Linux sbx-002ad584 6.18.15 x86_64`; `dotnet` initially absent → installed .NET SDK 8.0.425 via `dotnet-install.sh --channel 8.0`.
- All `dotnet` commands require `/p:EnableWindowsTargeting=true` on non-Windows because `TopLab.Presentation.csproj` sets `<TargetFramework>net8.0-windows</TargetFramework>`, `<UseWPF>true</UseWPF>`, `<RuntimeIdentifier>win-x64</RuntimeIdentifier>`.
- Working tree verified clean (`git status --short` empty) after all audit commands.

*End of report.*
