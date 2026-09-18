# Loop-Engineering Execution Prompt — Workstream S-05 (P4 UI Pass)

> Convention note: this file follows the repository's `Docs/OpenCode/Execution-Prompts.md` pattern (one self-contained, copy-paste-verbatim prompt per workstream for a local coding agent). Prior passes keep their prompts in that cumulative file; this standalone file carries the S-05 prompt for the P4 package. Prompt numbering continues that file's series.

## Prompt 7 — Workstream S-05: P4 UI Pass — PatientSearch (M08), ReportProduction (M07), ResultDelivery (M09), SentOutSamples (M16)

Copy everything below the line verbatim to the local coding agent.

---

You are the local executing agent for Top-Lab workstream **S-05** (the P4 UI pass), operating under the loop-engineering protocol already established in this repository by workstreams S-00 through S-04.

**Repository:** https://github.com/El-ogra/Top-Lab.git (already cloned locally — work inside that clone).
**Pinned commit:** `0e66b014d5f3889e0ec11e15398f00ce967e196d` («[S-04] Slice 7/8: Culture results screens + worklist routing — loop-engineering»). Before anything else, run `git rev-parse HEAD` and `git status --porcelain`: HEAD must equal the pinned commit and the tree must be clean. If either fails, STOP and report — do not proceed on any other commit.

**Your two source documents (read both fully before writing any code):**
1. `Docs/OpenCode/S-05.md` — the execution plan: six dependency-ordered slices, each with scope, verbatim-bound specification, a Validation Gate (VG-01…VG-06), blocking conditions, and a commit message.
2. `Docs/OpenCode/S-05-memory.md` — the memory file: frozen Arabic message tables, confirmed code facts, settled decisions, slice validation gates, the 10-stage checklists, the created-UI-texts register, and the execution log. You update this file at every stage that requires it.

**Mission:** implement the complete WPF Presentation layer for four modules whose backends are already finished and verified — M08 (patient search + visit history), M07 (combined/blank/history reports), M09 (result delivery with atomic settlement), M16 (sent-out samples) — and close the two disabled gateways in `PatientsHubViewModel` («بحث عن مريض» for M08, «تسليم نتائج المرضى» for M09). You are building UI only. **You are forbidden from touching** `src/TopLab.Domain/`, `src/TopLab.Application/`, and `src/TopLab.Infrastructure/`: no new or changed commands, queries, DTOs, validators, entities, permissions, or migrations. If any specification element appears to require one, that is a STOP condition — report it; never improvise a backend change.

**The slice loop (execute slices strictly in order 0 → 5; each slice runs the full 10-stage cycle):**

1. **Pre-Execution Verification** — build is 0/0, full test suite green, tree clean, HEAD still pinned.
2. **Deep Understanding** — read the slice's spec in S-05.md and every backend file it cites (queries, commands, DTOs, validators, handlers). The spec's claims were audited, but you verify the exact member names yourself before binding to them.
3. **File Analysis** — open the presentation-side files you will touch or imitate: the relevant `PatientsHubViewModel`/existing ViewModels whose patterns you mirror, `DependencyInjection.cs`, `MainWindow.xaml`, `IDialogService`, `ResultErrorPresenter`, `INavigationService`.
4. **Planning** — write down (in the memory file's stage notes) the exact new/modified files. For Slice 5 only: confirm the owner has settled the M16 temporary-entry placement; if not, STOP and report (that decision is recorded open — «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية» — you may not decide it yourself).
5. **Execution** — implement. Rules that bind every line:
   - **Verbatim Arabic:** any user-facing string with a backend counterpart is copied byte-for-byte from the backend file (the frozen message tables in the memory file). UI-only strings (empty-state texts, confirmation texts) are created once and appended to the memory file's Created UI Texts Register.
   - **Established patterns only:** `ViewModelBase.SetProperty`, `RelayCommand`/`AsyncRelayCommand`, `ISender` (MediatR) injection, the navigation idiom `_navigation.NavigateTo<XViewModel>(); if (_navigation.CurrentViewModel is XViewModel vm) { await vm.LoadAsync(...); }`, `ResultErrorPresenter` for every error surface, `IDialogService.ShowConfirmationAsync` for every confirmation the spec mandates, DI via `services.AddTransient<...>()` in `AddPresentation`, View↔ViewModel binding via `DataTemplate` in `MainWindow.xaml`.
   - **No-half-wired-state:** any button whose target screen belongs to a later slice ships present but disabled, and is enabled by that later slice's commit.
   - **Four states everywhere:** every screen implements Loading / Empty / Error / Confirmation exactly as its spec defines (including where Confirmation is explicitly not applicable).
   - **Settled decisions are binding:** D10 is closed — both M16 lab ComboBoxes are sourced from `SearchExternalEntitiesQuery(EntityType: EntityType.PartnerLab, SearchTerm: null, Page: 1, PageSize: 100)` projecting `Id`/`Name`; do not create any new query, and do not label or treat D10 as open anywhere. Do not add permission codes, Word export, NATIGH.COM integration, or a printed sent-out statement — all explicitly out of scope.
6. **Post-Execution Verification** — `dotnet build` 0/0; full suite green; zero-drift proof (`git diff --stat` on the three backend layers is empty; `dotnet ef migrations has-pending-model-changes` reports no changes).
7. **Validation Gate** — evaluate the slice's VG (S-05.md) item by item: build/tests/zero-drift, the grep/inspection checks, and the manual scenarios (run the app, perform each listed scenario, record the outcome in the memory file). Every item must pass; a failed item you cannot fix within the spec is a STOP.
8. **Documentation Update** — update the memory file's slice section: check off stages, record gate results, append created UI texts to the register.
9. **Memory Status Update** — mark the slice Done in the Slice Index and Current Status; set the next slice as current.
10. **Git Commit** — one local commit per slice with the exact message from S-05.md (`[S-05] Slice N/6: … — loop-engineering`).

**Git policy (absolute):** local commits only, one per slice, with the prescribed messages. **Never push.** Never amend, rebase, reset, force-push, create branches, or otherwise alter Git history. Never commit on a red build or a failed gate.

**Stop rules (any one halts the loop immediately — record in the memory file's Stop Report and wait):**
- A spec element requires a backend change of any kind.
- A live backend message contradicts the frozen message table.
- Build or tests go red and cannot be restored within the slice's scope.
- Slice 5 reaches Stage 4 without the owner's placement decision for the M16 temporary entry.
- Any ambiguity that is not already settled in S-05.md §3 — report it as «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»; do not decide it yourself.

**Slice map (details in S-05.md §4):**
- Slice 0 — Patient search screen (M08) + «بحث عن مريض» gateway activation → VG-01
- Slice 1 — Patient visit history master-detail (M08) + cross-module button skeletons → VG-02
- Slice 2 — Combined report + insert-history dialog (M07) → VG-03
- Slice 3 — Blank report + history reports, three modes (M07) → VG-04
- Slice 4 — Result delivery screens + «تسليم نتائج المرضى» gateway (M09) → VG-05
- Slice 5 — Sent-out samples screens + temporary entry point (M16, D10 closed) → VG-06

Begin with Slice 0, Stage 1 now.
