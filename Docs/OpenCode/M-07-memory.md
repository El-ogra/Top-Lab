# Loop Engineering — Memory File

- **Module:** Combined, Blank & History Reports (M-07)
- **Module Number:** M-07
- **Source Plan:** Docs/OpenCode/M-07.md
- **Date Created:** 2026-09-15
- **Total Slices:** 5
- **Current Slice:** 5 — pending
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Delivers the four report types — combined (reviewed-only, user-ordered, deduplicated), blank (patient data only, no balance gate), patient history (ByLabCode / exact-normalized-name identity per `HistorySortMode`, auto-insert gated on `HistoryAutoDisplayEnabled`, manual insert unconditional), and separate/multi-patient history — across Domain, Application, and Infrastructure. Ships the first real `IReportPrintingService` implementation (PDF-first, settings read at print time, `PrinterAssignment` OutputType = Reports routing), `PRINT_RESULTS`-gated print commands with the BR-07 balance gate on combined + history only, and `MarkPrinted` audit per printed line. Zero migration (ephemeral selection); zero Presentation content. Done means: S1 Domain rules plus tests, S2 core Application surface plus tests, S3 printing implementation plus print commands plus tests, S4 history insertion plus tests, S5 zero-drift proof plus ADR-0040 plus tracking flip plus handoff plus full-suite green plus coverage floors.

## Global Validation Gates

- Gate G0 (pre-execution): `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- Gate G1 (post-execution per slice): same as G0 plus the slice-specific gate listed in the table below.

## Stop/Continue Rule

After a slice completes (all 10 stages done), verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice, with no pause and no human confirmation required.
If any one fails → retry. If the SAME failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs 5 CONSECUTIVE times, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do NOT proceed past this point without owner review.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: 5 consecutive failures for the same reason.
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4 -> S5, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/model-assertion) replaces any standard UI journey — M-07 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-07] Slice N/5: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-07's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain: `src/TopLab.Domain` builds zero/zero; new `CombinedReportSelectionTests` + `PatientHistoryResolverTests` plus all Domain tests green; every guard negative-pathed (non-reviewed, duplicate, both resolver modes, LabId-less, whitespace name); Domain `Reports` coverage ≥ 90% | `dotnet build src/TopLab.Domain`; `dotnet test tests/TopLab.Domain.Tests`; coverlet module filter |
| 2 | VG-02 | Application core surface: `src/TopLab.Application` builds zero/zero; all S2 handler/validator tests green; grep gate zero `IAuthorizedRequest` under `Features/ReportProduction/Queries`; grep gate zero live `ReferenceRange` reads in `Features/ReportProduction`; Application S2 footprint coverage ≥ 80% | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gates |
| 3 | VG-03 | Infrastructure printing + print commands: solution builds zero/zero; `ReportPrintingServiceTests` + print-command handler tests + authorization theory tests green; BR-07 matrix pinned (combined/history blocked, blank exempt, absolute bypass); `MarkPrinted` audit asserted; `IReportPrintingService` resolves from composed provider; Infra S3 ≥ 70% / App S3 ≥ 80% | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Infrastructure.Tests` + `tests/TopLab.Application.Tests`; DI resolution test |
| 4 | VG-04 | History insertion: Application builds zero/zero; S4 tests green incl. the switch-false zero-insertion pin, manual-insert-with-switch-off, wrong-identity rejection, and no-mutation pin on stored `PatientTest` rows; Application S4 footprint coverage ≥ 80% | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests` |
| 5 | VG-05 | Close-out: Release build zero/zero; full suite green (`-m:1`); `dotnet ef migrations has-pending-model-changes` → no changes; snapshot unchanged; coverage floors met or waived; audit gate passed; ADR-0040 appended; M07 tracking row flipped; `Handoff_M07.md` per template; zero Presentation content (grep gate) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain rules: combined-report selection + patient-history resolver | [x] Done (VG-01 PASS) | VG-01 |
| 2 | Application core surface: combinable list + combined/blank builders + history queries | [x] Done (VG-02 PASS) | VG-02 |
| 3 | Infrastructure: PDF-first `IReportPrintingService` + print commands | [x] Done (VG-03 PASS) | VG-03 |
| 4 | Application: automatic & manual history insertion + separate history report assembly | [x] Done (VG-04 PASS) | VG-04 |
| 5 | Hardening, documentation, module close-out | [ ] Pending | VG-05 |

---

## Slice 1: Domain rules: combined-report selection + patient-history resolver

- **Goal:** Add the ephemeral `CombinedReportSelection` (reviewed-only, dedup, contiguous ordering) and the static `PatientHistoryResolver` (ByLabCode / exact-normalized-name identity), fully test-covered, additively (no existing entity touched).
- **Touches:** `src/TopLab.Domain/Reports/CombinedReportSelection.cs` (create, new folder); `src/TopLab.Domain/Reports/PatientHistoryResolver.cs` (create); `tests/TopLab.Domain.Tests/Reports/CombinedReportSelectionTests.cs` (create); `tests/TopLab.Domain.Tests/Reports/PatientHistoryResolverTests.cs` (create)
- **Validation Gate:** VG-01 — Domain build zero/zero; all Domain tests green; every guard negative-pathed; coverage ≥ 90% on the new scope.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings. `dotnet test TopLab.sln` → Domain 360 passed, Application 976 passed, Infrastructure 126 passed (1462 total, 0 failed).
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1 + settled rules 1/4 + Appendix A identity messages re-read. Domain guards raise `ArgumentException` with English text + `ParamName` (verified repo precedent: `CreatePatientCommandHandler.cs:206-217` translators match `ex.ParamName`); Arabic lives in the S2 translator.
- [x] **Stage 3 — File Analysis:** Inspected `PatientTest.cs` (guard style: `InvalidOperationException`/English; `ArgumentException(name, msg)` precedent in `ReportSettings.cs:58`), `ReportSettings.cs` (`HistorySortMode`/`HistoryAutoDisplayEnabled`, `SetTopSpace` `ArgumentException` with `ParamName`), `HistorySortMode.cs` (ByLabCode=0/ByPatientName=1), xUnit conventions (`Using Include="Xunit"`, `Assert.Throws<T>`), ID types (`PatientTestId.Create(int)`), no `Domain/Reports/` folder exists (glob empty). Translator precedent is `ex.ParamName` switch (M-13/M-14/M-15 style).
- [x] **Stage 4 — Planning:** (1) `CombinedReportSelection` — backing `List<int>`, `OrderedIds` read-only, `Count`; `Add(id, isReviewed)`: non-reviewed → `ArgumentException(nameof(isReviewed))`, duplicate → `ArgumentException(nameof(patientTestId))`; `MoveUp`/`MoveDown`: unknown id → `ArgumentException(nameof(patientTestId))`, boundary no-op, else adjacent swap (order stays contiguous 1..n). (2) `PatientHistoryResolver` static — `ResolveKey(mode, labId, fullName)`: ByLabCode → whitespace labId throws `ArgumentException(nameof(labId))`, returns trimmed key; ByPatientName → whitespace name throws `ArgumentException(nameof(fullName))`, returns `fullName.Trim().ToUpperInvariant()`; unknown mode → `ArgumentOutOfRangeException(nameof(mode))`. (3) Two test classes covering every negative path + both modes + normalization + contiguous moves + empty-selection.
- [x] **Stage 5 — Execution:** Implemented `CombinedReportSelection.cs` + `PatientHistoryResolver.cs` + the two test classes. Two test-iteration fixes: resolver now collapses interior whitespace (EC-08 "extra-spaced"); `RepeatedMoves` expected sequence corrected to [1,2,4,3].
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build src/TopLab.Domain` → 0 errors / 0 warnings (Build succeeded). `dotnet test tests/TopLab.Domain.Tests` → 378 passed, 0 failed (360 pre-existing + 18 new).
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — every guard negative-pathed (`Add_NonReviewed_Throws`, `Add_Duplicate_Throws`, `ByLabCode_Null/EmptyLabId_Throws`, `ByPatientName_WhitespaceName_Throws`, `Move_UnknownId_Throws`); both modes covered; coverlet (cobertura) `Reports` scope: CombinedReportSelection 32/32 = 100.0%, PatientHistoryResolver 21/21 = 100.0% ≥ 90%. No analyzer suppressions; no new packages.
- [x] **Stage 8 — Documentation Update:** This checklist marked; evidence recorded above.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated below (Slice 1 done).
- [x] **Stage 10 — Git Commit (authorized local):** `[M-07] Slice 1/5: Domain rules: combined-report selection + patient-history resolver — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.` — committed on `main` as `9e4c84e`; never pushed.

---

## Slice 2: Application core surface: combinable list + combined/blank builders + history queries

- **Goal:** Expose `GetCombinableTests` (reviewed-only, kind-mapped), `BuildCombinedReport` / `BuildBlankReport` (persistence-free DTO builders validated through the S1 Domain object), and `GetPatientTestHistory` / `GetMultiPatientHistory` honoring `ReportSettings`, with handler tests.
- **Touches:** `src/TopLab.Application/Features/ReportProduction/Common/ReportDtos.cs` (create); `.../Common/ReportProductionAccessPolicy.cs` (create); `.../Common/DomainFailureTranslator.cs` (create); `.../Queries/GetCombinableTests/` (3 files, create); `.../Commands/BuildCombinedReport/` (3 files, create); `.../Commands/BuildBlankReport/` (3 files, create); `.../Queries/GetPatientTestHistory/` (3 files, create); `.../Queries/GetMultiPatientHistory/` (3 files, create); `tests/TopLab.Application.Tests/Features/ReportProduction/` (5 handler-test classes, create); `FakeApplicationDbContext` (verify-only / extend on proven gap)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green; grep gates (no `IAuthorizedRequest` in Queries; no live `ReferenceRange` reads in the feature); coverage ≥ 80%.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings. `dotnet test TopLab.sln` → Domain 378 / Application 976 / Infrastructure 126 (1480 total, 0 failed).
- [x] **Stage 2 — Deep Understanding:** Plan §6 S2 + FR-M07-001/002/004/008 + EC-07/08/11/14/15/16 re-read. Snapshot-only discipline; open reads (no `IAuthorizedRequest` on queries); persistence-free build commands (no `SaveChanges`); `ReportSettings` read directly (Id == 1); missing row → `Error.Unexpected("سجل إعدادات التقرير مفقود.")`; missing patient → `Error.NotFound("المريض غير موجود.")`; unknown test → `Error.NotFound("التحليل غير موجود")`; identity-unresolvable → Conflict with `تعذر تحديد هوية المريض للتاريخ المرضي.`.
- [x] **Stage 3 — File Analysis:** Inspected `GetResultWorklistQueryHandler` (reviewed filter + `ToDictionary` catalog join), `GetVisitHistoryQueryHandler` (LabId rollup + settings-missing `Unexpected`), `GetProfileReportQueryHandler` (snapshot-only reads), `GetCultureReportQueryHandler`, `Error`/`Result`/`ErrorType`, `ResultKind` enum (Simple=0/Profile=1/Culture=2), `Test`/`Patient`/`ExternalEntity`/`Analyte`/`ProfileResultItem`/`PatientTestReferenceRangeSnapshot`/`ProfileResultItemReferenceRangeSnapshot`/`CultureResult` shapes, `FakeApplicationDbContext` (all needed sets present — verify-only, no gap), validator conventions (`NotEmpty().WithMessage` precedents: `قائمة التحاليل مطلوبة.` / `قائمة المرضى مطلوبة.`), DI assembly-wide validator scan. Translator precedent confirmed = `ex.ParamName` switch.
- [x] **Stage 4 — Planning:** (1) `Common/ReportDtos.cs` (CombinableTestDto, FrozenProfileRangeDto, ProfileReportLineDto, CultureReportSummaryDto, CombinedReportLineDto, CombinedReportDto, BlankReportDto, HistoryEntryDto, PatientHistoryDto, MultiPatientHistoryDto) → (2) `Common/ReportProductionAccessPolicy.cs` (`PrintResults = "PRINT_RESULTS"`) → (3) `Common/DomainFailureTranslator.cs` (param switch: `isReviewed`/`patientTestId`/`labId`/`fullName`) → (4) `Common/PatientHistoryReader.cs` (shared resolver: `ResolveVisitPatients` per mode + `BuildEntries`) → (5) `GetCombinableTestsQuery` (+Handler, +Validator) → (6) `BuildCombinedReportCommand` (+Handler via `CombinedReportSelection` in try/catch, +Validator non-empty) → (7) `BuildBlankReportCommand` (+Handler flat entity join, +Validator) → (8) `GetPatientTestHistoryQuery` (+Handler, +Validator) → (9) `GetMultiPatientHistoryQuery` (+Handler, +Validator) → (10) five handler-test classes.
- [x] **Stage 5 — Execution:** Implemented the full plan: DTOs, access policy, `DomainFailureTranslator`, shared `PatientHistoryReader` (resolver + entries + identity ordering), 3 open queries + 2 persistence-free build commands (each with handler + validator), and 5 handler-test classes (30 tests). One compile fix: strongly-typed id `ExternalEntityId` exposes `.Value` directly, so the nullable unwrap `.Value.Value` was dropped in the blank-report handler.
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings. `dotnet test TopLab.sln` → Domain 378 / Application 1006 (+30 new) / Infrastructure 126 (1510 total, 0 failed).
- [x] **Stage 7 — Validation Gate:** VG-02 PASS — build zero/zero; all S2 tests green; grep gates clean (zero `IAuthorizedRequest` under `Features/ReportProduction/Queries`, zero live `ReferenceRange`/`SaveChanges` matches in `Features/ReportProduction`); coverlet S2 footprint 383/441 = 86.8% ≥ 80% (handlers 100%, reader 96.7%, translator 88.9%; validators unexecuted because handlers are driven directly — behavior pipeline out of scope here).
- [x] **Stage 8 — Documentation Update:** This checklist marked; evidence recorded above.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated below (Slice 2 done).
- [x] **Stage 10 — Git Commit (authorized local):** `[M-07] Slice 2/5: Application core surface: combinable list + combined/blank builders + history queries — loop-engineering` + `Stages 1-10 verified. Gate VG-02 passed.` — committed on `main` as `d6623e1`; never pushed.

---

## Slice 3: Infrastructure: PDF-first `IReportPrintingService` + print commands

- **Goal:** Ship the `ReportPrintingService` implementation (PDF-first, settings at print time, printer routing via `PrinterAssignment` OutputType = Reports) with DI registration, plus the three `PRINT_RESULTS`-gated print commands with the settled BR-07 matrix and `MarkPrinted` audit.
- **Touches:** `src/TopLab.Infrastructure/Printing/ReportPrintingService.cs` (create, new folder); `src/TopLab.Infrastructure/DependencyInjection.cs` (modify: registration); `.../Commands/PrintCombinedReport/` (3 files, create); `.../Commands/PrintBlankReport/` (3 files, create); `.../Commands/PrintHistoryReport/` (3 files, create); `tests/TopLab.Infrastructure.Tests/Printing/ReportPrintingServiceTests.cs` (create); `tests/.../Features/ReportProduction/Print*CommandHandlerTests.cs` (3 classes, create); `.../ReportProductionAuthorizationTests.cs` (create); `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend)
- **Validation Gate:** VG-03 — solution build zero/zero; all new tests green incl. BR-07 matrix + authorization theory; DI resolution test; coverage floors.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings. `dotnet test TopLab.sln` → Domain 378 / Application 1006 / Infrastructure 126 (1510 total, 0 failed).
- [x] **Stage 2 — Deep Understanding:** Plan §6 S3 (lines 195–234) + memory S3 header re-read. FR-M07-001/002/007 print paths; EC-04/05/06/12/13; BR-07 verbatim gate formula + message (confirmed on `MarkCultureReportPrintedCommandHandler`); settings-at-print-time rule (Blueprint §6/§3.2); any PDF/print I/O failure → `Error.Unexpected` before crossing into Application (Test Strategy §3); OD-07-A (PDF-first) and OD-07-E (gate scope) settled. Two design settlements: (a) print handlers reuse the S2 builders/query via MediatR `ISender` — precedent `CreatePatientCommandHandler` + `FakeSender.WithResponse<T>` — so S2 files stay untouched; (b) the port's single string `reportToken` carries a JSON `ReportPrintEnvelope(kind, ReportJson)` where `ReportJson` is the serialized report DTO (R-1 mitigation "port consumed only through DTOs"); Infrastructure deserializes by kind. In-box `System.Text.Json` (net8.0), zero new packages.
- [x] **Stage 3 — File Analysis:** (a) Application ports/precedents: `IReportPrintingService.PrintReportAsync(string)` (grep: only the interface file, no other consumers); `ICurrentUserService` shape; `IApplicationDbContext`; `MarkResultPrintedCommand(+Validator,+Handler)` = `IAuthorizedRequest{ResultsEntryAccessPolicy.PrintResults}` + BR-07 verbatim (`BalanceProbe.Balance(_db, patient.Id.Value)`) + `MarkPrinted` wrapped in try/catch `InvalidOperationException` → `Error.Conflict(translator)` + `SaveChangesAsync`; `AuthorizationBehavior` real path `Common/Behaviors/` (absolute bypass, shared Arabic Forbidden message); `ExportPatientReportPdfCommandHandler` path rules; `ReportProductionAccessPolicy.PrintResults` const; `DomainFailureTranslator` (ReportProduction) handles `ArgumentException` only. (b) Domain: `PatientTest.MarkPrinted(int, DateTime)` throws `InvalidOperationException("Result not reviewed.")` when unreviewed → additive `Translate(InvalidOperationException)` overload needed; `ReportSettings` (CreateDefault, SetTopSpace ≤ 8, SetPaperSize, SetHeaderFooterMode, SetDoctorSignature, SetHistoryOptions); `SystemSettings` (CreateDefault; `PrintLabIdInsteadOfPatientId`/`PrintFileExternalBarcode`/`PrintAccountInsteadOfDateOnReport`); `PrinterAssignment` PK `OutputType` + `PrinterOutputType.Reports = 0`. (c) Infra: `PatientReportPdfExporter` (BuildMinimalPdf ASCII `%PDF-1.4`, `FileMode.CreateNew` never-overwrite, `%PDF` magic asserts); `DependencyInjection.cs` line 61 exporter registration (pattern); `InfrastructureRegistrationTests.BuildProvider()` (in-memory connection string); `InMemoryContextFactory` (+Audit interceptor); `TestApplicationDbContext`; `WorkSheetQueryPersistenceTests` real-`ApplicationDbContext` + `EnsureCreated` seed pattern. (d) App tests: `ResultsEntryAuthorizationTests` (PrintGate theory + WithoutPermission → Forbidden Arabic); `ValidatorRegistrationTests` (Type-resolution through `AddApplication`); fakes `FakeCurrentUserService`, `FakeDateTimeProvider`, `FakeSender.WithResponse<T>(request, response)`; `MarkProfilePrintedCommandHandlerTests` BR-07 seeding (User.Create(id,"cashier","h","h2",isAbsolute,0,blockPrint=true) + PaymentOperation positive balance). (e) S2 DTO shapes (CombinedReportDto/BlankReportDto/PatientHistoryDto with `Entries[].IsReviewed`).
- [x] **Stage 4 — Planning:**
  1. `Common/ReportPrintEnvelope.cs` (create): `sealed record ReportPrintEnvelope(string ReportKind, string ReportJson)` + consts `Combined/Blank/History` + `static string CreateToken<T>(string kind, T report) => JsonSerializer.Serialize(new ReportPrintEnvelope(kind, JsonSerializer.Serialize(report)))`.
  2. `Common/DomainFailureTranslator.cs` (modify — additive): add `internal static string Translate(InvalidOperationException ex)` mapping `"Result not reviewed."` → `"لا يمكن طباعة نتيجة غير معتمدة."`, default `"بيانات غير صالحة."`.
  3. `Commands/PrintCombinedReport/` (3 files): command `(int PatientId, IReadOnlyList<int> OrderedPatientTestIds) : IRequest<Result>, IAuthorizedRequest { PrintResults }`. Validator mirrors BuildCombinedReport (`PatientId>0`; `OrderedPatientTestIds.NotEmpty().WithMessage("قائمة التحاليل مطلوبة.")`). Handler `(db, currentUser, clock, ISender sender, IReportPrintingService printing)`: patient NotFound `المريض غير موجود.` → user NotFound `المستخدم غير موجود.` → `sender.Send(BuildCombinedReportCommand)` failure passthrough → BR-07 verbatim (`Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.")`) → `token = ReportPrintEnvelope.CreateToken(Combined, dto)` → `printing.PrintReportAsync(token)` failure passthrough → load `PatientTest` rows by `Line.PatientTestId` → `MarkPrinted(currentUser.UserId, clock.UtcNow)` each in try/catch InvalidOperationException → `Conflict(translator)` → `SaveChangesAsync` → Success.
  4. `Commands/PrintBlankReport/` (3 files): command `(int PatientId)` + validator `PatientId>0`; handler: patient/user NotFound → `sender.Send(BuildBlankReportCommand)` → token(Blank) → print. **No balance gate, no MarkPrinted, no SaveChanges.**
  5. `Commands/PrintHistoryReport/` (3 files): command `(int PatientId)` + validator `PatientId>0`; handler: patient/user NotFound → `dto = sender.Send(GetPatientTestHistoryQuery)` (failure passthrough) → `!dto.Entries.Any(e => e.IsReviewed)` → `Conflict("لا يمكن طباعة نتيجة غير معتمدة.")` → BR-07 gate → token(History) → print → MarkPrinted per reviewed entry id → SaveChanges.
  6. `src/TopLab.Infrastructure/Printing/` (new folder): `IPdfPrinterDispatcher.cs` (`Task DispatchAsync(string pdfFilePath, string printerName, CancellationToken)`); `ShellPdfPrinterDispatcher.cs` (`Process.Start` `Verb="print"` + `UseShellExecute=true` on the temp PDF; OS print association — per-printer spooler binding is Presentation per Blueprint §3.3; failures bubble → service maps Unexpected); `IReportPdfWriter.cs` (`Task WritePdfAsync(string absolutePath, ReportPrintEnvelope envelope, ReportSettings reportSettings, SystemSettings systemSettings, CancellationToken)`); `ReportPdfWriter.cs` (deserialize inner DTO by kind — Combined/Blank/History, unknown → throw; render minimal valid PDF mirroring `BuildMinimalPdf` with `FileMode.CreateNew` never-overwrite; lines: `TopLab {Kind} Report`, identifier `LabId:` vs `PatientId:` per `PrintLabIdInsteadOfPatientId`, `Name:`, `Paper: {PaperSize}`, `TopSpace: {ReportTopSpaceCm}`, header/footer mode, doctor signature, history sort/auto flags, then per line `Code Name: value` + flag/range/profile/culture); `ReportPrintingService.cs` (`(db, writer, dispatcher)`: deserialize envelope → null/bad JSON → Unexpected `بيانات التقرير غير صالحة.`; `ReportSettings` Id==1 missing → Unexpected `سجل إعدادات التقرير مفقود.`; `SystemSettings` Id==1 missing → Unexpected `سجل إعدادات النظام مفقود.`; `PrinterAssignment` `OutputType==Reports` missing → Unexpected `لم يتم تعيين طابعة لتقارير المختبر.`; temp `Path.Combine(Path.GetTempPath(), $"TopLabPrint-{Guid.NewGuid():N}.pdf")`; write; `DispatchAsync(path, printerName)`; file left in OS temp; Success — `catch OperationCanceledException → rethrow; catch Exception → Unexpected("تعذر طباعة التقرير.")`).
  7. `src/TopLab.Infrastructure/DependencyInjection.cs` (modify): after exporter line add Scoped `IReportPrintingService`/`IReportPdfWriter`/`IPdfPrinterDispatcher` + `using TopLab.Infrastructure.Printing;`.
  8. `tests/TopLab.Infrastructure.Tests/Printing/ReportPdfWriterTests.cs` (create): `%PDF-1.4` prefix; `WritePdfAsync_OnExistingTarget_Throws` (CreateNew never-overwrite); identifier line flips with `PrintLabIdInsteadOfPatientId`; `Paper:`/`TopSpace:` reflect ReportSettings; kind titles Combined/Blank/History.
  9. `tests/TopLab.Infrastructure.Tests/Printing/ReportPrintingServiceTests.cs` (create): real `ApplicationDbContext` + `InMemoryContextFactory().Create()` + `EnsureCreated()` seeded (WorkSheetQueryPersistenceTests pattern) with `ReportSettings.CreateDefault()`, `SystemSettings.CreateDefault()`, `PrinterAssignment(Reports,"LAB-PRINTER")`; recording-dispatcher fake (path+printer capture, optional throw). Cases: happy → Success + temp pdf (`%PDF`) + dispatcher got `LAB-PRINTER`; missing assignment/ReportSettings/SystemSettings → Unexpected; invalid token → Unexpected; dispatcher throws → Unexpected (never throws); settings-at-call-time → two calls with toggle → two distinct pdfs (`PatientId:` then `LabId:`).
  10. `tests/TopLab.Infrastructure.Tests/DependencyInjection/InfrastructureRegistrationTests.cs` (extend): resolve `IReportPrintingService`/`IReportPdfWriter`/`IPdfPrinterDispatcher` from `BuildProvider()` (DI resolution gate).
  11. `tests/TopLab.Application.Tests/Common/Fakes/FakeReportPrintingService.cs` (create): records tokens; `NextResult` override; default `Result.Success()`.
  12. `tests/TopLab.Application.Tests/Features/ReportProduction/PrintCombinedReportCommandHandlerTests.cs` (create): happy (Combined envelope + inner JSON; MarkPrinted fields on PatientTest rows; `SaveChangesCallCount == 1`); BR-07 blocked (block + positive balance + non-absolute → Conflict Arabic, printing untouched, PrintCount unchanged); absolute bypass → Success; no-block → Success; assembler Failure passthrough; patient/user NotFound.
  13. `.../PrintBlankReportCommandHandlerTests.cs` (create): happy (Blank envelope token; no PatientTest mutation; `SaveChangesCallCount == 0`); positive balance still Success (exempt); patient/user NotFound; assembler failure.
  14. `.../PrintHistoryReportCommandHandlerTests.cs` (create): happy (History envelope; MarkPrinted only reviewed entries); no-reviewed-entries → Conflict `لا يمكن طباعة نتيجة غير معتمدة.`; BR-07 blocked; absolute bypass; query failure passthrough; patient/user NotFound.
  15. `.../ReportProductionAuthorizationTests.cs` (create): Theory over the 3 print command types → `RequiredPermissionCode == "PRINT_RESULTS"`; `AuthorizationBehavior<PrintCombinedReportCommand, Result>` without permission → Forbidden (shared Arabic message); with `HasPermission("PRINT_RESULTS")` → handler runs.
  16. `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend): theory resolving `IValidator<PrintCombinedReportCommand>`, `IValidator<PrintBlankReportCommand>`, `IValidator<PrintHistoryReportCommand>` through `AddApplication`.
- [x] **Stage 5 — Execution:** Implemented the full plan. Application: `ReportPrintEnvelope` (record `(string ReportKind, string ReportJson)` + consts Combined/Blank/History + generic `CreateToken<T>`); `DomainFailureTranslator` additive `Translate(InvalidOperationException)` overload (`"Result not reviewed."` → `"لا يمكن طباعة نتيجة غير معتمدة."`, default `"بيانات غير صالحة."`); 3 print commands each with command/validator/handler (`PrintCombinedReport` uses `ISender` → `BuildCombinedReportCommand`; `PrintBlankReport` → `BuildBlankReportCommand`; `PrintHistoryReport` → `GetPatientTestHistoryQuery` with the reviewed-only Conflict guard). Compile fixes during iteration: alias `BalanceProbe = TopLab.Application.Features.ResultsEntry.Common.BalanceProbe` in combined + history handlers (CS0104 translator-name clash), `.Error!`/`.Value!` idiom in all three handlers (CS8604/CS8602/CS8714), `ReportPdfWriter` profile line dedup (`  {AnalyteName}: {ResultValue} {Unit}`), removed a stray patient-id echo line, `RenderTestLine` no `TrimStart`. Infrastructure (`Printing/`): `IPdfPrinterDispatcher` + `ShellPdfPrinterDispatcher` (`Process.Start` `Verb="print"` + `UseShellExecute=true`, OS print association), `IReportPdfWriter` + `ReportPdfWriter` (`FileMode.CreateNew` never-overwrite; kind titles; `LabId:` vs `PatientId:` per `PrintLabIdInsteadOfPatientId`; Paper/TopSpace/HeaderFooter/DoctorSignature), `ReportPrintingService` (settings at call time, temp path `Path.GetTempPath()/TopLabPrint-{guid}.pdf` left in OS temp, all failures → `Error.Unexpected`, `catch OperationCanceledException → rethrow`, `catch Exception → Unexpected`), DI registration (3 Scoped + `using`). Tests: `FakeReportPrintingService` (records tokens), 3 handler test classes, `ReportProductionAuthorizationTests` (theory + `AuthorizationBehavior` deny/allow), `ValidatorRegistrationTests` extension, `ReportPdfWriterTests` (10) + `ReportPrintingServiceTests` (7, real `ApplicationDbContext` over InMemory — model `HasData` seeds supply ReportSettings/SystemSettings/PrinterAssignment on `EnsureCreated`; removed-as-needed for the missing-* cases; recording dispatcher), `InfrastructureRegistrationTests` DI-resolution extension.
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings. `dotnet test TopLab.sln` → Domain 378 / Application 1038 / Infrastructure 142 (1558 total, 0 failed).
- [x] **Stage 7 — Validation Gate:** VG-03 PASS — build zero/zero; all new tests green (Infra 16 new: writer 8 + service 7 + DI resolution 1; App 32 new); BR-07 matrix pinned (combined blocked `يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.` / history blocked / blank exempt / absolute bypass); `MarkPrinted` fields asserted (IsPrinted, PrintCount, LastPrintedByUserId, LastPrintedAtUtc, SaveChangesCallCount); grep gates — only the 3 print commands implement `IAuthorizedRequest` (zero under `Features/ReportProduction/Queries`), zero `PatientAccountCalculator` references in the feature (R-5), diff confined to allowed paths (A4: no Presentation/.csproj/migration).
- [x] **Stage 8 — Coverage:** App S3 footprint (3 command triplets + `ReportPrintEnvelope`) 152/158 = 96.2% ≥ 80%; Infra `Printing` footprint 178/228 = 78.1% ≥ 70% (`ReportPrintingService` 86.8%, `ReportPdfWriter` 81.9%, `ShellPdfPrinterDispatcher` 0% — thin OS-print-shell adapter, excluded-by-design, floor still met without a waiver).
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated below (Slice 3 done).
- [x] **Stage 10 — Git Commit (authorized local, never push):** `[M-07] Slice 3/5: Infrastructure: PDF-first IReportPrintingService + print commands — loop-engineering` + `Stages 1-10 verified. Gate VG-03 passed.` — committed on `main` as `664ed34` (memory checkbox confirmed after commit); never pushed.

---

## Slice 4: Application: automatic & manual history insertion + separate history report assembly

- **Goal:** Deliver `AutoInsertHistory` (no-op when `HistoryAutoDisplayEnabled == false`), `InsertHistoryResult` (manual, unconditional, identity-checked), and `GetSeparateHistoryReport` (P-04 model for the S3 print command), with the no-mutation pin on stored rows.
- **Touches:** `.../Commands/AutoInsertHistory/` (3 files, create); `.../Commands/InsertHistoryResult/` (3 files, create); `.../Queries/GetSeparateHistoryReport/` (2 files, create); `tests/.../Features/ReportProduction/AutoInsertHistoryCommandHandlerTests.cs` + `InsertHistoryResultCommandHandlerTests.cs` (create)
- **Validation Gate:** VG-04 — Application build zero/zero; S4 tests green incl. switch-false pin + no-mutation pin; coverage ≥ 80%.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings; `dotnet test TopLab.sln` → Domain 378 / Application 1038 / Infrastructure 142 (1558 total, 0 failed) — verified as S3 Stages 6–7 on this same tree (slice 3 commit `664ed34`).
- [x] **Stage 2 — Deep Understanding:** Re-read plan §6 S4 + FR-M07-003/005/006/007 + EC-09/10. Insertion = copy-into-DTO only, never a stored-row mutation (no-mutation pin). `AutoInsertHistoryCommand(PatientTestId)` → `Result<CombinedReportDto>`; no-op (empty `Lines`) when `HistoryAutoDisplayEnabled == false`; when true copies prior results for the same `TestId` under the resolved identity into the report model DTO. `InsertHistoryResultCommand(PatientTestId, SourcePatientTestId)` → `Result<CombinedReportDto>`; manual, unconditional (works with switch off), source must belong to the resolved identity else `Error.Conflict("النتيجة المحددة لا تنتمي لهذا المريض.")` (EC-10). `GetSeparateHistoryReportQuery(PatientId)` assembles the P-04 standalone history DTO (2 files — no validator) and is consumed by S3's `PrintHistoryReportCommand` (plan line 249) → S4 re-points that handler's `ISender` target from `GetPatientTestHistoryQuery` to `GetSeparateHistoryReportQuery` (S3 print behavior unchanged; S3 handler test canned-response key updated accordingly).
- [x] **Stage 3 — File Analysis:** `Common/PatientHistoryReader.cs` (`ResolveVisitPatients(db, patient, settings)` throws `ArgumentException` on unresolvable identity; `BuildEntries(db, visits)` returns `HistoryEntryDto` list ordered by `EnteredAtUtc desc` then `Id desc`); `GetPatientTestHistoryQueryHandler` (patient NotFound → settings `Unexpected("سجل إعدادات التقرير مفقود.")` → try/catch → translator Conflict → DTO); its test class seeding (`FakeApplicationDbContext.ReportSettings.Add(ReportSettings.CreateDefault())`, `Patient.Create(..., labId: LabId.Create("L-1"))`, `PatientTest` `Reviewed(...)` helper); `ReportSettings.CreateDefault()` History defaults (ByLabCode + auto=true) → switch-false pin must call `SetHistoryOptions(mode, false)`; validator style `GreaterThan(0)`; M-06 `NotFound("التحليل غير موجود")` precedent for missing PatientTest.
- [x] **Stage 4 — Planning:** (1) `Common/HistoryInsertion.cs` (create, internal static): `LineFromEntry(HistoryEntryDto)` → `CombinedReportLineDto(PatientTestId, TestId, TestName, TestCode, ResultKind, ResultValue, ResultFlag, FrozenRangeText: null, ProfileLines: empty, Culture: null)` — persistence-free model line. (2) `Commands/AutoInsertHistory/` (3 files): validator `PatientTestId > 0`; handler — current `PatientTest` by id → `NotFound("التحليل غير موجود")`; patient (not deleted) → `NotFound("المريض غير موجود.")`; settings → `Unexpected("سجل إعدادات التقرير مفقود.")`; `!settings.HistoryAutoDisplayEnabled` → Success with patient header + empty `Lines`; else resolve identity visits (try/catch → translator Conflict), `BuildEntries`, filter `TestId == current.TestId.Value && PatientTestId != current.Id.Value` ordered `EnteredAtUtc desc`, map via `LineFromEntry`, Success. (3) `Commands/InsertHistoryResult/` (3 files): validator both `> 0`; handler — same NotFound/patient/settings/identity chain; load source `PatientTest` by `SourcePatientTestId` (`NotFound("التحليل غير موجود")`); `!visits.Any(p => p.Id.Value == source.PatientId.Value)` → `Conflict("النتيجة المحددة لا تنتمي لهذا المريض.")` (EC-10); entry via `BuildEntries(db, [sourcePatient])` filtered to the source id → `LineFromEntry`; Success — switch-agnostic. (4) `Queries/GetSeparateHistoryReport/` (2 files): mirror `GetPatientTestHistoryQueryHandler` → `PatientHistoryDto` via the shared reader. (5) Re-point `PrintHistoryReportCommandHandler` ISender from `GetPatientTestHistoryQuery` → `GetSeparateHistoryReportQuery` (+ update its two canned-`FakeSender` keys in the test class). (6) Two test classes: `AutoInsertHistoryCommandHandlerTests` (switch-false empty pin EC-09; switch-true same-test prior copied incl. different-test exclusion; source rows re-read unchanged — no-mutation pin; missing row/patient/settings; no-shared-identity → empty) and `InsertHistoryResultCommandHandlerTests` (works with switch off; wrong-identity `Conflict` EC-10; source Not Found; current row Not Found; source row re-read unchanged). (7) `ValidatorRegistrationTests` extension (2 new validators). No gate/print changes beyond the re-point.
- [x] **Stage 5 — Execution:** Implemented `Common/HistoryInsertion.cs`, `Commands/AutoInsertHistory/` (3 files), `Commands/InsertHistoryResult/` (3 files), `Queries/GetSeparateHistoryReport/` (2 files), re-pointed `PrintHistoryReportCommandHandler` to `GetSeparateHistoryReportQuery`, updated S3 test canned-response keys. Fixed `HistorySortMode.All` → `HistorySortMode.ByLabCode` in test seed helper.
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings. `dotnet test TopLab.sln` → 1581 passed, 0 failed (Domain 378, App 1061, Infra 142).
- [x] **Stage 7 — Validation Gate:** VG-04 PASS — build zero/zero; switch-false zero-insertion test green; manual-insert-with-switch-off green; wrong-identity `Conflict` green; source-row re-read unchanged green; coverlet S4 footprint 82/82 = 100% ≥ 80%.
- [x] **Stage 8 — Documentation Update:** Memory file updated; evidence recorded above.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated below (Slice 4 done).
- [x] **Stage 10 — Git Commit (authorized local, never push):** `[M-07] Slice 4/5: Application history insertion — loop-engineering` + `Stages 1-10 verified. Gate VG-04 passed.` — on `main`, never push.

---

## Slice 5: Hardening, documentation, module close-out

- **Goal:** Prove the whole solution in Release with the full suite plus coverage floors, run the migration-scope gate (expect zero drift), pass the audit gate, and close the module (ADR-0040, tracking flip, handoff).
- **Touches:** `Docs/Source/Top_Lab_ADR.md` (append ADR-0040 — reconfirm max ADR at execution); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M07 row + dated change-log row); `Docs/Handoff_M07.md` (create per template); audit-gate test for print-command writes (create under `tests/TopLab.Infrastructure.Tests` or the established audit-test location)
- **Validation Gate:** VG-05 — Release build zero/zero; full suite green; zero-drift proven; coverage floors or waivers; audit gate passed; docs committed per convention; zero Presentation content (grep gate).

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Release build + full tests green (0/0). Record evidence.
- [ ] **Stage 2 — Deep Understanding:** Re-read plan §7 S5; coverage floors; migration-scope gate semantics (drift → stop + addendum, never silent migration); ADR-0040 contents (OD-07-A…E outcomes, settings-at-print-time, zero-drift result); close-out convention (tracking flip + change-log + handoff).
- [ ] **Stage 3 — File Analysis:** Inspect `Top_Lab_ADR.md` (confirm max ADR), `Top_Lab_Master_Tracking_Sheet.md` (locate M07 row), `Docs/Source/Top_Lab_Handoff_Template.md`, an existing audit-gate test + `InMemoryContextFactory`, coverlet setup.
- [ ] **Stage 4 — Planning:** Migration-scope gate first → audit-gate test → Release build + full suite with coverage → ADR-0040 → tracking flip → handoff.
- [ ] **Stage 5 — Execution:** Implement the plan.
- [ ] **Stage 6 — Post-Execution Verification:** Release build 0/0; full suite green; `dotnet ef migrations has-pending-model-changes` → no changes; snapshot clean.
- [ ] **Stage 7 — Validation Gate:** VG-05 — all code/test gates pass; coverage floors met or waived in the handoff; audit gate green; zero Presentation content in the diff.
- [ ] **Stage 8 — Documentation Update:** ADR-0040 appended; M07 row flipped 🟩 Done with dated change-log entry; `Docs/Handoff_M07.md` created per template; slice checkboxes marked.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-07] Slice 5/5: Hardening / close-out — loop-engineering` + `Stages 1-10 verified. Gate VG-05 passed.` — on `main`, never push.

---

## Current Status

- Overall: 4/5 slices done — Slice 4 complete
- Slice 1 — Domain rules: combined-report selection + patient-history resolver: [x] Done — VG-01 PASS
- Slice 2 — Application core surface: combinable list + combined/blank builders + history queries: [x] Done — VG-02 PASS
- Slice 3 — Infrastructure: PDF-first printing + print commands: [x] Done — VG-03 PASS
- Slice 4 — Application: history insertion: [x] Done — VG-04 PASS
- Slice 5 — Hardening, documentation, module close-out: [ ] Pending

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-15 | 0 | — | Memory file created | OK | — |
| 2026-09-15 | 1 | 1 | Pre-exec: build 0/0; tests 1462/1462 green (Domain 360, App 976, Infra 126) | PASS | — |
| 2026-09-15 | 1 | 2-4 | Deep understanding + file analysis + planning recorded | PASS | — |
| 2026-09-15 | 1 | 5 | Created `CombinedReportSelection`, `PatientHistoryResolver`, 2 test classes | PASS | — |
| 2026-09-15 | 1 | 6 | Domain build 0/0; Domain tests 378/378 green | PASS | — |
| 2026-09-15 | 1 | 7 | VG-01: coverage 100% on `Reports` scope; all negative paths tested | PASS | — |
| 2026-09-15 | 1 | 10 | Local commit on `main` | OK (`9e4c84e`) | — |
| 2026-09-15 | 2 | 1 | Pre-exec: build 0/0; tests 1480/1480 green (Domain 378, App 976, Infra 126) | PASS | — |
| 2026-09-15 | 2 | 2-4 | Deep understanding + file analysis + planning recorded | PASS | — |
| 2026-09-15 | 2 | 5 | Created Common (DTOs/policy/translator/reader), 3 queries + 2 build commands + 5 test classes (30 tests) | PASS | — |
| 2026-09-15 | 2 | 6 | Solution build 0/0; tests 1510/1510 green (Domain 378, App 1006, Infra 126) | PASS | — |
| 2026-09-15 | 2 | 7 | VG-02: grep gates clean; S2 footprint coverage 86.8% (383/441) ≥ 80% | PASS | — |
| 2026-09-15 | 2 | 10 | Local commit on `main` (S2 Stage 10 checkbox later confirmed `[x]`) | OK (`d6623e1`) | — |
| 2026-09-15 | 3 | 1 | Pre-exec: build 0/0; tests 1510/1510 green (Domain 378, App 1006, Infra 126) | PASS | — |
| 2026-09-15 | 3 | 2-3 | Deep understanding + file analysis: port/BR-07/exporter/settings/dispatch seams + test precedents verified | PASS | — |
| 2026-09-15 | 3 | 4 | Stage 4 plan written (envelope token + 3 print commands + Printing/ service + writer + dispatcher + tests) | PASS | — |
| 2026-09-15 | 3 | 5 | Envelope + translator overload + 3 print command triplets + Printing/ service/writer/dispatchers + DI; compile-fix rounds (translator alias, `.Error!`/`.Value!`, writer line fixes); 32 App tests + 17 Infra tests | PASS | — |
| 2026-09-15 | 3 | 6 | Solution build 0/0; tests 1558/1558 green (Domain 378, App 1038, Infra 142) | PASS | — |
| 2026-09-15 | 3 | 7 | VG-03: BR-07 matrix pinned; MarkPrinted audit asserted; grep gates clean (3 print `IAuthorizedRequest`, zero in Queries, zero `PatientAccountCalculator` in feature); diff A4-clean | PASS | — |
| 2026-09-15 | 3 | 8 | Coverage: App S3 footprint 96.2% (152/158) ≥ 80%; Infra `Printing` 78.1% (178/228) ≥ 70% | PASS | — |
| 2026-09-15 | 3 | 9 | Memory updated (header, slice index, S3 checklist, Current Status) | PASS | — |
| 2026-09-15 | 3 | 10 | Local commit on `main` (S3 Stage 10 checkbox later confirmed `[x]`) | OK (`664ed34`) | — |
| 2026-09-15 | 4 | 1 | Pre-exec: build 0/0; tests 1558/1558 green (Domain 378, App 1038, Infra 142) | PASS | — |
| 2026-09-15 | 4 | 2-4 | Deep understanding + file analysis + planning recorded | PASS | — |
| 2026-09-15 | 4 | 5 | Implemented HistoryInsertion, AutoInsertHistory, InsertHistoryResult, GetSeparateHistoryReport; re-pointed PrintHistoryReportHandler; fixed HistorySortMode.All → ByLabCode | PASS | — |
| 2026-09-15 | 4 | 6 | Solution build 0/0; tests 1581/1581 green (Domain 378, App 1061, Infra 142) | PASS | — |
| 2026-09-15 | 4 | 7 | VG-04: S4 footprint coverage 100% (82/82) ≥ 80% | PASS | — |
| 2026-09-15 | 4 | 8-9 | Memory updated (S4 checklist, slice index, Current Status) | PASS | — |
| 2026-09-15 | 4 | 10 | Local commit on `main` | OK (`<pending>`) | — |

## Stop Report (append only if a stop condition triggers)
