# Top-Lab — Handoff Document M-03

## نظام توب لاب — تسليم جلسة عمل (Module 3 — Patient Billing & Account Settlement)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-08_M03_patient-billing-account-settlement` |
| Session date (UTC) | 2026-09-08 |
| Session start (UTC) | 2026-09-08 |
| Session end (UTC) | 2026-09-08 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-03 |
| Module name | Patient Billing & Account Settlement (backend only) |
| Wave | 5 |
| Feature folder(s) touched | `src/TopLab.Domain/Billing/`, `src/TopLab.Application/Features/PatientBilling/`, `tests/TopLab.Domain.Tests/Billing/`, `tests/TopLab.Application.Tests/Features/PatientBilling/`, `tests/TopLab.Infrastructure.Tests/Persistence/` |
| Layers touched | Domain + Application + Infrastructure proof (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `4e06856` (live `main` HEAD; the plan's reference commit `70f145e` is historical-only and was not used as a baseline — every "re-verified" claim was re-checked against the live tree) |
| Final commit at session end | `(filled at commit time)` |

---

## 2. Session Objective (Required)

Implement Module 3 **Patient Billing & Account Settlement** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-03.md` (Domain guards + balance calculator → read surface → write surface → infrastructure proof + close-out). The work is **backend only** — no Presentation content. S1 pins the settled balance formula as a pure Domain function with guard rails on `PaymentOperation.Create`. S2 ships the three ungated read queries (patient account, receipt data, paged payment history). S3 ships the five write commands with the settled gate matrix (only correction + void gated on `CASH_DISBURSE_DEPOSIT`) and the per-user discount cap. S4 proves zero model drift (no migration), pins the `PaymentOperations` mapping assertions, and closes the module (ADR-0034 + tracking flip + this handoff). Build must be 0 errors / 0 warnings; full suite green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain guards on `PaymentOperation` + balance calculator + Domain tests** — Implementation Complete — `PaymentOperation.Create` guards (`amount < 0`, `discountAmount < 0`, `discountAmount > amount`, discount on extra charge, `FullSettlement` with amount ≤ 0) + getter-only `IsEffectivelyZero` (unmapped by EF convention); new pure static `PatientAccountCalculator` (`TotalCharged`/`TotalPaid`/`Balance`) implementing the settled formula verbatim; `PatientAccountCalculatorTests` (9 tests incl. the shared worked example Charged 170/Paid 90/Balance 80) + 8 new `PaymentTests` guard/void-and-reissue cases. Commit `[M-03] Slice 1/4: ...`.
- **S2 — Application read surface** — Implementation Complete — `Common/PatientBillingDtos.cs` (`PatientAccountDto`, `ChargedTestDto`, `PaymentOperationDto` with `OperationType` as enum-name string for cashier readability, `ReceiptDto`), `Common/PatientBillingAccessPolicy.cs` (`CashDisburseDeposit = "CASH_DISBURSE_DEPOSIT"`, mirrored shape, no cross-feature import), shared `internal` reader `PatientBillingReader` (account computation via the Domain calculator, voided-included history, deleted-user raw-id fallback), `Queries/GetPatientAccount/`, `Queries/GetPatientReceipt/` (currency from `ReceiptSettings` row `Id == 1`, `Unexpected` when missing — M22 precedent), `Queries/ListPatientPayments/` (paged, `OperationAtUtc desc` + id tie-break, voided included) — all ungated; 13 handler tests. Commit `[M-03] Slice 2/4: ...`.
- **S3 — Application write surface** — Implementation Complete — `Common/DomainFailureTranslator.cs` (paramName matcher → frozen Arabic messages, M13/M14/M15 style); ungated `RecordPaymentCommand` (optional discount, `User.DiscountLimitPercent` cap → `Validation` breach, absolute-permission exemption, missing user row ⇒ effective cap zero — fail-closed), ungated `RecordExtraChargeCommand` (`IsExtraCharge = true`, no discount field), gated `RecordCorrectionCommand` + `VoidPaymentOperationCommand` (`IAuthorizedRequest` → `CashDisburseDeposit`), ungated `SettleAccountInFullCommand` (live balance via calculator, `Conflict` when balance ≤ 0, `FullSettlement` with `Amount = balance`); per-command handler tests + translator tests + `PatientBillingAuthorizationTests` (gate matrix + standard denial message). No edit command (void-and-reissue only). Commit `[M-03] Slice 3/4: ...`.
- **S4 — Infrastructure proof + close-out** — Implementation Complete — migration-scope gate: `dotnet ef migrations has-pending-model-changes` → "No changes have been made to the model since the last migration" (no migration); `F5ConfigurationTests` extended with `PaymentOperation_HasExpectedMapping` (decimal(18,2) × 2, int-converted `OperationType`, Cascade FK to `Patient`, index on `PatientId`) + `PaymentOperation_IsEffectivelyZero_IsNotMapped`; `PaymentOperationPersistenceTests` (InMemory round-trip: discount persists, void persists, calculator balance over re-loaded rows matches); ADR-0034; tracking-sheet M03 row → 🟩 Done + dated change-log row; this handoff. Commit `[M-03] Slice 4/4: ...`.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Release, `-m:1` posture).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -m:1`): **1077 green** = 287 Domain.Tests + 708 Application.Tests + 82 Infrastructure.Tests.
- New tests added: +61 (17 Domain + 41 Application + 3 Infrastructure).
- Tests currently failing: none.
- Coverage of the M-03 footprint (coverlet, Release): Domain 95.2% (40/42 — the 2 uncovered lines are the pre-existing EF-only private parameterless constructor, untestable by design) ≥ 90% ✅; Application 85.9% (280/326 — all handlers/queries/reader/DTOs/translator at 88–100%; only the 8 validator rule-definition files sit at 0/33 because handler-direct tests bypass the MediatR validation pipeline, same posture as prior modules) ≥ 80% ✅; Infrastructure footprint is tests-only (no `src` change) — the 3 new tests green ✅.

### 4.3 Migrations

- New EF Core migration(s) added: **None.** The S1 Domain change is guard-only plus one getter-only property (unmapped — proven by `PaymentOperation_IsEffectivelyZero_IsNotMapped` and by the `has-pending-model-changes` gate).
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: None. The 8 new validators (3 query + 5 command) resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (assembly scanning, no DI wiring change).
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the 13-row `PermissionConfiguration.cs` seed: **none** — `git diff src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` is empty.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All four slices reached a terminal state; module closed out in the Master Tracking Sheet (§4 M03 row + §9 change-log row) and this handoff. The natural next steps (billing-screen/receipt Presentation consuming the S2/S3 surface; M04/M09 follow-ups) are out of scope of this plan.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

All four were owner-confirmed verbatim before/during execution and are recorded in ADR-0034 (no re-derivation):

- **Decision (balance formula):** `TotalCharged = Σ PriceAtOrderTime + Σ Amount of non-voided extra-charge ops`; `TotalPaid = Σ (Amount + coalesce(DiscountAmount,0)) of non-voided non-extra-charge ops` (discounts included; `Correction` rows contribute to `TotalPaid`, positive Amount = credit); `Balance = TotalCharged − TotalPaid` (negative = credit, no clamping); `FullSettlement` rows are ordinary payments whose Amount equals the balance at settlement time; voided rows contribute nothing.
  - **Reason:** Settled requirements (Data Model §7.1 + ADR-0016 + FR-M03-001/003/004).
  - **Scope of impact:** `PatientAccountCalculator` + all three query handlers + settlement command; pinned by the shared worked example test.
  - **Follow-up required:** No.
- **Decision (`Correction` sign convention):** Positive Amount = credit reducing balance.
  - **Reason:** Owner-confirmed engineering pin.
  - **Scope of impact:** `RecordCorrectionCommand` (`Amount > 0` validator) + paid-side computation.
  - **Follow-up required:** No.
- **Decision (discount-cap exemption):** Absolute-permission users are EXEMPT from the discount cap; all others are capped (`Validation` failure on breach).
  - **Reason:** FR-M17-004 absolute/limited model; Data Model §13 BR-06; Test Strategy §3.2.
  - **Scope of impact:** `RecordPaymentCommandHandler` only; no permission-code change.
  - **Follow-up required:** No.
- **Decision (void-and-reissue + id-11 gate + S-04 note):** No edit command ships; correction and void are gated on `CASH_DISBURSE_DEPOSIT` (documented-scope reading of FR-M17-004 item 11, not a verbatim quote — recorded honestly in ADR-0034); S-04's «تعديل» action must become a void-and-reissue flow behind the button, or S-04 corrected — S-04 was NOT changed here.
  - **Scope of impact:** `VoidPaymentOperationCommandHandler` XML doc + ADR-0034.
  - **Follow-up required:** Yes — owned by the S-04/Presentation track, not this module.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** The plan cites a "29-list fake" and test-count snapshots (741/996/1016) — the live tree has a 33-list fake and 1016 baseline tests at session start. Numbers ignored per owner correction; the gates applied were "all 6 needed lists present" (verified: `Patients`, `PatientTests`, `PaymentOperations`, `Tests`, `Users`, `ReceiptSettings` — no fake extension needed) and "FULL suite green".
  - **Waiver:** No waiver required — owner-confirmed corrections applied during execution.
  - **Pinned by:** Full-suite green at every slice gate (1033 → 1046 → 1074 → 1077).
- **Deviation:** The F5 `tinyint` column-type assertion is not observable through the InMemory provider (`GetColumnType()` throws `InvalidCastException` — InMemory carries no relational type mapping).
  - **Waiver:** Asserted the InMemory-observable proxy instead (`GetProviderClrType() == typeof(int)` for the `<int>` conversion); the `tinyint` annotation itself is pinned by the zero-drift gate (model snapshot unchanged). Documented in a code comment at the assertion.
  - **Pinned by:** `PaymentOperation_HasExpectedMapping` + `has-pending-model-changes` = no changes.
- **Waiver:** None (no coverage waiver — floors measured at close-out; see §4.2).

---

## 8. Required Reading (Required — mark "None" if none)

- **M-03 Implementation Plan** — `Docs/OpenCode/M-03.md` (this session's source of truth; §2.1 inlines the complete assumed context).
- **M-03 Loop-Engineering Memory** — `Docs/OpenCode/M-03-memory.md` (slice index, gates VG-01..VG-04, per-slice 10-stage checklists, execution log).
- **ADR-0034** — `Docs/Source/Top_Lab_ADR.md` (balance formula, Correction-sign pin, cap exemption, void-and-reissue + S-04 note, gate split, deleted-user fallback).
- **M-02 Handoff** — `Docs/Handoff_M02.md` (for `Patient.IsDeleted` / `PriceAtOrderTime` baseline this module builds on).

---

*End of document.*
