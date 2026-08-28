# F5 — Data Model: Baseline Entity Schemas — Complete Implementation Plan

## 1. F5 Overview

### 1.1 Objective
Realize the complete relational data model defined in `Docs/Source/Top_Lab_Data_Model_Blueprint.md` as Domain entities + EF Core Fluent configurations + a single initial migration, making the schema capable of supporting every future module M01-M23 without adding a table that should have been in the baseline. F5 is the last Foundational item before Wave 1 (per `Top_Lab_Module_Dependency_Map.md:38-39`).

### 1.2 Expected Final Outcome
- `TopLab.Domain` contains every entity, strongly-typed ID, enum, value object, and join type for Groups A-I (§4-§12 of the Data Model Blueprint).
- `TopLab.Infrastructure/Persistence/Configurations/` contains one `IEntityTypeConfiguration<T>` per entity (Fluent API only, ADR-0011).
- `TopLab.Infrastructure/Persistence/ApplicationDbContext` discovers all configs via `ApplyConfigurationsFromAssembly` and exposes typed `DbSet<T>`s.
- `TopLab.Infrastructure/Persistence/Migrations/` contains a single `BaselineDataModel` migration that creates every table/index/FK/seed row and applies cleanly to an empty SQL Server (LocalDB).
- `AuditableEntitySaveChangesInterceptor` correctly handles all auditable entities regardless of `TId`.
- Build 0/0, all tests green, coverage floors met, dependency-rule and migration gates green.

### 1.3 Scope Boundaries
- **In:** Groups A (Patients §4), B (Test Catalog §5), C (Orders/Results §6), D (Billing §7), E (External Entities/Sent-Out §8), F (Users/Permissions/Attendance §9), G (Audit columns on hosts — no new table), H (Accounting §11), I (System Configuration §12), plus `IAuditableEntity` marker, enums, strong IDs, `PatientStatus` stub.
- **Out:** No Application Commands/Queries/Handlers/Validators, no Presentation work, no printing/barcode services, no business-rule calculator logic beyond stubs (full `PatientStatusCalculator` logic ships in M04 per `Master_Tracking_Sheet.md` Cross-Cutting Concern). No new top-level project.

---

## 2. Current Project State (Relevant to F5)

### 2.1 Solution & Build
- `TopLab.sln:1` — 4 prod (Domain net8.0, Application net8.0, Infrastructure net8.0, Presentation net8.0-windows) + 3 test (xUnit 2.9.3). `Directory.Build.props:4` Nullable=enable, LangVersion=latest, ManagePackageVersionsCentrally=true. `Directory.Packages.props:14` pins MediatR 12.5.0, FluentValidation 12.1.1, EF Core SqlServer/InMemory/Tools 8.0.30 — do NOT bump. `.editorconfig:1` 4-space, LF, Allman (when silent).
- Build state: F1 skeleton 🟩, F2 `Entity/AuditableEntity/ValueObject/StronglyTypedId/DomainException` 🟩 (`TopLab.Domain/Common/` 5 files), F3 Result+3 pipeline behaviors 🟩 (`TopLab.Application/Common/Results/` + `Behaviors/` + `Authorization/` + `DependencyInjection.cs:22` order Validation→Authorization→Logging), F4 persistence baseline 🟩 (`TopLab.Infrastructure/Persistence/ApplicationDbContext.cs:18` partial with `ApplyConfigurationsFromAssembly`, `Interceptors/AuditableEntitySaveChangesInterceptor.cs:24`, `Identity/CurrentUserService.cs:18` scoped, `Identity/SystemDateTimeProvider.cs:9`, `Infrastructure/DependencyInjection.cs:20` wiring `ISaveChangesInterceptor`, `ApplicationDbContext` UseSqlServer, `IApplicationDbContext`, `ICurrentUserService`, `IDateTimeProvider`).

### 2.2 What Is Missing (F5 Not Started)
- `src/TopLab.Domain` has only `Common/` — zero entity folders/files beyond common bases (verified `glob src/TopLab.Domain/**`).
- `src/TopLab.Infrastructure/Persistence/` has only `ApplicationDbContext.cs` + `Interceptors/` — no `Configurations/` folder, no `Migrations/` folder.
- `src/TopLab.Application/Features/` has only `SamplePipeline/` placeholder.
- `ApplicationDbContext.cs:15` comment explicitly says DbSets are added in F5; `OnModelCreating:30` already calls `ApplyConfigurationsFromAssembly` so per-entity registration is automatic.

### 2.3 Tracking & Gates
- `Docs/Top_Lab_Loop_State.md:31` → F3 🟩 (26 tests), F4 🟩 (37 tests), F5 ⬜ pending, resume pointer = F5. `Docs/Source/Top_Lab_Master_Tracking_Sheet.md:54` → F5 `Data model — baseline entity schemas | ⬜ Design`. Wave 0 🟨 In Progress.
- Quality gates per `Loop_State.md:20` / `Test_Strategy.md:6` — compile `-warnaserror` 0/0, `dotnet test` all pass, dependency-rule (Domain 0 external refs, Presentation→Application only, Application→no Infrastructure concrete), convention (`<Type>Tests`, feature/folder layout), coverage (Domain 90% / App 80% / Infra 70%), migration applies cleanly.

---

## 3. Prerequisites and Dependencies

### 3.1 Must Be Green Before Coding
- F2 common bases (`Entity<TId>:TopLab.Domain/Common/Entity.cs:8`, `AuditableEntity<TId>:AuditableEntity.cs:11`, `ValueObject:ValueObject.cs:7`, `StronglyTypedId<TValue>:StronglyTypedId.cs:12`, `DomainException:DomainException.cs:10`) — verified present.
- F4 `ApplicationDbContext` partial + `IApplicationDbContext:TopLab.Application/Common/Interfaces/IApplicationDbContext.cs:13` + interceptor + `ICurrentUserService:ICurrentUserService.cs:8` + `IDateTimeProvider:IDateTimeProvider.cs:8` + DI wiring — verified present.

### 3.2 Must Be Read Before Coding
- `Top_Lab_Data_Model_Blueprint.md` (§2-§14 — the only source of columns/types/FKs/indexes), `Top_Lab_Architecture_Blueprint.md` (§4.1 Domain grouping, §8 persistence conventions, §9 naming), `Top_Lab_ADR.md` (0003 SQL Server, 0004 EF Core, 0005 Clean Arch, 0011 one context/one config, 0012 strong IDs, 0013 auditable interceptor, 0014 T-surface on PatientTest, 0015 status not stored, 0016 no stored totals, 0017 void-and-reissue, 0018 soft-delete, 0020 singleton PK=1, 0021 connection local, 0022 single-branch), `Top_Lab_Coding_Standards.md` (§3.1 dependency rule, §4 naming, §5.1 Domain rules, §5.3 Infrastructure Fluent only, §6.6 UTC, §6.7 decimal(18,2)), `Top_Lab_Test_Strategy.md` (§3.1 Domain tests, §3.3 Infra tests, §4 coverage, §6 gates), `Top_Lab_Module_Dependency_Map.md:38` (F5 contents).

### 3.3 External Dependencies (No New Without ADR)
- EF Core 8.0.30 + SqlServer + Tools (may need `Microsoft.EntityFrameworkCore.Design` 8.0.30 for `dotnet ef` — currently absent; see Risk). No new package may introduce Internet dependency (ADR-0010).

---

## 4. Complete F5 Implementation Plan (Sequential)

### Task 1 — Branch & Baseline Verification
- **Where:** git root `C:\Users\LAP LINK\source\repos\Top-Lab`
- **What:** Create `feature/f5-data-model-baseline` from F4 green commit. Run `dotnet build TopLab.sln -c Debug -warnaserror` (expect 0/0) and `dotnet test TopLab.sln` (expect 37/37 per Loop_State) — snapshot before any edit.
- **Depends:** F4 green state.
- **Verify:** Commit hash recorded; gates green.

### Task 2 — Specify Cross-Cutting Enumerations & Strong IDs
- **Where:** `src/TopLab.Domain/Common/Enums/` + `src/TopLab.Domain/Common/Ids/`
- **What:** Create `IAuditableEntity.cs` marker (`int CreatedByUserId {get;set;} DateTime CreatedAtUtc {get;set;} int LastModifiedByUserId {get;set;} DateTime LastModifiedAtUtc {get;set;} int ModificationCount {get;set;}`) and make `AuditableEntity<TId>:Entity<TId>, IAuditableEntity` (`AuditableEntity.cs:11`). Create enums with explicit `byte`/`int` values: `Sex{Male,Female}`, `AgeUnit{Day,Month,Year}`, `AccountType{Individual,LabToLab,Contracts,VIP,Free}`, `ResultKind{Simple,SpecializedProfile,Culture}`, `ResultFlag{Normal,Low,High}`, `SensitivityCategory{HighlyFor,ModerateFor,LowFor,ResistantFor}`, `EntityType{TreatingDoctor,ReferralOrContract,PartnerLab}`, `OperationType{Payment,Correction,FullSettlement}`, `MovementType{Disbursement,Deposit}`, `PaperSize{A4,A5}`, `HeaderFooterMode{None,Words,Images}`, `TestDetailDisplayMode`, `HistorySortMode`, `ResultScreenAccountDisplayMode`, `MedicalConditionCategory{Medication,Condition}` — one file per enum, XML doc, `namespace TopLab.Domain.Common.Enums`.
- **Subtask 2a:** Create ~22 strongly-typed IDs `sealed class PatientId : StronglyTypedId<int> { private PatientId(int v):base(v){} public static PatientId Create(int v)=>new(v); }` for every surrogate PK table (Patient, PatientPhoneNumber, MedicalConditionType, TestGroup, Test, ReferenceRange, TestComment, CustomGroup, WorkGroupLog, PatientTitle, Antibiotic, PatientTest, ProfileResultItem, CultureAntibioticResult, PaymentOperation, PriceList, ExternalEntity, SentOutSample, SentOutSamplePayment, User, Permission, AttendanceRecord, CashMovement, etc.). Joins with composite PK (`PatientMedicalCondition`, `CustomGroupItem`, `WorkGroupLogItem`, `PriceListItem`, `UserPermissionGrant`, `CultureAntibioticAttachment`) keep no surrogate — no strong ID.
- **Why:** ADR-0012; enums are `tinyint` columns per Data Model. `IAuditableEntity` fixes interceptor bug where `AuditableEntity<int>` misses `AuditableEntity<PatientId>` (see `AuditableEntitySaveChangesInterceptor.cs:64`).
- **Depends:** Task 1.
- **Verify:** `dotnet build` 0/0; no Domain `PackageReference`.

### Task 3 — Domain Group A: Patients & Identification
- **Where:** `src/TopLab.Domain/Patients/`
- **What:**
  - `Patient.cs : AuditableEntity<PatientId>` — props per §4.2: `PatientId Id`, `string? LabId` (30, indexed not unique), `string? Title` (50), `string FullName` (200 required), `Sex Sex`, `int AgeValue`, `AgeUnit AgeUnit`, `string? NationalId` (30), `string? Address` (300), `ExternalEntityId? TreatingDoctorId`, `ExternalEntityId? ReferralEntityId`, `AccountType AccountType`, `bool IsVip`, `DateTime RegistrationDateUtc` (UTC), `DateTime? PickupDateUtc`, `bool IsFastingIndicated`, `int? FastingHours`, `bool RecentContrastImaging`, `string? Notes` (1000), navs `ICollection<PatientPhoneNumber>`, `ICollection<PatientMedicalCondition>`, `ICollection<PatientTest>`, `ICollection<PaymentOperation>`. Factory `static Result<Patient> Create(...)` validates FullName non-empty, AgeValue≥0, FastingHours only when IsFastingIndicated; private ctor + `Update(...)` for editable fields; no public setters.
  - `PatientPhoneNumber.cs : Entity<PatientPhoneNumberId>` — `PatientId PatientId`, `string PhoneNumber` (30 required, indexed), `byte SortOrder`, nav `Patient`.
  - `MedicalConditionType.cs : Entity<MedicalConditionTypeId>` — `string Name` (100), `MedicalConditionCategory Category`.
  - `PatientMedicalCondition.cs` — composite PK, no `Entity<>` base: `PatientId PatientId`, `MedicalConditionTypeId MedicalConditionTypeId`, navs.
  - `PatientTitle.cs : Entity<PatientTitleId>` — `string TitleText` (50), `bool IsDefault` (§5.7).
- **Why:** Data Model §4.1 note: `Patient` one row per visit, `PatientId` unique per visit, `LabId` shared grouping value — no master table.
- **Depends:** Task 2.
- **Verify:** `PatientTests` can construct via factory; `LabId` nullable.

### Task 4 — Group B: Test Catalog & Reference Data
- **Where:** `src/TopLab.Domain/Tests/`
- **What:** `TestGroup.cs : Entity<TestGroupId> { string Name 150 }`. `Test.cs : AuditableEntity<TestId>` per §5.2: `Name/ReportName/ReceiptName` 150, `TestGroupId?`, `Barcode?` 50, `int CompletionDurationMinutes>0`, `bool IsSentOut`, `decimal? SentOutCostPrice (18,2)`, `decimal PatientPrice (18,2)`, `decimal? LabToLabPrice (18,2)`, `ResultKind ResultKind`, `bool IsCultureType`, navs `ReferenceRanges/TestComments`. Invariant: IsSentOut→SentOutCostPrice required. `ReferenceRange.cs : Entity<ReferenceRangeId>` per §5.3: `TestId`, `Sex?`, `AgeUnit`, `int AgeMin/Max` (Min≤Max), `decimal MinValue/MaxValue (18,4)` (Min≤Max), `LowComment? HighComment?` 500 — no unit conversion. `TestComment.cs : Entity<TestCommentId>` per §5.4: `TestId`, `CommentText` 1000. `CustomGroup.cs : Entity<CustomGroupId>` + `CustomGroupItem.cs` (composite PK CustomGroupId+TestId, `decimal Price (18,2)`). `WorkGroupLog.cs` + `WorkGroupLogItem.cs` per §5.6. `Antibiotic.cs : Entity<AntibioticId>` per §5.8: `Name` 150, `bool IsPregnancyFlagged`, `bool IsChildrenFlagged` (<12y). `CultureAntibioticAttachment.cs` composite per §5.9.
- **Depends:** Task 3.
- **Verify:** Build 0/0.

### Task 5 — Group C: Orders/Results & Clinical Data
- **Where:** `src/TopLab.Domain/Results/` (matches `Architecture Blueprint §4.1 Results/`)
- **What:** `PatientTest.cs : AuditableEntity<PatientTestId>` per §6.1 — central row: `PatientId`, `TestId`, `decimal PriceAtOrderTime (18,2)` snapshot, sample flags `IsUrine/IsStool/IsBlood/IsSemen/IsCSF`, `bool IsTakenOutsideLab`, `bool IsSampleDrawn`, `DateTime? SampleDrawnAtUtc`, `string? ResultValue` 200 (simple only), `ResultFlag?`, `string? Notes` 500, T-audit `int? EnteredByUserId / DateTime? EnteredAtUtc`, `bool IsReviewed`, `int? ReviewedByUserId/ReviewedAtUtc`, `bool IsPrinted`, `int PrintCount`, `int? LastPrintedByUserId/LastPrintedAtUtc`, `bool IsDelivered`, `int? DeliveredByUserId/DeliveredAtUtc`, `bool IsExported`, `DateTime? ExportedAtUtc`. Method `EnterResult(string value, Sex sex, AgeUnit age, IReadOnlyList<ReferenceRange> ranges)` evaluates `ResultFlag` at entry and persists — not recomputed after range change (BR-05). `ProfileResultItem.cs : Entity<ProfileResultItemId>` per §6.2: `PatientTestId`, `AnalyteName` 150, `ResultValue` 100, `Unit?` 30, `Flag?`, `bool IsVerified/IsPrinted`. `CultureResult.cs` PK=`PatientTestId` one-to-one per §6.3: `Sample?` 100, `OrganismA/B/C?` 150, `CultureCondition?` 200, `ColonyCount?` 50. `CultureAntibioticResult.cs : Entity<CultureAntibioticResultId>` per §6.4: `PatientTestId`, `AntibioticId`, `SensitivityCategory`.
- **Why:** T-audit stored on PatientTest per ADR-0014 (§10 Group G); one-to-one CultureResult avoids duplicate table.
- **Depends:** Task 4.
- **Verify:** Invariant that `ResultFlag` stored at entry time.

### Task 6 — Group D: Billing & Payments
- **Where:** `src/TopLab.Domain/Billing/`
- **What:** `PaymentOperation.cs : AuditableEntity<PaymentOperationId>` per §7.1: `PatientId`, `decimal Amount (18,2)`, `decimal? DiscountAmount (18,2)`, `bool IsExtraCharge` (+ sign), `OperationType`, `int ReceivedByUserId` (feeds P-button), `DateTime OperationAtUtc`, `bool IsVoided`. Method `Void()` sets IsVoided=true (ADR-0017 void-and-reissue; never physical delete). Aggregates computed at query time (ADR-0016). `PriceList.cs : Entity<PriceListId>` + `PriceListItem.cs` composite per §7.2: `Price (18,2)`.
- **Depends:** Task 5.
- **Verify:** Correction path never deletes.

### Task 7 — Group E: External Entities & Sent-Out
- **Where:** `src/TopLab.Domain/ExternalEntities/` + `src/TopLab.Domain/SentOutSamples/`
- **What:** `ExternalEntity.cs : AuditableEntity<ExternalEntityId>` per §8.1 type discrimination: `EntityType`, `string Name` 200, `City?` 100, `Address?` 300, `Phone?` 30, `Fax?` 30, `ResponsiblePersonName?` 150, `ResponsiblePersonPhone?` 30, `PriceListId?` (null for TreatingDoctor), `decimal? DiscountOrCommissionPercent (5,2)`, `string? GeneratedIdCode` 50. Factory validates TreatingDoctor must NOT have PriceListId. `SentOutSample.cs : AuditableEntity<SentOutSampleId>` per §8.2: `PatientTestId`, `ExternalEntityId ExternalLabEntityId` (must be PartnerLab — validated in handler), `decimal CostPrice (18,2)`, `decimal PatientPrice (18,2)`, `DateTime SentAtUtc`. `SentOutSamplePayment.cs : Entity<SentOutSamplePaymentId>` per §8.3: `SentOutSampleId`, `decimal AmountPaid (18,2)`, `DateTime PaidAtUtc`, `int PerformedByUserId` (multiple rows per sample, full settlement when sum==CostPrice).
- **Depends:** Task 6 (PriceList needed).
- **Verify:** Single-table inheritance via `EntityType` discriminator.

### Task 8 — Group F: Users/Permissions/Attendance
- **Where:** `src/TopLab.Domain/Users/` + `src/TopLab.Domain/Attendance/`
- **What:** `User.cs : AuditableEntity<UserId>` per §9.1: `string UserName` 100 unique, `PasswordHash` 300, `InternalWindowsPasswordHash` 300, `bool IsAbsolutePermission`, `decimal DiscountLimitPercent (5,2)`, `bool BlockPrintOnRemainingBalance`, `TimeOnly? WorkStartTime/WorkEndTime` (map to `time`), `bool HasBreakPeriod`, `int? BreakDurationMinutes`, `DateTime? LastLoginAtUtc`, `bool IsActive`, `ICollection<UserPermissionGrant> PermissionGrants`; `Deactivate()` sets IsActive=false. `Permission.cs : Entity<PermissionId>` per §9.2: `string Code` 50 UPPER_SNAKE (stable, never renamed), `Description` 300 — 13 fixed rows. `UserPermissionGrant.cs` composite per §9.3: `UserId+PermissionId`. `AttendanceRecord.cs : Entity<AttendanceRecordId>` per §9.4: `UserId`, `DateTime CheckInAtUtc`, `DateTime? BreakStart/End/CheckOutAtUtc`, `int? OvertimeMinutes/LatenessMinutes` computed at events.
- **Depends:** Task 7.
- **Verify:** User.IsActive preferred over delete.

### Task 9 — Group H: Accounting
- **Where:** `src/TopLab.Domain/Accounting/`
- **What:** `CashMovement.cs : AuditableEntity<CashMovementId>` per §11.1: `MovementType`, `decimal Amount (18,2)`, `ExternalEntityId? RelatedExternalEntityId`, `int PerformedByUserId`, `DateTime OccurredAtUtc`, `string? Notes` 500. Note §11.2-11.3 inventory/company balances are computed aggregates — no stored inventory table.
- **Depends:** Task 8.
- **Verify:** No running-total column.

### Task 10 — Group I: System Configuration (Singletons)
- **Where:** `src/TopLab.Domain/Settings/`
- **What:** `SystemSettings.cs : Entity<int>` PK=1 per §12.1: `AccountType DefaultAccountType`, `bool PrintLabIdInsteadOfPatientId/AutoReviewAndComplete/SaveTreatingDoctorOnlyFromEntityWindow/EnablePatientNameSearchAssist/DisableAutoTitleInsertion/PrintFileExternalBarcode/PrintDateTimeOnTubeBarcode/PrintAccountInsteadOfDateOnReport/DailyBackupEnabled`, `int ResultScreenAccountDisplayMode` (tinyint), `string? DailyBackupPath` 300. `ReportSettings.cs : Entity<int>` PK=1 §12.2: `decimal(5,2) PageMarginLeft/Bottom/ReportTopSpaceCm` (max 8), `PaperSize`, `HeaderFooterMode`, `bool DoctorSignatureEnabled`, `string? HeaderColor/FooterColor` 9, `HistorySortMode`, `bool HistoryAutoDisplayEnabled`. `ReceiptSettings.cs : Entity<int>` PK=1 §12.3 + `EnvelopeSettings.cs : Entity<int>` PK=1 §12.4 + `EnvelopePrintItemPosition.cs` PK `string ItemName` (Name/Code/ReferralEntity/Date) per §12.5: `bool IsEnabled`, `decimal(5,2) Left/TopOffsetCm`. `PrinterAssignment.cs` PK `byte OutputType` per §12.6: `string PrinterName` 200. Private ctor `():this(1){}`; factory `CreateDefault()` seeds defaults; validation `ReportTopSpaceCm <=8` returns `Result.Failure(Validation)`.
- **Why:** ADR-0020 single row PK=1, seeded.
- **Depends:** Task 9.
- **Verify:** Exactly one row per config after migration.

### Task 11 — Domain Services Stub
- **Where:** `src/TopLab.Domain/PatientStatus/`
- **What:** `PatientAggregateStatus.cs` enum S1-S7 + `PatientStatusCalculator.cs` stateless service stub `Calculate(Patient, IReadOnlyList<PatientTest>, decimal balance)` → `throw new NotImplementedException("Full precedence delivered in M04 per Master_Tracking_Sheet Cross-Cutting Concern")`. Keep type so future M04 has target; add TODO referencing BR-01/ADR-0015.
- **Why:** Architecture §7, Data Model BR-01.
- **Depends:** Task 10.
- **Verify:** Type exists but not called by F5 tests beyond construction.

### Task 12 — Infrastructure: Fluent Configurations (One Per Entity)
- **Where:** `src/TopLab.Infrastructure/Persistence/Configurations/` (create via `Test-Path -LiteralPath` check before `New-Item -ItemType Directory`)
- **What:** For every entity above, create `sealed <Entity>Configuration : IEntityTypeConfiguration<T>` with:
  - `HasKey` (surrogate or composite; `CultureResult` HasKey=PatientTestId).
  - `Property(...).IsRequired()/IsRequired(false)` + `HasMaxLength(n)` + `HasColumnType("decimal(18,2)"|"decimal(5,2)"|"decimal(18,4)"|"datetime2"|"tinyint"|"time"|"nvarchar")` + `HasPrecision(18,2)` where needed.
  - `HasConversion(v=>v.Value, v=>PatientId.Create(v))` for every strong ID, `HasConversion<int>()` or `HasColumnType("tinyint")` for enums.
  - FKs `HasOne.WithMany.HasForeignKey.OnDelete(Restrict)` (avoid cascade deleting patient history).
  - Indexes: `HasIndex(e=>e.LabId)` Patient, `HasIndex(e=>e.FullName)`, `NationalId`, `RegistrationDateUtc`, `PatientPhoneNumber.PhoneNumber`, composite `HasIndex(e=>new{e.IsReviewed,e.IsPrinted,e.IsDelivered})` on PatientTest, `UserName.IsUnique`, `PaymentOperation.PatientId`, `PatientTest.PatientId`.
  - `HasQueryFilter(e=>!e.IsDeleted)` only where `IsDeleted` exists (Data Model convention §2 — assume entities with audit relevance that are logically deletable: use filter only if column present; otherwise omit — flag UNKNOWN where not explicit).
  - Singletons: `HasKey(e=>e.Id)` + `HasData(new SystemSettings{Id=1,...})`, similarly Report/Receipt/Envelope (one row), `EnvelopePrintItemPosition` 4 rows (Name/Code/ReferralEntity/Date), `PrinterAssignment` 4 rows, `Permission` 13 rows — infer codes from PRD FR-M17-004 (see Risk R2) — mark as inferred.
  - `HasCheckConstraint("CK_ReportSettings_TopSpace"," [ReportTopSpaceCm] <= 8")`.
  - Namespace `TopLab.Infrastructure.Persistence.Configurations`, `sealed`, one type per file matching `Coding Standards §4`.
- **Depends:** Tasks 3-11.
- **Verify:** Count configs = entity count; `grep -c IEntityTypeConfiguration` matches.

### Task 13 — DbSets Partial
- **Where:** `src/TopLab.Infrastructure/Persistence/ApplicationDbContext.DbSets.cs`
- **What:** `partial class ApplicationDbContext { public DbSet<Patient> Patients => Set<Patient>(); public DbSet<PatientPhoneNumber> PatientPhoneNumbers => Set<PatientPhoneNumber>(); ... }` one per table entity (including joins where DbSet useful). Keep `OnModelCreating:30` unchanged — discovery handles configs.
- **Depends:** Task 12.
- **Verify:** `IApplicationDbContext.Set<Patient>()` resolvable.

### Task 14 — Interceptor Fix (Blocking)
- **Where:** `src/TopLab.Infrastructure/Persistence/Interceptors/AuditableEntitySaveChangesInterceptor.cs:64`
- **What:** Change `if (entry.Entity is not AuditableEntity<int> auditable) continue;` to `if (entry.Entity is not IAuditableEntity auditable) continue;`. Keep `IsModified=false` revert on `CreatedByUserId/CreatedAtUtc` for Modified, increment `ModificationCount`. Inject `ICurrentUserService` + `IDateTimeProvider` already scoped.
- **Why:** Without this, every strong-ID auditable entity silently skips audit.
- **Depends:** Task 2 (IAuditableEntity).
- **Verify:** Interceptor test with `Patient (PatientId)` proves audit populated.

### Task 15 — Migration
- **Where:** `src/TopLab.Infrastructure/Persistence/Migrations/`
- **What:** Run `dotnet ef migrations add BaselineDataModel --project src/TopLab.Infrastructure --startup-project src/TopLab.Presentation --context ApplicationDbContext` (Presentation is `net8.0-windows` executable with `appsettings.json` dummy `ConnectionStrings:TopLab` → `Server=(localdb)\\mssqllocaldb;Database=TopLab;Trusted_Connection=True;MultipleActiveResultSets=true`). Verify generated `Up()` contains every `CreateTable`, `CreateIndex`, `AddForeignKey`, `InsertData` for seeds. If `dotnet ef` fails with "Design package not referenced", add `Microsoft.EntityFrameworkCore.Design` 8.0.30 to `Directory.Packages.props` + `Infrastructure.csproj` (PrivateAssets=all) and retry.
- **Depends:** Tasks 12-14, `dotnet build` 0/0.
- **Verify:** `dotnet ef migrations list` shows `BaselineDataModel`; `dotnet ef migrations script` contains all tables.

### Task 16 — Apply & Rollback
- **Where:** LocalDB ephemeral DB (never shared/prod)
- **What:** `dotnet ef database update --project src/TopLab.Infrastructure --startup-project src/TopLab.Presentation` → verify `SELECT name FROM sys.tables` contains all, `SELECT * FROM SystemSettings WHERE Id=1` =1 row, `SELECT COUNT(*) FROM Permission` =13. Then `dotnet ef database update 0` rollback → tables dropped, re-apply succeeds.
- **Depends:** Task 15.
- **Verify:** Seed rows idempotent.

### Task 17 — Domain Tests
- **Where:** `tests/TopLab.Domain.Tests/` mirroring Domain folders (`Patients/Tests/Results/Billing/ExternalEntities/Users/Attendance/Accounting/Settings/Common/`)
- **What:** Per `Test_Strategy §3.1` — each entity at least `Create_Valid_Succeeds`, `Create_MissingRequired_Fails`, equality by Id, strong-ID non-interchangeability (`PatientId != TestId` compile error path), reference-range `AgeUnitSensitive` test (1 day-60 days matches 15d/35d not 1mo), `AuditableEntity` equality, `PaymentOperation.Void()`, `Test` invariants (CompletionDuration>0, IsSentOut→CostPrice). Builders `PatientBuilder.NewMale(age:30)` etc., no real PII, `IDateTimeProvider` fake never `DateTime.UtcNow` in assertions.
- **Naming:** `<TypeUnderTest>Tests`, methods `Method_Condition_Expected` or `Given_When_Then` consistently per class (`Coding Standards §8`).
- **Depends:** Tasks 3-11.
- **Verify:** `dotnet test tests/TopLab.Domain.Tests` all pass; coverage ≥90%.

### Task 18 — Infrastructure Config & Migration Tests
- **Where:** `tests/TopLab.Infrastructure.Tests/Persistence/`
- **What:** Per `Test_Strategy §3.3` — one `*ConfigurationTests` per entity asserting `FindPrimaryKey()`, `IsNullable`, `GetMaxLength()`, `GetPrecision()/GetColumnType()`, FK existence, index existence, singleton reject second insert (`DbUpdateException`), soft-delete filter (`query.Count()` vs `IgnoreQueryFilters()`), plus `AuditableEntitySaveChangesInterceptorTests` (Added→Created* and ModificationCount=0, Modified→LastModified* incremented, Created* untouched via `IsModified=false`), plus `MigrationTests.Migrate_And_Rollback_Cleanly` using LocalDB/Testcontainers (not InMemory).
- **Depends:** Tasks 12-16.
- **Verify:** `dotnet test tests/TopLab.Infrastructure.Tests` all pass; Infra ≥70%.

### Task 19 — Gate Verification
- **Where:** Repo root
- **What:** `dotnet build TopLab.sln -c Debug -warnaserror` 0/0. `dotnet test TopLab.sln` all pass (expect ~60+ new tests, total ~97). `grep -r "using TopLab.Infrastructure" src/TopLab.Application` =0. `grep -r "Microsoft.EntityFrameworkCore" src/TopLab.Domain` =0. `TopLab.Domain.csproj` no `PackageReference`. Naming/folder conventions per `Coding Standards §4`/`Architecture §9`.
- **Depends:** Tasks 17-18.
- **Verify:** Paste build/test/coverage logs into PR.

### Task 20 — Commit & Handoff
- **Where:** git
- **What:** Commit locally only (per `Loop_State.md:15` No push until approval) with message `feat(data-model): baseline entity schemas across all entity groups (F5)\n\nData Model §4-§12, Architecture §4.1/§8, ADR-0011/0012/0013/0014/0020/0022. Migration: BaselineDataModel.` Update change-log in handoff doc per `Handoff_Template.md`. Do NOT touch F6 files.
- **Depends:** Task 19.
- **Verify:** `git status` clean except new F5 files; `git log --oneline -3` shows F5 commit.

---

## 5. Files / Components Impact

### 5.1 New Files to Create (F5)
- Domain: `Common/IAuditableEntity.cs`, `Common/Enums/*.cs` (~14), `Common/Ids/*.cs` (~22), `Patients/{Patient,PatientPhoneNumber,MedicalConditionType,PatientMedicalCondition,PatientTitle}.cs`, `Tests/{TestGroup,Test,ReferenceRange,TestComment,CustomGroup,CustomGroupItem,WorkGroupLog,WorkGroupLogItem,Antibiotic,CultureAntibioticAttachment}.cs`, `Results/{PatientTest,ProfileResultItem,CultureResult,CultureAntibioticResult}.cs`, `Billing/{PaymentOperation,PriceList,PriceListItem}.cs`, `ExternalEntities/ExternalEntity.cs`, `SentOutSamples/{SentOutSample,SentOutSamplePayment}.cs`, `Users/{User,Permission,UserPermissionGrant}.cs`, `Attendance/AttendanceRecord.cs`, `Accounting/CashMovement.cs`, `Settings/{SystemSettings,ReportSettings,ReceiptSettings,EnvelopeSettings,EnvelopePrintItemPosition,PrinterAssignment}.cs`, `PatientStatus/{PatientAggregateStatus,PatientStatusCalculator}.cs`
- Infrastructure: `Persistence/Configurations/*Configuration.cs` (28+), `Persistence/ApplicationDbContext.DbSets.cs`, `Persistence/Migrations/*BaselineDataModel*.cs` + `Designer.cs` + `ApplicationDbContextModelSnapshot.cs` update
- Tests: `tests/TopLab.Domain.Tests/{Patients,Tests,Results,Billing,ExternalEntities,Users,Attendance,Accounting,Settings}/...Tests.cs`, `tests/TopLab.Infrastructure.Tests/Persistence/Configurations/*ConfigurationTests.cs`, `Persistence/Interceptors/...Tests.cs`, `Persistence/MigrationTests.cs`

### 5.2 Files to Edit (F5)
- `src/TopLab.Domain/Common/AuditableEntity.cs:11` → implement `IAuditableEntity`
- `src/TopLab.Infrastructure/Persistence/Interceptors/AuditableEntitySaveChangesInterceptor.cs:64` → `IAuditableEntity` check
- `src/TopLab.Infrastructure/TopLab.Infrastructure.csproj:9` → add `Microsoft.EntityFrameworkCore.Design` if needed (CPM `Directory.Packages.props:8`)

### 5.3 Files to Delete
- **None.**

### 5.4 Must Remain Untouched
- `TopLab.Application/Common/Results/{Result,Error,ErrorType}.cs`, `Behaviors/{Validation,Authorization,Logging}Behavior.cs`, `Interfaces/{IApplicationDbContext,ICurrentUserService,IDateTimeProvider}.cs`, `DependencyInjection.cs:22` — stable F3/F4. `TopLab.Domain/Common/{Entity,ValueObject,StronglyTypedId,DomainException}.cs` beyond additive `IAuditableEntity`. `Directory.Build.props`, `.editorconfig`, `Docs/Source/*.md` (Final). `TopLab.Presentation/*` (F6 scope). Existing 37 tests must stay green.

---

## 6. Integration and Compatibility Requirements

- **Discovery:** `ApplicationDbContext.cs:30` `ApplyConfigurationsFromAssembly` must discover all new configs — no manual `modelBuilder.Entity<T>()` per entity.
- **Seeding:** Singletons `HasData(Id=1,...)` + `Permission` 13 rows + `EnvelopePrintItemPosition` 4 rows + `PrinterAssignment` 4 rows — must not collide with later M17/M22 seeds; PK=1 enforcement via `HasKey` + `HasData` guarantees single row.
- **Port stability:** `IApplicationDbContext.Set<T>/Add/Update/Remove/SaveChangesAsync/Database` unchanged; handlers in later waves consume via this port — F5 must not change signature.
- **Audit:** Interceptor is single writer of `CreatedByUserId/AtUtc/LastModifiedByUserId/AtUtc/ModificationCount`; handlers never set them (`Coding Standards §6.4`); `ICurrentUserService` + `IDateTimeProvider` scoped so tests inject deterministic fakes.
- **Connection:** `Infrastructure/DependencyInjection.cs:26` reads `ConnectionStrings:TopLab` — workstation-local per ADR-0021, never stored in DB; migrant design-time connection uses LocalDB dummy in `appsettings.json` (ignored for prod).
- **Compatibility:** No breaking rename of F2/F3/F4 public types; add-only changes so past green tests still pass.

---

## 7. Testing and Validation Plan

### 7.1 Unit — Domain (`TopLab.Domain.Tests`)
- **Scope:** Every new entity factory/invariant, `ValueObject` equality, `StronglyTypedId` non-interchangeability, `ReferenceRange` age-unit no-conversion, `Patient` required fields/VIP/age, `Test` completion/sentOut, `PaymentOperation` void-and-reissue, `User` IsActive, `SystemSettings` TopSpace ≤8. Builders `PatientBuilder`, `TestBuilder` with no real PII (`Test_Strategy §5`).
- **Run:** `dotnet test tests/TopLab.Domain.Tests --collect:"XPlat Code Coverage"`.
- **Pass:** All green, Domain ≥90% line, mandatory scenarios `Test_Strategy §3.1` present.

### 7.2 Integration — Infrastructure (`TopLab.Infrastructure.Tests`)
- **Scope:** Per `Test_Strategy §3.3` — each config asserts PK, required/nullable, `nvarchar` length, `decimal(18,2)/(5,2)/(18,4)`, `datetime2` UTC, FK, indexes (§14), `HasQueryFilter` for `IsDeleted`, singleton second insert → `DbUpdateException`, `HasCheckConstraint` for TopSpace. Interceptor Added/Modified with strong-ID entity + `FakeCurrentUserService`/`FakeDateTimeProvider`. Migration apply/rollback cleanly against LocalDB (not InMemory).
- **Run:** `dotnet test tests/TopLab.Infrastructure.Tests` (LocalDB or Testcontainers SqlServer per §3.3 — never shared/prod DB).
- **Pass:** All green, Infra ≥70%.

### 7.3 Error-Path Testing
- Duplicate singleton PK → `DbUpdateException`; missing required (FullName null) → `Result.Failure(Validation)` or `ArgumentNullException` per guard; `AgeMin>AgeMax` → validation failure; soft-delete filtered vs `IgnoreQueryFilters()`; `IsSentOut` without CostPrice → validation; `ReportTopSpaceCm=9` → check-constraint or factory validation.

### 7.4 Edge-Case Validation
- Multi-phone 1/2/3 numbers persisted + indexed search; 0 phones allowed at DB but handler requires at least one when exercising phone-driven behavior (cover both). `PriceAtOrderTime` snapshot immune to later `Test.PatientPrice` change. `LabId` grouping: multiple Patient rows share same string index, query by LabId returns many visits. `PaymentOperation.IsVoided` excluded from `SUM(Amount)` balance. Currency `decimal(18,2)` bankers rounding.

### 7.5 Regression
- Existing 37 tests (12 Domain +14 App +11 Infra) still green on tip. Dependency-rule: Domain 0 external refs, Application 0 `using TopLab.Infrastructure`. Convention: `<Type>Tests`, feature/folder layout.

### 7.6 Validation Checkpoints (Gates — `Test_Strategy §6` / `Loop_State.md:20`)
1. Compile `dotnet build -warnaserror` 0/0. 2. Test `dotnet test` all pass. 3. Dependency-rule check. 4. Convention check. 5. Coverage per-project floors (report honestly, attach `coverage.cobertura.xml`). 6. Migration gate — `dotnet ef migrations list` + `database update` clean.

---

## 8. Risks, Assumptions, and Unknowns

- **VERIFIED FACT:** Interceptor currently `is not AuditableEntity<int>` → misses strong-ID entities. **Mitigation:** `IAuditableEntity` refactor in Task 2/14 before migration.
- **UNKNOWN / REQUIRES VERIFICATION — Enum encodings:** Data Model uses `tinyint` but never lists numeric values for Sex/AccountType/ResultKind/etc. **Must verify** exact 0/1/2 ordering with PO before coding; until verified use explicit values `Male=0,Female=1...` and flag `UNKNOWN` in code.
- **UNKNOWN / REQUIRES VERIFICATION — Permission codes:** PRD FR-M17-004 lists 13 Arabic items; Data Model §9.2 says stable `UPPER_SNAKE` Code but exact strings not enumerated in one place. Inferred codes (e.g., `ADD_EDIT_PATIENT`, `PT_AUDIT_ACCESS`) are used for seeding — **must confirm** against MasrLab/PO before final seed.
- **UNKNOWN / REQUIRES VERIFICATION — `IsDeleted` scope:** Data Model §2 says [Soft-delete] tables carry `IsDeleted bit` but per-table column lists omit it for many entities. **Must verify** which baseline tables are soft-delete vs hard-delete; until verified implement `IsDeleted` only where explicitly required and document assumption.
- **UNKNOWN / REQUIRES VERIFICATION — `Microsoft.EntityFrameworkCore.Design`:** Not currently in `TopLab.Infrastructure.csproj:9`; `dotnet ef` may fail. **Must verify** by attempting migration; if needed add 8.0.30 Design (CPM).
- **INFERENCE:** `LabId` is `nvarchar(30)` grouping value on `Patient` per §4.1-4.2, not a surrogate PK. Do NOT create `LabId : StronglyTypedId<int>` PK; if a query-param strong type is needed use `LabIdValue : StronglyTypedId<string>` as value object, document distinction.
- **ASSUMPTION:** `ReportTopSpaceCm <=8` enforced both at factory (Result.Failure) and at DB `HasCheckConstraint` for defense-in-depth.
- **ASSUMPTION:** Concurrency/consistency non-functional checks (`Test_Strategy §9`) — two contexts on different patients must not interfere — require LocalDB relational tests, not InMemory.

---

## 9. Final Sequential Execution Checklist

1. Branch `feature/f5-data-model-baseline` from F4 green commit; verify build/test green.
2. Read Data Model, Architecture §4.1/§8/§9, ADR 0003-0023, Coding Standards, Test Strategy, Module Dependency Map F5.
3. Create `IAuditableEntity.cs`; edit `AuditableEntity.cs` to implement it; create `Common/Enums/` (14 enums) with explicit values.
4. Create `Common/Ids/` (~22 `StronglyTypedId<int>` subclasses).
5. Create `Domain/Patients/` — `Patient`, `PatientPhoneNumber`, `MedicalConditionType`, `PatientMedicalCondition` (composite), `PatientTitle`.
6. Create `Domain/Tests/` — `TestGroup`, `Test`, `ReferenceRange`, `TestComment`, `CustomGroup+Item`, `WorkGroupLog+Item`, `Antibiotic`, `CultureAntibioticAttachment`.
7. Create `Domain/Results/` — `PatientTest` (T-audit columns), `ProfileResultItem`, `CultureResult` (PK=PatientTestId), `CultureAntibioticResult`.
8. Create `Domain/Billing/` — `PaymentOperation`, `PriceList+Item`.
9. Create `Domain/ExternalEntities/ExternalEntity` + `Domain/SentOutSamples/SentOutSample+SentOutSamplePayment`.
10. Create `Domain/Users/{User,Permission,UserPermissionGrant}` + `Domain/Attendance/AttendanceRecord`.
11. Create `Domain/Accounting/CashMovement`.
12. Create `Domain/Settings/{SystemSettings,ReportSettings,ReceiptSettings,EnvelopeSettings,EnvelopePrintItemPosition,PrinterAssignment}` (PK=1 / string / byte).
13. Create `Domain/PatientStatus/{PatientAggregateStatus,PatientStatusCalculator}` stub (TODO M04).
14. Create `Infrastructure/Persistence/Configurations/` and implement 28+ `*Configuration.cs` (Fluent only: keys, required/length/precision/time/enum conversion, FKs Restrict, indexes §14, query filters, HasData seeds, check constraint).
15. Create `Infrastructure/Persistence/ApplicationDbContext.DbSets.cs` partial with typed `DbSet<T>`s.
16. Fix `AuditableEntitySaveChangesInterceptor.cs:64` to check `IAuditableEntity`.
17. Add `Microsoft.EntityFrameworkCore.Design` 8.0.30 via `Directory.Packages.props` if `dotnet ef` requires it; `dotnet build -warnaserror` 0/0.
18. `dotnet ef migrations add BaselineDataModel --project src/TopLab.Infrastructure --startup-project src/TopLab.Presentation` — inspect `Up()` covers all tables/indexes/seeds.
19. `dotnet ef database update` (LocalDB) → verify singleton `Id=1` row, 13 permissions, 4 envelope positions, 4 printer assignments; `dotnet ef database update 0` rollback + re-apply.
20. Write `TopLab.Domain.Tests` mirroring folders — invariants, reference-range age-unit, strong-ID non-interchangeability, factories.
21. Write `TopLab.Infrastructure.Tests` — per-config PK/required/precision/FK/index/singleton-reject/soft-delete filter + interceptor Added/Modified + migration apply/rollback.
22. `dotnet test TopLab.sln` all green (37 existing + ~60 new); `dotnet test --collect:"XPlat Code Coverage"` → Domain ≥90%, App ≥80%, Infra ≥70%, attach reports.
23. Verify dependency-rule (`grep` Domain/Infrastructure), convention (file/type/namespace, Async/CancellationToken), no workstation/branch scoping.
24. Commit locally `feat(data-model): baseline entity schemas across all entity groups (F5)` (no push); produce handoff per `Handoff_Template.md`.

---

## 10. F5 Definition of Done

F5 is DONE iff **all** are objectively true (one failure = not Done):

1. Every table/column/FK/index/seed in `Top_Lab_Data_Model_Blueprint.md` §4-§12 (Groups A-I, ~28 entities + 6 singletons + 4 composites + envelope/printer assignments) exists as a Domain entity (or join) with correct types, lengths (`nvarchar` per spec), precisions (`decimal(18,2)/(5,2)/(18,4)`), required/nullable, navs, and enums `tinyint`.
2. Every surrogate PK is `StronglyTypedId<int>` with EF value converter; no raw `int` PK on auditable entities; composite PKs exactly as Data Model.
3. Every `[Auditable]` entity derives `AuditableEntity<TId> : IAuditableEntity`; interceptor handles `IAuditableEntity` and correctly populates `Created*` on Added (Count=0) and updates `LastModified*` + increments `ModificationCount` on Modified leaving `Created*` untouched (`IsModified=false`).
4. One Fluent `IEntityTypeConfiguration<T>` per entity in `Persistence/Configurations/`, discovered by `ApplyConfigurationsFromAssembly`, covering keys, required/length/precision/UTC/enums/FKs Restrict/indexes §14/query filters/`HasData` singletons/`HasCheckConstraint` TopSpace≤8/converters.
5. `ApplicationDbContext.DbSets.cs` partial exposes typed `DbSet<T>`s; `OnModelCreating:30` unchanged beyond discovery.
6. Single migration `BaselineDataModel` under `Persistence/Migrations/` with complete `Up()/Down()/ModelSnapshot`; `dotnet ef database update` on empty LocalDB succeeds, singleton `Id=1` =1 row, permission =13 rows, envelope positions =4, printer =4; rollback to 0 + re-apply succeeds.
7. `dotnet build TopLab.sln -c Debug -warnaserror` → 0 errors 0 warnings.
8. `dotnet test TopLab.sln` → all pass (existing 37 + new Domain/Infra tests).
9. Coverlet per-project → Domain ≥90%, Application ≥80%, Infrastructure ≥70% (reported honestly; waivers noted per `Test_Strategy §4`).
10. Dependency-rule green: Domain 0 `PackageReference` beyond BCL; Application 0 `using TopLab.Infrastructure`; Presentation 0 Infrastructure concrete in ViewModels. Convention green: `<Type>Tests`, feature/folder, one type per file, `Async`/`CancellationToken`.
11. No table that should have been baseline is deferred to a later module — future M01-M23 can `Set<Patient>` etc. without adding a missing baseline table.
