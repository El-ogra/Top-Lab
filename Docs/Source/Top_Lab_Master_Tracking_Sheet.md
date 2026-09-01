
# Top-Lab — Master Tracking Sheet

## نظام توب لاب — لوحة متابعة المشروع الرئيسية

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Master Tracking Sheet |
| Status | **Active** — updatable project tracking board |
| Purpose | Track the implementation and audit progress of every foundational item and every functional module of Top-Lab in one place. |

---

## 1. Status Legend

| Symbol | Status | Meaning |
|---|---|---|
| ⬜ | Not Started | No implementation work has begun. |
| 🟨 | In Progress | Implementation has begun; not yet ready for review. |
| 🟦 | Implementation Complete | Implementation finished; ready for code review. |
| 🟧 | In Review | Code review under way. |
| 🟪 | In Audit | Functional/acceptance audit under way. |
| 🟩 | Done | Implementation complete, review passed, audit passed, merged. |
| ⬛ | Blocked | Progress halted by a documented blocker. |
| 🟥 | Rework | Failed review or audit; sent back for corrections. |

---

## 2. Phase Legend

| Phase | Meaning |
|---|---|
| Design | Detailed design deliverables for the item are being produced. |
| Implement | Source code for the item is being written and unit-tested. |
| Review | Code review of the item is under way. |
| Audit | Functional/acceptance audit of the item is under way. |
| Closed | The item is Done and no further work is planned in the current cycle. |

---

## 3. Foundational Track

| ID | Item | Wave | Status | Phase | Assignee | Started | Completed | Blockers / Notes |
|---|---|---|---|---|---|---|---|---|
| F1 | Solution & project skeleton | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Committed to `main` (194220f); builds clean (0 errors/0 warnings). |
| F2 | Domain common types (`Entity`, `AuditableEntity`, `ValueObject`, `DomainException`, strong IDs) | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented in `TopLab.Domain/Common` (Entity, ValueObject, AuditableEntity, DomainException, StronglyTypedId); matches Architecture §4.1 + ADR-0012/0013. Addendum 2026-08-30: added `LabId : StronglyTypedId<string>` and migrated `Patient.LabId` from `string?` to `LabId?` with an EF value converter back to `nvarchar(30)` (ADR-0012 compliance, commit `a035904`). |
| F3 | Result pattern & MediatR pipeline behaviors (Validation, Authorization, Logging) | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented in `TopLab.Application/Common/{Results,Interfaces,Behaviors,Authorization}` + `DependencyInjection.cs`; 3 pipeline behaviors wrap every request. Domain tests 12, Application tests 14, all green. |
| F4 | Persistence baseline (`ApplicationDbContext`, `AuditableEntitySaveChangesInterceptor`, `IDateTimeProvider`, `ICurrentUserService`) | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented `IApplicationDbContext` port in Application; `ApplicationDbContext` in Infrastructure with Fluent-API discovery; `AuditableEntitySaveChangesInterceptor` populates Created/Modified audit columns and increments `ModificationCount`; `SystemDateTimeProvider` and `CurrentUserService` (scoped, in-memory session) in Infrastructure; `AddInfrastructure` wires DbContext, interceptor, identity and time providers; 11 Infrastructure tests + 14 Application + 12 Domain = 37 tests green; build 0/0. Addendum 2026-08-30: composition root now runs `ApplicationDbContext.Database.MigrateAsync()` guarded by try/catch after `Host.Start()` (commit `a035904`). |
| F5 | Data model — baseline entity schemas across all entity groups | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented 36-table baseline: Domain entities across 9 groups + Fluent-API configurations + `BaselineDataModel` migration (commit `94b5213` / merge `61cad11`); build 0/0, 73 tests green; conventions per Data Model §2–§12, Architecture §4.1/§8. Addendum 2026-08-30: verified 36 business tables + `__EFMigrationsHistory` (37 total) reachable through the wizard-written connection; `LabId` column unchanged at `nvarchar(30)`. |
| F6 | Presentation composition root (`App.xaml.cs`), main-window shell, navigation and dialog services, `ResultErrorPresenter` | 0 | 🟩 | Closed | Local coding agent (Top-Lab) | 2026-08-28 | 2026-08-28 | Implemented `App.xaml.cs` composition root wiring Application+Infrastructure+Presentation, `MainWindow` shell, `NavigationService`, `DialogService`, `ResultErrorPresenter` (commit `896db5b` "بعد تنفيذ F6"); build 0/0. Addendum 2026-08-30: added Arabic navigation titles, first-run `DatabaseSetupWindow` wizard, and ProgramData configuration fallback (ADR-0025); `Content` copy of `appsettings.json` is now conditional on the file existing (commit `a035904`). |

---

## 4. Modules — Master Board

| ID | Module | Wave | Status | Phase | Assignee | Depends on | Started | Completed | Blockers / Notes |
|---|---|---|---|---|---|---|---|---|---|
| M17 | User & Permission Management | 1 | 🟩 | Done | Local coding agent (Top-Lab) | Foundations | 2026-09-01 | 2026-09-01 | Delivered: PBKDF2-SHA256 hashing, sign-in/sign-out, secondary-password gate, user management CRUD, first-run wizard, floor & guarded delete; 162 tests green. |
| M22 | System & Print Settings | 1 | 🟩 | Done | Local coding agent (Top-Lab) | Foundations | 2026-09-02 | 2026-09-02 | Delivered: six settings aggregates with mutators/invariant guards, read/write Application surface, Infrastructure maintenance + workstation-local lab-text store + daily-backup hook, Settings dashboard (S-27), System (S-28), Report (S-29), Receipt (S-30), Envelope (S-31) and secondary-password-gated Database Maintenance (S-32) screens; 268 tests green. |
| M14 | External Entities | 2 | ⬜ | Design |  | M17, M22 |  |  |  |
| M12 | Test Catalog & Reference Ranges | 2 | ⬜ | Design |  | M17, M22 |  |  |  |
| M13 | Price Lists, Comments & Custom Groups | 3 | ⬜ | Design |  | M12, M14 |  |  |  |
| M15 | Culture & Antibiotic Configuration | 3 | ⬜ | Design |  | M12 |  |  |  |
| M01 | Application Access & Main Navigation | 3 | ⬜ | Design |  | M17 |  |  |  |
| M02 | Patient Registration & Test Ordering | 4 | ⬜ | Design |  | M01, M12, M13, M14, M22 |  |  |  |
| M21 | Sample Collection & Separation | 4 | ⬜ | Design |  | M02 |  |  |  |
| M03 | Patient Billing & Account Settlement | 5 | ⬜ | Design |  | M02, M13, M17 |  |  |  |
| M04 | Results Entry & Result Lifecycle | 5 | ⬜ | Design |  | M02, M03, M12, M22 |  |  |  |
| M05 | Specialized Profile Result Reports | 6 | ⬜ | Design |  | M04, M12 |  |  |  |
| M06 | Culture & Sensitivity Result Entry | 6 | ⬜ | Design |  | M04, M15 |  |  |  |
| M08 | Patient Search, Lab ID & Visit History | 6 | ⬜ | Design |  | M02, M04 |  |  |  |
| M11 | Work Sheets | 6 | ⬜ | Design |  | M02, M12 |  |  |  |
| M07 | Combined, Blank & History Reports | 7 | ⬜ | Design |  | M04, M05, M06, M22 |  |  |  |
| M09 | Result Delivery & Settlement at Handover | 7 | ⬜ | Design |  | M04, M03, M17 |  |  |  |
| M16 | Sent-Out Samples | 7 | ⬜ | Design |  | M02, M12, M14, M03 |  |  |  |
| M10 | Case Tracking, Audit & Traceability (P/T) | 8 | ⬜ | Design |  | M02, M03, M04, M09, M17 |  |  |  |
| M18 | Attendance & Time Tracking | 8 | ⬜ | Design |  | M17 |  |  |  |
| M19 | Statistics | 8 | ⬜ | Design |  | M02, M04, M14, M16, M17, M18 |  |  |  |
| M20 | Inventory & Lab Accounting | 9 | ⬜ | Design |  | M02, M03, M04, M09, M16, M14, M17, M22 |  |  |  |
| M23 | Utilities (Tools) | 10 | ⬜ | Design |  | M01 |  |  |  |

---

## 5. Wave-Level Summary

| Wave | Modules | Status Summary |
|---|---|---|
| Wave 0 — Foundations | F1, F2, F3, F4, F5, F6 | 🟩 Done |
| Wave 1 — Configuration Backbone | M17, M22 | 🟩 Done |
| Wave 2 — Reference Data | M14, M12 | ⬜ Not Started |
| Wave 3 — Reference-Data Extensions | M13, M15, M01 | ⬜ Not Started |
| Wave 4 — Patient Lifecycle Entry | M02, M21 | ⬜ Not Started |
| Wave 5 — Patient Money & Results | M03, M04 | ⬜ Not Started |
| Wave 6 — Result Specializations & Search | M05, M06, M08, M11 | ⬜ Not Started |
| Wave 7 — Report Production & Handover | M07, M09, M16 | ⬜ Not Started |
| Wave 8 — Audit, Attendance & Statistics | M10, M18, M19 | ⬜ Not Started |
| Wave 9 — Financial Consolidation | M20 | ⬜ Not Started |
| Wave 10 — Utilities | M23 | ⬜ Not Started |

Fill the "Status Summary" cell with the earliest non-Done status of any item in the wave, or 🟩 when every item in the wave is Done.

---

## 6. Per-Module Detailed Tracking Blocks

Each module has its own tracking block. Blocks are updated as work progresses. Use the same status and phase legends as §1 and §2.

### 6.1 Template (copy for each module row)

```
Module: Mxx — <Name>
Wave: <n>
Owner: <name>
Dependencies satisfied? (Yes / No): 
Implementation status: ⬜ / 🟨 / 🟦 / 🟧 / 🟪 / 🟩 / ⬛ / 🟥
Current phase: Design / Implement / Review / Audit / Closed
Start date: 
Target completion date: 
Actual completion date: 
Domain work — status: 
Application (Commands/Queries) work — status: 
Infrastructure work (EF configurations, migrations, services) — status: 
Presentation work (Views/ViewModels) — status: 
Unit tests present? (Yes / No):
Manual/ViewModel-level tests present where applicable? (Yes / No):
Audit outcome: Pass / Fail / Pending
Audit notes: 
Open blockers: 
Handoff document produced? (Yes / No):
```

### 6.2 Concrete blocks

The following blocks are pre-created; contents mirror the master board in §4 and are updated in place as work progresses.

---

**Module: M17 — User & Permission Management**
- Wave: 1
- Owner: Local coding agent (Top-Lab)
- Dependencies satisfied? Foundations only — satisfied.
- Implementation status: 🟩
- Current phase: Done
- Notes: prerequisite for every downstream authorization decision; delivers `User`, `Permission`, `UserPermissionGrant`, absolute/limited modes, internal windows password, discount limit %, print-block-on-balance flag, working-hours and break configuration. Delivered surface: PBKDF2-SHA256 password hashing (self-describing format, no schema change), sign-in/sign-out, secondary-password gate, user management screen, first-run administrator wizard, last-active-absolute-user floor, guarded delete. Build 0/0, 162 tests green (71 Domain + 60 Application + 31 Infrastructure).
- Started: 2026-09-01
- Completed: 2026-09-01

**Module: M22 — System & Print Settings**
- Wave: 1
- Owner: Local coding agent (Top-Lab)
- Dependencies satisfied? Foundations only.
- Implementation status: 🟩
- Current phase: Done
- Completed: 2026-09-02
- Notes: delivers `SystemSettings`, `ReportSettings`, `ReceiptSettings`, `EnvelopeSettings`, `EnvelopePrintItemPosition`, `PrinterAssignment`; daily backup, Database Maintenance, and system initialization functions.

**Module: M14 — External Entities**
- Wave: 2
- Owner:
- Dependencies satisfied? Requires M17, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: unified `ExternalEntity` table with type discrimination (TreatingDoctor / ReferralOrContract / PartnerLab).

**Module: M12 — Test Catalog & Reference Ranges**
- Wave: 2
- Owner:
- Dependencies satisfied? Requires M17, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `TestGroup`, `Test`, `ReferenceRange`, `TestComment`, `PatientTitle`, and the age-unit-sensitive matching rule.

**Module: M13 — Price Lists, Comments & Custom Groups**
- Wave: 3
- Owner:
- Dependencies satisfied? Requires M12, M14.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `PriceList`/`PriceListItem`, `CustomGroup`/`CustomGroupItem`, `TestComment` operations.

**Module: M15 — Culture & Antibiotic Configuration**
- Wave: 3
- Owner:
- Dependencies satisfied? Requires M12.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `Antibiotic`, `CultureAntibioticAttachment`, and the user-extensible culture-type surface.

**Module: M01 — Application Access & Main Navigation**
- Wave: 3
- Owner:
- Dependencies satisfied? Requires M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: login, navigation bar, status bar, permission-denial message.

**Module: M02 — Patient Registration & Test Ordering**
- Wave: 4
- Owner:
- Dependencies satisfied? Requires M01, M12, M13, M14, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `Patient`, `PatientPhoneNumber`, `MedicalConditionType`, `PatientMedicalCondition`; captures multiple phone numbers per patient.

**Module: M21 — Sample Collection & Separation**
- Wave: 4
- Owner:
- Dependencies satisfied? Requires M02.
- Implementation status: ⬜
- Current phase: Design
- Notes: sample-drawn / separated marking on `PatientTest`.

**Module: M03 — Patient Billing & Account Settlement**
- Wave: 5
- Owner:
- Dependencies satisfied? Requires M02, M13, M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers `PaymentOperation`, void-and-reissue correction, discount-limit enforcement.

**Module: M04 — Results Entry & Result Lifecycle**
- Wave: 5
- Owner:
- Dependencies satisfied? Requires M02, M03, M12, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: delivers the `PatientTest` lifecycle columns and the aggregate-status calculator invocation.

**Module: M05 — Specialized Profile Result Reports**
- Wave: 6
- Owner:
- Dependencies satisfied? Requires M04, M12.
- Implementation status: ⬜
- Current phase: Design
- Notes: `ProfileResultItem`.

**Module: M06 — Culture & Sensitivity Result Entry**
- Wave: 6
- Owner:
- Dependencies satisfied? Requires M04, M15.
- Implementation status: ⬜
- Current phase: Design
- Notes: `CultureResult`, `CultureAntibioticResult`.

**Module: M08 — Patient Search, Lab ID & Visit History**
- Wave: 6
- Owner:
- Dependencies satisfied? Requires M02, M04.
- Implementation status: ⬜
- Current phase: Design
- Notes: multi-criteria search (including any stored telephone number); Lab ID creation and visit list.

**Module: M11 — Work Sheets**
- Wave: 6
- Owner:
- Dependencies satisfied? Requires M02, M12.
- Implementation status: ⬜
- Current phase: Design
- Notes: patient-based, test-based, Log-group work sheets; test-frequency classification.

**Module: M07 — Combined, Blank & History Reports**
- Wave: 7
- Owner:
- Dependencies satisfied? Requires M04, M05, M06, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: combined, blank, auto/manual/separate/multi-patient history.

**Module: M09 — Result Delivery & Settlement at Handover**
- Wave: 7
- Owner:
- Dependencies satisfied? Requires M04, M03, M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: undelivered-results list; account view at delivery; print-block interaction.

**Module: M16 — Sent-Out Samples**
- Wave: 7
- Owner:
- Dependencies satisfied? Requires M02, M12, M14, M03.
- Implementation status: ⬜
- Current phase: Design
- Notes: `SentOutSample`, `SentOutSamplePayment`; per-lab follow-up and settlement.

**Module: M10 — Case Tracking, Audit & Traceability (P/T)**
- Wave: 8
- Owner:
- Dependencies satisfied? Requires M02, M03, M04, M09, M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: restricted `P` and `T` inspection queries; access via authorization pipeline.

**Module: M18 — Attendance & Time Tracking**
- Wave: 8
- Owner:
- Dependencies satisfied? Requires M17.
- Implementation status: ⬜
- Current phase: Design
- Notes: `AttendanceRecord`; overtime/lateness visibility restricted to the system manager.

**Module: M19 — Statistics**
- Wave: 8
- Owner:
- Dependencies satisfied? Requires M02, M04, M14, M16, M17, M18.
- Implementation status: ⬜
- Current phase: Design
- Notes: read-only projections; no dedicated Domain entities.

**Module: M20 — Inventory & Lab Accounting**
- Wave: 9
- Owner:
- Dependencies satisfied? Requires M02, M03, M04, M09, M16, M14, M17, M22.
- Implementation status: ⬜
- Current phase: Design
- Notes: cash-drawer inventory, per-element inventories, cash disbursement/deposit, companies & delegates account tracking.

**Module: M23 — Utilities (Tools)**
- Wave: 10
- Owner:
- Dependencies satisfied? Requires M01.
- Implementation status: ⬜
- Current phase: Design
- Notes: Test Library, Image Library, Shortcut Library, Unit Converter, Calculator, Stopwatch, Purchases List, Phone Book — self-contained.

---

## 7. Cross-Cutting Concern Tracking

| Concern | Owning artifact | Status | Phase | Notes |
|---|---|---|---|---|
| Result pattern | `Result`, `Result<T>`, `Error`, `ErrorType` | 🟩 | Closed | Delivered by F3. |
| Validation | `ValidationBehavior` + per-Command validators | ⬜ | Design | Delivered by F3; validators live inside each feature folder. |
| Authorization | `AuthorizationBehavior` + declared permissions on Commands/Queries | 🟩 | Closed | Delivered by F3; permission catalog of thirteen codes seeded by M17 and consumed by the authorization pipeline; no pending catalog. |
| Logging | `LoggingBehavior` | ⬜ | Design | Delivered by F3. |
| Audit columns | `AuditableEntity` + `AuditableEntitySaveChangesInterceptor` | ⬜ | Design | Delivered by F2 + F4. |
| Patient aggregate status | `PatientStatusCalculator` (Domain service) | ⬜ | Design | Delivered inside M04. |
| Time provider | `IDateTimeProvider` | ⬜ | Design | Delivered by F4. |
| Current-user context | `ICurrentUserService` | ⬜ | Design | Delivered by F4. |

---

## 8. Change Log Rules

- Each row is updated in place; historical values are not deleted from the sheet — instead, the change-log below records the update.
- Every non-trivial status transition (Not Started → In Progress, In Progress → Implementation Complete, and every audit outcome) is appended to §9 with the date and the person who made the change.
- Blockers are recorded in the row's "Blockers / Notes" cell and mirrored in §9.

---

## 9. Change Log

| Date | Item | Change | By |
|---|---|---|---|
| 2026-08-28 | F1 | Solution & project skeleton created; builds clean (0 errors/0 warnings); committed to `main` (194220f). | Local coding agent (Top-Lab) |
| 2026-08-28 | F3 | Result pattern + MediatR pipeline behaviors (Validation/Authorization/Logging) implemented in `TopLab.Application`; Application ports added; 14 Application tests + 12 Domain tests green. | Local coding agent (Top-Lab) |
| 2026-08-28 | F4 | Persistence baseline implemented: `IApplicationDbContext` port + `ApplicationDbContext` + `AuditableEntitySaveChangesInterceptor` + `SystemDateTimeProvider` + `CurrentUserService` + Infrastructure DI. 11 Infrastructure tests + 14 Application + 12 Domain = 37 tests green; build 0/0. | Local coding agent (Top-Lab) |
| 2026-08-28 | F5 | Baseline entity schemas across all entity groups implemented (36 tables, Fluent-API configurations, `BaselineDataModel` migration `20260828052248`). Commits `94b5213` / `61cad11`; build 0/0, tests 73 green; verified against Data Model §4–§12 and Architecture §4.1. | Local coding agent (Top-Lab) |
| 2026-08-28 | F6 | Presentation composition root and shell implemented (`App.xaml.cs`, `MainWindow`, navigation/dialog services, `ResultErrorPresenter`). Commit `896db5b` "بعد تنفيذ F6"; build 0/0, presentation boots via Host. | Local coding agent (Top-Lab) |
| 2026-09-02 | M22 | System & Print Settings implemented across S1–S8: domain mutators/invariant guards, read+write Application surface, Infrastructure maintenance + workstation-local lab-text store + daily-backup hook + seed-repair, Settings dashboard and System/Report/Receipt/Envelope/Database Maintenance screens, secondary-password gate. 268 tests green; build 0/0. Commit `docs(m22): finalize module 22 documentation, ADR-0027, tracking sheet, and handoff`. | Local coding agent (Top-Lab) |

Add one row per material change.

---

*End of document.*
