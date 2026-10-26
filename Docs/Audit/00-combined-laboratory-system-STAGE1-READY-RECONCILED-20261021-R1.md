# Functional Reference — the reference laboratory system

The canonical functional reference of the laboratory system this document describes. It is the common baseline for the independent comparison of another implementation against this one, and it is written to be read by an auditor who has not taken part in producing it.

**Authority.** The content of this document rests on two source documents: a user guide, which is the primary authority, and a product showcase, which supplies the screens and the printed outputs that the guide describes in prose only. They are named here as *the guide* and as *the showcase*, and are cited by the page of the one they belong to; neither document is named by file. Every entry in the inventory carries an evidence status, and every entry that rests on a specific page carries the identifier of that page in the evidence register (§4). The source documents are not named in this document; they are referred to as *the guide* and as *the showcase*, and the pages are cited by their own number within each of them.

**What this document is not.** It is not a specification, not a design and not an implementation plan. It records what the source material shows and what it describes, and it separates clearly between what is established and what is not. It does not claim to be exhaustive, and §15 states the limits of what was inspected and what could not be established.

**How to use it.** §3 sets out the levels at which entries are recorded and the rule that decides between them. A future comparison should read a *function* as a behaviour that must be present or absent, a *subfunction* as a distinct operation inside a function, a *rule* as a constraint, and a *UI element* as a field, button, column or label that carries a function but is not itself a business behaviour. Counting the UI elements as functions inflates the inventory and misplaces the gaps. An entry whose evidence status is not *Verified* is not a gap: it is an open question, and §14 lists the open questions. An entry marked *OUT OF SCOPE FOR TOP-LAB PARITY* is documented because the source establishes it, but it is not a parity requirement and must not be counted as one.

## Contents

| § | Section |
|---|---|
| §1 | Purpose, authority and scope |
| §2 | Scope exclusions and the parity boundary |
| §3 | Methodology and functional-granularity policy |
| §4 | Evidence-status policy and evidence register |
| §5 | Identifier scheme, conventions and cross-references |
| §6 | Unit 1 — Patient registration and test registration |
| §7 | Unit 2 — Work sheet for patients or tests |
| §8 | Unit 3 — Test data and external dealings |
| §9 | Unit 4 — Users, roles and lab performance monitoring |
| §10 | Unit 5 — Statistics |
| §11 | Unit 6 — Inventory and lab accounting |
| §12 | Unit 7 — Basic system settings |
| §13 | Unit 8 — Laboratory sample withdrawal |
| §14 | Consolidated unresolved questions |
| §15 | Coverage and validation summary |
| §16 | Global identifier registry |

## §1 Purpose, authority and scope

**Purpose.** To hold one authoritative, machine-checkable description of the reference laboratory system's documented functionality, so that an independent comparison against a different implementation can be made against a fixed and traceable baseline rather than against recollection.

**In scope.** The whole of the user guide and the whole of the showcase, and nothing else. The guide runs to its final chapter on sample withdrawal; the showcase runs to the page that closes the accounts and tools sequence, and the pages beyond it are outside the material that was inspected.

**Units.** The system is described in eight functional units. The units follow the guide's own structure, and each of them keeps the identifier space of the module number that owns it. The unit of the laboratory's utility window is held in unit 1, beside the shell and the sign-in window that reach it, because the utilities are reached from the shell's ribbon and are not a workflow of their own; this is a placement decision and is recorded in the validation report.

**Out of the inventory.** Anything the source does not show or describe is not in the inventory, and its absence is not evidence that the system lacks it. Where a screen is named but never shown, the entry is recorded with the evidence status *Not shown* and is listed in the uncertainties of its unit, so that it is never confused with a function that the system does not have.

## §2 Scope exclusions and the parity boundary

Five categories of functionality are excluded from the parity scope of the implementation this baseline is written for. They are:

1. **Online portal ecosystem** — the external result service through which a patient or a doctor collects a result over the web.
2. **Equipment management** — the laboratory's devices and equipment.
3. **Analyser integration** — the connection of the system to an analysing instrument.
4. **Multi-branch support** — the deployment in which each user carries a branch number and the data are held per branch.
5. **External SMS, e-mail and fax delivery** — the sending of a result to the patient by short message, by e-mail or by fax.

**How the exclusions are applied.** An entry that belongs to one of these categories is **retained** in the inventory, with its scope stated, because the reference system documents it and deleting it would make the reference inaccurate. It is marked *OUT OF SCOPE FOR TOP-LAB PARITY* in the definition table and it is counted separately in §15. It must not be carried into an implementation-gap list.

**Where the boundary is not clean.** Two entries mix in-scope and excluded behaviour and are therefore stated explicitly rather than split, because the evidence does not establish the split: the *Site Upload Action* and the *Site Reference Field* of the patient screen, which belong to the portal ecosystem, and the *User Branch Selector* of the user screen, which sets the branch of a user in the multi-branch deployment but is also the field that records the branch of the patient. The *Inventory Branch Selector* and the *Inventory Branch Rule* are the same case: the account is computed for a branch in both deployments, but the branch itself only exists in the excluded one.

**What the exclusions do not reach.** The exclusions are applied to the five categories named and to nothing adjacent. A function is not excluded merely because it involves networking, an external party, a device or a second location, and the *samples sent outside* family, the *external entities and laboratories* family and the *contract price lists* family all stay in scope.

## §3 Methodology and functional-granularity policy

**How the inventory was produced.** The two source documents were read page by page. Pages that carry a screen, a form, a printed output or a statement of behaviour were read as rendered images; pages that carry only a heading, a divider or a body of prose were read through the document's own text layer and checked against the rendered page where the text was ambiguous. The text layer of the guide stores its Arabic in a broken order, so a literal match against it is meaningless; it was used to locate sections and to cross-check the presence of a label, and every substantive claim was confirmed against the page itself.

**The levels.** Six levels are used for the inventory, and the level of an entry is part of its identity. A seventh kind of record, the open question, is not an inventory level and is kept apart in §14.

| Level | What it is | What it is not | Example |
|---|---|---|---|
| **Function** | An independently identifiable behaviour that can be exercised and observed on its own: an action that does something, or a capability the source states. | Not a screen. A screen is a surface that carries behaviours; it is a container. | *Compute the stocktaking account for a chosen branch and period.* |
| **Subfunction** | A distinct selectable operation, a route into a screen, or a step that only navigates. | Not a behaviour in its own right. | *Print the samples sent outside for one laboratory.* |
| **Business rule** | A constraint, an acceptance condition, a default, or a calculation that governs a function. | Not an operation. | *A range of one to sixty days is not recognised for a patient of one month.* |
| **UI element** | A field, a button, a column, a label, an indicator, a grid or a list. | Not a business behaviour, however many of them a screen carries. | *The field that carries the date the period starts at.* |
| **Container** | A surface or a grouping that carries other entries: a screen, a panel, a block, a field group. | Not a function merely because it is where things happen. | *The result-entry screen.* |
| **Output** | An artefact the system produces. | Not the operation that produces it. | *The patients' account statement.* |

**The decision rule for a function.** An entry is a function when it is a materially distinct operation, business outcome, decision, validation, state transition or independently observable behaviour. A screen is **never** counted as a function by virtue of being a screen: the behaviours a screen supports are the functions, and the screen is the container that holds them. A screen that carries no child entry is the one exception, and it is recorded as a function in its own right, because it then describes a behaviour that is not decomposed anywhere.

**The rule against inflation.** A column is not a function. A list of check boxes is not a function. A row of a grid is not a function. An action whose only effect is to leave the current screen and return to the main list is a navigation step and is recorded as a subfunction, not as a behaviour. Where a parent has several materially distinct children, the parent is recorded **and** the children are recorded; the parent is never used in place of the children, and the children are never dropped to keep the inventory short.

**The rule against deflation.** Distinct behaviours that happen to sit on the same screen are never merged into one vague entry. The four ways of breaking a stocktaking account down by doctor are four options, not one.

**Cross-unit references.** A behaviour that belongs to one unit and is reached from another is defined once, in the unit that owns it, and referred to by its identifier from the other. The inventory is therefore a set: every element appears exactly once, and the count of elements is well defined.

**How the source labels were verified.** The third column of a definition table carries the label as it is written on the screen, and every label was put through a four-step test against the source text: an **exact** match of the label, whitespace for whitespace; a match after a **documented normalisation** of the Arabic letter variants and of the order in which the words of a line are stored; an **approximate** match, in which every character of the label is present on one line of the source but the order is not established; and **unverified**, in which the text layer does not carry the label at all. Only the first two classes establish the wording of a label. The third class does **not** establish exact correspondence, and nothing in this document rests on it: it is a screening test that shows where the text layer is broken, not a proof. The association of a label with the element it is recorded against is a separate matter and rests on the page that carries the element; the pages that were read as rendered images are listed in the evidence register of §4, and the entries added by the present audit carry labels that were read directly on those pages. A composite cell, which holds a set of column headers, is verified part by part rather than as a whole.

**How the levels are assigned.** The assignment is mechanical and reproducible: the level follows from the kind of the entry, with the single exception of the navigation-only action described above. The levels partition the inventory — every entry has exactly one level, and the counts of the levels add up to the total — so the totals in §15 cannot double count a parent and its children.

## §4 Evidence-status policy and evidence register

**The statuses.** Every entry carries exactly one status.

| Status | Meaning |
|---|---|
| **Verified** | Directly supported by readable source text or by clear visual evidence. |
| **Unverified** | A plausible interpretation that the available evidence does not conclusively establish. The entry may well be right, but the source does not prove it. |
| **Not shown** | The source names the element but never displays it. Its presence is established; its fields and its behaviour are not. This is **not** evidence that the function is missing, and it must never be counted as a gap. |
| **Sample** | An example value or an illustrative presentation. It is not a rule and not an exhaustive catalogue. |
| **Out of scope for Top-Lab parity** | The reference system documents the element, but the element belongs to one of the five excluded categories of §2. |

**Statuses are assigned at the level the evidence supports.** A function may be verified while one of its rules is unverified; the status of the parent does not spread to the child, and the child carries its own. Where a source shows a control but does not establish what it does, the control is recorded as a UI element and the behaviour is recorded as a separate entry with the status *Unverified* or *Not shown*.

**What would resolve an open question.** Each entry in §14 states what the source establishes, what it does not, and what evidence would settle it.

### §4.1 Evidence register

| Evidence ID | Source | Page | Screen or artefact | What it establishes |
|---|---|---|---|---|
| E-001 | Guide | p. 36 | Result-entry screen | Result grid columns, comment row buttons, account-type buttons, bottom action bar, and the 'foreign report' action and its rule. |
| E-002 | Showcase | p. 3 | Barcode labels page | Kinds of specimen-container label; the file, laboratory-identifier and site-access labels; the 38 mm x 25 mm sticker size; the sorting and distribution-edit banner. |
| E-003 | Showcase | p. 4 | Receipt printout | Receipt header, test/amount table, totals, results password, and the cash-printer and print-once banner. |
| E-004 | Showcase | p. 5 | Printed result report | Report summary header block and the test-group table with the count and the price of each group. |
| E-005 | Showcase | p. 6 | Delivery sheet | The sheet's header fields, the five test columns, the two signature roles and the per-patient order-slip banner. |
| E-006 | Showcase | p. 7 | Work paper by patient name | The printed work paper's per-patient block, the five test columns, the pagination and the work-list banner. |
| E-007 | Showcase | p. 8 | Work paper by patient names | The abbreviated variant with the OK column and the banner about printing for one day, half a day or less. |
| E-008 | Showcase | p. 9 | Work paper by test name | The per-test block, the patient/result/ID columns and the patients-per-test counter. |
| E-009 | Showcase | p. 11 | Test-count census | The test-name and count table of the sample census. |
| E-010 | Showcase | p. 25 | Result review report | The follow-up report of a test over a period and the choice of the lower and the upper value. |
| E-011 | Showcase | p. 26 | Patient account statement | The eight account columns, the totals row and the settled / not-settled banner. |
| E-012 | Showcase | p. 27 | Detailed individual statement | The statement that lists the test lines with their prices. |
| E-013 | Showcase | p. 28 | Referring-entity statement | The three printed variants and the banner naming the four reports and the claim. |
| E-014 | Showcase | p. 29 | Sent-samples statement | The eight printed columns of the samples sent outside and the per-sample or per-entity report levels. |
| E-015 | Showcase | p. 30 | Census by month | The bar chart, the census table columns and the totals row. |
| E-016 | Showcase | p. 31 | Census by day | The line chart with the average line, the census table columns and the pagination. |
| E-017 | Showcase | p. 33 | Test-sample census | The pie chart, the test/sample/percentage/statistic columns and the census banner. |
| E-018 | Showcase | p. 34 | Sign-in window | The user-name combo, the masked password, the two check boxes and the two buttons. |
| E-019 | Showcase | p. 35 | Main window | The ribbon and the four large actions of the patient-side entry point. |
| E-020 | Showcase | p. 36 | Patient registration screen | The test-selection panel, the patient header, the medical check boxes, the delivery block and the action band. |
| E-021 | Showcase | p. 37 | Result-entry screen | The patient header strip, the result grid, the comment row and the right-hand panel. |
| E-022 | Showcase | p. 43 | Test data screen | The test search bar, the ten-column test grid and the test-information form. |
| E-023 | Showcase | p. 44 | Reference values screen | The reference list grid, the Range-for selector, the limits, the flags, the pregnant-only mark, the comments and the actions. |
| E-024 | Showcase | p. 45 | Antibiotics screen | The international list, the antibiotic grid, the count and the per-commercial-name print mark. |
| E-025 | Showcase | p. 46 | Test groups screen | The group list, the count, the per-group totals and the group and test actions. |
| E-026 | Showcase | p. 48 | Statistics screen | The period filter, the referring-entity selector and the three groups of five census buttons. |
| E-027 | Showcase | p. 49 | Accounts window | The four account entries, including the sales and income entry. |
| E-028 | Showcase | p. 50 | Stocktaking window | The alternative labels of the account figures and the breakdown selectors. |
| E-029 | Showcase | p. 51 | Main tools window | The nine tool entries and the capability list of the window. |
| E-030 | Guide | p. 171 | Statistics report examples | The pie chart of the census reports and the census table columns. |
| E-031 | Guide | p. 184 | Referring-entity statement | The four report variants named in the guide text. |
| E-032 | Guide | p. 177-180 | Stocktaking screen | The fourteen account figures and the breakdown groups of the guide variant. |
| E-033 | Guide | p. 93-148 | Chapter 3 | Test data, reference values, contract prices, test comments, test groups, external entities, cultures, antibiotics, samples sent outside, result-service accounts. |
| E-034 | Guide | p. 166-173 | Chapter 5 | Statistics menu, the census screens and the four report types. |
| E-035 | Guide | p. 84-92 | Chapter 2 | The two work sheets, their filters and the four printed documents. |
| E-036 | Guide | p. 149-165 | Chapter 4 | Users, the fourteen permissions, the work times, the attendance record and the login detector. |
| E-037 | Guide | p. 190-207 | Chapter 7 | The system settings window, the report, receipt and envelope tabs, the dedicated printers and the default account type. |
| E-038 | Guide | p. 208-211 | Chapter 8 | The sample withdrawal screen, its grids, its actions, the calendar and the note. |
| E-039 | Showcase | p. 12 | Printed urinalysis report | The layout of a printed profile report: the patient block, the report title, the named sub-sections, the result and reference-range columns, the emphasis of a flagged result, the secondary mark, the signature and the closing line. |
| E-040 | Showcase | p. 13 | Printed microbiology report | The same profile-report layout for a stool examination, with a second mark cell on two rows. |
| E-041 | Showcase | p. 14 | Printed haematology report | The blood-picture report with the flag column, the nested differential table, the automatic comment, and the statement that the calculations and the differential are made without the doctor's intervention. |
| E-042 | Showcase | p. 17 | Printed endocrinology report | A profile report without the laboratory header band, a multi-line reference range, the paper sizes A4, A5 and B5, and the statement that the reference values of a single patient may be edited while printing and that the normal range accepts up to 255 characters. |
| E-043 | Showcase | p. 19 | Printed serology report | The interpretation block of the report and the statement that the side on which the Arabic name and its translation are printed may be chosen by the doctor and that an interpretation may be written for a test and applied to the whole patient. |
| E-044 | Showcase | p. 20 | Printed semen analysis report | The reference column headed as being according to a named authority, the motility table over the time points, the embedded drawing and the embedded chart, and the statement that the graph and the drawing may be withheld from the report. |
| E-045 | Showcase | p. 21 | Semen report, second page | The motility grading by grade, the dead-forms percentage, the breakdown of the abnormal forms, the morphology drawings, and the statement that the second page is left out when a value or an element is not entered. |
| E-046 | Showcase | p. 23 | Printed culture and sensitivity report | The urine examination block, the culture block with the organism, the condition and the colony count, and the three sensitivity tables with the antibiotic, the sensitivity value, the inhibition zone and the commercial names. |
| E-047 | Showcase | p. 24 | Printed image sheet | The sheet of microscope images, the number of images it may carry, the annotations placed on and beside the images, and the control of their position, size and colour. |
| E-048 | Showcase | p. 38 | Urine composite entry screen | The composite entry screen: the named sub-sections, the per-item check box, the per-item value list, the extra value and comparison control, the second result row of an item, the comment and the actions. |
| E-049 | Showcase | p. 39 | Profile entry screen | The profile entry grid, the per-patient range text shown in the grid, and the sub-tests of the profile that were not ordered, shown without a result. |
| E-050 | Showcase | p. 40 | Blood-picture entry screen | The calculated indices with their inclusion marks, the differential count with the relative and the absolute counts, the immature-cell rows, the automatic comment with its basis, and the constants panel. |
| E-051 | Showcase | p. 41 | Single-test entry screen | The single-test entry grid, the resolved range text of the patient, the urine-sample sub-block with its print mark, and the actions. |
| E-052 | Showcase | p. 42 | System module window | The sixteen entries of the system window and the four capabilities its own text states. |
| E-053 | Showcase | p. 47 | Statistics module window | The six entries of the statistics window and the four capabilities its own text states. |
| E-054 | Showcase | p. 10 | Hormones work paper | The work paper of a work group, with its header row, its mark column and the two signature roles at the foot. |
| E-055 | Showcase | p. 32 | Sample census of a test group | The census of the samples of a test group over a month, with its pie chart and its table. |

## §5 Identifier scheme, conventions and cross-references

**The identifier is the address of the element.** The first segment is the number of the unit that owns the element, the second is the functional group, and the rest is the containment: a screen, a block, an element, and an option. The depth follows the real containment and is never flattened: `g.s.c.b.e.o` for an option of a field of a block of a screen of a group of a unit. An element therefore has one address, and a reference by that address is unambiguous on its own.

**Stability.** An identifier, once issued, keeps its meaning for ever. No identifier is reused for a different element, and no identifier is renumbered. New material is appended: a new group takes the next free number in its unit (`1.16`, `1.17`, …), and a new child takes the next free number inside its parent. Where a correction changed the meaning of an identifier, the change is recorded in the validation report, and the old and the new identifiers are both given there.

**One element, one place.** Every element is defined exactly once, at the unit and the screen that carries it. Every other place refers to it by its identifier. The inventory is therefore a set, and the totals in §15 are well defined.

**The source-label column.** The third column of a definition table carries the label exactly as it is written on the screen, in its own script, so that an element can be matched against the running software. A dash means the source carries no readable label for the element. A cell that lists several labels is a set of labels of one grid or one field group, and each of its parts is separately defined.

**Cross-unit references** are made with the identifier of the element in the unit that owns it, because the first segment is the unit number. A reference made by name alone in the text is resolved into its identifier at the point where it is used.

**Definition of a column.** A definition says what the element does, not where it sits. Where a source establishes only that a control exists, the definition says so and the behaviour is not supplied.

## §6 Unit 1 — Patient Registration and Test Registration

### §6.1 Functional hierarchy

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
│       │   ├── 1.2.2.4.1  Patient Notes Field
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
│       │   ├── 1.2.2.5.4  Patient Test Delete Action
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
│           ├── 1.4.1.4.2  Refresh Delivery List Action
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
├── 1.15  Patient Medical History and History Report
│   ├── 1.15.1  Automatic History Insertion
│   │   ├── 1.15.1.1  Automatic Previous Result Insertion
│   │   ├── 1.15.1.2  Multi-Test History Insertion
│   │   ├── 1.15.1.3  History Patient Identification
│   │   │   ├── 1.15.1.3.1  Identify By Patient Name
│   │   │   └── 1.15.1.3.2  Identify By Laboratory Code
│   │   ├── 1.15.1.4  Automatic History Display Setting
│   │   └── 1.15.1.5  History Sorting Method Setting
│   ├── 1.15.2  History Test Selection
│   │   ├── 1.15.2.1  Previous Visit Test List
│   │   ├── 1.15.2.2  Previous Test Selection Action
│   │   ├── 1.15.2.3  Previous Visit Count Indicator
│   │   ├── 1.15.2.4  Repeated Test Note
│   │   └── 1.15.2.5  Previous Result Grid
│   ├── 1.15.3  History Output Mode
│   │   ├── 1.15.3.1  Attach History In Result Report Option
│   │   ├── 1.15.3.2  Print History In Separate Report Option
│   │   ├── 1.15.3.3  History Mode Confirm Action
│   │   └── 1.15.3.4  History Mode Cancel Action
│   └── 1.15.4  Patient History Report
│       ├── 1.15.4.1  History Report Header
│       │   ├── 1.15.4.1.1  History Report Title
│       │   ├── 1.15.4.1.2  History Date Range Field
│       │   ├── 1.15.4.1.3  History Printed By Entry
│       │   └── 1.15.4.1.4  History Printed In Entry
│       ├── 1.15.4.2  History Report Grid
│       │   ├── 1.15.4.2.1  History Analysis Date Column
│       │   ├── 1.15.4.2.2  History Patient ID Column
│       │   ├── 1.15.4.2.3  History Patient Name Column
│       │   └── 1.15.4.2.4  History Test Result Column
│       ├── 1.15.4.3  Multi-Patient History Panel
│       │   ├── 1.15.4.3.1  Multi-Patient History Grid
│       │   └── 1.15.4.3.2  Multi-Patient History Count
│       ├── 1.15.4.4  History Test Lists
│       │   ├── 1.15.4.4.1  History Tests In Lab List
│       │   └── 1.15.4.4.2  History Tests Available In Report List
│       ├── 1.15.4.5  Delete All History Action
│       ├── 1.15.4.6  History Report Print Action
│       └── 1.15.4.7  History Report Preview Action
├── 1.16  Main Tools and Utilities
│   └── 1.16.1  Main Tools Window
│       ├── 1.16.1.1  Tools Action Band
│       │   ├── 1.16.1.1.1  Test Library Action
│       │   ├── 1.16.1.1.2  Image Library Action
│       │   ├── 1.16.1.1.3  Abbreviations Dictionary Action
│       │   ├── 1.16.1.1.4  Stopwatch Action
│       │   ├── 1.16.1.1.5  Unit Converter Action
│       │   ├── 1.16.1.1.6  Work And Laboratory List Action
│       │   ├── 1.16.1.1.7  Reference Weight Tables Action
│       │   ├── 1.16.1.1.8  Data Manual Action
│       │   └── 1.16.1.1.9  Calculator Action
│       └── 1.16.1.2  Tools Capability Block
│           ├── 1.16.1.2.1  Test And Normal Ranges Reference Capability
│           ├── 1.16.1.2.2  Clinical Terminology Capability
│           ├── 1.16.1.2.3  Work Request Entry Capability
│           ├── 1.16.1.2.4  Stopwatch Calculator And Converter Capability
│           ├── 1.16.1.2.5  Reminder Message Capability
│           └── 1.16.1.2.6  Data Manual Search Capability
├── 1.17  Foreign Report
│   ├── 1.17.1  Foreign Report Window
│   ├── 1.17.2  Foreign Report Window Rule
│   └── 1.17.3  Foreign Report Sheet Output
├── 1.18  Printed Delivery Sheet and Order Slip
│   ├── 1.18.1  Delivery Sheet Header Block
│   │   ├── 1.18.1.1  Delivery Sheet Patient Id Field
│   │   ├── 1.18.1.2  Delivery Sheet Patient Name Field
│   │   ├── 1.18.1.3  Delivery Sheet Age Sex Field
│   │   ├── 1.18.1.4  Delivery Sheet Telephone Field
│   │   ├── 1.18.1.5  Delivery Sheet Referred By Field
│   │   ├── 1.18.1.6  Delivery Sheet Date Field
│   │   ├── 1.18.1.7  Delivery Sheet Time Field
│   │   ├── 1.18.1.8  Delivery Sheet Total Field
│   │   ├── 1.18.1.9  Delivery Sheet Paid Field
│   │   └── 1.18.1.10  Delivery Sheet Charge Field
│   ├── 1.18.2  Delivery Sheet Test Grid
│   │   ├── 1.18.2.1  Delivery Sheet Test Name Column
│   │   ├── 1.18.2.2  Delivery Sheet Department Column
│   │   ├── 1.18.2.3  Delivery Sheet Result Column
│   │   ├── 1.18.2.4  Delivery Sheet Collection Notes Column
│   │   └── 1.18.2.5  Delivery Sheet Signature Column
│   ├── 1.18.3  Delivery Sheet Signature Roles
│   │   ├── 1.18.3.1  Samples Responsible Role
│   │   └── 1.18.3.2  Reception Staff Role
│   └── 1.18.4  Per Patient Order Slip Printing
├── 1.19  Printed Result Report Structure
│   ├── 1.19.1  Report Summary Header Block
│   │   ├── 1.19.1.1  Report Header Patient Name Field
│   │   ├── 1.19.1.2  Report Header Net Total Field
│   │   ├── 1.19.1.3  Report Header Amount Paid Field
│   │   ├── 1.19.1.4  Report Header Remainder Field
│   │   ├── 1.19.1.5  Report Header Treating Doctor Field
│   │   ├── 1.19.1.6  Report Header Result Receipt Date Field
│   │   ├── 1.19.1.7  Report Header Account Type Field
│   │   └── 1.19.1.8  Report Header Patient Code Field
│   ├── 1.19.2  Report Test Group Table
│   │   ├── 1.19.2.1  Report Test Group Column
│   │   ├── 1.19.2.2  Report Test Count Column
│   │   └── 1.19.2.3  Report Test Price Column
│   └── 1.19.3  Report Paper Variants
├── 1.20  Profile Report Layout
│   ├── 1.20.1  Report Patient Block
│   ├── 1.20.2  Report Title Line
│   ├── 1.20.3  Report Section Heading
│   ├── 1.20.4  Report Sub Item Row
│   ├── 1.20.5  Report Reference Range Column
│   ├── 1.20.6  Report Flag Column
│   ├── 1.20.7  Report Emphasised Result
│   ├── 1.20.8  Report Secondary Mark Cell
│   ├── 1.20.9  Report Interpretation Block
│   ├── 1.20.10  Report Signature Line
│   ├── 1.20.11  Report Closing Line
│   ├── 1.20.12  Report Laboratory Footer
│   ├── 1.20.13  Report Pagination Line
│   └── 1.20.14  Report Header Band Option
├── 1.21  Report Behaviour Rules
│   ├── 1.21.1  Patient Reference Override Rule
│   ├── 1.21.2  Normal Range Length Rule
│   ├── 1.21.3  Report Paper Choice Rule
│   ├── 1.21.4  Report Language Side Rule
│   ├── 1.21.5  Test Interpretation Rule
│   ├── 1.21.6  Report Chart And Drawing Rule
│   ├── 1.21.7  Report Second Page Rule
│   └── 1.21.8  Single Sheet Report Rule
├── 1.22  Composite Test Entry Screen
│   ├── 1.22.1  Composite Screen Test Banner
│   ├── 1.22.2  Composite Screen Section Heading
│   ├── 1.22.3  Composite Screen Item Row
│   ├── 1.22.4  Composite Screen Item Check Box
│   ├── 1.22.5  Composite Screen Item Value List
│   ├── 1.22.6  Composite Screen Unordered Item
│   ├── 1.22.7  Composite Screen Resolved Range
│   ├── 1.22.8  Composite Screen Semi Quantitative Control
│   ├── 1.22.9  Composite Screen Second Result Row
│   ├── 1.22.10  Composite Screen Extra Value Field
│   ├── 1.22.11  Composite Screen Comment Block
│   ├── 1.22.12  Composite Screen Action Block
│   │   ├── 1.22.12.1  Composite Screen Save Action
│   │   ├── 1.22.12.2  Composite Screen Print Action
│   │   ├── 1.22.12.3  Composite Screen Preview Action
│   │   ├── 1.22.12.4  Composite Screen Review Action
│   │   └── 1.22.12.5  Composite Screen Main Result Action
│   └── 1.22.13  Blood Picture Entry Block
│       ├── 1.22.13.1  Blood Picture Calculated Index Row
│       ├── 1.22.13.2  Blood Picture Index Inclusion Mark
│       ├── 1.22.13.3  Blood Picture Differential Block
│       │   ├── 1.22.13.3.1  Blood Picture Differential Row
│       │   ├── 1.22.13.3.2  Blood Picture Relative Count Column
│       │   ├── 1.22.13.3.3  Blood Picture Absolute Count Column
│       │   ├── 1.22.13.3.4  Blood Picture Other Cells Row
│       │   └── 1.22.13.3.5  Blood Picture Segmented And Band Rows
│       ├── 1.22.13.4  Blood Picture Immature Cell Block
│       ├── 1.22.13.5  Auto Comment Option
│       ├── 1.22.13.6  Auto Comment Basis Option
│       └── 1.22.13.7  Constants Panel
├── 1.23  Culture And Sensitivity Report
│   ├── 1.23.1  Culture Report Examination Block
│   ├── 1.23.2  Culture Report Culture Block
│   ├── 1.23.3  Culture Report Sensitivity Block
│   │   ├── 1.23.3.1  Culture Report Sensitivity Heading
│   │   ├── 1.23.3.2  Culture Report Antibiotic Row
│   │   ├── 1.23.3.3  Culture Report Inhibition Zone Column
│   │   └── 1.23.3.4  Culture Report Commercial Name Column
│   └── 1.23.4  Culture Report Trade Name Rule
├── 1.24  Semen Morphology Report Page
│   ├── 1.24.1  Semen Motility Profile Block
│   ├── 1.24.2  Semen Motility Profile Chart
│   ├── 1.24.3  Semen Normal Grade Line
│   ├── 1.24.4  Semen Dead Forms Field
│   ├── 1.24.5  Semen Abnormal Forms Block
│   ├── 1.24.6  Semen Abnormal Forms Chart
│   └── 1.24.7  Semen Morphology Drawing Panel
└── 1.25  Image Report
    ├── 1.25.1  Image Report Sheet
    ├── 1.25.2  Image Report Image Count
    ├── 1.25.3  Image Report Image
    ├── 1.25.4  Image Report Annotation
    ├── 1.25.5  Image Report Annotation Placement
    ├── 1.25.6  Image Report Annotation Size
    ├── 1.25.7  Image Report Annotation Colour
    └── 1.25.8  Image Report Entry Rule
```

### §6.2 Function inventory

The elements of the unit, each defined once, at the place that carries it. The *Type / Level* column gives the kind of the element and the level at which it is recorded under the policy of §3. The *Scope* column marks the elements that belong to one of the five excluded categories of §2; they are documented, and they are not parity requirements.

| ID | Canonical name | Source label | Type / Level | Definition | Evidence | Scope |
|---|---|---|---|---|---|---|
| **1.1** | **Application Shell and Sign-In** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.1.1 | Top Ribbon | الشريط العلوي | Control / UI element | The persistent ribbon of the application; it carries one item per module and the item of the active module is the one shown as active. | [VERIFIED] | In scope |
| 1.1.1.1 | Ribbon Item List | — | List / UI element | The ordered set of ribbon items; each item opens one module of the program. | [VERIFIED] | In scope |
| 1.1.1.1.1 | Exit Item | Exit | Navigation / Subfunction | Closes the program. | [VERIFIED] | In scope |
| 1.1.1.1.2 | About Us Item | About Us | Navigation / Subfunction | Opens the program's information window. | [VERIFIED] | In scope |
| 1.1.1.1.3 | Send Result Item | Send Result | Navigation / Subfunction | Opens the result-delivery screen. | [VERIFIED] | In scope |
| 1.1.1.1.4 | Setting Item | Setting | Navigation / Subfunction | Opens the system settings. | [VERIFIED] | In scope |
| 1.1.1.1.5 | System Item | System | Navigation / Subfunction | Opens the system module. | [VERIFIED] | In scope |
| 1.1.1.1.6 | Users Item | Users | Navigation / Subfunction | Opens the users module. | [VERIFIED] | In scope |
| 1.1.1.1.7 | Statistics Item | Statistics | Navigation / Subfunction | Opens the statistics module. | [VERIFIED] | In scope |
| 1.1.1.1.8 | Accounts Item | Accounts | Navigation / Subfunction | Opens the laboratory accounts module. | [VERIFIED] | In scope |
| 1.1.1.1.9 | Tools Item | Tools | Navigation / Subfunction | Opens the tools module. | [VERIFIED] | In scope |
| 1.1.1.1.10 | Work Sheet Item | Work sheet | Navigation / Subfunction | Opens the work sheet module. | [VERIFIED] | In scope |
| 1.1.1.1.11 | Laboratory Item | Laboratory | Navigation / Subfunction | Opens the laboratory module. | [VERIFIED] | In scope |
| 1.1.1.1.12 | Patients Item | Patients | Navigation / Subfunction | Opens the patient-side module. | [VERIFIED] | In scope |
| 1.1.1.2 | Active Module Indicator | — | Indicator / UI element | Marks the ribbon item of the module currently in use. | [VERIFIED] | In scope |
| 1.1.2 | Status Bar | — | Control / UI element | The persistent bar beneath the working area. | [VERIFIED] | In scope |
| 1.1.2.1 | User Name Indicator | User Name | Field / UI element | The signed-in user. | [VERIFIED] | In scope |
| 1.1.2.2 | Last Login Indicator | Last Login | Field / UI element | The previous sign-in of that user, with its date and time. | [VERIFIED] | In scope |
| 1.1.2.3 | Database Label Indicator | SQL DATABASE | Field / UI element | The label of the data source in use. | [VERIFIED] | In scope |
| 1.1.2.4 | Today Indicator | Today | Field / UI element | The current date and time. | [VERIFIED] | In scope |
| 1.1.3 | Startup Splash Screen | — | Screen / Container | The screen shown while the program starts. It carries the data-source choice and a progress indication. | [VERIFIED] | In scope |
| 1.1.3.1 | Data Source Option Group | — | Field group / Container | The two data sources the program may be started against. | [VERIFIED] | In scope |
| 1.1.3.1.1 | File Database Option | — | Option / Subfunction | Starts the program against a file-based database. | [VERIFIED] | In scope |
| 1.1.3.1.2 | Client Server Database Option | — | Option / Subfunction | Starts the program against a client-server database. | [VERIFIED] | In scope |
| 1.1.3.2 | Startup Progress Indicator | — | Indicator / UI element | Reports the progress of the start sequence. | [VERIFIED] | In scope |
| 1.1.4 | Sign-In Window | — | Screen / Container | The window that authenticates the user before the program opens. | [VERIFIED] | In scope |
| 1.1.4.1 | Sign-In User Name Field | User Name | Field / UI element | The user's name, entered at sign-in. | [VERIFIED] | In scope |
| 1.1.4.2 | Sign-In Password Field | Password | Field / UI element | The user's password, entered at sign-in. | [VERIFIED] | In scope |
| 1.1.4.3 | Show Password Characters Option | Show password characters | Field / UI element | Controls whether the password is displayed as characters. | [VERIFIED] | In scope |
| 1.1.4.4 | Remember Login Option | Remember Login | Field / UI element | Controls whether the sign-in is remembered for the next start. | [VERIFIED] | In scope |
| 1.1.4.5 | Application Sign In Action | Sign in | Action / Function | Authenticates the user and opens the program. | [VERIFIED] | In scope |
| 1.1.5 | Patient Search Toolbar | — | Control / UI element | The toolbar carried above the working area of the patient module; it holds the quick-search control and the mode options. | [VERIFIED] | In scope |
| 1.1.5.1 | Quick Search Field | start | Field / UI element | The quick-search box of the patient module. | [VERIFIED] | In scope |
| 1.1.5.2 | Quick Search Mode Options | TM / TG / CG | Field group / Container | Three mutually exclusive mode options marked with the short codes TM, TG and CG. The meaning of the codes is not established. | [UNVERIFIED] | In scope |
| **1.2** | **Patient Registration and Master Data** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.2.1 | Main Patients Window | — | Screen / Container | The entry point of the patient-side module. Through it a patient can be added and their barcode and receipt printed, their results entered and printed; a specific patient can be found, or the patients of a period displayed with their unfinished, unverified and unprinted results identified; and results delivered with the patients' accounts settled and the undelivered results counted. | [VERIFIED] | In scope |
| 1.2.1.1 | Add Patient Action | إضافة | Action / Function | Opens the add-and-edit patient data screen for a new patient. | [VERIFIED] | In scope |
| 1.2.1.2 | Enter Results Action | نتائج التحاليل | Action / Function | Opens the result-entry screen. | [VERIFIED] | In scope |
| 1.2.1.3 | Find Patient Action | — | Action / Function | Opens the patient-search screen. | [VERIFIED] | In scope |
| 1.2.1.4 | Period List Action | — | Action / Function | Displays the patients of a chosen period. | [VERIFIED] | In scope |
| 1.2.1.5 | Unfinished Results View | — | View / Subfunction | Lists the results that have been entered but not finished. | [VERIFIED] | In scope |
| 1.2.1.6 | Unverified Results View | — | View / Subfunction | Lists the results that have been entered but not verified. | [VERIFIED] | In scope |
| 1.2.1.7 | Unprinted Results View | — | View / Subfunction | Lists the results that have been entered but not printed. | [VERIFIED] | In scope |
| 1.2.1.8 | Deliver Results Action | — | Action / Function | Opens the results-delivery screen. | [VERIFIED] | In scope |
| 1.2.1.9 | Settle Accounts Action | — | Action / Function | Opens the account-settlement work of the delivery screen. | [VERIFIED] | In scope |
| 1.2.1.10 | Undelivered Results Count | — | Indicator / UI element | Counts the results that have not yet been delivered. | [VERIFIED] | In scope |
| 1.2.1.11 | Test Registration Statistics Action | — | Action / Function | Opens a statistics view of test registration. The resulting screen is not shown. | [NOT-SHOWN] | In scope |
| 1.2.2 | Add-and-Edit Patient Data Screen | إضافة وتعديل بيانات المرضى | Screen / Container | The screen used both to register a new patient and to modify an existing patient or test registration. It carries the patient header, the identity block, the medical-history block, the notes and communication block, the test-selection block, the account panel and the action band. | [VERIFIED] | In scope |
| 1.2.2.1 | Patient Header Block | — | Block / Container | The strip above the form carrying the visit dates, the patient code and two header actions. | [VERIFIED] | In scope |
| 1.2.2.1.1 | Entry Date Field | تاريخ الدخول | Field / UI element | The date on which the patient was entered; it carries the time as well. | [VERIFIED] | In scope |
| 1.2.2.1.2 | Receipt Date Field | تاريخ الاستلام | Field / UI element | The date and time at which the result is received. | [VERIFIED] | In scope |
| 1.2.2.1.3 | Patient Code Field | كود المريض | Field / UI element | The system-assigned patient code, shown read-only. | [VERIFIED] | In scope |
| 1.2.2.1.4 | VIP Action | VIP | Action / Function | Marks the patient as belonging to the VIP group. | [VERIFIED] | In scope |
| 1.2.2.1.5 | Laboratory ID Action | Lab. ID | Action / Function | Generates or shows the laboratory identifier of the current visit. | [VERIFIED] | In scope |
| 1.2.2.2 | Identity Block | — | Block / Container | The block that carries the patient's own particulars. | [VERIFIED] | In scope |
| 1.2.2.2.1 | Patient Name Field | اسم المريض | Field / UI element | Text entry for the patient's name, with a drop-down offering names already registered. | [VERIFIED] | In scope |
| 1.2.2.2.2 | Sex Radio Group | النوع | Field / UI element | The patient's sex. | [VERIFIED] | In scope |
| 1.2.2.2.2.1 | Male Option | Male | Option / Subfunction | Records the patient as male. | [VERIFIED] | In scope |
| 1.2.2.2.2.2 | Female Option | Female | Option / Subfunction | Records the patient as female. | [VERIFIED] | In scope |
| 1.2.2.2.3 | Age Value Field | سن المريض | Field / UI element | The patient's age. | [VERIFIED] | In scope |
| 1.2.2.2.4 | Age Unit Selector | Years | Field / UI element | The unit of the age value; Years is the option shown. | [VERIFIED] | In scope |
| 1.2.2.2.5 | Mobile and Telephone Field | موبيل / تليفون | Field / UI element | The patient's telephone number. Optional. | [VERIFIED] | In scope |
| 1.2.2.2.6 | Card Number Field | رقم بطاقة / الملف | Field / UI element | The patient's card number. Optional. | [VERIFIED] | In scope |
| 1.2.2.2.7 | Account Type Selector | نوع الحساب | Field / UI element | The account type the patient is handled under; Individual is the option shown. | [VERIFIED] | In scope |
| 1.2.2.2.8 | Sample Type Field | نوع العينة | Field / UI element | The sample type, as carried in the identity block. | [VERIFIED] | In scope |
| 1.2.2.2.9 | Patient Address Field | عنوان المريض | Field / UI element | The patient's address. Optional. | [VERIFIED] | In scope |
| 1.2.2.2.10 | Referring Entity Selector | جهة الإحالة | Field / UI element | The entity that referred the patient. | [VERIFIED] | In scope |
| 1.2.2.2.10.1 | Referring Entity Additional Information Action | معلومات اضافية | Action / Function | Opens the additional information of the referring entity. | [VERIFIED] | In scope |
| 1.2.2.2.11 | Treating Doctor Selector | الطبيب المعالج | Field / UI element | The doctor treating the patient. Optional. | [VERIFIED] | In scope |
| 1.2.2.2.11.1 | Treating Doctor Additional Information Action | معلومات اضافية | Action / Function | Opens the additional information of the treating doctor. | [VERIFIED] | In scope |
| 1.2.2.3 | Medical History Block | — | Block / Container | The block of check-box questions covering the patient's fasting state, the medication the patient is on, the conditions the patient has, and two further questions. Each question is answered independently and carries its own check box. | [VERIFIED] | In scope |
| 1.2.2.3.1 | Fasting Toggle | صائم | Toggle / UI element | Records that the patient is fasting. | [VERIFIED] | In scope |
| 1.2.2.3.2 | Fasting Hours Field | عدد ساعات الصيام | Field / UI element | The number of fasting hours, shown beside the fasting toggle. | [VERIFIED] | In scope |
| 1.2.2.3.3 | Diabetes Treatment Toggle | هل يؤخذ علاج للسكر | Toggle / UI element | Asks whether the patient is being treated for diabetes. | [VERIFIED] | In scope |
| 1.2.2.3.4 | Blood-Thinning Treatment Toggle | هل يؤخذ علاج لسيولة الدم | Toggle / UI element | Asks whether the patient is being treated for blood thinning. | [VERIFIED] | In scope |
| 1.2.2.3.5 | Gland Treatment Toggle | هل يؤخذ علاج للغدد | Toggle / UI element | Asks whether the patient is being treated for a gland disorder. | [VERIFIED] | In scope |
| 1.2.2.3.6 | Antiviral Treatment Toggle | هل يؤخذ علاج للفيروسات | Toggle / UI element | Asks whether the patient is being treated for a viral infection. | [VERIFIED] | In scope |
| 1.2.2.3.7 | Antibiotic Treatment Toggle | هل يؤخذ مضاد حيوي | Toggle / UI element | Asks whether the patient is being treated with an antibiotic. | [VERIFIED] | In scope |
| 1.2.2.3.8 | Liver Treatment Toggle | هل يؤخذ علاج للكبد | Toggle / UI element | Asks whether the patient is being treated for a liver condition. | [VERIFIED] | In scope |
| 1.2.2.3.9 | Pregnancy Toggle | للسيدات هل يوجد حمل | Toggle / UI element | Asks whether the patient is pregnant; the question is offered to female patients. | [VERIFIED] | In scope |
| 1.2.2.3.10 | Imaging Within Two Days Toggle | هل تم عمل اشاعة بالصبغة او بالموجات الصوتية خلال يومين | Toggle / UI element | Records that a contrast study or an ultrasound was performed within the last two days. | [VERIFIED] | In scope |
| 1.2.2.3.11 | Anaemia Condition Toggle | مريض بفقر الدم | Toggle / UI element | Records that the patient has anaemia. | [VERIFIED] | In scope |
| 1.2.2.3.12 | Joint Inflammation Condition Toggle | مريض بالتهاب المفاصل | Toggle / UI element | Records that the patient has joint inflammation. | [VERIFIED] | In scope |
| 1.2.2.3.13 | High Blood Pressure Condition Toggle | مريض بالضغط الدم | Toggle / UI element | Records that the patient has high blood pressure. | [VERIFIED] | In scope |
| 1.2.2.3.14 | Renal Failure Condition Toggle | مريض بالفشل كلوي | Toggle / UI element | Records that the patient has renal failure. | [VERIFIED] | In scope |
| 1.2.2.3.15 | Lupus Condition Toggle | مريض بالذئبة الحمراء | Toggle / UI element | Records that the patient has lupus erythematosus. | [VERIFIED] | In scope |
| 1.2.2.4 | Notes and Communication Block | — | Block / Container | The block that carries the patient's notes and the channels through which a result may be sent. | [VERIFIED] | In scope |
| 1.2.2.4.1 | Patient Notes Field | ملاحظات | Field / UI element | Free-text notes for the patient, shown in the result-entry and result-delivery windows. Optional. | [VERIFIED] | In scope |
| 1.2.2.4.2 | Print Result Password Field | Print Result Password | Field / UI element | The password required to print the result. | [VERIFIED] | In scope |
| 1.2.2.4.2.1 | Print Result Password Enable Option | — | Field / UI element | Enables the password requirement for printing the result. | [VERIFIED] | In scope |
| 1.2.2.4.3 | Send SMS Field | Send SMS | Field / UI element | The destination of the SMS. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — external SMS, e-mail and fax delivery |
| 1.2.2.4.4 | Send E-mail Field | Send E-mail | Field / UI element | The destination of the e-mail. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — external SMS, e-mail and fax delivery |
| 1.2.2.4.5 | Fax Field | By Fax | Field / UI element | The destination of the fax. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — external SMS, e-mail and fax delivery |
| 1.2.2.4.6 | Send Action | — | Action / Function | Sends the patient's result by SMS, e-mail or fax. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — external SMS, e-mail and fax delivery |
| 1.2.2.4.7 | Site Upload Action | — | Action / Function | Opens the result-upload screen of the external result service. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.2.2.4.8 | Site Reference Field | External Result Service Address | Field / UI element | A field carrying the address of the external result service, shown beside the notes; the same address is printed on the patient's site-access label and on the receipt. The source does not state whether the field itself is editable. | [UNVERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.2.2.5 | Test Selection Block | — | Block / Container | The block from which the tests to be performed are chosen. | [VERIFIED] | In scope |
| 1.2.2.5.1 | Test Filter Bar | — | Control / UI element | The strip above the master test list, carrying the filter control, the three mode options and the menu actions. | [VERIFIED] | In scope |
| 1.2.2.5.1.1 | Test Filter Field | start | Field / UI element | The filter control of the master test list. | [VERIFIED] | In scope |
| 1.2.2.5.1.2 | Test Filter Mode Options | TM / TG / CG | Field group / Container | Three mutually exclusive mode options marked with the short codes TM, TG and CG. The meaning of the codes is not established. | [UNVERIFIED] | In scope |
| 1.2.2.5.1.3 | Complete Test Menu Action | complete test menu | Action / Function | Opens the complete test menu of the master test list. | [VERIFIED] | In scope |
| 1.2.2.5.1.4 | Test Group Menu Action | test group menu | Action / Function | Opens the test-group menu of the master test list. | [VERIFIED] | In scope |
| 1.2.2.5.2 | Master Test List | — | List / UI element | The laboratory's scrollable list of registered tests, from which a test is chosen for the patient. | [VERIFIED] | In scope |
| 1.2.2.5.3 | Add Select Test Action | Add select test | Action / Function | Adds the selected master test to the patient. | [VERIFIED] | In scope |
| 1.2.2.5.4 | Patient Test Delete Action | Delete | Action / Function | Removes the selected test from the patient. | [VERIFIED] | In scope |
| 1.2.2.5.5 | All Tests Action | All | Action / Function | Clears all of the patient's tests; shown when the patient is registered for the first time. | [VERIFIED] | In scope |
| 1.2.2.5.6 | Patient's Tests List | Patient's tests | List / UI element | The tests registered for the current patient. | [VERIFIED] | In scope |
| 1.2.2.5.6.1 | Patient's Tests Count | — | Indicator / UI element | The number of tests in the patient's tests list, carried in the panel title. | [VERIFIED] | In scope |
| 1.2.2.5.7 | Test Group List | — | List / UI element | The laboratory's predefined test groups, from which a whole group can be added at once. | [VERIFIED] | In scope |
| 1.2.2.5.8 | Add All Tests In Group Action | Add all tests in group | Action / Function | Adds every test of the selected group to the patient; the group is added by pressing it twice. | [VERIFIED] | In scope |
| 1.2.2.5.9 | Group Send E-mail Action | Send E-mail | Action / Function | Sends the selected group by e-mail. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — external SMS, e-mail and fax delivery |
| 1.2.2.5.10 | Sample Type Radio Group | — | Field / UI element | The sample type applied to the registered tests; the options are shown as a row of radio buttons. | [VERIFIED] | In scope |
| 1.2.2.5.10.1 | Urine Option | Urine | Option / Subfunction | Applies the urine sample type. | [VERIFIED] | In scope |
| 1.2.2.5.10.2 | Stool Option | Stool | Option / Subfunction | Applies the stool sample type. | [VERIFIED] | In scope |
| 1.2.2.5.10.3 | Blood Option | Blood | Option / Subfunction | Applies the blood sample type. | [VERIFIED] | In scope |
| 1.2.2.5.10.4 | Semen Option | Semen | Option / Subfunction | Applies the semen sample type. | [VERIFIED] | In scope |
| 1.2.2.5.10.5 | CSF Option | CSF | Option / Subfunction | Applies the cerebrospinal-fluid sample type. | [VERIFIED] | In scope |
| 1.2.2.5.11 | Taken Outside Lab Flag | Taken outside lab | Field / UI element | Marks the sample as collected outside the laboratory; the mark then appears as a note in the patient's test report. | [VERIFIED] | In scope |
| 1.2.2.6 | Action Band | — | Block / Container | The strip of actions that closes the add-and-edit screen. | [VERIFIED] | In scope |
| 1.2.2.6.1 | Save Registration Action | حفظ | Action / Function | Persists the registration. | [VERIFIED] | In scope |
| 1.2.2.6.2 | Add Entry Action | إضافة | Action / Function | Adds the current entry. | [VERIFIED] | In scope |
| 1.2.2.6.3 | Edit Entry Action | تعديل | Action / Function | Opens the entry for editing. | [VERIFIED] | In scope |
| 1.2.2.6.4 | Undo Action | تراجع | Action / Function | Steps back out of the current operation. | [VERIFIED] | In scope |
| 1.2.2.6.5 | Print Barcode Action | الباركود | Action / Function | Prints the patient's barcode labels from the add-and-edit patient data window. The labels produced are the specimen-container labels, the file barcode and the laboratory-identifier barcode; their kinds, their content and the rules that distribute the tests over the containers are defined in group 1.14. | [VERIFIED] | In scope |
| 1.2.2.6.6 | Print Receipt Action | الإيصال | Action / Function | Prints the patient's receipt, carrying the patient identity, the ordered test lines with their amounts, the totals, the discount, the net invoice, the paid amount and the remainder, and a closing line. | [VERIFIED] | In scope |
| 1.2.2.6.7 | Patient Invoice Action | فاتورة المرضى | Action / Function | Prints the patient invoice. | [VERIFIED] | In scope |
| 1.2.2.6.8 | Patient Envelope Print Action | طباعة ظرف | Action / Function | Prints the result envelope from the patient screen. | [VERIFIED] | In scope |
| 1.2.2.6.9 | Modify Action | تعديل | Action / Function | Opens the patient's data for modification. | [VERIFIED] | In scope |
| 1.2.2.6.10 | Follow-up Action | — | Action / Function | Opens the case follow-up screen. | [VERIFIED] | In scope |
| 1.2.2.6.11 | Work Sheet Action | ورقة العمل | Action / Function | Opens the work sheet for the patient. It refers to the work sheet module. | [VERIFIED] | In scope |
| 1.2.2.6.12 | ID Card Action | الكارنيه | Action / Function | Opens the patient's identification card. The resulting document is not shown. | [NOT-SHOWN] | In scope |
| 1.2.2.6.13 | Patient Screen Action | الشاشة الرئيسية | Action / Subfunction | Returns to the main patient screen. | [VERIFIED] | In scope |
| 1.2.2.6.14 | Main Point Action | — | Action / Subfunction | Returns to the program's main point. | [VERIFIED] | In scope |
| 1.2.2.6.15 | Patient Window Action | نافذة المرضى | Action / Function | Opens the patient list from the add-and-edit screen. | [VERIFIED] | In scope |
| 1.2.2.6.16 | Lab Results Action | نتائج التحاليل | Action / Function | Opens the laboratory results from the add-and-edit screen. | [VERIFIED] | In scope |
| **1.3** | **Patient Account** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.3.1 | Account Panel | — | Screen region / UI element | The block on the patient screen that holds the account figures and the settlement actions. | [VERIFIED] | In scope |
| 1.3.1.1 | Total of Tests Field | اجمالى التحاليل | Field / UI element | The total value of the patient's registered tests, computed from the prices entered in the program. | [VERIFIED] | In scope |
| 1.3.1.2 | Discount Value Field | قيمة الخصم | Field / UI element | The discount applied to the patient's account as an absolute amount. | [VERIFIED] | In scope |
| 1.3.1.3 | Discount Percentage Field | % | Field / UI element | The discount applied to the patient's account as a percentage. | [VERIFIED] | In scope |
| 1.3.1.4 | Paid Amount Field | المدفوع | Field / UI element | The amount paid by the patient; entered by the user. | [VERIFIED] | In scope |
| 1.3.1.4.1 | Surcharge Entry Convention | — | Rule / Rule | An additional amount that is not linked to a test — an extra service or an additional cost — is entered in the paid field preceded by a `+` sign, and is then added to the total of the tests. | [VERIFIED] | In scope |
| 1.3.1.4.2 | Enter Key Account Operation | — | Rule / Rule | The first `Enter` press performs the accounting operation for the patient; the second press saves the operation. | [VERIFIED] | In scope |
| 1.3.1.5 | Paid Previously Field | المدفوع سابقا | Field / UI element | The amount already paid on the account. | [VERIFIED] | In scope |
| 1.3.1.6 | Amount After Discount Field | التكلفة بعد الخصم | Field / UI element | The account total after the discount is applied. | [VERIFIED] | In scope |
| 1.3.1.7 | Total Collected Field | — | Field / UI element | The total amount collected on the account. | [VERIFIED] | In scope |
| 1.3.1.8 | Remainder for Customer Field | الباقى للمريض | Field / UI element | The outstanding amount owed by the customer. | [VERIFIED] | In scope |
| 1.3.1.9 | Remainder for Laboratory Field | الباقى للمعمل | Field / UI element | The amount retained by the laboratory. | [VERIFIED] | In scope |
| 1.3.1.10 | Confirm Payment Action | موافق | Action / Function | Confirms the entered payment. | [VERIFIED] | In scope |
| 1.3.1.11 | Net Settlement Action | خالص | Action / Function | Closes the account when the patient has paid the amount in full; the settlement is then confirmed in the following dialog. | [VERIFIED] | In scope |
| 1.3.1.12 | Account Summary Action | حساب المريض | Action / Function | Opens the patient-account screen for the selected patient. | [VERIFIED] | In scope |
| 1.3.1.13 | Custody Action | حوافظ | Action / Function | Opens the custody holders of the patient. Its exact behaviour is not established. | [UNVERIFIED] | In scope |
| 1.3.1.14 | Account Back Action | تراجع | Action / Subfunction | Steps back out of the account operation. | [VERIFIED] | In scope |
| 1.3.2 | Patient Account Screen | حساب المريض | Screen / Container | The screen that lists the transactions of the patient's account and the patient's tests, and on which a paid amount is corrected. | [VERIFIED] | In scope |
| 1.3.2.1 | Account Transaction Grid | — | Grid / UI element | The grid of transactions made on the patient's account. | [VERIFIED] | In scope |
| 1.3.2.1.1 | Paid Date Column | Paid date | Grid column / UI element | The date on which the transaction was paid. | [VERIFIED] | In scope |
| 1.3.2.1.2 | Money Column | Money | Grid column / UI element | The amount of the transaction. | [VERIFIED] | In scope |
| 1.3.2.1.3 | User Name Column | User name | Grid column / UI element | The user who made the transaction. | [VERIFIED] | In scope |
| 1.3.2.1.4 | Edit Date Column | Edit date | Grid column / UI element | The date on which the transaction was last edited. | [VERIFIED] | In scope |
| 1.3.2.1.5 | Color Code Column | Color code | Grid column / UI element | A narrow column of the account grid whose header names a colour classification. The colours it carries are not described here. | [VERIFIED] | In scope |
| 1.3.2.2 | Modify Amount Action | تعديل | Action / Function | Edits an amount already entered on the account: the amount is selected and the modify action is pressed. | [VERIFIED] | In scope |
| 1.3.2.3 | Delete Transaction Action | حذف | Action / Function | Deletes a transaction from the account: the transaction is selected and the delete action is pressed. | [VERIFIED] | In scope |
| 1.3.2.4 | Pay Action | دفع | Action / Function | Records a payment against the account. | [VERIFIED] | In scope |
| 1.3.2.5 | Refund Action | استرداد | Action / Function | Records a refund against the account. | [VERIFIED] | In scope |
| 1.3.2.6 | Add Amount Action | اضافة | Action / Function | Adds a further amount against the account. | [VERIFIED] | In scope |
| 1.3.2.7 | Clear Account Action | تصفير الحساب | Action / Function | Clears the account of the patient. | [VERIFIED] | In scope |
| 1.3.2.8 | Account Patient List Action | قائمة المرضى | Action / Function | Returns to the patient list. | [VERIFIED] | In scope |
| **1.4** | **Results Delivery and Account Settlement** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.4.1 | Deliver Patient Results Screen | — | Screen / Container | Used when the patient has settled part of the account and a balance remains with the laboratory; the balance is viewed at the moment the results are delivered. | [VERIFIED] | In scope |
| 1.4.1.1 | Delivery Filter Block | — | Block / Container | The filters that narrow the delivery list. | [VERIFIED] | In scope |
| 1.4.1.1.1 | Delivery Date Filter | — | Field / UI element | The period selector of the delivery list. | [VERIFIED] | In scope |
| 1.4.1.1.2 | Delivery Name Filter | — | Field / UI element | The patient-name selector of the delivery list. | [VERIFIED] | In scope |
| 1.4.1.1.3 | Delivery Status Filter | — | Field / UI element | The delivery-status selector of the delivery list; the states are delivered, reviewed and printed. | [VERIFIED] | In scope |
| 1.4.1.1.4 | Delivery Department Filter | — | Field / UI element | The department selector of the delivery list. | [VERIFIED] | In scope |
| 1.4.1.2 | Patient Scope Block | — | Block / Container | The controls that decide which patients the delivery list covers. | [VERIFIED] | In scope |
| 1.4.1.2.1 | Patient Source Selector | Individual / LabToLab / Contracts | Field / UI element | Selects the source of the patients listed. | [VERIFIED] | In scope |
| 1.4.1.2.2 | Patient Group Selector | VIP / Free / All | Field / UI element | Selects the group of the patients listed. | [VERIFIED] | In scope |
| 1.4.1.2.3 | Patients Today Panel | عدد المرضى اليوم | Panel / Container | Lists the patients registered that day, with their count. | [VERIFIED] | In scope |
| 1.4.1.2.3.1 | Delivery Patient Grid | — | Grid / UI element | The grid of patients, with the columns code, name, type, laboratory identifier, referring entity and age. | [VERIFIED] | In scope |
| 1.4.1.3 | Delivery Result Block | — | Block / Container | The grid of patients and tests that the delivery list is built from. | [VERIFIED] | In scope |
| 1.4.1.3.1 | Delivery Result List Grid | — | Grid / UI element | Lists the patients and their tests, with a per-test check box in each of the finish, verify, print and export columns. The grid is what the delivery list prints or previews. | [VERIFIED] | In scope |
| 1.4.1.3.1.1 | Test Abbreviation Column | Test Abbreviation | Grid column / UI element | Shows the test's abbreviation. | [VERIFIED] | In scope |
| 1.4.1.3.1.2 | P Column | P | Grid column / UI element | A narrow column of the results grid whose header is the single letter P. Its content is not established. | [UNVERIFIED] | In scope |
| 1.4.1.3.1.3 | T Column | T | Grid column / UI element | A narrow column of the results grid whose header is the single letter T. Its content is not established. | [UNVERIFIED] | In scope |
| 1.4.1.3.1.4 | Result Column | Result | Grid column / UI element | Shows the entered result. | [VERIFIED] | In scope |
| 1.4.1.3.1.5 | Status Column | Status | Grid column / UI element | Shows the status of the test. | [VERIFIED] | In scope |
| 1.4.1.3.1.6 | Delivery Finish Column | Finish | Grid column / UI element | The per-test check box marking the test as finished. | [VERIFIED] | In scope |
| 1.4.1.3.1.7 | Delivery Verify Column | Verify | Grid column / UI element | The per-test check box marking the test as verified. | [VERIFIED] | In scope |
| 1.4.1.3.1.8 | Delivery Print Column | Print | Grid column / UI element | The per-test check box marking the test as printed. | [VERIFIED] | In scope |
| 1.4.1.3.1.9 | Delivery Export Column | Export | Grid column / UI element | The per-test check box marking the test as exported. | [VERIFIED] | In scope |
| 1.4.1.3.1.10 | See Report Action | See Report | Action / Function | Opens the report of the selected test from within the results grid. | [VERIFIED] | In scope |
| 1.4.1.4 | Delivery Action Block | — | Block / Container | The actions available on the delivery screen. | [VERIFIED] | In scope |
| 1.4.1.4.1 | Patient Account Action | — | Action / Function | Computes and displays the selected patient's account, showing the remainder for the patient or the remainder for the laboratory. | [VERIFIED] | In scope |
| 1.4.1.4.2 | Refresh Delivery List Action | Refresh | Action / Function | Reloads the delivery list. | [VERIFIED] | In scope |
| 1.4.1.4.3 | Delivery List Print Action | طباعة | Action / Function | Prints the delivery list. | [VERIFIED] | In scope |
| 1.4.1.4.4 | Preview Action | معاينة | Action / Function | Shows the delivery list before printing. | [VERIFIED] | In scope |
| 1.4.1.4.5 | Copy Action | — | Action / Function | Copies the current delivery entry. | [VERIFIED] | In scope |
| 1.4.1.4.6 | Notes Action | ملاحظات | Action / Function | Opens the notes attached to the delivery entry. | [VERIFIED] | In scope |
| 1.4.1.4.7 | Delivery Patient Data Action | بيانات المريض | Action / Function | Opens the patient's data from the delivery screen. | [VERIFIED] | In scope |
| 1.4.1.4.8 | Combined Report Action | تقرير مجمع | Action / Function | Opens the combined report. It refers to the combined report group of this module. | [VERIFIED] | In scope |
| 1.4.1.4.9 | Patient Medical History Action | التاريخ المرضي | Action / Function | Opens the patient's medical history. | [VERIFIED] | In scope |
| 1.4.1.4.10 | Delivery Empty Report Action | تقرير فارغ | Action / Function | Opens the empty report. It refers to the empty report group of this module. | [VERIFIED] | In scope |
| 1.4.1.4.11 | Delivery Envelope Print Action | طباعة ظرف | Action / Function | Prints the result envelope from the delivery screen. | [VERIFIED] | In scope |
| 1.4.1.4.12 | Send SMS Action | SMS | Action / Function | Sends the result by SMS. | [VERIFIED] | In scope |
| 1.4.1.4.13 | Delivery Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list from the delivery screen. | [VERIFIED] | In scope |
| **1.5** | **Patient and Test Modification** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.5.1 | Modification Path One | الطريقة الأولى | Workflow / UI element | Modify a previously entered patient, or add or remove a test, through the add-and-edit patient data screen: press the patient icon, press the add-and-edit patient data action, press the patient-list action, choose the registration day, select the patient, press modify, make the change, then press save. | [VERIFIED] | In scope |
| 1.5.1.1 | Patient Icon Action | Patient | Action / Function | Opens the patient-side working point from which the add-and-edit screen is reached. | [VERIFIED] | In scope |
| 1.5.1.2 | Patient List Open Action | قائمة المرضى | Action / Function | Opens the patient list from which the registration day is chosen. | [VERIFIED] | In scope |
| 1.5.1.3 | Registration Day Selector | — | Field / UI element | Chooses the day on which the patient was registered, within the patient list. | [VERIFIED] | In scope |
| 1.5.1.4 | Edit Patient Action | تعديل | Action / Function | Opens the patient's record for editing. | [VERIFIED] | In scope |
| 1.5.1.5 | Patient Selection Action | — | Action / Function | Selects the patient to be modified. | [VERIFIED] | In scope |
| 1.5.1.6 | Modification Save Action | حفظ | Action / Function | Persists the modification; the same action as the save of the registration. | [VERIFIED] | In scope |
| 1.5.2 | Modification Path Two | — | Workflow / UI element | Modify a patient by finding the patient through patient search and then entering the patient's data, which is displayed in detail. | [VERIFIED] | In scope |
| 1.5.2.1 | Search Action | — | Action / Function | Opens the patient-search screen. | [VERIFIED] | In scope |
| 1.5.2.2 | Patient Data Entry Action | — | Action / Function | Enters the patient's data, which is then displayed in detail. | [VERIFIED] | In scope |
| 1.5.3 | Patient Data Access Action | — | Action / Function | Reaches the patient's data from the result-entry screen. | [VERIFIED] | In scope |
| **1.6** | **Test Result Entry** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.6.1 | Result Entry Screen | — | Screen / Container | Where the results of a patient's tests are entered and the report is printed. | [VERIFIED] | In scope |
| 1.6.1.1 | Patient List Panel | عدد المرضى اليوم | Panel / Container | Lists the patients registered that day with their count; a patient is opened by clicking the name once, whereupon that patient's tests appear. | [VERIFIED] | In scope |
| 1.6.1.1.1 | Result Entry Patient Grid | — | Grid / UI element | The grid of patients registered that day, with the columns code, name, type, laboratory identifier, referring entity and age. | [VERIFIED] | In scope |
| 1.6.1.1.2 | Patient Name Click Action | — | Action / Function | Opens the selected patient and shows that patient's tests. | [VERIFIED] | In scope |
| 1.6.1.1.3 | Patient Count Indicator | — | Indicator / UI element | The number of patients in the patient list. | [VERIFIED] | In scope |
| 1.6.1.2 | Test Results Grid | — | Grid / UI element | The grid of the patient's tests, in which the results are entered. | [VERIFIED] | In scope |
| 1.6.1.2.1 | Result Entry Test Abbreviation Column | Test Abbreviation | Grid column / UI element | Shows the test's abbreviation. | [VERIFIED] | In scope |
| 1.6.1.2.2 | Result Entry P Column | P | Grid column / UI element | A narrow column of the result-entry grid whose header is the single letter P. Its content is not established. | [UNVERIFIED] | In scope |
| 1.6.1.2.3 | Result Entry T Column | T | Grid column / UI element | A narrow column of the result-entry grid whose header is the single letter T. Its content is not established. | [UNVERIFIED] | In scope |
| 1.6.1.2.4 | Result Entry Result Column | Result | Grid column / UI element | Shows the entered result. | [VERIFIED] | In scope |
| 1.6.1.2.5 | Result Entry Status Column | Status | Grid column / UI element | Shows the status of the test. | [VERIFIED] | In scope |
| 1.6.1.2.6 | Abnormal Flag Column | — | Grid column / UI element | Shows the abnormal flag of the result; the flag states are Low, High and Critical. | [VERIFIED] | In scope |
| 1.6.1.2.7 | Low and High Column | — | Grid column / UI element | Shows the low and high band of the result. | [VERIFIED] | In scope |
| 1.6.1.2.8 | Unit Column | — | Grid column / UI element | Shows the unit of the result. | [VERIFIED] | In scope |
| 1.6.1.2.9 | Normal Range Column | Range | Grid column / UI element | Shows the test's reference range. | [VERIFIED] | In scope |
| 1.6.1.2.10 | Finish Flag | Finish | Field / UI element | Per-test check box marking the test as finished. | [VERIFIED] | In scope |
| 1.6.1.2.11 | Verify Flag | Verify | Field / UI element | Per-test check box marking the test as verified. | [VERIFIED] | In scope |
| 1.6.1.2.12 | Result Print Flag | Print | Field / UI element | Per-test check box marking the test as printed. | [VERIFIED] | In scope |
| 1.6.1.2.13 | Export Flag | Export | Field / UI element | Per-test check box marking the test as exported. | [VERIFIED] | In scope |
| 1.6.1.2.14 | Per-Test Comment Field | — | Field / UI element | A comment recorded against an individual test result; the comment appears in the report. | [VERIFIED] | In scope |
| 1.6.1.2.15 | Double-Click Test Action | See Report | Action / Function | Opens the report of the selected test so that its data can be entered. | [VERIFIED] | In scope |
| 1.6.1.3 | Entry Action Block | — | Block / Container | The actions that close the result-entry screen. | [VERIFIED] | In scope |
| 1.6.1.3.1 | Save Results Action | — | Action / Function | Persists the entered results. | [VERIFIED] | In scope |
| 1.6.1.3.2 | Print Preview Action | معاينة | Action / Function | Shows the report preview before printing. | [VERIFIED] | In scope |
| 1.6.1.3.3 | Result Report Print Action | طباعة | Action / Function | Prints the result report, carrying the report titles, the per-test result with its unit and reference range, the abnormal flag, the patient identity block and the doctor's signature line. | [VERIFIED] | In scope |
| 1.6.1.4 | Report Settings Screen | — | Screen / Container | The dialog that controls how the result report is rendered. | [VERIFIED] | In scope |
| 1.6.1.4.1 | Report Title Block | — | Block / Container | The text elements of the report header. | [VERIFIED] | In scope |
| 1.6.1.4.1.1 | Report Screen Title Field | — | Field / UI element | Sets the title shown on the report screen. | [VERIFIED] | In scope |
| 1.6.1.4.1.2 | Report Main Title Field | — | Field / UI element | Sets the report's main title. | [VERIFIED] | In scope |
| 1.6.1.4.1.3 | Report Sub Title Field | — | Field / UI element | Sets the report's sub title. | [VERIFIED] | In scope |
| 1.6.1.4.1.4 | Report Sub Sub-title Field | — | Field / UI element | Sets the report's sub sub-title. | [VERIFIED] | In scope |
| 1.6.1.4.1.5 | Reference Range Field | — | Field / UI element | Sets the reference-range text of the report. | [VERIFIED] | In scope |
| 1.6.1.4.1.6 | Report Comment Field | — | Field / UI element | Sets the report's comment text. | [VERIFIED] | In scope |
| 1.6.1.4.1.7 | Report Normal Range Field | Normal Range | Field / UI element | Sets the normal-range text of the report, shown beside the reference range. | [VERIFIED] | In scope |
| 1.6.1.4.1.8 | Main Symbol Field | Main Symbol | Field / UI element | Sets the symbol used for the main element of the report. | [VERIFIED] | In scope |
| 1.6.1.4.1.9 | Sub Symbol Field | Sub Symbol | Field / UI element | Sets the symbol used for the sub-elements of the report. | [VERIFIED] | In scope |
| 1.6.1.4.1.10 | Sub-title Symbol Field | Sub-title Symb | Field / UI element | Sets the symbol used for the sub-title element of the report. | [VERIFIED] | In scope |
| 1.6.1.4.2 | Report Display Option Block | — | Block / Container | The options that decide what the report shows. | [VERIFIED] | In scope |
| 1.6.1.4.2.1 | Show Abnormal Background Option | — | Field / UI element | Controls the background shading of abnormal results. | [VERIFIED] | In scope |
| 1.6.1.4.2.2 | Print Status Flag Option | — | Field / UI element | Controls printing of the low, high and critical status flag. | [VERIFIED] | In scope |
| 1.6.1.4.2.3 | Print Patient History Automatically Option | — | Field / UI element | Controls whether the patient history is printed automatically; a change made in this dialog applies to that report only. | [VERIFIED] | In scope |
| 1.6.1.4.3 | Report Typography Block | — | Block / Container | The typographic controls of the report. | [VERIFIED] | In scope |
| 1.6.1.4.3.1 | Symbol Font Selectors | — | Field group / Container | Select the fonts used for the report's symbol elements. | [VERIFIED] | In scope |
| 1.6.1.4.3.2 | Vertical Spacing Fields | — | Field group / Container | Space after the sub title, space between texts, and additional space to the left of the range. | [VERIFIED] | In scope |
| 1.6.1.4.3.3 | Normal Result Font Selector | Normal result font | Field / UI element | Selects the font of the normal result in the report. | [VERIFIED] | In scope |
| 1.6.1.4.3.4 | Text Alignment Control | محاذاة النص من اليسار | Field / UI element | Controls the alignment of the report text from the left. | [VERIFIED] | In scope |
| 1.6.1.4.3.5 | Cell Expansion Control | عرض الخانة لتسع المحتوى | Field / UI element | Controls the expansion of a cell so that it fits its content. | [VERIFIED] | In scope |
| 1.6.1.4.3.6 | Offset Amount Control | مقدار الإزاحة | Field / UI element | Sets the offset amount applied to the report element. | [VERIFIED] | In scope |
| 1.6.1.4.4 | Report Output Block | — | Block / Container | The controls that decide where the report goes and how it leaves the program. | [VERIFIED] | In scope |
| 1.6.1.4.4.1 | Printing Target Selector | — | Field / UI element | Selects the printer the report is sent to. | [VERIFIED] | In scope |
| 1.6.1.4.4.2 | Save Settings Action | — | Action / Function | Persists the report settings. | [VERIFIED] | In scope |
| 1.6.1.4.4.3 | Export Data Action | — | Action / Function | Exports the report data. | [VERIFIED] | In scope |
| 1.6.1.4.4.4 | Close Action | — | Action / Function | Closes the report settings. | [VERIFIED] | In scope |
| 1.6.1.5 | Report Entry Grid | Test Name \ ·  Result \ ·  Unit \ ·  L or H \ ·  Normal Ranges \ ·  Verified \ ·  Print | Grid / UI element | The grid in which a test's result is entered, as shown on the test's own report screen. Its columns are the test name, the result, the unit, the low-or-high flag, the normal ranges, and the verified and print check boxes. | [VERIFIED] | In scope |
| 1.6.1.5.1 | Report Entry Test Name Column | Test Name | Grid column / UI element | Shows the name of the test or of the sub-test being entered. | [VERIFIED] | In scope |
| 1.6.1.5.2 | Report Entry Result Column | Result | Grid column / UI element | Shows the entered result. | [VERIFIED] | In scope |
| 1.6.1.5.3 | Report Entry Unit Column | Unit | Grid column / UI element | Shows the unit of the result. | [VERIFIED] | In scope |
| 1.6.1.5.4 | Report Entry Flag Column | L or H | Grid column / UI element | Shows whether the result is below or above the normal range. | [VERIFIED] | In scope |
| 1.6.1.5.5 | Report Entry Normal Ranges Column | Normal Ranges | Grid column / UI element | Shows the normal range of the test, with the patient's own range where the test defines one. | [VERIFIED] | In scope |
| 1.6.1.5.6 | Report Entry Verified Flag | Verified | Field / UI element | Check box marking the entered result as verified. | [VERIFIED] | In scope |
| 1.6.1.5.7 | Report Entry Print Flag | Print | Field / UI element | Check box marking the entered result for printing. | [VERIFIED] | In scope |
| 1.6.1.5.8 | Report Entry Comment Field | Comment | Field / UI element | A comment recorded against the entered result. | [VERIFIED] | In scope |
| 1.6.1.5.9 | Urine Sample Sub-Row Panel | عرض عينة البول في التقرير المجمع | Panel / Container | A panel of extra rows shown under certain tests, carrying one row per sample and one sub-result per row; a sub-result that has no corresponding sample is marked as having no sample. | [VERIFIED] | In scope |
| 1.6.1.5.9.1 | Urine Sample Sub-Row | Urine Sample | Field / UI element | The sample the extra row refers to. | [VERIFIED] | In scope |
| 1.6.1.5.9.2 | Sub-Result Row | Glucose / Acetone | Field group / Container | The sub-results recorded for the sample of that row. | [VERIFIED] | In scope |
| 1.6.1.5.9.3 | Missing Sample Marker | No Sample | Indicator / UI element | Marks a sub-result for which no sample is available. | [VERIFIED] | In scope |
| 1.6.1.6 | Test Entry Variants | — | Block / Container | The entry of a test takes different shapes according to the kind of test: a single test carries one result, a profile test carries several sub-tests, and some sub-tests carry a control value together with the patient's value. | [VERIFIED] | In scope |
| 1.6.1.6.1 | Single Test Entry | — | Variant / UI element | A simple test is entered as one row carrying one result, one unit and one normal range. | [VERIFIED] | In scope |
| 1.6.1.6.2 | Profile Test Entry | — | Variant / UI element | A profile test is entered as a set of sub-test rows, each with its own result, unit and normal range, under the name of the profile. | [VERIFIED] | In scope |
| 1.6.1.6.3 | Paired Control And Patient Entry | — | Variant / UI element | A sub-test of a profile may be entered as a pair: a control value and the patient's value, each with its own unit. | [VERIFIED] | In scope |
| 1.6.1.6.4 | Derived Value Entry | — | Variant / UI element | A value may be derived from other values of the same report, as an international normalised ratio derived from the control time and the patient's prothrombin time, or a ratio between two enzyme results. | [VERIFIED] | In scope |
| 1.6.1.6.5 | Multi-Unit Entry | — | Variant / UI element | A single sub-test may carry more than one unit, as a bleeding time carried in minutes and in seconds. | [VERIFIED] | In scope |
| 1.6.1.6.6 | Qualitative Result Entry | — | Variant / UI element | A result may be qualitative rather than numeric, as a value that exceeds a limit, or a result recorded as absent. | [VERIFIED] | In scope |
| 1.6.1.7 | Report Screen Actions | — | Block / Container | The actions carried by the test's own report screen. | [VERIFIED] | In scope |
| 1.6.1.7.1 | Save Report Action | حفظ | Action / Function | Persists the entered result. | [VERIFIED] | In scope |
| 1.6.1.7.2 | Report Screen Print Action | طباعة | Action / Function | Prints the test report. | [VERIFIED] | In scope |
| 1.6.1.7.3 | Report Screen Preview Action | معاينة الطباعة | Action / Function | Shows the test report before printing. | [VERIFIED] | In scope |
| 1.6.1.7.4 | Report Patient History Action | Patient History | Action / Function | Opens the patient's earlier tests, so that a previous result can be inserted into the report. | [VERIFIED] | In scope |
| 1.6.1.7.5 | Report Screen Back Action | رجوع | Action / Subfunction | Steps back out of the test report. | [VERIFIED] | In scope |
| 1.6.1.7.6 | Report Screen Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **1.7** | **Combined Report** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.7.1 | Combined Report Screen | تقرير مجمع | Screen / Container | Holds a group of test results selected from the patient's own tests; the combined report is the printed output of this screen. | [VERIFIED] | In scope |
| 1.7.1.1 | Patient Tests In Our Lab List | Patient tests in our lab | List / UI element | The patient's tests; the source of the combined report. | [VERIFIED] | In scope |
| 1.7.1.2 | Available Tests In Report List | Available tests in report | List / UI element | The tests already included in the combined report. | [VERIFIED] | In scope |
| 1.7.1.3 | Add Test To Report Action | — | Action / Function | Adds the selected test to the combined report, either by pressing it twice or by selecting it and pressing the arrow control. | [VERIFIED] | In scope |
| 1.7.1.4 | Reorder Report Tests Action | — | Action / Function | Moves the selected test up or down to change the order of the report. | [VERIFIED] | In scope |
| 1.7.1.4.1 | Move Test Up Action | — | Action / Function | Moves the selected test one position earlier in the report. | [VERIFIED] | In scope |
| 1.7.1.4.2 | Move Test Down Action | — | Action / Function | Moves the selected test one position later in the report. | [VERIFIED] | In scope |
| 1.7.1.5 | Combined Report Option Block | — | Block / Container | The print options of the combined report. | [VERIFIED] | In scope |
| 1.7.1.5.1 | Print Sub Title Option | — | Field / UI element | Prints the group name as the report's sub title. | [VERIFIED] | In scope |
| 1.7.1.5.2 | Print Printed Test Again Option | — | Field / UI element | Prints an already printed test again without a message. | [VERIFIED] | In scope |
| **1.8** | **Empty Report** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.8.1 | Empty Report Screen | تقرير فارغ | Screen / Container | A report carrying the patient's data only. | [VERIFIED] | In scope |
| 1.8.1.1 | Empty Report Open Action | تقرير فارغ | Action / Function | Opens an empty report for the selected patient. | [VERIFIED] | In scope |
| 1.8.1.2 | Enter Data And Print Option | — | Option / Subfunction | Data may be entered into the empty report and printed. | [VERIFIED] | In scope |
| 1.8.1.3 | Patient Data Sheet Option | — | Option / Subfunction | A sheet containing only the patient's data may be printed from the empty report. | [VERIFIED] | In scope |
| **1.9** | **Culture Result Entry** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.9.1 | Culture Screen | — | Screen / Container | Where culture and sensitivity results are entered. | [VERIFIED] | In scope |
| 1.9.1.1 | Culture Data Block | — | Block / Container | The fields that identify what was cultured. | [VERIFIED] | In scope |
| 1.9.1.1.1 | Sample Field | Sample | Field / UI element | The sample type cultured. | [VERIFIED] | In scope |
| 1.9.1.1.2 | Organism A Field | Organism A | Field / UI element | The first organism identified. | [VERIFIED] | In scope |
| 1.9.1.1.3 | Organism B Field | Organism B | Field / UI element | The second organism identified. | [VERIFIED] | In scope |
| 1.9.1.1.4 | Organism C Field | Organism C | Field / UI element | The third organism identified. | [VERIFIED] | In scope |
| 1.9.1.1.5 | Culture Condition Field | Culture Condition | Field / UI element | The conditions under which the sample was cultured. | [VERIFIED] | In scope |
| 1.9.1.1.6 | Colony Count Field | Colony Count | Field / UI element | The colony count of the culture. | [VERIFIED] | In scope |
| 1.9.1.2 | Microscopy Block | — | Block / Container | The fields of the microscopic examination. | [VERIFIED] | In scope |
| 1.9.1.2.1 | Reaction Field | Reaction | Field / UI element | The reaction of the sample. | [VERIFIED] | In scope |
| 1.9.1.2.2 | Pus Cells Field | Pus Cells | Field / UI element | The pus-cell finding. | [VERIFIED] | In scope |
| 1.9.1.2.3 | Red Blood Cells Field | R.B.Cs | Field / UI element | The finding of the red blood cells. | [VERIFIED] | In scope |
| 1.9.1.2.4 | Epithelial Cells Field | Epithelial Cells | Field / UI element | The epithelial-cell finding. | [VERIFIED] | In scope |
| 1.9.1.2.5 | Crystal Field | Crystal | Field / UI element | The crystal finding. | [VERIFIED] | In scope |
| 1.9.1.2.6 | Fungi Field | Fungi | Field / UI element | The fungi finding. | [VERIFIED] | In scope |
| 1.9.1.2.7 | Bacteria Field | — | Field / UI element | The bacteria finding; shown as disabled in the inspected state. | [VERIFIED] | In scope |
| 1.9.1.2.8 | Additional Microscopy Fields | Others | Field group / Container | Further microscopy rows presented as repeated `Others` entries. | [VERIFIED] | In scope |
| 1.9.1.2.9 | Direct Culture Fields | Direct1 / Direct2 | Field group / Container | The `Direct1` and `Direct2` entries. | [VERIFIED] | In scope |
| 1.9.1.2.10 | Microscopy Complete Flag | Complete | Field / UI element | Check box marking the microscopy entry as complete. | [VERIFIED] | In scope |
| 1.9.1.3 | Culture Flag Block | — | Block / Container | The per-test check boxes of the culture entry. | [VERIFIED] | In scope |
| 1.9.1.3.1 | Culture Verified Flag | Verified | Field / UI element | Check box marking the culture entry as verified. | [VERIFIED] | In scope |
| 1.9.1.3.2 | Culture Print Flag | Print | Field / UI element | Check box marking the culture entry for printing. | [VERIFIED] | In scope |
| 1.9.1.4 | Sensitivity Block | Sensitivity | Block / Container | The block in which the antibiotics and their sensitivity are recorded. | [VERIFIED] | In scope |
| 1.9.1.4.1 | Antibiotic List | — | List / UI element | The antibiotics available for the culture. | [VERIFIED] | In scope |
| 1.9.1.4.2 | Antibiotic Add Action | Add | Action / Function | Adds the selected antibiotic to the culture, either by pressing it twice in the list or by selecting it and pressing add. | [VERIFIED] | In scope |
| 1.9.1.4.3 | Sensitivity Classification | — | Field group / Container | Each antibiotic is classified under one of the four classes below. | [VERIFIED] | In scope |
| 1.9.1.4.3.1 | Highly For Option | Highly For | Option / Subfunction | Classifies the antibiotic as highly effective. | [VERIFIED] | In scope |
| 1.9.1.4.3.2 | Moderate For Option | Moderate For | Option / Subfunction | Classifies the antibiotic as moderately effective. | [VERIFIED] | In scope |
| 1.9.1.4.3.3 | Low For Option | Low For | Option / Subfunction | Classifies the antibiotic as weakly effective. | [VERIFIED] | In scope |
| 1.9.1.4.3.4 | Resistant For Option | Resistant For | Option / Subfunction | Classifies the antibiotic as resisted. | [VERIFIED] | In scope |
| 1.9.1.4.4 | Report Element Visibility Control | — | Field / UI element | Controls whether the sensitivity, the reference and the commercial name appear in the report. | [VERIFIED] | In scope |
| 1.9.1.5 | Culture Action Block | — | Block / Container | The actions of the culture screen. | [VERIFIED] | In scope |
| 1.9.1.5.1 | Sensitivity Entry Action | — | Action / Function | Opens the sensitivity entry. | [VERIFIED] | In scope |
| 1.9.1.5.2 | Save Culture Data Action | — | Action / Function | Persists the culture data. | [VERIFIED] | In scope |
| 1.9.1.6 | Microbiology Report Output | MICROBIOLOGY REPORT | Output / Output | The printed microbiology report, carrying the culture and sensitivity table, the microscopic examination table with its reference ranges, the culture block and the sensitivity blocks for Highly For, Moderate For and Low For, each with a sensitive-inhibition-zone column and a commercial-name column. | [VERIFIED] | In scope |
| **1.10** | **Patient Search** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.10.1 | Search Screen | — | Screen / Container | Opened from the find-patient action or from the search icon of the ribbon. | [VERIFIED] | In scope |
| 1.10.1.1 | Search Criteria Block | — | Block / Container | The criteria by which a patient may be found; each criterion has its own entry field. | [VERIFIED] | In scope |
| 1.10.1.1.1 | Search by Patient Name Exact | بحث باسم المريض (مطابق | Function / Function | Finds a patient by an exactly matching name. | [VERIFIED] | In scope |
| 1.10.1.1.2 | Search by Patient Name Partial | بحث باسم المريض (عشوائي | Function / Function | Finds a patient by a partially matching name. | [VERIFIED] | In scope |
| 1.10.1.1.3 | Search by Treating Doctor | بحث باسم الطبيب المعالج | Function / Function | Finds patients by the name of the treating doctor. | [VERIFIED] | In scope |
| 1.10.1.1.4 | Search by Request Date | بحث بتاريخ الطلب | Function / Function | Finds patients by their request date. | [VERIFIED] | In scope |
| 1.10.1.1.5 | Search by Test Within Period | بالتحليل في الفترة المحددة | Function / Function | Finds patients who received a given test within a given period. | [VERIFIED] | In scope |
| 1.10.1.1.6 | Search by Phone Number | بالتليفون | Function / Function | Finds a patient by telephone number. | [VERIFIED] | In scope |
| 1.10.1.1.7 | Search by Card Number | برقم البطاقة | Function / Function | Finds a patient by card number. | [VERIFIED] | In scope |
| 1.10.1.1.8 | Search by Patient Age | بحث بسن المريض | Function / Function | Finds patients by their age. | [VERIFIED] | In scope |
| 1.10.1.1.9 | Search by Patient Gender | بحث بجنس المريض | Function / Function | Finds patients by their gender. | [VERIFIED] | In scope |
| 1.10.1.1.10 | Search Within Branch | Branch No. | Function / Function | Finds patients of one branch, by choosing the branch number; applicable in the branch deployment. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 1.10.1.1.11 | Search for Results Not Entered | نتائج لم تدخل | Function / Function | Finds results that have not been recorded. | [VERIFIED] | In scope |
| 1.10.1.1.12 | Search for Results Not Verified | نتائج لم تراجع | Function / Function | Finds results that have not been reviewed. | [VERIFIED] | In scope |
| 1.10.1.1.13 | Search for Results Not Printed | نتائج لم تطبع | Function / Function | Finds results that have not been printed. | [VERIFIED] | In scope |
| 1.10.1.1.14 | Search for Results Not Delivered | نتائج لم تسلم | Function / Function | Finds results that have not been delivered. | [VERIFIED] | In scope |
| 1.10.1.1.15 | Date Range Mode Options | مقيد / حر | Field group / Container | Two mutually exclusive options that decide whether the from and to dates of a dated criterion are bounded or free. | [VERIFIED] | In scope |
| 1.10.1.2 | Search Scope Block | — | Block / Container | The controls that decide the scope and the source of the search. | [VERIFIED] | In scope |
| 1.10.1.2.1 | Branch Selector | All | Field / UI element | Selects the branch; All is the option shown. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 1.10.1.2.2 | Source Selector | Database / Backup | Field / UI element | Selects the searched source, either the live data or the backup. | [VERIFIED] | In scope |
| 1.10.1.3 | Search Result Block | — | Block / Container | The result of the search and the controls that act on it. | [VERIFIED] | In scope |
| 1.10.1.3.1 | Search Result Grid | — | Grid / UI element | Columns referred by, unit, age, gender, date, patient name, laboratory identifier and identifier. | [VERIFIED] | In scope |
| 1.10.1.3.2 | Search Result Count | عدد مرضى عملية البحث | Indicator / UI element | The number of patients the search returned. | [VERIFIED] | In scope |
| 1.10.1.3.3 | New Search Action | New search | Action / Function | Starts a new search. | [VERIFIED] | In scope |
| 1.10.1.3.4 | Enter ID Field | <<Enter ID>> | Field / UI element | Where an identifier is typed to start a search. | [VERIFIED] | In scope |
| 1.10.1.4 | Search Action Bar | — | Field group / Container | The group of actions available on the search screen. | [VERIFIED] | In scope |
| 1.10.1.4.1 | Patient Tests Action | تحاليل المريض | Action / Function | Opens the patient's tests from the search result. | [VERIFIED] | In scope |
| 1.10.1.4.2 | Search Patient Data Action | بيانات المريض | Action / Function | Opens the patient's data from the search result. | [VERIFIED] | In scope |
| 1.10.1.4.3 | Search Lab Results Action | نتائج التحاليل | Action / Function | Opens the patient's laboratory results from the search result. | [VERIFIED] | In scope |
| 1.10.1.4.4 | Delete Patient Action | حذف المريض | Action / Function | Deletes the selected patient. | [VERIFIED] | In scope |
| 1.10.1.4.5 | Open Account Action | حساب مفتوح | Action / Function | Opens the account of the selected patient. | [VERIFIED] | In scope |
| 1.10.1.4.6 | Restrict To VIP Action | VIP | Action / Function | Restricts the search result to the VIP group. Its exact behaviour is not established. | [UNVERIFIED] | In scope |
| 1.10.1.4.7 | Add Action | اضافة | Action / Function | Adds a patient to the current selection. | [VERIFIED] | In scope |
| 1.10.1.4.8 | Collected Results Action | نتائج مجمعة | Action / Function | Opens the collected results of the selected patient. Its exact behaviour is not established. | [UNVERIFIED] | In scope |
| 1.10.1.4.9 | Modify Group Action | — | Action / Function | Modifies the selected group. Its exact behaviour is not established. | [UNVERIFIED] | In scope |
| 1.10.1.4.10 | Search Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list from the search screen. | [VERIFIED] | In scope |
| **1.11** | **Patient Visit History** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.11.1 | Laboratory ID Lifecycle | — | Block / Container | The life of the laboratory identifier that ties the visits of one patient together. | [VERIFIED] | In scope |
| 1.11.1.1 | Laboratory ID Generation | — | Function / Function | A laboratory identifier is created for the patient when the patient's data is registered, the tests are entered and the registration is saved. | [VERIFIED] | In scope |
| 1.11.1.2 | Laboratory ID Storage | — | Function / Function | The identifier is kept as a distinguished patient data item. | [VERIFIED] | In scope |
| 1.11.1.3 | Laboratory ID Display at Next Registration | — | Function / Function | The identifier is shown automatically at the next registration of the same patient. | [VERIFIED] | In scope |
| 1.11.2 | Visit History | — | Block / Container | The listing of the visits of one patient. | [VERIFIED] | In scope |
| 1.11.2.1 | Search by Laboratory ID | — | Function / Function | Searching by the identifier returns all of the patient's visits to the laboratory with their dates. | [VERIFIED] | In scope |
| 1.11.2.2 | Visit List Grid | — | Grid / UI element | Columns referred by, unit, age, gender, date, patient name, laboratory identifier and identifier. | [VERIFIED] | In scope |
| 1.11.2.3 | Visit Tests List | — | List / UI element | The tests performed in a selected visit. | [VERIFIED] | In scope |
| **1.12** | **Case Follow-up and Audit Trail** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.12.1 | Case History Screen | — | Screen / Container | Shows the recorded events of the case, with the user name and the date and time of each; the case audit trail is the recorded output of this screen. | [VERIFIED] | In scope |
| 1.12.1.1 | Registration Audit Entry | تم تسجيل المريض بواسطة | Field / UI element | Who registered the case, with the date and time. | [VERIFIED] | In scope |
| 1.12.1.2 | Modification Count | عدد مرات التعديل | Field / UI element | How many times the case was modified. | [VERIFIED] | In scope |
| 1.12.1.3 | Last Modifier Entry | اخر تعديل تم بواسطة | Field / UI element | Who made the last modification, with the date and time. | [VERIFIED] | In scope |
| 1.12.1.4 | Result Entry Audit Entry | سجل نتيجة التحليل | Field / UI element | Who entered the test results, with the date and time. | [VERIFIED] | In scope |
| 1.12.1.5 | Verification Audit Entry | تمت المراجعة بواسطة | Field / UI element | Who verified the results, with the date and time. | [VERIFIED] | In scope |
| 1.12.1.6 | Printing Count | عدد مرات الطباعة | Field / UI element | How many times the report was printed. | [VERIFIED] | In scope |
| 1.12.1.7 | Last Printer Entry | اخر طباعة تمت بواسطة | Field / UI element | Who printed the report last, with the date and time. | [VERIFIED] | In scope |
| 1.12.1.8 | Delivery Audit Entry | تم تسليم النتيجة بواسطة | Field / UI element | Who delivered the result, with the date and time. | [VERIFIED] | In scope |
| 1.12.1.9 | Patient Samples Block | عينات المريض | Block / Container | The block that lists the samples taken from the patient. | [VERIFIED] | In scope |
| 1.12.1.9.1 | Tube Name Column | Tube Name | Grid column / UI element | The name of the sample tube. | [VERIFIED] | In scope |
| 1.12.1.9.2 | Sample Tests Column | Tests | Grid column / UI element | The tests to be performed on the sample. | [VERIFIED] | In scope |
| 1.12.1.9.3 | Sample Take Action | Take | Action / Function | Records the taking of the sample. | [VERIFIED] | In scope |
| **1.13** | **Site Result Delivery** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.13.1 | Upload Screen | — | Screen / Container | Lists the patient's tests with their results, the patient's number and code, the upload action and the progress line. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.1.1 | Upload Patient Tests List | — | List / UI element | The patient's tests with their results, as listed for upload. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.1.2 | Upload Action | — | Action / Function | Transfers the selected patient's result to the site, where it becomes visible to the patient, the doctor or the laboratory. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.1.3 | Select All Action | — | Action / Function | Selects every listed test. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.1.4 | Unselect All Action | — | Action / Function | Clears the selection. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.1.5 | Upload Progress Indicator | — | Field / UI element | Reports that the upload is in progress and how many items it covers. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.1.6 | Block Result Action | — | Action / Function | Removes the selected patient's result from the site, where it is no longer available. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.1.7 | Mobile Number Requirement | — | Rule / Rule | The patient's telephone number must be entered before a result can be uploaded to the site. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.2 | Uploaded Patient Count | — | Function / Function | Shows how many patients had results uploaded within a period, and the total number of patients. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.3 | Site Access Channels | — | Function / Function | The site is reachable through the web page, through the mobile application published in the application store, and through the direct link given by the program. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.4 | Site Account Types | — | Field group / Container | The account types offered by the site. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.4.1 | Patient Account Type | — | Option / Subfunction | The account of a patient. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.4.2 | Doctor Account Type | — | Option / Subfunction | The account of a doctor. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.4.3 | Laboratory Account Type | — | Option / Subfunction | The account of a laboratory. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.5 | Site Sign-In Block | — | Block / Container | How each kind of user signs in to the site. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.5.1 | Patient Sign-In Data | — | Rule / Rule | A patient signs in with the laboratory code and the telephone number, or with the password alone. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.5.2 | Doctor Or Laboratory Sign-In Data | — | Rule / Rule | A doctor or a laboratory signs in with the laboratory code, the laboratory identifier and the password. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.5.3 | Site Sign In Action | — | Action / Function | Signs the user in to the site. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.6 | Site Result View | — | Block / Container | What the signed-in user sees of the result and how it leaves the site. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.6.1 | Result View Action | — | Action / Function | Opens the signed-in user's own result, grouped by test group, with a per-test report action. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.6.2 | Result Export Formats | — | Function / Function | The result may be printed or exported. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.6.2.1 | Print Format | — | Format / Output | The result is printed. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.6.2.2 | Word Document Format | — | Format / Output | The result is exported as a word document. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.6.2.3 | PDF Format | — | Format / Output | The result is exported as a PDF. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.6.2.4 | Image Format | — | Format / Output | The result is exported as an image. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.13.7 | Site Access Data On Receipt | — | Output / Output | The site address, the telephone number and the access code printed on the patient's receipt, so that the patient can collect the result from the site. The same data is carried by the site-access label, which is defined in group 1.14. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| **1.14** | **Barcode Labels and Specimen Containers** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.14.1 | Barcode Label Kinds | — | Block / Container | The kinds of barcode label the program produces for a patient. | [VERIFIED] | In scope |
| 1.14.1.1 | Specimen Container Label | ملصقات الانابيب والعبوات والمسحات | Output / Output | A label for the tube, container or swab. One label is printed for each specimen container, carrying the tests allocated to that container. The stated sticker size is 38 mm wide by 25 mm long. | [VERIFIED] | In scope |
| 1.14.1.2 | File Barcode Label | File barcode | Output / Output | The label affixed to the patient's file, used in the case where results are delivered by barcode. | [VERIFIED] | In scope |
| 1.14.1.3 | Laboratory ID Barcode Label | Lab ID barcode | Output / Output | The label affixed to the laboratory card. It carries the laboratory code, so that the medical history is easy to follow. | [VERIFIED] | In scope |
| 1.14.1.4 | Site Access Label | Site Access Label | Output / Output | The label by which the patient collects the result from the external service. It carries the service address, the patient's telephone number and the access password, with a barcode above the address. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 1.14.2 | Specimen Container Label Content | — | Block / Container | The elements printed on a specimen-container label. | [VERIFIED] | In scope |
| 1.14.2.1 | Patient Name on Label | — | Field / UI element | The patient's name, printed at the top of the label. | [VERIFIED] | In scope |
| 1.14.2.2 | Sex and Age Line on Label | — | Field / UI element | The patient's sex and age, printed as a single line under the name. | [VERIFIED] | In scope |
| 1.14.2.3 | Allocated Test List on Label | — | Field / UI element | The abbreviations of the tests allocated to that container, printed as a list. | [VERIFIED] | In scope |
| 1.14.2.4 | Label Barcode | — | Field / UI element | The barcode printed on the label of the container. | [VERIFIED] | In scope |
| 1.14.2.5 | Laboratory Identifier on Label | — | Field / UI element | The laboratory identifier of the visit, printed as a number under the barcode. | [VERIFIED] | In scope |
| 1.14.2.6 | Container Designation on Label | — | Field / UI element | The designation of the container the sample is held in, printed after the laboratory identifier. | [VERIFIED] | In scope |
| 1.14.2.7 | File Barcode Number | — | Field / UI element | The number printed under the file barcode and the laboratory-identifier barcode, composed of a leading digit, the laboratory code and a sequence number. Its exact composition is not established. | [UNVERIFIED] | In scope |
| 1.14.2.8 | Label Print Timestamp | — | Field / UI element | The date and time the label was printed, printed under the barcode of the file and laboratory-identifier labels. | [VERIFIED] | In scope |
| 1.14.3 | Specimen Container Designations | — | Block / Container | The container designations the specimen-container labels carry. | [VERIFIED] | In scope |
| 1.14.3.1 | Serum Fasting Container | Serum Fasting | Option / Subfunction | The container for the tests performed on a fasting serum sample. | [VERIFIED] | In scope |
| 1.14.3.2 | Serum Postprandial Container | Serum Postprandial | Option / Subfunction | The container for the tests performed on a serum sample taken after a meal. | [VERIFIED] | In scope |
| 1.14.3.3 | EDTA Blood Container | EDTA Blood | Option / Subfunction | The container for the tests performed on an EDTA blood sample. | [VERIFIED] | In scope |
| 1.14.3.4 | Citrated Blood Container | Citrated Blood 0.4 | Option / Subfunction | The container for the tests performed on a citrated blood sample. | [VERIFIED] | In scope |
| 1.14.3.5 | Urine Container | Urine | Option / Subfunction | The container for the tests performed on a urine sample. | [VERIFIED] | In scope |
| 1.14.3.6 | Stool Container | Stool | Option / Subfunction | The container for the tests performed on a stool sample. | [VERIFIED] | In scope |
| 1.14.3.7 | Semen Container | Semen | Option / Subfunction | The container for the tests performed on a semen sample. | [VERIFIED] | In scope |
| 1.14.4 | Test Distribution Across Containers | — | Block / Container | How the patient's tests are allocated over the materials and containers, and how the printed labels follow from that. | [VERIFIED] | In scope |
| 1.14.4.1 | Automatic Test Sorting | — | Function / Function | The program sorts the patient's tests by the material and the container they are performed on. | [VERIFIED] | In scope |
| 1.14.4.2 | Container Distribution Edit | — | Function / Function | The doctor can modify the distribution of the tests over the materials drawn from the patient and over the containers. | [VERIFIED] | In scope |
| 1.14.4.3 | Vertical Coding on Printed Documents | — | Function / Function | The vertical coding appears on the result reports, the receipt and the envelope; its appearance there is optional. | [VERIFIED] | In scope |
| 1.14.4.4 | Repeated Sticker Printing | — | Function / Function | The sticker can be printed more than once. | [VERIFIED] | In scope |
| **1.15** | **Patient Medical History and History Report** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.15.1 | Automatic History Insertion | — | Block / Container | When the patient has visited the laboratory before, the program can bring the results of the earlier tests into the current report. | [VERIFIED] | In scope |
| 1.15.1.1 | Automatic Previous Result Insertion | — | Function / Function | Where the patient has already had the same test performed, the program inserts the earlier result into the result report automatically. | [VERIFIED] | In scope |
| 1.15.1.2 | Multi-Test History Insertion | — | Function / Function | The history of several tests may be inserted into a result report alongside a group of tests from another group, or into a combined report. | [VERIFIED] | In scope |
| 1.15.1.3 | History Patient Identification | — | Block / Container | How the program recognises the patient when it builds the history. | [VERIFIED] | In scope |
| 1.15.1.3.1 | Identify By Patient Name | فرز وتجميع التاريخ المرضي باسم المريض | Option / Subfunction | The patient is recognised for the history by the patient's name. | [VERIFIED] | In scope |
| 1.15.1.3.2 | Identify By Laboratory Code | فرز وتجميع التاريخ المرضي حسب كود المعمل | Option / Subfunction | The patient is recognised for the history by the laboratory code. | [VERIFIED] | In scope |
| 1.15.1.4 | Automatic History Display Setting | Print Patient History Automatically | Field / UI element | In the report settings, controls whether the patient's history is inserted into the report automatically; turning it off stops the automatic display. | [VERIFIED] | In scope |
| 1.15.1.5 | History Sorting Method Setting | طريقة فرز وتجميع التاريخ المرضي | Field / UI element | In the report settings, chooses how the patient's history is sorted and grouped. | [VERIFIED] | In scope |
| 1.15.2 | History Test Selection | — | Block / Container | Choosing which of the patient's earlier results are brought into the report. | [VERIFIED] | In scope |
| 1.15.2.1 | Previous Visit Test List | — | List / UI element | The tests the patient had in the previous visits to the laboratory. | [VERIFIED] | In scope |
| 1.15.2.2 | Previous Test Selection Action | — | Action / Function | A test is selected for the history by pressing it twice. | [VERIFIED] | In scope |
| 1.15.2.3 | Previous Visit Count Indicator | — | Indicator / UI element | Reports how many earlier visits the patient has; the report screen shows the number of the patient's history. | [VERIFIED] | In scope |
| 1.15.2.4 | Repeated Test Note | — | Indicator / UI element | Notes that the patient has had a given test more than once. | [VERIFIED] | In scope |
| 1.15.2.5 | Previous Result Grid | Result status \ ·  Test result \ ·  Analysis date | Grid / UI element | Lists the earlier results of the selected test, with the result status, the result value and the date of the analysis. | [VERIFIED] | In scope |
| 1.15.3 | History Output Mode | — | Block / Container | The two ways the patient's history may be produced. | [VERIFIED] | In scope |
| 1.15.3.1 | Attach History In Result Report Option | Attach history in result report | Option / Subfunction | The selected earlier results are inserted into the result report itself. | [VERIFIED] | In scope |
| 1.15.3.2 | Print History In Separate Report Option | Print history in separate report | Option / Subfunction | The selected earlier results are produced as a report separate from the result report. | [VERIFIED] | In scope |
| 1.15.3.3 | History Mode Confirm Action | موافق | Action / Function | Confirms the chosen history output mode. | [VERIFIED] | In scope |
| 1.15.3.4 | History Mode Cancel Action | الغاء الحالى | Action / Function | Abandons the current history operation. | [VERIFIED] | In scope |
| 1.15.4 | Patient History Report | — | Block / Container | The report that carries the patient's earlier results. | [VERIFIED] | In scope |
| 1.15.4.1 | History Report Header | Patient History | Field group / Container | The header of the history report, carrying the report title, the period it covers, the user who printed it and the date it was printed. | [VERIFIED] | In scope |
| 1.15.4.1.1 | History Report Title | Patient History | Field / UI element | The title of the history report. | [VERIFIED] | In scope |
| 1.15.4.1.2 | History Date Range Field | History from ... to ... | Field / UI element | The period the history report covers. | [VERIFIED] | In scope |
| 1.15.4.1.3 | History Printed By Entry | Printed by | Field / UI element | The user who printed the history report. | [VERIFIED] | In scope |
| 1.15.4.1.4 | History Printed In Entry | Printed In | Field / UI element | The date the history report was printed. | [VERIFIED] | In scope |
| 1.15.4.2 | History Report Grid | Analysis Date \ ·  Patient ID \ ·  Patient Name | Grid / UI element | The grid of the history report: one row per analysis date, carrying the patient identity and one column per test whose history was selected. | [VERIFIED] | In scope |
| 1.15.4.2.1 | History Analysis Date Column | Analysis Date | Grid column / UI element | The date of the analysis the row reports. | [VERIFIED] | In scope |
| 1.15.4.2.2 | History Patient ID Column | Patient ID | Grid column / UI element | The laboratory identifier of the patient the row reports. | [VERIFIED] | In scope |
| 1.15.4.2.3 | History Patient Name Column | Patient Name | Grid column / UI element | The name of the patient the row reports. | [VERIFIED] | In scope |
| 1.15.4.2.4 | History Test Result Column | — | Grid column / UI element | One column per selected test, carrying that test's result on the row of its analysis date. | [VERIFIED] | In scope |
| 1.15.4.3 | Multi-Patient History Panel | No Of Patients | Panel / Container | The panel used to build a history for more than one patient in the same report. | [VERIFIED] | In scope |
| 1.15.4.3.1 | Multi-Patient History Grid | Patient Name \ ·  Patient ID \ ·  Analysis Date | Grid / UI element | The grid of the patients the history is built for. | [VERIFIED] | In scope |
| 1.15.4.3.2 | Multi-Patient History Count | No Of Patients | Indicator / UI element | The number of patients the history covers. | [VERIFIED] | In scope |
| 1.15.4.4 | History Test Lists | — | Block / Container | The two lists from which the tests to be given a history are chosen. | [VERIFIED] | In scope |
| 1.15.4.4.1 | History Tests In Lab List | Patient tests in our lab. | List / UI element | The tests available to be given a history, with their count. | [VERIFIED] | In scope |
| 1.15.4.4.2 | History Tests Available In Report List | Available tests in report | List / UI element | The tests already chosen to be given a history, with their count. | [VERIFIED] | In scope |
| 1.15.4.5 | Delete All History Action | حذف الكل | Action / Function | Removes every selection from the history report. | [VERIFIED] | In scope |
| 1.15.4.6 | History Report Print Action | طباعة | Action / Function | Prints the history report. | [VERIFIED] | In scope |
| 1.15.4.7 | History Report Preview Action | معاينة طباعة | Action / Function | Shows the history report before printing. | [VERIFIED] | In scope |
| **1.16** | **Main Tools and Utilities** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.16.1 | Main Tools Window | نافذة الأدوات الرئيسية | Screen / Container | The window of the laboratory's utilities. | [VERIFIED] | In scope |
| 1.16.1.1 | Tools Action Band | — | Block / Container | The band of utility entries carried by the tools window. | [VERIFIED] | In scope |
| 1.16.1.1.1 | Test Library Action | مكتبة التحاليل | Action / Function | Opens the library of the tests, from which the test data and the normal values are consulted and printed for the patient. | [VERIFIED] | In scope |
| 1.16.1.1.2 | Image Library Action | مكتبة الصور | Action / Function | Opens the library of the images. | [VERIFIED] | In scope |
| 1.16.1.1.3 | Abbreviations Dictionary Action | قاموس للإختصارات | Action / Function | Opens the dictionary of the abbreviations used in the clinical tests. | [VERIFIED] | In scope |
| 1.16.1.1.4 | Stopwatch Action | ساعة التوقيت Stopwatch | Action / Function | Opens the stopwatch. | [VERIFIED] | In scope |
| 1.16.1.1.5 | Unit Converter Action | محول وحدات القياس | Action / Function | Opens the converter of the measurement units. | [VERIFIED] | In scope |
| 1.16.1.1.6 | Work And Laboratory List Action | قائمة الأمثلة والمختبرات | Action / Function | Opens the list of the work items and the laboratories. The label is only partly legible in the source. | [UNVERIFIED] | In scope |
| 1.16.1.1.7 | Reference Weight Tables Action | توزيع الأوزان المرجعية | Action / Function | Opens the reference weight tables. The label is only partly legible in the source. | [UNVERIFIED] | In scope |
| 1.16.1.1.8 | Data Manual Action | دليل البيانات | Action / Function | Opens the manual of the data. | [VERIFIED] | In scope |
| 1.16.1.1.9 | Calculator Action | آلة الحاسبة | Action / Function | Opens the calculator. | [VERIFIED] | In scope |
| 1.16.1.2 | Tools Capability Block | — | Block / Container | The list of what the tools window allows. | [VERIFIED] | In scope |
| 1.16.1.2.1 | Test And Normal Ranges Reference Capability | — | Function / Function | The data of the tests is consulted and printed, together with the normal ranges, for the patient. | [VERIFIED] | In scope |
| 1.16.1.2.2 | Clinical Terminology Capability | — | Function / Function | The terms used in the clinical tests are consulted. | [VERIFIED] | In scope |
| 1.16.1.2.3 | Work Request Entry Capability | — | Function / Function | Work items and the tests of the laboratory are entered through the work list. | [VERIFIED] | In scope |
| 1.16.1.2.4 | Stopwatch Calculator And Converter Capability | — | Function / Function | The stopwatch, the calculator and the converter of units are used. | [VERIFIED] | In scope |
| 1.16.1.2.5 | Reminder Message Capability | — | Function / Function | A reminder message is composed for a given user. The wording of this entry is only partly legible in the source. | [UNVERIFIED] | In scope |
| 1.16.1.2.6 | Data Manual Search Capability | — | Function / Function | The manual of the data is searched by a word or by a known value. The wording of this entry is only partly legible in the source. | [UNVERIFIED] | In scope |
| **1.17** | **Foreign Report** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.17.1 | Foreign Report Window | — | Screen / Function | The window in which the foreign report is prepared. The window is not shown in the source. | [NOT-SHOWN] | In scope |
| 1.17.2 | Foreign Report Window Rule | — | Rule / Rule | The program opens the foreign-report window, into which the data may be entered and printed, or a sheet carrying the patient's data may be printed. | [VERIFIED] | In scope |
| 1.17.3 | Foreign Report Sheet Output | — | Output / Output | A sheet carrying the patient's data. The sheet is not shown in the source. | [NOT-SHOWN] | In scope |
| **1.18** | **Printed Delivery Sheet and Order Slip** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.18.1 | Delivery Sheet Header Block | — | Block / Container | The identification printed at the head of the delivery sheet. | [VERIFIED] | In scope |
| 1.18.1.1 | Delivery Sheet Patient Id Field | ID | Field / UI element | The patient's identifier, printed as a barcode. | [VERIFIED] | In scope |
| 1.18.1.2 | Delivery Sheet Patient Name Field | Pat. Name | Field / UI element | The patient's name. | [VERIFIED] | In scope |
| 1.18.1.3 | Delivery Sheet Age Sex Field | Age / Sex | Field / UI element | The patient's age and sex. | [VERIFIED] | In scope |
| 1.18.1.4 | Delivery Sheet Telephone Field | Telephone | Field / UI element | The patient's telephone number. | [VERIFIED] | In scope |
| 1.18.1.5 | Delivery Sheet Referred By Field | Referred by | Field / UI element | The referring doctor. | [VERIFIED] | In scope |
| 1.18.1.6 | Delivery Sheet Date Field | Date | Field / UI element | The date of the request. | [VERIFIED] | In scope |
| 1.18.1.7 | Delivery Sheet Time Field | Time | Field / UI element | The time of the request. | [VERIFIED] | In scope |
| 1.18.1.8 | Delivery Sheet Total Field | Total | Field / UI element | The total of the request. | [VERIFIED] | In scope |
| 1.18.1.9 | Delivery Sheet Paid Field | Paid | Field / UI element | The amount paid. | [VERIFIED] | In scope |
| 1.18.1.10 | Delivery Sheet Charge Field | Charge | Field / UI element | The amount still owed. | [VERIFIED] | In scope |
| 1.18.2 | Delivery Sheet Test Grid | Test Name  ·  Unit  ·  Result  ·  Collection notes  ·  Sig. | Grid / UI element | The grid of the tests printed on the delivery sheet; one row is one test. | [VERIFIED] | In scope |
| 1.18.2.1 | Delivery Sheet Test Name Column | Test Name | Grid column / UI element | The name of the test. | [VERIFIED] | In scope |
| 1.18.2.2 | Delivery Sheet Department Column | Unit | Grid column / UI element | The column headed Unit, which carries the department the test belongs to, such as the chemistry, the clinical chemistry, the microbiology, the haematology, the serology or the virology. | [VERIFIED] | In scope |
| 1.18.2.3 | Delivery Sheet Result Column | Result | Grid column / UI element | The result of the test. | [VERIFIED] | In scope |
| 1.18.2.4 | Delivery Sheet Collection Notes Column | Collection notes | Grid column / UI element | The column that carries the requirement of the sample, such as the volume, the container and the conditions of the sample. | [VERIFIED] | In scope |
| 1.18.2.5 | Delivery Sheet Signature Column | Sig. | Grid column / UI element | The column in which the signature of the responsible person is placed. | [VERIFIED] | In scope |
| 1.18.3 | Delivery Sheet Signature Roles | — | Block / Container | The two roles that sign the delivery sheet. | [VERIFIED] | In scope |
| 1.18.3.1 | Samples Responsible Role | مسئول العينات | Field / UI element | The role of the person responsible for the samples. | [VERIFIED] | In scope |
| 1.18.3.2 | Reception Staff Role | موظف الاستقبال | Field / UI element | The role of the reception staff. | [VERIFIED] | In scope |
| 1.18.4 | Per Patient Order Slip Printing | — | Function / Function | An order slip is printed for each patient separately, so that the slip is not lost and so that the follow-up and the archiving of the file are easier. | [VERIFIED] | In scope |
| **1.19** | **Printed Result Report Structure** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.19.1 | Report Summary Header Block | — | Block / Container | The block printed at the head of the result report. | [VERIFIED] | In scope |
| 1.19.1.1 | Report Header Patient Name Field | اسم المريض | Field / UI element | The patient's name. | [VERIFIED] | In scope |
| 1.19.1.2 | Report Header Net Total Field | الإجمالي بعد الخصم | Field / UI element | The total of the request after the discount. | [VERIFIED] | In scope |
| 1.19.1.3 | Report Header Amount Paid Field | قيمة وقدرها | Field / UI element | The amount that was paid. | [VERIFIED] | In scope |
| 1.19.1.4 | Report Header Remainder Field | الباقي للمريض | Field / UI element | The amount that remains for the patient. | [VERIFIED] | In scope |
| 1.19.1.5 | Report Header Treating Doctor Field | الطبيب المعالج | Field / UI element | The doctor treating the patient. | [VERIFIED] | In scope |
| 1.19.1.6 | Report Header Result Receipt Date Field | تاريخ استلام النتيجة | Field / UI element | The date and the time at which the result is received. | [VERIFIED] | In scope |
| 1.19.1.7 | Report Header Account Type Field | نوع الحساب | Field / UI element | The account type the patient is handled under. | [VERIFIED] | In scope |
| 1.19.1.8 | Report Header Patient Code Field | كود المريض | Field / UI element | The patient's code, printed as a barcode. | [VERIFIED] | In scope |
| 1.19.2 | Report Test Group Table | Test Group  ·  No. Of test  ·  Price | Grid / UI element | The table that lists the groups of the request, the number of tests in each group and the price of the group. | [VERIFIED] | In scope |
| 1.19.2.1 | Report Test Group Column | Test Group | Grid column / UI element | The name of the group of the request. | [VERIFIED] | In scope |
| 1.19.2.2 | Report Test Count Column | No. Of test | Grid column / UI element | The number of tests in the group. | [VERIFIED] | In scope |
| 1.19.2.3 | Report Test Price Column | Price | Grid column / UI element | The price of the group. | [VERIFIED] | In scope |
| 1.19.3 | Report Paper Variants | — | Rule / Rule | The receipt is printed on the laboratory's own paper and on an ordinary printer, or on plain paper with a head, a foot and the logo added. | [VERIFIED] | In scope |
| **1.20** | **Profile Report Layout** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.20.1 | Report Patient Block | Patient Name  ·  Patient ID.  ·  Age / Sex  ·  Request Date  ·  Referred By  ·  Printed In | Block / Container | The block at the head of the report that identifies the patient and the request, carrying the patient's identifier as a barcode and the moment the report was printed. | [VERIFIED] | In scope |
| 1.20.2 | Report Title Line | — | Field / UI element | The centred title of the report, which names the test family the report covers. | [VERIFIED] | In scope |
| 1.20.3 | Report Section Heading | — | Field / UI element | The heading of a named sub-section of the report, such as the physical, the chemical and the microscopic examination of a urine report. | [VERIFIED] | In scope |
| 1.20.4 | Report Sub Item Row | — | Grid row / UI element | One line of a sub-section, naming the sub-item and carrying its result. | [VERIFIED] | In scope |
| 1.20.5 | Report Reference Range Column | Reference Range | Grid column / UI element | The column that carries the range the result is read against; the range may occupy more than one line, as the bands of a pregnancy test do. | [VERIFIED] | In scope |
| 1.20.6 | Report Flag Column | L or H | Grid column / UI element | The column that carries the state of the result against its range, shown as the abbreviated or the full form of the state. | [VERIFIED] | In scope |
| 1.20.7 | Report Emphasised Result | — | Indicator / UI element | A result that is marked on the printed report, whether by a shaded cell, a heavier stroke, or both. | [VERIFIED] | In scope |
| 1.20.8 | Report Secondary Mark Cell | — | Field / UI element | The cell beside a result that carries a second mark, such as a number of crosses or a qualitative quantity. | [VERIFIED] | In scope |
| 1.20.9 | Report Interpretation Block | — | Block / Container | The block of interpretation printed under the results, as the interpretation of a widal test. | [VERIFIED] | In scope |
| 1.20.10 | Report Signature Line | Doctor's signature: | Field / UI element | The line at the foot of the report that carries the signature of the doctor. | [VERIFIED] | In scope |
| 1.20.11 | Report Closing Line | Thanks | Field / UI element | The closing line printed under the signature. | [VERIFIED] | In scope |
| 1.20.12 | Report Laboratory Footer | — | Block / Container | The foot of the report that carries the laboratory's name, its address and its contact details. | [VERIFIED] | In scope |
| 1.20.13 | Report Pagination Line | Current Page 1 from 1 | Field / UI element | The line that carries the number of the page and the number of pages the report runs to. | [VERIFIED] | In scope |
| 1.20.14 | Report Header Band Option | — | Option / Subfunction | The band that carries the laboratory's logo and its title may be printed at the head of the report or left out, so that a report may be printed on plain paper. | [VERIFIED] | In scope |
| **1.21** | **Report Behaviour Rules** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.21.1 | Patient Reference Override Rule | — | Rule / Rule | The reference values of a single patient may be changed while the report is printed, or from the system data for every patient. | [VERIFIED] | In scope |
| 1.21.2 | Normal Range Length Rule | — | Rule / Rule | Up to two hundred and fifty-five characters may be entered for the normal range. | [VERIFIED] | In scope |
| 1.21.3 | Report Paper Choice Rule | — | Rule / Rule | The report may be printed on pre-printed paper or on plain paper, on which the head and the foot and the logo are added. | [VERIFIED] | In scope |
| 1.21.4 | Report Language Side Rule | — | Rule / Rule | The side on which the Arabic name and its translation are printed may be chosen by the doctor, and the composition of the report may be changed freely. | [VERIFIED] | In scope |
| 1.21.5 | Test Interpretation Rule | — | Rule / Rule | In some tests an interpretation may be written about the method of the test and about its result, and it is applied to the whole patient. | [VERIFIED] | In scope |
| 1.21.6 | Report Chart And Drawing Rule | — | Rule / Rule | The graph and the explanatory drawing of a report may be shown in it or withheld from it. | [VERIFIED] | In scope |
| 1.21.7 | Report Second Page Rule | — | Rule / Rule | The second page of a report is left out when a value or an element has not been entered. | [VERIFIED] | In scope |
| 1.21.8 | Single Sheet Report Rule | — | Rule / Rule | The analysis may be printed on a single sheet without the graphs and without the explanatory drawings. | [VERIFIED] | In scope |
| **1.22** | **Composite Test Entry Screen** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.22.1 | Composite Screen Test Banner | — | Field / UI element | The banner that carries the name of the test whose sub-items are being entered. | [VERIFIED] | In scope |
| 1.22.2 | Composite Screen Section Heading | — | Field / UI element | The heading of a named group of sub-items, as the physical, the chemical and the microscopic examination of a urine test. | [VERIFIED] | In scope |
| 1.22.3 | Composite Screen Item Row | — | Grid row / UI element | One sub-item of the test, carrying its own result and its own range. | [VERIFIED] | In scope |
| 1.22.4 | Composite Screen Item Check Box | — | Field / UI element | The mark beside a sub-item that decides whether the sub-item is entered at all. | [VERIFIED] | In scope |
| 1.22.5 | Composite Screen Item Value List | — | Field / UI element | The list of the values a sub-item may take. | [VERIFIED] | In scope |
| 1.22.6 | Composite Screen Unordered Item | — | Indicator / UI element | A sub-item of the test that was not ordered for this patient; it is shown without a result and without a mark. | [VERIFIED] | In scope |
| 1.22.7 | Composite Screen Resolved Range | — | Field / UI element | The range the patient's own sex and age resolve to, shown in the row of the sub-item. | [VERIFIED] | In scope |
| 1.22.8 | Composite Screen Semi Quantitative Control | — | Field / UI element | The control that carries a value together with a comparison symbol and a number, for a sub-item whose result is a band. | [UNVERIFIED] | In scope |
| 1.22.9 | Composite Screen Second Result Row | — | Grid row / UI element | A second result row under one sub-item, for a sub-item that may carry more than one result. | [VERIFIED] | In scope |
| 1.22.10 | Composite Screen Extra Value Field | — | Field / UI element | The extra value field carried at the end of some sub-item rows. | [VERIFIED] | In scope |
| 1.22.11 | Composite Screen Comment Block | — | Block / Container | The comment recorded against the test, with the mark that carries it and the control that fills it from the patient's history. | [VERIFIED] | In scope |
| 1.22.12 | Composite Screen Action Block | — | Block / Container | The actions of the composite entry screen. | [VERIFIED] | In scope |
| 1.22.12.1 | Composite Screen Save Action | حفظ | Action / Function | Persists the entered results. | [VERIFIED] | In scope |
| 1.22.12.2 | Composite Screen Print Action | طباعة | Action / Function | Prints the report of the test. | [VERIFIED] | In scope |
| 1.22.12.3 | Composite Screen Preview Action | معاينة الطباعة | Action / Function | Shows the report of the test before it is printed. | [VERIFIED] | In scope |
| 1.22.12.4 | Composite Screen Review Action | مراجعة | Action / Function | Opens the result for review. | [VERIFIED] | In scope |
| 1.22.12.5 | Composite Screen Main Result Action | النتيجة الرئيسية | Action / Function | Opens the main result of the request. | [VERIFIED] | In scope |
| 1.22.13 | Blood Picture Entry Block | Blood Picture | Block / Container | The block that carries the derived values of a blood picture. | [VERIFIED] | In scope |
| 1.22.13.1 | Blood Picture Calculated Index Row | Hct  ·  MCV  ·  MCH  ·  MCHC  ·  RDW  ·  hgb %  ·  Mentzer Index | Grid row / UI element | One of the indices the program computes from the entered values. | [VERIFIED] | In scope |
| 1.22.13.2 | Blood Picture Index Inclusion Mark | — | Field / UI element | The mark beside a computed index that decides whether the index is carried into the report. | [VERIFIED] | In scope |
| 1.22.13.3 | Blood Picture Differential Block | — | Block / Container | The block that carries the differential count. | [VERIFIED] | In scope |
| 1.22.13.3.1 | Blood Picture Differential Row | Neutrophil  ·  Lymphocyte  ·  Monocyte  ·  Eosinophil  ·  Basophil | Grid row / UI element | One line of the differential count. | [VERIFIED] | In scope |
| 1.22.13.3.2 | Blood Picture Relative Count Column | Relative count | Grid column / UI element | The share of the differential row, given as a percentage. | [VERIFIED] | In scope |
| 1.22.13.3.3 | Blood Picture Absolute Count Column | Absolute count | Grid column / UI element | The absolute count of the differential row. | [VERIFIED] | In scope |
| 1.22.13.3.4 | Blood Picture Other Cells Row | Other Cells | Grid row / UI element | The line that carries the cells that fall outside the named groups of the differential. | [VERIFIED] | In scope |
| 1.22.13.3.5 | Blood Picture Segmented And Band Rows | Segmented  ·  Band | Grid row / UI element | The lines that separate the segmented forms from the band forms of a differential row. | [VERIFIED] | In scope |
| 1.22.13.4 | Blood Picture Immature Cell Block | NRBC/100 WBC  ·  Promyelocytes  ·  Myelocytes  ·  Juvenile  ·  Blast | Block / Container | The block that carries the immature forms, each with its own value. | [VERIFIED] | In scope |
| 1.22.13.5 | Auto Comment Option | Enable auto comment | Field / UI element | The mark that makes the program write the comment of the report by itself, and it may be left out. | [VERIFIED] | In scope |
| 1.22.13.6 | Auto Comment Basis Option | By MCHC | Field / UI element | The mark that chooses the basis on which the program writes the comment of the report. | [VERIFIED] | In scope |
| 1.22.13.7 | Constants Panel | CONSTANTS | Block / Container | The panel of the constants of the blood picture. | [VERIFIED] | In scope |
| **1.23** | **Culture And Sensitivity Report** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.23.1 | Culture Report Examination Block | — | Block / Container | The block that carries the examination of the sample the culture was grown from. | [VERIFIED] | In scope |
| 1.23.2 | Culture Report Culture Block | Sample  ·  Organism A  ·  Culture Condition  ·  Colony Count | Block / Container | The block that carries the sample, the organism that grew, the condition the culture was kept in and the count of the colonies. | [VERIFIED] | In scope |
| 1.23.3 | Culture Report Sensitivity Block | — | Block / Container | The block that carries the sensitivity of each antibiotic, divided into the antibiotics that are highly active, those that are moderately active and those to which the organism is resistant. | [VERIFIED] | In scope |
| 1.23.3.1 | Culture Report Sensitivity Heading | Highly For (Organism A)  ·  Moderate For (Organism A)  ·  Resistant For (Organism A) | Field / UI element | The heading of one of the three tables of the sensitivity block. | [VERIFIED] | In scope |
| 1.23.3.2 | Culture Report Antibiotic Row | — | Grid row / UI element | One antibiotic, with the value of its sensitivity, the zone of inhibition and the commercial names under which it is sold. | [VERIFIED] | In scope |
| 1.23.3.3 | Culture Report Inhibition Zone Column | Inhibition zone | Grid column / UI element | The zone of inhibition recorded for the antibiotic. | [VERIFIED] | In scope |
| 1.23.3.4 | Culture Report Commercial Name Column | Commercial Name | Grid column / UI element | The commercial names of the antibiotic. | [VERIFIED] | In scope |
| 1.23.4 | Culture Report Trade Name Rule | — | Rule / Rule | The trade names of an antibiotic may be added for the part of the culture that is controlled, and they are printed only where they are entered. | [VERIFIED] | In scope |
| **1.24** | **Semen Morphology Report Page** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.24.1 | Semen Motility Profile Block | Grade A  ·  Grade B  ·  Grade C  ·  Grade D  ·  Grade A+B | Block / Container | The grading of the motility over a time point, with the share of each grade and its concentration. | [VERIFIED] | In scope |
| 1.24.2 | Semen Motility Profile Chart | — | Output / Output | The chart drawn for the grading of the motility. | [VERIFIED] | In scope |
| 1.24.3 | Semen Normal Grade Line | Normal: Grade A > 25% or Grade (A+B) >= 50% | Field / UI element | The condition printed under the grading of the motility, which states the share of the normal grades. | [VERIFIED] | In scope |
| 1.24.4 | Semen Dead Forms Field | Dead Forms In First Hour | Field / UI element | The share of the dead forms at the time point. | [VERIFIED] | In scope |
| 1.24.5 | Semen Abnormal Forms Block | Head Defects  ·  Mid piece defects  ·  Tail defects  ·  Micro spermia | Block / Container | The breakdown of the abnormal forms of the spermatozoa. | [VERIFIED] | In scope |
| 1.24.6 | Semen Abnormal Forms Chart | — | Output / Output | The chart drawn for the breakdown of the abnormal forms. | [VERIFIED] | In scope |
| 1.24.7 | Semen Morphology Drawing Panel | — | Output / Output | One of the panels that show the forms of the spermatozoa, with the share of each form. | [VERIFIED] | In scope |
| **1.25** | **Image Report** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 1.25.1 | Image Report Sheet | — | Output / Output | The sheet that carries the images. It is printed apart from the report of the analysis it belongs to. | [VERIFIED] | In scope |
| 1.25.2 | Image Report Image Count | — | Field / UI element | The number of images the sheet carries, from one to four. | [VERIFIED] | In scope |
| 1.25.3 | Image Report Image | — | Field / UI element | One of the microscope images carried on the sheet. | [VERIFIED] | In scope |
| 1.25.4 | Image Report Annotation | — | Field / UI element | A shape, a number or a word placed on an image or beside it. | [VERIFIED] | In scope |
| 1.25.5 | Image Report Annotation Placement | — | Field / UI element | The position of an annotation on the sheet. | [VERIFIED] | In scope |
| 1.25.6 | Image Report Annotation Size | — | Field / UI element | The size of an annotation. | [VERIFIED] | In scope |
| 1.25.7 | Image Report Annotation Colour | — | Field / UI element | The colour given to an annotation. | [VERIFIED] | In scope |
| 1.25.8 | Image Report Entry Rule | — | Rule / Rule | The sheet of images is made from inside the urine, the stool, the semen, the blood-picture and the culture analyses. | [VERIFIED] | In scope |

### §6.3 Workflows

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

### §6.4 Rules, validations and conditional behaviour

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

### §6.5 Outputs

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

### §6.6 Screens and interface structure

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

### §6.7 Uncertainties and unshown elements

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

## §7 Unit 2 — Work Sheet for Patients or Tests

### §7.1 Functional hierarchy

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
├── 2.5  Work Sheet Documents
│   ├── 2.5.1  Common Document Elements
│   │   ├── 2.5.1.1  Laboratory Header Block
│   │   │   └── 2.5.1.1.1  Laboratory Printed Header
│   │   ├── 2.5.1.2  Printed By Entry
│   │   ├── 2.5.1.3  Print Timestamp
│   │   ├── 2.5.1.4  Zoom Setting
│   │   └── 2.5.1.5  Page Count Entry
│   ├── 2.5.2  Work Paper by Patient ID
│   │   ├── 2.5.2.1  Work Paper Patient Title
│   │   ├── 2.5.2.2  Patient Sheet Count Entry
│   │   ├── 2.5.2.3  Work Paper Patient ID Column
│   │   ├── 2.5.2.4  Work Paper Patient Name Column
│   │   ├── 2.5.2.5  Work Paper Gender Column
│   │   ├── 2.5.2.6  Work Paper Age Column
│   │   ├── 2.5.2.7  Work Paper Date Column
│   │   ├── 2.5.2.8  Work Paper Referred By Column
│   │   ├── 2.5.2.9  Reception Officer Signature Area
│   │   ├── 2.5.2.10  Samples Officer Signature Area
│   │   └── 2.5.2.11  Work Paper Patient Entry
│   ├── 2.5.3  Work Paper by Test Name
│   │   ├── 2.5.3.1  Work Paper Test Title
│   │   ├── 2.5.3.2  Test Type Count Entry
│   │   ├── 2.5.3.3  Work Paper Test Code Column
│   │   ├── 2.5.3.4  Work Paper Test Name Column
│   │   ├── 2.5.3.5  Test Work Paper Patient Name Column
│   │   ├── 2.5.3.6  Work Paper Result Column
│   │   ├── 2.5.3.7  Test Work Paper Patient ID Column
│   │   └── 2.5.3.8  Patient Count For Test Entry
│   └── 2.5.4  Work Paper for Selected Test
│       ├── 2.5.4.1  Selected Test Work Paper Title
│       ├── 2.5.4.2  Selected Test Code Entry
│       ├── 2.5.4.3  Selected Test Name Entry
│       ├── 2.5.4.4  Selected Test Patient ID Column
│       ├── 2.5.4.5  Selected Test Patient Name Column
│       ├── 2.5.4.6  Selected Test Patient Result Column
│       ├── 2.5.4.7  Selected Test Signature Column
│       ├── 2.5.4.8  Selected Test Patient Count Entry
│       ├── 2.5.4.9  Selected Test Reception Officer Signature Area
│       └── 2.5.4.10  Selected Test Samples Officer Signature Area
└── 2.6  Printed Work Paper Structure
    ├── 2.6.1  Work Paper Print Header Block
    │   ├── 2.6.1.1  Work Paper Patient Count Field
    │   ├── 2.6.1.2  Work Paper Printed By Field
    │   └── 2.6.1.3  Work Paper Printed At Field
    ├── 2.6.2  Work Paper Pagination Field
    ├── 2.6.3  Work Paper Patient Block
    │   ├── 2.6.3.1  Work Paper Patient Name Field
    │   ├── 2.6.3.2  Work Paper Patient Gender Field
    │   ├── 2.6.3.3  Work Paper Patient Age Field
    │   ├── 2.6.3.4  Work Paper Patient Age Unit Field
    │   ├── 2.6.3.5  Work Paper Patient Ref By Field
    │   ├── 2.6.3.6  Work Paper Patient Total Field
    │   ├── 2.6.3.7  Work Paper Patient Paid Field
    │   ├── 2.6.3.8  Work Paper Patient Barcode Field
    │   └── 2.6.3.9  Work Paper Patient Ok Column
    ├── 2.6.4  Work Paper Test Block
    │   ├── 2.6.4.1  Work Paper Test Code Field
    │   ├── 2.6.4.2  Work Paper Test Name Field
    │   ├── 2.6.4.3  Work Paper Test Patient Count Field
    │   ├── 2.6.4.4  Work Paper Test Patient Name Column
    │   ├── 2.6.4.5  Work Paper Test Result Column
    │   └── 2.6.4.6  Work Paper Test Id Column
    ├── 2.6.5  Work Paper First And Last Patient Rule
    └── 2.6.6  Unfinished Tests Work Paper Option
```

### §7.2 Function inventory

The elements of the unit, each defined once, at the place that carries it. The *Type / Level* column gives the kind of the element and the level at which it is recorded under the policy of §3. The *Scope* column marks the elements that belong to one of the five excluded categories of §2; they are documented, and they are not parity requirements.

| ID | Canonical name | Source label | Type / Level | Definition | Evidence | Scope |
|---|---|---|---|---|---|---|
| **2.1** | **Work Sheet Module Entry** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 2.1.1 | Work Sheet Ribbon Item | Work sheet | Navigation / Subfunction | The ribbon item that opens the work sheet module. | [VERIFIED] | In scope |
| 2.1.2 | Work Sheet Menu | — | Menu / UI element | The choice presented when the module is opened: a work sheet by patient names, or a work sheet by test names with the group log. | [VERIFIED] | In scope |
| 2.1.2.1 | Work Sheet By Patient Names Item | ورقة عمل بأسماء المرضى | Menu item / Subfunction | Opens the work sheet that is built from a set of patients. | [VERIFIED] | In scope |
| 2.1.2.2 | Work Sheet By Test Names Item | ورقة عمل بأسماء التحاليل (Log) | Menu item / Subfunction | Opens the work sheet that is built from a set of tests. | [VERIFIED] | In scope |
| 2.1.3 | Work Sheet Capabilities | — | Block / Container | The kinds of work sheet the module produces. | [VERIFIED] | In scope |
| 2.1.3.1 | Patient Period Work Sheet | — | Function / Function | A work sheet for a group of patients within a chosen period, carrying them by their names or their codes. | [VERIFIED] | In scope |
| 2.1.3.2 | Test Period Work Sheet | — | Function / Function | A work sheet for a single test within a chosen period, or for a group of tests that have been selected. | [VERIFIED] | In scope |
| 2.1.3.3 | Work Group Work Sheet | — | Function / Function | A work sheet for a work group within the chosen period. | [VERIFIED] | In scope |
| 2.1.3.4 | Test Period Classification | — | Function / Function | A classification of the tests of a chosen period, showing how many times each test was performed in that period. | [VERIFIED] | In scope |
| 2.1.3.5 | Unfinished Only Selection | — | Function / Function | The work sheet may be produced for the unfinished tests alone, or for all of the tests. | [VERIFIED] | In scope |
| **2.2** | **Work Sheet by Patient Names** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 2.2.1 | Work Sheet by Patient Names Screen | ورقة عمل بأسماء المرضى | Screen / Container | Through this window a work sheet is made for the current day, for a chosen period, or for a chosen group of patients taken by name or by code. | [VERIFIED] | In scope |
| 2.2.1.1 | Patient Work Sheet Mode Options | — | Field group / Container | The two forms the patient work sheet may take. | [VERIFIED] | In scope |
| 2.2.1.1.1 | Full Patient Names Sheet Option | ورقة عمل حسب أسماء مرضى الفترة المحددة (كاملة) | Option / Subfunction | The work sheet carries the full record of each patient of the chosen period. | [VERIFIED] | In scope |
| 2.2.1.1.2 | Abbreviated Patient Names Sheet Option | ورقة عمل حسب أسماء مرضى الفترة المحددة (مختصرة) | Option / Subfunction | The work sheet carries a shortened record of each patient of the chosen period. | [VERIFIED] | In scope |
| 2.2.1.2 | Patient Sheet Filter Block | — | Block / Container | The filters that decide which patients the work sheet covers. | [VERIFIED] | In scope |
| 2.2.1.2.1 | Patient Sheet Referring Entity Selector | جهة الإحالة | Field / UI element | Selects the referring entity whose patients the work sheet covers. | [VERIFIED] | In scope |
| 2.2.1.2.2 | Patient Sheet From Code Field | من كود رقم | Field / UI element | The code the patient range starts at. | [VERIFIED] | In scope |
| 2.2.1.2.3 | Patient Sheet To Code Field | الى كود رقم | Field / UI element | The code the patient range ends at. | [VERIFIED] | In scope |
| 2.2.1.2.4 | Patient Sheet From Date Field | من تاريخ | Field / UI element | The date the period starts at. | [VERIFIED] | In scope |
| 2.2.1.2.5 | Patient Sheet To Date Field | الى | Field / UI element | The date the period ends at. | [VERIFIED] | In scope |
| 2.2.1.3 | Patient Sheet List Block | — | Block / Container | The list of patients the work sheet will cover. | [VERIFIED] | In scope |
| 2.2.1.3.1 | Patient Sheet Patient Grid | All \ ·  Patient ID \ ·  Patient Name | Grid / UI element | The list of patients, with the columns all, the patient identifier and the patient name, and with the count of the patients it holds. | [VERIFIED] | In scope |
| 2.2.1.3.2 | Patient Sheet Patient Count | — | Indicator / UI element | The number of patients in the list. | [VERIFIED] | In scope |
| 2.2.1.3.3 | Patient Name Double-Click Removal Action | — | Action / Function | A patient is removed from the list by pressing their name twice. | [VERIFIED] | In scope |
| 2.2.1.4 | Patient Sheet Print Option Block | — | Block / Container | The options that decide what the work sheet contains. | [VERIFIED] | In scope |
| 2.2.1.4.1 | Patient Sheet Unfinished Tests Only Option | طباعة التحاليل الغير منتهية فقط (عند اختيار مجموعة مرضى) | Option / Subfunction | The work sheet carries the unfinished tests alone; the option applies when a group of patients is selected. | [VERIFIED] | In scope |
| 2.2.1.4.2 | Print Selected Patient Sheet Only Option | طباعة ورقة العمل الخاصة بالمريض المحدد فقط | Option / Subfunction | The work sheet covers the selected patient alone. | [VERIFIED] | In scope |
| 2.2.1.5 | Patient Sheet Action Block | — | Block / Container | The actions of the patient work sheet window. | [VERIFIED] | In scope |
| 2.2.1.5.1 | Patient Sheet Print Action | Print | Action / Function | Prints the patient work sheet. | [VERIFIED] | In scope |
| 2.2.1.5.2 | Patient Sheet Refresh Action | Refresh | Action / Function | Reloads the patient list. | [VERIFIED] | In scope |
| 2.2.1.5.3 | Patient Sheet Preview Action | معاينة ورقة العمل قبل الطباعة | Action / Function | Shows the work sheet before printing it. | [VERIFIED] | In scope |
| 2.2.1.5.4 | Patient Sheet Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **2.3** | **Work Sheet by Test Names** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 2.3.1 | Work Sheet by Test Names Screen | ورقة عمل بأسماء التحاليل | Screen / Container | Through this window a work sheet is made for a single test or a group of tests within a chosen period, or a group of tests for a certain number of patients. | [VERIFIED] | In scope |
| 2.3.1.1 | Test Work Sheet Mode Options | — | Field group / Container | The forms the test work sheet may take. | [VERIFIED] | In scope |
| 2.3.1.1.1 | Selected Tests Sheet Option | ورقة عمل بأسماء التحاليل | Option / Subfunction | The work sheet is built from the tests selected in the test list. | [VERIFIED] | In scope |
| 2.3.1.1.2 | Period Test Names Sheet Option | ورقة عمل حسب أسماء تحاليل الفترة الزمنية المحددة | Option / Subfunction | The work sheet is built from the test names of the chosen period. | [VERIFIED] | In scope |
| 2.3.1.1.3 | Specific Test In Period Option | ورقة عمل حسب التحليل المحدد فى الفترة الزمنية المحددة | Option / Subfunction | The work sheet is built from one chosen test within the chosen period. | [VERIFIED] | In scope |
| 2.3.1.1.4 | Group Name Sheet Option | ورقة عمل بإسم مجموعة (Log) | Option / Subfunction | The work sheet is built from a work group, and the group's tests are not shown in the list. | [VERIFIED] | In scope |
| 2.3.1.2 | Test Sheet Filter Block | — | Block / Container | The filters that decide which tests the work sheet covers. | [VERIFIED] | In scope |
| 2.3.1.2.1 | Test Sheet Group Selector | المجموعة | Field / UI element | Selects the work group the work sheet covers. | [VERIFIED] | In scope |
| 2.3.1.2.2 | Test Sheet Referring Entity Selector | جهة الإحالة | Field / UI element | Selects the referring entity whose tests the work sheet covers. | [VERIFIED] | In scope |
| 2.3.1.2.3 | Test Sheet From Code Field | من كود رقم | Field / UI element | The code the patient range starts at. | [VERIFIED] | In scope |
| 2.3.1.2.4 | Test Sheet To Code Field | الى كود رقم | Field / UI element | The code the patient range ends at. | [VERIFIED] | In scope |
| 2.3.1.2.5 | Test Sheet From Date Field | من تاريخ | Field / UI element | The date the period starts at. | [VERIFIED] | In scope |
| 2.3.1.2.6 | Test Sheet To Date Field | الى | Field / UI element | The date the period ends at. | [VERIFIED] | In scope |
| 2.3.1.3 | Test Sheet List Block | — | Block / Container | The list of tests the work sheet will cover. | [VERIFIED] | In scope |
| 2.3.1.3.1 | Test Sheet Test Grid | All \ ·  Test ID \ ·  Test Name | Grid / UI element | The list of tests, with the columns all, the test identifier and the test name, and with the count of the tests it holds. | [VERIFIED] | In scope |
| 2.3.1.3.2 | Test Sheet Test Count | — | Indicator / UI element | The number of tests in the list. | [VERIFIED] | In scope |
| 2.3.1.3.3 | Test Name Double-Click Removal Action | — | Action / Function | A test is removed from the list by pressing its name twice. | [VERIFIED] | In scope |
| 2.3.1.4 | Test Sheet Print Option Block | — | Block / Container | The options that decide what the work sheet contains. | [VERIFIED] | In scope |
| 2.3.1.4.1 | Test Sheet Unfinished Tests Only Option | طباعة التحاليل الغير منتهية فقط (عند عمل لوج او لجهة فقط) | Option / Subfunction | The work sheet carries the unfinished tests alone; the option applies when a work group is made, or for a referring entity alone. | [VERIFIED] | In scope |
| 2.3.1.5 | Test Sheet Action Block | — | Block / Container | The actions of the test work sheet window. | [VERIFIED] | In scope |
| 2.3.1.5.1 | Test Sheet Print Action | Print | Action / Function | Prints the test work sheet. | [VERIFIED] | In scope |
| 2.3.1.5.2 | Test Sheet Refresh Action | Refresh | Action / Function | Reloads the test list. | [VERIFIED] | In scope |
| 2.3.1.5.3 | Test Sheet Preview Action | معاينة ورقة العمل قبل الطباعة | Action / Function | Shows the work sheet before printing it. | [VERIFIED] | In scope |
| 2.3.1.5.4 | Period Test Classification Action | تصنيف تحاليل الفترة المحددة | Action / Function | Opens the classification of the tests of the chosen period. | [VERIFIED] | In scope |
| 2.3.1.5.5 | Test Sheet Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **2.4** | **Period Test Classification** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 2.4.1 | Period Test Classification Screen | تصنيف تحاليل الفترة المحددة | Screen / Container | The classification of the tests of a chosen period, showing how many times each test was performed in that period. | [VERIFIED] | In scope |
| 2.4.1.1 | Classification Grid | Test Name \ ·  No. of Tests | Grid / UI element | The grid of the classification: one row per test, carrying the test name and the number of times the test was performed in the period. | [VERIFIED] | In scope |
| 2.4.1.1.1 | Classification Test Name Column | Test Name | Grid column / UI element | The name of the test. | [VERIFIED] | In scope |
| 2.4.1.1.2 | Classification Test Count Column | No. of Tests | Grid column / UI element | The number of times the test was performed in the period. | [VERIFIED] | In scope |
| 2.4.1.2 | Classification Report Output | statistics | Output / Output | The printed classification report, carrying the laboratory header, the date and time of printing, the user who printed it and the classification grid. | [VERIFIED] | In scope |
| **2.5** | **Work Sheet Documents** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 2.5.1 | Common Document Elements | — | Block / Container | The elements every work sheet document carries. | [VERIFIED] | In scope |
| 2.5.1.1 | Laboratory Header Block | — | Block / Container | The block of the printed document that carries the laboratory's own printed header. | [VERIFIED] | In scope |
| 2.5.1.1.1 | Laboratory Printed Header | — | Output / Output | The header the laboratory prints on its documents. Its content is the laboratory's own identity and is not described here. | [VERIFIED] | In scope |
| 2.5.1.2 | Printed By Entry | Printed By | Field / UI element | The user who printed the document. | [VERIFIED] | In scope |
| 2.5.1.3 | Print Timestamp | — | Field / UI element | The date and time the document was printed. | [VERIFIED] | In scope |
| 2.5.1.4 | Zoom Setting | Zoom 100% | Field / UI element | The magnification the document is shown at in the preview. | [VERIFIED] | In scope |
| 2.5.1.5 | Page Count Entry | Pages: 1 | Field / UI element | The number of pages the document has. | [VERIFIED] | In scope |
| 2.5.2 | Work Paper by Patient ID | Work Paper | Output / Output | The work sheet produced for a group of patients, carrying them in rows. | [VERIFIED] | In scope |
| 2.5.2.1 | Work Paper Patient Title | Work paper by Patient ID | Field / UI element | The title of the work sheet, shown above the patient rows. | [VERIFIED] | In scope |
| 2.5.2.2 | Patient Sheet Count Entry | No. of Patients | Field / UI element | The number of patients the work sheet carries. | [VERIFIED] | In scope |
| 2.5.2.3 | Work Paper Patient ID Column | Patient ID | Grid column / UI element | The patient's identifier. | [VERIFIED] | In scope |
| 2.5.2.4 | Work Paper Patient Name Column | Patient Name | Grid column / UI element | The patient's name. | [VERIFIED] | In scope |
| 2.5.2.5 | Work Paper Gender Column | Gender | Grid column / UI element | The patient's sex. | [VERIFIED] | In scope |
| 2.5.2.6 | Work Paper Age Column | Age | Grid column / UI element | The patient's age. | [VERIFIED] | In scope |
| 2.5.2.7 | Work Paper Date Column | Date | Grid column / UI element | The date of the patient's visit. | [VERIFIED] | In scope |
| 2.5.2.8 | Work Paper Referred By Column | Referred by | Grid column / UI element | The entity that referred the patient. | [VERIFIED] | In scope |
| 2.5.2.9 | Reception Officer Signature Area | موظف الاستقبال | Field / UI element | The area of the document where the reception officer signs. | [VERIFIED] | In scope |
| 2.5.2.10 | Samples Officer Signature Area | مسئول العينات | Field / UI element | The area of the document where the samples officer signs. | [VERIFIED] | In scope |
| 2.5.2.11 | Work Paper Patient Entry | Patient | Field / UI element | An entry on the document header whose meaning is not established. | [UNVERIFIED] | In scope |
| 2.5.3 | Work Paper by Test Name | Work paper by test name | Output / Output | The work sheet produced for a set of tests, carrying, under each test, the patients who had it and their results. | [VERIFIED] | In scope |
| 2.5.3.1 | Work Paper Test Title | Work paper by test name | Field / UI element | The title of the work sheet, shown above the test sections. | [VERIFIED] | In scope |
| 2.5.3.2 | Test Type Count Entry | No. of type of tests | Field / UI element | The number of test types the work sheet carries. | [VERIFIED] | In scope |
| 2.5.3.3 | Work Paper Test Code Column | Test Code | Grid column / UI element | The identifier of the test. | [VERIFIED] | In scope |
| 2.5.3.4 | Work Paper Test Name Column | Test Name | Grid column / UI element | The name of the test. | [VERIFIED] | In scope |
| 2.5.3.5 | Test Work Paper Patient Name Column | Patient Name | Grid column / UI element | The name of a patient who had the test. | [VERIFIED] | In scope |
| 2.5.3.6 | Work Paper Result Column | Result | Grid column / UI element | The result the patient obtained on the test. | [VERIFIED] | In scope |
| 2.5.3.7 | Test Work Paper Patient ID Column | ID | Grid column / UI element | The identifier of a patient who had the test. | [VERIFIED] | In scope |
| 2.5.3.8 | Patient Count For Test Entry | No. of Patient for this test | Field / UI element | The number of patients who had the test. | [VERIFIED] | In scope |
| 2.5.4 | Work Paper for Selected Test | Work paper for selected test | Output / Output | The work sheet produced for one chosen test, carrying the patients who had it. | [VERIFIED] | In scope |
| 2.5.4.1 | Selected Test Work Paper Title | Work paper for selected test | Field / UI element | The title of the work sheet, shown above the patient rows. | [VERIFIED] | In scope |
| 2.5.4.2 | Selected Test Code Entry | Test Code | Field / UI element | The identifier of the chosen test. | [VERIFIED] | In scope |
| 2.5.4.3 | Selected Test Name Entry | Test Name | Field / UI element | The name of the chosen test. | [VERIFIED] | In scope |
| 2.5.4.4 | Selected Test Patient ID Column | ID | Grid column / UI element | The patient's identifier. | [VERIFIED] | In scope |
| 2.5.4.5 | Selected Test Patient Name Column | Patient Name | Grid column / UI element | The patient's name. | [VERIFIED] | In scope |
| 2.5.4.6 | Selected Test Patient Result Column | Patient Result | Grid column / UI element | The result the patient obtained on the chosen test. | [VERIFIED] | In scope |
| 2.5.4.7 | Selected Test Signature Column | Sig. | Grid column / UI element | The area of the row where the result is signed for. | [VERIFIED] | In scope |
| 2.5.4.8 | Selected Test Patient Count Entry | No. of patient for this test | Field / UI element | The number of patients who had the chosen test. | [VERIFIED] | In scope |
| 2.5.4.9 | Selected Test Reception Officer Signature Area | موظف الاستقبال | Field / UI element | The area of the document where the reception officer signs. | [VERIFIED] | In scope |
| 2.5.4.10 | Selected Test Samples Officer Signature Area | مسئول العينات | Field / UI element | The area of the document where the samples officer signs. | [VERIFIED] | In scope |
| **2.6** | **Printed Work Paper Structure** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 2.6.1 | Work Paper Print Header Block | — | Block / Container | The header printed above every work paper. | [VERIFIED] | In scope |
| 2.6.1.1 | Work Paper Patient Count Field | No. of Patients | Field / UI element | The number of the patients the work paper covers. | [VERIFIED] | In scope |
| 2.6.1.2 | Work Paper Printed By Field | Printed By | Field / UI element | The name of the user who printed the work paper. | [VERIFIED] | In scope |
| 2.6.1.3 | Work Paper Printed At Field | — | Field / UI element | The date and the time the work paper was printed. | [VERIFIED] | In scope |
| 2.6.2 | Work Paper Pagination Field | Current Page 1 from 1 | Field / UI element | The number of the page and the number of pages the work paper runs to. | [VERIFIED] | In scope |
| 2.6.3 | Work Paper Patient Block | — | Block / Container | The block printed for each patient of a work paper by patient name. | [VERIFIED] | In scope |
| 2.6.3.1 | Work Paper Patient Name Field | Name | Field / UI element | The patient's name. | [VERIFIED] | In scope |
| 2.6.3.2 | Work Paper Patient Gender Field | Gender | Field / UI element | The patient's gender. | [VERIFIED] | In scope |
| 2.6.3.3 | Work Paper Patient Age Field | Age | Field / UI element | The patient's age. | [VERIFIED] | In scope |
| 2.6.3.4 | Work Paper Patient Age Unit Field | Years | Field / UI element | The unit of the age. | [VERIFIED] | In scope |
| 2.6.3.5 | Work Paper Patient Ref By Field | Ref. by | Field / UI element | The referring doctor. | [VERIFIED] | In scope |
| 2.6.3.6 | Work Paper Patient Total Field | Total | Field / UI element | The total of the patient's request. | [VERIFIED] | In scope |
| 2.6.3.7 | Work Paper Patient Paid Field | Paid | Field / UI element | The amount paid by the patient. | [VERIFIED] | In scope |
| 2.6.3.8 | Work Paper Patient Barcode Field | ID | Field / UI element | The patient's identifier, printed as a barcode. | [VERIFIED] | In scope |
| 2.6.3.9 | Work Paper Patient Ok Column | OK | Grid column / UI element | The column of the abbreviated work paper that carries the empty check boxes used to mark the work that is done. | [VERIFIED] | In scope |
| 2.6.4 | Work Paper Test Block | — | Block / Container | The block printed for each test of a work paper by test name. | [VERIFIED] | In scope |
| 2.6.4.1 | Work Paper Test Code Field | Test Code | Field / UI element | The code of the test. | [VERIFIED] | In scope |
| 2.6.4.2 | Work Paper Test Name Field | Test Name | Field / UI element | The name of the test. | [VERIFIED] | In scope |
| 2.6.4.3 | Work Paper Test Patient Count Field | No. of Patient for this test | Field / UI element | The number of the patients for which the test was requested. | [VERIFIED] | In scope |
| 2.6.4.4 | Work Paper Test Patient Name Column | Patient Name | Grid column / UI element | The name of the patient for whom the test was requested. | [VERIFIED] | In scope |
| 2.6.4.5 | Work Paper Test Result Column | Result | Grid column / UI element | The result entered for the patient. | [VERIFIED] | In scope |
| 2.6.4.6 | Work Paper Test Id Column | ID | Grid column / UI element | The patient's identifier, printed as a barcode. | [VERIFIED] | In scope |
| 2.6.5 | Work Paper First And Last Patient Rule | — | Rule / Rule | A work paper may be printed for one day, for half a day or for a shorter period, by giving the code of the first patient wanted and the code of the last patient. | [VERIFIED] | In scope |
| 2.6.6 | Unfinished Tests Work Paper Option | — | Option / Subfunction | A work paper may be printed with the tests that are not finished alone, whether a log or a single entity has been chosen. | [VERIFIED] | In scope |

### §7.3 Workflows

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

### §7.4 Rules, validations and conditional behaviour

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

### §7.5 Outputs

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

### §7.6 Screens and interface structure

| Screen or output | Element identifiers |
|---|---|
| Work Sheet Module Entry | 2.1, 2.1.1, 2.1.2, 2.1.2.1, 2.1.2.2, 2.1.3, 2.1.3.1, 2.1.3.2, 2.1.3.3, 2.1.3.4, 2.1.3.5 |
| Work Sheet by Patient Names Screen | 2.2.1, 2.2.1.1, 2.2.1.1.1, 2.2.1.1.2, 2.2.1.2, 2.2.1.2.1, 2.2.1.2.2, 2.2.1.2.3, 2.2.1.2.4, 2.2.1.2.5, 2.2.1.3, 2.2.1.3.1, 2.2.1.3.2, 2.2.1.3.3, 2.2.1.4, 2.2.1.4.1, 2.2.1.4.2, 2.2.1.5, 2.2.1.5.1, 2.2.1.5.2, 2.2.1.5.3, 2.2.1.5.4 |
| Work Sheet by Test Names Screen | 2.3.1, 2.3.1.1, 2.3.1.1.1, 2.3.1.1.2, 2.3.1.1.3, 2.3.1.1.4, 2.3.1.2, 2.3.1.2.1, 2.3.1.2.2, 2.3.1.2.3, 2.3.1.2.4, 2.3.1.2.5, 2.3.1.2.6, 2.3.1.3, 2.3.1.3.1, 2.3.1.3.2, 2.3.1.3.3, 2.3.1.4, 2.3.1.4.1, 2.3.1.5, 2.3.1.5.1, 2.3.1.5.2, 2.3.1.5.3, 2.3.1.5.4, 2.3.1.5.5 |
| Period Test Classification Screen | 2.4.1, 2.4.1.1, 2.4.1.1.1, 2.4.1.1.2, 2.4.1.2 |
| Work Sheet Documents | 2.5, 2.5.1, 2.5.1.1, 2.5.1.1.1, 2.5.1.2, 2.5.1.3, 2.5.1.4, 2.5.1.5, 2.5.2, 2.5.2.1, 2.5.2.2, 2.5.2.3, 2.5.2.4, 2.5.2.5, 2.5.2.6, 2.5.2.7, 2.5.2.8, 2.5.2.9, 2.5.2.10, 2.5.2.11, 2.5.3, 2.5.3.1, 2.5.3.2, 2.5.3.3, 2.5.3.4, 2.5.3.5, 2.5.3.6, 2.5.3.7, 2.5.3.8, 2.5.4, 2.5.4.1, 2.5.4.2, 2.5.4.3, 2.5.4.4, 2.5.4.5, 2.5.4.6, 2.5.4.7, 2.5.4.8, 2.5.4.9, 2.5.4.10 |

### §7.7 Uncertainties and unshown elements

| ID | Item | Tag |
|---|---|---|
| 2.2.1.1.2 | What the shortened form of the patient work sheet omits relative to the full one is not established. | [UNVERIFIED] |
| 2.2.1.2.2 | Whether the code range filter addresses the patient code or the laboratory identifier is not established. | [UNVERIFIED] |
| 2.3.1.2.2 | Whether the code range filter addresses the patient code or the laboratory identifier is not established. | [UNVERIFIED] |
| 2.3.1.1.4 | The source of the work group a group work sheet is built from is not established. | [UNVERIFIED] |
| 2.5.2.10 | An entry on the work sheet header whose meaning is not established. | [UNVERIFIED] |
| 2.1.3.2 | Whether a work sheet for a single test is produced from the test list or from the specific-test option is not established. | [UNVERIFIED] |
| — | No validation, rejection message or recovery path is established for the work sheet windows. | [NOT-SHOWN] |

## §8 Unit 3 — Test Data and External Dealings

### §8.1 Functional hierarchy

```
3  Test Data and External Dealings
├── 3.1  System Module Entry
│   ├── 3.1.1  System Ribbon Item
│   ├── 3.1.2  System Menu
│   │   ├── 3.1.2.1  Test Data Item
│   │   ├── 3.1.2.2  Test Units Item
│   │   ├── 3.1.2.3  Test Comments Item
│   │   ├── 3.1.2.4  Test Groups Item
│   │   ├── 3.1.2.5  Custom Groups Item
│   │   ├── 3.1.2.6  Culture Antibiotics Item
│   │   ├── 3.1.2.7  Barcode Types Item
│   │   ├── 3.1.2.8  External Entities And Labs Item
│   │   ├── 3.1.2.9  Contract Price Lists Item
│   │   ├── 3.1.2.10  Lab Branches Item
│   │   ├── 3.1.2.11  Patient Titles And Definitions Item
│   │   ├── 3.1.2.12  Patient Names And Phones Item
│   │   ├── 3.1.2.13  Lab Accounts Item
│   │   ├── 3.1.2.14  Work Groups Log Item
│   │   ├── 3.1.2.15  Result Screen Account Item
│   │   ├── 3.1.2.16  Test Price List Print Item
│   │   └── 3.1.2.17  Lab Devices And Equipment Item
│   └── 3.1.3  System Menu Capabilities
│       ├── 3.1.3.1  Test Data Capability
│       ├── 3.1.3.2  Test Annotations Capability
│       ├── 3.1.3.3  Counterparty Capability
│       └── 3.1.3.4  Equipment Capability
├── 3.2  Test Data
│   └── 3.2.1  Test Data Screen
│       ├── 3.2.1.1  Test Search Block
│       │   ├── 3.2.1.1.1  Search By Test Name Field
│       │   ├── 3.2.1.1.2  Search By Group Name Field
│       │   ├── 3.2.1.1.3  Search By Test ID Field
│       │   └── 3.2.1.1.4  Test Count Indicator
│       ├── 3.2.1.2  Test Grid Block
│       │   └── 3.2.1.2.1  Test Grid
│       │       ├── 3.2.1.2.1.1  Test Grid ID Column
│       │       ├── 3.2.1.2.1.2  Test Grid Arrange Column
│       │       ├── 3.2.1.2.1.3  Test Grid Group Name Column
│       │       ├── 3.2.1.2.1.4  Test Grid Test Name Column
│       │       ├── 3.2.1.2.1.5  Test Grid Patient Price Column
│       │       ├── 3.2.1.2.1.6  Test Grid Lab Price Column
│       │       ├── 3.2.1.2.1.7  Test Grid Out Lab Name Column
│       │       ├── 3.2.1.2.1.8  Test Grid Out Price Column
│       │       ├── 3.2.1.2.1.9  Test Grid Barcode Column
│       │       └── 3.2.1.2.1.10  Test Grid Routine Column
│       ├── 3.2.1.3  Test Information Block
│       │   ├── 3.2.1.3.1  Test Name Field
│       │   ├── 3.2.1.3.2  Test Code Field
│       │   ├── 3.2.1.3.3  Report Name Field
│       │   ├── 3.2.1.3.4  Bill Name Field
│       │   ├── 3.2.1.3.5  History Name Field
│       │   ├── 3.2.1.3.6  Arabic Name Field
│       │   ├── 3.2.1.3.7  Test Group Name Field
│       │   ├── 3.2.1.3.8  Test Branch Field
│       │   ├── 3.2.1.3.9  Test Log Group Field
│       │   ├── 3.2.1.3.10  Test Collection Field
│       │   ├── 3.2.1.3.11  Test Time Field
│       │   ├── 3.2.1.3.12  Test Arrange No Field
│       │   ├── 3.2.1.3.13  Reference Type Field
│       │   ├── 3.2.1.3.14  Patient Price Field
│       │   ├── 3.2.1.3.15  Lab To Lab Price Field
│       │   ├── 3.2.1.3.16  Test Barcode Field
│       │   ├── 3.2.1.3.17  Test Barcode Name Field
│       │   ├── 3.2.1.3.18  Tube 1 Field
│       │   ├── 3.2.1.3.19  Tube 2 Field
│       │   ├── 3.2.1.3.20  Tube 3 Field
│       │   ├── 3.2.1.3.21  Sent Outside Lab Field
│       │   ├── 3.2.1.3.22  Out Lab Name Field
│       │   ├── 3.2.1.3.23  Cost Price Field
│       │   ├── 3.2.1.3.24  Patient Question Field
│       │   ├── 3.2.1.3.25  Routine Test Check Box
│       │   ├── 3.2.1.3.26  See Report Check Box
│       │   ├── 3.2.1.3.27  Print With Other Check Box
│       │   ├── 3.2.1.3.28  Add With Group Check Box
│       │   └── 3.2.1.3.29  Main Test Check Box
│       └── 3.2.1.4  Test Data Action Block
│           ├── 3.2.1.4.1  Add Test Action
│           ├── 3.2.1.4.2  Edit Test Action
│           ├── 3.2.1.4.3  Save Test Action
│           ├── 3.2.1.4.4  Undo Test Action
│           ├── 3.2.1.4.5  Test Data Delete Action
│           ├── 3.2.1.4.6  Reference Values Action
│           └── 3.2.1.4.7  Test Data Main List Action
├── 3.3  Test Reference Values
│   └── 3.3.1  Reference Values Screen
│       ├── 3.3.1.1  Reference Range Block
│       │   ├── 3.3.1.1.1  Reference Gender Selector
│       │   ├── 3.3.1.1.2  Reference Age Range Field
│       │   ├── 3.3.1.1.3  Reference Minimum Field
│       │   ├── 3.3.1.1.4  Reference Maximum Field
│       │   ├── 3.3.1.1.5  Low Comment Field
│       │   └── 3.3.1.1.6  High Comment Field
│       ├── 3.3.1.2  Reference Range Action Block
│       │   ├── 3.3.1.2.1  Add Reference Range Action
│       │   ├── 3.3.1.2.2  Edit Reference Range Action
│       │   ├── 3.3.1.2.3  Save Reference Range Action
│       │   └── 3.3.1.2.4  Delete Reference Range Action
│       ├── 3.3.1.3  Reference Panel Block
│       │   ├── 3.3.1.3.1  Reference Panel Test Name Field
│       │   ├── 3.3.1.3.2  Reference Panel Report Name Field
│       │   ├── 3.3.1.3.3  Reference Panel History Name Field
│       │   ├── 3.3.1.3.4  Normal Range Field
│       │   ├── 3.3.1.3.5  Test Unit Field
│       │   ├── 3.3.1.3.6  Low Limit Field
│       │   ├── 3.3.1.3.7  Low Flag Field
│       │   ├── 3.3.1.3.8  High Limit Field
│       │   ├── 3.3.1.3.9  High Flag Field
│       │   ├── 3.3.1.3.10  Update Reference Action
│       │   ├── 3.3.1.3.11  Reference Confirm Action
│       │   └── 3.3.1.3.12  Reference Undo Action
│       └── 3.3.1.4  Reference Matching Rule
├── 3.4  Contract Price Lists
│   └── 3.4.1  Contract Price List Screen
│       ├── 3.4.1.1  Price List Block
│       │   ├── 3.4.1.1.1  Price List Name Field
│       │   ├── 3.4.1.1.2  Price List Add Action
│       │   ├── 3.4.1.1.3  Price List Add Confirm Action
│       │   ├── 3.4.1.1.4  Price List Edit Action
│       │   ├── 3.4.1.1.5  Price List Save Action
│       │   ├── 3.4.1.1.6  Price List Delete Action
│       │   ├── 3.4.1.1.7  Price List Delete Confirm Action
│       │   └── 3.4.1.1.8  Price List Print Action
│       └── 3.4.1.2  Price List Test Block
│           ├── 3.4.1.2.1  Price List Test Name Field
│           ├── 3.4.1.2.2  Price List Test Price Field
│           ├── 3.4.1.2.3  Price List Test Add Action
│           ├── 3.4.1.2.4  Price List Test Add Confirm Action
│           ├── 3.4.1.2.5  Price List Test Edit Action
│           ├── 3.4.1.2.6  Price List Test Save Action
│           └── 3.4.1.2.7  Price List Test Delete Action
├── 3.5  Test Comments
│   └── 3.5.1  Test Comments Screen
│       ├── 3.5.1.1  Test Comments Search Field
│       ├── 3.5.1.2  Test Comments Grid
│       │   ├── 3.5.1.2.1  Test Comments ID Column
│       │   ├── 3.5.1.2.2  Test Comments Test Name Column
│       │   └── 3.5.1.2.3  Test Comments Comment Column
│       ├── 3.5.1.3  Test Comment Entry Block
│       │   ├── 3.5.1.3.1  Test Comment Test Name Field
│       │   └── 3.5.1.3.2  Test Comment Text Field
│       └── 3.5.1.4  Test Comments Action Block
│           ├── 3.5.1.4.1  Add Test Comment Action
│           ├── 3.5.1.4.2  Save Test Comment Action
│           ├── 3.5.1.4.3  Edit Test Comment Action
│           ├── 3.5.1.4.4  Delete Test Comment Action
│           └── 3.5.1.4.5  Test Comments Main List Action
├── 3.6  Test Groups
│   └── 3.6.1  Test Groups Screen
│       ├── 3.6.1.1  Test Groups Count Indicator
│       ├── 3.6.1.2  Test Groups Grid
│       │   ├── 3.6.1.2.1  Test Groups Test ID Column
│       │   ├── 3.6.1.2.2  Test Groups Test Name Column
│       │   └── 3.6.1.2.3  Test Groups Price Column
│       ├── 3.6.1.3  Test Group Entry Block
│       │   ├── 3.6.1.3.1  Group Name Field
│       │   ├── 3.6.1.3.2  Group Test Name Field
│       │   ├── 3.6.1.3.3  Group Test Price Field
│       │   ├── 3.6.1.3.4  Tests In Group Count
│       │   └── 3.6.1.3.5  Total Group Price Indicator
│       └── 3.6.1.4  Test Groups Action Block
│           ├── 3.6.1.4.1  Add Test Group Action
│           ├── 3.6.1.4.2  Save Test Group Action
│           ├── 3.6.1.4.3  Edit Test Group Action
│           ├── 3.6.1.4.4  Delete Test Group Action
│           ├── 3.6.1.4.5  Add Group Test Action
│           ├── 3.6.1.4.6  Delete Group Test Action
│           ├── 3.6.1.4.7  Print Test Group List Action
│           ├── 3.6.1.4.8  Test Groups Undo Action
│           └── 3.6.1.4.9  Test Groups Main List Action
├── 3.7  External Entities and Laboratories
│   └── 3.7.1  External Entities Screen
│       ├── 3.7.1.1  Entity Count Indicator
│       ├── 3.7.1.2  Entity Search Block
│       │   ├── 3.7.1.2.1  Entity Search Field
│       │   └── 3.7.1.2.2  Entity Search All Option
│       ├── 3.7.1.3  Entity Grid
│       │   ├── 3.7.1.3.1  Entity Grid Name Column
│       │   ├── 3.7.1.3.2  Entity Grid Responsible Column
│       │   └── 3.7.1.3.3  Entity Grid Price List Column
│       ├── 3.7.1.4  Entity Type Options
│       │   ├── 3.7.1.4.1  Treating Doctor Entity Type
│       │   ├── 3.7.1.4.2  Sent Samples Entity Type
│       │   └── 3.7.1.4.3  Referring Or Contract Entity Type
│       ├── 3.7.1.5  Entity Data Block
│       │   ├── 3.7.1.5.1  Entity Capacity Field
│       │   ├── 3.7.1.5.2  Entity Name Field
│       │   ├── 3.7.1.5.3  Entity City Field
│       │   ├── 3.7.1.5.4  Entity Address Field
│       │   ├── 3.7.1.5.5  Entity Phone Field
│       │   ├── 3.7.1.5.6  Entity Mobile Field
│       │   ├── 3.7.1.5.7  Entity Fax Field
│       │   ├── 3.7.1.5.8  Entity Website Field
│       │   ├── 3.7.1.5.9  Entity Email Field
│       │   ├── 3.7.1.5.10  Entity Responsible Name Field
│       │   └── 3.7.1.5.11  Entity Responsible Phone Field
│       ├── 3.7.1.6  Entity Pricing Block
│       │   ├── 3.7.1.6.1  Entity Price List Selector
│       │   ├── 3.7.1.6.2  Entity Price List Options
│       │   │   ├── 3.7.1.6.2.1  Individual Price List Option
│       │   │   ├── 3.7.1.6.2.2  Lab To Lab Price List Option
│       │   │   ├── 3.7.1.6.2.3  Free Price List Option
│       │   │   ├── 3.7.1.6.2.4  Low Price List Option
│       │   │   ├── 3.7.1.6.2.5  Price Menu One Option
│       │   │   └── 3.7.1.6.2.6  Price Menu Two Option
│       │   └── 3.7.1.6.3  Discount And Commission Block
│       │       ├── 3.7.1.6.3.1  Entity Discount Percent Field
│       │       └── 3.7.1.6.3.2  Entity Commission Percent Field
│       ├── 3.7.1.7  Entity Site Credentials Block
│       │   ├── 3.7.1.7.1  Entity Site Address Field
│       │   ├── 3.7.1.7.2  Entity Site Password Field
│       │   ├── 3.7.1.7.3  Entity Site Identifier Field
│       │   └── 3.7.1.7.4  Generate Entity Identifier Action
│       └── 3.7.1.8  Entity Action Block
│           ├── 3.7.1.8.1  Save Entity Action
│           ├── 3.7.1.8.2  Edit Entity Action
│           ├── 3.7.1.8.3  Delete Entity Action
│           ├── 3.7.1.8.4  Entity Undo Action
│           └── 3.7.1.8.5  Entity Main List Action
├── 3.8  Culture Registration
│   └── 3.8.1  Culture Entry Slots
│       ├── 3.8.1.1  Culture Slot Identifier Range
│       ├── 3.8.1.2  Culture Group Name Selector
│       ├── 3.8.1.3  Culture Edit Action
│       ├── 3.8.1.4  Culture Save Action
│       └── 3.8.1.5  Culture Configuration File
├── 3.9  Culture Antibiotics
│   └── 3.9.1  Antibiotics Screen
│       ├── 3.9.1.1  Antibiotic Culture Selector
│       ├── 3.9.1.2  International Antibiotic Option
│       ├── 3.9.1.3  Antibiotic By Abbreviation Option
│       ├── 3.9.1.4  Antibiotics Count In Culture
│       ├── 3.9.1.5  Antibiotic Entry Block
│       │   ├── 3.9.1.5.1  Antibiotic Symbol Field
│       │   ├── 3.9.1.5.2  Antibiotic Scientific Name Field
│       │   ├── 3.9.1.5.3  Antibiotic Sensitivity Field
│       │   ├── 3.9.1.5.4  Antibiotic Pregnant Field
│       │   ├── 3.9.1.5.5  Antibiotic Children Field
│       │   └── 3.9.1.5.6  Antibiotic Commercial Names Field
│       └── 3.9.1.6  Antibiotic Action Block
│           ├── 3.9.1.6.1  Add Antibiotic Action
│           ├── 3.9.1.6.2  Save Antibiotic Action
│           ├── 3.9.1.6.3  Edit Antibiotic Action
│           ├── 3.9.1.6.4  Delete Antibiotic Action
│           ├── 3.9.1.6.5  Antibiotic Print Action
│           ├── 3.9.1.6.6  Antibiotics Undo Action
│           └── 3.9.1.6.7  Antibiotics Main List Action
├── 3.10  Samples Sent Outside
│   └── 3.10.1  Samples Sent Outside Procedure
│       ├── 3.10.1.1  Sent Outside Test Selection Action
│       ├── 3.10.1.2  Sent Outside Lab Selection
│       ├── 3.10.1.3  Cost Price Recording
│       └── 3.10.1.4  Patient Price Recording
├── 3.11  Result Service Accounts
│   ├── 3.11.1  Doctor Account Creation
│   │   ├── 3.11.1.1  Doctor Account Identifier Generation
│   │   ├── 3.11.1.2  Doctor Account Password Setting
│   │   └── 3.11.1.3  Doctor Result Access
│   └── 3.11.2  Laboratory Account Creation
│       ├── 3.11.2.1  Laboratory Contract Registration
│       └── 3.11.2.2  Laboratory Result Access
├── 3.12  Reference Range List and Form Details
│   ├── 3.12.1  Reference Range List Grid
│   │   ├── 3.12.1.1  Reference List Gender Column
│   │   ├── 3.12.1.2  Reference List From Column
│   │   ├── 3.12.1.3  Reference List To Column
│   │   ├── 3.12.1.4  Reference List Age Unit Column
│   │   ├── 3.12.1.5  Reference List Range Column
│   │   ├── 3.12.1.6  Reference List Low Limit Column
│   │   ├── 3.12.1.7  Reference List High Limit Column
│   │   ├── 3.12.1.8  Reference List Test Unit Column
│   │   ├── 3.12.1.9  Reference List Low Flag Column
│   │   └── 3.12.1.10  Reference List High Flag Column
│   └── 3.12.2  Reference Range Form Block
│       ├── 3.12.2.1  Range For Selector
│       │   ├── 3.12.2.1.1  Range For All Option
│       │   ├── 3.12.2.1.2  Range For By Sex Only Option
│       │   ├── 3.12.2.1.3  Range For By Sex And Age Option
│       │   └── 3.12.2.1.4  Range For By Age Only Option
│       ├── 3.12.2.2  Range For Male Option
│       ├── 3.12.2.3  Range For Female Option
│       ├── 3.12.2.4  Range Age From Field
│       ├── 3.12.2.5  Range Age To Field
│       ├── 3.12.2.6  Range Age Unit Selector
│       ├── 3.12.2.7  Reference Range Text Area
│       ├── 3.12.2.8  Pregnant Only Flag
│       ├── 3.12.2.9  Reference Count Indicator
│       └── 3.12.2.10  Feed The Analysis Action
├── 3.13  Antibiotic List and Commercial Name Marks
│   ├── 3.13.1  Culture Antibiotic List Grid
│   │   ├── 3.13.1.1  Culture Antibiotic Symbol Column
│   │   ├── 3.13.1.2  Culture Antibiotic Scientific Name Column
│   │   ├── 3.13.1.3  Culture Antibiotic Sensitivity Column
│   │   ├── 3.13.1.4  Culture Antibiotic Pregnant Column
│   │   └── 3.13.1.5  Culture Antibiotic Children Column
│   └── 3.13.2  Commercial Name Print Flag
├── 3.14  Test Group Screen Details
│   ├── 3.14.1  Laboratory Test Group List
│   ├── 3.14.2  Group Name List Field
│   ├── 3.14.3  Add Sub Group Action
│   └── 3.14.4  Group Fixed Price Rule
└── 3.15  System Window Further Entries
    ├── 3.15.1  Purchase Lab Devices Item
    ├── 3.15.2  Read Analysis Report Item
    ├── 3.15.3  Card And Identity Data Item
    ├── 3.15.4  System Menu Rendering Difference Rule
    └── 3.15.5  System Window Capability Block
        ├── 3.15.5.1  System Tests And Ranges Capability
        ├── 3.15.5.2  System Antibiotics Capability
        ├── 3.15.5.3  System Lists Capability
        └── 3.15.5.4  System Devices Capability
```

### §8.2 Function inventory

The elements of the unit, each defined once, at the place that carries it. The *Type / Level* column gives the kind of the element and the level at which it is recorded under the policy of §3. The *Scope* column marks the elements that belong to one of the five excluded categories of §2; they are documented, and they are not parity requirements.

| ID | Canonical name | Source label | Type / Level | Definition | Evidence | Scope |
|---|---|---|---|---|---|---|
| **3.1** | **System Module Entry** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.1.1 | System Ribbon Item | System | Navigation / Subfunction | The ribbon item that opens the system module, from which every data screen of the system is reached. | [VERIFIED] | In scope |
| 3.1.2 | System Menu | — | Menu / UI element | The list of the data screens the system module offers. | [VERIFIED] | In scope |
| 3.1.2.1 | Test Data Item | بيانات التحاليل | Menu item / Subfunction | Opens the screen that holds the tests registered in the program's database. | [VERIFIED] | In scope |
| 3.1.2.2 | Test Units Item | Test Units | Menu item / Subfunction | Opens the screen of the units used by the tests. | [VERIFIED] | In scope |
| 3.1.2.3 | Test Comments Item | Test Comments | Menu item / Subfunction | Opens the screen of the fixed comments attached to the results. | [VERIFIED] | In scope |
| 3.1.2.4 | Test Groups Item | مجموعات التحاليل | Menu item / Subfunction | Opens the screen of the laboratory's own test groups. | [VERIFIED] | In scope |
| 3.1.2.5 | Custom Groups Item | Custom Groups | Menu item / Subfunction | Opens the screen of the custom test groups. | [VERIFIED] | In scope |
| 3.1.2.6 | Culture Antibiotics Item | Culture Antibiotics | Menu item / Subfunction | Opens the screen of the antibiotics of the cultures. | [VERIFIED] | In scope |
| 3.1.2.7 | Barcode Types Item | Barcode Types | Menu item / Subfunction | Opens the screen of the barcode types. | [VERIFIED] | In scope |
| 3.1.2.8 | External Entities And Labs Item | الجهات الخارجية والمعامل | Menu item / Subfunction | Opens the screen of the external entities and laboratories, where a treating doctor and a referring entity are registered. | [VERIFIED] | In scope |
| 3.1.2.9 | Contract Price Lists Item | قوائم أسعار التعاقدات | Menu item / Subfunction | Opens the screen of the contract price lists. | [VERIFIED] | In scope |
| 3.1.2.10 | Lab Branches Item | Lab. branches | Menu item / Subfunction | Opens the screen of the laboratory branches. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 3.1.2.11 | Patient Titles And Definitions Item | القاب وتعريفات المرضي | Menu item / Subfunction | Opens the screen of the patient titles and definitions. | [VERIFIED] | In scope |
| 3.1.2.12 | Patient Names And Phones Item | اسماء وتليفونات المرضي | Menu item / Subfunction | Opens the screen of the patient names and telephone numbers. | [VERIFIED] | In scope |
| 3.1.2.13 | Lab Accounts Item | Lab. accounts | Menu item / Subfunction | Opens the screen of the laboratory accounts. | [VERIFIED] | In scope |
| 3.1.2.14 | Work Groups Log Item | (Log) مجموعات العمل | Menu item / Subfunction | Opens the screen of the work groups. | [VERIFIED] | In scope |
| 3.1.2.15 | Result Screen Account Item | Result screen account | Menu item / Subfunction | Opens the screen of the accounts of the result service. | [VERIFIED] | In scope |
| 3.1.2.16 | Test Price List Print Item | طباعة قائمة اسعار التحاليل | Menu item / Subfunction | Opens the printing of the test price list. | [VERIFIED] | In scope |
| 3.1.2.17 | Lab Devices And Equipment Item | أجهزة ومعدات المختبر | Menu item / Subfunction | Opens the screen of the laboratory devices and equipment. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — equipment management |
| 3.1.3 | System Menu Capabilities | — | Block / Container | What the system module allows to be entered and maintained. | [VERIFIED] | In scope |
| 3.1.3.1 | Test Data Capability | — | Function / Function | The tests, their normal values, their units, their groups and their barcodes are entered and edited. | [VERIFIED] | In scope |
| 3.1.3.2 | Test Annotations Capability | — | Function / Function | The comments on the test results, the antibiotics, the laboratory's own groups and their prices are entered and edited. | [VERIFIED] | In scope |
| 3.1.3.3 | Counterparty Capability | — | Function / Function | The counterparties — a referring entity, a receiving entity, a contract — are entered, with the prices that apply to them. | [VERIFIED] | In scope |
| 3.1.3.4 | Equipment Capability | — | Function / Function | The laboratory's devices and equipment are entered, with their condition, their periodic maintenance and their calibration history. | [VERIFIED] | In scope |
| **3.2** | **Test Data** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.2.1 | Test Data Screen | بيانات التحاليل | Screen / Container | The screen that holds every test registered in the program's database, and in which a test is added, edited or deleted. | [VERIFIED] | In scope |
| 3.2.1.1 | Test Search Block | — | Block / Container | The controls that find a test in the list. | [VERIFIED] | In scope |
| 3.2.1.1.1 | Search By Test Name Field | Search by test name | Field / UI element | Finds a test by its name. | [VERIFIED] | In scope |
| 3.2.1.1.2 | Search By Group Name Field | By group name | Field / UI element | Finds a test by the group that contains it. | [VERIFIED] | In scope |
| 3.2.1.1.3 | Search By Test ID Field | By test ID | Field / UI element | Finds a test by its identifier. | [VERIFIED] | In scope |
| 3.2.1.1.4 | Test Count Indicator | Number Of Tests | Indicator / UI element | The number of tests the list currently holds. | [VERIFIED] | In scope |
| 3.2.1.2 | Test Grid Block | — | Block / Container | The list of the registered tests. | [VERIFIED] | In scope |
| 3.2.1.2.1 | Test Grid | ID \ ·  Arrang \ ·  Group Name \ ·  Test Name \ ·  Pat. P \ ·  Lab P \ ·  Out Lab Name \ ·  Out P \ ·  Barcode \ ·  Routine | Grid / UI element | The list of the registered tests, with the columns: the identifier, the arrangement number, the group name, the test name, the patient's price, the laboratory-to-laboratory price, the name of the external laboratory, the external price, the barcode, and whether the test is routine. | [VERIFIED] | In scope |
| 3.2.1.2.1.1 | Test Grid ID Column | ID | Grid column / UI element | The identifier of the test. | [VERIFIED] | In scope |
| 3.2.1.2.1.2 | Test Grid Arrange Column | Arrang | Grid column / UI element | The arrangement number of the test, which fixes the order the tests appear in. | [VERIFIED] | In scope |
| 3.2.1.2.1.3 | Test Grid Group Name Column | Group Name | Grid column / UI element | The group the test belongs to. | [VERIFIED] | In scope |
| 3.2.1.2.1.4 | Test Grid Test Name Column | Test Name | Grid column / UI element | The name of the test. | [VERIFIED] | In scope |
| 3.2.1.2.1.5 | Test Grid Patient Price Column | Pat. P | Grid column / UI element | The patient's price of the test. | [VERIFIED] | In scope |
| 3.2.1.2.1.6 | Test Grid Lab Price Column | Lab P | Grid column / UI element | The laboratory-to-laboratory price of the test. | [VERIFIED] | In scope |
| 3.2.1.2.1.7 | Test Grid Out Lab Name Column | Out Lab Name | Grid column / UI element | The name of the external laboratory the test is sent to. | [VERIFIED] | In scope |
| 3.2.1.2.1.8 | Test Grid Out Price Column | Out P | Grid column / UI element | The price of the test at the external laboratory. | [VERIFIED] | In scope |
| 3.2.1.2.1.9 | Test Grid Barcode Column | Barcode | Grid column / UI element | The barcode assigned to the test. | [VERIFIED] | In scope |
| 3.2.1.2.1.10 | Test Grid Routine Column | Routine | Grid column / UI element | Whether the test is a routine test. | [VERIFIED] | In scope |
| 3.2.1.3 | Test Information Block | Test Information | Block / Container | The form in which the data of a test is entered and edited. | [VERIFIED] | In scope |
| 3.2.1.3.1 | Test Name Field | Test Name | Field / UI element | The name of the test. | [VERIFIED] | In scope |
| 3.2.1.3.2 | Test Code Field | Test Code | Field / UI element | The code of the test. | [VERIFIED] | In scope |
| 3.2.1.3.3 | Report Name Field | Report Name | Field / UI element | The name the test carries on the report. | [VERIFIED] | In scope |
| 3.2.1.3.4 | Bill Name Field | Bill Name | Field / UI element | The name the test carries on the receipt. | [VERIFIED] | In scope |
| 3.2.1.3.5 | History Name Field | History Name | Field / UI element | The name the test carries in the patient's history. | [VERIFIED] | In scope |
| 3.2.1.3.6 | Arabic Name Field | Arabic Name | Field / UI element | The name of the test in the other script. | [VERIFIED] | In scope |
| 3.2.1.3.7 | Test Group Name Field | Group Name | Field / UI element | The group the test belongs to. | [VERIFIED] | In scope |
| 3.2.1.3.8 | Test Branch Field | Branch | Field / UI element | The branch the test belongs to. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 3.2.1.3.9 | Test Log Group Field | Log Group | Field / UI element | The work group the test belongs to. | [VERIFIED] | In scope |
| 3.2.1.3.10 | Test Collection Field | Collection | Field / UI element | The collection the test belongs to. | [VERIFIED] | In scope |
| 3.2.1.3.11 | Test Time Field | Test time (Day) | Field / UI element | The number of days the test needs to be completed; the program computes the patient's receipt time from it. | [VERIFIED] | In scope |
| 3.2.1.3.12 | Test Arrange No Field | Arrange No. | Field / UI element | The number that fixes the order the test appears in. | [VERIFIED] | In scope |
| 3.2.1.3.13 | Reference Type Field | Reference type | Field / UI element | The way the reference values of the test are set: by sex and age, or by age alone. | [VERIFIED] | In scope |
| 3.2.1.3.14 | Patient Price Field | Patient price | Field / UI element | The price of the test taken from the patient. | [VERIFIED] | In scope |
| 3.2.1.3.15 | Lab To Lab Price Field | Lab to lab price | Field / UI element | The price of the test when it is performed for another laboratory. | [VERIFIED] | In scope |
| 3.2.1.3.16 | Test Barcode Field | Barcode | Field / UI element | The barcode assigned to the test. | [VERIFIED] | In scope |
| 3.2.1.3.17 | Test Barcode Name Field | Name | Field / UI element | The name printed with the barcode of the test. | [VERIFIED] | In scope |
| 3.2.1.3.18 | Tube 1 Field | Tube 1 | Field / UI element | The first tube the test is performed on. | [VERIFIED] | In scope |
| 3.2.1.3.19 | Tube 2 Field | Tube 2 | Field / UI element | The second tube the test is performed on. | [VERIFIED] | In scope |
| 3.2.1.3.20 | Tube 3 Field | Tube 3 | Field / UI element | The third tube the test is performed on. | [VERIFIED] | In scope |
| 3.2.1.3.21 | Sent Outside Lab Field | Sent outside Lab. | Field / UI element | Marks the test as one whose sample is sent to another laboratory. | [VERIFIED] | In scope |
| 3.2.1.3.22 | Out Lab Name Field | Lab. Name | Field / UI element | The external laboratory the sample is sent to. | [VERIFIED] | In scope |
| 3.2.1.3.23 | Cost Price Field | Cost price | Field / UI element | The price of the test paid to the external laboratory. | [VERIFIED] | In scope |
| 3.2.1.3.24 | Patient Question Field | Patient Question | Field / UI element | The question put to the patient about the test. | [VERIFIED] | In scope |
| 3.2.1.3.25 | Routine Test Check Box | Routine test | Field / UI element | Marks the test as a routine test. | [VERIFIED] | In scope |
| 3.2.1.3.26 | See Report Check Box | See Report | Field / UI element | Marks the test as one whose report is opened from the result grid. | [VERIFIED] | In scope |
| 3.2.1.3.27 | Print With Other Check Box | Print with other | Field / UI element | Marks the test as one printed together with other tests. | [VERIFIED] | In scope |
| 3.2.1.3.28 | Add With Group Check Box | Add with group | Field / UI element | Marks the test as one added together with a group. | [VERIFIED] | In scope |
| 3.2.1.3.29 | Main Test Check Box | Main test | Field / UI element | Marks the test as the main test of its group. | [VERIFIED] | In scope |
| 3.2.1.4 | Test Data Action Block | — | Block / Container | The actions of the test data screen. | [VERIFIED] | In scope |
| 3.2.1.4.1 | Add Test Action | اضافة تحليل | Action / Function | Opens the form in which a new test is entered. | [VERIFIED] | In scope |
| 3.2.1.4.2 | Edit Test Action | تعديل | Action / Function | Opens the data of the selected test for editing. | [VERIFIED] | In scope |
| 3.2.1.4.3 | Save Test Action | حفظ | Action / Function | Persists the test data. | [VERIFIED] | In scope |
| 3.2.1.4.4 | Undo Test Action | تراجع | Action / Function | Steps back out of the current test operation. | [VERIFIED] | In scope |
| 3.2.1.4.5 | Test Data Delete Action | حذف | Action / Function | Deletes the selected test. | [VERIFIED] | In scope |
| 3.2.1.4.6 | Reference Values Action | القيم المرجعية | Action / Function | Opens the reference values of the selected test. | [VERIFIED] | In scope |
| 3.2.1.4.7 | Test Data Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **3.3** | **Test Reference Values** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.3.1 | Reference Values Screen | القيم المرجعية | Screen / Container | The screen in which the normal ranges of a test are entered and edited. | [VERIFIED] | In scope |
| 3.3.1.1 | Reference Range Block | — | Block / Container | The form in which one normal range is entered. | [VERIFIED] | In scope |
| 3.3.1.1.1 | Reference Gender Selector | — | Field / UI element | The sex the normal range applies to. | [VERIFIED] | In scope |
| 3.3.1.1.2 | Reference Age Range Field | — | Field / UI element | The age range the normal range applies to. | [VERIFIED] | In scope |
| 3.3.1.1.3 | Reference Minimum Field | — | Field / UI element | The lowest value of the normal range. | [VERIFIED] | In scope |
| 3.3.1.1.4 | Reference Maximum Field | — | Field / UI element | The highest value of the normal range. | [VERIFIED] | In scope |
| 3.3.1.1.5 | Low Comment Field | low comment | Field / UI element | A comment shown automatically on the patient's report when the result falls below the lowest value. | [VERIFIED] | In scope |
| 3.3.1.1.6 | High Comment Field | High comment | Field / UI element | A comment shown automatically on the patient's report when the result rises above the highest value. | [VERIFIED] | In scope |
| 3.3.1.2 | Reference Range Action Block | — | Block / Container | The actions of the reference range screen. | [VERIFIED] | In scope |
| 3.3.1.2.1 | Add Reference Range Action | إضافة مدى | Action / Function | Opens the form in which a new normal range is entered. | [VERIFIED] | In scope |
| 3.3.1.2.2 | Edit Reference Range Action | تعديل | Action / Function | Opens the selected normal range for editing. | [VERIFIED] | In scope |
| 3.3.1.2.3 | Save Reference Range Action | حفظ | Action / Function | Persists the normal range. | [VERIFIED] | In scope |
| 3.3.1.2.4 | Delete Reference Range Action | حذف | Action / Function | Deletes the selected normal range. | [VERIFIED] | In scope |
| 3.3.1.3 | Reference Panel Block | — | Block / Container | The panel in which the reference limits of one test are entered. | [VERIFIED] | In scope |
| 3.3.1.3.1 | Reference Panel Test Name Field | Test Name | Field / UI element | The name of the test the limits belong to. | [VERIFIED] | In scope |
| 3.3.1.3.2 | Reference Panel Report Name Field | Report Name | Field / UI element | The name the test carries on the report. | [VERIFIED] | In scope |
| 3.3.1.3.3 | Reference Panel History Name Field | History Name | Field / UI element | The name the test carries in the patient's history. | [VERIFIED] | In scope |
| 3.3.1.3.4 | Normal Range Field | Normal range | Field / UI element | The normal range as it is shown on the report. | [VERIFIED] | In scope |
| 3.3.1.3.5 | Test Unit Field | Test unit | Field / UI element | The unit of the test's result. | [VERIFIED] | In scope |
| 3.3.1.3.6 | Low Limit Field | Low limit | Field / UI element | The lowest value of the normal range. | [VERIFIED] | In scope |
| 3.3.1.3.7 | Low Flag Field | Low flag | Field / UI element | The flag shown when the result is below the lowest value. | [VERIFIED] | In scope |
| 3.3.1.3.8 | High Limit Field | High limit | Field / UI element | The highest value of the normal range. | [VERIFIED] | In scope |
| 3.3.1.3.9 | High Flag Field | High flag | Field / UI element | The flag shown when the result is above the highest value. | [VERIFIED] | In scope |
| 3.3.1.3.10 | Update Reference Action | تحديث | Action / Function | Applies the entered limits. | [VERIFIED] | In scope |
| 3.3.1.3.11 | Reference Confirm Action | موافق | Action / Function | Confirms the reference limits. | [VERIFIED] | In scope |
| 3.3.1.3.12 | Reference Undo Action | تراجع | Action / Function | Steps back out of the reference operation. | [VERIFIED] | In scope |
| 3.3.1.4 | Reference Matching Rule | — | Rule / Rule | A normal range is entered for each age unit separately. A range given for the ages from one to sixty days is recognised for a patient aged fifteen or thirty-five days and appears in the report, but it is not recognised for a patient aged one month, although one month is the same as thirty days. A separate range must therefore be added for every new age unit, whether a day, a month or a year. | [VERIFIED] | In scope |
| **3.4** | **Contract Price Lists** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.4.1 | Contract Price List Screen | قوائم أسعار التعاقدات | Screen / Container | The screen in which the price lists used for contracts are entered, and the tests and their prices are held. | [VERIFIED] | In scope |
| 3.4.1.1 | Price List Block | — | Block / Container | The list of the registered price lists. | [VERIFIED] | In scope |
| 3.4.1.1.1 | Price List Name Field | — | Field / UI element | The name of the price list. | [VERIFIED] | In scope |
| 3.4.1.1.2 | Price List Add Action | اضافة | Action / Function | Opens the window in which a new price list is named. | [VERIFIED] | In scope |
| 3.4.1.1.3 | Price List Add Confirm Action | Ok | Action / Function | Confirms the name of the new price list. | [VERIFIED] | In scope |
| 3.4.1.1.4 | Price List Edit Action | تعديل | Action / Function | Opens the selected price list for editing. | [VERIFIED] | In scope |
| 3.4.1.1.5 | Price List Save Action | حفظ | Action / Function | Persists the change to the price list. | [VERIFIED] | In scope |
| 3.4.1.1.6 | Price List Delete Action | حذف | Action / Function | Deletes the selected price list. | [VERIFIED] | In scope |
| 3.4.1.1.7 | Price List Delete Confirm Action | موافق | Action / Function | Confirms the deletion of the price list. | [VERIFIED] | In scope |
| 3.4.1.1.8 | Price List Print Action | طباعة القائمة | Action / Function | Prints the selected price list. | [VERIFIED] | In scope |
| 3.4.1.2 | Price List Test Block | — | Block / Container | The tests held in a price list and their prices. | [VERIFIED] | In scope |
| 3.4.1.2.1 | Price List Test Name Field | — | Field / UI element | The test to be added to the list. | [VERIFIED] | In scope |
| 3.4.1.2.2 | Price List Test Price Field | — | Field / UI element | The price of the test within the list. | [VERIFIED] | In scope |
| 3.4.1.2.3 | Price List Test Add Action | اضافة | Action / Function | Adds the chosen test to the price list. | [VERIFIED] | In scope |
| 3.4.1.2.4 | Price List Test Add Confirm Action | موافق | Action / Function | Confirms the price of the test being added. | [VERIFIED] | In scope |
| 3.4.1.2.5 | Price List Test Edit Action | تعديل | Action / Function | Opens the price of the selected test for editing. | [VERIFIED] | In scope |
| 3.4.1.2.6 | Price List Test Save Action | حفظ | Action / Function | Persists the new price of the test. | [VERIFIED] | In scope |
| 3.4.1.2.7 | Price List Test Delete Action | حذف | Action / Function | Removes the selected test from the price list. | [VERIFIED] | In scope |
| **3.5** | **Test Comments** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.5.1 | Test Comments Screen | Test Comments | Screen / Container | The screen in which a comment is registered for a test, or for a profile, so that it is shown in the result entry screen when the comment action is pressed. | [VERIFIED] | In scope |
| 3.5.1.1 | Test Comments Search Field | Search | Field / UI element | Finds a test or a profile in the list. | [VERIFIED] | In scope |
| 3.5.1.2 | Test Comments Grid | ID \ ·  Test Name \ ·  Comment | Grid / UI element | The registered comments, with the columns the identifier, the test name and the comment. | [VERIFIED] | In scope |
| 3.5.1.2.1 | Test Comments ID Column | ID | Grid column / UI element | The identifier of the comment. | [VERIFIED] | In scope |
| 3.5.1.2.2 | Test Comments Test Name Column | Test Name | Grid column / UI element | The test or the profile the comment belongs to. | [VERIFIED] | In scope |
| 3.5.1.2.3 | Test Comments Comment Column | Comment | Grid column / UI element | The comment itself. | [VERIFIED] | In scope |
| 3.5.1.3 | Test Comment Entry Block | — | Block / Container | The form in which a comment is written. | [VERIFIED] | In scope |
| 3.5.1.3.1 | Test Comment Test Name Field | Test name | Field / UI element | The test or the profile the comment is registered for. | [VERIFIED] | In scope |
| 3.5.1.3.2 | Test Comment Text Field | Comment | Field / UI element | The text of the comment. | [VERIFIED] | In scope |
| 3.5.1.4 | Test Comments Action Block | — | Block / Container | The actions of the test comments screen. | [VERIFIED] | In scope |
| 3.5.1.4.1 | Add Test Comment Action | إضافة | Action / Function | Adds a new comment. | [VERIFIED] | In scope |
| 3.5.1.4.2 | Save Test Comment Action | حفظ | Action / Function | Persists the comment. | [VERIFIED] | In scope |
| 3.5.1.4.3 | Edit Test Comment Action | تعديل | Action / Function | Opens the selected comment for editing. | [VERIFIED] | In scope |
| 3.5.1.4.4 | Delete Test Comment Action | حذف | Action / Function | Deletes the selected comment. | [VERIFIED] | In scope |
| 3.5.1.4.5 | Test Comments Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **3.6** | **Test Groups** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.6.1 | Test Groups Screen | مجموعات التحاليل | Screen / Container | The screen in which a group of tests is given a price of its own, for ease of use when the tests and the patients are entered. | [VERIFIED] | In scope |
| 3.6.1.1 | Test Groups Count Indicator | عدد المجموعات المسجلة | Indicator / UI element | The number of registered groups. | [VERIFIED] | In scope |
| 3.6.1.2 | Test Groups Grid | Test ID \ ·  Test Name \ ·  Price | Grid / UI element | The tests of the group, with the columns the test identifier, the test name and the price. | [VERIFIED] | In scope |
| 3.6.1.2.1 | Test Groups Test ID Column | Test ID | Grid column / UI element | The identifier of the test. | [VERIFIED] | In scope |
| 3.6.1.2.2 | Test Groups Test Name Column | Test Name | Grid column / UI element | The name of the test. | [VERIFIED] | In scope |
| 3.6.1.2.3 | Test Groups Price Column | Price | Grid column / UI element | The price of the test within the group. | [VERIFIED] | In scope |
| 3.6.1.3 | Test Group Entry Block | — | Block / Container | The form in which the group and its tests are entered. | [VERIFIED] | In scope |
| 3.6.1.3.1 | Group Name Field | Group Name: | Field / UI element | The name of the group. | [VERIFIED] | In scope |
| 3.6.1.3.2 | Group Test Name Field | Test Name: | Field / UI element | The test to be added to the group. | [VERIFIED] | In scope |
| 3.6.1.3.3 | Group Test Price Field | Price: | Field / UI element | The price of the test within the group. | [VERIFIED] | In scope |
| 3.6.1.3.4 | Tests In Group Count | No. of tests in group | Indicator / UI element | The number of tests the group holds. | [VERIFIED] | In scope |
| 3.6.1.3.5 | Total Group Price Indicator | Total group Price | Indicator / UI element | The total price of the group. | [VERIFIED] | In scope |
| 3.6.1.4 | Test Groups Action Block | — | Block / Container | The actions of the test groups screen. | [VERIFIED] | In scope |
| 3.6.1.4.1 | Add Test Group Action | إضافة مجموعة | Action / Function | Opens the form in which a new group is named. | [VERIFIED] | In scope |
| 3.6.1.4.2 | Save Test Group Action | حفظ | Action / Function | Persists the group or the test. | [VERIFIED] | In scope |
| 3.6.1.4.3 | Edit Test Group Action | تعديل | Action / Function | Opens the selected group or test for editing. | [VERIFIED] | In scope |
| 3.6.1.4.4 | Delete Test Group Action | حذف مجموعة | Action / Function | Deletes the selected group. | [VERIFIED] | In scope |
| 3.6.1.4.5 | Add Group Test Action | إضافة تحليل | Action / Function | Adds the chosen test to the group. | [VERIFIED] | In scope |
| 3.6.1.4.6 | Delete Group Test Action | حذف تحليل | Action / Function | Removes the selected test from the group. | [VERIFIED] | In scope |
| 3.6.1.4.7 | Print Test Group List Action | طباعة القائمة | Action / Function | Prints the list of groups. | [VERIFIED] | In scope |
| 3.6.1.4.8 | Test Groups Undo Action | تراجع | Action / Function | Steps back out of the current group operation. | [VERIFIED] | In scope |
| 3.6.1.4.9 | Test Groups Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **3.7** | **External Entities and Laboratories** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.7.1 | External Entities Screen | الجهات الخارجية والمعامل | Screen / Container | The screen on which the names of the entities and the bodies the laboratory deals with are added, edited or deleted, for use when the patients' data, the accounts and the statistics are entered. | [VERIFIED] | In scope |
| 3.7.1.1 | Entity Count Indicator | عدد الجهات المسجلة | Indicator / UI element | The number of registered entities. | [VERIFIED] | In scope |
| 3.7.1.2 | Entity Search Block | — | Block / Container | The controls that find an entity in the list. | [VERIFIED] | In scope |
| 3.7.1.2.1 | Entity Search Field | بحث باسم الجهة | Field / UI element | Finds an entity by its name. | [VERIFIED] | In scope |
| 3.7.1.2.2 | Entity Search All Option | الكل | Field / UI element | Searches across all the entities. | [VERIFIED] | In scope |
| 3.7.1.3 | Entity Grid | اسم الجهة \ ·  اسم المسئول \ ·  قائمة اسعار التعاقد | Grid / UI element | The registered entities, with the columns the entity name, the person responsible and the contract price list. | [VERIFIED] | In scope |
| 3.7.1.3.1 | Entity Grid Name Column | اسم الجهة | Grid column / UI element | The name of the entity. | [VERIFIED] | In scope |
| 3.7.1.3.2 | Entity Grid Responsible Column | اسم المسئول | Grid column / UI element | The person responsible for the entity. | [VERIFIED] | In scope |
| 3.7.1.3.3 | Entity Grid Price List Column | قائمة اسعار التعاقد | Grid column / UI element | The contract price list applied to the entity. | [VERIFIED] | In scope |
| 3.7.1.4 | Entity Type Options | — | Field group / Container | The kinds of entity that may be registered. | [VERIFIED] | In scope |
| 3.7.1.4.1 | Treating Doctor Entity Type | طبيب معالج | Option / Subfunction | The entity is a treating doctor. | [VERIFIED] | In scope |
| 3.7.1.4.2 | Sent Samples Entity Type | عينات مرسلة | Option / Subfunction | The entity is a laboratory to which samples are sent. | [VERIFIED] | In scope |
| 3.7.1.4.3 | Referring Or Contract Entity Type | جهة إحالة او تعاقد | Option / Subfunction | The entity is a referring entity or a contract. | [VERIFIED] | In scope |
| 3.7.1.5 | Entity Data Block | — | Block / Container | The data of the entity. | [VERIFIED] | In scope |
| 3.7.1.5.1 | Entity Capacity Field | الصفة: | Field / UI element | The capacity or the description of the entity. | [VERIFIED] | In scope |
| 3.7.1.5.2 | Entity Name Field | اسم الجهة أو الهيئة: | Field / UI element | The name of the entity or the body. | [VERIFIED] | In scope |
| 3.7.1.5.3 | Entity City Field | المدينة: | Field / UI element | The city the entity is in. | [VERIFIED] | In scope |
| 3.7.1.5.4 | Entity Address Field | العنوان: | Field / UI element | The address of the entity. | [VERIFIED] | In scope |
| 3.7.1.5.5 | Entity Phone Field | تليفون: | Field / UI element | The telephone number of the entity. | [VERIFIED] | In scope |
| 3.7.1.5.6 | Entity Mobile Field | موبيل: | Field / UI element | The mobile number of the entity. | [VERIFIED] | In scope |
| 3.7.1.5.7 | Entity Fax Field | فاكس: | Field / UI element | The fax number of the entity. | [VERIFIED] | In scope |
| 3.7.1.5.8 | Entity Website Field | Web Site: | Field / UI element | The web address of the entity. | [VERIFIED] | In scope |
| 3.7.1.5.9 | Entity Email Field | E-Mail: | Field / UI element | The electronic mail address of the entity. | [VERIFIED] | In scope |
| 3.7.1.5.10 | Entity Responsible Name Field | اسم المسئول | Field / UI element | The person responsible for the entity. | [VERIFIED] | In scope |
| 3.7.1.5.11 | Entity Responsible Phone Field | تليفون المسئول | Field / UI element | The telephone number of the person responsible. | [VERIFIED] | In scope |
| 3.7.1.6 | Entity Pricing Block | — | Block / Container | The pricing applied to the entity. | [VERIFIED] | In scope |
| 3.7.1.6.1 | Entity Price List Selector | قائمة اسعار التعاقد مع الجهة | Field / UI element | Selects the price list applied to the entity. | [VERIFIED] | In scope |
| 3.7.1.6.2 | Entity Price List Options | — | Field group / Container | The price lists that may be applied to an entity. | [VERIFIED] | In scope |
| 3.7.1.6.2.1 | Individual Price List Option | Individual | Option / Subfunction | The price list applied to individual patients. | [VERIFIED] | In scope |
| 3.7.1.6.2.2 | Lab To Lab Price List Option | Lab To Lab | Option / Subfunction | The price list applied when the entity is another laboratory. | [VERIFIED] | In scope |
| 3.7.1.6.2.3 | Free Price List Option | Free | Option / Subfunction | The price list that charges nothing. | [VERIFIED] | In scope |
| 3.7.1.6.2.4 | Low Price List Option | lowpricelist | Option / Subfunction | The price list of the lower prices. | [VERIFIED] | In scope |
| 3.7.1.6.2.5 | Price Menu One Option | price menu1 | Option / Subfunction | The first of the two additional price lists. | [VERIFIED] | In scope |
| 3.7.1.6.2.6 | Price Menu Two Option | price menu2 | Option / Subfunction | The second of the two additional price lists. | [VERIFIED] | In scope |
| 3.7.1.6.3 | Discount And Commission Block | خصم وعمولات | Field group / Container | The discount and the commission applied to the entity. | [VERIFIED] | In scope |
| 3.7.1.6.3.1 | Entity Discount Percent Field | خصم % | Field / UI element | The discount applied to the entity, as a percentage. | [VERIFIED] | In scope |
| 3.7.1.6.3.2 | Entity Commission Percent Field | عمولة % | Field / UI element | The commission applied to the entity, as a percentage. | [VERIFIED] | In scope |
| 3.7.1.7 | Entity Site Credentials Block | — | Block / Container | The credentials the entity uses to reach the result service. | [VERIFIED] | In scope |
| 3.7.1.7.1 | Entity Site Address Field | Entity Result Service Address | Field / UI element | The address of the result service the entity reaches. | [VERIFIED] | In scope |
| 3.7.1.7.2 | Entity Site Password Field | PW | Field / UI element | The password the entity reaches the result service with. | [VERIFIED] | In scope |
| 3.7.1.7.3 | Entity Site Identifier Field | ID | Field / UI element | The identifier the entity reaches the result service with; the identifier action generates it. | [VERIFIED] | In scope |
| 3.7.1.7.4 | Generate Entity Identifier Action | ID | Action / Function | Generates the identifier of the entity, which is then written into the password field before saving. | [VERIFIED] | In scope |
| 3.7.1.8 | Entity Action Block | — | Block / Container | The actions of the external entities screen. | [VERIFIED] | In scope |
| 3.7.1.8.1 | Save Entity Action | حفظ | Action / Function | Persists the entity. | [VERIFIED] | In scope |
| 3.7.1.8.2 | Edit Entity Action | تعديل | Action / Function | Opens the selected entity for editing. | [VERIFIED] | In scope |
| 3.7.1.8.3 | Delete Entity Action | حذف | Action / Function | Deletes the selected entity. | [VERIFIED] | In scope |
| 3.7.1.8.4 | Entity Undo Action | تراجع | Action / Function | Steps back out of the current entity operation. | [VERIFIED] | In scope |
| 3.7.1.8.5 | Entity Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **3.8** | **Culture Registration** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.8.1 | Culture Entry Slots | — | Block / Container | A culture is not entered as an ordinary test. It is entered in slots that carry their own identifiers, running from one hundred and eighteen to one hundred and thirty-nine. | [VERIFIED] | In scope |
| 3.8.1.1 | Culture Slot Identifier Range | — | Rule / Rule | The slots reserved for the cultures run from the identifier one hundred and eighteen to the identifier one hundred and thirty-nine. | [VERIFIED] | In scope |
| 3.8.1.2 | Culture Group Name Selector | CULTURE AND SENSITIVITY | Field / UI element | The group a culture is registered under, chosen from the group name field of the test data screen. | [VERIFIED] | In scope |
| 3.8.1.3 | Culture Edit Action | تعديل | Action / Function | Opens the data of the chosen culture slot for editing. | [VERIFIED] | In scope |
| 3.8.1.4 | Culture Save Action | حفظ | Action / Function | Persists the culture data. | [VERIFIED] | In scope |
| 3.8.1.5 | Culture Configuration File | Configuration settings | File / UI element | A configuration file in the program's data folder that holds the settings of the cultures. The file's purpose and the way it is maintained are not established. | [UNVERIFIED] | In scope |
| **3.9** | **Culture Antibiotics** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.9.1 | Antibiotics Screen | Culture Antibiotics | Screen / Container | The screen in which the antibiotics of each culture are entered and edited, with the commercial names to be shown, and with whether the antibiotic is suitable for pregnant women or for children. | [VERIFIED] | In scope |
| 3.9.1.1 | Antibiotic Culture Selector | Culture | Field / UI element | The culture whose antibiotics are being entered. | [VERIFIED] | In scope |
| 3.9.1.2 | International Antibiotic Option | International Antibiotic | Field / UI element | Enters the antibiotic under its international name. | [VERIFIED] | In scope |
| 3.9.1.3 | Antibiotic By Abbreviation Option | Antibiotic By Abbr. | Field / UI element | Enters the antibiotic under the abbreviation used in the program. | [VERIFIED] | In scope |
| 3.9.1.4 | Antibiotics Count In Culture | Antibiotics No. in culture | Indicator / UI element | The number of antibiotics the culture holds. | [VERIFIED] | In scope |
| 3.9.1.5 | Antibiotic Entry Block | — | Block / Container | The data of one antibiotic. | [VERIFIED] | In scope |
| 3.9.1.5.1 | Antibiotic Symbol Field | Symb. | Field / UI element | The symbol of the antibiotic. | [VERIFIED] | In scope |
| 3.9.1.5.2 | Antibiotic Scientific Name Field | Scientific Name | Field / UI element | The scientific name of the antibiotic. | [VERIFIED] | In scope |
| 3.9.1.5.3 | Antibiotic Sensitivity Field | Sensitivity | Field / UI element | The sensitivity recorded for the antibiotic. | [VERIFIED] | In scope |
| 3.9.1.5.4 | Antibiotic Pregnant Field | Pregnant | Field / UI element | Whether the antibiotic is suitable for pregnant women. | [VERIFIED] | In scope |
| 3.9.1.5.5 | Antibiotic Children Field | Children | Field / UI element | Whether the antibiotic is suitable for children. | [VERIFIED] | In scope |
| 3.9.1.5.6 | Antibiotic Commercial Names Field | Commercial Names | Field / UI element | The commercial names shown for the antibiotic. | [VERIFIED] | In scope |
| 3.9.1.6 | Antibiotic Action Block | — | Block / Container | The actions of the antibiotics screen. | [VERIFIED] | In scope |
| 3.9.1.6.1 | Add Antibiotic Action | — | Action / Function | Adds the chosen antibiotic; the antibiotic is entered by pressing it twice, and the entry is then saved. | [VERIFIED] | In scope |
| 3.9.1.6.2 | Save Antibiotic Action | حفظ | Action / Function | Persists the antibiotic. | [VERIFIED] | In scope |
| 3.9.1.6.3 | Edit Antibiotic Action | تعديل | Action / Function | Opens the selected antibiotic for editing. | [VERIFIED] | In scope |
| 3.9.1.6.4 | Delete Antibiotic Action | حذف | Action / Function | Deletes the selected antibiotic. | [VERIFIED] | In scope |
| 3.9.1.6.5 | Antibiotic Print Action | Print | Action / Function | Prints the antibiotics of the culture. | [VERIFIED] | In scope |
| 3.9.1.6.6 | Antibiotics Undo Action | تراجع | Action / Function | Steps back out of the current antibiotic operation. | [VERIFIED] | In scope |
| 3.9.1.6.7 | Antibiotics Main List Action | القائمة_MAIN | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **3.10** | **Samples Sent Outside** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.10.1 | Samples Sent Outside Procedure | — | Block / Container | Where certain tests are performed in another laboratory, the test is marked as a sample sent outside, the laboratory it is sent to is chosen, and the price paid to that laboratory and the price taken from the patient are both recorded. | [VERIFIED] | In scope |
| 3.10.1.1 | Sent Outside Test Selection Action | — | Action / Function | The test to be sent outside is found in the test data screen and selected for editing. | [VERIFIED] | In scope |
| 3.10.1.2 | Sent Outside Lab Selection | — | Function / Function | The receiving laboratory is chosen once the sent-outside field is set. | [VERIFIED] | In scope |
| 3.10.1.3 | Cost Price Recording | — | Function / Function | The price of the test paid to the external laboratory is recorded in the cost price field. | [VERIFIED] | In scope |
| 3.10.1.4 | Patient Price Recording | — | Function / Function | The price taken from the patient is recorded in the patient price field. | [VERIFIED] | In scope |
| **3.11** | **Result Service Accounts** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.11.1 | Doctor Account Creation | — | Block / Container | An account is created for a treating doctor so that the doctor may reach the result service. When the identifier action is pressed the program generates an identifier for the doctor; the same number is written into the password field, and the entry is then saved. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 3.11.1.1 | Doctor Account Identifier Generation | — | Function / Function | The program generates an identifier for the doctor. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 3.11.1.2 | Doctor Account Password Setting | — | Function / Function | The generated identifier is written into the password field of the same entity before saving. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 3.11.1.3 | Doctor Result Access | — | Function / Function | When a patient is registered with the doctor as the treating doctor and the patient's result is uploaded to the result service, the doctor may see the result of the patient the doctor referred, by signing in to the result service with the identifier and the password that were created. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 3.11.2 | Laboratory Account Creation | — | Block / Container | An account is created for a laboratory so that the laboratory may reach the result service for the patients whose samples it sent. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 3.11.2.1 | Laboratory Contract Registration | — | Function / Function | The patient is registered as a contract, and the laboratory is chosen as the referring entity. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| 3.11.2.2 | Laboratory Result Access | — | Function / Function | The patient's result is uploaded to the result service, and the laboratory sees the result of the patient who sent samples to it by signing in to the result service with the identifier and the password that were created. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — online portal ecosystem |
| **3.12** | **Reference Range List and Form Details** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.12.1 | Reference Range List Grid | Gender  ·  From  ·  To  ·  Age unit  ·  Reference range  ·  low limit  ·  high limit  ·  Test unit  ·  Low flag  ·  High flag | Grid / UI element | The list of the normal ranges of the selected test; one row is one normal range. | [VERIFIED] | In scope |
| 3.12.1.1 | Reference List Gender Column | Gender | Grid column / UI element | The sex the normal range applies to. | [VERIFIED] | In scope |
| 3.12.1.2 | Reference List From Column | From | Grid column / UI element | The age the normal range starts at. | [VERIFIED] | In scope |
| 3.12.1.3 | Reference List To Column | To | Grid column / UI element | The age the normal range ends at. | [VERIFIED] | In scope |
| 3.12.1.4 | Reference List Age Unit Column | Age unit | Grid column / UI element | The unit of the age of the normal range. | [VERIFIED] | In scope |
| 3.12.1.5 | Reference List Range Column | Reference range | Grid column / UI element | The normal range as it is written. | [VERIFIED] | In scope |
| 3.12.1.6 | Reference List Low Limit Column | low limit | Grid column / UI element | The lowest value of the normal range. | [VERIFIED] | In scope |
| 3.12.1.7 | Reference List High Limit Column | high limit | Grid column / UI element | The highest value of the normal range. | [VERIFIED] | In scope |
| 3.12.1.8 | Reference List Test Unit Column | Test unit | Grid column / UI element | The unit of the result the range applies to. | [VERIFIED] | In scope |
| 3.12.1.9 | Reference List Low Flag Column | Low flag | Grid column / UI element | The flag shown when the result is below the lowest value. | [VERIFIED] | In scope |
| 3.12.1.10 | Reference List High Flag Column | High flag | Grid column / UI element | The flag shown when the result is above the highest value. | [VERIFIED] | In scope |
| 3.12.2 | Reference Range Form Block | — | Block / Container | The further controls of the form in which one normal range is entered. | [VERIFIED] | In scope |
| 3.12.2.1 | Range For Selector | Range for | Field / UI element | The way the range applies; the options are given as Range For Options. | [VERIFIED] | In scope |
| 3.12.2.1.1 | Range For All Option | For all | Option / Subfunction | The range applies to every patient. | [VERIFIED] | In scope |
| 3.12.2.1.2 | Range For By Sex Only Option | By sex only | Option / Subfunction | The range applies according to the patient's sex. | [VERIFIED] | In scope |
| 3.12.2.1.3 | Range For By Sex And Age Option | By sex and age | Option / Subfunction | The range applies according to the patient's sex and age. | [VERIFIED] | In scope |
| 3.12.2.1.4 | Range For By Age Only Option | By age only | Option / Subfunction | The range applies according to the patient's age. | [VERIFIED] | In scope |
| 3.12.2.2 | Range For Male Option | Male | Option / Subfunction | The range is entered for male patients. | [VERIFIED] | In scope |
| 3.12.2.3 | Range For Female Option | Female | Option / Subfunction | The range is entered for female patients. | [VERIFIED] | In scope |
| 3.12.2.4 | Range Age From Field | Age from | Field / UI element | The age the range starts at. | [VERIFIED] | In scope |
| 3.12.2.5 | Range Age To Field | to | Field / UI element | The age the range ends at. | [VERIFIED] | In scope |
| 3.12.2.6 | Range Age Unit Selector | Age unit | Field / UI element | The unit of the age of the range. | [VERIFIED] | In scope |
| 3.12.2.7 | Reference Range Text Area | القيمة المرجعية | Field / UI element | The normal range as it is written and as it is shown to the user. | [VERIFIED] | In scope |
| 3.12.2.8 | Pregnant Only Flag | for pregnant only | Field / UI element | The range applies to the pregnant patients alone. | [VERIFIED] | In scope |
| 3.12.2.9 | Reference Count Indicator | Number Of References | Field / UI element | The number of the normal ranges the test holds. | [VERIFIED] | In scope |
| 3.12.2.10 | Feed The Analysis Action | تغذية التحليل | Action / Function | Feeds the entered range into the analysis. The effect of the action is not established in the source. | [UNVERIFIED] | In scope |
| **3.13** | **Antibiotic List and Commercial Name Marks** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.13.1 | Culture Antibiotic List Grid | Symb.  ·  Scientific Name  ·  Sensitivity  ·  Pregnant  ·  Children | Grid / UI element | The antibiotics entered for the selected culture; one row is one antibiotic. | [VERIFIED] | In scope |
| 3.13.1.1 | Culture Antibiotic Symbol Column | Symb. | Grid column / UI element | The symbol of the antibiotic. | [VERIFIED] | In scope |
| 3.13.1.2 | Culture Antibiotic Scientific Name Column | Scientific Name | Grid column / UI element | The scientific name of the antibiotic. | [VERIFIED] | In scope |
| 3.13.1.3 | Culture Antibiotic Sensitivity Column | Sensitivity | Grid column / UI element | The sensitivity recorded for the antibiotic. | [VERIFIED] | In scope |
| 3.13.1.4 | Culture Antibiotic Pregnant Column | Pregnant | Grid column / UI element | Whether the antibiotic is suitable for pregnant women. | [VERIFIED] | In scope |
| 3.13.1.5 | Culture Antibiotic Children Column | Children | Grid column / UI element | Whether the antibiotic is suitable for children. | [VERIFIED] | In scope |
| 3.13.2 | Commercial Name Print Flag | Print | Field / UI element | The mark beside a commercial name that decides whether that name is printed with the antibiotic. | [VERIFIED] | In scope |
| **3.14** | **Test Group Screen Details** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.14.1 | Laboratory Test Group List | — | List / UI element | The groups registered by the laboratory, named in the list beside the group form; one entry is one group. | [VERIFIED] | In scope |
| 3.14.2 | Group Name List Field | Group Name: | Field / UI element | The name of the group that is being entered or edited. | [VERIFIED] | In scope |
| 3.14.3 | Add Sub Group Action | اضافة مسجموعة | Action / Function | Adds a sub-group to the selected group. | [VERIFIED] | In scope |
| 3.14.4 | Group Fixed Price Rule | — | Rule / Rule | A group of tests may be given a price of its own, so that the group is entered as one item on the patient's request and so that the entry of the tests and of the patients is quicker. | [VERIFIED] | In scope |
| **3.15** | **System Window Further Entries** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 3.15.1 | Purchase Lab Devices Item | شراء أجهزة المختبر | Menu item / Subfunction | Opens the screen of the purchase of the laboratory's devices. The screen is not displayed, so its fields are not established. | [NOT-SHOWN] | In scope |
| 3.15.2 | Read Analysis Report Item | قراءة تقرير التحاليل | Menu item / Subfunction | Opens the reading of the report of the analyses. The screen is not displayed, so its fields are not established. | [NOT-SHOWN] | In scope |
| 3.15.3 | Card And Identity Data Item | الكارنيه والبيانات المعرفية | Menu item / Subfunction | Opens the screen of the card and of the identifying data. The screen is not displayed, so its fields are not established. | [NOT-SHOWN] | In scope |
| 3.15.4 | System Menu Rendering Difference Rule | — | Rule / Rule | The two renderings of the system window do not carry the same entries. The window of the showcase carries sixteen entries, of which three are not in the menu of the guide; the menu of the guide carries seventeen, of which four are not in the window of the showcase. The two renderings are recorded as they are, and the difference is not resolved. | [VERIFIED] | In scope |
| 3.15.5 | System Window Capability Block | — | Block / Container | The four capabilities the text of the system window states. | [VERIFIED] | In scope |
| 3.15.5.1 | System Tests And Ranges Capability | — | Function / Function | The tests and the normal values are entered and edited, and the units and the groups are chosen. | [VERIFIED] | In scope |
| 3.15.5.2 | System Antibiotics Capability | — | Function / Function | The antibiotics are entered against the results of the analyses, together with the substances and the kits used. | [VERIFIED] | In scope |
| 3.15.5.3 | System Lists Capability | — | Function / Function | The units, the groups, the names and the other lists are entered. | [VERIFIED] | In scope |
| 3.15.5.4 | System Devices Capability | — | Function / Function | The devices and the equipment of the laboratory are entered, with their state, their periodic maintenance and their calibration date. | [VERIFIED] | In scope |

### §8.3 Workflows

### Open the data of the system

1. Press the system ribbon item; the menu of the data screens opens. → 3.1.1
2. Choose the data screen to work on: the test data, the units, the comments, the groups, the antibiotics, the barcode types, the external entities, the contract price lists, the branches, the patient titles, the patient names and numbers, the laboratory accounts, the work groups, the result-service accounts, the printing of the price list, or the devices and equipment. → 3.1.2

### Add or edit a test

1. Open the test data screen; every test registered in the program's database is listed. → 3.1.2.1, 3.2.1
2. Find the test to edit by its name, by the group that contains it, or by its identifier. → 3.2.1.1.1, 3.2.1.1.2, 3.2.1.1.3
3. Press edit; the data of the test becomes editable. → 3.2.1.4.2
4. The editable data are the test's name and the name it carries on the report, the name it carries on the receipt, the group it belongs to, its barcode, the time the test needs and the receipt time the program computes from it, whether the sample is sent to another laboratory with that laboratory's price, the patient's price, and the laboratory-to-laboratory price. → 3.2.1.3
5. Press save; to abandon the change press undo. → 3.2.1.4.3, 3.2.1.4.4
6. To add a new test, press the add-test action, write the test's name and its data, then press save. → 3.2.1.4.1

### Enter the reference values of a test

1. Find the test, then press the reference-values action. → 3.2.1.1, 3.2.1.4.6
2. Press the add-range action; choose the patient's sex and the age range, and enter the lowest and the highest value. → 3.3.1.1, 3.3.1.2
3. To have a comment shown when the result falls below the lowest value, write it in the low comment; to have one shown when the result rises above the highest value, write it in the high comment. The comment appears automatically on the patient's report when the comment cell is pressed twice. → 3.3.1.1.5, 3.3.1.1.6
4. Press save. To edit a range, select it and press edit, then save; to delete one, select it and press delete. → 3.3.1.2, 3.3.1.3, 3.3.1.4
5. Enter a separate range for every age unit, whether a day, a month or a year. → 3.3.1.4

### Maintain a contract price list

1. Open the contract price lists, press add, write the name of the new list, and press Ok. → 3.1.2.9, 3.4.1.1.2, 3.4.1.1.1, 3.4.1.1.3
2. To add a test to the list, choose it and press add, select the test, enter its price, and press the confirm action. → 3.4.1.2.3, 3.4.1.2.1, 3.4.1.2.2, 3.4.1.2.4
3. To change a price, press the test, press edit, enter the new price and press save. → 3.4.1.2.5, 3.4.1.2.6
4. To remove a test from the list, press the test and press delete. → 3.4.1.2.7
5. To change the name of a list, press edit below the list, change the name and press save. → 3.4.1.1.4, 3.4.1.1.5
6. To print a list, choose it and press the print action; to delete a list, choose it, press delete and then the confirm action. → 3.4.1.1.8, 3.4.1.1.6, 3.4.1.1.7

### Register a fixed comment on a test

1. Open the test comments. → 3.1.2.3
2. Find the test, write the comment, and save. → 3.5.1.1, 3.5.1.3.1, 3.5.1.3.2, 3.5.1.4.2
3. More than one comment may be registered for the same test; the comment is shown in the result entry screen when the comment action is pressed. → 3.5.1.3.2

### Maintain a test group

1. Open the test groups; the number of registered groups is shown. → 3.1.2.4, 3.6.1.1
2. Press the add-group action, write the name of the group, and press save. → 3.6.1.4.1, 3.6.1.3.1, 3.6.1.4.2
3. To add a test, write the test's name and its price and press the add-test action; the number of tests in the group and the total group price are shown. → 3.6.1.3.2, 3.6.1.3.3, 3.6.1.4.5, 3.6.1.3.4, 3.6.1.3.5
4. To remove a test, press it and press the delete-test action; to remove the group, press the delete-group action. → 3.6.1.4.6, 3.6.1.4.4
5. To print the list of groups, press the print action. → 3.6.1.4.7

### Register a treating doctor or a referring entity

1. Open the external entities screen. → 3.1.2.8
2. Press the identifier action; the program generates an identifier for the entity, and the same number is written into the password field before saving. → 3.7.1.7.4, 3.7.1.7.2
3. Enter the entity's capacity, name, city, address, telephone, mobile, fax, web address and electronic mail, and the name and telephone number of the person responsible. → 3.7.1.5.1, 3.7.1.5.2, 3.7.1.5.3, 3.7.1.5.4, 3.7.1.5.5, 3.7.1.5.6, 3.7.1.5.7, 3.7.1.5.8, 3.7.1.5.9, 3.7.1.5.10, 3.7.1.5.11
4. Choose the kind of entity: a treating doctor, a laboratory to which samples are sent, or a referring entity or a contract. → 3.7.1.4.1, 3.7.1.4.2, 3.7.1.4.3
5. Choose the price list applied to the entity, and set the discount and the commission. → 3.7.1.6.1, 3.7.1.6.3.1, 3.7.1.6.3.2
6. Press save. → 3.7.1.8.1
7. To let the entity reach the result service, enter the address of the service, the password and the identifier that were created. → 3.7.1.7.1, 3.7.1.7.2, 3.7.1.7.3

### Register a culture

1. Open the test data screen; the cultures are not entered as ordinary tests but in slots carrying their own identifiers, running from one hundred and eighteen to one hundred and thirty-nine. → 3.2.1, 3.8.1.1
2. Search by the identifier, press the chosen slot, then press edit. → 3.2.1.1.3, 3.2.1.4.2, 3.8.1.3
3. Enter the culture's data, choose the group the culture is registered under, complete the remaining data, and press save. → 3.8.1.2, 3.8.1.4

### Register the antibiotics of a culture

1. Open the antibiotics screen. → 3.1.2.6
2. Choose the culture, and choose whether the antibiotic is entered under its international name or under the abbreviation used in the program. → 3.9.1.1, 3.9.1.2, 3.9.1.3
3. Enter the antibiotic's symbol, scientific name, sensitivity, whether it is suitable for pregnant women and for children, and the commercial names to be shown. → 3.9.1.5.1, 3.9.1.5.2, 3.9.1.5.3, 3.9.1.5.4, 3.9.1.5.5, 3.9.1.5.6
4. Press the antibiotic twice, then save. → 3.9.1.6.1, 3.9.1.6.2
5. The number of antibiotics the culture holds is shown. → 3.9.1.4

### Send a test's sample to another laboratory

1. Open the test data screen, find the test to be sent outside, select it and press edit. → 3.2.1, 3.10.1.1
2. Set the sent-outside field, then choose the laboratory the sample is sent to. → 3.2.1.3.21, 3.2.1.3.22, 3.10.1.2
3. Record the price of the test paid to the external laboratory in the cost price field, and the price taken from the patient in the patient price field. → 3.10.1.3, 3.10.1.4

### Give a doctor or a laboratory access to the result service

1. Register the entity on the external entities screen and create its identifier and password. → 3.7.1.7.4, 3.7.1.7.2, 3.7.1.7.3
2. **For a doctor:** register the patient with that doctor as the treating doctor, then upload the patient's result to the result service. The doctor then sees the result of the patient they referred, by signing in with the identifier and the password that were created. → 3.11.1.1, 3.11.1.2, 3.11.1.3
3. **For a laboratory:** register the patient as a contract and choose that laboratory as the referring entity, then upload the patient's result to the result service. The laboratory then sees the result of the patient who sent samples to it, by signing in with the identifier and the password that were created. → 3.11.2.1, 3.11.2.2

### §8.4 Rules, validations and conditional behaviour

| ID | Rule | Tag |
|---|---|---|
| 3.1.2 | The system module offers one screen per kind of data; the data of the system are entered and maintained through these screens only. | [VERIFIED] |
| 3.2.1.3.11 | The patient's receipt time is computed by the program from the time the test needs. | [VERIFIED] |
| 3.2.1.3.23 | The cost price is the price paid to the external laboratory, and the patient price is the price taken from the patient. | [VERIFIED] |
| 3.3.1.4 | A normal range is recognised only for the age unit it was entered for; a separate range is needed for every age unit. | [VERIFIED] |
| 3.3.1.1.5 | The low comment appears automatically on the patient's report when the result falls below the lowest value, on pressing the comment cell twice. | [VERIFIED] |
| 3.3.1.1.6 | The high comment appears automatically on the patient's report when the result rises above the highest value, on pressing the comment cell twice. | [VERIFIED] |
| 3.4.1.2 | A test is added to a price list with a price of its own, which is independent of the test's price in the test data screen. | [VERIFIED] |
| 3.5.1 | More than one comment may be registered for the same test. | [VERIFIED] |
| 3.6.1 | A group of tests is given a price of its own, so that the tests and the patients may be entered more quickly. | [VERIFIED] |
| 3.7.1.4 | The kind of entity decides the way the entity is used: as a treating doctor, as a laboratory to which samples are sent, or as a referring entity or a contract. | [VERIFIED] |
| 3.7.1.7.4 | The identifier and the password of the entity are the same number. | [VERIFIED] |
| 3.8.1.1 | The cultures are not entered as ordinary tests, and are held in slots with their own identifiers. | [VERIFIED] |
| 3.9.1.5.4 | Each antibiotic records whether it is suitable for pregnant women and for children. | [VERIFIED] |
| 3.11.1.2 | The doctor reaches the result service with the identifier and the password that were created on the external entities screen. | [VERIFIED] |
| 3.11.2.2 | The laboratory reaches the result service with the identifier and the password that were created on the external entities screen. | [VERIFIED] |

### §8.5 Outputs

The artefacts below are produced by the elements listed. The content of each artefact is defined once, in the element's own definition, and is not repeated here.

| ID | Output | Tag |
|---|---|---|
| 3.1.2.16 | Printed test price list | [VERIFIED] |
| 3.4.1.1.8 | Printed contract price list | [VERIFIED] |
| 3.6.1.4.7 | Printed list of test groups | [VERIFIED] |
| 3.9.1.6.5 | Printed antibiotics of a culture | [VERIFIED] |

### §8.6 Screens and interface structure

| Screen | Element identifiers |
|---|---|
| System Module Entry | 3.1, 3.1.1, 3.1.2, 3.1.2.1, 3.1.2.2, 3.1.2.3, 3.1.2.4, 3.1.2.5, 3.1.2.6, 3.1.2.7, 3.1.2.8, 3.1.2.9, 3.1.2.10, 3.1.2.11, 3.1.2.12, 3.1.2.13, 3.1.2.14, 3.1.2.15, 3.1.2.16, 3.1.2.17, 3.1.3, 3.1.3.1, 3.1.3.2, 3.1.3.3, 3.1.3.4 |
| Test Data Screen | 3.2.1, 3.2.1.1, 3.2.1.1.1, 3.2.1.1.2, 3.2.1.1.3, 3.2.1.1.4, 3.2.1.2, 3.2.1.2.1, 3.2.1.2.1.1, 3.2.1.2.1.2, 3.2.1.2.1.3, 3.2.1.2.1.4, 3.2.1.2.1.5, 3.2.1.2.1.6, 3.2.1.2.1.7, 3.2.1.2.1.8, 3.2.1.2.1.9, 3.2.1.2.1.10, 3.2.1.3, 3.2.1.3.1, 3.2.1.3.2, 3.2.1.3.3, 3.2.1.3.4, 3.2.1.3.5, 3.2.1.3.6, 3.2.1.3.7, 3.2.1.3.8, 3.2.1.3.9, 3.2.1.3.10, 3.2.1.3.11, 3.2.1.3.12, 3.2.1.3.13, 3.2.1.3.14, 3.2.1.3.15, 3.2.1.3.16, 3.2.1.3.17, 3.2.1.3.18, 3.2.1.3.19, 3.2.1.3.20, 3.2.1.3.21, 3.2.1.3.22, 3.2.1.3.23, 3.2.1.3.24, 3.2.1.3.25, 3.2.1.3.26, 3.2.1.3.27, 3.2.1.3.28, 3.2.1.3.29, 3.2.1.4, 3.2.1.4.1, 3.2.1.4.2, 3.2.1.4.3, 3.2.1.4.4, 3.2.1.4.5, 3.2.1.4.6, 3.2.1.4.7 |
| Reference Values Screen | 3.3.1, 3.3.1.1, 3.3.1.1.1, 3.3.1.1.2, 3.3.1.1.3, 3.3.1.1.4, 3.3.1.1.5, 3.3.1.1.6, 3.3.1.2, 3.3.1.2.1, 3.3.1.2.2, 3.3.1.2.3, 3.3.1.2.4, 3.3.1.3, 3.3.1.3.1, 3.3.1.3.2, 3.3.1.3.3, 3.3.1.3.4, 3.3.1.3.5, 3.3.1.3.6, 3.3.1.3.7, 3.3.1.3.8, 3.3.1.3.9, 3.3.1.3.10, 3.3.1.3.11, 3.3.1.3.12, 3.3.1.4 |
| Contract Price List Screen | 3.4.1, 3.4.1.1, 3.4.1.1.1, 3.4.1.1.2, 3.4.1.1.3, 3.4.1.1.4, 3.4.1.1.5, 3.4.1.1.6, 3.4.1.1.7, 3.4.1.1.8, 3.4.1.2, 3.4.1.2.1, 3.4.1.2.2, 3.4.1.2.3, 3.4.1.2.4, 3.4.1.2.5, 3.4.1.2.6, 3.4.1.2.7 |
| Test Comments Screen | 3.5.1, 3.5.1.1, 3.5.1.2, 3.5.1.2.1, 3.5.1.2.2, 3.5.1.2.3, 3.5.1.3, 3.5.1.3.1, 3.5.1.3.2, 3.5.1.4, 3.5.1.4.1, 3.5.1.4.2, 3.5.1.4.3, 3.5.1.4.4, 3.5.1.4.5 |
| Test Groups Screen | 3.6.1, 3.6.1.1, 3.6.1.2, 3.6.1.2.1, 3.6.1.2.2, 3.6.1.2.3, 3.6.1.3, 3.6.1.3.1, 3.6.1.3.2, 3.6.1.3.3, 3.6.1.3.4, 3.6.1.3.5, 3.6.1.4, 3.6.1.4.1, 3.6.1.4.2, 3.6.1.4.3, 3.6.1.4.4, 3.6.1.4.5, 3.6.1.4.6, 3.6.1.4.7, 3.6.1.4.8, 3.6.1.4.9 |
| External Entities Screen | 3.7.1, 3.7.1.1, 3.7.1.2, 3.7.1.2.1, 3.7.1.2.2, 3.7.1.3, 3.7.1.3.1, 3.7.1.3.2, 3.7.1.3.3, 3.7.1.4, 3.7.1.4.1, 3.7.1.4.2, 3.7.1.4.3, 3.7.1.5, 3.7.1.5.1, 3.7.1.5.2, 3.7.1.5.3, 3.7.1.5.4, 3.7.1.5.5, 3.7.1.5.6, 3.7.1.5.7, 3.7.1.5.8, 3.7.1.5.9, 3.7.1.5.10, 3.7.1.5.11, 3.7.1.6, 3.7.1.6.1, 3.7.1.6.2, 3.7.1.6.2.1, 3.7.1.6.2.2, 3.7.1.6.2.3, 3.7.1.6.2.4, 3.7.1.6.2.5, 3.7.1.6.2.6, 3.7.1.6.3, 3.7.1.6.3.1, 3.7.1.6.3.2, 3.7.1.7, 3.7.1.7.1, 3.7.1.7.2, 3.7.1.7.3, 3.7.1.7.4, 3.7.1.8, 3.7.1.8.1, 3.7.1.8.2, 3.7.1.8.3, 3.7.1.8.4, 3.7.1.8.5 |
| Antibiotics Screen | 3.9.1, 3.9.1.1, 3.9.1.2, 3.9.1.3, 3.9.1.4, 3.9.1.5, 3.9.1.5.1, 3.9.1.5.2, 3.9.1.5.3, 3.9.1.5.4, 3.9.1.5.5, 3.9.1.5.6, 3.9.1.6, 3.9.1.6.1, 3.9.1.6.2, 3.9.1.6.3, 3.9.1.6.4, 3.9.1.6.5, 3.9.1.6.6, 3.9.1.6.7 |

### §8.7 Uncertainties and unshown elements

| ID | Item | Tag |
|---|---|---|
| 3.8.1.5 | The configuration file's purpose and the way it is maintained are not established. | [UNVERIFIED] |
| 3.1.2.7 | The content of the barcode types screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.10 | The content of the laboratory branches screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.11 | The content of the patient titles and definitions screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.12 | The content of the patient names and telephone numbers screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.13 | The content of the laboratory accounts screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.14 | The content of the work groups screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.15 | The content of the result screen account screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.17 | The content of the laboratory devices and equipment screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.2 | The content of the test units screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.1.2.5 | The content of the custom groups screen is not established; the screen is named in the system menu but is not shown. | [NOT-SHOWN] |
| 3.2.1.3.21 | Whether the sent-outside field of a test is set on the test or on each of its tubes is not established. | [UNVERIFIED] |
| 3.4.1 | The structure of the price list screen is inferred from the prose; the screen itself is not shown in full. | [UNVERIFIED] |
| 3.9.1.6.7 | The label of the main-list action on the antibiotics screen is not fully legible. | [UNVERIFIED] |
| — | No validation, rejection message or recovery path is established for the data screens of the system module. | [NOT-SHOWN] |

## §9 Unit 4 — Users, Roles and Lab Performance Monitoring

### §9.1 Functional hierarchy

```
4  Users, Roles and Lab Performance Monitoring
├── 4.1  Users Module Entry
│   ├── 4.1.1  Users Ribbon Item
│   ├── 4.1.2  Users Menu
│   │   ├── 4.1.2.1  Create Users Item
│   │   ├── 4.1.2.2  Change Password Item
│   │   ├── 4.1.2.3  Login Detector Item
│   │   └── 4.1.2.4  Attendance And Departure Item
│   ├── 4.1.3  Users Module Capabilities
│   │   ├── 4.1.3.1  User Creation Capability
│   │   ├── 4.1.3.2  User Maintenance Capability
│   │   ├── 4.1.3.3  Attendance Capability
│   │   └── 4.1.3.4  User Movement Capability
│   └── 4.1.4  Second Password Dialog
│       ├── 4.1.4.1  Second Password Prompt
│       ├── 4.1.4.2  Second Password Field
│       ├── 4.1.4.3  Second Password Confirm Action
│       ├── 4.1.4.4  Second Password Cancel Action
│       └── 4.1.4.5  Second Password Rule
├── 4.2  User Permissions Screen
│   └── 4.2.1  User Permissions Screen
│       ├── 4.2.1.1  User List Block
│       │   ├── 4.2.1.1.1  User Count Indicator
│       │   └── 4.2.1.1.2  User List
│       ├── 4.2.1.2  User Data Block
│       │   ├── 4.2.1.2.1  User Name Field
│       │   ├── 4.2.1.2.2  User Password Field
│       │   ├── 4.2.1.2.3  User Internal Password Field
│       │   └── 4.2.1.2.4  User Last Login Field
│       ├── 4.2.1.3  Permission Scope Block
│       │   ├── 4.2.1.3.1  Absolute Permission Option
│       │   ├── 4.2.1.3.2  Limited Permission Option
│       │   ├── 4.2.1.3.3  User Branch Selector
│       │   └── 4.2.1.3.4  Branch Selector Rule
│       ├── 4.2.1.4  Permission Block
│       │   ├── 4.2.1.4.1  Patient Data Permission Check Box
│       │   ├── 4.2.1.4.2  Result Entry Permission Check Box
│       │   ├── 4.2.1.4.3  Result Review Permission Check Box
│       │   ├── 4.2.1.4.4  Result Print Permission Check Box
│       │   ├── 4.2.1.4.5  Balance Withholding Permission Check Box
│       │   ├── 4.2.1.4.6  Result Delivery Permission Check Box
│       │   ├── 4.2.1.4.7  Discount Limit Check Box
│       │   ├── 4.2.1.4.8  Discount Limit Percent Field
│       │   ├── 4.2.1.4.9  Work Sheet Print Permission Check Box
│       │   ├── 4.2.1.4.10  Patient Delete Permission Check Box
│       │   ├── 4.2.1.4.11  System Data Permission Check Box
│       │   ├── 4.2.1.4.12  Accounts Permission Check Box
│       │   ├── 4.2.1.4.13  Statistics Permission Check Box
│       │   ├── 4.2.1.4.14  Stocktaking Permission Check Box
│       │   └── 4.2.1.4.15  Attendance Permission Check Box
│       ├── 4.2.1.5  Work Time Block
│       │   ├── 4.2.1.5.1  Work Start Time Field
│       │   ├── 4.2.1.5.2  Work End Time Field
│       │   ├── 4.2.1.5.3  Rest Period Exists Option
│       │   └── 4.2.1.5.4  Rest Period Duration Field
│       └── 4.2.1.6  User Action Block
│           ├── 4.2.1.6.1  Add User Action
│           ├── 4.2.1.6.2  Edit User Action
│           ├── 4.2.1.6.3  Save User Action
│           ├── 4.2.1.6.4  Delete User Action
│           ├── 4.2.1.6.5  User Undo Action
│           └── 4.2.1.6.6  User Main List Action
├── 4.3  Permission Enforcement
│   ├── 4.3.1  Permission Alert Dialog
│   │   ├── 4.3.1.1  Permission Alert Title
│   │   ├── 4.3.1.2  Permission Alert Message
│   │   └── 4.3.1.3  Permission Alert Confirm Action
│   └── 4.3.2  First Login Notice
├── 4.4  Attendance and Departure
│   ├── 4.4.1  Attendance Screen
│   │   ├── 4.4.1.1  Register Attendance Action
│   │   ├── 4.4.1.2  Register Departure Action
│   │   ├── 4.4.1.3  Start Rest Period Action
│   │   ├── 4.4.1.4  End Rest Period Action
│   │   └── 4.4.1.5  Attendance Main List Action
│   └── 4.4.2  Attendance Working Rule
└── 4.5  Login Detector
    ├── 4.5.1  Login Detector Screen
    │   ├── 4.5.1.1  Login Detector Filter Block
    │   │   ├── 4.5.1.1.1  Login From Date Field
    │   │   ├── 4.5.1.1.2  Login To Date Field
    │   │   └── 4.5.1.1.3  Login Search Action
    │   └── 4.5.1.2  Login Detector User Block
    │       ├── 4.5.1.2.1  Login Detector User List
    │       ├── 4.5.1.2.2  Show Selected Action
    │       ├── 4.5.1.2.3  Show All Action
    │       ├── 4.5.1.2.4  Delete Period Action
    │       └── 4.5.1.2.5  Login Detector Main List Action
    └── 4.5.2  Login Detector Report
        ├── 4.5.2.1  Login Detector Report Title
        ├── 4.5.2.2  Login Detector User Column
        ├── 4.5.2.3  Login Detector Login Date Column
        ├── 4.5.2.4  Login Detector Login Time Column
        ├── 4.5.2.5  Login Detector Logout Date Column
        ├── 4.5.2.6  Login Detector Logout Time Column
        └── 4.5.2.7  Login Detector Device Column
```

### §9.2 Function inventory

The elements of the unit, each defined once, at the place that carries it. The *Type / Level* column gives the kind of the element and the level at which it is recorded under the policy of §3. The *Scope* column marks the elements that belong to one of the five excluded categories of §2; they are documented, and they are not parity requirements.

| ID | Canonical name | Source label | Type / Level | Definition | Evidence | Scope |
|---|---|---|---|---|---|---|
| **4.1** | **Users Module Entry** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 4.1.1 | Users Ribbon Item | Users | Navigation / Subfunction | The ribbon item that opens the users module. | [VERIFIED] | In scope |
| 4.1.2 | Users Menu | — | Menu / UI element | The choices the users module offers. | [VERIFIED] | In scope |
| 4.1.2.1 | Create Users Item | انشاء مستخدمين | Menu item / Subfunction | Opens the screen in which users are created and their permissions are set. | [VERIFIED] | In scope |
| 4.1.2.2 | Change Password Item | تغير كلمة المرور | Menu item / Subfunction | Opens the change of the current user's password. The screen is not shown in the source. | [NOT-SHOWN] | In scope |
| 4.1.2.3 | Login Detector Item | Login detector | Menu item / Subfunction | Opens the screen that records the users' entries into and exits from the system. | [VERIFIED] | In scope |
| 4.1.2.4 | Attendance And Departure Item | الحضور والإنصراف | Menu item / Subfunction | Opens the screen in which the users register their attendance, their rest period and their departure. | [VERIFIED] | In scope |
| 4.1.3 | Users Module Capabilities | — | Block / Container | What the users module allows. | [VERIFIED] | In scope |
| 4.1.3.1 | User Creation Capability | — | Function / Function | Supervisors and system managers are added, a new user is added to the system, and the permissions of that user are set. | [VERIFIED] | In scope |
| 4.1.3.2 | User Maintenance Capability | — | Function / Function | The data and the permissions of the users and the managers are edited, and the password of the current user is changed. | [VERIFIED] | In scope |
| 4.1.3.3 | Attendance Capability | — | Function / Function | The attendance and the departure of the users are registered, and through them the user's overtime and lateness are known. | [VERIFIED] | In scope |
| 4.1.3.4 | User Movement Capability | — | Function / Function | The movement of the users into and out of the system is known and recorded. | [VERIFIED] | In scope |
| 4.1.4 | Second Password Dialog | — | Screen / Container | The dialog in which the program asks for the internal-window password before it opens the users screens. | [VERIFIED] | In scope |
| 4.1.4.1 | Second Password Prompt | Please Enter Your Second Password. | Field / UI element | The prompt in which the program asks for the internal-window password. | [VERIFIED] | In scope |
| 4.1.4.2 | Second Password Field | System menu password | Field / UI element | The field in which the internal-window password is entered. | [VERIFIED] | In scope |
| 4.1.4.3 | Second Password Confirm Action | OK | Action / Function | Confirms the entered internal-window password. | [VERIFIED] | In scope |
| 4.1.4.4 | Second Password Cancel Action | Cancel | Action / Function | Abandons the operation. | [VERIFIED] | In scope |
| 4.1.4.5 | Second Password Rule | — | Rule / Rule | The internal-window password is entered as one hundred and twenty-three, or as the user's own password in the case where the user has changed it. | [VERIFIED] | In scope |
| **4.2** | **User Permissions Screen** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 4.2.1 | User Permissions Screen | — | Screen / Container | Through this window users are created, the permissions of each user are set, their data are edited, and system managers and supervisors are created. | [VERIFIED] | In scope |
| 4.2.1.1 | User List Block | — | Block / Container | The users registered in the program. | [VERIFIED] | In scope |
| 4.2.1.1.1 | User Count Indicator | Users No. | Indicator / UI element | The number of registered users. | [VERIFIED] | In scope |
| 4.2.1.1.2 | User List | — | List / UI element | The registered users; a user is chosen from this list by pressing the name. | [VERIFIED] | In scope |
| 4.2.1.2 | User Data Block | — | Block / Container | The data of one user. | [VERIFIED] | In scope |
| 4.2.1.2.1 | User Name Field | اسم المستخدم | Field / UI element | The name of the user. | [VERIFIED] | In scope |
| 4.2.1.2.2 | User Password Field | كلمة المرور | Field / UI element | The password of the user; it is shown masked. | [VERIFIED] | In scope |
| 4.2.1.2.3 | User Internal Password Field | كلمة مرور النوافذ الداخلية | Field / UI element | The password of the user for the internal windows; it is shown masked. | [VERIFIED] | In scope |
| 4.2.1.2.4 | User Last Login Field | وقت آخر دخول | Field / UI element | The time of the user's last entry into the system. | [VERIFIED] | In scope |
| 4.2.1.3 | Permission Scope Block | — | Block / Container | The scope of the permissions granted to the user. | [VERIFIED] | In scope |
| 4.2.1.3.1 | Absolute Permission Option | صلاحية مطلقة (للمشرفين ومديرون النظام) | Option / Subfunction | The user is a supervisor or a system manager and holds the absolute permission. | [VERIFIED] | In scope |
| 4.2.1.3.2 | Limited Permission Option | صلاحية محدودة (المستخدمين النظام) | Option / Subfunction | The user is a system user and holds the limited permission. | [VERIFIED] | In scope |
| 4.2.1.3.3 | User Branch Selector | رقم الفرع المتاح للمستخدم التعامل معه | Field / UI element | The branch the user may deal with; the option shown is All. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 4.2.1.3.4 | Branch Selector Rule | — | Rule / Rule | Where All is chosen, the patient is registered in the main branch, whose number is one. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 4.2.1.4 | Permission Block | — | Block / Container | The permissions that may be granted to a user. | [VERIFIED] | In scope |
| 4.2.1.4.1 | Patient Data Permission Check Box | اضافة وتعديل بيانات المرضى (طباعة وصل - طباعة بار كود) | Field / UI element | Allows the user to add and edit the patients' data, print the receipt and print the barcode. | [VERIFIED] | In scope |
| 4.2.1.4.2 | Result Entry Permission Check Box | ادخال وتعديل نتائج تحاليل المرضى | Field / UI element | Allows the user to enter and edit the patients' test results. | [VERIFIED] | In scope |
| 4.2.1.4.3 | Result Review Permission Check Box | مراجعة وتعديل نتائج تحاليل المرضى | Field / UI element | Allows the user to review and edit the patients' test results. | [VERIFIED] | In scope |
| 4.2.1.4.4 | Result Print Permission Check Box | طباعة نتائج المرضى (عمل تاريخ مرضى وطباعتة - طباعة ظرف ... الخ) | Field / UI element | Allows the user to print the patients' results, to make and print the patient's history, and to print the envelope. | [VERIFIED] | In scope |
| 4.2.1.4.5 | Balance Withholding Permission Check Box | عدم طباعة نتائج المرضى في حالة وجود باقي حساب | Field / UI element | Prevents the patients' results from being printed while a balance remains on the account. | [VERIFIED] | In scope |
| 4.2.1.4.6 | Result Delivery Permission Check Box | صلاحية تسليم نتائج المرضى | Field / UI element | Allows the user to deliver the patients' results. | [VERIFIED] | In scope |
| 4.2.1.4.7 | Discount Limit Check Box | حد الخصم المسموح للمستخدم اعطاءه للمريض | Field / UI element | Allows the user to give the patient a discount, up to the limit set beside it. | [VERIFIED] | In scope |
| 4.2.1.4.8 | Discount Limit Percent Field | % | Field / UI element | The limit of the discount the user may give the patient, as a percentage. | [VERIFIED] | In scope |
| 4.2.1.4.9 | Work Sheet Print Permission Check Box | طباعة ورقة العمل و Log | Field / UI element | Allows the user to print the work sheet and the log. | [VERIFIED] | In scope |
| 4.2.1.4.10 | Patient Delete Permission Check Box | صلاحية حذف المرضى | Field / UI element | Allows the user to delete patients. | [VERIFIED] | In scope |
| 4.2.1.4.11 | System Data Permission Check Box | تعديل في بيانات النظام والاعدادات مثل ( بيانات التحاليل والقيم الطبيعية - الباركود - الوحدات ... الخ) | Field / UI element | Allows the user to edit the system data and the settings, such as the test data, the normal values, the barcode and the units. | [VERIFIED] | In scope |
| 4.2.1.4.12 | Accounts Permission Check Box | امكانية صرف وايداع نقدية محاسبة المرضى ومندوبين الجهات الخارجية (حسابات) | Field / UI element | Allows the user to deposit and withdraw cash for the patients' accounts and for the representatives of the external entities. | [VERIFIED] | In scope |
| 4.2.1.4.13 | Statistics Permission Check Box | امكانية عمل التقارير الاحصائية وطباعتها (الاحصائيات) | Field / UI element | Allows the user to make and print the statistical reports. | [VERIFIED] | In scope |
| 4.2.1.4.14 | Stocktaking Permission Check Box | الاطلاع على حساب الدرج والحسابات الاخري (جرد يومى - اسبوعى - شهرى - سنوى .... الخ ) | Field / UI element | Allows the user to see the stocktaking account and the other accounts, daily, weekly, monthly and yearly. | [VERIFIED] | In scope |
| 4.2.1.4.15 | Attendance Permission Check Box | تسجيل الحضور والانصراف | Field / UI element | Allows the user to register the attendance and the departure. | [VERIFIED] | In scope |
| 4.2.1.5 | Work Time Block | — | Block / Container | The working hours and the rest period set for the user. | [VERIFIED] | In scope |
| 4.2.1.5.1 | Work Start Time Field | وقت بداية العمل للمستخدم | Field / UI element | The hour at which the user's work begins. | [VERIFIED] | In scope |
| 4.2.1.5.2 | Work End Time Field | وقت انتهاء العمل للمستخدم | Field / UI element | The hour at which the user's work ends. | [VERIFIED] | In scope |
| 4.2.1.5.3 | Rest Period Exists Option | هل توجد فترة راحة للموظف | Field / UI element | Whether the employee has a rest period. | [VERIFIED] | In scope |
| 4.2.1.5.4 | Rest Period Duration Field | زمن فترة الراحة بالساعات والدقائق | Field / UI element | The length of the rest period, in hours and minutes. | [VERIFIED] | In scope |
| 4.2.1.6 | User Action Block | — | Block / Container | The actions of the user permissions screen. | [VERIFIED] | In scope |
| 4.2.1.6.1 | Add User Action | إضافة | Action / Function | Opens the form in which a new user is entered. | [VERIFIED] | In scope |
| 4.2.1.6.2 | Edit User Action | تعديل | Action / Function | Opens the data and the permissions of the selected user for editing. | [VERIFIED] | In scope |
| 4.2.1.6.3 | Save User Action | حفظ | Action / Function | Persists the user; the user then appears in the user list. | [VERIFIED] | In scope |
| 4.2.1.6.4 | Delete User Action | حذف | Action / Function | Deletes the selected user. | [VERIFIED] | In scope |
| 4.2.1.6.5 | User Undo Action | تراجع | Action / Function | Steps back out of the current user operation. | [VERIFIED] | In scope |
| 4.2.1.6.6 | User Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **4.3** | **Permission Enforcement** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 4.3.1 | Permission Alert Dialog | — | Screen / Container | The dialog shown when a user attempts an operation for which no permission was granted. | [VERIFIED] | In scope |
| 4.3.1.1 | Permission Alert Title | تنبيه | Field / UI element | The title of the alert. | [VERIFIED] | In scope |
| 4.3.1.2 | Permission Alert Message | أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام | Field / UI element | The message of the alert, stating that the user does not hold the permission for the work and should consult the system administrator. | [VERIFIED] | In scope |
| 4.3.1.3 | Permission Alert Confirm Action | OK | Action / Function | Closes the alert. | [VERIFIED] | In scope |
| 4.3.2 | First Login Notice | This first login for you | Indicator / UI element | The status bar shows this notice on the user's first entry into the system. | [VERIFIED] | In scope |
| **4.4** | **Attendance and Departure** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 4.4.1 | Attendance Screen | — | Screen / Container | Through this screen the users record their attendance and their departure, and through it the user's overtime and lateness are known. | [VERIFIED] | In scope |
| 4.4.1.1 | Register Attendance Action | تسجيل حضور | Action / Function | Records the user's arrival at the beginning of the work. | [VERIFIED] | In scope |
| 4.4.1.2 | Register Departure Action | تسجيل انصراف | Action / Function | Records the user's departure at the end of the work. | [VERIFIED] | In scope |
| 4.4.1.3 | Start Rest Period Action | بدأ فترة الراحة | Action / Function | Records the beginning of the user's rest period, where one exists. | [VERIFIED] | In scope |
| 4.4.1.4 | End Rest Period Action | انهاء فترة الراحة | Action / Function | Records the end of the user's rest period. | [VERIFIED] | In scope |
| 4.4.1.5 | Attendance Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 4.4.2 | Attendance Working Rule | — | Rule / Rule | The user records the attendance when beginning the work on the program, records the rest period where one exists, and records the departure when the working day ends. The system of attendance and departure, together with the rest period of each user, is used to follow the movement of the users into and out of the system, to follow the working times, and to know the user's overtime and lateness. | [VERIFIED] | In scope |
| **4.5** | **Login Detector** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 4.5.1 | Login Detector Screen | Login Detector | Screen / Container | The screen that records the users' entries into and exits from the system, over a chosen period. | [VERIFIED] | In scope |
| 4.5.1.1 | Login Detector Filter Block | — | Block / Container | The period the records are read over. | [VERIFIED] | In scope |
| 4.5.1.1.1 | Login From Date Field | من | Field / UI element | The date the period starts at. | [VERIFIED] | In scope |
| 4.5.1.1.2 | Login To Date Field | إلى | Field / UI element | The date the period ends at. | [VERIFIED] | In scope |
| 4.5.1.1.3 | Login Search Action | بحث | Action / Function | Reads the records of the chosen period. | [VERIFIED] | In scope |
| 4.5.1.2 | Login Detector User Block | — | Block / Container | The users the records are read for. | [VERIFIED] | In scope |
| 4.5.1.2.1 | Login Detector User List | المستخدمين | List / UI element | The users the records may be read for. | [VERIFIED] | In scope |
| 4.5.1.2.2 | Show Selected Action | عرض المحدد | Action / Function | Shows the records of the selected users alone. | [VERIFIED] | In scope |
| 4.5.1.2.3 | Show All Action | عرض الكل | Action / Function | Shows the records of all the users. | [VERIFIED] | In scope |
| 4.5.1.2.4 | Delete Period Action | حذف الفترة | Action / Function | Removes the records of the chosen period. | [VERIFIED] | In scope |
| 4.5.1.2.5 | Login Detector Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 4.5.2 | Login Detector Report | — | Output / Output | The printed record of the users' entries and exits, carrying the laboratory header, the date and time of printing, the user who printed it, and one row per entry. | [VERIFIED] | In scope |
| 4.5.2.1 | Login Detector Report Title | كشف تسجيل الدخول والخروج للمستخدم ... عن فترة محددة | Field / UI element | The title of the record, naming the user and the period it covers. | [VERIFIED] | In scope |
| 4.5.2.2 | Login Detector User Column | المستخدم | Grid column / UI element | The user the row reports. | [VERIFIED] | In scope |
| 4.5.2.3 | Login Detector Login Date Column | تاريخ الدخول | Grid column / UI element | The date the user entered the system. | [VERIFIED] | In scope |
| 4.5.2.4 | Login Detector Login Time Column | وقت الدخول | Grid column / UI element | The hour the user entered the system. | [VERIFIED] | In scope |
| 4.5.2.5 | Login Detector Logout Date Column | تاريخ الخروج | Grid column / UI element | The date the user left the system. | [VERIFIED] | In scope |
| 4.5.2.6 | Login Detector Logout Time Column | وقت الخروج | Grid column / UI element | The hour the user left the system. | [VERIFIED] | In scope |
| 4.5.2.7 | Login Detector Device Column | اسم الجهاز | Grid column / UI element | The name of the device the entry was made from. | [VERIFIED] | In scope |

### §9.3 Workflows

### Open the users module

1. Press the users ribbon item; the menu of the users module opens. → 4.1.1
2. Choose the operation: creating users, changing the password, the login detector, or the attendance and departure. → 4.1.2
3. The program asks for the internal-window password before it opens the users screens; enter one hundred and twenty-three, or the user's own password where the user has changed it. → 4.1.4.1, 4.1.4.2, 4.1.4.5

### Create a user and set its permissions

1. Choose the create-users item and enter the internal-window password. → 4.1.2.1, 4.1.4.2
2. Press the add action, and write the user's name, its main password and its internal-window password. → 4.2.1.6.1, 4.2.1.2.1, 4.2.1.2.2, 4.2.1.2.3
3. Choose the scope of the permissions: the absolute permission held by the supervisors and the system managers, or the limited permission held by the system users. → 4.2.1.3.1, 4.2.1.3.2
4. Choose the branch the user may deal with; where All is chosen, the patient is registered in the main branch, whose number is one. → 4.2.1.3.3, 4.2.1.3.4
5. Set the permissions the user holds, and the limit of the discount it may give the patient. → 4.2.1.4
6. Set the user's working hours and, where the employee has one, the length of the rest period. → 4.2.1.5
7. Press save; the user then appears in the user list on the left of the window. → 4.2.1.6.3, 4.2.1.1.2

### Edit the data or the permissions of a user

1. Choose the create-users item and enter the internal-window password. → 4.1.2.1, 4.1.4.2
2. Choose the user from the user list on the left of the window, and press edit. → 4.2.1.1.2, 4.2.1.6.2
3. Make the change, then press save. → 4.2.1.6.3

### Meet a permission that was not granted

1. The user signs in; the permissions set for it are the ones it holds. → 4.1.1
2. Where the user attempts an operation it was not granted, the alert is shown, stating that the user does not hold the permission for the work and should consult the system administrator; the alert is closed with the confirm action. → 4.3.1.1, 4.3.1.2, 4.3.1.3

### Record the attendance, the rest period and the departure

1. Choose the attendance and departure item. → 4.1.2.4
2. When beginning the work on the program, record the attendance. → 4.4.1.1
3. Where a rest period exists, record its beginning and its end. → 4.4.1.3, 4.4.1.4
4. When the working day ends, record the departure. → 4.4.1.2
5. The user's overtime and lateness are known from the recorded times and the rest period set for the user. → 4.4.2

### Read the users' entries and exits

1. Choose the login detector. → 4.1.2.3
2. Set the date the period starts at and the date it ends at, and press search. → 4.5.1.1.1, 4.5.1.1.2, 4.5.1.1.3
3. Choose the users, then press the show-selected action to see their records alone, or the show-all action to see the records of all the users. → 4.5.1.2.1, 4.5.1.2.2, 4.5.1.2.3
4. The record carries one row per entry, with the user, the date and the hour of the entry, the date and the hour of the exit, and the name of the device the entry was made from. → 4.5.2
5. To remove the records of the period, press the delete-period action. → 4.5.1.2.4

### §9.4 Rules, validations and conditional behaviour

| ID | Rule | Tag |
|---|---|---|
| 4.1.4.5 | The internal-window password is one hundred and twenty-three by default, and the user's own password where the user has changed it. | [VERIFIED] |
| 4.1.2 | The users screens are not opened until the internal-window password has been entered. | [VERIFIED] |
| 4.2.1.3.3 | Where the branch is set to All, the patient is registered in the main branch, whose number is one. | [VERIFIED] |
| 4.2.1.4.5 | While a balance remains on the account, the patients' results are not printed, where that permission is held. | [VERIFIED] |
| 4.2.1.4.7 | The discount the user gives the patient may not exceed the limit set beside it. | [VERIFIED] |
| 4.2.1.6.3 | The user appears in the user list only after the entry has been saved. | [VERIFIED] |
| 4.3.1.2 | The alert is shown only where the operation attempted is one the user was not granted. | [VERIFIED] |
| 4.4.2 | The overtime and the lateness of the user are worked out from the recorded attendance, the recorded departure and the rest period set for the user. | [VERIFIED] |
| 4.5.1.2.4 | The delete-period action removes the records of the chosen period. | [VERIFIED] |

### §9.5 Outputs

The artefacts below are produced by the elements listed. The content of each artefact is defined once, in the element's own definition, and is not repeated here.

| ID | Output | Tag |
|---|---|---|
| 4.5.2 | Login and logout record of the users | [VERIFIED] |

### §9.6 Screens and interface structure

| Screen | Element identifiers |
|---|---|
| Users Module Entry | 4.1, 4.1.1, 4.1.2, 4.1.2.1, 4.1.2.2, 4.1.2.3, 4.1.2.4, 4.1.3, 4.1.3.1, 4.1.3.2, 4.1.3.3, 4.1.3.4, 4.1.4, 4.1.4.1, 4.1.4.2, 4.1.4.3, 4.1.4.4, 4.1.4.5 |
| Second Password Dialog | 4.1.4, 4.1.4.1, 4.1.4.2, 4.1.4.3, 4.1.4.4, 4.1.4.5 |
| User Permissions Screen | 4.2.1, 4.2.1.1, 4.2.1.1.1, 4.2.1.1.2, 4.2.1.2, 4.2.1.2.1, 4.2.1.2.2, 4.2.1.2.3, 4.2.1.2.4, 4.2.1.3, 4.2.1.3.1, 4.2.1.3.2, 4.2.1.3.3, 4.2.1.3.4, 4.2.1.4, 4.2.1.4.1, 4.2.1.4.2, 4.2.1.4.3, 4.2.1.4.4, 4.2.1.4.5, 4.2.1.4.6, 4.2.1.4.7, 4.2.1.4.8, 4.2.1.4.9, 4.2.1.4.10, 4.2.1.4.11, 4.2.1.4.12, 4.2.1.4.13, 4.2.1.4.14, 4.2.1.4.15, 4.2.1.5, 4.2.1.5.1, 4.2.1.5.2, 4.2.1.5.3, 4.2.1.5.4, 4.2.1.6, 4.2.1.6.1, 4.2.1.6.2, 4.2.1.6.3, 4.2.1.6.4, 4.2.1.6.5, 4.2.1.6.6 |
| Permission Alert Dialog | 4.3.1, 4.3.1.1, 4.3.1.2, 4.3.1.3 |
| Attendance Screen | 4.4.1, 4.4.1.1, 4.4.1.2, 4.4.1.3, 4.4.1.4, 4.4.1.5 |
| Login Detector Screen | 4.5.1, 4.5.1.1, 4.5.1.1.1, 4.5.1.1.2, 4.5.1.1.3, 4.5.1.2, 4.5.1.2.1, 4.5.1.2.2, 4.5.1.2.3, 4.5.1.2.4, 4.5.1.2.5 |

### §9.7 Uncertainties and unshown elements

| ID | Item | Tag |
|---|---|---|
| 4.1.2.2 | The change-password screen is named in the menu but is not shown, so its fields and its behaviour are not established. | [NOT-SHOWN] |
| 4.2.1.1.2 | The list of registered users is shown, but the shape of the list beyond the names is not established. | [UNVERIFIED] |
| 4.5.2 | The record's page count, its magnification control and the laboratory's printed header are shown on the document, but their behaviour is not established. | [UNVERIFIED] |
| — | No validation, rejection message or recovery path is established for the users screens beyond the permission alert. | [NOT-SHOWN] |

## §10 Unit 5 — Statistics

### §10.1 Functional hierarchy

```
5  Statistics
├── 5.1  Statistics Module Entry
│   ├── 5.1.1  Statistics Ribbon Item
│   ├── 5.1.2  Statistics Password Gate
│   ├── 5.1.3  Statistics Menu
│   │   ├── 5.1.3.1  Patient Count Statistics Item
│   │   ├── 5.1.3.2  Test Count Statistics Item
│   │   ├── 5.1.3.3  Branch Statistics Item
│   │   ├── 5.1.3.4  Sent Samples Statistics Item
│   │   ├── 5.1.3.5  Work Evaluation Statistics Item
│   │   ├── 5.1.3.6  Result Monitoring Item
│   │   └── 5.1.3.7  Statistics Examples Item
│   ├── 5.1.4  Statistics Capabilities
│   │   ├── 5.1.4.1  Patient Statistics Capability
│   │   ├── 5.1.4.2  Test Statistics Capability
│   │   ├── 5.1.4.3  Sent Samples Statistics Capability
│   │   └── 5.1.4.4  Production Capacity Capability
│   └── 5.1.5  Numeric Summary Table
│       ├── 5.1.5.1  Numeric Summary Row
│       └── 5.1.5.2  Numeric Summary Total Row
├── 5.2  Patient Statistics Screen
│   └── 5.2.1  Patient Statistics Screen
│       ├── 5.2.1.1  Patient Statistics Period Block
│       │   ├── 5.2.1.1.1  Statistics Year Field
│       │   ├── 5.2.1.1.2  Statistics Month Field
│       │   ├── 5.2.1.1.3  Statistics From Date Field
│       │   ├── 5.2.1.1.4  Statistics To Date Field
│       │   └── 5.2.1.1.5  Statistics Referring Entities Selector
│       ├── 5.2.1.2  Census By Year Block
│       │   ├── 5.2.1.2.1  Year By Months Census Option
│       │   ├── 5.2.1.2.2  Year By Gender Census Option
│       │   ├── 5.2.1.2.3  Year By Account Type Census Option
│       │   ├── 5.2.1.2.4  Year By Referring Entity Census Option
│       │   └── 5.2.1.2.5  Year By Chosen Entity Only Census Option
│       ├── 5.2.1.3  Census By Month Block
│       │   ├── 5.2.1.3.1  Month By Days Census Option
│       │   ├── 5.2.1.3.2  Month By Gender Census Option
│       │   ├── 5.2.1.3.3  Month By Account Type Census Option
│       │   ├── 5.2.1.3.4  Month By Referring Entity Census Option
│       │   └── 5.2.1.3.5  Month By Chosen Entity Only Census Option
│       ├── 5.2.1.4  Census By Period Block
│       │   ├── 5.2.1.4.1  Period By Gender Census Option
│       │   ├── 5.2.1.4.2  Period By Account Type Census Option
│       │   ├── 5.2.1.4.3  Period By Referring Entity Census Option
│       │   └── 5.2.1.4.4  Period By Chosen Entity Only Census Option
│       └── 5.2.1.5  Patient Statistics Action Block
│           └── 5.2.1.5.1  Patient Statistics Main List Action
├── 5.3  Patient Statistics Reports
│   ├── 5.3.1  Patient Report Common Elements
│   │   ├── 5.3.1.1  Report Laboratory Header
│   │   ├── 5.3.1.2  Report Printed In Entry
│   │   ├── 5.3.1.3  Report Printed By Entry
│   │   ├── 5.3.1.4  Report Zoom Setting
│   │   └── 5.3.1.5  Report Page Count Entry
│   ├── 5.3.2  Patient Year By Months Report
│   │   ├── 5.3.2.1  Year By Months Report Title
│   │   ├── 5.3.2.2  Year By Months Report Year Line
│   │   └── 5.3.2.3  Year By Months Report Grid
│   │       ├── 5.3.2.3.1  Year By Months Month Column
│   │       ├── 5.3.2.3.2  Year By Months Patient Count Column
│   │       ├── 5.3.2.3.3  Year By Months Share Column
│   │       ├── 5.3.2.3.4  Year By Months Amount Column
│   │       └── 5.3.2.3.5  Year By Months Total Row
│   ├── 5.3.3  Patient Gender Report
│   │   ├── 5.3.3.1  Gender Report Title
│   │   └── 5.3.3.2  Gender Report Grid
│   │       ├── 5.3.3.2.1  Gender Report Gender Column
│   │       ├── 5.3.3.2.2  Gender Report Patient Count Column
│   │       ├── 5.3.3.2.3  Gender Report Share Column
│   │       ├── 5.3.3.2.4  Gender Report Amount Column
│   │       └── 5.3.3.2.5  Gender Report Total Row
│   ├── 5.3.4  Patient Month By Days Report
│   │   ├── 5.3.4.1  Month By Days Report Title
│   │   ├── 5.3.4.2  Month By Days Report Month Line
│   │   └── 5.3.4.3  Month By Days Report Grid
│   │       ├── 5.3.4.3.1  Month By Days Day Column
│   │       ├── 5.3.4.3.2  Month By Days Patient Count Column
│   │       └── 5.3.4.3.3  Month By Days Amount Column
│   └── 5.3.5  Patient Referring Entity Report
│       ├── 5.3.5.1  Referring Entity Report Title
│       └── 5.3.5.2  Referring Entity Report Grid
│           ├── 5.3.5.2.1  Referring Entity Report Entity Column
│           ├── 5.3.5.2.2  Referring Entity Report Patient Count Column
│           └── 5.3.5.2.3  Referring Entity Report Amount Column
├── 5.4  Test Statistics Screen
│   └── 5.4.1  Test Statistics Screen
│       ├── 5.4.1.1  Test Statistics Period Block
│       │   ├── 5.4.1.1.1  Test Statistics Year Field
│       │   ├── 5.4.1.1.2  Test Statistics From Date Field
│       │   └── 5.4.1.1.3  Test Statistics To Date Field
│       ├── 5.4.1.2  Test Census By Year Block
│       │   ├── 5.4.1.2.1  All Tests By Months Census Option
│       │   ├── 5.4.1.2.2  All Tests By Names Census Option
│       │   ├── 5.4.1.2.3  Test Group By Months Census Option
│       │   ├── 5.4.1.2.4  Test Group By Names Census Option
│       │   ├── 5.4.1.2.5  Specific Test By Months Census Option
│       │   └── 5.4.1.2.6  Referring Entity By Months Census Option
│       └── 5.4.1.3  Test Statistics Action Block
│           └── 5.4.1.3.1  Test Statistics Main List Action
├── 5.5  Test Statistics Reports
│   ├── 5.5.1  All Tests By Names Report
│   │   ├── 5.5.1.1  All Tests By Names Report Title
│   │   ├── 5.5.1.2  All Tests By Names Report Year Line
│   │   └── 5.5.1.3  All Tests By Names Report Grid
│   │       ├── 5.5.1.3.1  All Tests Report Test Name Column
│   │       ├── 5.5.1.3.2  All Tests Report Sample Count Column
│   │       ├── 5.5.1.3.3  All Tests Report Share Column
│   │       ├── 5.5.1.3.4  All Tests Report Amount Column
│   │       └── 5.5.1.3.5  All Tests Report Total Row
│   ├── 5.5.2  Test Group Request Rate Report
│   │   ├── 5.5.2.1  Test Group Request Rate Report Title
│   │   ├── 5.5.2.2  Test Group Request Rate Report Year Line
│   │   └── 5.5.2.3  Test Group Request Rate Report Grid
│   │       ├── 5.5.2.3.1  Group Rate Report Test Name Column
│   │       ├── 5.5.2.3.2  Group Rate Report Sample Count Column
│   │       ├── 5.5.2.3.3  Group Rate Report Share Column
│   │       ├── 5.5.2.3.4  Group Rate Report Amount Column
│   │       └── 5.5.2.3.5  Group Rate Report Total Row
│   └── 5.5.3  Test By Months Report
│       └── 5.5.3.1  Test By Months Report Grid
├── 5.6  Statistics Report Charts and Preview
│   ├── 5.6.1  Statistics Report Chart Block
│   │   ├── 5.6.1.1  Pie Chart Output
│   │   ├── 5.6.1.2  Bar Chart Output
│   │   └── 5.6.1.3  Line Chart Output
│   └── 5.6.2  Statistics Report Preview Window
│       ├── 5.6.2.1  Report Preview Zoom Selector
│       ├── 5.6.2.2  Report Preview Print Action
│       └── 5.6.2.3  Report Preview Save Action
└── 5.7  Result Review and Follow-up Report
    ├── 5.7.1  Result Review Report Screen
    │   ├── 5.7.1.1  Result Review Report Grid
    │   │   ├── 5.7.1.1.1  Result Review Date Column
    │   │   ├── 5.7.1.1.2  Result Review Patient Name Column
    │   │   ├── 5.7.1.1.3  Result Review Patient Data Column
    │   │   ├── 5.7.1.1.4  Result Review Referring Entity Column
    │   │   ├── 5.7.1.1.5  Result Review Test Column
    │   │   ├── 5.7.1.1.6  Result Review Result Column
    │   │   └── 5.7.1.1.7  Result Review Status Column
    │   ├── 5.7.1.2  Result Review Report Title
    │   ├── 5.7.1.3  Result Review Test Selector
    │   ├── 5.7.1.4  Result Review Lower Value Field
    │   └── 5.7.1.5  Result Review Upper Value Field
    └── 5.7.2  Result Review Rule
```

### §10.2 Function inventory

The elements of the unit, each defined once, at the place that carries it. The *Type / Level* column gives the kind of the element and the level at which it is recorded under the policy of §3. The *Scope* column marks the elements that belong to one of the five excluded categories of §2; they are documented, and they are not parity requirements.

| ID | Canonical name | Source label | Type / Level | Definition | Evidence | Scope |
|---|---|---|---|---|---|---|
| **5.1** | **Statistics Module Entry** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 5.1.1 | Statistics Ribbon Item | Statistics | Navigation / Subfunction | The ribbon item that opens the statistics module. | [VERIFIED] | In scope |
| 5.1.2 | Statistics Password Gate | — | Rule / Rule | The statistics screens are not opened until the internal-window password has been entered. The dialog and the password are the ones of the users module, in the users and permissions module. | [VERIFIED] | In scope |
| 5.1.3 | Statistics Menu | — | Menu / UI element | The kinds of statistic the module offers. | [VERIFIED] | In scope |
| 5.1.3.1 | Patient Count Statistics Item | احصائيات وفقا لعدد المرضي | Menu item / Subfunction | Opens the statistics of the patients. | [VERIFIED] | In scope |
| 5.1.3.2 | Test Count Statistics Item | احصائيات وفقا لعدد التحاليل | Menu item / Subfunction | Opens the statistics of the tests. | [VERIFIED] | In scope |
| 5.1.3.3 | Branch Statistics Item | احصائيات خاصة بفروع المعمل | Menu item / Subfunction | Opens the statistics of the laboratory branches. The screen is not shown in the source. | [NOT-SHOWN] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 5.1.3.4 | Sent Samples Statistics Item | احصائيات العينات المرسلة | Menu item / Subfunction | Opens the statistics of the samples sent outside. The screen is not shown in the source. | [NOT-SHOWN] | In scope |
| 5.1.3.5 | Work Evaluation Statistics Item | احصائيات تقييم ومتابعة العمل | Menu item / Subfunction | Opens the statistics that evaluate and follow the work, and through which the production capacity of the users is known by the work assigned to them. The screen is not shown in the source. | [NOT-SHOWN] | In scope |
| 5.1.3.6 | Result Monitoring Item | متابعة ومراقبة النتائج | Menu item / Subfunction | Opens the following up and the monitoring of the results. The screen is not shown in the source. | [NOT-SHOWN] | In scope |
| 5.1.3.7 | Statistics Examples Item | أمثلة متنوعة على الإحصائيات المتاحة | Menu item / Subfunction | Opens the various examples of the available statistics. The screen is not shown in the source. | [NOT-SHOWN] | In scope |
| 5.1.4 | Statistics Capabilities | — | Block / Container | What the statistics module produces. | [VERIFIED] | In scope |
| 5.1.4.1 | Patient Statistics Capability | — | Function / Function | Statistics of the number of patients within a chosen period, and their classification by gender, by referring entity and by account type. | [VERIFIED] | In scope |
| 5.1.4.2 | Test Statistics Capability | — | Function / Function | Statistics of the tests and their number, and their classification. | [VERIFIED] | In scope |
| 5.1.4.3 | Sent Samples Statistics Capability | — | Function / Function | Statistics of the samples sent outside, by time period and by the laboratories they are sent to. | [VERIFIED] | In scope |
| 5.1.4.4 | Production Capacity Capability | — | Function / Function | Statistics that show the production capacity of the users by the work assigned to them. | [VERIFIED] | In scope |
| 5.1.5 | Numeric Summary Table | NUMBERS \ ·  TOTAL | Table / UI element | A demonstration table of numbered rows with a total row, shown in the source as an illustration of the way a statistic is presented. Its values are examples and not a requirement. | [SAMPLE] | In scope |
| 5.1.5.1 | Numeric Summary Row | NUMBERS | Grid column / UI element | The numbered row of the demonstration table. | [SAMPLE] | In scope |
| 5.1.5.2 | Numeric Summary Total Row | TOTAL | Grid column / UI element | The total row of the demonstration table. | [SAMPLE] | In scope |
| **5.2** | **Patient Statistics Screen** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 5.2.1 | Patient Statistics Screen | نافذة الإحصائيات الخاصة بالمرضى | Screen / Container | Through this window the total number of patients who came to the laboratory is known, and they are classified over different time periods and sorted and grouped by gender, by account type and by referring entity. | [VERIFIED] | In scope |
| 5.2.1.1 | Patient Statistics Period Block | — | Block / Container | The period the census is taken over. | [VERIFIED] | In scope |
| 5.2.1.1.1 | Statistics Year Field | في سنة | Field / UI element | The year the census is taken over. | [VERIFIED] | In scope |
| 5.2.1.1.2 | Statistics Month Field | في الشهر | Field / UI element | The month the census is taken over. | [VERIFIED] | In scope |
| 5.2.1.1.3 | Statistics From Date Field | في الفترة من | Field / UI element | The date the period starts at. | [VERIFIED] | In scope |
| 5.2.1.1.4 | Statistics To Date Field | حتي | Field / UI element | The date the period ends at. | [VERIFIED] | In scope |
| 5.2.1.1.5 | Statistics Referring Entities Selector | جهات الإحالة | Field / UI element | The referring entities the census may be restricted to. | [VERIFIED] | In scope |
| 5.2.1.2 | Census By Year Block | الاحصاء حسب السنة المحددة | Block / Container | The censuses taken over the chosen year, and the way each one sorts the patients. | [VERIFIED] | In scope |
| 5.2.1.2.1 | Year By Months Census Option | احصاء اجمالي عدد المرضي (فرز بشهور السنة) | Option / Subfunction | The total number of patients, sorted by the months of the year. | [VERIFIED] | In scope |
| 5.2.1.2.2 | Year By Gender Census Option | احصاء اجمالي عدد المرضي (فرز بجنس المريض) | Option / Subfunction | The total number of patients, sorted by the patient's gender. | [VERIFIED] | In scope |
| 5.2.1.2.3 | Year By Account Type Census Option | احصاء اجمالي عدد المرضي (فرز بنوع الحساب) | Option / Subfunction | The total number of patients, sorted by the type of account. | [VERIFIED] | In scope |
| 5.2.1.2.4 | Year By Referring Entity Census Option | احصاء اجمالي عدد المرضي (فرز بجهة الإحالة) | Option / Subfunction | The total number of patients, sorted by the referring entity. | [VERIFIED] | In scope |
| 5.2.1.2.5 | Year By Chosen Entity Only Census Option | احصاء اجمالي عدد المرضي لجهة الإحالة المحددة فقط | Option / Subfunction | The total number of patients of the chosen referring entity alone. | [VERIFIED] | In scope |
| 5.2.1.3 | Census By Month Block | الاحصاء حسب الشهر المحدد | Block / Container | The censuses taken over the chosen month, and the way each one sorts the patients. | [VERIFIED] | In scope |
| 5.2.1.3.1 | Month By Days Census Option | احصاء اجمالي عدد المرضي (فرز بأيام الشهر) | Option / Subfunction | The total number of patients, sorted by the days of the month. | [VERIFIED] | In scope |
| 5.2.1.3.2 | Month By Gender Census Option | احصاء اجمالي عدد المرضي (فرز بجنس المريض) | Option / Subfunction | The total number of patients, sorted by the patient's gender. | [VERIFIED] | In scope |
| 5.2.1.3.3 | Month By Account Type Census Option | احصاء اجمالي عدد المرضي (فرز بنوع الحساب) | Option / Subfunction | The total number of patients, sorted by the type of account. | [VERIFIED] | In scope |
| 5.2.1.3.4 | Month By Referring Entity Census Option | احصاء اجمالي عدد المرضي (فرز بجهة الإحالة) | Option / Subfunction | The total number of patients, sorted by the referring entity. | [VERIFIED] | In scope |
| 5.2.1.3.5 | Month By Chosen Entity Only Census Option | احصاء اجمالي عدد المرضي لجهة الإحالة المحددة فقط | Option / Subfunction | The total number of patients of the chosen referring entity alone. | [VERIFIED] | In scope |
| 5.2.1.4 | Census By Period Block | الاحصاء حسب الفترة المحددة | Block / Container | The censuses taken over the chosen period, and the way each one sorts the patients. | [VERIFIED] | In scope |
| 5.2.1.4.1 | Period By Gender Census Option | احصاء اجمالي عدد المرضي (فرز بجنس المريض) | Option / Subfunction | The total number of patients, sorted by the patient's gender. | [VERIFIED] | In scope |
| 5.2.1.4.2 | Period By Account Type Census Option | احصاء اجمالي عدد المرضي (فرز بنوع الحساب) | Option / Subfunction | The total number of patients, sorted by the type of account. | [VERIFIED] | In scope |
| 5.2.1.4.3 | Period By Referring Entity Census Option | احصاء اجمالي عدد المرضي (فرز بجهة الإحالة) | Option / Subfunction | The total number of patients, sorted by the referring entity. | [VERIFIED] | In scope |
| 5.2.1.4.4 | Period By Chosen Entity Only Census Option | احصاء اجمالي عدد المرضي لجهة الإحالة المحددة فقط | Option / Subfunction | The total number of patients of the chosen referring entity alone. | [VERIFIED] | In scope |
| 5.2.1.5 | Patient Statistics Action Block | — | Block / Container | The actions of the patient statistics screen. | [VERIFIED] | In scope |
| 5.2.1.5.1 | Patient Statistics Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **5.3** | **Patient Statistics Reports** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 5.3.1 | Patient Report Common Elements | — | Block / Container | The elements every statistics document carries. | [VERIFIED] | In scope |
| 5.3.1.1 | Report Laboratory Header | — | Output / Output | The header the laboratory prints on its documents. Its content is the laboratory's own identity and is not described here. | [VERIFIED] | In scope |
| 5.3.1.2 | Report Printed In Entry | Printed In | Field / UI element | The date and time the report was printed. | [VERIFIED] | In scope |
| 5.3.1.3 | Report Printed By Entry | Printed By | Field / UI element | The user who printed the report. | [VERIFIED] | In scope |
| 5.3.1.4 | Report Zoom Setting | Zoom 100% | Field / UI element | The magnification the report is shown at in the preview. | [VERIFIED] | In scope |
| 5.3.1.5 | Report Page Count Entry | Pages: 1 | Field / UI element | The number of pages the report has. | [VERIFIED] | In scope |
| 5.3.2 | Patient Year By Months Report | — | Output / Output | The report of the total number of patients of a chosen year, sorted by the months of the year, and of the amounts they paid. | [VERIFIED] | In scope |
| 5.3.2.1 | Year By Months Report Title | إحصاء اجمالي عدد المرضي في سنة محددة (فرز بشهور السنة) | Field / UI element | The title of the report. | [VERIFIED] | In scope |
| 5.3.2.2 | Year By Months Report Year Line | وذلك لعام | Field / UI element | The line beneath the title, carrying the year the report covers. | [VERIFIED] | In scope |
| 5.3.2.3 | Year By Months Report Grid | — | Grid / UI element | The grid of the report: one row per month, carrying the number of patients, their share of the total and the amounts they paid, with a total row. | [VERIFIED] | In scope |
| 5.3.2.3.1 | Year By Months Month Column | الشهر | Grid column / UI element | The month the row reports. | [VERIFIED] | In scope |
| 5.3.2.3.2 | Year By Months Patient Count Column | المرضى | Grid column / UI element | The number of patients in the month. | [VERIFIED] | In scope |
| 5.3.2.3.3 | Year By Months Share Column | نسبة المريض | Grid column / UI element | The share of the row in the total, printed as a percentage. The guide prints this column under the heading نسبة المريض; the showcase renders the same column under the shorter heading المريض. | [VERIFIED] | In scope |
| 5.3.2.3.4 | Year By Months Amount Column | المبالغ المسددة | Grid column / UI element | The amount paid by the patients of the month. | [VERIFIED] | In scope |
| 5.3.2.3.5 | Year By Months Total Row | الاجمالي | Grid column / UI element | The total row of the report, carrying the total number of patients and the total amount. | [VERIFIED] | In scope |
| 5.3.3 | Patient Gender Report | — | Output / Output | The report of the total number of patients of a chosen period, sorted by the patient's gender, and of the amounts they paid. | [VERIFIED] | In scope |
| 5.3.3.1 | Gender Report Title | إحصاء اجمالي عدد المرضي في السنة المحددة (فرز بجنس المريض) | Field / UI element | The title of the report. | [VERIFIED] | In scope |
| 5.3.3.2 | Gender Report Grid | — | Grid / UI element | The grid of the report: one row per gender, carrying the number of patients, their share of the total and the amounts they paid, with a total row. | [VERIFIED] | In scope |
| 5.3.3.2.1 | Gender Report Gender Column | جنس المريض | Grid column / UI element | The gender the row reports. | [VERIFIED] | In scope |
| 5.3.3.2.2 | Gender Report Patient Count Column | المرضى | Grid column / UI element | The number of patients of that gender. | [VERIFIED] | In scope |
| 5.3.3.2.3 | Gender Report Share Column | نسبة المريض | Grid column / UI element | The share of the row in the total, printed as a percentage. The guide prints this column under the heading نسبة المريض; the showcase renders the same column under the shorter heading المريض. | [VERIFIED] | In scope |
| 5.3.3.2.4 | Gender Report Amount Column | المبالغ المسددة | Grid column / UI element | The amount paid by the patients of that gender. | [VERIFIED] | In scope |
| 5.3.3.2.5 | Gender Report Total Row | الاجمالي | Grid column / UI element | The total row of the report, carrying the total number of patients and the total amount. | [VERIFIED] | In scope |
| 5.3.4 | Patient Month By Days Report | — | Output / Output | The report of the total number of patients of a chosen month, sorted by the days of the month, and of the amounts they paid. | [VERIFIED] | In scope |
| 5.3.4.1 | Month By Days Report Title | إحصاء اجمالي عدد المرضي في شهر محدد (فرز بأيام الشهر) | Field / UI element | The title of the report. | [VERIFIED] | In scope |
| 5.3.4.2 | Month By Days Report Month Line | وذلك لشهر | Field / UI element | The line beneath the title, carrying the month and the year the report covers. | [VERIFIED] | In scope |
| 5.3.4.3 | Month By Days Report Grid | — | Grid / UI element | The grid of the report: one row per day, carrying the number of patients and the amounts they paid. | [VERIFIED] | In scope |
| 5.3.4.3.1 | Month By Days Day Column | اليوم | Grid column / UI element | The day the row reports. | [VERIFIED] | In scope |
| 5.3.4.3.2 | Month By Days Patient Count Column | عدد المرضى | Grid column / UI element | The number of patients on that day. | [VERIFIED] | In scope |
| 5.3.4.3.3 | Month By Days Amount Column | المبالغ المسددة | Grid column / UI element | The amount paid by the patients of that day. | [VERIFIED] | In scope |
| 5.3.5 | Patient Referring Entity Report | — | Output / Output | The report of the total number of patients of a chosen period, sorted by the referring entity, and of the amounts they paid. | [VERIFIED] | In scope |
| 5.3.5.1 | Referring Entity Report Title | إحصاء اجمالي عدد المرضي في شهر محدد (فرز بجهة الإحالة) | Field / UI element | The title of the report. | [VERIFIED] | In scope |
| 5.3.5.2 | Referring Entity Report Grid | — | Grid / UI element | The grid of the report: one row per referring entity, carrying the number of patients and the amounts they paid, with the names of the patients under each entity. | [VERIFIED] | In scope |
| 5.3.5.2.1 | Referring Entity Report Entity Column | جهة الإحالة | Grid column / UI element | The referring entity the row reports. | [VERIFIED] | In scope |
| 5.3.5.2.2 | Referring Entity Report Patient Count Column | عدد المرضى | Grid column / UI element | The number of patients of that entity. | [VERIFIED] | In scope |
| 5.3.5.2.3 | Referring Entity Report Amount Column | المبالغ المسددة | Grid column / UI element | The amount paid by the patients of that entity. | [VERIFIED] | In scope |
| **5.4** | **Test Statistics Screen** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 5.4.1 | Test Statistics Screen | نافذة الإحصائيات الخاصة بالتحاليل | Screen / Container | Through this window the total number of test samples is known, they are classified over different time periods, and they are sorted and grouped by drawing. | [VERIFIED] | In scope |
| 5.4.1.1 | Test Statistics Period Block | — | Block / Container | The period the census is taken over. | [VERIFIED] | In scope |
| 5.4.1.1.1 | Test Statistics Year Field | في سنة | Field / UI element | The year the census is taken over. | [VERIFIED] | In scope |
| 5.4.1.1.2 | Test Statistics From Date Field | في الفترة من | Field / UI element | The date the period starts at. | [VERIFIED] | In scope |
| 5.4.1.1.3 | Test Statistics To Date Field | حتى | Field / UI element | The date the period ends at. | [VERIFIED] | In scope |
| 5.4.1.2 | Test Census By Year Block | الأحصاء حسب السنة المحددة | Block / Container | The censuses taken over the chosen year, and the way each one sorts the samples. | [VERIFIED] | In scope |
| 5.4.1.2.1 | All Tests By Months Census Option | إحصاء إجمالي عدد العينات لكافة التحاليل (فرز بشهور السنة) | Option / Subfunction | The total number of samples of all the tests, sorted by the months of the year. | [VERIFIED] | In scope |
| 5.4.1.2.2 | All Tests By Names Census Option | إحصاء إجمالي عدد العينات لكافة التحاليل (فرز بأسماء التحاليل) | Option / Subfunction | The total number of samples of all the tests, sorted by the test names. | [VERIFIED] | In scope |
| 5.4.1.2.3 | Test Group By Months Census Option | إحصاء إجمالي عدد عينات مجموعة تحاليل (فرز بشهور السنة) | Option / Subfunction | The total number of samples of a test group, sorted by the months of the year. | [VERIFIED] | In scope |
| 5.4.1.2.4 | Test Group By Names Census Option | إحصاء إجمالي عدد عينات مجموعة تحاليل (فرز بأسماء التحاليل) | Option / Subfunction | The total number of samples of a test group, sorted by the test names. | [VERIFIED] | In scope |
| 5.4.1.2.5 | Specific Test By Months Census Option | إحصاء إجمالي عدد العينات للتحليل المحدد (فرز بشهور السنة) | Option / Subfunction | The total number of samples of the chosen test, sorted by the months of the year. | [VERIFIED] | In scope |
| 5.4.1.2.6 | Referring Entity By Months Census Option | إحصاء إجمالي عدد عينات جهة إحالة (فرز بشهور السنة) | Option / Subfunction | The total number of samples of a referring entity, sorted by the months of the year. | [VERIFIED] | In scope |
| 5.4.1.3 | Test Statistics Action Block | — | Block / Container | The actions of the test statistics screen. | [VERIFIED] | In scope |
| 5.4.1.3.1 | Test Statistics Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| **5.5** | **Test Statistics Reports** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 5.5.1 | All Tests By Names Report | — | Output / Output | The report of the total number of test samples of a chosen year, sorted by the test names, and of the amounts they represent. | [VERIFIED] | In scope |
| 5.5.1.1 | All Tests By Names Report Title | إحصاء اجمالي عدد عينات التحاليل في السنة المحددة (فرز بأسماء التحاليل) | Field / UI element | The title of the report. | [VERIFIED] | In scope |
| 5.5.1.2 | All Tests By Names Report Year Line | وذلك لعام | Field / UI element | The line beneath the title, carrying the year the report covers. | [VERIFIED] | In scope |
| 5.5.1.3 | All Tests By Names Report Grid | نسبة المريض | Grid / UI element | The share of the row in the total, printed as a percentage. The guide prints this column under the heading نسبة المريض; the showcase renders the same column under the shorter heading المريض. | [VERIFIED] | In scope |
| 5.5.1.3.1 | All Tests Report Test Name Column | اسم التحليل | Grid column / UI element | The name of the test the row reports. | [VERIFIED] | In scope |
| 5.5.1.3.2 | All Tests Report Sample Count Column | العينات | Grid column / UI element | The number of samples of that test. | [VERIFIED] | In scope |
| 5.5.1.3.3 | All Tests Report Share Column | — | Grid column / UI element | The share of the test in the total. The column carries the percentage but no readable header. | [UNVERIFIED] | In scope |
| 5.5.1.3.4 | All Tests Report Amount Column | الاجمالي تقريباً | Grid column / UI element | The amount the samples of that test represent. | [VERIFIED] | In scope |
| 5.5.1.3.5 | All Tests Report Total Row | الإجمالي | Grid column / UI element | The total row of the report, carrying the total number of samples and the total amount. | [VERIFIED] | In scope |
| 5.5.2 | Test Group Request Rate Report | — | Output / Output | The report of the rate at which the tests of a group are requested over a chosen year, and of the amounts they represent. | [VERIFIED] | In scope |
| 5.5.2.1 | Test Group Request Rate Report Title | إحصاء معدل طلب تحاليل مجموعة | Field / UI element | The title of the report, carrying the name of the test group. | [VERIFIED] | In scope |
| 5.5.2.2 | Test Group Request Rate Report Year Line | وذلك لعام | Field / UI element | The line beneath the title, carrying the year the report covers. | [VERIFIED] | In scope |
| 5.5.2.3 | Test Group Request Rate Report Grid | نسبة المريض | Grid / UI element | The share of the row in the total, printed as a percentage. The guide prints this column under the heading نسبة المريض; the showcase renders the same column under the shorter heading المريض. | [VERIFIED] | In scope |
| 5.5.2.3.1 | Group Rate Report Test Name Column | اسم التحليل | Grid column / UI element | The name of the test the row reports. | [VERIFIED] | In scope |
| 5.5.2.3.2 | Group Rate Report Sample Count Column | العينات | Grid column / UI element | The number of samples of that test. | [VERIFIED] | In scope |
| 5.5.2.3.3 | Group Rate Report Share Column | — | Grid column / UI element | The share of the test in the total. The column carries the percentage but no readable header. | [UNVERIFIED] | In scope |
| 5.5.2.3.4 | Group Rate Report Amount Column | الاجمالي تقريباً | Grid column / UI element | The amount the samples of that test represent. | [VERIFIED] | In scope |
| 5.5.2.3.5 | Group Rate Report Total Row | الإجمالي | Grid column / UI element | The total row of the report, carrying the total number of samples and the total amount. | [VERIFIED] | In scope |
| 5.5.3 | Test By Months Report | — | Output / Output | The census of the samples by the months of a year. Its table carries the month, the number of patients, the share and the amount paid, with a total row. | [VERIFIED] | In scope |
| 5.5.3.1 | Test By Months Report Grid | — | Grid / UI element | The grid of the report: one row per month. Its columns are not established. | [UNVERIFIED] | In scope |
| **5.6** | **Statistics Report Charts and Preview** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 5.6.1 | Statistics Report Chart Block | — | Block / Container | The chart drawn above the table of a census report. | [VERIFIED] | In scope |
| 5.6.1.1 | Pie Chart Output | — | Output / Output | A pie chart of the census, whose legend carries the names of the categories the census was split by. | [VERIFIED] | In scope |
| 5.6.1.2 | Bar Chart Output | — | Output / Output | A bar chart of the census, whose legend carries the names of the categories the census was split by. | [VERIFIED] | In scope |
| 5.6.1.3 | Line Chart Output | — | Output / Output | A line chart of the census over the days of the month, carrying the average of the month as a line. | [VERIFIED] | In scope |
| 5.6.2 | Statistics Report Preview Window | — | Screen / Container | The window in which the statistics report is shown before it is printed. | [VERIFIED] | In scope |
| 5.6.2.1 | Report Preview Zoom Selector | Zoom 100% | Field / UI element | The magnification the report is shown at. | [VERIFIED] | In scope |
| 5.6.2.2 | Report Preview Print Action | — | Action / Function | Prints the report shown in the preview window. | [VERIFIED] | In scope |
| 5.6.2.3 | Report Preview Save Action | — | Action / Function | Saves the report shown in the preview window. | [VERIFIED] | In scope |
| **5.7** | **Result Review and Follow-up Report** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 5.7.1 | Result Review Report Screen | تقرير متابعة ومراجعة النتائج | Screen / Container | The report of the results of a test over a period. | [VERIFIED] | In scope |
| 5.7.1.1 | Result Review Report Grid | اليوم  ·  اسم المريض  ·  بيانات المريض  ·  جهة الاحالة  ·  التحليل  ·  النتيجة  ·  الحالة | Grid / UI element | The grid of the review report; one row is one result. | [VERIFIED] | In scope |
| 5.7.1.1.1 | Result Review Date Column | اليوم | Grid column / UI element | The day of the request. | [VERIFIED] | In scope |
| 5.7.1.1.2 | Result Review Patient Name Column | اسم المريض | Grid column / UI element | The patient's name. | [VERIFIED] | In scope |
| 5.7.1.1.3 | Result Review Patient Data Column | بيانات المريض | Grid column / UI element | The patient's sex and age. | [VERIFIED] | In scope |
| 5.7.1.1.4 | Result Review Referring Entity Column | جهة الاحالة | Grid column / UI element | The referring entity. | [VERIFIED] | In scope |
| 5.7.1.1.5 | Result Review Test Column | التحليل | Grid column / UI element | The test the result belongs to. | [VERIFIED] | In scope |
| 5.7.1.1.6 | Result Review Result Column | النتيجة | Grid column / UI element | The result itself. | [VERIFIED] | In scope |
| 5.7.1.1.7 | Result Review Status Column | الحالة | Grid column / UI element | The state of the result against the range, shown as High or Low. | [VERIFIED] | In scope |
| 5.7.1.2 | Result Review Report Title | تقرير متابعة ومراجعة النتائج على الفترة من ... الى ... | Field / UI element | The title of the report, carrying the period it covers. | [VERIFIED] | In scope |
| 5.7.1.3 | Result Review Test Selector | — | Field / UI element | The test whose results are reviewed. | [VERIFIED] | In scope |
| 5.7.1.4 | Result Review Lower Value Field | — | Field / UI element | The lowest value the review takes into account. | [VERIFIED] | In scope |
| 5.7.1.5 | Result Review Upper Value Field | — | Field / UI element | The highest value the review takes into account. | [VERIFIED] | In scope |
| 5.7.2 | Result Review Rule | — | Rule / Rule | The results of a test over a period are reviewed by choosing the test and the lowest and the highest value of its result; the report of them is produced, and the biochemistry and the immunology results may be judged from it. The biochemical results of any test may be shown for a period. | [VERIFIED] | In scope |

### §10.3 Workflows

### Open the statistics module

1. Press the statistics ribbon item; the program asks for the internal-window password before the statistics screens open. → 5.1.1, 5.1.2
2. Choose the kind of statistic to produce: the patients, the tests, the branches, the sent samples, the evaluation of the work, the following up of the results, or the examples. → 5.1.3

### Produce a patient census

1. Choose the statistics of the patients. → 5.1.3.1
2. Set the period: the year, the month, or the dates the period starts at and ends at, and choose the referring entities the census may be restricted to. → 5.2.1.1
3. Choose the census: by the chosen year, by the chosen month, or by the chosen period. → 5.2.1.2, 5.2.1.3, 5.2.1.4
4. Choose the way the patients are sorted: by the months of the year or by the days of the month, by the patient's gender, by the type of account, by the referring entity, or for the chosen referring entity alone. → 5.2.1.2, 5.2.1.3, 5.2.1.4
5. The report is produced, carrying the laboratory header, the date and time of printing, the user who printed it, and the rows of the chosen census with a total row. → 5.3.1, 5.3.2, 5.3.3, 5.3.4, 5.3.5

### Produce a test-sample census

1. Choose the statistics of the tests. → 5.1.3.2
2. Set the period: the year, and the dates the period starts at and ends at. → 5.4.1.1
3. Choose the census: the total samples of all the tests, the total samples of a test group, the total samples of the chosen test, or the total samples of a referring entity; sorted either by the months of the year or by the test names. → 5.4.1.2
4. The report is produced, carrying the laboratory header, the date and time of printing, the user who printed it, and the rows with the number of samples, their share of the total and the amount they represent, with a total row. → 5.5.1, 5.5.2

### §10.4 Rules, validations and conditional behaviour

| ID | Rule | Tag |
|---|---|---|
| 5.1.2 | The statistics screens are not opened until the internal-window password has been entered. | [VERIFIED] |
| 5.1.3.3 | The statistics of the branches are named in the menu but their screen is not shown. | [VERIFIED] |
| 5.1.3.4 | The statistics of the sent samples are named in the menu but their screen is not shown. | [VERIFIED] |
| 5.1.3.5 | The statistics that evaluate the work are named in the menu but their screen is not shown. | [VERIFIED] |
| 5.1.3.6 | The following up of the results is named in the menu but its screen is not shown. | [VERIFIED] |
| 5.1.3.7 | The examples of the available statistics are named in the menu but their screen is not shown. | [VERIFIED] |
| 5.2.1.2 | The censuses of the chosen year are five: by the months, by the gender, by the account type, by the referring entity, and for the chosen referring entity alone. | [VERIFIED] |
| 5.2.1.3 | The censuses of the chosen month are five: by the days, by the gender, by the account type, by the referring entity, and for the chosen referring entity alone. | [VERIFIED] |
| 5.2.1.4 | The censuses of the chosen period are four: by the gender, by the account type, by the referring entity, and for the chosen referring entity alone. | [VERIFIED] |
| 5.3.2 | The report of the year sorted by the months carries a total row with the total number of patients and the total amount. | [VERIFIED] |
| 5.4.1.2 | The censuses of the chosen year are six: the samples of all the tests and of a test group and of the chosen test and of a referring entity, each sorted by the months of the year, and the samples of all the tests and of a test group sorted by the test names. | [VERIFIED] |

### §10.5 Outputs

The artefacts below are produced by the elements listed. The content of each artefact is defined once, in the element's own definition, and is not repeated here.

| ID | Output | Tag |
|---|---|---|
| 5.3.2 | Patient census of a year by months | [VERIFIED] |
| 5.3.3 | Patient census by gender | [VERIFIED] |
| 5.3.4 | Patient census of a month by days | [VERIFIED] |
| 5.3.5 | Patient census by referring entity | [VERIFIED] |
| 5.5.1 | Test-sample census by test names | [VERIFIED] |
| 5.5.2 | Request rate of the tests of a group | [VERIFIED] |
| 5.5.3 | Test-sample census by months | [VERIFIED] |

### §10.6 Screens and interface structure

| Screen or output | Element identifiers |
|---|---|
| Statistics Module Entry | 5.1, 5.1.1, 5.1.2, 5.1.3, 5.1.3.1, 5.1.3.2, 5.1.3.3, 5.1.3.4, 5.1.3.5, 5.1.3.6, 5.1.3.7, 5.1.4, 5.1.4.1, 5.1.4.2, 5.1.4.3, 5.1.4.4, 5.1.5, 5.1.5.1, 5.1.5.2 |
| Patient Statistics Screen | 5.2.1, 5.2.1.1, 5.2.1.1.1, 5.2.1.1.2, 5.2.1.1.3, 5.2.1.1.4, 5.2.1.1.5, 5.2.1.2, 5.2.1.2.1, 5.2.1.2.2, 5.2.1.2.3, 5.2.1.2.4, 5.2.1.2.5, 5.2.1.3, 5.2.1.3.1, 5.2.1.3.2, 5.2.1.3.3, 5.2.1.3.4, 5.2.1.3.5, 5.2.1.4, 5.2.1.4.1, 5.2.1.4.2, 5.2.1.4.3, 5.2.1.4.4, 5.2.1.5, 5.2.1.5.1 |
| Patient Statistics Reports | 5.3, 5.3.1, 5.3.1.1, 5.3.1.2, 5.3.1.3, 5.3.1.4, 5.3.1.5, 5.3.2, 5.3.2.1, 5.3.2.2, 5.3.2.3, 5.3.2.3.1, 5.3.2.3.2, 5.3.2.3.3, 5.3.2.3.4, 5.3.2.3.5, 5.3.3, 5.3.3.1, 5.3.3.2, 5.3.3.2.1, 5.3.3.2.2, 5.3.3.2.3, 5.3.3.2.4, 5.3.3.2.5, 5.3.4, 5.3.4.1, 5.3.4.2, 5.3.4.3, 5.3.4.3.1, 5.3.4.3.2, 5.3.4.3.3, 5.3.5, 5.3.5.1, 5.3.5.2, 5.3.5.2.1, 5.3.5.2.2, 5.3.5.2.3 |
| Test Statistics Screen | 5.4.1, 5.4.1.1, 5.4.1.1.1, 5.4.1.1.2, 5.4.1.1.3, 5.4.1.2, 5.4.1.2.1, 5.4.1.2.2, 5.4.1.2.3, 5.4.1.2.4, 5.4.1.2.5, 5.4.1.2.6, 5.4.1.3, 5.4.1.3.1 |
| Test Statistics Reports | 5.5, 5.5.1, 5.5.1.1, 5.5.1.2, 5.5.1.3, 5.5.1.3.1, 5.5.1.3.2, 5.5.1.3.3, 5.5.1.3.4, 5.5.1.3.5, 5.5.2, 5.5.2.1, 5.5.2.2, 5.5.2.3, 5.5.2.3.1, 5.5.2.3.2, 5.5.2.3.3, 5.5.2.3.4, 5.5.2.3.5, 5.5.3, 5.5.3.1 |

### §10.7 Uncertainties and unshown elements

| ID | Item | Tag |
|---|---|---|
| 5.3.2.3.3 | The share column carries a percentage but no readable header, so its exact title is not established. | [UNVERIFIED] |
| 5.3.3.2.3 | The share column carries a percentage but no readable header, so its exact title is not established. | [UNVERIFIED] |
| 5.5.1.3 | The share column carries a percentage but no readable header, so its exact title is not established. | [UNVERIFIED] |
| 5.5.2.3 | The share column carries a percentage but no readable header, so its exact title is not established. | [UNVERIFIED] |
| 5.5.3 | The layout of the census by months is named in the source but not shown, so its columns are not established. | [UNVERIFIED] |
| 5.1.5 | The demonstration table of numbered rows is an illustration in the source; its values are examples and are not a requirement. | [SAMPLE] |
| — | No validation, rejection message or recovery path is established for the statistics screens. | [NOT-SHOWN] |

## §11 Unit 6 — Inventory and Lab Accounting

### §11.1 Functional hierarchy

```
6  Inventory and Lab Accounting
├── 6.1  Accounts Module Entry
│   ├── 6.1.1  Accounts Ribbon Item
│   ├── 6.1.2  Accounts Menu
│   │   ├── 6.1.2.1  Inventory And Stocktaking Item
│   │   ├── 6.1.2.2  Sent Samples Outside Item
│   │   ├── 6.1.2.3  Cash Deposit And Withdrawal Item
│   │   └── 6.1.2.4  Companies And Representatives Item
│   └── 6.1.3  Accounts Capabilities
│       ├── 6.1.3.1  Stocktaking Capability
│       ├── 6.1.3.2  Counterparty Accounts Capability
│       ├── 6.1.3.3  Sent Samples Capability
│       └── 6.1.3.4  Cash Movement Capability
├── 6.2  Inventory and Stocktaking Account Screen
│   ├── 6.2.1  Inventory Account Screen
│   │   ├── 6.2.1.1  Inventory Filter Block
│   │   │   ├── 6.2.1.1.1  Inventory Branch Selector
│   │   │   ├── 6.2.1.1.2  Inventory From Date Field
│   │   │   ├── 6.2.1.1.3  Inventory To Date Field
│   │   │   └── 6.2.1.1.4  Inventory Display Action
│   │   ├── 6.2.1.2  Inventory Breakdown Block
│   │   │   ├── 6.2.1.2.1  Users Breakdown Group
│   │   │   │   ├── 6.2.1.2.1.1  Users Detailed Option
│   │   │   │   └── 6.2.1.2.1.2  Users Summary Option
│   │   │   ├── 6.2.1.2.2  Sent Samples Breakdown Group
│   │   │   │   ├── 6.2.1.2.2.1  Sent Samples Detailed By Result Option
│   │   │   │   ├── 6.2.1.2.2.2  Sent Samples Detailed By Price Option
│   │   │   │   └── 6.2.1.2.2.3  Sent Samples Summary Option
│   │   │   ├── 6.2.1.2.3  Treating Doctor Breakdown Group
│   │   │   │   ├── 6.2.1.2.3.1  Doctor Detailed By Result Option
│   │   │   │   ├── 6.2.1.2.3.2  Doctor Detailed By Price Option
│   │   │   │   ├── 6.2.1.2.3.3  Doctor Detailed Option
│   │   │   │   └── 6.2.1.2.3.4  Doctor Summary Option
│   │   │   ├── 6.2.1.2.4  Referring Entity Breakdown Group
│   │   │   │   ├── 6.2.1.2.4.1  Entity Detailed By Result Option
│   │   │   │   ├── 6.2.1.2.4.2  Entity Detailed By Price Option
│   │   │   │   ├── 6.2.1.2.4.3  Entity Detailed Option
│   │   │   │   └── 6.2.1.2.4.4  Entity Summary Option
│   │   │   └── 6.2.1.2.5  Account Type Breakdown Group
│   │   │       ├── 6.2.1.2.5.1  Account Type Individual Option
│   │   │       ├── 6.2.1.2.5.2  Account Type Detailed By Result Option
│   │   │       ├── 6.2.1.2.5.3  Account Type Detailed By Price Option
│   │   │       ├── 6.2.1.2.5.4  Account Type Detailed Option
│   │   │       └── 6.2.1.2.5.5  Account Type Summary Option
│   │   ├── 6.2.1.3  Account Figures Block
│   │   │   ├── 6.2.1.3.1  Account Figure Operator Signs
│   │   │   ├── 6.2.1.3.2  Total Patient Samples Figure
│   │   │   ├── 6.2.1.3.3  Discount Value Figure
│   │   │   ├── 6.2.1.3.4  Total After Discount Figure
│   │   │   ├── 6.2.1.3.5  Amount Collected Figure
│   │   │   ├── 6.2.1.3.6  Not Collected Figure
│   │   │   ├── 6.2.1.3.7  Sent Outside Samples Figure
│   │   │   ├── 6.2.1.3.8  Amount Paid Figure
│   │   │   ├── 6.2.1.3.9  Unsettled Balance Figure
│   │   │   ├── 6.2.1.3.10  Cash Deposits Figure
│   │   │   ├── 6.2.1.3.11  Cash Expenses Figure
│   │   │   ├── 6.2.1.3.12  Available In Till Figure
│   │   │   ├── 6.2.1.3.13  Remainder For Patients Figure
│   │   │   ├── 6.2.1.3.14  Commissions And Percentages Figure
│   │   │   └── 6.2.1.3.15  Net Profit Figure
│   │   └── 6.2.1.4  Inventory Action Block
│   │       └── 6.2.1.4.1  Inventory Main List Action
│   ├── 6.2.2  Inventory Branch Rule
│   ├── 6.2.3  Inventory Breakdown Rule
│   ├── 6.2.4  Patient Account Detail Report
│   │   ├── 6.2.4.1  Patient Account Detail Report Title
│   │   ├── 6.2.4.2  Patient Account Detail Grid
│   │   │   ├── 6.2.4.2.1  Patient Account Request Date Column
│   │   │   ├── 6.2.4.2.2  Patient Account Patient Code Column
│   │   │   ├── 6.2.4.2.3  Patient Account Patient Name Column
│   │   │   ├── 6.2.4.2.4  Patient Account Total Column
│   │   │   ├── 6.2.4.2.5  Patient Account Discount Column
│   │   │   ├── 6.2.4.2.6  Patient Account After Discount Column
│   │   │   ├── 6.2.4.2.7  Patient Account Paid Column
│   │   │   └── 6.2.4.2.8  Patient Account Balance Column
│   │   └── 6.2.4.3  Patient Account Detail Report Zoom
│   └── 6.2.5  Doctor Account Detail Report
│       ├── 6.2.5.1  Doctor Account Detail Report Title
│       └── 6.2.5.2  Doctor Account Detail Grid
├── 6.3  Sent Samples Outside Account
│   └── 6.3.1  Sent Samples Account Screen
│       ├── 6.3.1.1  Sent Samples Filter Block
│       │   ├── 6.3.1.1.1  Sent Samples From Date Field
│       │   ├── 6.3.1.1.2  Sent Samples To Date Field
│       │   ├── 6.3.1.1.3  Sent Samples Status Option
│       │   └── 6.3.1.1.4  Sent Samples Display Action
│       ├── 6.3.1.2  Sent Samples Summary Block
│       │   ├── 6.3.1.2.1  Sent Samples Count Figure
│       │   ├── 6.3.1.2.2  Sent Samples Total Figure
│       │   ├── 6.3.1.2.3  Sent Samples Paid Figure
│       │   └── 6.3.1.2.4  Sent Samples Remaining Figure
│       ├── 6.3.1.3  Sent Samples Grid
│       │   ├── 6.3.1.3.1  Sent Samples Request Date Column
│       │   ├── 6.3.1.3.2  Sent Samples Test Name Column
│       │   ├── 6.3.1.3.3  Sent Samples Patient Name Column
│       │   ├── 6.3.1.3.4  Sent Samples Lab Name Column
│       │   ├── 6.3.1.3.5  Sent Samples Cost Column
│       │   ├── 6.3.1.3.6  Sent Samples Paid Column
│       │   └── 6.3.1.3.7  Sent Samples Send Flag
│       ├── 6.3.1.4  Sent Sample Entry Block
│       │   ├── 6.3.1.4.1  Sent Sample Lab Or Entity Field
│       │   ├── 6.3.1.4.2  Sent Sample Send To Field
│       │   ├── 6.3.1.4.3  Sent Sample Price Field
│       │   ├── 6.3.1.4.4  Sent Sample Paid Field
│       │   └── 6.3.1.4.5  Sent Sample Sent Flag
│       ├── 6.3.1.5  Sent Samples Report Type Options
│       │   ├── 6.3.1.5.1  Chosen Entity Count Option
│       │   ├── 6.3.1.5.2  Show Not Sent Option
│       │   ├── 6.3.1.5.3  Open Samples Account Option
│       │   ├── 6.3.1.5.4  Test Sent Outside Option
│       │   └── 6.3.1.5.5  Test Not Sent Option
│       ├── 6.3.1.6  Sent Samples Action Block
│       │   ├── 6.3.1.6.1  Send To Action
│       │   ├── 6.3.1.6.2  Sent Sample Confirm Action
│       │   ├── 6.3.1.6.3  Sent Sample Net Action
│       │   ├── 6.3.1.6.4  Sent Sample Ignore Action
│       │   ├── 6.3.1.6.5  Always To Action
│       │   └── 6.3.1.6.6  Sent Samples Main List Action
│       └── 6.3.1.7  Sent Sample Selection Rule
├── 6.4  Cash and Counterparty Accounts
│   ├── 6.4.1  Cash Deposit And Withdrawal Screen
│   └── 6.4.2  Companies And Representatives Account Screen
└── 6.5  Account Statement Variants and Claims
    ├── 6.5.1  Account Statement Scope Options
    │   ├── 6.5.1.1  Statement Of All Patients Option
    │   ├── 6.5.1.2  Statement Of Balances Only Option
    │   └── 6.5.1.3  Statement Of Settled Only Option
    ├── 6.5.2  Statement Period Rule
    ├── 6.5.3  Detailed Account Statement
    ├── 6.5.4  Detailed Account Statement By Results
    ├── 6.5.5  Claim Report
    ├── 6.5.6  Four Report Variants Rule
    ├── 6.5.7  Sent Samples Account Statement
    ├── 6.5.8  Sent Samples Statement Grid
    ├── 6.5.9  Stocktaking Figure Label Variant
    └── 6.5.10  Accounts Window Items Rule
```

### §11.2 Function inventory

The elements of the unit, each defined once, at the place that carries it. The *Type / Level* column gives the kind of the element and the level at which it is recorded under the policy of §3. The *Scope* column marks the elements that belong to one of the five excluded categories of §2; they are documented, and they are not parity requirements.

| ID | Canonical name | Source label | Type / Level | Definition | Evidence | Scope |
|---|---|---|---|---|---|---|
| **6.1** | **Accounts Module Entry** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 6.1.1 | Accounts Ribbon Item | Accounts | Navigation / Subfunction | The ribbon item that opens the accounts module. | [VERIFIED] | In scope |
| 6.1.2 | Accounts Menu | — | Menu / UI element | The four screens the accounts module offers. | [VERIFIED] | In scope |
| 6.1.2.1 | Inventory And Stocktaking Item | الجرد وحساب الدرج | Menu item / Subfunction | Opens the screen that shows the stocktaking account and the net profit. | [VERIFIED] | In scope |
| 6.1.2.2 | Sent Samples Outside Item | العينات المرسلة للخارج | Menu item / Subfunction | Opens the screen of the accounts of the samples sent outside. | [VERIFIED] | In scope |
| 6.1.2.3 | Cash Deposit And Withdrawal Item | صرف وإيداع نقدية | Menu item / Subfunction | Opens the screen of cash deposits and cash withdrawals. The screen is not shown in the source. | [NOT-SHOWN] | In scope |
| 6.1.2.4 | Companies And Representatives Item | حساب شركات ومندوبين | Menu item / Subfunction | Opens the screen of the accounts of the companies and the representatives. The screen is not shown in the source. | [NOT-SHOWN] | In scope |
| 6.1.3 | Accounts Capabilities | — | Block / Container | What the accounts module allows. | [VERIFIED] | In scope |
| 6.1.3.1 | Stocktaking Capability | — | Function / Function | The stocktaking account is known, and a daily, weekly, monthly or yearly stocktaking is made, or one over different periods. | [VERIFIED] | In scope |
| 6.1.3.2 | Counterparty Accounts Capability | — | Function / Function | The accounts of the other laboratories and the shares of the referring entities over specific periods are known. | [VERIFIED] | In scope |
| 6.1.3.3 | Sent Samples Capability | — | Function / Function | The accounts of the samples sent outside are known and are followed up. | [VERIFIED] | In scope |
| 6.1.3.4 | Cash Movement Capability | — | Function / Function | Cash is deposited and withdrawn for the entities designated for it, or for other entities. | [VERIFIED] | In scope |
| **6.2** | **Inventory and Stocktaking Account Screen** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 6.2.1 | Inventory Account Screen | نافذة حساب الدرج والجرد اليومي والشهري | Screen / Container | Through this window the stocktaking account and the net profit for the day or for a chosen period are known, and the accounts of the other laboratories and the shares of the referring entities are known. | [VERIFIED] | In scope |
| 6.2.1.1 | Inventory Filter Block | — | Block / Container | The scope the account is computed over. | [VERIFIED] | In scope |
| 6.2.1.1.1 | Inventory Branch Selector | Branch No. | Field / UI element | The branch the account is computed for; the option shown is All, and the branch numbers are listed when the branch system is in use. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 6.2.1.1.2 | Inventory From Date Field | من | Field / UI element | The date the period starts at. | [VERIFIED] | In scope |
| 6.2.1.1.3 | Inventory To Date Field | الى | Field / UI element | The date the period ends at. | [VERIFIED] | In scope |
| 6.2.1.1.4 | Inventory Display Action | عرض | Action / Function | Computes and displays the account for the chosen branch and period. | [VERIFIED] | In scope |
| 6.2.1.2 | Inventory Breakdown Block | — | Block / Container | The elements the account may be broken down by, and the type of report each one produces. | [VERIFIED] | In scope |
| 6.2.1.2.1 | Users Breakdown Group | المستخدمين | Field group / Container | The account may be broken down by the users who made the transactions. | [VERIFIED] | In scope |
| 6.2.1.2.1.1 | Users Detailed Option | مفصل | Option / Subfunction | The report of the users is produced in detail. | [VERIFIED] | In scope |
| 6.2.1.2.1.2 | Users Summary Option | مجمع | Option / Subfunction | The report of the users is produced in summary. | [VERIFIED] | In scope |
| 6.2.1.2.2 | Sent Samples Breakdown Group | عينات مرسلة | Field group / Container | The account may be broken down by the samples sent outside. | [VERIFIED] | In scope |
| 6.2.1.2.2.1 | Sent Samples Detailed By Result Option | مفصل | Option / Subfunction | One of the two options of this group that print the same wording; the report of the sent samples is produced in detail by result. | [UNVERIFIED] | In scope |
| 6.2.1.2.2.2 | Sent Samples Detailed By Price Option | مفصل | Option / Subfunction | One of the two options of this group that print the same wording; the report of the sent samples is produced in detail by price. | [UNVERIFIED] | In scope |
| 6.2.1.2.2.3 | Sent Samples Summary Option | مجمع | Option / Subfunction | The report of the sent samples is produced in summary. | [VERIFIED] | In scope |
| 6.2.1.2.3 | Treating Doctor Breakdown Group | الطبيب المعالج | Field group / Container | The account may be broken down by the treating doctor. | [VERIFIED] | In scope |
| 6.2.1.2.3.1 | Doctor Detailed By Result Option | مفصل بالنتائج | Option / Subfunction | The report of the treating doctors is produced in detail by result. | [VERIFIED] | In scope |
| 6.2.1.2.3.2 | Doctor Detailed By Price Option | مفصل بالاسعار | Option / Subfunction | The report of the treating doctors is produced in detail by price. | [VERIFIED] | In scope |
| 6.2.1.2.3.3 | Doctor Detailed Option | مفصل | Option / Subfunction | The report of the treating doctors is produced in detail. | [VERIFIED] | In scope |
| 6.2.1.2.3.4 | Doctor Summary Option | مجمع | Option / Subfunction | The report of the treating doctors is produced in summary. | [VERIFIED] | In scope |
| 6.2.1.2.4 | Referring Entity Breakdown Group | جهة الاحالة | Field group / Container | The account may be broken down by the referring entity. | [VERIFIED] | In scope |
| 6.2.1.2.4.1 | Entity Detailed By Result Option | مفصل بالنتائج | Option / Subfunction | The report of the referring entities is produced in detail by result. | [VERIFIED] | In scope |
| 6.2.1.2.4.2 | Entity Detailed By Price Option | مفصل بالاسعار | Option / Subfunction | The report of the referring entities is produced in detail by price. | [VERIFIED] | In scope |
| 6.2.1.2.4.3 | Entity Detailed Option | مفصل | Option / Subfunction | The report of the referring entities is produced in detail. | [VERIFIED] | In scope |
| 6.2.1.2.4.4 | Entity Summary Option | مجمع | Option / Subfunction | The report of the referring entities is produced in summary. | [VERIFIED] | In scope |
| 6.2.1.2.5 | Account Type Breakdown Group | نوع الحساب | Field group / Container | The account may be broken down by the type of account. | [VERIFIED] | In scope |
| 6.2.1.2.5.1 | Account Type Individual Option | Individual | Option / Subfunction | The account is broken down by the individual account type. | [VERIFIED] | In scope |
| 6.2.1.2.5.2 | Account Type Detailed By Result Option | مفصل بالنتائج | Option / Subfunction | The report of the account types is produced in detail by result. | [VERIFIED] | In scope |
| 6.2.1.2.5.3 | Account Type Detailed By Price Option | مفصل بالاسعار | Option / Subfunction | The report of the account types is produced in detail by price. | [VERIFIED] | In scope |
| 6.2.1.2.5.4 | Account Type Detailed Option | مفصل | Option / Subfunction | The report of the account types is produced in detail. | [VERIFIED] | In scope |
| 6.2.1.2.5.5 | Account Type Summary Option | مجمع | Option / Subfunction | The report of the account types is produced in summary. | [VERIFIED] | In scope |
| 6.2.1.3 | Account Figures Block | — | Block / Container | The figures the account is made of, and the sign shown beside each of them. | [VERIFIED] | In scope |
| 6.2.1.3.1 | Account Figure Operator Signs | = \ ·  + \ ·  - \ ·  * | Indicator / UI element | Each figure carries a sign beside it. The equal sign marks a figure that is taken as it stands, the plus sign a figure added to the one above it, the minus sign a figure deducted from the one above it, and the asterisk a figure the program computes. | [VERIFIED] | In scope |
| 6.2.1.3.2 | Total Patient Samples Figure | اجمالي عينات المرضي | Field / UI element | The total value of the patients' samples; the sign beside it is the equal sign. | [VERIFIED] | In scope |
| 6.2.1.3.3 | Discount Value Figure | قيمة الخصومات | Field / UI element | The value of the discounts; the sign beside it is the plus sign. | [VERIFIED] | In scope |
| 6.2.1.3.4 | Total After Discount Figure | الاجمالي بعد الخصم | Field / UI element | The total after the discount is added. | [VERIFIED] | In scope |
| 6.2.1.3.5 | Amount Collected Figure | ماتم تحصيله | Field / UI element | The amount that has been collected; the sign beside it is the minus sign. | [VERIFIED] | In scope |
| 6.2.1.3.6 | Not Collected Figure | لم يحصل | Field / UI element | The amount that has not been collected. | [VERIFIED] | In scope |
| 6.2.1.3.7 | Sent Outside Samples Figure | العينات المرسلة للخارج | Field / UI element | The value of the samples sent outside; the sign beside it is the equal sign. | [VERIFIED] | In scope |
| 6.2.1.3.8 | Amount Paid Figure | ماتم سداده | Field / UI element | The amount that has been paid; the sign beside it is the plus sign. | [VERIFIED] | In scope |
| 6.2.1.3.9 | Unsettled Balance Figure | باقي لم يسدد | Field / UI element | The amount that has not been settled. | [VERIFIED] | In scope |
| 6.2.1.3.10 | Cash Deposits Figure | توريدات نقدية | Field / UI element | The cash deposited. | [VERIFIED] | In scope |
| 6.2.1.3.11 | Cash Expenses Figure | مصروفات نقدية | Field / UI element | The cash paid out. | [VERIFIED] | In scope |
| 6.2.1.3.12 | Available In Till Figure | المتاح في الخزينة | Field / UI element | What is available in the till; the sign beside it is the asterisk. | [VERIFIED] | In scope |
| 6.2.1.3.13 | Remainder For Patients Figure | باقي للمرضى | Field / UI element | The amount that remains for the patients. | [VERIFIED] | In scope |
| 6.2.1.3.14 | Commissions And Percentages Figure | عمولات ونسب | Field / UI element | The commissions and the percentages. | [VERIFIED] | In scope |
| 6.2.1.3.15 | Net Profit Figure | صافي الربح بعد التحصيل والتسديد | Field / UI element | The net profit after the collection and the settlement; the sign beside it is the asterisk. | [VERIFIED] | In scope |
| 6.2.1.4 | Inventory Action Block | — | Block / Container | The actions of the inventory account screen. | [VERIFIED] | In scope |
| 6.2.1.4.1 | Inventory Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 6.2.2 | Inventory Branch Rule | — | Rule / Rule | Where one device, or several devices joined to one database, serves all the users on the same branch, the branch is left at All and the period is set. Where several devices are joined to one database and each user carries a branch number, in the branch system of the laboratory, the branch number to be shown is chosen instead. | [VERIFIED] | OUT OF SCOPE FOR TOP-LAB PARITY — multi-branch support |
| 6.2.3 | Inventory Breakdown Rule | — | Rule / Rule | To make a stocktaking for a given user, a given referring entity, a given treating doctor, or for the samples sent outside, the element is chosen on the right of the window and the type of report is chosen with it. | [VERIFIED] | In scope |
| 6.2.4 | Patient Account Detail Report | كشف حساب المرضي | Output / Output | The report of the patients' accounts over the chosen period, opened by pressing the total of the patients' samples. | [VERIFIED] | In scope |
| 6.2.4.1 | Patient Account Detail Report Title | كشف حساب اجمالي عينات المرضى فى الفتره من ... الى ... | Field / UI element | The title of the report, carrying the period it covers. | [VERIFIED] | In scope |
| 6.2.4.2 | Patient Account Detail Grid | — | Grid / UI element | The grid of the report: one row per request, carrying the request date, the patient code, the patient name, the total, the discount, the total after the discount, the amount paid and the balance of the account. | [VERIFIED] | In scope |
| 6.2.4.2.1 | Patient Account Request Date Column | تاريخ الطلب | Grid column / UI element | The date of the request. | [VERIFIED] | In scope |
| 6.2.4.2.2 | Patient Account Patient Code Column | كود المريض | Grid column / UI element | The patient's code. | [VERIFIED] | In scope |
| 6.2.4.2.3 | Patient Account Patient Name Column | اسم المريض | Grid column / UI element | The patient's name. | [VERIFIED] | In scope |
| 6.2.4.2.4 | Patient Account Total Column | الاجمالى | Grid column / UI element | The total of the request. | [VERIFIED] | In scope |
| 6.2.4.2.5 | Patient Account Discount Column | قيمة الخصم | Grid column / UI element | The discount of the request. | [VERIFIED] | In scope |
| 6.2.4.2.6 | Patient Account After Discount Column | بعد الخصم | Grid column / UI element | The total of the request after the discount. | [VERIFIED] | In scope |
| 6.2.4.2.7 | Patient Account Paid Column | المدفوع | Grid column / UI element | The amount paid on the request. | [VERIFIED] | In scope |
| 6.2.4.2.8 | Patient Account Balance Column | باقى الحساب | Grid column / UI element | The balance of the account on the request. | [VERIFIED] | In scope |
| 6.2.4.3 | Patient Account Detail Report Zoom | Zoom 100% | Field / UI element | The magnification the report is shown at in the preview. | [VERIFIED] | In scope |
| 6.2.5 | Doctor Account Detail Report | — | Output / Output | The report of the accounts of the patients of one chosen treating doctor over the chosen period. | [VERIFIED] | In scope |
| 6.2.5.1 | Doctor Account Detail Report Title | كشف حساب إجمالي المرضي المحولون من ... في الفترة من ... الى ... | Field / UI element | The title of the report, carrying the name of the treating doctor and the period it covers. | [VERIFIED] | In scope |
| 6.2.5.2 | Doctor Account Detail Grid | — | Grid / UI element | The grid of the report; it carries the same columns as the grid of the patients' account report, 6.2.4.2. | [VERIFIED] | In scope |
| **6.3** | **Sent Samples Outside Account** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 6.3.1 | Sent Samples Account Screen | نافذة حسابات العينات المرسلة للخارج | Screen / Container | Through this window the samples that must be sent outside and their accounts are known, and the accounts of the laboratories dealt with are known. | [VERIFIED] | In scope |
| 6.3.1.1 | Sent Samples Filter Block | — | Block / Container | The scope the samples are listed over. | [VERIFIED] | In scope |
| 6.3.1.1.1 | Sent Samples From Date Field | من | Field / UI element | The date the request period starts at. | [VERIFIED] | In scope |
| 6.3.1.1.2 | Sent Samples To Date Field | الى | Field / UI element | The date the request period ends at. | [VERIFIED] | In scope |
| 6.3.1.1.3 | Sent Samples Status Option | الغير مرسل \ ·  الكل | Field / UI element | Whether only the samples that have not been sent are listed, or all of them. | [VERIFIED] | In scope |
| 6.3.1.1.4 | Sent Samples Display Action | عرض | Action / Function | Lists the samples of the chosen period. | [VERIFIED] | In scope |
| 6.3.1.2 | Sent Samples Summary Block | — | Block / Container | The figures of the account of the chosen laboratory. | [VERIFIED] | In scope |
| 6.3.1.2.1 | Sent Samples Count Figure | عدد العينات | Field / UI element | The number of samples sent to the laboratory in the chosen period. | [VERIFIED] | In scope |
| 6.3.1.2.2 | Sent Samples Total Figure | الإجمالي | Field / UI element | The total of the account. | [VERIFIED] | In scope |
| 6.3.1.2.3 | Sent Samples Paid Figure | المدفوع | Field / UI element | The amount paid on the account. | [VERIFIED] | In scope |
| 6.3.1.2.4 | Sent Samples Remaining Figure | الباقى | Field / UI element | The amount remaining on the account. | [VERIFIED] | In scope |
| 6.3.1.3 | Sent Samples Grid | Request Date \ ·  Test Name \ ·  Patient Name \ ·  Lab. Name \ ·  Cost \ ·  Paid \ ·  Send | Grid / UI element | The samples listed for the chosen period. | [VERIFIED] | In scope |
| 6.3.1.3.1 | Sent Samples Request Date Column | Request Date | Grid column / UI element | The date the request was made. | [VERIFIED] | In scope |
| 6.3.1.3.2 | Sent Samples Test Name Column | Test Name | Grid column / UI element | The name of the test. | [VERIFIED] | In scope |
| 6.3.1.3.3 | Sent Samples Patient Name Column | Patient Name | Grid column / UI element | The name of the patient. | [VERIFIED] | In scope |
| 6.3.1.3.4 | Sent Samples Lab Name Column | Lab. Name | Grid column / UI element | The laboratory the sample is sent to. | [VERIFIED] | In scope |
| 6.3.1.3.5 | Sent Samples Cost Column | Cost | Grid column / UI element | The cost of the sample. | [VERIFIED] | In scope |
| 6.3.1.3.6 | Sent Samples Paid Column | Paid | Grid column / UI element | The amount paid for the sample. | [VERIFIED] | In scope |
| 6.3.1.3.7 | Sent Samples Send Flag | Send | Grid column / UI element | Marks the sample as one that is sent. | [VERIFIED] | In scope |
| 6.3.1.4 | Sent Sample Entry Block | — | Block / Container | The data of the sample that is sent. | [VERIFIED] | In scope |
| 6.3.1.4.1 | Sent Sample Lab Or Entity Field | اسم المعمل أو الجهة: | Field / UI element | The name of the laboratory or the entity the sample is sent to. | [VERIFIED] | In scope |
| 6.3.1.4.2 | Sent Sample Send To Field | ترسل إلى | Field / UI element | The destination the sample is sent to. | [VERIFIED] | In scope |
| 6.3.1.4.3 | Sent Sample Price Field | سعر التحليل: | Field / UI element | The price of the test as sent. | [VERIFIED] | In scope |
| 6.3.1.4.4 | Sent Sample Paid Field | المدفوع: | Field / UI element | The amount paid for the sent test. | [VERIFIED] | In scope |
| 6.3.1.4.5 | Sent Sample Sent Flag | أرسلت | Field / UI element | Marks the sample as one that has been sent. | [VERIFIED] | In scope |
| 6.3.1.5 | Sent Samples Report Type Options | — | Field group / Container | The kinds of listing the sent samples screen produces. | [VERIFIED] | In scope |
| 6.3.1.5.1 | Chosen Entity Count Option | حصر نتيجة البحث للجهة المحددة | Option / Subfunction | Counts the result of the search for the chosen entity. | [VERIFIED] | In scope |
| 6.3.1.5.2 | Show Not Sent Option | عرض العينات التي لم ترسل | Option / Subfunction | Shows the samples that have not been sent. | [VERIFIED] | In scope |
| 6.3.1.5.3 | Open Samples Account Option | حساب عينات مفتوح | Option / Subfunction | The account of the samples that are still open. | [VERIFIED] | In scope |
| 6.3.1.5.4 | Test Sent Outside Option | تحليل يرسل للخارج | Option / Subfunction | A test that is sent outside. | [VERIFIED] | In scope |
| 6.3.1.5.5 | Test Not Sent Option | تحليل لا يرسل | Option / Subfunction | A test that is not sent. | [VERIFIED] | In scope |
| 6.3.1.6 | Sent Samples Action Block | — | Block / Container | The actions of the sent samples screen. | [VERIFIED] | In scope |
| 6.3.1.6.1 | Send To Action | ترسل إلي | Action / Function | Opens the sending of the selected sample; the amount paid is entered from the price of the sent test and the confirm action is pressed. | [VERIFIED] | In scope |
| 6.3.1.6.2 | Sent Sample Confirm Action | موافق | Action / Function | Confirms the sending of the selected sample. | [VERIFIED] | In scope |
| 6.3.1.6.3 | Sent Sample Net Action | خالص | Action / Function | Records that the whole amount of the sent sample has been paid. | [VERIFIED] | In scope |
| 6.3.1.6.4 | Sent Sample Ignore Action | اهمال | Action / Function | Leaves the selected sample without being sent. | [VERIFIED] | In scope |
| 6.3.1.6.5 | Always To Action | دائماً إليه | Action / Function | Sends the sample always to the same destination. | [VERIFIED] | In scope |
| 6.3.1.6.6 | Sent Samples Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 6.3.1.7 | Sent Sample Selection Rule | — | Rule / Rule | When a sent sample is selected, the program recognises the laboratory the sample was sent to, the total number of samples sent to that laboratory in the chosen period, and that laboratory's total account with the amount paid and the amount remaining. | [VERIFIED] | In scope |
| **6.4** | **Cash and Counterparty Accounts** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 6.4.1 | Cash Deposit And Withdrawal Screen | صرف وإيداع نقدية | Screen / Function | The screen on which cash is deposited and withdrawn for the entities designated for it, or for other entities. The screen is named in the accounts window but is never displayed, so its fields and its behaviour are not established. | [NOT-SHOWN] | In scope |
| 6.4.2 | Companies And Representatives Account Screen | حساب شركات ومندوبين | Screen / Function | The screen of the accounts of the companies and the representatives. The screen is named in the accounts window but is never displayed, so its fields and its behaviour are not established. | [NOT-SHOWN] | In scope |
| **6.5** | **Account Statement Variants and Claims** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 6.5.1 | Account Statement Scope Options | — | Field group / Container | The parts of the account the statement covers. | [VERIFIED] | In scope |
| 6.5.1.1 | Statement Of All Patients Option | — | Option / Subfunction | The statement covers every patient of the period. | [VERIFIED] | In scope |
| 6.5.1.2 | Statement Of Balances Only Option | — | Option / Subfunction | The statement covers the patients who have not settled their account alone. | [VERIFIED] | In scope |
| 6.5.1.3 | Statement Of Settled Only Option | — | Option / Subfunction | The statement covers the patients who have settled their account alone. | [VERIFIED] | In scope |
| 6.5.2 | Statement Period Rule | — | Rule / Rule | The statement is produced for any period, computed for the patient on the doctor's request, or over a year, a month or a day. | [VERIFIED] | In scope |
| 6.5.3 | Detailed Account Statement | — | Output / Output | The statement that lists, under each patient, the tests of the request with their prices. | [VERIFIED] | In scope |
| 6.5.4 | Detailed Account Statement By Results | — | Output / Output | The statement that lists, under each patient, the tests of the request with their results, their units and the state of each result against its range. A test that has a report of its own is shown with an instruction to open it. | [VERIFIED] | In scope |
| 6.5.5 | Claim Report | مطالبة | Output / Output | The claim raised on a referring entity for the period, carrying the results of the group of patients that were referred to a doctor. | [VERIFIED] | In scope |
| 6.5.6 | Four Report Variants Rule | — | Rule / Rule | Four reports are produced of the account of a referring entity: in detail by the results, in detail by the prices of the tests, grouped by the patient, and grouped by every collecting entity of the period. | [VERIFIED] | In scope |
| 6.5.7 | Sent Samples Account Statement | — | Output / Output | The statement of the samples sent outside over a period, carrying the entity, the test, the request date, the patient, the price of the test, the amount settled and the amount remaining. It may be produced for each patient sample or grouped for each entity. | [VERIFIED] | In scope |
| 6.5.8 | Sent Samples Statement Grid | اسم الجهة  ·  اسم التحليل  ·  تاريخ الطلب  ·  كود المريض  ·  اسم المريض  ·  سعر التحليل  ·  المسدد  ·  الباقي | Grid / UI element | The grid of the sent-samples statement; one row is one sample sent outside. | [VERIFIED] | In scope |
| 6.5.9 | Stocktaking Figure Label Variant | — | Rule / Rule | An alternative set of labels is used for the stocktaking figures: the collection, the amount not collected and the balance in the till, in place of the total after the discount, the amount not collected and the amount available in the till. | [VERIFIED] | In scope |
| 6.5.10 | Accounts Window Items Rule | — | Rule / Rule | The accounts window carries five entries: the stocktaking and stock account, the cash deposit and withdrawal, the sales and the comprehensive income, the accounts of the companies and the representatives, and the samples sent outside. | [VERIFIED] | In scope |

### §11.3 Workflows

### Produce the stocktaking account over a period

1. Press the accounts ribbon item. → 6.1.1
2. Choose the inventory and stocktaking item. → 6.1.2.1
3. Where one device, or several devices joined to one database, serves all the users on the same branch, leave the branch at All; where each user carries a branch number, in the branch system of the laboratory, choose the branch to be shown. → 6.2.1.1.1, 6.2.2
4. Set the date the period starts at and the date it ends at. → 6.2.1.1.2, 6.2.1.1.3
5. To break the account down, choose the element on the right of the window — the users, the sent samples, the treating doctor, the referring entity or the type of account — and the type of report with it. → 6.2.1.2, 6.2.3
6. Press the display action; the program shows the total of the patients' samples, the value of the discounts, the net profit after the collection and the settlement, and the figures of the account. → 6.2.1.1.4, 6.2.1.3
7. To see the amounts in detail, press the total of the patients' samples; the patients' account report opens, with one row per request. → 6.2.4, 6.2.4.2

### Produce the stocktaking account of one treating doctor

1. Open the inventory and stocktaking screen, choose the branch and the period, and press display. → 6.2.1.1
2. In the treating doctor group, choose the doctor to be shown; the doctor is chosen from the list that opens beside the group. → 6.2.1.2.3, 6.2.1.2.3.1
3. Choose the type of report: in detail by result, in detail by price, in detail, or in summary. → 6.2.1.2.3
4. The report of the patients of that doctor opens, with the request date, the patient code, the patient name and the amounts of each request, and a total row. → 6.2.5

### Settle the account of the samples sent outside

1. Press the accounts ribbon item, then choose the sent samples outside item. → 6.1.2.2
2. Set the date the request period starts at and the date it ends at, choose whether only the samples not sent are listed or all of them, and press the display action. → 6.3.1.1
3. The samples of the period are listed with their request date, test, patient, laboratory, cost and amount paid. → 6.3.1.3
4. Select a sent sample; the program recognises the laboratory it was sent to, the number of samples sent to that laboratory in the period, and that laboratory's total account with the amount paid and the amount remaining. → 6.3.1.7
5. Press the send-to action, enter the amount paid from the price of the sent test, and press the confirm action. → 6.3.1.6.1, 6.3.1.4.3, 6.3.1.4.4, 6.3.1.6.2
6. Where the whole amount is paid, press the net action instead. → 6.3.1.6.3
7. Repeat the step for every test to be sent. → 6.3.1.6
8. To count the tests of one chosen entity, use the search for that entity; the tests sent to the chosen laboratory alone are then shown. → 6.3.1.5.1

### §11.4 Rules, validations and conditional behaviour

| ID | Rule | Tag |
|---|---|---|
| 6.1.2.3 | The cash deposit and withdrawal screen is named in the menu but is not shown in the source. | [VERIFIED] |
| 6.1.2.4 | The companies and representatives account screen is named in the menu but is not shown in the source. | [VERIFIED] |
| 6.2.1.1.1 | Where all the users are on the same branch the branch is left at All; in the branch system the branch number is chosen instead. | [VERIFIED] |
| 6.2.1.2.2 | The sent samples breakdown offers three types of report: in detail, in detail by price, and in summary. | [VERIFIED] |
| 6.2.1.2.3 | The treating doctor breakdown offers four types of report: in detail by result, in detail by price, in detail, and in summary. | [VERIFIED] |
| 6.2.1.2.4 | The referring entity breakdown offers four types of report: in detail by result, in detail by price, in detail, and in summary. | [VERIFIED] |
| 6.2.1.2.5 | The account type breakdown offers five types of report: the individual account type, in detail by result, in detail by price, in detail, and in summary. | [VERIFIED] |
| 6.2.1.3.1 | The sign beside a figure says how the figure enters the account: the equal sign, the plus sign, the minus sign, or the asterisk of a figure the program computes. | [VERIFIED] |
| 6.2.1.3.12 | What is available in the till is the figure the program computes from the figures above it. | [VERIFIED] |
| 6.2.1.3.15 | The net profit is the figure the program computes after the collection and the settlement. | [VERIFIED] |
| 6.2.4 | The patients' account report is opened by pressing the total of the patients' samples. | [VERIFIED] |
| 6.3.1.6.3 | The net action is pressed where the whole amount of the sent sample has been paid. | [VERIFIED] |
| 6.3.1.7 | Selecting a sent sample shows the account of the laboratory it was sent to, over the chosen period. | [VERIFIED] |

### §11.5 Outputs

The artefacts below are produced by the elements listed. The content of each artefact is defined once, in the element's own definition, and is not repeated here.

| ID | Output | Tag |
|---|---|---|
| 6.2.4 | Report of the patients' accounts over a period | [VERIFIED] |
| 6.2.5 | Report of the accounts of the patients of one treating doctor | [VERIFIED] |
| 6.3.1.2 | Figures of the account of the chosen laboratory | [VERIFIED] |

### §11.6 Screens and interface structure

| Screen or output | Element identifiers |
|---|---|
| Accounts Module Entry | 6.1, 6.1.1, 6.1.2, 6.1.2.1, 6.1.2.2, 6.1.2.3, 6.1.2.4, 6.1.3, 6.1.3.1, 6.1.3.2, 6.1.3.3, 6.1.3.4 |
| Inventory Account Screen | 6.2.1, 6.2.1.1, 6.2.1.1.1, 6.2.1.1.2, 6.2.1.1.3, 6.2.1.1.4, 6.2.1.2, 6.2.1.2.1, 6.2.1.2.1.1, 6.2.1.2.1.2, 6.2.1.2.2, 6.2.1.2.2.1, 6.2.1.2.2.2, 6.2.1.2.2.3, 6.2.1.2.3, 6.2.1.2.3.1, 6.2.1.2.3.2, 6.2.1.2.3.3, 6.2.1.2.3.4, 6.2.1.2.4, 6.2.1.2.4.1, 6.2.1.2.4.2, 6.2.1.2.4.3, 6.2.1.2.4.4, 6.2.1.2.5, 6.2.1.2.5.1, 6.2.1.2.5.2, 6.2.1.2.5.3, 6.2.1.2.5.4, 6.2.1.2.5.5, 6.2.1.3, 6.2.1.3.1, 6.2.1.3.2, 6.2.1.3.3, 6.2.1.3.4, 6.2.1.3.5, 6.2.1.3.6, 6.2.1.3.7, 6.2.1.3.8, 6.2.1.3.9, 6.2.1.3.10, 6.2.1.3.11, 6.2.1.3.12, 6.2.1.3.13, 6.2.1.3.14, 6.2.1.3.15, 6.2.1.4, 6.2.1.4.1 |
| Sent Samples Account Screen | 6.3.1, 6.3.1.1, 6.3.1.1.1, 6.3.1.1.2, 6.3.1.1.3, 6.3.1.1.4, 6.3.1.2, 6.3.1.2.1, 6.3.1.2.2, 6.3.1.2.3, 6.3.1.2.4, 6.3.1.3, 6.3.1.3.1, 6.3.1.3.2, 6.3.1.3.3, 6.3.1.3.4, 6.3.1.3.5, 6.3.1.3.6, 6.3.1.3.7, 6.3.1.4, 6.3.1.4.1, 6.3.1.4.2, 6.3.1.4.3, 6.3.1.4.4, 6.3.1.4.5, 6.3.1.5, 6.3.1.5.1, 6.3.1.5.2, 6.3.1.5.3, 6.3.1.5.4, 6.3.1.5.5, 6.3.1.6, 6.3.1.6.1, 6.3.1.6.2, 6.3.1.6.3, 6.3.1.6.4, 6.3.1.6.5, 6.3.1.6.6, 6.3.1.7 |
| Cash Deposit And Withdrawal Screen | 6.4.1 |
| Companies And Representatives Account Screen | 6.4.2 |
| Patient Account Detail Report | 6.2.4, 6.2.4.1, 6.2.4.2, 6.2.4.2.1, 6.2.4.2.2, 6.2.4.2.3, 6.2.4.2.4, 6.2.4.2.5, 6.2.4.2.6, 6.2.4.2.7, 6.2.4.2.8, 6.2.4.3 |
| Doctor Account Detail Report | 6.2.5, 6.2.5.1, 6.2.5.2 |

### §11.7 Uncertainties and unshown elements

| ID | Item | Tag |
|---|---|---|
| 6.2.1.2.2.1 | Two of the three options of the sent samples breakdown print the same wording; the difference between the two is not established. | [UNVERIFIED] |
| 6.2.1.2.2.2 | Two of the three options of the sent samples breakdown print the same wording; the difference between the two is not established. | [UNVERIFIED] |
| 6.4.1 | The fields and the behaviour of the cash deposit and withdrawal screen are not established. | [NOT-SHOWN] |
| 6.4.2 | The fields and the behaviour of the companies and representatives account screen are not established. | [NOT-SHOWN] |
| 6.2.5 | The columns of the treating doctor report are the same as those of the patients' account report, but the report is not shown in full. | [UNVERIFIED] |
| — | No validation, rejection message or recovery path is established for the accounts screens. | [NOT-SHOWN] |

## §12 Unit 7 — Basic System Settings

### §12.1 Functional hierarchy

```
7  Basic System Settings
├── 7.1  Settings Module Entry and System Settings Screen
│   ├── 7.1.1  Setting Ribbon Item
│   ├── 7.1.2  System Settings Item
│   ├── 7.1.3  Database Maintenance Item
│   ├── 7.1.4  System Settings Screen
│   │   ├── 7.1.4.1  Settings Tab Strip
│   │   │   ├── 7.1.4.1.1  System Tab
│   │   │   ├── 7.1.4.1.2  Report Tab
│   │   │   ├── 7.1.4.1.3  Receipt Tab
│   │   │   ├── 7.1.4.1.4  Envelope Tab
│   │   │   ├── 7.1.4.1.5  Card Tab
│   │   │   └── 7.1.4.1.6  Database Server Tab
│   │   ├── 7.1.4.2  Session Information Block
│   │   │   ├── 7.1.4.2.1  Server Type Indicator
│   │   │   ├── 7.1.4.2.2  Session User Name
│   │   │   ├── 7.1.4.2.3  Session Last Login
│   │   │   ├── 7.1.4.2.4  Database Type Indicator
│   │   │   └── 7.1.4.2.5  Session Today
│   │   ├── 7.1.4.3  General Settings Block
│   │   │   ├── 7.1.4.3.1  English Patient And Doctor Name Option
│   │   │   ├── 7.1.4.3.2  Blank Referring Entity Default Option
│   │   │   ├── 7.1.4.3.3  Keep Treating Doctor Option
│   │   │   ├── 7.1.4.3.4  Search While Entering Patient Name Option
│   │   │   ├── 7.1.4.3.5  No Automatic Titles Option
│   │   │   ├── 7.1.4.3.6  Tube Barcode Date And Time Option
│   │   │   ├── 7.1.4.3.7  External File Barcode Option
│   │   │   ├── 7.1.4.3.8  Lab Code Instead Of Patient Code Option
│   │   │   ├── 7.1.4.3.9  Automatic Tests Review Option
│   │   │   ├── 7.1.4.3.10  Patient Account Instead Of Print Date Option
│   │   │   ├── 7.1.4.3.11  File Or Card Number Instead Of Print Date Option
│   │   │   ├── 7.1.4.3.12  Daily Database Backup Option
│   │   │   ├── 7.1.4.3.13  Backup Path Field
│   │   │   └── 7.1.4.3.14  Backup Fallback Path Rule
│   │   ├── 7.1.4.4  Database Server Settings Block
│   │   │   ├── 7.1.4.4.1  Server Name Field
│   │   │   ├── 7.1.4.4.2  Server Login Field
│   │   │   └── 7.1.4.4.3  Server Database Name Field
│   │   └── 7.1.4.5  System Settings Action Block
│   │       ├── 7.1.4.5.1  System Settings Save Action
│   │       ├── 7.1.4.5.2  System Settings Back Action
│   │       └── 7.1.4.5.3  System Settings Main List Action
│   └── 7.1.5  Database Maintenance Capabilities
│       ├── 7.1.5.1  Basic System Settings Capability
│       ├── 7.1.5.2  Server Data Capability
│       ├── 7.1.5.3  Backup And Restore Capability
│       ├── 7.1.5.4  Update Capability
│       └── 7.1.5.5  Prepare For Recent Version Capability
├── 7.2  Report Settings
│   ├── 7.2.1  Report Settings Tab
│   │   ├── 7.2.1.1  Report Signature Block
│   │   │   ├── 7.2.1.1.1  Report Signature Caption
│   │   │   ├── 7.2.1.1.2  Report Signature Text Field
│   │   │   └── 7.2.1.1.3  Report Signature Font Selector
│   │   │       ├── 7.2.1.1.3.1  Signature Font Preset One
│   │   │       ├── 7.2.1.1.3.2  Signature Font Preset Two
│   │   │       └── 7.2.1.1.3.3  Signature Font Preset Three
│   │   ├── 7.2.1.2  Report Margins Block
│   │   │   ├── 7.2.1.2.1  Page Margin Left Field
│   │   │   ├── 7.2.1.2.2  Page Margin Bottom Field
│   │   │   └── 7.2.1.2.3  Report Top Space Field
│   │   ├── 7.2.1.3  Report Paper Size Selector
│   │   │   ├── 7.2.1.3.1  Report Paper Size A4 Option
│   │   │   └── 7.2.1.3.2  Report Paper Size A5 Option
│   │   ├── 7.2.1.4  Patient Date Sorting Group
│   │   │   ├── 7.2.1.4.1  Sort By Lab Code Option
│   │   │   └── 7.2.1.4.2  Sort By Patient Name Option
│   │   ├── 7.2.1.5  Header Footer Printing Option Group
│   │   │   ├── 7.2.1.5.1  Do Not Print Header Footer Option
│   │   │   ├── 7.2.1.5.2  Print Header Footer As Words Option
│   │   │   └── 7.2.1.5.3  Print Header Footer From Images Option
│   │   ├── 7.2.1.6  Report Image Block
│   │   │   ├── 7.2.1.6.1  Lab Logo Image Field
│   │   │   ├── 7.2.1.6.2  Lab Logo Image Browse Action
│   │   │   ├── 7.2.1.6.3  Lab Logo Image Clear Action
│   │   │   ├── 7.2.1.6.4  Report Header Image Field
│   │   │   ├── 7.2.1.6.5  Report Header Image Browse Action
│   │   │   ├── 7.2.1.6.6  Report Header Image Clear Action
│   │   │   ├── 7.2.1.6.7  Report Footer Image Field
│   │   │   ├── 7.2.1.6.8  Report Footer Image Browse Action
│   │   │   └── 7.2.1.6.9  Report Footer Image Clear Action
│   │   ├── 7.2.1.7  Report Text Block
│   │   │   ├── 7.2.1.7.1  Lab Logo Name Text Area
│   │   │   ├── 7.2.1.7.2  Lab Logo Name Font Selector
│   │   │   │   ├── 7.2.1.7.2.1  Lab Logo Name Font Preset One
│   │   │   │   ├── 7.2.1.7.2.2  Lab Logo Name Font Preset Two
│   │   │   │   ├── 7.2.1.7.2.3  Lab Logo Name Font Preset Three
│   │   │   │   └── 7.2.1.7.2.4  Lab Logo Name Font Preset Four
│   │   │   ├── 7.2.1.7.3  Lab Logo Contact Text Area
│   │   │   ├── 7.2.1.7.4  Lab Logo Contact Font Selector
│   │   │   │   ├── 7.2.1.7.4.1  Lab Logo Contact Font Preset One
│   │   │   │   ├── 7.2.1.7.4.2  Lab Logo Contact Font Preset Two
│   │   │   │   ├── 7.2.1.7.4.3  Lab Logo Contact Font Preset Three
│   │   │   │   └── 7.2.1.7.4.4  Lab Logo Contact Font Preset Four
│   │   │   ├── 7.2.1.7.5  Report Header Text Area
│   │   │   ├── 7.2.1.7.6  Report Header Font Selector
│   │   │   │   ├── 7.2.1.7.6.1  Report Header Font Preset One
│   │   │   │   ├── 7.2.1.7.6.2  Report Header Font Preset Two
│   │   │   │   ├── 7.2.1.7.6.3  Report Header Font Preset Three
│   │   │   │   └── 7.2.1.7.6.4  Report Header Font Preset Four
│   │   │   ├── 7.2.1.7.7  Report Footer Text Area
│   │   │   └── 7.2.1.7.8  Report Footer Font Selector
│   │   │       ├── 7.2.1.7.8.1  Report Footer Font Preset One
│   │   │       ├── 7.2.1.7.8.2  Report Footer Font Preset Two
│   │   │       ├── 7.2.1.7.8.3  Report Footer Font Preset Three
│   │   │       └── 7.2.1.7.8.4  Report Footer Font Preset Four
│   │   ├── 7.2.1.8  Report Settings Note Block
│   │   │   ├── 7.2.1.8.1  Dimensions In Centimetres Note
│   │   │   └── 7.2.1.8.2  Image File Size Note
│   │   └── 7.2.1.9  Report Settings Action Block
│   │       ├── 7.2.1.9.1  Report Settings Save Action
│   │       ├── 7.2.1.9.2  Header Footer Colours Action
│   │       ├── 7.2.1.9.3  Report Settings Back Action
│   │       └── 7.2.1.9.4  Report Settings Main List Action
│   ├── 7.2.2  Report Margins Adjustment
│   ├── 7.2.3  Paper Size Adjustment
│   ├── 7.2.4  Header Footer Printing Choices
│   └── 7.2.5  Header Footer Layout Dialog
│       ├── 7.2.5.1  Show Top Line Toggle
│       ├── 7.2.5.2  Show Bottom Line Toggle
│       ├── 7.2.5.3  Long Date Toggle
│       ├── 7.2.5.4  Show Time Toggle
│       ├── 7.2.5.5  Header Footer Layout Grid
│       │   ├── 7.2.5.5.1  Layout Element Name Column
│       │   ├── 7.2.5.5.2  Layout Show Flag Column
│       │   ├── 7.2.5.5.3  Layout Width Column
│       │   ├── 7.2.5.5.4  Layout Height Column
│       │   ├── 7.2.5.5.5  Layout Top Column
│       │   ├── 7.2.5.5.6  Layout Left Column
│       │   ├── 7.2.5.5.7  Layout Patient Name Row
│       │   ├── 7.2.5.5.8  Layout Patient Id Row
│       │   ├── 7.2.5.5.9  Layout Age And Sex Row
│       │   ├── 7.2.5.5.10  Layout Request Date Row
│       │   ├── 7.2.5.5.11  Layout Referred By Row
│       │   └── 7.2.5.5.12  Layout Printed In Row
│       ├── 7.2.5.6  Patient History Format Grid
│       │   ├── 7.2.5.6.1  History Test Name Column
│       │   ├── 7.2.5.6.2  History Result Column
│       │   ├── 7.2.5.6.3  History Date Column
│       │   ├── 7.2.5.6.4  History Bold Flag Column
│       │   ├── 7.2.5.6.5  History Row Height Column
│       │   └── 7.2.5.6.6  Patient History Row
│       ├── 7.2.5.7  Layout Dialog Action Block
│       │   ├── 7.2.5.7.1  Layout Dialog Back Action
│       │   ├── 7.2.5.7.2  Restore Default Layout Action
│       │   └── 7.2.5.7.3  Layout Dialog Save Action
│       ├── 7.2.5.8  Colour Picker Block
│       │   ├── 7.2.5.8.1  Basic Colours Palette
│       │   ├── 7.2.5.8.2  Custom Colours Palette
│       │   ├── 7.2.5.8.3  Define Custom Colours Action
│       │   ├── 7.2.5.8.4  Colour Confirm Action
│       │   ├── 7.2.5.8.5  Colour Cancel Action
│       │   └── 7.2.5.8.6  Colour Dialog Close Action
│       ├── 7.2.5.9  Layout Change Applies Everywhere Rule
│       └── 7.2.5.10  Colour Change Rule
├── 7.3  Dedicated Printers
│   ├── 7.3.1  Dedicated Printer Selector Block
│   │   ├── 7.3.1.1  Default Printer Selector
│   │   ├── 7.3.1.2  Barcode Printer Selector
│   │   ├── 7.3.1.3  Envelope Printer Selector
│   │   ├── 7.3.1.4  Receipt Printer Selector
│   │   └── 7.3.1.5  Card Printer Selector
│   └── 7.3.2  Dedicated Printer Rule
├── 7.4  Default Account Type
│   ├── 7.4.1  Default Account Type Selector
│   │   ├── 7.4.1.1  Default Account Type Individual Option
│   │   ├── 7.4.1.2  Default Account Type Lab To Lab Option
│   │   ├── 7.4.1.3  Default Account Type Contracts Option
│   │   └── 7.4.1.4  Default Account Type Free Option
│   └── 7.4.2  Default Account Type Rule
├── 7.5  Receipt Settings
│   ├── 7.5.1  Receipt Settings Tab
│   │   ├── 7.5.1.1  Receipt Currency Block
│   │   │   ├── 7.5.1.1.1  Receipt Currency Local Option
│   │   │   └── 7.5.1.1.2  Receipt Currency Abbreviated Option
│   │   ├── 7.5.1.2  Result Delivery Time Field
│   │   ├── 7.5.1.3  Receipt Top Margin Field
│   │   ├── 7.5.1.4  Print Receipt Once Option
│   │   ├── 7.5.1.5  Print Delivery Data Option
│   │   ├── 7.5.1.6  Receipt Test Details Group
│   │   │   ├── 7.5.1.6.1  Hide Test Details Option
│   │   │   ├── 7.5.1.6.2  Show Test Details Option
│   │   │   ├── 7.5.1.6.3  Show Test Details With Code Option
│   │   │   └── 7.5.1.6.4  Cash Printer Option
│   │   ├── 7.5.1.7  Receipt Text Block
│   │   │   ├── 7.5.1.7.1  Receipt Right Header Text Area
│   │   │   ├── 7.5.1.7.2  Receipt Right Header Font Selector
│   │   │   │   ├── 7.5.1.7.2.1  Receipt Right Header Font Preset One
│   │   │   │   └── 7.5.1.7.2.2  Receipt Right Header Font Preset Two
│   │   │   ├── 7.5.1.7.3  Receipt Middle Text Area
│   │   │   ├── 7.5.1.7.4  Receipt Middle Font Selector
│   │   │   │   ├── 7.5.1.7.4.1  Receipt Middle Font Preset One
│   │   │   │   └── 7.5.1.7.4.2  Receipt Middle Font Preset Two
│   │   │   ├── 7.5.1.7.5  Receipt Height Field
│   │   │   └── 7.5.1.7.6  Receipt Text Font Type Selector
│   │   │       ├── 7.5.1.7.6.1  Receipt Text Font Preset One
│   │   │       └── 7.5.1.7.6.2  Receipt Text Font Preset Two
│   │   ├── 7.5.1.8  Receipt Image Block
│   │   │   ├── 7.5.1.8.1  Receipt Header Image Field
│   │   │   ├── 7.5.1.8.2  Receipt Header Image Browse Action
│   │   │   ├── 7.5.1.8.3  Receipt Header Image Clear Action
│   │   │   ├── 7.5.1.8.4  Receipt Lab Logo Image Field
│   │   │   ├── 7.5.1.8.5  Receipt Lab Logo Browse Action
│   │   │   ├── 7.5.1.8.6  Receipt Lab Logo Clear Action
│   │   │   ├── 7.5.1.8.7  Receipt Footer Image Field
│   │   │   ├── 7.5.1.8.8  Receipt Footer Image Browse Action
│   │   │   └── 7.5.1.8.9  Receipt Footer Image Clear Action
│   │   ├── 7.5.1.9  Receipt Settings Note Block
│   │   │   ├── 7.5.1.9.1  Receipt Dimensions Note
│   │   │   └── 7.5.1.9.2  Receipt Image Size Note
│   │   └── 7.5.1.10  Receipt Settings Action Block
│   │       ├── 7.5.1.10.1  Receipt Settings Save Action
│   │       ├── 7.5.1.10.2  Receipt Settings Back Action
│   │       └── 7.5.1.10.3  Receipt Settings Main List Action
│   └── 7.5.2  Receipt Settings Rule
├── 7.6  Envelope Settings
│   ├── 7.6.1  Envelope Settings Tab
│   │   ├── 7.6.1.1  Envelope Header Footer Option Group
│   │   │   ├── 7.6.1.1.1  Do Not Print Envelope Header Footer Option
│   │   │   ├── 7.6.1.1.2  Print Envelope Header Footer As Words Option
│   │   │   └── 7.6.1.1.3  Print Envelope Header Footer From Images Option
│   │   ├── 7.6.1.2  Envelope Top Margin Field
│   │   ├── 7.6.1.3  Envelope Text Block
│   │   │   ├── 7.6.1.3.1  Envelope Right Header Text Area
│   │   │   ├── 7.6.1.3.2  Envelope Font Selector
│   │   │   │   ├── 7.6.1.3.2.1  Envelope Font Preset One
│   │   │   │   ├── 7.6.1.3.2.2  Envelope Font Preset Two
│   │   │   │   ├── 7.6.1.3.2.3  Envelope Font Preset Three
│   │   │   │   ├── 7.6.1.3.2.4  Envelope Font Preset Four
│   │   │   │   ├── 7.6.1.3.2.5  Envelope Font Preset Five
│   │   │   │   ├── 7.6.1.3.2.6  Envelope Font Preset Six
│   │   │   │   ├── 7.6.1.3.2.7  Envelope Font Preset Seven
│   │   │   │   ├── 7.6.1.3.2.8  Envelope Font Preset Eight
│   │   │   │   ├── 7.6.1.3.2.9  Envelope Font Preset Nine
│   │   │   │   └── 7.6.1.3.2.10  Envelope Font Preset Ten
│   │   │   └── 7.6.1.3.3  Envelope Height Field
│   │   ├── 7.6.1.4  Envelope Image Block
│   │   │   ├── 7.6.1.4.1  Envelope Header Image Field
│   │   │   ├── 7.6.1.4.2  Envelope Header Image Browse Action
│   │   │   ├── 7.6.1.4.3  Envelope Header Image Clear Action
│   │   │   ├── 7.6.1.4.4  Envelope Lab Logo Image Field
│   │   │   ├── 7.6.1.4.5  Envelope Lab Logo Browse Action
│   │   │   ├── 7.6.1.4.6  Envelope Lab Logo Clear Action
│   │   │   ├── 7.6.1.4.7  Envelope Footer Image Field
│   │   │   ├── 7.6.1.4.8  Envelope Footer Image Browse Action
│   │   │   └── 7.6.1.4.9  Envelope Footer Image Clear Action
│   │   ├── 7.6.1.5  Envelope Data Alignment Block
│   │   │   ├── 7.6.1.5.1  Envelope Data Alignment Grid
│   │   │   │   ├── 7.6.1.5.1.1  Envelope Alignment Left Column
│   │   │   │   ├── 7.6.1.5.1.2  Envelope Alignment Top Column
│   │   │   │   ├── 7.6.1.5.1.3  Envelope Patient Name Row
│   │   │   │   ├── 7.6.1.5.1.4  Envelope Referring Entity Row
│   │   │   │   ├── 7.6.1.5.1.5  Envelope Patient Code Row
│   │   │   │   └── 7.6.1.5.1.6  Envelope Date Row
│   │   │   └── 7.6.1.5.2  Envelope Data Titles Option
│   │   └── 7.6.1.6  Envelope Settings Action Block
│   │       ├── 7.6.1.6.1  Envelope Settings Save Action
│   │       └── 7.6.1.6.2  Envelope Settings Main List Action
│   └── 7.6.2  Envelope Settings Rule
└── 7.7  Receipt and Envelope Document Details
    ├── 7.7.1  Receipt Laboratory Data Lines
    ├── 7.7.2  Receipt Results Password Field
    └── 7.7.3  Receipt Print Once Rule
```

### §12.2 Function inventory

The elements of the unit, each defined once, at the place that carries it. The *Type / Level* column gives the kind of the element and the level at which it is recorded under the policy of §3. The *Scope* column marks the elements that belong to one of the five excluded categories of §2; they are documented, and they are not parity requirements.

| ID | Canonical name | Source label | Type / Level | Definition | Evidence | Scope |
|---|---|---|---|---|---|---|
| **7.1** | **Settings Module Entry and System Settings Screen** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 7.1.1 | Setting Ribbon Item | Setting | Navigation / Subfunction | The ribbon item that opens the settings module. | [VERIFIED] | In scope |
| 7.1.2 | System Settings Item | اعدادات النظام | Menu item / Subfunction | Opens the system settings window, in which the tabs of the system, the report, the receipt, the envelope, the card and the database server are carried. | [VERIFIED] | In scope |
| 7.1.3 | Database Maintenance Item | Database Maintenance | Menu item / Subfunction | Opens the database maintenance panel, which carries the basic system settings, the server data, the backup and restore, the update of the database and of the program files, and the preparation of the system for the recent version. | [VERIFIED] | In scope |
| 7.1.4 | System Settings Screen | نافذة اعدادات النظام | Screen / Container | Through this window the main tasks and the main data of the system may be customised, such as the branch number, the time the result is received, the main printers, and the automatic printing of the patient's receipt. | [VERIFIED] | In scope |
| 7.1.4.1 | Settings Tab Strip | — | Block / Container | The tabs the system settings window carries. | [VERIFIED] | In scope |
| 7.1.4.1.1 | System Tab | اعدادات النظام | Tab / UI element | The tab that holds the general options of the system. | [VERIFIED] | In scope |
| 7.1.4.1.2 | Report Tab | اعدادات التقرير | Tab / UI element | The tab that holds the settings of the report. | [VERIFIED] | In scope |
| 7.1.4.1.3 | Receipt Tab | اعدادات الإيصال | Tab / UI element | The tab that holds the settings of the receipt. | [VERIFIED] | In scope |
| 7.1.4.1.4 | Envelope Tab | اعدادات الظرف | Tab / UI element | The tab that holds the settings of the envelope. | [VERIFIED] | In scope |
| 7.1.4.1.5 | Card Tab | اعدادات الكارنية | Tab / UI element | The tab that holds the settings of the card. | [VERIFIED] | In scope |
| 7.1.4.1.6 | Database Server Tab | اعدادات خادم قواعد البيانات | Tab / UI element | The tab that holds the settings of the database server. | [VERIFIED] | In scope |
| 7.1.4.2 | Session Information Block | — | Block / Container | The state of the session shown on the window. | [VERIFIED] | In scope |
| 7.1.4.2.1 | Server Type Indicator | SQL SERVER | Indicator / UI element | The kind of database server the program runs on. | [VERIFIED] | In scope |
| 7.1.4.2.2 | Session User Name | User Name | Field / UI element | The name of the signed-in user. | [VERIFIED] | In scope |
| 7.1.4.2.3 | Session Last Login | Last Login | Field / UI element | The date and the time of the previous sign-in. | [VERIFIED] | In scope |
| 7.1.4.2.4 | Database Type Indicator | SQL DATABASE | Indicator / UI element | The kind of database the program runs on. | [VERIFIED] | In scope |
| 7.1.4.2.5 | Session Today | Today | Field / UI element | The current date and the time. | [VERIFIED] | In scope |
| 7.1.4.3 | General Settings Block | — | Block / Container | The general options of the system tab. The dedicated printer selectors and the default account type selector of this tab are defined in groups 7.3 and 7.4 and are not repeated here. | [VERIFIED] | In scope |
| 7.1.4.3.1 | English Patient And Doctor Name Option | ادخال اسم المريض والطبيب المعالج باللغة الانجليزية وباقي العناصر | Option / Subfunction | The patient's name and the treating doctor's name are entered in English and the remaining elements in the other script. | [VERIFIED] | In scope |
| 7.1.4.3.2 | Blank Referring Entity Default Option | في حالة ترك خانة جهة الاحالة فارغة يتم وضع herself, Himself حسب جنس المريض | Option / Subfunction | Where the referring entity is left empty, the wording that stands for the patient himself is placed according to the patient's sex. | [VERIFIED] | In scope |
| 7.1.4.3.3 | Keep Treating Doctor Option | حفظ الطبيب المعالج من نافذة الجهات الخارجية والمعامل وعدم حفظه تلقائياً عند ادخال المرضي | Option / Subfunction | The treating doctor entered through the external entities and laboratories window is kept, and is not saved automatically when the patient is entered. | [VERIFIED] | In scope |
| 7.1.4.3.4 | Search While Entering Patient Name Option | تمكين خاصية البحث عند ادخال اسم المريض في نافذة اضافة وتعديل بيانات المرضي | Option / Subfunction | The search is made available while the patient's name is entered in the add-and-edit patient data window. | [VERIFIED] | In scope |
| 7.1.4.3.5 | No Automatic Titles Option | عدم وضع الألقاب تلقائيا عند تسجيل بيانات المرضي | Option / Subfunction | The titles are not placed automatically when the patient's data is recorded. | [VERIFIED] | In scope |
| 7.1.4.3.6 | Tube Barcode Date And Time Option | طباعة التاريخ والوقت على باركود الانابيب | Option / Subfunction | The date and the time are printed on the barcode of the tube. | [VERIFIED] | In scope |
| 7.1.4.3.7 | External File Barcode Option | طباعة الباركود الخارجي الخاص بالملف | Option / Subfunction | The external barcode of the file is printed. | [VERIFIED] | In scope |
| 7.1.4.3.8 | Lab Code Instead Of Patient Code Option | طباعة كود المعمل بدل كود المريض في تقرير النتائج واستكرز الباركود | Option / Subfunction | The laboratory code is printed instead of the patient's code in the results report, and the barcode is repeated. | [VERIFIED] | In scope |
| 7.1.4.3.9 | Automatic Tests Review Option | مراجعة وإكمال التحاليل بطريقة تلقائية | Option / Subfunction | The tests are reviewed and completed automatically. | [VERIFIED] | In scope |
| 7.1.4.3.10 | Patient Account Instead Of Print Date Option | طباعة حساب المريض والباقي بدل تاريخ الطباعة في تقرير النتيجة | Option / Subfunction | The patient's account and the balance are printed instead of the print date in the result report. | [VERIFIED] | In scope |
| 7.1.4.3.11 | File Or Card Number Instead Of Print Date Option | طباعة رقم الملف او البطاقة بدل تاريخ الطباعة في تقرير النتيجة | Option / Subfunction | The file number or the card number is printed instead of the print date in the result report. | [VERIFIED] | In scope |
| 7.1.4.3.12 | Daily Database Backup Option | عمل نسخ احتياطي لقاعدة البيانات يومياً | Option / Subfunction | A backup of the database is made every day. | [VERIFIED] | In scope |
| 7.1.4.3.13 | Backup Path Field | Path | Field / UI element | The path the daily backup is written to. | [VERIFIED] | In scope |
| 7.1.4.3.14 | Backup Fallback Path Rule | * في حالة تفعيل الخاصية وعدم وجود المسار يتم الحفظ في المسار التالي | Rule / Rule | Where the daily backup is enabled and the path does not exist, the backup is saved in the fallback folder shown beneath the path field, under the data folder. | [VERIFIED] | In scope |
| 7.1.4.4 | Database Server Settings Block | — | Block / Container | The data of the server the program runs on. | [VERIFIED] | In scope |
| 7.1.4.4.1 | Server Name Field | اسم السيرفر | Field / UI element | The name of the server. | [VERIFIED] | In scope |
| 7.1.4.4.2 | Server Login Field | تسجيل الدخول | Field / UI element | The login of the server. | [VERIFIED] | In scope |
| 7.1.4.4.3 | Server Database Name Field | اسم قاعدة البيانات | Field / UI element | The name of the database. | [VERIFIED] | In scope |
| 7.1.4.5 | System Settings Action Block | — | Block / Container | The actions of the system settings window. | [VERIFIED] | In scope |
| 7.1.4.5.1 | System Settings Save Action | حفظ الإعدادات | Action / Function | Persists the settings of the tab that is open. | [VERIFIED] | In scope |
| 7.1.4.5.2 | System Settings Back Action | رجوع | Action / Subfunction | Steps back out of the system settings. | [VERIFIED] | In scope |
| 7.1.4.5.3 | System Settings Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 7.1.5 | Database Maintenance Capabilities | — | Block / Container | What the database maintenance panel allows. | [VERIFIED] | In scope |
| 7.1.5.1 | Basic System Settings Capability | تعديل اعدادات النظام الاساسية (وقت التسليم ، الطابعة المخصصة للباركود ، رقم الفرع .... ) | Function / Function | The basic system settings are edited, such as the delivery time, the dedicated barcode printer and the branch number. | [VERIFIED] | In scope |
| 7.1.5.2 | Server Data Capability | تعديل بيانات السيرفر (اسم السيرفر وتسجيل الدخول واسم قاعدة البيانات) | Function / Function | The server data is edited, being the server name, the login and the name of the database. | [VERIFIED] | In scope |
| 7.1.5.3 | Backup And Restore Capability | عمل نسخة احتياطية من قاعدة بيانات النظام او استرجاع نسخة سابقة | Function / Function | A backup of the system database is made, or an earlier copy is restored. | [VERIFIED] | In scope |
| 7.1.5.4 | Update Capability | تحديث قاعدة البيانات وملفات البرنامج للتوافق مع النسخ الحديثة من البرنامج | Function / Function | The database and the program files are updated so that they fit the recent versions of the program. | [VERIFIED] | In scope |
| 7.1.5.5 | Prepare For Recent Version Capability | تهيئة النظام للنسخة الحديثة | Function / Function | The system is prepared for the recent version. | [VERIFIED] | In scope |
| **7.2** | **Report Settings** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 7.2.1 | Report Settings Tab | اعدادات التقرير | Screen / Container | The tab of the system settings window that holds the settings of the report. | [VERIFIED] | In scope |
| 7.2.1.1 | Report Signature Block | — | Block / Container | The signature printed on the report. | [VERIFIED] | In scope |
| 7.2.1.1.1 | Report Signature Caption | Doctor's signature | Label / UI element | The caption of the signature on the report. | [VERIFIED] | In scope |
| 7.2.1.1.2 | Report Signature Text Field | Doctor's signature: | Field / UI element | The text of the signature printed on the report. | [VERIFIED] | In scope |
| 7.2.1.1.3 | Report Signature Font Selector | Font | Field group / Container | The font presets offered for the signature. | [VERIFIED] | In scope |
| 7.2.1.1.3.1 | Signature Font Preset One | Font | Option / Subfunction | One of the three font presets of the signature; the preset carries the size used for the signature. | [UNVERIFIED] | In scope |
| 7.2.1.1.3.2 | Signature Font Preset Two | Font | Option / Subfunction | One of the three font presets of the signature; the preset carries the size used for the signature. | [UNVERIFIED] | In scope |
| 7.2.1.1.3.3 | Signature Font Preset Three | Font | Option / Subfunction | One of the three font presets of the signature; the preset carries the size used for the signature. | [UNVERIFIED] | In scope |
| 7.2.1.2 | Report Margins Block | — | Block / Container | The margins of the printed report. | [VERIFIED] | In scope |
| 7.2.1.2.1 | Page Margin Left Field | Left | Field / UI element | The left margin of the page; the value shown is 1. | [VERIFIED] | In scope |
| 7.2.1.2.2 | Page Margin Bottom Field | Bottom | Field / UI element | The bottom margin of the page; the value shown is 0. | [VERIFIED] | In scope |
| 7.2.1.2.3 | Report Top Space Field | Report top Space | Field / UI element | The space reserved above the report for the laboratory's printed logo; the value shown is 3.5. | [VERIFIED] | In scope |
| 7.2.1.3 | Report Paper Size Selector | حجم الورق المستخدم في طباعة التقارير | Field / UI element | The size of the paper the reports are printed on. | [VERIFIED] | In scope |
| 7.2.1.3.1 | Report Paper Size A4 Option | A4 (Large) | Option / Subfunction | The reports are printed on the large A4 paper. | [VERIFIED] | In scope |
| 7.2.1.3.2 | Report Paper Size A5 Option | A5 | Option / Subfunction | The reports are printed on the A5 paper. | [VERIFIED] | In scope |
| 7.2.1.4 | Patient Date Sorting Group | طريقة فرز وتجميع التاريخ المرضي | Field group / Container | The way the patient's tests are sorted and grouped in the report. | [VERIFIED] | In scope |
| 7.2.1.4.1 | Sort By Lab Code Option | فرز وتجميع التاريخ المرضي حسب كود المعمل | Option / Subfunction | The patient's tests are sorted and grouped by the laboratory code. | [VERIFIED] | In scope |
| 7.2.1.4.2 | Sort By Patient Name Option | فرز وتجميع التاريخ المرضي بإسم المريض | Option / Subfunction | The patient's tests are sorted and grouped by the patient's name. | [VERIFIED] | In scope |
| 7.2.1.5 | Header Footer Printing Option Group | — | Field group / Container | The way the report's head and foot are printed. | [VERIFIED] | In scope |
| 7.2.1.5.1 | Do Not Print Header Footer Option | عدم طباعة رأس وذيل التقرير (استخدام ورق مطبوع) | Option / Subfunction | The head and the foot of the report are not printed, and pre-printed paper is used. | [VERIFIED] | In scope |
| 7.2.1.5.2 | Print Header Footer As Words Option | طباعة رأس وذيل التقرير علي هيئة كلمات منفصلة (استخدام ورق أبيض) | Option / Subfunction | The head and the foot of the report are printed as separate words, and plain paper is used. | [VERIFIED] | In scope |
| 7.2.1.5.3 | Print Header Footer From Images Option | طباعة رأس وذيل التقرير من ملفات الصور المرفقة في البرنامج (استخدام ورق أبيض) | Option / Subfunction | The head and the foot of the report are printed from the image files held in the program, and plain paper is used. | [VERIFIED] | In scope |
| 7.2.1.6 | Report Image Block | — | Block / Container | The images printed in the head and the foot of the report. | [VERIFIED] | In scope |
| 7.2.1.6.1 | Lab Logo Image Field | JPG Image 145 * 95 Pixels | Field / UI element | The laboratory's logo, shown at 145 by 95 pixels. | [VERIFIED] | In scope |
| 7.2.1.6.2 | Lab Logo Image Browse Action | ? | Action / Function | Chooses the laboratory's logo from the computer. | [VERIFIED] | In scope |
| 7.2.1.6.3 | Lab Logo Image Clear Action | X | Action / Function | Removes the laboratory's logo. | [VERIFIED] | In scope |
| 7.2.1.6.4 | Report Header Image Field | JPG Image 680 * 140 Pixels | Field / UI element | The full image of the report's head, shown at 680 by 140 pixels. | [VERIFIED] | In scope |
| 7.2.1.6.5 | Report Header Image Browse Action | ? | Action / Function | Chooses the full image of the report's head from the computer. | [VERIFIED] | In scope |
| 7.2.1.6.6 | Report Header Image Clear Action | X | Action / Function | Removes the full image of the report's head. | [VERIFIED] | In scope |
| 7.2.1.6.7 | Report Footer Image Field | JPG Image 680 * 50 Pixels | Field / UI element | The full image of the report's foot, shown at 680 by 50 pixels. | [VERIFIED] | In scope |
| 7.2.1.6.8 | Report Footer Image Browse Action | ? | Action / Function | Chooses the full image of the report's foot from the computer. | [VERIFIED] | In scope |
| 7.2.1.6.9 | Report Footer Image Clear Action | X | Action / Function | Removes the full image of the report's foot. | [VERIFIED] | In scope |
| 7.2.1.7 | Report Text Block | — | Block / Container | The words printed in the head and the foot of the report. | [VERIFIED] | In scope |
| 7.2.1.7.1 | Lab Logo Name Text Area | شعار المعمل | Field / UI element | The name of the laboratory, printed in the logo area of the report. | [SAMPLE] | In scope |
| 7.2.1.7.2 | Lab Logo Name Font Selector | Font | Field group / Container | The font presets offered for the laboratory's name. | [VERIFIED] | In scope |
| 7.2.1.7.2.1 | Lab Logo Name Font Preset One | Font: 0 | Option / Subfunction | One of the numbered font presets of the laboratory's name. | [VERIFIED] | In scope |
| 7.2.1.7.2.2 | Lab Logo Name Font Preset Two | Font: 1 | Option / Subfunction | One of the numbered font presets of the laboratory's name. | [VERIFIED] | In scope |
| 7.2.1.7.2.3 | Lab Logo Name Font Preset Three | Font: 2 | Option / Subfunction | One of the numbered font presets of the laboratory's name. | [VERIFIED] | In scope |
| 7.2.1.7.2.4 | Lab Logo Name Font Preset Four | Font: 3 | Option / Subfunction | One of the numbered font presets of the laboratory's name. | [VERIFIED] | In scope |
| 7.2.1.7.3 | Lab Logo Contact Text Area | — | Field / UI element | The address of the laboratory and its contact details, printed beneath its name. | [SAMPLE] | In scope |
| 7.2.1.7.4 | Lab Logo Contact Font Selector | Font | Field group / Container | The font presets offered for the contact details of the laboratory. | [VERIFIED] | In scope |
| 7.2.1.7.4.1 | Lab Logo Contact Font Preset One | Font: 0 | Option / Subfunction | One of the numbered font presets of the contact details of the laboratory. | [VERIFIED] | In scope |
| 7.2.1.7.4.2 | Lab Logo Contact Font Preset Two | Font: 1 | Option / Subfunction | One of the numbered font presets of the contact details of the laboratory. | [VERIFIED] | In scope |
| 7.2.1.7.4.3 | Lab Logo Contact Font Preset Three | Font: 2 | Option / Subfunction | One of the numbered font presets of the contact details of the laboratory. | [VERIFIED] | In scope |
| 7.2.1.7.4.4 | Lab Logo Contact Font Preset Four | Font: 3 | Option / Subfunction | One of the numbered font presets of the contact details of the laboratory. | [VERIFIED] | In scope |
| 7.2.1.7.5 | Report Header Text Area | صورة رأس التقرير كاملة | Field / UI element | The words printed in the full head of the report. | [SAMPLE] | In scope |
| 7.2.1.7.6 | Report Header Font Selector | Font | Field group / Container | The font presets offered for the full head of the report. | [VERIFIED] | In scope |
| 7.2.1.7.6.1 | Report Header Font Preset One | Font: 0 | Option / Subfunction | One of the numbered font presets of the full head of the report. | [VERIFIED] | In scope |
| 7.2.1.7.6.2 | Report Header Font Preset Two | Font: 1 | Option / Subfunction | One of the numbered font presets of the full head of the report. | [VERIFIED] | In scope |
| 7.2.1.7.6.3 | Report Header Font Preset Three | Font: 2 | Option / Subfunction | One of the numbered font presets of the full head of the report. | [VERIFIED] | In scope |
| 7.2.1.7.6.4 | Report Header Font Preset Four | Font: 3 | Option / Subfunction | One of the numbered font presets of the full head of the report. | [VERIFIED] | In scope |
| 7.2.1.7.7 | Report Footer Text Area | صورة ذيل التقرير كاملة | Field / UI element | The words printed in the full foot of the report. | [SAMPLE] | In scope |
| 7.2.1.7.8 | Report Footer Font Selector | Font | Field group / Container | The font presets offered for the full foot of the report. | [VERIFIED] | In scope |
| 7.2.1.7.8.1 | Report Footer Font Preset One | Font: 0 | Option / Subfunction | One of the numbered font presets of the full foot of the report. | [VERIFIED] | In scope |
| 7.2.1.7.8.2 | Report Footer Font Preset Two | Font: 1 | Option / Subfunction | One of the numbered font presets of the full foot of the report. | [VERIFIED] | In scope |
| 7.2.1.7.8.3 | Report Footer Font Preset Three | Font: 2 | Option / Subfunction | One of the numbered font presets of the full foot of the report. | [VERIFIED] | In scope |
| 7.2.1.7.8.4 | Report Footer Font Preset Four | Font: 3 | Option / Subfunction | One of the numbered font presets of the full foot of the report. | [VERIFIED] | In scope |
| 7.2.1.8 | Report Settings Note Block | — | Block / Container | The two notes printed at the foot of the report settings tab. | [VERIFIED] | In scope |
| 7.2.1.8.1 | Dimensions In Centimetres Note | * جميع الأبعاد المستخدمة بالسنتيميتر | Note / UI element | All the dimensions used are in centimetres. | [VERIFIED] | In scope |
| 7.2.1.8.2 | Image File Size Note | * حجم ملف الصور يجب أن لا يتعدي 1 ميجا بايت | Note / UI element | The size of an image file must not exceed one megabyte. | [VERIFIED] | In scope |
| 7.2.1.9 | Report Settings Action Block | — | Block / Container | The actions of the report settings tab. | [VERIFIED] | In scope |
| 7.2.1.9.1 | Report Settings Save Action | حفظ الإعدادات | Action / Function | Persists the settings of the report tab. | [VERIFIED] | In scope |
| 7.2.1.9.2 | Header Footer Colours Action | تعديل ألوان رأس وذيل التقرير | Action / Function | Opens the dialog that changes the head and the foot of the report. | [VERIFIED] | In scope |
| 7.2.1.9.3 | Report Settings Back Action | رجوع | Action / Subfunction | Steps back out of the report settings. | [VERIFIED] | In scope |
| 7.2.1.9.4 | Report Settings Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 7.2.2 | Report Margins Adjustment | — | Rule / Rule | The margins of the report, being the page margin and the report top space, are edited to raise or to lower the top margin of the report so that it fits the laboratory's logo printed on the result paper. The top margin is raised or lowered in the field kept for it, to a maximum of eight centimetres. | [VERIFIED] | In scope |
| 7.2.3 | Paper Size Adjustment | — | Rule / Rule | The size of the paper used in printing the report is changed by choosing A4 or A5 in the paper size field of the report tab. | [VERIFIED] | In scope |
| 7.2.4 | Header Footer Printing Choices | — | Rule / Rule | Three ways of printing the head and the foot of the report are available. The head and the foot are not printed where pre-printed paper is used. The head and the foot are printed as separate words, and the laboratory's data is entered in the fields kept for it with the size, the kind and the colour of the font controlled. The head and the foot are printed from the image files held in the program, and the head image and the foot image of the laboratory are chosen from the computer. | [VERIFIED] | In scope |
| 7.2.5 | Header Footer Layout Dialog | نافذة تعديل ألوان رأس وذيل التقرير | Screen / Container | The dialog in which the elements of the head and the foot of the report are placed, sized, shown and given their appearance. | [VERIFIED] | In scope |
| 7.2.5.1 | Show Top Line Toggle | Show top line | Field / UI element | Shows or hides the top line of the report. | [VERIFIED] | In scope |
| 7.2.5.2 | Show Bottom Line Toggle | Show bottom line | Field / UI element | Shows or hides the bottom line of the report. | [VERIFIED] | In scope |
| 7.2.5.3 | Long Date Toggle | Long date | Field / UI element | Controls whether the date in the report is written in the long form. | [VERIFIED] | In scope |
| 7.2.5.4 | Show Time Toggle | Show time | Field / UI element | Controls whether the time is shown in the report. | [VERIFIED] | In scope |
| 7.2.5.5 | Header Footer Layout Grid | — | Grid / UI element | The grid that places the elements of the head and the foot of the report; each row is one element and carries its own width, height, top and left. | [VERIFIED] | In scope |
| 7.2.5.5.1 | Layout Element Name Column | — | Grid column / UI element | The name of the element of the head or the foot. | [VERIFIED] | In scope |
| 7.2.5.5.2 | Layout Show Flag Column | — | Grid column / UI element | The check box that shows or hides the element. | [VERIFIED] | In scope |
| 7.2.5.5.3 | Layout Width Column | Width | Grid column / UI element | The width of the element. | [VERIFIED] | In scope |
| 7.2.5.5.4 | Layout Height Column | Height | Grid column / UI element | The height of the element. | [VERIFIED] | In scope |
| 7.2.5.5.5 | Layout Top Column | Top | Grid column / UI element | The distance of the element from the top, in centimetres. | [VERIFIED] | In scope |
| 7.2.5.5.6 | Layout Left Column | Left | Grid column / UI element | The distance of the element from the left, in centimetres. | [VERIFIED] | In scope |
| 7.2.5.5.7 | Layout Patient Name Row | Patient Name | Grid row / UI element | The patient's name, placed on the top or the bottom line of the report. | [VERIFIED] | In scope |
| 7.2.5.5.8 | Layout Patient Id Row | Patient Id | Grid row / UI element | The patient's identifier, placed on the top or the bottom line of the report. | [VERIFIED] | In scope |
| 7.2.5.5.9 | Layout Age And Sex Row | Age / Sex | Grid row / UI element | The patient's age and sex, placed on the top or the bottom line of the report. | [VERIFIED] | In scope |
| 7.2.5.5.10 | Layout Request Date Row | Request Date | Grid row / UI element | The request date, placed on the top or the bottom line of the report. | [VERIFIED] | In scope |
| 7.2.5.5.11 | Layout Referred By Row | Referred By | Grid row / UI element | The referring doctor, placed on the top or the bottom line of the report. | [VERIFIED] | In scope |
| 7.2.5.5.12 | Layout Printed In Row | Printed In | Grid row / UI element | The moment the report was printed, placed on the top or the bottom line of the report. | [VERIFIED] | In scope |
| 7.2.5.6 | Patient History Format Grid | — | Grid / UI element | The grid that formats the table of the patient's earlier tests as it is printed inside the report. | [VERIFIED] | In scope |
| 7.2.5.6.1 | History Test Name Column | — | Grid column / UI element | The name of the earlier test. | [VERIFIED] | In scope |
| 7.2.5.6.2 | History Result Column | — | Grid column / UI element | The result of the earlier test. | [VERIFIED] | In scope |
| 7.2.5.6.3 | History Date Column | — | Grid column / UI element | The date of the earlier test. | [VERIFIED] | In scope |
| 7.2.5.6.4 | History Bold Flag Column | Bold | Grid column / UI element | The check box that prints the row of the earlier test in bold. | [VERIFIED] | In scope |
| 7.2.5.6.5 | History Row Height Column | Height | Grid column / UI element | The height of the row of the earlier test. | [VERIFIED] | In scope |
| 7.2.5.6.6 | Patient History Row | — | Grid row / UI element | One row of the table, standing for one earlier test of the patient. | [VERIFIED] | In scope |
| 7.2.5.7 | Layout Dialog Action Block | — | Block / Container | The actions of the head and foot dialog. | [VERIFIED] | In scope |
| 7.2.5.7.1 | Layout Dialog Back Action | عودة | Action / Subfunction | Steps back out of the dialog. | [VERIFIED] | In scope |
| 7.2.5.7.2 | Restore Default Layout Action | استعادة الوضع الافتراضي | Action / Function | Restores the default placement of the head and the foot of the report. | [VERIFIED] | In scope |
| 7.2.5.7.3 | Layout Dialog Save Action | حفظ الاعدادات | Action / Function | Persists the placement of the head and the foot of the report. | [VERIFIED] | In scope |
| 7.2.5.8 | Colour Picker Block | — | Block / Container | The colour chooser that opens when an element of the head or the foot is selected. | [VERIFIED] | In scope |
| 7.2.5.8.1 | Basic Colours Palette | Basic colors | Field / UI element | The set of colours offered by the chooser. | [VERIFIED] | In scope |
| 7.2.5.8.2 | Custom Colours Palette | Custom colors | Field / UI element | The set of colours already defined for the laboratory. | [VERIFIED] | In scope |
| 7.2.5.8.3 | Define Custom Colours Action | Define Custom Colors >> | Action / Function | Opens the definition of a new custom colour. | [VERIFIED] | In scope |
| 7.2.5.8.4 | Colour Confirm Action | OK | Action / Function | Applies the chosen colour to the selected element. | [VERIFIED] | In scope |
| 7.2.5.8.5 | Colour Cancel Action | Cancel | Action / Function | Leaves the chooser without applying a colour. | [VERIFIED] | In scope |
| 7.2.5.8.6 | Colour Dialog Close Action | X | Action / Function | Closes the chooser. | [VERIFIED] | In scope |
| 7.2.5.9 | Layout Change Applies Everywhere Rule | أي تعديل في هذه النافذة سيظهر في جميع تقارير النتائج | Rule / Rule | Any change made in the head and foot dialog appears in all the result reports. | [VERIFIED] | In scope |
| 7.2.5.10 | Colour Change Rule | — | Rule / Rule | The element whose appearance is to be changed is selected, the colour chooser appears, the wanted colour is chosen and the confirm action is pressed, and the appearance of the element then changes. | [VERIFIED] | In scope |
| **7.3** | **Dedicated Printers** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 7.3.1 | Dedicated Printer Selector Block | — | Block / Container | The printers that may be dedicated to a single kind of output; the printers of the reports, the barcode, the receipt, the envelope and the card. | [VERIFIED] | In scope |
| 7.3.1.1 | Default Printer Selector | تحديد الطابعة المخصصة لطباعة الإفتراضية | Field / UI element | The printer dedicated to the default printing. | [VERIFIED] | In scope |
| 7.3.1.2 | Barcode Printer Selector | تحديد الطابعة المخصصة لطباعة الباركود | Field / UI element | The printer dedicated to the printing of the barcode. | [VERIFIED] | In scope |
| 7.3.1.3 | Envelope Printer Selector | تحديد الطابعة المخصصة لطباعة الظرف | Field / UI element | The printer dedicated to the printing of the envelope. | [VERIFIED] | In scope |
| 7.3.1.4 | Receipt Printer Selector | تحديد الطابعة المخصصة لطباعة الإيصال | Field / UI element | The printer dedicated to the printing of the receipt. | [VERIFIED] | In scope |
| 7.3.1.5 | Card Printer Selector | تحديد الطابعة المخصصة لطباعة الكارنية | Field / UI element | The printer dedicated to the printing of the card. | [VERIFIED] | In scope |
| 7.3.2 | Dedicated Printer Rule | — | Rule / Rule | The printer used in printing each of the reports, the barcode, the receipt, the envelope and the card is chosen in the system settings. | [VERIFIED] | In scope |
| **7.4** | **Default Account Type** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 7.4.1 | Default Account Type Selector | نوع الحساب الافتراضي للنظام | Field / UI element | The default account type of the system. It is the account the program treats the patient on by itself, with regard to the prices of the tests, the discounts, the commissions and the samples withdrawn outside the laboratory. It is chosen in the system settings. | [VERIFIED] | In scope |
| 7.4.1.1 | Default Account Type Individual Option | Individual | Option / Subfunction | The patient is treated on an individual account. | [VERIFIED] | In scope |
| 7.4.1.2 | Default Account Type Lab To Lab Option | Lab To Lab | Option / Subfunction | The patient is treated on an account between laboratories. | [VERIFIED] | In scope |
| 7.4.1.3 | Default Account Type Contracts Option | Contracts | Option / Subfunction | The patient is treated on a contract account. | [VERIFIED] | In scope |
| 7.4.1.4 | Default Account Type Free Option | Free | Option / Subfunction | The patient is treated on a free account. | [VERIFIED] | In scope |
| 7.4.2 | Default Account Type Rule | — | Rule / Rule | The default account type is reached by pressing the settings ribbon item, then the system settings item, and choosing the default account type of the system. | [VERIFIED] | In scope |
| **7.5** | **Receipt Settings** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 7.5.1 | Receipt Settings Tab | اعدادات الإيصال | Screen / Container | The tab of the system settings window that holds the settings of the receipt. | [VERIFIED] | In scope |
| 7.5.1.1 | Receipt Currency Block | — | Block / Container | The currency the receipt is written in. | [VERIFIED] | In scope |
| 7.5.1.1.1 | Receipt Currency Local Option | تميز العملة المستخدمة: جنيهاً لا غير | Option / Subfunction | The currency of the receipt is the local currency alone; the example given beside it is that currency. | [VERIFIED] | In scope |
| 7.5.1.1.2 | Receipt Currency Abbreviated Option | تميز العملة المستخدمة: L.E. | Option / Subfunction | The currency of the receipt is written in the abbreviated form; the examples given beside it are the abbreviated forms of the local currency and of the pound. | [VERIFIED] | In scope |
| 7.5.1.2 | Result Delivery Time Field | وقت استلام النتيجة | Field / UI element | The hour at which the result is received; the example given beside it is a quarter past nine in the evening. | [VERIFIED] | In scope |
| 7.5.1.3 | Receipt Top Margin Field | الهامش العلوي للإيصال | Field / UI element | The top margin of the receipt, in centimetres; the value shown is 0. | [VERIFIED] | In scope |
| 7.5.1.4 | Print Receipt Once Option | طباعة إيصال المريض مرة واحدة فقط | Option / Subfunction | The patient's receipt is printed once only. | [VERIFIED] | In scope |
| 7.5.1.5 | Print Delivery Data Option | طباعة بيانات الاستلام من موقع نتيجة على الإنترنت | Option / Subfunction | The details of the reception are printed on the receipt, taken from the result-delivery website. | [VERIFIED] | In scope |
| 7.5.1.6 | Receipt Test Details Group | — | Field group / Container | What the receipt shows about the tests. | [VERIFIED] | In scope |
| 7.5.1.6.1 | Hide Test Details Option | عدم عرض اسماء وتفاصيل التحاليل في الوصل | Option / Subfunction | The names and the details of the tests are not shown on the receipt. | [VERIFIED] | In scope |
| 7.5.1.6.2 | Show Test Details Option | عرض اسماء وتفاصيل التحاليل في الوصل | Option / Subfunction | The names and the details of the tests are shown on the receipt. | [VERIFIED] | In scope |
| 7.5.1.6.3 | Show Test Details With Code Option | عرض اسماء وتفاصيل التحاليل في الوصل مع كود التحليل | Option / Subfunction | The names and the details of the tests are shown on the receipt together with the code of the test. | [VERIFIED] | In scope |
| 7.5.1.6.4 | Cash Printer Option | استخدام طابعة الكاشير اسماء التحاليل والاجمالي | Option / Subfunction | The cash printer is used, with the names of the tests and the total. | [VERIFIED] | In scope |
| 7.5.1.7 | Receipt Text Block | — | Block / Container | The words printed in the head and the foot of the receipt when it is printed as separate words. | [VERIFIED] | In scope |
| 7.5.1.7.1 | Receipt Right Header Text Area | كلمات الجهة يمني من رأس الإيصال | Field / UI element | The words of the right-hand side of the head of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.2 | Receipt Right Header Font Selector | Font | Field group / Container | The font presets offered for the right-hand side of the head of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.2.1 | Receipt Right Header Font Preset One | Font: 0 | Option / Subfunction | One of the numbered font presets of the right-hand side of the head of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.2.2 | Receipt Right Header Font Preset Two | Font: 0.8 | Option / Subfunction | One of the numbered font presets of the right-hand side of the head of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.3 | Receipt Middle Text Area | كلمات منتصف الإيصال من أسفل | Field / UI element | The words of the middle of the receipt, taken from below. | [VERIFIED] | In scope |
| 7.5.1.7.4 | Receipt Middle Font Selector | Font | Field group / Container | The font presets offered for the middle of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.4.1 | Receipt Middle Font Preset One | Font: 0 | Option / Subfunction | One of the numbered font presets of the middle of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.4.2 | Receipt Middle Font Preset Two | Font: 0.8 | Option / Subfunction | One of the numbered font presets of the middle of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.5 | Receipt Height Field | الارتفاع | Field / UI element | The height of the printed area of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.6 | Receipt Text Font Type Selector | حجم ونوع الخط | Field group / Container | The size and the kind of the font of the printed words of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.6.1 | Receipt Text Font Preset One | Font: 0 | Option / Subfunction | One of the numbered font presets of the printed words of the receipt. | [VERIFIED] | In scope |
| 7.5.1.7.6.2 | Receipt Text Font Preset Two | Font: 0.8 | Option / Subfunction | One of the numbered font presets of the printed words of the receipt. | [VERIFIED] | In scope |
| 7.5.1.8 | Receipt Image Block | — | Block / Container | The images printed in the head and the foot of the receipt when it is printed from images. | [VERIFIED] | In scope |
| 7.5.1.8.1 | Receipt Header Image Field | JPG Image 460 * 60 Pixels | Field / UI element | The image of the head of the receipt, shown at 460 by 60 pixels. | [VERIFIED] | In scope |
| 7.5.1.8.2 | Receipt Header Image Browse Action | ? | Action / Function | Chooses the image of the head of the receipt from the computer. | [VERIFIED] | In scope |
| 7.5.1.8.3 | Receipt Header Image Clear Action | X | Action / Function | Removes the image of the head of the receipt. | [VERIFIED] | In scope |
| 7.5.1.8.4 | Receipt Lab Logo Image Field | JPG Image 145 * 95 Pixels | Field / UI element | The laboratory's logo printed on the receipt, shown at 145 by 95 pixels. | [VERIFIED] | In scope |
| 7.5.1.8.5 | Receipt Lab Logo Browse Action | ? | Action / Function | Chooses the laboratory's logo from the computer. | [VERIFIED] | In scope |
| 7.5.1.8.6 | Receipt Lab Logo Clear Action | X | Action / Function | Removes the laboratory's logo from the receipt. | [VERIFIED] | In scope |
| 7.5.1.8.7 | Receipt Footer Image Field | JPG Image 720 * 50 Pixels | Field / UI element | The image of the foot of the receipt, shown at 720 by 50 pixels. | [VERIFIED] | In scope |
| 7.5.1.8.8 | Receipt Footer Image Browse Action | ? | Action / Function | Chooses the image of the foot of the receipt from the computer. | [VERIFIED] | In scope |
| 7.5.1.8.9 | Receipt Footer Image Clear Action | X | Action / Function | Removes the image of the foot of the receipt. | [VERIFIED] | In scope |
| 7.5.1.9 | Receipt Settings Note Block | — | Block / Container | The two notes printed at the foot of the receipt settings tab. | [VERIFIED] | In scope |
| 7.5.1.9.1 | Receipt Dimensions Note | * جميع الأبعاد المستخدمة بالسنتيميتر | Note / UI element | All the dimensions used are in centimetres. | [VERIFIED] | In scope |
| 7.5.1.9.2 | Receipt Image Size Note | * حجم ملف الصور يجب أن لا يتعدي 1 ميجا بايت | Note / UI element | The size of an image file must not exceed one megabyte. | [VERIFIED] | In scope |
| 7.5.1.10 | Receipt Settings Action Block | — | Block / Container | The actions of the receipt settings tab. | [VERIFIED] | In scope |
| 7.5.1.10.1 | Receipt Settings Save Action | حفظ الإعدادات | Action / Function | Persists the settings of the receipt tab. | [VERIFIED] | In scope |
| 7.5.1.10.2 | Receipt Settings Back Action | رجوع | Action / Subfunction | Steps back out of the receipt settings. | [VERIFIED] | In scope |
| 7.5.1.10.3 | Receipt Settings Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 7.5.2 | Receipt Settings Rule | — | Rule / Rule | The top margin of the receipt is edited where pre-printed paper is used. The currency in use and the hour at which the result is received may be changed, and the printing of the receipt once only and the showing of the reception details on the receipt are controlled. Where plain paper is used, the head and the foot of the receipt may be printed as words, and the laboratory's data is written, the kind, the size and the colour of the font and the laboratory's logo are chosen, or the image of the head and the foot of the receipt is chosen from the computer. The settings are persisted with the save action. | [VERIFIED] | In scope |
| **7.6** | **Envelope Settings** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 7.6.1 | Envelope Settings Tab | اعدادات الظرف | Screen / Container | The tab of the system settings window that holds the settings of the envelope. | [VERIFIED] | In scope |
| 7.6.1.1 | Envelope Header Footer Option Group | — | Field group / Container | The way the head and the foot of the envelope are printed. | [VERIFIED] | In scope |
| 7.6.1.1.1 | Do Not Print Envelope Header Footer Option | عدم طباعة رأس وذيل الظرف (استخدام ظرف مطبوع) | Option / Subfunction | The head and the foot of the envelope are not printed, and a pre-printed envelope is used. | [VERIFIED] | In scope |
| 7.6.1.1.2 | Print Envelope Header Footer As Words Option | طباعة رأس وذيل الظرف علي هيئة كلمات منفصلة (استخدام ظرف أبيض) | Option / Subfunction | The head and the foot of the envelope are printed as separate words, and a plain envelope is used. | [VERIFIED] | In scope |
| 7.6.1.1.3 | Print Envelope Header Footer From Images Option | طباعة رأس وذيل الظرف من ملفات الصور المرفقة في البرنامج (استخدام ظرف أبيض) | Option / Subfunction | The head and the foot of the envelope are printed from the image files held in the program, and a plain envelope is used. | [VERIFIED] | In scope |
| 7.6.1.2 | Envelope Top Margin Field | الهامش العلوى للظرف | Field / UI element | The top margin of the envelope, in centimetres; the value shown is 3. | [VERIFIED] | In scope |
| 7.6.1.3 | Envelope Text Block | — | Block / Container | The words printed in the head and the foot of the envelope when it is printed as separate words. | [VERIFIED] | In scope |
| 7.6.1.3.1 | Envelope Right Header Text Area | — | Field / UI element | The words of the right-hand side of the head of the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2 | Envelope Font Selector | Font | Field group / Container | The numbered font presets offered for the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.1 | Envelope Font Preset One | Font: 0 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.2 | Envelope Font Preset Two | Font: 1 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.3 | Envelope Font Preset Three | Font: 2 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.4 | Envelope Font Preset Four | Font: 3 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.5 | Envelope Font Preset Five | Font: 4 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.6 | Envelope Font Preset Six | Font: 5 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.7 | Envelope Font Preset Seven | Font: 6 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.8 | Envelope Font Preset Eight | Font: 7 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.9 | Envelope Font Preset Nine | Font: 8 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.2.10 | Envelope Font Preset Ten | Font: 9 | Option / Subfunction | One of the numbered font presets of the words printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.3.3 | Envelope Height Field | الارتفاع | Field / UI element | The height of the printed area of the envelope. | [VERIFIED] | In scope |
| 7.6.1.4 | Envelope Image Block | — | Block / Container | The images printed in the head and the foot of the envelope. | [VERIFIED] | In scope |
| 7.6.1.4.1 | Envelope Header Image Field | JPG Image 720 * 140 Pixels | Field / UI element | The full image of the head of the envelope, shown at 720 by 140 pixels. | [VERIFIED] | In scope |
| 7.6.1.4.2 | Envelope Header Image Browse Action | ? | Action / Function | Chooses the full image of the head of the envelope from the computer. | [VERIFIED] | In scope |
| 7.6.1.4.3 | Envelope Header Image Clear Action | X | Action / Function | Removes the full image of the head of the envelope. | [VERIFIED] | In scope |
| 7.6.1.4.4 | Envelope Lab Logo Image Field | JPG Image 145 * 95 Pixels | Field / UI element | The laboratory's logo printed on the envelope, shown at 145 by 95 pixels. | [VERIFIED] | In scope |
| 7.6.1.4.5 | Envelope Lab Logo Browse Action | ? | Action / Function | Chooses the laboratory's logo from the computer. | [VERIFIED] | In scope |
| 7.6.1.4.6 | Envelope Lab Logo Clear Action | X | Action / Function | Removes the laboratory's logo from the envelope. | [VERIFIED] | In scope |
| 7.6.1.4.7 | Envelope Footer Image Field | JPG Image 720 * 50 Pixels | Field / UI element | The full image of the foot of the envelope, shown at 720 by 50 pixels. | [VERIFIED] | In scope |
| 7.6.1.4.8 | Envelope Footer Image Browse Action | ? | Action / Function | Chooses the full image of the foot of the envelope from the computer. | [VERIFIED] | In scope |
| 7.6.1.4.9 | Envelope Footer Image Clear Action | X | Action / Function | Removes the full image of the foot of the envelope. | [VERIFIED] | In scope |
| 7.6.1.5 | Envelope Data Alignment Block | — | Block / Container | The placement of the patient's data on the envelope. | [VERIFIED] | In scope |
| 7.6.1.5.1 | Envelope Data Alignment Grid | — | Grid / UI element | The grid that places each piece of the patient's data on the envelope; each row is one item and carries its own left and top. | [VERIFIED] | In scope |
| 7.6.1.5.1.1 | Envelope Alignment Left Column | Left | Grid column / UI element | The distance of the item from the left of the envelope, in centimetres. | [VERIFIED] | In scope |
| 7.6.1.5.1.2 | Envelope Alignment Top Column | Top | Grid column / UI element | The distance of the item from the top of the envelope, in centimetres. | [VERIFIED] | In scope |
| 7.6.1.5.1.3 | Envelope Patient Name Row | الاسم | Grid row / UI element | The patient's name as it is printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.5.1.4 | Envelope Referring Entity Row | جهة الإحالة: اسم الطبيب او جهة الاحالة | Grid row / UI element | The referring entity, being the doctor's name or the name of the referring entity, as it is printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.5.1.5 | Envelope Patient Code Row | الكود | Grid row / UI element | The patient's code as it is printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.5.1.6 | Envelope Date Row | التاريخ | Grid row / UI element | The date as it is printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.5.2 | Envelope Data Titles Option | عدم طباعة عناوين بيانات المريض علي الظرف | Option / Subfunction | The titles of the patient's data are not printed on the envelope. | [VERIFIED] | In scope |
| 7.6.1.6 | Envelope Settings Action Block | — | Block / Container | The actions of the envelope settings tab. | [VERIFIED] | In scope |
| 7.6.1.6.1 | Envelope Settings Save Action | حفظ الإعدادات | Action / Function | Persists the settings of the envelope tab. | [VERIFIED] | In scope |
| 7.6.1.6.2 | Envelope Settings Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 7.6.2 | Envelope Settings Rule | — | Rule / Rule | The envelope settings are reached by pressing the settings ribbon item, then the system settings item, and choosing the envelope tab. | [VERIFIED] | In scope |
| **7.7** | **Receipt and Envelope Document Details** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 7.7.1 | Receipt Laboratory Data Lines | — | Field / UI element | Ten lines are kept on the receipt for the data of the laboratory and of its workers, and are printed when the receipt is produced on the cash printer. | [VERIFIED] | In scope |
| 7.7.2 | Receipt Results Password Field | Results PW | Field / UI element | The password that gives access to the result is printed on the receipt. | [VERIFIED] | In scope |
| 7.7.3 | Receipt Print Once Rule | — | Rule / Rule | The program may be set so that the receipt of a patient is printed once only. | [VERIFIED] | In scope |

### §12.3 Workflows

### Reach the settings of a single output

1. Press the settings ribbon item. → 7.1.1
2. Press the system settings item; the system settings window opens with its tab strip. → 7.1.2, 7.1.4
3. Choose the tab of the output to be set: the report tab, the receipt tab, the envelope tab, the card tab, or the database server tab. → 7.1.4.1
4. Edit the fields of that tab. → 7.2.1, 7.5.1, 7.6.1
5. Press the save action; the settings of the tab are persisted. → 7.1.4.5.1

### Set the margins of the report

1. Open the system settings window and choose the report tab. → 7.1.4, 7.2.1
2. Edit the left margin, the bottom margin and the report top space, to a maximum of eight centimetres for the top space. → 7.2.1.2, 7.2.2
3. Press the save action. → 7.2.1.9.1

### Choose the paper the report is printed on

1. Open the system settings window and choose the report tab. → 7.2.1
2. Choose A4 or A5 in the paper size field. → 7.2.1.3, 7.2.3
3. Press the save action. → 7.2.1.9.1

### Choose how the head and the foot of the report are printed

1. Open the system settings window and choose the report tab. → 7.2.1
2. Choose one of the three ways: not printed with pre-printed paper, printed as separate words with plain paper, or printed from the image files held in the program. → 7.2.1.5
3. Enter the laboratory's data in the text areas and choose the font preset of each of them. → 7.2.1.7
4. Where images are used, choose the image of the laboratory's logo, of the full head and of the full foot from the computer. → 7.2.1.6
5. Press the save action. → 7.2.1.9.1

### Place the elements of the head and the foot of the report

1. In the report tab, press the action that changes the head and the foot of the report. → 7.2.1.9.2
2. The head and foot dialog opens, carrying the grid of the elements of the top and the bottom line. → 7.2.5
3. Show or hide the top line and the bottom line, and choose the long form of the date and the showing of the time. → 7.2.5.1, 7.2.5.2, 7.2.5.3, 7.2.5.4
4. For each element, set its width, its height, its distance from the top and its distance from the left. → 7.2.5.5
5. Set the format of the table of the patient's earlier tests: the bold mark and the height of its rows. → 7.2.5.6
6. Select the element whose appearance is to be changed; the colour chooser opens, and the wanted colour is chosen and confirmed. → 7.2.5.8, 7.2.5.10
7. Press the save action of the dialog. → 7.2.5.7.3
8. Where the placement is to be reset, press the restore default action. → 7.2.5.7.2

### Dedicate a printer to an output

1. Open the system settings window. → 7.1.4
2. In the dedicated printer block, choose the printer for the default printing, for the barcode, for the envelope, for the receipt and for the card. → 7.3.1, 7.3.2
3. Press the save action. → 7.1.4.5.1

### Set the default account type of the system

1. Press the settings ribbon item, then the system settings item. → 7.1.1, 7.1.2
2. Choose the default account type of the system: an individual account, an account between laboratories, a contract account or a free account. → 7.4.1, 7.4.2
3. Press the save action. → 7.1.4.5.1

### Set the receipt

1. Open the system settings window and choose the receipt tab. → 7.5.1
2. Choose the currency of the receipt, the hour at which the result is received, and the top margin of the receipt. → 7.5.1.1, 7.5.1.2, 7.5.1.3
3. Choose whether the receipt is printed once only, whether the reception details are printed on it, and what it shows about the tests. → 7.5.1.4, 7.5.1.5, 7.5.1.6
4. Where the head and the foot are printed as words, write the laboratory's data and choose the font preset of each of the areas, and set the height. → 7.5.1.7
5. Where images are used, choose the image of the head of the receipt, the laboratory's logo and the image of the foot of the receipt from the computer. → 7.5.1.8
6. Press the save action. → 7.5.1.10.1

### Set the envelope

1. Open the system settings window and choose the envelope tab. → 7.6.1
2. Choose the way the head and the foot of the envelope are printed, and set its top margin. → 7.6.1.1, 7.6.1.2
3. Write the words printed on the envelope and choose the font preset of each of them, and set the height. → 7.6.1.3
4. Where images are used, choose the image of the head of the envelope, the laboratory's logo and the image of the foot of the envelope from the computer. → 7.6.1.4
5. Place the patient's data on the envelope by setting the left and the top of each of the four items, and choose whether the titles of the data are printed. → 7.6.1.5
6. Press the save action. → 7.6.1.6.1

### §12.4 Rules, validations and conditional behaviour

| ID | Rule | Tag |
|---|---|---|
| 7.1.4.3.14 | Where the daily backup is enabled and the path does not exist, the backup is written to the fallback folder under the data folder. | [VERIFIED] |
| 7.2.1.5.1 | The head and the foot of the report are not printed where pre-printed paper is used. | [VERIFIED] |
| 7.2.1.5.2 | The head and the foot of the report are printed as separate words where plain paper is used. | [VERIFIED] |
| 7.2.1.5.3 | The head and the foot of the report are printed from the image files held in the program where plain paper is used. | [VERIFIED] |
| 7.2.1.7.8 | The font presets of the printed words are numbered and each preset carries the size used for its line. | [VERIFIED] |
| 7.2.1.8.2 | The size of an image file must not exceed one megabyte. | [VERIFIED] |
| 7.2.2 | The top space of the report is raised or lowered to a maximum of eight centimetres. | [VERIFIED] |
| 7.2.3 | The paper size of the report is either A4 or A5. | [VERIFIED] |
| 7.2.5.9 | Any change made in the head and foot dialog appears in all the result reports. | [VERIFIED] |
| 7.2.5.10 | The colour chooser opens when the element whose appearance is to be changed is selected, and the change is applied when the confirm action is pressed. | [VERIFIED] |
| 7.3.2 | The printer of the reports, of the barcode, of the receipt, of the envelope and of the card are chosen in the system settings. | [VERIFIED] |
| 7.4.1 | The default account type governs the prices of the tests, the discounts, the commissions and the samples withdrawn outside the laboratory. | [VERIFIED] |
| 7.5.1.1.1 | The currency of the receipt may be the local currency alone. | [VERIFIED] |
| 7.5.1.1.2 | The currency of the receipt may be written in the abbreviated form. | [VERIFIED] |
| 7.5.1.6.4 | The cash printer is used with the names of the tests and the total. | [VERIFIED] |
| 7.6.1.1.1 | The head and the foot of the envelope are not printed where a pre-printed envelope is used. | [VERIFIED] |
| 7.6.1.1.2 | The head and the foot of the envelope are printed as separate words where a plain envelope is used. | [VERIFIED] |
| 7.6.1.1.3 | The head and the foot of the envelope are printed from the image files held in the program where a plain envelope is used. | [VERIFIED] |

### §12.5 Outputs

The artefacts below are produced by the elements listed. The content of each artefact is defined once, in the element's own definition, and is not repeated here.

| ID | Output | Tag |
|---|---|---|
| 7.2.5 | The placed head and foot of the report, which is applied to all the result reports | [VERIFIED] |
| 7.2.5.6 | The formatted table of the patient's earlier tests as it is printed inside the report | [VERIFIED] |
| 7.1.4.3.12 | The daily backup of the database | [VERIFIED] |

### §12.6 Screens and interface structure

| Screen, tab or output | Element identifiers |
|---|---|
| System Settings Screen | 7.1.4, 7.1.4.1, 7.1.4.1.1, 7.1.4.1.2, 7.1.4.1.3, 7.1.4.1.4, 7.1.4.1.5, 7.1.4.1.6, 7.1.4.2, 7.1.4.2.1, 7.1.4.2.2, 7.1.4.2.3, 7.1.4.2.4, 7.1.4.2.5, 7.1.4.3, 7.1.4.3.1, 7.1.4.3.2, 7.1.4.3.3, 7.1.4.3.4, 7.1.4.3.5, 7.1.4.3.6, 7.1.4.3.7, 7.1.4.3.8, 7.1.4.3.9, 7.1.4.3.10, 7.1.4.3.11, 7.1.4.3.12, 7.1.4.3.13, 7.1.4.3.14, 7.1.4.4, 7.1.4.4.1, 7.1.4.4.2, 7.1.4.4.3, 7.1.4.5, 7.1.4.5.1, 7.1.4.5.2, 7.1.4.5.3 |
| Report Settings Tab | 7.2.1, 7.2.1.1, 7.2.1.1.1, 7.2.1.1.2, 7.2.1.1.3, 7.2.1.1.3.1, 7.2.1.1.3.2, 7.2.1.1.3.3, 7.2.1.2, 7.2.1.2.1, 7.2.1.2.2, 7.2.1.2.3, 7.2.1.3, 7.2.1.3.1, 7.2.1.3.2, 7.2.1.4, 7.2.1.4.1, 7.2.1.4.2, 7.2.1.5, 7.2.1.5.1, 7.2.1.5.2, 7.2.1.5.3, 7.2.1.6, 7.2.1.6.1, 7.2.1.6.2, 7.2.1.6.3, 7.2.1.6.4, 7.2.1.6.5, 7.2.1.6.6, 7.2.1.6.7, 7.2.1.6.8, 7.2.1.6.9, 7.2.1.7, 7.2.1.7.1, 7.2.1.7.2, 7.2.1.7.2.1, 7.2.1.7.2.2, 7.2.1.7.2.3, 7.2.1.7.2.4, 7.2.1.7.3, 7.2.1.7.4, 7.2.1.7.4.1, 7.2.1.7.4.2, 7.2.1.7.4.3, 7.2.1.7.4.4, 7.2.1.7.5, 7.2.1.7.6, 7.2.1.7.6.1, 7.2.1.7.6.2, 7.2.1.7.6.3, 7.2.1.7.6.4, 7.2.1.7.7, 7.2.1.7.8, 7.2.1.7.8.1, 7.2.1.7.8.2, 7.2.1.7.8.3, 7.2.1.7.8.4, 7.2.1.8, 7.2.1.8.1, 7.2.1.8.2, 7.2.1.9, 7.2.1.9.1, 7.2.1.9.2, 7.2.1.9.3, 7.2.1.9.4 |
| Header Footer Layout Dialog | 7.2.5, 7.2.5.1, 7.2.5.2, 7.2.5.3, 7.2.5.4, 7.2.5.5, 7.2.5.5.1, 7.2.5.5.2, 7.2.5.5.3, 7.2.5.5.4, 7.2.5.5.5, 7.2.5.5.6, 7.2.5.5.7, 7.2.5.5.8, 7.2.5.5.9, 7.2.5.5.10, 7.2.5.5.11, 7.2.5.5.12, 7.2.5.6, 7.2.5.6.1, 7.2.5.6.2, 7.2.5.6.3, 7.2.5.6.4, 7.2.5.6.5, 7.2.5.6.6, 7.2.5.7, 7.2.5.7.1, 7.2.5.7.2, 7.2.5.7.3, 7.2.5.8, 7.2.5.8.1, 7.2.5.8.2, 7.2.5.8.3, 7.2.5.8.4, 7.2.5.8.5, 7.2.5.8.6, 7.2.5.9, 7.2.5.10 |
| Dedicated Printer Selector Block | 7.3.1, 7.3.1.1, 7.3.1.2, 7.3.1.3, 7.3.1.4, 7.3.1.5 |
| Default Account Type Selector | 7.4.1, 7.4.1.1, 7.4.1.2, 7.4.1.3, 7.4.1.4 |
| Receipt Settings Tab | 7.5.1, 7.5.1.1, 7.5.1.1.1, 7.5.1.1.2, 7.5.1.2, 7.5.1.3, 7.5.1.4, 7.5.1.5, 7.5.1.6, 7.5.1.6.1, 7.5.1.6.2, 7.5.1.6.3, 7.5.1.6.4, 7.5.1.7, 7.5.1.7.1, 7.5.1.7.2, 7.5.1.7.2.1, 7.5.1.7.2.2, 7.5.1.7.3, 7.5.1.7.4, 7.5.1.7.4.1, 7.5.1.7.4.2, 7.5.1.7.5, 7.5.1.7.6, 7.5.1.7.6.1, 7.5.1.7.6.2, 7.5.1.8, 7.5.1.8.1, 7.5.1.8.2, 7.5.1.8.3, 7.5.1.8.4, 7.5.1.8.5, 7.5.1.8.6, 7.5.1.8.7, 7.5.1.8.8, 7.5.1.8.9, 7.5.1.9, 7.5.1.9.1, 7.5.1.9.2, 7.5.1.10, 7.5.1.10.1, 7.5.1.10.2, 7.5.1.10.3 |
| Envelope Settings Tab | 7.6.1, 7.6.1.1, 7.6.1.1.1, 7.6.1.1.2, 7.6.1.1.3, 7.6.1.2, 7.6.1.3, 7.6.1.3.1, 7.6.1.3.2, 7.6.1.3.2.1, 7.6.1.3.2.2, 7.6.1.3.2.3, 7.6.1.3.2.4, 7.6.1.3.2.5, 7.6.1.3.2.6, 7.6.1.3.2.7, 7.6.1.3.2.8, 7.6.1.3.2.9, 7.6.1.3.2.10, 7.6.1.3.3, 7.6.1.4, 7.6.1.4.1, 7.6.1.4.2, 7.6.1.4.3, 7.6.1.4.4, 7.6.1.4.5, 7.6.1.4.6, 7.6.1.4.7, 7.6.1.4.8, 7.6.1.4.9, 7.6.1.5, 7.6.1.5.1, 7.6.1.5.1.1, 7.6.1.5.1.2, 7.6.1.5.1.3, 7.6.1.5.1.4, 7.6.1.5.1.5, 7.6.1.5.1.6, 7.6.1.5.2, 7.6.1.6, 7.6.1.6.1, 7.6.1.6.2 |

### §12.7 Uncertainties and unshown elements

| ID | Item | Tag |
|---|---|---|
| 7.1.4.1.5 | The card tab is named in the tab strip but its fields are not shown. | [UNVERIFIED] |
| 7.1.4.3.11 | The option that prints the file number or the card number instead of the print date is shown in the later rendering of the tab only; the field that carries it is not legible. | [UNVERIFIED] |
| 7.2.1.1.3.1 | The three font presets of the signature are not numbered in the source, and the property each of them controls is not established. | [UNVERIFIED] |
| 7.2.1.7.2 | The size that belongs to each numbered font preset is not legible where the presets and their sizes are printed together. | [UNVERIFIED] |
| 7.5.1.1.2 | The second form of the currency is shown with two examples; the source does not establish which of the two belongs to which kind of receipt. | [UNVERIFIED] |
| 7.6.1.3.1 | The words printed in the head of the envelope are shown in the source as a sample of one laboratory; the label of the field is not legible. | [UNVERIFIED] |
| 7.1.4.3 | No validation, rejection message or recovery path is established for the system settings window. | [NOT-SHOWN] |
| 7.1.4.1.5 | The card tab is named in the tab strip but its screen is never displayed. | [NOT-SHOWN] |

## §13 Unit 8 — Laboratory Sample Withdrawal

### §13.1 Functional hierarchy

```
8  Laboratory Sample Withdrawal
├── 8.1  Laboratory Module Entry
│   ├── 8.1.1  Laboratory Ribbon Item
│   ├── 8.1.2  Laboratory Window Capabilities
│   │   ├── 8.1.2.1  Sample Registration And Coding Capability
│   │   └── 8.1.2.2  Sample Withdrawal Capability
│   └── 8.1.3  Sample Withdrawal Item
└── 8.2  Sample Withdrawal Screen and Withdrawal Rules
    ├── 8.2.1  Sample Withdrawal Screen
    │   ├── 8.2.1.1  Daily Patient Count Block
    │   │   └── 8.2.1.1.1  Today Patient Count Field
    │   ├── 8.2.1.2  Patient List Block
    │   │   ├── 8.2.1.2.1  Patient List Grid
    │   │   │   ├── 8.2.1.2.1.1  Patient List Name Column
    │   │   │   ├── 8.2.1.2.1.2  Patient List Patient Id Column
    │   │   │   ├── 8.2.1.2.1.3  Patient List Lab Id Column
    │   │   │   ├── 8.2.1.2.1.4  Patient List Doctor Column
    │   │   │   ├── 8.2.1.2.1.5  Patient List Sex Column
    │   │   │   └── 8.2.1.2.1.6  Patient List Age Column
    │   │   └── 8.2.1.2.2  Patient List Row
    │   ├── 8.2.1.3  Patient Identification Block
    │   │   ├── 8.2.1.3.1  Selected Patient Name Field
    │   │   ├── 8.2.1.3.2  Selected Patient Id Field
    │   │   ├── 8.2.1.3.3  Selected Patient Lab Id Field
    │   │   ├── 8.2.1.3.4  Selected Patient Doctor Field
    │   │   ├── 8.2.1.3.5  Selected Patient Sex Field
    │   │   └── 8.2.1.3.6  Selected Patient Age Field
    │   ├── 8.2.1.4  Samples To Draw Grid
    │   │   └── 8.2.1.4.1  Sample To Draw Row
    │   ├── 8.2.1.5  Other Tube Control
    │   ├── 8.2.1.6  Withdrawn Samples Grid
    │   │   ├── 8.2.1.6.1  Withdrawn Tube Name Column
    │   │   ├── 8.2.1.6.2  Withdrawn Tests Column
    │   │   ├── 8.2.1.6.3  Withdrawn Take Flag Column
    │   │   └── 8.2.1.6.4  Withdrawn Sample Row
    │   ├── 8.2.1.7  Sample Action Block
    │   │   ├── 8.2.1.7.1  Take Action
    │   │   ├── 8.2.1.7.2  Delete Action
    │   │   ├── 8.2.1.7.3  Refresh Sample List Action
    │   │   └── 8.2.1.7.4  Collapse Control
    │   ├── 8.2.1.8  Save Action Block
    │   │   ├── 8.2.1.8.1  Save Patient Samples Action
    │   │   └── 8.2.1.8.2  Save Samples After Read Id Action
    │   ├── 8.2.1.9  Print Action Block
    │   │   ├── 8.2.1.9.1  Print Barcode After Read Id Action
    │   │   ├── 8.2.1.9.2  Print All Stickers Action
    │   │   ├── 8.2.1.9.3  Print Serum Plasma Blood Action
    │   │   └── 8.2.1.9.4  Print Semen Urine Stool Action
    │   ├── 8.2.1.10  Date Picker Block
    │   │   ├── 8.2.1.10.1  Month Navigation Back Action
    │   │   ├── 8.2.1.10.2  Month Label
    │   │   ├── 8.2.1.10.3  Month Navigation Forward Action
    │   │   ├── 8.2.1.10.4  Weekday Header Row
    │   │   ├── 8.2.1.10.5  Day Grid
    │   │   │   └── 8.2.1.10.5.1  Day Cell
    │   │   └── 8.2.1.10.6  Selected Date Indicator
    │   ├── 8.2.1.11  Notes Block
    │   │   ├── 8.2.1.11.1  Notes Label
    │   │   └── 8.2.1.11.2  Sample Day Notes Field
    │   └── 8.2.1.12  Screen Action Block
    │       └── 8.2.1.12.1  Main List Action
    ├── 8.2.2  Patient Selection Rule
    ├── 8.2.3  Sample Taking Rule
    ├── 8.2.4  Withdrawn Sample Listing Rule
    ├── 8.2.5  Tube Designation Correspondence Rule
    └── 8.2.6  Sticker Printing Rule
```

### §13.2 Function inventory

The elements of the unit, each defined once, at the place that carries it. The *Type / Level* column gives the kind of the element and the level at which it is recorded under the policy of §3. The *Scope* column marks the elements that belong to one of the five excluded categories of §2; they are documented, and they are not parity requirements.

| ID | Canonical name | Source label | Type / Level | Definition | Evidence | Scope |
|---|---|---|---|---|---|---|
| **8.1** | **Laboratory Module Entry** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 8.1.1 | Laboratory Ribbon Item | Laboratory | Navigation / Subfunction | The ribbon item that opens the laboratory module, in which the samples of the patients are registered, coded and separated, and in which the samples of each department are coded separately. | [VERIFIED] | In scope |
| 8.1.2 | Laboratory Window Capabilities | — | Block / Container | What the laboratory window allows. | [VERIFIED] | In scope |
| 8.1.2.1 | Sample Registration And Coding Capability | تسجيل وتكويد عينات المرضي وفصلها وتكويد عينات كل قسم على حدي | Function / Function | The patient's samples are registered and coded and are separated, and the samples of each department are coded separately. | [VERIFIED] | In scope |
| 8.1.2.2 | Sample Withdrawal Capability | سحب وفصل العينات | Function / Function | The samples are withdrawn and separated. | [VERIFIED] | In scope |
| 8.1.3 | Sample Withdrawal Item | سحب وفصل العينات | Menu item / Subfunction | Opens the sample withdrawal screen. | [VERIFIED] | In scope |
| **8.2** | **Sample Withdrawal Screen and Withdrawal Rules** | — | Block / Container | The functional group. | [VERIFIED] | In scope |
| 8.2.1 | Sample Withdrawal Screen | نافذة سحب وفصل العينات | Screen / Container | Through this window the samples of the patients are drawn and separated, and the samples that were drawn are told apart from the samples that were not. | [VERIFIED] | In scope |
| 8.2.1.1 | Daily Patient Count Block | — | Block / Container | The number of patients of the day. | [VERIFIED] | In scope |
| 8.2.1.1.1 | Today Patient Count Field | عدد المرضى اليوم | Field / UI element | The number of the patients of the day. | [VERIFIED] | In scope |
| 8.2.1.2 | Patient List Block | — | Block / Container | The list of the patients from whom the samples are drawn. | [VERIFIED] | In scope |
| 8.2.1.2.1 | Patient List Grid | الاسم \ ·  P. ID \ ·  L. ID \ ·  الطبيب \ ·  النوع \ ·  العمر | Grid / UI element | The list of the patients; one row is one patient. | [VERIFIED] | In scope |
| 8.2.1.2.1.1 | Patient List Name Column | الاسم | Grid column / UI element | The patient's name. | [VERIFIED] | In scope |
| 8.2.1.2.1.2 | Patient List Patient Id Column | P. ID | Grid column / UI element | The patient's identifier. | [VERIFIED] | In scope |
| 8.2.1.2.1.3 | Patient List Lab Id Column | L. ID | Grid column / UI element | The identifier of the visit of the laboratory. | [VERIFIED] | In scope |
| 8.2.1.2.1.4 | Patient List Doctor Column | الطبيب | Grid column / UI element | The treating doctor. | [VERIFIED] | In scope |
| 8.2.1.2.1.5 | Patient List Sex Column | النوع | Grid column / UI element | The patient's sex. | [VERIFIED] | In scope |
| 8.2.1.2.1.6 | Patient List Age Column | العمر | Grid column / UI element | The patient's age. | [VERIFIED] | In scope |
| 8.2.1.2.2 | Patient List Row | — | Grid row / UI element | One patient of the list, from whom the samples are drawn. | [VERIFIED] | In scope |
| 8.2.1.3 | Patient Identification Block | — | Block / Container | The identification of the patient whose samples are being drawn. | [VERIFIED] | In scope |
| 8.2.1.3.1 | Selected Patient Name Field | الاسم | Field / UI element | The name of the selected patient. | [VERIFIED] | In scope |
| 8.2.1.3.2 | Selected Patient Id Field | P. ID | Field / UI element | The identifier of the selected patient. | [VERIFIED] | In scope |
| 8.2.1.3.3 | Selected Patient Lab Id Field | L. ID | Field / UI element | The identifier of the visit of the laboratory for the selected patient. | [VERIFIED] | In scope |
| 8.2.1.3.4 | Selected Patient Doctor Field | الطبيب | Field / UI element | The treating doctor of the selected patient. | [VERIFIED] | In scope |
| 8.2.1.3.5 | Selected Patient Sex Field | النوع | Field / UI element | The sex of the selected patient. | [VERIFIED] | In scope |
| 8.2.1.3.6 | Selected Patient Age Field | العمر | Field / UI element | The age of the selected patient, shown in years. | [VERIFIED] | In scope |
| 8.2.1.4 | Samples To Draw Grid | — | Grid / UI element | The samples to be drawn for the selected patient, for the tests to be carried out; one row is one container, carrying the tests performed on it. It carries the same two columns as the withdrawn-samples grid. | [VERIFIED] | In scope |
| 8.2.1.4.1 | Sample To Draw Row | — | Grid row / UI element | One container to be drawn from the patient, with the tests that are performed on it. | [VERIFIED] | In scope |
| 8.2.1.5 | Other Tube Control | Other Tube | Control / UI element | The control for the container that carries no test of its own. | [UNVERIFIED] | In scope |
| 8.2.1.6 | Withdrawn Samples Grid | Tube Name \ ·  Tests \ ·  Take | Grid / UI element | The samples that were drawn; one row is one sample that was drawn, carrying the container it was drawn in, the tests performed on it, and the mark that it was drawn. | [VERIFIED] | In scope |
| 8.2.1.6.1 | Withdrawn Tube Name Column | Tube Name | Grid column / UI element | The container the sample was drawn in. | [VERIFIED] | In scope |
| 8.2.1.6.2 | Withdrawn Tests Column | Tests | Grid column / UI element | The tests performed on the sample. | [VERIFIED] | In scope |
| 8.2.1.6.3 | Withdrawn Take Flag Column | Take | Grid column / UI element | The mark that the sample was drawn. | [VERIFIED] | In scope |
| 8.2.1.6.4 | Withdrawn Sample Row | — | Grid row / UI element | One sample that was drawn. | [VERIFIED] | In scope |
| 8.2.1.7 | Sample Action Block | — | Block / Container | The actions that act on the samples. | [VERIFIED] | In scope |
| 8.2.1.7.1 | Take Action | Take | Action / Function | Records the selected sample as a sample that was drawn; the sample then appears at the foot of the window. | [VERIFIED] | In scope |
| 8.2.1.7.2 | Delete Action | Delete | Action / Function | Removes the selected sample. | [VERIFIED] | In scope |
| 8.2.1.7.3 | Refresh Sample List Action | Refresh | Action / Function | Reloads the list of the samples and the list of the patients. | [VERIFIED] | In scope |
| 8.2.1.7.4 | Collapse Control | <<< | Control / UI element | The control that shows or hides the part of the window that carries the samples. | [UNVERIFIED] | In scope |
| 8.2.1.8 | Save Action Block | — | Block / Container | The actions that persist the samples. | [VERIFIED] | In scope |
| 8.2.1.8.1 | Save Patient Samples Action | Save patient samples | Action / Function | Persists the samples drawn for the patient. | [VERIFIED] | In scope |
| 8.2.1.8.2 | Save Samples After Read Id Action | Save samples after read ID | Action / Function | Persists the samples once the identifier has been read. | [VERIFIED] | In scope |
| 8.2.1.9 | Print Action Block | — | Block / Container | The actions that print the labels of the samples. | [VERIFIED] | In scope |
| 8.2.1.9.1 | Print Barcode After Read Id Action | Print barcode after read ID | Action / Function | Prints the barcode once the identifier has been read. | [VERIFIED] | In scope |
| 8.2.1.9.2 | Print All Stickers Action | Print all stickers | Action / Function | Prints all the stickers of the samples. | [VERIFIED] | In scope |
| 8.2.1.9.3 | Print Serum Plasma Blood Action | Print (Serum,Plasma,Blood..) | Action / Function | Prints the stickers of the samples drawn in the serum, the plasma and the blood containers. | [VERIFIED] | In scope |
| 8.2.1.9.4 | Print Semen Urine Stool Action | Print (Semen,Urine,Stool..) | Action / Function | Prints the stickers of the samples drawn in the semen, the urine and the stool containers. | [VERIFIED] | In scope |
| 8.2.1.10 | Date Picker Block | — | Block / Container | The calendar that carries the day the samples are drawn on. | [VERIFIED] | In scope |
| 8.2.1.10.1 | Month Navigation Back Action | ◄ | Action / Subfunction | Moves the calendar to the preceding month. | [VERIFIED] | In scope |
| 8.2.1.10.2 | Month Label | July 2016 | Field / UI element | The month the calendar shows. | [SAMPLE] | In scope |
| 8.2.1.10.3 | Month Navigation Forward Action | ► | Action / Function | Moves the calendar to the following month. | [VERIFIED] | In scope |
| 8.2.1.10.4 | Weekday Header Row | Sat \ ·  Sun \ ·  Mon \ ·  Tue \ ·  Wed \ ·  Thu \ ·  Fri | Row / UI element | The days of the week the days of the month are laid out under. | [VERIFIED] | In scope |
| 8.2.1.10.5 | Day Grid | — | Grid / UI element | The days of the month the calendar shows. | [VERIFIED] | In scope |
| 8.2.1.10.5.1 | Day Cell | — | Grid cell / UI element | One day of the month. | [VERIFIED] | In scope |
| 8.2.1.10.6 | Selected Date Indicator | Today: 21/07/2016 | Field / UI element | The day the calendar is set on, and the indication that it is the current day. | [SAMPLE] | In scope |
| 8.2.1.11 | Notes Block | — | Block / Container | The note recorded against the samples of the day. | [VERIFIED] | In scope |
| 8.2.1.11.1 | Notes Label | ملاحظات | Label / UI element | The label of the note carried at the foot of the window. | [VERIFIED] | In scope |
| 8.2.1.11.2 | Sample Day Notes Field | — | Field / UI element | The note recorded against the samples of the day. | [VERIFIED] | In scope |
| 8.2.1.12 | Screen Action Block | — | Block / Container | The actions that close the screen. | [VERIFIED] | In scope |
| 8.2.1.12.1 | Main List Action | القائمة الرئيسية | Action / Subfunction | Returns to the main list. | [VERIFIED] | In scope |
| 8.2.2 | Patient Selection Rule | — | Rule / Rule | The patient from whom the samples are drawn is chosen from the list of the patients, and the samples to be drawn for the tests appear for that patient. | [VERIFIED] | In scope |
| 8.2.3 | Sample Taking Rule | — | Rule / Rule | The sample that was drawn is pressed, and it is then recorded as a sample that was drawn and appears at the foot of the window. | [VERIFIED] | In scope |
| 8.2.4 | Withdrawn Sample Listing Rule | — | Rule / Rule | In this way the person in charge of drawing the samples finds it easy to know the samples that were drawn and the samples that were not, for each patient. | [VERIFIED] | In scope |
| 8.2.5 | Tube Designation Correspondence Rule | — | Rule / Rule | The tube names shown in the grids are the designations of the specimen containers, whose kinds and their printed content are established in the patient module, 1.14.3.1 to 1.14.3.7. Three further designations are shown here that are not among them: Serum, Citrated Plasma 0.2 and Other Tube. | [VERIFIED] | In scope |
| 8.2.6 | Sticker Printing Rule | — | Rule / Rule | The stickers of the samples are printed all together, or by the group of the material of the container; the printing of the same sticker more than once is established in the patient module, 1.14.4.4. | [VERIFIED] | In scope |

### §13.3 Workflows

### Draw the samples of a patient

1. Press the laboratory ribbon item. → 8.1.1
2. Press the sample withdrawal item; the sample withdrawal screen opens. → 8.1.3
3. The number of the patients of the day is shown, and the list of the patients is loaded. → 8.2.1.1.1, 8.2.1.2.1
4. Choose the patient whose samples are to be drawn from the list of the patients; the samples to be drawn for the tests appear for that patient, and the identification of the patient is shown. → 8.2.1.3, 8.2.2
5. Press the sample that was drawn; it is recorded as a sample that was drawn and appears at the foot of the window. → 8.2.1.7.1, 8.2.3
6. Press the save patient samples action to persist the samples drawn for the patient. → 8.2.1.8.1
7. Where the identifier is read by a device, the samples are persisted and the barcode printed with the actions that act after the identifier is read. → 8.2.1.8.2, 8.2.1.9.1
8. Print the stickers, all of them together or by the group of the material of the container. → 8.2.1.9.2, 8.2.1.9.3, 8.2.1.9.4, 8.2.6
9. Press the main list action to return to the main list. → 8.2.1.12.1

### Tell the drawn samples from the samples not drawn

1. Draw the samples of the patient and record each of them with the take action. → 8.2.1.7.1
2. The samples that were drawn are listed at the foot of the window, each with the container it was drawn in and the tests performed on it. → 8.2.1.6, 8.2.4
3. Where a sample was entered by mistake, select it and press the delete action. → 8.2.1.7.2
4. Press the refresh action to bring the list of the samples and the list of the patients up to date. → 8.2.1.7.3

### §13.4 Rules, validations and conditional behaviour

| ID | Rule | Tag |
|---|---|---|
| 8.2.1.1.1 | The number of the patients of the day is shown at the head of the list of the patients. | [VERIFIED] |
| 8.2.1.7.1 | A sample is recorded as drawn when it is pressed, and it then appears at the foot of the window. | [VERIFIED] |
| 8.2.1.7.2 | The delete action acts on the selected sample of the withdrawn-samples grid. | [VERIFIED] |
| 8.2.1.7.3 | The refresh action reloads the list of the samples and the list of the patients. | [VERIFIED] |
| 8.2.1.8.1 | The samples drawn for the patient are persisted by the save patient samples action. | [VERIFIED] |
| 8.2.1.8.2 | The samples are persisted once the identifier has been read. | [VERIFIED] |
| 8.2.1.9.1 | The barcode is printed once the identifier has been read. | [VERIFIED] |
| 8.2.1.9.2 | The stickers of all the samples may be printed together. | [VERIFIED] |
| 8.2.1.9.3 | The stickers may be printed for the serum, the plasma and the blood containers. | [VERIFIED] |
| 8.2.1.9.4 | The stickers may be printed for the semen, the urine and the stool containers. | [VERIFIED] |
| 8.2.2 | The samples to be drawn for the tests appear for the patient that was chosen. | [VERIFIED] |
| 8.2.3 | The sample that was drawn is recorded as a sample that was drawn and appears at the foot of the window. | [VERIFIED] |
| 8.2.4 | The drawn samples and the samples not drawn are told apart for each patient. | [VERIFIED] |
| 8.2.5 | The tube names are the designations of the specimen containers, of which three are shown here that are not among the seven established in the patient module. | [VERIFIED] |

### §13.5 Outputs

The artefacts below are produced by the elements listed. The content of each artefact is defined once, in the element's own definition, and is not repeated here.

| ID | Output | Tag |
|---|---|---|
| 8.2.1.6 | The list of the samples that were drawn for the patient | [VERIFIED] |
| 8.2.1.9.2 | The printed stickers of the samples | [VERIFIED] |

### §13.6 Screens and interface structure

| Screen or output | Element identifiers |
|---|---|
| Sample Withdrawal Screen | 8.2.1, 8.2.1.1, 8.2.1.1.1, 8.2.1.2, 8.2.1.2.1, 8.2.1.2.1.1, 8.2.1.2.1.2, 8.2.1.2.1.3, 8.2.1.2.1.4, 8.2.1.2.1.5, 8.2.1.2.1.6, 8.2.1.2.2, 8.2.1.3, 8.2.1.3.1, 8.2.1.3.2, 8.2.1.3.3, 8.2.1.3.4, 8.2.1.3.5, 8.2.1.3.6, 8.2.1.4, 8.2.1.4.1, 8.2.1.5, 8.2.1.6, 8.2.1.6.1, 8.2.1.6.2, 8.2.1.6.3, 8.2.1.6.4, 8.2.1.7, 8.2.1.7.1, 8.2.1.7.2, 8.2.1.7.3, 8.2.1.7.4, 8.2.1.8, 8.2.1.8.1, 8.2.1.8.2, 8.2.1.9, 8.2.1.9.1, 8.2.1.9.2, 8.2.1.9.3, 8.2.1.9.4, 8.2.1.10, 8.2.1.10.1, 8.2.1.10.2, 8.2.1.10.3, 8.2.1.10.4, 8.2.1.10.5, 8.2.1.10.5.1, 8.2.1.10.6, 8.2.1.11, 8.2.1.11.1, 8.2.1.11.2, 8.2.1.12, 8.2.1.12.1 |

### §13.7 Uncertainties and unshown elements

| ID | Item | Tag |
|---|---|---|
| 8.2.1.4.1 | The column headings of the samples-to-draw grid are not printed on the page; the grid is shown carrying the same two columns as the withdrawn-samples grid. | [UNVERIFIED] |
| 8.2.1.5 | The control for the container that carries no test of its own is labelled, but its action is not established. | [UNVERIFIED] |
| 8.2.1.7.4 | The control that shows or hides the part of the window that carries the samples is not labelled, and its action is not established. | [UNVERIFIED] |
| 8.2.1.9.3 | The list of the containers of the first print group ends with an ellipsis in the source, so the whole list is not established. | [UNVERIFIED] |
| 8.2.1.9.4 | The list of the containers of the second print group ends with an ellipsis in the source, so the whole list is not established. | [UNVERIFIED] |
| 8.2.1.3.3 | The identifier of the visit of the laboratory is shown empty for the patient of the source, and the rule that fills it is not established. | [UNVERIFIED] |
| 8.2.1.3.4 | The treating doctor is shown empty for the patient of the source, and the rule that fills it is not established. | [UNVERIFIED] |
| 8.1.2.1 | The coding of the samples of each department separately is named as a capability of the laboratory window, but the screen that carries it is never displayed. | [NOT-SHOWN] |
| 8.2.1.1.1 | No validation, rejection message or recovery path is established for the sample withdrawal screen. | [NOT-SHOWN] |

## §14 Consolidated unresolved questions

Every question the source leaves open, gathered into one register. The register is a list of **questions**, not a list of gaps: an element in it is not a function the reference system is known to lack. Each row states what the source establishes, what it does not, where the question was examined, and what the question affects.

| Q ID | Category | Unit | Affected | Question | What the source establishes | What it does not establish | Where examined | Impact |
|---|---|---|---|---|---|---|---|---|
| Q-001 | B — element visible, its behaviour not established | 1 | 1.1.5.2 | The meaning of the three quick-search mode codes is not established. | The element is visible in the source. | The meaning of the three quick-search mode codes is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-002 | B — element visible, its behaviour not established | 1 | 1.2.2.4.2 | The purpose of the site-reference field is not established. | The element is visible in the source. | The purpose of the site-reference field is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-003 | B — element visible, its behaviour not established | 1 | 1.2.2.5.1.2 | The meaning of the three test-filter mode codes is not established. | The element is visible in the source. | The meaning of the three test-filter mode codes is not established. | showcase p. 36 | The behaviour of the element is not established; the element itself is. |
| Q-004 | A — element named, its screen never displayed | 1 | 1.2.2.6.9 | The identification card action is present, but the card it produces is never shown, so its fields and content are not established. | The element is named in the source. | The identification card action is present, but the card it produces is never shown, so its fields and content are not established. | guide p. 11-83 | Existence is established; the fields and the behaviour of the element are not. |
| Q-005 | A — element named, its screen never displayed | 1 | 1.2.1.11 | The test-registration statistics action is present, but the screen it opens is never shown, so its criteria and output are not established. | The element is named in the source. | The test-registration statistics action is present, but the screen it opens is never shown, so its criteria and output are not established. | guide p. 11-83 | Existence is established; the fields and the behaviour of the element are not. |
| Q-006 | B — element visible, its behaviour not established | 1 | 1.3.1.12 | The custody action is present on the account panel, but its behaviour is not established. | The element is visible in the source. | The custody action is present on the account panel, but its behaviour is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-007 | B — element visible, its behaviour not established | 1 | 1.6.1.2.3 | The content of the narrow P column of the results grid is not established. | The element is visible in the source. | The content of the narrow P column of the results grid is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-008 | B — element visible, its behaviour not established | 1 | 1.6.1.2.4 | The content of the narrow T column of the results grid is not established. | The element is visible in the source. | The content of the narrow T column of the results grid is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-009 | B — element visible, its behaviour not established | 1 | 1.10.1.4.6 | The VIP action's behaviour is not established. | The element is visible in the source. | The VIP action's behaviour is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-010 | B — element visible, its behaviour not established | 1 | 1.10.1.4.8 | The collected-results action's behaviour is not established. | The element is visible in the source. | The collected-results action's behaviour is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-011 | B — element visible, its behaviour not established | 1 | 1.10.1.4.9 | The modify-group action's behaviour is not established. | The element is visible in the source. | The modify-group action's behaviour is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-012 | B — element visible, its behaviour not established | 1 | 1.2.2.2.8 | The relation between the sample type carried in the identity block and the sample-type radio group of the test-selection block is not established. | The element is visible in the source. | The relation between the sample type carried in the identity block and the sample-type radio group of the test-selection block is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-013 | B — element visible, its behaviour not established | 1 | 1.6.1.2.6 | The relation between the abnormal-flag column and the low-and-high column of the result grid is not established. | The element is visible in the source. | The relation between the abnormal-flag column and the low-and-high column of the result grid is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-014 | D — presentation deliberately not described | 1 | 1.3.2.5 | The colour classification carried by the account grid's narrow column is not described here, because the presentation describes function only. | The element is visible in the source. | The colour classification carried by the account grid's narrow column is not described here, because the presentation describes function only. | guide p. 11-83 | The behaviour of the element is not established. |
| Q-015 | B — element visible, its behaviour not established | 1 | 1.6.1.2.2 | The content of the narrow P column of the delivery results grid is not established. | The element is visible in the source. | The content of the narrow P column of the delivery results grid is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-016 | B — element visible, its behaviour not established | 1 | 1.6.1.5.4 | The precise rule by which the low-or-high flag is set is not established, beyond the abnormal-flag states of Low, High and Critical. | The element is visible in the source. | The precise rule by which the low-or-high flag is set is not established, beyond the abnormal-flag states of Low, High and Critical. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-017 | B — element visible, its behaviour not established | 1 | 1.15.4.4 | The history report's per-test column carries the result of that test on the row of its analysis date; how a test that was not performed on a given date is represented is not established. | The element is visible in the source. | The history report's per-test column carries the result of that test on the row of its analysis date; how a test that was not performed on a given date is represented is not established. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-018 | C — property of the source as a whole | 1 | — | No validation, rejection message or recovery path is established for the patient screens, the account panel, the result entry, the culture entry, the search or the delivery screens. | The element is named in the source. | No validation, rejection message or recovery path is established for the patient screens, the account panel, the result entry, the culture entry, the search or the delivery screens. | guide p. 11-83 | Existence is established; the fields and the behaviour of the element are not. |
| Q-019 | C — property of the source as a whole | 1 | — | The required or optional status of the patient master fields is established only for the telephone number, the address, the treating doctor and the notes, which the source describes as optional. | The element is visible in the source. | The required or optional status of the patient master fields is established only for the telephone number, the address, the treating doctor and the notes, which the source describes as optional. | guide p. 11-83 | The behaviour of the element is not established; the element itself is. |
| Q-020 | B — element visible, its behaviour not established | 2 | 2.2.1.1.2 | What the shortened form of the patient work sheet omits relative to the full one is not established. | The element is visible in the source. | What the shortened form of the patient work sheet omits relative to the full one is not established. | guide p. 84-92 | The behaviour of the element is not established; the element itself is. |
| Q-021 | B — element visible, its behaviour not established | 2 | 2.2.1.2.2 | Whether the code range filter addresses the patient code or the laboratory identifier is not established. | The element is visible in the source. | Whether the code range filter addresses the patient code or the laboratory identifier is not established. | guide p. 84-92 | The behaviour of the element is not established; the element itself is. |
| Q-022 | B — element visible, its behaviour not established | 2 | 2.3.1.2.2 | Whether the code range filter addresses the patient code or the laboratory identifier is not established. | The element is visible in the source. | Whether the code range filter addresses the patient code or the laboratory identifier is not established. | guide p. 84-92 | The behaviour of the element is not established; the element itself is. |
| Q-023 | B — element visible, its behaviour not established | 2 | 2.3.1.1.4 | The source of the work group a group work sheet is built from is not established. | The element is visible in the source. | The source of the work group a group work sheet is built from is not established. | guide p. 84-92 | The behaviour of the element is not established; the element itself is. |
| Q-024 | B — element visible, its behaviour not established | 2 | 2.5.2.10 | An entry on the work sheet header whose meaning is not established. | The element is visible in the source. | An entry on the work sheet header whose meaning is not established. | guide p. 84-92 | The behaviour of the element is not established; the element itself is. |
| Q-025 | B — element visible, its behaviour not established | 2 | 2.1.3.2 | Whether a work sheet for a single test is produced from the test list or from the specific-test option is not established. | The element is visible in the source. | Whether a work sheet for a single test is produced from the test list or from the specific-test option is not established. | guide p. 84-92 | The behaviour of the element is not established; the element itself is. |
| Q-026 | C — property of the source as a whole | 2 | — | No validation, rejection message or recovery path is established for the work sheet windows. | The element is named in the source. | No validation, rejection message or recovery path is established for the work sheet windows. | guide p. 84-92 | Existence is established; the fields and the behaviour of the element are not. |
| Q-027 | B — element visible, its behaviour not established | 3 | 3.8.1.5 | The configuration file's purpose and the way it is maintained are not established. | The element is visible in the source. | The configuration file's purpose and the way it is maintained are not established. | guide p. 93-148 | The behaviour of the element is not established; the element itself is. |
| Q-028 | A — element named, its screen never displayed | 3 | 3.1.2.7 | The content of the barcode types screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the barcode types screen is not established; the screen is named in the system menu but is not shown. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-029 | A — element named, its screen never displayed | 3 | 3.1.2.10 | The content of the laboratory branches screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the laboratory branches screen is not established; the screen is named in the system menu but is not shown. | guide p. 96-99 | Existence is established; the fields and the behaviour of the element are not. |
| Q-030 | A — element named, its screen never displayed | 3 | 3.1.2.11 | The content of the patient titles and definitions screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the patient titles and definitions screen is not established; the screen is named in the system menu but is not shown. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-031 | A — element named, its screen never displayed | 3 | 3.1.2.12 | The content of the patient names and telephone numbers screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the patient names and telephone numbers screen is not established; the screen is named in the system menu but is not shown. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-032 | A — element named, its screen never displayed | 3 | 3.1.2.13 | The content of the laboratory accounts screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the laboratory accounts screen is not established; the screen is named in the system menu but is not shown. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-033 | A — element named, its screen never displayed | 3 | 3.1.2.14 | The content of the work groups screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the work groups screen is not established; the screen is named in the system menu but is not shown. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-034 | A — element named, its screen never displayed | 3 | 3.1.2.15 | The content of the result screen account screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the result screen account screen is not established; the screen is named in the system menu but is not shown. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-035 | A — element named, its screen never displayed | 3 | 3.1.2.17 | The content of the laboratory devices and equipment screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the laboratory devices and equipment screen is not established; the screen is named in the system menu but is not shown. | guide p. 96-99 | Existence is established; the fields and the behaviour of the element are not. |
| Q-036 | A — element named, its screen never displayed | 3 | 3.1.2.2 | The content of the test units screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the test units screen is not established; the screen is named in the system menu but is not shown. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-037 | A — element named, its screen never displayed | 3 | 3.1.2.5 | The content of the custom groups screen is not established; the screen is named in the system menu but is not shown. | The element is named in the source. | The content of the custom groups screen is not established; the screen is named in the system menu but is not shown. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-038 | B — element visible, its behaviour not established | 3 | 3.2.1.3.21 | Whether the sent-outside field of a test is set on the test or on each of its tubes is not established. | The element is visible in the source. | Whether the sent-outside field of a test is set on the test or on each of its tubes is not established. | guide p. 93-148 | The behaviour of the element is not established; the element itself is. |
| Q-039 | B — element visible, its behaviour not established | 3 | 3.4.1 | The structure of the price list screen is inferred from the prose; the screen itself is not shown in full. | The element is visible in the source. | The structure of the price list screen is inferred from the prose; the screen itself is not shown in full. | guide p. 93-148 | The behaviour of the element is not established; the element itself is. |
| Q-040 | B — element visible, its behaviour not established | 3 | 3.9.1.6.7 | The label of the main-list action on the antibiotics screen is not fully legible. | The element is visible in the source. | The label of the main-list action on the antibiotics screen is not fully legible. | showcase p. 45 | The behaviour of the element is not established; the element itself is. |
| Q-041 | C — property of the source as a whole | 3 | — | No validation, rejection message or recovery path is established for the data screens of the system module. | The element is named in the source. | No validation, rejection message or recovery path is established for the data screens of the system module. | guide p. 93-148 | Existence is established; the fields and the behaviour of the element are not. |
| Q-042 | A — element named, its screen never displayed | 4 | 4.1.2.2 | The change-password screen is named in the menu but is not shown, so its fields and its behaviour are not established. | The element is named in the source. | The change-password screen is named in the menu but is not shown, so its fields and its behaviour are not established. | guide p. 149-165 | Existence is established; the fields and the behaviour of the element are not. |
| Q-043 | B — element visible, its behaviour not established | 4 | 4.2.1.1.2 | The list of registered users is shown, but the shape of the list beyond the names is not established. | The element is visible in the source. | The list of registered users is shown, but the shape of the list beyond the names is not established. | guide p. 149-165 | The behaviour of the element is not established; the element itself is. |
| Q-044 | B — element visible, its behaviour not established | 4 | 4.5.2 | The record's page count, its magnification control and the laboratory's printed header are shown on the document, but their behaviour is not established. | The element is visible in the source. | The record's page count, its magnification control and the laboratory's printed header are shown on the document, but their behaviour is not established. | guide p. 149-165 | The behaviour of the element is not established; the element itself is. |
| Q-045 | C — property of the source as a whole | 4 | — | No validation, rejection message or recovery path is established for the users screens beyond the permission alert. | The element is named in the source. | No validation, rejection message or recovery path is established for the users screens beyond the permission alert. | guide p. 149-165 | Existence is established; the fields and the behaviour of the element are not. |
| Q-046 | B — element visible, its behaviour not established | 5 | 5.3.2.3.3 | The share column carries a percentage but no readable header, so its exact title is not established. | The element is visible in the source. | The share column carries a percentage but no readable header, so its exact title is not established. | guide p. 171; showcase p. 30-31 | The behaviour of the element is not established; the element itself is. |
| Q-047 | B — element visible, its behaviour not established | 5 | 5.3.3.2.3 | The share column carries a percentage but no readable header, so its exact title is not established. | The element is visible in the source. | The share column carries a percentage but no readable header, so its exact title is not established. | guide p. 171 | The behaviour of the element is not established; the element itself is. |
| Q-048 | B — element visible, its behaviour not established | 5 | 5.5.1.3 | The share column carries a percentage but no readable header, so its exact title is not established. | The element is visible in the source. | The share column carries a percentage but no readable header, so its exact title is not established. | showcase p. 30-31 | The behaviour of the element is not established; the element itself is. |
| Q-049 | B — element visible, its behaviour not established | 5 | 5.5.2.3 | The share column carries a percentage but no readable header, so its exact title is not established. | The element is visible in the source. | The share column carries a percentage but no readable header, so its exact title is not established. | showcase p. 33 | The behaviour of the element is not established; the element itself is. |
| Q-050 | B — element visible, its behaviour not established | 5 | 5.5.3 | The layout of the census by months is named in the source but not shown, so its columns are not established. | The element is visible in the source. | The layout of the census by months is named in the source but not shown, so its columns are not established. | showcase p. 30 | The behaviour of the element is not established; the element itself is. |
| Q-051 | E — an illustration, not a rule | 5 | 5.1.5 | The demonstration table of numbered rows is an illustration in the source; its values are examples and are not a requirement. | The element is visible in the source. | The demonstration table of numbered rows is an illustration in the source; its values are examples and are not a requirement. | guide p. 166-173 | The behaviour of the element is not established. |
| Q-052 | C — property of the source as a whole | 5 | — | No validation, rejection message or recovery path is established for the statistics screens. | The element is named in the source. | No validation, rejection message or recovery path is established for the statistics screens. | guide p. 166-173 | Existence is established; the fields and the behaviour of the element are not. |
| Q-053 | B — element visible, its behaviour not established | 6 | 6.2.1.2.2.1 | Two of the three options of the sent samples breakdown print the same wording; the difference between the two is not established. | The element is visible in the source. | Two of the three options of the sent samples breakdown print the same wording; the difference between the two is not established. | showcase p. 50 | The behaviour of the element is not established; the element itself is. |
| Q-054 | B — element visible, its behaviour not established | 6 | 6.2.1.2.2.2 | Two of the three options of the sent samples breakdown print the same wording; the difference between the two is not established. | The element is visible in the source. | Two of the three options of the sent samples breakdown print the same wording; the difference between the two is not established. | showcase p. 50 | The behaviour of the element is not established; the element itself is. |
| Q-055 | A — element named, its screen never displayed | 6 | 6.4.1 | The fields and the behaviour of the cash deposit and withdrawal screen are not established. | The element is named in the source. | The fields and the behaviour of the cash deposit and withdrawal screen are not established. | showcase p. 49 | Existence is established; the fields and the behaviour of the element are not. |
| Q-056 | A — element named, its screen never displayed | 6 | 6.4.2 | The fields and the behaviour of the companies and representatives account screen are not established. | The element is named in the source. | The fields and the behaviour of the companies and representatives account screen are not established. | showcase p. 49 | Existence is established; the fields and the behaviour of the element are not. |
| Q-057 | B — element visible, its behaviour not established | 6 | 6.2.5 | The columns of the treating doctor report are the same as those of the patients' account report, but the report is not shown in full. | The element is visible in the source. | The columns of the treating doctor report are the same as those of the patients' account report, but the report is not shown in full. | guide p. 174-189 | The behaviour of the element is not established; the element itself is. |
| Q-058 | C — property of the source as a whole | 6 | — | No validation, rejection message or recovery path is established for the accounts screens. | The element is named in the source. | No validation, rejection message or recovery path is established for the accounts screens. | guide p. 174-189 | Existence is established; the fields and the behaviour of the element are not. |
| Q-059 | B — element visible, its behaviour not established | 7 | 7.1.4.1.5 | The card tab is named in the tab strip but its fields are not shown. | The element is visible in the source. | The card tab is named in the tab strip but its fields are not shown. | guide p. 190-207 | The behaviour of the element is not established; the element itself is. |
| Q-060 | B — element visible, its behaviour not established | 7 | 7.1.4.3.11 | The option that prints the file number or the card number instead of the print date is shown in the later rendering of the tab only; the field that carries it is not legible. | The element is visible in the source. | The option that prints the file number or the card number instead of the print date is shown in the later rendering of the tab only; the field that carries it is not legible. | guide p. 190-207 | The behaviour of the element is not established; the element itself is. |
| Q-061 | B — element visible, its behaviour not established | 7 | 7.2.1.1.3.1 | The three font presets of the signature are not numbered in the source, and the property each of them controls is not established. | The element is visible in the source. | The three font presets of the signature are not numbered in the source, and the property each of them controls is not established. | guide p. 190-207 | The behaviour of the element is not established; the element itself is. |
| Q-062 | B — element visible, its behaviour not established | 7 | 7.2.1.7.2 | The size that belongs to each numbered font preset is not legible where the presets and their sizes are printed together. | The element is visible in the source. | The size that belongs to each numbered font preset is not legible where the presets and their sizes are printed together. | guide p. 190-207 | The behaviour of the element is not established; the element itself is. |
| Q-063 | B — element visible, its behaviour not established | 7 | 7.5.1.1.2 | The second form of the currency is shown with two examples; the source does not establish which of the two belongs to which kind of receipt. | The element is visible in the source. | The second form of the currency is shown with two examples; the source does not establish which of the two belongs to which kind of receipt. | showcase p. 203 | The behaviour of the element is not established; the element itself is. |
| Q-064 | B — element visible, its behaviour not established | 7 | 7.6.1.3.1 | The words printed in the head of the envelope are shown in the source as a sample of one laboratory; the label of the field is not legible. | The element is visible in the source. | The words printed in the head of the envelope are shown in the source as a sample of one laboratory; the label of the field is not legible. | showcase p. 206 | The behaviour of the element is not established; the element itself is. |
| Q-065 | A — element named, its screen never displayed | 7 | 7.1.4.3 | No validation, rejection message or recovery path is established for the system settings window. | The element is named in the source. | No validation, rejection message or recovery path is established for the system settings window. | guide p. 190-207 | Existence is established; the fields and the behaviour of the element are not. |
| Q-066 | A — element named, its screen never displayed | 7 | 7.1.4.1.5 | The card tab is named in the tab strip but its screen is never displayed. | The element is named in the source. | The card tab is named in the tab strip but its screen is never displayed. | guide p. 190-207 | Existence is established; the fields and the behaviour of the element are not. |
| Q-067 | B — element visible, its behaviour not established | 8 | 8.2.1.4.1 | The column headings of the samples-to-draw grid are not printed on the page; the grid is shown carrying the same two columns as the withdrawn-samples grid. | The element is visible in the source. | The column headings of the samples-to-draw grid are not printed on the page; the grid is shown carrying the same two columns as the withdrawn-samples grid. | guide p. 208-211 | The behaviour of the element is not established; the element itself is. |
| Q-068 | B — element visible, its behaviour not established | 8 | 8.2.1.5 | The control for the container that carries no test of its own is labelled, but its action is not established. | The element is visible in the source. | The control for the container that carries no test of its own is labelled, but its action is not established. | guide p. 209 | The behaviour of the element is not established; the element itself is. |
| Q-069 | B — element visible, its behaviour not established | 8 | 8.2.1.7.4 | The control that shows or hides the part of the window that carries the samples is not labelled, and its action is not established. | The element is visible in the source. | The control that shows or hides the part of the window that carries the samples is not labelled, and its action is not established. | guide p. 209 | The behaviour of the element is not established; the element itself is. |
| Q-070 | B — element visible, its behaviour not established | 8 | 8.2.1.9.3 | The list of the containers of the first print group ends with an ellipsis in the source, so the whole list is not established. | The element is visible in the source. | The list of the containers of the first print group ends with an ellipsis in the source, so the whole list is not established. | guide p. 208-211 | The behaviour of the element is not established; the element itself is. |
| Q-071 | B — element visible, its behaviour not established | 8 | 8.2.1.9.4 | The list of the containers of the second print group ends with an ellipsis in the source, so the whole list is not established. | The element is visible in the source. | The list of the containers of the second print group ends with an ellipsis in the source, so the whole list is not established. | guide p. 208-211 | The behaviour of the element is not established; the element itself is. |
| Q-072 | B — element visible, its behaviour not established | 8 | 8.2.1.3.3 | The identifier of the visit of the laboratory is shown empty for the patient of the source, and the rule that fills it is not established. | The element is visible in the source. | The identifier of the visit of the laboratory is shown empty for the patient of the source, and the rule that fills it is not established. | guide p. 208-211 | The behaviour of the element is not established; the element itself is. |
| Q-073 | B — element visible, its behaviour not established | 8 | 8.2.1.3.4 | The treating doctor is shown empty for the patient of the source, and the rule that fills it is not established. | The element is visible in the source. | The treating doctor is shown empty for the patient of the source, and the rule that fills it is not established. | guide p. 208-211 | The behaviour of the element is not established; the element itself is. |
| Q-074 | A — element named, its screen never displayed | 8 | 8.1.2.1 | The coding of the samples of each department separately is named as a capability of the laboratory window, but the screen that carries it is never displayed. | The element is named in the source. | The coding of the samples of each department separately is named as a capability of the laboratory window, but the screen that carries it is never displayed. | guide p. 208-211 | Existence is established; the fields and the behaviour of the element are not. |
| Q-075 | A — element named, its screen never displayed | 8 | 8.2.1.1.1 | No validation, rejection message or recovery path is established for the sample withdrawal screen. | The element is named in the source. | No validation, rejection message or recovery path is established for the sample withdrawal screen. | guide p. 208-211 | Existence is established; the fields and the behaviour of the element are not. |

**Count of unresolved questions: 75.** The register is divided into five categories, and the subtotals below are the counts of the rows of each category in this table:

| Category | Meaning | Questions |
|---|---|---|
| A — element named, its screen never displayed | The element is named in the source, but the screen or the document it produces is never displayed, so its fields and its behaviour are not established. Its existence is established; it is **not** a gap. | 19 |
| B — element visible, its behaviour not established | The element is visible in the source, but what it does is not established. | 47 |
| C — property of the source as a whole | A property the source as a whole does not establish, rather than a single element. | 7 |
| D — presentation deliberately not described | A property the source shows but does not describe, because the source does not describe it at all. | 1 |
| E — an illustration, not a rule | An example in the source that must not be read as a requirement. | 1 |
| **Total** | | **75** |

The categories are assigned from the row itself and are reproducible: a row whose affected element is a dash is a category C row; otherwise the category follows from the evidence status the row carries. The evidence status of an entry and the presence of a question about it are two separate measures and are not required to agree.

**What would resolve a question.** A question about a *behaviour* is resolved by a page of the source that shows the element being used, or by a sentence of the source that states what it does. A question about an *existence* is resolved by a page that shows the screen. A question about a *label* is resolved by a rendering in which the label is legible. None of these was available for the questions above, which is why they are recorded rather than answered.

## §15 Coverage and validation summary

### §15.1 What each count includes and excludes

The counts below are of **inventory entries**, not of business functions. An entry is one row of a definition table. A parent entry and its children are separate entries. The six levels of §3 partition the inventory: every entry has exactly one level, and the counts of the levels add up to the total, so no parent is counted with its children. every entry has exactly one level, and the levels partition the inventory.

| Measure | Definition | Value |
|---|---|---|
| Functional units | The major functional areas the system is documented in. | 8 |
| Functional groups | The groups inside the units; each group is one business process or one related set of capabilities. | 72 |
| Independently identifiable functions | Entries at the level *Function* of §3: an action that is a materially distinct operation, or a capability the source states. **A screen is not counted as a function by virtue of being a screen**; it is a container, unless it carries no child entry, in which case it describes a behaviour that is not decomposed anywhere and is recorded as a function. A view is a subfunction, and an action that only leaves the current screen is a subfunction. | 327 |
| Subfunctions | Entries at the level *Subfunction*: a menu entry, a navigation step, or an option that is a distinct selectable operation. | 266 |
| Rules and constraints | Entries at the level *Rule*. | 50 |
| UI and data elements | Entries at the level *UI element*: fields, buttons, columns, labels, indicators, grids and lists. | 786 |
| Containers | Entries at the level *Container*: the screens, the panels, the blocks and the field groups that carry other entries. | 248 |
| Outputs | Entries at the level *Output*: the artefacts the system produces. | 38 |
| **Total inventory entries** | Every row of every definition table, counted once. | **1715** |
| Out of scope for parity | Entries that belong to one of the five excluded categories of §2. They are counted inside the total and are listed separately so that they are never carried into a gap list. | 51 |
| Unresolved questions | The rows of §14. | 75 |
| Evidence status *Verified* | Entries directly supported by the source. | 1659 |
| Evidence status *Unverified* | Entries whose behaviour the source does not establish. | 30 |
| Evidence status *Not shown* | Entries that are named but never displayed. | 17 |
| Evidence status *Sample* | Entries that are an illustration rather than a rule. | 9 |

**The number of independent business functions is not the number of inventory entries.** A comparison that counts 1715 inventory entries against a target implementation will report a very large number of missing items, most of which are fields, columns and labels that are part of a function that may be present. The figures that describe behaviour are the 327 functions, the 266 subfunctions and the 50 business rules; the 786 UI and data elements and the 248 containers are the surface on which those behaviours are evidenced.

### §15.2 Per-unit counts

| Unit | Groups | Functions | Subfunctions | Rules | UI / data elements | Outputs | Entries | Out of scope | Unverified | Not shown |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 — Patient Registration and Test Registration | 25 | 159 | 52 | 17 | 379 | 15 | 622 | 36 | 17 | 4 |
| 2 — Work Sheet for Patients or Tests | 6 | 14 | 15 | 1 | 90 | 5 | 125 | 0 | 1 | 0 |
| 3 — Test Data and External Dealings | 15 | 69 | 41 | 4 | 180 | 0 | 294 | 10 | 2 | 3 |
| 4 — Users, Roles and Lab Performance Monitoring | 5 | 20 | 10 | 3 | 56 | 1 | 90 | 2 | 0 | 1 |
| 5 — Statistics | 7 | 6 | 30 | 2 | 87 | 11 | 136 | 1 | 3 | 5 |
| 6 — Inventory and Lab Accounting | 5 | 13 | 33 | 7 | 71 | 6 | 130 | 2 | 2 | 4 |
| 7 — Basic System Settings | 7 | 34 | 81 | 11 | 129 | 0 | 255 | 0 | 3 | 0 |
| 8 — Laboratory Sample Withdrawal | 2 | 12 | 4 | 5 | 42 | 0 | 63 | 0 | 2 | 0 |

### §15.3 What was inspected, and what was not

**Read as rendered images.** Forty-five pages of the in-scope range of the showcase were read as images: the barcode labels, the receipt, the printed reports, the work papers, the statistics, the accounts, the tools, the sign-in window, the main window, the system and statistics windows, the result-entry screens, and the printed culture and semen reports. The pages of the guide that carry a screen, a form or a printed output were read as images in the earlier passes, and pages 36, 103, 171 and 184 were read as images in the last pass. **The whole of the in-scope range was not read as an image**, and this document does not claim that it was.

**Characterised from the text layer only.** Six pages of the in-scope range of the showcase: pages 1 and 2, which carry the vendor's contact details and the title of the document and no function of the system, and pages 15, 16, 18 and 22, which are printed result reports of the families that were read in full on other pages. For these six, the wording of a label is not claimed to have been read from a rendering.

**Not opened.** The pages of the showcase beyond the accounts-and-tools sequence, which lie outside the range in scope. Nothing in this document rests on them.

**The text layer** of both documents was used to locate the sections and to cross-check the presence of a label. It was **not** used as the authority for any claim: the layer of the guide stores its Arabic in a broken order, so a literal match against it is meaningless.

**The label cross-check.** Every source label in the inventory was put through a four-step ladder against the text of the two source documents. The counts are of labels, not of entries.

| Class | Labels | What it establishes |
|---|---|---|
| Exact textual match | 645 | The wording of the label is confirmed against the source text, character for character. |
| Match after a documented normalisation | 28 | The wording is confirmed once the letter variants, or the order in which the words of a line are stored, are accounted for. |
| Approximate — characters present, order not established | 420 | **Nothing about exact correspondence.** It shows only that the characters of the label occur on one line of the source. The text layer of the guide is broken, and this class is the measure of that breakage. |
| Not matched by the text layer | 85 | The text layer does not carry the label. This is a fact about the text layer, not about the label. |
| Composite cell — a set of labels in one cell | 33 | Verified part by part rather than as a whole. |
| No label (a dash) | 504 | Not applicable. |

**What the check does not establish.** It establishes the **wording** of 673 labels and does not establish it for the 505 labels of the weaker classes; no such claim is made. It says nothing about the **association** of a label with the element the inventory records it against: that is settled by the page that carries the element, and for the entries of the earlier inventory a per-element page record was not kept. A label that failed the text-layer test is not thereby wrong.

**Not established.** Whether the showcase and the guide describe the same build of the software was not established; where the two differ, both readings are recorded and the difference is listed in §14. The per-page record of which pages of the guide were read as rendered images during the earlier passes was not kept and is not recoverable; the guide's prose pages are reliably covered by its text layer, its form pages by the rendered reads recorded in the evidence register of §4.

**Not claimable.** This document is not established as exhaustive. A function that the source never shows and never describes cannot be found by reading the source, and this document cannot therefore claim that no such function exists. The inventory is a faithful record of what the source shows and describes, and nothing more.

### §15.4 Coverage of the functional scope

| Source area | Inspected | Documented | Notes |
|---|---|---|---|
| Guide chapter 1, patient registration and test registration | Yes | Unit 1 | |
| Guide chapter 2, work sheet | Yes | Unit 2 | |
| Guide chapter 3, test data and external dealings | Yes | Unit 3 | The screens named in the system menu but never shown are listed as *Not shown*. |
| Guide chapter 4, users, roles and monitoring | Yes | Unit 4 | |
| Guide chapter 5, statistics | Yes | Unit 5 | The charts of the census reports were found on a page of the guide and on three pages of the showcase, and are now recorded. |
| Guide chapter 6, inventory and lab accounting | Yes | Unit 6 | The account statement variants and the claim were found in the showcase and are now recorded. |
| Guide chapter 7, basic system settings | Yes | Unit 7 | |
| Guide chapter 8, sample withdrawal | Yes | Unit 8 | The chapter carries no entry in the guide's own table of contents. |
| Showcase, the in-scope range | Partly — 45 pages read as images, 6 from the text layer alone | Units 1-8 | The main tools window, the foreign report, the delivery sheet, the result review report and the accounts entries were found here and were absent from the earlier inventory. |

## §16 Global identifier registry

Every identifier of the document, with its canonical name, the label as it is written on the screen, its kind, the level at which it is recorded, the unit that owns it, its evidence status and its parity scope. The identifiers are permanent; a new element is given an identifier appended to the block that holds it, and no identifier is ever renumbered.

| ID | Canonical name | Source label | Type | Level | Unit | Evidence | Scope |
|---|---|---|---|---|---|---|---|
| **1.1** | **Application Shell and Sign-In** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.1.1 | Top Ribbon | الشريط العلوي | Control | UI element | 1 | [VERIFIED] | In scope |
| 1.1.1.1 | Ribbon Item List | — | List | UI element | 1 | [VERIFIED] | In scope |
| 1.1.1.1.1 | Exit Item | Exit | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.2 | About Us Item | About Us | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.3 | Send Result Item | Send Result | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.4 | Setting Item | Setting | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.5 | System Item | System | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.6 | Users Item | Users | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.7 | Statistics Item | Statistics | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.8 | Accounts Item | Accounts | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.9 | Tools Item | Tools | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.10 | Work Sheet Item | Work sheet | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.11 | Laboratory Item | Laboratory | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.1.12 | Patients Item | Patients | Navigation | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.1.2 | Active Module Indicator | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.1.2 | Status Bar | — | Control | UI element | 1 | [VERIFIED] | In scope |
| 1.1.2.1 | User Name Indicator | User Name | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.2.2 | Last Login Indicator | Last Login | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.2.3 | Database Label Indicator | SQL DATABASE | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.2.4 | Today Indicator | Today | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.3 | Startup Splash Screen | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.1.3.1 | Data Source Option Group | — | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.1.3.1.1 | File Database Option | — | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.3.1.2 | Client Server Database Option | — | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.1.3.2 | Startup Progress Indicator | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.1.4 | Sign-In Window | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.1.4.1 | Sign-In User Name Field | User Name | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.4.2 | Sign-In Password Field | Password | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.4.3 | Show Password Characters Option | Show password characters | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.4.4 | Remember Login Option | Remember Login | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.4.5 | Application Sign In Action | Sign in | Action | Function | 1 | [VERIFIED] | In scope |
| 1.1.5 | Patient Search Toolbar | — | Control | UI element | 1 | [VERIFIED] | In scope |
| 1.1.5.1 | Quick Search Field | start | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.1.5.2 | Quick Search Mode Options | TM / TG / CG | Field group | Container | 1 | [UNVERIFIED] | In scope |
| **1.2** | **Patient Registration and Master Data** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.2.1 | Main Patients Window | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.2.1.1 | Add Patient Action | إضافة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.1.2 | Enter Results Action | نتائج التحاليل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.1.3 | Find Patient Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.1.4 | Period List Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.1.5 | Unfinished Results View | — | View | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.1.6 | Unverified Results View | — | View | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.1.7 | Unprinted Results View | — | View | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.1.8 | Deliver Results Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.1.9 | Settle Accounts Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.1.10 | Undelivered Results Count | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.2.1.11 | Test Registration Statistics Action | — | Action | Function | 1 | [NOT-SHOWN] | In scope |
| 1.2.2 | Add-and-Edit Patient Data Screen | إضافة وتعديل بيانات المرضى | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.2.2.1 | Patient Header Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.2.2.1.1 | Entry Date Field | تاريخ الدخول | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.1.2 | Receipt Date Field | تاريخ الاستلام | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.1.3 | Patient Code Field | كود المريض | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.1.4 | VIP Action | VIP | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.1.5 | Laboratory ID Action | Lab. ID | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.2 | Identity Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.2.2.2.1 | Patient Name Field | اسم المريض | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.2 | Sex Radio Group | النوع | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.2.1 | Male Option | Male | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.2.2.2 | Female Option | Female | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.2.3 | Age Value Field | سن المريض | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.4 | Age Unit Selector | Years | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.5 | Mobile and Telephone Field | موبيل / تليفون | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.6 | Card Number Field | رقم بطاقة / الملف | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.7 | Account Type Selector | نوع الحساب | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.8 | Sample Type Field | نوع العينة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.9 | Patient Address Field | عنوان المريض | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.10 | Referring Entity Selector | جهة الإحالة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.10.1 | Referring Entity Additional Information Action | معلومات اضافية | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.2.11 | Treating Doctor Selector | الطبيب المعالج | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.2.11.1 | Treating Doctor Additional Information Action | معلومات اضافية | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.3 | Medical History Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.2.2.3.1 | Fasting Toggle | صائم | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.2 | Fasting Hours Field | عدد ساعات الصيام | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.3 | Diabetes Treatment Toggle | هل يؤخذ علاج للسكر | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.4 | Blood-Thinning Treatment Toggle | هل يؤخذ علاج لسيولة الدم | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.5 | Gland Treatment Toggle | هل يؤخذ علاج للغدد | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.6 | Antiviral Treatment Toggle | هل يؤخذ علاج للفيروسات | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.7 | Antibiotic Treatment Toggle | هل يؤخذ مضاد حيوي | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.8 | Liver Treatment Toggle | هل يؤخذ علاج للكبد | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.9 | Pregnancy Toggle | للسيدات هل يوجد حمل | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.10 | Imaging Within Two Days Toggle | هل تم عمل اشاعة بالصبغة او بالموجات الصوتية خلال يومين | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.11 | Anaemia Condition Toggle | مريض بفقر الدم | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.12 | Joint Inflammation Condition Toggle | مريض بالتهاب المفاصل | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.13 | High Blood Pressure Condition Toggle | مريض بالضغط الدم | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.14 | Renal Failure Condition Toggle | مريض بالفشل كلوي | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.3.15 | Lupus Condition Toggle | مريض بالذئبة الحمراء | Toggle | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.4 | Notes and Communication Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.2.2.4.1 | Patient Notes Field | ملاحظات | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.4.2 | Print Result Password Field | Print Result Password | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.4.2.1 | Print Result Password Enable Option | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.4.3 | Send SMS Field | Send SMS | Field | UI element | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.2.2.4.4 | Send E-mail Field | Send E-mail | Field | UI element | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.2.2.4.5 | Fax Field | By Fax | Field | UI element | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.2.2.4.6 | Send Action | — | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.2.2.4.7 | Site Upload Action | — | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.2.2.4.8 | Site Reference Field | External Result Service Address | Field | UI element | 1 | [UNVERIFIED] | OUT OF SCOPE |
| 1.2.2.5 | Test Selection Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.2.2.5.1 | Test Filter Bar | — | Control | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.5.1.1 | Test Filter Field | start | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.5.1.2 | Test Filter Mode Options | TM / TG / CG | Field group | Container | 1 | [UNVERIFIED] | In scope |
| 1.2.2.5.1.3 | Complete Test Menu Action | complete test menu | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.5.1.4 | Test Group Menu Action | test group menu | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.5.2 | Master Test List | — | List | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.5.3 | Add Select Test Action | Add select test | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.5.4 | Patient Test Delete Action | Delete | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.5.5 | All Tests Action | All | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.5.6 | Patient's Tests List | Patient's tests | List | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.5.6.1 | Patient's Tests Count | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.5.7 | Test Group List | — | List | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.5.8 | Add All Tests In Group Action | Add all tests in group | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.5.9 | Group Send E-mail Action | Send E-mail | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.2.2.5.10 | Sample Type Radio Group | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.5.10.1 | Urine Option | Urine | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.5.10.2 | Stool Option | Stool | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.5.10.3 | Blood Option | Blood | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.5.10.4 | Semen Option | Semen | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.5.10.5 | CSF Option | CSF | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.5.11 | Taken Outside Lab Flag | Taken outside lab | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.2.2.6 | Action Band | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.2.2.6.1 | Save Registration Action | حفظ | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.2 | Add Entry Action | إضافة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.3 | Edit Entry Action | تعديل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.4 | Undo Action | تراجع | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.5 | Print Barcode Action | الباركود | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.6 | Print Receipt Action | الإيصال | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.7 | Patient Invoice Action | فاتورة المرضى | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.8 | Patient Envelope Print Action | طباعة ظرف | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.9 | Modify Action | تعديل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.10 | Follow-up Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.11 | Work Sheet Action | ورقة العمل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.12 | ID Card Action | الكارنيه | Action | Function | 1 | [NOT-SHOWN] | In scope |
| 1.2.2.6.13 | Patient Screen Action | الشاشة الرئيسية | Action | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.6.14 | Main Point Action | — | Action | Subfunction | 1 | [VERIFIED] | In scope |
| 1.2.2.6.15 | Patient Window Action | نافذة المرضى | Action | Function | 1 | [VERIFIED] | In scope |
| 1.2.2.6.16 | Lab Results Action | نتائج التحاليل | Action | Function | 1 | [VERIFIED] | In scope |
| **1.3** | **Patient Account** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.3.1 | Account Panel | — | Screen region | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.1 | Total of Tests Field | اجمالى التحاليل | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.2 | Discount Value Field | قيمة الخصم | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.3 | Discount Percentage Field | % | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.4 | Paid Amount Field | المدفوع | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.4.1 | Surcharge Entry Convention | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.3.1.4.2 | Enter Key Account Operation | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.3.1.5 | Paid Previously Field | المدفوع سابقا | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.6 | Amount After Discount Field | التكلفة بعد الخصم | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.7 | Total Collected Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.8 | Remainder for Customer Field | الباقى للمريض | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.9 | Remainder for Laboratory Field | الباقى للمعمل | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.3.1.10 | Confirm Payment Action | موافق | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.1.11 | Net Settlement Action | خالص | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.1.12 | Account Summary Action | حساب المريض | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.1.13 | Custody Action | حوافظ | Action | Function | 1 | [UNVERIFIED] | In scope |
| 1.3.1.14 | Account Back Action | تراجع | Action | Subfunction | 1 | [VERIFIED] | In scope |
| 1.3.2 | Patient Account Screen | حساب المريض | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.3.2.1 | Account Transaction Grid | — | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.3.2.1.1 | Paid Date Column | Paid date | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.3.2.1.2 | Money Column | Money | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.3.2.1.3 | User Name Column | User name | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.3.2.1.4 | Edit Date Column | Edit date | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.3.2.1.5 | Color Code Column | Color code | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.3.2.2 | Modify Amount Action | تعديل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.2.3 | Delete Transaction Action | حذف | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.2.4 | Pay Action | دفع | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.2.5 | Refund Action | استرداد | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.2.6 | Add Amount Action | اضافة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.2.7 | Clear Account Action | تصفير الحساب | Action | Function | 1 | [VERIFIED] | In scope |
| 1.3.2.8 | Account Patient List Action | قائمة المرضى | Action | Function | 1 | [VERIFIED] | In scope |
| **1.4** | **Results Delivery and Account Settlement** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.4.1 | Deliver Patient Results Screen | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.4.1.1 | Delivery Filter Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.4.1.1.1 | Delivery Date Filter | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.1.2 | Delivery Name Filter | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.1.3 | Delivery Status Filter | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.1.4 | Delivery Department Filter | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.2 | Patient Scope Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.4.1.2.1 | Patient Source Selector | Individual / LabToLab / Contracts | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.2.2 | Patient Group Selector | VIP / Free / All | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.2.3 | Patients Today Panel | عدد المرضى اليوم | Panel | Container | 1 | [VERIFIED] | In scope |
| 1.4.1.2.3.1 | Delivery Patient Grid | — | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3 | Delivery Result Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1 | Delivery Result List Grid | — | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1.1 | Test Abbreviation Column | Test Abbreviation | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1.2 | P Column | P | Grid column | UI element | 1 | [UNVERIFIED] | In scope |
| 1.4.1.3.1.3 | T Column | T | Grid column | UI element | 1 | [UNVERIFIED] | In scope |
| 1.4.1.3.1.4 | Result Column | Result | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1.5 | Status Column | Status | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1.6 | Delivery Finish Column | Finish | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1.7 | Delivery Verify Column | Verify | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1.8 | Delivery Print Column | Print | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1.9 | Delivery Export Column | Export | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.4.1.3.1.10 | See Report Action | See Report | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4 | Delivery Action Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.4.1.4.1 | Patient Account Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.2 | Refresh Delivery List Action | Refresh | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.3 | Delivery List Print Action | طباعة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.4 | Preview Action | معاينة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.5 | Copy Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.6 | Notes Action | ملاحظات | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.7 | Delivery Patient Data Action | بيانات المريض | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.8 | Combined Report Action | تقرير مجمع | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.9 | Patient Medical History Action | التاريخ المرضي | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.10 | Delivery Empty Report Action | تقرير فارغ | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.11 | Delivery Envelope Print Action | طباعة ظرف | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.12 | Send SMS Action | SMS | Action | Function | 1 | [VERIFIED] | In scope |
| 1.4.1.4.13 | Delivery Main List Action | القائمة الرئيسية | Action | Subfunction | 1 | [VERIFIED] | In scope |
| **1.5** | **Patient and Test Modification** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.5.1 | Modification Path One | الطريقة الأولى | Workflow | UI element | 1 | [VERIFIED] | In scope |
| 1.5.1.1 | Patient Icon Action | Patient | Action | Function | 1 | [VERIFIED] | In scope |
| 1.5.1.2 | Patient List Open Action | قائمة المرضى | Action | Function | 1 | [VERIFIED] | In scope |
| 1.5.1.3 | Registration Day Selector | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.5.1.4 | Edit Patient Action | تعديل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.5.1.5 | Patient Selection Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.5.1.6 | Modification Save Action | حفظ | Action | Function | 1 | [VERIFIED] | In scope |
| 1.5.2 | Modification Path Two | — | Workflow | UI element | 1 | [VERIFIED] | In scope |
| 1.5.2.1 | Search Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.5.2.2 | Patient Data Entry Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.5.3 | Patient Data Access Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| **1.6** | **Test Result Entry** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.6.1 | Result Entry Screen | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.1 | Patient List Panel | عدد المرضى اليوم | Panel | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.1.1 | Result Entry Patient Grid | — | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.1.2 | Patient Name Click Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.1.3 | Patient Count Indicator | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2 | Test Results Grid | — | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.1 | Result Entry Test Abbreviation Column | Test Abbreviation | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.2 | Result Entry P Column | P | Grid column | UI element | 1 | [UNVERIFIED] | In scope |
| 1.6.1.2.3 | Result Entry T Column | T | Grid column | UI element | 1 | [UNVERIFIED] | In scope |
| 1.6.1.2.4 | Result Entry Result Column | Result | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.5 | Result Entry Status Column | Status | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.6 | Abnormal Flag Column | — | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.7 | Low and High Column | — | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.8 | Unit Column | — | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.9 | Normal Range Column | Range | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.10 | Finish Flag | Finish | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.11 | Verify Flag | Verify | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.12 | Result Print Flag | Print | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.13 | Export Flag | Export | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.14 | Per-Test Comment Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.2.15 | Double-Click Test Action | See Report | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.3 | Entry Action Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.3.1 | Save Results Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.3.2 | Print Preview Action | معاينة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.3.3 | Result Report Print Action | طباعة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.4 | Report Settings Screen | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1 | Report Title Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.1 | Report Screen Title Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.2 | Report Main Title Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.3 | Report Sub Title Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.4 | Report Sub Sub-title Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.5 | Reference Range Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.6 | Report Comment Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.7 | Report Normal Range Field | Normal Range | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.8 | Main Symbol Field | Main Symbol | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.9 | Sub Symbol Field | Sub Symbol | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.1.10 | Sub-title Symbol Field | Sub-title Symb | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.2 | Report Display Option Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.4.2.1 | Show Abnormal Background Option | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.2.2 | Print Status Flag Option | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.2.3 | Print Patient History Automatically Option | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.3 | Report Typography Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.4.3.1 | Symbol Font Selectors | — | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.4.3.2 | Vertical Spacing Fields | — | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.4.3.3 | Normal Result Font Selector | Normal result font | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.3.4 | Text Alignment Control | محاذاة النص من اليسار | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.3.5 | Cell Expansion Control | عرض الخانة لتسع المحتوى | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.3.6 | Offset Amount Control | مقدار الإزاحة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.4 | Report Output Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.4.4.1 | Printing Target Selector | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.4.4.2 | Save Settings Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.4.4.3 | Export Data Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.4.4.4 | Close Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.5 | Report Entry Grid | Test Name \ ·  Result \ ·  Unit \ ·  L or H \ ·  Normal Ranges \ ·  Verified \ ·  Print | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.1 | Report Entry Test Name Column | Test Name | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.2 | Report Entry Result Column | Result | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.3 | Report Entry Unit Column | Unit | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.4 | Report Entry Flag Column | L or H | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.5 | Report Entry Normal Ranges Column | Normal Ranges | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.6 | Report Entry Verified Flag | Verified | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.7 | Report Entry Print Flag | Print | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.8 | Report Entry Comment Field | Comment | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.9 | Urine Sample Sub-Row Panel | عرض عينة البول في التقرير المجمع | Panel | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.5.9.1 | Urine Sample Sub-Row | Urine Sample | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.5.9.2 | Sub-Result Row | Glucose / Acetone | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.5.9.3 | Missing Sample Marker | No Sample | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.6 | Test Entry Variants | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.6.1 | Single Test Entry | — | Variant | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.6.2 | Profile Test Entry | — | Variant | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.6.3 | Paired Control And Patient Entry | — | Variant | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.6.4 | Derived Value Entry | — | Variant | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.6.5 | Multi-Unit Entry | — | Variant | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.6.6 | Qualitative Result Entry | — | Variant | UI element | 1 | [VERIFIED] | In scope |
| 1.6.1.7 | Report Screen Actions | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.6.1.7.1 | Save Report Action | حفظ | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.7.2 | Report Screen Print Action | طباعة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.7.3 | Report Screen Preview Action | معاينة الطباعة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.7.4 | Report Patient History Action | Patient History | Action | Function | 1 | [VERIFIED] | In scope |
| 1.6.1.7.5 | Report Screen Back Action | رجوع | Action | Subfunction | 1 | [VERIFIED] | In scope |
| 1.6.1.7.6 | Report Screen Main List Action | القائمة الرئيسية | Action | Subfunction | 1 | [VERIFIED] | In scope |
| **1.7** | **Combined Report** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.7.1 | Combined Report Screen | تقرير مجمع | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.7.1.1 | Patient Tests In Our Lab List | Patient tests in our lab | List | UI element | 1 | [VERIFIED] | In scope |
| 1.7.1.2 | Available Tests In Report List | Available tests in report | List | UI element | 1 | [VERIFIED] | In scope |
| 1.7.1.3 | Add Test To Report Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.7.1.4 | Reorder Report Tests Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.7.1.4.1 | Move Test Up Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.7.1.4.2 | Move Test Down Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.7.1.5 | Combined Report Option Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.7.1.5.1 | Print Sub Title Option | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.7.1.5.2 | Print Printed Test Again Option | — | Field | UI element | 1 | [VERIFIED] | In scope |
| **1.8** | **Empty Report** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.8.1 | Empty Report Screen | تقرير فارغ | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.8.1.1 | Empty Report Open Action | تقرير فارغ | Action | Function | 1 | [VERIFIED] | In scope |
| 1.8.1.2 | Enter Data And Print Option | — | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.8.1.3 | Patient Data Sheet Option | — | Option | Subfunction | 1 | [VERIFIED] | In scope |
| **1.9** | **Culture Result Entry** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.9.1 | Culture Screen | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.1 | Culture Data Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.1.1 | Sample Field | Sample | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.1.2 | Organism A Field | Organism A | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.1.3 | Organism B Field | Organism B | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.1.4 | Organism C Field | Organism C | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.1.5 | Culture Condition Field | Culture Condition | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.1.6 | Colony Count Field | Colony Count | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.2 | Microscopy Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.2.1 | Reaction Field | Reaction | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.2.2 | Pus Cells Field | Pus Cells | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.2.3 | Red Blood Cells Field | R.B.Cs | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.2.4 | Epithelial Cells Field | Epithelial Cells | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.2.5 | Crystal Field | Crystal | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.2.6 | Fungi Field | Fungi | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.2.7 | Bacteria Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.2.8 | Additional Microscopy Fields | Others | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.2.9 | Direct Culture Fields | Direct1 / Direct2 | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.2.10 | Microscopy Complete Flag | Complete | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.3 | Culture Flag Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.3.1 | Culture Verified Flag | Verified | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.3.2 | Culture Print Flag | Print | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.4 | Sensitivity Block | Sensitivity | Block | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.4.1 | Antibiotic List | — | List | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.4.2 | Antibiotic Add Action | Add | Action | Function | 1 | [VERIFIED] | In scope |
| 1.9.1.4.3 | Sensitivity Classification | — | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.4.3.1 | Highly For Option | Highly For | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.9.1.4.3.2 | Moderate For Option | Moderate For | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.9.1.4.3.3 | Low For Option | Low For | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.9.1.4.3.4 | Resistant For Option | Resistant For | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.9.1.4.4 | Report Element Visibility Control | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.9.1.5 | Culture Action Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.9.1.5.1 | Sensitivity Entry Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.9.1.5.2 | Save Culture Data Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.9.1.6 | Microbiology Report Output | MICROBIOLOGY REPORT | Output | Output | 1 | [VERIFIED] | In scope |
| **1.10** | **Patient Search** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.10.1 | Search Screen | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.10.1.1 | Search Criteria Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.10.1.1.1 | Search by Patient Name Exact | بحث باسم المريض (مطابق | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.2 | Search by Patient Name Partial | بحث باسم المريض (عشوائي | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.3 | Search by Treating Doctor | بحث باسم الطبيب المعالج | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.4 | Search by Request Date | بحث بتاريخ الطلب | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.5 | Search by Test Within Period | بالتحليل في الفترة المحددة | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.6 | Search by Phone Number | بالتليفون | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.7 | Search by Card Number | برقم البطاقة | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.8 | Search by Patient Age | بحث بسن المريض | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.9 | Search by Patient Gender | بحث بجنس المريض | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.10 | Search Within Branch | Branch No. | Function | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.10.1.1.11 | Search for Results Not Entered | نتائج لم تدخل | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.12 | Search for Results Not Verified | نتائج لم تراجع | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.13 | Search for Results Not Printed | نتائج لم تطبع | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.14 | Search for Results Not Delivered | نتائج لم تسلم | Function | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.1.15 | Date Range Mode Options | مقيد / حر | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.10.1.2 | Search Scope Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.10.1.2.1 | Branch Selector | All | Field | UI element | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.10.1.2.2 | Source Selector | Database / Backup | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.10.1.3 | Search Result Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.10.1.3.1 | Search Result Grid | — | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.10.1.3.2 | Search Result Count | عدد مرضى عملية البحث | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.10.1.3.3 | New Search Action | New search | Action | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.3.4 | Enter ID Field | <<Enter ID>> | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.10.1.4 | Search Action Bar | — | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.10.1.4.1 | Patient Tests Action | تحاليل المريض | Action | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.4.2 | Search Patient Data Action | بيانات المريض | Action | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.4.3 | Search Lab Results Action | نتائج التحاليل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.4.4 | Delete Patient Action | حذف المريض | Action | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.4.5 | Open Account Action | حساب مفتوح | Action | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.4.6 | Restrict To VIP Action | VIP | Action | Function | 1 | [UNVERIFIED] | In scope |
| 1.10.1.4.7 | Add Action | اضافة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.10.1.4.8 | Collected Results Action | نتائج مجمعة | Action | Function | 1 | [UNVERIFIED] | In scope |
| 1.10.1.4.9 | Modify Group Action | — | Action | Function | 1 | [UNVERIFIED] | In scope |
| 1.10.1.4.10 | Search Main List Action | القائمة الرئيسية | Action | Subfunction | 1 | [VERIFIED] | In scope |
| **1.11** | **Patient Visit History** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.11.1 | Laboratory ID Lifecycle | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.11.1.1 | Laboratory ID Generation | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.11.1.2 | Laboratory ID Storage | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.11.1.3 | Laboratory ID Display at Next Registration | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.11.2 | Visit History | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.11.2.1 | Search by Laboratory ID | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.11.2.2 | Visit List Grid | — | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.11.2.3 | Visit Tests List | — | List | UI element | 1 | [VERIFIED] | In scope |
| **1.12** | **Case Follow-up and Audit Trail** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.12.1 | Case History Screen | — | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.12.1.1 | Registration Audit Entry | تم تسجيل المريض بواسطة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.2 | Modification Count | عدد مرات التعديل | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.3 | Last Modifier Entry | اخر تعديل تم بواسطة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.4 | Result Entry Audit Entry | سجل نتيجة التحليل | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.5 | Verification Audit Entry | تمت المراجعة بواسطة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.6 | Printing Count | عدد مرات الطباعة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.7 | Last Printer Entry | اخر طباعة تمت بواسطة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.8 | Delivery Audit Entry | تم تسليم النتيجة بواسطة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.9 | Patient Samples Block | عينات المريض | Block | Container | 1 | [VERIFIED] | In scope |
| 1.12.1.9.1 | Tube Name Column | Tube Name | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.9.2 | Sample Tests Column | Tests | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.12.1.9.3 | Sample Take Action | Take | Action | Function | 1 | [VERIFIED] | In scope |
| **1.13** | **Site Result Delivery** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.13.1 | Upload Screen | — | Screen | Container | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.1.1 | Upload Patient Tests List | — | List | UI element | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.1.2 | Upload Action | — | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.1.3 | Select All Action | — | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.1.4 | Unselect All Action | — | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.1.5 | Upload Progress Indicator | — | Field | UI element | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.1.6 | Block Result Action | — | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.1.7 | Mobile Number Requirement | — | Rule | Rule | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.2 | Uploaded Patient Count | — | Function | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.3 | Site Access Channels | — | Function | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.4 | Site Account Types | — | Field group | Container | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.4.1 | Patient Account Type | — | Option | Subfunction | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.4.2 | Doctor Account Type | — | Option | Subfunction | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.4.3 | Laboratory Account Type | — | Option | Subfunction | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.5 | Site Sign-In Block | — | Block | Container | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.5.1 | Patient Sign-In Data | — | Rule | Rule | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.5.2 | Doctor Or Laboratory Sign-In Data | — | Rule | Rule | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.5.3 | Site Sign In Action | — | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.6 | Site Result View | — | Block | Container | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.6.1 | Result View Action | — | Action | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.6.2 | Result Export Formats | — | Function | Function | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.6.2.1 | Print Format | — | Format | Output | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.6.2.2 | Word Document Format | — | Format | Output | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.6.2.3 | PDF Format | — | Format | Output | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.6.2.4 | Image Format | — | Format | Output | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.13.7 | Site Access Data On Receipt | — | Output | Output | 1 | [VERIFIED] | OUT OF SCOPE |
| **1.14** | **Barcode Labels and Specimen Containers** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.14.1 | Barcode Label Kinds | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.14.1.1 | Specimen Container Label | ملصقات الانابيب والعبوات والمسحات | Output | Output | 1 | [VERIFIED] | In scope |
| 1.14.1.2 | File Barcode Label | File barcode | Output | Output | 1 | [VERIFIED] | In scope |
| 1.14.1.3 | Laboratory ID Barcode Label | Lab ID barcode | Output | Output | 1 | [VERIFIED] | In scope |
| 1.14.1.4 | Site Access Label | Site Access Label | Output | Output | 1 | [VERIFIED] | OUT OF SCOPE |
| 1.14.2 | Specimen Container Label Content | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.14.2.1 | Patient Name on Label | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.14.2.2 | Sex and Age Line on Label | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.14.2.3 | Allocated Test List on Label | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.14.2.4 | Label Barcode | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.14.2.5 | Laboratory Identifier on Label | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.14.2.6 | Container Designation on Label | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.14.2.7 | File Barcode Number | — | Field | UI element | 1 | [UNVERIFIED] | In scope |
| 1.14.2.8 | Label Print Timestamp | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.14.3 | Specimen Container Designations | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.14.3.1 | Serum Fasting Container | Serum Fasting | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.14.3.2 | Serum Postprandial Container | Serum Postprandial | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.14.3.3 | EDTA Blood Container | EDTA Blood | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.14.3.4 | Citrated Blood Container | Citrated Blood 0.4 | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.14.3.5 | Urine Container | Urine | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.14.3.6 | Stool Container | Stool | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.14.3.7 | Semen Container | Semen | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.14.4 | Test Distribution Across Containers | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.14.4.1 | Automatic Test Sorting | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.14.4.2 | Container Distribution Edit | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.14.4.3 | Vertical Coding on Printed Documents | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.14.4.4 | Repeated Sticker Printing | — | Function | Function | 1 | [VERIFIED] | In scope |
| **1.15** | **Patient Medical History and History Report** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.15.1 | Automatic History Insertion | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.15.1.1 | Automatic Previous Result Insertion | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.15.1.2 | Multi-Test History Insertion | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.15.1.3 | History Patient Identification | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.15.1.3.1 | Identify By Patient Name | فرز وتجميع التاريخ المرضي باسم المريض | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.15.1.3.2 | Identify By Laboratory Code | فرز وتجميع التاريخ المرضي حسب كود المعمل | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.15.1.4 | Automatic History Display Setting | Print Patient History Automatically | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.15.1.5 | History Sorting Method Setting | طريقة فرز وتجميع التاريخ المرضي | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.15.2 | History Test Selection | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.15.2.1 | Previous Visit Test List | — | List | UI element | 1 | [VERIFIED] | In scope |
| 1.15.2.2 | Previous Test Selection Action | — | Action | Function | 1 | [VERIFIED] | In scope |
| 1.15.2.3 | Previous Visit Count Indicator | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.15.2.4 | Repeated Test Note | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.15.2.5 | Previous Result Grid | Result status \ ·  Test result \ ·  Analysis date | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.15.3 | History Output Mode | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.15.3.1 | Attach History In Result Report Option | Attach history in result report | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.15.3.2 | Print History In Separate Report Option | Print history in separate report | Option | Subfunction | 1 | [VERIFIED] | In scope |
| 1.15.3.3 | History Mode Confirm Action | موافق | Action | Function | 1 | [VERIFIED] | In scope |
| 1.15.3.4 | History Mode Cancel Action | الغاء الحالى | Action | Function | 1 | [VERIFIED] | In scope |
| 1.15.4 | Patient History Report | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.15.4.1 | History Report Header | Patient History | Field group | Container | 1 | [VERIFIED] | In scope |
| 1.15.4.1.1 | History Report Title | Patient History | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.1.2 | History Date Range Field | History from ... to ... | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.1.3 | History Printed By Entry | Printed by | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.1.4 | History Printed In Entry | Printed In | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.2 | History Report Grid | Analysis Date \ ·  Patient ID \ ·  Patient Name | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.2.1 | History Analysis Date Column | Analysis Date | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.2.2 | History Patient ID Column | Patient ID | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.2.3 | History Patient Name Column | Patient Name | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.2.4 | History Test Result Column | — | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.3 | Multi-Patient History Panel | No Of Patients | Panel | Container | 1 | [VERIFIED] | In scope |
| 1.15.4.3.1 | Multi-Patient History Grid | Patient Name \ ·  Patient ID \ ·  Analysis Date | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.3.2 | Multi-Patient History Count | No Of Patients | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.4 | History Test Lists | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.15.4.4.1 | History Tests In Lab List | Patient tests in our lab. | List | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.4.2 | History Tests Available In Report List | Available tests in report | List | UI element | 1 | [VERIFIED] | In scope |
| 1.15.4.5 | Delete All History Action | حذف الكل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.15.4.6 | History Report Print Action | طباعة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.15.4.7 | History Report Preview Action | معاينة طباعة | Action | Function | 1 | [VERIFIED] | In scope |
| **1.16** | **Main Tools and Utilities** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.16.1 | Main Tools Window | نافذة الأدوات الرئيسية | Screen | Container | 1 | [VERIFIED] | In scope |
| 1.16.1.1 | Tools Action Band | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.16.1.1.1 | Test Library Action | مكتبة التحاليل | Action | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.1.2 | Image Library Action | مكتبة الصور | Action | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.1.3 | Abbreviations Dictionary Action | قاموس للإختصارات | Action | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.1.4 | Stopwatch Action | ساعة التوقيت Stopwatch | Action | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.1.5 | Unit Converter Action | محول وحدات القياس | Action | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.1.6 | Work And Laboratory List Action | قائمة الأمثلة والمختبرات | Action | Function | 1 | [UNVERIFIED] | In scope |
| 1.16.1.1.7 | Reference Weight Tables Action | توزيع الأوزان المرجعية | Action | Function | 1 | [UNVERIFIED] | In scope |
| 1.16.1.1.8 | Data Manual Action | دليل البيانات | Action | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.1.9 | Calculator Action | آلة الحاسبة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.2 | Tools Capability Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.16.1.2.1 | Test And Normal Ranges Reference Capability | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.2.2 | Clinical Terminology Capability | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.2.3 | Work Request Entry Capability | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.2.4 | Stopwatch Calculator And Converter Capability | — | Function | Function | 1 | [VERIFIED] | In scope |
| 1.16.1.2.5 | Reminder Message Capability | — | Function | Function | 1 | [UNVERIFIED] | In scope |
| 1.16.1.2.6 | Data Manual Search Capability | — | Function | Function | 1 | [UNVERIFIED] | In scope |
| **1.17** | **Foreign Report** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.17.1 | Foreign Report Window | — | Screen | Function | 1 | [NOT-SHOWN] | In scope |
| 1.17.2 | Foreign Report Window Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.17.3 | Foreign Report Sheet Output | — | Output | Output | 1 | [NOT-SHOWN] | In scope |
| **1.18** | **Printed Delivery Sheet and Order Slip** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.18.1 | Delivery Sheet Header Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.18.1.1 | Delivery Sheet Patient Id Field | ID | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.2 | Delivery Sheet Patient Name Field | Pat. Name | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.3 | Delivery Sheet Age Sex Field | Age / Sex | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.4 | Delivery Sheet Telephone Field | Telephone | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.5 | Delivery Sheet Referred By Field | Referred by | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.6 | Delivery Sheet Date Field | Date | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.7 | Delivery Sheet Time Field | Time | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.8 | Delivery Sheet Total Field | Total | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.9 | Delivery Sheet Paid Field | Paid | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.1.10 | Delivery Sheet Charge Field | Charge | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.2 | Delivery Sheet Test Grid | Test Name  ·  Unit  ·  Result  ·  Collection notes  ·  Sig. | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.18.2.1 | Delivery Sheet Test Name Column | Test Name | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.18.2.2 | Delivery Sheet Department Column | Unit | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.18.2.3 | Delivery Sheet Result Column | Result | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.18.2.4 | Delivery Sheet Collection Notes Column | Collection notes | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.18.2.5 | Delivery Sheet Signature Column | Sig. | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.18.3 | Delivery Sheet Signature Roles | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.18.3.1 | Samples Responsible Role | مسئول العينات | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.3.2 | Reception Staff Role | موظف الاستقبال | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.18.4 | Per Patient Order Slip Printing | — | Function | Function | 1 | [VERIFIED] | In scope |
| **1.19** | **Printed Result Report Structure** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.19.1 | Report Summary Header Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.19.1.1 | Report Header Patient Name Field | اسم المريض | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.19.1.2 | Report Header Net Total Field | الإجمالي بعد الخصم | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.19.1.3 | Report Header Amount Paid Field | قيمة وقدرها | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.19.1.4 | Report Header Remainder Field | الباقي للمريض | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.19.1.5 | Report Header Treating Doctor Field | الطبيب المعالج | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.19.1.6 | Report Header Result Receipt Date Field | تاريخ استلام النتيجة | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.19.1.7 | Report Header Account Type Field | نوع الحساب | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.19.1.8 | Report Header Patient Code Field | كود المريض | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.19.2 | Report Test Group Table | Test Group  ·  No. Of test  ·  Price | Grid | UI element | 1 | [VERIFIED] | In scope |
| 1.19.2.1 | Report Test Group Column | Test Group | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.19.2.2 | Report Test Count Column | No. Of test | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.19.2.3 | Report Test Price Column | Price | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.19.3 | Report Paper Variants | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| **1.20** | **Profile Report Layout** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.20.1 | Report Patient Block | Patient Name  ·  Patient ID.  ·  Age / Sex  ·  Request Date  ·  Referred By  ·  Printed In | Block | Container | 1 | [VERIFIED] | In scope |
| 1.20.2 | Report Title Line | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.20.3 | Report Section Heading | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.20.4 | Report Sub Item Row | — | Grid row | UI element | 1 | [VERIFIED] | In scope |
| 1.20.5 | Report Reference Range Column | Reference Range | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.20.6 | Report Flag Column | L or H | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.20.7 | Report Emphasised Result | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.20.8 | Report Secondary Mark Cell | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.20.9 | Report Interpretation Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.20.10 | Report Signature Line | Doctor's signature: | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.20.11 | Report Closing Line | Thanks | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.20.12 | Report Laboratory Footer | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.20.13 | Report Pagination Line | Current Page 1 from 1 | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.20.14 | Report Header Band Option | — | Option | Subfunction | 1 | [VERIFIED] | In scope |
| **1.21** | **Report Behaviour Rules** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.21.1 | Patient Reference Override Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.21.2 | Normal Range Length Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.21.3 | Report Paper Choice Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.21.4 | Report Language Side Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.21.5 | Test Interpretation Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.21.6 | Report Chart And Drawing Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.21.7 | Report Second Page Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| 1.21.8 | Single Sheet Report Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| **1.22** | **Composite Test Entry Screen** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.22.1 | Composite Screen Test Banner | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.2 | Composite Screen Section Heading | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.3 | Composite Screen Item Row | — | Grid row | UI element | 1 | [VERIFIED] | In scope |
| 1.22.4 | Composite Screen Item Check Box | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.5 | Composite Screen Item Value List | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.6 | Composite Screen Unordered Item | — | Indicator | UI element | 1 | [VERIFIED] | In scope |
| 1.22.7 | Composite Screen Resolved Range | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.8 | Composite Screen Semi Quantitative Control | — | Field | UI element | 1 | [UNVERIFIED] | In scope |
| 1.22.9 | Composite Screen Second Result Row | — | Grid row | UI element | 1 | [VERIFIED] | In scope |
| 1.22.10 | Composite Screen Extra Value Field | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.11 | Composite Screen Comment Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.22.12 | Composite Screen Action Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.22.12.1 | Composite Screen Save Action | حفظ | Action | Function | 1 | [VERIFIED] | In scope |
| 1.22.12.2 | Composite Screen Print Action | طباعة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.22.12.3 | Composite Screen Preview Action | معاينة الطباعة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.22.12.4 | Composite Screen Review Action | مراجعة | Action | Function | 1 | [VERIFIED] | In scope |
| 1.22.12.5 | Composite Screen Main Result Action | النتيجة الرئيسية | Action | Function | 1 | [VERIFIED] | In scope |
| 1.22.13 | Blood Picture Entry Block | Blood Picture | Block | Container | 1 | [VERIFIED] | In scope |
| 1.22.13.1 | Blood Picture Calculated Index Row | Hct  ·  MCV  ·  MCH  ·  MCHC  ·  RDW  ·  hgb %  ·  Mentzer Index | Grid row | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.2 | Blood Picture Index Inclusion Mark | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.3 | Blood Picture Differential Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.22.13.3.1 | Blood Picture Differential Row | Neutrophil  ·  Lymphocyte  ·  Monocyte  ·  Eosinophil  ·  Basophil | Grid row | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.3.2 | Blood Picture Relative Count Column | Relative count | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.3.3 | Blood Picture Absolute Count Column | Absolute count | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.3.4 | Blood Picture Other Cells Row | Other Cells | Grid row | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.3.5 | Blood Picture Segmented And Band Rows | Segmented  ·  Band | Grid row | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.4 | Blood Picture Immature Cell Block | NRBC/100 WBC  ·  Promyelocytes  ·  Myelocytes  ·  Juvenile  ·  Blast | Block | Container | 1 | [VERIFIED] | In scope |
| 1.22.13.5 | Auto Comment Option | Enable auto comment | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.6 | Auto Comment Basis Option | By MCHC | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.22.13.7 | Constants Panel | CONSTANTS | Block | Container | 1 | [VERIFIED] | In scope |
| **1.23** | **Culture And Sensitivity Report** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.23.1 | Culture Report Examination Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.23.2 | Culture Report Culture Block | Sample  ·  Organism A  ·  Culture Condition  ·  Colony Count | Block | Container | 1 | [VERIFIED] | In scope |
| 1.23.3 | Culture Report Sensitivity Block | — | Block | Container | 1 | [VERIFIED] | In scope |
| 1.23.3.1 | Culture Report Sensitivity Heading | Highly For (Organism A)  ·  Moderate For (Organism A)  ·  Resistant For (Organism A) | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.23.3.2 | Culture Report Antibiotic Row | — | Grid row | UI element | 1 | [VERIFIED] | In scope |
| 1.23.3.3 | Culture Report Inhibition Zone Column | Inhibition zone | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.23.3.4 | Culture Report Commercial Name Column | Commercial Name | Grid column | UI element | 1 | [VERIFIED] | In scope |
| 1.23.4 | Culture Report Trade Name Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| **1.24** | **Semen Morphology Report Page** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.24.1 | Semen Motility Profile Block | Grade A  ·  Grade B  ·  Grade C  ·  Grade D  ·  Grade A+B | Block | Container | 1 | [VERIFIED] | In scope |
| 1.24.2 | Semen Motility Profile Chart | — | Output | Output | 1 | [VERIFIED] | In scope |
| 1.24.3 | Semen Normal Grade Line | Normal: Grade A > 25% or Grade (A+B) >= 50% | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.24.4 | Semen Dead Forms Field | Dead Forms In First Hour | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.24.5 | Semen Abnormal Forms Block | Head Defects  ·  Mid piece defects  ·  Tail defects  ·  Micro spermia | Block | Container | 1 | [VERIFIED] | In scope |
| 1.24.6 | Semen Abnormal Forms Chart | — | Output | Output | 1 | [VERIFIED] | In scope |
| 1.24.7 | Semen Morphology Drawing Panel | — | Output | Output | 1 | [VERIFIED] | In scope |
| **1.25** | **Image Report** | — | Group | Container | 1 | [VERIFIED] | In scope |
| 1.25.1 | Image Report Sheet | — | Output | Output | 1 | [VERIFIED] | In scope |
| 1.25.2 | Image Report Image Count | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.25.3 | Image Report Image | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.25.4 | Image Report Annotation | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.25.5 | Image Report Annotation Placement | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.25.6 | Image Report Annotation Size | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.25.7 | Image Report Annotation Colour | — | Field | UI element | 1 | [VERIFIED] | In scope |
| 1.25.8 | Image Report Entry Rule | — | Rule | Rule | 1 | [VERIFIED] | In scope |
| **2.1** | **Work Sheet Module Entry** | — | Group | Container | 2 | [VERIFIED] | In scope |
| 2.1.1 | Work Sheet Ribbon Item | Work sheet | Navigation | Subfunction | 2 | [VERIFIED] | In scope |
| 2.1.2 | Work Sheet Menu | — | Menu | UI element | 2 | [VERIFIED] | In scope |
| 2.1.2.1 | Work Sheet By Patient Names Item | ورقة عمل بأسماء المرضى | Menu item | Subfunction | 2 | [VERIFIED] | In scope |
| 2.1.2.2 | Work Sheet By Test Names Item | ورقة عمل بأسماء التحاليل (Log) | Menu item | Subfunction | 2 | [VERIFIED] | In scope |
| 2.1.3 | Work Sheet Capabilities | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.1.3.1 | Patient Period Work Sheet | — | Function | Function | 2 | [VERIFIED] | In scope |
| 2.1.3.2 | Test Period Work Sheet | — | Function | Function | 2 | [VERIFIED] | In scope |
| 2.1.3.3 | Work Group Work Sheet | — | Function | Function | 2 | [VERIFIED] | In scope |
| 2.1.3.4 | Test Period Classification | — | Function | Function | 2 | [VERIFIED] | In scope |
| 2.1.3.5 | Unfinished Only Selection | — | Function | Function | 2 | [VERIFIED] | In scope |
| **2.2** | **Work Sheet by Patient Names** | — | Group | Container | 2 | [VERIFIED] | In scope |
| 2.2.1 | Work Sheet by Patient Names Screen | ورقة عمل بأسماء المرضى | Screen | Container | 2 | [VERIFIED] | In scope |
| 2.2.1.1 | Patient Work Sheet Mode Options | — | Field group | Container | 2 | [VERIFIED] | In scope |
| 2.2.1.1.1 | Full Patient Names Sheet Option | ورقة عمل حسب أسماء مرضى الفترة المحددة (كاملة) | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.2.1.1.2 | Abbreviated Patient Names Sheet Option | ورقة عمل حسب أسماء مرضى الفترة المحددة (مختصرة) | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.2.1.2 | Patient Sheet Filter Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.2.1.2.1 | Patient Sheet Referring Entity Selector | جهة الإحالة | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.2.1.2.2 | Patient Sheet From Code Field | من كود رقم | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.2.1.2.3 | Patient Sheet To Code Field | الى كود رقم | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.2.1.2.4 | Patient Sheet From Date Field | من تاريخ | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.2.1.2.5 | Patient Sheet To Date Field | الى | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.2.1.3 | Patient Sheet List Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.2.1.3.1 | Patient Sheet Patient Grid | All \ ·  Patient ID \ ·  Patient Name | Grid | UI element | 2 | [VERIFIED] | In scope |
| 2.2.1.3.2 | Patient Sheet Patient Count | — | Indicator | UI element | 2 | [VERIFIED] | In scope |
| 2.2.1.3.3 | Patient Name Double-Click Removal Action | — | Action | Function | 2 | [VERIFIED] | In scope |
| 2.2.1.4 | Patient Sheet Print Option Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.2.1.4.1 | Patient Sheet Unfinished Tests Only Option | طباعة التحاليل الغير منتهية فقط (عند اختيار مجموعة مرضى) | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.2.1.4.2 | Print Selected Patient Sheet Only Option | طباعة ورقة العمل الخاصة بالمريض المحدد فقط | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.2.1.5 | Patient Sheet Action Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.2.1.5.1 | Patient Sheet Print Action | Print | Action | Function | 2 | [VERIFIED] | In scope |
| 2.2.1.5.2 | Patient Sheet Refresh Action | Refresh | Action | Function | 2 | [VERIFIED] | In scope |
| 2.2.1.5.3 | Patient Sheet Preview Action | معاينة ورقة العمل قبل الطباعة | Action | Function | 2 | [VERIFIED] | In scope |
| 2.2.1.5.4 | Patient Sheet Main List Action | القائمة الرئيسية | Action | Subfunction | 2 | [VERIFIED] | In scope |
| **2.3** | **Work Sheet by Test Names** | — | Group | Container | 2 | [VERIFIED] | In scope |
| 2.3.1 | Work Sheet by Test Names Screen | ورقة عمل بأسماء التحاليل | Screen | Container | 2 | [VERIFIED] | In scope |
| 2.3.1.1 | Test Work Sheet Mode Options | — | Field group | Container | 2 | [VERIFIED] | In scope |
| 2.3.1.1.1 | Selected Tests Sheet Option | ورقة عمل بأسماء التحاليل | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.3.1.1.2 | Period Test Names Sheet Option | ورقة عمل حسب أسماء تحاليل الفترة الزمنية المحددة | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.3.1.1.3 | Specific Test In Period Option | ورقة عمل حسب التحليل المحدد فى الفترة الزمنية المحددة | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.3.1.1.4 | Group Name Sheet Option | ورقة عمل بإسم مجموعة (Log) | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.3.1.2 | Test Sheet Filter Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.3.1.2.1 | Test Sheet Group Selector | المجموعة | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.3.1.2.2 | Test Sheet Referring Entity Selector | جهة الإحالة | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.3.1.2.3 | Test Sheet From Code Field | من كود رقم | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.3.1.2.4 | Test Sheet To Code Field | الى كود رقم | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.3.1.2.5 | Test Sheet From Date Field | من تاريخ | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.3.1.2.6 | Test Sheet To Date Field | الى | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.3.1.3 | Test Sheet List Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.3.1.3.1 | Test Sheet Test Grid | All \ ·  Test ID \ ·  Test Name | Grid | UI element | 2 | [VERIFIED] | In scope |
| 2.3.1.3.2 | Test Sheet Test Count | — | Indicator | UI element | 2 | [VERIFIED] | In scope |
| 2.3.1.3.3 | Test Name Double-Click Removal Action | — | Action | Function | 2 | [VERIFIED] | In scope |
| 2.3.1.4 | Test Sheet Print Option Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.3.1.4.1 | Test Sheet Unfinished Tests Only Option | طباعة التحاليل الغير منتهية فقط (عند عمل لوج او لجهة فقط) | Option | Subfunction | 2 | [VERIFIED] | In scope |
| 2.3.1.5 | Test Sheet Action Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.3.1.5.1 | Test Sheet Print Action | Print | Action | Function | 2 | [VERIFIED] | In scope |
| 2.3.1.5.2 | Test Sheet Refresh Action | Refresh | Action | Function | 2 | [VERIFIED] | In scope |
| 2.3.1.5.3 | Test Sheet Preview Action | معاينة ورقة العمل قبل الطباعة | Action | Function | 2 | [VERIFIED] | In scope |
| 2.3.1.5.4 | Period Test Classification Action | تصنيف تحاليل الفترة المحددة | Action | Function | 2 | [VERIFIED] | In scope |
| 2.3.1.5.5 | Test Sheet Main List Action | القائمة الرئيسية | Action | Subfunction | 2 | [VERIFIED] | In scope |
| **2.4** | **Period Test Classification** | — | Group | Container | 2 | [VERIFIED] | In scope |
| 2.4.1 | Period Test Classification Screen | تصنيف تحاليل الفترة المحددة | Screen | Container | 2 | [VERIFIED] | In scope |
| 2.4.1.1 | Classification Grid | Test Name \ ·  No. of Tests | Grid | UI element | 2 | [VERIFIED] | In scope |
| 2.4.1.1.1 | Classification Test Name Column | Test Name | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.4.1.1.2 | Classification Test Count Column | No. of Tests | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.4.1.2 | Classification Report Output | statistics | Output | Output | 2 | [VERIFIED] | In scope |
| **2.5** | **Work Sheet Documents** | — | Group | Container | 2 | [VERIFIED] | In scope |
| 2.5.1 | Common Document Elements | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.5.1.1 | Laboratory Header Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.5.1.1.1 | Laboratory Printed Header | — | Output | Output | 2 | [VERIFIED] | In scope |
| 2.5.1.2 | Printed By Entry | Printed By | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.1.3 | Print Timestamp | — | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.1.4 | Zoom Setting | Zoom 100% | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.1.5 | Page Count Entry | Pages: 1 | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2 | Work Paper by Patient ID | Work Paper | Output | Output | 2 | [VERIFIED] | In scope |
| 2.5.2.1 | Work Paper Patient Title | Work paper by Patient ID | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.2 | Patient Sheet Count Entry | No. of Patients | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.3 | Work Paper Patient ID Column | Patient ID | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.4 | Work Paper Patient Name Column | Patient Name | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.5 | Work Paper Gender Column | Gender | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.6 | Work Paper Age Column | Age | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.7 | Work Paper Date Column | Date | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.8 | Work Paper Referred By Column | Referred by | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.9 | Reception Officer Signature Area | موظف الاستقبال | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.10 | Samples Officer Signature Area | مسئول العينات | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.2.11 | Work Paper Patient Entry | Patient | Field | UI element | 2 | [UNVERIFIED] | In scope |
| 2.5.3 | Work Paper by Test Name | Work paper by test name | Output | Output | 2 | [VERIFIED] | In scope |
| 2.5.3.1 | Work Paper Test Title | Work paper by test name | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.3.2 | Test Type Count Entry | No. of type of tests | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.3.3 | Work Paper Test Code Column | Test Code | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.3.4 | Work Paper Test Name Column | Test Name | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.3.5 | Test Work Paper Patient Name Column | Patient Name | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.3.6 | Work Paper Result Column | Result | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.3.7 | Test Work Paper Patient ID Column | ID | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.3.8 | Patient Count For Test Entry | No. of Patient for this test | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4 | Work Paper for Selected Test | Work paper for selected test | Output | Output | 2 | [VERIFIED] | In scope |
| 2.5.4.1 | Selected Test Work Paper Title | Work paper for selected test | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.2 | Selected Test Code Entry | Test Code | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.3 | Selected Test Name Entry | Test Name | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.4 | Selected Test Patient ID Column | ID | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.5 | Selected Test Patient Name Column | Patient Name | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.6 | Selected Test Patient Result Column | Patient Result | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.7 | Selected Test Signature Column | Sig. | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.8 | Selected Test Patient Count Entry | No. of patient for this test | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.9 | Selected Test Reception Officer Signature Area | موظف الاستقبال | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.5.4.10 | Selected Test Samples Officer Signature Area | مسئول العينات | Field | UI element | 2 | [VERIFIED] | In scope |
| **2.6** | **Printed Work Paper Structure** | — | Group | Container | 2 | [VERIFIED] | In scope |
| 2.6.1 | Work Paper Print Header Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.6.1.1 | Work Paper Patient Count Field | No. of Patients | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.1.2 | Work Paper Printed By Field | Printed By | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.1.3 | Work Paper Printed At Field | — | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.2 | Work Paper Pagination Field | Current Page 1 from 1 | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3 | Work Paper Patient Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.6.3.1 | Work Paper Patient Name Field | Name | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3.2 | Work Paper Patient Gender Field | Gender | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3.3 | Work Paper Patient Age Field | Age | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3.4 | Work Paper Patient Age Unit Field | Years | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3.5 | Work Paper Patient Ref By Field | Ref. by | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3.6 | Work Paper Patient Total Field | Total | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3.7 | Work Paper Patient Paid Field | Paid | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3.8 | Work Paper Patient Barcode Field | ID | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.3.9 | Work Paper Patient Ok Column | OK | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.6.4 | Work Paper Test Block | — | Block | Container | 2 | [VERIFIED] | In scope |
| 2.6.4.1 | Work Paper Test Code Field | Test Code | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.4.2 | Work Paper Test Name Field | Test Name | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.4.3 | Work Paper Test Patient Count Field | No. of Patient for this test | Field | UI element | 2 | [VERIFIED] | In scope |
| 2.6.4.4 | Work Paper Test Patient Name Column | Patient Name | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.6.4.5 | Work Paper Test Result Column | Result | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.6.4.6 | Work Paper Test Id Column | ID | Grid column | UI element | 2 | [VERIFIED] | In scope |
| 2.6.5 | Work Paper First And Last Patient Rule | — | Rule | Rule | 2 | [VERIFIED] | In scope |
| 2.6.6 | Unfinished Tests Work Paper Option | — | Option | Subfunction | 2 | [VERIFIED] | In scope |
| **3.1** | **System Module Entry** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.1.1 | System Ribbon Item | System | Navigation | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2 | System Menu | — | Menu | UI element | 3 | [VERIFIED] | In scope |
| 3.1.2.1 | Test Data Item | بيانات التحاليل | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.2 | Test Units Item | Test Units | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.3 | Test Comments Item | Test Comments | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.4 | Test Groups Item | مجموعات التحاليل | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.5 | Custom Groups Item | Custom Groups | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.6 | Culture Antibiotics Item | Culture Antibiotics | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.7 | Barcode Types Item | Barcode Types | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.8 | External Entities And Labs Item | الجهات الخارجية والمعامل | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.9 | Contract Price Lists Item | قوائم أسعار التعاقدات | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.10 | Lab Branches Item | Lab. branches | Menu item | Subfunction | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.1.2.11 | Patient Titles And Definitions Item | القاب وتعريفات المرضي | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.12 | Patient Names And Phones Item | اسماء وتليفونات المرضي | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.13 | Lab Accounts Item | Lab. accounts | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.14 | Work Groups Log Item | (Log) مجموعات العمل | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.15 | Result Screen Account Item | Result screen account | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.16 | Test Price List Print Item | طباعة قائمة اسعار التحاليل | Menu item | Subfunction | 3 | [VERIFIED] | In scope |
| 3.1.2.17 | Lab Devices And Equipment Item | أجهزة ومعدات المختبر | Menu item | Subfunction | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.1.3 | System Menu Capabilities | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.1.3.1 | Test Data Capability | — | Function | Function | 3 | [VERIFIED] | In scope |
| 3.1.3.2 | Test Annotations Capability | — | Function | Function | 3 | [VERIFIED] | In scope |
| 3.1.3.3 | Counterparty Capability | — | Function | Function | 3 | [VERIFIED] | In scope |
| 3.1.3.4 | Equipment Capability | — | Function | Function | 3 | [VERIFIED] | In scope |
| **3.2** | **Test Data** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.2.1 | Test Data Screen | بيانات التحاليل | Screen | Container | 3 | [VERIFIED] | In scope |
| 3.2.1.1 | Test Search Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.2.1.1.1 | Search By Test Name Field | Search by test name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.1.2 | Search By Group Name Field | By group name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.1.3 | Search By Test ID Field | By test ID | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.1.4 | Test Count Indicator | Number Of Tests | Indicator | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2 | Test Grid Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1 | Test Grid | ID \ ·  Arrang \ ·  Group Name \ ·  Test Name \ ·  Pat. P \ ·  Lab P \ ·  Out Lab Name \ ·  Out P \ ·  Barcode \ ·  Routine | Grid | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.1 | Test Grid ID Column | ID | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.2 | Test Grid Arrange Column | Arrang | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.3 | Test Grid Group Name Column | Group Name | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.4 | Test Grid Test Name Column | Test Name | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.5 | Test Grid Patient Price Column | Pat. P | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.6 | Test Grid Lab Price Column | Lab P | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.7 | Test Grid Out Lab Name Column | Out Lab Name | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.8 | Test Grid Out Price Column | Out P | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.9 | Test Grid Barcode Column | Barcode | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.2.1.10 | Test Grid Routine Column | Routine | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3 | Test Information Block | Test Information | Block | Container | 3 | [VERIFIED] | In scope |
| 3.2.1.3.1 | Test Name Field | Test Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.2 | Test Code Field | Test Code | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.3 | Report Name Field | Report Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.4 | Bill Name Field | Bill Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.5 | History Name Field | History Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.6 | Arabic Name Field | Arabic Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.7 | Test Group Name Field | Group Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.8 | Test Branch Field | Branch | Field | UI element | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.2.1.3.9 | Test Log Group Field | Log Group | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.10 | Test Collection Field | Collection | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.11 | Test Time Field | Test time (Day) | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.12 | Test Arrange No Field | Arrange No. | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.13 | Reference Type Field | Reference type | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.14 | Patient Price Field | Patient price | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.15 | Lab To Lab Price Field | Lab to lab price | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.16 | Test Barcode Field | Barcode | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.17 | Test Barcode Name Field | Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.18 | Tube 1 Field | Tube 1 | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.19 | Tube 2 Field | Tube 2 | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.20 | Tube 3 Field | Tube 3 | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.21 | Sent Outside Lab Field | Sent outside Lab. | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.22 | Out Lab Name Field | Lab. Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.23 | Cost Price Field | Cost price | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.24 | Patient Question Field | Patient Question | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.25 | Routine Test Check Box | Routine test | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.26 | See Report Check Box | See Report | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.27 | Print With Other Check Box | Print with other | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.28 | Add With Group Check Box | Add with group | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.3.29 | Main Test Check Box | Main test | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.2.1.4 | Test Data Action Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.2.1.4.1 | Add Test Action | اضافة تحليل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.2.1.4.2 | Edit Test Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.2.1.4.3 | Save Test Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.2.1.4.4 | Undo Test Action | تراجع | Action | Function | 3 | [VERIFIED] | In scope |
| 3.2.1.4.5 | Test Data Delete Action | حذف | Action | Function | 3 | [VERIFIED] | In scope |
| 3.2.1.4.6 | Reference Values Action | القيم المرجعية | Action | Function | 3 | [VERIFIED] | In scope |
| 3.2.1.4.7 | Test Data Main List Action | القائمة الرئيسية | Action | Subfunction | 3 | [VERIFIED] | In scope |
| **3.3** | **Test Reference Values** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.3.1 | Reference Values Screen | القيم المرجعية | Screen | Container | 3 | [VERIFIED] | In scope |
| 3.3.1.1 | Reference Range Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.3.1.1.1 | Reference Gender Selector | — | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.1.2 | Reference Age Range Field | — | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.1.3 | Reference Minimum Field | — | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.1.4 | Reference Maximum Field | — | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.1.5 | Low Comment Field | low comment | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.1.6 | High Comment Field | High comment | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.2 | Reference Range Action Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.3.1.2.1 | Add Reference Range Action | إضافة مدى | Action | Function | 3 | [VERIFIED] | In scope |
| 3.3.1.2.2 | Edit Reference Range Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.3.1.2.3 | Save Reference Range Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.3.1.2.4 | Delete Reference Range Action | حذف | Action | Function | 3 | [VERIFIED] | In scope |
| 3.3.1.3 | Reference Panel Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.3.1.3.1 | Reference Panel Test Name Field | Test Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.2 | Reference Panel Report Name Field | Report Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.3 | Reference Panel History Name Field | History Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.4 | Normal Range Field | Normal range | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.5 | Test Unit Field | Test unit | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.6 | Low Limit Field | Low limit | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.7 | Low Flag Field | Low flag | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.8 | High Limit Field | High limit | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.9 | High Flag Field | High flag | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.3.1.3.10 | Update Reference Action | تحديث | Action | Function | 3 | [VERIFIED] | In scope |
| 3.3.1.3.11 | Reference Confirm Action | موافق | Action | Function | 3 | [VERIFIED] | In scope |
| 3.3.1.3.12 | Reference Undo Action | تراجع | Action | Function | 3 | [VERIFIED] | In scope |
| 3.3.1.4 | Reference Matching Rule | — | Rule | Rule | 3 | [VERIFIED] | In scope |
| **3.4** | **Contract Price Lists** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.4.1 | Contract Price List Screen | قوائم أسعار التعاقدات | Screen | Container | 3 | [VERIFIED] | In scope |
| 3.4.1.1 | Price List Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.4.1.1.1 | Price List Name Field | — | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.4.1.1.2 | Price List Add Action | اضافة | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.1.3 | Price List Add Confirm Action | Ok | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.1.4 | Price List Edit Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.1.5 | Price List Save Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.1.6 | Price List Delete Action | حذف | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.1.7 | Price List Delete Confirm Action | موافق | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.1.8 | Price List Print Action | طباعة القائمة | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.2 | Price List Test Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.4.1.2.1 | Price List Test Name Field | — | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.4.1.2.2 | Price List Test Price Field | — | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.4.1.2.3 | Price List Test Add Action | اضافة | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.2.4 | Price List Test Add Confirm Action | موافق | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.2.5 | Price List Test Edit Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.2.6 | Price List Test Save Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.4.1.2.7 | Price List Test Delete Action | حذف | Action | Function | 3 | [VERIFIED] | In scope |
| **3.5** | **Test Comments** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.5.1 | Test Comments Screen | Test Comments | Screen | Container | 3 | [VERIFIED] | In scope |
| 3.5.1.1 | Test Comments Search Field | Search | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.5.1.2 | Test Comments Grid | ID \ ·  Test Name \ ·  Comment | Grid | UI element | 3 | [VERIFIED] | In scope |
| 3.5.1.2.1 | Test Comments ID Column | ID | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.5.1.2.2 | Test Comments Test Name Column | Test Name | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.5.1.2.3 | Test Comments Comment Column | Comment | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.5.1.3 | Test Comment Entry Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.5.1.3.1 | Test Comment Test Name Field | Test name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.5.1.3.2 | Test Comment Text Field | Comment | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.5.1.4 | Test Comments Action Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.5.1.4.1 | Add Test Comment Action | إضافة | Action | Function | 3 | [VERIFIED] | In scope |
| 3.5.1.4.2 | Save Test Comment Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.5.1.4.3 | Edit Test Comment Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.5.1.4.4 | Delete Test Comment Action | حذف | Action | Function | 3 | [VERIFIED] | In scope |
| 3.5.1.4.5 | Test Comments Main List Action | القائمة الرئيسية | Action | Subfunction | 3 | [VERIFIED] | In scope |
| **3.6** | **Test Groups** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.6.1 | Test Groups Screen | مجموعات التحاليل | Screen | Container | 3 | [VERIFIED] | In scope |
| 3.6.1.1 | Test Groups Count Indicator | عدد المجموعات المسجلة | Indicator | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.2 | Test Groups Grid | Test ID \ ·  Test Name \ ·  Price | Grid | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.2.1 | Test Groups Test ID Column | Test ID | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.2.2 | Test Groups Test Name Column | Test Name | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.2.3 | Test Groups Price Column | Price | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.3 | Test Group Entry Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.6.1.3.1 | Group Name Field | Group Name: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.3.2 | Group Test Name Field | Test Name: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.3.3 | Group Test Price Field | Price: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.3.4 | Tests In Group Count | No. of tests in group | Indicator | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.3.5 | Total Group Price Indicator | Total group Price | Indicator | UI element | 3 | [VERIFIED] | In scope |
| 3.6.1.4 | Test Groups Action Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.6.1.4.1 | Add Test Group Action | إضافة مجموعة | Action | Function | 3 | [VERIFIED] | In scope |
| 3.6.1.4.2 | Save Test Group Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.6.1.4.3 | Edit Test Group Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.6.1.4.4 | Delete Test Group Action | حذف مجموعة | Action | Function | 3 | [VERIFIED] | In scope |
| 3.6.1.4.5 | Add Group Test Action | إضافة تحليل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.6.1.4.6 | Delete Group Test Action | حذف تحليل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.6.1.4.7 | Print Test Group List Action | طباعة القائمة | Action | Function | 3 | [VERIFIED] | In scope |
| 3.6.1.4.8 | Test Groups Undo Action | تراجع | Action | Function | 3 | [VERIFIED] | In scope |
| 3.6.1.4.9 | Test Groups Main List Action | القائمة الرئيسية | Action | Subfunction | 3 | [VERIFIED] | In scope |
| **3.7** | **External Entities and Laboratories** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.7.1 | External Entities Screen | الجهات الخارجية والمعامل | Screen | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.1 | Entity Count Indicator | عدد الجهات المسجلة | Indicator | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.2 | Entity Search Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.2.1 | Entity Search Field | بحث باسم الجهة | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.2.2 | Entity Search All Option | الكل | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.3 | Entity Grid | اسم الجهة \ ·  اسم المسئول \ ·  قائمة اسعار التعاقد | Grid | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.3.1 | Entity Grid Name Column | اسم الجهة | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.3.2 | Entity Grid Responsible Column | اسم المسئول | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.3.3 | Entity Grid Price List Column | قائمة اسعار التعاقد | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.4 | Entity Type Options | — | Field group | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.4.1 | Treating Doctor Entity Type | طبيب معالج | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.4.2 | Sent Samples Entity Type | عينات مرسلة | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.4.3 | Referring Or Contract Entity Type | جهة إحالة او تعاقد | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.5 | Entity Data Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.5.1 | Entity Capacity Field | الصفة: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.2 | Entity Name Field | اسم الجهة أو الهيئة: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.3 | Entity City Field | المدينة: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.4 | Entity Address Field | العنوان: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.5 | Entity Phone Field | تليفون: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.6 | Entity Mobile Field | موبيل: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.7 | Entity Fax Field | فاكس: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.8 | Entity Website Field | Web Site: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.9 | Entity Email Field | E-Mail: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.10 | Entity Responsible Name Field | اسم المسئول | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.5.11 | Entity Responsible Phone Field | تليفون المسئول | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.6 | Entity Pricing Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.6.1 | Entity Price List Selector | قائمة اسعار التعاقد مع الجهة | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.6.2 | Entity Price List Options | — | Field group | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.6.2.1 | Individual Price List Option | Individual | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.6.2.2 | Lab To Lab Price List Option | Lab To Lab | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.6.2.3 | Free Price List Option | Free | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.6.2.4 | Low Price List Option | lowpricelist | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.6.2.5 | Price Menu One Option | price menu1 | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.6.2.6 | Price Menu Two Option | price menu2 | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.7.1.6.3 | Discount And Commission Block | خصم وعمولات | Field group | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.6.3.1 | Entity Discount Percent Field | خصم % | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.6.3.2 | Entity Commission Percent Field | عمولة % | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.7 | Entity Site Credentials Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.7.1 | Entity Site Address Field | Entity Result Service Address | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.7.2 | Entity Site Password Field | PW | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.7.3 | Entity Site Identifier Field | ID | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.7.1.7.4 | Generate Entity Identifier Action | ID | Action | Function | 3 | [VERIFIED] | In scope |
| 3.7.1.8 | Entity Action Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.7.1.8.1 | Save Entity Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.7.1.8.2 | Edit Entity Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.7.1.8.3 | Delete Entity Action | حذف | Action | Function | 3 | [VERIFIED] | In scope |
| 3.7.1.8.4 | Entity Undo Action | تراجع | Action | Function | 3 | [VERIFIED] | In scope |
| 3.7.1.8.5 | Entity Main List Action | القائمة الرئيسية | Action | Subfunction | 3 | [VERIFIED] | In scope |
| **3.8** | **Culture Registration** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.8.1 | Culture Entry Slots | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.8.1.1 | Culture Slot Identifier Range | — | Rule | Rule | 3 | [VERIFIED] | In scope |
| 3.8.1.2 | Culture Group Name Selector | CULTURE AND SENSITIVITY | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.8.1.3 | Culture Edit Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.8.1.4 | Culture Save Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.8.1.5 | Culture Configuration File | Configuration settings | File | UI element | 3 | [UNVERIFIED] | In scope |
| **3.9** | **Culture Antibiotics** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.9.1 | Antibiotics Screen | Culture Antibiotics | Screen | Container | 3 | [VERIFIED] | In scope |
| 3.9.1.1 | Antibiotic Culture Selector | Culture | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.2 | International Antibiotic Option | International Antibiotic | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.3 | Antibiotic By Abbreviation Option | Antibiotic By Abbr. | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.4 | Antibiotics Count In Culture | Antibiotics No. in culture | Indicator | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.5 | Antibiotic Entry Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.9.1.5.1 | Antibiotic Symbol Field | Symb. | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.5.2 | Antibiotic Scientific Name Field | Scientific Name | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.5.3 | Antibiotic Sensitivity Field | Sensitivity | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.5.4 | Antibiotic Pregnant Field | Pregnant | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.5.5 | Antibiotic Children Field | Children | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.5.6 | Antibiotic Commercial Names Field | Commercial Names | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.9.1.6 | Antibiotic Action Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.9.1.6.1 | Add Antibiotic Action | — | Action | Function | 3 | [VERIFIED] | In scope |
| 3.9.1.6.2 | Save Antibiotic Action | حفظ | Action | Function | 3 | [VERIFIED] | In scope |
| 3.9.1.6.3 | Edit Antibiotic Action | تعديل | Action | Function | 3 | [VERIFIED] | In scope |
| 3.9.1.6.4 | Delete Antibiotic Action | حذف | Action | Function | 3 | [VERIFIED] | In scope |
| 3.9.1.6.5 | Antibiotic Print Action | Print | Action | Function | 3 | [VERIFIED] | In scope |
| 3.9.1.6.6 | Antibiotics Undo Action | تراجع | Action | Function | 3 | [VERIFIED] | In scope |
| 3.9.1.6.7 | Antibiotics Main List Action | القائمة_MAIN | Action | Subfunction | 3 | [VERIFIED] | In scope |
| **3.10** | **Samples Sent Outside** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.10.1 | Samples Sent Outside Procedure | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.10.1.1 | Sent Outside Test Selection Action | — | Action | Function | 3 | [VERIFIED] | In scope |
| 3.10.1.2 | Sent Outside Lab Selection | — | Function | Function | 3 | [VERIFIED] | In scope |
| 3.10.1.3 | Cost Price Recording | — | Function | Function | 3 | [VERIFIED] | In scope |
| 3.10.1.4 | Patient Price Recording | — | Function | Function | 3 | [VERIFIED] | In scope |
| **3.11** | **Result Service Accounts** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.11.1 | Doctor Account Creation | — | Block | Container | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.11.1.1 | Doctor Account Identifier Generation | — | Function | Function | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.11.1.2 | Doctor Account Password Setting | — | Function | Function | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.11.1.3 | Doctor Result Access | — | Function | Function | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.11.2 | Laboratory Account Creation | — | Block | Container | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.11.2.1 | Laboratory Contract Registration | — | Function | Function | 3 | [VERIFIED] | OUT OF SCOPE |
| 3.11.2.2 | Laboratory Result Access | — | Function | Function | 3 | [VERIFIED] | OUT OF SCOPE |
| **3.12** | **Reference Range List and Form Details** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.12.1 | Reference Range List Grid | Gender  ·  From  ·  To  ·  Age unit  ·  Reference range  ·  low limit  ·  high limit  ·  Test unit  ·  Low flag  ·  High flag | Grid | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.1 | Reference List Gender Column | Gender | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.2 | Reference List From Column | From | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.3 | Reference List To Column | To | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.4 | Reference List Age Unit Column | Age unit | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.5 | Reference List Range Column | Reference range | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.6 | Reference List Low Limit Column | low limit | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.7 | Reference List High Limit Column | high limit | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.8 | Reference List Test Unit Column | Test unit | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.9 | Reference List Low Flag Column | Low flag | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.1.10 | Reference List High Flag Column | High flag | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.12.2 | Reference Range Form Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.12.2.1 | Range For Selector | Range for | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.12.2.1.1 | Range For All Option | For all | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.12.2.1.2 | Range For By Sex Only Option | By sex only | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.12.2.1.3 | Range For By Sex And Age Option | By sex and age | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.12.2.1.4 | Range For By Age Only Option | By age only | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.12.2.2 | Range For Male Option | Male | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.12.2.3 | Range For Female Option | Female | Option | Subfunction | 3 | [VERIFIED] | In scope |
| 3.12.2.4 | Range Age From Field | Age from | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.12.2.5 | Range Age To Field | to | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.12.2.6 | Range Age Unit Selector | Age unit | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.12.2.7 | Reference Range Text Area | القيمة المرجعية | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.12.2.8 | Pregnant Only Flag | for pregnant only | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.12.2.9 | Reference Count Indicator | Number Of References | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.12.2.10 | Feed The Analysis Action | تغذية التحليل | Action | Function | 3 | [UNVERIFIED] | In scope |
| **3.13** | **Antibiotic List and Commercial Name Marks** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.13.1 | Culture Antibiotic List Grid | Symb.  ·  Scientific Name  ·  Sensitivity  ·  Pregnant  ·  Children | Grid | UI element | 3 | [VERIFIED] | In scope |
| 3.13.1.1 | Culture Antibiotic Symbol Column | Symb. | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.13.1.2 | Culture Antibiotic Scientific Name Column | Scientific Name | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.13.1.3 | Culture Antibiotic Sensitivity Column | Sensitivity | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.13.1.4 | Culture Antibiotic Pregnant Column | Pregnant | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.13.1.5 | Culture Antibiotic Children Column | Children | Grid column | UI element | 3 | [VERIFIED] | In scope |
| 3.13.2 | Commercial Name Print Flag | Print | Field | UI element | 3 | [VERIFIED] | In scope |
| **3.14** | **Test Group Screen Details** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.14.1 | Laboratory Test Group List | — | List | UI element | 3 | [VERIFIED] | In scope |
| 3.14.2 | Group Name List Field | Group Name: | Field | UI element | 3 | [VERIFIED] | In scope |
| 3.14.3 | Add Sub Group Action | اضافة مسجموعة | Action | Function | 3 | [VERIFIED] | In scope |
| 3.14.4 | Group Fixed Price Rule | — | Rule | Rule | 3 | [VERIFIED] | In scope |
| **3.15** | **System Window Further Entries** | — | Group | Container | 3 | [VERIFIED] | In scope |
| 3.15.1 | Purchase Lab Devices Item | شراء أجهزة المختبر | Menu item | Subfunction | 3 | [NOT-SHOWN] | In scope |
| 3.15.2 | Read Analysis Report Item | قراءة تقرير التحاليل | Menu item | Subfunction | 3 | [NOT-SHOWN] | In scope |
| 3.15.3 | Card And Identity Data Item | الكارنيه والبيانات المعرفية | Menu item | Subfunction | 3 | [NOT-SHOWN] | In scope |
| 3.15.4 | System Menu Rendering Difference Rule | — | Rule | Rule | 3 | [VERIFIED] | In scope |
| 3.15.5 | System Window Capability Block | — | Block | Container | 3 | [VERIFIED] | In scope |
| 3.15.5.1 | System Tests And Ranges Capability | — | Function | Function | 3 | [VERIFIED] | In scope |
| 3.15.5.2 | System Antibiotics Capability | — | Function | Function | 3 | [VERIFIED] | In scope |
| 3.15.5.3 | System Lists Capability | — | Function | Function | 3 | [VERIFIED] | In scope |
| 3.15.5.4 | System Devices Capability | — | Function | Function | 3 | [VERIFIED] | In scope |
| **4.1** | **Users Module Entry** | — | Group | Container | 4 | [VERIFIED] | In scope |
| 4.1.1 | Users Ribbon Item | Users | Navigation | Subfunction | 4 | [VERIFIED] | In scope |
| 4.1.2 | Users Menu | — | Menu | UI element | 4 | [VERIFIED] | In scope |
| 4.1.2.1 | Create Users Item | انشاء مستخدمين | Menu item | Subfunction | 4 | [VERIFIED] | In scope |
| 4.1.2.2 | Change Password Item | تغير كلمة المرور | Menu item | Subfunction | 4 | [NOT-SHOWN] | In scope |
| 4.1.2.3 | Login Detector Item | Login detector | Menu item | Subfunction | 4 | [VERIFIED] | In scope |
| 4.1.2.4 | Attendance And Departure Item | الحضور والإنصراف | Menu item | Subfunction | 4 | [VERIFIED] | In scope |
| 4.1.3 | Users Module Capabilities | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.1.3.1 | User Creation Capability | — | Function | Function | 4 | [VERIFIED] | In scope |
| 4.1.3.2 | User Maintenance Capability | — | Function | Function | 4 | [VERIFIED] | In scope |
| 4.1.3.3 | Attendance Capability | — | Function | Function | 4 | [VERIFIED] | In scope |
| 4.1.3.4 | User Movement Capability | — | Function | Function | 4 | [VERIFIED] | In scope |
| 4.1.4 | Second Password Dialog | — | Screen | Container | 4 | [VERIFIED] | In scope |
| 4.1.4.1 | Second Password Prompt | Please Enter Your Second Password. | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.1.4.2 | Second Password Field | System menu password | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.1.4.3 | Second Password Confirm Action | OK | Action | Function | 4 | [VERIFIED] | In scope |
| 4.1.4.4 | Second Password Cancel Action | Cancel | Action | Function | 4 | [VERIFIED] | In scope |
| 4.1.4.5 | Second Password Rule | — | Rule | Rule | 4 | [VERIFIED] | In scope |
| **4.2** | **User Permissions Screen** | — | Group | Container | 4 | [VERIFIED] | In scope |
| 4.2.1 | User Permissions Screen | — | Screen | Container | 4 | [VERIFIED] | In scope |
| 4.2.1.1 | User List Block | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.2.1.1.1 | User Count Indicator | Users No. | Indicator | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.1.2 | User List | — | List | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.2 | User Data Block | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.2.1.2.1 | User Name Field | اسم المستخدم | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.2.2 | User Password Field | كلمة المرور | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.2.3 | User Internal Password Field | كلمة مرور النوافذ الداخلية | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.2.4 | User Last Login Field | وقت آخر دخول | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.3 | Permission Scope Block | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.2.1.3.1 | Absolute Permission Option | صلاحية مطلقة (للمشرفين ومديرون النظام) | Option | Subfunction | 4 | [VERIFIED] | In scope |
| 4.2.1.3.2 | Limited Permission Option | صلاحية محدودة (المستخدمين النظام) | Option | Subfunction | 4 | [VERIFIED] | In scope |
| 4.2.1.3.3 | User Branch Selector | رقم الفرع المتاح للمستخدم التعامل معه | Field | UI element | 4 | [VERIFIED] | OUT OF SCOPE |
| 4.2.1.3.4 | Branch Selector Rule | — | Rule | Rule | 4 | [VERIFIED] | OUT OF SCOPE |
| 4.2.1.4 | Permission Block | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.2.1.4.1 | Patient Data Permission Check Box | اضافة وتعديل بيانات المرضى (طباعة وصل - طباعة بار كود) | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.2 | Result Entry Permission Check Box | ادخال وتعديل نتائج تحاليل المرضى | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.3 | Result Review Permission Check Box | مراجعة وتعديل نتائج تحاليل المرضى | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.4 | Result Print Permission Check Box | طباعة نتائج المرضى (عمل تاريخ مرضى وطباعتة - طباعة ظرف ... الخ) | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.5 | Balance Withholding Permission Check Box | عدم طباعة نتائج المرضى في حالة وجود باقي حساب | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.6 | Result Delivery Permission Check Box | صلاحية تسليم نتائج المرضى | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.7 | Discount Limit Check Box | حد الخصم المسموح للمستخدم اعطاءه للمريض | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.8 | Discount Limit Percent Field | % | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.9 | Work Sheet Print Permission Check Box | طباعة ورقة العمل و Log | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.10 | Patient Delete Permission Check Box | صلاحية حذف المرضى | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.11 | System Data Permission Check Box | تعديل في بيانات النظام والاعدادات مثل ( بيانات التحاليل والقيم الطبيعية - الباركود - الوحدات ... الخ) | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.12 | Accounts Permission Check Box | امكانية صرف وايداع نقدية محاسبة المرضى ومندوبين الجهات الخارجية (حسابات) | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.13 | Statistics Permission Check Box | امكانية عمل التقارير الاحصائية وطباعتها (الاحصائيات) | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.14 | Stocktaking Permission Check Box | الاطلاع على حساب الدرج والحسابات الاخري (جرد يومى - اسبوعى - شهرى - سنوى .... الخ ) | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.4.15 | Attendance Permission Check Box | تسجيل الحضور والانصراف | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.5 | Work Time Block | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.2.1.5.1 | Work Start Time Field | وقت بداية العمل للمستخدم | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.5.2 | Work End Time Field | وقت انتهاء العمل للمستخدم | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.5.3 | Rest Period Exists Option | هل توجد فترة راحة للموظف | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.5.4 | Rest Period Duration Field | زمن فترة الراحة بالساعات والدقائق | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.2.1.6 | User Action Block | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.2.1.6.1 | Add User Action | إضافة | Action | Function | 4 | [VERIFIED] | In scope |
| 4.2.1.6.2 | Edit User Action | تعديل | Action | Function | 4 | [VERIFIED] | In scope |
| 4.2.1.6.3 | Save User Action | حفظ | Action | Function | 4 | [VERIFIED] | In scope |
| 4.2.1.6.4 | Delete User Action | حذف | Action | Function | 4 | [VERIFIED] | In scope |
| 4.2.1.6.5 | User Undo Action | تراجع | Action | Function | 4 | [VERIFIED] | In scope |
| 4.2.1.6.6 | User Main List Action | القائمة الرئيسية | Action | Subfunction | 4 | [VERIFIED] | In scope |
| **4.3** | **Permission Enforcement** | — | Group | Container | 4 | [VERIFIED] | In scope |
| 4.3.1 | Permission Alert Dialog | — | Screen | Container | 4 | [VERIFIED] | In scope |
| 4.3.1.1 | Permission Alert Title | تنبيه | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.3.1.2 | Permission Alert Message | أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.3.1.3 | Permission Alert Confirm Action | OK | Action | Function | 4 | [VERIFIED] | In scope |
| 4.3.2 | First Login Notice | This first login for you | Indicator | UI element | 4 | [VERIFIED] | In scope |
| **4.4** | **Attendance and Departure** | — | Group | Container | 4 | [VERIFIED] | In scope |
| 4.4.1 | Attendance Screen | — | Screen | Container | 4 | [VERIFIED] | In scope |
| 4.4.1.1 | Register Attendance Action | تسجيل حضور | Action | Function | 4 | [VERIFIED] | In scope |
| 4.4.1.2 | Register Departure Action | تسجيل انصراف | Action | Function | 4 | [VERIFIED] | In scope |
| 4.4.1.3 | Start Rest Period Action | بدأ فترة الراحة | Action | Function | 4 | [VERIFIED] | In scope |
| 4.4.1.4 | End Rest Period Action | انهاء فترة الراحة | Action | Function | 4 | [VERIFIED] | In scope |
| 4.4.1.5 | Attendance Main List Action | القائمة الرئيسية | Action | Subfunction | 4 | [VERIFIED] | In scope |
| 4.4.2 | Attendance Working Rule | — | Rule | Rule | 4 | [VERIFIED] | In scope |
| **4.5** | **Login Detector** | — | Group | Container | 4 | [VERIFIED] | In scope |
| 4.5.1 | Login Detector Screen | Login Detector | Screen | Container | 4 | [VERIFIED] | In scope |
| 4.5.1.1 | Login Detector Filter Block | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.5.1.1.1 | Login From Date Field | من | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.5.1.1.2 | Login To Date Field | إلى | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.5.1.1.3 | Login Search Action | بحث | Action | Function | 4 | [VERIFIED] | In scope |
| 4.5.1.2 | Login Detector User Block | — | Block | Container | 4 | [VERIFIED] | In scope |
| 4.5.1.2.1 | Login Detector User List | المستخدمين | List | UI element | 4 | [VERIFIED] | In scope |
| 4.5.1.2.2 | Show Selected Action | عرض المحدد | Action | Function | 4 | [VERIFIED] | In scope |
| 4.5.1.2.3 | Show All Action | عرض الكل | Action | Function | 4 | [VERIFIED] | In scope |
| 4.5.1.2.4 | Delete Period Action | حذف الفترة | Action | Function | 4 | [VERIFIED] | In scope |
| 4.5.1.2.5 | Login Detector Main List Action | القائمة الرئيسية | Action | Subfunction | 4 | [VERIFIED] | In scope |
| 4.5.2 | Login Detector Report | — | Output | Output | 4 | [VERIFIED] | In scope |
| 4.5.2.1 | Login Detector Report Title | كشف تسجيل الدخول والخروج للمستخدم ... عن فترة محددة | Field | UI element | 4 | [VERIFIED] | In scope |
| 4.5.2.2 | Login Detector User Column | المستخدم | Grid column | UI element | 4 | [VERIFIED] | In scope |
| 4.5.2.3 | Login Detector Login Date Column | تاريخ الدخول | Grid column | UI element | 4 | [VERIFIED] | In scope |
| 4.5.2.4 | Login Detector Login Time Column | وقت الدخول | Grid column | UI element | 4 | [VERIFIED] | In scope |
| 4.5.2.5 | Login Detector Logout Date Column | تاريخ الخروج | Grid column | UI element | 4 | [VERIFIED] | In scope |
| 4.5.2.6 | Login Detector Logout Time Column | وقت الخروج | Grid column | UI element | 4 | [VERIFIED] | In scope |
| 4.5.2.7 | Login Detector Device Column | اسم الجهاز | Grid column | UI element | 4 | [VERIFIED] | In scope |
| **5.1** | **Statistics Module Entry** | — | Group | Container | 5 | [VERIFIED] | In scope |
| 5.1.1 | Statistics Ribbon Item | Statistics | Navigation | Subfunction | 5 | [VERIFIED] | In scope |
| 5.1.2 | Statistics Password Gate | — | Rule | Rule | 5 | [VERIFIED] | In scope |
| 5.1.3 | Statistics Menu | — | Menu | UI element | 5 | [VERIFIED] | In scope |
| 5.1.3.1 | Patient Count Statistics Item | احصائيات وفقا لعدد المرضي | Menu item | Subfunction | 5 | [VERIFIED] | In scope |
| 5.1.3.2 | Test Count Statistics Item | احصائيات وفقا لعدد التحاليل | Menu item | Subfunction | 5 | [VERIFIED] | In scope |
| 5.1.3.3 | Branch Statistics Item | احصائيات خاصة بفروع المعمل | Menu item | Subfunction | 5 | [NOT-SHOWN] | OUT OF SCOPE |
| 5.1.3.4 | Sent Samples Statistics Item | احصائيات العينات المرسلة | Menu item | Subfunction | 5 | [NOT-SHOWN] | In scope |
| 5.1.3.5 | Work Evaluation Statistics Item | احصائيات تقييم ومتابعة العمل | Menu item | Subfunction | 5 | [NOT-SHOWN] | In scope |
| 5.1.3.6 | Result Monitoring Item | متابعة ومراقبة النتائج | Menu item | Subfunction | 5 | [NOT-SHOWN] | In scope |
| 5.1.3.7 | Statistics Examples Item | أمثلة متنوعة على الإحصائيات المتاحة | Menu item | Subfunction | 5 | [NOT-SHOWN] | In scope |
| 5.1.4 | Statistics Capabilities | — | Block | Container | 5 | [VERIFIED] | In scope |
| 5.1.4.1 | Patient Statistics Capability | — | Function | Function | 5 | [VERIFIED] | In scope |
| 5.1.4.2 | Test Statistics Capability | — | Function | Function | 5 | [VERIFIED] | In scope |
| 5.1.4.3 | Sent Samples Statistics Capability | — | Function | Function | 5 | [VERIFIED] | In scope |
| 5.1.4.4 | Production Capacity Capability | — | Function | Function | 5 | [VERIFIED] | In scope |
| 5.1.5 | Numeric Summary Table | NUMBERS \ ·  TOTAL | Table | UI element | 5 | [SAMPLE] | In scope |
| 5.1.5.1 | Numeric Summary Row | NUMBERS | Grid column | UI element | 5 | [SAMPLE] | In scope |
| 5.1.5.2 | Numeric Summary Total Row | TOTAL | Grid column | UI element | 5 | [SAMPLE] | In scope |
| **5.2** | **Patient Statistics Screen** | — | Group | Container | 5 | [VERIFIED] | In scope |
| 5.2.1 | Patient Statistics Screen | نافذة الإحصائيات الخاصة بالمرضى | Screen | Container | 5 | [VERIFIED] | In scope |
| 5.2.1.1 | Patient Statistics Period Block | — | Block | Container | 5 | [VERIFIED] | In scope |
| 5.2.1.1.1 | Statistics Year Field | في سنة | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.2.1.1.2 | Statistics Month Field | في الشهر | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.2.1.1.3 | Statistics From Date Field | في الفترة من | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.2.1.1.4 | Statistics To Date Field | حتي | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.2.1.1.5 | Statistics Referring Entities Selector | جهات الإحالة | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.2.1.2 | Census By Year Block | الاحصاء حسب السنة المحددة | Block | Container | 5 | [VERIFIED] | In scope |
| 5.2.1.2.1 | Year By Months Census Option | احصاء اجمالي عدد المرضي (فرز بشهور السنة) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.2.2 | Year By Gender Census Option | احصاء اجمالي عدد المرضي (فرز بجنس المريض) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.2.3 | Year By Account Type Census Option | احصاء اجمالي عدد المرضي (فرز بنوع الحساب) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.2.4 | Year By Referring Entity Census Option | احصاء اجمالي عدد المرضي (فرز بجهة الإحالة) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.2.5 | Year By Chosen Entity Only Census Option | احصاء اجمالي عدد المرضي لجهة الإحالة المحددة فقط | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.3 | Census By Month Block | الاحصاء حسب الشهر المحدد | Block | Container | 5 | [VERIFIED] | In scope |
| 5.2.1.3.1 | Month By Days Census Option | احصاء اجمالي عدد المرضي (فرز بأيام الشهر) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.3.2 | Month By Gender Census Option | احصاء اجمالي عدد المرضي (فرز بجنس المريض) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.3.3 | Month By Account Type Census Option | احصاء اجمالي عدد المرضي (فرز بنوع الحساب) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.3.4 | Month By Referring Entity Census Option | احصاء اجمالي عدد المرضي (فرز بجهة الإحالة) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.3.5 | Month By Chosen Entity Only Census Option | احصاء اجمالي عدد المرضي لجهة الإحالة المحددة فقط | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.4 | Census By Period Block | الاحصاء حسب الفترة المحددة | Block | Container | 5 | [VERIFIED] | In scope |
| 5.2.1.4.1 | Period By Gender Census Option | احصاء اجمالي عدد المرضي (فرز بجنس المريض) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.4.2 | Period By Account Type Census Option | احصاء اجمالي عدد المرضي (فرز بنوع الحساب) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.4.3 | Period By Referring Entity Census Option | احصاء اجمالي عدد المرضي (فرز بجهة الإحالة) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.4.4 | Period By Chosen Entity Only Census Option | احصاء اجمالي عدد المرضي لجهة الإحالة المحددة فقط | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.2.1.5 | Patient Statistics Action Block | — | Block | Container | 5 | [VERIFIED] | In scope |
| 5.2.1.5.1 | Patient Statistics Main List Action | القائمة الرئيسية | Action | Subfunction | 5 | [VERIFIED] | In scope |
| **5.3** | **Patient Statistics Reports** | — | Group | Container | 5 | [VERIFIED] | In scope |
| 5.3.1 | Patient Report Common Elements | — | Block | Container | 5 | [VERIFIED] | In scope |
| 5.3.1.1 | Report Laboratory Header | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.3.1.2 | Report Printed In Entry | Printed In | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.1.3 | Report Printed By Entry | Printed By | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.1.4 | Report Zoom Setting | Zoom 100% | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.1.5 | Report Page Count Entry | Pages: 1 | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.2 | Patient Year By Months Report | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.3.2.1 | Year By Months Report Title | إحصاء اجمالي عدد المرضي في سنة محددة (فرز بشهور السنة) | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.2.2 | Year By Months Report Year Line | وذلك لعام | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.2.3 | Year By Months Report Grid | — | Grid | UI element | 5 | [VERIFIED] | In scope |
| 5.3.2.3.1 | Year By Months Month Column | الشهر | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.2.3.2 | Year By Months Patient Count Column | المرضى | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.2.3.3 | Year By Months Share Column | نسبة المريض | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.2.3.4 | Year By Months Amount Column | المبالغ المسددة | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.2.3.5 | Year By Months Total Row | الاجمالي | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.3 | Patient Gender Report | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.3.3.1 | Gender Report Title | إحصاء اجمالي عدد المرضي في السنة المحددة (فرز بجنس المريض) | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.3.2 | Gender Report Grid | — | Grid | UI element | 5 | [VERIFIED] | In scope |
| 5.3.3.2.1 | Gender Report Gender Column | جنس المريض | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.3.2.2 | Gender Report Patient Count Column | المرضى | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.3.2.3 | Gender Report Share Column | نسبة المريض | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.3.2.4 | Gender Report Amount Column | المبالغ المسددة | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.3.2.5 | Gender Report Total Row | الاجمالي | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.4 | Patient Month By Days Report | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.3.4.1 | Month By Days Report Title | إحصاء اجمالي عدد المرضي في شهر محدد (فرز بأيام الشهر) | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.4.2 | Month By Days Report Month Line | وذلك لشهر | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.4.3 | Month By Days Report Grid | — | Grid | UI element | 5 | [VERIFIED] | In scope |
| 5.3.4.3.1 | Month By Days Day Column | اليوم | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.4.3.2 | Month By Days Patient Count Column | عدد المرضى | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.4.3.3 | Month By Days Amount Column | المبالغ المسددة | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.5 | Patient Referring Entity Report | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.3.5.1 | Referring Entity Report Title | إحصاء اجمالي عدد المرضي في شهر محدد (فرز بجهة الإحالة) | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.3.5.2 | Referring Entity Report Grid | — | Grid | UI element | 5 | [VERIFIED] | In scope |
| 5.3.5.2.1 | Referring Entity Report Entity Column | جهة الإحالة | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.5.2.2 | Referring Entity Report Patient Count Column | عدد المرضى | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.3.5.2.3 | Referring Entity Report Amount Column | المبالغ المسددة | Grid column | UI element | 5 | [VERIFIED] | In scope |
| **5.4** | **Test Statistics Screen** | — | Group | Container | 5 | [VERIFIED] | In scope |
| 5.4.1 | Test Statistics Screen | نافذة الإحصائيات الخاصة بالتحاليل | Screen | Container | 5 | [VERIFIED] | In scope |
| 5.4.1.1 | Test Statistics Period Block | — | Block | Container | 5 | [VERIFIED] | In scope |
| 5.4.1.1.1 | Test Statistics Year Field | في سنة | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.4.1.1.2 | Test Statistics From Date Field | في الفترة من | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.4.1.1.3 | Test Statistics To Date Field | حتى | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.4.1.2 | Test Census By Year Block | الأحصاء حسب السنة المحددة | Block | Container | 5 | [VERIFIED] | In scope |
| 5.4.1.2.1 | All Tests By Months Census Option | إحصاء إجمالي عدد العينات لكافة التحاليل (فرز بشهور السنة) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.4.1.2.2 | All Tests By Names Census Option | إحصاء إجمالي عدد العينات لكافة التحاليل (فرز بأسماء التحاليل) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.4.1.2.3 | Test Group By Months Census Option | إحصاء إجمالي عدد عينات مجموعة تحاليل (فرز بشهور السنة) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.4.1.2.4 | Test Group By Names Census Option | إحصاء إجمالي عدد عينات مجموعة تحاليل (فرز بأسماء التحاليل) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.4.1.2.5 | Specific Test By Months Census Option | إحصاء إجمالي عدد العينات للتحليل المحدد (فرز بشهور السنة) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.4.1.2.6 | Referring Entity By Months Census Option | إحصاء إجمالي عدد عينات جهة إحالة (فرز بشهور السنة) | Option | Subfunction | 5 | [VERIFIED] | In scope |
| 5.4.1.3 | Test Statistics Action Block | — | Block | Container | 5 | [VERIFIED] | In scope |
| 5.4.1.3.1 | Test Statistics Main List Action | القائمة الرئيسية | Action | Subfunction | 5 | [VERIFIED] | In scope |
| **5.5** | **Test Statistics Reports** | — | Group | Container | 5 | [VERIFIED] | In scope |
| 5.5.1 | All Tests By Names Report | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.5.1.1 | All Tests By Names Report Title | إحصاء اجمالي عدد عينات التحاليل في السنة المحددة (فرز بأسماء التحاليل) | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.5.1.2 | All Tests By Names Report Year Line | وذلك لعام | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.5.1.3 | All Tests By Names Report Grid | نسبة المريض | Grid | UI element | 5 | [VERIFIED] | In scope |
| 5.5.1.3.1 | All Tests Report Test Name Column | اسم التحليل | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.5.1.3.2 | All Tests Report Sample Count Column | العينات | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.5.1.3.3 | All Tests Report Share Column | — | Grid column | UI element | 5 | [UNVERIFIED] | In scope |
| 5.5.1.3.4 | All Tests Report Amount Column | الاجمالي تقريباً | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.5.1.3.5 | All Tests Report Total Row | الإجمالي | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.5.2 | Test Group Request Rate Report | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.5.2.1 | Test Group Request Rate Report Title | إحصاء معدل طلب تحاليل مجموعة | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.5.2.2 | Test Group Request Rate Report Year Line | وذلك لعام | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.5.2.3 | Test Group Request Rate Report Grid | نسبة المريض | Grid | UI element | 5 | [VERIFIED] | In scope |
| 5.5.2.3.1 | Group Rate Report Test Name Column | اسم التحليل | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.5.2.3.2 | Group Rate Report Sample Count Column | العينات | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.5.2.3.3 | Group Rate Report Share Column | — | Grid column | UI element | 5 | [UNVERIFIED] | In scope |
| 5.5.2.3.4 | Group Rate Report Amount Column | الاجمالي تقريباً | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.5.2.3.5 | Group Rate Report Total Row | الإجمالي | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.5.3 | Test By Months Report | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.5.3.1 | Test By Months Report Grid | — | Grid | UI element | 5 | [UNVERIFIED] | In scope |
| **5.6** | **Statistics Report Charts and Preview** | — | Group | Container | 5 | [VERIFIED] | In scope |
| 5.6.1 | Statistics Report Chart Block | — | Block | Container | 5 | [VERIFIED] | In scope |
| 5.6.1.1 | Pie Chart Output | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.6.1.2 | Bar Chart Output | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.6.1.3 | Line Chart Output | — | Output | Output | 5 | [VERIFIED] | In scope |
| 5.6.2 | Statistics Report Preview Window | — | Screen | Container | 5 | [VERIFIED] | In scope |
| 5.6.2.1 | Report Preview Zoom Selector | Zoom 100% | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.6.2.2 | Report Preview Print Action | — | Action | Function | 5 | [VERIFIED] | In scope |
| 5.6.2.3 | Report Preview Save Action | — | Action | Function | 5 | [VERIFIED] | In scope |
| **5.7** | **Result Review and Follow-up Report** | — | Group | Container | 5 | [VERIFIED] | In scope |
| 5.7.1 | Result Review Report Screen | تقرير متابعة ومراجعة النتائج | Screen | Container | 5 | [VERIFIED] | In scope |
| 5.7.1.1 | Result Review Report Grid | اليوم  ·  اسم المريض  ·  بيانات المريض  ·  جهة الاحالة  ·  التحليل  ·  النتيجة  ·  الحالة | Grid | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.1.1 | Result Review Date Column | اليوم | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.1.2 | Result Review Patient Name Column | اسم المريض | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.1.3 | Result Review Patient Data Column | بيانات المريض | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.1.4 | Result Review Referring Entity Column | جهة الاحالة | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.1.5 | Result Review Test Column | التحليل | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.1.6 | Result Review Result Column | النتيجة | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.1.7 | Result Review Status Column | الحالة | Grid column | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.2 | Result Review Report Title | تقرير متابعة ومراجعة النتائج على الفترة من ... الى ... | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.3 | Result Review Test Selector | — | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.4 | Result Review Lower Value Field | — | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.7.1.5 | Result Review Upper Value Field | — | Field | UI element | 5 | [VERIFIED] | In scope |
| 5.7.2 | Result Review Rule | — | Rule | Rule | 5 | [VERIFIED] | In scope |
| **6.1** | **Accounts Module Entry** | — | Group | Container | 6 | [VERIFIED] | In scope |
| 6.1.1 | Accounts Ribbon Item | Accounts | Navigation | Subfunction | 6 | [VERIFIED] | In scope |
| 6.1.2 | Accounts Menu | — | Menu | UI element | 6 | [VERIFIED] | In scope |
| 6.1.2.1 | Inventory And Stocktaking Item | الجرد وحساب الدرج | Menu item | Subfunction | 6 | [VERIFIED] | In scope |
| 6.1.2.2 | Sent Samples Outside Item | العينات المرسلة للخارج | Menu item | Subfunction | 6 | [VERIFIED] | In scope |
| 6.1.2.3 | Cash Deposit And Withdrawal Item | صرف وإيداع نقدية | Menu item | Subfunction | 6 | [NOT-SHOWN] | In scope |
| 6.1.2.4 | Companies And Representatives Item | حساب شركات ومندوبين | Menu item | Subfunction | 6 | [NOT-SHOWN] | In scope |
| 6.1.3 | Accounts Capabilities | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.1.3.1 | Stocktaking Capability | — | Function | Function | 6 | [VERIFIED] | In scope |
| 6.1.3.2 | Counterparty Accounts Capability | — | Function | Function | 6 | [VERIFIED] | In scope |
| 6.1.3.3 | Sent Samples Capability | — | Function | Function | 6 | [VERIFIED] | In scope |
| 6.1.3.4 | Cash Movement Capability | — | Function | Function | 6 | [VERIFIED] | In scope |
| **6.2** | **Inventory and Stocktaking Account Screen** | — | Group | Container | 6 | [VERIFIED] | In scope |
| 6.2.1 | Inventory Account Screen | نافذة حساب الدرج والجرد اليومي والشهري | Screen | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.1 | Inventory Filter Block | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.1.1 | Inventory Branch Selector | Branch No. | Field | UI element | 6 | [VERIFIED] | OUT OF SCOPE |
| 6.2.1.1.2 | Inventory From Date Field | من | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.1.3 | Inventory To Date Field | الى | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.1.4 | Inventory Display Action | عرض | Action | Function | 6 | [VERIFIED] | In scope |
| 6.2.1.2 | Inventory Breakdown Block | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.2.1 | Users Breakdown Group | المستخدمين | Field group | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.2.1.1 | Users Detailed Option | مفصل | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.1.2 | Users Summary Option | مجمع | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.2 | Sent Samples Breakdown Group | عينات مرسلة | Field group | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.2.2.1 | Sent Samples Detailed By Result Option | مفصل | Option | Subfunction | 6 | [UNVERIFIED] | In scope |
| 6.2.1.2.2.2 | Sent Samples Detailed By Price Option | مفصل | Option | Subfunction | 6 | [UNVERIFIED] | In scope |
| 6.2.1.2.2.3 | Sent Samples Summary Option | مجمع | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.3 | Treating Doctor Breakdown Group | الطبيب المعالج | Field group | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.2.3.1 | Doctor Detailed By Result Option | مفصل بالنتائج | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.3.2 | Doctor Detailed By Price Option | مفصل بالاسعار | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.3.3 | Doctor Detailed Option | مفصل | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.3.4 | Doctor Summary Option | مجمع | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.4 | Referring Entity Breakdown Group | جهة الاحالة | Field group | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.2.4.1 | Entity Detailed By Result Option | مفصل بالنتائج | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.4.2 | Entity Detailed By Price Option | مفصل بالاسعار | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.4.3 | Entity Detailed Option | مفصل | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.4.4 | Entity Summary Option | مجمع | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.5 | Account Type Breakdown Group | نوع الحساب | Field group | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.2.5.1 | Account Type Individual Option | Individual | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.5.2 | Account Type Detailed By Result Option | مفصل بالنتائج | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.5.3 | Account Type Detailed By Price Option | مفصل بالاسعار | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.5.4 | Account Type Detailed Option | مفصل | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.2.5.5 | Account Type Summary Option | مجمع | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.1.3 | Account Figures Block | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.3.1 | Account Figure Operator Signs | = \ ·  + \ ·  - \ ·  * | Indicator | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.2 | Total Patient Samples Figure | اجمالي عينات المرضي | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.3 | Discount Value Figure | قيمة الخصومات | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.4 | Total After Discount Figure | الاجمالي بعد الخصم | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.5 | Amount Collected Figure | ماتم تحصيله | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.6 | Not Collected Figure | لم يحصل | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.7 | Sent Outside Samples Figure | العينات المرسلة للخارج | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.8 | Amount Paid Figure | ماتم سداده | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.9 | Unsettled Balance Figure | باقي لم يسدد | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.10 | Cash Deposits Figure | توريدات نقدية | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.11 | Cash Expenses Figure | مصروفات نقدية | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.12 | Available In Till Figure | المتاح في الخزينة | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.13 | Remainder For Patients Figure | باقي للمرضى | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.14 | Commissions And Percentages Figure | عمولات ونسب | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.3.15 | Net Profit Figure | صافي الربح بعد التحصيل والتسديد | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.1.4 | Inventory Action Block | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.2.1.4.1 | Inventory Main List Action | القائمة الرئيسية | Action | Subfunction | 6 | [VERIFIED] | In scope |
| 6.2.2 | Inventory Branch Rule | — | Rule | Rule | 6 | [VERIFIED] | OUT OF SCOPE |
| 6.2.3 | Inventory Breakdown Rule | — | Rule | Rule | 6 | [VERIFIED] | In scope |
| 6.2.4 | Patient Account Detail Report | كشف حساب المرضي | Output | Output | 6 | [VERIFIED] | In scope |
| 6.2.4.1 | Patient Account Detail Report Title | كشف حساب اجمالي عينات المرضى فى الفتره من ... الى ... | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2 | Patient Account Detail Grid | — | Grid | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2.1 | Patient Account Request Date Column | تاريخ الطلب | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2.2 | Patient Account Patient Code Column | كود المريض | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2.3 | Patient Account Patient Name Column | اسم المريض | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2.4 | Patient Account Total Column | الاجمالى | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2.5 | Patient Account Discount Column | قيمة الخصم | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2.6 | Patient Account After Discount Column | بعد الخصم | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2.7 | Patient Account Paid Column | المدفوع | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.2.8 | Patient Account Balance Column | باقى الحساب | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.2.4.3 | Patient Account Detail Report Zoom | Zoom 100% | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.5 | Doctor Account Detail Report | — | Output | Output | 6 | [VERIFIED] | In scope |
| 6.2.5.1 | Doctor Account Detail Report Title | كشف حساب إجمالي المرضي المحولون من ... في الفترة من ... الى ... | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.2.5.2 | Doctor Account Detail Grid | — | Grid | UI element | 6 | [VERIFIED] | In scope |
| **6.3** | **Sent Samples Outside Account** | — | Group | Container | 6 | [VERIFIED] | In scope |
| 6.3.1 | Sent Samples Account Screen | نافذة حسابات العينات المرسلة للخارج | Screen | Container | 6 | [VERIFIED] | In scope |
| 6.3.1.1 | Sent Samples Filter Block | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.3.1.1.1 | Sent Samples From Date Field | من | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.1.2 | Sent Samples To Date Field | الى | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.1.3 | Sent Samples Status Option | الغير مرسل \ ·  الكل | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.1.4 | Sent Samples Display Action | عرض | Action | Function | 6 | [VERIFIED] | In scope |
| 6.3.1.2 | Sent Samples Summary Block | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.3.1.2.1 | Sent Samples Count Figure | عدد العينات | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.2.2 | Sent Samples Total Figure | الإجمالي | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.2.3 | Sent Samples Paid Figure | المدفوع | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.2.4 | Sent Samples Remaining Figure | الباقى | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.3 | Sent Samples Grid | Request Date \ ·  Test Name \ ·  Patient Name \ ·  Lab. Name \ ·  Cost \ ·  Paid \ ·  Send | Grid | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.3.1 | Sent Samples Request Date Column | Request Date | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.3.2 | Sent Samples Test Name Column | Test Name | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.3.3 | Sent Samples Patient Name Column | Patient Name | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.3.4 | Sent Samples Lab Name Column | Lab. Name | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.3.5 | Sent Samples Cost Column | Cost | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.3.6 | Sent Samples Paid Column | Paid | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.3.7 | Sent Samples Send Flag | Send | Grid column | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.4 | Sent Sample Entry Block | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.3.1.4.1 | Sent Sample Lab Or Entity Field | اسم المعمل أو الجهة: | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.4.2 | Sent Sample Send To Field | ترسل إلى | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.4.3 | Sent Sample Price Field | سعر التحليل: | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.4.4 | Sent Sample Paid Field | المدفوع: | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.4.5 | Sent Sample Sent Flag | أرسلت | Field | UI element | 6 | [VERIFIED] | In scope |
| 6.3.1.5 | Sent Samples Report Type Options | — | Field group | Container | 6 | [VERIFIED] | In scope |
| 6.3.1.5.1 | Chosen Entity Count Option | حصر نتيجة البحث للجهة المحددة | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.3.1.5.2 | Show Not Sent Option | عرض العينات التي لم ترسل | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.3.1.5.3 | Open Samples Account Option | حساب عينات مفتوح | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.3.1.5.4 | Test Sent Outside Option | تحليل يرسل للخارج | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.3.1.5.5 | Test Not Sent Option | تحليل لا يرسل | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.3.1.6 | Sent Samples Action Block | — | Block | Container | 6 | [VERIFIED] | In scope |
| 6.3.1.6.1 | Send To Action | ترسل إلي | Action | Function | 6 | [VERIFIED] | In scope |
| 6.3.1.6.2 | Sent Sample Confirm Action | موافق | Action | Function | 6 | [VERIFIED] | In scope |
| 6.3.1.6.3 | Sent Sample Net Action | خالص | Action | Function | 6 | [VERIFIED] | In scope |
| 6.3.1.6.4 | Sent Sample Ignore Action | اهمال | Action | Function | 6 | [VERIFIED] | In scope |
| 6.3.1.6.5 | Always To Action | دائماً إليه | Action | Function | 6 | [VERIFIED] | In scope |
| 6.3.1.6.6 | Sent Samples Main List Action | القائمة الرئيسية | Action | Subfunction | 6 | [VERIFIED] | In scope |
| 6.3.1.7 | Sent Sample Selection Rule | — | Rule | Rule | 6 | [VERIFIED] | In scope |
| **6.4** | **Cash and Counterparty Accounts** | — | Group | Container | 6 | [VERIFIED] | In scope |
| 6.4.1 | Cash Deposit And Withdrawal Screen | صرف وإيداع نقدية | Screen | Function | 6 | [NOT-SHOWN] | In scope |
| 6.4.2 | Companies And Representatives Account Screen | حساب شركات ومندوبين | Screen | Function | 6 | [NOT-SHOWN] | In scope |
| **6.5** | **Account Statement Variants and Claims** | — | Group | Container | 6 | [VERIFIED] | In scope |
| 6.5.1 | Account Statement Scope Options | — | Field group | Container | 6 | [VERIFIED] | In scope |
| 6.5.1.1 | Statement Of All Patients Option | — | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.5.1.2 | Statement Of Balances Only Option | — | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.5.1.3 | Statement Of Settled Only Option | — | Option | Subfunction | 6 | [VERIFIED] | In scope |
| 6.5.2 | Statement Period Rule | — | Rule | Rule | 6 | [VERIFIED] | In scope |
| 6.5.3 | Detailed Account Statement | — | Output | Output | 6 | [VERIFIED] | In scope |
| 6.5.4 | Detailed Account Statement By Results | — | Output | Output | 6 | [VERIFIED] | In scope |
| 6.5.5 | Claim Report | مطالبة | Output | Output | 6 | [VERIFIED] | In scope |
| 6.5.6 | Four Report Variants Rule | — | Rule | Rule | 6 | [VERIFIED] | In scope |
| 6.5.7 | Sent Samples Account Statement | — | Output | Output | 6 | [VERIFIED] | In scope |
| 6.5.8 | Sent Samples Statement Grid | اسم الجهة  ·  اسم التحليل  ·  تاريخ الطلب  ·  كود المريض  ·  اسم المريض  ·  سعر التحليل  ·  المسدد  ·  الباقي | Grid | UI element | 6 | [VERIFIED] | In scope |
| 6.5.9 | Stocktaking Figure Label Variant | — | Rule | Rule | 6 | [VERIFIED] | In scope |
| 6.5.10 | Accounts Window Items Rule | — | Rule | Rule | 6 | [VERIFIED] | In scope |
| **7.1** | **Settings Module Entry and System Settings Screen** | — | Group | Container | 7 | [VERIFIED] | In scope |
| 7.1.1 | Setting Ribbon Item | Setting | Navigation | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.2 | System Settings Item | اعدادات النظام | Menu item | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.3 | Database Maintenance Item | Database Maintenance | Menu item | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4 | System Settings Screen | نافذة اعدادات النظام | Screen | Container | 7 | [VERIFIED] | In scope |
| 7.1.4.1 | Settings Tab Strip | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.1.4.1.1 | System Tab | اعدادات النظام | Tab | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.1.2 | Report Tab | اعدادات التقرير | Tab | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.1.3 | Receipt Tab | اعدادات الإيصال | Tab | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.1.4 | Envelope Tab | اعدادات الظرف | Tab | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.1.5 | Card Tab | اعدادات الكارنية | Tab | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.1.6 | Database Server Tab | اعدادات خادم قواعد البيانات | Tab | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.2 | Session Information Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.1.4.2.1 | Server Type Indicator | SQL SERVER | Indicator | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.2.2 | Session User Name | User Name | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.2.3 | Session Last Login | Last Login | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.2.4 | Database Type Indicator | SQL DATABASE | Indicator | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.2.5 | Session Today | Today | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.3 | General Settings Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.1.4.3.1 | English Patient And Doctor Name Option | ادخال اسم المريض والطبيب المعالج باللغة الانجليزية وباقي العناصر | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.2 | Blank Referring Entity Default Option | في حالة ترك خانة جهة الاحالة فارغة يتم وضع herself, Himself حسب جنس المريض | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.3 | Keep Treating Doctor Option | حفظ الطبيب المعالج من نافذة الجهات الخارجية والمعامل وعدم حفظه تلقائياً عند ادخال المرضي | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.4 | Search While Entering Patient Name Option | تمكين خاصية البحث عند ادخال اسم المريض في نافذة اضافة وتعديل بيانات المرضي | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.5 | No Automatic Titles Option | عدم وضع الألقاب تلقائيا عند تسجيل بيانات المرضي | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.6 | Tube Barcode Date And Time Option | طباعة التاريخ والوقت على باركود الانابيب | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.7 | External File Barcode Option | طباعة الباركود الخارجي الخاص بالملف | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.8 | Lab Code Instead Of Patient Code Option | طباعة كود المعمل بدل كود المريض في تقرير النتائج واستكرز الباركود | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.9 | Automatic Tests Review Option | مراجعة وإكمال التحاليل بطريقة تلقائية | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.10 | Patient Account Instead Of Print Date Option | طباعة حساب المريض والباقي بدل تاريخ الطباعة في تقرير النتيجة | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.11 | File Or Card Number Instead Of Print Date Option | طباعة رقم الملف او البطاقة بدل تاريخ الطباعة في تقرير النتيجة | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.12 | Daily Database Backup Option | عمل نسخ احتياطي لقاعدة البيانات يومياً | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.3.13 | Backup Path Field | Path | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.3.14 | Backup Fallback Path Rule | * في حالة تفعيل الخاصية وعدم وجود المسار يتم الحفظ في المسار التالي | Rule | Rule | 7 | [VERIFIED] | In scope |
| 7.1.4.4 | Database Server Settings Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.1.4.4.1 | Server Name Field | اسم السيرفر | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.4.2 | Server Login Field | تسجيل الدخول | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.4.3 | Server Database Name Field | اسم قاعدة البيانات | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.1.4.5 | System Settings Action Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.1.4.5.1 | System Settings Save Action | حفظ الإعدادات | Action | Function | 7 | [VERIFIED] | In scope |
| 7.1.4.5.2 | System Settings Back Action | رجوع | Action | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.4.5.3 | System Settings Main List Action | القائمة الرئيسية | Action | Subfunction | 7 | [VERIFIED] | In scope |
| 7.1.5 | Database Maintenance Capabilities | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.1.5.1 | Basic System Settings Capability | تعديل اعدادات النظام الاساسية (وقت التسليم ، الطابعة المخصصة للباركود ، رقم الفرع .... ) | Function | Function | 7 | [VERIFIED] | In scope |
| 7.1.5.2 | Server Data Capability | تعديل بيانات السيرفر (اسم السيرفر وتسجيل الدخول واسم قاعدة البيانات) | Function | Function | 7 | [VERIFIED] | In scope |
| 7.1.5.3 | Backup And Restore Capability | عمل نسخة احتياطية من قاعدة بيانات النظام او استرجاع نسخة سابقة | Function | Function | 7 | [VERIFIED] | In scope |
| 7.1.5.4 | Update Capability | تحديث قاعدة البيانات وملفات البرنامج للتوافق مع النسخ الحديثة من البرنامج | Function | Function | 7 | [VERIFIED] | In scope |
| 7.1.5.5 | Prepare For Recent Version Capability | تهيئة النظام للنسخة الحديثة | Function | Function | 7 | [VERIFIED] | In scope |
| **7.2** | **Report Settings** | — | Group | Container | 7 | [VERIFIED] | In scope |
| 7.2.1 | Report Settings Tab | اعدادات التقرير | Screen | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.1 | Report Signature Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.1.1 | Report Signature Caption | Doctor's signature | Label | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.1.2 | Report Signature Text Field | Doctor's signature: | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.1.3 | Report Signature Font Selector | Font | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.1.3.1 | Signature Font Preset One | Font | Option | Subfunction | 7 | [UNVERIFIED] | In scope |
| 7.2.1.1.3.2 | Signature Font Preset Two | Font | Option | Subfunction | 7 | [UNVERIFIED] | In scope |
| 7.2.1.1.3.3 | Signature Font Preset Three | Font | Option | Subfunction | 7 | [UNVERIFIED] | In scope |
| 7.2.1.2 | Report Margins Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.2.1 | Page Margin Left Field | Left | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.2.2 | Page Margin Bottom Field | Bottom | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.2.3 | Report Top Space Field | Report top Space | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.3 | Report Paper Size Selector | حجم الورق المستخدم في طباعة التقارير | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.3.1 | Report Paper Size A4 Option | A4 (Large) | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.3.2 | Report Paper Size A5 Option | A5 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.4 | Patient Date Sorting Group | طريقة فرز وتجميع التاريخ المرضي | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.4.1 | Sort By Lab Code Option | فرز وتجميع التاريخ المرضي حسب كود المعمل | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.4.2 | Sort By Patient Name Option | فرز وتجميع التاريخ المرضي بإسم المريض | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.5 | Header Footer Printing Option Group | — | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.5.1 | Do Not Print Header Footer Option | عدم طباعة رأس وذيل التقرير (استخدام ورق مطبوع) | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.5.2 | Print Header Footer As Words Option | طباعة رأس وذيل التقرير علي هيئة كلمات منفصلة (استخدام ورق أبيض) | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.5.3 | Print Header Footer From Images Option | طباعة رأس وذيل التقرير من ملفات الصور المرفقة في البرنامج (استخدام ورق أبيض) | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.6 | Report Image Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.6.1 | Lab Logo Image Field | JPG Image 145 * 95 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.6.2 | Lab Logo Image Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.1.6.3 | Lab Logo Image Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.1.6.4 | Report Header Image Field | JPG Image 680 * 140 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.6.5 | Report Header Image Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.1.6.6 | Report Header Image Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.1.6.7 | Report Footer Image Field | JPG Image 680 * 50 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.6.8 | Report Footer Image Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.1.6.9 | Report Footer Image Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.1.7 | Report Text Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.7.1 | Lab Logo Name Text Area | شعار المعمل | Field | UI element | 7 | [SAMPLE] | In scope |
| 7.2.1.7.2 | Lab Logo Name Font Selector | Font | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.7.2.1 | Lab Logo Name Font Preset One | Font: 0 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.2.2 | Lab Logo Name Font Preset Two | Font: 1 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.2.3 | Lab Logo Name Font Preset Three | Font: 2 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.2.4 | Lab Logo Name Font Preset Four | Font: 3 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.3 | Lab Logo Contact Text Area | — | Field | UI element | 7 | [SAMPLE] | In scope |
| 7.2.1.7.4 | Lab Logo Contact Font Selector | Font | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.7.4.1 | Lab Logo Contact Font Preset One | Font: 0 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.4.2 | Lab Logo Contact Font Preset Two | Font: 1 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.4.3 | Lab Logo Contact Font Preset Three | Font: 2 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.4.4 | Lab Logo Contact Font Preset Four | Font: 3 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.5 | Report Header Text Area | صورة رأس التقرير كاملة | Field | UI element | 7 | [SAMPLE] | In scope |
| 7.2.1.7.6 | Report Header Font Selector | Font | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.7.6.1 | Report Header Font Preset One | Font: 0 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.6.2 | Report Header Font Preset Two | Font: 1 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.6.3 | Report Header Font Preset Three | Font: 2 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.6.4 | Report Header Font Preset Four | Font: 3 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.7 | Report Footer Text Area | صورة ذيل التقرير كاملة | Field | UI element | 7 | [SAMPLE] | In scope |
| 7.2.1.7.8 | Report Footer Font Selector | Font | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.7.8.1 | Report Footer Font Preset One | Font: 0 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.8.2 | Report Footer Font Preset Two | Font: 1 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.8.3 | Report Footer Font Preset Three | Font: 2 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.7.8.4 | Report Footer Font Preset Four | Font: 3 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.8 | Report Settings Note Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.8.1 | Dimensions In Centimetres Note | * جميع الأبعاد المستخدمة بالسنتيميتر | Note | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.8.2 | Image File Size Note | * حجم ملف الصور يجب أن لا يتعدي 1 ميجا بايت | Note | UI element | 7 | [VERIFIED] | In scope |
| 7.2.1.9 | Report Settings Action Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.2.1.9.1 | Report Settings Save Action | حفظ الإعدادات | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.1.9.2 | Header Footer Colours Action | تعديل ألوان رأس وذيل التقرير | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.1.9.3 | Report Settings Back Action | رجوع | Action | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.1.9.4 | Report Settings Main List Action | القائمة الرئيسية | Action | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.2 | Report Margins Adjustment | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| 7.2.3 | Paper Size Adjustment | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| 7.2.4 | Header Footer Printing Choices | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| 7.2.5 | Header Footer Layout Dialog | نافذة تعديل ألوان رأس وذيل التقرير | Screen | Container | 7 | [VERIFIED] | In scope |
| 7.2.5.1 | Show Top Line Toggle | Show top line | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.2 | Show Bottom Line Toggle | Show bottom line | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.3 | Long Date Toggle | Long date | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.4 | Show Time Toggle | Show time | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5 | Header Footer Layout Grid | — | Grid | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.1 | Layout Element Name Column | — | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.2 | Layout Show Flag Column | — | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.3 | Layout Width Column | Width | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.4 | Layout Height Column | Height | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.5 | Layout Top Column | Top | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.6 | Layout Left Column | Left | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.7 | Layout Patient Name Row | Patient Name | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.8 | Layout Patient Id Row | Patient Id | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.9 | Layout Age And Sex Row | Age / Sex | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.10 | Layout Request Date Row | Request Date | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.11 | Layout Referred By Row | Referred By | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.5.12 | Layout Printed In Row | Printed In | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.6 | Patient History Format Grid | — | Grid | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.6.1 | History Test Name Column | — | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.6.2 | History Result Column | — | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.6.3 | History Date Column | — | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.6.4 | History Bold Flag Column | Bold | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.6.5 | History Row Height Column | Height | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.6.6 | Patient History Row | — | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.7 | Layout Dialog Action Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.2.5.7.1 | Layout Dialog Back Action | عودة | Action | Subfunction | 7 | [VERIFIED] | In scope |
| 7.2.5.7.2 | Restore Default Layout Action | استعادة الوضع الافتراضي | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.5.7.3 | Layout Dialog Save Action | حفظ الاعدادات | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.5.8 | Colour Picker Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.2.5.8.1 | Basic Colours Palette | Basic colors | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.8.2 | Custom Colours Palette | Custom colors | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.2.5.8.3 | Define Custom Colours Action | Define Custom Colors >> | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.5.8.4 | Colour Confirm Action | OK | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.5.8.5 | Colour Cancel Action | Cancel | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.5.8.6 | Colour Dialog Close Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.2.5.9 | Layout Change Applies Everywhere Rule | أي تعديل في هذه النافذة سيظهر في جميع تقارير النتائج | Rule | Rule | 7 | [VERIFIED] | In scope |
| 7.2.5.10 | Colour Change Rule | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| **7.3** | **Dedicated Printers** | — | Group | Container | 7 | [VERIFIED] | In scope |
| 7.3.1 | Dedicated Printer Selector Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.3.1.1 | Default Printer Selector | تحديد الطابعة المخصصة لطباعة الإفتراضية | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.3.1.2 | Barcode Printer Selector | تحديد الطابعة المخصصة لطباعة الباركود | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.3.1.3 | Envelope Printer Selector | تحديد الطابعة المخصصة لطباعة الظرف | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.3.1.4 | Receipt Printer Selector | تحديد الطابعة المخصصة لطباعة الإيصال | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.3.1.5 | Card Printer Selector | تحديد الطابعة المخصصة لطباعة الكارنية | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.3.2 | Dedicated Printer Rule | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| **7.4** | **Default Account Type** | — | Group | Container | 7 | [VERIFIED] | In scope |
| 7.4.1 | Default Account Type Selector | نوع الحساب الافتراضي للنظام | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.4.1.1 | Default Account Type Individual Option | Individual | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.4.1.2 | Default Account Type Lab To Lab Option | Lab To Lab | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.4.1.3 | Default Account Type Contracts Option | Contracts | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.4.1.4 | Default Account Type Free Option | Free | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.4.2 | Default Account Type Rule | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| **7.5** | **Receipt Settings** | — | Group | Container | 7 | [VERIFIED] | In scope |
| 7.5.1 | Receipt Settings Tab | اعدادات الإيصال | Screen | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.1 | Receipt Currency Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.1.1 | Receipt Currency Local Option | تميز العملة المستخدمة: جنيهاً لا غير | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.1.2 | Receipt Currency Abbreviated Option | تميز العملة المستخدمة: L.E. | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.2 | Result Delivery Time Field | وقت استلام النتيجة | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.3 | Receipt Top Margin Field | الهامش العلوي للإيصال | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.4 | Print Receipt Once Option | طباعة إيصال المريض مرة واحدة فقط | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.5 | Print Delivery Data Option | طباعة بيانات الاستلام من موقع نتيجة على الإنترنت | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.6 | Receipt Test Details Group | — | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.6.1 | Hide Test Details Option | عدم عرض اسماء وتفاصيل التحاليل في الوصل | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.6.2 | Show Test Details Option | عرض اسماء وتفاصيل التحاليل في الوصل | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.6.3 | Show Test Details With Code Option | عرض اسماء وتفاصيل التحاليل في الوصل مع كود التحليل | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.6.4 | Cash Printer Option | استخدام طابعة الكاشير اسماء التحاليل والاجمالي | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.7 | Receipt Text Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.7.1 | Receipt Right Header Text Area | كلمات الجهة يمني من رأس الإيصال | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.7.2 | Receipt Right Header Font Selector | Font | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.7.2.1 | Receipt Right Header Font Preset One | Font: 0 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.7.2.2 | Receipt Right Header Font Preset Two | Font: 0.8 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.7.3 | Receipt Middle Text Area | كلمات منتصف الإيصال من أسفل | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.7.4 | Receipt Middle Font Selector | Font | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.7.4.1 | Receipt Middle Font Preset One | Font: 0 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.7.4.2 | Receipt Middle Font Preset Two | Font: 0.8 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.7.5 | Receipt Height Field | الارتفاع | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.7.6 | Receipt Text Font Type Selector | حجم ونوع الخط | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.7.6.1 | Receipt Text Font Preset One | Font: 0 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.7.6.2 | Receipt Text Font Preset Two | Font: 0.8 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.8 | Receipt Image Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.8.1 | Receipt Header Image Field | JPG Image 460 * 60 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.8.2 | Receipt Header Image Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.5.1.8.3 | Receipt Header Image Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.5.1.8.4 | Receipt Lab Logo Image Field | JPG Image 145 * 95 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.8.5 | Receipt Lab Logo Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.5.1.8.6 | Receipt Lab Logo Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.5.1.8.7 | Receipt Footer Image Field | JPG Image 720 * 50 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.8.8 | Receipt Footer Image Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.5.1.8.9 | Receipt Footer Image Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.5.1.9 | Receipt Settings Note Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.9.1 | Receipt Dimensions Note | * جميع الأبعاد المستخدمة بالسنتيميتر | Note | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.9.2 | Receipt Image Size Note | * حجم ملف الصور يجب أن لا يتعدي 1 ميجا بايت | Note | UI element | 7 | [VERIFIED] | In scope |
| 7.5.1.10 | Receipt Settings Action Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.5.1.10.1 | Receipt Settings Save Action | حفظ الإعدادات | Action | Function | 7 | [VERIFIED] | In scope |
| 7.5.1.10.2 | Receipt Settings Back Action | رجوع | Action | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.1.10.3 | Receipt Settings Main List Action | القائمة الرئيسية | Action | Subfunction | 7 | [VERIFIED] | In scope |
| 7.5.2 | Receipt Settings Rule | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| **7.6** | **Envelope Settings** | — | Group | Container | 7 | [VERIFIED] | In scope |
| 7.6.1 | Envelope Settings Tab | اعدادات الظرف | Screen | Container | 7 | [VERIFIED] | In scope |
| 7.6.1.1 | Envelope Header Footer Option Group | — | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.6.1.1.1 | Do Not Print Envelope Header Footer Option | عدم طباعة رأس وذيل الظرف (استخدام ظرف مطبوع) | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.1.2 | Print Envelope Header Footer As Words Option | طباعة رأس وذيل الظرف علي هيئة كلمات منفصلة (استخدام ظرف أبيض) | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.1.3 | Print Envelope Header Footer From Images Option | طباعة رأس وذيل الظرف من ملفات الصور المرفقة في البرنامج (استخدام ظرف أبيض) | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.2 | Envelope Top Margin Field | الهامش العلوى للظرف | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.3 | Envelope Text Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.6.1.3.1 | Envelope Right Header Text Area | — | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2 | Envelope Font Selector | Font | Field group | Container | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.1 | Envelope Font Preset One | Font: 0 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.2 | Envelope Font Preset Two | Font: 1 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.3 | Envelope Font Preset Three | Font: 2 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.4 | Envelope Font Preset Four | Font: 3 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.5 | Envelope Font Preset Five | Font: 4 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.6 | Envelope Font Preset Six | Font: 5 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.7 | Envelope Font Preset Seven | Font: 6 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.8 | Envelope Font Preset Eight | Font: 7 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.9 | Envelope Font Preset Nine | Font: 8 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.2.10 | Envelope Font Preset Ten | Font: 9 | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.3.3 | Envelope Height Field | الارتفاع | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.4 | Envelope Image Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.6.1.4.1 | Envelope Header Image Field | JPG Image 720 * 140 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.4.2 | Envelope Header Image Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.6.1.4.3 | Envelope Header Image Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.6.1.4.4 | Envelope Lab Logo Image Field | JPG Image 145 * 95 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.4.5 | Envelope Lab Logo Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.6.1.4.6 | Envelope Lab Logo Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.6.1.4.7 | Envelope Footer Image Field | JPG Image 720 * 50 Pixels | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.4.8 | Envelope Footer Image Browse Action | ? | Action | Function | 7 | [VERIFIED] | In scope |
| 7.6.1.4.9 | Envelope Footer Image Clear Action | X | Action | Function | 7 | [VERIFIED] | In scope |
| 7.6.1.5 | Envelope Data Alignment Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.6.1.5.1 | Envelope Data Alignment Grid | — | Grid | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.5.1.1 | Envelope Alignment Left Column | Left | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.5.1.2 | Envelope Alignment Top Column | Top | Grid column | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.5.1.3 | Envelope Patient Name Row | الاسم | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.5.1.4 | Envelope Referring Entity Row | جهة الإحالة: اسم الطبيب او جهة الاحالة | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.5.1.5 | Envelope Patient Code Row | الكود | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.5.1.6 | Envelope Date Row | التاريخ | Grid row | UI element | 7 | [VERIFIED] | In scope |
| 7.6.1.5.2 | Envelope Data Titles Option | عدم طباعة عناوين بيانات المريض علي الظرف | Option | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.1.6 | Envelope Settings Action Block | — | Block | Container | 7 | [VERIFIED] | In scope |
| 7.6.1.6.1 | Envelope Settings Save Action | حفظ الإعدادات | Action | Function | 7 | [VERIFIED] | In scope |
| 7.6.1.6.2 | Envelope Settings Main List Action | القائمة الرئيسية | Action | Subfunction | 7 | [VERIFIED] | In scope |
| 7.6.2 | Envelope Settings Rule | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| **7.7** | **Receipt and Envelope Document Details** | — | Group | Container | 7 | [VERIFIED] | In scope |
| 7.7.1 | Receipt Laboratory Data Lines | — | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.7.2 | Receipt Results Password Field | Results PW | Field | UI element | 7 | [VERIFIED] | In scope |
| 7.7.3 | Receipt Print Once Rule | — | Rule | Rule | 7 | [VERIFIED] | In scope |
| **8.1** | **Laboratory Module Entry** | — | Group | Container | 8 | [VERIFIED] | In scope |
| 8.1.1 | Laboratory Ribbon Item | Laboratory | Navigation | Subfunction | 8 | [VERIFIED] | In scope |
| 8.1.2 | Laboratory Window Capabilities | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.1.2.1 | Sample Registration And Coding Capability | تسجيل وتكويد عينات المرضي وفصلها وتكويد عينات كل قسم على حدي | Function | Function | 8 | [VERIFIED] | In scope |
| 8.1.2.2 | Sample Withdrawal Capability | سحب وفصل العينات | Function | Function | 8 | [VERIFIED] | In scope |
| 8.1.3 | Sample Withdrawal Item | سحب وفصل العينات | Menu item | Subfunction | 8 | [VERIFIED] | In scope |
| **8.2** | **Sample Withdrawal Screen and Withdrawal Rules** | — | Group | Container | 8 | [VERIFIED] | In scope |
| 8.2.1 | Sample Withdrawal Screen | نافذة سحب وفصل العينات | Screen | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.1 | Daily Patient Count Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.1.1 | Today Patient Count Field | عدد المرضى اليوم | Field | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.2 | Patient List Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.2.1 | Patient List Grid | الاسم \ ·  P. ID \ ·  L. ID \ ·  الطبيب \ ·  النوع \ ·  العمر | Grid | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.2.1.1 | Patient List Name Column | الاسم | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.2.1.2 | Patient List Patient Id Column | P. ID | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.2.1.3 | Patient List Lab Id Column | L. ID | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.2.1.4 | Patient List Doctor Column | الطبيب | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.2.1.5 | Patient List Sex Column | النوع | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.2.1.6 | Patient List Age Column | العمر | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.2.2 | Patient List Row | — | Grid row | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.3 | Patient Identification Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.3.1 | Selected Patient Name Field | الاسم | Field | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.3.2 | Selected Patient Id Field | P. ID | Field | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.3.3 | Selected Patient Lab Id Field | L. ID | Field | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.3.4 | Selected Patient Doctor Field | الطبيب | Field | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.3.5 | Selected Patient Sex Field | النوع | Field | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.3.6 | Selected Patient Age Field | العمر | Field | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.4 | Samples To Draw Grid | — | Grid | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.4.1 | Sample To Draw Row | — | Grid row | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.5 | Other Tube Control | Other Tube | Control | UI element | 8 | [UNVERIFIED] | In scope |
| 8.2.1.6 | Withdrawn Samples Grid | Tube Name \ ·  Tests \ ·  Take | Grid | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.6.1 | Withdrawn Tube Name Column | Tube Name | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.6.2 | Withdrawn Tests Column | Tests | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.6.3 | Withdrawn Take Flag Column | Take | Grid column | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.6.4 | Withdrawn Sample Row | — | Grid row | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.7 | Sample Action Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.7.1 | Take Action | Take | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.7.2 | Delete Action | Delete | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.7.3 | Refresh Sample List Action | Refresh | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.7.4 | Collapse Control | <<< | Control | UI element | 8 | [UNVERIFIED] | In scope |
| 8.2.1.8 | Save Action Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.8.1 | Save Patient Samples Action | Save patient samples | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.8.2 | Save Samples After Read Id Action | Save samples after read ID | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.9 | Print Action Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.9.1 | Print Barcode After Read Id Action | Print barcode after read ID | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.9.2 | Print All Stickers Action | Print all stickers | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.9.3 | Print Serum Plasma Blood Action | Print (Serum,Plasma,Blood..) | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.9.4 | Print Semen Urine Stool Action | Print (Semen,Urine,Stool..) | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.10 | Date Picker Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.10.1 | Month Navigation Back Action | ◄ | Action | Subfunction | 8 | [VERIFIED] | In scope |
| 8.2.1.10.2 | Month Label | July 2016 | Field | UI element | 8 | [SAMPLE] | In scope |
| 8.2.1.10.3 | Month Navigation Forward Action | ► | Action | Function | 8 | [VERIFIED] | In scope |
| 8.2.1.10.4 | Weekday Header Row | Sat \ ·  Sun \ ·  Mon \ ·  Tue \ ·  Wed \ ·  Thu \ ·  Fri | Row | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.10.5 | Day Grid | — | Grid | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.10.5.1 | Day Cell | — | Grid cell | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.10.6 | Selected Date Indicator | Today: 21/07/2016 | Field | UI element | 8 | [SAMPLE] | In scope |
| 8.2.1.11 | Notes Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.11.1 | Notes Label | ملاحظات | Label | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.11.2 | Sample Day Notes Field | — | Field | UI element | 8 | [VERIFIED] | In scope |
| 8.2.1.12 | Screen Action Block | — | Block | Container | 8 | [VERIFIED] | In scope |
| 8.2.1.12.1 | Main List Action | القائمة الرئيسية | Action | Subfunction | 8 | [VERIFIED] | In scope |
| 8.2.2 | Patient Selection Rule | — | Rule | Rule | 8 | [VERIFIED] | In scope |
| 8.2.3 | Sample Taking Rule | — | Rule | Rule | 8 | [VERIFIED] | In scope |
| 8.2.4 | Withdrawn Sample Listing Rule | — | Rule | Rule | 8 | [VERIFIED] | In scope |
| 8.2.5 | Tube Designation Correspondence Rule | — | Rule | Rule | 8 | [VERIFIED] | In scope |
| 8.2.6 | Sticker Printing Rule | — | Rule | Rule | 8 | [VERIFIED] | In scope |
