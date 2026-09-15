# Loop Engineering — Memory File

- **Module:** Result Delivery & Settlement at Handover (M-09)
- **Module Number:** M-09
- **Source Plan:** Docs/OpenCode/M-09.md
- **Date Created:** 2026-09-15
- **Total Slices:** 3
- **Current Slice:** 3 — not started (S2 committed)
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Delivers the delivery-handover aggregation surface: the period-filtered undelivered-results list with per-visit aggregated status, the per-patient delivery grid with the frozen price column (`PriceAtOrderTime`), the financial position at handover delegated to M-03's reader and sign-derived from `Balance` exclusively, and the composite `DeliverWithSettlement` command (printed-only delivery with per-line audit; optional partial `Payment` or delegated "خلاص" full settlement; NO balance gate at delivery — BR-08). All four use cases gated on `DELIVER_RESULTS` (OD-09-A). Zero Domain changes; zero migration; zero Presentation content. Done means: S1 read surface plus tests, S2 composite command plus tests, S3 persistence proof plus zero-drift gate plus ADR-0041 plus tracking flip plus handoff plus full-suite green plus coverage floors.

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
- Execution order: strictly sequential S1 -> S2 -> S3, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/model-assertion) replaces any standard UI journey — M-09 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-09] Slice N/3: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-09's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Application read surface: `src/TopLab.Application` builds zero/zero; all S1 handler/validator/authorization tests green; grep gate: every S1 use case carries `IAuthorizedRequest` with `DELIVER_RESULTS`; grep gate: no live `Test` price read in the grid (frozen `PriceAtOrderTime` only); undelivered-list status matches `PatientStatusCalculator` verbatim; delivery account matches `GetPatientAccount` number-for-number; Application S1 footprint coverage ≥ 80% | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gates |
| 2 | VG-02 | Composite command: Application builds zero/zero; `DeliverWithSettlementCommandHandlerTests` green incl. unprinted-line Conflict (translated message), per-line audit fields, exact balance reduction on partial payment, settle-in-full zeroing + zero-balance Conflict, no-settlement no-change, delivery-with-balance no-gate pin, unauthorized Forbidden; mutual-exclusion validator; Application S2 footprint coverage ≥ 80% | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests` |
| 3 | VG-03 | Persistence + close-out: Release build zero/zero; full suite green (`-m:1`); `ResultDeliveryPersistenceTests` green (period filter + `IsDeleted` + frozen price + audit columns after save); `dotnet ef migrations has-pending-model-changes` → no changes; snapshot unchanged; coverage floors met or waived; audit gate passed; ADR-0041 appended; M09 tracking row flipped; `Handoff_M09.md` per template; zero Presentation content (grep gate) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Application read surface: undelivered list + delivery grid + delivery account | [x] Done (VG-01 passed) | VG-01 |
| 2 | Application: composite `DeliverWithSettlement` command | [x] Done (VG-02 passed) | VG-02 |
| 3 | Tests + Infrastructure proof + close-out | [ ] Pending | VG-03 |

---

## Slice 1: Application read surface: undelivered list + delivery grid + delivery account

- **Goal:** Expose the three `DELIVER_RESULTS`-gated reads: period-filtered undelivered list (per-visit status via `PatientStatusCalculator`), delivery grid with frozen `Price`, and delivery account delegated to M-03 with sign-derived remaining amounts.
- **Touches:** `src/TopLab.Application/Features/ResultDelivery/Common/ResultDeliveryDtos.cs` (create); `.../Common/ResultDeliveryAccessPolicy.cs` (create); `.../Queries/GetUndeliveredResults/` (3 files, create); `.../Queries/GetDeliveryGrid/` (3 files, create); `.../Queries/GetDeliveryAccount/` (2 files, create); `tests/TopLab.Application.Tests/Features/ResultDelivery/GetUndeliveredResultsQueryHandlerTests.cs` (create); `.../GetDeliveryGridQueryHandlerTests.cs` (create); `.../GetDeliveryAccountQueryHandlerTests.cs` (create); `.../ResultDeliveryAuthorizationTests.cs` (create); `FakeApplicationDbContext` (verify-only / extend on proven gap)
- **Validation Gate:** VG-01 — Application build zero/zero; S1 tests green; both grep gates clean; coverage ≥ 80%.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` 0 warnings/0 errors; `dotnet test TopLab.sln` 1581 green (Domain 378 + Infra 142 + App 1061). M-07 5/5 present on `main` (HEAD `b182645`).
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1 re-read; OD-09-A/OD-09-C settled; frozen messages; inclusive UTC-day bounds; default-today-UTC (M-11 precedent).
- [x] **Stage 3 — File Analysis:** `GetResultWorklistQueryHandler` (date filter + calculator rollup), `GetPatientAccountQueryHandler` + `PatientBillingReader` (internal, same-assembly delegation), `PatientStatusCalculator`, `ResultsEntryAccessPolicy.DeliverResults`, `AuthorizationBehavior` (verbatim denial), fakes + `ReviewPrintDeliverCommandHandlerTests` pattern, `Error`/`Result`, validator message conventions.
- [x] **Stage 4 — Planning:** DTOs → access policy → `ResultDeliveryPeriod` (M-11 mirror) → 3 queries (+2 validators) → 4 test classes.
- [x] **Stage 5 — Execution:** 11 source files + 4 test files (26 tests) implemented as planned. Account query delegates to `PatientBillingReader.ReadAccount`; grid `Price = PriceAtOrderTime` only.
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build src/TopLab.Application` 0/0; `dotnet test tests/TopLab.Application.Tests` 1087/1087 green (26 new).
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — 3/3 queries `IAuthorizedRequest` + `DELIVER_RESULTS`; grid grep = `PriceAtOrderTime` only; status-verbatim + account number-for-number + sign-matrix tests green; coverlet line-rate 0.857–1.000 on S1 footprint (≥80%).
- [x] **Stage 8 — Documentation Update:** This memory file updated with evidence.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated (S1 Done).
- [x] **Stage 10 — Git Commit (authorized local):** `[M-09] Slice 1/3: Application read surface: undelivered list + delivery grid + delivery account — loop-engineering` — on `main`, never pushed.

---

## Slice 2: Application: composite `DeliverWithSettlement` command

- **Goal:** Deliver the atomic handover command: printed-only per-line delivery with audit, optional partial `Payment` or delegated "خلاص" full settlement, single `SaveChanges`, no balance gate (pinned).
- **Touches:** `src/TopLab.Application/Features/ResultDelivery/Common/DomainFailureTranslator.cs` (create); `.../Commands/DeliverWithSettlement/` (3 files, create); `tests/TopLab.Application.Tests/Features/ResultDelivery/DeliverWithSettlementCommandHandlerTests.cs` (create); `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green incl. the no-gate pin and exact-balance assertions; coverage ≥ 80%.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Solution build 0/0; full suite green (App 1087 incl. S1's 26; Infra 142; Domain 378) on `main` @ `be21e9e`.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S2 re-read; BR-08 no-gate; OD-09-B (Payment-default + خلاص delegation); mutual-exclusion validator; frozen messages (`النتيجة غير مطبوعة.` ≠ ResultsEntry translator wording → own translator).
- [x] **Stage 3 — File Analysis:** `MarkResultDeliveredCommandHandler` (translator pattern + no-gate comment), `PatientTest.MarkDelivered` guard, `RecordPayment` handler (`Create(0)` sentinel + `_db.Add` write path), `SettleAccountInFull` handler (balance ≤ 0 → Conflict; `FullSettlement` op), `PatientAccountCalculator` oracle, fakes, `ValidatorRegistrationTests` M04-theory pattern.
- [x] **Stage 4 — Planning:** Translator → command (`IAuthorizedRequest<Result>`) → validator (non-empty list, positive amount, mutual exclusion) → handler (load → guard-translate → audit → optional settle → single save) → 15-case test class + translator tests + auth extension + M09 validator-registration theory.
- [x] **Stage 5 — Execution:** 4 source files + 1 test class (15 cases) + translator tests + auth/registration extensions as planned. Settlement reuses M-03 `PaymentOperation.Create` path; no formula duplication; no `BlockPrintOnRemainingBalance` read.
- [x] **Stage 6 — Post-Execution Verification:** Application build 0/0; full Application suite 1107+/1107 green (45 ResultDelivery incl. 2 translator tests).
- [x] **Stage 7 — Validation Gate:** VG-02 PASS — unprinted Conflict + translated message; audit fields asserted; partial payment exact reduction; settle-in-full zeroing + zero/negative Conflict; no-settlement no-change; no-gate pin; unauthorized Forbidden; coverlet handler 1.000/1.000, validator 1.000, translator both arms.
- [x] **Stage 8 — Documentation Update:** This memory file updated with evidence.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated (S2 Done).
- [x] **Stage 10 — Git Commit (authorized local):** `[M-09] Slice 2/3: Application: composite DeliverWithSettlement command — loop-engineering` — on `main`, never pushed.

---

## Slice 3: Tests + Infrastructure proof + close-out

- **Goal:** Prove the module against the real model (InMemory persistence tests), run the zero-drift gate, pass coverage/audit/slopwatch, and close out (ADR-0041, tracking flip, handoff).
- **Touches:** `tests/TopLab.Infrastructure.Tests/Persistence/ResultDeliveryPersistenceTests.cs` (create); `Docs/Source/Top_Lab_ADR.md` (append ADR-0041 — reconfirm max ADR at execution; M-07 consumes ADR-0040 when it executes first); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M09 row + dated change-log row); `Docs/Handoff_M09.md` (create per template)
- **Validation Gate:** VG-03 — Release build zero/zero; full suite green; zero-drift proven; coverage floors or waivers; audit gate passed; docs committed per convention; zero Presentation content (grep gate).

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Release build + full tests green (0/0). Record evidence.
- [ ] **Stage 2 — Deep Understanding:** Re-read plan §6 S3; migration-scope gate semantics (drift → stop + addendum, never silent migration); ADR-0041 contents (OD-09-A/B/C outcomes, no-gate restatement, zero-drift result); close-out convention.
- [ ] **Stage 3 — File Analysis:** Inspect `WorkSheetQueryPersistenceTests` (InMemory-on-real-context pattern), `InMemoryContextFactory`, `ApplicationDbContext.DbSets`, `Top_Lab_ADR.md` (confirm max ADR), `Top_Lab_Master_Tracking_Sheet.md` (locate M09 row), `Docs/Source/Top_Lab_Handoff_Template.md`, coverlet setup, an existing audit-gate test.
- [ ] **Stage 4 — Planning:** Migration-scope gate first → persistence tests (period filter + IsDeleted + frozen price + audit columns) → audit gate → Release build + full suite with coverage → ADR-0041 → tracking flip → handoff.
- [ ] **Stage 5 — Execution:** Implement the plan.
- [ ] **Stage 6 — Post-Execution Verification:** Release build 0/0; full suite green; `dotnet ef migrations has-pending-model-changes` → no changes; snapshot clean.
- [ ] **Stage 7 — Validation Gate:** VG-03 — all code/test gates pass; persistence tests green; coverage floors met or waived in the handoff; audit gate green; zero Presentation content in the diff.
- [ ] **Stage 8 — Documentation Update:** ADR-0041 appended; M09 row flipped 🟩 Done with dated change-log entry; `Docs/Handoff_M09.md` created per template; slice checkboxes marked.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-09] Slice 3/3: Tests + Infrastructure proof + close-out — loop-engineering` + `Stages 1-10 verified. Gate VG-03 passed.` — on `main`, never push.

---

## Current Status

- Overall: 2/3 slices done — S2 committed, S3 next
- Slice 1 — Application read surface: undelivered list + delivery grid + delivery account: [x] Done (VG-01 passed, 26 tests, committed)
- Slice 2 — Application: composite `DeliverWithSettlement` command: [x] Done (VG-02 passed, 17 tests + registration, committed)
- Slice 3 — Tests + Infrastructure proof + close-out: [ ] Pending

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-15 | 0 | — | Memory file created | OK | — |
| 2026-09-15 | 1 | 1–10 | S1 read surface implemented; VG-01 passed (build 0/0, 1087 app tests green incl. 26 new, grep gates clean, coverage 0.857–1.000) | OK | [M-09] Slice 1/3 |
| 2026-09-15 | 2 | 1–10 | S2 composite command implemented; VG-02 passed (build 0/0, 45 ResultDelivery tests green, no-gate grep clean, handler 1.000/1.000) | OK | [M-09] Slice 2/3 |

## Stop Report (append only if a stop condition triggers)
