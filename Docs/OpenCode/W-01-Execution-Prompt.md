# Loop-Engineering Execution Prompt — Workstream W-01 (Wave 1)

> Convention: standalone prompt per workstream (S-05/S-06/S-07 pattern). Copy everything below the line verbatim to the local executing coding agent.

---

You are the local executing coding agent for Top-Lab workstream **W-01** (Wave 1), under the loop-engineering protocol used by S-00…S-07.

**Repository:** work inside the local clone of Top-Lab (`main`).
**Baseline commit:** `8607a8757a424f3a6dc1bd08ee82671a0195b200` («بعد اصلاح الحزمه 0»).

## Step 0 — Pin baseline

```
git rev-parse HEAD
git status --porcelain
```

HEAD must be `8607a8757a424f3a6dc1bd08ee82671a0195b200` (or a descendant whose diff is **only** the W-01 package files under `Docs/OpenCode/`: `W-01.md`, `W-01-memory.md`, `W-01-Execution-Prompt.md`). Tree clean otherwise. **If not → STOP and report.**

## Source documents (read both fully before any code)

1. `Docs/OpenCode/W-01.md` — 8 slices, VG-01…VG-08, SD-1…SD-10, plan-vs-code corrections C-1…C-8.
2. `Docs/OpenCode/W-01-memory.md` — baseline, gates, migration register, 10-stage checklists. **You update this file as you go.**

No other module file is required. If a fact is not in these two files and not in live code, stop and report.

## Mission

Implement Wave 1 packages: **WP-05** (combined-report ownership guard), **WP-02** (results worklist navigation + orphan screens), **WP-03** (sensitivity UI + repair migration), **WP-04** (ExternalEntity Email + referral editor + `AddExternalEntityEmail`), **WP-15** (BranchScope + `AddBranchNumber` + UI). **Exactly three new migrations** with the exact names in W-01. **Never edit** the eight existing migrations.

## Binding decisions (do not reopen)

- **SD-1** repair map `0→NULL` · `1→0` · `2→1` · `3→2` + read-only report before writes (C-6: column may need nullable in the same migration).
- **SD-2** branch = SystemSettings + User only; **no** `Patient.BranchNumber`; honest search message (C-7).
- **SD-3** English labels: Unspecified / Sensitive / Intermediate / Low Sensitivity / Resistant.
- **SD-8** do not change `ValidatePriceListRule`. **SD-9** do not change enum 0–3. **SD-10** keep CombinedReportLineDto trailing optional comments.
- **C-1** no `PatientTest.IsDeleted` — ownership filter only. **C-3** reuse `GetPriceListsQuery`. **C-5** Print delegates to Build.

## Slice loop (strictly S1 → S8)

Each slice runs the full 10-stage cycle from `W-01-memory.md`:

1. **Pre-Execution Verification** — build 0/0, tests ≥ baseline, HEAD/baseline OK.
2. **Deep Understanding** — re-read the slice in `W-01.md` and cited code.
3. **File Analysis** — open every file at cited lines **before** editing (SD-7: plan is a hypothesis).
4. **Planning** — record exact steps in the memory checklist Stage 4.
5. **Execution** — implement only the slice scope. Patterns: existing MediatR/FluentValidation/MVVM; Arabic error messages byte-for-byte; `Result`/`Error`; `NavigateTo<T>()` + `CurrentViewModel is T` + `LoadAsync`.
6. **Post-Execution Verification** — `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` (or VS MSBuild) **0 errors / 0 warnings**.
7. **Validation Gate** — `VG-nn` item by item. Migrations policy: new files only in S4/S6/S7.
8. **Documentation Update** — tick stages in memory.
9. **Memory Status Update** — Slice Index + Execution Log.
10. **Git Commit** — **LOCAL only**, message: `[W-01] Slice N/8: <title> — loop-engineering`. Stage explicit paths only.

**Upon a passing gate: commit, then start the next slice immediately** (no human pause). The only normal stop is Slice 8 + wave DoD.

## Git policy (absolute)

- One **local** commit per verified slice on `main`.
- **NEVER push. NEVER** create/switch branches. **NEVER** amend, rebase, reset, force-push, rewrite history.
- **NEVER** `git add -A` / `git add .` — stage only the slice's files.
- Never commit on a red build or failed gate.
- Owner pushes after the entire wave succeeds.

## Migrations policy (absolute)

- New migration classes **only**: `FixCultureSensitivityCategoryOffByOne` (S4), `AddExternalEntityEmail` (S6), `AddBranchNumber` (S7).
- **Never** edit, delete, or rename the eight existing migrations or their `.Designer.cs`.
- Touch `ApplicationDbContextModelSnapshot.cs` only via EF tooling inside those three slices.
- **Never** run `dotnet ef database update` against a production/owner database without explicit coordination.
- After each migration slice, ensure `W-01-memory.md` Migration Register is filled.

## Stop rules (any one → Stop Report in memory + wait)

- Live code contradicts `W-01.md` (SD-7 / C-1…C-8).
- Need to edit an existing migration or add `Patient.BranchNumber`.
- Temptation to change `ValidatePriceListRule` or `SensitivityCategory` values.
- Same failure **5 consecutive** times.
- Slice 1 identity-leak test cannot go green.

## Environment

- Windows / VS 2022 MSBuild is acceptable for build (`0/0`).
- `-p:EnableWindowsTargeting=true` when using `dotnet` CLI.
- Run test projects individually if solution-level `dotnet test` is blocked.
- Do not launch the WPF app against a real lab database (EF may migrate on startup).

## Slice map (details in W-01.md)

1. WP-05 ownership guard → VG-01  
2. WP-02 worklist + orphans → VG-02  
3. WP-03 sensitivity UI (no migration) → VG-03  
4. WP-03 repair migration → VG-04  
5. WP-04 Email surface → VG-05  
6. WP-04 editor + AddExternalEntityEmail → VG-06  
7. WP-15 BranchScope + AddBranchNumber → VG-07  
8. WP-15 UI + shell → VG-08  

Begin at **Slice 1, Stage 1** now.
