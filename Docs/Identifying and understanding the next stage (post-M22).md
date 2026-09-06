# Identifying and understanding the next stage

| Field | Value |
|---|---|
| Repository | https://github.com/El-ogra/Top-Lab.git |
| Target commit (authoritative snapshot) | `a8c1a2b61dea98405d40cc47571cc9988bd5d9c1` |
| Target commit subject | `docs(m22): finalize module 22 documentation, ADR-0027, tracking sheet, and handoff` |
| Target commit date | 2026-09-02 01:27:20 +0300 |
| Investigation date | 2026-09-02 |
| Report type | Development-state investigation and next-wave decision report |
| Report language | English (technical identifiers kept verbatim) |

---

## 1. Executive conclusion

- **Actual current stage:** The project has genuinely completed **Wave 0 — Foundations (F1–F6)** and **Wave 1 — Configuration Backbone (M17 — User & Permission Management, and M22 — System & Print Settings)**. At the target commit the codebase contains exactly two end-to-end functional modules (M17, M22) on top of a complete four-layer foundation. Nothing from Wave 2 onward is implemented beyond inert baseline data-model artifacts (schema-first entities created by F5).
- **Is the project ready to move forward?** **Yes.** Both Wave 1 modules are implemented across Domain, Application, Infrastructure, and Presentation, with meaningful unit-test evidence. The declared Wave 2 dependencies (M17, M22) are satisfied by real code, not merely by documentation. No blocker, broken exit gate, or architectural gap was found that must be fixed before starting new work. Minor documentation-hygiene issues (§5, §6) do not gate progression.
- **Recommended next development wave:** **Wave 2 — Reference Data**, comprising **M14 — External Entities** and **M12 — Test Catalog & Reference Ranges**, exactly as sequenced in `Docs/Source/Top_Lab_Module_Dependency_Map.md` §5 and tracked as "⬜ Not Started" in `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` §5.
- **Immediate next step:** Start **M12 — Test Catalog & Reference Ranges, Slice S1 (Domain behaviors)**: add the mutator/invariant-guard methods to the existing schema-first entities in `src/TopLab.Domain/Tests/` (`Test`, `TestGroup`, `ReferenceRange`, `TestComment`, `WorkGroupLog`, `WorkGroupLogItem`) and `src/TopLab.Domain/Patients/PatientTitle.cs` — including the age-unit-sensitive reference-range rule (BR-04 / FR-M12-005) — with accompanying Domain unit tests, following the proven M22 S1 pattern. (Within Wave 2, M12 is recommended before M14 because it sits on the critical path of eleven downstream modules; see §7.) Per established repository convention, authoring the module implementation plan and slice checklist (the `Module nn Implementation Plan.MD` / `M-nnLoop.MD` working documents) immediately precedes the S1 coding slice.

---

## 2. Investigation snapshot

| Item | Evidence |
|---|---|
| Repository accessed | `git clone https://github.com/El-ogra/Top-Lab.git` succeeded. |
| Target commit exists | `git cat-file -t a8c1a2b61dea98405d40cc47571cc9988bd5d9c1` → `commit`. |
| Checkout performed | `git checkout a8c1a2b61dea98405d40cc47571cc9988bd5d9c1` → `HEAD is now at a8c1a2b`. |
| Verified analysed state | `git rev-parse HEAD` = `a8c1a2b61dea98405d40cc47571cc9988bd5d9c1` (re-verified at the start and immediately before writing this report); `git status --porcelain` returned zero entries (clean tree) before this report file was created. |
| Relationship of target commit to HEAD | The target commit **is** the tip of `main` / `origin/main` at clone time (34 commits total; `git log main --oneline | head -1` = `a8c1a2b`). No commits exist after the target commit, so no post-snapshot work could contaminate the analysis. |
| Temporal boundary | Git history was inspected only at and before the target commit. |
| Scope of investigation | Full repository tree at the target commit: 4 production projects (`src/TopLab.Domain`, `src/TopLab.Application`, `src/TopLab.Infrastructure`, `src/TopLab.Presentation`), 3 test projects (`tests/TopLab.Domain.Tests`, `tests/TopLab.Application.Tests`, `tests/TopLab.Infrastructure.Tests`), ~350 `.cs` files, solution file `TopLab.sln`, configuration (`Directory.Build.props`, `Directory.Packages.props`, `appsettings.example.json`), EF Core migrations, DI composition roots, all ViewModels/Views, and all tracked Markdown documentation. |
| Documentation examined | All of `Docs/Source/`: `Top_Lab_Master_Tracking_Sheet.md`, `Top_Lab_Module_Dependency_Map.md`, `Top_Lab_PRD.md`, `Top_Lab_ADR.md`, `Top_Lab_Architecture_Blueprint.md`, `Top_Lab_Data_Model_Blueprint.md`, `Top_Lab_Test_Strategy.md`, `Top_Lab_UI_UX_Blueprint.md`, `Top_Lab_Reporting_Printing_Blueprint.md`, `Top_Lab_Coding_Standards.md`, `Top_Lab_Handoff_Template.md`; plus `Docs/Handoff_M22.md`, `FoundationPhaseAcceptanceReport.md`, `Top_Lab_Remediation_Report_F1_F6.md`, `INSTALL.txt`. |
| Prior "next stage" investigation report | **None found.** `git ls-tree -r HEAD` and a filename/content search (`*next stage*`, `*Next_Stage*`, `المرحلة التالية`) across all tracked Markdown returned no earlier next-stage determination report. The only pre-existing analytical reports are `FoundationPhaseAcceptanceReport.md` (2026-08-30, foundation-phase PDCA acceptance) and `Top_Lab_Remediation_Report_F1_F6.md` (F1–F6 verify-then-fix audit at commit `896db5b`) — both pre-date Wave 1 and were treated strictly as historical context. This report is a new file; no pre-existing file was modified, overwritten, or deleted. |
| Handoff documents | Only `Docs/Handoff_M22.md` exists at this commit. `Docs/Handoff_M22.md` §11 references a prior "`Handoff_M17`" document, but **no such file is present in the repository** (see Contradiction C1 in §5). |
| Build/test execution in this investigation | **Not executed.** The investigation environment has no .NET SDK, and the Presentation project targets `net8.0-windows` (WPF), which cannot be built on this Linux analysis host. Build/test status claims from documentation were therefore verified by static means only (test-source inventory, code inspection) and are flagged accordingly in §13. This is an evidence limitation, not a code finding. |

---

## 3. Actual implementation state

The code — not the documentation — is the basis for every statement below.

### 3.1 Wave 0 — Foundations (F1–F6): Implemented and verified (D/E)

| Foundation | Code evidence | Status |
|---|---|---|
| F1 solution skeleton | `TopLab.sln`; 4 production + 3 test projects; dependency direction correct (`Presentation → Application/Infrastructure → Domain`; `Domain` references nothing). | Implemented and verified |
| F2 domain common types | `src/TopLab.Domain/Common/`: `Entity.cs`, `AuditableEntity.cs`, `ValueObject.cs`, `DomainException.cs`, `StronglyTypedId.cs`, plus 24 strongly-typed ID classes in `Common/Ids/` (incl. `TestId`, `TestGroupId`, `ExternalEntityId`, `PatientId`, `UserId`) and 17 shared enums in `Common/Enums/` (incl. `AgeUnit`, `Sex`, `EntityType`, `ResultKind`). | Implemented and verified |
| F3 Result pattern & pipeline | `src/TopLab.Application/Common/Results/` (`Result.cs`, `Error.cs`, `ErrorType.cs`); `Common/Behaviors/` (`ValidationBehavior.cs`, `AuthorizationBehavior.cs`, `LoggingBehavior.cs`) registered in order Validation → Authorization → Logging in `src/TopLab.Application/DependencyInjection.cs`. | Implemented and verified |
| F4 persistence baseline | `src/TopLab.Infrastructure/Persistence/ApplicationDbContext.cs` + `ApplicationDbContext.DbSets.cs`; `AuditableEntitySaveChangesInterceptor` in `Persistence/Interceptors/`; `IDateTimeProvider`/`SystemDateTimeProvider`; `ICurrentUserService`/`CurrentUserService`; composition root runs `Database.MigrateAsync()` guarded after host start (`src/TopLab.Presentation/App.xaml.cs`, per F4 addendum in the tracking sheet). | Implemented and verified |
| F5 baseline data model | 36 Fluent-API configurations in `src/TopLab.Infrastructure/Persistence/Configurations/` (one per entity); migration `20260828052248_BaselineDataModel.cs` (36 `CreateTable` calls) + `20260828123530_RenamePkColumns.cs` (54 `RenameColumn` calls) + `ApplicationDbContextModelSnapshot.cs`. **All Wave-2-needed tables (`Tests`, `TestGroups`, `ReferenceRanges`, `TestComments`, `PatientTitles`, `WorkGroupLogs`, `WorkGroupLogItems`, `ExternalEntities`, …) already exist physically** — no migration is required to start M12/M14. | Implemented and verified |
| F6 presentation composition root | `src/TopLab.Presentation/App.xaml.cs` wires `AddApplication()` / `AddInfrastructure(Configuration)` / `AddPresentation()` (lines 53–55); `MainWindow.xaml` shell; `NavigationService`, `DialogService`, `ResultErrorPresenter` in `Common/`; first-run `DatabaseSetupWindow` wizard and `%ProgramData%\TopLab` configuration fallback (ADR-0025). | Implemented and verified |

### 3.2 Wave 1 — M17 User & Permission Management: Implemented and verified

- **Domain:** `src/TopLab.Domain/Users/` — `User.cs` (226 lines, behavioral aggregate with `RecordLogin` and permission/activation mutators), `Permission.cs`, `UserPermissionGrant.cs`.
- **Application:** `src/TopLab.Application/Features/UsersAndPermissions/` — 8 commands (`CreateUser`, `UpdateUser`, `DeactivateUser`, `ReactivateUser`, `DeleteUser`, `SaveUserPermissions`, `SignIn`, `SignOut`) and 5 queries (`GetUsers`, `GetUserById`, `GetCurrentSession`, `HasAnyAbsoluteUser`, `VerifySecondaryPassword`), each with handler and, where applicable, FluentValidation validator. `SignInCommandHandler.cs` enforces uniform failure (`Error.Forbidden("اسم المستخدم أو كلمة المرور غير صحيحة")`) for unknown user and wrong password, matching the M17 acceptance checklist in `Top_Lab_Test_Strategy.md`.
- **Infrastructure:** `Identity/Pbkdf2PasswordHasher.cs` (self-describing `PBKDF2-SHA256$<iterations>$<salt>$<hash>` format, ≥100,000 iterations, constant-time verify — per ADR-0026); 13-row permission catalog seeded in `Persistence/Configurations/PermissionConfiguration.cs` lines 27–30 (incl. `EDIT_SYSTEM_SETTINGS` id 10 and `PT_AUDIT_ACCESS` id 13).
- **Presentation:** first-run administrator wizard `Views/Setup/FirstRunAdminWindow.xaml` invoked from `App.xaml.cs` lines 77–82 when `HasAnyAbsoluteUserQuery` returns none; user-management screen `Views/Users/UserManagementView.xaml` + `ViewModels/Users/UserManagementViewModel.cs`, reached through the secondary-password gate (`ShellViewModel.cs` ≈ lines 132–143, `ShowSecondaryPasswordDialogAsync`).
- **Tests:** `tests/TopLab.Domain.Tests/Users/UserTests.cs` (23 test methods), `tests/TopLab.Application.Tests/Features/UsersAndPermissions/` (handler, validator, and authorization tests), `tests/TopLab.Infrastructure.Tests/Identity/Pbkdf2PasswordHasherTests.cs` + `Persistence/UserGrantPersistenceTests.cs`.
- **Residual gap:** the **sign-in UI** (a login window dispatching `SignInCommand` at startup) does not exist — `SignInCommand` has no Presentation consumer at this commit. That capability belongs to M01 (Wave 3), so it is an expected gap, not an M17 defect (see §6, G2).

### 3.3 Wave 1 — M22 System & Print Settings: Implemented and verified

- **Domain:** `src/TopLab.Domain/Settings/` — six aggregates with real mutators and invariant guards: `SystemSettings.cs` (117 lines), `ReportSettings.cs` (100 lines, top-space 8 cm clamp), `ReceiptSettings.cs`, `EnvelopeSettings.cs`, `EnvelopePrintItemPosition.cs`, `PrinterAssignment.cs`.
- **Application:** `src/TopLab.Application/Features/SystemAndPrintSettings/` — 10 commands (`UpdateSystemSettings`, `UpdateReportSettings`, `UpdateReceiptSettings`, `UpdateEnvelopeSettings`, `SavePrinterAssignments`, `SaveLabPrintText`, `ApplyDatabaseUpdates`, `BackupDatabaseNow`, `RestoreDatabase`, `UpdateDatabaseServerSettings`) + 8 queries (`GetSystemSettings`, `GetReportSettings`, `GetReceiptSettings`, `GetEnvelopeSettings`, `GetPrinterAssignments`, `GetLabPrintText`, `CheckBackupPath`, `GetDatabaseServerSettings`) + DTOs in `Common/`. Write commands carry `IAuthorizedRequest` with `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` (e.g. `UpdateSystemSettingsCommand.cs`), and single-row configuration semantics (ADR-0020) are enforced by handlers (`UpdateSystemSettingsCommandHandler.cs` reads `SingleOrDefault(s => s.Id == 1)`).
- **Infrastructure:** `Persistence/Maintenance/SqlServerDatabaseMaintenanceService.cs` (`IDatabaseMaintenanceService`), `Backup/DailyBackupHostedService.cs` (`BackgroundService`), seed rows via `HasData` (e.g. `SystemSettingsConfiguration.cs` line 26; `EnvelopePrintItemPositionConfiguration.cs` line 16), seed-repair routine (per `Docs/Handoff_M22.md` §3 S4).
- **Presentation:** six ViewModels in `ViewModels/Settings/` (`SettingsDashboardViewModel`, `SystemSettingsViewModel`, `ReportSettingsViewModel`, `ReceiptSettingsViewModel`, `EnvelopeSettingsViewModel`, `DatabaseMaintenanceViewModel`) with six matching XAML views in `Views/Settings/`; printer catalog via `Services/PrinterCatalogService.cs`; workstation-local lab print text via `Services/Configuration/JsonLabPrintTextStore.cs` (`lab-print-text.json`, per ADR-0027); Database Maintenance gated by the M17 secondary-password dialog; shell navigation wired (`ShellViewModel.cs` ≈ lines 145–147, "الإعدادات" → `SettingsDashboardViewModel`).
- **Tests:** `tests/TopLab.Domain.Tests/Settings/` (7 files, 40 test methods) and `tests/TopLab.Application.Tests/Features/SystemAndPrintSettings/` (24 files, incl. handler, validator, and `SystemAndPrintSettingsAuthorizationTests.cs`).
- **Known documented exclusions (not defects):** no image and no color print-configuration controls (FR-M22-004/005 deliberately excluded; ADR-0027); envelope barcode preview is a static placeholder rectangle (ADR-0027).

### 3.4 Everything from Wave 2 onward: Planned only (A) — with schema-first baseline artifacts

- The Application layer contains exactly four feature folders: `SamplePipeline` (test scaffolding only — `EchoNameCommand` exists solely to exercise pipeline behaviors, per its own XML doc-comment), `AccessAndNavigation` (contains **only** `CheckDatabaseConnectivityQuery`), `UsersAndPermissions` (M17), `SystemAndPrintSettings` (M22). There is **no** `TestCatalogAndReferenceRanges`, `ExternalEntities`, `PatientRegistration`, or any other M01–M16/M18–M23 feature folder.
- The Domain layer contains schema-first entities for later modules (F5 artifacts) with **factory-only or minimal behavior**: `ExternalEntities/ExternalEntity.cs` (`Create` only, with name-required and "TreatingDoctor must not have PriceListId" guards), `Tests/Test.cs` (`Create` + one `Update`), `Tests/ReferenceRange.cs` (`Create` with `AgeMin <= AgeMax` / `MinValue <= MaxValue` guards + a `Matches(Sex, AgeUnit, int)` predicate), `Tests/TestGroup.cs`, `Tests/TestComment.cs`, `Tests/WorkGroupLog.cs`/`WorkGroupLogItem.cs`, `Patients/PatientTitle.cs` (`Create` only). These are persistence shapes from F5, **not** delivered module behavior: there are no commands, queries, DTOs, validators, ViewModels, or Views for them.
- `src/TopLab.Domain/PatientStatus/PatientStatusCalculator.cs` line 8 carries `/// TODO: implement seven-state min-over-stages + account check when M04 ships.` and line 15 throws `NotImplementedException("Full precedence delivered in M04 per Master Tracking Sheet Cross-Cutting Concern (BR-01, ADR-0015).")` — an explicit, by-design deferred implementation.
- The shell navigation (`ShellViewModel.BuildNavigationItems()`, lines 115–155) builds 11 Arabic navigation titles (`المرضى`, `المعمل`, `ورقة العمل`, `الأدوات`, `الحسابات`, `الإحصائيات`, `المستخدمون`, `النظام`, `الإعدادات`, `حول البرنامج`, `خروج`) but only three do anything: `خروج` shuts down, `المستخدمون` opens the gated user screen, `الإعدادات` opens the settings dashboard. All other items fall into `// Future: navigate to feature` (line ≈149) — navigation placeholders, not implemented modules.
- No TODO/NotImplemented markers other than the above were found in `src/` (the two `NotSupportedException` occurrences in `Common/Converters/` are standard one-way WPF converter `ConvertBack` bodies, not deferred work).

### 3.5 Test inventory (static count)

| Project | `[Fact]`+`[Theory]` methods | Test files |
|---|---|---|
| `tests/TopLab.Domain.Tests` | 97 | 25 (incl. `Settings/`, `Users/`, `Billing/`, `Patients/`, `Results/`, `Tests/`, `Common/`) |
| `tests/TopLab.Application.Tests` | 119 | 34 (incl. 8 fakes in `Common/Fakes/`) |
| `tests/TopLab.Infrastructure.Tests` | 31 | 16 |
| **Total** | **247 methods** (+30 `[InlineData]` rows → ≈ 268 executed cases) | **75** |

The static count (247 methods + 30 theory data rows ≈ 268 executed cases) is consistent with the "268 tests green" claim in `Docs/Handoff_M22.md` §4.2, but the suite was **not re-executed** in this investigation (no .NET SDK available — see §2).

---

## 4. Current stage determination

**Determination: Wave 0 and Wave 1 are complete; the project stands at the Wave 1 → Wave 2 boundary and is ready to start Wave 2.**

Evidence chain:

1. `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` §5 marks Wave 0 (F1–F6) and Wave 1 (M17, M22) "🟩 Done" and Wave 2 (M14, M12) "⬜ Not Started" (lines 93–95). §4 marks M17 and M22 "🟩 Done" (lines 63–64) and every module M01–M16, M18–M23 "⬜ Design".
2. The documentation status is **independently corroborated by code**: every Wave 0/1 item in §3.1–§3.3 above resolves to concrete classes, handlers, configurations, views, and tests (classification D/E), while every post-Wave-1 module resolves to at most F5 schema artifacts with factory-only domain classes and no Application/Presentation surface (classification A).
3. Both Wave 1 modules are usable end-to-end within their scope: M17 from first-run provisioning (`App.xaml.cs` lines 77–82) through sign-in/sign-out commands, secondary-password gate, and CRUD screen; M22 from the shell's "الإعدادات" navigation through the settings dashboard, five settings screens, the gated Database Maintenance window, the permission-gated Application write surface, and the SQL/JSON/backup Infrastructure services.
4. Exit criteria for Wave 1 are satisfied: the authorization pipeline and permission catalog (the "Configuration Backbone" purpose stated in `Top_Lab_Module_Dependency_Map.md` §5) are live and consumed — M22's write commands are already gated by the M17 permission `EDIT_SYSTEM_SETTINGS` and the M17 secondary-password dialog, which is precisely the cross-module interaction Wave 1 was designed to establish.
5. There is no evidence of an in-flight third module at the target commit: no other feature folders, no partially wired screens, no WIP branches (`git branch -a` shows only `main`), and `Docs/Handoff_M22.md` §10 explicitly states "No further Module 22 implementation work is outstanding."

The stage was **not** inferred from roadmap numbering: it was determined by the absence of any Application/Presentation implementation for post-Wave-1 modules (§3.4) and the presence of tested, end-to-end implementations for exactly the Wave 0 and Wave 1 scope.

---

## 5. Documentation versus implementation

Structured comparison of the load-bearing claims. Status codes: A = Planned, B = Documented as complete (unverified), C = Partially implemented, D = Implemented, E = Implemented and verified.

| # | Documentation statement | Expected capability | Actual code evidence | Status | Gap / contradiction | Impact on next-stage decision |
|---|---|---|---|---|---|---|
| 1 | Tracking sheet §4/§5: F1–F6 "🟩 Done" (lines 51–56, 93) | Full four-layer platform with persistence, pipeline, shell | §3.1: all artifacts present; composition root in `App.xaml.cs` lines 53–55; 2 migrations; 36 configurations | E | None | Wave 0 dependency for Wave 2 confirmed by code. |
| 2 | Tracking sheet §4: M17 "🟩 Done … 162 tests green" (line 63) | Users, sign-in/out, secondary gate, CRUD, wizard | §3.2: full stack present incl. `SignInCommandHandler.cs`, `Pbkdf2PasswordHasher.cs`, `FirstRunAdminWindow.xaml` | E | None material. ("162 tests" was the count at M17 close; it is now part of the ≈268 total.) | M17 dependency for Wave 2 confirmed by code. |
| 3 | Tracking sheet §4: M22 "🟩 Done … 268 tests green" (line 64); `Handoff_M22.md` §4.2 | Six settings aggregates, read/write surface, maintenance, 6 screens | §3.3: full stack present incl. 6 VMs + 6 XAML views, `SqlServerDatabaseMaintenanceService.cs`, `DailyBackupHostedService.cs` | E (implementation); B (test execution — see §13 U1) | Test suite not re-executed here; static count consistent. | M22 dependency for Wave 2 confirmed by code. |
| 4 | Dependency map §4/§5: Wave 2 = M14 + M12, depending on M17, M22 (lines 83–84, 124–127) | Wave 2 unblocked once M17+M22 done | M17, M22 verified in code (rows 2–3); F5 schema for both modules already migrated | D | None | This is the controlling fact for the next-wave selection. |
| 5 | PRD §M12 (FR-M12-001…006, lines 273–278) and §M14 (FR-M14-001…006, lines 288–293) | Test catalog CRUD, age-unit-sensitive ranges, groups/Log config; external-entity CRUD with type discrimination | Domain shapes only: `Test.cs` (Create/Update), `ReferenceRange.cs` (Create/Matches), `ExternalEntity.cs` (Create only). No feature folders `TestCatalogAndReferenceRanges`/`ExternalEntities`; no DTOs, handlers, validators, screens | A | Documentation describes requirements (not completion) — no false completion claim; but the *capability* is entirely absent from code | Both modules must be built from the Application layer upward; Domain/persistence groundwork exists. |
| 6 | `Handoff_M22.md` §7: "Open Issues, Bugs and Risks — None known." | No known defects/deferred work | Code contains `PatientStatusCalculator.cs` line 15 `NotImplementedException` (deferred to M04 by design) and 8 unwired shell navigation stubs (`ShellViewModel.cs` line ≈149 `// Future: navigate to feature`) | C (self-report inaccurate in detail) | **Contradiction C2:** the "None known" declaration omits known deferred items. Both are by-design deferrals (M04, M01+) rather than defects, so the practical impact is nil — but the self-reported "None" was verified against code and found imprecise, per this investigation's standing instruction to distrust handoff self-reports | None on the wave decision; reinforces that handoff claims require code verification. |
| 7 | `Handoff_M22.md` §11 "Required Reading": "Prior handoff documents for adjacent modules — `Handoff_M17`" | M17 handoff file exists in repo | `git ls-tree -r HEAD` shows only `Docs/Handoff_M22.md` + `Docs/Source/Top_Lab_Handoff_Template.md`; no `Handoff_M17*` file at this commit | — | **Contradiction C1:** referenced document does not exist in the repository (it was presumably kept untracked locally, like the M22 planning files) | None on implementation; a documentation-completeness gap for future agents (§6, G5). |
| 8 | Tracking sheet §7 cross-cutting table: "Validation … ⬜ Design", "Logging … ⬜ Design", "Audit columns … ⬜ Design", "Time provider … ⬜ Design", "Current-user context … ⬜ Design" (with notes "Delivered by F3/F2/F4") | Symbols should read Done | `ValidationBehavior.cs`, `LoggingBehavior.cs`, `AuditableEntitySaveChangesInterceptor`, `SystemDateTimeProvider.cs`, `CurrentUserService.cs` all exist and are wired (`DependencyInjection.cs`, `AddInfrastructure`) | E (code); stale symbols (docs) | **Contradiction C3:** status symbols were never updated after F2–F4 delivery; the note text contradicts the symbol. Code wins. | None; cosmetic tracking-debt only (§6, G4). |
| 9 | UI/UX Blueprint §5.10 mapping (line 202): "S-32 Sample Draw / Separation → `SampleCollectionViewModel`" vs `Handoff_M22.md` §2/§3: "S-32 … Database Maintenance" | Consistent screen numbering | `Views/Settings/DatabaseMaintenanceView.xaml` exists (M22 usage); no `SampleCollectionViewModel` exists (M21, Wave 4) | D (M22 screen); A (M21 screen) | **Contradiction C4:** screen ID `S-32` is assigned to two different screens across documents (blueprint line 202 vs the M22 handoff and tracking-sheet line 64). The implemented screen is the Database Maintenance one; the blueprint's M21 mapping remains unimplemented | None functionally; a numbering collision that should be reconciled when M21 is designed (§6, G6). |
| 10 | Dependency map §2 / tracking sheet §3: F5 delivered "baseline entity schemas across all entity groups" | Schema exists for all groups | 36 configurations + `BaselineDataModel` migration (36 `CreateTable`) verified, incl. all M12/M14 tables | E | None | Wave 2 needs **no new migration** for its core entities — a material readiness advantage. |
| 11 | `Handoff_M22.md` §10: "No further Module 22 implementation work is outstanding." | M22 closed | All S1–S7 code committed (`d1bfebf`…`ac77cc8`) and target commit `a8c1a2b` is the S8 docs finalize; code inspection found no M22 TODO/stub | D | None | Confirms Wave 1 closure; frees capacity for Wave 2. |

**Contradictions summary:** C1 (missing `Handoff_M17` file), C2 ("None known" vs known by-design deferrals), C3 (stale ⬜ symbols for delivered cross-cutting concerns), C4 (S-32 screen-ID collision). In every case the **code controlled the decision**: none of C1–C4 indicates missing implementation that would block Wave 2.

---

## 6. Remaining gaps and blockers

**Gaps that gate future waves (not Wave 2):**

- **G1 — M01 sign-in surface missing (Wave 3).** `SignInCommand` has no Presentation consumer; there is no login window at startup. Modules whose PRD flows assume an authenticated interactive session (M02 onward) ultimately need M01. Wave 2 (M12/M14) does not: its acceptance items are CRUD/reference-data surfaces already gateable by the existing authorization pipeline and secondary-password dialog.
- **G2 — 8 shell navigation entries are stubs** (`ShellViewModel.cs` line ≈149 `// Future: navigate to feature`). Expected at this stage; each is filled by its owning module (M01/M02/M08/M11/M19/M20/M23…). Not a Wave 2 blocker because M12/M14 entry points are specified under the System pane ("System → بيانات التحاليل", "System → الجهات الخارجية والمعامل", PRD FR-M12-001/FR-M14-001), consistent with how M22 screens hang off the settings dashboard.
- **G3 — `PatientStatusCalculator` deliberately unimplemented** (line 15 `NotImplementedException`), deferred to M04 (Wave 5) by its own TODO and by ADR-0015. Must **not** be completed early (§10).

**Non-blocking gaps / hygiene (fix opportunistically, do not gate the wave):**

- **G4 — Stale tracking symbols** in tracking sheet §7 (Contradiction C3): five delivered cross-cutting concerns still show "⬜ Design".
- **G5 — Missing `Handoff_M17.md`** (Contradiction C1): the M17 session record referenced by `Handoff_M22.md` §11 is not in the repository. Future agents lose the M17 decision context (ADR-0026 partially compensates).
- **G6 — S-32 screen-ID collision** (Contradiction C4) between the UI/UX Blueprint (M21 sample collection) and the M22 handoff (Database Maintenance). Reconcile before M21 design.
- **G7 — Test execution not independently verified in this investigation** (environment limitation; §13 U1). Recommended first CI/build-machine action before S1 merge: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln -m:1` on a Windows host to re-baseline the "268 green" claim.

**Blockers for Wave 2: none found.** Both declared dependencies (M17, M22) are implemented in code; the F5 schema already contains every table M12/M14 require (verified: `TestConfiguration.cs`, `TestGroupConfiguration.cs`, `ReferenceRangeConfiguration.cs`, `TestCommentConfiguration.cs`, `PatientTitleConfiguration.cs`, `WorkGroupLogConfiguration.cs`, `WorkGroupLogItemConfiguration.cs`, `ExternalEntityConfiguration.cs` + migration), so Wave 2 starts with zero schema work and zero infrastructure prerequisites.

---

## 7. Candidate next waves

| Candidate | Description | Evaluation against actual state |
|---|---|---|
| **C-i — Wave 2: Reference Data (M14 + M12)** | Build external-entity management and the test catalog with reference ranges | Dependencies (M17, M22) verified complete in code (§3.2–§3.3). Schema pre-built by F5. Directly unblocks Wave 3 (M13 needs M12+M14; M15 needs M12) and Wave 4 (M02 needs M12+M14). Matches the binding execution order (`Top_Lab_Module_Dependency_Map.md` §5, "Final — binding implementation order baseline"). |
| C-ii — M01 (Application Access & Main Navigation) early | Pull the login screen forward from Wave 3 | Dependency (M17) is satisfied, so it is *technically* buildable. But M01 unblocks nothing in Wave 2, its permission-denial/navigation behavior has no downstream consumer until M02 (Wave 4), and pulling it forward violates the binding map without cause. The absence of a login screen does not block reference-data CRUD (M17's own management screen already operates without one). Rejected as the primary wave; keep in Wave 3. |
| C-iii — Skip to M02 (Patient Registration) | Jump to the first patient-facing module | Explicitly forbidden by evidence: M02 depends on M01, M12, M13, M14, M22 (dependency map §4), of which only M22 exists. Building M02 now would require stubbing the test catalog, price lists, and external entities — manufacturing throwaway work. Rejected. |
| C-iv — Hardening-only pause (re-run tests, fix doc hygiene) | No new module; close G4/G5/G7 | G7 (test re-execution) is a 30-minute machine task that can run *inside* Wave 2 S1 setup; G4/G5/G6 are cosmetic. A full wave of hardening would idle the project with no requirement demanding it. Rejected as the primary wave; absorbed as preconditions/hygiene within Wave 2. |

**Selection: C-i — Wave 2 (M14 + M12)** is the single recommended next wave. Within the wave the modules are mutually independent (dependency map §1: "Within a wave, modules are independent of each other"), but if executed sequentially **M12 should go first**: the dependency graph (§6 diagram, edges `M12 ──┬── M02 … ├── M13 … ├── M15 └── M16`) shows M12 feeding eleven downstream modules versus M14's five, and M12 carries the business-rule-dense age-unit matching rule (BR-04/FR-M12-005) that benefits most from early, heavily tested implementation.

---

## 8. Recommended next development wave

**Wave 2 — Reference Data: M14 — External Entities, and M12 — Test Catalog & Reference Ranges.**

- **Purpose:** Deliver the reference-data surfaces on which all patient-facing work rests: the external-entity register (treating doctors, referral/contract entities, partner labs — FR-M14-001…006) and the test catalog with groups, work-group (Log) configuration, patient titles, and age-unit-sensitive reference ranges (FR-M12-001…006, BR-04, BR-13).
- **Scope:**
  - M12: `TestGroup`, `Test`, `ReferenceRange`, `TestComment`, `PatientTitle`, `WorkGroupLog`/`WorkGroupLogItem` behaviors; catalog search (name / group / test number); test add/edit/save/cancel; reference-range add/edit/delete with low/high comments; age-unit-sensitive matching (no cross-unit conversion); test-group and Log-group configuration screens under the System pane.
  - M14: unified `ExternalEntity` management with type discrimination (TreatingDoctor / ReferralOrContract / PartnerLab); price-list assignment rules (none for treating doctors; required-choice for referral/contract); ID-generation action; edit/delete; the empty-referral-field default-placeholder behavior tied to the M22 system settings (`SaveTreatingDoctorOnlyFromEntityWindow` already exists in `SystemSettings` — the first downstream consumer of Wave 1 configuration).
  - New Application feature folders: `Features/TestCatalogAndReferenceRanges/` and `Features/ExternalEntities/` (names per dependency map §3), following the M17/M22 command/query/validator/DTO pattern.
  - Presentation screens per the UI/UX Blueprint, reached from the System pane, permission-gated via the existing authorization pipeline (permission codes per the 13-row catalog; `EDIT_SYSTEM_SETTINGS` already covers "Edit system and test settings").
- **Why now:** Both declared dependencies are implemented in code (not merely documented); the physical schema for every needed table already exists (F5), so the wave is pure behavior/surface work with no migration risk; the wave is the unique unlock for Waves 3–4; and the project has just demonstrated (M17, M22) a working, repeatable slice pattern for exactly this kind of module.
- **Dependencies:** M17 (permission catalog, authorization pipeline, secondary-password gate), M22 (system-settings flags consumed by M14 flows), F1–F6 (platform, schema). All verified present (§3).
- **Preconditions:** (P1) re-baseline build/tests on a Windows host (`dotnet build TopLab.sln` 0/0; `dotnet test` ≈268 green) to convert claim B→E (§13 U1); (P2) author the Wave 2 implementation plan + slice checklist per repository convention (the untracked `Module nn Implementation Plan.MD` / `M-nnLoop.MD` working documents, as done for M22); (P3) confirm the S-32 numbering reconciliation approach (G6) so M21's future screen ID does not clash in updated docs.
- **Expected outcome:** Both modules reach "🟩" in the tracking sheet with handler/validator/domain tests green; FR-M12-001…006 and FR-M14-001…006 demonstrable end-to-end; M12 acceptance items in `Top_Lab_Test_Strategy.md` §7.2 (age-unit-sensitive matching; low/high comments) satisfied; tracking sheet, ADR log (next sequential ADRs), and per-module handoff documents updated per convention.
- **Explicitly out of scope for this wave:** the M01 login window and main-navigation completion; any patient-facing flow (M02/M21); price-list *pricing behavior* beyond the assignment surface M14 needs (full M13 belongs to Wave 3); culture/antibiotic configuration (M15, Wave 3); completing `PatientStatusCalculator` (M04, Wave 5); report printing and the Reporting/Printing blueprint surfaces (Wave 7); image/color print configuration (excluded by ADR-0027); any new EF Core migration for the existing 36 tables.

---

## 9. Immediate next implementation step

**Start M12 — Test Catalog & Reference Ranges, Slice S1: Domain behaviors and invariant guards on the test-catalog aggregates.**

- **Concrete objective:** Extend the schema-first entities `src/TopLab.Domain/Tests/Test.cs`, `TestGroup.cs`, `ReferenceRange.cs`, `TestComment.cs`, `WorkGroupLog.cs`, `WorkGroupLogItem.cs`, and `src/TopLab.Domain/Patients/PatientTitle.cs` with the mutator methods and invariant guards required by FR-M12-001…006 — e.g. test rename/report-name/receipt-name/group/barcode/completion-duration/sent-out-flag/pricing updates (`Test.Update` already exists and should be split/extended to match the editable field set in FR-M12-002), reference-range add/edit/delete guards preserving the age-unit rule (BR-04: `AgeUnit` is never converted — `ReferenceRange.Matches(Sex, AgeUnit, int)` must remain exact-unit), group/Log membership behaviors, and patient-title default handling — each with Domain unit tests under `tests/TopLab.Domain.Tests/Tests/` (and `Patients/`), exactly mirroring the M22 S1 pattern (commit `d1bfebf`, `tests/TopLab.Domain.Tests/Settings/`).
- **Relevant area of the codebase:** `src/TopLab.Domain/Tests/`, `src/TopLab.Domain/Patients/PatientTitle.cs`, `tests/TopLab.Domain.Tests/Tests/`, `tests/TopLab.Domain.Tests/Patients/`.
- **Why this first:** Domain behaviors are the bottom slice of the established S1→S8 pattern (domain → application read → application write → infrastructure → presentation → hardening/docs); the entities already exist as F5 persistence shapes, so S1 is the first slice that adds *new* behavior; and the age-unit rule (BR-04/FR-M12-005) is the highest-risk business rule in the wave, so it should be pinned by unit tests before any Application or UI code depends on it.
- **Dependencies:** none beyond the existing platform (F2 common types, F5 schema). No migration, no NuGet addition, no Infrastructure change is needed for S1.
- **Immediate precondition (same working session, before code):** author the M12 implementation plan and slice checklist per repository convention (as `797892f` did before M22), and re-baseline build/tests on a Windows host (P1/G7).
- **Expected completion condition:** the new mutators/guards compile with 0 errors/0 warnings; new + existing Domain tests green; the BR-04 non-conversion rule covered by explicit test cases (e.g. a 1–60-day range must NOT match a 1-month-old patient); no change outside Domain + Domain tests; tracking-sheet M12 block moved to "🟨 In Progress" with a §9 change-log row.

---

## 10. Suggested implementation sequence

Planning recommendation only — not executed by this investigation. Mirrors the proven M17/M22 slice pattern.

1. **W2-S0 — Planning & re-baseline:** author `Module 12 Implementation Plan.MD` / `M-12Loop.MD` (untracked working docs per convention); run `dotnet build`/`dotnet test` on Windows to re-baseline (P1); reconcile S-32 doc numbering note (G6).
2. **M12 S1 — Domain behaviors** (the immediate next step, §9) + Domain tests.
3. **M12 S2 — Application read surface:** catalog search query (name/group/number per FR-M12-001), per-test detail, reference-range list, groups/Log queries + DTOs + test fakes.
4. **M12 S3 — Application write surface:** add/edit test (incl. completion duration per BR-13), add/edit/delete reference range with low/high comments, group/Log management commands + validators + `IAuthorizedRequest` permission declarations.
5. **M12 S4 — Infrastructure:** any query-specific persistence needs (the schema exists; expected near-zero — verify against plan; add a migration only if the plan proves a schema gap).
6. **M12 S5–S7 — Presentation:** test-catalog screen, test edit dialog, reference-range editor, groups/Log configuration; System-pane entry; permission-denial presentation per M17 pattern.
7. **M12 S8 — Hardening, docs, handoff:** tracking sheet rows, ADR if any decision arises (e.g. ID-generation semantics), `Handoff_M12.md`.
8. **M14 S1–S8 — External Entities:** repeat the same slice pattern for `ExternalEntity` (Domain guards already partly exist — `Create` enforces the treating-doctor/no-price-list rule; extend for edit/delete/ID-generation and the price-list assignment rules of FR-M14-002/003), its Application surface, and its screens, consuming the M22 `SaveTreatingDoctorOnlyFromEntityWindow` setting.
9. **Wave 2 close-out:** audit against `Top_Lab_Test_Strategy.md` §7.2 M12 items; flip tracking sheet §4 rows and §5 Wave 2 to "🟩"; then proceed to Wave 3 (M13, M15, M01).

---

## 11. Acceptance criteria

**For the immediate next step (M12 S1):**

- A1 — All new mutator/guard methods compile; `dotnet build TopLab.sln` reports 0 errors / 0 warnings on a Windows host.
- A2 — New Domain tests cover: every new mutator; every invariant guard's negative path; and BR-04 explicitly — a `ReferenceRange` with `AgeUnit.Day` (1–60 days) matches 15-day-old and 35-day-old patients and does **not** match a 1-month-old patient (FR-M12-005).
- A3 — Full suite green (previous ≈268 + new Domain tests) via `dotnet test TopLab.sln -m:1`.
- A4 — Diff touches only `src/TopLab.Domain/**` and `tests/TopLab.Domain.Tests/**` (+ the untracked planning docs); no migration, no `.csproj`/package change, no Infrastructure/Application/Presentation change.
- A5 — Tracking sheet M12 block shows "🟨 In Progress" with a dated §9 change-log entry.

**For the wider Wave 2 (M12 + M14 complete):**

- W1 — FR-M12-001…006 demonstrable end-to-end through the UI; FR-M14-001…006 demonstrable end-to-end.
- W2 — `Top_Lab_Test_Strategy.md` §7.2 M12 checklist items pass (age-unit-sensitive matching; low/high comments at the correct boundary).
- W3 — All write paths reject unauthorized callers through `AuthorizationBehavior` (tests analogous to `SystemAndPrintSettingsAuthorizationTests.cs` exist and pass).
- W4 — Build 0/0; full suite green; handoff documents `Handoff_M12.md` / `Handoff_M14.md` produced per `Top_Lab_Handoff_Template.md`; tracking sheet §4/§5 updated with §9 change-log rows.
- W5 — No migration added unless a documented ADR justifies a schema gap discovered against the F5 baseline; no new NuGet packages beyond the pinned set (`Directory.Packages.props`).

---

## 12. Evidence index

**Repository / Git**

- Commit analysed: `a8c1a2b61dea98405d40cc47571cc9988bd5d9c1` (verified twice via `git rev-parse HEAD`); subject `docs(m22): finalize module 22 documentation, ADR-0027, tracking sheet, and handoff`; 2026-09-02 01:27:20 +0300; tip of `main` (no later commits exist).
- M22 slice commits: `d1bfebf` (S1 domain), `14ba0f1` (S2 read), `a8cbf5a` (S3 write), `3f41c8e` (S4 infra), `c267944` (S5), `9d99eae` (S6), `ac77cc8` (S7), `a8c1a2b` (S8 docs).
- M17 commits: `4629e54`…`bbb2dd2`, docs `1ab0772`. Foundations: `194220f`/`94b5213`/`61cad11`/`896db5b`/`a035904`/`7cc5515`/`17b4049`.

**Code (paths relative to repository root at the target commit)**

- `src/TopLab.Application/DependencyInjection.cs` — pipeline order Validation → Authorization → Logging (F3/ADR-0009).
- `src/TopLab.Presentation/App.xaml.cs` lines 53–55 — `AddApplication()`/`AddInfrastructure()`/`AddPresentation()`; lines 77–82 — `HasAnyAbsoluteUserQuery` + `FirstRunAdminWindow` first-run provisioning.
- `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` lines 115–155 — `BuildNavigationItems()`; line 117 nav titles; lines 128–130 `خروج` shutdown; ≈132–143 gated `المستخدمون`; ≈145–147 `الإعدادات`; ≈149 `// Future: navigate to feature` stubs.
- `src/TopLab.Application/Features/UsersAndPermissions/Commands/SignIn/SignInCommandHandler.cs` — uniform sign-in failure; `RecordLogin` on success.
- `src/TopLab.Application/Features/SystemAndPrintSettings/Commands/UpdateSystemSettings/UpdateSystemSettingsCommand.cs` — `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"`; `UpdateSystemSettingsCommandHandler.cs` — single-row (`Id == 1`) ADR-0020 semantics.
- `src/TopLab.Domain/Settings/` — `SystemSettings.cs`, `ReportSettings.cs`, `ReceiptSettings.cs`, `EnvelopeSettings.cs`, `EnvelopePrintItemPosition.cs`, `PrinterAssignment.cs` (mutators/invariant guards).
- `src/TopLab.Domain/Users/User.cs` (226 lines), `Permission.cs`, `UserPermissionGrant.cs`.
- `src/TopLab.Domain/Tests/` — `Test.cs` (`Create`, `Update`), `ReferenceRange.cs` lines 55–81 (`Create` guards, `Matches(Sex, AgeUnit, int)`), `TestGroup.cs`, `TestComment.cs`, `WorkGroupLog.cs`, `WorkGroupLogItem.cs`, `Antibiotic.cs`, `CultureAntibioticAttachment.cs`, `CustomGroup.cs`, `CustomGroupItem.cs`.
- `src/TopLab.Domain/ExternalEntities/ExternalEntity.cs` — `Create` only; guards: name required; `TreatingDoctor` must not have `PriceListId`.
- `src/TopLab.Domain/PatientStatus/PatientStatusCalculator.cs` line 8 (TODO) / line 15 (`NotImplementedException`, deferred to M04).
- `src/TopLab.Infrastructure/Persistence/Configurations/` — 36 configurations; `PermissionConfiguration.cs` lines 27–30 (13-seed catalog incl. `EDIT_SYSTEM_SETTINGS`, `PT_AUDIT_ACCESS`); `SystemSettingsConfiguration.cs` line 26 (`HasData` single-row seed); `EnvelopePrintItemPositionConfiguration.cs` line 16 (`HasData`).
- `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` (36 `CreateTable`), `20260828123530_RenamePkColumns.cs` (54 `RenameColumn`), `ApplicationDbContextModelSnapshot.cs`.
- `src/TopLab.Infrastructure/Persistence/Maintenance/SqlServerDatabaseMaintenanceService.cs`; `src/TopLab.Infrastructure/Backup/DailyBackupHostedService.cs`; `src/TopLab.Infrastructure/Identity/Pbkdf2PasswordHasher.cs`.
- `src/TopLab.Presentation/ViewModels/Settings/` (6 VMs) + `src/TopLab.Presentation/Views/Settings/` (6 XAML views) incl. `DatabaseMaintenanceView.xaml`.
- `tests/` — 75 files; 247 `[Fact]`/`[Theory]` methods (97 Domain / 119 Application / 31 Infrastructure) + 30 `[InlineData]` rows ≈ 268 executed cases; `tests/TopLab.Application.Tests/Common/Fakes/` (8 fakes).
- Placeholder pipeline (not a feature): `src/TopLab.Application/Features/SamplePipeline/Commands/EchoName/` — test scaffolding per its own doc-comment; `Features/AccessAndNavigation/` contains only `CheckDatabaseConnectivityQuery`.

**Documentation**

- `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` — §4 lines 63–64 (M17/M22 Done), lines 65–66 (M14/M12 Not Started); §5 lines 93–103 (wave statuses); §7 (stale cross-cutting symbols, C3); §9 line 364 (M22 change log).
- `Docs/Source/Top_Lab_Module_Dependency_Map.md` — §3 module catalogue; §4 lines 83–84 (M14/M12 depend on M17, M22); §5 lines 124–127 (Wave 2 definition); §6 dependency diagram (M12 fan-out).
- `Docs/Source/Top_Lab_PRD.md` — lines 273–278 (FR-M12-001…006), lines 288–293 (FR-M14-001…006), line 392 (BR-04), line 401 (BR-13).
- `Docs/Source/Top_Lab_Test_Strategy.md` — §7.2 lines 232–235 (M12 acceptance items), M17 checklist (uniform sign-in failure, first-run wizard, last-absolute-user floor).
- `Docs/Source/Top_Lab_UI_UX_Blueprint.md` — lines 192–196 (S-27…S-31 settings screens, implemented), line 202 (S-32 → `SampleCollectionViewModel`, C4), line 238 (feature mapping).
- `Docs/Source/Top_Lab_ADR.md` — ADR-0020 (single-row config), ADR-0021 (workstation-locality), ADR-0025 (`%ProgramData%\TopLab`), ADR-0026 (M17 security/floor/provisioning, line 537), ADR-0027 (M22 workstation-local lab text; no images/colors, line 566).
- `Docs/Handoff_M22.md` — §3 (S1–S8 achievements), §4.2 (268 tests claim), §5 (WIP = docs finalize), §7 ("None known", C2), §10 (next-session objective, M22 closed), §11 (references non-existent `Handoff_M17`, C1), §13 (untracked planning docs convention).
- `FoundationPhaseAcceptanceReport.md` (2026-08-30, foundation PDCA acceptance) and `Top_Lab_Remediation_Report_F1_F6.md` (F1–F6 audit at `896db5b`) — prior reports, historical context only.

---

## 13. Confidence and uncertainty

| Judgment | Confidence | Basis |
|---|---|---|
| Current stage = Wave 0 + Wave 1 complete; project at the Wave 1 → Wave 2 boundary | **High** | Direct code evidence for every claimed-delivered item (§3.1–§3.3); direct absence evidence for every post-Wave-1 module (§3.4: no feature folders, no handlers, no screens, factory-only domain shapes, explicit TODO/`NotImplementedException` deferral). |
| Next wave = Wave 2 (M14 + M12) | **High** | Binding execution order in `Top_Lab_Module_Dependency_Map.md` §5; both dependencies verified implemented in code; schema pre-built by F5; all alternative candidates fail dependency or value tests (§7). |
| Immediate next step = M12 S1 domain behaviors | **High (direction) / Moderate (M12-before-M14 ordering)** | The slice pattern is proven twice (M17, M22) and S1 is unambiguously the first slice. Within-wave ordering (M12 before M14) rests on fan-out/critical-path reasoning, not on a hard dependency — a parallel or M14-first execution would also be valid; M12-first is the recommendation, not a requirement. |

**Stated uncertainties / evidence limitations:**

- **U1 — Test execution not independently verified.** No .NET SDK was available in the investigation environment, and the WPF Presentation project (`net8.0-windows`) cannot build on the Linux analysis host. The "268 tests green / build 0-0" figures are documentation claims (`Docs/Handoff_M22.md` §4.2) that are *consistent* with the static test inventory (247 methods + 30 `[InlineData]` rows ≈ 268 cases) but were not re-executed. Mitigation: precondition P1 (§8) re-baselines on a Windows host before Wave 2 code is written. This limitation does not weaken the structural conclusions, which rest on file/class-level presence-absence evidence.
- **U2 — Runtime behavior not exercised.** End-to-end usability of M17/M22 screens (e.g. actual backup/restore against a live SQL Server, actual printer enumeration) was assessed from code wiring, not from execution. Classified accordingly (E for code+tests; runtime smoke remains with the project's normal audit step).
- **U3 — Untracked planning documents** (`M-22Loop.MD`, `Module 22 Implementation Plan.MD`, `M-22.md`, `RLS_Learn_M-22.pdf`) are deliberately not committed per `Docs/Handoff_M22.md` §13 and were therefore not part of the analysed snapshot. Conclusions do not depend on them; the committed sources (`Docs/Source/*`) fully specify the wave structure used here.
- **U4 — The empty-referral-field placeholder rule** (FR-M14-006's second half) refers to "a default placeholder per patient sex"; the precise behavior should be pinned during M14 planning against `Docs/Reference system files/RLS_Learn.pdf` / `RL_Show.pdf` (reference-system captures), which are binary and were not textually analysed. Flagged as a design-time input, not a state uncertainty.

**Verification pass record:** before writing this report it was re-confirmed that (1) `HEAD` == `a8c1a2b61dea98405d40cc47571cc9988bd5d9c1` with a clean tree; (2) the target commit is the tip of `main`, so no post-target work exists to contaminate the analysis; (3) every "Done" claim relied upon was matched to code artifacts and tests; (4) every documentation/code disagreement found (C1–C4) is recorded in §5 with the code controlling the decision; (5) no feature was classified complete on the strength of documentation, a handoff self-report, or a prior report alone; (6) no application file was modified — the only repository change introduced by this investigation is this report file.

---

*End of report.*
