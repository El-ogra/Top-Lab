# Loop Engineering — Memory File

- **Module:** Statistics (M-19)
- **Module Number:** M-19
- **Source Plan:** Docs/OpenCode/M-19.md
- **Date Created:** 2026-09-15
- **Total Slices:** 4
- **Current Slice:** S1 — completed; S2 next
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Delivers the four statistics surfaces as `STATISTICS`-gated (seeded id=12), read-only projections over the already-complete physical schema (zero migration, zero Domain change): patient counts classified by sex / referral entity / account type with optional monthly breakdown (soft-deleted excluded); test counts per test and per test group with optional group filter; sent-out statistics per destination lab with totals via the existing `SentOutAccountCalculator` (never restated); user productivity from `PatientTest` attribution timestamps (no M-18 coupling). Periods use DateTime half-open UTC bounds with default-today. Names via dictionary lookups with raw-id fallback. Zero Presentation content. Done means: S1 patient statistics plus tests, S2 test statistics plus tests, S3 sent-out + productivity plus tests, S4 zero-drift gate plus validator registration plus ADR-0045 plus tracking flip plus handoff plus full-suite green plus coverage floors.

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
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/model-assertion) replaces any standard UI journey — M-19 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-19] Slice N/4: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-19's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Patient statistics: `src/TopLab.Application` builds zero/zero; S1 tests green (each classification number-for-number, monthly buckets, soft-deleted excluded, null-referral bucket, default-today, `From > To` validator, empty period, authorization theory shell with verbatim denial + absolute bypass); zero `Persistence/**` diff (grep gate); Application S1 footprint coverage ≥ 80%. **Migration: none required by this slice** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 2 | VG-02 | Test statistics: Application builds zero/zero; S2 tests green (per-test + per-group number-for-number, group filter, deleted-patient exclusion, empty-set, validator); theory extended; zero `Persistence/**` diff (grep gate); Application S2 footprint coverage ≥ 80%. **Migration: none required by this slice** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 3 | VG-03 | Sent-out + productivity: Application builds zero/zero; S3 tests green (per-lab totals number-for-number vs `SentOutAccountCalculator`, lab filter, per-timestamp attribution counts, zero-activity omission, raw-id fallback); grep gate: no settlement-formula restatement; Application S3 footprint coverage ≥ 80%. **Migration: none required by this slice** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 4 | VG-04 | Zero-drift + close-out: Release build zero/zero; full suite green (`-m:1`); `dotnet ef migrations has-pending-model-changes` → no changes; snapshot unchanged; validator-registration extension green; coverage floors met or waived; ADR-0045 appended; M19 tracking row flipped; `Handoff_M19.md` per template; zero writes / zero Domain changes / zero Presentation content (grep gates) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Patient statistics (FR-M19-001/005) | [x] Done | VG-01 PASS |
| 2 | Test statistics (FR-M19-002/005) | [ ] Pending | VG-02 |
| 3 | Sent-out + user-productivity statistics (FR-M19-003/004) | [ ] Pending | VG-03 |
| 4 | Tests + zero-drift proof + close-out | [ ] Pending | VG-04 |

---

## Slice 1: Patient statistics (FR-M19-001/005)

- **Goal:** Ship the `STATISTICS`-gated `GetPatientCountStatisticsQuery` with the three classifications, the optional monthly breakdown, soft-delete exclusion, and the authorization theory shell.
- **Touches:** `src/TopLab.Application/Features/Statistics/Common/StatisticsDtos.cs` (create); `.../Common/StatisticsAccessPolicy.cs` (create); `.../Queries/GetPatientCountStatistics/` (3 files, create); `tests/TopLab.Application.Tests/Features/Statistics/GetPatientCountStatisticsQueryHandlerTests.cs` (create); `.../StatisticsAuthorizationTests.cs` (create)
- **Validation Gate:** VG-01 — Application build zero/zero; S1 tests green; zero-Persistence grep gate; coverage ≥ 80%. Migration: none.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln --no-restore` 0/0; `dotnet test` full suite 1789/1789 green.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1 re-read; SD-19-1…SD-19-8; Appendix A messages applied verbatim.
- [x] **Stage 3 — File Analysis:** `Patient.cs` classification columns + SoftDelete; `PermissionConfiguration.cs:29` id=12; `AuthorizationBehavior`; `GetSentOutSamplesQueryHandler` period/dictionary precedent; `AuditAuthorizationTests` theory shell; Fake DB/clock/user services.
- [x] **Stage 4 — Planning:** DTOs → access policy → query + validator → handler → 2 test classes.
- [x] **Stage 5 — Execution:** Created Statistics Common + GetPatientCountStatistics (query/handler/validator) + handler tests + authorization theory shell. Fixed referral test to use TreatingDoctor (no PriceListId) and half-open bound at Mar3 00:00.
- [x] **Stage 6 — Post-Execution Verification:** Application build 0/0; Statistics filter 15/15; full Application suite 1237/1237.
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — zero Persistence/** and Domain/** diff; soft-deleted excluded; classifications number-for-number; From>To frozen message; default-today; empty period zeros.
- [x] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 2: Test statistics (FR-M19-002/005)

- **Goal:** Ship the gated `GetTestCountStatisticsQuery` with per-test and per-group counts, the optional group filter, and deleted-patient exclusion.
- **Touches:** `src/TopLab.Application/Features/Statistics/Common/StatisticsDtos.cs` (modify: add DTOs); `.../Queries/GetTestCountStatistics/` (3 files, create); `tests/TopLab.Application.Tests/Features/Statistics/GetTestCountStatisticsQueryHandlerTests.cs` (create)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green; zero-Persistence grep gate; coverage ≥ 80%. Migration: none.

### 10-Stage Progress (Slice 2)

- [ ] **Stage 1 — Pre-Execution Verification:** build + full suite green.
- [ ] **Stage 2 — Deep Understanding:** Plan §5 S2 re-read; SD-19-6; order-time = `CreatedAtUtc`.
- [ ] **Stage 3 — File Analysis:** `PatientTest.cs` (audit `CreatedAtUtc`), `Test.cs` (`TestGroupId`, `Name`), `TestGroup.cs` (`Name`); S1 DTO file + theory.
- [ ] **Stage 4 — Planning:** DTO additions → query + validator → handler → test class; extend the theory.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Application build 0/0; S2 filter green; full Application suite green.
- [ ] **Stage 7 — Validation Gate:** VG-02.
- [ ] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [ ] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 3: Sent-out + user-productivity statistics (FR-M19-003/004)

- **Goal:** Ship the two remaining gated reads: `GetSentOutStatistics` (per-lab counts + calculator totals) and `GetUserProductivityStatistics` (per-timestamp attribution counts).
- **Touches:** `src/TopLab.Application/Features/Statistics/Common/StatisticsDtos.cs` (modify: add DTOs); `.../Queries/GetSentOutStatistics/` (3 files, create); `.../Queries/GetUserProductivityStatistics/` (3 files, create); `tests/TopLab.Application.Tests/Features/Statistics/GetSentOutStatisticsQueryHandlerTests.cs` (create); `.../GetUserProductivityStatisticsQueryHandlerTests.cs` (create)
- **Validation Gate:** VG-03 — Application build zero/zero; S3 tests green; calculator-single-source grep gate; coverage ≥ 80%. Migration: none.

### 10-Stage Progress (Slice 3)

- [ ] **Stage 1 — Pre-Execution Verification:** build + full suite green.
- [ ] **Stage 2 — Deep Understanding:** Plan §5 S3 re-read; SD-19-4/5; calculator reuse rule.
- [ ] **Stage 3 — File Analysis:** `SentOutAccountCalculator` API, `SentOutSample`/`SentOutSamplePayment` shapes, `PatientTest` attribution columns, `GetSentOutLabAccountQueryHandler` (calculator-consumption precedent).
- [ ] **Stage 4 — Planning:** DTO additions → 2 queries + validators → 2 handlers → 2 test classes; complete the theory.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Application build 0/0; S3 filter green; full Application suite green.
- [ ] **Stage 7 — Validation Gate:** VG-03.
- [ ] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [ ] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 4: Tests + zero-drift proof + close-out

- **Goal:** Prove zero drift, extend validator registration, pass coverage/slopwatch, and close out (ADR-0045, tracking flip, handoff).
- **Touches:** `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend); `Docs/Source/Top_Lab_ADR.md` (append ADR-0045 — reconfirm max ADR at execution; M-10/M-18 consume 0043/0044 when they execute first); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M19 row + dated change-log row); `Docs/Handoff_M19.md` (create per template)
- **Validation Gate:** VG-04 — Release build zero/zero; full suite green; zero-drift proven; docs committed per convention; zero writes/Domain/Presentation content (grep gates).

### 10-Stage Progress (Slice 4)

- [ ] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln -c Release` 0/0 before touching anything.
- [ ] **Stage 2 — Deep Understanding:** Plan §6 S4 re-read; drift → stop + addendum (never silent migration); ADR-0045 contents; close-out convention.
- [ ] **Stage 3 — File Analysis:** `ValidatorRegistrationTests` per-module theory pattern; ADR max reconfirmed; M19 tracking row + §9 log format; `Handoff_M16.md` structure precedent.
- [ ] **Stage 4 — Planning:** Drift gate first → validator-reg extension → Release full suite → ADR → tracking → handoff.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Release build 0/0; Release full suite green `-m:1`; drift gate → no changes; snapshot untouched.
- [ ] **Stage 7 — Validation Gate:** VG-04.
- [ ] **Stage 8 — Documentation Update:** ADR-0045 appended; M19 row flipped 🟩 Done + dated §9 row; `Docs/Handoff_M19.md` created per template; this checklist recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Current Status

- Overall: 1/4 slices done — S1 COMPLETE
- Slice 1 — Patient statistics: [x] Done (VG-01 PASS)
- Slice 2 — Test statistics: [ ] Pending
- Slice 3 — Sent-out + user-productivity statistics: [ ] Pending
- Slice 4 — Tests + zero-drift proof + close-out: [ ] Pending

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-15 | 0 | — | Memory file created | OK | — |
| 2026-09-15 | 1 | 1 | Pre-exec: build 0/0 + full suite 1789/1789 | OK | — |
| 2026-09-15 | 1 | 2-5 | Implemented S1 Statistics patient-count surface | OK | — |
| 2026-09-15 | 1 | 6-7 | App 0/0; S1 15/15; full App 1237/1237; VG-01 PASS | OK | — |
| 2026-09-15 | 1 | 10 | Local commit Slice 1/4 | OK | (this commit) |

## Stop Report (append only if a stop condition triggers)
