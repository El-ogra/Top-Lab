
# Top-Lab — Test Strategy & Audit Acceptance Criteria

## نظام توب لاب — استراتيجية الاختبار ومعايير قبول التدقيق

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Test Strategy & Audit Acceptance Criteria |
| Status | **Final** — binding on all implementation work |
| Purpose | Define the automated-testing strategy and the audit acceptance criteria that every module of Top-Lab must satisfy before it is considered Done. |

---

## 1. Scope

This document specifies:

- the mandatory automated-testing layout and coverage expectations per production layer;
- the concrete acceptance criteria each module must pass to be marked Done;
- the audit checklist a reviewer applies to every module;
- the pass/fail rules for each verification.

It does not specify UI-automation tooling: no UI-automation layer is mandated by this document. The Presentation layer is exercised through ViewModel-level unit tests and manual verification.

---

## 2. Testing Layout (Binding)

Test projects mirror the production layer they exercise. Exactly three test projects exist:

```
tests/
├── TopLab.Domain.Tests/            (entity invariants, domain services, value objects)
├── TopLab.Application.Tests/       (handlers, validators, pipeline behaviors — with fakes for Infrastructure interfaces)
└── TopLab.Infrastructure.Tests/    (EF Core configurations, printing/barcode services, migrations, interceptors)
```

Additional test projects are not introduced. New tests belong inside the mirroring project of the code they cover.

### 2.1 Folder layout inside a test project

- `TopLab.Domain.Tests/` mirrors the Domain grouping structure: one folder per grouping (`Patients/`, `Tests/`, `Results/`, `PatientStatus/`, …).
- `TopLab.Application.Tests/` mirrors the Application feature-folder structure: one folder per feature (`Features/PatientRegistration/`, …), each holding tests for that feature's commands, queries, and validators.
- `TopLab.Infrastructure.Tests/` mirrors the Infrastructure structure: `Persistence/`, `Printing/`, `Barcode/`, `Identity/`, `BackupAndMaintenance/`.

### 2.2 Test naming

- Test class name: `<TypeUnderTest>Tests` (for example, `PatientStatusCalculatorTests`, `RegisterPatientCommandHandlerTests`).
- Test method names use one of these two styles consistently within a class:
  - `MethodUnderTest_Condition_ExpectedResult`, or
  - `Given_<state>_When_<action>_Then_<outcome>`.
- Test data builders and fakes are named `<Entity>Builder` and `Fake<Interface>` respectively.

---

## 3. Testing Levels

### 3.1 Domain unit tests (`TopLab.Domain.Tests`)

**Purpose.** Verify entity invariants, value-object equality and immutability, domain enumerations, and stateless domain services.

**Rules.**

- No external dependency. No database, no file I/O, no network, no time source other than `DateTime.UtcNow` where already isolated behind a provider.
- Every public entity method that enforces an invariant has at least one positive test (invariant holds) and one negative test (invariant would be violated).
- Every domain service has coverage of every documented branch of its business rule.

**Mandatory domain tests.**

- `PatientStatusCalculator`: covers every stage (S1 through S7) and the aggregation rule (`min` over per-analysis stages plus account condition), including the binding worked example (stages {1, 3, 4} → S2).
- Reference-range matching (age-unit-sensitive, no cross-unit conversion): the "1 day – 60 days" range must match 15 days and 35 days, and must **not** match a patient recorded as 1 month.
- `Patient` invariants surrounding required fields, VIP flag, sample-type flags, and the presence of at least one telephone number when telephone-number-driven behavior is exercised.
- `PaymentOperation` invariants surrounding void-and-reissue and discount ceilings applied to `Amount` / `DiscountAmount`.
- `Test` catalog invariants surrounding `CompletionDurationMinutes`, `ResultKind`, and `IsSentOut` + cost-price co-existence.
- Auditable-entity base equality by identifier.
- Strongly-typed identifier equality and non-interchangeability (`PatientId` vs `LabId` vs `TestId`).

### 3.2 Application-layer tests (`TopLab.Application.Tests`)

**Purpose.** Verify handler behavior, validator behavior, and the composition of pipeline behaviors (validation, authorization, logging), using fakes for every Infrastructure interface.

**Rules.**

- No handler test touches a real database. Persistence interfaces are replaced by in-memory or fake doubles.
- Every Command has a handler test class covering, at minimum: happy path, each validation failure branch, each authorization failure branch, and each expected business-rule failure.
- Every Query has a handler test class covering the happy path and every documented restricted-access rejection.
- Validator tests exhaustively cover each rule of each validator; validators are tested independently of their handler.
- Pipeline behaviors have their own tests: `ValidationBehavior` short-circuits on invalid input; `AuthorizationBehavior` short-circuits on missing permission; `LoggingBehavior` records outcome and duration.

**Mandatory handler-level tests (representative — extend per feature).**

- `RegisterPatientCommand`: rejects missing name/sex/age; captures multiple telephone numbers; applies default account type when none is supplied; accepts optional national ID, address, treating doctor and referral entity; produces a `Result.Success` containing the generated Patient identifier.
- Discount-limit enforcement: a payment operation whose discount exceeds the operating user's `DiscountLimitPercent` produces `Result.Failure` of type `Validation`.
- Print-block-on-balance: with `User.BlockPrintOnRemainingBalance = true` and an outstanding balance, the print-a-report command produces `Result.Failure` of type `Forbidden` / `Conflict` (whichever the handler declares) — the exact `ErrorType` chosen is asserted verbatim by the test.
- Void-and-reissue on `PaymentOperation`: the correction path never physically deletes a row; the voided row remains queryable but excluded from balance computation.
- Restricted `P` / `T` queries: the same query returns success for a user with `IsAbsolutePermission = true` or with the specific `PT_AUDIT_ACCESS` grant, and `Result.Failure` of type `Forbidden` for any other user.
- Culture antibiotic filtering: antibiotics flagged `Pregnant` appear only when the patient is pregnancy-indicated; antibiotics flagged `Children` appear only for patients under 12 years.
- Patient-history retrieval by name or by Lab ID: both paths return the same visits when the patient has multiple registrations sharing the same Lab ID.
- Multiple-phone-number search: a query with any one of a patient's stored telephone numbers returns that patient's record.
- Connection configuration is intentionally *not* an Application-layer responsibility: the file read/write contract and the first-run wizard live in Presentation and are verified at the Presentation/manual layer (§3.4, §7.3).

### 3.3 Infrastructure tests (`TopLab.Infrastructure.Tests`)

**Purpose.** Verify EF Core entity configurations, migration correctness, interceptor behavior, and the concrete implementations of Infrastructure services.

**Rules.**

- Tests that exercise EF Core run against an ephemeral SQL Server instance (LocalDB or an equivalent per-test SQL Server database). No shared or production database is used.
- Every entity configuration has a test asserting: primary key mapping, required-column presence, nullable-column presence, decimal precision `(18, 2)` for money columns, `nvarchar` types for text columns, UTC-stored `datetime2` semantics for timestamp columns, and correct foreign-key mapping.
- The `AuditableEntitySaveChangesInterceptor` has explicit tests: on `Added`, it populates `CreatedByUserId`, `CreatedAtUtc`, and sets `ModificationCount = 0`; on `Modified`, it updates `LastModifiedByUserId`, `LastModifiedAtUtc`, increments `ModificationCount`, and never touches `CreatedByUserId`/`CreatedAtUtc`.
- Single-row configuration tables (`SystemSettings`, `ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`) are tested to reject a second insert.
- Soft-delete filtering is tested: queries by default exclude `IsDeleted = 1` rows, and the explicit unfiltered path returns them.
- Migrations are applied against an empty database and rolled back cleanly in an automated test that creates the schema and drops it at the end of the run.
- Printing and barcode services are tested through their `IReportPrintingService`, `IReceiptPrintingService`, `IEnvelopePrintingService`, and `IBarcodeService` interfaces, using either a physical or virtual printer / barcode target as configured for the test environment; when neither is available, tests assert the service correctly surfaces a `Result.Failure` of type `Unexpected` rather than throwing.

### 3.4 Presentation-layer verification

- ViewModels are exercised by unit tests that dispatch commands to the mediator via a fake and assert the resulting UI-state properties (loading flags, error text produced by `ResultErrorPresenter`, navigation invocations).
- No UI-automation layer is mandated. Manual verification checklists (see §7) supplement automated coverage for user-facing behaviors that cannot be asserted without rendering the visual tree.
- **First-run setup wizard (smoke).** With no `%ProgramData%\TopLab\appsettings.json` and no local `appsettings.json`, the application must show `DatabaseSetupWindow` and stay alive (no raw crash). After the wizard saves, the next launch must skip the wizard, run `MigrateAsync` idempotently against the stored connection, and reach `MainWindow`. Verified in the Foundation phase end-to-end gate (§7.3) and at PDCA-5.
- **Building without a local `appsettings.json`.** `TopLab.Presentation.csproj` copies `appsettings.json` only when it exists (`Condition="Exists(...)"`), so a clean clone builds despite the file being gitignored; the committed `appsettings.example.json` still ships to the output.

---

## 4. Coverage Expectations

Coverage is measured per project, not globally. The minimum line-coverage figures below are floors, not ceilings; a test suite that meets them may still be rejected in audit if it omits the mandatory scenarios enumerated in §3.

| Project | Minimum line coverage |
|---|---|
| `TopLab.Domain` | 90% |
| `TopLab.Application` | 80% |
| `TopLab.Infrastructure` | 70% |

Public members without test coverage require an explicit waiver recorded in the module's handoff document.

---

## 5. Test-Data Rules

- Test data does not contain real patient names, real telephone numbers, real national IDs, or real financial figures.
- Fixed sample datasets used across tests are constructed via named builders (for example, `PatientBuilder.NewMale(age: 30)`, `TestBuilder.SimpleTest("CBC")`) rather than raw object initializers scattered through tests.
- The `IDateTimeProvider` and `ICurrentUserService` are always replaced by deterministic fakes in Application tests; direct calls to `DateTime.UtcNow` inside a test assertion are not permitted.

---

## 6. Automated Quality Gates (Binding)

Every pull request must pass the following gates before it may be merged:

1. **Compile gate.** The solution builds with zero errors and zero new warnings.
2. **Test gate.** All tests in every test project pass. New behavior is accompanied by new tests.
3. **Coverage gate.** Per-project line coverage floors (§4) are satisfied.
4. **Dependency-Rule gate.** No source file introduces a dependency that violates the layer rules (Domain has no external references beyond the base class library; Presentation does not reference Infrastructure or Domain directly; Application does not reference Infrastructure concrete types).
5. **Convention gate.** Naming, folder placement, and file layout follow the coding-standards conventions.
6. **Migration gate.** If schema changes were made, an EF Core migration is added and applies cleanly against an empty database.

Failure at any gate blocks the merge until the failure is resolved.

---

## 7. Manual Verification Checklists

The following checklists are applied per module during audit. Each item is Pass / Fail; a Fail on any item blocks Done.

### 7.1 Common checklist (every module)

- ☐ Every Command and Query is dispatched through the mediator; ViewModels do not call handlers directly.
- ☐ Every handler returns `Result` / `Result<T>`; no expected outcome escapes as an exception.
- ☐ The associated screens display failures via `ResultErrorPresenter`, never a raw stack trace.
- ☐ The permission-denial message text ("**أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام**") appears exactly as specified when an unauthorized action is attempted.
- ☐ Timestamps are persisted as UTC; conversion to local time occurs only at display.
- ☐ No workstation-scoped or branch-scoped data is introduced by the module.
- ☐ Every mutable business entity introduced by the module derives from `AuditableEntity` and has its audit columns populated automatically by the interceptor.

### 7.2 Module-specific criteria

**M02 — Patient Registration & Test Ordering.**

- ☐ Patient can be registered with one, two, three or more telephone numbers.
- ☐ Age is captured with its unit (day / month / year) and stored exactly as entered.
- ☐ Medical history is captured through the extensible catalog + join table, not through fixed boolean columns.
- ☐ Sample-type flags (Urine, Stool, Blood, Semen, CSF) and "Taken outside lab" are captured per ordered test.
- ☐ Default account type is applied from system settings when the user does not override it.
- ☐ Registration surface honors the "disable automatic title insertion" setting.

**M03 — Patient Billing.**

- ☐ Account totals are computed at query time from `PatientTest.PriceAtOrderTime` and `PaymentOperation.Amount` (`IsVoided = 0`); no running-total column is written.
- ☐ Discount entered exceeds the operating user's `DiscountLimitPercent` → rejected with `Validation` error.
- ☐ Correction of a payment operation uses void-and-reissue; the row is not physically deleted.
- ☐ The "+" extra-charge amount is added to the test total and stored on the payment operation with `IsExtraCharge = true`.

**M04 — Results Entry & Result Lifecycle.**

- ☐ Result flag (Normal / Low / High) is evaluated at entry time against the matching reference range and stored on `PatientTest.ResultFlag`.
- ☐ Changing a `ReferenceRange` after the fact does not retroactively alter previously stored `ResultFlag` values.
- ☐ Auto-completion setting, when enabled, marks the affected stages complete automatically; patient aggregate status recomputes from the resulting stages without exception.
- ☐ `Export` is orthogonal to the lifecycle and does not advance any stage.

**M07 — Reports.**

- ☐ Combined, blank, and history reports render the exact fields required (Patient ID with barcode or Lab ID per setting, name, age/sex, request date, print timestamp, results vs reference range, doctor's signature).
- ☐ System setting "print Lab ID instead of Patient ID" changes both the printed report and barcode stickers when enabled.
- ☐ System setting "print the patient's account and balance instead of the print date" changes the printed report accordingly.
- ☐ History auto-display setting behaves as configured (auto / off, by lab code / by patient name).

**M08 — Patient Search.**

- ☐ Search accepts any stored telephone number and retrieves the patient's record and associated data.
- ☐ Search by Lab ID returns every visit of the patient with dates.
- ☐ Result-state filters (not entered / not reviewed / not printed / not delivered) return the expected sets.
- ☐ Patient lists display the seven-state aggregate status icon computed via `PatientStatusCalculator`.

**M09 — Result Delivery.**

- ☐ Undelivered-results list respects the selected time period.
- ☐ Delivery view exposes paid / remaining-to-patient / remaining-to-lab, all computed at query time.
- ☐ Print-block-on-balance permission prevents printing when a balance remains, before delivery.

**M10 — Case Tracking, Audit & Traceability (P/T).**

- ☐ Access is denied to any user lacking `IsAbsolutePermission = true` or the `PT_AUDIT_ACCESS` grant, regardless of any other permission set.
- ☐ The `P` view surfaces: registering user, modification count, most recent modifying user, payment-receiving users — each with date and time where applicable.
- ☐ The `T` view surfaces, per selected test: entry user, reviewing user, printing user and print count, delivering user — each with date and time.

**M12 — Test Catalog & Reference Ranges.**

- ☐ Reference-range matching is age-unit-sensitive; no cross-unit conversion occurs.
- ☐ Low/high comments are optional and, when set, appear on the printed report at the corresponding boundary.

**M15 — Culture & Antibiotic Configuration.**

- ☐ New culture test types can be added entirely as an in-app operation, without database work.
- ☐ Antibiotics flagged `Pregnant` appear only for pregnancy-indicated patients.
- ☐ Antibiotics flagged `Children` appear only for patients under 12 years.

**M16 — Sent-Out Samples.**

- ☐ Per-lab, per-period accounts show total sent, paid, and remaining values, all computed at query time.
- ☐ Multiple partial payments per sent-out sample are supported; full settlement is reached when the sum equals `CostPrice`.

**M17 — Users & Permissions.**

- ☐ The internal windows password gate is required before sensitive windows open.
- ☐ Adding a user assigns absolute or limited permissions and, in limited mode, the granular thirteen items.
- ☐ Discount-limit percentage is captured per user and enforced downstream.
- ☐ Last-login date/time is recorded and visible in user management.
- ☐ The default `admin` user exists.

**M18 — Attendance.**

- ☐ Check-in, break start/end, and check-out are captured per user.
- ☐ Overtime and lateness are visible only to the system manager.

**M20 — Inventory & Lab Accounting.**

- ☐ The internal windows password gate is required before the inventory window opens.
- ☐ Inventory figures are computed at query time over `PatientTest`, `PaymentOperation`, `SentOutSample`/`SentOutSamplePayment`, and `CashMovement`, filtered by the selected period.
- ☐ Per-element inventories (user / referral entity / treating doctor / account type / sent-out) reflect the underlying primary records.

**M22 — System & Print Settings.**

- ☐ Daily-backup enable + destination path + path-check behave as specified.
- ☐ Database Maintenance provides backup, restore, and update actions against the single shared database.
- ☐ Database-server settings (server name, login, database name) live in workstation-local configuration, never in a database table.
- ☐ Report top margin cannot exceed 8 cm.

**M23 — Utilities.**

- ☐ Utilities is opened from the main navigation and functions independently of other modules.
- ☐ The Utilities Phone Book is separate from the patient telephone-number capability.

### 7.3 Cross-cutting non-functional checks

- ☐ No layer introduces an Internet dependency.
- ☐ Every workstation on the LAN sees the same live figures for a given time window.
- ☐ Backup covers the entire single shared database as one unit; no table is excluded.
- ☐ The permission-denial message is delivered without leaking information about what would have happened had the action succeeded.

---

## 8. Audit Acceptance Criteria (Per Module)

A module is accepted as Done only when all of the following are true:

1. Every functional requirement scoped to the module is realized in code and demonstrated by a passing automated test or a passing manual checklist item from §7.
2. Every business rule scoped to the module (from BR-01 through BR-13, where applicable) is realized in code and demonstrated by a passing automated test.
3. Every entity introduced by the module conforms to the modeling conventions: single-column surrogate primary key named `<Entity>Id`; audit columns present on auditable entities; soft-delete column present where required; `decimal(18,2)` for money; `datetime2` UTC for timestamps.
4. All automated quality gates (§6) pass.
5. All items in the common checklist (§7.1) pass.
6. All module-specific items (§7.2, if any) pass.
7. All relevant cross-cutting checks (§7.3) pass.
8. Coverage floors (§4) are met, and every mandatory test scenario listed in §3 for the module's layers is present.
9. The Master Tracking Sheet row for the module is updated to reflect the transition to Done.
10. A handoff document has been produced and signed off.

A module that fails any single criterion is not Done. Waiving a criterion requires an explicit deviation entry in the handoff document (Deviations and Waivers section) with a corrective-action deadline.

---

## 9. Non-Functional Verification

- **Concurrency.** Two workstations updating adjacent, independent records (for example, entering results for two different patients) shall not interfere; verified by an Infrastructure-layer test opening two `ApplicationDbContext` instances concurrently.
- **Consistency.** After a payment operation is written on one workstation, another workstation's patient-balance query reflects the new figure without any client-side cache invalidation; verified by an Infrastructure-layer test using two contexts against the same database.
- **Auditability.** Every mutation of an auditable entity increments `ModificationCount` and updates `LastModifiedByUserId` and `LastModifiedAtUtc`; verified by dedicated interceptor tests.
- **Recoverability.** A backup produced by the Database Maintenance function restores into an empty database and reproduces every table, row, and audit column of the source; verified by an integration test that backs up, drops, restores, and compares row counts and audit-column checksums.

---

## 10. Reporting Test Results

- Every pull request produces a build log, a test log, and a coverage report; all three are attached to the pull request.
- Every failing test in the log names the test method, the layer, and the file location.
- Coverage figures are reported per project (§4). A coverage drop against `main` requires a documented justification in the pull request description.

---

## 11. Test-Suite Maintenance

- A test is deleted only when the behavior it verifies has been intentionally removed. Test removal is called out in the pull request description.
- A test that becomes flaky (non-deterministic across runs) is triaged as a High-severity item in the module's handoff and fixed before further module work continues.
- Fakes and builders are refactored alongside the code they support; leaving obsolete fakes in the test tree is not permitted.

---

*End of document.*
