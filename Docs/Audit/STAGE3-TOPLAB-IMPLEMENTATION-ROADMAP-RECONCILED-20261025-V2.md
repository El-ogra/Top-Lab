# Stage 3 — Top-Lab Dependency-Ordered Implementation Roadmap (Reconciled)

**Audit date:** 2026-10-25
**Audited revision:** `efc909c688ccd3375b6bf3bc15af617f3739c647` (confirmed as HEAD)
**Input:** `STAGE2-TOPLAB-REFERENCE-COMPARISON-INDEPENDENTLY-VERIFIED-20261025-V2.md` (the independently verified gap register)
**Audit type:** Read-only verification of the prior report. **No stage is implemented by this document.**

> This document enumerates the 30 confirmed in-scope gaps from §7 of the companion report, in a dependency-ordered stage structure, with prerequisites, scope, deliverables, acceptance criteria and traceability. It is a planning artifact, not a build plan. Future implementation must re-check each acceptance criterion against the code as it stands at the time of implementation.

---

## 1. Stage summary

| Stage | Title | Gaps | Dependencies | Schema/data changes |
|---|---|---|---|---|
| **R1** | Patient registration, result entry, delivery and account action bands | GAP-04, GAP-05, GAP-10, GAP-11, GAP-12, GAP-13, GAP-14, GAP-22, GAP-30 | — | Required for GAP-13/14 only |
| **R2** | Per-patient print-result password and its enforcement | GAP-01, GAP-02, GAP-03 | **R1** | Required (per-patient password columns + receipt projection) |
| **R3** | Patient search criteria and the search action bar | GAP-06, GAP-07, GAP-08, GAP-09 | — | Not indicated |
| **R4** | Main-window period list and the work-sheet module | GAP-15, GAP-17 | **R1** | Not indicated |
| **R5** | Reference-data editing: undo, group membership, deletion semantics and history | GAP-23, GAP-24, GAP-25, GAP-26, GAP-27, GAP-28 | — | Required only for GAP-26 (owner decision) |
| **R6** | Utilities, statistics, auditing, sent-out samples, sample withdrawal and settings | GAP-16, GAP-18, GAP-19, GAP-20, GAP-21, GAP-29 | — | Required for GAP-19 (deletion path), GAP-20 (always-to), GAP-29 (colour/layout persistence) |

---

## 2. Roadmap-wide constraints

1. **Offline operation is a hard requirement.** No stage may introduce an online service or any internet-dependent runtime behaviour. Every proposed change is local: local hashing and comparison, local file pickers, local PDF writers, local queries. The existing `QuestPDF` printing stack and the local SQL Server / file database deployment are unchanged.
2. **Existing behaviour is preserved unless the canonical reference establishes a required change.** Where Top-Lab's behaviour differs from the reference but the reference does not require the difference to change (for example the soft-delete of tests and groups), the difference is raised as a gap with the rationale stated, and the decision is taken explicitly rather than by omission.
3. **No stage is implemented by this audit.** The stages are proposals only. Each acceptance criterion in §3–§8 must be re-checked against the code at implementation time.
4. **Migration assessments distinguish confirmed schema needs from behaviour.** Only the account-model work (GAP-13, GAP-14), the per-patient password (GAP-02, GAP-03), the audit-period deletion (GAP-19), the always-to destination (GAP-20) and the settings values (GAP-29) are assessed as requiring a persisted change; everything else is behaviour over data that already exists.
5. **No roadmap stage is implemented from this document alone.** The acceptance criteria in §3–§8 must be re-checked against the code as it stands at the time of implementation.

---

## 3. Stage R1 — Patient registration, result entry, delivery and account action bands

**Associated confirmed gaps:** GAP-04, GAP-05, GAP-10, GAP-11, GAP-12, GAP-13, GAP-14, GAP-22, GAP-30
**Canonical reference identifiers covered:** `1.2.2.4.1`, `1.2.2.5.1`, `1.2.2.6.8`, `1.2.2.6.10`, `1.2.2.6.15`, `1.2.2.6.16`, `1.2.2.6.6`, `1.2.2.6.7`, `1.3.1.1`, `1.3.1.2`, `1.3.1.3`, `1.3.1.4.1`, `1.3.1.4.2`, `1.3.1.14`, `1.3.2.1`, `1.3.2.3`, `1.3.2.5`, `1.3.2.7`, `1.3.2.8`, `1.4.1.4.3`, `1.4.1.4.4`, `1.4.1.4.5`, `1.4.1.4.6`, `1.4.1.4.7`, `1.4.1.4.8`, `1.4.1.4.9`, `1.4.1.4.10`, `1.4.1.4.11`, `1.6.1.3.2`, `1.6.1.3.3`, `1.6.1.7.2`, `1.6.1.7.3`, `1.6.1.7.4`, `1.22.12.2`, `1.22.12.3`, `1.22.12.4`, `1.22.12.5`
**Gap count:** 9

### 3.1 Scope
Restore the reference's action bands and operating rules on the screens a user works in every day: the patient registration screen, the three result-entry screens, the delivery screen and the patient-account screen. Give the test catalogue its filter bar and its two menus; give the delivery screen its eight missing actions (excluding notes, which is GAP-04) and its print/preview/copy/patient-data/combined-report/medical-history/empty-report/envelope-print actions; give the account panel its refund, clear-account, percentage-discount, surcharge and Enter-key rules; render patient notes on the profile and culture entry screens and make notes reachable from the delivery workflow; and give the single-test result screen its print, preview, review and main-result actions.

### 3.2 Prerequisites
- R0: The audited commit `efc909c` is the branch tip. No prerequisite stage. R1 is the natural starting point.
- R1 owns the three explicitly in-scope areas that live on the registration, result-entry and delivery screens: notes rendering, action band and navigation, and financial/account panel.
- GAP-22 and GAP-30 require a decision on whether to extend the profile/culture screens or to add the actions to the single-test screen; the two paths must not diverge. The decision is an **owner decision** and is flagged in §10 (owner decisions block nothing but must be recorded).

### 3.3 Deliverables
1. **Patient editor action band** — envelope print, follow-up, patient-window (back-to-patient-list) and lab-results actions bound and reachable without leaving the open registration.
2. **Test catalogue** — filter field, mutually exclusive TM/TG/CG mode options, complete-test menu action, and test-group menu action.
3. **Delivery screen** — patient account, refresh, print, preview, copy, notes, patient data, combined report, medical history, empty report and envelope print actions; the delivery list must be printable and previewable.
4. **Account panel** — refund operation kind, clear-account action, percentage discount field, surcharge (`+`-prefix) convention, and Enter-key account-operation rule; the balance formula must reconcile against the reference's field set (paid-previously, total-after-discount, total-collected, remainder-for-customer, remainder-for-laboratory).
5. **Result-entry screens** — profile and culture screens render patient notes; the single-test result screen exposes print, preview, review and main-result actions.
6. **Notes reachability** — the delivery workflow exposes the patient note on the delivery entry and the handover screen, and the note action opens the stored patient note.

### 3.4 Downstream impact
- The account-panel work changes the balance formula in `PatientAccountCalculator`, which the concurrent-settlement behaviour already depends on. The settlement transaction's serializability must not be weakened, and the new discount and refund semantics must be settled against the reference's field definitions before the formula is changed.
- No schema change is implied for the action-band, filter, menu, notes or single-test-screen work. The account-panel schema change (GAP-13/14) is assessed in §9.

### 3.5 Acceptance criteria
Each gap carries its own objective acceptance criterion in the companion report (§7). The stage is complete when:
1. The patient editor exposes envelope print, follow-up, patient-window and lab-results, each reachable without leaving the open registration.
2. The test catalogue exposes the filter field, mutually exclusive mode options, and both menus.
3. The delivery screen exposes all eight missing actions; the delivery list is printable and previewable.
4. The account panel accepts a refund, clears the account in bulk, expresses both a value and a percentage discount, switches a payment into a surcharge by `+`-prefix, and performs the operation on the first Enter press and saves on the second.
5. The balance formula reconciles against the reference's field set after refund and percentage-discount work.
6. Profile and culture entry screens render the patient note; the delivery workflow exposes the note on the delivery entry, the handover screen, and the notes action.
7. The single-test result screen exposes print, preview, review and main-result, each operating on the test just entered.

---

## 4. Stage R2 — Per-patient print-result password and its enforcement

**Associated confirmed gaps:** GAP-01, GAP-02, GAP-03
**Canonical reference identifiers covered:** `1.2.2.4.2`, `1.2.2.4.2.1`, `7.7.2`, `1.6.1.3.3`, `1.6.1.7.2`, `1.2.2.6.6`, `1.2.2.6.7`, `1.4.1.4.11`
**Gap count:** 3

### 4.1 Scope
Make the print-result password a per-patient value with a per-patient enable flag, enforce it on every path that produces or releases a result (report printing, receipt printing, invoice printing, envelope printing, bulk print and the delivery screen's own print path), and print it on the receipt as the credential that gives the patient access to their result.

### 4.2 Prerequisites
- **R1** — the per-patient password column is added to the `Patient` aggregate and displayed in the patient editor's Notes and Communication Block, which R1 edits. The schema migration is sequenced after R1's patient data model is final for the patient row.
- Verification machinery already exists and is already unit-tested (`PrintResultPasswordHasher`, `PrintResultPasswordVerifier`). The only gap is the absence of production call sites and the global-to-per-patient migration.

### 4.3 Deliverables
1. **Domain:** `Patient` aggregate gains a print-result password hash, enable flag, and the reference's `Notes`/communication block fields are present.
2. **Application:** a single enforcement service that reuses `PrintResultPasswordVerifier` rather than adding a second; the receipt query and print handler, report print handlers, envelope print handler, and bulk print handler each call the verifier before printing; a password-prompt dialog modelled on the existing `SystemMenuPasswordDialog`.
3. **Presentation:** `PatientEditorView` carries the per-patient field in place of the global block; the print entry points prompt when the setting is enabled.
4. **Infrastructure:** a migration adding a per-patient password hash and enable flag, without dropping the existing global `SystemSettings` columns. The behaviour for a patient with no password set must be decided explicitly.

### 4.4 Schema / data changes
**REQUIRED.** The reference defines the password as a per-patient field on the patient record (`1.2.2.4.2`) and requires it to be printed on the receipt (`7.7.2`). A per-patient password requires a persisted hash and enable flag on the patient, and the receipt projection needs the password value. The existing `SystemSettings` columns may be retained as a laboratory-wide default. The migration assessment is justified: the password must travel with the patient, not with the global settings row.

### 4.5 Security consideration (owner decision)
The reference requires the password to be printed on the receipt (`7.7.2`). The current system stores a **one-way hash**. A one-way hash cannot recover the original password, so the receipt-restored-password requirement is impossible to satisfy with the current storage scheme. The implementation phase must choose between:

| Option | Functional consequence | Security consequence |
|---|---|---|
| (a) Recoverable storage (encryption, not one-way hash) and print the plaintext | Implements the reference as specified | Reversible storage requires a key-management decision; the key must not leak. A meaningful operational and security decision |
| (b) Keep one-way hash; print the hash as the "password" | Fails to implement the reference's stated receipt content | Does not weaken security (hash is not reversible) but no longer implements the reference's requirement |
| (c) Documented deviation (GAP-03) | The receipt requirement is recorded as an accepted deviation | No new exposure; the audit records the deviation rather than engineering around it |

**Owner decision blocking R2:** whether to implement the reference as specified (option (a)) or record GAP-03 as an accepted deviation (option (c)). See §10 (owner decisions).

### 4.6 Acceptance criteria
1. Given `PrintResultPasswordEnabled = true` and a known password, request any result print (report, receipt, invoice, envelope) or any result-delivery print; expect a password prompt.
2. Enter the correct password and expect the print to proceed.
3. Enter a wrong password and expect the print to be refused and no PDF produced.
4. Disable the setting and expect prints to proceed without a prompt.
5. Register two patients A and B, set a distinct print-result password for each, reload each patient, and expect each patient's stored password to round-trip independently.
6. Print a receipt and expect the receipt to carry the results-password element in the position the reference shows.

---

## 5. Stage R3 — Patient search criteria and the search action bar

**Associated confirmed gaps:** GAP-06, GAP-07, GAP-08, GAP-09
**Canonical reference identifiers covered:** `1.10.1.1.1`, `1.10.1.1.2`, `1.10.1.1.6`, `1.10.1.1.7`, `1.10.1.1.14`, `1.10.1.4.2`, `1.10.1.4.3`, `1.10.1.4.5`, `1.10.1.4.7`
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

### 5.5 Acceptance criteria
1. Exact-name search returns exactly the patient whose full name matches; partial name returns the documented partial set.
2. Name criteria are separately selectable and name search is available in a default installation without an opt-in setting.
3. Card-number search resolves a patient by exact and partial card number.
4. The not-delivered criterion returns only undelivered results and holds tri-state semantics (undelivered / delivered / no filter).
5. The search screen's selection basket retains two selected patients; the account action opens the selected patient's account; the reference's delete action is available from the search result.

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

### 6.5 Acceptance criteria
1. The main patients window exposes a period-list action; choosing a period returns the patients registered in that period with registration dates, reachable without going through the search screen.
2. The test-group work sheet prints and previews; the unfinished-only option restricts the sheet to tests without a result; the pending count and the printed sheet agree.
3. The print and preview pair is available in every sheet mode (visit, test-group, work-group).
4. The reference's double-press removal (2.2.1.3.3, 2.3.1.3.3) is listed as an open design question: the reference presupposes an editable patient/test list on the work sheet, but Top-Lab's sheet is a generated read-only report. Implementing the removal means building an editable list, a larger change than the other items in this stage. It is flagged as an open design question, not assumed.

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
**Required only for GAP-26**, and only if the reference's deletion behaviour is adopted literally. Hard deletion of a payment operation changes the persisted account history, and the `PatientTest` and `TestGroup` rows would need a delete path in place of their active flag. The audit does not decide soft vs hard deletion: that is an **owner decision** (the audit-trail argument for soft deletion is the reason Top-Lab behaves as it does). The migration assessment is therefore conditional and is flagged in §10 (owner decisions).

**NOT INDICATED** for undo, membership editing, antibiotic print, history clearing, and the range-confirm step.

### 7.5 Acceptance criteria
1. On each of the six editing surfaces, begin an edit, expect an undo action, and expect the surface to return to its state before the edit began; undo leaves the persisted records unchanged.
2. Open a test group for editing, expect the group's test membership listed, add a test and expect it to appear in the group's count, remove a test and expect the count to fall, and confirm the membership survives a reload.
3. The antibiotics screen exposes a print action; print and expect a document listing the antibiotics with their commercial names.
4. Record an account operation and delete it; expect the operation to be absent from the account statement and the totals to reconcile. The choice between soft and hard delete is an owner decision, so the implementation must meet the acceptance criterion that matches the recorded decision.
5. Clear a history report's selection and abandon a history operation; the selection is empty and the operation is abandoned without applying it.
6. Apply reference-range limits, expect a distinct confirm action to commit them, and expect the committed range to be persisted only after the confirm.

---

## 8. Stage R6 — Utilities, statistics, auditing, sent-out samples, sample withdrawal and settings

**Associated confirmed gaps:** GAP-16, GAP-18, GAP-19, GAP-20, GAP-21, GAP-29
**Canonical reference identifiers covered:** `1.16.1.1.2`, `1.16.1.1.3`, `1.16.1.1.8`, `1.16.1.2.2`, `5.6.2.3`, `4.5.1.2.2`, `4.5.1.2.3`, `4.5.1.2.4`, `6.3.1.6.4`, `6.3.1.6.5`, `8.2.1.7.2`, `8.2.1.8.1`, `8.2.1.9.1`, `8.2.1.9.2`, `8.2.1.9.3`, `8.2.1.9.4`, `8.2.1.10.3`, `7.2.1.9.2`, `7.2.5.7.2`, `7.2.5.8.3`, `7.2.5.8.4`, `7.2.5.8.5`, `7.2.5.8.6`, `7.6.1.4.2`, `7.6.1.4.3`, `7.6.1.4.5`, `7.6.1.4.6`, `7.6.1.4.8`, `7.6.1.4.9`, `7.5.1.8.2`, `7.5.1.8.3`, `7.5.1.8.8`, `7.5.1.8.9`
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
- **GAP-19** — the user filter over the audit records itself needs no schema change, but the delete-period operation removes persisted audit rows and therefore needs a deletion path and, by extension, a retention decision.
- **GAP-20** — the always-to destination persists a default-destination setting and requires a column.
- **GAP-29** — the colour values and the layout defaults are persisted settings with no column today.

**NOT INDICATED** for the remainder (utility surfaces, statistics save, sample-withdrawal remove/save/sticker-print, calendar).

### 8.5 Acceptance criteria
1. The utilities window exposes the four reference surfaces; the image library lists and opens images; the abbreviations dictionary is searchable; the data manual is searchable by word or value.
2. Load a census, expect a save action on the preview, save and expect the rendered report to be persisted, and expect the saved artefact to match the preview.
3. Load the login records for a period, expect a user selector, select two users and expect only their records, expect a show-all control to restore the full period, and expect a delete-period action that removes the chosen period's records.
4. Ignore a selected sample (takes it off the sending list without sending) and the always-to control persists the destination for a sample or a referring entity; a subsequent sending pre-fills the persisted destination.
5. The sample draw board exposes remove, save, sticker-print (all and container-filtered) and a month calendar that steps forward and back.
6. The settings module exposes a colour chooser with confirm, cancel and close; a restore-default-layout action; receipt header/footer image browse and clear; envelope header image, footer image and laboratory logo browse and clear; and the report tab carries a laboratory logo browse and clear. Every setting persists across a reload.

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
| GAP-26 | R5 | `1.3.2.3`, `3.2.1.4.5`, `3.6.1.4.4` | §7.5 (owner decision) |
| GAP-27 | R5 | `1.15.4.5`, `1.15.3.4` | §7.5 |
| GAP-28 | R5 | `3.3.1.3.11` | §7.5 |
| GAP-29 | R6 | `7.2.1.9.2`, `7.2.5.7.2`, `7.2.5.8.3/4/5/6`, `7.6.1.4.2/3/5/6/8/9`, `7.5.1.8.2/3/8/9` | §8.5 |
| GAP-30 | R1 | `1.22.12.5` | §3.5 |

---

## 10. Owner decisions that block or gate stages

The following are **not** product, security, policy or scope decisions that can responsibly be inferred from code or documentation. Each blocks its stage until the owner records a decision. For each, the options, functional consequences, security consequences and the audit's recommendation are stated.

| # | Decision | Stage | Impact | Options | Functional consequence | Security consequence | Recommendation |
|---|---|---|---|---|---|---|---|
| OD-1 | Whether to implement the reference's per-patient print-password requirement, and if so whether to use recoverable storage (encryption) rather than a one-way hash, or record GAP-03 as an accepted deviation | R2 | Blocks GAP-03; the encryption alternative changes the security posture | (a) Recoverable storage (encryption) and print the plaintext; (b) Keep one-way hash, print the hash; (c) Documented deviation | (a) Implements the reference as specified; (b) fails to implement the reference's stated receipt content; (c) accepts the deviation | (a) Reversible storage requires key management; (b) does not weaken security; (c) no new exposure | **Recommend (c) unless the owner explicitly accepts the key-management burden of (a).** Option (b) is not recommended because it produces a receipt the patient cannot use. |
| OD-2 | Whether to adopt the reference's deletion behaviour literally (hard delete of payment operations, tests, test groups) or keep soft deletion with an accepted deviation | R5 | Blocks GAP-26 and its migration assessment | (a) Hard delete; (b) Keep soft delete with documented deviation | (a) Operation absent from statement and totals reconcile; (b) operation stays flagged 'ملغاة' | (a) Removes audit-trail rows; (b) preserves audit trail | **Recommend (b).** The audit-trail argument for soft deletion is the reason Top-Lab behaves as it does; hard deletion is a policy decision the owner must make explicitly. |
| OD-3 | Whether to build an editable patient/test list for the work-sheet double-press removal, and how | R4 | Changes GAP-17's scope; the removal is not assumed | (a) Build editable list; (b) Leave as generated read-only report with documented deviation | (a) Larger change; enables double-press removal; (b) no change to current behaviour | (a) Larger attack surface for data entry errors; (b) none | **Recommend (b) unless the owner confirms the reference's double-press removal is required.** |
| OD-4 | The retention policy for audit-period deletion (which rows are retained, under what policy) | R6 | Blocks the GAP-19 deletion-path implementation | (a) Define retention policy; (b) Defer deletion path | (a) Enables delete-period; (b) leaves audit rows in place | (a) Requires a documented retention policy; (b) none | **Recommend (a) with an explicit retention policy recorded before implementation.** |
| OD-5 | The colour-chooser interaction model and the default-layout restore target | R6 | Affects GAP-29 acceptance criteria | (a) Implement colour chooser with confirm/cancel/close; (b) Defer | (a) Matches reference vocabulary; (b) leaves settings unconfigurable | (a) None; (b) none | **Recommend (a) with the reference's own vocabulary (confirm, cancel, close).** |
| OD-6 | The exact behaviour of the sent-out "always to" default-destination preference | R6 | Blocks GAP-20 acceptance criteria | (a) Persist default destination per sample/entity; (b) Defer | (a) Subsequent sending pre-fills; (b) no pre-fill | (a) None; (b) none | **Recommend (a) with a column on the sent-out sample / referring entity.** |
| OD-7 | Whether the status criteria (not-entered, not-verified, not-printed, not-delivered) are added to the search screen, moved from the results-worklist screen, or kept in both | R3 | Affects the counting rule and the search-screen design; must be recorded | (a) Add to search screen; (b) Move from results-worklist; (c) Keep in both | (a) Search screen gains 3 criteria; (b) results-worklist loses them; (c) duplication | (a)/(b)/(c) none | **Recommend (a) for not-delivered only, keeping the other three on the results-worklist screen, unless the owner wants full parity.** |
| OD-8 | Whether to extend the profile/culture screens with the print/preview/review/main-result actions or to add them to the single-test screen (GAP-22, GAP-30) | R1 | Affects GAP-22 and GAP-30 scope | (a) Extend profile/culture screens; (b) Add to single-test screen only | (a) Broader change; (b) narrower change | (a)/(b) none | **Recommend (a) for parity, but the two paths must not diverge; record the decision before implementation.** |

---

## 11. Verification requirements (without claiming the work is done)

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

## 12. End of roadmap

*This document is a planning artifact only. No stage in it has been implemented. Every acceptance criterion must be re-checked against the code as it stands at the time of implementation.*

---

*End of reconciled implementation roadmap.*
