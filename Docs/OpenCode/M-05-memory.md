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
| 2 | Catalog and ordering integration: authoritative analyte source and central selection pricing | [ ] Not started | VG-02 |
| 3 | Profile entry, reporting, print, and post-print amendment surface | [ ] Not started | VG-03 |
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

## Slice 3: Profile result and amendment surface

Implement configured-analyte DTO/query/save/report/print flow, frozen snapshot rendering, lifecycle reuse, central balance loader, amendment command (EDIT_RESULTS) and audit query (PT_AUDIT_ACCESS). One relational save atomically persists active amendment and immutable audit record.

**VG-03:** Prove freeze/reprint, entry/lifecycle/balance, authorised printed amendment, immediate active value/no unprint/no version, audit content/immutability/restricted read, and rollback. Build and full suite green.

## Slice 4: Infrastructure proof and close-out

Prove migration, mapping, historical output, calculator, transaction and authorization; append ADR, tracking and handoff.

**VG-04:** Release build zero warnings/errors, full suite green, migration applies from HEAD with zero drift, all owner-decision tests pass, and no Presentation edits exist.

## Current Status

- Overall: **1/4 slices done — Slice 2 next (Catalog and ordering integration)**
- Slice 1: [x] Done — VG-01 green; committed `[M-05] Slice 1/4: Domain and schema foundation — loop-engineering`
- Slice 2: [ ] Not started
- Slice 3: [ ] Not started
- Slice 4: [ ] Not started
- Blocking decisions: none

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-08 | 0 | — | Original memory created | Superseded by binding owner decisions | — |
| 2026-09-09 | 0 | Planning | Re-verified HEAD and rewrote M05 plan/memory for owner decisions and stale-baseline defects | Execution-ready | — |
| 2026-09-09 | 1 | 1–10 | Domain/schema foundation + backfill migration; full suite 1282 green; VG-01 met | Done | `[M-05] Slice 1/4: Domain and schema foundation — loop-engineering` |

## Stop Report

Stop migration execution if a legacy ProfileResultItem name cannot be mapped deterministically to a configured Analyte. Record affected rows and request an execution addendum; do not guess clinical identity. This is a validation stop condition, not an open design decision.
