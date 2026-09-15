# Loop Engineering — Memory File

- **Module:** Sent-Out Samples (M-16)
- **Module Number:** M-16
- **Source Plan:** Docs/OpenCode/M-16.md
- **Date Created:** 2026-09-15
- **Total Slices:** 4
- **Current Slice:** done — all 4 slices complete, VG-01/VG-02/VG-03/VG-04 passed
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Delivers the sent-out-samples workflow over the already-complete physical schema (zero migration): additive Domain guards on the two factory-only entities (`SentOutSample` non-negative prices, `SentOutSamplePayment` positive amount) plus the single-sourced `SentOutAccountCalculator` (TotalCost / TotalPaid / Remaining / IsFullySettled per Data Model §8.3); the full `Features/SentOutSamples/` Application surface — dispatch with eligibility/type/duplication guards and auto-captured editable pricing (OD-16-D), partial payment «ترسل إلى» capped at the remaining, «خلاص» full settlement, period query with optional entity filter, and per-lab account query; all writes gated on `CASH_DISBURSE_DEPOSIT` (OD-16-C), reads open. Per-sample lab binding (OD-16-A); no cancel/re-send in v1 (OD-16-B). Zero Presentation content. Done means: S1 Domain plus tests, S2 writes plus tests, S3 reads plus tests, S4 persistence proof (Cascade + Restrict pins) plus zero-drift gate plus ADR-0042 plus tracking flip plus handoff plus full-suite green plus coverage floors.

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
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/model-assertion) replaces any standard UI journey — M-16 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-16] Slice N/4: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-16's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain: `src/TopLab.Domain` builds zero/zero; `SentOutSampleTests` + `SentOutSamplePaymentTests` + `SentOutAccountCalculatorTests` plus all Domain tests green; every guard negative-pathed (negative cost/patient price; zero/negative payment); calculator branches covered (partial / over / exact / empty); Domain `SentOutSamples` coverage ≥ 90% | `dotnet build src/TopLab.Domain`; `dotnet test tests/TopLab.Domain.Tests`; coverlet module filter |
| 2 | VG-02 | Application writes: `src/TopLab.Application` builds zero/zero; all S2 handler/validator/authorization tests green (full dispatch guard matrix with verbatim messages, default-price capture + override, over-remaining rejection, «خلاص» exact settlement + nothing-due Conflict, audit fields); grep gate: no handler restates the settlement formula (all via `SentOutAccountCalculator`); Application S2 footprint coverage ≥ 80% | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 3 | VG-03 | Application reads: Application builds zero/zero; S3 tests green (period inclusive both ends, entity filter only-own-samples, name resolution, empty-set, `From > To` validator, lab-account number-for-number vs the calculator, unknown/non-lab entity rejections); grep gate: zero `IAuthorizedRequest` under `Features/SentOutSamples/Queries`; Application S3 footprint coverage ≥ 80% | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 4 | VG-04 | Persistence + close-out: Release build zero/zero; full suite green (`-m:1`); `SentOutSamplePersistenceTests` green (store/retrieve + Cascade payment→sample + Restrict sample→entity); `dotnet ef migrations has-pending-model-changes` → no changes; snapshot unchanged; validator-registration extension green; coverage floors met or waived; audit gate passed; ADR-0042 appended; M16 tracking row flipped; `Handoff_M16.md` per template; zero cancel/void path (grep gate); zero Presentation content (grep gate) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain: enrich the two sent-out entities with guards + the settlement calculator | [x] Done | VG-01 |
| 2 | Application write surface: dispatch + partial payment + full settlement | [x] Done | VG-02 |
| 3 | Application read surface: period query + per-lab account query with entity filtering | [x] Done | VG-03 |
| 4 | Tests + Infrastructure proof + close-out | [x] Done | VG-04 |

---

## Slice 1: Domain: enrich the two sent-out entities with guards + the settlement calculator

- **Goal:** Add the non-negative price guards to `SentOutSample.Create`, the positive-amount guard to `SentOutSamplePayment`, and create the static `SentOutAccountCalculator` (the single source of the §8.3 formula) — additively, with no breaking change.
- **Touches:** `src/TopLab.Domain/SentOutSamples/SentOutSample.cs` (modify: additive guards only); `src/TopLab.Domain/SentOutSamples/SentOutSamplePayment.cs` (modify: additive guard only); `src/TopLab.Domain/SentOutSamples/SentOutAccountCalculator.cs` (create); `tests/TopLab.Domain.Tests/SentOutSamples/SentOutSampleTests.cs` (create); `tests/TopLab.Domain.Tests/SentOutSamples/SentOutSamplePaymentTests.cs` (create); `tests/TopLab.Domain.Tests/SentOutSamples/SentOutAccountCalculatorTests.cs` (create)
- **Validation Gate:** VG-01 — Domain build zero/zero; all Domain tests green; every guard negative-pathed; coverage ≥ 90%.

### 10-Stage Progress (Slice 1 — evidence 2026-09-15)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` 0 warnings/0 errors; `dotnet test TopLab.sln` 1632 passed (Domain 378 + Infra 145 + App 1109), 0 failed.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1 re-read; §8.3 formula; additive-only guards; English ArgumentException precedent.
- [x] **Stage 3 — File Analysis:** `SentOutSample.cs` (Create-only 6-35), `SentOutSamplePayment.cs` (6-32), `PatientAccountCalculator.cs` style precedent, `Test.cs:188-196` + `PaymentOperation.cs:65-74` guard precedents, `PatientAccountCalculatorTests.cs` xUnit conventions; no `Domain.Tests/SentOutSamples/` folder — created.
- [x] **Stage 4 — Planning:** Guards into both factories → calculator (4 static members) → 3 test classes.
- [x] **Stage 5 — Execution:** Implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build src/TopLab.Domain` 0/0; `dotnet test tests/TopLab.Domain.Tests` 390 passed (378 + 12 new), 0 failed.
- [x] **Stage 7 — Validation Gate:** VG-01 passed — build 0/0; every guard negative-pathed (cost/patient negative; zero/negative payment); calculator partial/over/exact/empty covered; coverlet footprint line-rate Calculator 1.0 / Sample 0.88 / Payment 0.85, branch 1.0 all (uncovered = private EF ctors only).
- [x] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 2: Application write surface: dispatch + partial payment + full settlement

- **Goal:** Ship the three `CASH_DISBURSE_DEPOSIT`-gated commands: `SendSampleOut` (eligibility/type/duplication guards + default-price capture with override), `RecordSentOutPayment` («ترسل إلى», capped at remaining), `SettleSentOutInFull` («خلاص», exact remaining), with the full test matrix.
- **Touches:** `src/TopLab.Application/Features/SentOutSamples/Common/SentOutSampleDtos.cs` (create); `.../Common/SentOutSamplesAccessPolicy.cs` (create or reuse `PatientBillingAccessPolicy.CashDisburseDeposit` — same code value either way); `.../Common/DomainFailureTranslator.cs` (create); `.../Commands/SendSampleOut/` (3 files, create); `.../Commands/RecordSentOutPayment/` (3 files, create); `.../Commands/SettleSentOutInFull/` (3 files, create); `tests/TopLab.Application.Tests/Features/SentOutSamples/SendSampleOutCommandHandlerTests.cs` (create); `.../RecordSentOutPaymentCommandHandlerTests.cs` (create); `.../SettleSentOutInFullCommandHandlerTests.cs` (create); `.../SentOutSamplesAuthorizationTests.cs` (create); `FakeApplicationDbContext` (verify-only / extend on proven gap)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green; formula-single-source grep gate; coverage ≥ 80%.

### 10-Stage Progress (Slice 2 — evidence 2026-09-15)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` 0/0; `dotnet test TopLab.sln` full suite green (Domain 390 + Infra 145 + App 1109).
- [x] **Stage 2 — Deep Understanding:** Plan §6 S2 re-read; FR-M16-001/003; EC-01…EC-12/17; OD-16-A/C/D settled; verbatim Appendix A messages.
- [x] **Stage 3 — File Analysis:** `Test.cs` (IsSentOut/Cost/Price), `EntityType.cs` (PartnerLab=2), `DeleteExternalEntityCommandHandler.cs:34-37`, `PatientBillingAccessPolicy.cs:12`, `CreateAntibioticCommandHandler` (translator pattern), `FakeApplicationDbContext` (proven gap: no `SentOutSamplePayments` set — extended with List + Set/Add/Remove routing), `FakeCurrentUserService`/`FakeDateTimeProvider`, `Error`/`Result` API, `AuthorizationBehavior` + `ExternalEntitiesAuthorizationTests` precedent, `Create(0)` IDENTITY sentinel verified in both EF configurations.
- [x] **Stage 4 — Planning:** DTOs (list + account) → access policy (feature-local const, same code value) → translator → SendSampleOut (guard chain + price defaulting) → RecordSentOutPayment (remaining cap) → SettleSentOutInFull (exact remaining) → 4 test classes.
- [x] **Stage 5 — Execution:** Implemented per plan (U1 decided: new `SentOutSamplesAccessPolicy` class, identical code value).
- [x] **Stage 6 — Post-Execution Verification:** Application build 0/0; S2 filter 30/30 green; full Application suite 1139/1139 green.
- [x] **Stage 7 — Validation Gate:** VG-02 passed — dispatch guard matrix verbatim (8 negatives); default-capture + override; over-remaining Conflict; «خلاص» exact + nothing-due Conflict; audit fields asserted; formula grep clean (calculator only); no cancel/void path (false-positive-free); coverlet footprint 85.2% ≥ 80%.
- [x] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 3: Application read surface: period query + per-lab account query with entity filtering

- **Goal:** Ship the two open reads: `GetSentOutSamples` (period on `SentAtUtc`, optional entity filter, resolved names) and `GetSentOutLabAccount` (period dispatch count + calculator totals).
- **Touches:** `src/TopLab.Application/Features/SentOutSamples/Queries/GetSentOutSamples/` (3 files, create); `.../Queries/GetSentOutLabAccount/` (3 files, create); `tests/TopLab.Application.Tests/Features/SentOutSamples/GetSentOutSamplesQueryHandlerTests.cs` (create); `.../GetSentOutLabAccountQueryHandlerTests.cs` (create)
- **Validation Gate:** VG-03 — Application build zero/zero; S3 tests green; open-reads grep gate; coverage ≥ 80%.

### 10-Stage Progress (Slice 3 — evidence 2026-09-15)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` 0/0; `dotnet test TopLab.sln` full suite green (Domain 390 + Infra 145 + App 1139).
- [x] **Stage 2 — Deep Understanding:** Plan §6 S3 re-read; FR-M16-002/004; EC-13/14/15; inclusive UTC-calendar-day periods; default-today-UTC when omitted (M-11 precedent); open reads; totals via calculator only.
- [x] **Stage 3 — File Analysis:** `GetVisitHistoryQueryHandler` (existence-guard + sync-Task pattern), `SearchExternalEntitiesQueryHandler` (IQueryable composition + Skip/Take + dictionary name resolution, no Include/AsNoTracking — fake-compatible), `SearchExternalEntitiesQueryValidator` (paging messages), `GetResultWorklistQuery/Handler` (DateOnly period + default-today idiom); `Patient.FullName`/`Test.Name`/`ExternalEntity.Name` verified.
- [x] **Stage 4 — Planning:** `GetSentOutSamples` (+validator) → `GetSentOutLabAccount` (+validator) → 2 test classes. Periods as DateTime half-open bounds (SQL-translatable, fake-compatible, semantically inclusive); default-today via `IDateTimeProvider` (testable); per-sample paid via grouped payments + calculator.
- [x] **Stage 5 — Execution:** Implemented per plan (6 source files + 2 test files).
- [x] **Stage 6 — Post-Execution Verification:** Application build 0/0; SentOutSamples filter 43/43 green; full Application suite 1152/1152 green.
- [x] **Stage 7 — Validation Gate:** VG-03 passed — period inclusive both ends (+boundary instants); entity filter only-own; name resolution; empty-set; default-today; paging; lab-account number-for-number vs calculator; unknown → NotFound / non-lab → Conflict; zero `IAuthorizedRequest` under Queries (grep); no formula restatement (grep); coverlet S3 footprint 100% ≥ 80%.
- [x] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 4: Tests + Infrastructure proof + close-out

- **Goal:** Prove the module against the real model (Cascade + Restrict pins), run the zero-drift gate, extend validator registration, pass coverage/audit/slopwatch, and close out (ADR-0042, tracking flip, handoff).
- **Touches:** `tests/TopLab.Infrastructure.Tests/Persistence/SentOutSamplePersistenceTests.cs` (create); `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend); `Docs/Source/Top_Lab_ADR.md` (append ADR-0042 — reconfirm max ADR at execution; M-07/M-09 consume ADR-0040/0041 when they execute first); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M16 row + dated change-log row); `Docs/Handoff_M16.md` (create per template — includes the OD-16-A PRD-contradiction note and the OD-16-B deferral note)
- **Validation Gate:** VG-04 — Release build zero/zero; full suite green; zero-drift proven; persistence pins green; coverage floors or waivers; audit gate passed; no cancel/void path (grep); docs committed per convention; zero Presentation content (grep gate).

### 10-Stage Progress (Slice 4 — evidence 2026-09-15)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln -c Release` 0/0 before touching anything.
- [x] **Stage 2 — Deep Understanding:** Plan §7 S4 re-read; drift → stop + addendum (never silent migration); ADR-0042 contents; close-out convention (tracking + change-log + handoff).
- [x] **Stage 3 — File Analysis:** Both EF configurations (Cascade payment→sample line 18; Restrict sample→entity line 20); `InMemoryContextFactory` + `PaymentOperationPersistenceTests` (InMemory-on-real-context pattern); `ExternalEntityDeleteBehaviorTests` (model-level FK pin precedent); `ExternalEntityAuditGateTests` (interceptor audit pattern); `ValidatorRegistrationTests` (per-module theory pattern); ADR max reconfirmed = 0041 → ADR-0042; M16 tracking row + §9 log format; `Handoff_M09.md` structure precedent.
- [x] **Stage 4 — Planning:** Drift gate first → persistence tests → validator-reg extension → audit gate → Release full suite → ADR → tracking → handoff.
- [x] **Stage 5 — Execution:** Implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** Release build 0/0; Release full suite 1696/1696 green `-m:1` (390 + 1157 + 149); drift gate → no changes; snapshot untouched.
- [x] **Stage 7 — Validation Gate:** VG-04 passed — persistence 4/4 (store/retrieve + calculator agreement + audit columns; behavioral Cascade; model Cascade + Restrict pins); validator-reg 5/5; audit gate (interceptor `CreatedByUserId`/`CreatedAtUtc` + handler `PerformedByUserId`/`PaidAtUtc` from S2); slopwatch clean; no cancel/void path (grep); zero Presentation content (grep); diff confined to `Domain/SentOutSamples`, `Application/Features/SentOutSamples`, `tests/**`, `Docs/**`; coverage waiver recorded in handoff (M-11/M-14 precedent).
- [x] **Stage 8 — Documentation Update:** ADR-0042 appended; M16 row flipped 🟩 Done + §6 block + dated §9 row; `Docs/Handoff_M16.md` created per template (OD-16-A contradiction + OD-16-B deferral + InMemory key-generation deviation + M-14 consumer note); this checklist recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated; module close-out recorded.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Current Status

- Overall: 4/4 slices done — MODULE COMPLETE
- Slice 1 — Domain: sent-out guards + settlement calculator: [x] Done (VG-01 passed 2026-09-15)
- Slice 2 — Application write surface: dispatch + partial payment + full settlement: [x] Done (VG-02 passed 2026-09-15)
- Slice 3 — Application read surface: period query + per-lab account query: [x] Done (VG-03 passed 2026-09-15)
- Slice 4 — Tests + Infrastructure proof + close-out: [x] Done (VG-04 passed 2026-09-15)

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-15 | 0 | — | Memory file created | OK | — |
| 2026-09-15 | 1 | 1–10 | S1 Domain guards + calculator; build 0/0; Domain 390 green; VG-01 passed | OK | 3b499c5 |
| 2026-09-15 | 2 | 1–10 | S2 writes + 30 tests; App 1139 green; formula grep clean; footprint 85.2%; VG-02 passed | OK | f6db46d |
| 2026-09-15 | 3 | 1–10 | S3 reads + 13 tests; App 1152 green; open-reads grep clean; S3 footprint 100%; VG-03 passed | OK | 9d4c88a |
| 2026-09-15 | 4 | 1–10 | S4 persistence 4/4 + reg 5/5; Release 1696 green; drift clean; ADR-0042 + tracking + handoff; VG-04 passed | OK | ae35175 |

## Stop Report (append only if a stop condition triggers)
