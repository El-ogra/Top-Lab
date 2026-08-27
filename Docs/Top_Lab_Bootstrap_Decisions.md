# Top-Lab — Bootstrap Scaffold: Open-Question Decisions

**Date:** 2026-08-28
**Context:** Resolves the 10 open questions raised in §2 of the planning agent's "detect current status" report, blocking execution of F1 (solution & project skeleton, Phases A–E). Every decision keeps Top-Lab's architecture and technology stack consistent with MasrLab (Clean Architecture, .NET 8, WPF, EF Core, MediatR, FluentValidation, single-tenant, offline/local-network deployment).

---

## Summary Table

| # | Open Question | Decision |
|---|----------------|----------|
| 1 | Test framework | **xUnit** |
| 2 | Mocking/faking library | **None** — hand-rolled fakes only; no FluentAssertions, use built-in xUnit `Assert` |
| 3 | Validation library + version | **FluentValidation**, latest stable **12.x** |
| 4 | MediatR package + version | Latest stable **12.x — do not go to 13.x or later** |
| 5 | EF Core + SQL Server provider | `Microsoft.EntityFrameworkCore` / `.SqlServer` / `.Tools`, latest stable **8.x** |
| 6 | Centralized Package Management | **Yes** — `Directory.Packages.props` at solution root |
| 7 | Initialize git? | **Yes** — `git init` + `.gitignore` now; no remote yet |
| 8 | Presentation TFM | **`net8.0-windows`** (default suffix, no pinned Windows SDK version) |
| 9 | LangVersion / Nullable | **Confirmed** — `Nullable=enable`, `LangVersion=latest`, all projects |
| 10 | Initial placeholder contents | **Truly minimal** — template defaults only, no `Common/` base types yet |

---

## Detailed Decisions & Rationale

### 1. Test framework — xUnit
Standard choice for .NET 8 / Clean-Architecture / MediatR scaffolds, strong `dotnet test` tooling, MIT-licensed with no commercial risk. Matches the planning agent's own recommendation.

### 2. Mocking library & assertions — none added
`Top_Lab_Test_Strategy.md` (§3.2/§3.3) already speaks of "fakes" and "in-memory doubles," never "mocks." Decision: write hand-rolled fakes/test doubles only — zero extra dependency, no ambiguity, matches the wording already in the docs. Do **not** add Moq or NSubstitute by default.
For assertions, use xUnit's built-in `Assert` class — **not** FluentAssertions. FluentAssertions v8.0+ (Jan 2025 re-license under Xceed) requires a paid commercial license ($130/developer/year) for any commercial-use project; only the frozen v7.x line remains permanently free (Apache-2.0, bug-fixes only, no new features). Since Top-Lab is commercial software for resale, avoid this dependency entirely rather than track a paid seat count. If a fluent assertion style is wanted later, use **Shouldly** (MIT, no commercial tier) instead of FluentAssertions.

### 3. Validation library — FluentValidation, latest stable 12.x
Remains fully Apache-2.0 / free for commercial use (only an optional sponsorship request from the maintainer, not a license requirement) — no conflict, safe to track the newest stable release at install time.

### 4. MediatR — pin to latest stable 12.x ⚠ licensing-sensitive
MediatR was taken commercial by its creator (Lucky Penny Software) effective **v13.0.0 (launched July 2, 2025)**: v13+ requires a paid license, tiered by team size (Standard 1–10 devs / Professional 11–50 / Enterprise unlimited), with only a limited free Community edition for very small teams. **Versions before 13.0.0 remain permanently under the original open-source license (Apache-2.0/MIT)** and stay free regardless of team size or commercial use.
**Decision:** pin to the latest stable **12.x** release and do not let any agent silently bump past major version 12. Since Top-Lab (like MasrLab) is sold to other lab owners, this avoids any future licensing entanglement tied to team growth or resale terms. *Flag for later: confirm whether MasrLab has pinned a MediatR version yet — if not, apply the same 12.x ceiling there for consistency.*

### 5. EF Core + SQL Server provider packages
`Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools` (for migrations) — latest stable **8.x** patch release, matching the `net8.0` TFM used across Domain/Application/Infrastructure. Do not move to EF Core 9.x, which targets .NET 9 and isn't needed here. MIT-licensed by Microsoft — no licensing concern.

### 6. Centralized Package Management (CPM) — yes
Add `Directory.Packages.props` at the solution root and pin every package version there, including the two licensing-sensitive ones above (MediatR ≤12.x, FluentAssertions excluded entirely). This makes the version ceiling a single source of truth that no future agent or `dotnet add package` call can accidentally drift past.

### 7. Initialize git — yes, now
`git init` inside the relocated `Top-Lab` solution folder, with a `.gitignore` covering `bin/`, `obj/`, `*.user`, any `appsettings.*.json` holding connection strings or secrets. Keep it local-only for now — no GitHub remote is created as part of this bootstrap; visibility (public/private) and remote setup are a separate decision for later.

### 8. Presentation target framework — `net8.0-windows`
Use the default suffix, not a pinned Windows SDK version like `net8.0-windows10.0.19041.0`. Top-Lab deploys to ordinary lab desktop PCs on whatever supported Windows version they run; pinning a specific Windows SDK adds an unnecessary API-surface constraint with no current benefit (no WinRT/WinUI-specific API is planned).

### 9. LangVersion / Nullable — confirmed as specified
`<Nullable>enable</Nullable>` and `<LangVersion>latest</LangVersion>` in every project — Domain, Application, Infrastructure, Presentation, and all three test projects. No change from what the Coding Standards already require.

### 10. Initial placeholder contents — truly minimal
The scaffold (F1) should contain only what `dotnet new classlib` / `dotnet new wpf` generate by default (with the demo `Class1.cs` stripped), plus the empty folder tree already specified in Phase B8 — nothing else. Do **not** pre-create `Entity.cs`, `Result.cs`, or any other `Common/` base type now: those are real design decisions that belong to the first functional implementation slice (where they can be reviewed deliberately), not silently invented during a structural bootstrap pass.

---

## Consistency Check
Decision 6 (CPM) pins exactly the versions chosen in Decisions 1, 3, 4, and 5 in one place, so the two licensing-sensitive constraints (MediatR ≤12.x, no FluentAssertions) can't silently drift in a later package update. No decision above contradicts another; Phases A–E can proceed using this table as-is.
