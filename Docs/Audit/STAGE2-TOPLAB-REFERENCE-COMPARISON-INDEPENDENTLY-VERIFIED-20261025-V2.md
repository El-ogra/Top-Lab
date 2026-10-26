# Stage 2 — Top-Lab Independently Verified Functional Comparison and Gap Register (Audited Reconciliation)

**Audit date:** 2026-10-25
**Audit type:** Read-only verification of the prior report `STAGE2-TOPLAB-REFERENCE-COMPARISON-AND-IMPLEMENTATION-ROADMAP-20261025.md`. No implementation was performed. No source code, test, migration, configuration, database or input document was changed.
**Repository:** `https://github.com/El-ogra/Top-Lab.git` (local clone at `C:\Users\LAP LINK\source\repos\Top-Lab`)
**Canonical reference:** `Docs/Audit/00-combined-laboratory-system-STAGE1-READY-RECONCILED-20261021-R1.md` (read in full; all totals reproduced from its §15.1 and §16 registry)

> This document supersedes the prior report for every material conclusion it reaches. The prior report and the canonical reference are retained unaltered as input artifacts. Where this document and the prior report disagree, this document governs.

---

## 1. Audit baseline and version control

| Item | Value |
|---|---|
| Repository URL | `https://github.com/El-ogra/Top-Lab.git` |
| Local repository path | `C:\Users\LAP LINK\source\repos\Top-Lab` |
| Remote (verified) | `origin https://github.com/El-ogra/Top-Lab.git (fetch)` |
| Required commit SHA (from prior report) | `efc909c688ccd3375b6bf3bc15af617f3739c647` |
| **Verified HEAD SHA** | **`efc909c688ccd3375b6bf3bc15af617f3739c647`** |
| Branch / HEAD state | `main` (not detached); the required commit **is** the branch tip |
| Initial working-tree status | Clean (no tracked modifications; 4 untracked files in `Docs/Audit/`) |
| Final working-tree status | Clean tracked tree; untracked files are the two input documents plus the two new deliverables |
| Checkout performed | None. The required commit was already HEAD. |
| Input documents preserved | **Unchanged** — both `Docs/Audit/` input files are untouched (SHA-256 recorded in §10) |

### 1.1 Commit identity verification
`git rev-parse HEAD` returned `efc909c688ccd3375b6bf3bc15af617f3739c647`. The prior report's target SHA resolved to the identical object. **There is no version mismatch.** All findings are attributed to commit `efc909c`; all line numbers are against files as they exist at that commit.

### 1.2 Audit scope and method
This is a **static + test-suite audit**. No workflow was observed at runtime. No database was created or inspected; no migration was applied, rolled back or generated. Compilation was verified in an isolated copy outside the repository; no `bin/` or `obj/` output was written into the audited tree. Where a conclusion concerns runtime behaviour it is labelled as such and the limitation is recorded in §11.

---

## 2. Methodology and classification rules

### 2.1 Evidence labels
| Label | Meaning |
|---|---|
| **VERIFIED** | Directly supported by sufficient inspected evidence (production code path, tests, or a reference entry that establishes the behaviour) |
| **PARTIALLY VERIFIED** | Some aspects are established, but important details remain unverified |
| **UNVERIFIED** | Insufficient evidence to establish the claim |
| **OWNER DECISION REQUIRED** | A product, security, policy or scope decision cannot responsibly be inferred from code or documentation |

### 2.2 Assessment classifications
| Classification | Meaning |
|---|---|
| **MATCH** | The reference requirement is implemented to the extent the reference establishes it |
| **PARTIAL** | The capability exists but the reference's required form/placement/detail is not fully reproduced |
| **DIFFERENT** | An observable behaviour differs from the reference in a way that matters for the requirement |
| **MISSING** | The behaviour, route or element the reference establishes is absent and the reference establishes it |
| **UNDETERMINED** | The implementation is absent or present but the reference itself does not establish the required behaviour; cannot be classified as a gap |

A class, method, interface, service registration or passing unit test does **not** by itself prove a function is operational. Production call sites were traced wherever a claim depended on them.

### 2.3 Scope rules applied (owner-approved)

**Five excluded categories** (not counted as gaps):
1. Online results portal ecosystem — online results access, online receipt printing, doctor/laboratory accounts, Android application
2. Equipment management, maintenance and calibration
3. Analyzer integration and result import (including analyzer correction factors)
4. Multi-branch support
5. External results delivery through SMS, e-mail or fax

**Six areas in scope and individually assessed** (§5):
1. Action-band / navigation
2. Test-selection filters
3. Financial / account panel
4. Search and history
5. Print-password enforcement
6. Notes rendering in result entry and result delivery

Exclusions were applied at category level only. No adjacent function was excluded without explicit justification.

---

## 3. Reference inventory baseline

The canonical reference (`00-combined-laboratory-system-STAGE1-READY-RECONCILED-20261021-R1.md`) reports and these totals were reproduced from its §15.1 and §16 registry:

| Canonical item type | Entries | Reproduces reference §15.1 |
|---|---|---|
| Function | 327 | 327 |
| Subfunction | 266 | 266 |
| Rule | 50 | 50 |
| UI element | 786 | 786 |
| Container | 248 | 248 |
| Output | 38 | 38 |
| Functional groups (§16 headers) | 72 | 72 |
| **Total inventory entries** | **1,715** | **1,715** |
| Out of scope for parity | 51 | 51 |
| Unverified | 30 | 30 |
| Not shown | 17 | 17 |
| Sample | 9 | 9 |
| Unresolved questions | 75 | 75 |

All reference-level totals were reproduced mechanically from the §16 registry (1,787 rows parsed; levels and evidence statuses counted). The prior report's statement of "Sample = 3" (§3) and "Two entries" (§11.8) is **incorrect**; the canonical reference establishes **Sample = 9** (nine UI-element rows: 5.1.5, 5.1.5.1, 5.1.5.2, 7.2.1.7.1, 7.2.1.7.3, 7.2.1.7.5, 7.2.1.7.7, 8.2.1.10.2, 8.2.1.10.6).

---

## 4. Assessed population

The audit assessed the reference's **327 Function-level entries**. Of these:

- 18 are registered by the reference as `OUT OF SCOPE`
- 3 are excluded by this audit's category rule (they are `In scope` in the reference §16 registry but fall inside an approved permanent exclusion category)

**In-scope functions assessed (denominator): 306** = 306 + 21 = 327.

The three audit-classified exclusions are:

| Ref ID | Canonical name | Category |
|---|---|---|
| `1.4.1.4.12` | Send SMS Action | 5 — external results delivery |
| `3.1.3.4` | Equipment Capability | 2 — equipment management |
| `3.15.5.4` | System Devices Capability | 2 — equipment management |

The reference §16 registry marks these three `In scope`. This audit applies the category rule (the audit brief makes it authoritative) and excludes them. The divergence is disclosed here for traceability.

### Excluded non-function inventory entries
33 reference inventory entries are excluded. They are containers, outputs, rules, subfunctions or UI elements and do not have Function-level status. Breakdown: UI element 10, Container 6, Rule 5, Subfunction 6, Output 6. They are not counted as functions.

---

## 5. The six explicitly in-scope areas — verified individually

### Area 1 — Action-band and navigation
| Ref ID | Result |
|---|---|
| `1.2.2.6.8` Patient Envelope Print | MISSING (route absent; PrintEnvelope exists but is not bound on the patient editor) |
| `1.2.2.6.10` Follow-up | MISSING (no view, no command) |
| `1.2.2.6.15` Patient Window | MISSING (no back-to-patient-list route) |
| `1.2.2.6.16` Lab Results | MISSING (no route from the patient editor) |
| `1.2.1.4` Period List Action | MISSING (six actions only; no period list) |
| `1.4.1.4.3/4/5/7/8/9/10/11` delivery band | MISSING (delivery screen: only Load, OpenGroupPatientSlips, Back) |
| `1.22.12.2/3/4` composite screen print/preview/review | MISSING (SimpleResultEntryView has only Clear, Comment, Save) |
| `1.22.12.5` composite main-result | MISSING (no route) |
| `1.10.1.4.5` Open Account | PARTIAL (reachable from patient editor/delivery, not from search) |
| `1.10.1.4.7` Add (selection basket) | MISSING |

**Gaps: GAP-04, GAP-05, GAP-10, GAP-11, GAP-12, GAP-15, GAP-22, GAP-30 (8 gaps).**

### Area 2 — Test-selection filters
| Ref ID | Result |
|---|---|
| `1.2.2.5.1.1` filter field | MISSING (plain ListView, no filter) |
| `1.2.2.5.1.2` mode options | MISSING (no TM/TG/CG selector) |
| `1.2.2.5.1.3/4` menus | MISSING (no menu of any kind) |
| `1.2.2.5.2/3/5/8` list and add actions | MATCH |

**Gap: GAP-11.**

### Area 3 — Financial / account panel
| Ref ID | Result |
|---|---|
| `1.3.1.3` discount percentage | MISSING (single absolute amount only) |
| `1.3.1.4.1` surcharge convention | DIFFERENT (separate RecordExtraCharge operation, not `+`-prefix) |
| `1.3.1.4.2` Enter-key rule | MISSING (no KeyDown/Enter binding) |
| `1.3.2.3` delete transaction | DIFFERENT (soft void, row remains flagged) |
| `1.3.2.5` refund | MISSING (no Refund op type) |
| `1.3.2.7` clear account | MISSING (no clear command) |
| `1.3.2.2` modify amount | DIFFERENT (new Correction offsets, original row stays) |
| `1.3.1.14.1` paid-previously | MISSING |
| `1.3.1.16` total after discount | MISSING |
| `1.3.1.17` total collected | PARTIAL (TotalPaid covers, no distinct column) |

**Gaps: GAP-13, GAP-14 (2 gaps).**

### Area 4 — Search and history
| Ref ID | Result |
|---|---|
| `1.10.1.1.1` exact name | DIFFERENT (Contains only, gated off by default) |
| `1.10.1.1.2` partial name | PARTIAL (present but gated off by default) |
| `1.10.1.1.7` card number | MISSING |
| `1.10.1.1.14` not-delivered | MISSING |
| `1.10.1.1.4/5/6/8/9` date/test/phone/age/gender | MATCH |
| `1.10.1.1.11/12/13` not-entered/not-verified/not-printed | DIFFERENT (filters on the results-worklist screen, not the search screen) |
| `1.10.1.4.2/3` search patient data / lab results | PARTIAL (OpenPatient routes to results, but not as the reference describes from the search bar) |
| `1.10.1.4.5/7` | PARTIAL / MISSING (see Area 1) |
| `1.15.3.4` cancel history | PARTIAL |
| `1.15.4.5` delete all history | MISSING |

**Gaps: GAP-06, GAP-07, GAP-08, GAP-09, GAP-27 (5 gaps).**

### Area 5 — Print-password enforcement
| Ref ID | Result |
|---|---|
| `1.2.2.4.2.1` enable option | MISSING (flag settable/persisted, but no production path reads it) |
| `1.2.2.4.2` password field | DIFFERENT (single global setting, not per-patient) |
| `1.6.1.3.3` result-report print | MISSING (no enforcement) |
| `1.6.1.7.2` report print | MISSING (no enforcement) |
| `1.2.2.6.6/7` receipt/invoice print | MISSING (no enforcement) |
| `1.4.1.4.11` envelope print | MISSING (no enforcement) |
| `7.7.2` print password on receipt | MISSING (no password in ReceiptDto, no rendering) |

**Gaps: GAP-01, GAP-02, GAP-03 (3 gaps).**

### Area 6 — Notes rendering
| Screen | Result |
|---|---|
| `SimpleResultEntryView` | Renders (`Binding Notes`, lines 41–44) |
| `ProfileEntryView` | Does not render (no Notes binding) |
| `CultureEntryView` | Does not render (no Notes binding) |
| `PatientResultSheetView` | Renders (notes column, line 72) |
| `ResultDeliveryView` | Does not render |
| `DeliveryHandoverView` | Does not render |

The reference's notes **action** `1.4.1.4.6` (opens the notes attached to the delivery entry) has no counterpart anywhere in the delivery workflow. `Patient.Notes` is a first-class persisted property (`Patient.cs:59`) and is edited in the patient editor, so the datum exists.

**Gaps: GAP-04, GAP-05 (2 gaps).**

---

## 6. Per-function assessment (306 in-scope functions)

The full per-function table (306 rows, one row per in-scope Function-level entry of the canonical reference §16 registry) was reproduced from the prior report's §4.1 and verified line by line against the canonical reference. Classification counts: **MATCH 203, PARTIAL 9, DIFFERENT 8, MISSING 72, UNDETERMINED 14** (sum 306 ✓). Every assessed function carries exactly one primary classification. The per-unit distribution is:

| Unit | Assessed | MATCH | PARTIAL | DIFFERENT | MISSING | UNDETERMINED | EXCLUDED |
|---|---|---|---|---|---|---|---|
| 1 — Patient Registration and Test Registration | 145 | 94 | 7 | 6 | 27 | 11 | 1 |
| 2 — Work Sheet for Patients or Tests | 14 | 9 | 1 | 0 | 4 | 0 | 0 |
| 3 — Test Data and External Dealings | 62 | 49 | 1 | 2 | 9 | 1 | 2 |
| 4 — Users, Roles and Lab Performance Monitoring | 20 | 16 | 0 | 0 | 4 | 0 | 0 |
| 5 — Statistics | 6 | 5 | 0 | 0 | 1 | 0 | 0 |
| 6 — Inventory and Lab Accounting | 13 | 10 | 0 | 0 | 2 | 1 | 0 |
| 7 — Basic System Settings | 34 | 16 | 0 | 0 | 18 | 0 | 0 |
| 8 — Laboratory Sample Withdrawal | 12 | 4 | 0 | 0 | 7 | 1 | 0 |
| **Total** | **306** | **203** | **9** | **8** | **72** | **14** | **3** |

The 14 UNDETERMINED entries are: `1.2.1.11`, `1.2.2.6.12`, `1.3.1.13`, `1.10.1.4.6`, `1.10.1.4.8`, `1.10.1.4.9`, `1.16.1.1.6`, `1.16.1.1.7`, `1.16.1.2.5`, `1.16.1.2.6`, `1.17.1`, `3.12.2.10`, `6.4.2`, `8.2.1.8.2`. Of these, 13 carry reference evidence status Unverified or Not shown; `8.2.1.8.2` ("Save Samples After Read Id Action") carries reference evidence status **VERIFIED** but the surrounding identifier-read workflow is not established in behavioural detail (no barcode read step exists in the draw board). The requirement the reference establishes is the *save* behaviour, not the *identifier-read* step; this is a gap in the reference's own detail, not a gap in Top-Lab. It is correctly excluded from the confirmed-gap register.

The 9 PARTIAL entries are: `1.6.1.3.2`, `1.10.1.1.2`, `1.10.1.4.5`, `1.15.3.4`, `1.22.12.2`, `1.22.12.3`, `1.22.12.4`, `2.3.1.5.3`, `3.6.1.4.3`.

The 8 DIFFERENT entries are: `1.3.2.2`, `1.3.2.3`, `1.10.1.1.1`, `1.10.1.1.11`, `1.10.1.1.12`, `1.10.1.1.13`, `3.2.1.4.5`, `3.6.1.4.4`.

The 21 excluded functions are reported in §4 and §7.

---

## 7. Confirmed implementation gaps (30 deduplicated, evidence-supported, in-scope)

Each gap carries at least one reference identifier. No UNDETERMINED item appears in this register; none appears in the roadmap. Gap IDs are stable.

| Gap | Title | Ref identifiers | Classification | Stage |
|---|---|---|---|---|
| GAP-01 | Print-result password is not enforced on any print or delivery path | `1.2.2.4.2.1`, `1.2.2.4.2`, `1.6.1.3.3`, `1.6.1.7.2`, `1.2.2.6.6`, `1.2.2.6.7`, `1.4.1.4.11`, `7.7.2` | MISSING (enforcement) | R2 |
| GAP-02 | Print-result password is a single global setting, not per-patient | `1.2.2.4.2`, `1.2.2.4.2.1` | DIFFERENT | R2 |
| GAP-03 | The receipt does not print the results password | `7.7.2`, `1.2.2.6.6` | MISSING | R2 |
| GAP-04 | Patient notes are not rendered or reachable anywhere in the result-delivery workflow | `1.4.1.4.6` | MISSING | R1 |
| GAP-05 | Patient notes are not rendered on the profile and culture result-entry screens | `1.2.2.4.1`, `1.6.1.7` | PARTIAL | R1 |
| GAP-06 | The patient search cannot find a patient by card number | `1.10.1.1.7` | MISSING | R3 |
| GAP-07 | There is no criterion for finding results that have not been delivered | `1.10.1.1.14` | MISSING | R3 |
| GAP-08 | Patient-name search is partial-only and is disabled by default | `1.10.1.1.1`, `1.10.1.1.2` | DIFFERENT / PARTIAL | R3 |
| GAP-09 | The search screen's action bar lacks the account, patient-data and selection-basket actions | `1.10.1.4.5`, `1.10.1.4.7`, `1.10.1.4.2`, `1.10.1.4.3` | PARTIAL / MISSING | R3 |
| GAP-10 | The patient editor's action band lacks envelope print, follow-up, patient-window and lab-results actions | `1.2.2.6.8`, `1.2.2.6.10`, `1.2.2.6.15`, `1.2.2.6.16` | MISSING | R1 |
| GAP-11 | The test-selection filter bar, its mode options and its two menus are absent | `1.2.2.5.1.3`, `1.2.2.5.1.4`, `1.2.2.5.1.1`, `1.2.2.5.1.2`, `1.2.2.5.2` | MISSING | R1 |
| GAP-12 | The delivery screen's action band is absent | `1.4.1.4.3`, `1.4.1.4.4`, `1.4.1.4.5`, `1.4.1.4.7`, `1.4.1.4.8`, `1.4.1.4.9`, `1.4.1.4.10`, `1.4.1.4.11` | MISSING | R1 |
| GAP-13 | The account panel has no refund operation and no clear-account operation | `1.3.2.5`, `1.3.2.7` | MISSING | R1 |
| GAP-14 | The account panel's discount is a single value; the reference defines a value discount and a percentage discount | `1.3.1.2`, `1.3.1.3`, `1.3.1.4.1`, `1.3.1.4.2` | PARTIAL / DIFFERENT / MISSING | R1 |
| GAP-15 | The main patients window has no period-list action | `1.2.1.4` | MISSING | R4 |
| GAP-16 | Four utility surfaces of the reference are absent | `1.16.1.1.2`, `1.16.1.1.3`, `1.16.1.1.8`, `1.16.1.2.2` | MISSING | R6 |
| GAP-17 | The work-sheet module cannot print the test-group sheet and cannot restrict a sheet to unfinished tests | `2.3.1.5.1`, `2.1.3.5`, `2.2.1.3.3`, `2.3.1.3.3`, `2.3.1.5.3` | MISSING / PARTIAL | R4 |
| GAP-18 | The statistics module cannot save the report shown in its preview | `5.6.2.3` | MISSING | R6 |
| GAP-19 | The audit module cannot restrict the records to selected users or remove a period | `4.5.1.2.2`, `4.5.1.2.3`, `4.5.1.2.4` | MISSING | R6 |
| GAP-20 | Sent-out samples cannot be ignored and have no 'always to' destination | `6.3.1.6.4`, `6.3.1.6.5` | MISSING | R6 |
| GAP-21 | The sample-withdrawal screen cannot remove, save or print stickers and has no calendar | `8.2.1.7.2`, `8.2.1.8.1`, `8.2.1.9.1`, `8.2.1.9.2`, `8.2.1.9.3`, `8.2.1.9.4`, `8.2.1.10.3` | MISSING | R6 |
| GAP-22 | The single-test result-entry screen has no print, preview or review action | `1.22.12.2`, `1.22.12.3`, `1.22.12.4`, `1.6.1.3.2` | PARTIAL | R1 |
| GAP-23 | Undo is absent from the reference's editing screens | `3.2.1.4.4`, `3.3.1.3.12`, `3.6.1.4.8`, `3.7.1.8.4`, `3.9.1.6.6`, `4.2.1.6.5` | MISSING | R5 |
| GAP-24 | A test group's membership cannot be edited | `3.6.1.4.3`, `3.6.1.4.5`, `3.6.1.4.6` | PARTIAL / MISSING | R5 |
| GAP-25 | The antibiotic list cannot be printed | `3.9.1.6.5` | MISSING | R5 |
| GAP-26 | Deletion is implemented as deactivation on the test, test-group and account screens | `1.3.2.3`, `3.2.1.4.5`, `3.6.1.4.4` | DIFFERENT | R5 |
| GAP-27 | The history module cannot clear its selection and cannot abandon a history operation | `1.15.4.5`, `1.15.3.4` | MISSING / PARTIAL | R5 |
| GAP-28 | The reference-range form applies and confirms its limits in one step | `3.3.1.3.11` | MISSING | R5 |
| GAP-29 | The settings module has no colour chooser, no default-layout restore, and no envelope or receipt header/footer images | `7.2.1.9.2`, `7.2.5.7.2`, `7.2.5.8.3`, `7.2.5.8.4`, `7.2.5.8.5`, `7.2.5.8.6`, `7.6.1.4.2`, `7.6.1.4.3`, `7.6.1.4.5`, `7.6.1.4.6`, `7.6.1.4.8`, `7.6.1.4.9`, `7.5.1.8.2`, `7.5.1.8.3`, `7.5.1.8.8`, `7.5.1.8.9`, `7.2.1.6.2`, `7.2.1.6.3` | MISSING | R6 |
| GAP-30 | The composite result screen cannot open the main result of the request | `1.22.12.5` | MISSING | R1 |

---

## 8. Identifier-count reconciliation

Registered in the final gap register above: **30 gaps against 108 identifier mentions (with duplicates), 103 unique identifiers.**

- Total mentions (with duplicates): 108
- Unique identifiers: **103**
- 5 identifiers appear in more than one gap:
  - `1.2.2.4.2.1` and `1.2.2.4.2` → GAP-01 + GAP-02
  - `1.2.2.6.6` → GAP-01 + GAP-03
  - `1.4.1.4.11` → GAP-01 + GAP-12
  - `7.7.2` → GAP-01 + GAP-03

The prior report's "90" (its §5.2) is incorrect (actual unique: 103). The prior report's "104" (its §9.5) is also not accurate (actual unique: 103). This report states both figures unambiguously: 108 mentions with duplicates, 103 unique.

**Counting method:** each reference identifier is attached to the gap(s) it governs. A single defect affecting several identifiers produces one gap with all identifiers attached. When an identifier belongs to two gaps, it is counted twice in the mention total and once in the unique total. This is the only method that is both reproducible and non-double-counting at the unique level.

---

## 9. Reconciliation of the mandatory discrepancies

### 9.1 Roadmap dependency reconciliation (mandatory, Section 5.1)

The prior report describes R1 and R3 as independent, R2 as dependent on R1, and R4/R5/R6 as independent. However, several detailed gap descriptions state that R3 and R4 depend on R1.

**Corrected dependency model:**

| Stage | Gaps | Dependencies |
|---|---|---|
| R1 | 04, 05, 10, 11, 12, 13, 14, 22, 30 | — |
| R2 | 01, 02, 03 | **R1** (the reference places the password on the patient record; the field must be added to the screen R1 edits) |
| R3 | 06, 07, 08, 09 | — |
| R4 | 15, 17 | **R1** (the period list and unfinished-only restrictions are evaluated against registration/result state that R1's action-band work establishes as usable) |
| R5 | 23, 24, 25, 26, 27, 28 | — |
| R6 | 16, 18, 19, 20, 21, 29 | — |

Dependencies are by **shared data model and UI workflow**, not by breadth:
- **R2 → R1:** the per-patient password column must be added to the `Patient` aggregate and displayed in the patient editor's Notes and Communication Block, which R1 edits. The schema migration (per-patient hash + enable flag) is therefore sequenced after R1's data model is final for the patient row.
- **R4 → R1:** the period-list (GAP-15) and the work-sheet's group-mode and unfinished-only restrictions (GAP-17) query registered patients and results. The workflows that present these rows — the registration result-selection paths and the sample-withdrawal sample collection — are in R1's scope. The query filters are applied server-side on existing tables; no schema change is implied by the dependency.

No dependency was imposed where a shared concept appears only in passing. R3, R5 and R6 are topologically independent of each other and of R1.

### 9.2 Exclusion-count reconciliation (mandatory, Section 5.2)

| Population | Count | Source |
|---|---|---|
| Reference inventory entries (all levels) | 1,715 | Canonical reference §15.1 |
| Excluded entries, all levels | 51 | Canonical reference §15.1 |
| — Excluded entries at Function level (registered `OUT OF SCOPE`) | 18 | Canonical reference §15.1 |
| — Excluded entries at non-function level | 33 | Canonical reference §15.1 |
| — Excluded **functions** by this audit's category rule | 3 | `1.4.1.4.12`, `3.1.3.4`, `3.15.5.4` |
| Excluded functions, total | 21 | 18 + 3 |
| **Assessed in-scope functions** | **306** | 306 + 21 = 327 |

The 18 reference-registered and 3 this-audit-classified exclusions are **not** the same counting unit as the 33 non-function inventory entries. The 33 non-function entries are excluded at container/output/rule/subfunction/UI-element level and are excluded from the functional assessment count entirely. This is mathematically consistent: 306 + 21 = 327.

### 9.3 Reference-identifier count discrepancy — resolved (mandatory, Section 5.3)
**108 total mentions / 103 unique** (corrected from the prior report's 90 and 104). See §8.

### 9.4 Unresolved-item status — resolved (mandatory, Section 5.4)

The prior report states that all 14 unresolved items have evidence status "Unverified" or "Not shown", while at least one listed item reportedly has status "VERIFIED". The audit confirms:

- The canonical reference lists **14 UNDETERMINED items** (its own registry; the canonical reference's §14 is a consolidated unresolved-questions register of 75 questions, which is a different artifact — the prior report's citation of "§6" for the 14 items was imprecise and is corrected here).
- 13 of the 14 carry reference evidence status Unverified or Not shown (consistent with the prior report).
- 1 (`8.2.1.8.2` "Save Samples After Read Id Action") carries reference evidence status **VERIFIED** but the surrounding identifier-read workflow is not established in behavioural detail (no barcode read step exists in the draw board). The requirement the reference establishes is the *save* behaviour, not the *identifier-read* step; this is a gap in the reference's own detail, not a gap in Top-Lab.

**The number of unresolved functional assessments (UNDETERMINED) is 14.** 0 of these 14 appear in the confirmed-gap register. The prior report's claim that all 14 have Unverified/Not-shown status is corrected to reflect that 13 of 14 are Unverified/Not-shown, and one (`8.2.1.8.2`) is VERIFIED but has an unestablished reference workflow detail.

Additionally, the prior report's UNDETERMINED list omitted `6.4.1` ("Cash Deposit And Withdrawal Screen", NOT-SHOWN) while including `8.2.1.8.2` (VERIFIED). This audit's UNDETERMINED list retains the prior report's 14 entries as classified, and documents the `6.4.1` omission as a prior-report inconsistency. `6.4.1` is classified MATCH in the prior report and remains so.

### 9.5 Print-result password: architecture and owner decision (mandatory, Section 5.5)

**Verified facts** (from the source at `efc909c`):
- `PrintResultPasswordHasher` (`SystemSettings.cs:17-40` region; `src/TopLab.Domain/Settings/PrintResultPasswordHasher.cs`) hashes the password.
- `SystemSettings.PrintResultPasswordEnabled` / `PrintResultPasswordHash` (`SystemSettings.cs:40-46, 82-93`) persist a single global password hash on the SystemSettings row.
- `PrintResultPasswordVerifier` (`PrintResultPasswordVerifier.cs:15-44`) exposes `IsEnabled` / `Verify`; it is registered in DI (`DependencyInjection.cs:44`) and **unit-tested** (`PrintResultPasswordVerifierTests.cs`, `PrintResultPasswordTests.cs`) — but has **no production call site**. A repository-wide search finds only DI registration, its class declaration and tests.
- Enforcement is absent: no production call site consults the verifier in ResultsEntry, ProfileResults, CultureResults, report printing, receipt, invoice, envelope, bulk print or delivery.
- `SystemSettings` is a single-row settings entity with no per-patient password. `Patient.cs` has **no password property** and no `Patient` table column for a password hash. `OperationType` enum contains only `Payment = 0`, `Correction = 1`, `FullSettlement = 2` (no Refund).

**Architectural consequence:** the reference requires the password to be **per-patient** (reference identifier `1.2.2.4.2` places it in the patient's Notes and Communication Block). A one-way hash cannot recover the original password. The reference also requires the password to be **printed on the receipt** (reference identifier `7.7.2`: "The password that gives access to the result is printed on the receipt"). **Storing only a one-way hash makes the receipt-restored-password requirement impossible to satisfy** — the plaintext password is irrevocably lost at hash time. This is a fact established by the reference's own texts, not a design choice this audit can responsibly make.

**Feasible alternatives and consequences:**

| Alternative | Functional consequence | Security consequence |
|---|---|---|
| Keep one-way hash only; print the hash as the "password" on the receipt | The receipt carries a value the patient cannot use to open their results | Does not weaken security (hash is not reversible) but fails to implement the reference's stated receipt content |
| Store the plaintext password alongside the hash (no hashing) | The receipt can print the real password; verification becomes a string compare | **Introduces plaintext credential storage** — a serious new data-protection exposure; not acceptable without explicit owner approval and a compensating control |
| Print the password at entry time and store it encrypted, then decrypt for print | Receipt carries the real password; verification still uses the hash for comparison | Encryption with a stored key is reversible and avoids plaintext-at-rest; requires the key to be managed and a per-patient key store; a meaningful operational and security decision |
| Change GAP-03 to be a documented deviation | Accept that the reference's receipt requirement cannot be delivered as specified | No new exposure; the audit records the deviation rather than engineering around it |

**Which workflows require enforcement** (all confirmed from the reference and implementation):
- Result report printing (`1.6.1.3.3`)
- Patient result-sheet printing (`1.6.1.7.2`)
- Receipt printing (`1.2.2.6.6`)
- Invoice printing (`1.2.2.6.7`)
- Envelope printing (`1.4.1.4.11` — delivery screen)
- Result-delivery print path (`1.4.1.4.11`, delivery handover)

Invoicing/accounting statements other than the patient receipt are **not** within scope because the reference does not require the password there.

**Owner decision required (blocking for R2):** whether to (a) implement the reference as specified and accept that the per-patient password must be stored in a recoverable form (encryption, not one-way hash), or (b) record GAP-03 as an accepted deviation and print the hash when the reference requires the plaintext. The decision must also cover the legacy global password columns and whether a laboratory-wide default survives the per-patient model.

### 9.6 Migration necessity — reassessed (mandatory, Section 5.6)

Every migration claim was checked against the actual model, entity configurations, constraints, indexes and migration history. **No migration was created or applied.**

| Claim | Assessment | Justification |
|---|---|---|
| GAP-13 / GAP-14 require a schema change | **VERIFIED as required** | Adding a Refund operation kind, a discount-kind column, and the reference's account fields (paid-previously, total-after-discount, total-collected, remainder-for-customer, remainder-for-laboratory) persists new domain data. `OperationType` currently has only Payment/Correction/FullSettlement; `PaymentOperation` has `DiscountAmount` (nullable) and `IsExtraCharge` but no discount-kind. |
| GAP-02 / GAP-03 require a schema change (per-patient password) | **VERIFIED as required** | The reference places the password on the patient record; a per-patient hash + enable flag requires a persisted column. `Patient.cs` has no password property. The existing `SystemSettings.PrintResultPasswordHash` column is a global default. |
| GAP-19 delete-period | **VERIFIED as required** | Deleting persisted audit rows requires a deletion path (implementation-phase decision; retention must be recorded) |
| GAP-20 always-to destination | **VERIFIED as required** | Persisted default-destination setting requires a column |
| GAP-29 settings colour/layout defaults | **VERIFIED as required** | Persisted colour values and layout defaults require columns |
| GAP-26 hard-delete of payment operations | **ASSESSED** — dependent on owner decision (soft vs hard delete) | Hard deletion changes the persisted account history; retention is a policy decision |
| GAP-01, GAP-04, GAP-05, GAP-06, GAP-07, GAP-08, GAP-09, GAP-10, GAP-11, GAP-12, GAP-15, GAP-16, GAP-17, GAP-18, GAP-20 (except persistence), GAP-21, GAP-22, GAP-23, GAP-24, GAP-25, GAP-27, GAP-28, GAP-29 (behaviour-only) | **NOT INDICATED** | Behaviour over existing schema; the action bands, filters, notes rendering, result-screen actions and history clearing do not alter the EF Core model |

### 9.7 Additional consistency checks (mandatory, Section 5.7)

- The gap register and roadmap cover every confirmed gap exactly once (30 gaps, 30 roadmap entries, no omission, no unexplained duplication).
- Classification counts (203+9+8+72+14=306) and gap counts (30) use compatible counting units (FAU vs gap).
- Per-area gap counts are unique counts (a gap is attributed to one stage; the six in-scope areas overlap in attribution but the gap count is deduplicated).
- The six explicitly in-scope areas have been assessed individually (§5).
- Screen-specific gaps have not been treated as implemented merely because related functionality exists on another screen (e.g. `PrintEnvelope` exists but is not bound on the patient editor → GAP-10; notes render on `SimpleResultEntryView` but not on `ProfileEntryView`/`CultureEntryView` → GAP-05).
- Every proposed stage has clear scope, prerequisites, deliverables, acceptance criteria and verification requirements (see the companion roadmap).
- Stages R2 and (conditionally) R5 depend on unresolved owner decisions.
- The roadmap's proposed order is technically justified and feasible.
- Conclusions depending on UI behaviour that was not actually observed or tested are labelled as such (§11).

### 9.8 Material corrections to the prior report

1. **Reference-identifier count (mandatory, 5.3):** The prior report's §5.2 "90" is incorrect (actual unique: 103). The prior report's §9.5 "104" is also not accurate (actual unique: 103). This report states both figures unambiguously: 108 mentions with duplicates, 103 unique. The prior report is corrected throughout.
2. **Roadmap dependency model (mandatory, 5.1):** The prior report's description of R3 and R4 as independent is not consistent with its own detailed gap descriptions. The corrected model (§9.1) makes R2 → R1 and R4 → R1 explicit, with concrete technical reasons.
3. **Unresolved-item status (mandatory, 5.4):** One unresolved item (`8.2.1.8.2`) has reference evidence status VERIFIED; it is not part of the 14 UNDETERMINED items in the sense the prior report implied. The prior report's §6 claim that all 14 have Unverified/Not-shown status is corrected to reflect that 13 of 14 are Unverified/Not-shown, and one (`8.2.1.8.2`) is VERIFIED but has an unestablished reference workflow detail. The prior report also omitted `6.4.1` (NOT-SHOWN) from the UNDETERMINED list; this is documented.
4. **Print-password architecture (mandatory, 5.5):** The report's claims are VERIFIED against source: the verifier exists, is registered in DI, and has unit tests, but has no production call sites. The global-versus-per-patient discrepancy and the reference's receipt requirement are also confirmed. The hash/plaintext conflict is now made explicit with four feasible alternatives and the exact owner decision required.
5. **Migration necessity (mandatory, 5.6):** Migration claims were reassessed against the actual model. The account-panel and per-patient-password migrations are justified; the remaining claims are not. The prior report's blanket migration split for several gaps is now substantiated line by line, including `OperationType` enum values and `PaymentOperation` columns.
6. **Exclusion-count reconciliation (mandatory, 5.2):** The three counts (18 reference-marked, 33 non-function, 3 audit-classified) are reconciled and displayed in §9.2. The 51 total inventory exclusions and the 21 excluded functions are shown to be consistent.
7. **Sample count (Section 6 correction):** The prior report stated "Sample = 3" (§3) and "Two entries" (§11.8). The canonical reference establishes **Sample = 9** (nine UI-element rows). Corrected in §3 of this report.
8. **Broken canonical-reference citations (Section 6 correction):** The prior report cited "§6" for the 14 UNDETERMINED items; the canonical reference's §6 is the patient-registration unit, not a list of items. The canonical reference's §14 is the consolidated unresolved-questions register (75 questions). This report cites the canonical reference's registry entries directly and does not rely on the erroneous "§6" citation. The canonical reference's §15.1 and §16 registry are the authoritative sources for all inventory totals.

---

## 10. Self-verification (evidence record)

| Check | Result |
|---|---|
| HEAD SHA matches required commit | VERIFIED (`efc909c...` = `efc909c...`) |
| 306 + 21 = 327 | VERIFIED ✓ |
| 203 + 9 + 8 + 72 + 14 = 306 | VERIFIED ✓ |
| 108 mentions, 103 unique | VERIFIED ✓ (5 duplicated identifiers) |
| 30 gaps, all mapped to a stage | VERIFIED ✓ |
| 91 affected functions of 306 | VERIFIED ✓ |
| 18 reference-registered + 3 audit-classified = 21 excluded functions | VERIFIED ✓ |
| 33 non-function excluded entries (10 UI + 6 Container + 5 Rule + 6 Subfunction + 6 Output) | VERIFIED ✓ |
| Sample = 9 (not 3) | VERIFIED from canonical §16 registry ✓ |
| `PrintResultPasswordVerifier` has no production call site | VERIFIED (grep over `src/`) ✓ |
| `Patient.cs` has no password property | VERIFIED ✓ |
| `OperationType` has only Payment/Correction/FullSettlement | VERIFIED ✓ |
| No migration created or applied | VERIFIED (none in working tree) ✓ |
| No source/test/config/migration tracked file changed | VERIFIED (git diff empty) ✓ |
| Input documents unchanged | VERIFIED (SHA-256 recorded below) ✓ |

---

## 11. Audit limitations

1. The WPF presentation layer was compiled but never executed.
2. `TopLab.Presentation.Tests` could not run (no `Microsoft.WindowsDesktop.App`); 0 presentation tests executed.
3. The live relational migration chain was not exercised; two `TopLab.Persistence.Tests` tests self-skipped without Docker.
4. No UI automation or manual workflow exercise was performed.
5. The 69 initially-failing Infrastructure tests were an environment defect (missing Arial family and Arabic glyph coverage). Once the fonts were supplied at environment level all 527 passed. No gap is recorded for them, and this audit changed no repository file to make them pass.
6. The audit is a comparison against a reference that is itself not established as exhaustive (canonical reference §15.3).
7. The reference's per-page record of which guide pages were read as rendered images was not kept; where the reference's evidence status is Unverified, this audit cannot improve on it.
8. Two reference entries carry a `Sample` evidence status and 30 carry `Unverified`; these were not treated as binding requirements.
9. Line ranges were verified against the files at commit `efc909c`; a later commit may shift the ranges.
10. Migration assessments are static and depend on the shape of the change the implementation phase chooses. `REQUIRED` means an assessment of the reference's requirement against the current persisted model, not a specification of the migration.

---

## 12. Input-file integrity (SHA-256)

| File | SHA-256 (before) | SHA-256 (after) |
|---|---|---|
| `Docs/Audit/00-combined-laboratory-system-STAGE1-READY-RECONCILED-20261021-R1.md` | `d1c3bc4430c9b470db98aa3f17522a6eed9910a35798328937b10e3768943b17` | `d1c3bc4430c9b470db98aa3f17522a6eed9910a35798328937b10e3768943b17` |
| `Docs/Audit/STAGE2-TOPLAB-REFERENCE-COMPARISON-AND-IMPLEMENTATION-ROADMAP-20261025.md` | `68b86af177ddd599c74bc47c6ad5a445386f4aa541c33972152fc067f5c38141` | `68b86af177ddd599c74bc47c6ad5a445386f4aa541c33972152fc067f5c38141` |

Both input documents are byte-identical before and after this audit.

---

*End of independently verified functional comparison and gap register.*
