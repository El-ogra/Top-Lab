# 1 — Patient Registration and Test Registration

Functional documentation of the patient-facing part of the laboratory system: signing in, registering a patient, capturing the patient's master data and medical-history answers, selecting and pricing the tests to be performed, maintaining the patient's account, entering and verifying results, producing combined / empty / culture / patient-history outputs, searching patients and their visits, the per-case audit trail, and delivery of results through the external result service.

**Identifier scheme.** The identifier is the address of the element in the interface, and the address is as deep as the containment really is. A module is `g`; a functional group is `g.s`; a screen, panel or container inside a group is `g.s.c`; a block or region inside a screen is `g.s.c.b`; an individual field, button, list, column, option, rule or output is `g.s.c.b.e`; and where an element has options of its own, each option is `g.s.c.b.e.o`. The scheme therefore reaches four, five or six levels according to the real depth of the interface, and never flattens a container that in fact contains something. Every field, button, list, column, option, action, rule and output carries its own identifier, and no identifier ever stands for more than one element. The same element is never documented twice: a field is described once, at the screen that carries it, and every other place refers to it by its identifier. Identifiers are permanent, are never reused for a different element, and are never renumbered.

**Source label.** The third column of the definition table carries the label as it is written on the screen in the source, in its original script, so that the element can be matched against the running software. A dash means the source carries no readable label for that element.

**Cross-module references.** A numeric identifier always points inside this module. A reference to a function of another module is made by name and module number — for example the work sheet module — and is converted into a numeric identifier when the modules are assembled into the single combined file.

**Evidence tags.** `[VERIFIED]` where the element is directly readable in the source; `[UNVERIFIED]` where the element's existence or meaning could not be established confidently; `[NOT-SHOWN]` where the element is named but its screen is never displayed, so that it is not counted as a missing feature; and `[SAMPLE]` where a value is an example rather than a requirement.

## §1.1 Functional Hierarchy

```
1  Patient Registration and Test Registration
├── 1.1  Application Shell and Sign-In
│   ├── 1.1.1  Top Ribbon
│   │   ├── 1.1.1.1  Ribbon Item List
│   │   │   ├── 1.1.1.1.1  Exit Item
│   │   │   ├── 1.1.1.1.2  About Us Item
│   │   │   ├── 1.1.1.1.3  Send Result Item
│   │   │   ├── 1.1.1.1.4  Setting Item
│   │   │   ├── 1.1.1.1.5  System Item
│   │   │   ├── 1.1.1.1.6  Users Item
│   │   │   ├── 1.1.1.1.7  Statistics Item
│   │   │   ├── 1.1.1.1.8  Accounts Item
│   │   │   ├── 1.1.1.1.9  Tools Item
│   │   │   ├── 1.1.1.1.10  Work Sheet Item
│   │   │   ├── 1.1.1.1.11  Laboratory Item
│   │   │   └── 1.1.1.1.12  Patients Item
│   │   └── 1.1.1.2  Active Module Indicator
│   ├── 1.1.2  Status Bar
│   │   ├── 1.1.2.1  User Name Indicator
│   │   ├── 1.1.2.2  Last Login Indicator
│   │   ├── 1.1.2.3  Database Label Indicator
│   │   └── 1.1.2.4  Today Indicator
│   ├── 1.1.3  Startup Splash Screen
│   │   ├── 1.1.3.1  Data Source Option Group
│   │   │   ├── 1.1.3.1.1  File Database Option
│   │   │   └── 1.1.3.1.2  Client Server Database Option
│   │   └── 1.1.3.2  Startup Progress Indicator
│   ├── 1.1.4  Sign-In Window
│   │   ├── 1.1.4.1  Sign-In User Name Field
│   │   ├── 1.1.4.2  Sign-In Password Field
│   │   ├── 1.1.4.3  Show Password Characters Option
│   │   ├── 1.1.4.4  Remember Login Option
│   │   └── 1.1.4.5  Application Sign In Action
│   └── 1.1.5  Patient Search Toolbar
│       ├── 1.1.5.1  Quick Search Field
│       └── 1.1.5.2  Quick Search Mode Options
├── 1.2  Patient Registration and Master Data
│   ├── 1.2.1  Main Patients Window
│   │   ├── 1.2.1.1  Add Patient Action
│   │   ├── 1.2.1.2  Enter Results Action
│   │   ├── 1.2.1.3  Find Patient Action
│   │   ├── 1.2.1.4  Period List Action
│   │   ├── 1.2.1.5  Unfinished Results View
│   │   ├── 1.2.1.6  Unverified Results View
│   │   ├── 1.2.1.7  Unprinted Results View
│   │   ├── 1.2.1.8  Deliver Results Action
│   │   ├── 1.2.1.9  Settle Accounts Action
│   │   ├── 1.2.1.10  Undelivered Results Count
│   │   └── 1.2.1.11  Test Registration Statistics Action
│   └── 1.2.2  Add-and-Edit Patient Data Screen
│       ├── 1.2.2.1  Patient Header Block
│       │   ├── 1.2.2.1.1  Entry Date Field
│       │   ├── 1.2.2.1.2  Receipt Date Field
│       │   ├── 1.2.2.1.3  Patient Code Field
│       │   ├── 1.2.2.1.4  VIP Action
│       │   └── 1.2.2.1.5  Laboratory ID Action
│       ├── 1.2.2.2  Identity Block
│       │   ├── 1.2.2.2.1  Patient Name Field
│       │   ├── 1.2.2.2.2  Sex Radio Group
│       │   │   ├── 1.2.2.2.2.1  Male Option
│       │   │   └── 1.2.2.2.2.2  Female Option
│       │   ├── 1.2.2.2.3  Age Value Field
│       │   ├── 1.2.2.2.4  Age Unit Selector
│       │   ├── 1.2.2.2.5  Mobile and Telephone Field
│       │   ├── 1.2.2.2.6  Card Number Field
│       │   ├── 1.2.2.2.7  Account Type Selector
│       │   ├── 1.2.2.2.8  Sample Type Field
│       │   ├── 1.2.2.2.9  Patient Address Field
│       │   ├── 1.2.2.2.10  Referring Entity Selector
│       │   │   └── 1.2.2.2.10.1  Referring Entity Additional Information Action
│       │   └── 1.2.2.2.11  Treating Doctor Selector
│       │       └── 1.2.2.2.11.1  Treating Doctor Additional Information Action
│       ├── 1.2.2.3  Medical History Block
│       │   ├── 1.2.2.3.1  Fasting Toggle
│       │   ├── 1.2.2.3.2  Fasting Hours Field
│       │   ├── 1.2.2.3.3  Diabetes Treatment Toggle
│       │   ├── 1.2.2.3.4  Blood-Thinning Treatment Toggle
│       │   ├── 1.2.2.3.5  Gland Treatment Toggle
│       │   ├── 1.2.2.3.6  Antiviral Treatment Toggle
│       │   ├── 1.2.2.3.7  Antibiotic Treatment Toggle
│       │   ├── 1.2.2.3.8  Liver Treatment Toggle
│       │   ├── 1.2.2.3.9  Pregnancy Toggle
│       │   ├── 1.2.2.3.10  Imaging Within Two Days Toggle
│       │   ├── 1.2.2.3.11  Anaemia Condition Toggle
│       │   ├── 1.2.2.3.12  Joint Inflammation Condition Toggle
│       │   ├── 1.2.2.3.13  High Blood Pressure Condition Toggle
│       │   ├── 1.2.2.3.14  Renal Failure Condition Toggle
│       │   └── 1.2.2.3.15  Lupus Condition Toggle
│       ├── 1.2.2.4  Notes and Communication Block
│       │   ├── 1.2.2.4.1  Notes Field
│       │   ├── 1.2.2.4.2  Print Result Password Field
│       │   │   └── 1.2.2.4.2.1  Print Result Password Enable Option
│       │   ├── 1.2.2.4.3  Send SMS Field
│       │   ├── 1.2.2.4.4  Send E-mail Field
│       │   ├── 1.2.2.4.5  Fax Field
│       │   ├── 1.2.2.4.6  Send Action
│       │   ├── 1.2.2.4.7  Site Upload Action
│       │   └── 1.2.2.4.8  Site Reference Field
│       ├── 1.2.2.5  Test Selection Block
│       │   ├── 1.2.2.5.1  Test Filter Bar
│       │   │   ├── 1.2.2.5.1.1  Test Filter Field
│       │   │   ├── 1.2.2.5.1.2  Test Filter Mode Options
│       │   │   ├── 1.2.2.5.1.3  Complete Test Menu Action
│       │   │   └── 1.2.2.5.1.4  Test Group Menu Action
│       │   ├── 1.2.2.5.2  Master Test List
│       │   ├── 1.2.2.5.3  Add Select Test Action
│       │   ├── 1.2.2.5.4  Delete Test Action
│       │   ├── 1.2.2.5.5  All Tests Action
│       │   ├── 1.2.2.5.6  Patient's Tests List
│       │   │   └── 1.2.2.5.6.1  Patient's Tests Count
│       │   ├── 1.2.2.5.7  Test Group List
│       │   ├── 1.2.2.5.8  Add All Tests In Group Action
│       │   ├── 1.2.2.5.9  Group Send E-mail Action
│       │   ├── 1.2.2.5.10  Sample Type Radio Group
│       │   │   ├── 1.2.2.5.10.1  Urine Option
│       │   │   ├── 1.2.2.5.10.2  Stool Option
│       │   │   ├── 1.2.2.5.10.3  Blood Option
│       │   │   ├── 1.2.2.5.10.4  Semen Option
│       │   │   └── 1.2.2.5.10.5  CSF Option
│       │   └── 1.2.2.5.11  Taken Outside Lab Flag
│       └── 1.2.2.6  Action Band
│           ├── 1.2.2.6.1  Save Registration Action
│           ├── 1.2.2.6.2  Add Entry Action
│           ├── 1.2.2.6.3  Edit Entry Action
│           ├── 1.2.2.6.4  Undo Action
│           ├── 1.2.2.6.5  Print Barcode Action
│           ├── 1.2.2.6.6  Print Receipt Action
│           ├── 1.2.2.6.7  Patient Invoice Action
│           ├── 1.2.2.6.8  Patient Envelope Print Action
│           ├── 1.2.2.6.9  Modify Action
│           ├── 1.2.2.6.10  Follow-up Action
│           ├── 1.2.2.6.11  Work Sheet Action
│           ├── 1.2.2.6.12  ID Card Action
│           ├── 1.2.2.6.13  Patient Screen Action
│           ├── 1.2.2.6.14  Main Point Action
│           ├── 1.2.2.6.15  Patient Window Action
│           └── 1.2.2.6.16  Lab Results Action
├── 1.3  Patient Account
│   ├── 1.3.1  Account Panel
│   │   ├── 1.3.1.1  Total of Tests Field
│   │   ├── 1.3.1.2  Discount Value Field
│   │   ├── 1.3.1.3  Discount Percentage Field
│   │   ├── 1.3.1.4  Paid Amount Field
│   │   │   ├── 1.3.1.4.1  Surcharge Entry Convention
│   │   │   └── 1.3.1.4.2  Enter Key Account Operation
│   │   ├── 1.3.1.5  Paid Previously Field
│   │   ├── 1.3.1.6  Amount After Discount Field
│   │   ├── 1.3.1.7  Total Collected Field
│   │   ├── 1.3.1.8  Remainder for Customer Field
│   │   ├── 1.3.1.9  Remainder for Laboratory Field
│   │   ├── 1.3.1.10  Confirm Payment Action
│   │   ├── 1.3.1.11  Net Settlement Action
│   │   ├── 1.3.1.12  Account Summary Action
│   │   ├── 1.3.1.13  Custody Action
│   │   └── 1.3.1.14  Account Back Action
│   └── 1.3.2  Patient Account Screen
│       ├── 1.3.2.1  Account Transaction Grid
│       │   ├── 1.3.2.1.1  Paid Date Column
│       │   ├── 1.3.2.1.2  Money Column
│       │   ├── 1.3.2.1.3  User Name Column
│       │   ├── 1.3.2.1.4  Edit Date Column
│       │   └── 1.3.2.1.5  Color Code Column
│       ├── 1.3.2.2  Modify Amount Action
│       ├── 1.3.2.3  Delete Transaction Action
│       ├── 1.3.2.4  Pay Action
│       ├── 1.3.2.5  Refund Action
│       ├── 1.3.2.6  Add Amount Action
│       ├── 1.3.2.7  Clear Account Action
│       └── 1.3.2.8  Account Patient List Action
├── 1.4  Results Delivery and Account Settlement
│   └── 1.4.1  Deliver Patient Results Screen
│       ├── 1.4.1.1  Delivery Filter Block
│       │   ├── 1.4.1.1.1  Delivery Date Filter
│       │   ├── 1.4.1.1.2  Delivery Name Filter
│       │   ├── 1.4.1.1.3  Delivery Status Filter
│       │   └── 1.4.1.1.4  Delivery Department Filter
│       ├── 1.4.1.2  Patient Scope Block
│       │   ├── 1.4.1.2.1  Patient Source Selector
│       │   ├── 1.4.1.2.2  Patient Group Selector
│       │   └── 1.4.1.2.3  Patients Today Panel
│       │       └── 1.4.1.2.3.1  Delivery Patient Grid
│       ├── 1.4.1.3  Delivery Result Block
│       │   └── 1.4.1.3.1  Delivery Result List Grid
│       │       ├── 1.4.1.3.1.1  Test Abbreviation Column
│       │       ├── 1.4.1.3.1.2  P Column
│       │       ├── 1.4.1.3.1.3  T Column
│       │       ├── 1.4.1.3.1.4  Result Column
│       │       ├── 1.4.1.3.1.5  Status Column
│       │       ├── 1.4.1.3.1.6  Delivery Finish Column
│       │       ├── 1.4.1.3.1.7  Delivery Verify Column
│       │       ├── 1.4.1.3.1.8  Delivery Print Column
│       │       ├── 1.4.1.3.1.9  Delivery Export Column
│       │       └── 1.4.1.3.1.10  See Report Action
│       └── 1.4.1.4  Delivery Action Block
│           ├── 1.4.1.4.1  Patient Account Action
│           ├── 1.4.1.4.2  Refresh Action
│           ├── 1.4.1.4.3  Delivery List Print Action
│           ├── 1.4.1.4.4  Preview Action
│           ├── 1.4.1.4.5  Copy Action
│           ├── 1.4.1.4.6  Notes Action
│           ├── 1.4.1.4.7  Delivery Patient Data Action
│           ├── 1.4.1.4.8  Combined Report Action
│           ├── 1.4.1.4.9  Patient Medical History Action
│           ├── 1.4.1.4.10  Delivery Empty Report Action
│           ├── 1.4.1.4.11  Delivery Envelope Print Action
│           ├── 1.4.1.4.12  Send SMS Action
│           └── 1.4.1.4.13  Delivery Main List Action
├── 1.5  Patient and Test Modification
│   ├── 1.5.1  Modification Path One
│   │   ├── 1.5.1.1  Patient Icon Action
│   │   ├── 1.5.1.2  Patient List Open Action
│   │   ├── 1.5.1.3  Registration Day Selector
│   │   ├── 1.5.1.4  Edit Patient Action
│   │   ├── 1.5.1.5  Patient Selection Action
│   │   └── 1.5.1.6  Modification Save Action
│   ├── 1.5.2  Modification Path Two
│   │   ├── 1.5.2.1  Search Action
│   │   └── 1.5.2.2  Patient Data Entry Action
│   └── 1.5.3  Patient Data Access Action
├── 1.6  Test Result Entry
│   └── 1.6.1  Result Entry Screen
│       ├── 1.6.1.1  Patient List Panel
│       │   ├── 1.6.1.1.1  Result Entry Patient Grid
│       │   ├── 1.6.1.1.2  Patient Name Click Action
│       │   └── 1.6.1.1.3  Patient Count Indicator
│       ├── 1.6.1.2  Test Results Grid
│       │   ├── 1.6.1.2.1  Result Entry Test Abbreviation Column
│       │   ├── 1.6.1.2.2  Result Entry P Column
│       │   ├── 1.6.1.2.3  Result Entry T Column
│       │   ├── 1.6.1.2.4  Result Entry Result Column
│       │   ├── 1.6.1.2.5  Result Entry Status Column
│       │   ├── 1.6.1.2.6  Abnormal Flag Column
│       │   ├── 1.6.1.2.7  Low and High Column
│       │   ├── 1.6.1.2.8  Unit Column
│       │   ├── 1.6.1.2.9  Normal Range Column
│       │   ├── 1.6.1.2.10  Finish Flag
│       │   ├── 1.6.1.2.11  Verify Flag
│       │   ├── 1.6.1.2.12  Result Print Flag
│       │   ├── 1.6.1.2.13  Export Flag
│       │   ├── 1.6.1.2.14  Per-Test Comment Field
│       │   └── 1.6.1.2.15  Double-Click Test Action
│       ├── 1.6.1.3  Entry Action Block
│       │   ├── 1.6.1.3.1  Save Results Action
│       │   ├── 1.6.1.3.2  Print Preview Action
│       │   └── 1.6.1.3.3  Result Report Print Action
│       ├── 1.6.1.4  Report Settings Screen
│       │   ├── 1.6.1.4.1  Report Title Block
│       │   │   ├── 1.6.1.4.1.1  Report Screen Title Field
│       │   │   ├── 1.6.1.4.1.2  Report Main Title Field
│       │   │   ├── 1.6.1.4.1.3  Report Sub Title Field
│       │   │   ├── 1.6.1.4.1.4  Report Sub Sub-title Field
│       │   │   ├── 1.6.1.4.1.5  Reference Range Field
│       │   │   ├── 1.6.1.4.1.6  Report Comment Field
│       │   │   ├── 1.6.1.4.1.7  Report Normal Range Field
│       │   │   ├── 1.6.1.4.1.8  Main Symbol Field
│       │   │   ├── 1.6.1.4.1.9  Sub Symbol Field
│       │   │   └── 1.6.1.4.1.10  Sub-title Symbol Field
│       │   ├── 1.6.1.4.2  Report Display Option Block
│       │   │   ├── 1.6.1.4.2.1  Show Abnormal Background Option
│       │   │   ├── 1.6.1.4.2.2  Print Status Flag Option
│       │   │   └── 1.6.1.4.2.3  Print Patient History Automatically Option
│       │   ├── 1.6.1.4.3  Report Typography Block
│       │   │   ├── 1.6.1.4.3.1  Symbol Font Selectors
│       │   │   ├── 1.6.1.4.3.2  Vertical Spacing Fields
│       │   │   ├── 1.6.1.4.3.3  Normal Result Font Selector
│       │   │   ├── 1.6.1.4.3.4  Text Alignment Control
│       │   │   ├── 1.6.1.4.3.5  Cell Expansion Control
│       │   │   └── 1.6.1.4.3.6  Offset Amount Control
│       │   └── 1.6.1.4.4  Report Output Block
│       │       ├── 1.6.1.4.4.1  Printing Target Selector
│       │       ├── 1.6.1.4.4.2  Save Settings Action
│       │       ├── 1.6.1.4.4.3  Export Data Action
│       │       └── 1.6.1.4.4.4  Close Action
│       ├── 1.6.1.5  Report Entry Grid
│       │   ├── 1.6.1.5.1  Report Entry Test Name Column
│       │   ├── 1.6.1.5.2  Report Entry Result Column
│       │   ├── 1.6.1.5.3  Report Entry Unit Column
│       │   ├── 1.6.1.5.4  Report Entry Flag Column
│       │   ├── 1.6.1.5.5  Report Entry Normal Ranges Column
│       │   ├── 1.6.1.5.6  Report Entry Verified Flag
│       │   ├── 1.6.1.5.7  Report Entry Print Flag
│       │   ├── 1.6.1.5.8  Report Entry Comment Field
│       │   └── 1.6.1.5.9  Urine Sample Sub-Row Panel
│       │       ├── 1.6.1.5.9.1  Urine Sample Sub-Row
│       │       ├── 1.6.1.5.9.2  Sub-Result Row
│       │       └── 1.6.1.5.9.3  Missing Sample Marker
│       ├── 1.6.1.6  Test Entry Variants
│       │   ├── 1.6.1.6.1  Single Test Entry
│       │   ├── 1.6.1.6.2  Profile Test Entry
│       │   ├── 1.6.1.6.3  Paired Control And Patient Entry
│       │   ├── 1.6.1.6.4  Derived Value Entry
│       │   ├── 1.6.1.6.5  Multi-Unit Entry
│       │   └── 1.6.1.6.6  Qualitative Result Entry
│       └── 1.6.1.7  Report Screen Actions
│           ├── 1.6.1.7.1  Save Report Action
│           ├── 1.6.1.7.2  Report Screen Print Action
│           ├── 1.6.1.7.3  Report Screen Preview Action
│           ├── 1.6.1.7.4  Report Patient History Action
│           ├── 1.6.1.7.5  Report Screen Back Action
│           └── 1.6.1.7.6  Report Screen Main List Action
├── 1.7  Combined Report
│   └── 1.7.1  Combined Report Screen
│       ├── 1.7.1.1  Patient Tests In Our Lab List
│       ├── 1.7.1.2  Available Tests In Report List
│       ├── 1.7.1.3  Add Test To Report Action
│       ├── 1.7.1.4  Reorder Report Tests Action
│       │   ├── 1.7.1.4.1  Move Test Up Action
│       │   └── 1.7.1.4.2  Move Test Down Action
│       └── 1.7.1.5  Combined Report Option Block
│           ├── 1.7.1.5.1  Print Sub Title Option
│           └── 1.7.1.5.2  Print Printed Test Again Option
├── 1.8  Empty Report
│   └── 1.8.1  Empty Report Screen
│       ├── 1.8.1.1  Empty Report Open Action
│       ├── 1.8.1.2  Enter Data And Print Option
│       └── 1.8.1.3  Patient Data Sheet Option
├── 1.9  Culture Result Entry
│   └── 1.9.1  Culture Screen
│       ├── 1.9.1.1  Culture Data Block
│       │   ├── 1.9.1.1.1  Sample Field
│       │   ├── 1.9.1.1.2  Organism A Field
│       │   ├── 1.9.1.1.3  Organism B Field
│       │   ├── 1.9.1.1.4  Organism C Field
│       │   ├── 1.9.1.1.5  Culture Condition Field
│       │   └── 1.9.1.1.6  Colony Count Field
│       ├── 1.9.1.2  Microscopy Block
│       │   ├── 1.9.1.2.1  Reaction Field
│       │   ├── 1.9.1.2.2  Pus Cells Field
│       │   ├── 1.9.1.2.3  Red Blood Cells Field
│       │   ├── 1.9.1.2.4  Epithelial Cells Field
│       │   ├── 1.9.1.2.5  Crystal Field
│       │   ├── 1.9.1.2.6  Fungi Field
│       │   ├── 1.9.1.2.7  Bacteria Field
│       │   ├── 1.9.1.2.8  Additional Microscopy Fields
│       │   ├── 1.9.1.2.9  Direct Culture Fields
│       │   └── 1.9.1.2.10  Microscopy Complete Flag
│       ├── 1.9.1.3  Culture Flag Block
│       │   ├── 1.9.1.3.1  Culture Verified Flag
│       │   └── 1.9.1.3.2  Culture Print Flag
│       ├── 1.9.1.4  Sensitivity Block
│       │   ├── 1.9.1.4.1  Antibiotic List
│       │   ├── 1.9.1.4.2  Antibiotic Add Action
│       │   ├── 1.9.1.4.3  Sensitivity Classification
│       │   │   ├── 1.9.1.4.3.1  Highly For Option
│       │   │   ├── 1.9.1.4.3.2  Moderate For Option
│       │   │   ├── 1.9.1.4.3.3  Low For Option
│       │   │   └── 1.9.1.4.3.4  Resistant For Option
│       │   └── 1.9.1.4.4  Report Element Visibility Control
│       ├── 1.9.1.5  Culture Action Block
│       │   ├── 1.9.1.5.1  Sensitivity Entry Action
│       │   └── 1.9.1.5.2  Save Culture Data Action
│       └── 1.9.1.6  Microbiology Report Output
├── 1.10  Patient Search
│   └── 1.10.1  Search Screen
│       ├── 1.10.1.1  Search Criteria Block
│       │   ├── 1.10.1.1.1  Search by Patient Name Exact
│       │   ├── 1.10.1.1.2  Search by Patient Name Partial
│       │   ├── 1.10.1.1.3  Search by Treating Doctor
│       │   ├── 1.10.1.1.4  Search by Request Date
│       │   ├── 1.10.1.1.5  Search by Test Within Period
│       │   ├── 1.10.1.1.6  Search by Phone Number
│       │   ├── 1.10.1.1.7  Search by Card Number
│       │   ├── 1.10.1.1.8  Search by Patient Age
│       │   ├── 1.10.1.1.9  Search by Patient Gender
│       │   ├── 1.10.1.1.10  Search Within Branch
│       │   ├── 1.10.1.1.11  Search for Results Not Entered
│       │   ├── 1.10.1.1.12  Search for Results Not Verified
│       │   ├── 1.10.1.1.13  Search for Results Not Printed
│       │   ├── 1.10.1.1.14  Search for Results Not Delivered
│       │   └── 1.10.1.1.15  Date Range Mode Options
│       ├── 1.10.1.2  Search Scope Block
│       │   ├── 1.10.1.2.1  Branch Selector
│       │   └── 1.10.1.2.2  Source Selector
│       ├── 1.10.1.3  Search Result Block
│       │   ├── 1.10.1.3.1  Search Result Grid
│       │   ├── 1.10.1.3.2  Search Result Count
│       │   ├── 1.10.1.3.3  New Search Action
│       │   └── 1.10.1.3.4  Enter ID Field
│       └── 1.10.1.4  Search Action Bar
│           ├── 1.10.1.4.1  Patient Tests Action
│           ├── 1.10.1.4.2  Search Patient Data Action
│           ├── 1.10.1.4.3  Search Lab Results Action
│           ├── 1.10.1.4.4  Delete Patient Action
│           ├── 1.10.1.4.5  Open Account Action
│           ├── 1.10.1.4.6  Restrict To VIP Action
│           ├── 1.10.1.4.7  Add Action
│           ├── 1.10.1.4.8  Collected Results Action
│           ├── 1.10.1.4.9  Modify Group Action
│           └── 1.10.1.4.10  Search Main List Action
├── 1.11  Patient Visit History
│   ├── 1.11.1  Laboratory ID Lifecycle
│   │   ├── 1.11.1.1  Laboratory ID Generation
│   │   ├── 1.11.1.2  Laboratory ID Storage
│   │   └── 1.11.1.3  Laboratory ID Display at Next Registration
│   └── 1.11.2  Visit History
│       ├── 1.11.2.1  Search by Laboratory ID
│       ├── 1.11.2.2  Visit List Grid
│       └── 1.11.2.3  Visit Tests List
├── 1.12  Case Follow-up and Audit Trail
│   └── 1.12.1  Case History Screen
│       ├── 1.12.1.1  Registration Audit Entry
│       ├── 1.12.1.2  Modification Count
│       ├── 1.12.1.3  Last Modifier Entry
│       ├── 1.12.1.4  Result Entry Audit Entry
│       ├── 1.12.1.5  Verification Audit Entry
│       ├── 1.12.1.6  Printing Count
│       ├── 1.12.1.7  Last Printer Entry
│       ├── 1.12.1.8  Delivery Audit Entry
│       └── 1.12.1.9  Patient Samples Block
│           ├── 1.12.1.9.1  Tube Name Column
│           ├── 1.12.1.9.2  Sample Tests Column
│           └── 1.12.1.9.3  Sample Take Action
├── 1.13  Site Result Delivery
│   ├── 1.13.1  Upload Screen
│   │   ├── 1.13.1.1  Upload Patient Tests List
│   │   ├── 1.13.1.2  Upload Action
│   │   ├── 1.13.1.3  Select All Action
│   │   ├── 1.13.1.4  Unselect All Action
│   │   ├── 1.13.1.5  Upload Progress Indicator
│   │   ├── 1.13.1.6  Block Result Action
│   │   └── 1.13.1.7  Mobile Number Requirement
│   ├── 1.13.2  Uploaded Patient Count
│   ├── 1.13.3  Site Access Channels
│   ├── 1.13.4  Site Account Types
│   │   ├── 1.13.4.1  Patient Account Type
│   │   ├── 1.13.4.2  Doctor Account Type
│   │   └── 1.13.4.3  Laboratory Account Type
│   ├── 1.13.5  Site Sign-In Block
│   │   ├── 1.13.5.1  Patient Sign-In Data
│   │   ├── 1.13.5.2  Doctor Or Laboratory Sign-In Data
│   │   └── 1.13.5.3  Site Sign In Action
│   ├── 1.13.6  Site Result View
│   │   ├── 1.13.6.1  Result View Action
│   │   └── 1.13.6.2  Result Export Formats
│   │       ├── 1.13.6.2.1  Print Format
│   │       ├── 1.13.6.2.2  Word Document Format
│   │       ├── 1.13.6.2.3  PDF Format
│   │       └── 1.13.6.2.4  Image Format
│   └── 1.13.7  Site Access Data On Receipt
├── 1.14  Barcode Labels and Specimen Containers
│   ├── 1.14.1  Barcode Label Kinds
│   │   ├── 1.14.1.1  Specimen Container Label
│   │   ├── 1.14.1.2  File Barcode Label
│   │   ├── 1.14.1.3  Laboratory ID Barcode Label
│   │   └── 1.14.1.4  Site Access Label
│   ├── 1.14.2  Specimen Container Label Content
│   │   ├── 1.14.2.1  Patient Name on Label
│   │   ├── 1.14.2.2  Sex and Age Line on Label
│   │   ├── 1.14.2.3  Allocated Test List on Label
│   │   ├── 1.14.2.4  Label Barcode
│   │   ├── 1.14.2.5  Laboratory Identifier on Label
│   │   ├── 1.14.2.6  Container Designation on Label
│   │   ├── 1.14.2.7  File Barcode Number
│   │   └── 1.14.2.8  Label Print Timestamp
│   ├── 1.14.3  Specimen Container Designations
│   │   ├── 1.14.3.1  Serum Fasting Container
│   │   ├── 1.14.3.2  Serum Postprandial Container
│   │   ├── 1.14.3.3  EDTA Blood Container
│   │   ├── 1.14.3.4  Citrated Blood Container
│   │   ├── 1.14.3.5  Urine Container
│   │   ├── 1.14.3.6  Stool Container
│   │   └── 1.14.3.7  Semen Container
│   └── 1.14.4  Test Distribution Across Containers
│       ├── 1.14.4.1  Automatic Test Sorting
│       ├── 1.14.4.2  Container Distribution Edit
│       ├── 1.14.4.3  Vertical Coding on Printed Documents
│       └── 1.14.4.4  Repeated Sticker Printing
└── 1.15  Patient Medical History and History Report
    ├── 1.15.1  Automatic History Insertion
    │   ├── 1.15.1.1  Automatic Previous Result Insertion
    │   ├── 1.15.1.2  Multi-Test History Insertion
    │   ├── 1.15.1.3  History Patient Identification
    │   │   ├── 1.15.1.3.1  Identify By Patient Name
    │   │   └── 1.15.1.3.2  Identify By Laboratory Code
    │   ├── 1.15.1.4  Automatic History Display Setting
    │   └── 1.15.1.5  History Sorting Method Setting
    ├── 1.15.2  History Test Selection
    │   ├── 1.15.2.1  Previous Visit Test List
    │   ├── 1.15.2.2  Previous Test Selection Action
    │   ├── 1.15.2.3  Previous Visit Count Indicator
    │   ├── 1.15.2.4  Repeated Test Note
    │   └── 1.15.2.5  Previous Result Grid
    ├── 1.15.3  History Output Mode
    │   ├── 1.15.3.1  Attach History In Result Report Option
    │   ├── 1.15.3.2  Print History In Separate Report Option
    │   ├── 1.15.3.3  History Mode Confirm Action
    │   └── 1.15.3.4  History Mode Cancel Action
    └── 1.15.4  Patient History Report
        ├── 1.15.4.1  History Report Header
        │   ├── 1.15.4.1.1  History Report Title
        │   ├── 1.15.4.1.2  History Date Range Field
        │   ├── 1.15.4.1.3  History Printed By Entry
        │   └── 1.15.4.1.4  History Printed In Entry
        ├── 1.15.4.2  History Report Grid
        │   ├── 1.15.4.2.1  History Analysis Date Column
        │   ├── 1.15.4.2.2  History Patient ID Column
        │   ├── 1.15.4.2.3  History Patient Name Column
        │   └── 1.15.4.2.4  History Test Result Column
        ├── 1.15.4.3  Multi-Patient History Panel
        │   ├── 1.15.4.3.1  Multi-Patient History Grid
        │   └── 1.15.4.3.2  Multi-Patient History Count
        ├── 1.15.4.4  History Test Lists
        │   ├── 1.15.4.4.1  History Tests In Lab List
        │   └── 1.15.4.4.2  History Tests Available In Report List
        ├── 1.15.4.5  Delete All History Action
        ├── 1.15.4.6  History Report Print Action
        └── 1.15.4.7  History Report Preview Action
```

## §1.2 Element Definitions

### 1.1 — Application Shell and Sign-In

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.1.1 | Top Ribbon | الشريط العلوي | Control | The persistent ribbon of the application; it carries one item per module and the item of the active module is the one shown as active. | [VERIFIED] |
| 1.1.1.1 | Ribbon Item List | — | List | The ordered set of ribbon items; each item opens one module of the program. | [VERIFIED] |
| 1.1.1.1.1 | Exit Item | Exit | Navigation | Closes the program. | [VERIFIED] |
| 1.1.1.1.2 | About Us Item | About Us | Navigation | Opens the program's information window. | [VERIFIED] |
| 1.1.1.1.3 | Send Result Item | Send Result | Navigation | Opens the result-delivery screen. | [VERIFIED] |
| 1.1.1.1.4 | Setting Item | Setting | Navigation | Opens the system settings. | [VERIFIED] |
| 1.1.1.1.5 | System Item | System | Navigation | Opens the system module. | [VERIFIED] |
| 1.1.1.1.6 | Users Item | Users | Navigation | Opens the users module. | [VERIFIED] |
| 1.1.1.1.7 | Statistics Item | Statistics | Navigation | Opens the statistics module. | [VERIFIED] |
| 1.1.1.1.8 | Accounts Item | Accounts | Navigation | Opens the laboratory accounts module. | [VERIFIED] |
| 1.1.1.1.9 | Tools Item | Tools | Navigation | Opens the tools module. | [VERIFIED] |
| 1.1.1.1.10 | Work Sheet Item | Work sheet | Navigation | Opens the work sheet module. | [VERIFIED] |
| 1.1.1.1.11 | Laboratory Item | Laboratory | Navigation | Opens the laboratory module. | [VERIFIED] |
| 1.1.1.1.12 | Patients Item | Patients | Navigation | Opens the patient-side module. | [VERIFIED] |
| 1.1.1.2 | Active Module Indicator | — | Indicator | Marks the ribbon item of the module currently in use. | [VERIFIED] |
| 1.1.2 | Status Bar | — | Control | The persistent bar beneath the working area. | [VERIFIED] |
| 1.1.2.1 | User Name Indicator | User Name | Field | The signed-in user. | [VERIFIED] |
| 1.1.2.2 | Last Login Indicator | Last Login | Field | The previous sign-in of that user, with its date and time. | [VERIFIED] |
| 1.1.2.3 | Database Label Indicator | SQL DATABASE | Field | The label of the data source in use. | [VERIFIED] |
| 1.1.2.4 | Today Indicator | Today | Field | The current date and time. | [VERIFIED] |
| 1.1.3 | Startup Splash Screen | — | Screen | The screen shown while the program starts. It carries the data-source choice and a progress indication. | [VERIFIED] |
| 1.1.3.1 | Data Source Option Group | — | Field group | The two data sources the program may be started against. | [VERIFIED] |
| 1.1.3.1.1 | File Database Option | — | Option | Starts the program against a file-based database. | [VERIFIED] |
| 1.1.3.1.2 | Client Server Database Option | — | Option | Starts the program against a client-server database. | [VERIFIED] |
| 1.1.3.2 | Startup Progress Indicator | — | Indicator | Reports the progress of the start sequence. | [VERIFIED] |
| 1.1.4 | Sign-In Window | — | Screen | The window that authenticates the user before the program opens. | [VERIFIED] |
| 1.1.4.1 | Sign-In User Name Field | User Name | Field | The user's name, entered at sign-in. | [VERIFIED] |
| 1.1.4.2 | Sign-In Password Field | Password | Field | The user's password, entered at sign-in. | [VERIFIED] |
| 1.1.4.3 | Show Password Characters Option | Show password characters | Field | Controls whether the password is displayed as characters. | [VERIFIED] |
| 1.1.4.4 | Remember Login Option | Remember Login | Field | Controls whether the sign-in is remembered for the next start. | [VERIFIED] |
| 1.1.4.5 | Application Sign In Action | Sign in | Action | Authenticates the user and opens the program. | [VERIFIED] |
| 1.1.5 | Patient Search Toolbar | — | Control | The toolbar carried above the working area of the patient module; it holds the quick-search control and the mode options. | [VERIFIED] |
| 1.1.5.1 | Quick Search Field | start | Field | The quick-search box of the patient module. | [VERIFIED] |
| 1.1.5.2 | Quick Search Mode Options | TM / TG / CG | Field group | Three mutually exclusive mode options marked with the short codes TM, TG and CG. The meaning of the codes is not established. | [UNVERIFIED] |

### 1.2 — Patient Registration and Master Data

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.2.1 | Main Patients Window | — | Screen | The entry point of the patient-side module. Through it a patient can be added and their barcode and receipt printed, their results entered and printed; a specific patient can be found, or the patients of a period displayed with their unfinished, unverified and unprinted results identified; and results delivered with the patients' accounts settled and the undelivered results counted. | [VERIFIED] |
| 1.2.1.1 | Add Patient Action | إضافة | Action | Opens the add-and-edit patient data screen for a new patient. | [VERIFIED] |
| 1.2.1.2 | Enter Results Action | نتائج التحاليل | Action | Opens the result-entry screen. | [VERIFIED] |
| 1.2.1.3 | Find Patient Action | — | Action | Opens the patient-search screen. | [VERIFIED] |
| 1.2.1.4 | Period List Action | — | Action | Displays the patients of a chosen period. | [VERIFIED] |
| 1.2.1.5 | Unfinished Results View | — | View | Lists the results that have been entered but not finished. | [VERIFIED] |
| 1.2.1.6 | Unverified Results View | — | View | Lists the results that have been entered but not verified. | [VERIFIED] |
| 1.2.1.7 | Unprinted Results View | — | View | Lists the results that have been entered but not printed. | [VERIFIED] |
| 1.2.1.8 | Deliver Results Action | — | Action | Opens the results-delivery screen. | [VERIFIED] |
| 1.2.1.9 | Settle Accounts Action | — | Action | Opens the account-settlement work of the delivery screen. | [VERIFIED] |
| 1.2.1.10 | Undelivered Results Count | — | Indicator | Counts the results that have not yet been delivered. | [VERIFIED] |
| 1.2.1.11 | Test Registration Statistics Action | — | Action | Opens a statistics view of test registration. The resulting screen is not shown. | [NOT-SHOWN] |
| 1.2.2 | Add-and-Edit Patient Data Screen | إضافة وتعديل بيانات المرضى | Screen | The screen used both to register a new patient and to modify an existing patient or test registration. It carries the patient header, the identity block, the medical-history block, the notes and communication block, the test-selection block, the account panel and the action band. | [VERIFIED] |
| 1.2.2.1 | Patient Header Block | — | Block | The strip above the form carrying the visit dates, the patient code and two header actions. | [VERIFIED] |
| 1.2.2.1.1 | Entry Date Field | تاريخ الدخول | Field | The date on which the patient was entered; it carries the time as well. | [VERIFIED] |
| 1.2.2.1.2 | Receipt Date Field | تاريخ الاستلام | Field | The date and time at which the result is received. | [VERIFIED] |
| 1.2.2.1.3 | Patient Code Field | كود المريض | Field | The system-assigned patient code, shown read-only. | [VERIFIED] |
| 1.2.2.1.4 | VIP Action | VIP | Action | Marks the patient as belonging to the VIP group. | [VERIFIED] |
| 1.2.2.1.5 | Laboratory ID Action | Lab. ID | Action | Generates or shows the laboratory identifier of the current visit. | [VERIFIED] |
| 1.2.2.2 | Identity Block | — | Block | The block that carries the patient's own particulars. | [VERIFIED] |
| 1.2.2.2.1 | Patient Name Field | اسم المريض | Field | Text entry for the patient's name, with a drop-down offering names already registered. | [VERIFIED] |
| 1.2.2.2.2 | Sex Radio Group | النوع | Field | The patient's sex. | [VERIFIED] |
| 1.2.2.2.2.1 | Male Option | Male | Option | Records the patient as male. | [VERIFIED] |
| 1.2.2.2.2.2 | Female Option | Female | Option | Records the patient as female. | [VERIFIED] |
| 1.2.2.2.3 | Age Value Field | سن المريض | Field | The patient's age. | [VERIFIED] |
| 1.2.2.2.4 | Age Unit Selector | Years | Field | The unit of the age value; Years is the option shown. | [VERIFIED] |
| 1.2.2.2.5 | Mobile and Telephone Field | موبيل / تليفون | Field | The patient's telephone number. Optional. | [VERIFIED] |
| 1.2.2.2.6 | Card Number Field | رقم بطاقة / الملف | Field | The patient's card number. Optional. | [VERIFIED] |
| 1.2.2.2.7 | Account Type Selector | نوع الحساب | Field | The account type the patient is handled under; Individual is the option shown. | [VERIFIED] |
| 1.2.2.2.8 | Sample Type Field | نوع العينة | Field | The sample type, as carried in the identity block. | [VERIFIED] |
| 1.2.2.2.9 | Patient Address Field | عنوان المريض | Field | The patient's address. Optional. | [VERIFIED] |
| 1.2.2.2.10 | Referring Entity Selector | جهة الإحالة | Field | The entity that referred the patient. | [VERIFIED] |
| 1.2.2.2.10.1 | Referring Entity Additional Information Action | معلومات اضافية | Action | Opens the additional information of the referring entity. | [VERIFIED] |
| 1.2.2.2.11 | Treating Doctor Selector | الطبيب المعالج | Field | The doctor treating the patient. Optional. | [VERIFIED] |
| 1.2.2.2.11.1 | Treating Doctor Additional Information Action | معلومات اضافية | Action | Opens the additional information of the treating doctor. | [VERIFIED] |
| 1.2.2.3 | Medical History Block | — | Block | The block of check-box questions covering the patient's fasting state, the medication the patient is on, the conditions the patient has, and two further questions. Each question is answered independently and carries its own check box. | [VERIFIED] |
| 1.2.2.3.1 | Fasting Toggle | صائم | Toggle | Records that the patient is fasting. | [VERIFIED] |
| 1.2.2.3.2 | Fasting Hours Field | عدد ساعات الصيام | Field | The number of fasting hours, shown beside the fasting toggle. | [VERIFIED] |
| 1.2.2.3.3 | Diabetes Treatment Toggle | هل يؤخذ علاج للسكر | Toggle | Asks whether the patient is being treated for diabetes. | [VERIFIED] |
| 1.2.2.3.4 | Blood-Thinning Treatment Toggle | هل يؤخذ علاج لسيولة الدم | Toggle | Asks whether the patient is being treated for blood thinning. | [VERIFIED] |
| 1.2.2.3.5 | Gland Treatment Toggle | هل يؤخذ علاج للغدد | Toggle | Asks whether the patient is being treated for a gland disorder. | [VERIFIED] |
| 1.2.2.3.6 | Antiviral Treatment Toggle | هل يؤخذ علاج للفيروسات | Toggle | Asks whether the patient is being treated for a viral infection. | [VERIFIED] |
| 1.2.2.3.7 | Antibiotic Treatment Toggle | هل يؤخذ مضاد حيوي | Toggle | Asks whether the patient is being treated with an antibiotic. | [VERIFIED] |
| 1.2.2.3.8 | Liver Treatment Toggle | هل يؤخذ علاج للكبد | Toggle | Asks whether the patient is being treated for a liver condition. | [VERIFIED] |
| 1.2.2.3.9 | Pregnancy Toggle | للسيدات هل يوجد حمل | Toggle | Asks whether the patient is pregnant; the question is offered to female patients. | [VERIFIED] |
| 1.2.2.3.10 | Imaging Within Two Days Toggle | هل تم عمل اشاعة بالصبغة او بالموجات الصوتية خلال يومين | Toggle | Records that a contrast study or an ultrasound was performed within the last two days. | [VERIFIED] |
| 1.2.2.3.11 | Anaemia Condition Toggle | مريض بفقر الدم | Toggle | Records that the patient has anaemia. | [VERIFIED] |
| 1.2.2.3.12 | Joint Inflammation Condition Toggle | مريض بالتهاب المفاصل | Toggle | Records that the patient has joint inflammation. | [VERIFIED] |
| 1.2.2.3.13 | High Blood Pressure Condition Toggle | مريض بالضغط الدم | Toggle | Records that the patient has high blood pressure. | [VERIFIED] |
| 1.2.2.3.14 | Renal Failure Condition Toggle | مريض بالفشل كلوي | Toggle | Records that the patient has renal failure. | [VERIFIED] |
| 1.2.2.3.15 | Lupus Condition Toggle | مريض بالذئبة الحمراء | Toggle | Records that the patient has lupus erythematosus. | [VERIFIED] |
| 1.2.2.4 | Notes and Communication Block | — | Block | The block that carries the patient's notes and the channels through which a result may be sent. | [VERIFIED] |
| 1.2.2.4.1 | Notes Field | ملاحظات | Field | Free-text notes for the patient, shown in the result-entry and result-delivery windows. Optional. | [VERIFIED] |
| 1.2.2.4.2 | Print Result Password Field | Print Result Password | Field | The password required to print the result. | [VERIFIED] |
| 1.2.2.4.2.1 | Print Result Password Enable Option | — | Field | Enables the password requirement for printing the result. | [VERIFIED] |
| 1.2.2.4.3 | Send SMS Field | Send SMS | Field | The destination of the SMS. | [VERIFIED] |
| 1.2.2.4.4 | Send E-mail Field | Send E-mail | Field | The destination of the e-mail. | [VERIFIED] |
| 1.2.2.4.5 | Fax Field | By Fax | Field | The destination of the fax. | [VERIFIED] |
| 1.2.2.4.6 | Send Action | — | Action | Sends the patient's result by SMS, e-mail or fax. | [VERIFIED] |
| 1.2.2.4.7 | Site Upload Action | — | Action | Opens the result-upload screen of the external result service. | [VERIFIED] |
| 1.2.2.4.8 | Site Reference Field | NATIGH.COM | Field | A field carrying the address of the external result service, shown beside the notes; the same address is printed on the patient's site-access label and on the receipt. The source does not state whether the field itself is editable. | [UNVERIFIED] |
| 1.2.2.5 | Test Selection Block | — | Block | The block from which the tests to be performed are chosen. | [VERIFIED] |
| 1.2.2.5.1 | Test Filter Bar | — | Control | The strip above the master test list, carrying the filter control, the three mode options and the menu actions. | [VERIFIED] |
| 1.2.2.5.1.1 | Test Filter Field | start | Field | The filter control of the master test list. | [VERIFIED] |
| 1.2.2.5.1.2 | Test Filter Mode Options | TM / TG / CG | Field group | Three mutually exclusive mode options marked with the short codes TM, TG and CG. The meaning of the codes is not established. | [UNVERIFIED] |
| 1.2.2.5.1.3 | Complete Test Menu Action | complete test menu | Action | Opens the complete test menu of the master test list. | [VERIFIED] |
| 1.2.2.5.1.4 | Test Group Menu Action | test group menu | Action | Opens the test-group menu of the master test list. | [VERIFIED] |
| 1.2.2.5.2 | Master Test List | — | List | The laboratory's scrollable list of registered tests, from which a test is chosen for the patient. | [VERIFIED] |
| 1.2.2.5.3 | Add Select Test Action | Add select test | Action | Adds the selected master test to the patient. | [VERIFIED] |
| 1.2.2.5.4 | Delete Test Action | Delete | Action | Removes the selected test from the patient. | [VERIFIED] |
| 1.2.2.5.5 | All Tests Action | All | Action | Clears all of the patient's tests; shown when the patient is registered for the first time. | [VERIFIED] |
| 1.2.2.5.6 | Patient's Tests List | Patient's tests | List | The tests registered for the current patient. | [VERIFIED] |
| 1.2.2.5.6.1 | Patient's Tests Count | — | Indicator | The number of tests in the patient's tests list, carried in the panel title. | [VERIFIED] |
| 1.2.2.5.7 | Test Group List | — | List | The laboratory's predefined test groups, from which a whole group can be added at once. | [VERIFIED] |
| 1.2.2.5.8 | Add All Tests In Group Action | Add all tests in group | Action | Adds every test of the selected group to the patient; the group is added by pressing it twice. | [VERIFIED] |
| 1.2.2.5.9 | Group Send E-mail Action | Send E-mail | Action | Sends the selected group by e-mail. | [VERIFIED] |
| 1.2.2.5.10 | Sample Type Radio Group | — | Field | The sample type applied to the registered tests; the options are shown as a row of radio buttons. | [VERIFIED] |
| 1.2.2.5.10.1 | Urine Option | Urine | Option | Applies the urine sample type. | [VERIFIED] |
| 1.2.2.5.10.2 | Stool Option | Stool | Option | Applies the stool sample type. | [VERIFIED] |
| 1.2.2.5.10.3 | Blood Option | Blood | Option | Applies the blood sample type. | [VERIFIED] |
| 1.2.2.5.10.4 | Semen Option | Semen | Option | Applies the semen sample type. | [VERIFIED] |
| 1.2.2.5.10.5 | CSF Option | CSF | Option | Applies the cerebrospinal-fluid sample type. | [VERIFIED] |
| 1.2.2.5.11 | Taken Outside Lab Flag | Taken outside lab | Field | Marks the sample as collected outside the laboratory; the mark then appears as a note in the patient's test report. | [VERIFIED] |
| 1.2.2.6 | Action Band | — | Block | The strip of actions that closes the add-and-edit screen. | [VERIFIED] |
| 1.2.2.6.1 | Save Registration Action | حفظ | Action | Persists the registration. | [VERIFIED] |
| 1.2.2.6.2 | Add Entry Action | إضافة | Action | Adds the current entry. | [VERIFIED] |
| 1.2.2.6.3 | Edit Entry Action | تعديل | Action | Opens the entry for editing. | [VERIFIED] |
| 1.2.2.6.4 | Undo Action | تراجع | Action | Steps back out of the current operation. | [VERIFIED] |
| 1.2.2.6.5 | Print Barcode Action | الباركود | Action | Prints the patient's barcode labels from the add-and-edit patient data window. The labels produced are the specimen-container labels, the file barcode and the laboratory-identifier barcode; their kinds, their content and the rules that distribute the tests over the containers are defined in group 1.14. | [VERIFIED] |
| 1.2.2.6.6 | Print Receipt Action | الإيصال | Action | Prints the patient's receipt, carrying the patient identity, the ordered test lines with their amounts, the totals, the discount, the net invoice, the paid amount and the remainder, and a closing line. | [VERIFIED] |
| 1.2.2.6.7 | Patient Invoice Action | فاتورة المرضى | Action | Prints the patient invoice. | [VERIFIED] |
| 1.2.2.6.8 | Patient Envelope Print Action | طباعة ظرف | Action | Prints the result envelope from the patient screen. | [VERIFIED] |
| 1.2.2.6.9 | Modify Action | تعديل | Action | Opens the patient's data for modification. | [VERIFIED] |
| 1.2.2.6.10 | Follow-up Action | — | Action | Opens the case follow-up screen. | [VERIFIED] |
| 1.2.2.6.11 | Work Sheet Action | ورقة العمل | Action | Opens the work sheet for the patient. It refers to the work sheet module. | [VERIFIED] |
| 1.2.2.6.12 | ID Card Action | الكارنيه | Action | Opens the patient's identification card. The resulting document is not shown. | [NOT-SHOWN] |
| 1.2.2.6.13 | Patient Screen Action | الشاشة الرئيسية | Action | Returns to the main patient screen. | [VERIFIED] |
| 1.2.2.6.14 | Main Point Action | — | Action | Returns to the program's main point. | [VERIFIED] |
| 1.2.2.6.15 | Patient Window Action | نافذة المرضى | Action | Opens the patient list from the add-and-edit screen. | [VERIFIED] |
| 1.2.2.6.16 | Lab Results Action | نتائج التحاليل | Action | Opens the laboratory results from the add-and-edit screen. | [VERIFIED] |

### 1.3 — Patient Account

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.3.1 | Account Panel | — | Screen region | The block on the patient screen that holds the account figures and the settlement actions. | [VERIFIED] |
| 1.3.1.1 | Total of Tests Field | اجمالى التحاليل | Field | The total value of the patient's registered tests, computed from the prices entered in the program. | [VERIFIED] |
| 1.3.1.2 | Discount Value Field | قيمة الخصم | Field | The discount applied to the patient's account as an absolute amount. | [VERIFIED] |
| 1.3.1.3 | Discount Percentage Field | % | Field | The discount applied to the patient's account as a percentage. | [VERIFIED] |
| 1.3.1.4 | Paid Amount Field | المدفوع | Field | The amount paid by the patient; entered by the user. | [VERIFIED] |
| 1.3.1.4.1 | Surcharge Entry Convention | — | Rule | An additional amount that is not linked to a test — an extra service or an additional cost — is entered in the paid field preceded by a `+` sign, and is then added to the total of the tests. | [VERIFIED] |
| 1.3.1.4.2 | Enter Key Account Operation | — | Rule | The first `Enter` press performs the accounting operation for the patient; the second press saves the operation. | [VERIFIED] |
| 1.3.1.5 | Paid Previously Field | المدفوع سابقا | Field | The amount already paid on the account. | [VERIFIED] |
| 1.3.1.6 | Amount After Discount Field | التكلفة بعد الخصم | Field | The account total after the discount is applied. | [VERIFIED] |
| 1.3.1.7 | Total Collected Field | — | Field | The total amount collected on the account. | [VERIFIED] |
| 1.3.1.8 | Remainder for Customer Field | الباقى للمريض | Field | The outstanding amount owed by the customer. | [VERIFIED] |
| 1.3.1.9 | Remainder for Laboratory Field | الباقى للمعمل | Field | The amount retained by the laboratory. | [VERIFIED] |
| 1.3.1.10 | Confirm Payment Action | موافق | Action | Confirms the entered payment. | [VERIFIED] |
| 1.3.1.11 | Net Settlement Action | خالص | Action | Closes the account when the patient has paid the amount in full; the settlement is then confirmed in the following dialog. | [VERIFIED] |
| 1.3.1.12 | Account Summary Action | حساب المريض | Action | Opens the patient-account screen for the selected patient. | [VERIFIED] |
| 1.3.1.13 | Custody Action | حوافظ | Action | Opens the custody holders of the patient. Its exact behaviour is not established. | [UNVERIFIED] |
| 1.3.1.14 | Account Back Action | تراجع | Action | Steps back out of the account operation. | [VERIFIED] |
| 1.3.2 | Patient Account Screen | حساب المريض | Screen | The screen that lists the transactions of the patient's account and the patient's tests, and on which a paid amount is corrected. | [VERIFIED] |
| 1.3.2.1 | Account Transaction Grid | — | Grid | The grid of transactions made on the patient's account. | [VERIFIED] |
| 1.3.2.1.1 | Paid Date Column | Paid date | Grid column | The date on which the transaction was paid. | [VERIFIED] |
| 1.3.2.1.2 | Money Column | Money | Grid column | The amount of the transaction. | [VERIFIED] |
| 1.3.2.1.3 | User Name Column | User name | Grid column | The user who made the transaction. | [VERIFIED] |
| 1.3.2.1.4 | Edit Date Column | Edit date | Grid column | The date on which the transaction was last edited. | [VERIFIED] |
| 1.3.2.1.5 | Color Code Column | Color code | Grid column | A narrow column of the account grid whose header names a colour classification. The colours it carries are not described here. | [VERIFIED] |
| 1.3.2.2 | Modify Amount Action | تعديل | Action | Edits an amount already entered on the account: the amount is selected and the modify action is pressed. | [VERIFIED] |
| 1.3.2.3 | Delete Transaction Action | حذف | Action | Deletes a transaction from the account: the transaction is selected and the delete action is pressed. | [VERIFIED] |
| 1.3.2.4 | Pay Action | دفع | Action | Records a payment against the account. | [VERIFIED] |
| 1.3.2.5 | Refund Action | استرداد | Action | Records a refund against the account. | [VERIFIED] |
| 1.3.2.6 | Add Amount Action | اضافة | Action | Adds a further amount against the account. | [VERIFIED] |
| 1.3.2.7 | Clear Account Action | تصفير الحساب | Action | Clears the account of the patient. | [VERIFIED] |
| 1.3.2.8 | Account Patient List Action | قائمة المرضى | Action | Returns to the patient list. | [VERIFIED] |

### 1.4 — Results Delivery and Account Settlement

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.4.1 | Deliver Patient Results Screen | — | Screen | Used when the patient has settled part of the account and a balance remains with the laboratory; the balance is viewed at the moment the results are delivered. | [VERIFIED] |
| 1.4.1.1 | Delivery Filter Block | — | Block | The filters that narrow the delivery list. | [VERIFIED] |
| 1.4.1.1.1 | Delivery Date Filter | — | Field | The period selector of the delivery list. | [VERIFIED] |
| 1.4.1.1.2 | Delivery Name Filter | — | Field | The patient-name selector of the delivery list. | [VERIFIED] |
| 1.4.1.1.3 | Delivery Status Filter | — | Field | The delivery-status selector of the delivery list; the states are delivered, reviewed and printed. | [VERIFIED] |
| 1.4.1.1.4 | Delivery Department Filter | — | Field | The department selector of the delivery list. | [VERIFIED] |
| 1.4.1.2 | Patient Scope Block | — | Block | The controls that decide which patients the delivery list covers. | [VERIFIED] |
| 1.4.1.2.1 | Patient Source Selector | Individual / LabToLab / Contracts | Field | Selects the source of the patients listed. | [VERIFIED] |
| 1.4.1.2.2 | Patient Group Selector | VIP / Free / All | Field | Selects the group of the patients listed. | [VERIFIED] |
| 1.4.1.2.3 | Patients Today Panel | عدد المرضى اليوم | Panel | Lists the patients registered that day, with their count. | [VERIFIED] |
| 1.4.1.2.3.1 | Delivery Patient Grid | — | Grid | The grid of patients, with the columns code, name, type, laboratory identifier, referring entity and age. | [VERIFIED] |
| 1.4.1.3 | Delivery Result Block | — | Block | The grid of patients and tests that the delivery list is built from. | [VERIFIED] |
| 1.4.1.3.1 | Delivery Result List Grid | — | Grid | Lists the patients and their tests, with a per-test check box in each of the finish, verify, print and export columns. The grid is what the delivery list prints or previews. | [VERIFIED] |
| 1.4.1.3.1.1 | Test Abbreviation Column | Test Abbreviation | Grid column | Shows the test's abbreviation. | [VERIFIED] |
| 1.4.1.3.1.2 | P Column | P | Grid column | A narrow column of the results grid whose header is the single letter P. Its content is not established. | [UNVERIFIED] |
| 1.4.1.3.1.3 | T Column | T | Grid column | A narrow column of the results grid whose header is the single letter T. Its content is not established. | [UNVERIFIED] |
| 1.4.1.3.1.4 | Result Column | Result | Grid column | Shows the entered result. | [VERIFIED] |
| 1.4.1.3.1.5 | Status Column | Status | Grid column | Shows the status of the test. | [VERIFIED] |
| 1.4.1.3.1.6 | Delivery Finish Column | Finish | Grid column | The per-test check box marking the test as finished. | [VERIFIED] |
| 1.4.1.3.1.7 | Delivery Verify Column | Verify | Grid column | The per-test check box marking the test as verified. | [VERIFIED] |
| 1.4.1.3.1.8 | Delivery Print Column | Print | Grid column | The per-test check box marking the test as printed. | [VERIFIED] |
| 1.4.1.3.1.9 | Delivery Export Column | Export | Grid column | The per-test check box marking the test as exported. | [VERIFIED] |
| 1.4.1.3.1.10 | See Report Action | See Report | Action | Opens the report of the selected test from within the results grid. | [VERIFIED] |
| 1.4.1.4 | Delivery Action Block | — | Block | The actions available on the delivery screen. | [VERIFIED] |
| 1.4.1.4.1 | Patient Account Action | — | Action | Computes and displays the selected patient's account, showing the remainder for the patient or the remainder for the laboratory. | [VERIFIED] |
| 1.4.1.4.2 | Refresh Action | Refresh | Action | Reloads the delivery list. | [VERIFIED] |
| 1.4.1.4.3 | Delivery List Print Action | طباعة | Action | Prints the delivery list. | [VERIFIED] |
| 1.4.1.4.4 | Preview Action | معاينة | Action | Shows the delivery list before printing. | [VERIFIED] |
| 1.4.1.4.5 | Copy Action | — | Action | Copies the current delivery entry. | [VERIFIED] |
| 1.4.1.4.6 | Notes Action | ملاحظات | Action | Opens the notes attached to the delivery entry. | [VERIFIED] |
| 1.4.1.4.7 | Delivery Patient Data Action | بيانات المريض | Action | Opens the patient's data from the delivery screen. | [VERIFIED] |
| 1.4.1.4.8 | Combined Report Action | تقرير مجمع | Action | Opens the combined report. It refers to the combined report group of this module. | [VERIFIED] |
| 1.4.1.4.9 | Patient Medical History Action | التاريخ المرضي | Action | Opens the patient's medical history. | [VERIFIED] |
| 1.4.1.4.10 | Delivery Empty Report Action | تقرير فارغ | Action | Opens the empty report. It refers to the empty report group of this module. | [VERIFIED] |
| 1.4.1.4.11 | Delivery Envelope Print Action | طباعة ظرف | Action | Prints the result envelope from the delivery screen. | [VERIFIED] |
| 1.4.1.4.12 | Send SMS Action | SMS | Action | Sends the result by SMS. | [VERIFIED] |
| 1.4.1.4.13 | Delivery Main List Action | القائمة الرئيسية | Action | Returns to the main list from the delivery screen. | [VERIFIED] |

### 1.5 — Patient and Test Modification

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.5.1 | Modification Path One | الطريقة الأولى | Workflow | Modify a previously entered patient, or add or remove a test, through the add-and-edit patient data screen: press the patient icon, press the add-and-edit patient data action, press the patient-list action, choose the registration day, select the patient, press modify, make the change, then press save. | [VERIFIED] |
| 1.5.1.1 | Patient Icon Action | Patient | Action | Opens the patient-side working point from which the add-and-edit screen is reached. | [VERIFIED] |
| 1.5.1.2 | Patient List Open Action | قائمة المرضى | Action | Opens the patient list from which the registration day is chosen. | [VERIFIED] |
| 1.5.1.3 | Registration Day Selector | — | Field | Chooses the day on which the patient was registered, within the patient list. | [VERIFIED] |
| 1.5.1.4 | Edit Patient Action | تعديل | Action | Opens the patient's record for editing. | [VERIFIED] |
| 1.5.1.5 | Patient Selection Action | — | Action | Selects the patient to be modified. | [VERIFIED] |
| 1.5.1.6 | Modification Save Action | حفظ | Action | Persists the modification; the same action as the save of the registration. | [VERIFIED] |
| 1.5.2 | Modification Path Two | — | Workflow | Modify a patient by finding the patient through patient search and then entering the patient's data, which is displayed in detail. | [VERIFIED] |
| 1.5.2.1 | Search Action | — | Action | Opens the patient-search screen. | [VERIFIED] |
| 1.5.2.2 | Patient Data Entry Action | — | Action | Enters the patient's data, which is then displayed in detail. | [VERIFIED] |
| 1.5.3 | Patient Data Access Action | — | Action | Reaches the patient's data from the result-entry screen. | [VERIFIED] |

### 1.6 — Test Result Entry

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.6.1 | Result Entry Screen | — | Screen | Where the results of a patient's tests are entered and the report is printed. | [VERIFIED] |
| 1.6.1.1 | Patient List Panel | عدد المرضى اليوم | Panel | Lists the patients registered that day with their count; a patient is opened by clicking the name once, whereupon that patient's tests appear. | [VERIFIED] |
| 1.6.1.1.1 | Result Entry Patient Grid | — | Grid | The grid of patients registered that day, with the columns code, name, type, laboratory identifier, referring entity and age. | [VERIFIED] |
| 1.6.1.1.2 | Patient Name Click Action | — | Action | Opens the selected patient and shows that patient's tests. | [VERIFIED] |
| 1.6.1.1.3 | Patient Count Indicator | — | Indicator | The number of patients in the patient list. | [VERIFIED] |
| 1.6.1.2 | Test Results Grid | — | Grid | The grid of the patient's tests, in which the results are entered. | [VERIFIED] |
| 1.6.1.2.1 | Result Entry Test Abbreviation Column | Test Abbreviation | Grid column | Shows the test's abbreviation. | [VERIFIED] |
| 1.6.1.2.2 | Result Entry P Column | P | Grid column | A narrow column of the result-entry grid whose header is the single letter P. Its content is not established. | [UNVERIFIED] |
| 1.6.1.2.3 | Result Entry T Column | T | Grid column | A narrow column of the result-entry grid whose header is the single letter T. Its content is not established. | [UNVERIFIED] |
| 1.6.1.2.4 | Result Entry Result Column | Result | Grid column | Shows the entered result. | [VERIFIED] |
| 1.6.1.2.5 | Result Entry Status Column | Status | Grid column | Shows the status of the test. | [VERIFIED] |
| 1.6.1.2.6 | Abnormal Flag Column | — | Grid column | Shows the abnormal flag of the result; the flag states are Low, High and Critical. | [VERIFIED] |
| 1.6.1.2.7 | Low and High Column | — | Grid column | Shows the low and high band of the result. | [VERIFIED] |
| 1.6.1.2.8 | Unit Column | — | Grid column | Shows the unit of the result. | [VERIFIED] |
| 1.6.1.2.9 | Normal Range Column | Range | Grid column | Shows the test's reference range. | [VERIFIED] |
| 1.6.1.2.10 | Finish Flag | Finish | Field | Per-test check box marking the test as finished. | [VERIFIED] |
| 1.6.1.2.11 | Verify Flag | Verify | Field | Per-test check box marking the test as verified. | [VERIFIED] |
| 1.6.1.2.12 | Result Print Flag | Print | Field | Per-test check box marking the test as printed. | [VERIFIED] |
| 1.6.1.2.13 | Export Flag | Export | Field | Per-test check box marking the test as exported. | [VERIFIED] |
| 1.6.1.2.14 | Per-Test Comment Field | — | Field | A comment recorded against an individual test result; the comment appears in the report. | [VERIFIED] |
| 1.6.1.2.15 | Double-Click Test Action | See Report | Action | Opens the report of the selected test so that its data can be entered. | [VERIFIED] |
| 1.6.1.3 | Entry Action Block | — | Block | The actions that close the result-entry screen. | [VERIFIED] |
| 1.6.1.3.1 | Save Results Action | — | Action | Persists the entered results. | [VERIFIED] |
| 1.6.1.3.2 | Print Preview Action | معاينة | Action | Shows the report preview before printing. | [VERIFIED] |
| 1.6.1.3.3 | Result Report Print Action | طباعة | Action | Prints the result report, carrying the report titles, the per-test result with its unit and reference range, the abnormal flag, the patient identity block and the doctor's signature line. | [VERIFIED] |
| 1.6.1.4 | Report Settings Screen | — | Screen | The dialog that controls how the result report is rendered. | [VERIFIED] |
| 1.6.1.4.1 | Report Title Block | — | Block | The text elements of the report header. | [VERIFIED] |
| 1.6.1.4.1.1 | Report Screen Title Field | — | Field | Sets the title shown on the report screen. | [VERIFIED] |
| 1.6.1.4.1.2 | Report Main Title Field | — | Field | Sets the report's main title. | [VERIFIED] |
| 1.6.1.4.1.3 | Report Sub Title Field | — | Field | Sets the report's sub title. | [VERIFIED] |
| 1.6.1.4.1.4 | Report Sub Sub-title Field | — | Field | Sets the report's sub sub-title. | [VERIFIED] |
| 1.6.1.4.1.5 | Reference Range Field | — | Field | Sets the reference-range text of the report. | [VERIFIED] |
| 1.6.1.4.1.6 | Report Comment Field | — | Field | Sets the report's comment text. | [VERIFIED] |
| 1.6.1.4.1.7 | Report Normal Range Field | Normal Range | Field | Sets the normal-range text of the report, shown beside the reference range. | [VERIFIED] |
| 1.6.1.4.1.8 | Main Symbol Field | Main Symbol | Field | Sets the symbol used for the main element of the report. | [VERIFIED] |
| 1.6.1.4.1.9 | Sub Symbol Field | Sub Symbol | Field | Sets the symbol used for the sub-elements of the report. | [VERIFIED] |
| 1.6.1.4.1.10 | Sub-title Symbol Field | Sub-title Symb | Field | Sets the symbol used for the sub-title element of the report. | [VERIFIED] |
| 1.6.1.4.2 | Report Display Option Block | — | Block | The options that decide what the report shows. | [VERIFIED] |
| 1.6.1.4.2.1 | Show Abnormal Background Option | — | Field | Controls the background shading of abnormal results. | [VERIFIED] |
| 1.6.1.4.2.2 | Print Status Flag Option | — | Field | Controls printing of the low, high and critical status flag. | [VERIFIED] |
| 1.6.1.4.2.3 | Print Patient History Automatically Option | — | Field | Controls whether the patient history is printed automatically; a change made in this dialog applies to that report only. | [VERIFIED] |
| 1.6.1.4.3 | Report Typography Block | — | Block | The typographic controls of the report. | [VERIFIED] |
| 1.6.1.4.3.1 | Symbol Font Selectors | — | Field group | Select the fonts used for the report's symbol elements. | [VERIFIED] |
| 1.6.1.4.3.2 | Vertical Spacing Fields | — | Field group | Space after the sub title, space between texts, and additional space to the left of the range. | [VERIFIED] |
| 1.6.1.4.3.3 | Normal Result Font Selector | Normal result font | Field | Selects the font of the normal result in the report. | [VERIFIED] |
| 1.6.1.4.3.4 | Text Alignment Control | محاذاة النص من اليسار | Field | Controls the alignment of the report text from the left. | [VERIFIED] |
| 1.6.1.4.3.5 | Cell Expansion Control | عرض الخانة لتسع المحتوى | Field | Controls the expansion of a cell so that it fits its content. | [VERIFIED] |
| 1.6.1.4.3.6 | Offset Amount Control | مقدار الإزاحة | Field | Sets the offset amount applied to the report element. | [VERIFIED] |
| 1.6.1.4.4 | Report Output Block | — | Block | The controls that decide where the report goes and how it leaves the program. | [VERIFIED] |
| 1.6.1.4.4.1 | Printing Target Selector | — | Field | Selects the printer the report is sent to. | [VERIFIED] |
| 1.6.1.4.4.2 | Save Settings Action | — | Action | Persists the report settings. | [VERIFIED] |
| 1.6.1.4.4.3 | Export Data Action | — | Action | Exports the report data. | [VERIFIED] |
| 1.6.1.4.4.4 | Close Action | — | Action | Closes the report settings. | [VERIFIED] |
| 1.6.1.5 | Report Entry Grid | Test Name \| Result \| Unit \| L or H \| Normal Ranges \| Verified \| Print | Grid | The grid in which a test's result is entered, as shown on the test's own report screen. Its columns are the test name, the result, the unit, the low-or-high flag, the normal ranges, and the verified and print check boxes. | [VERIFIED] |
| 1.6.1.5.1 | Report Entry Test Name Column | Test Name | Grid column | Shows the name of the test or of the sub-test being entered. | [VERIFIED] |
| 1.6.1.5.2 | Report Entry Result Column | Result | Grid column | Shows the entered result. | [VERIFIED] |
| 1.6.1.5.3 | Report Entry Unit Column | Unit | Grid column | Shows the unit of the result. | [VERIFIED] |
| 1.6.1.5.4 | Report Entry Flag Column | L or H | Grid column | Shows whether the result is below or above the normal range. | [VERIFIED] |
| 1.6.1.5.5 | Report Entry Normal Ranges Column | Normal Ranges | Grid column | Shows the normal range of the test, with the patient's own range where the test defines one. | [VERIFIED] |
| 1.6.1.5.6 | Report Entry Verified Flag | Verified | Field | Check box marking the entered result as verified. | [VERIFIED] |
| 1.6.1.5.7 | Report Entry Print Flag | Print | Field | Check box marking the entered result for printing. | [VERIFIED] |
| 1.6.1.5.8 | Report Entry Comment Field | Comment | Field | A comment recorded against the entered result. | [VERIFIED] |
| 1.6.1.5.9 | Urine Sample Sub-Row Panel | عرض عينة البول في التقرير المجمع | Panel | A panel of extra rows shown under certain tests, carrying one row per sample and one sub-result per row; a sub-result that has no corresponding sample is marked as having no sample. | [VERIFIED] |
| 1.6.1.5.9.1 | Urine Sample Sub-Row | Urine Sample | Field | The sample the extra row refers to. | [VERIFIED] |
| 1.6.1.5.9.2 | Sub-Result Row | Glucose / Acetone | Field group | The sub-results recorded for the sample of that row. | [VERIFIED] |
| 1.6.1.5.9.3 | Missing Sample Marker | No Sample | Indicator | Marks a sub-result for which no sample is available. | [VERIFIED] |
| 1.6.1.6 | Test Entry Variants | — | Block | The entry of a test takes different shapes according to the kind of test: a single test carries one result, a profile test carries several sub-tests, and some sub-tests carry a control value together with the patient's value. | [VERIFIED] |
| 1.6.1.6.1 | Single Test Entry | — | Variant | A simple test is entered as one row carrying one result, one unit and one normal range. | [VERIFIED] |
| 1.6.1.6.2 | Profile Test Entry | — | Variant | A profile test is entered as a set of sub-test rows, each with its own result, unit and normal range, under the name of the profile. | [VERIFIED] |
| 1.6.1.6.3 | Paired Control And Patient Entry | — | Variant | A sub-test of a profile may be entered as a pair: a control value and the patient's value, each with its own unit. | [VERIFIED] |
| 1.6.1.6.4 | Derived Value Entry | — | Variant | A value may be derived from other values of the same report, as an international normalised ratio derived from the control time and the patient's prothrombin time, or a ratio between two enzyme results. | [VERIFIED] |
| 1.6.1.6.5 | Multi-Unit Entry | — | Variant | A single sub-test may carry more than one unit, as a bleeding time carried in minutes and in seconds. | [VERIFIED] |
| 1.6.1.6.6 | Qualitative Result Entry | — | Variant | A result may be qualitative rather than numeric, as a value that exceeds a limit, or a result recorded as absent. | [VERIFIED] |
| 1.6.1.7 | Report Screen Actions | — | Block | The actions carried by the test's own report screen. | [VERIFIED] |
| 1.6.1.7.1 | Save Report Action | حفظ | Action | Persists the entered result. | [VERIFIED] |
| 1.6.1.7.2 | Report Screen Print Action | طباعة | Action | Prints the test report. | [VERIFIED] |
| 1.6.1.7.3 | Report Screen Preview Action | معاينة الطباعة | Action | Shows the test report before printing. | [VERIFIED] |
| 1.6.1.7.4 | Report Patient History Action | Patient History | Action | Opens the patient's earlier tests, so that a previous result can be inserted into the report. | [VERIFIED] |
| 1.6.1.7.5 | Report Screen Back Action | رجوع | Action | Steps back out of the test report. | [VERIFIED] |
| 1.6.1.7.6 | Report Screen Main List Action | القائمة الرئيسية | Action | Returns to the main list. | [VERIFIED] |

### 1.7 — Combined Report

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.7.1 | Combined Report Screen | تقرير مجمع | Screen | Holds a group of test results selected from the patient's own tests; the combined report is the printed output of this screen. | [VERIFIED] |
| 1.7.1.1 | Patient Tests In Our Lab List | Patient tests in our lab | List | The patient's tests; the source of the combined report. | [VERIFIED] |
| 1.7.1.2 | Available Tests In Report List | Available tests in report | List | The tests already included in the combined report. | [VERIFIED] |
| 1.7.1.3 | Add Test To Report Action | — | Action | Adds the selected test to the combined report, either by pressing it twice or by selecting it and pressing the arrow control. | [VERIFIED] |
| 1.7.1.4 | Reorder Report Tests Action | — | Action | Moves the selected test up or down to change the order of the report. | [VERIFIED] |
| 1.7.1.4.1 | Move Test Up Action | — | Action | Moves the selected test one position earlier in the report. | [VERIFIED] |
| 1.7.1.4.2 | Move Test Down Action | — | Action | Moves the selected test one position later in the report. | [VERIFIED] |
| 1.7.1.5 | Combined Report Option Block | — | Block | The print options of the combined report. | [VERIFIED] |
| 1.7.1.5.1 | Print Sub Title Option | — | Field | Prints the group name as the report's sub title. | [VERIFIED] |
| 1.7.1.5.2 | Print Printed Test Again Option | — | Field | Prints an already printed test again without a message. | [VERIFIED] |

### 1.8 — Empty Report

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.8.1 | Empty Report Screen | تقرير فارغ | Screen | A report carrying the patient's data only. | [VERIFIED] |
| 1.8.1.1 | Empty Report Open Action | تقرير فارغ | Action | Opens an empty report for the selected patient. | [VERIFIED] |
| 1.8.1.2 | Enter Data And Print Option | — | Option | Data may be entered into the empty report and printed. | [VERIFIED] |
| 1.8.1.3 | Patient Data Sheet Option | — | Option | A sheet containing only the patient's data may be printed from the empty report. | [VERIFIED] |

### 1.9 — Culture Result Entry

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.9.1 | Culture Screen | — | Screen | Where culture and sensitivity results are entered. | [VERIFIED] |
| 1.9.1.1 | Culture Data Block | — | Block | The fields that identify what was cultured. | [VERIFIED] |
| 1.9.1.1.1 | Sample Field | Sample | Field | The sample type cultured. | [VERIFIED] |
| 1.9.1.1.2 | Organism A Field | Organism A | Field | The first organism identified. | [VERIFIED] |
| 1.9.1.1.3 | Organism B Field | Organism B | Field | The second organism identified. | [VERIFIED] |
| 1.9.1.1.4 | Organism C Field | Organism C | Field | The third organism identified. | [VERIFIED] |
| 1.9.1.1.5 | Culture Condition Field | Culture Condition | Field | The conditions under which the sample was cultured. | [VERIFIED] |
| 1.9.1.1.6 | Colony Count Field | Colony Count | Field | The colony count of the culture. | [VERIFIED] |
| 1.9.1.2 | Microscopy Block | — | Block | The fields of the microscopic examination. | [VERIFIED] |
| 1.9.1.2.1 | Reaction Field | Reaction | Field | The reaction of the sample. | [VERIFIED] |
| 1.9.1.2.2 | Pus Cells Field | Pus Cells | Field | The pus-cell finding. | [VERIFIED] |
| 1.9.1.2.3 | Red Blood Cells Field | R.B.Cs | Field | The red-blood-cell finding. | [VERIFIED] |
| 1.9.1.2.4 | Epithelial Cells Field | Epithelial Cells | Field | The epithelial-cell finding. | [VERIFIED] |
| 1.9.1.2.5 | Crystal Field | Crystal | Field | The crystal finding. | [VERIFIED] |
| 1.9.1.2.6 | Fungi Field | Fungi | Field | The fungi finding. | [VERIFIED] |
| 1.9.1.2.7 | Bacteria Field | — | Field | The bacteria finding; shown as disabled in the inspected state. | [VERIFIED] |
| 1.9.1.2.8 | Additional Microscopy Fields | Others | Field group | Further microscopy rows presented as repeated `Others` entries. | [VERIFIED] |
| 1.9.1.2.9 | Direct Culture Fields | Direct1 / Direct2 | Field group | The `Direct1` and `Direct2` entries. | [VERIFIED] |
| 1.9.1.2.10 | Microscopy Complete Flag | Complete | Field | Check box marking the microscopy entry as complete. | [VERIFIED] |
| 1.9.1.3 | Culture Flag Block | — | Block | The per-test check boxes of the culture entry. | [VERIFIED] |
| 1.9.1.3.1 | Culture Verified Flag | Verified | Field | Check box marking the culture entry as verified. | [VERIFIED] |
| 1.9.1.3.2 | Culture Print Flag | Print | Field | Check box marking the culture entry for printing. | [VERIFIED] |
| 1.9.1.4 | Sensitivity Block | Sensitivity | Block | The block in which the antibiotics and their sensitivity are recorded. | [VERIFIED] |
| 1.9.1.4.1 | Antibiotic List | — | List | The antibiotics available for the culture. | [VERIFIED] |
| 1.9.1.4.2 | Antibiotic Add Action | Add | Action | Adds the selected antibiotic to the culture, either by pressing it twice in the list or by selecting it and pressing add. | [VERIFIED] |
| 1.9.1.4.3 | Sensitivity Classification | — | Field group | Each antibiotic is classified under one of the four classes below. | [VERIFIED] |
| 1.9.1.4.3.1 | Highly For Option | Highly For | Option | Classifies the antibiotic as highly effective. | [VERIFIED] |
| 1.9.1.4.3.2 | Moderate For Option | Moderate For | Option | Classifies the antibiotic as moderately effective. | [VERIFIED] |
| 1.9.1.4.3.3 | Low For Option | Low For | Option | Classifies the antibiotic as weakly effective. | [VERIFIED] |
| 1.9.1.4.3.4 | Resistant For Option | Resistant For | Option | Classifies the antibiotic as resisted. | [VERIFIED] |
| 1.9.1.4.4 | Report Element Visibility Control | — | Field | Controls whether the sensitivity, the reference and the commercial name appear in the report. | [VERIFIED] |
| 1.9.1.5 | Culture Action Block | — | Block | The actions of the culture screen. | [VERIFIED] |
| 1.9.1.5.1 | Sensitivity Entry Action | — | Action | Opens the sensitivity entry. | [VERIFIED] |
| 1.9.1.5.2 | Save Culture Data Action | — | Action | Persists the culture data. | [VERIFIED] |
| 1.9.1.6 | Microbiology Report Output | MICROBIOLOGY REPORT | Output | The printed microbiology report, carrying the culture and sensitivity table, the microscopic examination table with its reference ranges, the culture block and the sensitivity blocks for Highly For, Moderate For and Low For, each with a sensitive-inhibition-zone column and a commercial-name column. | [VERIFIED] |

### 1.10 — Patient Search

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.10.1 | Search Screen | — | Screen | Opened from the find-patient action or from the search icon of the ribbon. | [VERIFIED] |
| 1.10.1.1 | Search Criteria Block | — | Block | The criteria by which a patient may be found; each criterion has its own entry field. | [VERIFIED] |
| 1.10.1.1.1 | Search by Patient Name Exact | بحث باسم المريض (مطابق | Function | Finds a patient by an exactly matching name. | [VERIFIED] |
| 1.10.1.1.2 | Search by Patient Name Partial | بحث باسم المريض (عشوائي | Function | Finds a patient by a partially matching name. | [VERIFIED] |
| 1.10.1.1.3 | Search by Treating Doctor | بحث باسم الطبيب المعالج | Function | Finds patients by the name of the treating doctor. | [VERIFIED] |
| 1.10.1.1.4 | Search by Request Date | بحث بتاريخ الطلب | Function | Finds patients by their request date. | [VERIFIED] |
| 1.10.1.1.5 | Search by Test Within Period | بالتحليل في الفترة المحددة | Function | Finds patients who received a given test within a given period. | [VERIFIED] |
| 1.10.1.1.6 | Search by Phone Number | بالتليفون | Function | Finds a patient by telephone number. | [VERIFIED] |
| 1.10.1.1.7 | Search by Card Number | برقم البطاقة | Function | Finds a patient by card number. | [VERIFIED] |
| 1.10.1.1.8 | Search by Patient Age | بحث بسن المريض | Function | Finds patients by their age. | [VERIFIED] |
| 1.10.1.1.9 | Search by Patient Gender | بحث بجنس المريض | Function | Finds patients by their gender. | [VERIFIED] |
| 1.10.1.1.10 | Search Within Branch | Branch No. | Function | Finds patients of one branch, by choosing the branch number; applicable in the branch deployment. | [VERIFIED] |
| 1.10.1.1.11 | Search for Results Not Entered | نتائج لم تدخل | Function | Finds results that have not been recorded. | [VERIFIED] |
| 1.10.1.1.12 | Search for Results Not Verified | نتائج لم تراجع | Function | Finds results that have not been reviewed. | [VERIFIED] |
| 1.10.1.1.13 | Search for Results Not Printed | نتائج لم تطبع | Function | Finds results that have not been printed. | [VERIFIED] |
| 1.10.1.1.14 | Search for Results Not Delivered | نتائج لم تسلم | Function | Finds results that have not been delivered. | [VERIFIED] |
| 1.10.1.1.15 | Date Range Mode Options | مقيد / حر | Field group | Two mutually exclusive options that decide whether the from and to dates of a dated criterion are bounded or free. | [VERIFIED] |
| 1.10.1.2 | Search Scope Block | — | Block | The controls that decide the scope and the source of the search. | [VERIFIED] |
| 1.10.1.2.1 | Branch Selector | All | Field | Selects the branch; All is the option shown. | [VERIFIED] |
| 1.10.1.2.2 | Source Selector | Database / Backup | Field | Selects the searched source, either the live data or the backup. | [VERIFIED] |
| 1.10.1.3 | Search Result Block | — | Block | The result of the search and the controls that act on it. | [VERIFIED] |
| 1.10.1.3.1 | Search Result Grid | — | Grid | Columns referred by, unit, age, gender, date, patient name, laboratory identifier and identifier. | [VERIFIED] |
| 1.10.1.3.2 | Search Result Count | عدد مرضى عملية البحث | Indicator | The number of patients the search returned. | [VERIFIED] |
| 1.10.1.3.3 | New Search Action | New search | Action | Starts a new search. | [VERIFIED] |
| 1.10.1.3.4 | Enter ID Field | <<Enter ID>> | Field | Where an identifier is typed to start a search. | [VERIFIED] |
| 1.10.1.4 | Search Action Bar | — | Field group | The group of actions available on the search screen. | [VERIFIED] |
| 1.10.1.4.1 | Patient Tests Action | تحاليل المريض | Action | Opens the patient's tests from the search result. | [VERIFIED] |
| 1.10.1.4.2 | Search Patient Data Action | بيانات المريض | Action | Opens the patient's data from the search result. | [VERIFIED] |
| 1.10.1.4.3 | Search Lab Results Action | نتائج التحاليل | Action | Opens the patient's laboratory results from the search result. | [VERIFIED] |
| 1.10.1.4.4 | Delete Patient Action | حذف المريض | Action | Deletes the selected patient. | [VERIFIED] |
| 1.10.1.4.5 | Open Account Action | حساب مفتوح | Action | Opens the account of the selected patient. | [VERIFIED] |
| 1.10.1.4.6 | Restrict To VIP Action | VIP | Action | Restricts the search result to the VIP group. Its exact behaviour is not established. | [UNVERIFIED] |
| 1.10.1.4.7 | Add Action | اضافة | Action | Adds a patient to the current selection. | [VERIFIED] |
| 1.10.1.4.8 | Collected Results Action | نتائج مجمعة | Action | Opens the collected results of the selected patient. Its exact behaviour is not established. | [UNVERIFIED] |
| 1.10.1.4.9 | Modify Group Action | — | Action | Modifies the selected group. Its exact behaviour is not established. | [UNVERIFIED] |
| 1.10.1.4.10 | Search Main List Action | القائمة الرئيسية | Action | Returns to the main list from the search screen. | [VERIFIED] |

### 1.11 — Patient Visit History

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.11.1 | Laboratory ID Lifecycle | — | Block | The life of the laboratory identifier that ties the visits of one patient together. | [VERIFIED] |
| 1.11.1.1 | Laboratory ID Generation | — | Function | A laboratory identifier is created for the patient when the patient's data is registered, the tests are entered and the registration is saved. | [VERIFIED] |
| 1.11.1.2 | Laboratory ID Storage | — | Function | The identifier is kept as a distinguished patient data item. | [VERIFIED] |
| 1.11.1.3 | Laboratory ID Display at Next Registration | — | Function | The identifier is shown automatically at the next registration of the same patient. | [VERIFIED] |
| 1.11.2 | Visit History | — | Block | The listing of the visits of one patient. | [VERIFIED] |
| 1.11.2.1 | Search by Laboratory ID | — | Function | Searching by the identifier returns all of the patient's visits to the laboratory with their dates. | [VERIFIED] |
| 1.11.2.2 | Visit List Grid | — | Grid | Columns referred by, unit, age, gender, date, patient name, laboratory identifier and identifier. | [VERIFIED] |
| 1.11.2.3 | Visit Tests List | — | List | The tests performed in a selected visit. | [VERIFIED] |

### 1.12 — Case Follow-up and Audit Trail

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.12.1 | Case History Screen | — | Screen | Shows the recorded events of the case, with the user name and the date and time of each; the case audit trail is the recorded output of this screen. | [VERIFIED] |
| 1.12.1.1 | Registration Audit Entry | تم تسجيل المريض بواسطة | Field | Who registered the case, with the date and time. | [VERIFIED] |
| 1.12.1.2 | Modification Count | عدد مرات التعديل | Field | How many times the case was modified. | [VERIFIED] |
| 1.12.1.3 | Last Modifier Entry | اخر تعديل تم بواسطة | Field | Who made the last modification, with the date and time. | [VERIFIED] |
| 1.12.1.4 | Result Entry Audit Entry | سجل نتيجة التحليل | Field | Who entered the test results, with the date and time. | [VERIFIED] |
| 1.12.1.5 | Verification Audit Entry | تمت المراجعة بواسطة | Field | Who verified the results, with the date and time. | [VERIFIED] |
| 1.12.1.6 | Printing Count | عدد مرات الطباعة | Field | How many times the report was printed. | [VERIFIED] |
| 1.12.1.7 | Last Printer Entry | اخر طباعة تمت بواسطة | Field | Who printed the report last, with the date and time. | [VERIFIED] |
| 1.12.1.8 | Delivery Audit Entry | تم تسليم النتيجة بواسطة | Field | Who delivered the result, with the date and time. | [VERIFIED] |
| 1.12.1.9 | Patient Samples Block | عينات المريض | Block | The block that lists the samples taken from the patient. | [VERIFIED] |
| 1.12.1.9.1 | Tube Name Column | Tube Name | Grid column | The name of the sample tube. | [VERIFIED] |
| 1.12.1.9.2 | Sample Tests Column | Tests | Grid column | The tests to be performed on the sample. | [VERIFIED] |
| 1.12.1.9.3 | Sample Take Action | Take | Action | Records the taking of the sample. | [VERIFIED] |

### 1.13 — Site Result Delivery

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.13.1 | Upload Screen | — | Screen | Lists the patient's tests with their results, the patient's number and code, the upload action and the progress line. | [VERIFIED] |
| 1.13.1.1 | Upload Patient Tests List | — | List | The patient's tests with their results, as listed for upload. | [VERIFIED] |
| 1.13.1.2 | Upload Action | — | Action | Transfers the selected patient's result to the site, where it becomes visible to the patient, the doctor or the laboratory. | [VERIFIED] |
| 1.13.1.3 | Select All Action | — | Action | Selects every listed test. | [VERIFIED] |
| 1.13.1.4 | Unselect All Action | — | Action | Clears the selection. | [VERIFIED] |
| 1.13.1.5 | Upload Progress Indicator | — | Field | Reports that the upload is in progress and how many items it covers. | [VERIFIED] |
| 1.13.1.6 | Block Result Action | — | Action | Removes the selected patient's result from the site, where it is no longer available. | [VERIFIED] |
| 1.13.1.7 | Mobile Number Requirement | — | Rule | The patient's telephone number must be entered before a result can be uploaded to the site. | [VERIFIED] |
| 1.13.2 | Uploaded Patient Count | — | Function | Shows how many patients had results uploaded within a period, and the total number of patients. | [VERIFIED] |
| 1.13.3 | Site Access Channels | — | Function | The site is reachable through the web page, through the mobile application published in the application store, and through the direct link given by the program. | [VERIFIED] |
| 1.13.4 | Site Account Types | — | Field group | The account types offered by the site. | [VERIFIED] |
| 1.13.4.1 | Patient Account Type | — | Option | The account of a patient. | [VERIFIED] |
| 1.13.4.2 | Doctor Account Type | — | Option | The account of a doctor. | [VERIFIED] |
| 1.13.4.3 | Laboratory Account Type | — | Option | The account of a laboratory. | [VERIFIED] |
| 1.13.5 | Site Sign-In Block | — | Block | How each kind of user signs in to the site. | [VERIFIED] |
| 1.13.5.1 | Patient Sign-In Data | — | Rule | A patient signs in with the laboratory code and the telephone number, or with the password alone. | [VERIFIED] |
| 1.13.5.2 | Doctor Or Laboratory Sign-In Data | — | Rule | A doctor or a laboratory signs in with the laboratory code, the laboratory identifier and the password. | [VERIFIED] |
| 1.13.5.3 | Site Sign In Action | — | Action | Signs the user in to the site. | [VERIFIED] |
| 1.13.6 | Site Result View | — | Block | What the signed-in user sees of the result and how it leaves the site. | [VERIFIED] |
| 1.13.6.1 | Result View Action | — | Action | Opens the signed-in user's own result, grouped by test group, with a per-test report action. | [VERIFIED] |
| 1.13.6.2 | Result Export Formats | — | Function | The result may be printed or exported. | [VERIFIED] |
| 1.13.6.2.1 | Print Format | — | Format | The result is printed. | [VERIFIED] |
| 1.13.6.2.2 | Word Document Format | — | Format | The result is exported as a word document. | [VERIFIED] |
| 1.13.6.2.3 | PDF Format | — | Format | The result is exported as a PDF. | [VERIFIED] |
| 1.13.6.2.4 | Image Format | — | Format | The result is exported as an image. | [VERIFIED] |
| 1.13.7 | Site Access Data On Receipt | — | Output | The site address, the telephone number and the access code printed on the patient's receipt, so that the patient can collect the result from the site. The same data is carried by the site-access label, which is defined in group 1.14. | [VERIFIED] |

### 1.14 — Barcode Labels and Specimen Containers

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.14.1 | Barcode Label Kinds | — | Block | The kinds of barcode label the program produces for a patient. | [VERIFIED] |
| 1.14.1.1 | Specimen Container Label | ملصقات الانابيب والعبوات والمسحات | Output | A label for the tube, container or swab. One label is printed for each specimen container, carrying the tests allocated to that container. The stated sticker size is 38 mm wide by 25 mm long. | [VERIFIED] |
| 1.14.1.2 | File Barcode Label | File barcode | Output | The label affixed to the patient's file, used in the case where results are delivered by barcode. | [VERIFIED] |
| 1.14.1.3 | Laboratory ID Barcode Label | Lab ID barcode | Output | The label affixed to the laboratory card. It carries the laboratory code, so that the medical history is easy to follow. | [VERIFIED] |
| 1.14.1.4 | Site Access Label | NATIGH.com | Output | The label by which the patient collects the result from the external service. It carries the service address, the patient's telephone number and the access password, with a barcode above the address. | [VERIFIED] |
| 1.14.2 | Specimen Container Label Content | — | Block | The elements printed on a specimen-container label. | [VERIFIED] |
| 1.14.2.1 | Patient Name on Label | — | Field | The patient's name, printed at the top of the label. | [VERIFIED] |
| 1.14.2.2 | Sex and Age Line on Label | — | Field | The patient's sex and age, printed as a single line under the name. | [VERIFIED] |
| 1.14.2.3 | Allocated Test List on Label | — | Field | The abbreviations of the tests allocated to that container, printed as a list. | [VERIFIED] |
| 1.14.2.4 | Label Barcode | — | Field | The barcode printed on the label of the container. | [VERIFIED] |
| 1.14.2.5 | Laboratory Identifier on Label | — | Field | The laboratory identifier of the visit, printed as a number under the barcode. | [VERIFIED] |
| 1.14.2.6 | Container Designation on Label | — | Field | The designation of the container the sample is held in, printed after the laboratory identifier. | [VERIFIED] |
| 1.14.2.7 | File Barcode Number | — | Field | The number printed under the file barcode and the laboratory-identifier barcode, composed of a leading digit, the laboratory code and a sequence number. Its exact composition is not established. | [UNVERIFIED] |
| 1.14.2.8 | Label Print Timestamp | — | Field | The date and time the label was printed, printed under the barcode of the file and laboratory-identifier labels. | [VERIFIED] |
| 1.14.3 | Specimen Container Designations | — | Block | The container designations the specimen-container labels carry. | [VERIFIED] |
| 1.14.3.1 | Serum Fasting Container | Serum Fasting | Option | The container for the tests performed on a fasting serum sample. | [VERIFIED] |
| 1.14.3.2 | Serum Postprandial Container | Serum Postprandial | Option | The container for the tests performed on a serum sample taken after a meal. | [VERIFIED] |
| 1.14.3.3 | EDTA Blood Container | EDTA Blood | Option | The container for the tests performed on an EDTA blood sample. | [VERIFIED] |
| 1.14.3.4 | Citrated Blood Container | Citrated Blood 0.4 | Option | The container for the tests performed on a citrated blood sample. | [VERIFIED] |
| 1.14.3.5 | Urine Container | Urine | Option | The container for the tests performed on a urine sample. | [VERIFIED] |
| 1.14.3.6 | Stool Container | Stool | Option | The container for the tests performed on a stool sample. | [VERIFIED] |
| 1.14.3.7 | Semen Container | Semen | Option | The container for the tests performed on a semen sample. | [VERIFIED] |
| 1.14.4 | Test Distribution Across Containers | — | Block | How the patient's tests are allocated over the materials and containers, and how the printed labels follow from that. | [VERIFIED] |
| 1.14.4.1 | Automatic Test Sorting | — | Function | The program sorts the patient's tests by the material and the container they are performed on. | [VERIFIED] |
| 1.14.4.2 | Container Distribution Edit | — | Function | The doctor can modify the distribution of the tests over the materials drawn from the patient and over the containers. | [VERIFIED] |
| 1.14.4.3 | Vertical Coding on Printed Documents | — | Function | The vertical coding appears on the result reports, the receipt and the envelope; its appearance there is optional. | [VERIFIED] |
| 1.14.4.4 | Repeated Sticker Printing | — | Function | The sticker can be printed more than once. | [VERIFIED] |

### 1.15 — Patient Medical History and History Report

| ID | Canonical name | Source label | Type | Definition | Tag |
|---|---|---|---|---|---|
| 1.15.1 | Automatic History Insertion | — | Block | When the patient has visited the laboratory before, the program can bring the results of the earlier tests into the current report. | [VERIFIED] |
| 1.15.1.1 | Automatic Previous Result Insertion | — | Function | Where the patient has already had the same test performed, the program inserts the earlier result into the result report automatically. | [VERIFIED] |
| 1.15.1.2 | Multi-Test History Insertion | — | Function | The history of several tests may be inserted into a result report alongside a group of tests from another group, or into a combined report. | [VERIFIED] |
| 1.15.1.3 | History Patient Identification | — | Block | How the program recognises the patient when it builds the history. | [VERIFIED] |
| 1.15.1.3.1 | Identify By Patient Name | فرز وتجميع التاريخ المرضي باسم المريض | Option | The patient is recognised for the history by the patient's name. | [VERIFIED] |
| 1.15.1.3.2 | Identify By Laboratory Code | فرز وتجميع التاريخ المرضي حسب كود المعمل | Option | The patient is recognised for the history by the laboratory code. | [VERIFIED] |
| 1.15.1.4 | Automatic History Display Setting | Print Patient History Automatically | Field | In the report settings, controls whether the patient's history is inserted into the report automatically; turning it off stops the automatic display. | [VERIFIED] |
| 1.15.1.5 | History Sorting Method Setting | طريقة فرز وتجميع التاريخ المرضي | Field | In the report settings, chooses how the patient's history is sorted and grouped. | [VERIFIED] |
| 1.15.2 | History Test Selection | — | Block | Choosing which of the patient's earlier results are brought into the report. | [VERIFIED] |
| 1.15.2.1 | Previous Visit Test List | — | List | The tests the patient had in the previous visits to the laboratory. | [VERIFIED] |
| 1.15.2.2 | Previous Test Selection Action | — | Action | A test is selected for the history by pressing it twice. | [VERIFIED] |
| 1.15.2.3 | Previous Visit Count Indicator | — | Indicator | Reports how many earlier visits the patient has; the report screen shows the number of the patient's history. | [VERIFIED] |
| 1.15.2.4 | Repeated Test Note | — | Indicator | Notes that the patient has had a given test more than once. | [VERIFIED] |
| 1.15.2.5 | Previous Result Grid | Result status \| Test result \| Analysis date | Grid | Lists the earlier results of the selected test, with the result status, the result value and the date of the analysis. | [VERIFIED] |
| 1.15.3 | History Output Mode | — | Block | The two ways the patient's history may be produced. | [VERIFIED] |
| 1.15.3.1 | Attach History In Result Report Option | Attach history in result report | Option | The selected earlier results are inserted into the result report itself. | [VERIFIED] |
| 1.15.3.2 | Print History In Separate Report Option | Print history in separate report | Option | The selected earlier results are produced as a report separate from the result report. | [VERIFIED] |
| 1.15.3.3 | History Mode Confirm Action | موافق | Action | Confirms the chosen history output mode. | [VERIFIED] |
| 1.15.3.4 | History Mode Cancel Action | الغاء الحالى | Action | Abandons the current history operation. | [VERIFIED] |
| 1.15.4 | Patient History Report | — | Block | The report that carries the patient's earlier results. | [VERIFIED] |
| 1.15.4.1 | History Report Header | Patient History | Field group | The header of the history report, carrying the report title, the period it covers, the user who printed it and the date it was printed. | [VERIFIED] |
| 1.15.4.1.1 | History Report Title | Patient History | Field | The title of the history report. | [VERIFIED] |
| 1.15.4.1.2 | History Date Range Field | History from ... to ... | Field | The period the history report covers. | [VERIFIED] |
| 1.15.4.1.3 | History Printed By Entry | Printed by | Field | The user who printed the history report. | [VERIFIED] |
| 1.15.4.1.4 | History Printed In Entry | Printed In | Field | The date the history report was printed. | [VERIFIED] |
| 1.15.4.2 | History Report Grid | Analysis Date \| Patient ID \| Patient Name | Grid | The grid of the history report: one row per analysis date, carrying the patient identity and one column per test whose history was selected. | [VERIFIED] |
| 1.15.4.2.1 | History Analysis Date Column | Analysis Date | Grid column | The date of the analysis the row reports. | [VERIFIED] |
| 1.15.4.2.2 | History Patient ID Column | Patient ID | Grid column | The laboratory identifier of the patient the row reports. | [VERIFIED] |
| 1.15.4.2.3 | History Patient Name Column | Patient Name | Grid column | The name of the patient the row reports. | [VERIFIED] |
| 1.15.4.2.4 | History Test Result Column | — | Grid column | One column per selected test, carrying that test's result on the row of its analysis date. | [VERIFIED] |
| 1.15.4.3 | Multi-Patient History Panel | No Of Patients | Panel | The panel used to build a history for more than one patient in the same report. | [VERIFIED] |
| 1.15.4.3.1 | Multi-Patient History Grid | Patient Name \| Patient ID \| Analysis Date | Grid | The grid of the patients the history is built for. | [VERIFIED] |
| 1.15.4.3.2 | Multi-Patient History Count | No Of Patients | Indicator | The number of patients the history covers. | [VERIFIED] |
| 1.15.4.4 | History Test Lists | — | Block | The two lists from which the tests to be given a history are chosen. | [VERIFIED] |
| 1.15.4.4.1 | History Tests In Lab List | Patient tests in our lab. | List | The tests available to be given a history, with their count. | [VERIFIED] |
| 1.15.4.4.2 | History Tests Available In Report List | Available tests in report | List | The tests already chosen to be given a history, with their count. | [VERIFIED] |
| 1.15.4.5 | Delete All History Action | حذف الكل | Action | Removes every selection from the history report. | [VERIFIED] |
| 1.15.4.6 | History Report Print Action | طباعة | Action | Prints the history report. | [VERIFIED] |
| 1.15.4.7 | History Report Preview Action | معاينة طباعة | Action | Shows the history report before printing. | [VERIFIED] |

## §1.3 Screens and Interface Structure

| Screen | Element identifiers |
|---|---|
| Startup Splash Screen | 1.1.3, 1.1.3.1, 1.1.3.1.1, 1.1.3.1.2, 1.1.3.2 |
| Sign-In Window | 1.1.4, 1.1.4.1, 1.1.4.2, 1.1.4.3, 1.1.4.4, 1.1.4.5 |
| Top Ribbon | 1.1.1, 1.1.1.1, 1.1.1.1.1, 1.1.1.1.2, 1.1.1.1.3, 1.1.1.1.4, 1.1.1.1.5, 1.1.1.1.6, 1.1.1.1.7, 1.1.1.1.8, 1.1.1.1.9, 1.1.1.1.10, 1.1.1.1.11, 1.1.1.1.12, 1.1.1.2 |
| Status Bar | 1.1.2, 1.1.2.1, 1.1.2.2, 1.1.2.3, 1.1.2.4 |
| Patient Search Toolbar | 1.1.5, 1.1.5.1, 1.1.5.2 |
| Main Patients Window | 1.2.1, 1.2.1.1, 1.2.1.2, 1.2.1.3, 1.2.1.4, 1.2.1.5, 1.2.1.6, 1.2.1.7, 1.2.1.8, 1.2.1.9, 1.2.1.10, 1.2.1.11 |
| Add-and-Edit Patient Data Screen — patient header | 1.2.2.1, 1.2.2.1.1, 1.2.2.1.2, 1.2.2.1.3, 1.2.2.1.4, 1.2.2.1.5 |
| Add-and-Edit Patient Data Screen — identity block | 1.2.2.2, 1.2.2.2.1, 1.2.2.2.2, 1.2.2.2.2.1, 1.2.2.2.2.2, 1.2.2.2.3, 1.2.2.2.4, 1.2.2.2.5, 1.2.2.2.6, 1.2.2.2.7, 1.2.2.2.8, 1.2.2.2.9, 1.2.2.2.10, 1.2.2.2.10.1, 1.2.2.2.11, 1.2.2.2.11.1 |
| Add-and-Edit Patient Data Screen — medical history block | 1.2.2.3, 1.2.2.3.1, 1.2.2.3.2, 1.2.2.3.3, 1.2.2.3.4, 1.2.2.3.5, 1.2.2.3.6, 1.2.2.3.7, 1.2.2.3.8, 1.2.2.3.9, 1.2.2.3.10, 1.2.2.3.11, 1.2.2.3.12, 1.2.2.3.13, 1.2.2.3.14, 1.2.2.3.15 |
| Add-and-Edit Patient Data Screen — notes and communication block | 1.2.2.4, 1.2.2.4.1, 1.2.2.4.2, 1.2.2.4.2.1, 1.2.2.4.3, 1.2.2.4.4, 1.2.2.4.5, 1.2.2.4.6, 1.2.2.4.7, 1.2.2.4.8 |
| Add-and-Edit Patient Data Screen — test selection block | 1.2.2.5, 1.2.2.5.1, 1.2.2.5.1.1, 1.2.2.5.1.2, 1.2.2.5.1.3, 1.2.2.5.1.4, 1.2.2.5.2, 1.2.2.5.3, 1.2.2.5.4, 1.2.2.5.5, 1.2.2.5.6, 1.2.2.5.6.1, 1.2.2.5.7, 1.2.2.5.8, 1.2.2.5.9, 1.2.2.5.10, 1.2.2.5.10.1, 1.2.2.5.10.2, 1.2.2.5.10.3, 1.2.2.5.10.4, 1.2.2.5.10.5, 1.2.2.5.11 |
| Add-and-Edit Patient Data Screen — action band | 1.2.2.6, 1.2.2.6.1, 1.2.2.6.2, 1.2.2.6.3, 1.2.2.6.4, 1.2.2.6.5, 1.2.2.6.6, 1.2.2.6.7, 1.2.2.6.8, 1.2.2.6.9, 1.2.2.6.10, 1.2.2.6.11, 1.2.2.6.12, 1.2.2.6.13, 1.2.2.6.14, 1.2.2.6.15, 1.2.2.6.16 |
| Account Panel | 1.3.1, 1.3.1.1, 1.3.1.2, 1.3.1.3, 1.3.1.4, 1.3.1.4.1, 1.3.1.4.2, 1.3.1.5, 1.3.1.6, 1.3.1.7, 1.3.1.8, 1.3.1.9, 1.3.1.10, 1.3.1.11, 1.3.1.12, 1.3.1.13, 1.3.1.14 |
| Patient Account Screen | 1.3.2, 1.3.2.1, 1.3.2.1.1, 1.3.2.1.2, 1.3.2.1.3, 1.3.2.1.4, 1.3.2.1.5, 1.3.2.2, 1.3.2.3, 1.3.2.4, 1.3.2.5, 1.3.2.6, 1.3.2.7, 1.3.2.8 |
| Deliver Patient Results Screen — filters | 1.4.1.1, 1.4.1.1.1, 1.4.1.1.2, 1.4.1.1.3, 1.4.1.1.4 |
| Deliver Patient Results Screen — patient scope | 1.4.1.2, 1.4.1.2.1, 1.4.1.2.2, 1.4.1.2.3, 1.4.1.2.3.1 |
| Deliver Patient Results Screen — results grid | 1.4.1.3, 1.4.1.3.1, 1.4.1.3.1.1, 1.4.1.3.1.2, 1.4.1.3.1.3, 1.4.1.3.1.4, 1.4.1.3.1.5, 1.4.1.3.1.6, 1.4.1.3.1.7, 1.4.1.3.1.8, 1.4.1.3.1.9, 1.4.1.3.1.10 |
| Deliver Patient Results Screen — actions | 1.4.1.4, 1.4.1.4.1, 1.4.1.4.2, 1.4.1.4.3, 1.4.1.4.4, 1.4.1.4.5, 1.4.1.4.6, 1.4.1.4.7, 1.4.1.4.8, 1.4.1.4.9, 1.4.1.4.10, 1.4.1.4.11, 1.4.1.4.12, 1.4.1.4.13 |
| Result Entry Screen — patient list | 1.6.1.1, 1.6.1.1.1, 1.6.1.1.2, 1.6.1.1.3 |
| Result Entry Screen — results grid | 1.6.1.2, 1.6.1.2.1, 1.6.1.2.2, 1.6.1.2.3, 1.6.1.2.4, 1.6.1.2.5, 1.6.1.2.6, 1.6.1.2.7, 1.6.1.2.8, 1.6.1.2.9, 1.6.1.2.10, 1.6.1.2.11, 1.6.1.2.12, 1.6.1.2.13, 1.6.1.2.14, 1.6.1.2.15 |
| Result Entry Screen — action block | 1.6.1.3, 1.6.1.3.1, 1.6.1.3.2, 1.6.1.3.3 |
| Report Settings Screen | 1.6.1.4, 1.6.1.4.1, 1.6.1.4.1.1, 1.6.1.4.1.2, 1.6.1.4.1.3, 1.6.1.4.1.4, 1.6.1.4.1.5, 1.6.1.4.1.6, 1.6.1.4.1.7, 1.6.1.4.1.8, 1.6.1.4.1.9, 1.6.1.4.1.10, 1.6.1.4.2, 1.6.1.4.2.1, 1.6.1.4.2.2, 1.6.1.4.2.3, 1.6.1.4.3, 1.6.1.4.3.1, 1.6.1.4.3.2, 1.6.1.4.3.3, 1.6.1.4.3.4, 1.6.1.4.3.5, 1.6.1.4.3.6, 1.6.1.4.4, 1.6.1.4.4.1, 1.6.1.4.4.2, 1.6.1.4.4.3, 1.6.1.4.4.4 |
| Combined Report Screen | 1.7.1, 1.7.1.1, 1.7.1.2, 1.7.1.3, 1.7.1.4, 1.7.1.4.1, 1.7.1.4.2, 1.7.1.5, 1.7.1.5.1, 1.7.1.5.2 |
| Empty Report Screen | 1.8.1, 1.8.1.1, 1.8.1.2, 1.8.1.3 |
| Culture Screen — culture data | 1.9.1.1, 1.9.1.1.1, 1.9.1.1.2, 1.9.1.1.3, 1.9.1.1.4, 1.9.1.1.5, 1.9.1.1.6 |
| Culture Screen — microscopy | 1.9.1.2, 1.9.1.2.1, 1.9.1.2.2, 1.9.1.2.3, 1.9.1.2.4, 1.9.1.2.5, 1.9.1.2.6, 1.9.1.2.7, 1.9.1.2.8, 1.9.1.2.9, 1.9.1.2.10 |
| Culture Screen — sensitivity | 1.9.1.4, 1.9.1.4.1, 1.9.1.4.2, 1.9.1.4.3, 1.9.1.4.3.1, 1.9.1.4.3.2, 1.9.1.4.3.3, 1.9.1.4.3.4, 1.9.1.4.4 |
| Search Screen — criteria | 1.10.1.1, 1.10.1.1.1, 1.10.1.1.2, 1.10.1.1.3, 1.10.1.1.4, 1.10.1.1.5, 1.10.1.1.6, 1.10.1.1.7, 1.10.1.1.8, 1.10.1.1.9, 1.10.1.1.10, 1.10.1.1.11, 1.10.1.1.12, 1.10.1.1.13, 1.10.1.1.14, 1.10.1.1.15 |
| Search Screen — scope | 1.10.1.2, 1.10.1.2.1, 1.10.1.2.2 |
| Search Screen — result | 1.10.1.3, 1.10.1.3.1, 1.10.1.3.2, 1.10.1.3.3, 1.10.1.3.4 |
| Search Screen — action bar | 1.10.1.4, 1.10.1.4.1, 1.10.1.4.2, 1.10.1.4.3, 1.10.1.4.4, 1.10.1.4.5, 1.10.1.4.6, 1.10.1.4.7, 1.10.1.4.8, 1.10.1.4.9, 1.10.1.4.10 |
| Case History Screen | 1.12.1, 1.12.1.1, 1.12.1.2, 1.12.1.3, 1.12.1.4, 1.12.1.5, 1.12.1.6, 1.12.1.7, 1.12.1.8, 1.12.1.9, 1.12.1.9.1, 1.12.1.9.2, 1.12.1.9.3 |
| Upload Screen | 1.13.1, 1.13.1.1, 1.13.1.2, 1.13.1.3, 1.13.1.4, 1.13.1.5, 1.13.1.6, 1.13.1.7 |
| Site Result View | 1.13.5, 1.13.5.1, 1.13.5.2, 1.13.5.3 |

## §1.4 Workflows


### Sign in

1. Start the program from the desktop; the startup splash screen is shown. → 1.1.3
2. Choose the data source, then wait for the start sequence to complete. → 1.1.3.1, 1.1.3.2
3. The sign-in window opens. → 1.1.4
4. Enter the user name and the password, optionally enabling the character display and the remembered sign-in. → 1.1.4.1, 1.1.4.2, 1.1.4.3, 1.1.4.4
5. Press sign in. → 1.1.4.5

### Register a patient

1. Press the patients ribbon item. → 1.1.1.1.12
2. On the main patients window, press the add-patient action. → 1.2.1.1
3. The add-and-edit patient data screen opens; the entry date, the receipt date and the patient code are shown in the patient header. → 1.2.2.1
4. Enter the patient's name, sex, age, sample type and address; then the telephone number and the card number; and optionally the treating doctor. → 1.2.2.2.1, 1.2.2.2.2, 1.2.2.2.3, 1.2.2.2.4, 1.2.2.2.5, 1.2.2.2.6, 1.2.2.2.7, 1.2.2.2.8, 1.2.2.2.9, 1.2.2.2.11
5. Answer the medical-history questions; each question is answered independently. → 1.2.2.3
6. If notes exist for the patient, enter them; they appear in the result-entry and result-delivery windows. → 1.2.2.4.1
7. Add the patient's tests from the master test list by pressing each test twice; it then appears in the patient's tests list. → 1.2.2.5.5, 1.2.2.5.6
8. To remove a duplicated or unwanted test, press it twice in the patient's tests list; it disappears from that list. → 1.2.2.5.4
9. To clear all of the patient's tests, press the all-tests action. → 1.2.2.5.5
10. To add a whole test group, press the group name twice; the whole group is added, after which a single test can still be removed. → 1.2.2.5.7, 1.2.2.5.8
11. Apply the sample type to the registered tests, and set the taken-outside flag if the sample was collected outside the laboratory; the flag then appears as a note in the patient's test report. → 1.2.2.5.9, 1.2.2.5.10
12. To print the patient's barcode labels, press the barcode action: the specimen-container labels, the file barcode and the laboratory-identifier barcode are produced. → 1.2.2.6.5, 1.14.1
13. Press save. → 1.2.2.6.1

### Compute and settle the patient account

1. The account figures appear in the account panel; the total of the tests is computed from the prices entered in the program. → 1.3.1.1
2. If a discount exists, enter its value or its percentage. → 1.3.1.2, 1.3.1.3
3. Enter the amount already paid by the patient in the paid field. → 1.3.1.4
4. Press `Enter`: the first press performs the accounting operation, the second press saves it. → 1.3.1.4.2
5. **Branch — the patient pays in full:** press the net-settlement action, then confirm in the dialog that follows. → 1.3.1.10
6. **Branch — an additional amount linked to a test:** enter the amount in the paid field prefixed with `+` and press the confirm-payment action; the amount is added to the total of the tests. → 1.3.1.4.1, 1.3.1.9
7. **Branch — correcting an amount:** press the account-summary action; on the patient-account screen select the amount and press the modify action, or press the delete action to remove the transaction. → 1.3.1.11, 1.3.2.5, 1.3.2.6

### Deliver results and settle a remaining balance

1. Press the deliver-results action. → 1.2.1.8
2. The delivery screen lists the patients and their finished and unfinished results, shown for the period selected in the program. → 1.4.1.3.1, 1.4.1.1.1
3. For the selected patient, press the patient-account action to compute and display the account, which shows the remainder for the patient or the remainder for the laboratory. → 1.4.1.4.1

### Modify a patient's data or tests

- **Path one:** press the patient icon, press the add-and-edit patient data action, press the patient-list action, choose the registration day, select the patient, press modify, make the change, then press save. → 1.5.1.1, 1.5.1.2, 1.5.1.3, 1.5.1.4, 1.5.1.5, 1.5.1.6
- **Path two:** find the patient through patient search, then enter the patient's data, which is displayed in detail. → 1.5.2.1, 1.5.2.2
- From the result-entry screen, the patient's data is reached through the patient-data action. → 1.5.3

### Enter results and print the report

1. Press the enter-results action to open the result-entry screen. → 1.2.1.2, 1.6.1
2. The patients registered that day are listed with their number. → 1.6.1.1.1, 1.6.1.1.3
3. Select the patient by clicking the name once; that patient's tests appear. → 1.6.1.1.2
4. Press the see-report action on the test to open its report, enter the data, and press save. → 1.6.1.2.13, 1.6.1.3.1
5. To preview the report before printing, press the print-preview action. → 1.6.1.3.2
6. To print, press the print action. → 1.6.1.3.3

### Enter a result, and the shape the entry takes

1. Open the test's report with the see-report action. → 1.6.1.2.15
2. Enter the result in the report entry grid; the grid carries the test name, the result, the unit, the low-or-high flag, the normal ranges and the verified and print check boxes. → 1.6.1.5
3. **The entry takes the shape of the test.** A simple test is one row. A profile test is a set of sub-test rows. A sub-test may be entered as a pair of a control value and the patient's value. A value may be derived from other values of the same report. A single sub-test may carry more than one unit. A result may be qualitative rather than numeric. → 1.6.1.6
4. Some tests carry an extra panel of rows, one row per sample, with a sub-result on each row; a sub-result with no sample available is marked as having no sample. → 1.6.1.5.9
5. Press save, then print or preview the report. → 1.6.1.7.1, 1.6.1.7.2, 1.6.1.7.3

### Produce a combined report

1. From the result-entry screen press the combined-report action; the combined report screen opens. → 1.7.1
2. The patient's tests appear in the patient's-tests list. → 1.7.1.1
3. Press the required test twice, or select it and press the arrow control; the added test appears in the available-tests list. → 1.7.1.3, 1.7.1.2
4. To change the order, select the test and move it up or down. → 1.7.1.4.1, 1.7.1.4.2

### Produce an empty report

1. Press the enter-results action to open the result-entry screen, then press the empty-report action. → 1.2.1.2, 1.8.1.1
2. Select the patient from the patient list. → 1.6.1.1.2
3. The empty report opens; data may be entered and printed, or a sheet carrying only the patient's data may be printed. → 1.8.1.2, 1.8.1.3

### Enter culture and sensitivity results

1. Press the culture action. → 1.9.1
2. Enter the culture data: sample, organism A, organism B, organism C, culture condition and colony count. → 1.9.1.1
3. After entering the sensitivity values, press the sensitivity action. → 1.9.1.5.1
4. Enter the antibiotics in the classification: press the antibiotic twice in the list, or select it and press add; classify it under one of the four classes. → 1.9.1.4.1, 1.9.1.4.2, 1.9.1.4.3
5. Control what the report shows by selecting the element to display. → 1.9.1.4.4
6. Press save. → 1.9.1.5.2

### Search for a patient

1. Press the find-patient action, or the search icon of the ribbon; the search screen opens. → 1.2.1.3, 1.10.1
2. Choose one of the search criteria, each with its own entry field. → 1.10.1.1
3. For a dated criterion, set the from and to dates and choose the bounded or free mode. → 1.10.1.1.15, 1.10.1.1.4, 1.10.1.1.5
4. For the branch criterion choose the branch number. → 1.10.1.1.10, 1.10.1.2.1
5. Optionally switch the source between the live data and the backup. → 1.10.1.2.2
6. The matching patients are listed in the search result grid, with their count. → 1.10.1.3.1, 1.10.1.3.2
7. From the action bar, the user can open the patient's tests, the patient's data, the laboratory results, the open account, or delete the patient. → 1.10.1.4.1, 1.10.1.4.2, 1.10.1.4.3, 1.10.1.4.5, 1.10.1.4.4

### List all visits of one patient

1. The laboratory identifier is created when the patient's data is registered, the tests are entered and the registration is saved. → 1.11.1.1
2. Press the laboratory-identifier action to display the identifier, then press save. → 1.2.2.1.5, 1.2.2.6.1
3. It is stored as a distinguished patient data item and is shown automatically at the next registration of the same patient. → 1.11.1.2, 1.11.1.3
4. Search by the laboratory identifier; all of the patient's visits with their dates are then displayed. → 1.11.2.1, 1.11.2.2
5. Selecting a visit shows that visit's tests. → 1.11.2.3

### Build the patient's history

1. Where the patient has had the same test before, the program inserts the earlier result into the result report automatically. → 1.15.1.1
2. The history of several tests may be inserted alongside a group of tests, or into a combined report. → 1.15.1.2
3. The program recognises the patient for the history by the patient's name or by the laboratory code, as set in the report settings. → 1.15.1.3
4. To choose the earlier results by hand, press the patient-history action on the report screen; the tests the patient had in earlier visits appear. → 1.6.1.7.4, 1.15.2.1
5. Select a test by pressing it twice; the earlier results of that test are listed with their result, their status and the date of the analysis. → 1.15.2.2, 1.15.2.5
6. Choose the output mode: insert the history into the result report, or print it as a report of its own, then confirm. → 1.15.3.1, 1.15.3.2, 1.15.3.3
7. To stop the history being inserted automatically, turn off the automatic-history setting in the report settings. → 1.15.1.5
8. The history report carries the period it covers, the user who printed it, the date it was printed, and one row per analysis date carrying the patient identity and one column per selected test. → 1.15.4
9. To build a history for more than one patient in the same report, open the patients icon, press the find-patient action, and search for the patients. → 1.2.1.3, 1.15.4.7

### Follow up a case

1. The user signs in and records the patient's case, the results and their printing, as shown before. → 1.1.4.5
2. The case history shows who registered the case and how many times it was modified, with the time and date; who entered the test results; who reviewed them; how many times the report was printed and who printed it last; and who delivered the result. → 1.12.1.1, 1.12.1.2, 1.12.1.3, 1.12.1.4, 1.12.1.5, 1.12.1.6, 1.12.1.7, 1.12.1.8
3. The patient's samples are listed with their tube name, their tests and the taking action. → 1.12.1.9

### Deliver results through the external service

1. Register the patient and enter the patient's telephone number in the dedicated field, then press the site-upload action. → 1.2.2.2.5, 1.2.2.4.7, 1.13.1.7
2. Register the patient's tests and enter the results as before. → 1.6.1.3.1
3. Open the result-upload screen and select the patient whose result is to be uploaded. → 1.13.1
4. Press upload. → 1.13.1.2
5. The upload screen shows the patient's tests with their results and the upload progress. → 1.13.1.1, 1.13.1.6
6. To see how many patients had results uploaded in a period, or the total number of patients, open the administrator view; set the period and press the patient-count action. → 1.13.2
7. To remove a patient's result from the site, select the patient and press the block action. → 1.13.1.7
8. **User side:** the patient, the treating doctor or the laboratory opens the web page, the mobile application, or the direct link given by the program, and chooses the account type. → 1.13.3, 1.13.4
9. A patient enters the laboratory code and the telephone number, or the password alone; a doctor or a laboratory enters the laboratory code, the laboratory identifier and the password. → 1.13.4.1, 1.13.4.2
10. Press sign in. → 1.13.4.3
11. The patient presses the result action to see their result. → 1.13.5.1
12. The result may be printed or exported as a word document, a PDF or an image. → 1.13.5.2
13. The site's access data may be printed on the patient's receipt so that the patient can collect the result from the site. → 1.13.6

## §1.5 Validation and Conditional Behavior

| ID | Rule | Tag |
|---|---|---|
| 1.2.2.2.2 | The recorded sex decides whether the pregnancy question is offered to the patient. | [VERIFIED] |
| 1.2.2.3.10 | The pregnancy question is asked of female patients only. | [VERIFIED] |
| 1.2.2.5.4 | The all-tests action is offered when the patient is registered for the first time. | [VERIFIED] |
| 1.2.2.5.9 | More than one sample type may be selected. | [VERIFIED] |
| 1.2.2.5.10 | The taken-outside flag takes effect once set, and then appears as a note in the patient's test report. | [VERIFIED] |
| 1.3.1.4.1 | An amount prefixed with `+` in the paid field is treated as an addition to the total of the tests; an unprefixed amount is the payment itself. | [VERIFIED] |
| 1.3.1.4.2 | The two `Enter` presses are not interchangeable: the first computes, the second saves. | [VERIFIED] |
| 1.3.1.10 | The net settlement applies when the patient has paid the amount in full, and is confirmed in the dialog that follows it. | [VERIFIED] |
| 1.4.1 | The patient account is shown on the delivery screen only in the case where the patient has settled part of the account and a balance remains with the laboratory. | [VERIFIED] |
| 1.5.1.3 | The registration day must be selected before the patient can be modified. | [VERIFIED] |
| 1.6.1.2.5 | A result is flagged abnormal when it falls outside the test's reference range; the flag states are Low, High and Critical. | [VERIFIED] |
| 1.6.1.4.2.3 | A change made in the report settings applies to that report only. | [VERIFIED] |
| 1.9.1.2.7 | The bacteria field is disabled in the inspected state and cannot be entered. | [VERIFIED] |
| 1.9.1.4.3 | Each antibiotic carries exactly one of the four sensitivity classes. | [VERIFIED] |
| 1.10.1.1.10 | The branch criterion applies in the branch deployment, where each user carries a branch number. | [VERIFIED] |
| 1.10.1.1.15 | A dated criterion is either bounded by a from and to date or free; the two modes are mutually exclusive. | [VERIFIED] |
| 1.11.1.1 | The laboratory identifier is created only after the patient's data is registered, the tests are entered and the registration is saved. | [VERIFIED] |
| 1.13.1.7 | The telephone number is a precondition of the upload. | [VERIFIED] |
| 1.13.4.1 | A patient signing in with the password alone does not supply the laboratory code or the telephone number. | [VERIFIED] |

## §1.6 Outputs and Side Effects

The artefacts below are produced by the elements listed. The content of each artefact is defined once, in the element's own definition, and is not repeated here.

| ID | Output | Tag |
|---|---|---|
| 1.2.2.6.5 | Barcode label set | [VERIFIED] |
| 1.2.2.6.6 | Patient receipt | [VERIFIED] |
| 1.2.2.6.7 | Patient invoice | [VERIFIED] |
| 1.2.2.6.8 | Result envelope | [VERIFIED] |
| 1.2.2.5.10 | Taken-outside note in the test report | [VERIFIED] |
| 1.4.1.4.3 | Delivery list | [VERIFIED] |
| 1.6.1.2.12 | Per-test comment in the result report | [VERIFIED] |
| 1.6.1.3.3 | Result report | [VERIFIED] |
| 1.7.1 | Combined report | [VERIFIED] |
| 1.8.1.3 | Patient data sheet | [VERIFIED] |
| 1.9.1.6 | Microbiology report | [VERIFIED] |
| 1.12.1 | Case audit trail | [VERIFIED] |
| 1.15.4 | Patient history report | [VERIFIED] |
| 1.13.1.2 | Uploaded result on the external service | [VERIFIED] |
| 1.13.1.6 | Result removed from the external service | [VERIFIED] |
| 1.13.6 | Site access data printed on the receipt | [VERIFIED] |

## §1.7 Uncertainties and Unshown Elements

| ID | Item | Tag |
|---|---|---|
| 1.1.5.2 | The meaning of the three quick-search mode codes is not established. | [UNVERIFIED] |
| 1.2.2.4.2 | The purpose of the site-reference field is not established. | [UNVERIFIED] |
| 1.2.2.5.1.2 | The meaning of the three test-filter mode codes is not established. | [UNVERIFIED] |
| 1.2.2.6.9 | The identification card action is present, but the card it produces is never shown, so its fields and content are not established. | [NOT-SHOWN] |
| 1.2.1.11 | The test-registration statistics action is present, but the screen it opens is never shown, so its criteria and output are not established. | [NOT-SHOWN] |
| 1.3.1.12 | The custody action is present on the account panel, but its behaviour is not established. | [UNVERIFIED] |
| 1.6.1.2.3 | The content of the narrow P column of the results grid is not established. | [UNVERIFIED] |
| 1.6.1.2.4 | The content of the narrow T column of the results grid is not established. | [UNVERIFIED] |
| 1.10.1.4.6 | The VIP action's behaviour is not established. | [UNVERIFIED] |
| 1.10.1.4.8 | The collected-results action's behaviour is not established. | [UNVERIFIED] |
| 1.10.1.4.9 | The modify-group action's behaviour is not established. | [UNVERIFIED] |
| 1.2.2.2.8 | The relation between the sample type carried in the identity block and the sample-type radio group of the test-selection block is not established. | [UNVERIFIED] |
| 1.6.1.2.6 | The relation between the abnormal-flag column and the low-and-high column of the result grid is not established. | [UNVERIFIED] |
| 1.3.2.5 | The colour classification carried by the account grid's narrow column is not described here, because the presentation describes function only. | [VERIFIED] |
| 1.6.1.2.2 | The content of the narrow P column of the delivery results grid is not established. | [UNVERIFIED] |
| 1.6.1.5.4 | The precise rule by which the low-or-high flag is set is not established, beyond the abnormal-flag states of Low, High and Critical. | [UNVERIFIED] |
| 1.15.4.4 | The history report's per-test column carries the result of that test on the row of its analysis date; how a test that was not performed on a given date is represented is not established. | [UNVERIFIED] |
| — | No validation, rejection message or recovery path is established for the patient screens, the account panel, the result entry, the culture entry, the search or the delivery screens. | [NOT-SHOWN] |
| — | The required or optional status of the patient master fields is established only for the telephone number, the address, the treating doctor and the notes, which the source describes as optional. | [UNVERIFIED] |
