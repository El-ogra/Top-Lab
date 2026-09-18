# Loop-Engineering Execution Prompt — Workstream S-06 (P5 UI Pass)

> Convention note: this file follows the repository's `Docs/OpenCode/Execution-Prompts.md` pattern (one self-contained, copy-paste-verbatim prompt per workstream for a local coding agent). Prompts 1–6 live in that cumulative file (Modules 3, 4, 5, 6, 8, 11); `S-05-Execution-Prompt.md` carries Prompt 7; this standalone file carries Prompt 8 for the S-06 package. Prompt numbering continues that series.

## Prompt 8 — Workstream S-06: P5 UI Pass — AuditAndTraceability (M10), Attendance (M18), Statistics (M19), InventoryAndAccounting (M20), Utilities (M23)

Copy everything below the line verbatim to the local coding agent.

---

You are the local executing agent for Top-Lab workstream **S-06** (the P5 UI pass), operating under the loop-engineering protocol already established in this repository by workstreams S-00 through S-05.

**Repository:** https://github.com/El-ogra/Top-Lab.git (already cloned locally — work inside that clone).
**Pinned commit:** `c34b9ad35022684f0b65ab2cfe4e6b84c511d59f` («[S-05] Slice 5/6: Sent-out samples screens + temporary entry point — loop-engineering»). Before anything else, run `git rev-parse HEAD` and `git status --porcelain`. **Baseline check (adapted for the package files):** HEAD must equal the pinned commit, OR be a descendant of it whose diff against the pinned commit touches ONLY the S-06 package files under `Docs/OpenCode/` (`S-06.md`, `S-06-memory.md`, `S-06-Execution-Prompt.md`); the working tree may contain only those package files as untracked/added. Anything else → STOP and report — do not proceed on any other state.

**Your two source documents (read both fully before writing any code):**
1. `Docs/OpenCode/S-06.md` — the execution plan: seven dependency-ordered slices, each with scope, verbatim-bound specification, a Validation Gate (VG-01…VG-07), blocking conditions, and a commit message.
2. `Docs/OpenCode/S-06-memory.md` — the memory file: frozen Arabic message tables, confirmed code facts, settled decisions, the recorded open decision, slice validation gates, the 10-stage checklists, the created-UI-texts register, and the execution log. You update this file at every stage that requires it.

**Mission:** implement the complete WPF Presentation layer for five modules whose backends are already finished and verified — M10 (patient/test audit, P/T tabs), M18 (self-service attendance + admin records/summary), M19 (four-section statistics dashboard), M20 (accounts hub with four tabs + cash movement dialog), M23 (six-tab utilities + two add dialogs) — and settle the four shell-navigation debts: wire «الأدوات», «الإحصائيات», «النظام», and activate «الحسابات» (absorbing the two temporary settings-dashboard routes and closing the D3/D10 comments). You are building UI only. **You are forbidden from touching** `src/TopLab.Domain/`, `src/TopLab.Application/`, and `src/TopLab.Infrastructure/`: no new or changed commands, queries, DTOs, validators, entities, permissions, or migrations. If any specification element appears to require one, that is a STOP condition — report it; never improvise a backend change.

**The slice loop (execute slices strictly in order 0 → 6; each slice runs the full 10-stage cycle):**

1. **Pre-Execution Verification** — build is 0/0, full test suite green, tree clean, baseline check above still holds.
2. **Deep Understanding** — read the slice's spec in S-06.md and every backend file it cites (queries, commands, DTOs, validators, handlers). The spec's claims were audited, but you verify the exact member names yourself before binding to them.
3. **File Analysis** — open the presentation-side files you will touch or imitate: `ShellViewModel` (navigation wiring), `SettingsDashboardViewModel`/`.xaml` (temporary routes), the existing hub patterns (`PatientsHubViewModel`, `LabHubViewModel`), `DependencyInjection.cs`, `MainWindow.xaml`, `IDialogService`, `ResultErrorPresenter`, `INavigationService`.
4. **Planning** — write down (in the memory file's stage notes) the exact new/modified files. **For Slice 1 only: confirm the owner has settled the attendance entry point; if not, STOP and report (that decision is recorded open — «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»).**
5. **Execution** — implement. Binding rules:
   - **Verbatim Arabic:** every user-facing string with a backend counterpart is copied byte-for-byte from the frozen message tables in the memory file. UI-only strings are created, used, and appended to the Created UI Texts Register.
   - **Established patterns only:** hand-rolled MVVM (`ViewModelBase.SetProperty`), `RelayCommand`/`AsyncRelayCommand`, the `_navigation.NavigateTo<XViewModel>(); if (_navigation.CurrentViewModel is XViewModel vm) { await vm.LoadAsync(...); }` idiom, `IDialogService` for dialogs/confirmations, `ResultErrorPresenter` for all backend errors. Do not introduce new frameworks, controls libraries, or patterns; **no new package reference of any kind** (D5 is settled: tables only — `Directory.Packages.props` and all `*.csproj` files stay untouched).
   - **No-half-wired-state:** every button/route whose target is not yet built ships present-but-disabled until its slice lands; the M14/M16 temporary routes move atomically in Slice 4 — never remove them before the Accounts hub routes are live.
   - **Four states everywhere:** Loading / Empty / Error / Confirmation on every screen and tab, with the spec's justification wherever a state is not applicable.
   - **Settled decisions are binding:** D5 (M19: tables and aggregate numbers only — no charting library, no new package reference), D11 (M23: every elapsed-time computation goes through `ComputeStopwatchElapsedQuery`; no local elapsed arithmetic in the Presentation layer), D12 (M10: the existing shell title «النظام» navigates directly to the audit screen — no new shell button). Do not label or treat any of the three as open anywhere. Do not add permission codes, Word export, NATIGH.COM/portal features, branch concepts, or audit period/event filters — all explicitly out of scope.
6. **Post-Execution Verification** — `dotnet build` 0/0; full suite green; zero-drift proof (`git diff --stat` on the three backend layers is empty; `dotnet ef migrations has-pending-model-changes` reports no changes).
7. **Validation Gate** — evaluate the slice's VG (S-06.md) item by item: build/tests/zero-drift, the grep/inspection checks (including the D5/D11/D12 decision gates in VG-03/VG-07/VG-06), and the manual scenarios (run the app, perform each listed scenario, record the outcome in the memory file). Every item must pass; a failed item you cannot fix within the spec is a STOP.
8. **Documentation Update** — update the memory file's slice section: check off stages, record gate results, append created UI texts to the register.
9. **Memory Status Update** — mark the slice Done in the Slice Index and Current Status; set the next slice as current.
10. **Git Commit** — one local commit per slice with the exact message from S-06.md (`[S-06] Slice N/7: … — loop-engineering`).

**Git policy (absolute):** local commits only, one per slice, with the prescribed messages. **Never push.** Never amend, rebase, reset, force-push, create branches, or otherwise alter Git history. Never commit on a red build or a failed gate.

**Stop rules (any one halts the loop immediately — record in the memory file's Stop Report and wait):**
- A spec element requires a backend change of any kind.
- A live backend message contradicts the frozen message tables.
- Build or tests go red and cannot be restored within the slice's scope.
- Slice 1 reaches Stage 4 without the owner's decision on the attendance entry point (the recorded open decision).
- Any ambiguity that is not already settled in S-06.md §3 — report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»; do not decide it yourself.

**Slice map (details in S-06.md §4):**
- Slice 0 — Attendance self-service screen «حضوري» (M18) → VG-01
- Slice 1 — Attendance admin screens + entry point (M18; STOP-gate on the recorded open decision) → VG-02
- Slice 2 — Statistics dashboard + «الإحصائيات» wiring (M19, D5 closed) → VG-03
- Slice 3 — Accounts hub + cash drawer + cash movement dialog + «الحسابات» activation (M20) → VG-04
- Slice 4 — Remaining accounts tabs + temporary-route absorption (M20; closes D3/D10) → VG-05
- Slice 5 — Audit screen + «النظام» wiring (M10, D12 closed) → VG-06
- Slice 6 — Utilities screen + «الأدوات» wiring (M23, D11 closed) → VG-07

Begin with Slice 0, Stage 1 now.
