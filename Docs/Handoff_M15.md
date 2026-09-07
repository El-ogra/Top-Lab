# Top-Lab — Handoff Document M15

## نظام توب لاب — تسليم جلسة عمل (Module 15 — Culture & Antibiotic Configuration)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-07_M15_culture-antibiotic-configuration` |
| Session date (UTC) | 2026-09-07 |
| Session start (UTC) | 2026-09-07 |
| Session end (UTC) | 2026-09-07 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M15 |
| Module name | Culture & Antibiotic Configuration |
| Wave | 3 |
| Feature folder(s) touched | `src/TopLab.Domain/Tests/Antibiotic.cs`, `src/TopLab.Application/Features/CultureAndAntibiotics/` (new), `tests/TopLab.Application.Tests/Common/Fakes/FakeApplicationDbContext.cs` (extended), `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extended) |
| Layers touched | Domain / Application / Infrastructure (tests + close-out) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `e7ca616` |
| Final commit at session end | `(filled at commit time)` |

---

## 2. Session Objective (Required)

Implement Module 15 **Culture & Antibiotic Configuration** end-to-end in the three slices S1–S3 of `Docs/OpenCode/M-15.md` (Domain behaviors + display-filter contract → Application read + write surface → Infrastructure proof + close-out), satisfying FR-M15-001…004 and BR-12. This covers the `Antibiotic.Update` mutator (name guard + freely editable flags — confirmed D4-a), the pure static `CultureAntibioticDisplay` resolver implementing BR-12 union semantics (confirmed D5-a, `ChildAgeThresholdYears = 12`), the read/write Application surface (2 queries + 5 `IAuthorizedRequest` commands + 5 validators + feature-local `DomainFailureTranslator`, `EDIT_SYSTEM_SETTINGS` authorization), the D6-a two-command manual-entry sequence (no composite `CreateAndAttachAntibiotic` command), antibiotic delete guards on both `CultureAntibioticAttachment` rows (no DB FK, application-level) and `CultureAntibioticResult` rows (DB-level Restrict, forward-safe) plus the save-time `IsReferenceConflict` catch, the FK matrix with two negative "no-FK" assertions on `CultureAntibioticAttachment`, and ADR-0031. Build must be 0 errors / 0 warnings (Debug + Release); all tests green; no new migration (zero drift against the F5 baseline). Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain behaviors + display-filter contract** — Implementation Complete — `src/TopLab.Domain/Tests/Antibiotic.cs` (+ `Update(string name, bool isPregnancyFlagged, bool isChildrenFlagged)` with the same name guard as `Create`; confirmed D4-a: flags freely editable); `src/TopLab.Application/Features/CultureAndAntibiotics/Common/CultureAntibioticDisplay.cs` (pure static class — M14 `ReferralNameResolver` placement precedent — `public const int ChildAgeThresholdYears = 12;` and `public static bool IsDisplayable(bool isPregnancyFlagged, bool isChildrenFlagged, bool isPregnancyIndicated, bool isChildUnder12)` implementing the union formulation); `tests/TopLab.Domain.Tests/Tests/AntibioticTests.cs` (12 tests — Create/Update guards incl. empty/whitespace/trimming, flag mutation both directions, id immutability under Update); `tests/TopLab.Application.Tests/Features/CultureAndAntibiotics/CultureAntibioticDisplayTests.cs` (20 tests — 16-row truth table + threshold pinned at 12 + union/either-context-holds/purity assertions). Commit `0bc2016`; 814 tests green (+32).
- **S2 — Application read + write surface** — Implementation Complete — DTOs (`AntibioticDto`/`AttachedAntibioticDto`/`CultureAntibioticListDto` with `AttachedCount`); 2 unauthorized queries (`GetAntibiotics` with optional trimmed `SearchTerm` ordered by name + `GetCultureAntibiotics(TestId)` with `IsCultureType` enforcement + in-memory `.Value` join); 5 `IAuthorizedRequest` commands with `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"`: `CreateAntibiotic` / `UpdateAntibiotic` / `DeleteAntibiotic` (with both attachment-row + result-row guards + save-time `IsReferenceConflict` catch) / `AttachAntibioticToCulture` (with `IsCultureType` enforcement + duplicate guard) / `DetachAntibioticFromCulture` (no composite `CreateAndAttachAntibiotic` — D6-a). 5 FluentValidation validators; feature-local `Common/DomainFailureTranslator.cs` (paramName + message-fragment → frozen Arabic messages). `FakeApplicationDbContext` extended with `Antibiotics` / `CultureAntibioticAttachments` / `CultureAntibioticResults` lists + `Set<T>()`/`Add`/`Remove` branches. New test fake `ReferenceConflictFakeApplicationDbContext` (mirrors `UniqueViolationFakeApplicationDbContext` for the M14-style `IsReferenceConflict` catch). 49 new tests across 6 files: handler tests (happy + NotFound + Conflict + Validation + translator + D6-a two-command flow + save-time reference-conflict mapping), validator tests, and a 5-row authorization theory class (`CultureAndAntibioticsAuthorizationTests`). Commit `d5d41fb`; 863 tests green (+49).
- **S3 — Infrastructure proof + close-out** — Implementation Complete — `F5ConfigurationTests` extended (2 mapping assertions: `Antibiotic_HasExpectedMapping` — Id identity / column `AntibioticId` / Name max-150 required / both flags required bit; `CultureAntibioticAttachment_HasCompositeKey` — composite `{TestId, AntibioticId}` + zero FKs); `CultureAntibioticDeleteBehaviorTests` (FK matrix: `CultureAntibioticResult → Antibiotic` **Restrict**; `CultureAntibioticResult → CultureResult` **Cascade**; `CultureResult → PatientTest` 1-to-1 **Cascade**; **two negative no-FK assertions** on `CultureAntibioticAttachment → Test` and `→ Antibiotic`); `ValidatorRegistrationTests` extended (5 M-15 validators resolved via `AddApplication()` host); ADR-0031 appended to `Docs/Source/Top_Lab_ADR.md`; M-15 row on the Master Tracking Sheet flipped to 🟩 Done + dated change-log row; this handoff created; zero drift against F5 baseline verified (`ApplicationDbContextModelSnapshot.cs` unchanged since M-12 commit `9758466`).

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Debug and Release).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (Debug, `dotnet test TopLab.sln`): **863 green** = 256 Domain + 544 Application + 63 Infrastructure.
- New tests added: +81 net since M-13 close (782 at session start → 863).
- Tests currently failing: none.

### 4.3 Migrations

- New EF Core migration(s) added: **None**.
- Migration-scope zero-drift gate: ran via `F5ConfigurationTests` (`Antibiotic_HasExpectedMapping`, `CultureAntibioticAttachment_HasCompositeKey`) + `CultureAntibioticDeleteBehaviorTests` (FK matrix incl. 2 negative no-FK assertions) against the existing `ApplicationDbContextModelSnapshot.cs` — no drift; no addendum migration required.
- `ApplicationDbContextModelSnapshot.cs` unchanged since M-12 close (commit `9758466`).
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: None. Validators are auto-discovered by `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()`; `ValidatorRegistrationTests` confirms all 5 M-15 validators resolve through the host.
- Composition-root changes (`App.xaml.cs`): none (no Presentation content anywhere — A6 grep gate holds).

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All three slices reached a terminal state; module closed out in the Master Tracking Sheet (§4/§6/§9) and this handoff.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

- **Decision:** Antibiotic flags (`IsPregnancyFlagged`, `IsChildrenFlagged`) are freely editable via `Antibiotic.Update` (confirmed D4-a, owner-approved).
  - **Reason:** Reference §9-3 shows the flags as ordinary checkboxes on the antibiotic data form with no immutability indication. M15-S1 plan §0.1.1 and §9 record the owner confirmation.
  - **Scope of impact:** `Antibiotic.Update(name, isPregnancyFlagged, isChildrenFlagged)` mutates both flags in place; existing attachments and results survive the correction. `DomainTests.Tests.AntibioticTests` covers both flag-clear and flag-toggle directions.
  - **Follow-up required:** No.
- **Decision:** Both-flags-set display semantics = union (confirmed D5-a, owner-approved).
  - **Reason:** Plan §9 records the owner confirmation of the union formulation. The 12-year threshold is a named constant per reference §9-3 and PRD FR-M15-004.
  - **Scope of impact:** `CultureAntibioticDisplay.IsDisplayable` static method: unflagged = displayable for all; single flag = displayable only when that condition holds; both flags = displayable when **either** condition holds.
  - **Follow-up required:** No (pinned by the 16-row truth table in `CultureAntibioticDisplayTests`).
- **Decision:** Manual antibiotic entry inside the attach flow is a two-command sequence (confirmed D6-a — no composite command).
  - **Reason:** Reference §9-3's manual-entry flow is ambiguous; owner explicitly approved the two-command sequence. The conditional `CreateAndAttachAntibiotic` row in M15-S2's file table is cancelled; write-command count is fixed at **5**.
  - **Scope of impact:** `CreateAntibioticCommandHandler` followed by `AttachAntibioticToCultureCommandHandler`. `CreateThenAttachFlowTests` covers the sequence and the standalone-create case (created-but-unattached intermediate is harmless, visible, and retryable).
  - **Follow-up required:** No.
- **Decision:** Culture-ness is enforced via `Test.IsCultureType` (no schema change, no separate culture table).
  - **Reason:** The reference's fixed-ID-slot 118–139 + file-copy at `D:\real lab system\Data` are explicitly rejected as legacy mechanics; the user-extensible, in-app culture test under the `CULTURE AND SENSITIVITY` group matches PRD FR-M13-001 / FR-M15-001. The flag exists precisely for this gate (verified — `Test.Create(isCultureType = true)`).
  - **Scope of impact:** `AttachAntibioticToCultureCommandHandler` and `GetCultureAntibioticsQueryHandler` reject non-culture tests with `Validation("التحليل المحدد ليس مزرعة.")`. The flag is **create-time-only** settable (verified — no `isCultureType` parameter in `Test.Update`); a mis-flagged test must be deactivated and recreated via M-12's lifecycle. M-15 does not reopen catalog CRUD.
  - **Follow-up required:** No.
- **Decision:** No new migration; zero drift against the F5 baseline.
  - **Reason:** All three M-15 tables (`Antibiotics`, `CultureAntibioticAttachments`, `CultureAntibioticResults`) already exist from the baseline migration `20260828052248_BaselineDataModel.cs` (verified line-level). The model-vs-snapshot gate confirms no drift.
  - **Scope of impact:** None — only configuration assertions + integration tests added; `ApplicationDbContextModelSnapshot.cs` unchanged.
  - **Follow-up required:** No.
- **Decision:** Resolver placement in Application-Common (`src/TopLab.Application/Features/CultureAndAntibiotics/Common/CultureAntibioticDisplay.cs`).
  - **Reason:** M14 `ReferralNameResolver` precedent (a static class in the feature's Application `Common` folder, verified). Corrected vs the M15 initial draft's Domain location.
  - **Scope of impact:** The resolver ships as a pure static method, with no DI registration and no Domain dependency on the Application layer.
  - **Follow-up required:** No.

---

## 7. Open Issues, Bugs and Risks (Required — mark "None" if none)

- **Symptom:** `CultureAntibioticAttachment` has **no FK to either `Test` or `Antibiotic`** in the F5 baseline (verified — no FK in the snapshot, in the migration, or in the EF configuration; the configuration file's comment states `"FK via convention (removed explicit HasOne to avoid shadow)"`).
- **Reproduction steps:** Inspect `CultureAntibioticAttachmentConfiguration.cs` and `CultureAntibioticDeleteBehaviorTests.CultureAntibioticAttachment_HasNoRelationshipToTest` / `CultureAntibioticAttachment_HasNoRelationshipToAntibiotic`.
- **Suspected cause / area of code:** Pre-existing baseline design decision (not introduced by M-15).
- **Severity:** Low for M-15 (the M-15 Application handlers existence-check `TestId` and `AntibioticId` on every attach/detach op; the negative model assertions pin the asymmetry so any silent schema change is caught). A future module that introduces test or antibiotic hard-delete without application guards could orphan `CultureAntibioticAttachment` rows.
- **Suggested next investigation step:** Add the missing `Test → CultureAntibioticAttachment` / `Antibiotic → CultureAntibioticAttachment` cascade or restrict FKs in the module that first introduces a hard-delete path (under its own migration). Carries to the M-12 owner (if a test hard-delete is ever introduced) and the M-06 owner (consumer of attachment rows).

---

## 8. Deviations and Waivers (Required — mark "None" if none)

- **Convention departed from:** Plan §3.3 anticipated `ValidatorRegistrationTests` at `tests/TopLab.Infrastructure.Tests/Persistence/`; verified file lives at `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (the M-12 close-out added it there as a DI-host regression test; the M-13 close-out further extended it for the M-13 validators).
  - **Nature:** Extended the existing Application.Tests file in place (consistent with M-12/M13 precedent); the validator registration itself is an Application concern, so the location matches the file's existing scope. Added a `HostBuiltLikeApp_ResolvesM15Validators` theory class covering all 5 new validators.
  - **Justification:** The plan's path was incorrect; this deviation corrects it (mirrors the same correction recorded in the M-13 handoff §8).
  - **Temporary?** No — the file lives at the correct location going forward.

---

## 9. Pending Reviews and Audits (Required)

- **Code review status:** Not started (local-only commits; no reviewer assigned).
- **Audit acceptance status:** Not started.
- **Blocking findings from review or audit:** none.

---

## 10. Next Session Objective (Required)

- **Most important task:** Begin the next module per the dependency map. The natural successor after M-15 is **M-02 — Patient Registration & Test Ordering** (depends on M-12, M-13, M-14, M-22, all now ✅), or **M-06 — Culture & Sensitivity Result Entry** (the consumer of M-15's `CultureAntibioticDisplay` resolver — see §11 for the precise patient-context recipe M-06 needs).
- **Prerequisites:** M-15 requires a Presentation layer to expose the antibiotic catalog and culture-attachment screens (out of M-15 scope, no UI work shipped). When the Presentation layer is built, it consumes the M-15 queries/commands already in place.
- **Expected end-state:** The next module reads `Docs/Handoff_<module>.md`, the Master Tracking Sheet `M15` row (should read 🟩 Done), and `Docs/OpenCode/M-<next>.md` exit criteria to verify, then proceeds with its own slice plan.

---

## 11. Required Reading Before Continuing (Required)

- Coding Standards & Conventions.
- Architecture & Folder Structure Blueprint.
- Data Model / Database Schema Blueprint — §4 `Tests` (IsCultureType) + §4 (Antibiotic / CultureAntibioticAttachment / CultureAntibioticResult — no migration introduced by M-15; existing F5 baseline confirmed).
- Product Requirements Document — §M15 (FR-M15-001…004, BR-12).
- Test Strategy & Audit Acceptance Criteria — coverage-floor rules.
- Module Dependency & Execution Order Map — M-15 relations (M12, M06 consumer).
- Master Tracking Sheet — §4 row M15 and §9 change log.
- ADR-0031 (this module), ADR-0028 (TestCode), ADR-0029 (Test/TestGroup lifecycle), ADR-0030 (close), ADR-0011 (one config per entity via Fluent API), ADR-0018 (close).
- Prior handoff documents for the same module: none (first delivery).
- In-repo execution record: `Docs/OpenCode/M-15.md`, `Docs/OpenCode/M-15-memory.md`.

### 11.1 M-06 consumer note (for the result-entry module owner)

The pure static `CultureAntibioticDisplay.IsDisplayable(bool isPregnancyFlagged, bool isChildrenFlagged, bool isPregnancyIndicated, bool isChildUnder12)` resolver in `src/TopLab.Application/Features/CultureAndAntibiotics/Common/` is the **single source of truth** for BR-12 display filtering at result entry. M-06 supplies the two patient-context booleans:

- `isChildUnder12`: derived from `Patient.AgeValue` and `Patient.AgeUnit` per BR-04 (no unit conversion exists per the enum's doc-comment, verified):
  - `AgeUnit.Year` → `AgeValue < CultureAntibioticDisplay.ChildAgeThresholdYears` (i.e. `< 12`)
  - `AgeUnit.Month` or `AgeUnit.Day` → always `true` (any month/day age is under 12 years)
- `isPregnancyIndicated`: **no pregnancy field exists on `Patient` at this commit** (verified — grep for `pregnan`/`pregnant` in Domain/Application returns no matches outside the `Antibiotic` flags themselves). Its introduction is M-02/M-06's schema decision, not M-15's. Until that field ships, callers can pass `false` and the resolver's union semantics will simply skip the pregnancy branch (PregnancyFlagged antibiotics will display only when the children-context branch holds, when both flags are set).

The 16-row truth-table test in `tests/TopLab.Application.Tests/Features/CultureAndAntibiotics/CultureAntibioticDisplayTests.cs` pins the semantics and is the reference for M-06's expected behavior.

### 11.2 M-12 owner note (for the catalog module owner)

`Test.IsCultureType` is **immutable after creation** (verified — `Test.Update(...)` has no `isCultureType` parameter). A mis-flagged test must be **deactivated and recreated** via M-12's Deactivate/Reactivate + Create lifecycle. M-15's `AttachAntibioticToCultureCommandHandler` enforces the flag; presenting a culture in M-12 (by passing `isCultureType: true` to `CreateTestCommand`) is the only way to attach antibiotics to it. M-15 does **not** reopen catalog CRUD.

---

## 12. Environment and Tooling Notes (Optional)

- .NET SDK 8; EF Core InMemory provider used for the Infrastructure mapping + FK matrix tests (real SQL Server not required for M-15; zero migration, no schema drift).
- Commands executed during this session: `dotnet build TopLab.sln` / `dotnet build TopLab.sln -c Release` / `dotnet test TopLab.sln` / `dotnet test TopLab.sln -c Release`; `git status --porcelain`; `git diff` for each slice; `git log --oneline` to verify the prior slice's commit hash before committing the next.
- **Tooling gotcha (notable):** The `Antibiotic` entity's `Name` parameter is non-nullable in the record commands, but the compiler still emits `CS8604` warnings when `request.Name` is passed to `Antibiotic.Create` / `Antibiotic.Update` if the local variable is null-coalesced via `request.Name?.Trim() ?? string.Empty` — the compiler sees `string?` after `?.` and refuses to flow non-nullability into the `string` parameter. Fixed by trimming via a local non-nullable `string` (e.g. `var trimmed = request.Name.Trim();`) and passing the trimmed local to the Domain method. No warnings in the final build (0/0).

---

## 13. Artifacts Produced (Required — mark "None" if none)

- **Name:** M-15 execution memory file — **Location:** `Docs/OpenCode/M-15-memory.md` — **Purpose:** slice-by-slice loop-engineering trace (Stages 1–10 per slice) — **Persistence:** Kept (in-repo working record).

---

## 14. Signature Block (Required)

| Role | Name | Date (UTC) | Confirmation |
|---|---|---|---|
| Outgoing agent | Local coding agent (Top-Lab) | 2026-09-07 | I confirm this handoff document accurately reflects the state of the work at session end. |
| Reviewer (if any) | TBD |  | I have reviewed this handoff for completeness. |
| Incoming agent (on acceptance) | TBD |  | I confirm I have read and understood this handoff and accept it as my starting context. |

---

## 15. Attachments (Optional)

- `Docs/OpenCode/M-15.md` — execution plan with exit criteria.
- `Docs/OpenCode/M-15-memory.md` — living loop-engineering trace (per-slice Stages 1–10 evidence).
- `Docs/Source/Top_Lab_ADR.md` — ADR-0031.

---

*End of handoff document.*