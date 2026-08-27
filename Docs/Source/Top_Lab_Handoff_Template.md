
# Top-Lab — Handoff Document Template

## نظام توب لاب — نموذج تسليم جلسة العمل

---

## 0. How to Use This Template

- Duplicate this file at the end of every working session, renaming it to `Handoff_<YYYY-MM-DD>_<Mxx>_<short-description>.md` (for example, `Handoff_2026-08-27_M02_patient-registration-slice-1.md`).
- Fill in every section marked **Required**. Sections marked *Optional* are filled when applicable; leave them present but empty when not.
- Do not delete section headings — a handoff with a missing heading is incomplete.
- Keep the language technical and factual. State facts, not intentions.
- Attach any supporting artifact by relative path or by pull-request URL. Do not paste large code blobs into the handoff; reference them instead.
- The receiving agent reads only this document to reconstruct context. Anything not written here is invisible to them.

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_<YYYY-MM-DD>_<Mxx>_<short-description>` |
| Session date (UTC) |  |
| Session start (UTC) |  |
| Session end (UTC) |  |
| Outgoing agent / contributor |  |
| Incoming agent / contributor (if known) |  |
| Module ID (`Mxx` or `Fx`) |  |
| Module name |  |
| Wave |  |
| Feature folder(s) touched | e.g., `Features/PatientRegistration/` |
| Layers touched | Domain / Application / Infrastructure / Presentation |
| Branch name |  |
| Pull request URL (if opened) |  |
| Baseline commit at session start |  |
| Final commit at session end |  |

---

## 2. Session Objective (Required)

State, in one paragraph, what this session was expected to accomplish. Use the imperative voice ("Implement …", "Refactor …", "Fix …"). Reference the specific requirement identifiers, business rule identifiers, or ADR identifiers that scoped the work.

Example: "Implement the `RegisterPatientCommand` end-to-end, including its handler, validator, and multi-phone-number capture, satisfying FR-M02-001, FR-M02-003, and BR-03."

---

## 3. Achievements This Session (Required)

List every unit of work that reached a terminal state within the session. Use short, factual bullets. Each bullet references the file(s) or artifact(s) affected.

- `<Artifact / feature / test>` — status transition (for example, "Implementation Complete") — files or PR references.
- …

If a scope item was **partially** completed, do not place it here. Place it in §5 (Work In Progress).

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes / No.
- If No: exact error text and the file/line where it occurs.

### 4.2 Tests

- All existing tests still pass: Yes / No.
- New tests added: count and their locations.
- Tests currently failing: list each failing test name and the reason.

### 4.3 Migrations

- New EF Core migration(s) added: names and location.
- Migration applied to a local database during the session: Yes / No.
- Any manual schema change made outside a migration: Yes / No (if Yes, describe fully).

### 4.4 Dependency Injection wiring

- New services registered: list them with their lifetimes.
- Composition-root changes (`App.xaml.cs`): summary of edits.

### 4.5 Configuration

- New application configuration keys added: list them with default values and where they must be set.
- Changes to `.editorconfig` or solution-level configuration: summary.

---

## 5. Work In Progress (Required — mark "None" if none)

For every scope item that was started but not finished in the session:

- **What is in progress:** name the artifact or capability.
- **Current state:** exactly what has been written or changed.
- **What remains:** enumerate the concrete next steps, in order.
- **Estimated effort remaining:** hours or session count.
- **Location:** files, folders, or commits containing the partial work.

Example:

- **What is in progress:** `RegisterPatientCommandHandler` unit tests.
- **Current state:** Test class scaffolding and fakes created; two of nine planned test methods written.
- **What remains:** Seven test methods for validation, authorization, phone-number capture, and audit-column population.
- **Estimated effort remaining:** ~2 hours.
- **Location:** `tests/TopLab.Application.Tests/Features/PatientRegistration/`.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

Record every architectural or design decision made during the session that will constrain future work. For each:

- **Decision:** state it as a directive.
- **Reason:** why this choice, in one sentence.
- **Scope of impact:** which modules or layers are constrained by it.
- **Follow-up required:** Yes / No. If Yes, describe (for example, "Promote this to an ADR in the next session").

Decisions that materially change architecture must be promoted to a numbered ADR before further code is written on top of them.

---

## 7. Open Issues, Bugs and Risks (Required — mark "None" if none)

For each open issue:

- **Symptom / description.**
- **Reproduction steps** (if reproducible).
- **Suspected cause / area of code.**
- **Severity:** Blocker / High / Medium / Low.
- **Suggested next investigation step.**

Every Blocker or High-severity item must also be reflected in the Master Tracking Sheet's "Blockers / Notes" column for the relevant module.

---

## 8. Deviations and Waivers (Required — mark "None" if none)

Any place the session's output departs from a documented convention, standard, or blueprint. For each:

- **Convention departed from** (name the rule).
- **Nature of the departure.**
- **Justification.**
- **Whether the departure is temporary** (Yes / No). If Yes, describe the corrective action and its deadline.

Unwaived departures from binding rules are non-conforming; they must be corrected before merge, not carried through the handoff.

---

## 9. Pending Reviews and Audits (Required)

- **Code review status:** Not started / In review / Approved / Rework requested. Provide reviewer name(s) if known.
- **Audit acceptance status:** Not started / In audit / Passed / Failed. Provide auditor name(s) if known.
- **Blocking findings from review or audit** (if any): list, one per bullet.

---

## 10. Next Session Objective (Required)

State, in one paragraph, what the next agent should accomplish first. Be specific enough that the next agent can start immediately without searching for context.

Include:

- The single most important task to pick up.
- Any prerequisites that must exist before that task can start.
- The expected end-state of the next session.

If the next task requires another module to reach a specific state before this one can continue, name that module and its required state.

---

## 11. Required Reading Before Continuing (Required)

Enumerate the documents, files, and artifacts the incoming agent must read before writing code. Order by priority.

- Coding Standards & Conventions.
- Architecture & Folder Structure Blueprint.
- Data Model / Database Schema Blueprint.
- Product Requirements Document — sections relevant to the module (list section numbers).
- Test Strategy & Audit Acceptance Criteria.
- UI/UX & ViewModel Blueprint — sections relevant to the module (list section numbers).
- Reporting & Printing Blueprint — if the module touches printed output.
- Module Dependency & Execution Order Map.
- Master Tracking Sheet.
- Any relevant Architecture Decision Records — list identifiers (for example, ADR-0008, ADR-0015).
- Prior handoff documents for the same module — list filenames.

---

## 12. Environment and Tooling Notes (Optional)

- Local environment details that materially affected the work (SQL Server edition, WPF preview versions, .NET SDK build, editor extensions used).
- Any non-standard scripts or one-off commands executed. Include the exact command and its purpose.
- Any temporary files or scratch directories created; state whether they were deleted.

---

## 13. Artifacts Produced (Required — mark "None" if none)

Enumerate every artifact created or modified in the session that lives outside source control (design notes, diagrams, screenshots, sample data). For each:

- **Name.**
- **Location** (path or URL).
- **Purpose.**
- **Persistence:** Kept / To be deleted after review.

Artifacts that must not persist beyond the session are deleted before handoff and listed as "Deleted".

---

## 14. Signature Block (Required)

| Role | Name | Date (UTC) | Confirmation |
|---|---|---|---|
| Outgoing agent |  |  | I confirm this handoff document accurately reflects the state of the work at session end. |
| Reviewer (if any) |  |  | I have reviewed this handoff for completeness. |
| Incoming agent (on acceptance) |  |  | I confirm I have read and understood this handoff and accept it as my starting context. |

---

## 15. Attachments (Optional)

List every attachment by name and relative path.

- `<name>` — `<path>` — `<purpose>`.

---

*End of template.*
