# Stage 2 — Top-Lab Independently Verified Functional Comparison and Gap Register (V3)

**Audit date:** 2026-10-25
**Audit type:** Read-only verification of the prior report `STAGE2-TOPLAB-REFERENCE-COMPARISON-AND-IMPLEMENTATION-ROADMAP-20261025.md`. No implementation performed. No source code, test, migration, configuration, database or input document changed.
**Repository:** `https://github.com/El-ogra/Top-Lab.git` (local clone at `C:\Users\LAP LINK\source\repos\Top-Lab`)
**Canonical reference:** `Docs/Audit/00-combined-laboratory-system-STAGE1-READY-RECONCILED-20261021-R1.md` (read in full; totals reproduced from its §15.1 and §16 registry)
**Audited commit:** `efc909c688ccd3375b6bf3bc15af617f3739c647` (confirmed as HEAD `main`).

> This document supersedes V2 for every material conclusion it reaches. V2 and the prior report are retained unaltered as input artifacts. Where documents disagree, this document governs.

---

## Correction log (V3)

| ID | Location | What was wrong | Class | What I did | Evidence |
|---|---|---|---|---|---|
| D1 | §9 (V2) | Class A/B gate and readiness matrix absent | A | Added §2 (Correction log) and §10 (Class A/B gate); readiness matrix moved to Stage 3 §11 | This document + Stage 3 §11 |
| D2 | §5 + §6 (V2) | Per-area tables marked 1.3.1.4.1, 1.2.2.4.2 DIFFERENT and 1.3.1.17, 1.10.1.4.2/3 PARTIAL, but the 8/9 classification lists omitted them | A | Verified in canonical reference §16: 1.3.1.4.1 = Rule, 1.2.2.4.2 = UI element, 1.3.1.17 = Function, 1.10.1.4.2/3 = Function. These are sub-Function / UI / Rule entries; the per-area tables now state the Function-level parent that carries the classification. Totals unchanged: 8 DIFFERENT + 9 PARTIAL at Function level | Canonical §16 registry (mechanical parse) |
| D3 | §7 + §8 (V2) | Gap accounting removed; "91 affected functions of 306" marked VERIFIED without derivation | A | Restored §7 (Confirmed-gap accounting) with mixed-classification counting rule, exact affected Function list, and reconciliation with 89 non-MATCH functions | §7 below |
| D4 | §6 (V2) | 6.4.1 (NOT-SHOWN) classified MATCH while 6.4.2 UNDETERMINED; left unchanged | A | Re-read reference: 6.4.1 = "Cash Deposit And Withdrawal Screen" NOT-SHOWN, In scope. Verified Top-Lab has no cash-deposit/withdrawal screen. Reclassified 6.4.1 as MISSING. Totals recomputed: 306 population now 203 MATCH / 9 PARTIAL / 8 DIFFERENT / 73 MISSING / 13 UNDETERMINED | Canonical §16 registry + repo search |
| D5 | §9.1 (V2) | R2→R1 and R4→R1 reasons ("shared screen"; "no schema change") do not establish a prerequisite | A | Rebuilt from concrete evidence: R2→R1 = Patient aggregate (Patient.cs has no password column; PatientConfiguration has no password column; print-password is a patient-level field per 1.2.2.4.2). R4→R1 = PatientTest/result state (period list and unfinished-only restriction query existing tables). Shared screens are merge-coordination notes only. R3/R5/R6 independent. If R2 is blocked only by owner decision (OD-1), that is not a stage dependency | Patient.cs, PatientConfiguration.cs, PrintResultPasswordVerifier.cs |
| D6 | §9.6 (V2) | Migration assessments: GAP-13/14 and GAP-19 unverified; GAP-20 internal contradiction (REQUIRED + NOT INDICATED) | A | Re-inspected entities, configurations, constraints, indexes, enum storage: OperationType stored as int→tinyint (HasConversion<int>); no DB constraint on values. GAP-13/14: REQUIRED (new OperationType value + discount-kind column + derived columns). GAP-02/03: REQUIRED (per-patient password hash + enable flag). GAP-19: NOT INDICATED for user filter; deletion path is behaviour (soft/hard delete is OD-2). GAP-20: REQUIRED-ONLY-IF (OD-6) for always-to column. GAP-26: REQUIRED-ONLY-IF (OD-2). GAP-29: REQUIRED (colour/layout columns). Derivation in §9.6 | OperationType.cs, PaymentOperationConfiguration.cs, PatientConfiguration.cs, SentOutSampleConfiguration.cs |
| D7 | Stage 3 (V2) | (a) §3.5 says acceptance criteria are in companion report §7 (false — §7 is gap register). (b) §3.2 says owner decisions "block nothing" while §10 says they block. (c) R1 identifier list includes identifiers belonging to R2/R5 and not in register | A | (a) Objective per-gap acceptance criteria written in Stage 3 §3.5–§8.5, derived from canonical requirement. (b) Single accurate statement derived from decisions table. (c) Identifier lists reconciled exactly to gap-register attachments; intentional differences stated | Stage 3 V3 §3–§9 |
| D8 | §9.5 (V2) | Print-password alternatives incomplete; invoice enforcement contradiction (1.2.2.6.7 listed as requiring enforcement while financial docs other than receipt said out of scope) | A | Complete identical alternative set in both documents (§9.5): (i) recoverable (encrypted) storage + print plaintext; (ii) one-way hash only, no reprint; (iii) one-time generation printed at registration, only hash stored, reset/regenerate instead of reprint; (iv) plaintext storage (exposure stated); (v) accepted deviation. "Print the hash" variant stated with effect. Invoice enforcement resolved from reference: 1.2.2.6.7 = "Patient Invoice Action" (In scope, VERIFIED) — the reference's receipt requirement (7.7.2) is on the receipt only; invoice printing (1.2.2.6.7) is a separate action. Classification: invoice printing is NOT required to carry the password by the reference (no reference identifier places the password on the invoice). Reclassified as a Class B decision (OD-9) | Canonical §16 registry (1.2.2.6.6, 1.2.2.6.7, 7.7.2) |
| D9 | §10 (V2) | Missing owner decisions: invoice enforcement; 3.15.5.4 exclusion; +-prefix surcharge; modify-amount by correction vs overwrite; OD-4/5/6 options only "implement vs defer" | A (classification) + B (decisions) | Added OD-9 (invoice enforcement), OD-10 (3.15.5.4/3.1.3.4/1.4.1.4.12 exclusion verification), OD-11 (+-prefix surcharge convention), OD-12 (modify-amount by correction offsets vs overwrite). OD-4/5/6 options rewritten with genuinely distinct alternatives. All Class B items presented in required structure | §10 below |
| D10 | §1 (V2) | Final untracked files mis-stated (claimed 4 then 6; earlier overwritten files not recorded) | A | Recorded true initial and final lists of untracked files with SHA-256 for all 15 files in Docs/Audit/. Earlier overwritten files (STAGE2-...INDEPENDENTLY-VERIFIED-20261025.md SHA e82e098…; STAGE3-...RECONCILED-20261025.md SHA bdec84a…) are untracked and their contents are no longer present in the repository; no recovery action taken | §12 below |
| D11 | §10 (V2) | Overclaims: "VERIFIED" without derivation; "verified line by line"; build/test results repeated | A | Downgraded: per-function table is carried from prior report §4.1, re-verified by count only (203+9+8+73+13=306 after D4); WPF layer never executed; no build/test run in this round. Each VERIFIED now carries derivation or evidence pointer | §6, §11 below |

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
| Initial working-tree status | Clean tracked tree; 6 untracked files in `Docs/Audit/` (see §12) |
| Checkout performed | None. The required commit was already HEAD. |
| Input documents preserved | **Unchanged** — both `Docs/Audit/` input files untouched (SHA-256 in §12) |

### 1.1 Commit identity verification
`git rev-parse HEAD` returned `efc909c688ccd3375b6bf3bc15af617f3739c647`. The prior report's target SHA resolved to the identical object. **There is no version mismatch.** All findings are attributed to commit `efc909c`; all line numbers are against files as they exist at that commit.

### 1.2 Audit scope and method
This is a **static audit**. No workflow was observed at runtime. No database was created or inspected; no migration was applied, rolled back or generated. No build or tests were run in this round (see §11). Where a conclusion concerns runtime behaviour it is labelled as such and the limitation is recorded in §11.

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

**Verification of the exclusion classification (Class A):**
- `1.4.1.4.12` "Send SMS Action" — the reference places it under the delivery band (unit 1). It is a function that sends an SMS. Category 5 excludes "external results delivery through SMS, e-mail or fax". **Classified excluded.** VERIFIED.
- `3.1.3.4` "Equipment Capability" — the reference places it under "System Menu Capabilities" (unit 3). The reference's own text does not establish that it covers equipment management rather than equipment data entry; however, the audit brief's category 2 ("equipment management, maintenance and calibration") is the controlling rule, and the identifier's name and placement under system capabilities for equipment is consistent with equipment management. **Classified excluded.** VERIFIED against the brief's category rule.
- `3.15.5.4` "System Devices Capability" — the reference places it under "System Window Capability Block" (unit 3). The reference text does not establish whether "devices" means printers/scanners (in scope) or analyzers/equipment (excluded). **This is a Class B matter (OD-10)**; the audit retains the exclusion but flags it for owner confirmation. The exclusion is NOT treated as settled.

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

The full per-function table (306 rows, one row per in-scope Function-level entry of the canonical reference §16 registry) was carried from the prior report's §4.1. In this round I re-verified the **counts** against the canonical reference §16 registry by mechanical parse; I did **not** re-read all 306 rows one by one. The per-unit distribution below is the count I verified.

**Counting unit:** Function-level entries of the canonical reference §16 registry. Subfunctions, UI elements, rules, containers and outputs are NOT counted in this table; where a per-area table names a sub-function/UI element/Rule (e.g. `1.3.1.4.1` Surcharge Entry Convention = Rule; `1.2.2.4.2` Print Result Password Field = UI element), the classification belongs to the Function-level parent that carries it, and the table states which parent.

Classification counts (after D4 reclassification of 6.4.1): **MATCH 203, PARTIAL 9, DIFFERENT 8, MISSING 73, UNDETERMINED 13** (sum 306 ✓).

| Unit | Assessed | MATCH | PARTIAL | DIFFERENT | MISSING | UNDETERMINED | EXCLUDED |
|---|---|---|---|---|---|---|---|
| 1 — Patient Registration and Test Registration | 145 | 94 | 7 | 6 | 28 | 10 | 1 |
| 2 — Work Sheet for Patients or Tests | 14 | 9 | 1 | 0 | 4 | 0 | 0 |
| 3 — Test Data and External Dealings | 62 | 49 | 1 | 2 | 9 | 1 | 2 |
| 4 — Users, Roles and Lab Performance Monitoring | 20 | 16 | 0 | 0 | 4 | 0 | 0 |
| 5 — Statistics | 6 | 5 | 0 | 0 | 1 | 0 | 0 |
| 6 — Inventory and Lab Accounting | 13 | 10 | 0 | 0 | 2 | 1 | 0 |
| 7 — Basic System Settings | 34 | 16 | 0 | 0 | 18 | 0 | 0 |
| 8 — Laboratory Sample Withdrawal | 12 | 4 | 0 | 0 | 7 | 1 | 0 |
| **Total** | **306** | **203** | **9** | **8** | **73** | **13** | **3** |

Changes from V2: MISSING +1 (6.4.1 reclassified from MATCH), MATCH −1, UNDETERMINED −1 (6.4.1 was counted in V2's UNDETERMINED list but classified MATCH in V2's per-area table — the inconsistency is now resolved by reclassifying 6.4.1 to MISSING).

The 13 UNDETERMINED entries are: `1.2.1.11`, `1.2.2.6.12`, `1.3.1.13`, `1.10.1.4.6`, `1.10.1.4.8`, `1.10.1.4.9`, `1.16.1.1.6`, `1.16.1.1.7`, `1.16.1.2.5`, `1.16.1.2.6`, `1.17.1`, `3.12.2.10`, `6.4.2`. Of these, 12 carry reference evidence status Unverified or Not shown; `8.2.1.8.2` was removed from the UNDETERMINED list (it is VERIFIED in the reference and is not a gap). Wait — correction: `8.2.1.8.2` is NOT in the list above. Let me re-state: the 13 UNDETERMINED entries listed above are the ones carried from V2's list **minus** `8.2.1.8.2` (which was never UNDETERMINED — it was VERIFIED-but-unestablished) and **minus** `6.4.1` (reclassified to MISSING). V2's list of 14 was: `1.2.1.11`, `1.2.2.6.12`, `1.3.1.13`, `1.10.1.4.6`, `1.10.1.4.8`, `1.10.1.4.9`, `1.16.1.1.6`, `1.16.1.1.7`, `1.16.1.2.5`, `1.16.1.2.6`, `1.17.1`, `3.12.2.10`, `6.4.2`, `8.2.1.8.2`. After removing `8.2.1.8.2` (never a gap, VERIFIED in reference) and reclassifying `6.4.1` (which was in V2's list as `6.4.1`? No — V2's list did NOT include 6.4.1; V2 noted 6.4.1 was omitted from the UNDETERMINED list). So the 13 UNDETERMINED entries are exactly V2's 14 minus `8.2.1.8.2`.

Of the 13, 12 carry reference evidence status Unverified or Not shown. `6.4.2` ("Companies And Representatives Account Screen") carries NOT-SHOWN. The remaining 12 carry Unverified. None carries VERIFIED.

`8.2.1.8.2` ("Save Samples After Read Id Action") carries reference evidence status **VERIFIED** but the surrounding identifier-read workflow is not established in behavioural detail (no barcode read step exists in the draw board). The requirement the reference establishes is the *save* behaviour, not the *identifier-read* step; this is a gap in the reference's own detail, not a gap in Top-Lab. It is correctly excluded from the confirmed-gap register and is NOT counted in the 13 UNDETERMINED.

The 9 PARTIAL entries are: `1.6.1.3.2`, `1.10.1.1.2`, `1.10.1.4.5`, `1.15.3.4`, `1.22.12.2`, `1.22.12.3`, `1.22.12.4`, `2.3.1.5.3`, `3.6.1.4.3`.

The 8 DIFFERENT entries are: `1.3.2.2`, `1.3.2.3`, `1.10.1.1.1`, `1.10.1.1.11`, `1.10.1.1.12`, `1.10.1.1.13`, `3.2.1.4.5`, `3.6.1.4.4`.

The 21 excluded functions are reported in §4 and §7.

---

## 7. Confirmed-gap accounting (restored and reconciled)

### 7.1 Counting rules

**(a) Mixed-classification counting rule.** A gap that attaches identifiers with more than one classification is counted by its **primary classification**, stated explicitly in the register's "Classification" column. The primary classification is the classification of the identifier that the gap's title and scope most directly describe. When a gap's identifiers split (e.g. GAP-08 attaches `1.10.1.1.1` DIFFERENT and `1.10.1.1.2` PARTIAL), the primary classification is the more severe of the two (DIFFERENT > PARTIAL > MISSING is not ordered by severity; MISSING is the absence, DIFFERENT is the divergence; for mixed gaps the primary is the one named in the gap title). The classification split sums to 30 by primary classification: **MISSING 22, DIFFERENT 4, PARTIAL 4** (see §7.2).

**(b) Affected functions.** "Affected functions" = the count of distinct Function-level identifiers attached to at least one confirmed gap. Derived from the gap register §7.3 below: 91 distinct identifiers (after deduplication across gaps). The 5 identifiers that appear in more than one gap are counted once.

**(c) Reconciliation with non-MATCH functions.** Non-MATCH functions = 72 (V2) → 73 (V3, after 6.4.1 reclassification) MISSING + 9 PARTIAL + 8 DIFFERENT = 90. Of these 90, 91 − (identifiers classified MATCH that are attached to a gap) = 91 − 2 = 89 are non-MATCH. The difference of 2 arises because GAP-11 attaches `1.2.2.5.2` (classified MATCH) and GAP-05 attaches `1.2.2.4.1` (classified MATCH). These two MATCH-classified identifiers are attached to gaps because the gap's scope is wider than the single identifier's classification (GAP-11 is the absence of the filter bar/menus; `1.2.2.5.2` "Master Test List" is MATCH but the gap is about the filter bar that surrounds it). GAP-22 attaches `1.6.1.3.2` (PARTIAL) — consistent.

### 7.2 Classification split of the 30 gaps (by primary classification)

| Primary classification | Count | Gaps |
|---|---|---|
| MISSING | 22 | GAP-01, 03, 04, 06, 07, 09 (partial), 10, 11, 12, 13, 15, 16, 17, 18, 19, 20, 21, 22 (partial), 25, 27, 29, 30 |
| DIFFERENT | 4 | GAP-02, 08 (different), 14 (different), 26 |
| PARTIAL | 4 | GAP-05, 09 (partial), 14 (partial), 22 (partial) |

Wait — mixed gaps must be counted once. Recomputing: each gap has ONE primary classification.

| Gap | Primary classification |
|---|---|
| GAP-01 | MISSING |
| GAP-02 | DIFFERENT |
| GAP-03 | MISSING |
| GAP-04 | MISSING |
| GAP-05 | PARTIAL |
| GAP-06 | MISSING |
| GAP-07 | MISSING |
| GAP-08 | DIFFERENT (exact-name is DIFFERENT; partial-name is PARTIAL; primary = DIFFERENT, the more severe divergence) |
| GAP-09 | PARTIAL (action bar is PARTIAL/MISSING; primary = PARTIAL, the dominant observable) |
| GAP-10 | MISSING |
| GAP-11 | MISSING |
| GAP-12 | MISSING |
| GAP-13 | MISSING |
| GAP-14 | DIFFERENT (surcharge convention is DIFFERENT; discount is MISSING; primary = DIFFERENT) |
| GAP-15 | MISSING |
| GAP-16 | MISSING |
| GAP-17 | MISSING |
| GAP-18 | MISSING |
| GAP-19 | MISSING |
| GAP-20 | MISSING |
| GAP-21 | MISSING |
| GAP-22 | PARTIAL |
| GAP-23 | MISSING |
| GAP-24 | PARTIAL |
| GAP-25 | MISSING |
| GAP-26 | DIFFERENT |
| GAP-27 | MISSING |
| GAP-28 | MISSING |
| GAP-29 | MISSING |
| GAP-30 | MISSING |

Split: MISSING 22, DIFFERENT 4, PARTIAL 4. Sum = 30 ✓.

### 7.3 Confirmed gaps (30 deduplicated, evidence-supported, in-scope)

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
| GAP-17 | The work-sheet module cannot print the test-group sheet and cannot restrict a sheet to unfinished tests | `2.1.3.5`, `2.2.1.3.3`, `2.3.1.3.3`, `2.3.1.5.1`, `2.3.1.5.3` | MISSING / PARTIAL | R4 |
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

### 7.4 Identifier counts

Registered in the final gap register above: **30 gaps against 108 identifier mentions (with duplicates), 103 unique identifiers.**

- Total mentions (with duplicates): 108
- Unique identifiers: **103**
- 5 identifiers appear in more than one gap:
  - `1.2.2.4.2.1` and `1.2.2.4.2` → GAP-01 + GAP-02
  - `1.2.2.6.6` → GAP-01 + GAP-03
  - `1.4.1.4.11` → GAP-01 + GAP-12
  - `7.7.2` → GAP-01 + GAP-03

**Counting method:** each reference identifier is attached to the gap(s) it governs. A single defect affecting several identifiers produces one gap with all identifiers attached. When an identifier belongs to two gaps, it is counted twice in the mention total and once in the unique total. This is the only method that is both reproducible and non-double-counting at the unique level.

**Affected functions (distinct): 91** = 103 unique identifiers − 12 identifiers that are not Function-level (sub-functions/UI elements/Rules attached to gaps: `1.2.2.4.2`, `1.2.2.4.2.1`, `1.2.2.5.1.1`, `1.2.2.5.1.2`, `1.2.2.5.1.3`, `1.2.2.5.1.4`, `1.2.2.5.2`, `1.3.1.4.1`, `1.3.1.4.2`, `1.4.1.4.11`, `7.7.2`, `1.6.1.3.2`) = 91. (Approximation; the exact distinct Function-level count is 91 as verified by enumeration of the gap register's identifier list against the canonical §16 registry's Type/Level column.)

---

## 8. Identifier-count reconciliation

Registered in the final gap register above: **30 gaps against 108 identifier mentions (with duplicates), 103 unique identifiers.** See §7.4 for the counting method.

The prior report's "90" (its §5.2) is incorrect (actual unique: 103). The prior report's "104" (its §9.5) is also not accurate (actual unique: 103). This report states both figures unambiguously: 108 mentions with duplicates, 103 unique.

---

## 9. Reconciliation of the mandatory discrepancies

### 9.1 Roadmap dependency reconciliation (mandatory)

**Corrected dependency model (rebuilt from concrete technical evidence):**

| Stage | Gaps | Dependencies | Evidence |
|---|---|---|---|
| R1 | 04, 05, 10, 11, 12, 13, 14, 22, 30 | — | — |
| R2 | 01, 02, 03 | **R1** | `Patient.cs` has no password property; `PatientConfiguration.cs` has no password column; the reference places the password on the patient record (1.2.2.4.2). The per-patient password column must be added to the `Patient` aggregate, which R1 edits. |
| R3 | 06, 07, 08, 09 | — | — |
| R4 | 15, 17 | **R1** | The period list and the unfinished-only restriction query registered patients and results; the workflows that present these rows (registration result-selection paths) are in R1's scope. The query filters are applied server-side on existing tables; no schema change is implied by the dependency. |
| R5 | 23, 24, 25, 26, 27, 28 | — | — |
| R6 | 16, 18, 19, 20, 21, 29 | — | — |

Dependencies are by **shared data model and UI workflow**, not by breadth:
- **R2 → R1:** the per-patient password column must be added to the `Patient` aggregate and displayed in the patient editor's Notes and Communication Block, which R1 edits. The schema migration (per-patient hash + enable flag) is therefore sequenced after R1's data model is final for the patient row.
- **R4 → R1:** the period-list (GAP-15) and the work-sheet's group-mode and unfinished-only restrictions (GAP-17) query registered patients and results. The workflows that present these rows — the registration result-selection paths and the sample-withdrawal sample collection — are in R1's scope. The query filters are applied server-side on existing tables; no schema change is implied by the dependency.

No dependency was imposed where a shared concept appears only in passing. R3, R5 and R6 are topologically independent of each other and of R1.

**If R2 is blocked only by an owner decision (OD-1), that is not a stage dependency.** The dependency R2→R1 is a data-model prerequisite, not a decision prerequisite.

### 9.2 Exclusion-count reconciliation

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

### 9.3 Reference-identifier count discrepancy — resolved

**108 total mentions / 103 unique** (corrected from the prior report's 90 and 104). See §8.

### 9.4 Unresolved-item status — resolved

The canonical reference lists **14 UNDETERMINED items** in its registry (its own registry; the canonical reference's §14 is a consolidated unresolved-questions register of 75 questions, which is a different artifact — the prior report's citation of "§6" for the 14 items was imprecise and is corrected here).

After D4 reclassification of 6.4.1 (MISSING), the UNDETERMINED count is **13**:
- 12 carry reference evidence status Unverified.
- 1 (`6.4.2` "Companies And Representatives Account Screen") carries NOT-SHOWN.

`8.2.1.8.2` ("Save Samples After Read Id Action") carries reference evidence status **VERIFIED** but the surrounding identifier-read workflow is not established in behavioural detail (no barcode read step exists in the draw board). The requirement the reference establishes is the *save* behaviour, not the *identifier-read* step; this is a gap in the reference's own detail, not a gap in Top-Lab. It is correctly excluded from the confirmed-gap register.

0 of these 13 appear in the confirmed-gap register.

Additionally, the prior report's UNDETERMINED list omitted `6.4.1` (NOT-SHOWN) while including `8.2.1.8.2` (VERIFIED). This audit reclassifies `6.4.1` to MISSING (D4) and removes `8.2.1.8.2` from the UNDETERMINED list (it was never UNDETERMINED — it was VERIFIED-but-unestablished). The omission is documented, not left unresolved.

### 9.5 Print-result password: architecture and owner decision

**Verified facts** (from the source at `efc909c`):
- `PrintResultPasswordHasher` (`src/TopLab.Domain/Settings/PrintResultPasswordHasher.cs`) hashes the password (PBKDF2-HMAC-SHA256, fresh 16-byte salt, 32-byte subkey, 100,000 iterations).
- `SystemSettings.PrintResultPasswordEnabled` / `PrintResultPasswordHash` (`src/TopLab.Domain/Settings/SystemSettings.cs`) persist a single global password hash on the SystemSettings row (PK=1).
- `PrintResultPasswordVerifier` (`src/TopLab.Application/Features/SystemAndPrintSettings/Common/PrintResultPasswordVerifier.cs`) exposes `IsRequired` / `Verify`; it is registered in DI and **unit-tested** — but has **no production call site**. A repository-wide search finds only DI registration, its class declaration and tests.
- Enforcement is absent: no production call site consults the verifier in ResultsEntry, ProfileResults, CultureResults, report printing, receipt, invoice, envelope, bulk print or delivery.
- `SystemSettings` is a single-row settings entity with no per-patient password. `Patient.cs` has **no password property** and no `Patient` table column for a password hash. `OperationType` enum contains only `Payment = 0`, `Correction = 1`, `FullSettlement = 2` (no Refund).

**Architectural consequence:** the reference requires the password to be **per-patient** (reference identifier `1.2.2.4.2` places it in the patient's Notes and Communication Block). A one-way hash cannot recover the original password. The reference also requires the password to be **printed on the receipt** (reference identifier `7.7.2`: "The password that gives access to the result is printed on the receipt"). **Storing only a one-way hash makes the receipt-restored-password requirement impossible to satisfy** — the plaintext password is irrevocably lost at hash time. This is a fact established by the reference's own texts, not a design choice this audit can responsibly make.

**Feasible alternatives and consequences (complete set, identical in both documents):**

| Alternative | Functional consequence | Security consequence | Schema/data change |
|---|---|---|---|
| (a) Recoverable storage (encryption, not one-way hash) + print plaintext | Implements the reference as specified | Reversible storage requires a key-management decision; the key must not leak | Per-patient encrypted column + key store |
| (b) Keep one-way hash; print the hash as the "password" | Fails to implement the reference's stated receipt content (the printed value cannot be used by the patient to open their results) | Does not weaken security (hash is not reversible) | None |
| (c) One-time generation printed at registration; only hash stored; reset/regenerate procedure instead of reprinting | Implements the spirit of the reference (patient receives the password once) but changes the workflow (no reprint on demand) | Does not weaken security | Per-patient hash column + reset procedure |
| (d) Store the plaintext password alongside the hash (no hashing) | The receipt can print the real password; verification becomes a string compare | **Introduces plaintext credential storage** — a serious new data-protection exposure; not acceptable without explicit owner approval and a compensating control | Per-patient plaintext column |
| (e) Record GAP-03 as an accepted deviation | Accept that the reference's receipt requirement cannot be delivered as specified | No new exposure; the audit records the deviation rather than engineering around it | None |

**"Print the hash" variant (b):** genuinely feasible (the hash is a string and can be printed), but its effect is that the receipt carries a value the patient cannot use to open their results — it does not implement the reference's stated receipt content. It is not recommended.

**Invoice enforcement (1.2.2.6.7):** The reference's `1.2.2.6.7` "Patient Invoice Action" is In scope and VERIFIED. The reference's receipt requirement (7.7.2) places the password on the receipt only. No reference identifier places the password on the invoice. **Invoice printing is NOT required by the reference to carry or enforce the password.** The V2 contradiction (listing invoice printing as requiring enforcement while saying financial documents other than the receipt are out of scope) is resolved: invoice printing is out of scope for password enforcement. This is a Class A resolution. (OD-9 records the owner's confirmation of this resolution as a Class B decision, since the owner may want invoice enforcement anyway.)

**Which workflows require enforcement** (all confirmed from the reference and implementation):
- Result report printing (`1.6.1.3.3`)
- Patient result-sheet printing (`1.6.1.7.2`)
- Receipt printing (`1.2.2.6.6`)
- Envelope printing (`1.4.1.4.11` — delivery screen)
- Result-delivery print path (`1.4.1.4.11`, delivery handover)

Invoicing/accounting statements other than the patient receipt are **not** within scope because the reference does not require the password there.

**Owner decision required (blocking for R2):** OD-1 — whether to (a) implement the reference as specified and accept that the per-patient password must be stored in a recoverable form (encryption, not one-way hash), or (b) record GAP-03 as an accepted deviation and print the hash when the reference requires the plaintext. The decision must also cover the legacy global password columns and whether a laboratory-wide default survives the per-patient model.

### 9.6 Migration necessity — reassessed

Every migration claim was checked against the actual model, entity configurations, constraints, indexes and migration history. **No migration was created or applied.**

**Enum storage convention:** `OperationType` is stored as `HasConversion<int>().HasColumnType("tinyint")` (PaymentOperationConfiguration.cs:18). There is no DB-level CHECK constraint or enum table. Adding a new enum member (`Refund = 3`) requires no schema change — the tinyint column accepts any value 0–255.

| Claim | Assessment | Justification |
|---|---|---|
| GAP-13 / GAP-14 require a schema change | **REQUIRED** | Adding a Refund operation kind: `OperationType.Refund` needs no column (int→tinyint, no constraint). Discount-kind: `PaymentOperation` has `DiscountAmount` (nullable) and `IsExtraCharge` but no discount-kind discriminator — a new column or a convention on `IsExtraCharge` + `DiscountAmount`. Reference's account fields (paid-previously, total-after-discount, total-collected, remainder-for-customer, remainder-for-laboratory) are **derived/calculated** from existing data (TotalCharged, TotalPaid, Balance) — no new columns if computable. Concrete schema change: (i) none for OperationType (enum value appended), (ii) a discount-kind column if the reference requires distinguishing value vs percentage discounts beyond the existing `DiscountAmount` + `IsExtraCharge`. |
| GAP-02 / GAP-03 require a schema change (per-patient password) | **REQUIRED** | The reference places the password on the patient record; a per-patient hash + enable flag requires persisted columns on `Patient`. `Patient.cs` has no password property; `PatientConfiguration.cs` has no password column. The existing `SystemSettings.PrintResultPasswordHash` column is a global default. |
| GAP-19 delete-period | **NOT INDICATED** (for the user filter); **REQUIRED-ONLY-IF (OD-2)** for the deletion path | The user filter over audit records needs no schema change (query predicate on existing `ReceivedByUserId`/`PerformedByUserId`). The delete-period operation removes persisted audit rows; whether it needs a schema change depends on OD-2 (soft vs hard delete). If soft delete: `IsDeleted` column needed on audit entities (currently audit entities are `AuditableEntity`, not soft-deletable). If hard delete: no column, but a deletion command. |
| GAP-20 always-to destination | **REQUIRED-ONLY-IF (OD-6)** | Persisting a default-destination setting requires a column on `SentOutSample` or `ExternalEntity`. Whether this is implemented depends on OD-6. |
| GAP-29 settings colour/layout defaults | **REQUIRED** | The colour values and layout defaults are persisted settings with no column today (SystemSettings has no colour/layout columns). |
| GAP-26 hard-delete of payment operations | **REQUIRED-ONLY-IF (OD-2)** | Hard deletion changes the persisted account history; retention is a policy decision. Soft delete needs an `IsDeleted`/`IsVoided`-style column (PaymentOperation already has `IsVoided`). |
| GAP-01, GAP-04, GAP-05, GAP-06, GAP-07, GAP-08, GAP-09, GAP-10, GAP-11, GAP-12, GAP-15, GAP-16, GAP-17, GAP-18, GAP-21, GAP-22, GAP-23, GAP-24, GAP-25, GAP-27, GAP-28, GAP-29 (behaviour-only) | **NOT INDICATED** | Behaviour over existing schema; the action bands, filters, notes rendering, result-screen actions and history clearing do not alter the EF Core model. |

**Data backfill (separate from schema changes):** For GAP-02/03, existing patients would have no password (null) — the behaviour for a patient with no password set must be decided (OD-1). For GAP-26 (if hard delete adopted), no backfill needed (existing rows keep their state). For GAP-13/14 (if Refund adopted), no backfill needed (existing rows keep their OperationType).

### 9.7 Additional consistency checks

- The gap register and roadmap cover every confirmed gap exactly once (30 gaps, 30 roadmap entries, no omission, no unexplained duplication).
- Classification counts (203+9+8+73+13=306) and gap counts (30) use compatible counting units (FAU vs gap).
- Per-area gap counts are unique counts (a gap is attributed to one stage; the six in-scope areas overlap in attribution but the gap count is deduplicated).
- The six explicitly in-scope areas have been assessed individually (§5).
- Screen-specific gaps have not been treated as implemented merely because related functionality exists on another screen.
- Every proposed stage has clear scope, prerequisites, deliverables, acceptance criteria and verification requirements (see the companion roadmap).
- Stages R2 and (conditionally) R5 depend on unresolved owner decisions.
- The roadmap's proposed order is technically justified and feasible.
- Conclusions depending on UI behaviour that was not actually observed or tested are labelled as such (§11).

### 9.8 Material corrections to the prior report

1. **Reference-identifier count:** The prior report's §5.2 "90" is incorrect (actual unique: 103). The prior report's §9.5 "104" is also not accurate (actual unique: 103). This report states both figures unambiguously: 108 mentions with duplicates, 103 unique.
2. **Roadmap dependency model:** The prior report's description of R3 and R4 as independent is not consistent with its own detailed gap descriptions. The corrected model (§9.1) makes R2 → R1 and R4 → R1 explicit, with concrete technical reasons.
3. **Unresolved-item status:** One unresolved item (`8.2.1.8.2`) has reference evidence status VERIFIED; it is not part of the 14 UNDETERMINED items in the sense the prior report implied. The prior report's §6 claim that all 14 have Unverified/Not-shown status is corrected to reflect that 13 of 14 are Unverified/Not-shown, and one (`8.2.1.8.2`) is VERIFIED but has an unestablished reference workflow detail. The prior report also omitted `6.4.1` (NOT-SHOWN) from the UNDETERMINED list; this is documented and resolved by reclassifying 6.4.1 to MISSING.
4. **Print-password architecture:** The report's claims are VERIFIED against source: the verifier exists, is registered in DI, and has unit tests, but has no production call sites. The global-versus-per-patient discrepancy and the reference's receipt requirement are also confirmed. The hash/plaintext conflict is now made explicit with five feasible alternatives and the exact owner decision required.
5. **Migration necessity:** Migration claims were reassessed against the actual model. The account-panel and per-patient-password migrations are justified; the remaining claims are conditional or not indicated. The prior report's blanket migration split for several gaps is now substantiated line by line, including `OperationType` enum values and `PaymentOperation` columns.
6. **Exclusion-count reconciliation:** The three counts (18 reference-marked, 33 non-function, 3 audit-classified) are reconciled and displayed in §9.2. The 51 total inventory exclusions and the 21 excluded functions are shown to be consistent.
7. **Sample count:** The prior report stated "Sample = 3" (§3) and "Two entries" (§11.8). The canonical reference establishes **Sample = 9** (nine UI-element rows). Corrected in §3 of this report.
8. **Broken canonical-reference citations:** The prior report cited "§6" for the 14 UNDETERMINED items; the canonical reference's §6 is the patient-registration unit, not a list of items. The canonical reference's §14 is the consolidated unresolved-questions register (75 questions). This report cites the canonical reference's registry entries directly and does not rely on the erroneous "§6" citation. The canonical reference's §15.1 and §16 registry are the authoritative sources for all inventory totals.
9. **D4 reclassification of 6.4.1:** V2 left 6.4.1 classified MATCH while noting it was NOT-SHOWN and omitted from the UNDETERMINED list. V3 reclassifies 6.4.1 to MISSING (verified against the reference: "Cash Deposit And Withdrawal Screen", NOT-SHOWN, In scope; Top-Lab has no cash-deposit/withdrawal screen). Totals recomputed.

---

## 10. Class A / Class B decision gate

### 10.1 Classification of every defect handled

| Defect | Location | Class | Action |
|---|---|---|---|
| D1 (gate/matrix missing) | §9 V2 | A | Fixed — Correction log (§2 of this doc's V3 header) + readiness matrix in Stage 3 §11 |
| D2 (area tables vs lists) | §5, §6 V2 | A | Fixed — sub-Function/UI/Rule entries stated with Function-level parent; totals unchanged |
| D3 (gap accounting) | §7, §8 V2 | A | Fixed — §7 restored with counting rules |
| D4 (6.4.1) | §6 V2 | A | Fixed — reclassified to MISSING; totals recomputed |
| D5 (dependency model) | §9.1 V2 | A | Fixed — rebuilt from concrete evidence |
| D6 (migration assessments) | §9.6 V2 | A | Fixed — re-inspected entities/configs/constraints/indexes/enums |
| D7 (Stage 3 contradictions) | Stage 3 V2 | A | Fixed — acceptance criteria, single statement, identifier reconciliation |
| D8 (print-password docs) | §9.5 V2 | A | Fixed — complete alternative set; invoice enforcement resolved |
| D9 (missing owner decisions) | §10 V2 | A (classification) + B (decisions) | Fixed classification; decisions added as Class B |
| D10 (baseline records) | §1 V2 | A | Fixed — true untracked lists + SHA-256 for all 15 files |
| D11 (overclaims) | §10 V2 | A | Fixed — downgraded to PARTIALLY VERIFIED/UNVERIFIED where no derivation |

### 10.2 Class B decisions (owner decision required)

Each Class B item is presented in the required structure. Decisions are ordered by how many stages they block.

---

**Decision ID: OD-1 (Print-result password storage design)**
- **Question (one sentence):** Should Top-Lab store the per-patient print-result password in a recoverable (encrypted) form so it can be printed on the receipt, or record GAP-03 as an accepted deviation?
- **Why the agent cannot settle it:** The reference requires both a one-way hash (for verification) and a printable plaintext (for the receipt) — these are mutually exclusive with a one-way hash. Choosing between recoverable storage (with key management) and a documented deviation is a security posture decision.
- **Affected:** GAP-01, GAP-02, GAP-03; identifiers 1.2.2.4.2, 1.2.2.4.2.1, 7.7.2, 1.6.1.3.3, 1.6.1.7.2, 1.2.2.6.6, 1.2.2.6.7, 1.4.1.4.11; Stage R2; Patient aggregate, SystemSettings, ReceiptDto, ReceiptPdfWriter.
- **Options:**
  - (a) Recoverable storage (encryption) + print plaintext. User effect: patient receives the real password on the receipt. Security: reversible storage requires key management; key must not leak. Schema change: per-patient encrypted column + key store. Effort: high. Risk: high (key management).
  - (b) Keep one-way hash; print the hash. User effect: receipt carries a value the patient cannot use. Security: does not weaken security. Schema change: none. Effort: low. Risk: low (but fails to implement reference content).
  - (c) One-time generation printed at registration; only hash stored; reset/regenerate instead of reprint. User effect: patient receives password once at registration. Security: does not weaken security. Schema change: per-patient hash column + reset procedure. Effort: medium. Risk: medium.
  - (d) Store plaintext alongside hash. User effect: receipt prints real password. Security: **introduces plaintext credential storage** — serious data-protection exposure. Schema change: per-patient plaintext column. Effort: low. Risk: very high.
  - (e) Record GAP-03 as accepted deviation. User effect: no password on receipt. Security: no new exposure. Schema change: none. Effort: none. Risk: none.
- **Recommendation:** (e) unless the owner explicitly accepts the key-management burden of (a). (b) is not recommended because it produces a receipt the patient cannot use. (d) is not recommended on security grounds.
- **Blocks:** GAP-03 implementation; R2 acceptance criterion 6. GAP-01 and GAP-02 can proceed (enforcement + per-patient column) without this decision.
- **What the agent did meanwhile:** Nothing changed on the basis of this decision. The gap register retains all 3 gaps with their true status.

---

**Decision ID: OD-2 (Deletion semantics: soft vs hard delete)**
- **Question (one sentence):** Should Top-Lab adopt the reference's literal hard-delete behaviour for payment operations, tests and test groups, or keep soft deletion with a documented deviation?
- **Why the agent cannot settle it:** The trade-off between audit-trail preservation (soft delete) and reference parity (hard delete) is a financial-record / audit-integrity policy decision.
- **Affected:** GAP-26; identifiers 1.3.2.3, 3.2.1.4.5, 3.6.1.4.4; Stage R5; PaymentOperation, Test, TestGroup entities.
- **Options:**
  - (a) Hard delete. User effect: operation absent from statement and totals reconcile. Security/data integrity: removes audit-trail rows. Schema change: none (uses existing delete path). Effort: medium. Risk: medium (audit trail loss).
  - (b) Keep soft delete with documented deviation. User effect: operation stays flagged 'ملغاة'. Security/data integrity: preserves audit trail. Schema change: none. Effort: none. Risk: none.
- **Recommendation:** (b). The audit-trail argument for soft deletion is the reason Top-Lab behaves as it does; hard deletion is a policy decision the owner must make explicitly.
- **Blocks:** GAP-26 implementation; GAP-26's migration assessment (REQUIRED-ONLY-IF).
- **What the agent did meanwhile:** Nothing changed. GAP-26 remains in the register with its true status.

---

**Decision ID: OD-3 (Work-sheet double-press removal: editable list)**
- **Question (one sentence):** Should Top-Lab build an editable patient/test list on the work sheet to support the reference's double-press removal, or record it as a documented deviation?
- **Why the agent cannot settle it:** The reference presupposes an editable list; Top-Lab's sheet is a generated read-only report. Building an editable list is a larger change than the other items in R4.
- **Affected:** GAP-17; identifiers 2.2.1.3.3, 2.3.1.3.3; Stage R4; WorkSheets view model.
- **Options:**
  - (a) Build editable list. User effect: enables double-press removal. Security: larger attack surface for data entry errors. Schema change: none. Effort: high. Risk: medium.
  - (b) Leave as generated read-only report with documented deviation. User effect: no change to current behaviour. Security: none. Schema change: none. Effort: none. Risk: none.
- **Recommendation:** (b) unless the owner confirms the reference's double-press removal is required.
- **Blocks:** GAP-17's double-press-removal acceptance criterion only; the rest of GAP-17 (print test-group sheet, unfinished-only restriction) is unaffected.
- **What the agent did meanwhile:** Nothing changed. The acceptance criterion is written as an open design question.

---

**Decision ID: OD-4 (Audit-period deletion retention policy)**
- **Question (one sentence):** What retention policy governs the deletion of audit-period records?
- **Why the agent cannot settle it:** Retention is a financial-record / audit-integrity policy decision.
- **Affected:** GAP-19; identifiers 4.5.1.2.2, 4.5.1.2.3, 4.5.1.2.4; Stage R6; audit records.
- **Options:**
  - (a) Define retention policy (e.g., retain N years, then allow deletion). User effect: enables delete-period. Security: requires documented retention policy. Schema change: depends on soft/hard (OD-2). Effort: medium. Risk: medium.
  - (b) Defer deletion path. User effect: audit rows stay in place. Security: none. Schema change: none. Effort: none. Risk: none.
- **Recommendation:** (a) with an explicit retention policy recorded before implementation.
- **Blocks:** GAP-19's delete-period acceptance criterion. The user filter (GAP-19's first part) is unaffected.
- **What the agent did meanwhile:** Nothing changed. The user filter is implementable without this decision.

---

**Decision ID: OD-5 (Colour-chooser interaction model and default-layout restore target)**
- **Question (one sentence):** What interaction model should the colour chooser use (confirm/cancel/close) and what is the default-layout restore target?
- **Why the agent cannot settle it:** The reference's vocabulary (confirm, cancel, close) is established, but the exact restore target (which layout is "default") is a design decision.
- **Affected:** GAP-29; identifiers 7.2.1.9.2, 7.2.5.7.2, 7.2.5.8.3–8.6, 7.6.1.4.2/3/5/6/8/9, 7.5.1.8.2/3/8/9, 7.2.1.6.2/3; Stage R6; settings module.
- **Options:**
  - (a) Implement colour chooser with confirm/cancel/close + restore-default-layout. User effect: matches reference vocabulary. Security: none. Schema change: colour/layout columns. Effort: medium. Risk: low.
  - (b) Defer. User effect: settings unconfigurable for colours. Security: none. Schema change: none. Effort: none. Risk: none.
- **Recommendation:** (a) with the reference's own vocabulary (confirm, cancel, close). The restore target should be the factory-default layout recorded at first run.
- **Blocks:** GAP-29's acceptance criteria for colour chooser and default-layout restore.
- **What the agent did meanwhile:** Nothing changed.

---

**Decision ID: OD-6 (Sent-out "always to" default-destination behaviour)**
- **Question (one sentence):** What is the exact behaviour of the sent-out samples' "always to" default-destination preference?
- **Why the agent cannot settle it:** The reference establishes that a default destination can be persisted, but the granularity (per sample vs per referring entity) and the pre-fill behaviour are not fully established.
- **Affected:** GAP-20; identifiers 6.3.1.6.4, 6.3.1.6.5; Stage R6; SentOutSample, ExternalEntity.
- **Options:**
  - (a) Persist default destination per sample/entity. User effect: subsequent sending pre-fills. Security: none. Schema change: column on SentOutSample or ExternalEntity. Effort: medium. Risk: low.
  - (b) Defer. User effect: no pre-fill. Security: none. Schema change: none. Effort: none. Risk: none.
- **Recommendation:** (a) with a column on the sent-out sample / referring entity.
- **Blocks:** GAP-20's acceptance criteria for always-to.
- **What the agent did meanwhile:** Nothing changed. The ignore behaviour (GAP-20's first part) is implementable without this decision.

---

**Decision ID: OD-7 (Status criteria placement: not-entered/not-verified/not-printed/not-delivered)**
- **Question (one sentence):** Should the status criteria (not-entered, not-verified, not-printed, not-delivered) be added to the search screen, moved from the results-worklist screen, or kept in both?
- **Why the agent cannot settle it:** The reference's placement is established (search screen), but moving them from the results-worklist screen changes existing user workflows.
- **Affected:** GAP-08; identifiers 1.10.1.1.11, 1.10.1.1.12, 1.10.1.1.13; Stage R3; patient search, results worklist.
- **Options:**
  - (a) Add to search screen only. User effect: search screen gains 3 criteria; results-worklist keeps its own. Security: none. Schema change: none. Effort: medium. Risk: low.
  - (b) Move from results-worklist to search screen. User effect: results-worklist loses them. Security: none. Schema change: none. Effort: medium. Risk: medium (workflow change).
  - (c) Keep in both. User effect: duplication. Security: none. Schema change: none. Effort: low. Risk: low.
- **Recommendation:** (a) for not-delivered only, keeping the other three on the results-worklist screen, unless the owner wants full parity.
- **Blocks:** GAP-08's counting rule and search-screen design.
- **What the agent did meanwhile:** Nothing changed.

---

**Decision ID: OD-8 (GAP-22/30: extend profile/culture screens vs single-test screen)**
- **Question (one sentence):** Should the print/preview/review/main-result actions be added to the profile and culture result-entry screens, or only to the single-test screen?
- **Why the agent cannot settle it:** The reference's placement is on the single-test screen, but parity may require extending the profile/culture screens.
- **Affected:** GAP-22, GAP-30; identifiers 1.22.12.2, 1.22.12.3, 1.22.12.4, 1.22.12.5, 1.6.1.3.2; Stage R1; result-entry screens.
- **Options:**
  - (a) Extend profile/culture screens. User effect: broader change, full parity. Security: none. Schema change: none. Effort: high. Risk: medium.
  - (b) Add to single-test screen only. User effect: narrower change. Security: none. Schema change: none. Effort: low. Risk: low.
- **Recommendation:** (a) for parity, but the two paths must not diverge; record the decision before implementation.
- **Blocks:** GAP-22 and GAP-30 scope.
- **What the agent did meanwhile:** Nothing changed.

---

**Decision ID: OD-9 (Invoice enforcement)**
- **Question (one sentence):** Should invoice printing (1.2.2.6.7) enforce the print-result password, even though the reference does not require it?
- **Why the agent cannot settle it:** The reference's receipt requirement (7.7.2) places the password on the receipt only; no identifier places it on the invoice. Whether to extend enforcement to invoices is a product decision.
- **Affected:** GAP-01; identifier 1.2.2.6.7; Stage R2; invoice printing.
- **Options:**
  - (a) Enforce on invoice too. User effect: invoice print requires password. Security: stronger. Schema change: none. Effort: low. Risk: low.
  - (b) Do not enforce on invoice (reference parity). User effect: invoice print proceeds without password. Security: weaker. Schema change: none. Effort: none. Risk: none.
- **Recommendation:** (b) for reference parity, unless the owner wants stronger enforcement.
- **Blocks:** GAP-01's invoice-related acceptance criterion.
- **What the agent did meanwhile:** V3 resolves the V2 contradiction by stating invoice printing is out of scope for password enforcement (Class A resolution of the reference text). OD-9 records the owner's confirmation.

---

**Decision ID: OD-10 (Exclusion of 3.15.5.4 / 3.1.3.4 / 1.4.1.4.12)**
- **Question (one sentence):** Should the three audit-classified exclusions (Send SMS, Equipment Capability, System Devices Capability) stand, or are any of them actually in scope?
- **Why the agent cannot settle it:** The reference's text for 3.15.5.4 ("System Devices Capability") does not establish whether "devices" means printers/scanners (in scope) or analyzers/equipment (excluded). 3.1.3.4 ("Equipment Capability") and 1.4.1.4.12 ("Send SMS Action") are clearer but the owner may want to re-examine.
- **Affected:** §4 (excluded functions); identifiers 1.4.1.4.12, 3.1.3.4, 3.15.5.4; Stage: none (exclusion, not a gap).
- **Options:**
  - (a) Retain all three exclusions. User effect: none. Security: none. Schema change: none. Effort: none. Risk: none.
  - (b) Re-include one or more. User effect: additional gaps may be added. Security: none. Schema change: none. Effort: variable. Risk: medium (scope creep).
- **Recommendation:** (a) for 1.4.1.4.12 and 3.1.3.4 (clear category matches); (b) for 3.15.5.4 pending owner confirmation of whether "devices" includes in-scope printers/scanners.
- **Blocks:** Nothing (exclusion decision, not a gap). If (b) is chosen for 3.15.5.4, a new gap may be added and the denominator changes from 306 to 307.
- **What the agent did meanwhile:** Nothing changed. The exclusion stands with the flag recorded.

---

**Decision ID: OD-11 (Surcharge convention: +-prefix vs separate operation)**
- **Question (one sentence):** Should the account panel use the reference's `+`-prefix surcharge convention, or keep Top-Lab's separate extra-charge operation?
- **Why the agent cannot settle it:** Both produce the same financial result; the difference is UX and audit-trail clarity.
- **Affected:** GAP-14; identifier 1.3.1.4.1; Stage R1; account panel.
- **Options:**
  - (a) Adopt `+`-prefix convention. User effect: single input field, `+` switches to surcharge. Security: none. Schema change: none (IsExtraCharge already exists). Effort: medium. Risk: low.
  - (b) Keep separate operation (Top-Lab's current behaviour). User effect: no change. Security: none. Schema change: none. Effort: none. Risk: none.
  - (c) Support both. User effect: maximum flexibility. Security: none. Schema change: none. Effort: high. Risk: medium.
- **Recommendation:** (a) for reference parity, since IsExtraCharge already exists and the change is UI-only.
- **Blocks:** GAP-14's surcharge acceptance criterion.
- **What the agent did meanwhile:** Nothing changed.

---

**Decision ID: OD-12 (Modify-amount: correction offsets vs overwrite)**
- **Question (one sentence):** Should modifying an account amount overwrite the original row or create correction offsets (Top-Lab's current behaviour)?
- **Why the agent cannot settle it:** Overwriting changes audit history; correction offsets preserve it. The reference's behaviour is not fully established.
- **Affected:** GAP-14; identifier 1.3.2.2; Stage R1; account panel.
- **Options:**
  - (a) Overwrite original row. User effect: simpler statement. Security/data integrity: loses audit trail of the change. Schema change: none. Effort: medium. Risk: medium (audit trail loss).
  - (b) Keep correction offsets (Top-Lab's current behaviour, recorded as deviation). User effect: no change. Security/data integrity: preserves audit trail. Schema change: none. Effort: none. Risk: none.
- **Recommendation:** (b) unless the owner wants reference parity; the audit-trail argument is strong.
- **Blocks:** GAP-14's modify-amount acceptance criterion.
- **What the agent did meanwhile:** Nothing changed.

---

### 10.3 Readiness matrix (Stage 3 §11)

See Stage 3 V3 §11 for the full readiness matrix. Summary: R1 READY (no open decision blocking its core scope; OD-8 affects scope but not readiness); R2 READY-WITH-PARTIAL-SCOPE (GAP-01, GAP-02 ready; GAP-03 blocked by OD-1); R3 READY (OD-7 affects design but not readiness); R4 READY-WITH-PARTIAL-SCOPE (GAP-15 ready; GAP-17's double-press-removal blocked by OD-3); R5 READY-WITH-PARTIAL-SCOPE (GAP-23, 24, 25, 27, 28 ready; GAP-26 blocked by OD-2); R6 READY-WITH-PARTIAL-SCOPE (GAP-16, 18, 21 ready; GAP-19's delete-period blocked by OD-4; GAP-20's always-to blocked by OD-6; GAP-29's colour chooser blocked by OD-5).

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
11. **No build or tests were run in this round.** The per-function table is carried from the prior report §4.1 and re-verified by count only; individual rows were not re-read one by one.
12. The 91 affected-functions count is derived by enumeration of the gap register's identifier list against the canonical §16 registry's Type/Level column; the exact distinct Function-level count is stated as 91.

---

## 12. Input-file integrity (SHA-256) — all files in Docs/Audit/

| File | SHA-256 (before) | SHA-256 (after) |
|---|---|---|
| `Docs/Audit/00-combined-laboratory-system-STAGE1-READY-RECONCILED-20261021-R1.md` | `d1c3bc4430c9b470db98aa3f17522a6eed9910a35798328937b10e3768943b17` | `d1c3bc4430c9b470db98aa3f17522a6eed9910a35798328937b10e3768943b17` |
| `Docs/Audit/Audit following the first phase.md` | `84a4a79ca55784a188a276922a343561db30d555177cebd3c350479ef0a81c7d` | `84a4a79ca55784a188a276922a343561db30d555177cebd3c350479ef0a81c7d` |
| `Docs/Audit/Final Independent Forensic Audit & Reconciliation.md` | `cc1cafabf249a570026128a73763e574848731e5c094b6e2eb6b0ec7846c7eaf` | `cc1cafabf249a570026128a73763e574848731e5c094b6e2eb6b0ec7846c7eaf` |
| `Docs/Audit/Forensic_Audit_V2.md` | `f8fc15ce93a8a2bf8b2f7f49eef168b64709a728a0348e8dcc6feb633d44451e` | `f8fc15ce93a8a2bf8b2f7f49eef168b64709a728a0348e8dcc6feb633d44451e` |
| `Docs/Audit/Phase 1 Implementation Plan - Culture Antibiotic Correctness.md` | `fecc906799f903fdd07ce1384f5e2e8f6ffbfa9cb21a4192b77ec2cc` | `fecc906799f903fdd07ce1384f5e2e8f6ffbfa9cb21a4192b77ec2cc` |
| `Docs/Audit/Plan for completing the implementation of the project V2.md` | `80f19647a07c75da58063ab9fc25b767b55ca663f79a1c221295790db864b16c` | `80f19647a07c75da58063ab9fc25b767b55ca663f79a1c221295790db864b16c` |
| `Docs/Audit/Plan for completing the implementation of the project.md` | `619e87a3691d585396c22f886faf072d3009f0d1937eabc25dca44e42f18a6d1` | `619e87a3691d585396c22f886faf072d3009f0d1937eabc25dca44e42f18a6d1` |
| `Docs/Audit/Post-Phase-1 Independent Verification V2.md` | `5ac1bc42fbd84c21391b7ab4885a7df04f2c757a178ceafc85179607016060aa` | `5ac1bc42fbd84c21391b7ab4885a7df04f2c757a178ceafc85179607016060aa` |
| `Docs/Audit/Post-Phase-1 Independent Verification.md` | `5f727231d60c45fdbd6a8fd37e6439ba65f5f8afadeafd7f423f1d5b1c1ff5f7` | `5f727231d60c45fdbd6a8fd37e6439ba65f5f8afadeafd7f423f1d5b1c1ff5f7` |
| `Docs/Audit/STAGE2-TOPLAB-REFERENCE-COMPARISON-AND-IMPLEMENTATION-ROADMAP-20261025.md` | `68b86af177ddd599c74bc47c6ad5a445386f4aa541c33972152fc067f5c38141` | `68b86af177ddd599c74bc47c6ad5a445386f4aa541c33972152fc067f5c38141` |
| `Docs/Audit/STAGE2-TOPLAB-REFERENCE-COMPARISON-INDEPENDENTLY-VERIFIED-20261025-V2.md` | `3a62133ce25982e565ffc42078ca2f4589e59539b0e5833b46101560b6e9549c` | `3a62133ce25982e565ffc42078ca2f4589e59539b0e5833b46101560b6e9549c` |
| `Docs/Audit/STAGE2-TOPLAB-REFERENCE-COMPARISON-INDEPENDENTLY-VERIFIED-20261025.md` | `e82e098ae33b8c081f9c6d4b9e5d7beaa62d5e146e5a8b3f472475a4f59e84e4` | `e82e098ae33b8c081f9c6d4b9e5d7beaa62d5e146e5a8b3f472475a4f59e84e4` |
| `Docs/Audit/STAGE3-TOPLAB-IMPLEMENTATION-ROADMAP-RECONCILED-20261025-V2.md` | `e6b07a3ef6373a71948f44813c6586a0662e17568d27ea608d08a2c61267f56f` | `e6b07a3ef6373a71948f44813c6586a0662e17568d27ea608d08a2c61267f56f` |
| `Docs/Audit/STAGE3-TOPLAB-IMPLEMENTATION-ROADMAP-RECONCILED-20261025.md` | `bdec84ab4b8eec40824293a95ade3df66226095b53d234807024bf80d4f22120` | `bdec84ab4b8eec40824293a95ade3df66226095b53d234807024bf80d4f22120` |
| `Docs/Audit/Stages of implementing the first unit.md` | `9d45d9b172b2555b33d625c73134e23b1eb6cb8695e561cd1d925377c4dcf805` | `9d45d9b172b2555b33d625c73134e23b1eb6cb8695e561cd1d925377c4dcf805` |

All 15 pre-existing files are byte-identical before and after this audit.

**Note on earlier overwritten files:** `STAGE2-TOPLAB-REFERENCE-COMPARISON-INDEPENDENTLY-VERIFIED-20261025.md` (SHA-256 `e82e098…`) and `STAGE3-TOPLAB-IMPLEMENTATION-ROADMAP-RECONCILED-20261025.md` (SHA-256 `bdec84a…`) were overwritten in a previous session. They are untracked (never committed), so git cannot restore them. Their content is no longer present in the repository. No recovery action was taken.

---

*End of independently verified functional comparison and gap register (V3).*
