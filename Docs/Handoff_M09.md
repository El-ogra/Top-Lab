# Top-Lab — Handoff Document M-09

## نظام توب لاب — تسليم جلسة عمل (Module 9 — Result Delivery & Settlement at Handover)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-15_M09-result-delivery-settlement` |
| Session date (UTC) | 2026-09-15 |
| Session start (UTC) | 2026-09-15 |
| Session end (UTC) | 2026-09-15 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-09 |
| Module name | Result Delivery & Settlement at Handover |
| Wave | 7 |
| Feature folder(s) touched | `src/TopLab.Application/Features/ResultDelivery/**`, `tests/TopLab.Application.Tests/Features/ResultDelivery/`, `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs`, `tests/TopLab.Infrastructure.Tests/Persistence/ResultDeliveryPersistenceTests.cs` |
| Layers touched | Application + Infrastructure-proof tests (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `b182645` (live `main` HEAD after M-07 S5) |
| Final commit at session end | `[M-09] Slice 3/3: Tests + Infrastructure proof + close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 9 **Result Delivery & Settlement at Handover** end-to-end in the three slices S1–S3 of `Docs/OpenCode/M-09.md` (Application read surface + composite delivery/settlement command + persistence proof and close-out). The work is **backend only** — no Presentation content. S1 ships the `DELIVER_RESULTS`-gated read surface: period-filtered undelivered-results list (per-visit status via `PatientStatusCalculator` verbatim), delivery grid with the frozen `PriceAtOrderTime` column, and the delivery account delegating to `PatientBillingReader` with sign-derived remaining amounts. S2 ships the composite `DeliverWithSettlement` command (printed-only per-line delivery with audit; partial `Payment` by default or delegated "خلاص" full settlement; single `SaveChanges`; NO balance gate — BR-08). S3 proves zero drift (no migration), appends ADR-0041, flips the tracking sheet, creates this handoff, and confirms full-suite green. Build must be 0 errors / 0 warnings; full suite green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Application read surface: undelivered list + delivery grid + delivery account** — Implementation Complete — DTOs (`ResultDeliveryDtos.cs`: `UndeliveredPatientRowDto`, `DeliveryGridRowDto`, `DeliveryAccountDto`); access policy (`ResultDeliveryAccessPolicy.DeliverResults`); period helper (`ResultDeliveryPeriod`: both-null → today UTC, `To ??= From`, inclusive UTC-day bounds — M-11 mirror); 3 gated queries (`GetUndeliveredResults` + validator, `GetDeliveryGrid` + validator, `GetDeliveryAccount` delegating to `PatientBillingReader`); 4 test classes (26 tests: inclusive period filter, fully-delivered absence, soft-deleted exclusion, undelivered counting, status-verbatim pin, today-default, frozen-price pin, `GetPatientAccount` number-for-number match, sign-derivation matrix, authorization theory + denial + absolute bypass). Commit `[M-09] Slice 1/3: Application read surface: undelivered list + delivery grid + delivery account — loop-engineering` (`be21e9e`).
- **S2 — Application: composite `DeliverWithSettlement` command** — Implementation Complete — feature-local `DomainFailureTranslator` (`"Result not printed."` → `"النتيجة غير مطبوعة."`, distinct from the ResultsEntry wording); command (`IAuthorizedRequest<Result>`, `DeliverResults`); validator (non-empty line list, positive amount, `SettleInFull` vs `SettleAmount > 0` mutual exclusion); handler (patient load → per-patient line load with `NotFound("التحليل غير موجود")` → `MarkDelivered` with `DeliveredByUserId`/`DeliveredAtUtc` audit and guard translation → partial `Payment` or `FullSettlement` via M-03's `PaymentOperation.Create` path → single `SaveChanges`); 15-case handler-test class (unprinted Conflict, audit fields, exact balance reduction, settle-in-full zeroing + zero/negative Conflict, no-settlement no-change, no-gate pin, unknown patient/line, foreign line, validator trio) + translator tests + command authorization tests + M09 validator-registration theory. Commit `[M-09] Slice 2/3: Application: composite DeliverWithSettlement command — loop-engineering` (`025b2d0`).
- **S3 — Tests + Infrastructure proof + close-out** — Implementation Complete — `ResultDeliveryPersistenceTests` (3 InMemory-on-real-`ApplicationDbContext` tests: period filter + fully-delivered/soft-deleted exclusions, frozen-price grid, audit columns persisted after save); one genuine handler fix found by the persistence run (untranslatable bare `GroupBy` → aggregate-composed projection); zero-drift gate clean (`has-pending-model-changes` → no changes, snapshot untouched); Release build 0/0; full suite green 1632; ADR-0041 appended; tracking sheet M09 row flipped to Done + dated change-log row; `Handoff_M09.md` created; zero Presentation content confirmed.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Release).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -m:1`): **1632 green** = 378 Domain.Tests + 1109 Application.Tests + 145 Infrastructure.Tests.
- New tests added across S1–S3: S1 +26 Application (3 handler-test classes + authorization); S2 +15 handler tests + 2 translator tests + 2 command authorization tests + 3 validator-registration cases; S3 +3 Infrastructure (`ResultDeliveryPersistenceTests`).
- Tests currently failing: none.
- Coverage: per-slice gates passed (VG-01 Application S1 footprint line-rate 0.857–1.000, VG-02 handler 1.000/1.000 + validator 1.000 + translator both arms, VG-03 persistence 3/3 green). Whole-project floors are inapplicable for M-09 (the module touches a subset of files); waiver recorded per the M-11/M-14 precedent.

### 4.3 Migrations

- New EF Core migration(s) added: **None** (delivery audit columns, frozen price, and payment tables all exist at the F5 baseline).
- `has-pending-model-changes` at close-out: No changes — zero drift. Snapshot unchanged.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: none (handlers take the existing `IApplicationDbContext`/`ICurrentUserService`/`IDateTimeProvider`; MediatR + validator assembly scan cover the new types).
- Validators: 3 new validators (`GetUndeliveredResultsQueryValidator`, `GetDeliveryGridQueryValidator`, `DeliverWithSettlementCommandValidator`) resolve via the existing assembly scan; resolution pinned by the M09 validator-registration theory; no DI wiring change.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the `PermissionConfiguration.cs` seed: **none** — `DELIVER_RESULTS` (id=6) already seeded.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All three slices reached a terminal state; module closed out in the Master Tracking Sheet (§4 M09 row + §9 change-log row), ADR-0041, and this handoff. Natural next steps (Presentation-layer delivery screen consuming the queries/command DTOs, M-10 audit consumption of the delivery audit columns, M-16 settlement-pattern reuse) are out of scope of this plan.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

All were owner-confirmed verbatim before execution (plan Finality Assessment) and are recorded in ADR-0041 (no re-derivation):

- **Decision (single operational gate — OD-09-A):** All four use cases carry `IAuthorizedRequest` with `DELIVER_RESULTS`; queries are gated too (the delivery screen is one operational surface; the catalog has no general read permission).
  - **Scope of impact:** All M-09 queries/command; `ResultDeliveryAccessPolicy`; authorization theory tests.
  - **Follow-up required:** No.
- **Decision (Payment-default + "خلاص" delegation — OD-09-B):** Partial settlement records a `Payment` via M-03's write path; full settlement delegates to the `SettleAccountInFull` logic (`FullSettlement`, `Amount = balance`); mutual exclusion enforced by validator.
  - **Scope of impact:** `DeliverWithSettlementCommandHandler` + validator; handler-test matrix.
  - **Follow-up required:** No.
- **Decision (sign-derived position — OD-09-C):** Remaining-to-lab/to-patient derive from the sign of `Balance` only; the account query delegates to `PatientBillingReader` and never recomputes.
  - **Scope of impact:** `GetDeliveryAccountQueryHandler`; sign-matrix tests.
  - **Follow-up required:** No.
- **Decision (no balance gate on delivery — BR-08):** Delivery succeeds with a remaining balance; the command never reads `BlockPrintOnRemainingBalance` (grep-pinned).
  - **Scope of impact:** `DeliverWithSettlementCommandHandler`; no-gate pin test.
  - **Follow-up required:** No.
- **Decision (frozen grid price):** Grid `Price` = `PatientTest.PriceAtOrderTime` only (grep-pinned; test uses a catalog price of 500 vs frozen 100).
  - **Scope of impact:** `GetDeliveryGridQueryHandler`; frozen-price tests (fake + persistence).
  - **Follow-up required:** No.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** One corrective handler fix inside S3 (not a plan deviation): the S1 undelivered-list handler composed a bare `GroupBy` → `ToDictionary` that EF (InMemory and relational) cannot translate. Fixed to an aggregate-composed projection (`GroupBy` → `Select(g => new { … Count = g.Count() })` → `ToList` → `ToDictionary`), matching the vessel of the existing `GetResultWorklist` client-evaluation pattern. Found by the S3 persistence run on its first attempt; fake-backed S1 tests were unaffected (LINQ-to-Objects tolerates the bare form).
- **Waiver:** Whole-project coverage floors (Domain ≥90%, Application ≥80%, Infrastructure ≥70%) are inapplicable for M-09 because the module touches a subset of files in each project (and zero Domain files). Per-slice footprint coverage gates were verified (VG-01 through VG-03 all PASS). This waiver follows the M-11/M-14 precedent where whole-project floors are replaced by footprint posture.

---

## 8. Required Reading (Required — mark "None" if none)

- **M-09 Implementation Plan** — `Docs/OpenCode/M-09.md` (this session's source of truth; §5–§7, Appendix A).
- **M-09 Loop-Engineering Memory** — `Docs/OpenCode/M-09-memory.md` (slice index, gates VG-01..VG-03, per-slice 10-stage checklists, execution log).
- **ADR-0041** — `Docs/Source/Top_Lab_ADR.md` (single gate, settlement semantics, sign-derived position, no-gate restatement, zero-drift).
- **M-03 Handoff** — `Docs/Handoff_M03.md` (for `PatientAccountCalculator`, `PatientBillingReader`, `RecordPayment`/`SettleAccountInFull` write path).
- **M-04 Handoff** — `Docs/Handoff_M04.md` (for `MarkDelivered` guard, `BalanceProbe` delegation, lifecycle audit).
- **M-08 Handoff** — `Docs/Handoff_M08.md` (for `PatientStatusCalculator` seven-state rollup consumed verbatim).
- **M-07 Handoff** — `Docs/Handoff_M07.md` (for the BR-07 print-time gate that M-09 deliberately does not re-apply, and the close-out convention followed here).

---

*End of document.*
