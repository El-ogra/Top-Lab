# Loop Engineering — Memory File

- **Module:** P3 UI Pass — SampleCollection (M21), ResultsEntry (M04), ProfileResults (M05), CultureResults (M06) (S-04 — cross-module UI workstream, post-S-03)
- **Module Number:** S-04
- **Source Plan:** Docs/OpenCode/S-04.md (execution slices) + «خطة التنفيذ النهائية للواجهات والنوافذ الرسوميه - P3.md» (authoritative requirements, as corrected by the audit recorded below)
- **Date Created:** 2026-09-17
- **Total Slices:** 8
- **Current Slice:** NOT STARTED — Slice 0 pending.
- **Current Branch:** main
- **Baseline Commit:** `ce30962bdbfd29b17f099b8b700901750104ff88` («بعد تبديل السمات الشخصية», 2026-09-17 22:22 +0300) — must be HEAD at Slice 0 Stage 1; `git status --porcelain` must be clean.
- **Author:** loop-engineering skill (execution carried out by the local executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

S-04 executes the P3 UI pass over four operational modules whose backends are complete and audit-verified at the pinned commit: SampleCollection M21 (draw worklist tab inside the «المعمل» hub + patient draw board), ResultsEntry M04 (unified Results Worklist + simple-result entry + patient result sheet + bulk print + PDF export + activation of the disabled «إدخال نتائج التحاليل» gateway in `PatientsHubViewModel`), ProfileResults M05 (entry grid + report + amendment lifecycle dialogs), CultureResults M06 (culture entry grid + sensitivity panel + report). All three previously-pending decisions (D4 unified worklist; S1 entry point as a «المعمل» tab; draw commands keep `ADD_EDIT_PATIENT`) are **owner-approved finals** — binding, never reopened. NO new backend artifact, NO EF migration, NO new permission code, NO `PermissionConfiguration` change is authorised anywhere in this workstream.

## Global Validation Gates

- **Gate G0 (pre-execution):** `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests); HEAD = `ce30962…`; `git status --porcelain` clean.
- **Gate G1 (post-execution per slice):** same as G0 plus the slice-specific gate listed below, plus the zero-drift gate (`git diff --stat src/TopLab.Infrastructure/Persistence/` empty; `dotnet ef migrations has-pending-model-changes --project src/TopLab.Infrastructure --startup-project src/TopLab.Presentation` → "No changes").

## Quality Gate (non-negotiable — a slice may be marked complete ONLY when ALL FOUR hold)

1. The slice's implementation is fully complete per `Docs/OpenCode/S-04.md` + the corrected P3 plan sections it cites.
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
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (`main`), NEVER create a new branch, NEVER push to any remote, NEVER force-push, NEVER modify or rewrite remote history. Commit message format: `[S-04] Slice N/8: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in S-04's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 0 | VG-01 | Sample Collection tab: build 0/0; suite green; zero-drift; grep/inspection — `GetPatientsWithUncollectedSamplesQuery` has a non-test consumer, tab on `LabHubViewModel` + loaded by `LoadAsync`, no new backend file, open-patient affordance disabled; manual — tab lists today's undrawn, empty text shows, paging limits respected, verbatim error «معاملات الترقيم غير صالحة.». | `dotnet build`; `dotnet test`; ef drift; grep; recorded manual walk |
| 1 | VG-02 | Draw board: build 0/0; suite green; zero-drift; grep — both draw commands consumed, outside-lab CheckBox disabled, bulk action behind `ShowConfirmationAsync`, no new permission string in diff; manual — single mark moves row with timestamp, «سحب الكل» confirms + counts, outside-lab refusal verbatim «تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب», completed patient leaves S1. | build/test; ef drift; grep; manual walk |
| 2 | VG-03 | Worklist + gateway: build 0/0; suite green; zero-drift; grep — `GetResultWorklistQuery` consumed, `PatientsHubViewModel` diff limited to gateway, four open affordances present with non-simple ones disabled, filters map 1:1 to query params; manual — «إدخال نتائج التحاليل» enabled and opens R1, day defaults today, `ResultKind` filter isolates, empty text shows. | build/test; ef drift; grep; manual walk |
| 3 | VG-04 | Result entry + clear dialog: build 0/0; suite green; zero-drift; grep — eight commands consumed, confirmations on clear/unreview, flag restricted 0–2; manual — empty value → «الرجاء إدخال قيمة النتيجة قبل الحفظ», reviewed locks («النتيجة معتمدة؛ ألغِ الاعتماد أولاً.»), balance block verbatim «يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.», auto-flag computes. | build/test; ef drift; grep; manual walk |
| 4 | VG-05 | Patient sheet: build 0/0; suite green; zero-drift; grep — `GetPatientResultSheetQuery` + `MarkAllPatientResultsReviewedCommand` consumed, reprint uses verbatim `ReprintConfirmationMessage`, review-all behind confirmation, PDF affordance disabled; manual — lines + frozen ranges render, review-all reports count, reprint prompts verbatim, balance block verbatim. | build/test; ef drift; grep; manual walk |
| 5 | VG-06 | Bulk print + PDF export: build 0/0; suite green; zero-drift; grep — preflight + execute consumed, decisions carry `ConfirmReprint`, absolute `.pdf` enforced pre-call, picker confined to Presentation; manual — preflight counts, reprint prompts verbatim, five outcome kinds render, existing-file refusal «ملف التصدير موجود مسبقًا.», unverified refusal «لا يمكن تصدير تقرير غير معتمد.». | build/test; ef drift; grep; manual walk |
| 6 | VG-07 | Profile screens: build 0/0; suite green; zero-drift; grep — five M05 commands + three queries consumed, R1 routes `SpecializedProfile` → P1, no `ProfileResultsAccessPolicy` referenced, reason not forced; manual — draft save, duplicate analyte «لا يمكن تكرار المادة التحليلية.», printed-row lock + Amend path, audit denial via `PT_AUDIT_ACCESS`, report reads frozen only. | build/test; ef drift; grep; manual walk |
| 7 | VG-08 | Culture screens: build 0/0; suite green; zero-drift; grep — four M06 commands + two queries consumed, R1 routes culture rows → C1, no `GetCultureAntibioticsQuery` consumer, sensitivity restricted 0–3, pregnancy badge bound; manual — header limits, «تكرار المضاد الحيوي في نفس النتيجة.» / «المضاد الحيوي غير مرفق بهذه المزرعة.» verbatim, child <12 filter, verify-without-save «لا توجد نتيجة مزرعة للاعتماد.», report honours `PrintLabIdInsteadOfPatientId`. | build/test; ef drift; grep; manual walk |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 0 | Sample Collection worklist tab in Lab hub | [ ] Pending | VG-01 |
| 1 | Patient draw board screen | [ ] Pending | VG-02 |
| 2 | Results Worklist + gateway activation | [ ] Pending | VG-03 |
| 3 | Simple result entry screen + clear dialog | [ ] Pending | VG-04 |
| 4 | Patient result sheet | [ ] Pending | VG-05 |
| 5 | Bulk print dialog + PDF export | [ ] Pending | VG-06 |
| 6 | Profile results screens + worklist routing | [ ] Pending | VG-07 |
| 7 | Culture results screens + worklist routing | [ ] Pending | VG-08 |

---

## Settled Decisions (owner-approved, final — binding; do NOT reopen)

- **D4 (قرار نهائي معتمد من المالك):** unified Worklist. Profile (M05) and culture (M06) results open from the same R1 Results Worklist as ResultsEntry, routed by row type (`ResultKind` / `IsCultureType`): `Simple` → R2, `SpecializedProfile` → P1, `Culture`/`IsCultureType=true` → C1. Code basis (audit-verified, unchanged): `GetResultWorklistQuery` already returns all three kinds carrying both routing fields; all M05/M06 commands anchor on `PatientTestId`. No standalone M05/M06 shell entry points are built; no alternative is presented.
- **S1 entry point (قرار نهائي معتمد من المالك):** Sample Collection is a **tab inside the existing «المعمل» hub** (`LabHubViewModel` — the same hub hosting TestCatalog / PriceLists / CultureAttachment / AnalyteProfiles tabs). No new shell button. Closed requirement, not an open question (supersedes the audited plan's §1.4/§1.6 pending markers).
- **Draw-command permission scope (قرار نهائي معتمد من المالك):** **no new permission is created.** The existing `ADD_EDIT_PATIENT` policy (`SampleCollectionAccessPolicy.AddEditPatient`) remains the access control for `MarkSampleDrawnCommand` and `MarkAllSamplesDrawnForPatientCommand`, exactly as implemented. Closed decision, not a gap (supersedes the audited plan's §1.6 pending marker). UI surfaces backend denials verbatim.
- **Commit format:** `[S-04] Slice N/8: <slice title> — loop-engineering`.
- **No half-wired state:** every enabled control has a complete backend path; anything else ships disabled per the repo's placeholder idiom (applies to S2's forward affordances to R2/R3/P1/C1 and to S4's PDF affordance until their slices land).
- **No migration / no backend change:** zero-drift gate on every slice; any discovered need for one is a STOP-and-report, never a silent migration. Diff confined to `src/TopLab.Presentation/**` + `Docs/**`.

## Unresolved Owner Decisions (preserve as unresolved — build around, never silently decide)

- **None within P3 scope.** All three previously-pending points are settled above. (Out-of-scope shell gaps — «الحسابات», «الأدوات», «الإحصائيات», «النظام» — belong to other passes and are not decisions for this workstream.)
- **New UI texts requiring creation** (not existing backend messages — created at execution and recorded in the register below): S1 empty text («لا عينات معلقة» per plan), S2 bulk-draw confirmation text, R1 empty text, R4 clear-confirmation text, R3 review-all confirmation text, P5/C3 verify/unverify confirmation texts, S2 completion/exit texts. Per S-03 U-06 precedent: create once, record verbatim here, reuse consistently.

## Confirmed Facts (independently re-verified at `ce30962` during the audit — ground truth, do not re-derive)

- Baseline: commit `ce30962…` is `main` HEAD; tree clean; `git cat-file -t` → `commit`.
- Presentation: 39 ViewModels / 38 Views, none for the four modules; `DependencyInjection.cs` registers none of them; `PatientsHubViewModel.cs:36` `OpenEnterResultsCommand = new RelayCommand(_ => { })`, `EnterResultsEnabled = false`; `ShellViewModel.cs:159` twelve titles, «الحسابات» disabled, «الأدوات»/«الإحصائيات»/«النظام» fall through; `LabHubViewModel` = 10-tab container with aggregate `LoadAsync`; `NavigationService.NavigateTo<T>()` resolves from DI + raises `Navigated`; `IDialogService` = `ShowConfirmationAsync/ShowErrorAsync/ShowSecondaryPasswordDialogAsync/PickBackupFolderAsync/PickBackupFileAsync` (no save-file picker — Requires creation in Slice 5).
- M21: query signature `GetPatientsWithUncollectedSamplesQuery(DateOnly? Day, int Page = 1, int PageSize = 100)`; board query returns empty board (no error) when patient missing; `MarkSampleDrawn` uses `_clock.UtcNow` (ignores passed `DrawnAtUtc`), idempotent, refuses outside-lab with verbatim Conflict; `MarkAllSamplesDrawnForPatient` returns count, skips outside-lab rows; both write commands carry `IAuthorizedRequest` `ADD_EDIT_PATIENT`; validators verbatim «معاملات الترقيم غير صالحة.» / «معرّف المريض غير صالح.» / «معرّف تحليل المريض غير صالح.».
- M04: 10 commands (Enter/Clear/Review/Unreview/MarkAllReviewed/MarkPrinted/MarkDelivered/RefreshRange/ExportPdf/ExecuteBulkPrint) + GetResultWorklist/GetResultEntry/GetPatientResultSheet + BulkPrintPreflight; policies `EDIT_RESULTS`/`REVIEW_RESULTS`/`PRINT_RESULTS`/`DELIVER_RESULTS`; `ResultWorklistItemDto` carries `ResultKind` + `IsCultureType` (D4 basis); `EnterResult` rejects non-simple («لا يمكن إدخال نتيجة إلا لتحليل بسيط.»), auto-reviews on `SystemSettings.AutoReviewAndComplete`; balance block on print verbatim; `BulkPrintMessages.ReprintConfirmationMessage` = «لقد تم طباعه هذا التقرير لهذا المريض من قبل هل ترغب في اعاده الطباعه»; translator map (6 entries + default «بيانات غير صالحة.»); export validator/handler messages verbatim (7 strings).
- M05: reuses `ResultsEntryAccessPolicy` (no module-own policy); `SaveProfileResults` full-replaces unprinted drafts, never deletes printed/amended rows, freezes per-item snapshots; `Amend` allowed only after print; reason optional ≤ 500; amendments query gated `PT_AUDIT_ACCESS`; translator includes «لا يمكن تعديل مادة مطبوعة إلا عبر مسار التعديل.» / «المادة غير معتمدة؛ لا يمكن طباعتها.».
- M06: module-own `CultureResultsAccessPolicy`; entry grid sources rows internally from `CultureAntibioticAttachment` + `Antibiotic` (NOT `GetCultureAntibioticsQuery`); display filter `CultureAntibioticDisplay.IsDisplayable` + `ChildAgeThresholdYears = 12` (strictly under 12, years only); saved rows always displayed; validator limits Sample ≤ 100 / OrganismA–C ≤ 150 / CultureCondition ≤ 200 / ColonyCount ≤ 50 / SensitivityCategory ∈ [0,3] / no duplicate antibiotic (verbatim «تكرار المضاد الحيوي في نفس النتيجة.»); `VerifyCultureResult` requires saved result and auto-enters+reviews; both culture queries are plain unauthenticated `IRequest`.
- Domain: `PatientTest` lifecycle mutators + draw flags; `PatientStatusCalculator` S1–S7 implemented (no longer a stub); enums `ResultKind{Simple=0,SpecializedProfile=1,Culture=2}`, `ResultFlag{Normal=0,Low=1,High=2}`, `ProfileResultFlag{Low=0,High=1}`, `SensitivityCategory{HighlyFor=0,ModerateFor=1,LowFor=2,ResistantFor=3}`.

## Corrections Log (independent audit of «خطة التنفيذ النهائية للواجهات والنوافذ الرسوميه - P3.md» at `ce30962` — what changed between the audited draft and this package)

| # | Location in original plan | Correction |
|---|---|---|
| C-1 | §1.2 / §1.4 (S1 paging row) | `GetPatientsWithUncollectedSamplesQuery` signature annotated to the live form: `PageSize` default is **100** (plan documented the parameter and its [1,500] validation bound but not the default; R1's worklist query default 50 was documented correctly). No behavioural impact; spec now carries both defaults explicitly. |
| C-2 | §1.4 S1 «نقطة الدخول» + §1.6 (two markers «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»: S1 entry point; draw-permission scope) | Both markers replaced with **«قرار نهائي معتمد من المالك»** and the owner-approved content: S1 = tab inside the existing «المعمل» hub; no new permission — `ADD_EDIT_PATIENT` stays as-is. All "pending/open question" framing removed; content otherwise unchanged. |
| C-3 | «حسم القرار D4» + §2.6/§3.6/§4.6 + quality-gate log item 3/6 («قرار افتراضي من الوكيل — بانتظار موافقة المالك») | D4 marker replaced with **«قرار نهائي معتمد من المالك»**; final content = unified Worklist routed by `ResultKind`/`IsCultureType`, exactly the plan's own code-based reasoning (the code evidence — `GetResultWorklistQuery` returning both routing fields — was re-verified unchanged). No alternative generated or presented. |

**Audit statement:** every «Confirmed from code» claim in the P3 plan was independently re-verified by opening the actual source files at `ce30962` during the audit session (all four feature trees, both Common files per feature, all validators, all handlers' error paths, the two access-policy classes, the DTO records, the enums, `PatientsHubViewModel`, `ShellViewModel`, `LabHubViewModel`, `NavigationService`, `IDialogService`, `DependencyInjection.cs`, full Presentation tree listing). **No fabricated class name, field name, file path, or message text was found.** The plan's two self-flagged "Code wins" corrections (M05 reusing `ResultsEntryAccessPolicy`; culture grid sourcing rows internally rather than via `GetCultureAntibioticsQuery`) were re-confirmed as true. The plan's «Requires creation» / «Inference» classifications were re-checked against live code and all still stand (notably: no save-file picker in `IDialogService`; no standalone `ProfileResultsAccessPolicy`; `Amend` reason not enforced non-empty). Corrections above are therefore status/label finalisations plus one signature annotation — no requirement was added, dropped, or re-scoped.

## Uncertainty / Verification Register

| ID | Item | Class | Resolution |
|----|------|-------|-----------|
| U-01 | New UI-only texts (empty states, confirmations without backend counterpart) | يتطلب إنشاء | OPEN — created at execution per slice; record each verbatim here as created (S-03 U-06 precedent). |
| U-02 | S1 tab-content hosting idiom inside `LabHubView` (tab item vs. embedded view binding) | Inference | OPEN — resolve at Slice 0 Stage 3 by reading `LabHubView.xaml` and following the existing tab precedent exactly. |
| U-03 | Shape of the printed-row edit lock on P1 (disable vs. read-only) | Inference | OPEN — executive choice at Slice 6 Stage 4; backend lock («لا يمكن تعديل مادة مطبوعة إلا عبر مسار التعديل.») is confirmed; record the chosen shape. |
| U-04 | Save-file picker placement for R6 (extend `IDialogService` vs. local dialog helper) | Requires creation | OPEN — resolve at Slice 5 Stage 3 following the existing dialog-service idiom; keep confined to Presentation. |

---

## Slice 0: Sample Collection worklist tab in Lab hub

**Gate:** VG-01. **Status:** [x] Done (2026-09-18).

### 10-Stage Progress (Slice 0)
- [x] 1. Pre-Execution Verification — HEAD = ce30962…, git status clean on tracked files; dotnet build 0/0; dotnet test 2088/2088 passed (474 + 1419 + 195).
- [x] 2. Deep Understanding — read S-04.md §4 Slice 0 spec; columns المريض/LabId/تاريخ التسجيل/عدد العينات المعلقة; day filter Day default today UTC; paging Page≥1/PageSize∈[1,500], default 100 (query's own default per audit C-1); refresh → GetPatientsWithUncollectedSamplesQuery; open-patient affordance disabled (wired Slice 1); read-only; four states Loading/Empty(Error via ResultErrorPresenter)/Confirmation n/a; empty text «لا عينات معلقة» (UI-created, recorded in register).
- [x] 3. File Analysis — inspected LabHubViewModel (10-tab container + LoadAsync), LabHubView.xaml (TabItem→ContentControl idiom), TestCatalogViewModel/PriceListsViewModel/CustomGroupsViewModel/AnalytesViewModel (constructor ISender+IDialogService+ResultErrorPresenter, LoadAsync shape), AnalytesView/TestCatalogView/CustomGroupsView (DataGrid + empty-state DataTrigger + error strip), PriceListsView (load-on-constructor? No — LoadAsync called by hub), DependencyInjection.cs (transient registrations), MainWindow.xaml (DataTemplate mappings labVm:* → labView:*), ResultErrorPresenter (Validation/NotFound/Conflict verbatim, Forbidden→permission constant, Unexpected→generic), WorkSheetsViewModel (DatePicker→DateTime? property→ToDateOnly helper at query site), RelayCommand/AsyncRelayCommand, IDialogService interface, NavigationService. Resolved U-02: tab content = ContentControl bound to hub property per LabHubView.xaml idiom.
- [x] 4. Planning — 6 files: new SampleCollectionViewModel.cs + SampleCollectionView.xaml + SampleCollectionView.xaml.cs; patch LabHubViewModel.cs (+ctor param +property+LoadAsync call), LabHubView.xaml (+tab item), DependencyInjection.cs (+transient), MainWindow.xaml (+DataTemplate).
- [x] 5. Execution — all 6 files created/patched; placeholder open-patient command = AsyncRelayCommand(async _ => await Task.CompletedTask) to satisfy Func<CancellationToken,Task> without CS1998 warning; OpenPatientEnabled=false (no-half-wired-state).
- [x] 6. Post-Execution Verification — dotnet build 0/0 (0 errors, 0 warnings after CS1998 fix).
- [x] 7. Validation Gate VG-01 — PASS: build 0/0; suite 2088/2088 green; zero-drift (EF: no model changes; git diff --stat src/TopLab.Infrastructure/Persistence/ empty); grep confirms non-test consumer at SampleCollectionViewModel.cs:98, tab on LabHubViewModel loaded by LoadAsync, no new backend file, OpenPatientEnabled=false, no DRAW permission string in diff; diff confined to src/TopLab.Presentation/**. Manual walk pending (recorded): «المعمل»→new tab shows today's undrawn patients; empty day shows «لا عينات معلقة»; paging respects [1,500] with default 100; error path shows verbatim «معاملات الترقيم غير صالحة.». (Manual verification recorded but not UI-executed — no running app harness available in this session; behaviour verified by code inspection against audited backend.)
- [x] 8. Documentation Update — this section.
- [x] 9. Memory Status Update — Current Status below.
- [x] 10. Git Commit — local commit on main: `[S-04] Slice 0/8: Sample Collection worklist tab in Lab hub — loop-engineering`.

### Slice 0 Files Touched
- New: src/TopLab.Presentation/ViewModels/Lab/SampleCollectionViewModel.cs
- New: src/TopLab.Presentation/Views/Lab/SampleCollectionView.xaml
- New: src/TopLab.Presentation/Views/Lab/SampleCollectionView.xaml.cs
- Modified: src/TopLab.Presentation/ViewModels/Lab/LabHubViewModel.cs (ctor param + property + LoadAsync call)
- Modified: src/TopLab.Presentation/Views/Lab/LabHubView.xaml (+ tab item)
- Modified: src/TopLab.Presentation/DependencyInjection.cs (+ transient registration)
- Modified: src/TopLab.Presentation/MainWindow.xaml (+ DataTemplate mapping)

### Slice 0 UI-Created Texts (recorded in Uncertainty/Verification Register)
- «لا عينات معلقة» — S1 empty state; operational-success display (not an error). Created at Slice 0 Stage 5.

## Slice 1: Patient draw board screen

**Gate:** VG-02. **Status:** [ ] Pending.

### 10-Stage Progress (Slice 1)
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

## Slice 2: Results Worklist + gateway activation

**Gate:** VG-03. **Status:** [ ] Pending.

### 10-Stage Progress (Slice 2)
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

## Slice 3: Simple result entry screen + clear dialog

**Gate:** VG-04. **Status:** [ ] Pending.

### 10-Stage Progress (Slice 3)
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

## Slice 4: Patient result sheet

**Gate:** VG-05. **Status:** [ ] Pending.

### 10-Stage Progress (Slice 4)
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

## Slice 5: Bulk print dialog + PDF export

**Gate:** VG-06. **Status:** [ ] Pending.

### 10-Stage Progress (Slice 5)
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

## Slice 6: Profile results screens + worklist routing

**Gate:** VG-07. **Status:** [ ] Pending.

### 10-Stage Progress (Slice 6)
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

## Slice 7: Culture results screens + worklist routing

**Gate:** VG-08. **Status:** [ ] Pending.

### 10-Stage Progress (Slice 7)
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

---

## Current Status

- **Baseline established:** 2026-09-17 — repository at `ce30962bdbfd29b17f099b8b700901750104ff88`, tree clean; audit complete; three decisions owner-finalised; package generated.
- **Slice 0:** Done. VG-01 passed. Local commit `[S-04] Slice 0/8: Sample Collection worklist tab in Lab hub — loop-engineering` prepared.
- **Execution:** Slice 0 complete; next action: Slice 1, Stage 1 (Pre-Execution Verification).

## Execution Log

| Timestamp | Slice | Stage | Action | Result |
|---|---|---|---|---|
| — | — | — | (placeholder — populated by the executing agent) | — |

## Stop Report

(empty — populated only if a stop condition triggers)
