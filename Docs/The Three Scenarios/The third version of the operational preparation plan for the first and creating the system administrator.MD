# Top-Lab — B-02 Remediation & First-Run Setup / System Admin Creation — Implementation Plan

> PLANNING MODE. No files, database, Git state, or project configuration were modified during this analysis.
> All findings below were verified by direct inspection of the working copy at HEAD `28cb2765f757a12b08780f07965169a94fb13729` on branch `main`, working tree clean.
> Conventions used: **[FACT]** = verified from current code; **[REQ]** = requirement from this prompt; **[REC]** = engineering recommendation; **[DEC]** = owner decision required; **[ASM]** = assumption.

---

## 1. Executive Summary

[FACT] Top-Lab is a .NET 8 WPF MVVM Clean-Architecture/CQRS app (MediatR + EF Core + SQL Server). The current developer connection string uses **SQL Server LocalDB** (`(localdb)\mssqllocaldb`, `Trusted_Connection=True`) — see `src/TopLab.Presentation/appsettings.json:3` and `src/TopLab.Infrastructure/Persistence/DesignTimeDbContextFactory.cs:12`.

[FACT] **LocalDB cannot accept incoming network/TCP connections from other machines.** It is local-only (named pipes / shared memory). This is the genuine **B-02 contradiction**: the stated product model (one primary PC hosting the DB, multiple client PCs on the LAN) is impossible with the current LocalDB-based connection strategy. This must be resolved by moving to a network-capable SQL Server edition (Express/Standard) — it is NOT a documentation defect.

[FACT] The application-level authentication/authorisation scaffolding exists (Domain `User`/`Permission`/`UserPermissionGrant`, `CurrentUserService`, `AuthorizationBehavior`) but **no password hashing, no password verification, no login window, and no first-run flow exist**. `App.xaml.cs:30` launches `MainWindow` directly; `CurrentUserService.SetSession` is defined but never called (`CurrentUserService.cs:41`). `IsAbsolutePermission` already satisfies REQ #10 (a non-admin can be granted full rights).

[REC] Plan resolves B-02 by (a) switching the DB engine to SQL Server Express with TCP enabled and a fixed instance/database name, (b) adding an app-level password hasher + login (independent of Windows auth), (c) adding a first-run setup orchestrator (role selection, primary provisioning, client LAN discovery), (d) adding the Create System Admin window + CQRS command guarded by a DB-backed setup-state flag, and (e) deferring the actual SQL-Express installer bundling to a future packaging task while keeping app logic installer-agnostic.

---

## 2. Current Repository State

[FACT]
- Branch: `main` (verified via `git branch --show-current`).
- HEAD: `28cb2765f757a12b08780f07965169a94fb13729`.
- Working tree clean (`git status --porcelain` empty, 0 entries).
- No local modifications, no untracked files, no staged changes.
- HEAD matches the known commit in the prompt exactly.
- No divergence, no extra commits, no stashes. No Git state was changed.

[FACT] Top-level layout: `src/` (4 projects: `TopLab.Application`, `TopLab.Domain`, `TopLab.Infrastructure`, `TopLab.Presentation`), `tests/` (`TopLab.Application.Tests`, `TopLab.Domain.Tests`, `TopLab.Infrastructure.Tests`), `TopLab.sln`, `Directory.Build.props`, `Directory.Packages.props`, plus root `.md` reports and a `Source` folder reference that does **not** physically exist at root (only `Docs/Source` would be excluded; root `Source` is absent). The `Docs` folder and any `Docs/Source` were **not** opened, per the exclusion rule.

---

## 3. Current Implementation Inventory

| Area | Status | Evidence |
|------|--------|----------|
| Solution / projects | [FACT] 4 layers + 3 test projects, CPM via `Directory.Packages.props` | `TopLab.sln`, `Directory.Packages.props` |
| EF Core DbContext | [FACT] Single `ApplicationDbContext`, discovers configs via assembly | `ApplicationDbContext.cs:18,40` |
| DbSets (Users/Permissions/UserPermissionGrants/SystemSettings) | [FACT] Present | `ApplicationDbContext.DbSets.cs:41-46` |
| Migrations | [FACT] 2 migrations exist (Baseline + RenamePkColumns); **NOT auto-applied at runtime** | `Persistence/Migrations/*`; `DependencyInjection.cs` has no `MigrateAsync` |
| Connection string source | [FACT] `IConfiguration.GetConnectionString("TopLab")`; LocalDB; git-ignored | `DependencyInjection.cs:26`; `.gitignore:25` |
| User entity | [FACT] `UserName`(unique), `PasswordHash`, `InternalWindowsPasswordHash`(required), `IsAbsolutePermission` | `User.cs`, `UserConfiguration.cs:14-18` |
| Permission model | [FACT] `Permission` seeded via `HasData`; `UserPermissionGrant` link; `AuthorizationBehavior` enforces `IsAbsolutePermission` OR `HasPermission` | `PermissionConfiguration.cs:17`; `AuthorizationBehavior.cs:34-35` |
| Current user session | [FACT] `CurrentUserService` singleton with `SetSession`/`ClearSession`; **never invoked** | `CurrentUserService.cs:41`; no callers found |
| Connectivity check | [FACT] `CheckDatabaseConnectivityQuery` + `CanConnectAsync` exist and are reusable | `CheckDatabaseConnectivityQueryHandler.cs`; `ApplicationDbContext.cs:33` |
| Login window | [FACT] **Absent.** `App.xaml.cs` shows `MainWindow` directly | `App.xaml.cs:30` |
| First-run / setup UI | [FACT] **Absent.** No role selection, no provisioning, no discovery | no matches in `src` |
| Password hasher | [FACT] **Absent.** No `IPasswordHasher`, no PBKDF2/BCrypt, no verify logic | grep returned only entity property names |
| Installer / packaging | [FACT] **Absent.** No `.wix`, `.vdproj`, `.slnx`, bootstrapper, or publish profile | file search returned nothing |
| Windows auth (`InternalWindowsPasswordHash`) | [FACT] Column exists & is required; semantics belong to B-03/B-04 (out of scope) | `User.cs:12`; `UserConfiguration.cs:17` |

---

## 4. B-02 Current-State Assessment

[FACT] The only DB connection target in the repo is LocalDB (`appsettings.json:3`, `DesignTimeDbContextFactory.cs:12`). LocalDB is a developer/embedded engine that **binds to the launching user session and refuses remote TCP clients**.

[REQ] The product must run on multiple PCs in one lab LAN with one primary DB host.

**Contradiction (surfaced, not silently resolved):** LocalDB satisfies single-PC dev but **violates** the multi-PC distribution requirement. This is the real B-02 defect.

[REC] Replace LocalDB in the shipped product with a network-capable engine:
- **SQL Server Express** (free, supports TCP, up to 10 GB/DB, sufficient for a single-lab LIS) — **recommended**.
- SQL Server Standard/Enterprise — only if Express limits are exceeded (rare for one lab).
- A **fixed instance name** (e.g. `TOPLAB`) and **fixed database name** `TopLab` must be application-level constants so no user ever types them.

[REC] Keep LocalDB **only** as the developer fallback (it already works for single-dev `dotnet run`); the production connection string is supplied by the installer/local config, not Git.

[DEC] Engine edition + instance name + auth mode are owner decisions (see §22).

---

## 5. First-Run Requirements Gap Analysis

| REQ | Exists? | Gap / Action |
|-----|---------|--------------|
| 1. Role selection (primary vs client) | No | New first-run orchestrator + window |
| 2. Primary auto-provision DB + migrate + verify | No (engine=LocalDB, no migrate) | Switch engine (§4); add provisioning service |
| 3. Client LAN discovery, no typing | No | New discovery component (§9) |
| 4. Automatic connection verification, no Test button | Partial (`CheckDatabaseConnectivityQuery` exists) | Orchestrate it in setup flow |
| 5. Create System Admin window (exact fields) | No | New window + VM (§10) |
| 6. Admin works via normal login | No login exists | Implement login + verify (dependency/risk §11) |
| 7. Run-once (DB-backed) | No | New `SetupState` (§12) |
| 8. Clients skip admin window | N/A (flow) | Client path ends at login |
| 9. App auth independent of Windows | Partial (DB uses Windows auth; app has its own User) | Keep app login app-level; DB cred separate (§12/§13) |
| 10. Non-admin can get full perms | Yes (`IsAbsolutePermission`) | No change needed |
| 11. No hardcoded/fallback/ plaintext pw | Partial (no hashing at all) | Add hasher (§11) |
| 12/13. Credential separation + config storage | Partial | Define local config + DB cred model (§7/§13) |
| 14. First-run UI / auto-provision / discovery | No | Build minimal (§8/§9) |
| 15/16. Error handling + recovery | No | Define (§13) |
| 17. Security (medical lab) | None yet | Define (§14) |
| 18. Architecture fit | n/a | Place per §15 |
| 19. Installer | No | Document + defer (§16) |
| 20. Testing | Partial (xUnit exists) | Plan (§20) |

---

## 6. Database Engine and Provisioning Analysis

[FACT] Current engine = LocalDB (local-only). Not suitable for LAN clients (see §4).

[REC] Target engine = **SQL Server Express** (e.g. 2022 Express), configured for the lab:
- **TCP/IP enabled**, fixed port (1433 recommended) or named instance + SQL Server Browser.
- **Authentication**: Mixed Mode (SQL login) recommended for workgroup LAN reliability; Windows-auth-to-SQL only if the lab is on a domain (unreliable for non-technical workgroups).
- **Fixed identity**: instance `TOPLAB`, database `TopLab`, created by installer.
- **Dedicated app SQL login** `TopLabApp` (least-privilege `db_owner` on `TopLab` only), random password stored only in local config (see §13). This DB credential is **never** the System Admin password (REQ #12).

[FACT→REQ contradiction] REQ #2 says the engine "must be installed/configured automatically." [FACT] There is **no installer** (§16). Therefore:
- [REC] The **installer** (future task) installs SQL Express, creates instance/DB/login, opens firewall. The **application first-run** verifies the engine is present/running and provisions the **schema** (migrations) — it should not attempt a full SQL Express silent install at runtime (heavy, fragile, privilege-heavy).
- [REC] If, at primary first-run, the engine is missing, the app shows a plain-language error and stops (REQ #2: "must not silently continue").

Provisioning steps (app side, primary only):
1. Resolve connection string from local config (DB engine + `TopLabApp` login).
2. `Database.CanConnectAsync()` — if fail, clear message.
3. `Database.MigrateAsync()` to create/build schema.
4. **Schema verification**: run a read against a known table (e.g. `SELECT TOP 1 1 FROM Users`) or compare `context.Model`/`GetPendingMigrations()` returns empty; on mismatch → fail loudly.
5. Write `SetupState.SchemaProvisioned = true` (DB-backed).

---

## 7. Configuration Storage Analysis

[FACT] `.gitignore:25` excludes `appsettings*.json`; `.gitignore:29` excludes `connectionStrings.config`/`secrets.json`. This is intentional (ADR-0021: connection settings are workstation-local, never in DB, never committed). [REC] **Do not** propose committing secrets to Git.

[REQ #13] Distinguish A) dev repo config, B) local dev config, C) shipped customer config.
- A/B: current git-ignored `appsettings.json` (developer LocalDB) — fine as-is.
- C (shipped): installer writes a **local, non-Git** config file in the install/ProgramData directory containing the production connection string. The DB password is generated at install and stored **encrypted** (Windows DPAPI `ProtectedData` or `DataProtectionProvider` — BCL, no new package).

[REC] Add a small machine-level (non-secret) block to the local config to remember per-machine role: `"DeploymentRole": "Primary" | "Client"`. This is **not** the run-once guard (that is DB-backed); it merely prevents re-showing the role-selection UI every restart.

[REC] Connection string format (production): `Server=<PrimaryNetBIOS>\TOPLAB,1433;Database=TopLab;User Id=TopLabApp;Password=<encrypted-local>;TrustServerCertificate=True`. The server/instance/db are **fixed constants**; only the machine name portion is discovered for clients (or left as `localhost` for primary).

---

## 8. Primary/Server Setup Plan

[REC] Flow (orchestrated in `App.xaml.cs` composition root via a new `StartupOrchestrator`):
1. Read local config. If `DeploymentRole` already set → go to normal launch (§12).
2. Else show **First-Run Role window** (REQ #1): two buttons, no text entry.
3. User picks "Primary/server":
   a. Verify SQL Express present/running (§6). If missing → plain error + stop.
   b. Provision DB (migrate) + verify schema (§6).
   c. If success → show **Create System Admin window** (§10).
   d. On admin created → set `SetupState` complete, write `DeploymentRole=Primary`, show success + "launch via desktop shortcut".
4. All steps show progress text only; no raw technical strings to user.

[FACT] `CheckDatabaseConnectivityQuery` and `CanConnectAsync` are reusable for steps 3a/3b/4.
[REC] Put provisioning logic in **Infrastructure** (`IDatabaseProvisioner`) behind an **Application** port (`IApplicationDbContext`/new `ISetupRepository`), invoked by an Application handler/`StartupOrchestrator` — keep business rules out of WPF code-behind.

---

## 9. Client LAN Discovery and Connection Plan

[REQ #3] Clients must discover the primary with **no typing** and confirm via a simple "Connect to: [Name]" action.

[REC] **Primary mechanism: custom UDP broadcast discovery beacon** (recommended).
- Primary machine runs a lightweight broadcaster (a `BackgroundService` in the app, or a tiny separate Windows service — owner decision) that periodically sends a UDP packet on a **fixed port** (e.g. `49152`) to the subnet broadcast address containing: `ComputerName`, `LabId` (random GUID generated at primary setup and stored in `SetupState`), and instance hint.
- Client first-run listens on that port for a few seconds, lists discovered primaries by `ComputerName`, user clicks one, app validates `LabId` matches the one stored locally (or, on very first client setup, imports it) to avoid wrong-lab connections (§14).
- On selection → build connection string (`Server=<ComputerName>\TOPLAB,1433;...`), auto-connect verify, write local config (`DeploymentRole=Client`), proceed to login.

Alternatives evaluated:
| Option | Pros | Cons | Suitability |
|--------|------|------|-------------|
| UDP broadcast beacon | Simple, no SQL dependency, controllable, supports LabId token | Needs app/background component running; firewall UDP 49152 | **Recommended** |
| SQL Server Browser (UDP 1434) | Built-in, discovers named instance | Requires Browser service + firewall; exposes instance; less controllable; user still must know instance | Secondary |
| mDNS (Bonjour) | Standard, name-based | Extra dependency; firewall; overkill | Weak |
| Windows service discovery (WS-Discovery) | Standard | Heavy, firewall, complexity | Weak |

[REC] No manual IP/host/instance/db/connection-string entry anywhere. Discovery is automatic; user only confirms.

---

## 10. Create System Admin Plan

[REQ #5] Window contains exactly: red warning banner, Username, Password (masked + eye toggle), Confirm Password (masked + own eye toggle), "Create Admin User" button.

[REC] Files (Presentation layer):
- `Views/Setup/CreateSystemAdminWindow.xaml` + `.xaml.cs` (code-behind only wires VM; no business rules).
- `ViewModels/Setup/CreateSystemAdminViewModel.cs` (MVVM, `ViewModelBase`/`RelayCommand` pattern already used in `ShellViewModel`).

[REC] Behaviour (delegated to Application command `CreateSystemAdminCommand`):
- Validate: username non-empty & unique (unique index `UserConfiguration.cs:15`); password not empty; confirm matches; optional password policy (DEC #9).
- Guard idempotency: if `SetupState.IsSystemAdminCreated` → refuse (REQ #7, §12).
- Hash password via `IPasswordHasher` (§11); set `IsAbsolutePermission = true`.
- `InternalWindowsPasswordHash`: set to empty string `""` for this app-only admin (column is `IsRequired` but allows empty string) — **dependency/risk on B-03/B-04** (see §11/§22 DEC #4).
- Persist `User`; set `SetupState.IsSystemAdminCreated = true`, `CompletedUtc`, `PrimaryComputerName`.

[REQ #6] On success: clear success message; instruct user to close setup and launch via desktop shortcut; the same credentials then authenticate through the normal **login screen** (which must exist — §11 dependency).

---

## 11. Authentication and Permission Integration

[FACT] App-level identity model already present: `User.PasswordHash`, `CurrentUserService.SetSession(userId, isAbsolutePermission, grantedPermissions)`, `AuthorizationBehavior` checks `IsAbsolutePermission || HasPermission(code)`. `User.Create` requires `passwordHash` + `internalWindowsPasswordHash`.

[FACT] **No hasher, no verifier, no login window.** `SetSession` has zero callers.

[REC] Add (no second auth system — reuse existing):
- **Application** port `IPasswordHasher` (`Hash(password)→string`, `Verify(hash,password)→bool`).
- **Infrastructure** implementation using **PBKDF2 via `System.Security.Cryptography.Rfc2898DeriveBytes`** (BCL, **no new NuGet package** — `Directory.Packages.props` has no identity package). Store format `pbkdf2:iters:salt:hash` in `PasswordHash` (max 300, `UserConfiguration.cs:16` — sufficient).
- **Login window** + `AuthenticateUserQuery`/handler: load `User` by `UserName`, `Verify`, on success call `CurrentUserService.SetSession(...)` with `IsAbsolutePermission` and granted permission codes (read `UserPermissionGrants`). This fulfils REQ #6 and REQ #9 (app auth independent of Windows).

[REQ #10] Supported as-is: admin created with `IsAbsolutePermission=true` bypasses per-item checks (`AuthorizationBehavior.cs:34`); a later admin can grant a regular user all permission codes or flip `IsAbsolutePermission`. No weakening needed.

**Dependency / Risk on B-04:** REQ #6 requires a working normal login. The code shows none. Either (a) login was meant to be delivered by B-04 and is simply not yet in this tree (unlikely since HEAD is post-B-04), or (b) login must be built here. **Flag as DEC #5** — owner must confirm whether login is in-scope here or covered elsewhere; plan assumes it must be implemented because REQ #6 is non-negotiable.

[REQ #11] No hardcoded/default/fallback password; empty `InternalWindowsPasswordHash` for app admin is not a password and never authenticates. Plaintext never stored (hash only).

---

## 12. Setup State and Recovery Design

[REQ #7] Run-once must be **DB-backed**, not a local flag/file alone.

[REC] New **Domain** entity `SetupState` (single row, PK=1):
- `IsSystemAdminCreated bit` (authoritative guard for admin window),
- `SchemaProvisioned bit`,
- `PrimaryComputerName nvarchar`,
- `LabId uniqueidentifier` (discovery token, §9),
- `CompletedUtc datetime2 null`.

Because all clients share the **same** database, this flag is visible to every machine → guarantees no admin window on clients and no duplicate admin (REQ #7/#8). A unique index on `UserName` (`UserConfiguration.cs:15`) provides a second guard against duplicate admin creation.

[REC] Per-machine role (`DeploymentRole` in local config, §7) prevents re-showing role selection UI every restart, but is **not** the run-once authority.

[REQ #16] Idempotency/recovery:
- If DB exists but admin missing → admin window still shows (guard only checks `IsSystemAdminCreated`).
- If admin created but `SetupState` not written → transaction wraps both; on failure neither persists (use EF transaction in the command handler).
- Partial migration → `MigrateAsync` is resumable; re-running completes it.
- App restart mid-setup → role not yet saved locally, so role window reshows; safe because provisioning is idempotent and admin creation is guarded.

---

## 13. Error Handling Design

| Scenario | User-facing (non-technical, AR) | Internal handling / log |
|----------|--------------------------------|--------------------------|
| SQL engine missing | "تعذر العثور على محرك قاعدة البيانات. الرجاء تشغيل المثبّت أو التواصل مع الدعم." | Log engine-detection result; no secrets |
| Engine start failure | "تعذر تشغيل قاعدة البيانات." | Log service state |
| DB create failure | "فشل إنشاء قاعدة البيانات." | Log EF exception (no conn password) |
| Migration failure | "فشل تجهيز هيكل قاعدة البيانات." | Log `MigrateAsync` exception |
| Schema verify failure | "بنية قاعدة البيانات غير مكتملة." | Log missing table/version |
| LAN discovery timeout | "لم يتم العثور على جهاز رئيسي. تأكد تشغيل الجهاز الرئيسي." | Log timeout/port |
| No primary found | same as above | Log |
| Multiple primaries | "وُجد أكثر من جهاز. اختر الصحيح." | Log count + names |
| Primary found, unreachable | "الجهاز الرئيسي غير متاح حالياً." | Log connect failure |
| Auth/connection failure | "تعذر الاتصال. تحقق من الشبكة." | Log (no creds) |
| Invalid username | "اسم المستخدم غير صالح/موجود." | Log attempt (no pw) |
| Weak password | "كلمة المرور ضعيفة." (if policy) | Log policy code |
| Mismatch | "كلمتا المرور غير متطابقتين." | none |
| Duplicate admin attempt | "تم إنشاء مدير النظام مسبقاً." | Log guarded attempt |
| Partial/interrupted setup | Resumes automatically on relaunch | Idempotent (§12) |
| App restart during setup | Re-shows appropriate step | Idempotent |

[REQ] All logs **must never** contain passwords, hashes, or connection-string secrets (strip before logging). Use `Error`/`IAppLogger` already present (`IAppLogger.cs`, `LoggingBehavior.cs`).

---

## 14. Security Design

- **DB credential separation (REQ #12):** `TopLabApp` SQL login password lives only in local encrypted config; never shown to users; never equals any app password.
- **App password hashing (REQ #11):** PBKDF2, per-user salt, in `PasswordHash`. `InternalWindowsPasswordHash` unused for app admin.
- **LAN discovery spoofing (REQ #17):** use `LabId` token so a client only connects to its own lab's primary; reject mismatched tokens. Optional passphrase = DEC #7.
- **Unauthorised clients:** SQL Express firewall scoped to LAN; app login still required after discovery. Discovery does not grant data access.
- **Firewall exposure:** only TCP 1433 (SQL) + UDP 49152 (beacon) opened; SQL Browser optional. Least-privilege `TopLabApp` login.
- **Accidental disclosure:** no secrets in UI, logs, or Git. Connection string excluded by `.gitignore`.
- **No regulatory claims:** nothing here asserts HIPAA/GDPR compliance; that needs separate review.

---

## 15. Architecture and Layer Placement

[FACT] Layers: Domain (entities/VO), Application (CQRS/ports/behaviors), Infrastructure (EF/Identity/impl), Presentation (WPF/MVVM). Composition root is `App.xaml.cs` (may reference Infrastructure per ADR-0024).

| Component | Layer | Why |
|-----------|-------|-----|
| `SetupState`, `User` admin factory use | Domain | business rules/invariants |
| `CreateSystemAdminCommand`, `AuthenticateUserQuery`, `ProvisionDatabaseCommand`, setup ports | Application | use-cases, CQRS, validation |
| `IPasswordHasher` impl (PBKDF2), `IDatabaseProvisioner` (MigrateAsync), `ILanDiscovery` (UDP), `SetupState` config, `TopLabApp` conn builder | Infrastructure | external concerns/EF/network |
| `StartupOrchestrator` (called from `App.xaml.cs`), Role window, CreateSystemAdmin window, Login window, discovery UI | Presentation | UI/UX only |
| Local config read/write (role, encrypted conn) | Infrastructure (file) or Presentation bootstrap | config access |

[REC] No second architecture; reuse MediatR pipelines, `Result<T>`, `ValidationBehavior`, `AuthorizationBehavior`. Business rules stay in Domain/Application; UI stays dumb.

---

## 16. Installer / Deployment Analysis

[FACT] **No installer, bootstrapper, publish profile, or packaging project exists** in the repo (file search returned nothing).

[REC] Distinguish clearly:
- **Application work (this task):** role selection, provisioning, discovery, admin creation, login, config write — all implementable now, installer-agnostic.
- **Installer work (future, out-of-scope to implement):** bundle SQL Server Express, create instance/DB/`TopLabApp` login, open firewall (TCP 1433, UDP 49152), deploy app + write local config (encrypted). Choose tool (DEC #10): WiX Burn, Advanced Installer, or Visual Studio Installer Projects.

[REC] App must **not** assume an installer is present at dev time (LocalDB fallback remains). At customer time, the installer supplies the production connection string; the app reads it.

---

## 17. Dependency-Ordered Implementation Plan

Dependencies (A→B means A before B):
1. **Password hasher** (`IPasswordHasher` + PBKDF2 impl) → needed by admin creation & login.
2. **SetupState entity + migration** → needed by run-once guard & provisioning flag.
3. **DB provisioner** (migrate + verify) behind Application port → needed by primary setup.
4. **Local config model** (role + encrypted conn string) → needed by all flows.
5. **StartupOrchestrator** in composition root → coordinates everything.
6. **Role selection window** → first UI step.
7. **Primary setup flow** (uses #2,#3,#4) → then admin window.
8. **CreateSystemAdminCommand + window** (uses #1,#2) → run-once admin.
9. **LAN discovery** (`ILanDiscovery` UDP) → client flow.
10. **Client setup flow** (uses #4,#9, connectivity query) → then login.
11. **Login window + AuthenticateUserQuery** (uses #1, `SetSession`) → normal launch (REQ #6).
12. **Error handling + logging** woven through #3,#7,#8,#9,#10.
13. **Tests** (unit/integration) parallel to each.
14. **Installer/packaging** (future, separate task) wraps engine.

---

## 18. File-Level Implementation Map

**New files (Application):** `Features/Setup/CreateSystemAdminCommand.cs` (+Handler, Validator); `Features/Setup/ProvisionDatabaseCommand.cs` (+Handler); `Features/AccessAndNavigation/Queries/AuthenticateUserQuery.cs` (+Handler); `Common/Interfaces/IPasswordHasher.cs`; `Common/Interfaces/ISetupRepository.cs` (or reuse `IApplicationDbContext`); `Common/Interfaces/ILanDiscovery.cs`.

**New files (Domain):** `Settings/SetupState.cs`; `Users/` factory adjustments if needed (none required beyond existing `Create`).

**New files (Infrastructure):** `Identity/PasswordHasher.cs`; `Persistence/Configurations/SetupStateConfiguration.cs`; `Persistence/Provisioning/DatabaseProvisioner.cs`; `Network/LanDiscoveryService.cs` (UDP); `Persistence/SetupState` DbSet in `ApplicationDbContext.DbSets.cs`.

**New files (Presentation):** `AppBootstrapper/StartupOrchestrator.cs` (or in `App.xaml.cs`); `Views/Setup/RoleSelectionWindow.xaml[.cs]`; `Views/Setup/CreateSystemAdminWindow.xaml[.cs]`; `Views/Auth/LoginWindow.xaml[.cs]`; `ViewModels/Setup/*`; `ViewModels/Auth/LoginViewModel.cs`; `Common/Config/LocalSettingsProvider.cs` (reads/writes role + encrypted conn).

**Modified files:** `App.xaml.cs` (route through `StartupOrchestrator` instead of directly `MainWindow`); `ApplicationDbContext.cs`/`DbSets.cs` (add `SetupState`); `UserConfiguration.cs` (optional: make `InternalWindowsPasswordHash` nullable if DEC #4 chooses); `DependencyInjection.cs` (register new services); `DesignTimeDbContextFactory.cs` (keep LocalDB for dev, or parameterise); `appsettings.json` (dev remains LocalDB; production supplied by installer).

**Migrations:** one new migration adding `SetupState` table (and optional `InternalWindowsPasswordHash` nullability). Generated, not hand-written.

**Tests:** `TopLab.Application.Tests` (commands, hasher, setup-state logic), `TopLab.Infrastructure.Tests` (PBKDF2 verify, provisioner against SQL Express or InMemory), `TopLab.Domain.Tests` (SetupState invariants).

---

## 19. Database / Migration Impact

[REQ] Schema changes required:
- **New table** `SetupState` (PK=1, `IsSystemAdminCreated`, `SchemaProvisioned`, `PrimaryComputerName`, `LabId`, `CompletedUtc`). Single-row.
- **Optional:** make `InternalWindowsPasswordHash` nullable (`UserConfiguration.cs:17`) if DEC #4 prefers not storing empty string (belongs to B-03/B-04 reconciliation — flag, don't redesign).
- **Seed data:** none required for admin (created at runtime). `Permission` already seeded.
- **New entities/columns/indexes:** none beyond above; `UserName` unique index already exists (`UserConfiguration.cs:15`) — reuse for duplicate prevention.
- **Relationships:** none new.
- **Dependency order:** `SetupState` migration generated after entity+config added; provisioning depends on it; admin creation depends on it.

No migrations will be generated during this planning task.

---

## 20. Testing Plan

**Unit (xUnit, existing framework):**
- `PasswordHasher`: hash→verify true; wrong pw false; unique salt; format stable.
- `CreateSystemAdminCommand`: rejects empty user/mismatch; refuses when `IsSystemAdminCreated`; sets `IsAbsolutePermission`; stores hash not plaintext.
- `SetupState`: guards + idempotency logic.
- `LanDiscoveryService`: packet parse/serialize; LabId match/reject.

**Integration:**
- Provisioning: against SQL Express (or InMemory for speed) — `MigrateAsync` creates schema; verify query passes; failure path throws.
- Admin creation end-to-end via `IApplicationDbContext` (InMemory or SQL Express): row persisted, login verifies.
- Login: `AuthenticateUserQuery` returns session; `SetSession` populates `CurrentUserService`.

**End-to-end / manual:**
- Fresh primary install (engine pre-installed): role→provision→admin window→create→relaunch→login works.
- Fresh client: role→discover→confirm→login (no admin window).
- Multi-machine LAN: 2 clients discover 1 primary; wrong-lab LabId rejected.
- Interrupted setup: kill mid-flow; relaunch resumes; no duplicate admin.
- Restart after setup: admin window never reappears; clients never show it.
- Network failure / DB unavailable: clear message, no crash.
- Wrong creds / mismatch: messages; no secrets in logs.
- Regression: existing `CheckDatabaseConnectivity`, `AuthorizationBehavior`, `ShellViewModel` unaffected.

---

## 21. Acceptance Criteria

- [ ] Primary setup installs/verifies network SQL engine and provisions schema via `MigrateAsync`; verification passes before continuing; failure stops with plain message.
- [ ] Client discovers primary via LAN with zero typed technical values; user confirms by name.
- [ ] Connection verified automatically (no Test button) before proceeding.
- [ ] Create System Admin window has exactly the 5 required elements incl. red warning + two independent eye toggles.
- [ ] Admin password hashed (PBKDF2), no default/fallback/plaintext; logs never contain secrets.
- [ ] Created admin logs in via normal login; has full rights (`IsAbsolutePermission`).
- [ ] Admin window appears **once**, only on primary, never on clients, never after restart/update.
- [ ] Additional privileged accounts can be created later (REQ #10 preserved).
- [ ] DB credential separate from app credential; not exposed to users.
- [ ] Interrupted setup recovers idempotently; no duplicate admin.
- [ ] LAN discovery resistant to wrong-lab spoofing (LabId).
- [ ] Existing auth/connectivity behaviour regression-free.
- [ ] No new architecture; fits Clean/CQRS; no files committed to Git unexpectedly.

---

## 22. Owner Decisions Required

1. **DB engine edition + instance name** — Express (recommended) vs Standard; instance `TOPLAB` (fixed). Impact: distribution, licensing, LAN connectivity.
2. **SQL auth mode** — Mixed (SQL `TopLabApp` login, recommended for workgroups) vs Windows-auth-to-SQL (domain only). Impact: client connectivity reliability, credential storage.
3. **LAN discovery mechanism** — UDP beacon (recommended) vs SQL Browser vs mDNS. Impact: firewall, reliability, complexity.
4. **`InternalWindowsPasswordHash` for app admin** — store empty string (minimal, recommended) vs make column nullable (touches B-03/B-04 schema). Impact: schema, B-03/B-04 reconciliation.
5. **Is the normal Login window in-scope here or covered by B-04?** — REQ #6 needs it; code lacks it. Must confirm to avoid overlap/duplication.
6. **SetupState location** — dedicated `SetupState` table (recommended) vs new column on `SystemSettings`. Impact: SRP, migration.
7. **Discovery security token** — use auto `LabId` only (recommended) vs add optional user passphrase. Impact: UX, spoofing resistance.
8. **Password policy** — none (recommended minimal) vs min length/complexity. Impact: UX, REQ #11 weak-pw handling.
9. **Local config encryption** — DPAPI `ProtectedData` (recommended, BCL) vs plaintext file. Impact: secret exposure.
10. **Installer framework (future)** — WiX / Advanced Installer / VS Installer. Impact: how SQL Express is bundled (out-of-scope to implement now).

---

## 23. Assumptions and Verification Items

- [ASM] Lab is a single workgroup LAN, not domain-joined. → Verify with owner; if domain, DEC #2 changes.
- [ASM] One primary, multiple clients (<= ~10). → Verify scale; affects Express 10 GB limit (DEC #1).
- [ASM] SQL Express can be installed by an installer before first-run app launch. → Verify packaging plan (§16).
- [ASM] `InternalWindowsPasswordHash` is purely a B-03/B-04 concern and empty string is acceptable for app admin. → Verify with B-03/B-04 owner; else DEC #4.
- [ASM] `Permission` seed data is sufficient for "full admin" via `IsAbsolutePermission` (no need to grant every code). → Verified by `AuthorizationBehavior.cs:34`; OK.
- [ASM] Firewall can be opened by installer on customer machines. → Verify with owner/IT.
- [ASM] B-04 did not already deliver login (code shows none). → Confirm DEC #5.
If any assumption is false, the corresponding DEC/section changes (noted inline).

---

## 24. Out-of-Scope Items

- No redesign of B-01, B-03, B-04 (touched only as dependencies/risks, e.g. DEC #4/#5).
- No unrelated refactoring, features, or documentation changes.
- No installer implementation (only analysis + future task description, §16).
- No regulatory/compliance certification.
- No schema changes to `User`/`Permission` beyond what §19 states.
- No new authentication system beyond reusing the existing app-level model.

---

## 25. Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| LocalDB cannot serve clients (B-02) | Core failure | Switch to SQL Express + TCP (§4/§6) |
| No installer exists | Engine not present at customer | Separate packaging task; app verifies+errors (§16) |
| Login absent (possible B-04 overlap) | REQ #6 unmet | Build minimal login (DEC #5) |
| `InternalWindowsPasswordHash` required | Schema friction | Empty string or nullable (DEC #4) |
| LAN discovery spoofing | Wrong-lab connect | LabId token (§14) |
| SQL network/firewall blocked | Clients can't connect | Installer opens ports; clear errors (§13) |
| Secrets leaked in logs | Compliance | Strip passwords/conn in logging (§13/§14) |
| Duplicate admin on race | Security | DB guard + transaction (§12) |
| Interrupted setup | Inconsistent state | Idempotent provisioning + guarded admin (§12) |

---

## 26. Final Recommended Implementation Sequence

1. Add `IPasswordHasher` (Application) + PBKDF2 impl (Infrastructure). *Tests: hasher.*
2. Add `SetupState` entity + config + DbSet + migration. *Tests: invariants.*
3. Add `IDatabaseProvisioner` (migrate+verify) + Application `ProvisionDatabaseCommand`. *Tests: integration.*
4. Add local config provider (role + encrypted conn). *Tests: read/write.*
5. Add `StartupOrchestrator` in composition root; route `App.xaml.cs` through it.
6. Add Role Selection window (primary/client).
7. Wire Primary flow: detect engine → provision → verify → (if ok) show Create System Admin.
8. Add `CreateSystemAdminCommand` + window (red warning, username, 2 masked pw + toggles, button). *Tests: command.*
9. Add `ILanDiscovery` UDP service + Client flow (discover → confirm → connect → login). *Tests: parse.*
10. Add Login window + `AuthenticateUserQuery` (verify + `SetSession`). *Tests: integration.* (confirm DEC #5)
11. Weave error handling + secret-safe logging throughout.
12. Regression tests for existing auth/connectivity.
13. (Future, separate) Installer bundles SQL Express, creates DB/login, opens firewall, writes config.

All steps keep Clean Architecture, reuse existing MediatR/`Result`/behaviors, introduce no second framework, and keep DB credential strictly separate from the System Admin password.
