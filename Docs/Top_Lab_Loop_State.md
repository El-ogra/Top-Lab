# Top-Lab — Loop Execution State (F3–F6)

## نظام توب لاب — سجل حالة تشغيل الحلقة (تقنية هندسة الحلقات)

---

## 0. Purpose
This file is the persistent **state / memory** of the autonomous loop that implements
Wave 0 features **F3 → F4 → F5 → F6** in a single run. Per the Loop Engineering pattern
(Maker → Judge → Loop → State), it lets the run resume from the last *green* feature if
execution is interrupted (e.g. context compaction), and records every judge verdict.

## 1. Loop operating rules (locked by user)
- One run executes F3..F6 fully.
- After each feature reaches **green** (all gates pass), commit **locally** only.
- **No `git push`** until the user explicitly approves.
- The "Judge" = deterministic gates, not self-evaluation: `dotnet build -warnaserror`,
  `dotnet test`, Dependency-Rule check, convention check, coverage report, migration gate.

## 2. Quality gates (Test Strategy §6)
1. Compile: `dotnet build TopLab.sln -c Debug -warnaserror` → 0 errors / 0 warnings.
2. Test: `dotnet test TopLab.sln` → all pass.
3. Dependency-Rule: Domain has 0 external refs; Presentation refs Application only;
   Application refs Domain + abstractions only (no Infrastructure concrete types named).
4. Convention: `<Type>Tests` naming + feature/folder layout (Coding Standards §4, §8).
5. Coverage: measure per project (coverlet). Report floors — Domain 90%, App 80%, Infra 70%.
   (Figures reported honestly; not fabricated.)
6. Migration: if schema changed, an EF migration is added and applies cleanly.

## 3. Feature status
| Feature | Status | Iterations | Build | Test | Gates | Completed | Commit |
|---|---|---|---|---|---|---|---|
| F3 | 🟩 done | 3 | 0/0 | 26 pass | compile+test+dep-rule+convention OK | 2026-08-28 | (local, pending push) |
| F4 | ⬜ pending | - | - | - | - | - | - |
| F5 | ⬜ pending | - | - | - | - | - | - |
| F6 | ⬜ pending | - | - | - | - | - | - |

## 4. Iteration log (append-only)
- **Iter 1 (F3):** Wrote Result/Error/ErrorType + Application ports (ICurrentUserService, IDateTimeProvider, IAppLogger, IReportPrintingService, IBarcodeService, IAuthorizedRequest) + 3 MediatR behaviors (Validation/Authorization/Logging) + DI. Tests: fakes + behavior tests + Result tests. Build failed (missing using in 2 interfaces) → fixed. Build failed (fake validators must derive AbstractValidator; WPF App ambiguous with TopLab.Application namespace) → fixed. Build failed (RequestHandlerDelegate takes CancellationToken; closed-generic Result matching) → fixed. **JUDGE: build 0/0, tests 26/26, dep-rule OK → GREEN.**

## 5. Blockers / open questions
<!-- None yet -->

## 6. Resume pointer
Currently at feature: **F4**. Last green feature: **F3**.
