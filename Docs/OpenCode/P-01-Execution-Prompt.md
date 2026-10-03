# Loop-Engineering Execution Prompt — Workstream P-01 (Parity Wave 1)

> Convention: standalone prompt per workstream (S-05 / S-06 / S-07 / W-01 / W-02 pattern). **Copy everything below the line verbatim to the local executing coding agent.**

---

You are the local executing coding agent for Top-Lab workstream **P-01** (Parity Wave 1), under the loop-engineering protocol used by S-00…S-07, W-01 and W-02.

**Repository:** work inside the local clone of Top-Lab (`main`).
**Pinned commit:** `9d042d06e56093e480d1c04375fd6b10d5caf23c` («تحقق أول مره»).

## Step 0 — Pin the baseline

```
git rev-parse HEAD
git status --porcelain
```

`git rev-parse HEAD` must print `9d042d06e56093e480d1c04375fd6b10d5caf23c`, or a descendant whose diff is **only** the three P-01 package files under `Docs/OpenCode/`: `P-01.md`, `P-01-memory.md`, `P-01-Execution-Prompt.md`. `git status --porcelain` must otherwise be empty. **If not → STOP and report.**

At the start of every slice, re-run both and include them in your report. At the end of every slice, re-run `git status --porcelain` (empty after committing) and `git log --oneline -1`, so the owner can see exactly what that slice committed.

## Source documents (read both fully before any code)

1. **`Docs/OpenCode/P-01.md`** — the wave plan: §0 binding decisions SD-1…SD-11, §1 plan-vs-code corrections C-1…C-8, §2 the dependency-ordered slice map, §3–§8 the six slices each with a scope file table, an implementation approach and a validation gate `VG-01`…`VG-06`, §9 general gates, §10 baseline, §11 stop conditions, §12 out of scope, §13 wave DoD.
2. **`Docs/OpenCode/P-01-memory.md`** — the living memory: SD/C tables, confirmed code facts, the G0 baseline table, the slice index, per-slice 10-stage checklists, the created-UI-texts register, the recorded assumptions, the execution log and the stop report. **You update this file as you go.**

**No other module file is required.** If a fact is not in these two files and not in the live code, stop and report. Do not go hunting through `Docs/` for audit reports; they are unverified inputs, not sources.

## Mission

Bring **nine** reference-system functions to parity with the reference laboratory system, in **six slices**, with **zero migrations, zero schema changes and zero new packages**:

| ID | Function | Produces |
|---|---|---|
| F1 | Search by **treating doctor** | **2 controls** — treating doctor **and** referral entity |
| F2 | Search by **test** | 1 control |
| F3 | Search by **gender** | 1 control |
| F4 | Search by **age** | 1 control |
| F5 | Search by **date range** | 1 control |
| F6 | Worklist filter: **results not printed** | 1 checkbox |
| F7 | Worklist filter: **results not verified** in the UI | 1 checkbox |
| F8 | **Print the test price list** | 1 print command |
| F9 | **Print the custom test-group list** | 1 print command |

**Nine functions → 6 search controls + 2 worklist checkboxes + 2 print commands.**

**The reference system** for these functions is the RealLab system, described in three manuals owned by the project owner: `RLS_Learn_Enhanced.pdf`, `RL_Show_Enhanced.pdf` and `Real_Lab_System_Reference.pdf`. The behaviours you are matching are, briefly: a patient-search window that can restrict a result set to a time period, an age stage, a gender, or a referral entity (`REF3` p.34 §1) and search by name, phone, national ID or a patient code (`REF3` p.35); a results window carrying per-test marks for entered / reviewed / printed / delivered and seven patient status symbols (`REF3` p.22, p.31); and a printed price list (`RLS` p.111) and a printed custom test-group list (`RLS` p.121). The plan cites the page for every function; follow the plan, not this summary.

**Not in this wave.** One further reference-parity function was **deferred by owner decision** because its evidentiary basis is too weak to justify building now. It is **not** part of P-01: do not build it, do not stub it, do not add a placeholder, do not add a disabled button for it. Later-wave work (barcode and label production; results-domain and report rendering) is **not** described in this package and must not be started. If you believe a slice needs something from it, that is a STOP, not a licence to build it.

## Binding decisions — do NOT reopen (P-01.md §0)

- **SD-1** the plan is a hypothesis: live code contradicting it ⇒ **STOP and report**.
- **SD-2** git: one **local** commit per verified slice. **Never push.** No branch, amend, rebase, reset, stash, clean, tag, force-push. **Never `git add -A` / `git add .`** — stage explicit paths only. Never commit on a red build or failed gate. The owner pushes after the wave.
- **SD-3** **zero migrations.** `Migrations/` and `ApplicationDbContextModelSnapshot.cs` are read-only for this entire wave. A slice that appears to need a migration is a STOP condition.
- **SD-4** **zero packages.** `Directory.Packages.props` and every `.csproj` stay untouched. QuestPDF and ZXing.Net are already pinned and are sufficient. Never write `Version=` in a `.csproj`.
- **SD-5** the search-query guardrails below. All five clauses are binding.
- **SD-6** **six search filters, not five.** `TreatingDoctorId` and `ReferralEntityId` are two different columns and two different concepts. Do not collapse them.
- **SD-7** the display cap is `PageSize` (default 50) and is **unchanged**. The reference's 100 is not adopted and no 100 literal is introduced.
- **SD-8** do not change `EnablePatientNameSearchAssist` — its meaning, default or gating.
- **SD-9** no new backend error message in this wave. New **UI-only** strings are created once and appended to the register. Never invent copyright text, a support address, or a commercial name.
- **SD-10** layering: Presentation may not reference Infrastructure. New PDF writers are reached through **new `I*` ports in `Application/Common/Interfaces`**, exactly as `IReportPrintingService` and `IWorkSheetPdfWriter` already are.
- **SD-11** scope ceiling: nine functions, six slices, no migration, no schema change, no package, no permission code.

## ⚠️ Search query guardrails (SD-5 — binding, all five clauses)

**This is the single most important instruction in this package. The work in S2 and S3 widens one `IQueryable`. Get this wrong and the application loads the entire patient table into memory.**

1. **Every new filter parameter must remain inside a guard clause in the query handler.** No new parameter may be applied outside a guard, and no parameter may replace or remove the existing guard. Each of the six filters gets its own independent guard, shaped like the existing term guard, and is inert when its parameter is null.
2. **Pagination must be preserved exactly as it is today.** No change to the `Page` or `PageSize` parameters, no change to the paging behaviour, no removal or reordering of the `Skip`/`Take` chain, no change to the `OrderByDescending` that feeds it.
3. **The display cap must not be raised. It must remain in force exactly as it is, and this change must not weaken it in any way.** The cap is the `PageSize` parameter of `SearchPatientsGlobalQuery`, whose default is **50**. Do not raise it, do not lower it, do not introduce a literal 100, and do not add a new cap clause. *(The reference system caps at 100; that is a different system's value and it is not being adopted here.)*
4. **Every new predicate must be composed safely so that it narrows the result set rather than broadening it.** A predicate that can widen — an `||` against a patient column, a negation applied unconditionally, a subquery that does not actually constrain — is a defect even if it compiles and even if it "looks right".
5. **The change must be covered by tests that prove each filter narrows correctly, and by a test that proves the cap is still enforced.** For each of the six filters there must be a test whose assertion would fail if the predicate were removed, and there must be a test asserting the cap. The six narrowing tests must not be satisfied by an empty or single-row fixture that would also pass with no filter at all.

**S1 exists to serve clause 5 before any filter exists.** It writes characterisation tests for paging, the cap, ordering, the soft-delete exclusion and the current text-search paths, and touches **no production file**. Do not merge S1 into S2.

## ⚠️ The doctor and the referral entity are two separate filters (SD-6 — binding)

**The search screen ends up with six filter controls, not five. This is a clarification, not an error.**

- A **treating doctor** is an individual physician.
- A **referral entity** is an **institution** — for example a cardiac centre or a maternal-and-child centre — where a physician may work and may sign the examination while being employed by the centre rather than owning it.

These are two different fields on `Patient` (`TreatingDoctorId` and `ReferralEntityId`) and they are **not** the same concept. The English reference manual names the treating doctor; the Arabic reference manual names the referral entity. **Both are correct, and both fields exist.** You must therefore provide **both as separate search filters**.

Concretely, and without exception:
- Two separate query parameters.
- Two separate `Where` clauses, each over its own column.
- Two separate bindable ViewModel properties.
- **Two separate, differently-labelled controls in the UI.**

**Do not treat them as one and the same. Do not collapse them into a single control, a single parameter, or a single lookup.** VG-03 contains a test that fails if the two are ever merged; write it.

## Plan-vs-code corrections you must honour (P-01.md §1)

The plan's line numbers are unverified claims. **Use them, then confirm them at Stage 3.** Among the ones that change what you build:

- **C-1** Top-Lab has **no 100-patient cap**. The cap is `PageSize`, default 50. Preserve it; do not introduce 100.
- **C-2** **six** controls from **five** search functions.
- **C-3** the results-not-verified filter is **not** missing: `ResultsWorklistViewModel.IsReviewed` exists and is already dispatched. **F7 is a XAML-only change** — do not rebuild the backend. The grid column at `ResultsWorklistView.xaml:80` is a **display** column and must survive.
- **C-4** the results-not-printed filter is **not** a one-line binding: the query has no `IsPrinted` parameter and the handler only *projects* it. F6 is real backend work, and the `false` case must select unprinted rows rather than behaving as "no filter".
- **C-5** price-list printing needs **two** missing artefacts — a print command **and** a writer.
- **C-6** the custom-group print is the same shape over different DTOs; do not share a writer or widen either DTO.
- **C-8** text search resolves phone matches in a **separate** query **before** the patient `Where`, then ORs the resulting ids in. Preserve that structure exactly.

## Slice loop (strictly S1 → S6)

Each slice runs the full 10-stage cycle from `P-01-memory.md`:

1. **Pre-Execution Verification** — build 0/0, no test count below your measured baseline, HEAD still valid, tree clean apart from your own package files.
2. **Deep Understanding** — re-read that slice's section in `P-01.md` and every correction it cites.
3. **File Analysis** — open every file the slice touches, at the cited lines, **before** editing anything.
4. **Planning** — write the exact step-by-step plan into the slice's Stage 4 note in the memory file, including any decision you take.
5. **Execution** — implement only this slice's scope. Patterns: existing MediatR + FluentValidation; hand-rolled MVVM (`ViewModelBase.SetProperty`, `RelayCommand` / `AsyncRelayCommand`); `Result`/`Error` with Arabic-first messages; `ArgumentException.ThrowIfNullOrWhiteSpace`; `NavigateTo<T>()` + `CurrentViewModel is T` + `LoadAsync`; hand-rolled xUnit fakes, **no mocking library**; QuestPDF with the `WorkSheetPdfWriter` static-constructor settings and `ArabicFontResolver`.
6. **Post-Execution Verification** — `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` (or VS MSBuild) ⇒ **0 errors / 0 warnings**.
7. **Validation Gate** — `VG-nn` **item by item**, including the zero-migration and zero-package gates. Every item must pass. Record the evidence in the memory file.
8. **Documentation Update** — tick the slice's stage checkboxes; append every new user-facing string to the Created UI Texts Register.
9. **Memory Status Update** — Slice Index, Current Status and Execution Log updated.
10. **Git Commit** — **LOCAL only**, on `main`, message `[P-01] Slice N/6: <slice title> — loop-engineering`. Stage **explicit paths only**.

**Upon a passing gate: commit, then start the next slice immediately** — no human pause. The only normal stop is S6 plus the wave DoD.

## Environment rules (this repository is fussy about these)

- **Every** `dotnet` command needs `-p:EnableWindowsTargeting=true` — `restore`, `build`, `test`, `dotnet-ef`. `TopLab.Presentation` targets `net8.0-windows`, uses WPF and sets `RuntimeIdentifier=win-x64`; without the flag MSBuild fails with `NETSDK1100`.
- **Never run `dotnet test` on the solution.** Run each test project on its own:
  ```
  dotnet test tests/TopLab.Domain.Tests          -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Application.Tests     -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Infrastructure.Tests  -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Presentation.Tests    -p:EnableWindowsTargeting=true
  dotnet test tests/TopLab.Persistence.Tests     -p:EnableWindowsTargeting=true
  ```
- **Use the .NET 8 SDK** for `dotnet` and `dotnet-ef`; set `DOTNET_ROOT` if you installed it to a custom path, or `libhostfxr` will not be found.
- **Package versions live only in `Directory.Packages.props`.** `ManagePackageVersionsCentrally` is on. **This wave adds no package.**
- **The build is warning-free. Treat any new warning as a regression**, exactly like a failing test.
- **Never launch the application.** `App.xaml.cs` applies EF migrations to the configured database during startup, so running it could migrate the owner's real database. Building and running automated tests is fine.
- **A skipped test is reported as skipped, never as a pass.**

## Baseline (G0 — measure it yourself before Slice 1)

**The counts in the plan are a reference, not your gate.** Run the build and all five test projects on your machine, record your own numbers in the Baseline table in `P-01-memory.md`, and only then compare. Adopting someone else's numbers makes "no count may fall below baseline" meaningless. If any delta is unexplained, stop and report before Slice 1.

Record: the .NET SDK version, `git rev-parse HEAD`, `git status --porcelain`, build warning and error counts, the passed/total count of each test project, the `has-pending-model-changes` result, the migration file count, and confirmation that `Directory.Packages.props` is unchanged.

**No count may fall below *your own measured* table in any slice, and no build warning may appear.**

## Migrations and packages policy (absolute)

- **Zero new migration classes.** The `Migrations/` folder is read-only for this entire wave.
- **Never** edit, delete or rename any existing migration or its `.Designer.cs`.
- **Never** touch `ApplicationDbContextModelSnapshot.cs` — not even via EF tooling.
- **Never** run `dotnet ef database update`.
- **Never** edit `Directory.Packages.props` or any `.csproj`.
- Prove it every slice: `git diff -- src/TopLab.Infrastructure/Persistence/Migrations/` and `git diff -- Directory.Packages.props` must both be **empty**.

## Git policy (absolute)

- One **local** commit per verified slice on `main`, message `[P-01] Slice N/6: <slice title> — loop-engineering`.
- **Never push. Never** create or switch a branch. **Never** amend, rebase, reset, force-push, stash, clean, or tag.
- **Never** `git add -A` / `git add .` — stage explicit paths only.
- Never commit on a red build or a failed gate.
- The owner reviews each commit afterwards; that review is **not** a gate you wait on. Proceed immediately.
- Read-only git commands are encouraged in every report: `rev-parse`, `status`, `diff`, `log`, `show`, `ls-files`.

## Forbidden, without exception

- Any git write command beyond the one authorised local commit per slice.
- Creating, editing, deleting or applying any migration; changing the model snapshot by hand; `dotnet ef database update`; `--force`.
- Adding a package, editing `Directory.Packages.props`, or writing `Version=` in a `.csproj`.
- Any schema change, any `HasData` row, any new permission code.
- Collapsing the treating-doctor filter and the referral-entity filter into one control, one parameter, or one lookup.
- Applying a filter predicate outside its guard clause, or touching the paging chain, the ordering, or the cap.
- Naming an Infrastructure type from a Presentation file.
- Launching the application.
- Editing any document except `P-01-memory.md`. Do not edit `P-01.md`.
- Inventing Arabic strings, copyright text, a support address, or any commercial name.
- Building, stubbing or scaffolding the deferred function, or any later-wave work.
- Deciding anything the plan leaves open. Record what you observed and report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية».

## The plan is a hypothesis — re-verify before you edit

**The plan's line numbers, counts and descriptions are unverified claims about the code as it was when the plan was written. Treat every one of them as a hypothesis and check it against the live code before you change anything.**

Before editing, confirm that the cited line really contains what the plan quotes, that the count is still what the plan says, and that the pattern you are about to copy really exists at the place the plan points to. **If the code differs from the plan — the file moved, the line changed, the count differs, a call the plan describes is gone, or something the plan depends on is not there — STOP and report the difference.** Do not silently adapt, do not "fix the plan's number in your head", and do not proceed on the assumption that the plan is roughly right. A wrong line number is cheap to check; a wrong *count* can silently change what you build.

Counting traps this wave contains:
- `grep -rn "IsPrinted"` matches migration designers and the model snapshot as well as production code. Strip `Migrations/` before reasoning about blast radius.
- `SearchPatientsGlobalQuery` is a **positional record**. Appended members must be defaulted and must **never be inserted in the middle** — the positional shape is part of the public surface.
- The literal `100` appears nowhere in `SearchPatientsGlobalQuery`. Do not "restore" a cap that was never there.

## Slice-specific warnings you must not skip

- **S1 must land before S2.** S1 writes the guardrail characterisation tests and touches no production file. Without it, a paging or cap regression in S2 cannot be told apart from one the filters caused. Verify S1's commit exists in `git log` at the start of S2's Stage 1; if not, **STOP**.
- **S2 before S3.** S3 binds controls to properties S2 introduces. A control bound to a property that does not exist will fail the binding gate, but not necessarily the build.
- **S3's six-control assertion.** VG-03 asserts **six** controls, not five, and includes a test that fails if the doctor and referral-entity controls are ever merged. Do not weaken that test to make it pass.
- **F7 is XAML-only.** The backend already works (C-3). If you find yourself writing a new `IsReviewed` query parameter or handler branch, you have misread the plan — stop and re-read C-3.
- **F6's `false` case is the important one.** A `bool? IsPrinted = false` must select **unprinted** rows. Code that treats `false` as "no filter" compiles, runs, and returns everything. That is the whole failure mode of F6.
- **F7 must not remove the `IsReviewed` grid column.** The column and the new checkbox are different things and both must remain.
- **F5 before F6** is not a dependency; S4 is independent of S1–S3. Do not reorder anyway — the protocol is strictly sequential.
- **S5 before S6.** S6 mirrors S5's shape. Do not let S6 refactor S5's port or writer into a shared abstraction.
- **The two print paths must not share a writer or a DTO.** `PriceListDtos.cs` and `CustomGroupDtos.cs` are unrelated. Widening either to fit both is a defect.
- **A PDF test must assert real output.** "The call did not throw" is not evidence that a PDF was produced with the right content. Assert non-empty output and the presence of the expected content.
- **S1's `git diff --stat` over `src/` must be empty.** If S1 shows a production change, it has failed its own purpose.

## After each slice: report, commit, continue

When a slice's gate passes, produce a report containing:

- the slice number and title, and the gate result **item by item**;
- the exact commands you ran and their key output — build warnings/errors, each test project's passed/failed/total, the `has-pending-model-changes` line, and `git diff --stat` over both `Migrations/` and `Directory.Packages.props`;
- the measured counts **compared against your measured baseline**, stated as a delta;
- the commit hash you just made, and `git status --porcelain` **after** committing — it must be empty;
- anything you noticed that the plan did not anticipate, and anything you deliberately did not do;
- any new user-facing string (appended to the register), and any open owner decision the slice surfaced.

**Then commit (Stage 10) and begin Stage 1 of the next slice immediately — no pause, no request for human confirmation.**

## Stop rules — any one halts the loop immediately

Record it in `P-01-memory.md`'s Stop Report and wait.

- Live code contradicts `P-01.md` (SD-1), or a `file:line` turns out to be wrong in a way that changes what the slice should do.
- Any slice appears to require a migration, a schema change, a `HasData` row, a permission code, or a new package (SD-3, SD-4).
- The treating-doctor and referral-entity filters are about to be collapsed into one (SD-6).
- A filter predicate is about to be written outside its guard clause, or the paging chain, the ordering, or the cap is about to be touched (SD-5).
- S2 is about to start before S1's commit exists.
- The build or the tests go red and cannot be restored within the slice's scope.
- The same failure occurs **5 consecutive** times.
- Any ambiguity not already settled in §0 — report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية» and do **not** decide it yourself.
- The application is about to be launched, or a migration is about to be created, edited or applied. Stop; both are forbidden.

## Slice map (details in P-01.md)

| # | Title | Package | Migration | Depends on |
|---|---|---|---|---|
| 1 | Search guardrail net — pin paging, cap and current behaviour | PP-01 | **No** | — |
| 2 | Six patient-search filters in query + handler | PP-01 | **No** | S1 |
| 3 | Search screen — the six filter controls | PP-01 | **No** | S2 |
| 4 | Worklist `IsPrinted` predicate + both status checkboxes | PP-02 | **No** | — |
| 5 | Price-list print — port, writer, command, control | PP-03 | **No** | — |
| 6 | Custom-group list print + wave DoD | PP-03 | **No** | S5 |

**Begin at Step 0, measure and report the G0 baseline, then go straight into Slice 1, Stage 1. Run all six slices in sequence, committing each one, and stop only when Slice 6's gate and the wave DoD pass, or a Stop Rule triggers.**
