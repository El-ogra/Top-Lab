# Loop Engineering — Memory File

- **Module:** Inventory & Lab Accounting (M-20, backend only)
- **Module Number:** M-20
- **Source Plan:** Docs/OpenCode/M-20.md
- **Date Created:** 2026-09-15
- **Total Slices:** 4
- **Current Slice:** S1 — completed; S2 next
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Delivers the Accounts backend surface of M-20 as `CASH_DISBURSE_DEPOSIT`-gated (seeded id=11) computed aggregates plus the cash-movement write surface over the already-complete physical schema (zero migration, zero schema change): S1 hardens `CashMovement.Create` with three guards (amount > 0, notes ≤ 500, occurredAtUtc set) — behavior-only, no mapped-shape change; S2 ships the read surface — `GetCashDrawerInventoryQuery` (the ten FR-M20-002 figures via `PatientAccountCalculator`/`SentOutAccountCalculator`, never restated), `GetElementInventoryQuery` (user/referral/doctor/account-type/sent-out × four report types), `GetPatientSamplesDetailQuery` (FR-M20-005 drill-down); S3 ships `RecordCashDepositCommand`/`RecordCashDisbursementCommand` (handler-stamped performer/timestamp), `ListCashMovementsQuery`, `GetCompanyDelegateAccountsQuery` (per-entity SD-20-9), plus the authorization theory; S4 is the zero-drift proof, validator-registration extension, coverage roll-up, ADR-0046, tracking flip, and `Handoff_M20.md`. Periods are uniform half-open UTC bounds with default-today and the frozen inverted-period message. Soft-deleted patients excluded. Names via dictionary lookups with raw-id fallback. Zero Presentation content. No M-23 coupling of any kind.

## Global Validation Gates

- Gate G0 (pre-execution): `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- Gate G1 (post-execution per slice): same as G0 plus the slice-specific gate listed in the table below.

## Stop/Continue Rule

After a slice completes (all 10 stages done), verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice, with no pause and no human confirmation required.
If any one fails → retry. If the SAME failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs 5 CONSECUTIVE times, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do NOT proceed past this point without owner review. Ordinary expected test failures caused by the current slice and resolved within the same correction cycle do NOT count as five separate failures.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: 5 consecutive failures for the same reason.
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/model-assertion) replaces any standard UI journey — M-20 has no UI in scope.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote, NEVER force-push, NEVER modify or rewrite remote history. Commit message format: `[M-20] Slice N/4: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-20's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | CashMovement guards: solution build zero/zero; Domain tests green (guard paramNames, valid round-trip); zero `Persistence/**` diff (grep gate); **Migration: NONE — zero-drift gate** (`has-pending-model-changes` → no changes; empty `Persistence/Migrations/` diff) | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Domain.Tests`; grep gate; ef drift check |
| 2 | VG-02 | Inventory read surface: Application build zero/zero; S2 tests green (every figure number-for-number on the worked example; each element kind × report type; NotFound paths; soft-deleted exclusion; empty period; authorization theory: verbatim denial + absolute bypass); zero `Persistence/**` + zero `Domain/**` diff; coverage ≥ 80%. **Migration: NONE — zero-drift gate** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gates; ef drift check |
| 3 | VG-03 | Cash writes + accounts: Application build zero/zero; S3 tests green (persisted-row field assertions incl. stamped performer/timestamp; validators; entity NotFound; movement list names + raw-id fallback; per-entity totals number-for-number; theory extended); grep gate: no formula restatement, zero `Persistence/**` diff; coverage ≥ 80%. **Migration: NONE — zero-drift gate** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gates; ef drift check |
| 4 | VG-04 | Zero-drift + close-out: Release build zero/zero; full suite green (`-m:1`); `dotnet ef migrations has-pending-model-changes` → no changes; snapshot unchanged; validator-registration extension green; coverage floors met or waived; ADR-0046 appended; M20 tracking row flipped; `Handoff_M20.md` per template; zero schema diff / zero Presentation content (grep gates) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | CashMovement domain guards + tests | [x] Done | VG-01 PASS |
| 2 | Inventory read surface (FR-M20-001/002/003/004/005) | [ ] Pending | VG-02 |
| 3 | Cash writes + company/delegate accounts (FR-M20-001/006) | [ ] Pending | VG-03 |
| 4 | Tests + zero-drift proof + close-out | [ ] Pending | VG-04 |

---

## Settled Decisions (from plan — binding)

- **SD-20-1:** Inventory figures are computed read-only aggregates — zero inventory storage (Data Model §11.2; M-19 precedent). Precedent-based.
- **SD-20-2:** Cash writes persist via the existing F5 `CashMovement` table; the module adds guards only — **no migration anywhere**. Precedent-based.
- **SD-20-3:** Every command and query is `IAuthorizedRequest` with `CASH_DISBURSE_DEPOSIT` (id=11); absolute bypass via pipeline; no second gate in handlers; `PermissionConfiguration` untouched; the secondary-password dialog is Presentation-side over the existing `VerifySecondaryPasswordQuery`. Precedent-based (M-03 scope-reading recorded in ADR).
- **SD-20-4:** Feature folder `Features/InventoryAndAccounting/`. Precedent-based (dependency map §3).
- **SD-20-5:** `PatientAccountCalculator` + `SentOutAccountCalculator` receive period-filtered full lists; formulas never restated (grep gate). Precedent-based (M-19 SD-19-5).
- **SD-20-6:** Half-open UTC period bounds; default-today; «بداية الفترة يجب ألا تتجاوز نهايتها.» on `From > To`. Precedent-based (M-11/M-16/M-19).
- **SD-20-7:** Soft-deleted patients excluded. Precedent-based (M-19 SD-19-3).
- **SD-20-8:** Commissions = `DiscountOrCommissionPercent/100 × Σ referred patients' PriceAtOrderTime` per entity. Consistency-based decision — no direct precedent found.
- **SD-20-9:** Company/delegate accounts = per-entity `CashMovement` aggregation by `RelatedExternalEntityId` + sent-out settlement position via calculator. Precedent-based (Data Model §11.3).
- **SD-20-10:** Ten-figure formula table fixed (see plan §Settled Decisions); SafeCash = Collected + Deposits − Disbursements and NetProfit = Collected − SentOutPaid − Disbursements are consistency-based — no direct precedent found; the rest are precedent-based via the calculators.
- **SD-20-11:** One element-inventory query with element + report-type selectors; names via dictionaries, raw-id fallback. Precedent-based.
- **SD-20-12:** FR-M20-005 drill-down is a per-patient detail query composing TotalSamples. Precedent-based.
- **SD-20-13:** `CashMovement.Create` guards (amount > 0, notes ≤ 500, timestamp set). Precedent-based (`PaymentOperation.Create`).

---

## Slice 1: CashMovement domain guards + tests

- **Goal:** Harden `CashMovement.Create` (SD-20-13) and pin the guards with Domain tests; prove zero persistence drift.
- **Touches:** `src/TopLab.Domain/Accounting/CashMovement.cs` (modify); `tests/TopLab.Domain.Tests/Accounting/CashMovementTests.cs` (create)
- **Validation Gate:** VG-01 — build zero/zero; Domain tests green; zero-Persistence grep gate; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite 1839/1839 green.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1 + SD-20-13; PaymentOperation.Create guard style.
- [x] **Stage 3 — File Analysis:** CashMovement.cs (guard-free Create); CashMovementConfiguration Notes 500; MovementType enum; PaymentOperation guards.
- [x] **Stage 4 — Planning:** Guards first, then tests.
- [x] **Stage 5 — Execution:** Added amount>0, notes≤500, occurredAtUtc!=default guards; CashMovementTests (7 tests).
- [x] **Stage 6 — Post-Execution Verification:** solution build 0/0.
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — CashMovement 7/7; Domain suite 420/420; zero Persistence diff; has-pending-model-changes → no changes.
- [x] **Stage 8 — Documentation Update:** checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 2: Inventory read surface (FR-M20-001/002/003/004/005)

- **Goal:** Ship the three gated read queries with DTOs, validators, the authorization theory shell, and number-for-number figure tests.
- **Touches:** `src/TopLab.Application/Features/InventoryAndAccounting/Common/InventoryAndAccountingAccessPolicy.cs` (create); `.../Common/InventoryDtos.cs` (create); `.../Queries/GetCashDrawerInventory/` (3 files, create); `.../Queries/GetElementInventory/` (3 files, create); `.../Queries/GetPatientSamplesDetail/` (3 files, create); four test classes under `tests/TopLab.Application.Tests/Features/InventoryAndAccounting/` (create)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green; zero-Persistence + zero-Domain grep gates; coverage ≥ 80%; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 2)

- [ ] **Stage 1 — Pre-Execution Verification:** Full suite green after S1; re-verified before S2 edits.
- [ ] **Stage 2 — Deep Understanding:** Plan §5 S2 + SD-20-1/3/5/6/7/8/10/11/12; Appendix A messages.
- [ ] **Stage 3 — File Analysis:** `PatientAccountCalculator`, `SentOutAccountCalculator`, `BalanceProbe` (formula comment), `Patient`/`PaymentOperation`/`SentOutSample`/`CashMovement`/`ExternalEntity` columns, M-19 period helper + theory precedent, fake DB/clock/user services.
- [ ] **Stage 4 — Planning:** DTOs → access policy → queries + validators → handlers → 4 test classes.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** `dotnet build src/TopLab.Application` 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-02 (incl. zero-drift).
- [ ] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-20] Slice 2/4: Inventory read surface — loop-engineering`.

---

## Slice 3: Cash writes + company/delegate accounts (FR-M20-001/006)

- **Goal:** Ship the two cash commands (guarded `CashMovement` persistence, stamped performer/timestamp), the movement-history query, and the per-entity company/delegate accounts query.
- **Touches:** `.../Commands/RecordCashDeposit/` (3 files, create); `.../Commands/RecordCashDisbursement/` (3 files, create); `.../Queries/ListCashMovements/` (3 files, create); `.../Queries/GetCompanyDelegateAccounts/` (3 files, create); four test classes (create); authorization theory (extend)
- **Validation Gate:** VG-03 — Application build zero/zero; S3 tests green; no-formula-restatement + zero-Persistence grep gates; coverage ≥ 80%; zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 3)

- [ ] **Stage 1 — Pre-Execution Verification:** Full suite green after S2.
- [ ] **Stage 2 — Deep Understanding:** Plan §5 S3 + SD-20-2/3/9/13; Appendix A messages.
- [ ] **Stage 3 — File Analysis:** `CashMovementConfiguration` (ValueGeneratedOnAdd), `ICurrentUserService`, `IDateTimeProvider`, M-03 command/handler/validator precedent.
- [ ] **Stage 4 — Planning:** Commands + validators → handlers → queries → tests → theory extension.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** `dotnet build src/TopLab.Application` 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-03 (incl. zero-drift).
- [ ] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-20] Slice 3/4: Cash writes + company/delegate accounts — loop-engineering`.

---

## Slice 4: Tests + zero-drift proof + close-out

- **Goal:** Prove solution-wide zero drift, extend validator registration, roll up coverage, append ADR-0046, flip tracking, write `Handoff_M20.md`, finish full-suite green.
- **Touches:** `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (M20 row + change log); `Docs/Handoff_M20.md` (create); `Docs/Source/Top_Lab_ADR.md` (append ADR-0046 — reconfirm next free number at execution)
- **Validation Gate:** VG-04 — Release build 0/0; full suite green (`-m:1`); zero-drift proven; ADR + tracking + handoff present; grep gates clean. Migration: none (proven).

### 10-Stage Progress (Slice 4)

- [ ] **Stage 1 — Pre-Execution Verification:** Full suite green after S3.
- [ ] **Stage 2 — Deep Understanding:** Plan §6; handoff template; ADR numbering re-checked.
- [ ] **Stage 3 — File Analysis:** `ValidatorRegistrationTests.cs`, tracking sheet rows 84/102, ADR tail, `Docs/Source/Top_Lab_Handoff_Template.md`.
- [ ] **Stage 4 — Planning:** Validator extension → gates → docs → final run.
- [ ] **Stage 5 — Execution:** Implement per plan.
- [ ] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln -c Release` 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-04 (zero-drift proof binding).
- [ ] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" updated → MODULE COMPLETE.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-20] Slice 4/4: Tests + zero-drift proof + close-out — loop-engineering`.

---

## Current Status

- Slices complete: 1/4. Next action: begin S2 Stage 1.
- Dependency posture: M-20 ⇄ M-23 — no code-level dependency (verified); M-20 executes first by wave order, not by technical necessity.
- S1 evidence (2026-09-15): VG-01 PASS; Domain 420/420; drift clean; commit pending this stage-10.

## Execution Log

| Date | Slice | Stage | Action | Result |
|---|---|---|---|---|
| 2026-09-15 | 1 | 1-10 | CashMovement guards + CashMovementTests; VG-01 PASS | OK |

## Stop Report

(None — no stop condition has triggered.)
