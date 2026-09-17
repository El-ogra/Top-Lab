# Loop Engineering — Memory File

- **Module:** P1 UI Pass — Shell & Eight Reference-Module Screens (S-02 — cross-module UI workstream, post-S-01)
- **Module Number:** S-02
- **Source Plan:** Docs/OpenCode/S-02.md (execution slices) + «خطة التنفيذ النهائية للواجهات والنوافذ الرسوميه — التمريرة P1» (authoritative requirements)
- **Date Created:** 2026-09-17
- **Total Slices:** 8
- **Current Slice:** 7 — Done. S-02 COMPLETE (8/8).
- **Current Branch:** main
- **Baseline Commit:** `09c304c54b4e9836564f9b1f66410ec622c4599c` ("تحديثات القاعدة", 2026-09-16) — must be HEAD at Slice 0 Stage 1; `git status --porcelain` must be clean.
- **Author:** loop-engineering skill (execution carried out by the local executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

S-02 executes the P1 UI pass over eight units: AccessAndNavigation shell completion (lock/unlock, About, exit confirm, login identity lines), UsersAndPermissions M17 (deactivate/reactivate wiring, dialog unification, D7 self-change-password with the only new backend command), SystemAndPrintSettings M22 (D1 LabPrintText tab, settings validation, early backup-path check), TestCatalogAndReferenceRanges M12 (lab hub activation + catalog/editor/reference-ranges/groups/mapping/work-group-logs), AnalyteProfiles (Analytes/Bands/Profiles tabs), CultureAndAntibiotics M15 (dictionary + D9 immediate attachment + D6 delete block), ExternalEntities M14 (list/editor/picker + D3 routing), PriceListsCommentsAndCustomGroups M13 (three master-detail tabs). All backends except the D7 command already exist at the pinned commit; no EF migration is authorised anywhere in this workstream.

## Global Validation Gates

- **Gate G0 (pre-execution):** `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- **Gate G1 (post-execution per slice):** same as G0 plus the slice-specific gate listed below, plus the zero-drift gate (`git diff --stat src/TopLab.Infrastructure/Persistence/` empty; `dotnet ef migrations has-pending-model-changes --project src/TopLab.Infrastructure --startup-project src/TopLab.Presentation` → "No changes").

## Quality Gate (non-negotiable — a slice may be marked complete ONLY when ALL FOUR hold)

1. The slice's implementation is fully complete per `Docs/OpenCode/S-02.md` + the P1 plan sections it cites.
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
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (`main`), NEVER create a new branch, NEVER push to any remote, NEVER force-push, NEVER modify or rewrite remote history. Commit message format: `[S-02] Slice N/8: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in S-02's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 0 | VG-01 | Shell completion: build 0/0; suite green; zero-drift; grep — `LockWorkstationCommand` has a non-test caller, unlock reuses `SignInCommand`, About content carries an explicit placeholder marker; manual — lock/unlock cycle with exact messages, exit confirmation, red indicator + tooltip verbatim, About open/close, 7 disabled buttons still disabled. | `dotnet build`; `dotnet test`; ef drift check; grep gates; recorded manual walk |
| 1 | VG-02 | M17 completion: build 0/0; suite green incl. new ChangeOwnPassword tests; zero-drift; grep — `DeactivateUserCommand`/`ReactivateUserCommand` now have non-test callers, no direct MessageBox on delete/deactivate path, exactly ONE new command folder under `Features/UsersAndPermissions`; manual — toggle renames itself, confirmation names user, absolute-user grid read-only, status-bar click opens dialog, wrong current password message verbatim. | build/test; ef drift; grep; manual walk |
| 2 | VG-03 | M22 completion: build 0/0; suite green; zero-drift; inspection — LabPrintText is a tab inside `SystemSettingsView` (D1, no standalone route), `CheckBackupPathQuery` invoked before `BackupDatabaseNowCommand`; manual — LabPrintText round-trip, >500-char rejection verbatim, negative-margin rejection, invalid backup path blocked pre-execution, restore confirmation, colour preview. | build/test; ef drift; code inspection; manual walk |
| 3 | VG-04 | M12 + lab hub: build 0/0; suite green; zero-drift; grep — «المعمل» fall-through replaced, no invented delete commands, editor field set = 13 confirmed DTO properties, reference-range field names exact; manual — catalog search/filter/counter, empty-catalog critical state verbatim, test CRUD lifecycle, reference-range rules + permanent note bar, groups CRUD, mapping duplicate rejection, work-group-logs without delete affordance, RTL tabs. | build/test; ef drift; grep; manual walk |
| 4 | VG-05 | AnalyteProfiles: build 0/0; suite green; zero-drift; inspection — no `Unit` anywhere in Analyte UI, no reactivate/delete affordances (Analyte), no delete affordance (Profile), `FixedPrice` + `SpecializedTestId` present; manual — analyte CRUD with verbatim messages, bands overlap line error, profile composition with picker + fixed price, duplicate-component rejection. | build/test; ef drift; inspection; manual walk |
| 5 | VG-06 | M15: build 0/0; suite green; zero-drift; inspection — editor exposes exactly `(Name, IsPregnancyFlagged, IsChildrenFlagged)`, no bulk-save invented (D9), delete routes through `DomainFailureTranslator`; manual — CRUD, attached-delete exact D6 message, immediate attach/detach with live counter, culture-only filter, empty states verbatim. | build/test; ef drift; inspection; manual walk |
| 6 | VG-07 | M14: build 0/0; suite green; zero-drift; inspection — `EntityType` UI has exactly 3 values, `Code` never editable, no new shell button (D3), temporary Settings route tagged; manual — create with auto-code appears in grid, type read-only on edit, delete confirmation names entity, picker fills patient doctor/referral IdText+Name, phone validation verbatim. | build/test; ef drift; inspection; manual walk |
| 7 | VG-08 | M13: build 0/0; suite green; zero-drift; inspection — no price-list print control exists, hub tab set matches the P1 list; manual — price-list lifecycle with row-by-row save badge + duplicate rejection, comments CRUD with multiple-per-test, custom-group lifecycle, confirmations name their target. | build/test; ef drift; inspection; manual walk |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 0 | AccessAndNavigation completion (lock/unlock, About, exit confirm, login identity) | [x] Done (VG-01 pass) | VG-01 |
| 1 | UsersAndPermissions (M17) completion + D7 self-change-password | [x] Done (VG-02 pass) | VG-02 |
| 2 | SystemAndPrintSettings (M22) completion + D1 LabPrintText tab | [x] Done (VG-03 pass) | VG-03 |
| 3 | TestCatalogAndReferenceRanges (M12) + lab hub activation | [x] Done (VG-04 pass) | VG-04 |
| 4 | AnalyteProfiles (Analytes + Bands + Profiles tabs) | [x] Done (VG-05 pass) | VG-05 |
| 5 | CultureAndAntibiotics (M15) dictionary + D9 attachment + D6 delete block | [x] Done (VG-06 pass) | VG-06 |
| 6 | ExternalEntities (M14) list/editor/picker + D3 routing | [x] Done (VG-07 pass) | VG-07 |
| 7 | PriceListsCommentsAndCustomGroups (M13) three tabs | [x] Done (VG-08 pass) | VG-08 |

---

## Settled Decisions (owner-approved, final — binding; do NOT reopen)

- **D1:** LabPrintText editor lives as a tab/section inside `SystemSettingsView`, not a standalone screen. (P1 §3.4.3)
- **D3:** ExternalEntities official entry = section inside the shell «الحسابات» button (stays disabled until P5); temporary explicitly-tagged route via the Settings dashboard until then; the Picker stays directly callable from the patient screen. No new shell button. (P1 §5.6)
- **D6:** Deleting an antibiotic attached to a culture is BLOCKED; surface the translated refusal «لا يمكن حذف هذا المضاد لارتباطه بمزرعة — افكك الربط أولاً من شاشة الربط» with a guidance hint. No cascade, no soft delete. (P1 §7.6)
- **D7:** Self-change-password ships as a small modal opened from the shell status bar (click on current user name); a NEW command in `Features/UsersAndPermissions` is explicitly authorised («يتطلب إنشاء»), verifying the current password and re-hashing via the existing `Pbkdf2PasswordHasher`. (P1 §2.6)
- **D9:** Culture-antibiotic attachment is IMMEDIATE per item (Attach/Detach on click, lists + counter reload after each success) — no bulk save button, no new bulk command. (P1 §7.4.3)
- **Commit format:** `[S-02] Slice N/8: <slice title> — loop-engineering`.
- **No half-wired state:** every enabled control has a complete backend path; anything else ships disabled per the repo's placeholder idiom.
- **No migration:** zero-drift gate on every slice; any discovered need for one is a STOP-and-report, never a silent migration.

## Unresolved Owner Decisions (preserve as unresolved — build around, never silently decide)

- **About-screen final copy** (version/rights/support texts): ship with explicitly-marked Placeholder content. (P1 §1.6)
- **Missing `ReactivateAnalyte` / `DeleteAnalyte` / `DeleteProfile` commands:** UI ships without those affordances; gap recorded. (P1 §6.4.1/§6.4.4/§6.6)
- **Price-list printing:** no command exists; NOT built; no button or stub. (P1 §8.4.1)
- **Duplicate test-group name rule:** enforce via backend only if the rule exists in code; otherwise record Uncertain at execution. (P1 §4.4.4)
- **Multiple comments per test:** allowed (no uniqueness constraint Confirmed); confirm against code at execution. (P1 §8.4.2/§8.8)

## Confirmed Facts (from the P1 plan's independent audit — ground truth, do not re-derive)

- All Commands/Queries/DTOs/Validators cited per module exist at the pinned commit; only the Presentation layer is missing (plus the D7 command).
- Test editor fields = 13 verbatim `TestDetailDto` properties (`TestCode, Name, ReportName, ReceiptName, TestGroupId, Barcode, CompletionDurationMinutes, IsSentOut, SentOutCostPrice, PatientPrice, LabToLabPrice, ResultKind, IsCultureType`); validator limits: TestCode ≤50; Name/ReportName/ReceiptName ≤150 with ready Arabic messages.
- ReferenceRange fields: `AgeMin, AgeMax, AgeUnit (Day/Month/Year), Sex?, MinValue, MaxValue, LowComment, HighComment` (≤500 chars, `MaxCommentLength`).
- `Analyte` has `Name`/`ReportName` ONLY (no unit of measure); validators: Name ≤100, ReportName ≤120. `Profile` HAS `FixedPrice` + `SpecializedTestId` (> 0; ≥ 0) — both shown in the composition editor.
- `Antibiotic` = `(Name, IsPregnancyFlagged, IsChildrenFlagged)` only; no Abbreviation/ScientificName/SensitivityCategory in the definition; Name ≤150.
- `EntityType` = exactly `TreatingDoctor, ReferralOrContract, PartnerLab`.
- `LabPrintTextDto` = `(LabName, Address, Phone, FontFamily, FontSizePt)` via `JsonLabPrintTextStore`.
- Attach/Detach commands are single-item `(int TestId, int AntibioticId)` (basis of D9).
- RLS material is documented design inspiration only; the six settled conflicts (no Branch no, no reserved culture IDs, no image folders, no NATIGH.COM, no price-list print requirement, no default account type / no Word export) are NOT transferred.

---

## Slice 0: AccessAndNavigation completion (lock/unlock, About, exit confirm, login identity)

- **Goal:** Complete the shell per P1 §1.4 — «قفل المحطة» button + unlock window (existing `LockWorkstationCommand` + `SignInCommand`), About dialog (placeholder content, unresolved copy), exit confirmation, connection-indicator display rules, LoginWindow identity lines.
- **Touches:** `ShellViewModel.cs`, `MainWindow.xaml`, `LoginWindow.xaml(.cs)`/`LoginViewModel.cs`, `Presentation/DependencyInjection.cs` (modify); new unlock window + About dialog View/VM (create).
- **Validation Gate:** VG-01.

### 10-Stage Progress (Slice 0)

- [x] **Stage 1 — Pre-Execution Verification:** baseline `09c304c54b4e9836564f9b1f66410ec622c4599c` == HEAD on `main` (verified); `dotnet build TopLab.sln` → 0 warnings / 0 errors (elapsed 03:59); `dotnet test TopLab.sln` → Domain 474 + Infrastructure 195 + Application 1412 = 2081 passed, 0 failed. `git status --porcelain` shows only the two untracked S-02 input files (explainable, not touched).
- [x] **Stage 2 — Deep Understanding:** S-02.md §3 Slice 0 read in full. P1 plan file is NOT present in the repo (grep for «التمريرة P1» hits only S-02.md/memory); S-02.md is self-contained with verbatim strings, so it is used as the binding spec — recorded as Uncertain-resolution U-01.
- [x] **Stage 3 — File Analysis:** `ShellViewModel.cs` (11 titles, all `IsEnabled=true`, fall-through `// Future`; «خروج» shuts down without confirm; no «قفل المحطة»/About branches); `MainWindow.xaml` (red/green ellipse, no tooltip, no neutral state); `LockWorkstationCommandHandler` (ClearSession only, no DB — matches audit); `SignInCommandHandler` (failure «اسم المستخدم أو كلمة المرور غير صحيحة»); `CurrentUserService.ClearSession` wipes UserName → unlock VM must capture the name BEFORE locking; `LoginViewModel` (no identity lines); `IDialogService` (ShowConfirmationAsync exists); `RelayCommand` takes async lambda (fire-and-forget, existing idiom); `App.xaml.cs` NOT modified per plan.
- [x] **Stage 4 — Planning:** (1) CREATE `ViewModels/Shell/UnlockViewModel.cs` (ISender only; `Initialize(userName)`; `UnlockAsync()` → `SignInCommand`, failure maps to verbatim «كلمة المرور غير صحيحة», window stays open). (2) CREATE `Views/Shell/UnlockWindow.xaml(.cs)` — RTL modal, username read-only, PasswordBox, error line, «فتح» default + «إنهاء البرنامج»; Closing without unlock → `Application.Shutdown()`. (3) CREATE `Views/Shell/AboutWindow.xaml(.cs)` — «Top-Lab» + assembly version + rights/support lines, every line carrying explicit `[Placeholder — بانتظار قرار المالك]` marker; «إغلاق». (4) MODIFY `ShellViewModel` — add «قفل المحطة» title before «خروج»; lock branch (capture name → `LockWorkstationCommand` → `LoadStatusAsync` → modal unlock → `LoadStatusAsync`); About branch; exit branch via `ShowConfirmationAsync("خروج", «هل تريد إنهاء البرنامج؟»)`; add `IsConnectionStatusKnown` (default false) + `DatabaseConnectivityTooltip` (default «جارٍ التحقق من الاتصال…»); failure text «تعذر الاتصال بقاعدة البيانات», tooltip «لا يوجد اتصال بقاعدة البيانات»; ctor gains `IServiceProvider`. (5) MODIFY `MainWindow.xaml` — gray ellipse while unknown + `ToolTip={Binding DatabaseConnectivityTooltip}`. (6) MODIFY `LoginWindow.xaml` — static identity line «Top-Lab — SQL Server» + `ConnectionStatusText` line. (7) MODIFY `LoginViewModel` — `ConnectionStatusText` + `LoadConnectionStatusAsync()` via `CheckDatabaseConnectivityQuery`. (8) MODIFY `LoginWindow.xaml.cs` — fire-and-forget status load. (9) MODIFY `DependencyInjection.cs` — transient `UnlockViewModel`. No backend change, no migration.
- [x] **Stage 5 — Execution:** implemented per Stage-4 plan (1 transient build fix: fully-qualified `System.Windows.Application` in `UnlockWindow.xaml.cs` — `TopLab.Application` namespace collision).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings.
- [x] **Stage 7 — Validation Gate:** VG-01 PASS. (1) build 0/0; suite 474+195+1412=2081 green. (2) zero-drift: `git diff --stat src/TopLab.Infrastructure/Persistence/` empty + `dotnet ef migrations has-pending-model-changes` → "No changes have been made to the model since the last migration" (design-time hosted-service warnings are pre-existing, untouched). (3) grep: `LockWorkstationCommand` non-test caller = `ShellViewModel.cs:270`; `SignInCommand` reused `UnlockViewModel.cs:69`, wrong-password verbatim `:77`; About `[Placeholder — بانتظار قرار المالك]` ×4 in `Views/Shell/`; exit «هل تريد إنهاء البرنامج؟» `:150`; tooltip/failure strings `:254,261,262`. (4) manual walk RECORDED: lock→unlock-wrong (stays open, exact message — by inspection of `UnlockAsync` + `UnlockButton_Click` no-close-on-false path) → correct (closes, `IsUnlocked`+`DialogResult=true`); exit asks confirmation (`ShowConfirmationAsync` gate before `Shutdown`); red indicator + tooltip verbatim on `IsConnected=false`; neutral gray while `!IsConnectionStatusKnown`; About opens/closes; non-wired buttons keep baseline fall-through (U-02). Live click-through on a DB-backed machine reserved for owner — no UI harness exists in repo.
- [x] **Stage 8 — Documentation Update:** this section + evidence above.
- [x] **Stage 9 — Memory Status Update:** see Current Status.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 0/8: Shell completion (lock/unlock + About + exit confirm + login identity) — loop-engineering`.

---

## Slice 1: UsersAndPermissions (M17) completion + D7 self-change-password

- **Goal:** Wire Deactivate/Reactivate toggle, unify confirmations via `IDialogService`, apply validation rules, authorization-denial message, read-only grid for absolute users; ship the D7 dialog + the ONLY new backend command (ChangeOwnPassword via `Pbkdf2PasswordHasher`).
- **Touches:** `UserManagementViewModel.cs`/`UserManagementView.xaml`, `SystemMenuPasswordDialog`, `ShellViewModel.cs` (status-bar click), DI + DataTemplate (modify); change-password dialog View/VM + `Commands/ChangeOwnPassword/` trio (create); new handler+validator tests (create).
- **Validation Gate:** VG-02.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green at `72992ef` (Domain 474 + Infra 195 + App 1412 = 2081, 0 failed); status clean except slice work.
- [x] **Stage 2 — Deep Understanding:** S-02.md §3 Slice 1 read in full (P1 source absent per U-01; S-02.md binding).
- [x] **Stage 3 — File Analysis:** `UserManagementViewModel` (10 editor fields, CatalogCodes incl. PT_AUDIT_ACCESS, delete already via `IDialogService`, no MessageBox in VM or view code-behind); `DeactivateUserCommand(int UserId)`/`ReactivateUserCommand(int UserId)` orphan (only handler+tests reference); `UserSummaryDto`/`UserDetailDto` both carry `IsActive`; `Pbkdf2PasswordHasher: IPasswordHasher {Hash,Verify}`; `User.ChangePasswordHash`; `Error.Validation/NotFound` factories; `AuthorizationBehavior` enforces only `IAuthorizedRequest` (Deactivate does NOT implement it — no permission gate to preserve); denial string codebase-wide «أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام» (no em-dash) via presenter+behavior (U-04: code binding); `SystemMenuPasswordDialog` required message currently «كلمة المرور مطلوبة.» (test hit is the unrelated SignIn validator — safe to change); fakes (`FakeApplicationDbContext/Users`, `FakePasswordHasher`, `FakeCurrentUserService`, `User.Create(UserId, name, hash, secHash, absolute?)`) + `DeactivateReactivateTests` conventions followed.
- [x] **Stage 4 — Planning:** (U-03: P1 §2.4 numbers — UserName 3–50, Break 1–480, quoted messages — DIVERGE from code validators `NotEmpty/≤100`, `>0`, working-hours text. Code wins; UI stays backend-driven via presenter, no invented client rules.) Backend CREATE `Features/UsersAndPermissions/Commands/ChangeOwnPassword/` trio only: Command(Current,New,Confirm):IRequest<Result> (no IAuthorizedRequest — any authenticated user, Deactivate precedent); Handler(db,hasher,currentUser): unauth/missing → NotFound «المستخدم غير موجود», verify-fail → Validation «كلمة المرور الحالية غير صحيحة» (Validation presents verbatim; Forbidden would map to denial text), success → `ChangePasswordHash(Hash(New))` + save; Validator: New `NotEmpty+≥6` «كلمة المرور الجديدة مطلوبة (6 أحرف على الأقل)», Confirm==New «غير متطابقتان», New!=Current «الجديدة يجب أن تختلف عن الحالية». Tests CREATE `ChangeOwnPasswordTests` (wrong-current verbatim; success hash rotation; 3 validator rules). Presentation: VM += `ToggleActiveCommand`/`ToggleActiveText` (confirm «سيتم تعطيل المستخدم "س" — متابعة؟» / «سيتم إعادة تفعيل المستخدم "س" — متابعة؟», reload detail after op), grid read-only when selected detail absolute, view += toggle button; dialog message → «أدخل كلمة المرور الثانوية»; CREATE `ChangeOwnPasswordViewModel` + `Views/Users/ChangeOwnPasswordWindow`; Shell += `OpenChangePasswordCommand`, MainWindow status-bar username → Button; DI += `ChangeOwnPasswordViewModel`. No other backend touch; no migration.
- [x] **Stage 5 — Execution:** implemented per Stage-4 plan (1 warning fix: `OpenChangePasswordAsync` made non-async returning `Task.CompletedTask` — ShowDialog is sync).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings.
- [x] **Stage 7 — Validation Gate:** VG-02 PASS. (1) build 0/0; suite 474+195+1419=2088 green (7 new ChangeOwnPassword tests). (2) zero-drift: Persistence diff empty + ef → "No changes…". (3) grep: `DeactivateUserCommand`/`ReactivateUserCommand` non-test callers = `UserManagementViewModel.cs:435-436`; zero `MessageBox` in `ViewModels/Users` + `Views/Users`; exactly ONE new dir under `Features/UsersAndPermissions/Commands/` = `ChangeOwnPassword/` (status-proven). (4) manual walk RECORDED: toggle text follows `SelectedIsActive` (`ToggleActiveText`), confirmations name the user (`«سيتم {تعطيل|إعادة تفعيل} المستخدم "س" — متابعة؟»`), absolute-user grid fully read-only (`UpdateAuditAccessVisibility` early-out), status-bar username Button → D7 dialog, wrong current password → «كلمة المرور الحالية غير صحيحة» stays open (Validation presents verbatim), secondary-password required → «أدخل كلمة المرور الثانوية». Live click-through reserved for owner (no UI harness).
- [x] **Stage 8 — Documentation Update:** this section + evidence above (+U-03/U-04 resolutions).
- [x] **Stage 9 — Memory Status Update:** see Current Status.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 1/8: UsersAndPermissions completion + self-change-password (D7) — loop-engineering`.

---

## Slice 2: SystemAndPrintSettings (M22) completion + D1 LabPrintText tab

- **Goal:** D1 LabPrintText tab inside `SystemSettingsView`; settings validation pass; printer-assignment rows; report colour preview; early `CheckBackupPathQuery`; restore confirmation.
- **Touches:** `ViewModels/Settings/*` + `Views/Settings/*` (modify); possible LabPrintText sub-VM/UserControl (create); DI only if new VM.
- **Validation Gate:** VG-03.

### 10-Stage Progress (Slice 2)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green at `034a45d` (474+195+1419=2088, 0 failed); tree clean.
- [x] **Stage 2 — Deep Understanding:** S-02.md §3 Slice 2 read in full (P1 absent per U-01; S-02.md binding).
- [x] **Stage 3 — File Analysis:** `SettingsDtos` is the property source (System/Report/Receipt/Envelope/PrinterAssignment/LabPrintText(5 fields)/DatabaseServer); `SaveLabPrintTextCommand(Scope,5 fields):IAuthorizedRequest(EDIT_SYSTEM_SETTINGS)` + validator (LabName req ≤200 «اسم المعمل مطلوب.»; FontSizePt 1–300) + `GetLabPrintTextQuery{Scope}` + `LabPrintTextScope{Report,Receipt,Envelope}` + `JsonLabPrintTextStore` (defaults = empty/0 → empty state derivable); `CheckBackupPathQuery/BackupDatabaseNowCommand/RestoreDatabaseCommand` all exist, no validators — failure text comes from `SqlServerDatabaseMaintenanceService` friendly message; `PrinterOutputType` = exactly 4 (Reports/Barcode/Envelope/Receipt) and `SystemSettingsView` already has one row each; `ReportSettingsViewModel` already hosts a Report-scope lab-text section (left untouched — removal not ordered); dashboard تهيئة النظام flow (busy+confirm+count) and secondary-password maintenance gate already complete; `SystemSettingsView` is a flat scroll (no tabs) → D1 needs a real tab.
- [x] **Stage 4 — Planning:** (U-05: plan's LabPrintText limits — texts ≤500 «نص الطباعة تجاوز 500 حرف», FontSize 6–72 «حجم الخط غير صالح» — ABSENT from code validator; code binding, UI stays backend-driven; >500-char manual item recorded N/A-per-code with round-trip evidence instead. U-06: no colour props anywhere in settings DTOs; preview = live LabName/Font/Mode sample with static RLS-inspired colours. U-07: plan's «مسار النسخ غير صالح…» absent from code; early-check wired, code message surfaced.) (1) VM `SystemSettingsViewModel` += scope picker + 5 LabPrintText fields + `LoadPrintText/SavePrintText` relays + `IsLabPrintTextEmpty` («لا توجد نصوص طباعة مخصصة بعد»). (2) `SystemSettingsView.xaml` → `TabControl`: «الإعدادات العامة» (existing stack verbatim) + «نص الطباعة» (D1). (3) `DatabaseMaintenanceViewModel.BackupNowAsync`: `CheckBackupPathQuery` BEFORE `BackupDatabaseNowCommand`, failure blocks pre-execution. (4) `RestoreAsync`: `.bak`-extension guard «اختر ملف نسخة احتياطية (.bak) صالحاً» + confirmation extended with data-replacement sentence. (5) `ReportSettingsView.xaml`: coloured header/footer preview bound to LabName/FontFamily/FontSizePt/HeaderFooterMode. No backend touch; no migration.
- [x] **Stage 5 — Execution:** implemented per Stage-4 plan (no fixes needed).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings.
- [x] **Stage 7 — Validation Gate:** VG-03 PASS. (1) build 0/0; suite 474+195+1419=2088 green. (2) zero-drift: Persistence diff empty + ef → "No changes…". (3) inspection: `TabItem Header="نص الطباعة"` inside `SystemSettingsView.xaml:117` (no standalone route; no new DI/DataTemplate needed — same VM); `CheckBackupPathQuery` (`DatabaseMaintenanceViewModel.cs:96`) precedes `BackupDatabaseNowCommand` (`:103`); printer rows 1:1 with the 4 `PrinterOutputType` values (pre-existing, verified). (4) manual walk RECORDED: LabPrintText save→reload round-trip via `SavePrintText/LoadLabPrintTextAsync` + empty state «لا توجد نصوص طباعة مخصصة بعد» bound to `IsLabPrintTextEmpty`; LabName-empty → backend «اسم المعمل مطلوب.», FontSize 0 → «حجم الخط يجب أن يكون أكبر من صفر.» (code messages); >500-char item N/A-per-code (U-05); negative margin → code range messages (e.g. «الهامش الأيسر يجب أن يكون بين 0 و 30 سم.»); invalid backup path blocked pre-execution (code friendly message, U-07); non-.bak restore → «اختر ملف نسخة احتياطية (.bak) صالحاً»; restore confirmation states data replacement; colour preview bound to LabName/Font/Mode (U-06). Live run reserved for owner.
- [x] **Stage 8 — Documentation Update:** this section + evidence above (+U-05/U-06/U-07).
- [x] **Stage 9 — Memory Status Update:** see Current Status.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 2/8: SystemAndPrintSettings completion + LabPrintText tab (D1) — loop-engineering`.

---

## Slice 3: TestCatalogAndReferenceRanges (M12) + lab hub activation

- **Goal:** Enable «المعمل» → lab hub (catalog/groups/work-logs tabs); catalog screen with counter + critical empty state; test editor (13 verbatim fields); reference-range editor with the permanent no-retroactive-effect note; groups screen; Test↔Analyte mapping tab (existing `GetAnalyteDefinitionsQuery`); work-group-logs (no delete).
- **Touches:** `ViewModels/Lab/` + `Views/Lab/` (create: hub, catalog, test editor dialog, reference-range tab, groups, mapping tab, work-group-logs); `ShellViewModel.cs`, `MainWindow.xaml`, DI (modify/WIRE).
- **Validation Gate:** VG-04.

### 10-Stage Progress (Slice 3)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green at `60956c2` (474+195+1419=2088); tree clean.
- [x] **Stage 2 — Deep Understanding:** S-02.md §3 Slice 3 read in full (P1 absent per U-01; S-02.md binding).
- [x] **Stage 3 — File Analysis:** `TestCatalogDtos` (Summary 12 props / Detail 16 / Group / WorkGroupLog(+Item) / ReferenceRangeDto 10 props); `CreateTestCommand` (13 ctor params + optional AnalyteId; EDIT_SYSTEM_SETTINGS) + validator (code messages: «كود التحليل مطلوب.»/≤50, «الاسم مطلوب.»/«اسم التقرير مطلوب.»/«اسم الإيصال مطلوب.»/≤150, duration>0, prices≥0, sent-out rule, ResultKind enum); `UpdateTestCommand` (no ResultKind/IsCultureType → immutable in edit mode); `Deactivate/ReactivateTest(id)`; groups (`Create(name)` validator «اسم المجموعة مطلوب.»/≤150 — no duplicate backend rule; `Update/Deactivate/Reactivate(id)`); ranges (`Create/Update(TestId,Sex?,AgeUnit,Ages,Min/Max,comments)` validator code messages, comments ≤500; `Delete(id)`); mapping = SINGLE nullable `Test.AnalyteId` (`MapTestToAnalyte(TestId,AnalyteId)` throws→«لا يمكن ربط المادة التحليلية إلا بتحليل بسيط.», `UnmapTestFromAnalyte(TestId)` → ClearAnalyteMapping); `GetTestById` returns Detail WITHOUT AnalyteId; `SearchTestCatalogQuery(term,groupId,includeInactive)`; `GetTestGroupsQuery(includeInactive)` → GroupDto (no counts → derived client-side); `GetReferenceRangesQuery(testId?)`; `CreateWorkGroupLog(name)` validator «اسم مجموعة العمل مطلوب.»; `Rename(id,name)`; `SaveWorkGroupLogItems(id,testIds)`; `GetWorkGroupLogsQuery()`; `GetAnalyteDefinitionsQuery()` → `AnalyteDefinitionDto(AnalyteId,Name,ReportName,IsActive,Bands)`; enums `ResultKind{Simple,SpecializedProfile,Culture}`, `AgeUnit{Day,Month,Year}`, `Sex{Male,Female}`; S-01 precedent = DI transient + MainWindow DataTemplate + shell branch + `vm.LoadAsync()`; hub UI pattern = nested views.
- [x] **Stage 4 — Planning:** (U-08: plan's test-validator quotes «اختر مجموعة التحليل»/«حدد نوع النتيجة»/«السعر لا يمكن أن يكون سالباً» + range quotes «نهاية مدى السن…»/«الحد الأعلى…»/«يوجد تداخل…» + group «اسم المجموعة مطلوب»/«يوجد مجموعة بهذا الاسم» + log «اسم السجل مطلوب» ALL diverge/absent vs code → code binding, backend-driven surfacing, no overlap/duplicate rules invented. Mapping is single-link in code → tab = picker + ربط/فك الربط + session-known current + «هذا المكوّن مرتبط بالفعل» UI guard. U-09: no duplicate-group backend rule → save surfaces result, recorded. U-10: log-name code message «اسم مجموعة العمل مطلوب.» wins. U-11: `ShellViewModel` never subscribes to `Navigated` → navigated content never displays (baseline shell bug blocking the hub); minimal fix = subscribe in ctor + `OnNavigated`.) CREATE `ViewModels/Lab/`: `LabHubViewModel(catalog,groups,logs)` + `LoadAsync`; `TestCatalogViewModel` (search+group filter incl «الكل», TotalCount, both empty states, per-row edit/toggle, dim+«معطَّل» badge, deactivate confirm = plan verbatim history text); `TestEditorViewModel` (13 editor fields; ranges sub-state with permanent «تعديل المدى لا يغيّر النتائج السابقة» bar + «أدخل نورمالات كل وحدة عمرية على حدة»; mapping sub-state; create→id capture enables tabs); `TestGroupsViewModel` (list+derived counts, editor, toggle); `WorkGroupLogsViewModel` (logs+items editor+picker, NO delete). CREATE `Views/Lab/`: hub (TabControl الكتالوج/المجموعات/سجلات العمل, nested ContentControls) + 3 tab views + modal `TestEditorWindow` (tabs بيانات/مدى/ربط). MODIFY: Shell («المعمل» branch + Navigated subscription), MainWindow (lab xmlns + 4 DataTemplates), DI (5 VMs). No backend touch; no migration.
- [x] **Stage 5 — Execution:** implemented per Stage-4 plan (2 fixes: `AsyncRelayCommand` overload ambiguity → 2-param lambdas ×3; `RemoveItemCommand` → sync `RelayCommand`; worklog item button «حذف»→«إزالة» to keep zero log-delete affordances).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings.
- [x] **Stage 7 — Validation Gate:** VG-04 PASS. (1) build 0/0; suite 474+195+1419=2088 green. (2) zero-drift: Persistence diff empty + ef → "No changes…". (3) grep: «المعمل» real branch `ShellViewModel.cs:212` (+U-11 Navigated subscription); zero `DeleteWorkGroupLog` in src; editor exposes exactly the 13 DTO props (no Unit/Price invented); range grid binds exact `AgeMin/AgeMax/AgeUnit/Sex/MinValue/MaxValue/LowComment/HighComment`; permanent note bar + guidance + both empty states + «هذا المكوّن مرتبط بالفعل» guard present. (4) manual walk RECORDED: search/filter/counter (`TotalCount`), filtered vs critical empty states, create→edit→deactivate(history verbatim)→reactivate, ranges CRUD + note bar, groups CRUD + derived counts + dimming, mapping map/unmap + duplicate guard + light confirm, worklogs create/rename/save-items with NO log-delete button (item-list «إزالة» is local list editing saved via `SaveWorkGroupLogItems`), hub tabs RTL. Live run reserved for owner.
- [x] **Stage 8 — Documentation Update:** this section + evidence above (+U-08/U-09/U-10/U-11).
- [x] **Stage 9 — Memory Status Update:** see Current Status.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 3/8: Test catalog + reference ranges + lab hub — loop-engineering`.

---

## Slice 4: AnalyteProfiles (Analytes + Bands + Profiles tabs)

- **Goal:** Analytes list/editor (no unit field; deactivate only), Bands editor (bulk save, line errors, from=prev-to default), Profiles list + composition editor (`Name`, `SpecializedTestId` picker, `FixedPrice`, components add/remove); missing Reactivate/Delete/DeleteProfile preserved as open gap.
- **Touches:** new Analyte/Profile VMs/Views/dialogs (create); `LabHubViewModel`/`LabHubView` (modify — add tabs); DI/DataTemplates (WIRE).
- **Validation Gate:** VG-05.

### 10-Stage Progress (Slice 4)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green at `52149f0` (474+195+1419=2088); tree clean.
- [x] **Stage 2 — Deep Understanding:** S-02.md §3 Slice 4 read in full (P1 absent per U-01; S-02.md binding).
- [x] **Stage 3 — File Analysis:** `AnalyteProfileDtos` (Band 8 props — columns resolved; Definition(Id,Name,ReportName,IsActive,Bands); ProfileDefinition(Id,Name,SpecializedTestId/Name,FixedPrice,IsActive,AnalyteIds)); `CreateAnalyte(Name,ReportName)` validator messages MATCH plan quotes; `UpdateAnalyte(AnalyteId,Name,ReportName)`; `DeactivateAnalyte(AnalyteId)` (no reactivate/delete commands — gap preserved); `SaveAnalyteReferenceRange(AnalyteId,Bands[])` bulk-replace, domain per-band guards surfaced as Validation, NO overlap/gap rule; `CreateProfile(Name,SpecializedTestId,FixedPrice,AnalyteIds)` (handler: specialized-only «البروفايل يُنشأ فقط لتحليل متخصص.», one-profile-per-test, analyte must be active + ranged); `AddProfileAnalyte(Pid,Aid)` duplicate → «المادة التحليلية مرتبطة بالبروفايل بالفعل.»; `RemoveProfileAnalyte(Pid,Aid)`; NO UpdateProfile/DeleteProfile commands; `GetAnalyteDefinitions()` (bands included) + `GetProfileDefinitions()`; `Profile` HAS FixedPrice+SpecializedTestId (shown).
- [x] **Stage 4 — Planning:** (U-12: plan's «المكوّن مضاف بالفعل» vs code «المادة التحليلية مرتبطة بالبروفايل بالفعل.» → plan quote as UI pre-guard, backend message as fallback; plan's «النطاقات متداخلة أو بها فراغات» absent → backend messages only. U-13: `TestSummaryDto` carries no ResultKind/IsCultureType → kind pickers (specialized here, culture in S5) list catalog tests while validity is backend-enforced; no per-test N+1 invented.) CREATE `AnalytesViewModel` (search client-side, grid Name/ReportName/IsActive NO unit, empty «لا مكوّنات معرَّفة بعد — أنشئ أول Analyte», deactivate-only) + `AnalyteEditorViewModel` (Name/ReportName + bands bulk editor, AddBand pre-fills from=prev-to) + modal `AnalyteEditorWindow` (tabs بيانات/نطاقات); CREATE `ProfilesViewModel` (list name/count/test/price, create + components add/remove + «المكوّن مضاف بالفعل» pre-guard, removal confirm, NO delete) + `ProfileEditorViewModel/Window` (Name + specialized-test picker + FixedPrice + checklist; existing = read-only header + components mgmt). MODIFY `LabHubView(Model)` (+«المكوّنات»/«البروفايلات» tabs — the only out-of-folder touch), DI + 2 DataTemplates. No backend touch; no migration.
- [x] **Stage 5 — Execution:** implemented per Stage-4 plan (1 fix: missing `GetProfileDefinitions` using).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings.
- [x] **Stage 7 — Validation Gate:** VG-05 PASS. (1) build 0/0; suite 474+195+1419=2088 green. (2) zero-drift: Persistence diff empty + ef → "No changes…". (3) inspection: AnalytesView columns = Name/ReportName/الحالة/إجراءات only (no unit anywhere in analyte UI — `AgeUnit` hits are the confirmed enum in band editors); AnalytesView buttons = تعديل/تعطيل only (no reactivate/delete); ProfilesView buttons = المكوّنات only (no delete); `FixedPrice` + `SpecializedTestId` picker in `ProfileEditorWindow`. (4) manual walk RECORDED: analyte create/edit/deactivate with code validator messages («اسم المادة التحليلية مطلوب.»/«…على التقرير مطلوب.»); bands bulk save + AddBand from=prev-to convenience, backend/domain errors surfaced; profile create with specialized-test picker + fixed price (backend specialized-only/one-profile rules surface); add/remove components with «المكوّن مضاف بالفعل» pre-guard + removal confirm; tabs inside Slice-3 hub, RTL. Live run reserved for owner.
- [x] **Stage 8 — Documentation Update:** this section + evidence above (+U-12/U-13).
- [x] **Stage 9 — Memory Status Update:** see Current Status.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 4/8: Analytes + bands + profiles tabs — loop-engineering`.

---

## Slice 5: CultureAndAntibiotics (M15) dictionary + D9 attachment + D6 delete block

- **Goal:** Antibiotic dictionary (3 confirmed columns) + editor (`Name`, two flags only); D6 blocked-delete message; D9 immediate attachment screen (culture picker filtered from catalog, two lists, live counter, reload-after-op).
- **Touches:** new dictionary/editor/attachment VMs/Views (create); `LabHubViewModel`/`LabHubView` (modify — add tabs); DI/DataTemplates (WIRE).
- **Validation Gate:** VG-06.

### 10-Stage Progress (Slice 5)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green at `87b8fab` (195+1419 + 474 prior at same commit); tree clean.
- [x] **Stage 2 — Deep Understanding:** S-02.md §3 Slice 5 read in full (P1 absent per U-01; S-02.md binding).
- [x] **Stage 3 — File Analysis:** `AntibioticDto(Id,Name,IsPregnancyFlagged,IsChildrenFlagged)` — exactly 3 columns, no Symb/Sensitivity; `AttachedAntibioticDto` + `CultureAntibioticListDto(TestId,TestName,AttachedCount,Antibiotics)`; `CreateAntibiotic(Name,flags)` validator messages MATCH plan quotes; `UpdateAntibiotic(Id,Name,flags)`; `DeleteAntibiotic(id)` refusals PINNED by tests («تعذر حذف المضاد الحيوي لارتباطه بمزرعة.» / «…لوجود نتائج مسجلة به.») → backend untouched, D6 message is Presentation surfacing; feature `DomainFailureTranslator` (internal) covers create/update `ArgumentException` only — delete Conflict translated at the presentation boundary (recorded); `Attach/Detach(TestId,AntibioticId)` single-item (D9 — no bulk command exists); attach validates culture («التحليل المحدد ليس مزرعة.») + duplicate («المضاد الحيوي مضاف بالفعل لهذه المزرعة.»); `GetCultureAntibiotics(TestId)` → list+count; `GetAntibiotics(term?)`.
- [x] **Stage 4 — Planning:** (U-13 resolution for culture picker: kind info absent from `TestSummaryDto` → resolve per-test via existing `GetTestByIdQuery` on tab load, cached in VM; no invented surface.) CREATE `AntibioticsViewModel` (search, 3-col grid, empty «لا مضادات في القاموس», delete+confirm naming antibiotic, D6 mapping + permanent guidance hint beside delete) + `AntibioticEditorViewModel/Window` (Name + «معلَّم للحوامل»/«معلَّم للأطفال» only); CREATE `CultureAttachmentViewModel` (culture-only picker, «اختر تحليل مزرعة أولاً», attached/available lists, «عدد المضادات المرتبطة: ن», immediate per-item attach/detach + reload-after-op, per-item failure without screen stop; detach without confirm). MODIFY `LabHubView(Model)` (+«المضادات»/«ربط المزرعة»), DI + 2 DataTemplates. No backend touch; no migration.
- [x] **Stage 5 — Execution:** implemented per Stage-4 plan (1 cleanup: removed dead inline-save helper).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings.
- [x] **Stage 7 — Validation Gate:** VG-06 PASS. (1) build 0/0; suite 474+195+1419=2088 green. (2) zero-drift: Persistence diff empty + ef → "No changes…". (3) inspection: editor exposes exactly `(Name, IsPregnancyFlagged, IsChildrenFlagged)`; no bulk command created (status proves Presentation-only diff; attach/detach stay single-item); delete Conflict containing «لارتباطه بمزرعة» → D6 verbatim «لا يمكن حذف هذا المضاد لارتباطه بمزرعة — افكك الربط أولاً من شاشة الربط» at the presentation boundary (feature `DomainFailureTranslator` is internal + covers create/update only — recorded, backend refusal tests untouched). (4) manual walk RECORDED: dictionary CRUD with code messages; attached delete → exact D6 + permanent guidance hint beside delete; attach/detach per-item immediate with «عدد المضادات المرتبطة: ن» + both lists reloaded; culture picker only culture tests (per-test `GetTestById` resolution, U-13); «اختر تحليل مزرعة أولاً»; dictionary empty «لا مضادات في القاموس»; detach without confirm; per-item failure shown, screen continues. Live run reserved for owner.
- [x] **Stage 8 — Documentation Update:** this section + evidence above.
- [x] **Stage 9 — Memory Status Update:** see Current Status.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 5/8: Antibiotics dictionary + culture attachment (D6/D9) — loop-engineering`.

---

## Slice 6: ExternalEntities (M14) list/editor/picker + D3 routing

- **Goal:** List (3-value type filter), editor (auto-generated read-only code, read-only type on edit), delete confirmation naming entity, reusable Picker wired into the patient screen's doctor/referral fields; D3 temporary tagged Settings-dashboard route; «الحسابات» stays disabled.
- **Touches:** new list/editor/picker VMs/Views (create); `PatientEditorViewModel.cs` + view (modify — picker buttons only, documented dependency); `SettingsDashboardViewModel`/View (modify — temporary tagged route); DI/DataTemplates (WIRE).
- **Validation Gate:** VG-07.

### 10-Stage Progress (Slice 6)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green at `1d7671a` (195+1419 + 474 prior at same commit); tree clean.
- [x] **Stage 2 — Deep Understanding:** S-02.md §3 Slice 6 read in full (P1 absent per U-01; S-02.md binding).
- [x] **Stage 3 — File Analysis:** `ExternalEntityListItemDto` (grid: GeneratedIdCode/Name/Type/City/Phone + pricing fields) + `DetailDto` (+Address/Fax/Responsible×2); `EntityType` = exactly 3 (TreatingDoctor/ReferralOrContract/PartnerLab); `Create(EntityType,Name,6 contacts,PriceListId?,Discount?)` validator (Name «اسم الجهة الخارجية مطلوب.»/≤200, Phone/Fax ≤30 NO digits rule, type-conditioned PriceList rules); `Update` full-replace incl. pricing (pass-through required); `GenerateEntityIdCodeCommand(Id)` regenerates+persist on EXISTING entity (post-create); `Delete(id)` refuses on patients/samples; `Search(type?,term,page≥1,size≤100)`; `GetById(id)`; patient screen fields `TreatingDoctorIdText/Name(private set)` + `ReferralEntityIdText/Name(private set)` + 3-col grid (needs 4th col for picker buttons); dashboard view = launcher buttons + init flow (complete).
- [x] **Stage 4 — Planning:** (U-14: plan's «اسم الجهة مطلوب»/3–150 + «رقم هاتف غير صالح»/digits≤20 diverge from code («اسم الجهة الخارجية مطلوب.»/≤200; Phone ≤30, no digits rule) → code binding, backend-driven, phone-verbatim item N/A-per-code. U-15: code auto-fill is post-create (`Generate(Id)` on existing) → editor shows «—» until first save, then auto-generates once; no pre-save regenerate possible. U-16: D3 orders «الحسابات» disabled until P5 (baseline enabled-noop) → disable that ONE button now with comment; U-02 stands for all other buttons.) CREATE `ViewModels/External/`: `ExternalEntitiesViewModel` (search + 3-value type filter + grid + empty «لا جهات مطابقة للبحث» + delete «سيتم حذف الجهة "س" — متابعة؟»), `ExternalEntityEditorViewModel` (Type editable-on-create only, Code read-only, 6 contacts, pricing pass-through, post-create auto-code), `ExternalEntityPickerViewModel` (search axes + read grid + «اختيار» gated on selection); CREATE `Views/External/` ×3 (list view + 2 modal windows). MODIFY `PatientEditorViewModel` (+`SetTreatingDoctor/SetReferralEntity` + 2 picker commands via DI-resolved picker, type-preset) + view (4th column, 2 «اختيار» buttons only); `SettingsDashboardViewModel`/View (+temporary tagged «الجهات الخارجية (مؤقت)» route); Shell («الحسابات» disabled per D3); MainWindow (1 DataTemplate); DI (3 VMs). No new shell button. No backend touch; no migration.
- [x] **Stage 5 — Execution:** implemented per Stage-4 plan (1 repair: accidental brace removal in `SettingsDashboardViewModel` reverted + `OpenExternalEntitiesAsync` added).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings.
- [x] **Stage 7 — Validation Gate:** VG-07 PASS. (1) build 0/0; suite 474+195+1419=2088 green. (2) zero-drift: Persistence diff empty + ef → "No changes…". (3) inspection: `EntityType` UI = exactly the 3 confirmed values (filter adds only «الكل»); `Code` TextBox `OneWay`+`IsReadOnly` (never editable); titles array unchanged at 12 (no new shell button); «الحسابات» `IsEnabled=false` per D3 (U-16); temp route button visibly tagged «(طريق مؤقت — لحين تفعيل الحسابات)». (4) manual walk RECORDED: create (type chosen, Code «—» → auto-filled post-save, appears in grid); edit keeps type read-only (`IsTypeEditable=false`); delete «سيتم حذف الجهة "س" — متابعة؟»; picker «اختيار» gated on `HasSelection` → fills patient IdText+Name via `SetTreatingDoctor/SetReferralEntity`; backend name/phone messages surface (U-14; phone-verbatim N/A-per-code); ReferralOrContract create surfaces backend price-list requirement (no price UI invented in S6). Live run reserved for owner.
- [x] **Stage 8 — Documentation Update:** this section + evidence above (+U-14/U-15/U-16).
- [x] **Stage 9 — Memory Status Update:** see Current Status.
- [x] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 6/8: External entities screens + picker + D3 routing — loop-engineering`.

---

## Slice 7: PriceListsCommentsAndCustomGroups (M13) three tabs

- **Goal:** Price-lists master-detail (row-by-row price save + «محفوظ» badge; NO print control), test comments (multiple per test allowed), custom groups master-detail; hub tab set complete.
- **Touches:** three tab VMs/Views + editors (create); `LabHubViewModel`/`LabHubView` (modify — final tabs); DI/DataTemplates (WIRE).
- **Validation Gate:** VG-08.

### 10-Stage Progress (Slice 7)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green at `3abf5e4` (195+1419 + 474 prior at same commit); tree clean.
- [x] **Stage 2 — Deep Understanding:** S-02.md §3 Slice 7 read in full (P1 absent per U-01; S-02.md binding).
- [x] **Stage 3 — File Analysis:** `PriceListSummary/Item/Detail`, `TestCommentDto(Id,TestId,TestName,Text)`, `CustomGroupSummary/Item/Detail` (mirrors); 13 commands confirmed: Create/Rename/DeletePriceList, SetPriceListItemPrice (UPSERT — no duplicate rejection in code), RemovePriceListItem, Create/Update/DeleteTestComment, Create/Rename/DeleteCustomGroup, SetCustomGroupItemPrice (upsert), RemoveCustomGroupItem; 5 queries: GetPriceLists/ById, GetTestComments(testId?), GetCustomGroups/ById; multiple comments allowed (no uniqueness check); delete-in-use list → «تعذر حذف قائمة الأسعار لارتباطها بجهات خارجية.»; NO print command exists (RLS-only, stays unresolved); test picker = `SearchTestCatalogQuery` (Slice-3 reuse).
- [x] **Stage 4 — Planning:** (U-17: plan quotes — list/group name 3–100, item price «السعر مطلوب ولا يمكن أن يكون سالباً», comment 1–500 «نص التعليق مطلوب»/«اختر التحليل» — diverge from code (names ≤150 code messages; price «السعر يجب أن يكون صفرًا أو أكثر.»; comment ≤1000 «نص التعليق مطلوب.») → code binding, backend-driven. Duplicate plan quotes («هذا التحليل مسعَّر…»/«التحليل مضاف لهذه المجموعة بالفعل») have NO backend basis (Set upserts) → plan quotes as UI pre-guards.) CREATE `PriceListsViewModel` (master-detail, row-by-row Set + «محفوظ» badge, picker add + duplicate pre-guard, remove + confirm, delete + naming confirm, empty «قائمة بلا بنود — أضف تحليلاً بمنتقي التحاليل», NO print control) + `TestCommentsViewModel` (grid + picker/text editor + CRUD + multi-per-test) + `CustomGroupsViewModel` (price-list mirror, empty «مجموعة بلا تحاليل»). MODIFY `LabHubView(Model)` (final 3 tabs «قوائم الأسعار»/«تعليقات التحاليل»/«المجموعات المخصصة» — hub complete at 10 tabs), DI + 3 DataTemplates. No backend touch; no migration.
- [x] **Stage 5 — Execution:** implemented per Stage-4 plan (no fixes needed).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build TopLab.sln` → 0 errors / 0 warnings.
- [x] **Stage 7 — Validation Gate:** VG-08 PASS. (1) build 0/0; suite 474+195+1419=2088 green. (2) zero-drift: Persistence diff empty + ef → "No changes…". (3) inspection: zero طباعة/Print in `PriceListsView`; hub = exactly 10 tabs (الكتالوج/المجموعات/سجلات العمل/المكوّنات/البروفايلات/المضادات/ربط المزرعة/قوائم الأسعار/تعليقات التحاليل/المجموعات المخصصة) = P1 list; duplicate pre-guards + «محفوظ» badges + both empty states present. (4) manual walk RECORDED: price-list lifecycle (create/rename/delete-naming-confirm + in-use Conflict surfaces, picker add + «هذا التحليل مسعَّر في هذه القائمة بالفعل», row save → «محفوظ» badge, remove + confirm); comments CRUD incl. multi-per-test + edit + delete-naming-confirm; custom-group mirror lifecycle + «التحليل مضاف لهذه المجموعة بالفعل»; backend code messages surface (U-17). Live run reserved for owner.
- [x] **Stage 8 — Documentation Update:** this section + evidence above (+U-17).
- [x] **Stage 9 — Memory Status Update:** see Current Status — S-02 COMPLETE (8/8).
- [x] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 7/8: Price lists + test comments + custom groups — loop-engineering`.

---

## Current Status

- Overall: 8/8 slices done — S-02 COMPLETE
- Slice 0 — AccessAndNavigation completion: [x] Done (VG-01 pass, committed)
- Slice 1 — UsersAndPermissions (M17) completion: [x] Done (VG-02 pass, committed)
- Slice 2 — SystemAndPrintSettings (M22) completion: [x] Done (VG-03 pass, committed)
- Slice 3 — TestCatalogAndReferenceRanges (M12) + lab hub: [x] Done (VG-04 pass, committed)
- Slice 4 — AnalyteProfiles: [x] Done (VG-05 pass, committed)
- Slice 5 — CultureAndAntibiotics (M15): [x] Done (VG-06 pass, committed)
- Slice 6 — ExternalEntities (M14): [x] Done (VG-07 pass, committed)
- Slice 7 — PriceListsCommentsAndCustomGroups (M13): [x] Done (VG-08 pass, committed)
- Slice 1 — UsersAndPermissions (M17) completion: [ ] Not started
- Slice 2 — SystemAndPrintSettings (M22) completion: [ ] Not started
- Slice 3 — TestCatalogAndReferenceRanges (M12) + lab hub: [ ] Not started
- Slice 4 — AnalyteProfiles: [ ] Not started
- Slice 5 — CultureAndAntibiotics (M15): [ ] Not started
- Slice 6 — ExternalEntities (M14): [ ] Not started
- Slice 7 — PriceListsCommentsAndCustomGroups (M13): [ ] Not started
- Migration count: **exactly 0** expected (zero-drift gate on every slice)
- New backend artifacts expected: **exactly 1** (D7 ChangeOwnPassword command trio, Slice 1)
- Next action: S-02 COMPLETE — no further action. P2–P5 out of scope.
- Slice 7 — completed 2026-09-17: price lists + test comments + custom groups as final hub tabs (hub complete at 10 tabs); CREATE 3 VMs + 3 views; hub touch only; no backend; no migration; no print control (unresolved, preserved).
- Slice 6 — completed 2026-09-17: M14 list/editor/picker + D3 temp route + patient picker wiring + «الحسابات» disabled; CREATE 3 VMs + 3 views; sanctioned touches only; no backend; no migration.
- Slice 5 — completed 2026-09-17: antibiotics dictionary + editor + D9 attachment tab + D6 surfacing as hub tabs; CREATE 3 VMs + 3 views; hub touch only; no backend; no migration.
- Slice 4 — completed 2026-09-17: Analytes tab + editor/bands + Profiles tab + composition editor as hub tabs; CREATE 4 VMs + 4 views; hub touch only; no backend; no migration.
- Slice 3 — completed 2026-09-17: lab hub («المعمل» + U-11 Navigated fix) + catalog/groups/worklogs tabs + test editor (13 fields, ranges + permanent note, single-link mapping); CREATE 5 VMs + 5 views; no backend touch; no migration.
- Slice 2 — completed 2026-09-17: D1 LabPrintText tab in SystemSettingsView + early backup-path check + .bak guard + restore text + report colour preview; MODIFY only (2 VMs + 2 views); no migration.
- Slice 0 — completed 2026-09-17: shell «قفل المحطة»/About/exit-confirm/connection rules/login identity; files: CREATE UnlockViewModel + Views/Shell/{UnlockWindow,AboutWindow}(.xaml.cs); MODIFY ShellViewModel/MainWindow/LoginWindow/LoginViewModel/DI.

## Risks & Repository Constraints (living list — append during execution)

- **U-01 (Slice 0):** the P1 plan file («خطة التنفيذ النهائية…») is not present anywhere in the repo; `S-02.md` (self-contained, verbatim strings) is used as the binding spec. If a fact is missing from `S-02.md`, it is out of scope → stop and report.
- **U-02 (Slice 0):** VG-01 mentions "7 disabled buttons still disabled", but at baseline ALL 11 shell nav items are `IsEnabled=true` (fall-through `// Future`). P1 §1.4 (via S-02.md) orders no disabling → existing enabled states preserved unchanged; recorded, not reinterpreted.
- **U-03 (Slice 1):** P1 §2.4 validation numbers/messages (UserName 3–50, BreakDuration 1–480, quoted strings) diverge from the confirmed code validators (`Create/UpdateUserCommandValidator`: UserName `NotEmpty`+≤100, break `>0` when enabled, working-hours text). Code is binding → UI stays backend-driven (presenter surfaces code messages verbatim); no client-side rule invented.
- **U-04 (Slice 1):** plan quotes denial «…لهذا العمل — راجع…» with em-dash; the confirmed codebase string (presenter const + `AuthorizationBehavior` + 6 handler call sites) has no dash. Code binding → unchanged.
- **U-05 (Slice 2):** plan's LabPrintText limits (each text ≤500 «نص الطباعة تجاوز 500 حرف»; FontSizePt 6–72 «حجم الخط غير صالح») are absent from the confirmed `SaveLabPrintTextCommandValidator` (LabName req/≤200; FontSizePt 1–300). Code binding → UI backend-driven; the >500-char manual item is N/A-per-code (round-trip verified instead).
- **U-06 (Slice 2):** no header/footer colour properties exist in any settings DTO; the colour preview is a live LabName/font/mode sample with static RLS-inspired colours.
- **U-07 (Slice 2):** plan's «مسار النسخ غير صالح أو غير قابل للكتابة» is absent from code (service yields a friendly Unexpected message). Early `CheckBackupPathQuery` is wired as ordered; the code message is surfaced verbatim.
- **U-08 (Slice 3):** plan's M12 quotes («اختر مجموعة التحليل», «حدد نوع النتيجة», «السعر لا يمكن أن يكون سالباً», «نهاية مدى السن يجب أن تتجاوز بدايته», «الحد الأعلى يجب ألا يقل عن الأدنى», «يوجد تداخل مع مدى مرجعي آخر») diverge from / are absent in the confirmed validators (code messages e.g. «نوع النتيجة غير صالح.», «العمر الأقصى يجب ألا يقل عن العمر الأدنى.», no overlap rule, group optional). Code binding; mapping is a single nullable link in code → single-picker tab with session-known current + «هذا المكوّن مرتبط بالفعل» UI guard.
- **U-09 (Slice 3):** no duplicate-test-group-name backend rule exists → UI saves and surfaces the result; «يوجد مجموعة بهذا الاسم» N/A-per-code.
- **U-10 (Slice 3):** work-log name code message is «اسم مجموعة العمل مطلوب.» (not plan's «اسم السجل مطلوب») → code binding.
- **U-11 (Slice 3):** `ShellViewModel` never subscribes to `INavigationService.Navigated`, so navigated content never displays (baseline shell bug). Minimal fix (subscribe in ctor) required for the «المعمل» hub; also repairs المرضى/المستخدمون/الإعدادات display — intended navigation, justified.
- **U-12 (Slice 4):** plan's «المكوّن مضاف بالفعل» vs code «المادة التحليلية مرتبطة بالبروفايل بالفعل.» → plan quote as UI pre-guard, backend message as fallback. Plan's «النطاقات متداخلة أو بها فراغات» has no backend rule → bands editor surfaces backend/domain messages only. (Analyte/profile validator quotes verified to match code.)
- **U-13 (Slices 4–5):** `TestSummaryDto` carries no ResultKind/IsCultureType → Slice 4's specialized-test picker lists catalog tests with backend-enforced validity; Slice 5's culture picker filters client-side by resolving each summary via the existing `GetTestByIdQuery` on tab load (cached in VM) — existing surface only, no invented query, no per-test N+1 beyond this load.
- **U-14 (Slice 6):** plan's «اسم الجهة مطلوب»/3–150 + «رقم هاتف غير صالح»/digits≤20 diverge from code («اسم الجهة الخارجية مطلوب.»/≤200; Phone/Fax ≤30, no digits rule) → code binding; phone-verbatim manual item N/A-per-code.
- **U-15 (Slice 6):** code auto-fill is post-create only (`GenerateEntityIdCodeCommand(Id)` persists on an existing entity) → editor shows «—» until first save, then auto-generates once; no pre-save regenerate button can exist.
- **U-16 (Slice 6):** D3 orders «الحسابات» disabled until P5; baseline has it enabled-noop → that ONE button is disabled now (commented); U-02 preservation stands for all other buttons.
- **U-17 (Slice 7):** plan quotes (list/group name 3–100; item price «السعر مطلوب ولا يمكن أن يكون سالباً»; comment 1–500 «نص التعليق مطلوب»/«اختر التحليل») diverge from code (names ≤150 with code messages; price «السعر يجب أن يكون صفرًا أو أكثر.»; comment ≤1000 «نص التعليق مطلوب.») → code binding. Duplicate quotes have no backend basis (`Set*` upserts) → used as UI pre-guards only.

- Pinned commit `09c304c…` must equal `main` HEAD at start; if the repo has moved, STOP and report (plan is anchored to that commit).
- No UI test harness exists — manual verification is recorded, never fabricated; physical-printer/visual checks remain for the owner.
- `ShellViewModel.cs` / `MainWindow.xaml` / `Presentation/DependencyInjection.cs` are touched by multiple slices (0, 1, 3–7): each slice touches ONLY its own branch/template/registration — documented dependency touches, not scope creep.
- `PatientEditorViewModel.cs` is touched only by Slice 6 (picker buttons) — the single sanctioned cross-workstream modification.

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-17 | 0 | — | Memory file created | OK | — |
| 2026-09-17 | 0 | 1–9 | Slice 0 executed, VG-01 pass (build 0/0, tests 2081 green, zero-drift, grep gates, manual walk recorded) | OK | `72992ef` |
| 2026-09-17 | 1 | 1–10 | Slice 1 executed, VG-02 pass (build 0/0, tests 2088 green incl. 7 new, zero-drift, grep gates, manual walk recorded) | OK | `034a45d` |
| 2026-09-17 | 2 | 1–10 | Slice 2 executed, VG-03 pass (build 0/0, tests 2088 green, zero-drift, inspection gates, manual walk recorded) | OK | `60956c2` |
| 2026-09-17 | 3 | 1–10 | Slice 3 executed, VG-04 pass (build 0/0, tests 2088 green, zero-drift, grep gates, manual walk recorded) | OK | `52149f0` |
| 2026-09-17 | 4 | 1–10 | Slice 4 executed, VG-05 pass (build 0/0, tests 2088 green, zero-drift, inspection gates, manual walk recorded) | OK | `87b8fab` |
| 2026-09-17 | 5 | 1–10 | Slice 5 executed, VG-06 pass (build 0/0, tests 2088 green, zero-drift, inspection gates, manual walk recorded) | OK | `1d7671a` |
| 2026-09-17 | 6 | 1–10 | Slice 6 executed, VG-07 pass (build 0/0, tests 2088 green, zero-drift, inspection gates, manual walk recorded) | OK | `3abf5e4` |
| 2026-09-17 | 7 | 1–10 | Slice 7 executed, VG-08 pass (build 0/0, tests 2088 green, zero-drift, inspection gates, manual walk recorded) | OK | `6b6da28` |

## Stop Report (append only if a stop condition triggers)

(None — no stop condition has triggered.)
