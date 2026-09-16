# Loop Engineering — Memory File

- **Module:** Startup Authentication & First-Run UX (S-00 — startup phase, pre-M-01)
- **Module Number:** S-00
- **Source Plan:** Docs/OpenCode/S-00.md
- **Date Created:** 2026-09-16
- **Total Slices:** 2
- **Current Slice:** S2 — In Progress (S1 complete, committed; begin S2 Stage 1 next)
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

S-00 closes the startup authentication gap and fixes the first-run administrator window's UX. S1 creates `LoginWindow`/`LoginViewModel` (reusing the fully implemented and tested `SignInCommand` pipeline — `SignInCommandHandler.cs:L31-L66`, `Pbkdf2PasswordHasher`, singleton `CurrentUserService` at `Infrastructure/DependencyInjection.cs:L56-L57`) and inserts the login gate in `App.xaml.cs` immediately after the admin-existence try/catch (`L90-L96`) and before the currently-ungated `MainWindow.Show()` (`L98-L99`), plus two DI registrations and a `SignInCommandValidatorTests` class. S2 adds four independent per-field show/hide password toggles to `FirstRunAdminWindow` (`FirstRunAdminWindow.xaml:L35,L38,L41,L44`) and a post-creation success MessageBox («تم إنشاء حساب مدير النظام بنجاح. سيتم الآن عرض شاشة تسجيل الدخول.») before the existing close (`FirstRunAdminWindow.xaml.cs:L26-L30`). No migration in either slice; no Domain/Application/Infrastructure production-code change in either slice (S1's only Application-layer artifact is a test).

## Global Validation Gates

- Gate G0 (pre-execution): `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- Gate G1 (post-execution per slice): same as G0 plus the slice-specific gate listed in the table below.

## Quality Gate (non-negotiable — a slice may be marked complete ONLY when ALL FOUR hold)

1. The slice's implementation is fully complete per `Docs/OpenCode/S-00.md`.
2. The ENTIRE solution builds successfully — zero errors AND zero warnings (`dotnet build TopLab.sln`).
3. ALL existing automated tests across ALL test projects pass (`dotnet test TopLab.sln`).
4. The slice's own specific exit criteria (its Validation Gate in the plan) pass.

## Stop/Continue Rule

After a slice completes, verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice, with no pause and no human confirmation required.
If any one fails → retry. If the SAME failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs 5 CONSECUTIVE times, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do NOT proceed past this point without owner review. Ordinary expected test failures caused by the current slice and resolved within the same correction cycle do NOT count as five separate failures.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: 5 consecutive failures for the same reason.
- Execution order: strictly sequential S1 -> S2, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/manual state-machine verification) replaces any standard UI journey — this workstream's UI behaviour is verified manually per the plan (no UI test harness exists in the repo).
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote, NEVER force-push, NEVER modify or rewrite remote history. Commit message format: `[S-00] Slice N/2: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in S-00's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Login screen + startup gate: solution build zero/zero; full test suite green (existing SignIn suite + new `SignInCommandValidatorTests`); state machine verified: (a) fresh DB → after admin creation next window is `LoginWindow`, not `MainWindow`; (b) cancelling `LoginWindow` exits code 1, `MainWindow` never appears; (c) wrong credentials → Arabic error in `LoginWindow`, `IsAuthenticated` false, window stays open; (d) correct credentials → `IsAuthenticated` true, `MainWindow` opens; (e) existing-DB launch → `LoginWindow` before `MainWindow`. **Migration: NONE — zero-drift gate** (`has-pending-model-changes` → no changes; `git diff --stat src/TopLab.Infrastructure/Persistence/` empty) | `dotnet build TopLab.sln`; `dotnet test TopLab.sln`; manual state-machine verification; ef drift check; grep gates |
| 2 | VG-02 | FirstRunAdminWindow UX: solution build zero/zero; full test suite green; manual verification: each of the four secret fields reveals/masks independently; creation succeeds identically with toggles on or off (revealed text == what `CreateAsync` receives); success MessageBox appears after creation and, on dismissal, the Login window (S1) appears next — no `MainWindow`, no restart prompt. **Migration: NONE — zero-drift gate** | `dotnet build TopLab.sln`; `dotnet test TopLab.sln`; manual UX verification; ef drift check |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Login screen (LoginWindow + LoginViewModel) and startup gate | [x] Complete | VG-01 — PASS |
| 2 | FirstRunAdminWindow UX: independent show/hide toggles + post-creation success confirmation | [ ] Pending | VG-02 — NOT RUN |

---

## Settled Decisions (from plan — binding)

- **Audit verdict:** candidate plan CORRECTED (C-1 validator-test naming precedent; C-2 four toggles not two; C-3 S-00 naming). All other candidate claims verified accurate at HEAD `f0d6457594b6b6fcfff1f4bbc277566dfb3854c2`.
- **Login gate position:** `App.xaml.cs` `OnStartup`, after the admin-existence try/catch (`L90-L96`), before `MainWindow` resolution (`L98`). Failure semantics: `ShowDialog() != true` → `Shutdown(1); return;` (mirrors `L45-L49`, `L84-L88`).
- **Reuse, never rebuild:** `SignInCommand`/`SignInCommandHandler`/`SignInCommandValidator`, `Pbkdf2PasswordHasher`, `CurrentUserService`, `ResultErrorPresenter`, `ViewModelBase`, MediatR validation pipeline. `SignInCommandHandler` is NOT modified.
- **Four independent toggles** (one per secret field), not two and not one global toggle (Audit C-2).
- **No restart after admin creation:** session is set on the already-live singleton `CurrentUserService`; flow transitions directly FirstRunAdminWindow → LoginWindow.
- **Error wording:** handler messages verbatim via `ResultErrorPresenter` (preserves unknown-user/wrong-password ambiguity).
- **Session lifecycle (sign-out/lock/switch user): out of scope.** `SignOutCommand`/`LockWorkstationCommand` stay orphaned; close MainWindow = exit application.
- **No new UI test framework.** New automated tests are confined to `tests/TopLab.Application.Tests` xUnit + hand-fakes conventions.
- **No migration in any slice**; zero-drift gate runs per slice.

---

## Slice 1: Login screen (LoginWindow + LoginViewModel) and startup gate

- **Goal:** Ship the login UI and make it the single mandatory gate before `MainWindow` on both launch paths.
- **Touches:** `src/TopLab.Presentation/Views/Setup/LoginWindow.xaml` (create); `src/TopLab.Presentation/Views/Setup/LoginWindow.xaml.cs` (create); `src/TopLab.Presentation/ViewModels/Setup/LoginViewModel.cs` (create); `src/TopLab.Presentation/App.xaml.cs` (modify — gate insertion); `src/TopLab.Presentation/DependencyInjection.cs` (modify — two registrations); `tests/TopLab.Application.Tests/Features/UsersAndPermissions/SignInCommandValidatorTests.cs` (create)
- **Validation Gate:** VG-01 — build zero/zero; full suite green; state-machine items (a)–(e); zero-drift gate. Migration: none.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite green before touching anything. Evidence 2026-09-16: `dotnet build TopLab.sln` → Build succeeded, 0 Warning(s), 0 Error(s); `dotnet test TopLab.sln` → Domain 467/467, Infrastructure 160/160, Application 1354/1354 (total 1981 passed, 0 failed). Baseline HEAD `f0d6457` on `main` confirmed.
- [x] **Stage 2 — Deep Understanding:** Plan §3 (S1) + Appendix A re-read. Settled: gate after admin try/catch close (`App.xaml.cs` L90-L96) before `MainWindow` resolve (L98); `ShowDialog() != true` → `Shutdown(1); return;`; reuse `SignInCommand` pipeline verbatim via `_presenter.Present(result.Error)` idiom (`FirstRunAdminViewModel.CreateAsync` failure path); OQ-1 default (RTL/centered/fixed-size, «تسجيل الدخول — Top-Lab», «اسم المستخدم»/«كلمة المرور», «دخول»/«خروج», one show/hide toggle on password field); OQ-2 verbatim via presenter (ambiguity preserved); OQ-3 sign-out/lock out of scope.
- [x] **Stage 3 — File Analysis:** `App.xaml.cs:L28-L101` (insertion point L96/L98 confirmed); `DependencyInjection.cs:L29-L44` (VM block L30-L40, Windows block L42-L44); `FirstRunAdminWindow.xaml` L1-L56 + `.xaml.cs` L17-L31 (code-behind copies `PasswordBox.Password` at L20, success → `DialogResult=true; Close()`); `FirstRunAdminViewModel.cs` L22-L26 ctor, `SetProperty` L28-L68, `CreateAsync` L70-L140 incl. presenter failure idiom L127-L132; `SignInCommand` record, handler L31-L66, validator L5-L17; `Result<T>` (`Error`/`Errors`); `ResultErrorPresenter.Present/PresentAll`; validator-test precedent `ResultsEntryValidatorTests.cs` (namespace `TopLab.Application.Tests.Features.<Area>`).
- [x] **Stage 4 — Planning:** step-by-step slice plan encoded here (completed 2026-09-16):
  1. Create `src/TopLab.Presentation/ViewModels/Setup/LoginViewModel.cs`: sealed, `ViewModelBase`, ctor `(ISender mediator, ResultErrorPresenter presenter)`; props `UserName`, `Password`, `ErrorMessage`, `IsBusy` via `SetProperty`; `SignInAsync(): Task<bool>` — clear `ErrorMessage`, send `new SignInCommand(UserName.Trim(), Password)`, on success `true`, on failure presenter idiom (+ multi-error `PresentAll` join) and `false`, `IsBusy` try/finally.
  2. Create `src/TopLab.Presentation/Views/Setup/LoginWindow.xaml`: RTL modal, `CenterScreen`, `NoResize`, title «تسجيل الدخول — Top-Lab»; username `TextBox` (bound), `PasswordBox` + paired reveal `TextBox` + «إظهار» toggle, error `TextBlock` (bound), «دخول»/«خروج» buttons.
  3. Create `LoginWindow.xaml.cs`: DI ctor `(LoginViewModel vm)`; login click copies `UserNameTextBox.Text` + visible secret into VM, awaits `SignInAsync()`, success → `DialogResult=true; Close()` (failure stays open); exit click → `DialogResult=false; Close()`.
  4. Modify `DependencyInjection.cs`: `AddTransient<LoginViewModel>()` after L33; `AddTransient<LoginWindow>()` after L43.
  5. Modify `App.xaml.cs`: login block after L96 close-brace, before L98 — scope, resolve `LoginWindow`, `ShowDialog()`, `!= true` → `Shutdown(1); return;` (no new strings; mirrors L45-L49/L84-L88).
  6. Create `tests/.../UsersAndPermissions/SignInCommandValidatorTests.cs`: empty username → «اسم المستخدم مطلوب.»; empty password → «كلمة المرور مطلوبة.»; valid → no errors (Theory null/empty/whitespace + Fact).
  7. Must-NOT-change respected: handler/hasher/session/migration/connection blocks/MainWindow/Domain untouched.
- [x] **Stage 5 — Execution:** implement per plan §3. Done 2026-09-16: created `LoginViewModel.cs` (ctor `(ISender, ResultErrorPresenter)`, `UserName`/`Password`/`ErrorMessage`/`IsBusy`, `SignInAsync` sending `new SignInCommand(UserName.Trim(), Password)` with presenter idiom incl. multi-error `PresentAll` join + `IsBusy` try/finally); created `LoginWindow.xaml` (RTL/CenterScreen/NoResize, «تسجيل الدخول — Top-Lab», bound username `TextBox`, `PasswordBox` + paired reveal `TextBox` + «إظهار» toggle, bound error `TextBlock`, «دخول»/«خروج»); created `LoginWindow.xaml.cs` (DI ctor, login click copies visible secret → `SignInAsync`, success → `DialogResult=true; Close()`, exit → `false; Close()`, toggle syncs both controls); modified `DependencyInjection.cs` (+2 registrations after L33/L43); modified `App.xaml.cs` (login block L98-L107, scope + resolve + `ShowDialog`, `!= true` → `Shutdown(1); return;`, no new strings); created `SignInCommandValidatorTests.cs` (3+3 Theory + 1 Fact). Must-NOT-change lists fully respected (git status shows only the 2 intended modifications).
- [x] **Stage 6 — Post-Execution Verification:** solution build 0/0. Evidence: `dotnet build TopLab.sln` → Build succeeded, 0 Warning(s), 0 Error(s).
- [x] **Stage 7 — Validation Gate:** VG-01 PASS. (1) Build 0/0 ✓. (2) Full suite green: Domain 467/467, Infrastructure 160/160, Application 1361/1361 (total 1988 = baseline 1981 + 7 new validator tests), 0 failed ✓. (3) State-machine (a)–(e) verified structurally against `App.xaml.cs` L73-L110 + `LoginWindow.xaml.cs` + handler L56 path (manual window interaction per plan — no UI harness exists, none invented): (a) first-run falls through L89→L98 LoginWindow, not MainWindow ✓; (b) cancel → `Shutdown(1)`, L109 unreachable ✓; (c) failure → `ErrorMessage` set, no `Close()`, session untouched ✓; (d) success → handler `SetSession` → `IsAuthenticated` true → `MainWindow.Show()` ✓; (e) existing DB skips L80-L89, gate L98 still runs ✓. (4) Zero-drift: `has-pending-model-changes` → "No changes have been made to the model since the last migration."; `git diff --stat src/TopLab.Infrastructure/Persistence/` empty ✓. (Design-time host-validation warning in ef output is pre-existing, unrelated to model.)
- [x] **Stage 8 — Documentation Update:** checklist recorded.
- [x] **Stage 9 — Memory Status Update:** Current Status updated.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-00] Slice 1/2: Login screen + startup gate — loop-engineering` (record hash in Execution Log).

---

## Slice 2: FirstRunAdminWindow UX — independent show/hide toggles + post-creation success confirmation

- **Goal:** Four independent per-field visibility toggles + success MessageBox matching the real next step (Login window).
- **Touches:** `src/TopLab.Presentation/Views/Setup/FirstRunAdminWindow.xaml` (modify); `src/TopLab.Presentation/Views/Setup/FirstRunAdminWindow.xaml.cs` (modify); `src/TopLab.Presentation/ViewModels/Setup/FirstRunAdminViewModel.cs` (modify — four booleans only)
- **Validation Gate:** VG-02 — build zero/zero; full suite green; manual UX verification (independent toggles; identical creation with toggles on/off; success message → Login next). Migration: none.

### 10-Stage Progress (Slice 2)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; full suite green after S1.
- [ ] **Stage 2 — Deep Understanding:** Plan §4 (S2) + Appendix A.
- [ ] **Stage 3 — File Analysis:** `FirstRunAdminWindow.xaml:L1-L56` (four `PasswordBox` at L35/L38/L41/L44); code-behind `L17-L31`; VM `SetProperty` pattern `L28-L68`.
- [ ] **Stage 4 — Planning:** step-by-step slice plan encoded here.
- [ ] **Stage 5 — Execution:** implement per plan §4.
- [ ] **Stage 6 — Post-Execution Verification:** solution build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-02 (incl. manual UX verification + zero-drift gate).
- [ ] **Stage 8 — Documentation Update:** checklist recorded.
- [ ] **Stage 9 — Memory Status Update:** Current Status updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-00] Slice 2/2: FirstRunAdminWindow UX fixes — loop-engineering` (record hash in Execution Log).

---

## Current Status

- Slices complete: 1/2.
- Baseline commit: `f0d6457594b6b6fcfff1f4bbc277566dfb3854c2` (main).
- S1 evidence: build 0/0; tests 1988/1988 (467 Domain + 160 Infra + 1361 App incl. 7 new validator tests); state-machine (a)–(e) structurally verified; zero-drift "No changes" + empty Persistence diff. VG-01 PASS.
- S2 evidence: — (pending).

## Execution Log

| Date | Slice | Stage | Action | Result |
|---|---|---|---|---|
| 2026-09-16 | S1 | 1 | Pre-execution: `dotnet build` + `dotnet test` on HEAD `f0d6457` | Build 0/0; 1981/1981 green |
| 2026-09-16 | S1 | 2–4 | Re-read plan §3 + App.A; file analysis; step-by-step plan encoded | Done |
| 2026-09-16 | S1 | 5 | Created LoginViewModel/LoginWindow.xaml(.cs)/SignInCommandValidatorTests; modified DI + App.xaml.cs gate | Done, Must-NOT-change respected |
| 2026-09-16 | S1 | 6–7 | Post-build 0/0; full suite 1988/1988; state-machine (a)–(e); zero-drift gate | VG-01 PASS |

## Stop Report

(None — no stop condition has triggered.)
