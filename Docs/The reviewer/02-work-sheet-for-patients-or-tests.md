# 2 — Work Sheet for Patients or Tests

Functional documentation of the work sheet module: the two kinds of work sheet the module produces and the two windows that produce them, the filters that narrow each work sheet, the classification of the tests performed within a chosen period, and the printed work sheet documents themselves.

**Identifier scheme.** The identifier is the address of the element in the interface, and the address is as deep as the containment really is. A module is `g`; a functional group is `g.s`; a screen, panel or container inside a group is `g.s.c`; a block or region inside a screen is `g.s.c.b`; an individual field, button, list, column, option, rule or output is `g.s.c.b.e`; and where an element has options of its own, each option is `g.s.c.b.e.o`. The scheme therefore reaches four, five or six levels according to the real depth of the interface, and never flattens a container that in fact contains something. Every field, button, list, column, option, action, rule and output carries its own identifier, and no identifier ever stands for more than one element. The same element is never documented twice: a field is described once, at the screen that carries it, and every other place refers to it by its identifier. Identifiers are permanent, are never reused for a different element, and are never renumbered.

**Source label.** The third column of the definition table carries the label as it is written on the screen in the source, in its original script, so that the element can be matched against the running software. A dash means the source carries no readable label for that element.

**Cross-module references.** A numeric identifier always points inside this module. A reference to a function of another module is made by name and module number — the patient module and the test-data module — and is converted into a numeric identifier when the modules are assembled into the single combined file.

**Evidence tags.** `[VERIFIED]` where the element is directly readable in the source; `[UNVERIFIED]` where the element's existence or meaning could not be established confidently; `[NOT-SHOWN]` where the element is named but its screen is never displayed, so that it is not counted as a missing feature; and `[SAMPLE]` where a value is an example rather than a requirement.

## §2.1 Functional Hierarchy

```
2  Work Sheet for Patients or Tests
├── 2.1  Work Sheet Module Entry
│   ├── 2.1.1  Work Sheet Ribbon Item
│   ├── 2.1.2  Work Sheet Menu
│   │   ├── 2.1.2.1  Work Sheet By Patient Names Item
│   │   └── 2.1.2.2  Work Sheet By Test Names Item
│   └── 2.1.3  Work Sheet Capabilities
│       ├── 2.1.3.1  Patient Period Work Sheet
│       ├── 2.1.3.2  Test Period Work Sheet
│       ├── 2.1.3.3  Work Group Work Sheet
│       ├── 2.1.3.4  Test Period Classification
│       └── 2.1.3.5  Unfinished Only Selection
├── 2.2  Work Sheet by Patient Names
│   └── 2.2.1  Work Sheet by Patient Names Screen
│       ├── 2.2.1.1  Patient Work Sheet Mode Options
│       │   ├── 2.2.1.1.1  Full Patient Names Sheet Option
│       │   └── 2.2.1.1.2  Abbreviated Patient Names Sheet Option
│       ├── 2.2.1.2  Patient Sheet Filter Block
│       │   ├── 2.2.1.2.1  Patient Sheet Referring Entity Selector
│       │   ├── 2.2.1.2.2  Patient Sheet From Code Field
│       │   ├── 2.2.1.2.3  Patient Sheet To Code Field
│       │   ├── 2.2.1.2.4  Patient Sheet From Date Field
│       │   └── 2.2.1.2.5  Patient Sheet To Date Field
│       ├── 2.2.1.3  Patient Sheet List Block
│       │   ├── 2.2.1.3.1  Patient Sheet Patient Grid
│       │   ├── 2.2.1.3.2  Patient Sheet Patient Count
│       │   └── 2.2.1.3.3  Patient Name Double-Click Removal Action
│       ├── 2.2.1.4  Patient Sheet Print Option Block
│       │   ├── 2.2.1.4.1  Patient Sheet Unfinished Tests Only Option
│       │   └── 2.2.1.4.2  Print Selected Patient Sheet Only Option
│       └── 2.2.1.5  Patient Sheet Action Block
│           ├── 2.2.1.5.1  Patient Sheet Print Action
│           ├── 2.2.1.5.2  Patient Sheet Refresh Action
│           ├── 2.2.1.5.3  Patient Sheet Preview Action
│           └── 2.2.1.5.4  Patient Sheet Main List Action
├── 2.3  Work Sheet by Test Names
│   └── 2.3.1  Work Sheet by Test Names Screen
│       ├── 2.3.1.1  Test Work Sheet Mode Options
│       │   ├── 2.3.1.1.1  Selected Tests Sheet Option
│       │   ├── 2.3.1.1.2  Period Test Names Sheet Option
│       │   ├── 2.3.1.1.3  Specific Test In Period Option
│       │   └── 2.3.1.1.4  Group Name Sheet Option
│       ├── 2.3.1.2  Test Sheet Filter Block
│       │   ├── 2.3.1.2.1  Test Sheet Group Selector
│       │   ├── 2.3.1.2.2  Test Sheet Referring Entity Selector
│       │   ├── 2.3.1.2.3  Test Sheet From Code Field
│       │   ├── 2.3.1.2.4  Test Sheet To Code Field
│       │   ├── 2.3.1.2.5  Test Sheet From Date Field
│       │   └── 2.3.1.2.6  Test Sheet To Date Field
│       ├── 2.3.1.3  Test Sheet List Block
│       │   ├── 2.3.1.3.1  Test Sheet Test Grid
│       │   ├── 2.3.1.3.2  Test Sheet Test Count
│       │   └── 2.3.1.3.3  Test Name Double-Click Removal Action
│       ├── 2.3.1.4  Test Sheet Print Option Block
│       │   └── 2.3.1.4.1  Test Sheet Unfinished Tests Only Option
│       └── 2.3.1.5  Test Sheet Action Block
│           ├── 2.3.1.5.1  Test Sheet Print Action
│           ├── 2.3.1.5.2  Test Sheet Refresh Action
│           ├── 2.3.1.5.3  Test Sheet Preview Action
│           ├── 2.3.1.5.4  Period Test Classification Action
│           └── 2.3.1.5.5  Test Sheet Main List Action
├── 2.4  Period Test Classification
│   └── 2.4.1  Period Test Classification Screen
│       ├── 2.4.1.1  Classification Grid
│       │   ├── 2.4.1.1.1  Classification Test Name Column
│       │   └── 2.4.1.1.2  Classification Test Count Column
│       └── 2.4.1.2  Classification Report Output
└── 2.5  Work Sheet Documents
    ├── 2.5.1  Common Document Elements
    │   ├── 2.5.1.1  Laboratory Header Block
    │   │   └── 2.5.1.1.1  Laboratory Printed Header
    │   ├── 2.5.1.2  Printed By Entry
    │   ├── 2.5.1.3  Print Timestamp
    │   ├── 2.5.1.4  Zoom Setting
    │   └── 2.5.1.5  Page Count Entry
    ├── 2.5.2  Work Paper by Patient ID
    │   ├── 2.5.2.1  Work Paper Patient Title
    │   ├── 2.5.2.2  Patient Sheet Count Entry
    │   ├── 2.5.2.3  Work Paper Patient ID Column
    │   ├── 2.5.2.4  Work Paper Patient Name Column
    │   ├── 2.5.2.5  Work Paper Gender Column
    │   ├── 2.5.2.6  Work Paper Age Column
    │   ├── 2.5.2.7  Work Paper Date Column
    │   ├── 2.5.2.8  Work Paper Referred By Column
    │   ├── 2.5.2.9  Reception Officer Signature Area
    │   ├── 2.5.2.10  Samples Officer Signature Area
    │   └── 2.5.2.11  Work Paper Patient Entry
    ├── 2.5.3  Work Paper by Test Name
    │   ├── 2.5.3.1  Work Paper Test Title
    │   ├── 2.5.3.2  Test Type Count Entry
    │   ├── 2.5.3.3  Work Paper Test Code Column
    │   ├── 2.5.3.4  Work Paper Test Name Column
    │   ├── 2.5.3.5  Test Work Paper Patient Name Column
    │   ├── 2.5.3.6  Work Paper Result Column
    │   ├── 2.5.3.7  Test Work Paper Patient ID Column
    │   └── 2.5.3.8  Patient Count For Test Entry
    └── 2.5.4  Work Paper for Selected Test
        ├── 2.5.4.1  Selected Test Work Paper Title
        ├── 2.5.4.2  Selected Test Code Entry
        ├── 2.5.4.3  Selected Test Name Entry
        ├── 2.5.4.4  Selected Test Patient ID Column
        ├── 2.5.4.5  Selected Test Patient Name Column
        ├── 2.5.4.6  Selected Test Patient Result Column
        ├── 2.5.4.7  Selected Test Signature Column
        ├── 2.5.4.8  Selected Test Patient Count Entry
        ├── 2.5.4.9  Selected Test Reception Officer Signature Area
        └── 2.5.4.10  Selected Test Samples Officer Signature Area
```

## §2.2 Element Definitions

### 2.1 — Work Sheet Module Entry

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 2.1.1 | Work Sheet Ribbon Item | Work sheet | Navigation | The ribbon item that opens the work sheet module. | [VERIFIED] |
| 2.1.2 | Work Sheet Menu | — | Menu | The choice presented when the module is opened: a work sheet by patient names, or a work sheet by test names with the group log. | [VERIFIED] |
| 2.1.2.1 | Work Sheet By Patient Names Item | ورقة عمل بأسماء المرضى | Menu item | Opens the work sheet that is built from a set of patients. | [VERIFIED] |
| 2.1.2.2 | Work Sheet By Test Names Item | ورقة عمل بأسماء التحاليل (Log) | Menu item | Opens the work sheet that is built from a set of tests. | [VERIFIED] |
| 2.1.3 | Work Sheet Capabilities | — | Block | The kinds of work sheet the module produces. | [VERIFIED] |
| 2.1.3.1 | Patient Period Work Sheet | — | Function | A work sheet for a group of patients within a chosen period, carrying them by their names or their codes. | [VERIFIED] |
| 2.1.3.2 | Test Period Work Sheet | — | Function | A work sheet for a single test within a chosen period, or for a group of tests that have been selected. | [VERIFIED] |
| 2.1.3.3 | Work Group Work Sheet | — | Function | A work sheet for a work group within the chosen period. | [VERIFIED] |
| 2.1.3.4 | Test Period Classification | — | Function | A classification of the tests of a chosen period, showing how many times each test was performed in that period. | [VERIFIED] |
| 2.1.3.5 | Unfinished Only Selection | — | Function | The work sheet may be produced for the unfinished tests alone, or for all of the tests. | [VERIFIED] |

### 2.2 — Work Sheet by Patient Names

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 2.2.1 | Work Sheet by Patient Names Screen | ورقة عمل بأسماء المرضى | Screen | Through this window a work sheet is made for the current day, for a chosen period, or for a chosen group of patients taken by name or by code. | [VERIFIED] |
| 2.2.1.1 | Patient Work Sheet Mode Options | — | Field group | The two forms the patient work sheet may take. | [VERIFIED] |
| 2.2.1.1.1 | Full Patient Names Sheet Option | ورقة عمل حسب أسماء مرضى الفترة المحددة (كاملة) | Option | The work sheet carries the full record of each patient of the chosen period. | [VERIFIED] |
| 2.2.1.1.2 | Abbreviated Patient Names Sheet Option | ورقة عمل حسب أسماء مرضى الفترة المحددة (مختصرة) | Option | The work sheet carries a shortened record of each patient of the chosen period. | [VERIFIED] |
| 2.2.1.2 | Patient Sheet Filter Block | — | Block | The filters that decide which patients the work sheet covers. | [VERIFIED] |
| 2.2.1.2.1 | Patient Sheet Referring Entity Selector | جهة الإحالة | Field | Selects the referring entity whose patients the work sheet covers. | [VERIFIED] |
| 2.2.1.2.2 | Patient Sheet From Code Field | من كود رقم | Field | The code the patient range starts at. | [VERIFIED] |
| 2.2.1.2.3 | Patient Sheet To Code Field | الى كود رقم | Field | The code the patient range ends at. | [VERIFIED] |
| 2.2.1.2.4 | Patient Sheet From Date Field | من تاريخ | Field | The date the period starts at. | [VERIFIED] |
| 2.2.1.2.5 | Patient Sheet To Date Field | الى | Field | The date the period ends at. | [VERIFIED] |
| 2.2.1.3 | Patient Sheet List Block | — | Block | The list of patients the work sheet will cover. | [VERIFIED] |
| 2.2.1.3.1 | Patient Sheet Patient Grid | All \| Patient ID \| Patient Name | Grid | The list of patients, with the columns all, the patient identifier and the patient name, and with the count of the patients it holds. | [VERIFIED] |
| 2.2.1.3.2 | Patient Sheet Patient Count | — | Indicator | The number of patients in the list. | [VERIFIED] |
| 2.2.1.3.3 | Patient Name Double-Click Removal Action | — | Action | A patient is removed from the list by pressing their name twice. | [VERIFIED] |
| 2.2.1.4 | Patient Sheet Print Option Block | — | Block | The options that decide what the work sheet contains. | [VERIFIED] |
| 2.2.1.4.1 | Patient Sheet Unfinished Tests Only Option | طباعة التحاليل الغير منتهية فقط (عند اختيار مجموعة مرضى) | Option | The work sheet carries the unfinished tests alone; the option applies when a group of patients is selected. | [VERIFIED] |
| 2.2.1.4.2 | Print Selected Patient Sheet Only Option | طباعة ورقة العمل الخاصة بالمريض المحدد فقط | Option | The work sheet covers the selected patient alone. | [VERIFIED] |
| 2.2.1.5 | Patient Sheet Action Block | — | Block | The actions of the patient work sheet window. | [VERIFIED] |
| 2.2.1.5.1 | Patient Sheet Print Action | Print | Action | Prints the patient work sheet. | [VERIFIED] |
| 2.2.1.5.2 | Patient Sheet Refresh Action | Refresh | Action | Reloads the patient list. | [VERIFIED] |
| 2.2.1.5.3 | Patient Sheet Preview Action | معاينة ورقة العمل قبل الطباعة | Action | Shows the work sheet before printing it. | [VERIFIED] |
| 2.2.1.5.4 | Patient Sheet Main List Action | القائمة الرئيسية | Action | Returns to the main list. | [VERIFIED] |

### 2.3 — Work Sheet by Test Names

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 2.3.1 | Work Sheet by Test Names Screen | ورقة عمل بأسماء التحاليل | Screen | Through this window a work sheet is made for a single test or a group of tests within a chosen period, or a group of tests for a certain number of patients. | [VERIFIED] |
| 2.3.1.1 | Test Work Sheet Mode Options | — | Field group | The forms the test work sheet may take. | [VERIFIED] |
| 2.3.1.1.1 | Selected Tests Sheet Option | ورقة عمل بأسماء التحاليل | Option | The work sheet is built from the tests selected in the test list. | [VERIFIED] |
| 2.3.1.1.2 | Period Test Names Sheet Option | ورقة عمل حسب أسماء تحاليل الفترة الزمنية المحددة | Option | The work sheet is built from the test names of the chosen period. | [VERIFIED] |
| 2.3.1.1.3 | Specific Test In Period Option | ورقة عمل حسب التحليل المحدد فى الفترة الزمنية المحددة | Option | The work sheet is built from one chosen test within the chosen period. | [VERIFIED] |
| 2.3.1.1.4 | Group Name Sheet Option | ورقة عمل بإسم مجموعة (Log) | Option | The work sheet is built from a work group, and the group's tests are not shown in the list. | [VERIFIED] |
| 2.3.1.2 | Test Sheet Filter Block | — | Block | The filters that decide which tests the work sheet covers. | [VERIFIED] |
| 2.3.1.2.1 | Test Sheet Group Selector | المجموعة | Field | Selects the work group the work sheet covers. | [VERIFIED] |
| 2.3.1.2.2 | Test Sheet Referring Entity Selector | جهة الإحالة | Field | Selects the referring entity whose tests the work sheet covers. | [VERIFIED] |
| 2.3.1.2.3 | Test Sheet From Code Field | من كود رقم | Field | The code the patient range starts at. | [VERIFIED] |
| 2.3.1.2.4 | Test Sheet To Code Field | الى كود رقم | Field | The code the patient range ends at. | [VERIFIED] |
| 2.3.1.2.5 | Test Sheet From Date Field | من تاريخ | Field | The date the period starts at. | [VERIFIED] |
| 2.3.1.2.6 | Test Sheet To Date Field | الى | Field | The date the period ends at. | [VERIFIED] |
| 2.3.1.3 | Test Sheet List Block | — | Block | The list of tests the work sheet will cover. | [VERIFIED] |
| 2.3.1.3.1 | Test Sheet Test Grid | All \| Test ID \| Test Name | Grid | The list of tests, with the columns all, the test identifier and the test name, and with the count of the tests it holds. | [VERIFIED] |
| 2.3.1.3.2 | Test Sheet Test Count | — | Indicator | The number of tests in the list. | [VERIFIED] |
| 2.3.1.3.3 | Test Name Double-Click Removal Action | — | Action | A test is removed from the list by pressing its name twice. | [VERIFIED] |
| 2.3.1.4 | Test Sheet Print Option Block | — | Block | The options that decide what the work sheet contains. | [VERIFIED] |
| 2.3.1.4.1 | Test Sheet Unfinished Tests Only Option | طباعة التحاليل الغير منتهية فقط (عند عمل لوج او لجهة فقط) | Option | The work sheet carries the unfinished tests alone; the option applies when a work group is made, or for a referring entity alone. | [VERIFIED] |
| 2.3.1.5 | Test Sheet Action Block | — | Block | The actions of the test work sheet window. | [VERIFIED] |
| 2.3.1.5.1 | Test Sheet Print Action | Print | Action | Prints the test work sheet. | [VERIFIED] |
| 2.3.1.5.2 | Test Sheet Refresh Action | Refresh | Action | Reloads the test list. | [VERIFIED] |
| 2.3.1.5.3 | Test Sheet Preview Action | معاينة ورقة العمل قبل الطباعة | Action | Shows the work sheet before printing it. | [VERIFIED] |
| 2.3.1.5.4 | Period Test Classification Action | تصنيف تحاليل الفترة المحددة | Action | Opens the classification of the tests of the chosen period. | [VERIFIED] |
| 2.3.1.5.5 | Test Sheet Main List Action | القائمة الرئيسية | Action | Returns to the main list. | [VERIFIED] |

### 2.4 — Period Test Classification

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 2.4.1 | Period Test Classification Screen | تصنيف تحاليل الفترة المحددة | Screen | The classification of the tests of a chosen period, showing how many times each test was performed in that period. | [VERIFIED] |
| 2.4.1.1 | Classification Grid | Test Name \| No. of Tests | Grid | The grid of the classification: one row per test, carrying the test name and the number of times the test was performed in the period. | [VERIFIED] |
| 2.4.1.1.1 | Classification Test Name Column | Test Name | Grid column | The name of the test. | [VERIFIED] |
| 2.4.1.1.2 | Classification Test Count Column | No. of Tests | Grid column | The number of times the test was performed in the period. | [VERIFIED] |
| 2.4.1.2 | Classification Report Output | statistics | Output | The printed classification report, carrying the laboratory header, the date and time of printing, the user who printed it and the classification grid. | [VERIFIED] |

### 2.5 — Work Sheet Documents

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 2.5.1 | Common Document Elements | — | Block | The elements every work sheet document carries. | [VERIFIED] |
| 2.5.1.1 | Laboratory Header Block | — | Block | The block of the printed document that carries the laboratory's own printed header. | [VERIFIED] |
| 2.5.1.1.1 | Laboratory Printed Header | — | Output | The header the laboratory prints on its documents. Its content is the laboratory's own identity and is not described here. | [VERIFIED] |
| 2.5.1.2 | Printed By Entry | Printed By | Field | The user who printed the document. | [VERIFIED] |
| 2.5.1.3 | Print Timestamp | — | Field | The date and time the document was printed. | [VERIFIED] |
| 2.5.1.4 | Zoom Setting | Zoom 100% | Field | The magnification the document is shown at in the preview. | [VERIFIED] |
| 2.5.1.5 | Page Count Entry | Pages: 1 | Field | The number of pages the document has. | [VERIFIED] |
| 2.5.2 | Work Paper by Patient ID | Work Paper | Output | The work sheet produced for a group of patients, carrying them in rows. | [VERIFIED] |
| 2.5.2.1 | Work Paper Patient Title | Work paper by Patient ID | Field | The title of the work sheet, shown above the patient rows. | [VERIFIED] |
| 2.5.2.2 | Patient Sheet Count Entry | No. of Patients | Field | The number of patients the work sheet carries. | [VERIFIED] |
| 2.5.2.3 | Work Paper Patient ID Column | Patient ID | Grid column | The patient's identifier. | [VERIFIED] |
| 2.5.2.4 | Work Paper Patient Name Column | Patient Name | Grid column | The patient's name. | [VERIFIED] |
| 2.5.2.5 | Work Paper Gender Column | Gender | Grid column | The patient's sex. | [VERIFIED] |
| 2.5.2.6 | Work Paper Age Column | Age | Grid column | The patient's age. | [VERIFIED] |
| 2.5.2.7 | Work Paper Date Column | Date | Grid column | The date of the patient's visit. | [VERIFIED] |
| 2.5.2.8 | Work Paper Referred By Column | Referred by | Grid column | The entity that referred the patient. | [VERIFIED] |
| 2.5.2.9 | Reception Officer Signature Area | موظف الاستقبال | Field | The area of the document where the reception officer signs. | [VERIFIED] |
| 2.5.2.10 | Samples Officer Signature Area | مسئول العينات | Field | The area of the document where the samples officer signs. | [VERIFIED] |
| 2.5.2.11 | Work Paper Patient Entry | Patient | Field | An entry on the document header whose meaning is not established. | [UNVERIFIED] |
| 2.5.3 | Work Paper by Test Name | Work paper by test name | Output | The work sheet produced for a set of tests, carrying, under each test, the patients who had it and their results. | [VERIFIED] |
| 2.5.3.1 | Work Paper Test Title | Work paper by test name | Field | The title of the work sheet, shown above the test sections. | [VERIFIED] |
| 2.5.3.2 | Test Type Count Entry | No. of type of tests | Field | The number of test types the work sheet carries. | [VERIFIED] |
| 2.5.3.3 | Work Paper Test Code Column | Test Code | Grid column | The identifier of the test. | [VERIFIED] |
| 2.5.3.4 | Work Paper Test Name Column | Test Name | Grid column | The name of the test. | [VERIFIED] |
| 2.5.3.5 | Test Work Paper Patient Name Column | Patient Name | Grid column | The name of a patient who had the test. | [VERIFIED] |
| 2.5.3.6 | Work Paper Result Column | Result | Grid column | The result the patient obtained on the test. | [VERIFIED] |
| 2.5.3.7 | Test Work Paper Patient ID Column | ID | Grid column | The identifier of a patient who had the test. | [VERIFIED] |
| 2.5.3.8 | Patient Count For Test Entry | No. of Patient for this test | Field | The number of patients who had the test. | [VERIFIED] |
| 2.5.4 | Work Paper for Selected Test | Work paper for selected test | Output | The work sheet produced for one chosen test, carrying the patients who had it. | [VERIFIED] |
| 2.5.4.1 | Selected Test Work Paper Title | Work paper for selected test | Field | The title of the work sheet, shown above the patient rows. | [VERIFIED] |
| 2.5.4.2 | Selected Test Code Entry | Test Code | Field | The identifier of the chosen test. | [VERIFIED] |
| 2.5.4.3 | Selected Test Name Entry | Test Name | Field | The name of the chosen test. | [VERIFIED] |
| 2.5.4.4 | Selected Test Patient ID Column | ID | Grid column | The patient's identifier. | [VERIFIED] |
| 2.5.4.5 | Selected Test Patient Name Column | Patient Name | Grid column | The patient's name. | [VERIFIED] |
| 2.5.4.6 | Selected Test Patient Result Column | Patient Result | Grid column | The result the patient obtained on the chosen test. | [VERIFIED] |
| 2.5.4.7 | Selected Test Signature Column | Sig. | Grid column | The area of the row where the result is signed for. | [VERIFIED] |
| 2.5.4.8 | Selected Test Patient Count Entry | No. of patient for this test | Field | The number of patients who had the chosen test. | [VERIFIED] |
| 2.5.4.9 | Selected Test Reception Officer Signature Area | موظف الاستقبال | Field | The area of the document where the reception officer signs. | [VERIFIED] |
| 2.5.4.10 | Selected Test Samples Officer Signature Area | مسئول العينات | Field | The area of the document where the samples officer signs. | [VERIFIED] |

## §2.3 Screens and Interface Structure

| Screen or output | Element identifiers |
|---|---|
| Work Sheet Module Entry | 2.1, 2.1.1, 2.1.2, 2.1.2.1, 2.1.2.2, 2.1.3, 2.1.3.1, 2.1.3.2, 2.1.3.3, 2.1.3.4, 2.1.3.5 |
| Work Sheet by Patient Names Screen | 2.2.1, 2.2.1.1, 2.2.1.1.1, 2.2.1.1.2, 2.2.1.2, 2.2.1.2.1, 2.2.1.2.2, 2.2.1.2.3, 2.2.1.2.4, 2.2.1.2.5, 2.2.1.3, 2.2.1.3.1, 2.2.1.3.2, 2.2.1.3.3, 2.2.1.4, 2.2.1.4.1, 2.2.1.4.2, 2.2.1.5, 2.2.1.5.1, 2.2.1.5.2, 2.2.1.5.3, 2.2.1.5.4 |
| Work Sheet by Test Names Screen | 2.3.1, 2.3.1.1, 2.3.1.1.1, 2.3.1.1.2, 2.3.1.1.3, 2.3.1.1.4, 2.3.1.2, 2.3.1.2.1, 2.3.1.2.2, 2.3.1.2.3, 2.3.1.2.4, 2.3.1.2.5, 2.3.1.2.6, 2.3.1.3, 2.3.1.3.1, 2.3.1.3.2, 2.3.1.3.3, 2.3.1.4, 2.3.1.4.1, 2.3.1.5, 2.3.1.5.1, 2.3.1.5.2, 2.3.1.5.3, 2.3.1.5.4, 2.3.1.5.5 |
| Period Test Classification Screen | 2.4.1, 2.4.1.1, 2.4.1.1.1, 2.4.1.1.2, 2.4.1.2 |
| Work Sheet Documents | 2.5, 2.5.1, 2.5.1.1, 2.5.1.1.1, 2.5.1.2, 2.5.1.3, 2.5.1.4, 2.5.1.5, 2.5.2, 2.5.2.1, 2.5.2.2, 2.5.2.3, 2.5.2.4, 2.5.2.5, 2.5.2.6, 2.5.2.7, 2.5.2.8, 2.5.2.9, 2.5.2.10, 2.5.2.11, 2.5.3, 2.5.3.1, 2.5.3.2, 2.5.3.3, 2.5.3.4, 2.5.3.5, 2.5.3.6, 2.5.3.7, 2.5.3.8, 2.5.4, 2.5.4.1, 2.5.4.2, 2.5.4.3, 2.5.4.4, 2.5.4.5, 2.5.4.6, 2.5.4.7, 2.5.4.8, 2.5.4.9, 2.5.4.10 |

## §2.4 Workflows


### Produce a work sheet for a group of patients

1. Press the work sheet ribbon item. → 2.1.1
2. Choose the work sheet by patient names. → 2.1.2.1
3. The work sheet window opens, through which a work sheet is made for the current day, for a chosen period, or for a chosen group of patients taken by name or by code. → 2.2.1
4. Choose the form of the work sheet: the full one or the shortened one. → 2.2.1.1
5. Set the filters: the referring entity, the code the patient range starts at, the code it ends at, the date the period starts at and the date it ends at. → 2.2.1.2
6. The patients in range are listed with their identifiers and their names, and with their count. → 2.2.1.3
7. To leave a patient out, press their name twice; the name is removed from the list. In the work sheet taken by code the names are not shown. → 2.2.1.3.3
8. To carry the unfinished tests alone, set the corresponding option; the option applies when a group of patients is selected. To cover the selected patient alone, set the second option. → 2.2.1.4
9. Press print, or press the preview action to see the work sheet before printing it. → 2.2.1.5.1, 2.2.1.5.3
10. The document produced carries the laboratory header, the user who printed it, the date and time, the number of patients, and one row per patient with the identifier, the name, the sex, the age, the date and the referring entity, together with the signature areas of the reception officer and of the samples officer. → 2.5.2

### Produce a work sheet for a set of tests

1. Press the work sheet ribbon item, then choose the work sheet by test names. → 2.1.1, 2.1.2.2
2. The work sheet window opens, through which a work sheet is made for a single test or a group of tests within a chosen period, or a group of tests for a certain number of patients. → 2.3.1
3. Choose the form of the work sheet: from the selected tests, from the test names of the period, from one chosen test of the period, or from a work group. → 2.3.1.1
4. Set the filters: the work group, the referring entity, the code the patient range starts at, the code it ends at, the date the period starts at and the date it ends at. → 2.3.1.2
5. The tests in range are listed with their identifiers and their names, and with their count. → 2.3.1.3
6. To leave a test out, press its name twice; the name is removed from the list. In the work sheet taken by group the tests are not shown. → 2.3.1.3.3
7. To carry the unfinished tests alone, set the corresponding option; the option applies when a work group is made, or for a referring entity alone. → 2.3.1.4
8. Press print, or press the preview action to see the work sheet before printing it. → 2.3.1.5.1, 2.3.1.5.3
9. The document produced carries the laboratory header, the user who printed it, the date and time, the number of test types, and under each test the identifier and the name of the test, the patients who had it with their identifiers and their results, and the number of those patients. → 2.5.3

### Classify the tests of a period

1. In the work sheet by test names, press the classification action. → 2.3.1.5.4
2. The classification of the tests of the chosen period is produced, showing how many times each test was performed in that period. → 2.4.1
3. The printed classification report carries the laboratory header, the date and time of printing, the user who printed it, and one row per test with the test name and the number of times it was performed. → 2.5.1, 2.4.1.2

## §2.5 Validation and Conditional Behavior

| ID | Rule | Tag |
|---|---|---|
| 2.2.1.1 | The two forms of the patient work sheet are alternatives; only one applies to a work sheet. | [VERIFIED] |
| 2.2.1.3.3 | The removal of a patient by pressing the name twice applies to the work sheet taken by names; in the work sheet taken by code the names are not shown. | [VERIFIED] |
| 2.2.1.4.1 | The unfinished-tests-only option applies when a group of patients is selected. | [VERIFIED] |
| 2.3.1.1 | The four forms of the test work sheet are alternatives; only one applies to a work sheet. | [VERIFIED] |
| 2.3.1.3.3 | The removal of a test by pressing the name twice applies to the work sheet taken by names; in the work sheet taken by group the tests are not shown. | [VERIFIED] |
| 2.3.1.4.1 | The unfinished-tests-only option applies when a work group is made, or for a referring entity alone. | [VERIFIED] |
| 2.1.3.5 | A work sheet may carry the unfinished tests alone, or all of the tests. | [VERIFIED] |
| 2.4.1 | The classification counts the performances of each test within the chosen period. | [VERIFIED] |

## §2.6 Outputs and Side Effects

The artefacts below are produced by the elements listed. The content of each artefact is defined once, in the element's own definition, and is not repeated here.

| ID | Output | Tag |
|---|---|---|
| 2.2.1.5.1 | Work sheet of a group of patients | [VERIFIED] |
| 2.3.1.5.1 | Work sheet of a set of tests | [VERIFIED] |
| 2.2.1.5.3 | Work sheet preview | [VERIFIED] |
| 2.3.1.5.3 | Test work sheet preview | [VERIFIED] |
| 2.4.1.2 | Period test classification report | [VERIFIED] |
| 2.5.2 | Work paper by patient identifier | [VERIFIED] |
| 2.5.3 | Work paper by test name | [VERIFIED] |
| 2.5.4 | Work paper for a selected test | [VERIFIED] |

## §2.7 Uncertainties and Unshown Elements

| ID | Item | Tag |
|---|---|---|
| 2.2.1.1.2 | What the shortened form of the patient work sheet omits relative to the full one is not established. | [UNVERIFIED] |
| 2.2.1.2.2 | Whether the code range filter addresses the patient code or the laboratory identifier is not established. | [UNVERIFIED] |
| 2.3.1.2.2 | Whether the code range filter addresses the patient code or the laboratory identifier is not established. | [UNVERIFIED] |
| 2.3.1.1.4 | The source of the work group a group work sheet is built from is not established. | [UNVERIFIED] |
| 2.5.2.10 | An entry on the work sheet header whose meaning is not established. | [UNVERIFIED] |
| 2.1.3.2 | Whether a work sheet for a single test is produced from the test list or from the specific-test option is not established. | [UNVERIFIED] |
| — | No validation, rejection message or recovery path is established for the work sheet windows. | [NOT-SHOWN] |
