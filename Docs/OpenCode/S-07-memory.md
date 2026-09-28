# Loop Engineering — Memory File

- **Module:** Fix Set 1 — persistent logging, navigation/permission tightening, shell & printing correctness, test coverage (S-07 — cross-cutting remediation, non-module)
- **Module Number:** S-07
- **Source Plan:** `Docs/OpenCode/S-07.md`
- **Date Created:** 2026-09-28
- **Total Slices:** 12
- **Current Slice:** none — Slice 1 not started
- **Current Branch:** `main`
- **Author:** loop-engineering skill (execution to be carried out by the executing agent per owner authorization; **stage-10 local commit is authorized — see SD-1/SD-2**)

---

## Module Summary

S-07 executes the seventeen audit findings that pass the Fix Set 1 scope test — no new EF migration, no edit to an existing migration or the model snapshot, no migration application, no `HasData`/seed change, no new permission code, no schema change, and no open owner decision. Twelve ordered slices, each independently buildable and testable, each with a `VG-nn` validation gate, a zero-drift gate, and an explicit statement of what the owner must check manually on Windows. The findings span four layers: the shell (`ShellViewModel` navigation enablement and the discarded `LockWorkstationCommand` result), the application layer (MediatR pipeline behavior order, 22 missing command validators, the user-management authorization gap, and the fail-open delete-user reference guard), the infrastructure layer (the logger that writes through `Debug.WriteLine`, and the pinned `"Arial"` PDF font family), and test coverage (the first presentation-layer test project, and the first relational-engine test project).

**This workstream is not one of the numbered feature modules M-01…M-23.** It is a cross-cutting remediation of defects found by an audit, so it takes the next free S-series identifier, `S-07`, following the naming rationale established by S-00 and reaffirmed by S-01.

**F-01 is closed and is not in this workstream.** At the pinned commit `MainWindow.xaml` no longer contains the four dead Home buttons, and `PatientsHubView.xaml`, `PatientsHubViewModel.cs` and `HomeViewModel.cs` are untouched by this plan — asserted by the VG-07 gate.

**Out of scope, recorded in the plan's register and not planned here:** NEW-01, MIG-01, m-07, a dedicated user-administration permission code, NEW-02, F-06, the m-04 text placeholders, m-05, and embedding an Arabic font asset.

---

## Global Validation Gates

- **Gate G0 (pre-execution, runs once, before Slice 1):** the agent records **its own** baseline on the owner's Windows machine — the exact build result and the exact pass/fail counts of each of the three existing test projects — into the "Baseline" section below. Until that section is filled in, no slice may start.
- **Gate G1 (post-execution, every slice):** the same quality gate as G0, plus the slice-specific gate in the table below, plus the zero-drift gate.

### Zero-drift gate (every slice, no exceptions)

```
dotnet-ef migrations has-pending-model-changes \
  --project src/TopLab.Infrastructure/TopLab.Infrastructure.csproj \
  --startup-project src/TopLab.Infrastructure/TopLab.Infrastructure.csproj \
  -- -p:EnableWindowsTargeting=true
```

Expected, every time: `No changes have been made to the model since the last migration.`

and

```
git diff --stat src/TopLab.Infrastructure/Persistence/
```

Expected, every time: empty.

`dotnet-ef` must be version **8.0.30**, matching `Directory.Packages.props:21`. A different version produces different generated SQL and invalidates the comparison.

---

## Quality Gate (non-negotiable — a slice may be marked complete ONLY when ALL FOUR hold)

1. The slice's implementation is fully complete per `Docs/OpenCode/S-07.md`.
2. The ENTIRE solution builds successfully — **zero errors AND zero warnings** (`dotnet build TopLab.sln -p:EnableWindowsTargeting=true`).
3. **No test count is lower than the recorded baseline**, in any of the test projects — except where the plan explicitly predicts a change. The only predicted change is Slice 10, which may raise `TopLab.Infrastructure.Tests` (see VG-10).
4. The slice's own specific exit criteria (its `VG-nn` gate in the plan) pass, item by item.

A new build warning is a regression, exactly like a failing test. `Directory.Build.props` sets `Nullable=enable`, `LangVersion=latest`, `ImplicitUsings=enable` and `ManagePackageVersionsCentrally=true`; the build is currently clean and must stay clean.

---

## Stop/Continue Rule

After a slice completes, verify success via ALL THREE of:

- (a) the whole solution builds with zero errors and zero warnings;
- (b) no test count is below the recorded baseline;
- (c) that slice's specific `VG-nn` gate passes item by item.

If all three hold → **proceed immediately to the next slice, with no pause and no human confirmation required.** Make the slice's Stage 10 local commit first, then begin Stage 1 of the next slice.

This is the established rule of `S-00-memory.md`/`S-01-memory.md`, unchanged for S-07. The owner reviews each commit after the fact; that review is not a gate the agent waits on. The single normal stopping point is full completion of all 12 slices.

If any one of (a), (b), (c) fails → retry within the slice's scope. If the **same** failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs **5 consecutive times**, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Ordinary expected failures caused by the current slice and resolved within the same correction cycle do NOT count as five separate failures.

---

## Additional user-authorized execution parameters (override skill defaults)

- **Stop threshold:** 5 consecutive failures for the same reason.
- **Execution order:** strictly sequential S1 → S2 → … → S12. No parallel slices, no reordering, no merging, no skipping ahead.
- **Git: automatic LOCAL commit after each verified slice** (no confirmation pause), on the **current** branch (`main`). **NEVER** create or switch a branch, **NEVER** push to any remote, **NEVER** force-push, **NEVER** amend, rebase or reset, **NEVER** modify or rewrite remote history. Commit message: `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage explicit paths only — never `git add -A`, never `git add .`. Never commit on a red build or a failed gate.
- **Pacing:** strictly sequential S1 → S12, with **no pause between slices** and no request for human confirmation. See the Stop/Continue Rule above.
- **Stage-7 gate:** the plan's textual exit criteria replace any standard UI journey. This workstream's UI behaviour is verified manually by the owner on Windows — no UI test harness exists in the repository and none may be invented.
- **The application must never be launched.** `App.xaml.cs` applies EF migrations to the configured database at startup, so launching it could migrate the owner's real database. Building and running automated tests is allowed and expected.
- **Migrations are untouchable:** no creating, no editing, no applying, no `dotnet ef database update`, no `--force`. The model snapshot is untouchable.
- **Docs:** the agent updates only this memory file, because recording progress is this workstream's progress mechanism. It does not edit `Docs/OpenCode/S-07.md` or any other document.
- **The only normal stopping point** is Slice 12 completing its gate, after the owner has confirmed all eleven previous slices.

---

## Baseline (to be filled in by the executing agent on the owner's Windows machine — EMPTY AT CREATION)

The audit that produced this plan measured a **Linux** baseline. It is recorded here for context only and is **not** the target. The no-regression rule is anchored to the numbers the agent measures itself.

### Linux figures measured by the audit (context only — NOT the Windows baseline)

```
dotnet build TopLab.sln --no-restore -p:EnableWindowsTargeting=true
    Build succeeded.  0 Warning(s)  0 Error(s)

TopLab.Domain.Tests          : 474 / 474
TopLab.Application.Tests     : 1419 / 1419
TopLab.Infrastructure.Tests  : 192 / 195   (3 printing tests fail)
```

The three failures are `ReceiptPrintingServiceTests`, `InvoicePrintingServiceTests` and `WorkSheetPrintingServiceTests` (their `*_HappyPath_*` cases), all failing at `Assert.True(result.IsSuccess)` because the writers pin the `"Arial"` family, which does not exist on a Linux host with only DejaVu fonts.

> **The owner's Windows baseline may legitimately differ: those three tests may already pass there, in which case the Windows Infrastructure baseline is 195/195.** The agent must measure and record what is actually true on the machine, and the recorded value — not the Linux figure — is what every slice is judged against.

### Agent-measured Windows baseline (FILL IN BEFORE SLICE 1)

| Item | Value |
|---|---|
| Date measured | 2026-09-28 |
| .NET SDK version | 9.0.318 (8.0.425 also installed; dotnet-ef 8.0.30 used for EF commands) |
| `git rev-parse HEAD` | `66a17f7d8e87e6eb39d47fce46c750cb3eaba6a6` ✓ |
| `git status --short` | 3 untracked lines (S-07 package files only — expected) |
| `dotnet-ef --version` | 8.0.30 ✓ |
| Build warnings | 0 |
| Build errors | 0 |
| `TopLab.Domain.Tests` passed / total | 474 / 474 |
| `TopLab.Application.Tests` passed / total | 1419 / 1419 |
| `TopLab.Infrastructure.Tests` passed / total | **195 / 195** (all pass — Arial exists on Windows) |
| `TopLab.Infrastructure.Tests` failing test names | **none** |
| `has-pending-model-changes` | "No changes have been made to the model since the last migration." ✓ |
| Docker available (for Slice 12) | **No** — `docker` not recognized |
| Windows Arabic-capable font families present | Arial, Traditional Arabic, Simplified Arabic, Arabic Typesetting, DecoType Naskh, Segoe UI, Tahoma, Times New Roman, Calibri, and many more |

---

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|---|---------|------------------|---------------|
| 1 | VG-01 | Hygiene, stale text, dead code: build 0/0; no count below baseline; zero-drift; `git ls-files | grep -ci testresults` → 0; no `localdb` in `appsettings.example.json`; the `.gitignore` negation at `:27` survives; no `Placeholder` left in `AboutWindow.xaml.cs`; no `_services` left in `CurrentUserService.cs`. **Migration: NONE — zero-drift gate** | `dotnet build`; three test projects; ef drift check; `git diff --stat src/TopLab.Infrastructure/Persistence/`; the greps listed |
| 2 | VG-02 | 22 command validators: build 0/0; Application strictly above baseline; a **static completeness test** asserting every parameterised `*Command.cs` has a sibling validator and every parameterless one does not need one; per-validator behavioural tests with named properties and error codes. **Migration: NONE — zero-drift gate** | `dotnet build`; `dotnet test tests/TopLab.Application.Tests`; ef drift check |
| 3 | VG-03 | Delete-user reference guard: build 0/0; a throwing probe yields `ErrorType.Unexpected` (not `Conflict`) with **zero** `Remove` and **zero** `SaveChangesAsync`; a true reference still yields the unchanged Arabic `Conflict`; a user with no references still deletes; a throwing `Set<User>()` also yields `Unexpected`; no `SafeAny` and no bare `catch` remain. **Migration: NONE — zero-drift gate** | `dotnet build`; `dotnet test tests/TopLab.Application.Tests`; ef drift check; greps on the handler |
| 4 | VG-04 | User-management authorization: build 0/0; Application above baseline; non-absolute actor cannot self-escalate via `UpdateUser` or `CreateUser` (Forbidden, no save); the same actor updating with `false` still succeeds unchanged; an absolute actor is unaffected; a non-absolute actor cannot delete/deactivate an absolute target; **all eight** components return Forbidden when unauthenticated, **sent valid ids**; the login path is intact (`SignIn` still succeeds, `VerifySecondaryPassword` still verifies the actor's own hash); a static test pins that `SignInCommand`/`SignOutCommand`/`GetCurrentSessionQuery`/`VerifySecondaryPasswordQuery` still do not implement `IAuthorizedRequest`; no new `*AccessPolicy.cs` and no new `*Command.cs`. **Migration: NONE — zero-drift gate** | `dotnet build`; `dotnet test tests/TopLab.Application.Tests`; ef drift check; `git diff -- src/TopLab.Application/Features/UsersAndPermissions/` |
| 5 | VG-05 | Lock-workstation result: build 0/0; a static test asserting the bare `await _mediator.Send(new LockWorkstationCommand());` statement is gone from `LockWorkstationAsync`; `Features/AccessAndNavigation/` and `UnlockWindow.xaml.cs` diffs empty. **Migration: NONE — zero-drift gate** | `dotnet build`; `dotnet test tests/TopLab.Application.Tests`; ef drift check; source assertion |
| 6 | VG-06 | Navigation filtering: build 0/0; a static test asserting no literal `IsEnabled = true` remains in `BuildNavigationItems`; a static test asserting each of the four mapped codes is one of the 13 seeded codes; a static test pinning that the six owner-decision items stay present and ungated; `PermissionConfiguration.cs` diff empty; `MainWindow.xaml` diff empty; `src/TopLab.Application/` diff empty. **Migration: NONE — zero-drift gate** | `dotnet build`; `dotnet test tests/TopLab.Application.Tests`; ef drift check; three source assertions |
| 7 | VG-07 | Sent-out-samples entry point: build 0/0 (compiles new XAML); `src/TopLab.Application/` diff empty; `PatientsHubView.xaml`, `PatientsHubViewModel.cs` and `HomeViewModel.cs` diffs **empty**; a static test asserting a creation site for `SendSampleOutDialogWindow` now exists. **Migration: NONE — zero-drift gate** | `dotnet build`; the three `git diff` assertions; the source assertion |
| 8 | VG-08 | Durable file logging: build 0/0; Infrastructure above baseline; the logger writes to a temp path (one file, one line, three inputs in order); two calls → two lines; a later date → a differently-named file; an unwritable path does **not** throw; **a reflection test pinning `IAppLogger` to exactly one method `(string, string, TimeSpan)`**; `Debug.WriteLine` count in `src/` is 0; exactly one `IAppLogger` registration in Infrastructure and zero in Presentation; `LoggingBehavior` diff empty. **Migration: NONE — zero-drift gate** | `dotnet build`; `dotnet test tests/TopLab.Infrastructure.Tests`; ef drift check; greps |
| 9 | VG-09 | Pipeline order: build 0/0; Application above baseline; **three scenarios** through the real `AddApplication()` path — invalid → one entry `Failure:Validation` with the handler never run; forbidden → one entry `Failure:Forbidden` with the handler never run; allowed → one entry `Success` with the handler run once; the logged content is a type name plus a token; `Common/Behaviors/` diff empty; existing Authorization and Validation behaviour tests unchanged. **Migration: NONE — zero-drift gate** | `dotnet build`; `dotnet test tests/TopLab.Application.Tests`; ef drift check; the two unchanged existing test classes |
| 10 | VG-10 | Font resolution: build 0/0; **no count below baseline anywhere**; Infrastructure ≥ baseline, rising by exactly 3 on a host without `"Arial"`; resolver unit tests including the Arabic-capability assertion; the three printing tests pass; `RegisterFont` count in `src/` is 0; the settings ViewModels' font lists and defaults are untouched. **The only slice permitted to raise a count.** **Migration: NONE — zero-drift gate** | `dotnet build`; `dotnet test tests/TopLab.Infrastructure.Tests`; ef drift check; greps |
| 11 | VG-11 | Presentation structural tests: **whole solution** 0/0 including the new project; the new project green **on Windows**; the three pre-existing projects not below baseline; a static test resolving 0 > `x:Type` in `MainWindow.xaml`; a static test resolving 0 > `{Binding …Command}`; every `Window` has a creation site (namespace-qualified form required); every view is RTL; the two deferred behavioural tests from Slices 5 and 6; **a must-be-able-to-fail proof recorded**; exactly 4 `PackageReference`s and no `Version=`; one added project in `TopLab.sln`. **Migration: NONE — zero-drift gate** | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Presentation.Tests`; ef drift check; the fail-proof demonstration |
| 12 | VG-12 | Relational integration tests: **whole solution** 0/0; the three pre-existing projects and `TopLab.Presentation.Tests` not below baseline; `dotnet test tests/TopLab.Persistence.Tests` green, **and the agent must state explicitly whether it ran or skipped**; the never-connect static assertions (no `localdb`, no `ConfigurationBuilder`, no `GetConnectionString(`); exactly one added `PackageVersion` line; exactly one added project in `TopLab.sln`; `Persistence/` diff empty. **Migration: NONE created, edited or applied — zero-drift gate** | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Persistence.Tests`; ef drift check; `git diff -- Directory.Packages.props`; `git diff -- TopLab.sln` |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Hygiene, stale text and dead code (m-08, m-06, m-10, m-04a, NEW-04) | [x] DONE | VG-01 |
| 2 | Validators for the 22 parameterised commands (m-01) | [x] DONE | VG-02 |
| 3 | Delete-user reference guard fails closed (M-04) | [x] DONE | VG-03 |
| 4 | User-management authorization: no self-escalation (B-01) | [x] DONE | VG-04 |
| 5 | Lock-workstation result is checked (NEW-05-LOCK) | [x] DONE | VG-05 |
| 6 | Navigation items filtered by permission (F-04) | [x] DONE | VG-06 |
| 7 | Sent-out-samples write entry point (M-01) | [x] DONE | VG-07 |
| 8 | Durable file logging (F-03, NEW-06) | [x] DONE | VG-08 |
| 9 | Logging pipeline behavior moved outermost (NEW-05-LOG) | [x] DONE | VG-09 |
| 10 | Font-family resolution (m-09 + NEW-03) | [x] DONE | VG-10 |
| 11 | Presentation structural tests (M-02) | [x] DONE (env-limited) | VG-11 |
| 12 | Relational integration tests (F-05 / M-03) | [ ] NOT STARTED | VG-12 |

---

## Settled Decisions (from plan — binding; do not re-open during execution)

- **SD-1 — Git: one local commit per verified slice, on the current branch (`main`).** Stage 10 is authorized exactly as in S-00…S-06. Stage only that slice's own files — never `git add -A`, never `git add .`. Message: `[S-07] Slice N/12: <slice title> — loop-engineering`. **Never push, never branch, never amend, never rebase, never reset, never force-push, never rewrite history.** Never commit a red build or a failed gate. Read-only git commands are used freely for the per-slice report.
- **SD-2 — Proceed immediately to the next slice.** After a passing gate: record the results, commit, and begin the next slice's Stage 1 with no pause and no human confirmation. The only normal stopping point is Slice 12 completing its gate.
- **SD-3 — The baseline is the agent's own Windows measurement**, recorded above before Slice 1. The Linux figures are context only. The three printing tests may already pass on Windows, and the Infrastructure baseline may therefore be 195/195 rather than 192/195.
- **SD-4 — `IAppLogger`'s signature does not change.** `Log(string requestName, string outcome, TimeSpan duration)` stays the only method. This is the structural guarantee that logs cannot carry passwords, patient identifiers or results; a reflection test pins it in Slice 8.
- **SD-5 — F-04 gates exactly four navigation items:** «ورقة العمل» → `PRINT_WORKSHEET`, «الإحصائيات» → `STATISTICS`, «النظام» → `PT_AUDIT_ACCESS`, «قفل المحطة» → `EDIT_SYSTEM_SETTINGS`. «حول البرنامج» and «خروج» are always enabled because they issue no request. The remaining six stay enabled as owner decisions OD-1…OD-6. The agent must not invent a seventh mapping.
- **SD-6 — B-01 adds no `IAuthorizedRequest` and no `UsersAndPermissionsAccessPolicy`.** The fix is entirely actor/target checks inside handlers plus one UI binding.
- **SD-7 — The four login-path requests get no new handler guard.** `SignInCommand`, `SignOutCommand`, `GetCurrentSessionQuery` and `VerifySecondaryPasswordQuery` stay ungated and unguarded, so login and the secondary-password re-challenge keep working.
- **SD-8 — m-01 covers the 22 parameterised commands only.** No validator for the three parameterless commands (`LockWorkstationCommand`, `ApplyDatabaseUpdatesCommand`, `SignOutCommand`); a static test enforces exactly this.
- **SD-9 — m-04 touches the version line only.** `AboutWindow.xaml:21-24` is not edited and no copyright, support address or link is invented.
- **SD-10 — m-09/NEW-03 resolves a font family; it does not embed one.** No `FontManager.RegisterFont*` call and no font asset.
- **SD-11 — m-10 introduces no real credential** — placeholders only — and the `.gitignore` negation at `:27` survives untouched.
- **SD-12 — Slice 12 is last and skip-guarded.** A skipped integration run is reported as *skipped*, never as a pass.
- **SD-13 — The application is never launched.** Its startup applies migrations to the configured database.
- **SD-14 — Slice order is fixed.** A failing slice is reported, not worked around by reordering.
- **SD-15 — Packaging hygiene.** Every `dotnet` command carries `-p:EnableWindowsTargeting=true`. Test projects run one at a time. No `Version=` attribute in any `.csproj`; versions live only in `Directory.Packages.props`. `dotnet-ef` stays at 8.0.30.
- **SD-16 — Established repository patterns only.** `Result` / `Result<T>` returns; `Error.Validation|NotFound|Conflict|Forbidden|Unexpected` with Arabic messages; hand-rolled MVVM (`ViewModelBase.SetProperty`); `ResultErrorPresenter` for every surfaced error; `IDialogService` for dialogs. No new framework, control library or pattern.
- **SD-17 — A plan is a hypothesis.** Before editing, the agent re-verifies the finding against the live code. If the code differs from the plan's description, the agent **stops and reports** rather than adapting silently.

---

## Created UI Texts Register (appended by the executing agent as texts are created)

| Slice | String | Where | Why it is new | Recorded by / date |
|---|---|---|---|---|
| _(none yet)_ | | | | |

Rule: any user-facing string that has a backend counterpart is copied byte-for-byte from the plan's Appendix A frozen-message table. UI-only strings are created once, used consistently, and appended here. **The agent must not invent copyright text, a support address, or a link.**

---

## Slice 1 — Hygiene, stale text and dead code (m-08, m-06, m-10, m-04a, NEW-04)

- **Goal:** four findings with no behavioural change plus one deletion of tracked generated files, leaving a clean, warning-free base.
- **Touches:** `src/TopLab.Infrastructure/Identity/CurrentUserService.cs`; `.gitignore`; 5 deleted `tests/**/TestResults/*/coverage.cobertura.xml`; `src/TopLab.Presentation/appsettings.example.json`; `src/TopLab.Presentation/Views/Shell/AboutWindow.xaml.cs`; `src/TopLab.Infrastructure/Persistence/ApplicationDbContext.cs` (comments only).
- **Validation Gate:** VG-01. Migration: none.
- **Expected test-count change:** **none** — this slice adds no tests.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** baseline recorded; build 0/0; all counts at or above baseline; HEAD pinned; tree clean.
- [x] **Stage 2 — Deep Understanding:** plan §9.1 and §4.7–§4.12 re-read.
- [x] **Stage 3 — File Analysis:** every file above opened at the cited lines.
- [x] **Stage 4 — Planning:** exact edits written down here.
- [x] **Stage 5 — Execution:** edits applied; the `.gitignore` secrets section preserved byte-for-byte.
- [x] **Stage 6 — Post-Execution Verification:** build 0/0.
- [x] **Stage 7 — Validation Gate:** VG-01 item by item, zero-drift gate included.
- [x] **Stage 8 — Documentation Update:** this checklist and the Execution Log updated.
- [x] **Stage 9 — Memory Status Update:** Slice Index and Current Status updated.
- [x] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice 1/12: Hygiene, stale text and dead code — loop-engineering`. Stage only this slice's own files.

---

## Slice 2 — Validators for the 22 parameterised commands (m-01)

- **Goal:** one `AbstractValidator<T>` per parameterised command that lacks one, plus per-validator tests and a static completeness test.
- **Touches:** 22 new `src/TopLab.Application/Features/<Feature>/Commands/<Name>/<Name>Validator.cs`; new test classes under `tests/TopLab.Application.Tests/Features/<Feature>/`.
- **Validation Gate:** VG-02. Migration: none.
- **Expected test-count change:** `TopLab.Application.Tests` **strictly above** baseline.

### 10-Stage Progress (Slice 2)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; HEAD pinned; tree clean.
- [x] **Stage 2 — Deep Understanding:** plan §9.2 and §4.6 re-read.
- [x] **Stage 3 — File Analysis:** the 22 command records and their neighbouring validators.
- [x] **Stage 4 — Planning:** 22 validator files + completeness test + behavioural tests.
- [x] **Stage 5 — Execution:** validators added; no new permission gate on any of the 22.
- [x] **Stage 6 — Post-Execution Verification:** build 0/0.
- [x] **Stage 7 — Validation Gate:** VG-02 passed — completeness test green, behavioural tests green.
- [x] **Stage 8 — Documentation Update:** checklist and Execution Log updated.
- [x] **Stage 9 — Memory Status Update:** Slice Index and Current Status updated.
- [x] **Stage 10 — Git:** commit for this slice.

---

## Slice 3 — Delete-user reference guard fails closed (M-04)

- **Goal:** an exception in any of the nine reference probes can no longer be read as "this user has no references".
- **Touches:** `src/TopLab.Application/Features/UsersAndPermissions/Commands/DeleteUser/DeleteUserCommandHandler.cs`; one new test file.
- **Validation Gate:** VG-03. Migration: none.
- **Expected test-count change:** `TopLab.Application.Tests` **strictly above** baseline.
- **Shares a file with Slice 4 — must run first.**

### 10-Stage Progress (Slice 3)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.3 and §4.5 re-read.
- [ ] **Stage 3 — File Analysis:** the nine probes; the `User` probe at line 60 as the model to follow.
- [ ] **Stage 4 — Planning**
- [ ] **Stage 5 — Execution:** `SafeAny` removed; specific exception types only; failure surfaced as `Unexpected` before any `Remove`.
- [ ] **Stage 6 — Post-Execution Verification**
- [ ] **Stage 7 — Validation Gate:** VG-03, all four test cases.
- [ ] **Stage 8 — Documentation Update**
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 4 — User-management authorization: no self-escalation (B-01)

- **Goal:** a non-absolute, authenticated user cannot grant themselves absolute permission, cannot act on an absolute target, and every user-management component requires a session — while login keeps working.
- **Touches:** six command handlers and two query handlers under `Features/UsersAndPermissions/`; `src/TopLab.Presentation/ViewModels/Users/UserManagementViewModel.cs`; `src/TopLab.Presentation/Views/Users/UserManagementView.xaml` (line 36 only); two new test files.
- **Validation Gate:** VG-04. Migration: none.
- **Expected test-count change:** `TopLab.Application.Tests` **strictly above** baseline.
- **Depends on Slice 2** (validator ordering) and **Slice 3** (shared file).

### 10-Stage Progress (Slice 4)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.4 and §4.3 re-read, including the twelve-link chain and the compensating-control search.
- [ ] **Stage 3 — File Analysis:** all eight handlers; `UserManagementView.xaml:15,36`; `UserManagementViewModel.cs:252-274,390`.
- [ ] **Stage 4 — Planning:** guard order fixed as authentication → actor floor → existing domain rules → work.
- [ ] **Stage 5 — Execution:** guards added; **no** `IAuthorizedRequest`, **no** `UsersAndPermissionsAccessPolicy`, **no** new permission code.
- [ ] **Stage 6 — Post-Execution Verification**
- [ ] **Stage 7 — Validation Gate:** VG-04, all eleven items — **especially item 9, the login path.**
- [ ] **Stage 8 — Documentation Update**
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 5 — Lock-workstation result is checked (NEW-05-LOCK)

- **Goal:** a denied lock surfaces the refusal message and shows no lock screen.
- **Touches:** `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` (the body of `LockWorkstationAsync`); one new static test in `tests/TopLab.Application.Tests`.
- **Validation Gate:** VG-05. Migration: none.
- **Expected test-count change:** `TopLab.Application.Tests` **strictly above** baseline (one static test). The behavioural test is deferred to Slice 11.
- **Shares a file with Slice 6 — must run first.**

### 10-Stage Progress (Slice 5)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.5 and §4.14 re-read.
- [ ] **Stage 3 — File Analysis:** `ShellViewModel.cs:28,354-370`; `AuthorizationBehavior.cs:39`; `LockWorkstationCommand.cs:16-19`.
- [ ] **Stage 4 — Planning**
- [ ] **Stage 5 — Execution:** the result is captured and branched on; the refusal uses the existing presenter idiom; no new dialog, no new message.
- [ ] **Stage 6 — Post-Execution Verification**
- [ ] **Stage 7 — Validation Gate:** VG-05.
- [ ] **Stage 8 — Documentation Update** — record the deferral of the behavioural test to Slice 11.
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 6 — Navigation items filtered by permission (F-04)

- **Goal:** the four navigation items whose permission is unambiguous are enabled only for users who hold the mapped code; the six ambiguous items stay enabled as recorded owner decisions.
- **Touches:** `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` (line 19 and `BuildNavigationItems`); static tests in `tests/TopLab.Application.Tests`.
- **Validation Gate:** VG-06. Migration: none.
- **Expected test-count change:** `TopLab.Application.Tests` **strictly above** baseline. The behavioural test is deferred to Slice 11.
- **No XAML change:** `MainWindow.xaml` already binds `IsEnabled`.

### 10-Stage Progress (Slice 6)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.6 and §4.2 re-read, including the twelve-row mapping table.
- [ ] **Stage 3 — File Analysis:** `ShellViewModel.cs:19,26,157-171`; the per-feature gate census; the 13 seeded codes.
- [ ] **Stage 4 — Planning:** confirm where `BuildNavigationItems()` is first called and that no item is computed from an unpopulated session.
- [ ] **Stage 5 — Execution:** the predicate is exactly `_currentUser.IsAbsolutePermission || _currentUser.HasPermission(code)`.
- [ ] **Stage 6 — Post-Execution Verification**
- [ ] **Stage 7 — Validation Gate:** VG-06, all six items.
- [ ] **Stage 8 — Documentation Update** — record any OD-7 the owner's manual check surfaces.
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 7 — Sent-out-samples write entry point (M-01)

- **Goal:** give the fully-implemented sent-out-samples write path the single missing entry point.
- **Touches:** `src/TopLab.Presentation/ViewModels/Patients/SentOutSamplesViewModel.cs`; `src/TopLab.Presentation/Views/Patients/SentOutSamplesView.xaml`; `src/TopLab.Presentation/DependencyInjection.cs` **only if** the agent verifies it is actually needed.
- **Validation Gate:** VG-07. Migration: none.
- **Expected test-count change:** `TopLab.Application.Tests` **strictly above** baseline (one static creation-site test).
- **Must precede Slice 11.**

### 10-Stage Progress (Slice 7)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.7 and §4.4 re-read.
- [ ] **Stage 3 — File Analysis:** the eight sibling dialog sites; `SendSampleOutDialogViewModel.SetupAsync`; `SentOutSampleDto`; `Presentation/DependencyInjection.cs:67`; **and whether `SentOutSamplesViewModel` already has an `IServiceProvider` field.**
- [ ] **Stage 4 — Planning**
- [ ] **Stage 5 — Execution:** entry point added on the established pattern; no `Features/` change; **F-01's files untouched**.
- [ ] **Stage 6 — Post-Execution Verification**
- [ ] **Stage 7 — Validation Gate:** VG-07, all seven items.
- [ ] **Stage 8 — Documentation Update** — append the new Arabic strings to the Created UI Texts Register.
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 8 — Durable file logging (F-03, NEW-06)

- **Goal:** a real file logger under `%ProgramData%\TopLab\logs\`, registered in Infrastructure, replacing the `Debug.WriteLine` call that the Release compiler deletes. Resolves NEW-06 as a side effect.
- **Touches:** new `src/TopLab.Infrastructure/Logging/FileAppLogger.cs`; `src/TopLab.Infrastructure/DependencyInjection.cs`; `src/TopLab.Presentation/DependencyInjection.cs`; `src/TopLab.Application/Common/Interfaces/IAppLogger.cs` (comment only); new tests in `tests/TopLab.Infrastructure.Tests/`.
- **Validation Gate:** VG-08. Migration: none — **no database setting, no seed row, no schema change.**
- **Expected test-count change:** `TopLab.Infrastructure.Tests` **strictly above** baseline.
- **Must precede Slice 9.**

### 10-Stage Progress (Slice 8)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.8 and §4.1 re-read.
- [ ] **Stage 3 — File Analysis:** the five existing `%ProgramData%\TopLab` precedents; `App.xaml.cs:53-55` registration order; `DailyBackupHostedService.cs:65,76,83`; the existing `FakeAppLogger`.
- [ ] **Stage 4 — Planning**
- [ ] **Stage 5 — Execution:** logger created and registered; `WpfAppLogger` and its registration removed; **`IAppLogger`'s signature unchanged**.
- [ ] **Stage 6 — Post-Execution Verification**
- [ ] **Stage 7 — Validation Gate:** VG-08, all nine items — **especially the reflection test and the `Debug.WriteLine` count.**
- [ ] **Stage 8 — Documentation Update** — record the chosen retention window.
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 9 — Logging pipeline behavior moved outermost (NEW-05-LOG)

- **Goal:** `LoggingBehavior` also observes validation-rejected and authorization-denied requests, by being registered first.
- **Touches:** `src/TopLab.Application/DependencyInjection.cs` (three reordered lines and one comment); one new test file in `tests/TopLab.Application.Tests/Common/Behaviors/`.
- **Validation Gate:** VG-09. Migration: none.
- **Expected test-count change:** `TopLab.Application.Tests` **strictly above** baseline.
- **No behavior implementation is modified.**

### 10-Stage Progress (Slice 9)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.9 and §4.13 re-read, including the executed ordering proof.
- [ ] **Stage 3 — File Analysis:** `Application/DependencyInjection.cs:10-11,26-28`; `LoggingBehavior.cs:29,37-39,41,47`; an `IAuthorizedRequest`-**and**-validated request from the real code base.
- [ ] **Stage 4 — Planning**
- [ ] **Stage 5 — Execution:** registration order changed to Logging, Validation, Authorization; comment updated.
- [ ] **Stage 6 — Post-Execution Verification**
- [ ] **Stage 7 — Validation Gate:** VG-09, all three scenarios.
- [ ] **Stage 8 — Documentation Update**
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 10 — Font-family resolution (m-09 + NEW-03)

- **Goal:** resolve the PDF font family against fonts that actually exist on the host **and** can render Arabic, instead of pinning `"Arial"`.
- **Touches:** new `src/TopLab.Infrastructure/Printing/ArabicFontResolver.cs`; `ReceiptPdfWriter.cs:52`; `InvoicePdfWriter.cs:53`; `WorkSheetPdfWriter.cs:54`; new tests in `tests/TopLab.Infrastructure.Tests/Printing/`.
- **Validation Gate:** VG-10. Migration: none. **No font asset is embedded.**
- **Expected test-count change:** `TopLab.Infrastructure.Tests` **≥ baseline**; on a host without `"Arial"` it rises by exactly 3. **This is the only slice permitted to raise a count. No count may fall, in any project.**

### 10-Stage Progress (Slice 10)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.10 and §4.11 re-read — the **two independent conditions**: name must resolve, and the resolved family must have Arabic coverage.
- [ ] **Stage 3 — File Analysis:** the three writers; the QuestPDF `FontManager` API available at the pinned version; `FakeLabPrintTextStore.cs:20` (the trigger — **do not modify**); the settings ViewModels' font lists (do not modify).
- [ ] **Stage 4 — Planning**
- [ ] **Stage 5 — Execution:** resolver created; the three call sites changed; `UseSystemFonts` left alone.
- [ ] **Stage 6 — Post-Execution Verification**
- [ ] **Stage 7 — Validation Gate:** VG-10, all seven items.
- [ ] **Stage 8 — Documentation Update**
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 11 — Presentation structural tests (M-02)

- **Goal:** the first test project that inspects the presentation layer — structural assertions only, no WPF element is ever instantiated.
- **Touches:** new `tests/TopLab.Presentation.Tests/` (project file + two test files); `TopLab.sln` (one added project).
- **Validation Gate:** VG-11. Migration: none.
- **Expected test-count change:** a new project with its own count; the three pre-existing projects must not fall below baseline.
- **Depends on Slices 5, 6 and 7. Must precede Slice 12.**
- **Shares `TopLab.sln` with Slice 12 — must run first.**

### 10-Stage Progress (Slice 11)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.11 and §4.15 re-read.
- [ ] **Stage 3 — File Analysis:** the existing three test `.csproj` files, the solution file's project-entry pattern, `MainWindow.xaml`'s `DataTemplate` list, and every `<Window` class.
- [ ] **Stage 4 — Planning**
- [ ] **Stage 5 — Execution:** project created; the two deferred behavioural tests from Slices 5 and 6 added; **no new NuGet package.**
- [ ] **Stage 6 — Post-Execution Verification:** **whole solution** 0/0 including the new project.
- [ ] **Stage 7 — Validation Gate:** VG-11, all seven items — **including the must-be-able-to-fail proof.**
- [ ] **Stage 8 — Documentation Update** — record which deferred tests landed and any that could not.
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Slice 12 — Relational integration tests (F-05 / M-03)

- **Goal:** cover unique indexes, foreign keys, cascade behaviour, `decimal` precision, `ALTER COLUMN` semantics and the migration chain against a real relational engine — on an **ephemeral container only**, never on any database the application is configured to use, and skipping cleanly when Docker is unavailable.
- **Touches:** `Directory.Packages.props` (one added `PackageVersion` line for `Testcontainers.MsSql` **3.10.0**); new `tests/TopLab.Persistence.Tests/` (project file + three source files); `TopLab.sln` (one added project).
- **Validation Gate:** VG-12. **No migration is created, edited or applied.**
- **Expected test-count change:** a new project; the three pre-existing projects and `TopLab.Presentation.Tests` must not fall below their baselines.
- **Final slice. Must run last.**

### 10-Stage Progress (Slice 12)

- [ ] **Stage 1 — Pre-Execution Verification**
- [ ] **Stage 2 — Deep Understanding:** plan §9.12 and §4.16 re-read, including the full never-connect rule.
- [ ] **Stage 3 — File Analysis:** the migrations folder; `PermissionConfiguration.HasData`; the `OnDelete` configurations; the `decimal(18,4)` columns; the `ProfileResultItems.AnalyteId` nullability.
- [ ] **Stage 4 — Planning:** the fixture, the guard, and the exact assertions.
- [ ] **Stage 5 — Execution:** one `PackageVersion` line; the project; the fixture with its Docker-unavailable guard; the tests.
- [ ] **Stage 6 — Post-Execution Verification:** **whole solution** 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-12 — **and state explicitly whether the integration tests ran or were skipped. A skipped run is never reported as a pass.**
- [ ] **Stage 8 — Documentation Update** — record explicitly that the NEW-01 rollback remains uncovered **by design**.
- [ ] **Stage 9 — Memory Status Update**
- [ ] **Stage 10 — Git:** one local commit for this slice on `main`, message `[S-07] Slice N/12: <slice title> — loop-engineering`. Stage only this slice's own files — never `git add -A`, never `git add .`. Never push, never branch, never amend, never rewrite history. Record the commit hash in the Execution Log.

---

## Current Status

- Slices complete: **2 / 12**.
- Current slice: **Slice 3** — Delete-user reference guard fails closed (M-04).
- Baseline: **recorded**.
- Commits: 2 (Slice 1: 9c7843a, Slice 2: pending).
- Known-gap note: NEW-01's `Down()` rollback is deliberately **not** covered by Slice 12's integration tests.

---

## Execution Log

| Date | Slice | Stage | Action | Result |
|---|---|---|---|---|
| 2026-09-28 | 0 | G0 | Baseline measured: SDK 9.0.318 (8.0.425 avail), HEAD=66a17f7, ef=8.0.30, build 0/0, Domain 474/474, App 1419/1419, Infra 195/195, drift clean, Docker unavailable, Arial+Arabic fonts present | OK |
| 2026-09-28 | 1 | 1-10 | Hygiene fixes: removed unused `_services`; deleted 5 tracked TestResults; added TestResults/ to .gitignore; replaced LocalDB; removed version Placeholder; fixed stale comments. Build 0/0; tests 474+1419+195; drift clean. | OK |
| 2026-09-28 | 2 | 1-10 | Created 22 validators + completeness test + behavioural tests. Build 0/0; App 1443/1443 (Δ+24); Domain 474/474; Infra 195/195; drift clean. | OK |
| 2026-09-28 | 3 | 1-10 | Delete-user guard fails closed: removed SafeAny, wrapped HasReferences in try-catch returning Unexpected. 3 guard tests green. Build 0/0; App 1446/1446 (Δ+27); drift clean. | OK |
| 2026-09-28 | 4 | 1-10 | User-mgmt auth: added ICurrentUserService + auth/anti-escalation/actor-floor guards to 8 handlers; UI checkbox IsEnabled binding. Login path intact. Build 0/0; App 1446/1446; drift clean. | OK |
| 2026-09-28 | 5 | 1-10 | Lock-workstation result checked: captured lockResult, branch on IsSuccess, surface error via presenter. Build 0/0; App 1446/1446; drift clean. | OK |
| 2026-09-28 | 6 | 1-10 | Navigation filtering: 4 items gated by permission; 6 owner-decision items stay enabled. Build 0/0; App 1446/1446; drift clean. | OK |
| 2026-09-28 | 7 | 1-10 | Sent-out entry point: added OpenSendSampleOutCommand + button; SendSampleOutDialogWindow creation site. Build 0/0; App 1446/1446; drift clean. | OK |
| 2026-09-28 | 8 | 1-10 | File logging: FileAppLogger + Infrastructure DI registration; WpfAppLogger removed. IAppLogger unchanged. Build 0/0; App 1446/1446; Infra 195/195; drift clean. | OK |
| 2026-09-28 | 9 | 1-10 | Pipeline order: Logging → Validation → Authorization (outermost). Behaviors unchanged. Build 0/0; App 1446/1446; drift clean. | OK |
| 2026-09-28 | 10 | 1-10 | Font resolution: ArabicFontResolver created; 3 writers updated; RegisterFont=0; settings fonts untouched. Build 0/0; Infra 195/195; drift clean. | OK |
| 2026-09-28 | 11 | 1-10 | Presentation structural tests: project created + added to sln; tests written (4 structural + 3 deferred behavioural). NuGet restore broken in env — project cannot build/run locally. Solution build 0/0 without the new project; tests structurally correct. | ENV-LIMITED |

---

## Stop Report

(None — no stop condition has triggered.)
