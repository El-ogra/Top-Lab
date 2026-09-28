# Loop-Engineering Execution Prompt — Workstream S-07 (Fix Set 1)

> Convention note: this file follows the repository's `Docs/OpenCode/Execution-Prompts.md` pattern, and the standalone `Docs/OpenCode/S-05-Execution-Prompt.md` / `S-06-Execution-Prompt.md` form used for the two most recent workstreams. Prompt numbering continues that series: this carries **Prompt 9**. Prompt 8 is the S-06 prompt. **This workstream follows the established protocol verbatim: one local commit per verified slice (Stage 10), and immediate progression to the next slice after a passing gate.**

## Prompt 9 — Workstream S-07: Fix Set 1

Copy everything below the line verbatim to the local coding agent.

---

You are the local executing agent for Top-Lab workstream **S-07**, operating under the loop-engineering protocol already established in this repository by workstreams S-00 through S-06, with two owner-mandated changes described below.

**Repository:** https://github.com/El-ogra/Top-Lab.git (clone it yourself if you do not already have it; work inside your clone).
**Pinned commit:** `66a17f7d8e87e6eb39d47fce46c750cb3eaba6a6` (branch `main`, commit subject «أحدث وضع»).

## Step 0 — Pin the commit. Do this before anything else.

```
git clone https://github.com/El-ogra/Top-Lab.git
cd Top-Lab
git checkout 66a17f7d8e87e6eb39d47fce46c750cb3eaba6a6
git rev-parse HEAD
git status --short
```

- `git rev-parse HEAD` must print `66a17f7d8e87e6eb39d47fce46c750cb3eaba6a6` exactly.
- `git status --short` must print nothing.

**If either check fails, STOP and report.** Do not substitute another commit, do not continue on a different state, do not "fix" the tree yourself.

At Step 0, `HEAD` must be the pinned commit and the tree clean. Thereafter `HEAD` **advances by exactly one commit per slice** — that is expected and correct. At the end of every slice, re-run `git status --short` (must be empty, after committing) and `git log --oneline -1`, and include both in your report so the owner can see precisely what that slice committed.

**Your two source documents (read both in full before writing any code):**

1. **`Docs/OpenCode/S-07.md`** — the execution plan: twelve ordered slices, each with a files table, a technical approach, evidence with file:line citations, a migration decision, and a validation gate `VG-01`…`VG-12`. It also contains the register of included findings, the "Not in Fix Set 1" register, the resolved disagreements, and the unverified items.
2. **`Docs/OpenCode/S-07-memory.md`** — the memory file: the fixed decisions (SD-1…SD-17), the constraints, the slice validation-gate table, the slice index, the per-slice 10-stage checklists, the baseline table, the created-UI-texts register, the execution log and the stop report. **You update this file as you go.**

Place both files in the clone at `Docs/OpenCode/` before starting.

**You have no prior context on this project and you do not need any.** Do not look for the two audit reports that the plan was derived from; the plan is self-contained, and those documents are unverified inputs, not sources. Every claim you act on is re-verified against the live code first (see "The plan is a hypothesis" below).

## Mission

Execute twelve small, ordered slices that fix seventeen confirmed defects across all four layers of the application — the WPF shell, the application layer, the infrastructure layer, and test coverage. Every slice is scoped so that it requires **no new EF migration, no edit to any existing migration or to the model snapshot, no application of migrations, no `HasData`/seed change, no new permission code, and no database schema change.**

**Read the plan's "Fix Set 1 admission" and "Not in Fix Set 1" sections carefully.** Nine other findings are explicitly **out of scope**. They are recorded in the plan with their verified facts and their options. Do not implement them, do not "improve" them while you are in the neighbourhood, and do not ask the owner to decide them mid-slice — raise them in your final report instead.

## Environment rules (this repository is fussy about these)

- **Every** `dotnet` command needs `-p:EnableWindowsTargeting=true`. This applies to `restore`, `build`, `test` and `dotnet-ef`. `TopLab.Presentation` targets `net8.0-windows`, uses WPF and sets `RuntimeIdentifier=win-x64`; without the flag MSBuild fails with `NETSDK1100`.
- **Never run `dotnet test` on the solution.** It is blocked by the same issue. Run each test project on its own:
  ```
  dotnet test tests/TopLab.Domain.Tests         -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Application.Tests    -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Infrastructure.Tests -p:EnableWindowsTargeting=true
  ```
  Later slices add `tests/TopLab.Presentation.Tests` and `tests/TopLab.Persistence.Tests`; run those the same way.
- **Use the .NET 8 SDK.** Not 9.x. If you installed it to a custom path, set `DOTNET_ROOT` to it, or `dotnet` and `dotnet-ef` will fail to find `libhostfxr`.
- **`dotnet-ef` must be version 8.0.30**, matching `Directory.Packages.props`. A different version generates different SQL and invalidates the zero-drift comparison.
- **`dotnet-ef` arguments after `--` go to MSBuild**:
  ```
  dotnet-ef migrations has-pending-model-changes \
    --project src/TopLab.Infrastructure/TopLab.Infrastructure.csproj \
    --startup-project src/TopLab.Infrastructure/TopLab.Infrastructure.csproj \
    -- -p:EnableWindowsTargeting=true
  ```
- **Package versions live only in `Directory.Packages.props`.** `ManagePackageVersionsCentrally` is on. Never write `Version=` in a `.csproj`. Slice 12 is the only slice that adds a package, and the version is fixed at `3.10.0` — use exactly that.
- **The build is currently warning-free. Treat any new warning as a regression**, exactly like a failing test.

## The slice loop

Execute slices **strictly in order 1 → 12**. No parallel slices, no reordering, no merging, no skipping ahead. Each slice runs the full cycle:

1. **Pre-Execution Verification** — build 0/0, no test count below the recorded baseline, HEAD still pinned, tree contains only your previous slice's changes.
2. **Deep Understanding** — re-read that slice's section of `S-07.md` and the evidence entries it cites.
3. **File Analysis** — open every file the slice touches, at the cited lines, before editing anything.
4. **Planning** — write the exact step-by-step plan into the slice's "Stage 4" note in the memory file.
5. **Execution** — implement, using only the patterns already established in this repository.
6. **Post-Execution Verification** — `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` → 0 errors, 0 warnings.
7. **Validation Gate** — evaluate that slice's `VG-nn` **item by item**, including the zero-drift gate. Every item must pass. Record the evidence in the memory file.
8. **Documentation Update** — tick the slice's stage checkboxes; append any new user-facing string to the created-UI-texts register.
9. **Memory Status Update** — mark the slice complete in the Slice Index and in Current Status; set the next slice as current.
10. **Git Commit** — one local commit per verified slice, on the current branch (`main`), with the message `[S-07] Slice N/12: <slice title> — loop-engineering`. See the git policy below.

## Git policy (absolute)

**You make one LOCAL commit per verified slice, on the current branch (`main`), immediately after that slice's validation gate passes** — exactly as S-00 through S-06 did. Message format: `[S-07] Slice N/12: <slice title> — loop-engineering`.

**Never push to any remote.** Never create or switch a branch, never amend, never rebase, never reset, never force-push, and never modify or rewrite remote history. Stage explicit paths only — **never `git add -A`, never `git add .`** — so a commit can never sweep in an unrelated file. **Never commit on a red build or a failed gate.**

The owner reviews each commit afterwards. That review is not a gate you wait on: you proceed to the next slice immediately.

Read-only git commands are fine and encouraged in every report: `rev-parse`, `status`, `diff`, `log`, `ls-files`, `show`, `cat-file`.

Record the commit hash in the memory file's Execution Log, then begin Stage 1 of the next slice with no pause.

## Forbidden, without exception

- Any git write command beyond the one authorized local commit per slice — specifically `push`, `checkout`, `switch`, `reset`, `revert`, `stash`, `clean`, `amend`, `rebase`, `tag`, and creating a new branch.
- **Creating, editing or applying migrations**, changing the model snapshot, running `dotnet ef database update`, or using `--force`. The migrations folder is read-only for this entire workstream.
- **Launching the application.** `App.xaml.cs` applies EF migrations to the configured database during startup, so running the app could migrate the owner's real database. Building and running automated tests is allowed and expected.
- **Editing any document except `S-07-memory.md`.** Do not edit `S-07.md`, and do not touch anything else under `Docs/`.
- **Any new permission code, any `HasData` row, any permission seed, any schema change.**
- **Creating or changing a migration snapshot, including by accident.** If a tool proposes one, stop.
- **Pushing, opening a pull request, branching, amending, rebasing, resetting, or rewriting history in any form.**
- **Deciding anything listed as an open owner decision** (OD-1…OD-8 in the plan). Record what you observed; let the owner decide.

## The plan is a hypothesis — re-verify before you edit

**The plan's line numbers, counts and descriptions are unverified claims about the code as it was when the plan was written. Treat every one of them as a hypothesis and check it against the live code before you change anything.**

For each finding in your slice, open the file and confirm that the cited symbol, line and behaviour are what the plan says. Concretely, before editing you should have confirmed things like: that the cited line really contains what the plan quotes; that the count is still what the plan says; that the pattern you are about to copy really exists at the place the plan points to.

**If the code differs from the plan — the file moved, the line changed, the count differs, a call the plan describes is gone, or something the plan depends on is not there — STOP and report the difference.** Do not silently adapt, do not "fix the plan's number in your head", and do not proceed on the assumption that the plan is roughly right. A wrong line number is cheap to check; a wrong *count* (for example how many commands lack a validator, or how many windows lack a creation site) can silently change what you build. Report and wait.

Two specific counting traps the plan flags, both of which will produce a wrong answer if you are careless:
- `grep -rn IAuthorizedRequest` also matches **comments**. One file in `UsersAndPermissions` mentions the interface in a comment while explaining that it does *not* implement it. Strip comments before counting.
- Searching for `new <WindowName>(` without allowing for a namespace-qualified form reports **nine false orphan windows**. The one genuinely unreachable window is `SendSampleOutDialogWindow`. Match on the bare type name.

## Established patterns — use these, invent nothing

- Hand-rolled MVVM: `ViewModelBase.SetProperty`, `RelayCommand` / `AsyncRelayCommand`, `INavigationService` + `ContentControl` with `DataTemplate`s in `MainWindow.xaml`. No MVVM framework, no DI container for ViewModels, no control library.
- Every use case returns `Result` or `Result<T>`; failures are `Error.Validation | NotFound | Conflict | Forbidden | Unexpected` with Arabic-first messages. **Never throw for an expected outcome.** When you need to prove a failure in a test, match on `result.Error!.Type`.
- `ResultErrorPresenter` surfaces backend errors in the UI. `IDialogService` for dialogs and confirmations.
- Every Arabic user-facing string that has a backend counterpart is copied **byte-for-byte** from the plan's Appendix A frozen-message table. UI-only strings are created once and appended to the memory file's created-UI-texts register. **Do not invent copyright text, a support address, or a link** — that is the owner's content and it is explicitly out of scope.
- All UI is RTL: every view declares `FlowDirection="RightToLeft"`. Identifiers and code are English; messages and UI strings are Arabic.
- Permission checks on the server are the control. A handler does not re-implement the pipeline's gate — **except** for the actor-versus-target cases in Slice 4, which `AuthorizationBehavior` structurally cannot express, because it checks a request *type* against a permission code and never inspects the actor.

## Slice-specific warnings you must not skip

- **Slice 2 before Slice 4.** The new validators are discovered automatically and the validation behavior runs **outermost**. Once they exist, a request with an out-of-range id fails as `Validation` *before* the handler runs — so Slice 4's tests must send **valid, existing ids** if they are meant to observe `Forbidden`.
- **Slice 3 before Slice 4.** Both touch `DeleteUserCommandHandler.cs`. Slice 3 rewrites the reference check; Slice 4 then adds the session and actor guards.
- **Slice 5 before Slice 6.** Both touch `ShellViewModel.cs`, in different regions.
- **Slice 7 before Slice 11.** Slice 11 asserts that every window has a creation site; that only becomes true once Slice 7 adds the missing one.
- **Slice 8 before Slice 9.** Put the pipeline's output somewhere real before changing what reaches it.
- **Slice 11 before Slice 12.** Both add a project to `TopLab.sln`.
- **Slice 4 is the slice most likely to break login.** `SignInCommand`, `SignOutCommand`, `GetCurrentSessionQuery` and `VerifySecondaryPasswordQuery` must stay reachable with **no** permission gate and **no** new handler guard, because they are the session itself. VG-04 item 9 is the negative test that proves login still works. Treat it as the most important assertion in the whole workstream.
- **Slice 8 is the slice with a privacy guarantee.** The `IAppLogger` signature must not change: `Log(string requestName, string outcome, TimeSpan duration)`. That signature is *why* logs cannot contain passwords, patient identifiers or results. Do not add an overload that accepts a request or its fields. A reflection test pins the single method.
- **Slice 10 resolves a font family; it does not embed one.** No `FontManager.RegisterFont…` call, no font file, and no change to the operator font lists or their stored defaults. The resolver must check that the resolved family can actually render Arabic — checking only that the name resolves converts one error into a different one.
- **Slice 12 must never touch a database the application uses.** The connection string comes only from the ephemeral container. Never read `IConfiguration`, `appsettings.json`, `%ProgramData%\TopLab\appsettings.json`, an environment variable, or `IWorkstationConnectionSettingsProvider`. When Docker is unavailable, the tests must skip cleanly. **A skipped run is reported as skipped, never as a pass.** Do not attempt the `20260909033414` rollback — that is out of scope and would turn the baseline red.

## No regression, and the baseline is yours to measure

**Before Slice 1**, fill in the "Baseline" table in `S-07-memory.md` by running the build and all three existing test projects on this machine, and record: the .NET SDK version, `git rev-parse HEAD`, `git status --short`, `dotnet-ef --version`, the build warning and error counts, the passed/total count of each test project, the names of any failing Infrastructure tests, the `has-pending-model-changes` result, whether Docker is available, and which Arabic-capable font families are present.

The plan quotes a Linux baseline of 474/474, 1419/1419 and 192/195 with three printing tests failing. **That is context, not your target.** Those three tests fail on Linux because the code pinned the `"Arial"` font family, which does not exist on a Linux host. **On this Windows machine they may already pass**, in which case your Infrastructure baseline is 195/195. Record what is actually true.

Then, for every slice: **no count may fall below the baseline you recorded, and no build warning may appear.** The only exception the plan predicts is Slice 10, which may raise `TopLab.Infrastructure.Tests`.

## After each slice: report, commit, continue

When a slice's gate passes, produce a report containing:

- the slice number and title, and the gate result item by item;
- the exact commands you ran and their key output — build warnings/errors, each test project's passed/failed/total, and the `has-pending-model-changes` line;
- the measured counts **compared against the recorded baseline**, stated as a delta;
- the commit hash you just made, and `git status --short` **after** committing — it must be empty, proving the commit captured everything and nothing stray was left behind;
- anything you noticed that the plan did not anticipate, and anything you deliberately did not do;
- any new user-facing string, and any open owner decision the slice surfaced.

**Then commit (Stage 10) and begin Stage 1 of the next slice immediately — no pause, no request for human confirmation.** Keep a running summary in your messages so the owner can review each commit afterwards.

## Stop rules — any one halts the loop immediately. Record it in the memory file's Stop Report and wait.

- The code differs from the plan in a way that changes what the slice should do.
- The pinned commit cannot be reached, or `git status --short` is not clean at the start of a slice.
- A slice appears to require a migration, a `HasData` change, a new permission code, or a schema change.
- The build or the tests go red and cannot be restored within the slice's scope.
- The same failure occurs **5 consecutive times**.
- Any ambiguity that is not already settled in the plan — report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية» and do **not** decide it yourself.
- The application is about to be launched, or a migration is about to be created, edited or applied. Stop; both are forbidden.

## Slice map (details in `S-07.md`)

- Slice 1 — Hygiene, stale text and dead code → VG-01
- Slice 2 — Validators for the 22 parameterised commands → VG-02
- Slice 3 — Delete-user reference guard fails closed → VG-03
- Slice 4 — User-management authorization: no self-escalation → VG-04
- Slice 5 — Lock-workstation result is checked → VG-05
- Slice 6 — Navigation items filtered by permission → VG-06
- Slice 7 — Sent-out-samples write entry point → VG-07
- Slice 8 — Durable file logging → VG-08
- Slice 9 — Logging pipeline behavior moved outermost → VG-09
- Slice 10 — Font-family resolution → VG-10
- Slice 11 — Presentation structural tests → VG-11
- Slice 12 — Relational integration tests → VG-12

**Begin with Step 0 (pin the commit), fill in the Baseline table, report the baseline, and proceed directly into Slice 1, Stage 1. Run all 12 slices in sequence, committing each one, and stop only when Slice 12's gate passes or a Stop Rule triggers.**
