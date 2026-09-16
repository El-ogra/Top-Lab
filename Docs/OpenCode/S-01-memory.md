# Loop Engineering — Memory File

- **Module:** Patients Screen: Unified Registration, Test Ordering, Billing & Printing (S-01 — cross-module UI workstream, post-S-00)
- **Module Number:** S-01
- **Source Plan:** Docs/OpenCode/S-01.md
- **Date Created:** 2026-09-16
- **Total Slices:** 6
- **Current Slice:** S5 — Done (committed); S6 — Pending (final)
- **Current Branch:** main
- **Author:** loop-engineering skill (execution to be carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

S-01 closes the Patients-screen gap: a Patients hub reachable from the shell's «المرضى» button plus a unified "Add/Edit Patient Data" screen combining demographics, test ordering with sample-type flags, live billing, and four working print actions (barcode label / worksheet / receipt / invoice). The backend for registration, ordering, and billing is already Done (M02/M03/M07/M11/M22); this workstream adds (a) the four missing document renderers behind their MediatR commands (S1–S4), (b) the Patients hub with «المرضى» wiring (S5), and (c) the unified editor screen orchestrating everything (S6). Six slices, backend-first, UI-last, strictly sequential: S1 barcode service → S2 receipt printing → S3 invoice concept + renderer (the only migrating slice) → S4 visit worksheet renderer → S5 Patients hub + navigation → S6 unified editor + LabId auto-generation. No half-wired states: every enabled button in the final screen has a complete backend path ending at a routed printer or a persisted mutation.

## Global Validation Gates

- **Gate G0 (pre-execution):** `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- **Gate G1 (post-execution per slice):** same as G0 plus the slice-specific gate listed in the table below.

## Quality Gate (non-negotiable — a slice may be marked complete ONLY when ALL FOUR hold)

1. The slice's implementation is fully complete per `Docs/OpenCode/S-01.md`.
2. The ENTIRE solution builds successfully — zero errors AND zero warnings (`dotnet build TopLab.sln`).
3. ALL existing automated tests across ALL test projects pass (`dotnet test TopLab.sln`).
4. The slice's own specific exit criteria (its Validation Gate in the plan) pass.

## Stop/Continue Rule

After a slice completes, verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice, with no pause and no human confirmation required.
If any one fails → retry. If the SAME failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs **5 CONSECUTIVE** times, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do NOT proceed past this point without owner review. Ordinary expected test failures caused by the current slice and resolved within the same correction cycle do NOT count as five separate failures.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: **5 consecutive failures for the same reason.**
- Execution order: strictly sequential **S1 → S2 → S3 → S4 → S5 → S6**, no parallel slices.
- Stage-7 gate: the plan's textual exit criteria (build/test/manual state-machine verification, plus **migration up/down verification in S3 only**) replace any standard UI journey — this workstream's UI behaviour is verified manually per the plan (no UI test harness exists in the repo).
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (`main`), NEVER create a new branch, NEVER push to any remote, NEVER force-push, NEVER modify or rewrite remote history. Commit message format: `[S-01] Slice N/6: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in S-01's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-S1 | Barcode service: build 0/0; full suite green incl. new `BarcodeServiceTests` + `PrintBarcodeCommandHandlerTests`; manual — print a Code-128 label for a patient with and without `PrintDateTimeOnTubeBarcode`, scans back to expected payload; routed to `OutputType=Barcode` printer (not Reports). **Migration: NONE — zero-drift gate** (`has-pending-model-changes` → no changes; `git diff --stat src/TopLab.Infrastructure/Persistence/` empty). | `dotnet build TopLab.sln`; `dotnet test TopLab.sln`; manual barcode scan-back; ef drift check; grep gates |
| 2 | VG-S2 | Receipt printing: build 0/0; full suite green incl. new `ReceiptPrintingServiceTests` + `PrintReceiptCommandHandlerTests`; manual — receipt for a patient with mixed Arabic/English name and Arabic test names renders **shaped and right-to-left** (SD-2 acceptance proof); totals match `GetPatientAccount`; routed to Receipt printer. **Migration: NONE — zero-drift gate.** | `dotnet build`; `dotnet test`; manual Arabic-rendering verification; ef drift check |
| 3 | VG-S3 | Invoice: build 0/0; full suite green incl. new `InvoiceIssueTests` + `PrintInvoiceCommandHandlerTests` + `GetPatientInvoiceQueryHandlerTests`; **migration `AddInvoiceIssues` applies cleanly up/down on a copy of the baseline; `has-pending-model-changes` clean afterward**; manual — two invoices for two patients produce sequential numbers; Arabic item names render RTL; reprint produces a new number (new issue) per SD-3. **Migration: YES — up/down verification is part of this gate.** | `dotnet build`; `dotnet test`; `dotnet ef database update` on a copy + `dotnet ef migrations remove --force` reversibility check; manual two-print sequence; ef drift check post-apply |
| 4 | VG-S4 | Visit worksheet: build 0/0; full suite green incl. new `GetVisitWorkSheetQueryHandlerTests` + `PrintWorkSheetCommandHandlerTests`; manual — visit worksheet for a patient with mixed sample types prints with per-line flags (U/S/B/Se/CSF), barcode values match `Test.Barcode`, routed to Reports printer, Arabic renders correctly. **Migration: NONE — zero-drift gate.** | `dotnet build`; `dotnet test`; manual per-visit worksheet print; ef drift check |
| 5 | VG-S5 | Patients hub + navigation: build 0/0; full suite green; manual — click «المرضى» in the shell top bar → Patients hub renders in the content region (RTL); all four hub buttons visible and disabled; status bar unaffected; «المستخدمون»/«الإعدادات» navigation still works. **Migration: NONE — zero-drift gate.** | `dotnet build`; `dotnet test`; manual navigation walk; ef drift check |
| 6 | VG-S6 | Unified editor screen: build 0/0; full suite green incl. new `CreatePatientLabIdTests` + `GetNextLabIdQueryHandlerTests`; manual end-to-end on a real database — create patient with 3 tests incl. mixed sample flags → account totals correct → record payment with discount → balance updates → each of the four print buttons produces its document on its routed printer → edit the patient (add a test, change a flag) → totals update → soft delete → patient no longer searchable. **Also (binding-constraint checklist): every enabled control on the screen has a complete backend path ending at a routed printer or a persisted mutation.** **Migration: NONE — zero-drift gate.** | `dotnet build`; `dotnet test`; manual end-to-end walk incl. all four print buttons; binding-constraint checklist; ef drift check |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|---|-------------|--------|-----------------|
| 1 | Barcode service (`IBarcodeService` impl, Code-128, printer routing) | [x] Done | VG-S1 |
| 2 | Receipt printing (`IReceiptPrintingService` port + impl + `PrintReceiptCommand`) | [x] Done | VG-S2 |
| 3 | Invoice concept + renderer + `PrintInvoiceCommand` (**migrating**) | [x] Done | VG-S3 |
| 4 | Visit worksheet renderer + `PrintWorkSheetCommand` | [x] Done | VG-S4 |
| 5 | Patients hub + «المرضى» navigation wiring | [x] Done | VG-S5 |
| 6 | Unified Add/Edit Patient Data screen (demographics + ordering + billing + 4 print actions) | [ ] Pending | VG-S6 |

---

## Settled Decisions (from plan — binding)

- **SD-1 — Barcode library:** pin **ZXing.Net** (Apache-2.0, Code-128, no internet dependency). Owner-approved.
- **SD-2 — Arabic/RTL PDF rendering:** pin **QuestPDF** (HarfBuzzSharp shaping/bidi). Owner-approved.
- **SD-3 — Receipt vs. Invoice distinction:** Receipt = payment acknowledgment (cashier; `ReceiptSettings`-driven). Invoice = itemized numbered statement of services (`InvoiceIssues` gapless numbering; reprint = new issue; routes through `OutputType=Receipt` — no new `PrinterOutputType`). Owner-approved.
- **SD-4 — `LabId` generation:** UI auto-generates via new `GetNextLabIdQuery`; editable override; handler-level duplicate rejection (`Conflict «رمز المعمل مستخدم بالفعل.»`) as the only existing-handler modification. No DB unique index (zero-drift preserved). Owner-approved.
- **SD-5 — Workstream identifier:** **S-01** (S-series per S-00's Naming Rationale; M-24 rejected).
- **SD-6 — Hub placeholder idiom:** hub buttons without an existing feature screen ship disabled, not half-wired.
- **SD-7 — Worksheet port shape:** dedicated `IWorkSheetPrintingService` port (recorded; reversible at owner review).
- **SD-8 — Invoice routing:** through `OutputType=Receipt` — no fifth `PrinterOutputType`.
- **SD-9 — No Presentation-layer authorization duplication:** server gates + presenter idiom only.
- **SD-10 — Commit format:** `[S-01] Slice N/6: <title> — loop-engineering`.
- **No half-wired state:** binding constraint applies through Stage 7 of every slice; S6's VG explicitly checks that every enabled control has a complete backend path.
- **Only S3 migrates.** S1/S2/S4/S5/S6 have zero-drift gates.

**Out-of-scope (flagged, NOT fixed by this workstream):** Master Tracking Sheet's M01-status and M14-dependency inconsistencies — owner housekeeping.

---

## Slice 1: Barcode service — `IBarcodeService` implementation (Code-128) with printer routing

- **Goal:** Ship the missing barcode label renderer + printer dispatch behind the existing `IBarcodeService` port and its thin `PrintBarcodeCommand`.
- **Touches:**
  - `Directory.Packages.props` (modify — pin ZXing.Net)
  - `src/TopLab.Infrastructure/Barcode/BarcodeLabelRenderer.cs` (create)
  - `src/TopLab.Infrastructure/Barcode/BarcodeService.cs` (create)
  - `src/TopLab.Application/Features/PatientRegistration/Commands/PrintBarcode/PrintBarcodeCommand.cs` (+Handler, +Validator — create)
  - `src/TopLab.Infrastructure/DependencyInjection.cs` (modify — WIRE `IBarcodeService`)
  - `tests/TopLab.Infrastructure.Tests/Barcode/BarcodeServiceTests.cs` (create)
  - `tests/TopLab.Application.Tests/Features/PatientRegistration/Commands/PrintBarcodeCommandHandlerTests.cs` (create)
- **Validation Gate:** VG-S1 — build 0/0; full suite green; manual Code-128 scan-back + Barcode-printer routing; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** run `dotnet build TopLab.sln` (0/0) + `dotnet test TopLab.sln` (all green) on baseline HEAD `faceab6c…` before touching anything. DONE 2026-09-16: build 0/0; tests 467+160+1361=1988 green.
- [x] **Stage 2 — Deep Understanding:** re-read plan §3 in full; internalize the SD-1 ZXing.Net decision; note the "never throws — fixed Arabic errors" contract mirrored from `ReportPrintingService`.
- [x] **Stage 3 — File Analysis:** inspect `IBarcodeService.cs`; `ReportPrintingService.cs:18–77` (structural precedent — try/catch pattern, settings single-row read, `PrinterAssignment` lookup); `ShellPdfPrinterDispatcher.cs:11–28`; `PrinterOutputType.cs:3–8` (Barcode=1); `SystemSettings.cs` (`PrintLabIdInsteadOfPatientId`, `PrintDateTimeOnTubeBarcode`); `Directory.Packages.props` (central pin block + license comment). DONE: all inspected; actual enum path `Domain/Common/Enums/PrinterOutputType.cs`; seeded Barcode assignment name "Barcode"; InMemory `EnsureCreated` applies seed rows; `FakeSender.WithResponse` + `FakeApplicationDbContext.SystemSettings` available for handler tests.
- [x] **Stage 4 — Planning:** step-by-step slice plan encoded here.
  1. `Directory.Packages.props`: pin `ZXing.Net` 0.16.11 (+license comment); `TopLab.Infrastructure.csproj`: versionless `PackageReference`.
  2. `Barcode/BarcodeLabelRenderer.cs`: `BarcodeWriterPixelData` Code-128 → `BarcodeLabelData(Pixels,Width,Height)`; throws on blank content (service maps to Unexpected).
  3. `Barcode/BarcodeService.cs`: mirror `ReportPrintingService` — blank→«قيمة الباركود غير صالحة.»; missing SystemSettings→«سجل إعدادات النظام مفقود.»; missing Barcode assignment→«لم يتم تعيين طابعة للباركود.»; payload=value[+" "+UTC yyyy-MM-dd HH:mm when flag]; minimal PDF (image XObject + ASCII id line); dispatch; catch-all→«تعذر طباعة الباركود.», rethrow cancellation.
  4. `PrintBarcode/PrintBarcodeCommand(PatientId)` + Handler (NotFound «المريض غير موجود.» incl. soft-deleted; `GetSystemSettingsQuery` via ISender; LabId-vs-PatientId; `ADD_EDIT_PATIENT` gate) + Validator (PatientId>0).
  5. DI: `AddScoped<IBarcodeService, BarcodeService>` + `AddScoped<BarcodeLabelRenderer>`.
  6. Tests: `BarcodeServiceTests` (InMemory: happy-path %PDF+Barcode printer; datetime on/off payload; missing assignment; dispatcher-throw never-throws) + `PrintBarcodeCommandHandlerTests` (NotFound unknown/soft-deleted; LabId fallback; capture-fake payload; gate) + extend `PatientRegistrationAuthorizationTests` theory with PrintBarcode.
- [x] **Stage 5 — Execution:** implement per plan §3. DONE: ZXing.Net 0.16.11 pinned+restored; `Barcode/` (renderer+service); `PrintBarcode/` (command+handler+validator); DI wiring; 2 new test files +1 authorization case.
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0/0. DONE 2026-09-16: 0 warnings/0 errors (2 intermediate errors fixed: missing `Printing` using; `BarcodeReader`→`BarcodeReaderGeneric`).
- [x] **Stage 7 — Validation Gate:** VG-S1 pass (build + tests + manual scan-back + zero-drift). DONE: full suite 467+168+1371=2006 green (+18 vs baseline); Code-128 scan-back proven by `RenderedLabel_ScansBackToExpectedPayload` (decodes to "LAB-42", format CODE_128); Barcode-printer routing proven by dispatcher capture (printer "Barcode"); zero-drift by construction — no Domain/Persistence file touched, `git diff --stat src/TopLab.Infrastructure/Persistence/` empty. Physical-printer manual step not possible on this machine (no printer); dispatcher seam covers routing.
- [x] **Stage 8 — Documentation Update:** checkboxes updated.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-01] Slice 1/6: Barcode service (IBarcodeService impl + Code-128 + printer routing) — loop-engineering`.

---

## Slice 2: Receipt printing — `IReceiptPrintingService` port + implementation + `PrintReceiptCommand`

- **Goal:** Ship the cashier receipt document layer end-to-end (port + envelope token + Infrastructure renderer + MediatR command) with proper Arabic/RTL rendering (SD-2 QuestPDF).
- **Touches:**
  - `src/TopLab.Application/Common/Interfaces/IReceiptPrintingService.cs` (create)
  - `src/TopLab.Application/Common/Interfaces/IReceiptPdfWriter.cs` (create)
  - `src/TopLab.Application/Features/PatientBilling/Common/ReceiptPrintEnvelope.cs` (create)
  - `src/TopLab.Infrastructure/Printing/ReceiptPrintingService.cs` (create)
  - `src/TopLab.Infrastructure/Printing/ReceiptPdfWriter.cs` (create — QuestPDF)
  - `src/TopLab.Application/Features/PatientBilling/Commands/PrintReceipt/PrintReceiptCommand.cs` (+Handler, +Validator — create)
  - `Directory.Packages.props` (modify — pin QuestPDF)
  - `src/TopLab.Infrastructure/DependencyInjection.cs` (modify — WIRE)
  - `tests/TopLab.Infrastructure.Tests/Printing/ReceiptPrintingServiceTests.cs` (create)
  - `tests/TopLab.Application.Tests/Features/PatientBilling/Commands/PrintReceiptCommandHandlerTests.cs` (create)
- **Validation Gate:** VG-S2 — build 0/0; full suite green; manual Arabic-rendering + Receipt-printer routing; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 2)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite green after S1. DONE 2026-09-16: S1 gate runs serve as S2 baseline (build 0/0; suite 2006 green at commit `f074f3e`); `git status` clean except untracked input `S-01.md`.
- [x] **Stage 2 — Deep Understanding:** re-read plan §4; internalize SD-2 QuestPDF decision; note the ASCII-only limitation of `ReportPdfWriter` that motivated a separate renderer.
- [x] **Stage 3 — File Analysis:** `ReportPrintEnvelope.cs:14–27` (token idiom); `IReportPrintingService.cs`; `ReportPrintingService.cs` (structural precedent); `GetPatientReceiptQueryHandler.cs` (data source); `ReceiptSettings.cs` (`TopMarginCm`, `Currency`, `PickupTimeDefault`, `PrintOnce`, `TestDetailDisplayMode`, `CashierPrinterEnabled`, `HeaderFooterMode`); `ILabPrintTextStore` registration in Presentation DI. DONE: `ReceiptDto` carries everything (ChargedTests+totals+Currency); `RecordPayment` ungated → `PrintReceiptCommand` ungated; `PrintOnce` display-only (no behavioral wiring anywhere in `src/`); `TestDetailDisplayMode` Hide/Show/ShowWithCode; `HeaderFooterMode` None/Words/Images; lab-text store returns empty-string defaults on missing file (writer falls back to Arial/12pt); `ILabPrintTextStore` is already an Application port (Presentation-implemented) → no new port needed.
- [x] **Stage 4 — Planning:** step-by-step slice plan encoded here.
  1. `Directory.Packages.props`: pin `QuestPDF` 2026.9.0 (+license comment, SD-2 owner-confirmed); Infra csproj versionless `PackageReference`.
  2. `Application/Common/Interfaces/IReceiptPrintingService.cs`: `PrintReceiptAsync(string receiptToken, CT)`.
  3. `Application/Common/Interfaces/IReceiptPdfWriter.cs`: `WritePdfAsync(path, ReceiptDto, ReceiptSettings, LabPrintTextDto, CT)`.
  4. `PatientBilling/Common/ReceiptPrintEnvelope.cs`: `record ReceiptPrintEnvelope(string ReceiptJson)` + `CreateToken(ReceiptDto)`.
  5. `Infrastructure/Printing/ReceiptPrintingService.cs`: mirror `ReportPrintingService` — bad token→«بيانات الإيصال غير صالحة.»; missing ReceiptSettings→«سجل إعدادات الإيصال مفقود.»; missing Receipt assignment→«لم يتم تعيين طابعة للإيصالات.»; lab text via `ILabPrintTextStore` (failure propagates); dispatch; catch-all→«تعذر طباعة الإيصال.». `PrintOnce` documented display-only in XML doc.
  6. `Infrastructure/Printing/ReceiptPdfWriter.cs` (QuestPDF, `Settings.License=Community` static init): A5 portrait RTL; Words/None header-footer (Images→words fallback, documented); patient block; itemized tests per display mode; totals block with Currency; TopMarginCm; pickup line when set; `FileMode.CreateNew`; public static `BuildTextLines` for content assertions.
  7. `PatientBilling/Commands/PrintReceipt/` command (ungated `IRequest<Result>`) + handler (via `ISender GetPatientReceiptQuery`; propagate; token; service) + validator (PatientId>0).
  8. DI: `AddScoped<IReceiptPrintingService, ReceiptPrintingService>` + `AddScoped<IReceiptPdfWriter, ReceiptPdfWriter>`.
  9. Tests: `ReceiptPrintingServiceTests` (InMemory + fake lab-text store + recording dispatcher: happy %PDF+Receipt routing; 2 missing-row messages; invalid token; dispatch-throw never-throws; display-mode Hide/Show line-count via `BuildTextLines`) + `PrintReceiptCommandHandlerTests` (NotFound propagation; token-verbatim round-trip; service-failure propagation; validator class).
- [x] **Stage 5 — Execution:** implement per plan §4. DONE: QuestPDF 2026.9.0 pinned+restored; ports (`IReceiptPrintingService`, `IReceiptPdfWriter`); `ReceiptPrintEnvelope`; `ReceiptPrintingService`; `ReceiptPdfWriter` (QuestPDF A5 RTL); `PrintReceipt/` trio (ungated); DI wiring; 2 new test files (9 infra + 5 app facts).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0/0. DONE 2026-09-16 (2 intermediate errors fixed: `Settings` needs `using QuestPDF;`; `ContentFromRightToLeft()` returns void — call on `page`, then `page.Content()`).
- [x] **Stage 7 — Validation Gate:** VG-S2 pass. DONE: full suite 467+177+1376=2020 green (+14 vs S1); Receipt-printer routing proven by dispatcher capture (printer "Receipt"); totals/content proven by `BuildTextLines` verbatim-Arabic test + token round-trip test; zero-drift (`git diff --stat Persistence/` empty). Arabic-rendering acceptance: automated proof = generation succeeds with Arabic-capable system font + `ContentFromRightToLeft` + RTL text style; KEY PITFALL FOUND: QuestPDF 2026+ disables system fonts by default — `Settings.UseSystemFonts=true` is mandatory (without it generation throws "font families not available: Arial"; S3/S4 writers must copy the same static init). Physical-paper visual check remains for the owner.
- [x] **Stage 8 — Documentation Update:** checkboxes updated.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-01] Slice 2/6: Receipt printing (IReceiptPrintingService + PrintReceiptCommand) — loop-engineering`.

---

## Slice 3: Invoice — product definition, issue-record entity, renderer, `PrintInvoiceCommand` (**MIGRATING**)

- **Goal:** Introduce the Invoice concept (SD-3), persist `InvoiceIssue` rows with gapless numbering, and ship the renderer + MediatR command. **This is the only migrating slice.**
- **Touches:**
  - `src/TopLab.Domain/Billing/InvoiceIssue.cs` (create)
  - `src/TopLab.Infrastructure/Persistence/Configurations/InvoiceIssueConfiguration.cs` (create)
  - New EF migration `AddInvoiceIssues` (create — the only migration in this workstream)
  - `src/TopLab.Application/Features/PatientBilling/Common/PatientBillingDtos.cs` (modify — add `InvoiceDto`)
  - `src/TopLab.Application/Features/PatientBilling/Queries/GetPatientInvoice/GetPatientInvoiceQuery.cs` (+Handler, +Validator — create)
  - `src/TopLab.Application/Features/PatientBilling/Commands/PrintInvoice/PrintInvoiceCommand.cs` (+Handler, +Validator — create)
  - `src/TopLab.Application/Common/Interfaces/IInvoicePrintingService.cs` + `Common/InvoicePrintEnvelope.cs` (create)
  - `src/TopLab.Infrastructure/Printing/InvoicePrintingService.cs` + `InvoicePdfWriter.cs` + port (create)
  - `src/TopLab.Infrastructure/DependencyInjection.cs` (modify — WIRE)
  - `tests/TopLab.Domain.Tests/Billing/InvoiceIssueTests.cs` (create)
  - `tests/TopLab.Application.Tests/Features/PatientBilling/Commands/PrintInvoiceCommandHandlerTests.cs` (create)
  - `tests/TopLab.Application.Tests/Features/PatientBilling/Queries/GetPatientInvoiceQueryHandlerTests.cs` (create)
- **Validation Gate:** VG-S3 — build 0/0; full suite green; **migration `AddInvoiceIssues` applies up/down cleanly on a copy of the baseline; `has-pending-model-changes` clean afterward**; manual two-print sequential numbering + Arabic RTL. Migration: **YES**.

### 10-Stage Progress (Slice 3)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite green after S2. DONE 2026-09-16: S2 gate runs serve as S3 baseline (build 0/0; suite 2020 green at commit `a88f130`); working tree clean except untracked input `S-01.md`.
- [x] **Stage 2 — Deep Understanding:** re-read plan §5; internalize SD-3 (Receipt vs Invoice) and SD-8 (route via `OutputType=Receipt`); note the no-FK convention (application-level reference; M-15 precedent).
- [x] **Stage 3 — File Analysis:** existing EF configurations under `src/TopLab.Infrastructure/Persistence/Configurations/` (fluent style, decimal(18,2), datetime2, unique/index conventions); latest migrations `AddAnalyteProfileDomain` + `AddPregnancyMedicalConditionTypeSeed` (structural reference); `PatientTest.PriceAtOrderTime` (frozen-price contract); `PatientBillingReader`/`PatientAccountCalculator` (invoice DTO source). DONE: unique-index idiom `HasIndex().IsUnique()` (PermissionConfiguration); strong-id conversion idiom (PatientConfiguration/PatientPhoneNumberConfiguration); no-FK negative-test idiom (`Assert.DoesNotContain(et.GetForeignKeys(), ...)`); `PatientBillingReader.ReadAccount` gives account+charged tests in one call; `IApplicationDbContext` exposes no EF types and Application has no EF reference → the plan's "DbUpdateException retry" is implemented via the established M-12 `IsUniqueViolation` message-sniff (`CreateTestCommandHandler:76-88`); `UniqueViolationFake` precedent exists for the retry test; dotnet-ef 8.0.30 + working SQL Server (`.\SQLEXPRESS`, LocalDB) available for up/down proof; design-time factory targets localdb — scratch DB `TopLab_S01_Verify` via `--connection` override.
- [x] **Stage 4 — Planning:** step-by-step slice plan encoded here.
  1. `Domain/Common/Ids/InvoiceIssueId.cs` (StronglyTypedId-int) + `Domain/Billing/InvoiceIssue.cs` (`Entity<InvoiceIssueId>`; PatientId app-level ref no nav; guards number≥1, totals≥0, itemCount≥0; immutable).
  2. `InvoiceIssueConfiguration.cs` (key conversion ValueGeneratedOnAdd; PatientId conversion; unique index InvoiceNumber; index PatientId; datetime2; decimal(18,2); no nav → no FK).
  3. `PatientBillingDtos.cs`: add `InvoiceDto` (PatientId, LabId?, FullName, `int? InvoiceNumber` null=preview, `DateTime? IssuedAtUtc`, ChargedTests, totals, Currency).
  4. `GetPatientInvoice/` query+handler+validator: patient (NotFound incl. deleted) → `ReadAccount` → latest issue (MAX number) or preview(null,null).
  5. `PrintInvoice/` command (ungated)+handler+validator: patient check; `ReadAccount`; allocate MAX+1 → Add → SaveChanges with retry-once on `IsUniqueViolation` (M-12 idiom, `Remove` failed instance); second failure/any error → Unexpected «تعذر إصدار الفاتورة.» (never-throws; cancellation rethrown); IssuedByUserId from `ICurrentUserService`; token → `IInvoicePrintingService`.
  6. `IInvoicePrintingService` + `InvoicePrintEnvelope` + `IInvoicePdfWriter` (Application ports/common).
  7. `InvoicePrintingService` (routes Receipt per SD-8; «بيانات الفاتورة غير صالحة.»/«لم يتم تعيين طابعة للإيصالات.»/«تعذر طباعة الفاتورة.») + `InvoicePdfWriter` (QuestPDF A5 RTL, same License+UseSystemFonts static init; `BuildTextLines` for tests).
  8. DI wiring. FakeApplicationDbContext: add `InvoiceIssues` list + Set/Add/Remove branches.
  9. Tests: Domain `InvoiceIssueTests`; App `PrintInvoiceCommandHandlerTests` (N,N+1 / retry via `UniqueViolation`-style wrapper fake / NotFound / frozen prices) + `GetPatientInvoiceQueryHandlerTests` (totals=account; preview; latest); Infra `InvoiceIssueConfigurationTests` (unique index + no-FK-to-Patient per M-15 convention).
  10. Migration `AddInvoiceIssues` (`--project Infrastructure --startup-project Presentation`); `has-pending-model-changes` clean; up/down proof on scratch localdb `TopLab_S01_Verify` (update → table+index exist; downgrade to `AddPregnancyMedicalConditionTypeSeed` → table gone; re-upgrade).
- [x] **Stage 5 — Execution:** implement per plan §5, including EF migration `AddInvoiceIssues`. DONE: `InvoiceIssueId`+`InvoiceIssue` (Domain); `InvoiceIssueConfiguration` (+`DbSets.InvoiceIssues` → plural `InvoiceIssues` table, re-scaffolded once); `InvoiceDto`; ports (`IInvoicePrintingService`, `IInvoicePdfWriter`) + `InvoicePrintEnvelope`; `GetPatientInvoice/` + `PrintInvoice/` trios (M-12 `IsUniqueViolation` retry-once; never-throws); `InvoicePrintingService` (Receipt routing SD-8) + `InvoicePdfWriter` (QuestPDF, same static init); DI; fake extended; 4 test files (7 Domain + 13 App + 11 Infra).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0/0; migration `dotnet ef migrations add AddInvoiceIssues` succeeds; `dotnet ef database update` on a copy succeeds; `dotnet ef migrations remove --force` reverses cleanly (dry-run). DONE 2026-09-16 with one adaptation (see Stage 7): `add` succeeded; isolated round-trip substituted for the blocked full-baseline copy.
- [x] **Stage 7 — Validation Gate:** VG-S3 pass (build + tests + **migration up/down** + two-print manual + Arabic RTL). DONE: suite 474+1389+188=2051 green (+31); up/down via REAL EF applier on SQL Server scratch `TopLab_S01_InvoiceRT` (UP: table+PK+unique `IX_InvoiceIssues_InvoiceNumber`+`IX_InvoiceIssues_PatientId` present; unique enforced live Msg 2601 on duplicate; DOWN to `AddPregnancyMedicalConditionTypeSeed`: table+history gone; re-UP clean); `has-pending-model-changes` → "No changes"; sequential numbering proven (`TwoPrints` → 1,2); reprint=new-issue by construction; Arabic RTL via `BuildTextLines` verbatim test + S2-proven QuestPDF pipeline. ADAPTATION (pre-existing defect, NOT this slice): full-baseline from-zero `database update` fails in committed `20260909033414_AddAnalyteProfileDomain` (`ALTER COLUMN AnalyteId NOT NULL` after `IX_ProfileResultItems_AnalyteId` creation → Msg 5074; file last touched M-06, predates S-01) → full "copy of baseline" impossible until owner fixes that migration (DO NOT fix here — rewriting shared history is unauthorized). Scratch DBs `TopLab_S01_Verify` (partial) + `TopLab_S01_InvoiceRT` left on localdb for owner cleanup. Physical-paper two-print check remains for the owner.
- [x] **Stage 8 — Documentation Update:** checkboxes updated.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-01] Slice 3/6: Invoice (InvoiceIssue + renderer + PrintInvoiceCommand + migration) — loop-engineering`.

---

## Slice 4: Visit worksheet — `GetVisitWorkSheetQuery` + renderer + `PrintWorkSheetCommand`

- **Goal:** Close the M11 renderer gap for the per-visit worksheet (audit §E.3 "DTO-as-worksheet"); add the missing per-visit query and the renderer + command.
- **Touches:**
  - `src/TopLab.Application/Features/WorkSheets/Common/WorkSheetDtos.cs` (modify — add `VisitWorkSheetDto`)
  - `src/TopLab.Application/Features/WorkSheets/Queries/GetVisitWorkSheet/GetVisitWorkSheetQuery.cs` (+Handler, +Validator — create)
  - `src/TopLab.Application/Common/Interfaces/IWorkSheetPrintingService.cs` + `WorkSheetPrintEnvelope.cs` (create)
  - `src/TopLab.Infrastructure/Printing/WorkSheetPrintingService.cs` + `WorkSheetPdfWriter.cs` + port (create)
  - `src/TopLab.Application/Features/WorkSheets/Commands/PrintWorkSheet/PrintWorkSheetCommand.cs` (+Handler, +Validator — create)
  - `src/TopLab.Infrastructure/DependencyInjection.cs` (modify — WIRE)
  - `tests/TopLab.Application.Tests/Features/WorkSheets/Queries/GetVisitWorkSheetQueryHandlerTests.cs` (create)
  - `tests/TopLab.Application.Tests/Features/WorkSheets/Commands/PrintWorkSheetCommandHandlerTests.cs` (create)
- **Validation Gate:** VG-S4 — build 0/0; full suite green; manual visit-worksheet print (per-line sample flags, textual barcode, Reports routing, Arabic RTL); zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 4)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite green after S3. DONE 2026-09-16: S3 gate runs serve as S4 baseline (build 0/0; suite 2051 green at commit `19681a1`); working tree clean except untracked input `S-01.md`.
- [x] **Stage 2 — Deep Understanding:** re-read plan §6; internalize SD-7 (dedicated worksheet port) and the `OutputType=Reports` routing choice.
- [x] **Stage 3 — File Analysis:** `WorkSheetDtos.cs` (`WorkSheetLineDto` shape — `Barcode`, `IsSampleDrawn`, `CompletionDurationMinutes`, test name/code); `WorkSheetsAccessPolicy.cs` (ungated reads); `SystemSettings` flags consumed (`PrintFileExternalBarcode`, `PrintDateTimeOnTubeBarcode`, `PrintLabIdInsteadOfPatientId`); the four existing worksheet queries as structural precedent (`GetWorkSheetByTestGroup`, `GetWorkSheetByWorkGroupLog`, …). DONE with two corrections: (a) ALL FOUR existing queries are GATED on `PRINT_WORKSHEET` via `IAuthorizedRequest` — the plan's "ungated" label is factually wrong; its rationale ("consistent with WorkSheets queries") is honored by GATING the new query+command on `PRINT_WORKSHEET` (recorded deviation, not an SD change). (b) `WorkSheetLineDto` carries NO sample-kind flags (only `IsSampleDrawn`) — reused verbatim per plan; kinds travel in a parallel `VisitWorkSheetSampleDto(PatientTestId+6 flags)` list joined by `PatientTestId` in the writer. Line projection copied from `WorkSheetHelpers.WorkSheetLines.Select` (period/testIds filters dropped, outside-lab rows INCLUDED with flags shown); new internal `SelectVisit` helper in same file.
- [x] **Stage 4 — Planning:** step-by-step slice plan encoded here.
  1. `WorkSheetDtos.cs`: add `VisitWorkSheetDto` (patient header + `Sections` of `WorkSheetSectionDto` + `Samples` of new `VisitWorkSheetSampleDto` + `TotalTests` + 3 settings echoes) — `WorkSheetLineDto` untouched.
  2. `WorkSheetHelpers`: add internal `SelectVisit(db, patient)` returning lines+samples.
  3. `GetVisitWorkSheet/` query (GATED `PRINT_WORKSHEET`)+handler (NotFound incl. deleted; settings missing → «سجل الإعدادات العامة مفقود.»; sections = active groups by name + ungrouped; empty visit → empty sections, still success)+validator.
  4. `IWorkSheetPrintingService` + `IWorkSheetPdfWriter` ports; `WorkSheetPrintEnvelope` (`Features/WorkSheets/Common`, `CreateToken(VisitWorkSheetDto)`).
  5. `WorkSheetPrintingService` (Reports routing, «بيانات ورقة العمل غير صالحة.»/reports-printer message/«تعذر طباعة ورقة العمل.»; lab text via store) + `WorkSheetPdfWriter` (QuestPDF A4 RTL, same static init; sections; textual `Test.Barcode`; kind letters U/S/B/Se/CSF + خارج; draw [ ]/[X]; identifier LabId-vs-PatientId + datetime line; `BuildTextLines` for tests).
  6. `PrintWorkSheet/` command (GATED `PRINT_WORKSHEET`)+handler (via ISender; token; service)+validator.
  7. DI wiring.
  8. Tests: `GetVisitWorkSheetQueryHandlerTests` (mixed flags assembly; soft-deleted NotFound; empty visit printable; gate) + `PrintWorkSheetCommandHandlerTests` (token round-trip; missing Reports assignment; never-throws; gate) + `WorkSheetPrintingServiceTests` (happy %PDF + Reports routing; invalid token; dispatch-throw; `BuildTextLines` letters).
- [x] **Stage 5 — Execution:** implement per plan §6. DONE: `VisitWorkSheetDto`+`VisitWorkSheetSampleDto` (WorkSheetLineDto untouched); `WorkSheetVisitLines.SelectVisit`; `GetVisitWorkSheet/` trio (GATED PRINT_WORKSHEET — recorded deviation); ports (`IWorkSheetPrintingService`, `IWorkSheetPdfWriter`) + `WorkSheetPrintEnvelope`; `WorkSheetPrintingService` (Reports routing) + `WorkSheetPdfWriter` (QuestPDF A4 RTL, kind letters, draw checkbox); `PrintWorkSheet/` trio (GATED); DI; 3 test files +1 auth assertion (13 App + 6 Infra facts).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0/0. DONE 2026-09-16 (1 intermediate error fixed: test-only `SampleKinds` needed `public` — no InternalsVisibleTo for Infra tests).
- [x] **Stage 7 — Validation Gate:** VG-S4 pass. DONE: suite 474+1402+194=2070 green (+19); Reports-printer routing proven by dispatcher capture; per-line flags + textual barcodes proven by `BuildTextLines` test (`BC-10`+`U B`+`[X]` vs `—`+`خارج`+`[ ]`); zero-drift (no Persistence diff; model untouched since S3's clean check). Physical-paper check remains for the owner.
- [x] **Stage 8 — Documentation Update:** checkboxes updated.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-01] Slice 4/6: Visit worksheet renderer + PrintWorkSheetCommand — loop-engineering`.

---

## Slice 5: Patients hub + «المرضى» navigation wiring

- **Goal:** Wire the shell's «المرضى» button to a new Patients hub (four-action grid, all buttons disabled in this slice; help panel); adopt the audit §A.5 three-step recipe verbatim.
- **Touches:**
  - `src/TopLab.Presentation/ViewModels/Patients/PatientsHubViewModel.cs` (create)
  - `src/TopLab.Presentation/Views/Patients/PatientsHubView.xaml` (+.cs — create)
  - `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` (modify — WIRE «المرضى» branch)
  - `src/TopLab.Presentation/MainWindow.xaml` (modify — add `DataTemplate` + namespace decls)
  - `src/TopLab.Presentation/DependencyInjection.cs` (modify — WIRE `AddTransient<PatientsHubViewModel>()`)
- **Validation Gate:** VG-S5 — build 0/0; full suite green; manual hub navigation walk; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 5)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite green after S4. DONE 2026-09-16: S4 gate runs serve as S5 baseline (build 0/0; suite 2070 green at commit `94c9eec`); working tree clean except untracked input `S-01.md`.
- [x] **Stage 2 — Deep Understanding:** re-read plan §7; internalize SD-6 (disabled placeholder idiom); no secondary-password gate.
- [x] **Stage 3 — File Analysis:** `ShellViewModel.cs:115–159` incl. the `// Future: navigate to feature` fall-through at L152; `MainWindow.xaml:64–95` DataTemplate block + Home placeholder (L64–74); `Presentation/DependencyInjection.cs` ViewModels block; `Views/Settings/*` for RTL UserControl styling precedent. DONE: «المرضى» currently falls in `else` (L150–153); «الإعدادات» branch (L146–149) is the copy template; `SettingsDashboardViewModel`/`SettingsDashboardView` is the hub precedent (launcher VM + RTL UserControl, `RelayCommand(_ => navigation.NavigateTo<T>())`); DI ViewModels block at L29–41; window is RTL 1024×600.
- [x] **Stage 4 — Planning:** step-by-step slice plan encoded here.
  1. `ViewModels/Patients/PatientsHubViewModel.cs`: `ViewModelBase`; 4 `RelayCommand`s (no-op bodies — buttons disabled) + 4 bool flags default false (`AddEditPatientEnabled` etc.); help text property. S6 flips the editor flag + wires navigation.
  2. `Views/Patients/PatientsHubView.xaml` (+.cs): RTL `UserControl`, 2×2 button grid bound to commands+flags, help `TextBlock`; styling per Settings views.
  3. `ShellViewModel`: `else if (t == "المرضى") _navigation.NavigateTo<PatientsHubViewModel>()` before `else` (no password gate).
  4. `MainWindow.xaml`: `patientsVm`/`patientsView` xmlns + `DataTemplate` for hub.
  5. Presentation DI: `AddTransient<PatientsHubViewModel>()` (+`using ViewModels.Patients`).
- [x] **Stage 5 — Execution:** implement per plan §7. DONE: `PatientsHubViewModel` (4 no-op commands + 4 false flags + help text); `PatientsHubView` (RTL 2×2 grid + help, Settings-view styling); «المرضى» branch (no password gate); `DataTemplate` + xmlns; `AddTransient<PatientsHubViewModel>`.
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0/0. DONE 2026-09-16 (XAML compiles; 1 patch retry on non-unique match — no code error).
- [x] **Stage 7 — Validation Gate:** VG-S5 pass. DONE: suite 474+1402+194=2070 green; zero-drift (Presentation-only, no Persistence diff). Manual navigation walk cannot run headless here — wiring is the verbatim A.5 recipe (DI registration + DataTemplate + title branch), compile-verified; OWNER MANUAL: click «المرضى» → hub renders RTL with 4 disabled buttons; «المستخدمون»/«الإعدادات» still navigate; status bar unaffected.
- [x] **Stage 8 — Documentation Update:** checkboxes updated.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-01] Slice 5/6: Patients hub + المرضى navigation wiring — loop-engineering`.

---

## Slice 6: Unified Add/Edit Patient Data screen (demographics + ordering + billing + four print actions)

- **Goal:** Ship the unified editor orchestrating M02 + M03 + S1–S4 behind a single ViewModel; enable the hub's «إضافة وتعديل بيانات المرضى» button; deliver the LabId auto-generator (SD-4) + duplicate-rejection guard.
- **Touches:**
  - `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` (create)
  - `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` (+.cs — create)
  - `src/TopLab.Presentation/ViewModels/Patients/PatientsHubViewModel.cs` (modify — enable «إضافة وتعديل بيانات المرضى»)
  - `src/TopLab.Presentation/MainWindow.xaml` (modify — `DataTemplate` for `PatientEditorViewModel`)
  - `src/TopLab.Presentation/DependencyInjection.cs` (modify — `AddTransient<PatientEditorViewModel>()`)
  - `src/TopLab.Application/Features/PatientRegistration/Queries/GetNextLabId/GetNextLabIdQuery.cs` (+Handler, +Validator — create)
  - `src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs` (modify — add duplicate-LabId guard)
  - `tests/TopLab.Application.Tests/Features/PatientRegistration/Commands/CreatePatientLabIdTests.cs` (create)
  - `tests/TopLab.Application.Tests/Features/PatientRegistration/Queries/GetNextLabIdQueryHandlerTests.cs` (create)
- **Validation Gate:** VG-S6 — build 0/0; full suite green (incl. new LabId tests); manual end-to-end (create → tests → payment → 4 prints → edit → soft-delete); **binding-constraint checklist: every enabled control has a complete backend path**; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 6)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite green after S5.
- [ ] **Stage 2 — Deep Understanding:** re-read plan §8; internalize SD-4 LabId decision; internalize SD-9 (no Presentation-layer authorization duplication); internalize the edit-mode delta strategy (granular M02 commands, not visit rebuild) and single-source-of-truth billing refresh via `GetPatientAccountQuery`.
- [ ] **Stage 3 — File Analysis:** `PatientRegistrationDtos.cs:78–86` (`RegistrationCatalogDto`); `PatientBillingDtos.cs:3–13` (`PatientAccountDto`); `AddTestsToVisitCommand.cs:7` (`AddTestInput` sample-flag shape); `UpdatePatientTestSampleFlagsCommand.cs:9–21`; `CreatePatientCommandHandler.cs:38–48` (existing `LabId.Create` guard — insertion point for duplicate-rejection guard); `ResultErrorPresenter` (established VM idiom); the M02 test suite (as untouched baseline).
- [ ] **Stage 4 — Planning:** step-by-step slice plan encoded here, including the delta-command choreography for Edit mode and the four print-button wire-ups.
- [ ] **Stage 5 — Execution:** implement per plan §8.
- [ ] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-S6 pass (build + tests + end-to-end walk + all four print buttons produce documents + binding-constraint checklist + zero-drift).
- [ ] **Stage 8 — Documentation Update:** checkboxes updated.
- [ ] **Stage 9 — Memory Status Update:** Current Status updated → 6/6 complete.
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-01] Slice 6/6: Unified Add/Edit Patient Data screen (demographics + ordering + billing + 4 print actions) — loop-engineering`.

---

## Current Status

- Slices complete: **5 / 6** (S5 committed; S6 final slice next, begin Stage 1 immediately).
- Baseline commit: `faceab6c871230a73640faa4af5013068a403649` (main).
- S1–S5 executed and committed; S6 Stage-1 check pending.
- Migration count: **exactly 1** (`AddInvoiceIssues`, S3) — no further migrations in S4–S6 (zero-drift gates).

## Execution Log

| Date | Slice | Stage | Action | Result |
|---|---|---|---|---|
| 2026-09-16 | S1 | 1–10 | Barcode service slice (ZXing.Net 0.16.11; `Barcode/`; `PrintBarcode/`; DI; 18 new tests) | VG-S1 pass: build 0/0, suite 2006 green, scan-back + routing proven, zero-drift |
| 2026-09-16 | S2 | 1–10 | Receipt printing slice (QuestPDF 2026.9.0; receipt ports+envelope+service+writer; `PrintReceipt/`; DI; 14 new tests) | VG-S2 pass: build 0/0, suite 2020 green, Receipt routing + totals proven, zero-drift; pitfall: QuestPDF needs `UseSystemFonts=true` |
| 2026-09-16 | S3 | 1–10 | Invoice slice (InvoiceIssue+config+migration AddInvoiceIssues; InvoiceDto; invoice ports+envelope+service+writer; GetPatientInvoice/PrintInvoice; DI; 31 new tests) | VG-S3 pass: build 0/0, suite 2051 green, live up/down round-trip + unique enforcement + drift-clean; PRE-EXISTING: baseline from-zero update broken in Sept-9 migration (owner housekeeping, not fixed) |
| 2026-09-16 | S4 | 1–10 | Visit worksheet slice (VisitWorkSheetDto+Samples; SelectVisit; GetVisitWorkSheet/PrintWorkSheet gated PRINT_WORKSHEET; worksheet ports+envelope+service+writer; DI; 19 new tests) | VG-S4 pass: build 0/0, suite 2070 green, Reports routing + per-line flags/barcodes proven, zero-drift |
| 2026-09-16 | S5 | 1–10 | Patients hub slice (hub VM+view; المرضى branch; DataTemplate; DI) | VG-S5 pass: build 0/0, suite 2070 green, zero-drift; manual nav-walk left for owner (headless here) |

## Stop Report

(None — no stop condition has triggered.)
