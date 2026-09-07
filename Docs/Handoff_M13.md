# Top-Lab — Handoff Document M13

## نظام توب لاب — تسليم جلسة عمل (Module 13 — Price Lists, Comments & Custom Groups)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-07_M13_price-lists-comments-custom-groups` |
| Session date (UTC) | 2026-09-07 |
| Session start (UTC) | 2026-09-07 |
| Session end (UTC) | 2026-09-07 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M13 |
| Module name | Price Lists, Comments & Custom Groups |
| Wave | 3 |
| Feature folder(s) touched | `src/TopLab.Domain/Billing/`, `src/TopLab.Domain/Tests/` (`PriceList`, `PriceListItem`, `CustomGroup`, `CustomGroupItem`, `TestComment`), `src/TopLab.Application/Features/PriceListsCommentsAndCustomGroups/` |
| Layers touched | Domain / Application / Infrastructure (tests + close-out) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `e7ca616` |
| Final commit at session end | `(filled at commit time)` |

---

## 2. Session Objective (Required)

Implement Module 13 **Price Lists, Comments & Custom Groups** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-13.md` (Domain behaviors → Application read surface → Application write surface → Infrastructure proof + close-out), satisfying FR-M13-001 … FR-M13-003. This covers the `PriceList`/`PriceListItem`/`CustomGroup`/`CustomGroupItem`/`TestComment` lifecycle (rename, item upsert/remove, price >= 0, comment text <= 1000), the read/write Application surface with `EDIT_SYSTEM_SETTINGS` authorization, FluentValidation, a feature-local `DomainFailureTranslator` (M14 paramName+fragment style), the aggregate-as-invariant-checker + flat-set persistence protocol (with the double-tracking hazard pinned by an EF Core InMemory regression test), the FK matrix with four negative "no-FK" assertions, and ADR-0030. Build must be 0 errors / 0 warnings (Debug + Release); all tests green; no new migration (zero drift against the F5 baseline). Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain behaviors** — Implementation Complete — `src/TopLab.Domain/Billing/PriceList.cs` (+ `Rename`, `ContainsTest`, `AddItem(testId, price)` duplicate-throw + negative-price-throw, `SetItemPrice` upsert, `RemoveItem` absent-throw), `src/TopLab.Domain/Billing/PriceListItem.cs` (constructor `price >= 0` guard + `UpdatePrice`), `src/TopLab.Domain/Tests/CustomGroup.cs` + `CustomGroupItem.cs` mirrors, `src/TopLab.Domain/Tests/TestComment.cs` (+ `MaxCommentTextLength = 1000` constant, length guard in `Create`, `Update` mutator). Commit `a92d238`; 244 Domain tests green (+43).
- **S2 — Application read surface** — Implementation Complete — DTOs (`PriceListSummaryDto`/`PriceListItemDto`/`PriceListDetailDto`/`TestCommentDto`/`CustomGroupSummaryDto`/`CustomGroupItemDto`/`CustomGroupDetailDto`), 5 unauthorized queries (`GetPriceLists`/`GetPriceListById`/`GetTestComments`/`GetCustomGroups`/`GetCustomGroupById`), `FakeApplicationDbContext` extension with `PriceListItems`/`CustomGroups`/`CustomGroupItems` sets. Commit `1379127`; 372 Application tests green (+16).
- **S3 — Application write surface** — Implementation Complete — 13 `IAuthorizedRequest` commands with `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"`: `CreatePriceList`/`RenamePriceList`/`DeletePriceList`/`SetPriceListItemPrice`/`RemovePriceListItem`; `CreateTestComment`/`UpdateTestComment`/`DeleteTestComment`; `CreateCustomGroup`/`RenameCustomGroup`/`DeleteCustomGroup`/`SetCustomGroupItemPrice`/`RemoveCustomGroupItem`. 13 FluentValidation validators; feature-local `Common/DomainFailureTranslator.cs` (paramName + message-fragment → frozen Arabic messages). `DeletePriceList` blocked by `ExternalEntity.PriceListId` references (settled rule 7, confirmed D1-a). Aggregate-as-invariant-checker + flat-set persistence protocol (the double-tracking hazard pinned by the S4 regression test). Commit `75f5b8d`; 462 Application tests green (+90).
- **S4 — Infrastructure proof + close-out** — Implementation Complete — `F5ConfigurationTests` extended (5 mapping assertions: PriceList/PriceListItem/CustomGroup/CustomGroupItem/TestComment); `PriceListCustomGroupDeleteBehaviorTests` (FK matrix incl. 2 negative no-FK assertions); `PriceListItemPersistenceTests` (EF Core InMemory end-to-end: set-price inserts one row, set-price again updates the same row, remove deletes it, no double-tracking exception at SaveChanges); `ValidatorRegistrationTests` extended (13 new validators resolved via `AddApplication()` host). ADR-0030 appended; Master Tracking Sheet M13 row → 🟩 Done + dated change-log row; this handoff created; Release build 0/0; full suite 782/782. Slice commits `a92d238`/`1379127`/`75f5b8d` + this S4 commit.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Debug and Release).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (Release, `dotnet test TopLab.sln -m:1`): **782 green** = 244 Domain + 475 Application + 63 Infrastructure.
- New tests added: +177 net since baseline (605 at session start → 782).
- Tests currently failing: none.

### 4.3 Migrations

- New EF Core migration(s) added: **None**.
- Migration-scope zero-drift gate: ran via `F5ConfigurationTests` + `PriceListCustomGroupDeleteBehaviorTests` + `PriceListItemPersistenceTests` (all green) against the existing `ApplicationDbContextModelSnapshot.cs` — no drift; no addendum migration required.
- `ApplicationDbContextModelSnapshot.cs` unchanged.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: None. Validators are auto-discovered by `AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()` (introduced in M-12); `ValidatorRegistrationTests` confirms all 13 M-13 validators resolve through the host.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All four slices reached a terminal state; module closed out in the Master Tracking Sheet (§4/§5/§6/§9) and this handoff.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

- **Decision:** Item-mutation mechanics = aggregate-as-invariant-checker + flat-set persistence (confirmed D3-a).
  - **Reason:** The Application port has no `Include` (verified), so EF cannot populate aggregate `_items` collections through the interface. Persisting aggregate-mutated instances alongside fresh flat-row instances risks the double-tracking hazard at `SaveChanges` (verified by the regression test in `PriceListItemPersistenceTests`).
  - **Scope of impact:** All four item-mutating handlers (`SetPriceListItemPrice`/`RemovePriceListItem`/`SetCustomGroupItemPrice`/`RemoveCustomGroupItem`); the aggregate method serves as the price guard only; persistence is purely against the flat set.
  - **Follow-up required:** No (ADR-0030 records the protocol; `PriceListItem.UpdatePrice`/`CustomGroupItem.UpdatePrice` are now `public`).
- **Decision:** Price-list delete is blocked while referenced by `ExternalEntity.PriceListId` (confirmed D1-a).
  - **Reason:** The DB-level `SetNull` would silently null a live `ReferralOrContract` entity's price list — manufacturing domain-invalid entities and silently repricing future patients at walk-in prices.
  - **Scope of impact:** `DeletePriceListCommandHandler`; `Conflict("تعذر حذف قائمة الأسعار لارتباطها بجهات خارجية.")`; save-time `IsReferenceConflict(ex)` re-map covers the race window.
  - **Follow-up required:** No (FK-matrix test pins `SetNull` so any schema change is caught).
- **Decision:** Unique names for price lists and custom groups (confirmed D2-a).
  - **Reason:** Reference-data hygiene; matches `CreateTestGroupCommandHandler` precedent.
  - **Scope of impact:** `CreatePriceList`/`RenamePriceList`/`CreateCustomGroup`/`RenameCustomGroup` handlers — exact match on the trimmed value (no case folding); excludes self on rename.
  - **Follow-up required:** No (Arabic case-insensitivity recorded as a cross-cutting concern in the handoff if ever required).
- **Decision:** No new migration; zero drift against the F5 baseline.
  - **Reason:** All five M-13 tables and the `ExternalEntities.PriceListId` column already exist from the baseline migration `20260828052248_BaselineDataModel.cs` (verified line-level); the model-vs-snapshot gate confirms no drift.
  - **Scope of impact:** None — only configuration assertions + integration tests added.
  - **Follow-up required:** No.
- **Decision:** `RemoveItem`-absent throws (aligned to the verified `WorkGroupLog.RemoveItem` precedent; corrected from the M-13 initial draft's no-op).
  - **Reason:** Consistency with the codebase's only shipped aggregate-with-items implementation.
  - **Scope of impact:** `PriceList.RemoveItem`/`CustomGroup.RemoveItem` throw `ArgumentException` with `paramName = nameof(testId)`; handlers translate to `Error.NotFound` before calling the domain method so the throw is a safety net only.

---

## 7. Open Issues, Bugs and Risks (Required — mark "None" if none)

- **Symptom:** `PriceListItem` and `CustomGroupItem` have no FK to `Test` in the F5 baseline (verified — no FK in the snapshot, in the migration, or in the EF configurations).
- **Reproduction steps:** Inspect `PriceListItemConfiguration.cs`/`CustomGroupItemConfiguration.cs` (comments say "removed explicit HasOne to avoid shadow") and `PriceListCustomGroupDeleteBehaviorTests.PriceListItem_HasNoRelationshipToTest` / `CustomGroupItem_HasNoRelationshipToTest`.
- **Suspected cause / area of code:** Pre-existing baseline design decision, not introduced by M-13.
- **Severity:** Low for M-13 (the M-13 Application handlers existence-check `TestId` on every item op, and the negative model assertions pin the asymmetry so any silent schema change is caught). A future module that introduces test hard-delete without application guards could orphan `PriceListItem`/`CustomGroupItem` rows.
- **Suggested next investigation step:** Add the missing `Test → PriceListItem` / `Test → CustomGroupItem` cascade FKs in the module that first introduces a hard-delete path (under its own migration). Carries to M-12 owners (if a test hard-delete is ever introduced) and M-02 owners (patient-side consumer).

---

## 8. Deviations and Waivers (Required — mark "None" if none)

- **Convention departed from:** Plan §3.4 anticipated `ValidatorRegistrationTests` at `tests/TopLab.Infrastructure.Tests/Persistence/`; verified file lives at `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (the M-12 close-out added it there as a DI-host regression test).
  - **Nature:** Extended the existing Application.Tests file in place (consistent with M-12/M14 precedent); the validator registration itself is an Application concern, so the location matches the file's existing scope.
  - **Justification:** Per plan §0.1.3, the original draft's path was wrong; this deviation corrects it.
  - **Temporary?** No — the file lives at the correct location going forward.
- **Convention departed from:** Plan §3.4 listed "12 M13 write commands" in A4; the actual count of distinct commands in §3.3 is **13** (5 PL + 3 TC + 5 CG).
  - **Nature:** Implemented all 13 commands enumerated in the plan's command-by-command table; the authorization theory class covers all 13 with `[InlineData]`.
  - **Justification:** The "12" in the plan body is a typo (or pre-finalization draft count); the plan's own per-command table is the authoritative source and lists 13.
  - **Temporary?** No.

---

## 9. Pending Reviews and Audits (Required)

- **Code review status:** Not started (local-only commits; no reviewer assigned).
- **Audit acceptance status:** Not started.
- **Blocking findings from review or audit:** none.

---

## 10. Next Session Objective (Required)

- **Most important task:** Begin **Module 15 — Culture & Antibiotic Configuration** per the dependency map (depends on M-12, now also on M-13), or the next module per the dependency map. M-02 (Patient Registration & Test Ordering) consumes `GetPriceListById`/`GetCustomGroupById` and the `EDIT_SYSTEM_SETTINGS` write surface.
- **Prerequisites:** M-13 requires a Presentation layer to expose the price-list / custom-group / comment screens (out of M-13 scope); when M-02 builds those screens it consumes the M-13 queries/commands already in place.
- **Expected end-state:** The next module reads `Docs/Handoff_<module>.md`, the Master Tracking Sheet `M13` row (should read 🟩 Done), and `Docs/OpenCode/M-13.md` exit criteria to verify, then proceeds with its own slice plan.

---

## 11. Required Reading Before Continuing (Required)

- Coding Standards & Conventions.
- Architecture & Folder Structure Blueprint.
- Data Model / Database Schema Blueprint — §4.5 `PriceList`/`PriceListItem`, §5.6 `CustomGroup`/`CustomGroupItem`/`TestComment` (no migration introduced by M-13; existing F5 baseline confirmed).
- Product Requirements Document — §M13 (FR-M13-001 … FR-M13-003).
- Test Strategy & Audit Acceptance Criteria — coverage-floor rules.
- Module Dependency & Execution Order Map — M-13 relations (M12, M14, M02, M03, M07).
- Master Tracking Sheet — §4 row M13 and §9 change log.
- ADR-0030 (this module), ADR-0029, ADR-0011 (close), ADR-0018 (close).
- Prior handoff documents for the same module: none (first delivery).
- In-repo execution record: `Docs/OpenCode/M-13.md`, `Docs/OpenCode/M-13-memory.md`.

---

## 12. Environment and Tooling Notes (Optional)

- .NET SDK 8; EF Core InMemory provider used for the item-persistence regression test (real SQL Server not required for M-13; zero migration, no schema drift).
- Commands executed during this session: `dotnet build TopLab.sln` / `dotnet build TopLab.sln -c Release` / `dotnet test TopLab.sln` / `dotnet test TopLab.sln -c Release`; `git status --porcelain`; `git diff` for each slice; `git log --oneline` to verify the prior slice's commit hash before committing the next.
- **Tooling gotcha (important):** Expression trees in EF Core (returned by `_db.Set<T>().AsQueryable()`) reject `out` variable declarations and statement-body lambdas — fixed in the read-side queries by materializing via `.ToList()` before the dictionary-lookup projection (mirrors the verified `GetWorkGroupLogsQueryHandler` pattern). When writing new query handlers in the codebase, always materialize the queryable before applying `tryGetValue`-style projections.
- **Tooling gotcha (important):** EF Core InMemory enforces identity-map tracking strictly — the M-13-S3 protocol's "aggregate-as-invariant-checker + flat-set persistence" was designed precisely to avoid this. The `PriceListItemPersistenceTests.SetPriceListItemPrice_CalledTwice_UpdatesSameRow_NoDoubleTracking` test reproduces the failure mode the protocol prevents.

---

## 13. Artifacts Produced (Required — mark "None" if none)

- **Name:** M-13 execution memory file — **Location:** `Docs/OpenCode/M-13-memory.md` — **Purpose:** slice-by-slice loop-engineering trace (Stages 1–10 per slice) — **Persistence:** Kept (in-repo working record).

---

## 14. Signature Block (Required)

| Role | Name | Date (UTC) | Confirmation |
|---|---|---|---|
| Outgoing agent | Local coding agent (Top-Lab) | 2026-09-07 | I confirm this handoff document accurately reflects the state of the work at session end. |
| Reviewer (if any) | TBD |  | I have reviewed this handoff for completeness. |
| Incoming agent (on acceptance) | TBD |  | I confirm I have read and understood this handoff and accept it as my starting context. |

---

## 15. Attachments (Optional)

- `Docs/OpenCode/M-13.md` — execution plan with exit criteria.
- `Docs/OpenCode/M-13-memory.md` — living loop-engineering trace (per-slice Stages 1–10 evidence).
- `Docs/Source/Top_Lab_ADR.md` — ADR-0030.

---

*End of handoff document.*
