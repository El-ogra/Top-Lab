# Loop Engineering — Memory File

- **Module:** Wave 1 — WP-02, WP-03, WP-04, WP-05, WP-15
- **Module Number:** W-01
- **Source Plan:** `Docs/OpenCode/W-01.md`
- **Date Created:** 2026-09-30
- **Total Slices:** 8
- **Current Slice:** 5 — ExternalEntity Email surface — NEXT
- **Current Branch:** `main`
- **Baseline Commit:** `8607a8757a424f3a6dc1bd08ee82671a0195b200`
- **Author:** loop-engineering (execution by local coding agent per owner authorization)

---

## Module Summary

Wave 1 reopens the broken daily paths: results-entry navigation (WP-02), culture sensitivity off-by-one + data repair (WP-03), referral/contract entity editor with price list + commission + email (WP-04), combined-report ownership guard (WP-05), and branch scope on SystemSettings+User only (WP-15, decision 4=أ). Three new EF migrations; no edits to the eight existing migrations.

---

## Settled Decisions (binding)

- **SD-1** — Sensitivity repair rule: `0→NULL` · `1→0` · `2→1` · `3→2` + read-only report first. (Owner decision 1 = أ.)
- **SD-2** — Branch model: SystemSettings + User only. **No** `Patient.BranchNumber`. Honest search message instead of patient-branch filter. (Owner decision 4 = أ.)
- **SD-3** — Sensitivity labels (UI): `Unspecified` · `Sensitive` · `Intermediate` · `Low Sensitivity` · `Resistant` (English). Enum values 0–3 unchanged (SD-9).
- **SD-4** — Decision 9 (merge Email+EnglishName) **deferred to Wave 3**. W-01 only adds `Email` (`AddExternalEntityEmail`).
- **SD-5** — Git: one **local** commit per verified slice. **Never push.** Owner pushes after the full wave succeeds.
- **SD-6** — Migration review is done by an independent review agent via an analysis-only prompt (owner copies/sends). Owner does not review migrations.
- **SD-7** — The plan is a hypothesis: code mismatch → STOP and report.
- **SD-8** — Do not touch `ExternalEntity.ValidatePriceListRule`.
- **SD-9** — Do not change `SensitivityCategory` enum values 0–3.
- **SD-10** — Keep `CombinedReportLineDto` trailing `LowComment`/`HighComment` optional params (WP-01).

---

## Plan-vs-Code Corrections (binding — see W-01 §1)

| ID | Correction |
|---|---|
| C-1 | No `PatientTest.IsDeleted` — ownership filter only |
| C-2 | `BlankReportViewModel` is complete; fix `LoadAsync:85` + navigation only |
| C-3 | Reuse `GetPriceListsQuery`; do not create `GetPriceListsForPick` |
| C-4 | Fix `CreateExternalEntityCommand` call at `ExternalEntityEditorViewModel.cs:157-161` |
| C-5 | `PrintCombinedReportCommandHandler` delegates to Build — one guard covers print |
| C-6 | `CultureAntibioticResult.SensitivityCategory` is NON-NULL; `0→NULL` needs nullable column in the repair migration |
| C-7 | No patient-branch filter (SD-2) |
| C-8 | `OpenDetailAsync` null → Arabic StatusMessage, not silent return |

---

## Confirmed Code Facts (do not re-derive blindly — verify at Stage 3)

**WP-05:** `BuildCombinedReportCommandHandler.cs:33-42` lacks ownership filter. `GetCombinableTestsQueryHandler.cs:34` is the correct idiom. `PrintCombinedReportCommandHandler.cs:49-55` sends `BuildCombinedReportCommand`. Seed helpers use `PatientId.Create(1)`.

**WP-02:** `ResultKind { Simple=0, SpecializedProfile=1, Culture=2 }`. `OpenDetailAsync:172-208` routes 0/1/2. `ResultsWorklistView.xaml` DataGrid unbound to `OpenDetailCommand`. `PatientsHubView` = 4 buttons. Blank/History VMs registered `DependencyInjection.cs:61-62`, templates `MainWindow.xaml:140-146`. `FakeNavigationService.NavigateTo<T>()` throws — use instance overload in tests.

**WP-03:** `SensitivityCategory` 0–3. `CultureEntryView.xaml:111` SelectedIndex on 4 items. VM `int?`. Save: delete+reinsert `CultureAntibioticResult`. Config: `HasConversion<int>()` `tinyint` **IsRequired**.

**WP-04:** No `Email` anywhere on ExternalEntity. Create passes `null, null` at `:157-161`. Update passes real `_priceListId`. `ValidatePriceListRule` at `ExternalEntity.cs:159-170`. `GetPriceListsQuery` → `PriceListSummaryDto(Id, Name, ItemCount)` sorted by Name.

**WP-15:** No `BranchNumber` in Domain (except comment `TestPriceResolver.cs:16`). `SystemSettings.CreateDefault` + `HasData` seed Id=1. `CreateUserCommand`/`UpdateUserCommand`/`UpdateSystemSettingsCommand` shapes in W-01. StatusBar `MainWindow.xaml:48-90`.

---

## Global Validation Gates

- **G0 (once, before Slice 1):** measure Windows baseline — build 0/0 + each test project pass/total + `git rev-parse HEAD` + tree clean. Record below.
- **G1 (every slice):** build 0/0; no test count below baseline; slice `VG-nn` item-by-item; migrations policy.

### Migrations policy (every slice)
- New migration files **only** in S4, S6, S7.
- **Never** edit the eight existing migrations or apply `dotnet ef database update` to the owner's DB without coordination.
- **Never** touch `ApplicationDbContextModelSnapshot.cs` except via EF tooling as part of a planned migration slice.

---

## Quality Gate (slice complete only if ALL hold)

1. Scope matches `W-01.md` (SD-7).
2. `dotnet build TopLab.sln -p:EnableWindowsTargeting=true` → **0 errors, 0 warnings**.
3. No test count below recorded baseline.
4. Slice `VG-nn` passes item by item.
5. Local commit only (SD-5).

---

## Stop/Continue Rule

- Continue automatically after a passing gate + local commit (SD-5).
- **STOP** on: plan/code mismatch (SD-7); editing an existing migration; adding `Patient.BranchNumber`; touching `ValidatePriceListRule` or enum values; same failure **5 consecutive** times; S1 identity-leak test red.
- Write Stop Report in `## Stop Report` below and wait.

---

## Baseline (fill in G0 on the owner's Windows machine before Slice 1)

| Item | Value |
|---|---|
| Date measured | 2026-09-30 |
| .NET SDK / VS MSBuild | VS 2022 Community MSBuild (dotnet 9 SDK present) |
| `git rev-parse HEAD` | `8607a8757a424f3a6dc1bd08ee82671a0195b200` ✓ |
| `git status --short` | 3 untracked W-01 package files only (expected) |
| Build warnings / errors | 0 / 0 |
| TopLab.Domain.Tests | (in full suite) |
| TopLab.Application.Tests | 1495/1495 (post Slice 1) |
| TopLab.Infrastructure.Tests | (in full suite) |
| TopLab.Presentation.Tests | (in full suite) |
| TopLab.Persistence.Tests | (in full suite) |
| Full suite baseline | 2237 total · 2236 passed · 1 skipped (pre-Slice 1) |
| Migrations folder unchanged | yes |

---

## Slice Validation Gates (from plan)

| Slice | Gate | Key checks |
|---|---|---|
| 1 | VG-01 | ownership tests; no leak; zero-drift |
| 2 | VG-02 | orphan VM tests; hub buttons; structural XAML |
| 3 | VG-03 | 5 sensitivity options; save mapping; **no** migration file |
| 4 | VG-04 | repair map tests; backup table; only new migration file |
| 5 | VG-05 | Email persist; price-list rule intact |
| 6 | VG-06 | create referral via VM; `AddExternalEntityEmail` only |
| 7 | VG-07 | BranchScope tests; no Patient.BranchNumber |
| 8 | VG-08 | UI+shell; honest search message; wave DoD |

---

## Slice Index

| # | Slice Title | Status | Gate |
|---|---|---|---|
| 1 | Combined-report ownership guard (WP-05) | [x] DONE | VG-01 PASS |
| 2 | Results worklist navigation + orphan screens (WP-02) | [x] DONE | VG-02 PASS |
| 3 | Sensitivity UI + validation (WP-03) | [x] DONE | VG-03 PASS |
| 4 | Sensitivity repair migration (WP-03) | [x] DONE | VG-04 PASS |
| 5 | ExternalEntity Email + commands (WP-04) | ⬜ | VG-05 |
| 6 | Referral editor + AddExternalEntityEmail (WP-04) | ⬜ | VG-06 |
| 7 | BranchScope + AddBranchNumber (WP-15) | ⬜ | VG-07 |
| 8 | Branch UI + shell (WP-15) | ⬜ | VG-08 |

---

## Per-slice 10-stage checklists

### Slice 1 — Combined-report ownership guard (WP-05)
**Gate:** VG-01. **Status:** ✅ Passed.
- [x] 1. Pre-Execution Verification — build 0/0; baseline 2236 passed / 1 skipped
- [x] 2. Deep Understanding — W-01 §3; C-1/C-5; handler :33-42
- [x] 3. File Analysis — BuildCombinedReportCommandHandler/Validator; Print delegates :49-55
- [x] 4. Planning — ownership filter + Forbidden message + distinct validator rule
- [x] 5. Execution — filter + validator + 5 tests (foreign, duplicate, own, no-leak, print-passthrough)
- [x] 6. Post-Execution Verification — build 0/0; Application 1495/1495
- [x] 7. Validation Gate — VG-01 PASS
- [x] 8. Documentation Update — this checklist
- [x] 9. Memory Status Update — Slice 1 done, Slice 2 next
- [x] 10. Git Commit — local

### Slice 2 — Results worklist + orphans (WP-02)
**Gate:** VG-02. **Status:** ✅ Passed.
- [x] 1–10 complete. OpenDetail button + double-click + null message; hub +2 buttons; BlankReport LoadAsync builds; OrphanedViewModelTests green (57/57 Presentation).

### Slice 3 — Sensitivity UI (WP-03, no migration)
**Gate:** VG-03. **Status:** ✅ Passed.
- [x] 1–10. SensitivityOption + 5 English labels (SD-3); SelectedValue binding; OrganismC preview; mapping tests green. No migration file.

### Slice 4 — Sensitivity repair migration (WP-03)
**Gate:** VG-04. **Status:** ✅ Passed.
- [x] 1–10. `20260930163921_FixCultureSensitivityCategoryOffByOne` + backup + CASE map; nullable column (C-6); repair tests green; only new migration + snapshot (EF tooling).

### Slice 5 — ExternalEntity Email surface (WP-04)
**Gate:** VG-05. **Status:** ⬜
- [ ] 1–10

### Slice 6 — Referral editor + email migration (WP-04)
**Gate:** VG-06. **Status:** ⬜
- [ ] 1–10

### Slice 7 — BranchScope + migration (WP-15)
**Gate:** VG-07. **Status:** ⬜
- [ ] 1–10

### Slice 8 — Branch UI + shell (WP-15)
**Gate:** VG-08. **Status:** ⬜
- [ ] 1–10 + wave DoD

---

## Migration Register (fill as they land)

| Slice | Migration name | Tables/columns | Backup | Review agent |
|---|---|---|---|---|
| 4 | FixCultureSensitivityCategoryOffByOne | CultureAntibioticResults (nullable + CASE 0→NULL/1→0/2→1/3→2) | CultureSensitivityBackup | pending |
| 6 | AddExternalEntityEmail | ExternalEntities.Email | n/a | pending |
| 7 | AddBranchNumber | SystemSettings.BranchNumber, Users.BranchNumber + seed | n/a | pending |

---

## Created UI Texts Register

| Slice | Screen | Text | Kind |
|---|---|---|---|
| 2 | ResultsWorklistViewModel | «اختر مريضاً من القائمة أولاً.» | Status |
| 3 | CultureEntryView | Unspecified/Sensitive/Intermediate/Low Sensitivity/Resistant | Labels (SD-3) |
| 8 | PatientSearch | «البحث بفرع غير متاح دون مبيعات موزّعة على الفروع.» | Honest notice |

---

## Execution Log

| Slice | Commit | Notes |
|---|---|---|
| 1 | (this commit) | Ownership guard WP-05; App tests 1495/1495 |

---

## Stop Report

(none)
