# Top-Lab — UI/UX & Screen Blueprint

## نظام توب لاب — مخطط الشاشات وتجربة المستخدم

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — UI/UX & Screen Blueprint |
| Status | **Final** — approved presentation-layer baseline |
| Document purpose | This document defines every screen in Top-Lab, its controls, its bindings to the Application layer, and the interaction conventions shared across the product. It is the single source of truth for the WPF/MVVM Presentation layer and must be implemented exactly as specified. |

---

## 1. Purpose and Scope

This document specifies **what each screen contains and how it behaves**, at a level precise enough to build the View and ViewModel for that screen without further interpretation. It answers: which fields appear, which buttons exist, which Application-layer Command or Query each control invokes, and how the screen reports success or failure to the user.

This document does not specify the concrete rendering technology for printed/previewed documents (PDF engine, barcode renderer) — those choices belong to the Reporting & Printing Blueprint and are deliberately abstracted behind `IReportPrintingService` and `IBarcodeService` per the Architecture & Folder Structure Blueprint, so that this document remains valid regardless of which concrete library is later selected.

Every screen defined here corresponds to a `View`/`ViewModel` pair in `TopLab.Presentation`, organized per the folder structure of the Architecture & Folder Structure Blueprint (§4.4), and communicates exclusively through `IMediator`, receiving a `Result` or `Result<T>` from every Command or Query it sends, per §6.1 of that document.

---

## 2. UI/UX Principles

- **Arabic-first, right-to-left.** All screens render right-to-left by default. Arabic labels are authoritative; where an English gloss is included in this document, it is for engineering clarity only and is not shown in the product.
- **Desktop-dense layout.** Top-Lab is a professional desktop tool used continuously by trained staff, not a casual consumer app. Screens favor information density (grids, multi-field forms, always-visible action buttons) over progressive disclosure.
- **One workflow, one screen.** Each screen corresponds to exactly one operational task (register a patient, enter a result, deliver a result). Screens do not attempt to combine unrelated tasks.
- **Consistent action placement.** Recurring actions (Save, Cancel/Undo, Delete, Return to Main Menu) occupy consistent positions across all screens that offer them, so staff build muscle memory across the product.
- **Immediate, non-blocking feedback.** Every action shows its outcome inline — success or a specific error message — without a separate confirmation screen, except where the action is destructive (see §5.4).

---

## 3. Application Shell and Global Navigation

### 3.1 Login Window (Screen ID: S-01)

| Aspect | Specification |
|---|---|
| Primary ViewModel | `LoginViewModel` |
| Fields | User Name (text), Password (masked, with a show-characters toggle bound to a boolean property) |
| Options | Remember-login checkbox |
| Actions | **Sign in** → `SignInCommand` (Application: `Features/AccessAndNavigation/Commands/SignIn`); **Exit** → closes the application |
| Status indicators | A database-connectivity indicator bound to a lightweight health-check query, refreshed on load |
| Validation & error display | On failure, `SignInCommand` returns a `Result` with an `Error` of type `Validation` (empty fields) or `Forbidden`/`NotFound` (invalid credentials); the error is rendered inline below the password field via the shared error-presentation convention (§5.3) |
| Navigation | On success, navigates to the Main Shell (S-02) and passes the authenticated user context to `ICurrentUserService` |

### 3.2 Main Shell and Navigation Bar (Screen ID: S-02)

| Aspect | Specification |
|---|---|
| Primary ViewModel | `ShellViewModel` |
| Top navigation bar | Fixed items: Patients, Laboratory, Work sheet, Tools, Accounts, Statistics, Users, System, Setting, About Us, Exit — each item is a navigation command routed through `INavigationService` |
| Central patient actions | Four prominent actions: Add/Edit Patient Data, Result Entry, Patient Search, Result Delivery — shortcuts into the corresponding feature screens |
| Status bar | Logged-in user name, last-login date/time (with a first-login indication), shared-database connectivity indicator, current date/time — all bound to lightweight, auto-refreshing queries |
| Permission gating | Navigation items and central actions are enabled/disabled based on the current user's permission grants; attempting an unpermitted action surfaces the standard denial message (§5.4) rather than hiding the control entirely, so staff understand what exists but is restricted |

---

## 4. Common UI Components and Interaction Patterns

These patterns are defined once and reused by every screen that needs them, rather than being redefined per screen.

### 4.1 Data grid pattern

Used for all patient lists, test lists, and audit lists. Standard behavior: single-click selects a row and loads its detail into a side or lower panel; double-click performs the row's primary action (e.g., opening a test for result entry, adding a test to an order). Grids that represent lifecycle-bearing data display the status indicator (§4.2) as a leading column.

### 4.2 Patient aggregate status indicator

A single, reusable visual component bound to the computed seven-state status described in the Architecture & Folder Structure Blueprint (§7). It renders as a color-coded icon with a text tooltip identifying the exact state. This component appears wherever a patient list is shown (registration list, results entry, search results, delivery list) and is always computed live — never cached on the ViewModel between refreshes.

### 4.3 Result and validation error presentation

Every screen that sends a Command or Query binds its result-handling to the shared `ResultErrorPresenter` (Architecture & Folder Structure Blueprint, §4.4). On failure:

- `Validation` errors render inline, next to the offending field(s), listing every violated rule at once.
- `NotFound` / `Conflict` errors render as a dismissible banner at the top of the screen.
- `Forbidden` errors render as the standard permission-denial message (§5.4).
- `Unexpected` errors render as a generic, non-technical message with an option to retry, never exposing raw exception details to the user.

On success, the ViewModel updates its bound state directly from the returned value; no full-screen reload is triggered unless the use case specifically requires one.

### 4.4 Permission-denial message

A single shared string, rendered identically everywhere a `Forbidden` result is received:

> **"أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام"**

This is not re-typed per screen; it is a constant referenced by `ResultErrorPresenter`.

### 4.5 Restricted audit controls (P / T)

The **P** and **T** buttons that appear in the results-entry grid are bound to `PatientAuditQuery` and `TestAuditQuery` respectively. Their visibility is not merely permission-gated at the click level — the controls themselves are only rendered when `ICurrentUserService` reports System Administrator status or an Absolute Permissions grant; for all other users, the columns are omitted from the grid entirely rather than shown disabled, since their presence alone should not hint at restricted functionality to unauthorized staff.

### 4.6 Destructive-action confirmation

Any action that deletes data (deleting a patient, a payment operation, a test from the catalog) requires a confirmation dialog with an explicit Yes/No choice before the Command is sent. Corrections that use void-and-reissue (per the Data Model Blueprint's payment-operation design) are treated as destructive for this purpose.

### 4.7 Secondary-password gate

Sensitive windows (user creation, inventory/cash-drawer screens) are wrapped by a shared modal — the "System menu password" dialog — prompting for the internal windows password before the underlying screen becomes accessible. This is implemented once as a reusable navigation guard, not duplicated per screen. The dialog verifies the current session user's own secondary password (via VerifySecondaryPasswordQuery) and is consumed through the single shared IDialogService implementation (ShowSecondaryPasswordDialogAsync) by every gated window, including the Users screen.

### 4.8 Print / preview action

Every "Print" or "Preview" button sends a Command carrying the relevant identifiers (e.g., a patient's test set) and awaits a `Result`. The concrete rendering surface (an embedded preview pane, a generated file opened externally, or a direct print-dialog invocation) is finalized alongside the concrete `IReportPrintingService` implementation in the Reporting & Printing Blueprint; at the Presentation level, every such screen defines only a preview region placeholder and the triggering action, so this document remains valid independent of that later choice.

---

## 5. Screen Catalog by Functional Area

Each entry lists: the screen, its primary ViewModel, its key bound fields/controls, its actions mapped to Application-layer Commands/Queries, and screen-specific notes. Screen IDs (S-xx) are used consistently across this document and correspond one-to-one with the Presentation-layer View/ViewModel pair.

### 5.1 Patient Registration & Test Ordering

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-03 Patient Registration & Test Ordering** | `PatientRegistrationViewModel` | Patient ID (read-only, system-assigned), VIP flag, Lab ID field, title + name, age + unit, sex, account type, multiple phone-number entries (dynamic list), national ID, address, treating doctor (lookup), referral entity (lookup), registration/pickup dates, medical-history checklist, sample-type flags, test list (double-click add/remove), test-group quick-add, billing summary panel | Save → `RegisterPatientCommand` / `UpdatePatientDataCommand`; Add tests → `AddTestsToVisitCommand`; Delete all tests → `RemoveAllTestsCommand` (enabled only immediately after initial add, per business rule); Undo → discards unsaved local changes; Receipt/Barcode/Work sheet quick-print → respective print Commands (§4.8) | The "delete all tests" action is enabled only in the just-added state, matching the underlying business rule; the billing panel is a live read-only projection recalculated after every Save |
| **S-04 Payment Operations List** | `PaymentOperationsViewModel` | List of the patient's payment operations | Edit → `UpdatePaymentOperationCommand`; Delete → `VoidPaymentOperationCommand` (confirmation required, §4.6) | Opened from the billing summary panel of S-03 |
| **S-14 Lab ID Assignment** | Embedded in `PatientRegistrationViewModel` | Generated Lab ID display | Create → `AssignLabIdCommand` | Not a separate window; a contextual action within S-03, listed separately here for traceability to the functional requirement it satisfies |

### 5.2 Results Entry & Result Lifecycle

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-05 Results Entry** | `ResultsEntryViewModel` | Right panel: today's patients with count, search box, account-type filter (Individual / LabToLab / Contracts / VIP / Free / All), refresh; header: code, name, sex, entity, age, Lab ID; results grid: test abbreviation, P, T, Result, Status, Finish, Verify, Print, Export; per-result notes field | Load patient tests → `GetVisitTestsQuery`; Enter result → `EnterResultCommand`; Finish/Verify/Print flags → `MarkResultFinishedCommand` / `MarkResultReviewedCommand` / `MarkResultPrintedCommand`; Export → `ExportResultCommand`; P/T buttons → `PatientAuditQuery` / `TestAuditQuery` (§4.5) | The aggregate status indicator (§4.2) is shown per patient in the right panel; the account-display setting from system configuration controls whether balance information is shown here |
| **S-06 Specialized Profile Report Entry** | `ProfileResultEntryViewModel` | Patient header (shared component, §5.6); per-analyte grid: test name, result, unit, L/H flag, normal range, verified, print; structured test-specific input fields (e.g., control/patient time, ISI, INR, ratio); comment area | Save → `EnterProfileResultCommand`; Print/Preview → §4.8; Patient History → `GetPatientTestHistoryQuery` | Each analyte row carries its own verification flag, independent of the others in the same profile |
| **S-10 Culture Result Entry** | `CultureResultEntryViewModel` | Sample, Organism A/B/C, culture condition, colony count | Save → `EnterCultureResultCommand` | Feeds into S-11 for sensitivity |
| **S-11 Antibiotic Sensitivity Entry** | `CultureSensitivityViewModel` | Attached antibiotic list, sensitivity classification (Highly For / Moderate For / Low For / Resistant For), display-mode toggles (sensitivity / reference / commercial name) | Add antibiotic result → `AddAntibioticSensitivityCommand`; Save | Antibiotic list is filtered by the Pregnant/Children flags against the patient's current data, per the corresponding business rule |

### 5.3 Report Production

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-07 Print Preview / Printed Report** | `ReportPreviewViewModel` | Preview region (§4.8); patient ID/barcode or Lab ID per setting; name; age/sex; request date; print timestamp; results vs. reference ranges; doctor's signature area | Print → `PrintReportCommand`; Preview → `GeneratePreviewCommand` | Rendering surface finalized with the Reporting & Printing Blueprint (§4.8) |
| **S-08 Combined Report Builder** | `CombinedReportViewModel` | Left list: patient's tests in the lab; right list: tests selected for the combined report; add/remove controls; reorder controls | Build → `BuildCombinedReportCommand` | |
| **S-09 Blank Report** | `BlankReportViewModel` | Fillable, patient-data-only sheet | Print → §4.8 | Opened directly from S-05 |

### 5.4 Patient Search & Visit History

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-13 Patient Search** | `PatientSearchViewModel` | Criteria: name (exact/partial), doctor, sex, age, phone number (any stored number), national ID, test, date range; result-state filters (not entered / not reviewed / not printed / not delivered) | Search → `SearchPatientsQuery` | Phone-number search matches against any of the patient's stored numbers, not a single designated primary number |
| **Patient History (contextual)** | Embedded in `ResultsEntryViewModel` and `ProfileResultEntryViewModel` | Prior-visit test list; double-click inserts prior result into the current report | Load → `GetPatientTestHistoryQuery`; multi-patient variant → `GetMultiPatientHistoryQuery` | Not a standalone window; available as an action from any results screen |

### 5.5 Result Delivery

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-12 Result Delivery** | `ResultDeliveryViewModel` | Period picker; undelivered-results list (finished and unfinished shown together); results grid with an added Price column; settlement panel (paid / remaining-to-patient / remaining-to-lab) | Load → `GetUndeliveredResultsQuery`; Settle → `SettlePatientAccountCommand`; Deliver → `MarkResultDeliveredCommand` | The print-block-on-balance rule, where enabled for the current user, is enforced before the Deliver action is permitted, surfaced as a `Forbidden`-type result if triggered |

### 5.6 System — Test Catalog & Pricing

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-15 System Pane** | `SystemPaneViewModel` | Navigation buttons into S-16 through S-22 and settings screens | Navigation only | |
| **S-16 Test Catalog** | `TestCatalogViewModel` | Full test list, search by name/group/number; name, report name, receipt name, group, barcode, completion duration, sent-out flag + cost price, patient price, Lab-to-Lab price | Add → `CreateTestCommand`; Edit → `UpdateTestCommand`; navigate to Reference Values → S-17 | |
| **S-17 Reference Values** | `ReferenceRangeViewModel` | Sex, age range (with explicit unit selector), min/max value, low/high comment | Add → `AddReferenceRangeCommand`; Edit → `UpdateReferenceRangeCommand`; Delete → `DeleteReferenceRangeCommand` | Age-unit selector is mandatory and never implicitly converted, per the corresponding business rule |
| **S-18 Price Lists** | `PriceListViewModel` | Price list selector; test/price grid | Add/rename/delete list → respective Commands; Print list → §4.8 | |
| **S-19 Test Comments** | `TestCommentViewModel` | Test selector; comment text; comment list per test | Add → `AddTestCommentCommand` | Feeds the comment dropdown in S-06/S-07 |
| **S-20 Custom Groups** | `CustomGroupViewModel` | Group name; test + price entries | Add group → `CreateCustomGroupCommand`; Add test to group → `AddCustomGroupItemCommand`; Delete | |
| **S-22 Culture Antibiotic Configuration** | `CultureAntibioticConfigViewModel` | Culture-test selector; attached-antibiotic list with count; manual entry; Pregnant/Children checkboxes | Add → `AttachAntibioticCommand`; Delete → `DetachAntibioticCommand` | |

### 5.7 External Entities & Sent-Out Samples

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-21 External Entities** | `ExternalEntityViewModel` | Entity-type selector (treating doctor / partner lab / referral-contract); name, city, address, phone, fax, responsible person + phone; assigned price list (not applicable to doctors); discount/commission percentage (doctors only); ID-generation action; left entity list | Save → `CreateExternalEntityCommand` / `UpdateExternalEntityCommand`; Delete | Field visibility adapts to the selected entity type (e.g., price-list selector hidden for treating doctors) |
| **Sent-Out Samples (contextual)** | Embedded in Accounts area (S-25) | Sent-out sample list; cost/patient price; settlement entries | Send → `SendSampleOutCommand`; Record payment → `RecordSentOutPaymentCommand` | |

### 5.8 Users, Permissions & Attendance

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-23 Users & Permissions** | `UserManagementViewModel` | Users list; user name, main password (write-only), internal windows password (write-only), last-login (read-only); working-hours start/end; break-period checkbox + duration; Absolute/Limited permission toggle; granular permission checklist including discount limit percentage | Add → `CreateUserCommand`; Edit → `UpdateUserCommand`; Save → `SaveUserPermissionsCommand`; Delete | Gated by the secondary-password dialog (§4.7); permission changes take effect at the affected user's next login; password fields are write-only (always empty on edit, populated only to change password) and the audit-access checklist item is not offered in limited mode |
| **S-24 Attendance** | `AttendanceViewModel` | Check-in/break/check-out registration controls; manager-only overview (overtime, lateness) | Record → `RecordAttendanceEventCommand`; Overview → `GetAttendanceOverviewQuery` | Overview panel visible only to the system manager |

### 5.9 Accounting & Statistics

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-25 Accounts** | `AccountingViewModel` | Buttons: cash-drawer inventory, sent-out samples, cash disbursement/deposit, company/delegate accounts; period pickers; per-element filters (user / sent samples / doctor / referral entity / account type); report-type selector; summary cards (totals, discounts, paid/unpaid, cash supplies, commissions, net profit) | Load → `GetAccountingSummaryQuery`; Disburse/Deposit → `RecordCashMovementCommand`; Settle entity → `SettleExternalEntityAccountCommand` | Gated by the secondary-password dialog (§4.7); all figures are computed live per the Data Model Blueprint's design note (§7.1 of that document) |
| **S-26 Statistics** | `StatisticsViewModel` | Statistic-type selector; yearly-by-month/sex, monthly, group-request-rate, yearly-sample-count views | Load → `GetStatisticsQuery` (parameterized by statistic type) | Read-only; no write Commands |

### 5.10 System & Print Settings

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-27 Settings Dashboard** | `SettingsDashboardViewModel` | Navigation into S-28 through S-31, plus Database Maintenance | Navigation only | |
| **S-28 System Settings** | `SystemSettingsViewModel` | Printer assignment dropdowns (default/reports, barcode, envelope, receipt); default account type; general behavior checkboxes; daily-backup checkbox + path + connectivity check | Save → `UpdateSystemSettingsCommand` | Single-row configuration entity per the Data Model Blueprint |
| **S-29 Report Settings** | `ReportSettingsViewModel` | Page margins; top space (≤ 8 cm, enforced); paper size; header/footer mode; font parameters; doctor-signature toggle; header/footer colors; history sort mode; history auto-display toggle | Save → `UpdateReportSettingsCommand` | |
| **S-30 Receipt Settings** | `ReceiptSettingsViewModel` | Top margin; currency; default pickup time; print-once toggle; test-detail display mode; cashier-printer toggle; header/footer mode | Save → `UpdateReceiptSettingsCommand` | |
| **S-31 Envelope Settings** | `EnvelopeSettingsViewModel` | Top margin; header/footer mode; per-item position controls (name/code/referral entity/date) with left/top offsets; caption-suppression toggle | Save → `UpdateEnvelopeSettingsCommand` | |

### 5.11 Sample Collection, Work Sheets & Utilities

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-32 Sample Draw / Separation** | `SampleCollectionViewModel` | Patient list; per-patient sample list; click-to-mark drawn; drawn-samples summary | Mark drawn → `MarkSampleDrawnCommand` | |
| **S-33 Work Sheets** | `WorkSheetViewModel` | Mode selector (by patient names / by test names); period picker; work-group (Log) selector; classification view | Generate → `GenerateWorkSheetQuery`; Print → §4.8 | |
| **S-34 Utilities** | `UtilitiesViewModel` | Test Library, Image Library, Shortcut Library, unit converter, calculator, stopwatch, purchases list, phone book | Self-contained tools; no Domain dependency, per the Architecture & Folder Structure Blueprint (§5) | |

### 5.12 Audit & Traceability

| Screen | ViewModel | Key fields / controls | Actions → Commands/Queries | Notes |
|---|---|---|---|---|
| **S-35 P/T Audit Inspection** | Embedded modal within `ResultsEntryViewModel` | **P view:** registering user, modification count, most recent modifying user, payment-receiving users. **T view:** result-entry user, reviewing user, printing user, print count, delivering user — each with date/time | Load → `PatientAuditQuery` / `TestAuditQuery` | Rendered only per the visibility rule in §4.5; never a separately navigable menu item |

---

## 6. Screen-to-Feature Traceability

| Screen ID | Application feature folder |
|---|---|
| S-01, S-02 | `AccessAndNavigation` |
| S-03, S-04, S-14 | `PatientRegistration` |
| S-05 | `ResultsEntry` |
| S-06 | `SpecializedProfileReports` |
| S-10, S-11 | `CultureAndSensitivity` |
| S-07, S-08, S-09 | `ReportProduction` |
| S-13 | `PatientSearchAndVisitHistory` |
| S-12 | `ResultDelivery` |
| S-35 | `AuditAndTraceability` |
| S-33 | `WorkSheets` |
| S-16, S-17 | `TestCatalogAndReferenceRanges` |
| S-18, S-19, S-20 | `PriceListsAndCustomGroups` |
| S-21 | `ExternalEntities` |
| S-22 | `CultureConfiguration` |
| Sent-Out Samples (S-25 contextual) | `SentOutSamples` |
| S-23 | `UsersAndPermissions` |
| S-24 | `Attendance` |
| S-26 | `Statistics` |
| S-25 | `InventoryAndAccounting` |
| S-32 | `SampleCollection` |
| S-27, S-28, S-29, S-30, S-31 | `SystemAndPrintSettings` |
| S-34 | `Utilities` |

---

## 7. Localization and RTL Requirements

- All screens default to right-to-left flow direction; numeric fields and date fields remain left-to-right internally but are positioned within an RTL layout.
- Every label, button caption, and message shown to the user is in Arabic; this document's English text is engineering annotation only and is never surfaced in the product.
- Grid columns, form field ordering, and button groupings follow natural RTL reading order (rightmost = first), not a mirrored left-to-right layout translated in place.

---

## 8. Non-Functional UI Considerations

- **Grid performance.** Screens displaying potentially large result sets (S-05's daily patient list, S-13's search results, S-25's accounting reports) use paged or virtualized loading rather than materializing the full result set into the UI at once.
- **Live, non-cached figures.** Any screen displaying computed financial or status figures (billing summaries, aggregate status icons, accounting summary cards) re-queries on each screen load rather than caching values locally, consistent with the single-shared-database design in the Data Model Blueprint.
- **Consistent permission behavior.** A control that is unavailable to the current user is either omitted (for restricted audit views, §4.5) or shown and met with the standard denial message on interaction (§4.4) — the two behaviors are never mixed for the same control across different screens.

---

*End of document.*
