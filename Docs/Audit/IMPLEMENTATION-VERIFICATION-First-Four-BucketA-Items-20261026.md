# Implementation Verification — First Four Bucket A Items

**Report date:** 2026-10-26
**Repository:** `C:\Users\LAP LINK\source\repos\Top-Lab`
**Branch:** `main`
**HEAD commit:** `b3c46611b1e28fed1f9d2b648e51b2e39841a546`
**Report file:** `Docs/Audit/IMPLEMENTATION-VERIFICATION-First-Four-BucketA-Items-20261026.md`

---

## Section 1 — Audit Scope and Method

### Audit objective
Independently verify, by code inspection, that the four most-recently-implemented Bucket A items from the Stage R1–R6 classification audit (GAP-22, GAP-06, GAP-07, GAP-08) were actually implemented correctly in the repository — without relying on the implementation report's claims. The owner will not run the WPF application, so verification rests entirely on static code evidence.

### Repository root, branch, HEAD commit
- Root: `C:\Users\LAP LINK\source\repos\Top-Lab`
- Branch: `main`
- HEAD: `b3c46611b1e28fed1f9d2b648e51b2e39841a546`

### Audit timestamp
2026-10-26 (session start).

### Read-only methodology
This audit performed **no implementation, no file modification, no migration, no seed change, no build, no test execution, no commit, and no push**. The only operations performed were: reading files, `git status`, `git diff --stat`, `git diff`, `grep`, and repository content search. The four GAP implementations under audit are present in the **working tree as uncommitted changes** (see Section 10) — they were NOT committed at `b3c4661`, and this audit did not stage or commit them.

### Classification report used
`Docs/Audit/StageR1-R6-Unimplemented-Items-Classification-Audit-20261026-FINAL-VERIFIED-R1.md` (Section 3.1 Bucket A rows for GAP-22, GAP-06, GAP-07, GAP-08).

### Important version note
The classification report was authored against a **clean** tree at `b3c4661`, where these four items were recorded as *remaining work* (not yet implemented). At the time of this audit the same HEAD `b3c4661` is checked out, but the four items now exist as **uncommitted working-tree modifications**. The audit therefore inspects the working-tree state (the actual current code on disk), not the committed `b3c4661` blob.

---

## Section 2 — Executive Summary

| Item | Stage | Reference ID(s) | Verdict |
|---|---|---|---|
| GAP-22 | R1 | `1.22.12.2/3/4`, `1.6.1.3.2` | **VERIFIED** |
| GAP-06 | R3 | `1.10.1.1.7` | **VERIFIED** |
| GAP-07 | R3 | `1.10.1.1.14` | **VERIFIED** |
| GAP-08 | R3 | `1.10.1.1.1`, `1.10.1.1.2` | **VERIFIED** |

### Counts
- Total items inspected: **4**
- VERIFIED: **4** (GAP-22, GAP-06, GAP-07, GAP-08)
- PARTIALLY_VERIFIED: **0**
- NOT_VERIFIED: **0**
- UNDETERMINED: **0**

### Correction note (re-audit)
An earlier pass reported GAP-06 as PARTIALLY_VERIFIED on the basis that the "رقم البطاقة" (Card Number) input was absent from `PatientSearchView.xaml`. That conclusion was **wrong**: it was drawn from a stale in-memory copy of the file read before the implementer's edit was flushed to disk. Re-reading the file from disk and inspecting `git diff` shows the input **is present** (`PatientSearchView.xaml:12-13`) and the compiled BAML also contains it. GAP-06 is therefore VERIFIED. The corrected finding is recorded below.

---

## Section 3 — GAP-22 Verification Report (Print/Preview/Review on Profile & Culture Screens)

### Acceptance criteria (classification report, Section 3.1, GAP-22 row)
Extend the existing print/preview/review actions from the single-test screen (`SimpleResultEntryViewModel`) to the **profile** and **culture** result-entry screens, reusing the same Application handlers. No schema change, no owner decision outstanding (OD-8 resolved in Round 4).

### Code evidence

**ProfileEntryViewModel (`src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs`)**
- `PreviewCommand` declared line **130** (constructed) and **209** (property).
- `ReviewCommand` declared line **131** and **210**.
- `PreviewAsync` lines **420–448**: guarded by `if (_patientTestId == 0) return;` (line 422); sends `GetProfileReportQuery` (line 432) then calls `PreviewProfilePdfAsync` (line 436). **Reuses the existing `PreviewProfilePdfAsync` path** (lines 510–535), which is the same path `ShowReportAsync` uses.
- `ReviewAsync` lines **450–476**: guarded (line 452); dispatches `ReviewResultCommand(_patientTestId)` via `_mediator.Send` (line 462) — i.e. **ReviewResultCommand via mediator**.
- `PrintCommand` still present (constructed line 125, property line 207) and `PrintAsync` (376–418) still works via `_printCoordinator`.
- `ShowReportCommand` still present (constructed line 126, property line 208); `ShowReportAsync` (478–504) unchanged.

**CultureEntryViewModel (`src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs`)**
- `PreviewCommand` declared line **123** and **222**.
- `ReviewCommand` declared line **124** and **223**.
- `PreviewAsync` lines **515–542**: guarded (line 517); sends `GetCultureReportQuery` (line 527) and sets `Report = result.Value` (line 530). **Reuses the `ShowReportAsync`/`GetCultureReportQuery → Report` path** (compare `ShowReportAsync` lines 572–597).
- `ReviewAsync` lines **544–570**: guarded (line 546); dispatches `ReviewResultCommand(_patientTestId)` via `_mediator.Send` (line 556) — **ReviewResultCommand via mediator**.
- `IPdfPreviewService` is **NOT** injected in this VM (constructor lines 104–109 take only `mediator, presenter, dialogs, printCoordinator, printedState`; no `_pdfPreview` field). Consistent with the owner decision that the reuse path needs no PDF-preview service.
- `PrintCommand` still present (line 120 / 220); `ShowReportCommand` still present (line 121 / 221).

**ProfileEntryView.xaml (`src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml`)**
- "معاينة" (Preview) button line **142**, bound `Command="{Binding PreviewCommand}"`.
- "مراجعة" (Review) button line **143**, bound `Command="{Binding ReviewCommand}"`.

**CultureEntryView.xaml (`src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml`)**
- "معاينة" (Preview) button line **217**, bound `Command="{Binding PreviewCommand}"`.
- "مراجعة" (Review) button line **218**, bound `Command="{Binding ReviewCommand}"`.

### Verification verdict: **VERIFIED**

### Detailed findings
1. Profile VM has both PreviewCommand and ReviewCommand — yes.
2. Profile PreviewCommand reuses `PreviewProfilePdfAsync` (the existing PDF-preview path) — yes.
3. Profile ReviewCommand dispatches `ReviewResultCommand` via mediator — yes.
4. Culture VM has both PreviewCommand and ReviewCommand — yes.
5. Culture PreviewCommand reuses `ShowReportAsync` path (`GetCultureReportQuery → Report`) — yes.
6. Culture ReviewCommand dispatches `ReviewResultCommand` via mediator — yes.
7. `IPdfPreviewService` NOT injected into Culture VM — confirmed (owner decision satisfied).
8. Both screens' commands are guarded (`_patientTestId == 0` early return) — yes.
9. Existing `PrintCommand` and `ShowReportCommand` preserved on both screens — yes.
10. Both XAML views contain the "معاينة" and "مراجعة" buttons, correctly bound — yes.

### Risks / deviations
None material. The Culture `PreviewAsync` sets the `Report` property (populating the on-screen report preview panel) but, unlike the Profile `PreviewAsync`, does not invoke a PDF preview dialog — this is the intended reuse of the `ShowReportAsync`/`Report` path per owner decision Q1, not a defect.

### Owner decision Q1 compliance
See Section 7 — **COMPLIANT**.

---

## Section 4 — GAP-06 Verification Report (CardNumber Exact Search)

### Acceptance criteria (classification report, Section 3.1, GAP-06 row)
Add a card-number search criterion: search patients by **exact card number** (reference `1.10.1.1.7`). `CardNumber` already exists on `Patient`; this is application logic over an existing column — no schema change, no owner decision.

### Code evidence

**SearchPatientsGlobalQuery (`src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs`)**
- `string? CardNumber = null` parameter at line **43**, appended at the tail after `From`/`To` (line **42–43**). Optional and defaulted — inert when null.

**SearchPatientsGlobalQueryHandler (`src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQueryHandler.cs`)**
- Guard clause `if (!string.IsNullOrWhiteSpace(request.CardNumber))` at line **49**.
- Exact trimmed match: `query = query.Where(p => p.CardNumber != null && p.CardNumber.Trim() == cardNumber);` lines **51–52** (where `cardNumber = request.CardNumber.Trim()`, line 51).
- Skips the filter when CardNumber is null/empty/whitespace (guard at line 49).
- Combined with other filters via **AND** logic (query reassignment with `.Where`, line 52, after the term clause at lines 40–46).

**PatientSearchViewModel (`src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs`)**
- `CardNumberText` property at lines **113–117** (backed by `_cardNumberText`, line 31).
- In `SearchAsync`, `cardNumber` derived from `CardNumberText` (line **564**: `string.IsNullOrWhiteSpace(CardNumberText) ? null : CardNumberText.Trim()`) and passed as the `CardNumber` argument to `SearchPatientsGlobalQuery` (line **596**).

**PatientSearchView.xaml (`src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml`)**
- "رقم البطاقة" (Card Number) text input present at lines **12–13**: a `<TextBlock Text="رقم البطاقة:" .../>` label (line 12) and `<TextBox Text="{Binding CardNumberText, UpdateSourceTrigger=PropertyChanged}" Width="120" .../>` (line 13), bound to the `CardNumberText` property.
- **Re-audit correction:** an earlier pass of this audit wrongly reported this control as absent. That was based on a stale in-memory copy of the file read before the implementer's edit reached disk. Re-reading the file from disk and inspecting `git diff` (which shows the added lines 12–13) and the compiled BAML (`PatientSearchView.baml`, which contains `CardNumberText`) confirms the control **is present and correctly bound**.

### Verification verdict: **VERIFIED**

### Detailed findings
1. Query has `CardNumber` parameter — yes (line 43).
2. Parameter is optional (`string? CardNumber = null`) — yes.
3. Appended at the tail of the record parameters — yes (line 43, after `To`).
4. Handler has a guard clause for CardNumber — yes (line 49).
5. Handler performs exact trimmed match `p.CardNumber.Trim() == request.CardNumber.Trim()` — yes (lines 51–52).
6. Handler skips the filter when CardNumber is null/empty/whitespace — yes (line 49).
7. Handler combines with other filters using AND logic — yes.
8. `PatientSearchViewModel` has a `CardNumberText` property — yes (lines 113–117).
9. `PatientSearchViewModel.SearchAsync` passes `CardNumberText` to the query — yes (lines 564, 596).
10. `PatientSearchView.xaml` contains a "رقم البطاقة" text input bound to `CardNumberText` — **yes (lines 12–13); corrected from an earlier erroneous "missing" finding.**

### Risks / deviations
None material. Full end-to-end wiring is present: XAML input → `CardNumberText` property → `SearchAsync` trim/null-coalesce → `SearchPatientsGlobalQuery.CardNumber` → handler exact-trimmed AND predicate. No schema change, no migration, no seed change (confirmed in Section 10), consistent with the Bucket A classification.

---

## Section 5 — GAP-07 Verification Report (Not-Delivered Filter)

### Acceptance criteria (classification report, Section 3.1, GAP-07 row)
Add a not-delivered criterion: return only patients whose results are undelivered (reference `1.10.1.1.14`). `IsDelivered` already exists; this is a tri-state predicate over an existing column — no schema change, no owner decision.

### Code evidence

**SearchPatientsGlobalQuery (`SearchPatientsGlobalQuery.cs`)**
- `bool? NotDelivered = null` parameter at line **45**, appended at the tail (after `CardNumber`). Optional and defaulted — inert when null.

**SearchPatientsGlobalQueryHandler (`SearchPatientsGlobalQueryHandler.cs`)**
- Guard clause `if (request.NotDelivered == true)` at line **56**.
- Filters patients with at least one undelivered `PatientTest`: `query = query.Where(p => _db.Set<PatientTest>().Any(pt => pt.PatientId.Value == p.Id.Value && !pt.IsDelivered));` line **58**.
- Because `Any(...)` requires a matching `PatientTest` row, **patients with no tests are excluded** when the filter is active (an `Any` over an empty set is false).
- Combined with other filters via AND logic (line 58, after the term and card-number clauses).

**PatientSearchViewModel (`PatientSearchViewModel.cs`)**
- `NotDelivered` property at lines **119–123** (backed by `_notDelivered`, line 32).
- In `SearchAsync`, passed to the query as `NotDelivered ? true : (bool?)null` at line **597** (line **597**), so an unchecked box sends null (inert).

**PatientSearchView.xaml (`PatientSearchView.xaml`)**
- "غير مُسلَّم" (Not Delivered) checkbox at line **66**: `<CheckBox Content="غير مُسلَّم" IsChecked="{Binding NotDelivered}" ... />`, bound to the `NotDelivered` property.

### Verification verdict: **VERIFIED**

### Detailed findings
1. Query has `NotDelivered` parameter — yes (line 45).
2. Optional (`bool? NotDelivered = null`) — yes.
3. Appended at the tail — yes (line 45).
4. Handler guard clause — yes (line 56).
5. Filters patients with at least one undelivered `PatientTest` — yes (line 58).
6. Uses `_db.Set<PatientTest>().Any(pt => pt.PatientId.Value == p.Id.Value && !pt.IsDelivered)` — yes, verbatim match (line 58).
7. Excludes patients with no tests when filter active — yes (via `Any` semantics).
8. Combined with other filters via AND — yes.
9. VM has `NotDelivered` property — yes (lines 119–123).
10. VM passes `NotDelivered` to the query — yes (line 597).
11. XAML contains "غير مُسلَّم" checkbox bound to `NotDelivered` — yes (line 66).

### Risks / deviations
None material. The tri-state semantics are correct: unchecked → null → inert; checked → true → filter applied.

### Owner decision Q3 compliance
See Section 7 — **COMPLIANT**.

---

## Section 6 — GAP-08 Verification Report (Exact-Name + Default-On)

### Acceptance criteria (classification report, Section 3.1, GAP-08 row)
Exact-name search criterion (reference `1.10.1.1.1`) and name search **enabled by default** (not gated off) (reference `1.10.1.1.2`). Partial-name search previously existed but was gated off by default; exact-name is a separate predicate. Behaviour-only change over the existing name column — no schema change. OD-7 affects design only.

### Code evidence

**SearchPatientsGlobalQueryHandler (`SearchPatientsGlobalQueryHandler.cs`)**
- Default changed from `?? false` to `?? true`: `var nameAssist = _db.Set<SystemSettings>().SingleOrDefault()?.EnablePatientNameSearchAssist ?? true;` line **25**.
- Exact-name predicate, separate from the partial (Contains) predicate: `|| p.FullName.ToLowerInvariant() == lowerTerm` at line **42**, combined with the partial-name clause and the other text clauses using **OR** logic inside a single `p => (...)` predicate (lines **40–45**).
- Partial (Contains) path is gated by `nameAssist`: `(nameAssist && p.FullName.ToLowerInvariant().Contains(lowerTerm))` line **41** — so the assist flag still controls only the *substring* path.
- Explicit `false` is still respected: when `EnablePatientNameSearchAssist == false`, `nameAssist` is false, the Contains clause is disabled, but the exact-name clause (line 42) still matches.

**SystemSettings (`src/TopLab.Domain/Settings/SystemSettings.cs`)**
- `EnablePatientNameSearchAssist` property at line **19**.
- The **seed/default value remains `false`** at line **65** (`EnablePatientNameSearchAssist = false,`). This was **not** changed — consistent with owner decision Q2 (code-level default only; seed data untouched, no migration).

**Test updates**

`SearchGuardrailTests.cs` (`tests/TopLab.Application.Tests/Features/PatientSearch/SearchGuardrailTests.cs`)
- Test `SearchGuardrail_Text_NameAssistDefaultsOff_WhenNoSettingsRowExists` (lines **374–387**) was updated: with no `SystemSettings` row present, it now asserts `Assert.Single(result.Value!)` (line **386**), documenting that name search is enabled by default (code-level `?? true`) and exact-name match returns the patient. Comment at lines 384–385 records the GAP-08 change.

`SearchPatientsGlobalQueryHandlerTests.cs` (`tests/TopLab.Application.Tests/Features/PatientSearch/SearchPatientsGlobalQueryHandlerTests.cs`)
- Test `Search_ByNameIgnored_WhenAssistDisabled_MissingSettingsDefaultsOff` (lines **88–101**) was updated: with no settings row, it now asserts `Assert.Single(result.Value!)` (line **100**), with the comment (lines 98–99) noting exact-name match returns the patient even without `SystemSettings`.

**Do the updated tests still pass?** The implementation report claims 3,564 passed / 0 failed / 2 skipped. This audit did **not** run the tests (read-only constraint), so test-pass status is taken from the report and **not independently re-verified**. The updated test assertions are internally consistent with the `?? true` code change, so no contradiction with the code is evident.

### Verification verdict: **VERIFIED**

### Detailed findings
1. Handler has an exact-name predicate separate from partial-name — yes (line 42 vs line 41).
2. Uses `p.FullName.ToLowerInvariant() == lowerTerm` — yes (line 42).
3. Combined with other text-search predicates using OR logic — yes (lines 40–45).
4. Default changed from `?? false` to `?? true` — yes (line 25).
5. Still respects explicit `EnablePatientNameSearchAssist = false` — yes (Contains gated by `nameAssist`, line 41; exact-name remains, line 42).
6. `SearchGuardrailTests.cs` updated for default-on — yes (lines 374–387).
7. `SearchPatientsGlobalQueryHandlerTests.cs` updated — yes (lines 88–101).
8. Updated tests pass — claimed by report; not independently re-run (read-only).

### Risks / deviations
- **Behavioural note (by design):** with the code-level default now `true` and the seed default still `false`, a database whose `SystemSettings` row has `EnablePatientNameSearchAssist = false` will still match on *exact* name but not on *substring*. This is the intended GAP-08 behaviour and matches owner decision Q2.

### Owner decision Q2 compliance
See Section 7 — **COMPLIANT**.

---

## Section 7 — Owner Decisions Compliance Summary

### Q1 (GAP-22 Preview on Culture) — **COMPLIANT**
- **No `ICultureResultPreviewBuilder` port created.** A repository search for `ICultureResultPreviewBuilder` / `CultureResultPreviewBuilder` under `src/` returned **0 matches**. Confirmed not created.
- **Existing `ShowReportAsync` path reused.** Culture `PreviewAsync` (lines 515–542) sends `GetCultureReportQuery` and sets `Report`, exactly mirroring `ShowReportAsync` (lines 572–597). No new builder was introduced.
- **`IPdfPreviewService` NOT injected.** Culture `CultureEntryViewModel` constructor (lines 104–109) does not take `IPdfPreviewService`; no `_pdfPreview` field exists. The Profile VM does inject it (line 75/106) because its preview path calls `PreviewProfilePdfAsync`, which is correct and unchanged.

### Q2 (GAP-08 default-on) — **COMPLIANT**
- **Code-level default changed to `?? true`.** `SearchPatientsGlobalQueryHandler.cs:25`.
- **Seed data NOT changed.** `SystemSettings.cs:65` still initialises `EnablePatientNameSearchAssist = false`.
- **No migration created.** `git diff --name-only` shows no migration files among the modified set (confirmed in Section 10).
- **Still respects explicit `false`.** The substring path is gated by `nameAssist` (line 41); explicit false disables only substring, exact-name remains (line 42).

### Q3 (GAP-07 not-delivered) — **COMPLIANT**
- **Returns patients with at least one undelivered test.** `SearchPatientsGlobalQueryHandler.cs:58` (`Any(... !pt.IsDelivered)`).
- **Excludes patients with no tests.** `Any` over an empty set is false, so test-less patients are filtered out when the flag is active.
- **Combines with other filters via AND.** Line 58 reassigns `query` with `.Where`, conjoined with the prior clauses.

---

## Section 8 — Risks, Warnings, and Deviations

| # | Severity | Area | Description |
|---|---|---|---|
| 1 | Low / by-design | GAP-08 dual default | Code default `?? true` (handler line 25) vs seed default `false` (`SystemSettings.cs:65`). This is the intended GAP-08/Q2 behaviour, not a defect, but worth noting: substring name search remains off unless the setting is explicitly enabled, while exact-name search is always on. |
| 2 | Informational | Test execution | The 3,564-pass claim was **not independently re-run** by this audit (read-only constraint). The updated test assertions are consistent with the code, but test-pass status relies on the implementation report. |
| 3 | Informational | Culture preview vs profile preview | Culture `PreviewAsync` populates the on-screen `Report` panel only; it does not open a PDF preview dialog (it reuses the `ShowReportAsync`/`Report` path per Q1). Profile `PreviewAsync` additionally calls `PreviewProfilePdfAsync`. This is consistent with the owner decision but means the two screens' "معاينة" buttons have slightly different visible behaviour. |
| 4 | Informational | Audit process note | An earlier audit pass reported GAP-06 as PARTIALLY_VERIFIED due to reading a stale in-memory copy of `PatientSearchView.xaml`. The re-audit corrected this to VERIFIED (see Section 2 correction note and Section 5). No code defect was involved. |

### Test-coverage observations
- GAP-06 has **no dedicated handler test** for the card-number predicate (the modified test files added only GAP-08 default-on assertions). The predicate is simple and guarded, but a unit test for exact card-number matching would harden it.
- GAP-07 has **no dedicated handler test** for the not-delivered filter (no `PatientTest`/`IsDelivered` fixture in the modified files). The `Any(!IsDelivered)` semantics are non-trivial (excludes test-less patients), so a unit test is advisable.

---

## Section 9 — Final Verification Checklist

- [x] All four items inspected (GAP-22, GAP-06, GAP-07, GAP-08).
- [x] Code evidence provided for each, with file paths and line numbers.
- [x] Line numbers verified by direct file read (not inferred).
- [x] Owner decisions Q1, Q2, Q3 checked and evidence recorded (Section 7).
- [x] No implementation performed.
- [x] No files modified except this verification report.
- [x] Read-only constraints satisfied (no build, no test run, no migration, no commit/push, no file write other than this report).

---

## Section 10 — Final Repository-State Verification

### Final working-tree status
`git status --short` at audit time showed the four GAP implementations as **uncommitted working-tree modifications** (the implementation is staged in the working tree but NOT committed), plus two pre-existing untracked audit files:

```
 M src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQuery.cs
 M src/TopLab.Application/Features/PatientSearch/Queries/SearchPatientsGlobal/SearchPatientsGlobalQueryHandler.cs
 M src/TopLab.Presentation/ViewModels/Patients/CultureEntryViewModel.cs
 M src/TopLab.Presentation/ViewModels/Patients/PatientSearchViewModel.cs
 M src/TopLab.Presentation/ViewModels/Patients/ProfileEntryViewModel.cs
 M src/TopLab.Presentation/Views/Patients/CultureEntryView.xaml
 M src/TopLab.Presentation/Views/Patients/PatientSearchView.xaml
 M src/TopLab.Presentation/Views/Patients/ProfileEntryView.xaml
 M tests/TopLab.Application.Tests/Features/PatientSearch/SearchGuardrailTests.cs
 M tests/TopLab.Application.Tests/Features/PatientSearch/SearchPatientsGlobalQueryHandlerTests.cs
?? Docs/Audit/StageR1-R6-Unimplemented-Items-Classification-Audit-20261026-FINAL-VERIFIED-R1.md
?? Docs/Audit/VERIFICATION-StageR1-R6-20261026.md
```

`git diff --stat` confirmed exactly 10 modified tracked files (174 insertions, 5 deletions) and **no migration or seed files** among them.

### Files modified during audit: none (except this report)
This audit created exactly one new file: the verification report itself. No tracked file was modified, staged, or committed by this audit.

### Exact path of the verification report
`Docs/Audit/IMPLEMENTATION-VERIFICATION-First-Four-BucketA-Items-20261026.md`

### Compliance with read-only and one-file-only constraints
- No build/test/publish/restore/EF command was run.
- No `git add`, `git commit`, `git push`, `git reset`, `git clean`, or file-modifying `git checkout` was run.
- No migration, schema, seed, permission, or configuration change was made.
- The only write operation was the creation of this single report under `Docs/Audit/`.

**Compliance statement:** the repository evidence (unchanged HEAD `b3c4661`; the four GAP changes remain uncommitted working-tree edits that this audit did not stage or commit; no migration/seed files touched; single new report file) supports full compliance with the read-only and one-file-only constraints. The four GAP implementations under audit were left exactly as found — uncommitted in the working tree.

---

# Second Batch Implementation Verification — GAP-09, GAP-15, GAP-17, GAP-23

## Implementation date

2026-10-26

## HEAD commit at time of implementation

`b3c4661` — `قبل التصنيف` (unchanged; all second-batch changes are uncommitted working-tree edits)

## Owner decisions applied

| Decision | Applied | Evidence |
|---|---|---|
| D1 — GAP-23 scope is 5 screens; `TestCatalogViewModel` excluded | YES | `UndoCommand` present on exactly 5 VMs; `grep -c UndoCommand` on `TestCatalogViewModel.cs` = **0** |
| D2 — GAP-09 navigation destinations | YES | account → `PatientAccountViewModel`; patient data → `PatientEditorViewModel`; lab results → `PatientVisitHistoryViewModel`; basket → in-memory collection |
| D3 — GAP-17 predicate is `EnteredAtUtc == null` | YES | `WorkSheetHelpers.cs:115` — `.Where(pt => unfinishedOnly != true || pt.EnteredAtUtc == null)` |
| D4 — double-press removal NOT implemented | YES | no change to any double-press path; no OD-3 code touched |

---

## ITEM 1 — GAP-09 (Search screen action bar)

| # | Verification point | Evidence | Result |
|---|---|---|---|
| 1.1 | `OpenAccountCommand` exists | `ViewModels/Patients/PatientSearchViewModel.cs:70` (init), `:224` (property) | PASS |
| 1.2 | navigates to `PatientAccountViewModel` | `PatientSearchViewModel.cs:734` — `_navigation.NavigateTo<PatientAccountViewModel>()` | PASS |
| 1.3 | `OpenPatientDataCommand` exists | `PatientSearchViewModel.cs:71`, `:225` | PASS |
| 1.4 | navigates to `PatientEditorViewModel` | `PatientSearchViewModel.cs:748` — `_navigation.NavigateTo<PatientEditorViewModel>()` | PASS |
| 1.5 | `OpenLabResultsCommand` exists | `PatientSearchViewModel.cs:72`, `:226` | PASS |
| 1.6 | navigates to `PatientVisitHistoryViewModel` | `PatientSearchViewModel.cs:762` | PASS |
| 1.7 | `AddToSelectionCommand` exists | `PatientSearchViewModel.cs:73`, `:227` | PASS |
| 1.8 | adds to in-memory collection | `PatientSearchViewModel.cs:40` (`_selectionBasket`), `:703` (`SelectionBasket`), `:774` (`AddToSelection`) | PASS |
| 1.9 | buttons present in the view | `Views/Patients/PatientSearchView.xaml:143,152,161,170` | PASS |
| 1.10 | disabled with no selection | all four registered with `_ => SelectedItem is not null`; `RaiseCanExecuteChanged()` at `:158-161` | PASS |
| 1.11 | no persistence added | `_selectionBasket` is an `ObservableCollection<PatientSearchHitDto>` field only — no mediator call | PASS |

Tests: `tests/TopLab.Presentation.Tests/PatientSearch/PatientSearchActionBarTests.cs` — 7 tests, all passing.

---

## ITEM 2 — GAP-15 (Period-list action)

| # | Verification point | Evidence | Result |
|---|---|---|---|
| 2.1 | period-list action exists | `ViewModels/Patients/PatientsHubViewModel.cs:86` — `OpenPeriodListCommand` | PASS |
| 2.2 | date range is user-selectable | `PatientsHubViewModel.cs:190` (`PeriodFrom`), `:196` (`PeriodTo`) | PASS |
| 2.3 | filters on registration date | forwards to `PatientSearchViewModel.FromDate/ToDate`, which feed the existing F5 `SearchPatientsGlobalQuery.From/To` (registered `RegistrationDateUtc`) | PASS |
| 2.4 | UI controls present | `Views/Patients/PatientsHubView.xaml:30,32,34` — two `DatePicker`s + the button | PASS |
| 2.5 | no schema change | no entity/migration touched; reuses the existing F5 filter | PASS |

---

## ITEM 3 — GAP-17 (Work-sheet test-group print + unfinished-only)

| # | Verification point | Evidence | Result |
|---|---|---|---|
| 3.1 | test-group print command exists | `Application/Features/WorkSheets/Commands/PrintTestGroupWorkSheet/PrintTestGroupWorkSheetCommand.cs` | PASS |
| 3.2 | handler wired | `.../PrintTestGroupWorkSheetCommandHandler.cs` | PASS |
| 3.3 | validator registered | `.../PrintTestGroupWorkSheetCommandValidator.cs` (resolved by `AddValidatorsFromAssemblyContaining`) | PASS |
| 3.4 | print path in the VM | `WorkSheetsViewModel.cs:424` (`PrintTestGroupSheetAsync`), `:173` (property) | PASS |
| 3.5 | `UnfinishedOnly` on the query | `GetWorkSheetByTestGroupQuery.cs:9` — `bool? UnfinishedOnly = null` | PASS |
| 3.6 | **predicate is `EnteredAtUtc == null`** | `WorkSheetHelpers.cs:115` | PASS |
| 3.7 | `ResultValue == null` NOT used | `WorkSheetHelpers.cs` contains no `ResultValue` predicate | PASS |
| 3.8 | UI toggle present | `Views/WorkSheets/WorkSheetsView.xaml:74` — `CheckBox ... IsChecked="{Binding UnfinishedOnly}"` | PASS |
| 3.9 | print button enabled | `WorkSheetsView.xaml:75` — `Command="{Binding PrintTestGroupSheetCommand}"` (was `IsEnabled="False"`) | PASS |
| 3.10 | double-press removal NOT implemented | no file under `Features/WorkSheets` references the OD-3 paths; scope untouched | PASS |

Tests: `tests/TopLab.Application.Tests/Features/WorkSheets/WorkSheetUnfinishedOnlyTests.cs` — 4 tests, all passing, including the discriminating test that a line with `EnteredAtUtc` set and a null `ResultValue` is treated as **finished**.

---

## ITEM 4 — GAP-23 (Undo on 5 editing screens)

| # | Verification point | Evidence | Result |
|---|---|---|---|
| 4.1 | `TestEditorViewModel.UndoCommand` | `:135`, `:204`; `UndoAsync` at `:277` | PASS |
| 4.2 | `TestGroupsViewModel.UndoCommand` | `:62`, `:88` | PASS |
| 4.3 | `ExternalEntityEditorViewModel.UndoCommand` | `:55`, `:132` | PASS |
| 4.4 | `AntibioticEditorViewModel.UndoCommand` | `:44`, `:57` | PASS |
| 4.5 | `UserManagementViewModel.UndoCommand` | `:95`, `:250` | PASS |
| 4.6 | `TestCatalogViewModel` has NO UndoCommand | `grep -c` = 0 | PASS |
| 4.7 | Undo persists nothing | no `SaveCommand`/`UpdateCommand`/`SavePermissionsCommand` dispatch inside any `UndoAsync` | PASS |
| 4.8 | Undo reverts the permission matrix too | `UserManagementViewModel.UndoAsync` → `LoadDetailAsync`, which reassigns every `PermissionItem.IsGranted` | PASS |
| 4.9 | Undo buttons present in the views | `TestEditorWindow.xaml:14`, `TestGroupsView.xaml:11`, `ExternalEntityEditorWindow.xaml:10`, `AntibioticEditorWindow.xaml:11`, `UserManagementView.xaml:75` | PASS |

---

## Build and test results

| Step | Command | Result |
|---|---|---|
| Build | `dotnet build TopLab.sln -c Release` | **Build succeeded — 0 Warning(s), 0 Error(s)** |
| Test | `dotnet test TopLab.sln -c Release --no-build` | **All green** |

| Test project | Passed | Failed | Skipped | Total |
|---|---|---|---|---|
| TopLab.Domain.Tests | 728 | 0 | 0 | 728 |
| TopLab.Application.Tests | 1916 | 0 | 0 | 1916 |
| TopLab.Infrastructure.Tests | 527 | 0 | 0 | 527 |
| TopLab.Presentation.Tests | 380 | 0 | 0 | 380 |
| TopLab.Persistence.Tests | 24 | 0 | 2 | 26 |
| **Total** | **3575** | **0** | **2** | **3577** |

---

## Files changed

### Modified — Application
- `src/TopLab.Application/Common/Interfaces/IWorkSheetPdfWriter.cs` — added `WriteGroupPdfAsync`
- `src/TopLab.Application/Common/Interfaces/IWorkSheetPrintingService.cs` — added `PrintGroupWorkSheetAsync`
- `src/TopLab.Application/Features/WorkSheets/Common/WorkSheetHelpers.cs` — `unfinishedOnly` parameter + `EnteredAtUtc == null` predicate
- `src/TopLab.Application/Features/WorkSheets/Queries/GetWorkSheetByTestGroup/GetWorkSheetByTestGroupQuery.cs` — `UnfinishedOnly`
- `src/TopLab.Application/Features/WorkSheets/Queries/GetWorkSheetByTestGroup/GetWorkSheetByTestGroupQueryHandler.cs` — forwards the flag

### New — Application
- `src/TopLab.Application/Features/WorkSheets/Commands/PrintTestGroupWorkSheet/PrintTestGroupWorkSheetCommand.cs`
- `src/TopLab.Application/Features/WorkSheets/Commands/PrintTestGroupWorkSheet/PrintTestGroupWorkSheetCommandHandler.cs`
- `src/TopLab.Application/Features/WorkSheets/Commands/PrintTestGroupWorkSheet/PrintTestGroupWorkSheetCommandValidator.cs`
- `src/TopLab.Application/Features/WorkSheets/Common/WorkSheetGroupPrintEnvelope.cs`

### Modified — Infrastructure
- `src/TopLab.Infrastructure/Printing/WorkSheetPdfWriter.cs` — `WriteGroupPdfAsync` (A4 RTL group sheet)
- `src/TopLab.Infrastructure/Printing/WorkSheetPrintingService.cs` — `PrintGroupWorkSheetAsync`

### Modified — Presentation
- `ViewModels/Patients/PatientSearchViewModel.cs` — four commands + selection basket
- `ViewModels/Patients/PatientsHubViewModel.cs` — period-list action
- `ViewModels/WorkSheets/WorkSheetsViewModel.cs` — `UnfinishedOnly`, `PrintTestGroupSheetCommand`
- `ViewModels/Lab/TestEditorViewModel.cs` — `UndoCommand`
- `ViewModels/Lab/TestGroupsViewModel.cs` — `UndoCommand`
- `ViewModels/Lab/AntibioticEditorViewModel.cs` — `UndoCommand` + snapshot
- `ViewModels/External/ExternalEntityEditorViewModel.cs` — `UndoCommand`
- `ViewModels/Users/UserManagementViewModel.cs` — `UndoCommand`
- `Views/Patients/PatientSearchView.xaml` — four action columns
- `Views/Patients/PatientsHubView.xaml` — period-list panel
- `Views/WorkSheets/WorkSheetsView.xaml` — unfinished-only checkbox + enabled print button
- `Views/Lab/TestEditorWindow.xaml`, `Views/Lab/TestGroupsView.xaml`, `Views/Lab/AntibioticEditorWindow.xaml`, `Views/External/ExternalEntityEditorWindow.xaml`, `Views/Users/UserManagementView.xaml` — undo buttons

### New — Tests
- `tests/TopLab.Presentation.Tests/PatientSearch/PatientSearchActionBarTests.cs`
- `tests/TopLab.Application.Tests/Features/WorkSheets/WorkSheetUnfinishedOnlyTests.cs`

### Modified — Tests
- `tests/TopLab.Application.Tests/Features/WorkSheets/Commands/PrintWorkSheetCommandHandlerTests.cs` — fake implements the new port member

---

## Risks, warnings, deviations

1. **Period-list UX reuses the search screen.** GAP-15's `OpenPeriodListCommand` navigates to `PatientSearchViewModel` with `FromDate`/`ToDate` pre-filled rather than opening a dedicated list screen. This satisfies "display patients registered in a chosen period" with zero new query and zero schema change, but the owner may prefer a standalone screen.
2. **Group worksheet rendering is a new writer method.** `WriteGroupPdfAsync` renders a section/line table (patient, test, barcode, drawn). It does not include the per-visit sample-kind letters, which only exist on the visit DTO.
3. **No handler unit test for `PrintTestGroupWorkSheetCommandHandler`.** The underlying query is covered by `WorkSheetUnfinishedOnlyTests` and the existing `WorkSheetQueryHandlerTests`; the thin command wrapper is not separately tested.
4. **Two pre-existing skipped tests** in `TopLab.Persistence.Tests` remain skipped (unchanged by this batch).

---

## Git state

- HEAD unchanged: `b3c4661`
- No `git add`, `git commit`, `git push`, `git reset`, `git clean`, or file-modifying `git checkout` was run.
- All second-batch work is uncommitted working-tree state, as required.
- No migration, schema, seed, permission, or configuration change was made.

---

# Second Batch Test Coverage Fix Verification

**Verification date:** 2026-10-27  
**Verifier:** Independent verification audit (read-only, no implementation)  
**HEAD commit at time of verification:** `b3c46611b1e28fed1f9d2b648e51b2e39841a546` (branch `main`)  
**Scope:** Verify closure of the three test-coverage gaps identified in the previous audit (GAP-15, GAP-17 handler, GAP-23). Test-only round — no application code changes expected.

---

## GAP-15 Tests (Period-List Action) — CLOSED

**File:** `tests/TopLab.Presentation.Tests/Patients/PatientsHubPeriodListTests.cs`

Tests verify core behaviors:
- `PeriodFrom_Property_Exists` (line 38) — PeriodFrom set/get works
- `PeriodTo_Property_Exists` (line 46) — PeriodTo set/get works
- `OpenPeriodListCommand_Exists` (line 54) — command exists
- `PeriodListEnabled_Property_Exists` (line 61) — property exists, defaults true
- `OpenPeriodList_Disabled_WhenPeriodFromNull` (line 68) — null date handling
- `OpenPeriodList_ExecutesWithoutException_WhenBothSet` (line 79) — executes with dates set
- `OpenPeriodList_ExecutesWithoutException_WhenBothNull` (line 89) — executes with both null
- `OpenPeriodList_NavigatesToPatientSearchViewModel` (line 97) — navigates to search screen
- `OpenPeriodList_CarriesPeriodFromAndPeriodTo` (line 107) — dates pre-filled on search VM (`search.FromDate`, `search.ToDate`)
- `OpenPeriodList_DoesNotCarryStaleValues_WhenCleared` (line 121) — cleared dates produce null FromDate/ToDate
- `PeriodFrom_Equals_PeriodTo_IsValid` (line 142) — equal dates handled
- `PeriodFrom_After_PeriodTo_DoesNotThrow` (line 156) — reversed dates don't throw
- `NullDates_DoNotCauseException` (line 166) — null dates don't throw
- `PatientSearchViewModel_Accepts_FromDate_ToDate` (line 174) — search VM accepts FromDate/ToDate
- `PatientSearchViewModel_NullDates_BehaveAsBefore` (line 186) — search VM null dates behave correctly

**Verdict: CLOSED** — Tests exist covering period properties, command existence, navigation with pre-filled dates, and null/empty date handling.

---

## GAP-17 Handler Test (PrintTestGroupWorkSheetCommandHandler) — CLOSED

**File:** `tests/TopLab.Application.Tests/Features/WorkSheets/Commands/PrintTestGroupWorkSheetCommandHandlerTests.cs`

Tests verify handler wiring:
- `HappyPath_BuildsTokenFromSheet` (line 47) — command → query → envelope → printing service wire-up; token round-trips through `WorkSheetGroupPrintEnvelope` back to `WorkSheetDto`
- `QueryFailure_PropagatesWithoutPrinting` (line 65) — query failure propagates, no printing attempted
- `PrintingFailure_Propagates` (line 77) — printing failure propagates
- `UnfinishedOnly_True_PropagatesToQuery` (line 89) — `UnfinishedOnly` value passed from command to `GetWorkSheetByTestGroupQuery` (verified via `FakeSender.WithResponse` matching the query with `UnfinishedOnly: true`)
- `EmptyResult_HandledGracefully` (line 102) — empty sheet handled
- `Requires_PrintWorksheet_Permission` (line 114) — `PRINT_WORKSHEET` permission code

Additional validator tests: `PrintTestGroupWorkSheetCommandValidatorTests` (line 121) and envelope tests: `WorkSheetGroupPrintEnvelopeTests` (line 147).

**Verdict: CLOSED** — Dedicated unit test exists for the handler, covering command → query → envelope → printing service wiring, and `UnfinishedOnly` propagation from command to query.

---

## GAP-23 Tests (Undo on 5 Editing Screens) — CLOSED

**File:** `tests/TopLab.Presentation.Tests/Lab/Gap23UndoCommandTests.cs`

Tests cover all five ViewModels plus the excluded sixth:
- `TestCatalogViewModel_HasNoUndoCommand` (line 63) — negative test: `TestCatalogViewModel` has NO `UndoCommand` (reflection-based check)
- `TestEditorViewModel_HasUndoCommand` (line 70) — `UndoCommand` exists
- `TestGroupsViewModel_HasUndoCommand` (line 77) — `UndoCommand` exists
- `ExternalEntityEditorViewModel_HasUndoCommand` (line 84) — `UndoCommand` exists
- `AntibioticEditorViewModel_HasUndoCommand` (line 91) — `UndoCommand` exists
- `UserManagementViewModel_HasUndoCommand` (line 98) — `UndoCommand` exists

Tests verify Undo reverts without persisting:
- `TestEditor_Undo_InNewMode_ResetsFields_WithoutSaving` (line 105) — fields reset, no save dispatched
- `TestGroups_Undo_DoesNotThrow` (line 120) — no exception
- `ExternalEntity_Undo_InNewMode_ResetsFields` (line 129) — fields reset
- `Antibiotic_Undo_RevertsToLoadedSnapshot` (line 142) — reverts to loaded snapshot values
- `Antibiotic_Undo_InNewMode_ResetsToEmpty` (line 159) — resets to empty
- `UserManagement_Undo_WithNoSelection_DoesNotThrow` (line 172) — no exception
- `UserManagement_Undo_RevertsSelectedDetail` (line 180) — reverts selected detail
- `ExternalEntity_Undo_InEditMode_ReloadsDetail` (line 191) — reloads persisted detail in edit mode
- `TestEditor_Undo_InEditMode_ReloadsDetail` (line 204) — reloads persisted detail in edit mode

**Verdict: CLOSED** — Tests exist for all five ViewModels, verify Undo reverts fields without persisting (no save/update/commit dispatched), and include a negative test confirming `TestCatalogViewModel` has no `UndoCommand`.

---

## Application Code Modified — NO

`git status --short` confirms:
- All `M` (modified tracked) files are from the original second-batch implementation (pre-existing, not changed in this round)
- The only new untracked (`??`) files relevant to this round are test files:
  - `tests/TopLab.Application.Tests/Features/WorkSheets/Commands/PrintTestGroupWorkSheetCommandHandlerTests.cs`
  - `tests/TopLab.Presentation.Tests/Lab/Gap23UndoCommandTests.cs`
  - `tests/TopLab.Presentation.Tests/Patients/PatientsHubPeriodListTests.cs`
- No application source file (under `src/`) was added or modified in this round. The task was strictly test-only.

---

## Final Verdict

| Gap | Verdict |
|-----|---------|
| GAP-15 tests | CLOSED |
| GAP-17 handler test | CLOSED |
| GAP-23 tests | CLOSED |
| Application code modified | NO |

**ALL GAPS CLOSED — READY FOR OWNER COMMIT**
