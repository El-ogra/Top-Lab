# Loop Engineering — Memory File

- **Module:** Price Lists, Comments & Custom Groups (M-13)
- **Module Number:** M-13
- **Source Plan:** Docs/OpenCode/M-13.md
- **Date Created:** 2026-09-07
- **Total Slices:** 4
- **Current Slice:** Slice 2 complete — 2/4 slices done
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Implements the lifecycle, maintenance, and read surface for the three M-13 reference-data concepts (Price Lists, Test Comments, Custom Groups) on top of the M-12 catalog. Domain behaviors + guards (rename, item add/update/remove, price >= 0, text max-1000), Application read surface (DTOs, queries, fake-context extensions), Application write surface (12 IAuthorizedRequest commands, validators, feature-local `DomainFailureTranslator` translating `ArgumentException` paramNames to frozen Arabic messages), and an Infrastructure slice that runs the model-vs-snapshot zero-drift gate against the F5 baseline (no new migration expected) and pins the FK / delete-behavior matrix including four negative "no-FK" assertions. Done means: build 0/0, full test suite green, ADR-0030, tracking-sheet flip, and `Handoff_M13.md` produced.

## Global Validation Gates

- Gate G0 (pre-execution): `dotnet build TopLab.sln` passes zero errors + zero warnings; `dotnet test TopLab.sln` passes 100% (full suite, not just affected tests).
- Gate G1 (post-execution per slice): same as G0 plus the slice-specific gate listed in the table below.

## Stop/Continue Rule

After a slice completes (all 10 stages done), verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice, with no pause and no human confirmation required.
If any one fails → retry. If the SAME failure (a specific build error, a specific file-edit failure, a specific test failing to pass, or any other single repeated failure) occurs 5 CONSECUTIVE times, STOP execution entirely and emit a Stop Report describing exactly what failed, at which slice/stage, and the evidence from each of the 5 attempts. Do NOT proceed past this point without owner review.

Additional user-authorized execution parameters (override skill defaults):
- Stop threshold: 5 consecutive failures for the same reason (not 4).
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/inspection) replaces any standard UI journey — M-13 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-13] Slice N/Total: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-13's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain behaviors + tests: build zero/zero; all Domain tests pass; no Application or Infrastructure file touched; guard-text strings frozen in plan bind translator | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Domain.Tests`; grep src+tests for new mutator coverage |
| 2 | VG-02 | Application read surface: build zero/zero; all Application tests pass (queries, DTOs, fake-context extensions, joined DTOs from M12 `Test` set, per-test comment filter) | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Application.Tests` |
| 3 | VG-03 | Application write surface: build zero/zero; all Application tests pass (12 IAuthorizedRequest commands, validators, feature-local translator, two-message duplicate-attach coverage, grep gate: no write missing `IAuthorizedRequest`, grep gate: no handler persists an instance from an aggregate's `Items` collection) | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Application.Tests`; grep gates on src+tests |
| 4 | VG-04 | Infrastructure + close-out: Release build zero/zero; full suite green; FK-matrix model tests green incl. four negative "no-FK" assertions; one EF Core InMemory `PriceListItemPersistenceTests` integration test green; `ApplicationDbContextModelSnapshot.cs` unchanged; coverage floors (Domain ≥ 90%, Application ≥ 80%, Infrastructure ≥ 70% of M-13 footprint) or explicit handoff waivers; ADR-0030 appended; tracking flip for M-13 with dated change-log row; `Handoff_M13.md` produced | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain behaviors and invariants (Domain layer) | [x] Complete | VG-01 |
| 2 | Application read surface (queries + DTOs + fakes) | [x] Complete | VG-02 |
| 3 | Application write surface + validators + auth tests | [ ] Not started | VG-03 |
| 4 | Infrastructure proof + module close-out | [ ] Not started | VG-04 |

---

## Slice 1: Domain behaviors and invariants (Domain layer)

- **Goal:** Turn the four Create-only / constructor-only entities (`PriceList`, `PriceListItem`, `CustomGroup`, `CustomGroupItem`, `TestComment`) into behavioral aggregates with rename / update / item-mutation methods, price/name/text guards (`price >= 0`, name trimmed required max 150, comment text max 1000), and full domain test coverage. Follow the verified `WorkGroupLog` precedent: duplicate-add throws, remove-of-absent throws, guards raise `ArgumentException` with `paramName` set.
- **Touches:** `src/TopLab.Domain/Billing/PriceList.cs` (modify — add `Rename`, `ContainsTest`, `AddItem`, `SetItemPrice`, `RemoveItem`); `src/TopLab.Domain/Billing/PriceListItem.cs` (modify — add `price >= 0` guard, `internal UpdatePrice`); `src/TopLab.Domain/Tests/CustomGroup.cs` (modify — mirror of `PriceList`); `src/TopLab.Domain/Tests/CustomGroupItem.cs` (modify — `price >= 0` guard + `UpdatePrice`); `src/TopLab.Domain/Tests/TestComment.cs` (modify — `MaxCommentTextLength = 1000` constant, `Create` length guard, `Update` mutator); `tests/TopLab.Domain.Tests/Billing/PriceListTests.cs` (create); `tests/TopLab.Domain.Tests/Tests/CustomGroupTests.cs` (create); `tests/TopLab.Domain.Tests/Tests/TestCommentTests.cs` (create)
- **Validation Gate:** VG-01 — build zero/zero over the full solution; all Domain tests pass; every new public method has at least one positive + one negative test; no Application or Infrastructure file touched; guard-text strings frozen in the plan bind the translator in S3.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln` (0 errors, 0 warnings); `dotnet test TopLab.sln` (Domain 201, Application 356, Infrastructure 48 = 605 tests, 0 failures).
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §3.1 + §5 frozen messages; duplicate-add throws `ArgumentException` with `paramName = nameof(testId)`; remove-of-absent throws `ArgumentException("Test is not in the price list.", nameof(testId))` (aligned to verified `WorkGroupLog.RemoveItem` precedent); price-guard message `Price must be >= 0.`; `TestComment.MaxCommentTextLength = 1000` mirrors `ReferenceRange.MaxCommentLength` / `Test.MaxTestCodeLength` precedent.
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: 5 Domain entities + 3 new test files; existing `WorkGroupLog` / `WorkGroupLogItem` (verified aggregate-with-items precedent) read in full; `StronglyTypedId<TValue>`, `AuditableEntity<TId>`, `ValueObject`, `Entity<TId>` Common base types inspected; csproj (`Nullable=enable`, `LangVersion=latest`, `ImplicitUsings=enable`); test conventions (xUnit, namespace `TopLab.Domain.Tests.<Area>`).
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: (1) `PriceList.cs` mutators + guards; (2) `PriceListItem.cs` guard + `UpdatePrice`; (3) `CustomGroup.cs` mirror; (4) `CustomGroupItem.cs` guard + `UpdatePrice`; (5) `TestComment.cs` constant + length guard + `Update`; (6) `PriceListTests.cs`; (7) `CustomGroupTests.cs`; (8) `TestCommentTests.cs`.
- [x] **Stage 5 — Execution:** Slice implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`. Evidence: 0/0 build; Domain 244 (+43), Application 356, Infrastructure 48 = 648 tests, 0 failures.
- [x] **Stage 7 — Validation Gate:** VG-01 passed. Evidence: build/test output; `git status --porcelain` confirms diff confined to `src/TopLab.Domain/{Billing,Tests}/**` + `tests/TopLab.Domain.Tests/**`.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (authorized local):** `[M-13] Slice 1/4: Domain behaviors and invariants (Domain layer) — loop-engineering` + `Stages 1-10 verified. Gate VG-01 passed.` — on `main`, never push.

---

## Slice 2: Application read surface (queries + DTOs + fakes)

- **Goal:** Expose the M-13 read surface over the catalog: list/detail queries for price lists, test comments, and custom groups, with DTOs and a fake-context extension. Reference-shown shapes: price-list detail includes items with test names and prices; custom-group detail includes items with test names and prices; test-comment listing is per-test (the report-entry dropdown shape).
- **Touches:** `src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Common/PriceListDtos.cs` (create — `PriceListSummaryDto`, `PriceListItemDto`, `PriceListDetailDto`); `src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Common/TestCommentDtos.cs` (create — `TestCommentDto`); `src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Common/CustomGroupDtos.cs` (create — `CustomGroupSummaryDto`, `CustomGroupItemDto`, `CustomGroupDetailDto`); `.../Queries/GetPriceLists/` (create — Query + Handler, list summary, `ItemCount` via flat `Set<PriceListItem>()` group-by); `.../Queries/GetPriceListById/` (create — detail with in-memory `.Value` join to M12 `Test` set); `.../Queries/GetTestComments/` (create — optional `int? TestId` filter); `.../Queries/GetCustomGroups/` + `.../Queries/GetCustomGroupById/` (create — mirrors); `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` (extend — `PriceListItems`, `CustomGroups`, `CustomGroupItems` lists + `Set<T>`/Add/Remove branches); `tests/TopLab.Application.Tests/Features/PriceListsCommentsAndCustomGroups/` (create — one handler-test class per query)
- **Validation Gate:** VG-02 — Application build zero/zero; all Application tests pass; every DTO field traced to a verified domain property; no invented fields (e.g., no `IsActive` on price lists); no write commands in this slice.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: `dotnet build TopLab.sln` (0/0); `dotnet test TopLab.sln` (Domain 244, Application 356, Infrastructure 48 = 648 tests).
- [x] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §3.2; reads are unauthorized plain `IRequest<Result<...>>` (M12/M14 precedent); `ItemCount` computed from flat `Set<PriceListItem>()` group-by (not aggregate navigation — port has no `Include`, verified); missing list → `NotFound("قائمة الأسعار غير موجودة.")`; test-name join via `.Value` in-memory join (`SearchTestCatalogQueryHandler` precedent); no trim on the query side (domain trims before storing); expression trees reject `out` var + statement lambdas — fixed by materializing via `.ToList()` before projection (mirrors `GetWorkGroupLogsQueryHandler` pattern).
- [x] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `IApplicationDbContext` (read in full — no `Include` capability), `FakeApplicationDbContext` (read — extended with 3 new sets), `GetWorkGroupLogsQueryHandler` (verified in-memory join pattern), `GetTestByIdQueryHandler` (NotFound pattern), `Result/Error`, `TestCatalogDtos` (DTO conventions), feature folder conventions.
- [x] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: DTOs (`PriceListDtos.cs`, `TestCommentDtos.cs`, `CustomGroupDtos.cs`) -> 5 query files (`GetPriceLists`, `GetPriceListById`, `GetTestComments`, `GetCustomGroups`, `GetCustomGroupById`) -> fake extension (3 new lists + typeof branches + Add/Remove branches) -> 5 query test classes.
- [x] **Stage 5 — Execution:** Slice implemented per plan.
- [x] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`. Evidence: Domain 244, Application 372 (+16 new), Infrastructure 48 = 664 tests.
- [x] **Stage 7 — Validation Gate:** VG-02 passed. Evidence: build/test output; diff confined to `src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/**` + tests + memory.
- [x] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (authorized local):** `[M-13] Slice 2/4: Application read surface (queries + DTOs + fakes) — loop-engineering` + `Stages 1-10 verified. Gate VG-02 passed.` — on `main`, never push.

---

## Slice 3: Application write surface + validators + auth tests

- **Goal:** Implement the 12 M-13 write commands (create/rename/delete price list, set/remove priced items, create/update/delete fixed comments, create/rename/delete custom group, set/remove priced group items) with FluentValidation validators, one authorization theory class over the 12 write commands (`RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"`), and a feature-local `DomainFailureTranslator` translating paramNames to frozen Arabic messages. Apply the mandated item-mutation protocol (aggregate-as-invariant-checker + flat-set persistence) to avoid the double-tracking hazard at SaveChanges.
- **Touches:** `src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/Commands/CreatePriceList/` + `RenamePriceList/` + `DeletePriceList/` + `SetPriceListItemPrice/` + `RemovePriceListItem/` (each with Command/Handler/Validator); `.../Commands/CreateTestComment/` + `UpdateTestComment/` + `DeleteTestComment/` (each with Command/Handler/Validator); `.../Commands/CreateCustomGroup/` + `RenameCustomGroup/` + `DeleteCustomGroup/` + `SetCustomGroupItemPrice/` + `RemoveCustomGroupItem/` (each with Command/Handler/Validator); `.../Common/DomainFailureTranslator.cs` (create — M14 paramName+fragment style); `tests/TopLab.Application.Tests/Features/PriceListsCommentsAndCustomGroups/` (create — handler + validator tests + one authorization theory class)
- **Validation Gate:** VG-03 — build zero/zero; all Application tests pass (12 commands, validators, auth); every §2 rule exercised by at least one named test; grep gate: no write command missing `IAuthorizedRequest`; grep gate: no handler persists an instance obtained from an aggregate's `Items` collection (double-tracking guard).

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §3.3 + §5 frozen messages; all handlers implement `IAuthorizedRequest` with `EDIT_SYSTEM_SETTINGS`; mandatory item-mutation protocol (load header -> exist-check `TestId` -> pre-check flat row -> aggregate method inside try/catch ArgumentException -> translator -> persist idempotently against flat set -> SaveChanges); `DeletePriceList` guards on `ExternalEntity.PriceListId` references -> `Conflict("تعذر حذف قائمة الأسعار لارتباطها بجهات خارجية.")`; multiple comments per test allowed (no uniqueness check); save-time `IsReferenceConflict(ex)` catch -> `Error.Conflict` (M14 precedent).
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: 12 command use-case folders + 12 validators + `DomainFailureTranslator` + `CreateExternalEntityCommand` / `DeleteExternalEntityCommand` (M14 reference handlers) + authorization test conventions.
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: commands -> validators -> translator -> DI wiring (no change — `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` already discovers M-13 validators) -> handler unit tests -> auth theory class -> grep gates.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-03 passed. Evidence: build/test output; grep gates clean.
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-13] Slice 3/4: Application write surface + validators + auth tests — loop-engineering` + `Stages 1-10 verified. Gate VG-03 passed.` — on `main`, never push.

---

## Slice 4: Infrastructure proof + module close-out

- **Goal:** Run the migration-scope zero-drift gate against the F5 baseline (expected: zero drift; any drift triggers plan revision, not silent migration). Pin the FK / delete-behavior matrix with model-assertion tests including four negative "no-FK" assertions. Add one EF Core InMemory `PriceListItemPersistenceTests` integration test executing the mandated item-mutation protocol end-to-end and asserting no double-tracking at SaveChanges. Extend `ValidatorRegistrationTests` for the 12 new validators. Record ADR-0030, flip the M-13 row on the tracking sheet, and produce `Handoff_M13.md`.
- **Touches:** `tests/TopLab.Infrastructure.Tests/Persistence/Configurations/F5ConfigurationTests.cs` (extend — mapping assertions for `PriceList`, `PriceListItem`, `CustomGroup`, `CustomGroupItem`, `TestComment`); `tests/TopLab.Infrastructure.Tests/Persistence/PriceListCustomGroupDeleteBehaviorTests.cs` (create — Cascade / SetNull / Restrict / four negative no-FK assertions); `tests/TopLab.Infrastructure.Tests/Persistence/PriceListItemPersistenceTests.cs` (create — EF Core InMemory end-to-end: set-price inserts one row, set-price again updates same row, remove deletes it, no double-tracking exception); `tests/TopLab.Infrastructure.Tests/Persistence/ValidatorRegistrationTests.cs` (extend — resolve all 12 new validators from a host built like `App`); `Docs/Source/Top_Lab_ADR.md` (append ADR-0030 — confirm max ADR at execution); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M-13 row to 🟩 Done + dated change-log row); `Docs/Handoff_M13.md` (create per template)
- **Validation Gate:** VG-04 — Release build zero/zero; full suite green (`dotnet test TopLab.sln -m:1`); FK-matrix tests green incl. four negative assertions; `PriceListItemPersistenceTests` green; `ApplicationDbContextModelSnapshot.cs` unchanged (or addendum executed per gate); coverage floors per `Top_Lab_Test_Strategy.md` §4 (Domain ≥ 90%, Application ≥ 80%, Infrastructure ≥ 70% of M-13 footprint) or explicit handoff waivers; ADR-0030 + handoff + tracking flip committed per convention.

### 10-Stage Progress

- [ ] **Stage 1 — Pre-Execution Verification:** Build passes `zero errors + zero warnings` and all tests pass. Evidence: run `dotnet build TopLab.sln` + `dotnet test TopLab.sln`.
- [ ] **Stage 2 — Deep Understanding:** Requirements, inputs, outputs, edge cases documented. Notes: plan §3.4 + §6 / §7 / §9; migration-scope gate is an input hypothesis that must be proven — "no migration expected" is not the proof; FK matrix incl. negative assertions: `PriceListItem → Test` ✗, `CustomGroupItem → Test` ✗, `CultureAntibioticAttachment → Test` ✗, `CultureAntibioticAttachment → Antibiotic` ✗; the double-tracking regression test pins the M-13-S3 protocol; the handoff carries the orphan-risk note to M12 (if a test-delete ever appears) and M02 owners.
- [ ] **Stage 3 — File Analysis:** Every file this slice touches listed and inspected. Files: `20260828052248_BaselineDataModel.cs` (read in full — creates all M-13 tables), `ApplicationDbContextModelSnapshot.cs` (relevant sections read in full), `TestDeletionCascadeTests.cs` + `ExternalEntityDeleteBehaviorTests.cs` (precedent), `InMemoryContextFactory.cs` + `TestApplicationDbContext.cs` (harness), `Top_Lab_ADR.md`, `Top_Lab_Master_Tracking_Sheet.md`, `Top_Lab_Handoff_Template.md` / `Handoff_M12.md` (template).
- [ ] **Stage 4 — Planning:** Step-by-step execution plan written. Plan: run migration-scope gate first (config vs snapshot; expect zero drift) -> configs -> EF config tests -> FK matrix incl. negative assertions -> InMemory item-persistence test -> validator registration test -> ADR-0030 -> tracking flip -> handoff.
- [ ] **Stage 5 — Execution:** Slice implemented per plan.
- [ ] **Stage 6 — Post-Execution Verification:** Build + tests pass again `zero errors + zero warnings`.
- [ ] **Stage 7 — Validation Gate:** VG-04 passed. Evidence: Release build 0/0, full suite green, FK-matrix incl. negative assertions green, `PriceListItemPersistenceTests` green, snapshot unchanged (or addendum executed per gate), coverage floors met or waivers documented.
- [ ] **Stage 8 — Documentation Update:** Every checkbox in this slice marked [x] where applicable.
- [ ] **Stage 9 — Memory Status Update:** "Current Status" section updated; module close-out recorded.
- [ ] **Stage 10 — Git Commit (authorized local):** `[M-13] Slice 4/4: Infrastructure proof + module close-out — loop-engineering` + `Stages 1-10 verified. Gate VG-04 passed.` — on `main`, never push.

---

## Current Status

- Overall: 2/4 slices done
- Slice 1 — Domain behaviors and invariants (Domain layer): [x] Complete (VG-01 passed; commit a92d238)
- Slice 2 — Application read surface (queries + DTOs + fakes): [x] Complete (VG-02 passed)
- Slice 3 — Application write surface + validators + auth tests: [ ] Not started
- Slice 4 — Infrastructure proof + module close-out: [ ] Not started

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-07 | 0 | — | Memory file created | OK | — |
| 2026-09-07 | 1 | 10 | Slice 1 committed (Domain behaviors + tests, VG-01) | OK | a92d238 |

## Stop Report (append only if a stop condition triggers)
