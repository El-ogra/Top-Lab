# Top-Lab — Data Model / Database Schema Blueprint

## نظام توب لاب — نموذج البيانات وهيكل قاعدة البيانات

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Data Model / Database Schema Blueprint |
| Status | **Final** — approved data model baseline |
| Document purpose | This document defines the complete relational data model for Top-Lab: every entity, its attributes, its keys, and its relationships. It is the single source of truth for database design and must be implemented exactly as specified before any Entity Framework Core configuration or migration is written. |

---

## 1. Purpose and Scope

This document specifies **what data Top-Lab stores and how that data relates**. It answers: which tables exist, what each column means, which columns are required, and how tables reference one another.

This document does not specify:

- Screen layout or user interaction (see the UI/UX Blueprint).
- Step-by-step use-case logic (see the Business Logic specifications per module).
- Physical index tuning or SQL Server-specific performance configuration beyond the indexing guidance in §8.

This document is written at the **conceptual/logical entity level**. Entity names here correspond directly to the Domain-layer entity names defined in the Architecture & Folder Structure Blueprint; the mapping between the two is exact and intentional — this document is the persistence realization of that Domain model.

---

## 2. Modeling Conventions

Every table in this document follows these conventions unless explicitly noted otherwise.

- **Primary keys.** Every table has a single-column surrogate primary key, named `<Entity>Id`, of type `int IDENTITY` unless a different type is specified.
- **Auditable tables.** Any table representing a mutable business entity carries five audit columns: `CreatedByUserId`, `CreatedAtUtc`, `LastModifiedByUserId`, `LastModifiedAtUtc`, `ModificationCount`. These are populated automatically at the persistence layer, never set manually by application code. Tables carrying these columns are marked **[Auditable]** below.
- **Soft delete.** Tables whose rows must remain queryable for audit or historical-reporting purposes after logical deletion carry an `IsDeleted bit` column and are never physically removed. Tables marked **[Soft-delete]** follow this rule; all others use ordinary (hard) deletion, gated by permission.
- **Money.** All monetary columns use `decimal(18,2)`.
- **Text.** Arabic and English text share the same `nvarchar` columns; no separate localized columns exist, consistent with the single-language-per-deployment nature of the product.
- **Time.** All timestamp columns are `datetime2` stored in UTC, suffixed `AtUtc`.
- **Single shared database.** Every table below lives in the one shared SQL Server database described in the technology baseline; no table is scoped to a workstation, and no table carries a branch identifier, consistent with the single-branch operating model.

---

## 3. Entity Groups Overview

The data model is organized into nine groups, mirroring the Domain-layer groupings of the Architecture Blueprint:

| Group | Concern |
|---|---|
| A. Patients & Identification | The patient/visit record, phone numbers, medical history |
| B. Test Catalog & Reference Data | Tests, groups, reference ranges, comments, work groups |
| C. Orders, Results & Clinical Data | Test orders, results, profile results, culture & antibiotic results |
| D. Billing & Payments | Payment operations, price lists |
| E. External Entities & Sent-Out Samples | Doctors, referral/contract entities, partner labs, sent-out tracking |
| F. Users, Permissions & Attendance | Users, permission grants, attendance records |
| G. Audit & Traceability | Restricted per-test audit trail (the "T" data) |
| H. Accounting & Cash Management | Cash movements, company/delegate account balances |
| I. System Configuration | Report, receipt, envelope, printer and general system settings |

---

## 4. Group A — Patients & Identification

### 4.1 Design note: the Patient/visit relationship

Top-Lab distinguishes the **Patient ID** (generated fresh for each registration) from the **Lab ID** (a persistent identifier that is entered once and then carried forward on every later registration of the same person). To realize this behavior:

> **`Patient` holds one row per registration (per visit).** The `LabId` column is a shared, non-unique grouping value: the first time a person is registered, a Lab ID may be created and stored on that row; on every subsequent registration of the same person, the same `LabId` value is copied onto the new row. Searching by Lab ID therefore naturally returns every visit belonging to that person, because every visit's row carries the same `LabId` value. `PatientId` (the primary key) is unique per visit; `LabId` is unique per person but repeated across that person's rows.

This directly satisfies the requirement that Lab ID and Patient ID are distinct identifiers with distinct behavior, without requiring a second "master patient" table that the product's own requirements do not describe.

### 4.2 `Patient` **[Auditable]**

| Column | Type | Notes |
|---|---|---|
| PatientId | int PK | System-generated per registration ("Patient ID") |
| LabId | nvarchar(30), nullable | Persistent cross-visit identifier; indexed, not unique |
| Title | nvarchar(50), nullable | Configurable honorific |
| FullName | nvarchar(200) | Required |
| Sex | tinyint | Male / Female |
| AgeValue | int | Required |
| AgeUnit | tinyint | Day / Month / Year |
| NationalId | nvarchar(30), nullable | |
| Address | nvarchar(300), nullable | |
| TreatingDoctorId | int FK → ExternalEntity, nullable | |
| ReferralEntityId | int FK → ExternalEntity, nullable | |
| AccountType | tinyint | Individual / LabToLab / Contracts / VIP / Free |
| IsVip | bit | |
| RegistrationDateUtc | datetime2 | |
| PickupDateUtc | datetime2, nullable | Suggested from ordered tests' completion durations |
| IsFastingIndicated | bit | |
| FastingHours | int, nullable | |
| RecentContrastImaging | bit | Contrast X-ray/ultrasound within last two days |
| Notes | nvarchar(1000), nullable | Surfaced on results-entry and delivery screens |

### 4.3 `PatientPhoneNumber`

| Column | Type | Notes |
|---|---|---|
| PatientPhoneNumberId | int PK | |
| PatientId | int FK → Patient | |
| PhoneNumber | nvarchar(30) | Indexed — supports search by any stored number |
| SortOrder | tinyint | Entry order |

### 4.4 `MedicalConditionType` (catalog)

| Column | Type | Notes |
|---|---|---|
| MedicalConditionTypeId | int PK | |
| Name | nvarchar(100) | e.g., Diabetes medication, Anemia, Renal failure, Hypertension |
| Category | tinyint | Medication / Condition |

### 4.5 `PatientMedicalCondition`

| Column | Type | Notes |
|---|---|---|
| PatientId | int FK → Patient | Composite PK with MedicalConditionTypeId |
| MedicalConditionTypeId | int FK → MedicalConditionType | |

Modeling the medical-history checklist as a catalog plus a join table (rather than fixed boolean columns) allows the checklist to be extended without a schema change, consistent with the product's general pattern of user-extensible catalogs.

---

## 5. Group B — Test Catalog & Reference Data

### 5.1 `TestGroup`

| Column | Type | Notes |
|---|---|---|
| TestGroupId | int PK | |
| Name | nvarchar(150) | e.g., Kidney Function, Liver Profile |

### 5.2 `Test` **[Auditable]**

| Column | Type | Notes |
|---|---|---|
| TestId | int PK | |
| Name | nvarchar(150) | |
| ReportName | nvarchar(150) | |
| ReceiptName | nvarchar(150) | |
| TestGroupId | int FK → TestGroup, nullable | |
| Barcode | nvarchar(50), nullable | |
| CompletionDurationMinutes | int | Basis for pickup-time computation |
| IsSentOut | bit | |
| SentOutCostPrice | decimal(18,2), nullable | Paid to the external lab |
| PatientPrice | decimal(18,2) | |
| LabToLabPrice | decimal(18,2), nullable | |
| ResultKind | tinyint | Simple / SpecializedProfile / Culture — determines which result table is used |
| IsCultureType | bit | True for user-defined culture test types |

### 5.3 `ReferenceRange`

| Column | Type | Notes |
|---|---|---|
| ReferenceRangeId | int PK | |
| TestId | int FK → Test | |
| Sex | tinyint, nullable | Null = applies to both sexes |
| AgeUnit | tinyint | Day / Month / Year — never converted between units |
| AgeMin | int | |
| AgeMax | int | |
| MinValue | decimal(18,4) | |
| MaxValue | decimal(18,4) | |
| LowComment | nvarchar(500), nullable | |
| HighComment | nvarchar(500), nullable | |

### 5.4 `TestComment`

| Column | Type | Notes |
|---|---|---|
| TestCommentId | int PK | |
| TestId | int FK → Test | |
| CommentText | nvarchar(1000) | Multiple comments per test allowed |

### 5.5 `CustomGroup` and `CustomGroupItem`

| Table | Column | Type | Notes |
|---|---|---|---|
| CustomGroup | CustomGroupId | int PK | |
| CustomGroup | Name | nvarchar(150) | |
| CustomGroupItem | CustomGroupId | int FK → CustomGroup | Composite PK with TestId |
| CustomGroupItem | TestId | int FK → Test | |
| CustomGroupItem | Price | decimal(18,2) | Group-specific price, independent of the test's catalog price |

### 5.6 `WorkGroupLog` and `WorkGroupLogItem`

| Table | Column | Type | Notes |
|---|---|---|---|
| WorkGroupLog | WorkGroupLogId | int PK | |
| WorkGroupLog | Name | nvarchar(150) | |
| WorkGroupLogItem | WorkGroupLogId | int FK → WorkGroupLog | Composite PK with TestId |
| WorkGroupLogItem | TestId | int FK → Test | |

### 5.7 `PatientTitle`

| Column | Type | Notes |
|---|---|---|
| PatientTitleId | int PK | |
| TitleText | nvarchar(50) | |
| IsDefault | bit | |

### 5.8 `Antibiotic`

| Column | Type | Notes |
|---|---|---|
| AntibioticId | int PK | |
| Name | nvarchar(150) | |
| IsPregnancyFlagged | bit | Appears only when pregnancy is indicated |
| IsChildrenFlagged | bit | Appears only for patients under 12 years |

### 5.9 `CultureAntibioticAttachment`

| Column | Type | Notes |
|---|---|---|
| TestId | int FK → Test | Composite PK with AntibioticId; Test must have IsCultureType = true |
| AntibioticId | int FK → Antibiotic | |

---

## 6. Group C — Orders, Results & Clinical Data

### 6.1 `PatientTest` **[Auditable]**

This is the central order/result line: one row per test ordered for one patient visit.

| Column | Type | Notes |
|---|---|---|
| PatientTestId | int PK | |
| PatientId | int FK → Patient | |
| TestId | int FK → Test | |
| PriceAtOrderTime | decimal(18,2) | Snapshot of the price applied at ordering, immune to later catalog price changes |
| IsUrine, IsStool, IsBlood, IsSemen, IsCSF | bit | Sample-type flags |
| IsTakenOutsideLab | bit | |
| IsSampleDrawn | bit | Sample collection/separation flag |
| SampleDrawnAtUtc | datetime2, nullable | |
| ResultValue | nvarchar(200), nullable | Used for simple (non-profile, non-culture) tests only |
| ResultFlag | tinyint, nullable | Normal / Low / High, evaluated against ReferenceRange at entry time |
| Notes | nvarchar(500), nullable | Per-result notes accompanying status controls |
| EnteredByUserId | int FK → User, nullable | |
| EnteredAtUtc | datetime2, nullable | |
| IsReviewed | bit | |
| ReviewedByUserId | int FK → User, nullable | |
| ReviewedAtUtc | datetime2, nullable | |
| IsPrinted | bit | |
| PrintCount | int | |
| LastPrintedByUserId | int FK → User, nullable | |
| LastPrintedAtUtc | datetime2, nullable | |
| IsDelivered | bit | "Delivered" = physical handover only |
| DeliveredByUserId | int FK → User, nullable | |
| DeliveredAtUtc | datetime2, nullable | |
| IsExported | bit | Local file export; does not affect lifecycle stage |
| ExportedAtUtc | datetime2, nullable | |

**Design note.** `EnteredByUserId/AtUtc`, `ReviewedByUserId/AtUtc`, `LastPrintedByUserId/PrintCount/AtUtc`, and `DeliveredByUserId/AtUtc` together constitute the complete "T-button" restricted audit data (§7). They are stored directly on `PatientTest` rather than in a duplicate table, since the relationship is strictly one-to-one; access restriction is enforced at the application layer (only System Administrator / Absolute Permissions may query these specific columns), not by physical table separation.

### 6.2 `ProfileResultItem`

Used for specialized multi-analyte profile tests (one row per analyte within a profile test).

| Column | Type | Notes |
|---|---|---|
| ProfileResultItemId | int PK | |
| PatientTestId | int FK → PatientTest | |
| AnalyteName | nvarchar(150) | |
| ResultValue | nvarchar(100) | |
| Unit | nvarchar(30), nullable | |
| Flag | tinyint, nullable | Low / High |
| IsVerified | bit | Per-row verification, independent of other analytes in the same profile |
| IsPrinted | bit | |

### 6.3 `CultureResult`

| Column | Type | Notes |
|---|---|---|
| PatientTestId | int PK, FK → PatientTest | One-to-one |
| Sample | nvarchar(100), nullable | |
| OrganismA | nvarchar(150), nullable | |
| OrganismB | nvarchar(150), nullable | |
| OrganismC | nvarchar(150), nullable | |
| CultureCondition | nvarchar(200), nullable | |
| ColonyCount | nvarchar(50), nullable | |

### 6.4 `CultureAntibioticResult`

| Column | Type | Notes |
|---|---|---|
| CultureAntibioticResultId | int PK | |
| PatientTestId | int FK → CultureResult (PatientTestId) | |
| AntibioticId | int FK → Antibiotic | |
| SensitivityCategory | tinyint | Highly For / Moderate For / Low For / Resistant For |

---

## 7. Group D — Billing & Payments

### 7.1 `PaymentOperation` **[Auditable]**

| Column | Type | Notes |
|---|---|---|
| PaymentOperationId | int PK | |
| PatientId | int FK → Patient | |
| Amount | decimal(18,2) | |
| DiscountAmount | decimal(18,2), nullable | Constrained by the operating user's discount limit at entry time |
| IsExtraCharge | bit | The "+" amount unrelated to ordered tests |
| OperationType | tinyint | Payment / Correction / FullSettlement |
| ReceivedByUserId | int FK → User | Feeds the P-button "users who received payments" view |
| OperationAtUtc | datetime2 | |
| IsVoided | bit | Corrections use void-and-reissue rather than physical deletion, to preserve the audit trail |

**Design note.** Running account totals (total, discount, remaining-to-patient, remaining-to-lab) are **computed**, not stored: they are derived at query time as `SUM(PatientTest.PriceAtOrderTime)` for the patient's tests minus `SUM(PaymentOperation.Amount)` where `IsVoided = 0`, per the non-functional requirement that all figures shown to users are derived from the same shared, current data rather than cached per workstation.

**Design note — the P-button.** Patient-record audit data required for the restricted "P" inspection (registering user, modification count, last modifying user, payment-receiving users) requires no dedicated table: the registering user and modification count are the `Patient` entity's own inherited audit columns (§2), and the list of payment-receiving users is a query over `PaymentOperation.ReceivedByUserId` filtered by patient. This avoids storing the same fact twice.

### 7.2 `PriceList` and `PriceListItem`

| Table | Column | Type | Notes |
|---|---|---|---|
| PriceList | PriceListId | int PK | |
| PriceList | Name | nvarchar(150) | |
| PriceListItem | PriceListId | int FK → PriceList | Composite PK with TestId |
| PriceListItem | TestId | int FK → Test | |
| PriceListItem | Price | decimal(18,2) | |

---

## 8. Group E — External Entities & Sent-Out Samples

### 8.1 `ExternalEntity` **[Auditable]**

A single physical table using type discrimination to represent the three external-entity kinds (treating doctor, referral/contract entity, partner lab), reflecting that they share the same structural shape while differing in which fields are meaningful.

| Column | Type | Notes |
|---|---|---|
| ExternalEntityId | int PK | |
| EntityType | tinyint | TreatingDoctor / ReferralOrContract / PartnerLab |
| Name | nvarchar(200) | |
| City | nvarchar(100), nullable | |
| Address | nvarchar(300), nullable | |
| Phone | nvarchar(30), nullable | |
| Fax | nvarchar(30), nullable | |
| ResponsiblePersonName | nvarchar(150), nullable | |
| ResponsiblePersonPhone | nvarchar(30), nullable | |
| PriceListId | int FK → PriceList, nullable | Not applicable to TreatingDoctor entities |
| DiscountOrCommissionPercent | decimal(5,2), nullable | Applicable to TreatingDoctor entities |
| GeneratedIdCode | nvarchar(50), nullable | Produced by the entity's ID-generation action |

### 8.2 `SentOutSample` **[Auditable]**

| Column | Type | Notes |
|---|---|---|
| SentOutSampleId | int PK | |
| PatientTestId | int FK → PatientTest | |
| ExternalLabEntityId | int FK → ExternalEntity | Must reference an entity of type PartnerLab |
| CostPrice | decimal(18,2) | Paid to the external lab |
| PatientPrice | decimal(18,2) | Charged to the patient |
| SentAtUtc | datetime2 | |

### 8.3 `SentOutSamplePayment`

| Column | Type | Notes |
|---|---|---|
| SentOutSamplePaymentId | int PK | |
| SentOutSampleId | int FK → SentOutSample | |
| AmountPaid | decimal(18,2) | |
| PaidAtUtc | datetime2 | |
| PerformedByUserId | int FK → User | |

Multiple partial payments toward a single sent-out sample are supported by allowing multiple rows per `SentOutSampleId`; full settlement is reached when the sum of payments equals the cost price.

---

## 9. Group F — Users, Permissions & Attendance

### 9.1 `User` **[Auditable]**

| Column | Type | Notes |
|---|---|---|
| UserId | int PK | |
| UserName | nvarchar(100) | Unique |
| PasswordHash | nvarchar(300) | Main password |
| InternalWindowsPasswordHash | nvarchar(300) | Secondary password gating sensitive windows |
| IsAbsolutePermission | bit | |
| DiscountLimitPercent | decimal(5,2) | |
| BlockPrintOnRemainingBalance | bit | |
| WorkStartTime | time, nullable | |
| WorkEndTime | time, nullable | |
| HasBreakPeriod | bit | |
| BreakDurationMinutes | int, nullable | |
| LastLoginAtUtc | datetime2, nullable | |
| IsActive | bit | Deactivation preferred over deletion to preserve historical audit references |

### 9.2 `Permission` (catalog — fixed set of thirteen granular items)

| Column | Type | Notes |
|---|---|---|
| PermissionId | int PK | |
| Code | nvarchar(50) | Stable code, e.g., `ADD_EDIT_PATIENT`, `PT_AUDIT_ACCESS` |
| Description | nvarchar(300) | |

### 9.3 `UserPermissionGrant`

| Column | Type | Notes |
|---|---|---|
| UserId | int FK → User | Composite PK with PermissionId |
| PermissionId | int FK → Permission | |

The permission granting the restricted P/T audit view is treated as an ordinary row in this table, but the application layer additionally enforces that only users who are either flagged `IsAbsolutePermission` or hold this specific grant may access P/T data — a defense-in-depth rule, not a schema-level distinction.

### 9.4 `AttendanceRecord`

| Column | Type | Notes |
|---|---|---|
| AttendanceRecordId | int PK | |
| UserId | int FK → User | |
| CheckInAtUtc | datetime2 | |
| BreakStartAtUtc | datetime2, nullable | |
| BreakEndAtUtc | datetime2, nullable | |
| CheckOutAtUtc | datetime2, nullable | |
| OvertimeMinutes | int, nullable | Computed at check-out |
| LatenessMinutes | int, nullable | Computed at check-in |

Visible only to the system manager, enforced at the application layer.

---

## 10. Group G — Audit & Traceability

The restricted audit surfaces (P and T) are realized entirely by columns already defined on `Patient` (§4.2, inherited audit columns), `PaymentOperation` (§7.1), and `PatientTest` (§6.1). No additional physical tables are required for Group G; this section exists to record, in one place, exactly which columns constitute each restricted view, for direct use when building the corresponding screens.

| Restricted view | Source columns |
|---|---|
| **P — patient-record activity** | `Patient.CreatedByUserId/CreatedAtUtc` (registering user), `Patient.ModificationCount`, `Patient.LastModifiedByUserId/LastModifiedAtUtc`, `PaymentOperation.ReceivedByUserId` (per patient) |
| **T — per-test result activity** | `PatientTest.EnteredByUserId/AtUtc`, `PatientTest.ReviewedByUserId/AtUtc`, `PatientTest.LastPrintedByUserId/PrintCount/AtUtc`, `PatientTest.DeliveredByUserId/AtUtc` |

---

## 11. Group H — Accounting & Cash Management

### 11.1 `CashMovement` **[Auditable]**

| Column | Type | Notes |
|---|---|---|
| CashMovementId | int PK | |
| MovementType | tinyint | Disbursement / Deposit |
| Amount | decimal(18,2) | |
| RelatedExternalEntityId | int FK → ExternalEntity, nullable | For company/delegate-related movements |
| PerformedByUserId | int FK → User | |
| OccurredAtUtc | datetime2 | |
| Notes | nvarchar(500), nullable | |

### 11.2 Inventory and cash-drawer figures

Daily/weekly/monthly/annual/custom-period inventory figures (total samples, discounts, collected/uncollected amounts, cash supplies, remaining-to-lab, commissions and shares, safe cash, net profit) are **computed reporting aggregates** over `PatientTest`, `PaymentOperation`, `SentOutSample`/`SentOutSamplePayment`, and `CashMovement`, filtered by the selected period. No dedicated storage table exists for these figures, consistent with the requirement that all such figures be derived from the same live, shared data on every workstation.

### 11.3 Company/delegate accounts

Per-entity balances for `حساب شركات ومندوبين` are likewise computed by aggregating `CashMovement` and relevant `PaymentOperation`/`SentOutSamplePayment` rows filtered by `RelatedExternalEntityId` / the entity's associated patients, over the selected period.

---

## 12. Group I — System Configuration

Configuration tables hold **exactly one row each** (enforced by a fixed, non-editable primary key value of 1), except where noted.

### 12.1 `SystemSettings` (single row)

| Column | Type | Notes |
|---|---|---|
| DefaultAccountType | tinyint | |
| PrintLabIdInsteadOfPatientId | bit | |
| AutoReviewAndComplete | bit | Interacts with the lifecycle per the status model |
| ResultScreenAccountDisplayMode | tinyint | Display-configuration only; does not alter billing |
| SaveTreatingDoctorOnlyFromEntityWindow | bit | |
| EnablePatientNameSearchAssist | bit | |
| DisableAutoTitleInsertion | bit | |
| PrintFileExternalBarcode | bit | |
| PrintDateTimeOnTubeBarcode | bit | |
| PrintAccountInsteadOfDateOnReport | bit | |
| DailyBackupEnabled | bit | |
| DailyBackupPath | nvarchar(300), nullable | |

### 12.2 `ReportSettings` (single row)

| Column | Type | Notes |
|---|---|---|
| PageMarginLeftCm | decimal(5,2) | |
| PageMarginBottomCm | decimal(5,2) | |
| ReportTopSpaceCm | decimal(5,2) | Maximum 8 cm |
| PaperSize | tinyint | A4 / A5 |
| HeaderFooterMode | tinyint | None / Words / Images |
| DoctorSignatureEnabled | bit | |
| HeaderColor | nvarchar(9), nullable | |
| FooterColor | nvarchar(9), nullable | |
| HistorySortMode | tinyint | By lab code / By patient name |
| HistoryAutoDisplayEnabled | bit | |

### 12.3 `ReceiptSettings` (single row)

| Column | Type | Notes |
|---|---|---|
| TopMarginCm | decimal(5,2) | |
| Currency | nvarchar(10) | e.g., L.E. |
| PickupTimeDefault | time, nullable | |
| PrintOnce | bit | |
| TestDetailDisplayMode | tinyint | Hide / Show / Show with code |
| CashierPrinterEnabled | bit | |
| HeaderFooterMode | tinyint | Words / Images |

### 12.4 `EnvelopeSettings` (single row)

| Column | Type | Notes |
|---|---|---|
| TopMarginCm | decimal(5,2) | |
| HeaderFooterMode | tinyint | None / Words / Images |
| SuppressCaptions | bit | |

### 12.5 `EnvelopePrintItemPosition`

| Column | Type | Notes |
|---|---|---|
| ItemName | nvarchar(50) PK | Name / Code / ReferralEntity / Date |
| IsEnabled | bit | |
| LeftOffsetCm | decimal(5,2) | |
| TopOffsetCm | decimal(5,2) | |

### 12.6 `PrinterAssignment`

| Column | Type | Notes |
|---|---|---|
| OutputType | tinyint PK | Reports / Barcode / Envelope / Receipt |
| PrinterName | nvarchar(200) | |

**Design note — database connection settings.** The server name, login, and database name used to reach the shared SQL Server database (§14, "Database server settings") are **not** stored as a row inside that same database, for the evident reason that the connection details must be available before a connection can be made. These values belong in a local application configuration file on each workstation, outside the scope of this data model.

**Design note — workstation configuration store.** The effective connection settings are read from `%ProgramData%\TopLab\appsettings.json` (ADR-0025). The first-run setup wizard writes this file after validating connectivity; the committed `appsettings.example.json` documents the safe Integrated-Security default. The database schema itself is unaffected — `LabId` stays on the existing `nvarchar(30)` `Patients` column.

---

## 13. Business-Rule-to-Schema Mapping

| Rule | Schema mechanism |
|---|---|
| **BR-01** — Patient aggregate status precedence | Computed at query time from `PatientTest` flags and the patient's outstanding balance; never stored, per §11 of this document and §7 of the Architecture Blueprint |
| **BR-02** — Identifier distinction | `Patient.PatientId` (PK, unique per visit) vs. `Patient.LabId` (shared, non-unique grouping value) — see §4.1 |
| **BR-03** — Multiple phone numbers | `PatientPhoneNumber` one-to-many child table, indexed on `PhoneNumber` |
| **BR-04** — Age-unit-sensitive reference ranges | `ReferenceRange.AgeUnit` is part of every range's matching key; no unit conversion occurs in queries |
| **BR-05** — Old reference values persist | `PatientTest.ResultFlag` is evaluated and stored at entry time, not recalculated retroactively when `ReferenceRange` changes |
| **BR-06** — Discount limit | Enforced at the application layer against `User.DiscountLimitPercent` before a `PaymentOperation` row is written |
| **BR-07** — Print-block on balance | Enforced at the application layer using `User.BlockPrintOnRemainingBalance` and the computed patient balance |
| **BR-08** — Delivered means physical handover | `PatientTest.IsDelivered` is set only through the in-person delivery use case; no online/SMS/e-mail/fax pathway exists anywhere in the schema |
| **BR-09** — Single branch | No branch column exists on any table in this document |
| **BR-10** — Default account type | `SystemSettings.DefaultAccountType`, applied to `Patient.AccountType` at registration unless overridden |
| **BR-11** — Restricted audit access | Enforced at the application layer per §10; not a schema-level table separation |
| **BR-12** — Pregnancy/children antibiotic display | `Antibiotic.IsPregnancyFlagged` / `IsChildrenFlagged`, filtered against `Patient` sex/pregnancy indication and age at query time |
| **BR-13** — Completion duration drives pickup time | `Test.CompletionDurationMinutes`, used to compute `Patient.PickupDateUtc` at ordering time |

---

## 14. Indexing Guidance

| Table | Index | Purpose |
|---|---|---|
| Patient | LabId | Cross-visit search |
| Patient | FullName | Name search (exact/partial) |
| Patient | NationalId | Search by national ID |
| Patient | RegistrationDateUtc | Date-range search, daily lists |
| PatientPhoneNumber | PhoneNumber | Search by any stored number |
| PatientTest | PatientId | Loading a patient's full test set |
| PatientTest | (IsReviewed, IsPrinted, IsDelivered) | Status-filter searches (not reviewed / not printed / not delivered) |
| PaymentOperation | PatientId | Account computation |
| User | UserName | Login lookup |

---

## 15. Alignment with Non-Functional Requirements

- **Single source of truth.** Every computed figure (account balances, aggregate status, inventory totals) is derived from this shared schema at query time, so every workstation on the LAN always presents identical figures.
- **Recoverability.** The daily backup and Database Maintenance capabilities operate over this entire schema as a single unit; no table is excluded from backup scope.
- **Auditability.** Every mutable business entity is auditable per §2, and the restricted P/T views are fully satisfiable from the columns defined here without additional storage.

---

*End of document.*
