# Loop Engineering — Memory File

- **Module:** Patient Billing & Account Settlement (M-03)
- **Module Number:** M-03
- **Source Plan:** Docs/OpenCode/M-03.md
- **Date Created:** 2026-09-08
- **Total Slices:** 4
- **Current Slice:** Slice 2 done — 2/4 slices done
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Implements the patient billing and account settlement backend: balance-formula guard layer on `PaymentOperation` (amount ≥ 0, discount ≤ amount, extra-charge + discount mutually exclusive, settlement amount > 0) plus a pure-Domain `PatientAccountCalculator` implementing the settled balance formula; Application read surface (patient account, payment history, receipt data, with deleted-user fallback); Application write surface (`RecordPaymentCommand` ungated, `RecordExtraChargeCommand` ungated, `RecordCorrectionCommand` and `VoidPaymentOperationCommand` gated on `CASH_DISBURSE_DEPOSIT`, `SettleAccountInFullCommand` ungated) including the discount-limit enforcement against `User.DiscountLimitPercent` with `Validation` failure on breach and absolute-user exemption; feature-local `DomainFailureTranslator` mapping Domain guards to the frozen Arabic messages; and an Infrastructure close-out slice proving zero model drift (no migration), pinning the FK / index assertions for `PaymentOperation`, and producing ADR-0034 + the M03 tracking-sheet flip + `Handoff_M03.md`. No `PatientTest.IsDeleted` filter is added on charged-tests selections; if M02 deviated and added the flag, the Slice 2 queries must add `&& !pt.IsDeleted` and record the drift. No edit command ships — payment correction is void-and-reissue only (ADR-0017). Done means: build 0/0, full test suite green, ADR-0034, tracking-sheet flip, and `Handoff_M03.md` produced.

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
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/inspection) replaces any standard UI journey — M-03 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-03] Slice N/Total: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-03's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain guards + balance calculator + tests: build zero/zero; new Domain tests green; 741-baseline + M02/M21 deltas green; no migration added (the Slice 4 model-vs-snapshot gate proves zero drift); diff touches only `src/TopLab.Domain/Billing/` and `tests/TopLab.Domain.Tests/Billing/` | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Domain.Tests`; grep src+tests for new mutator coverage |
| 2 | VG-02 | Application read surface: build zero/zero; new tests green (handler tests for `GetPatientAccount`/`GetPatientReceipt`/`ListPatientPayments` incl. soft-deleted patient → NotFound, deleted-user fallback, balance matches Domain formula exactly, voided rows present and flagged, `ReceiptName` used on lines, currency from settings, missing settings row → Unexpected, ordering/pagination, voided included); cumulative suite green (`-m:1`); no command in this slice; diff touches only `src/TopLab.Application/Features/PatientBilling/` + test project | `dotnet build TopLab.sln`; `dotnet test TopLab.sln -m:1` |
| 3 | VG-03 | Application write surface: build zero/zero; all new tests green incl. authorization tests (gate matrix: `RecordCorrectionCommand` + `VoidPaymentOperationCommand` → `CASH_DISBURSE_DEPOSIT`; `RecordPaymentCommand`/`RecordExtraChargeCommand`/`SettleAccountInFullCommand` → no gate; standard denial message `"أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"`); discount-percent cap enforced from `User.DiscountLimitPercent` → `Validation` failure; absolute user bypasses the cap; void double-call → Conflict; settlement with zero/negative balance → Conflict; settlement amount equals the computed balance; extra charge rejects discount via Domain guard surfaced through translator; cumulative suite green (`-m:1`); no migration; no `PermissionConfiguration` change; diff touches only `Features/PatientBilling/` + tests | `dotnet build TopLab.sln`; `dotnet test TopLab.sln -m:1`; grep gates on src+tests |
| 4 | VG-04 | Infrastructure + close-out: Release build zero/zero; full suite green (`-m:1`); zero model drift proven (no new columns); FK / index assertions for `PaymentOperation` green (decimal(18,2) on `Amount`/`DiscountAmount`, tinyint `OperationType`, FK Cascade to `Patient`, index on `PatientId`); `PaymentOperationPersistenceTests` (EF Core InMemory round-trip) green; coverage floors (Domain ≥ 90%, Application ≥ 80%, Infrastructure ≥ 70% of M-03 footprint) or explicit handoff waivers; ADR-0034 appended; tracking-sheet M03 row flipped with dated change-log row; `Handoff_M03.md` created per template; zero Presentation content in the diff (grep gate) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain guards on `PaymentOperation` + balance calculator + Domain tests | [x] Done | VG-01 |
| 2 | Application read surface: patient account, payment history, receipt data | [x] Done | VG-02 |
| 3 | Application write surface: record payment (with optional discount), extra charge, correction, full settlement, void | [ ] Not started | VG-03 |
| 4 | Infrastructure proof + close-out | [ ] Not started | VG-04 |

---

## Slice 1: Domain guards on `PaymentOperation` + balance calculator + Domain tests

- **Goal:** Give the billing aggregate the invariants the write surface needs, and pin the balance formula as a pure, testable Domain function.
- **Touches:** `src/TopLab.Domain/Billing/PaymentOperation.cs` (modify — add guards to `Create` and the `IsEffectivelyZero` convenience read-only property; guard-only change; no new properties — **no migration**); `src/TopLab.Domain/Billing/PatientAccountCalculator.cs` (create — pure static Domain service with `TotalCharged`/`TotalPaid`/`Balance` implementing the inlined balance formula); `tests/TopLab.Domain.Tests/Billing/PatientAccountCalculatorTests.cs` (create — table-driven tests: empty, prices only, extra charge, voided ignored, discount, correction, full-settlement net-to-zero, negative balance); `tests/TopLab.Domain.Tests/Billing/PaymentTests.cs` (extend — negative amount rejected; negative discount rejected; discount > amount rejected; discount on extra charge rejected; full-settlement with amount 0 rejected; void-and-reissue invariants; existing `Create_Valid`/`Void_Sets` keep passing).
- **Validation Gate:** VG-01 — build zero/zero over the full solution; all Domain tests pass; 741-baseline + M02/M21 deltas green; no migration added (the Slice 4 model-vs-snapshot gate proves zero drift); diff touches only `src/TopLab.Domain/Billing/` and `tests/TopLab.Domain.Tests/Billing/`.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §2.2; the inlined balance formula (Data Model §7.1 + ADR-0016 + FR-M03-001/003/004); `Correction` rows contribute to `TotalPaid` (positive Amount = credit — stated engineering pin); `FullSettlement` rows contribute as ordinary payments whose `Amount` equals the balance at settlement time (FR-M03-004); voided rows contribute nothing; guards on `Create`: `amount < 0` → `ArgumentException("Amount must be >= 0.", nameof(amount))`; `discountAmount < 0` → `ArgumentException("Discount must be >= 0.", nameof(discountAmount))`; `discountAmount > amount` → `ArgumentException("Discount cannot exceed the operation amount.", nameof(discountAmount))`; `isExtraCharge && discountAmount > 0` → `ArgumentException("An extra charge cannot carry a discount.", nameof(discountAmount))`; `operationType == FullSettlement && amount <= 0` → `ArgumentException("Settlement amount must be > 0.", nameof(amount))`; `IsEffectivelyZero` is a getter-only convenience property (EF Core ignores by convention).
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `PaymentOperation.cs` (existing Create/Void, guard-free), `PaymentOperationConfiguration.cs` (identity PK, Cascade FK, index on `PatientId`, decimal(18,2), tinyint OperationType), `tests/TopLab.Domain.Tests/Billing/PaymentTests.cs` (`Create_Valid`/`Void_Sets` only), `PatientStatusCalculator.cs` (mirror placement style in `Domain/PatientStatus/`), `PatientAccountCalculator` placement precedent (mirrors `PatientStatusCalculator` static style).
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: (1) add guards to `PaymentOperation.Create` + `IsEffectivelyZero` property; (2) create `PatientAccountCalculator` static class with three methods; (3) write table-driven `PatientAccountCalculatorTests` covering empty/prices-only/extra-charge/voided/discount/correction/full-settlement/credit; (4) extend `PaymentTests` with negative-amount, negative-discount, discount > amount, discount on extra charge, full-settlement amount 0, and void-and-reissue invariants.
- [x] **Stage 5 — Execution:** Slice implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [x] **Stage 7 — Validation Gate:** VG-01 passed. Evidence: build/test output; no Application/Infrastructure diff; no migration added.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (authorized local):** `[M-03] Slice 1/4: Domain guards on PaymentOperation + balance calculator + Domain tests — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.` — on `main`, never push.

---

## Slice 2: Application read surface: patient account, payment history, receipt data

- **Goal:** Queries the (future) billing screen and receipt printing consume. All read-only; no `IAuthorizedRequest`.
- **Touches:** `src/TopLab.Application/Features/PatientBilling/Common/PatientBillingDtos.cs` (create — `PatientAccountDto`, `ChargedTestDto`, `PaymentOperationDto`, `ReceiptDto`); `src/TopLab.Application/Features/PatientBilling/Common/PatientBillingAccessPolicy.cs` (create — `CashDisburseDeposit = "CASH_DISBURSE_DEPOSIT"`); `Queries/GetPatientAccount/GetPatientAccountQuery.cs` (+Handler, +Validator); `Queries/GetPatientReceipt/GetPatientReceiptQuery.cs` (+Handler, +Validator); `Queries/ListPatientPayments/ListPatientPaymentsQuery.cs` (+Handler, +Validator); `tests/TopLab.Application.Tests/Features/PatientBilling/` (create — `GetPatientAccountQueryHandlerTests`, `GetPatientReceiptQueryHandlerTests`, `ListPatientPaymentsQueryHandlerTests`).
- **Validation Gate:** VG-02 — Application build zero/zero; all Application tests pass; every DTO field traced to a verified domain property; no invented fields; no write commands in this slice; diff touches only `src/TopLab.Application/Features/PatientBilling/` + test project.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §2.3; `OperationType` projected as enum name string for cashier readability (code comment); `ReceiptName` carried because receipt lines must show the receipt-facing name, not the internal catalog name; handler loads Patient (NotFound on missing or `IsDeleted`), loads the patient's `PatientTest` rows (join `Test` for `Name`/`TestCode`/`ReceiptName`; NO `IsDeleted` filter — live tree confirms `PatientTest` has no such flag, no drift), loads all `PaymentOperation` rows including voided (history must show them, flagged), resolves `ReceivedByUserName` from `Users` set (fallback to raw id string when the user row is gone), computes `TotalCharged`/`TotalPaid`/`Balance` via `PatientAccountCalculator` and `TotalDiscount = Σ coalesce(DiscountAmount,0) of non-voided non-extra-charge ops`; receipt query reads `ReceiptSettings` row `Id == 1` for `Currency` (Unexpected if missing, M22 precedent); list query is paged flat list newest first (`OperationAtUtc` desc, id desc; NotFound on missing/soft-deleted patient for consistency).
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `IApplicationDbContext` (read in full), `FakeApplicationDbContext` (read — no extension needed; all 6 entity types present), `PaymentOperationConfiguration` (re-verified), `TestConfiguration` (`ReceiptName` ≤150), `ReceiptSettings` (Currency = "L.E."), `User` set in fake (for `ReceivedByUserName` resolution), `Error`/`Result` patterns, M22 settings-missing message style, M02 pagination precedent.
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: DTOs (`PatientBillingDtos.cs`) → access policy class → `GetPatientAccountQuery` (load patient/Tests/PaymentOperations, compute via `PatientAccountCalculator`, resolve user names with deleted-user fallback) → `GetPatientReceiptQuery` (same + currency from `ReceiptSettings`) → `ListPatientPaymentsQuery` (paged, newest first, voided included) → handler test classes.
- [x] **Stage 5 — Execution:** Slice implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [x] **Stage 7 — Validation Gate:** VG-02 passed. Evidence: build/test output; every DTO field traced; no write commands.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (authorized local):** `[M-03] Slice 2/4: Application read surface: patient account, payment history, receipt data — loop-engineering` + `Stages 1-10 verified. Gate VG-02 passed.` — on `main`, never push.

---

## Slice 3: Application write surface: record payment (with optional discount), extra charge, correction, full settlement, void

- **Goal:** The complete cashier write surface with the settled gates.
- **Touches:** `Commands/RecordPayment/RecordPaymentCommand.cs` (+Handler, +Validator — ungated, discount-cap enforcement); `Commands/RecordExtraCharge/RecordExtraChargeCommand.cs` (+Handler, +Validator — ungated); `Commands/RecordCorrection/RecordCorrectionCommand.cs` (+Handler, +Validator — `IAuthorizedRequest` with `CashDisburseDeposit`); `Commands/SettleAccountInFull/SettleAccountInFullCommand.cs` (+Handler, +Validator — ungated); `Commands/VoidPaymentOperation/VoidPaymentOperationCommand.cs` (+Handler, +Validator — `IAuthorizedRequest` with `CashDisburseDeposit`); `Common/DomainFailureTranslator.cs` (create — paramName matcher mapping Slice 1 guards to frozen Arabic strings); `tests/TopLab.Application.Tests/Features/PatientBilling/` (extend — per-command handler tests + `PatientBillingAuthorizationTests`).
- **Validation Gate:** VG-03 — build zero/zero; all new tests green incl. authorization tests (gate matrix asserted); discount-percent cap enforced from `User.DiscountLimitPercent` → `Validation` failure; absolute user bypasses the cap; void double-call → Conflict; settlement with zero/negative balance → Conflict; settlement amount equals the computed balance; extra charge rejects discount via Domain guard surfaced through translator; cumulative suite green (`-m:1`); no migration; no `PermissionConfiguration` change; diff touches only `Features/PatientBilling/` + tests.

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §2.4; `RecordPaymentCommand` is ungated (no `IAuthorizedRequest`) — recording a payment is the registrar/cashier's routine action; when `DiscountAmount` is supplied, the handler loads the current `User` row and — unless `_currentUser.IsAbsolutePermission` — rejects with `Error.Validation("الخصم يتجاوز الحد المسموح به لهذا المستخدم.")` when `DiscountAmount > Amount * user.DiscountLimitPercent / 100m` (Data Model §13 BR-06 + Test Strategy §3.2/§7.2: breach → `Result.Failure` of type `Validation`); absolute-permission users exempt (FR-M17-004 absolute/limited model — stated pin); `RecordExtraChargeCommand` is `IsExtraCharge=true` ungated; `RecordCorrectionCommand` and `VoidPaymentOperationCommand` carry `IAuthorizedRequest` with `RequiredPermissionCode => PatientBillingAccessPolicy.CashDisburseDeposit` (FR-M17-004 item 11 scope: «محاسبة المرضى … (حسابات)»); `SettleAccountInFullCommand` computes current balance via `PatientAccountCalculator` and rejects `Error.Conflict("لا يوجد رصيد مستحق للتسوية.")` when balance ≤ 0; void is idempotent in Domain but handler returns friendly `العملية ملغاة بالفعل.` conflict on double-call; no edit command ships (ADR-0017 + Coding Standards §7.4); `DomainFailureTranslator` maps `ArgumentException` paramNames to frozen Arabic messages (M13/M14/M15 style).
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `ICurrentUserService` (`IsAbsolutePermission`, `UserId`, `UserName`), `IDateTimeProvider`, `IApplicationDbContext` (`PaymentOperations`, `Patients`, `Users` sets), `Error`/`Result` patterns, M13/M14/M15-style `DomainFailureTranslator` precedent, M22 direct settings read precedent, `IAuthorizedRequest` template, `MediatR` pipeline (Validation → Authorization → Logging).
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: (1) `DomainFailureTranslator` (paramName matcher); (2) `RecordPaymentCommand` (ungated, discount-cap path); (3) `RecordExtraChargeCommand` (ungated); (4) `RecordCorrectionCommand` (gated on `CashDisburseDeposit`); (5) `SettleAccountInFullCommand` (balance check, ungated); (6) `VoidPaymentOperationCommand` (gated on `CashDisburseDeposit`, friendly-conflict on double-call); (7) per-command handler tests; (8) `PatientBillingAuthorizationTests` (gate matrix + standard denial).
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-03 passed. Evidence: build/test output; authorization theory tests green; grep gates clean (no missing `IAuthorizedRequest`).
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-03] Slice 3/4: Application write surface: record payment (with optional discount), extra charge, correction, full settlement, void — loop-engineering` + `Stages 1-10 verified. Gate VG-03 passed.` — on `main`, never push.

---

## Slice 4: Infrastructure proof + close-out

- **Goal:** Prove the persistence model still matches the snapshot (guard-only Domain change ⇒ zero drift), pin FK/index assertions for `PaymentOperations`, record decisions, close the module.
- **Touches:** `tests/TopLab.Infrastructure.Tests/Persistence/Configurations/F5ConfigurationTests.cs` (extend — assert `PaymentOperation` mapping: decimal(18,2) on `Amount`/`DiscountAmount`, tinyint `OperationType`, FK Cascade to `Patient`, index on `PatientId`); `tests/TopLab.Infrastructure.Tests/Persistence/PaymentOperationPersistenceTests.cs` (create — EF Core InMemory round-trip: create payment with discount, void it, re-read; assert `IsVoided` persists and the computed balance via the Domain calculator over re-loaded rows matches); `Docs/Source/Top_Lab_ADR.md` (append ADR-0034 — reconfirm max ADR at execution); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M03 row to 🟩 Done + dated change-log row — verified at line 72); `Docs/Handoff_M03.md` (create per template).
- **Validation Gate:** VG-04 — Release build zero/zero; full suite green (`dotnet test TopLab.sln -m:1`); zero model drift proven; coverage floors (Domain ≥ 90%, Application ≥ 80%, Infrastructure ≥ 70% of M-03 footprint) or explicit handoff waivers; ADR-0034 + handoff + tracking flip committed per convention; zero Presentation content in the diff (grep gate).

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §2.5; migration-scope gate is an input hypothesis that must be proven — "no migration expected" is not the proof; ADR-0034 records: settled balance formula, void-and-reissue + id-11 gate (with honest scope-reading note), discount-cap-only enforcement with `Validation` failure + absolute-user exemption pin, single-payment-command shape, `Correction` sign-convention pin, no-edit decision with UI-Blueprint S-04 reconciliation note, payments-ungated/corrections-gated split, deleted-user name fallback; tracking-sheet M03 row verified at line 72.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `F5ConfigurationTests.cs` (existing `PaymentOperation` partial coverage), `ApplicationDbContextModelSnapshot.cs` (relevant sections), `20260828052248_BaselineDataModel.cs` + `20260828123530_RenamePkColumns.cs` + `20260906093902_AddTestCodeAndLifecycleColumns.cs` (the three F5 migrations), InMemory harness, `Top_Lab_ADR.md` (last allocated ADR-0031 at `70f145e`; M02/M21 reserve 0032/0033 per their FINAL plans), `Top_Lab_Master_Tracking_Sheet.md` (M03 row at line 72), `Top_Lab_Handoff_Template.md` / `Handoff_M12.md` (template).
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: run migration-scope gate first (model vs. snapshot; expect zero drift) → extend `F5ConfigurationTests` with `PaymentOperation` mapping assertions → create `PaymentOperationPersistenceTests` (InMemory round-trip) → ADR-0034 → tracking flip → handoff.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-04 passed. Evidence: Release build 0/0, full suite green, zero model drift proven, coverage floors met or waivers documented, zero Presentation content (grep gate).
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-03] Slice 4/4: Infrastructure proof + close-out — loop-engineering` + `Stages 1-10 verified. Gate VG-04 passed.` — on `main`, never push.

---

## Current Status

- Overall: 2/4 slices done
- Slice 1 — Domain guards on `PaymentOperation` + balance calculator + Domain tests: [x] Done (VG-01 passed: build 0/0, Domain 287 green, full suite 1033 green, diff confined to Domain/Billing + Domain.Tests/Billing, no migration)
- Slice 2 — Application read surface: patient account, payment history, receipt data: [x] Done (VG-02 passed: build 0/0, Application 680 green, full suite 1046 green, diff confined to Features/PatientBilling + PatientBilling tests, no commands, PatientTest has no IsDeleted flag — no drift)
- Slice 2 — Application read surface: patient account, payment history, receipt data: [ ] Not started
- Slice 3 — Application write surface: record payment (with optional discount), extra charge, correction, full settlement, void: [ ] Not started
- Slice 4 — Infrastructure proof + close-out: [ ] Not started

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-08 | 0 | — | Memory file created | OK | — |
| 2026-09-08 | 1 | 1-10 | S1 Domain guards + calculator + Domain tests; live-tree re-verified (config, 33-list fake with all 6 lists, ADR max = 0033 → next 0034, tracking row L72); build 0/0; Domain 287 green; full 1033 green; VG-01 passed | OK | pending |
| 2026-09-08 | 2 | 1-10 | S2 read surface (account/receipt/list queries + DTOs + policy + shared reader); PatientTest confirmed flag-free; 1 build fix (missing using); build 0/0; Application 680 green; full 1046 green; VG-02 passed | OK | pending |

## Stop Report (append only if a stop condition triggers)
