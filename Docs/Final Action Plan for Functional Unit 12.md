# Final Action Plan for Functional Unit 12

**Module:** M-12 — Test Catalog & Reference Ranges
**Repository:** https://github.com/El-ogra/Top-Lab.git
**Reference snapshot (sole basis):** commit `3ce602bee79653edc674be7939df899349853ef8` ("تحسينات المهارة", 2026-09-06) — verified as the HEAD of `main` at analysis time; all file paths and facts below were read directly from the tree at this exact commit.
**Prior planning inputs used as analysis sources only (verified against code, not adopted blindly):** `Docs/Hermes/M-12.md` (136 lines) and `Docs/OpenCode/M-12.md` (1,144 lines), both introduced by the reference commit itself.
**Excluded from all analysis:** the entire `Docs/Reference system files/` folder (`RLS_Learn.pdf`, `RL_Show.pdf`). No file inside it was read, searched, or used in any way. Where information exists only there, it is treated as unavailable.

---

## 1. Executive Summary

Functional Unit 12 (M-12 — Test Catalog & Reference Ranges) delivers the test-catalog management surface of Top-Lab: the **System → بيانات التحاليل** screen with live search (FR-M12-001), test add/edit with the full FR-M12-002 field set, test-group and work-group (Log) configuration (FR-M12-003), the per-test reference-ranges editor with age-unit-sensitive matching (FR-M12-004/005, BR-04), the freeze-contract Domain shape for historical results (FR-M12-006, BR-05), and patient-title configuration (per the repository's own tracking-sheet scope note for M12).

The single most important finding of this plan: **the project at the reference commit is substantially more ready for M-12 than a generic plan would assume.** Every table, entity, EF configuration, DbSet, strongly-typed ID, enum (`Sex`, `AgeUnit`, `ResultKind`), the authorization pipeline, the audit interceptor, and the M22 S1–S8 slicing pattern already exist and are proven by two completed modules (M17, M22 — both 🟩 Done in the Master Tracking Sheet). M-12 requires **zero database migrations, zero new NuGet packages, and zero new architectural patterns**. It is an exercise in correctly extending an established machine.

A second key finding: several structural assumptions present in the prior planning drafts do **not** match the verified snapshot and are therefore rejected or corrected in this plan — most notably (a) a repository/`IUnitOfWork` layer (does not exist; handlers use `IApplicationDbContext` directly), (b) range edit-as-new-version with a "superseded" flag (impossible without a schema change; the `ReferenceRanges` table has no such column), (c) per-field `Test` mutators with domain events (the codebase has no domain-event infrastructure and Domain is architecturally forbidden from referencing MediatR), (d) FlaUI/WinAppDriver UI-automation gates (the repo's own Test Strategy §3.4 explicitly mandates ViewModel unit tests plus manual checklists instead), and (e) a `Max()+1` ID strategy (the F5 migration defines SQL Server `IDENTITY(1,1)` on every M-12 table).

One **newly discovered latent defect** in the existing codebase is flagged in this plan because it directly affects M-12's validation gates: no `AddValidatorsFromAssembly` (or any equivalent) registration exists anywhere in the solution, so FluentValidation validators are likely never resolved by the MediatR pipeline at runtime (the `ValidationBehavior` constructor parameter `IValidator<TRequest>?` silently defaults to `null`). Section 14 (R-1) specifies the verification step and the minimal fix.

The plan is organized as 8 slices (S1–S8) following the proven M22 pattern, with a verified file-by-file change map, an evidence-resolved decision log, a full test strategy aligned to `Top_Lab_Test_Strategy.md`, and an explicit boundary against M-13 (TestComment commands) and M-04 (snapshot persistence).

---

## 2. Reference State and Scope

### 2.1 Verified reference state

- Repository cloned and the commit `3ce602bee79653edc674be7939df899349853ef8` verified to exist (it is the tip commit of the default branch at the time of analysis; `git rev-parse HEAD` returns exactly this hash). All analysis below was performed on a checkout of this commit.
- The commit itself is a planning/docs commit: it adds `.opencode/` agent definitions, `Docs/Hermes/M-12.md`, `Docs/OpenCode/M-12.md`, `Docs/GeneralSettings-Implementation-Plan.md` (a plan for a *different*, future module — not M-12 scope), and `Docs/Identifying and understanding the next stage (post-M22).md`. It changes **no source code**.
- Module status at the snapshot: M17 (User & Permission Management) and M22 (System & Print Settings) are 🟩 Done; M12 and M14 are ⬜ Design / Not Started (`Docs/Source/Top_Lab_Master_Tracking_Sheet.md` §4–§5). M-12's declared dependencies (M17, M22) are therefore satisfied — **no blockers exist for starting M-12**.

### 2.2 Technology stack (verified from `Directory.Packages.props`, `Directory.Build.props`, project files)

- .NET 8 (`net8.0` for Domain/Application/Infrastructure, `net8.0-windows` for the WPF Presentation executable).
- Clean Architecture, 4 projects: `TopLab.Domain` → `TopLab.Application` → `TopLab.Infrastructure` / `TopLab.Presentation` (`TopLab.sln`, 4 src + 3 test projects).
- CQRS via **MediatR 12.5.0** (last free Apache-2.0 line — pinned; do not upgrade).
- **FluentValidation 12.1.1**.
- **EF Core 8.0.30** on **SQL Server** (`UseSqlServer` in `src/TopLab.Infrastructure/DependencyInjection.cs`); `Microsoft.EntityFrameworkCore.InMemory 8.0.30` available in test projects.
- WPF MVVM with the project's **own** `ViewModelBase` / `RelayCommand` / `AsyncRelayCommand` (`src/TopLab.Presentation/Common/`) — **not** CommunityToolkit.Mvvm (contrary to one prior draft's header; no such package is referenced anywhere).
- xUnit v2, no mocking library, plain asserts; fakes hand-written under `tests/*/Common/Fakes/`.
- Central Package Management (`Directory.Packages.props`); licensed-package pins are deliberate (commercial product).

### 2.3 Scope boundaries (enforced throughout)

**In scope:** FR-M12-001 … FR-M12-006, BR-04, BR-05, BR-13 (data side), patient-title configuration (see §3.3, decision D-1), Domain shape of the reference-range snapshot (freeze contract), System-pane entry point with the M17 secondary-password gate.

**Out of scope (verified against PRD and code):**
- Price-list pricing rules (M13); per-test **fixed comments UI and commands** (FR-M13-002 → M13; M-12 adds only a courtesy `TestComment.Update` domain method); culture/antibiotic configuration (M15); result lifecycle and the actual **persistence** of the reference-range snapshot on `PatientTest` (M04 owns the column; M-12 owns the Domain value object only); rendering of low/high comments on printed reports (M04/M05 consume the stored strings); M01 login window; any new EF migration; any new NuGet package; test deletion (no PRD requirement exists for deleting a Test — see §13 edge case E-9).
- `Docs/GeneralSettings-Implementation-Plan.md` describes a *different* future feature (a `GeneralSetting` key-value entity with its own migration). It shares no code or requirements with M-12 and is not consumed by this plan.
- **Everything under `Docs/Reference system files/`** — hard-excluded by mandate.

---

## 3. Functional Unit 12: Confirmed Scope

### 3.1 Requirements (from `Docs/Source/Top_Lab_PRD.md` §M12, lines 271–278, and §BR table)

| Req. | Requirement (condensed, verified wording) |
|---|---|
| FR-M12-001 | Catalog accessible via **System → بيانات التحاليل**; lists all tests; searchable by **test name, containing group, or test number**. |
| FR-M12-002 | Test editing (تعديل) modifies: name, report name, receipt name, group, barcode, completion duration (BR-13 pickup-time basis), sent-out flag + price, patient price, Lab-to-Lab price. Save commits; تراجع cancels. New test via إضافة تحليل. |
| FR-M12-003 | System pane provides **test groups (مجموعات التحاليل)** and **work groups (Log)** configuration. |
| FR-M12-004 | Reference values per test via **القيم المرجعية**; إضافة مدى captures sex, age range, min value, max value; optional low/high comments; editable (تعديل → حفظ) and deletable (حذف). |
| FR-M12-005 / BR-04 | Age-unit-sensitive matching; **no cross-unit conversion** (a 1–60 *day* range does not match a patient registered as 1 *month*). |
| FR-M12-006 / BR-05 | Previously registered patients keep old values until explicitly updated in the report (FR-M04-008). |
| BR-13 | Completion duration drives pickup-time computation (M-12 stores it; the computation itself is M02/M04 territory). |

### 3.2 Entities owned by M-12 at the snapshot (all exist, schema-first from the F5 baseline)

| Entity (verified path) | Existing behavior | Missing for M-12 |
|---|---|---|
| `src/TopLab.Domain/Tests/Test.cs` (`AuditableEntity<TestId>`) | `Create(...)` full field set incl. `ResultKind`, `IsCultureType`; **bulk `Update(...)` already covering exactly the FR-M12-002 editable fields** with guards (non-empty names, duration > 0, sent-out ⇒ price) | Nothing (see D-2) |
| `src/TopLab.Domain/Tests/TestGroup.cs` | `Create` only | `Update(string name)` |
| `src/TopLab.Domain/Tests/ReferenceRange.cs` | `Create` with guards (`AgeMin ≤ AgeMax`, `MinValue ≤ MaxValue`); **`Matches(Sex, AgeUnit, int)` already implements BR-04 exactly** (returns false on unit mismatch, on sex mismatch when `Sex != null`, then age-window check) | In-place `Update(...)` mutator; `CaptureSnapshot()` |
| `src/TopLab.Domain/Tests/WorkGroupLog.cs` | `Create`; `_items` list + `Items` read-only collection (EF relationship mapped **by convention** — snapshot line ~1704: `HasOne(WorkGroupLog).WithMany("Items")`) | `Update(string name)`; membership methods (`AddItem`, `RemoveItem`, `ClearItems`) |
| `src/TopLab.Domain/Tests/WorkGroupLogItem.cs` | Public parameterless ctor (EF) + **public parameterized ctor** | Make parameterized ctor private + `static Create(WorkGroupLogId, TestId)` factory (pre-check passed — see D-6) |
| `src/TopLab.Domain/Tests/TestComment.cs` | `Create` | `Update(string commentText)` (courtesy for M13; **no M-12 commands**) |
| `src/TopLab.Domain/Patients/PatientTitle.cs` | `Create(id, titleText, isDefault)` with guard | `Update(string titleText, bool isDefault)` |

### 3.3 Module scope note on PatientTitle (Decision D-1 — resolved by evidence)

The PRD places patient-title *usage* in M02 (FR-M02-006/007, "القاب وتعريفات المرضى … accessible from the System pane"), but the repository's own committed `Top_Lab_Master_Tracking_Sheet.md` M12 row explicitly notes: *"delivers `TestGroup`, `Test`, `ReferenceRange`, `TestComment`, `PatientTitle`, and the age-unit-sensitive matching rule"* — and the post-M22 stage-identification document (§7 of `Docs/Identifying and understanding the next stage (post-M22).md`) likewise includes "patient titles" in the M12 deliverable list. **Resolution: PatientTitle CRUD (create / update / list, with default handling) is IN M-12 scope.** The prior drafts diverged here (one in, one out); the committed tracking documents decide it.

### 3.4 Freeze contract (Decision D-3 — resolved by evidence)

Both prior drafts independently verified (and this plan re-verified at the snapshot): `PatientTest` (`src/TopLab.Domain/Results/PatientTest.cs`) carries **no** reference-range fields — no `ReferenceRangeId`, `MinValue`, `MaxValue`, `Sex`, `AgeUnit`, `AgeMin`, `AgeMax`, no snapshot column; grep for `Snapshot|ReferenceRange|MinValue|MaxValue|AgeUnit` across `src/TopLab.Domain/Results/` returns zero matches. The `ReferenceRanges` table FK is `Cascade` on `TestId` only.

**Resolution:** M-12 introduces a **pure-Domain** `ReferenceRangeSnapshot` value object (immutable record) + `ReferenceRange.CaptureSnapshot()` factory method. M-12 **does not** persist it anywhere and **must not** register any snapshot entity in `ApplicationDbContext` (no such table exists, none may be created — no migrations in M-12). Persisting the snapshot on result rows is M-04's responsibility (it owns the `PatientTests` schema and its migration authority). This satisfies the M-12 side of BR-05/FR-M12-006 while keeping the no-migration constraint intact. A regression test asserts no snapshot entity appears in the EF model (§12).

---

## 4. Current Architecture Relevant to Unit 12

### 4.1 Layering and flow (verified)

- **Domain** (`TopLab.Domain`): zero external package references (`TopLab.Domain.csproj` has no `PackageReference`). Entities + strongly-typed IDs (`StronglyTypedId<int>` wrappers, ADR-0012) + enums. **Architecturally forbidden from referencing MediatR/EF/WPF** (Architecture Blueprint §3) — this is why domain events are rejected (D-7).
- **Application** (`TopLab.Application`): vertical feature folders under `Features/<FeatureName>/{Commands,Queries,Common}/<UseCase>/` — one file per role (Command, Handler, Validator). Handlers depend only on `IApplicationDbContext` (a persistence port exposing `IQueryable<TEntity> Set<TEntity>()`, `Add`, `Update`, `Remove`, `SaveChangesAsync`, `CanConnectAsync` — **no repositories exist anywhere in the solution**; verified by grep) and return `Result` / `Result<T>` with `Error` (`Validation|NotFound|Conflict|Forbidden|Unexpected`, Arabic messages).
- **Pipeline (verified in `src/TopLab.Application/DependencyInjection.cs`):** MediatR behaviors registered in binding order **Validation → Authorization → Logging** (ADR-0009). `AuthorizationBehavior` checks `IAuthorizedRequest.RequiredPermissionCode` against `ICurrentUserService` (`IsAbsolutePermission` bypass; failure message "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام").
- **Infrastructure**: single `ApplicationDbContext` (partial class: `.cs` + `ApplicationDbContext.DbSets.cs`) with `ApplyConfigurationsFromAssembly` auto-discovery — **adding an entity configuration requires zero edits to the context** (but M-12 adds none). `AuditableEntitySaveChangesInterceptor` (registered scoped) is the single writer of audit columns: on `Added` sets created/modified + `ModificationCount = 0`; on `Modified` updates last-modified and **increments `ModificationCount`** — the established audit mechanism, already covered by dedicated tests.
- **Presentation**: `App.xaml.cs` is the composition root (`AddApplication` → `AddInfrastructure` → `AddPresentation`; auto-`MigrateAsync` at startup — **seed data in `HasData` reaches databases through migrations, not at runtime**). Shell = `MainWindow.xaml` with a top nav bar driven by `ShellViewModel.BuildNavigationItems()`; content switched via `ContentControl` + `DataTemplate` per ViewModel; `INavigationService.NavigateTo<TViewModel>()` resolves VMs from DI. `IDialogService` provides `ShowSecondaryPasswordDialogAsync()` (the M17 gate, already used by "المستخدمون" nav entry and the M22 Database Maintenance window).

### 4.2 Persistence facts that shape the plan (all verified in the F5 migration + model snapshot)

- Tables `Tests`, `TestGroups`, `ReferenceRanges`, `TestComments`, `WorkGroupLogs`, `WorkGroupLogItems`, `PatientTitles` **all exist** in migration `20260828052248_BaselineDataModel` (36 `CreateTable` calls) + `20260828123530_RenamePkColumns` (PK columns renamed to `TestId`, `TestGroupId`, `ReferenceRangeId`, `TestCommentId`, `WorkGroupLogId`, `PatientTitleId` …).
- **Every M-12 table PK is SQL Server `IDENTITY(1,1)`** (migration annotations + `SqlServerModelBuilderExtensions.UseIdentityColumns` in the model snapshot) with EF `ValueGeneratedOnAdd()` + value converters on the strongly-typed IDs.
- Column shapes: `Tests` — all FR-M12-002 fields + `ResultKind tinyint` + `IsCultureType bit` + audit columns; `ReferenceRanges` — `TestId, Sex tinyint null, AgeUnit tinyint, AgeMin int, AgeMax int, MinValue/MaxValue decimal(18,4), LowComment/HighComment nvarchar(500) null`; `WorkGroupLogItems` — composite PK `(WorkGroupLogId, TestId)`, FK Cascade to `WorkGroupLogs`; `PatientTitles` — `TitleText nvarchar(50)`, `IsDefault bit`.
- FK behaviors: `Tests.TestGroupId → TestGroups` **SetNull**; `ReferenceRanges.TestId → Tests` Cascade; `TestComments.TestId → Tests` Cascade.
- **No unique index exists on `Tests.Name`** (only a non-unique `HasIndex`) — duplicate-name protection must be handler-side (same as `CreateUserCommandHandler`'s duplicate check + catch on unique violation), with the residual race documented (R-4).
- Permissions are seeded via `PermissionConfiguration.HasData` (13 codes, Ids 1–13), including **`EDIT_SYSTEM_SETTINGS` (Id 10, "Edit system and test settings")** — the exact permission the PRD/tracking notes assign to this surface. No other catalog permission exists; inventing new codes would require a migration (forbidden) — see D-5.

### 4.3 Established conventions M-12 must follow (verified in M17/M22 code)

- Handler pattern: constructor-inject `IApplicationDbContext`; Arabic error strings; `Error.Conflict("…موجود بالفعل")` for duplicates; `Error.NotFound("…غير موجود")` for missing rows; FK pre-validation before insert; child-row management via load → domain mutator → explicit `_db.Remove(...)`/`_db.Add(...)` per row (precedent: `SaveUserPermissionsCommandHandler`).
- Validator pattern: `AbstractValidator<TCommand>` with Arabic `.WithMessage(...)` (precedents across `SystemAndPrintSettings` and `UsersAndPermissions`).
- ViewModel pattern: `ViewModelBase` + `ObservableCollection` + `RelayCommand`/`AsyncRelayCommand` + `IsBusy` double-submission guard + `ResultErrorPresenter` for errors + `IDialogService` for confirms (precedent: `UserManagementViewModel`, `SettingsDashboardViewModel`).
- View pattern: `Views/<Area>/<Name>View.xaml(.cs)` UserControl, RTL (`FlowDirection="RightToLeft"`), DataTemplate mapping in `MainWindow.xaml`.
- Test pattern: Domain xUnit per-entity test files; Application handler tests against `FakeApplicationDbContext` (hand-written in-memory fake — must be **extended** for the new entity types, see §7 S2); Infrastructure tests via `InMemoryContextFactory`/`TestApplicationDbContext`; Test Strategy §3.3 nominates ephemeral SQL Server (LocalDB) for EF-exercising tests.
- Documentation convention (per `Docs/Identifying...post-M22.md` §13/U3 and Handoff_M22): working documents (`Module nn Implementation Plan.MD`, `M-nnLoop.MD`) stay **untracked**; committed docs are the tracking sheet, ADR log (next sequential number: **ADR-0028**, since ADR-0027 closes M22), and a `Handoff_M12.md` following the 15-section handoff template.

---

## 5. Existing Components That Can Be Reused (no re-implementation)

| Component | Path | Reuse in M-12 |
|---|---|---|
| `ReferenceRange.Matches(Sex, AgeUnit, int)` | `src/TopLab.Domain/Tests/ReferenceRange.cs` | BR-04 already implemented — S1 adds unit tests pinning it; **do not rewrite** |
| `Test.Create` / bulk `Test.Update` | `src/TopLab.Domain/Tests/Test.cs` | Direct use by S3 handlers; field set already equals FR-M12-002 |
| `IApplicationDbContext` + `ApplicationDbContext` + all DbSets/Configurations | `src/TopLab.Application/Common/Interfaces/`, `src/TopLab.Infrastructure/Persistence/` | Handlers use `Set<Test>()` etc. — **no repository layer is to be built** (D-4) |
| MediatR pipeline (Validation → Authorization → Logging) | `src/TopLab.Application/DependencyInjection.cs`, `Common/Behaviors/` | Free for all M-12 requests; commands just implement `IAuthorizedRequest` |
| `EDIT_SYSTEM_SETTINGS` permission + `AuthorizationBehavior` | `PermissionConfiguration.cs`, `AuthorizationBehavior.cs` | Single permission for the whole M-12 write surface (D-5) |
| `AuditableEntitySaveChangesInterceptor` | `src/TopLab.Infrastructure/Persistence/Interceptors/` | Automatic audit incl. `ModificationCount` increment — S8 asserts it fires |
| `ShowSecondaryPasswordDialogAsync` gate | `src/TopLab.Presentation/Common/Dialogs/` | Gate for the "النظام" nav entry (FR: sensitive System surfaces; precedent: المستخدمون, Database Maintenance) |
| `NavigationService`, `ViewModelBase`, `RelayCommand`/`AsyncRelayCommand`, `ResultErrorPresenter` | `src/TopLab.Presentation/Common/` | All M-12 VMs/Views |
| `ShellViewModel` nav stub | `ViewModels/Shell/ShellViewModel.cs` — `"النظام"` currently falls into the `else // Future: navigate to feature` branch | Wire the M-12 entry point here (S7) |
| `MainWindow.xaml` DataTemplate block | `src/TopLab.Presentation/MainWindow.xaml` | Add M-12 templates (S5/S6) |
| Test fakes | `tests/TopLab.Application.Tests/Common/Fakes/`, `tests/TopLab.Infrastructure.Tests/Common/` | Extend `FakeApplicationDbContext`; reuse `FakeCurrentUserService`, `FakeDateTimeProvider`, `InMemoryContextFactory` |
| M22 S1–S8 slicing model | M22 history (commits `c267944`…`a8c1a2b` visible in git log at the snapshot) | Slice structure of this plan |

---

## 6. Required Changes (summary; details in §7–§8)

1. **Domain (S1):** add `Update`/membership/factory methods + `ReferenceRangeSnapshot` VO + `CaptureSnapshot()`; make `WorkGroupLogItem`'s parameterized ctor private with a `Create` factory (pre-check verified: **zero** `new WorkGroupLogItem(` call sites in `src/` and `tests/` at the snapshot).
2. **Application (S2/S3):** new feature folder `Features/TestCatalogAndReferenceRanges/` (name fixed by `Top_Lab_Module_Dependency_Map.md` §2 — **not** "TestsAndReferenceRanges") with 7 queries and 12 commands + validators, all commands `IAuthorizedRequest` with `EDIT_SYSTEM_SETTINGS`.
3. **Infrastructure (S4):** **no new configurations, no migrations.** Only verification (model has no pending changes; no snapshot entity registered) plus the validator-DI fix if the R-1 verification confirms the gap.
4. **Presentation (S5/S6/S7):** 6 ViewModels + 6 Views under `ViewModels/Tests/` & `Views/Tests/`; shell wiring of "النظام" behind the secondary-password gate; DataTemplates; DI registrations.
5. **Tests:** extend Domain/Application test suites; extend `FakeApplicationDbContext`; optionally add `tests/TopLab.Presentation.Tests` (see R-6).
6. **Docs (S8):** Master Tracking Sheet M12 row → Done; ADR-0028; `Docs/Handoff_M12.md`; manual verification checklist executed.

**Must NOT change:** any existing entity's schema mapping; `ApplicationDbContext` model shape (no new entity types — `ReferenceRangeSnapshot` is a Domain value object, **never registered** in EF); the permission seed; the pipeline behavior order; existing M17/M22 handlers and screens; `Hermes/M-12.md` and `OpenCode/M-12.md` (read-only inputs); anything under `Docs/Reference system files/`.

---

## 7. Detailed Implementation Plan (Slices S1–S8)

> Each slice carries: goal, affected files, changes, dependencies, exit criteria (verified before moving on), and tests. Naming rule for Application files: one folder per use case containing `<Name>Command.cs`, `<Name>CommandHandler.cs`, `<Name>CommandValidator.cs` (queries: `<Name>Query.cs`, `<Name>QueryHandler.cs`).

### S1 — Domain Behaviors, Invariant Guards, Snapshot Value Object

- **Goal:** complete the behavioral surface of the seven M-12 aggregates and add the freeze-contract value object.
- **Affected files (modify):** `src/TopLab.Domain/Tests/TestGroup.cs`, `WorkGroupLog.cs`, `WorkGroupLogItem.cs`, `TestComment.cs`, `ReferenceRange.cs`, `src/TopLab.Domain/Patients/PatientTitle.cs`. **Create:** `src/TopLab.Domain/Tests/ReferenceRangeSnapshot.cs`. **No change:** `Test.cs` (its `Create`/`Update` already cover FR-M12-002 exactly — D-2).
- **Changes:**
  - `TestGroup.Update(string name)` — guard `IsNullOrWhiteSpace`, trim (mirror `Create`).
  - `WorkGroupLog.Update(string name)` — same guard; `AddItem(WorkGroupLogItem)` — throw `InvalidOperationException` on duplicate `TestId`; `RemoveItem(TestId)` — throw if absent; `ClearItems()` — for the atomic-replace command (precedent: `User.ClearPermissions()`).
  - `WorkGroupLogItem` — parameterized ctor → `private`; add `static WorkGroupLogItem Create(WorkGroupLogId, TestId)`; keep the private parameterless ctor for EF. *Pre-check result at the snapshot: `grep -rn "new WorkGroupLogItem(" src tests` → 0 matches — the change is safe today; the executor must re-run this grep immediately before editing (D-6).*
  - `TestComment.Update(string commentText)` — guard + trim. **No M-12 Application command touches TestComment** (FR-M13-002 → M13).
  - `ReferenceRange.Update(...)` — **in-place** mutator with the same guards as `Create` (`AgeMin ≤ AgeMax`, `MinValue ≤ MaxValue`); plus `CaptureSnapshot()` returning a new `ReferenceRangeSnapshot`.
  - `ReferenceRangeSnapshot` — `sealed record` with `TestId TestId` (or `int` — implementation choice; record equality is the point), `Sex? Sex`, `AgeUnit AgeUnit`, `int AgeMin/AgeMax`, `decimal MinValue/MaxValue`, `string? LowComment/HighComment`, `DateTimeOffset CapturedAtUtc`. Immutable; **not** an EF entity; **never** added to any `DbSet` or configuration.
  - `PatientTitle.Update(string titleText, bool isDefault)` — guard + trim.
- **Dependencies:** none (pure Domain; compiles with zero packages).
- **Exit criteria:** `dotnet build src/TopLab.Domain` clean; new Domain tests green (below); no public API removed except the `WorkGroupLogItem` ctor (pre-check re-run documented in the working `M-12Loop.MD` file).
- **Tests (`tests/TopLab.Domain.Tests/`):** extend `Tests/TestCatalogTests.cs` and add `Tests/ReferenceRangeTests.cs`, `Tests/WorkGroupLogTests.cs`, `Patients/PatientTitleTests.cs`: BR-04 matrix (Day row matches 15/35 days, **not** Month request; Month row matches 1–12 months only; `Sex`-null row matches both sexes, `Sex=Male` row rejects Female); `Matches` boundary inclusivity (`ageValue == AgeMin/AgeMax` match); `Create`/`Update` guard throws (AgeMin > AgeMax, MinValue > MaxValue, empty names, zero duration, sent-out without price via `Test.Create`); `CaptureSnapshot` returns value-equal copy + `CapturedAtUtc` set and record immutability (init-only); `WorkGroupLog.AddItem` duplicate throws / `RemoveItem` absent throws / `ClearItems` empties; `WorkGroupLogItem.Create` sets both ids; `PatientTitle.Update` guards.

### S2 — Application Read Surface (Queries + DTOs)

- **Goal:** all read-side queries and DTOs for catalog, groups, work groups, ranges, titles.
- **Affected files (create, under `src/TopLab.Application/Features/TestCatalogAndReferenceRanges/`):**
  - `Common/TestCatalogDtos.cs` — `TestSummaryDto(Id, Name, ReportName, TestGroupId?, TestGroupName?, Barcode?, CompletionDurationMinutes, IsSentOut, PatientPrice, LabToLabPrice?)`; `TestDetailDto` (adds `ReceiptName`, `SentOutCostPrice?`, `ResultKind`, `IsCultureType`); `TestGroupDto(Id, Name)`; `WorkGroupLogDto(Id, Name, IReadOnlyList<WorkGroupLogItemDto>)` with `WorkGroupLogItemDto(TestId, TestName)`; `ReferenceRangeDto(Id, TestId, Sex?, AgeUnit, AgeMin, AgeMax, MinValue, MaxValue, LowComment?, HighComment?)`; `PatientTitleDto(Id, TitleText, IsDefault)`. Sealed records.
  - `Queries/GetTestCatalog/` — `GetTestCatalogQuery(string? SearchTerm = null, int? TestGroupId = null) : IRequest<Result<IReadOnlyList<TestSummaryDto>>>` + handler.
  - `Queries/GetTestById/` — query + handler (`Error.NotFound("التحليل غير موجود")`).
  - `Queries/GetTestGroups/`, `Queries/GetWorkGroupLogs/` (loads logs + items; resolves test names from `Set<Test>()`), `Queries/GetReferenceRanges/` (filter by `TestId`), `Queries/GetPatientTitles/`.
- **Search filter (FR-M12-001 — corrected versus both prior drafts):** the PRD names three axes: **test name, containing group, test number**. The handler therefore ORs: `Name.Contains(term)` ∨ `ReportName.Contains(term)` ∨ `Barcode.Contains(term)` (name-family axes) ∨ **`TestGroup.Name.Contains(term)` via the group navigation** ∨ numeric branch — `int.TryParse(term)` succeeded ⇒ `Id.Value == parsed`. Non-numeric term skips the numeric branch. (Prior draft `Docs/OpenCode/M-12.md` omitted the group-name axis its own CR-4 was meant to satisfy; this plan fixes it.) SQL Server default collation is case-insensitive for `nvarchar`, so `Contains` suffices — note this in code comments; no `EF.Functions.Like` gymnastics needed (that note in a prior draft assumed SQLite; the verified provider is **SQL Server**).
- **Read authorization decision (D-8):** follow the M17/M22 precedent — queries are **not** `IAuthorizedRequest` (e.g. `GetUsersQuery` isn't; the sensitive entry is gated at the shell by the secondary password). Reads stay open; writes are permission-gated.
- **Dependencies:** S1 (entity shapes). Parallelizable with S3 after S1.
- **Exit criteria:** `dotnet build src/TopLab.Application` clean; handler tests green; `FakeApplicationDbContext` **extended first** (see below).
- **Tests:** extend `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` — it currently supports only `Test` from the Tests family; add `List<TestGroup>`, `List<ReferenceRange>`, `List<TestComment>`, `List<WorkGroupLog>`, `List<WorkGroupLogItem>`, `List<PatientTitle>` (+ `Set/Add/Remove` branches; unsupported-type exception otherwise). Then `tests/TopLab.Application.Tests/Features/TestCatalogAndReferenceRanges/` query-handler tests: partial Arabic-name search; group-name search (the new axis); numeric search returns Id match + name coincidences; empty term = all; `TestGroupId` filter; not-found path; ranges filtered per test; work-group items resolve test names.

### S3 — Application Write Surface (Commands + Validators + Authorization)

- **Goal:** all write commands with guards, duplicate checks, FK validation, and `EDIT_SYSTEM_SETTINGS` gating.
- **Affected files (create, under `Features/TestCatalogAndReferenceRanges/Commands/`):**

| Use case | Command (result) | Handler responsibilities |
|---|---|---|
| `CreateTest` | `Result<int>` | Duplicate `Name` check → `Error.Conflict("اسم التحليل موجود بالفعل")`; `TestGroupId` FK check → `Error.NotFound("مجموعة التحاليل غير موجودة")`; `Test.Create(TestId.Create(0), …)` with full field set incl. `ResultKind`, `IsCultureType` (**sentinel 0** — see D-9); `_db.Add` + save; return generated `Id.Value` |
| `UpdateTest` | `Result` | Load by `Id.Value`; not-found → `"التحليل غير موجود"`; FK check; call existing bulk `Test.Update(...)`; save |
| `CreateTestGroup` / `UpdateTestGroup` | `Result<int>` / `Result` | Duplicate group name check; `TestGroup.Create(TestGroupId.Create(0), …)` / `Update` |
| `CreateWorkGroupLog` / `UpdateWorkGroupLog` | `Result<int>` / `Result` | Same shape for `WorkGroupLog` |
| `SaveWorkGroupLogItems` | `Result` | **Atomic replace** (precedent `SaveUserPermissionsCommandHandler`): load log (not-found → `"مجموعة العمل غير موجودة"`); validate every `TestId` exists (`"التحليل {id} غير موجود"`); `ClearItems()` + `_db.Remove(...)` per existing item row; `AddItem(WorkGroupLogItem.Create(...))` + `_db.Add(...)` per requested id; single `SaveChangesAsync` |
| `CreateReferenceRange` | `Result<int>` | Test FK check; `ReferenceRange.Create(ReferenceRangeId.Create(0), …)` |
| `UpdateReferenceRange` | `Result` | **In-place** `ReferenceRange.Update(...)` (D-10 — versioning rejected: no flag column exists and no migration is allowed); not-found → `"النطاق المرجعي غير موجود"` |
| `DeleteReferenceRange` | `Result` | Load live row; not-found path; `_db.Remove` + save. **Live row only** — nothing else is touched; historical results are unaffected (BR-05) because M-04's snapshot (future) is independent; the confirmation copy shown by the UI states this |
| `CreatePatientTitle` / `UpdatePatientTitle` | `Result<int>` / `Result` | Duplicate `TitleText` check; default handling — see E-7 |

- All commands implement `IAuthorizedRequest` with `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` (D-5). Validators per command (Arabic `WithMessage`), mirroring `CreateUserCommandValidator`:
  - `Create/UpdateTest`: names `NotEmpty` + `MaximumLength(150)`; `Barcode ≤ 50`; `CompletionDurationMinutes > 0` ("مدة الإنجاز يجب أن تكون أكبر من صفر"); prices `≥ 0` (`decimal(18,2)`); `Must(x => !x.IsSentOut || x.SentOutCostPrice.HasValue)` ("سعر الإرسال مطلوب عندما يكون التحليل صادرًا"); `ResultKind` `IsInEnum`.
  - `Create/UpdateReferenceRange`: `AgeMin ≥ 0`; `AgeMax ≥ AgeMin`; `MinValue ≤ MaxValue`; `AgeUnit`/`Sex` `IsInEnum`; comments `≤ 500`.
  - `SaveWorkGroupLogItems`: non-empty list; distinct ids.
- **Dependencies:** S1 (mutators/factories), S2 (folder + DTO context). Parallelizable with S2 after S1.
- **Exit criteria:** build clean; every command marked `IAuthorizedRequest`; handler tests + validator tests green; **R-1 verification executed** (validators actually resolved through the pipeline — see §14) and the registration fix applied if confirmed.
- **Tests:** `Features/TestCatalogAndReferenceRanges/` command-handler tests with the extended fake: duplicate name → Conflict; bad group FK → NotFound; create returns generated id and adds the row; update mutates in place and leaves `ResultKind`/`IsCultureType` untouched; `SaveWorkGroupLogItems` replaces atomically (pre-existing items gone, new set present, unknown test id fails with no partial save — fake's `SaveChangesCallCount` and list state assert atomicity); range create/update/delete happy + not-found; `IAuthorizedRequest` conformance asserted via the `AuthorizationBehavior` test pattern with `FakeCurrentUserService` (no permission → `Forbidden` with the exact Arabic message; with permission / `IsAbsolutePermission` → passes).

### S4 — Infrastructure & DI Verification (no schema work)

- **Goal:** prove the persistence layer needs nothing new, and close the validator-DI gap if real.
- **Affected files:** none created; **conditionally modify** `src/TopLab.Application/DependencyInjection.cs` (add `services.AddValidatorsFromAssembly(assembly)` from `FluentValidation.DependencyInjectionInjection` — **only if** the R-1 runtime verification confirms validators are currently unresolved; keep the change minimal and re-run the full suite). Modify `src/TopLab.Presentation/DependencyInjection.cs` only in S5/S6 (VM registrations).
- **Verification steps (exit criteria):**
  1. `dotnet ef migrations has-pending-model-changes` (or equivalent diff of the model snapshot) → **false**; zero new files under `Persistence/Migrations/`.
  2. Test asserting `modelBuilder` model contains **no** entity named `ReferenceRangeSnapshot` / no `ResultReferenceRangeSnapshot` table (inspect `modelBuilder.Model.GetEntityTypes()` via the existing `InMemoryContextFactory` harness — guards the freeze-contract boundary).
  3. Existing `F5ConfigurationTests` still green (they already cover `Test.PatientPrice` and `ReferenceRange.MinValue` precision).
  4. R-1: resolve `IValidator<CreateTestCommand>` from a host built exactly like `App.xaml.cs` builds it — if `null`, apply the registration fix and add a regression test (resolve a known validator non-null).
- **Dependencies:** S2/S3 (to have a validator to resolve). Fast; can run alongside S5 preparation.

### S5 — Presentation Batch 1: Test Catalog + Test Groups + Patient Titles

- **Goal:** the primary FR-M12-001/002 surface plus two FR-M12-003/System-pane configuration screens.
- **Affected files (create):**
  - `src/TopLab.Presentation/ViewModels/Tests/TestCatalogViewModel.cs` — two-column UX: right/left per RTL; search box (`SearchTerm`, live or button-triggered), optional group filter `ComboBox` (`TestGroups`, `SelectedTestGroupId`, "الكل" = null), `ObservableCollection<TestSummaryDto> Tests`, `SelectedTest`, detail/edit pane with staged edit fields (`EditName`, `EditReportName`, `EditReceiptName`, `EditGroup`, `EditBarcode`, `EditCompletionDuration`, `EditIsSentOut` gating `EditSentOutCostPrice` visibility, `EditPatientPrice`, `EditLabToLabPrice`), `IsEditMode`/`IsAddMode`, `IsBusy`, `ErrorMessage` via `ResultErrorPresenter`; commands `Search`, `LoadDetail`, `Add` (إضافة تحليل), `Edit` (تعديل), `Save` (حفظ → `CreateTestCommand` or `UpdateTestCommand` — **one command per save action**, D-2), `Cancel` (تراجع → discard staged fields, 0 dispatches), `OpenGroups` (مجموعات التحاليل), `OpenWorkGroups` (Log), `OpenReferenceRanges` (القيم المرجعية — enabled when a test is selected). Follow `UserManagementViewModel` structure; no direct handler calls — everything via `ISender`.
  - `src/TopLab.Presentation/Views/Tests/TestCatalogView.xaml(.cs)` — RTL UserControl, error `TextBlock` red/wrapped, buttons in PRD wording.
  - `ViewModels/Tests/TestGroupsViewModel.cs` + `Views/Tests/TestGroupsView.xaml(.cs)` — list + create + rename (تعديل/حفظ), back navigation. **No delete** (E-9: no PRD requirement; FK is SetNull so deletion semantics would need an owner decision — recorded as an open point, default = not offered).
  - `ViewModels/Tests/PatientTitlesViewModel.cs` + `Views/Tests/PatientTitlesView.xaml(.cs)` — list + create + edit + default toggle (ألقاب المرضى).
- **Affected files (modify):**
  - `src/TopLab.Presentation/DependencyInjection.cs` — register the three VMs (`AddTransient`).
  - `src/TopLab.Presentation/MainWindow.xaml` — add `xmlns` for the new VM/View namespaces + three `DataTemplate` mappings (copy the exact pattern of the `usersVm`/`settingsVm` blocks).
- **Dependencies:** S2+S3 complete (commands/queries exist), S4 registrations verified.
- **Exit criteria:** build clean; app launches; **manual smoke:** System → بيانات التحاليل opens after secondary password (wired in S7 — until then, navigation can be tested via a temporary direct branch or after S7 lands), search filters by name/group/number, add/edit/save/تراجع behave, groups and titles screens work; no binding errors in Debug output.
- **Tests:** ViewModel unit tests (see R-6 for project placement): with a fake `ISender` — `Search` sends `GetTestCatalogQuery` with expected term+group; `Save` in add mode sends `CreateTestCommand` with staged fields; in edit mode sends `UpdateTestCommand`; `Cancel` sends nothing and clears staged fields; busy guard prevents double-dispatch; failure results surface through `ResultErrorPresenter` into `ErrorMessage`.

### S6 — Presentation Batch 2: Work Groups (Log) + Reference Ranges Editor

- **Goal:** FR-M12-003 (Log) and FR-M12-004/005/006 range editor.
- **Affected files (create):**
  - `ViewModels/Tests/WorkGroupsViewModel.cs` + `Views/Tests/WorkGroupsView.xaml(.cs)` — left: work-group list + create/rename; right: checklist of all tests (`AvailableTests` from `GetTestCatalogQuery`) with checked state for membership of the selected group; `SaveItems` → `SaveWorkGroupLogItemsCommand` (atomic replace matches the checklist UX exactly).
  - `ViewModels/Tests/ReferenceRangesViewModel.cs` + `Views/Tests/ReferenceRangesView.xaml(.cs)` — opened from `TestCatalogViewModel.OpenReferenceRanges` for the selected test (test id/name carried via constructor parameters resolved from DI, or a navigation parameter pattern consistent with how `SettingsDashboardViewModel` passes `INavigationService` — pick the simplest consistent approach; both are precedented). DataGrid of ranges (`ReferenceRangeDto`); edit form: `Sex` ComboBox (ذكر/أنثى/كلاهما → null), **`AgeUnit` ComboBox per range (يوم/شهر/سنة) displayed alongside AgeMin–AgeMax so BR-04 is visually explicit**, numeric Min/Max value, optional Low/High comment; buttons إضافة قيمة مرجعية / تعديل → حفظ / حذف / تراجع / رجوع. Delete shows `ShowConfirmationAsync` with copy explaining historical results are unaffected (BR-05/FR-M12-006).
- **Affected files (modify):** `DependencyInjection.cs` (register 2 VMs), `MainWindow.xaml` (2 DataTemplates).
- **Dependencies:** S5 (navigation host: catalog → sub-screens). **Parallelizable with S5** if both teams coordinate on the shared `MainWindow.xaml`/`DependencyInjection.cs` merge.
- **Exit criteria:** build clean; manual smoke per §12 checklist; BR-04 visible in UI (unit column shown; no conversion attempted anywhere).
- **Tests:** VM unit tests: `SaveItems` sends exact `TestIds` list; range Save dispatches Create vs Update correctly by mode; Delete sends `DeleteReferenceRangeCommand` only after confirmation; validation state blocks Save when `AgeMax < AgeMin` client-side (server-side remains authoritative).

### S7 — Shell Wiring: Gated "النظام" Entry

- **Goal:** the production entry point, matching the established sensitive-surface pattern.
- **Affected files (modify):** `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` — in `BuildNavigationItems()`, replace the `"النظام"` fall-through with a dedicated branch:
  1. `bool ok = await _dialogs.ShowSecondaryPasswordDialogAsync();` — on `false`, return (navigation does not happen; current view stays).
  2. `_navigation.NavigateTo<ViewModels.Tests.TestCatalogViewModel>();`
  3. If `CurrentViewModel` is the catalog VM → `await vm.SearchAsync()` (initial load).
  - This mirrors the existing `"المستخدمون"` branch line-for-line (verified in the snapshot's `ShellViewModel.cs`) and the M22 Database Maintenance gate in `SettingsDashboardViewModel.OpenDatabaseMaintenanceAsync`. Rationale (adopted from prior draft CR-5, confirmed by two in-code precedents): sensitive System-pane surfaces in this product are secondary-password gated; skipping the gate would be inconsistent and would bypass the M17 prompt users expect.
- **Dependencies:** S5 (VM exists + registered).
- **Exit criteria:** manual smoke — cancel on the password dialog leaves the current view untouched; success opens the catalog with data loaded; **regression:** "المستخدمون" and "الإعدادات" entries still work.
- **Tests:** ShellViewModel unit test with a fake `IDialogService` (true/false cases) + fake `INavigationService` asserting `NavigateTo<TestCatalogViewModel>` called only on success and `SearchAsync` invoked; existing nav behavior for other entries unbroken.

### S8 — Hardening, End-to-End Acceptance, Documentation & Handoff

- **Goal:** Arabic-first error surface audit, edge-case pass, full-suite run, and the repository's documentation convention.
- **Work items:**
  1. Error-message audit — the canonical Arabic table (adopted, verified consistent with existing handler style): `التحليل غير موجود` / `اسم التحليل موجود بالفعل` / `مجموعة التحاليل غير موجودة` / `مجموعة العمل غير موجودة` / `النطاق المرجعي غير موجود` / `[الحقل] مطلوب` / `مدة الإنجاز يجب أن تكون أكبر من صفر` / `سعر الإرسال مطلوب عندما يكون التحليل صادرًا` / `أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام` (pipeline-owned, exact text per Test Strategy §7.1).
  2. Audit verification — integration-style test (Infrastructure test project, `InMemoryContextFactory` + interceptor): after `UpdateTestCommand`-equivalent save, the `Test` row's `ModificationCount` incremented by 1 and `LastModifiedByUserId`/`LastModifiedAtUtc` updated while `CreatedByUserId`/`CreatedAtUtc` untouched (the `AuditableEntitySaveChangesInterceptor` is the verified mechanism — it is the single audit writer; handlers never set audit fields).
  3. Full solution build + `dotnet test` — zero errors, zero new warnings (quality gate §6.1), all prior suites (≈268 tests at snapshot) still green — regression proof for M17/M22.
  4. Coverage floors (Test Strategy §4): Domain ≥ 90%, Application ≥ 80%, Infrastructure ≥ 70% — measured per project.
  5. Manual verification checklist executed and recorded (§12.4).
  6. Documentation: `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` M12 row → 🟩 Done with deliverables note; append **ADR-0028** (M-12 decisions: no-migration extension, freeze-contract ownership split M-12 Domain shape / M-04 persistence, in-place range update, atomic work-group items replace, PatientTitle scope, single `EDIT_SYSTEM_SETTINGS` gate) — sequential numbering continues from ADR-0027; author `Docs/Handoff_M12.md` per the 15-section `Top_Lab_Handoff_Template.md`; working files (`Module 12 Implementation Plan.MD`, `M-12Loop.MD`) remain untracked per convention.
- **Exit criteria:** all of the above; handoff signed.

---

## 8. File-by-File Change Map

### New files (18 src + tests)

```
src/TopLab.Domain/Tests/ReferenceRangeSnapshot.cs                                   (S1)
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Common/TestCatalogDtos.cs   (S2)
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetTestCatalog/{GetTestCatalogQuery.cs, GetTestCatalogQueryHandler.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetTestById/{GetTestByIdQuery.cs, GetTestByIdQueryHandler.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetTestGroups/{GetTestGroupsQuery.cs, GetTestGroupsQueryHandler.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetWorkGroupLogs/{GetWorkGroupLogsQuery.cs, GetWorkGroupLogsQueryHandler.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetReferenceRanges/{GetReferenceRangesQuery.cs, GetReferenceRangesQueryHandler.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetPatientTitles/{GetPatientTitlesQuery.cs, GetPatientTitlesQueryHandler.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/CreateTest/{CreateTestCommand.cs, CreateTestCommandHandler.cs, CreateTestCommandValidator.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/UpdateTest/{UpdateTestCommand.cs, UpdateTestCommandHandler.cs, UpdateTestCommandValidator.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/CreateTestGroup/{...3 files}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/UpdateTestGroup/{...3 files}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/CreateWorkGroupLog/{...3 files}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/UpdateWorkGroupLog/{...3 files}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/SaveWorkGroupLogItems/{...3 files}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/CreateReferenceRange/{...3 files}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/UpdateReferenceRange/{...3 files}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/DeleteReferenceRange/{DeleteReferenceRangeCommand.cs, DeleteReferenceRangeCommandHandler.cs}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/CreatePatientTitle/{...3 files}
src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Commands/UpdatePatientTitle/{...3 files}
src/TopLab.Presentation/ViewModels/Tests/{TestCatalogViewModel.cs, TestGroupsViewModel.cs, WorkGroupsViewModel.cs, ReferenceRangesViewModel.cs, PatientTitlesViewModel.cs}
src/TopLab.Presentation/Views/Tests/{TestCatalogView.xaml(.cs), TestGroupsView.xaml(.cs), WorkGroupsView.xaml(.cs), ReferenceRangesView.xaml(.cs), PatientTitlesView.xaml(.cs)}
tests/TopLab.Domain.Tests/Tests/{ReferenceRangeTests.cs, WorkGroupLogTests.cs}
tests/TopLab.Domain.Tests/Patients/PatientTitleTests.cs
tests/TopLab.Application.Tests/Features/TestCatalogAndReferenceRanges/...   (handler/validator/authorization test files per use case)
tests/TopLab.Presentation.Tests/   (OPTIONAL new project — see R-6)
```

### Modified files (11 src + docs)

```
src/TopLab.Domain/Tests/TestGroup.cs                    (S1: add Update)
src/TopLab.Domain/Tests/WorkGroupLog.cs                 (S1: Update/AddItem/RemoveItem/ClearItems)
src/TopLab.Domain/Tests/WorkGroupLogItem.cs             (S1: ctor private + Create factory)
src/TopLab.Domain/Tests/TestComment.cs                  (S1: add Update — courtesy for M13)
src/TopLab.Domain/Tests/ReferenceRange.cs               (S1: add Update + CaptureSnapshot)
src/TopLab.Domain/Patients/PatientTitle.cs              (S1: add Update)
src/TopLab.Application/DependencyInjection.cs           (S4 — CONDITIONAL: AddValidatorsFromAssembly, only if R-1 confirms the gap)
src/TopLab.Presentation/DependencyInjection.cs          (S5/S6: register 5 VMs)
src/TopLab.Presentation/MainWindow.xaml                 (S5/S6: xmlns + 5 DataTemplates)
src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs  (S7: النظام branch — gate + navigate + initial search)
tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs  (S2: add 6 entity lists + Set/Add/Remove branches)
tests/TopLab.Domain.Tests/Tests/TestCatalogTests.cs     (S1: extend)
Docs/Source/Top_Lab_Master_Tracking_Sheet.md            (S8: M12 row → Done)
Docs/Source/Top_Lab_ADR.md                              (S8: append ADR-0028)
Docs/Handoff_M12.md                                     (S8: new, committed)
```

### Explicitly NOT modified

`src/TopLab.Domain/Tests/Test.cs` (existing Create/Update already sufficient — D-2); all EF configurations and `ApplicationDbContext*` (no model change); `PermissionConfiguration.cs` (seed already contains `EDIT_SYSTEM_SETTINGS`); `App.xaml.cs`; all M17/M22 source files; `Directory.Packages.props` (no new packages); `Docs/Hermes/M-12.md`; `Docs/OpenCode/M-12.md`; everything under `Docs/Reference system files/`.

---

## 9. Data / API / Contract Changes

- **Database schema: NONE.** All seven tables, columns, indexes, FK behaviors, and `IDENTITY` PKs already exist in the F5 baseline (verified in `20260828052248_BaselineDataModel.cs` + snapshot). No migration is generated; a verification test/assertion guards this (§7 S4).
- **"API" surface (internal MediatR contract):** 7 queries + 12 commands as enumerated in §7 S2/S3 — this is the contract downstream modules will call. Notable contract decisions binding on consumers:
  - `GetTestCatalogQuery(SearchTerm, TestGroupId)` — search axes: name-family (Name/ReportName/Barcode), **containing group name**, and numeric test number.
  - `SaveWorkGroupLogItemsCommand(WorkGroupLogId, IReadOnlyList<int> TestIds)` — atomic full replacement (diff-based add/remove is NOT offered; the checklist UI and the `SaveUserPermissions` precedent both fit replace semantics).
  - `UpdateReferenceRangeCommand` — **in-place** update of the live row; there is no versioning and no soft-delete. Historical-result protection comes exclusively from M-04's future snapshot persistence (BR-05), not from M-12 row semantics.
  - `DeleteReferenceRangeCommand` — hard delete of the live row only.
  - Freeze-contract handoff to M-04: `ReferenceRange.CaptureSnapshot()` → `ReferenceRangeSnapshot` (Domain VO) is the shape M-04 will capture at result entry and persist on its own aggregate. M-12 registers nothing for it in EF.
- **Permission contract:** every M-12 command requires `EDIT_SYSTEM_SETTINGS`; queries are open (shell-level secondary-password gate protects the surface) — matching M17/M22 behavior (D-5/D-8).
- **Configuration/environment: NONE.** No appsettings keys, no connection changes, no new hosted services.

---

## 10. Dependency Graph and Execution Order

```
S1 (Domain)
 ├── S2 (Application read surface)  ┐
 └── S3 (Application write surface) ┘ both depend on S1; S2 ∥ S3 allowed
        └── S4 (Infra/DI verification + conditional validator fix)   [depends on S3 existing; short]
              ├── S5 (UI: Catalog + Groups + Titles)   ┐ S5 ∥ S6 allowed (coordinate shared-file merges)
              └── S6 (UI: WorkGroups + Ranges editor)  ┘
                     └── S7 (Shell wiring: gated النظام entry)      [depends on S5]
                            └── S8 (Hardening, acceptance, docs, handoff)
```

Mandatory sequencing: S1 before S2/S3 (handlers call the new domain methods); S2+S3 before S5/S6 (VMs dispatch real commands); S5 before S7 (target VM must exist); S8 last. Tests are written **with** each slice, not deferred to S8.

**Recommended single-agent order:** S1 → S2 → S3 → S4 → S5 → S7 (wire the gate as soon as the catalog exists, enabling end-to-end manual verification earlier) → S6 → S8.

---

## 11. Parallelisable Work

- **S2 ∥ S3** after S1 lands (read and write surfaces share only the DTO/Common folder; agree on `TestCatalogDtos.cs` content first or let S2 own it).
- **S5 ∥ S6** after S4 — independent ViewModels/Views; the only merge conflicts are `DependencyInjection.cs` and `MainWindow.xaml` (trivial additive merges; sequence the shared-file edits or have one stream own them).
- **Test-authoring ∥ slice implementation** within each slice (different files).
- **Not parallelisable:** S1 (everything depends on it), S7 (needs S5), S8 (needs all).
- M-12 is also fully independent of M-14 (Wave-2 sibling) per the dependency map — they can proceed in parallel work streams without coordination, sharing only the tracking-sheet/ADR merge points in S8.

---

## 12. Testing and Verification Strategy

Aligned to `Docs/Source/Top_Lab_Test_Strategy.md` (binding gates §6; per-module items §7.2 line 232–235; coverage floors §4).

### 12.1 Unit — Domain (`tests/TopLab.Domain.Tests/`)
Per §7 S1: BR-04 `Matches` matrix (the two §7.2 module-specific checklist items — age-unit sensitivity and the presence/optionality of low/high comments at boundaries — are pinned here at the Domain level); all `Create`/`Update` guard exceptions; `CaptureSnapshot` value-equality + immutability; work-group membership semantics; `PatientTitle.Update` guards. Builders per Test Strategy §5 (e.g. a `TestBuilder.SimpleTest("CBC")`-style helper) instead of scattered initializers.

### 12.2 Unit — Application (`tests/TopLab.Application.Tests/`)
Handler tests against the **extended** `FakeApplicationDbContext` (deterministic fakes only — `FakeCurrentUserService`, `FakeDateTimeProvider`; no `DateTime.UtcNow` in assertions). Cover: every happy path; every Arabic error path (duplicate, not-found, FK); the numeric-vs-text and group-name search branches; atomic `SaveWorkGroupLogItems` (no partial state on mid-list failure); authorization via the behavior pattern (forbidden vs granted vs absolute). Validator tests: each rule + message.

### 12.3 Infrastructure (`tests/TopLab.Infrastructure.Tests/`)
- Reuse `InMemoryContextFactory`/`TestApplicationDbContext`: model assertions — no snapshot entity registered; existing configuration tests green; `has-pending-model-changes` negative.
- Interceptor verification relevant to M-12 entities: `Test`/`TestGroup`/`ReferenceRange` rows get audit columns populated; `ModificationCount` increments on modify (S8 item 2).
- **Where a real database behavior matters (IDENTITY generation with the `Create(0)` sentinel, FK cascade on range delete, SetNull on group delete), Test Strategy §3.3 nominates an ephemeral SQL Server (LocalDB) test.** The current codebase practice is InMemory (which does **not** simulate identity — see R-3), so: add a LocalDB-guarded integration test for insert-with-sentinel-id returning a database-generated id, skipped gracefully when no LocalDB is available (precedent-consistent, honest about environment limits), and record in the handoff which environments ran it.

### 12.4 Presentation & manual verification
- Test Strategy §3.4: ViewModels via unit tests with fakes; **no UI-automation framework is mandated** (this plan therefore rejects the FlaUI/WinAppDriver gates proposed in a prior draft — they contradict the repo's own strategy and would add an unpinned dependency).
- **R-6 (project placement):** no `TopLab.Presentation.Tests` project exists at the snapshot (sln lists exactly three test projects). Test Strategy §3.4 mandates ViewModel unit tests, so M-12 should **create `tests/TopLab.Presentation.Tests`** (xUnit, net8.0-windows, references Presentation; fake `ISender`/`IDialogService`/`INavigationService`) and add it to `TopLab.sln` — a small, convention-following addition. If the owner vetoes the new project, record a waiver in the handoff (Test Strategy §4) and cover the VM logic through code review + manual checklist only.
- **Manual checklist (§7-style, executed in S8):** System → بيانات التحاليل opens only after secondary password; search by Arabic partial name / by group name / by test number; group filter; add test → appears in list; edit 3 fields → save → values persist; تراجع discards; sent-out flag reveals/hides price field and blocks save without it; groups create/rename; Log create + assign tests + save + reopen shows assignment; ranges: add Day-row (e.g. 1–60 days) → verify; add Month-row; edit in place; delete with confirmation; patient titles create/edit/default; error paths show Arabic messages (no stack traces); RTL layout and tab order sane; permission-denied user (no `EDIT_SYSTEM_SETTINGS`, non-absolute) sees the exact denial message on any save attempt.

### 12.5 Regression
Full `dotnet test` across all test projects after each slice merge and at S8; the ~268 pre-existing tests (M17/M22) must remain green throughout — the only permitted existing-file behavioral change is the `WorkGroupLogItem` ctor visibility (compile-safe: zero call sites verified) and the conditional validator registration (additive; guarded by a regression test).

---

## 13. Edge Cases and Failure Handling

| # | Case | Handling |
|---|---|---|
| E-1 | Search term is numeric ("42") | OR-match `Id.Value == 42` **plus** name-family and group-name contains — a test named "B42" is still findable |
| E-2 | Search term empty/whitespace | Returns all tests (filter skipped) |
| E-3 | Range with `Sex == null` | Applies to both sexes (`Matches` already handles) |
| E-4 | AgeMax < AgeMin, MinValue > MaxValue, AgeMin < 0 | Validator + domain guard both reject (defense in depth) |
| E-5 | Sent-out test without price | Validator ("سعر الإرسال مطلوب…") + `Test.Create` guard |
| E-6 | Duplicate test name | Handler `Error.Conflict`; **no unique index exists** → residual race window between check and insert (R-4) |
| E-7 | Two PatientTitles both `IsDefault = true` | Schema has no constraint. Handler-level rule (D-11, an explicit design decision recorded for owner confirmation): setting a title as default clears `IsDefault` on all others in the same `SaveChangesAsync`; `GetPatientTitles` may surface the single default. If the owner wants DB-level enforcement it would require a migration — out of M-12 scope |
| E-8 | Delete a ReferenceRange that historical results relied on | Live row removed only; BR-05 guarantee comes from M-04's snapshot (future). Until M-04 lands, deleting a range means future evaluations can't match it — **flag in the delete-confirmation dialog copy** and in the handoff as a known cross-module timing note |
| E-9 | Delete a Test / TestGroup | **Not offered** — no PRD requirement for test deletion exists in §M12; TestGroup FK is SetNull so a group delete would silently null tests' group. Both recorded as open points for the owner (default: no delete buttons) |
| E-10 | `SaveWorkGroupLogItems` with an unknown TestId | Fail whole command before any save (validate all ids first) — no partial replacement |
| E-11 | `WorkGroupLogItem` duplicate (same log + test) | Domain `AddItem` throws; composite PK also backs it at the DB level |
| E-12 | Cancel on secondary-password gate | Navigation does not happen; current view unchanged (asserted in S7 test) |
| E-13 | Concurrent edits to the same Test | Last-writer-wins on columns; audit interceptor records `ModificationCount` growth. No concurrency token exists in the schema (adding one = migration = out of scope); noted as R-5 |
| E-14 | InMemory test provider vs IDENTITY | InMemory does not generate ids — Application tests use the list-based fake (ids irrelevant); identity behavior verified only in the LocalDB-guarded Infrastructure test (R-3) |

---

## 14. Risks and Mitigations

| # | Risk | Evidence / status | Mitigation |
|---|---|---|---|
| **R-1** | **Validators never execute at runtime** — no `AddValidatorsFromAssembly` (or per-validator registration) exists anywhere in `src/` at the snapshot; `ValidationBehavior` takes `IValidator<TRequest>?` (optional, defaults null). Existing validator tests construct behaviors manually, so the gap is invisible to the suite. | Verified by exhaustive grep of all `DependencyInjection.cs` + `App.xaml.cs`. **Classification: strongly supported conclusion, not yet runtime-proven** (the host could theoretically resolve validators via another mechanism not found — unlikely). | S4 step 4 resolves a validator from a host built like `App.xaml.cs`; if null → add `AddValidatorsFromAssembly` in `AddApplication` + regression test. This also silently benefits M17/M22. |
| R-2 | `WorkGroupLogItem` ctor privatization breaks hidden callers | Pre-check executed at the snapshot: **0 call sites** | Re-run the grep immediately before the edit; the compiler is the final guard (any missed caller fails the build) |
| R-3 | `Create(0)` sentinel + `IDENTITY` mismatch at runtime (existing `CreateUserCommandHandler` uses `Max()+1`, an inconsistent precedent that would fail on real SQL Server identity columns) | Migration proves IDENTITY on all M-12 tables; `Max()+1` precedent exists in Users (pre-M-12 code) | M-12 uses the sentinel `Create(0)` exclusively; LocalDB integration test proves database-generated ids; do NOT copy the `Max()+1` pattern; handoff notes the Users inconsistency for a future fix (out of M-12 scope) |
| R-4 | Duplicate-name race (no unique index on `Tests.Name`) | Configuration verified (non-unique index only) | Handler check + document residual race; DB-level fix would need a migration (out of scope); acceptable for a single-workstation desktop LIS |
| R-5 | No optimistic concurrency on Test rows | No `RowVersion`/concurrency token in schema | Accepted (schema frozen); audit `ModificationCount` gives traceability; recorded in ADR-0028 |
| R-6 | No Presentation test project exists | `TopLab.sln` verified (3 test projects) | Create `tests/TopLab.Presentation.Tests` (recommended) or record a Test Strategy §4 waiver in the handoff |
| R-7 | RTL/WPF XAML binding mistakes (DataTemplate wiring is the classic failure point) | M22 history shows the pattern works | Copy the exact `MainWindow.xaml` template blocks; manual smoke per slice; watch Debug binding output |
| R-8 | Search case-sensitivity assumptions | SQL Server default collation is case-insensitive for `nvarchar` (a prior draft's `EF.Functions.Like` note assumed SQLite — wrong provider) | Plain `Contains` is correct; add a comment; if a deployment uses a case-sensitive collation, revisit in a future decision |
| R-9 | Scope creep into M-13 (TestComment UI/commands) or M-04 (snapshot persistence) | PRD ownership is explicit (FR-M13-002, FR-M04-008) | This plan hard-gates both: TestComment gets a domain method only; snapshot gets a Domain VO only; regression test asserts no EF entity for the snapshot |
| R-10 | Validator/permission drift (inventing codes like `config:ref-ranges` — proposed in a prior draft) | Permission catalog is seeded, fixed at 13 codes; new codes require a migration (forbidden) | Single `EDIT_SYSTEM_SETTINGS` for all M-12 commands (D-5) |

---

## 15. Acceptance Criteria (module Done)

A coding agent or team may mark M-12 Done only when **all** of the following hold (Test Strategy §8 adapted):

1. **Functional:** FR-M12-001 … FR-M12-006 demonstrable end-to-end on a workstation: gated System entry → catalog with three-axis search → test add/edit/save/تراجع → groups + Log configuration (incl. test assignment) → per-test range add/edit-in-place/delete with age-unit semantics → patient-title management with default handling.
2. **Business rules pinned by tests:** BR-04 matrix green (`Matches` never converts units; day-range ≠ month patient); BR-05 boundary honored (no M-12 persistence of snapshots; no snapshot entity in the EF model — asserted); BR-13 data (`CompletionDurationMinutes > 0`) enforced at validator + domain.
3. **Test Strategy §7.2 M12 items:** age-unit sensitivity verified; low/high comments present, optional, and stored at the corresponding boundary (report *rendering* itself is M04/M05 scope — boundary recorded in the handoff).
4. **Gates (§6):** compile 0 errors/0 new warnings; **all** tests green in every project (incl. the ≈268 pre-existing); coverage floors met (Domain 90 / Application 80 / Infrastructure 70); dependency-rule gate (no layer violations — Domain stays package-free; Presentation references Application only); convention gate (folder/namespace/naming per coding standards); migration gate (no new migration, verified).
5. **Authorization & audit:** every M-12 command requires `EDIT_SYSTEM_SETTINGS`; denial shows the exact mandated Arabic message; audit interceptor populates/increments audit columns on Test/TestGroup/ReferenceRange/WorkGroupLogItem/PatientTitle writes (test-verified).
6. **Freeze-contract boundary:** `ReferenceRangeSnapshot` exists as a Domain VO with `CaptureSnapshot()`; nothing else — no table, no command, no persistence (test-verified).
7. **Documentation:** Master Tracking Sheet M12 row 🟩 with deliverables note; ADR-0028 appended; `Docs/Handoff_M12.md` completed per template (including deviations/waivers if R-1/R-6 resolutions warrant any); manual checklist results recorded.
8. **No collateral change:** `git diff` vs. the pre-M-12 baseline touches only the files in §8; `Docs/Reference system files/` untouched; `Hermes/M-12.md` and `OpenCode/M-12.md` untouched.

---

## 16. Final End-to-End Execution Sequence

```
0. Pre-flight   — checkout the M-12 baseline branch; re-baseline: dotnet build TopLab.sln (0/0) + dotnet test (all green ≈268);
                  re-run the WorkGroupLogItem call-site grep (expect 0); author untracked working docs
                  (Module 12 Implementation Plan.MD / M-12Loop.MD) per repository convention.
1. S1           — Domain methods + ReferenceRangeSnapshot + domain unit tests; build + Domain tests green.
2. S2 ∥ S3      — extend FakeApplicationDbContext first, then queries/DTOs and commands/validators
                  (+ their tests) in either order or parallel; Application tests green.
3. S4           — no-pending-model-changes check; no-snapshot-entity test; R-1 validator resolution check
                  (+ conditional AddValidatorsFromAssembly fix + regression test); LocalDB-guarded identity test.
4. S5 (then S7) — catalog/groups/titles VMs+views, DI + DataTemplates; wire the gated النظام branch;
                  manual smoke of the full entry → search → edit loop.
5. S6           — work-groups + reference-ranges editor VMs+views (+ tests); manual smoke; BR-04 visual check.
6. S8           — Arabic message audit; interceptor audit test; full dotnet test; coverage floors;
                  manual checklist §12.4; tracking sheet → Done; ADR-0028; Handoff_M12.md; sign-off.
```

Every transition is gated by the previous step's exit criteria (§7). If any verification fails, the slice is not complete — fix before proceeding.

---

## Appendix A — Evidence-Resolved Decision Log

| # | Decision | Evidence at commit `3ce602b` |
|---|---|---|
| D-1 | PatientTitle **in** M-12 scope | `Top_Lab_Master_Tracking_Sheet.md` M12 notes line ("delivers … `PatientTitle` …") + post-M22 stage doc §7; PRD FR-M02-007 places the *screen* in the System pane |
| D-2 | Keep the **single bulk `Test.Update`** (reject per-field mutators + `TestChangedEvent`) | `Test.Update` already exists and its parameter list equals FR-M12-002's field set exactly; **zero domain-event infrastructure exists** (grep: no `DomainEvent`/`INotification`/MediatR in Domain) and Architecture §3 forbids Domain package references, so a MediatR-based event record is architecturally impossible without a new pattern |
| D-3 | Snapshot = pure Domain VO; M-04 owns persistence | `PatientTest` verified to carry no range fields; no snapshot table exists; no-migration constraint |
| D-4 | **No repositories / no IUnitOfWork** — handlers use `IApplicationDbContext` directly | `IUnitOfWork` grep = 0 matches; `IApplicationDbContext` exposes `Set/Add/Update/Remove/SaveChangesAsync`; M17/M22 handlers are the precedent (a prior draft's repository layer does not exist in this codebase and would be a new pattern) |
| D-5 | Single permission `EDIT_SYSTEM_SETTINGS` for all M-12 commands | Seeded permission catalog (13 fixed codes, `HasData` in `PermissionConfiguration`); new codes would require a migration |
| D-6 | `WorkGroupLogItem` ctor → private + `Create` factory; **pre-check passed** | `grep -rn "new WorkGroupLogItem(" src tests` = 0 matches at the snapshot |
| D-7 | No FlaUI/WinAppDriver UI-automation gates | Test Strategy §3.4: "No UI-automation layer is mandated" — ViewModel unit tests + manual checklists |
| D-8 | Queries not permission-marked; surface protected by the shell's secondary-password gate | `GetUsersQuery` (M17) is not `IAuthorizedRequest`; gated nav entries are the precedent |
| D-9 | Insert ids via strongly-typed **sentinel `Create(0)`** (reject `Max()+1`) | `IDENTITY(1,1)` + `ValueGeneratedOnAdd` verified on every M-12 table in the F5 migration and model snapshot; `Max()+1` (used by the older `CreateUserCommandHandler`) conflicts with identity columns on real SQL Server — do not replicate |
| D-10 | `UpdateReferenceRange` = **in-place update** (reject new-version-row + superseded flag) | `ReferenceRanges` table has no version/superseded column (verified in migration); adding one = migration = forbidden; BR-05 protection is M-04's snapshot, not M-12 row semantics |
| D-11 | PatientTitle "at most one default" enforced in the handler, not the schema | No unique/partial index exists; no migration allowed; flagged for owner confirmation |

## Appendix B — Uncertainties (explicitly unresolved)

- **U-1 (R-1):** whether validators resolve at runtime today — strongly indicated **no** by static evidence; must be settled by the S4 runtime check before M-12's validation gates can be trusted.
- **U-2:** whether the deployment SQL Server collation is case-insensitive (default assumption: yes). Affects only search case behavior; harmless under the default.
- **U-3:** LocalDB availability on CI/build agents for the identity integration test — the test is environment-guarded; handoff must record where it actually ran.
- **U-4 (E-9):** whether the owner wants Test/TestGroup deletion in a future change — no PRD requirement exists; M-12 ships without delete and records the open point.
- **U-5 (D-11):** single-default PatientTitle semantics (handler-enforced here) — owner may prefer multiple defaults or a different rule; schema change out of scope.
- No information from `Docs/Reference system files/` was consulted; any requirement that exists only there is, by mandate, outside this plan's knowledge.

---

*End of document. Produced from a verified checkout of commit `3ce602bee79653edc674be7939df899349853ef8` (El-ogra/Top-Lab). All file paths, line-level claims, grep results, and migration facts above were read directly from that snapshot.*
