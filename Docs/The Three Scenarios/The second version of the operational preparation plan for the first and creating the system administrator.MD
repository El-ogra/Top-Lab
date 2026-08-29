# Top-Lab — Full Implementation Plan: First-Run Setup, Database Provisioning & System Admin Creation (B-02 Remediation)

> **Mode:** PLANNING ONLY — no file/database/Git changes. All statements marked as **Fact (verified from code)**, **Requirement (from this prompt)**, **Recommendation**, **Assumption**, or **Decision-required**.

---

## 1. Executive Summary

**Fact:** Top-Lab is .NET 8 / WPF / MVVM / Clean Architecture / CQRS-MediatR / EF Core 8 / SQL Server. `src` has 4 projects (`Domain`, `Application`, `Infrastructure`, `Presentation`). `Presentation` is `WinExe` `net8.0-windows` / `UseWPF=true`. `Infrastructure` registers `ApplicationDbContext` via `UseSqlServer` with connection string `ConnectionStrings:TopLab` read from `IConfiguration` (fail-fast if missing, `src/TopLab.Infrastructure/DependencyInjection.cs:26`). Current `src/TopLab.Presentation/appsettings.json:3` is `Server=(localdb)\mssqllocaldb;Database=TopLab;Trusted_Connection=True;...` — **LocalDB**. Design-time factory (`src/TopLab.Infrastructure/Persistence/DesignTimeDbContextFactory.cs:12`) hardcodes same LocalDB.

**Fact:** No installer / packaging project exists (glob for `installer|setup|publish|wix|msix|inno` returned zero source files; `TopLab.sln:1` lists only the 4 src + 3 test projects). No first-run UI exists (`src` xaml = `App.xaml`, `MainWindow.xaml` only). No LAN discovery, no provisioning, no `Create System Admin` window, no `IPasswordHasher`, no login flow. Shell is just `ShellViewModel` + `HomeViewModel` + `MainWindow`.

**Fact:** Auth/permission model exists and is usable: `TopLab.Domain/Users/User.cs:6` (with `PasswordHash`, `InternalWindowsPasswordHash`, `IsAbsolutePermission`, `IsActive`), `Permission.cs:6` (13 seeded permissions via `PermissionConfiguration.cs:17`), `UserPermissionGrant.cs:6` (composite PK), `ICurrentUserService.cs:7`, `CurrentUserService.cs:18` (singleton session, `SetSession`/`ClearSession`), `AuthorizationBehavior.cs:18` (bypasses check when `IsAbsolutePermission` is true). This already satisfies Requirement §10 (redundant full-admin via `IsAbsolutePermission`).

**Contradiction (must be owned by owner):** Requirement §2 says "install/configure local SQL Server automatically" and also historically suggests "LocalDB or equivalent". **LocalDB cannot accept remote TCP connections and therefore cannot serve clients over LAN.** The present `appsettings.json`/`DesignTimeDbContextFactory` LocalDB choice is incompatible with the multi-PC lab model described in Business Context. This plan makes an explicit recommendation (SQL Server Express, named instance `TOPLAB`, fixed DB name `TopLab`) and requires owner approval before implementation.

**Plan in one line:** (1) Fix database engine decision + distribution-owned configuration storage, (2) add first-run infrastructure (setup-state, provisioning, verification, DPAPI-protected connection storage), (3) add LAN discovery/connection, (4) add Create System Admin (Application + Presentation), (5) add startup router, (6) add installer/packaging, (7) test. The dependency-ordered sequence is in §17/§26.

---

## 2. Current Repository State

| Item | Value | Source |
|------|-------|--------|
| Current branch | `main` | `git branch --show-current` |
| Current HEAD | `28cb2765f757a12b08780f07965169a94fb13729` | `git rev-parse HEAD` |
| Expected commit (prompt) | `28cb2765f757a12b08780f07965169a94fb13729` (`بعد إصلاح B-04`) | prompt |
| HEAD matches expected | **Yes — exact match** | comparison |
| Working tree | **Clean** | `git status --porcelain` = empty, `git diff --stat` = empty |
| Staged changes | None | `git diff --cached --stat` = empty |
| Untracked files | None | `git status -u` clean |
| Remote | `origin https://github.com/El-ogra/Top-Lab.git` | `git remote -v` |
| `.gitignore` | Ignores `appsettings*.json`, `*.mdf/*.ldf/*.bak`, `bin/obj`, `.vs` | `.gitignore:1` |

**Fact:** The local working copy is exactly the expected post-B-04 state. No recovery or reset needed. Analysis below was performed against this state. Documentation exclusion rule was respected — no file under `Docs/` was opened.

---

## 3. Current Implementation Inventory

### 3.1 Solution & Build
- **Fact:** `TopLab.sln:1` — 7 projects. TFMs: `net8.0` for Domain/Application/Infrastructure/tests, `net8.0-windows` for Presentation. `ManagePackageVersionsCentrally=true` (`Directory.Build.props:1`). `Directory.Packages.props:1` pins `MediatR 12.5.0`, `FluentValidation 12.1.1`, `EFCore 8.0.30`, `Microsoft.Extensions.Hosting 8.0.1 + Configuration.Json 8.0.1`.
- **Fact:** `TopLab.Presentation.csproj:24` adds `Content: appsettings.json CopyToOutputDirectory=PreserveNewest`.

### 3.2 Architecture
- **Fact:** Clean Architecture enforced: `App.xaml.cs:23-25` is the sole composition root (`AddApplication` → `AddInfrastructure(builder.Configuration)` → `AddPresentation`). Comment at `App.xaml.cs:10` forbids Presentation referencing Infrastructure except here. `TopLab.Infrastructure.csproj` references `Application` + `Domain`; `Application` references `Domain` only.
- **Fact:** CQRS/MediatR pipeline: `TopLab.Application/DependencyInjection.cs:22-24` registers `Validation → Authorization → Logging` (`ValidationBehavior`, `AuthorizationBehavior`, `LoggingBehavior`). `IApplicationDbContext.cs:10` is the Application port (no EF Core leakage; only `IQueryable`, `Add/Update/Remove`, `SaveChangesAsync`, `CanConnectAsync`).
- **Fact:** EF Core model: `ApplicationDbContext.cs:18` (`ApplyConfigurationsFromAssembly`), `ApplicationDbContext.DbSets.cs:15` (52 DbSets). `SystemSettings` (single row PK=1, seeded), `ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`, `Permissions` (13 seeded), etc. Snapshot = `ApplicationDbContextModelSnapshot.cs:1`.

### 3.3 Presentation
- **Fact:** `MainWindow.xaml:1` — simple shell: nav bar + `ContentControl(CurrentViewModel)` (only `HomeViewModel` template) + `StatusBar` (DB dot via `ShellViewModel.IsDatabaseConnected`, clock via `DispatcherTimer`). `ShellViewModel.cs:68` calls `CheckDatabaseConnectivityQuery` on startup. `DependencyInjection.cs:15` registers singletons: `INavigationService`, `IDialogService`, `ResultErrorPresenter`, `IAppLogger(WpfAppLogger)`; `ShellViewModel`/`HomeViewModel` transient; `MainWindow` singleton. No login, no setup windows.

### 3.4 Domain / Auth
- **Fact:** `User.cs:6` — `UserName` (unique index), `PasswordHash` (300), `InternalWindowsPasswordHash` (300), `IsAbsolutePermission` bool, `DiscountLimitPercent`, `BlockPrintOnRemainingBalance`, `WorkStartTime/EndTime`, `HasBreakPeriod`, `BreakDurationMinutes`, `LastLoginAtUtc`, `IsActive`, plus `PermissionGrants`. Factory `User.Create` validates `UserName` only. No password hasher in Domain or Application. `InternalWindowsPasswordHash` appears legacy/unused — treat as out-of-scope (§24).
- **Fact:** `Permission.cs:6` + `PermissionConfiguration.cs:17` — 13 codes (`ADD_EDIT_PATIENT`, `EDIT_RESULTS`, `REVIEW_RESULTS`, `PRINT_RESULTS`, `BLOCK_PRINT_ON_BALANCE`, `DELIVER_RESULTS`, `DISCOUNT_LIMIT`, `PRINT_WORKSHEET`, `DELETE_PATIENT`, `EDIT_SYSTEM_SETTINGS`, `CASH_DISBURSE_DEPOSIT`, `STATISTICS`, `PT_AUDIT_ACCESS`).
- **Fact:** `UserPermissionGrantConfiguration.cs:11` — composite PK `(UserId, PermissionId)`. No FK nav configured explicitly.
- **Fact:** `CurrentUserService.cs:41` — `SetSession(userId, isAbsolute, grantedCodes)` + `ClearSession()`, `HasPermission(code)` ordinal. Registered as **Singleton** (`Infrastructure/DependencyInjection.cs:49`) — correct for WPF single-session.

### 3.5 Tests
- **Fact:** 3 test projects present (`TopLab.Domain.Tests`, `TopLab.Application.Tests`, `TopLab.Infrastructure.Tests`) covering behaviors, `CheckDatabaseConnectivityQueryHandler`, `ApplicationDbContext`/`F5 configurations`/interceptors. No test covers login/hashing/setup.

### 3.6 What does NOT exist (Fact)
- No installer/packaging project; no `publish` profile.
- No `IPasswordHasher` / `PBKDF2` / `BCrypt` — `grep PasswordHash` only hits `User.cs`/`UserConfiguration`.
- No login `Command`/`Query`/`Handler` or login Window/VM.
- No `SetupState` table/flag; no first-run router.
- No UDP/mDNS/discovery code; no `Create System Admin` window/VM/Command.
- No connection-string protection (no DPAPI, no `DataProtection`).
- No `appsettings.*.Production.json` committed (intentionally `.gitignore`-excluded).

---

## 4. B-02 Current-State Assessment

**B-02 (as inferred from requirements + code):** "Database configuration is missing/brittle; first-run provisioning does not exist; LocalDB cannot serve clients."

| B-02 facet | Verdict | Evidence |
|------------|---------|----------|
| Connection config source of truth | **Partial — dev-only** | `appsettings.json:3` exists locally, works for single-dev LocalDB, but is `.gitignore`-excluded, has no customer mechanism, and is LocalDB not LAN-capable. |
| `Infrastructure` error handling | **Partial** | `DependencyInjection.cs:26` fail-fast with `InvalidOperationException` if `ConnectionStrings:TopLab` missing — good for dev, but user-facing message is not localized / not recoverable via UI; startup crashes. |
| Database provisioning | **Missing** | No `MigrateAsync`/`EnsureCreated` call at startup. Migrations exist on disk (`2026082805...`, `2026082812...`) but are never applied automatically. No schema verification. |
| `DesignTimeDbContextFactory` | **Exists but hardcoded LocalDB** | `DesignTimeDbContextFactory.cs:12` duplicates LocalDB string — must be updated when engine decision is made. |
| Multi-PC suitability | **Broken by design** | LocalDB = per-user, no TCP listener, no firewall rule, no discovery. Business Context requires LAN clients — impossible with LocalDB. |
| Overlap with B-01/B-03/B-04 | **None required** | B-01/B-03/B-04 are not to be redesigned. No dependency found that forces their modification; B-02 remediation can be additive. If a legacy `appsettings*.json` encryption helper was added under B-04, it is not visible in inspected code — verify before reusing. |

**Risk if left as-is:** Fresh customer install on primary PC silently fails (or forces manual JSON edits), clients can never connect, and there is no admin-seed path. This is the genuine B-02 problem this plan solves.

---

## 5. First-Run Requirements Gap Analysis

| # | Requirement (prompt) | Already exists | Partially exists | Missing | Files likely affected | Dependencies | Security | Tests |
|---|----------------------|----------------|------------------|---------|-----------------------|--------------|----------|-------|
| 1 | Role selection: Primary vs Client | — | — | **Missing** — no installer choice, no first-run window | `Presentation`: new `FirstRunRoleSelectionWindow`+VM; `App.xaml.cs`; optional installer project | §6 engine decision | None directly | Unit (VM logic) + manual E2E |
| 2 | Primary: auto install/configure SQL engine, fixed DB/instance constants | — | `appsettings.json` holds DB name `TopLab` as string constant | **Missing engine provisioning; missing constants** | New `Infrastructure/SqlEngine/*` or `Presentation/Services/SqlEngineProvisioner`? See §8/§15 | Engine choice (§6), config storage (§7) | Least privilege for DB creation; SA vs Windows Auth | Integration (provisioner), manual install |
| 2b | `MigrateAsync` + schema verification, fail-loud | Migrations exist | `CheckDatabaseConnectivityQuery` exists but not provisioning | **Missing** | `Infrastructure` migrator/validator; `App.xaml.cs` startup | Engine online, connection string resolved | Log without secrets | Integration (migration), unit (validator) |
| 3 | Client: auto LAN discovery, no manual IP/host entry, "Connect to: X" | — | — | **Missing entirely** | New `Infrastructure/Discovery/*` or `Application/Discovery/*`; new Window/VM | Network; primary beacon reachable; FW | Spoofing risk (§14) | Unit + LAN integration + manual |
| 4 | Automatic verification (no Test button), progress/final outcome, retries | `CheckDatabaseConnectivityQueryHandler` can test | — | **Missing auto flow + progress** | VM + Application `VerifyConnection` handler | DB reachable | Internal logging | Unit + integration |
| 5-6 | Create System Admin window (red warning, User/Pass/Confirm + eye toggles, button, success → close → login via shortcut) | Domain `User` + `Permission` + `CurrentUserService` support creation | — | **Missing window/VM/command/hasher** | `Application/Features/Users/Commands/CreateSystemAdmin`; `Domain` hasher contract; `Presentation/Views+VMs/Setup` | Hashing (§11), run-once (§7) | See §11 | Unit (handler/validator), integration, manual |
| 7 | Run-once guarantee (DB-backed) | `SystemSettings(Id=1)` singleton exists | — | **Missing flag/table/condition** | DB schema (§19), app startup router | Migrations, `Users` table | No bypass | Integration |
| 8 | Clients go to login, not admin creation | — | `ShellViewModel` exists | **Missing login screen entirely** | Login Window/VM (pre-existing gap — see assumption) | Admin exists | — | Manual |
| 9 | App-level auth, not Windows/SQL creds | `ICurrentUserService`, `IsAbsolutePermission`, `PasswordHash` field | — | **Missing login handler/hasher** | Same as 5-6 | Hasher | No WinAuth | Unit |
| 10 | Full-admin via grant (redundancy) | **Exists** — `IsAbsolutePermission` bypasses `AuthorizationBehavior:34` | — | Nothing — reuse | `UserPermissionGrant`, `AuthorizationBehavior` | — | — | Regression |
| 11-12 | No hardcoded/default/plaintext passwords; DB cred ≠ app cred | — | `PasswordHash` column exists | **Missing hasher + separation** | `IPasswordHasher`, connection-string store (§7) | See §14 | Hasher tests |
| 13 | Config storage: dev repo vs local dev vs distribution | `.gitignore` correct, `appsettings.json` in output | — | **Missing customer mechanism** | See §7 | Engine constants | DPAPI | Manual distribution test |
| 14 | First-run via UI, auto provisioning/storage/discovery/verify | None | Host builder present | **Missing all** | §8 + §9 + §10 | §7, §11, §12 | — | E2E |
| 15-16 | Error handling & recovery (idempotent) | `Result`/`Error` types exist | — | **Missing per-error UX** | VMs, handlers, migrator | Setup state | Safe messages | Unit + manual |
| 17-18 | Security & Arch fit | Building blocks exist | — | **Missing wiring** | See §14/§15 | — | See §14 | Security review |

---

## 6. Database Engine and Provisioning Analysis

### 6.1 Evaluation matrix (Fact-based, Windows desktop, LAN)

| Engine | Can serve LAN clients? | Auto-install story | Service / firewall | Suitability for commercial lab | Licensing / redistribution |
|--------|------------------------|--------------------|--------------------|--------------------------------|----------------------------|
| **LocalDB** (current) | **No** — per-user on-demand Named Pipe, no TCP listener by default, not discoverable remotely | Trivial (lightweight MSI) but irrelevant | No service, no FW rule | **Unsuitable** — violates Business Context multi-PC requirement | Free, but technically wrong |
| **SQL Server Express** (default instance `SQLEXPRESS`) | **Yes** — real service (`MSSQL$SQLEXPRESS`), TCP/IP, Browser | `SQLEXPR` bootstrapper (offline ~600MB) or online download; installer can chain | Service auto-start; requires TCP enabled + port open + Browser | **Recommended** — correct for small lab (≤10GB per DB in Express is ample) | Free, redistributable per Microsoft terms, must include EULA checkbox |
| **SQL Server Express LocalDB** disguised as server (attempted hack) | No — any workaround is unsupported | — | — | Do not attempt | — |
| **SQLite / SQL Compact** | No multi-PC | — | — | Rejected — no concurrent network access | — |

**Recommendation:** **SQL Server Express** (named instance **`TOPLAB`** — distinct from default `SQLEXPRESS` to avoid colliding with an existing instance on customer PC; fixed DB name **`TopLab`**). Alternative acceptable: `SQLEXPRESS` if `TOPLAB` install idempotency is complex — owner decision §22.

**Contradiction to flag explicitly:** The current `appsettings.json`/`DesignTimeDbContextFactory` LocalDB line is a **technical contradiction** with "primary hosts DB for clients". It must be replaced. This plan does NOT silently keep LocalDB.

### 6.2 What "automatic" means (no manual SQL config)

- Installer (or first-run elevated helper) does: `SQLEXPR_x64_ENU.exe /QS /ACTION=Install /FEATURES=SQLEngine /INSTANCENAME=TOPLAB /SQLSVCACCOUNT="NT AUTHORITY\NETWORK SERVICE" /SQLSYSADMINACCOUNTS="BUILTIN\Administrators" /TCPENABLED=1 /IACCEPTSQLSERVERLICENSETERMS` (exact switches TBD, kept minimal).
- Enable TCP/IP via `SqlServerManager16.msc` equivalent (WMI/SMO or registry) + restart service.
- Ensure **SQL Server Browser** set to Automatic + Started (needed for named-instance UDP 1434 discovery).
- Create DB `TopLab` if absent (`CREATE DATABASE [TopLab]` via `master` connection); then `MigrateAsync`.

**Requires elevation (UAC).** First-run provisioning must be run elevated or delegate to an elevated helper/installer step — non-elevated WPF cannot install a service.

### 6.3 Network / Firewall / Service

- **Ports:** TCP 1433 (or dynamic port — prefer fixed 1433 for `TOPLAB` to simplify clients; otherwise Browser + dynamic port). Firewall rule: `netsh advfirewall firewall add rule name="TopLab SQL (TOPLAB)" dir=in action=allow program="…\MSSQL16.TOPLAB\MSSQL\Binn\sqlservr.exe" enable=yes` or port-based rule. Must be created by elevated installer/helper.
- **Protocols:** Enable `TCP` + `Named Pipes` optional; disable `VIA`.
- **Discovery aid:** Browser on `UDP 1434` must be allowed for instance lookup (or clients use fixed port, avoiding Browser dependency — recommended as fallback).
- **Licensing/Distribution:** SQL Express is free to redistribute; MSI bootstrapper must present Microsoft license; offline bundle increases installer size but works without internet — owner decision §22.

### 6.4 Alternatives considered

- **App-level relay (WPF acting as DB proxy via gRPC/SignalR):** Adds unnecessary architecture, violates §5/§18 (no second architecture), not needed for LAN SQL.
- **Cloud-hosted DB:** Contradicts "local network" & offline medical lab requirement.

---

## 7. Configuration Storage Analysis

### 7.1 Three tiers (per §13)

| Tier | Purpose | Where it lives | In Git? |
|------|---------|----------------|---------|
| **A. Dev repo config** | `dotnet ef` design-time, CI build | Hard-coded fallback in `DesignTimeDbContextFactory.cs:12` (to be changed to `Server=.\TOPLAB;Database=TopLab;...`) | Yes (not secret) |
| **B. Local developer config** | Each dev's workstation DB | `src/TopLab.Presentation/appsettings.json` (user-local, `.gitignore`-excluded) | **No** — correct, not a defect |
| **C. Customer distribution** | Primary: generated connection string; Client: discovered connection string | **New mechanism**: `ProgramData\TopLab\connection.json` (DPAPI-protected) + fallback to exe-adjacent `appsettings.json` for dev | Shipped template + runtime file (see below). Template may be committed; runtime file never committed |

**Fact:** Current `.gitignore:18` excludes `appsettings*.json`. This is **correct** for B, and not to be "fixed" by committing secrets. C must be designed for published package, not repo clone.

### 7.2 Recommended mechanism (Recommendation)

- **Primary:** After provisioning, write a JSON file to **`%ProgramData%\TopLab\connection.json`** (machine-wide, survives updates) containing `{ "ConnectionStrings:TopLab": "<DPAPI-encrypted>" }` or the raw connection string encrypted via `ProtectedData.Protect` (CurrentUser+Machine scope → LocalMachine scope so all Windows users on that PC can read). Also write a secondary copy beside exe (`appsettings.json`) for backward compat with existing `Host.CreateApplicationBuilder` JSON provider, but primary authority is `ProgramData`.
- **Implementation excerpt (plan-only, not implemented):**
  - Introduce `IConnectionStringProvider` / `IConnectionStringStore` (Application port, Infrastructure implementation) that `AddInfrastructure` resolves *before* `AddDbContext` (replace direct `configuration.GetConnectionString("TopLab")`).
  - Resolution order: `ProgramData DPAPI file` → `appsettings.json` → `environment variable` → throw localized setup error (not raw `InvalidOperationException`).
  - `DesignTimeDbContextFactory` keeps a constant fallback (`Server=.\TOPLAB;Database=TopLab;Trusted_Connection=True;TrustServerCertificate=True`) for `dotnet ef` in dev — does NOT use DPAPI.

### 7.3 Why not alternatives

| Alternative | Disadvantage |
|-------------|--------------|
| Registry `HKLM\Software\TopLab` | Works, but less transparent for support, still needs DPAPI; file is simpler to inspect/backup |
| `user-secrets` / env vars | Per-user, not machine-wide; client PCs need machine-wide |
| Check in `appsettings.json` with example connection | Violates `.gitignore` intent, leaks local paths |
| Store connection string inside DB | Circular — need connection to reach DB |

### 7.4 Separation of secrets (§12)

- **DB credential:** For Trusted_Connection mode, no password stored; if SA SQL Auth is chosen, the SQL password is DPAPI-protected in `ProgramData` and **never shown in UI nor logs** (`IAppLogger` must scrub).
- **App user credential:** `User.PasswordHash` only; plaintext never stored. The two are never conflated.

---

## 8. Primary/Server Setup Plan

### 8.1 Owner-visible flow (what user sees)

1. App first launch → **Role selection** (`§5.1`) — two large buttons: `هذا هو الجهاز الرئيسي (السيرفر)` / `هذا الجهاز سيتصل بجهاز رئيسي موجود`. No text fields.
2. User picks **Primary** → progress page: `جاري تجهيز قاعدة البيانات...` (spinner + 4 steps: *Engine → Create DB → Migrations → Verify*). No "Test Connection" button — auto.
3. On DB success → **Create System Admin** window (§10) immediately (modal, blocks shell).
4. On admin created → `تم ✓ — أغلق نافذة الإعداد وشغّل البرنامج من اختصار سطح المكتب وسجّل الدخول ببياناتك الجديدة.` Shell closes; next launch → normal login.

### 8.2 Technical steps (in order — see §17)

| Step | Action | Component | Failure message (Arabic, non-technical) | Internal log |
|------|--------|-----------|------------------------------------------|--------------|
| P1 | Detect if `.\TOPLAB` Express installed (`sc query MSSQL$TOPLAB`, registry `HKLM\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL`) | `ISqlEngineProvisioner.IsInstalledAsync` | `تعذّر العثور على محرك قاعدة البيانات` | Version, instance name, error code |
| P2 | If not installed, launch **elevated** installer helper (bundled `SQLEXPR` bootstrapper in `%ProgramData%\TopLab\Setup\SQLEXPR...exe`) or prompt "جاري تثبيت محرك قاعدة البيانات — قد تستغرق دقائق" | Installer or elevated `ProvisionEngineAsync` | `فشل تثبيت محرك قاعدة البيانات (P2). تحقّق من اتصال الكهرباء وحاول مرة أخرى.` + "Show details" expandable | Exit code, log file path, UAC denied flag |
| P3 | Enable TCP/Browser/restart, create FW rule | `ConfigureNetworkAsync` | `تعذّر تهيئة الشبكة لقاعدة البيانات` | Registry keys, FW rule result |
| P4 | Create DB `TopLab` if missing (`master` connection: `IF DB_ID('TopLab') IS NULL CREATE DATABASE [TopLab]`) | `EnsureDatabaseAsync` | `تعذّر إنشاء قاعدة البيانات` | SQL error number, duration |
| P5 | `await db.Database.MigrateAsync()` | `MigrateAsync` | `تعذّر إعداد جداول النظام` | Migration names, exception |
| P6 | **Schema verification:** probe for `Users` + `SystemSettings` + `__EFMigrationsHistory` + seeded `Permissions` count==13, `SystemSettings(Id=1)` exists | `VerifySchemaAsync` | `اكتمل الإنشاء لكن التحقق فشل — سيُعاد المحاولة` | Missing table/row |
| P7 | Persist connection string (DPAPI) to `ProgramData\TopLab\connection.json` | `IConnectionStringStore.SaveAsync` | `تعذّر حفظ الإعدادات` | Path, DPAPI result |
| P8 | Navigate to Create System Admin (§10) if `Users` empty; else skip (idempotent) | Startup router (§12) | — | Decision trace |

**Never silently continue on P2-P6 failure.** All failures block forward navigation, show Arabic non-technical headline + expandable technical details (logged, not toast), and offer **إعادة المحاولة**.

### 8.3 Constants & Build

- **Fixed values** (Recommendation — owner approval §22): SQL instance `TOPLAB`, DB `TopLab`, Auth `Trusted_Connection=True;TrustServerCertificate=True` (no SQL password). If SQL Auth required (e.g., to avoid Windows user variance), username `toplab_app` + strong random 32-char password DPAPI-stored — still constant across installs in *form* but per-machine secret in *value*.
- No user-editable constants. These live in `Infrastructure` constants (`SqlEngineConstants.cs`) and `appsettings.template.json` shipped in package.

---

## 9. Client LAN Discovery and Connection Plan

### 9.1 Options evaluation (as required by §3 of prompt)

| Mechanism | How it works | Pros | Cons | Reliability | Security | Complexity | Windows FW | Lab suitability |
|-----------|--------------|------|------|-------------|----------|------------|------------|-----------------|
| **A. UDP broadcast + app beacon (Recommended)** | Primary runs a lightweight **beacon service** (or beacon loop inside primary app when running, + optional Windows Service `TopLab.Beacon` on port e.g. `UDP 47777`). Clients broadcast `DISCOVER_TOPLAB_v1`; beacons reply with `{host, instance, port, db=TopLab, version}` signed with a pre-shared lab token (or unauthenticated on trusted LAN — see §14). | No dependency on name resolution; works on typical flat /24 lab LAN; instant; UX = list "Connect to: LAB-PC-01" | Needs beacon running; blocked if FW blocks UDP; needs beacon impl | **High** on flat LAN with FW rule | Spoofing risk mitigated by verification handshake (§14) | Medium — sane `UdpClient` + timeout + retry | Requires one inbound UDP rule on primary, outbound on clients | **Best fit** — non-technical UX, deterministic |
| **B. mDNS / DNS-SD (Bonjour)** | Publish `_toplab._tcp.local` PTR/SRV; clients browse via `Makare.X\Zeroconf` nuget | Zero-config standard, no beacon | Needs mDNS responder on all PCs; corporate VLANs may filter multicast 224.0.0.251; extra nuget + native interop | Medium — depends on mDNS stack | Similar spoof risk | Medium-high | Multicast 5353/UDP must be open | Good on modern Win 10/11 but less predictable |
| **C. SQL Server Browser (UDP 1434) + SqlDataSourceEnumerator** | `SqlDataSourceEnumerator.Instance.GetDataSources()` or raw UDP 1434 SSRP `CLNT_BCAST_EX` | Native SQL discovery, no custom beacon | Deprecated/slow (30s+), unreliable on Win 11, filtered by FW, enumerates *all* SQL instances (noise) | **Low** — often empty | Lists unrelated instances | Low code but high flakiness | UDP 1434 must be open | **Not recommended as sole mechanism** |
| **D. NetBIOS / `NetServerEnum` / `DirectoryEntry` scan** | Enumerate SMB hosts, probe each for SQL | No custom service | Noisy, slow, requires SMB, admin shares may be disabled | Low | SMB exposure | Medium | SMB ports | Poor |
| **E. Cloud rendezvous / QR pairing** | Owner enters pairing code shown on primary | Works across subnets | Requires internet or manual step — contradicts "no typing" | — | — | High | — | Out-of-scope |

**Recommended approach (A): UDP broadcast beacon + SQL verification handshake.** Optionally augment with **C as fallback** (try `GetDataSources` if UDP yields zero results). mDNS (B) can be **phase-2** if UDP blocked in some labs.

### 9.2 Recommended detailed flow (Recommendation)

1. Client role selected → progress: `جاري البحث عن الجهاز الرئيسي...` (indeterminate + broadcast animation).
2. App sends 3 UDP broadcast rounds (`255.255.255.255:47777`, plus subnet-directed if known) spaced 800ms, timeout 6s total.
3. Collect replies (de-dup by `Environment.MachineName`). Cases:
   - **0 replies:** → `لم يتم العثور على جهاز رئيسي — تأكد أن الجهاز الرئيسي يعمل ومتصل بنفس الشبكة` + `إعادة البحث` + `أدخل التفاصيل يدويًا (للدعم الفني فقط)` collapsed (advanced, not primary UX).
   - **1 reply:** → show `اتصل بـ: LAB-PC-01` (host+instance), button `اتصال`.
   - **>1 reply:** → list radios `اختر الجهاز الرئيسي:` with machine names + last-seen + optional `Ping` hint.
4. User clicks `اتصال` → **automatic verification** (§4/§14) **without** separate Test button: build `Server=HOST\TOPLAB;Database=TopLab;Trusted_Connection=True;...` (or `HOST,1433` if fixed port), call `CanConnectAsync` → `SELECT 1` → probe `Users` table exists. Success → persist DPAPI connection file then route to **normal login** (not admin creation). Failure → inline error `تعذّر الاتصال بـ LAB-PC-01 — ...` + `إعادة المحاولة`.

### 9.3 Architecture for discovery

- **New components:** `Application/Discovery/IPrimaryDiscoveryService`, `Infrastructure/Discovery/UdpPrimaryDiscoveryService`, `Infrastructure/Discovery/BeaconHostedService` (primary-side `BackgroundService` listening on UDP, replying with fixed payload). Contracts live in `Application` (Clean-Arch §15), implementations in `Infrastructure` (needs `System.Net.Sockets`).
- **Beacon payload:** versioned JSON, includes `machineName`, `fqdn`, `instance=TOPLAB`, `port=1433`, `db=TopLab`, `appVersion`, optional `hmac` if lab token used (see §14).

### 9.4 Why this fits Top-Lab

- Keeps SQL as the **single source of truth** (discovery only finds *where* to connect; verification confirms schema).
- No cloud, no typing, no installer-time config. Works on typical small-lab flat LAN with consumer switches.

---

## 10. Create System Admin Plan

### 10.1 Window spec (exact per §5)
┌────────────────────────────────────────────────┐ │ ⚠️ باللون الأحمر: احفظ اسم المستخدم وكلمة المرور في مكان آمن — لا يمكن استرجاعهما بسهولة. │ ├────────────────────────────────────────────────┤ │ اسم المستخدم: [________________] │ │ كلمة المرور: [••••••••] [👁 toggle] │ │ تأكيد كلمة المرور: [••••••••] [👁 toggle] │ │ │ │ [ إنشاء حساب المدير ] │ └────────────────────────────────────────────────┘


- **a. Red warning** — `TextBlock Foreground=Red FontWeight=Bold TextWrapping=Wrap`, `FlowDirection=RightToLeft`.
- **b-e exactly** — no extra fields. If architecture forces one field (e.g., `IsAbsolutePermission` is always `true` for this flow — it is, no UI control needed).
- Eye toggles are **independent** (`ShowPassword`/`ShowConfirmPassword` bools → `PasswordBox` vs `TextBox` overlay or attached behavior; plain `PasswordBox.Password` is not bindable — need behavior/attached prop).

### 10.2 Behaviour (§6)

| Step | Application | Presentation |
|------|-------------|--------------|
| Validate | `CreateSystemAdminCommandValidator` (FluentValidation): `UserName` required, trimmed, 3-100 chars, regex `^[\w.\-@]+$` (allow Arabic? decision §22), `Password` 8+ chars, `Confirm==Password`, optional strength (if policy exists — inspect §11). Error → `Error.Validation` + `ResultErrorPresenter`. | VM validates live, shows inline Arabic messages; button disabled while `HasErrors`. |
| Check uniqueness | Handler queries `Users.Any(UserName==normalized)` → `Error.Conflict("اسم المستخدم موجود مسبقًا")`. | Show error near username field. |
| Hash | `IPasswordHasher.Hash(password)` → `PasswordHash`; `InternalWindowsPasswordHash` — set to same hash or sentinel (see §11 — keep column non-empty due to `IsRequired` mapping). | — |
| Create | `User.Create(UserId.Create(0) or generated, userName, hash, internalHash, isAbsolute: true)`; immediately `Add` + `SaveChangesAsync`. Use auditable interceptor as system user (CreatedBy=0 or new user's Id — see assumption). | — |
| Check permissions | If `IsAbsolutePermission=true`, no grants needed (bypasses `AuthorizationBehavior`). Optionally grant every seeded `Permission` for redundancy (§10 of prompt explicitly says a non-admin with all grants should still be full-admin — that is also covered by `IsAbsolute=false + all grants` path; here we use absolute for the initial admin). | — |
| Run-once guard | Wrap in transaction + unique index on `UserName`; if second concurrent creation attempted, DB constraint → `Conflict`. Pre-check `Users.Any()` at start of handler. | VMs race is harmless — second gets `Conflict` message. |
| Success UX | Handler returns `Result<UserId>` success → VM shows `MessageBox/Dialog` `تم إنشاء حساب المدير بنجاح — أغلق نافذة الإعداد وشغّل البرنامج من اختصار سطح المكتب وسجّل الدخول بالبيانات الجديدة.` → close setup window (or `Application.Current.Shutdown()` + instruct desktop shortcut). | Red warning remains visible; do not auto-fill login. |
| Next launch | Shell startup router sees `Users.Any()` → skip admin window → go to normal login screen. Credentials work via normal login handler (must verify/still TODO — assumption). | — |

### 10.3 No second auth system

Reuse `User.PasswordHash` + `IPasswordHasher` (see §11). Do not introduce `AspNetCore.Identity`. Keep `User` entity as-is, extend via `IPasswordHasher` port.

### 10.4 Out-of-scope extras that must NOT be added silently

- Email, phone, display name, role picker — all **Not added**. If owner later wants display name, it is a separate feature request.

---

## 11. Authentication and Permission Integration

### 11.1 Current state (Fact)

- Auth is **application-level** (§9 of prompt already satisfied architecturally): `ICurrentUserService` + `AuthorizationBehavior` + DB-stored `User`/`Permission`/`UserPermissionGrant`. No `Windows Authentication` or `Integrated Security` used as *login identity* (the `Trusted_Connection=True` in connection string is **SQL transport**, not app login — separation preserved §12).
- **Missing piece:** `IPasswordHasher` + `LoginCommandHandler` (assumption: login not yet implemented — not found in `Features`). If login exists elsewhere hidden, wire to same hasher.

### 11.2 `IPasswordHasher` (Recommendation)

```csharp
// Application port (TopLab.Application.Common.Security)
public interface IPasswordHasher {
  string Hash(string password);
  bool   Verify(string password, string hash);
}
Impl: Infrastructure/Security/Pbkdf2PasswordHasher using Rfc2898DeriveBytes (PBKDF2-HMAC-SHA256, 210k iterations, 16B salt, 32B subkey, format PBKDF2$<iter>$<saltB64>$<hashB64>) — built-in BCL, no extra nuget, AOT-friendly, upgrade-path via re-hash on login.
Alternative: BCrypt.Net-Next — stronger per-hash cost but adds LGPL nuget. PBKDF2 is recommended to keep dependencies minimal.
Registration: AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>() in Infrastructure.
Tests: Hash is salted (two hashes differ), verify correct/incorrect, upgrade path.
11.3 Login integration (if not yet present)
New Application/Features/Auth/Commands/Login/{LoginCommand, Handler, Validator}: look up User by normalized UserName, Verify(password, PasswordHash), check IsActive, update LastLoginAtUtc + RecordLogin, CurrentUserService.SetSession(...) with grants loaded via UserPermissionGrants join; return Result<LoginResponse>.
Presentation LoginWindow/VM consumes it — but that login screen is a prerequisite for §10 success criteria ("credentials must work via normal login"). The plan treats login as an existing-gap dependency: if login truly doesn't exist, the Create-Admin success cannot be demonstrated end-to-end. Flag as assumption requiring verification.
11.4 Permissions — already supports §10-11
IsAbsolutePermission=true grants everything (AuthorizationBehavior:34). §10's "non-admin with complete grants" scenario is satisfied by creating a second user with IsAbsolute=false but UserPermissionGrants containing all 13 Permission rows — verified by creating a test user that way and asserting HasPermission for each code.
Initial System Admin is created with IsAbsolutePermission=true (simplest, fulfills "full System Admin").
12. Setup State and Recovery Design
12.1 DB-backed setup state (Recommendation — required by §7)
Add a new singleton row/table to avoid false positives from file flags.

Option A (preferred): New table SetupState (PK=1) with IsInitialSetupCompleted bool, CompletedAtUtc, CreatedAdminUserId int?.

Option B: Reuse SystemSettings.Id=1 adding IsInitialSetupCompleted column. Less clean than dedicated table but fewer migrations.

Recommendation: Option A (SetupState), because SystemSettings is a domain settings aggregate for lab tuning (§3.2 SystemSettingsConfiguration); overloading it with setup lifecycle mixes concerns.

Run-once predicate (DB-true): Users.Any(u => u.IsActive) AND SetupState.IsInitialSetupCompleted. Rationale: Users alone is insufficient if a manual DB is restored with users but setup not officially completed; SetupState is explicit. Use transaction when setting SetupState after admin creation so both commit together.

Do NOT rely on a local file/registry flag as primary — file is only a cache/hint (e.g., %ProgramData%\TopLab\.setupCompleted) to short-circuit DB probe when DB unreachable; authoritative is DB.

12.2 Startup router (state machine)
On App.OnStartup before showing MainWindow:

Try resolve connection string (DPAPI file → appsettings).
If no connection string:
  → Show RoleSelectionWindow.
Else:
  Try CanConnectAsync (timeout 5s).
  If !CanConnect → show "قاعدة البيانات غير متصلة" + RoleSelection for repair? (or reconnect flow)
  Else:
    Ensure schema verification (quick probe).
    If SetupState.IsInitialSetupCompleted == false or !Users.Any():
      If running on Primary (determined by connection host == localhost / .\TOPLAB):
        → Show CreateSystemAdminWindow (or RoleSelection if host unknown)
      Else (client):
        → Show LoginWindow (not admin creation)
    Else:
      → Show LoginWindow → Shell
Presentation App.xaml.cs hosts this router; it uses a small synchronous bootstrap scope (create ServiceProvider to query SetupState via IApplicationDbContext) before deciding which Window to construct.
The router must not throw InvalidOperationException to the OS on missing config — it must show the role selector.
12.3 Idempotency table (§16)
Interrupted at	State after crash	Recovery on next launch
Before engine install completes	No DB	Re-enter Primary flow at P1, installer is idempotent, retry
After DB created, before migrations	Empty DB, no tables	MigrateAsync is idempotent (EF checks __EFMigrationsHistory), re-run
After migrations, before verification	Tables present, SetupState false, Users empty	Verification probe fails first time if incomplete, re-run migrations (no-op)
After migrations+verify, before admin created	DB ready, Users empty, SetupState false	Router shows Create Admin again (correct)
During admin SaveChanges (crash mid-tx)	No row committed (transaction rollback)	Same as above — retry
After admin row committed, before SetupState marked	Users.Any()==true but SetupState false	Router still shows Create Admin → second attempt hits Conflict+ idempotent guard; handler should auto-heal: if Users.Any()==true and admin creation is racing, mark SetupState=true and skip second insert (or offer "Admin already exists — go to login")
After both committed, flag set	SetupState=true, admin exists	Normal — skip setup forever, go to login
Elevated helper crash	Partial install log remains	Read MSI log, retry with repair switch
Implementation guard: CreateSystemAdminCommandHandler runs inside TransactionScope (or ExecutionStrategy+ explicit BeginTransaction) that (1) inserts User, (2) inserts/grants if needed, (3) upserts SetupState(Id=1, IsInitialSetupCompleted=true), committed atomically.

13. Error Handling Design
13.1 Per-error matrix (Requirement §15)
Error	User-visible (Arabic)	Internal handling / log	Retry / next step
DB engine install failure	فشل تثبيت محرك قاعدة البيانات (رمز {code}). تأكد من أن لديك صلاحيات المسؤول وأعد المحاولة.	MSI exit code, log path %TEMP%\TopLab_SqlSetup.log, UAC denied flag; IAppLogger logs without secrets	إعادة المحاولة + عرض التفاصيل expandable; do not proceed
Engine startup failure	محرك قاعدة البيانات لا يستجيب.	sc query state, event log excerpt	إعادة المحاولة after 10s; offer "إعادة تشغيل الجهاز" hint
DB creation failure	تعذّر إنشاء قاعدة البيانات — تحقّق من مساحة القرص.	SQL error 1807/5170, disk info	Retry; if disk full show disk hint
MigrateAsync failure	تعذّر إعداد جداول النظام. + code	Migration name, SQL error, exception type; scrub connection string	Retry; if persistent, suggest support contact with log file path
Schema verification failure	اكتمل الإنشاء لكن التحقق فشل — سيُعاد المحاولة تلقائيًا.	Missing table list	Auto-retry once, then manual support
LAN discovery timeout (0 hosts)	لم يتم العثور على جهاز رئيسي — تأكد أن الجهاز الرئيسي يعمل ومتصل بنفس الشبكة وحاول مرة أخرى.	Beacons seen =0, duration, subnet	إعادة البحث
Multiple primaries found	Show list: اختر الجهاز الرئيسي + radios	All hosts seen	User picks one → then verify
Primary found but unreachable	تم العثور على {name} لكن تعذّر الاتصال — تأكد من جدار الحماية.	CanConnectAsync exception, TCP error	إعادة المحاولة with exponential backoff 1s/2s/4s (max 3)
Auth/connection failure	فشل الاتصال بقاعدة البيانات على {name}	Same as above	Same
Invalid username (CreateAdmin)	اسم المستخدم مطلوب — 3 أحرف على الأقل	Validation path	Inline field error
Weak password (if policy)	كلمة المرور ضعيفة — استخدم 8 أحرف مع حروف وأرقام	Policy rule violated	Inline
Password mismatch	كلمتا المرور غير متطابقتين	—	Inline
Duplicate admin creation	حساب المدير موجود مسبقًا — انتقل إلى تسجيل الدخول.	UNIQUE violation on UserName	Navigate to login
Partial/interrupted setup	See §12.2	SetupState + Users probes	Auto-heal or re-enter setup
User closes setup window / app crash	Log SetupAborted	—	Re-enter router on next launch
13.2 Where & how
Validation errors: ValidationBehavior → Result.Failure(errors) → ResultErrorPresenter → field-level in VMs.
Infrastructure exceptions: In handlers/provisioner: catch (SqlException) → Error.Unexpected(Arabic message, code) — never leak connection string/stack to UI by default; detailed exception goes to IAppLogger + Debug.WriteLine + optional log file.
Logging without secrets: LoggingBehavior + provisioner/discovery must redact Password=, Pwd=, TrustServerCertificate not redacted, but passwords/PBKDF2 hashes never logged. Enforce via helper RedactConnectionString(string).
14. Security Design
Concern	Requirement	Recommendation	Notes
DB credential protection	Trusted_Connection=True → no DB password on LAN; if SQL Auth used, DPAPI (LocalMachine) protects password file	ProtectedData.Protect(..., DataProtectionScope.LocalMachine) + ACL ProgramData\TopLab to Users:Read, Administrators:Full	Never echo DB credential in UI/logs; RedactConnectionString in logging pipeline
App password hashing	No plaintext, no hardcoded/default fallback	IPasswordHasher (PBKDF2) as in §11; User.PasswordHash only stores hash; never log password even on failure	Verify column HasMaxLength(300) fits PBKDF2 format (fits ~120 chars)
LAN discovery spoofing	Malicious host replies to broadcast and pretends to be primary	Handshake verification: after picking a beacon, client performs token-free but schema-verifying connection attempt (CanConnect + SELECT from __EFMigrationsHistory + check Permissions count). Optional second factor: a pairing code shown on primary screen (6-digit, ephemeral) that client must confirm — owner decision §22. Without pairing code, attacker can only claim to be primary but cannot serve correct schema; failure will surface on verification step	Do not claim prevention of determined insider; goal is non-trivial spoof + clear error
Unauthorized clients	Any device on LAN could attempt discovery/connect	SQL Trusted_Connection restricts to Windows-authenticated LAN users; if lab WLAN is open, consider additional app-level shared secret (lab PIN set at primary setup) stored hashed in SystemSettings — owner decision	
Firewall exposure	Primary opens ports to LAN	Minimal: one TCP port (1433 fixed) + UDP 47777 (beacon) + optional 1434 if Browser/dyn-port; all scoped to private/Domain profile only, not Public	Installer creates private-profile rule; claim "Expose to Public networks: No"
SQL network least privilege	Don't expose SA, don't run as admin	App connects via NT AUTHORITY\NETWORK SERVICE or dedicated toplab_app SQL login with db_datareader/writer + db_ddladmin (only during migrate) then downgrade; never use sa as app credential	Migrate step uses elevated temporary connection? — actually Trusted_Connection as installer admin suffices for DDL
Credential disclosure	No accidental UI/log disclosure	Rules: (1) UI never displays connection string, (2) logs redact, (3) PasswordBox masked, clipboard disabled on hash, (4) crash dumps not containing secrets	
Logging	Audit without secrets	IAppLogger.Log(requestName, outcome, duration) must not log field values; explicit NoSensitiveData convention in code review	
Medical-lab / compliance	Do not claim HIPAA/GDPR compliance from code	Plan does not claim compliance; notes that encryption at rest (TDE) is not enabled by default on Express; backups must be considered separately	
15. Architecture and Layer Placement
Principle: Avoid a second architecture. Each new piece goes to the layer where its responsibility naturally lives (Clean Arch + existing DI).

Component	Layer	Reason
IConnectionStringProvider / IConnectionStringStore (interface + DPAPI file impl)	Application (interface) / Infrastructure (impl)	Persistence-neutral port; Infrastructure handles DPAPI/file
SqlEngineConstants (Instance=TOPLAB, Database=TopLab)	Infrastructure (or SharedKernel)	Deployment constants, not business rules
ISqlEngineProvisioner / SqlEngineProvisioner (detect/install/configure TCP/Browser/FW, EnsureDatabase)	Infrastructure	Needs registry, ServiceController, netsh, SQL master connection — infra-only
IMigrator / EfCoreMigrator (MigrateAsync, schema verification VerifySchemaAsync)	Infrastructure (implements Application port IDatabaseProvisioner)	Wraps ApplicationDbContext + migrations assembly
IPrimaryDiscoveryService / UdpPrimaryDiscoveryService	Application (interface) / Infrastructure (impl)	Network I/O is infrastructure; discovery query object is Application if built as MediatR query
BeaconHostedService : BackgroundService (primary beacon listener)	Infrastructure (or Presentation if tied to app lifetime)	Prefer IHostedService registered in AddInfrastructure only on primary; alternative: Presentation owns beacon when primary app is running + defer to Windows Service phase-2 if needed
SetupState entity + SetupStateConfiguration + migration	Domain + Infrastructure	Domain entity (singleton), Fluent config + seed as in SystemSettingsConfiguration
IPasswordHasher / Pbkdf2PasswordHasher	Application (interface) / Infrastructure (impl)	Hashing is crypto infra; port stays in Application for CreateSystemAdmin handler
CreateSystemAdminCommand / Handler / Validator	Application	Business rule (unique username, hash, IsAbsolute, grant SetupState) — not UI
VerifyConnectionQuery/Handler (used by client flow)	Application	Wraps CanConnectAsync + schema probe, returns Error typed appropriately
RoleSelectionViewModel / PrimarySetupViewModel / ClientDiscoveryViewModel / CreateSystemAdminViewModel + Windows	Presentation	MVVM, ViewModelBase, RelayCommand, NavigationService; VMs call ISender.Send(...) only
Startup router / SetupOrchestrator	Presentation (App.xaml.cs or dedicated StartupRouter service)	Owns Window lifetime decisions; calls Application ports to probe DB, not direct EF
Installer / packaging	Outside src (Wix Toolset / Inno Setup project, or publish profile)	See §16
Validation / Authorization / Logging	Application (existing behaviors)	Reused as-is; CreateSystemAdmin is IRequest<Result> with FluentValidation
What NOT to do: Do not put MigrateAsync or SQL install logic in code-behind; do not put MessageBox in Application; do not create TopLab.Setup separate architecture with its own DI — reuse the single Host.

16. Installer / Deployment Analysis
Fact: No installer/packaging project exists in the inspected working copy. No *.wixproj, *.iss, publish profile, or MSIX manifest was found. TopLab.Presentation.csproj is just a WinExe WPF app that copies appsettings.json.

Implication:

Aspect	Application first-run (WPF)	Installer / packaging
Role selection	Yes — can live entirely in WPF first-run (simpler, no installer tooling)	Optionally duplicated as installer radio if installer exists, but not required
SQL Express install	Can be triggered by elevated WPF helper on primary first-run (bootstrapper exe bundled under %ProgramData%\TopLab\Setup\)	Cleaner to chain via installer (Wix Bundle / ExePackage or Inno Run), because installer already elevated and can create FW rules; either path works
FW rules / service config	Can be done by same elevated helper	Or by installer custom action
Desktop shortcut	Created by installer, or by helper on first admin success	Installer is the proper owner
Future updates	App updater (not part of this plan) must not re-trigger setup (§7 run-once)	Installer major upgrade must preserve ProgramData\TopLab\connection.json + SetupState DB
Recommendation for commercial distribution:

Short-term (this plan's Phase 1): Implement everything as application first-run (no new installer project required to unblock B-02). Bundle SQLEXPR_x64_ENU.exe (~600 MB) in a Setup subfolder of the published package (a .zip that customer copies to primary PC) — the WPF helper launches it elevated. This lets the team ship without learning Wix immediately.
Medium-term (Phase 2, before mass distribution): Add a proper Wix Toolset v4 Bundle (or innocent alternative Inno Setup) project TopLab.Setup that: (1) installs .NET 8 Desktop Runtime if missing, (2) installs SQL Server Express TOPLAB instance if missing, (3) deploys app to Program Files\TopLab, (4) creates desktop shortcut, (5) sets ProgramData\TopLab ACLs. First-run WPF then assumes engine is already present and only does DB create/migrate/verify.
No pretense: The plan explicitly records installer does not exist and separates its future work from the application work in §17/§22.

17. Dependency-Ordered Implementation Plan
Each item lists preconditions and why order matters. A later item must not start until its preconditions are done.

Order	Work item	Preconditions	Why this order	Size
D1 — Decisions	Owner approves §22 decisions (engine name, pairing-code, lab PIN, constants, installer path) — meeting + signed record	None	All downstream design depends on engine/instance choice	1 day
D2 — Password hasher	Verify whether a hasher/login already exists on an unseen branch; if truly missing, add IPasswordHasher (Application) + Pbkdf2PasswordHasher (Infrastructure) + register	D1 (algo choice)	CreateSystemAdmin handler (D6) and login depend on it; no point building admin UI without hashing	0.5 day
D3 — Fix DesignTimeDbContextFactory + defaults	Fallback string → Server=.\TOPLAB;Database=TopLab;Trusted_Connection=True;TrustServerCertificate=True (or approved constant)	D1 (instance/DB name)	Ensures dotnet ef migrations add after D5 still works and matches shipped constants	0.1 day
D4 — Connection-string storage	Implement IConnectionStringProvider/Store (DPAPI ProgramData path + fallback appsettings.json), wire AddInfrastructure to use it instead of configuration.GetConnectionString alone, keep backwards compat for dev	D1, D3	Every other runtime piece reads connection via this; provisioning (D6) writes via it; discovery (D8) writes via it	1 day
D5 — SetupState schema	Domain SetupState entity + SetupStateConfiguration + EF migration AddSetupState (Id=1 singleton seeded IsInitialSetupCompleted=false) + snapshot update	D2-D4	Run-once predicate (D7) and idempotent recovery (D10) depend on this table; must exist before any runtime probe	0.5 day
D6 — Database provisioner	IDatabaseProvisioner port + EfCoreMigrator: EnsureDatabaseAsync, MigrateAsync, VerifySchemaAsync (checks __EFMigrationsHistory, Users, SystemSettings, Permissions count) — unit-tested with InMemory/sqlite; logging without secrets	D4, D5	Primary flow and client verification share migration/verify logic; no UI should be built before DB can be created	1 day
D7 — Startup router	StartupRouter/SetupOrchestrator in Presentation that probes connection, DB, SetupState+Users, decides: RoleSelection vs PrimarySetup vs CreateAdmin vs Login vs Shell; localize errors (Arabic heads) + expandable details + retry	D4, D6, D5	This is the hub; all first-run windows are leaves it navigates to	1.5 days
D8 — Role selection UI	RoleSelectionWindow + RoleSelectionViewModel (two buttons), wired via router D7; tests for navigation	D7 skeleton	Entry point for both Primary and Client branches	0.5 day
D9 — Primary provisioning UI + elevation helper	PrimarySetupViewModel (progress steps P1-P8), ISqlEngineProvisioner (detect/configure TCP/Browser/FW; launch bundled SQLEXPR helper elevated via ProcessStartInfo Verb=runas or installer chaining); FW rule creation; error UX per §13	D4, D6, D8, D1	Power/long-running step; depends on D6 provider + D8 navigation; elevation nuance	2-3 days
D10 — LAN discovery (client)	IPrimaryDiscoveryService + UdpPrimaryDiscoveryService + BeaconHostedService (primary), ClientDiscoveryViewModel showing 0/1/N states + اتصال → auto verify	D4, D6, D8	Needs connection-store + provisioner verification; blocked until primary beacon story settled	2 days
D11 — Connection verification (auto, no button)	VerifyConnectionQuery/Handler (uses CanConnectAsync + schema probe SELECT trio), retries w/ backoff, redact logs	D6, D10	Shared by primary (P8) and client اتصال flow	0.5 day
D12 — Create System Admin (Application)	CreateSystemAdminCommand/Handler/Validator (unique check, PBKDF2 hash, IsAbsolute=true, atomic SetupState upsert + transaction), plus IPasswordHasher integration; tests	D2, D5, D6	Core business capability; needs hasher + SetupState	1 day
D13 — Create System Admin (Presentation)	CreateSystemAdminWindow + CreateSystemAdminViewModel (exact fields per §5, independent eye toggles, inline validation, success dialog → close app + shortcut hint)	D12, D7	UI cannot be tested without handler	1 day
D14 — Login wiring (if missing)	LoginCommand/Handler/Validator, LoginWindow/VM, CurrentUserService.SetSession post-hash verify; LastLoginAtUtc; remember IsActive check	D2	Without login, admin credentials cannot be proven to work; treat as dependency even if out-of-scope formally — verify existence first	1-1.5 days
D15 — Hardening & security review	Redaction helper, DPAPI scope audit, FW scoping, beacon anti-spoof review, log scrub, appsettings not committed check	D4-14	Cross-cutting sweep before testing	0.5 day
D16 — Testing	Execute §20 plan (unit + integration + manual)	All above	Cannot test run-once/LAN without all pieces	2 days
D17 — Packaging & distribution dry-run	Build dotnet publish -c Release, copy to clean VM (no SQL), exercise primary fresh → admin → client VM discovery → login; verify .gitignore still excludes runtime file	D16	Final proof	1 day
Critical-path dependencies (DAG):

D1 Decisions
 ├─► D2 Hasher ─┬─► D12 Admin (app) ─► D13 Admin (UI)
 │              └─► D14 Login
 ├─► D3 Factory
 ├─► D4 ConnStore ─► D6 Provisioner ─► D7 Router ─► D8 Role
 │                                    │          ├─► D9 Primary
 │                                    │          └─► D10 Discovery ─► D11 Verify
 D5 SetupState ────────────────────────┘
18. File-Level Implementation Map
Legend: M = modify existing file, E = extend (add members), N = new file. No file is created/modified during planning — map is for later execution.

18.1 New files (likely)
Path (new)	Purpose
src/TopLab.Domain/Setup/SetupState.cs	Singleton SetupState : Entity<int> with IsInitialSetupCompleted, CompletedAtUtc, CreatedAdminUserId
src/TopLab.Application/Common/Security/IPasswordHasher.cs	Port for PBKDF2 hashing
src/TopLab.Application/Common/Security/ (or Common/Interfaces/IConnectionStringProvider.cs, IConnectionStringStore.cs, IDatabaseProvisioner.cs, IPrimaryDiscoveryService.cs)	Application ports (no infra coupling)
src/TopLab.Application/Features/Users/Commands/CreateSystemAdmin/CreateSystemAdminCommand.cs	IRequest<Result<int>> with UserName, Password, ConfirmPassword
src/TopLab.Application/Features/Users/Commands/CreateSystemAdmin/CreateSystemAdminCommandHandler.cs	Unique check, hash, User.Create(... isAbsolute:true), transaction + SetupState upsert
src/TopLab.Application/Features/Users/Commands/CreateSystemAdmin/CreateSystemAdminCommandValidator.cs	FluentValidation rules
src/TopLab.Application/Features/AccessAndNavigation/Queries/VerifyConnection/VerifyConnectionQuery.cs + Handler.cs	Auto verification (no button), schema probe, redact logs
src/TopLab.Application/Features/Auth/Commands/Login/LoginCommand.cs + Handler.cs + Validator.cs	Login (if proven missing)
src/TopLab.Infrastructure/Security/Pbkdf2PasswordHasher.cs	PBKDF2 impl, registered as singleton
src/TopLab.Infrastructure/Configuration/DpapiConnectionStringStore.cs	ProgramData\TopLab\connection.json DPAPI (LocalMachine) read/write, fallback to appsettings.json
src/TopLab.Infrastructure/Persistence/SqlEngineConstants.cs	InstanceName, DatabaseName, BuildConnectionString(host)
src/TopLab.Infrastructure/Persistence/Provisioning/EfCoreMigrator.cs	MigrateAsync + VerifySchemaAsync
src/TopLab.Infrastructure/Persistence/Provisioning/SqlEngineProvisioner.cs + ISqlEngineProvisioner.cs	Detect/install (launch bootstrapper), enable TCP/Browser, FW rule, EnsureDatabase
src/TopLab.Infrastructure/Discovery/UdpPrimaryDiscoveryService.cs + BeaconHostedService.cs	UDP broadcast discovery + beacon listener
src/TopLab.Presentation/ViewModels/Setup/RoleSelectionViewModel.cs	Two-button VM
src/TopLab.Presentation/ViewModels/Setup/PrimarySetupViewModel.cs	Progress steps P1-P8, retry
src/TopLab.Presentation/ViewModels/Setup/ClientDiscoveryViewModel.cs	Broadcast, list, اتصال + auto verify
src/TopLab.Presentation/ViewModels/Setup/CreateSystemAdminViewModel.cs	Fields per §5, eye toggles, inline validation, success flow
src/TopLab.Presentation/Views/Setup/RoleSelectionWindow.xaml(+.cs)	Role choice
src/TopLab.Presentation/Views/Setup/PrimarySetupWindow.xaml(+.cs)	Provisioning progress
src/TopLab.Presentation/Views/Setup/ClientDiscoveryWindow.xaml(+.cs)	Search + اتصل بـ: X
src/TopLab.Presentation/Views/Setup/CreateSystemAdminWindow.xaml(+.cs)	Exact spec §5 (red warning, eye toggles)
src/TopLab.Presentation/Views/Auth/LoginWindow.xaml(+.cs) + LoginViewModel.cs	If missing — normal login
src/TopLab.Presentation/Services/StartupRouter.cs + RedactHelper.cs	Router (§12) + log scrub
src/TopLab.Infrastructure/Discovery/ tests + tests/TopLab.Application.Tests/Features/Users/CreateSystemAdminTests.cs	New test files
18.2 Existing files likely modified/extended
Path	Change
src/TopLab.Presentation/App.xaml.cs	M — replace direct MainWindow launch with startup router → conditional window (Role / Setup / Login / Shell). Keep Host.CreateApplicationBuilder pattern but switch to async router probe
src/TopLab.Presentation/DependencyInjection.cs	E — register new VMs/Windows (RoleSelection, PrimarySetup, ClientDiscovery, CreateSystemAdmin, Login), StartupRouter, BeaconHostedService
src/TopLab.Infrastructure/DependencyInjection.cs	M — replace configuration.GetConnectionString("TopLab") ?? throw... with IConnectionStringProvider.GetAsync() resolution (keep throw but localized); register ISaveChangesInterceptor, IPasswordHasher, IConnectionStringStore, IDatabaseProvisioner, ISqlEngineProvisioner, discovery services
src/TopLab.Infrastructure/Persistence/DesignTimeDbContextFactory.cs	M — LocalDB string → .\TOPLAB constant (or approved)
src/TopLab.Presentation/appsettings.json	M (local dev only) — change to .\TOPLAB default for new devs; shipped template variant may be appsettings.template.json
src/TopLab.Domain/Users/User.cs	E (optional) — consider factory overload for system-admin (CreateSystemAdmin) that enforces IsAbsolutePermission; or keep generic Create with flag
src/TopLab.Infrastructure/Persistence/Configurations/SystemSettingsConfiguration.cs or new SetupStateConfiguration.cs	E/N — seed SetupState or extend SystemSettings
TopLab.sln, Directory.Packages.props, src/*/csproj	E — optional nuget adds (System.Net.Sockets is BCL; FluentValidation already present; no new package needed for PBKDF2/UDP)
src/TopLab.Application/Common/Behaviors/LoggingBehavior.cs	E — add scrub hook for connection-string/broadcast payload
tests/** mirrors	E — new unit/integration tests
18.3 Installer/deployment (future, not part of this plan's coding)
Path	Purpose
installer/TopLab.Setup/TopLab.Setup.wixproj (or TopLab.Installer.iss)	Bundle, SQL Express ExePackage, app MsiPackage, FW rules, shortcut
src/TopLab.Presentation/publish/ profile	dotnet publish self-contained? Owner decision
TopLab.Presentation/appsettings.template.json (shipped)	Template pointing at .\TOPLAB, not secret, committed
18.4 No file created now (plan-only)
This section is descriptive. Actual file creation belongs to the coding agent after owner approval (§29 — no implementation in planning mode).

19. Database / Migration Impact
Item	Required?	Details	Order
New table SetupState	Yes (recommended)	Id int PK (1, ValueGeneratedNever), IsInitialSetupCompleted bit NOT NULL, CompletedAtUtc datetime2 NULL, CreatedAdminUserId int NULL FK→Users, seeded row Id=1 IsInitialSetupCompleted=0. Created via dotnet ef migrations add AddSetupState.	Immediately after D5
Alternative: add column IsInitialSetupCompleted to SystemSettings(Id=1)	Only if owner rejects new table	Single bit column default 0; re-uses singleton row	Same
New indexes	No	Existing UserName unique index suffices for duplicate guard	—
New entities beyond SetupState	No	User/Permission/UserPermissionGrant already model admin creation	—
New relationships	If SetupState.CreatedAdminUserId FK added	FK_SetupState_Users_CreatedAdminUserId nullable	Part of same migration
Seed data change	No	13 permissions + settings seeds remain as-is (§3.5)	—
Migration file	One new migration (AddSetupState or ExtendSystemSettingsForSetup) + snapshot update	Commit migration file; do not generate now	D5
Rollback	Reversible (DropTable SetupState)	Test dotnet ef database update down/up before ship	—
Dependency order: Schema migration → provisioner must run it via MigrateAsync on primary; clients must not attempt migrations (read-only after discovery). Verification probe (§13) must include SetupState presence.

20. Testing Plan
Test area (prompt §20)	Unit	Integration	Manual/E2E
Fresh primary installation (engine not present → install → DB → admin)	SqlEngineProvisionerTests (mock registry/process)	EfCoreMigratorTests (real SQL Express on CI VM), DpapiStoreTests	Manual on clean Windows VM (no SQL) — full path
Fresh client installation	UdpPrimaryDiscoveryServiceTests (loopback beacon)	Broadcast VM test: 1 primary VM + 1 client VM on same virtual switch	Manual 2-machine LAN
Primary DB provisioning (P1-P8)	Validator + hasher + provisioner unit tests	InMemory/Sqlite migration apply + verify 13 permissions	—
Migrations + schema verification	Snapshot assertion F5ConfigurationTests	VerifySchemaAsync with tampered DB (drop table) expects failure	—
System Admin creation — success	CreateSystemAdminCommandValidatorTests, HandlerTests (duplicate, mismatch, hash shape)	Handler with real ApplicationDbContext (InMemory or LocalDB)	UI: create + close + login
Successful login with new creds	LoginCommandHandlerTests (hash verify, inactive, not found)	Login against seeded admin	Manual login via Presentation
Admin permissions (full access)	AuthorizationBehaviorTests with IsAbsolute=true	End-to-end: admin calls IRequest requiring EDIT_SYSTEM_SETTINGS	Manual: login → navigate all modules
Creation of additional privileged accounts	Grant-all-permissions handler test	Verify second non-absolute admin can also access everything	Manual: admin creates backup admin, second logs in
Restart after setup (run-once)	StartupRouterTests (SetupState=true → skips admin window)	10× restart loop, assert no second admin prompt	Manual: close/open 5×
Repeated launch	Same	—	Manual
Interrupted setup (kill mid-migrate, mid-hash, power)	Transaction rollback tests	Kill MigrateAsync mid-way (mock), verify §12 recovery	Manual: close via Task Manager mid-progress
Duplicate admin prevention	Handler concurrency test (two parallel Creates → one succeeds, one Conflict)	DB unique index enforcement test	Manual: relaunch primary after completion → no admin window
Client discovery — 0/1/N	UdpDiscoveryTests (mock sockets)	Subnet broadcast live	Manual: 0,1,2 primaries
Multiple-machine LAN	Mocked	2 VMs simultaneous broadcast	Manual lab
Network failure (FW block, cable unplug)	Timeout tests	FW-block simulation (netsh advfirewall set ... block)	Manual
DB unavailable (service stopped)	CanConnectAsync == false path	Stop MSSQL$TOPLAB, assert router shows reconnect prompt	Manual
Wrong credentials (login)	Verify false → Validation error	—	Manual: wrong password shows error
Password mismatch	Validator mismatch case	—	Manual
Secrets not in logs	RedactHelperTests (assert Password= redacted, hash not logged)	LoggingBehavior integration with fake logger inspect	Manual review of log file
Regression — existing login/auth	Existing BehaviorsAuthorizationBehaviorTests + CheckDatabaseConnectivityQueryHandlerTests	Existing ApplicationDbContextTests, AuditableInterceptor	Smoke: old modules still launch
Test categorization: dotnet test for unit/integration (no LAN needed). Manual/E2E must run on at least one real Windows pair (two VMs or two lab PCs) with real SQL Express — this cannot be mocked. CI should keep SQL Express available on self-hosted runner or skip LAN tests with [Trait("requires-lan","true")].

New test files to create (map): TopLab.Application.Tests/Features/Users/CreateSystemAdminCommandHandlerTests.cs, TopLab.Infrastructure.Tests/Security/Pbkdf2PasswordHasherTests.cs, TopLab.Infrastructure.Tests/Provisioning/EfCoreMigratorTests.cs, TopLab.Infrastructure.Tests/Discovery/UdpDiscoveryTests.cs, TopLab.Presentation.Tests (optional) for VM validation — or keep VMs covered via handler tests.

21. Acceptance Criteria
Implementation is not done when it compiles — these are measurable.

#	Criterion	How verified
AC-P1	On a clean VM with no SQL, fresh TopLab.exe first launch shows role selection with exactly two choices, no technical fields	Manual screenshot + test RoleSelectionViewModel options ==2
AC-P2	Choosing Primary installs/configures TOPLAB instance (no manual SQL steps), creates DB TopLab, runs migrations, passes schema verification	Manual on clean VM; sqlcmd -S .\TOPLAB -Q "SELECT name FROM sys.databases WHERE name='TopLab'" returns row; SELECT COUNT(*) FROM __EFMigrationsHistory == expected
AC-P3	Provisioning failures (stop service mid-migrate, kill helper) show Arabic non-technical headline + details drawer, never silently continue; retry works	Manual fault injection + verify Result.Failure path
AC-P4	Immediately after DB success, Create System Admin window appears with: red warning, Username, Password(+eye), Confirm(+eye separate), Create Admin User — no extra fields	Visual inspection + XAML element count test
AC-P5	Weak/mismatch/empty username are rejected inline; duplicate username after first admin shows Conflict	Unit validator tests + manual duplicate attempt
AC-P6	On success, dialog تم ✓ ... أغلق وشغّل من اختصار سطح المكتب... appears; next launch credentials work via normal login screen with full admin perms	Manual create → close → launch → login → assert IsAbsolute==true or all HasPermission true
AC-P7	Password is PBKDF2-hashed, not plaintext; no default/hardcoded/fallback password; hash differs per user; verify succeeds	DB: SELECT PasswordHash FROM Users shows PBKDF2$...; unit hasher tests; no constant "Admin123" anywhere (grep -i admin)
AC-P8	DB credential (Trusted_Connection or DPAPI-protected) never equals app password, never shown in UI, never logged	Log-file grep for Password= must be redacted; UI tree search shows no connection string
AC-P9	Run-once: after admin created, 10× restarts never re-show admin window; no second admin row created	SELECT COUNT(*) FROM Users ==1; SELECT IsInitialSetupCompleted FROM SetupState ==1; StartupRouterTests
AC-P10	Interrupted setup (kill mid-tx) recovers on next launch: either re-shows admin window or auto-heals to login; never leaves SetupState inconsistent	Fault-injected manual test + transaction test
AC-P11	Client fresh launch → role=Client → UDP broadcast finds LAB-PC-01 showing اتصل بـ: LAB-PC-01, no typing required; اتصال auto-verifies without Test button	2-VM manual; wire capture shows broadcast on 47777
AC-P12	Client cases: 0 hosts → message + retry; N hosts → chooser; unreachable → retry with backoff; verify logs without secrets	Manual ×3 scenarios
AC-P13	Clients never see Create System Admin window; after connect they see login, using primary-hosted accounts	Client VM manual: assert no admin window; login with primary admin succeeds
AC-P14	IsAbsolutePermission=true bypasses all perms; second user with IsAbsolute=false+all 13 grants also has full access	AuthorizationBehaviorTests matrix + manual permission matrix
AC-P15	Existing regression: CheckDatabaseConnectivity, Behaviors, AuditableEntity, F5Configuration tests all still pass	dotnet test green
AC-P16	Metrics: primary provisioning ≤5 min on clean VM (excluding SQLEXPR download), client discovery ≤7s, no secrets in logs	Time measurement in manual run
22. Owner Decisions Required
Each is blocking — implementation should not exceed recommendation without owner approval.

#	Decision	Options	Pros	Cons	Impact (User/Arch/Sec)	Complexity	Recommendation	If owner chooses otherwise
D-01	SQL engine instance name + fixed DB	A: TOPLAB (recommended) / B: SQLEXPRESS (default) / C: keep LocalDB (incompatible)	A: isolated, no collision, clear branding. B: already on many PCs, saves ~600MB download if present. C: none	A: must install new instance even if SQLEXPRESS exists. B: collides with other apps, permission trample risk. C: breaks LAN	User: A invisible. Arch: A requires constant change. Sec: same	A: medium (bootstrapper), C: broken	A: Instance=TOPLAB, DB=TopLab — change from current LocalDB	If B, plan keeps BS: code constant becomes SQLEXPRESS and provisioner first checks for existing .\SQLEXPRESS before installing
D-02	SQL Express bundling & licensing	A: Offline bundle (~600MB) shipped in package / B: Online download on demand	A: works offline in labs with no internet. B: small package	A: large installer/zip. B: fails offline, needs Microsoft download consent	User: A one-shot, B needs internet. Arch: A simpler offline flow	A higher size, B needs download UI	A for commercial product (lab internet unreliable); show Microsoft EULA checkbox	If B, provisioner shows تحميل محرك قاعدة البيانات — يحتاج إنترنت + resume/backoff
D-03	Auth mode for DB	A: Trusted_Connection=True (Windows Integrated, recommended) / B: SQL Auth toplab_app + random password	A: no password to manage, simplest. B: explicit app credential, works if Windows users vary	A: depends on Windows account having DB access (provisioner grants). B: must DPAPI-store + protect password	Sec: A least privilege via Windows group; B adds secret	A low, B medium	A — aligns with current Trusted_Connection	If B, add SA-created toplab_app login + DPAPI store + grant db_owner at migrate then downgrade to db_datareader/writer
D-04	LAN discovery pairing security	A: UDP beacon + schema verification only (open LAN) / B: A + 6-digit ephemeral pairing code shown on primary	A: zero typing, simplest. B: spoof-resistant (out-of-band confirm)	A: insider could spoof beacon but fails verification without correct schema. B: requires user to read+confirm code once per client	Sec: B higher. User: B adds one step per client	A low, B medium	A for Phase 1, schedule B as Phase-2 option if labs report spoof concerns	If B now, add PairingCode generated on primary, expiry 5 min, verified over TCP handshake
D-05	Lab shared secret for clients	A: None (open trusted LAN) / B: Lab PIN (4-6 digits) set at admin creation, required for future client joins	A simpler. B limits rogue clients	B adds PIN to remember	Sec: B higher. User: B adds PIN creation	A low, B medium	A unless lab needs client gate (prompt says no typing — respect that)	If B, add column ClientJoinPinHash to SetupState + client PIN prompt
D-06	Connection storage location & scope	A: ProgramData\TopLab\connection.json LocalMachine DPAPI (recommended) / B: Registry HKLM / C: exe-adjacent appsettings.json plaintext	A: machine-wide, survives updates, ACL friendly	B opaque, C leaks if copied	User: invisible. Sec: A via LocalMachine DPAPI	A medium	A	If C, downgrade to plaintext with ACL — must not be committed to Git
D-07	Password policy for admin	A: 8 chars, no complexity enforced (simple) / B: 8+ with letters+digits / C: no policy	A low friction for lab. B more secure	B may frustrate non-technical	User: A easiest	A low, B low	A (min 8) + live strength hint but not blocking; confirm Arabic error text with owner	If B, change CreateSystemAdminValidator to enforce regex
D-08	Username language	A: Allow Arabic+English (trimmed) / B: ASCII-only ^[\w.\-@]+$	A matches lab, B simpler	A needs regex /^[\p{L}\p{N}._\-@ ]+$/u	User: A natural	Low	A — permit Arabic usernames (lab is Arabic-first)	If B, keep ASCII validator
D-09	Client advanced manual fallback	A: Hidden collapsed “إدخال يدوي (للدعم الفني)” / B: No manual entry at all	A lets support bypass discovery if FW blocks	B pure auto but support-blocked	User: A emergency only	Low	A collapsed behind “خيارات متقدمة” disclosure	If B, support must fix beacon/FW — no workaround
D-10	Fixed SQL port vs dynamic+Browser	A: Fixed TCP 1433 for TOPLAB / B: Dynamic port + Browser UDP 1434	A clients can connect via HOST,1433 even if Browser blocked. B native but FW/noise	A requires setting TcpPort=1433 in provisioner	User: invisible. Arch: A more reliable for clients	Both low	A: fixed 1433	If B, document FW 1434 requirement
D-11	Installer vs app-only distribution	A: Phase 1 = app+SQL bundle without formal installer (zip+helper); Phase 2 = Wix/Inno installer / B: Immediately build Wix bundle	A ships faster, unblocks B-02. B more professional from day one	A less polished, needs manual shortcut/uac. B delays B-02 but cleaner	User: B smoother. Arch: A reuses elevate-helper	A low, B high	A→B staged per §16	If B now, add TopLab.Setup.wixproj to sprint and slip provisioner to installer
23. Assumptions and Verification Items
#	Assumption	Why needed	How to verify before coding	If assumption is false
A-01	No installer exists	Plan assumes app-only first-run; if a .wixproj/.iss already exists hidden, plan's installer section is redundant	Search again for *.wixproj/*.iss/*.msixmanifest at impl kickoff (plan already did — empty)	Reuse existing installer project: move provisioner to Bundle/ExePackage
A-02	Login flow is missing (no LoginCommand)	Needed to validate §10 "creds work via normal login"; search found zero login handlers	grep -R Login on branch head before D12; open tests for hidden login branch	If login exists: integrate IPasswordHasher into existing handler instead of creating new one
A-03	Lab LAN is a flat /24 (192.168.x.x), not segmented across VLANs/subnets, and broadcast is allowed	UDP broadcast assumption of §9	Ask owner: "Are labs' PCs on one switch / one subnet, typically?"	If segmented, discovery must add mDNS or directed IP list; broadcast alone fails
A-04	Primary PC will stay powered on and beacon/app running when clients need it	Beacon model (§9) assumes primary is on	Ask owner: expected power schedule	If not, promote beacon to Windows Service TopLab.Beacon auto-start (extra work)
A-05	Lab Windows is Win10/Win11 Home/Pro with UAC enabled	Provisioner elevation & FW rules assume standard Windows	Test on target VM image	If old Win7/Server, adjust sc/netsh switches
A-06	Internet not guaranteed in labs → offline SQL bundle preferred	Informs D-02 bundle decision	Ask owner: "Do labs reliably have internet during install?"	If internet reliable, use online download and slim package
A-07	SQL Express 10GB limit is sufficient; lab DB < ~2GB	Chose Express over Standard	Check current DB growth from backup samples	If growth >10GB, plan must switch to Standard/Developer (licensing change)
A-08	InternalWindowsPasswordHash column is legacy/unused and can be set to same hash or dummy hashed value	Domain UserConfiguration:17 marks it Required; setting it avoids migration altering nullability	Ask maintainer: purpose of InternalWindowsPasswordHash	If it has distinct business rule, keep separate rotation policy
A-09	No compliance claim (HIPAA etc.) is required beyond stated security checklist	Avoids over-engineering audit/compliance	Confirm with owner: any regulatory certification promised?	If compliance required, plan must add encryption-at-rest, audit log, backups
A-10	Single DB per lab (one logical database TopLab); no multi-tenant per-lab isolation needed	Fixed DB constant TopLab (§8.3)	Confirm owner: "One DB per lab, not per user?"	If multi-DB, constants become per-lab config (rare)
A-11	Host.CreateApplicationBuilder JSON provider order can be preceded by custom IConnectionStringProvider without breaking DI; AddInfrastructure(configuration) will be refactored to AddInfrastructure(IConnectionStringProvider)	Needed for §7 DPAPI precedence	Spike test: resolve connection before AddDbContext in a sandbox branch	If impossible without rewrite, instead add Configuration JSON source at runtime (builder.Configuration.AddJsonFile(ProgramDataPath, optional:true))
A-12	AuditableEntitySaveChangesInterceptor correctly handles User creation when SetupState also inserted in same transaction (no FK cycle)	§12 atomic commit	Test single-transaction mimic with InMemory	If cycle, split into two SaveChanges (user then setupState)
24. Out-of-Scope Items
Do not implement as part of this B-02 + System Admin plan. Record only — do not redesign.

B-01, B-03, B-04 redesign — explicitly preserved. If dependency found (e.g., B-04 added a secrets helper), reuse as-is, do not rewrite.
General refactoring of names, foldering, coding style, or unrelated entities.
Feature work unrelated to first-run: patient CRUD, lab modules, reporting, barcode, printing.
Documentation under Docs/ — excluded per rule; do not read or reconcile.
Unrelated UI polish beyond the exact System Admin fields (do not add themes, animations, extra settings pages).
Backup/restore, TDE, cloud sync, multi-branch replication — separate roadmap.
App updater/auto-update — preserve run-once guarantee across updates (§7) but do not build updater now.
Changing the existing IsAbsolutePermission semantics or 13-permission seeding — already correct.
Migrating InternalWindowsPasswordHash nullability — keep required; no schema break.
Any appsettings.json that should be committed to Git — intentionally not committed; do not "fix" .gitignore.
25. Risks and Mitigations
#	Risk	Likelihood	Impact	Mitigation
R-01	LocalDB contradiction realized too late — team ships LocalDB and clients fail on-site	High if ignored	Critical — product unusable multi-PC	Adopt D-01 TOPLAB Express immediately; mark as owner blocker
R-02	SQL Express install fails offline / slow / huge package	Medium	High — first-run stuck	Offer A (offline bundle) + B (online) switch per D-02; show progress + log path; retry
R-03	Beacon/FW blocks discovery — UDP filtered by antivirus/lab FW → 0 hosts	Medium	High — client appears "broken"	FW rules created elevated; fallback Probe GetDataSources + manual advanced collapse (D-09); log so support can diagnose
R-04	Elevated helper UAC denied — non-admin Windows user clicks Primary but cancels UAC	Medium	High — provisioning stalls	Detect UAC denial, show Arabic "يجب الموافقة على صلاحيات المسؤول — اطلب من مسؤول الجهاز" + retry
R-05	Race: two admins create concurrently → duplicate, or orphan SetupState	Low	Medium — confusing state	DB unique index on UserName + transactional SetupState upsert + Conflict handling (§12)
R-06	Beacon spoof — attacker on same LAN replies faster	Low in small lab	Medium	Schema handshake (§14) rejects spoof; optional pairing code D-04
R-07	Owner non-technical, won't decide D-01..D-11 → plan stalls	Medium	High	Present decisions as Arabic summary with recommendation checked; offer single meeting to sign off
R-08	Scope creep — coding agent adds extra fields / installer features not in spec	Medium	Medium	Enforce exact spec §5 window fields; CI checks grep -c "Password" shape
R-09	Regression — new startup router breaks dev flow (every F5 shows role selector)	Medium	Medium	Dev-detector: if Environment.GetEnvironmentVariable("TOPLAB_DEV_BYPASS_SETUP")=="1" skip setup; or detect dotnet run vs published exe — keep but never ship bypass
R-10	DPAPI per-user vs per-machine confusion — client user B cannot read connection created by user A	Medium	High	Use LocalMachine scope + ACL ProgramData\TopLab (not CurrentUser); test with two Windows users
R-11	Migration drift — dev keeps adding migrations before SetupState migration is pushed	Low	Medium	Coding agent must dotnet ef migrations list and ensure AddSetupState is next; CI asserts migrations applied in order
R-12	Primary offline when clients need DB	Medium	High	Document "Primary must stay on"; phase-2 Windows Service for beacon so discovery works even if WPF not running
26. Final Recommended Implementation Sequence
The one sequence a coding agent should follow after owner approval. No parallel improvisation — each step's preconditions are strict.

Phase 0 — Align (1 day, blocking)

Owner signs D-01..D-11 (§22). Record in repo TopLab_Setup_Decisions.md (outside Docs/ if allowed, or tracked as issue). Do not start D2..D17 until signed.
Phase 1 — Foundations (2 days) 2. Implement IPasswordHasher (PBKDF2) + tests + DI registration. (D2) 3. Fix DesignTimeDbContextFactory + dev appsettings.json fallback to .\TOPLAB. (D3) 4. Implement DPAPI ProgramData connection store + refactor AddInfrastructure to use it (keep fail-loud but Arabic). (D4) 5. Add SetupState entity + config + migration AddSetupState, apply to dev DB, verify snapshot. (D5)

Phase 2 — Core runtime (3 days) 6. Implement IDatabaseProvisioner (EnsureDatabase, MigrateAsync, VerifySchema) + RedactHelper. (D6) 7. Implement StartupRouter + probe order (connection → CanConnect → schema → SetupState/Users) with localized fail UX. (D7) 8. Implement RoleSelection window/VM and wire router→it. (D8)

Phase 3 — Primary & Discovery (3-4 days) 9. Implement ISqlEngineProvisioner + PrimarySetup progress VM (steps P1-P8) + elevated bootstrapper launch + FW/Browser rules. (D9) 10. Implement IPrimaryDiscoveryService (UDP broadcast) + BeaconHostedService + ClientDiscovery window/VM + N-host chooser. (D10) 11. Implement auto VerifyConnectionQuery (no button) with backoff, shared by primary & client. (D11)

Phase 4 — Admin & Login (2 days) 12. Implement CreateSystemAdmin Command/Handler/Validator + atomic SetupState commit + unit tests. (D12) 13. Implement CreateSystemAdmin Window/VM to exact spec (red warning, two eye toggles independent, Arabic). (D13) 14. If login missing, implement Login Command/VM/Window and verify admin can log in; else integrate existing login with new hasher. (D14)

Phase 5 — Hardening & Ship (2-3 days) 15. Security/log scrub sweep, DPAPI scope audit, FW rule scoping, beacon review. (D15) 16. Full test pass per §20 — primary VM clean, client VM discovery, interrupt, duplicate-admin, secrets-not-in-logs, regression suite green. (D16) 17. Package dry-run: dotnet publish -c Release, copy to isolated VM pair, run distribution without source clone, confirm ProgramData file, desktop shortcut, run-once across reboots. (D17)

Phase 6 — Installer (deferred, after B-02 accepted) 18. Create TopLab.Setup Wix/Inno bundle that chains SQL Express, places app, creates FW rules & shortcut, preserves ProgramData\TopLab and SetupState DB on upgrade. Treat as separate issue — not part of B-02 done-done.

Done-done gate (all of §21 AC-P1..AC-P16 must pass; no AC may be skipped "because it compiles").

Fact / Recommendation / Assumption / Decision legend used above
Fact: directly observed in inspected code (path:line cited).
Requirement: verbatim from this prompt's §§1-29.
Recommendation: engineering judgment of planning agent — owner may override via §22.
Assumption: §23 items needing verification before coding.
Decision-required: §22 items needing owner approval before implementation.
Plan produced in read-only planning mode against actual local HEAD 28cb2765 on branch main (clean). No files, migrations, or Git state were modified. Docs/ was not inspected per exclusion rule. ```

