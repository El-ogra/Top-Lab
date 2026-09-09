# Loop Engineering — Memory File

- **Module:** Specialized Profile Result Reports (M-05)
- **Source Plan:** Docs/OpenCode/M-05.md
- **Current Branch:** main
- **Baseline verified:** 2026-09-09 (031a43e)
- **Status:** Execution-ready, not started

## Settled design record

The owner fixed three requirements: Analyte is the sole source of current range and results freeze it; manual/profile selection charging is centralised in PatientAccountCalculator; authorised printed-result amendments immediately become active and atomically create immutable restricted audit records. No owner decision is open.

Verified HEAD has no Analyte/Profile/audit model; ProfileResultItem.AnalyteName is text only; the calculator has no selection methods; M04 already supplies lifecycle guards/snapshot infrastructure; and the fake already supports ProfileResultItems. M05 reuses the latter two and creates only the missing model.

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain and schema foundation: analytes, profiles, snapshots, amendments, central pricing | [x] Done | VG-01 |
| 2 | Catalog and ordering integration: authoritative analyte source and central selection pricing | [x] Done | VG-02 |
| 3 | Profile entry, reporting, print, and post-print amendment surface | [x] Done | VG-03 |
| 4 | Infrastructure proof and close-out | [ ] Not started | VG-04 |

## Slice 1: Domain and schema foundation

Create Analyte/Profile/range-band/snapshot/amendment entities, IDs, configurations, DbSets, calculator selection methods, and one migration/backfill. One analyte range aggregate has typed bands; Profile has immutable FixedPrice and many analytes; the item snapshot preserves selected range and analyte identity. Backfill failure on non-deterministic legacy profile-item identity stops execution.

**VG-01:** Domain/configuration/migration tests prove memberships, one range aggregate, frozen snapshot, immutable price/audit rows, central pricing branches, and backfill/index/FK correctness. Build and full suite green.

### 10-Stage Progress — S1

- [x] Stage 1 — Pre-Execution Verification: `dotnet build TopLab.sln` 0 warnings/0 errors; `dotnet test TopLab.sln` 1223 passed / 0 failed (2026-09-09, HEAD 031a43e).
- [x] Stage 2 — Deep Understanding: re-read M-05.md §4.1 + §4.2 + §4.6 (frozen messages) and memory S1/VG-01 in full.
- [x] Stage 3 — File Analysis: inspected PatientTest.cs lifecycle, PatientAccountCalculator.cs, ProfileResultItem.cs, Test.cs, ReferenceRange.cs, ReferenceRangeSnapshot.cs, PatientTestReferenceRangeSnapshot.cs, StronglyTypedId/IDs/Entity/AuditableEntity, DbSets + ApplicationDbContext, all relevant configurations, FakeApplicationDbContext, M-04 snapshot migration, F5ConfigurationTests, InMemoryContextFactory, ExportPatientReportPdfCommandHandler (only AnalyteName consumer). M-04 lifecycle/fake support confirmed reusable.
- [x] Stage 4 — Planning: plan below; checklist recorded.
- [x] Stage 5 — Execution: implemented IDs, Analyte/Profile/band/ProfileAnalyte/snapshot/amendment entities, Test.AnalyteId mapping, calculator selection branches, Test.Configuration overrides, 7 DbSets + fake support, sole AnalyteName consumer w/ Analyte catalog, 7 entity configs, one EF migration `20260909033414_AddAnalyteProfileDomain` restructured so backfill (analyte-per-simple-test + one range aggregate copying ReferenceRange bands + Test mapping + profile-per-specialised-test FixedPrice=PatientPrice + legacy ProfileResultItem name-match mapping) runs BEFORE the THROW stop-guard and the AnalyteName drop; column tightened to NOT NULL post-backfill; Down symmetric. Domain tests 317→357, Infra tests 88→107 (incl. 10 migration/backfill structural tests), Application stays green at 818 after Export handler update.
- [x] Stage 6 — Post-Execution Verification: `dotnet build TopLab.sln` 0 warnings/0 errors (2026-09-09). `dotnet ef migrations has-pending-model-changes`: no model drift.
- [x] Stage 7 — Validation Gate VG-01: full suite green (357+818+107 = 1282 passed / 0 failed). Migration tests prove backfill-before-drop ordering, NOT NULL tightening, deterministic name-match guard (`THROW 50001` stop), Restrict FKs on both AnalyteId columns, unique indexes (Analytes.Name, AnalyteReferenceRange.AnalyteId, ProfileAnalyte (ProfileId,AnalyteId), Profile.TestId), symmetric Down with AnalyteName reconstruction before dropping Analytes. VG-01 met.
- [x] Stage 8 — Documentation update: Stages 1–10 marked complete.
- [x] Stage 9 — Memory Status update: status + log below.
- [x] Stage 10 — Git commit `[M-05] Slice 1/4: Domain and schema foundation — loop-engineering`.

**S1 Execution plan (Stage 5):**
1. Domain IDs: AnalyteId, AnalyteReferenceRangeId, AnalyteReferenceRangeBandId, ProfileId, ProfileAnalyteId, ProfileResultAmendmentId.
2. Domain entities — src/TopLab.Domain/Tests: Analyte (auditable, Name/ReportName/IsActive, AttachCurrentRange guard = exactly one), AnalyteReferenceRange (1:1 per analyte, band collection), AnalyteReferenceRangeBand (typed sex/age/unit band with Matches + band guards), Profile (immutable non-negative FixedPrice, specialised-test-only guard, many ProfileAnalyte), ProfileAnalyte (unique profile/analyte link guard).
3. Domain entities — src/TopLab.Domain/Results: ProfileResultItem (Replace AnalyteName→required AnalyteId; add draft Update, idempotent Verify/Unverify/MarkPrinted, narrowly-scoped Amend), ProfileResultItemReferenceRangeSnapshot (1:1 keyed by ProfileResultItemId, retains AnalyteId + captured band fields), ProfileResultAmendment (immutable audit row, creation-only).
4. Modify Test: add nullable AnalyteId (simple-test mapping).
5. Extend PatientAccountCalculator: ManualSelectionCharge(IReadOnlyList<decimal>) and ProfileSelectionCharge(decimal) — no formula duplication; Balance stays the sole aggregate.
6. ApplicationDbContext.DbSets + FakeApplicationDbContext: add the 7 new Set/Add/Remove branches.
7. Update ExportPatientReportPdfCommandHandler line 111 to resolve analyte name via Analytes catalog (sole AnalyteName consumer; keeps build green after name→Id replacement).
8. EF configurations for all new entities + Test.AnalyteId + ProfileResultItem override.
9. Tests: Domain tests (Analyte/range/band/Profile/ProfileAnalyte/ProfileResultItem lifecycle+amend/snapshot/amendment immutability/calculator branches), config tests (FKs/indexes), migration+backfill integration test incl. deterministic-match stop.
10. One EF migration (dotnet ef) + manual backfill SQL (create analyte+range+band per simple test copying ReferenceRange bands; Test.AnalyteId mapping; Profile per specialised test FixedPrice=PatientPrice; legacy ProfileResultItem mapping by exact configured name match with RAISERROR stop on unmatched rows; drop AnalyteName only after backfill; NOT NULL + FK + unique indexes; Down symmetric).

## Slice 2: Catalog and ordering integration

Implement gated analyte/profile maintenance, map M12 simple/specialised test contracts, redirect future range source, integrate M02 profile ordering and M04 individual snapshot use. Profile order persists only Profile.FixedPrice via the calculator; manual order passes individual unit pricing through that calculator.

**VG-02:** Prove manual/profile central pricing, individual historical freeze, catalog integrity, and no new live Test.ReferenceRange result/report read. Build and full suite green.

### 10-Stage Progress — S2

- [x] Stage 1 — Pre-Execution Verification: HEAD 2283841 green (1282/0).
- [x] Stage 2 — Deep Understanding: re-read M-05.md §4.3 + VG-02.
- [x] Stage 3 — File Analysis: read the four M-04 files (EnterResult, RefreshResultReferenceRange, GetResultEntry, ResultFlagComputer), Test/Analyte/Profile/ProfileAnalyte/ReferenceRange/ReferenceRangeSnapshot/PatientTestReferenceRangeSnapshot, PatientTest entered state, TestPriceResolver, AddTestsToVisit (handler/command/validator), CreateTest/UpdateTest commands, PatientRegistrationAccessPolicy, FakeApplicationDbContext, Application.test conventions; confirmed fake is list-based (no EF Include), ReferenceRangeSnapshot is a public positional record, InternalsVisibleTo enables internal band test.
- [x] Stage 4 — Planning: plan below.
- [x] Stage 5 — Execution: shared ResultReferenceRangeSource (AnalyteId-resolved band loader returning null→legacy path + Capture building M-04 snapshot from band or legacy range); EnterResult/Refresh resolve Test.AnalyteId → analyte bands → flag+snapshot (mapped tests never fall back to live Test.ReferenceRange); GetResultEntry emits band-derived ReferenceRangeDtos for mapped tests; M12 CreateTestCommand gained trailing optional AnalyteId + MapTestToAnalyte/UnmapTestFromAnalyte commands (EDIT_SYSTEM_SETTINGS); M02 AddProfileToVisitCommand (TypedProfileOrdering, ADD_EDIT_PATIENT) stored only ProfileSelectionCharge(FixedPrice); AddTestsToVisit wraps resolved individual price in ManualSelectionCharge; new Features/AnalyteProfiles catalog (CreateAnalyte/UpdateAnalyte/DeactivateAnalyte/SaveAnalyteReferenceRange/CreateProfile/AddProfileAnalyte/RemoveProfileAnalyte + GetAnalyteDefinitions/GetProfileDefinitions queries, all EDIT_SYSTEM_SETTINGS; profile add/remove guarded to pre-results via PatientTest.EnteredAtUtc; CreateProfile requires ranged, active analytes and one profile per specialised test with immutable FixedPrice).
- [x] Stage 6 — Post-Execution Verification: `dotnet build TopLab.sln` 0/0 (2026-09-09).
- [x] Stage 7 — Validation Gate VG-02: full suite green (357+864+107 = 1328 passed / 0 failed; +46 new Application tests). Tests prove band flag parity (sex-narrowest-tiebreak + compute), mapped-entry uses bands not legacy rows, no-bands no-fallback, snapshot freeze survives later live band edits, refresh reselects/resnapshots, manual order stores calculator sum, profile order stores only FixedPrice, M12 mapping guards, catalog CRUD + definitions + pre-results profile composition guard. VG-02 met.
- [x] Stage 8 — Documentation update: stages + section below.
- [x] Stage 9 — Memory Status update.
- [x] Stage 10 — Git commit `[M-05] Slice 2/4: Catalog and ordering integration — loop-engineering`.

**S2 Execution plan (Stage 5):**
1. Features/ResultsEntry/Common/ResultReferenceRangeSource.cs — LoadAnalyteBands (null→legacy, else band list) + Capture (band match → PatientTestReferenceRangeSnapshot.FromSnapshot(ReferenceRangeSnapshot(testId, band fields)) ; legacy → range.CaptureSnapshot()).
2. ResultFlagComputer — add AnalyteReferenceRangeBand SelectMatch/Compute overloads reusing the exact M-04 SelectBest ordering (sex match first, narrowest AgeMax−AgeMin, lowest Id); legacy ReferenceRange overloads untouched.
3. EnterResult/RefreshResultReferenceRange — branch on Test.AnalyteId; mapped uses bands, unmapped legacy; flag recompute + snapshot capture share the helper.
4. GetResultEntry — mapped test returns band-derived ReferenceRangeDto list.
5. M12 — CreateTestCommand `int? AnalyteId = null` + handler explicitly maps only simple tests (MapToAnalyte guard); new MapTestToAnalyte/UnmapTestFromAnalyte commands (EDIT_SYSTEM_SETTINGS).
6. M02 — AddProfileToVisit typing (profile-FixedPrice only) + AddTestsToVisit central ManualSelectionCharge wrap.
7. AnalyteProfiles catalog feature + validators + definitions queries.
8. 46 new Application tests (band parity, mapped entry/no-fallback/freeze/refresh/get, central pricing both paths, mapping guards, catalog CRUD/definitions/profile-composition guard).

## Slice 3: Profile result and amendment surface

Implement configured-analyte DTO/query/save/report/print flow, frozen snapshot rendering, lifecycle reuse, central balance loader, amendment command (EDIT_RESULTS) and audit query (PT_AUDIT_ACCESS). One relational save atomically persists active amendment and immutable audit record.

**VG-03:** Prove freeze/reprint, entry/lifecycle/balance, authorised printed amendment, immediate active value/no unprint/no version, audit content/immutability/restricted read, and rollback. Build and full suite green.

### 10-Stage Progress — S3

- [x] Stage 1 — Pre-Execution Verification: HEAD 17ba790 green (1328/0).
- [x] Stage 2 — Deep Understanding: re-read M-05.md §4.4 + §4.6 (frozen messages) + VG-03.
- [x] Stage 3 — File Analysis: read ProfileResultItem (Update/Verify/Unverify/MarkPrinted/Amend guards + messages), ProfileResultItemReferenceRangeSnapshot.Create signature, ProfileResultAmendment.Create (immutability + reason guard), PatientTest lifecycle methods incl. EnterResult(null) notes mapping, ProfileResultFlag (Low=0/High=1 only), ResultsEntryAccessPolicy, MarkResultPrintedCommandHandler per-user balance block, and M-04 test conventions (FakeCurrentUserService, balance seeding, SaveChangesCallCount).
- [x] Stage 4 — Planning: plan below.
- [x] Stage 5 — Execution: Features/ProfileResults — Common ProfileResultDtos (grid/report/line/amendment DTOs + internal ProfileResultReferenceRangeCapture reusing ResultFlagComputer band SelectMatch + FindProfile + ConfiguredAnalytes by ProfileAnalyte order); GetProfileEntryGrid (edit surface, frozen ranges from snapshot only); GetProfileReport (snapshot-only ranges; notes/verified/print header; no live-range resolution); GetProfileResultAmendmentsQuery (PT_AUDIT_ACCESS; ordered AmendedAtUtc then Id); SaveProfileResultsCommand (non-empty unique subset of configured analytes; pre-review only via EnterResult(null,..) guard; comment→Notes + EnteredAtUtc; wholesale draft replacement never touching printed rows; per-item band snapshot captured in the same save); Verify/UnverifyProfileResults (reuse MarkReviewed/Unreview + item Verify/Unverify; per-item print-safe scope); MarkProfilePrintedCommand (not-entered/not-reviewed guard + BalanceProbe delegate + exact M-04 per-user balance block; marks order + items); AmendProfileResultCommand (EDIT_RESULTS; missing target → `نتيجة البروفايل غير موجودة.`; printed-only; updates active row + complete immutable audit row, one SaveChangesAsync). Feature-local DomainFailureTranslator added (ambiguity resolved via alias in the print handler).
- [x] Stage 6 — Post-Execution Verification: `dotnet build TopLab.sln` 0/0 (2026-09-09).
- [x] Stage 7 — Validation Gate VG-03: full suite green (357+904+107 = 1368 passed / 0 failed; +40 new Application tests). Tests prove configured-analyte-only entry (non-configured/empty/duplicate guards), per-item snapshot capture incl. no-band → no snapshot, entry state/comment mapping, draft replacement keeping printed rows' identity, grid/report frozen rendering, reprint after live band change still shows frozen values, verify/unverify lifecycle reuse + printed-guard, print central balance block (block/absolute bypass/flag-off), authorised printed amendment without unprint/new version with immediate active value + full audit content (one save), audit row ordering + append-only history. Relational amendment+audit rollback on forced failure is a Slice 4 relational test. VG-03 met.
- [x] Stage 8 — Documentation update: stages + section below.
- [x] Stage 9 — Memory Status update.
- [x] Stage 10 — Git commit `[M-05] Slice 3/4: Profile entry, reporting, print, and post-print amendment surface — loop-engineering`.

**S3 Execution plan (Stage 5):**
1. Features/ProfileResults/Common/ProfileResultDtos.cs — DTOs (FrozenProfileRangeDto, ProfileEntryItemDto, ProfileEntryGridDto, ProfileReportLineDto, ProfileReportDto, ProfileAmendmentDto) + ProfileResultReferenceRangeCapture (Capture/FindProfile/ConfiguredAnalytes) + feature-local DomainFailureTranslator.
2. Queries: GetProfileEntryGrid (configured analytes + current items merged by AnalyteId), GetProfileReport (snapshot-only frozen ranges; verified header/comment/print fields), GetProfileResultAmendments (PT_AUDIT_ACCESS, timestamp-then-id ordering).
3. Commands: SaveProfileResults (+ validator), VerifyProfileResults, UnverifyProfileResults, MarkProfilePrinted (thin balance loader + per-user balance block), AmendProfileResult (+ validator; single SaveChangesAsync).
4. 40 new Application tests across authorization (8 gates), save (7), grid (3), report (3), verify/unverify (4), print (6), amend (6), audit query (2).

## Slice 4: Infrastructure proof and close-out

Prove migration, mapping, historical output, calculator, transaction and authorization; append ADR, tracking and handoff.

**VG-04:** Release build zero warnings/errors, full suite green, migration applies from HEAD with zero drift, all owner-decision tests pass, and no Presentation edits exist.

## Current Status

- Overall: **3/4 slices done — Slice 4 next (Infrastructure proof and close-out)**
- Slice 1: [x] Done — VG-01 green; committed `[M-05] Slice 1/4: Domain and schema foundation — loop-engineering`
- Slice 2: [x] Done — VG-02 green; committed `[M-05] Slice 2/4: Catalog and ordering integration — loop-engineering`
- Slice 3: [x] Done — VG-03 green; committed `[M-05] Slice 3/4: Profile entry, reporting, print, and post-print amendment surface — loop-engineering`
- Slice 4: [ ] Not started
- Blocking decisions: none

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-08 | 0 | — | Original memory created | Superseded by binding owner decisions | — |
| 2026-09-09 | 0 | Planning | Re-verified HEAD and rewrote M05 plan/memory for owner decisions and stale-baseline defects | Execution-ready | — |
| 2026-09-09 | 1 | 1–10 | Domain/schema foundation + backfill migration; full suite 1282 green; VG-01 met | Done | `[M-05] Slice 1/4: Domain and schema foundation — loop-engineering` |
| 2026-09-09 | 2 | 1–10 | Catalog + ordering integration (analyte profile feature, M12 mapping, profile/manual central pricing, M04 band redirect); full suite 1328 green; VG-02 met | Done | `[M-05] Slice 2/4: Catalog and ordering integration — loop-engineering` |
| 2026-09-09 | 3 | 1–10 | Profile entry/report/print/amendment surface (DTOs + capture helper, grid/report/amendments queries, save/verify/unverify/print/amend commands, feature-local failure translator); full suite 1368 green (904 Application, +40); VG-03 met | Done | `[M-05] Slice 3/4: Profile entry, reporting, print, and post-print amendment surface — loop-engineering` |

## Stop Report

Stop migration execution if a legacy ProfileResultItem name cannot be mapped deterministically to a configured Analyte. Record affected rows and request an execution addendum; do not guess clinical identity. This is a validation stop condition, not an open design decision.
