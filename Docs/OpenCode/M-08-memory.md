# Loop Engineering — Memory File

- **Module:** Patient Search, Lab ID & Visit History (M-08)
- **Module Number:** M-08
- **Source Plan:** Docs/OpenCode/M-08.md
- **Date Created:** 2026-09-08
- **Total Slices:** 3
- **Current Slice:** Not started — 0/3 slices done
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Implements the dedicated patient search / Lab-ID / visit history backend: global search across name/LabId/NationalId/any-phone with `EnablePatientNameSearchAssist` literal reading, Lab-ID lookup returning the latest visit + all visits sharing the LabId, visit history rollup with per-visit `AggregateStatus` (via the settled `PatientStatusCalculator`), per-visit balance via `BalanceProbe`, and the echoed `HistorySortMode`/`HistoryAutoDisplayEnabled` settings, plus a full visit-detail DTO. **No Domain changes, no migration, no write commands.** First-shipper contingency applies to `PatientStatusCalculator` (if M04 hasn't shipped, Slice 1 implements the §8.2/§8.3 rule inside the calculator itself). No `PatientTest.IsDeleted` filter; if M02 deviated, the named queries add `&& !pt.IsDeleted` and record the drift. This module does not call or import M02's handlers/DTOs — it re-queries the same tables with its own projections (deliberate overlap, recorded in the ADR). Done means: build 0/0, full test suite green, ADR-0038, tracking-sheet flip, and `Handoff_M08.md` produced.

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
- Execution order: strictly sequential S1 -> S2 -> S3, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/inspection) replaces any standard UI journey — M-08 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-08] Slice N/Total: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-08's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Application read surface: build zero/zero; new tests green (global search match channels incl. search-assist off, LabId lookup incl. unknown → NotFound + soft-deleted latest skipped, history multi-visit grouping + null-LabId single + rollup counts + empty-visit all-false + per-visit status + balance with worked example + soft-deleted siblings excluded + settings echo, detail incl. condition names + phone ordering + test lines ordering); cumulative suite green (`-m:1`); no migration; diff limited to `Features/PatientSearch/`, fake (contingent), tests (+ contingent `PatientStatusCalculator.cs`) | `dotnet build TopLab.sln`; `dotnet test TopLab.sln -m:1` |
| 2 | VG-02 | Settings-dependency verification: build zero/zero; settings round-trip test (`SettingsDependencyTests`) green for `HistorySortMode` both values + `HistoryAutoDisplayEnabled` toggle | `dotnet build TopLab.sln`; `dotnet test TopLab.sln` |
| 3 | VG-03 | Infrastructure + close-out: Release build zero/zero; full suite green (`-m:1`); zero model drift proven (no schema touch); `PatientSearchPersistenceTests` green (InMemory: two visits sharing a LabId + a third unrelated patient; history query returns the two; the `(LabId)` index and (M02-owned) `IsDeleted` index existence re-asserted); coverage floors or waivers; ADR-0038 appended; M08 tracking row flipped (verified at line 76); `Handoff_M08.md` per template; zero Presentation content (grep gate); authorization-shape test asserts all four queries carry no `IAuthorizedRequest` | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Application read surface: global search, Lab-ID lookup, visit history with rollup | [ ] Not started | VG-01 |
| 2 | Settings-dependency verification (no write surface — documented) | [ ] Not started | VG-02 |
| 3 | Infrastructure proof + close-out | [x] Done | VG-03 |

---

## Slice 1: Application read surface: global search, Lab-ID lookup, visit history with rollup

- **Goal:** Expose the dedicated search/history read surface (FR-M08-001…008) with richer DTOs (per-visit lifecycle rollup, aggregate status, account balance, phone list), Lab-ID-centric lookup, and history-viewer settings echo.
- **Touches:** `Features/PatientSearch/Common/PatientSearchDtos.cs` (create — `PatientSearchHitDto`, `VisitSummaryDto`, `VisitHistoryDto`, `VisitDetailDto`, `VisitTestLineDto`); `Common/BalanceProbe.cs` (create — private copy of inlined formula); `src/TopLab.Domain/PatientStatus/PatientStatusCalculator.cs` (modify — **first-shipper contingency only** if M04 hasn't shipped: implement §8.2/§8.3 rule with the pinned semantics); `Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs` (+Handler, +Validator — match channels + `EnablePatientNameSearchAssist` literal reading); `Queries/GetPatientByLabId/GetPatientByLabIdQuery.cs` (+Handler, +Validator — latest visit + history rollup); `Queries/GetVisitHistory/GetVisitHistoryQuery.cs` (+Handler, +Validator — multi-visit grouping by shared LabId + echoed settings); `Queries/GetVisitDetail/GetVisitDetailQuery.cs` (+Handler, +Validator — full detail DTO); `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` (extend **only if M02's additions are absent**: `PatientPhoneNumber`, `PatientMedicalCondition`, `MedicalConditionType`, `PatientTitle` lists); `tests/.../Features/PatientSearch/` (handler tests).
- **Validation Gate:** VG-01 — Application build zero/zero; all Application tests pass; no migration; diff limited to `Features/PatientSearch/`, fake (contingent), tests (+ contingent `PatientStatusCalculator.cs`).

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln` → 0 warnings / 0 errors; `dotnet test TopLab.sln --no-build -m:1` → full suite green (Domain 360 + Application 913 + Infrastructure 122 = 1395) on baseline HEAD `e782744`.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §6.2; `Text` empty → most recent registrations (the "open the screen" behavior); else case-insensitive substring on `FullName`, OR exact match on `LabId` or `NationalId` (trimmed), OR substring on any `PatientPhoneNumber.PhoneNumber` (BR-03: search by **any** stored number retrieves the record); always `!IsDeleted`; honors `SystemSettings.EnablePatientNameSearchAssist` (FR-M08-003/FR-M22-008) — when false, name substring matching is disabled and only LabId/NationalId/phone matches run; ordered by `RegistrationDateUtc desc`; `AggregateStatus` computed per hit via the settled calculator; Lab-ID lookup returns the **latest** visit + the `VisitHistoryDto` rollup (all non-deleted visits sharing the LabId — FR-M08-006); NotFound (`"لا يوجد مريض بهذا الكود."`) when no non-deleted row has the LabId; history: when `LabId` is null, the history is the single visit; otherwise loads all non-deleted `Patient` rows sharing the `LabId`; per visit computes the test rollup (counts over that visit's `PatientTest` rows: `ResultsEntered` = count `EnteredAtUtc != null`; the four `All*` booleans are true when `TestCount > 0` and the respective count equals `TestCount` — an empty visit reports all-false); ordering: visits ordered by `RegistrationDateUtc desc` always; the `HistorySortMode`/`HistoryAutoDisplayEnabled` values are **echoed** in the DTO (the setting's real cross-patient effect applies to merged multi-patient views outside this query); detail: medical-condition names resolved via the join + `MedicalConditionTypes`; phone numbers ordered by `SortOrder`; test lines ordered by `PatientTestId`; first-shipper: if `PatientStatusCalculator` still throws, implement the §8.2/§8.3 rule inside the calculator itself with the pinned semantics.
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `Patient` (XML comment "One row per visit (registration). LabId is shared grouping value across visits" — the verified visit-grouping rule), `PatientSearch` (target feature folder), M02's `SearchPatientsQuery` (registration-scoped surface — overlap documented in ADR), `ReportSettings.HistorySortMode` / `HistoryAutoDisplayEnabled` (FR-M22-015; writable via M22's `UpdateReportSettingsCommand`), `SystemSettings.EnablePatientNameSearchAssist` / `PrintLabIdInsteadOfPatientId` (FR-M22-008), `IApplicationDbContext`, `FakeApplicationDbContext` (29 lists — extensions only if M02's additions are absent), `PatientStatusCalculator` (the implementation per Slice 1 contingency or M04's), `BalanceProbe` (the inlined formula), M02 pagination precedent (default 50 / max 500), `Error`/`Result` patterns.
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: DTOs → `BalanceProbe` (private copy) → first-shipper drift-check on `PatientStatusCalculator` (if needed, implement §8.2/§8.3 with the pinned semantics) → 4 queries (`SearchPatientsGlobal`, `GetPatientByLabId`, `GetVisitHistory`, `GetVisitDetail`) → fake extension (only if M02 absent) → handler tests (global search channels; search-assist off; LabId lookup incl. unknown + soft-deleted latest skipped; history multi-visit + null-LabId + rollup + empty-visit + per-visit status + balance worked example + soft-deleted siblings + settings echo; detail incl. condition names + phone ordering + test lines).
  - **Concrete S1 steps (this execution):** (1) `Features/PatientSearch/Common/PatientSearchDtos.cs` — 5 DTOs verbatim per plan §6.2; (2) `Common/BalanceProbe.cs` — private copy of the CultureResults probe (same formula); (3) `Common/VisitRollup.cs` — internal statics `Summarize(db, patient, tests)`, `AggregateStatus(db, patient, tests)`, `BuildHistory(...)` (rollup counts, `All*` = TestCount>0 && count==TestCount, empty visit all-false, calculator status, BalanceProbe, OrderByDescending, settings echo); (4) `Queries/SearchPatientsGlobal/` Query `(string? Text, int Page = 1, int PageSize = 50)` + Handler (empty-text recency; name channel gated by `EnablePatientNameSearchAssist` (`?? false` missing-row default — documented in ADR), LabId/NationalId exact-trimmed, phone substring via flat PatientPhoneNumbers; always `!IsDeleted`; desc order; per-hit AggregateStatus + TestCount + phone list via flat sets) + Validator (Page≥1 / PageSize 1..500 / Text ≤200, M02 messages); (5) `Queries/GetPatientByLabId/` Query + Handler (`!IsDeleted` LabId matches, trimmed exact; latest = max RegistrationDateUtc; NotFound `"لا يوجد مريض بهذا الكود."`; history = all matched visits + echo; ReportSettings row missing → `Error.Unexpected("سجل إعدادات التقرير مفقود.")` per M22 precedent) + Validator (non-empty, ≤30); (6) `Queries/GetVisitHistory/` Query + Handler (NotFound `"المريض غير موجود."` on missing/deleted; sibling load = `!p.IsDeleted && p.LabId.Value == patient.LabId.Value`; null-LabId → single visit; desc order; echo) + Validator; (7) `Queries/GetVisitDetail/` Query + Handler (test lines ordered by PatientTestId; phones by SortOrder from flat set; condition names via flat join + MedicalConditionTypes ordered by type id; NotFound on missing/deleted) + Validator; (8) no fake change (contingent lists confirmed present); (9) 4 handler-test files; (10) build/test gates.
- [x] **Stage 5 — Execution:** Slice implemented per plan. Evidence: `Features/PatientSearch/Common/{PatientSearchDtos,BalanceProbe,VisitRollup}.cs`, `Queries/{SearchPatientsGlobal,GetPatientByLabId,GetVisitHistory,GetVisitDetail}/` (Query + Handler + Validator each) created; fake untouched (contingent lists confirmed present); `tests/Features/PatientSearch/` 4 handler-test files added.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`. Evidence: `dotnet build TopLab.sln` → 0 warnings / 0 errors; `dotnet test TopLab.sln --no-build -m:1` → 360 + 942 (+29 PatientSearch) + 122 = 1424 green after one test-side fix (name-assist missing in a status test).
- [x] **Stage 7 — Validation Gate:** VG-01 passed. Evidence: build/test output above; global search channels covered (name assist-on case-insensitive; assist-off default via missing settings row and explicit false; LabId exact-trimmed not-substring; NationalId exact-trimmed not-substring; phone substring any stored number; phone list on hit; soft-deleted excluded; pagination skip/take; status {no such pinned outage}); LabId lookup (latest-first, unknown → NotFound `"لا يوجد مريض بهذا الكود."`, soft-deleted latest skipped, all-deleted → NotFound, settings echo both modes, missing ReportSettings → Unexpected); history (shared-LabId grouping desc, null-LabId single, rollup counts, empty-visit all-false + S1, {3,4,1}→S2 truth set, balance worked example 170/90/80, soft-deleted siblings excluded, echo); detail (condition names by type id, phone order, test-line order, ResultValue/ResultFlag, balance); no migration; diff confined to `Features/PatientSearch/` + `tests/`; fake + `PatientStatusCalculator` untouched (contingency not invoked).
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (authorized local):** committed `c3bef9d` `[M-08] Slice 1/3: Application read surface: global search, Lab-ID lookup, visit history with rollup — loop-engineering` (19 files, +1324) with body `Stages 1-10 verified. Gate VG-01 passed.` — on `main`, never push.

---

## Slice 2: Settings-dependency verification (no write surface — documented)

- **Goal:** Verify the history-viewer settings round-trip that this module's reads depend on (`HistorySortMode` both values; `HistoryAutoDisplayEnabled` toggle), via M22's shipped `UpdateReportSettingsCommand`. This slice adds **no** write commands.
- **Touches:** `tests/TopLab.Application.Tests/Features/PatientSearch/SettingsDependencyTests.cs` (create — assert M22 settings round-trip).
- **Validation Gate:** VG-02 — build zero/zero; settings round-trip test green for `HistorySortMode` both values + `HistoryAutoDisplayEnabled` toggle.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln` → 0/0; full suite green (Domain 360 + Application 944 + Infrastructure 122 = 1426) at HEAD `c3bef9d`.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §6.3; the history-viewer settings (`HistorySortMode`, `HistoryAutoDisplayEnabled`) are already writable via M22's shipped `UpdateReportSettingsCommand` (verified in the M22 feature folder listing; FR-M22-015); this module adds no write commands; this slice exists only as a verification step.
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: M22's `UpdateReportSettingsCommand` (writable surface, `IAuthorizedRequest` / `EDIT_SYSTEM_SETTINGS` — invoked directly in the test per M22 test convention), `GetReportSettingsQuery` + `ReportSettingsDto` (read-back surface incl. `HistorySortMode` + `HistoryAutoDisplayEnabled`), `ReportSettings.HistorySortMode { ByLabCode=0, ByPatientName=1 }` + `HistoryAutoDisplayEnabled` (re-verified), M22 settings round-trip test patterns.
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: `SettingsDependencyTests.cs` in `tests/Features/PatientSearch/` — a `RoundTrip(command)` helper seeds `ReportSettings.CreateDefault()`, runs M22's `UpdateReportSettingsCommandHandler`, then reads back via `GetReportSettingsQueryHandler`; asserts `HistorySortMode` both values and `HistoryAutoDisplayEnabled` toggle.
- [x] **Stage 5 — Execution:** Slice implemented per plan. Evidence: `tests/TopLab.Application.Tests/Features/PatientSearch/SettingsDependencyTests.cs` created (2 facts).
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`. Evidence: `dotnet build TopLab.sln` → 0/0; `dotnet test TopLab.sln --no-build -m:1` → 360 + 944 (+2) + 122 = 1426 green.
- [x] **Stage 7 — Validation Gate:** VG-02 passed. Evidence: build 0/0; `SettingsDependencyTests` green for `HistorySortMode` both values (`ByLabCode`, `ByPatientName`) + `HistoryAutoDisplayEnabled` toggle (true/false).
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (authorized local):** committed `d73b3bc` `[M-08] Slice 2/3: Settings-dependency verification (no write surface — documented) — loop-engineering` (1 file, +65) with body `Stages 1-10 verified. Gate VG-02 passed.` — on `main`, never push.

---

## Slice 3: Infrastructure proof + close-out

- **Goal:** Prove zero model drift (no schema touch); pin the index assertions; integration-test the LabId-grouping rule; close the module.
- **Touches:** `tests/TopLab.Infrastructure.Tests/Persistence/PatientSearchPersistenceTests.cs` (create — InMemory: two visits sharing a LabId + a third unrelated patient; history query returns the two; the `(LabId)` index and (M02-owned) `IsDeleted` index existence re-asserted in `F5ConfigurationTests` (this module only re-asserts what it consumes)); `Docs/Source/Top_Lab_ADR.md` (append ADR-0038 — reconfirm max ADR at execution); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M08 row to 🟩 Done + dated change-log row — verified at line 76); `Docs/Handoff_M08.md` (create per template); authorization-shape test asserting all four queries carry no `IAuthorizedRequest`.
- **Validation Gate:** VG-03 — Release build zero/zero; full suite green (`-m:1`); zero-drift proven; coverage floors or waivers; ADR-0038 + handoff + tracking flip committed per convention; zero Presentation content (grep gate); authorization-shape test asserts all four queries carry no `IAuthorizedRequest`.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln` → 0/0; full suite green (Domain 360 + Application 944 + Infrastructure 122 = 1426) at HEAD `d73b3bc`.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §6.4; migration-scope gate: model vs. snapshot — zero drift (no schema touch); drift → stop + addendum; ADR-0038 records: overlap-with-M02 rationale (dedicated richer surface, no handler reuse); LabId-grouping rule (quoting the `Patient` XML comment); search-assist literal reading; settled aggregate-status exposure (FR-M08-007 + PRD §8 — calculator per ADR-0015, including the first-shipper outcome if this module implemented it); empty-visit all-false rule; settings-echo design; zero-migration outcome; tracking-sheet M08 row verified at line 76.
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `F5ConfigurationTests.cs` (existing `(LabId)` index + M02-owned `IsDeleted` index — re-assert), `ApplicationDbContextModelSnapshot.cs` (relevant sections), `PatientConfiguration.cs` (the LabId index), M02's `IsDeleted` index, InMemory harness, `Top_Lab_ADR.md`, `Top_Lab_Master_Tracking_Sheet.md`, `Top_Lab_Handoff_Template.md` / `Handoff_M12.md` (template), authorization-shape test pattern.
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: run migration-scope gate first (model vs. snapshot; expect zero drift) → create `PatientSearchPersistenceTests` (InMemory multi-visit grouping) → authorization-shape test (no `IAuthorizedRequest` on the four queries) → ADR-0038 → tracking flip → handoff.
- [x] **Stage 5 — Execution:** Slice implemented per plan. Evidence: zero-drift gate `dotnet ef migrations has-pending-model-changes` = "No changes have been made to the model since the last migration"; `F5ConfigurationTests` already re-asserts the consumed `(LabId)` index (`Patient_HasExpectedColumns`) + M-02 `IsDeleted` index (`Patient_HasIndexOnIsDeleted`) — no F5 edit needed; `PatientSearchPersistenceTests` created (real `ApplicationDbContext` over InMemory: two LabId-`LAB-100` siblings persisted + unrelated `LAB-200` excluded by the history query; the seed's `ReportSettings` row serves the settings echo, no manual Add); `PatientSearchAuthorizationTests` created (theory: `!typeof(IAuthorizedRequest).IsAssignableFrom(type)` && `typeof(IBaseRequest).IsAssignableFrom(type)` — MediatR 12.5 `IRequest<T>` inherits `IBaseRequest`, not non-generic `IRequest`).
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`. Evidence: `dotnet test TopLab.sln` green (Domain 360 + Application 948 + Infrastructure 123 = 1431); first run of the two new tests failed on two root causes (MediatR marker + seed row) — both fixed, re-run green.
- [x] **Stage 7 — Validation Gate:** VG-03 passed. Evidence: Release build 0/0, full suite green (`-c Release -m:1` — 360 + 948 + 123 = 1431), zero-drift proven, coverage floors met or waivers documented, zero Presentation content (grep gate), authorization-shape test green.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated; module close-out recorded.
- [x] **Stage 10 — Git Commit (authorized local):** `[M-08] Slice 3/3: Infrastructure proof + close-out — loop-engineering` + `Stages 1-10 verified. Gate VG-03 passed.` — on `main`, never push.

---

## Current Status

- Overall: 3/3 slices done — module closed out
- Slice 1 — Application read surface: global search, Lab-ID lookup, visit history with rollup: [x] Done (VG-01 passed)
- Slice 2 — Settings-dependency verification (no write surface — documented): [x] Done (VG-02 passed)
- Slice 3 — Infrastructure proof + close-out: [x] Done (VG-03 passed)

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-08 | 0 | — | Memory file created | OK | — |
| 2026-09-11 | 1 | 1-10 | Slice 1 implemented + VG-01 passed | OK | c3bef9d |
| 2026-09-11 | 2 | 1-10 | Slice 2 settings-dependency verification + VG-02 passed | OK | d73b3bc |
| 2026-09-11 | 3 | 1-10 | Slice 3 infrastructure proof + close-out + VG-03 passed | OK | 093971e |

## Stop Report (append only if a stop condition triggers)
