
# Top-Lab — Architecture Decision Record (ADR) Log

## نظام توب لاب — سجل قرارات المعمارية

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Architecture Decision Record (ADR) Log |
| Status | **Active** — cumulative decision log |
| Purpose | Record architecturally significant decisions that shape the Top-Lab codebase. Each ADR is immutable once accepted; superseding decisions are added as new records that reference the earlier one. |

---

## 1. How to Use This Log

- Every ADR is numbered sequentially (`ADR-0001`, `ADR-0002`, …) and never renumbered.
- An ADR is added when a decision materially constrains structure, layering, technology choice, cross-cutting concern, data-model semantics, or team convention.
- An ADR is never edited to reverse its outcome. To change a decision, add a new ADR that declares the earlier one **Superseded** and cross-references it in both directions.

### 1.1 ADR states

| State | Meaning |
|---|---|
| Proposed | Under discussion; not yet binding |
| Accepted | Binding; implementations must comply |
| Superseded | Replaced by a later ADR (referenced in the record) |

### 1.2 ADR template

```
# ADR-NNNN — <Short title>

Status: Proposed | Accepted | Superseded by ADR-XXXX
Date:   YYYY-MM-DD

Context
    What forces are at play; why a decision is required now.

Decision
    The choice made, stated as a directive (imperative voice).

Consequences
    Positive outcomes, trade-offs accepted, and constraints imposed
    on future work.

Related
    Other ADRs, requirements, or rules this decision interacts with.
```

---

## 2. Accepted Decisions

### ADR-0001 — Target platform is Windows desktop, .NET 8

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Top-Lab is a single-site laboratory management application deployed at one physical site on a local area network, with no external-Internet dependency. A single deployment target must be fixed before any layer or library selection can proceed.

**Decision.** Top-Lab is a Windows desktop application built on .NET 8.

**Consequences.**
- No cross-platform runtime constraints apply; APIs and libraries exclusive to Windows/.NET 8 may be used.
- No web front-end, browser client, mobile client, or hosted service is part of the product.
- Distribution and update mechanisms are limited to those compatible with Windows desktop deployment on a LAN.

**Related.** ADR-0002, ADR-0003, ADR-0010.

---

### ADR-0002 — WPF with the MVVM pattern for the presentation layer

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The application requires a native Windows desktop UI supporting Arabic-first rendering, rich data-entry screens, print previews, and status-driven visual indicators. A single presentation technology and pattern must be fixed to constrain screen construction.

**Decision.** The presentation layer uses **WPF** exclusively, and every screen is implemented using the **Model–View–ViewModel (MVVM)** pattern. Views contain no business logic; ViewModels communicate with the rest of the system only through Application-layer contracts.

**Consequences.**
- Alternative desktop UI stacks (WinForms, WinUI, MAUI, UWP) are not permitted in the Presentation layer.
- Data binding, `INotifyPropertyChanged`, and command patterns are the mandated interaction mechanism between Views and ViewModels.
- ViewModels must remain independent of Infrastructure and Domain concrete types.

**Related.** ADR-0001, ADR-0005, ADR-0007.

---

### ADR-0003 — Microsoft SQL Server as the single shared database

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Multiple workstations at one site must present a consistent operational view of patient, test, financial and audit data. A single database engine must be fixed to constrain schema, migrations, and connection management.

**Decision.** All persistent data is stored in one Microsoft SQL Server database shared by every workstation on the LAN. No per-workstation database, cache store, or alternative RDBMS is introduced.

**Consequences.**
- Every workstation reads and writes the same live figures; no reconciliation or synchronization layer is required or permitted.
- Database connection parameters (server name, login, database name) are workstation-local application configuration and never stored inside the database itself.
- Backup, restore, and maintenance operations target this single database as one unit.

**Related.** ADR-0004, ADR-0010, ADR-0012.

---

### ADR-0004 — Entity Framework Core as the sole data-access technology

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** A single, uniform data-access approach must be chosen for the Infrastructure layer so that persistence logic, migrations, and auditing behave consistently across all entities.

**Decision.** Entity Framework Core is the mandated data-access technology. All persistence work uses one `ApplicationDbContext` bound to the shared SQL Server database.

**Consequences.**
- Direct ADO.NET, Dapper, stored-procedure-first approaches, or alternative ORMs are not used in production code.
- Schema evolution is expressed exclusively through EF Core migrations under Infrastructure.
- Cross-cutting behaviors such as auditable-column population are implemented through EF Core interceptors, not by handler code.

**Related.** ADR-0003, ADR-0006, ADR-0011.

---

### ADR-0005 — Clean Architecture with four layers (Domain, Application, Infrastructure, Presentation)

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The system will be extended across many functional modules over time, potentially by different implementers. A layering discipline must be fixed so that dependencies never accumulate in a way that couples business rules to persistence or UI.

**Decision.** The solution follows Clean Architecture with four layers realized as four projects:

- `TopLab.Domain` — no dependencies on any other layer.
- `TopLab.Application` — depends only on Domain; defines interfaces for outside capabilities.
- `TopLab.Infrastructure` — depends on Application and Domain; implements Application interfaces.
- `TopLab.Presentation` — depends only on Application (through commands, queries, and their results).

Source-code dependencies flow strictly inward. Outer layers never appear in inner layers.

**Consequences.**
- Domain compiles with only the .NET base class library — no EF Core, WPF, MediatR, or file-I/O references.
- Presentation never references Infrastructure or Domain concrete types.
- Any capability added to the system belongs inside one of these four projects; new top-level projects require a superseding ADR.

**Related.** ADR-0002, ADR-0004, ADR-0006, ADR-0007.

---

### ADR-0006 — CQRS with MediatR inside the Application layer

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Use cases split cleanly into state-changing operations and read-only queries. A single dispatching mechanism is required so that cross-cutting concerns (validation, authorization, logging) apply uniformly and Presentation never invokes handlers directly.

**Decision.** The Application layer implements Command Query Responsibility Segregation. Every use case is either a Command (changes state) or a Query (does not change state), dispatched through a single mediator. Presentation code sends Commands and Queries via the mediator and receives a `Result` object back.

**Consequences.**
- No Command is also a Query; no handler both mutates state and returns non-metadata data mixed with mutation semantics.
- Cross-cutting pipeline behaviors (validation, authorization, logging) run once per request in a fixed order.
- Direct method calls into Application handlers from Presentation are non-conforming.

**Related.** ADR-0005, ADR-0007, ADR-0008, ADR-0009.

---

### ADR-0007 — Feature-folder organization inside the Application and Presentation layers

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The system contains many functional modules. Organizing code by technical type (all commands here, all handlers there) fragments a feature across many folders and slows implementation and review.

**Decision.** The Application layer is organized by feature. Each feature folder contains its own `Commands/<UseCase>/` and `Queries/<UseCase>/` subfolders holding the request, its handler, and its validator together. The Presentation layer mirrors this organization: one folder per screen area, mirroring the Application feature folders.

**Consequences.**
- A single feature can be delivered, reviewed, or handed off as one coherent unit of work.
- Adding a new capability requires either extending an existing feature folder or adding a new one that maps to a documented functional area.
- Views, ViewModels, commands, queries, handlers, and validators for one feature are colocated within their respective layers.

**Related.** ADR-0005, ADR-0006.

---

### ADR-0008 — Result pattern for expected outcomes; exceptions reserved for the unexpected

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Anticipated business outcomes such as "patient not found", "discount exceeds limit", or "cannot print while a balance remains" occur frequently. Communicating them through exceptions is expensive, hides intent, and complicates ViewModel logic.

**Decision.** Every Command and Query handler returns a `Result` or `Result<T>`. Failures carry an `Error` object with `Code`, `Message`, and `ErrorType` (`Validation`, `NotFound`, `Conflict`, `Forbidden`, `Unexpected`). Exceptions are used only for truly unanticipated conditions such as loss of database connectivity or a corrupted file. Infrastructure translates raw external exceptions into an `Error` of type `Unexpected` before they cross back into the Application layer.

**Consequences.**
- No handler throws to signal an expected business condition.
- Presentation renders failures uniformly via a single error presenter and never sees a raw exception from a use case.
- All expected failure paths are testable as return values.

**Related.** ADR-0006, ADR-0009.

---

### ADR-0009 — Cross-cutting concerns as MediatR pipeline behaviors

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Validation, authorization, and logging must be applied consistently to every use case. Duplicating these checks inside each handler is error-prone and non-uniform.

**Decision.** Validation, authorization, and logging are implemented as MediatR pipeline behaviors:

- `ValidationBehavior` runs the request through its validator and short-circuits on failure with a `Result.Failure` of type `Validation`, returning every violated rule.
- `AuthorizationBehavior` verifies the current user has the permission declared by the Command/Query and returns `Result.Failure` of type `Forbidden` if not.
- `LoggingBehavior` records the request name, outcome, and execution duration.

No handler re-implements any of these concerns.

**Consequences.**
- A single enforcement point exists for each cross-cutting concern.
- Invalid input never reaches a handler.
- Every request produces structured logging without per-handler code.

**Related.** ADR-0006, ADR-0008.

---

### ADR-0010 — Offline-only operation; no Internet dependency in any layer

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The product operates at a single site on a LAN and must not depend on any external service to perform its core functions.

**Decision.** No layer introduces an Internet dependency. Persistence, printing, barcode generation, backup, and maintenance operate entirely against local or LAN resources. No web tier, external API, cloud service, or online notification pathway is part of the product.

**Consequences.**
- Result delivery is by physical handover of the printed report; no SMS, e-mail, fax, portal, or mobile channel exists.
- Third-party libraries requiring outbound calls to external services are excluded from Infrastructure choices.
- Deployment topology does not require Internet connectivity at the site.

**Related.** ADR-0001, ADR-0003.

---

### ADR-0011 — Single `ApplicationDbContext`; one EF Core configuration per entity via Fluent API

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** With one shared database and many entities, persistence configuration must be organized so that entity mapping is discoverable and does not leak persistence concerns into Domain classes.

**Decision.**
- Exactly one `ApplicationDbContext` exists, matching the single shared SQL Server database.
- Every entity has exactly one configuration class named `<Entity>Configuration`, using the EF Core Fluent API.
- Data annotations for persistence are not placed on Domain entities.
- Migrations live in `TopLab.Infrastructure/Persistence/Migrations` and are applied only against the single shared database.

**Consequences.**
- Domain classes remain free of persistence attributes and references.
- All mapping decisions for an entity live in one file.
- No per-workstation database state or migration path exists.

**Related.** ADR-0003, ADR-0004, ADR-0005.

---

### ADR-0012 — Strongly-typed identifiers for entity IDs

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The system uses many distinct identifier concepts (patient identifier, laboratory identifier, test identifier, and others). Passing them as raw `int` values invites mix-ups that the compiler cannot catch.

**Decision.** Entities expose strongly-typed identifier value objects (for example `PatientId`, `LabId`, `TestId`) rather than raw primitives. EF Core value converters translate between the strong types and their underlying storage representation.

**Consequences.**
- Method signatures and command/query definitions self-document which identifier is expected.
- Accidental substitution of one identifier for another produces a compile error.
- Serialization boundaries (persistence, presentation) require the value-converter or explicit mapping to their underlying value.

**Related.** ADR-0011.

---

### ADR-0013 — Auditable-entity columns populated automatically by a SaveChanges interceptor

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Every mutable business entity must record who created it, who last modified it, when, and how many times. Requiring each handler to set these fields is unreliable because omissions are silent.

**Decision.** Entities requiring creation/modification tracking derive from a common `AuditableEntity` base carrying `CreatedByUserId`, `CreatedAtUtc`, `LastModifiedByUserId`, `LastModifiedAtUtc`, and `ModificationCount`. An `AuditableEntitySaveChangesInterceptor` in the Infrastructure layer populates these fields automatically at persistence time. Handlers never set them manually.

**Consequences.**
- Audit fields cannot be forgotten by an implementer and cannot be falsified by handler code.
- The restricted `P` inspection surface (registering user, modification count, most recent modifier) is satisfied directly from these columns with no duplicate storage.

**Related.** ADR-0011, ADR-0014.

---

### ADR-0014 — Per-test audit surface stored on `PatientTest`, restricted at the Application layer

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The `T` audit view exposes, per test, who entered, who reviewed, who was responsible for printing, how many times it was printed, and who delivered the result. The relationship of this data to the test is strictly one-to-one.

**Decision.** The `T` audit surface is stored directly as columns on the `PatientTest` entity (`EnteredByUserId/AtUtc`, `ReviewedByUserId/AtUtc`, `LastPrintedByUserId/PrintCount/AtUtc`, `DeliveredByUserId/AtUtc`). Access restriction to System Administrator or users holding Absolute Permissions is enforced at the Application layer through the authorization pipeline behavior, not by placing the columns in a separate physical table.

**Consequences.**
- No duplicate row must be kept in sync alongside the main record.
- The restricted `T` view is a set of dedicated queries whose authorization is enforced uniformly.
- Physical table separation is deliberately not used as a defense; application-level authorization is the single enforcement point.

**Related.** ADR-0009, ADR-0013, ADR-0015.

---

### ADR-0015 — Patient aggregate status computed as a Domain Service; never stored

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The seven-state patient-level status is derived from the earliest incomplete stage across all of a patient's analyses and the account condition. Storing it as an independent, editable field risks divergence between screens and reports.

**Decision.** Patient aggregate status is implemented as a stateless Domain Service (`PatientStatusCalculator`). Every screen or report that displays patient status obtains it through this single calculation, invoked via the Application layer. The status is never persisted as an independently maintained column.

**Consequences.**
- All workstations always show the same status for the same patient, computed from the same current data.
- Adding or changing a lifecycle stage requires one change in one Domain Service, not per-screen code changes.
- Any attempt to cache or independently maintain the status is non-conforming.

**Related.** ADR-0005, ADR-0016.

---

### ADR-0016 — Financial figures computed at query time; no stored running totals

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Account balances, inventory figures, and per-entity settlement values must be identical on every workstation. Storing running totals introduces the risk of workstation-specific caching drift.

**Decision.** Running totals — patient balances, discounts, remaining-to-lab, remaining-to-patient, inventory aggregates, cash-drawer figures, and company/delegate balances — are computed at query time from primary records (`PatientTest`, `PaymentOperation`, `SentOutSample`, `SentOutSamplePayment`, `CashMovement`). No dedicated storage column or table maintains a pre-aggregated total.

**Consequences.**
- Every workstation always presents identical figures for the same time window.
- Query design must ensure the aggregation performs adequately under expected volumes; supporting indexes are placed accordingly.
- Corrections to underlying records are reflected in derived figures without a separate reconciliation step.

**Related.** ADR-0015, ADR-0017.

---

### ADR-0017 — Void-and-reissue instead of physical delete for payment operations

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Payment operations may need correction after they are recorded. Physically deleting them would erase evidence needed by the restricted `P` audit view.

**Decision.** Corrections to payment operations are made by marking the incorrect row as voided (`IsVoided = true`) and, where necessary, recording a new payment operation with the corrected values. Physical deletion is not used for `PaymentOperation`.

**Consequences.**
- The full history of financial activity remains queryable for the `P` view and inventory audits.
- Aggregation queries filter by `IsVoided = 0` when computing outstanding balances.
- The user interface for correcting a payment operation must implement void-and-reissue semantics, not row deletion.

**Related.** ADR-0013, ADR-0014, ADR-0016.

---

### ADR-0018 — Soft delete for records with audit relevance; hard delete only where none

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Some records must remain queryable after logical deletion because audit trails, historical reports, or restricted views still reference them. Others have no audit consequence and can be removed outright.

**Decision.** Records with audit relevance carry an `IsDeleted` boolean and are never physically removed; queries filter them out by default. Records without audit relevance may be hard-deleted, gated by the appropriate permission.

**Consequences.**
- Historical references remain resolvable for as long as required by the audit views.
- Query paths must apply the soft-delete filter consistently, ideally through EF Core global query filters.
- User-visible "delete" actions may correspond to either operation; the choice is a per-entity decision recorded on the entity itself.

**Related.** ADR-0011, ADR-0013.

---

### ADR-0019 — Single-language deployment; Arabic and English share the same text columns

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The product runs Arabic-first, and some free-text values may include English content. Introducing per-language columns would multiply schema surface without matching any product requirement for multi-language storage.

**Decision.** Text values are stored in `nvarchar` columns shared by Arabic and English content. No language discriminator column and no parallel localized column exist in the data model.

**Consequences.**
- Text collation and indexing are configured once per column, without language-scoped variants.
- Cross-language search behavior is limited to what a single `nvarchar` column supports.
- Introducing full multi-language storage would require a superseding ADR.

**Related.** ADR-0011.

---

### ADR-0020 — Configuration tables hold exactly one row, keyed by a fixed primary key value

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** System-wide settings (report, receipt, envelope, general system checkboxes) are logically singletons: only one active value exists per option.

**Decision.** Configuration tables containing system-wide settings are constrained to a single row by using a fixed primary key value of `1`. Their existence is guaranteed by seed data at first deployment.

**Consequences.**
- Access code reads the single row by its known primary key, never by a search.
- Attempting to insert additional rows fails at the database level.
- Adding a per-scope configuration surface (per-user, per-workstation) requires a superseding ADR.

**Related.** ADR-0011.

---

### ADR-0021 — Database connection settings are workstation-local, not database-stored

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** To reach the shared database, a workstation needs server name, login, and database name. These values are needed **before** a database connection exists, so they cannot live inside the database itself.

**Decision.** Database connection settings are stored in a local application configuration file on each workstation. They are excluded from the shared database schema.

**Consequences.**
- Workstations can differ in how they reach the shared database (different named servers, aliases) while agreeing on the same target database.
- Backup/restore of the database does not carry connection settings between workstations.
- A local configuration mechanism must be maintained by the Presentation layer at startup.

**Related.** ADR-0003, ADR-0020.

---

### ADR-0022 — Single-branch model; no branch-scoping in any layer

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** The product serves one physical site. Introducing a branch identifier prematurely would spread branch-awareness across the codebase for no product benefit.

**Decision.** No branch concept is introduced anywhere. No table carries a branch identifier; no query filters by branch; no screen offers a branch selector. Multi-branch support is out of scope.

**Consequences.**
- Every aggregation is a site-wide aggregation.
- Introducing multi-branch operation later requires a superseding ADR and coordinated schema and code changes across all layers.

**Related.** ADR-0003, ADR-0010.

---

### ADR-0023 — Shared kernel: one Domain project used across all features

- **Status:** Accepted
- **Date:** 2026-08-27

**Context.** Concepts such as patient, test, and result appear in many features. Duplicating them per-feature (as in strict Bounded Contexts) would allow the same real-world concept to diverge across screens.

**Decision.** A single Domain project (`TopLab.Domain`) hosts all entities, value objects, and domain services. Every feature that references a shared concept references the one canonical definition.

**Consequences.**
- Rules such as patient aggregate status precedence exist as one calculation reused everywhere.
- Feature teams do not create parallel definitions of the same concept.
- Cross-feature refactors of shared concepts are made in one place.

**Related.** ADR-0005, ADR-0015.

---

### ADR-0024 — Composition root may reference Infrastructure directly; reflection workaround removed

- **Status:** Accepted
- **Date:** 2026-08-29

**Context.** The composition-root wiring in `TopLab.Presentation` avoided a compile-time reference to `TopLab.Infrastructure` via `ReferenceOutputAssembly=false` on the `ProjectReference`, then loaded Infrastructure at runtime through `Assembly.Load("TopLab.Infrastructure")` and reflection-invoked `AddInfrastructure` by string name in `App.xaml.cs`, supported by a custom MSBuild target (`CopyInfrastructureRuntime`) that copied the DLL manually. This satisfied the letter of "Presentation never references Infrastructure" but hid a real dependency behind fragile, non-refactor-safe, runtime-only-failing string lookups. Two independent audits flagged this as blocker B-03. The Dependency Rule (Architecture §2.2, Coding Standards §3.1) stated Presentation references Application only, with no composition-root exception.

**Decision.** Adopt option O2 (owner-approved): the composition root is the one place in Clean Architecture that is expected to know all layers. `TopLab.Presentation` MAY reference `TopLab.Infrastructure` directly and exclusively inside the composition root (`App.xaml.cs` and its direct DI wiring call) for the purpose of dependency registration via a normal compile-time call `TopLab.Infrastructure.DependencyInjection.AddInfrastructure(services, configuration)`. Every other file in Presentation (ViewModels, Views, any other class) remains strictly forbidden from referencing Infrastructure. The reflection-based loading, string-based method lookup, `ReferenceOutputAssembly=false`, and `CopyInfrastructureRuntime` target are removed.

**Consequences.**
- `TopLab.Presentation.csproj` carries a normal `ProjectReference` to `TopLab.Infrastructure`; the DLL is copied automatically by MSBuild.
- `App.xaml.cs` calls `AddInfrastructure` directly; renames and signature changes are caught at compile time, not at runtime.
- The Dependency Rule in Architecture §2.2 and Coding Standards §3.1 is amended to carve out the composition-root exception with identical wording.

**Alternatives considered.**
- *Keep reflection (O1):* rejected — preserves runtime fragility, hides the real dependency without removing it, and defeats compile-time safety and IDE refactoring.
- *Separate Composition/Bootstrapper project (O3):* rejected — adds a fifth top-level project and indirection for a single wiring call; disproportionate to the problem when the well-established Clean Architecture convention already permits the composition root to know all layers.

**Related.** ADR-0005, Architecture §2.2, Coding Standards §3.1.

---

### ADR-0025 — Workstation connection settings stored under `%ProgramData%\TopLab` with a committed safe template

- **Status:** Accepted
- **Date:** 2026-08-30

**Context.** `appsettings.json` in `TopLab.Presentation` is the conventional workstation-local settings file, but it carries a real connection string and is therefore gitignored (`.gitignore`: `appsettings*.json`). The guard introduced for B-02 then left a clean clone of the repository unable to produce a working application configuration, and the effective per-machine settings lived only in the developer's working tree — invisible to CI, to other developers, and to a first-time operator. A second, machine-scoped store is needed for a desktop workload that must run before any database connection exists, so it cannot come from the database it describes (see ADR-0021).

**Decision.** Connection settings are stored in two distinct places, with one source of truth for runtime:

1. **Committed safe template** — `appsettings.example.json` in the Presentation project contains the default Integrated-Security form (`Server=(localdb)\mssqllocaldb;Database=TopLab;...`) with **no** password, is explicitly un-ignored in `.gitignore`, and is copied to the output directory. It documents the schema and the safe default without leaking credentials.
2. **Machine-scoped store** — the first-run setup wizard (`DatabaseSetupWindow`) validates a connection and writes the effective `ConnectionStrings:TopLab` value to `%ProgramData%\TopLab\appsettings.json`. The composition root registers that file with the configuration builder (optional at load time), and it takes precedence over the gitignored local `appsettings.json`. The personal per-developer `appsettings.json` remains supported for development but is never the distribution path.

Strongly-typed string identifiers (`LabId`) are stored through an EF Core value converter mapping to the existing `nvarchar(30)` column, so the data model is unchanged by the type migration (ADR-0012).

**Consequences.**
- A clean machine runs the setup wizard on first launch instead of crashing with a raw `InvalidOperationException`.
- `ConfigurationFileService` (Presentation) owns the store path and the JSON read/write contract; it is registered as a singleton in DI.
- `MigrateAsync` runs from the composition root after the host starts, behind a `try/catch` that surfaces a friendly Arabic message and shuts down cleanly if migration fails.

**Alternatives considered.**
- *User-profile store (`%LocalAppData%`):* rejected — settings must follow the workstation so every operator account on the same machine reaches the same database.
- *Encrypt the file with DPAPI at write time:* deferred — the store lives on a trusted workstation volume; DPAPI remains a candidate hardening step and is documented as such, not implemented here.

**Related.** ADR-0007 (close), ADR-0012, ADR-0021, Architecture §2.2/§11, Coding Standards §3.1/§10.

---

### ADR-0026 — M17: User & Permission Management security, floor and provisioning

- **Status:** Accepted
- **Date:** 2026-09-01

**Context.** The `Users`, `Permissions`, and `UserPermissionGrants` tables and the `AuthorizationBehavior` pipeline existed from F5 but had no authentication, no password hashing, no user-management surface, and no first-run bootstrap. M17 had to deliver the complete backbone without a new migration, without an external identity package (ADR-0010), without a seeded credential, and without a database trigger.

**Decision.**

1. **Password hashing — PBKDF2-SHA256 on the .NET BCL only.** Both the main and the secondary (internal windows) passwords are stored only as self-describing strings `PBKDF2-SHA256$<iterations>$<base64-salt>$<base64-hash>` inside the existing `nvarchar(300)` columns (`PasswordHash`, `InternalWindowsPasswordHash`). Parameters: PBKDF2 with HMAC-SHA256 via `Rfc2898DeriveBytes`, minimum 100,000 iterations, 128-bit cryptographically random salt per hash, 256-bit derived key, constant-time verification via `CryptographicOperations.FixedTimeEquals`. The storage format is self-describing so iteration counts can be raised without a schema change; total length ≈ 120–160 characters fits within `nvarchar(300)`. No NuGet package is added; implementation lives in `Infrastructure/Identity/Pbkdf2PasswordHasher` behind the `IPasswordHasher` port.

2. **Application-layer-only last-active-absolute-user floor.** The invariant "at least one active absolute-permission user must exist" is enforced as a hard refusal in the Application layer only (`Error.Conflict("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً")`) on every path that could violate it: demote (clear `IsAbsolutePermission`), deactivate, and physical delete. No database constraint, trigger, or filtered index is introduced.

3. **Guarded physical delete.** A user is physically removed only when that `UserId` has zero references anywhere in audit-relevant data (`CreatedByUserId`/`LastModifiedByUserId` on all auditable sets plus `PaymentOperation.ReceivedByUserId`, `CashMovement.PerformedByUserId`, `PatientTest` lifecycle columns, and `AttendanceRecord.UserId`). If any reference exists the operation is refused with `Error.Conflict("لا يمكن حذف مستخدم له سجلات مرتبطة؛ استخدم التعطيل بدلاً من الحذف")` and deactivation is offered instead. No `IsDeleted` column is added; deactivation (`IsActive = false`) is the soft path.

4. **First-run interactive administrator provisioning with no shipped credential.** No seed row, no factory password literal, no hard-coded hash exists in source, tests, or migrations. On startup after `MigrateAsync`, the composition root dispatches `HasAnyAbsoluteUserQuery`; when no active absolute user exists it shows `FirstRunAdminWindow` (collecting username, main password + confirmation, secondary password + confirmation, dispatching `CreateUserCommand` with `isAbsolute: true`) before `MainWindow`. Exiting without creating an administrator shuts the application down. When an active absolute user already exists the wizard never appears. The only documented recovery for a lost administrator is a manual SQL procedure (generate a PBKDF2-SHA256 hash with the same parameters and update `PasswordHash` directly); no break-glass or in-product recovery key is introduced.

5. **Secondary-password gate and permission catalog handling.** The shared "System menu password" dialog is implemented once in `DialogService.ShowSecondaryPasswordDialogAsync` and dispatches `VerifySecondaryPasswordQuery` against the current session user's own `InternalWindowsPasswordHash`. The Users screen is the first consumer; the dialog is reusable for later modules. The permission catalog is fixed at the thirteen seed rows; the audit-access grant (`PT_AUDIT_ACCESS`) is hidden/disabled for limited-mode users at the Presentation surface while the pipeline honors a present grant as defense-in-depth. Password fields on the management screen are write-only.

**Consequences.**
- Credential material never leaves the write path: queries return no hash material, DTOs contain no hash members, and edit-form password fields are always empty on load.
- Sign-in failures are uniform (`Forbidden` with "اسم المستخدم أو كلمة المرور غير صحيحة") for unknown user and wrong password; inactive users receive "المستخدم غير مفعل"; no account lockout, no session timeout, no workstation or time-window restriction is introduced.
- Permission and grant changes take effect at the affected user's next login only; the `ICurrentUserService` singleton is populated at sign-in.
- Deployment on a fresh database provisions the first administrator interactively; deployment on an existing database skips the wizard. No credential is documented because none exists.

**Related.** ADR-0009, ADR-0010, ADR-0013, M17 Implementation Plan S1–S6.

---

### ADR-0027 — M22: workstation-local lab identification text and font storage; no images, no colors in print configuration

- **Status:** Accepted
- **Date:** 2026-09-02

**Context.** Report, receipt, and envelope settings surface lab identification text (name, address, phone) and a font family/size for the printed subjects. M22 keeps configuration workstation-local where it concerns machine-specific artifacts (ADR-0021) and must not introduce schema churn, binary assets, or NuGet packages.

**Decision.**

1. **Lab print text is workstation-local, file-backed, per scope.** The lab name/address/phone and chosen font are persisted to a JSON file (`lab-print-text.json` under the workstation configuration location) via the `ILabPrintTextStore` port, keyed by `LabPrintTextScope` (Report / Receipt / Envelope). The single system-wide `LabPrintTextDto` carries `LabName`, `Address`, `Phone`, `FontFamily`, `FontSizePt`. This keeps the business database unchanged (no migration, no new tables) and matches ADR-0021's locality rationale for machine-specific print output. `SaveLabPrintTextCommand` / `GetLabPrintTextQuery` are permission-gated like the other setting writes.

2. **No images and no color controls in the implemented print configuration.** The PRD's optional image-based header/footer and header/footer color editing (FR-M22-004/005) are deliberately excluded from this delivery. Text/font configuration replaces them; no image picker and no color picker appear anywhere in the system-report/receipt/envelope screens, and the DTOs carry no image or color members.

3. **Envelope alignment is data, not drawings.** Envelope item alignment (`EnvelopePrintItemPosition`) is configured as four persisted rows (Name, Code, ReferralEntity, Date) with enable and Left/Top offset-cm values; the barcode preview on the envelope screen is a static placeholder rectangle with no live rendering dependency.

**Consequences.**

- Report/receipt/envelope provisioning requires no migration; print text and fonts travel only on the workstation that printed them.
- The excluded image/color capabilities remain documented in the PRD; a future decision can introduce them without changing the database schema or the lab-text DTO shape (they would only add fields).
- A single `EDIT_SYSTEM_SETTINGS` permission guards the entire settings surface; the Database Maintenance window additionally requires the secondary-password gate reused from M17 (`DialogService.ShowSecondaryPasswordDialogAsync`).

**Related.** ADR-0020, ADR-0021, ADR-0025, M22 Implementation Plan S1–S8.

---

## 3. Reserved Ranges for Future Decisions

- **ADR-0100 – 0199** — reserved for reporting/printing infrastructure decisions.
- **ADR-0200 – 0299** — reserved for future security and identity-related decisions.
- **ADR-0300 – 0399** — reserved for future test-strategy and quality-gate decisions.

Adding an ADR in a reserved range does not require reorganizing the log; sequential allocation may continue at the current tail.

---

*End of document.*
