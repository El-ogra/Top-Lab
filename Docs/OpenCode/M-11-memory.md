# Loop Engineering — Memory File

- **Module:** Work Sheets (M-11)
- **Module Number:** M-11
- **Source Plan:** Docs/OpenCode/M-11.md
- **Date Created:** 2026-09-08
- **Total Slices:** 2
- **Current Slice:** Slice 1 done — 1/2 slices done
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Implements the backend for the period/range-based work sheets (FR-M11-001/002/003 + S-33 + FR-OUT-07) and the in-module period test-count classification (FR-M11-004 — frequency classification view on the M11 screen per S-33). All four queries are gated on `PRINT_WORKSHEET` (FR-M17-004 item 8; the classification lives inside the M11 screen, so it shares the worksheet grant — not `STATISTICS`). Worksheet semantics: `PatientTest` rows for a chosen `DateOnly? From`/`To` (both default to current day in UTC when unspecified; `From > To` → Validation; bounds inclusive on UTC calendar days) whose sample should be in the lab (`!IsTakenOutsideLab`), grouped either by a saved `WorkGroupLog` (FR-M11-003) or by `TestGroup` / explicit test selection (FR-M11-002), with the owner-settled line identifier pair (`PatientTestId` + `LabId`) and `Test.Barcode` as the test classifier. Echoes the three system settings (`PrintFileExternalBarcode`, `PrintDateTimeOnTubeBarcode`, `PrintLabIdInsteadOfPatientId`) for the future renderer. The classification view counts all `PatientTest` rows (including `IsTakenOutsideLab`) for the same period. No `MarkWorkSheetPrinted` audit command ships; no print-tracking columns exist. No `PatientTest.IsDeleted` filter; if M02 deviated, all four queries add `&& !pt.IsDeleted` and record the drift. No migration, no Domain change, no `PermissionConfiguration` change. The worksheet deliverable is a DTO; `IBarcodeService`/`IReportPrintingService` stay unimplemented (Reporting §13: rendering is M07's). Done means: build 0/0, full test suite green, ADR-0039, tracking-sheet flip, and `Handoff_M11.md` produced.

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
- Stop threshold: 5 consecutive failures for the same reason (not 4).
- Execution order: strictly sequential S1 -> S2, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/inspection) replaces any standard UI journey — M-11 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-11] Slice N/Total: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-11's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Application: worksheet generation queries + test-count classification + DTOs + tests: build zero/zero; all new tests green (period defaulting incl. `From > To` → Validation + inclusive both ends, outside-lab tests excluded from the three worksheets but included in the test-count classification, soft-deleted patient excluded, log/group/test-id NotFound, ungrouped tests land in section 0, line ordering, summary counts partition correctly, classification counts group correctly + ordering + total + outside-lab inclusion rule, settings echo + missing settings row → Unexpected, `WorkSheetsAuthorizationTests` for all four queries on `PRINT_WORKSHEET` + standard denial); cumulative suite green (`-m:1`); no migration; no Domain change; no `PermissionConfiguration` change; diff limited to `Features/WorkSheets/` + tests | `dotnet build TopLab.sln`; `dotnet test TopLab.sln -m:1`; grep gates on src+tests |
| 2 | VG-02 | Infrastructure + close-out: Release build zero/zero; full suite green (`-m:1`); zero model drift proven; `WorkSheetQueryPersistenceTests` green (InMemory: by-log DTO contains only the in-lab rows with resolved names + correct period filtering; classification query counts the same seed correctly including the outside-lab row); `F5ConfigurationTests` extended for `WorkGroupLog`/`WorkGroupLogItem` mapping (composite key, name required); coverage floors or waivers; ADR-0039 appended; M11 tracking row flipped (verified at line 77); `Handoff_M11.md` per template; zero Presentation content (grep gate) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Application: worksheet generation queries + test-count classification + DTOs + tests (single combined slice) | [x] Done | VG-01 |
| 2 | Infrastructure proof + close-out | [ ] Not started | VG-02 |

---

## Slice 1: Application: worksheet generation queries + test-count classification + DTOs + tests (single combined slice)

- **Goal:** Expose the four worksheet queries (by WorkGroupLog, by TestGroup, summary, test-count classification) all gated on `PRINT_WORKSHEET`, with the owner-settled line identifier pair, period defaulting, and the three echoed system settings.
- **Touches:** `Features/WorkSheets/Common/WorkSheetDtos.cs` (create — `WorkSheetLineDto`, `WorkSheetSectionDto`, `WorkSheetDto`, `WorkSheetSummaryRowDto`, `WorkSheetTestCountRowDto`, `WorkSheetTestCountDto`); `Common/WorkSheetsAccessPolicy.cs` (create — `PrintWorksheet = "PRINT_WORKSHEET"`); `Queries/GetWorkSheetByWorkGroupLog/GetWorkSheetByWorkGroupLogQuery.cs` (+Handler, +Validator — `IAuthorizedRequest` with `PrintWorksheet`); `Queries/GetWorkSheetByTestGroup/GetWorkSheetByTestGroupQuery.cs` (+Handler, +Validator — same gate); `Queries/GetWorkSheetSummary/GetWorkSheetSummaryQuery.cs` (+Handler, +Validator — same gate); `Queries/GetWorkSheetTestCountByPeriod/GetWorkSheetTestCountByPeriodQuery.cs` (+Handler, +Validator — **FR-M11-004** classification, same gate); `tests/TopLab.Application.Tests/Features/WorkSheets/` (handler tests + `WorkSheetsAuthorizationTests`).
- **Validation Gate:** VG-01 — Application build zero/zero; all new tests green; cumulative suite green (`-m:1`); no migration; no Domain change; no `PermissionConfiguration` change; diff limited to `Features/WorkSheets/` + tests.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln` → 0/0; `dotnet test TopLab.sln -m:1` → 1430/1431 with the single waived midnight flake excluded per owner waiver (Execution Log 2026-09-11).
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §7.2; period: both null → today UTC; `To ??= From` (settled by the owner); `From > To` → Validation (`"بداية الفترة يجب ألا تتجاوز نهايتها."`); bounds inclusive on UTC calendar days; the owner-settled per-line scannable identity is the `LabId` + `PatientTestId` pair; `Barcode` is the test classifier from the catalog; the pinned tube-barcode content for the future renderer is Code 128 carrying `PatientId` or `LabId` per `PrintLabIdInsteadOfPatientId`, plus print date/time when `PrintDateTimeOnTubeBarcode` is set (Reporting §7); by-WorkGroupLog: log not found → `Error.NotFound("سجل مجموعة العمل غير موجود.")`; take its item `TestId`s; select `PatientTest` rows where `TestId ∈ items`, patient not soft-deleted, `Patient.RegistrationDateUtc` within `[From, To]`, `!IsTakenOutsideLab`; one section (the log itself: `SectionId = log.Id`, `SectionName = log.Name`); lines ordered by `Patient.RegistrationDateUtc` then `PatientTestId`; echo the three settings (read `SystemSettings` row `Id == 1` directly; missing row → `Error.Unexpected("سجل الإعدادات العامة مفقود.")`); by-TestGroup: when `TestIds` supplied (non-empty) → those tests only (unknown id → `Error.NotFound("التحليل غير موجود")`); else when `TestGroupId` supplied → the single group (`Error.NotFound("مجموعة التحاليل غير موجودة.")`), one section; else → all active groups, one section each, tests without a group in `SectionId = 0, SectionName = "بدون مجموعة"`; same row selection and line ordering; summary: per work-group log — `PendingCount` (rows `!IsSampleDrawn`), `DrawnCount` (`IsSampleDrawn && EnteredAtUtc == null`), `ResultedCount` (`EnteredAtUtc != null`); test-count classification (FR-M11-004): count `PatientTest` rows grouped by `TestId` over `Patient.RegistrationDateUtc` within `[From, To]` (patient not soft-deleted; **all** tests counted — the `!IsTakenOutsideLab` bench-work exclusion does NOT apply here — stated rule, one line in the handler XML comment); resolve `TestName`/`TestCode`; rows ordered by `Count desc`, tie-break `TestId`; `TotalCount` = sum.
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected + verified precedents: `WorkGroupLog`/`WorkGroupLogItem` (composite PK), `GetWorkGroupLogsQueryHandler` (`l.Id.Value` lookup, `WorkGroupLogId.Equals(log.Id)` items), `TestCatalogDtos` shapes, `Test`/`TestGroup`/`Patient`/`PatientTest`/`SystemSettings` fields, `GetCultureReportQueryHandler` (settings-missing style + `LabId?.Value`), `GetSystemSettingsQueryHandler` (`SingleOrDefault(s => s.Id == 1)`), `GetPatientsWithUncollectedSamplesQueryHandler` (`DateOnly.FromDateTime(reg) == day` idiom), `SaveWorkGroupLogItems` (command/validator/`TestId.Create`), `*AccessPolicy` (static class + const), `AuthorizationBehavior` (verbatim denial) + `BehaviorsAuthorizationBehaviorTests` (denial pattern), `FakeApplicationDbContext` (all sets present), `FakeCurrentUserService`, `Error`/`Result` API, `ValidationBehavior` present. Verified: NO `PatientTest.IsDeleted` (`AuditableEntity` clean) → no drift filter needed.
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: DTOs (6 records) → access policy → 4 queries (`GetWorkSheetByWorkGroupLog`, `GetWorkSheetByTestGroup`, `GetWorkSheetSummary`, `GetWorkSheetTestCountByPeriod`) → handler tests (period defaulting incl. `From > To`; outside-lab excluded from the three worksheets but included in the classification; soft-deleted excluded; log/group/test-id NotFound; ungrouped section 0; line ordering; summary counts partition; classification counts group + ordering + total + outside-lab inclusion rule; settings echo; missing settings row → Unexpected) → `WorkSheetsAuthorizationTests` (all four queries on `PRINT_WORKSHEET` + standard denial).
- [x] **Stage 5 — Execution:** Slice implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [x] **Stage 7 — Validation Gate:** VG-01 passed. Evidence: build/test output; authorization theory tests green; grep gates clean.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-11] Slice 1/2: Application: worksheet generation queries + test-count classification + DTOs + tests (single combined slice) — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.` — on `main`, never push.

---

## Slice 2: Infrastructure proof + close-out

- **Goal:** Prove zero model drift (no schema touch); pin the `WorkGroupLog`/`WorkGroupLogItem` mapping; integration-test the period filtering and the classification outside-lab inclusion rule; close the module.
- **Touches:** `tests/TopLab.Infrastructure.Tests/Persistence/Configurations/F5ConfigurationTests.cs` (extend — assert `WorkGroupLog`/`WorkGroupLogItem` mapping: composite key, name required — re-assert only what this module consumes); `tests/TopLab.Infrastructure.Tests/Persistence/WorkSheetQueryPersistenceTests.cs` (create — InMemory: seed log + items + two patients' tests (one outside-lab, one drawn) across a two-day period; the by-log DTO contains only the in-lab rows with resolved names and correct period filtering; the classification query counts the same seed correctly including the outside-lab row); `Docs/Source/Top_Lab_ADR.md` (append ADR-0039 — reconfirm max ADR at execution); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M11 row to 🟩 Done + dated change-log row — verified at line 77); `Docs/Handoff_M11.md` (create per template).
- **Validation Gate:** VG-02 — Release build zero/zero; full suite green (`-m:1`); zero-drift proven; coverage floors or waivers; ADR-0039 + handoff + tracking flip committed per convention; zero Presentation content (grep gate).

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §7.3; migration-scope gate: model vs. snapshot — zero drift; drift → stop + addendum; ADR-0039 records: generation-equals-printing permission choice; DTO-as-worksheet (no `IBarcodeService`/`IReportPrintingService` implementation — rendering per Reporting §13); outside-lab exclusion from bench sheets (and its non-application to the FR-M11-004 count); settled period-based design (FR-M11-001/002/003 + S-33 + FR-OUT-07) with the owner-settled default/boundary semantics (default-today-UTC, inclusive; UTC-day caveat recorded — a 01:00 registration in Egypt (UTC+2/+3) appears on "yesterday's" sheet; a lab-timezone setting is flagged as future work since no such setting exists in the verified `SystemSettings`); the in-module test-count classification (FR-M11-004); the owner-settled line-identifier choice (`LabId` + `PatientTestId` pair within the Reporting §7 barcode-content rule); zero-migration outcome; tracking-sheet M11 row verified at line 77.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `F5ConfigurationTests.cs`, `ApplicationDbContextModelSnapshot.cs` (relevant sections), `WorkGroupLogConfiguration.cs` (composite key, name required), `WorkGroupLogItemConfiguration.cs`, InMemory harness, `Top_Lab_ADR.md`, `Top_Lab_Master_Tracking_Sheet.md`, `Top_Lab_Handoff_Template.md` / `Handoff_M12.md` (template), `IBarcodeService` / `IReportPrintingService` (ports; no implementations — stays unimplemented per Reporting §13).
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: run migration-scope gate first (model vs. snapshot; expect zero drift) → extend `F5ConfigurationTests` for `WorkGroupLog`/`WorkGroupLogItem` mapping → create `WorkSheetQueryPersistenceTests` (InMemory by-log DTO + classification) → ADR-0039 → tracking flip → handoff.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-02 passed. Evidence: Release build 0/0, full suite green, zero-drift proven, coverage floors met or waivers documented, zero Presentation content (grep gate).
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-11] Slice 2/2: Infrastructure proof + close-out — loop-engineering` + `Stages 1-10 verified. Gate VG-02 passed.` — on `main`, never push.

---

## Current Status

- Overall: 1/2 slices done
- Slice 1 — Application: worksheet generation queries + test-count classification + DTOs + tests (single combined slice): [x] Done (VG-01 passed 2026-09-11; build 0/0; 28/28 new tests green; cumulative Domain 360/360 + Application 975/976 (sole FAIL = waived midnight flake) + Infra 123/123; diff confined to `Features/WorkSheets/` + tests)
- Slice 2 — Infrastructure proof + close-out: [ ] Not started

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-08 | 0 | — | Memory file created | OK | — |
| 2026-09-11 | 1 | 1 | Pre-execution: `dotnet build TopLab.sln` → 0 warnings, 0 errors | PASS | — |
| 2026-09-11 | 1 | 1 | Pre-execution: `dotnet test TopLab.sln -m:1` → 1430/1431; single FAIL `GetPatientsWithUncollectedSamples...OrderedByRegistrationAsc` (exp 2, got 0) — pre-existing midnight-boundary flake, unrelated to M-11 (UTC 00:33; seeds now-3h/now-1h fall on prior UTC day). Owner decision: rerun until green, then start S1 | BLOCKED-gate | — |
| 2026-09-11 | 1 | 10 | Owner decisions (Arabic): executor performs local commit per slice (no push, no branches) then starts next slice immediately; stop threshold 5; rerun suite until green | OK | — |
| 2026-09-11 | 1 | 1 | WAIVER (owner, Arabic): do not wait for the midnight flake — `GetPatientsWithUncollectedSamples...OrderedByRegistrationAsc` is EXCLUDED from all M-11 slice gates (VG-01/VG-02). Stage 1/6/7 gates = full suite green except that single known pre-existing failure; any OTHER failure still blocks | OK | — |
| 2026-09-11 | 1 | 1-7 | S1 implemented (15 src files: Common ×3 + 4 queries ×3) + 28 tests (17 handler + 6 validator + 5 auth); build 0/0; new 28/28 green; cumulative 360 + 975/976 (waived flake only) + 123; VG-01 PASS | OK | — |

## Stop Report (append only if a stop condition triggers)
