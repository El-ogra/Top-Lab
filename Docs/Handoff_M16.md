# Top-Lab — Handoff Document M-16

## نظام توب لاب — تسليم جلسة عمل (Module 16 — Sent-Out Samples)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-15_M16-sent-out-samples` |
| Session date (UTC) | 2026-09-15 |
| Session start (UTC) | 2026-09-15 |
| Session end (UTC) | 2026-09-15 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-16 |
| Module name | Sent-Out Samples |
| Wave | 7 |
| Feature folder(s) touched | `src/TopLab.Domain/SentOutSamples/**`, `src/TopLab.Application/Features/SentOutSamples/**`, `tests/TopLab.Domain.Tests/SentOutSamples/`, `tests/TopLab.Application.Tests/Features/SentOutSamples/`, `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs`, `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs`, `tests/TopLab.Infrastructure.Tests/Persistence/SentOutSamplePersistenceTests.cs` |
| Layers touched | Domain + Application + Infrastructure-proof tests (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `a698ed7` (live `main` HEAD after M-09) |
| Final commit at session end | `[M-16] Slice 4/4: Tests + Infrastructure proof + close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 16 **Sent-Out Samples** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-16.md` (Domain guards + settlement calculator → Application write surface → Application read surface → persistence proof and close-out). The work is **backend only** — no Presentation content. S1 enriches the two factory-only Domain entities with additive guards and ships the single-sourced `SentOutAccountCalculator` (Data Model §8.3). S2 ships the three `CASH_DISBURSE_DEPOSIT`-gated write commands (dispatch with eligibility/type/duplication guards and auto-captured editable pricing, «ترسل إلى» partial payment capped at remaining, «خلاص» full settlement). S3 ships the two open reads (period list with optional entity filter, per-lab account with dispatch count and calculator totals). S4 proves zero drift (no migration), pins Cascade + Restrict, appends ADR-0042, flips the tracking sheet, creates this handoff, and confirms full-suite green. Build must be 0 errors / 0 warnings; full suite green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain: sent-out guards + settlement calculator** — Implementation Complete — additive guards in `SentOutSample.Create` (`CostPrice`/`PatientPrice >= 0`, English `ArgumentException`) and `SentOutSamplePayment.Create` (`AmountPaid > 0`); new pure static `SentOutAccountCalculator` (`TotalCost`/`TotalPaid`/`Remaining`/`IsFullySettled`); 3 test classes (12 tests: negative cost/patient price, zero prices accepted, zero/negative payment, calculator partial/over/exact/empty + multi-sample sum). Commit `[M-16] Slice 1/4: Domain: sent-out guards + settlement calculator — loop-engineering` (`3b499c5`).
- **S2 — Application write surface: dispatch + partial payment + full settlement** — Implementation Complete — DTOs (`SentOutSampleDto` with per-sample totals + `SentOutLabAccountDto`); `SentOutSamplesAccessPolicy.CashDisburseDeposit` (feature-local const, identical code value — U1 decided); `DomainFailureTranslator` (cost/patient → «السعر يجب ألا يكون سالبًا.»; amount → «مبلغ الدفع يجب أن يكون أكبر من صفر.»); `SendSampleOut` (guard chain with all six verbatim messages, prices default from `Test.SentOutCostPrice`/`Test.PatientPrice`, `Create(0)` IDENTITY sentinel, single save); `RecordSentOutPayment` (remaining via calculator, over-remaining Conflict, `PerformedByUserId`/`PaidAtUtc` audit); `SettleSentOutInFull` (exact-remaining single payment, nothing-due Conflict); `FakeApplicationDbContext.SentOutSamplePayments` extension (proven gap: List + Set/Add/Remove routing); 4 test classes (30 tests: 8 dispatch negatives, default-capture + override + negative-override validation, partial/over/recomputed-remaining, exact-settle + nothing-due, audit fields, authorization theory + denial + absolute bypass). Commit `[M-16] Slice 2/4: Application write surface: dispatch + partial payment + full settlement — loop-engineering` (`f6db46d`).
- **S3 — Application read surface: period query + per-lab account query** — Implementation Complete — `GetSentOutSamples` (DateTime half-open bounds = inclusive UTC calendar days, SQL-translatable and fake-compatible; default-today-UTC via `IDateTimeProvider`; optional entity filter; Skip/Take; dictionary name resolution; per-sample paid/remaining/settled via calculator) + validator (`From ≤ To`, paging bounds); `GetSentOutLabAccount` (entity existence + `PartnerLab` check with S2 messages, period dispatch count + calculator totals) + validator; 2 test classes (13 tests: inclusive period + boundary instants, entity-only filter, name/totals resolution, empty-set, today-default, paging, number-for-number vs calculator, unknown/non-lab rejections, validator matrices). Zero `IAuthorizedRequest` under Queries (grep-pinned). Commit `[M-16] Slice 3/4: Application read surface: period query + per-lab account query — loop-engineering` (`9d4c88a`).
- **S4 — Tests + Infrastructure proof + close-out** — Implementation Complete — `SentOutSamplePersistenceTests` (4 InMemory-on-real-`ApplicationDbContext` tests: store/retrieve + calculator agreement + interceptor audit columns, behavioral Cascade delete, model-level Cascade + Restrict pins); M16 validator-registration theory (5 cases); zero-drift gate clean (`has-pending-model-changes` → no changes, snapshot untouched); Release build 0/0; full Release suite green 1696 (`-m:1`); ADR-0042 appended; tracking sheet M16 row flipped to Done + dated change-log row; `Handoff_M16.md` created; zero Presentation content confirmed.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Release).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -c Release -m:1`): **1696 green** = 390 Domain.Tests + 1157 Application.Tests + 149 Infrastructure.Tests.
- New tests added across S1–S4: S1 +12 Domain; S2 +30 Application (3 handler-test classes + authorization); S3 +13 Application (2 query-test classes); S4 +4 Infrastructure (`SentOutSamplePersistenceTests`) + 5 validator-registration cases.
- Tests currently failing: none.
- Coverage: per-slice gates passed (VG-01 Domain SentOutSamples footprint line-rate 0.85–1.00 branch 1.00; VG-02 Application S2 footprint 85.2%; VG-03 Application S3 footprint 100%; VG-04 persistence 4/4 green). Whole-project floors are inapplicable for M-16 (the module touches a subset of files); waiver recorded per the M-11/M-14 precedent.

### 4.3 Migrations

- New EF Core migration(s) added: **None** (sent-out tables, Cascade payment→sample, Restrict sample→entity, and DbSets all exist at the F5 baseline).
- `has-pending-model-changes` at close-out: No changes — zero drift. Snapshot unchanged.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: none (handlers take the existing `IApplicationDbContext`/`ICurrentUserService`/`IDateTimeProvider`; MediatR + validator assembly scan cover the new types).
- Validators: 5 new validators (`SendSampleOutCommandValidator`, `RecordSentOutPaymentCommandValidator`, `SettleSentOutInFullCommandValidator`, `GetSentOutSamplesQueryValidator`, `GetSentOutLabAccountQueryValidator`) resolve via the existing assembly scan; resolution pinned by the M16 validator-registration theory; no DI wiring change.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the `PermissionConfiguration.cs` seed: **none** — `CASH_DISBURSE_DEPOSIT` (id=11) already seeded.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All four slices reached a terminal state; module closed out in the Master Tracking Sheet (§4 M16 row + §6 block + §9 change-log row), ADR-0042, and this handoff. Deferred scope (out of this plan): cancel/re-send of a recorded sample (OD-16-B — needs a void/status column + migration + a later owner decision); Presentation-layer sent-out follow-up screen consuming the queries/commands; M-19/M-20 consumption of the read semantics.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

All were owner-confirmed verbatim before execution (plan Finality Assessment) and are recorded in ADR-0042 (no re-derivation):

- **Decision (per-sample lab binding — OD-16-A):** The external lab is chosen at each sample's dispatch (`SentOutSample.ExternalLabEntityId`); no `Test.DefaultExternalLabEntityId` column, no migration. FR-M16-001's test-definition-time phrasing is a documented PRD-vs-code contradiction resolved in favor of code.
  - **Scope of impact:** `SendSampleOutCommandHandler` (entity existence + `PartnerLab` check); ADR-0042; this handoff.
  - **Follow-up required:** No (consumer note: M-19/M-20 read `ExternalLabEntityId` per sample).
- **Decision (no cancel/re-send in v1 — OD-16-B):** No void/status mutator; nothing deletes or cancels a recorded sample (grep-pinned absence).
  - **Scope of impact:** Whole M-16 diff (no cancel path by construction).
  - **Follow-up required:** Later owner decision if re-send is ever required (schema change + migration).
- **Decision (`CASH_DISBURSE_DEPOSIT` reuse — OD-16-C):** All three write commands gated on the seeded id=11 code via `SentOutSamplesAccessPolicy`; reads open.
  - **Scope of impact:** 3 commands; authorization theory tests.
  - **Follow-up required:** No.
- **Decision (auto-capture-with-override pricing — OD-16-D):** Omitted prices default from `Test.SentOutCostPrice`/`Test.PatientPrice`; explicit overrides honored under Domain guards.
  - **Scope of impact:** `SendSampleOutCommandHandler`; default-capture + override tests.
  - **Follow-up required:** No.
- **Decision (U1 — access-policy shape):** New feature-local `SentOutSamplesAccessPolicy` class (identical code value to `PatientBillingAccessPolicy.CashDisburseDeposit`), decided in S2 execution.
  - **Scope of impact:** 3 commands' `RequiredPermissionCode`.
  - **Follow-up required:** No.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation (test-environment, not a product change):** The EF InMemory provider does not generate keys through strongly-typed-ID value converters when two same-type entities are added before a save (both track key 0 → identity conflict; first seen in `StoreRetrieve_SamplePlusPayments_ReloadMatchesAndCalculatorAgrees`). Fixed by saving between adds and using explicit payment IDs (`Create(1)`/`Create(2)`). Single-add `Create(0)` usage is unaffected (matches the `ExternalEntityAuditGateTests` precedent). No product code changed.
- **Waiver:** Whole-project coverage floors (Domain ≥90%, Application ≥80%, Infrastructure ≥70%) are inapplicable for M-16 because the module touches a subset of files in each project. Per-slice footprint coverage gates were verified (VG-01 through VG-04 all PASS). This waiver follows the M-11/M-14 precedent where whole-project floors are replaced by footprint posture.
- **Consumer note (M-14 consistency):** Deleting a partner lab that has samples stays blocked twice — the `DeleteExternalEntityCommandHandler` guard («تعذر حذف الجهة لوجود عينات مرسلة مرتبطة بها.») plus the model-level `Restrict` (pinned in `SentOutSamplePersistenceTests.SampleToEntity_FK_IsRestrict`).

---

## 8. Required Reading (Required — mark "None" if none)

- **M-16 Implementation Plan** — `Docs/OpenCode/M-16.md` (this session's source of truth; §5–§7, Appendix A).
- **M-16 Loop-Engineering Memory** — `Docs/OpenCode/M-16-memory.md` (slice index, gates VG-01..VG-04, per-slice 10-stage checklists, execution log).
- **ADR-0042** — `Docs/Source/Top_Lab_ADR.md` (per-sample binding + PRD-contradiction record, no cancel/re-send, permission reuse, pricing, single-sourced calculator, zero-drift).
- **M-12 Handoff** — `Docs/Handoff_M12.md` (for `Test.IsSentOut`/`SentOutCostPrice`/`PatientPrice` definition consumed read-only).
- **M-14 Handoff** — `Docs/Handoff_M14.md` (for `ExternalEntity` lifecycle + `PartnerLab` type + the sent-out delete guard).
- **M-03 Handoff** — `Docs/Handoff_M03.md` (for the `PaymentOperation` partial-payment pattern emulated by `SentOutSamplePayment`, and `CASH_DISBURSE_DEPOSIT` id=11).

---

*End of document.*
