# Top-Lab — Handoff Document M-20

## نظام توب لاب — تسليم جلسة عمل (Module 20 — Inventory & Lab Accounting)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-15_M20-inventory-lab-accounting` |
| Session date (UTC) | 2026-09-15 |
| Session start (UTC) | 2026-09-15 |
| Session end (UTC) | 2026-09-15 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-20 |
| Module name | Inventory & Lab Accounting |
| Wave | 9 |
| Feature folder(s) touched | `src/TopLab.Domain/Accounting/CashMovement.cs`, `src/TopLab.Application/Features/InventoryAndAccounting/**`, `tests/TopLab.Domain.Tests/Accounting/`, `tests/TopLab.Application.Tests/Features/InventoryAndAccounting/`, `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` |
| Layers touched | Domain (guards only) + Application + tests + close-out docs |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `93cb752` (M-19 close-out HEAD) |
| Final commit at session end | `[M-20] Slice 4/4: Tests + zero-drift proof + close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 20 **Inventory & Lab Accounting** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-20.md`. Harden `CashMovement.Create` with three guards; ship the `CASH_DISBURSE_DEPOSIT`-gated computed Accounts surface (drawer inventory, element inventory, patient samples detail, cash deposit/disbursement, movement history, company/delegate accounts) over the already-complete schema with zero migration; close out with zero-drift proof, validator registration, ADR-0046, tracking flip, and this handoff. Backend only — no Presentation content.

---

## 3. Achievements This Session (Required)

- **S1 — CashMovement domain guards + tests** — Implementation Complete — `amount > 0`, `notes ≤ 500`, `occurredAtUtc != default` on `Create` (behavior-only); `CashMovementTests` (7 tests: paramNames, max-length boundary, valid round-trip). Commit `7072143`.
- **S2 — Inventory read surface** — Implementation Complete — `InventoryAndAccountingAccessPolicy` + `InventoryDtos`; `GetCashDrawerInventory` (ten SD-20-10 figures via calculators + commissions SD-20-8), `GetElementInventory` (5 element kinds × 4 report types + NotFound), `GetPatientSamplesDetail` (FR-M20-005); 4 test classes including authorization theory shell. Commit `69b7712`.
- **S3 — Cash writes + company/delegate accounts** — Implementation Complete — `RecordCashDeposit`/`RecordCashDisbursement` (stamped performer/timestamp, entity NotFound); `ListCashMovements` (dictionary names + raw-id fallback); `GetCompanyDelegateAccounts` (per-entity cash + sent-out settlement); theory extended over all 7 commands/queries. Commit `764efb0`.
- **S4 — Zero-drift + close-out** — Implementation Complete — drift gate clean; M20 validator-registration theory (7 cases); ADR-0046; tracking M20 + Wave 9 flipped; `Handoff_M20.md`.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build
- Solution builds locally: Yes. Errors: 0. Warnings: 0.

### 4.2 Tests
- All existing tests still pass: Yes.
- New tests: S1 +7 Domain; S2 +23 Application (3 handler + auth); S3 +18 Application (4 handler classes + theory extension); S4 +7 validator-registration cases.
- Tests currently failing: none.
- Coverage: per-slice footprint gates under the M-11/M-14/M-19 waiver posture.

### 4.3 Migrations
- New EF Core migration(s) added: **None**.
- `has-pending-model-changes` at close-out: No changes — zero drift. Snapshot unchanged.

### 4.4 Dependency Injection wiring
- New registration: none (existing ports + assembly validator scan).

### 4.5 Configuration
- New configuration keys: none.
- `PermissionConfiguration.cs` seed: **none** — `CASH_DISBURSE_DEPOSIT` (id=11) already seeded.

---

## 5. Work In Progress (Required)

None.

---

## 6. Contracts / API Surface (Required)

| Member | Kind | Gate |
|---|---|---|
| `GetCashDrawerInventoryQuery(From, To)` → `Result<CashDrawerInventoryDto>` | Query | `CASH_DISBURSE_DEPOSIT` |
| `GetElementInventoryQuery(From, To, Element, ElementId, AccountType, ReportType)` → `Result<ElementInventoryDto>` | Query | `CASH_DISBURSE_DEPOSIT` |
| `GetPatientSamplesDetailQuery(From, To)` → `Result<IReadOnlyList<PatientSampleDetailDto>>` | Query | `CASH_DISBURSE_DEPOSIT` |
| `RecordCashDepositCommand(Amount, RelatedExternalEntityId?, Notes?)` → `Result<int>` | Command | `CASH_DISBURSE_DEPOSIT` |
| `RecordCashDisbursementCommand(Amount, RelatedExternalEntityId?, Notes?)` → `Result<int>` | Command | `CASH_DISBURSE_DEPOSIT` |
| `ListCashMovementsQuery(From, To)` → `Result<IReadOnlyList<CashMovementDto>>` | Query | `CASH_DISBURSE_DEPOSIT` |
| `GetCompanyDelegateAccountsQuery(From, To, ExternalEntityId?)` → `Result<IReadOnlyList<CompanyDelegateAccountDto>>` | Query | `CASH_DISBURSE_DEPOSIT` |

---

## 7. Notes for the Incoming Agent (Required)

- **SD-20-3:** Every M-20 use case requires `CASH_DISBURSE_DEPOSIT` (id=11). No second gate in handlers. Secondary-password dialog is Presentation-side via existing `VerifySecondaryPasswordQuery`.
- **SD-20-5:** Financial figures come only from `PatientAccountCalculator` / `SentOutAccountCalculator`. Never restate formulas.
- **SD-20-10:** SafeCash = Collected + Deposits − Disbursements; NetProfit = Collected − SentOutPaid − Disbursements; RemainingToLab = Uncollected.
- **SD-20-13:** `CashMovement.Create` rejects non-positive amount, over-long notes (>500), and default timestamp. No edit/void path ships (ADR-0046).
- **Soft-deleted patients** are excluded from all inventory figures.
- **Periods:** half-open UTC, inclusive calendar days, default-today, «بداية الفترة يجب ألا تتجاوز نهايتها.» on inverted range.
- **No Presentation content.** Accounts icon / inventory window / password dialog are Presentation concerns.

---

## 8. Artifacts / Attachments (Optional)

- Plan: `Docs/OpenCode/M-20.md`
- Memory: `Docs/OpenCode/M-20-memory.md`
- ADR: `Docs/Source/Top_Lab_ADR.md` → ADR-0046
- Tracking: `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` M20 row + Wave 9 + §9 change-log

---

## 9. Acceptance / Sign-off (Optional)

| Criterion | Status |
|---|---|
| Build 0 errors / 0 warnings | Yes |
| Full suite green | Yes |
| Zero migration / zero drift proven | Yes |
| Zero Presentation / zero Persistence schema change | Yes (only Domain behavior guards) |
| ADR-0046 + tracking flip + handoff present | Yes |
| Local commits only, never pushed | Yes |

---

*End of handoff.*
