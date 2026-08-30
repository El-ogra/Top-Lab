
# Top-Lab — Coding Standards & Conventions

## نظام توب لاب — معايير وقواعد كتابة الشيفرة

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Coding Standards & Conventions |
| Status | **Final** — binding on all implementation work |
| Purpose | Define the naming, structural, stylistic, versioning, and workflow conventions that every contributor — human or automated — must follow when writing or modifying Top-Lab source code. |

---

## 1. Scope and Applicability

These conventions apply to every file inside the Top-Lab solution: Domain, Application, Infrastructure, and Presentation projects, and their mirrored test projects. They apply equally to code produced by contributors and by automated coding agents. Any pull request or code hand-off that violates a rule in this document is non-conforming and must be corrected before acceptance.

Rules marked **[Binding]** may not be waived by an individual contributor; changing them requires a superseding architectural decision.

---

## 2. Technology Baseline (Binding)

| Concern | Value |
|---|---|
| Runtime / framework | .NET 8 |
| Language | C# (latest stable version supported by .NET 8) |
| Application type | Windows desktop |
| UI framework | WPF, following the Model–View–ViewModel pattern |
| Application architecture | Clean Architecture with four layers (Domain, Application, Infrastructure, Presentation) |
| Data access | Entity Framework Core |
| Database | Microsoft SQL Server (one shared database) |
| Connectivity | LAN only — no Internet dependency introduced by any package |

No source file may take a dependency on a technology outside this baseline without a superseding architectural decision.

---

## 3. Solution and Project Structure (Binding)

- The solution contains exactly four production projects and three test projects:
  - `src/TopLab.Domain/`
  - `src/TopLab.Application/`
  - `src/TopLab.Infrastructure/`
  - `src/TopLab.Presentation/`
  - `tests/TopLab.Domain.Tests/`
  - `tests/TopLab.Application.Tests/`
  - `tests/TopLab.Infrastructure.Tests/`
- New top-level projects are not created except via a superseding architectural decision.
- New functional capabilities are added inside the existing projects, in the feature folder that corresponds to the capability's functional area.

### 3.1 Dependency Rule (Binding)

Source-code dependencies point strictly inward:

```
Presentation   ──▶  Application  ──▶  Domain
Infrastructure ──▶  Application  ──▶  Domain
```

- `TopLab.Domain` compiles with only the .NET base class library. It never references EF Core, WPF, MediatR, file I/O, or any external package.
- `TopLab.Application` references only `TopLab.Domain` and mediator / validation abstractions permitted by architectural decision.
- `TopLab.Infrastructure` references `TopLab.Application` and `TopLab.Domain`.
- `TopLab.Presentation` references `TopLab.Application` only, EXCEPT for the composition root (`App.xaml.cs` and its direct DI wiring call), which may reference `TopLab.Infrastructure` directly and exclusively for the purpose of dependency registration. No ViewModel, View, or any other Presentation class may reference Infrastructure under any circumstance.

### 3.2 Feature-folder organization (Binding)

Inside `TopLab.Application` and `TopLab.Presentation`, code is organized by feature, not by technical type. Each feature folder holds every artifact that belongs to that feature.

Application feature folder shape:

```
Features/<FeatureName>/
├── Commands/
│   └── <UseCaseName>/
│       ├── <UseCaseName>Command.cs
│       ├── <UseCaseName>CommandHandler.cs
│       └── <UseCaseName>CommandValidator.cs
└── Queries/
    └── <UseCaseName>/
        ├── <UseCaseName>Query.cs
        ├── <UseCaseName>QueryHandler.cs
        └── <UseCaseName>QueryValidator.cs   (when needed)
```

Presentation layer mirrors the same feature names under `Views/<FeatureName>/` and `ViewModels/<FeatureName>/`.

### 3.3 Domain grouping

Inside `TopLab.Domain`, code is organized by domain grouping (Patients, Tests, Results, PatientStatus, Billing, ExternalEntities, SentOutSamples, Users, Attendance, Audit, Accounting, SampleCollection, Settings, Common). One folder per grouping. Cross-cutting base types live under `Common/`.

---

## 4. Naming Conventions (Binding)

### 4.1 File and type names

| Element | Convention | Example |
|---|---|---|
| Command | `<Verb><Noun>Command` | `RegisterPatientCommand` |
| Command handler | `<CommandName>Handler` | `RegisterPatientCommandHandler` |
| Command validator | `<CommandName>Validator` | `RegisterPatientCommandValidator` |
| Query | `<Verb><Noun>Query` | `SearchPatientsQuery` |
| Query handler | `<QueryName>Handler` | `SearchPatientsQueryHandler` |
| Domain entity | Singular noun | `Patient`, `Test`, `Result` |
| Value object | Singular noun describing the concept | `PatientPhoneNumber`, `PatientId` |
| Domain service | `<Concept>Calculator` or `<Concept>Service` | `PatientStatusCalculator` |
| Application port (interface) | `I<Capability>` | `IReportPrintingService`, `IBarcodeService` |
| Infrastructure implementation | `<Capability>Service` / `<Capability>Repository` | `ReportPrintingService` |
| ViewModel | `<Screen>ViewModel` | `PatientRegistrationViewModel` |
| View (XAML) | `<Screen>View` or `<Screen>Window` | `PatientRegistrationView` |
| EF Core configuration | `<Entity>Configuration` | `PatientConfiguration` |
| DbContext | `ApplicationDbContext` | `ApplicationDbContext` |
| Interceptor | `<Purpose>Interceptor` | `AuditableEntitySaveChangesInterceptor` |
| Pipeline behavior | `<Concern>Behavior` | `ValidationBehavior`, `AuthorizationBehavior`, `LoggingBehavior` |
| Result / error types | `Result`, `Result<T>`, `Error`, `ErrorType` | as documented in the Application layer |

### 4.2 Identifier casing

- **PascalCase** — type names, method names, public members, constants, enum values, namespaces.
- **camelCase** — parameters, local variables.
- **_camelCase** — private instance fields.
- **camelCase** — private static readonly fields treated as instance-like constants may also use `s_` prefix with camelCase (`s_defaultTimeout`) where a clear distinction is required, at the discretion of the reviewer.
- **UPPER_SNAKE_CASE** — reserved exclusively for stable permission codes (see §4.4). It is not used elsewhere.

Abbreviations follow standard C# rules: acronyms of two letters remain uppercased (`Id`, `IO`); acronyms longer than two letters are cased like ordinary words (`Sql`, `Html`). Do not write `ID`, `SQL`, or `HTML` in identifiers.

### 4.3 Boolean naming

Boolean fields, parameters, and properties are named with an affirmative predicate: `IsDeleted`, `IsVoided`, `HasBreakPeriod`, `IsAbsolutePermission`. Negations (`IsNotFinished`) are not used; the caller writes `!IsFinished` instead.

### 4.4 Permission codes

Permission codes stored on the `Permission` catalog use stable, uppercase snake case (for example, `ADD_EDIT_PATIENT`, `PT_AUDIT_ACCESS`). Once assigned, a permission code is never renamed; new permissions receive new codes.

### 4.5 Namespaces

Namespace equals folder path within the project. Example: a class in `src/TopLab.Application/Features/PatientRegistration/Commands/RegisterPatient/` uses namespace `TopLab.Application.Features.PatientRegistration.Commands.RegisterPatient`.

### 4.6 Files

- One public type per file. The file name matches the type name.
- XAML views live beside their code-behind file with the same base name (`PatientRegistrationView.xaml` + `PatientRegistrationView.xaml.cs`).
- ViewModel files are placed in `ViewModels/<FeatureName>/` and named `<Screen>ViewModel.cs`.

---

## 5. Layer-Specific Rules (Binding)

### 5.1 Domain layer

- No dependency on any package outside the .NET base class library.
- Entities inherit from a common `Entity` base and, when they carry audit information, from `AuditableEntity`.
- Domain invariants are enforced inside entity methods. Public setters that would allow an invalid state are not exposed.
- Business-rule violations that are expected (for example, "cannot mark printed before reviewed") are surfaced by returning a `Result` from the enforcing method, not by throwing.
- Value objects are immutable and implement structural equality via the `ValueObject` base.
- Strongly-typed identifiers (`PatientId`, `LabId`, `TestId`, and equivalents) are used in place of raw primitives on Domain entity IDs.
- Domain services are stateless.

### 5.2 Application layer

- Every use case is either a Command (state-changing) or a Query (read-only). A single use case is never both.
- Commands and Queries are plain data records; they do not contain behavior.
- Handlers depend only on abstractions (interfaces) defined in the Application layer, plus Domain types.
- No handler references a concrete Infrastructure class directly; wiring happens in `DependencyInjection.cs`.
- Handlers return `Result` or `Result<T>`. They do not throw for expected outcomes.
- Validators live in the same folder as their Command/Query. Every Command has a validator; a Query has one only when input validation is required.
- Cross-cutting concerns (validation, authorization, logging) are applied via pipeline behaviors, never by per-handler code.
- Every Command or Query that requires a permission declares it as part of its own definition.

### 5.3 Infrastructure layer

- Every Infrastructure class implements an Application-layer interface.
- Exceptions thrown by external libraries (EF Core, printing APIs, file I/O) are caught at the Infrastructure boundary and translated into an `Error` of type `Unexpected` before the failure crosses back into the Application layer.
- EF Core entity configurations use the Fluent API exclusively. Data annotations on Domain entities are not used.
- One `ApplicationDbContext` exists. New context types are not introduced.
- Migrations live in `Persistence/Migrations/`. Migration file names produced by the EF Core tooling are kept as-is.
- The `AuditableEntitySaveChangesInterceptor` is the single mechanism that populates audit columns. Handlers do not set them manually.

### 5.4 Presentation layer

- Views (XAML) contain no business logic. Code-behind is limited to view construction and to wiring events that cannot be expressed through data binding.
- ViewModels depend only on `IMediator` (or the equivalent Application-layer entry point), on Presentation-layer services (navigation, dialogs, error presentation), and on primitive/immutable types.
- ViewModels never reference `ApplicationDbContext`, EF Core types, or Infrastructure classes.
- Every ViewModel command receives a `Result` from the mediator and, on failure, passes the `Error` to `ResultErrorPresenter` for consistent, user-facing display.
- The composition root (dependency injection wiring for all layers) lives in `App.xaml.cs`. Other files do not compose the container.

---

## 6. Cross-Cutting Coding Rules

### 6.1 Error handling

- **[Binding]** Exceptions are reserved for truly unanticipated failures. Expected business outcomes are conveyed via `Result` / `Result<T>`.
- **[Binding]** Failure `Error` values carry `Code`, `Message`, and `ErrorType` (one of `Validation`, `NotFound`, `Conflict`, `Forbidden`, `Unexpected`).
- Catch blocks that swallow exceptions without translating them into an `Error` are not permitted.
- Never rethrow with `throw ex;` — use `throw;` to preserve the stack trace.
- `catch (Exception)` is used only at Infrastructure boundaries, where the raw exception is translated to an `Error` of type `Unexpected`.

### 6.2 Validation

- Validation of input and business preconditions is declared via a fluent validation library, colocated with the Command/Query it validates.
- Validators return every violated rule, not just the first.
- Validation runs before the handler, through `ValidationBehavior`. Handlers assume valid input.

### 6.3 Authorization

- Every use case that requires a permission declares it as part of its definition (for example, via an interface or attribute recognized by `AuthorizationBehavior`).
- Handlers do not re-check permissions.
- The restricted `P` and `T` audit surfaces are gated end-to-end by the authorization pipeline behavior.

### 6.4 Auditability

- Mutable business entities derive from `AuditableEntity`.
- The `SaveChanges` interceptor is the only writer of `CreatedByUserId`, `CreatedAtUtc`, `LastModifiedByUserId`, `LastModifiedAtUtc`, and `ModificationCount`. Handlers and repositories do not touch these fields.

### 6.5 Logging

- `LoggingBehavior` records each request's name, outcome (success or failure with error type), and duration.
- Handlers do not add ad hoc logging that duplicates the pipeline output. Structured additions are permitted only when they carry information not visible to the pipeline (for example, the identifiers involved in a batch operation).

### 6.6 Time

- All persisted timestamps are `DateTime` values in UTC with `Kind = DateTimeKind.Utc`. Persisted column names end with `AtUtc`.
- Presentation converts to local time only at display and never persists the local value.
- Wall-clock reads inside handlers go through `IDateTimeProvider`; direct calls to `DateTime.UtcNow` from handlers are not allowed. The default implementation in Infrastructure delegates to `DateTime.UtcNow`.

### 6.7 Money

- Monetary values use `decimal` (never `float` or `double`).
- Persisted monetary columns use `decimal(18,2)`.
- Rounding, when required, uses banker's rounding (`MidpointRounding.ToEven`) unless a specific use case documents a different requirement.

### 6.8 Nullability

- The `Nullable` compiler feature is enabled solution-wide.
- Reference-type parameters and members that may be null carry the `?` suffix.
- Guard clauses at the start of a public method reject nulls with `ArgumentNullException` for unexpected nulls or with a `Result.Failure(...)` of type `Validation` for expected user-facing input.

### 6.9 Async and cancellation

- I/O operations (EF Core, printing, file I/O) are asynchronous. Handlers accept and forward a `CancellationToken`.
- Method names for asynchronous work end with `Async`.
- `.Result` and `.Wait()` are not called on tasks. Blocking waits are permitted only at the composition-root entry when unavoidable.

### 6.10 Dependency injection

- Every Infrastructure and Application service is registered in its layer's `DependencyInjection.cs`.
- Service lifetimes: `Scoped` for anything touching the `ApplicationDbContext` or user context; `Singleton` for stateless, thread-safe services; `Transient` for lightweight stateless services.
- ViewModels are `Transient` unless a specific screen requires a longer-lived instance.

### 6.11 Comments

- Comments explain **why**, not **what**. Restating what the code already expresses is discouraged.
- XML documentation is required on public members of `TopLab.Domain` and on all Application-layer interfaces.
- Arabic content in code is limited to strings that surface as Arabic UI text. Identifiers, comments other than user-facing strings, and log messages remain in English.

### 6.12 Formatting

- Formatting is enforced by the solution-wide `.editorconfig`. When the file is silent, defaults are:
  - Indentation: 4 spaces, no tabs.
  - Braces: Allman style (open brace on its own line for types, methods, and control blocks).
  - Line length: soft limit 120 characters.
  - `using` directives are sorted alphabetically, `System.*` first.
  - One statement per line.

### 6.13 Prohibited constructs

- `public` mutable fields.
- Static mutable state in Application or Infrastructure code (Domain excepted only for immutable constants).
- Direct instantiation of concrete Infrastructure classes from Application handlers.
- Direct calls to `ApplicationDbContext` from Presentation.
- `dynamic` types.
- Reflection over Domain entities to bypass encapsulation.

---

## 7. Domain-Specific Coding Rules

### 7.1 Patient aggregate status

- Patient aggregate status is obtained through `PatientStatusCalculator` (Domain service) invoked via the Application layer.
- No screen or report computes the status independently.
- No column stores the status; adding one is non-conforming.

### 7.2 Identifiers

- `PatientId` and `LabId` are distinct types and are never used interchangeably.
- Search paths that accept a laboratory identifier accept a `LabId`; those that accept a per-registration identifier accept a `PatientId`.

### 7.3 Reference ranges

- Reference-range matching is performed against the age unit as stored (day / month / year). No unit conversion occurs anywhere in the codebase.
- Once a result is entered against a reference range, the flag decision is persisted with the result and is not recomputed retroactively when the reference range changes.

### 7.4 Payment operations

- Corrections to payment operations follow void-and-reissue semantics. `PaymentOperation` rows are never physically deleted.

### 7.5 Financial aggregates

- Patient balances and inventory figures are computed at query time from primary records. Running-total columns are not introduced.

### 7.6 Restricted audit surfaces

- Queries that expose the `P` or `T` restricted views declare the corresponding permission requirement on the Query itself. `AuthorizationBehavior` gates them uniformly.
- No screen displays `P` or `T` data through any other query path.

---

## 8. Testing Conventions

- Test projects mirror the production layer they exercise: `TopLab.Domain.Tests`, `TopLab.Application.Tests`, `TopLab.Infrastructure.Tests`.
- Test class names end with `Tests`. Test method names read as `MethodUnderTest_Condition_ExpectedResult` or `Given_When_Then` — one style per test class, applied consistently.
- Domain tests exercise entity invariants, value-object equality, and domain services with no external dependencies.
- Application tests use fakes or in-memory doubles for Infrastructure interfaces. They never touch a real database.
- Infrastructure tests exercising EF Core configurations run against an ephemeral SQL Server instance (LocalDB or equivalent) or a compatible provider explicitly configured for tests; they never run against a shared or production database.
- Presentation is exercised through manual verification and, where practical, ViewModel-level unit tests that invoke the mediator with fakes. No UI-automation layer is mandated.

---

## 9. Version Control Workflow

### 9.1 Branching

- `main` — protected. Always in a state that could be built and shipped.
- `feature/<short-description>` — for new work.
- `fix/<short-description>` — for bug fixes.
- `chore/<short-description>` — for changes that do not modify runtime behavior (build scripts, documentation, formatting).
- Branch names are lowercase with hyphens. Underscores and mixed case are not used.
- Long-lived feature branches are avoided; work is broken down and merged incrementally.

### 9.2 Commits

Commit messages follow the Conventional Commits format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

- **`<type>`** — one of `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `build`, `ci`.
- **`<scope>`** — the feature folder or layer touched (for example, `patient-registration`, `results-entry`, `infrastructure`, `domain`).
- **`<subject>`** — imperative present tense, no trailing period, ≤ 72 characters.
- **`<body>`** — optional; explains motivation and contrast with previous behavior.
- **`<footer>`** — optional; references to tracked work items (for example, `Refs: FR-M02-003`).

Examples:

```
feat(patient-registration): capture multiple phone numbers for a patient

fix(results-entry): guard against print before review

Refs: FR-M04-004
```

- One logical change per commit. Unrelated changes are split.
- Commits do not carry generated artifacts, secrets, connection strings, or local configuration.

### 9.3 Pull requests

- Every change lands via pull request.
- A pull request references the requirement, business rule, or ADR it satisfies in its description.
- A pull request that touches more than one layer explains the cross-layer contract change explicitly.
- A pull request must build successfully and pass all automated tests before review.
- Reviewers verify: Dependency Rule respected, layer responsibilities respected, error-handling convention followed, tests present for new behavior, no forbidden constructs (§6.13) introduced.

### 9.4 Prohibited practices

- Force-pushing to `main` is not permitted.
- Rewriting history on a shared branch after review has started is not permitted.
- Committing binaries into source control, other than icons and assets required at build time, is not permitted.
- Committing user data (real patient names, real financial figures) is not permitted, in any file.

---

## 10. Configuration and Secrets

- Database connection settings live in a workstation-local application configuration file and are never committed to source control.
- The canonical machine-scoped store is `%ProgramData%\TopLab\appsettings.json`, written by the first-run setup wizard and read by the composition root with precedence over the local `appsettings.json` (ADR-0025).
- `appsettings.example.json` is a committed, password-free template that mirrors the schema of the personal settings file; it is the only configuration file that may be committed.
- The internal windows password (secondary password gating sensitive windows) ships with a documented default and is expected to be changed at deployment. The factory default is not committed as an environment-specific value; it is documented as such in deployment materials.
- No credential, connection string, or key material appears in the source tree, in tests, or in commit history.

---

## 11. Localization Rules

- The user interface is Arabic-first. Arabic strings surface as user-facing text; identifiers, code comments, and log output remain in English.
- Arabic and English content share the same `nvarchar` columns; a per-language column is not introduced.
- Right-to-left layout considerations are handled inside the Presentation layer; ViewModels return plain strings and do not encode direction information.

---

## 12. Reviewer Checklist (Applies to Every Pull Request)

- ☐ Files placed inside the correct project and feature folder.
- ☐ Type names, file names, and namespace paths follow §4.
- ☐ Dependency Rule (§3.1) respected.
- ☐ Command / Query / Handler / Validator triad shape respected (§5.2).
- ☐ Handlers return `Result` / `Result<T>` — no unhandled expected outcomes escape as exceptions (§6.1).
- ☐ Cross-cutting concerns applied through pipeline behaviors, not per-handler (§6.2 – §6.5).
- ☐ Timestamps stored as UTC and named `AtUtc` (§6.6).
- ☐ Money stored as `decimal(18,2)` (§6.7).
- ☐ Async work forwards `CancellationToken` (§6.9).
- ☐ No prohibited constructs (§6.13).
- ☐ Tests present in the mirroring test project (§8).
- ☐ Commit messages follow Conventional Commits (§9.2).
- ☐ No secrets, credentials, or real user data in the diff (§10).

---

*End of document.*
