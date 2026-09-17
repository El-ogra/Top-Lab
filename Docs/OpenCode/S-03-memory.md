# Loop Engineering — Memory File

- **Module:** P2 UI Pass — PatientRegistration (M02) completion, PatientBilling (M03) screens, WorkSheets screen (S-03 — cross-module UI workstream, post-S-02)
- **Module Number:** S-03
- **Source Plan:** Docs/OpenCode/S-03.md (execution slices) + «خطة التنفيذ النهائية للواجهات والنوافذ الرسوميه — P2» (authoritative requirements)
- **Date Created:** 2026-09-17
- **Total Slices:** 8
- **Current Slice:** 1 — Next (Slice 0 complete + committed).
- **Current Branch:** main
- **Baseline Commit:** `ef99502051d9405632915915c0c214beec8c84d6` ("[S-02] Slice 7/8: record final commit hash in memory file — loop-engineering", 2026-09-17 13:26:35 +0300) — must be HEAD at Slice 0 Stage 1; `git status --porcelain` must be clean.
- **Author:** loop-engineering skill (execution carried out by the local executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

S-03 executes the P2 UI pass over three units: PatientRegistration M02 editor completion (visible Loading indicator + empty states; search-to-edit section consuming the orphaned `SearchPatientsQuery`; read-only visit-history section consuming the orphaned `GetPatientVisitHistoryQuery`; wiring the three orphaned commands `AddProfileToVisit`/`AddCustomGroupToVisit`/`ClearAllTests`), PatientBilling M03 (new `PatientAccountView` entered from the editor's totals row; correction/extra-charge/void dialogs; invoice preview with the SD-3 new-number-per-print rule visible), and WorkSheets (new three-mode screen — visit / test-group / work-group-log — plus summary + period-count sections, settling the «ورقة العمل» shell navigation debt). All backends exist at the pinned commit; NO new backend artifact and NO EF migration is authorised anywhere in this workstream.

## Global Validation Gates

- **Gate G0 (pre-execution):** `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- **Gate G1 (post-execution per slice):** same as G0 plus the slice-specific gate listed below, plus the zero-drift gate (`git diff --stat src/TopLab.Infrastructure/Persistence/` empty; `dotnet ef migrations has-pending-model-changes --project src/TopLab.Infrastructure --startup-project src/TopLab.Presentation` → "No changes").

## Quality Gate (non-negotiable — a slice may be marked complete ONLY when ALL FOUR hold)

1. The slice's implementation is fully complete per `Docs/OpenCode/S-03.md` + the P2 plan sections it cites.
2. The ENTIRE solution builds successfully — zero errors AND zero warnings (`dotnet build TopLab.sln`).
3. ALL existing automated tests across ALL test projects pass (`dotnet test TopLab.sln`).
4. The slice's own specific Validation Gate (VG-0N below) passes with recorded evidence (build/test output, grep/diff results, manual-checklist outcomes).

## Stop/Continue Rule

After a slice completes, verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice, with no pause and no human confirmation required.
If any one fails → retry. If the SAME failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs **5 CONSECUTIVE** times, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do NOT proceed past this point without owner review. Ordinary expected test failures caused by the current slice and resolved within the same correction cycle do NOT count as five separate failures.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: **5 consecutive failures for the same reason.**
- Execution order: strictly sequential **S0 → S1 → S2 → S3 → S4 → S5 → S6 → S7**, no parallel slices.
- Stage-7 gate: the plan's textual exit criteria (build/test/grep/inspection + recorded manual verification) — this workstream's UI behaviour is verified manually per plan (no UI test harness exists in the repo; do not invent one).
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (`main`), NEVER create a new branch, NEVER push to any remote, NEVER force-push, NEVER modify or rewrite remote history. Commit message format: `[S-03] Slice N/8: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in S-03's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 0 | VG-01 | Editor state surfacing: build 0/0; suite green; zero-drift; inspection — a visible element bound to `IsBusy`, both empty-state texts verbatim, no new confirmation on any cancel/undo path (D8); translator file physically located and texts recorded; manual — indicator during IO, empty→filled transitions, delete still confirms. | `dotnet build`; `dotnet test`; ef drift check; inspection; recorded manual walk |
| 1 | VG-02 | Search-to-edit: build 0/0; suite green; zero-drift; grep — `SearchPatientsQuery` has a non-test consumer, no new backend type, open path uses only `LoadPatientAsync`, no confirmation on open (D8); manual — paging, >200-char verbatim message, deleted row flagged/refused, «لا نتائج مطابقة.», silent discard of unsaved edits. | build/test; ef drift; grep; manual walk |
| 2 | VG-03 | Visit history: build 0/0; suite green; zero-drift; grep — `GetPatientVisitHistoryQuery` has a non-test consumer, section read-only, DTO-fields only; manual — sibling visits ordered with tests, single-visit empty text, create-mode absence, refresh after «تراجع». | build/test; ef drift; grep; manual walk |
| 3 | VG-04 | Orphaned-action wiring: build 0/0; suite green; zero-drift; grep — all three commands have non-test consumers, no new Application type, no category grouping in conditions UI; manual — profile/group adds with verbatim refusals, clear-all confirm + 24h/resulted refusals, create-mode selection unchanged. | build/test; ef drift; grep; manual walk |
| 4 | VG-05 | Account screen: build 0/0; suite green; zero-drift; grep — `GetPatientAccountQuery`/`ListPatientPaymentsQuery` consumers, no local totals arithmetic, exactly one DataTemplate + one DI registration, no `BLOCK_PRINT_ON_BALANCE` enforcement; manual — totals-row entry, header + 4 cards, voided rows flagged, refresh, empty-account text. | build/test; ef drift; grep; manual walk |
| 5 | VG-06 | Financial dialogs: build 0/0; suite green; zero-drift; grep — 3 commands consumed, no edit/delete affordance, no reason field, no discount field on extra charge, all mutations via `ShowConfirmationAsync`; manual — correction/extra-charge/void effects, double-void verbatim message, permission denial verbatim. | build/test; ef drift; grep; manual walk |
| 6 | VG-07 | Invoice preview: build 0/0; suite green; zero-drift; inspection — binds only `InvoiceDto`/`ChargedTestDto`, null-number case handled, SD-3 explanatory line present, print via `PrintInvoiceCommand`; manual — preview, increasing numbers on repeated prints, verbatim settings-missing message. | build/test; ef drift; inspection; manual walk |
| 7 | VG-08 | WorkSheets screen: build 0/0; suite green; zero-drift; grep — «ورقة العمل» branch wired, all 5 queries consumed, no new print command, group/log print disabled, no LabId field; manual — three modes, period defaulting, verbatim errors, summary/count grids + empty texts, permission denial verbatim, visit-mode print. | build/test; ef drift; grep; manual walk |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 0 | Patient editor state surfacing (loading + empty states) | [x] Complete (committed) | VG-01 PASS |
| 1 | Patient search-to-edit section in editor | [ ] Not started | VG-02 |
| 2 | Visit-history section in patient editor | [ ] Not started | VG-03 |
| 3 | Editor wiring for profile / custom-group / clear-all | [ ] Not started | VG-04 |
| 4 | Patient account screen | [ ] Not started | VG-05 |
| 5 | Billing dialogs (correction + extra charge + void) | [ ] Not started | VG-06 |
| 6 | Invoice preview before print | [ ] Not started | VG-07 |
| 7 | WorkSheets screen (three modes + summary/count) + shell wiring | [ ] Not started | VG-08 |

---

## Settled Decisions (owner-approved, final — binding; do NOT reopen)

- **D8 (verbatim owner text):** "When the user modifies patient data … and has NOT clicked Save, then clicks Cancel (or otherwise closes/exits the editor without saving): the operation is cancelled immediately, with no confirmation or warning dialog of any kind. … there is nothing to protect the user from, because no data was ever removed or overwritten in storage." → No unsaved-changes confirmation anywhere in P2, INCLUDING the new search section (opening a patient over an unsaved form discards edits silently). Matches existing code behaviour; documentation-binding only.
- **D2 (code-settled by the P2 plan's direct inspection — adopted):** medical-conditions display = existing partial display stays (CheckBoxes; category grouping is RLS-Inspiration-Only and NOT built); visit history = new read-only section on the existing editor view.
- **Void settlement (Top-Lab wins over RLS):** void sets `IsVoided=true` only — no reverse operation, no record deletion, no amount editing. The only correction flow is void-and-reissue. No «تعديل مبلغ» / «حذف عملية» affordance anywhere.
- **SD-3:** every invoice print issues a new sequential number; the preview must say so.
- **SD-9:** totals/money come from `GetPatientAccountQuery` exclusively — no client-side arithmetic; UI keeps buttons visible and surfaces backend permission denials («أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام»).
- **Commit format:** `[S-03] Slice N/8: <slice title> — loop-engineering`.
- **No half-wired state:** every enabled control has a complete backend path; anything else ships disabled per the repo's placeholder idiom (applies to group/log worksheet print until the owner decides).
- **No migration:** zero-drift gate on every slice; any discovered need for one is a STOP-and-report, never a silent migration. No new backend command/query; no `PermissionConfiguration` change.

## Unresolved Owner Decisions (preserve as unresolved — build around, never silently decide)

- **`BLOCK_PRINT_ON_BALANCE` enforcement** («بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»): the code is seeded/displayed but unenforced in billing. P2 does NOT enforce it. Do not add any balance-block to receipt/invoice printing.
- **Visit-worksheet entry by LabId** («بانتظار قرار المالك»): visit mode accepts integer PatientId only; `PatientSearch.GetPatientByLabIdQuery` (M08) is NOT consumed.
- **Print path for group/work-group-log worksheet modes** («بانتظار قرار المالك»): no print command exists; the affordance ships disabled; no new command is created.
- **New UI texts requiring creation** (not existing messages — created at execution and recorded here): «لا توجد تحاليل مختارة.», catalog empty text, «لا نتائج مطابقة.», «لا توجد زيارات سابقة.», «لا توجد عمليات مسجلة.», «لا توجد عناصر مطابقة في الفترة المحددة.», S-WS-2 empty texts, financial confirmation texts, invoice SD-3 explanatory line, clear-all confirmation text.

## Confirmed Facts (independently re-verified at `ef99502` — ground truth, do not re-derive)

- Baseline: commit `ef99502…` is `main` HEAD; tree clean.
- M02: 12 command folders / 7 query folders; NO validators for `UpdatePatient`/`ClearAllTests`/`RemoveMedicalCondition`/`SoftDeletePatient`; `GetPatientVisitHistory` + `SearchPatients` have ZERO Presentation consumers; `PatientsHubViewModel` → `NavigateTo<PatientEditorViewModel>()`; both VMs transient in `DependencyInjection.cs:41–42`.
- M03: 7 commands + 4 queries + `DomainFailureTranslator` + `PatientBillingReader` (voided rows returned flagged) + `ReceiptPrintEnvelope`/`InvoicePrintEnvelope`; NO dedicated billing View/VM exists; `BLOCK_PRINT_ON_BALANCE` present in `src` as seed/display only.
- WorkSheets: 5 queries + `PrintWorkSheetCommand` + `WorkSheetsAccessPolicy.PrintWorksheet="PRINT_WORKSHEET"` + `WorkSheetHelpers.WorkSheetPeriod` (UTC-today default, `To ??= From`, inclusive); no group/log print command; shell titles include «ورقة العمل» (`ShellViewModel.cs:159`) with fall-through branch (`:226`).
- `PatientSearch.GetPatientByLabIdQuery` exists (M08) — available only if the owner approves.
- `CreatePatient.DomainFailureTranslator`: referenced by handlers per the P2 plan but no standalone file under `Features/PatientRegistration/Common/` → **U-01: locate and record at Slice 0 Stage 3.**
- S-02 test baseline at completion: Domain 474 + Infrastructure 195 + Application 1419 = 2088 green — re-confirm current counts at Slice 0 Stage 1.

## Uncertainty / Verification Register

| ID | Item | Class | Resolution |
|----|------|-------|-----------|
| U-01 | `CreatePatient.DomainFailureTranslator` physical location + verbatim texts | Uncertain (carried from P2 plan) | RESOLVED Slice 0 Stage 3 — NOT a standalone file: `internal static class DomainFailureTranslator` at the bottom of `src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommandHandler.cs:215-227`. Verbatim texts: fullName→«اسم المريض مطلوب.»; ageValue→«العمر يجب أن يكون صفرًا أو أكثر.»; fastingHours→«ساعات الصيام تتطلب تحديد الصيام.»; default→«بيانات المريض غير صالحة.». Surfaced via handlers through `ResultErrorPresenter` — no UI action needed. |
| U-02 | `SearchPatientsQueryHandler` exact filter channels | Uncertain (handler unopened in P2 plan) | OPEN — verify at Slice 1 Stage 3; adapt UI hints only, never the handler |
| U-03 | Busy-indicator idiom (existing converter/control precedent) | Inference | RESOLVED Slice 0 Stage 3 — repo idiom = collapsed-by-default + `DataTrigger` on a bool VM property (precedents: `TestCatalogView.xaml:15-26` `ShowFilteredEmpty`, `AnalytesView.xaml:12-23` `ShowEmpty`; VM pattern `TestCatalogViewModel.cs:109-140` computed `Show*Empty` + `RefreshEmptyStates()`). App-wide `BoolToVis` resource exists (`App.xaml:6`; resolves to WPF built-in converter) but NO view bound `IsBusy` before Slice 0 (grep `IsBusy` over `*.xaml` = 0 hits) — Slice 0 is the first consumer, using the dominant DataTrigger idiom. |
| U-04 | Profiles/custom-groups picker source inside the registration catalog | Inference | OPEN — resolve at Slice 3 Stage 3; if no catalog surface exists → STOP-and-report |
| U-05 | Account-screen navigation idiom (`LoadAsync(patientId)` after parameterless `NavigateTo<T>`) | Inference (structural, per P2 plan) | ADOPTED unless contradicted at Slice 4 Stage 3; record outcome |
| U-06 | New UI texts (see Unresolved Owner Decisions list) | يتطلب إنشاء | PARTIAL — Slice 0 created+recorded: «لا توجد تحاليل مختارة.» (plan-verbatim, `PatientEditorView.xaml:162`) + «لا توجد تحاليل في الكتالوج.» (created at execution — the P2 plan's proposed catalog text is not quoted in `S-03.md` and the P2 plan file is absent from the repo; parallel phrasing adopted and recorded here, `PatientEditorView.xaml:193`). Remaining texts still open for their slices. |

---

## Slice 0: Patient editor state surfacing (loading + empty states)

- **Goal:** Visible `IsBusy` indicator + empty states («لا توجد تحاليل مختارة.» + catalog empty) in `PatientEditorView`; Error/Confirmation verified as-is; D8 grep gate; U-01/U-03 resolved.
- **Touches:** `Views/Patients/PatientEditorView.xaml` (modify); possibly `PatientEditorViewModel.cs` (empty-state visibility only).
- **Validation Gate:** VG-01.

### 10-Stage Progress (Slice 0)

- [x] **Stage 1 — Pre-Execution Verification:** HEAD `ef99502051d9405632915915c0c214beec8c84d6` on `main` confirmed; `git status` showed only the two untracked S-03 input files (expected — owner-placed); `dotnet build TopLab.sln` 0 errors/0 warnings; `dotnet test TopLab.sln` green 474+195+1419=2088 (matches S-02 baseline exactly).
- [x] **Stage 2 — Deep Understanding:** S-03.md §4 Slice 0 read in full (P2 M02 §4 state rows; P2 plan file itself absent from repo — S-03.md is the requirements surface).
- [x] **Stage 3 — File Analysis:** `PatientEditorView.xaml` (184 lines, no IsBusy binding, red `ErrorMessage` + `StatusMessage` present, delete-only confirmation path), `PatientEditorViewModel.cs` (`IsBusy` managed around all IO; `UndoAsync:827-838` no dialog — D8 as-is; `DeleteAsync:801` delete-only confirmation as-is), `ResultErrorPresenter.cs:7,17` (Forbidden/Unexpected verbatim as specified), empty-state precedent `TestCatalogView.xaml`/`AnalytesView.xaml` + `TestCatalogViewModel.cs:109-140`. U-01/U-03 resolved (see register); U-06 partial (catalog text created+recorded).
- [x] **Stage 4 — Planning:** (1) VM: `ShowSelectedTestsEmpty`/`ShowCatalogEmpty` computed props + `CollectionChanged` subscriptions + `RefreshEmptyStates()`; (2) XAML: indeterminate `ProgressBar` bound to `IsBusy`, two empty-state `TextBlock`s in repo DataTrigger idiom; (3) no VM logic/behaviour change; no new backend; no confirmation anywhere near cancel/undo.
- [x] **Stage 5 — Execution:** `PatientEditorViewModel.cs` — ctor subscriptions + 2 props + `RefreshEmptyStates()`; `PatientEditorView.xaml` — `ProgressBar` (lines 6-20), «لا توجد تحاليل مختارة.» (line 162), «لا توجد تحاليل في الكتالوج.» (line 193). No other files touched.
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings (XAML compiled clean).
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — (1) build 0/0 + full suite green 474+195+1419=2088; (2) zero-drift: `git diff --stat src/TopLab.Infrastructure/Persistence/` empty + `dotnet ef migrations has-pending-model-changes` → «No changes have been made to the model since the last migration.»; (3) inspection: `IsBusy` DataTrigger (`PatientEditorView.xaml:12`), both empty texts verbatim (lines 162, 193), `ShowConfirmationAsync` occurs once — pre-existing delete path (`PatientEditorViewModel.cs:817`), nothing on cancel/undo; (4) manual walk RECORDED AS INSPECTION (no interactive run — no live DB/UI session provisioned): fresh form → both collections empty → both empty texts visible; catalog load / test add clears them via `CollectionChanged`; `IsBusy=True` during `LoadCatalogAsync`/`LoadPatientAsync`/`SaveAsync` shows the bar; delete still confirms; `UndoAsync` reloads with no dialog per code.
- [x] **Stage 8 — Documentation Update:** this section + register + status updated with evidence.
- [x] **Stage 9 — Memory Status Update:** see Current Status.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-03] Slice 0/8: Patient editor state surfacing (loading + empty states) — loop-engineering` (includes the two owner-placed S-03 plan files now tracked under `Docs/OpenCode/`).

---

## Slice 1: Patient search-to-edit section in editor

- **Goal:** Search bar + paged results grid in `PatientEditorView` consuming `SearchPatientsQuery`; row select → `LoadPatientAsync`; deleted rows flagged/non-openable; D8 silent discard.
- **Touches:** `PatientEditorViewModel.cs`, `PatientEditorView.xaml` (modify).
- **Validation Gate:** VG-02.

### 10-Stage Progress (Slice 1)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; tree clean.
- [ ] **Stage 2 — Deep Understanding:** S-03.md §4 Slice 1 + P2 M02 §4 S-M02-3 read in full.
- [ ] **Stage 3 — File Analysis:** `SearchPatientsQuery(.Handler/Validator)` — resolve U-02 (record actual filter channels); `PatientSummaryDto`; `LoadPatientAsync`; paging precedent.
- [ ] **Stage 4 — Planning:**
- [ ] **Stage 5 — Execution:**
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-02 (record evidence).
- [ ] **Stage 8 — Documentation Update:**
- [ ] **Stage 9 — Memory Status Update:**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-03] Slice 1/8: Patient search-to-edit section in editor — loop-engineering`.

---

## Slice 2: Visit-history section in patient editor

- **Goal:** Read-only visit-history section consuming `GetPatientVisitHistoryQuery`; edit mode only; «لا توجد زيارات سابقة.» empty text.
- **Touches:** `PatientEditorViewModel.cs`, `PatientEditorView.xaml` (modify).
- **Validation Gate:** VG-03.

### 10-Stage Progress (Slice 2)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; tree clean.
- [ ] **Stage 2 — Deep Understanding:** S-03.md §4 Slice 2 + P2 M02 §4 S-M02-5 + D2 settlement read in full.
- [ ] **Stage 3 — File Analysis:** `GetPatientVisitHistoryQuery(.Handler)`, `VisitHistoryDto`/`PatientTestSummaryDto`, editor load path.
- [ ] **Stage 4 — Planning:**
- [ ] **Stage 5 — Execution:**
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-03 (record evidence).
- [ ] **Stage 8 — Documentation Update:**
- [ ] **Stage 9 — Memory Status Update:**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-03] Slice 2/8: Visit-history section in patient editor — loop-engineering`.

---

## Slice 3: Editor wiring for profile / custom-group / clear-all

- **Goal:** Bind `AddProfileToVisitCommand`, `AddCustomGroupToVisitCommand`, `ClearAllTestsCommand` in the editor (edit mode); clear-all confirmation (new text, record); conditions grouping NOT built; disabled-per-idiom if any path is incomplete.
- **Touches:** `PatientEditorViewModel.cs`, `PatientEditorView.xaml` (modify); picker per repo idiom (U-04).
- **Validation Gate:** VG-04.

### 10-Stage Progress (Slice 3)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; tree clean.
- [ ] **Stage 2 — Deep Understanding:** S-03.md §4 Slice 3 + P2 M02 §2 command table + §4 action table read in full.
- [ ] **Stage 3 — File Analysis:** three commands + validators; catalog DTO for picker sources (U-04); `IDialogService` idioms.
- [ ] **Stage 4 — Planning:**
- [ ] **Stage 5 — Execution:**
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-04 (record evidence).
- [ ] **Stage 8 — Documentation Update:**
- [ ] **Stage 9 — Memory Status Update:**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-03] Slice 3/8: Editor wiring for profile / custom-group / clear-all — loop-engineering`.

---

## Slice 4: Patient account screen

- **Goal:** `PatientAccountView(Model)`; entry via clickable totals row in the editor; header + 4 cards from `GetPatientAccountQuery`; operations grid; refresh/payment/settle/receipt wired; dialog/invoice affordances staged per no-half-wired idiom; NO `BLOCK_PRINT_ON_BALANCE` enforcement.
- **Touches:** CREATE `PatientAccountViewModel.cs` + `PatientAccountView.xaml(.cs)`; WIRE `MainWindow.xaml` DataTemplate + DI; MODIFY `PatientEditorView(.xaml/ViewModel)` totals row.
- **Validation Gate:** VG-05.

### 10-Stage Progress (Slice 4)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; tree clean.
- [ ] **Stage 2 — Deep Understanding:** S-03.md §4 Slice 4 + P2 M03 §4 S-M03-2 read in full.
- [ ] **Stage 3 — File Analysis:** `GetPatientAccountQuery`/`ListPatientPaymentsQuery` + DTOs; `PatientBillingReader` voided-flag behaviour; navigation idiom (U-05); folder convention for new VMs/Views.
- [ ] **Stage 4 — Planning:**
- [ ] **Stage 5 — Execution:**
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-05 (record evidence).
- [ ] **Stage 8 — Documentation Update:**
- [ ] **Stage 9 — Memory Status Update:**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-03] Slice 4/8: Patient account screen — loop-engineering`.

---

## Slice 5: Billing dialogs (correction + extra charge + void)

- **Goal:** Three dialogs from the account screen; correction = amount only (no reason field); extra charge = amount only (no discount); void from selected row with confirmation; all mutations confirmed via `IDialogService`; void = flag only (no reverse op, no delete, no edit).
- **Touches:** CREATE dialog Views/VMs ×3; MODIFY `PatientAccountViewModel.cs`/`PatientAccountView.xaml` (enable toolbar); DI per idiom.
- **Validation Gate:** VG-06.

### 10-Stage Progress (Slice 5)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; tree clean.
- [ ] **Stage 2 — Deep Understanding:** S-03.md §4 Slice 5 + P2 M03 §4 S-M03-3/4/5 + §7 settlement read in full.
- [ ] **Stage 3 — File Analysis:** `RecordCorrectionCommand`/`RecordExtraChargeCommand`/`VoidPaymentOperationCommand` (+ validators, handler comments incl. sign semantics + void-and-reissue note); S-02 dialog idiom.
- [ ] **Stage 4 — Planning:** (record the four new confirmation/success texts verbatim here).
- [ ] **Stage 5 — Execution:**
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-06 (record evidence).
- [ ] **Stage 8 — Documentation Update:**
- [ ] **Stage 9 — Memory Status Update:**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-03] Slice 5/8: Billing dialogs (correction + extra charge + void) — loop-engineering`.

---

## Slice 6: Invoice preview before print

- **Goal:** Preview from `GetPatientInvoiceQuery` (null number tolerated); print via `PrintInvoiceCommand`; SD-3 new-number-per-print explanatory line (new text, record); no receipt preview.
- **Touches:** MODIFY `PatientAccountViewModel.cs`/`PatientAccountView.xaml`; enable the Slice-4 invoice affordance.
- **Validation Gate:** VG-07.

### 10-Stage Progress (Slice 6)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; tree clean.
- [ ] **Stage 2 — Deep Understanding:** S-03.md §4 Slice 6 + P2 M03 §4 S-M03-6 read in full.
- [ ] **Stage 3 — File Analysis:** `GetPatientInvoiceQuery`/`PrintInvoiceCommand` + `InvoiceDto`/`ChargedTestDto`; `AllocateIssueAsync` numbering; `ReceiptSettings.Currency` surface.
- [ ] **Stage 4 — Planning:**
- [ ] **Stage 5 — Execution:**
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-07 (record evidence).
- [ ] **Stage 8 — Documentation Update:**
- [ ] **Stage 9 — Memory Status Update:**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-03] Slice 6/8: Invoice preview before print — loop-engineering`.

---

## Slice 7: WorkSheets screen (three modes + summary/count) + shell wiring

- **Goal:** «ورقة العمل» shell branch → `WorkSheetsView(Model)`; three exclusive modes (visit by PatientId / test group / work-group log) + From/To period; sections grid; S-WS-2 summary + period-count grids; visit-mode print via existing command; group/log print ships DISABLED (owner-pending); no LabId entry (owner-pending).
- **Touches:** CREATE `WorkSheetsViewModel.cs` + `WorkSheetsView.xaml(.cs)`; MODIFY `ShellViewModel.cs` (one branch), `MainWindow.xaml` (one DataTemplate), `DependencyInjection.cs` (one transient).
- **Validation Gate:** VG-08.

### 10-Stage Progress (Slice 7)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; tree clean.
- [ ] **Stage 2 — Deep Understanding:** S-03.md §4 Slice 7 + P2 WorkSheets §3/§4 read in full.
- [ ] **Stage 3 — File Analysis:** all five WorkSheets queries + `PrintWorkSheetCommand`; `GetTestGroupsQuery`/`GetWorkGroupLogsQuery` DTO shapes; `WorkSheetHelpers.WorkSheetPeriod`; the three wiring sites (S-01/S-02 recipe).
- [ ] **Stage 4 — Planning:**
- [ ] **Stage 5 — Execution:**
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-08 (record evidence).
- [ ] **Stage 8 — Documentation Update:**
- [ ] **Stage 9 — Memory Status Update:**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-03] Slice 7/8: WorkSheets screen (three modes + summary/count) + shell wiring — loop-engineering`.

---

## Current Status

- **Programme:** S-03 (P2 UI pass) — IN PROGRESS (Slice 0/8 complete).
- **Completed slices:** Slice 0 (VG-01 pass, committed) — 1/8.
- **Blocked slices:** none.
- **Exact next action:** Slice 1, Stage 1 — verify build 0/0 + suite green + tree clean (post-Slice-0 commit), then read S-03.md §4 Slice 1.
- **Conditions before proceeding:** G0 green on the Slice-0 commit.
- **Open uncertainties:** U-02, U-04, U-05 (+ U-06 remainder for later slices).
- **Owner-pending (never silently decide):** `BLOCK_PRINT_ON_BALANCE` enforcement; LabId visit-worksheet entry; group/log worksheet print path.
- **Evidence collected (Slice 0):** build 0/0 (pre+post); tests 474+195+1419=2088 green (pre+post); EF «No changes»; IsBusy trigger line 12, empty texts lines 162/193; single pre-existing delete confirmation line 817.
- **Changed files (Slice 0):** `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs`, `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` (+ `Docs/OpenCode/S-03.md`, `Docs/OpenCode/S-03-memory.md` newly tracked).
- **Deviations:** (1) P2 plan file absent from repo — S-03.md used as requirements surface; (2) catalog empty text created at execution («لا توجد تحاليل في الكتالوج.») as S-03.md quotes no verbatim for it — recorded in U-06; (3) manual walk by inspection (no live DB/UI session) — explicitly recorded, not claimed as interactive.
- **Lessons learned:** `BoolToVis` app resource resolves to the WPF built-in converter; the dominant empty-state idiom is `Show*Empty` + DataTrigger (not BoolToVis); `CreatePatient.DomainFailureTranslator` lives inside its handler file.

## Stop Report

(none)
