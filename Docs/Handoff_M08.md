# Top-Lab — Handoff Document M-08

## نظام توب لاب — تسليم جلسة عمل (Module 8 — Patient Search, Lab ID & Visit History)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-11_M08-patient-search` |
| Session date (UTC) | 2026-09-11 |
| Session start (UTC) | 2026-09-11 |
| Session end (UTC) | 2026-09-11 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-08 |
| Module name | Patient Search, Lab ID & Visit History (backend only) |
| Wave | 6 |
| Feature folder(s) touched | `src/TopLab.Application/Features/PatientSearch/**` (Common + Queries), `tests/TopLab.Application.Tests/Features/PatientSearch/`, `tests/TopLab.Infrastructure.Tests/Persistence/` |
| Layers touched | Application + Infrastructure proof (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `d73b3bc` (live `main` HEAD after Slice 2) |
| Final commit at session end | Slice 3 close-out `[M-08] Slice 3/3: Infrastructure proof + close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 8 **Patient Search, Lab ID & Visit History** end-to-end in the three slices S1–S3 of `Docs/OpenCode/M-08.md` (Application read surface → settings-dependency verification → infrastructure proof + close-out). The work is **backend only** — no Presentation content. S1 ships the read surface: `SearchPatientsGlobal` (multi-criteria global search with per-hit settled aggregate status + test count), `GetPatientByLabId` (NotFound channel), `GetVisitHistory` (LabId row-grouping with the `!IsDeleted` sibling filter + rollup + settled balance + settings echo), and `GetVisitDetail` (flat-set phones/conditions + test lines + balance), all permission-light read queries carrying no `IAuthorizedRequest`. S2 verifies the settings dependency (round-trip via M-22's existing write/read handlers) and documents that no write surface is in scope. S3 proves zero model drift (the module re-asserts the existing `(LabId)` and M-02-owned `IsDeleted` indexes), integration-tests the LabId grouping rule over the real `ApplicationDbContext` with InMemory, and closes the module (ADR-0038 + tracking flip + this handoff). Build must be 0 errors / 0 warnings; full suite green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Application read surface** — Implementation Complete — `Features/PatientSearch/Common/{PatientSearchDtos,BalanceProbe,VisitRollup}.cs`; 4 query/handler/validator triplets (`SearchPatientsGlobalQuery` — exact-trimmed `LabId`/`NationalId` channels, any stored phone via `PatientPhoneNumber`, assist-gated name search, exempt-filled/page-sized hits, per-hit settled `AggregateStatus` + `TestCount` with the {3,4,1}→S2 truth set; `GetPatientByLabIdQuery` — trimmed exact LabId lookup with NotFound `لا يوجد مريض بهذا الكود.`; `GetVisitHistoryQuery` — LabId row-grouping with the `!IsDeleted` sibling filter, rollup in `RegistrationDateUtc`-desc order, worked-example balance Charged 170 / Paid 90 / Balance 80, settings echo + asymmetric missing-settings policy; `GetVisitDetailQuery` — flat-set phones/conditions + test lines by `PatientTestId` + `BalanceProbe`). The seven-state calculator exists on `main` — first-shipper contingency **not invoked**. 4 handler-test files (29 tests). Commit `c3bef9d` `[M-08] Slice 1/3: ...`.
- **S2 — Settings-dependency verification (no write surface)** — Implementation Complete — `SettingsDependencyTests.cs` (2 facts: `HistorySortMode` both values + `HistoryAutoDisplayEnabled` toggle round-trip via M-22's `UpdateReportSettingsCommandHandler` → `GetReportSettingsQueryHandler` over the fake). The module deliberately ships **no write surface**; the round-trip proves the read path consumes the M-22-settable settings. Commit `d73b3bc` `[M-08] Slice 2/3: ...`.
- **S3 — Infrastructure proof + close-out** — Implementation Complete — migration-scope gate: `dotnet ef migrations has-pending-model-changes` = "No changes have been made to the model since the last migration" (**zero drift**; no schema touch — the module re-asserts only the indexes it consumes, already covered by the existing `F5ConfigurationTests.Patient_HasExpectedColumns` (`(LabId)`) and `Patient_HasIndexOnIsDeleted` (M-02-owned)); new `tests/TopLab.Infrastructure.Tests/Persistence/PatientSearchPersistenceTests.cs` (1 test: real `ApplicationDbContext` over InMemory — two registrations sharing LabId `LAB-100` persisted and reloaded, unrelated `LAB-200` registration excluded by the history query, which returns exactly the two siblings); new `tests/TopLab.Application.Tests/Features/PatientSearch/PatientSearchAuthorizationTests.cs` (theory over all four query types: `typeof(IAuthorizedRequest).IsAssignableFrom(type)` is false and `typeof(MediatR.IBaseRequest).IsAssignableFrom(type)` is true); ADR-0038; tracking-sheet M08 row → 🟩 Done + dated change-log row; this handoff. Commit `[M-08] Slice 3/3: ...`.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Release, `-m:1` posture).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -c Release -m:1`): **1431 green** = 360 Domain.Tests + 948 Application.Tests + 123 Infrastructure.Tests.
- New tests added: S1 +29 Application, S2 +2 Application, S3 +4 Application (authorization-shape theory) +1 Infrastructure (persistence proof).
- Tests currently failing: none.
- Coverage of the M-08 footprint: Application — search hit aggregate-status/test-count (mixed stages {3,4,1}→S2, name-assist on/off), exact-trimmed LabId/NationalId channels, phone-any-row match, Lab-ID NotFound + soft-deleted cases, visit grouping/ordering/rollup, `!IsDeleted` sibling exclusion, worked-example balance, settings echo + asymmetric missing-settings policy (search default-false / history Unexpected), null-LabId single-visit convention with the empty-string LabId, detail-line ordering, authorization-shape theory over all four queries; Infrastructure — InMemory LabId-grouping proof on the real context and the existing index re-assertions. No coverage waiver; the M-03 posture applies (validators auto-registered via the existing assembly scan, handler-direct tests bypass the pipeline).

### 4.3 Migrations

- New EF Core migration(s) added: **None** — the module consumes existing F5/M-02 schema and indexes only.
- Migration order verified: unchanged (Baseline → RenamePkColumns → AddTestCodeAndLifecycleColumns → M02 → M04 → M05 → M06).
- `has-pending-model-changes` at close-out: no changes — zero drift.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: none.
- Validators: 4 new validators (`SearchPatientsGlobalQueryValidator`, `GetPatientByLabIdQueryValidator`, `GetVisitHistoryQueryValidator`, `GetVisitDetailQueryValidator`) resolve via the existing assembly scan (`AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()`); no DI wiring change.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the 13-row `PermissionConfiguration.cs` seed: **none** — all four M-08 queries are permission-light reads carrying no `IAuthorizedRequest` (pinned by the authorization-shape test).

---

## 5. Work In Progress (Required — mark "None" if none)

None. All three slices reached a terminal state; module closed out in the Master Tracking Sheet (§4 M08 row + §9 change-log row) and this handoff. Natural next steps (search/lookup/history Presentation consuming the S1 read surface; the P/T audit wave re-using these read paths; M07 history reports on top of the visit-rollup shape) are out of scope of this plan.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

All are recorded in ADR-0038 and were settled or owner-confirmed before/during execution:

- **Decision (dedicated read overlap with M-02):** M-08 re-queries the same tables with its own projections and imports no M-02 handler/DTO; the M-02 surface stays the registration-screen contract, M-08 the search/lookup/history contract.
  - **Scope of impact:** `Features/PatientSearch/**`; no handler reuse across features.
  - **Follow-up required:** No.
- **Decision (exact-trimmed LabId/NationalId channels):** global search matches `LabId`/`NationalId` by exact trimmed equality (unlike M-02's `Contains`); phones match any stored `PatientPhoneNumber` row.
  - **Scope of impact:** `SearchPatientsGlobalQueryHandler` + validator.
  - **Follow-up required:** No.
- **Decision (`!IsDeleted` sibling filter beyond M-02):** visit history for a non-null-LabId patient loads all sharing rows and excludes soft-deleted ones; null-LabId history is the single visit with `LabId = string.Empty`.
  - **Scope of impact:** `GetVisitHistoryQueryHandler`; deviates deliberately from the M-02 history behavior (hardening).
  - **Follow-up required:** No.
- **Decision (flat-set reads with deterministic ordering):** phones/conditions read from `Set<PatientPhoneNumber>`/`Set<PatientMedicalCondition>` (not navigation collections), ordered by `SortOrder`/`MedicalConditionTypeId`; visit test lines ordered by `PatientTestId`.
  - **Scope of impact:** `VisitRollup` + `GetVisitDetailQueryHandler`.
  - **Follow-up required:** No.
- **Decision (asymmetric settings policy):** history echoes `HistorySortMode`/`HistoryAutoDisplayEnabled` and returns `Unexpected("سجل إعدادات التقرير مفقود.")` when `ReportSettings` is absent (M-22 mirror); search treats a missing `SystemSettings` row as name-assist disabled (graceful default).
  - **Scope of impact:** both history queries vs. `SearchPatientsGlobalQueryHandler`.
  - **Follow-up required:** No.
- **Decision (permission-light read surface):** all four queries carry no `IAuthorizedRequest`; the authorization-shape test pins the absence so Presentation is not forced to gate them.
  - **Scope of impact:** `PatientSearchAuthorizationTests`; no `PermissionConfiguration`/seed change.
  - **Follow-up required:** No.
- **Decision (settled status via the Domain calculator):** `PatientStatusCalculator` already exists on `main` — the first-shipper contingency was **not invoked**; M-08 imports the calculator, never re-implements or caches it.
  - **Scope of impact:** none (observed, recorded).
  - **Follow-up required:** No.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** None (the M-08/documented deviations from M-02 behavior are intentional ADR-0038 decisions, not corrections).
- **Waiver:** None (no coverage waiver — exercised paths enumerated in §4.2).

---

## 8. Required Reading (Required — mark "None" if none)

- **M-08 Implementation Plan** — `Docs/OpenCode/M-08.md` (this session's source of truth; §4–§6).
- **M-08 Loop-Engineering Memory** — `Docs/OpenCode/M-08-memory.md` (slice index, gates VG-01..VG-03, per-slice 10-stage checklists, execution log).
- **ADR-0038** — `Docs/Source/Top_Lab_ADR.md` (dedicated read overlap, exact-trim channels, `!IsDeleted` sibling filter, flat-set reads, asymmetric settings policy, permission-light reads, zero-migration outcome).
- **M-02 Handoff** — `Docs/Handoff_M02.md` (the `Patient`/`PatientPhoneNumber`/`PatientMedicalCondition` model, `LabId`/`IsDeleted` schema owner, the search precedent M-08 deviates from deliberately).
- **M-04 Handoff** — `Docs/Handoff_M04.md` (for `PatientStatusCalculator` seven-state semantics and the balance-block posture M-08's `BalanceProbe` reuses).
- **M-03 Handoff** — `Docs/Handoff_M03.md` (for `PatientAccountCalculator` balance formula the feature-local `BalanceProbe` delegates to).
- **M-22 Handoff** — `Docs/Handoff_M22.md` (for `HistorySortMode`/`HistoryAutoDisplayEnabled` and the `GetReportSettingsQueryHandler` the settings echo mirrors).
- **M-06 Handoff** — `Docs/Handoff_M06.md` (close-out precedent — ADR/tracking/handoff shape this slice mirrors).

---

*End of document.*