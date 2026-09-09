# Top-Lab — Handoff Document M-05

## نظام توب لاب — تسليم جلسة عمل (Module 5 — Specialized Profile Result Reports)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-09_M05-specialized-profile-result-reports` |
| Session date (UTC) | 2026-09-09 |
| Session start (UTC) | 2026-09-09 |
| Session end (UTC) | 2026-09-09 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-05 |
| Module name | Specialized Profile Result Reports (backend only) |
| Wave | 6 |
| Feature folder(s) touched | `src/TopLab.Domain/Tests/`, `src/TopLab.Domain/Results/`, `src/TopLab.Application/Features/ProfileResults/`, `src/TopLab.Application/Features/PatientRegistration/Commands/AddProfileToVisit/` (central-charge skin), `src/TopLab.Infrastructure/Persistence/Configurations/` + `DbSets` + migration `AddAnalyteProfileDomain`, `tests/TopLab.Domain.Tests/`, `tests/TopLab.Application.Tests/Features/ProfileResults/`, `tests/TopLab.Infrastructure.Tests/Persistence/` |
| Layers touched | Domain + Application + Infrastructure proof (+ tests + close-out docs only) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `5cf6946` (live `main` HEAD after Slice 3) |
| Final commit at session end | Slice 4 close-out `[M-05] Slice 4/4: Infrastructure proof and close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 5 **Specialized Profile Result Reports** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-05.md` (profile catalog + frozen-range result tables + migration → Application read surface → Application write surface + central pricing → infrastructure proof + close-out). The work is **backend only** — no Presentation content. S1 delivers the profile domain (Analyte owns the only live `AnalyteReferenceRange`; `Profile` 1:1 with a `ResultKind.SpecializedProfile` test; `ProfileAnalyte` composition; `ProfileResultItem` + 1:1 `ProfileResultItemReferenceRangeSnapshot` freeze child + `ProfileResultAmendment` audit history) plus the module's only migration. S2 ships the read surface (entry grid, frozen-range report/reprint reading snapshots only, amendment history under `PT_AUDIT_ACCESS`). S3 ships the write surface (draft save with per-item band capture, verify/unverify, print reusing the M-04 `BalanceProbe` block, atomic post-print amendment) plus the central-calculator integration. S4 proves the EF model config, relational rollback atomicity, reprint-after-range-change, and central-charge persistence; closes the module (ADR-0036 + tracking flip + this handoff). Build must be 0 errors / 0 warnings; full suite green. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Domain + migration** — Implementation Complete — Analyte-owned live range (unique `AnalyteReferenceRange.AnalyteId`; `AnalyteReferenceRangeBand` age/sex rows), `Profile`/`ProfileAnalyte` catalog (`Profile.TestId` unique 1:1 to a `SpecializedProfile` test, composite `(ProfileId, AnalyteId)` unique), `ProfileResultItem` + 1:1 frozen-range snapshot + `ProfileResultAmendment`, configuration + `DbSet`s, migration `AddAnalyteProfileDomain` (the module's only migration). Commit `[M-05] Slice 1/4: ...`.
- **S2 — Application read surface** — Implementation Complete — `ProfileResultDtos` (entry item/grid, report line/report, amendment DTO) + `ProfileResultReferenceRangeCapture` (`Capture` via the shared `ResultFlagComputer.SelectMatch`, `FindProfile`, `ConfiguredAnalytes`); `GetProfileEntryGrid` (snapshots filtered by item ids), `GetProfileReport` (snapshot-only frozen ranges, `ReportName` display, reprint after band change reads frozen values), `GetProfileResultAmendments` (gated `PT_AUDIT_ACCESS`, newest-first). Commit `[M-05] Slice 2/4: ...`.
- **S3 — Application write surface + central pricing** — Implementation Complete — `SaveProfileResults` (before-review only, replaces drafts, never touches printed rows, captures each item's band snapshot, single save), `VerifyProfileResults`/`UnverifyProfileResults` (idempotent, review-gated), `MarkProfilePrinted` (entered+reviewed eligibility, M-04 `BalanceProbe` per-user print block with absolute-permission bypass, marks only unprinted items — reprint-safe), `AmendProfileResult` (printed-only, active row + immutable audit row in one `SaveChangesAsync`), feature-local `DomainFailureTranslator`, and the central-calculator wiring: `AddProfileToVisitCommandHandler` stores exactly `PatientAccountCalculator.ProfileSelectionCharge(profile.FixedPrice)`. 40 application tests. Commit `[M-05] Slice 3/4: ...`.
- **S4 — Infrastructure proof + close-out** — Implementation Complete — `M05ProfileDomainConfigurationTests` (7 F5-style mapping assertions incl. unique indexes, snapshot 1:1 by `ProfileResultItemId` Cascade, item→Analyte **Restrict** pin, amendment Cascade + index); `ProfileDomainPersistenceTests` real-`ApplicationDbContext` InMemory proofs (profile-order central charge round-trip; conditional `AmendProfileResult` + audit atomic-rollback proven by a deliberately failing `ISaveChangesInterceptor` then a fresh-context assertion that neither the active value nor the audit row persisted; report-after-live-range-change across three separate contexts reading the frozen 1..5 values); ADR-0036; tracking-sheet M05 row → 🟩 Done + dated change-log row; this handoff. Commit `[M-05] Slice 4/4: ...`.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes (Release, `-m:1` posture).
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full suite (`dotnet test TopLab.sln -m:1`): **1378 green** = 357 Domain.Tests + 904 Application.Tests + 117 Infrastructure.Tests.
- New tests added: +155 across S2–S4 (40 Application + 7 + 3 = 10 Infrastructure, plus S1 Domain) — full counts verified at each slice gate.
- Tests currently failing: none.
- Coverage of the M-05 footprint: every entry-grid/search/report/amendment read path, every write guard (before-review draft lock, verify/unverify, print eligibility + balance block + absolute bypass, amendment printed-only), the frozen-range reprint semantics (print/amend/test-only surfaces), the central-charge storage, and the EF model config/FK matrix + relational atomicity + cascade proofs are pinned by dedicated tests. No coverage waiver; the M-03 posture applies (validators auto-registered, handler-direct tests bypass the pipeline).

### 4.3 Migrations

- New EF Core migration(s) added: **One** — `AddAnalyteProfileDomain` (analyte/profile catalog + `ProfileResultItems`, `ProfileResultItemReferenceRangeSnapshots`, `ProfileResultAmendments`; applied to LocalDB in S1).
- Migration order verified: Baseline → RenamePkColumns → AddTestCodeAndLifecycleColumns → M02 → M04 → M05 `AddAnalyteProfileDomain` (in order).
- `has-pending-model-changes` at close-out: no changes — zero drift.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: none.
- Validators: 5 new validators resolve via the existing assembly scan (`AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>()`); `ValidatorRegistrationTests` confirms them.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the 13-row `PermissionConfiguration.cs` seed: **none** — `git diff src/TopLab.Infrastructure/Persistence/Configurations/PermissionConfiguration.cs` is empty. Amendment history reuses the existing `PT_AUDIT_ACCESS` gate.

---

## 5. Work In Progress (Required — mark "None" if none)

None. All four slices reached a terminal state; module closed out in the Master Tracking Sheet (§4 M05 row + §9 change-log row) and this handoff. Natural next steps (profile entry/report Presentation consuming the S2/S3 surface; M06 culture entry on top of the result tables; M07/M09 reporting/delivery) are out of scope of this plan.

---

## 6. Decisions Taken This Session (Required — mark "None" if none)

All were owner-confirmed verbatim before/during execution and are recorded in ADR-0036 (no re-derivation):

- **Decision (live-range freeze):** Only the analyte's `AnalyteReferenceRange` is live; matching via `ResultFlagComputer.SelectMatch`; the matched band freezes into the 1:1 `ProfileResultItemReferenceRangeSnapshot`; reports/reprints read snapshots only — a later band change never mutates saved/printed items (`ReportName` for display).
  - **Scope of impact:** Analyte/range/band domain + snapshot child + report query.
  - **Follow-up required:** No.
- **Decision (central pricing):** No feature-local formula — `ManualSelectionCharge` (manual picks) / `ProfileSelectionCharge(profile.FixedPrice)` (typed profile orders, stored at order time via `AddProfileToVisit`); price lists/custom groups never influence the profile price.
  - **Scope of impact:** `AddProfileToVisit` handler skin + central-calculator round-trip test.
  - **Follow-up required:** No.
- **Decision (atomic post-print amendment):** Only printed items qualify; the active row's value/unit/flag updates in place and a complete immutable `ProfileResultAmendment` audit row is appended in the SAME `SaveChangesAsync` (both commit or both roll back — proven by the failing-interceptor rollback test).
  - **Scope of impact:** `AmendProfileResultCommand` + handler + audit entity + rollback proof.
  - **Follow-up required:** No.
- **Decision (permission reuse):** Entry/edit gates reuse `ResultsEntryAccessPolicy` (`EDIT_RESULTS`/`REVIEW_RESULTS`/`PRINT_RESULTS`); amendment history is gated by the existing `PT_AUDIT_ACCESS`; no new permission rows, no `PermissionConfiguration` change.
  - **Scope of impact:** Authorization surface + authorization tests.
  - **Follow-up required:** No.
- **Decision (profile FK guards):** Analyte `Name` unique; `Profile.TestId` unique 1:1 Cascade; `ProfileAnalyte` composite unique with cascade FKs; `ProfileResultItem`→`PatientTest` Cascade but →`Analyte` **Restrict** (an analyte carrying result rows cannot be deleted); snapshot 1:1 by `ProfileResultItemId` Cascade; amendment Cascade.
  - **Scope of impact:** EF config + FK-matrix tests (incl. negative Restrict assertion).
  - **Follow-up required:** No.

---

## 7. Deviations and Waivers (Required — mark "None" if none)

- **Deviation:** InMemory cannot observe `tinyint`/`datetimeoffset` column-type annotations (provider surface) — same limitation as M-03/M-04.
  - **Waiver:** No waiver — the InMemory-observable surface is asserted (nullability, CLR types, precision/scale/max-length, PK/FK/delete behavior); column types are pinned by the zero-drift gate (`has-pending-model-changes` = no changes and the S1 migration snapshot).
  - **Pinned by:** `M05ProfileDomainConfigurationTests` + the drift gate.
- **Deviation:** The amendment-rollback proof uses a deliberately failing custom `ISaveChangesInterceptor` (both sync and async overloads) because the InMemory provider ignores real transactions; correctness rests on the store never being written when `SaveChanges` aborts before commit, asserted from a fresh context.
  - **Waiver:** No waiver — this is the mandated VG-03 "relational rollback" proof realized without Testcontainers.
  - **Pinned by:** `AmendAndAudit_AtomicRollback_WhenSaveFails`.
- **Waiver:** None (no coverage waiver — exercised paths listed in §4.2).

---

## 8. Required Reading (Required — mark "None" if none)

- **M-05 Implementation Plan** — `Docs/OpenCode/M-05.md` (this session's source of truth; VG-01..VG-04 gates).
- **M-05 Loop-Engineering Memory** — `Docs/OpenCode/M-05-memory.md` (slice index, gates, per-slice 10-stage checklists, execution log).
- **ADR-0036** — `Docs/Source/Top_Lab_ADR.md` (live-range freeze, central pricing, atomic amendment, profile-catalog pins, permission reuse).
- **M-04 Handoff** — `Docs/Handoff_M04.md` (for `BalanceProbe` print block, `ResultFlagComputer` matcher, snapshot/freeze precedent).
- **M-12 Handoff** — `Docs/Handoff_M12.md` (for the analyte/range catalog the profile uses).
- **M-03 Handoff** — `Docs/Handoff_M03.md` (for `PatientAccountCalculator` central charge the profile pricing delegates to).
- **M-15 Handoff** — `Docs/Handoff_M15.md` (for the feature-local `DomainFailureTranslator` precedent reused here).

---

*End of document.*