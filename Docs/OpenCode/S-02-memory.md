# Loop Engineering — Memory File

- **Module:** P1 UI Pass — Shell & Eight Reference-Module Screens (S-02 — cross-module UI workstream, post-S-01)
- **Module Number:** S-02
- **Source Plan:** Docs/OpenCode/S-02.md (execution slices) + «خطة التنفيذ النهائية للواجهات والنوافذ الرسوميه — التمريرة P1» (authoritative requirements)
- **Date Created:** 2026-09-17
- **Total Slices:** 8
- **Current Slice:** 2 — Next (Stage 1 on resume)
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
| 2 | SystemAndPrintSettings (M22) completion + D1 LabPrintText tab | [ ] Not started | VG-03 |
| 3 | TestCatalogAndReferenceRanges (M12) + lab hub activation | [ ] Not started | VG-04 |
| 4 | AnalyteProfiles (Analytes + Bands + Profiles tabs) | [ ] Not started | VG-05 |
| 5 | CultureAndAntibiotics (M15) dictionary + D9 attachment + D6 delete block | [ ] Not started | VG-06 |
| 6 | ExternalEntities (M14) list/editor/picker + D3 routing | [ ] Not started | VG-07 |
| 7 | PriceListsCommentsAndCustomGroups (M13) three tabs | [ ] Not started | VG-08 |

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

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; record counts.
- [ ] **Stage 2 — Deep Understanding:** re-read S-02.md §3 Slice 2 + P1 §3.3–§3.4.4, D1.
- [ ] **Stage 3 — File Analysis:** `SettingsDtos.cs` (authoritative property source), `LabPrintTextDto`, `JsonLabPrintTextStore`, six settings VMs/Views, `CheckBackupPathQuery`, `BackupDatabaseNowCommand`, `RestoreDatabaseCommand`, `PrinterOutputType` enum, system-printer enumeration point.
- [ ] **Stage 4 — Planning:** step-by-step plan here.
- [ ] **Stage 5 — Execution:** implement.
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-03 with evidence.
- [ ] **Stage 8 — Documentation Update.**
- [ ] **Stage 9 — Memory Status Update.**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 2/8: SystemAndPrintSettings completion + LabPrintText tab (D1) — loop-engineering`.

---

## Slice 3: TestCatalogAndReferenceRanges (M12) + lab hub activation

- **Goal:** Enable «المعمل» → lab hub (catalog/groups/work-logs tabs); catalog screen with counter + critical empty state; test editor (13 verbatim fields); reference-range editor with the permanent no-retroactive-effect note; groups screen; Test↔Analyte mapping tab (existing `GetAnalyteDefinitionsQuery`); work-group-logs (no delete).
- **Touches:** `ViewModels/Lab/` + `Views/Lab/` (create: hub, catalog, test editor dialog, reference-range tab, groups, mapping tab, work-group-logs); `ShellViewModel.cs`, `MainWindow.xaml`, DI (modify/WIRE).
- **Validation Gate:** VG-04.

### 10-Stage Progress (Slice 3)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; record counts.
- [ ] **Stage 2 — Deep Understanding:** re-read S-02.md §3 Slice 3 + P1 §4.3–§4.4.6, §4.6/§4.8 (Uncertain items resolved from code, recorded).
- [ ] **Stage 3 — File Analysis:** `TestCatalogDtos.cs`, `CreateTestCommandValidator.cs` (verbatim limits/messages), `ReferenceRange.cs` (`MaxCommentLength`), `ResultKind`/`AgeUnit`/`Sex` enums, the 16 commands + 5 queries, `GetAnalyteDefinitionsQuery`, S-01 navigation-wiring precedent (DI + DataTemplate + branch).
- [ ] **Stage 4 — Planning:** step-by-step plan here.
- [ ] **Stage 5 — Execution:** implement.
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-04 with evidence.
- [ ] **Stage 8 — Documentation Update.**
- [ ] **Stage 9 — Memory Status Update.**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 3/8: Test catalog + reference ranges + lab hub — loop-engineering`.

---

## Slice 4: AnalyteProfiles (Analytes + Bands + Profiles tabs)

- **Goal:** Analytes list/editor (no unit field; deactivate only), Bands editor (bulk save, line errors, from=prev-to default), Profiles list + composition editor (`Name`, `SpecializedTestId` picker, `FixedPrice`, components add/remove); missing Reactivate/Delete/DeleteProfile preserved as open gap.
- **Touches:** new Analyte/Profile VMs/Views/dialogs (create); `LabHubViewModel`/`LabHubView` (modify — add tabs); DI/DataTemplates (WIRE).
- **Validation Gate:** VG-05.

### 10-Stage Progress (Slice 4)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; record counts.
- [ ] **Stage 2 — Deep Understanding:** re-read S-02.md §3 Slice 4 + P1 §6.3–§6.4.4, §6.6.
- [ ] **Stage 3 — File Analysis:** `AnalyteProfileDtos.cs` (`AnalyteBandDto` columns resolved here — record them), the 3 validators (verbatim messages), `Analyte.cs`/`Profile.cs`, `SaveAnalyteReferenceRangeCommandValidator`, `SearchTestCatalogQuery` (profile picker source).
- [ ] **Stage 4 — Planning:** step-by-step plan here.
- [ ] **Stage 5 — Execution:** implement.
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-05 with evidence.
- [ ] **Stage 8 — Documentation Update.**
- [ ] **Stage 9 — Memory Status Update.**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 4/8: Analytes + bands + profiles tabs — loop-engineering`.

---

## Slice 5: CultureAndAntibiotics (M15) dictionary + D9 attachment + D6 delete block

- **Goal:** Antibiotic dictionary (3 confirmed columns) + editor (`Name`, two flags only); D6 blocked-delete message; D9 immediate attachment screen (culture picker filtered from catalog, two lists, live counter, reload-after-op).
- **Touches:** new dictionary/editor/attachment VMs/Views (create); `LabHubViewModel`/`LabHubView` (modify — add tabs); DI/DataTemplates (WIRE).
- **Validation Gate:** VG-06.

### 10-Stage Progress (Slice 5)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; record counts.
- [ ] **Stage 2 — Deep Understanding:** re-read S-02.md §3 Slice 5 + P1 §7.3–§7.4.3, D6/D9.
- [ ] **Stage 3 — File Analysis:** `AntibioticDtos.cs`, `CreateAntibioticCommandValidator`, `CultureAntibioticDisplay`, `DomainFailureTranslator`, Attach/Detach signatures `(int TestId, int AntibioticId)`, `SearchTestCatalogQuery` + `ResultKind`/`IsCultureType` filter basis.
- [ ] **Stage 4 — Planning:** step-by-step plan here.
- [ ] **Stage 5 — Execution:** implement.
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-06 with evidence.
- [ ] **Stage 8 — Documentation Update.**
- [ ] **Stage 9 — Memory Status Update.**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 5/8: Antibiotics dictionary + culture attachment (D6/D9) — loop-engineering`.

---

## Slice 6: ExternalEntities (M14) list/editor/picker + D3 routing

- **Goal:** List (3-value type filter), editor (auto-generated read-only code, read-only type on edit), delete confirmation naming entity, reusable Picker wired into the patient screen's doctor/referral fields; D3 temporary tagged Settings-dashboard route; «الحسابات» stays disabled.
- **Touches:** new list/editor/picker VMs/Views (create); `PatientEditorViewModel.cs` + view (modify — picker buttons only, documented dependency); `SettingsDashboardViewModel`/View (modify — temporary tagged route); DI/DataTemplates (WIRE).
- **Validation Gate:** VG-07.

### 10-Stage Progress (Slice 6)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; record counts.
- [ ] **Stage 2 — Deep Understanding:** re-read S-02.md §3 Slice 6 + P1 §5.3–§5.4.3, D3 (§5.6).
- [ ] **Stage 3 — File Analysis:** `ExternalEntityDtos.cs`, `EntityType.cs` (3 values), `GenerateEntityIdCodeCommand` + `SecureEntityIdCodeGenerator`, `DomainFailureTranslator`, `PatientEditorViewModel` `TreatingDoctor`/`ReferralEntity` (IdText + Name) fields, `IDialogService` picker-hosting pattern.
- [ ] **Stage 4 — Planning:** step-by-step plan here.
- [ ] **Stage 5 — Execution:** implement.
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-07 with evidence.
- [ ] **Stage 8 — Documentation Update.**
- [ ] **Stage 9 — Memory Status Update.**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 6/8: External entities screens + picker + D3 routing — loop-engineering`.

---

## Slice 7: PriceListsCommentsAndCustomGroups (M13) three tabs

- **Goal:** Price-lists master-detail (row-by-row price save + «محفوظ» badge; NO print control), test comments (multiple per test allowed), custom groups master-detail; hub tab set complete.
- **Touches:** three tab VMs/Views + editors (create); `LabHubViewModel`/`LabHubView` (modify — final tabs); DI/DataTemplates (WIRE).
- **Validation Gate:** VG-08.

### 10-Stage Progress (Slice 7)

- [ ] **Stage 1 — Pre-Execution Verification:** build 0/0; suite green; record counts.
- [ ] **Stage 2 — Deep Understanding:** re-read S-02.md §3 Slice 7 + P1 §8.3–§8.4.3, §8.6.
- [ ] **Stage 3 — File Analysis:** `PriceListDtos`/`TestCommentDtos`/`CustomGroupDtos`, the 13 commands + 5 queries, catalog picker reuse from Slice 3, absence of any print command for price lists (grep-confirmed), uniqueness check for multiple comments (recorded).
- [ ] **Stage 4 — Planning:** step-by-step plan here.
- [ ] **Stage 5 — Execution:** implement.
- [ ] **Stage 6 — Post-Execution Verification:** build 0/0.
- [ ] **Stage 7 — Validation Gate:** VG-08 with evidence.
- [ ] **Stage 8 — Documentation Update.**
- [ ] **Stage 9 — Memory Status Update.**
- [ ] **Stage 10 — Git Commit (authorized local):** `[S-02] Slice 7/8: Price lists + test comments + custom groups — loop-engineering`.

---

## Current Status

- Overall: 2/8 slices done
- Slice 0 — AccessAndNavigation completion: [x] Done (VG-01 pass, committed)
- Slice 1 — UsersAndPermissions (M17) completion: [x] Done (VG-02 pass, committed)
- Slice 1 — UsersAndPermissions (M17) completion: [ ] Not started
- Slice 2 — SystemAndPrintSettings (M22) completion: [ ] Not started
- Slice 3 — TestCatalogAndReferenceRanges (M12) + lab hub: [ ] Not started
- Slice 4 — AnalyteProfiles: [ ] Not started
- Slice 5 — CultureAndAntibiotics (M15): [ ] Not started
- Slice 6 — ExternalEntities (M14): [ ] Not started
- Slice 7 — PriceListsCommentsAndCustomGroups (M13): [ ] Not started
- Migration count: **exactly 0** expected (zero-drift gate on every slice)
- New backend artifacts expected: **exactly 1** (D7 ChangeOwnPassword command trio, Slice 1)
- Next action: begin Slice 2, Stage 1 (build 0/0 + suite green after Slice 1).
- Slice 0 — completed 2026-09-17: shell «قفل المحطة»/About/exit-confirm/connection rules/login identity; files: CREATE UnlockViewModel + Views/Shell/{UnlockWindow,AboutWindow}(.xaml.cs); MODIFY ShellViewModel/MainWindow/LoginWindow/LoginViewModel/DI.

## Risks & Repository Constraints (living list — append during execution)

- **U-01 (Slice 0):** the P1 plan file («خطة التنفيذ النهائية…») is not present anywhere in the repo; `S-02.md` (self-contained, verbatim strings) is used as the binding spec. If a fact is missing from `S-02.md`, it is out of scope → stop and report.
- **U-02 (Slice 0):** VG-01 mentions "7 disabled buttons still disabled", but at baseline ALL 11 shell nav items are `IsEnabled=true` (fall-through `// Future`). P1 §1.4 (via S-02.md) orders no disabling → existing enabled states preserved unchanged; recorded, not reinterpreted.
- **U-03 (Slice 1):** P1 §2.4 validation numbers/messages (UserName 3–50, BreakDuration 1–480, quoted strings) diverge from the confirmed code validators (`Create/UpdateUserCommandValidator`: UserName `NotEmpty`+≤100, break `>0` when enabled, working-hours text). Code is binding → UI stays backend-driven (presenter surfaces code messages verbatim); no client-side rule invented.
- **U-04 (Slice 1):** plan quotes denial «…لهذا العمل — راجع…» with em-dash; the confirmed codebase string (presenter const + `AuthorizationBehavior` + 6 handler call sites) has no dash. Code binding → unchanged.

- Pinned commit `09c304c…` must equal `main` HEAD at start; if the repo has moved, STOP and report (plan is anchored to that commit).
- No UI test harness exists — manual verification is recorded, never fabricated; physical-printer/visual checks remain for the owner.
- `ShellViewModel.cs` / `MainWindow.xaml` / `Presentation/DependencyInjection.cs` are touched by multiple slices (0, 1, 3–7): each slice touches ONLY its own branch/template/registration — documented dependency touches, not scope creep.
- `PatientEditorViewModel.cs` is touched only by Slice 6 (picker buttons) — the single sanctioned cross-workstream modification.

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-17 | 0 | — | Memory file created | OK | — |
| 2026-09-17 | 0 | 1–9 | Slice 0 executed, VG-01 pass (build 0/0, tests 2081 green, zero-drift, grep gates, manual walk recorded) | OK | `72992ef` |
| 2026-09-17 | 1 | 1–9 | Slice 1 executed, VG-02 pass (build 0/0, tests 2088 green incl. 7 new, zero-drift, grep gates, manual walk recorded) | OK | pending Stage 10 |

## Stop Report (append only if a stop condition triggers)

(None — no stop condition has triggered.)
