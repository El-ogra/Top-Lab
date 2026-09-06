# Loop Engineering — Memory File

- **Module:** External Entities (Treating Doctor / Referral or Contract Entity / Partner Lab) — M-14
- **Module Number:** M-14
- **Source Plan:** Docs/OpenCode/M-14.md
- **Date Created:** 2026-09-06
- **Total Slices:** 5
- **Current Slice:** 5 — all slices executed, module closed pending deferred docs + owner commit confirmation
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out in-run per explicit owner authorization; no git operations at any stage per owner constraint)

---

## Module Summary

Full lifecycle management for the single-table, type-discriminated ExternalEntity aggregate: Domain mutators plus per-type price-list invariants, an Application read surface with a pure empty-referral resolver and a write surface with a discrete ID-generation command, and Infrastructure proof with zero schema drift. Done means: S1 Domain behaviors plus tests, S2 reads plus resolver plus tests, S3 writes plus validators plus auth tests, S4 generator plus mapping and FK-matrix proofs, S5 audit gate plus full-suite green plus coverage floors.

## Global Validation Gates

- Gate G0 (pre-execution): `build` passes zero errors + zero warnings, `tests` pass 100%.
- Gate G1 (post-execution per slice): same as G0 plus slice-specific gate below.

## Stop/Continue Rule

After a slice completes (all 10 stages done), verify success via ALL THREE of:
(a) The full solution builds with zero errors and zero warnings.
(b) All existing tests pass (full suite, not just affected tests).
(c) That slice's specific validation gate(s) pass.

If all three hold → proceed immediately to the next slice.
If any one fails → STOP execution and emit a stop report. Do NOT proceed to the next slice.

Additional owner-ordered execution parameters (override skill defaults):
- Stop threshold: 4 consecutive failures for the same failure mode (per stop-conditions.md).
- Execution order: strictly sequential S1 -> S2 -> S3 -> S4 -> S5, no parallel slices, no waiting between slices.
- Stage 7 gate: the plan's textual validation gates (build/test/grep/model-assertion) — M-14 has no UI journey.
- Git: NO git operations at any point (`git add` / `git commit` / `git push` all forbidden). Stage 10 records a drafted commit message only, status pending explicit owner confirmation. Working tree stays uncommitted.
- Docs freeze: only `Docs/OpenCode/M-14-memory.md` (this file) plus plan-required Domain/Application/Infrastructure files may be touched. `Docs/OpenCode/M-14.md` stays unchanged; ADR-0030 append, tracking-sheet flip, and `Docs/Handoff_M14.md` creation are DEFERRED (recorded in S4/S5 blocks, not executed).

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | Domain behaviors: `src/TopLab.Domain` builds zero/zero; new `ExternalEntityTests` plus all Domain tests green; every guard has a negative-path test; Domain ExternalEntities coverage ≥ 90% | `dotnet build src/TopLab.Domain`; `dotnet test tests/TopLab.Domain.Tests`; coverlet module filter |
| 2 | VG-02 | Read surface: `src/TopLab.Application` builds zero/zero; S2 handler/validator/resolver tests green; no query implements `IAuthorizedRequest`; Application S2 footprint coverage ≥ 80% | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep `IAuthorizedRequest` under `Features/ExternalEntities/Queries` returns zero |
| 3 | VG-03 | Write surface: Application builds zero/zero; all S3 handler/validator/authorization tests green incl. unique-violation catch path via `UniqueViolationFakeApplicationDbContext`; grep gate confirms no create-path input accepts `GeneratedIdCode`; Application S3 footprint coverage ≥ 80% | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep `GeneratedIdCode` on command inputs |
| 4 | VG-04 | Infrastructure: solution builds zero/zero; new Infra tests green (4-side FK matrix, no-unique-index assertion, generator/validator resolution); `dotnet ef migrations list` shows nothing new; `ApplicationDbContextModelSnapshot.cs` unchanged; ADR-0030 DEFERRED per docs freeze (recorded, not executed) | `dotnet build TopLab.sln`; `dotnet test tests/TopLab.Infrastructure.Tests`; `dotnet ef migrations list`; `git status --porcelain` shows snapshot unmodified |
| 5 | VG-05 | Close-out: Release build zero/zero; full suite green; coverage floors Domain ≥ 90 / Application ≥ 80 / Infrastructure ≥ 70 (or waivers noted); audit-interceptor gate passes; tracking-sheet/ADR-polish/handoff DEFERRED per docs freeze (recorded, not executed) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverlet per-project report |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Domain behaviors and invariants | [x] Done | VG-01 |
| 2 | Application read surface + referral-name resolver | [x] Done | VG-02 |
| 3 | Application write surface + validators + ID-generator port | [x] Done | VG-03 |
| 4 | Infrastructure proof: generator, mappings, FK matrix, ADR-0030 | [x] Done | VG-04 |
| 5 | Hardening, documentation, module close-out | [x] Done | VG-05 |

---

## Slice 1: Domain behaviors and invariants

- **Goal:** Extend ExternalEntity from Create-only to full lifecycle with per-type price-list guards, uniform percent guard, ID-code regeneration, and string normalization, all covered by domain tests.
- **Touches:** src/TopLab.Domain/ExternalEntities/ExternalEntity.cs (modify); tests/TopLab.Domain.Tests/ExternalEntities/ExternalEntityTests.cs (create)
- **Validation Gate:** VG-01 — Domain build zero/zero; all Domain tests pass; every guard negative-pathed.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** PASS — full solution build 0 Warning(s)/0 Error(s); full suite 487/487 (Domain 169, Application 280, Infrastructure 38). HEAD e3fa083 matches plan assumptions; ExternalEntity.cs Create-only verified.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1; FR-M14-002/003 per-type rules; EC-01/02/03/04/06/07/08/09/19/20. Domain guards English ArgumentException (repo precedent); Arabic messages live in Application layer.
- [x] **Stage 3 — File Analysis:** ExternalEntity.cs (89 lines, Create-only, 2 guards); zero existing `ExternalEntity.Create` call sites in src/tests (breaking Referral guard has no blast radius); xUnit conventions from TestTests.cs; MaxNameLength const follows Test.MaxTestCodeLength precedent.
- [x] **Stage 4 — Planning:** Shared private guards (RequireName, ValidatePriceListRule, ValidatePercent, Normalize) used by both Create and Update; Update re-validates all rules; RegenerateIdCode length 50; normalization whitespace-to-null.
- [x] **Stage 5 — Execution:** ExternalEntity.cs extended (MaxNameLength/MaxGeneratedIdCodeLength consts, Update, RegenerateIdCode, Referral-requires-list, percent 0-100, normalization); ExternalEntityTests.cs created (33 cases).
- [x] **Stage 6 — Post-Execution Verification:** Domain build 0/0; Domain tests 201/201 (one test-data fix: InlineData decimal conversion → double theory plus null fact).
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — build zero/zero; 201/201 green; coverlet ExternalEntity.cs line-rate 0.97 ≥ 0.90.
- [x] **Stage 8 — Documentation Update:** Slice checkboxes and evidence recorded here.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (Draft + Human Confirmation):** No git operations per owner constraint. Draft: `[M-14] Slice 1/5: Domain behaviors and invariants — loop-engineering` — Status: blocked by owner constraint, awaiting explicit confirmation — Commit: none (working tree uncommitted).

---

## Slice 2: Application read surface + referral-name resolver

- **Goal:** Expose type-filtered searchable paged entity listing, single-entity detail, by-code lookup for entity identification, and the pure sex-based empty-referral resolver, with handler tests.
- **Touches:** src/TopLab.Application/Features/ExternalEntities/Common/ExternalEntityDtos.cs (create); .../Common/ReferralNameResolver.cs (create); .../Queries/SearchExternalEntities/ (3 files, create); .../Queries/GetExternalEntityById/ (2 files, create); .../Queries/GetExternalEntityByCode/ (2 files, create); tests/.../Features/ExternalEntities/SearchExternalEntitiesQueryHandlerTests.cs, GetExternalEntityByCodeQueryHandlerTests.cs, ReferralNameResolverTests.cs (create)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green; queries carry no `IAuthorizedRequest`.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** PASS — Application build 0/0; Application tests 280/280.
- [x] **Stage 2 — Deep Understanding:** Plan §6 S2; FR-M14-001/005/006; EC-17/18/19/22; open reads; AsNoTracking; no Include chains; no pagination precedent in Application → plain paged list (Skip/Take), no new shared types.
- [x] **Stage 3 — File Analysis:** GetTestById query/handler, Error API, SearchTestCatalog test-seeding conventions, Result accessors; PriceList.Create(id, name) signature. Deviation found: FakeApplicationDbContext lacked PriceLists (plan assumed verify-only) → extended with PriceList list plus Set/Add/Remove branches (M-12 S2 precedent).
- [x] **Stage 4 — Planning:** DTOs -> resolver -> 3 queries+handlers+validator -> 4 test classes (GetById tests in own file — minor file-map addition).
- [x] **Stage 5 — Execution:** 2 DTOs, resolver, Search (query/handler/validator), GetById, GetByCode, 25 test cases; fake extended.
- [x] **Stage 6 — Post-Execution Verification:** Application build 0/0; Application tests 302/302.
- [x] **Stage 7 — Validation Gate:** VG-02 PASS — build zero/zero; 302/302; `IAuthorizedRequest` grep under Queries returns zero; S2 footprint line coverage 304/306 = 0.9935 ≥ 0.80 (added DTO field asserts to lift ListItemDto).
- [x] **Stage 8 — Documentation Update:** Slice checkboxes and evidence recorded here.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (Draft + Human Confirmation):** No git operations per owner constraint. Draft: `[M-14] Slice 2/5: Application read surface + referral-name resolver — loop-engineering` — Status: blocked by owner constraint, awaiting explicit confirmation — Commit: none (working tree uncommitted).

---

## Slice 3: Application write surface + validators + ID-generator port

- **Goal:** Implement Create/Update/Delete/GenerateEntityIdCode commands with per-type rules, Arabic validators, EDIT_SYSTEM_SETTINGS authorization, the generator port, and full handler test matrices.
- **Touches:** .../Common/Interfaces/IEntityIdCodeGenerator.cs (create); .../Commands/CreateExternalEntity/, UpdateExternalEntity/, DeleteExternalEntity/, GenerateEntityIdCode/ (3 files each, create); tests/.../Features/ExternalEntities/*CommandHandlerTests.cs (4 files, create); .../ExternalEntitiesAuthorizationTests.cs (create)
- **Validation Gate:** VG-03 — Application build zero/zero; S3 tests green incl. catch path; create-path `GeneratedIdCode` grep gate clean.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** PASS — Application build 0/0; Application tests 302/302 (S2 Stage 6 evidence, no slice files touched since).
- [x] **Stage 2 — Deep Understanding:** Plan §6 S3; delete-guard matrix; pre-check plus catch plus 5-attempt retry; create-then-generate; EC-01/02/03/04/05/10/11/12/13/14/15/16; auth-theory plus forbidden plus bypass pattern from M12 auth tests; ThrowOnce wrapper pattern for catch-path test.
- [x] **Stage 3 — File Analysis:** CreateTest command/validator/handler, auth-test file (full pattern), UniqueViolationFake (fixed TestCode message → handler matches UNIQUE/duplicate generically), Patient/SentOutSample/CashMovement Create signatures, Result API. One compile fix during Stage 5 (missing using in Create handler).
- [x] **Stage 4 — Planning:** Port -> DomainFailureTranslator -> 4 commands -> 4 validators -> 6 test classes (validator tests in own file — minor file-map addition).
- [x] **Stage 5 — Execution:** Port, translator, 12 command files, 6 test classes (50 new cases).
- [x] **Stage 6 — Post-Execution Verification:** Application build 0/0; Application tests 352/352.
- [x] **Stage 7 — Validation Gate:** VG-03 PASS — build zero/zero; 352/352; create-input `GeneratedIdCode` grep returns zero; M14 Application footprint line coverage 800/824 = 0.9709 ≥ 0.80.
- [x] **Stage 8 — Documentation Update:** Slice checkboxes and evidence recorded here.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (Draft + Human Confirmation):** No git operations per owner constraint. Draft: `[M-14] Slice 3/5: Application write surface + validators + ID-generator port — loop-engineering` — Status: blocked by owner constraint, awaiting explicit confirmation — Commit: none (working tree uncommitted).

---

## Slice 4: Infrastructure proof: generator, mappings, FK matrix, ADR-0030

- **Goal:** Ship the Singleton generator implementation plus DI registration, verify (not rebuild) the storage mapping, pin the FK delete-behavior matrix and the no-unique-index rule with model-assertion tests, and extend validator-registration coverage. ADR-0030 append DEFERRED per docs freeze.
- **Touches:** src/TopLab.Infrastructure/Services/SecureEntityIdCodeGenerator.cs (create, new folder); src/TopLab.Infrastructure/DependencyInjection.cs (modify: Singleton registration); src/TopLab.Infrastructure/Persistence/Configurations/ExternalEntityConfiguration.cs (verify only); tests/.../Persistence/ExternalEntityDeleteBehaviorTests.cs (create); tests/.../Persistence/Configurations/F5ConfigurationTests.cs (extend); tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs (extend); Infrastructure DI/generator resolution test (create)
- **Validation Gate:** VG-04 — Solution build zero/zero; new Infra tests green; no new migration listed; snapshot unchanged.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** PASS — solution build 0/0; Infrastructure tests 38/38.
- [x] **Stage 2 — Deep Understanding:** Plan §7 S4; verification-first ordering; FkToPrincipal precedent; 12-char unambiguous alphabet (no 0/O/1/I); Singleton stateless; snapshot proof over ephemeral server (model assertions are deterministic).
- [x] **Stage 3 — File Analysis:** Infrastructure DependencyInjection.cs, ExternalEntityConfiguration.cs, baseline migration lines 394-420, snapshot lines 206-280 + relationship blocks (SentOutSample Restrict confirmed; Patient block has navigations only → zero model-level FKs to ExternalEntity), TestDeletionCascadeTests.cs, ValidatorRegistrationTests.cs, Infra test csproj (no config-memory package → tried AddInMemoryCollection, resolved transitively).
- [x] **Stage 4 — Planning:** Generator -> DI registration -> model tests -> config extension -> validator-reg extension -> resolution test -> migration-scope verification.
- [x] **Stage 5 — Execution:** SecureEntityIdCodeGenerator (new Services/ folder), Singleton registration, ExternalEntityDeleteBehaviorTests (4 facts incl. Patient-has-no-FK documentation), F5 extension (mapping + no-unique-index), validator-reg theory, InfrastructureRegistrationTests. ADR-0030 append SKIPPED per docs freeze (recorded as deferred). Fixes during execution: test-code cast bug; GetColumnType unsupported on InMemory → precision/scale annotations.
- [x] **Stage 6 — Post-Execution Verification:** Solution build 0/0; Infrastructure tests 46/46; Application tests 356/356 (validator-reg extension).
- [x] **Stage 7 — Validation Gate:** VG-04 PASS (code/test portions) — build zero/zero; 46/46 + 356/356; `dotnet ef migrations list` shows only the 3 pre-existing migrations; snapshot `git status` clean; generator line coverage 1.0. ADR-0030 portion DEFERRED per docs freeze — recorded, not failed.
- [x] **Stage 8 — Documentation Update:** Slice checkboxes and evidence recorded here.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated.
- [x] **Stage 10 — Git Commit (Draft + Human Confirmation):** No git operations per owner constraint. Draft: `[M-14] Slice 4/5: Infrastructure proof: generator, mappings, FK matrix — loop-engineering` — Status: blocked by owner constraint, awaiting explicit confirmation — Commit: none (working tree uncommitted).

---

## Slice 5: Hardening, documentation, module close-out

- **Goal:** Prove the whole solution in Release with the full suite plus coverage floors and pass the audit-interceptor gate. Tracking-sheet flip, ADR polish, and handoff creation DEFERRED per docs freeze.
- **Touches:** tests/TopLab.Infrastructure.Tests audit-gate test for entity writes (create); Release build; full suite run; coverlet per-project report. Docs/Source/Top_Lab_Master_Tracking_Sheet.md, Docs/Source/Top_Lab_ADR.md polish, Docs/Handoff_M14.md — DEFERRED per docs freeze (not touched).
- **Validation Gate:** VG-05 — Release build zero/zero; full suite green; coverage floors met or waivers noted; audit gate passes.

### 10-Stage Progress

- [x] **Stage 1 — Pre-Execution Verification:** PASS — Release build 0/0 on rebuild (first Release build emitted 1 transient warning that did not reproduce; no code warning present — second consecutive Release build 0 Warning(s)/0 Error(s)).
- [x] **Stage 2 — Deep Understanding:** Plan §8 S5; coverage floors; interceptor single-writer rule; doc writes (tracking flip, ADR polish, handoff) understood and deliberately SKIPPED per docs freeze.
- [x] **Stage 3 — File Analysis:** AuditableEntitySaveChangesInterceptor, InMemoryContextFactory (interceptor wired via options), TestApplicationDbContext pattern, coverlet collector setup. Audit gate uses the real ApplicationDbContext with InMemory options (no TestApplicationDbContext change needed).
- [x] **Stage 4 — Planning:** Audit-gate test -> Release build -> full suite with coverage -> report (docs deferred).
- [x] **Stage 5 — Execution:** ExternalEntityAuditGateTests.cs (2 facts: create-populates + update-advances/created-untouched). Tracking-sheet flip, ADR-0030 polish, Handoff_M14 creation SKIPPED per docs freeze (recorded as deferred).
- [x] **Stage 6 — Post-Execution Verification:** Release build 0/0; full suite 605/605 (Domain 201 + Application 356 + Infrastructure 48).
- [x] **Stage 7 — Validation Gate:** VG-05 PASS (code/test portions) — Release 0/0; 605/605; footprint coverage Domain-ExternalEntity 0.97 ≥ 0.90, Application-M14 0.9709 ≥ 0.80, Infra generator 1.0 + DI 0.78 ≥ 0.70 (whole-project floors inapplicable: future-module code untested, same posture as M-12 handoff waiver); audit gate 2/2; slopwatch CLI unavailable → manual pass (no disabled tests, no suppressions, catches map to Conflict/Validation, no empty catches). Doc portions DEFERRED per docs freeze — recorded, not failed.
- [x] **Stage 8 — Documentation Update:** Slice checkboxes and evidence recorded here; TestResults artifact dirs removed to keep the tree clean.
- [x] **Stage 9 — Memory Status Update:** "Current Status" section updated; module closed pending deferred docs + owner commit confirmation.
- [x] **Stage 10 — Git Commit (Draft + Human Confirmation):** No git operations per owner constraint. Draft: `[M-14] Slice 5/5: Hardening / close-out (code gates) — loop-engineering` — Status: blocked by owner constraint, awaiting explicit confirmation — Commit: none (working tree uncommitted).

---

## Current Status

- Overall: 5/5 slices done
- Slice 1 — Domain behaviors and invariants: [x] Done
- Slice 2 — Application read surface + referral-name resolver: [x] Done
- Slice 3 — Application write surface + validators + ID-generator port: [x] Done
- Slice 4 — Infrastructure proof: generator, mappings, FK matrix, ADR-0030: [x] Done (ADR-0030 append deferred per docs freeze)
- Slice 5 — Hardening, documentation, module close-out: [x] Done (tracking flip, ADR polish, handoff deferred per docs freeze)
- Slice 5 — Hardening, documentation, module close-out: [x] Done (tracking flip, ADR polish, handoff deferred per docs freeze)

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-06 | 0 | — | Memory file created | OK | — |
| 2026-09-06 | 0 | 1 | G0 baseline: full build 0/0; full suite 487/487 | PASS | — |
| 2026-09-06 | 1 | 1-4 | Pre-verify + understand + analyze + plan | PASS — zero Create call sites; plan fixed | — |
| 2026-09-06 | 1 | 5 | Execution: entity + 33-case test class | PASS (1 test-data fix: decimal InlineData) | — |
| 2026-09-06 | 1 | 6-7 | Post-verify + VG-01 | PASS — build 0/0; 201/201; coverage 0.97 | — |
| 2026-09-06 | 1 | 8-10 | Docs + status + commit-draft (no git per constraint) | PASS | — |
| 2026-09-06 | 2 | 1 | Pre-verify: Application build 0/0; tests 280/280 | PASS | — |
| 2026-09-06 | 2 | 2-4 | Understand + analyze + plan (no pagination precedent → plain list) | PASS | — |
| 2026-09-06 | 2 | 5 | Execution: DTOs, resolver, 3 queries, 4 test classes; fake plus PriceLists | PASS after fake fix (10 initial failures: fake lacked PriceList support) | — |
| 2026-09-06 | 2 | 6-7 | Post-verify + VG-02 | PASS — build 0/0; 302/302; no query auth; coverage 0.9935 | — |
| 2026-09-06 | 2 | 8-10 | Docs + status + commit-draft (no git per constraint) | PASS | — |
| 2026-09-06 | 3 | 1 | Pre-verify: Application build 0/0; tests 302/302 | PASS | — |
| 2026-09-06 | 3 | 2-4 | Understand + analyze + plan (port, translator, matrices) | PASS | — |
| 2026-09-06 | 3 | 5 | Execution: port, translator, 12 command files, 6 test classes | PASS after 1 compile fix (missing using) | — |
| 2026-09-06 | 3 | 6-7 | Post-verify + VG-03 | PASS — build 0/0; 352/352; create-input grep clean; coverage 0.9709 | — |
| 2026-09-06 | 3 | 8-10 | Docs + status + commit-draft (no git per constraint) | PASS | — |
| 2026-09-06 | 4 | 1 | Pre-verify: solution build 0/0; infra tests 38/38 | PASS | — |
| 2026-09-06 | 4 | 2-4 | Understand + analyze + plan (snapshot proves Patient has no model FK) | PASS | — |
| 2026-09-06 | 4 | 5 | Execution: generator, DI reg, 4 model tests, F5 ext, reg ext, resolution tests | PASS after 2 fixes (test cast; GetColumnType→precision/scale) | — |
| 2026-09-06 | 4 | 6-7 | Post-verify + VG-04 | PASS — build 0/0; infra 46/46; app 356/356; migrations list unchanged; snapshot clean; generator coverage 1.0 (ADR-0030 deferred per freeze) | — |
| 2026-09-06 | 4 | 8-10 | Docs + status + commit-draft (no git per constraint) | PASS | — |
| 2026-09-06 | 5 | 1 | Pre-verify Release (rebuild clean after 1 transient warning) | PASS — Release 0/0 | — |
| 2026-09-06 | 5 | 2-4 | Understand + analyze + plan (real-DbContext audit gate, no TestApplicationDbContext change) | PASS | — |
| 2026-09-06 | 5 | 5 | Execution: audit-gate test (2 facts); docs skipped per freeze | PASS | — |
| 2026-09-06 | 5 | 6-7 | Post-verify + VG-05 | PASS — Release 0/0; 605/605; footprint coverage 0.97/0.9709/1.0+0.78; audit 2/2 (docs deferred per freeze) | — |
| 2026-09-06 | 5 | 8-10 | Docs + status + commit-draft (no git per constraint); TestResults dirs removed | PASS | — |

## Stop Report (append only if a stop condition triggers)
