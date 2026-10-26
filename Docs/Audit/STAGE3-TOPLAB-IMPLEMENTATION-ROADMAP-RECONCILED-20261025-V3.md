# Stage 3 — Top-Lab Dependency-Ordered Implementation Roadmap (V3)

**Audit date:** 2026-10-25
**Audited revision:** `efc909c688ccd3375b6bf3bc15af617f3739c647` (confirmed as HEAD)
**Input:** `STAGE2-TOPLAB-REFERENCE-COMPARISON-INDEPENDENTLY-VERIFIED-20261025-V3.md` (the independently verified gap register, V3)
**Audit type:** Read-only verification of the prior report. **No stage is implemented by this document.**

> This document enumerates the 30 confirmed in-scope gaps from §7 of the companion report (V3), in a dependency-ordered stage structure, with prerequisites, scope, deliverables, acceptance criteria and traceability. It is a planning artifact, not a build plan. Future implementation must re-check each acceptance criterion against the code as it stands at the time of implementation.

---

## 1. Stage summary

| Stage | Title | Gaps | Dependencies | Schema/data changes |
|---|---|---|---|---|
| **R1** | Patient registration, result entry, delivery and account action bands | GAP-04, GAP-05, GAP-10, GAP-11, GAP-12, GAP-13, GAP-14, GAP-22, GAP-30 | — | Required for GAP-13/14 only (account model) |
| **R2** | Per-patient print-result password and its enforcement | GAP-01, GAP-02, GAP-03 | **R1** | Required (per-patient password columns + receipt projection) |
| **R3** | Patient search criteria and the search action bar | GAP-06, GAP-07, GAP-08, GAP-09 | — | Not indicated |
| **R4** | Main-window period list and the work-sheet module | GAP-15, GAP-17 | **R1** | Not indicated |
| **R5** | Reference-data editing: undo, group membership, deletion semantics and history | GAP-23, GAP-24, GAP-25, GAP-26, GAP-27, GAP-28 | — | Required only for GAP-26 (owner decision OD-2) |
| **R6** | Utilities, statistics, auditing, sent-out samples, sample withdrawal and settings | GAP-16, GAP-18, GAP-19, GAP-20, GAP-21, GAP-29 | — | Required for GAP-19 (deletion path, OD-4), GAP-20 (always-to, OD-6), GAP-29 (colour/layout persistence) |

---

## 2. Roadmap-wide constraints

1. **Offline operation is a hard requirement.** No stage may introduce an online service or any internet-dependent runtime behaviour. Every proposed change is local: local hashing and comparison, local file pickers, local PDF writers, local queries. The existing `QuestPDF` printing stack and the local SQL Server / file database deployment are unchanged.
2. **Existing behaviour is preserved unless the canonical reference establishes a required change.** Where Top-Lab's behaviour differs from the reference but the reference does not require the difference to change (for example the soft-delete of tests and groups), the difference is raised as a gap with the rationale stated, and the decision is taken explicitly rather than by omission.
3. **No stage is implemented by this audit.** The stages are proposals only. Each acceptance criterion in §3–§8 must be re-checked against the code at implementation time.
4. **Migration assessments distinguish confirmed schema needs from behaviour.** Only the account-model work (GAP-13, GAP-14), the per-patient password (GAP-02, GAP-03), the audit-period deletion (GAP-19, conditional on OD-2/OD-4), the always-to destination (GAP-20, conditional on OD-6) and the settings values (GAP-29) are assessed as requiring a persisted change; everything else is behaviour over data that already exists.
5. **No roadmap stage is implemented from this document alone.** The acceptance criteria in §3–§8 must be re-checked against the code as it stands at the time of implementation.
6. **Owner decisions do not block stages unless the decisions table says so.** Where a decision blocks specific gaps, the stage's readiness is READY-WITH-PARTIAL-SCOPE and the blocked gaps are listed with their decision ID.

---

## 3. Stage R1 — Patient registration, result entry, delivery and account action bands

**Associated confirmed gaps:** GAP-04, GAP-05, GAP-10, GAP-11, GAP-12, GAP-13, GAP-14, GAP-22, GAP-30
**Canonical reference identifiers covered:** `1.2.2.4.1`, `1.2.2.5.1.1`, `1.2.2.5.1.2`, `1.2.2.5.1.3`, `1.2.2.5.1.4`, `1.2.2.5.2`, `1.2.2.6.6`, `1.2.2.6.7`, `1.2.2.6.8`, `1.2.2.6.10`, `1.2.2.6.15`, `1.2.2.6.16`, `1.3.1.2`, `1.3.1.3`, `1.3.1.4.1`, `1.3.1.4.2`, `1.3.2.5`, `1.3.2.7`, `1.4.1.4.3`, `1.4.1.4.4`, `1.4.1.4.5`, `1.4.1.4.6`, `1.4.1.4.7`, `1.4.1.4.8`, `1.4.1.4.9`, `1.4.1.4.10`, `1.4.1.4.11`, `1.6.1.3.2`, `1.6.1.7`, `1.22.12.2`, `1.22.12.3`, `1.22.12.4`, `1.22.12.5`
**Gap count:** 9

### 3.1 Scope
Restore the reference's action bands and operating rules on the screens a user works in every day: the patient registration screen, the three result-entry screens, the delivery screen and the patient-account screen. Give the test catalogue its filter bar and its two menus; give the delivery screen its eight missing actions (excluding notes, which is GAP-04) and its print/preview/copy/patient-data/combined-report/medical-history/empty-report/envelope-print actions; give the account panel its refund, clear-account, percentage-discount, surcharge and Enter-key rules; render patient notes on the profile and culture entry screens and make notes reachable from the delivery workflow; and give the single-test result screen its print, preview, review and main-result actions.

### 3.2 Prerequisites
- R0: The audited commit `efc909c` is the branch tip. No prerequisite stage. R1 is the natural starting point.
- R1 owns the three explicitly in-scope areas that live on the registration, result-entry and delivery screens: notes rendering, action band and navigation, and financial/account panel.

### 3.3 Deliverables
1. **Patient editor action band** — envelope print, follow-up, patient-window (back-to-patient-list) and lab-results actions bound and reachable without leaving the open registration.
2. **Test catalogue** — filter field, mutually exclusive TM/TG/CG mode options, complete-test menu action, and test-group menu action.
3. **Delivery screen** — patient account, refresh, print, preview, copy, notes, patient data, combined report, medical history, empty report and envelope print actions; the delivery list must be printable and previewable.
4. **Account panel** — refund operation kind, clear-account action, percentage discount field, surcharge (`+`-prefix) convention, and Enter-key account-operation rule; the balance formula must reconcile against the reference's field set (paid-previously, total-after-discount, total-collected, remainder-for-customer, remainder-for-laboratory).
5. **Result-entry screens** — profile and culture screens render patient notes; the single-test result screen exposes print, preview, review and main-result actions.
6. **Notes reachability** — the delivery workflow exposes the patient note on the delivery entry and the handover screen, and the note action opens the stored patient note.

### 3.4 Downstream impact
- The account-panel work changes the balance formula in `PatientAccountCalculator`, which the concurrent-settlement behaviour already depends on. The settlement transaction's serializability must not be weakened, and the new discount and refund semantics must be settled against the reference's field definitions before the formula is changed.
- No schema change is implied for the action-band, filter, menu, notes or single-test-screen work. The account-panel schema change (GAP-13/14) is assessed in Stage 2 §9.6.

### 3.5 Acceptance criteria (per gap, derived from the canonical requirement)

**GAP-04 (MISSING) — Patient notes reachable in result-delivery workflow** (ref: `1.4.1.4.6`)
- Precondition: a patient with a non-empty `Notes` value exists.
- Action: open the delivery entry for that patient; open the handover screen.
- Observable result: the note is visible on both screens; an action opens the stored patient note.
- Validation: the note action does not modify the note; the note round-trips unchanged.
- Persistence: no schema change (Patient.Notes already exists).
- Security/access: note visible only to users with delivery permission.
- Regression: existing delivery workflow tests pass.

**GAP-05 (PARTIAL) — Patient notes on profile and culture result-entry screens** (ref: `1.2.2.4.1`, `1.6.1.7`)
- Precondition: a patient with a non-empty `Notes` value exists.
- Action: open ProfileEntryView and CultureEntryView for that patient.
- Observable result: the note renders on both screens (as it already does on SimpleResultEntryView).
- Validation: note is read-only on these screens; no edit path.
- Persistence: no schema change.
- Security/access: none beyond existing screen access.
- Regression: existing result-entry tests pass.

**GAP-10 (MISSING) — Patient editor action band** (ref: `1.2.2.6.8`, `1.2.2.6.10`, `1.2.2.6.15`, `1.2.2.6.16`)
- Precondition: patient editor open with a loaded patient.
- Action: click envelope print, follow-up, patient-window, lab-results.
- Observable result: each action performs its function without leaving the open registration.
- Validation: actions are disabled when no patient is loaded.
- Persistence: no schema change.
- Security/access: each action respects its existing permission.
- Regression: existing patient editor tests pass.

**GAP-11 (MISSING) — Test-selection filter bar** (ref: `1.2.2.5.1.1`–`1.2.2.5.1.4`, `1.2.2.5.2`)
- Precondition: test catalogue open.
- Action: use filter field, TM/TG/CG mode options, complete-test menu, test-group menu.
- Observable result: filter narrows the list; mode options are mutually exclusive; menus open.
- Validation: filter with no match shows empty list; invalid mode is rejected.
- Persistence: no schema change.
- Security/access: none beyond existing catalogue access.
- Regression: existing catalogue tests pass.

**GAP-12 (MISSING) — Delivery screen action band** (ref: `1.4.1.4.3`–`1.4.1.4.11`)
- Precondition: delivery screen open with a loaded delivery.
- Action: click patient account, refresh, print, preview, copy, notes, patient data, combined report, medical history, empty report, envelope print.
- Observable result: each action performs its function.
- Validation: actions disabled when no delivery loaded.
- Persistence: no schema change.
- Security/access: each action respects its existing permission.
- Regression: existing delivery tests pass.

**GAP-13 (MISSING) — Refund and clear-account operations** (ref: `1.3.2.5`, `1.3.2.7`)
- Precondition: patient account open with at least one operation.
- Action: record a refund; clear the account.
- Observable result: refund appears as a new operation; clear removes all operations (or marks them cleared, per OD-2).
- Validation: refund amount > 0; clear requires confirmation.
- Persistence: REQUIRED — `OperationType.Refund` (no column, enum value appended) + `RefundAmount` column (OD-2 dependent).
- Security/access: refund and clear require cashier permission.
- Regression: existing account tests pass; balance formula reconciles.

**GAP-14 (PARTIAL/DIFFERENT/MISSING) — Discount: value and percentage** (ref: `1.3.1.2`, `1.3.1.3`, `1.3.1.4.1`, `1.3.1.4.2`)
- Precondition: account operation entry open.
- Action: enter a percentage discount; enter a value discount; use `+`-prefix for surcharge; press Enter.
- Observable result: percentage discount computes correctly; value discount applies; `+`-prefix switches to surcharge; first Enter performs, second Enter saves.
- Validation: discount cannot exceed amount; surcharge cannot carry discount.
- Persistence: REQUIRED-ONLY-IF — discount-kind column if value vs percentage must be distinguished beyond existing `DiscountAmount` + `IsExtraCharge`.
- Security/access: none beyond existing cashier permission.
- Regression: existing account tests pass; balance formula reconciles.
- **Depends on OD-11 (surcharge convention) and OD-12 (modify-amount semantics).**

**GAP-22 (PARTIAL) — Single-test result-entry print/preview/review/main-result** (ref: `1.22.12.2`, `1.22.12.3`, `1.22.12.4`, `1.6.1.3.2`)
- Precondition: single-test result entry open with a saved result.
- Action: print, preview, review, open main result.
- Observable result: each action performs its function.
- Validation: actions disabled when no result saved.
- Persistence: no schema change.
- Security/access: each respects existing permission.
- Regression: existing result-entry tests pass.
- **Depends on OD-8 (scope: profile/culture vs single-test).**

**GAP-30 (MISSING) — Composite result screen: open main result** (ref: `1.22.12.5`)
- Precondition: composite result screen open.
- Action: open main result of the request.
- Observable result: main result opens.
- Validation: disabled when no main result exists.
- Persistence: no schema change.
- Security/access: respects existing permission.
- Regression: existing composite screen tests pass.

---

## 4. Stage R2 — Per-patient print-result password and its enforcement

**Associated confirmed gaps:** GAP-01, GAP-02, GAP-03
**Canonical reference identifiers covered:** `1.2.2.4.2`, `1.2.2.4.2.1`, `7.7.2`, `1.6.1.3.3`, `1.6.1.7.2`, `1.2.2.6.6`, `1.2.2.6.7`, `1.4.1.4.11`
**Gap count:** 3

### 4.1 Scope
Make the print-result password a per-patient value with a per-patient enable flag, enforce it on every path that produces or releases a result (report printing, receipt printing, envelope printing, bulk print and the delivery screen's own print path), and print it on the receipt as the credential that gives the patient access to their result.

### 4.2 Prerequisites
- **R1** — the per-patient password column is added to the `Patient` aggregate and displayed in the patient editor's Notes and Communication Block, which R1 edits. The schema migration is sequenced after R1's patient data model is final for the patient row.
- Verification machinery already exists and is already unit-tested (`PrintResultPasswordHasher`, `PrintResultPasswordVerifier`). The only gap is the absence of production call sites and the global-to-per-patient migration.

### 4.3 Deliverables
1. **Domain:** `Patient` aggregate gains a print-result password hash, enable flag.
2. **Application:** a single enforcement service that reuses `PrintResultPasswordVerifier` rather than adding a second; the receipt query and print handler, report print handlers, envelope print handler, and bulk print handler each call the verifier before printing; a password-prompt dialog modelled on the existing `SystemMenuPasswordDialog`.
3. **Presentation:** `PatientEditorView` carries the per-patient field in place of the global block; the print entry points prompt when the setting is enabled.
4. **Infrastructure:** a migration adding a per-patient password hash and enable flag, without dropping the existing global `SystemSettings` columns. The behaviour for a patient with no password set must be decided explicitly (OD-1).

### 4.4 Schema / data changes
**REQUIRED.** The reference defines the password as a per-patient field on the patient record (`1.2.2.4.2`) and requires it to be printed on the receipt (`7.7.2`). A per-patient password requires a persisted hash and enable flag on the patient, and the receipt projection needs the password value. The existing `SystemSettings` columns may be retained as a laboratory-wide default. The migration assessment is justified: the password must travel with the patient, not with the global settings row.

### 4.5 Security consideration (owner decision OD-1)
The reference requires the password to be printed on the receipt (`7.7.2`). The current system stores a **one-way hash**. A one-way hash cannot recover the original password, so the receipt-restored-password requirement is impossible to satisfy with the current storage scheme. See Stage 2 §9.5 and §10 (OD-1) for the complete alternative set.

### 4.6 Acceptance criteria (per gap)

**GAP-01 (MISSING) — Print-result password enforcement** (ref: `1.2.2.4.2.1`, `1.2.2.4.2`, `1.6.1.3.3`, `1.6.1.7.2`, `1.2.2.6.6`, `1.2.2.6.7`, `1.4.1.4.11`, `7.7.2`)
- Precondition: `PrintResultPasswordEnabled = true` and a known password set.
- Action: request any result print (report, receipt, envelope) or any result-delivery print.
- Observable result: a password prompt appears.
- Validation: correct password → print proceeds; wrong password → print refused, no PDF produced; disabled setting → prints proceed without prompt.
- Persistence: no additional schema change beyond GAP-02.
- Security/access: password prompt must not leak the stored hash; comparison uses constant-time equality.
- Regression: existing print tests pass; `PrintResultPasswordVerifierTests` extended to cover each enforcement call site.
- **OD-9 (invoice enforcement) affects whether invoice printing is included.**

**GAP-02 (DIFFERENT) — Per-patient password** (ref: `1.2.2.4.2`, `1.2.2.4.2.1`)
- Precondition: two patients A and B.
- Action: set a distinct print-result password for each; reload each patient.
- Observable result: each patient's stored password round-trips independently.
- Validation: password for A does not match password for B; clearing the password disables enforcement for that patient.
- Persistence: REQUIRED — per-patient password hash + enable flag columns on Patient.
- Security/access: password stored as hash (PBKDF2, as existing); plaintext never persisted.
- Regression: existing patient tests pass.

**GAP-03 (MISSING) — Receipt prints results password** (ref: `7.7.2`, `1.2.2.6.6`)
- Precondition: patient with a print-result password set; receipt print requested.
- Action: print a receipt.
- Observable result: the receipt carries the results-password element in the position the reference shows.
- Validation: receipt without a password (patient has none) omits the element or shows a placeholder (per OD-1).
- Persistence: receipt projection needs the password value (depends on OD-1 for recoverable storage).
- Security/access: password printed only when the patient has one; the printed value must be usable by the patient (OD-1).
- Regression: existing receipt tests pass.
- **Blocked by OD-1.**

---

## 5. Stage R3 — Patient search criteria and the search action bar

**Associated confirmed gaps:** GAP-06, GAP-07, GAP-08, GAP-09
**Canonical reference identifiers covered:** `1.10.1.1.1`, `1.10.1.1.2`, `1.10.1.1.7`, `1.10.1.1.14`, `1.10.1.4.2`, `1.10.1.4.3`, `1.10.1.4.5`, `1.10.1.4.7`
**Gap count:** 4

### 5.1 Scope
Give the patient-search screen the reference's full criterion set and action bar: an exact-name criterion alongside the partial one, a card-number criterion, a not-delivered criterion, an open-account action from the search result, and a selection basket that lets a user hold several patients for the reference's multi-patient actions.

### 5.2 Prerequisites
- None. R3 is topologically independent of R1 and R4–R6.

### 5.3 Deliverables
1. **Application:** `SearchPatientsGlobalQuery` and its handler — new card-number and exact-name predicates, a result-status parameter (not-delivered included), and a selection-basket command set.
2. **Presentation:** `PatientSearchView` — a card-number control, an exact/partial name mode, a not-delivered control, an account action, a selection collection (multi-selection basket).
3. **ResultsEntry:** a not-delivered filter on `GetResultWorklistQuery` if the criterion is to be reachable there as well.
4. **Settings:** revisit the `EnablePatientNameSearchAssist` default of `false`, because the reference does not make the principal name criterion an opt-in setting.

### 5.4 Schema / data changes
**NOT INDICATED.** Every criterion filters on columns that already exist on the patient and patient-test rows, and the action bar needs no new persisted state.

### 5.5 Acceptance criteria (per gap)

**GAP-06 (MISSING) — Card-number search** (ref: `1.10.1.1.7`)
- Precondition: patients with card numbers exist.
- Action: search by exact card number; search by partial card number.
- Observable result: matching patient(s) returned.
- Validation: non-existent card number returns empty; null card numbers excluded.
- Persistence: no schema change (CardNumber already exists).
- Security/access: respects existing search permission.
- Regression: existing search tests pass.

**GAP-07 (MISSING) — Not-delivered criterion** (ref: `1.10.1.1.14`)
- Precondition: a mix of delivered and undelivered results exists.
- Action: apply not-delivered criterion.
- Observable result: only undelivered results returned.
- Validation: tri-state semantics (undelivered / delivered / no filter).
- Persistence: no schema change (IsDelivered already exists).
- Security/access: respects existing search permission.
- Regression: existing search tests pass.

**GAP-08 (DIFFERENT/PARTIAL) — Exact-name search and default-on** (ref: `1.10.1.1.1`, `1.10.1.1.2`)
- Precondition: patients with similar names exist.
- Action: search by exact name; search by partial name; default installation.
- Observable result: exact name returns only exact matches; partial name returns partial matches; name search available without opt-in.
- Validation: exact match is not implemented by narrowing the partial match.
- Persistence: no schema change.
- Security/access: respects existing search permission.
- Regression: existing search tests pass.
- **Depends on OD-7 (placement of not-entered/not-verified/not-printed criteria).**

**GAP-09 (PARTIAL/MISSING) — Search action bar** (ref: `1.10.1.4.2`, `1.10.1.4.3`, `1.10.1.4.5`, `1.10.1.4.7`)
- Precondition: search results displayed.
- Action: open account; open patient data; add to selection basket.
- Observable result: each action performs its function; selection basket retains multiple patients.
- Validation: actions disabled when no patient selected.
- Persistence: no schema change.
- Security/access: each respects existing permission.
- Regression: existing search tests pass.

---

## 6. Stage R4 — Main-window period list and the work-sheet module

**Associated confirmed gaps:** GAP-15, GAP-17
**Canonical reference identifiers covered:** `1.2.1.4`, `2.1.3.5`, `2.2.1.3.3`, `2.3.1.3.3`, `2.3.1.5.1`, `2.3.1.5.3`
**Gap count:** 2

### 6.1 Scope
Give the main patients window its period-list action — the patients of a chosen registration period — and complete the work-sheet module: print the test-group sheet, add the unfinished-only restriction, and provide the print and preview pair in every sheet mode.

### 6.2 Prerequisites
- **R1** — the period list and the unfinished-only restriction query registered patients and results. The workflows that present these rows are in R1's scope. The query filters are applied server-side on existing tables; no schema change is implied.

### 6.3 Deliverables
1. **Presentation:** `PatientsHubView` and its view model — a period-list action that displays the patients registered in a chosen period with their registration dates.
2. **Application:** `WorkSheets` — `GetVisitWorkSheetQuery`, `GetWorkSheetByTestGroupQuery`, `GetWorkSheetByWorkGroupLogQuery` gain an unfinished-only parameter; the print action is bound in the group modes (the source comment records "print ships disabled — owner-pending, no command exists").
3. **Presentation:** `WorkSheetsView` — the enabled group print button, an unfinished-only option, and a print and preview pair in every sheet mode.
4. **Domain:** a pending-test predicate for the unfinished-only restriction.

### 6.4 Schema / data changes
**NOT INDICATED.** The period list is a query over existing patient registrations and the unfinished-only restriction is a predicate over existing result state; neither needs a schema change.

### 6.5 Acceptance criteria (per gap)

**GAP-15 (MISSING) — Period-list action** (ref: `1.2.1.4`)
- Precondition: patients registered in multiple periods exist.
- Action: choose a period from the main patients window.
- Observable result: patients registered in that period with registration dates displayed; reachable without going through the search screen.
- Validation: empty period returns empty list; invalid period rejected.
- Persistence: no schema change.
- Security/access: respects existing patient-list permission.
- Regression: existing patient hub tests pass.

**GAP-17 (MISSING/PARTIAL) — Work-sheet print and unfinished-only** (ref: `2.1.3.5`, `2.2.1.3.3`, `2.3.1.3.3`, `2.3.1.5.1`, `2.3.1.5.3`)
- Precondition: work-sheet data exists for visit, test-group and work-group modes.
- Action: print test-group sheet; apply unfinished-only filter; print and preview in each mode.
- Observable result: test-group sheet prints and previews; unfinished-only restricts to tests without a result; pending count and printed sheet agree.
- Validation: print disabled when no data; unfinished-only with no pending tests returns empty.
- Persistence: no schema change.
- Security/access: respects existing work-sheet permission.
- Regression: existing work-sheet tests pass.
- **Open design question (OD-3):** double-press removal (2.2.1.3.3, 2.3.1.3.3) requires an editable patient/test list. The reference presupposes this; Top-Lab's sheet is a generated read-only report. Implementing removal means building an editable list — a larger change. Flagged as open design question, not assumed.

---

## 7. Stage R5 — Reference-data editing: undo, group membership, deletion semantics and history

**Associated confirmed gaps:** GAP-23, GAP-24, GAP-25, GAP-26, GAP-27, GAP-28
**Canonical reference identifiers covered:** `1.3.2.3`, `1.15.3.4`, `1.15.4.5`, `3.2.1.4.4`, `3.2.1.4.5`, `3.3.1.3.11`, `3.3.1.3.12`, `3.6.1.4.3`, `3.6.1.4.5`, `3.6.1.4.6`, `3.6.1.4.8`, `3.7.1.8.4`, `3.9.1.6.5`, `3.9.1.6.6`, `4.2.1.6.5`
**Gap count:** 6

### 7.1 Scope
Give the reference-data editing surfaces the reference's operating rules: an undo on every editing screen, an editor for a test group's test membership, a printed antibiotic list, the reference's deletion behaviour on the account, test and test-group screens, a way to clear a history report's selection and to abandon a history operation, and a distinct confirm step for the reference-range limits.

### 7.2 Prerequisites
- None. R5 is topologically independent of R1 and R3–R6.

### 7.3 Deliverables
1. **Presentation:** six editing screens (test catalogue, test editor reference-range block, test-groups, external-entities, antibiotics, user-management) and the audit/history/combined-report surfaces gain undo and history-clear/cancel actions.
2. **Application:** test-group membership editor (add and remove tests from a group), undo commands (test catalogue, test groups, external entities, user management), antibiotic print, clear-history and cancel-history, and delete-operation semantics.
3. **Infrastructure:** a new antibiotic-list PDF writer in the Printing folder.

### 7.4 Schema / data changes
**Required only for GAP-26**, and only if the reference's deletion behaviour is adopted literally (OD-2). Hard deletion of a payment operation changes the persisted account history, and the `PatientTest` and `TestGroup` rows would need a delete path in place of their active flag. The audit does not decide soft vs hard deletion: that is an **owner decision** (the audit-trail argument for soft deletion is the reason Top-Lab behaves as it does). The migration assessment is therefore conditional and is flagged in Stage 2 §10 (OD-2).

**NOT INDICATED** for undo, membership editing, antibiotic print, history clearing, and the range-confirm step.

### 7.5 Acceptance criteria (per gap)

**GAP-23 (MISSING) — Undo on editing screens** (ref: `3.2.1.4.4`, `3.3.1.3.12`, `3.6.1.4.8`, `3.7.1.8.4`, `3.9.1.6.6`, `4.2.1.6.5`)
- Precondition: an editing screen open with unsaved changes.
- Action: click undo.
- Observable result: screen returns to state before the edit began; persisted records unchanged.
- Validation: undo with no changes is a no-op; undo after save is disabled.
- Persistence: no schema change.
- Security/access: respects existing edit permission.
- Regression: existing editing tests pass.

**GAP-24 (PARTIAL/MISSING) — Test-group membership editor** (ref: `3.6.1.4.3`, `3.6.1.4.5`, `3.6.1.4.6`)
- Precondition: a test group open for editing.
- Action: add a test; remove a test.
- Observable result: membership listed; count updates; membership survives reload.
- Validation: duplicate add rejected; remove of non-member rejected.
- Persistence: no schema change (existing membership table).
- Security/access: respects existing test-group permission.
- Regression: existing test-group tests pass.

**GAP-25 (MISSING) — Antibiotic list print** (ref: `3.9.1.6.5`)
- Precondition: antibiotics with commercial names exist.
- Action: print antibiotic list.
- Observable result: document listing antibiotics with commercial names.
- Validation: empty list produces empty document or is disabled.
- Persistence: no schema change.
- Security/access: respects existing antibiotic permission.
- Regression: existing antibiotic tests pass.

**GAP-26 (DIFFERENT) — Deletion semantics** (ref: `1.3.2.3`, `3.2.1.4.5`, `3.6.1.4.4`)
- Precondition: an operation/test/test-group exists.
- Action: delete it.
- Observable result: meets the acceptance criterion matching the recorded decision (OD-2): either operation absent from statement and totals reconcile (hard delete), or operation stays flagged 'ملغاة' (soft delete).
- Validation: totals reconcile after deletion.
- Persistence: REQUIRED-ONLY-IF (OD-2) — soft delete needs `IsDeleted` column on audit entities; hard delete needs deletion command.
- Security/access: delete requires appropriate permission.
- Regression: existing delete tests pass.
- **Blocked by OD-2.**

**GAP-27 (MISSING/PARTIAL) — History clear and cancel** (ref: `1.15.4.5`, `1.15.3.4`)
- Precondition: a history report with a selection exists.
- Action: clear selection; abandon history operation.
- Observable result: selection empty; operation abandoned without applying.
- Validation: clear on empty selection is a no-op; cancel does not persist partial changes.
- Persistence: no schema change.
- Security/access: respects existing history permission.
- Regression: existing history tests pass.

**GAP-28 (MISSING) — Reference-range confirm step** (ref: `3.3.1.3.11`)
- Precondition: reference-range form open.
- Action: apply limits; confirm.
- Observable result: committed range persisted only after confirm; applying without confirming does not persist.
- Validation: cancel discards applied limits; confirm without apply is disabled.
- Persistence: no schema change.
- Security/access: respects existing test-data permission.
- Regression: existing reference-range tests pass.

---

## 8. Stage R6 — Utilities, statistics, auditing, sent-out samples, sample withdrawal and settings

**Associated confirmed gaps:** GAP-16, GAP-18, GAP-19, GAP-20, GAP-21, GAP-29
**Canonical reference identifiers covered:** `1.16.1.1.2`, `1.16.1.1.3`, `1.16.1.1.8`, `1.16.1.2.2`, `5.6.2.3`, `4.5.1.2.2`, `4.5.1.2.3`, `4.5.1.2.4`, `6.3.1.6.4`, `6.3.1.6.5`, `8.2.1.7.2`, `8.2.1.8.1`, `8.2.1.9.1`, `8.2.1.9.2`, `8.2.1.9.3`, `8.2.1.9.4`, `8.2.1.10.3`, `7.2.1.9.2`, `7.2.5.7.2`, `7.2.5.8.3`, `7.2.5.8.4`, `7.2.5.8.5`, `7.2.5.8.6`, `7.6.1.4.2`, `7.6.1.4.3`, `7.6.1.4.5`, `7.6.1.4.6`, `7.6.1.4.8`, `7.6.1.4.9`, `7.5.1.8.2`, `7.5.1.8.3`, `7.5.1.8.8`, `7.5.1.8.9`, `7.2.1.6.2`, `7.2.1.6.3`
**Gap count:** 6

### 8.1 Scope
Complete the reference's remaining modules: the four missing utility surfaces (image library, abbreviations dictionary, data manual, clinical terminology), the statistics report save, the audit module's user filter and period deletion, the sent-out samples' ignore and always-to behaviours, the sample-withdrawal screen's removal, save, sticker printing and calendar, and the settings module's colour chooser, default-layout restore, envelope images and receipt header/footer images.

### 8.2 Prerequisites
- None. R6 is topologically independent of R1 and R3–R5.
- The utility surfaces (GAP-16), the sent-out always-to behaviour (GAP-20) and the sample-withdrawal save-after-read behaviour are the parts of this stage whose exact behaviour the reference itself leaves unestablished (reference questions Q-046, Q-067–Q-072 and the UNDETERMINED entries). Those parts must be re-confirmed against a further reference reading before implementation; the acceptance criteria that depend on them are labelled as such.

### 8.3 Deliverables
1. **Application:** `Utilities` (manual, abbreviations, image library, terminology surfaces), `Statistics` (report save), `AuditAndTraceability` (user filter, delete period), `SentOutSamples` (ignore, always-to), `SampleCollection` (remove, save, sticker print), `SystemAndPrintSettings` (colours, default layout, envelope and receipt images).
2. **Presentation:** `UtilitiesView`, `StatisticsView`, `AuditView`, `SentOutSamplesView`, `SendSampleOutDialogWindow`, `SampleDrawBoardView`, `SampleCollectionView`, `ReportSettingsView`, `ReceiptSettingsView`, `EnvelopeSettingsView`, `ReportItemTemplatesView`.
3. **Domain:** report colour and layout settings, sent-out default destination.
4. **Infrastructure:** a sticker PDF writer; the existing barcode writer reused by the patient editor must be checked for the print-once convention.

### 8.4 Schema / data changes
**REQUIRED for:**
- **GAP-19** — the user filter over the audit records itself needs no schema change, but the delete-period operation removes persisted audit rows and therefore needs a deletion path and, by extension, a retention decision (OD-4).
- **GAP-20** — the always-to destination persists a default-destination setting and requires a column (OD-6).
- **GAP-29** — the colour values and the layout defaults are persisted settings with no column today.

**NOT INDICATED** for the remainder (utility surfaces, statistics save, sample-withdrawal remove/save/sticker-print, calendar).

### 8.5 Acceptance criteria (per gap)

**GAP-16 (MISSING) — Four utility surfaces** (ref: `1.16.1.1.2`, `1.16.1.1.3`, `1.16.1.1.8`, `1.16.1.2.2`)
- Precondition: utilities window open.
- Action: open image library, abbreviations dictionary, data manual, clinical terminology.
- Observable result: image library lists and opens images; abbreviations dictionary searchable; data manual searchable by word or value.
- Validation: empty library shows empty state; search with no match shows empty.
- Persistence: no schema change.
- Security/access: respects existing utility permission.
- Regression: existing utility tests pass.
- **Note:** exact behaviour of image library and abbreviations unestablished (reference questions Q-046, Q-067–Q-072); re-confirm before implementation.

**GAP-18 (MISSING) — Statistics report save** (ref: `5.6.2.3`)
- Precondition: census loaded in statistics preview.
- Action: save.
- Observable result: rendered report persisted; saved artefact matches preview.
- Validation: save with no census loaded is disabled; save failure shows error.
- Persistence: no schema change.
- Security/access: respects existing statistics permission.
- Regression: existing statistics tests pass.

**GAP-19 (MISSING) — Audit user filter and period deletion** (ref: `4.5.1.2.2`, `4.5.1.2.3`, `4.5.1.2.4`)
- Precondition: login records for a period loaded.
- Action: select users; show all; delete period.
- Observable result: user selector filters records; show-all restores full period; delete-period removes chosen period's records.
- Validation: selecting two users returns only their records; delete requires confirmation.
- Persistence: user filter NOT INDICATED (query predicate); delete-period REQUIRED-ONLY-IF (OD-4/OD-2).
- Security/access: respects existing audit permission.
- Regression: existing audit tests pass.
- **Delete-period blocked by OD-4 (retention policy) and OD-2 (deletion semantics).**

**GAP-20 (MISSING) — Sent-out ignore and always-to** (ref: `6.3.1.6.4`, `6.3.1.6.5`)
- Precondition: sent-out samples list open.
- Action: ignore a selected sample; set always-to destination.
- Observable result: ignored sample removed from sending list without sending; always-to persists destination; subsequent sending pre-fills.
- Validation: ignore on empty selection is a no-op; always-to with no destination rejected.
- Persistence: ignore NOT INDICATED; always-to REQUIRED-ONLY-IF (OD-6).
- Security/access: respects existing sent-out permission.
- Regression: existing sent-out tests pass.
- **Always-to blocked by OD-6.**

**GAP-21 (MISSING) — Sample-withdrawal remove, save, sticker print, calendar** (ref: `8.2.1.7.2`, `8.2.1.8.1`, `8.2.1.9.1`–`8.2.1.9.4`, `8.2.1.10.3`)
- Precondition: sample draw board open.
- Action: remove; save; print stickers (all / container-filtered); step calendar.
- Observable result: remove removes; save persists; stickers print; calendar steps forward/back.
- Validation: remove on empty selection is a no-op; save with no changes is a no-op.
- Persistence: no schema change.
- Security/access: respects existing sample permission.
- Regression: existing sample tests pass.

**GAP-29 (MISSING) — Settings colour chooser, default layout, envelope/receipt images** (ref: `7.2.1.9.2`, `7.2.5.7.2`, `7.2.5.8.3`–`7.2.5.8.6`, `7.6.1.4.2`, `7.6.1.4.3`, `7.6.1.4.5`, `7.6.1.4.6`, `7.6.1.4.8`, `7.6.1.4.9`, `7.5.1.8.2`, `7.5.1.8.3`, `7.5.1.8.8`, `7.5.1.8.9`, `7.2.1.6.2`, `7.2.1.6.3`)
- Precondition: settings module open.
- Action: choose colour (confirm/cancel/close); restore default layout; browse/clear receipt header/footer images; browse/clear envelope header/footer/logo images; browse/clear report logo.
- Observable result: colour chooser works with confirm/cancel/close; default layout restored; images browse and clear; all persist across reload.
- Validation: cancel discards colour change; clear removes image; invalid image rejected.
- Persistence: REQUIRED (colour/layout columns).
- Security/access: respects existing settings permission.
- Regression: existing settings tests pass.
- **Colour chooser and default-layout restore blocked by OD-5.**

---

## 9. Traceability — confirmed gaps to stage and acceptance criteria

| Gap | Stage | Key helper artifacts | Stage acceptance criteria |
|---|---|---|---|
| GAP-01 | R2 | `PrintResultPasswordVerifier`, `SystemSettings`, `ReceiptDto` | §4.6 |
| GAP-02 | R2 | `Patient`, `SystemSettings` | §4.6 |
| GAP-03 | R2 | `7.7.2`, `ReceiptDto`, `ReceiptPdfWriter` | §4.6 |
| GAP-04 | R1 | `1.4.1.4.6`, `Patient.Notes`, delivery handover | §3.5 |
| GAP-05 | R1 | `1.2.2.4.1`, `1.6.1.7`, profile/culture entry | §3.5 |
| GAP-06 | R3 | `1.10.1.1.7`, `CardNumber` | §5.5 |
| GAP-07 | R3 | `1.10.1.1.14`, `IsDelivered` | §5.5 |
| GAP-08 | R3 | `1.10.1.1.1/2`, `EnablePatientNameSearchAssist` | §5.5 |
| GAP-09 | R3 | `1.10.1.4.2/3/5/7` | §5.5 |
| GAP-10 | R1 | `1.2.2.6.8/10/15/16` | §3.5 |
| GAP-11 | R1 | `1.2.2.5.1`, catalogue filters/menus | §3.5 |
| GAP-12 | R1 | `1.4.1.4.3/4/5/7/8/9/10/11` | §3.5 |
| GAP-13 | R1 | `1.3.2.5`, `1.3.2.7` | §3.5 |
| GAP-14 | R1 | `1.3.1.3`, `1.3.1.4.1`, `1.3.1.4.2` | §3.5 |
| GAP-15 | R4 | `1.2.1.4` | §6.5 |
| GAP-16 | R6 | `1.16.1.1.2/3/8`, `1.16.1.2.2` | §8.5 |
| GAP-17 | R4 | `2.1.3.5`, `2.3.1.5.1/3` | §6.5 |
| GAP-18 | R6 | `5.6.2.3` | §8.5 |
| GAP-19 | R6 | `4.5.1.2.2/3/4`, audit records | §8.5 |
| GAP-20 | R6 | `6.3.1.6.4/5` | §8.5 |
| GAP-21 | R6 | `8.2.1.7.2/8.1/9.1/9.2/9.3/9.4/10.3` | §8.5 |
| GAP-22 | R1 | `1.22.12.2/3/4`, `1.6.1.3.2` | §3.5 |
| GAP-23 | R5 | `3.2.1.4.4`, `3.3.1.3.12`, `3.6.1.4.8`, `3.7.1.8.4`, `3.9.1.6.6`, `4.2.1.6.5` | §7.5 |
| GAP-24 | R5 | `3.6.1.4.3/5/6` | §7.5 |
| GAP-25 | R5 | `3.9.1.6.5` | §7.5 |
| GAP-26 | R5 | `1.3.2.3`, `3.2.1.4.5`, `3.6.1.4.4` | §7.5 |
| GAP-27 | R5 | `1.15.4.5`, `1.15.3.4` | §7.5 |
| GAP-28 | R5 | `3.3.1.3.11` | §7.5 |
| GAP-29 | R6 | `7.2.1.9.2`, `7.2.5.7.2`, `7.2.5.8.3/4/5/6`, `7.6.1.4.2/3/5/6/8/9`, `7.5.1.8.2/3/8/9`, `7.2.1.6.2/3` | §8.5 |
| GAP-30 | R1 | `1.22.12.5` | §3.5 |

---

## 10. Owner decisions that block or gate stages

The following are **not** product, security, policy or scope decisions that can responsibly be inferred from code or documentation. Each blocks its stage until the owner records a decision. For each, the options, functional consequences, security consequences and the audit's recommendation are stated. Full structure in Stage 2 §10.

| # | Decision | Stage | Impact | Blocks |
|---|---|---|---|---|
| OD-1 | Print-result password storage design | R2 | Blocks GAP-03 | GAP-03 |
| OD-2 | Deletion semantics: soft vs hard delete | R5 | Blocks GAP-26 | GAP-26 |
| OD-3 | Work-sheet double-press removal: editable list | R4 | Changes GAP-17 scope | GAP-17 (double-press only) |
| OD-4 | Audit-period deletion retention policy | R6 | Blocks GAP-19 delete-period | GAP-19 (delete-period only) |
| OD-5 | Colour-chooser interaction model and default-layout restore target | R6 | Affects GAP-29 acceptance criteria | GAP-29 (colour chooser + default layout) |
| OD-6 | Sent-out "always to" default-destination behaviour | R6 | Blocks GAP-20 always-to | GAP-20 (always-to only) |
| OD-7 | Status criteria placement (not-entered/not-verified/not-printed/not-delivered) | R3 | Affects GAP-08 design | GAP-08 (design only) |
| OD-8 | GAP-22/30: extend profile/culture screens vs single-test screen | R1 | Affects GAP-22/30 scope | GAP-22, GAP-30 (scope) |
| OD-9 | Invoice enforcement | R2 | Affects GAP-01 invoice criterion | GAP-01 (invoice criterion) |
| OD-10 | Exclusion of 3.15.5.4 / 3.1.3.4 / 1.4.1.4.12 | — | Exclusion decision | None (may add a gap if reversed) |
| OD-11 | Surcharge convention: +-prefix vs separate operation | R1 | Affects GAP-14 surcharge criterion | GAP-14 (surcharge) |
| OD-12 | Modify-amount: correction offsets vs overwrite | R1 | Affects GAP-14 modify-amount criterion | GAP-14 (modify-amount) |

---

## 11. Readiness matrix

| Stage | Status | Ready gaps | Blocked gaps (decision ID) | Can start now? |
|---|---|---|---|---|
| R1 | READY-WITH-PARTIAL-SCOPE | GAP-04, 05, 10, 11, 12, 13, 14 (core), 30 | GAP-14 (surcharge: OD-11; modify-amount: OD-12); GAP-22, 30 (scope: OD-8) | Yes — start with GAP-04 (notes rendering, no schema, no decision) |
| R2 | READY-WITH-PARTIAL-SCOPE | GAP-01 (enforcement), GAP-02 (per-patient column) | GAP-03 (OD-1); GAP-01 (invoice criterion: OD-9) | After R1 |
| R3 | READY | GAP-06, 07, 08, 09 | None (OD-7 affects design only) | Yes — independent; can run in parallel with R1 |
| R4 | READY-WITH-PARTIAL-SCOPE | GAP-15 | GAP-17 (double-press removal: OD-3) | After R1 |
| R5 | READY-WITH-PARTIAL-SCOPE | GAP-23, 24, 25, 27, 28 | GAP-26 (OD-2) | Yes — independent; can run in parallel with R1 |
| R6 | READY-WITH-PARTIAL-SCOPE | GAP-16, 18, 21 | GAP-19 (delete-period: OD-4/OD-2); GAP-20 (always-to: OD-6); GAP-29 (colour chooser + default layout: OD-5) | Yes — independent; can run in parallel with R1 |

### 11.1 Recommended first implementation step

**Start with GAP-04 (patient notes rendering in the result-delivery workflow) in R1.** Justification:
- No schema change (Patient.Notes already exists, Patient.cs:59).
- No open owner decision (notes rendering is a pure presentation gap).
- Independent of all other gaps (no prerequisite).
- Low risk (read-only rendering of an existing persisted property).
- High user value (notes are already edited in the patient editor but invisible in the delivery workflow).
- Establishes the pattern for GAP-05 (profile/culture entry screens) in the same stage.

After GAP-04, proceed with GAP-05, then GAP-10, GAP-11, GAP-12 (action bands, no schema), then GAP-13/14 (account model, schema change, requires OD-11/OD-12).

---

## 12. Verification requirements (targets, not claims)

Verification requirements are stated per stage. They are **targets for the implementation phase**, not claims that the work has occurred. Each stage's tests must be re-run after every commit; the build must remain 0/0 errors and no new test failures.

| Stage | Verification |
|---|---|
| R1 | New unit and integration tests for the account rules and for the delivery action band; the whole test suite must pass; each acceptance criterion in §3.5 must be re-checked against the code |
| R2 | Extend `PrintResultPasswordVerifierTests` to cover each enforcement call site; verify the migration applies without error; verify a wrong password refuses the print and no PDF is produced |
| R3 | New handler tests for each criterion, including AND-combination semantics; verify the exact-name criterion is not implemented by narrowing the existing partial match |
| R4 | New query tests for the unfinished-only restriction; a print-path test for the group modes; verify the period list and the unfinished-only restriction query on the server side |
| R5 | New command tests for undo, membership editing, clear-history and the delete semantics; verify the account statement, the catalogue and the groups list reconcile |
| R6 | New command and query tests per module; verify the audit deletion against a retention requirement; verify the colour chooser does not change the rendered output of the tests that already cover it |

---

## 13. End of roadmap

*This document is a planning artifact only. No stage in it has been implemented. Every acceptance criterion must be re-checked against the code as it stands at the time of implementation.*

---

*End of reconciled implementation roadmap (V3).*
