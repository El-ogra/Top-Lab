
# Top-Lab — Master Tracking Sheet

## نظام توب لاب — لوحة متابعة المشروع الرئيسية

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Master Tracking Sheet |
| Status | **Active** — updatable project tracking board |
| Purpose | Track the implementation and audit progress of every foundational item and every functional module of Top-Lab in one place. |

---

## 1. Status Legend

| Symbol | Status | Meaning |
|---|---|---|
| ⬜ | Not Started | No implementation work has begun. |
| 🟨 | In Progress | Implementation has begun; not yet ready for review. |
| 🟦 | Implementation Complete | Implementation finished; ready for code review. |
| 🟧 | In Review | Code review under way. |
| 🟪 | In Audit | Functional/acceptance audit under way. |
| 🟩 | Done | Implementation complete, review passed, audit passed, merged. |
| ⬛ | Blocked | Progress halted by a documented blocker. |
| 🟥 | Rework | Failed review or audit; sent back for corrections. |

---

## 2. Phase Legend

| Phase | Meaning |
|---|---|
| Design | Detailed design deliverables for the item are being produced. |
| Implement | Source code for the item is being written and unit-tested. |
| Review | Code review of the item is under way. |
| Audit | Functional/acceptance audit of the item is under way. |
| Closed | The item is Done and no further work is planned in the current cycle. |

---

## 3. Foundational Track

| ID | Item | Wave | Status | Phase | Assignee | Started | Completed | Blockers / Notes |
|---|---|---|---|---|---|---|---|---|
| F1 | Solution & project skeleton | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Committed to `main` (194220f); builds clean (0 errors/0 warnings). |
| F2 | Domain common types (`Entity`, `AuditableEntity`, `ValueObject`, `DomainException`, strong IDs) | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented in `TopLab.Domain/Common` (Entity, ValueObject, AuditableEntity, DomainException, StronglyTypedId); matches Architecture §4.1 + ADR-0012/0013. Addendum 2026-08-30: added `LabId : StronglyTypedId<string>` and migrated `Patient.LabId` from `string?` to `LabId?` with an EF value converter back to `nvarchar(30)` (ADR-0012 compliance, commit `a035904`). |
| F3 | Result pattern & MediatR pipeline behaviors (Validation, Authorization, Logging) | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented in `TopLab.Application/Common/{Results,Interfaces,Behaviors,Authorization}` + `DependencyInjection.cs`; 3 pipeline behaviors wrap every request. Domain tests 12, Application tests 14, all green. |
| F4 | Persistence baseline (`ApplicationDbContext`, `AuditableEntitySaveChangesInterceptor`, `IDateTimeProvider`, `ICurrentUserService`) | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented `IApplicationDbContext` port in Application; `ApplicationDbContext` in Infrastructure with Fluent-API discovery; `AuditableEntitySaveChangesInterceptor` populates Created/Modified audit columns and increments `ModificationCount`; `SystemDateTimeProvider` and `CurrentUserService` (scoped, in-memory session) in Infrastructure; `AddInfrastructure` wires DbContext, interceptor, identity and time providers; 11 Infrastructure tests + 14 Application + 12 Domain = 37 tests green; build 0/0. Addendum 2026-08-30: composition root now runs `ApplicationDbContext.Database.MigrateAsync()` guarded by try/catch after `Host.Start()` (commit `a035904`). |
| F5 | Data model — baseline entity schemas across all entity groups | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented 36-table baseline: Domain entities across 9 groups + Fluent-API configurations + `BaselineDataModel` migration (commit `94b5213` / merge `61cad11`); build 0/0, 73 tests green; conventions per Data Model §2–§12, Architecture §4.1/§8. Addendum 2026-08-30: verified 36 business tables + `__EFMigrationsHistory` (37 total) reachable through the wizard-written connection; `LabId` column unchanged at `nvarchar(30)`. |
| F6 | Presentation composition root (`App.xaml.cs`), main-window shell, navigation and dialog services, `ResultErrorPresenter` | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented `App.xaml.cs` composition root wiring Application+Infrastructure+Presentation, `MainWindow` shell, `NavigationService`, `DialogService`, `ResultErrorPresenter` (commit `896db5b` "بعد تنفيذ F6"); build 0/0. Addendum 2026-08-30: added Arabic navigation titles, first-run `DatabaseSetupWindow` wizard, and ProgramData configuration fallback (ADR-0025); `Content` copy of `appsettings.json` is now conditional on the file existing (commit `a035904`). |

---

## 4. Modules — Master Board

| ID | Module | Wave | Status | Phase | Assignee | Depends on | Started | Completed | Blockers / Notes |
|---|---|---|---|---|---|---|---|---|---|
| M17 | User & Permission Management | 1 | 🟩 | Done | Local coding agent (Top-Lab) | Foundations | 2026-09-01 | 2026-09-01 | Delivered: PBKDF2-SHA256 hashing, sign-in/sign-out, secondary-password gate, user management CRUD, first-run wizard, floor & guarded delete; 162 tests green. |
| M22 | System & Print Settings | 1 | 🟩 | Done | Local coding agent (Top-Lab) | Foundations | 2026-09-02 | 2026-09-02 | Delivered: six settings aggregates with mutators/invariant guards, read/write Application surface, Infrastructure maintenance + workstation-local lab-text store + daily-backup hook, Settings dashboard (S-27), System (S-28), Report (S-29), Receipt (S-30), Envelope (S-31) and secondary-password-gated Database Maintenance (S-32) screens; 268 tests green. |
| M14 | External Entities | 2 | ⬜ | Design |  | M17, M22 |  |  |  |
| M12 | Test Catalog & Reference Ranges | 2 | 🟩 | Done | Local coding agent (Top-Lab) | M17, M22 | 2026-09-06 | 2026-09-06 | Delivered: Test/TestGroup/ReferenceRange lifecycle (deactivate/reactivate, atomic TestGroup cascade), WorkGroupLog item atomic save, unique TestCode search column, EDIT_SYSTEM_SETTINGS gate; 487 tests green; ADR-0028/0029. |
| M13 | Price Lists, Comments & Custom Groups | 3 | 🟩 | Done | Local coding agent (Top-Lab) | M12, M14 | 2026-09-07 | 2026-09-07 | Delivered: PriceList/CustomGroup rename + item upsert/remove + price>=0 guards; TestComment create/update/delete + 1000-char guard; DeletePriceList blocked by ExternalEntity.PriceListId references; 13 IAuthorizedRequest commands (5 PL + 3 TC + 5 CG) with EDIT_SYSTEM_SETTINGS gate and frozen Arabic messages; 5 read queries + DTOs; feature-local DomainFailureTranslator (paramName+fragment, M14 style); aggregate-as-invariant-checker + flat-set persistence protocol (no double-tracking, pinned by EF InMemory regression test); zero drift against F5 baseline (no migration); 782 tests green; ADR-0030. |
| M15 | Culture & Antibiotic Configuration | 3 | 🟩 | Done | Local coding agent (Top-Lab) | M12 | 2026-09-07 | 2026-09-07 | Delivered: `Antibiotic.Update(name, isPregnancyFlagged, isChildrenFlagged)` (name guard + freely editable flags — confirmed D4-a); pure static `CultureAntibioticDisplay` resolver in Application-Common (M14 `ReferralNameResolver` placement precedent) with `ChildAgeThresholdYears = 12` constant and union-semantics BR-12 filter (confirmed D5-a, 16-row truth-table pinned); 2 read queries + 5 IAuthorizedRequest commands (Create/Update/Delete Antibiotic + Attach/Detach Antibiotic) with `EDIT_SYSTEM_SETTINGS` gate; 5 frozen Arabic messages incl. `التحليل المحدد ليس مزرعة.` and `تعذر حذف المضاد الحيوي لارتباطه بمزرعة.`; manual-entry flow is two-command sequence `CreateAntibiotic` then `AttachAntibioticToCulture` (confirmed D6-a, no composite command); feature-local `DomainFailureTranslator` (paramName+fragment, M14 style); antibiotic delete blocked by `CultureAntibioticAttachment` rows (application-level, no DB FK exists) + `CultureAntibioticResult` rows (DB-level Restrict, forward-safe) + save-time `IsReferenceConflict` catch; FK-matrix tests pin 2 negative no-FK assertions on `CultureAntibioticAttachment`; zero drift against F5 baseline (no migration); 863 tests green; build 0/0; ADR-0031. |
| M01 | Application Access & Main Navigation | 3 | ⬜ | Design |  | M17 |  |  |  |
| M02 | Patient Registration & Test Ordering | 4 | 🟩 | Done | Local coding agent (Top-Lab) | M01, M12, M13, M14, M22 | 2026-09-07 | 2026-09-07 | Delivered: Domain mutators (`Patient.SetPhoneNumbers` replace-list, `AddMedicalCondition`/`RemoveMedicalCondition`, `IsDeleted`/`SoftDelete`/`Restore`, `EnsureNotDeleted` guards on `Update`/`AssignLabId`; `PatientTest.UpdateSampleFlags` 6 booleans per-test); new migration `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs` adds `IsDeleted` bit not null + `(PatientId, IsSampleDrawn)` composite index (only new M-02 migration); 6 read queries (`SearchPatients` BR-03 any-phone-number match, `GetPatientById` w/ soft-delete NotFound, `GetPatientVisitHistory` grouped by visit, `GetPatientTitles`, `GetMedicalConditionTypes`, `GetRegistrationCatalog` aggregating M12/M22/M14 — OD-4 wiring); 10 write commands (`CreatePatient`, `UpdatePatient`, `SoftDeletePatient`, `AddMedicalCondition`/`RemoveMedicalCondition`, `AddTestsToVisit`, `AddCustomGroupToVisit` both fully integrated against M-13's final surface via intra-Application MediatR, `RemoveTestFromVisit`, `UpdatePatientTestSampleFlags`, `ClearAllTests`) all `IAuthorizedRequest` with `ADD_EDIT_PATIENT` except `SoftDeletePatient` with `DELETE_PATIENT` (settled OD-8 — no new seed row, ADR-0032); shared `internal` pure `TestPriceResolver` (single source of truth for 4-step pricing, 8 unit tests pin every branch); `FakeSender` test seam for M-02 → M-13 MediatR testing; FK-matrix incl. 2 negative no-FK assertions (`PatientDeleteBehaviorTests`); `AddTestsToVisitPersistenceTests` EF Core InMemory integration test pins M-13-precondition assumption; zero drift against snapshot (`dotnet ef migrations has-pending-model-changes` = no changes); ADR-0032; 996 tests green; Release build 0/0. |
| M21 | Sample Collection & Separation | 4 | 🟩 | Done | Local coding agent (Top-Lab) | M02 | 2026-09-08 | 2026-09-08 | Delivered: Application-only surface in `SampleCollection` feature folder (no Domain/Infrastructure changes, no migration); 2 unauthorized queries (`GetPatientsWithUncollectedSamples` — `DateOnly? Day` defaulting to today UTC, `RegistrationDateUtc asc`, excludes soft-deleted patients / drawn tests / outside-drawn tests, page size ≤ 500; `GetPatientTestsForDraw` — Drawn/NotDrawn board with M12 `Test.Name` resolution, unknown patient → empty board echoing `PatientId`); 2 `IAuthorizedRequest` commands (`MarkSampleDrawn` — `NotFound`/`Conflict` guards incl. the FR-M21-001 outside-drawn message `"تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب"`, idempotent double-draw, time via `IDateTimeProvider`; `MarkAllSamplesDrawnForPatient` — marks only `!IsSampleDrawn && !IsTakenOutsideLab` rows, single save, returns count) both gated by `ADD_EDIT_PATIENT` via the mirrored `SampleCollectionAccessPolicy.AddEditPatient` constant (settled OD-8 — no new seed row, ADR-0033); 5 validators auto-registered (no DI change); no fake changes (`Remove<PatientTest>` already present); 20 new tests pin every gate incl. the FR-M21-001 invariant and the exactly-once save; 1016 tests green (270 + 667 + 79); build 0/0. |
| M03 | Patient Billing & Account Settlement | 5 | 🟩 | Done | Local coding agent (Top-Lab) | M02, M13, M17 | 2026-09-08 | 2026-09-08 | Delivered: Domain guards on `PaymentOperation.Create` (amount ≥ 0, discount 0..amount, no discount on extra charge, settlement > 0) + `IsEffectivelyZero` getter-only + pure static `PatientAccountCalculator` (settled formula, shared worked example Charged 170/Paid 90/Balance 80); read surface (3 ungated queries: `GetPatientAccount` with voided-included history + deleted-user raw-id fallback, `GetPatientReceipt` with `ReceiptSettings.Currency`, paged `ListPatientPayments` newest-first); write surface (ungated `RecordPayment` with `User.DiscountLimitPercent` cap → `Validation` breach + absolute-user exemption, ungated `RecordExtraCharge`, gated `RecordCorrection` + `VoidPaymentOperation` on `CASH_DISBURSE_DEPOSIT` id 11, ungated `SettleAccountInFull` with balance ≤ 0 → `Conflict`); feature-local `DomainFailureTranslator` to frozen Arabic messages; no edit command (void-and-reissue only, S-04 reconciliation note in ADR-0034); zero drift (no migration); no `PermissionConfiguration` change; full suite green; ADR-0034. |
| M04 | Results Entry & Result Lifecycle | 5 | 🟩 | Done | Local coding agent (Top-Lab) | M02, M03, M12, M22 | 2026-09-08 | 2026-09-08 | Delivered: Domain lifecycle guards on `PatientTest` (reviewed-lock, locked-clear, unreview/print/deliver chain + `MarkEntered`) + `PatientTestReferenceRangeSnapshot` 1:1 child (only migration `AddPatientTestReferenceRangeSnapshots`) + full `PatientStatusCalculator` S1–S7 with UTC-day micro-pin and worked example {1,3,4}→S2; read surface (worklist with day/has-result/reviewed/group/kind filters + aggregate status, entry detail with live ranges + frozen range, patient sheet) + `ResultFlagComputer` most-specific selection + `BalanceProbe` delegating to `PatientAccountCalculator` (D5); write surface (D1 atomic `CreatePatient` with ordered tests + single save, D4 whitespace guard with exact message, clear/refresh/review/unreview/print with per-user balance block + deliver with no balance gate, D2 PDF export via port with Conflict on existing/Validation on path/Unexpected on I/O and export-only marks, D3 two-step bulk print with RequiresReprintConfirmation + Confirm/Cancel and mandated Arabic question); feature-local `DomainFailureTranslator`; 14 validators auto-registered; InMemory lifecycle + cascade proof; zero drift; no `PermissionConfiguration` change; full suite green; ADR-0035. |
| M05 | Specialized Profile Result Reports | 6 | ⬜ | Design |  | M04, M12 |  |  |  |
| M06 | Culture & Sensitivity Result Entry | 6 | ⬜ | Design |  | M04, M15 |  |  |  |
| M08 | Patient Search, Lab ID & Visit History | 6 | ⬜ | Design |  | M02, M04 |  |  |  |
| M11 | Work Sheets | 6 | ⬜ | Design |  | M02, M12 |  |  |  |
| M07 | Combined, Blank & History Reports | 7 | ⬜ | Design |  | M04, M05, M06, M22 |  |  |  |
| M09 | Result Delivery & Settlement at Handover | 7 | ⬜ | Design |  | M04, M03, M17 |  |  |  |
| M16 | Sent-Out Samples | 7 | ⬜ | Design |  | M02, M12, M14, M03 |  |  |  |
| M10 | Case Tracking, Audit & Traceability (P/T) | 8 | ⬜ | Design |  | M02, M03, M04, M09, M17 |  |  |  |
| M18 | Attendance & Time Tracking | 8 | ⬜ | Design |  | M17 |  |  |  |
| M19 | Statistics | 8 | ⬜ | Design |  | M02, M04, M14, M16, M17, M18 |  |  |  |
| M20 | Inventory & Lab Accounting | 9 | ⬜ | Design |  | M02, M03, M04, M09, M16, M14, M17, M22 |  |  |  |
| M23 | Utilities (Tools) | 10 | ⬜ | Design |  | M01 |  |  |  |

---

## 5. Wave-Level Summary

| Wave | Modules | Status Summary |
|---|---|---|
| Wave 0 — Foundations | F1, F2, F3, F4, F5, F6 | 🟩 Done |
| Wave 1 — Configuration Backbone | M17, M22 | 🟩 Done |
| Wave 2 — Reference Data | M14, M12 | ⬜ Not Started — M12 🟩 Done, M14 pending |
| Wave 3 — Reference-Data Extensions | M13, M15, M01 | ⬜ Not Started |
| Wave 4 — Patient Lifecycle Entry | M02, M21 | 🟩 Done |
| Wave 5 — Patient Money & Results | M03, M04 | ⬜ Not Started |
| Wave 6 — Result Specializations & Search | M05, M06, M08, M11 | ⬜ Not Started |
| Wave 7 — Report Production & Handover | M07, M09, M16 | ⬜ Not Started |
| Wave 8 — Audit, Attendance & Statistics | M10, M18, M19 | ⬜ Not Started |
| Wave 9 — Financial Consolidation | M20 | ⬜ Not Started |
| Wave 10 — Utilities | M23 | ⬜ Not Started |

Fill the "Status Summary" cell with the earliest non-Done status of any item in the wave, or 🟩 when every item in the wave is Done.

---

## 6. Per-Module Detailed Tracking Blocks

Each module has its own tracking block. Blocks are updated as work progresses. Use the same status and phase legends as §1 and §2.

### 6.1 Template (copy for each module row)

```
Module: Mxx — <Name>
Wave: <n>
Owner: <name>
Dependencies satisfied? (Yes / No): 
Implementation status: ⬜ / 🟨 / 🟦 / 🟧 / 🟪 / 🟩 / ⬛ / 🟥
Current phase: Design / Implement / Review / Audit / Closed
Start date: 
Target completion date: 
Actual completion date: 
Domain work — status: 
Application (Commands/Queries) work — status: 
Infrastructure work (EF configurations, migrations, services) — status: 
Presentation work (Views/ViewModels) — status: 
Unit tests present? (Yes / No):
Manual/ViewModel-level tests present where applicable? (Yes / No):
Audit outcome: Pass / Fail / Pending
Audit notes: 
Open blockers: 
Handoff document produced? (Yes / No):
```

### 6.2 Concrete blocks

The following blocks are pre-created; contents mirror the master board in §4 and are updated in place as work progresses.

---

**Module: M17 — User & Permission Management**
- Wave: 1
- Owner: Local coding agent (Top-Lab)
- Dependencies satisfied? Foundations only — satisfied.
- Implementation status: 🟩
- Current phase: Done
- Notes: prerequisite for every downstream authorization decision; delivers `User`, `Permission`, `UserPermissionGrant`, absolute/limited modes, internal windows password, discount limit %, print-block-on-balance flag, working-hours and break configuration. Delivered surface: PBKDF2-SHA256 password hashing (self-describing format, no schema change), sign-in/sign-out, secondary-password gate, user management screen, first-run administrator wizard, last-active-absolute-user floor, guarded delete. Build 0/0, 162 tests green (71 Domain + 60 Application + 31 Infrastructure).
- Started: 2026-09-01
- Completed: 2026-09-01

**Module: M22 — System & Print Settings**
- Wave: 1
- Owner: Local coding agent (Top-Lab)
- Dependencies satisfied? Foundations only.
- Implementation status: 🟩
- Current phase: Done
- Completed: 2026-09-02
- Notes: delivers `SystemSettings`, `ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`, `EnvelopePrintItemPosition`, `PrinterAssignment`; daily backup, Database Maintenance, and system initialization functions.

**Module: M14 — External Entities**
- Wave: 2
- Owner:
- Dependencies satisfied? Requires M17, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: unified `ExternalEntity` table with type discrimination (TreatingDoctor / ReferralOrContract / PartnerLab).

**Module: M12 — Test Catalog & Reference Ranges**
- Wave: 2
- Owner: Local coding agent (Top-Lab)
- Dependencies satisfied? Requires M17, M22 — satisfied.
- Implementation status: 🟩
- Current phase: Done
- Completed: 2026-09-06
- Notes: delivers `TestGroup`, `Test`, `ReferenceRange`, `TestComment`, `PatientTitle`, and the age-unit-sensitive matching rule. M-12 adds `Tests.TestCode` (nvarchar(50), unique `IX_Tests_TestCode`) and lifecycle `IsActive` columns (default 1) on `Tests`/`TestGroups` (migration `20260906093902_AddTestCodeAndLifecycleColumns`), plus the soft-lifecycle write surface (no hard delete of Test/TestGroup). ADR-0028/0029. Documentation: `Docs/Handoff_M12.md`.

**Module: M13 — Price Lists, Comments & Custom Groups**
- Wave: 3
- Owner:
- Dependencies satisfied? Requires M12, M14.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `PriceList`/`PriceListItem`, `CustomGroup`/`CustomGroupItem`, `TestComment` operations.

**Module: M15 — Culture & Antibiotic Configuration**
- Wave: 3
- Owner:
- Dependencies satisfied? Requires M12.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `Antibiotic`, `CultureAntibioticAttachment`, and the user-extensible culture-type surface.

**Module: M01 — Application Access & Main Navigation**
- Wave: 3
- Owner:
- Dependencies satisfied? Requires M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: login, navigation bar, status bar, permission-denial message.

**Module: M02 — Patient Registration & Test Ordering**
- Wave: 4
- Owner:
- Dependencies satisfied? Requires M01, M12, M13, M14, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `Patient`, `PatientPhoneNumber`, `MedicalConditionType`, `PatientMedicalCondition`; captures multiple phone numbers per patient.

**Module: M21 — Sample Collection & Separation**
- Wave: 4
- Owner:
- Dependencies satisfied? Requires M02.
- Implementation status: ⬜
- Current phase: Design
- Notes: sample-drawn / separated marking on `PatientTest`.

**Module: M03 — Patient Billing & Account Settlement**
- Wave: 5
- Owner:
- Dependencies satisfied? Requires M02, M13, M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `PaymentOperation`, void-and-reissue correction, discount-limit enforcement.

**Module: M04 — Results Entry & Result Lifecycle**
- Wave: 5
- Owner:
- Dependencies satisfied? Requires M02, M03, M12, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers the `PatientTest` lifecycle columns and the aggregate-status calculator invocation.

**Module: M05 — Specialized Profile Result Reports**
- Wave: 6
- Owner:
- Dependencies satisfied? Requires M04, M12.
- Implementation status: ⬜
- Current phase: Design
- Notes: `ProfileResultItem`.

**Module: M06 — Culture & Sensitivity Result Entry**
- Wave: 6
- Owner:
- Dependencies satisfied? Requires M04, M15.
- Implementation status: ⬜
- Current phase: Design
- Notes: `CultureResult`, `CultureAntibioticResult`.

**Module: M08 — Patient Search, Lab ID & Visit History**
- Wave: 6
- Owner:
- Dependencies satisfied? Requires M02, M04.
- Implementation status: ⬜
- Current phase: Design
- Notes: multi-criteria search (including any stored telephone number); Lab ID creation and visit list.

**Module: M11 — Work Sheets**
- Wave: 6
- Owner:
- Dependencies satisfied? Requires M02, M12.
- Implementation status: ⬜
- Current phase: Design
- Notes: patient-based, test-based, Log-group work sheets; test-frequency classification.

**Module: M07 — Combined, Blank & History Reports**
- Wave: 7
- Owner:
- Dependencies satisfied? Requires M04, M05, M06, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: combined, blank, auto/manual/separate/multi-patient history.

**Module: M09 — Result Delivery & Settlement at Handover**
- Wave: 7
- Owner:
- Dependencies satisfied? Requires M04, M03, M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: undelivered-results list; account view at delivery; print-block interaction.

**Module: M16 — Sent-Out Samples**
- Wave: 7
- Owner:
- Dependencies satisfied? Requires M02, M12, M14, M03.
- Implementation status: ⬜
- Current phase: Design
- Notes: `SentOutSample`, `SentOutSamplePayment`; per-lab follow-up and settlement.

**Module: M10 — Case Tracking, Audit & Traceability (P/T)**
- Wave: 8
- Owner:
- Dependencies satisfied? Requires M02, M03, M04, M09, M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: restricted `P` and `T` inspection queries; access via authorization pipeline.

**Module: M18 — Attendance & Time Tracking**
- Wave: 8
- Owner:
- Dependencies satisfied? Requires M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: `AttendanceRecord`; overtime/lateness visibility restricted to the system manager.

**Module: M19 — Statistics**
- Wave: 8
- Owner:
- Dependencies satisfied? Requires M02, M04, M14, M16, M17, M18.
- Implementation status: ⬜
- Current phase: Design
- Notes: read-only projections; no dedicated Domain entities.

**Module: M20 — Inventory & Lab Accounting**
- Wave: 9
- Owner:
- Dependencies satisfied? Requires M02, M03, M04, M09, M16, M14, M17, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: cash-drawer inventory, per-element inventories, cash disbursement/deposit, companies & delegates account tracking.

**Module: M23 — Utilities (Tools)**
- Wave: 10
- Owner:
- Dependencies satisfied? Requires M01.
- Implementation status: ⬜
- Current phase: Design
- Notes: Test Library, Image Library, Shortcut Library, Unit Converter, Calculator, Stopwatch, Purchases List, Phone Book — self-contained.

---

## 7. Cross-Cutting Concern Tracking

| Concern | Owning artifact | Status | Phase | Notes |
|---|---|---|---|---|
| Result pattern | `Result`, `Result<T>`, `Error`, `ErrorType` | 🟩 | Closed | Delivered by F3. |
| Validation | `ValidationBehavior` + per-Command validators | ⬜ | Design | Delivered by F3; validators live inside each feature folder. |
| Authorization | `AuthorizationBehavior` + declared permissions on Commands/Queries | 🟩 | Closed | Delivered by F3; permission catalog of thirteen codes seeded by M17 and consumed by the authorization pipeline; no pending catalog. |
| Logging | `LoggingBehavior` | ⬜ | Design | Delivered by F3. |
| Audit columns | `AuditableEntity` + `AuditableEntitySaveChangesInterceptor` | ⬜ | Design | Delivered by F2 + F4. |
| Patient aggregate status | `PatientStatusCalculator` (Domain service) | ⬜ | Design | Delivered inside M04. |
| Time provider | `IDateTimeProvider` | ⬜ | Design | Delivered by F4. |
| Current-user context | `ICurrentUserService` | ⬜ | Design | Delivered by F4. |

---

## 8. Change Log Rules

- Each row is updated in place; historical values are not deleted from the sheet — instead, the change-log below records the update.
- Every non-trivial status transition (Not Started → In Progress, In Progress → Implementation Complete, and every audit outcome) is appended to §9 with the date and the person who made the change.
- Blockers are recorded in the row's "Blockers / Notes" cell and mirrored in §9.

---

## 9. Change Log

| Date | Item | Change | By |
|---|---|---|---|
| 2026-08-28 | F1 | Solution & project skeleton created; builds clean (0 errors/0 warnings); committed to `main` (194220f). | Local coding agent (Top-Lab) |
| 2026-08-28 | F3 | Result pattern + MediatR pipeline behaviors (Validation/Authorization/Logging) implemented in `TopLab.Application`; Application ports added; 14 Application tests + 12 Domain tests green. | Local coding agent (Top-Lab) |
| 2026-08-28 | F4 | Persistence baseline implemented: `IApplicationDbContext` port + `ApplicationDbContext` + `AuditableEntitySaveChangesInterceptor` + `SystemDateTimeProvider` + `CurrentUserService` + Infrastructure DI. 11 Infrastructure tests + 14 Application + 12 Domain = 37 tests green; build 0/0. | Local coding agent (Top-Lab) |
| 2026-08-28 | F5 | Baseline entity schemas across all entity groups implemented (36 tables, Fluent-API configurations, `BaselineDataModel` migration `20260828052248`). Commits `94b5213` / `61cad11`; build 0/0, tests 73 green; verified against Data Model §4–§12 and Architecture §4.1. | Local coding agent (Top-Lab) |
| 2026-08-28 | F6 | Presentation composition root and shell implemented (`App.xaml.cs`, `MainWindow`, navigation/dialog services, `ResultErrorPresenter`). Commit `896db5b` "بعد تنفيذ F6"; build 0/0, presentation boots via Host. | Local coding agent (Top-Lab) |
| 2026-09-02 | M22 | System & Print Settings implemented across S1–S8: domain mutators/invariant guards, read+write Application surface, Infrastructure maintenance + workstation-local lab-text store + daily-backup hook + seed-repair, Settings dashboard and System/Report/Receipt/Envelope/Database Maintenance screens, secondary-password gate. 268 tests green; build 0/0. Commit `docs(m22): finalize module 22 documentation, ADR-0027, tracking sheet, and handoff`. | Local coding agent (Top-Lab) |
| 2026-09-06 | M12 | Test Catalog & Reference Ranges implemented across S1–S5: domain lifecycle behaviors (Deactivate/Reactivate + atomic TestGroup cascade), ReferenceRange snapshot + BR-04 age/sex matching, Application read/write surface (5 queries, 14 commands, 14 validators, EDIT_SYSTEM_SETTINGS gate), Infrastructure migration `AddTestCodeAndLifecycleColumns` (Tests.TestCode nvarchar(50) unique, Tests/TestGroups.IsActive default 1) applied to LocalDB, ADR-0028/0029. 487 tests green; build 0/0; coverage Domain/Application/Infrastructure floors met. Slice commits `234f98d`, `5a641d0`, `d44bf18`, `9758466`. | Local coding agent (Top-Lab) |
| 2026-09-07 | M13 | Price Lists, Test Comments & Custom Groups implemented across S1–S4: domain behaviors (rename, item upsert/remove, price>=0, comment length<=1000), Application read surface (5 queries + DTOs + fake extension), Application write surface (13 IAuthorizedRequest commands + 13 validators + feature-local DomainFailureTranslator), Infrastructure proof (5 mapping assertions + FK matrix incl. 2 negative no-FK + EF InMemory item-persistence regression test), zero drift against F5 baseline (no migration). DeletePriceList guarded by ExternalEntity.PriceListId references; aggregate-as-invariant-checker + flat-set persistence protocol pinned by the A9 grep gate and the PriceListItemPersistenceTests regression test. 782 tests green; Release build 0/0. Slice commits `a92d238`, `1379127`, `75f5b8d`. ADR-0030. | Local coding agent (Top-Lab) |
| 2026-09-07 | M15 | Culture & Antibiotic Configuration implemented across S1–S3: domain `Antibiotic.Update` (name guard + free flag mutation — confirmed D4-a); pure static `CultureAntibioticDisplay` resolver (Application-Common, M14 placement precedent) implementing union-semantics BR-12 with `ChildAgeThresholdYears = 12` constant and 16-row truth table (confirmed D5-a); Application read+write surface (2 queries, 5 IAuthorizedRequest commands, 5 validators, feature-local `DomainFailureTranslator`, EDIT_SYSTEM_SETTINGS gate); D6-a two-command manual-entry sequence (no composite `CreateAndAttachAntibiotic`); antibiotic delete guarded by `CultureAntibioticAttachment` rows (no DB FK exists — application-level) + `CultureAntibioticResult` rows (DB-level Restrict, forward-safe) + save-time `IsReferenceConflict` catch; FK-matrix tests pin 2 negative no-FK assertions on `CultureAntibioticAttachment`; zero drift against F5 baseline (no migration). 863 tests green; build 0/0; coverage Domain/Application/Infrastructure floors met. Slice commits `0bc2016`, `d5d41fb`. ADR-0031. | Local coding agent (Top-Lab) |
| 2026-09-07 | M02 | Patient Registration & Test Ordering implemented across S1–S4: domain mutators (`Patient.SetPhoneNumbers` replace-list, `AddMedicalCondition`/`RemoveMedicalCondition`, `IsDeleted`/`SoftDelete`/`Restore`, `EnsureNotDeleted` guards; `PatientTest.UpdateSampleFlags` 6 per-test booleans); new migration `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs` (only new M-02 migration); Application read surface (6 queries incl. `GetRegistrationCatalog` aggregating M12/M22/M14 — OD-4 wiring; `SearchPatients` BR-03 any-phone-number match; `GetPatientById` w/ soft-delete NotFound); Application write surface (10 commands — `CreatePatient`/`UpdatePatient`/`SoftDeletePatient`/`AddMedicalCondition`/`RemoveMedicalCondition`/`AddTestsToVisit`/`AddCustomGroupToVisit`/`RemoveTestFromVisit`/`UpdatePatientTestSampleFlags`/`ClearAllTests` — all `IAuthorizedRequest` with `ADD_EDIT_PATIENT` except `SoftDeletePatient` with `DELETE_PATIENT`, settled OD-8 — no new seed row, ADR-0032); `AddTestsToVisit`/`AddCustomGroupToVisit` fully integrated against M-13's final surface (no stub-then-gate split); shared `internal` pure `TestPriceResolver` (single source of truth for 4-step pricing); `FakeSender` test seam for M-02 → M-13 MediatR testing; FK-matrix incl. 2 negative no-FK assertions; `AddTestsToVisitPersistenceTests` EF Core InMemory integration test pins M-13-precondition assumption at the Infrastructure layer; zero drift against snapshot. 996 tests green; Release build 0/0. | Local coding agent (Top-Lab) |
| 2026-09-08 | M21 | Sample Collection & Separation implemented across S1–S2: Application-only `SampleCollection` feature folder (no Domain/Infrastructure changes, no migration, no DI change); 2 unauthorized queries (`GetPatientsWithUncollectedSamples` with optional `DateOnly? Day` defaulting to today UTC ordered by `RegistrationDateUtc asc`; `GetPatientTestsForDraw` Drawn/NotDrawn board with M12 `Test.Name` resolution); 2 `IAuthorizedRequest` commands (`MarkSampleDrawn` with `NotFound`/`Conflict` guards incl. the FR-M21-001 outside-drawn message, idempotent double-draw; `MarkAllSamplesDrawnForPatient` marking only eligible rows with a single save) both gated by `ADD_EDIT_PATIENT` via the mirrored `SampleCollectionAccessPolicy.AddEditPatient` constant (settled OD-8 — no new seed row, ADR-0033); 5 validators auto-registered; 20 new tests pin every gate incl. the FR-M21-001 invariant and the exactly-once save. 1016 tests green (270 + 667 + 79); build 0/0. | Local coding agent (Top-Lab) |
| 2026-09-08 | M03 | Patient Billing & Account Settlement implemented across S1–S4: Domain guards + `PatientAccountCalculator` (settled formula verbatim in ADR-0034, incl. Correction-sign pin positive = credit and absolute-user cap exemption); read surface (3 ungated queries + DTOs + shared `PatientBillingReader` + `PatientBillingAccessPolicy` constant, deleted-user raw-id fallback, `ReceiptSettings` currency with Unexpected on missing row, paged list newest-first); write surface (5 commands + 5 validators + feature-local `DomainFailureTranslator`; gate matrix — only `RecordCorrection` + `VoidPaymentOperation` on `CASH_DISBURSE_DEPOSIT`, asserted by authorization theory tests; discount-cap breach → `Validation`, settlement on balance ≤ 0 → `Conflict`, double-void → `Conflict` with friendly message); Infrastructure proof (`PaymentOperation_HasExpectedMapping` + `IsEffectivelyZero_IsNotMapped` F5 assertions, InMemory void round-trip with calculator balance match, `has-pending-model-changes` = no changes → no migration); no `PermissionConfiguration` change; no Presentation content. Full suite green; Release build 0/0. ADR-0034. | Local coding agent (Top-Lab) |
| 2026-09-08 | M04 | Results Entry & Result Lifecycle implemented across S1–S4: Domain guards + 1:1 snapshot child + full seven-state calculator (S1/S2 UTC-day micro-pin, worked example {1,3,4}→S2) + only migration `AddPatientTestReferenceRangeSnapshots`; read surface (3 ungated queries + `ResultFlagComputer` most-specific selection + `BalanceProbe` D5 delegation); write surface (D1 atomic registration, D4 exact whitespace message, clear/refresh/review/unreview/print with balance block/deliver with no gate, D2 PDF export via port with Conflict/Validation/Unexpected semantics and export-only marks, D3 two-step bulk print with mandated Arabic question); 14 validators auto-registered; F5 mapping + InMemory lifecycle/cascade proof; zero drift; no `PermissionConfiguration` change; zero Presentation. Full suite green; Release build 0/0. ADR-0035. | Local coding agent (Top-Lab) |

Add one row per material change.

---

*End of document.*
