# Final Implementation Plan — Module 14: External Entities

**Module:** M-14 — External Entities (Treating Doctor / Referral or Contract Entity / Partner Lab)
**Wave:** 2 — Reference Data
**Target commit:** `e3fa0832abba7dde3e2592415f5e57c3360037f7` (verified: `git rev-parse HEAD` matches)
**Stack:** .NET 8, EF Core 8.0.30, SQL Server (LocalDB `(localdb)\mssqllocaldb`, database `TopLab`), MediatR 12.5.0, FluentValidation 12.1.1, xUnit 2.9.3 + coverlet, Result pattern (no exceptions for expected failures).
**PRD section:** `Docs/Source/Top_Lab_PRD.md` §M14 (FR-M14-001 … FR-M14-006)
**Reference material:** `Docs/M14_External_Entities_Reference_Pages.pdf` (curated reference-system screens: §3-7 add/edit/delete of treating doctors and referral/contract entities, §3-11 ID-generation action for doctors and partner labs, plus the System → الجهات الخارجية والمعامل navigation and the left-hand registered-entities list).
**Dependencies (verified done):** M17 (Users & Permissions) and M22 (System & Print Settings) — confirmed 🟩 Done in `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` §4; authorization pipeline (`IAuthorizedRequest` + `AuthorizationBehavior`) and the `SystemSettings.SaveTreatingDoctorOnlyFromEntityWindow` flag exist in code.
**Downstream consumers:** M02 (patient references treating doctor + referral entity; generated-code lookup), M13 (price lists assigned to entities), M16 (sent-out sample external lab), M19 (statistics by entity), M20 (inventory & accounting per entity) — per `Docs/Source/Top_Lab_Module_Dependency_Map.md` §4.

---

## 1. Current State (verified at target commit)

- **Domain.** `src/TopLab.Domain/ExternalEntities/ExternalEntity.cs` (89 lines) exists as an `AuditableEntity<ExternalEntityId>` with the full field set: `EntityType`, `Name`, `City`, `Address`, `Phone`, `Fax`, `ResponsiblePersonName`, `ResponsiblePersonPhone`, `PriceListId` (strongly-typed `PriceListId?`), `DiscountOrCommissionPercent` (`decimal?`), `GeneratedIdCode` (`string?`). It exposes **only** a `Create` factory with exactly two guards: name required (non-whitespace, trimmed) and *TreatingDoctor must not have `PriceListId`*. There are **no** `Update`, delete, or ID-code mutators, and **no** ReferralOrContract-requires-price-list guard. `EntityType` (`src/TopLab.Domain/Common/Enums/EntityType.cs`) is `TreatingDoctor = 0, ReferralOrContract = 1, PartnerLab = 2`.
- **Application.** No `Features/ExternalEntities/` folder exists. The established pattern (M12/M22) is one folder per use case (`<Name>Command.cs` / `<Name>CommandHandler.cs` / `<Name>CommandValidator.cs`; `<Name>Query.cs` / `<Name>QueryHandler.cs`), `IAuthorizedRequest` with a string permission code, `Result<T>` + `Error.NotFound/Conflict/Validation/Forbidden`, handlers depending only on `IApplicationDbContext` (`Set<T>()`, `Add/Update/Remove`, `SaveChangesAsync`). Validators are registered assembly-wide in `src/TopLab.Application/DependencyInjection.cs` (`AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()`), so new validators need **no DI change**. A `UniqueViolationFakeApplicationDbContext` already exists in the test project for exercising unique-violation catch paths (precedent: `CreateTestCommandHandler` catches a simulated SQL unique violation and maps it to `Error.Conflict`).
- **Infrastructure.** `src/TopLab.Infrastructure/Persistence/Configurations/ExternalEntityConfiguration.cs` is complete: `ExternalEntityId` conversion + `ValueGeneratedOnAdd`, `EntityType` as `tinyint` required, `Name` max-200 required, contact fields optional at lengths 100/300/30/30/150/30, `PriceListId` nullable with strongly-typed-id conversion, `DiscountOrCommissionPercent` `decimal(5,2)`, `GeneratedIdCode` max-50 **nullable, no unique index**, FK `ExternalEntity → PriceList` with `OnDelete(SetNull)`, `HasIndex(e => e.EntityType)`. The F5 baseline migration `20260828052248_BaselineDataModel.cs` already creates the `ExternalEntities` table (lines 394–420). Infrastructure DI (`src/TopLab.Infrastructure/DependencyInjection.cs`) is the established registration site for concrete services behind Application ports.
- **Referencing sides (delete-guard landscape, verified).** `Patient.TreatingDoctorId` and `Patient.ReferralEntityId` are optional `ExternalEntityId?` columns configured as plain nullable conversions **without explicit `HasOne` relationships** (shadow/implicit behavior at the model level — the baseline migration creates no FK constraints for them); `SentOutSample.ExternalLabEntityId` is a **required** FK with `OnDelete(Restrict)` and an index; `CashMovement.RelatedExternalEntityId` is optional with `OnDelete(SetNull)`.
- **Settings.** `SystemSettings.SaveTreatingDoctorOnlyFromEntityWindow` exists (seeded `false`). `Sex` enum is `Male = 0, Female = 1`.
- **Docs.** `Docs/Source/Top_Lab_ADR.md` max ADR at HEAD is **ADR-0029** → the next number is **ADR-0030**. `Docs/Handoff_M12.md` and `Docs/Handoff_M22.md` confirm the close-out convention: tracking-sheet row flip + dated change-log row + handoff document per `Docs/Source/Top_Lab_Handoff_Template.md`.

**Consequence:** Module 14 requires **no new EF Core migration** and **no schema change**. The work is: complete the Domain aggregate (mutators + invariants), build the Application read/write surface, add the ID-code generator behind a port, and lock all of it down with tests and close-out documentation.

---

## 2. Settled Business Rules (fixed — not open decisions)

These five rules are settled facts and are encoded directly in the slices below. They must not be reopened or presented as alternatives:

1. **Empty-referral default.** When a referral-entity name is empty, the default display value is produced by a standalone, pure resolver returning the frozen literals `"Himself"` (patient sex Male) / `"Herself"` (patient sex Female). It has **no dependency on any system/print setting**; nothing is stored.
2. **Deletion.** One **uniform guarded hard-delete** rule for all three entity types: deletion is blocked (`Conflict`) when the entity is still referenced by a patient (`TreatingDoctorId` or `ReferralEntityId`) or by a sent-out record (`ExternalLabEntityId`); otherwise the row is physically removed. No per-type rules, no soft delete.
3. **Generated-ID uniqueness.** Enforced at the Application/handler level **only**: existence pre-check → save → unique-violation catch → bounded retry. **No new database unique index and no migration.**
4. **Placeholder style.** Only the sex-based `Himself`/`Herself` rule is implemented. No secondary/legacy placeholder style is in scope.
5. **Contact fields & partner-lab price lists.** All contact fields (city, address, phone, fax, responsible-person name/phone) remain optional for every entity type. Partner Lab price-list assignment accepts **any existing price list** — no list-kind/taxonomy restriction (taxonomy belongs to M13).

Additional rules derived from code evidence and the reference material (also settled, with evidence):

- **Percent range.** `DiscountOrCommissionPercent`, when set, is **0–100 inclusive**, uniform across all three types (column is `decimal(5,2)`; 0–100 is the business-sensible bound).
- **Type transitions.** `Update` re-validates **all** cross-field guards on every call, so a type transition can never bypass guards (TreatingDoctor → no price list; ReferralOrContract → price list required; PartnerLab → price list optional).
- **Generated-code origin.** `GeneratedIdCode` is written **only** by the dedicated ID-generation command (create-then-generate sequencing). No create path accepts a caller-supplied code. The reference PDF (§3-11) shows the ID action as a discrete button press per entity — matching this design.
- **Permission gate.** Every M14 write command carries `IAuthorizedRequest` with `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` (the established reference-data gate: seeded permission Id 10, used by M22 settings writes and M12 test-catalog commands). Read queries are open (no `IAuthorizedRequest`), per the established read policy.

### Cross-module delegations

| Concern | Owner | M-14's relationship |
|---|---|---|
| `PriceList` CRUD and list-kind taxonomy | M13 | M-14 existence-checks `PriceListId` only (`PriceLists` set); no behavior change, no taxonomy enforcement |
| `SaveTreatingDoctorOnlyFromEntityWindow` semantics (patient-entry auto-save) | M02 (future) | M-14 is read-only aware; no handler reads or branches on the flag. The deferred question of whether this flag relates to patient-side auto-save is carried to the M02 owner via the handoff |
| `Patient.TreatingDoctorId` / `ReferralEntityId` | M02 | M-14 only counts references for the delete guard; never mutates |
| `SentOutSample.ExternalLabEntityId` | M16 | M-14 only counts references for the delete guard; never mutates |
| Reporting-time rendering of the resolver output | M07 | M-14 exposes the pure resolver; the reporting consumer invokes it and owns any rendering-time concerns |
| Generated-code lookup on the patient screen | M02 | M-14 generates and persists `GeneratedIdCode`; the by-code lookup query ships in M-14's read surface so M02 can consume it |

---

## 3. Slices and Execution Order

Five slices, executed in order: **S1 → S2 → S3 → S4 → S5.** (S2 and S3 both depend only on S1 and may be worked in either sequence; each slice is completed and gated before the next is closed.)

---

### Slice S1 — Domain behaviors and invariants (Domain layer)

**Objective.** Extend `ExternalEntity` from Create-only to full lifecycle: add `Update` and `RegenerateIdCode` mutators, add the *ReferralOrContract requires `PriceListId`* guard to `Create` **and** `Update`, add the uniform 0–100 percent guard, and normalize optional strings (trim; whitespace → null). All invariants live inside the entity; the Application layer cannot reach an invalid state.

**Files to add / change.**

| File | Action |
|---|---|
| `src/TopLab.Domain/ExternalEntities/ExternalEntity.cs` | **Modify.** Add `Update(EntityType, string name, string? city, string? address, string? phone, string? fax, string? responsiblePersonName, string? responsiblePersonPhone, PriceListId?, decimal? discountOrCommissionPercent)` and `RegenerateIdCode(string code)`; extend `Create` with the Referral-requires-list and percent guards; keep the existing TreatingDoctor-no-list and name-required guards; normalize all string inputs |
| `tests/TopLab.Domain.Tests/ExternalEntities/ExternalEntityTests.cs` | **Create** (folder does not exist at HEAD) — full guard matrix plus happy paths |

**Design rules.**

- `Update` re-validates **every** cross-field rule, so both transition directions fail safely: `TreatingDoctor → ReferralOrContract` without a list fails; `ReferralOrContract → TreatingDoctor` while a list is present fails unless the list is cleared in the same call.
- `RegenerateIdCode(string code)`: rejects null/whitespace and length > 50 (matches the `nvarchar(50)` column); stores the trimmed value; **uniqueness is not checkable in Domain** — it is the handler's responsibility (S3).
- Percent: `null` = not set; otherwise `0 ≤ value ≤ 100` inclusive, uniform across all three types.
- **No delete mutator and no `IsActive` column.** Deletion is a handler-side operation (`IApplicationDbContext.Remove`) after reference checks, because the checks inherently need the database; a Domain precondition hook would duplicate logic without enforcement power.
- No services, no persistence access, no MediatR/EF references in Domain. Existing `Create` call sites and tests broken by the new Referral guard are fixed **inside this slice**.

**Dependencies.** None (root slice).

**Completion / acceptance condition.**
`dotnet build src/TopLab.Domain` → 0 errors / 0 warnings. New `ExternalEntityTests` cover: `Create`/`Update` happy paths per type; negative paths for *referral-without-list*, *doctor-with-list*, *percent < 0 / > 100*, *name whitespace / > 200*, *code empty / > 50*; both type-transition directions; whitespace-to-null normalization. All existing Domain tests still green. Domain coverage for the `ExternalEntities` scope ≥ 90% (coverlet, module filter). No analyzer suppressions; no new packages.

**Risk.** Medium — `Create` gains a breaking guard by design; missed call sites surface here, not downstream.

---

### Slice S2 — Application read surface + referral-name resolver (Application layer)

**Objective.** Deliver the query surface (type-filtered, searchable, paged list; single-entity detail; lookup by generated code) and the pure `ReferralNameResolver` implementing the settled empty-referral default. No state changes; reads are open (no permission gate).

**Files to add / change.**

| File | Action |
|---|---|
| `src/TopLab.Application/Features/ExternalEntities/Common/ExternalEntityDtos.cs` | **Create.** `ExternalEntityListItemDto` (Id, EntityType, Name, City, Phone, PriceListId, PriceListName, DiscountOrCommissionPercent, GeneratedIdCode) and `ExternalEntityDetailDto` (all fields) |
| `Queries/SearchExternalEntities/{SearchExternalEntitiesQuery.cs, SearchExternalEntitiesQueryHandler.cs, SearchExternalEntitiesQueryValidator.cs}` | **Create.** `EntityType? EntityType`, `string? SearchTerm`, `int Page`, `int PageSize`; `AsNoTracking`; type filter + `Contains` search over Name/City/Phone/GeneratedIdCode; ordered by Name; paged projection; `PriceListName` via flat left-join projection only (no `Include`, no lazy loading); validator: paging bounds, search max length, `EntityType IsInEnum`, Arabic messages |
| `Queries/GetExternalEntityById/{GetExternalEntityByIdQuery.cs, GetExternalEntityByIdQueryHandler.cs}` | **Create.** Single fetch by id; missing → `Error.NotFound("الجهة الخارجية غير موجودة.")` |
| `Queries/GetExternalEntityByCode/{GetExternalEntityByCodeQuery.cs, GetExternalEntityByCodeQueryHandler.cs}` | **Create.** Lookup by `GeneratedIdCode` (the code the patient screen will use); missing → `Error.NotFound` |
| `Common/ReferralNameResolver.cs` | **Create.** `public static string Resolve(string? referralName, Sex patientSex)` — returns the trimmed input when non-empty; whitespace-only treated as empty; otherwise `"Himself"` (Male) / `"Herself"` (Female). Frozen literals; **no EF, no MediatR, no settings read, no localization dependency** (settled rules 1 & 4) |
| `tests/TopLab.Application.Tests/Features/ExternalEntities/Queries/*` + `ReferralNameResolverTests.cs` | **Create.** Type filter (seed 3 types, assert only the requested type returns), each search axis, paging, empty-set, unknown-id and unknown-code NotFound; resolver: passthrough, trim, whitespace-as-empty, null-male → Himself, null-female → Herself |
| `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` | **Verify only** — already exposes `ExternalEntities`, `Patients`, `SentOutSamples`, `CashMovements` lists with `Set/Add/Remove` branches; extend only on proven drift |

**Dependencies.** Requires S1 (entity shape, enum values).

**Completion / acceptance condition.**
`dotnet build src/TopLab.Application` → 0 errors / 0 warnings. All S2 handler/validator/resolver tests green (fake-backed). No command implemented in this slice; no query implements `IAuthorizedRequest`. Application coverage for the S2 footprint ≥ 80%. Validator Arabic messages match the normative table (§5).

**Risk.** Low — no schema, no concurrency, no cross-aggregate writes.

---

### Slice S3 — Application write surface + validators + ID-generator port (Application layer)

**Objective.** `Create` / `Update` / `Delete` / `GenerateEntityIdCode` commands with per-type rules, FluentValidation validators with Arabic messages, `EDIT_SYSTEM_SETTINGS` authorization on every write command, and the `IEntityIdCodeGenerator` port. Enforces `PriceListId` existence, `GeneratedIdCode` uniqueness (pre-check + catch + bounded retry, settled rule 3), and the uniform guarded hard-delete (settled rule 2).

**Files to add / change.**

| File | Action |
|---|---|
| `Common/Interfaces/IEntityIdCodeGenerator.cs` | **Create.** Stateless port `string Generate()`; concrete implementation ships in S4 (Infrastructure) |
| `Commands/CreateExternalEntity/{CreateExternalEntityCommand.cs, CreateExternalEntityCommandHandler.cs, CreateExternalEntityCommandValidator.cs}` | **Create.** `IAuthorizedRequest<Result<int>>`; sentinel `ExternalEntityId.Create(0)` then real id read back after save; `PriceListId` existence check when supplied; Domain-guard failures mapped to Arabic `Error`; **validator rejects a caller-supplied `GeneratedIdCode`** |
| `Commands/UpdateExternalEntity/{...3 files}` | **Create.** Same field set + `Id`; load → `NotFound` when missing → existence-check new `PriceListId` → `entity.Update(...)` (re-validates all guards, including type transitions) → save |
| `Commands/DeleteExternalEntity/{...3 files}` | **Create.** Load → `NotFound` → reference-guard matrix (below) → `Remove` → save; `DbUpdateException` (Restrict) catch → `Conflict` as a second net |
| `Commands/GenerateEntityIdCode/{...3 files}` | **Create.** Input `Id` → `Result<string>`; load → `NotFound` → `IEntityIdCodeGenerator.Generate()` → `AnyAsync` pre-check on `GeneratedIdCode` → `entity.RegenerateIdCode(candidate)` → save; unique-violation catch (`Exception` filter matching the M12 `IsUniqueViolation` precedent) → regenerate, **max 5 attempts**, exhaustion → `Error.Conflict("تعذر توليد رمز فريد، حاول مرة أخرى.")` |
| `tests/.../Commands/<UseCase>/*Tests.cs` (one test class per command) + `ExternalEntitiesAuthorizationTests.cs` | **Create.** Per-command matrices (below) + `[Theory]` over all 4 write commands: `RequiredPermissionCode == "EDIT_SYSTEM_SETTINGS"`, forbidden without the grant, absolute-permission bypass |

**Delete-guard matrix (settled rule 2, uniform across all three types).**
Handler checks run **before** `Remove`, so behavior is explicit and storage-independent:

- Block with `Error.Conflict("تعذر حذف الجهة لوجود مرضى مرتبطين بها.")` when `Patients.AnyAsync(p => p.TreatingDoctorId == id || p.ReferralEntityId == id)`.
- Block with `Error.Conflict("تعذر حذف الجهة لوجود عينات مرسلة مرتبطة بها.")` when `SentOutSamples.AnyAsync(s => s.ExternalLabEntityId == id)` (mirrors the DB-level `Restrict`).
- Do **not** block on `CashMovement.RelatedExternalEntityId` (`SetNull` nulls the FK) or on price-list deletion (`SetNull` on `ExternalEntity.PriceListId`).

**Layer split.** Domain checks null/shape (price-list presence per type, percent range, name/code shape); handlers check existence (`PriceLists` set) and references. Validators mirror Domain rules textually per the normative table (§5); Domain remains authoritative at runtime (defense in depth). The `SaveTreatingDoctorOnlyFromEntityWindow` flag is neither read nor branched on in any M14 handler (settled rule 1's corollary: M14 commands **are** the entity-management surface and are always permitted to authorized callers).

**Dependencies.** Requires S1.

**Completion / acceptance condition.**
`dotnet build src/TopLab.Application` → 0 errors / 0 warnings. All handler/validator/authorization tests green, including: create — doctor without list succeeds / with list fails, referral without list fails, referral with unknown list → NotFound, partner lab with/without list succeeds, percent out-of-range fails; update — success, NotFound, both transition rejections, list existence; delete — success when unreferenced, Conflict via each patient FK, Conflict via sent-out sample, success with only CashMovement references, NotFound; generate — non-empty unique code, overwrite of prior code, collision-retry via stubbed generator, exhaustion → Conflict, code trim/length. The unique-violation catch path is exercised with the existing `UniqueViolationFakeApplicationDbContext`. A grep gate confirms no create-path input accepts `GeneratedIdCode`. Application coverage for the S3 footprint ≥ 80%.

**Risk.** Medium — concurrency on code generation and delete-guard completeness, contained by bounded retry + explicit checks + S4 model-proof tests.

---

### Slice S4 — Infrastructure proof: generator, mappings, FK matrix, ADR-0030 (Infrastructure layer)

**Objective.** Ship the `IEntityIdCodeGenerator` implementation and DI registration; **verify (not rebuild)** the storage mapping against the F5 baseline; pin the FK delete-behavior matrix with model-assertion tests; extend validator-registration coverage; record ADR-0030 including the migration-scope outcome.

**Files to add / change.**

| File | Action |
|---|---|
| `src/TopLab.Infrastructure/Services/SecureEntityIdCodeGenerator.cs` | **Create.** Implements `IEntityIdCodeGenerator`: cryptographic-random alphanumeric, fixed 12 chars from an unambiguous alphabet (no 0/O/1/I), always within the `nvarchar(50)` column, thread-safe stateless |
| `src/TopLab.Infrastructure/DependencyInjection.cs` | **Modify.** `services.AddSingleton<IEntityIdCodeGenerator, SecureEntityIdCodeGenerator>();` (Singleton: stateless; matches the established Infrastructure-side registration pattern) |
| `src/TopLab.Infrastructure/Persistence/Configurations/ExternalEntityConfiguration.cs` | **Verify only** — modify only if the scope gate proves a gap |
| `tests/TopLab.Infrastructure.Tests/Persistence/ExternalEntityDeleteBehaviorTests.cs` | **Create.** Model-assertion tests via `InMemoryContextFactory` (precedent: `TestDeletionCascadeTests.cs`): both `Patient → ExternalEntity` FKs recorded with their actual `DeleteBehavior` asserted; `SentOutSample → ExternalEntity` = `Restrict`; `CashMovement → ExternalEntity` = `SetNull`; `ExternalEntity → PriceList` = `SetNull` |
| `tests/TopLab.Infrastructure.Tests/Persistence/Configurations/F5ConfigurationTests.cs` | **Extend.** `ExternalEntity` mapping assertions: `Name` required/max-200; contact fields optional at configured lengths; `GeneratedIdCode` nullable/max-50; `PriceListId` nullable + `SetNull`; `DiscountOrCommissionPercent` `decimal(5,2)`; index on `EntityType`; **negative assertion: no unique index includes `GeneratedIdCode`** (locks settled rule 3) |
| `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` | **Extend.** Resolve the 4 new validators from a host built like `App` (precedent: existing lines 10–19) |
| Infrastructure DI/generator resolution test | **Create.** Resolve `IEntityIdCodeGenerator` from the composed provider; assert Singleton lifetime and non-empty, distinct outputs |
| `Docs/Source/Top_Lab_ADR.md` | **Modify.** Append **ADR-0030** (max ADR at HEAD is 0029 — reconfirm at execution; if higher, use the next free number) |

**Migration-scope gate.** Model-vs-snapshot verification runs **first**: compare `ExternalEntityConfiguration` against the baseline migration (`20260828052248_BaselineDataModel.cs:394-418`) and `ApplicationDbContextModelSnapshot.cs` for `GeneratedIdCode` type/nullability/length, FK behaviors, and indexes. If drift is found, S4 produces a narrowly-scoped migration addendum (and plan finality is revisited for that addendum); otherwise ADR-0030 records *"No migration — verified zero drift."* "No migration expected" is an input assumption, never the proof.

**ADR-0030 contents.** Type→price-list matrix; hard-delete-with-guards rationale (alignment with ADR-0018); uniqueness strategy (handler-level pre-check + catch + retry; no index; revisit only on observed duplicates); pure-resolver decision; permission reuse (`EDIT_SYSTEM_SETTINGS`); PartnerLab taxonomy non-enforcement; ID algorithm choice (12-char unambiguous alphabet); create-then-generate sequencing; zero-drift verification result.

**Dependencies.** Requires S2 + S3 (handlers, DTOs, port stable).

**Completion / acceptance condition.**
`dotnet build TopLab.sln` → 0 errors / 0 warnings. All new + extended Infrastructure tests green, including the four-side FK matrix and the generator/validator resolution tests. `dotnet ef migrations list` shows no new migration (or, if the gate triggered, the addendum applies cleanly to an F5-baselined database and rolls back). `ApplicationDbContextModelSnapshot.cs` unchanged. ADR-0030 committed with the outcome recorded. Infrastructure coverage for the S4 footprint ≥ 70%.

**Risk.** Low–Medium — risk is confined to discovering a schema gap late; the verification-first ordering and the narrow migration gate contain it.

---

### Slice S5 — Hardening, documentation, module close-out (cross-cutting, no entity code)

**Objective.** Close the module: tracking-sheet flip, handoff document, coverage roll-up, audit-interceptor gate, final full-suite green. Inputs: S1–S4 merged.

**Files to add / change.**

| File | Action |
|---|---|
| `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` | **Modify.** Flip the M14 row (currently line 65, `⬜ Design`) to 🟩 Done with a deliverables note; add a dated §9 change-log row mirroring the M12 closing row |
| `Docs/Handoff_M14.md` | **Create** per `Docs/Source/Top_Lab_Handoff_Template.md`: contracts (commands/queries/DTOs/`Result` codes), Arabic message table, consumer notes — M02 (resolver signature + `GetExternalEntityByCode` + the deferred flag-link question for the M02 owner), M07 (resolver contract; consumers own rendering-time concerns), M13 (taxonomy non-enforcement + price-list-deletion orphan note: reads unaffected, next `Update` forces reassignment — self-healing), M16/M19/M20 (guarded-delete semantics) |
| `Docs/Source/Top_Lab_ADR.md` (ADR-0030) | **Editorial polish only** — ensure settled outcomes are recorded |

**Quality gates.**
- **Build:** `dotnet build TopLab.sln -c Release` → 0 errors / 0 warnings.
- **Tests:** full suite green (`dotnet test TopLab.sln -m:1` posture per the M12 precedent).
- **Coverage floors** (per `Docs/Source/Top_Lab_Test_Strategy.md` §4): Domain ≥ 90%, Application ≥ 80%, Infrastructure ≥ 70% for the M-14 footprint; uncovered public members get explicit waivers in the handoff, not ad-hoc test padding.
- **Audit gate:** integration-style test asserting that after an entity write, audit columns are written solely by `AuditableEntitySaveChangesInterceptor` — `CreatedByUserId/CreatedAtUtc` untouched, modification columns advanced.
- Slopwatch-style pass on all touched files; the unique-violation catch maps to `Conflict`, never swallows.
- No `git add` / `git commit` / `git push` performed by the executing agent (owner commits per established convention).

**Dependencies.** Requires S1–S4.

**Completion / acceptance condition.**
All files above committed/staged per convention; handoff sections filled; coverage floors met or explicitly waived; audit gate passed; Release build 0/0; full suite green; Master Tracking Sheet M14 row 🟩 Done with dated change-log entry.

**Risk.** Low.

---

## 4. Acceptance Criteria (module level)

- **A1** — `dotnet build TopLab.sln -c Release` → 0 errors / 0 warnings.
- **A2** — Domain tests cover every new mutator and every guard's negative path, including both type-transition directions.
- **A3** — Full suite green over all affected projects.
- **A4** — Diff confined to `src/TopLab.Domain/ExternalEntities/**`, `src/TopLab.Application/Features/ExternalEntities/**`, `src/TopLab.Infrastructure/**` (generator + DI registration only), `tests/TopLab.*/**`, and `Docs/**`. No `.csproj`/package changes; no migration unless the S4 gate proves a gap.
- **A5** — All 4 write commands reject unauthorized callers via `AuthorizationBehavior` (theory tests).
- **A6** — S4 FK-matrix model tests pin all referencing-side behaviors (both `Patient` FKs, `SentOutSample`, `CashMovement`) plus `ExternalEntity → PriceList SetNull`, and the no-unique-index assertion on `GeneratedIdCode`.
- **A7** — Zero Presentation content anywhere in the diff: no ViewModel, View, XAML, window, dialog, or navigation artifact.
- **A8** — Coverage floors met (Domain ≥ 90% / Application ≥ 80% / Infrastructure ≥ 70%) or explicit waivers recorded.
- **A9** — ADR-0030 appended; migration-scope outcome recorded (zero-drift proof or gap-driven addendum).
- **A10** — Validator/generator resolution tests pass against the composed provider.
- **A11** — No `GeneratedIdCode` input accepted on any create path (grep gate).
- **A12** — Audit-interceptor gate passes for entity writes.

## 5. Normative Arabic Messages

Validators mirror Domain wording for the same rule; final phrasing is frozen in S2/S3 review.

| Condition | Message |
|---|---|
| Name missing / whitespace | `اسم الجهة الخارجية مطلوب.` |
| Name too long (> 200) | `اسم الجهة الخارجية يجب ألا يتجاوز 200 حرفًا.` |
| `EntityType` invalid | `نوع الجهة غير صالح.` |
| Treating doctor with `PriceListId` | `الطبيب المعالج لا يرتبط بقائمة أسعار.` |
| Referral/contract without `PriceListId` | `جهة الإحالة / التعاقد تتطلب قائمة أسعار.` |
| `PriceListId` not found | `قائمة الأسعار المحددة غير موجودة.` |
| Percent out of range | `نسبة الخصم / العمولة يجب أن تكون بين 0 و 100.` |
| Entity not found | `الجهة الخارجية غير موجودة.` |
| Delete blocked by patients | `تعذر حذف الجهة لوجود مرضى مرتبطين بها.` |
| Delete blocked by sent-out samples | `تعذر حذف الجهة لوجود عينات مرسلة مرتبطة بها.` |
| Code-generation retry exhausted | `تعذر توليد رمز فريد، حاول مرة أخرى.` |
| `GeneratedIdCode` format invalid | `رمز الجهة غير صالح.` |
| Caller-supplied code on create | `رمز الجهة يتم توليده فقط عبر أمر التوليد.` |
| Paging invalid | `معاملات الترقيم غير صالحة.` |
| Pipeline-owned forbidden | `أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام` (shared pipeline message) |

## 6. Risks and Mitigations

| ID | Risk | Mitigation |
|---|---|---|
| R-1 | Referral-requires-list guard breaks existing seeds/tests | S1 owns the break in-slice; S3 validators mirror the rule; normative table keeps wording identical |
| R-2 | `GeneratedIdCode` collision under concurrency without a unique index | Pre-check + unique-violation catch + 5-attempt bounded retry + high-entropy 12-char generator; revisit trigger recorded in ADR-0030 |
| R-3 | `Patient` shadow/implicit FK delete behavior differs from assumption | S4 model-assertion tests pin the actual behavior per FK; handler pre-checks make runtime behavior explicit regardless |
| R-4 | Validator/Domain message drift (two layers, Arabic text) | Single normative table (§5); validator tests assert key messages |
| R-5 | PartnerLab list-type expectation mismatch with M13 | Accept any existing list in M14; non-enforcement documented in ADR-0030 and the handoff |
| R-6 | Placeholder wording/localization divergence | Frozen literals + unit tests; consumers own rendering-time localization |
| R-7 | Scope creep into settings or reporting aggregates | Hard boundary: no setting writes, no patient/report writes; deferred flag question carried in the handoff for the M02 owner |
| R-8 | Schema drift discovered late in S4 | Verification-first ordering; narrow migration gate with rollback proof if triggered |

## 7. Edge Cases

| ID | Edge case | Expected behavior |
|---|---|---|
| EC-01 | Create referral/contract without `PriceListId` | Rejected (validator + Domain) |
| EC-02 | Create doctor with `PriceListId` | Rejected (existing guard retained) |
| EC-03 | Update doctor → referral without supplying a list | Rejected; transitions re-validated |
| EC-04 | Update referral → doctor while a list is present | Rejected unless the list is cleared in the same update |
| EC-05 | Referral/partner lab with unknown `PriceListId` | `NotFound` |
| EC-06 | Percent `null` / `0` / `100` | Accepted |
| EC-07 | Percent `< 0` / `> 100` | Rejected |
| EC-08 | Name whitespace-only / over 200 chars | Rejected; stored names trimmed |
| EC-09 | Optional contact fields empty/whitespace | Normalized to `null`, never stored as `""` |
| EC-10 | `GenerateEntityIdCode` on missing entity | `NotFound` |
| EC-11 | Generated candidate collides on pre-check | Regenerate, up to 5 attempts, then `Conflict` |
| EC-12 | Race: two concurrent generations pick the same code | Unique-violation catch → retry → `Conflict` on exhaustion |
| EC-13 | Delete entity referenced by `Patient` via either FK | `Conflict` naming the patient linkage |
| EC-14 | Delete entity referenced by `SentOutSample` | `Conflict` (mirrors `Restrict`) |
| EC-15 | Delete entity referenced only by `CashMovement` | Succeeds; storage nulls the FK (`SetNull`) |
| EC-16 | Delete entity whose price list was deleted earlier | Succeeds; `PriceListId` already nulled (`SetNull`) |
| EC-17 | Search with unknown `EntityType` value | Validator rejects; no unhandled cast |
| EC-18 | Search with null/empty term | Treated as no text filter, not a failure |
| EC-19 | Resolver with whitespace-only referral name | Treated as empty → sex-based default |
| EC-20 | `RegenerateIdCode` empty / over 50 chars | Domain rejects; handler maps to failure |
| EC-21 | Price list deleted by M13 while referral entities reference it (`SetNull` nulls their `PriceListId`) | Reads unaffected; the next `Update` on such a row re-validates and rejects until a replacement list is assigned (self-healing); M13 is notified via the handoff to consider blocking or reassigning on list deletion |

## 8. Explicitly NOT Modified

- `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` — already supports `ExternalEntity` (verify-only).
- `src/TopLab.Domain/Settings/SystemSettings.cs` and any settings configuration/seed — no new setting, no value change.
- `src/TopLab.Domain/Billing/PriceList.cs` and any M13 surface — existence-checks only.
- `src/TopLab.Domain/Patients/Patient.cs`, `SentOutSample`, `CashMovement` — read for guards only.
- `src/TopLab.Application/DependencyInjection.cs` — assembly-wide validator scan already covers new validators (verified line 21).
- `Directory.Packages.props` — no new packages.
- Any migration file — unless the S4 scope gate proves a gap.
- Anything Presentation-related — nothing in this plan touches it.

---

## Finality Statement

**This plan is FINAL and ready for immediate execution.**

- The five settled business rules (§2, items 1–5) are encoded as fixed facts in Slices S1–S4 and are not open points.
- Every design element was verified against the code at commit `e3fa0832abba7dde3e2592415f5e57c3360037f7` and against the reference PDF: the Create-only aggregate, the complete F5 baseline schema (no migration needed), the FK delete-behavior landscape (`Restrict` / `SetNull` / implicit `Patient` FKs), the `EDIT_SYSTEM_SETTINGS` gate, the assembly-wide validator scan, the unique-violation catch precedent, the `Sex` enum, and the discrete ID-generation workflow.
- No additional open point requiring resolution before execution was identified. All known residual questions are either consumer-side concerns carried to downstream owners via the S5 handoff (the M02 flag-link question; M07 rendering; M13 orphan handling) or fully contained within slice specifications with both outcomes pre-decided (S4 migration-scope gate; ADR-number fallback).

Slices may start in order: **S1 → S2 → S3 → S4 → S5.**
