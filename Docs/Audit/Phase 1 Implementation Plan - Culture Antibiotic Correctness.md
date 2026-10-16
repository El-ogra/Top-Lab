# Phase 1 Implementation Plan — Culture Antibiotic Correctness

> **Document type:** Phase Handoff / Implementation Plan (planning artifact only)
> **Mode of production:** analysis and planning. This plan changes nothing by itself. No product file, source file, test file, migration, snapshot, configuration, or audit report was created, modified, or deleted while producing it.
> **Authority:** the current source code at the measured HEAD. The audit report that defines this phase is the framing document; every implementation-level detail below was re-derived from the current code and is cited to `path:line`.

---

## A. Phase Identity

| Field | Value |
|---|---|
| Plan ID | `TL-P1-CULTURE-ABX` |
| Exact filename | `Phase 1 Implementation Plan - Culture Antibiotic Correctness.md` |
| Intended placement path | `Docs/Phase 1 Implementation Plan - Culture Antibiotic Correctness.md` |
| Creation timestamp (UTC) | 2026-10-08T11:17:00Z |
| Phase number | **1** |
| Phase title | **Culture antibiotic correctness** |
| Number of functions in phase | **3** (REF-093, REF-094, REF-092) |
| Plan revision | **1** |
| Planning status | **READY** — 3 of 3 functions `READY_AUTONOMOUS_DECISIONS_RECORDED`; 0 `BLOCKED` |

**Framing source.** The phase is defined by `Docs/Audit/Audit following the first phase.md` (measured against HEAD `6fa10aab…`), which lists it as *PHASE 1 — Culture antibiotic correctness (3 items; cheapest, clinically important)*. The three REF identifiers, their current classifications, and their priority ordering are taken from that audit and **re-verified against the current code** in §D.

**Relationship to the audit's "PHASE 0".** The audit carries a 12-item *PHASE 0 — Owner decisions* block. **PHASE 0 is not a deliverable, not a phase, and not a blocker for this plan.** Exactly one PHASE 0 item materially touches Phase 1 — **REF-209 "code samples per section"** (the audit marks it `BLOCKED` with "departments and coding granularity undocumented"). It is resolved autonomously in §D.1.8 decision 4, §D.2.8 decision 3 and §D.3.8 decision 6 (summarised in §G). No other PHASE 0 item is resolved, referenced, or mentioned anywhere in this plan, because none of them touches Phase 1.

---

## B. Baseline

| Item | Value | How verified |
|---|---|---|
| Repository root | `C:\Users\LAP LINK\source\repos\Top-Lab` (clone of `https://github.com/El-ogra/Top-Lab.git`; the cloud planning environment used `/workspace/Top-Lab` — historical only) | observed on this Windows machine |
| Branch | `main` | `git branch --show-current` |
| **Measured HEAD** | **`6fa10aabcb93d4ea6f49dba00423d7f89fc1b11d`** | `git rev-parse HEAD` |
| Working-tree state | Clean except two untracked planning artifacts (the audit report and this plan file) | `git status --porcelain` on this machine |
| Commits reachable from HEAD | 287 | `git rev-list --count HEAD` |
| Solution | `TopLab.sln` | root listing |
| Projects | 4 src + 5 test | `(Get-ChildItem -Path src -Filter *.csproj -Recurse).Count` → 4; `(Get-ChildItem -Path tests -Filter *.csproj -Recurse).Count` → 5 (run on this machine) |
| Target frameworks | `src/TopLab.Domain` `net8.0`; `src/TopLab.Application` `net8.0`; `src/TopLab.Infrastructure` `net8.0`; `src/TopLab.Presentation` `net8.0-windows`; test projects mirror these, with `tests/TopLab.Presentation.Tests` on `net8.0-windows` | `Get-ChildItem -Path src, tests -Filter *.csproj -Recurse \| Select-String -Pattern '<TargetFramework>([^<]+)</TargetFramework>'` — confirmed `net8.0` everywhere except the two `net8.0-windows` projects (run on this machine) |
| Installed SDKs | `8.0.425` and `9.0.318` (cloud value `9.0.316` is historical) | `dotnet --list-sdks` on this machine |
| Installed runtimes | .NET 8 present (`8.0.31`; also 6.0.36, 9.0.20) — **no `DOTNET_ROLL_FORWARD` needed on Windows** (cloud had .NET 9 only — historical) | `dotnet --list-runtimes` on this machine |
| EF Core | `Microsoft.EntityFrameworkCore` **8.0.30** (+ `SqlServer`, `InMemory`, `Tools`, `Design` all 8.0.30) | `Directory.Packages.props` |
| Other pinned packages | MediatR 12.5.0, FluentValidation 12.1.1, QuestPDF 2026.9.0, ZXing.Net 0.16.11, Testcontainers.MsSql 3.10.0 | `Directory.Packages.props` |
| Current migration set | **14 migrations**, `20260828052248_BaselineDataModel` … `20261002002546_AddAntibioticMasterFields` | `Get-ChildItem -Path src/TopLab.Infrastructure/Persistence/Migrations/*.cs -Exclude *.Designer.cs, ApplicationDbContextModelSnapshot.cs` → 14 files (run on this machine) |
| **Latest (most recent) migration** | **`20261002002546_AddAntibioticMasterFields`** | `… \| Sort-Object Name \| Select-Object -Last 1 -ExpandProperty Name` → `20261002002546_AddAntibioticMasterFields.cs` (run on this machine) |
| Model snapshot path | `src/TopLab.Infrastructure/Persistence/Migrations/ApplicationDbContextModelSnapshot.cs` | file listing |
| `Antibiotics` table columns at HEAD | `AntibioticId` (PK, identity int), `Name` (nvarchar(150), required), `IsPregnancyFlagged` (bit, required), `IsChildrenFlagged` (bit, required), `Symbol` (nvarchar(10), nullable), `ScientificName` (nvarchar(150), nullable) — **no commercial-name column** | baseline migration `:17-29`; snapshot `modelBuilder.Entity("TopLab.Domain.Tests.Antibiotic")`; `AntibioticConfiguration.cs:12-18` |
| Known pre-existing changes | Exactly two untracked files: the audit artifact and this plan file. **No tracked file is modified.** | `git status --porcelain` on this machine |
| Windows pre-implementation baseline (this machine, 2026-10-08) | Build `dotnet build TopLab.sln -c Release -p:EnableWindowsTargeting=true`: **0 warnings, 0 errors**. Tests per project — Domain 507 passed / 0 failed / 0 skipped; Application 1713 / 0 / 0; Infrastructure 349 / 0 / 0 (the 28 Linux Arial font failures pass here — Arial present); Persistence 13 passed / 0 failed / 2 skipped (Docker-gated: `MigrationChain_AppliesToEmptyDatabase`, `Settle_ConcurrentCalls_InsertOnePayment`); Presentation 171 / 0 / 0 (**runs on Windows — a required gate**). Antibiotic-related: 116 Application facts + 5 migration-guard facts, all passing. **Any failure after implementation is therefore NEW.** | commands run on this machine |
| Design-time migrations-list observation (this machine) | `dotnet ef migrations list` against the design-time database `localdb\TopLab` shows the 6 post-`AddInvoiceIssues` migrations as Pending on this machine. The planned `AddAntibioticCommercialName` migration was validated in a throwaway copy against disposable database `TopLab_Phase1Verify` (Up = single `AddColumn CommercialName / nvarchar(150) / nullable`; Down = single `DropColumn`; all 14 migrations apply cleanly; `CommercialName` absent before, present after, removed again by Down). | observed on this machine |

> **Historical-baseline warning.** This baseline is valid **only** at `6fa10aab…`. The moment the Build Agent creates its first commit, HEAD moves and this section becomes historical. The Build Agent must record its own baseline HEAD before starting and must re-verify the four "how verified" code facts that this phase depends on (§E.3) before editing.

---

## C. Phase Overview

### C.1 Purpose — why these three functions are grouped

These three functions are the entire culture-antibiotic master-data surface, and they share one aggregate (`TopLab.Domain.Tests.Antibiotic`), one EF configuration, one CRUD command pair, one editor window, and one DTO trio. They are grouped because:

1. **They are clinically coupled.** REF-093 and REF-094 are two halves of *one* filter rule implemented at *one* call site. Fixing one without the other produces an inconsistent grid. The audit already established that they must change together.
2. **They are the cheapest clinically important work in the entire sequence.** Two are a one-clause logic fix; one is a single additive nullable column following an existing, proven pattern (`Symbol`/`ScientificName`).
3. **They share a migration-free/single-migration boundary.** REF-093 and REF-094 touch no schema at all. REF-092 touches exactly one column. Grouping them keeps the phase to a single migration and a single coherent commit-per-function history.

### C.2 Ordered function list

| Order | REF-ID | Function | Current status | Migration | Depends on |
|---|---|---|---|---|---|
| 1 | **REF-093** | Antibiotic suitable for pregnancy | DIFFERENT | `NO_MIGRATION_REQUIRED` | — (none) |
| 2 | **REF-094** | Antibiotic suitable for children under twelve | DIFFERENT | `NO_MIGRATION_REQUIRED` | REF-093 (must land in the same change) |
| 3 | **REF-092** | Antibiotic master data with commercial name | DIFFERENT | `NEW_MIGRATION_REQUIRED` | REF-093, REF-094 (shared aggregate; sequenced after them so the aggregate is edited once, sequentially, and the migration is the last schema write) |

### C.3 Phase-level migration summary

| REF | Migration decision | Basis |
|---|---|---|
| REF-093 | **NO_MIGRATION_REQUIRED** | The fix changes only a boolean predicate in a LINQ-to-Objects `Where` over an already-materialised list. No column, index, key, or data row is added, removed, or altered. |
| REF-094 | **NO_MIGRATION_REQUIRED** | Same change site as REF-093; same basis. |
| REF-092 | **NEW_MIGRATION_REQUIRED** | The `Antibiotic` aggregate has no commercial-name member, `AntibioticConfiguration` maps no such property, the model snapshot declares no such column, and the `Antibiotics` table has no such column. One additive nullable column is required. |

**Phase-level count: exactly 1 new migration, belonging to REF-092.** This matches §E exactly.

### C.4 Cross-function dependencies inside the phase

* **REF-093 → REF-094 (hard).** Both are the two arms of one predicate at one call site (`GetCultureEntryGridQueryHandler.cs:37`). The audit's Phase-1 rule stands: *"Same clause; must change in the same commit to keep the grid consistent."* This plan executes them as **two separate commits in sequence, where the REF-094 commit's tests would fail if REF-093's change were absent** — the ordering is real, but the change is not safely splittable at runtime. See §D.2 and the risk in §H.
* **REF-092 → both (soft).** REF-092 adds a member to the same aggregate, the same EF configuration, the same command records, and the same editor window that REF-093/REF-094 are defined against. Sequencing it **last** means the aggregate's constructor/factory/normalisation logic is edited once, in one contiguous block, rather than twice.
* **Shared component:** `CultureAntibioticDisplay.IsDisplayable` (`src/TopLab.Application/Features/CultureAndAntibiotics/Common/CultureAntibioticDisplay.cs`) is the single shared rule engine consumed by both REF-093 and REF-094. **Neither function requires a change to this component** — see §D.2 for why. It is recorded as a shared component so the Build Agent does not "helpfully" modify it.

---

## D. Per-Function Sections

---

### D.1 — REF-093: Antibiotic marked as suitable for pregnancy

#### D.1.1 Function identity

| Field | Value |
|---|---|
| REF-ID | **REF-093** |
| Name | Antibiotic marked as suitable for pregnancy |
| Current status (verified) | **DIFFERENT** |
| Position in phase | **1 of 3** |
| Dependencies on earlier functions | None |
| Pre-identified BLOCKED flag (from the audit) | **None.** The audit lists REF-093 as DIFFERENT with the defect note `DEF-001`; it is *not* in the audit's BLOCKED list. The PHASE 0 block it appeared under in the audit sequence was a sequencing label, not a specification block. |

#### D.1.2 Investigation findings

**Evidence standard:** Claim → Evidence → Exact Location → Interpretation → Confidence.

---

**Claim 1 — The filter rule itself is correct.**
→ **Evidence:** `CultureAntibioticDisplay.IsDisplayable` returns `(!isPregnancyFlagged && !isChildrenFlagged) || (isPregnancyFlagged && isPregnancyIndicated) || (isChildrenFlagged && isChildUnder12)`.
→ **Exact location:** `src/TopLab.Application/Features/CultureAndAntibiotics/Common/CultureAntibioticDisplay.cs:7-16`
→ **Interpretation:** This is exactly the reference rule: an antibiotic with no flags is always shown; a pregnancy-flagged antibiotic is shown only when pregnancy is indicated; a children-flagged antibiotic only when the patient is under 12.
→ **Confidence:** **CONFIRMED**

**Claim 2 — The reference requires the rule to be applied unconditionally.**
→ **Evidence:** LEARN p.139, two passages read directly from the permitted PDF: `وٌظهر المضاد الحٌوى فى المزرعة فقط فى حالة وجود حمل` ("the antibiotic appears **only** in the case of pregnancy" — emphatic `فقط` = *only*), and for children `وٌظهر المضاد الحٌوى فى المزرعة فقط فى حالة الطفل` ("the antibiotic appears **only** in the case of the child"). The reference also states the child test is age < 12: `البرنامج ٌتعرؾ على األطفال إذا كان سنه أقل من 12 سنة ( ذكر أو أنثى )`.
→ **Exact location:** `RLS_Learn_Enhanced.pdf`, page 139
→ **Interpretation:** The reference states the visibility rule as a hard, unconditional constraint on what the culture grid may display. There is no "unless a result already exists" qualification anywhere on the page. The word `فقط` (only) is the decisive token and it appears twice, once per flag.
→ **Confidence:** **CONFIRMED** (Arabic prose on this page extracts cleanly; the two `فقط` occurrences are unambiguous and the surrounding sentence is intact)

**Claim 3 — The defect (DEF-001) is the `||` escape clause at the call site, not the rule engine.**
→ **Evidence:** the culture grid query composes the display predicate with an unconditional second disjunct:
```csharp
var rows = antibiotics.Where(x => CultureAntibioticDisplay.IsDisplayable(x.IsPregnancyFlagged, x.IsChildrenFlagged, pregnant, child) || saved.ContainsKey(x.Id.Value))
```
where `saved` is the set of `CultureAntibioticResult` rows already saved for this patient test.
→ **Exact location:** `src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs:37` (with `saved` built at `:34` and `antibiotics` materialised at `:36`)
→ **Interpretation:** Once a sensitivity result is saved for an antibiotic, that row re-enters the grid **regardless** of the patient's pregnancy state. For a non-pregnant adult, a pregnancy-only antibiotic that was once entered becomes visible again. The rule engine is doing the right thing; the caller overrides it. This precisely confirms the audit's DEF-001.
→ **Confidence:** **CONFIRMED**

**Claim 4 — End-to-end trace of current behaviour.**

| Layer | Symbol | Location |
|---|---|---|
| UI entry | culture result entry screen | `src/TopLab.Presentation/Views/…` (grid bound from the query below) |
| ViewModel | `CultureEntryViewModel` | loads the grid via the query |
| Query | `GetCultureEntryGridQuery(PatientTestId)` | `…/GetCultureEntryGrid/GetCultureEntryGridQuery.cs` |
| Handler | `GetCultureEntryGridQueryHandler.Handle` | `…/GetCultureEntryGridQueryHandler.cs:18-42` |
| Domain inputs | `PregnancySignal.IsPregnancyIndicated(categories)` (`:32`), `AgeRules.IsUnderTwelve(patient.AgeValue, patient.AgeUnit)` (`:33`) | `TopLab.Domain.Patients` |
| Persistence reads | `PatientTest` (`:20`), `Test` (`:22`, must be `IsCultureType`), `Patient` (`:25`), `PatientMedicalCondition`⋈`MedicalConditionType` (`:27-31`), `CultureAntibioticResult` (`:34`), `CultureAntibioticAttachment` (`:35`), `Antibiotic` (`:36`) | EF Core via `IApplicationDbContext` |
| Result | `CultureEntryGridDto` with `IReadOnlyList<CultureSensitivityRowDto> rows` (`:41`) | `TopLab.Application.Features.CultureResults.Common` |
| Database | `Antibiotics`, `CultureAntibioticResults`, `CultureAntibioticAttachments` tables | migrations 1 and later |

→ **Interpretation:** The predicate is evaluated in-process on an already-materialised `List<Antibiotic>`; the query is synchronous (`Task.FromResult`), so no SQL shape changes.
→ **Confidence:** **CONFIRMED**

**Claim 5 — Test coverage: the rule engine is fully covered; the handler is tested on every arm except the defect arm — and one existing fact locks the defect in.**
→ **Evidence:** `CultureAntibioticDisplayTests.cs` contains 5 facts including `IsDisplayable_FullTruthTable`, `IsDisplayable_BothFlagsSet_AndEitherContextHolds_ReturnsTrue`, `IsDisplayable_BothFlagsSet_NoContextHolds_ReturnsFalse`, `IsDisplayable_NoFlags_AlwaysTrue`. Separately, `find tests -iname "*CultureEntryGrid*"` returns **no file by that name**, but the handler **is** exercised behaviourally by `tests/TopLab.Application.Tests/Features/CultureResults/CultureResultQueryHandlerTests.cs` (7 facts invoking `GetCultureEntryGridQueryHandler.Handle`, including infant month-unit cases) and `CultureZoneRoundTripTests.cs`. Critically, `Grid_StrictlyFiltersChildrenButKeepsSavedFacts` seeds age 12 with a saved result for the children-flagged antibiotic and asserts **both** rows are returned — i.e. it encodes the DEF-001 escape clause as expected behaviour and will fail once the fix lands.
→ **Exact location:** `tests/TopLab.Application.Tests/Features/CultureAndAntibiotics/CultureAntibioticDisplayTests.cs:38,55,71,81`; `tests/TopLab.Application.Tests/Features/CultureResults/CultureResultQueryHandlerTests.cs:17-28`
→ **Interpretation:** This is a textbook **false-assurance split** with a sharper edge than first stated: the pure function is exhaustively truth-table tested, the handler is tested on every arm **except** the saved-row defeat — and on that arm a test actively asserts the defective outcome. All 5 rule-engine facts are genuine **behavioural** tests (they invoke `IsDisplayable` and assert its return value) — none is a source-text assertion. The new coverage must target the *handler*, and the REF-093 commit must **update** `Grid_StrictlyFiltersChildrenButKeepsSavedFacts` to assert exclusion (see §D.1.4 and §H risk R13).
→ **Confidence:** **CONFIRMED** (coverage wording corrected 2026-10-08 after handler-test discovery; the behavioural-vs-source-text distinction stands)

**Claim 6 — Saving a result is validated against attachment, but not against the display rule.**
→ **Evidence:** `SaveCultureResultsCommandHandler` checks that every submitted `AntibioticId` is in the culture's allowed attachment set (`if((r.Sensitivities??[]).Any(x=>!allowed.Contains(x.AntibioticId))) return Result.Failure(Error.Conflict("المضاد الحيوي غير مرفق بهذه المزرعة."));`) and that the culture is not already reviewed/printed/delivered.
→ **Exact location:** `src/TopLab.Application/Features/CultureResults/Commands/SaveCultureResults/SaveCultureResultsCommandHandler.cs` (single-expression handler; upsert plus stale-sync with `_db.Remove(stale)` all on `:5`)
→ **Interpretation:** A pre-existing saved row for an antibiotic that is *no longer* displayable is **not** filtered by the save path — but it is also **not** left dormant. The same handler syncs saved rows to the submission: submitted items are updated-or-created, and every saved row whose antibiotic is absent from the submission is deleted via `_db.Remove(stale)`. The entry screen submits only displayed grid rows (`CultureEntryViewModel.cs:264-278` loads `SensitivityRows` solely from `grid.Rows`; `:326-346` submits all of them), so the next save from that screen omits the hidden antibiotic and the stale-sync **deletes** its stored row permanently. Until that save happens, the hidden row still prints: the printed culture report (`GetCultureReportQueryHandler.cs:31,41-45`) and the combined report (`BuildCombinedReportCommandHandler.cs:115-120`) read saved rows directly with **no** `IsDisplayable` filter (`IsDisplayable` is used only at the grid, `:37`). For reviewed/printed/delivered cultures saves are blocked, so the row persists and keeps printing. The earlier "dormant row that re-appears" wording is withdrawn as incorrect (see §D.1.8 Decision 2 and §H risk R14).
→ **Confidence:** **CONFIRMED** (interpretation corrected 2026-10-08 after save-path verification)

**What is missing / materially different, stated precisely:**
The single disjunct `|| saved.ContainsKey(x.Id.Value)` at `GetCultureEntryGridQueryHandler.cs:37` allows a flagged antibiotic to be displayed when the reference requires it to be hidden. The reference is unconditional (`فقط` = only). Everything else about the rule — the flag semantics, the pregnancy signal, the age threshold, the rule engine — is already correct and must **not** be changed.

#### D.1.3 Scope definition

**IN SCOPE**
1. Remove the `|| saved.ContainsKey(x.Id.Value)` disjunct from the culture-grid row filter so visibility is governed solely by `CultureAntibioticDisplay.IsDisplayable`.
2. Preserve the existing `saved.TryGetValue(...)` lookup used to populate each row's `SensitivityCategory` and `InhibitionZoneMm` — only the *filter* changes, never the projection.
3. Add behavioural tests for the handler predicate covering: pregnancy-only antibiotic hidden for a non-pregnant patient **with a pre-existing saved result**; shown when pregnancy is indicated; unflagged antibiotic always shown.

**OUT OF SCOPE**
* Any change to `CultureAntibioticDisplay.IsDisplayable` — the rule is correct.
* Any change to `PregnancySignal`, `AgeRules`, or the age-unit semantics.
* Deleting or hiding already-saved `CultureAntibioticResult` rows. Cleanup of historical rows is a data decision, not a code correctness fix, and is explicitly deferred (§D.1.8, decision 3).
* Any change to the printed culture report layout.
* Any change to the attach/detach antibiotic commands.

**Non-goals**
* Not making pregnancy/children data a hard prerequisite for saving a sensitivity result. The reference constrains *display*; it does not state a save-time rule, and inventing one would change existing workflow behaviour beyond the reference. (Recorded as decision 1 in §D.1.8.)

#### D.1.4 Implementation plan by layer

**Domain — NO CHANGES.**
`src/TopLab.Domain/Tests/Antibiotic.cs` and `src/TopLab.Application/Features/CultureAndAntibiotics/Common/CultureAntibioticDisplay.cs` are **not modified**. This is deliberate: the rule engine is correct and independently tested.

**Application — MODIFY (1 file).**
* **File:** `src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs`
* **Line:** `:37`
* **Action:** change
```csharp
// BEFORE
var rows = antibiotics.Where(x => CultureAntibioticDisplay.IsDisplayable(x.IsPregnancyFlagged, x.IsChildrenFlagged, pregnant, child) || saved.ContainsKey(x.Id.Value))
// AFTER
var rows = antibiotics.Where(x => CultureAntibioticDisplay.IsDisplayable(x.IsPregnancyFlagged, x.IsChildrenFlagged, pregnant, child))
```
* **Must not change:** the `.OrderBy(x => x.Id.Value).Select(x => { saved.TryGetValue(...); ... })` continuation on `:38`. `saved` is still required for row projection.
* **Required behaviour:** a pregnancy-flagged antibiotic is returned **only** when `pregnant` is true, regardless of whether a `CultureAntibioticResult` exists for it.
* **Validation / authorization:** unchanged. This is a query handler with no `IAuthorizedRequest`; the permission model for culture result entry is enforced upstream and is out of scope.
* **Error handling:** unchanged. All existing early returns (`:21`, `:23`, `:24`, `:26`) stay as they are.

**Infrastructure — NO CHANGES.**

**Presentation — NO CHANGES.**

**Tests — CREATE (1 file).**
* **File:** `tests/TopLab.Application.Tests/Features/CultureResults/GetCultureEntryGridQueryHandlerTests.cs`
* **Style:** follow the existing culture/antibiotic test conventions in `tests/TopLab.Application.Tests/Features/CultureAndAntibiotics/` (xUnit, in-memory `IApplicationDbContext` where that is the established pattern — inspect `AntibioticCommandHandlerTests.cs` for the fixture shape before writing).
* **Required tests (behavioural, not source-text):**
  1. Pregnancy-only antibiotic with a saved result is **excluded** for a non-pregnant adult patient.
  2. Pregnancy-only antibiotic with a saved result **is included** when the patient's medical conditions indicate pregnancy.
  3. Unflagged antibiotic with a saved result **is included** (guards against over-correction: the fix must not hide ordinary antibiotics).
  4. A saved result for an excluded antibiotic does **not** appear in the projection (i.e. no row is emitted with a null `SensitivityCategory` for a hidden antibiotic).
* **Anti-requirement:** do **not** add a test that reads the `.cs` file and asserts on its text. The audit's DEF-009 records exactly that anti-pattern in this codebase and it is explicitly rejected.
* **Edit one existing test file inside the REF-093 commit (ordinary file edit, NOT `git commit --amend`, which is forbidden):** `tests/TopLab.Application.Tests/Features/CultureResults/CultureResultQueryHandlerTests.cs` — the fact `Grid_StrictlyFiltersChildrenButKeepsSavedFacts` asserts the defective outcome (both rows for a 12-year-old with a saved children-flagged result) and **will fail after the fix**. Update its expectation to a single row (antibiotic 1 only) so it asserts the corrected behaviour. This expected test change belongs to the REF-093 commit; it is not a regression (see §H risk R13).

**Shared-component note.** `CultureAntibioticDisplay` is shared with REF-094. It is **not** modified by either. If the Build Agent believes it must change, that belief is wrong and must be reported rather than acted on.

**Migration — none.**

#### D.1.5 Migration decision

**`NO_MIGRATION_REQUIRED`**

Evidence: the change removes a Boolean disjunct from an in-process LINQ-to-Objects `Where` over an already-materialised `List<Antibiotic>` (`GetCultureEntryGridQueryHandler.cs:36-37`). No entity member is added, removed, or altered; no column, index, key, constraint, or seed row is affected; `AntibioticConfiguration.cs` is untouched; the model snapshot is untouched. The two flag columns `IsPregnancyFlagged` / `IsChildrenFlagged` already exist in `20260828052248_BaselineDataModel.cs:22-23` and are already mapped at `AntibioticConfiguration.cs:15-16`. The existing schema is sufficient in full. **Confidence: CONFIRMED.**

#### D.1.6 Migration Contract

**Not applicable.** REF-093 is `NO_MIGRATION_REQUIRED`.

#### D.1.7 Acceptance criteria

1. `GetCultureEntryGridQueryHandler.cs:37` contains **no** `saved.ContainsKey` disjunct in the `Where` clause.
2. `CultureAntibioticDisplay.cs` is **byte-identical** to its HEAD state (`git diff` shows no change to that file).
3. `Antibiotic.cs` is byte-identical to its HEAD state.
4. `AntibioticConfiguration.cs` and `ApplicationDbContextModelSnapshot.cs` are byte-identical to their HEAD state.
5. Only intended files changed in the REF-093 commit: `git diff --stat` shows **exactly one** production file changed (`GetCultureEntryGridQueryHandler.cs`) plus test file(s) (the new handler-test file and the updated `CultureResultQueryHandlerTests.cs`).
6. A new test proves a pregnancy-flagged antibiotic with a pre-existing saved result is excluded for a non-pregnant adult.
7. A new test proves an unflagged antibiotic with a pre-existing saved result is still included.
8. The full `TopLab.Application.Tests` suite passes with **0 failures**.
9. `TopLab.Domain.Tests` and `TopLab.Infrastructure.Tests` show **no new failures** versus the pre-phase baseline.
10. No source-text-assertion test was added.
11. The pre-existing fact `Grid_StrictlyFiltersChildrenButKeepsSavedFacts` is updated to assert the corrected behaviour (children-flagged antibiotic with a saved result excluded for a 12-year-old) and passes.

#### D.1.8 Autonomous decisions made

**Decision 1 — Should the save path reject a sensitivity result for a non-displayable antibiotic?**
* **Options:** (a) leave save unchanged, enforce visibility only at read; (b) also validate at save and reject a pregnancy-only antibiotic for a non-pregnant patient.
* **Trade-offs:** (b) is more defensive but **invents a rule the reference does not state** — the reference text on LEARN p.139 constrains what *appears in the culture*, not what may be *entered*. It would also change existing workflow behaviour and could break legitimate historical re-entry.
* **SELECTED: (a)** — enforce visibility at read only.
* **Rationale:** evidence-first. The reference's operative words are about appearance (`يظهر … فقط`). Adding a write-time rule is a scope expansion, and the phase is explicitly a *correctness* fix, not a policy change.
* **Flag:** **INFERRED_OWNER_DECISION** (inferred from the reference's wording, not stated by the owner).

**Decision 2 — Should already-saved rows for now-invisible antibiotics be deleted or hidden?**
* **Options:** (a) filter at read only, leave stored rows; (b) purge stored rows that no longer qualify.
* **Trade-offs:** (b) would actively purge rows on deployment and is irreversible without a backup. (a) as verified does **not** leave a dormant row: the next entry-screen save deletes the hidden row through the existing submission-sync (the screen submits displayed rows only), and until then the row still prints in the culture and combined reports. So (a) converges to deletion through normal use rather than preserving data. This consequence is **accepted for Phase 1 because the application has not been used in production yet** (owner statement) — no real clinical data is at risk.
* **SELECTED: (a).**
* **Rationale:** no new deletion code is added by this phase — the outcome follows from the existing save-sync plus the corrected grid, and the reference constrains appearance, which the grid now enforces. A dedicated purge, a save-time rejection, or a print-path filter would each be new scope and are explicitly not added. The SELECTED option is unchanged.
* **Flag:** **INFERRED_OWNER_DECISION**.

**Decision 3 — Does the fix belong in `IsDisplayable` or at the call site?**
* **Options:** (a) add a parameter/overload to `IsDisplayable` to model the "saved" case; (b) remove the disjunct at the call site.
* **Trade-offs:** (a) would encode a *wrong* rule into a component whose current contract is correct and truth-table tested; it would also require rewriting or extending 5 passing tests. (b) is a one-token deletion at the single defect site.
* **SELECTED: (b).**
* **Rationale:** the rule engine is correct and independently verified; the caller is wrong. Fixing the caller is the minimal correct change.
* **Flag:** **autonomous decision**.

**Decision 4 — PHASE 0 item touching this function: REF-209 "code samples per section" (audit-flagged BLOCKED).**
* **Context:** the audit flags REF-209 as `BLOCKED` because the reference does not state the departments, nor whether the per-department code is per patient, per visit, or per test (LEARN p.208).
* **Does it touch REF-093? Material assessment: it does not.** REF-209 governs **sample separation/coding by department**. REF-093 governs **which antibiotics are visible in a culture grid**. The two share only the word "culture". Neither reads the other's data, neither depends on the other's entity, and resolving REF-209's granularity would change no line of the REF-093 fix.
* **Resolution:** **carried forward unresolved, with no Phase 1 impact.** The Build Agent must not implement REF-209, must not create a department entity, and must not let REF-209 block the REF-093 commit. This is recorded here so that a reader can see the item was considered and consciously deferred rather than overlooked.
* **Flag:** **autonomous decision** (deferred by explicit analysis, not by omission).

**No other PHASE 0 item touches REF-093, and none is referenced.**

#### D.1.9 Function-level status

**`READY_AUTONOMOUS_DECISIONS_RECORDED`**

---

### D.2 — REF-094: Antibiotic marked as suitable for children under twelve

#### D.2.1 Function identity

| Field | Value |
|---|---|
| REF-ID | **REF-094** |
| Name | Antibiotic marked as suitable for children under twelve |
| Current status (verified) | **DIFFERENT** |
| Position in phase | **2 of 3** |
| Dependencies on earlier functions | **REF-093** — same predicate, same file, same line |
| Pre-identified BLOCKED flag (from the audit) | **None** |

#### D.2.2 Investigation findings

**Claim 1 — REF-094 and REF-093 are the same defect, not two defects.**
→ **Evidence:** the call site passes both flags and both context booleans into one shared predicate: `CultureAntibioticDisplay.IsDisplayable(x.IsPregnancyFlagged, x.IsChildrenFlagged, pregnant, child)`. The `|| saved.ContainsKey(...)` disjunct defeats **both** arms simultaneously.
→ **Exact location:** `GetCultureEntryGridQueryHandler.cs:37`; rule at `CultureAntibioticDisplay.cs:7-16`
→ **Interpretation:** There is exactly one defective expression. REF-093 and REF-094 are two reference capabilities defeated by one line. The Build Agent therefore performs **one** code change and produces **two** function-closing commits (the second commit carrying the children-specific tests).
→ **Confidence:** **CONFIRMED**

**Claim 2 — The children rule matches the reference, including the age threshold.**
→ **Evidence:** `CultureAntibioticDisplay.ChildAgeThresholdYears = 12` and the `isChildrenFlagged && isChildUnder12` arm. `AgeRules.IsUnderTwelve(patient.AgeValue, patient.AgeUnit)` is the context source, i.e. it is **age-unit aware** (a "6 months" patient is not treated as a 6-year-old).
→ **Exact location:** `CultureAntibioticDisplay.cs:5,15`; `GetCultureEntryGridQueryHandler.cs:33`
→ **Interpretation:** The threshold and the unit-awareness are already correct. LEARN p.139 independently states `< 12 سنة ( ذكر أو أنثى )`. Nothing about the children rule needs changing — only its enforcement.
→ **Confidence:** **CONFIRMED**

**Claim 3 — The reference requirement is unconditional for children too.**
→ **Evidence:** LEARN p.139: `وٌظهر المضاد الحٌوى فى المزرعة فقط فى حالة الطفل إذا تم تخصيص المضاد الحٌوى للطفل عند إضافة المضاد الحٌوى للمزرعة` — "the antibiotic appears **only** in the case of the child, if the antibiotic was allocated to the child when added to the farm."
→ **Exact location:** `RLS_Learn_Enhanced.pdf`, page 139
→ **Interpretation:** The same unconditional `فقط` (only) construction as the pregnancy rule. Identical treatment required.
→ **Confidence:** **CONFIRMED**

**Claim 4 — Test coverage: zero at the defect site; the age-unit logic is tested elsewhere.**
→ **Evidence:** no test file *named* `*CultureEntryGrid*` exists, but handler-level facts do: `CultureResultQueryHandlerTests.cs` covers the children arm behaviourally (including `Infant11Months`/`Infant23Months` month-unit cases and the age-12 boundary), and its `Grid_StrictlyFiltersChildrenButKeepsSavedFacts` fact encodes the saved-row defeat as expected behaviour (updated in the REF-093 commit per §D.1.4). `CultureAntibioticDisplayTests.cs:71` (`IsDisplayable_BothFlagsSet_NoContextHolds_ReturnsFalse`) and `:81` (`IsDisplayable_NoFlags_AlwaysTrue`) cover the pure function only. Age-unit matching is covered by the analyte band tests elsewhere in the suite, not on this path.
→ **Exact location:** `tests/TopLab.Application.Tests/Features/CultureResults/CultureResultQueryHandlerTests.cs:31-76`; `CultureAntibioticDisplayTests.cs:71,81`
→ **Interpretation:** Same false-assurance pattern as REF-093, with the same correction: the handler is tested except on the saved-row defeat arm. Children-specific handler tests proving the corrected behaviour are still required.
→ **Confidence:** **CONFIRMED**

**What is missing / materially different, stated precisely:**
Identical to REF-093. A children-flagged antibiotic stays visible for a patient aged 12 or over once a sensitivity result has been saved for it, because of the `|| saved.ContainsKey(x.Id.Value)` disjunct at `GetCultureEntryGridQueryHandler.cs:37`. The reference requires it to appear only for a child.

#### D.2.3 Scope definition

**IN SCOPE**
1. The single disjunct removal (shared with REF-093) is what makes this function correct.
2. Behavioural tests proving: a children-only antibiotic with a pre-existing saved result is **excluded** for a patient aged ≥ 12; **included** for a patient aged < 12; an unflagged antibiotic remains included.

**OUT OF SCOPE**
* `CultureAntibioticDisplay.ChildAgeThresholdYears` — correct at 12, matches the reference, not changed.
* `AgeRules.IsUnderTwelve` — correct and unit-aware, not changed.
* Deleting stored results for now-invisible antibiotics (same as REF-093 decision 2).
* The printed culture report.

**Non-goals**
* Not adding a paediatric-dosing concept. The reference says only "allocated to the child", which the `IsChildrenFlagged` boolean already models.

#### D.2.4 Implementation plan by layer

**Domain — NO CHANGES.**
`CultureAntibioticDisplay.cs` is **not modified**, despite this function being nominally about children. The threshold and the rule are already correct.

**Application — MODIFY (1 file, same line as REF-093).**
* **File:** `src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs`
* **Action:** the same one-token removal of `|| saved.ContainsKey(x.Id.Value)` from `:37`.
* **Ordering note:** because the production change is identical to REF-093's, the Build Agent makes the **production edit in the REF-093 commit** and the **children-specific tests in the REF-094 commit**. The REF-094 commit therefore contains **test-only** changes. The update to the pre-existing `Grid_StrictlyFiltersChildrenButKeepsSavedFacts` fact (which fails as soon as the production line changes) belongs to the REF-093 commit, not here. This is stated explicitly so the Build Agent does not duplicate the edit or attempt a second conflicting modification. If the owner would prefer a single combined commit, the audit's own phase ordering still requires two commits; the test-only second commit satisfies both the commit rule and the correctness requirement.

**Infrastructure — NO CHANGES.**

**Presentation — NO CHANGES.**

**Tests — EXTEND (1 file, same file as REF-093).**
* **File:** `tests/TopLab.Application.Tests/Features/CultureResults/GetCultureEntryGridQueryHandlerTests.cs`
* **Required tests (behavioural):**
  1. Children-only antibiotic with a saved result is **excluded** for a patient aged ≥ 12 (both `Years` and a `Months` unit case, to prove unit-awareness is preserved).
  2. Children-only antibiotic with a saved result **is included** for a patient aged < 12.
  3. Unflagged antibiotic with a saved result **is included**.
  4. An antibiotic flagged for **both** pregnancy and children is included when **either** context holds (regression guard for the combined-flag arm).

**Migration — none.**

#### D.2.5 Migration decision

**`NO_MIGRATION_REQUIRED`**

Evidence: identical basis to REF-093 — a Boolean disjunct removed from an in-process predicate at `GetCultureEntryGridQueryHandler.cs:37`; the flag columns `IsChildrenFlagged` already exist (`20260828052248_BaselineDataModel.cs:23`) and are mapped (`AntibioticConfiguration.cs:16`); no entity, configuration, or snapshot change. **Confidence: CONFIRMED.**

#### D.2.6 Migration Contract

**Not applicable.** REF-094 is `NO_MIGRATION_REQUIRED`.

#### D.2.7 Acceptance criteria

1. The same single production change as REF-093 is in place (verified by `git diff` against the phase baseline).
2. `CultureAntibioticDisplay.cs`, `Antibiotic.cs`, `AntibioticConfiguration.cs`, and the model snapshot are all byte-identical to HEAD.
3. A test proves a children-only antibiotic with a saved result is excluded for a patient aged ≥ 12, in **both** `Years` and `Months` age units.
4. A test proves a children-only antibiotic is included for a patient aged < 12.
5. A test proves a both-flags antibiotic is included when either context holds.
6. The REF-094 commit contains **no production file changes** (test-only), and `git show --stat` confirms it.
7. `TopLab.Application.Tests` passes with 0 failures.

#### D.2.8 Autonomous decisions made

**Decision 1 — Should the children's threshold be configurable rather than the constant 12?**
* **Options:** (a) keep `ChildAgeThresholdYears = 12` as a compile-time constant; (b) add a system setting.
* **Trade-offs:** (b) is more flexible but the reference states a **fixed** 12-year rule in two places (LEARN p.139 body text, and the grid semantics), and this project already treats equivalent reference-fixed values as constants. (b) would also require a settings migration, expanding the phase's migration surface for no reference basis.
* **SELECTED: (a).**
* **Rationale:** the reference is unambiguous and fixed. Adding configurability is unrequested scope with a schema cost.
* **Flag:** **autonomous decision**.

**Decision 2 — Should the fix be split into two distinct predicates (one per flag) for clarity?**
* **Options:** (a) keep the single `IsDisplayable` call; (b) split into `IsDisplayableForPregnancy` + `IsDisplayableForChildren` and combine.
* **Trade-offs:** (b) reads more explicitly but duplicates the no-flags arm, doubles the truth-table surface, and would require rewriting 5 currently-passing behavioural tests for zero behavioural gain. The reference describes **one** filter with two flags.
* **SELECTED: (a).**
* **Rationale:** minimal correct change; the existing component already expresses the reference rule faithfully and is verified.
* **Flag:** **autonomous decision**.

**Decision 3 — PHASE 0 item touching this function.**
* **Assessment:** the only PHASE 0 item in the neighbourhood of the culture surface is **REF-209** (per-section sample coding), resolved for REF-093 in §D.1.8 decision 4. It does **not** touch REF-094 either — sample-section coding is independent of antibiotic visibility. Deferred with no Phase 1 impact.
* **Flag:** **autonomous decision** (deferred by explicit analysis).

**No other PHASE 0 item touches REF-094, and none is referenced.**

#### D.2.9 Function-level status

**`READY_AUTONOMOUS_DECISIONS_RECORDED`**

---

### D.3 — REF-092: Antibiotic master data with commercial name

#### D.3.1 Function identity

| Field | Value |
|---|---|
| REF-ID | **REF-092** |
| Name | Antibiotic master data with commercial name |
| Current status (verified) | **DIFFERENT** |
| Position in phase | **3 of 3** |
| Dependencies on earlier functions | REF-093, REF-094 (shared aggregate, edited once, sequenced last) |
| Pre-identified BLOCKED flag (from the audit) | **None.** The audit notes the adjacent contradiction C-1 (a stale VM comment claiming no `ScientificName` exists) as documentation-vs-code, not as a blocker. |

#### D.3.2 Investigation findings

**Claim 1 — The `Antibiotic` aggregate has no commercial-name member.**
→ **Evidence:** the entity declares exactly `Name`, `IsPregnancyFlagged`, `IsChildrenFlagged`, `Symbol`, `ScientificName`, and exposes `MaxSymbolLength = 10`, `MaxScientificNameLength = 150`. `Get-ChildItem -Path src -Recurse -Include *.cs | Select-String -Pattern 'CommercialName|TradeName'` returns **0 hits** (run on this machine).
→ **Exact location:** `src/TopLab.Domain/Tests/Antibiotic.cs:8-21`
→ **Interpretation:** The reference capability has no domain representation at all. This is a genuine missing capability, not a partially-implemented one.
→ **Confidence:** **CONFIRMED**

**Claim 2 — No configuration maps one.**
→ **Evidence:** `AntibioticConfiguration.Configure` maps `Id`, `Name`, `IsPregnancyFlagged`, `IsChildrenFlagged`, `Symbol`, `ScientificName` — six properties, no commercial name.
→ **Exact location:** `src/TopLab.Infrastructure/Persistence/Configurations/AntibioticConfiguration.cs:12-18`
→ **Confidence:** **CONFIRMED**

**Claim 3 — No migration provides one.**
→ **Evidence:** the `Antibiotics` table is created in `20260828052248_BaselineDataModel` with four columns (`AntibioticId`, `Name`, `IsPregnancyFlagged`, `IsChildrenFlagged`); the only later migration touching the table is `20261002002546_AddAntibioticMasterFields`, which adds exactly `Symbol` and `ScientificName`; the current model snapshot's `Antibiotic` entity block declares no commercial-name property.
→ **Exact location:** `src/TopLab.Infrastructure/Persistence/Migrations/20260828052248_BaselineDataModel.cs:17-29`; `…/20261002002546_AddAntibioticMasterFields.cs:13-25`; `…/ApplicationDbContextModelSnapshot.cs` (the `modelBuilder.Entity("TopLab.Domain.Tests.Antibiotic", …)` block)
→ **Interpretation:** **This confirms the audit's `NEW_MIGRATION_REQUIRED` expectation for REF-092 with no contradiction.**
→ **Confidence:** **CONFIRMED**

**Claim 4 — The reference documents a commercial-name column, distinct from the international/scientific name.**
→ **Evidence:** LEARN p.135, the antibiotic master window, shows a three-part header: `الاسم التجاري` ("the commercial name") on one axis alongside `International Antibiotic | Antibiotic By Abbr. | Scientific Name | Sensitivity`, and separately `مناسب للحوامل` ("suitable for pregnant") and `للأطفال` ("for children"). LEARN p.138, the per-farm attachment grid, shows the column set `Symb. | Scientific Name | Sensitivity | Pregnant | Children`.
→ **Exact location:** `RLS_Learn_Enhanced.pdf`, pages 135 and 138
→ **Interpretation:** The reference carries **four** distinct antibiotic text attributes: Symbol/abbreviation, International (generic) name, Scientific name, and **Commercial name**. Top-Lab models three. The gap is precisely the commercial name. This is decisive page evidence (Latin/numeric grid headers extract cleanly on p.138).
→ **Confidence:** **CONFIRMED**

**Claim 5 — An exact, proven template exists for the implementation.**
→ **Evidence:** `Symbol` and `ScientificName` were added end-to-end in migration `20261002002546_AddAntibioticMasterFields` with a dedicated migration test file, and the current code carries the complete vertical slice for both: entity constant + property + factory parameter + `Update` parameter + `Normalize` call, EF config line, DTO field on `AntibioticDto` and `AttachedAntibioticDto`, both command records + validators, the editor VM property/`InitializeNew`/`InitializeEdit`/`SaveAsync`, and an XAML `TextBox`.
→ **Exact location:** `Antibiotic.cs:15,18,20-22,28,38,45,48,58-59,62-76`; `AntibioticConfiguration.cs:17-18`; `AntibioticDtos.cs:8-9,16-17`; `CreateAntibioticCommand.cs:12`, `UpdateAntibioticCommand.cs:12`; both validators; `AntibioticEditorViewModel.cs:24-25,40-41,53-54,65-66,79-89`; `AntibioticEditorWindow.xaml:19,21`
→ **Interpretation:** REF-092 is a **mechanical replication** of an already-shipped pattern. This is why the audit called it "low" complexity, and it is the strongest available evidence that the change is low-risk.
→ **Confidence:** **CONFIRMED**

**Claim 6 — Test coverage for the existing fields is strong and must be mirrored, not replaced.**
→ **Evidence:** `AntibioticMasterFieldsTests.cs` (6 facts), `AntibioticCommandHandlerTests.cs` (15 facts), `AntibioticValidatorTests.cs`, `AntibioticQueryHandlerTests.cs`, `AntibioticTests.cs` (8 facts, Domain), `AddAntibioticMasterFieldsMigrationTests.cs` (5 facts asserting column type, nullability, count, and absence of any commercial-name operation).
→ **Exact location:** as listed
→ **Interpretation:** Two of those migration facts are **load-bearing constraints on this work** and are analysed in Claim 7.
→ **Confidence:** **CONFIRMED**

**Claim 7 — CRITICAL CONSTRAINT: two existing migration tests forbid the obvious shortcut.**
→ **Evidence:**
```csharp
[Fact] public void AddAntibioticMasterFields_AddsExactlyTwoColumns()
    => Assert.Equal(2, Operations(true).OfType<AddColumnOperation>().Count());

/// <summary>SD-2 (decision 3 = c): the commercial-name table must never appear.</summary>
[Fact] public void AddAntibioticMasterFields_ContainsNoCommercialNameColumn()
{ /* asserts no AddColumn/CreateTable whose name contains "Commercial", in Up or Down */ }
```
Both instantiate `new AddAntibioticMasterFields()` — the **old** migration — and inspect only its own operations.
→ **Exact location:** `tests/TopLab.Infrastructure.Tests/Persistence/Migrations/AddAntibioticMasterFieldsMigrationTests.cs:13,45-49,51-60`
→ **Interpretation — two consequences, both mandatory:**
> **(a) The commercial name MUST NOT be added to the existing `AddAntibioticMasterFields` migration.** Doing so would break `AddsExactlyTwoColumns` *and* `ContainsNoCommercialNameColumn`. A **new** migration is required — which independently corroborates the `NEW_MIGRATION_REQUIRED` decision for a reason beyond mere schema absence.
> **(b) Neither test may be edited or deleted to make the new work fit.** They are existing regression guards encoding a prior owner decision ("SD-2 decision 3 = c"). The Build Agent must leave this file untouched.
→ **Confidence:** **CONFIRMED**

**Claim 8 — DEFECT: the editor VM carries a stale, actively misleading comment.**
→ **Evidence:** the class doc states *"Antibiotic editor dialog VM (S-02 Slice 5): Name + the two flags only (**no Abbreviation/ScientificName/SensitivityCategory exists in code**)."* `ScientificName` and `Symbol` both exist in the entity, in the commands, in the DTOs, and in the XAML.
→ **Exact location:** `src/TopLab.Presentation/ViewModels/Lab/AntibioticEditorViewModel.cs:10-13`
→ **Interpretation:** This is the audit's contradiction **C-1**. It is additionally the **only** occurrence of the token `Abbreviation` in `src/`, which is what satisfied the audit's REF-218 negative search. It is documentation-level, not behaviour-level, but it sits directly in the file being edited for REF-092 and will actively mislead the next reader. Correcting it is **in scope** for this function because the file is already being modified.
→ **Confidence:** **CONFIRMED**

**Claim 9 — `AttachedAntibioticDto` also carries `Symbol`/`ScientificName`, so the attach/culture screens are part of the vertical slice.**
→ **Evidence:** `AttachedAntibioticDto(int AntibioticId, string Name, bool IsPregnancyFlagged, bool IsChildrenFlagged, string? Symbol = null, string? ScientificName = null)` and `CultureAntibioticListDto(TestId, TestName, AttachedCount, Antibiotics)`.
→ **Exact location:** `src/TopLab.Application/Features/CultureAndAntibiotics/Common/AntibioticDtos.cs:11-22`; consumed by `GetCultureAntibioticsQueryHandler` / `CultureAttachmentViewModel`
→ **Interpretation:** To keep the DTO trio symmetric and avoid a second asymmetry, `CommercialName` should be added to `AttachedAntibioticDto` as well. Whether it is then *displayed* on the attachment screen is a presentation question resolved in §D.3.8.
→ **Confidence:** **CONFIRMED**

**What is missing / materially different, stated precisely:**
The `Antibiotic` master data carries Symbol and Scientific name but **no commercial name**, in the entity, the schema, both DTOs, both commands, both validators, the editor view-model, and the editor window. The reference documents the commercial name as a first-class column (LEARN p.135, p.138). Every other layer of the feature is complete, so the gap is a single missing attribute threaded through the existing vertical slice.

#### D.3.3 Scope definition

**IN SCOPE**
1. `Antibiotic`: new nullable `CommercialName` property, `MaxCommercialNameLength` constant, factory parameter, `Update` parameter, `Normalize` call.
2. `AntibioticConfiguration`: one `HasMaxLength(...).IsRequired(false)` mapping.
3. **One new EF Core migration** adding a single nullable `nvarchar(150)` column, plus the regenerated model snapshot.
4. `AntibioticDto` **and** `AttachedAntibioticDto`: new optional field.
5. `CreateAntibioticCommand` + validator; `UpdateAntibioticCommand` + validator.
6. `AntibioticEditorViewModel`: property, `InitializeNew`, `InitializeEdit`, `SaveAsync` wiring — and the stale class comment corrected (Claim 8).
7. `AntibioticEditorWindow.xaml`: one `TextBox`.
8. Tests: Domain, Application (commands/validators/query/editor VM), Infrastructure (migration contract), Presentation (editor VM property round-trip).

**OUT OF SCOPE**
* **Not** an "International name" / generic-name field. LEARN p.135 shows it as a separate reference column, but the audit did **not** identify it as an in-scope gap for REF-092, and adding it would widen the phase. Recorded in §D.3.8 decision 3 as explicitly excluded.
* **Not** a per-culture trade-name list. The reference's commercial name is a master-data attribute.
* **Not** printed on the culture sensitivity report. Show p.24/LEARN p.139 describe the *entry grid*, and the audit's print-related items are separate REF identifiers.
* **Not** a uniqueness constraint on commercial name. Brand names are not unique across manufacturers.
* **Not** an edit to `AddAntibioticMasterFields` or its tests (Claim 7).

**Non-goals**
* Not backfilling any existing data. Every existing antibiotic gets `NULL`, which the UI renders as an empty field.

#### D.3.4 Implementation plan by layer

**Domain — MODIFY (1 file).**
* **File:** `src/TopLab.Domain/Tests/Antibiotic.cs`
* **Add:** `public const int MaxCommercialNameLength = 150;` and `public string? CommercialName { get; private set; }`
* **Modify:** the private constructor — add a `string? commercialName` parameter after `scientificName`; the `Create` factory — add a `string? commercialName = null` parameter after `scientificName` and pass `Normalize(commercialName, MaxCommercialNameLength, nameof(commercialName))`; the `Update` method — add a `string? commercialName = null` parameter after `scientificName` and assign the same normalised value.
* **Required behaviour:** blank/whitespace input normalises to `null`; input longer than 150 characters throws `ArgumentException` naming `commercialName` — identical to `Normalize`'s existing contract.
* **Optional parameter ordering matters:** appending new optional parameters to the end keeps every existing positional call site compiling. Do not reorder existing parameters.

**Application — MODIFY (9 files).**
* `Common/AntibioticDtos.cs` — append `string? CommercialName = null` to `AntibioticDto` and to `AttachedAntibioticDto` (append last to preserve existing positional construction).
* `Commands/CreateAntibiotic/CreateAntibioticCommand.cs` — append `string? CommercialName = null`.
* `Commands/CreateAntibiotic/CreateAntibioticCommandHandler.cs` — pass `request.CommercialName` into `Antibiotic.Create`.
* `Commands/CreateAntibiotic/CreateAntibioticCommandValidator.cs` — `RuleFor(x => x.CommercialName).MaximumLength(150).WithMessage("الاسم التجاري يجب ألا يتجاوز 150 حرفًا.");`
* `Commands/UpdateAntibiotic/UpdateAntibioticCommand.cs` — append `string? CommercialName = null`.
* `Commands/UpdateAntibiotic/UpdateAntibioticCommandHandler.cs` — pass `request.CommercialName` into `antibiotic.Update`.
* `Commands/UpdateAntibiotic/UpdateAntibioticCommandValidator.cs` — matching rule and message.
* `Queries/GetAntibiotics/GetAntibioticsQueryHandler.cs` and/or `GetCultureAntibiotics/GetCultureAntibioticsQueryHandler.cs` — map the new DTO field where `Symbol`/`ScientificName` are mapped today. **The Build Agent must locate the actual projection by searching for `ScientificName` in those handlers rather than assuming the line numbers.**

**Validation / authorization:** both commands already implement `IAuthorizedRequest` with `RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS"` (`UpdateAntibioticCommand.cs:16`, `CreateAntibioticCommand.cs:15`). **This must remain unchanged** — no new permission code is introduced by this phase.

**Error handling:** existing pattern — `ArgumentException` from the domain is caught and converted via `DomainFailureTranslator.Translate(ex)` into `Error.Validation(...)`. `CommercialName` must follow the identical path. Duplicate-name `Error.Conflict` logic is unaffected.

**Infrastructure — MODIFY (1 configuration file).**
* **File:** `src/TopLab.Infrastructure/Persistence/Configurations/AntibioticConfiguration.cs`
* **Add after line 18:** `b.Property(e => e.CommercialName).HasMaxLength(150).IsRequired(false);`
* **Do not** add a unique index or any other constraint.

**Presentation — MODIFY (2 files).**
* `ViewModels/Lab/AntibioticEditorViewModel.cs` — add `_commercialName` field + `CommercialName` property; set to `null` in `InitializeNew`; assign `dto.CommercialName` in `InitializeEdit`; pass `BlankToNull(CommercialName)` as the final argument in both `CreateAntibioticCommand` and `UpdateAntibioticCommand` constructions inside `SaveAsync`. **Also correct the stale class comment at lines 10-13** (Claim 8) so it no longer claims `ScientificName` is absent.
* `Views/Lab/AntibioticEditorWindow.xaml` — add one labelled `TextBox` bound to `CommercialName` with `MaxLength="150"`, placed adjacent to the existing `ScientificName` box (line 19), using an Arabic label consistent with the surrounding markup.

**Tests — CREATE / EXTEND.**
| Project | Action | Content |
|---|---|---|
| `TopLab.Domain.Tests` | extend `Tests/AntibioticTests.cs` | `Create` stores/normalises `CommercialName`; blank → `null`; >150 chars throws; `Update` sets and clears it |
| `TopLab.Application.Tests` | extend `AntibioticMasterFieldsTests.cs` (or add a sibling file) | command round-trip carries `CommercialName` through create and update |
| `TopLab.Application.Tests` | extend `AntibioticValidatorTests.cs` | validator rejects >150 chars, accepts empty/null |
| `TopLab.Application.Tests` | extend `AntibioticQueryHandlerTests.cs` | DTO projection carries `CommercialName` |
| `TopLab.Infrastructure.Tests` | **CREATE** `Persistence/Migrations/AddAntibioticCommercialNameMigrationTests.cs` | the new migration adds exactly one `AddColumnOperation`: table `Antibiotics`, type `nvarchar(150)`, nullable; `Down` drops exactly that one column; Up/Down contain no `CreateTableOperation` and no other column |
| `TopLab.Presentation.Tests` | create/extend an editor-VM test | `InitializeNew` clears; `InitializeEdit` populates; `SaveAsync` forwards the value (this project targets `net8.0-windows`; it **runs on this Windows machine and is a required gate** — see §H risk R3) |
| **DO NOT MODIFY** | `AddAntibioticMasterFieldsMigrationTests.cs` | Claim 7 — existing regression guards |

**Migration — CREATE 1 (see §D.3.6).**

**Shared-component note.** The `Antibiotic` aggregate and the editor window are shared with REF-093/REF-094 at the *entity* level only; REF-093/REF-094 make no change to the aggregate, so there is no write conflict. Sequencing REF-092 last simply keeps the aggregate's edit contiguous.

#### D.3.5 Migration decision

**`NEW_MIGRATION_REQUIRED`**

Evidence, all re-verified at HEAD and none contradicting the audit:
1. `src/TopLab.Domain/Tests/Antibiotic.cs` has no commercial-name member (`:8-21`).
2. `src/TopLab.Infrastructure/Persistence/Configurations/AntibioticConfiguration.cs:12-18` maps no such property.
3. `20260828052248_BaselineDataModel.cs:17-29` creates `Antibiotics` without it.
4. `20261002002546_AddAntibioticMasterFields.cs:13-25` — the only later migration touching `Antibiotics` — adds only `Symbol` and `ScientificName`.
5. `ApplicationDbContextModelSnapshot.cs`, `modelBuilder.Entity("TopLab.Domain.Tests.Antibiotic", …)`, declares no such property.
6. `Get-ChildItem -Path src -Recurse -Include *.cs | Select-String -Pattern 'CommercialName|TradeName'` → **0 hits** (run on this machine).
7. **Independent corroboration:** the existing regression tests at `AddAntibioticMasterFieldsMigrationTests.cs:45-60` actively forbid putting a commercial-name operation into the existing migration, so a *new* migration is the only compliant route.

**Confidence: CONFIRMED.** The audit's expectation is correct and is reinforced, not corrected.

#### D.3.6 Migration Contract

| Field | Value |
|---|---|
| Current latest migration | `20261002002546_AddAntibioticMasterFields` (class `AddAntibioticMasterFields`) |
| Proposed migration name | `AddAntibioticCommercialName` |
| Proposed file | `src/TopLab.Infrastructure/Persistence/Migrations/<yyyyMMddHHmmss>_AddAntibioticCommercialName.cs` (+ its `.Designer.cs`) |
| Timestamp | Generated by `dotnet ef migrations add`; must sort **after** `20261002002546` |
| Reason | The antibiotic master data requires a commercial-name attribute (LEARN p.135, p.138) that has no domain member, no EF mapping, and no column. |

**Expected `Up()` — exactly one operation**
```csharp
migrationBuilder.AddColumn<string>(
    name: "CommercialName",
    table: "Antibiotics",
    type: "nvarchar(150)",
    maxLength: 150,
    nullable: true);
```

**Expected `Down()` — exactly one operation**
```csharp
migrationBuilder.DropColumn(
    name: "CommercialName",
    table: "Antibiotics");
```

**Affected schema objects**
| Object | Change |
|---|---|
| Table `Antibiotics` | +1 column `CommercialName` `nvarchar(150)` NULL |
| Primary key | **unchanged** (`AntibioticId`) |
| Foreign keys | **none** — this column is a scalar; no FK is created |
| Indexes | **none** added, none dropped |
| Unique constraints | **none** |
| Other tables | **untouched** — no change to `CultureAntibioticAttachments`, `CultureAntibioticResults`, `CultureResults`, `Tests`, `Patients` |
| Seed data | **no seed change.** The `Antibiotics` table has no `HasData` seed rows in any migration; the 13 seeded rows are permission codes, not antibiotics |

**Data migration / backfill:** **none required.** Every pre-existing row receives `NULL`. This is safe because the new column is nullable, the domain property is `string?`, the DTO field is optional, and the editor renders `null` as an empty text box. A backfill is explicitly **out of scope**.

**Prohibited operations** (any of these is a material mismatch):
* Editing, regenerating, or deleting `20261002002546_AddAntibioticMasterFields` or any earlier migration.
* Editing, regenerating, or deleting `ApplicationDbContextModelSnapshot.cs` **by hand** — it is regenerated by the tooling, never hand-edited.
* Editing or deleting `AddAntibioticMasterFieldsMigrationTests.cs` (Claim 7).
* Dropping/renaming any existing column, the PK, or any index.
* Adding a `CreateTableOperation` (the audit's `SD-2 decision 3 = c` explicitly forbids a separate commercial-name **table**; this phase adds a **column**).
* Adding a unique index or constraint on `CommercialName`.
* Any destructive (`Drop`/`Delete`) data operation.

**Compatibility considerations**
* **Additive and nullable**, so it is backward-compatible with a database created at any earlier migration.
* Existing deployments can roll forward safely; the application must tolerate `NULL` (it does, by construction: `string?` end-to-end and `Normalize` maps blank → `null`).
* `Down()` is destructive to any commercial-name data entered after the migration — that is expected for a `DropColumn` and is the standard EF contract.

**Rollback expectations:** running `Down()` (or rolling the application back to a pre-migration build) removes the column. A pre-migration build that still selects `CommercialName` would fail; therefore the migration and the code change must ship together, which they do within this function's single commit.

**Expected database verification** (to be performed by the local agent only — see the local agent prompt):
1. `dotnet ef migrations list` shows the new migration applied and no pending migrations.
2. `SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Antibiotics'` returns `CommercialName | nvarchar | 150 | YES`.
3. `SELECT COUNT(*) FROM Antibiotics WHERE CommercialName IS NOT NULL` returns `0` immediately after migration (no backfill).
4. Existing row count is unchanged by the migration.

**What constitutes a material mismatch**
* Any `Up()` operation other than the single `AddColumnOperation` above.
* A column type other than `nvarchar(150)`, or a non-nullable column.
* Any edit to a pre-existing migration, its `.Designer.cs`, or the existing migration test file.
* A hand-edited snapshot that does not match the entity + configuration.
* A pending-migration list that is non-empty after applying.

**What the Build Agent must do if a mismatch appears:** **stop**, do not hand-patch the migration or the snapshot, record the exact discrepancy (operation list, generated SQL, `migrations list` output) in the function's report, and leave the working tree in a state where the discrepancy is diagnosable. The Build Agent must not delete and re-add migrations to force a clean diff.

#### D.3.7 Acceptance criteria

1. `Antibiotic.cs` exposes `CommercialName` (nullable) and `MaxCommercialNameLength = 150`; blank input normalises to `null`; >150 chars throws.
2. `AntibioticConfiguration.cs` maps `CommercialName` with `HasMaxLength(150).IsRequired(false)`.
3. A new migration `AddAntibioticCommercialName` exists with **exactly one** `AddColumnOperation` in `Up()` and **exactly one** `DropColumnOperation` in `Down()`.
4. `ApplicationDbContextModelSnapshot.cs` is regenerated by tooling (not hand-edited) and declares `CommercialName`.
5. `dotnet ef migrations has-pending-model-changes` (or the EF8 equivalent) reports no pending model change.
6. `AddAntibioticMasterFieldsMigrationTests.cs` is byte-identical to its HEAD state, and its 5 facts still pass.
7. Both `AntibioticDto` and `AttachedAntibioticDto` carry `CommercialName`.
8. Create and update commands, handlers, and validators carry `CommercialName`; validator rejects >150 chars with an Arabic message consistent with siblings.
9. `AntibioticEditorViewModel` exposes, initialises, and forwards `CommercialName`; the stale class comment at lines 10-13 no longer claims `ScientificName` is absent.
10. `AntibioticEditorWindow.xaml` has one `CommercialName` `TextBox` with `MaxLength="150"`.
11. Permission behaviour unchanged: both commands still require `EDIT_SYSTEM_SETTINGS`.
12. `TopLab.Domain.Tests`, `TopLab.Application.Tests`, `TopLab.Infrastructure.Tests` all pass with **0 new failures** vs the phase baseline.
13. A migration-contract test file exists asserting the operation count/type/nullability in both directions.
14. No unique index or constraint on `CommercialName`.

#### D.3.8 Autonomous decisions made

**Decision 1 — Column length: 150 or 200?**
* **Options:** (a) `nvarchar(150)`, matching `Name` and `ScientificName`; (b) `nvarchar(200)` or larger.
* **Trade-offs:** (b) allows longer brand strings but breaks the aggregate's internal consistency and every existing validator message pattern. The reference page gives no length. Commercial/trade names in this domain are short.
* **SELECTED: (a) 150.**
* **Rationale:** exact precedent — `Name` and `ScientificName` are both 150 with matching Arabic messages, so the new field is indistinguishable in the codebase's own idiom.
* **Flag:** **autonomous decision**.

**Decision 2 — Nullable, or required with a backfill?**
* **Options:** (a) nullable `string?`; (b) required.
* **Trade-offs:** (b) would require backfilling every existing row with invented data, or failing the migration. Fabricating trade names is unacceptable.
* **SELECTED: (a) nullable.** No backfill, empty field in the editor.
* **Rationale:** matches `Symbol` and `ScientificName`, both nullable; a nullable additive column is the only non-destructive option.
* **Flag:** **autonomous decision**.

**Decision 3 — Should the reference's "International name" column be added at the same time?**
* **Options:** (a) add only the commercial name; (b) add both.
* **Trade-offs:** LEARN p.135 shows an international/generic name column distinct from the commercial and scientific names. (b) would be a second additive column — a modest extra cost. **However**, the audit's REF-092 definition and the Phase 1 scope are the commercial name specifically; the international name was not identified as an in-scope gap by the audit, and widening a 3-function phase without an audit finding risks scope creep.
* **SELECTED: (a).** The international name is **explicitly excluded from Phase 1** and is not a defect in this function.
* **Rationale:** execute the phase as scoped; record the observation so a future phase can pick it up deliberately.
* **Flag:** **autonomous decision**.

**Decision 4 — Should `AttachedAntibioticDto` also carry `CommercialName`?**
* **Options:** (a) add it, keeping the DTO trio symmetric; (b) add only to `AntibioticDto`.
* **Trade-offs:** (a) touches one more projection (a read path for the culture-attachment screen) but keeps the two DTOs symmetric and avoids a *second* asymmetry of exactly the kind already recorded as contradiction C-1. (b) is smaller but leaves `AttachedAntibioticDto` permanently behind the master DTO.
* **SELECTED: (a).** Carry the value in the DTO.
* **Trade-offs accepted:** whether to **display** it on the attachment screen is out of scope — the reference evidence for a *displayed* commercial-name column on the attachment grid (LEARN p.138) shows `Symb. | Scientific Name | Sensitivity | Pregnant | Children` and does **not** list a commercial-name column. So: carried in the DTO, **not** added to that screen's columns. This distinction is deliberate and is recorded so the Build Agent does not add an unjustified column to the attachment grid.
* **Flag:** **INFERRED_OWNER_DECISION** (inferred from p.138's column set).

**Decision 5 — Naming: `CommercialName` vs `TradeName`.**
* **Options:** (a) `CommercialName`, matching the reference's `الاسم التجاري`; (b) `TradeName`.
* **Trade-offs:** (b) is idiomatic English but diverges from the reference's own terminology and from the `International/Scientific` naming family already present.
* **SELECTED: (a) `CommercialName`.**
* **Rationale:** maps 1:1 to the reference column and is what the existing regression test's `Contains("Commercial")` check anticipates, which keeps the new test file idiomatic with its neighbour.
* **Flag:** **autonomous decision**.

**Decision 6 — PHASE 0 item touching this function.**
* **Assessment:** the only PHASE 0 item in this neighbourhood is **REF-209** (per-section sample coding). It does **not** touch REF-092 — REF-092 is master-data text for an antibiotic, while REF-209 is sample coding granularity, touching `PatientTest` sample-kind flags, not `Antibiotic`. Deferred with no Phase 1 impact, consistent with §D.1.8 decision 4 and §D.2.8 decision 3.
* **Flag:** **autonomous decision** (deferred by explicit analysis).

**No other PHASE 0 item touches REF-092, and none is referenced.**

#### D.3.9 Function-level status

**`READY_AUTONOMOUS_DECISIONS_RECORDED`**

---

## E. Phase-Level Migration Sequence

### E.1 Ordered migration list

| Order | Migration | Belongs to | Cross-function dependency |
|---|---|---|---|
| — | *(none)* | REF-093 | No migration. Schema unchanged. |
| — | *(none)* | REF-094 | No migration. Schema unchanged. |
| **1 (only)** | `AddAntibioticCommercialName` | **REF-092** | None. It is the phase's only schema write and depends on nothing in the phase. |

**The Build Agent must create migrations in this order: exactly one migration, for REF-092, after the REF-093 and REF-094 commits.** The order among functions is fixed (REF-093 → REF-094 → REF-092); the migration order is therefore trivially the same.

### E.2 Count confirmation

```
Functions in phase                       = 3
Functions requiring NO_MIGRATION          = 2   (REF-093, REF-094)
Functions requiring NEW_MIGRATION         = 1   (REF-092)
Functions with MIGRATION_DECISION_BLOCKED = 0
Migrations to be created                  = 1
Existing migrations to be edited           = 0   (prohibited)
Migrations per phase summary (§C.3)       = 1   -> MATCHES
```

**Reconciliation: §C.3 states exactly 1 new migration belonging to REF-092; §E lists exactly 1. Consistent.**

### E.3 Facts the Build Agent must re-verify before editing

Because this baseline becomes historical at the first commit, the Build Agent must re-verify these five facts at the actual working HEAD before making any edit, and must report the result:

1. `git rev-parse HEAD` — record the true starting commit.
2. `Get-ChildItem -Path src/TopLab.Infrastructure/Persistence/Migrations/*.cs -Exclude *.Designer.cs, ApplicationDbContextModelSnapshot.cs | Sort-Object Name | Select-Object -Last 1 -ExpandProperty Name` — confirm `20261002002546_AddAntibioticMasterFields.cs` is still the latest.
3. `Get-ChildItem -Path src -Recurse -Include *.cs | Select-String -Pattern 'CommercialName|TradeName'` — confirm still zero hits.
4. `(Get-Content -LiteralPath "src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs")[36]` — confirm the `|| saved.ContainsKey(...)` disjunct is still present on line 37.
5. `git status --porcelain` — confirm no unexpected pre-existing modifications.

**If any of these has changed, stop and report it.** Do not adapt silently.

---

## F. Phase-Level Acceptance Criteria

The phase is complete when **all** of the following hold:

1. **REF-093 is closed.** A pregnancy-flagged antibiotic is not displayed for a non-pregnant patient even when a saved result exists; proven by a behavioural test.
2. **REF-094 is closed.** A children-flagged antibiotic is not displayed for a patient aged ≥ 12 even when a saved result exists, in both `Years` and `Months` age units; proven by behavioural tests.
3. **REF-092 is closed.** Commercial name is captured, validated, stored, projected, and editable, and survives a create→read→update round trip; proven by tests at Domain, Application and (where runnable) Presentation level.
4. **Arithmetic.** `3 functions = 2 NO_MIGRATION + 1 NEW_MIGRATION`; exactly **1** new migration created; **0** existing migrations edited.
5. **Schema integrity.** After the migration, `Antibiotics` has exactly one new column: `CommercialName nvarchar(150) NULL`. No table, key, index, or constraint was added, dropped, or altered.
6. **Regression guards intact.** `AddAntibioticMasterFieldsMigrationTests` (5 facts) is byte-identical and passing.
7. **Build.** `dotnet build TopLab.sln -c Release -p:EnableWindowsTargeting=true` completes with **0 errors and 0 warnings**.
8. **Tests.** `TopLab.Domain.Tests`, `TopLab.Application.Tests`, `TopLab.Infrastructure.Tests`, `TopLab.Persistence.Tests` and `TopLab.Presentation.Tests` show **no new failures** relative to the Windows baseline in §B (which is 0 failed everywhere; the Linux Arial font failures do not occur on this machine — Arial present, 349/0/0 at baseline).
9. **No scope creep.** `git diff` for the phase touches only the files named in §D.1.4, §D.2.4, §D.3.4, plus the one new migration, its `.Designer.cs`, the regenerated snapshot, and new/extended test files.
10. **Commits.** Exactly **three** local commits on `main`, one per function, in the order REF-093 → REF-094 → REF-092, and **no push** to any remote.
11. **No excluded function is touched.** The 25 owner-excluded REF identifiers are not implemented, referenced as work, or counted as gaps anywhere in this phase.
12. **Honest reporting.** Every deviation, every unverifiable acceptance criterion, and every environment limitation is reported rather than papered over.

---

## G. Phase-Level Blocked Functions List

**None.**

All three functions are `READY_AUTONOMOUS_DECISIONS_RECORDED`. No function is `BLOCKED`.

The PHASE 0 item in this neighbourhood (**REF-209**, per-section sample coding) was assessed in §D.1.8 decision 4, §D.2.8 decision 3 and §D.3.8 decision 6, and determined **not to touch any Phase 1 function**. It is carried forward unresolved with no Phase 1 impact. It is not a blocker and must not be implemented in this phase.

---

## H. Risks and Dependencies

| # | Risk | Type | Severity | Likelihood | Mitigation / instruction |
|---|---|---|---|---|---|
| R1 | **The obvious shortcut breaks two existing tests.** Adding `CommercialName` to `AddAntibioticMasterFields` satisfies the schema need but breaks `AddsExactlyTwoColumns` and `ContainsNoCommercialNameColumn`. | Technical | High | Medium | **Prohibited explicitly** in §D.3.6 and §D.3.4. A new migration is the only compliant route. If the Build Agent is tempted to edit that test file, it must not. |
| R2 | **Over-correction in the predicate fix.** Removing the escape clause could, if done carelessly, also hide ordinary (unflagged) antibiotics. | Clinical | High | Low | Acceptance criteria in §D.1.7 (7) and §D.2.7 (5) explicitly require a regression test proving unflagged antibiotics remain visible. |
| R3 | **`TopLab.Presentation.Tests` targets `net8.0-windows`.** (Cloud/Linux note — historical: it could not run on Linux, so the editor-VM test was unverifiable there.) | Environment | Low | **N/A on this machine** | On this Windows machine it **runs and is a required gate** (baseline 171/0/0). Never report it as passed without running it. |
| R4 | **PDF-render tests depend on the Arial font.** (Cloud/Linux note — historical: 28 Infrastructure tests failed on missing Arial.) | Environment | Low | **None observed on this machine** | On this Windows machine Arial is present and all 349 Infrastructure tests pass at baseline. Any such failure after implementation is NEW and must be investigated, not dismissed as environmental. |
| R5 | **Snapshot hand-editing.** The model snapshot is tooling-generated; a manual edit is a common and damaging shortcut. | Technical | High | Medium | Prohibited in §D.3.6. Regenerate via `dotnet ef migrations`. |
| R6 | **New optional parameters must be appended last.** Inserting `CommercialName` before existing optional parameters silently changes positional call sites across the codebase. | Technical | High | Medium | §D.3.4 requires appending to the end of every record/parameter list. A full build is the detector; do not rely on review alone. |
| R7 | **The `saved` dictionary could be removed by mistake.** After removing the disjunct, `saved` is still needed by the projection on line 38. | Technical | Medium | Low | Explicitly called out in §D.1.4. A removed-but-still-referenced variable breaks the build; a removed-and-unreferenced one is harmless. Either way the build catches it. |
| R8 | **The two predicate functions are one change, but two commits are required.** A reviewer may expect REF-094's commit to contain a production change. | Process | Low | High | Explained in §D.2.4. The REF-094 commit is test-only by design. |
| R9 | **Scope creep into the "International name" column** visible on LEARN p.135. | Scope | Low | Medium | Excluded in §D.3.8 decision 3. |
| R10 | **Scope creep into the printed culture report.** The commercial name is master data; whether it prints is a different REF. | Scope | Low | Medium | Excluded in §D.3.3. |
| R11 | **The stale `AntibioticEditorViewModel` comment (C-1) is the only `Abbreviation` hit in `src/`.** Correcting it removes a token that a future negative search for REF-218 may rely on. | Documentation | Low | Low | Worth doing (it is actively misleading), but the consequence must be recorded: a future REF-218 absence search must not depend on a stale comment. Noted here so the finding is not lost. |
| R12 | **EF tooling and database access on this machine.** `dotnet ef` 8.0.30 is installed; SQL Server Express `.\SQLEXPRESS` is reachable; Docker is absent (the 2 Docker-gated Persistence tests skip by design). | Environment | Low | Low | Create the migration with `dotnet ef migrations add` only (never hand-written, never hand-edit the snapshot). Follow the §J database rule for applying vs files-only. (Cloud note — historical: no SQL Server or Docker, tooling unrunnable.) |
| R13 | **An existing handler test encodes the defect and will turn red on the fix.** `Grid_StrictlyFiltersChildrenButKeepsSavedFacts` (`CultureResultQueryHandlerTests.cs:17-28`) asserts both rows for a 12-year-old with a saved children-flagged result — the DEF-001 outcome. | Technical | Medium | **Certain** (fails as soon as `:37` changes) | Update its expectation in the REF-093 commit per §D.1.4 (expected change, not a regression). Do not "fix" it by restoring the disjunct; do not delete it. |
| R14 | **A hidden saved row is deleted by the next entry-screen save (not kept dormant), and still prints until then.** Verified: the entry screen submits displayed rows only (`CultureEntryViewModel.cs:264-278,326-346`); the save-sync removes unsubmitted saved rows (`SaveCultureResultsCommandHandler.cs:5`); the culture report (`GetCultureReportQueryHandler.cs:31,41-45`) and combined report (`BuildCombinedReportCommandHandler.cs:115-120`) read saved rows with no `IsDisplayable` filter. For reviewed/printed/delivered cultures the row persists and keeps printing (saves blocked). | Clinical/Data | Medium | **Certain** on next save | **Owner-visible and accepted for Phase 1 because the application has not been used in production yet** — no real clinical data is at risk. Do not add a purge, a print filter, or extra tests for this; if production data existed, stop and report instead. |

**Cross-function dependencies (phase-internal):** REF-093 → REF-094 (same line, same change); REF-093, REF-094 → REF-092 (shared aggregate, sequenced last for a contiguous edit). No dependency on any function outside this phase.

**External dependencies:** none. This phase depends on no other phase, no excluded capability, no new package, and no hardware.

---

## I. Evidence Index

| # | Claim | Exact evidence |
|---|---|---|
| E1 | Predicate defect is the call-site `\|\|`, not the rule | `src/TopLab.Application/Features/CultureResults/Queries/GetCultureEntryGrid/GetCultureEntryGridQueryHandler.cs:37` |
| E2 | Rule engine is correct | `src/TopLab.Application/Features/CultureAndAntibiotics/Common/CultureAntibioticDisplay.cs:7-16` |
| E3 | Child threshold is 12, matching the reference | `CultureAntibioticDisplay.cs:5,15`; `GetCultureEntryGridQueryHandler.cs:33` |
| E4 | Reference requires unconditional visibility | `RLS_Learn_Enhanced.pdf` p.139 — `يظهر … فقط` (twice) |
| E5 | Reference states age < 12 | `RLS_Learn_Enhanced.pdf` p.139 |
| E6 | Handler coverage except the defect arm; one fact encodes the defect | No file *named* `*CultureEntryGrid*`, but the handler is exercised by `CultureResultQueryHandlerTests.cs:17-28` (`Grid_StrictlyFiltersChildrenButKeepsSavedFacts` asserts the defective outcome; updated in the REF-093 commit) — see §D.1.2 Claim 5 |
| E7 | Rule engine is fully truth-table tested (behavioural) | `tests/TopLab.Application.Tests/Features/CultureAndAntibiotics/CultureAntibioticDisplayTests.cs:38,55,71,81` |
| E8 | Save path validates attachment, not display | `src/TopLab.Application/Features/CultureResults/Commands/SaveCultureResults/SaveCultureResultsCommandHandler.cs` |
| E9 | `Antibiotic` has no commercial-name member | `src/TopLab.Domain/Tests/Antibiotic.cs:8-21` |
| E10 | EF config maps no commercial name | `src/TopLab.Infrastructure/Persistence/Configurations/AntibioticConfiguration.cs:12-18` |
| E11 | `Antibiotics` table created without it | `20260828052248_BaselineDataModel.cs:17-29` |
| E12 | Only later migration adds `Symbol` + `ScientificName` | `20261002002546_AddAntibioticMasterFields.cs:13-25` |
| E13 | Snapshot declares no commercial-name property | `ApplicationDbContextModelSnapshot.cs`, `modelBuilder.Entity("TopLab.Domain.Tests.Antibiotic", …)` |
| E14 | `CommercialName`/`TradeName` appear nowhere in `src/` | `Get-ChildItem -Path src -Recurse -Include *.cs \| Select-String -Pattern 'CommercialName','TradeName'` → 0 hits (run on this machine) |
| E15 | Reference documents a commercial-name column | `RLS_Learn_Enhanced.pdf` p.135 (`الاسم التجاري`), p.138 (column set) |
| E16 | Exact template for the vertical slice | `Antibiotic.cs:15,18,20-22,28,38,45,48,58-59,62-76`; `AntibioticDtos.cs:8-9,16-17`; `AntibioticEditorViewModel.cs:24-25,40-41,53-54,65-66,79-89`; `AntibioticEditorWindow.xaml:19,21` |
| E17 | **Existing tests forbid editing the old migration** | `tests/TopLab.Infrastructure.Tests/Persistence/Migrations/AddAntibioticMasterFieldsMigrationTests.cs:13,45-49,51-60` |
| E18 | Stale VM comment (audit C-1) | `src/TopLab.Presentation/ViewModels/Lab/AntibioticEditorViewModel.cs:10-13` |
| E19 | `AttachedAntibioticDto` mirrors the master DTO | `src/TopLab.Application/Features/CultureAndAntibiotics/Common/AntibioticDtos.cs:11-22` |
| E20 | Commands already carry `EDIT_SYSTEM_SETTINGS` | `CreateAntibioticCommand.cs:15`; `UpdateAntibioticCommand.cs:16` |
| E21 | Baseline toolchain | `Directory.Packages.props` (EF Core 8.0.30); `dotnet --list-sdks` (8.0.425, 9.0.318 on this machine; 9.0.316 historical); `Get-ChildItem -Path src, tests -Filter *.csproj -Recurse \| Select-String -Pattern '<TargetFramework>([^<]+)</TargetFramework>'` (run on this machine) |
| E22 | Migration test convention to mirror | `AddAntibioticMasterFieldsMigrationTests.cs:8-21` (reflection-based `Operations(bool up)` helper) |

---

## J. Build Agent Entry Contract

**You are the Build Agent for Phase 1 — Culture antibiotic correctness of Top-Lab.**

1. **This plan is the authoritative execution input for Phase 1.** It is the plan of record. **Do not write a new plan and do not replace it.** If you believe it is wrong, follow §J.6 — report, do not re-plan.

2. **Read this document in full before doing anything.** Every decision you need is here. You are expected to be able to execute all three functions from this document plus the current repository, with no other context.

3. **Targets, in this exact order:**
   - **Function 1 — REF-093** Antibiotic suitable for pregnancy → `READY_AUTONOMOUS_DECISIONS_RECORDED`
   - **Function 2 — REF-094** Antibiotic suitable for children under twelve → `READY_AUTONOMOUS_DECISIONS_RECORDED`
   - **Function 3 — REF-092** Antibiotic master data with commercial name → `READY_AUTONOMOUS_DECISIONS_RECORDED`

4. **Before the first edit, re-verify §E.3.** Record your true baseline HEAD. If any of the five facts has changed, **stop and report**.

5. **Targeted verification is permitted; re-planning is not.** You may open any file, re-read any cited line, and confirm any symbol. You may **not** reorder the functions, drop a function, split a function differently, add a function, change a migration decision, or substitute a different design. The decisions in §D.1.8, §D.2.8 and §D.3.8 are settled.

6. **Two hard prohibitions, both load-bearing:**
   * **Never** edit, regenerate, or delete any existing migration, any existing `.Designer.cs`, or the existing `AddAntibioticMasterFieldsMigrationTests.cs`. See R1 and §D.3.6.
   * **Never** hand-write a migration or hand-edit the model snapshot. Use the EF tooling. If it cannot run, **stop and report** (R12).

7. **Both gates must pass before a function's commit is created.** For every function:
    * **Gate 1 — build:** `dotnet build TopLab.sln -c Release -p:EnableWindowsTargeting=true` completes with **0 errors and 0 warnings**. (The `-p:EnableWindowsTargeting=true` property is harmless on Windows and is kept so the command stays identical; it touches no file. This exact command was run on this machine: 0/0.)
    * **Gate 2 — tests:** `TopLab.Domain.Tests`, `TopLab.Application.Tests`, `TopLab.Infrastructure.Tests`, `TopLab.Persistence.Tests` **and `TopLab.Presentation.Tests`** show **no new failures** versus the Windows pre-implementation baseline in §B. `TopLab.Presentation.Tests` **runs on Windows — it is a required gate** (171/0/0 at baseline); never report it as "not run" here, and never report it as passed without running it.
    * No `DOTNET_ROLL_FORWARD` is needed on this machine (.NET 8 runtime present).

8. **Commit discipline (owner decision).** After both gates pass for a function, create **exactly one LOCAL commit for that function** on the **current branch only** (`main`). Three functions → three commits, in the order REF-093 → REF-094 → REF-092. Use a message naming the REF, e.g. `[Phase 1] REF-093: Antibiotic suitable for pregnancy`. Note that the REF-094 commit is **test-only by design** (§D.2.4, R8). `git commit --amend` is FORBIDDEN. Any push to any remote is FORBIDDEN. The prohibitions on branch creation/switching, checkout, reset, revert, rebase, stash and clean stay in force (§J.9, §J.10).

9. **Do NOT push to any remote.** No remote operation of any kind.

10. **No branch creation, no branch switching, no checkout of any other commit, no reset, revert, rebase, stash, or clean.**

11. **Do not implement any of the 25 owner-excluded functions**, and do not count any of them as a gap. This phase touches none of them.

12. **Do not implement REF-209** or anything else outside the three functions in §J.3. It was assessed and explicitly deferred (§G).

13. **Report honestly.** State, per function: what you changed, which gate results you observed, which acceptance criteria you met, which you could not verify and **why**, and any deviation you made. Never claim a test passed unless it ran and passed. Never claim database behaviour without performing it. If something is blocked, say so and leave the tree diagnosable.

14. **Do not modify this plan file.** It is the plan of record. Report corrections in your final message instead.

15. **Migrations — at most one (owner decision).** Create at most ONE new migration (`AddAntibioticCommercialName`) and only with `dotnet ef migrations add` — never hand-written, never hand-edit the snapshot (see §J.6). If more than one migration seems necessary, **stop and report** before creating it.

16. **Database rule — locate, check pending, then decide, in this order (owner decision).**
    1. Locate the connection string(s) in use and state the server/database (redact passwords).
    2. If a SQL Server is reachable: run `dotnet ef migrations list` and check `__EFMigrationsHistory` against the target database. If ANY earlier migration is pending, create the new migration FILES ONLY and apply NOTHING (migrations apply in order, all or none; never apply the new one while an older one is pending).
    3. If NO migration is pending AND the server is local (`localhost`, `.`, `.\SQLEXPRESS`, or LocalDB): apply the new migration, then verify with `SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Antibiotics'` (expect `CommercialName | nvarchar | 150 | YES`), `SELECT COUNT(*) FROM Antibiotics WHERE CommercialName IS NOT NULL` (expect 0), and an unchanged row count. If the server is not local, stop and report.
    4. If no SQL Server is reachable or there is ANY doubt: create the migration files only, apply nothing, and still report whether any older migration is pending.
    5. Permitted extra check: validate the migration on a FRESH disposable database (e.g. `TopLab_Phase1Verify`) created and dropped within the session; never touch `localdb\TopLab` or any pre-existing database for trials.
    *Informational, observed on this machine at verification: `localdb\TopLab` showed 6 pending migrations, so rule 2 applies to it (files only).*

---

*End of Phase 1 Implementation Plan. Produced in analysis-and-planning mode against HEAD `6fa10aabcb93d4ea6f49dba00423d7f89fc1b11d`. No product file, source file, test file, migration, snapshot, configuration, or audit report was created or modified in producing it.*
