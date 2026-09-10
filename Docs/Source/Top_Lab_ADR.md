
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

### ADR-0028 — Add user-facing `Tests.TestCode` column for FR-M12-001 "test number" search

- **Status:** Accepted
- **Date:** 2026-09-06

**Context.** FR-M12-001 requires searchability by test name, containing group, or test number. "Test number" is a user-entered, display-facing, stable identifier distinct from the internal `TestId` surrogate. No existing column satisfies this; reusing `TestId` is rejected because it is internal and not display-stable across data imports.

**Decision.** Add `Tests.TestCode nvarchar(50) NOT NULL` with a unique index `IX_Tests_TestCode` using the database default collation (`SQL_Latin1_General_CP1_CI_AS`, case-insensitive — same precedent as `IX_Users_UserName`). Domain enforces required + trimmed; Application validator enforces max-length 50; DB enforces uniqueness.

**Consequences.** One new EF migration; the F5 baseline tables are otherwise unchanged; search by `TestCode` is exact-match (codes are unique); M-04 (or any downstream module) may display `TestCode` on receipts and reports. M-12 does not change `PatientTest` or any other downstream schema.

**Related.** M12 Implementation Plan §5, F5 baseline.

---

### ADR-0029 — Add `IsActive` lifecycle field to `Tests` and `TestGroups` with Deactivate/Reactivate write surface and cascading deactivation

- **Status:** Accepted
- **Date:** 2026-09-06

**Context.** The module requires a way to retire tests and test groups from active use without destroying historical data. Hard delete is rejected because it would orphan reference ranges, work-group log items, and future patient-test rows. A boolean lifecycle flag is the simplest mechanism that satisfies this need.

**Decision.** Add `Tests.IsActive bit NOT NULL DEFAULT 1` and `TestGroups.IsActive bit NOT NULL DEFAULT 1`. The write surface exposes `DeactivateTest`/`ReactivateTest` and `DeactivateTestGroup`/`ReactivateTestGroup` commands. No hard delete of Test or TestGroup exists in M-12. **Deactivating a TestGroup cascades atomically to all member Tests** — the handler loads every Test with `TestGroupId == group.Id` and sets `IsActive = false` on each, within a single `SaveChangesAsync` call (single transaction). **Reactivating a TestGroup does NOT cascade** — member Tests retain their current `IsActive` state and must be individually reactivated if desired. This asymmetry is deliberate: deactivation cascading ensures a group and its contents are retired together (no orphaned active tests under an inactive group); non-cascading reactivation prevents inadvertently reactivating tests that were individually deprecated, pending review, or temporarily unavailable. Read-side queries filter by `IsActive = true` by default; an `IncludeInactive` parameter explicitly overrides this.

**Consequences.** The migration adds two columns with a `DEFAULT 1` constraint (existing rows become active). The `DeactivateTestGroupCommandHandler` must query `_db.Set<Test>().Where(t => t.TestGroupId == groupId && t.IsActive)` to find affected tests, then call `Deactivate()` on each and add/update them. The read surface must pass `IncludeInactive` correctly. The Presentation layer (future) uses the flag to grey-out or hide inactive records in catalog and group lists. Reactivation is always possible per-test.

**Baseline observation (waiver for M-12).** `WorkGroupLogItem` has no FK relationship to `Test` in the F5 baseline — `WorkGroupLogItemConfiguration` deliberately suppresses the relationship, so no `Test → WorkGroupLogItem` cascade constraint exists. M-12 offers no hard delete of tests (soft-deactivate instead), so the gap is inert for this module; adding the FK would exceed the locked migration scope (§5.5), so M-12 records the gap and leaves it to a future module.

**Related.** ADR-0018 (soft delete for audit-relevant records), M12 Implementation Plan §5.

---

### ADR-0030 — M-13: Price Lists, Test Comments, and Custom Groups — behaviors, item-mutation protocol, and zero-migration outcome

- **Status:** Accepted
- **Date:** 2026-09-07

**Context.** Module 13 implements the three reference-data concepts (price lists, fixed test comments, custom groups) on top of the M-12 catalog and the M-14 `ExternalEntity` contract. The four entities (`PriceList`, `PriceListItem`, `CustomGroup`, `CustomGroupItem`, `TestComment`) shipped as inert Create-only shells in the F5 baseline; they now need maintenance behaviors (rename, item add/update/remove, price and length guards). The Application port has no `Include` capability, so EF cannot populate aggregate `_items` collections through the interface — this constrains the item-mutation mechanics. Two further constraints are explicit: (a) price-list delete must guard against silently nulling a live `ReferralOrContract` entity's price list (the DB-level `SetNull` would do that, but it would manufacture domain-invalid entities and silently reprice future patients); (b) the schema is already complete from the F5 baseline, so no new migration is expected.

**Decision.**

1. **Domain behaviors.** `PriceList` and `CustomGroup` gain `Rename`, `ContainsTest`, `AddItem(testId, price)` (duplicate → `ArgumentException("Test already exists in the price list.", nameof(testId))`, negative price → `ArgumentException("Price must be >= 0.", nameof(price))`), `SetItemPrice(testId, price)` (upsert), `RemoveItem(testId)` (absent → `ArgumentException("Test is not in the price list.", nameof(testId))`). `PriceListItem` and `CustomGroupItem` gain the `price >= 0` constructor guard and `UpdatePrice`. `TestComment` gains `public const int MaxCommentTextLength = 1000`, the length guard in `Create` and `Update`, and an `Update` mutator. Guards raise `ArgumentException` with `paramName` set — the M14-style `DomainFailureTranslator` binds to `paramName` + message-fragment to produce the frozen Arabic messages.

2. **Item-mutation mechanics — aggregate-as-invariant-checker + flat-set persistence.** The Application port has no `Include` (verified). The M12 precedent (`SaveWorkGroupLogItemsCommandHandler`) persists flat-set rows explicitly. M-13 mandates the same pattern, sharpened to avoid the **double-tracking hazard**: handlers (a) load the header via `Set<T>()`; (b) existence-check the `TestId` via `Set<Test>()`; (c) pre-check the flat row via `Set<PriceListItem>()` / `Set<CustomGroupItem>()`; (d) **call the aggregate method purely for invariant enforcement inside try/catch (ArgumentException) → translator** — the aggregate's in-memory `_items` mutation is intentionally NOT persisted because the aggregate instance is loaded fresh per handler invocation and its `_items` is always empty when first loaded; (e) persist idempotently against the flat set only (fresh `PriceListItem`/`CustomGroupItem` constructor for Add; tracked flat row from step (c) for Update and Remove). `RemoveItem` handlers do NOT call the aggregate's `RemoveItem` (the aggregate's `_items` would be empty, so the call would throw spuriously; the pre-check on the flat set is the invariant). `PriceListItem.UpdatePrice` and `CustomGroupItem.UpdatePrice` are `public` (the `price >= 0` invariant is enforced inside) so handlers can mutate flat rows directly.

3. **Price-list delete is blocked while referenced.** `DeletePriceListCommandHandler` checks `Set<ExternalEntity>().Any(e => e.PriceListId != null && e.PriceListId.Value == request.Id)` and returns `Conflict("تعذر حذف قائمة الأسعار لارتباطها بجهات خارجية.")` before removal. A `SaveChangesAsync` `try/catch` re-maps a race-condition `IsReferenceConflict(ex)` to the same `Conflict`. Custom-group delete has no such guard (no FK target). Items cascade with their list/group at the DB level (FK Cascade, verified).

4. **Permission reuse.** All 13 M-13 write commands carry `IAuthorizedRequest` with `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` (consistent with M12/M14); reads are unauthorized plain `IRequest<Result<...>>`.

5. **Multiple comments per test.** `CreateTestCommentCommand` performs no uniqueness check (the reference system explicitly allows it: *"ويمكن إضافة أكثر من كومنت لنفس التحليل"*).

6. **No new migration.** All five M-13 tables (`PriceLists`, `PriceListItems`, `CustomGroups`, `CustomGroupItems`, `TestComments`) and the `ExternalEntities.PriceListId` column already exist from the F5 baseline migration `20260828052248_BaselineDataModel.cs` (verified line-level). The model-vs-snapshot zero-drift gate in `F5ConfigurationTests` + `PriceListCustomGroupDeleteBehaviorTests` + `PriceListItemPersistenceTests` confirms no drift; `ApplicationDbContextModelSnapshot.cs` is unchanged.

7. **`TestComment.MaxCommentTextLength = 1000` constant.** Mirrors `ReferenceRange.MaxCommentLength` / `Test.MaxTestCodeLength` precedent — the column-level limit is now also enforced in the domain.

8. **`RemoveItem`-absent throws.** Aligned to the verified `WorkGroupLog.RemoveItem` precedent (not the M-13 initial draft's no-op, corrected per plan §0.1.2). Handlers translate the absence case to `Error.NotFound` before calling the domain method, so the throw is a safety net only.

**Consequences.** Diff is confined to `src/TopLab.Domain/{Billing,Tests}/**`, `src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/**`, `tests/**`, `Docs/**`. No Presentation content anywhere. The FK matrix is pinned by `PriceListCustomGroupDeleteBehaviorTests` (Cascade for `PriceListItem→PriceList` and `CustomGroupItem→CustomGroup` with `Items` navigation; SetNull for `ExternalEntity→PriceList`; negative assertions that `PriceListItem→Test` and `CustomGroupItem→Test` do not exist). The orphan-risk note for the absence of the latter two FKs carries to the M12 (if a future test hard-delete is ever introduced) and M02 owners. 13 new validators resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (no DI wiring change). `ValidatorRegistrationTests` covers all 13. The aggregate method is no longer authoritative for item-row state — the flat set is. The plan's A9 grep gate (`\b\.Items\b` references in handlers) is clean.

**Related.** M-13 Implementation Plan §3, §5, §7, §9; ADR-0011 (one config per entity via Fluent API); ADR-0018 (no soft-delete for items — items cascade with their header).

### ADR-0031 — M-15: Culture & Antibiotic Configuration — culture-ness via Test.IsCultureType, application-level attach integrity, and zero-migration outcome

- **Status:** Accepted
- **Date:** 2026-09-07

**Context.** Module 15 implements the maintenance and read surface for two reference-data concepts: antibiotics as a global catalog, and the per-culture antibiotic attachment. Both concepts already have all required tables/columns/FKs from the F5 baseline (`Antibiotics`, `CultureAntibioticAttachments`, `CultureAntibioticResults` — verified line-level). The reference system's culture-entry mechanics (fixed ID slots 118–139 + file-copy at `D:\real lab system\Data`, §8-3 of the reference PDF) are **explicitly rejected as legacy** — the user-extensible, in-app culture test under the `CULTURE AND SENSITIVITY` group is the rule content, matching PRD FR-M13-001 / FR-M15-001. Antibiotic delete must respect both an application-level attachment check (no DB FK from `CultureAntibioticAttachment` exists to either `Test` or `Antibiotic` — verified) and a DB-level `Restrict` from `CultureAntibioticResult` to `Antibiotic` (forward-safe — M06 data cannot exist yet). The display filter at result entry (BR-12) is the M06 consumer's concern; M-15 ships the pure contract.

**Decision.**

1. **Culture-ness via `Test.IsCultureType` (no schema change).** A culture is a catalog test created through M-12's `CreateTestCommand` with `isCultureType = true`. The flag is **create-time-only** settable (verified — `Test.Update(...)` has no `isCultureType` parameter). M-15 does **not** reopen catalog CRUD; it only **reads** the catalog to enforce culture-ness on attach and to expose culture-attachment listings. The reference's fixed-ID-slot (118–139) and file-copy mechanics are explicitly rejected as legacy. A mis-flagged test must be deactivated and recreated via M-12's lifecycle.

2. **Application-level attach integrity (no DB FK).** `CultureAntibioticAttachment` has a composite PK `{TestId, AntibioticId}` but **no FK to either `Test` or `Antibiotic`** (verified in the snapshot, in the migration, and in the EF configuration; the configuration file's comment states "FK via convention (removed explicit HasOne to avoid shadow)"). M-15's handlers are the sole enforcement point: every attach/detach operation existence-checks both `TestId` and `AntibioticId`, and the `AttachAntibioticToCultureCommandHandler` enforces `IsCultureType == true` on the target test (otherwise `Validation("التحليل المحدد ليس مزرعة.")`). Duplicate attachment returns `Conflict`. The missing FKs are compensated by the negative model assertions in `CultureAntibioticDeleteBehaviorTests` (`CultureAntibioticAttachment_HasNoRelationshipToTest`, `CultureAntibioticAttachment_HasNoRelationshipToAntibiotic`) so any silent schema change is caught. The orphan-risk note carries to the M-12 owner (if a future test hard-delete is introduced) and the M-06 owner (consumer of attachment rows).

3. **Antibiotic delete guards.** Blocked with `Conflict("تعذر حذف المضاد الحيوي لارتباطه بمزرعة.")` when `CultureAntibioticAttachment` rows exist for the antibiotic (application-level check); blocked with `Conflict("تعذر حذف المضاد الحيوي لوجود نتائج مسجلة به.")` when `CultureAntibioticResult` rows exist (DB-level `Restrict` FK; the application-level check is forward-safe because M-06 data cannot exist yet). Save-time `IsReferenceConflict(ex)` catch (M14 `DeleteExternalEntityCommandHandler` precedent — message contains `"REFERENCE"` or `"conflicted"`) maps to the results-message `Conflict`.

4. **Antibiotic flag editability (confirmed D4-a).** `Antibiotic.Update(string name, bool isPregnancyFlagged, bool isChildrenFlagged)` mutates the name (with the same guard as `Create`) and the flags freely. A mis-set flag is corrected in place; existing attachments and results survive the correction. This matches the reference system's display of the flags as ordinary checkboxes on the antibiotic data form with no immutability indication (§9-3 of the reference PDF).

5. **Display-filter contract (BR-12): pure static resolver in Application-Common.** `CultureAntibioticDisplay` (a static class in `src/TopLab.Application/Features/CultureAndAntibiotics/Common/`) ships the union-semantics decision (confirmed D5-a): `public const int ChildAgeThresholdYears = 12;` and `public static bool IsDisplayable(bool isPregnancyFlagged, bool isChildrenFlagged, bool isPregnancyIndicated, bool isChildUnder12)` returning `(!isPregnancyFlagged && !isChildrenFlagged) || (isPregnancyFlagged && isPregnancyIndicated) || (isChildrenFlagged && isChildUnder12)`. Unflagged = displayable for all patients; single flag = displayable only when that condition holds; both flags = displayable when **either** condition holds (union). The resolver is **pure** (no `IDateTimeProvider`, no `Patient` entity — M06 supplies the booleans from the patient context). The 12-year threshold is a named constant per reference §9-3 and PRD FR-M15-004. The 16-row truth-table test in `CultureAntibioticDisplayTests` pins the semantics. The placement in Application-Common follows the verified M14 `ReferralNameResolver` precedent (a static class in the feature's `Common` folder).

6. **Manual-entry attach flow is a two-command sequence (confirmed D6-a — no composite command).** The reference's manual-entry path (§9-3 — type antibiotic data into the form, then Save) is served by `CreateAntibioticCommand` followed by `AttachAntibioticToCultureCommand`. **No `CreateAndAttachAntibiotic` command is created**; the conditional row in the plan's M15-S2 file table is cancelled. The created-but-unattached intermediate state is harmless, visible, and retryable. The write-command count is fixed at **5** (Create / Update / Delete Antibiotic + Attach / Detach Antibiotic).

7. **Permission reuse.** All 5 M-15 write commands carry `IAuthorizedRequest` with `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` (consistent with M12/M13/M14); reads are unauthorized plain `IRequest<Result<...>>`.

8. **No new migration.** All three M-15 tables (`Antibiotics`, `CultureAntibioticAttachments`, `CultureAntibioticResults`) already exist from the F5 baseline migration `20260828052248_BaselineDataModel.cs` (verified line-level — line 17, 32, 758). The model-vs-snapshot zero-drift gate in `F5ConfigurationTests` (`Antibiotic_HasExpectedMapping`, `CultureAntibioticAttachment_HasCompositeKey`) + `CultureAntibioticDeleteBehaviorTests` (FK matrix incl. 2 negative no-FK assertions) confirms no drift; `ApplicationDbContextModelSnapshot.cs` is unchanged.

**Consequences.** Diff is confined to `src/TopLab.Domain/Tests/Antibiotic.cs` (+`Update`), `src/TopLab.Application/Features/CultureAndAntibiotics/**` (new feature folder: `Common/CultureAntibioticDisplay` + `Common/AntibioticDtos` + `Common/DomainFailureTranslator` + 2 queries + 5 commands), `tests/**` (Domain + Application + Infrastructure + Fake extension), `Docs/**` (ADR, tracking-sheet flip, handoff). **No Presentation content anywhere** (A6 grep gate). 5 new validators resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (no DI wiring change); `ValidatorRegistrationTests` covers all 5. 16-row truth table pins BR-12 semantics. The FK-matrix tests pin the verified delete-behavior matrix (`CultureAntibioticResult → Antibiotic` Restrict; `CultureAntibioticResult → CultureResult` Cascade; `CultureResult → PatientTest` 1-to-1 Cascade; **two negative no-FK assertions on `CultureAntibioticAttachment`**). The plan's coverage floors (Domain ≥ 90%, Application ≥ 80%, Infrastructure ≥ 70% of the M-15 footprint) are met without waivers.

**Related.** M-15 Implementation Plan §3, §5, §7, §9; ADR-0011 (one config per entity via Fluent API); ADR-0028 (TestCode); ADR-0029 (Test/TestGroup lifecycle + IsActive); ADR-0030 (M-13 zero-migration precedent).

---

## 3. Reserved Ranges for Future Decisions

- **ADR-0100 – 0199** — reserved for reporting/printing infrastructure decisions.
- **ADR-0200 – 0299** — reserved for future security and identity-related decisions.
- **ADR-0300 – 0399** — reserved for future test-strategy and quality-gate decisions.

Adding an ADR in a reserved range does not require reorganizing the log; sequential allocation may continue at the current tail.

---

### ADR-0032 — M-02: Patient Registration permission reuse, M-02 → M-13 integration via TestPriceResolver + FakeSender, and per-test sample flags

- **Status:** Accepted
- **Date:** 2026-09-07

**Context.** Module 2 implements the patient registration and test-ordering backend (Domain + Application + Infrastructure; no Presentation). M-02's write surface depends on M-13's read surface for price-list lookups (`GetPriceListByIdQuery`) and custom-group lookups (`GetCustomGroupByIdQuery`). Three architecture-level decisions shape M-02: (a) the permission gate for all M-02 writes, (b) where the per-test pricing algorithm lives, and (c) how M-02 handler tests exercise the M-02 → M-13 MediatR integration. Each is settled as a closed decision rather than as an open point and each lands cleanly into the existing codebase patterns (M12/M14/M17/M22).

**Decision.**

1. **Permission gate re-use — no new seed row.** All M-02 write commands (`CreatePatientCommand`, `UpdatePatientCommand`, `AddMedicalConditionCommand`, `RemoveMedicalConditionCommand`, `AddTestsToVisitCommand`, `AddCustomGroupToVisitCommand`, `RemoveTestFromVisitCommand`, `UpdatePatientTestSampleFlagsCommand`, `ClearAllTestsCommand`) carry `IAuthorizedRequest` with `RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient` (the literal `"ADD_EDIT_PATIENT"`). `SoftDeletePatientCommand` carries `RequiredPermissionCode => PatientRegistrationAccessPolicy.DeletePatient` (the literal `"DELETE_PATIENT"`). Both codes are already seeded in `src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` at IDs 1 and 9 respectively (the 13-row catalog verified at the M-02 plan's reference commit and unchanged in this slice — the `git diff` against `PermissionConfiguration.cs` is empty). The two `PatientRegistrationAccessPolicy` constants (`AddEditPatient` and `DeletePatient`) centralize the magic strings once in `src/TopLab.Application/Features/PatientRegistration/Common/PatientRegistrationAccessPolicy.cs`, mirroring the M12/M14/M22 implicit magic-string pattern. The settling validates the M17 permission seed design — no new row is added for M-02, M-13, M-15, or any future module that reuses an existing code.

2. **`TestPriceResolver` placement in M-02's `Common` folder (`internal`).** The per-test pricing algorithm lives in `src/TopLab.Application/Features/PatientRegistration/Common/TestPriceResolver.cs` as a pure `internal static` class. Both `AddTestsToVisitCommandHandler` and `AddCustomGroupToVisitCommandHandler` delegate to it. The algorithm branches, in order: (i) if the patient has a `ReferralEntityId` whose `PriceListId` is set and the price list contains the requested test, use the price-list price; (ii) if the account type is `LabToLab` and `Test.LabToLabPrice` is set, use it; (iii) if a custom-group fallback price is supplied (only in `AddCustomGroupToVisit`), use it; (iv) fall back to `Test.PatientPrice`. The handler does the existence check before calling the resolver for branch (i) — when the price list does not contain the requested test for a contract referral, the handler returns `Error.Conflict("التحليل غير موجود في قائمة أسعار الجهة المحال منها.")` instead of letting the resolver misprice. The resolver is pure (no DB, no clock, no MediatR), unit-testable in isolation with 8 branches (`TestPriceResolverTests`), and `internal` to keep the API surface small — `InternalsVisibleTo("TopLab.Application.Tests")` is set in `TopLab.Application.csproj`. The placement in `Common` (not `Commands/AddTestsToVisit` or `Commands/AddCustomGroupToVisit`) follows the verified `ReferralNameResolver` (M14) and `CultureAntibioticDisplay` (M15) precedent: feature-local pure helpers live in `Common/`.

3. **`FakeSender` test seam — intra-Application MediatR shim.** M-02 handler tests exercise the M-02 → M-13 MediatR integration through `tests/TopLab.Application.Tests/Common/Fakes/FakeSender.cs`, a hand-rolled `ISender` shim that returns canned M-13 query results. `FakeSender.WithPriceList(int id, PriceListDetailDto dto)` and `FakeSender.WithCustomGroup(int id, CustomGroupDetailDto dto)` register the canned responses; the `Send<TResponse>` overload pattern-matches on `GetPriceListByIdQuery` and `GetCustomGroupByIdQuery` and returns the canned `Result<PriceListDetailDto>` / `Result<CustomGroupDetailDto>` (or `Result.Failure(Error.NotFound(...))` when the id was not registered). The shim implements the full `ISender` interface including the generic `Send<TRequest>(TRequest)` for `IRequest`-typed requests (used by `GetSystemSettingsQuery` in `CreatePatientCommandHandler`) via the `_otherResponses` dictionary. The shim is a small test utility — one file, one class, no Application code change. It does not stand up a real `IApplicationDbContext` for M-13 read paths; it is the seam. Production code is unaffected. The seam's existence records a precedent: any future Application module that needs to test intra-Application MediatR can copy this pattern.

4. **Per-test sample flags (settled OD-6).** `PatientTest` carries six independent booleans (`IsUrine`, `IsStool`, `IsBlood`, `IsSemen`, `IsCsf`, `IsTakenOutsideLab`) and a per-row `MarkSampleDrawn(DateTime)` mutator. The flags are *per test*, not per visit — faithful to the reference system's "خانة التحليل المسحوب خارج المعمل أسفل قائمة التحاليل" wording. `AddTestsToVisitCommand` carries the flags per input test (via `AddTestInput`); `UpdatePatientTestSampleFlagsCommand` provides the post-add edit path. M-21's `MarkSampleDrawnCommand` (in M-21's own plan) rejects re-drawing an outside-drawn test with the message `"تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب"`. The Domain `PatientTest.UpdateSampleFlags` is a pure state-setter (no guards beyond identity) added in M-02-S1.

5. **Contract-with-price-list-missing-test rule.** When the patient has a `ReferralEntityId` whose `PriceListId` is set but the price list does not contain the requested test, `AddTestsToVisitCommandHandler` returns `Error.Conflict("التحليل غير موجود في قائمة أسعار الجهة المحال منها.")` — a referral/contract entity must have every requested test in its price list, otherwise the lab is silently repricing the contract. The same rule applies to `AddCustomGroupToVisitCommandHandler` (group items not in the contract price list trigger the same `Conflict`). The presence of the rule pins the FR-M02-006 contract-pricing intent at the Application layer; the Domain aggregate `ExternalEntity` (M14) already enforces the inverse rule (TreatingDoctor must not have a `PriceListId`; ReferralOrContract must have one) at entity creation.

**Consequences.** Diff is confined to `src/TopLab.Domain/Patients/**`, `src/TopLab.Domain/Results/PatientTest.cs` (M-02-S1), `src/TopLab.Application/Features/PatientRegistration/**` (10 commands, 6 queries, 1 DTO file, 1 access-policy file, 1 `TestPriceResolver`), `src/TopLab.Infrastructure/Persistence/Configurations/{Patient,PatientTest}Configuration.cs` + new migration `20260907_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs` (M-02-S1), `tests/**`, `Docs/Source/Top_Lab_ADR.md` (this entry). **No `PermissionConfiguration` change** — settled OD-8 grep gate: `git diff src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` is empty. **No Presentation content anywhere**. 10 new validators resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (no DI wiring change); `ValidatorRegistrationTests` covers all 10. The `FakeSender` seam is the M-02 → M-13 integration testing pattern; the `TestPriceResolver` is the single source of truth for the per-test pricing algorithm; both `AddTestsToVisit` and `AddCustomGroupToVisit` delegate to the resolver. The 8 `TestPriceResolverTests` cases pin every branch (cash, lab-to-lab with/without `LabToLabPrice`, contract with price-list hit, contract missing price-list case, free, custom-group without contract, custom-group-with-contract price-list-wins).

**Related.** M-02 Implementation Plan §5.3 + §3.1; ADR-0026 (M17 permission seeding); ADR-0030 (M13 zero-migration outcome + permission reuse + frozen Arabic message table); ADR-0031 (M15 zero-migration outcome + permission reuse); ADR-0018 (no soft-delete for items — items cascade with their header); ADR-0028/0029 (M12 test catalog decisions).

---

### ADR-0033 — M-21: Sample Collection permission reuse (no new seed row)

- **Status:** Accepted
- **Date:** 2026-09-08

**Context.** Module 21 implements the sample collection & separation backend (Application layer only; no Presentation). The M21 surface is two unauthorized queries (`GetPatientsWithUncollectedSamplesQuery`, `GetPatientTestsForDrawQuery`) and two write commands (`MarkSampleDrawnCommand`, `MarkAllSamplesDrawnForPatientCommand`). The permission gate for the two writes is settled OD-8: reuse the existing `ADD_EDIT_PATIENT` code rather than introducing a new one.

**Decision.**

1. **Permission gate re-use — no new seed row.** Both M-21 write commands carry `IAuthorizedRequest` with `RequiredPermissionCode => SampleCollectionAccessPolicy.AddEditPatient` (the literal `"ADD_EDIT_PATIENT"`, id 1 in the 13-row catalog seeded in `src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs`, unchanged — the `git diff` against `PermissionConfiguration.cs` is empty). The phlebotomist's draw-marking write is the same operational write as the registrator's, so the M02 code applies directly.
2. **Own access-policy constant, mirrored shape — no cross-feature import.** M21 ships `src/TopLab.Application/Features/SampleCollection/Common/SampleCollectionAccessPolicy.cs` (`public const string AddEditPatient = "ADD_EDIT_PATIENT";`) mirroring the M02 `PatientRegistrationAccessPolicy` shape. M21 does not import M02's class; the per-feature access-policy convention (one constant holder per feature) is preserved.
3. **Outside-drawn tests are read-only from the M21 screen (FR-M21-001).** `MarkSampleDrawnCommandHandler` rejects an `IsTakenOutsideLab` row with `Error.Conflict("تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب")`, and `GetPatientsWithUncollectedSamplesQueryHandler` excludes outside-drawn rows from the un-drawn list. The message string is frozen by `MarkSampleDrawnCommandHandlerTests.OutsideDrawnTest_ReturnsConflict_WithSpecificMessage_FR_M21_001`.

**Consequences.** Diff is confined to `src/TopLab.Application/Features/SampleCollection/**` (2 DTO/query files in `Common`, 2 queries, 2 commands, 5 validators), `tests/TopLab.Application.Tests/Features/SampleCollection/**` (5 test files, 20 tests), and `Docs/**` (this entry, the tracking-sheet flip, `Handoff_M21.md`). **No Domain change, no Infrastructure change, no migration, no DI wiring change** (validators resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()`), **no Presentation content anywhere**. The 20 new tests pin every gate: the FR-M21-001 invariant on both the query and the command, the idempotent double-draw, and the exactly-once save of `MarkAllSamplesDrawnForPatientCommandHandler`.

**Related.** M-21 Implementation Plan §5.1 + §3.1; ADR-0032 (M02 permission-gate re-use of `ADD_EDIT_PATIENT`/`DELETE_PATIENT`); ADR-0026 (M17 permission seeding); ADR-0030/ADR-0031 (zero-migration + permission-reuse precedent).

---

### ADR-0034 — M-03: Patient Billing balance formula, void-and-reissue correction, discount-cap enforcement, and the payments-ungated / corrections-gated split

- **Status:** Accepted
- **Date:** 2026-09-08

**Context.** Module 3 implements the patient billing and account settlement backend (Domain guards + pure balance calculator; Application read surface for the billing screen and receipt printing; Application write surface for the cashier). Four rules arrive as settled requirements from the documentation set (Data Model §7.1 + ADR-0016 + FR-M03-001/003/004 for the formula; ADR-0017 + Coding Standards §7.4 for void-and-reissue; FR-M17-004 item 11 for the accounting-control gate; Data Model §13 BR-06 + FR-M03-006/BR-06 + FR-M17-004 item 7 + Test Strategy §3.2/§7.2 for the discount cap) but each contains an undocumented sub-detail the implementation had to pin. The owner confirmed all four pins verbatim at execution; this ADR records them so no future module re-derives them.

**Decision.**

1. **Balance formula (settled — recorded verbatim).** `TotalCharged = Σ PriceAtOrderTime + Σ Amount of non-voided extra-charge ops`; `TotalPaid = Σ (Amount + coalesce(DiscountAmount,0)) of non-voided non-extra-charge ops` (discounts included; `Correction` rows contribute to `TotalPaid`, positive Amount = credit); `Balance = TotalCharged − TotalPaid` (negative = credit, no clamping); `FullSettlement` rows are ordinary payments whose Amount equals the balance at settlement time; voided rows contribute nothing. Implemented once as the pure static Domain service `PatientAccountCalculator` (`TotalCharged`/`TotalPaid`/`Balance`); every Application query and the settlement command delegate to it, so the formula cannot drift between consumers. Table-driven Domain tests pin the shared worked example (prices 100+50, extra charge 20, payment 80 with discount 10, voided payment 999 ⇒ Charged 170, Paid 90, Balance 80).
2. **`Correction` sign convention (settled — recorded verbatim).** Positive Amount = credit reducing balance. `RecordCorrectionCommand` therefore validates `Amount > 0` and creates `OperationType.Correction` rows that land on the paid side of the formula.
3. **Discount-cap exemption (settled — recorded verbatim).** Absolute-permission users are EXEMPT from the discount cap; all others are capped. Enforcement is application-layer only, against `User.DiscountLimitPercent`, before the row is written: when `DiscountAmount` is supplied and the current user is not absolute-permission, `DiscountAmount > Amount * user.DiscountLimitPercent / 100m` ⇒ `Error.Validation("الخصم يتجاوز الحد المسموح به لهذا المستخدم.")` (breach is a `Validation`-type failure per Test Strategy §3.2). There is no discount-limit permission code — the M17 grant screen's meaning is unchanged. Defensive pin (implementation-level, not owner-confirmed): a missing current-`User` row carries no limit grant, so the effective cap is zero and any positive discount breaches with the same frozen message.
4. **Void-and-reissue-only correction + the id-11 gate.** No edit command ships — a mis-recorded amount is corrected by voiding the row and re-recording (ADR-0017 + Coding Standards §7.4; the `Void()` mutator stays idempotent in Domain while the handler returns the friendly `Error.Conflict("العملية ملغاة بالفعل.")` on double-call). `RecordCorrectionCommand` and `VoidPaymentOperationCommand` are `IAuthorizedRequest` with `RequiredPermissionCode => PatientBillingAccessPolicy.CashDisburseDeposit` (the literal `"CASH_DISBURSE_DEPOSIT"`, id 11, already seeded — no `PermissionConfiguration` change). Honest note: the gate assignment is the documented-scope reading of FR-M17-004 item 11 (whose scope explicitly includes patient accounting «محاسبة المرضى»), not a verbatim quote; the owner can re-point the gate by editing the one constant with zero structural change.
5. **Single-payment-command shape + payments-ungated / corrections-gated split.** `RecordPaymentCommand` (optional discount), `RecordExtraChargeCommand` (`IsExtraCharge = true`, no discount field — the Domain guard forbids the combination), and `SettleAccountInFullCommand` (computes the live balance, rejects `Error.Conflict("لا يوجد رصيد مستحق للتسوية.")` when balance ≤ 0, writes `FullSettlement` with `Amount = balance`) are UNGATED routine registrar/cashier actions; only correction and void are gated. Settlement carries no discount field — a cashier combining settlement with a discount issues `RecordPaymentCommand` for the balance amount instead (handler XML doc). New-entity ids use the `Create(0)` sentinel (SQL Server IDENTITY generates the real value; EF reads it back) — never `Max()+1`.
6. **No-edit decision + UI-Blueprint S-04 reconciliation note.** S-04's "Edit → `UpdatePaymentOperationCommand`" mapping must be realized as a composite void-and-reissue flow behind the «تعديل» button, or S-04 corrected — ADR-0017 governs. This module was NOT changed to add an edit command, and S-04 was NOT changed here.
7. **Deleted-user name fallback.** `ReceivedByUserName` resolves from the `Users` set; when the user row is gone the DTO carries the raw id string (dedicated handler test).

**Consequences.** Diff is confined to `src/TopLab.Domain/Billing/` (guards + `IsEffectivelyZero` getter-only convenience property + `PatientAccountCalculator`), `src/TopLab.Application/Features/PatientBilling/` (DTOs, access policy, shared `PatientBillingReader`, 3 queries + 3 validators, 5 commands + 5 validators, feature-local `DomainFailureTranslator`), `tests/**` (Domain calculator/guard tests, Application handler/authorization/translator tests, Infrastructure F5 mapping assertions + InMemory round-trip), and `Docs/**` (this entry, the tracking-sheet flip, `Handoff_M03.md`). **No migration** — `dotnet ef migrations has-pending-model-changes` returns "No changes have been made to the model since the last migration" (guard-only Domain change; the getter-only property is unmapped, pinned by `PaymentOperation_IsEffectivelyZero_IsNotMapped`). **No `PermissionConfiguration` change. No Presentation content anywhere.** Validators resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (no DI wiring change); no `FakeApplicationDbContext` extension was needed (all 6 lists present in the 33-list fake). `PatientTest` carries no `IsDeleted` flag (confirmed against the live tree; corroborates the recorded M02 position) — no soft-delete filter on charged-tests selections.

**Related.** M-03 Implementation Plan §2; ADR-0016 (balance formula) + ADR-0017 (void-and-reissue); Data Model §7.1/§13 BR-06; FR-M03-001/003/004/006 + FR-M17-004 items 7/11; Test Strategy §3.1/§3.2/§7.2; ADR-0030/ADR-0031 (zero-migration + permission-reuse precedent); ADR-0032/ADR-0033 (access-policy mirror convention).

---

### ADR-0035 — M-04: Results Entry lifecycle guards, reference-range freeze table, seven-state calculator, and D1–D5 owner pins

- **Status:** Accepted
- **Date:** 2026-09-08

**Context.** Module 4 implements the results-entry backend (Domain lifecycle guards + persisted BR-05 freeze; Application read surface for the results screen/entry form/print preview; Application write surface for entry/review/print/deliver/export/bulk; Infrastructure proof). The seven-state model (PRD §8.2/§8.3, BR-01, ADR-0015, Test Strategy §3.1), the print-time balance block (Data Model §13 BR-07 + Reporting §9 + FR-M09-003), auto review-and-completion (FR-M22-008 + PRD §8.4 + Test Strategy §7.2-M04), flag auto-compute + freeze persistence with explicit refresh (Data Model §6.1/§13 BR-05 + FR-M04-008 + Test Strategy §7.2-M04), printability-requires-verification (Reporting §9), and the permission catalog (EDIT_RESULTS/REVIEW_RESULTS/PRINT_RESULTS/DELIVER_RESULTS) arrive as settled requirements. Five owner decisions (D1–D5) arrived after the initial plan and override any older conflicting text; this ADR records the settled implementation plus the D1–D5 pins so no future module re-derives them.

**Decision.**

1. **Seven-state calculator (settled — delivered in full, no stub, no waiver).** `PatientStatusCalculator.Calculate(Patient, IReadOnlyList<PatientTest>, decimal balance)` implements PRD §8.2/§8.3 verbatim: per-analysis stage from the lifecycle columns (entry-pending→1, review-pending→2, print-pending→3, delivery-pending→4, delivered→6), account stage from the injected balance (balance > 0 ⇒ stage 5, evaluated only when no analysis is at stages 1–4; balance ≤ 0 ⇒ stage 6), `PatientStatus = state(min stage)` in lifecycle precedence order (never by date). S1 applies iff no analysis has `EnteredAtUtc` set AND `RegistrationDateUtc` falls on the current UTC day; otherwise unentered ⇒ S2 (the "newly registered" micro-pin — §8.3 is not operationally defined in `Docs/Source/`, so the pin is recorded here; flipping it is a one-line change plus one test row). Binding worked example {1,3,4} → S2 pinned by test. S5-with-balance ⇒ S5 (delivery precedes settlement); S6 only when all delivered with balance; S7 when delivered and settled (credit ≤ 0 ⇒ S7). Stateless Domain service, never stored, never cached (ADR-0015).
2. **Lifecycle guard chain + edit-lock policy (owner-settled).** `EnterResult` rejects when `IsReviewed` ("Result is reviewed; unreview first."); `ClearResult` resets value/flag/notes/entered columns and rejects when `IsReviewed || IsPrinted || IsDelivered` ("Result is locked."); `Unreview` clears review columns and rejects when `IsPrinted || IsDelivered` ("Printed or delivered results cannot be un-reviewed."); `MarkReviewed` rejects when `EnteredAtUtc is null` ("Result not entered."); `MarkPrinted` rejects when `EnteredAtUtc is null || !IsReviewed` ("Result not reviewed."); `MarkDelivered` rejects when `!IsPrinted` ("Result not printed."); `MarkEntered(int, DateTime)` stamps entered-only for profile/culture flows and mirrors the reviewed-guard. Free re-entry until reviewed; after `IsReviewed`, entry rejects with `Conflict` and only a `REVIEW_RESULTS` holder may un-review-then-re-enter via dedicated `UnreviewResultCommand`. M02/M21 never call these mutators (verified) — guards are additive.
3. **Reference-range freeze as a dedicated 1:1 child table (owner-settled storage mechanism; the module's only migration).** `PatientTestReferenceRangeSnapshots` keyed PK+FK `PatientTestId → PatientTests` Cascade, mirroring the `ReferenceRangeSnapshot` record shape verbatim (`TestId` as plain `int`, no FK to `Tests` — historical document, not live reference; `Sex`/`AgeUnit` tinyint, `MinValue`/`MaxValue` decimal(18,4), `LowComment`/`HighComment` nvarchar(500), `CapturedAtUtc` datetimeoffset). No stateless snapshot intent methods on the aggregate (they would be dead code — handlers create/remove the child row directly via `_db`). Implementation clarification recorded here: `FromSnapshot` takes the owning `PatientTestId` plus the record (`FromSnapshot(PatientTestId, ReferenceRangeSnapshot)`) because the 1:1 PK cannot come from the record alone; the plan snippet showing a single-parameter overload is read as abbreviated.
4. **Overlapping-range selection (owner-settled).** Most-specific wins: sex-matched preferred over sex-null, then narrowest `(AgeMax − AgeMin)`, then lowest id (oldest row). Pure `ResultFlagComputer.SelectMatch`; `Compute` returns null on non-numeric or no match, else Low/High/Normal with inclusive boundaries. Age-unit-sensitive per BR-04/FR-M12-005 (no conversion).
5. **Explicit refresh path (FR-M04-008).** Old values persist until refresh; `RefreshResultReferenceRangeCommand` (gated `EDIT_RESULTS`) rejects reviewed results with the same lock message, replaces the snapshot row (or deletes when no range now matches) and recomputes `ResultFlag` from the stored value, saving once (report-side «تحديث» then «موافق»).
6. **Auto review-and-completion as a system action (§8.4).** When `SystemSettings` row `Id == 1` has `AutoReviewAndComplete == true` (read directly — M22 precedent), `EnterResultCommandHandler` immediately calls `MarkReviewed` with no `REVIEW_RESULTS` precondition; both on/off paths tested.
7. **Balance block before printing only (settled).** `MarkResultPrinted` (gated `PRINT_RESULTS`) refuses before printing with `Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.")` when `user.BlockPrintOnRemainingBalance && !_currentUser.IsAbsolutePermission && BalanceProbe.Balance(patientId) > 0` (Test Strategy §3.2 permits `Forbidden`/`Conflict` — this handler declares `Conflict`, asserted verbatim). `BLOCK_PRINT_ON_BALANCE` (id 5) is not a runtime gate — it is the grantable per-user item backing the flag. `MarkResultDelivered` (gated `DELIVER_RESULTS`) has no balance gate (FR-M09-003 places the block before printing for delivery; delivery is the physical handover — BR-08). `BalanceProbe` is a thin loader delegating exclusively to `PatientAccountCalculator.Balance` (D5 — no duplicated formula; inward-safe because Application already depends on Domain).
8. **Simple-only entry channel.** `EnterResultCommand` (gated `EDIT_RESULTS`) is for `ResultKind.Simple` only (`Error.Conflict("لا يمكن إدخال نتيجة إلا لتحليل بسيط.")` otherwise; profile/culture retain dedicated surfaces M05/M06). D4 guard rejects null/empty/whitespace before any mutation with exactly `الرجاء إدخال قيمة النتيجة قبل الحفظ` (validator + handler defense; numeric and symbolic non-whitespace remain valid; `ClearResult` remains the explicit removal path).
9. **D1 atomic registration.** `CreatePatientCommand` requires one-or-more ordered-test inputs (same shape as `AddTestsToVisit.AddTestInput`); the handler validates, resolves prices via `TestPriceResolver` (including contract price-list via `GetPriceListByIdQuery`), creates the `Patient` plus `PatientTest` rows, then saves once. Empty selection is `Validation("يجب اختيار تحليل واحد على الأقل.")`; duplicates reuse the M02 message. `AddTestsToVisit` remains edit-existing-visit only.
10. **D2 local PDF export.** `ExportPatientReportPdfCommand` (gated `PRINT_RESULTS`) exports the complete verified report to a caller-supplied absolute local/LAN `.pdf` path via the Application port `IPatientReportPdfExporter` implemented by Infrastructure `PatientReportPdfExporter` (minimal valid PDF, `FileMode.CreateNew` defense-in-depth, testable without Presentation). Same verified eligibility as printing (all rows entered+reviewed) but no balance gate, no `PrintCount`/lifecycle change. Pre-existing target ⇒ `Conflict` with no overwrite; non-absolute/non-PDF ⇒ `Validation`; I/O/render failure ⇒ `Unexpected` with no export marks. Included rows marked `MarkExported` only after successful write. No export permission seed, no migration.
11. **D3 bulk reprint consent.** Bulk print is a patient-report two-step contract: preflight returns per-patient `RequiresReprintConfirmation` (true when any verified included row has `IsPrinted`); Presentation asks exactly `لقد تم طباعه هذا التقرير لهذا المريض من قبل هل ترغب في اعاده الطباعه` per such candidate (pinned as `BulkPrintMessages.ReprintConfirmationMessage`) then submits explicit per-patient Confirm/Cancel. Confirm reprints via the ordinary path (permission, verified eligibility, balance block once per patient, `PrintCount`/audit increment — intentionally non-idempotent); Cancel skips the entire patient report and continues. No review/delivery/balance/absolute-permission bypass.

**Consequences.** Diff is confined to `src/TopLab.Domain/Results/`, `src/TopLab.Domain/PatientStatus/`, `src/TopLab.Application/Features/ResultsEntry/**`, `src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/` (D1), `src/TopLab.Infrastructure/Persistence/Configurations/PatientTestReferenceRangeSnapshotConfiguration.cs` + `DbSets` + the one new migration `AddPatientTestReferenceRangeSnapshots`, `src/TopLab.Infrastructure/Services/PatientReportPdfExporter.cs` + DI registration (D2), `tests/**`, `Docs/**`. Exactly one migration; no `PermissionConfiguration` change; zero Presentation content. Validators (14 new) resolve via `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (no DI change); `ValidatorRegistrationTests` covers all 14. `FakeApplicationDbContext` gains `PatientTestReferenceRangeSnapshots` (+ `ProfileResultItems`/`CultureResults` for the export flavor coverage). No `PatientTest.IsDeleted` (confirmed against the live tree) — named queries carry no soft-delete filter on `PatientTest`.

**Related.** M-04 Implementation Plan §3; PRD §8.2/§8.3/§8.4; BR-01/BR-05/BR-07/BR-08; ADR-0015 (computed status) + ADR-0016 (computed figures); Data Model §6.1/§6.3/§7.1/§13; Reporting §9; FR-M04-001/008 + FR-M08-007 + FR-M09-003 + FR-M12-005 + FR-M22-008; Test Strategy §3.1/§3.2/§7.2-M04; M12 `ReferenceRangeDto` surface; M22 settings; ADR-0032/ADR-0033/ADR-0034 (permission-reuse + zero-drift precedent).

---

### ADR-0036 — M-05: Specialized profile result reports — Analyte-owned live ranges, frozen-range freeze table, central pricing, atomic post-print amendment, and permission reuse

- **Status:** Accepted
- **Date:** 2026-09-09

**Context.** Module 5 implements the specialized profile result-report backend (Domain range/freeze/pricing semantics; Application read surface for the results-board entry grid, the frozen-range report/reprint, and the amendment history; Application write surface for draft save/verify/unverify/print/amend; Infrastructure proof). Profiles are typed test rows whose `TestId` links a committed `Test` of `ResultKind.SpecializedProfile` 1:1 to a `Profile`; analyaste composition is the `ProfileAnalyte` join table; the analyte owns the *only* live `AnalyteReferenceRange` (replacing the M12 `ReferenceRange` matrix for profiles) and freezes the matched band into a per-item reference-range snapshot. Five owner decisions (D1–D5 plus the re-derived profile-domain pins) arrived after the initial plan and override older text; this ADR records the settled shape so no later module re-derives it.

**Decision.**

1. **Only the live range freezes (settled D1).** The analyte’s `AnalyteReferenceRange` (with its `AnalyteReferenceRangeBand` age/sex rows) is the sole live range for a profile analyte. `ProfileResultItemReferenceRangeSnapshot` is a 1:1 child of `ProfileResultItem` (PK+FK `ProfileResultItemId`, Cascade), storing the matched band verbatim (`AnalyteId` plain FK to `Analytes`, `Sex` tinyint nullable, `AgeUnit` tinyint, `AgeMin`/`AgeMax` int, `MinValue`/`MaxValue` decimal(18,4), `LowComment`/`HighComment` nvarchar(500), `CapturedAtUtc` datetimeoffset). Matching runs through the shared `ResultFlagComputer.SelectMatch(sex, ageUnit, age, bands)` (most-specific selection, BR-04 age-unit sensitive — no conversion). Reports/reprints read snapshots only — a recapture on band change never mutates a saved/printed item; `GetProfileReport` renders `analyte.ReportName` for display. Only the single `AddAnalyteProfileDomain` migration adds the catalog + result tables.
2. **Profile result accounting through the central calculator (settled D2).** No feature-local price formula. Manual picks charge `PatientAccountCalculator.ManualSelectionCharge` (sum of `TestPriceResolver` outputs) and typed profile ordering charges exactly `PatientAccountCalculator.ProfileSelectionCharge(profile.FixedPrice)`. `AddProfileToVisitCommandHandler` stores that center charge at order time; constituent analytes, price lists and custom groups never influence the profile price.
3. **Atomic post-print amendment (settled D3).** `AmendProfileResultCommand` (gated `EDIT_RESULTS`): only printed items are amendable (`Conflict("لا يمكن تعديل نتيجة البروفايل قبل الطباعة.")` otherwise); the active row’s value/unit/flag is updated in place (no unprint, no new version) via `ProfileResultItem.Amend`, and a complete immutable `ProfileResultAmendment` audit row (old+new value/unit/flag, `AmendedByUserId`, `AmendedAtUtc`, optional `Reason` nvarchar(500), `ProfileResultItemId` FK Cascade + index) is added in the SAME `SaveChangesAsync` so both commit or both roll back. Amendment history is served by the existing `PT_AUDIT_ACCESS` permission — no new permission row, no `PermissionConfiguration` change.
4. **Profile-domain pins.** `Analyte.Name` unique; `AnalyteReferenceRange.AnalyteId` unique (cascade from `Analyte`); `Profile.TestId` unique 1:1 to `Tests` (cascade); `ProfileAnalyte` composite `(ProfileId, AnalyteId)` unique with cascade FKs + `AnalyteId` index; `ProfileResultItem` FK to `PatientTest` Cascade + to `Analyte` **Restrict** (an analyte with result rows cannot be deleted) with `PatientTestId`/`AnalyteId` indexes. Analyte deactivation (`IsActive`) keeps history printable.
5. **Draft-only edit surface.** Typed profile entry uses a grid of items; `SaveProfileResults` acts before review only (`EnterResult`-equivalent guard with Arabic message), replaces draft rows (never touches printed ones), captures each item’s band snapshot, and saves once. `VerifyProfileResults`/`UnverifyProfileResults` gate on review; `MarkProfilePrinted` verifies entered+reviewed, reuses the M-04 `BalanceProbe` per-user print block with absolute-permission bypass, and marks only unprinted items (reprint-safe). Feature-local `DomainFailureTranslator` maps Domain `Conflict`s to friendly Arabic; a `ResultsEntryAccessPolicy` constant reuse (`EDIT_RESULTS`/`REVIEW_RESULTS`/`PRINT_RESULTS`) keeps the permission catalog unchanged.
6. **Backend only.** No Presentation views, no DI changes, no additional migrations, no seed mutations. VG proof includes EF model-config assertions (F5 conventions), InMemory relational round-trips on the real `ApplicationDbContext` (profile-order central charge; amendment+audit atomic rollback proven by a deliberately failing `ISaveChangesInterceptor`; reprint-after-live-range-change across separate contexts reading the frozen snapshot), and the `has-pending-model-changes` drift gate.

**Consequences.** Diff confined to `src/TopLab.Domain/Tests/` (Analyte/Profile/ProfileAnalyte/AnalyteReferenceRange(+Band)/patient-result tables), `src/TopLab.Domain/Results/` (`ProfileResultItem` + snapshot + amendment), `src/TopLab.Application/Features/ProfileResults/**` + the `AddProfileToVisit` central-charge integration, `src/TopLab.Infrastructure/Persistence/Configurations/*` + `DbSets` + the one `AddAnalyteProfileDomain` migration, `tests/**`, `Docs/**`. Exactly one migration; zero drift against the applied baseline; no `PermissionConfiguration`, no DI, no Presentation change. Full solution suite stays green; Release build 0 warnings/0 errors.

**Related.** M-05 Implementation Plan §4; PRD §8 (profile entry/review/print/amend); BR-04/BR-05; ADR-0016 (computed figures) + ADR-0034 (central calculator) + ADR-0035 (range freeze + balance block + `ResultFlagComputer` matcher); Data Model §6.1/§7.1/§13; Reporting §9; FR-M05-001/004/008 (entry grid, freeze, amendment history); Test Strategy §7.2-M05; M-04 permission reuse (ADR-0032/ADR-0033 precedent).

---

*End of document.*

---

### ADR-0037 — M-06: Culture result entry uses attached antibiotics and structural pregnancy indication

- **Status:** Accepted
- **Date:** 2026-09-11

**Decision.** Culture entry accepts only antibiotics attached to the culture test; a submitted unattached ID is rejected. Entry-grid flags are display-time only and never suppress already recorded results. Pregnancy is represented by `MedicalConditionCategory.Pregnancy = 2` and the seeded «حمل» catalog row, evaluated structurally through attached condition categories. Culture sensitivity saves replace the list atomically; verification uses `MarkEntered` then `MarkReviewed`; printing uses the settled balance block.

**Consequences.** The only migration is `AddPregnancyMedicalConditionTypeSeed`, which inserts the required catalog row. The existing `PatientTest.MarkEntered`/`Unreview` contract was already present, so the first-shipper contingency was not invoked. No Presentation content or permission-catalog change was added.
