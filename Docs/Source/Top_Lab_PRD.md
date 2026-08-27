# Top-Lab — Product Requirements Document

## نظام توب لاب لإدارة معامل التحاليل الطبية

---

## 0. Document Control

| Field | Value |
|---|---|
| Product | **Top-Lab (نظام توب لاب)** |
| Document title | Top-Lab — Product Requirements Document |
| Version | 1.0 |
| Date | 2026-08-27 |
| Status | **Final** — approved requirements baseline |
| Product scope | A commercial desktop **Windows** Laboratory Management System (LMS) for medical analysis laboratories, with an **Arabic-first** user interface, operating on a **local area network (LAN)** over a **single shared SQL Server database**, serving a **single branch**, with **no external-Internet dependency** for any product function. |
| Document purpose | This document is the authoritative functional requirements baseline for Top-Lab prior to architecture, database design, UI specifications, use-case development, implementation planning, and development. It is a requirements baseline, not an implementation specification. |

---

## 1. Product Purpose and Scope

### 1.1 Product purpose

**Top-Lab** manages the complete operational cycle of a single-site medical analysis laboratory:

- registering patients and their requested tests, with mandatory and optional patient data, multiple telephone numbers, medical history and notes;
- computing patient accounts automatically from configured prices, handling discounts, partial payments, extra ("+") charges and full settlement, with correction of payment operations;
- tracking sample collection/separation and printing sample barcodes, receipts and work sheets;
- entering results — including multi-analyte profile reports and culture & antibiotic-sensitivity reports — under a controlled **result lifecycle** (entry → finish → review → print → physical delivery), with a formal **seven-state patient-level status model** (§8);
- printing single, combined, blank and medical-history reports with fully configurable layout;
- maintaining a persistent **Lab ID** per patient (distinct from the per-registration **Patient ID**) enabling cross-visit search and automatic medical-history retrieval;
- managing the test catalog, reference ranges (by sex and age unit), price lists, fixed comments, custom groups and work groups (Log);
- managing external entities (treating doctors, referral/contract entities, partner laboratories) with entity-specific price lists, discounts and commissions;
- managing tests sent to external laboratories, including follow-up and settlement of external-lab accounts;
- enforcing privacy and preventing tampering through a granular user-permissions system, a secondary internal-windows password, **P/T restricted audit & traceability** (§11), and user attendance tracking;
- producing laboratory statistics and periodic financial inventory of the cash drawer;
- protecting data through daily database backup and database maintenance functions.

### 1.2 Included scope

1. Application access (login) and main navigation.
2. Patient registration, test/group ordering, outside-drawn sample flags, medical history capture, **multiple patient telephone numbers with search by any of them**.
3. Patient billing: automatic totals, discounts, partial payments, "+" extra charges, full settlement, payment-operation correction.
4. Result entry, result lifecycle (Status / Finish / Verify / Print / Export), specialized multi-analyte profile reports, culture & sensitivity entry.
5. Report production: single report, combined report, blank report, automatic/manual/separate/multi-patient medical-history reports.
6. Patient search (multi-criteria incl. **any stored telephone number**), Lab ID creation, full visit history per patient.
7. Physical result delivery with account settlement at handover, including the undelivered-results list.
8. Work sheets: by patient names/codes, by test(s), by work group (Log), plus test-frequency classification.
9. Test catalog, reference ranges (sex/age-unit sensitive), price lists, fixed comments, custom groups, test groups and Log work groups.
10. External entities (treating doctors, referral/contract entities, partner labs) and Lab-to-Lab pricing.
11. Culture and antibiotic configuration, including **user-defined culture types** and pregnancy/children-conditional antibiotics.
12. Sent-out sample configuration, follow-up and settlement.
13. Users, granular permissions (absolute/limited), per-user discount limit, internal windows password, attendance tracking.
14. Statistics (patients, tests, sent-out samples, user productivity).
15. Financial inventory and cash-drawer accounting, cash disbursement/deposit, **company and delegate accounts**.
16. Sample collection & separation tracking.
17. System, report, receipt, envelope, printer, database-server, backup and maintenance settings; **result-screen account display setting**.
18. **P/T restricted audit & traceability** (System Administrator / Absolute Permissions only).
19. Utilities (Tools) area.

### 1.3 Excluded scope (binding — see §17)

Online result delivery (all forms); patient/doctor/lab web portals; web-based result viewing; SMS, e-mail and fax result notification; web-result credentials; online-service administration; multi-branch operation and all branch-scoped behavior; installation/SQL-Server-setup content; **patient card / patient ID card printing in any form**; and **laboratory equipment/device tracking in any form**.

### 1.4 Operating model

Desktop **Windows** application; **Arabic-first** UI; **LAN** environment; multiple workstations sharing **one SQL Server database** at **one physical site**; **single branch**; no external-Internet dependency.

### 1.5 Technology baseline

| Concern | Mandated technology |
|---|---|
| Application type | **Windows desktop** application |
| Runtime / framework | **.NET 8** |
| UI framework | **WPF** with the **MVVM** (Model–View–ViewModel) pattern |
| Application architecture | **Clean Architecture** |
| Data access | **Entity Framework Core** |
| Database | **Microsoft SQL Server** — one shared database |
| Deployment | Local **LAN**; multiple workstations connected to the shared database at one site |
| Connectivity | **No Internet dependency** for any product function |

---

## 2. Product Vision and Principles

**Vision.** Top-Lab gives a medical analysis laboratory one reliable desktop system for the full cycle from patient registration to physical result delivery, with financial control, traceability and data protection — without any dependency on the Internet.

**Principles.**

- **Offline by design.** Every product function operates on the local network without any Internet dependency; results reach the patient by physical handover of the printed report.
- **Requirements, not design.** This document specifies behavior, business rules, and data concepts. Beyond the mandated technology baseline (§1.5), it prescribes no internal architecture, schema, code, or deployment design.
- **Operational safety first.** Status indicators and audit capabilities exist so that unfinished work remains visible and every action is attributable to a user.

---

## 3. Glossary

| Term (Arabic) | English | Meaning in Top-Lab |
|---|---|---|
| مريض | Patient | Person registered for whom tests are ordered, resulted and billed |
| كود المريض | **Patient ID** | System-generated code of the patient's registration record — **distinct from Lab ID** (§9) |
| لاب آي دي / كود المعمل | **Lab ID** | Persistent, separately created patient identifier enabling cross-visit search and medical history |
| تحليل | Test / Analysis | Catalog entry: name, report name, receipt name, group, barcode, completion duration, prices, reference ranges |
| القيم المرجعية / النورمالات | Reference ranges (Normals) | Valid result intervals per test, definable by sex and **age unit** (day/month/year), with optional low/high auto-comments |
| مجموعة تحاليل (Custom Group) | Custom group | Lab-defined bundle of tests with own prices, addable to a patient in one action |
| مجموعات العمل (Log) | Work group (Log) | Named work grouping for which work sheets can be printed |
| ورقة العمل | Work Sheet | Printable work list of patients or of tests for a defined period |
| الحالة / تمت / مراجعة / طبعت | Status / Finish / Verify / Print | Result lifecycle flags and their setter buttons |
| التسليم | Delivery | **Physical handover** of the printed result report to the patient |
| مزرعة | Culture | Microbiology culture test type with dedicated Culture/Sensitivity entry screens; **user-extensible** set (FR-M15-001) |
| مضاد حيوي | Antibiotic | Antibiotic attached to a culture, classified into sensitivity categories (Highly/Moderate/Low/Resistant) |
| عينة مرسلة للخارج | Sent-out sample | Test performed by an external lab; has a Cost Price (paid to the external lab) and a Patient Price |
| جهة إحالة / تعاقد | Referral / contract entity | External party: treating doctor, sent-sample lab, referral or contract entity |
| قائمة أسعار | Price list | Named list of per-test prices assignable to contract/referral entities and labs |
| الجرد وحساب الدرج | Inventory & cash drawer | Daily/weekly/monthly/annual/custom financial inventory and cash-drawer accounting |
| الخزينة / توريدات نقدية | Safe / cash supplies | Cash available in the safe; cash deposited into the drawer |
| عمولات ونسب | Commissions & shares | Entity/doctor commission and share amounts surfaced in inventory |
| كلمة مرور النوافذ الداخلية | Internal windows password | Secondary password gating sensitive internal windows (a default exists; must be changed at deployment) |
| الحضور والانصراف / فترة الراحة | Attendance / break period | User check-in, optional break, check-out; overtime and lateness tracking |
| تقرير مجمع | Combined report | Single report combining results of multiple selected tests |
| تقرير فارغ | Blank report | Printable report containing only patient data, optionally fillable |
| التاريخ المرضي | Patient history | Automatic or manual inclusion of a patient's previous results of the same test in reports |
| سحب وفصل العينات | Sample draw/separation | Marking which patient samples have been drawn/separated |
| ظرف | Envelope | Printed result envelope with configurable per-item layout and positioning |
| الألقاب | Titles | Patient honorifics (e.g., السيد / السيدة); configurable, with automatic insertion during registration that can be disabled (FR-M02-007) |
| P / T | P/T audit buttons | **Restricted audit & traceability** controls on the results screen (patient-level P; per-test T) — System Administrator / Absolute Permissions only (§11) |
| تهيئة النظام | System initialization | Settings-dashboard function preparing the system for a newer version |
| صيانة قاعدة البيانات | Database Maintenance | Backup / restore / update functions for the database |
| حالة المريض (المجمعة) | Patient aggregate status | One of the seven operational states, derived from ALL of the patient's analyses by the precedence rule (§8) |

---

## 4. Functional Module Map

Top-Lab comprises the following functional modules.

| Module | Name | Summary |
|---|---|---|
| M01 | Application Access & Main Navigation | Login, navigation bar, status bar, permission denial behavior |
| M02 | Patient Registration & Test Ordering | Patient data, **multiple phone numbers**, medical history, test/group ordering, outside-drawn flag, quick print actions |
| M03 | Patient Billing & Account Settlement | Auto totals, discount/paid, "+" charges, settlement, payment correction |
| M04 | Results Entry & Result Lifecycle | Result entry, Status/Finish/Verify/Print/Export, filters, comments, range refresh, **P/T restricted audit buttons** |
| M05 | Specialized Profile Result Reports | Multi-analyte profile entry with units, L/H flags, per-row verify/print |
| M06 | Culture & Sensitivity Result Entry | Culture fields and antibiotic classification entry |
| M07 | Combined, Blank & History Reports | Combined report, blank report, auto/manual/separate/multi-patient history |
| M08 | Patient Search, Lab ID & Visit History | Multi-criteria search (incl. **any phone number**), result-state filters, Lab ID, visit history, **seven-state aggregate status display** |
| M09 | Result Delivery & Settlement at Handover | Undelivered-results list, account check at physical delivery |
| M10 | Case Tracking, Audit & Traceability (P/T) | P button: registering user, edit count, last modifying user, payment-receiving users. T button: per-test entry/review/print/delivery actors with date/time and print count. **Restricted access** |
| M11 | Work Sheets | By patients, by tests, by Log group, frequency classification |
| M12 | Test Catalog & Reference Ranges | Test CRUD, groups, reference ranges by sex/age unit, range comments |
| M13 | Price Lists, Comments & Custom Groups | Contract price lists, fixed test comments, custom groups |
| M14 | External Entities | Doctors, referral/contract entities, labs; price lists, discount/commission |
| M15 | Culture & Antibiotic Configuration | **User-defined culture types** (in-app); antibiotic attach; pregnancy/children flags |
| M16 | Sent-Out Samples | Sent-out configuration, follow-up, external-lab settlement |
| M17 | User & Permission Management | Users, absolute/limited permissions, discount limit, internal password |
| M18 | Attendance & Time Tracking | Check-in/break/check-out; manager-only overtime/lateness |
| M19 | Statistics | Patients, tests, sent-out, productivity statistics |
| M20 | Inventory & Lab Accounting | Cash drawer inventory, per-element inventories, disbursement/deposit, **company & delegate accounts** |
| M21 | Sample Collection & Separation | Drawn/separated marking per patient |
| M22 | System & Print Settings | Report/receipt/envelope/printer/server settings; **result-screen account display setting**; backup & maintenance |
| M23 | Utilities (Tools) | Test/Image/Shortcut libraries, unit converter, calculator, stopwatch, purchases list, phone book |

---

## 5. Functional Requirements by Module

> Requirements are numbered per module as **FR-Mxx-nnn**.

### M01 — Application Access & Main Navigation

- **FR-M01-001** — Top-Lab shall launch from a desktop icon and present a **login window** with: **User Name**, **Password**, a **show-password-characters** option, a **remember-login** option, and **Sign in** / **Exit** actions. A database-connectivity indicator is displayed at login.
- **FR-M01-002** — After login, the main window shall present a top navigation bar with the items: **Patients, Laboratory, Work sheet, Tools, Accounts, Statistics, Users, System, Setting, About Us, Exit**.
- **FR-M01-003** — The **Patients** central actions shall include: **إضافة وتعديل بيانات المرضى** (add/edit patient data), **إدخال نتائج التحاليل** (result entry), **بحث عن مريض** (patient search), **تسليم نتائج المرضى** (result delivery).
- **FR-M01-004** — A **status bar** shall display the logged-in user name, last-login date/time (with a first-login indication), a shared-database connectivity indicator, and the current date/time.
- **FR-M01-005** — Attempting an action for which the user has no permission shall display the denial message: "**أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام**".

### M02 — Patient Registration & Test Ordering

- **FR-M02-001** — A new patient shall be added via **Patients → إضافة وتعديل بيانات المرضى → إضافة**. Patient **name, sex and age** are the mandatory basic data; address, national ID (رقم البطاقة) and treating doctor (الطبيب المعالج) are optional.
- **FR-M02-002** — Age shall be captured **with its unit** (day / month / year); reference-range matching is age-unit sensitive (FR-M12-005).
- **FR-M02-003** — **Multiple telephone numbers per patient.** During patient registration, Top-Lab shall allow the user to enter **one or more telephone numbers** for the patient — including two or three numbers. A patient is **not** restricted to a single telephone number. This is an integral patient-identification capability, **not** a standalone phone-book feature.
- **FR-M02-004** — A dedicated **ملحوظات (notes)** field shall accept patient notes; these notes appear in the **results-entry window** and the **result-delivery window**.
- **FR-M02-005** — Patient **medical history** shall be captured: current medications (diabetes, blood-pressure, antivirals, antibiotics, anticoagulants, liver treatment, …), conditions (anemia, SLE, renal failure, hypertension, arthritis, …), whether contrast X-ray or ultrasound was performed within the last two days, and **fasting status** (صائم — عدد ساعات الصيام). This history is viewable via the **بيانات المريض** button in the results-entry window.
- **FR-M02-006** — The registration screen shall display: a system-generated **كود المريض (Patient ID)**; a **VIP** flag; a separate **Lab. ID** field; a **title (اللقب)** for the patient name; sex (Male/Female); **account type** (نوع الحساب, default per system settings, e.g., Individual); mobile/phone fields supporting multiple numbers (FR-M02-003); national ID; address; treating doctor; **referral entity** (جهة الإحالة); and **registration and pickup dates** (تاريخ الدخول / تاريخ الاستلام).
- **FR-M02-007** — **Patient titles.** The system shall support configurable patient titles/definitions (**القاب وتعريفات المرضى**, accessible from the System pane), automatic insertion of titles during registration, and a **system setting to disable automatic title insertion** (FR-M22-008).
- **FR-M02-008** — Sample-type flags include **Urine, Stool, Blood, Semen, CSF**. When a sample was drawn outside the lab, the user flags **Taken Outside Lab** for that test; the flag appears as a note in the patient's test report.
- **FR-M02-009** — Tests shall be added by double-clicking the test in the test list (it appears under **Patient tests**); double-clicking a chosen test removes it. An **All** button below the Patient tests list deletes **all** of the patient's tests, available **only when the patient is first added**.
- **FR-M02-010** — A complete predefined **test group** (e.g., Kidney Function, Liver Profile) shall be addable at once by selecting **TG** and double-clicking the group name; a single test can be removed from the added group by double-clicking it; registration completes via **حفظ** (Save).
- **FR-M02-011** — Patient data and tests shall be editable afterwards via **Patients → Add/Edit → قائمة المرضى → select registration day → select patient → تعديل → حفظ**; alternatively via patient search, or via the **بيانات المريض** button in the results-entry window.
- **FR-M02-012** — The registration screen shall provide direct action buttons: **القائمة الرئيسية، نتائج التحاليل، شاشة المرضى، حفظ، تعديل، حذف، تراجع، اضافة، الايصال، الباركود، ورقة العمل، خالص، موافق**.

### M03 — Patient Billing & Account Settlement

- **FR-M03-001** — The patient account total shall appear in **إجمالي التحاليل**, computed automatically from stored test prices. The billing block shows: **إجمالي التحاليل، الخصم، قيمة الخصم، التكلفة بعد الخصم، المدفوع سابقا، الباقي للمعمل، الباقي للمريض، المدفوع**.
- **FR-M03-002** — The paid amount and any discount shall be entered; pressing **Enter** once performs the calculation and pressing Enter a second time saves the operation.
- **FR-M03-003** — An amount **unrelated to tests** (extra service/cost) shall be addable by entering it in the paid field preceded by a "**+**" sign; it is added to the test total.
- **FR-M03-004** — When the patient pays in full, the user presses **خلاص** then **موافق**.
- **FR-M03-005** — Pressing the **إجمالي التحاليل** icon shall open a window listing payment operations; an operation can be **deleted (حذف)** or **edited (تعديل)**, e.g., to correct a wrong value or settle the account.
- **FR-M03-006** — Discounts granted by a user are constrained by that user's configured **discount limit** (FR-M17-004).
- **FR-M03-007** — The users who **received payments** for a patient shall be recorded and inspectable via the restricted **P** audit function (§11).

### M04 — Results Entry & Result Lifecycle

- **FR-M04-001** — Results shall be entered via **Patients → إدخال نتائج التحاليل**. The right panel lists patients registered today with their **count**; single-clicking a patient shows their tests; double-clicking a test opens its report for data entry, then **حفظ**.
- **FR-M04-002** — The results screen header shall show: **الكود (code), الاسم (name), النوع (sex), الجهة (entity), السن (age)**, and **Lab ID**.
- **FR-M04-003** — The results table columns are: **Test Abbreviation, P, T, Result, Status, Finish, Verify, Print, Export**. **P and T are audit & traceability buttons** (§11): **P** opens patient-record activity inspection; **T** opens per-test result audit. Both are **restricted to the System Administrator or users with Absolute Permissions**. The delivery window's results table additionally shows a **Price** column (FR-M09-001).
- **FR-M04-004** — **Result lifecycle.** Each individual result carries a **Status** (current state) and lifecycle flags set via the buttons **تمت (Finish), مراجعة (Verify), طبعت (Print)**; **Export** marks results exported to a local file (FR-OUT-09). A **ملاحظات (notes)** field accompanies the status controls. The result-state search filters (not entered / not reviewed / not printed / not delivered) reflect the lifecycle (FR-M08-004). **"Delivered" = physical handover** of the printed report to the patient (M09). The individual-result lifecycle and its relationship to the **patient-level aggregate status** are formally specified in §8.
- **FR-M04-005** — The results screen shall provide the buttons: **القائمة الرئيسية، طباعة ظرف، تقرير فارغ، التاريخ المرضي، طباعة، معاينة، تقرير مجمع، بيانات المريض**, plus **Refresh** and account-type filters: **Individual, LabToLab, Contracts, VIP, Free, All**.
- **FR-M04-006** — **معاينة** (print preview) shall display the report exactly as it will print; **طباعة** prints it.
- **FR-M04-007** — A fixed comment registered for a test shall be insertable into a report via the **Comment** icon at the bottom of the report, which opens a dropdown of that test's saved comments; clicking a comment adds it to the patient's report.
- **FR-M04-008** — After a test's reference values are modified, previously registered patients remain on the old values; to apply new values, the user opens the report, clicks the reference-values cell, presses **تحديث** (Update), then **موافق**.

### M05 — Specialized Profile Result Reports

- **FR-M05-001** — Multi-analyte tests (profiles, e.g., Coagulation Profile) shall open a **specialized report screen** containing the replicated patient header and a per-analyte table with columns: **Test Name, Result, Unit, L or H, Normal Ranges, Verified, Print**.
- **FR-M05-002** — The screen shall support test-specific structured inputs (e.g., for coagulation: Bleeding/clotting min:sec; PT with **Control's time + ISI**, Patient's time, Concentration %, I.N.R., Ratio; APTT; Fibrinogen; FDP), a per-report **Comment** area, and matched **Normal Ranges** displayed per analyte.
- **FR-M05-003** — The specialized screen shall provide: **حفظ، طباعة، معاينة الطباعة، Patient History، رجوع، القائمة الرئيسية**.
- **FR-M05-004** — The **printed report** shall include: **Patient ID** with **barcode** (or Lab ID per the code-printing setting, FR-M22-008), patient name, **Age/Sex**, **Request Date**, **Printed In** (print timestamp), report title, results versus **Reference Range**, and a **Doctor's signature** element.

### M06 — Culture & Sensitivity Result Entry

- **FR-M06-001** — For cultures, pressing the **Culture** button shall open culture data entry with the fields: **Sample, Organism A, Organism B, Organism C, Culture Condition, Colony Count**.
- **FR-M06-002** — Pressing **Sensitivity** shall open antibiotic entry; antibiotics are entered by double-clicking from the right of the window (or select + **Add**) into one of four classifications: **Highly For, Moderate For, Low For, Resistant For**.
- **FR-M06-003** — The user can control display of **Sensitivity – Reference – Commercial Name** in the report; entry ends with **Save**.

### M07 — Combined, Blank & History Reports

- **FR-M07-001** — A **combined report (تقرير مجمع)** shall aggregate selected test results into one report: patient tests appear on the left under **Patient tests in our lab**; double-click a test (or select it and press the **green-arrow** icon) to move it right under **Available tests in report**; ordering is controlled with **up/down arrows**.
- **FR-M07-002** — A **blank report (تقرير فارغ)** containing only patient data shall be creatable; its data can be filled in and printed, or printed as a patient-data-only sheet.
- **FR-M07-003** — If a patient visits the lab more than once, the system shall **automatically insert** the patient's previous results of the same test into the result report when that test was performed before.
- **FR-M07-004** — The system identifies the patient for history purposes **by name** or **by lab code (Lab ID)**; sorting/aggregation settings for the history are configured in the report-settings window.
- **FR-M07-005** — A **system-wide setting** (in report settings) controls whether patient medical history is automatically displayed in reports; the setting can **disable** the auto-display.
- **FR-M07-006** — Pressing **Patient History (التاريخ المرضي)** shows tests the patient performed in previous visits; double-clicking a test inserts its previous results into the current report (including across groups and into combined reports).
- **FR-M07-007** — Selecting **"Print history in separate report"** shall produce the medical history as a standalone report.
- **FR-M07-008** — A history for **more than one patient in the same report** shall be possible: **Patients → search → select patient → إضافة**, repeat for others, then press **نتائج مجمعة** to choose the tests for which history is built.

### M08 — Patient Search, Lab ID & Visit History

- **FR-M08-001** — Patient search shall be accessible via **Patients → بحث عن مريض**, with criteria: patient name (exact or partial/random), treating-doctor name, sex, age, phone number or national-ID number, test, and patients within a date range.
- **FR-M08-002** — **Search by any stored telephone number.** The system shall allow searching for a patient using **any** of the telephone numbers associated with that patient (FR-M02-003). A successful telephone-number search shall allow the user to **retrieve the patient's existing record and associated patient data**.
- **FR-M08-003** — A system setting may **enable search assistance when entering a patient name** in the add/edit patient window (general setting, FR-M22-008).
- **FR-M08-004** — The system shall support finding results **not yet entered** (نتائج لم تدخل), and results **not reviewed, not printed, or not delivered**.
- **FR-M08-005** — A **Lab ID** shall be creatable for a patient after saving registration and tests by pressing **Lab Id** then **Save**; the Lab ID is saved as a distinguishing data element of the patient and **appears automatically** in the patient's data on subsequent registrations.
- **FR-M08-006** — Searching by **Lab ID** shall display **all of the patient's visits** to the lab with their dates.
- **FR-M08-007** — Patient lists shall display the patient's **aggregate status icon** computed by the seven-state precedence rule of §8.
- **FR-M08-008** — **Lab ID and Patient ID (كود المريض) are separate identifiers** and shall be treated as such throughout (§9). The Patient ID is the system-generated code of the registration record; the Lab ID is a separately created persistent identifier aggregating visits. Identifier-generation algorithms are not prescribed by this document (design-phase concern); the behavioral relationship is documented in §9.

### M09 — Result Delivery & Settlement at Handover

- **FR-M09-001** — Result delivery shall be accessible via **Patients → تسليم نتائج المرضى**. Patients are displayed per a **selected time period**, with **finished and unfinished** results visible. The window lists patients whose results are **not yet delivered (النتائج الغير مستلمة)**; the results table in this window shows **Test Name, Result, Status, Finish, Verify, Print, Export, Price**.
- **FR-M09-002** — At delivery, the system shall show the patient's **financial position** — paid, remaining-to-patient, remaining-to-lab — via the **حساب المريض** button, supporting settlement at **physical handover**.
- **FR-M09-003** — Where a permission blocks printing while a balance remains (FR-M17-004), the block applies before printing for delivery.
- **FR-M09-004** — The user who **delivered** a result to the patient shall be recorded with date and time and inspectable via the restricted **T** audit function (§11). Delivery completion feeds the patient-level status model (§8).

### M10 — Case Tracking, Audit & Traceability (P/T)

The complete requirements for this module are specified in §11 (Auditability and Traceability):

- **FR-M10-001** — Authorized users (System Administrator / Absolute Permissions only) shall be able to inspect, via **P**: which user registered the patient's data; the number of modifications made to the patient's data; the most recent user who modified it; and the payment/account-related user activity including the users who received payments.
- **FR-M10-002** — Authorized users shall be able to inspect, via **T** (per selected patient test/analysis): who entered the result; who reviewed it; who was responsible for printing it; how many times it was printed; and which user delivered it to the patient — each recorded activity with **date and time**.

### M11 — Work Sheets

- **FR-M11-001** — The system shall print a work sheet for a group of patients in a specified period, **by their names or their codes**.
- **FR-M11-002** — The system shall print a work sheet **for a single test or a selected group of tests** in a specified period. (Both modes — specific patients OR a specific group of tests — are required.)
- **FR-M11-003** — The system shall print a work sheet for a **work group (Log)** in the specified period.
- **FR-M11-004** — The system shall produce a **classification of a period's tests** showing how many times each test was performed.
- **FR-M11-005** — Access: **Work sheet** icon → **ورقة عمل بأسماء المرضى** (by patient names) or **ورقة عمل بأسماء التحاليل** (by test names).

### M12 — Test Catalog & Reference Ranges

- **FR-M12-001** — The test catalog shall be accessible via **System → بيانات التحاليل**, displaying all registered tests; tests are searchable by **test name**, **containing group**, or **test number**.
- **FR-M12-002** — Editing a test (تعديل) shall allow modifying: **test name, report name, receipt name, group, barcode, completion duration** (basis for computing the patient's pickup time), **sent-out flag with its price**, **patient price**, and **Lab-to-Lab price**. Save commits; **تراجع** cancels. A new test is addable via **إضافة تحليل** → enter name and remaining data → Save.
- **FR-M12-003** — The System pane shall provide access to **test groups** (مجموعات التحاليل) and **work groups (Log)** configuration.
- **FR-M12-004** — Reference values shall be addable/editable per test via the **القيم المرجعية** button. Adding a range (**إضافة مدى**) captures: **patient sex, age range, minimum value, maximum value**. An optional **low comment** appears automatically in the patient report when the result is below the minimum (double-clicking the comment field); an optional **High comment** likewise above the maximum. Ranges are editable (تعديل → حفظ) and deletable (حذف).
- **FR-M12-005** — **Business rule (age units):** for reference values typed **By Sex and Age** or **By Age Only**, normals must be entered **separately for each age unit**. A normal for ages "1 day – 60 days" matches patients registered as 15 or 35 days old, but a patient registered as **1 month** old is **not** matched — even though one month equals 30 days; a new normal must be added for each age unit (day – month – year).
- **FR-M12-006** — After a test's reference values change, previously registered patients remain on the old values until explicitly updated in the report (FR-M04-008).

### M13 — Price Lists, Comments & Custom Groups

- **FR-M13-001** — **Price lists (قوائم أسعار التعاقدات)** shall be creatable (إضافة → name → OK); tests added with their price; prices editable; tests removable; list name editable; list deletable; a list printable via **طباعة القائمة** (also surfaced via the System pane's "print test price list" action).
- **FR-M13-002** — **Fixed comments** per test shall be registrable via **System → Test Comments → إضافة**: choose the test, write the comment, Save; multiple comments per test allowed; comments are inserted into reports per FR-M04-007.
- **FR-M13-003** — **Custom Groups** bundling several tests with prices shall be creatable (إضافة مجموعة → name → Save; then إضافة تحليل with price). The group can be added to a patient; a group can be deleted.

### M14 — External Entities (Doctors / Referral / Contracts / Labs)

- **FR-M14-001** — External entities shall be managed via **System → الجهات الخارجية والمعامل → إضافة**, choosing entity type: **treating doctor / sent-out samples / referral or contract entity**.
- **FR-M14-002** — For a **treating doctor**: enter name and data; **no price list is assigned** — the patient's account follows the test price in **Patient Price**; a **discount or commission percentage** can be specified; then Save.
- **FR-M14-003** — For a **referral/contract entity**: enter name and data (city, address, phone, fax, responsible-person name and phone, …); **assign a price list** — prices are computed per that list; the **Lab to Lab** list (for labs) or any previously added list can be chosen. The saved entity appears in the entities list on the left.
- **FR-M14-004** — Entities are editable (تعديل → حفظ) and deletable (حذف → موافق).
- **FR-M14-005** — The entity record shall support an **ID** generation action (used for entity identification).
- **FR-M14-006** — A system setting controls whether the treating doctor is **saved only from the external-entities window** (not auto-saved during patient entry); another setting defines behavior when the referral-entity field is left empty (a default placeholder per patient sex).

### M15 — Culture & Antibiotic Configuration

- **FR-M15-001** — **Users shall be able to add new culture test types** beyond predefined examples such as urine, stool and CSF culture (e.g., blood culture), registered in the test catalog under the group **CULTURE AND SENSITIVITY**, entirely as an **in-app, user-facing capability**.
- **FR-M15-002** — Antibiotics shall be attached to a culture via **System → Culture Antibiotics**: select the culture (its attached antibiotics and their count appear), press **إضافة**, double-click the antibiotic from the list, then Save. If the antibiotic is not in the list, its data is **typed manually**, then Save. An antibiotic is removed via حذف.
- **FR-M15-003** — An antibiotic can be flagged **Pregnant**; it then appears in the culture only when pregnancy is indicated in the patient's data.
- **FR-M15-004** — An antibiotic can be flagged **Children**; the system recognizes children as patients younger than **12 years** (male or female), and a child-designated antibiotic appears only for a child patient.

### M16 — Sent-Out Samples

- **FR-M16-001** — A test performed outside the lab shall be configured by editing its data: flag "**Sent outside Lab**", choose the external lab, record the external price in "**Cost Price**" and the patient price in "**Patient Price**".
- **FR-M16-002** — Sent-out accounts shall be followed up via **Accounts → العينات المرسلة للخارج**: choose period → **عرض** shows the period's sent tests; selecting a sent sample identifies the **destination lab**, the **total samples sent** to that lab in the period, and that lab's **total account, paid and remaining**.
- **FR-M16-003** — Payment to the external lab: press **ترسل إلى**, enter the amount paid from the sent test's price, press **موافق**; for full payment press **خلاص**; repeat for all tests.
- **FR-M16-004** — Search by a specific entity filters the display to only the tests sent to that lab.

### M17 — User & Permission Management

- **FR-M17-001** — The system shall support creating users and assigning each user work permissions (**Users** icon → **إنشاء مستخدمين**).
- **FR-M17-002** — Opening user creation shall request the **internal windows password** (كلمة مرور النوافذ الداخلية). A **default** exists; the responsible user **shall change it at deployment** (NFR-02/03).
- **FR-M17-003** — Adding a user: press **إضافة**, enter the user name and the user's **main and secondary passwords** (كلمة المرور / كلمة مرور النوافذ الداخلية), assign permissions, then **حفظ**; the user appears in the users list. The window shows "**وقت آخر دخول**" (last login time), **working-hours scheduling** (وقت بداية العمل / وقت انتهاء العمل, AM/PM), and an **employee break-period** definition (هل توجد فترة راحة للموظف + duration).
- **FR-M17-004** — Permissions shall support **absolute / limited** modes (صلاحية مطلقة للمشرفين ومديري النظام / صلاحية محدودة لمستخدمي النظام) and the following granular items:
  1. إضافة وتعديل بيانات المريض (طباعة وصل — طباعة باركود)
  2. إدخال وتعديل نتائج تحاليل المرضى
  3. مراجعة وتعديل نتائج تحاليل المرضى
  4. طباعة نتائج المرضى (عمل تاريخ مرضى وطباعته — طباعة ظرف … إلخ)
  5. **عدم طباعة نتائج المرضى في حالة وجود باقي حساب** (block printing when a balance remains)
  6. صلاحية تسليم نتائج المرضى
  7. **حد الخصم المسموح للمستخدم إعطاءه للمريض** (per-user discount limit %)
  8. طباعة ورقة العمل و Log
  9. صلاحية حذف المرضى
  10. التعديل في بيانات النظام والإعدادات (بيانات التحاليل والقيم الطبيعية — الباركود — الوحدات … إلخ)
  11. إمكانية صرف وإيداع نقدية ومحاسبة المرضى ومندوبي الجهات الخارجية (حسابات)
  12. إمكانية عمل التقارير الإحصائية وطباعتها (الإحصائيات)
  13. الاطلاع على حساب الدرج والحسابات الأخرى (جرد يومي — أسبوعي — شهري — سنوي … إلخ)
- **FR-M17-005** — On login, permissions apply as entered; attempting a non-authorized action displays the denial message (FR-M01-005).
- **FR-M17-006** — User data and permissions are editable: select user → **تعديل** → **حفظ**; users are deletable (**حذف**).
- **FR-M17-007** — An **admin** user exists by default.
- **FR-M17-008** — Access to the **P/T audit functionality** (§11) shall be restricted to the **System Administrator or users with Absolute Permissions**. This restriction is binding and is not configurable down to limited-permission users.

### M18 — Attendance & Time Tracking

- **FR-M18-001** — The system shall provide **attendance (حضور)**, **break-period**, and **check-out (انصراف)** registration per user, tracking entry/exit, working times, **overtime** and **lateness**.
- **FR-M18-002** — **Only the system manager** may view users' entry/exit times and each user's overtime and lateness.

### M19 — Statistics

- **FR-M19-001** — Statistics on **patient counts** over a specified period, classified by **sex, referral entity and account type**.
- **FR-M19-002** — Statistics on **tests and their counts**, with classification.
- **FR-M19-003** — Statistics on **samples sent outside**, by time periods and destination laboratories.
- **FR-M19-004** — Statistics on **user productivity** according to assigned work.
- **FR-M19-005** — Access: **Statistics** icon → choose statistic type. Examples: patients in a year sorted by month and sex; patients in a month; request rate of a group's tests; number of test samples in a year.

### M20 — Inventory & Lab Accounting

- **FR-M20-001** — The system shall reveal lab profits and perform **daily, weekly, monthly, annual or custom-period** inventory; show other laboratories' accounts and referral-entity shares in specified periods; follow up sent-out samples and their accounts; and perform **cash disbursement/deposit (صرف وإيداع نقدية)** for designated or other entities. Access: **Accounts** icon, providing: **الجرد وحساب الدرج — العينات المرسلة للخارج — صرف وإيداع نقدية — حساب شركات ومندوبين**.
- **FR-M20-002** — **الجرد وحساب الدرج** shall request the **internal windows password** (dialog titled "System menu password" — "Please Enter Your Second Password", OK/Cancel), then show: **total patient samples, discounts value, total after discount, sent-out samples, collected and uncollected amounts (ما تم سداده / باقي لم يسدد), cash supplies (توريدات نقدية), remaining-to-lab (باقي للمعمل), commissions & shares (عمولات ونسب), cash available in the safe (المتاح في الخزينة), and net profit after collection and payment (صافي الربح بعد التحصيل والتسديد)**.
- **FR-M20-003** — For one or several workstations on one shared database: set the period and press **عرض** — the system displays the totals above.
- **FR-M20-004** — Inventory can be run on a **specific user, referral entity, treating doctor, account type, or sent-out samples**: select the element on the right of the window and choose report type — "**مفصل بالنتائج – مفصل بالأسعار – مفصل – مجمع**" (per-element availability of report types may vary).
- **FR-M20-005** — Pressing **إجمالي عينات المرضى** shows amounts in detail.
- **FR-M20-006** — **حساب شركات ومندوبين (companies & delegates accounts)** shall be available from the Accounts window as **company/delegate settlement tracking consistent with the entity-accounting model of this module** (per-period balances, paid/remaining, integrated with cash disbursement/deposit). Detailed screens and behavior shall be specified in the UI-specification phase, consistent with this requirement.

### M21 — Sample Collection & Separation

- **FR-M21-001** — Via **Laboratory → سحب وفصل العينات**, selecting a patient from the list on the right shows the samples to be drawn; clicking a drawn sample registers it as **drawn** and displays it at the bottom of the window, enabling the phlebotomist to distinguish drawn from not-drawn samples per patient.

### M22 — System & Print Settings

- **FR-M22-001** — System settings shall be controllable via **Setting → إعدادات النظام**, with sections: **إعدادات التقرير، إعدادات الإيصال، إعدادات الظرف، إعدادات خادم قواعد البيانات**; plus printer assignment, default account type, general system checkboxes, and daily database backup. The settings dashboard also provides **Database Maintenance** and a **system-initialization** function (تهيئة النظام للنسخة الحديثة).
- **FR-M22-002** — **Report margins:** "Page Margin" (Left / Bottom) and "Report Top Space" editable to fit a pre-printed lab logo; **top margin maximum 8 cm**.
- **FR-M22-003** — **Paper size** for report printing selectable as **A4 or A5**.
- **FR-M22-004** — **Header/footer options:** (a) none (pre-printed paper); (b) as separate words — lab data in dedicated fields with font size/color/type control; (c) from image files chosen from the computer. A **Doctor's signature** element is configurable in report settings. End with **حفظ الإعدادات**.
- **FR-M22-005** — Header/footer **colors** editable via **تعديل ألوان رأس وذيل التقرير**: click element → choose color → موافق.
- **FR-M22-006** — **Printers** shall be assignable per output type: **default/reports printer, barcode printer, envelope printer (الظرف), receipt printer (الإيصال)**.
- **FR-M22-007** — The **default account type** — on which the system automatically deals with the patient regarding prices, discounts, commissions and outside-drawn samples — is selectable; available options: **Individual, Lab To Lab, Contracts, Free**.
- **FR-M22-008** — **General system checkboxes** shall include: entering patient name and treating doctor in English; default placeholder when the referral-entity field is left empty (per patient sex); saving the treating doctor only from the external-entities window (no auto-save during patient entry); **enabling search when entering a patient name** in the add/edit patient window; **disabling automatic title insertion** during registration (FR-M02-007); printing the **file's external barcode**; printing **date and time on tube barcodes**; **printing the lab code (Lab ID) instead of the patient code (Patient ID)** on the result report and barcode stickers (§9); **automatic review-and-completion of tests** (مراجعة واكمال التحاليل بطريقة تلقائية — see §8.4 for its interaction with the lifecycle); and **printing the patient's account and balance instead of the print date** on the result report.
- **FR-M22-009** — **Receipt settings (إعدادات الإيصال):** top margin for pre-printed paper; **currency type** (عملة, e.g., L.E.); **result pickup time** (وقت استلام النتيجة); **print receipt only once**; control over whether **test names/details** are shown on the receipt — options: **hide test names/details**, **show test names/details**, or **show them with the test code**; a **cashier-printer** option for test names and totals (with an Edit action); for blank paper, receipt header/footer as **words** (lab data, font type/size/color, lab logo) or as **images** chosen from the computer (header/footer/logo image slots with defined pixel sizes); end with **حفظ الإعدادات**.
- **FR-M22-010** — **Envelope settings (إعدادات الظرف):** configurable **envelope top margin** (الهامش العلوي للظرف, e.g., 3 cm); options: **no header/footer** (pre-printed envelope), header/footer as **separate words** (blank envelope), or header/footer **from attached image files** with header/footer image slots and a **lab logo** slot; configurable **lab name/title text blocks** with font sizes; **alignment of the data printed on the envelope** (محاذاة البيانات المطبوعة على الظرف) with per-item enable checkboxes and **Left/Top offset (cm)** positioning for: **الاسم** (patient name), **الكود** (code, with barcode preview), **جهة الإحالة** (referring doctor/entity name), **التاريخ** (date); plus an option to **suppress printing of the patient-data captions** on the envelope; end with **حفظ الإعدادات**.
- **FR-M22-011** — The system shall support a **daily database backup** (عمل نسخ احتياطي لقاعدة البيانات يومياً): an enable checkbox with a configurable **backup destination path** and a **path-check** action; surfaced within system settings.
- **FR-M22-012** — **Database Maintenance** shall provide: **backup** of the database, **restore** of a previous backup, and **update** of the database and program files for compatibility with newer versions.
- **FR-M22-013** — **Barcode visibility** shall be controllable from system settings (incl. tube barcode date/time and lab-code-vs-patient-code options, FR-M22-008).
- **FR-M22-014** — **Database server settings** (إعدادات خادم قواعد البيانات) shall allow configuring the **server name, login, and database name** for the shared database connection.
- **FR-M22-015** — The system-wide **patient-history auto-display toggle** (FR-M07-005) and history sorting/aggregation (by lab code / by patient name) are configured in report settings.
- **FR-M22-016** — A **result-screen account display configuration** ("Result screen account", System pane) shall control **whether and how the patient's account information (paid / remaining balance) is displayed on the results screen**, consistent with — and complementary to — the general setting that prints the patient's account and balance instead of the print date on the result report (FR-M22-008). This is a display-configuration capability only; it does not change billing behavior.

### M23 — Utilities (Tools)

- **FR-M23-001** — The main navigation bar shall include a **Tools** item opening a **Utilities** area.
- **FR-M23-002** — The Utilities area shall provide: **Test Library, Image Library, Shortcut Library, Results Measurement Unit Converter, Calculator, Stopwatch, Requirements & Purchases List, Phone Book**, and similar utilities. The Tools Phone Book is a general-purpose utility directory and is **separate from** the patient telephone-number capability of FR-M02-003 / FR-M08-002, which is an integral patient-identification feature.

---

## 6. Cross-Module Business Rules

| ID | Rule |
|---|---|
| **BR-01** | **Patient aggregate status precedence.** A patient's displayed status in any patient list is an aggregate over ALL of the patient's analyses, equal to the **earliest incomplete stage** in the lifecycle precedence order defined in §8.3. A completed/advanced analysis must never hide an analysis that still requires action. |
| **BR-02** | **Identifier distinction.** Patient Name, Lab ID and Patient ID are three distinct identifiers. Lab ID ≠ Patient ID. The Lab ID aggregates visits and drives history; the Patient ID identifies the registration record. A system setting selects which code prints on reports and barcode stickers. |
| **BR-03** | **Multiple phone numbers.** A patient may have two, three or more telephone numbers; registration accepts multiple numbers, and search by **any** of them retrieves the patient record. |
| **BR-04** | **Age-unit-sensitive reference ranges.** Reference-range matching never converts between age units (day ≠ month ≠ year); a normal exists per age unit (FR-M12-005). |
| **BR-05** | **Old reference values persist** for already-registered patients until explicitly refreshed in the report (FR-M04-008 / FR-M12-006). |
| **BR-06** | **Discount limit.** A user may not grant a discount beyond their configured limit (FR-M03-006 / FR-M17-004). |
| **BR-07** | **Print-block on balance.** Where the corresponding permission is set, printing results is blocked while a balance remains (FR-M17-004, FR-M09-003). |
| **BR-08** | **"Delivered" means physical handover.** No status, report, or notification path delivers results online, by SMS, e-mail, or fax (§17). |
| **BR-09** | **Single branch.** No branch selectors, branch filtering, or branch-scoped accounting/inventory/reporting anywhere in the product. Multiple workstations share one database at one site. |
| **BR-10** | **Default account type** drives automatic pricing/discount/commission/outside-sample handling for a patient unless overridden (FR-M22-007). |
| **BR-11** | **Restricted audit access.** P/T audit inspection is available only to the System Administrator or users with Absolute Permissions (§11). |
| **BR-12** | **Pregnancy/children antibiotic display.** Antibiotics flagged Pregnant appear only when pregnancy is indicated; antibiotics flagged Children appear only for patients younger than 12 years (FR-M15-003/004). |
| **BR-13** | **Completion duration drives pickup time.** A test's configured completion duration is the basis for computing the patient's result pickup time (FR-M12-002). |

---

## 7. Users, Roles and Permissions

| Role | Responsibilities / capabilities | Access behavior |
|---|---|---|
| **System manager (مدير النظام / مشرف)** | Manage users and their permissions; manage system data and settings; **use P/T audit & traceability inspection (§11)**; view all users' attendance, overtime and lateness; access inventory/cash drawer | Holds **absolute permission** (صلاحية مطلقة) or the equivalent full item set; exclusive access to attendance overview and P/T; named escalation point in the denial message. An **admin** user exists by default. |
| **User with Absolute Permissions** | As assigned by the system manager | Where absolute permission is granted, P/T audit access applies (§11); otherwise limited-permission users never see P/T content. |
| **Regular user (مستخدم)** | Register patients, enter/review results, print, deliver, operate work sheets — per assigned permissions | **Limited permission** (صلاحية محدودة) with granular items, per-user **discount limit %**, optional **print-block on remaining balance**; **no P/T access**; denial message on unauthorized action (FR-M01-005). |
| **Phlebotomist / sample-drawer (مسئول سحب العينات)** | Mark drawn/separated samples per patient | Uses the sample draw/separation window. |

**Access-control requirements.**

- **FR-SEC-01** — Each user has a **main password** and a **secondary (internal windows) password**.
- **FR-SEC-02** — Sensitive internal windows (user creation, inventory/cash drawer) require the **internal windows password** via the "System menu password" dialog.
- **FR-SEC-03** — The internal windows password ships with a **default**; the responsible user **shall change it during deployment**. The default value must not be kept in production (NFR-03).
- **FR-SEC-04** — Permission changes take effect at the user's next login; the user's **last login time** is recorded and visible in user management.
- **FR-SEC-05** — P/T audit content shall not be visible to, or retrievable by, any user lacking System Administrator status or Absolute Permissions.

---

## 8. Result Lifecycle and Status Model

This section is binding.

### 8.1 Two distinct status concepts

1. **Individual-result lifecycle (per analysis).** Each patient analysis/result progresses operationally through: **result entry → Finish (تمت) → Verify (مراجعة) → Print (طبعت) → Delivered (physical handover)**, with **Export** marking locally exported results and **Status** reflecting the current state. The search filters (not entered / not reviewed / not printed / not delivered) and the delivery window's finished/unfinished distinction reflect this lifecycle. The lifecycle comprises exactly these milestones; no additional per-result states exist.
2. **Patient-level aggregate status.** A patient-level visual indicator communicating the operational state of **all** of the patient's laboratory work, computed per §8.2–§8.3. It is **not** the status of any single analysis and is **never** taken from the most recent, most advanced, first, or first-displayed analysis.

### 8.2 The seven patient-level statuses

| # | Indicator | Meaning |
|---|---|---|
| S1 | **New / red circle** | The patient has newly been registered; no result has yet been entered for the patient's requested analyses. |
| S2 | **Notes-paper icon** | The patient has one or more results that have not yet been entered. |
| S3 | **Green and blue arrows icon** | The patient has results that have been entered but have not yet been reviewed. |
| S4 | **Printer icon** | The patient has results that have been entered and reviewed but have not yet been printed. |
| S5 | **Shopping-cart icon** | The patient has results that have not yet been delivered to the patient. |
| S6 | **Currency / pound icon** | The patient has received all of their results, but there is still an outstanding financial/account balance involving the laboratory or the patient. |
| S7 | **Badge/medal icon** | The patient's results have been entered and printed, the account is fully settled, and the results have been delivered to the patient. |

No additional patient-level statuses may be introduced; none may be removed.

### 8.3 Formal aggregation and precedence rule (binding)

Let a patient have analyses A₁…Aₙ (n ≥ 1). Each analysis Aᵢ maps to a **stage** in the laboratory processing lifecycle according to the furthest lifecycle milestone it has NOT yet passed, i.e., the outstanding action it requires:

| Stage (outstanding action) | Condition on analysis Aᵢ | Patient-level state it contributes |
|---|---|---|
| 1 — result entry pending | no result entered | S2 |
| 2 — review pending | result entered, not reviewed | S3 |
| 3 — print pending | entered and reviewed, not printed | S4 |
| 4 — delivery pending | printed (and account clear, see below), not delivered | S5 |
| 5 — settlement pending | all results delivered, outstanding financial balance | S6 |
| 6 — complete | entered, printed, delivered, account fully settled | S7 |

**Aggregation rule (BR-01 formalized).** The patient-level status equals the state contributed by the analysis (or account condition) at the **minimum stage number** present across all of the patient's analyses and the patient's account — i.e., the **earliest incomplete stage / highest-priority outstanding action**:

> **PatientStatus = state( min{ stage(A₁), …, stage(Aₙ), stage(account) } )**

where "earliest" means earliest in the **processing lifecycle precedence order** (S1 < S2 < S3 < S4 < S5 < S6 < S7 as lifecycle progression), **not** earliest by calendar date or record creation.

**Special cases.**

- **S1 (New / red circle)** applies exactly when the patient is newly registered and no result has been entered for **any** requested analysis; operationally S1 coincides with "all analyses at stage 1 with none started". As soon as the registration is no longer new while results remain unentered, the patient shows **S2**.
- **Account integration (S6).** The currency/pound state represents an **account condition after result delivery**: it is evaluated only when no analysis is at stages 1–4 (all results delivered). If a balance then remains, the patient shows **S6**; when the account is fully settled, the patient reaches **S7**. Consequently a patient with an undelivered analysis AND an outstanding balance shows **S5** (delivery precedes settlement in the lifecycle), and a patient with an unentered culture result and an unpaid balance shows **S2**.
- **Worked example (binding interpretation).** Analysis A: entered, reviewed, not printed (stage 3). Analysis B: printed, not delivered (stage 4). Analysis C (e.g., a culture requiring several days): result not yet entered (stage 1). Patient status = state(min{3, 4, 1}) = **S2 ("result not yet entered")** — the unfinished culture remains visible even though other analyses are printed.

### 8.4 Interaction with lifecycle controls

- The individual-result flags (**تمت / مراجعة / طبعت**) and the **automatic review-and-completion** system setting (FR-M22-008) operate on individual results; when auto-completion is enabled, the corresponding stages are marked complete automatically for the affected results, and the patient-level aggregation of §8.3 is computed from the resulting per-analysis stages without exception.
- **Export** (FR-OUT-09) is orthogonal to the lifecycle and does not advance or alter any stage.
- The account condition (S6) is derived from the patient's payment operations (M03), including settlement at physical delivery (M09).

---

## 9. Patient Identification and Patient Management

### 9.1 Three distinct identifiers (binding)

| Identifier | Nature | Where used | Established relationships |
|---|---|---|---|
| **Patient Name** | Human-readable identity, with optional configurable **title** (اللقب) | Registration, search (exact or partial), reports, history matching "by name" | History matching may use name or Lab ID (FR-M07-004) |
| **Patient ID (كود المريض)** | **System-generated** code of the patient's registration record; shown on the registration screen, printed report (with barcode) and barcode stickers by default | Registration, result report, barcode stickers, search contexts | A system setting prints the **Lab ID instead of the Patient ID** on the result report and barcode stickers (FR-M22-008) |
| **Lab ID (لاب آي دي / كود المعمل)** | **Persistent**, separately created identifier (Lab Id action after saving registration and tests); **appears automatically** on subsequent registrations | Cross-visit search (all visits with dates), medical-history retrieval, optional report/barcode printing per setting | Aggregates all of the patient's visits; drives history retrieval |

**Rules.**

- **Lab ID ≠ Patient ID.** They are separate identifiers with separate purposes and shall never be treated as interchangeable.
- Identifier-generation algorithms are **not prescribed** by this document; they are a design-phase concern. What is required behaviorally is: (a) the Patient ID is generated by the system at registration and displayed; (b) the Lab ID is created via the dedicated Lab Id action, saved as a distinguishing patient data element, and auto-appears on later registrations; (c) both are searchable/printable as documented.
- The code-printing setting (Lab ID vs Patient ID on reports/stickers) defines the relationship between the two codes on printed output.

### 9.2 Patient contact data (binding)

- Registration shall capture **multiple telephone numbers** per patient (two, three, or more).
- Search by **any** stored telephone number shall retrieve the patient's record and associated data (FR-M08-002).
- This capability is part of patient identification and retrieval — it is **not** a standalone directory and is **not** satisfied by the Tools Phone Book utility (FR-M23-002).

### 9.3 Patient management operations

Add (M02), edit via patient list / search / results window (FR-M02-011), delete (permission-gated, FR-M17-004 item 9), notes surfacing in results and delivery windows (FR-M02-004), medical-history capture and display (FR-M02-005, M07), VIP flag, account types, outside-drawn sample flags, and quick print actions (receipt, barcode, work sheet) from the registration screen.

---

## 10. UI / Screen Inventory

Arabic UI labels are authoritative; English glosses are provided for technical clarity.

| UI ID | Screen / window | Visible elements / controls |
|---|---|---|
| UI-01 | **Login window** | User Name; Password; show-password-characters option; remember-login option; Sign in; Exit; database indicator (SQL Server Database) |
| UI-02 | **Main window / navigation bar** | Top bar: **Patients, Laboratory, Work sheet, Tools, Accounts, Statistics, Users, System, Setting, About Us, Exit**. Central patient actions: إضافة وتعديل بيانات المرضى — إدخال نتائج التحاليل — بحث عن مريض — تسليم نتائج المرضى. Status bar: User Name, Last Login (first-login indication), SQL DATABASE / SQL SERVER indicator, Today (date/time) |
| UI-03 | **Patient registration / test ordering** | Selectors TM/TG/CG; dates تاريخ الدخول / تاريخ الاستلام; fields: **كود المريض (Patient ID)**, **VIP**, **Lab. ID**, اسم المريض with configurable title, سن المريض + unit, Male/Female, نوع الحساب (Individual…), **multiple mobile/phone number fields**, رقم البطاقة, عنوان المريض, الطبيب المعالج, جهة الإحالة; medical-history checklist (صيام/ساعات الصيام, سيولة الدم, فقر دم, …); test list with double-click add; Patient tests list + All; sample flags Taken outside lab / Urine / Stool / Blood / Semen / CSF; billing block (إجمالي التحاليل, الخصم, قيمة الخصم, التكلفة بعد الخصم, المدفوع سابقا, الباقي للمعمل, الباقي للمريض, المدفوع); buttons: القائمة الرئيسية, نتائج التحاليل, شاشة المرضى, حفظ, تعديل, حذف, تراجع, اضافة, الايصال, الباركود, ورقة العمل, خالص, موافق |
| UI-04 | **Payment operations window** | List of operations; حذف, تعديل |
| UI-05 | **Results-entry window** | Right panel: today's patients with count, search, Refresh; account filters **Individual / LabToLab / Contracts / VIP / Free / All**; **patient aggregate status icons per §8**. Header: الكود, الاسم, النوع, الجهة, السن, Lab ID. Table columns: **Test Abbreviation, P, T, Result, Status, Finish, Verify, Print, Export** — **P/T = restricted audit buttons, System Administrator / Absolute Permissions only**. Status buttons: **تمت, مراجعة, طبعت** + ملاحظات. Buttons: القائمة الرئيسية, طباعة ظرف, تقرير فارغ, التاريخ المرضي, طباعة, معاينة, تقرير مجمع, بيانات المريض. **Result-screen account display per the dedicated setting (FR-M22-016)**. |
| UI-06 | **Specialized profile report screen** | Patient header replica; per-analyte table **Test Name / Result / Unit / L or H / Normal Ranges / Verified / Print**; test-specific structured inputs (e.g., PT control time + ISI, patient time, concentration, INR, ratio); Comment area; حفظ, طباعة, معاينة الطباعة, Patient History, رجوع, القائمة الرئيسية |
| UI-07 | **Print preview / printed report** | Patient ID + barcode (or Lab ID per setting); name; Age/Sex; Request Date; Printed In; report title; results vs Reference Range; Doctor's signature; optional account/balance instead of print date (FR-M22-008) |
| UI-08 | **Combined report window** | Left list "Patient tests in our lab"; right list "Available tests in report"; green-arrow add; up/down ordering |
| UI-09 | **Blank report window** | Opened via تقرير فارغ; fillable; printable as patient-data-only sheet |
| UI-10 | **Culture entry screen** | Culture button; Sample, Organism A/B/C, Culture Condition, Colony Count |
| UI-11 | **Sensitivity entry screen** | Classifications Highly For / Moderate For / Low For / Resistant For; antibiotic list; Add; display toggles Sensitivity / Reference / Commercial Name; Save |
| UI-12 | **Result delivery window** | Period pickers; list of patients with **undelivered results (النتائج الغير مستلمة)**; finished/unfinished results; table adds **Price** column; **حساب المريض** settlement view (paid / remaining-to-patient / remaining-to-lab) |
| UI-13 | **Patient search window** | Criteria: name (exact/partial), doctor, sex, age, **phone number — any stored number**, national ID, test, date range; filters: نتائج لم تدخل, not reviewed / printed / delivered |
| UI-14 | **Lab ID creation** | Lab Id button on registration screen; generated Lab ID shown and saved; auto-appears on later registrations; search by Lab ID lists all visits with dates |
| UI-15 | **System pane** | Buttons: بيانات التحاليل, مجموعات التحاليل, (Log) مجموعات العمل, Custom Groups, **القاب وتعريفات المرضى**, طباعة قائمة اسعار التحاليل, الجهات الخارجية والمعامل, قوائم اسعار التعاقدات, **Result screen account (result-screen account display configuration, FR-M22-016)** |
| UI-16 | **Test data window** | Full test list; search by name/group/number; تعديل, إضافة تحليل, القيم المرجعية, تراجع, حفظ; fields incl. names, group, barcode, duration, sent-out flag + cost price, patient price, Lab-to-Lab price |
| UI-17 | **Reference values window** | إضافة مدى; sex; age range; min/max; low comment; High comment; تعديل/حذف/حفظ |
| UI-18 | **Price lists window** | Add/rename/delete list; add/edit/remove test prices; طباعة القائمة |
| UI-19 | **Test Comments window** | إضافة → choose test → write comment → حفظ; multiple comments; Comment dropdown in report |
| UI-20 | **Custom Groups window** | إضافة مجموعة; إضافة تحليل with price; حذف |
| UI-21 | **External entities window** | Entity-type selection (treating doctor / sent samples / referral-contract); name, city, address, phone, fax, responsible person & phone; Patient Price; discount/commission %; ID generation action; حفظ/تعديل/حذف; left entity list |
| UI-22 | **Culture Antibiotics window** | Culture selection; attached-antibiotic list + count; إضافة; manual-entry fields; حذف; Pregnant and Children checkboxes |
| UI-23 | **Users & permissions window** | Title "نافذة إنشاء وتعديل صلاحيات المستخدمين"; users list with "Users No." (admin default); fields اسم المستخدم, كلمة المرور, كلمة مرور النوافذ الداخلية, وقت آخر دخول; working-hours start/end (AM/PM); **break-period checkbox + duration**; صلاحية مطلقة / صلاحية محدودة; permission checkboxes (full list in FR-M17-004) incl. حد الخصم المسموح %; buttons القائمة الرئيسية, حذف, حفظ, تعديل, اضافة |
| UI-24 | **Attendance window** | Attendance / break / departure registration; manager-only review view (overtime, lateness) |
| UI-25 | **Accounts window** | Buttons: الجرد وحساب الدرج, العينات المرسلة للخارج, صرف وإيداع نقدية, **حساب شركات ومندوبين (company/delegate settlement tracking)**; password dialog "System menu password / Please Enter Your Second Password / OK / Cancel"; period pickers; عرض; per-element filters (user / sent samples / treating doctor / referral entity / account type) with report types مفصل بالنتائج / بالأسعار / مفصل / مجمع; summary cards (total samples, discounts, after-discount, sent-out, paid, unpaid, cash supplies, remaining-to-lab, commissions & shares, safe cash, net profit); ترسل إلى; خلاص; إجمالي عينات المرضى drill-down |
| UI-26 | **Statistics window** | Statistics icon → statistic-type selection; yearly by month/sex, monthly, group request rate, yearly sample counts |
| UI-27 | **Settings dashboard** | Buttons: **اعدادات النظام**, **Database Maintenance**, **تهيئة النظام للنسخة الحديثة**; status bar as UI-02 |
| UI-28 | **System settings window** | Left sections: **اعدادات التقرير — اعدادات الايصال — اعدادات الظرف — اعدادات خادم قواعد البيانات**. Main panel: 4 printer dropdowns (default/reports, barcode, envelope, receipt); default account type dropdown (Individual / Lab To Lab / Contracts / Free); general checkboxes (FR-M22-008); **daily backup checkbox + Path + Check**; حفظ الإعدادات, القائمة الرئيسية |
| UI-29 | **Report settings** | Page Margin (Left/Bottom); Report Top Space (cm, ≤ 8); paper A4/A5; header/footer modes (none / words / images); font parameters; **Doctor's signature**; colors via تعديل ألوان رأس وذيل التقرير; preview; history sorting/aggregation (by lab code / by patient name) + **history auto-display toggle (FR-M07-005)**; حفظ الاعدادات |
| UI-30 | **Receipt settings** | Top margin; currency (e.g., L.E.); pickup time (e.g., 09:00 PM); print-once; test-detail display options (hide / show / show with test code); cashier-printer option + Edit; header/footer as words (lab data, font, logo) or images (header/footer/logo slots with pixel sizes); حفظ الاعدادات |
| UI-31 | **Envelope settings** | الهامش العلوي للظرف (cm, e.g., 3); header/footer radio options (none / words / from image files) with image slots + lab logo slot; lab name/title text blocks with fonts; **محاذاة البيانات المطبوعة على الظرف** with per-item checkboxes + Left/Top (cm) offsets for **الاسم / الكود (barcode preview) / جهة الإحالة / التاريخ**; option to suppress patient-data captions; حفظ الإعدادات, القائمة الرئيسية |
| UI-32 | **Sample draw/separation window** | Laboratory → سحب وفصل العينات; patient list (right); per-patient sample list; click-to-mark drawn; drawn samples at bottom |
| UI-33 | **Work Sheet window** | Work sheet icon → by patient names / by test names; period; classification; Log-group sheet |
| UI-34 | **Utilities (Tools) area** | Test Library, Image Library, Shortcut Library, Results Measurement Unit Converter, Calculator, Stopwatch, Requirements & Purchases List, Phone Book, and similar |
| UI-35 | **P/T audit inspection views (restricted)** | **P view:** patient-record activity — registering user, modification count, most recent modifying user, payment-receiving users. **T view:** per-test audit — result-entry user, reviewing user, printing user, print count, delivering user; each activity with **date and time**. **Accessible only to System Administrator / Absolute Permissions (§11).** |

---

## 11. Auditability and Traceability

Top-Lab distinguishes three information classes: **ordinary operational information** (visible in day-to-day screens to permitted users), **audit information** (records of who did what and when, kept by the system), and **restricted audit information** (inspectable only by the System Administrator or users with Absolute Permissions).

### 11.1 P/T audit & traceability (restricted — binding)

**P button (patient-record activity).** For a selected patient, the P function shall allow an authorized user to inspect:

- the user who **originally registered** the patient's data;
- the **number of modifications** made to the patient's data;
- the **most recent user** who modified the patient's data;
- the **payment/account-related user activity**, including the **users who received payments**.

**T button (per-test result audit).** For a selected patient test/analysis, the T function shall allow an authorized user to determine:

- **who entered** the result;
- **who reviewed** the result;
- **who was responsible for printing** the result;
- **how many times** the result was printed;
- **which user delivered** the result to the patient.

**Recorded activities include date and time.**

**Access restriction (binding).** P/T functionality is restricted to the **System Administrator or users with Absolute Permissions** (FR-M17-008, FR-SEC-05). It is not available to limited-permission users under any configuration.

**Form of the mechanism.** P/T is an operational audit and traceability mechanism. The specific information items above are required and are inspectable through the dedicated P/T controls on the results screen; the P/T function is a dedicated inspection mechanism and shall not be reduced to a generic audit log.

### 11.2 Other audit and traceability records

- **FR-AUD-01** — Case-tracking data shall be recorded: registering user; number of edits with time and date; result-entry user; print count; last printing user with time and date (surfaced through the restricted P/T views per §11.1).
- **FR-AUD-02** — Attendance/break/check-out times per user shall be recorded; overtime/lateness viewable only by the system manager; last login time per user is recorded (M18).
- **FR-AUD-03** — Payment operations are individually listed and correctable (edit/delete) in the payment-operations window (FR-M03-005), and the receiving user is recorded for P-view inspection (§11.1).

---

## 12. Data and Entity Requirements

Conceptual only — no schema, keys, indexes, ORM entities, migrations, or scripts (those belong to later phases).

| Entity / concept | Conceptual attributes | Relationships |
|---|---|---|
| **Patient** | name (+ configurable title), sex, age **with unit** (day/month/year), **one or more telephone numbers**, address, national ID, treating doctor, referral entity, notes, medical history (medications, conditions, recent imaging), fasting status, pregnancy indication (drives antibiotic display), VIP flag, **Patient ID (كود المريض)**, **Lab ID**, account type (Individual / LabToLab / Contracts / VIP / Free / default placeholder), **aggregate status (derived, §8)** | has many visits; has ordered tests; has account operations; referred by doctor/entity; retrievable by name, Lab ID, Patient ID, or **any phone number** |
| **Patient ID vs Lab ID** | Patient ID = system-generated registration-record code shown on screen, report and barcode. Lab ID = separately created persistent identifier (Lab Id action) aggregating visits and driving history. **Distinct identifiers.** A setting selects which code prints on reports/stickers | both identify the patient in different scopes (§9) |
| **Visit / case** | registration/pickup dates; registering user; edit count/timestamps; result-entry user; print count; last printing user; delivering user | groups tests and results of one encounter; visits listed per patient via Lab ID |
| **Test (analysis)** | name, report name, receipt name, group, barcode, completion duration (drives pickup time), sent-out flag, cost price, patient price, Lab-to-Lab price, test number | belongs to a group; has reference ranges; has fixed comments; member of custom groups, price lists, work groups (Log) |
| **Reference range** | sex, age range **per age unit**, min, max, low comment, high comment; typing By Sex and Age / By Age Only | belongs to a test; matched by sex/age-unit; old values retained for pre-existing patients until refreshed |
| **Result / report** | result value, unit, L/H flag, lifecycle flags (Finish/Verify/Print), Status, Export flag, notes, comments, print count, last printing user; profile results per analyte; culture results: sample, organisms A/B/C, condition, colony count, sensitivity classifications | belongs to a visit; aggregates into combined/blank/history reports; feeds patient aggregate status (§8) |
| **Custom group / test group / work group (Log)** | group name; member tests with per-group price (custom groups) | added to patients in one action; Log groups drive work sheets |
| **Price list** | list name; test→price entries | assigned to referral/contract entities and labs |
| **External entity** | type (doctor / sent-out / referral-contract), name, city, address, phone, fax, responsible person & phone, discount/commission %, generated ID, price list (except doctors) | refers patients; settles sent-out accounts; appears in statistics/inventory; companies & delegates settle via M20 |
| **Culture** | culture test under group "CULTURE AND SENSITIVITY"; **user-extensible** set of culture types (in-app, FR-M15-001) | has many antibiotics; resulted via culture/sensitivity screens |
| **Antibiotic** | name/data (manual entry allowed); classifications Highly/Moderate/Low/Resistant; **Pregnant** flag; **Children** flag (< 12 yrs) | attached to cultures; conditionally displayed |
| **User** | username, main & secondary passwords, permission set (absolute/limited + items + discount limit), working hours, break period, last login; attendance events (in/break/out, overtime, lateness) | performs audited actions; P/T inspection restricted to admin/absolute users |
| **Payment operation** | amount, paid/discount, "+" extra charge, editable/deletable; خلاص full-settlement marker; **receiving user** | belongs to patient account; feeds inventory, delivery settlement, P-view, and the S6/S7 account condition |
| **Sent-out sample** | external lab, cost price, patient price, paid/remaining per lab per period | settled via Accounts |
| **Sample (drawn)** | drawn vs not-drawn marking; outside-lab drawn flag per test; sample types Urine/Stool/Blood/Semen/CSF | belongs to patient visit |
| **Printed artifacts** | receipt, envelope (name/code/referral/date per settings), barcode, work sheet, price list | produced per patient/visit; per-artifact printer assignment |
| **Cash drawer / inventory** | totals, discounts, collected/uncollected, cash supplies, remaining-to-lab, commissions & shares, safe cash, net profit | aggregated per period and per element (user/doctor/entity/account type/sent-out/company/delegate) |

---

## 13. Non-Functional Requirements

| Category | NFR |
|---|---|
| Security / access control | **NFR-01:** A user-permissions system shall prevent tampering and preserve privacy, with absolute/limited modes and granular items incl. discount limits and print-blocking on unpaid balances (FR-M17-004/005). |
| Security | **NFR-02:** Sensitive internal windows (user creation, inventory/cash drawer) shall require the internal windows password ("System menu password" dialog). |
| Security | **NFR-03:** The internal windows password ships with a documented default; it **shall be changed at deployment**; the factory default must not be kept in production. |
| Security / audit | **NFR-04:** P/T audit inspection shall be restricted to the System Administrator or users with Absolute Permissions (§11). |
| Auditability | **NFR-05:** Per-case audit data shall be recorded: registering user, edit counts with date/time, result-entry user, reviewing user, print counts, last printing user, delivering user with date/time (§11). |
| Auditability | **NFR-06:** Attendance/break/check-out times per user shall be recorded; overtime/lateness viewable only by the system manager; last login time per user is recorded. |
| Recoverability | **NFR-07:** Daily database backup (configurable destination, path check) plus Database Maintenance (backup / restore / update). |
| Platform / compatibility | **NFR-08:** Desktop **Windows** application built on **.NET 8** with a **WPF** presentation layer following the **MVVM** pattern, structured per **Clean Architecture**, with **Entity Framework Core** data access over a shared **SQL Server** database; multi-workstation operation on one database at one site; **single branch**; **no internet dependency** (§1.5). |
| Printing / output | **NFR-09:** Printed outputs: patient reports, receipts, envelopes, barcodes, work sheets, price lists; per-output printer assignment; paper A4/A5; top margin ≤ 8 cm; header/footer text or images; color control. |
| Interoperability | **NFR-10:** LAN-only interoperability (shared database across workstations); database server connection configurable (server name, login, database name). No external interfaces. |
| Data integrity | **NFR-11:** All status, account and audit figures shown to users shall be derived from the same shared database so that all workstations present consistent data; patient aggregate status is computed per §8.3 from current data, not cached from any single analysis. |
| Usability | **NFR-12:** Arabic-first UI; patient search provided in easy and simple ways (product goal, not a testable metric). |

---

## 14. Reports, Printing and Output

**All outputs are offline; online delivery and SMS/e-mail/fax notification are excluded (§17).**

- **FR-OUT-01** — **Patient result report:** Patient ID + barcode (or Lab ID per the code-printing setting), name, age/sex, request date, print timestamp, results vs reference ranges, doctor's signature; layout per report settings (margins ≤ 8 cm top, A4/A5, header/footer none/words/images, colors).
- **FR-OUT-02** — Report options: printing the **lab code (Lab ID) instead of the patient code** on the report and barcode stickers; printing the **patient's account and balance instead of the print date**; history auto-insertion per FR-M07-003/005.
- **FR-OUT-03** — **Combined report** and **blank report** printing (FR-M07-001/002); **separate history report** (FR-M07-007); **multi-patient history report** (FR-M07-008).
- **FR-OUT-04** — **Receipt (إيصال):** printed from registration; configurable top margin, currency, pickup time, print-once, test-detail display (hide / show / show with test code), cashier-printer option, header/footer as words or images.
- **FR-OUT-05** — **Envelope (ظرف):** printed from the results screen (طباعة ظرف) on the assigned envelope printer; layout per envelope settings (top margin, header/footer modes, per-item data selection and cm-positioning for name/code/referral/date).
- **FR-OUT-06** — **Sample barcodes:** printed from registration (الباركود) on the assigned barcode printer; options for external file barcode and date/time on tube barcodes.
- **FR-OUT-07** — **Work sheets** (patient-based, test-based, Log-group) and **test-frequency classification** printouts.
- **FR-OUT-08** — **Price list printing** (طباعة القائمة).
- **FR-OUT-09** — **Export** is defined as **local file export** of the printed result report (e.g., PDF / Word) to the workstation or local/LAN storage. The Export column marks results so exported. **No online upload exists** (§17). Export does not advance the result lifecycle (§8.4).
- **FR-OUT-10** — **Per-output printer assignment** (reports/default, barcode, envelope, receipt).
- **FR-OUT-11** — No output channel may transmit results over the internet, SMS, e-mail or fax (§17).

---

## 15. Backup and Data Recovery Requirements

- **FR-BKP-01** — A **daily database backup** capability shall be provided within system settings: enable checkbox, configurable **backup destination path**, and a **path-check** action verifying the configured destination.
- **FR-BKP-02** — **Database Maintenance** shall provide manual **backup**, **restore** of a previous backup, and **update** of the database and program files for compatibility with newer versions.
- **FR-BKP-03** — A **system-initialization** function (تهيئة النظام للنسخة الحديثة) shall prepare the system when moving to a newer version.
- **FR-BKP-04** — Backup/maintenance functions operate on the single shared SQL Server database and are performed from an authorized workstation; database-server connection parameters (server name, login, database name) are configurable (FR-M22-014). Installation and SQL Server setup procedures are outside the scope of this document (§17).

---

## 16. Configuration and System Settings

All configuration is consolidated in M22 (FR-M22-001 … FR-M22-016) and surfaces in UI-27 … UI-31:

- **Report settings** (UI-29): margins (top ≤ 8 cm), paper A4/A5, header/footer modes (none/words/images), font parameters, colors, Doctor's signature, preview, **history sorting/aggregation (by lab code / by patient name)** and **history auto-display toggle (FR-M07-005)**.
- **Receipt settings** (UI-30): top margin, currency, pickup time, print-once, test-detail display (hide/show/show-with-code), cashier-printer option, header/footer words or images.
- **Envelope settings** (UI-31): top margin, header/footer modes, lab name/title blocks, per-item enable + Left/Top cm positioning (name / code+barcode / referral / date), caption suppression.
- **Database server settings:** server name, login, database name (FR-M22-014).
- **General checkboxes (FR-M22-008):** English entry of patient/doctor names; empty-referral placeholder per sex; doctor saved only from external-entities window; patient-name search assistance; **disable automatic title insertion**; external file barcode; date/time on tube barcodes; **Lab-ID-instead-of-Patient-ID printing**; **automatic review-and-completion**; **account/balance instead of print date on reports**.
- **Result-screen account display (FR-M22-016):** controls whether/how patient account information appears on the results screen.
- **Printers:** per-output assignment (reports/default, barcode, envelope, receipt).
- **Default account type:** Individual / Lab To Lab / Contracts / Free.
- **Backup:** daily backup enable + destination path + check; Database Maintenance; system initialization (§15).
- **System-pane data configuration:** test data, test groups, work groups (Log), custom groups, **patient titles & definitions**, price-list printing, external entities & labs, contract price lists, result-screen account.

---

## 17. Explicit Exclusions

The following are intentionally **excluded** from Top-Lab and must not appear as requirements, features, modules, future items, dashboards, registries, or audit capabilities:

**17.1 Online result delivery (all forms).** Patient web login; doctor web login; laboratory web login; website result upload; online result viewing; online result accounts and credentials; result block/unblock administration and upload counters; printing web credentials or online result passwords on receipts or elsewhere; printing online-service pickup data on the receipt; mobile-application result access; online-service administration. **Top-Lab is a desktop-only application.**

**17.2 SMS / E-mail / Fax result notification.** Any SMS, e-mail, or fax result-notification feature, and any notification feature dependent on an online service.

**17.3 Multi-branch support.** Branch selectors; branch filtering in search, inventory, accounting or reporting; multi-branch inventory; branch-specific accounting or reporting. *(Multiple workstations connected to ONE shared SQL Server database at ONE physical site ARE supported.)*

**17.4 Laboratory equipment/device tracking.** No equipment module, feature, future requirement, dashboard, inventory-like equipment registry, maintenance/calibration tracking, or equipment audit capability.

**17.5 Patient card / ID card printing.** No patient card or patient ID card printing feature in any form, and no card-printing settings or printer assignment.

**17.6 Installation / SQL Server setup content.** Database deployment and installation procedures are not product requirements in this document.

---

*End of Top-Lab PRD.*
