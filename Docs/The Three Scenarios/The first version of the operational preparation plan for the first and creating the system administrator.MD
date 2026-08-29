# Top-Lab — B-02 Remediation, First-Run Setup & Create System Admin — Implementation Plan

> **Mode:** PLANNING ONLY. No files were created, modified, deleted, renamed, or moved. No Git, DB, package, or project changes were made. Inspection of the actual local source on `main` is the sole basis for every fact in this plan.

---

## 0. Quick-Reference Index (for later execution)

1. Executive Summary
2. Current Repository State
3. Current Implementation Inventory
4. B-02 Current-State Assessment
5. First-Run Requirements Gap Analysis
6. Database Engine and Provisioning Analysis
7. Configuration Storage Analysis
8. Primary/Server Setup Plan
9. Client LAN Discovery and Connection Plan
10. Create System Admin Plan
11. Authentication and Permission Integration
12. Setup State and Recovery Design
13. Error Handling Design
14. Security Design
15. Architecture and Layer Placement
16. Installer / Deployment Analysis
17. Dependency-Ordered Implementation Plan
18. File-Level Implementation Map
19. Database / Migration Impact
20. Testing Plan
21. Acceptance Criteria
22. Owner Decisions Required
23. Assumptions and Verification Items
24. Out-of-Scope Items
25. Risks and Mitigations
26. Final Recommended Implementation Sequence

---

## 1. Executive Summary

Top-Lab currently boots straight into `MainWindow` with `LocalDB` as the only configured SQL engine. There is **no installer**, **no first-run flow**, **no login screen**, **no password hashing**, **no `Authentication` feature folder**, **no setup-state record**, and **no LAN-discovery code**. The two "role choices" the owner wants do not exist anywhere in the code.

Concretely (all from the current source):

- `src/TopLab.Presentation/App.xaml.cs:19-34` calls `Host.CreateApplicationBuilder` and immediately shows `MainWindow` — there is no "first run" gate.
- `src/TopLab.Presentation/appsettings.json:1-5` hardcodes a `LocalDB` connection string with `Trusted_Connection=True`. This file is Git-ignored via `.gitignore:25-28` (which is correct for a per-workstation secret — it is **not** a defect; the distribution model is a build/published package, not a Git clone).
- `src/TopLab.Infrastructure/DependencyInjection.cs:26-29` *throws* if `ConnectionStrings:TopLab` is missing, so a fresh user with no `appsettings.json` cannot even start the app today. **This is the genuine B-02 problem**: the application does not bootstrap on a fresh machine.
- `src/TopLab.Infrastructure/Identity/CurrentUserService.cs:18-58` and `src/TopLab.Application/Common/Authorization/AuthorizationBehavior.cs:16-44` define an in-memory session that is "replaced at login time" — but no login exists, so the session is always empty.
- `src/TopLab.Domain/Users/User.cs:8-17` has `PasswordHash` and `InternalWindowsPasswordHash` columns but **no code anywhere** calls a hash function, an authentication command, or a password verification routine. (Verified via grep: no `BCrypt`, `PBKDF`, `argon`, `HashPassword`, `VerifyPassword`, or `System.Security.Cryptography` reference exists.)
- `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs:822-840` already seeds the 13 permission codes (`ADD_EDIT_PATIENT` … `PT_AUDIT_ACCESS`), so the permission catalog exists; the new System Admin simply needs to be created with `IsAbsolutePermission = true`.
- `src/TopLab.Infrastructure/Persistence/ApplicationDbContext.cs:35-43` uses `ApplyConfigurationsFromAssembly`, so any new entity/configuration is auto-discovered — no central switch to flip.
- No `.iss`, `.wxs`, `publish.json`, `Inno Setup`, `WiX`, or `Advanced Installer` artifact exists. **No installer is in the repository today.**

B-02 is therefore **not** a bug in some `appsettings.json` file — it is "the application is not designed to run on a machine where no one has set up a database yet." The plan resolves this with a first-run experience that:

1. presents a two-choice role selector (Primary/Server vs. Client);
2. on the primary path, provisions SQL Server, creates the `TopLab` database, runs EF Core migrations, and verifies the schema — without asking the user for technical values;
3. on the client path, discovers the primary on the LAN and stores the resulting connection;
4. then opens a Create System Admin window;
5. then closes the setup experience and presents the normal application shell;
6. gates itself with a durable, **database-backed** completion flag — not a local file.

No architectural rewrite is proposed. The plan reuses the existing `IApplicationDbContext`, MediatR pipeline, `ICurrentUserService`, `User`/`Permission`/`UserPermissionGrant` aggregates, `Result` pattern, and WPF MVVM scaffolding.

---

## 2. Current Repository State

| Item | Value (verified from local working copy) |
|---|---|
| Branch | `main` |
| Local HEAD | `28cb2765f757a12b08780f07965169a94fb13729` — "بعد إصلاح B-04" |
| Known commit per prompt | `7ca4d600a4580ac08fedf22c9e28e4b0c1c34c4a` ("بعد الإصلاحات لمجموعات F") |
| Status | `nothing to commit, working tree clean` |
| Local vs. known | **Local HEAD is ahead of the known commit** (three later commits exist: `caca365`, `56ddeec`, `a67ad54`, plus the current `28cb276`). Per instructions, **no Git state was modified.** The plan is built on the actual local state at `28cb276`. |
| .NET SDK / TFM | `net8.0` (Domain/Application/Infrastructure) and `net8.0-windows` (Presentation). Confirmed in `src/*/*.csproj`. |
| Hosting | `Microsoft.Extensions.Hosting` 8.0.1 (Central Package Management, pinned in `Directory.Packages.props:21`). |
| EF Core | 8.0.30 (`Directory.Packages.props:16-20`). Migrations: `20260828052248_BaselineDataModel`, `20260828123530_RenamePkColumns`. |
| Solution | `TopLab.sln` with `src` solution folder (4 projects) and `tests` (3 projects: Domain/Application/Infrastructure). |
| Installer / Publish artifact | **None.** Verified by `glob` for `*.iss`, `*.wxs`, `publish.json`, and grep for installer packages in csproj. |
| Secrets policy | `appsettings*.json`, `*.secrets.json`, `connectionStrings.config`, `*.config.user` are Git-ignored (`.gitignore:25-31`). |

> **No Git operations were performed during this analysis.**

---

## 3. Current Implementation Inventory

### 3.1 Solution layout (verified)

| Project | Path | Purpose |
|---|---|---|
| `TopLab.Domain` | `src/TopLab.Domain/` | Pure domain (entities, value objects, IDs, common types). |
| `TopLab.Application` | `src/TopLab.Application/` | CQRS/MediatR, `IApplicationDbContext`, `ICurrentUserService`, `Result`/`Error`, pipeline behaviors. |
| `TopLab.Infrastructure` | `src/TopLab.Infrastructure/` | EF Core (`ApplicationDbContext`), interceptor, `CurrentUserService`, `SystemDateTimeProvider`, `DesignTimeDbContextFactory`. |
| `TopLab.Presentation` | `src/TopLab.Presentation/` | WPF shell, `MainWindow`, `ShellViewModel`, `HomeViewModel`, `appsettings.json`, `IDialogService`. |

### 3.2 Cross-cutting infrastructure (already in place)

| Concern | Where | Status |
|---|---|---|
| `IApplicationDbContext` | `src/TopLab.Application/Common/Interfaces/IApplicationDbContext.cs:1-27` | Exists. Exposes `Set<T>`, `Add`, `Update`, `Remove`, `SaveChangesAsync`, `CanConnectAsync`. **No `MigrateAsync` is exposed** — this is a small but real gap. |
| `ICurrentUserService` | `src/TopLab.Application/Common/Interfaces/ICurrentUserService.cs:1-18` | Exists. `IsAuthenticated`, `UserId`, `IsAbsolutePermission`, `HasPermission(code)`. **No method to look up a user by username exists** yet. |
| `CurrentUserService` impl | `src/TopLab.Infrastructure/Identity/CurrentUserService.cs:1-58` | Exists. `SetSession` / `ClearSession` are intended to be called from "the composition root at sign-in" (comment line 39) — but no caller exists. |
| `AuthorizationBehavior` | `src/TopLab.Application/Common/Behaviors/AuthorizationBehavior.cs:1-68` | Exists. Bypasses per-permission check when `IsAbsolutePermission`. |
| `ValidationBehavior` | `src/TopLab.Application/Common/Behaviors/ValidationBehavior.cs:1-68` | Exists. FluentValidation wired. |
| `LoggingBehavior` | `src/TopLab.Application/Common/Behaviors/LoggingBehavior.cs` | Exists (per `DependencyInjection.cs:24`). |
| `Result` / `Error` | `src/TopLab.Application/Common/Results/*` | Exists. Five error categories. |
| `IDialogService` / `DialogService` | `src/TopLab.Presentation/Common/Dialogs/*` | Exists; uses `MessageBox`; `ShowSecondaryPasswordDialogAsync` is a stub (line 19-23) explicitly deferred to M17. |
| `INavigationService` / `NavigationService` | `src/TopLab.Presentation/Common/Navigation/*` | Exists, but only one navigation target is registered (`HomeViewModel`). |
| `User` aggregate | `src/TopLab.Domain/Users/User.cs:1-99` | Exists. `Create` factory, `Deactivate`, `RecordLogin`. `IsAbsolutePermission` is settable per user. |
| `Permission` aggregate | `src/TopLab.Domain/Users/Permission.cs:1-32` | Exists. |
| `UserPermissionGrant` join | `src/TopLab.Domain/Users/UserPermissionGrant.cs:1-21` | Exists. Composite PK `(UserId, PermissionId)`. **This is what makes "a regular user with all permissions" possible today** — no model change required. |
| `UserConfiguration` | `src/TopLab.Infrastructure/Persistence/Configurations/UserConfiguration.cs:1-28` | Exists. `UserName` is `UNIQUE`. `PasswordHash` is `NVARCHAR(300) NOT NULL`. |
| `CheckDatabaseConnectivityQuery` | `src/TopLab.Application/Features/AccessAndNavigation/Queries/CheckDatabaseConnectivity/*` | Exists. Returns `Result<bool>`. |
| `ShellViewModel` | `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs:1-175` | Exists. Status bar already calls `CheckDatabaseConnectivityQuery` on load (line 160-168). |
| Composition root | `src/TopLab.Presentation/App.xaml.cs:1-45` | Exists. **Throws at startup** if `ConnectionStrings:TopLab` is missing (per `DependencyInjection.cs:26-29`). |

### 3.3 What is genuinely missing

| Missing piece | Why it matters for this task |
|---|---|
| Login screen / `AuthenticateCommand` | Owner wants the created admin to "work through the normal application login screen." That screen does not exist. |
| Password hashing implementation | `User.PasswordHash` exists but is never populated by a real algorithm. |
| First-run gate | `App.OnStartup` unconditionally shows `MainWindow`. |
| Setup-state record | No DB table, no local file, no flag of any kind indicates "initial setup has been completed." |
| LAN discovery | No code; no NuGet reference; no abstraction. |
| Database provisioning | `IApplicationDbContext` has no `MigrateAsync`; no `EnsureDatabaseExistsAsync`; no abstraction for "create database if missing." |
| Configuration persistence | `appsettings.json` is read-only at startup; no write-back path exists. |
| Installer / packaging | None present. |

---

## 4. B-02 Current-State Assessment

| # | Aspect | Verdict |
|---|---|---|
| 1 | "There is a `appsettings.json` missing from Git" | **Not a defect.** The exclusion in `.gitignore:25-28` is correct for a per-workstation secret. A development `appsettings.json` is shipped with the repo at `src/TopLab.Presentation/appsettings.json` for local runs. The distribution model (a published package transferred to customers) is what governs final configuration. |
| 2 | "The application throws when the connection string is missing" | **Genuine defect.** `src/TopLab.Infrastructure/DependencyInjection.cs:26-29` throws `InvalidOperationException` on startup. A non-technical user with a fresh Windows PC cannot recover. **This is the B-02 problem.** |
| 3 | "Use a different database engine" | The current `LocalDB` connection (`DesignTimeDbContextFactory.cs:12`) **cannot accept inbound TCP connections from other machines on the LAN** (LocalDB is per-user, named-pipes only). This is incompatible with the required deployment model (primary + LAN clients). **LocalDB is unsuitable for production distribution** — this is a real architectural contradiction in the existing config, not just a B-02 artifact. The plan surfaces this as **Owner Decision D-1** (see §22). |
| 4 | "Windows Authentication / Integrated Security" | Currently used in dev only (`Trusted_Connection=True`). Owner requirement §9 forbids using Windows identity as application identity. The plan keeps SQL-level `Integrated Security` only as a **technical** option for connecting to SQL Server (engine credential), but the **application's** users are stored in the `User` table with their own hashes. These are two separate concerns. |
| 5 | "Hardcoded default admin" | None exists in the current code (verified). The plan will not introduce one. |

**Conclusion:** B-02 is real, but its root cause is broader than the prompt implies. It is the absence of a first-run bootstrap, combined with a `LocalDB` default that cannot serve LAN clients. The plan addresses both.

---

## 5. First-Run Requirements Gap Analysis

| # | Requirement | Current state | Required change |
|---|---|---|---|
| 1 | Two-choice role selection | Absent. | Add `FirstRunWindow.xaml` (role selector) presented by `App.OnStartup` instead of `MainWindow` when setup is incomplete. |
| 2 | Primary path auto-provisions DB | Absent. | Add `IDatabaseProvisioner` (Infrastructure) that creates the database, runs `MigrateAsync`, and verifies schema. Triggered by `RunDatabaseProvisioningCommand`. |
| 3 | Client path auto-discovers primary | Absent. | Add `IPrimaryComputerDiscovery` (Infrastructure) with a UDP broadcast strategy (see §9). |
| 4 | Auto connection verification | Partial — `CheckDatabaseConnectivityQuery` exists (`src/TopLab.Application/Features/AccessAndNavigation/Queries/CheckDatabaseConnectivity/...`) but is bound to a connection string that does not exist yet on a fresh machine. | Reuse the query once a real `IApplicationDbContext` can be built. Add a `RunConnectivityCheckCommand` for client path. |
| 5 | Create System Admin window | Absent. | Add `CreateSystemAdminWindow.xaml` + `CreateSystemAdminViewModel` + `CreateSystemAdminCommand` + `CreateSystemAdminCommandHandler`. |
| 6 | Admin creation uses existing auth infra | Auth infra is incomplete (no hashing, no commands). | Add `IPasswordHasher` (Infrastructure) using `Microsoft.AspNetCore.Identity.PasswordHasher` or PBKDF2 from `Microsoft.AspNetCore.Cryptography.KeyDerivation`. **Decision D-3.** Add `CreateSystemAdminCommand` writing through `IApplicationDbContext`. |
| 7 | Run-once guarantee | No state record of any kind. | Add `SetupState` entity + EF migration; query is the source of truth. |
| 8 | Client machines skip Create Admin | No branching exists. | `App.OnStartup` selects window based on `role.json` (workstation-local) **and** DB `SetupState`. |
| 9 | App-level auth independent of Windows | Already correct in design intent (no Windows-identity code exists), but no actual login exists. | Build login on the `User` table; never bind to `WindowsIdentity`. |
| 10 | Admin can grant full perms to any user | Already supported by `UserPermissionGrant` join — no model change. | Only the *creation* command needs to be added. |
| 11 | No hardcoded / fallback passwords | None today; plan keeps that. | `CreateSystemAdminCommand` validates `password` is non-empty and rejects any code that supplies a default. |
| 12 | DB credential ≠ user credential | Engine and app creds are conceptually separate today; concrete separation must be enforced. | Use Windows Integrated Security **for SQL Server** (engine auth), and a separate `User` table (app auth). Credentials are never cross-used. |
| 13 | Config storage | Dev reads `appsettings.json`; nothing writes it. | Add `IWorkstationConfigurationStore` (Infrastructure) that reads/writes a **non-ignored** `toplab.config.json` in `%LocalAppData%\TopLab\`. This file contains the connection string **and the role** (Primary/Client). It is *not* `appsettings.json` and is *not* secrets-bearing beyond the connection string. See §7. |
| 14 | Non-technical UI configuration | `MainWindow` is the only window today. | Add a small `FirstRunWindow` (role selector) that drives the rest. |
| 15 | Error handling coverage | None of the listed failure modes have a defined UX. | §13 enumerates per-failure UX. |
| 16 | Setup state & recovery | No record. | `SetupState` table (idempotent write on each step), plus transactional handling in the provisioning handler. |
| 17 | Security in a medical environment | `User.PasswordHash` exists but is unused; no secrets-management code exists. | §14 enumerates the controls; `IAppLogger` is upgraded to scrub secrets (see §14 / §18). |
| 18 | Architectural fit | Plan reuses every existing abstraction; no new patterns introduced. | See §15. |
| 19 | Installer reality | None. | §16 calls this out and does **not** include installer work in this task. |
| 20 | Testing | Tests exist for `CheckDatabaseConnectivityQueryHandler` and behaviors. | §20 enumerates new tests. |
| 21 | Dependency ordering | n/a | §17 / §26 sequence. |
| 22 | Owner decisions | n/a | §22 list. |
| 23 | Assumptions | n/a | §23 list. |
| 24 | Out of scope | n/a | §24 list. |

---

## 6. Database Engine and Provisioning Analysis

> **Genuine architectural contradiction identified — must be resolved by the owner before implementation begins.**

### 6.1 LocalDB (current default, `DesignTimeDbContextFactory.cs:12`)

| Property | Value |
|---|---|
| TCP reachable from other machines | **No** — LocalDB uses named pipes, runs under the user's Windows account, single-machine only. |
| Suitable for primary + LAN clients | **No.** |
| Suitable for developer machines | **Yes.** |
| Auto-installable without admin | **No** (LocalDB is a Visual Studio / SQL Server Express workload). |
| Commercial-distribution friendly | **Acceptable for dev only.** |

**Verdict:** LocalDB is fine for development but **must not** be the production engine that backs the primary computer in a customer's lab. Keeping it as the dev default is acceptable if the production primary uses SQL Server Express (or higher).

### 6.2 SQL Server Express (recommended for production primary)

| Property | Value |
|---|---|
| TCP reachable from other machines | **Yes** (when `SQL Server Browser` and `TCP/IP` protocol are enabled). |
| Suitable for primary + LAN clients | **Yes** — well-established pattern for small multi-PC deployments. |
| Max DB size | 10 GB per database (sufficient for a typical lab). |
| Free for commercial use | **Yes** (Express edition is free, with the standard EULA). |
| Auto-installable from WPF setup | **Partially.** Bundling the Express installer is a **separate** task (out of scope here; see §16). For this task, the plan assumes SQL Server Express is **already installed** on the primary computer and provides a "verify and configure" flow rather than a "download and install" flow. |
| Network configuration required | Yes — `TCP/IP` enabled, `SQL Server Browser` running, port `1433` (or named instance port) open in Windows Firewall. |
| Service account | Default `NT Service\MSSQL$SQLEXPRESS` is acceptable; the application does not need to know it. |

### 6.3 Recommended engine matrix

| Role | Engine | Why |
|---|---|---|
| Developer workstation (dev) | LocalDB | No install; matches current `appsettings.json` and `DesignTimeDbContextFactory`. |
| Production primary computer (customer site) | **SQL Server Express** (default instance or named `TOPLAB`) | Free, redistributable under Express EULA, network-reachable. |
| Production client computer | **No engine.** Connects over TCP to the primary. |

### 6.4 Provisioning operations the application must perform on the primary

| Step | Action | Where it lives |
|---|---|---|
| 1 | Verify SQL Server is reachable on `localhost\TOPLAB` (or whatever instance is fixed at build time — see D-2). | `IDatabaseProvisioner.EnsureReachableAsync` |
| 2 | Open a master-DB connection and `CREATE DATABASE [TopLab]` if missing. | `IDatabaseProvisioner.EnsureDatabaseExistsAsync` |
| 3 | Run EF Core migrations against the new database. | `IApplicationDbContext` gets a `MigrateAsync` (extension) and `IDatabaseProvisioner.ApplyMigrationsAsync` calls it. |
| 4 | Verify schema by querying `sys.tables` for a known sentinel table (e.g. `Users`). | `IDatabaseProvisioner.VerifySchemaAsync` |
| 5 | Write `SetupState` row `{ Id=1, IsPrimarySetupComplete=true, CompletedAtUtc=now, SchemaVersion=... }` inside the same `SaveChanges` as the migration verification. | New `SetupState` entity + handler |

### 6.5 Distribution model — what the install package must contain

> **Out of scope for this task** but the plan acknowledges it.

| Component | Where it lives today | Where it must live at distribution time |
|---|---|---|
| `TopLab.exe` | `bin/` after `dotnet publish` | Installer output, `%ProgramFiles%\TopLab\` |
| `toplab.config.json` (workstation-local) | Does not exist | `%LocalAppData%\TopLab\toplab.config.json` (created by setup) |
| `appsettings.json` | Source-controlled for dev only | Not needed at customer sites if `toplab.config.json` exists; can be omitted from installer output. |
| SQL Server Express | Not in repo | Must be bundled or pre-installed by the lab's IT; **not** silently downloaded. |

---

## 7. Configuration Storage Analysis

### 7.1 Current configuration model

| Source | Lifetime | Scope | Secrets? | Git-controlled? |
|---|---|---|---|---|
| `src/TopLab.Presentation/appsettings.json` | Dev only | Developer workstation | Yes (LocalDB conn string) | **Yes** (with the secrets line being non-secret). |
| `%LocalAppData%\TopLab\toplab.config.json` | **Does not exist** | Customer workstation | Yes (SQL conn string) | No. |

### 7.2 Proposed `toplab.config.json` schema

```jsonc
{
  "Role": "Primary",                 // or "Client"
  "ConnectionStrings": {
    "TopLab": "Server=localhost\\TOPLAB;Database=TopLab;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "PrimaryEndpoint": null,            // populated on Client machines after discovery
  "SchemaVersion": "0.0.0"            // written by SetupState on completion
}
Decision	Why
New file, not a write-back to appsettings.json	appsettings.json lives next to the EXE in %ProgramFiles% and is not user-writable without elevation. %LocalAppData% is.
JSON, not registry	JSON is human-debuggable by the owner, easy to back up, easy to clear on reinstall.
Role is persisted	Lets the app know at next launch whether to show First-Run or login.
No plaintext passwords	No application password is ever persisted. The User table holds only the hash. The SQL Server engine credential is the OS-managed Integrated Security (or a SQL login if D-1 deviates — see §22).
7.3 IWorkstationConfigurationStore contract (proposed — for planning only)
Method	Purpose
Task<WorkstationConfiguration> LoadAsync(CancellationToken)	Read toplab.config.json from %LocalAppData%\TopLab\.
Task SaveAsync(WorkstationConfiguration, CancellationToken)	Atomic write (write-temp + replace) to the same path.
string FilePath { get; }	Exposed for diagnostics.
Layer: Infrastructure. Implementation: FileSystemWorkstationConfigurationStore. Imported into IConfiguration via a custom ConfigurationProvider so the existing IConfiguration consumers (e.g. DependencyInjection.cs:26) keep working without modification.

7.4 What is not changed by this plan
.gitignore is not modified.
appsettings.json is not deleted from the repository; it remains the developer's local default.
No plaintext password is ever written to any file by the application.
8. Primary/Server Setup Plan
8.1 Flow (UX)
[App.OnStartup]
  └─ if SetupState row missing OR not completed:
       show FirstRunWindow (role selector)
       ├─ user picks "Primary/Server"
       │    └─ navigate to ServerSetupWindow
       │         1. status: "جاري التحقق من خادم قاعدة البيانات…"
       │         2. status: "جاري إنشاء قاعدة البيانات…"
       │         3. status: "جاري تثبيت بنية قاعدة البيانات…"
       │         4. status: "تم بنجاح"
       │         5. open CreateSystemAdminWindow
       │              └─ on success: write SetupState, close setup, show MainWindow
       │         6. on failure: show non-technical error, keep window open, allow retry
       └─ user picks "Client"
            └─ navigate to ClientSetupWindow (see §9)
  └─ else:
       if role=Primary: show MainWindow
       if role=Client: show MainWindow (login path)
8.2 Components added (planning only — not created now)
Component	Layer	Purpose
FirstRunWindow / FirstRunViewModel	Presentation	Two large buttons only. No input fields.
ServerSetupWindow / ServerSetupViewModel	Presentation	Status text + cancel + retry. No input fields.
RunServerSetupCommand / Handler	Application	Orchestrates the four provisioning steps.
IDatabaseProvisioner / SqlServerDatabaseProvisioner	Infrastructure (Application port)	Engine reachability, DB creation, migrations, schema verification.
ISetupStateRepository (or use IApplicationDbContext)	Application/Infrastructure	Reads/writes the SetupState row.
SetupState aggregate	Domain	{ Id, IsPrimarySetupComplete, CompletedAtUtc, SchemaVersion, PrimaryMachineName }.
SetupStateConfiguration	Infrastructure	EF mapping.
8.3 What the user sees vs. what is logged
User sees (Arabic)	Internally logged (English, structured)
"جاري التحقق من خادم قاعدة البيانات…"	ServerSetup: Engine probe → endpoint=…
"جاري إنشاء قاعدة البيانات…"	ServerSetup: CreateDatabase → exists=…
"جاري تثبيت بنية قاعدة البيانات…"	ServerSetup: Migrate → pending=…, applied=…
"تم بنجاح"	ServerSetup: Complete → schema=…, version=…
"تعذّر إكمال الإعداد. حاول مرة أخرى."	ServerSetup: Failed → { stage, exceptionType, message, innerStack } — passwords and connection strings are scrubbed.
8.4 Idempotency
The provisioning command must be safe to re-run after a crash: every step first checks state, then acts.
Migrations are inherently idempotent at the EF Core level.
SetupState is written only after all four steps succeed. A partial run leaves no SetupState row, so the next launch re-enters FirstRunWindow.
9. Client LAN Discovery and Connection Plan
9.1 Options considered
Option	Reliability	Security	Complexity	Firewall impact	Windows compatibility	Recommendation
UDP broadcast (recommended)	High on a flat LAN; degrades across VLANs.	Acceptable if discovery payload is a nonce + minimal metadata; no secrets.	Low.	One outbound UDP packet, one inbound response on a known port — usually allowed.	Excellent.	Recommended.
mDNS / DNS-SD	High, but Windows does not advertise services by default; requires mDNS responder.	Good.	Medium (extra package + Windows service config).	mDNS uses multicast 224.0.0.251 over UDP 5353 — often blocked.	Requires Apple Bonjour or built-in Windows mDNS.	Rejected: extra dependency + extra firewall changes for non-technical users.
Windows network browsing (Computer Browser / SMB)	Unreliable; SMBv1 disabled by default in modern Windows.	Poor.	Medium.	SMB ports.	Mixed.	Rejected.
Static primary entered by user	Most reliable.	N/A.	Trivial.	None.	N/A.	Rejected by owner requirement §3 (no IP/host typing).
Application-level discovery protocol over TCP	High.	Good (TLS optional).	Medium-high.	One inbound port on primary.	Excellent.	Plausible alternative; rejected for this iteration because UDP is simpler and matches the lab-LAN assumption.
9.2 Recommended: UDP broadcast with token reply
Field	Value
Discovery port (configurable, default 47800)	UDP.
Beacon interval (primary side)	Every 2 s while ServerSetupWindow or MainWindow is in primary mode.
Discovery request (client)	Broadcast "TOPLAB_DISCOVERY_v1" + 8-byte nonce.
Primary response	Unicast back to sender with { MachineName, SqlInstance, DatabaseName, Nonce }.
Timeout	3 s; up to 3 attempts; collect all responses.
Selection rule	If exactly one primary responds, auto-select it. If multiple, present a list (D-4 — owner decision in §22). If none, show "لم يتم العثور على الخادم الرئيسي. تأكد أن الجهاز الرئيسي يعمل.".
9.3 Components
Component	Layer	Purpose
IPrimaryComputerDiscovery	Application port	Task<IReadOnlyList<DiscoveredPrimary>> DiscoverAsync(TimeSpan timeout, CancellationToken).
UdpPrimaryComputerDiscovery	Infrastructure	UDP broadcast implementation.
IPrimaryComputerBeacon	Application port	Task StartAsync, Task StopAsync. Started by ServerSetupViewModel after successful provisioning.
UdpPrimaryComputerBeacon	Infrastructure	Replies to discovery requests; throttles by nonce to prevent floods.
ClientSetupWindow / ClientSetupViewModel	Presentation	Status text; "Connect to: " confirmation; no input fields.
RunClientSetupCommand	Application	Discovery → confirm → save toplab.config.json with Role=Client and PrimaryEndpoint → smoke-test via CheckDatabaseConnectivityQuery.
9.4 What the client never types
No IP, no hostname, no SQL instance, no database name, no port — all are derived from the primary's response and the application-level constants in §7.2.
10. Create System Admin Plan
10.1 Window contract (matches owner requirement §5 exactly)
Element	Spec
Top banner	Red warning text: "سجّل اسم المستخدم وكلمة المرور واحتفظ بهما في مكان آمن. لن يستطيع التطبيق استرجاعهما لاحقًا."
Username input	TextBox bound to Username (no minimum length, no email — keep it simple; trim and uniqueness enforced server-side).
Password input	PasswordBox (WPF), masked by default. Independent show/hide eye toggle.
Confirm Password input	PasswordBox, masked by default. Independent show/hide eye toggle.
Create Admin User button	Button bound to AsyncRelayCommand. Disabled until both passwords are non-empty and match.
Footer	Optional inline message area for validation feedback. No technical detail.
10.2 Required fields — and what is not added
Field	Required?	Reason
Username	Yes	Login identity.
Password	Yes	App-level credential.
Confirm Password	Yes	Owner requirement.
Database-engine credential	No	Forbidden by owner requirement §12.
Permission selector	No	First admin gets IsAbsolutePermission = true — the "admin" classification is implicit, and AuthorizationBehavior.cs:33-40 already bypasses all per-permission checks for such users. The plan does not add a permission-grant UI in this window; an admin can grant permissions to other users through the (future) Users management screen.
10.3 Command flow
Step	Actor	Outcome
1	User clicks Create	CreateSystemAdminCommand is dispatched via MediatR.
2	ValidationBehavior	CreateSystemAdminCommandValidator (FluentValidation) — username not empty, password ≥ 8 chars, password == confirm.
3	AuthorizationBehavior	Bypassed because the first-run window is pre-authentication. The handler itself enforces "no users exist yet" (see §11).
4	Handler	Hashes the password (PBKDF2 / PasswordHasher — D-3), creates the User aggregate (User.Create(..., isAbsolutePermission: true, ...)), persists via IApplicationDbContext.Add + SaveChangesAsync.
5	Handler	Writes SetupState row IsPrimarySetupComplete=true (idempotent — see §12).
6	UI	On Result.Success, show success message + "You may now close this window and launch the application."
7	App	Closes setup windows, shows MainWindow (which today has no login screen — see §11.1 risk).
10.4 New components
Component	Layer
CreateSystemAdminWindow.xaml(.cs)	Presentation
CreateSystemAdminViewModel	Presentation
CreateSystemAdminCommand / Handler / Validator	Application (Features/Setup/Commands/CreateSystemAdmin)
IPasswordHasher / Pbkdf2PasswordHasher (or AspNetIdentityPasswordHasher)	Application port + Infrastructure impl
11. Authentication and Permission Integration
11.1 Critical observation: there is no login screen today
MainWindow is shown unconditionally from App.OnStartup (line 30-31) and the status bar's database check is the only "auth" surface. The owner requirement §6 says "the created credentials must then work through the normal application login screen." That screen does not exist.

This is a dependency, not a redesign. The Create System Admin command produces a user that will be usable by the login flow that the team will build. The current plan does not require implementing that login flow as part of B-02 — it requires:

The admin is created correctly.
The data is in place so the future login flow can use it.
The first-launch UX ends in MainWindow, which is consistent with today's behavior (so we do not regress or open B-01 / B-03 / B-04).
Owner decision D-5 (see §22): confirm that the login screen is intentionally deferred to a later milestone, and that the first admin "works" in the sense of being usable by that future login screen rather than by today's shell.

11.2 Permission model fit
Owner requirement	Current model	Fit
"First admin has full System Admin permissions"	User.IsAbsolutePermission = true already bypasses all per-permission checks (AuthorizationBehavior.cs:33-40).	Already met. No code change required for this point.
"An administrator can create additional accounts"	User aggregate has Create; IApplicationDbContext exposes Add/SaveChangesAsync.	Already met in the sense of "no schema blocker." A future Users-management UI is out of scope for this task.
"A non-admin user can be granted the complete set of permissions"	UserPermissionGrant join table already allows any (UserId, PermissionId) pairing.	Already met. No model change.
11.3 Password handling (owner requirement §11)
Concern	Approach
No hardcoded default	CreateSystemAdminCommandHandler throws Error.Validation if password is null/whitespace. There is no else branch.
No fallback	No code path produces a default.
No plaintext storage	IPasswordHasher.Hash(password) is called once; the hash is what is persisted. User.PasswordHash is NVARCHAR(300) (current schema) — fits PBKDF2 / ASP.NET Identity PasswordHasher output.
No example password in source	None will be added.
Password algorithm — D-3: two reasonable choices:

Option	Package	Pros	Cons
Microsoft.AspNetCore.Identity.PasswordHasher<TUser> (v1-style)	Microsoft.Extensions.Identity.Password 8.0.x	Battle-tested, salt + iteration built in, output fits in 300 chars.	Adds a small package.
Microsoft.AspNetCore.Cryptography.KeyDerivation (PBKDF2)	Same package, lower-level	No Identity types leak.	Slightly more code.
Recommendation: PasswordHasher (Identity) — minimum code, widely understood output format, already in the Microsoft.Extensions.* family already used here.

11.4 What the plan does not implement
Login screen (out of scope; flagged as dependency).
Password change / reset.
Password policy beyond a minimum length (D-6 in §22).
Users-management UI.
12. Setup State and Recovery Design
12.1 Database-backed SetupState (single source of truth)
Field	Type	Notes
Id	int	Fixed value 1. Only one row ever exists.
IsPrimarySetupComplete	bool	True after successful provisioning + admin creation.
PrimaryMachineName	string?	Set on primary.
CompletedAtUtc	DateTime?	Set on primary.
SchemaVersion	string?	Set on primary.
LastClientConnectedAtUtc	DateTime?	Optional telemetry (D-7).
Why DB-backed and not a local file: an interruption (power loss, crash) cannot produce a "phantom" completion flag, because the row is written inside the same EF Core transaction as the admin creation. A local file could be deleted by a user, written but not flushed, or out of sync with the DB.

12.2 Recovery matrix
Scenario	State at next launch	UX
Power loss after migration, before admin created	SetupState row absent	FirstRunWindow re-appears. Provisioning handler detects DB exists + migrations applied → skips steps 1-3, only does admin creation.
App crash during admin creation	SetupState row absent	Same as above.
Admin created, but SetupState write failed (e.g. network blip)	User table has the admin, SetupState row absent	Same as above; admin creation's "no users exist" check would now see one user. Mitigation: the handler is the unique point of admin creation, and the SetupState write is in the same SaveChanges as the User.Add. A second launch will see one user and refuse to re-create (see below).
Admin already exists, no SetupState row	One user in DB	FirstRunWindow is suppressed; the app shows MainWindow (login screen when it exists). No duplicate admin can be created.
Power loss after SetupState written, before MainWindow shown	SetupState row present	Next launch skips first-run entirely.
Client disconnects mid-toplab.config.json save	Either old file or new file	Atomic write: write to toplab.config.json.tmp, then File.Replace. No half-file.
Primary machine replaced	Client must re-discover	The current toplab.config.json is overwritten by the next client setup. No special recovery needed.
12.3 Duplicate-admin prevention
Condition	Behavior
Users table is empty AND SetupState.IsPrimarySetupComplete == false	Show CreateSystemAdminWindow.
Users table is non-empty AND SetupState.IsPrimarySetupComplete == false	Defensive: mark SetupState complete (best-effort) and skip first-run. This is the "SetupState missing but admin already exists" recovery row above.
SetupState.IsPrimarySetupComplete == true	Skip first-run entirely.
The CreateSystemAdminCommandHandler is the only place that creates the first admin and refuses to run if a user with IsAbsolutePermission == true already exists — even if the user invoked the window manually. This is the "no duplicate" guarantee.

13. Error Handling Design
All UX messages are Arabic-first, non-technical. All log messages are English, structured, and never include passwords or connection strings.

#	Failure	User sees	Internal action	Retry?
1	SQL Server not installed / not running	"تعذّر الاتصال بخادم قاعدة البيانات. تأكد من تثبيت SQL Server على هذا الجهاز ثم حاول مرة أخرى."	Log: ServerSetup.EngineProbe failed: { type, message }.	Yes.
2	SQL Server service stopped	"خدمة قاعدة البيانات متوقفة. جاري محاولة التشغيل…" — if cannot start: "تعذّر تشغيل خدمة قاعدة البيانات. تواصل مع الدعم."	Log + try net start MSSQL$TOPLAB once with admin.	Yes (manual retry).
3	CREATE DATABASE failed (permissions, disk)	"تعذّر إنشاء قاعدة البيانات. حاول مرة أخرى."	Log: ServerSetup.CreateDatabase failed: { … }.	Yes.
4	Migration failed	"تعذّر تثبيت بنية قاعدة البيانات. حاول مرة أخرى."	Log: ServerSetup.Migrate failed: { migration, exception }.	Yes.
5	Schema verification failed (expected table missing)	"اكتمل التثبيت جزئيًا. حاول مرة أخرى."	Log: ServerSetup.VerifySchema failed: { missing=[...] }.	Yes — and handler re-runs migrations.
6	LAN discovery timeout	"لم يتم العثور على الخادم الرئيسي. تأكد أن الجهاز الرئيسي يعمل وأنكما على نفس الشبكة."	Log: ClientSetup.Discovery timeout.	Yes, after a "Retry" button.
7	No primary found	Same as #6.	Same.	Yes.
8	Multiple primaries found	"تم العثور على أكثر من جهاز رئيسي. اختر الجهاز المطلوب." — list of machine names.	Log: ClientSetup.Discovery multipleResults count=….	Yes.
9	Primary found but unreachable after selection	"تعذّر الاتصال بالخادم الرئيسي المختار."	Log: ClientSetup.Connectivity failed.	Yes.
10	Login (future) failed	n/a (out of scope).	n/a.	n/a
11	Invalid username (empty / whitespace / non-unique)	"اسم المستخدم مطلوب" / "اسم المستخدم مستخدم من قبل."	Validation pipeline error.	Inline.
12	Weak password	"كلمة المرور يجب ألا تقل عن 8 أحرف." (length is the only rule unless D-6 adds complexity).	Validation pipeline error.	Inline.
13	Password confirmation mismatch	"كلمتا المرور غير متطابقتين."	Validation pipeline error.	Inline.
14	Duplicate admin attempt	"تم إنشاء مدير النظام مسبقًا."	Conflict error; logged at Setup.CreateAdmin conflict.	No — first-run window closes.
15	Setup interrupted (window closed, power loss)	Next launch: state machine recovers per §12.2.	State machine.	Automatic.
16	App restart during setup	Same as #15.	Same.	Automatic.
13.1 Logging guarantees
IAppLogger (src/TopLab.Application/Common/Interfaces/IAppLogger.cs:1-10) is upgraded — interface unchanged, behavior expanded in WpfAppLogger (src/TopLab.Presentation/DependencyInjection.cs:31-37) to a sink that scrubs:
the substring "Password=" and anything after it on the same line;
any field named password in structured payloads.
This is a small change to the existing WpfAppLogger only — no Application-layer signature change.
14. Security Design
#	Risk	Mitigation
1	DB engine credential exposure	Plan uses Integrated Security to SQL Server. The SQL credential is the OS user — never typed into the app, never logged. If D-1 deviates to a SQL login, the same toplab.config.json storage rules apply and the secret is not displayed in any UI.
2	Plaintext password storage	User.PasswordHash is populated by IPasswordHasher.Hash. Plaintext is never stored, never logged, never written to disk.
3	LAN discovery spoofing	UDP discovery reply is only useful to a client; it contains no secrets. A nonce prevents trivial replay. A malicious actor on the LAN could point a client at a wrong server — D-8.
4	Unauthorised clients attempting discovery/connection	Application-level auth (the User table) is the boundary; SQL-level auth (Windows account) does not grant application access. There is no anonymous "browse data" path.
5	Firewall exposure	UDP 47800 is a non-privileged port; SQL 1433 is opened only on the primary by the bundled SQL Server Express installer, not by this application.
6	SQL Server network exposure	The plan instructs the bundled Express installer to enable TCP/IP for the named instance, but does not open SQL to the public internet. The customer controls Windows Firewall.
7	Least privilege	The SQL login used by the application is a member of db_owner for the TopLab database only (recommended Express post-install step — not enforced by this plan; out of scope). The plan documents this in the install guide (out of scope).
8	Accidental credential disclosure in logs	WpfAppLogger scrubs per §13.1. The connection string is logged only by category (Server=***), not the full value.
9	Brute-force on the (future) login screen	Out of scope for B-02, but a hook point is preserved: the future AuthenticateCommand can rate-limit; nothing in this plan prevents that.
10	Regulatory posture	The plan does not claim HIPAA / GDPR / ISO compliance. Compliance depends on deployment practices (encrypted disk, audit logs, access policy) that are out of scope.
15. Architecture and Layer Placement
Proposed component	Layer	Reason
FirstRunWindow, ServerSetupWindow, ClientSetupWindow, CreateSystemAdminWindow	Presentation	WPF views.
FirstRunViewModel, ServerSetupViewModel, ClientSetupViewModel, CreateSystemAdminViewModel	Presentation	Coordinate view state and dispatch commands.
CreateSystemAdminCommand, RunServerSetupCommand, RunClientSetupCommand	Application	MediatR requests.
CreateSystemAdminCommandHandler, RunServerSetupCommandHandler, RunClientSetupCommandHandler	Application	Business orchestration.
FluentValidation validators	Application	Reuses existing pipeline.
IPasswordHasher	Application (port)	Domain-of-the-app concern.
IWorkstationConfigurationStore, IDatabaseProvisioner, IPrimaryComputerDiscovery, IPrimaryComputerBeacon	Application (ports)	All swappable.
FileSystemWorkstationConfigurationStore, SqlServerDatabaseProvisioner, UdpPrimaryComputerDiscovery, UdpPrimaryComputerBeacon, Pbkdf2PasswordHasher (or Identity PasswordHasher)	Infrastructure	Concrete implementations.
SetupState aggregate + SetupStateConfiguration	Domain / Infrastructure	First-class entity; one row.
AuditableEntitySaveChangesInterceptor (already present)	Infrastructure	Already writes CreatedAtUtc/CreatedById. Will populate CreatedById=0 (system) for the first admin — D-9.
No second architecture is introduced. No new patterns, no new frameworks beyond one NuGet package (password hashing — D-3).

16. Installer / Deployment Analysis
Question	Answer (verified)
Does an installer exist in the repo?	No. Searched for *.iss, *.wxs, publish.json, Inno, WiX, AdvancedInstaller. Nothing.
What does csproj say about publishing?	Nothing. The Presentation project has OutputType=WinExe, UseWPF=true, and no PublishProfile files.
What must eventually exist for commercial distribution?	(a) a real installer (WiX, Inno Setup, or Advanced Installer) that bundles TopLab.exe + SQL Server Express; (b) a per-customer installer that writes %LocalAppData%\TopLab\toplab.config.json; (c) a service-installation step (optional) for the primary beacon.
Is installer work in scope for this task?	No. Per owner instruction §19, this is a planning task only; the plan does not implement installer work and does not propose adding an installer as a code change in this iteration.
What is recommended as the next, separate deliverable?	A dedicated "Installer" workstream that picks one of WiX / Inno / Advanced Installer and produces a build that bundles the SQL Server Express redistributable. This is out of scope for B-02.
16.1 Implication for the prompt's "first-run experience"
Because there is no installer, the role-selector window must live inside the WPF application itself (not in an installer wizard). This is what the plan does. If/when an installer is added later, it can either pre-populate the role (D-10) or simply launch the WPF app and let App.OnStartup show the role selector — both options remain valid.

17. Dependency-Ordered Implementation Plan
Strictly ordered. Each phase lists prerequisites from earlier phases. The "verify" steps reuse the existing test infrastructure.

Phase 0 — Pre-flight (no code yet; prerequisites)
#	Item	Status
0.1	Owner decisions D-1 through D-10 collected (§22).	Required before Phase 1.
0.2	SQL Server Express confirmed available on a primary-class dev machine for integration testing.	Required before Phase 4.
0.3	Two physical or VM machines available for LAN-discovery test (Phase 6).	Required before Phase 6.
Phase 1 — Configuration store (no behavior change)
#	Item	Dependencies	Verify
1.1	Add WorkstationConfiguration record + IWorkstationConfigurationStore in Application.	0.1	Unit: load missing file → default.
1.2	Add FileSystemWorkstationConfigurationStore in Infrastructure; register in DependencyInjection.AddInfrastructure.	1.1	Unit: round-trip write+read.
1.3	Add a custom ConfigurationProvider that layers toplab.config.json over appsettings.json.	1.2	Existing CheckDatabaseConnectivityQuery test still passes.
1.4	DependencyInjection.AddInfrastructure no longer throws when ConnectionStrings:TopLab is missing — it now throws only after the first-run flow fails. Behavior change: today it throws at startup. After this phase, if toplab.config.json is missing, the app still throws — but the new App.OnStartup will catch that and route to FirstRunWindow. (See Phase 2.)	1.3	Manual: run with no appsettings.json and no toplab.config.json → app should still launch into First-Run (this requires Phase 2 first; otherwise the throw is uncaught).
Phase 2 — First-run gate (UI only, no DB change yet)
#	Item	Dependencies	Verify
2.1	Add SetupState entity + SetupStateConfiguration + migration AddSetupState.	1.1	Migration applies on existing dev DB.
2.2	Add GetSetupStateQuery / Handler (Application).	2.1	Unit.
2.3	Add FirstRunWindow + FirstRunViewModel (two buttons, no inputs).	—	Visual.
2.4	Modify App.OnStartup: query SetupState; if missing or IsPrimarySetupComplete=false, show FirstRunWindow; else show MainWindow.	2.2, 2.3	Manual: existing dev DB (no SetupState) → FirstRun.
2.5	Add FirstRunWindow registrations to AddPresentation.	2.3	DI resolution check.
Phase 3 — Database provisioner
#	Item	Dependencies	Verify
3.1	Extend IApplicationDbContext with MigrateAsync(CancellationToken) (Application port — not EF types leaked in interface, but a method that takes the EF context is unavoidable; per existing pattern in IApplicationDbContext.cs:26 it forwards to base.Database.MigrateAsync; for testability we wrap in a try/catch in the Infrastructure impl).	0.2	Unit (InMemory path).
3.2	Add IDatabaseProvisioner port (Application) with EnsureReachableAsync, EnsureDatabaseExistsAsync, ApplyMigrationsAsync, VerifySchemaAsync.	3.1	—
3.3	Add SqlServerDatabaseProvisioner (Infrastructure) using Microsoft.Data.SqlClient for the master-DB connection (engine auth) and the EF context for migrations.	3.2	Integration: local SQL Express.
3.4	Add RunServerSetupCommand / Handler orchestrating the four steps + writing SetupState.	3.3, 2.2	Integration test against a real Express instance.
3.5	Add ServerSetupWindow / ServerSetupViewModel (status text + cancel + retry).	3.4	Manual.
Phase 4 — Password hashing + Create System Admin
#	Item	Dependencies	Verify
4.1	Add IPasswordHasher (Application) with Hash(string) → string and Verify(string hash, string password) → bool.	—	Unit.
4.2	Add AspNetIdentityPasswordHasher (Infrastructure) using Microsoft.Extensions.Identity.Password 8.0.x.	4.1	Unit.
4.3	Add CreateSystemAdminCommand / Handler / Validator. Validator enforces: username not empty; password ≥ 8 chars (D-6); password == confirm; no user with IsAbsolutePermission exists. Handler creates the User (IsAbsolutePermission=true, hash) and writes SetupState.IsPrimarySetupComplete=true in the same SaveChanges.	4.2, 2.1	Unit (handler with InMemory).
4.4	Add CreateSystemAdminWindow / ViewModel (banner + Username + Password + Confirm + independent eye toggles + button).	4.3	Visual.
4.5	Wire ServerSetupWindow to navigate to CreateSystemAdminWindow on success.	3.5, 4.4	Manual.
Phase 5 — Login placeholder (deferred but acknowledged)
D-5 dependency — owner must confirm that the login screen is out of scope for B-02. The first-admin creation is correct and the data is in place for the future login flow; this phase is "do nothing here."

Phase 6 — Client discovery & connection
#	Item	Dependencies	Verify
6.1	Add IPrimaryComputerDiscovery port + UdpPrimaryComputerDiscovery impl.	0.3	Integration on two-machine test bed.
6.2	Add IPrimaryComputerBeacon port + UdpPrimaryComputerBeacon impl; start it from ServerSetupViewModel after success.	6.1	Integration.
6.3	Add RunClientSetupCommand / Handler (discover → confirm → save toplab.config.json → connectivity check).	6.1, 1.2	Integration.
6.4	Add ClientSetupWindow / ViewModel.	6.3	Manual.
6.5	Wire FirstRunWindow "Client" button to ClientSetupWindow.	2.3, 6.4	Manual.
6.6	Add IPrimaryEndpointProbe (Application) wrapping CheckDatabaseConnectivityQuery to avoid duplicate plumbing.	—	Reuses existing.
Phase 7 — Hardening & regression
#	Item	Dependencies	Verify
7.1	WpfAppLogger scrubs passwords / connection strings.	—	Unit: known input → no plaintext in output.
7.2	Setup-state recovery tests (kill mid-flow scenarios).	1.1, 2.1, 3.4, 4.3	Integration.
7.3	Regression: existing CheckDatabaseConnectivityQueryHandlerTests still pass.	3.1	dotnet test.
7.4	Manual test plan (§20) executed and signed off.	All	Manual.
Phase 8 — Documentation & handoff (out of scope as code change; recorded here for the owner)
Owner-facing setup guide (Arabic + English).
IT-facing checklist for the lab: enable TCP/IP, open firewall, install Express, run setup on primary, run setup on each client.
18. File-Level Implementation Map
No files are created or modified by this plan. The list below is the deliverable spec for the future implementation phase.

18.1 Files to create (new)
File	Purpose
src/TopLab.Application/Common/Configuration/WorkstationConfiguration.cs	POCO for toplab.config.json.
src/TopLab.Application/Common/Configuration/IWorkstationConfigurationStore.cs	Port.
src/TopLab.Application/Common/Security/IPasswordHasher.cs	Port.
src/TopLab.Application/Features/Setup/Queries/GetSetupState/GetSetupStateQuery.cs + Handler	Read SetupState.
src/TopLab.Application/Features/Setup/Commands/RunServerSetup/RunServerSetupCommand.cs + Handler + Validator	Orchestrate provisioning.
src/TopLab.Application/Features/Setup/Commands/RunClientSetup/RunClientSetupCommand.cs + Handler	Orchestrate client setup.
src/TopLab.Application/Features/Setup/Commands/CreateSystemAdmin/CreateSystemAdminCommand.cs + Handler + Validator	First admin creation.
src/TopLab.Application/Common/Interfaces/IDatabaseProvisioner.cs	Port.
src/TopLab.Application/Common/Interfaces/IPrimaryComputerDiscovery.cs	Port.
src/TopLab.Application/Common/Interfaces/IPrimaryComputerBeacon.cs	Port.
src/TopLab.Application/Common/Interfaces/IPrimaryEndpointProbe.cs	Port wrapping CheckDatabaseConnectivityQuery.
src/TopLab.Domain/Setup/SetupState.cs	Aggregate.
src/TopLab.Infrastructure/Common/Configuration/FileSystemWorkstationConfigurationStore.cs	Impl.
src/TopLab.Infrastructure/Common/Configuration/WorkstationConfigurationProvider.cs	Custom ConfigurationProvider that layers toplab.config.json.
src/TopLab.Infrastructure/Common/Security/AspNetIdentityPasswordHasher.cs	Impl.
src/TopLab.Infrastructure/Persistence/Configurations/SetupStateConfiguration.cs	EF mapping.
src/TopLab.Infrastructure/Persistence/DatabaseProvisioner/SqlServerDatabaseProvisioner.cs	Impl.
src/TopLab.Infrastructure/Discovery/UdpPrimaryComputerDiscovery.cs	Impl.
src/TopLab.Infrastructure/Discovery/UdpPrimaryComputerBeacon.cs	Impl.
src/TopLab.Presentation/Windows/FirstRun/FirstRunWindow.xaml(.cs)	View.
src/TopLab.Presentation/Windows/FirstRun/FirstRunViewModel.cs	VM.
src/TopLab.Presentation/Windows/ServerSetup/ServerSetupWindow.xaml(.cs)	View.
src/TopLab.Presentation/Windows/ServerSetup/ServerSetupViewModel.cs	VM.
src/TopLab.Presentation/Windows/ClientSetup/ClientSetupWindow.xaml(.cs)	View.
src/TopLab.Presentation/Windows/ClientSetup/ClientSetupViewModel.cs	VM.
src/TopLab.Presentation/Windows/CreateSystemAdmin/CreateSystemAdminWindow.xaml(.cs)	View.
src/TopLab.Presentation/Windows/CreateSystemAdmin/CreateSystemAdminViewModel.cs	VM.
src/TopLab.Presentation/Common/Controls/PasswordBoxWithToggle.xaml(.cs)	Reusable masked-input control.
src/TopLab.Presentation/Common/Converters/InverseBoolToVisibilityConverter.cs	For the eye toggles.
New EF Core migration: AddSetupState	Generated by dotnet ef migrations add AddSetupState.
18.2 Files to extend (modify)
File	Change	Why
src/TopLab.Infrastructure/DependencyInjection.cs	Register new services; the missing-connection-string check is moved behind a "first-run awareness" flag (D-1, D-11).	Wire new infrastructure.
src/TopLab.Application/Common/Interfaces/IApplicationDbContext.cs	Add MigrateAsync.	Let the handler trigger migrations.
src/TopLab.Infrastructure/Persistence/ApplicationDbContext.cs	Forward MigrateAsync to base.Database.MigrateAsync.	Implement.
src/TopLab.Presentation/App.xaml.cs	Replace unconditional MainWindow.Show() with the First-Run / Main-Window decision from §12.3.	Bootstrap.
src/TopLab.Presentation/DependencyInjection.cs	Register new VMs/Windows/services; upgrade WpfAppLogger to scrub.	Wire new presentation.
Directory.Packages.props	Add Microsoft.Extensions.Identity.Password 8.0.x (D-3).	New dependency.
src/TopLab.Infrastructure/TopLab.Infrastructure.csproj	Reference the new package (CPM handles version).	New dependency.
tests/TopLab.Application.Tests/Common/Fakes/	Add FakePasswordHasher, FakeWorkstationConfigurationStore, FakeDatabaseProvisioner, FakePrimaryComputerDiscovery, FakePrimaryComputerBeacon.	Test infrastructure.
tests/TopLab.Infrastructure.Tests/Persistence/Configurations/	Add tests for SetupStateConfiguration.	Regression.
tests/TopLab.Application.Tests/Features/Setup/...	New test classes.	New coverage.
18.3 Files not to change (recorded explicitly)
.gitignore — the exclusion of appsettings*.json stays.
src/TopLab.Presentation/appsettings.json — dev default stays (not deployed to customers).
Any B-01, B-03, or B-04 work — out of scope.
Docs/ and Docs/Source/ — not read, not modified.
19. Database / Migration Impact
Change	Type	Migration name	Rationale
New SetupState table	Schema	AddSetupState	Durable setup-state record. Columns: Id (PK, fixed=1), IsPrimarySetupComplete (bit), PrimaryMachineName (nvarchar(200) null), CompletedAtUtc (datetime2 null), SchemaVersion (nvarchar(50) null), LastClientConnectedAtUtc (datetime2 null).
New Permissions rows	Seed (already in 20260828052248_BaselineDataModel.cs:822-840)	none	Already seeded.
New User row	Data (inserted at first run)	none	Created by CreateSystemAdminCommandHandler.
New columns on existing tables	None	none	Plan introduces no changes to Users/Permissions/UserPermissionGrants beyond what the schema already supports (User.IsAbsolutePermission, User.PasswordHash — both exist).
New indexes	None	none	SetupState has only one row.
MigrateAsync exposed on IApplicationDbContext	Code only	none	No DB schema impact.
No migrations are generated by this plan. Migration generation is part of the future implementation phase and must be produced by the implementer via dotnet ef migrations add AddSetupState against the current ApplicationDbContext + SetupState entity.

20. Testing Plan
20.1 Unit tests (Application layer, InMemory or fakes)
Test class	Cases
CreateSystemAdminCommandHandlerTests	(a) Creates user + SetupState; (b) Rejects if any user with IsAbsolutePermission=true already exists; (c) Rejects empty username / password / mismatch; (d) Hashes the password (asserts hash ≠ plaintext and IPasswordHasher.Verify returns true).
RunServerSetupCommandHandlerTests	(a) Calls all four provisioner methods in order; (b) Writes SetupState only after all succeed; (c) Translates provisioner failures into Result.Failure with non-technical message.
RunClientSetupCommandHandlerTests	(a) Discovers → saves config → runs probe; (b) "Multiple primaries" → returns Conflict; (c) "None found" → returns NotFound.
GetSetupStateQueryHandlerTests	Returns SetupState row (or null/empty result on missing).
Pbkdf2PasswordHasherTests (in Infrastructure.Tests)	Hash is non-empty, salt-dependent, Verify round-trips.
WorkstationConfigurationProviderTests	Layers correctly over appsettings.json; missing file → default.
WpfAppLoggerScrubbingTests	Known input containing Password=... → output never contains the value.
Existing regression	CheckDatabaseConnectivityQueryHandlerTests (passes unchanged); AuthorizationBehaviorTests; ValidationBehaviorTests.
20.2 Integration tests (Infrastructure, real SQL Express)
Class	Cases
SqlServerDatabaseProvisionerIntegrationTests	(a) Fresh instance → creates DB + applies migrations + verifies schema; (b) Existing DB + migrations already applied → no-op; (c) Engine unreachable → throws categorized error; (d) Migration failure → partial state recoverable. Gated on availability of SQL Express; skipped on CI without it.
UdpPrimaryComputerDiscoveryIntegrationTests	Two-process test: beacon replies, discovery finds it, multiple beacons handled.
UdpPrimaryComputerBeaconIntegrationTests	Throttles by nonce; stops cleanly on StopAsync.
20.3 End-to-end / manual tests (two physical or VM machines)
Scenario	Steps	Pass criteria
Fresh primary install	Boot app on a clean machine with SQL Express installed.	First-Run window appears; "Primary" selected; provisioning shows 4 statuses; Create System Admin window appears; admin created; success message; app closes setup; MainWindow opens.
Fresh client install	Boot app on a second machine on the same LAN.	First-Run window appears; "Client" selected; primary discovered within 3 s; "Connect to: " confirmation; success; MainWindow opens.
Restart after primary setup	Reboot primary, launch app.	Skips First-Run; opens MainWindow.
Restart after client setup	Reboot client, launch app.	Skips First-Run; opens MainWindow.
Update simulation	(Simulated by re-running TopLab.exe over the installed one.)	Skips First-Run; preserves admin.
Interrupted setup	Kill the app during step 3 of provisioning.	Next launch: enters First-Run, detects DB+migrations already applied, skips to admin creation only.
Duplicate admin attempt	Manually invoke CreateSystemAdminCommand twice via a test.	Second invocation returns Conflict.
Multiple primaries on LAN	Spin up a second primary.	Client shows list.
Primary unreachable	Pull the network cable on primary, then try to connect from client.	Client shows "تعذّر الاتصال".
Discovery timeout	Disconnect LAN.	Client shows "لم يتم العثور على الخادم الرئيسي."
Bad credentials (future)	n/a	n/a (login out of scope).
Log scrubbing	Trigger a logging path that includes Password=secret123.	Log file does not contain secret123.
Regression: existing shell	Boot the app as a non-first-run user.	Status bar still works (CheckDatabaseConnectivityQuery still wired).
20.4 Coverage targets
Application handlers: ≥ 80% branch coverage.
Infrastructure: provisioner and discovery covered by integration tests; other code by unit tests.
Presentation: VMs covered by manual test plan only (no headless WPF tests in scope for this task).
21. Acceptance Criteria
The implementation is not complete just because it compiles.

#	Criterion	Verifiable by
1	A user with no appsettings.json and no toplab.config.json can complete first-run setup without typing any technical value.	Manual.
2	After primary setup, the Users table contains exactly one user with IsAbsolutePermission=true and a non-empty PasswordHash.	DB inspection.
3	After primary setup, the SetupState row exists with IsPrimarySetupComplete=true.	DB inspection.
4	The SetupState row is written in the same SaveChanges as the admin User insert (transactional).	Code review + integration test.
5	After a successful primary setup, restarting the app does not re-show the First-Run or Create Admin windows.	Manual.
6	After a fresh client setup, restarting the app does not re-show the First-Run window.	Manual.
7	The Create System Admin window contains only: red banner, Username, Password (+ eye), Confirm Password (+ eye), Create button. No other inputs.	Visual.
8	The eye toggles on Password and Confirm Password are independent.	Manual.
9	The created admin's password is stored as a hash, never plaintext.	DB inspection + unit test.
10	No source file contains a hardcoded default or example admin password.	grep for Password= and any literal near CreateSystemAdmin.
11	A primary with a fresh SQL Express install completes provisioning automatically.	Manual.
12	A client machine discovers the primary within 3 s and does not require IP/host/SQL inputs.	Manual.
13	All 16 error scenarios in §13 produce a non-technical Arabic message and an English log line.	Manual + log inspection.
14	Connection string, password, and any Password= fragment never appear in any log file.	Grep test.
15	The app uses application-level (User table) auth, not Windows-identity auth.	Code review.
16	The original CheckDatabaseConnectivityQueryHandlerTests and behavior tests still pass.	dotnet test.
17	No B-01 / B-03 / B-04 file was modified by this work.	git diff against 28cb276.
18	The plan does not introduce a second architecture, second auth system, or second hashing algorithm.	Code review.
19	The Create System Admin flow refuses to run when an admin already exists.	Unit test.
20	Setup-state recovery scenarios in §12.2 all produce a coherent next-launch behavior.	Integration + manual.
22. Owner Decisions Required
These are the decisions the owner must make before implementation starts. Each one materially affects the deliverable. The recommendation is provided; the owner can override.

D-1. Production database engine
Option	Pros	Cons	Impact
SQL Server Express (recommended)	Free, redistributable, network-reachable.	Requires IT pre-install on the primary.	Determines IDatabaseProvisioner connection string template and the install guide.
SQL Server (paid)	Larger capacity, full feature set.	Cost.	Same.
LocalDB (current)	Already configured.	Cannot serve LAN clients — incompatible with the deployment model.	Disqualified.
Recommendation: Express. Rejecting Express forces a redesign of the entire provisioning and distribution approach.

D-2. SQL instance identity
Option	Pros	Cons
Default instance (MSSQLSERVER) on port 1433 (recommended)	Simplest.	Requires Windows admin to install with default.
Named instance TOPLAB	Predictable.	Requires the customer to install Express with that specific name.
Configurable per customer	Flexible.	Defeats the "no technical typing" requirement.
Recommendation: named instance TOPLAB on the primary; the connection string in toplab.config.json is a build-time constant. If the customer disagrees, the build emits a different constant.

D-3. Password hashing algorithm
Option	Pros	Cons
Microsoft.Extensions.Identity.PasswordHasher (recommended)	Battle-tested, fits NVARCHAR(300).	One NuGet package.
PBKDF2 via KeyDerivation	Slightly less code at the impl.	More code at the call site.
Recommendation: Identity PasswordHasher.

D-4. LAN discovery: auto-pick vs. always-list
Option	Pros	Cons
Auto-pick when exactly one primary responds (recommended)	One-click UX.	If two primaries appear for any reason, the user has to choose.
Always show a list	Predictable.	Two-click UX even for the common case.
Recommendation: auto-pick on one, list on many.

D-5. Login screen scope
Option	Pros	Cons
Defer login screen to a later milestone (recommended for B-02)	B-02 stays scoped.	First admin "works through the login screen" only when that screen exists. The data is correct today; the surface is not.
Build the login screen in B-02	Fully working "round trip."	Scope creep into B-01 / M01 territory.
Recommendation: defer. The plan records this as a known dependency in §11.1.

D-6. Password policy
Option	Pros	Cons
Minimum length 8, no other rules (recommended for a lab audience)	Easy to remember.	Weaker.
Complexity rules	Stronger.	Users will write passwords on monitors.
Length ≥ 12, no other rules	NIST-aligned.	Harder to remember.
Recommendation: length ≥ 8 only.

D-7. LastClientConnectedAtUtc telemetry
Option	Pros	Cons
No telemetry (recommended)	Less code, no privacy concern.	Owner loses visibility.
Yes, write a heartbeat on every successful client boot	Visibility.	Adds writes; minor.
Recommendation: no. The column is in the schema but is not written in this iteration.

D-8. LAN discovery security
Option	Pros	Cons
Plain UDP reply (recommended for a small LAN)	Simple, no extra infra.	An attacker on the LAN could redirect a client to a rogue server.
Signed reply (HMAC with a per-customer key)	Mitigates spoofing.	Requires key distribution to every client.
Recommendation: plain for v1. Application-level auth still protects data — a rogue server cannot read another lab's data without valid users. Mitigation deferred to a hardening phase.

D-9. CreatedById for the first admin
Option	Pros	Cons
0 (system) (recommended)	The interceptor (AuditableEntitySaveChangesInterceptor) handles it.	"System" is a convention.
Null + nullable FK	Avoids "0" magic.	Schema change.
Recommendation: 0.

D-10. Installer-time role selection vs. first-run role selection
Option	Pros	Cons
First-run role selection (recommended for this task)	No installer dependency.	Every launch on a fresh PC shows First-Run.
Installer-time role selection	Cleaner UX.	Requires the installer to exist first.
Recommendation: first-run, since there is no installer. Revisit when an installer is added.

D-11. Behavior of DependencyInjection.AddInfrastructure when no connection string exists
Option	Pros	Cons
Throw as today, but App.OnStartup catches and routes to First-Run (recommended)	Tiny diff, no behavior change for existing devs.	One try/catch in App.xaml.cs.
Defer the throw until first DB use	Cleaner bootstrap.	Larger refactor.
Recommendation: first option.

Items 1–11 above are the only owner decisions required. Nothing else is hidden; everything else is either a verified fact or a recommendation with a default that the implementer can apply without further consultation.

23. Assumptions and Verification Items
#	Assumption	Why needed	What to verify	What changes if false
A-1	SQL Server Express will be pre-installed on the customer primary by lab IT or the install team.	Plan does not bundle the Express installer.	Confirm with the owner's deployment plan.	If the owner wants bundled install, an installer workstream is required and the plan must be re-scoped.
A-2	A typical lab LAN is a single broadcast domain (no VLANs blocking UDP 47800).	UDP discovery requires broadcast reachability.	Owner confirms typical topology.	If VLANs are common, switch to TCP probe after broadcast, or to mDNS.
A-3	The customer will grant the primary machine a static IP or DHCP reservation.	Otherwise the client cannot reconnect after the primary's IP changes.	Owner confirms IT practice.	Discovery can be re-run on each launch, or the client can store a machine name (which the application does today).
A-4	Microsoft.Extensions.Identity.Password 8.0.x is the right NuGet family (vs. Microsoft.AspNetCore.Identity).	D-3.	Confirm in NuGet.	Use the alternative.
A-5	The Application layer can grow a new Features/Setup/ folder without disturbing the existing AccessAndNavigation and SamplePipeline folders.	Verified — both folders exist alongside each other.	n/a	n/a
A-6	The Microsoft.Extensions.Hosting 8.0.1 version already in Directory.Packages.props:21 is compatible with adding a custom ConfigurationProvider that layers toplab.config.json over appsettings.json.	Standard pattern, version-agnostic.	Compile.	If incompatible, fall back to direct IConfigurationBuilder.AddJsonFile in App.xaml.cs.
A-7	The AuditableEntitySaveChangesInterceptor (src/TopLab.Infrastructure/Persistence/Interceptors/) writes CreatedById from ICurrentUserService.UserId; the first admin must therefore be created while ICurrentUserService.IsAuthenticated == false.	Interceptor must not write null or crash.	Inspect interceptor.	If the interceptor requires an authenticated user, the first admin creation is wrapped in a session-temp "system" identity, or the interceptor is bypassed for the first admin.
A-8	The first-run experience can be implemented in pure WPF (no installer wizard) given there is no installer.	Verified — no installer exists.	n/a	If the owner wants an installer-driven flow, the role selector moves to the installer; otherwise no change.
A-9	ICurrentUserService does not need to be "logged in" for the first admin to be created. The Create System Admin handler runs pre-authentication.	Current user service is "in-memory" and starts empty.	Inspect CurrentUserService.cs:25-27 — confirms IsAuthenticated=false by default.	If a future change makes it require a session, the handler must set a "system" session.
A-10	No regulatory claim (HIPAA, GDPR) is implied or required.	Owner has not asked.	Owner confirms.	Plan is unchanged; a separate compliance workstream would be needed.
24. Out-of-Scope Items
#	Item	Why excluded
1	Login screen implementation	B-02 scope; flagged as a dependency on D-5.
2	Password change / reset / "forgot password"	Future login workstream.
3	Users-management UI (creating additional non-admin users)	Future Users feature.
4	Permission-grant UI	Future Users feature.
5	Installer creation (WiX / Inno / Advanced Installer)	Owner instruction §19; explicit "do not implement installer work."
6	Bundling the SQL Server Express redistributable	Same.
7	B-01, B-03, B-04 redesign or rework	Owner instruction: "Do not reopen those issues."
8	Audit log review / compliance certification	Not requested.
9	Migration of the existing dev appsettings.json away from LocalDB	Dev environment; left as-is.
10	Unrelated documentation changes	Owner instruction.
11	Themes / RTL polish / accessibility pass on First-Run	Not requested.
12	Telemetry / usage analytics	Not requested.
13	Replacing the current WpfAppLogger with a full logging library (Serilog etc.)	IAppLogger is sufficient; only the scrubbing behavior is added.
14	Headless WPF UI tests	Out of scope; manual test plan covers the UI.
25. Risks and Mitigations
#	Risk	Likelihood	Impact	Mitigation
1	Owner picks LocalDB as production engine.	Low (it cannot serve LAN clients).	High (entire model fails).	§22 D-1 makes the contradiction explicit.
2	LAN discovery fails on real customer networks.	Medium.	High.	D-4 list-on-many; D-8 plain UDP is a v1 choice; manual fallback to entering the server name is a deferred recovery path (not in v1, but noted in D-10).
3	First admin's password is lost by the user.	High.	High.	Red banner warning in the window; no recovery; owner can re-run the first-run flow only on a fresh DB. This is intentional — the alternative (a hardcoded recovery password) violates §11. The plan does not add a recovery backdoor.
4	AuditableEntitySaveChangesInterceptor rejects the first admin because no user is logged in.	Low.	Medium.	A-7: verify and add a "system" session if needed.
5	UDP 47800 blocked by an overzealous corporate firewall.	Medium.	Medium.	Document the port in the install guide; out-of-scope here.
6	Customer disables Windows Firewall on the primary, exposing SQL to the internet.	Medium.	High.	Install guide (out of scope) should call this out.
7	Migration generation is done incorrectly (e.g., applied to the wrong DB).	Low.	Medium.	Plan generates exactly one new migration (AddSetupState); code review must verify.
8	The plan is implemented without owner decisions.	High.	High.	D-1 … D-11 are blockers; Phase 0 of §17 is "no code until decisions."
9	The PasswordHasher output exceeds 300 chars.	Low.	Low.	Identity's PasswordHasher output is well under 300 chars; verified in package docs.
10	"First admin" gets created with a privilege that doesn't actually exist in the seed data.	Low.	High.	IsAbsolutePermission=true does not require a UserPermissionGrant row; verified in AuthorizationBehavior.cs:33-40.
11	WpfAppLogger scrubber misses a new log path.	Low.	Medium.	Centralize log construction; test all call sites.
12	Client machine's toplab.config.json survives a primary IP change and silently points to a wrong host.	Medium.	Medium.	A-3: customer must assign a static IP or use machine-name-based connection strings. v1 stores the IP that was discovered; out-of-scope to add periodic re-discovery.
13	The plan's UDP discovery leaks information about the lab's existence.	Very low.	Low.	Reply contains only machine name, instance name, and DB name — all of which would be discoverable by any LAN-side network scan. No additional risk.
26. Final Recommended Implementation Sequence
Strictly ordered. Each step's prerequisites are satisfied by the previous steps.

Owner decisions (D-1 … D-11) collected and recorded. No code yet. (§22)
Assumptions A-1 … A-10 verified. No code yet. (§23)
Phase 1 — Configuration store. Adds toplab.config.json and a layered configuration provider. The app still throws on a missing connection string, but App.OnStartup (in Phase 2) will catch it. (§17.1)
Phase 2 — First-run gate. Adds SetupState migration and the FirstRunWindow (two buttons, no inputs). App.OnStartup selects First-Run vs. Main. (§17.2)
Phase 3 — Database provisioner. Adds IDatabaseProvisioner + SqlServerDatabaseProvisioner, RunServerSetupCommand, and ServerSetupWindow. Primary path now works end-to-end. (§17.3)
Phase 4 — Create System Admin. Adds IPasswordHasher, CreateSystemAdminCommand, and CreateSystemAdminWindow with banner + three inputs + two eye toggles + button. The first admin is created with IsAbsolutePermission=true and SetupState.IsPrimarySetupComplete=true in the same SaveChanges. (§17.4)
Phase 5 — Login placeholder. No code. Owner confirms D-5. (§17.5, §11.1)
Phase 6 — Client discovery & connection. Adds IPrimaryComputerDiscovery + UdpPrimaryComputerDiscovery, IPrimaryComputerBeacon + UdpPrimaryComputerBeacon, RunClientSetupCommand, and ClientSetupWindow. Client path works end-to-end. (§17.6)
Phase 7 — Hardening & regression. WpfAppLogger scrubs; recovery tests; full manual test plan executed. (§17.7, §20)
Phase 8 — Handoff. Owner-facing setup guide + IT-facing checklist (out of scope as code change). (§17.8)
Total estimated touch points (for sizing only, not for implementation): ~30 new files, ~8 modified files, 1 new EF migration, 1 new NuGet package, no architectural change. Every new file is mapped in §18.1; every modified file is mapped in §18.2; the no-touch list is in §18.3.

Plan ends here. No files were created, modified, deleted, or moved during this planning task. No Git state, database state, package, or project configuration was changed.

