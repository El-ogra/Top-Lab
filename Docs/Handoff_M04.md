# Top-Lab — Handoff Document M-04

## نظام توب لاب — تسليم جلسة عمل (Module 4 — Results Entry & Result Lifecycle)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-08_M04_results-entry-result-lifecycle` |
| Session date (UTC) | 2026-09-08 |
| Session start (UTC) | 2026-09-08 |
| Session end (UTC) | 2026-09-08 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-04 |
| Module name | Results Entry & Result Lifecycle (backend only) |
| Wave | 5 |
| Feature folder(s) touched | `src/TopLab.Domain/Results/`, `src/TopLab.Domain/PatientStatus/`, `src/TopLab.Application/Features/ResultsEntry/`, `src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/` (D1), `src/TopLab.Infrastructure/Persistence/Configurations/` + `DbSets` + migration `AddPatientTestReferenceRangeSnapshots`, `src/TopLab.Infrastructure/Services/PatientReportPdfExporter.cs`, `tests/TopLab.Domain.Tests/Results/`, `tests/TopLab.Domain.Tests/PatientStatus/`, `tests/TopLab.Application.Tests/Features/ResultsEntry/`, `tests/TopLab.Application.Tests/Features/PatientRegistration/Commands/`, `tests/TopLab.Infrastructure.Tests/Persistence/`, `tests/TopLab.Infrastructure.Tests/Services/` |
| Layers touched | Domain + Application + Infrastructure proof (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `ef3008b` (live `main` HEAD after M-03; the plan's reference commit `70f145e` is historical-only) |
| Final commit at session end | `(filled at commit time)` |

---

## 2. Session Objective (Required)

Implement Module 4 **Results Entry & Result Lifecycle** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-04.md` (Domain guards + snapshot child + full status calculator + migration → read surface → write surface with D1–D5 → infrastructure proof + close-out). The work is **backend only** — no Presentation content. S1 enforces the lifecycle on `PatientTest`, persists the BR-05 freeze as a 1:1 child table (the module's only migration), and delivers `PatientStatusCalculator` in full with the S1/S2 UTC-day micro-pin. S2 ships the three ungated read queries plus `ResultFlagComputer` (most-specific selection) and `BalanceProbe` (D5 delegation). S3 ships the write surface with D1 atomic registration, D4 exact whitespace message, clear/refresh/review/unreview/print with per-user balance block, deliver with no balance gate, D2 local PDF export via a testable port, and D3 two-step bulk-print consent with the mandated Arabic question. S4 proves migration↔snapshot zero drift, pins the mapping, integration-tests the lifecycle including cascade, extends validator registration, and closes the module (ADR-0035 + tracking flip + this handoff). Build must be 0 errors / 0 warnings; full suite green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain + migration** — Implementation Complete — `PatientTest` guards (`EnterResult`/`ClearResult`/`Unreview`/`MarkReviewed`/`MarkPrinted`/`MarkDelivered` + `MarkEntered`), new `PatientTestReferenceRangeSnapshot` 1:1 entity (`FromSnapshot(PatientTestId, ReferenceRangeSnapshot)` — PK cannot come from the record alone, recorded in ADR-0035), configuration (PK, Cascade FK, decimal(18,4), nvarchar(500), tinyint enums, datetimeoffset) + `DbSet`, migration `AddPatientTestReferenceRangeSnapshots`, full `PatientStatusCalculator` (min-over-stages + account condition + S1/S2 pin + worked example {1,3,4}→S2), Domain tests (guards, snapshot 1:1, status S1–S7). Commit `[M-04] Slice 1/4: ...`.
- **S2 — Application read surface** — Implementation Complete — `ResultsEntryDtos` (5 records, M12 `ReferenceRangeDto` reused), `ResultsEntryAccessPolicy` (4 codes), `ResultFlagComputer` (Compute/SelectMatch with sex-matched → narrowest → lowest-id), `BalanceProbe` (D5 loader delegating exclusively to `PatientAccountCalculator.Balance`), 3 ungated queries (worklist with day/has-result/reviewed/group/kind filters + per-patient aggregate status, entry detail with live ranges + frozen range, patient sheet) + validators, fake extension (`PatientTestReferenceRangeSnapshots` + `ProfileResultItems`/`CultureResults` for export flavors), 29 handler tests. Commit `[M-04] Slice 2/4: ...`.
- **S3 — Application write surface + D1–D5** — Implementation Complete — D1 `CreatePatientCommand` extended with `AddTestInput` tests, single-save atomic handler + validator + 4 new tests; `DomainFailureTranslator` (English guard → frozen Arabic); `EnterResult` (D4 exact message in validator + handler defense, wrong-kind Conflict, auto-flag vs override, snapshot upsert/delete, auto-review on/off as system action); `ClearResult`; `RefreshResultReferenceRange` (FR-M04-008 persist-until-refresh + replace + flag recompute, reviewed rejected); `ReviewResult` (idempotent); `UnreviewResult`; `MarkResultPrinted` (verified-eligibility Conflict + per-user balance-block Conflict with worked-example matrix + absolute/flag-off/settled allows); `MarkResultDelivered` (no balance gate, proven by test); D2 `ExportPatientReportPdfCommand` + `IPatientReportPdfExporter` port + Infrastructure `PatientReportPdfExporter` (minimal valid PDF, `CreateNew` no-overwrite, DI Scoped) with handler semantics (verified-eligibility Conflict, existing-file Conflict with no overwrite, Validation on path, Unexpected on I/O with no marks, export-only marks, no PrintCount/lifecycle/balance change); D3 `MarkAllPatientResultsReviewed` + two-step bulk print (preflight `RequiresReprintConfirmation`, execution Confirm reprints with audit increment / Cancel skips entire report and continues, balance block once per patient, mandated question pinned as `BulkPrintMessages.ReprintConfirmationMessage`); `ResultsEntryAuthorizationTests` + validator tests. Commit `[M-04] Slice 3/4: ...`.
- **S4 — Infrastructure proof + close-out** — Implementation Complete — migration-scope gate: `has-pending-model-changes` = "No changes..." + ordered list (3 baseline + M02 + M04); `F5ConfigurationTests.PatientTestReferenceRangeSnapshot_HasExpectedMapping`; `ResultLifecyclePersistenceTests` (InMemory enter→review→print×2→deliver round-trip + snapshot + cascade delete); `ValidatorRegistrationTests.HostBuiltLikeApp_ResolvesM04Validators` (14 validators); ADR-0035; tracking-sheet M04 row → 🟩 Done + dated change-log row; this handoff. Commit `[M-04] Slice 4/4: ...`.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Release, `-m:1` posture).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -m:1`): **1223 green** = 317 Domain.Tests + 818 Application.Tests + 88 Infrastructure.Tests.
- New tests added: +146 (30 Domain + 107 Application + 9 Infrastructure).
- Tests currently failing: none.
- Coverage of the M-04 footprint: new Domain logic (guards, snapshot mapper, calculator) at 100% by dedicated tests; Application handlers/queries/computers covered by 107 tests including every guard negative path, flag truth table, status S1–S7 table, balance-block matrix with the shared worked example, export Conflict/Validation/Unexpected paths, and bulk Confirm/Cancel paths; Infrastructure mapping + round-trip + exporter paths green. No waiver requested; floors met by construction (same posture as M-03: validators resolve via assembly scan, handler-direct tests bypass the pipeline).

### 4.3 Migrations

- New EF Core migration(s) added: **One** — `20260908175555_AddPatientTestReferenceRangeSnapshots` (creates `PatientTestReferenceRangeSnapshots`, PK `PatientTestId`, Cascade FK to `PatientTests`, decimal(18,4)×2, nvarchar(500)×2, tinyint×2, datetimeoffset). This is Module 4's only migration.
- Migration order verified: Baseline → RenamePkColumns → AddTestCodeAndLifecycleColumns → M02 `AddPatientIsDeletedAndPatientTestSampleDrawnIndex` → M04 `AddPatientTestReferenceRangeSnapshots` (all Pending on a fresh DB, in order).
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: `IPatientReportPdfExporter` → `PatientReportPdfExporter` as Scoped in `src/TopLab.Infrastructure/DependencyInjection.cs` (D2 port implementation).
- Validators: 14 new validators resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (no DI wiring change); `ValidatorRegistrationTests` confirms all 14.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the 13-row `PermissionConfiguration.cs` seed: **none** — `git diff src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` is empty.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All four slices reached a terminal state; module closed out in the Master Tracking Sheet (§4 M04 row + §9 change-log row) and this handoff. Natural next steps (results-screen/print-preview Presentation consuming the S2/S3 surface; M05 profile + M06 culture entry on top of `MarkEntered`; M07/M09 reporting/delivery) are out of scope of this plan.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

All were owner-confirmed verbatim before/during execution and are recorded in ADR-0035 (no re-derivation):

- **Decision (seven-state calculator):** Per-analysis stage from lifecycle columns, account stage from injected balance, `PatientStatus = state(min stage)`; S1 iff none entered AND registration today (UTC-day micro-pin); worked example {1,3,4}→S2; S5 precedes settlement; S6 only when all delivered with balance; S7 when settled/credit.
  - **Scope of impact:** `PatientStatusCalculator` + worklist `AggregateStatus`; pinned by table-driven tests.
  - **Follow-up required:** No.
- **Decision (edit-lock policy):** Free re-entry until reviewed; after `IsReviewed`, only a `REVIEW_RESULTS` holder may un-review-then-re-enter via `UnreviewResultCommand`.
  - **Scope of impact:** `PatientTest` guards + 4 gated commands.
  - **Follow-up required:** No.
- **Decision (freeze storage):** Dedicated 1:1 child table `PatientTestReferenceRangeSnapshots` (Cascade); no stateless intent methods; `FromSnapshot(PatientTestId, snapshot)` signature clarification.
  - **Scope of impact:** Domain entity + configuration + the module's only migration.
  - **Follow-up required:** No.
- **Decision (D1 atomic registration):** `CreatePatientCommand` requires ≥1 ordered test, single save; empty is Validation; `AddTestsToVisit` is edit-existing only.
  - **Scope of impact:** M02 `CreatePatient` shape/handler/validator/tests.
  - **Follow-up required:** No.
- **Decision (D2 local PDF export):** Absolute `.pdf` path, verified-eligibility, no balance gate, no lifecycle/PrintCount change, Conflict on existing with no overwrite, Validation on path, Unexpected on I/O with no marks, export-only marks after success; port + Infrastructure implementation, no seed/migration.
  - **Scope of impact:** `ExportPatientReportPdfCommand` + exporter + DI.
  - **Follow-up required:** Yes — Presentation later chooses/passes the path and asks the D3 question (out of this module).
- **Decision (D3 bulk reprint consent):** Two-step preflight (`RequiresReprintConfirmation`) + explicit Confirm/Cancel per patient; mandated question `لقد تم طباعه هذا التقرير لهذا المريض من قبل هل ترغب في اعاده الطباعه`; Confirm reprints with audit increment; Cancel skips entire report and continues; no guard bypass; non-idempotent by design.
  - **Scope of impact:** Bulk preflight/execution contract.
  - **Follow-up required:** Yes — Presentation later renders the question (out of this module).
- **Decision (D4 simple-value guard):** Null/empty/whitespace rejected before mutation with exactly `الرجاء إدخال قيمة النتيجة قبل الحفظ`; numeric/symbolic non-whitespace valid.
  - **Scope of impact:** `EnterResultCommandValidator` + handler defense.
  - **Follow-up required:** No.
- **Decision (D5 calculator reuse):** `BalanceProbe` loads only and delegates exclusively to `PatientAccountCalculator.Balance`; no duplicated formula.
  - **Scope of impact:** All balance-gated paths (single + bulk print).
  - **Follow-up required:** No.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** The plan cites a "29-list fake" — the live tree has a 33-list fake (extended to 36 with `PatientTestReferenceRangeSnapshots` + `ProfileResultItems`/`CultureResults` for the export flavor coverage).
  - **Waiver:** No waiver required — owner-corrected "live tree governs" applied; gates used "FULL suite green".
  - **Pinned by:** Full-suite green at every slice gate (1107 → 1136 → 1206 → 1223).
- **Deviation:** The plan snippet shows `FromSnapshot(ReferenceRangeSnapshot)` single-parameter — the 1:1 PK cannot come from the record alone.
  - **Waiver:** No waiver — implemented as `FromSnapshot(PatientTestId, ReferenceRangeSnapshot)` and recorded as an implementation clarification in ADR-0035.
  - **Pinned by:** `PatientTestReferenceRangeSnapshotTests` (all ten fields + PK) + persistence round-trip.
- **Deviation:** The F5 `tinyint`/`datetimeoffset` column-type annotations are not observable through the InMemory provider (`GetColumnType()` throws; `GetProviderClrType()` is null for the nullable enum).
  - **Waiver:** Asserted the InMemory-observable surface instead (nullability + CLR types + precision/scale/max-length + PK/FK/delete behavior); the column types themselves are pinned by the zero-drift gate (model snapshot unchanged since the migration).
  - **Pinned by:** `PatientTestReferenceRangeSnapshot_HasExpectedMapping` + `has-pending-model-changes` = no changes (same posture as the M-03 `tinyint` waiver).
- **Waiver:** None (no coverage waiver — exercised paths listed in §4.2).

---

## 8. Required Reading (Required — mark "None" if none)

- **M-04 Implementation Plan** — `Docs/OpenCode/M-04.md` (this session's source of truth; §3.1.1 D1–D5 override any older conflicting text).
- **M-04 Loop-Engineering Memory** — `Docs/OpenCode/M-04-memory.md` (slice index, gates VG-01..VG-04, per-slice 10-stage checklists, execution log).
- **ADR-0035** — `Docs/Source/Top_Lab_ADR.md` (status model, freeze table, guard chain, auto-review, balance block, selection rule, simple-only channel, D1–D5).
- **M-12 Handoff** — `Docs/Handoff_M12.md` (for the `ReferenceRangeDto` surface reused by the entry detail).
- **M-22 Handoff** — `Docs/Handoff_M22.md` (for `SystemSettings.AutoReviewAndComplete` direct-read precedent).
- **M-02 Handoff** — `Docs/Handoff_M02.md` (for `Patient.IsDeleted` / `PriceAtOrderTime` / `TestPriceResolver` baseline D1 builds on).
- **M-03 Handoff** — `Docs/Handoff_M03.md` (for `PatientAccountCalculator.Balance` D5 delegates to + the worked-example prices).

---

*End of document.*
