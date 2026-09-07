# Top-Lab — Handoff Document M-02

## نظام توب لاب — تسليم جلسة عمل (Module 2 — Patient Registration & Test Ordering)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-07_M02_patient-registration-test-ordering` |
| Session date (UTC) | 2026-09-07 |
| Session start (UTC) | 2026-09-07 |
| Session end (UTC) | 2026-09-07 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-02 |
| Module name | Patient Registration & Test Ordering (backend only) |
| Wave | 4 |
| Feature folder(s) touched | `src/TopLab.Domain/Patients/`, `src/TopLab.Domain/Results/PatientTest.cs`, `src/TopLab.Application/Features/PatientRegistration/`, `src/TopLab.Infrastructure/Persistence/Configurations/{Patient,PatientTest}Configuration.cs` + new migration `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs` |
| Layers touched | Domain / Application / Infrastructure (tests + close-out) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `70f145e` (after M-01) |
| Final commit at session end | `(filled at commit time)` |

---

## 2. Session Objective (Required)

Implement Module 2 **Patient Registration & Test Ordering** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-02.md` (Domain mutators → Application read surface → Application write surface → Infrastructure proof + close-out), satisfying FR-M02-001 … FR-M02-012 (and the soft-delete flag for M-10 audit needs). The work is **backend only** — Domain + Application + Infrastructure; no Presentation content. S3 ships the full M-02 write surface **fully integrated against M-13's final surface** (no stub-then-gate split): both `AddTestsToVisit` and `AddCustomGroupToVisit` delegate to a shared pure `TestPriceResolver` (`internal`) and route through M-13's `GetPriceListByIdQuery` / `GetCustomGroupByIdQuery` via intra-Application MediatR. All writes are `IAuthorizedRequest` with `ADD_EDIT_PATIENT` (id 1) except `SoftDeletePatient` which uses `DELETE_PATIENT` (id 9) — both codes are already seeded in the 13-row catalog (settled OD-8); no new permission row is added (ADR-0032). One new M-02 migration is added: `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs` (the `IsDeleted` column on `Patients` and the `(PatientId, IsSampleDrawn)` composite index on `PatientTests` — the only schema dependency M-21 has on M-02). Build must be 0 errors / 0 warnings (Debug + Release); all tests green; FK-matrix incl. negative no-FK assertions; zero drift against `ApplicationDbContextModelSnapshot.cs`. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain mutators and invariants** — Implementation Complete — `src/TopLab.Domain/Patients/Patient.cs` (`SetPhoneNumbers(IEnumerable<PatientNumberInput>)` replace-list with trim + skip-empty, `AddMedicalCondition(MedicalConditionTypeId)` idempotent, `RemoveMedicalCondition(MedicalConditionTypeId)`, `IsDeleted` property + `SoftDelete()`/`Restore()` idempotent, `EnsureNotDeleted` guard added to `Update`/`AssignLabId`); `src/TopLab.Domain/Patients/PatientMedicalCondition.cs` (`public static Create(PatientId, MedicalConditionTypeId)` factory); `src/TopLab.Domain/Patients/PatientNumberInput.cs` (new record); `src/TopLab.Domain/Results/PatientTest.cs` (`UpdateSampleFlags(bool, bool, bool, bool, bool, bool)` per-test state-setter — settled OD-6); `src/TopLab.Infrastructure/Persistence/Configurations/PatientConfiguration.cs` (`IsDeleted` bit not null + index); `src/TopLab.Infrastructure/Persistence/Configurations/PatientTestConfiguration.cs` (`(PatientId, IsSampleDrawn)` composite index); `src/TopLab.Infrastructure/Persistence/Migrations/20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs` (new migration — only M-02 migration; Designer + `ApplicationDbContextModelSnapshot.cs` updated). 270 Domain.Tests (+14 new tests). Commit `[M-02] Slice 1/4: ...`.

- **S2 — Application read surface** — Implementation Complete — DTOs (`PatientSummaryDto`/`PatientDetailDto`/`PatientPhoneNumberDto`/`PatientMedicalConditionDto`/`PatientTitleDto`/`MedicalConditionTypeDto`/`PatientTestSummaryDto`/`VisitHistoryDto`/`LabIdAvailabilityDto`/`RegistrationCatalogDto`); `Common/PatientRegistrationAccessPolicy.cs` (`AddEditPatient`/`DeletePatient` constants — settled OD-8); 6 unauthorized plain `IRequest<Result<...>>` queries (`SearchPatients` with BR-03 any-phone-number match, `GetPatientById` with soft-delete NotFound, `GetPatientVisitHistory` grouped by visit, `GetPatientTitles`, `GetMedicalConditionTypes`, `GetRegistrationCatalog` aggregating M12 `SearchTestCatalogQuery` + M12 `GetTestGroupsQuery` + this-slice `GetPatientTitlesQuery` + `GetMedicalConditionTypesQuery` + M22 `GetSystemSettingsQuery` for `DefaultAccountType` + `DisableAutoTitleInsertion` + M14 `ReferralNameResolver.Resolve(null, Sex)` for the two placeholders — settled OD-4); `FakeApplicationDbContext` extension with `PatientPhoneNumbers`/`PatientMedicalConditions`/`PatientTitles`/`MedicalConditionTypes` lists. 582 Application.Tests (+21 new tests).

- **S3 — Application write surface (integrated against M-13's final surface)** — Implementation Complete — 10 commands (`CreatePatientCommand`, `UpdatePatientCommand`, `SoftDeletePatientCommand` with `DELETE_PATIENT`, `AddMedicalConditionCommand`/`RemoveMedicalConditionCommand`, `AddTestsToVisitCommand`/`AddCustomGroupToVisitCommand` **fully integrated** against M-13's final surface per §3.1 of M-02 plan — no stub-then-gate split, `RemoveTestFromVisitCommand`, `UpdatePatientTestSampleFlagsCommand`, `ClearAllTestsCommand`) — all `IAuthorizedRequest` with `ADD_EDIT_PATIENT` except `SoftDeletePatientCommand` (settled OD-8); `Common/TestPriceResolver.cs` (`internal` pure static — single source of truth for 4-step pricing algorithm shared by both `AddTestsToVisit` and `AddCustomGroupToVisit`); 6 validators (`CreatePatientCommandValidator`/`AddMedicalConditionCommandValidator`/`AddTestsToVisitCommandValidator`/`AddCustomGroupToVisitCommandValidator`/`RemoveTestFromVisitCommandValidator`/`UpdatePatientTestSampleFlagsCommandValidator`); `tests/.../Common/Fakes/FakeSender.cs` (hand-rolled `ISender` shim for M-02 → M-13 MediatR testing — pattern-matches `GetPriceListByIdQuery` and `GetCustomGroupByIdQuery`, returns canned `Result<PriceListDetailDto>` / `Result<CustomGroupDetailDto>`); `InternalsVisibleTo("TopLab.Application.Tests")` added to `TopLab.Application.csproj` so `TestPriceResolverTests` can resolve the `internal` resolver; ADR-0032 appended to `Docs/Source/Top_Lab_ADR.md` recording permission re-use + `TestPriceResolver` placement + `FakeSender` test seam + per-test sample-flag rule + contract-with-price-list-missing-test rule. 641 Application.Tests (+59 new tests; 8 `TestPriceResolverTests` cases pin every pricing branch).

- **S4 — Infrastructure proof + close-out** — Implementation Complete — Migration-scope zero-drift gate: `dotnet ef migrations has-pending-model-changes` returned "No changes have been made to the model since the last migration" (the migration script verifies the column + index additions and the FK-matrix is unchanged); `F5ConfigurationTests` extended with `Patient_IsDeleted_IsBitNotNull` + `Patient_HasIndexOnIsDeleted` + `PatientTest_HasCompositeIndex_OnPatientIdAndIsSampleDrawn`; `PatientDeleteBehaviorTests` new file — FK matrix: `DeletingPatient_CascadesToPatientPhoneNumber` (Cascade), `DeletingPatient_CascadesToPatientMedicalCondition` (Cascade), `DeletingPatient_CascadesToPatientTest` (Cascade) — and 2 negative no-FK assertions (`PatientPhoneNumber_HasNoModelForeignKeyToPatientPhoneNumberType_BaselineDocumented`, `PatientTest_HasNoModelForeignKeyToTestGroup_BaselineDocumented`); `ValidatorRegistrationTests` extended with 6 new `IValidator<M-02*Command>` resolution cases; `AddTestsToVisitPersistenceTests` new file — EF Core InMemory integration test that pins the M-13-precondition assumption at the Infrastructure layer (registers a contract referral entity with `PriceListId`; creates the matching `PriceList` + `PriceListItem`; creates a contract patient; persists a `PatientTest` row carrying the price-list price; verifies the round-trip via `AsNoTracking`); Master Tracking Sheet M-02 row → 🟩 Done + dated change-log row; this handoff created; Release build 0/0; full suite 996/996 (270 + 647 + 79).

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Debug and Release).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (Release, `dotnet test TopLab.sln -m:1` posture per M12/M14/M13/M15 precedent): **996 green** = 270 Domain.Tests + 647 Application.Tests + 79 Infrastructure.Tests.
- New tests added: +175 net since baseline (529 at session start — the plan's "529-test baseline + new tests" number — to 996).
- Tests currently failing: none.

### 4.3 Migrations

- New EF Core migration(s) added: **One — `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs`** (the only new M-02 migration).
- Migration-scope zero-drift gate: ran via `dotnet ef migrations has-pending-model-changes` → "No changes have been made to the model since the last migration" (zero drift). `dotnet ef migrations script --idempotent` confirms `ALTER TABLE [Patients] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit)` + two indexes. The migration matches `ApplicationDbContextModelSnapshot.cs` (Designer + snapshot updated). No addendum migration required.
- `ApplicationDbContextModelSnapshot.cs`: **updated** to reflect the new column + two indexes (this is the only time this file is touched in M-02).
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: None. Validators are auto-discovered by `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (introduced in M-12); `ValidatorRegistrationTests` confirms all 6 M-02 validators (CreatePatient, AddMedicalCondition, AddTestsToVisit, AddCustomGroupToVisit, UpdatePatientTestSampleFlags, RemoveTestFromVisit) resolve through the host.
- `InternalsVisibleTo("TopLab.Application.Tests")` added to `TopLab.Application.csproj` so `TestPriceResolverTests` can resolve the `internal` resolver.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the 13-row `PermissionConfiguration.cs` seed: **none** — settled OD-8 grep gate: `git diff src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` is empty.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All four slices reached a terminal state; module closed out in the Master Tracking Sheet (§4/§9) and this handoff.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

The seven formerly-open decisions (OD-2, OD-3, OD-4, OD-5, OD-6, OD-7, OD-8, OD-9) are all settled by explicit owner confirmation of the V2 plan's own recommended options in every case. They are recorded here for traceability and are encoded directly in the slices, message tables, and rules (see M-02.md §2.1–§2.6).

- **Decision (settled OD-4):** M-02 honors `SystemSettings.DefaultAccountType` and the referral-entity default placeholder via a non-coupling read against the existing M-22 `GetSystemSettingsQuery` (intra-Application MediatR) and calls M-14's `ReferralNameResolver.Resolve(null, sex)` directly for the two placeholders. No new port, no new interface, no new permission.
  - **Reason:** The owner-approved V2 option avoids a new port + interface + permission just to read two already-exposed settings; M-12/M14/M22 precedent is to consume the read via the MediatR pipeline or direct static call.
  - **Scope of impact:** `GetRegistrationCatalogQueryHandler` (the single query the future registration screen calls on open) — exposes `AccountType DefaultAccountType`, `bool DisableAutoTitleInsertion`, `string ReferralPlaceholderMale`, `string ReferralPlaceholderFemale`.
  - **Follow-up required:** No (encoded in `GetRegistrationCatalogQueryHandler`; `RegistrationCatalogDto` exposes the four values).

- **Decision (settled OD-5):** `CreatePatientCommand` and `UpdatePatientCommand` both carry the full `IReadOnlyList<PatientNumberInput>` and use `Patient.SetPhoneNumbers(...)` (replace-list, trim, skip-empty). The handler does not invent validation beyond the `string.IsNullOrWhiteSpace` guard already in `PatientPhoneNumber.Create`.
  - **Reason:** The owner-approved V2 option preserves the FR-M02-003 invariant that all stored numbers are non-empty after trim by enforcing it once in the Domain mutator.
  - **Scope of impact:** `Patient.SetPhoneNumbers` mutator (M-02-S1); `CreatePatientCommand` and `UpdatePatientCommand` shapes (M-02-S3).
  - **Follow-up required:** No (the validator's `RuleForEach(PhoneNumbers).ChildRules(phone => RuleFor(p => p.Number).NotEmpty())` rejects empty entries at the FluentValidation pipeline level too).

- **Decision (settled OD-6):** M-02 carries the five sample-type flags and `IsTakenOutsideLab` in `AddTestsToVisitCommand` (per test, via `AddTestInput`) and on `UpdatePatientTestSampleFlagsCommand`. `PatientTest.UpdateSampleFlags(...)` is a pure state-setter.
  - **Reason:** The owner-approved V2 option is faithful to the reference system's "خانة التحليل المسحوب خارج المعمل أسفل قائمة التحاليل" wording — the flag is per test, not per visit.
  - **Scope of impact:** `PatientTest.UpdateSampleFlags` mutator (M-02-S1); `AddTestsToVisitCommand` and `UpdatePatientTestSampleFlagsCommand` shapes (M-02-S3); M-21's `MarkSampleDrawnCommand` (in M-21's plan) rejects redrawing an outside-drawn test with the message `"تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب"`.
  - **Follow-up required:** No (encoded in `PatientTest.UpdateSampleFlags` and the `AddTestInput` record).

- **Decision (settled OD-7 — reframed as M-21-owned):** The patient-list-with-undrawn-tests query is owned by M-21 (`GetPatientsWithUncollectedSamplesQuery` + `GetPatientTestsForDrawQuery`), not by M-02. M-02's `SearchPatientsQuery` is name/phone/national-id/LabId search only.
  - **Reason:** The owner-approved V2 option keeps M-02's search focused on registration-time lookup, and lets M-21 own the sample-collection-time list shape.
  - **Scope of impact:** None on M-02 (the M-02-S1 `(PatientId, IsSampleDrawn)` composite index is the only schema dependency M-21 has on M-02).
  - **Follow-up required:** M-21 owner uses the M-02-S1 index.

- **Decision (settled OD-8):** M-02 reuses the existing `ADD_EDIT_PATIENT` (id 1) and `DELETE_PATIENT` (id 9) codes. No new permission row is added.
  - **Reason:** The owner-approved V2 option validates the original M-17 seed design — every operational permission a Top-Lab module needs is already in the 13-row catalog.
  - **Scope of impact:** All 10 M-02 write commands + the `PatientRegistrationAccessPolicy` constants (one source of truth for the magic strings).
  - **Follow-up required:** No (encoded in `PatientRegistrationAccessPolicy.cs`; the `git diff` against `PermissionConfiguration.cs` is empty).

- **Decision (settled OD-9):** EF Core identity for `Patient.PatientId` (the `int` identity column); `LabId` is user-supplied and applied via the existing `Patient.AssignLabId(...)` mutator if present. No new port, no `IPatientIdGenerator`.
  - **Reason:** The owner-approved V2 option avoids a new port + interface for an identity column EF Core already handles.
  - **Scope of impact:** `CreatePatientCommandHandler` calls `Patient.Create(PatientId.Create(0), ...)`; EF Core assigns the value on save; the response DTO carries the saved `PatientId`.
  - **Follow-up required:** No (encoded in `CreatePatientCommandHandler`).

- **Decision (reframed as precondition — M-13 dependency):** M-13 is fully implemented in the codebase before M-02-S3 starts. M-02's `AddTestsToVisit` and `AddCustomGroupToVisit` are written directly against M-13's final surface as specified in §3.1 of M-02 plan. There is no stub-then-gate split.
  - **Reason:** The owner reframed OD-2 as a real-world code-deployment precondition rather than a reading dependency on any other plan file; the inlined §3.1 contracts are the complete specification M-02 needs.
  - **Scope of impact:** `AddTestsToVisitCommandHandler` and `AddCustomGroupToVisitCommandHandler` consume `GetPriceListByIdQuery` and `GetCustomGroupByIdQuery` via `ISender`; both delegate the per-test price to `TestPriceResolver` (`internal` pure); the contract-with-price-list-missing-test rule (`Error.Conflict("التحليل غير موجود في قائمة أسعار الجهة المحال منها.")`) is enforced at the handler level.
  - **Follow-up required:** No (encoded in the two handlers and the resolver).

- **Decision (reframed as precondition — execution on local Windows dev machine):** The plan is being executed on the local Windows development machine that has the full required toolchain (.NET 8 SDK, EF tools, build tools). Build/test gates are carried out on that machine, not designed around in a planning environment.
  - **Reason:** The owner reframed OD-3 as an execution-environment precondition rather than an open decision.
  - **Scope of impact:** None on the codebase — only on where the gates are run.
  - **Follow-up required:** No.

- **Decision (settled during S3):** The M-02 → M-13 MediatR integration test seam is a hand-rolled `ISender` shim (`FakeSender`) — one file, one class, no Application code change. The shim implements the full `ISender` interface (including the generic `Send<TRequest>(TRequest)` for `IRequest`-typed requests used by `GetSystemSettingsQuery`) via an `_otherResponses` dictionary.
  - **Reason:** A clean test seam records a precedent for any future Application module that needs to test intra-Application MediatR; no need for the M-13 DbContext to be exercised by M-02 handler tests.
  - **Scope of impact:** `tests/TopLab.Application.Tests/Common/Fakes/FakeSender.cs` only.
  - **Follow-up required:** No (encoded in the file).

- **Decision (settled during S3):** The shared pure `TestPriceResolver` lives in `src/TopLab.Application/Features/PatientRegistration/Common/TestPriceResolver.cs` as an `internal static` class. Both `AddTestsToVisit` and `AddCustomGroupToVisit` delegate to it. The resolver has no DB, no clock, no MediatR; pure function.
  - **Reason:** A single source of truth for the 4-step pricing algorithm eliminates the duplicate-implementation hazard; the `internal` modifier keeps the API surface small while remaining unit-testable in isolation (8 `TestPriceResolverTests` cases pin every branch).
  - **Scope of impact:** `AddTestsToVisitCommandHandler` and `AddCustomGroupToVisitCommandHandler` both call `TestPriceResolver.Resolve(...)`.
  - **Follow-up required:** No (encoded in the resolver + tests).

- **Decision (settled during S4):** `PatientTest` cascade from `Patient` is **Cascade**, not Restrict — the plan narrative §5.4 says Restrict but the actual `PatientTestConfiguration` says Cascade. The Infrastructure-layer test (`PatientDeleteBehaviorTests.DeletingPatient_CascadesToPatientTest`) pins the actual behavior.
  - **Reason:** Resolving the discrepancy between the plan narrative and the actual configuration in favor of the code (Cascade); the existing F5 baseline migration already established Cascade.
  - **Scope of impact:** `PatientTestConfiguration.cs` (unchanged from baseline); the new `PatientDeleteBehaviorTests.DeletingPatient_CascadesToPatientTest` test pins the Cascade behavior.
  - **Follow-up required:** No (the test pins the behavior; the plan narrative correction is recorded here).

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** The M-02 plan narrative §5.4 says `PatientTest → Patient` delete behavior is `Restrict`. The actual `PatientTestConfiguration.cs` (verified at the M-02 plan's reference commit `e7ca616`) configures `OnDelete(DeleteBehavior.Cascade)`. The plan finality statement said "verify from baseline FK matrix" — the baseline matrix says Cascade.
  - **Waiver:** No waiver required — the implementation follows the actual baseline, not the plan narrative.
  - **Pinned by:** `PatientDeleteBehaviorTests.DeletingPatient_CascadesToPatientTest` (new in S4).
- **Deviation:** The plan §5.1 mentions "`MarkSampleDrawn(DateTime)` rejects if the patient-test is soft-deleted (mirror of the Patient guard)" — but `PatientTest` itself has no `IsDeleted` flag (only `Patient` does, per the migration scope). The Domain-level `MarkSampleDrawn` therefore remains unchanged; the soft-delete check is the M21 Application handler's responsibility (`Patient.IsDeleted` lookup before calling `MarkSampleDrawn`).
  - **Waiver:** No waiver required — the natural enforcement is at the Application layer (M21), which is out of scope for this plan.
  - **Pinned by:** The M-22 `PatientTest.UpdateSampleFlags` mutator is the M-02-S1 Domain surface; M21 will consult `Patient.IsDeleted` upstream.
- **Waiver:** None.

---

## 8. Required Reading (Required — mark "None" if none)

- **M-02 Implementation Plan** — `Docs/OpenCode/M-02.md` (this plan's source of truth).
- **M-13 Handoff** — `Docs/Handoff_M13.md` (for the integrated M-13 surface; see §3.1 of M-02 plan for the inlined contracts that M-02 consumes; the executing agent does not need to open any other plan file — the inlined content is the complete specification).
- **M-14 Handoff** — `Docs/Handoff_M14.md` (for `ExternalEntity` resolution — referral entity names in `GetPatientById` and `ReferralNameResolver` usage in `GetRegistrationCatalog`).
- **M-22 Handoff** — `Docs/Handoff_M22.md` (for `SystemSettings` — `DefaultAccountType` and `DisableAutoTitleInsertion` consumed by `GetRegistrationCatalog`).
- **M-12 Handoff** — `Docs/Handoff_M12.md` (for the test catalog + test groups used by `GetRegistrationCatalog`).
- **ADR-0032** — `Docs/Source/Top_Lab_ADR.md` (M-02 permission re-use + `TestPriceResolver` placement + `FakeSender` test seam + per-test sample-flag rule + contract-with-price-list-missing-test rule).

---

*End of document.*