# Loop-Engineering Execution Prompts — Top-Lab Modules 3, 4, 5, 6, 8, 11

This file contains **six ready-to-send, standalone, professional prompts** — one per module — addressed to the **local executing coding agent** that will carry out the loop-engineering work on the [Top-Lab](https://github.com/El-ogra/Top-Lab.git) repository.

**Strict execution order:** Prompt 1 (Module 3) → Prompt 2 (Module 4) → Prompt 3 (Module 5) → Prompt 4 (Module 6) → Prompt 5 (Module 8) → Prompt 6 (Module 11). **Do not begin any later prompt until the previous one has fully completed every slice in its module's plan and committed each slice locally.**

**Plan and memory files placement.** Each module's two input files must already be present in the project's `Docs` folder before the corresponding prompt is sent. The plan files are: `Docs/OpenCode/M-03.md`, `Docs/OpenCode/M-04.md`, `Docs/OpenCode/M-05.md`, `Docs/OpenCode/M-06.md`, `Docs/OpenCode/M-08.md`, `Docs/OpenCode/M-11.md`. The memory files produced by this deliverable are: `Docs/OpenCode/M-03-memory.md`, `Docs/OpenCode/M-04-memory.md`, `Docs/OpenCode/M-05-memory.md`, `Docs/OpenCode/M-06-memory.md`, `Docs/OpenCode/M-08-memory.md`, `Docs/OpenCode/M-11-memory.md`.

**Pre-conditions assumed (settled by the project's own requirements documentation and the project owner's written choices, per the FINAL combined plan):** Modules **M-02** (Patient Registration & Test Ordering) and **M-21** (Sample Collection & Separation) are implemented and fully merged BEFORE any of these six modules are executed. Their surface is inlined verbatim in the combined plan and into each consuming module's context section, so the executing agent does not need to read `M-02.md` or `M-21.md`.

---

## Prompt 1 — Module 3: Patient Billing & Account Settlement

**Module:** M-03 — Patient Billing & Account Settlement (backend only; no Presentation/UI content anywhere).
**Sequence position:** 1 of 6. **You must not begin this prompt until any prerequisite steps in the sequence are complete (this is the first prompt).** Begin slice M-03-S1 immediately against the current commit on the `main` branch.

You are the local executing coding agent. You will execute **Module 3 of the Top-Lab repository** using a strict, owner-authorized loop-engineering process.

**Input files (already placed in the project's `Docs` folder — read only these two):**
- `Docs/OpenCode/M-03.md` — the module's final, fully self-contained implementation plan.
- `Docs/OpenCode/M-03-memory.md` — this module's loop-engineering memory file.

**No other module's plan or memory file is needed or should be opened.** Do not open `M-04.md` / `M-04-memory.md` / any other module's files. If you find yourself referencing a fact not present in `M-03.md` or `M-03-memory.md`, that fact is out of scope; stop and report.

**Execution rules (mandatory):**

1. **Work strictly and only from the two files above.** Every fact about slice count, slice titles, files touched, and validation gates comes from `M-03.md`. The memory file's structure, gates, and 10-stage cycle come from `M-03-memory.md`.
2. **Execute slices strictly sequentially in the order defined in the memory file: S1 → S2 → S3 → S4.** No parallel slices, no skipping, no reordering.
3. **For every slice, follow this exact 10-stage cycle before moving to the next slice:**
   - **Stage 1 — Pre-Execution Verification:** Confirm the current build and full test suite are green before touching anything. Run `dotnet build TopLab.sln` and `dotnet test TopLab.sln` on the local Windows dev machine.
   - **Stage 2 — Deep Understanding:** Re-read the slice's requirements from the plan file in full (M-03.md §2.2/§2.3/§2.4/§2.5 + the §2.1 inlined context + the §2.6 frozen Arabic message table).
   - **Stage 3 — File Analysis:** Inspect every file the slice will touch or depend on, including the verified precedents cited in the plan (e.g. `PaymentOperation` configuration with `decimal(18,2)`/`tinyint`/Cascade FK/index on `PatientId`; existing `PaymentTests`; `User.DiscountLimitPercent` + `SetPolicy` guard; `Test.ReceiptName`; `ReceiptSettings.Currency`; `Result/Error`; M13/M14/M15 `DomainFailureTranslator` param-name matcher precedent).
   - **Stage 4 — Planning:** Write an explicit step-by-step implementation plan for this slice, encoded in the memory file's "### 10-Stage Progress" checklist.
   - **Stage 5 — Execution:** Implement the plan.
   - **Stage 6 — Post-Execution Verification:** Full solution build zero errors/zero warnings. Run `dotnet build TopLab.sln`.
   - **Stage 7 — Validation Gate:** Check this slice's specific Validation Gate (VG-01 / VG-02 / VG-03 / VG-04) from the memory file. Only a pass allows continuing. Apply the grep gates and assertions the plan requires (e.g. S3 `RecordCorrectionCommand` and `VoidPaymentOperationCommand` carry `IAuthorizedRequest` with `CASH_DISBURSE_DEPOSIT`; S1 guards raise `ArgumentException` with the exact `paramName` values; S2 deleted-user fallback to raw id string).
   - **Stage 8 — Documentation Update:** Update the memory file's slice section and checkboxes (Stages 1–10 → [x]).
   - **Stage 9 — Memory Status Update:** Update the "Current Status" section in the memory file.
   - **Stage 10 — Git Commit:** LOCAL commit only, on the current branch (`main`), never push, never branch. Commit message format: `[M-03] Slice N/4: <slice title> — loop-engineering`.
4. **Upon Stage 7 passing, immediately begin Stage 1 of the next slice with no pause and no request for human confirmation.** Do not stop between slices under any condition other than a Stop Rule trigger.
5. **Apply the Stop Rule exactly as defined in the memory file:** 5 consecutive identical failures of any kind (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) → STOP with a full stop report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do not silently retry indefinitely or skip the failing point. Append the stop report to the memory file's "## Stop Report" section.
6. **Stop only when (a) every slice in M-03's plan is fully completed and committed (all 4 slices done), or (b) a stop condition is triggered and the stop report is written.** No other stopping points are permitted.
7. **The ONLY normal stopping point (no report needed) is full completion of every slice in this module's plan.** After all 4 slices are committed locally, M-03 is done; emit a brief summary and stop.
8. **Git policy:** Local commit only on `main`, never create a new branch, never push to any remote. Commit message format: `[M-03] Slice N/4: <slice title> — loop-engineering`. Stage-10 is automatically authorized; do not pause for confirmation.
9. **Slice count and gates are encoded in the memory file.** Do not invent additional slices; do not collapse slices; do not skip a slice's Validation Gate.

**Module-specific reminders (from M-03.md):**
- 4 slices, each gated VG-01 → VG-02 → VG-03 → VG-04.
- All M-03 write commands are ungated **except** `RecordCorrectionCommand` and `VoidPaymentOperationCommand`, which are `IAuthorizedRequest` with `RequiredPermissionCode => "CASH_DISBURSE_DEPOSIT"` (FR-M17-004 item 11's documented scope explicitly includes patient accounting «محاسبة المرضى»; recorded in the close-out ADR with the honest scope-reading note).
- Discount-limit enforcement (settled — Data Model §13 BR-06 + Test Strategy §3.2/§7.2): when `DiscountAmount` is supplied, the handler loads the current `User` row and — unless `_currentUser.IsAbsolutePermission` — rejects with `Error.Validation("الخصم يتجاوز الحد المسموح به لهذا المستخدم.")` when `DiscountAmount > Amount * user.DiscountLimitPercent / 100m` (breach → `Result.Failure` of type **Validation**). Absolute-permission users exempt (stated pin documented in the close-out ADR).
- `SettleAccountInFullCommand` rejects `Error.Conflict("لا يوجد رصيد مستحق للتسوية.")` when balance ≤ 0 and creates a `FullSettlement` operation with `Amount = balance` (FR-M03-004: «خلاص» then «موافق» = paying the remaining amount in full).
- Void is idempotent in Domain; the handler returns the friendly `العملية ملغاة بالفعل.` Conflict on a double-call.
- No new migration is expected (zero-drift gate runs in S4; any drift triggers plan revision, not silent migration).
- No `PermissionConfiguration` change (the `CASH_DISBURSE_DEPOSIT` code is already seeded).
- No edit command ships — payment correction is void-and-reissue only (ADR-0017 + Coding Standards §7.4 binding); the close-out ADR carries the UI-Blueprint S-04 reconciliation note.
- Deleted `User` row → `ReceivedByUserName` falls back to the raw id string (stated rule, dedicated handler test).
- Diff is confined to `src/TopLab.Domain/Billing/**`, `src/TopLab.Application/Features/PatientBilling/**`, `tests/**`, `Docs/**`. No Presentation content anywhere.

Begin now at slice M-03-S1. Work slice-by-slice, commit-by-commit, gate-by-gate, until all 4 slices are complete or a stop condition triggers.

---

## Prompt 2 — Module 4: Results Entry & Result Lifecycle

**Module:** M-04 — Results Entry & Result Lifecycle (backend only; no Presentation/UI content anywhere).
**Sequence position:** 2 of 6. **You must not begin this prompt until Prompt 1 (Module 3) has fully completed all 4 of its slices and committed each one locally.** Begin slice M-04-S1 immediately against the current commit on the `main` branch, after M-03's code is present in the codebase.

You are the local executing coding agent. You will execute **Module 4 of the Top-Lab repository** using a strict, owner-authorized loop-engineering process.

**Input files (already placed in the project's `Docs` folder — read only these two):**
- `Docs/OpenCode/M-04.md` — the module's final, fully self-contained implementation plan.
- `Docs/OpenCode/M-04-memory.md` — this module's loop-engineering memory file.

**No other module's plan or memory file is needed or should be opened.** Do not open `M-03.md` / `M-03-memory.md` / any other module's files.

**Execution rules (mandatory):**

1. **Work strictly and only from the two files above.** Every fact about slice count, slice titles, files touched, and validation gates comes from `M-04.md`. The memory file's structure, gates, and 10-stage cycle come from `M-04-memory.md`.
2. **Execute slices strictly sequentially in the order defined in the memory file: S1 → S2 → S3 → S4.** No parallel slices, no skipping, no reordering.
3. **For every slice, follow this exact 10-stage cycle before moving to the next slice:**
   - **Stage 1 — Pre-Execution Verification:** Confirm the current build and full test suite are green before touching anything. Run `dotnet build TopLab.sln` and `dotnet test TopLab.sln` on the local Windows dev machine.
   - **Stage 2 — Deep Understanding:** Re-read the slice's requirements from the plan file in full (M-04.md §3.2/§3.3/§3.4/§3.5 + the §3.1 inlined context incl. the seven-state status model + the §3.6 frozen Arabic message table).
   - **Stage 3 — File Analysis:** Inspect every file the slice will touch or depend on, including the verified precedents cited in the plan (e.g. `PatientTest` full lifecycle shape + guard-free mutators; `ReferenceRange.Matches` + `CaptureSnapshot`; `ReferenceRangeSnapshot` record; `PatientStatusCalculator` `NotImplementedException` stub; `SystemSettings.AutoReviewAndComplete`; `User.BlockPrintOnRemainingBalance`; M12's `ReferenceRangeDto` shape; `CultureResult` 1:1 PK precedent; `Result/Error`; M17 re-issue idempotency precedent; M21 `MarkAllSamplesDrawnForPatient` bulk UX precedent).
   - **Stage 4 — Planning:** Write an explicit step-by-step implementation plan for this slice, encoded in the memory file's "### 10-Stage Progress" checklist.
   - **Stage 5 — Execution:** Implement the plan.
   - **Stage 6 — Post-Execution Verification:** Full solution build zero errors/zero warnings. Run `dotnet build TopLab.sln`.
   - **Stage 7 — Validation Gate:** Check this slice's specific Validation Gate (VG-01 / VG-02 / VG-03 / VG-04) from the memory file. Only a pass allows continuing. Apply the assertions the plan requires (e.g. S1 `PatientStatusCalculator` table-driven over S1–S7 incl. the binding worked example {1,3,4} → S2 and the S1/S2 "newly registered" boundary; S2 `ResultFlagComputer.SelectMatch` rule — sex-matched preferred, then narrowest age band, lowest id; S3 balance-block matrix with the worked example prices 100+50, extra charge 20, payment 80 with discount 10, voided payment 999 ⇒ Charged 170, Paid 90, Balance 80; S4 migration ↔ snapshot zero drift).
   - **Stage 8 — Documentation Update:** Update the memory file's slice section and checkboxes (Stages 1–10 → [x]).
   - **Stage 9 — Memory Status Update:** Update the "Current Status" section in the memory file.
   - **Stage 10 — Git Commit:** LOCAL commit only, on the current branch (`main`), never push, never branch. Commit message format: `[M-04] Slice N/4: <slice title> — loop-engineering`.
4. **Upon Stage 7 passing, immediately begin Stage 1 of the next slice with no pause and no request for human confirmation.** Do not stop between slices under any condition other than a Stop Rule trigger.
5. **Apply the Stop Rule exactly as defined in the memory file:** 5 consecutive identical failures of any kind → STOP with a full stop report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do not silently retry indefinitely or skip the failing point. Append the stop report to the memory file's "## Stop Report" section.
6. **Stop only when (a) every slice in M-04's plan is fully completed and committed (all 4 slices done), or (b) a stop condition is triggered and the stop report is written.** No other stopping points are permitted.
7. **The ONLY normal stopping point (no report needed) is full completion of every slice in this module's plan.** After all 4 slices are committed locally, M-04 is done; emit a brief summary and stop.
8. **Git policy:** Local commit only on `main`, never create a new branch, never push to any remote. Commit message format: `[M-04] Slice N/4: <slice title> — loop-engineering`. Stage-10 is automatically authorized; do not pause for confirmation.
9. **Slice count and gates are encoded in the memory file.** Do not invent additional slices; do not collapse slices; do not skip a slice's Validation Gate.

**Module-specific reminders (from M-04.md):**
- 4 slices, each gated VG-01 → VG-02 → VG-03 → VG-04.
- Lifecycle guard chain: `EnterResult` rejects when `IsReviewed` (`"النتيجة معتمدة؛ ألغِ الاعتماد أولاً."`); `ClearResult` rejects when reviewed/printed/delivered; `Unreview` rejects when printed/delivered; `MarkReviewed` requires `EnteredAtUtc` set; `MarkPrinted` requires reviewed + entered; `MarkDelivered` requires printed. `MarkEntered(int, DateTime)` is the dedicated entered-invariant mutator for profile/culture.
- **`PatientStatusCalculator` is delivered in full** in S1 (no stub, no waiver) implementing PRD §8.2/§8.3 + the S1/S2 "newly registered" micro-pin (`RegistrationDateUtc` falls on the current UTC day) + the binding worked example {1,3,4} → S2.
- **The reference-range snapshot storage is a dedicated child table `PatientTestReferenceRangeSnapshots`** keyed 1:1 by `PatientTestId` with Cascade FK — the module's **only** migration (`AddPatientTestReferenceRangeSnapshots`). No stateless intent methods on the aggregate.
- **Auto-compute of `ResultFlag` at entry is settled** (Data Model §6.1/§13 BR-05 + Test Strategy §7.2-M04); the **overlapping-range selection rule is settled by the owner**: most-specific range wins — sex-matched preferred over sex-null, then narrowest age band, then lowest id on a tie.
- **FR-M04-008 explicit user refresh** of frozen reference values: refresh command replaces the snapshot + recomputes flag; reviewed results rejected.
- **Auto review-and-completion is a system action** when `SystemSettings.AutoReviewAndComplete` is set (read row `Id == 1` directly per M22 precedent); no `REVIEW_RESULTS` precondition.
- **Print-time balance block** (settled — Data Model §13 BR-07 + Reporting §9 + FR-M09-003): `user.BlockPrintOnRemainingBalance && !_currentUser.IsAbsolutePermission && BalanceProbe.Balance(patientId) > 0` → `Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.")` (refused **before** printing). The `BLOCK_PRINT_ON_BALANCE` permission code is **not** used as a runtime gate — it is the grantable per-user item backing the flag.
- **No balance gate on delivery** (FR-M09-003 places the block before printing for delivery; delivery is the physical handover — BR-08).
- Bulk commands (`MarkAllPatientResultsReviewed`/`MarkPatientResultsPrinted`) use skip-and-report semantics; rows failing guards are skipped, not errors.
- No new `PermissionConfiguration` change (codes 2/3/4/6 are already seeded).
- The profile/culture modules (M-05/M-06) **rely upon** `MarkEntered`/`Unreview`/the guard behaviors — first-shipper rule applies if M-04 ships first.
- Diff is confined to `src/TopLab.Domain/Results/`, `src/TopLab.Domain/PatientStatus/`, `src/TopLab.Application/Features/ResultsEntry/**`, `Persistence/Configurations/` + `DbSets` + the one new migration, `tests/**`, `Docs/**`. No Presentation content anywhere.

Begin now at slice M-04-S1. Work slice-by-slice, commit-by-commit, gate-by-gate, until all 4 slices are complete or a stop condition triggers.

---

## Prompt 3 — Module 5: Specialized Profile Result Reports

**Module:** M-05 — Specialized Profile Result Reports (backend only; no Presentation/UI content anywhere).
**Sequence position:** 3 of 6. **You must not begin this prompt until Prompt 2 (Module 4) has fully completed all 4 of its slices and committed each one locally.** Begin slice M-05-S1 immediately against the current commit on the `main` branch, after M-04's code is present in the codebase.

You are the local executing coding agent. You will execute **Module 5 of the Top-Lab repository** using a strict, owner-authorized loop-engineering process.

**Input files (already placed in the project's `Docs` folder — read only these two):**
- `Docs/OpenCode/M-05.md` — the module's final, fully self-contained implementation plan.
- `Docs/OpenCode/M-05-memory.md` — this module's loop-engineering memory file.

**No other module's plan or memory file is needed or should be opened.** Do not open `M-04.md` / `M-04-memory.md` / any other module's files. The M-05 plan inlines the M-04 mutator signatures (`MarkEntered(int, DateTime)`, `Unreview`, and the guard behaviors) in §4.1 — the inlined content is the complete contract. First-shipper rule applies verbatim: if M-04 has not shipped, M-05's Slice 1 adds those mutators/guards with exactly the inlined signatures and records the collision in the close-out ADR.

**Execution rules (mandatory):**

1. **Work strictly and only from the two files above.** Every fact about slice count, slice titles, files touched, and validation gates comes from `M-05.md`. The memory file's structure, gates, and 10-stage cycle come from `M-05-memory.md`.
2. **Execute slices strictly sequentially in the order defined in the memory file: S1 → S2 → S3 → S4.** No parallel slices, no skipping, no reordering.
3. **For every slice, follow this exact 10-stage cycle before moving to the next slice:**
   - **Stage 1 — Pre-Execution Verification:** Confirm the current build and full test suite are green before touching anything. Run `dotnet build TopLab.sln` and `dotnet test TopLab.sln` on the local Windows dev machine.
   - **Stage 2 — Deep Understanding:** Re-read the slice's requirements from the plan file in full (M-05.md §4.2/§4.3/§4.4/§4.5 + the §4.1 inlined context + the §4.6 frozen Arabic message table).
   - **Stage 3 — File Analysis:** Inspect every file the slice will touch or depend on, including the verified precedents cited in the plan (e.g. `ProfileResultItem` create-only shape with the verbatim `ArgumentException("AnalyteName and ResultValue required.")` guard; `Test.ResultKind.SpecializedProfile = 1`; `Test.ReportName`; `IReportPrintingService` port with zero implementations; M02's `SetPhoneNumbers` replace-list pattern; M17 `Deactivate/Reactivate` idempotency precedent; the M-04 first-shipper contract for `MarkEntered`/`Unreview`; `Result/Error`; `IAuthorizedRequest` template).
   - **Stage 4 — Planning:** Write an explicit step-by-step implementation plan for this slice, encoded in the memory file's "### 10-Stage Progress" checklist.
   - **Stage 5 — Execution:** Implement the plan.
   - **Stage 6 — Post-Execution Verification:** Full solution build zero errors/zero warnings. Run `dotnet build TopLab.sln`.
   - **Stage 7 — Validation Gate:** Check this slice's specific Validation Gate (VG-01 / VG-02 / VG-03 / VG-04) from the memory file. Only a pass allows continuing. Apply the assertions the plan requires (e.g. S1 `ProfileResultItem.Update` reuses the verbatim `ArgumentException` contract; S3 replace-list resets old `IsVerified`/`IsPrinted` flags; print balance-block matrix with the worked example prices 100+50, extra charge 20, payment 80 with discount 10, voided payment 999 ⇒ Charged 170, Paid 90, Balance 80; S4 zero model drift proven).
   - **Stage 8 — Documentation Update:** Update the memory file's slice section and checkboxes (Stages 1–10 → [x]).
   - **Stage 9 — Memory Status Update:** Update the "Current Status" section in the memory file.
   - **Stage 10 — Git Commit:** LOCAL commit only, on the current branch (`main`), never push, never branch. Commit message format: `[M-05] Slice N/4: <slice title> — loop-engineering`.
4. **Upon Stage 7 passing, immediately begin Stage 1 of the next slice with no pause and no request for human confirmation.** Do not stop between slices under any condition other than a Stop Rule trigger.
5. **Apply the Stop Rule exactly as defined in the memory file:** 5 consecutive identical failures of any kind → STOP with a full stop report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do not silently retry indefinitely or skip the failing point. Append the stop report to the memory file's "## Stop Report" section.
6. **Stop only when (a) every slice in M-05's plan is fully completed and committed (all 4 slices done), or (b) a stop condition is triggered and the stop report is written.** No other stopping points are permitted.
7. **The ONLY normal stopping point (no report needed) is full completion of every slice in this module's plan.** After all 4 slices are committed locally, M-05 is done; emit a brief summary and stop.
8. **Git policy:** Local commit only on `main`, never create a new branch, never push to any remote. Commit message format: `[M-05] Slice N/4: <slice title> — loop-engineering`. Stage-10 is automatically authorized; do not pause for confirmation.
9. **Slice count and gates are encoded in the memory file.** Do not invent additional slices; do not collapse slices; do not skip a slice's Validation Gate.

**Module-specific reminders (from M-05.md):**
- 4 slices, each gated VG-01 → VG-02 → VG-03 → VG-04.
- All M-05 write commands are `IAuthorizedRequest` with `RequiredPermissionCode => "EDIT_RESULTS"` (`SaveProfileResultsCommand`), `"REVIEW_RESULTS"` (`VerifyProfileResultsCommand`/`UnverifyProfileResultsCommand`), or `"PRINT_RESULTS"` (`MarkProfileReportPrintedCommand`).
- **Replace-list maintenance semantics (settled by the owner):** `SaveProfileResultsCommand` removes all existing `ProfileResultItem` rows for the `PatientTestId` and re-creates from the submitted list, **not** preserving old `IsVerified`/`IsPrinted` flags (a re-edited grid is unverified by definition). `Comment` rides on `PatientTest.Notes`. Validator: `Items` non-empty (`"أدخل بندًا واحدًا على الأقل."`), duplicate `AnalyteName` (ordinal ignore-case, trimmed) → validation error (`"تكرار اسم المادة في نفس البروفايل."`).
- **Settled entered-invariant mechanism:** `VerifyProfileResultsCommand` calls `PatientTest.MarkEntered(_currentUser.UserId, _dateTime.UtcNow)` then `MarkReviewed(...)`; marks each item `MarkVerified()`. `AutoReviewAndComplete` is **not** consulted for profile verify (it collapses simple-result entry only).
- `VerifyProfileResultsCommand` requires ≥1 item (`Error.Conflict("لا توجد نتائج بروفايل للاعتماد.")`); already-reviewed parent → idempotent success (no-op).
- `UnverifyProfileResultsCommand` parent printed/delivered → `Error.Conflict("لا يمكن إلغاء اعتماد نتائج بروفايل مطبوع أو مسلم.")`; calls `PatientTest.Unreview()` + each item `Unverify()`.
- `MarkProfileReportPrintedCommand` applies the balance block: `user.BlockPrintOnRemainingBalance && !_currentUser.IsAbsolutePermission && BalanceProbe.Balance(patientId) > 0` → `Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.")`; marks each item `MarkPrinted()` + parent `MarkPrinted(...)`.
- **No printing implementation ships in this module** — `IReportPrintingService` stays unimplemented (settled: rendering is M07's per Reporting §13); the report deliverable is the `ProfileReportDto`.
- `NormalRangeText` is the matched normal-range display string per analyte (composed from the parent test's `ReferenceRange` rows — profiles in the verified schema carry no per-analyte range table).
- `AnalyteName` is immutable after create (analyte identity is the row's identity within a grid).
- First-shipper contingency for `MarkEntered`/`Unreview`/guard behaviors on `PatientTest`: if M-04 has not shipped, M-05's Slice 1 adds them per the inlined signatures and the close-out ADR records the outcome.
- No new migration is expected (zero-drift gate runs in S4; any drift triggers plan revision, not silent migration). No `PermissionConfiguration` change.
- Diff is confined to `src/TopLab.Domain/Results/ProfileResultItem.cs` (+ contingent `PatientTest.cs`), `src/TopLab.Application/Features/ProfileResults/**`, fake, `tests/**`, `Docs/**`. No Presentation content anywhere.

Begin now at slice M-05-S1. Work slice-by-slice, commit-by-commit, gate-by-gate, until all 4 slices are complete or a stop condition triggers.

---

## Prompt 4 — Module 6: Culture & Sensitivity Result Entry

**Module:** M-06 — Culture & Sensitivity Result Entry (backend only; no Presentation/UI content anywhere).
**Sequence position:** 4 of 6. **You must not begin this prompt until Prompt 3 (Module 5) has fully completed all 4 of its slices and committed each one locally.** Begin slice M-06-S1 immediately against the current commit on the `main` branch, after M-05's code is present in the codebase.

You are the local executing coding agent. You will execute **Module 6 of the Top-Lab repository** using a strict, owner-authorized loop-engineering process.

**Input files (already placed in the project's `Docs` folder — read only these two):**
- `Docs/OpenCode/M-06.md` — the module's final, fully self-contained implementation plan.
- `Docs/OpenCode/M-06-memory.md` — this module's loop-engineering memory file.

**No other module's plan or memory file is needed or should be opened.** Do not open `M-05.md` / `M-05-memory.md` / any other module's files. The M-06 plan inlines the M-04/M-05 mutator signatures (`MarkEntered(int, DateTime)`, `Unreview`, and the guard behaviors) in §5.1 — the inlined content is the complete contract. First-shipper rule applies verbatim: if M-04/M-05 have not shipped, M-06's Slice 1 adds those mutators/guards with exactly the inlined signatures and records the collision in the close-out ADR.

**Execution rules (mandatory):**

1. **Work strictly and only from the two files above.** Every fact about slice count, slice titles, files touched, and validation gates comes from `M-06.md`. The memory file's structure, gates, and 10-stage cycle come from `M-06-memory.md`.
2. **Execute slices strictly sequentially in the order defined in the memory file: S1 → S2 → S3 → S4.** No parallel slices, no skipping, no reordering.
3. **For every slice, follow this exact 10-stage cycle before moving to the next slice:**
   - **Stage 1 — Pre-Execution Verification:** Confirm the current build and full test suite are green before touching anything. Run `dotnet build TopLab.sln` and `dotnet test TopLab.sln` on the local Windows dev machine.
   - **Stage 2 — Deep Understanding:** Re-read the slice's requirements from the plan file in full (M-06.md §5.2/§5.3/§5.4/§5.5 + the §5.1 inlined context + the §5.6 frozen Arabic message table).
   - **Stage 3 — File Analysis:** Inspect every file the slice will touch or depend on, including the verified precedents cited in the plan (e.g. `CultureResult` 1:1 with `PatientTest` (Cascade FK, public constructor, no `Update`); `CultureAntibioticResult` (Cascade to `CultureResult` on shared key, Restrict to `Antibiotic`, identity PK, index); M15's `CultureAntibioticDisplay` (verbatim `ChildAgeThresholdYears = 12` + `IsDisplayable`); `AntibioticDto`/`AttachedAntibioticDto`; `MedicalConditionType` (`Category { Medication=0, Condition=1 }`); `Test.IsCultureType`/`Test.ResultKind.Culture`; `ExternalEntity.Normalize` whitespace→null precedent; the M-04 first-shipper contract for `MarkEntered`/`Unreview`; `Result/Error`; M02's `AddMedicalCondition`/`RemoveMedicalCondition` mutators + commands; M15's `DeleteAntibiotic` (blocks on recorded results per its message table); `IAuthorizedRequest` template).
   - **Stage 4 — Planning:** Write an explicit step-by-step implementation plan for this slice, encoded in the memory file's "### 10-Stage Progress" checklist.
   - **Stage 5 — Execution:** Implement the plan.
   - **Stage 6 — Post-Execution Verification:** Full solution build zero errors/zero warnings. Run `dotnet build TopLab.sln`.
   - **Stage 7 — Validation Gate:** Check this slice's specific Validation Gate (VG-01 / VG-02 / VG-03 / VG-04) from the memory file. Only a pass allows continuing. Apply the assertions the plan requires (e.g. S1 `MedicalConditionCategory.Pregnancy = 2` enum append produces no migration; `PregnancySignal` structural read on the enumerated value — never name-matching; S2 child <12 sees children-flagged rows, child ≥12 does not, boundary at exactly 12; S3 attached-only rule rejects non-attached antibiotics with `"المضاد الحيوي غير مرفق بهذه المزرعة."`; print balance matrix with the worked example prices 100+50, extra charge 20, payment 80 with discount 10, voided payment 999 ⇒ Charged 170, Paid 90, Balance 80).
   - **Stage 8 — Documentation Update:** Update the memory file's slice section and checkboxes (Stages 1–10 → [x]).
   - **Stage 9 — Memory Status Update:** Update the "Current Status" section in the memory file.
   - **Stage 10 — Git Commit:** LOCAL commit only, on the current branch (`main`), never push, never branch. Commit message format: `[M-06] Slice N/4: <slice title> — loop-engineering`.
4. **Upon Stage 7 passing, immediately begin Stage 1 of the next slice with no pause and no request for human confirmation.** Do not stop between slices under any condition other than a Stop Rule trigger.
5. **Apply the Stop Rule exactly as defined in the memory file:** 5 consecutive identical failures of any kind → STOP with a full stop report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do not silently retry indefinitely or skip the failing point. Append the stop report to the memory file's "## Stop Report" section.
6. **Stop only when (a) every slice in M-06's plan is fully completed and committed (all 4 slices done), or (b) a stop condition is triggered and the stop report is written.** No other stopping points are permitted.
7. **The ONLY normal stopping point (no report needed) is full completion of every slice in this module's plan.** After all 4 slices are committed locally, M-06 is done; emit a brief summary and stop.
8. **Git policy:** Local commit only on `main`, never create a new branch, never push to any remote. Commit message format: `[M-06] Slice N/4: <slice title> — loop-engineering`. Stage-10 is automatically authorized; do not pause for confirmation.
9. **Slice count and gates are encoded in the memory file.** Do not invent additional slices; do not collapse slices; do not skip a slice's Validation Gate.

**Module-specific reminders (from M-06.md):**
- 4 slices, each gated VG-01 → VG-02 → VG-03 → VG-04.
- All M-06 write commands are `IAuthorizedRequest` with `RequiredPermissionCode => "EDIT_RESULTS"` (`SaveCultureResultsCommand`), `"REVIEW_RESULTS"` (`VerifyCultureResultCommand`/`UnverifyCultureResultCommand`), or `"PRINT_RESULTS"` (`MarkCultureReportPrintedCommand`).
- **Attached-only write rule (settled):** every submitted `AntibioticId` in `SaveCultureResultsCommand` must exist in the test's `CultureAntibioticAttachment` set, else `Error.Conflict("المضاد الحيوي غير مرفق بهذه المزرعة.")` (free-text names on results are structurally rejected per Data Model §6.4).
- **Replace-list on sensitivities** (stated engineering pin, consistent with M02 `SetPhoneNumbers`): remove all existing for the `PatientTestId`, re-create via `CultureAntibioticResult.Create(CultureAntibioticResultId.Create(0), …)`. Sensitivities may be empty (negative culture with no panel is valid). Duplicate `AntibioticId` → validation error (`"تكرار المضاد الحيوي في نفس النتيجة."`).
- **Display filter is entry-time only** — recorded facts are never hidden (S-11 note + ADR-0031); the `PregnancySignal` Application helper uses a structural category read on the enumerated value (never name-matching).
- **Settled pregnancy storage (owner-settled):** `MedicalConditionCategory.Pregnancy = 2` (code-only enum append, no migration) + seeded «حمل» `MedicalConditionType` row with `Category = Pregnancy`. The seed may require a narrowly-scoped flagged migration if the seeding mechanism demands it.
- `CultureResult.Update` is a pure setter (trims each, whitespace-only → null, mirroring `ExternalEntity.Normalize` precedent); no new columns.
- `ChildAgeThresholdYears = 12` strictly under 12 (`patient.AgeUnit == AgeUnit.Year && patient.AgeValue < 12`).
- `VerifyCultureResultCommand` requires the `CultureResult` row to exist (`Error.Conflict("لا توجد نتيجة مزرعة للاعتماد.")`); calls `PatientTest.MarkEntered(...)` + `MarkReviewed(...)`; save once; idempotent on re-verify.
- `UnverifyCultureResultCommand` parent printed/delivered → `Error.Conflict("لا يمكن إلغاء اعتماد نتيجة مزرعة مطبوعة أو مسلمة.")`; calls `PatientTest.Unreview()`.
- `MarkCultureReportPrintedCommand` applies the balance block: `BlockPrintOnRemainingBalance && !IsAbsolutePermission && BalanceProbe.Balance(patientId) > 0` → `Error.Conflict("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.")`; parent `MarkPrinted(...)`; save once.
- First-shipper contingency for `MarkEntered`/`Unreview`/guard behaviors on `PatientTest`: if M-04/M-05 have not shipped, M-06's Slice 1 adds them per the inlined signatures and the close-out ADR records the outcome.
- This module does **not** manage the antibiotic catalog or attachments (M15 owns them, already shipped, writes gated on `EDIT_SYSTEM_SETTINGS` — this module only reads).
- No new migration beyond the flagged «حمل» catalog-seed migration if the seed mechanism requires one. No `PermissionConfiguration` change.
- Diff is confined to `src/TopLab.Domain/Results/CultureResult.cs`, `src/TopLab.Domain/Common/Enums/MedicalConditionCategory.cs` (+ contingent `PatientTest.cs`), the catalog configuration/seed, `src/TopLab.Application/Features/CultureResults/**`, fake, `tests/**`, `Docs/**`. No Presentation content anywhere.

Begin now at slice M-06-S1. Work slice-by-slice, commit-by-commit, gate-by-gate, until all 4 slices are complete or a stop condition triggers.

---

## Prompt 5 — Module 8: Patient Search, Lab ID & Visit History

**Module:** M-08 — Patient Search, Lab ID & Visit History (backend only; no Presentation/UI content anywhere).
**Sequence position:** 5 of 6. **You must not begin this prompt until Prompt 4 (Module 6) has fully completed all 4 of its slices and committed each one locally.** Begin slice M-08-S1 immediately against the current commit on the `main` branch, after M-06's code is present in the codebase.

You are the local executing coding agent. You will execute **Module 8 of the Top-Lab repository** using a strict, owner-authorized loop-engineering process.

**Input files (already placed in the project's `Docs` folder — read only these two):**
- `Docs/OpenCode/M-08.md` — the module's final, fully self-contained implementation plan.
- `Docs/OpenCode/M-08-memory.md` — this module's loop-engineering memory file.

**No other module's plan or memory file is needed or should be opened.** Do not open `M-06.md` / `M-06-memory.md` / any other module's files. The M-08 plan inlines the `PatientStatusCalculator` PRD §8.2/§8.3 contract in §6.1 — the inlined content is the complete specification. First-shipper rule applies verbatim: if `PatientStatusCalculator` still throws at execution time, M-08's Slice 1 implements the §8.2/§8.3 rule inside the calculator itself with the pinned semantics and the close-out ADR records the collision.

**Execution rules (mandatory):**

1. **Work strictly and only from the two files above.** Every fact about slice count, slice titles, files touched, and validation gates comes from `M-08.md`. The memory file's structure, gates, and 10-stage cycle come from `M-08-memory.md`.
2. **Execute slices strictly sequentially in the order defined in the memory file: S1 → S2 → S3.** No parallel slices, no skipping, no reordering.
3. **For every slice, follow this exact 10-stage cycle before moving to the next slice:**
   - **Stage 1 — Pre-Execution Verification:** Confirm the current build and full test suite are green before touching anything. Run `dotnet build TopLab.sln` and `dotnet test TopLab.sln` on the local Windows dev machine.
   - **Stage 2 — Deep Understanding:** Re-read the slice's requirements from the plan file in full (M-08.md §6.2/§6.3/§6.4 + the §6.1 inlined context incl. the seven-state status model and the LabId-grouping rule + the §6.5 frozen Arabic message table).
   - **Stage 3 — File Analysis:** Inspect every file this slice touches or depends on, including the verified precedents cited in the plan (e.g. `Patient` full shape incl. `LabId : StronglyTypedId<string>`; `Patient`'s XML comment "One row per visit (registration). LabId is shared grouping value across visits"; `ReportSettings.HistorySortMode` / `HistoryAutoDisplayEnabled` (FR-M22-015); `SystemSettings.EnablePatientNameSearchAssist` / `PrintLabIdInsteadOfPatientId` (FR-M22-008); M02's `SearchPatientsQuery` (registration-scoped surface — overlap documented); M02 pagination precedent (default 50 / max 500); `PatientStatusCalculator` (the implementation per Slice 1 contingency or M-04's); `Result/Error`; M22 direct settings read precedent).
   - **Stage 4 — Planning:** Write an explicit step-by-step implementation plan for this slice, encoded in the memory file's "### 10-Stage Progress" checklist.
   - **Stage 5 — Execution:** Implement the plan.
   - **Stage 6 — Post-Execution Verification:** Full solution build zero errors/zero warnings. Run `dotnet build TopLab.sln`.
   - **Stage 7 — Validation Gate:** Check this slice's specific Validation Gate (VG-01 / VG-02 / VG-03) from the memory file. Only a pass allows continuing. Apply the assertions the plan requires (e.g. S1 global search match channels incl. `EnablePatientNameSearchAssist` off disables name match; S1 per-visit status via the calculator; S1 balance from the inlined formula with the worked example prices 100+50, extra charge 20, payment 80 with discount 10, voided payment 999 ⇒ Charged 170, Paid 90, Balance 80; S2 M22 settings round-trip; S3 zero model drift + authorization-shape test asserts all four queries carry no `IAuthorizedRequest`).
   - **Stage 8 — Documentation Update:** Update the memory file's slice section and checkboxes (Stages 1–10 → [x]).
   - **Stage 9 — Memory Status Update:** Update the "Current Status" section in the memory file.
   - **Stage 10 — Git Commit:** LOCAL commit only, on the current branch (`main`), never push, never branch. Commit message format: `[M-08] Slice N/3: <slice title> — loop-engineering`.
4. **Upon Stage 7 passing, immediately begin Stage 1 of the next slice with no pause and no request for human confirmation.** Do not stop between slices under any condition other than a Stop Rule trigger.
5. **Apply the Stop Rule exactly as defined in the memory file:** 5 consecutive identical failures of any kind → STOP with a full stop report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do not silently retry indefinitely or skip the failing point. Append the stop report to the memory file's "## Stop Report" section.
6. **Stop only when (a) every slice in M-08's plan is fully completed and committed (all 3 slices done), or (b) a stop condition is triggered and the stop report is written.** No other stopping points are permitted.
7. **The ONLY normal stopping point (no report needed) is full completion of every slice in this module's plan.** After all 3 slices are committed locally, M-08 is done; emit a brief summary and stop.
8. **Git policy:** Local commit only on `main`, never create a new branch, never push to any remote. Commit message format: `[M-08] Slice N/3: <slice title> — loop-engineering`. Stage-10 is automatically authorized; do not pause for confirmation.
9. **Slice count and gates are encoded in the memory file.** Do not invent additional slices; do not collapse slices; do not skip a slice's Validation Gate.

**Module-specific reminders (from M-08.md):**
- 3 slices, each gated VG-01 → VG-02 → VG-03. (This module adds no write commands; S2 exists only as a settings-round-trip verification, not as a write surface.)
- All M-08 queries are **unauthorized plain** `IRequest<Result<...>>` (read policy per M12/M14 precedent) — asserted by an authorization-shape test in S3.
- `Text` empty → most recent registrations (the "open the screen" behavior); else case-insensitive substring on `FullName`, OR exact match on `LabId` or `NationalId` (trimmed), OR substring on any `PatientPhoneNumber.PhoneNumber` (BR-03: search by **any** stored number retrieves the record); always `!IsDeleted`.
- Honors `SystemSettings.EnablePatientNameSearchAssist` (FR-M08-003/FR-M22-008) — when false, name substring matching is disabled and only LabId/NationalId/phone matches run (the setting's literal reading; recorded in the ADR).
- Lab-ID lookup returns the **latest** visit + the `VisitHistoryDto` rollup (all non-deleted visits sharing the LabId — FR-M08-006); NotFound (`"لا يوجد مريض بهذا الكود."`) when no non-deleted row has the LabId.
- Visit history: when `LabId` is null, the history is the single visit; otherwise loads all non-deleted `Patient` rows sharing the `LabId`; per visit computes the test rollup (counts over that visit's `PatientTest` rows: `ResultsEntered` = count `EnteredAtUtc != null`; the four `All*` booleans are true when `TestCount > 0` and the respective count equals `TestCount` — an empty visit reports all-false); visits ordered by `RegistrationDateUtc desc` always; the `HistorySortMode`/`HistoryAutoDisplayEnabled` values are **echoed** in the DTO (the setting's real cross-patient effect applies to merged multi-patient views outside this query — documented in code and ADR).
- Per-visit `AggregateStatus` is computed by the settled `PatientStatusCalculator` over the visit's own tests + the visit's own balance (FR-M08-007 + PRD §8).
- This module does **NOT** call or import M02's handlers/DTOs; it re-queries the same tables with its own projections (deliberate overlap recorded in the ADR).
- First-shipper contingency: if `PatientStatusCalculator` still throws, M-08's Slice 1 implements the §8.2/§8.3 rule inside the calculator itself with the pinned semantics (S1/S2 "newly registered" micro-pin, binding worked example {1,3,4} → S2) and the close-out ADR records the collision.
- No new Domain changes, no migration, no `PermissionConfiguration` change.
- Diff is confined to `src/TopLab.Application/Features/PatientSearch/**`, fake (contingent), `tests/**`, `Docs/**` (+ contingent `src/TopLab.Domain/PatientStatus/`). No Presentation content anywhere.

Begin now at slice M-08-S1. Work slice-by-slice, commit-by-commit, gate-by-gate, until all 3 slices are complete or a stop condition triggers.

---

## Prompt 6 — Module 11: Work Sheets

**Module:** M-11 — Work Sheets (backend only; no Presentation/UI content anywhere).
**Sequence position:** 6 of 6. **You must not begin this prompt until Prompt 5 (Module 8) has fully completed all 3 of its slices and committed each one locally.** Begin slice M-11-S1 immediately against the current commit on the `main` branch, after M-08's code is present in the codebase.

You are the local executing coding agent. You will execute **Module 11 of the Top-Lab repository** using a strict, owner-authorized loop-engineering process.

**Input files (already placed in the project's `Docs` folder — read only these two):**
- `Docs/OpenCode/M-11.md` — the module's final, fully self-contained implementation plan.
- `Docs/OpenCode/M-11-memory.md` — this module's loop-engineering memory file.

**No other module's plan or memory file is needed or should be opened.** Do not open `M-08.md` / `M-08-memory.md` / any other module's files.

**Execution rules (mandatory):**

1. **Work strictly and only from the two files above.** Every fact about slice count, slice titles, files touched, and validation gates comes from `M-11.md`. The memory file's structure, gates, and 10-stage cycle come from `M-11-memory.md`.
2. **Execute slices strictly sequentially in the order defined in the memory file: S1 → S2.** No parallel slices, no skipping, no reordering.
3. **For every slice, follow this exact 10-stage cycle before moving to the next slice:**
   - **Stage 1 — Pre-Execution Verification:** Confirm the current build and full test suite are green before touching anything. Run `dotnet build TopLab.sln` and `dotnet test TopLab.sln` on the local Windows dev machine.
   - **Stage 2 — Deep Understanding:** Re-read the slice's requirements from the plan file in full (M-11.md §7.2/§7.3 + the §7.1 inlined context + the §7.4 frozen Arabic message table).
   - **Stage 3 — File Analysis:** Inspect every file this slice touches or depends on, including the verified precedents cited in the plan (e.g. `WorkGroupLog` + `WorkGroupLogItem` (composite PK `(WorkGroupLogId, TestId)`); M12's `GetWorkGroupLogsQuery` / `GetTestGroupsQuery` DTO shapes; `Test` (`Barcode` ≤50, `TestCode` ≤50 unique, `Name` ≤150, `ReceiptName` ≤150, `CompletionDurationMinutes`, `IsActive`, `TestGroupId?`); M21's draw state (`IsSampleDrawn`/`SampleDrawnAtUtc`); M02's `Patient.RegistrationDateUtc`; `IBarcodeService`/`IReportPrintingService` (ports; no implementations); M22 direct settings read precedent; `IAuthorizedRequest` template; `AuthorizationBehavior` (verbatim denial message); `Result/Error`).
   - **Stage 4 — Planning:** Write an explicit step-by-step implementation plan for this slice, encoded in the memory file's "### 10-Stage Progress" checklist.
   - **Stage 5 — Execution:** Implement the plan.
   - **Stage 6 — Post-Execution Verification:** Full solution build zero errors/zero warnings. Run `dotnet build TopLab.sln`.
   - **Stage 7 — Validation Gate:** Check this slice's specific Validation Gate (VG-01 / VG-02) from the memory file. Only a pass allows continuing. Apply the assertions the plan requires (e.g. S1 period defaulting incl. `From > To` → Validation + inclusive both ends; outside-lab tests excluded from the three worksheets but included in the test-count classification; S2 zero-drift; `WorkSheetsAuthorizationTests` all four queries on `PRINT_WORKSHEET` + standard denial).
   - **Stage 8 — Documentation Update:** Update the memory file's slice section and checkboxes (Stages 1–10 → [x]).
   - **Stage 9 — Memory Status Update:** Update the "Current Status" section in the memory file.
   - **Stage 10 — Git Commit:** LOCAL commit only, on the current branch (`main`), never push, never branch. Commit message format: `[M-11] Slice N/2: <slice title> — loop-engineering`.
4. **Upon Stage 7 passing, immediately begin Stage 1 of the next slice with no pause and no request for human confirmation.** Do not stop between slices under any condition other than a Stop Rule trigger.
5. **Apply the Stop Rule exactly as defined in the memory file:** 5 consecutive identical failures of any kind → STOP with a full stop report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do not silently retry indefinitely or skip the failing point. Append the stop report to the memory file's "## Stop Report" section.
6. **Stop only when (a) every slice in M-11's plan is fully completed and committed (all 2 slices done), or (b) a stop condition is triggered and the stop report is written.** No other stopping points are permitted.
7. **The ONLY normal stopping point (no report needed) is full completion of every slice in this module's plan.** After all 2 slices are committed locally, M-11 is done; emit a brief summary and stop.
8. **Git policy:** Local commit only on `main`, never create a new branch, never push to any remote. Commit message format: `[M-11] Slice N/2: <slice title> — loop-engineering`. Stage-10 is automatically authorized; do not pause for confirmation.
9. **Slice count and gates are encoded in the memory file.** Do not invent additional slices; do not collapse slices; do not skip a slice's Validation Gate.

**Module-specific reminders (from M-11.md):**
- 2 slices, each gated VG-01 → VG-02.
- All M-11 queries are `IAuthorizedRequest` with `RequiredPermissionCode => "PRINT_WORKSHEET"` (FR-M17-004 item 8; the classification lives inside the M11 screen per S-33, so it shares the worksheet grant — not `STATISTICS`).
- **Settled period semantics (settled by the owner):** `DateOnly? From` / `DateOnly? To`, both default to the current day in UTC when unspecified; `To ??= From` (settled by the owner); `From > To` → Validation (`"بداية الفترة يجب ألا تتجاوز نهايتها."`); bounds are inclusive on UTC calendar days.
- **Owner-settled worksheet-line identifier:** each line carries `PatientTestId` + `LabId` (the scannable identity pair); `Test.Barcode` is the test classifier. The pinned tube-barcode content for the future renderer (Reporting §7): Code 128 carrying `PatientId` or `LabId` per `PrintLabIdInsteadOfPatientId`, plus print date/time when `PrintDateTimeOnTubeBarcode` is set.
- **Outside-lab tests excluded from the three worksheets** (`!IsTakenOutsideLab`) **but included in the test-count classification** (the classification is a period activity count, so the bench-work exclusion does NOT apply — stated rule, one line in the handler XML comment).
- **The in-module test-count classification ships in S1** (FR-M11-004 — period test-count classification; same `PRINT_WORKSHEET` gate). Rows ordered by `Count desc`, tie-break `TestId`; `TotalCount` = sum.
- **No `MarkWorkSheetPrinted` audit command ships** (no print-tracking columns exist for worksheets; generation-gating on `PRINT_WORKSHEET` is the stated rule; recorded in the ADR).
- Echoes the three system settings (`PrintFileExternalBarcode`, `PrintDateTimeOnTubeBarcode`, `PrintLabIdInsteadOfPatientId`) for the future renderer; missing settings row → `Error.Unexpected("سجل الإعدادات العامة مفقود.")`.
- UTC-day caveat: a 01:00 registration in Egypt (UTC+2/+3) appears on "yesterday's" sheet; a lab-timezone setting is flagged as future work (no such setting exists in the verified `SystemSettings`).
- No new Domain change, no new migration, no `PermissionConfiguration` change.
- The worksheet deliverable is a DTO; `IBarcodeService`/`IReportPrintingService` stay unimplemented (Reporting §13: rendering is M07's).
- Diff is confined to `src/TopLab.Application/Features/WorkSheets/**`, `tests/**`, `Docs/**`. No Presentation content anywhere.

Begin now at slice M-11-S1. Work slice-by-slice, commit-by-commit, gate-by-gate, until all 2 slices are complete or a stop condition triggers.

---

*End of prompts — 6 prompts, one per module, ordered 3 → 4 → 5 → 6 → 8 → 11. Each prompt is standalone and ready to send to the local executing coding agent.*
