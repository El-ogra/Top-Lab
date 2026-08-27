
# Top-Lab — Module Dependency & Execution Order Map

## نظام توب لاب — خريطة الوحدات وترتيب التنفيذ

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Module Dependency & Execution Order Map |
| Status | **Final** — binding implementation order baseline |
| Purpose | Define the twenty-three functional modules of Top-Lab, the dependencies between them, and the wave-based order in which they may be implemented so that no module is built before the modules it depends on. |

---

## 1. How to Read This Map

- Each module has a stable identifier (`M01` … `M23`) and a functional area.
- A dependency `A → B` means "B must be usable before A is implemented" — either because A consumes B's data, calls B's use cases, or presupposes B's runtime behavior.
- Modules are grouped into **implementation waves**. All dependencies of a module are satisfied by a strictly earlier wave. Within a wave, modules are independent of each other and may be implemented in parallel.
- Foundational cross-cutting work (layers, cross-cutting behaviors, data-model baseline) is captured as **F1 – F6** and precedes every functional wave.

---

## 2. Foundational Track (F1 – F6)

These items are not modules; they are the platform every module rests on. They must exist before Wave 1.

| ID | Foundation item | Contents |
|---|---|---|
| F1 | Solution & project skeleton | Four production projects (Domain, Application, Infrastructure, Presentation) and three test projects, wired per the Dependency Rule. |
| F2 | Domain common types | `Entity`, `AuditableEntity`, `ValueObject`, `DomainException`, strongly-typed identifier bases, shared enumerations. |
| F3 | Result pattern & pipeline behaviors | `Result`, `Result<T>`, `Error`, `ErrorType`; `ValidationBehavior`, `AuthorizationBehavior`, `LoggingBehavior`. |
| F4 | Persistence baseline | `ApplicationDbContext`, one-configuration-per-entity convention, migrations folder, `AuditableEntitySaveChangesInterceptor`, `IDateTimeProvider`, `ICurrentUserService`. |
| F5 | Data model — baseline entity schemas | Physical schema for every entity group (Patients, Test Catalog, Orders & Results, Billing, External Entities & Sent-Out, Users & Permissions, Audit columns on hosts, Accounting, System Configuration). |
| F6 | Presentation composition root | `App.xaml`/`App.xaml.cs` startup, DI wiring for all layers, main window shell (`Shell/`), navigation service, dialog service, `ResultErrorPresenter`. |

Every module below assumes F1 – F6 are complete.

---

## 3. Module Catalogue

| Module | Name | Application feature folder | Primary Domain grouping(s) |
|---|---|---|---|
| M01 | Application Access & Main Navigation | `AccessAndNavigation` | `Users` |
| M02 | Patient Registration & Test Ordering | `PatientRegistration` | `Patients`, `Tests` |
| M03 | Patient Billing & Account Settlement | `PatientBilling` | `Billing` |
| M04 | Results Entry & Result Lifecycle | `ResultsEntry` | `Results`, `PatientStatus` |
| M05 | Specialized Profile Result Reports | `SpecializedProfileReports` | `Results` |
| M06 | Culture & Sensitivity Result Entry | `CultureAndSensitivity` | `Results`, `Tests` |
| M07 | Combined, Blank & History Reports | `ReportProduction` | `Results`, `Patients` |
| M08 | Patient Search, Lab ID & Visit History | `PatientSearchAndVisitHistory` | `Patients`, `PatientStatus` |
| M09 | Result Delivery & Settlement at Handover | `ResultDelivery` | `Results`, `Billing` |
| M10 | Case Tracking, Audit & Traceability (P/T) | `AuditAndTraceability` | `Audit`, `Patients`, `Results`, `Billing` |
| M11 | Work Sheets | `WorkSheets` | `Tests`, `Patients` |
| M12 | Test Catalog & Reference Ranges | `TestCatalogAndReferenceRanges` | `Tests` |
| M13 | Price Lists, Comments & Custom Groups | `PriceListsAndCustomGroups` | `Tests`, `Billing` |
| M14 | External Entities | `ExternalEntities` | `ExternalEntities` |
| M15 | Culture & Antibiotic Configuration | `CultureConfiguration` | `Tests` |
| M16 | Sent-Out Samples | `SentOutSamples` | `SentOutSamples`, `ExternalEntities` |
| M17 | User & Permission Management | `UsersAndPermissions` | `Users` |
| M18 | Attendance & Time Tracking | `Attendance` | `Attendance`, `Users` |
| M19 | Statistics | `Statistics` | (read-only projections; no dedicated Domain entities) |
| M20 | Inventory & Lab Accounting | `InventoryAndAccounting` | `Accounting`, `Billing`, `SentOutSamples`, `ExternalEntities` |
| M21 | Sample Collection & Separation | `SampleCollection` | `SampleCollection`, `Results` |
| M22 | System & Print Settings | `SystemAndPrintSettings` | `Settings` |
| M23 | Utilities (Tools) | `Utilities` | (self-contained; no Domain dependency) |

---

## 4. Module-by-Module Dependencies

Each row lists the modules whose presence is required for the row's module to be implemented.

| Module | Depends on | Rationale |
|---|---|---|
| M17 — User & Permission Management | (foundational only) | Users, granular permissions, and the internal windows password are prerequisites for every authorization decision elsewhere. |
| M22 — System & Print Settings | (foundational only) | System-wide toggles (default account type, code-printing choice, backup destination, result-screen account display) drive downstream behavior. |
| M14 — External Entities | M17, M22 | Treating doctors, referral/contract entities, and partner labs must exist before patients can reference them and before sent-out samples can address a lab. |
| M12 — Test Catalog & Reference Ranges | M17, M22 | The test catalog and its reference ranges are prerequisites for ordering, result entry, pricing, and printing. |
| M13 — Price Lists, Comments & Custom Groups | M12, M14 | Price lists are assigned to external entities; custom groups and fixed comments target tests. |
| M15 — Culture & Antibiotic Configuration | M12 | User-defined culture test types and antibiotic attachments extend the test catalog. |
| M01 — Application Access & Main Navigation | M17 | Login and the permission-denial behavior depend on the user and permission surfaces. |
| M02 — Patient Registration & Test Ordering | M01, M12, M13, M14, M22 | Registering a patient orders tests from the catalog, may apply contract prices, references external entities, and honors system settings. |
| M03 — Patient Billing & Account Settlement | M02, M13, M17 | Account totals are computed from ordered tests and their prices; discounts are constrained by user limits. |
| M21 — Sample Collection & Separation | M02 | Sample drawn/separated marking operates on ordered tests of registered patients. |
| M04 — Results Entry & Result Lifecycle | M02, M03, M12, M22 | Result entry targets ordered tests; the lifecycle and the result-screen account display depend on billing state and system settings. |
| M05 — Specialized Profile Result Reports | M04, M12 | Specialized profile entry is a specialization of the results-entry surface and consumes reference ranges. |
| M06 — Culture & Sensitivity Result Entry | M04, M15 | Culture entry operates on tests of culture type and their attached antibiotics. |
| M07 — Combined, Blank & History Reports | M04, M05, M06, M22 | Report production consumes results from all result flavors and honors report settings. |
| M08 — Patient Search, Lab ID & Visit History | M02, M04 | Search returns registered patients; result-state filters depend on the lifecycle. |
| M09 — Result Delivery & Settlement at Handover | M04, M03, M17 | Delivery consumes finished results and enforces the print-block-on-balance permission. |
| M10 — Case Tracking, Audit & Traceability (P/T) | M02, M03, M04, M09, M17 | The `P` view aggregates patient, payment, and modification data; the `T` view aggregates per-test lifecycle activity; access is gated by permissions. |
| M11 — Work Sheets | M02, M12 | Work sheets list patients and/or tests over a period. |
| M16 — Sent-Out Samples | M02, M12, M14, M03 | A sent-out sample links a patient's ordered test to a partner lab and carries cost/patient prices and settlement. |
| M18 — Attendance & Time Tracking | M17 | Attendance events are per-user; overtime and lateness reference user working-hour configuration. |
| M19 — Statistics | M02, M04, M14, M16, M17, M18 | Statistics aggregate patient, test, sent-out and user-productivity data. |
| M20 — Inventory & Lab Accounting | M02, M03, M04, M09, M16, M14, M17, M22 | Inventory figures aggregate patient tests, payments, sent-out settlement, and cash movements, gated by the internal windows password. |
| M23 — Utilities (Tools) | M01 | The Utilities area is opened from the main navigation and is otherwise self-contained. |

---

## 5. Implementation Waves (Recommended Order)

Each wave is buildable when every earlier wave is complete. Within a wave, modules are mutually independent and may be delivered in any order or in parallel.

### Wave 0 — Foundations

F1 → F2 → F3 → F4 → F5 → F6.

Every wave below assumes Wave 0 is complete.

### Wave 1 — Configuration Backbone

- **M17** — User & Permission Management
- **M22** — System & Print Settings

Rationale: authorization and system settings are consumed by nearly every downstream module.

### Wave 2 — Reference Data

- **M14** — External Entities
- **M12** — Test Catalog & Reference Ranges

Rationale: tests and external entities are the reference surfaces on which patient orders, pricing, and printing rest.

### Wave 3 — Reference-Data Extensions

- **M13** — Price Lists, Comments & Custom Groups
- **M15** — Culture & Antibiotic Configuration
- **M01** — Application Access & Main Navigation

Rationale: price lists depend on tests and external entities; culture configuration depends on the test catalog; the login/navigation surface depends on users.

### Wave 4 — Patient Lifecycle Entry

- **M02** — Patient Registration & Test Ordering
- **M21** — Sample Collection & Separation

Rationale: registration is the earliest patient-facing entry point and unblocks sample drawing.

### Wave 5 — Patient Money & Results

- **M03** — Patient Billing & Account Settlement
- **M04** — Results Entry & Result Lifecycle

Rationale: billing and results entry both build on the registered patient and the ordered tests.

### Wave 6 — Result Specializations & Search

- **M05** — Specialized Profile Result Reports
- **M06** — Culture & Sensitivity Result Entry
- **M08** — Patient Search, Lab ID & Visit History
- **M11** — Work Sheets

Rationale: all four extend the operational data produced by earlier waves and are independent of each other.

### Wave 7 — Report Production & Handover

- **M07** — Combined, Blank & History Reports
- **M09** — Result Delivery & Settlement at Handover
- **M16** — Sent-Out Samples

Rationale: report production consumes every result flavor; delivery closes the result lifecycle; sent-out samples span patient, test, external entity, and payment surfaces.

### Wave 8 — Audit, Attendance & Statistics

- **M10** — Case Tracking, Audit & Traceability (P/T)
- **M18** — Attendance & Time Tracking
- **M19** — Statistics

Rationale: the restricted audit surfaces depend on every attributed action being in place; statistics aggregate across most operational surfaces.

### Wave 9 — Financial Consolidation

- **M20** — Inventory & Lab Accounting

Rationale: inventory and cash-drawer figures aggregate everything from patient tests to sent-out settlement and cash movements.

### Wave 10 — Utilities

- **M23** — Utilities (Tools)

Rationale: self-contained utilities are additive and depend only on the main navigation.

---

## 6. Dependency Diagram

```
                    ┌──────────────────────────────────────────┐
                    │  Wave 0 — Foundations (F1..F6)           │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 1 — M17, M22                       │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 2 — M14, M12                       │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 3 — M13, M15, M01                  │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 4 — M02, M21                       │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 5 — M03, M04                       │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 6 — M05, M06, M08, M11             │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 7 — M07, M09, M16                  │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 8 — M10, M18, M19                  │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 9 — M20                            │
                    └──────────────────────────────────────────┘
                                        │
                                        ▼
                    ┌──────────────────────────────────────────┐
                    │  Wave 10 — M23                           │
                    └──────────────────────────────────────────┘
```

Selected inter-module edges (for detail beyond wave-level ordering):

```
M12 ──┬── M02 ──┬── M03 ──┬── M09 ──┐
      │         │         │         │
      │         │         └── M04 ──┼── M05
      │         │                   ├── M06 ── (needs M15)
      │         │                   ├── M07
      │         │                   ├── M08
      │         │                   └── M10 ── (also needs M03, M09, M17)
      │         │
      │         └── M11
      │
      ├── M13 (also needs M14)
      ├── M15
      └── M16 (also needs M02, M14, M03)

M17 ── M01, M18, and gates every module through authorization
M22 ── influences M02, M04, M07, M09, M20 (defaults, code-printing, backup, result-screen account, print-block interactions)
M14 ── M13, M02, M16, M19, M20
M20 ── aggregates M02, M03, M04, M09, M16, and cash movements
M19 ── aggregates M02, M04, M14, M16, M17, M18
```

---

## 7. Rules for Extending the Map

- Any new capability is placed in an existing module first. A new module is added only when the capability does not fit any existing row.
- A new module's row is appended to §3 with a stable `Mnn` identifier that is not reused if the module is later removed.
- The new module's dependencies are recorded in §4 and its assigned wave is chosen so that every dependency lies in a strictly earlier wave.
- Cyclic dependencies between modules are not allowed. Where two modules appear to require each other, one of them is refactored so that the shared surface lives in a lower-numbered module.
- Foundational items (F1 – F6) may be extended, never split into different-layer contents; changes to the foundational track affect every wave and must be treated as high-impact.

---

*End of document.*
