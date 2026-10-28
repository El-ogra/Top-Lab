# Unit 1 — Verified Gap Specification

## Patient Registration and Test Registration

**Repository:** `https://github.com/El-ogra/Top-Lab.git`
**Analysed commit:** `0300bddf7decf1c4ff983d059e698c25764eaa36` — verified as `HEAD` after checkout (working tree clean, 2014 tracked files)
**Stack:** C# / .NET 8, WPF, Clean Architecture (Domain; Application with MediatR CQRS and FluentValidation; Infrastructure with EF Core 8; Presentation with MVVM)

## Section A — Title and Scope

**Unit 1 — Patient Registration and Test Registration: Verified Gap Specification.**

Unit 1 covers the patient-registration surface (`Views/Patients/PatientEditorView.xaml`, `ViewModels/Patients/PatientEditorViewModel.cs`), the patient search directory (`Views/Patients/PatientSearchView.xaml`, `ViewModels/Patients/PatientSearchViewModel.cs`), the patient account ledger (`Views/Patients/PatientAccountView.xaml`, `ViewModels/Patients/PatientAccountViewModel.cs`), the cashiering domain (`TopLab.Domain/Billing/AccountEntryParser.cs`, `AccountEntrySession.cs`, `PatientAccountCalculator.cs`), the patient aggregate and its persistence (`TopLab.Domain/Patients/Patient.cs`, `TopLab.Infrastructure/Persistence/Configurations/PatientConfiguration.cs`) and the printed artefacts they emit (`TopLab.Infrastructure/Printing/`, `TopLab.Infrastructure/Barcode/`).

This document specifies the verified remaining gaps in Unit 1 together with the owner decisions that must be settled before the affected work can proceed. Every current-state statement derives from direct inspection of the source at the commit named above and names the file path and symbol that supports it. Where the code establishes a fact, the fact is stated as established; where a business, workflow or product choice cannot be derived from the code, the item carries an explicit owner-decision block and the decision is left open.

**Scope of this document.** It addresses only the patient-registration, test-ordering, cashiering and identity-printing capabilities of Unit 1. Product areas outside the Unit 1 boundary — including result-delivery channels, instrument interfacing and laboratory-equipment administration — are not gap items here, are not raised as owner decisions, and generate no implementation task in this specification.

**Document type.** A planning specification. It contains no application code and no repository modification. Migration statements are derived from the readable EF Core migration files and the current model snapshot in `TopLab.Infrastructure/Persistence/Migrations/`; no database was contacted and no migration was created, applied or modified.

## Section B — Execution Overview Table

| ID | Gap Title | Verified Status | Owner Decision Required? | Migration Required? |
|---|---|---|---|---|
| U1-GAP-001 | Durable visit follow-up flag | PARTIALLY EXISTS | Yes | Yes |
| U1-GAP-002 | Primary sample type applied to ordered tests | PARTIALLY EXISTS | Yes | Yes |
| U1-GAP-003 | Surcharge entry convention in the cashiering panel | PARTIALLY EXISTS | Yes | No |
| U1-GAP-004 | Discount percentage ↔ discount amount synchronisation | PARTIALLY EXISTS | No | No |
| U1-GAP-005 | Cash-receipt print-once enforcement | PARTIALLY EXISTS | Yes | Conditional |
| U1-GAP-006 | Unfinished-results filter on the patient search view | PARTIALLY EXISTS | No | No |
| U1-GAP-007 | Unverified-results filter on the patient search view | PARTIALLY EXISTS | No | No |
| U1-GAP-008 | Unprinted-results filter on the patient search view | PARTIALLY EXISTS | No | No |
| U1-GAP-009 | Explicit search-mode selector on the patient search view | PARTIALLY EXISTS | No | No |
| U1-GAP-010 | Full test-catalogue browser for the registration screen | PARTIALLY EXISTS | No | No |
| U1-GAP-011 | One-click ordering of a master test group | PARTIALLY EXISTS | No | No |
| U1-GAP-012 | Undelivered-results count indicator in the application shell | PARTIALLY EXISTS | No | No |
| U1-GAP-013 | Registration-statistics quick action on the patient editor | PARTIALLY EXISTS | Yes | No |
| U1-GAP-014 | Explicit main-menu navigation action on the patient editor | PARTIALLY EXISTS | Yes | No |
| U1-GAP-015 | Patient identification slip printing | OWNER DECISION REQUIRED | Yes | Conditional |
| U1-GAP-016 | Remembered sign-in identity | CONFIRMED MISSING | Yes | Conditional |

**Table conventions.** "Migration Required?" follows the migration-evidence rule: the EF Core migration files and the model snapshot are readable, so migration status is stated for every item. "No" means the required end state introduces no schema change; "Yes" means a new migration is required; "Conditional" means the requirement depends on the owner decision recorded inside that item, and both branches are stated explicitly in that item's Section H. "Owner Decision Required?" is "Yes" for the eight items carrying an unresolved business, workflow or product choice.

## Section C — Execution Order Rationale

**1. Visit-aggregate changes first (U1-GAP-001, U1-GAP-002).** Both change `TopLab.Domain.Patients.Patient` — the only domain aggregate in the solution that carries the `EnsureNotDeleted` guard — and both add a column to the `Patients` table. They lead because every other registration item reads and writes that aggregate, and changing its shape first avoids a second round of command, DTO and view-model churn.

**2. Cashiering domain rules (U1-GAP-003, then U1-GAP-004, then U1-GAP-005).** This is the one strict prerequisite chain in the set. `AccountEntrySession` enforces mutual exclusion between the discount value field and the discount percentage field (`AccountEntrySession.cs:77-102`) and forbids a surcharge carrying a discount (`:188-192`); `PaymentOperation.Create` enforces the same at the aggregate level (`PaymentOperation.cs:73-77`). U1-GAP-004 adds a derivation between the two entry fields, so the sign convention settled by U1-GAP-003 constrains it; U1-GAP-004 precedes U1-GAP-005 because both touch `TopLab.Domain.Billing.PaymentOperation`, and the receipt-emission design must be layered onto the final set of ledger invariants rather than onto a moving target.

**3. Application query contract (U1-GAP-006 → U1-GAP-007 → U1-GAP-008 → U1-GAP-009).** All four extend one shared contract, `SearchPatientsGlobalQuery`, whose own documentation fixes the positional shape `Text, Page, PageSize` and requires every additional filter to be appended at the tail with a default. The tail must therefore be extended in a defined sequence. The three result-state filters are logically independent of one another and are adjacent for contract hygiene only. U1-GAP-009 follows because it appends to the same record and must bind to a settled filter set.

**4. Registration catalogue surface (U1-GAP-010, then U1-GAP-011).** Both extend the catalogue surface assembled by `GetRegistrationCatalogQueryHandler` and already delivered to the editor as `RegistrationCatalogDto`. U1-GAP-011 follows because a browser organised by test group can host the group-ordering action; that is a design convenience, not a hard prerequisite — U1-GAP-011 is independently implementable.

**5. Shell and entry points (U1-GAP-012, U1-GAP-013, U1-GAP-014).** Independent of each other and of everything above; grouped because each adds an entry point to a capability that already exists elsewhere. No dependency is claimed between them.

**6. Decision items at their point of use (U1-GAP-015, U1-GAP-016).** U1-GAP-015 is a scoping decision with no downstream dependents. U1-GAP-016 concerns the sign-in surface, depends on nothing in this set, and is sequenced last because it is independent of the registration workflow.

**No invented dependencies.** U1-GAP-001, U1-GAP-002, U1-GAP-006, U1-GAP-007, U1-GAP-008, U1-GAP-010, U1-GAP-011, U1-GAP-012, U1-GAP-013, U1-GAP-014, U1-GAP-015 and U1-GAP-016 have no prerequisite other than those stated inside their own sections.

## Section D — Detailed Gap Specifications

---

### U1-GAP-001 — Durable visit follow-up flag

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The registration screen already carries a follow-up action. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml:71` binds a button labelled `متابعة` to `FollowUpCommand`. The command is constructed at `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs:199` and implemented by `FollowUpAsync` at lines 1778-1793 of the same file. `FollowUpAsync` navigates to `TopLab.Presentation.ViewModels.Audit.AuditViewModel` and calls `audit.LoadWithPatientIdAsync(_patientId.Value, cancellationToken)` (`src/TopLab.Presentation/ViewModels/Audit/AuditViewModel.cs:162-167`), which sets `SelectedTab = 0` and loads the per-patient case history through `src/TopLab.Application/Features/AuditAndTraceability/Queries/GetPatientAudit/GetPatientAuditQuery.cs` (`record GetPatientAuditQuery(int PatientId)`, requiring `AuditAccessPolicy.PtAuditAccess`). The view model's own documentation at lines 890-895 states that the follow-up element opens the case follow-up / audit-trail screen, implemented as the `AuditViewModel` patient-history tab. `AuditViewModel` exposes exactly three tabs — `IsPatientTab` (0), `IsTestTab` (1), `IsActivityTab` (2) at lines 67-69 — so the destination is a per-patient case history and nothing more.

**Remaining gap.** No durable follow-up *state* on the visit exists anywhere in the solution. `TopLab.Domain.Patients.Patient` (declared at `src/TopLab.Domain/Patients/Patient.cs:11` as `public sealed class Patient : AuditableEntity<PatientId>`) exposes `IsVip` (line 35), `PickupDateUtc` (line 39), `IsFastingIndicated`, `RecentContrastImaging`, `RecentUltrasoundImaging`, `CardNumber`, `Notes` and the inherited `IsDeleted` — and no follow-up member. Neither `Patient.Create` nor `Patient.Update` accepts or sets such a flag, and `src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommand.cs` and `.../UpdatePatient/UpdatePatientCommand.cs` carry no corresponding field. A search of the whole source tree for `FollowUp` and `IsFollowUpRequired` returns no domain member of any kind. Consequently nothing can record that a visit awaits clinical follow-up or re-sampling, no such state survives a reload, and it cannot be filtered or reported.

**Scope note.** The visit aggregate is `Patient`: one row per registration, with `LabId` as the shared grouping value across sibling visits. No separate visit entity exists, so the flag belongs on `Patient`.

#### Precise Required End State

1. `TopLab.Domain.Patients.Patient` gains a persisted boolean follow-up flag with a domain mutator that calls `EnsureNotDeleted()` — the guard at `src/TopLab.Domain/Patients/Patient.cs:69-75` — before mutating, and that can be set and cleared independently of the demographic fields.
2. The flag must be readable on the visit without a separate query and must be surfaced in the patient-search grid so a flagged visit is identifiable from the search surface.
3. The Application layer gains a MediatR command that sets or clears the flag for one visit, gated on `PatientRegistrationAccessPolicy.AddEditPatient` — the permission code already used by `AddTestsToVisitCommand`, `AddCustomGroupToVisitCommand` and `UpdatePatientTestSampleFlagsCommand`.
4. The command handler must reject a soft-deleted visit and return a not-found result for an unknown visit identifier, matching the guard order used by `AddCustomGroupToVisitCommandHandler` in `src/TopLab.Application/Features/PatientRegistration/Commands/AddCustomGroupToVisit/`.
5. The registration screen must present a control that displays and changes the flag for the loaded visit, disabled when no visit is loaded, matching the `IsEnabled="{Binding IsEditMode}"` idiom already used on `PatientEditorView.xaml:68`, `:70` and `:71`.
6. Saving the flag must not require re-submitting the registration form's other fields, so a follow-up can be flagged on an already-registered visit without touching its demographics.

#### Affected Layers, Files, and Symbols

**Domain — expected to change.** `src/TopLab.Domain/Patients/Patient.cs` — the `Patient` aggregate: new property and mutator; `EnsureNotDeleted` is already available.

**Application — expected to change.** `src/TopLab.Application/Features/PatientRegistration/Commands/` — a new command/handler/validator folder is justified by the established per-operation folder convention. `src/TopLab.Application/Features/PatientRegistration/Common/PatientRegistrationDtos.cs` — expected to change to carry the flag to the editor and the search grid. `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/` — the handler and search DTOs, so the flag reaches the search grid.

**Infrastructure — expected to change.** `src/TopLab.Infrastructure/Persistence/Configurations/PatientConfiguration.cs` — mapping for the new column. A new EF Core migration and an updated `src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` would be required (Section H).

**Presentation — expected to change.** `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — the command, its state, and the visit-load path that populates the flag alongside the other visit fields. `src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs` — the search DTO projection carrying the new column. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` — the control. `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml` — the grid column.

**Tests — expected to change.** New coverage in `tests/TopLab.Domain.Tests/` for the aggregate mutator and in `tests/TopLab.Application.Tests/Features/PatientRegistration/Commands/` for the handler, following the existing per-command test-folder convention. `tests/TopLab.Presentation.Tests/Patients/PatientEditorMedicalHistoryTests.cs` is the closest editor-level precedent for a per-visit boolean control.

#### Dependencies and Execution Prerequisites

No prerequisite identified.

#### Owner Decision

**Established.** The follow-up *action* exists and opens the per-patient case history. What does not exist is a durable per-visit follow-up *state*. The label `متابعة` is already occupied by the case-history navigation.

**Undecided.** Whether the new state gets a new, separately labelled control alongside the existing one, or whether the existing `متابعة` control is repurposed and the case-history navigation relocated or relabelled.

**Option 1 — Add a distinct follow-up control; leave the existing case-history button unchanged.**
* *Meaning:* a new "requires follow-up" toggle is added to the registration screen; `متابعة` continues to open the case history.
* *Functional consequences:* two distinct follow-up-related affordances coexist with clearly separate meanings.
* *Operational impact:* the operator sees both and must learn which is which. The action bar already carries thirteen controls (`PatientEditorView.xaml:58-71`).
* *Architectural impact:* Presentation for the control, on top of the Domain/Application/Infrastructure work above. `FollowUpCommand` and `FollowUpAsync` are untouched.
* *Persistence impact:* the new flag column on `Patients` (Section H).
* *Risks and trade-offs:* lowest risk of losing an existing working function; the cost is one additional control and the risk of operator confusion between two similarly-worded affordances.
* *Rule conflicts:* none.

**Option 2 — Repurpose `متابعة` as the follow-up flag toggle; expose the case history under a separate label.**
* *Meaning:* the existing button becomes the flag action; the case-history navigation moves to a new control.
* *Functional consequences:* the element reachable through `متابعة` changes meaning. `FollowUpAsync` and `GetPatientAuditQuery` are no longer reachable from that control.
* *Operational impact:* existing muscle memory for the case-history screen is broken; operators are retrained on the new label.
* *Architectural impact:* `FollowUpCommand` / `FollowUpAsync` are replaced or split; the audit navigation moves to a new command in the same view model.
* *Persistence impact:* the new flag column on `Patients` (Section H).
* *Risks and trade-offs:* avoids two similarly-worded controls, but removes a working, permission-aware navigation from its current position.
* *Rule conflicts:* the doc-comment at `PatientEditorViewModel.cs:890-895` binds the current control to the follow-up element of the registration screen; repurposing it requires that mapping to be revised deliberately.

**Option 3 — Retain the current control; add no durable follow-up state.**
* *Meaning:* the case-history navigation remains the only follow-up affordance.
* *Functional consequences:* a visit cannot be marked as awaiting follow-up; the state cannot be filtered or reported.
* *Operational impact:* the only way to signal a need for re-sampling is free-text notes on the visit.
* *Architectural impact:* none. *Persistence impact:* none.
* *Risks and trade-offs:* zero implementation cost; the operational need remains unmet and is unrecoverable from the ledger.
* *Rule conflicts:* none.

**Recommended option: Option 1.**

**Reasoning.** The existing `متابعة` control is documented in the code as the case-history screen and is wired to a permission-gated audit query. Overwriting a working, permission-aware navigation to introduce a boolean loses capability and creates retraining cost for no gain; a separate, explicitly labelled flag control preserves both behaviours. Option 1 and Option 2 carry identical schema cost, so the differentiator is operational, and Option 1 carries no regression risk to the existing audit surface.

**Owner decision required:** Does the visit follow-up state get its own new, separately labelled control on the registration screen, or should the existing `متابعة` control be repurposed and the case-history navigation relocated or relabelled?

*The recommendation is advisory only; the choice is the owner's.*

#### Migration Requirement

**A new migration is required.** The `Patients` table was created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` and carries no follow-up column; the current `ApplicationDbContextModelSnapshot.cs` shows the `TopLab.Domain.Patients.Patient` entity with `AccountType`, `Address`, `AgeUnit`, `AgeValue`, `CardNumber`, `FastingHours`, `FullName`, `IsDeleted`, `IsFastingIndicated`, `IsVip`, `LabId`, `NationalId`, `Notes` and the audit columns, and no follow-up property.

The new column follows the pattern already used on the same table: a `bit` column, not nullable, default `false` — as applied to `Patients.IsDeleted` in `20260907162756_AddPatientIsDeletedAndPatientTestSampleDrawnIndex` and to `Patients.RecentUltrasoundImaging` in `20261020211907_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue`. A new index is optional and justified only if the flag becomes a filterable or sortable search criterion.

#### Acceptance Conditions

1. A registered visit can be marked as requiring follow-up, and the mark persists across an application restart and a re-fetch of the visit.
2. The mark can be cleared independently, and clearing it alters no other field of the visit.
3. The mark is rejected for a soft-deleted visit and for an unknown visit identifier, with a not-found or validation result rather than an unhandled exception.
4. The follow-up state is displayed in the patient-search grid for every row that carries it, and absent rows are unaffected.
5. The existing `متابعة` behaviour is unchanged under Option 1, and the case history remains reachable and permission-gated under Option 2.
6. Saving the follow-up state writes, clears or alters neither the visit's demographic fields nor its ordered tests nor its ledger.
7. No new `PaymentOperation` row is created by flagging a visit.

---

### U1-GAP-002 — Primary sample type applied to ordered tests

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** Sample type is modelled per ordered test. `src/TopLab.Domain/Results/PatientTest.cs:19-29` declares `IsUrine`, `IsStool`, `IsBlood`, `IsSemen`, `IsCsf` and `IsTakenOutsideLab`; `PatientTest.Create` (lines 98-113) accepts all six, and `PatientTest.UpdateSampleFlags` (lines 226-240) changes them after creation. The registration screen exposes them twice: per test, as checkboxes in the `SelectedTests` list (`src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml:522-527`), and as a matrix of profile-level defaults (`ProfileIsUrine` … `ProfileIsTakenOutsideLab` at `PatientEditorView.xaml:486-491`, declared at `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs:823-831`) that are applied to the order created for a profile (`PatientEditorViewModel.cs:1818-1822` → `AddProfileToVisitCommand`). The screen also shows a live summary: `PatientEditorView.xaml:238-241` binds a read-only `TextBlock` to `SampleTypeSummary`, and `PatientEditorViewModel.cs:619` documents the property as *"a read-only summary of the sample types set on the visit's registered tests. The per-test control is the only editable source."*

A **de-facto specimen default already exists**, but only inside the view model and only for two of the three order-creation paths. `SelectedTestItem` declares `private bool _isBlood = true;` at `PatientEditorViewModel.cs:225`, and the profile matrix declares `private bool _profileIsBlood = true;` at line 131. `AddTestToSelection` (lines 1212-1232) constructs a `SelectedTestItem` without specifying any specimen flag, so those initialisers apply and a newly picked test is blood by default; the same holds for a profile order, whose matrix is passed to `AddProfileToVisitCommand` at lines 1818-1822. The third path does not share this default: `AddCustomGroupToVisitCommandHandler` creates its rows through `PatientTest.Create` with the factory's own defaults, in which all six specimen flags are `false` (`src/TopLab.Domain/Results/PatientTest.cs:103-108`).

**Order-creation paths.** Three commands create `PatientTest` rows and each already carries the six specimen flags: `AddTestsToVisitCommand` carries them per test through `AddTestInput`; `AddProfileToVisitCommand` carries them for the order it creates; `AddCustomGroupToVisitCommand` creates its rows with the default flags.

**Remaining gap.** There is no *primary* or *default* sample type **on the visit**. A search of the whole source tree for `DefaultSampleType`, `PrimarySampleType` and `specimen` returns no member of any kind in Domain, Application, Infrastructure or Presentation. The existing blood default is a view-model field initialiser, not a property of the visit: it is hard-coded to one specimen, it is not persisted, it is not visible to the operator, it does not survive a reload, and it is not applied by the custom-group path at all. The registration form therefore has no single field stating which specimen the visit as a whole is for, and nothing applies a visit-level default to tests ordered later in the registration session or in a subsequent session against the same visit. The profile matrix is scoped to one profile order, is consumed at the moment that order is created, and is not retained on the visit.

#### Precise Required End State

1. `TopLab.Domain.Patients.Patient` gains a persisted primary-sample-type property whose values are exactly the specimen matrices already modelled on `PatientTest` — blood, urine, stool, semen, cerebrospinal fluid — plus the "taken outside the laboratory" condition, so the value can be projected onto a test without inventing a new vocabulary. A value that cannot be projected must be rejected by the aggregate.
2. The primary sample type must be applied to every test ordered against the visit, including tests added after the primary type was chosen and including tests added in a later session against the same visit. This requires the value to be read at order time, not only at save time.
3. It must apply to each of the three order-creation paths: the individual-test path (`AddTestsToVisitCommand`), the profile path (the single `PatientTest` created by `AddProfileToVisitCommand` for the profile's own test), and the custom-group path (the rows created by `AddCustomGroupToVisitCommand`). The three paths must agree: the view model's current blood-only initialiser is a presentation convenience, and a visit-level default must replace it rather than compete with it, so that a test ordered by any path inherits the same specimen.
4. The projection must not silently overwrite a per-test specimen flag the operator has already set explicitly. The established rule to preserve is that the per-test controls are the authoritative editable source (`PatientEditorViewModel.cs:619`) and the primary type is a default.
5. The profile matrix must remain the profile-specific default; the primary sample type is the visit-wide default and must not remove or override the profile matrix.
6. The registration screen must present the primary sample type in the identity block alongside the existing read-only `SampleTypeSummary`, and the summary must continue to reflect the effective per-test specimen set rather than the primary value.
7. Saving the primary sample type must not change the specimen flags of already-ordered tests unless the operator explicitly asks for it (owner decision).
8. When the primary sample type is set, the view-model blood initialiser must not silently override it, and the view model's initialiser must remain the fallback for a visit that has no primary sample type.
8. A visit with no primary sample type must behave exactly as today: no default is applied and every test keeps its own flags.

#### Affected Layers, Files, and Symbols

**Domain — expected to change.** `src/TopLab.Domain/Patients/Patient.cs` — `Patient`: new property, validation in the aggregate, and integration with the existing `Update` method. `src/TopLab.Domain/Results/PatientTest.cs` — `PatientTest.Create` and `PatientTest.UpdateSampleFlags` express the per-test specimen state; a projection from a visit-level default onto a new test is expected alongside them or in the application layer that calls them.

**Application — expected to change.** `src/TopLab.Application/Features/PatientRegistration/Commands/CreatePatient/CreatePatientCommand.cs` and `.../CreatePatientCommandHandler.cs`; `.../UpdatePatient/UpdatePatientCommand.cs` and `.../UpdatePatientCommandHandler.cs` — to carry and persist the value. `.../AddTestsToVisit/AddTestsToVisitCommandHandler.cs`, `.../AddProfileToVisit/AddProfileToVisitCommandHandler.cs` and `.../AddCustomGroupToVisit/AddCustomGroupToVisitCommandHandler.cs` — to apply the visit default when creating rows. `src/TopLab.Application/Features/PatientRegistration/Common/PatientRegistrationDtos.cs` — to carry the value to the editor and the search surface.

**Infrastructure — expected to change.** `src/TopLab.Infrastructure/Persistence/Configurations/PatientConfiguration.cs` — mapping for the new column. A new EF Core migration and an updated model snapshot would be required (Section H).

**Presentation — expected to change.** `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — the new property, its load path, and the per-test flag initialisation. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` — the new control in the identity block next to the existing `SampleTypeSummary` block.

**Tests — expected to change.** New coverage in `tests/TopLab.Domain.Tests/` for the aggregate and in `tests/TopLab.Application.Tests/Features/PatientRegistration/Commands/` for the three order-creation handlers. `tests/TopLab.Application.Tests/Features/PatientRegistration/Commands/AddTestsToVisitCommandHandlerTests.cs` and `.../AddProfileToVisitCommandHandlerTests.cs` are the direct precedents.

#### Dependencies and Execution Prerequisites

No prerequisite identified. The item is independent of U1-GAP-001, although both change `TopLab.Domain.Patients.Patient` and both add a column to the same table; sequencing them adjacently avoids repeated migration churn but is not a technical constraint.

#### Owner Decision

**Established.** Per-test specimen flags are authoritative and editable today. A precedent for applying a default at order time already exists: the profile matrix is applied to the order it accompanies and is not retained. The profile order creates exactly one `PatientTest` row — for the profile's own test at `ResultKind.SpecializedProfile`, priced by `PatientAccountCalculator.ProfileSelectionCharge` — so the visit default reaches it in the same way it reaches any other order.

**Undecided.** Whether choosing a primary sample type is purely a default for subsequently ordered tests, or whether it also re-projects onto tests already ordered for the visit. The second reading is a bulk edit of committed clinical data.

**Option 1 — Pure default: applies only to tests ordered from the moment it is set.**
* *Meaning:* the value seeds new orders only. Existing test flags remain as the operator set them.
* *Functional consequences:* the visit's effective specimen set can remain heterogeneous; the read-only `SampleTypeSummary` continues to show the real per-test set.
* *Operational impact:* the operator states the specimen once and it stops appearing on later orders unless changed. No silent rewriting of clinical data.
* *Architectural impact:* the value is read at order time inside the three order-creation handlers. No bulk-update path is required.
* *Persistence impact:* the new column on `Patients` (Section H).
* *Risks and trade-offs:* safest with respect to clinical data integrity; a visit whose tests were ordered before the primary type was set will not be uniform.
* *Rule conflicts:* none — it preserves the rule recorded at `PatientEditorViewModel.cs:619`.

**Option 2 — Re-project onto all tests already ordered for the visit whenever it changes.**
* *Meaning:* setting or changing the value rewrites the specimen flags of every test already ordered against the visit.
* *Functional consequences:* the visit's tests become uniform; existing explicit per-test corrections are lost unless re-applied.
* *Operational impact:* the operator must be warned before the overwrite and must be able to inspect which tests will change. It also raises what happens when a test has already been drawn, reviewed or printed — overwriting a specimen flag on a printed result changes the meaning of an issued document.
* *Architectural impact:* a bulk-update path over `PatientTest` rows, with its own authorisation and concurrency considerations.
* *Persistence impact:* the new column on `Patients`; no additional column on `PatientTest`, whose six specimen columns already exist.
* *Risks and trade-offs:* delivers a uniform visit but can corrupt already-issued results and can silently change the interpretation of a printed report.
* *Rule conflicts:* the lifecycle methods `PatientTest.MarkPrinted` and `PatientTest.MarkDelivered` (`src/TopLab.Domain/Results/PatientTest.cs:178-200`) require an entered-and-reviewed result and a printed result respectively, showing that specimen state is treated as part of a produced result's integrity once it exists. A bulk re-projection does not violate those guards directly but undermines the intent behind them.

**Option 3 — Retain current behaviour: per-test specimen flags only.**
* *Meaning:* the operator sets the specimen on every ordered test, as today.
* *Functional consequences:* no visit-level specimen statement exists.
* *Operational impact:* repetitive entry when several tests share a specimen; the profile matrix remains the only shortcut and it applies to one order only.
* *Architectural impact:* none. *Persistence impact:* none.
* *Risks and trade-offs:* no implementation cost; the operational friction remains.
* *Rule conflicts:* none.

**Recommended option: Option 1.**

**Reasoning.** Option 1 delivers the intended benefit using the projection pattern the codebase already applies for profiles at order time, without rewriting clinical data that may already have been resulted, reviewed, printed or delivered. The lifecycle methods on `PatientTest` demonstrate that specimen state is treated as part of a produced result's integrity; Option 2 would breach that intent for a convenience gain. The profile matrix is the in-repository precedent for exactly this behaviour.

**Owner decision required:** Is the primary sample type a default that applies only to tests ordered after it is set, or must setting it also re-project onto tests already ordered for the visit?

*The recommendation is advisory only; the choice is the owner's.*

#### Migration Requirement

**A new migration is required.** `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` created the `Patients` table without any primary-sample-type column, and the current `ApplicationDbContextModelSnapshot.cs` shows no such property on the `TopLab.Domain.Patients.Patient` entity. The new column follows the same pattern used on this table for `Patients.RecentUltrasoundImaging` in `20261020211907_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue` — a `bit` column, not nullable, default `false`. If the primary sample type is represented by an enumeration rather than a set of booleans, the column type follows the convention already used on this table for enumerated values: `tinyint` with an integer conversion, as applied to `AccountType` and `AgeUnit` in `PatientConfiguration.cs`.

No new column is required on `PatientTest`: the six specimen columns already exist, created in `20260828052248_BaselineDataModel` and mapped in `src/TopLab.Infrastructure/Persistence/Configurations/PatientTestConfiguration.cs`.

#### Acceptance Conditions

1. A primary sample type can be selected on a visit, persisted, and read back after the visit is re-fetched.
2. Every test ordered after the primary sample type is set inherits it on the specimen axes the type determines — on the individual-test path, on the profile path, and on the custom-group path.
3. The profile path is covered specifically: the single `PatientTest` created for a profile order carries the primary sample type unless the profile matrix supplies a different value for that axis.
4. Tests ordered before the primary sample type was set keep their existing flags under Option 1, and are re-projected under Option 2.
5. A test whose specimen the operator set explicitly keeps that explicit value when the primary sample type is changed afterwards, under Option 1.
6. The read-only `SampleTypeSummary` continues to report the effective per-test specimen set and is not reduced to the primary value.
7. A visit with no primary sample type behaves exactly as today: no default is applied and every test keeps its own flags.
8. The aggregate rejects a primary sample type value that cannot be projected onto the six specimen axes modelled by `PatientTest`.
9. The three order-creation paths agree: a test added individually, a profile order and a custom-group expansion all inherit the same specimen from the visit's primary sample type.
10. A visit with no primary sample type keeps today's behaviour, including the view model's blood default on individually added tests and on profile orders.
11. `UpdatePatientTestSampleFlagsCommand` remains the only path that changes an individual test's specimen after creation, and `PatientTest.UpdateSampleFlags` remains the only domain method that performs that change.
12. No `PaymentOperation` row is created by setting the primary sample type.

---

### U1-GAP-003 — Surcharge entry convention in the cashiering panel

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** Direct surcharge entry from the main cashiering panel is implemented, in both cashiering surfaces. `src/TopLab.Domain/Billing/AccountEntryParser.cs:8-15` declares `public enum AccountEntryKind { Payment = 0, Surcharge = 1 }`, and `AccountEntryParser.ParsePaidAmount` (lines 75-101) selects the surcharge kind when the trimmed entry begins with `+`. `src/TopLab.Domain/Billing/AccountEntrySession.cs:188-192` rejects a surcharge carrying a discount, and lines 259-261 produce the Arabic surcharge preview line. On the presentation side `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs:1962-1975` detects `AccountEntryKind.Surcharge` and dispatches `RecordExtraChargeCommand` instead of `RecordPaymentCommand`; `src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs:335-344` does the same on the account screen. The resulting ledger row is created by `src/TopLab.Application/Features/PatientBilling/Commands/RecordExtraCharge/RecordExtraChargeCommandHandler.cs`, which passes `isExtraCharge: true` to `PaymentOperation.Create`, and is validated by `RecordExtraChargeCommandValidator`, whose amount must be strictly greater than zero. A second, modal surcharge path exists: `src/TopLab.Presentation/Views/Patients/ExtraChargeDialogWindow.xaml`, opened from `src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml:99` through `OpenExtraChargeCommand`.

**Remaining gap.** The alternative spelling of a surcharge — a *negative* value in the discount field — is not accepted, and is rejected at four independent points. `AccountEntryParser.ParseDiscount` (lines 107-120) fails any value below zero with `InvalidDiscountMessage` ("قيمة الخصم غير صالحة."). `src/TopLab.Domain/Billing/PaymentOperation.cs:73-77` throws from `PaymentOperation.Create` when `discountAmount` is negative, when it exceeds `amount`, or when an extra charge carries a discount. `src/TopLab.Application/Features/PatientBilling/Commands/RecordPayment/RecordPaymentCommandValidator.cs` requires `DiscountAmount` strictly greater than zero and no greater than `Amount`. `AccountEntrySession.DiscountText` (lines 77-89) treats the discount field as the value channel only. `AccountEntryParser.HasMinusSign` (lines 222-224) exists as a detector for a leading minus sign and is referenced nowhere else in the solution.

**Effect of a negative discount on the ledger.** `src/TopLab.Domain/Billing/PatientAccountCalculator.cs` computes `TotalPaid` as the sum of `Amount + (DiscountAmount ?? 0)` over non-voided, non-extra-charge, non-refund operations, and `CashRefundable` as the sum of `Amount` alone over the same rows, explicitly excluding `DiscountAmount` because *"a discount is never cash"*. A negative `DiscountAmount` would therefore reduce `TotalPaid` while leaving `CashRefundable` unchanged, and would be indistinguishable in effect from a charge misclassified as a discount. This is why the extra charge is modelled as a separate `IsExtraCharge` row rather than as a negative discount.

#### Precise Required End State

The surcharge capability required by the cashiering panel is already present. The outstanding question is whether the *negative-sign spelling* must additionally be accepted and, if so, how it is represented so the ledger arithmetic above stays correct. The end state depends entirely on the owner decision below.

**Established, and true under every option.**
1. A surcharge entered in the main cashiering panel must be recorded as a distinct `PaymentOperation` row with `IsExtraCharge = true`, and must not alter the discount semantics of a payment.
2. A surcharge and a discount must remain mutually exclusive in a single entry — the rule at `AccountEntrySession.cs:188-192` and in `PaymentOperation.Create` must remain in force.
3. `PatientAccountCalculator.TotalCharged`, `TotalPaid` and `CashRefundable` must continue to produce the same results for a surcharge recorded under whichever input convention is chosen.
4. The operator's entered value must be echoed in the preview line produced by `AccountEntrySession.PressEnter` before anything is persisted, consistent with the two-press confirmation already implemented.

**Option-dependent requirements** are stated in the owner decision.

#### Affected Layers, Files, and Symbols

**Domain — expected to change only if the negative-sign spelling is adopted.** `src/TopLab.Domain/Billing/AccountEntryParser.cs` — `ParseDiscount` and, if adopted, `ParsePaidAmount`'s handling of a leading minus sign. `src/TopLab.Domain/Billing/AccountEntrySession.cs` — `DiscountText` and `PressEnter` if the sign convention changes. `src/TopLab.Domain/Billing/PaymentOperation.cs` — `PaymentOperation.Create` guards if a negative `DiscountAmount` becomes representable. `src/TopLab.Domain/Billing/PatientAccountCalculator.cs` — requires verification during implementation if a negative discount becomes representable, because `TotalPaid` and `CashRefundable` treat `DiscountAmount` asymmetrically.

**Application — expected to change only if the negative-sign spelling is adopted.** `src/TopLab.Application/Features/PatientBilling/Commands/RecordPayment/RecordPaymentCommandValidator.cs` — the `DiscountAmount` rules. `src/TopLab.Application/Features/PatientBilling/Commands/RecordExtraCharge/RecordExtraChargeCommandValidator.cs` — expected to remain unchanged; it already enforces a strictly positive amount.

**Infrastructure — not expected to change.** `PaymentOperations.DiscountAmount` is `decimal(18,2)` and nullable, and already exists.

**Presentation — expected to change only if the negative-sign spelling is adopted.** `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — `RecordPaymentAsync` (lines 1931-1993) and `PaymentDiscountText` (lines 682-694). `src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs` — the equivalent payment path.

**Tests — expected to change.** `tests/TopLab.Domain.Tests/Billing/AccountEntryParserTests.cs`, `tests/TopLab.Domain.Tests/Billing/AccountEntrySessionTests.cs` (already covering the surcharge convention at lines 115-135) and `tests/TopLab.Presentation.Tests/Patients/PatientEditorRoundThreeTests.cs` are the existing touchpoints.

#### Dependencies and Execution Prerequisites

**Prerequisite: U1-GAP-003 must be decided before U1-GAP-004 is implemented.** `AccountEntrySession` enforces a mutual-exclusion rule between the discount value field and the discount percentage field (lines 77-102), and a surcharge may not carry a discount (lines 188-192). The derivation that U1-GAP-004 adds between those two fields must be defined against the final sign convention of the discount field, so that a surcharge, a discount value and a discount percentage cannot be simultaneously active.

Existing components this item depends on: `AccountEntryParser`, `AccountEntrySession`, `AccountEntryCalculator`, `PaymentOperation`, `RecordPaymentCommand`, `RecordExtraChargeCommand`, and the `AccountEntryEnterCommand` two-press sequence implemented in `PatientEditorViewModel.AccountEntryEnterAsync` (lines 2001-2046).

#### Owner Decision

**Established.** Surcharge entry from the main cashiering panel is implemented and reachable today through the `+` prefix, in both the patient editor and the patient account screen; the modal `ExtraChargeDialogWindow` provides a second path. The negative-sign spelling is rejected deliberately at the parser, the aggregate factory, the validator and the session, and the aggregate's own documentation states that an extra charge cannot carry a discount.

**Undecided.** Whether the negative-sign spelling must also be accepted at the cashiering panel, and if it is, whether a negative entry is recorded as a negative discount on the payment row or translated into an extra-charge operation.

**Option 1 — Retain the `+`-prefix convention as the only surcharge spelling.**
* *Meaning:* a surcharge is distinguished by the `+` prefix; a negative value in the discount field remains invalid and keeps its current Arabic error message.
* *Functional consequences:* the requirement that a surcharge be enterable from the main cashiering panel is met. Nothing changes about how a surcharge is recorded.
* *Operational impact:* the operator must learn the `+` prefix, which is not discoverable from the field labels; the preview line explains the convention only after a first entry.
* *Architectural impact:* none beyond optional discoverability work in `AccountEntrySession`'s preview message.
* *Persistence impact:* none.
* *Risks and trade-offs:* no risk to the ledger, because the extra-charge row keeps `TotalPaid` and `CashRefundable` unambiguous. The residual risk is discoverability of the `+` convention.
* *Rule conflicts:* none; every existing invariant is preserved.

**Option 2 — Accept a negative value in the discount field and translate it into an extra-charge operation.**
* *Meaning:* a negative discount entry is interpreted as a surcharge. The stored representation is an `IsExtraCharge = true` operation, never a negative `DiscountAmount`.
* *Functional consequences:* the operator can express a surcharge in either spelling. The ledger representation is unchanged, so `TotalPaid` and `CashRefundable` keep their current meaning.
* *Operational impact:* two spellings for one action raise the chance of the wrong one being used, and a mistyped negative in the discount field would silently become a charge rather than raising an error — a materially different failure mode from today, where the same keystroke is rejected with a message.
* *Architectural impact:* `AccountEntryParser.ParseDiscount` must distinguish "negative discount" from "invalid discount" and route the former to the surcharge kind; `AccountEntrySession` needs the equivalent routing; `PatientEditorViewModel.RecordPaymentAsync` and `PatientAccountViewModel` must dispatch the extra-charge command for that case. `PaymentOperation.Create` guards stay intact because the value never reaches `DiscountAmount`.
* *Persistence impact:* none.
* *Risks and trade-offs:* more permissive input at the cost of a silent-misclassification failure mode; the existing guard that an extra charge cannot carry a discount becomes load-bearing for keeping the two channels apart.
* *Rule conflicts:* none, provided the negative entry is translated into an extra charge rather than stored as a negative discount. Storing it as a negative `DiscountAmount` would require relaxing `PaymentOperation.Create` and would break the asymmetry between `TotalPaid` and `CashRefundable`.

**Option 3 — Accept a negative value and store it as a negative discount on the payment operation.**
* *Meaning:* `DiscountAmount` becomes permitted below zero and the arithmetic treats it as such.
* *Functional consequences:* a negative discount reduces `TotalPaid` — the operator effectively grants credit — while `CashRefundable` remains computed from `Amount` alone. The two measures disagree by the magnitude of the negative discount.
* *Operational impact:* the keystroke that currently produces a clear validation message would instead create a credit, silently.
* *Architectural impact:* `PaymentOperation.Create` must be relaxed; `AccountEntryParser.ParseDiscount` and `RecordPaymentCommandValidator` must accept negative values; `PatientAccountCalculator.TotalPaid` and `CashRefundable` need an explicit reconciliation.
* *Persistence impact:* none — the column already exists and is `decimal(18,2)`.
* *Risks and trade-offs:* highest risk of the three. It makes a data-entry error indistinguishable from a deliberate credit and puts the refund ceiling in conflict with the paid total, because `CashRefundable` deliberately excludes discounts.
* *Rule conflicts:* conflicts with the documented rule in `PatientAccountCalculator.cs` that a discount is *"money the patient no longer owes, which is correct for the balance but wrong for a refund — a discount was never cash"*, and with the `PaymentOperation.Create` guard that a discount cannot be negative.

**Recommended option: Option 1.**

**Reasoning.** The functional requirement — a surcharge enterable from the main cashiering panel — is already satisfied by the `+` convention, implemented consistently in the parser, the session, the editor and the account screen, and it keeps the surcharge on the charged side of the ledger where `PatientAccountCalculator` expects it. Option 2 adds a second spelling whose failure mode is a silently misclassified entry, and Option 3 would put the discount channel in direct conflict with the refund-ceiling rule the domain deliberately encodes. The only residual concern with Option 1 is discoverability of the `+` prefix, which is a presentation matter rather than a capability gap.

**Owner decision required:** Must a negative value entered in the cashiering panel's discount field be accepted as a surcharge? If so, must it be stored as an extra-charge operation or as a negative discount?

*The recommendation is advisory only; the choice is the owner's.*

#### Migration Requirement

**No new migration is required under any option.** The `PaymentOperations` table was created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` with `DiscountAmount` as a nullable `decimal(18,2)`, and the current `ApplicationDbContextModelSnapshot.cs` shows the same. Option 1 changes nothing. Option 2 records the surcharge as an extra-charge row and therefore uses the existing `IsExtraCharge` column, added in the same baseline migration. Option 3 also requires no schema change, because the existing column can already hold a negative value; the change would be confined to the domain guard and the validator.

#### Acceptance Conditions

1. A surcharge entered in the main cashiering panel of the patient editor is recorded as a `PaymentOperation` row with `IsExtraCharge = true` and raises the charged total.
2. A surcharge entered on the patient account screen is recorded identically.
3. A surcharge is never combined with a discount in one entry: the entry is rejected with the existing Arabic message and nothing is persisted.
4. Under Option 1, a negative value in the discount field is rejected with a clear message and no ledger row is written.
5. Under Option 2, a negative value in the discount field produces an extra-charge row and never a negative `DiscountAmount`.
6. Under Option 3, a negative discount is stored as entered, and `TotalPaid` and `CashRefundable` remain consistent with each other and with the refund ceiling.
7. The preview line shown on the first Enter press states the correct resulting figures for a surcharge entry, in the format already produced by `AccountEntrySession.PressEnter`.
8. The second Enter press persists exactly what the first press previewed.
9. Regression: an ordinary payment entry — amount, optional discount, optional percentage — behaves exactly as it does today, including the D-01 availability message and the two-press confirmation.

---


### U1-GAP-004 — Discount percentage ↔ discount amount synchronisation

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** Both fields exist and one direction of synchronisation works. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml:45` binds a percentage text box to `PaymentPercentageText`; the same view binds the discount amount to `PaymentDiscountText` at line 39 and the payment amount to `PaymentAmountText` at line 33. `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs:697-707` defines `PaymentPercentageText`, and `AccountEntryEnterAsync` (lines 2001-2046) converts it: when the percentage field is non-empty it calls `_accountEntry.ApplyPercentage(TotalCharged)` at line 2014 and writes the converted amount into the discount field through the backing field, deliberately bypassing the `PaymentDiscountText` setter so the mutual-exclusion rule does not discard the result (lines 2021-2028). The conversion is `AccountEntrySession.ApplyPercentage` (`src/TopLab.Domain/Billing/AccountEntrySession.cs:125-153`), which calls `AccountEntryCalculator.DiscountAmountFromPercentage` (`src/TopLab.Domain/Billing/AccountEntryParser.cs:238-251`) to compute the amount from the patient's total account amount, rounded to two decimals away from zero. `src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs:379-398` performs the identical sequence, so the two cashiering surfaces are already at parity.

**Remaining gap.** The reverse direction does not exist. There is no code path anywhere that derives a percentage from an entered discount amount. Entering an amount actively clears the percentage: `PatientEditorViewModel.PaymentDiscountText` (lines 682-694) sets `_accountEntry.DiscountText` and then assigns `PaymentPercentageText = null` at line 691, and `AccountEntrySession.DiscountText` (lines 77-89) likewise clears `_percentageText` and resets `_appliedPercentage`. The converse also holds — `AccountEntrySession.PercentageText` (lines 91-102) clears `_discountText`. The two fields are strictly mutually exclusive by construction.

**Established rules that constrain the missing direction.**
1. The percentage is computed against the patient's *total* account amount, not the outstanding balance — `AccountEntryCalculator.DiscountAmountFromPercentage` takes `totalAccountAmount`, and both view models pass `TotalCharged`.
2. Owner decision D-01, recorded at `AccountEntrySession.cs:194-221` and exercised by `tests/TopLab.Domain.Tests/Billing/AccountEntryPercentageProbeTests.cs` and `tests/TopLab.Presentation.Tests/Patients/PatientEditorPercentageConflictTests.cs`, makes a percentage discount available **only** when the entry settles the whole remaining balance: `amount + discount` must equal `TotalCharged - net TotalPaid`. The check runs before the domain rule that a discount cannot exceed the operation amount.
3. The domain rule that a discount cannot exceed the operation amount is applied second and must remain in force (`AccountEntrySession.cs:225-229`).
4. The two-press confirmation must be preserved: the first Enter press previews, the second persists (`AccountEntrySession.PressEnter`, lines 160-273; both view models' `AccountEntryEnterAsync`).

#### Precise Required End State

1. When the operator enters a discount **amount**, the percentage field must display the percentage that the amount represents, computed against the same base the existing conversion uses — the patient's total account amount — so the two fields are mutually consistent by construction.
2. The derived percentage must be a read-only projection of the amount; the operator must not be able to type a percentage that contradicts an amount already entered. The established mutual-exclusion rule remains in force: the amount is authoritative once entered and the percentage reflects it.
3. The existing percentage → amount direction must continue to work unchanged, including its Enter-driven trigger, its D-01 availability check and its two-press confirmation.
4. The derived percentage must respect the D-01 condition: a percentage is only meaningful when the entry settles the remaining balance. The operator must not be shown a percentage the session would then reject as unavailable. The end state must therefore either suppress the derived percentage when D-01 is not satisfied, or show it with the reason it is unavailable — but never display a value contradicting the session's own rule.
5. The percentage must be cleared whenever the amount is cleared, and vice versa, so no stale derived value survives an edit.
6. The derived value must be computed by domain logic in `TopLab.Domain.Billing`, alongside `AccountEntryCalculator.DiscountAmountFromPercentage`, and not in the view model, in keeping with the separation where the view model forwards text to the session and reads back the session's state.
7. Rounding must follow the panel's existing money convention — two decimals away from zero, as implemented by `AccountEntryCalculator.Round2` — and the percentage base and rounding must match the existing forward conversion so the two directions round-trip.

#### Affected Layers, Files, and Symbols

**Domain — expected to change.** `src/TopLab.Domain/Billing/AccountEntryParser.cs` — `AccountEntryCalculator`, the expected home for the reverse derivation beside `DiscountAmountFromPercentage` and `Round2`. `src/TopLab.Domain/Billing/AccountEntrySession.cs` — `AccountEntrySession`: the amount setter at lines 77-89 currently clears the percentage, and `PressEnter` at lines 160-273 must continue to see a consistent pair.

**Application — not expected to change.** The percentage is a transient entry value; the design note on `AccountEntryParser` states that the type *"introduces no schema change and no stored percentage"*. The persisted command carries only `Amount` and `DiscountAmount` (`RecordPaymentCommand`).

**Infrastructure — not expected to change.**

**Presentation — expected to change.** `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — `PaymentDiscountText` (lines 682-694) and `PaymentPercentageText` (lines 697-707); the existing write-through of the backing fields in `AccountEntryEnterAsync` (lines 2021-2028) is the established idiom for publishing a value derived by the session. `src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs` — the equivalent account-screen path, so the two cashiering surfaces do not diverge. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` and `src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml` — the percentage field may need to be presented as a read-only projection.

**Tests — expected to change.** `tests/TopLab.Domain.Tests/Billing/AccountEntryParserTests.cs`, `tests/TopLab.Domain.Tests/Billing/AccountEntrySessionTests.cs` and `tests/TopLab.Domain.Tests/Billing/AccountEntryPercentageProbeTests.cs` are the domain touchpoints; `tests/TopLab.Presentation.Tests/Patients/PatientEditorPercentageConflictTests.cs` and `.../PatientEditorRoundThreeTests.cs` are the view-model touchpoints.

#### Dependencies and Execution Prerequisites

**Prerequisite: U1-GAP-003.** The discount-field sign convention must be settled first, because the derived percentage is computed from the discount amount and a negative-discount interpretation would change what the amount represents. The mutual-exclusion rule in `AccountEntrySession` is also the mechanism that must host the new derivation.

Existing components this item depends on: `AccountEntrySession`, `AccountEntryParser` / `AccountEntryCalculator`, the `AccountEntryEnterCommand` two-press sequence in both view models.

#### Owner Decision

No decision required

#### Migration Requirement

**No new migration is required.** The percentage is a transient entry value and is never persisted. `RecordPaymentCommand` carries only the operation amount and the discount amount, and `PaymentOperation` has no percentage column in `src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`. The design note on `AccountEntryParser` states explicitly that the type holds no state and introduces no stored percentage. The D-01 rule itself establishes that the percentage is converted to an amount before anything is persisted.

#### Acceptance Conditions

1. Entering a discount amount causes the percentage field to display the percentage that amount represents, computed against the visit's total account amount and rounded to two decimals away from zero.
2. The displayed percentage is a projection: it cannot be edited into a value that contradicts the amount, and the established mutual-exclusion rule is preserved.
3. The existing percentage-to-amount conversion continues to work unchanged, including its Enter trigger, its D-01 availability check and the two-press confirmation.
4. When D-01 is not satisfied for the current entry, the derived percentage is either suppressed or shown together with the reason it is unavailable; a value contradicting the session's own rule is never displayed.
5. Clearing the amount clears the derived percentage, and clearing the percentage clears any derived amount.
6. The forward and reverse derivations round-trip: converting a percentage to an amount and back reproduces the original percentage within the two-decimal money rounding.
7. The behaviour is identical on the patient editor and on the patient account screen.
8. Regression: the percentage behaviour asserted by `AccountEntryPercentageProbeTests` and `PatientEditorPercentageConflictTests` continues to hold unchanged.

---

### U1-GAP-005 — Cash-receipt print-once enforcement

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The *configuration* for a print-once policy exists, is persisted and is editable. `src/TopLab.Domain/Settings/ReceiptSettings.cs:15` declares `public bool PrintOnce { get; private set; }` on the single-row settings aggregate (PK = 1); `ReceiptSettings.CreateDefault` initialises it to `false` at line 59 and `ReceiptSettings.Update` persists a new value at line 112. It is mapped in `src/TopLab.Infrastructure/Persistence/Configurations/ReceiptSettingsConfiguration.cs:17` (`b.Property(e => e.PrintOnce).IsRequired();`) and seeded by `HasData` at line 28 of the same file. It is exposed end-to-end through `GetReceiptSettingsQueryHandler` and `UpdateReceiptSettingsCommand` in `src/TopLab.Application/Features/SystemAndPrintSettings/`, and presented to the operator as a checkbox labelled "الطباعة مرة واحدة فقط" at `src/TopLab.Presentation/Views/Settings/ReceiptSettingsView.xaml:41`, bound to `ReceiptSettingsViewModel.PrintOnce` (`src/TopLab.Presentation/ViewModels/Settings/ReceiptSettingsViewModel.cs:114`).

**Remaining gap — the enforcement.** No reprint guard is wired. `src/TopLab.Infrastructure/Printing/ReceiptPrintingService.cs:21-24` states this explicitly: *"`ReceiptSettings.PrintOnce` is a display-only setting: no reprint-guard behavior is wired anywhere in the codebase, so this service treats every call as an independent print (documented per plan §4; a reprint guard would be a future behavioral decision, not part of this slice)."* A search of the production source for `PrintOnce` returns only the settings CRUD path — the domain property and its mutator, the configuration and seed, the settings command/handler/DTO/query, and the settings view model — plus that one comment. No other production code reads it. Corroborating inspection:
* `src/TopLab.Application/Features/PatientBilling/Commands/PrintReceipt/PrintReceiptCommand.cs` is documented as *"Ungated (mirrors RecordPayment — cashier action at the same desk)"* and carries only `int PatientId`.
* `PrintReceiptCommandHandler` (same folder) loads the receipt through `GetPatientReceiptQuery` and calls `IReceiptPrintingService.PrintReceiptAsync`; it performs no duplicate check and accepts no override parameter.
* `src/TopLab.Application/Features/PatientBilling/Queries/GetPatientReceipt/GetPatientReceiptQueryHandler.cs` reads `ReceiptSettings` for `settings.Currency` only (line 47); it never reads `PrintOnce`. The `ReceiptDto` it produces (`src/TopLab.Application/Features/PatientBilling/Common/PatientBillingDtos.cs:33-42`) contains no receipt number and no print identity — a receipt is a rendering of the patient's *current* account state, not a numbered document.
* `src/TopLab.Domain/Billing/PaymentOperation.cs:9-27` carries `PatientId`, `Amount`, `DiscountAmount`, `RefundAmount`, `IsExtraCharge`, `OperationType`, `ReceivedByUserId`, `OperationAtUtc`, `IsVoided` and the computed `IsEffectivelyZero`. There is no print flag, and the `PaymentOperation` snapshot in `ApplicationDbContextModelSnapshot.cs` shows no print column.
* The only print-state tracking in the solution belongs to result reports: `src/TopLab.Domain/Results/PatientTest.cs:48-53` declares `IsPrinted`, `PrintCount`, `LastPrintedByUserId` and `LastPrintedAtUtc`, maintained by `PatientTest.MarkPrinted` (lines 178-189).

**A print-blocking policy already exists elsewhere in the solution, and is the pattern a receipt guard would follow.** `src/TopLab.Domain/Users/User.cs:18` declares `public bool BlockPrintOnRemainingBalance { get; private set; }`, and six print handlers enforce it — `MarkResultPrintedCommandHandler`, `MarkProfilePrintedCommandHandler`, `MarkCultureReportPrintedCommandHandler`, `PrintCombinedReportCommandHandler`, `PrintHistoryReportCommandHandler` and `ExecuteBulkPrintCommandHandler`. Each applies the same shape: read the user row, and where `user.BlockPrintOnRemainingBalance` is set, the user is not an absolute permission holder, and `BalanceProbe.Balance(...)` shows an outstanding balance, refuse with the Arabic conflict message "يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة." The pattern — a per-user policy flag, a balance probe, a single Arabic refusal message, an absolute-permission bypass, and the check inside the print command handler — is exactly the shape a receipt print-once guard would take. The `BalanceProbe` helper it relies on is declared three times, once per feature that needs it, as `internal static class BalanceProbe`: `src/TopLab.Application/Features/ResultsEntry/Common/BalanceProbe.cs` (used by `MarkResultPrintedCommandHandler`, `MarkProfilePrintedCommandHandler`, `ExecuteBulkPrintCommandHandler`, and by `PrintCombinedReportCommandHandler` and `PrintHistoryReportCommandHandler` through the alias `using BalanceProbe = TopLab.Application.Features.ResultsEntry.Common.BalanceProbe;`), `src/TopLab.Application/Features/CultureResults/Common/BalanceProbe.cs` (used by `MarkCultureReportPrintedCommandHandler`) and `src/TopLab.Application/Features/PatientSearch/Common/BalanceProbe.cs`. All three are `internal` within the same `TopLab.Application` assembly, so the helper is reachable from a new receipt guard without a new shared abstraction.

**The established precedent for recording a print emission.** The invoice path already solves the same problem. `src/TopLab.Domain/Billing/InvoiceIssue.cs` is an append-only aggregate whose own documentation states: *"An invoice is an itemized, sequentially numbered statement of services (what was charged). Every physical print allocates a new numbered issue; reprints never reuse a number. Immutable after creation."* It carries `PatientId` as an application-level reference with a deliberately absent model foreign key, *"per the repo's FK-matrix convention — deleting a patient never cascades here and no model FK exists to assert on"*. `src/TopLab.Application/Features/PatientBilling/Commands/PrintInvoice/PrintInvoiceCommandHandler.cs` allocates it in `AllocateIssueAsync` with gapless numbering — `MAX(InvoiceNumber) + 1` inside the same unit of work as the insert, with the unique index as the backstop and one retry on a lost race, detected by the message-based `IsUniqueViolation` helper. The table and its indexes were created by `src/TopLab.Infrastructure/Persistence/Migrations/20260916113704_AddInvoiceIssues.cs` as `InvoiceIssues` with `IX_InvoiceIssues_InvoiceNumber` (unique) and `IX_InvoiceIssues_PatientId`.

**Consequence for a guard.** Because a receipt has no identity of its own and no emission is recorded, the system cannot currently distinguish a first receipt print from a repeat print. A guard requires either recorded emission state or a different emission rule.

#### Precise Required End State

**Established, and true under every option.**
1. `ReceiptSettings.PrintOnce` remains the control that expresses the policy; it must not be replaced by a second setting.
2. A receipt print must remain available at the two entry points that exist today: the `الإيصال` control on the registration screen (`src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml:65`) and the print-receipt action on the account screen (`src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml`).
3. A receipt print must not change the account arithmetic. The printed document is a projection of `GetPatientReceiptQuery`; recording that it was printed must not create, alter or void a `PaymentOperation` row, and must not change `TotalCharged`, `TotalPaid`, `CashRefundable` or `Balance`.
4. The two-press confirmation and the audit behaviour of the surrounding cashiering workflow must not be disturbed.
5. When the policy is enabled and a repeat print is refused, the operator must receive a clear Arabic message on the screen's error line, following the `ErrorMessage` convention in `PatientEditorViewModel`.
6. The policy must be evaluated at the moment of the print attempt and enforced in the Application layer, so that it cannot be bypassed by invoking the print command directly.

**Option-dependent requirements** are stated in the owner decision.

#### Affected Layers, Files, and Symbols

**Domain — expected to change if emission state is recorded.** `src/TopLab.Domain/Billing/` — a receipt-emission aggregate is expected alongside `InvoiceIssue` and `PaymentOperation`. Recording emission in a new aggregate, rather than as a mutable flag on `PaymentOperation`, preserves the append-only character of the ledger: `PaymentOperation` is immutable except for `Void()` (`src/TopLab.Domain/Billing/PaymentOperation.cs:96-99`). `src/TopLab.Domain/Billing/PatientAccountCalculator.cs` — not expected to change; a receipt-emission record is not a `PaymentOperation` and does not enter `TotalCharged`, `TotalPaid` or `CashRefundable`.

**Application — expected to change.** `src/TopLab.Application/Features/PatientBilling/Commands/PrintReceipt/PrintReceiptCommand.cs` and `.../PrintReceiptCommandHandler.cs` — the guard decision and, under the recording option, the allocation and recording of the emission, following the `AllocateIssueAsync` pattern of `PrintInvoiceCommandHandler`. `src/TopLab.Application/Features/PatientBilling/Queries/GetPatientReceipt/GetPatientReceiptQueryHandler.cs` — expected to remain unchanged unless the guard needs the current emission state, in which case a dedicated query is the expected route. `src/TopLab.Application/Features/PatientBilling/Common/PatientBillingDtos.cs` — expected to change only if emission state is carried on the receipt DTO.

**Infrastructure — expected to change if emission state is recorded.** `src/TopLab.Infrastructure/Persistence/ApplicationDbContext.DbSets.cs` — a `DbSet` for the new aggregate. `src/TopLab.Infrastructure/Persistence/Configurations/` — a new entity configuration. A new EF Core migration and an updated model snapshot would be required (Section H).

**Presentation — expected to change.** `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — the `PrintReceiptCommand` wiring at line 164 and the `PrintAsync` dispatch at lines 2185-2231, to surface a refusal message. `src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs` — the equivalent path.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/PatientBilling/Commands/PrintReceiptCommandHandlerTests.cs` is the direct precedent; `tests/TopLab.Application.Tests/Features/PatientBilling/GetPatientReceiptQueryHandlerTests.cs` covers the receipt projection.

#### Dependencies and Execution Prerequisites

**Prerequisite: U1-GAP-003.** The final set of ledger invariants on `PaymentOperation` must be settled before the receipt-print design is layered onto the billing domain, because the guard's storage choice interacts with whether `PaymentOperation` is being modified at the same time.

Existing components this item depends on: `ReceiptSettings.PrintOnce`, `PrintReceiptCommand`, `GetPatientReceiptQuery`, `IReceiptPrintingService` and its Infrastructure implementation `ReceiptPrintingService`, `PatientAccountCalculator`, the authorisation code `"CASH_DISBURSE_DEPOSIT"` already required by `GetPatientReceiptQuery`, and — for the recording option — the `InvoiceIssue` / `AllocateIssueAsync` precedent.

#### Owner Decision

**Established.** The policy switch exists, is persisted and is operator-visible. The codebase explicitly records the enforcement as a deferred behavioural decision and states that every call is currently treated as an independent print. The domain already provides the pattern a working implementation would follow, twice over: `PatientTest.MarkPrinted` records that a report was produced, by whom and when; and `InvoiceIssue` records that a numbered financial document was issued, with gapless numbering and an explicit no-FK link to the patient.

**Undecided.** How the policy is enforced and, consequently, what is recorded and who may authorise an exception.

**Option 1 — Record receipt emissions and refuse a repeat print unless the print is explicitly authorised.**
* *Meaning:* each receipt emission is recorded, following the `InvoiceIssue` pattern — a monotonically numbered, immutable record carrying the visit, the operator, the time and the account figures that were printed. When `PrintOnce` is enabled, a subsequent emission for the same visit requires an explicit authorisation, which is itself recorded.
* *Functional consequences:* the system can distinguish a first print from a repeat, and the receipt history becomes auditable in the way invoice issuance already is.
* *Operational impact:* the cashier receives an explicit message on a repeat attempt and must confirm the authorisation — visible friction on a legitimate re-print, for example when a customer has lost their receipt.
* *Architectural impact:* a new append-only aggregate in the Domain following `InvoiceIssue`, a `DbSet`, an entity configuration, a migration, and the guard plus allocation inside the `PrintReceiptCommand` handler, reusing the gapless-numbering and retry-on-race pattern of `PrintInvoiceCommandHandler.AllocateIssueAsync`. The ledger arithmetic is unaffected because the new aggregate is not a `PaymentOperation`.
* *Persistence impact:* a new table is required (Section H).
* *Risks and trade-offs:* the most faithful to the setting's own wording and to two existing precedents. It adds a table and a confirmation step, and it raises what "the same receipt" means when the account has changed between prints — a receipt is a projection of current state, so two prints of the same visit may legitimately show different totals.
* *Rule conflicts:* none. The append-only ledger is preserved because emission is recorded in a separate aggregate rather than by mutating `PaymentOperation`.

**Option 2 — Restrict receipt emission to the payment flow, without recording emissions.**
* *Meaning:* when `PrintOnce` is enabled, the receipt is produced only as part of recording a payment, and the standalone receipt-print control is disabled. The refusal is expressed as a control-state decision in the print command handler, in the shape `BlockPrintOnRemainingBalance` already uses.
* *Functional consequences:* the operator cannot print a receipt on demand while the policy is enabled. No emission history exists.
* *Operational impact:* a customer who has lost their receipt cannot be served a reprint while the policy is enabled; the cashier would have to disable the setting, which is a global single-row setting shared by every workstation.
* *Architectural impact:* the guard is a control-state decision in the Application layer; no new aggregate and no migration. It requires the receipt to be emitted from the payment path, which today it is not: `PatientEditorViewModel.RecordPaymentAsync` (lines 1931-1993) records the payment and refreshes the account and does not dispatch a receipt print.
* *Persistence impact:* none.
* *Risks and trade-offs:* the smallest change and no schema cost, but it is all-or-nothing: it removes the on-demand re-print capability entirely, and it couples the behaviour to a global setting that is scoped to no visit, user or shift.
* *Rule conflicts:* none to existing invariants. It does conflict with the operational reality that a cashier must be able to re-issue a receipt, and it silently changes the meaning of the `الإيصال` control on the registration screen.

**Option 3 — Retain current behaviour: `PrintOnce` remains display-only and every print is independent.**
* *Meaning:* no guard; the setting continues to be stored and displayed but changes nothing.
* *Functional consequences:* duplicate cash receipts can be produced without restriction or record.
* *Operational impact:* the cashier retains full freedom, and there is no way to establish afterwards how many receipts a visit generated.
* *Architectural impact:* none. *Persistence impact:* none.
* *Risks and trade-offs:* no implementation cost. It leaves a setting whose label promises a control the system does not apply, and leaves the duplicate-receipt exposure in place.
* *Rule conflicts:* none technically; the mismatch between the setting's label and its effect is the substantive cost.

**Recommended option: Option 1.**

**Reasoning.** Option 1 is the only option that both enforces the policy and preserves an auditable record, and it follows two patterns the codebase already uses for the identical problem: `PatientTest.MarkPrinted` for print state, and `InvoiceIssue` for a numbered, immutable financial document issued on every physical print. It also leaves `PaymentOperation` and `PatientAccountCalculator` untouched, so the ledger arithmetic and the append-only model are preserved. Option 2 achieves the restriction without a schema cost but removes a capability the cashier genuinely needs and couples it to a global setting. Option 3 leaves an operator-visible control that does nothing.

**Owner decision required:** When `ReceiptSettings.PrintOnce` is enabled, how should a repeat cash-receipt print be handled — refused outright, permitted only with an explicit authorisation that is recorded, or restricted to the payment flow — and should receipt emissions be recorded so that a repeat can be distinguished from a first print?

*The recommendation is advisory only; the choice is the owner's.*

#### Migration Requirement

**Conditional — the migration requirement depends on the owner decision.**

* **Option 1 (record emissions):** a new migration **is** required. No receipt-emission table exists: `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` created the `PaymentOperations` and `InvoiceIssues` tables, and the current `ApplicationDbContextModelSnapshot.cs` shows no emission entity. The new table would follow the shape of the existing append-only `InvoiceIssues` table, created by `20260916113704_AddInvoiceIssues` and configured in `src/TopLab.Infrastructure/Persistence/Configurations/InvoiceIssueConfiguration.cs` — identity key, a non-nullable `PatientId` **without** a model foreign key to `Patients` (the repo's documented FK-matrix convention), an emission number, the issuing user, the timestamp, the printed figures and a unique index on the number. No change to the existing `PaymentOperations` table is required.
* **Option 2 (restrict to the payment flow) and Option 3 (retain current behaviour):** no new migration is required. `ReceiptSettings.PrintOnce` already exists as a `bit` column on the `ReceiptSettings` table, created in `20260828052248_BaselineDataModel.cs` and mapped in `ReceiptSettingsConfiguration.cs`.

#### Acceptance Conditions

1. With `ReceiptSettings.PrintOnce` disabled, a receipt can be printed any number of times, exactly as today.
2. With `PrintOnce` enabled and a first print, the receipt is produced, and under Option 1 the emission is recorded.
3. With `PrintOnce` enabled and a subsequent print, the print is refused with a clear Arabic message unless the authorisation required by the chosen option is supplied.
4. The refusal is enforced in the Application layer, so that invoking the print command directly cannot bypass it.
5. Recording a receipt emission creates no `PaymentOperation` row and changes no figure: `TotalCharged`, `TotalPaid`, `CashRefundable` and `Balance` are unchanged by a print.
6. Under Option 1, the recorded emission carries the visit, the issuing user, the timestamp and the printed figures, and a repeat print is detected from that record rather than from the receipt content.
7. Under Option 2, the standalone receipt-print control is disabled while `PrintOnce` is enabled, and the receipt is produced only through the payment path.
8. A user without the permission required by `GetPatientReceiptQuery` cannot obtain a receipt.
9. The setting remains editable in the receipt settings screen and continues to be stored in the single-row `ReceiptSettings` table.
10. Regression: the receipt content itself — the figures, the currency and the charged-test list — is unchanged under every option.

---


### U1-GAP-006 — Unfinished-results filter on the patient search view

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The state that defines "unfinished" is modelled and persisted. `src/TopLab.Domain/Results/PatientTest.cs` declares `EnteredAtUtc` (line 39), `IsReviewed` (line 45), `IsPrinted` (line 48) and `IsDelivered` (line 56), and `src/TopLab.Domain/PatientStatus/PatientStatusCalculator.cs:30-45` maps them onto a per-test stage: not entered ⇒ stage 1, entered but not reviewed ⇒ stage 2, reviewed but not printed ⇒ stage 3, printed but not delivered ⇒ stage 4, delivered ⇒ stage 6. The aggregate status that results is the seven-valued `TopLab.Domain.PatientStatus.PatientAggregateStatus` (S1–S7). The columns are indexed for exactly these predicates: `src/TopLab.Infrastructure/Persistence/Configurations/PatientTestConfiguration.cs` declares `b.HasIndex(e => new { e.IsReviewed, e.IsPrinted, e.IsDelivered });`, and the same index is created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` as `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered`. A related filter already exists on the patient search view: `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml:69` binds a `غير مُسلَّم` checkbox to `NotDelivered` (`src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs:126`), forwarded to the query at line 612 and translated at `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQueryHandler.cs:56-59` into "patients with at least one `PatientTest` that is not delivered".

**Remaining gap.** The patient search view has no filter for visits whose results are still outstanding. Its complete filter surface is eleven properties: `SearchText` (line 114), `CardNumberText` (120), `NotDelivered` (126), `LabIdText` (132), `SelectedTreatingDoctor` (246), `SelectedReferralEntity` (259), `SelectedTest` (272), `SelectedSex` (285), `SelectedAgeUnit` (298), `AgeFrom` (318) and `AgeTo` (334), together with the four option lists at lines 394-402. None of them selects visits by result state. The equivalent predicates are already applied elsewhere: `src/TopLab.Presentation/Views/Patients/ResultsWorklistView.xaml:14-35` provides three-state filters bound to `HasResult`, `IsReviewed` and `IsPrinted`, and `src/TopLab.Presentation/Views/WorkSheets/WorkSheetsView.xaml:74` provides an `UnfinishedOnly` toggle ("غير مُكتملة فقط") forwarded at `src/TopLab.Presentation/ViewModels/WorkSheets/WorkSheetsViewModel.cs:437`.

#### Precise Required End State

1. The patient search view must offer a filter restricting the result set to visits that still have outstanding work, consistent with the domain's own definition: at least one ordered test that has not been entered, or has been entered but not reviewed.
2. The filter must be a tri-state control, so that "no filter" remains reachable, exactly as the three-state controls in `ResultsWorklistView.xaml` are documented to do — the comment at lines 16-19 of that view records that a two-state box bound to a nullable boolean cannot express "no filter" and that the row could never be cleared back to showing everything.
3. The filter must integrate with the existing filter machinery: combine conjunctively with the other filters, be cleared by the existing `ClearFiltersCommand` (`src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml:68`), and survive the paging controls.
4. The filter must be expressed as a parameter on the shared search query, in the same manner as the existing `NotDelivered` parameter, so the predicate is evaluated in the database rather than in the view model.
5. A visit with no ordered tests must be treated consistently with the domain's own treatment of a test-less visit in `PatientStatusCalculator` — not as unfinished on the result axis — and the filter must not exclude it by accident.
6. The filter must not change the shape of the result grid, the paging behaviour or the actions available on each row.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.** `PatientTest` already carries `EnteredAtUtc` and `IsReviewed`, and `PatientStatusCalculator` defines the stage mapping.

**Application — expected to change.** `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs` — a new optional parameter appended at the tail with a `null` default, consistent with the record's documented rule that the positional shape `Text, Page, PageSize` must not be reordered and every additional filter is appended at the tail and is inert when null. `.../SearchPatientsGlobalQueryHandler.cs` — the corresponding predicate. `.../SearchPatientsGlobalQueryValidator.cs` — expected to require verification during implementation, to confirm an appended optional parameter needs no additional rule.

**Infrastructure — not expected to change.** The required columns and the composite index `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered` already exist.

**Presentation — expected to change.** `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml` — the tri-state control in the filter row. `src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs` — the nullable boolean property, its forwarding to the query (the pattern at line 612) and its participation in `ClearFiltersCommand`.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/PatientSearch/SearchPatientsGlobalFilterTests.cs` and `.../SearchPatientsGlobalQueryHandlerTests.cs` are the direct precedents.

#### Dependencies and Execution Prerequisites

**Prerequisite for contract hygiene only, not for correctness: U1-GAP-007 and U1-GAP-008.** All three extend the same query record, whose documented rule requires appended parameters to be added in a defined order. The three filters are logically independent of one another.

Existing components this item depends on: `SearchPatientsGlobalQuery` and its handler, `PatientSearchViewModel`'s filter-reload mechanism (the generation-guarded reload at lines 605-620), and the `PatientTest` lifecycle columns.

#### Owner Decision

No decision required

#### Migration Requirement

**No new migration is required.** The predicate relies on `PatientTests.EnteredAtUtc`, `PatientTests.IsReviewed` and `PatientTests.IsPrinted`, all created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs`, and on the index `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered` created in the same migration and declared in `src/TopLab.Infrastructure/Persistence/Configurations/PatientTestConfiguration.cs`. The change is a read-side query predicate only.

#### Acceptance Conditions

1. With the unfinished filter indeterminate, the result set is identical to the set returned with no filter.
2. With the filter applied, the result set contains exactly the visits with at least one ordered test that is not yet entered, or is entered but not reviewed.
3. The filter combines conjunctively with the doctor, referral entity, test, sex, age band and date-range filters, and the result set is the conjunction of all of them.
4. `ClearFiltersCommand` returns the filter to the indeterminate state along with the other filters.
5. The filter is evaluated in the database: the row count and the paging controls reflect the filtered set.
6. A visit with no ordered tests is not returned by the filtered set, consistent with `PatientStatusCalculator`'s treatment of a test-less visit.
7. Regression: the existing `NotDelivered` filter and the six granular filters continue to behave exactly as they do today.

---

### U1-GAP-007 — Unverified-results filter on the patient search view

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The unverified state is a first-class, persisted and indexed domain fact. `src/TopLab.Domain/Results/PatientTest.cs:45` declares `public bool IsReviewed { get; private set; }`, set through the `MarkReviewed` lifecycle method, and `src/TopLab.Domain/PatientStatus/PatientStatusCalculator.cs:36-38` maps "entered but not reviewed" onto stage 2, reported as `PatientAggregateStatus.S3`. The column is part of the composite index `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered` in `src/TopLab.Infrastructure/Persistence/Configurations/PatientTestConfiguration.cs` and in `20260828052248_BaselineDataModel.cs`. A working unverified filter already exists in the results worklist: `src/TopLab.Presentation/Views/Patients/ResultsWorklistView.xaml:22-24` provides a tri-state `IsReviewed` control, and `src/TopLab.Application/Features/ResultsEntry/Commands/BulkPrint/BulkPrintPreflightQueryHandler.cs` computes the same notion when it selects the tests that are "verified" for bulk printing.

**Remaining gap.** The patient search view exposes no unverified filter. Its complete filter surface is enumerated at U1-GAP-006 and contains no member that selects visits by review state. The grid does show an aggregate status column (`PatientSearchView.xaml:127`, bound to `AggregateStatus`), so the information is visible per row but cannot be used to narrow the set.

#### Precise Required End State

1. The patient search view must offer a tri-state filter restricting the result set to visits with at least one ordered test that has been entered but not reviewed.
2. The tri-state semantics must match the existing convention: indeterminate means "no filter", unchecked means "only unverified rows", checked means "only verified rows" — the mapping documented at `ResultsWorklistView.xaml:28-33` for the equivalent printed filter.
3. The filter must combine conjunctively with the other filters, be cleared by `ClearFiltersCommand`, and be evaluated in the database through the shared search query.
4. The predicate must use the persisted `IsReviewed` flag rather than recomputing the aggregate status, so the filter remains a simple indexed predicate.
5. The filter must not change the result grid, the row actions or the paging behaviour.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.**

**Application — expected to change.** `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs` — a new optional parameter appended at the tail. `.../SearchPatientsGlobalQueryHandler.cs` — the predicate.

**Infrastructure — not expected to change.**

**Presentation — expected to change.** `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml` — the tri-state control. `src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs` — the property, its forwarding and its participation in the clear-filters path.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/PatientSearch/SearchPatientsGlobalFilterTests.cs` is the direct precedent.

#### Dependencies and Execution Prerequisites

**Prerequisite for contract hygiene only, not for correctness: U1-GAP-006 and U1-GAP-008.** The three filters append to one shared query record.

Existing components this item depends on: `SearchPatientsGlobalQuery` and its handler, `PatientTest.IsReviewed`, and the tri-state binding convention established in `ResultsWorklistView.xaml`.

#### Owner Decision

No decision required

#### Migration Requirement

**No new migration is required.** `PatientTests.IsReviewed` is a `bit` column created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` and covered by the index `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered` created in the same migration.

#### Acceptance Conditions

1. With the filter indeterminate, the result set is unchanged.
2. Set to "only unverified", the result set contains exactly the visits with at least one entered, not-yet-reviewed test.
3. Set to "only verified", the result set contains exactly the visits whose every ordered test is reviewed.
4. The filter combines conjunctively with the other filters and is cleared by `ClearFiltersCommand`.
5. The predicate uses the persisted `IsReviewed` flag, so the grid's `AggregateStatus` column remains consistent with the filter.
6. Regression: the tri-state behaviour of the equivalent control in the results worklist is unchanged.

---

### U1-GAP-008 — Unprinted-results filter on the patient search view

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The unprinted state is modelled in detail. `src/TopLab.Domain/Results/PatientTest.cs:48-53` declares `IsPrinted`, `PrintCount`, `LastPrintedByUserId` and `LastPrintedAtUtc`, maintained by `PatientTest.MarkPrinted` (lines 178-189), which increments the counter and stamps the operator and the time. `src/TopLab.Domain/PatientStatus/PatientStatusCalculator.cs:39-40` maps "reviewed but not printed" onto stage 3, reported as `PatientAggregateStatus.S4`. The `IsPrinted` column is covered by the composite index `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered`. A working unprinted filter already exists in the results worklist: `src/TopLab.Presentation/Views/Patients/ResultsWorklistView.xaml:34-35` provides a tri-state `IsPrinted` control, and `src/TopLab.Application/Features/ResultsEntry/Commands/BulkPrint/BulkPrintPreflightQueryHandler.cs` computes `verified.Any(pt => pt.IsPrinted)` to raise the `RequiresReprintConfirmation` flag surfaced in `src/TopLab.Presentation/Views/Patients/BulkPrintDialogWindow.xaml`.

**Remaining gap.** The patient search view exposes no unprinted filter. Its complete filter surface is enumerated at U1-GAP-006 and contains no member that selects visits by print state.

#### Precise Required End State

1. The patient search view must offer a tri-state filter restricting the result set to visits with at least one ordered test that has been reviewed but not printed.
2. The tri-state semantics must match the convention documented at `ResultsWorklistView.xaml:28-33`: indeterminate means "no filter", unchecked means "only unprinted rows", checked means "only printed rows".
3. The filter must combine conjunctively with the other filters, be cleared by `ClearFiltersCommand`, and be evaluated in the database through the shared search query.
4. The predicate must use the persisted `IsPrinted` flag.
5. The filter must not change the result grid, the row actions or the paging behaviour.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.**

**Application — expected to change.** `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs` — a new optional parameter appended at the tail. `.../SearchPatientsGlobalQueryHandler.cs` — the predicate.

**Infrastructure — not expected to change.**

**Presentation — expected to change.** `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml` — the tri-state control. `src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs` — the property, its forwarding and its participation in the clear-filters path.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/PatientSearch/SearchPatientsGlobalFilterTests.cs` is the direct precedent.

#### Dependencies and Execution Prerequisites

**Prerequisite for contract hygiene only, not for correctness: U1-GAP-006 and U1-GAP-007.** The three filters append to one shared query record.

Existing components this item depends on: `SearchPatientsGlobalQuery` and its handler, `PatientTest.IsPrinted`, and the tri-state binding convention established in `ResultsWorklistView.xaml`.

#### Owner Decision

No decision required

#### Migration Requirement

**No new migration is required.** `PatientTests.IsPrinted` and `PatientTests.PrintCount` are columns created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs`, and `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered` was created in the same migration.

#### Acceptance Conditions

1. With the filter indeterminate, the result set is unchanged.
2. Set to "only unprinted", the result set contains exactly the visits with at least one reviewed, not-yet-printed test.
3. Set to "only printed", the result set contains exactly the visits whose reviewed tests have all been printed.
4. The filter combines conjunctively with the other filters and is cleared by `ClearFiltersCommand`.
5. The predicate uses the persisted `IsPrinted` flag.
6. Regression: the tri-state behaviour of the equivalent control in the results worklist is unchanged.

---

### U1-GAP-009 — Explicit search-mode selector on the patient search view

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The searching capability a mode selector would expose is fully present and unified. `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs` declares `record SearchPatientsGlobalQuery(string? Text, int Page = 1, int PageSize = 50, …, string? CardNumber = null, bool? NotDelivered = null)`. `.../SearchPatientsGlobalQueryHandler.cs:33-45` matches the free-text term against four different attributes in a single disjunctive clause: `FullName` contains the term (subject to the `EnablePatientNameSearchAssist` setting read at line 25), `FullName` equals the term, `LabId` equals the term, `NationalId` equals the term, or the patient owns a `PatientPhoneNumber` containing the term. An exact card-number predicate is applied at lines 48-52, and an exact lab-id lookup exists as a separate command, `src/TopLab.Application/Features/PatientSearch/Queries/GetPatientByLabId/GetPatientByLabIdQuery.cs`, dispatched by `PatientSearchViewModel.FetchByLabIdAsync` (lines 640-654) from the `جلب بالكود` control at `PatientSearchView.xaml:17`.

**Remaining gap.** There is no explicit mode selector on the search view. `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml:7-25` provides a single `SearchText` box, a `CardNumberText` box, a `LabIdText` box with its fetch button, the branch-notice button, a back button and a reprint-barcode button; the filter row below adds the six granular filters. The operator cannot choose to search by name only, by phone only, by lab id only or by national id only — the single box always searches all four at once. The complete filter surface of the view model, enumerated at U1-GAP-006, contains no mode member, and a search of the whole source tree for `SearchMode`, `SearchBy`, `SearchScope` and `MatchMode` returns no member of any kind.

**A behavioural consequence that constrains the end state.** The existing semantics are deliberately mixed: the name match is a *substring* match, the lab id and national id matches are *exact* matches, and the phone match is a substring match over the phone-number table. An explicit mode selector therefore changes matching semantics for the selected attribute, and today's expectation of "search by lab id" is a global box that also returns name matches.

#### Precise Required End State

1. The patient search view must offer an explicit mode selector scoping the free-text search to one attribute at a time, covering at least: patient name, phone number, laboratory identifier, national identifier, and an unrestricted mode equivalent to today's behaviour.
2. The default state of the selector must reproduce today's behaviour exactly — the unrestricted global search — so that no existing workflow changes on upgrade.
3. The per-mode matching semantics must be defined and consistent: an identifier mode must match exactly, and a name or phone mode must match as a substring, each case-insensitively. The existing `EnablePatientNameSearchAssist` setting must continue to govern the name assist behaviour in every mode that matches on the name, and it must not be silently extended to the identifier modes.
4. The mode must be an input to the shared search query, so that the predicate is evaluated in the database and the paging totals stay correct. A mode that filtered only within the returned page would be incorrect.
5. The existing dedicated controls — the exact card-number field and the exact lab-id fetch button — must continue to work unchanged and must not conflict with the new selector.
6. The mode must be cleared by the existing `ClearFiltersCommand` only if the mode is treated as a filter; if it is a persistent view preference it must survive filter clearing, and that behaviour must be deliberate rather than incidental.
7. The mode must not change the result grid, the row actions or the paging behaviour.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.**

**Application — expected to change.** `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs` — a new optional mode parameter appended at the tail, and a possible narrowing of the `Text` clause. `.../SearchPatientsGlobalQueryHandler.cs` — the per-mode predicate, replacing the current disjunctive `Text` clause at lines 33-45 with a mode-directed one while retaining the unrestricted default. `.../SearchPatientsGlobalQueryValidator.cs` — expected to require verification during implementation.

**Infrastructure — not expected to change.**

**Presentation — expected to change.** `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml` — the mode selector control. `src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs` — the mode property, its forwarding to the query and its treatment by `ClearFiltersCommand`.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/PatientSearch/SearchPatientsGlobalQueryHandlerTests.cs`, `.../SearchPatientsGlobalFilterTests.cs` and `.../SearchGuardrailTests.cs` are the direct precedents. The guardrail tests are expected to constrain the new parameter so that a mode cannot widen the result set relative to today's global search.

#### Dependencies and Execution Prerequisites

**Prerequisite: U1-GAP-006, U1-GAP-007 and U1-GAP-008.** The mode parameter is appended to the same query record as the three result-state filters, and the record's documented rule requires appended parameters to be added in a defined order.

Existing components this item depends on: `SearchPatientsGlobalQuery` and its handler, `GetPatientByLabIdQuery`, the `EnablePatientNameSearchAssist` setting on `SystemSettings`, and the filter-reload mechanism in `PatientSearchViewModel`.

#### Owner Decision

No decision required

#### Migration Requirement

**No new migration is required.** The change alters a read-side query predicate and a view-model property. The queried columns — `Patients.FullName`, `Patients.NationalId`, `Patients.LabId`, `PatientPhoneNumbers.PhoneNumber` — already exist, and the matching indexes already exist in `src/TopLab.Infrastructure/Persistence/Configurations/PatientConfiguration.cs` (`b.HasIndex(e => e.FullName)`, `b.HasIndex(e => e.LabId)`, `b.HasIndex(e => e.NationalId)`) and in `20260828052248_BaselineDataModel.cs`.

#### Acceptance Conditions

1. In the default mode, the result set is identical to the set returned today by the single free-text box, including the substring match on name, the exact match on laboratory identifier and national identifier, and the substring match over phone numbers.
2. In name mode only name matches are returned; in phone mode only phone matches; in laboratory-identifier mode only exact laboratory-identifier matches; in national-identifier mode only exact national-identifier matches.
3. `EnablePatientNameSearchAssist = false` suppresses the substring name match in every mode that matches on the name, and does not affect the identifier modes.
4. The mode is applied as a database-side predicate, so the paging totals and the row count reflect the selected mode.
5. The dedicated card-number field and the exact laboratory-identifier fetch button continue to work unchanged and are unaffected by the mode.
6. The mode is either cleared together with the other filters or retained, and that behaviour is deliberate and documented rather than incidental.
7. Regression: the existing `CardNumber` and `NotDelivered` parameters behave exactly as they do today.

---

### U1-GAP-010 — Full test-catalogue browser for the registration screen

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The complete test catalogue is already available for browsing and ordering directly on the registration screen, without any dialog. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml:537-546` provides the three mutually exclusive catalogue modes — `TM` (all tests), `TG` (search by name or code) and `CG` (custom groups) — bound to `IsTmMode`, `IsTgMode` and `IsCgMode`. The `TM` mode shows every test: `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs:760` evaluates `CatalogFilterMode.Tm => true` in the catalogue filter, so no filter narrows the list. The list itself is `PatientEditorView.xaml:579-591`, a `ListView` over `FilteredCatalogTests` whose item template carries a per-row "إضافة" control bound to `AddTestToSelectionCommand` (lines 583-585). The catalogue is supplied by `src/TopLab.Application/Features/PatientRegistration/Queries/GetRegistrationCatalog/GetRegistrationCatalogQueryHandler.cs:31`, which aggregates `SearchTestCatalogQuery(null, null)` — the unfiltered catalogue — together with `GetTestGroupsQuery`, `GetPatientTitlesQuery`, `GetMedicalConditionTypesQuery` and `GetSystemSettingsQuery`, and delivers it as `RegistrationCatalogDto` (`src/TopLab.Application/Features/PatientRegistration/Common/PatientRegistrationDtos.cs:83-93`), whose `Tests` member is `IReadOnlyList<TestSummaryDto>`.

**Remaining gap.** The presentation the catalogue is missing is a hierarchical, navigable one. A search of `src/TopLab.Presentation/` for `TreeView`, `HierarchicalDataTemplate`, `Expander`, `CollectionViewSource`, `IGrouping`, `GroupDescriptions` and `PropertyGroupDescription` returns nothing, so no grouped or tree control exists anywhere in the application. The catalogue on the registration screen is a flat list with a text filter, and the text filter matches only the test name and code — `PatientEditorViewModel.MatchesCatalogFilter` (lines 779-781) tests `test.Name.Contains(needle, …) || test.TestCode.Contains(needle, …)` and does not consider the test's group, even though the catalogue DTO carries it: `TestSummaryDto` (`src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Common/`) declares `int? TestGroupId` and `string? TestGroupName`. The administrative catalogue screen, `src/TopLab.Presentation/Views/Lab/TestCatalogView.xaml:49`, is likewise a flat `DataGrid` with a `TestGroupName` column rather than a tree, and it is a maintenance screen rather than an ordering surface. Consequently a registrar cannot navigate the catalogue by department or group, nor see the surrounding structure of a test, from the registration screen.

#### Precise Required End State

1. The registration screen must provide a way to browse the complete test catalogue with a hierarchical structure — by test group, then by test — so a registrar can navigate to a group and see its members together rather than scanning a flat list and typing filters.
2. The hierarchical browser must be reachable from the registration screen and must not require the registrar to abandon the visit being registered.
3. The browser must be able to add a test to the current visit's selection with the same effect as the existing per-row "إضافة" control, and must respect the same price resolution used by `AddTestToSelectionCommand` and `AddTestsToVisitCommand` — the contract price list, then the lab-to-lab rate, then the custom-group rate, then the standard catalogue price, as resolved by `src/TopLab.Application/Features/PatientRegistration/Common/TestPriceResolver.cs`.
4. The browser must show the information the flat list shows — test name, test code and the patient price — and may additionally show the test's group, which the catalogue DTO already carries.
5. The browser must not change the behaviour of the existing flat list, which must continue to work as it does today.
6. The browser must respect the existing catalogue-mode model, so that the `TM`, `TG` and `CG` semantics continue to describe what the registrar is looking at.
7. The browser must not alter the visit's existing ordered tests and must not affect the ledger.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.**

**Application — expected to require verification, and possibly minor change.** `src/TopLab.Application/Features/PatientRegistration/Queries/GetRegistrationCatalog/GetRegistrationCatalogQueryHandler.cs` — expected to require verification during implementation, to confirm that the aggregated catalogue response is sufficient for a grouped presentation, or to require an additional grouped projection. `TestSummaryDto` already carries `TestGroupId` and `TestGroupName`, and `GetTestGroupsQuery` already returns the group list, so no new query is anticipated. `src/TopLab.Application/Features/PatientRegistration/Queries/GetRegistrationCatalog/GetRegistrationCatalogQuery.cs` and `src/TopLab.Application/Features/PatientRegistration/Common/PatientRegistrationDtos.cs` — expected to require verification if the aggregated shape changes.

**Infrastructure — not expected to change.**

**Presentation — expected to change.** `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` — the entry control that opens the browser. `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — the browser's view model and the reuse of `AddTestToSelectionCommand` (constructed at line 168) for ordering. A new browser window and its view model are justified, following the established dialog idiom in `src/TopLab.Presentation/Views/Patients/` — for example `ExternalEntityPickerWindow` and `TestCommentPickerWindow` — and the `IDialogService` pattern referenced by `PatientEditorViewModel`.

**Tests — expected to change.** `tests/TopLab.Presentation.Tests/Patients/PatientEditorCatalogFilterTests.cs` and `.../PatientEditorCatalogModeTests.cs` are the direct precedents and are the expected regression guards for the unchanged flat list.

#### Dependencies and Execution Prerequisites

**Prerequisite for design convenience only: U1-GAP-011 benefits from, but does not require, this item.** If the browser is implemented as a group tree, the group-ordering action can be hosted inside it, which is the natural placement. U1-GAP-011 is nevertheless implementable independently, and this item is implementable independently of it.

Existing components this item depends on: `GetRegistrationCatalogQueryHandler`, `RegistrationCatalogDto`, `TestSummaryDto`, `TestPriceResolver`, `AddTestToSelectionCommand`, `AddTestsToVisitCommand` and the dialog conventions in the Patients views folder.

#### Owner Decision

No decision required

#### Migration Requirement

**No new migration is required.** The catalogue data already exists and is already queried: the `Tests` and `TestGroups` tables were both created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs`, and `Tests.TestGroupId` is a mapped foreign key with an index in `src/TopLab.Infrastructure/Persistence/Configurations/TestConfiguration.cs` (`b.HasOne<TestGroup>().WithMany().HasForeignKey(e => e.TestGroupId).OnDelete(DeleteBehavior.SetNull);` and `b.HasIndex(e => e.TestGroupId);`). The change is presentation-side only.

#### Acceptance Conditions

1. A registrar can open a hierarchical view of the complete catalogue from the registration screen, organised by test group, without abandoning the visit being registered.
2. Every test in the catalogue can be selected and added to the visit with the same effect as the existing per-row "إضافة" control.
3. The price applied to a test added from the browser is identical to the price applied when the same test is added from the flat list, under the same price list, account type and custom-group circumstances.
4. The browser displays the test name, the test code and the patient price, and may display the test's group, which the catalogue DTO already carries.
5. The existing flat list, its three modes and its filter continue to behave exactly as they do today.
6. The browser alters no other field of the visit and creates no ledger row.
7. A test already ordered on the visit is handled exactly as the existing flat list handles that case.

---

### U1-GAP-011 — One-click ordering of a master test group

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** A group-ordering control already exists on the registration screen. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml:493-499` provides a `مجموعة:` label, a `ComboBox` bound to `CustomGroups` and `SelectedCustomGroup`, an "إضافة" control bound to `AddCustomGroupCommand`, and the clear-all control. `AddCustomGroupCommand` is constructed at `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs:201` and implemented by `AddCustomGroupAsync` (lines 1851-1860), which dispatches `AddCustomGroupToVisitCommand`. That command and its handler live in `src/TopLab.Application/Features/PatientRegistration/Commands/AddCustomGroupToVisit/`; the handler expands the group's items into `PatientTest` rows, resolving each price through `TestPriceResolver.Resolve` exactly as individual ordering does, and returns `Result<IReadOnlyList<int>>` because the expansion creates many rows.

**Remaining gap.** The existing control covers only *custom* groups — user-defined bundles maintained in `src/TopLab.Presentation/Views/Lab/CustomGroupsView.xaml`, backed by the `CustomGroup` and `CustomGroupItem` entities. The **master test group** catalogue — the classification of tests by department maintained in `src/TopLab.Presentation/Views/Lab/TestGroupsView.xaml` and backed by `TopLab.Domain.Tests.TestGroup` — cannot be ordered in one action from the registration screen. The evidence is threefold. First, the master groups are loaded into the editor but never bound: `PatientEditorViewModel.cs:379` declares `public ObservableCollection<TestGroupDto> TestGroups { get; } = new();` and lines 924-927 populate it from the catalogue, but a search of `PatientEditorView.xaml` for `TestGroups` returns no occurrence, although `RegistrationCatalogDto` delivers the very same `IReadOnlyList<TestGroupDto>` to the editor. Second, the catalogue's text filter ignores the group: `MatchesCatalogFilter` (lines 779-781) matches the test name and code only, though the catalogue DTO carries `TestGroupId` and `TestGroupName`. Third, no command expands a master test group into a visit: a search across the solution for `AddTestGroup` and `TestGroupToVisit` returns no result, and the only commands that create `PatientTest` rows are `AddTestsToVisitCommand` (individual tests), `AddProfileToVisitCommand` (one order for a profile) and `AddCustomGroupToVisitCommand` (custom groups). The master group is, in domain terms, a classification rather than an orderable bundle: `TopLab.Domain.Tests.TestGroup` holds only `Name` and `IsActive`, and its members are the `Test` rows whose `TestGroupId` points at it.

**The expansion precedent to follow.** `AddCustomGroupToVisitCommandHandler` is the established shape for a many-row expansion: it refuses an unknown or soft-deleted visit, refuses an empty group with the message `"المجموعة فارغة."`, verifies every referenced test exists, resolves each price through `TestPriceResolver.Resolve`, creates the rows and returns their identifiers. The group-to-tests relationship is already queryable in the same way the laboratory's own group screen does it: `src/TopLab.Presentation/ViewModels/Lab/TestGroupsViewModel.cs:115-123` groups the `Test` rows on `TestGroupId` to obtain each group's test count.

**Ordering precondition to reuse.** `src/TopLab.Application/Features/PatientRegistration/Commands/ClearAllTests/ClearAllTestsCommandHandler.cs` establishes the rule that a visit's test list may only be altered while the visit is still the patient's *first registration* — the earliest non-deleted `Patient` row in its `LabId` group, compared on `RegistrationDateUtc` with `PatientId` ascending as the tie-breaker — with the Arabic message `FirstRegistrationOnlyMessage` = "لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة." and a 24-hour check. A bulk ordering command must be consistent with that rule.

#### Precise Required End State

1. The registration screen must provide a control that lists the master test groups and, on selection, orders every active test belonging to the chosen group onto the current visit.
2. The expansion must use the same price resolution as every other ordering path — contract price list, then lab-to-lab rate, then custom-group rate, then the standard catalogue price — through `TestPriceResolver.Resolve`, so a group expansion produces the same prices as ordering the same tests individually.
3. The expansion must respect the same preconditions as `AddCustomGroupToVisitCommandHandler`: the visit must exist and must not be soft-deleted; the group must contain at least one test, and a group with no tests must produce the existing Arabic conflict message rather than a silent no-op.
4. Only active tests must be expanded, consistent with `IsActive` on `TopLab.Domain.Tests.Test` and with the inactive-group exclusion already applied by `GetTestGroupsQueryHandler`.
5. A test already ordered on the visit must not be added a second time. The behaviour on a duplicate must be defined — rejected, or skipped with a count reported — and must be consistent with the behaviour of the existing custom-group path and with the no-duplicate rule in `CreatePatientCommandValidator`.
6. The control must be disabled when no visit is loaded, matching the `IsEditMode` guard applied by the existing registration commands.
7. The command must be refused for a visit that is no longer the patient's first registration, consistent with `ClearAllTestsCommandHandler`'s first-registration rule and its 24-hour check, because a bulk ordering command changes the visit's test list.
8. The ordering must not alter the visit's demographics and must not affect the ledger.

#### Affected Layers, Files, and Symbols

**Domain — expected to require verification, and possibly minor change.** `src/TopLab.Domain/Tests/TestGroup.cs` — `TestGroup` currently holds only `Name` and `IsActive`; a member-enumeration helper is the natural place for the expansion rule, and its expected involvement is justified by the absence of any member list on the entity. `src/TopLab.Domain/Tests/Test.cs` — `Test.TestGroupId` is the existing relationship that defines membership.

**Application — expected to change.** `src/TopLab.Application/Features/PatientRegistration/Commands/` — a new command, handler and validator, following the `AddCustomGroupToVisit` folder convention and returning the created identifiers as `Result<IReadOnlyList<int>>`. `src/TopLab.Application/Features/PatientRegistration/Common/TestPriceResolver.cs` — expected to be reused unchanged. `src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Queries/GetTestGroups/GetTestGroupsQueryHandler.cs` — the existing source of the group list, expected to be reused; the handler requires verification to confirm that per-group member identifiers and counts are obtainable from the existing `Test` projection, as `TestGroupsViewModel` already does.

**Infrastructure — not expected to change.** `Tests.TestGroupId` is a mapped foreign key with an index (`TestConfiguration.cs`), created in `20260828052248_BaselineDataModel.cs`.

**Presentation — expected to change.** `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` — the new group control, alongside the existing custom-group control at lines 493-499. `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — the new command, and the `TestGroups` collection at line 379 becomes bound rather than merely populated, with the `IsEditMode` guard.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/PatientRegistration/Commands/AddCustomGroupToVisitCommandHandlerTests.cs` is the direct precedent for the handler; `tests/TopLab.Presentation.Tests/Patients/PatientEditorCatalogModeTests.cs` is the precedent for the view-model wiring.

#### Dependencies and Execution Prerequisites

**U1-GAP-010 is a design convenience, not a prerequisite.** If the catalogue browser is implemented as a group tree, the group-ordering action belongs inside it; the two items remain independently implementable.

Existing components this item depends on: `TestPriceResolver`, `AddCustomGroupToVisitCommand` and its handler as the expansion pattern, `GetTestGroupsQuery`, `Test.TestGroupId`, `PatientTest.Create`, `PatientTestConfiguration`, and the first-registration rule in `ClearAllTestsCommandHandler`.

#### Owner Decision

No decision required

#### Migration Requirement

**No new migration is required.** The master-group membership already exists as `Tests.TestGroupId`, a nullable foreign key created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` and mapped in `src/TopLab.Infrastructure/Persistence/Configurations/TestConfiguration.cs` with `b.HasIndex(e => e.TestGroupId);` and `OnDelete(DeleteBehavior.SetNull)`. The `TestGroups` table was created in the same baseline migration. The change adds an application command and a presentation control over existing data.

#### Acceptance Conditions

1. The registration screen offers a control that lists the master test groups and, on selection, orders every active test of that group onto the visit.
2. The price applied to each ordered test equals the price that the same test receives when ordered individually, under the same price list, account type and custom-group circumstances.
3. A group containing no tests produces the existing Arabic conflict message and orders nothing.
4. A group whose tests are all inactive orders nothing and reports that fact rather than failing silently.
5. A test already ordered on the visit is not added a second time, and the behaviour is consistent with the existing custom-group path.
6. The control is disabled when no visit is loaded, and the command is refused for a visit that is no longer the patient's first registration, consistently with `ClearAllTestsCommandHandler`.
7. A soft-deleted visit is refused.
8. The existing custom-group control continues to behave exactly as it does today.

---

### U1-GAP-012 — Undelivered-results count indicator in the application shell

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The count data exists and is already computed. `src/TopLab.Application/Features/ResultDelivery/Queries/GetUndeliveredResults/GetUndeliveredResultsQuery.cs` declares `record GetUndeliveredResultsQuery(DateOnly? From = null, DateOnly? To = null, int Page = 1, int PageSize = 50)`, and its handler builds a per-patient count of undelivered tests at lines 44-51, returning it in `UndeliveredPatientRowDto` (`src/TopLab.Application/Features/ResultDelivery/Common/ResultDeliveryDtos.cs:13-22`, field `UndeliveredCount`). The count is presented on the delivery screen: `src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml` binds a `غير المسلَّم` column to `UndeliveredCount`, and `src/TopLab.Presentation/ViewModels/Patients/GroupPatientSlipsViewModel.cs` carries the same count. The patient search view can additionally restrict itself to patients with undelivered results through the `NotDelivered` filter, implemented at `SearchPatientsGlobalQueryHandler.cs:56-59`.

**Remaining gap.** The count is not surfaced in the application shell. `src/TopLab.Presentation/MainWindow.xaml:37-50` renders the navigation ribbon, and the status bar at lines 57-93 carries exactly four items: the current user name (a button that opens the change-password dialog), `LastLoginText`, the database-connectivity indicator with its tooltip, and the current date and time. `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` exposes `CurrentUserName`, `LastLoginText`, `IsDatabaseConnected`, `DatabaseConnectivityText`, `DatabaseConnectivityTooltip`, `IsConnectionStatusKnown` and `CurrentDateTime`, and its `LoadStatusAsync` (lines 319-378) refreshes the user and database status only. A search of the shell view model for a count, badge or pending member returns nothing. A count-display precedent does exist in the presentation layer — `src/TopLab.Presentation/Views/WorkSheets/WorkSheetsView.xaml:216` binds a `معلّق:` count from the work-sheet summary rows — but that is a per-row count inside a module screen, not a shell indicator.

#### Precise Required End State

1. The application shell must display a count of results that are complete and awaiting delivery, so the operator can see the outstanding work without first opening the delivery screen.
2. The count must be the number of undelivered ordered tests, matching the semantics already used by `GetUndeliveredResultsQueryHandler` and the `UndeliveredCount` field of `UndeliveredPatientRowDto`, and not a count of visits.
3. The indicator must be recomputed at a point at which it cannot become stale, and the refresh must not block the shell's responsiveness. The shell already has an established refresh point in `LoadStatusAsync`.
4. The indicator must respect the same authorisation boundary as the underlying query: `GetUndeliveredResultsQuery` implements `IAuthorizedRequest` and requires `ResultDeliveryAccessPolicy.DeliverResults`, so the count must not be requested by a user not entitled to it, and the indicator must be hidden rather than showing a zero for an unauthorised user.
5. The indicator must be presented in the shell's existing visual language, alongside the existing status-bar items, and must not change the navigation ribbon.
6. The indicator must be accurate with respect to the period it covers, and the period basis must be explicit in the specification rather than implicit.
7. Clicking the indicator may navigate to the delivery screen pre-filtered to the same period; that is a permitted convenience, not a required behaviour.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.**

**Application — expected to change.** `src/TopLab.Application/Features/ResultDelivery/Queries/GetUndeliveredResults/` — the existing query is paged and period-scoped and returns a list of rows, so a scalar total count is a different shape; a dedicated count query in the same feature folder is the expected route, mirroring the existing per-feature query layout. `src/TopLab.Application/Features/ResultDelivery/Common/ResultDeliveryAccessPolicy.cs` — the permission code the new query must require.

**Infrastructure — not expected to change.** `PatientTests.IsDelivered` already exists and is indexed.

**Presentation — expected to change.** `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` — a count property and its refresh within `LoadStatusAsync`. `src/TopLab.Presentation/MainWindow.xaml` — the indicator element in the status bar.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/ResultDelivery/GetUndeliveredResultsQueryHandlerTests.cs` is the direct precedent for the query; `tests/TopLab.Presentation.Tests/ShellViewModelUsersNavigationGateTests.cs` is the precedent for shell view-model behaviour.

#### Dependencies and Execution Prerequisites

No prerequisite identified. The item is independent of the patient-search filters, although both consume result-delivery state.

Existing components this item depends on: `GetUndeliveredResultsQuery` and its handler, `UndeliveredPatientRowDto`, `ResultDeliveryAccessPolicy`, `ShellViewModel.LoadStatusAsync`, and the `MainWindow` status bar.

#### Owner Decision

No decision required

#### Migration Requirement

**No new migration is required.** The count is derived from `PatientTests.IsDelivered`, a `bit` column created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` and covered by the index `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered` created in the same migration. The change is a read-side aggregate and a presentation element.

#### Acceptance Conditions

1. The shell displays a count of undelivered ordered tests.
2. The count is the number of undelivered ordered tests, matching the semantics of `UndeliveredPatientRowDto.UndeliveredCount`, and not a count of visits.
3. The count is refreshed at the shell's existing status-refresh point and is not stale after a delivery is recorded.
4. A user without the permission required by `GetUndeliveredResultsQuery` neither sees the indicator nor causes a count to be requested.
5. The indicator does not change the navigation ribbon and does not displace the existing status-bar items.
6. The period the indicator covers is stated in the user-facing text and matches the period the count is computed over.
7. Regression: the delivery screen and the group-slip screen continue to show the same per-patient counts as today.

---

### U1-GAP-013 — Registration-statistics quick action on the patient editor

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** The statistics required already exist and are already computed for the current day by default. `src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/GetPatientCountStatisticsQuery.cs` declares `record GetPatientCountStatisticsQuery(DateOnly? From, DateOnly? To, bool BySex, bool ByReferralEntity, bool ByAccountType, bool GroupByMonth, bool GroupByDayOfMonth, bool IncludeMoneyRow)`, and the handler at `src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/GetPatientCountStatisticsQueryHandler.cs:30-33` defaults the period to the current day when none is supplied — `var from = request.From ?? today; var to = request.To ?? today;` — so a query issued with no period already returns the day's registration counters. The handler returns the total patient count (lines 38-44), a day-of-month grouping when `GroupByDayOfMonth` is set (lines 103-115), and a money row when `IncludeMoneyRow` is set (lines 120-130), computed as `PatientAccountCalculator.TotalPaid(periodOperations)` over the operations of the period. The total test orders are produced by the separate `src/TopLab.Application/Features/Statistics/Queries/GetTestCountStatistics/`. The statistics screen presents all of it: `src/TopLab.Presentation/Views/Statistics/StatisticsView.xaml:66-77` provides the grouping and money-row controls, and `src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs:216-223` formats the money line. A census report can also be exported through `SaveCensusReportCommand` and `IStatisticsReportPdfWriter`.

**Remaining gap.** There is no entry point from the registration screen. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` contains no control that reaches the statistics, and the statistics screen is reachable only from the shell navigation, where `ShellViewModel.BuildNavigationItems` (lines 188-308) registers an `الإحصائيات` item that navigates to `StatisticsViewModel` and requires the `STATISTICS` permission. The registrar registering a patient therefore has no way to see the day's counters from the screen on which they are registering.

#### Precise Required End State

1. The patient editor must offer an action presenting today's registration counters, comprising: the total number of patients registered today, the total number of tests ordered today, and the amount received today.
2. The figures must come from the existing statistics queries rather than from a second, divergent computation: the patient count and the amount received must be the values `GetPatientCountStatisticsQuery` produces for the current day, and the test-order count must be the value `GetTestCountStatisticsQuery` produces for the current day.
3. The "today" basis must be the same day boundary the statistics handler already uses — `DateOnly.FromDateTime(_dateTime.UtcNow)` — so the editor's figures and the statistics screen's figures agree for the same day.
4. The action must be available on the registration screen and must not require the visit to be saved; the counters are about the day, not about the visit being edited.
5. The action must respect the authorisation boundary of the underlying queries: `GetPatientCountStatisticsQuery` and `GetTestCountStatisticsQuery` both implement `IAuthorizedRequest` and require `StatisticsAccessPolicy.Statistics`. A user without that permission must not be able to obtain the counters, and the action must be hidden or disabled for such a user rather than failing at the moment of use.
6. Presenting the counters must not change any visit, test or ledger data.
7. The action must not interfere with the two-press cashiering confirmation, which is a sensitive interaction on the same screen.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.**

**Application — expected to change only if a dedicated summary query is required.** `src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/` and `.../GetTestCountStatistics/` — the existing queries, their handlers and their DTOs in `src/TopLab.Application/Features/Statistics/Common/StatisticsDtos.cs`, the expected source of the figures. `src/TopLab.Application/Features/Statistics/Common/StatisticsAccessPolicy.cs` — the permission code.

**Infrastructure — not expected to change.**

**Presentation — expected to change.** `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` — the new action control. `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — the command, its state and its presentation. `src/TopLab.Presentation/Views/Statistics/StatisticsView.xaml` and `src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs` — expected to require verification if the navigation option is chosen, since the screen's `From` and `To` properties would need to accept a pre-set period.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/Statistics/GetPatientCountStatisticsQueryHandlerTests.cs` and `.../GetTestCountStatisticsQueryHandlerTests.cs` are the direct precedents; `tests/TopLab.Presentation.Tests/Statistics/StatisticsViewModelMonitorTests.cs` is the presentation precedent.

#### Dependencies and Execution Prerequisites

No prerequisite identified.

Existing components this item depends on: `GetPatientCountStatisticsQuery` and its handler, `GetTestCountStatisticsQuery`, `StatisticsAccessPolicy`, `StatisticsViewModel`, and the `PatientEditorViewModel` command conventions.

#### Owner Decision

**Established.** The counters exist, default to the current day, and are already rendered by a dedicated screen. What is missing is the way in.

**Undecided.** The form of the entry point: an in-place summary on the registration screen, or a navigation to the existing statistics screen with the current day pre-selected — and, if in place, whether the summary is a compact panel or a separate window.

**Option 1 — A dedicated summary query and an in-place summary on the registration screen.**
* *Meaning:* a small query returns today's three figures in one round trip, and the editor shows them in a compact panel or small window.
* *Functional consequences:* the registrar sees the counters without leaving the registration screen. The figures come from a purpose-built query rather than from composing two existing queries.
* *Operational impact:* one additional control on a screen that already carries thirteen controls in its action bar and a large demographics form. Because the registrar may record a payment afterwards, the summary must be refreshable.
* *Architectural impact:* a new query, handler and DTO in the Statistics feature; a new panel or window in the Presentation layer. The existing `StatisticsView` is untouched.
* *Persistence impact:* none.
* *Risks and trade-offs:* the lowest navigation cost for the operator, and the figures are guaranteed internally consistent because they come from one query. The cost is a new query surface and additional screen furniture on an already dense form.
* *Rule conflicts:* none.

**Option 2 — Reuse the existing statistics queries and navigate to the statistics screen with the current day pre-selected.**
* *Meaning:* the editor's action sets the statistics screen's period to today and selects the patients section.
* *Functional consequences:* the operator reaches the full statistics screen rather than a summary. The figures come from the queries that already exist, so there is no new query and no risk of divergence.
* *Operational impact:* the registrar leaves the registration screen, navigates through the shell to the statistics module, and returns. For a registrar working through a queue of patients this is a disruptive round trip.
* *Architectural impact:* the editor's command sets `From` and `To` on `StatisticsViewModel` and navigates there. The statistics screen's own layout is unchanged; its period controls already exist.
* *Persistence impact:* none.
* *Risks and trade-offs:* the smallest change and the least duplication, but it does not deliver a quick action in any meaningful sense — it delivers a shortcut to a general-purpose screen whose layout, five sections and export tooling were built for a different purpose.
* *Rule conflicts:* none.

**Option 3 — Retain the current behaviour: the counters remain reachable only from the statistics module.**
* *Meaning:* no action is added to the registration screen.
* *Functional consequences:* unchanged.
* *Operational impact:* the registrar must navigate to the statistics module to see the day's counters.
* *Architectural impact:* none. *Persistence impact:* none.
* *Risks and trade-offs:* zero cost; the operational friction remains.
* *Rule conflicts:* none.

**Recommended option: Option 1.**

**Reasoning.** The requirement is explicitly a quick action on the registration screen, and Option 2 satisfies the letter of that by navigating to a general-purpose screen built for a different purpose; it does not deliver the counters in context. Option 1 delivers the three figures where the registrar is working and reuses the existing statistics queries and their day boundary, so the two routes cannot disagree. The additional screen furniture is the cost, and it is a bounded one on a single view.

**Owner decision required:** Should the registration-statistics quick action present the counters in place on the registration screen, or navigate to the existing statistics screen with the current day pre-selected?

*The recommendation is advisory only; the choice is the owner's.*

#### Migration Requirement

**No new migration is required.** The counters are read-side aggregates over the `Patients`, `PatientTests` and `PaymentOperations` tables, all created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs`, with the money row computed by `PatientAccountCalculator.TotalPaid` over persisted `PaymentOperation` rows. No persisted value is introduced or changed.

#### Acceptance Conditions

1. The registration screen offers an action that presents today's total registered patients, today's total ordered tests and the amount received today.
2. The figures equal those produced by `GetPatientCountStatisticsQuery` and `GetTestCountStatisticsQuery` for the same day.
3. The day boundary is the one the statistics handler already uses, so the editor and the statistics screen agree.
4. The action is available whether or not the visit being edited has been saved.
5. A user without the `STATISTICS` permission neither sees the action nor obtains the figures.
6. Presenting the counters creates no visit, test or ledger change.
7. The action does not interfere with the two-press cashiering confirmation on the same screen.
8. Under Option 2, the statistics screen opens with the period set to the current day and the patients section selected, and the screen's own controls continue to work.

---

### U1-GAP-014 — Explicit main-menu navigation action on the patient editor

#### Verified Status

**PARTIALLY EXISTS**

#### Current Implementation and Evidence

**Implemented portion.** Leaving the registration screen for another module is already possible without closing anything. `src/TopLab.Presentation/MainWindow.xaml:37-50` renders a navigation ribbon above the content area on every screen, driven by `NavigationItems`; `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs:188-308` builds twelve items — `المرضى`, `المعمل`, `ورقة العمل`, `الأدوات`, `الحسابات`, `الإحصائيات`, `المستخدمون`, `النظام`, `الإعدادات`, `حول البرنامج`, `قفل المحطة` and `خروج` — each of which navigates to a module or opens a dialog. The ribbon is a sibling of the editor within the same `DockPanel`, so it stays visible while the patient editor is displayed. The editor additionally offers `PatientWindowCommand` (`PatientEditorView.xaml:69`) to open the patients list, `LabResultsCommand` (line 70) and `FollowUpCommand` (line 71).

**Remaining gap.** The editor's own action bar carries no explicit main-menu or home control. `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml:58-71` enumerates its thirteen controls: `جديد`, `حفظ`, `حذف`, `تراجع`, `باركود`, `ورقة العمل`, `الإيصال`, `الفاتورة`, `طلب تحاليل`, `مغلف`, `نافذة المرضى`, `نتائج التحاليل` and `متابعة`. A search of that view for `الرئيسية`, `Home`, `MainMenu` and `لوحة التحكم` returns nothing. Navigation away from the editor therefore depends on the operator reaching the shell ribbon, which is plausible but is not a labelled, discoverable action within the form the operator is working in.

#### Precise Required End State

1. The patient editor must provide an explicit, labelled action that returns the operator to the application's main navigation point without closing the window and without discarding an unsaved visit.
2. The action must not be conflated with the existing `PatientWindowCommand`, which opens the patients list rather than the main menu. The two must be distinguishable by label and by destination.
3. The action must not interfere with the `IsEditMode` guards on the other controls in the same bar, and must remain available regardless of whether a visit is loaded or saved.
4. The action must not change any visit, test or ledger data, and must not trigger the two-press cashiering confirmation.
5. The action must use the existing navigation abstraction rather than opening a window: `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` already navigates through the injected navigation service in `PatientWindowAsync` and `LabResultsAsync`, and the shell's home view model is `TopLab.Presentation.ViewModels.Shell.HomeViewModel`, which `MainWindow.xaml` renders as the content-control template for the home view.
6. The action must be present exactly once in the editor and must not be duplicated in the shell ribbon.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.**
**Application — not expected to change.**
**Infrastructure — not expected to change.**

**Presentation — expected to change.** `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` — a new command following the `PatientWindowCommand` pattern (constructed at line 197). `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` — the new labelled control in the action bar.

**Tests — expected to change.** `tests/TopLab.Presentation.Tests/Patients/PatientEditorStageOneViewTests.cs` and the navigation tests under `tests/TopLab.Presentation.Tests/Navigation/` are the relevant precedents.

#### Dependencies and Execution Prerequisites

No prerequisite identified.

Existing components this item depends on: the injected navigation service used by `PatientEditorViewModel`, `TopLab.Presentation.ViewModels.Shell.HomeViewModel`, and the shell's `NavigationItems` ribbon.

#### Owner Decision

**Established.** The operator can already reach every module from the editor through the shell ribbon, which is rendered on the same screen. The editor additionally has a dedicated route to the patients list. The only absent element is a labelled main-menu control within the editor's own action bar.

**Undecided.** Whether the editor should carry a redundant main-menu control given that the shell ribbon already provides that navigation, and if so where it should sit and what it should be labelled.

**Option 1 — Add a dedicated main-menu control to the editor's action bar.**
* *Meaning:* a new labelled control navigates to the application's home view through the existing navigation service.
* *Functional consequences:* the editor exposes an explicit route to the main menu, distinct from `PatientWindowCommand`.
* *Operational impact:* a fourteenth control on an action bar that already carries thirteen, and a second route to a destination the shell ribbon already offers. The operator gains discoverability at the cost of some redundancy.
* *Architectural impact:* Presentation only — one command in `PatientEditorViewModel` and one control in `PatientEditorView.xaml`.
* *Persistence impact:* none.
* *Risks and trade-offs:* the change is trivial and carries no risk, but it adds redundancy that a reviewer could reasonably call avoidable.
* *Rule conflicts:* none.

**Option 2 — Do not add a control; make the existing shell ribbon the single navigation affordance and close the item.**
* *Meaning:* the editor keeps its thirteen controls; the ribbon remains the way to reach the main menu.
* *Functional consequences:* unchanged — the navigation capability is already present and always visible.
* *Operational impact:* the registrar continues to reach the main menu via the ribbon at the top of the window. The cost is that the action is not discoverable from within the form.
* *Architectural impact:* none. *Persistence impact:* none.
* *Risks and trade-offs:* avoids duplicating an existing, always-visible affordance. The residual risk is discoverability, which is bounded because the ribbon is never hidden on this screen.
* *Rule conflicts:* none.

**Option 3 — Repurpose the existing `PatientWindowCommand` control to reach the main menu, and leave the patients list to the shell's `المرضى` ribbon item.**
* *Meaning:* the `نافذة المرضى` control becomes the main-menu control.
* *Functional consequences:* the editor loses its direct route to the patients list, which the shell ribbon's `المرضى` item already provides.
* *Operational impact:* the operator reaches the patients list through the ribbon rather than from the editor's action bar. A frequently used registration-to-lookup step becomes a two-step path.
* *Architectural impact:* `PatientEditorViewModel.PatientWindowCommand` changes destination, and its existing behaviour is removed.
* *Persistence impact:* none.
* *Risks and trade-offs:* keeps the control count unchanged, at the cost of removing a working, frequently used navigation from a fixed position.
* *Rule conflicts:* none technically; the ribbon already provides the displaced destination.

**Recommended option: Option 2.**

**Reasoning.** The capability the requirement describes — returning to the application's main menu without closing the window — is already delivered by the shell ribbon, which `MainWindow.xaml:37-50` renders on every screen including the editor, and which `ShellViewModel.BuildNavigationItems` populates with the module destinations. Adding a fourteenth control to an action bar that already carries thirteen, to duplicate a ribbon that is never hidden, buys discoverability at the cost of redundancy, and the editor already has two navigation controls of its own. Option 3 is rejected because it removes a working, frequently used route to replace it with one the ribbon already provides.

**Owner decision required:** Should the patient editor carry its own explicit main-menu control, given that the shell ribbon is permanently visible above it, or should the existing ribbon be accepted as the single navigation affordance?

*The recommendation is advisory only; the choice is the owner's.*

#### Migration Requirement

**No new migration is required.** The change is confined to a view-model command and a control in a view. No persisted state is introduced or altered.

#### Acceptance Conditions

1. The action returns the operator to the application's main navigation point without closing the window and without discarding an unsaved visit.
2. The action is distinguishable from `PatientWindowCommand` by label and by destination.
3. The action is available whether or not a visit is loaded or saved.
4. The action changes no visit, test or ledger data, and does not trigger the two-press cashiering confirmation.
5. The action is present exactly once in the editor and is not duplicated in the shell ribbon.
6. Under Option 2, the shell ribbon remains the single navigation affordance and stays visible while the patient editor is displayed.

---

### U1-GAP-015 — Patient identification slip printing

#### Verified Status

**OWNER DECISION REQUIRED**

#### Current Implementation and Evidence

**What exists today.** The patient's card or file number is a plain, optional string on the visit. `src/TopLab.Domain/Patients/Patient.cs` declares `public string? CardNumber { get; private set; }`, with `public const int MaxCardNumberLength = 30;` and a `NormalizeCardNumber` helper that maps a blank entry to null. It is mapped in `src/TopLab.Infrastructure/Persistence/Configurations/PatientConfiguration.cs` as `b.Property(e => e.CardNumber).HasMaxLength(Patient.MaxCardNumberLength).IsRequired(false);` with `b.HasIndex(e => e.CardNumber);`. It is edited at `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml:234` and searched by at `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml:13`, through the exact card-number predicate at `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQueryHandler.cs:48-52`. The column and its index were added by `src/TopLab.Infrastructure/Persistence/Migrations/20261020211907_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue.cs`, which added `CardNumber` as `nvarchar(30)` and created `IX_Patients_CardNumber`.

There is **no** plastic-card printing of any kind: no card layout template, no card-size setting, no card printer path. `src/TopLab.Infrastructure/Printing/` contains writers for receipts, invoices, worksheets, lab orders, envelopes, price lists, antibiotic lists, custom-group lists, combined and blank reports, banded-result monitors, statistics reports and group patient slips — none of which is a card.

**What is adjacent but different.** A **paper per-patient identification slip already exists**, and it is not a plastic card. `src/TopLab.Application/Features/ResultDelivery/Commands/PrintGroupPatientSlips/PrintGroupPatientSlipsCommand.cs` declares `record PrintGroupPatientSlipsCommand(IReadOnlyList<int> PatientIds)`, and `src/TopLab.Infrastructure/Printing/GroupPatientSlipPdfWriter.cs` renders it: the writer's title constant is `Title = "قصاصة تقرير مريض"`, it produces one page per patient in a single PDF, it reuses `BarcodeLabelRenderer` and `BarcodePngEncoder` for the identifier, and its own header records that it follows the lab-order slip pattern at A5 portrait with RTL layout. The source of the patients is `GetGroupPatientSlipsQuery`, whose comment describes it as serving "the follow-up/delivery desk", and the entry point is the `قصاصة تقرير جماعية` control on `src/TopLab.Presentation/Views/Patients/ResultDeliveryView.xaml:24`. A search of the registration view and its view model for "slip" returns no match, so the slip is not reachable from the registration screen.

**A single-patient slip is already producible from the existing code.** `GetGroupPatientSlipsQueryHandler` selects patients by **explicit identifier only** — `_db.Set<Patient>().Where(p => distinctIds.Contains(p.Id.Value) && !p.IsDeleted)` — and imposes no delivery-state filter; the undelivered list is only the user interface that *supplies* the identifiers. `PrintGroupPatientSlipsCommandHandler` resolves the batch through that query and hands it to `IGroupPatientSlipPrintingService`, and `GroupPatientSlipPdfWriter` renders one A5 page per slip. A one-element identifier list therefore already produces a one-page slip for any non-deleted patient, without a new writer, a new command or a new query. Two constraints follow. First, the command is gated on `ResultDeliveryAccessPolicy.DeliverResults` — the delivery desk's permission — not on the registration desk's `ADD_EDIT_PATIENT`, so a registration-desk operator may not be able to invoke it. Second, the slip content does not carry the card number: `GroupPatientSlipDto` is `record GroupPatientSlipDto(int PatientId, string FullName, string? LabId, string Identifier, DateTime RegistrationDateUtc, IReadOnlyList<GroupPatientSlipLineDto> Lines)` (`src/TopLab.Application/Features/ResultDelivery/Common/GroupPatientSlipDtos.cs`), and the identifier is `BarcodePayload.For(patientId, labId, printLabIdInsteadOfPatientId)` — the laboratory identifier or the patient identifier per `SystemSettings.PrintLabIdInsteadOfPatientId` (`src/TopLab.Application/Features/PatientEnvelope/Common/BarcodePayload.cs`) — never the card number. The slip is in any case a *report* slip, carrying the ordered test lines, not an identification card.

The specimen tube label is the other identity artefact: `src/TopLab.Application/Features/PatientRegistration/Commands/PrintBarcode/PrintBarcodeCommand.cs` is dispatched from the `باركود` control at `PatientEditorView.xaml:63`, and `src/TopLab.Infrastructure/Barcode/BarcodeService.cs` builds a Code-128 label whose page is fixed at 4 in × 2 in at 72 pt/in, which is 288 × 144 points, or 101.6 × 50.8 mm, with a 300 × 80 pixel bitmap produced by `src/TopLab.Infrastructure/Barcode/BarcodeLabelRenderer.cs`. The label size is not configurable in the code.

**What is genuinely undecided.** Whether patient identification printing is in scope at all, and if it is, which artefact is required: an identification slip on paper, an identification card on plastic, or both. The repository establishes what is possible — an existing slip writer, an existing barcode renderer and a fixed label geometry — but not which of these the operator needs, nor whether the identity slip must be reachable from the registration screen.

#### Precise Required End State

**Established, and true under every option below.**
1. `Patient.CardNumber` remains a plain, optional, indexed string of at most 30 characters, with the existing normalisation and the existing exact-match search. It is not converted into a structured card record.
2. Plastic CR80 card printing is not part of the requirement as stated. It becomes in scope only if the owner selects Option 3 below, and the consequences of that selection are set out in that option.
3. The existing tube-label geometry of 4 in × 2 in and the existing slip writer are not to be modified by this item.
4. The specimen tube label must remain available from the registration screen on the same terms as today.
5. No persisted state may be introduced that changes any visit, the ledger or the account figures.

**Option-dependent requirements** are stated in the owner decision below. In every option in which a paper artefact is produced, the artefact must contain at least the patient's full name, the laboratory identifier and the card number where one is present, and it must be produced by the existing printing abstractions rather than by a new printing stack.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.** `TopLab.Domain.Patients.Patient` already carries `CardNumber`.

**Application — expected to change only if a single-patient slip is added.** `src/TopLab.Application/Features/ResultDelivery/Commands/PrintGroupPatientSlips/` — the existing command and handler are the closest existing implementation and are reusable unchanged in the base form; the DTOs in `src/TopLab.Application/Features/ResultDelivery/Common/GroupPatientSlipDtos.cs` are the expected point of change if the card number must be carried. `src/TopLab.Application/Features/PatientRegistration/Commands/PrintBarcode/` — the registration screen's existing identity-printing command, the natural home for a registration-screen identity print if a registration-side command is introduced.

**Infrastructure — expected to change only if a card-template store is introduced, or if the card number must appear on the slip.** `src/TopLab.Infrastructure/Printing/GroupPatientSlipPdfWriter.cs` — the existing writer, expected to be reused unchanged in the base form and generalised rather than replaced if the artefact's content changes. `src/TopLab.Infrastructure/Barcode/BarcodeLabelRenderer.cs` and `BarcodeService.cs` — the existing identifier pipeline. `src/TopLab.Infrastructure/Persistence/Configurations/` — a new configuration would be required only if a persisted card or slip template is introduced.

**Presentation — expected to change only if a single-patient slip is added.** `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` — the entry control, if the artefact must be reachable from the registration screen. `src/TopLab.Presentation/Views/Patients/GroupPatientSlipsView.xaml` — the existing multi-patient slip screen, the reference for the artefact's content and layout.

**Tests — expected to change.** `tests/TopLab.Application.Tests/Features/ResultDelivery/PrintGroupPatientSlipsCommandHandlerTests.cs` and `.../GetGroupPatientSlipsQueryHandlerTests.cs` are the direct precedents.

#### Dependencies and Execution Prerequisites

No prerequisite identified. The item is independent of every other item in this specification.

Existing components this item depends on: `GroupPatientSlipPdfWriter` and `IGroupPatientSlipPdfWriter`, `PrintGroupPatientSlipsCommand`, `BarcodeLabelRenderer`, `BarcodeService`, `PrintBarcodeCommand`, and the `CardNumber` field and its index.

#### Owner Decision

**Established.** The repository contains a working paper per-patient slip that already carries the patient's identity block and a barcode, but it is a *report* slip produced in bulk from the delivery desk for undelivered patients, and it is not reachable from the registration screen. The repository contains no plastic-card printing, and the specimen tube label is a separate, already-shipped artefact with a fixed 4 in × 2 in geometry. `CardNumber` is a plain indexed string.

**Undecided.** Whether patient identification printing belongs in the registration workflow at all, and if so, whether the required artefact is a paper slip, a plastic card, or both — and whether a single-patient print must be available from the registration screen.

**Option 1 — Patient identification printing stays out of scope; no new artefact.**
* *Meaning:* the laboratory continues to identify patients by the specimen tube label, the receipts and the group report slip. `CardNumber` remains a reference string for filing and for exact search.
* *Functional consequences:* unchanged. A patient cannot be issued an identification slip from the registration screen.
* *Operational impact:* a patient who needs an identifying document must be issued the tube label or be included in a group slip at the delivery desk. There is no plastic card, which is consistent with what the application does today.
* *Architectural impact:* none. *Persistence impact:* none.
* *Risks and trade-offs:* zero implementation cost and no risk of introducing a partially specified artefact. The cost is that the operational need, if it exists, remains unmet and will be raised again.
* *Rule conflicts:* none.

**Option 2 — A paper patient identification slip is in scope; plastic cards remain out of scope.**
* *Meaning:* a single-patient identification slip is printable, reusing the existing QuestPDF and ZXing pipeline. The artefact carries the patient's name, laboratory identifier and card number, plus a Code-128 identifier. Plastic card printing is explicitly excluded.
* *Functional consequences:* the registration screen gains an identity-print action, and the system can issue an identifying document for one patient on demand rather than only in a delivery-desk batch.
* *Operational impact:* the registrar can issue an identifying document at the point of registration, where the patient's details are most complete. The cost is a further control on the registration screen's already dense action bar.
* *Architectural impact:* in its base form **no new writer, command or query is required** — the registration screen's control can dispatch the existing `PrintGroupPatientSlipsCommand` with a one-element identifier list, and the existing `GroupPatientSlipPdfWriter` and `IGroupPatientSlipPrintingService` produce the one-page A5 slip unchanged. Two decisions then determine the size of the change: the permission under which the control is offered, since the existing command requires the delivery desk's `DELIVER_RESULTS` and a registration-desk operator may not hold it — either the control is gated on the same code, or a registration-side command with `ADD_EDIT_PATIENT` is introduced; and whether the card number must appear on the artefact, which is not part of `GroupPatientSlipDto` and would require extending that DTO, the query handler and the writer's patient block. A separate writer is justified only if the identification slip must differ materially from the report slip.
* *Persistence impact:* none for the slip itself. If the owner additionally wants the slip or label size configurable per laboratory, a new `SystemSettings` or `ReceiptSettings` column would be required — note that the existing tube-label geometry is hard-coded in `BarcodeService.BuildLabelPdf` and is not currently configurable.
* *Risks and trade-offs:* reuses shipped code and satisfies the paper-sticker reading of the requirement. The principal open detail is the size of the artefact, which the code currently fixes at 4 in × 2 in for the tube label and A5 for the report slip; a slip intended for a different stock would need a different geometry, and that geometry is not configurable today.
* *Rule conflicts:* none.

**Option 3 — Both a paper slip and plastic CR80 card printing are in scope.**
* *Meaning:* in addition to Option 2, a card-sized plastic identity is produced.
* *Functional consequences:* the system issues a physical card as the patient's identity document.
* *Operational impact:* the laboratory must acquire and maintain plastic card stock and a compatible printer, and the operator gains a durable identity artefact.
* *Architectural impact:* card-layout templates, a card-size setting and a print path for card stock — none of which exists in `src/TopLab.Infrastructure/Printing/` today. A persisted template store would be needed.
* *Persistence impact:* a new table for card templates and their geometry, plus the associated entity, configuration, migration and snapshot update.
* *Risks and trade-offs:* the largest build and the largest operational commitment, for a capability the application has never provided.
* *Rule conflicts:* none technically, but this option reverses the position the application has consistently held, which is that the specimen label, the receipt, the invoice, the worksheet, the lab order, the envelope and the report slip constitute the complete set of printed identity artefacts.

**Recommended option: Option 2.**

**Reasoning.** Option 1 is the status quo and, if the operational need is not real, the correct answer. Between the remaining options, Option 2 satisfies the paper-sticker reading of the requirement with a small, well-bounded build that reuses the existing slip writer, the existing barcode renderer and the existing printing abstraction, while leaving the plastic-card path out. Option 3 commits the laboratory to card stock, a card printer and a template store, and would be the first such capability in the application. The recommendation is conditional on the geometry question being settled before implementation, because the tube label's size is hard-coded and a slip intended for different stock would otherwise be printed at the wrong size.

**Owner decision required:** Is patient identification printing in scope for the registration workflow? If it is, is a paper identification slip required — with plastic CR80 card printing remaining out of scope — and must that slip be printable for a single patient from the registration screen, at what size?

*The recommendation is advisory only; the choice is the owner's.*

#### Migration Requirement

**Conditional — the migration requirement depends on the owner decision.**

* **Option 1 (out of scope) and Option 2 (paper slip):** no new migration is required. The `Patients.CardNumber` column and the index `IX_Patients_CardNumber` already exist, created by `src/TopLab.Infrastructure/Persistence/Migrations/20261020211907_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue.cs`. The slip is produced by the printing layer and records no new persisted state. A new migration would be required only if Option 2 were extended to make the slip or label geometry configurable, in which case a column would be added to the existing `SystemSettings` or `ReceiptSettings` single-row tables, both created in `20260828052248_BaselineDataModel.cs`.
* **Option 3 (plastic cards):** a new migration **would** be required, for a card-template table that does not exist in `src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs`.

#### Acceptance Conditions

*Unconditional — these hold under every option.*

1. `Patient.CardNumber` remains a plain, optional, indexed string of at most 30 characters, with the existing normalisation and the existing exact-match search.
2. The specimen tube label remains available from the registration screen on the same terms as today, at the existing 4 in × 2 in geometry.
3. The existing group report slip continues to be produced from the delivery desk, unchanged.
4. No plastic-card printing capability is introduced under Option 1 or Option 2.
5. No persisted state is introduced that changes any visit, the ledger or the account figures.

*Option-dependent — these apply only if the corresponding option is selected.*

6. Under Option 1, the registration screen is unchanged and no new identity artefact is produced.
7. Under Option 2, a single-patient identification slip can be produced from the registration screen through the existing slip pipeline, containing at least the patient's full name and the laboratory identifier together with a Code-128 identifier; where the card number is required, it appears on the artefact.
8. Under Option 2, the slip is produced through the existing printing abstractions and the existing identifier pipeline, and prints at the geometry agreed for it.
9. Under Option 2, the print is subject to an explicit permission, and a user without it cannot produce the slip.
10. Under Option 3, a card-layout template store exists and a plastic card can be produced at the configured geometry.

---

### U1-GAP-016 — Remembered sign-in identity

#### Verified Status

**CONFIRMED MISSING**

#### Current Implementation and Evidence

**What exists.** The sign-in surface is `src/TopLab.Presentation/Views/Setup/LoginWindow.xaml` with `src/TopLab.Presentation/ViewModels/Setup/LoginViewModel.cs` and the code-behind `src/TopLab.Presentation/Views/Setup/LoginWindow.xaml.cs`. The window already provides a reveal-password toggle: `LoginWindow.xaml:36-37` declares a `PlainPasswordBox` and a `ShowPasswordCheckBox`, and the code-behind's `ShowPasswordCheckBox_Changed` swaps their visibility, with `LoginButton_Click` reading the password from whichever control is active. The sign-in itself is `src/TopLab.Application/Features/UsersAndPermissions/Commands/SignIn/SignInCommand.cs` (`record SignInCommand(string UserName, string Password)`), handled by `SignInCommandHandler`, which verifies the password through `IPasswordHasher`, checks `user.IsActive`, and then calls `user.RecordLogin(_dateTime.UtcNow)`. The domain records the login *timestamp* on the user row: `src/TopLab.Domain/Users/User.cs:28` declares `public DateTime? LastLoginAtUtc { get; private set; }`, set by `User.RecordLogin` at line 121. `ShellViewModel.LastLoginText` displays that timestamp.

**What is absent.** No component of the application stores the last signed-in identity for reuse at the next launch. Evidence:
* A search of the entire repository — source, tests, project and configuration files — for `LastLoggedIn`, `RememberLogin`, `RememberedUser` and `LastUserName` returns no match of any kind.
* `LoginViewModel` (98 lines in full) declares exactly `UserName`, `Password`, `ErrorMessage`, `ConnectionStatusText` and `IsBusy`. It holds no persistence of any kind and has no remembered-identity member.
* `LoginWindow.xaml` has no "remember me" control; the only checkbox is the reveal-password toggle at line 37.
* `LoginWindow.xaml.cs` reads the user name from the `UserNameTextBox` on every sign-in and assigns it to the view model; it never reads a previously stored value and never writes one.
* `SignInCommandHandler` persists only `LastLoginAtUtc` on the user row. The *username* is never written outside the `User` table's own identity column, and nothing reads the most recent sign-in identity back to the sign-in form.
* `src/TopLab.Domain/Settings/SystemSettings.cs` is a single global row — its configuration `src/TopLab.Infrastructure/Persistence/Configurations/SystemSettingsConfiguration.cs` declares `b.HasKey(e => e.Id);` and seeds one row with `Id = 1` — and carries no member for a remembered user. `src/TopLab.Domain/Settings/ReceiptSettings.cs` and the other settings aggregates are likewise single-row and purpose-specific.
* A workstation-local persistence mechanism does exist and is established: `src/TopLab.Presentation/Services/Configuration/ConfigurationFileService.cs` reads and writes `%ProgramData%\TopLab\appsettings.json` (`GetProgramDataPath`, `TryLoadConnectionString`, `SaveConnectionString`), and `src/TopLab.Infrastructure/Services/JsonPhoneBookStore.cs` establishes the local-store pattern in the Infrastructure layer with an explicit locality rationale — *"Workstation-local JSON store for the Tools Phone Book (phone-book.json under %ProgramData%\TopLab — ADR-0021/ADR-0027 locality). Atomic write: temp file then move. Missing file → empty list."* — and a `directoryOverride` constructor parameter that provides a test seam.

**What is genuinely undecided.** Where the remembered identity should live. The two mechanisms available are not equivalent in scope: the local file is per workstation, while the `SystemSettings` row is shared by every client of the same SQL Server, and the application has no user-scoped preferences store.

#### Precise Required End State

1. The sign-in window must offer an explicit, labelled control that determines whether the signed-in identity is remembered.
2. When the control is enabled and a sign-in succeeds, the identity used must be stored, and the next launch of the sign-in window must present that identity pre-filled, so the operator can proceed without retyping it.
3. The remembered identity must be the user name only. The password must never be stored, in any form, by this feature. The application already hashes passwords through `IPasswordHasher` for storage in `User.PasswordHash`; the remembered identity must not weaken that.
4. The pre-filled identity must be editable, and the operator must be able to clear it. The window's existing `UserNameTextBox` is already a two-way bound editable field.
5. The behaviour must be consistent with the existing authorisation model: a remembered identity does not bypass authentication, and a sign-in attempt with a wrong password must still fail exactly as it does today, through `SignInCommandHandler`'s `InvalidCredentialsError`.
6. A remembered identity must not be displayed anywhere except the sign-in form.
7. The remembered identity must survive an application restart, and must not be written when the operator declines to have it remembered.
8. Where the identity is stored, it must be written in a form that does not disclose a credential, and — following the established local-store pattern — written atomically.

#### Affected Layers, Files, and Symbols

**Domain — not expected to change.** No domain concept of a remembered identity is required.

**Application — expected to require verification.** `src/TopLab.Application/Features/UsersAndPermissions/Commands/SignIn/SignInCommand.cs` and `.../SignInCommandHandler.cs` — expected to require verification during implementation, to confirm that the sign-in result carries enough information for the caller to persist the identity, or that the caller already holds it. `src/TopLab.Application/Features/SystemAndPrintSettings/` — the feature that owns the settings aggregates and their commands, and the expected home of a settings-backed implementation.

**Infrastructure — expected to change.** `src/TopLab.Infrastructure/Services/` — the local JSON store pattern, as established by `JsonPhoneBookStore.cs` and `JsonPurchasesListStore.cs`, is the expected home of a workstation-local implementation; a new store is justified if none of the existing stores fits the shape. `src/TopLab.Presentation/Services/Configuration/ConfigurationFileService.cs` — the existing `%ProgramData%\TopLab\appsettings.json` accessor, the expected route if the identity is stored in the same file. `src/TopLab.Infrastructure/Persistence/Configurations/SystemSettingsConfiguration.cs` and `src/TopLab.Domain/Settings/SystemSettings.cs` — expected to change only under the database-backed option.

**Presentation — expected to change.** `src/TopLab.Presentation/Views/Setup/LoginWindow.xaml` — the control, and the pre-filling of `UserNameTextBox`. `src/TopLab.Presentation/ViewModels/Setup/LoginViewModel.cs` — the property, the load and the save. `src/TopLab.Presentation/Views/Setup/LoginWindow.xaml.cs` — the code-behind interaction with the control, following the pattern already used for `ShowPasswordCheckBox`.

**Tests — expected to change.** New coverage is expected in `tests/TopLab.Presentation.Tests/` for the view model and the window interaction. `tests/TopLab.Application.Tests/Features/UsersAndPermissions/` holds the sign-in command's existing coverage.

#### Dependencies and Execution Prerequisites

No prerequisite identified. The item is independent of every other item in this specification.

Existing components this item depends on: `LoginWindow`, `LoginViewModel`, `SignInCommand` and `SignInCommandHandler`, `IPasswordHasher`, `ConfigurationFileService`, and the `JsonPhoneBookStore` local-store pattern.

#### Owner Decision

**Established.** Nothing in the application stores the sign-in identity, and every mechanism that could store it already exists: a workstation-local JSON file at `%ProgramData%\TopLab\appsettings.json` via `ConfigurationFileService`, and a single-row `SystemSettings` table in the database. The two are not equivalent in scope, and the application has no user-scoped preferences store.

**Undecided.** Which of those scopes the remembered identity should live in, and consequently whether the setting is per workstation or shared across every client of the same server.

**Option 1 — Store the remembered identity in the workstation-local configuration file.**
* *Meaning:* the identity is written to `%ProgramData%\TopLab\appsettings.json` and read back at the next launch of that workstation.
* *Functional consequences:* the identity is remembered on the machine where it was signed in, and on no other machine.
* *Operational impact:* on a shared counter with several workstations, each workstation remembers its own last user. This matches how a receptionist at a fixed station normally works, and it avoids one operator's identity appearing on a colleague's screen.
* *Architectural impact:* the existing `ConfigurationFileService` accessor is the natural route, or a dedicated store in `src/TopLab.Infrastructure/Services/` following `JsonPhoneBookStore` — including its atomic temp-file-then-move write and its `directoryOverride` test seam. The change is confined to the Presentation and Infrastructure file-access layers; no domain or application change is required beyond the view model.
* *Persistence impact:* none. The file already exists and already carries a structured JSON document with a `ConnectionStrings` section; a new key would be added to it. No database schema change.
* *Risks and trade-offs:* no schema cost and a natural per-workstation scope. The trade-off is that a workstation genuinely shared between operators will surface the previous operator's user name to whoever sits down next, and the file lives in a shared `ProgramData` location writable by all users of the machine.
* *Rule conflicts:* none.

**Option 2 — Store the remembered identity in the database, in the single-row `SystemSettings` table.**
* *Meaning:* the identity is a new column on the shared `SystemSettings` row and is read back at the next launch.
* *Functional consequences:* the identity is shared by every client of the same SQL Server, because the settings row is a single global row keyed by `Id = 1` and is not scoped to a user or a workstation.
* *Operational impact:* on a multi-workstation installation, the last identity signed in anywhere becomes the pre-filled identity everywhere. This is useful where one person signs in on several machines, and misleading where several people share a server.
* *Architectural impact:* a new property on `src/TopLab.Domain/Settings/SystemSettings.cs` with its mutator, a new column in `src/TopLab.Infrastructure/Persistence/Configurations/SystemSettingsConfiguration.cs`, a settings command in the `SystemAndPrintSettings` feature, and a read on the sign-in path. The aggregate's `SetGeneralFlags` and its sibling mutators show the established pattern for adding a setting.
* *Persistence impact:* a new column on the `SystemSettings` table, which was created in `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` and extended by `20261020211907_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue.cs` for its own settings. A new migration would be required.
* *Risks and trade-offs:* the identity follows the user across workstations, which is the behaviour many operators expect, at the cost of a schema change and of a shared-state behaviour that is not obviously correct for a laboratory counter.
* *Rule conflicts:* none, but the single-row scope of `SystemSettings` conflicts with the natural scope of a remembered sign-in identity, which is per person and per workstation. That tension is the substance of the decision.

**Option 3 — Retain the current behaviour: no identity is remembered and the user name is typed on every launch.**
* *Meaning:* the sign-in window behaves exactly as it does today.
* *Functional consequences:* unchanged.
* *Operational impact:* the operator retypes the user name at every launch. The password is typed on every launch in any case, so the cost of retyping the user name is marginal against that.
* *Architectural impact:* none. *Persistence impact:* none.
* *Risks and trade-offs:* zero cost. The operational gain from remembering a user name is small relative to the fact that the password must always be entered, which is the point of the sign-in boundary.
* *Rule conflicts:* none.

**Recommended option: Option 1.**

**Reasoning.** The remembered identity is a per-person convenience, and Option 1 places it in the scope that matches the convenience — the workstation the operator actually signs in at — without a schema change. Option 2 places it in a single global row shared by every client of the server, a scope that does not match the concept and that would surface one operator's identity on a colleague's screen on a shared installation. Option 3 remains defensible on the grounds that the password must always be typed, and the owner should weigh that against the very small cost of Option 1.

**Owner decision required:** Should the remembered sign-in identity be stored per workstation in the local configuration file, or in the shared single-row `SystemSettings` table — and should it be remembered at all, given that the password must be entered on every launch regardless?

*The recommendation is advisory only; the choice is the owner's.*

#### Migration Requirement

**Conditional — the migration requirement depends on the owner decision.**

* **Option 1 (workstation-local file):** no new migration is required. The stored value is a user name written to an existing local JSON document, and no database state is introduced or altered.
* **Option 2 (database):** a new migration **is** required. `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs` created the `SystemSettings` table with its own settings columns, and the current `ApplicationDbContextModelSnapshot.cs` shows the `TopLab.Domain.Settings.SystemSettings` entity without any member for a remembered identity. The new column follows the pattern used for the settings added in `20261020211907_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue.cs`, which added `PrintResultPasswordEnabled` as a `bit` column not nullable with a default of `false`.
* **Option 3 (retain current behaviour):** no new migration is required.

#### Acceptance Conditions

1. The sign-in window offers an explicit, labelled control that determines whether the identity is remembered.
2. With the control enabled and a successful sign-in, the next launch of the sign-in window presents the identity pre-filled in `UserNameTextBox`.
3. The password is never stored in any form by this feature, and the existing hashing of `User.PasswordHash` is untouched.
4. The pre-filled identity is editable, and the operator can clear it.
5. A wrong password still fails with the existing `InvalidCredentialsError`; a remembered identity bypasses nothing.
6. The remembered identity is displayed only on the sign-in form.
7. The identity survives an application restart, and nothing is written when the operator declines to be remembered.
8. Where the identity is stored, it is stored in a form that does not disclose a credential.
9. Regression: the reveal-password toggle, the exit button and the connection-status line continue to behave exactly as they do today.

---

## Appendix — Evidence Index

The primary source locations on which the findings in this specification depend, so that each can be re-verified independently.

| Area | Repository-relative path | Symbols relied upon |
|---|---|---|
| Registration view | `src/TopLab.Presentation/Views/Patients/PatientEditorView.xaml` | `IsVip` checkbox (207); `PaymentAmountText` (33), `PaymentDiscountText` (39), `PaymentPercentageText` (45); `ShowTreatingDoctorDetailsCommand` (261), `ShowReferralEntityDetailsCommand` (266); `PrintWorkSheetCommand` (64), `PrintReceiptCommand` (65), `PrintLabOrderCommand` (67); `PrintEnvelopeCommand` (68), `PatientWindowCommand` (69), `LabResultsCommand` (70), `FollowUpCommand` (71); `IsFastingIndicated` (273); `MedicationConditions` (277-283), `RecentContrastImaging` (289), `RecentUltrasoundImaging` (290), `ConditionToggles` (294-300); `CardNumber` (234), `SampleTypeSummary` (238-241); `ProfileIs*` matrix (486-491); `CustomGroups` picker (493-499); catalogue modes (537-546), filter (549), list (579-591) |
| Registration view model | `src/TopLab.Presentation/ViewModels/Patients/PatientEditorViewModel.cs` | command construction (153-202); `TestGroups` (379), `MedicationConditions` (389), `ConditionToggles` (395); `PaymentAmountText` (670), `PaymentDiscountText` (682), `PaymentPercentageText` (697), `SampleTypeSummary` (619); `FollowUpCommand` documentation (890-895); `_profileIsBlood` (131), `SelectedTestItem._isBlood` (225), `ProfileIs*` (823-831); `AddTestToSelection` (1212-1232); `RefreshSampleTypeSummary` (1132); `MatchesCatalogFilter` (779-781); catalogue mode predicates (760); `ApplyConditionDeltasCommand` dispatch (1531); `RecordPaymentAsync` (1931-1993); `AccountEntryEnterAsync` (2001-2046); `FollowUpAsync` (1778-1793); `PrintAsync` (2185-2231) |
| Search view and view model | `src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml`; `src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs` | search, card-number, lab-id and branch controls (7-25); filter row (28-70); `NotDelivered` (69); `AggregateStatus` column (127); complete filter surface — `SearchText` (114), `CardNumberText` (120), `NotDelivered` (126), `LabIdText` (132), `SelectedTreatingDoctor` (246), `SelectedReferralEntity` (259), `SelectedTest` (272), `SelectedSex` (285), `SelectedAgeUnit` (298), `AgeFrom` (318), `AgeTo` (334), option lists (394-402); query forwarding (600-612); `FetchByLabIdAsync` (640-654) |
| Global search query | `src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/` | `SearchPatientsGlobalQuery` (record shape and tail-append rule); handler name-assist setting (25), term clause (33-45), card-number clause (48-52), not-delivered clause (56-59) |
| Account ledger view model | `src/TopLab.Presentation/ViewModels/Patients/PatientAccountViewModel.cs` | surcharge dispatch (335-344); percentage conversion (379-398) |
| Account ledger view | `src/TopLab.Presentation/Views/Patients/PatientAccountView.xaml` | amount, discount and percentage fields (70-82); correction and extra-charge buttons (98-99) |
| Billing domain | `src/TopLab.Domain/Billing/AccountEntryParser.cs` | `AccountEntryKind` (8-15); `ParsePaidAmount` (75-101); `ParseDiscount` (107-120); `ParsePercentage` (126-139); `HasMinusSign` (222-224); `AccountEntryCalculator` (231-301), `DiscountAmountFromPercentage` (238-251), `Round2` (254-255) |
| Billing domain | `src/TopLab.Domain/Billing/AccountEntrySession.cs` | `DiscountText` (77-89), `PercentageText` (91-102), `ApplyPercentage` (125-153), `PressEnter` (160-273), surcharge/discount exclusion (188-192), D-01 availability rule (194-221) |
| Billing domain | `src/TopLab.Domain/Billing/PaymentOperation.cs` | properties (9-27), `Create` guards (68-95), `Void` (96-99) |
| Billing domain | `src/TopLab.Domain/Billing/PatientAccountCalculator.cs` | `TotalCharged`, `TotalPaid`, `CashRefundable`, `Balance` |
| Billing domain | `src/TopLab.Domain/Billing/InvoiceIssue.cs` | append-only issuance aggregate; gapless-numbering and no-FK conventions |
| Patient aggregate | `src/TopLab.Domain/Patients/Patient.cs` | class declaration (11), `IsVip` (35), `PickupDateUtc` (39), `CardNumber` (58), `MaxCardNumberLength` (132), `EnsureNotDeleted` (69-75), `Create` and `Update` |
| Test aggregate | `src/TopLab.Domain/Results/PatientTest.cs` | specimen flags (19-29), `EnteredAtUtc` (39), `IsReviewed` (45), `IsPrinted` (48), `PrintCount` (50), `IsDelivered` (56), `Create` (98-113), `MarkPrinted` (178-189), `UpdateSampleFlags` (226-240) |
| Status calculator | `src/TopLab.Domain/PatientStatus/PatientStatusCalculator.cs` | stage mapping (30-45) |
| Test catalogue | `src/TopLab.Domain/Tests/Test.cs`; `src/TopLab.Domain/Tests/TestGroup.cs`; `src/TopLab.Application/Features/TestCatalogAndReferenceRanges/Common/` | `Test.TestGroupId`; `TestGroup` (`Name`, `IsActive`); `TestSummaryDto` (`TestGroupId`, `TestGroupName`); `GetTestGroupsQueryHandler` |
| Group screen precedent | `src/TopLab.Presentation/ViewModels/Lab/TestGroupsViewModel.cs` | grouping of `Test` rows on `TestGroupId` to obtain per-group counts (115-123) |
| Registration commands | `src/TopLab.Application/Features/PatientRegistration/Commands/` | `CreatePatientCommand`/`Handler`/`Validator`, `UpdatePatientCommand`/`Handler`, `AddTestsToVisitCommand` (+`AddTestInput`), `AddProfileToVisitCommand`/`Handler`, `AddCustomGroupToVisitCommand`/`Handler`, `UpdatePatientTestSampleFlagsCommand`, `PrintBarcodeCommand`; `ClearAllTestsCommandHandler` (first-registration rule and `FirstRegistrationOnlyMessage`) |
| Billing commands | `src/TopLab.Application/Features/PatientBilling/Commands/` | `RecordPaymentCommand`/`Validator`, `RecordExtraChargeCommand`/`Validator`/`Handler`, `PrintReceiptCommand`/`Handler`, `PrintInvoiceCommand`/`Handler` (gapless number allocation and retry) |
| Registration catalogue | `src/TopLab.Application/Features/PatientRegistration/Queries/GetRegistrationCatalog/GetRegistrationCatalogQueryHandler.cs`; `.../Common/PatientRegistrationDtos.cs` | catalogue aggregation (31-38); `RegistrationCatalogDto` (83-93) |
| Statistics | `src/TopLab.Application/Features/Statistics/Queries/GetPatientCountStatistics/`; `.../GetTestCountStatistics/`; `src/TopLab.Presentation/ViewModels/Statistics/StatisticsViewModel.cs` | day defaulting (30-33), day-of-month grouping (103-115), money row (120-130), `MoneyRowText` (216-223) |
| Result delivery | `src/TopLab.Application/Features/ResultDelivery/` | `GetUndeliveredResultsQuery`/`Handler` (44-51), `UndeliveredPatientRowDto`, `PrintGroupPatientSlipsCommand`/`Handler`, `GetGroupPatientSlipsQuery`/`Handler` (explicit-id selection), `GroupPatientSlipDtos` |
| Identifier rule | `src/TopLab.Application/Features/PatientEnvelope/Common/BarcodePayload.cs` | `BarcodePayload.For` — the single identifier rule for scannable surfaces |
| Bulk print | `src/TopLab.Application/Features/ResultsEntry/Commands/BulkPrint/BulkPrintPreflightQueryHandler.cs`; `src/TopLab.Presentation/Views/Patients/BulkPrintDialogWindow.xaml` | verified and reprint predicates |
| Print policy precedent | `src/TopLab.Domain/Users/User.cs`; `src/TopLab.Application/Features/ResultsEntry/Commands/MarkResultPrinted/`; `.../ProfileResults/Commands/MarkProfilePrinted/`; `.../CultureResults/Commands/MarkCultureReportPrinted/`; `.../ReportProduction/Commands/PrintCombinedReport/`, `PrintHistoryReport/`; `.../ResultsEntry/Commands/BulkPrint/ExecuteBulkPrint/`; `src/TopLab.Application/Features/ResultsEntry/Common/BalanceProbe.cs`; `.../CultureResults/Common/BalanceProbe.cs`; `.../PatientSearch/Common/BalanceProbe.cs` | `User.BlockPrintOnRemainingBalance` (18); the six handlers enforcing it with `BalanceProbe` and the Arabic refusal message; the three `internal static class BalanceProbe` declarations and the alias import used by the ReportProduction handlers |
| Results worklist and worksheets | `src/TopLab.Presentation/Views/Patients/ResultsWorklistView.xaml`; `src/TopLab.Presentation/Views/WorkSheets/WorkSheetsView.xaml`; `src/TopLab.Presentation/ViewModels/WorkSheets/WorkSheetsViewModel.cs` | tri-state filters (14-35) with the tri-state binding note (16-19) and the printed-filter mapping (28-33); `UnfinishedOnly` (74) and its forwarding (437); `PendingCount` display (216) |
| Shell | `src/TopLab.Presentation/MainWindow.xaml`; `src/TopLab.Presentation/ViewModels/Shell/ShellViewModel.cs` | ribbon (37-50), status bar (57-93), `NavigationItems` (188-308), `LoadStatusAsync` (319-378) |
| Audit surface | `src/TopLab.Presentation/ViewModels/Audit/AuditViewModel.cs`; `src/TopLab.Application/Features/AuditAndTraceability/Queries/GetPatientAudit/` | `LoadWithPatientIdAsync` (162-167), the three tabs (67-69); `GetPatientAuditQuery` |
| Sign-in | `src/TopLab.Presentation/Views/Setup/LoginWindow.xaml` (+ code-behind); `src/TopLab.Presentation/ViewModels/Setup/LoginViewModel.cs`; `src/TopLab.Application/Features/UsersAndPermissions/Commands/SignIn/`; `src/TopLab.Domain/Users/User.cs` | reveal-password toggle (36-37), `LoginViewModel` members, `SignInCommand`/`Handler`, `User.LastLoginAtUtc` (28), `User.RecordLogin` (121) |
| Local persistence | `src/TopLab.Presentation/Services/Configuration/ConfigurationFileService.cs`; `src/TopLab.Infrastructure/Services/JsonPhoneBookStore.cs`; `JsonPurchasesListStore.cs` | `%ProgramData%\TopLab\appsettings.json` accessor; workstation-locality rationale, atomic write and `directoryOverride` test seam |
| Printing | `src/TopLab.Infrastructure/Printing/ReceiptPrintingService.cs` (21-24), `GroupPatientSlipPdfWriter.cs` (A5, one page per slip, barcode reuse), `WorkSheetPdfWriter.cs`, `LabOrderPdfWriter.cs`; `src/TopLab.Infrastructure/Barcode/BarcodeService.cs`, `BarcodeLabelRenderer.cs` | `PrintOnce` is display-only; slip title and layout; label page 288 × 144 pt and 300 × 80 px bitmap |
| Settings | `src/TopLab.Domain/Settings/ReceiptSettings.cs`; `SystemSettings.cs`; `src/TopLab.Infrastructure/Persistence/Configurations/ReceiptSettingsConfiguration.cs`, `SystemSettingsConfiguration.cs` | `PrintOnce` (15, 59, 112); single-row `SystemSettings` (key and `HasData` seed with `Id = 1`) |
| Persistence configuration | `src/TopLab.Infrastructure/Persistence/Configurations/PatientConfiguration.cs`, `PatientTestConfiguration.cs`, `PaymentOperationConfiguration.cs`, `TestConfiguration.cs`, `InvoiceIssueConfiguration.cs` | `CardNumber` mapping and index; `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered`; `PaymentOperation` columns; `Tests.TestGroupId` foreign key and index; `InvoiceIssues` unique number index and absent patient foreign key |
| Migrations and snapshot | `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs`; `20260907162756_AddPatientIsDeletedAndPatientTestSampleDrawnIndex.cs`; `20260916113704_AddInvoiceIssues.cs`; `20261020211907_AddPatientCardNumberUltrasoundAndMedicalHistoryCatalogue.cs`; `20261025123435_AddRefundAmountToPaymentOperation.cs`; `ApplicationDbContextModelSnapshot.cs` | creation of `Patients`, `PatientTests`, `PaymentOperations`, `TestGroups`, `Tests`, `ReceiptSettings`, `SystemSettings`; `Patients.IsDeleted`; `IX_PatientTests_IsReviewed_IsPrinted_IsDelivered`; `Patients.CardNumber` and `IX_Patients_CardNumber`; `PaymentOperations.RefundAmount`; current entity shapes for `Patient`, `PatientTest` and `PaymentOperation` |
