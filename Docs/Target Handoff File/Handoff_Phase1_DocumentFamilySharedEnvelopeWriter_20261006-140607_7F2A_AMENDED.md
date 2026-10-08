# Phase Handoff — Phase 1: Document family: the shared envelope writer

---

## A. Phase Identity

- Handoff ID: `PH1-ENVELOPE-20261006-7F2A`
- Exact filename: `Handoff_Phase1_DocumentFamilySharedEnvelopeWriter_20261006-140607_7F2A.md`
- Exact path: `Docs/Target Handoff File/Handoff_Phase1_DocumentFamilySharedEnvelopeWriter_20261006-140607_7F2A.md`
- Creation timestamp (UTC): 2026-10-06 14:06:07
- Phase number: 1
- Phase title: Document family: the shared envelope writer
- Number of functions in the phase: 5
- Plan revision: 2
- Planning status: `PHASE_PLANNING_READY_AUTONOMOUS_DECISIONS`
- Revision note: Revision 2 — removes all non-plan content; the Handoff is now a
  pure work-plan document, in conformance with the Planning Framework.

---

## B. Baseline (captured once, before any investigation began)

- Repository root: `C:\Users\LAP LINK\source\repos\Top-Lab`
- Branch: `main` (tracks `origin/main`, up to date)
- HEAD: `7cd2585e4cc2c1fbee9e5f1a50594f2f8563b858`
  (`chore: add Target Handoff File directory for phase planning handoffs`)
- Working-tree state at planning start: **clean** —
  `git status` = `nothing to commit, working tree clean`; staged: none;
  unstaged: none; untracked (excl. standard ignores): none.
- Solution: `TopLab.sln` — projects `TopLab.Domain`, `TopLab.Application`,
  `TopLab.Infrastructure`, `TopLab.Presentation` (all `net8.0`),
  plus `TopLab.Domain.Tests`, `TopLab.Application.Tests`,
  `TopLab.Infrastructure.Tests`, `TopLab.Presentation.Tests`,
  `TopLab.Persistence.Tests`.
- Installed .NET SDKs: `8.0.425`, `9.0.318` (project targets `net8.0`).
- EF Core version: `8.0.30` (all EF packages; do NOT move to 9.x/10.x per
  `Directory.Packages.props`). Licensed pins respected:
  MediatR `12.5.0`, ZXing.Net `0.16.11`, QuestPDF `2026.9.0`,
  FluentValidation `12.1.1`, xUnit `2.9.3`.
- Migration state: 14 migrations under
  `src/TopLab.Infrastructure/Persistence/Migrations/`;
  actual latest migration: `20261002002546_AddAntibioticMasterFields`
  (`.cs` + `.Designer.cs`); model snapshot:
  `.../Migrations/ApplicationDbContextModelSnapshot.cs` (contains
  `EnvelopeSettings`, `EnvelopePrintItemPositions`, `PrinterAssignments`
  incl. the `Envelope` row, `SystemSettings` barcode flags).
- Baseline signature/manifest: HEAD `7cd2585`, clean tree, 14 migrations,
  snapshot as above, `Docs/Target Handoff File/` containing only `.gitkeep`.
- Known pre-existing changes: none (tree clean).
- Historical-baseline note: this baseline is **historical** once the build
  agent creates commits; `git rev-parse HEAD` — not the hash above — is the
  truth for current state. If HEAD has moved to an incompatible state or the
  files cited below no longer match, treat the plan as stale.

---

## C. Phase Overview

### Purpose and grouping

Phase 1 builds the patient-envelope document family end to end. Today the
envelope settings stack (entity, EF configuration, seed, editor UI) exists but
is **stored-but-inert**: no document consumes it, no envelope writer exists,
no live UI path prints an envelope, and the receipt/invoice writers emit
identifier text only (no scannable barcode). The five functions together turn
the inert settings into a live document family sharing one writer, one
barcode-image pipeline, and one identifier rule:

1. REF-065 wires/validates the existing envelope settings as a consumable
   contract (no schema change).
2. REF-066 creates the shared envelope writer + printing service + live
   results-screen trigger (owns all shared infrastructure).
3. REF-067 draws the patient barcode on the envelope (consumes REF-066).
4. REF-068 creates the laboratory order (requisition) slip with barcode
   (consumes REF-066's barcode pipeline).
5. REF-127 adds scannable barcodes to the receipt and invoice writers and
   verifies the envelope barcode (consumes REF-066/067, extends receipt path).

### Ordered function list

| # | REF | Name | Status | Depends on |
|---|---|------|--------|------------|
| 1/5 | REF-065 | Envelope settings | DIFFERENT | — |
| 2/5 | REF-066 | Print the patient envelope | MISSING | REF-065 |
| 3/5 | REF-067 | Patient barcode printed on the envelope | MISSING | REF-066, REF-037 |
| 4/5 | REF-068 | Print a laboratory order with barcode | MISSING | REF-066 |
| 5/5 | REF-127 | Barcode printed on the receipt and the envelope | MISSING | REF-066 |

REF-037 (standalone tube/label barcode) is a prior-phase dependency used only
as the proven renderer (`BarcodeLabelRenderer` + `IBarcodeService`); this
phase does not modify it.

### Phase-level migration summary

- `NO_MIGRATION_REQUIRED`: 5 (REF-065, REF-066, REF-067, REF-068, REF-127)
- `NEW_MIGRATION_REQUIRED`: 0
- `MIGRATION_DECISION_BLOCKED`: 0
- Matches the TASK INPUT expectation (5 / 0). No migration sequence exists;
  Section E records the empty sequence explicitly.

### Cross-function dependencies inside the phase

- REF-066 owns ALL shared infrastructure (envelope DTO + token envelope +
  writer + service + PNG encoder + identifier helper + results-screen
  trigger). REF-067, REF-068, REF-127 consume it; none re-implements it.
- REF-067 has no independent writer work; it completes the Code-item barcode
  inside REF-066's writer (specified as acceptance-level behavior of the
  writer, verified in REF-067).
- REF-068 reuses REF-066's `BarcodePngEncoder` and identifier helper.
- REF-127 reuses the identifier helper for receipt/invoice; its envelope half
  is verification of REF-066/067 output, not new writer code.

### Shared infrastructure (created once, consumed by several)

Owner in all cases: **REF-066**. Consumers noted per item:

- `IEnvelopePrintingService` / `EnvelopePrintingService` → consumed by
  REF-066 trigger, REF-067 (no change), REF-127 verification.
- `IEnvelopePdfWriter` / `EnvelopePdfWriter` (incl. pure `BuildTextLines` /
  `EnvelopeTextLines` mapping) → consumed by REF-067 (Code barcode branch)
  and REF-127 (envelope half).
- `BarcodePngEncoder` (RGBA → PNG bytes, no new packages) → consumed by
  REF-067, REF-068, REF-127.
- `BarcodePayload.For(patientId, labId, printLabIdInstead)` (Application-layer
  pure helper; identifier rule in one place) → consumed by REF-066, REF-068,
  REF-127. (Owned by REF-066; specified in D.4 of REF-066.)
- `EnvelopePrintEnvelope` token record → consumed by REF-066 handler/service.
- `PrintEnvelopeCommand` + results-screen `طباعة ظرف` button → REF-066;
  verified live by REF-066 acceptance, not re-touched later.

---

## D. Per-Function Sections

---

### D-1. REF-065 — Envelope settings (Function 1 of 5)

#### D.1 Function identity

- REF-ID: REF-065. Name: Envelope settings. Audit status: DIFFERENT.
- Position: Function 1 of 5. Dependencies on earlier phase functions: none.
- Pre-identified BLOCKED flag: NONE.

#### D.2 Investigation findings (Claim → Evidence → Exact Location → Interpretation → Confidence)

1. Claim: The settings entity, EF configuration, migration seed, and editor
   UI all exist.
   Evidence: `EnvelopeSettings` (single row PK=1, `TopMarginCm`,
   `HeaderFooterMode`, `SuppressCaptions`, `CreateDefault`, range-guarded
   `Update`); `EnvelopePrintItemPosition` (PK `ItemName`, `IsEnabled`,
   offsets, range-guarded `Update`); EF configurations with `HasData` seeds;
   baseline migration creates both tables with seeds; `EnvelopeSettingsViewModel`
   + `EnvelopeSettingsView.xaml` editor with top-margin/header-footer/
   suppress-captions/positions/lab-text/font controls and Save path via
   `GetEnvelopeSettingsQuery` + `UpdateEnvelopeSettingsCommand` +
   `SaveLabPrintTextCommand(Envelope)`.
   Exact Location:
   `src/TopLab.Domain/Settings/EnvelopeSettings.cs:7-43`,
   `src/TopLab.Domain/Settings/EnvelopePrintItemPosition.cs:4-41`,
   `src/TopLab.Infrastructure/Persistence/Configurations/EnvelopeSettingsConfiguration.cs:10-17`,
   `src/TopLab.Infrastructure/Persistence/Configurations/EnvelopePrintItemPositionConfiguration.cs:9-21`,
   `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs:57-81,807-818`,
   `src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateEnvelopeSettings/UpdateEnvelopeSettingsCommandHandler.cs:17-44`,
   `src/TopLab.Application/Features/SystemAndPrintSettings/Queries/GetEnvelopeSettings/GetEnvelopeSettingsQueryHandler.cs:20-46`,
   `src/TopLab.Presentation/ViewModels/Settings/EnvelopeSettingsViewModel.cs:120-227`,
   `src/TopLab.Presentation/Views/Settings/EnvelopeSettingsView.xaml:1-129`.
   Interpretation: the settings capability is fully stored and editable.
   Confidence: CONFIRMED.

2. Claim: NO document consumes the envelope settings; the capability is
   present but inert.
   Evidence: the only readers of `Set<EnvelopeSettings>` /
   `Set<EnvelopePrintItemPosition>` are the Get/Update handlers and the
   `ApplyDatabaseUpdates` seed-repair; no `*PdfWriter`, no
   `*PrintingService`, no ViewModel outside the settings editor references
   them; no `Envelope*PdfWriter`, `IEnvelopePrintingService`,
   `PrintEnvelope*`, `EnvelopePrint*` (document-side) symbol exists — every
   `*PrintEnvelope` hit is the unrelated token-envelope pattern
   (`ReportPrintEnvelope`, `ReceiptPrintEnvelope`, …).
   Exact Location: grep `Set<EnvelopeSettings>|Set<EnvelopePrintItemPosition>`
   (only `ApplicationDbContext.DbSets.cs:59-60` declarations plus the three
   readers above); grep `PrintEnvelope|EnvelopePdf|PrintLabOrder|Requisition`
   in `src/TopLab.Application` returns only token-envelope records;
   `src/TopLab.Infrastructure/Printing/` contains 23 files, none envelope.
   Interpretation: DIFFERENT = exists-but-unconsumed, exactly as TASK INPUT
   states. Confidence: CONFIRMED.

3. Claim: Schema already matches the data-model contract; seed rows exist in
   migration history and the snapshot.
   Evidence: snapshot maps `EnvelopeSettings` (PK `EnvelopeSettingsId`,
   `decimal(5,2)`, `tinyint` mode, seed Id=1/3.0cm/None/false) and
   `EnvelopePrintItemPositions` (PK `ItemName(50)`, 4 seeded rows
   Name/Code/ReferralEntity/Date at 1..4 cm tops) and `PrinterAssignments`
   (4 rows incl. `Envelope`→`"Envelope"`); `ApplyDatabaseUpdates` repairs any
   missing row at runtime (`EnsureSingleRow` + `EnsureMissingPositions` +
   `EnsureMissingPrinterAssignments`).
   Exact Location:
   `src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs:1028-1144`,
   `src/TopLab.Application/Features/SystemAndPrintSettings/Commands/ApplyDatabaseUpdates/ApplyDatabaseUpdatesCommandHandler.cs:37-43,64-99`.
   Interpretation: no schema/seed work remains. Confidence: CONFIRMED.

4. Claim: Tests cover the settings domain/validator/seed, but nothing asserts
   a document consuming the settings (correctly — none exists).
   Evidence: `EnvelopeSettingsTests`, `EnvelopePrintItemPositionTests`,
   `UpdateEnvelopeSettingsCommandValidatorTests`,
   `F5ConfigurationTests.EnvelopePrintItemPosition_SeedCount_Is4`,
   `SettingsFontFamilyDefaultTests.EnvelopeSettings_DefaultFontFamily_IsEmpty`.
   Exact Location: `tests/TopLab.Domain.Tests/Settings/EnvelopeSettingsTests.cs`,
   `tests/TopLab.Domain.Tests/Settings/EnvelopePrintItemPositionTests.cs`,
   `tests/TopLab.Application.Tests/Features/SystemAndPrintSettings/UpdateEnvelopeSettingsCommandValidatorTests.cs`,
   `tests/TopLab.Infrastructure.Tests/Persistence/Configurations/F5ConfigurationTests.cs:122-126`.
   Interpretation: coverage is real behavioural coverage for settings, not
   string-assertion theater; the gap is the consumer, owned by REF-066.
   Confidence: CONFIRMED.

5. Claim: The settings-screen barcode preview is a static fake, not a real
   barcode render.
   Evidence: the "معاينة الباركود (ثابت)" block renders literal text
   `"|| ||| |||| ||"` in Consolas inside a gray border.
   Exact Location: `EnvelopeSettingsView.xaml:83-88` (labelled ثابت = static
   by the codebase itself).
   Interpretation: cosmetic DIFFERENT vs PRD UI-31 "الكود (barcode preview)";
   intentionally left as-is this phase (OUT OF SCOPE, see D.3); the real
   scannable barcode is writer output (REF-067). Confidence: CONFIRMED.

#### D.3 Scope definition

IN SCOPE:

- Verify-and-keep: domain entities, EF configurations, seeds, handlers,
  validator, editor VM/view — no changes except items below.
- Define the consumer contract for REF-066: `EnvelopeSettingsDto`
  (`TopMarginCm`, `HeaderFooterMode`, `SuppressCaptions`, ordered positions
  Name/Code/ReferralEntity/Date) as read at print time (live read, no
  caching, per Reporting Blueprint §6 "read at print time").
- Verify `GetEnvelopeSettingsQuery` authorization posture against sibling
  `Get*Settings` queries and match it (see D.8 decision 65-A). If siblings
  are gated and this one is not, add `IAuthorizedRequest` with
  `EDIT_SYSTEM_SETTINGS`; if siblings are ungated reads, leave unchanged.
  (The Update command already declares `EDIT_SYSTEM_SETTINGS` —
  `UpdateEnvelopeSettingsCommand.cs:14-16` — CONFIRMED.)
- Add/extend tests ONLY for settings behavior touched above (validator +
  handler posture); no new test projects.

OUT OF SCOPE / non-goals:

- Any schema, migration, seed, or snapshot change (prohibited; schema
  sufficient — see D.5).
- The writer, service, token, commands, UI trigger (all REF-066).
- Replacing the static settings-screen barcode preview with a live render.
- Fixing `ReferralNameResolver`'s English Himself/Herself fallback (noted
  below; envelope uses the registration Arabic placeholders instead).
- Any change to report/receipt/worksheet writers or unrelated settings.

#### D.4 Implementation plan (Domain → Application → Infrastructure → Presentation → Tests → Migration)

1. Application — `src/TopLab.Application/Features/SystemAndPrintSettings/Queries/GetEnvelopeSettings/GetEnvelopeSettingsQuery.cs` (MODIFY, only if
   sibling-query verification requires): implement `IAuthorizedRequest` with
   `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"`. Else: no touch.
2. Tests — extend `UpdateEnvelopeSettingsCommandValidatorTests` style coverage
   for any posture change; assert `GetEnvelopeSettingsQueryHandler` still
   returns positions in canonical order Name/Code/ReferralEntity/Date.
3. Migration — none.

Shared-component note: REF-065 creates no shared component; it freezes the
`EnvelopeSettingsDto` shape that REF-066's service consumes.

#### D.5 Migration decision

`NO_MIGRATION_REQUIRED` — tables, keys, seeds, and the `Envelope`
printer-assignment row all exist in the baseline migration and snapshot
(D.2 items 1, 3). No migration is part of the work for REF-065.

#### D.6 Migration Contract

Not applicable (decision is `NO_MIGRATION_REQUIRED`).

#### D.7 Acceptance criteria

- `dotnet test` passes for settings suites; seed-count test still asserts 4
  positions.
- `GetEnvelopeSettingsQuery` posture matches sibling `Get*Settings` queries
  (verified by build agent, cited in its report).
- No migration files added; snapshot byte-identical to baseline.
- `EnvelopeSettingsDto` shape documented as the REF-066 consumer contract
  (this handoff).

#### D.8 Autonomous decisions made

- 65-A (autonomous decision): `GetEnvelopeSettingsQuery` currently lacks a
  permission grant while its Update counterpart declares
  `EDIT_SYSTEM_SETTINGS`. Options: (a) gate the read now; (b) leave and match
  siblings. Selected: verify-then-match — the build agent inspects sibling
  `Get*Settings` queries and mirrors the majority posture. Rationale: avoids
  inventing an inconsistent auth rule; standing rule governs *new*
  commands/queries (REF-066+ declare grants explicitly).
- 65-B (autonomous decision): static preview stays static. Options: (a) live
  ZXing→WPF preview now; (b) defer. Selected: defer (OUT OF SCOPE).
  Rationale: reference behavior for the *document* is writer output; a live
  settings preview needs a WPF bitmap pipeline with no existing pattern and
  adds risk without reference-mandated value.

#### D.9 Function-level status

`READY`

---

### D-2. REF-066 — Print the patient envelope (Function 2 of 5)

#### D.1 Function identity

- REF-ID: REF-066. Name: Print the patient envelope. Audit status: MISSING.
- Position: Function 2 of 5. Depends on earlier phase function: REF-065
  (consumes its settings contract).
- Pre-identified BLOCKED flag: NONE.

#### D.2 Investigation findings (Claim → Evidence → Exact Location → Interpretation → Confidence)

1. Claim: No envelope document, writer, service, command, or UI trigger
   exists anywhere.
   Evidence: `src/TopLab.Infrastructure/Printing/` has no `Envelope*` writer
   (23 files listed: ArabicFontResolver, BandedResultMonitor, CustomGroup,
   Invoice*, Report*, Receipt*, WorkSheet*, PriceList, dispatchers — no
   envelope); `DependencyInjection.cs` registers report/receipt/invoice/
   worksheet/price-list/custom-group/banded writers and services but no
   envelope pair; no `PrintEnvelopeCommand`, no `طباعة ظرف` button, no live
   caller of envelope settings.
   Exact Location: `src/TopLab.Infrastructure/Printing/` directory listing;
   `src/TopLab.Infrastructure/DependencyInjection.cs:63-99` (registrations,
   envelope absent); grep `PrintEnvelope|EnvelopePdf` in Application returns
   only token envelopes.
   Interpretation: full vertical slice must be created. Confidence: CONFIRMED.

2. Claim: The reference envelope is fully specified (content + trigger +
   routing).
   Evidence: PRD FR-OUT-05 (printed from results screen `طباعة ظرف`, assigned
   envelope printer, layout per settings incl. per-item cm-positioning for
   name/code/referral/date); FR-M04-005/UI-05 (results-entry window owns the
   `طباعة ظرف` button); FR-M22-010/UI-31 (top margin, header/footer
   none/words/images + logo slot, lab name/title blocks + fonts, per-item
   enable + Left/Top offsets, caption suppression); Reporting Blueprint §6
   matrix (settings→P-06), §10 (`Envelope`→P-06), §12 item 5 (each enabled
   item at its offsets), §7 placement (barcode on envelope when Code enabled).
   Exact Location: `Docs/Source/Top_Lab_PRD.md:209,370,518,544,640,665`;
   `Docs/Source/Top_Lab_Reporting_Printing_Blueprint.md:67,114,202,216,254,280`.
   Interpretation: envelope content = header (lab words) + positioned
   Name/Code(+barcode)/ReferralEntity/Date + top margin + optional captions.
   Confidence: CONFIRMED (docs corroborated by implemented settings shape).

3. Claim: A proven writer/service pattern exists to clone (never invent).
   Evidence: `ReceiptPrintingService` (token→settings→assignment→labText→
   writer→dispatcher, never throws, Arabic `Error.Unexpected`) and
   `ReceiptPdfWriter` (QuestPDF A5 RTL, `BuildTextLines` pure mapping,
   `HeaderFooterMode.Images`→words fallback with documented comment,
   `ArabicFontResolver`, `Settings.UseSystemFonts = true`); identical shape
   in `InvoicePrintingService`/`InvoicePdfWriter`; token pattern
   (`ReceiptPrintEnvelope.CreateToken`); worksheet writer documents the
   text-only barcode limitation explicitly.
   Exact Location:
   `src/TopLab.Infrastructure/Printing/ReceiptPrintingService.cs:25-98`,
   `src/TopLab.Infrastructure/Printing/ReceiptPdfWriter.cs:20-56,150-215`,
   `src/TopLab.Infrastructure/Printing/InvoicePrintingService.cs:19-85`,
   `src/TopLab.Application/Features/PatientBilling/Common/ReceiptPrintEnvelope.cs:12-18`,
   `src/TopLab.Infrastructure/Printing/WorkSheetPdfWriter.cs:12-18`.
   Interpretation: envelope writer/service mirrors receipt pair file-for-file
   in spirit, routed via `PrinterOutputType.Envelope`. Confidence: CONFIRMED.

4. Claim: All envelope data is already queryable; no new persistence needed.
   Evidence: `Patient` carries `FullName`, `LabId`, `TreatingDoctorId`,
   `ReferralEntityId`, `RegistrationDateUtc`, `Sex`
   (`Patient.cs:13-37`); referral display has `ReferralNameResolver` plus
   registration-side Arabic sex-based placeholders
   (`ReferralPlaceholderMale/Female`, `PatientRegistrationDtos.cs:85-86`);
   identifier rule precedent (`PrintLabIdInsteadOfPatientId && LabId → LabId
   else PatientId`) in `PrintBarcodeCommandHandler.cs:40-42`; lab header via
   `ILabPrintTextStore.GetAsync(Envelope)` (workstation-local JSON, no DB —
   `ILabPrintTextStore.cs:6-22`).
   Interpretation: envelope DTO is computed at print time from existing rows.
   Confidence: CONFIRMED (placeholder wiring detail: INFERRED, see D.8).

5. Claim: The live trigger home is the results screen, which currently has no
   envelope button.
   Evidence: results screen = `PatientResultSheetView` (action row today:
   only `BulkPrintCommand`/`ExportPdfCommand`) backed by
   `PatientResultSheetViewModel` (Load/Export/BulkPrint only); PRD UI-05
   requires `طباعة ظرف` among its buttons; permission precedent for
   results-screen print actions = `ResultsEntryAccessPolicy.PrintResults`
   (used by `MarkResultPrinted`, `ExecuteBulkPrint`, `ExportPatientReportPdf`).
   Exact Location: `PatientResultSheetView.xaml:82-88`,
   `PatientResultSheetViewModel.cs:42-52,97-99`,
   `ResultsEntry/Common/ResultsEntryAccessPolicy.cs`,
   `MarkResultPrintedCommand.cs:10`, `ExecuteBulkPrintCommand.cs:12`.
   Interpretation: add `PrintEnvelopeCommand` + `طباعة ظرف` button here.
   Confidence: CONFIRMED.

6. Claim: QuestPDF image embedding is unproven in this codebase.
   Evidence: grep `\.Image\(|ImageData` across `src/` returns zero hits; all
   writers are text/vector only; the only bitmap pipeline is
   `BarcodeLabelRenderer` (ZXing → RGBA `PixelData`) feeding a hand-rolled
   PDF label (`BarcodeService.BuildLabelPdf`), never QuestPDF.
   Exact Location: `BarcodeLabelRenderer.cs:12-30`,
   `BarcodeService.cs:94-175`, negative grep result.
   Interpretation: barcode-in-QuestPDF needs a small new bridge + mandatory
   compile/visual verification with a text fallback. Confidence: CONFIRMED
   (gap) / INFERRED (API availability — QuestPDF `.Image(byte[])` PNG path).

#### D.3 Scope definition

IN SCOPE:

- Application: `Features/PatientEnvelope/` folder — `Common/EnvelopeDtos.cs`
  (`EnvelopeDto`: PatientId, FullName, Identifier, ReferralDisplayName,
  DateUtc, LabId?), `Common/EnvelopePrintEnvelope.cs` token record,
  `Common/BarcodePayload.cs` pure helper
  (`For(patientId:int, labId:string?, printLabIdInstead:bool):string`),
  `Commands/PrintEnvelope/PrintEnvelopeCommand` + Validator + Handler.
  Handler: loads `Patient` (NotFound if null/deleted), reads `SystemSettings`
  identifier flag, resolves referral display, builds DTO, creates token,
  calls `IEnvelopePrintingService`; permission
  `ResultsEntryAccessPolicy.PrintResults`.
- Application interfaces: `IEnvelopePdfWriter`, `IEnvelopePrintingService`
  in `Common/Interfaces/` (mirroring `IReceiptPdfWriter` /
  `IReceiptPrintingService` XML-doc style).
- Infrastructure: `Printing/EnvelopePdfWriter.cs` (QuestPDF; top margin cm→pt;
  header words from `LabPrintTextDto` when mode != None; Images→words
  fallback with documented comment; each enabled position drawn at
  Left/Top cm offsets; `SuppressCaptions` hides captions; Date line
  `yyyy-MM-dd HH:mm` InvariantCulture; pure static `BuildTextLines` returning
  `EnvelopeTextLines` incl. `BarcodePayload` string for the Code item),
  `Printing/EnvelopePrintingService.cs` (live-read `EnvelopeSettings` +
  positions + `PrinterAssignment(Envelope)` + `ILabPrintTextStore(Envelope)`;
  never throws; Arabic Unexpected errors),
  `Barcode/BarcodePngEncoder.cs` (static RGBA→PNG encoder, no new packages;
  used with `BarcodeLabelRenderer.Render(payload)`).
- DI: register writer + service + renderer usage in
  `Infrastructure/DependencyInjection.cs`; register VM command path in
  `Presentation/DependencyInjection.cs` only if the established per-VM
  pattern requires it.
- Presentation: `PrintEnvelopeCommand` (AsyncRelayCommand) on
  `PatientResultSheetViewModel` + `طباعة ظرف` button in
  `PatientResultSheetView.xaml` action row; error via `ResultErrorPresenter`.
- Tests: handler (NotFound/deleted/missing-settings/success-token),
  validator, `BuildTextLines` mapping (positions on/off, captions on/off,
  header modes), service failure paths (missing settings/assignment/labText),
  PNG encoder signature + determinism, DI registration presence,
  dispatch-token assertion like `PrintWorkSheetCommandHandlerTests`.

OUT OF SCOPE / non-goals:

- Schema/migration/seed/snapshot changes of any kind.
- Drawing the scannable barcode image itself as a *verified visual* (image
  bytes wired here; scannable-image acceptance owned by REF-067).
- Lab order slip (REF-068), receipt/invoice barcode (REF-127).
- Header/footer *image files* pipeline (no asset pipeline exists; words
  fallback stands, mirroring receipt precedent).
- Online/portal/SMS/e-mail/fax delivery; multi-branch; patient-card printing
  (all excluded per Blueprint §11 — must not appear).
- Touching report/receipt/worksheet writers or unrelated ViewModels.

#### D.4 Implementation plan (Domain → Application → Infrastructure → Presentation → Tests → Migration)

1. Domain — no changes (all inputs exist).
2. Application —
   a. `Features/PatientEnvelope/Common/EnvelopeDtos.cs` (CREATE):
      `EnvelopeDto`, position DTO reuse (`EnvelopeSettingsDto` from
      SystemAndPrintSettings, no duplicate).
   b. `Features/PatientEnvelope/Common/EnvelopePrintEnvelope.cs` (CREATE):
      token record mirroring `ReceiptPrintEnvelope`.
   c. `Features/PatientEnvelope/Common/BarcodePayload.cs` (CREATE, shared
      owner): `public static string For(int patientId, string? labId,
      bool printLabIdInstead)` — LabId when flag && non-empty else
      InvariantCulture PatientId.
   d. `Features/PatientEnvelope/Commands/PrintEnvelope/PrintEnvelopeCommand.cs`
      (CREATE): `IAuthorizedRequest`, `PrintResults` grant.
   e. `.../PrintEnvelopeCommandValidator.cs` (CREATE): PatientId > 0.
   f. `.../PrintEnvelopeCommandHandler.cs` (CREATE): flow per D.3; referral
      via ExternalEntities name lookup, fallback per D.8 decision 66-D.
3. Infrastructure —
   a. `Printing/EnvelopePdfWriter.cs` (CREATE) + `IEnvelopePdfWriter`
      port; static `BuildTextLines(EnvelopeDto, EnvelopeSettingsDto,
      LabPrintTextDto)` pure mapping (Code item carries
      `BarcodePayload.For(...)` output string; image rendering in
      `WritePdfAsync` only).
   b. `Printing/EnvelopePrintingService.cs` (CREATE) + port; Envelope
      printer routing; temp PDF via `Path.GetTempPath()` + GUID (receipt
      precedent); never throws.
   c. `Barcode/BarcodePngEncoder.cs` (CREATE, shared owner): RGBA→PNG bytes;
      deterministic; throws on invalid input (writers/services map to
      `Error.Unexpected`).
   d. `DependencyInjection.cs` (MODIFY): scoped writer + service.
4. Presentation — `PatientResultSheetViewModel.cs` (MODIFY: command),
   `PatientResultSheetView.xaml` (MODIFY: `طباعة ظرف` button).
5. Tests — Application handler/validator tests; Infrastructure writer-mapping
   + service + encoder tests; Presentation VM command test if the suite
   pattern covers VM commands.
6. Migration — none.

Shared-component note: this function CREATES every shared item in Section C;
later functions consume without re-implementation.

#### D.5 Migration decision

`NO_MIGRATION_REQUIRED` — envelope content derives from existing tables
(`Patient`, `EnvelopeSettings`, `EnvelopePrintItemPositions`,
`PrinterAssignment`, `SystemSettings`, `ExternalEntity`) plus workstation-local
lab text. No migration is part of the work for REF-066.

#### D.6 Migration Contract

Not applicable (decision is `NO_MIGRATION_REQUIRED`).

#### D.7 Acceptance criteria

- Clicking `طباعة ظرف` on a loaded result sheet with a configured envelope
  printer produces a PDF dispatched to the Envelope printer; errors surface
  as Arabic non-technical messages, never exceptions.
- Rendered envelope shows lab header words (mode Words), positioned
  Name/Code/Referral/Date lines per offsets, top margin honored, captions
  hidden when `SuppressCaptions`.
- `HeaderFooterMode.Images` falls back to words with an in-code documented
  comment (receipt precedent).
- New command declares its permission grant; validator rejects PatientId ≤ 0.
- No migration added; no other writer/VM modified.

#### D.8 Autonomous decisions made

- 66-A (autonomous): barcode→QuestPDF bridge = hand-rolled RGBA→PNG encoder
  in `Barcode/BarcodePngEncoder`, zero new NuGet packages. Options: (a) new
  imaging package; (b) hand encoder; (c) reuse `BuildLabelPdf` manual-PDF
  path inside QuestPDF (impossible — QuestPDF owns the page). Selected (b).
  Rationale: licensed-package pins forbid casual additions; ZXing+QuestPDF
  already approved; PNG encoding is small, deterministic, testable.
  Verification mandated: build agent compiles `.Image(pngBytes)` against
  QuestPDF 2026.9.0; if the API shape differs, adapt the call (same design)
  and record; if image embedding proves unviable, Code item falls back to
  human-readable text + records `READY_AUTONOMOUS_DECISIONS_RECORDED`
  (never silently text-only — must be explicit).
- 66-B (autonomous): envelope paper = A5 portrait RTL (receipt precedent),
  NOT a DL-envelope stock size. Options: A5 / DL-C5 stock / letter-size
  positioning sheet. Selected A5. Rationale: codebase has exactly two proven
  QuestPDF page setups (A5 receipt/invoice, A4 worksheet/report); DL stock
  metrics are absent from all reference docs; offsets are cm-absolute so the
  layout ports to stock later without redesign. Flag: autonomous.
- 66-C (autonomous): `PrintEnvelopeCommand` permission =
  `ResultsEntryAccessPolicy.PrintResults`. Options: PrintResults /
  EDIT_SYSTEM_SETTINGS / new grant. Selected PrintResults. Rationale: a
  results-screen print action beside BulkPrint/Export/MarkPrinted, all gated
  identically; system-settings grant would wrongly require admin for a
  cashier/clerk print.
- 66-D (INFERRED_OWNER_DECISION): referral line source. `Patient` holds only
  `ReferralEntityId`; display name comes from the ExternalEntity lookup;
  when absent, use the registration Arabic sex-based placeholder
  (`ReferralPlaceholderMale/Female` in `PatientRegistrationDtos.cs:85-86`).
  The build agent MUST trace that DTO's query source and reuse the identical
  value; if untraceable, omit the referral line when no entity exists and
  record the omission. Rationale: `ReferralNameResolver`'s Himself/Herself
  fallback is English, violating the no-English product rule.
- 66-E (autonomous): envelope Date item = `Patient.RegistrationDateUtc`
  rendered `yyyy-MM-dd HH:mm` InvariantCulture. Options: registration date /
  print-now date. Selected registration date. Rationale: envelope identifies
  the *visit* (name/code/referral/date quad); print timestamp belongs to tube
  barcodes (`PrintDateTimeOnTubeBarcode`) and report headers, not the
  envelope identity block.

#### D.9 Function-level status

`READY_AUTONOMOUS_DECISIONS_RECORDED`

---

### D-3. REF-067 — Patient barcode printed on the envelope (Function 3 of 5)

#### D.1 Function identity

- REF-ID: REF-067. Name: Patient barcode printed on the envelope. Audit
  status: MISSING. Position: Function 3 of 5.
- Depends on earlier phase functions: REF-066 (writer + encoder + trigger).
  External: REF-037 (proven `BarcodeLabelRenderer`/tube-label path — consumed
  read-only, not modified).
- Pre-identified BLOCKED flag: NONE.

#### D.2 Investigation findings (Claim → Evidence → Exact Location → Interpretation → Confidence)

1. Claim: No envelope document exists, so no envelope barcode exists; the
   barcode renderer is reachable only as a standalone label and as plain
   text on the worksheet.
   Evidence: D-2/REF-066 item 1 (no envelope writer); `BarcodeService`
   (standalone 4×2in label via `PrinterOutputType.Barcode`) invoked only by
   `PrintBarcodeCommand` (registration + search reprint buttons);
   `WorkSheetPdfWriter` prints `row.Barcode` as a text cell with an explicit
   out-of-scope comment about scannable images.
   Exact Location: `BarcodeService.cs:44-92`,
   `PrintBarcodeCommandHandler.cs:26-45`,
   `PatientEditorViewModel.cs:1381` (`"barcode" => PrintBarcodeCommand`),
   `PatientSearchViewModel.cs:649-661` (reprint),
   `WorkSheetPdfWriter.cs:12-18,111-114`.
   Interpretation: REF-067 = complete the Code branch inside REF-066's
   writer using the REF-037 renderer. Confidence: CONFIRMED.

2. Claim: Reference mandates the envelope barcode and defines its data rule.
   Evidence: Blueprint §7 Placement ("on the envelope when
   `EnvelopePrintItemPosition` Code item is enabled"), Data content
   (PatientId or LabId per `PrintLabIdInsteadOfPatientId`), Symbology Code
   128 via `IBarcodeService` path; PRD UI-31 (Code item "with barcode
   preview").
   Exact Location: Reporting Blueprint `§7 table (lines 209-218)`,
   PRD `UI-31` line 544.
   Interpretation: Code-enabled ⇒ scannable Code128 of the D-2/66 identifier
   + human-readable code text beside it. Confidence: CONFIRMED.

3. Claim: `PrintDateTimeOnTubeBarcode` must NOT leak into the envelope barcode.
   Evidence: Blueprint §7 row "Tube barcode" scopes datetime-suffix to the
   tube label; §6 matrix maps the flag to P-07 only; `BarcodeService` applies
   it to the standalone label payload only.
   Exact Location: Blueprint lines 200, 213-215; `BarcodeService.cs:65-67`.
   Interpretation: envelope payload = identifier only. Confidence: CONFIRMED.

4. Claim: Test seam already proven for the renderer.
   Evidence: `BarcodeServiceTests` builds `BarcodeLabelRenderer` directly and
   asserts render output; writer-mapping tests (`ReceiptPrintingServiceTests`
   `BuildTextLines` lines 219-252) show the pure-mapping test idiom to copy
   for `EnvelopeTextLines` + barcode payload assertions.
   Exact Location:
   `tests/TopLab.Infrastructure.Tests/Barcode/BarcodeServiceTests.cs:40-79`,
   `tests/TopLab.Infrastructure.Tests/Printing/ReceiptPrintingServiceTests.cs:219-252`.
   Interpretation: REF-067 tests assert payload correctness + PNG validity
   without asserting on source text. Confidence: CONFIRMED.

#### D.3 Scope definition

IN SCOPE:

- `EnvelopePdfWriter`: when Code position enabled, render the Code128 image
  (via `BarcodeLabelRenderer.Render(BarcodePayload.For(...))` +
  `BarcodePngEncoder`) at the Code offsets + human-readable identifier text;
  when disabled, omit both (no empty box).
- `EnvelopeTextLines` carries `CodeBarcodePayload: string?` (null when Code
  disabled) so mapping tests assert payload logic PDF-free.
- Tests: payload follows identifier rule (LabId vs PatientId × flag);
  datetime flag ignored; PNG magic bytes valid; disabled-Code output
  contains no barcode.

OUT OF SCOPE / non-goals:

- New commands, services, DTOs, UI, migrations (all owned by REF-066).
- Touching `BarcodeService`, `BarcodeLabelRenderer`, worksheet/receipt code.
- Live settings-screen preview (still static per REF-065 decision 65-B).
- Tube-label behavior changes (REF-037 path untouched).

#### D.4 Implementation plan (Domain → Application → Infrastructure → Presentation → Tests → Migration)

1. Domain/Application — no changes (helper + renderer already owned by
   REF-066 / REF-037).
2. Infrastructure — `Printing/EnvelopePdfWriter.cs` (MODIFY, Code branch +
   mapping field); `Barcode/BarcodePngEncoder.cs` used as-is.
3. Presentation — no changes (trigger already live from REF-066).
4. Tests — Infrastructure: mapping tests (payload on/off), encoder/PNG
   validity test, renderer-integration test (distinct identifiers ⇒ distinct
   bytes; invalid input ⇒ guarded error, never throw out of service).
5. Migration — none.

Shared-component note: consumes REF-066's writer/encoder/helper and REF-037's
renderer; creates nothing shared.

#### D.5 Migration decision

`NO_MIGRATION_REQUIRED` — in-memory image bytes on an existing document;
no stored state changes. No migration is part of the work for REF-067.

#### D.6 Migration Contract

Not applicable.

#### D.7 Acceptance criteria

- Envelope with Code enabled shows a scannable Code128 image encoding
  exactly the identifier from the `PrintLabIdInsteadOfPatientId` rule, plus
  readable code text; verified by decoding the PNG bytes in test (or
  documented scan check) — not by string-matching source.
- Code disabled ⇒ no barcode image, no code text, layout of other items
  unchanged.
- Enabling datetime-on-tube-barcode does not alter envelope payload.
- No migration; no changes outside `EnvelopePdfWriter` + tests.

#### D.8 Autonomous decisions made

- 67-A (autonomous): barcode size on envelope = renderer default 300×80
  scaled to fit a ~6×1.5 cm box at Code offsets (aspect preserved).
  Options: fixed cm box / renderer-native scale / full-width band. Selected
  fixed box. Rationale: envelope offsets are per-item points, not bands;
  scannability needs quiet zones + minimum X-dimension, both preserved at
  this size; exact metrics verified visually by build agent.
- 67-B (autonomous): human-readable text under the image always accompanies
  the barcode (mirrors `BarcodeService` label's identifier line,
  `BarcodeService.cs:118-120`). Rationale: clerk fallback when scanners
  fail; matches existing product behavior.

#### D.9 Function-level status

`READY_AUTONOMOUS_DECISIONS_RECORDED`

---

### D-4. REF-068 — Print a laboratory order with barcode (Function 4 of 5)

#### D.1 Function identity

- REF-ID: REF-068. Name: Print a laboratory order with barcode. Audit status:
  MISSING. Position: Function 4 of 5. Depends on earlier phase function:
  REF-066 (barcode pipeline: renderer + PNG encoder + identifier helper).
- Pre-identified BLOCKED flag: NONE.

#### D.2 Investigation findings (Claim → Evidence → Exact Location → Interpretation → Confidence)

1. Claim: No order/requisition document exists anywhere.
   Evidence: grep `Requisition|LabOrder|OrderPdfWriter` across `src/`
   returns zero files; Application `Features/` has no order-print folder;
   registration screen (UI-03 ordering flow) buttons are
   باركود/ورقة العمل/الإيصال/الفاتورة — no order-slip action.
   Exact Location: negative greps; `PatientEditorView.xaml:45-48` (button
   row); PRD `UI-03` line 516 (button list ends with موافق, no order print).
   Interpretation: new document slice modeled on the receipt pair.
   Confidence: CONFIRMED.

2. Claim: All order-slip content is already available without new storage.
   Evidence: patient demographics on `Patient`; ordered tests per visit via
   `PatientBillingReader.ReadAccount` → `ChargedTests` (`TestName`,
   `TestCode`, `PriceAtOrderTime`) — the same set the receipt itemizes;
   identifier rule via `BarcodePayload.For`; lab header via
   `ILabPrintTextStore`; sample-kind context via worksheet DTOs if needed.
   Exact Location: `PatientBillingDtos.cs:15-20` (`ChargedTestDto`),
   `PatientBillingReader.cs:38`, `GetPatientReceiptQueryHandler.cs:36-47`.
   Interpretation: slip = header + patient block + ordered-test table +
   barcode; computed live. Confidence: CONFIRMED (test-set =
   charged-tests equivalence: INFERRED — build agent verifies the reader
   returns the visit's ordered tests incl. uninvoiced ones).

3. Claim: The natural trigger is the S-03 registration/ordering screen.
   Evidence: PRD UI-03 is the ordering flow (test selectors, Patient tests
   list, billing block); its action row hosts باركود (standalone label) and
   ورقة العمل already; Blueprint P-07 trigger = "S-03 ordering, sample draw";
   P-06 trigger = "sample collection / ordering flow".
   Exact Location: PRD line 516; Blueprint lines 114-115;
   `PatientEditorViewModel.cs:125-128` (print dispatch pattern
   `PrintAsync(ct, kind)` with `"barcode"|"receipt"|...` arms at 1381-1383).
   Interpretation: add `طلب تحاليل` button + `"laborder"` arm beside them.
   Confidence: CONFIRMED (caption wording: autonomous, D.8).

4. Claim: Only four printer output types exist, so the slip must route to one
   of them.
   Evidence: `PrinterOutputType` = Reports/Barcode/Envelope/Receipt
   (`PrinterOutputType.cs:5-8`); `PrinterAssignment` seeds exactly those four
   (snapshot lines 1124-1144); invoice precedent routes to an adjacent type
   (Receipt) via Settled Decision SD-8 "no fifth output type"
   (`InvoicePrintingService.cs:16-17`).
   Exact Location: as cited. Interpretation: order slip → Reports printer
   (plain-paper document; decision 68-A). Confidence: CONFIRMED.

#### D.3 Scope definition

IN SCOPE:

- Application: `Features/PatientEnvelope/Common/LabOrderDtos.cs`
  (`LabOrderDto`: patient header fields + `IReadOnlyList<LabOrderLineDto>`
  TestCode/TestName + Identifier + DateUtc), `Common/LabOrderPrintEnvelope.cs`
  token, `Commands/PrintLabOrder/PrintLabOrderCommand` + Validator +
  Handler (loads Patient + account lines, builds DTO, token,
  `ILabOrderPrintingService`), permission
  `PatientRegistrationAccessPolicy.AddEditPatient` (mirrors
  `PrintBarcodeCommand.cs:8-11`).
- Interfaces: `ILabOrderPdfWriter`, `ILabOrderPrintingService`.
- Infrastructure: `Printing/LabOrderPdfWriter.cs` (QuestPDF A5 RTL; lab
  header words; patient block; test table Code/Name; footer barcode image +
  readable identifier), `Printing/LabOrderPrintingService.cs` (reads
  `SystemSettings` flag + `PrinterAssignment(Reports)` + lab text per D.8;
  never throws), DI registrations.
- Presentation: `طلب تحاليل` button on `PatientEditorView` action row +
  `PrintLabOrderCommand` wiring in `PatientEditorViewModel.PrintAsync`
  dispatch (new `"laborder"` arm).
- Tests: handler/validator/service/writer-mapping/DI/dispatch tests.

OUT OF SCOPE / non-goals:

- Schema/migration changes; new printer output type; new lab-text scope.
- Order lifecycle/status tracking (no `IsPrinted` semantics for slips).
- Modifying receipt/invoice/worksheet/barcode-label paths.
- Send-out / external-lab order variants.

#### D.4 Implementation plan (Domain → Application → Infrastructure → Presentation → Tests → Migration)

1. Domain — no changes.
2. Application — CREATE DTOs, token, command+validator+handler under
   `Features/PatientEnvelope/` (same family folder; NOT a new top-level
   area). Handler reuses `PatientBillingReader.ReadAccount` for lines and
   `BarcodePayload.For` for the identifier; verify line-set = ordered tests
   (D.2 item 2) and record the verification.
3. Infrastructure — CREATE `LabOrderPdfWriter`, `LabOrderPrintingService`;
   barcode image via `BarcodeLabelRenderer` + `BarcodePngEncoder` (REF-066
   shared); MODIFY `DependencyInjection.cs`.
4. Presentation — MODIFY `PatientEditorView.xaml` (button after الفاتورة) +
   `PatientEditorViewModel.cs` (`PrintLabOrderCommand`, dispatch arm,
   error presentation via existing pattern).
5. Tests — per D.3; include empty-test-list behavior (slip still prints
   patient block + barcode with "لا تحاليل مطلوبة" line — decision 68-D).
6. Migration — none.

Shared-component note: consumes REF-066's encoder + helper; creates the lab-
order pair, which no later function in this phase reuses (REF-127 touches
receipt/invoice, not the slip).

#### D.5 Migration decision

`NO_MIGRATION_REQUIRED` — transient document from existing rows; no stored
state. No migration is part of the work for REF-068.

#### D.6 Migration Contract

Not applicable.

#### D.7 Acceptance criteria

- `طلب تحاليل` on the registration screen prints an A5 RTL slip on the
  Reports printer listing ordered tests (code + name), patient block,
  and a scannable identifier barcode + readable text.
- Permission grant declared (`AddEditPatient`); validator rejects invalid
  PatientId; deleted/missing patient ⇒ NotFound Arabic message.
- Empty order list still prints (patient + barcode + no-tests line).
- No migration; no new printer type; no new lab-text scope.

#### D.8 Autonomous decisions made

- 68-A (autonomous): routing = `PrinterOutputType.Reports`. Options:
  Reports / Receipt / Envelope / Barcode. Selected Reports. Rationale:
  plain-paper A5 slip like receipt/invoice/worksheets; Envelope/Barcode
  printers use special stock; Receipt printer may be a cashier roll;
  SD-8 precedent forbids a fifth type.
- 68-B (INFERRED_OWNER_DECISION): lab header = `LabPrintTextScope.Receipt`
  block. Options: Receipt / Envelope / new scope. Selected Receipt.
  Rationale: the slip originates at the S-03 billing desk beside receipt
  printing; adding a fourth scope needs store + editor + seed work with zero
  reference mandate. Genuinely absent from docs ⇒ flagged
  INFERRED_OWNER_DECISION; a future owner may redirect to Envelope scope
  with a one-line change (service call site isolated).
- 68-C (autonomous): caption = `طلب تحاليل`, placed after الفاتورة in the
  S-03 action row. Rationale: Arabic product language; action-row grouping
  with sibling print actions.
- 68-D (autonomous): empty slip prints with `لا تحاليل مطلوبة` line rather
  than refusing. Rationale: matches worksheet empty-state idiom
  (`لا تحاليل للزيارة`, `PatientResultSheetView.xaml:38`); a refusal would
  invent lifecycle semantics.

#### D.9 Function-level status

`READY_AUTONOMOUS_DECISIONS_RECORDED`

---

### D-5. REF-127 — Barcode printed on the receipt and the envelope (Function 5 of 5)

#### D.1 Function identity

- REF-ID: REF-127. Name: Barcode printed on the receipt and the envelope.
  Audit status: MISSING. Position: Function 5 of 5.
- Depends on earlier phase functions: REF-066 (writer pattern + encoder +
  helper; envelope half = verification of REF-066/067 output).
- Pre-identified BLOCKED flag: NONE.

#### D.2 Investigation findings (Claim → Evidence → Exact Location → Interpretation → Confidence)

1. Claim: Neither receipt nor invoice writer draws a barcode; both emit
   identifier text only.
   Evidence: `ReceiptPdfWriter.BuildTextLines` (lines 150-216) builds header/
   patient/items/totals/pickup strings with zero image calls; patient block
   shows `رقم المعمل: {LabId}` or `رقم المريض: {PatientId}` as text;
   `InvoicePdfWriter.BuildTextLines` (lines 133-186) likewise; zero `.Image(`
   calls in `src/`.
   Exact Location: `ReceiptPdfWriter.cs:150-216`, `InvoicePdfWriter.cs:133-186`.
   Interpretation: scannable-barcode branch must be added to both writers
   (TASK INPUT cites both files — invoice is in scope as the itemized
   counterpart). Confidence: CONFIRMED.

2. Claim: DTOs already carry both identifier forms; services lack only the
   selector flag.
   Evidence: `ReceiptDto`/`InvoiceDto` carry `PatientId` + `LabId`
   (`PatientBillingDtos.cs:33-55`); NEITHER printing service reads
   `SystemSettings` today (`ReceiptPrintingService` reads ReceiptSettings +
   assignment + labText; `InvoicePrintingService` reads assignment + labText).
   Exact Location: `ReceiptPrintingService.cs:64-80`,
   `InvoicePrintingService.cs:58-68`.
   Interpretation: services add one live `SystemSettings` read and pass the
   computed payload string to the writers — DTOs unchanged (token compatible,
   no migration). Confidence: CONFIRMED.

3. Claim: The envelope half of REF-127 is satisfied by REF-066/067; this
   function's envelope work is verification, not new code.
   Evidence: REF-067 draws the Code128 image on the envelope when Code
   enabled (D-3/REF-067); Blueprint §7 placement lists receipt/report/tube/
   envelope as the four barcode surfaces — report/tube exist, envelope comes
   from this phase, receipt (+invoice) from this function.
   Interpretation: no duplicate envelope-barcode implementation; acceptance
   asserts both surfaces in one pass. Confidence: INFERRED (phase-plan
   coherence, no code contradiction).

4. Claim: Existing receipt/invoice tests assert text mapping and will need
   barcode-aware extension, not replacement.
   Evidence: `ReceiptPrintingServiceTests` lines 219-252 and
   `InvoicePrintingServiceTests` lines 165-184 assert `BuildTextLines`
   content; `PrintReceiptCommandHandlerTests` / `PrintInvoiceCommandHandlerTests`
   assert dispatch tokens.
   Exact Location: as cited.
   Interpretation: extend mapping tests with payload assertions; keep all
   green. Confidence: CONFIRMED.

#### D.3 Scope definition

IN SCOPE:

- `ReceiptPdfWriter` + `InvoicePdfWriter`: draw Code128 image (renderer +
  shared encoder) + human-readable identifier line in the patient block;
  mapping functions expose the payload for PDF-free tests.
- `ReceiptPrintingService` + `InvoicePrintingService`: live-read
  `SystemSettings.PrintLabIdInsteadOfPatientId`, compute
  `BarcodePayload.For(...)`, pass payload into writers (writer signatures
  extended with a `barcodePayload` parameter; call sites + tests updated).
- Envelope half: end-to-end verification (envelope shows barcode when Code
  enabled — REF-067 acceptance re-run, not new code).
- Tests: payload rule matrix (LabId present/absent × flag on/off), PNG
  validity, writer includes image, service failure when SystemSettings row
  missing, all pre-existing receipt/invoice tests green.

OUT OF SCOPE / non-goals:

- Any new command, DTO field, table, migration, or printer routing change.
- Reworking receipt/invoice layout beyond the barcode block.
- Envelope writer changes (frozen after REF-067).
- `PrintOnce`/reprint-guard semantics (display-only precedent documented in
  `ReceiptPrintingService.cs:19-24` — not revived here).

#### D.4 Implementation plan (Domain → Application → Infrastructure → Presentation → Tests → Migration)

1. Domain/Application — no changes (helper owned by REF-066; DTOs frozen).
2. Infrastructure —
   a. `Printing/ReceiptPdfWriter.cs` (MODIFY): `WritePdfAsync(..., string
      barcodePayload, ...)` + `BuildTextLines(..., string barcodePayload)`;
      barcode block after patient lines; LTR-safe rendering note.
   b. `Printing/InvoicePdfWriter.cs` (MODIFY): same shape.
   c. `Printing/ReceiptPrintingService.cs`, `Printing/InvoicePrintingService.cs`
      (MODIFY): add `SystemSettings` live read; missing row ⇒
      `Error.Unexpected("سجل إعدادات النظام مفقود.")` (BarcodeService
      precedent, `BarcodeService.cs:53-57`).
3. Presentation — no changes (existing receipt/invoice buttons dispatch
   unchanged commands).
4. Tests — extend receipt/invoice mapping + service tests; add payload-matrix
   tests; re-run envelope barcode acceptance (REF-067) as this function's
   envelope-half evidence.
5. Migration — none.

Shared-component note: consumes REF-066's `BarcodePayload.For`,
`BarcodeLabelRenderer`, `BarcodePngEncoder`; creates nothing shared.

#### D.5 Migration decision

`NO_MIGRATION_REQUIRED` — DTOs frozen, payload computed in memory, no stored
state. No migration is part of the work for REF-127.

#### D.6 Migration Contract

Not applicable.

#### D.7 Acceptance criteria

- Receipt and invoice PDFs each show a scannable Code128 of the identifier
  (`PrintLabIdInsteadOfPatientId` rule) + readable text; matrix covered in
  tests with real byte assertions.
- Missing `SystemSettings` row ⇒ clean Arabic Unexpected failure, no throw.
- Envelope barcode acceptance (REF-067) re-verified green in the same run.
- No migration; no DTO/token-shape change; no new UI.

#### D.8 Autonomous decisions made

- 127-A (autonomous): payload passed as a method parameter, DTOs frozen.
  Options: (a) extend `WritePdfAsync`/`BuildTextLines` signatures; (b) add
  `BarcodePayload` field to `ReceiptDto`/`InvoiceDto`. Selected (a).
  Rationale: tokens are transient JSON — either is compatible — but (a)
  keeps billing DTOs free of presentation concerns and avoids touching
  query handlers/tests that construct the DTOs.
- 127-B (autonomous): barcode block placed immediately after the patient
  identification lines (before the items table) on both documents.
  Rationale: cashier scan-first workflow; patient block is the only stable
  anchor on both layouts.

#### D.9 Function-level status

`READY_AUTONOMOUS_DECISIONS_RECORDED`

---

## E. Phase-Level Migration Sequence

Ordered list of migrations across all functions in the phase: **none**.

| Order | Migration | Function | Dependency |
|-------|-----------|----------|------------|
| — | (no migration) | REF-065 | — |
| — | (no migration) | REF-066 | — |
| — | (no migration) | REF-067 | — |
| — | (no migration) | REF-068 | — |
| — | (no migration) | REF-127 | — |

Cross-function migration dependencies: none. No migration is created in this phase. If `dotnet ef migrations add` reports a model difference during implementation, the difference is a diagnostic signal to be investigated (drift — e.g., snapshot vs configuration), not a reason to scaffold a migration.

Count confirmation: TASK INPUT expects 5 `NO_MIGRATION` / 0 `NEW_MIGRATION`;
this plan delivers exactly 5 / 0 / 0 blocked. Difference explanation: none
required — full agreement. Rationale per function: every persisted input
(EnvelopeSettings + 4 positions + Envelope PrinterAssignment + SystemSettings
barcode flags + ReceiptSettings + Patient/ExternalEntity/PatientTest data)
exists in the baseline migration, snapshot, and runtime seed-repair; all new
artifacts are transient documents computed live at print time.

---

## F. Phase-Level Acceptance Criteria

1. `طباعة ظرف` on the results screen prints a live envelope on the Envelope
   printer honoring top margin, header/footer words, per-item offsets, and
   caption suppression (REF-066).
2. The envelope Code item renders a scannable Code128 of the correct
   identifier; disabled Code omits it cleanly; tube-datetime flag never leaks
   in (REF-067).
3. `طلب تحاليل` on the registration screen prints an order slip with test
   lines + scannable barcode on the Reports printer (REF-068).
4. Receipt and invoice each carry a scannable identifier barcode; envelope
   barcode re-verified in the same run (REF-127).
5. Every new command/query declares its permission grant; unauthorized paths
   are denied by the existing authorization behavior (standing rule).
6. No dead UI control: both new buttons are wired to live MediatR dispatches
   and covered by tests that fail if the wiring is removed.
7. Full test suite green; no migration files added; snapshot byte-identical;
   no changes outside the files listed in Section D.
8. Excluded machinery untouched: no online/portal, analyser-interface,
   multi-branch, SMS/e-mail/fax, or patient-card code paths.

---

## G. Phase-Level Blocked Functions List

None. All five functions are planned (4 × READY-family, 1 × READY).
No function is marked BLOCKED; no `MIGRATION_DECISION_BLOCKED` exists.

---

## H. Risks and Dependencies

1. **QuestPDF `.Image(byte[])` API shape** (the only INFERRED API fact):
   zero in-repo usages. Mitigation: build agent verifies at compile time;
   fallback recorded in decision 66-A (adapt call, or explicit text fallback
   — never silent). Risk: LOW-MEDIUM (QuestPDF image support is
   long-standing public API; version 2026.9.0).
2. **Scannability is physical**: byte-level tests prove correct payload +
   valid PNG, not print-head fidelity. Mitigation: acceptance requires one
   real scan check on the envelope, slip, and receipt before phase sign-off
   (manual step, recorded by build agent).
3. **ChargedTests ≙ ordered tests** (REF-068 line source): `ReadAccount` is
   billing-derived. Mitigation: build agent verifies the reader returns the
   visit's ordered lines including uninvoiced/unpaid ones; if not, source
   lines from the PatientTest query used by worksheets instead (same DTO
   shape, no migration either way).
4. **Referral placeholder trace** (decision 66-D): if the registration
   placeholder source is untraceable, omit-with-record fallback applies.
5. **External dependency REF-037**: consumed read-only (`BarcodeLabelRenderer`
   + `PrintBarcodeCommand` identifier rule). If REF-037's path is broken at
   build time, REF-067/068/127 barcode work degrades to text + explicit
   record; the phase still completes (no BLOCKED propagation without cause).
6. **No migration discipline**: the strongest phase risk is a stray
   scaffolded migration from EF drift noise. Mitigation: Section E;
   any `ef migrations add` output is investigated as drift, not committed.

---

## I. Evidence Index

| # | Claim | Exact source evidence |
|---|-------|----------------------|
| 1 | Settings stack exists & editable | `TopLab.Domain/Settings/EnvelopeSettings.cs:7-43`; `EnvelopePrintItemPosition.cs:4-41`; `Configurations/Envelope*Configuration.cs`; `Migrations/20260828052248_BaselineDataModel.cs:57-81`; `EnvelopeSettingsViewModel.cs:120-227`; `EnvelopeSettingsView.xaml:1-129` |
| 2 | No consumer / no envelope writer | `ApplicationDbContext.DbSets.cs:59-60`; `Printing/` 23-file listing (no Envelope); `DependencyInjection.cs:63-99`; negative `PrintEnvelope|EnvelopePdf` grep |
| 3 | Schema + seeds sufficient | `ApplicationDbContextModelSnapshot.cs:1028-1144`; `ApplyDatabaseUpdatesCommandHandler.cs:37-99` |
| 4 | Receipt/invoice text-only | `ReceiptPdfWriter.cs:150-216`; `InvoicePdfWriter.cs:133-186`; zero `.Image(` hits |
| 5 | Barcode renderer proven | `Barcode/BarcodeLabelRenderer.cs:12-30`; `Barcode/BarcodeService.cs:44-175`; `PrintBarcodeCommandHandler.cs:26-45` |
| 6 | Worksheet barcode is text | `WorkSheetPdfWriter.cs:12-18,111-114` |
| 7 | Identifier rule precedent | `PrintBarcodeCommandHandler.cs:40-42`; `SystemSettingsConfiguration.cs:27` |
| 8 | Reference envelope spec | PRD `FR-OUT-05`, `FR-M04-005`, `FR-M22-010`, `UI-05`, `UI-31`; Reporting Blueprint `§6 matrix`, `§7`, `§10`, `§12.5` |
| 9 | Writer/service pattern to clone | `ReceiptPrintingService.cs:25-98`; `ReceiptPdfWriter.cs:20-56`; `InvoicePrintingService.cs:19-85`; `ReceiptPrintEnvelope.cs:12-18` |
| 10 | Permission precedents | `UpdateEnvelopeSettingsCommand.cs:14-16`; `PrintBarcodeCommand.cs:8-11`; `PrintWorkSheetCommand.cs` (`PRINT_WORKSHEET`); `ResultsEntryAccessPolicy` + `MarkResultPrinted`/`ExecuteBulkPrint`; `PrintReceiptCommand.cs:10-11` (ungated precedent) |
| 11 | Trigger homes lack buttons | `PatientResultSheetView.xaml:82-88`; `PatientResultSheetViewModel.cs:42-52`; `PatientEditorView.xaml:45-48`; `PatientEditorViewModel.cs:125-128,1381-1383` |
| 12 | Four printer types only | `PrinterOutputType.cs:5-8`; snapshot `:1124-1144`; `InvoicePrintingService.cs:16-17` (SD-8) |
| 13 | Lab-text scopes = 3, JSON store | `ILabPrintTextStore.cs:6-22` |
| 14 | Baseline truth | HEAD `7cd2585`, clean tree, 14 migrations, latest `20261002002546_AddAntibioticMasterFields`, EF 8.0.30, net8.0 |
| 15 | Static preview + referral notes | `EnvelopeSettingsView.xaml:83-88`; `ReferralNameResolver.cs:7-15`; `PatientRegistrationDtos.cs:85-86`; `Patient.cs:13-37` |

