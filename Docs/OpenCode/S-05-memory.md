# Loop Engineering — Memory File

- **Module:** P4 UI Pass — PatientSearch (M08), ReportProduction (M07), ResultDelivery (M09), SentOutSamples (M16) (S-05 — cross-module UI workstream, post-S-04)
- **Module Number:** S-05
- **Source Plan:** Docs/OpenCode/S-05.md (execution slices) + «Fourth Pass Plan.md» (authoritative requirements, as corrected by the audit recorded below)
- **Date Created:** 2026-09-18
- **Total Slices:** 6
- **Current Slice:** 2 — Combined report + insert-history dialog — NEXT
- **Current Branch:** main
- **Baseline Commit:** `0e66b014d5f3889e0ec11e15398f00ce967e196d`
- **Latest Committed:** Slice 0 (`9b1bdfe`); Slice 1 pending commit
- **Author:** loop-engineering skill (local executing agent per owner authorization)

---

## Module Summary

Ship the complete WPF Presentation layer for M08 (patient global search + visit history master-detail), M07 (combined/blank/history reports + insert-history dialog), M09 (undelivered list + atomic deliver-with-settlement handover), and M16 (sent-out samples list + send dialog + external-lab account). Close the two remaining `PatientsHubViewModel` navigation debts (M08/M09 gateways). D10 (external-lab list source for M16) is closed by delegated authority — see "Settled Decisions" below. Zero backend changes; zero migrations; zero permission changes.

## Global Validation Gates

- Build: `dotnet build` → 0 errors / 0 warnings (0/0).
- Tests: full suite green (no UI harness exists; UI behaviour is verified manually and recorded per slice).
- Zero-drift: `git diff --stat src/TopLab.Domain/ src/TopLab.Application/ src/TopLab.Infrastructure/` empty; `dotnet ef migrations has-pending-model-changes` → "No changes".
- Verbatim-messages: every Arabic string with a backend counterpart matches the backend file byte-for-byte; UI-only strings are recorded in the Created UI Texts register below.
- No-half-wired-state: every open affordance whose target screen is not yet built ships disabled until its slice lands.

## Stop/Continue Rule

Continue automatically between slices while: build stays 0/0, suite stays green, zero-drift holds, and every spec element maps to a verified backend surface. **STOP and report** when: (a) a spec element requires a new/changed backend type, command, query, DTO, validator, permission, or migration; (b) a live backend message contradicts the frozen message table; (c) Slice 5 reaches Stage 4 without an owner decision on the M16 temporary-entry placement (the one recorded open decision below); (d) any manual verification fails irreproducibly. On STOP: record the trigger in the Stop Report section and wait.

## Confirmed Facts (verbatim-verified at `0e66b01` during the audit — do not re-derive)

**Gateways (PatientsHubViewModel.cs):** `OpenSearchPatientCommand = new RelayCommand(_ => { })`, `OpenDeliverResultsCommand = new RelayCommand(_ => { })`; `_searchPatientEnabled`/`_deliverResultsEnabled` never set true. M04 gateway already enabled → `ResultsWorklistViewModel`. Buttons: «بحث عن مريض» `PatientsHubView.xaml` lines 14–15; «تسليم نتائج المرضى» lines 16–17.

**Shell (src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs):** 12 titles; `IsEnabled = t != "الحسابات"` (line 171); «الأدوات»/«الإحصائيات»/«النظام» fall through `// Future: navigate to feature` (line 234); D3 comment lines 167–168 (ExternalEntities official entry under «الحسابات» disabled until P5; temporary tagged route via `SettingsDashboardViewModel.OpenExternalEntitiesAsync()`).

**M08 frozen messages:** «معاملات الترقيم غير صالحة.» (Page ≥ 1; PageSize ∈ [1,500]) / «نص البحث يجب ألا يتجاوز 200 حرفًا.» / «كود المعمل مطلوب.» / «كود المعمل يجب ألا يتجاوز 30 حرفًا.» / «لا يوجد مريض بهذا الكود.» / «المريض غير موجود.» / «معرف المريض غير صالح.» / «سجل إعدادات التقرير مفقود.»

**M07 frozen messages:** «قائمة المرضى مطلوبة.» / «قائمة التحاليل مطلوبة.» / «معرّف المريض غير صالح.» / «يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.» / «لا يمكن طباعة نتيجة غير معتمدة.» / «لا يمكن إدراج نتيجة غير معتمدة في التقرير.» / «لا يمكن تكرار نفس التحليل في التقرير.» / «تعذر تحديد هوية المريض للتاريخ المرضي.» / «المريض غير موجود.» / «التحليل غير موجود». Permission: `ReportProductionAccessPolicy.PrintResults = "PRINT_RESULTS"`. Blank report: no balance gate, no MarkPrinted (OD-07-E). Print token: `ReportPrintEnvelope.CreateToken(kind, report)`, kinds `Combined`/`Blank`/`History`, via `IReportPrintingService.PrintReportAsync`.

**M09 frozen messages:** «بداية الفترة يجب ألا تتجاوز نهايتها.» / «معاملات الترقيم غير صالحة.» / «معرّف المريض غير صالح.» / «المريض غير موجود.» / «حدد نتيجة واحدة على الأقل للتسليم.» / «مبلغ التسوية غير صالح.» / «لا يوجد رصيد مستحق للتسوية.» / «النتيجة غير مطبوعة.» (from Domain «Result not printed.», PatientTest.cs line 195). Permission: `ResultDeliveryAccessPolicy.DeliverResults = "DELIVER_RESULTS"` (seeded id=6). Account delegation: `PatientBillingReader.ReadAccount` (verified by direct read of `GetDeliveryAccountQueryHandler` line 32). `RemainingToLab = Balance>0 ? Balance : 0`; `RemainingToPatient = Balance<0 ? -Balance : 0` (OD-09-C). Atomicity: single `SaveChanges`.

**M16 frozen messages:** «التحليل غير موجود» / «الجهة الخارجية غير موجودة.» / «السعر يجب ألا يكون سالبًا.» / «هذا التحليل غير مهيأ للإرسال للخارج.» / «الجهة المختارة ليست معملًا خارجيًا.» / «تم إرسال هذه العينة مسبقًا.» / «العينة المُرسَلة غير موجودة.» / «مبلغ الدفع يجب أن يكون أكبر من صفر.» / «مبلغ الدفع يتجاوز المتبقي.» / «لا يوجد رصيد مستحق للتسوية.» / «بداية الفترة يجب ألا تتجاوز نهايتها.» / «معاملات الترقيم غير صالحة.» (PageSize ∈ [1,100]). Permission: `SentOutSamplesAccessPolicy.CashDisburseDeposit = "CASH_DISBURSE_DEPOSIT"` (seeded id=11). Defaults: `CostPrice ?? test.SentOutCostPrice ?? 0m`; `PatientPrice ?? test.PatientPrice`.

**Cross-cutting:** `ResultErrorPresenter` — Forbidden → «أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام»; Unexpected → «حدث خطأ غير متوقع. حاول مرة أخرى.». Navigation idiom: `_navigation.NavigateTo<XViewModel>(); if (_navigation.CurrentViewModel is XViewModel vm) { await vm.LoadAsync(...); }`. `IDialogService.ShowConfirmationAsync(title, message)` for all confirmations.

## Settled Decisions

- **D10 — مصدر قائمة المعامل الخارجية (M16):** **«قرار نهائي بتفويض من المالك — اتخذه الوكيل بناءً على تحليل الكود»**. المصدر: `SearchExternalEntitiesQuery(EntityType: EntityType.PartnerLab, SearchTerm: null, Page: 1, PageSize: 100)` → `ExternalEntityListItemDto(Id, Name)`؛ لا استعلام جديد. الأساس (مسار a — الكود الحي، حاسم): (1) `EntityType.PartnerLab = 2` قيمة صريحة في `Domain/Common/Enums/EntityType.cs`؛ (2) `SendSampleOutCommandHandler` يفرض `EntityType.PartnerLab` ويرفض غيره بـ «الجهة المختارة ليست معملًا خارجيًا.»؛ (3) `SearchExternalEntitiesQueryHandler` يرشّح بالنوع فعلياً والمنتقي القائم `ExternalEntityPickerViewModel` يتضمن `PartnerLab` في `TypeFilters`. توافق مؤيد (b): «إبتدائي» حسم أن المعمل الخارجي جهة من M14 (التعارض 4)، وفلتر RLS بأعمدة Lab Name (RLS_Learn ص184–188) متسق. البديل المعتمد للحوار: إعادة استخدام `ExternalEntityPickerViewModel` مع قفل الفلتر على `PartnerLab`.
- **Recorded open decision (NOT settled — do not decide):** الموضع الدقيق لزر الدخول المؤقت لشاشة العينات المرسلة داخل لوحة الإعدادات حتى P5: **«بانتظار قرار المالك — غير مُدرج في القائمة الأصلية»**. الآلية (مسار مؤقت موسوم بنمط D3) محسومة؛ الموضع الدقيق ليس كذلك. Slice 5 Stage 4 stop-gate applies.

## Slice Validation Gates (from plan)

- **VG-01 (Slice 0):** build 0/0; suite green; zero-drift; search + lab-id queries consumed; M08 gateway enabled and wired; open-patient affordance disabled; manual scenarios recorded.
- **VG-02 (Slice 1):** build 0/0; suite green; zero-drift; visit-history + visit-detail queries consumed; M03/M04 cross-buttons wired to existing VMs; M07/M09/M16 buttons disabled; manual scenarios recorded.
- **VG-03 (Slice 2):** build 0/0; suite green; zero-drift; all six M07 combined-report/insert surfaces consumed; insertions behind confirmation; «التقارير» enabled; no re-declared permission code; manual scenarios recorded.
- **VG-04 (Slice 3):** build 0/0; suite green; zero-drift; blank + three history modes consumed; blank print has no balance interference; manual scenarios recorded.
- **VG-05 (Slice 4):** build 0/0; suite green; zero-drift; three queries + atomic command consumed; M09 gateway enabled; deliver CheckBox gated on printed-and-undelivered; single atomic confirmation; no UI-side account math; manual scenarios recorded.
- **VG-06 (Slice 5):** build 0/0; suite green; zero-drift; all M16 commands/queries consumed; D10 grep gate (both ComboBoxes ← `SearchExternalEntitiesQuery` + `PartnerLab`; no new backend query); temporary entry tagged; «إرسال خارجياً» enabled; manual scenarios recorded.

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 0 | Patient search screen + gateway activation | [x] Done | VG-01 PASS |
| 1 | Patient visit history master-detail screen | [x] Done | VG-02 PASS |
| 2 | Combined report screen + insert-history dialog | [x] Done | VG-03 PASS |
| 3 | Blank report + history reports screens | [x] Done | VG-04 PASS |
| 4 | Result delivery screens + gateway activation | [ ] Not started | VG-05 |
| 5 | Sent-out samples screens + temporary entry point | [ ] Not started | VG-06 |

## Slice 0: Patient search screen + gateway activation

**Gate:** VG-01. **Status:** ✅ Passed.

### 10-Stage Progress
- [x] 1. Pre-Execution Verification — build 0/0; App tests 1419/1419; HEAD pinned
- [x] 2. Deep Understanding — S-05.md §4 Slice 0 spec; PatientSearchDtos; SearchPatientsGlobalQuery/GetPatientByLabIdQuery signatures
- [x] 3. File Analysis — PatientsHubViewModel, PatientsHubView.xaml, ResultsWorklistViewModel pattern, DI, MainWindow.xaml
- [x] 4. Planning — PatientSearchViewModel + View; enable gateway in PatientsHubViewModel; DI + DataTemplate
- [x] 5. Execution — Created PatientSearchViewModel/View; enabled SearchPatientEnabled + wired OpenSearchPatientCommand; registered in DI; added DataTemplate
- [x] 6. Post-Execution Verification — Presentation build 0/0; full solution 0/0; App tests 1419/1419; zero backend diff
- [x] 7. Validation Gate — VG-01 PASS: both M08 queries consumed; gateway enabled+wired; open-patient button disabled; no new backend file
- [x] 8. Documentation Update — memory checklist recorded
- [x] 9. Memory Status Update — Slice 0 done, Slice 1 next
- [x] 10. Git Commit — local commit

## Slice 1: Patient visit history master-detail screen

**Gate:** VG-02. **Status:** ✅ Passed.

### 10-Stage Progress
- [x] 1. Pre-Execution Verification — build 0/0; App tests 1419/1419
- [x] 2. Deep Understanding — VisitHistoryDto/VisitSummaryDto/VisitDetailDto shapes; GetVisitHistoryQuery/GetVisitDetailQuery signatures
- [x] 3. File Analysis — PatientAccountViewModel.LoadAsync(int), PatientResultSheetViewModel.LoadAsync(int), PatientsHub pattern
- [x] 4. Planning — PatientVisitHistoryViewModel + View; enable open-patient + fetch-by-lab-id from S0; cross-module buttons
- [x] 5. Execution — Created master-detail VM/View; enabled open-patient + lab-id navigation; wired account/result-sheet buttons; disabled reports/delivery/sent-out
- [x] 6. Post-Execution Verification — build 0/0; tests 1419/1419; zero backend diff
- [x] 7. Validation Gate — VG-02 PASS: both M08 queries consumed; account+result-sheet wired; reports/delivery/sent-out disabled
- [x] 8. Documentation Update — memory checklist recorded
- [x] 9. Memory Status Update — Slice 1 done, Slice 2 next
- [x] 10. Git Commit — local commit

## Slice 2: Combined report screen + insert-history dialog

**Gate:** VG-03. **Status:** ⬜ Not started.

### 10-Stage Progress
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

## Slice 3: Blank report + history reports screens

**Gate:** VG-04. **Status:** ⬜ Not started.

### 10-Stage Progress
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

## Slice 4: Result delivery screens + gateway activation

**Gate:** VG-05. **Status:** ⬜ Not started.

### 10-Stage Progress
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

## Slice 5: Sent-out samples screens + temporary entry point

**Gate:** VG-06. **Status:** ⬜ Not started.

### 10-Stage Progress
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning — **STOP-GATE:** owner decision on the temporary-entry placement («بانتظار قرار المالك — غير مُدرج في القائمة الأصلية») must exist before proceeding
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

## Created UI Texts Register (append at execution time)

| Slice | Screen | Text (Arabic, as shipped) | Kind |
|---|---|---|---|
| 0 | PatientSearchView | «لا توجد نتائج مطابقة.» | Empty-state |
| 0 | PatientSearchView | «كود المعمل مطلوب.» | Client-side validation (mirrors backend) |
| 1 | PatientVisitHistoryView | «لا توجد تحاليل في هذه الزيارة.» | Empty-state |
| 2 | CombinedReportView | «لا توجد تحاليل معتمدة قابلة للدمج لهذا المريض.» | Empty-state |
| 2 | InsertHistoryDialogWindow | «لا نتائج تاريخية لهذا التحليل.» | Empty-state |
| 2 | CombinedReportViewModel | «سيتم إدراج نتائج تاريخية تلقائياً في التقرير. هل تريد المتابعة؟» | Confirmation (auto-insert) |
| 2 | InsertHistoryDialogViewModel | «هل تريد إدراج النتيجة التاريخية للتحليل «{TestName}»؟» | Confirmation (manual insert) |
| 3 | BlankReportView | «لم يُبنَ تقرير بعد» | Empty-state |
| 3 | HistoryReportsView | «لا يوجد تاريخ مرضي لهذا المريض.» | Empty-state |
| 3 | HistoryReportsViewModel | «سيتم طباعة التقرير التاريخي وتعليم النتائج كمطبوعة. هل تريد المتابعة؟» | Confirmation (history print) |

Expected entries (from the audited plan — all Requires-creation view texts): «لا توجد نتائج مطابقة.» (S0) / «لا توجد تحاليل في هذه الزيارة.» (S1) / «لا توجد تحاليل معتمدة قابلة للدمج لهذا المريض.» + «لا نتائج تاريخية لهذا التحليل.» + insert/auto-insert confirmation texts (S2) / «لم يُبنَ تقرير بعد» + «لا يوجد تاريخ مرضي لهذا المريض.» + history-print confirmation (S3) / «لا توجد نتائج غير مسلَّمة في هذه الفترة.» + «لا توجد تحاليل لهذا المريض.» + deliver-and-settle confirmation (S4) / «لا توجد عينات مرسلة في هذه الفترة/لهذا المعمل.» + «لا توجد معامل خارجية مسجلة — أضف جهة بنوع «معمل خارجي» من إدارة الجهات.» + «لا عينات مرسلة لهذا المعمل في الفترة.» + send/payment/settle confirmations (S5).

## Current Status
- **Slice 0:** Done. VG-01 passed.
- **Slice 1:** Done. VG-02 passed.
- **Slice 2:** Done. VG-03 passed.
- **Slice 3:** Done. VG-04 passed.
- **Slice 4:** Not started. Result delivery + gateway.
- **Slice 5:** Not started.

---

## Corrections Log (independent audit of «Fourth Pass Plan.md» at `0e66b014d5f3889e0ec11e15398f00ce967e196d`)

Audit method: fresh clone; `git checkout 0e66b01` (detached HEAD, clean tree); `git rev-parse origin/HEAD` = same commit (still default-branch HEAD); every "Confirmed from code" claim re-verified by opening the actual file. The plan's self-reported quality-gate log (its §7) was treated as a claim, not evidence.

**Corrections made (3):**

1. **C1 — Path precision:** the plan cites `ShellViewModel.cs` without a directory in §0.1 (items 2–3) and §7. The actual file is `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` (there is no `ViewModels/ShellViewModel.cs`). Fully qualified everywhere in this package. No other content change — the cited facts (12 titles, «الحسابات» disabled at line 171, `// Future: navigate to feature` at line 234, D3 comment at lines 167–168) all verified true at the qualified path.
2. **C2 — D10 status in the plan's §4.6:** the marker «قرار افتراضي من الوكيل — بانتظار موافقة المالك» replaced with the closed decision under the owner-mandated label **«قرار نهائي بتفويض من المالك — اتخذه الوكيل بناءً على تحليل الكود»** (see "Settled Decisions" above). The delegation covers D10 only; it is NOT labelled «معتمد من المالك» because the owner did not personally review this specific content.
3. **C3 — D10 status paragraph in the plan's §6:** the sentence declaring the decision «ليس نهائياً ولا مؤكداً» and the pending framing replaced with the final closed text; the three-part code rationale (PartnerLab enum value, handler enforcement, query type-filter + existing picker) re-verified verbatim and retained unchanged. The decision is now embedded as closed throughout the SentOutSamples specification (filter ComboBox + send-dialog ComboBox both sourced from `SearchExternalEntitiesQuery` with `EntityType.PartnerLab`; grep-gated in VG-06).

**Verified without correction (spot-audit held):** all four modules' command/query/validator/DTO names and signatures; every frozen Arabic message listed in "Confirmed Facts" above (byte-for-byte against the validators/handlers/translators); both disabled gateways and their XAML line references; the M04 gateway's enabled state; the D3 entry mechanism and its code comment; the complete absence of P4 Views/ViewModels/DI registrations/DataTemplates; permission codes `PRINT_RESULTS` / `DELIVER_RESULTS` (id=6) / `CASH_DISBURSE_DEPOSIT` (id=11); blank-report's no-balance-gate rule; delivery atomicity; `MarkDelivered`'s print guard (PatientTest.cs:195); M16 price defaults.

**Preserved as open (not decided):** the plan's own newly-discovered decision — exact placement of the M16 temporary entry inside the settings dashboard — remains «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية». The delegation was scoped to D10 only; no other ambiguity was resolved by the auditor.

---

## Stop Report (append only if a stop condition triggers)

(Empty — no stop condition triggered)

---

## Execution Log

| Timestamp | Slice | Stage | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-09-18 | 0 | 1-10 | PatientSearchViewModel + View; PatientsHub gateway activation; DI + DataTemplate; VG-01 PASS | Success |
| 2026-09-18 | 1 | 1-10 | PatientVisitHistoryViewModel + View; open-patient + lab-id navigation enabled; cross-module buttons; VG-02 PASS | Success |
| 2026-09-18 | 2 | 1-10 | CombinedReportViewModel + View + InsertHistoryDialog; «التقارير» enabled; VG-03 PASS | Success |
| 2026-09-18 | 3 | 1-10 | BlankReportViewModel + View + HistoryReportsViewModel + View (3 modes); VG-04 PASS | Success |
