# Top-Lab — Architecture & Folder Structure Blueprint

## نظام توب لاب — الهيكلية المعمارية وبنية المشروع

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Architecture & Folder Structure Blueprint |
| Status | **Final** — approved architecture baseline |
| Document purpose | This document defines the mandatory software architecture, solution structure, layer responsibilities, and structural conventions for Top-Lab. It governs how the system's functional scope is organized into code, independent of any particular screen or database design detail. It is binding on all implementation work. |

---

## 1. Purpose and Scope of This Document

This document specifies **how Top-Lab is structured as a software system**. It answers: which layers exist, what each layer is allowed to know about, how features are organized on disk, how errors are represented and propagated, and what conventions every part of the codebase must follow.

This document does **not** specify:

- Individual database tables or columns (see the Data Model document).
- Screen-by-screen UI layout (see the UI/UX Blueprint).
- Feature-by-feature business rules (see the Business Logic specifications).

Every architectural decision in this document is **binding**. Any implementation that violates the Dependency Rule (§2.2), the layer responsibilities (§4), or the error-handling convention (§6.1) is non-conforming and must be corrected before acceptance.

---

## 2. Architectural Principles

### 2.1 Style

Top-Lab is built using **Clean Architecture**, composed with **CQRS** (Command Query Responsibility Segregation) inside the Application layer, and presented through **WPF** using the **MVVM** (Model–View–ViewModel) pattern.

The system is a **single Windows desktop application**, deployed on a **local area network**, with **multiple workstations connecting to one shared SQL Server database** at one physical site. There is no web tier, no external API surface, and no internet dependency anywhere in the architecture.

### 2.2 The Dependency Rule

Source-code dependencies point in one direction only: **outward layers depend on inner layers; inner layers never depend on outward layers.**

```
Presentation  ──depends on──▶  Application  ──depends on──▶  Domain
Infrastructure ──depends on──▶  Application  ──depends on──▶  Domain
```

- **Domain** has no dependency on any other layer. It does not reference Entity Framework Core, WPF, or any external library beyond the base class library.
- **Application** depends only on Domain. It defines interfaces (ports) for anything it needs from the outside world (persistence, printing, barcode generation) but never implements them.
- **Infrastructure** depends on Application (to implement its interfaces) and on Domain (to persist and reconstruct domain objects). Infrastructure is a plugin to Application, never the reverse.
- **Presentation** depends on Application only. It never references Infrastructure or Domain types directly in view code; it communicates exclusively through Application-layer contracts (Commands, Queries, and their results).

### 2.3 Why this matters for a multi-agent build process

Because different parts of Top-Lab may be implemented by different coding agents at different times, the Dependency Rule is the single most important guardrail in this document. An agent working on a screen (Presentation) must never need to know how data is stored (Infrastructure). An agent working on persistence must never need to know how a screen looks. Each layer is independently testable and independently replaceable because of this separation.

---

## 3. Solution Structure Overview

The solution is organized as **four class library projects plus one executable**, plus a mirrored test project per layer.

```
TopLab.sln
│
├── src/
│   ├── TopLab.Domain/            (class library — no external dependencies)
│   ├── TopLab.Application/       (class library — depends on Domain)
│   ├── TopLab.Infrastructure/    (class library — depends on Application, Domain)
│   └── TopLab.Presentation/      (WPF executable — depends on Application)
│
└── tests/
    ├── TopLab.Domain.Tests/
    ├── TopLab.Application.Tests/
    └── TopLab.Infrastructure.Tests/
```

No other top-level projects exist. Every functional capability of Top-Lab is implemented as an addition **inside** one or more of these four projects — never as a new top-level project — unless a future architectural revision explicitly authorizes one.

---

## 4. Layer Specifications

### 4.1 Domain Layer — `TopLab.Domain`

**Responsibility.** The Domain layer contains the system's core business concepts and the rules that must always hold true, regardless of how the system is used or where the data is stored.

**Contains:**

- **Entities** — objects with identity that persist over time (e.g., a patient, a test, a result, a user).
- **Value Objects** — immutable objects defined only by their attributes, with no identity of their own (e.g., a phone number, a money amount, an age with its unit).
- **Domain Services** — stateless operations that don't naturally belong to a single entity but express a core business rule spanning multiple entities (e.g., the patient aggregate-status precedence computation described in §7).
- **Domain Events** (where warranted) — signals that something significant happened inside the domain, to be reacted to elsewhere without coupling the Domain layer to the reaction.
- **Enumerations and constants** that represent domain concepts (e.g., result lifecycle stage, permission level).
- **Domain-level validation** — invariants that must always be true for an entity to be in a valid state (e.g., a test cannot be marked printed before it is marked reviewed).

**Must not contain:** any reference to Entity Framework Core, SQL, WPF, MediatR, file I/O, printers, or any other external concern. The Domain layer must compile and be fully testable with zero external package references beyond the .NET base class library.

**Folder structure:**

```
TopLab.Domain/
├── Patients/
│   ├── Patient.cs
│   ├── PatientPhoneNumber.cs
│   └── PatientId.cs / LabId.cs   (strongly-typed identifiers)
├── Tests/
│   ├── Test.cs
│   ├── TestGroup.cs
│   ├── CustomGroup.cs
│   ├── ReferenceRange.cs
│   └── TestComment.cs
├── Results/
│   ├── Result.cs
│   ├── ResultLifecycleStage.cs
│   ├── ProfileResult.cs
│   ├── CultureResult.cs
│   └── AntibioticSensitivity.cs
├── PatientStatus/
│   ├── PatientAggregateStatus.cs
│   └── PatientStatusCalculator.cs   (domain service — precedence rule)
├── Billing/
│   ├── PatientAccount.cs
│   ├── PaymentOperation.cs
│   └── PriceList.cs
├── ExternalEntities/
│   ├── TreatingDoctor.cs
│   ├── ReferralEntity.cs
│   └── PartnerLab.cs
├── SentOutSamples/
│   ├── SentOutSample.cs
│   └── ExternalLabSettlement.cs
├── Users/
│   ├── User.cs
│   ├── Permission.cs
│   └── DiscountLimit.cs
├── Attendance/
│   └── AttendanceRecord.cs
├── Audit/
│   ├── PatientAuditTrail.cs        (P-button concept)
│   └── TestAuditTrail.cs           (T-button concept)
├── Accounting/
│   ├── CashDrawerInventory.cs
│   └── CompanyDelegateAccount.cs
├── SampleCollection/
│   └── SampleDrawStatus.cs
├── Settings/
│   ├── ReportSettings.cs
│   ├── ReceiptSettings.cs
│   ├── EnvelopeSettings.cs
│   └── PrinterAssignment.cs
└── Common/
    ├── Entity.cs                   (base class: Id, equality)
    ├── ValueObject.cs              (base class: structural equality)
    ├── AuditableEntity.cs          (CreatedBy, CreatedAt, ModifiedBy, ModifiedAt, ModificationCount)
    ├── DomainException.cs
    └── Enums/
```

### 4.2 Application Layer — `TopLab.Application`

**Responsibility.** The Application layer orchestrates use cases. It contains no business rules of its own beyond coordination — actual rule enforcement lives in the Domain layer. The Application layer answers "what happens when the user does X," not "what is always true about a patient."

**Organizing principle: feature folders.** The Application layer is organized **by feature**, not by technical type. Every feature folder is self-contained: its commands, queries, handlers, and validators live together, so that a single feature can be implemented, reviewed, or handed to an agent as one coherent unit of work.

```
TopLab.Application/
├── Features/
│   ├── AccessAndNavigation/
│   ├── PatientRegistration/
│   │   ├── Commands/
│   │   │   ├── RegisterPatient/
│   │   │   │   ├── RegisterPatientCommand.cs
│   │   │   │   ├── RegisterPatientCommandHandler.cs
│   │   │   │   └── RegisterPatientCommandValidator.cs
│   │   │   ├── AddTestsToVisit/
│   │   │   └── UpdatePatientData/
│   │   └── Queries/
│   │       ├── SearchPatients/
│   │       └── GetPatientHistory/
│   ├── PatientBilling/
│   ├── ResultsEntry/
│   ├── SpecializedProfileReports/
│   ├── CultureAndSensitivity/
│   ├── ReportProduction/
│   ├── PatientSearchAndVisitHistory/
│   ├── ResultDelivery/
│   ├── AuditAndTraceability/
│   ├── WorkSheets/
│   ├── TestCatalogAndReferenceRanges/
│   ├── PriceListsAndCustomGroups/
│   ├── ExternalEntities/
│   ├── CultureConfiguration/
│   ├── SentOutSamples/
│   ├── UsersAndPermissions/
│   ├── Attendance/
│   ├── Statistics/
│   ├── InventoryAndAccounting/
│   ├── SampleCollection/
│   ├── SystemAndPrintSettings/
│   └── Utilities/
├── Common/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   ├── AuthorizationBehavior.cs
│   │   └── LoggingBehavior.cs
│   ├── Results/
│   │   ├── Result.cs
│   │   ├── Result{T}.cs
│   │   └── Error.cs
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs
│   │   ├── IReportPrintingService.cs
│   │   ├── IBarcodeService.cs
│   │   ├── ICurrentUserService.cs
│   │   └── IDateTimeProvider.cs
│   └── Mappings/
└── DependencyInjection.cs
```

**Each feature folder follows the same internal shape:**

- `Commands/<UseCaseName>/` — one folder per command, containing the command (a plain data record), its handler, and its validator.
- `Queries/<UseCaseName>/` — the same shape for read operations.
- Commands change state; Queries never change state. A single use case is never both.

**CQRS mediation.** Every command and query is dispatched through a single mediator (MediatR). Presentation code never calls a handler directly — it always sends a Command or Query object through the mediator and receives a `Result` back.

### 4.3 Infrastructure Layer — `TopLab.Infrastructure`

**Responsibility.** The Infrastructure layer provides concrete implementations of everything the Application layer declared as an interface: database access, report/receipt/envelope printing, barcode generation, file-system backup operations, and any other technical concern.

```
TopLab.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── PatientConfiguration.cs
│   │   ├── TestConfiguration.cs
│   │   ├── ResultConfiguration.cs
│   │   └── ... (one configuration class per entity)
│   ├── Migrations/
│   ├── Repositories/
│   │   └── (only where a repository abstraction adds real value beyond IApplicationDbContext)
│   └── Interceptors/
│       └── AuditableEntitySaveChangesInterceptor.cs
├── Printing/
│   ├── ReportPrintingService.cs
│   ├── ReceiptPrintingService.cs
│   └── EnvelopePrintingService.cs
├── Barcode/
│   └── BarcodeService.cs
├── BackupAndMaintenance/
│   └── DatabaseMaintenanceService.cs
├── Identity/
│   └── CurrentUserService.cs
└── DependencyInjection.cs
```

**Rule.** Infrastructure classes implement Application-layer interfaces. Application never references an Infrastructure class by name. Wiring happens exclusively in each layer's `DependencyInjection.cs`, composed together at application startup in Presentation.

### 4.4 Presentation Layer — `TopLab.Presentation`

**Responsibility.** The Presentation layer is the WPF desktop application: windows, user controls, and the ViewModels that bind to them. It contains no business logic and no direct data access.

```
TopLab.Presentation/
├── Views/
│   ├── Shell/                       (main window, navigation bar, status bar)
│   ├── PatientRegistration/
│   ├── ResultsEntry/
│   ├── ReportPreview/
│   ├── PatientSearch/
│   ├── Accounts/
│   ├── Statistics/
│   ├── UsersAndPermissions/
│   ├── Settings/
│   └── ... (one folder per screen area, mirroring Application/Features)
├── ViewModels/
│   ├── Shell/
│   ├── PatientRegistration/
│   └── ... (mirrors Views/)
├── Common/
│   ├── ViewModelBase.cs
│   ├── RelayCommand.cs
│   ├── Converters/
│   ├── Navigation/
│   │   └── INavigationService.cs / NavigationService.cs
│   └── ErrorPresentation/
│       └── ResultErrorPresenter.cs  (translates a failed Result into a user-facing message)
├── App.xaml / App.xaml.cs           (composition root — wires DI for all layers)
└── AssemblyInfo.cs
```

**Rule.** A ViewModel depends only on `IMediator` (to send Commands/Queries) and on Presentation-layer services (navigation, dialogs). It never depends on `ApplicationDbContext`, EF Core types, or any Infrastructure class.

---

## 5. Feature-to-Layer Traceability

Every functional area of Top-Lab maps to exactly one feature folder in the Application layer, one corresponding folder in Presentation, and one or more entity groupings in the Domain layer, as shown below.

| Functional area | Application feature folder | Primary Domain grouping |
|---|---|---|
| Application access & main navigation | `AccessAndNavigation` | `Users` |
| Patient registration & test ordering | `PatientRegistration` | `Patients`, `Tests` |
| Patient billing & account settlement | `PatientBilling` | `Billing` |
| Results entry & result lifecycle | `ResultsEntry` | `Results`, `PatientStatus` |
| Specialized profile result reports | `SpecializedProfileReports` | `Results` |
| Culture & sensitivity result entry | `CultureAndSensitivity` | `Results` |
| Combined, blank & history reports | `ReportProduction` | `Results`, `Patients` |
| Patient search, Lab ID & visit history | `PatientSearchAndVisitHistory` | `Patients` |
| Result delivery & settlement at handover | `ResultDelivery` | `Results`, `Billing` |
| Case tracking, audit & traceability | `AuditAndTraceability` | `Audit` |
| Work sheets | `WorkSheets` | `Tests`, `Patients` |
| Test catalog & reference ranges | `TestCatalogAndReferenceRanges` | `Tests` |
| Price lists, comments & custom groups | `PriceListsAndCustomGroups` | `Tests`, `Billing` |
| External entities | `ExternalEntities` | `ExternalEntities` |
| Culture & antibiotic configuration | `CultureConfiguration` | `Tests` |
| Sent-out samples | `SentOutSamples` | `SentOutSamples` |
| User & permission management | `UsersAndPermissions` | `Users` |
| Attendance & time tracking | `Attendance` | `Attendance` |
| Statistics | `Statistics` | (read-only projections; no dedicated domain entities) |
| Inventory & lab accounting | `InventoryAndAccounting` | `Accounting` |
| Sample collection & separation | `SampleCollection` | `SampleCollection` |
| System & print settings | `SystemAndPrintSettings` | `Settings` |
| Utilities | `Utilities` | (no domain dependency — self-contained tools) |

This table is the authoritative map between "what the system does" and "where it lives in code." Any new capability is added by locating its row here first; if no row fits, the table is extended before code is written.

---

## 6. Cross-Cutting Concerns

### 6.1 Error Handling — the Result Pattern (binding)

Top-Lab does **not** use exceptions to communicate expected, anticipated outcomes such as "patient not found," "discount exceeds limit," or "cannot print while a balance remains." Exceptions are reserved exclusively for truly exceptional, unanticipated failures (e.g., loss of database connectivity, a corrupted file).

**Every Command and Query handler returns a `Result` or `Result<T>`.**

```
Result                — represents success or failure with no return value
Result<T>              — represents success (carrying a value of type T) or failure
Error                  — { Code: string, Message: string, Type: ErrorType }
ErrorType               — Validation | NotFound | Conflict | Forbidden | Unexpected
```

**Flow through the layers:**

1. **Domain** raises a business-rule violation by returning a `Result` (or a domain-level equivalent) from the entity method that enforces the rule — never by throwing for an expected condition.
2. **Application** handlers propagate the Domain result outward, or produce their own `Result.Failure(...)` for orchestration-level problems (e.g., a referenced entity does not exist).
3. **Infrastructure** translates technical failures (a database timeout, a printer not found) into an `Error` of type `Unexpected` before it crosses back into Application; raw exceptions from external libraries never leak past Infrastructure.
4. **Presentation** never receives a raw exception from a use case. Every ViewModel calls the mediator, receives a `Result`, and — on failure — passes the `Error` to `ResultErrorPresenter`, which renders a consistent, user-facing message. On success, the ViewModel proceeds with the returned value.

**Validation** is a specialized producer of `Result` failures: `ValidationBehavior` runs each Command/Query through its associated validator before the handler executes, short-circuiting with a `Result.Failure` of type `Validation` on any rule violation. Handlers never receive invalid input.

### 6.2 Validation

Input and business-precondition validation is expressed declaratively per Command/Query using a fluent validation library, colocated with the Command/Query it validates (see the feature-folder shape in §4.2). Validation failures are collected and returned as a single `Result` carrying every violated rule, not just the first one encountered — so a user (or an agent building the corresponding screen) can present all problems at once.

### 6.3 Authorization

Every Command and Query that requires a specific permission level (e.g., P/T audit access restricted to System Administrator / Absolute Permissions) declares its required permission as part of its definition. `AuthorizationBehavior` checks this before the handler executes and returns a `Result.Failure` of type `Forbidden` if the current user lacks the required permission. No handler re-implements its own permission check — the pipeline behavior is the single enforcement point.

### 6.4 Auditability

Every entity that requires creation/modification tracking derives from `AuditableEntity` (§4.1), which carries `CreatedByUserId`, `CreatedAtUtc`, `LastModifiedByUserId`, `LastModifiedAtUtc`, and a running `ModificationCount`. These fields are populated automatically by `AuditableEntitySaveChangesInterceptor` in the Infrastructure layer at the moment of persistence — no handler sets them manually, which guarantees they cannot be forgotten or falsified by an incomplete implementation.

The restricted per-test audit trail (result entry user, review user, printing user and count, delivery user, with timestamps for each) is modeled as a dedicated `TestAuditTrail` entity in the `Audit` domain grouping, populated at each lifecycle transition by the corresponding `ResultsEntry` / `ResultDelivery` command handlers, and exposed only through the `AuditAndTraceability` feature, which is gated end-to-end by the authorization rule in §6.3.

### 6.5 Logging

`LoggingBehavior` wraps every Command and Query with structured logging of the request name, the outcome (success/failure and error type on failure), and execution duration. Logging is a pipeline concern, not something each handler implements individually.

### 6.6 Shared Kernel

Top-Lab uses a **single, shared Domain project** (`TopLab.Domain`) rather than per-feature domain models. Entities referenced by more than one functional area — most notably the patient, the test, and the result — are defined exactly once and reused everywhere they are needed. This avoids duplicate or divergent definitions of the same real-world concept across features and keeps the patient-status precedence rule (§7) as the single computation that every feature reads from, rather than a rule reimplemented per screen.

---

## 7. Patient Aggregate Status — Architectural Placement

The seven-state patient-level status is a core business rule, not a UI concern and not a stored, independently-editable field. It is implemented as a **Domain Service** (`PatientStatusCalculator`) that computes the current aggregate status from the live state of a patient's analyses and account, using the precedence rule (earliest incomplete stage across all analyses and the account). Every screen or report that displays patient status (results entry, patient search, delivery) calls the same calculation through the Application layer — the status is never independently maintained or duplicated per feature.

---

## 8. Persistence Conventions

- **One `ApplicationDbContext`**, matching the single shared SQL Server database.
- **One EF Core entity configuration class per entity**, using the Fluent API exclusively (no data annotations on Domain entities, to keep the Domain layer free of persistence concerns).
- **Migrations** live in `TopLab.Infrastructure/Persistence/Migrations` and are applied only against the single shared database; no per-workstation database state exists.
- **Soft-delete** is used wherever the product requires records to remain for audit purposes after logical deletion; hard deletes are reserved for cases with no audit requirement.
- **Strongly-typed identifiers** (e.g., `PatientId`, `LabId`, `TestId`) are used in place of raw primitives to prevent identifier mix-ups across the many distinct ID concepts in the system.

---

## 9. Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Command | `<Verb><Noun>Command` | `RegisterPatientCommand` |
| Command handler | `<CommandName>Handler` | `RegisterPatientCommandHandler` |
| Command validator | `<CommandName>Validator` | `RegisterPatientCommandValidator` |
| Query | `<Verb><Noun>Query` | `SearchPatientsQuery` |
| Domain entity | Singular noun | `Patient`, `Test`, `Result` |
| Domain service | `<Concept>Calculator` / `<Concept>Service` | `PatientStatusCalculator` |
| Application interface (port) | `I<Capability>` | `IReportPrintingService` |
| Infrastructure implementation | `<Capability>Service` / `<Capability>Repository` | `ReportPrintingService` |
| ViewModel | `<Screen>ViewModel` | `PatientRegistrationViewModel` |
| EF configuration class | `<Entity>Configuration` | `PatientConfiguration` |

---

## 10. Testing Structure

Tests mirror the production layer they exercise, one-to-one:

```
tests/
├── TopLab.Domain.Tests/            (entity invariants, domain services, value objects)
├── TopLab.Application.Tests/       (handler behavior, validation, pipeline behaviors — with fakes for Infrastructure interfaces)
└── TopLab.Infrastructure.Tests/    (EF Core configuration behavior, printing/barcode integration, migration correctness)
```

The Presentation layer is exercised through manual verification and, where practical, ViewModel-level unit tests that call the mediator with fakes; no UI-automation layer is mandated by this document.

---

## 11. Non-Functional Alignment

- **Offline by design.** No layer in this architecture introduces an internet dependency. All Infrastructure implementations (persistence, printing, barcode, backup) operate entirely against local or LAN resources.
- **Single shared database.** `ApplicationDbContext` connects to exactly one SQL Server instance shared by all workstations at the site; the architecture contains no per-workstation data store and no synchronization layer, because none is needed.
- **Single branch.** No branch-scoping concept exists anywhere in Domain, Application, or Infrastructure; introducing one would be a deviation from this architecture and requires a formal revision of this document first.
- **Operational safety.** The Result pattern (§6.1) ensures that an unfinished or invalid operation is always visible to the user as a clear message rather than a silent failure or an unhandled crash, directly supporting predictable, attributable system behavior.

---

## 12. Full Solution Tree (Reference)

```
TopLab.sln
├── src/
│   ├── TopLab.Domain/
│   │   ├── Patients/
│   │   ├── Tests/
│   │   ├── Results/
│   │   ├── PatientStatus/
│   │   ├── Billing/
│   │   ├── ExternalEntities/
│   │   ├── SentOutSamples/
│   │   ├── Users/
│   │   ├── Attendance/
│   │   ├── Audit/
│   │   ├── Accounting/
│   │   ├── SampleCollection/
│   │   ├── Settings/
│   │   └── Common/
│   ├── TopLab.Application/
│   │   ├── Features/
│   │   │   ├── AccessAndNavigation/
│   │   │   ├── PatientRegistration/
│   │   │   ├── PatientBilling/
│   │   │   ├── ResultsEntry/
│   │   │   ├── SpecializedProfileReports/
│   │   │   ├── CultureAndSensitivity/
│   │   │   ├── ReportProduction/
│   │   │   ├── PatientSearchAndVisitHistory/
│   │   │   ├── ResultDelivery/
│   │   │   ├── AuditAndTraceability/
│   │   │   ├── WorkSheets/
│   │   │   ├── TestCatalogAndReferenceRanges/
│   │   │   ├── PriceListsAndCustomGroups/
│   │   │   ├── ExternalEntities/
│   │   │   ├── CultureConfiguration/
│   │   │   ├── SentOutSamples/
│   │   │   ├── UsersAndPermissions/
│   │   │   ├── Attendance/
│   │   │   ├── Statistics/
│   │   │   ├── InventoryAndAccounting/
│   │   │   ├── SampleCollection/
│   │   │   ├── SystemAndPrintSettings/
│   │   │   └── Utilities/
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   ├── Results/
│   │   │   ├── Interfaces/
│   │   │   └── Mappings/
│   │   └── DependencyInjection.cs
│   ├── TopLab.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   ├── Migrations/
│   │   │   ├── Repositories/
│   │   │   └── Interceptors/
│   │   ├── Printing/
│   │   ├── Barcode/
│   │   ├── BackupAndMaintenance/
│   │   ├── Identity/
│   │   └── DependencyInjection.cs
│   └── TopLab.Presentation/
│       ├── Views/
│       ├── ViewModels/
│       ├── Common/
│       │   ├── Converters/
│       │   ├── Navigation/
│       │   └── ErrorPresentation/
│       ├── App.xaml
│       └── App.xaml.cs
└── tests/
    ├── TopLab.Domain.Tests/
    ├── TopLab.Application.Tests/
    └── TopLab.Infrastructure.Tests/
```

---

*End of document.*
