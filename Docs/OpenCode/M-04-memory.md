# Loop Engineering — Memory File

- **Module:** Results Entry & Result Lifecycle (M-04)
- **Module Number:** M-04
- **Source Plan:** Docs/OpenCode/M-04.md
- **Date Created:** 2026-09-08
- **Total Slices:** 4
- **Current Slice:** Not started — 0/4 slices done
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Implements the seven-state patient aggregate status (`PatientStatusCalculator` delivered in full from PRD §8.2/§8.3 — no stub, no waiver — with the S1/S2 "newly registered" micro-pin), the result-lifecycle guard chain on `PatientTest` (`EnterResult` requires not-reviewed, `ClearResult` requires not-locked, `Unreview` requires not-printed-or-delivered, `MarkReviewed` requires entered, `MarkPrinted` requires reviewed, `MarkDelivered` requires printed), the new `MarkEntered(int, DateTime)` entered-invariant mutator for profile/culture, the persisted reference-range freeze as a dedicated child table `PatientTestReferenceRangeSnapshots` (1:1 by `PatientTestId`, Cascade FK — the module's only migration), the auto-compute of `ResultFlag` at entry with the owner-settled overlapping-range selection rule (sex-matched preferred, then narrowest age band, lowest id on tie) and an explicit user-refresh path (FR-M04-008), the auto review-and-completion system action when `SystemSettings.AutoReviewAndComplete` is set, the print-time balance block via per-user `BlockPrintOnRemainingBalance` + `BalanceProbe` (no runtime `BLOCK_PRINT_ON_BALANCE` gate; the code is the per-user grant item backing the flag), and the write surface (enter, clear, refresh-range, review, unreview, print, deliver, bulk variants) gated on `EDIT_RESULTS`/`REVIEW_RESULTS`/`PRINT_RESULTS`/`DELIVER_RESULTS`. No `PatientTest.IsDeleted` filter; if M02 deviated and added the flag, the named queries add `&& !pt.IsDeleted` and record the drift. Done means: build 0/0, full test suite green, ADR-0035, tracking-sheet flip, and `Handoff_M04.md` produced.

## Global Validation Gates

## Binding owner decisions added after plan creation

- **D1:** new registration is one Application transaction: `CreatePatientCommand` requires at least one ordered-test input and persists the patient plus `PatientTest` rows together; empty selection is Validation. `AddTestsToVisit` remains only for an existing visit.
- **D2:** `ExportPatientReportPdfCommand` exports the complete verified patient report to a supplied absolute local/LAN `.pdf` path through an Application PDF port implemented by Infrastructure. It uses `PRINT_RESULTS`, does not apply the balance gate, does not print or advance lifecycle, and marks included rows exported only after success. Existing target → `Conflict` with no overwrite; invalid/non-absolute path → Validation; I/O/render failure → Unexpected with no export marks. No migration or export permission seed.
- **D3:** bulk printing is two-step: preflight reports `RequiresReprintConfirmation` per patient report when any included row was printed; Presentation asks the mandated Arabic question and submits explicit confirm/cancel. Confirm reprints and increments audit counts; cancel skips the whole patient report and continues. All normal print guards remain effective.
- **D4:** simple `EnterResultCommand` rejects null/empty/whitespace result values before mutation with exactly `الرجاء إدخال قيمة النتيجة قبل الحفظ`; numeric and symbolic non-whitespace values are valid.
- **D5:** `BalanceProbe` loads data only and calls `PatientAccountCalculator.Balance`; the balance formula is never duplicated.

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
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/inspection) replaces any standard UI journey — M-04 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-04] Slice N/Total: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-04's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain lifecycle guards + snapshot child entity + status calculator + tests: build zero/zero; Domain tests green; cumulative suite green; **migration applies cleanly on a fresh database and the existing three migrations + M02's `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex` still apply in order**; diff touches `src/TopLab.Domain/Results/`, `src/TopLab.Domain/PatientStatus/`, `Persistence/Configurations/` + `DbSets`, the new migration, Domain tests only | `dotnet build TopLab.sln`; `dotnet test TopLab.sln`; migration apply check |
| 2 | VG-02 | Application read surface: build zero/zero; new tests green (worklist filter tests incl. soft-deleted patient excluded, entry-detail tests with frozen range, sheet tests; `ResultFlagComputerTests` — parse-fail/no-match/low/normal/high/boundary-inclusive/age-unit-mismatch/most-specific selection); cumulative suite green; no commands in this slice; no migration in this slice | `dotnet build TopLab.sln`; `dotnet test TopLab.sln -m:1` |
| 3 | VG-03 | Application write surface: build zero/zero; all new tests green (per-command handler tests incl. auto-review on/off, flag auto-compute vs. operator override, snapshot upsert/delete, refresh-range command, wrong-kind test rejected, balance-block matrix with worked example, delivery not balance-gated, bulk skip semantics; `ResultsEntryAuthorizationTests` asserting each command's gate code + the standard denial message); cumulative suite green; no migration in this slice; no `PermissionConfiguration` change; diff limited to `Features/ResultsEntry/` + fake + tests | `dotnet build TopLab.sln`; `dotnet test TopLab.sln -m:1`; grep gates on src+tests |
| 4 | VG-04 | Infrastructure + close-out: Release build zero/zero; full suite green (`-m:1`); migration ↔ snapshot zero drift proven; `ResultLifecyclePersistenceTests` green (InMemory end-to-end + cascade); `F5ConfigurationTests` extended for `PatientTestReferenceRangeSnapshots` mapping (PK = `PatientTestId`, Cascade FK, decimal(18,4), nvarchar(500), tinyint enums, datetimeoffset); validator-registration test extended for all Module 4 validators; coverage floors or waivers; ADR-0035 appended; M04 tracking row flipped with dated change-log row (verified at line 73); `Handoff_M04.md` per template; zero Presentation content (grep gate) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain: lifecycle guards, snapshot child entity, full status calculator + Domain tests + MIGRATION | [x] Done | VG-01 |
| 2 | Application read surface: worklist, result detail, patient result sheet | [ ] Not started | VG-02 |
| 3 | Application write surface: enter, clear, refresh-range, review, unreview, print, deliver, bulk variants + authorization tests | [ ] Not started | VG-03 |
| 4 | Infrastructure proof + close-out | [ ] Not started | VG-04 |

---

## Slice 1: Domain: lifecycle guards, snapshot child entity, full status calculator + Domain tests + MIGRATION

- **Goal:** Make `PatientTest` enforce the result lifecycle; persist the BR-05 reference-range freeze as a dedicated child table (settled by the owner); implement `PatientStatusCalculator` in full from the settled PRD §8.2/§8.3 model.
- **Touches:** `src/TopLab.Domain/Results/PatientTest.cs` (modify — add `EnterResult` reviewed-guard, `ClearResult`, `Unreview`, `MarkReviewed` entered-guard, `MarkPrinted` reviewed-guard, `MarkDelivered` printed-guard, `MarkEntered(int, DateTime)`; no stateless snapshot intent methods); `src/TopLab.Domain/Results/PatientTestReferenceRangeSnapshot.cs` (create — thin EF entity with strongly-typed PK = `PatientTestId` + 9 scalar properties + `FromSnapshot` mapper); `src/TopLab.Infrastructure/Persistence/Configurations/PatientTestReferenceRangeSnapshotConfiguration.cs` (create — PK = `PatientTestId`, Cascade FK, decimal(18,4), nvarchar(500), tinyint enums, datetimeoffset); `ApplicationDbContext.DbSets.cs` (register `DbSet<PatientTestReferenceRangeSnapshot>`); **NEW MIGRATION `AddPatientTestReferenceRangeSnapshots`** (creates the `PatientTestReferenceRangeSnapshots` table — Module 4's only migration); `src/TopLab.Domain/PatientStatus/PatientStatusCalculator.cs` (modify — replace `NotImplementedException` with full PRD §8.2/§8.3 implementation: per-analysis stage from lifecycle columns, account stage from injected balance, `PatientStatus = state(min stage)`, S1/S2 "newly registered" micro-pin); `tests/TopLab.Domain.Tests/Results/PatientTestTests.cs` (extend — every guard above); `tests/TopLab.Domain.Tests/Results/PatientTestReferenceRangeSnapshotTests.cs` (create — `FromSnapshot` maps all ten fields 1:1); `tests/TopLab.Domain.Tests/PatientStatus/PatientStatusCalculatorTests.cs` (create — table-driven over S1–S7, min-over-stages + account condition, binding worked example {1,3,4} → S2, S5-with-balance case, S6 only when all delivered with balance, S7 when delivered and settled, S1/S2 "newly registered" boundary).
- **Validation Gate:** VG-01 — build zero/zero; Domain tests green; cumulative suite green; **migration applies cleanly on a fresh database and the existing three migrations + M02's `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex` still apply in order**; diff touches `src/TopLab.Domain/Results/`, `src/TopLab.Domain/PatientStatus/`, `Persistence/Configurations/` + `DbSets`, the new migration, Domain tests only.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §3.2; the seven states (S1 New / S2 / S3 / S4 / S5 / S6 / S7) implementing PRD §8.2/§8.3 verbatim; per-analysis stage mapping (entry→1, review→2, print→3, delivery→4, account→5/6) and `PatientStatus = state(min over all per-analysis stages AND the account condition)`; S1 iff no analysis has `EnteredAtUtc` set AND `RegistrationDateUtc` falls on the current UTC day; binding worked example {1,3,4}→S2; `EnterResult` guard message `"Result is reviewed; unreview first."`; `ClearResult` guard message `"Result is locked."`; `Unreview` guard message `"Printed or delivered results cannot be un-reviewed."`; `MarkReviewed` guard message `"Result not entered."`; `MarkPrinted` guard message `"Result not reviewed."`; `MarkDelivered` guard message `"Result not printed."`; `MarkEntered` guard message `"Result is reviewed; unreview first."`; `PatientTestReferenceRangeSnapshot` 1:1 PK = `PatientTestId`, no FK to `Tests` on the scalar (snapshot is a historical document, not a live reference), Cascade FK to `PatientTest` (the snapshot dies with its result row); no stateless snapshot intent methods on the aggregate (ADR note records why: would be dead code).
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `PatientTest.cs` (full shape + guard-free mutators), `PatientTestConfiguration.cs` (no `IsDeleted`, no Ref* columns), `ReferenceRangeSnapshot.cs` (verbatim "M-12 owns the shape; M-04 owns persistence"; ten-field record), `ReferenceRange.cs` (`Matches` semantics, `CaptureSnapshot`, `MaxCommentLength = 500`), `ReferenceRange.Matches` precedent (1:1 child pattern — `CultureResult`), `PatientStatusCalculator.cs` (`NotImplementedException` stub, "Full precedence delivered in M04" comment), `PatientAggregateStatus` (S1–S7, "Computed, never stored"), `ApplicationDbContext.DbSets.cs` (36 DbSets; add one), three existing migrations + M02's `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex` (apply-order check).
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: (1) `PatientTest.cs` mutator guards + `MarkEntered`; (2) `PatientTestReferenceRangeSnapshot.cs` entity with `FromSnapshot`; (3) `PatientTestReferenceRangeSnapshotConfiguration.cs`; (4) register `DbSet`; (5) generate the new migration `AddPatientTestReferenceRangeSnapshots`; (6) implement `PatientStatusCalculator` in full (replace stub); (7) `PatientTestTests.cs` (every guard); (8) `PatientTestReferenceRangeSnapshotTests.cs` (`FromSnapshot` 1:1); (9) `PatientStatusCalculatorTests.cs` (table-driven S1–S7 + worked example + S1/S2 boundary).
- [x] **Stage 5 — Execution:** Slice implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [x] **Stage 7 — Validation Gate:** VG-01 passed. Evidence: build/test output; migration apply-order check on fresh DB.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (authorized local):** `[M-04] Slice 1/4: Domain: lifecycle guards, snapshot child entity, full status calculator + Domain tests + MIGRATION — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.` — on `main`, never push.

---

## Slice 2: Application read surface: worklist, result detail, patient result sheet

- **Goal:** Read-only projections for the (future) results screen, the entry form, and the (future) print preview.
- **Touches:** `Features/ResultsEntry/Common/ResultsEntryDtos.cs` (create — `ResultWorklistItemDto`, `ResultEntryDto`, `FrozenRangeDto`, `PatientResultSheetDto`, `ResultSheetLineDto`; maps to M12's `ReferenceRangeDto` shape directly — same assembly); `Common/ResultsEntryAccessPolicy.cs` (create — `EditResults`, `ReviewResults`, `PrintResults`, `DeliverResults`); `Common/ResultFlagComputer.cs` (create — internal static, pure: `Compute` returns `Low`/`High`/`Normal`; `SelectMatch` implements the settled overlapping-range rule — sex-matched preferred over sex-null, then narrowest age band, then lowest id); `Common/BalanceProbe.cs` (create — internal static implementing the inlined formula over `IApplicationDbContext` for a `PatientId`); `Queries/GetResultWorklist/GetResultWorklistQuery.cs` (+Handler, +Validator — optional `Day`/`HasResult`/`IsReviewed`/`TestGroupId`/`ResultKind` filters, excludes soft-deleted patients, paged 50/500); `Queries/GetResultEntry/GetResultEntryQuery.cs` (+Handler, +Validator — single `PatientTestId` → `ResultEntryDto` with frozen range preferred into `FrozenRange`); `Queries/GetPatientResultSheet/GetPatientResultSheetQuery.cs` (+Handler, +Validator — all of a patient's tests with entry state + frozen ranges); `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` (extend — `List<PatientTestReferenceRangeSnapshot>` + branches); `tests/.../Features/ResultsEntry/` (create — worklist/entry/sheet handler tests + `ResultFlagComputerTests` with exhaustive truth table).
- **Validation Gate:** VG-02 — Application build zero/zero; all Application tests pass; every DTO field traced to a verified domain property; no write commands in this slice; no migration in this slice.

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §3.3; `AggregateStatus` is computed per patient via the settled `PatientStatusCalculator` over the patient's tests + a `BalanceProbe` balance — FR-M08-007 requires patient lists to display the §8 aggregate status icon; the right-panel patient list of the results screen (FR-M04-001) is such a list; worklist shows Simple rows and *also* SpecializedProfile/Culture rows (visibility here is intentional and stated; `ResultKind` filter lets the future UI narrow); the frozen range is what the report shows — BR-05: old values persist until explicitly refreshed; `ResultFlagComputer.SelectMatch` candidates = rows where `Matches(...)`; prefer `Sex == patient sex` over `Sex == null`; then narrowest `(AgeMax − AgeMin)`; then lowest id.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: M12's `ReferenceRangeDto` (same Application assembly — maps directly), `ReferenceRange.Matches` (age-unit-sensitive per BR-04/FR-M12-005; sex-null matches both; inclusive bounds), `PatientStatusCalculator` (now implemented per Slice 1), `ResultFlag` enum, `IApplicationDbContext`, `FakeApplicationDbContext` (29 lists — needs `List<PatientTestReferenceRangeSnapshot>` extension), `Error`/`Result` patterns, M02 pagination precedent (default 50 / max 500), reference-range query pattern.
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: DTOs → access policy → `ResultFlagComputer` (with `SelectMatch` rule) → `BalanceProbe` (private formula copy) → 3 queries (worklist, entry, sheet) → fake extension → handler tests + `ResultFlagComputerTests` (parse-fail / no-match / low / normal / high / boundary-inclusive / age-unit-mismatch / most-specific selection).
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-02 passed. Evidence: build/test output; no write commands; no migration.
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-04] Slice 2/4: Application read surface: worklist, result detail, patient result sheet — loop-engineering` + `Stages 1-10 verified. Gate VG-02 passed.` — on `main`, never push.

---

## Slice 3: Application write surface: registration guard, enter, clear, refresh-range, review, unreview, print, export, deliver, bulk variants + authorization tests

- **Goal:** The complete results-entry write surface with D1 registration atomicity, D2 local PDF export, D3 deterministic bulk-reprint consent, D4 nonblank simple values, D5 calculator reuse, the settled lifecycle guard chain, auto-review system action, snapshot freeze, and balance block.
- **Touches:** M02 `CreatePatientCommand`/handler/validator/tests (D1 atomic ordered-test requirement); `Common/BalanceProbe.cs` (D5 loader delegating to Domain calculator); all existing result commands; `ExportPatientReportPdfCommand`, report-PDF port and Infrastructure implementation/tests (D2); two-step bulk-print preflight/execution DTOs and tests (D3); `EnterResult` validator and message tests (D4); `DomainFailureTranslator`; relevant fakes, DI registration and authorization tests. No new migration or permission seed.
- **Validation Gate:** VG-03 — build zero/zero; all new tests green; cumulative suite green; no migration in this slice; no `PermissionConfiguration` change; diff limited to `Features/ResultsEntry/` + fake + tests.

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §3.4; `EnterResultCommand` rejects `Error.Conflict("النتيجة معتمدة؛ ألغِ الاعتماد أولاً.")` when `IsReviewed` (settled by the owner: lock on review); rejects `Error.Conflict("لا يمكن إدخال نتيجة إلا لتحليل بسيط.")` when `Test.ResultKind != Simple` (simple channel is for simple tests; profile/culture entry happens on specialized surfaces); computes flag via `ResultFlagComputer.Compute` when `ResultFlag` param is null (operator override wins when supplied); BR-05 freeze: upsert snapshot row (`FromSnapshot(match.CaptureSnapshot())`) when match non-null, delete any existing snapshot row when null; auto review-and-completion: when `SystemSettings.AutoReviewAndComplete` (read row `Id == 1` directly — M22 precedent) is true, immediately call `MarkReviewed(_currentUser.UserId, _dateTime.UtcNow)` as a system action (no `REVIEW_RESULTS` precondition); `ClearResult` translates the Domain guard to `Error.Conflict("لا يمكن مسح نتيجة معتمدة أو مطبوعة أو مسلمة.")`; `RefreshResultReferenceRange` rejects reviewed results (same lock rule); `MarkResultPrinted` applies balance block: `user.BlockPrintOnRemainingBalance && !_currentUser.IsAbsolutePermission && BalanceProbe.Balance(patientId) > 0` → `Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.")` (refused before printing; `Conflict` declared, asserted verbatim); `BLOCK_PRINT_ON_BALANCE` is **not** a runtime gate — it is the grantable per-user item backing the flag; `MarkResultDelivered` has **no balance gate** (FR-M09-003 places the block before printing for delivery; delivery is the physical handover — BR-08); bulk commands have skip-and-report semantics (rows failing guards are skipped, not errors); `ResultsEntryAuthorizationTests` assert each command's gate code + standard denial message.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `IAuthorizedRequest` template, `AuthorizationBehavior` (verbatim denial message), `ICurrentUserService`, `IDateTimeProvider`, `IApplicationDbContext` (incl. `SystemSettings`, `Users`, `PatientTests`, `PaymentOperations`, `PatientTestReferenceRangeSnapshots` sets), M22 direct settings read precedent, `Error`/`Result` patterns, M17 re-issue idempotency precedent, M21 `MarkAllSamplesDrawnForPatient` bulk UX precedent.
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: `DomainFailureTranslator` → `EnterResultCommand` (with auto-flag, snapshot, auto-review) → `ClearResultCommand` → `RefreshResultReferenceRangeCommand` → `ReviewResultCommand` → `UnreviewResultCommand` → `MarkResultPrintedCommand` (with balance block) → `MarkResultDeliveredCommand` → bulk variants (reviewed + printed) → handler tests (per-command + balance-block matrix with worked example prices 100+50, extra charge 20, payment 80 with discount 10, voided payment 999 ⇒ Charged 170, Paid 90, Balance 80) → `ResultsEntryAuthorizationTests`.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-03 passed. Evidence: build/test output; authorization theory tests green; grep gates clean.
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-04] Slice 3/4: Application write surface: enter, clear, refresh-range, review, unreview, print, deliver, bulk variants + authorization tests — loop-engineering` + `Stages 1-10 verified. Gate VG-03 passed.` — on `main`, never push.

---

## Slice 4: Infrastructure proof + close-out

- **Goal:** Prove the Slice 1 migration ↔ snapshot zero drift; pin the lifecycle FK/index matrix; integration-test the enter→review→print→deliver flow incl. snapshot persistence; close the module.
- **Touches:** `tests/TopLab.Infrastructure.Tests/Persistence/Configurations/F5ConfigurationTests.cs` (extend — assert `PatientTestReferenceRangeSnapshots` mapping: PK = `PatientTestId`, Cascade FK, decimal(18,4), nvarchar(500), tinyint enums, datetimeoffset); `tests/TopLab.Infrastructure.Tests/Persistence/ResultLifecyclePersistenceTests.cs` (create — InMemory end-to-end: create patient+test+`PatientTest`, enter result (snapshot row persisted), review, print (`PrintCount` increments), deliver; re-read and assert every column incl. snapshot row; assert cascade deleting the `PatientTest` removes the snapshot row); validator-registration test (Infrastructure-side host test used by prior modules — extend for all Module 4 validators); `Docs/Source/Top_Lab_ADR.md` (append ADR-0035 — reconfirm max ADR at execution); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M04 row to 🟩 Done + dated change-log row — verified at line 73); `Docs/Handoff_M04.md` (create per template).
- **Validation Gate:** VG-04 — Release build zero/zero; full suite green (`-m:1`); migration ↔ snapshot zero drift proven; coverage floors or waivers; ADR-0035 + handoff + tracking flip committed per convention; zero Presentation content (grep gate).

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §3.5; migration-scope gate: diff the new migration vs. `ApplicationDbContextModelSnapshot.cs` (table shape, PK/FK, column types/nullability) — expected exact match; drift → stop + addendum; ADR-0035 records: settled status-model implementation (PRD §8.2/§8.3 states + min-over-stages + account condition; the S1/S2 "newly registered" micro-pin); snapshot persistence as the settled BR-05 freeze in a dedicated child table (the storage mechanism settled by the owner) and the dropped stateless intent methods; the FR-M04-008 refresh command; the lifecycle guard chain and the owner-settled edit-lock policy (free re-entry until reviewed; `UnreviewResultCommand` gated on `REVIEW_RESULTS`); auto review-and-completion as a system action (§8.4); balance-block via per-user bool + delivery-not-gated (settled); the owner-settled most-specific range selection (sex-matched preferred, then narrowest age band, lowest id on a tie); simple-only `EnterResultCommand` channel rule; tracking-sheet M04 row verified at line 73.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `F5ConfigurationTests.cs`, `ApplicationDbContextModelSnapshot.cs` (relevant sections), the new migration, InMemory harness, `Top_Lab_ADR.md`, `Top_Lab_Master_Tracking_Sheet.md`, `Top_Lab_Handoff_Template.md` / `Handoff_M12.md` (template), M22 settings (`AutoReviewAndComplete`).
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: run migration-scope gate first (migration vs. snapshot; expect exact match) → extend `F5ConfigurationTests` for snapshot mapping → create `ResultLifecyclePersistenceTests` (InMemory end-to-end + cascade) → extend validator-registration test → ADR-0035 → tracking flip → handoff.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-04 passed. Evidence: Release build 0/0, full suite green, migration gate passed, coverage floors met or waivers documented, zero Presentation content (grep gate).
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-04] Slice 4/4: Infrastructure proof + close-out — loop-engineering` + `Stages 1-10 verified. Gate VG-04 passed.` — on `main`, never push.

---

## Current Status

- Overall: 1/4 slices done
- Slice 1 — Domain: lifecycle guards, snapshot child entity, full status calculator + Domain tests + MIGRATION: [x] Done (VG-01 passed: build 0/0, Domain 317 green, full suite 1107 green, migration AddPatientTestReferenceRangeSnapshots, has-pending-model-changes = no changes)
- Slice 2 — Application read surface: worklist, result detail, patient result sheet: [ ] Not started
- Slice 3 — Application write surface: enter, clear, refresh-range, review, unreview, print, deliver, bulk variants + authorization tests: [ ] Not started
- Slice 4 — Infrastructure proof + close-out: [ ] Not started

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-08 | 0 | — | Memory file created | OK | — |
| 2026-09-08 | 1 | 1-10 | S1 Domain guards + snapshot 1:1 + status calculator + migration; build 0/0; Domain 317 green; full 1107 green; VG-01 passed | OK | pending |

## Stop Report (append only if a stop condition triggers)
