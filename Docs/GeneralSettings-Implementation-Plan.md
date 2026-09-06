# General Settings - Implementation Plan

## Module Overview

This module adds a **GeneralSettings** entity to the TopLab system — a typed key-value store for miscellaneous configuration flags and values that don't belong in the existing strongly-typed settings classes (SystemSettings, ReportSettings, etc.). This replaces the previous plan's incorrect approach (REST API + Blazor) with the correct WPF desktop app patterns: MediatR commands/queries, WPF ViewModel/View, FakeApplicationDbContext testing, and IAuthorizedRequest authorization.

The entity uses **typed properties** (not raw string key-value) with an allowlist of valid setting keys. Each key maps to a string value, with domain validation ensuring only known keys are persisted. A unique index on `SettingKey` prevents duplicates at the database level.

## Architecture

**Pattern**: Clean Architecture with vertical feature slices under `Features/GeneralSettings/` in the Application layer, matching the existing `SystemAndPrintSettings` feature.

**Layers touched**:
- **Domain**: New `GeneralSetting` entity with typed key enum + string value + concurrency token
- **Application**: Query/Command handlers, DTOs, validators, IAuthorizedRequest
- **Infrastructure**: EF Core entity configuration + migration
- **Presentation**: WPF ViewModel + UserControl View
- **Tests**: Handler tests with FakeApplicationDbContext, authorization tests, validator tests

**Concurrency strategy**: Optimistic concurrency via `RowVersion` (byte[] concurrency token). The handler wraps the update in a try/catch for `DbUpdateConcurrencyException` and returns a `Result.Failure` with an `Error.Conflict` error, which the ViewModel presents as "This setting was modified by another user. Please reload and try again."

## Slice Breakdown

### Slice 1: Domain Entity + Application Query/Command Layer

**Description**: Creates the `GeneralSetting` entity (typed key enum + string value + RowVersion), the EF Core configuration, the `IApplicationDbContext` wiring, and the MediatR query (`GetGeneralSettingsQuery`) and command (`UpdateGeneralSettingCommand`) with their handlers, DTOs, and validators. Also adds the `FakeApplicationDbContext` support and authorization tests.

**Dependencies**: None

**Technical Approach**:

1. **Domain — `GeneralSetting` entity** (`src/TopLab.Domain/Settings/GeneralSetting.cs`):
   - `GeneralSettingKey` enum with allowlisted values (e.g., `ShowPatientIdOnReport`, `EnableVoiceNotifications`, `DefaultLanguage`)
   - Properties: `GeneralSettingKey Key`, `string Value`, `byte[] RowVersion` (concurrency token)
   - Constructor validates key is in the enum; `SetValue(string)` validates value length (max 500 chars) and trims
   - Inherits `Entity<GeneralSettingKey>` for strongly-typed identity

2. **Domain — `GeneralSettingKey` enum** (`src/TopLab.Domain/Common/Enums/GeneralSettingKey.cs`):
   - Initial allowlist: `ShowPatientIdOnReport`, `EnableVoiceNotifications`, `DefaultLanguage`, `AutoLockTimeoutMinutes`, `MaxRecentPatients`
   - Extensible — new keys added here propagate through the entire stack

3. **Infrastructure — EF configuration** (`src/TopLab.Infrastructure/Persistence/Configurations/GeneralSettingConfiguration.cs`):
   - `HasKey(g => g.Key)` — natural PK from enum
   - `Property(g => g.Value).HasMaxLength(500).IsRequired()`
   - `Property(g => g.RowVersion).IsRowVersion()` — optimistic concurrency
   - Seed with default values for each enum member

4. **Infrastructure — Migration**:
   - `dotnet ef migrations add AddGeneralSettings --project src/TopLab.Infrastructure --startup-project src/TopLab.Presentation`
   - Creates `GeneralSettings` table with PK on `Key`, `Value` nvarchar(500), `RowVersion` rowversion

5. **Application — `IApplicationDbContext` update**:
   - Add `IQueryable<GeneralSetting> GeneralSettings` property (or verify existing `Set<GeneralSetting>()` works with the new entity)
   - Update `FakeApplicationDbContext` to include `List<GeneralSetting> GeneralSettings` and its `Set<T>` / `Add<T>` / `Remove<T>` cases

6. **Application — `GetGeneralSettingsQuery`** (`src/TopLab.Application/Features/GeneralSettings/Queries/GetGeneralSettings/`):
   - Returns `Result<IReadOnlyList<GeneralSettingDto>>` — all settings as key-value pairs
   - DTO: `GeneralSettingDto(GeneralSettingKey Key, string Value)`
   - No authorization required (read-only, all users can view)

7. **Application — `UpdateGeneralSettingCommand`** (`src/TopLab.Application/Features/GeneralSettings/Commands/UpdateGeneralSetting/`):
   - Record: `UpdateGeneralSettingCommand(GeneralSettingKey Key, string Value) : IRequest<Result>, IAuthorizedRequest`
   - `RequiredPermissionCode => "EDIT_GENERAL_SETTINGS"`
   - Handler: loads entity by key, calls `SetValue(request.Value)`, calls `SaveChangesAsync()`
   - Catches `DbUpdateConcurrencyException` → returns `Result.Failure(Error.Conflict("Setting was modified by another user. Please reload."))`

8. **Application — `UpdateGeneralSettingCommandValidator`**:
   - `RuleFor(x => x.Value).NotEmpty().MaximumLength(500)`
   - `RuleFor(x => x.Key).IsInEnum()` — only valid enum values allowed

9. **Tests — Handler tests** (`tests/TopLab.Application.Tests/Features/GeneralSettings/`):
   - `GetGeneralSettingsQueryHandlerTests`: seed 3 settings, verify returns all; empty DB returns empty list
   - `UpdateGeneralSettingCommandHandlerTests`: update existing setting; update non-existent key returns Unexpected; value too long returns validation error (via validator)
   - `GeneralSettingsAuthorizationTests`: verify `EDIT_GENERAL_SETTINGS` permission required; absolute user bypasses

**Files to Create/Modify**:
- `src/TopLab.Domain/Settings/GeneralSetting.cs` (new)
- `src/TopLab.Domain/Common/Enums/GeneralSettingKey.cs` (new)
- `src/TopLab.Infrastructure/Persistence/Configurations/GeneralSettingConfiguration.cs` (new)
- `src/TopLab.Infrastructure/Migrations/[timestamp]_AddGeneralSettings.cs` (generated)
- `src/TopLab.Application/Features/GeneralSettings/Queries/GetGeneralSettings/GetGeneralSettingsQuery.cs` (new)
- `src/TopLab.Application/Features/GeneralSettings/Queries/GetGeneralSettings/GetGeneralSettingsQueryHandler.cs` (new)
- `src/TopLab.Application/Features/GeneralSettings/Commands/UpdateGeneralSetting/UpdateGeneralSettingCommand.cs` (new)
- `src/TopLab.Application/Features/GeneralSettings/Commands/UpdateGeneralSetting/UpdateGeneralSettingCommandHandler.cs` (new)
- `src/TopLab.Application/Features/GeneralSettings/Commands/UpdateGeneralSetting/UpdateGeneralSettingCommandValidator.cs` (new)
- `src/TopLab.Application/Features/GeneralSettings/Common/GeneralSettingDtos.cs` (new)
- `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` (modify — add GeneralSettings list)
- `tests/TopLab.Application.Tests/Features/GeneralSettings/GetGeneralSettingsQueryHandlerTests.cs` (new)
- `tests/TopLab.Application.Tests/Features/GeneralSettings/UpdateGeneralSettingCommandHandlerTests.cs` (new)
- `tests/TopLab.Application.Tests/Features/GeneralSettings/GeneralSettingsAuthorizationTests.cs` (new)

**Risk Level**: Medium
**Estimated Complexity**: Medium

---

### Slice 2: WPF Presentation Layer (ViewModel + View + Navigation)

**Description**: Creates the WPF ViewModel and UserControl for the General Settings screen, wires it into the existing navigation system, and integrates it with the Settings Dashboard.

**Dependencies**: Slice 1 (Application layer must be compilable)

**Technical Approach**:

1. **ViewModel** (`src/TopLab.Presentation/ViewModels/Settings/GeneralSettingsViewModel.cs`):
   - Extends `ViewModelBase` (INotifyPropertyChanged)
   - Properties: `IReadOnlyList<GeneralSettingItemViewModel> Settings` (ObservableCollection)
   - Each item: `GeneralSettingKey Key`, `string Value` (bindable), `string OriginalValue` (for dirty tracking)
   - Commands: `LoadCommand` (AsyncRelayCommand), `SaveCommand` (AsyncRelayCommand), `BackToDashboardCommand` (RelayCommand)
   - `LoadAsync()`: sends `GetGeneralSettingsQuery`, populates items
   - `SaveAsync()`: iterates changed items, sends `UpdateGeneralSettingCommand` for each, handles `Result.Failure` (Conflict → show message, Validation → show message)
   - `IsBusy` / `ErrorMessage` / `StatusMessage` following existing pattern from `SystemSettingsViewModel`

2. **View** (`src/TopLab.Presentation/Views/Settings/GeneralSettingsView.xaml` + `.cs`):
   - UserControl with `FlowDirection="RightToLeft"` (Arabic)
   - DataGrid or ItemsControl with two columns: Setting Name (read-only label, Arabic), Value (TextBox)
   - Save button, Back button, Error/Status text blocks
   - Matches visual style of `SystemSettingsView.xaml`

3. **DI Registration** (`src/TopLab.Presentation/DependencyInjection.cs`):
   - `services.AddTransient<GeneralSettingsViewModel>();`

4. **Navigation** (`src/TopLab.Presentation/ViewModels/Settings/SettingsDashboardViewModel.cs`):
   - Add `GeneralSettingsCommand` that navigates to `GeneralSettingsViewModel`

5. **Settings Dashboard View** (`src/TopLab.Presentation/Views/Settings/SettingsDashboardView.xaml`):
   - Add "General Settings" button/card that triggers the navigation command

**Files to Create/Modify**:
- `src/TopLab.Presentation/ViewModels/Settings/GeneralSettingsViewModel.cs` (new)
- `src/TopLab.Presentation/Views/Settings/GeneralSettingsView.xaml` (new)
- `src/TopLab.Presentation/Views/Settings/GeneralSettingsView.xaml.cs` (new)
- `src/TopLab.Presentation/DependencyInjection.cs` (modify — add ViewModel registration)
- `src/TopLab.Presentation/ViewModels/Settings/SettingsDashboardViewModel.cs` (modify — add navigation command)
- `src/TopLab.Presentation/Views/Settings/SettingsDashboardView.xaml` (modify — add button)

**Risk Level**: Low
**Estimated Complexity**: Simple

---

### Slice 3: Integration Wiring + End-to-End Verification

**Description**: Verifies the full stack compiles, tests pass, migration applies cleanly, and the feature is accessible from the UI. Adds any final integration wiring.

**Dependencies**: Slices 1 and 2

**Technical Approach**:

1. **Build verification**: `dotnet build TopLab.sln -c Release` — 0 errors, 0 warnings

2. **Test verification**: `dotnet test TopLab.sln -c Release --no-build` — all existing + new tests pass

3. **Migration verification**: `dotnet ef database update` — migration applies cleanly, `GeneralSettings` table created with correct schema

4. **Seeding verification**: After migration, query `GeneralSettings` table — all enum keys seeded with default values

5. **Navigation verification**: Launch app → Settings Dashboard → click "General Settings" → view loads with seeded values

6. **Update verification**: Change a value → Save → re-query database → value persisted with updated `RowVersion`

7. **Concurrency verification**: (manual or automated) Two concurrent saves to same key → second returns Conflict error

8. **Authorization verification**: User without `EDIT_GENERAL_SETTINGS` permission → save returns Forbidden

**Files to Create/Modify**:
- None (verification only, except possible minor wiring fixes)

**Risk Level**: Low
**Estimated Complexity**: Simple

---

## Dependency Graph

```
Slice 1 (Domain + Application)
    ↓
Slice 2 (Presentation — ViewModel + View)
    ↓
Slice 3 (Integration Wiring + Verification)
```

Slice 1 must complete first (Domain entity, Application handlers, FakeApplicationDbContext update). Slice 2 depends on Slice 1 (sends MediatR queries/commands). Slice 3 depends on both (end-to-end verification).

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| EF migration conflicts with existing schema | Medium | Run `dotnet ef migrations list` first; use unique migration name `AddGeneralSettings` |
| Concurrency token (`RowVersion`) requires SQL Server | Low | TopLab already uses SQL Server (LocalDB); `IsRowVersion()` is supported |
| FakeApplicationDbContext incomplete for new entity | Low | Follow existing pattern exactly (add list, add Set/Add/Remove cases) |
| Arabic localization of setting names | Low | Use Arabic display names in ViewModel/View; enum values remain English |
| Too many settings in one screen | Low | Start with 5 allowlisted keys; UI is scrollable DataGrid |

## Validation Criteria

- [ ] `GeneralSetting` entity compiles with correct `Entity<GeneralSettingKey>` base
- [ ] `GeneralSettingKey` enum has exactly the 5 allowlisted values
- [ ] EF configuration sets `HasKey`, `HasMaxLength(500)`, `IsRowVersion()`
- [ ] Migration creates `GeneralSettings` table with correct columns
- [ ] `GetGeneralSettingsQuery` returns all seeded settings
- [ ] `UpdateGeneralSettingCommand` persists value and updates RowVersion
- [ ] `UpdateGeneralSettingCommand` returns Conflict on concurrency exception
- [ ] `UpdateGeneralSettingCommandValidator` rejects empty/oversized values
- [ ] `IAuthorizedRequest` with `EDIT_GENERAL_SETTINGS` is implemented
- [ ] `FakeApplicationDbContext` includes `GeneralSettings` list and routing
- [ ] All handler tests pass with FakeApplicationDbContext
- [ ] Authorization test verifies permission is required
- [ ] WPF ViewModel loads settings via MediatR and binds to View
- [ ] WPF View renders settings in Arabic DataGrid
- [ ] Navigation from Settings Dashboard works
- [ ] `dotnet build TopLab.sln -c Release` passes with 0 errors
- [ ] `dotnet test TopLab.sln -c Release` passes all tests
