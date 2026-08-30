# Top-Lab — Foundation Phase Acceptance Report

## نظام توب لاب — تقرير قبول المرحلة التأسيسية

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Phase | Foundation Phase (Loop Engineering / PDCA) |
| Report date | 2026-08-30 |
| Loops | 8 — all **COMPLETED** |
| Status | **ACCEPTED** |

---

## 1. Summary (الملخص)

The Foundation Phase strengthened the configuration surface, removed the
raw-connection-string crash cause (B-02 / DEF-1), introduced a first-run
setup wizard, wired automatic migrations, and hardened local distribution.
All quality gates below were met on commit `a035904` through `7cc5515`.

Committed (local only, per engagement rule — no push performed):

| Commit | Scope |
|---|---|
| `a035904` | PDCA-1..1.5..2 — config surface, strong `LabId`, wizard + Arabic titles + migrate |
| `5b6742e` | PDCA-3 — publish profiles (folder + self-contained) |
| `7cc5515` | PDCA-4 — ADR-0025 + Coding/DataModel/Architecture/Tracking/Test-Strategy docs + INSTALL.txt |
| `(none)` | PDCA-5 — verification loop only; no tracked-file changes (empty commit not created by policy) |

---

## 2. Final Verification Results (نتائج التحقق النهائي)

| Gate | Result |
|---|---|
| `dotnet build TopLab.sln -c Release` | **0 errors / 0 warnings** |
| `dotnet test TopLab.sln -c Release --no-build` | **73 / 73 passed** (37 Domain + 15 Application + 21 Infrastructure) |
| `dotnet publish -p:PublishProfile=FolderProfile` | **SUCCESS** → `bin/Release/net8.0-windows/win-x64/publish/` |
| `dotnet publish -p:PublishProfile=FolderProfile.SelfContained` | **SUCCESS** → `.../publish-selfcontained/` |
| `dotnet ef database update` (5.3) | **SUCCESS** — database up to date, idempotent |
| Schema | **36 business tables** + `__EFMigrationsHistory` (37 total); seeds: `SystemSettings`=1, `Permissions`=13 |
| 5.4 Clean-config launch | **PASS** — setup wizard shown, no crash |
| 5.5 Existing-config launch | **PASS** — wizard skipped, `MigrateAsync` idempotent, `MainWindow` reached |
| 5.6 Offline launch (framework-dependent) | **PASS** — wizard shown from clean publish copy |
| 5.7 Dependency Rule | **PASS** — Infrastructure referenced only by the composition root (`App.xaml.cs`) |

---

## 3. What Was Delivered (ما تم إنجازه)

1. **B-02 root cause fixed (non-UI part).** New `appsettings.example.json`
   committed and re-included in `.gitignore`; `%ProgramData%\TopLab\
   appsettings.json` is now the machine-scoped store written by the wizard
   and read with precedence by the composition root. The raw
   `InvalidOperationException` on a missing connection string is replaced by
   a first-run wizard + clean shutdown.
2. **DEF-1 fixed.** `Patient.LabId` migrated from `string?` to
   `LabId : StronglyTypedId<string>` (ADR-0012 compliance), mapped back to
   the existing `nvarchar(30)` column via an EF value converter. No schema
   change required.
3. **First-run wizard (Arabic).** `DatabaseSetupWindow.xaml(.cs)` +
   `DatabaseSetupViewModel.cs` — validates connectivity (Test) and writes the
   settings file (Save). Navigation titles are Arabic (DEF-3).
4. **Automatic migration.** `MigrateAsync` runs from the composition root
   behind a guarded `try/catch` (DEF-7) and reaches `MainWindow` only when the
   schema is ready.
5. **Distribution.** Folder + self-contained publish profiles
   (`FolderProfile.pubxml`, `FolderProfile.SelfContained.pubxml`),
   `RuntimeIdentifier=win-x64`, and an `INSTALL.txt` covering
   build/transfer/install/first-run. The csproj `Content` copy of
   `appsettings.json` is now `Condition="Exists(...)"` so a clean clone
   builds without a local settings file.
6. **Documentation.** ADR-0025 added; Coding Standards §10, Data Model §12.6,
   Architecture §11, Master Tracking Sheet (F2/F4/F5/F6), and Test Strategy
   §3.2/§3.4 updated.

---

## 4. Issues Encountered and Resolutions (المشاكل وحلولها)

| Issue | Resolution |
|---|---|
| Bare `dotnet ef migrations list` failed when `appsettings.json` missing (csproj `Content` copy is a hard build dependency) | Added `Condition="Exists('appsettings.json')"`; clean clones now build |
| Test-config JSON written by hand used `\m` (invalid escape) — app crashed parsing ProgramData file | Reproduced with properly escaped JSON; confirmed wizard-written files use `JsonSerializer` (always valid) |
| LocalDB not started → connection timeout during smoke | `sqllocaldb start MSSQLLocalDB`; docs recommend this in INSTALL.txt troubleshooting |
| DI lifetime warning (singleton `MainWindow` resolves scoped `IDateTimeProvider`, flagged by EF design-time host build) | **Pre-existing** from F6 wiring; runtime unaffected in Release. Captured here for a future hardening loop (addressed outside this phase) |
| LoopMemory `[x]` gate numbers (16, 23, 26, 31) did not match the provided template baseline (12) | Kept template verbatim; gates re-derived from actual content. No behavioral impact |

---

## 5. Acceptance (القرار)

**PASS.** All eight PDCA loops (PDCA-0, 1, 1.5, 2, 3, 4, 5, Final) are
marked COMPLETED in `Docs/LoopMemory.md`, which has been **deleted** as a
transient working file, and its gitignore exemptions removed. The repository
is ready for the next phase.

---
*Prepared during the Foundation Phase closing loop (PDCA-Final), 2026-08-30.*