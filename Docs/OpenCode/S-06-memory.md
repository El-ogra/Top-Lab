# Loop Engineering — Memory File

- **Module:** P5 UI Pass — AuditAndTraceability (M10), Attendance (M18), Statistics (M19), InventoryAndAccounting (M20), Utilities (M23) (S-06 — cross-module UI workstream, post-S-05)
- **Module Number:** S-06
- **Source Plan:** Docs/OpenCode/S-06.md (execution slices) + «Fifth Pass Plan.md» (authoritative requirements, as corrected by the audit recorded below)
- **Date Created:** 2026-09-18
- **Total Slices:** 7
- **Current Slice:** 1 — Attendance admin screens + entry point — NEXT
- **Current Branch:** main
- **Baseline Commit:** `c34b9ad35022684f0b65ab2cfe4e6b84c511d59f`
- **Latest Committed:** Slice 0 pending commit
- **Author:** loop-engineering skill (local executing agent per owner authorization)

---

## Module Summary

Ship the complete WPF Presentation layer for M10 (patient/test audit, P/T views), M18 (self-service attendance + admin records/summary), M19 (four-section statistics dashboard, tables-only per D5), M20 (accounts hub: cash drawer + cash movements dialog + element inventory + patient samples + company/delegate accounts), and M23 (six-tab utilities). Settle the four shell-navigation debts: wire «الأدوات»/«الإحصائيات»/«النظام» and activate «الحسابات», absorbing the two temporary settings-dashboard routes and closing the D3/D10 comments. D5, D11, D12 are closed by delegated authority — see "Settled Decisions" below. Zero backend changes; zero migrations; zero permission changes.

## Global Validation Gates

- Build: `dotnet build` → 0 errors / 0 warnings (0/0).
- Tests: full suite green (no UI harness exists; UI behaviour is verified manually and recorded per slice).
- Zero-drift: `git diff --stat src/TopLab.Domain/ src/TopLab.Application/ src/TopLab.Infrastructure/` empty; `dotnet ef migrations has-pending-model-changes` → "No changes".
- Verbatim-messages: every Arabic string with a backend counterpart matches the backend file byte-for-byte; UI-only strings are recorded in the Created UI Texts register below.
- No-half-wired-state: every open affordance whose target screen is not yet built ships disabled until its slice lands; the M14/M16 temporary routes move atomically in Slice 4 (never removed before the hub routes are live).
- No new package reference anywhere (D5); `git diff` on `Directory.Packages.props` and any `*.csproj` stays empty for the whole workstream.

## Stop/Continue Rule

Continue automatically between slices while: build stays 0/0, suite stays green, zero-drift holds, and every spec element maps to a verified backend surface. **STOP and report** when: (a) a spec element requires a new/changed backend type, command, query, DTO, validator, permission, or migration; (b) a live backend message contradicts the frozen message tables below; (c) Slice 1 reaches Stage 4 without an owner decision on the attendance entry point (the one recorded open decision below); (d) any manual verification fails irreproducibly. On STOP: record the trigger in the Stop Report section and wait.

## Confirmed Facts (verbatim-verified at `c34b9ad` during the audit — do not re-derive)

**Commit:** `git log -1` → `c34b9ad35022684f0b65ab2cfe4e6b84c511d59f` «[S-05] Slice 5/6: Sent-out samples screens + temporary entry point — loop-engineering» 2026-09-18 20:01:39 +0300; clean tree.

**Shell (`src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs`, read in full):** 12 titles `["المرضى", "المعمل", "ورقة العمل", "الأدوات", "الحسابات", "الإحصائيات", "المستخدمون", "النظام", "الإعدادات", "حول البرنامج", "قفل المحطة", "خروج"]`; `IsEnabled = t != "الحسابات"`; D3 comment above it; «الأدوات»/«الإحصائيات»/«النظام» fall through `else { // Future: navigate to feature }`; «المستخدمون» gated by `ShowSecondaryPasswordDialogAsync()`; exit confirmation `ShowConfirmationAsync("خروج", "هل تريد إنهاء البرنامج؟")`.

**Settings dashboard (read in full):** `SettingsDashboardView.xaml` carries the two temporary buttons «الجهات الخارجية (طريق مؤقت — لحين تفعيل الحسابات)» and «العينات المرسلة (طريق مؤقت — لحين تفعيل الحسابات)»; `SettingsDashboardViewModel` carries `OpenExternalEntitiesAsync` (D3 comment) and `OpenSentOutSamplesAsync` (D10 placement comment) plus `OpenDatabaseMaintenanceAsync` behind the secondary password with error «لم يتم التحقق من كلمة المرور الثانوية. لا يمكن فتح صيانة قاعدة البيانات.».

**Patients hub (read):** `SearchPatientEnabled = true;` / `DeliverResultsEnabled = true;` with both commands wired — S-05 debts settled; no P5-relevant debt remains there.

**Permissions (`PermissionConfiguration.cs`):** id=11 `CASH_DISBURSE_DEPOSIT`; id=12 `STATISTICS`; id=13 `PT_AUDIT_ACCESS`.

**Packages:** `Directory.Packages.props` — MediatR 12.5.0, ZXing.Net 0.16.11, QuestPDF 2026.9.0, FluentValidation 12.1.1, EF Core 8.0.30 family, Microsoft.Extensions.* 8.0.1, test stack; **no charting library anywhere** (full sweep over csproj/props/xaml/cs = 0 hits).

**M10 frozen messages:** «معرف غير صالح.» (both validators) / «المريض غير موجود.» / «التحليل غير موجود» (no trailing dot — verbatim). Permission: `AuditAccessPolicy.PtAuditAccess = "PT_AUDIT_ACCESS"` (seeded id=13). DTOs in `Features/AuditAndTraceability/Common/AuditDtos.cs` exactly as specced; P handler includes soft-deleted patients and voided operations, resolves names via `Users` dictionary with raw-id fallback, orders receivers `OperationAtUtc` then `UserId`; T handler lifecycle fields nullable.

**M18 frozen messages:** «يوجد تسجيل حضور مفتوح لهذا المستخدم.» / «لا يوجد تسجيل حضور مفتوح.» / «الاستراحة بدأت بالفعل.» / «لا توجد استراحة مفتوحة.» / «أنهِ الاستراحة قبل تسجيل الانصراف.» / «تم تسجيل الانصراف مسبقًا.» / «بيانات غير صالحة.» (translator default) / «بداية الفترة يجب ألا تتجاوز نهايتها.» / «معاملات الترقيم غير صالحة.» (Page ≥ 1; PageSize ∈ [1,100]) / «المستخدم غير موجود.» / Forbidden «أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام». The four command validators are intentionally empty (documented comments); both admin query handlers gate on `_currentUser.IsAbsolutePermission`. `AttendanceRecordDto.FromRecord` computes `WorkedMinutes` (null until check-out); `AttendanceCalculator` compares against `User.WorkStartTime`/`WorkEndTime` (`Domain/Users/User.cs` lines 20/22).

**M19 frozen messages:** «بداية الفترة يجب ألا تتجاوز نهايتها.» (all four validators) / «مجموعة التحاليل غير موجودة.» / «الجهة الخارجية غير موجودة.». Permission: `StatisticsAccessPolicy.Statistics = "STATISTICS"` (seeded id=12). Defaults: omitted period = today; «بدون جهة إحالة» bucket label verbatim; sex/account-type `DisplayName` = `Enum.ToString()` (English). Filter sources confirmed: `GetTestGroupsQuery(bool IncludeInactive = false)`, `SearchExternalEntitiesQuery` (PartnerLab, D10 pattern), `GetUsersQuery` → `UserSummaryDto(Id, UserName, IsAbsolutePermission, IsActive, LastLoginAtUtc)`.

**M20 frozen messages:** «المبلغ يجب أن يكون أكبر من صفر.» / «الجهة الخارجية غير موجودة.» / «بداية الفترة يجب ألا تتجاوز نهايتها.» (all five query validators) / «المستخدم غير موجود.» / «نوع الحساب مطلوب.» (last three from `GetElementInventoryQueryHandler`, read during this audit). Notes ≤ 500: no custom Arabic message (FluentValidation default). Domain guards (`CashMovement.cs`): «Amount must be > 0.» / «Notes must be at most 500 characters.» / «OccurredAtUtc is required.». Permission: `InventoryAndAccountingAccessPolicy.CashDisburseDeposit = "CASH_DISBURSE_DEPOSIT"` (seeded id=11). **Enum locations (corrected, C1):** `InventoryElementKind { User=0, ReferralEntity=1, TreatingDoctor=2, AccountType=3, SentOutSamples=4 }` and `InventoryReportType { Summary=0, Detailed=1, DetailedByPrices=2, DetailedByResults=3 }` in `src/TopLab.Application/Features/InventoryAndAccounting/Common/InventoryDtos.cs`; `MovementType { Disbursement=0, Deposit=1 }` in `Domain/Common/Enums/MovementType.cs`; `AccountType { Individual=0, LabToLab=1, Contracts=2, Vip=3, Free=4 }` in `Domain/Common/Enums/AccountType.cs`. `CashDrawerInventoryDto` carries 18 fields incl. `SafeCash`, `RemainingToLab`, `NetProfit`; `GetPatientSamplesDetailQueryHandler` filters rows to `TestsCount > 0 || Charged != 0 || Paid != 0`.

**M23 frozen messages:** «التعبير الحسابي غير صالح.» / «لا يمكن القسمة على صفر.» / «زوج الوحدات غير مدعوم.» / «وقت النهاية يسبق وقت البداية.» / «مجموعة التحاليل غير موجودة.» / «الاسم مطلوب.» / «رقم الهاتف مطلوب.» / «نص البند مطلوب.» / «البند غير موجود.». Limits without custom messages: phone `Name ≤ 200` / `Phone ≤ 30` / `Notes ≤ 500`; purchase `Text ≤ 200`; library `NameFilter ≤ 200`; remove/toggle `Id > 0`. `MeasurementUnitConverter` holds exactly 18 fixed pairs (read in full). Stores: `IPhoneBookStore`/`IPurchasesListStore` (Application) + `JsonPhoneBookStore`/`JsonPurchasesListStore` (Infrastructure) — station-local.

**Cross-cutting:** `ResultErrorPresenter` — Forbidden → «أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام»; Unexpected → «حدث خطأ غير متوقع. حاول مرة أخرى.». Navigation idiom: `_navigation.NavigateTo<XViewModel>(); if (_navigation.CurrentViewModel is XViewModel vm) { await vm.LoadAsync(...); }`. `IDialogService.ShowConfirmationAsync(title, message)` for all confirmations.

## Settled Decisions

- **D5 — Statistics (M19): جداول فقط بلا رسوم بيانية:** **«قرار نهائي بتفويض من المالك — اتخذه الوكيل بناءً على تحليل الكود»**. القرار: العرض جداول وأرقام مجمعة فقط؛ لا مكتبة رسوم ولا مرجع حزمة جديد. الأساس (المسار a — الكود الحي، حاسم): (1) `Directory.Packages.props` و`TopLab.Presentation.csproj` بلا أي مكتبة رسم؛ (2) بحث شامل (`livecharts|oxyplot|scottplot|microcharts|chart`) عبر كل الحل = صفر؛ (3) الـ DTOs الأربعة كاملة التجميعات للجداول بلا نقص. بوابة التنفيذ: VG-03 بند الفحص (grep + diff فارغ على ملفات الحزم).
- **D11 — Utilities (M23): حساب المنقضي عبر الاستعلام:** **«قرار نهائي بتفويض من المالك — اتخذه الوكيل بناءً على تحليل الكود»**. القرار: كل حساب منقضٍ عبر `ComputeStopwatchElapsedQuery` (`EndUtc = null` ← وقت الخادم)؛ لا حساب محلي في الواجهة. الأساس (المسار a — حاسم): `ComputeStopwatchElapsedQueryHandler` يعيد `StopwatchElapsedDto(StartUtc, EndUtc, Elapsed)` محسوباً عبر `StopwatchCalculator.Elapsed`؛ المسار (c) مؤيد (تجنب انحراف ساعة العميل). بوابة التنفيذ: VG-07 بند الفحص (grep على استهلاك الاستعلام وغياب حساب محلي).
- **D12 — AuditAndTraceability (M10): الدخول عبر عنوان «النظام»:** **«قرار نهائي بتفويض من المالك — اتخذه الوكيل بناءً على تحليل الكود»**. القرار: ربط «النظام» القائم مباشرة بـ `AuditViewModel` (تبويبا P/T)؛ بلا زر شِل جديد. الأساس (المسار a): «النظام» ممكّن ويسقط في `// Future` عند `c34b9ad` (قُرئ `ShellViewModel.cs` كاملاً)؛ ولا عنصر نائب طبيعي في `SettingsDashboardViewModel` (قُرئ كاملاً — أوامره الثمانية محجوزة)؛ (b) غير منطبق؛ (c) مؤيد (اصطلاح LIS المكتبية). بوابة التنفيذ: VG-06 بند الفحص.

## Recorded Open Decisions (with stop-gates)

- **نقطة دخول شاشات Attendance (M18) الثلاث:** **«قرار نهائي من المالك»** — **(أ) امتداد مسار «المستخدمون» في الشِل**. المالك حسمها مباشرة عند سؤاله. التنفيذ: أزرار الحضور الثلاثة (سجلات الحضور + ملخص المستخدم + حضوري الذاتي) مضافة داخل `UserManagementView` بعد فحص كلمة المرور الثانوية القائم على مسار «المستخدمون».

## Slice Validation Gates (from plan)

- **VG-01 (Slice 0):** build 0/0; suite green; zero-drift; four attendance commands consumed; check-out behind confirmation; no navigation wiring added; verbatim failure messages; manual scenarios recorded.
- **VG-02 (Slice 1):** build 0/0; suite green; zero-drift; both admin queries + `GetUsersQuery` consumed; entry point wired per the owner's settled decision (not self-resolved); no new permission string; manual scenarios recorded.
- **VG-03 (Slice 2):** build 0/0; suite green; zero-drift; four statistics queries consumed; **D5 grep gate** (no charting reference; package files diff empty); «الإحصائيات» wired; manual scenarios recorded.
- **VG-04 (Slice 3):** build 0/0; suite green; zero-drift; drawer + movements queries and both cash commands consumed; «الحسابات» enabled + wired behind secondary password; both movements behind confirmation; manual scenarios recorded.
- **VG-05 (Slice 4):** build 0/0; suite green; zero-drift; three remaining M20 queries consumed; hub hosts M14/M16 routes; «طريق مؤقت» grep = 0 hits in settings dashboard; D3/D10 comments closed; no duplicated VM; manual scenarios recorded.
- **VG-06 (Slice 5):** build 0/0; suite green; zero-drift; **D12 grep gate** («النظام» → `AuditViewModel`; no new shell button); both audit queries consumed; contextual buttons pass ids; manual scenarios recorded.
- **VG-07 (Slice 6):** build 0/0; suite green; zero-drift; six queries + five commands consumed; **D11 grep gate** (all elapsed computation via the query; no local elapsed arithmetic); «الأدوات» wired; deletions behind confirmation; manual scenarios recorded.

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 0 | Attendance self-service screen | [x] Done | VG-01 PASS |
| 1 | Attendance admin screens + entry point | [x] Done | VG-02 PASS |
| 2 | Statistics dashboard + «الإحصائيات» wiring | [ ] Not started | VG-03 |
| 3 | Accounts hub + cash drawer + «الحسابات» activation | [ ] Not started | VG-04 |
| 4 | Accounts tabs + temporary-route absorption | [ ] Not started | VG-05 |
| 5 | Audit screen + «النظام» wiring | [ ] Not started | VG-06 |
| 6 | Utilities screen + «الأدوات» wiring | [ ] Not started | VG-07 |

## Slice 0: Attendance self-service screen

**Gate:** VG-01. **Status:** ✅ Passed.

### 10-Stage Progress
- [x] 1. Pre-Execution Verification — build 0/0; App tests 1419/1419; HEAD pinned
- [x] 2. Deep Understanding — M18 command handlers + DomainFailureTranslator read; frozen messages verified
- [x] 3. File Analysis — ShellViewModel DispatcherTimer pattern; DI; MainWindow.xaml
- [x] 4. Planning — MyAttendanceViewModel + View; no shell wiring (Slice 1 carries entry point)
- [x] 5. Execution — Created VM + View with 4 commands, live clock, command→response pattern; DI + DataTemplate
- [x] 6. Post-Execution Verification — Presentation build 0/0; full solution 0/0; App tests 1419/1419; zero backend diff
- [x] 7. Validation Gate — VG-01 PASS: all 4 commands consumed; check-out behind confirmation; no navigation wiring; verbatim messages via ResultErrorPresenter
- [x] 8. Documentation Update — memory checklist recorded
- [x] 9. Memory Status Update — Slice 0 done, Slice 1 next
- [x] 10. Git Commit — local commit

## Slice 1: Attendance admin screens + entry point

**Gate:** VG-02. **Status:** ⬜ Not started.

### 10-Stage Progress
- [ ] 1. Pre-Execution Verification
- [ ] 2. Deep Understanding
- [ ] 3. File Analysis
- [ ] 4. Planning — **STOP-GATE:** owner decision on the attendance entry point («بانتظار قرار المالك — غير مُدرج في القائمة الأصلية») must exist before proceeding
- [ ] 5. Execution
- [ ] 6. Post-Execution Verification
- [ ] 7. Validation Gate
- [ ] 8. Documentation Update
- [ ] 9. Memory Status Update
- [ ] 10. Git Commit

## Slice 2: Statistics dashboard + «الإحصائيات» wiring

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

## Slice 3: Accounts hub + cash drawer + «الحسابات» activation

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

## Slice 4: Accounts tabs + temporary-route absorption

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

## Slice 5: Audit screen + «النظام» wiring

**Gate:** VG-06. **Status:** ⬜ Not started.

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

## Slice 6: Utilities screen + «الأدوات» wiring

**Gate:** VG-07. **Status:** ⬜ Not started.

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

## Created UI Texts Register (append at execution time)

| Slice | Screen | Text (Arabic, as shipped) | Kind |
|---|---|---|---|
| 0 | MyAttendanceView | «جاهز — اختر الإجراء المطلوب.» | Ready-state (replaces Empty) |
| 0 | MyAttendanceViewModel | «هل تريد تسجيل الانصراف الآن؟» | Confirmation (check-out) |
| 0 | MyAttendanceViewModel | «تم تسجيل الحضور بنجاح.» | Status (success) |
| 0 | MyAttendanceViewModel | «تم بدء الاستراحة.» | Status (success) |
| 0 | MyAttendanceViewModel | «تم إنهاء الاستراحة.» | Status (success) |
| 0 | MyAttendanceViewModel | «تم تسجيل الانصراف بنجاح.» | Status (success) |
| 1 | AttendanceRecordsView | «لا توجد سجلات في هذه الفترة.» | Empty-state |
| 1 | UserAttendanceSummaryView | «اختر مستخدماً واضغط «عرض» لعرض الملخص.» | Empty-state |

Expected entries (from the audited plan — all Requires-creation view texts): «لا توجد عمليات دفع مسجلة لهذا المريض.» (S5, P-tab empty) / check-out confirmation text (S0) / «لا توجد سجلات في هذه الفترة.» (S1, empty) / «لا توجد بيانات في هذه الفترة.» (S2 + S4, empty) / cash-movement confirmation text showing direction + amount (S3) / «لا توجد حركات نقدية في هذه الفترة.» (S3, empty) / «إيداع»/«صرف» MovementType display translations (S3) / «لا توجد عينات في هذه الفترة.» (S4, empty) / InventoryElementKind + InventoryReportType display translations (S4) / phone-book/purchases delete confirmations (S6) / «هذه البيانات محلية لهذه المحطة ولا تُزامَن.» (S6, scope note) / empty-state texts for utilities tabs 4/5/6 (S6) / any display text for the Notes ≤ 500 limit if surfaced (S3/S6).

## Current Status
- **Slice 0:** Done. VG-01 passed. Commit `816ee6e`.
- **Slice 1:** Done. VG-02 passed.
- **Slice 2:** Not started. Statistics dashboard + «الإحصائيات» wiring.
- **Slice 3:** Not started.
- **Slice 4:** Not started.
- **Slice 5:** Not started.
- **Slice 6:** Not started.

---

## Corrections Log (independent audit of «Fifth Pass Plan.md» at `c34b9ad35022684f0b65ab2cfe4e6b84c511d59f`)

Audit method: fresh clone of `https://github.com/El-ogra/Top-Lab.git`; `git checkout c34b9ad35022684f0b65ab2cfe4e6b84c511d59f` (detached HEAD; `git status --porcelain` → 0 lines); every material claim re-verified by opening the actual file at this commit. The plan's self-reported quality-gate log was treated as a claim, not evidence. No repository file was modified; no commit, push, branch, or history change was made.

**Corrections made (5):**

1. **C1 — Enum location:** the plan (§4.2, "التعدادات") cites `InventoryElementKind` and `InventoryReportType` as residing in `Domain/Common/Enums/`. Observed at the target commit: both enums are declared inside `src/TopLab.Application/Features/InventoryAndAccounting/Common/InventoryDtos.cs` (namespace `TopLab.Application.Features.InventoryAndAccounting.Common`); `grep` confirms no `Domain/Common/Enums/InventoryElementKind.cs` or `InventoryReportType.cs` exists. (`MovementType` and `AccountType` citations were correct: `Domain/Common/Enums/MovementType.cs` / `AccountType.cs`.) Corrected in S-06.md §2 and §4 Slice 4. No behavioural change — the enum values themselves verified identical to the plan.
2. **C2 — Declared evidence gap closed:** the plan (§4.4 شاشة 3) marked `GetElementInventoryQueryHandler`'s internal NotFound messages "Uncertain — لم تُقرأ في هذه الجلسة" and deferred reading to the executor. Opened and read during this audit: User element → NotFound «المستخدم غير موجود.»; ReferralEntity/TreatingDoctor element → NotFound «الجهة الخارجية غير موجودة.»; AccountType element without `AccountType` → Validation «نوع الحساب مطلوب.». Frozen in Confirmed Facts and the Slice 4 spec; the executor no longer needs to discover them.
3. **C3 — D5 status:** the plan carried D5 as «قرار افتراضي من الوكيل — بانتظار موافقة المالك» (§3.6, §حسم القرارات). Re-derived independently from live code (path a decisive: no charting library in `Directory.Packages.props`/`TopLab.Presentation.csproj`; zero-hit solution-wide sweep; DTOs fully table-sufficient); the plan's hypothesis supported. Closed with the owner-mandated label **«قرار نهائي بتفويض من المالك — اتخذه الوكيل بناءً على تحليل الكود»**; embedded in the M19 spec, Slice 2 scope, the VG-03 grep gate, Settled Decisions in both files, and the traceability table.
4. **C4 — D11 status:** same provisional framing in the plan (§5.6, §حسم القرارات). Re-derived: `ComputeStopwatchElapsedQueryHandler` computes and returns `Elapsed` (`EndUtc = null` → `_dateTime.UtcNow`) — path a decisive; hypothesis supported. Closed with the same label; embedded in the M23 spec, Slice 6 scope, the VG-07 grep gate, and both Settled Decisions sections.
5. **C5 — D12 status:** same provisional framing in the plan (§1.6, §حسم القرارات). Re-derived: «النظام» enabled and unwired at `c34b9ad` (ShellViewModel read in full); no natural audit placeholder in `SettingsDashboardViewModel` (read in full — all eight commands reserved); path (b) inapplicable; path (c) supportive. Hypothesis supported. Closed with the same label; embedded in the M10 spec, Slice 5 scope, the VG-06 grep gate, and both Settled Decisions sections.

**Verified without correction (spot-audit held):** commit identity and clean tree; all 12 shell titles and their exact wiring incl. the «الحسابات» disable + D3 comment + the three `// Future` titles; the two temporary settings-dashboard buttons and their VM commands/comments; PatientsHub gateway settled state; permission seeds 11/12/13; every frozen Arabic message in the tables above (byte-for-byte against the validators/handlers/translators); all command/query/DTO/validator names and signatures for the five modules; M18's parameterless commands and intentionally empty validators; the `IsAbsolutePermission` admin gate; `GetUsersQuery`/`UserSummaryDto` shape; `GetTestGroupsQuery(bool IncludeInactive = false)`; the 18-pair converter table; JSON store interfaces/implementations; `GetPatientSamplesDetailQueryHandler`'s row filter; `CashMovement` domain guards; complete absence of P5 Views/ViewModels/DI registrations/DataTemplates.

**Preserved as open (not decided):** the plan's newly-discovered decision — the official entry point of the three M18 attendance screens — remains verbatim «بانتظار قرار المالك — غير مُدرج في القائمة الأصلية». The delegation was scoped to D5/D11/D12 only; no other ambiguity was resolved by the auditor. Stop-gate placed at Slice 1, Stage 4.

---

## Stop Report (append only if a stop condition triggers)

**STOP resolved at Slice 1, Stage 4 — 2026-09-18.** Owner settled the attendance entry point directly: **(أ) امتداد مسار «المستخدمون» في الشِل**. Implementation: three attendance buttons (سجلات الحضور + ملخص المستخدم + حضوري الذاتي) added inside `UserManagementView` after the existing secondary-password gate on the «المستخدمون» shell path. Execution resumed.

---

## Execution Log

| Timestamp | Slice | Stage | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-09-18 | 0 | 1-10 | MyAttendanceViewModel + View; 4 commands + live clock; DI + DataTemplate; VG-01 PASS | Success |
| 2026-09-18 | 1 | 1-3 | Pre-exec + deep understanding + file analysis | Success |
| 2026-09-18 | 1 | 4 | STOP-GATE resolved: owner settled (أ) امتداد مسار «المستخدمون» | Resolved |
| 2026-09-18 | 1 | 5-10 | AttendanceRecordsViewModel + View + UserAttendanceSummaryViewModel + View; 3 attendance buttons in UserManagementView; VG-02 PASS | Success |
