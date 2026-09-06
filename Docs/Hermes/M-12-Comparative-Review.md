# Comparative Review — M-12 Implementation Plans (Hermes vs. OpenCode vs. Final Action Plan)

**Author:** Hermes Side (analyst role — read-only review)
**Date:** 2026-09-06
**Sources compared:**
- `Docs/Hermes/M-12.md` (Hermes plan, 136 lines, 30,840 bytes) — **DO NOT MODIFY**
- `Docs/OpenCode/M-12.md` (OpenCode plan, ~1,144 lines, 56,486 bytes) — reference only
- `Docs/Final Action Plan for Functional Unit 12.md` (consolidated plan, 506 lines, 71,816 bytes) — **DO NOT MODIFY**

**Methodology:** point-by-point comparison of structure, technical claims, scope boundaries, data-model decisions, sequencing, testability, and alignment with the verified code state (commit `3ce602b` per the Final Action Plan's reference snapshot).

**Explicit constraint reminder:** This document is a new file. Neither `Docs/Hermes/M-12.md` nor `Docs/Final Action Plan for Functional Unit 12.md` is modified, merged, patched, or "corrected" in any way. Discrepancies are described — not silently resolved by editing either source.

---

## Part 1 — Overall Verdict

**The Final Action Plan is materially better than the Hermes M-12 plan** in nearly every dimension that matters for a real implementation on this codebase. The Final Action Plan is not merely "more thorough prose" — it is grounded in direct verification of the F5 migration, the EF model snapshot, the M17/M22 handler precedents, the dependency map, and the test strategy. It rejects several premises that the Hermes plan (and the OpenCode plan) carry forward, and it does so with cited evidence rather than assertion. The Hermes plan would, if executed as written, lead to at least one architectural regression (per-field Test mutators + DomainEvent pattern that does not exist and is architecturally forbidden) and at least one database-identity collision (the `Max()+1` strategy on `IDENTITY(1,1)` columns).

That said, the Final Action Plan is not a complete rewrite of the Hermes plan. It absorbs three of the five cross-review improvements (`CR-1`, `CR-3`, `CR-5`) and the spirit of `CR-7`, and it adopts several architectural assumptions that the Hermes plan is *not even aware of* (the absence of a repository/`IUnitOfWork` layer, the absence of `AddValidatorsFromAssembly`, the use of `ViewModelBase`/`RelayCommand` instead of `CommunityToolkit.Mvvm`, the actual SQL Server default collation). The Hermes plan's core value — an 8-slice structure with explicit validation gates and a human-confirmation-before-commit policy that survived auditing in an earlier task — is preserved as scaffolding.

**Specific gap analysis (where the Final Action Plan is strictly better than Hermes):**

| # | Aspect | Hermes M-12 says | Final Action Plan says | Verdict |
|---|---|---|---|---|
| 1 | ID generation on new Test / TestGroup / WorkGroupLog / ReferenceRange rows | "F5 schema does not define identity columns on Test / TestGroup / WorkGroupLog / ReferenceRange" (S3, line 59) → use `Max()+1` strategy | Verified `SqlServer:Identity` annotation on every M-12 table (migration lines 254, 296, 451, 633) + `ValueGeneratedOnAdd()` in every EF configuration; use sentinel `TestId.Create(0)` | **Final is correct; Hermes is wrong.** A `Max()+1` strategy on `IDENTITY(1,1)` columns would either fail at INSERT (if EF passes the value through) or generate silently wrong IDs (if EF ignores the value but the in-memory `Id.Value` is consumed by callers expecting it to match the DB). The Final Action Plan's D-9 + R-3 explicitly addresses this and recommends a LocalDB integration test. |
| 2 | Per-field `Test` mutators with `TestChangedEvent` | Recommended (Ambiguity #1, line 35; S1, line 45) | Rejected (D-2, line 484): the codebase has no domain-event infrastructure (grep finds 0 matches) and Architecture §3 forbids Domain from referencing MediatR/EF/WPF | **Final is correct; Hermes is wrong architecturally.** Adding `DomainEvent` types would require either a new MediatR dependency in Domain (forbidden) or a hand-rolled event dispatcher in Domain (a new architectural pattern, never used in M17/M22). Hermes's Q2 was framed as a "design preference" but is in fact an architectural constraint. |
| 3 | `ReferenceRange.Update` = new-version-row + "superseded" flag (Hermes S2, line 52) | The F5 schema has **no** `SupersededAt` (or similar) column on `ReferenceRanges`. Adding one requires a migration. M-12 forbids migrations. | **Final is correct; Hermes is internally contradictory.** Hermes S3 says "no new column; no new migration" while S2's gate (b) and (c) assert the "superseded" semantics. The Final Action Plan correctly uses `UpdateReferenceRange` = in-place update (D-10), and the BR-05 freeze-contract protection is delegated to M-04 (snapshot persistence on `PatientTest`, owned by M-04 because that is the module that owns the migration authority on that table). |
| 4 | Repository / `IUnitOfWork` layer | "Each handler depends on `IUnitOfWork` (M17)" (S2, line 53; S3, line 59) | Verified: `IUnitOfWork` does **not exist** in the codebase (grep = 0 matches); handlers use `IApplicationDbContext` directly (D-4, line 486) | **Final is correct; Hermes describes a non-existent layer.** M17's `CreateUserCommandHandler` injects `IApplicationDbContext`, not `IUnitOfWork`. The Hermes plan's S2 and S3 reference a layer that the project does not have, which would force the implementer to invent one. |
| 5 | WPF MVVM library | "CommunityToolkit.Mvvm" (Stack line; S4, line 67; S5–S7) | The project uses its **own** `ViewModelBase` / `RelayCommand` / `AsyncRelayCommand` under `src/TopLab.Presentation/Common/` (line 40) | **Final is correct; Hermes specifies a non-existent dependency.** A new NuGet package would have to be added — explicitly forbidden by both the Hermes plan's "no new NuGet" clause and the Final Action Plan. |
| 6 | Search collation (Arabic / case) | "EF.Functions.Like" for Arabic-aware partial match (S3, line 60) | SQL Server default collation is case-insensitive for `nvarchar`; plain `Contains` suffices (line 183, R-8) | **Final is correct; Hermes references a SQLite-specific concern** that the verified provider (SQL Server) does not need. |
| 7 | Validators at runtime | "FluentValidation in `Application/Validation`" (S2, line 53) | **No `AddValidatorsFromAssembly` is registered anywhere** in the solution; `ValidationBehavior` takes `IValidator<TRequest>?` (nullable) and silently defaults to null (R-1, line 429) | **Final surfaces a latent defect that Hermes ignores.** This is a project-wide issue (affects M17 and M22 also), but Hermes's validation gates for M-12 are untrustworthy until this is fixed. The Final Action Plan's S4 step 4 verifies the gap and prescribes the minimal fix (`AddValidatorsFromAssembly` in `AddApplication`). |
| 8 | Search by "containing group" (FR-M12-001) | S2, line 52: "partial match across `Test.Name`, `TestGroup.Name`, and `Test.Number`" — but Hermes S2 gate (d) only tests "name/reportName/barcode contains '42'" — **`TestGroup.Name` is missing from the gate** | Final Plan line 183 explicitly includes `TestGroup.Name.Contains(term)` via the group navigation (FR-M12-001: "searchable by test name, **containing group**, or test number") and notes that the OpenCode draft omitted this axis | **Final is more complete.** The Final explicitly fixed what OpenCode missed (CR-4 in Hermes was incomplete), and Hermes's own gate (d) was tightened by the Final's review process. |
| 9 | TestComment scope | "Owned by M13 (FR-M13-002). M-12 does not deliver… only a courtesy `TestComment.Update` domain method" (line 30) | Same resolution — explicit Delegations section, same conclusion, with the additional decision log D-9 citing the "M12 row in `Top_Lab_Master_Tracking_Sheet.md` says `TestComment` is in M-12" note and clarifying that M-12 adds only the domain `Update` method (no commands) | **Both resolve the same ambiguity the same way.** The Final adds the evidence trail (D-9 cites the tracking sheet) that the Hermes plan lacks. |
| 10 | Secondary-password gate for "النظام" navigation | Included in S7 (line 85), explicit use of `IDialogService.ShowSecondaryPasswordDialogAsync` | Same gate, also explicit, also citing the existing precedent (المستخدمون, Database Maintenance) — Final calls this out as CR-5 absorbed | **Both arrive at the same answer.** Tie. |
| 11 | `AuditableEntitySaveChangesInterceptor` as the audit mechanism | CR-1 added after the cross-review pass: "explicitly references the existing `AuditableEntitySaveChangesInterceptor`… automatically increments `ModificationCount`" (line 7) | Same mechanism, same callout, plus the Final's S8 item 2 (line 268) requires an Infrastructure test that verifies `ModificationCount` increments by 1 after `UpdateTest`-equivalent save | **Final is slightly more rigorous** — the Hermes plan's S8 gate (b) just says "audit-log entries exist for every CRUD verb" without specifying the verifiable assertion. The Final makes the test concrete. |
| 12 | SQL Server provider confirmation | (Hermes does not name the provider explicitly) | "EF Core 8.0.30 on SQL Server" (line 39) | **Final is more useful for implementer.** This is a small but material difference: a M-12 implementer reading the Hermes plan has to confirm the provider themselves; the Final has done it. |
| 13 | File-by-file change map (new files + modified files + explicitly NOT modified) | Implicit (the slices describe what to do but not in a manifest) | Explicit §8: 18 new files, 11 modified files, 11 explicitly-not-modified files, with the test project `TopLab.Presentation.Tests` flagged for creation (R-6) | **Final is materially more useful** for an implementer or coding agent. The Hermes plan lists 8 slices but does not enumerate every file to create. |
| 14 | Edge cases (E-1 to E-14) and Risks (R-1 to R-10) | Implicit in the validation gates per slice; 0 explicit edge-case table | §13 (14 edge cases) and §14 (10 risks) | **Final is materially more useful** for catching failure modes before they happen. The Hermes plan's gates test the happy path and a small number of explicit assertions. |
| 15 | Acceptance criteria as a hard gate (module Done) | Implicit ("End-to-End acceptance" in S8) | §15: 8 numbered, testable conditions including a `git diff` scope check (item 8) | **Final is materially more useful.** The Hermes plan's S8 gate (d) says "per-slice gates pass" but does not enumerate the module-level conditions for Done. |
| 16 | R-1 (validator resolution) and R-3 (LocalDB identity test) | Not mentioned | Explicitly named as risks to be verified at S4 runtime | **Final is materially more honest.** It does not pretend that the static analysis of a plan constitutes execution evidence. |
| 17 | `ID generation` — specifically the `Create(0)` sentinel | Wrongly used `Max()+1` | Correctly uses `Create(0)` with explicit risk R-3 noting the LocalDB integration test needed | (Same as row 1 above.) |
| 18 | Sequencing recommendation | Implicit | §16: explicit 0-pre-flight, 1-S1, 2-(S2 ∥ S3), 3-S4, 4-(S5 then S7), 5-S6, 6-S8 — with the rationale for S7 before S6 (gate the entry as soon as the catalog exists, enabling earlier end-to-end manual smoke) | **Final is materially more useful.** The Hermes plan describes the slice DAG but does not recommend a single-agent execution order. |

**Net of rows 1–18:** the Final Action Plan is strictly better than the Hermes M-12 plan on 14 dimensions, ties on 1, and is otherwise comparable. There is no dimension on which the Hermes plan is strictly better than the Final.

---

## Part 2 — What the Final Action Plan got right that Hermes missed

The Final Action Plan adds, fixes, or rejects the following items in the Hermes plan, with cited evidence:

### 2.1 — Rejected the `Max()+1` ID strategy on `IDENTITY(1,1)` columns

Hermes's S3 line 59: *"ID generation for new rows: since the F5 schema does not define identity columns on `Test` / `TestGroup` / `WorkGroupLog` / `ReferenceRange`, M-12 supplies a `newId = (current max) + 1` strategy at the Application handler boundary…"*

This is factually incorrect. The F5 migration (`20260828052248_BaselineDataModel.cs`) annotates every M-12 table's `Id` with `.Annotation("SqlServer:Identity", "1, 1")` (lines 254, 296, 451, 633). Every EF configuration sets `b.Property(e => e.Id)…ValueGeneratedOnAdd()`. The Hermes plan's premise is wrong; the `Max()+1` strategy is at best a dead computation (the value is never inserted into the DB) and at worst a collision risk in race conditions (two concurrent inserts both reading `max=5`, both computing `newId=6`).

The Final Action Plan uses `TestId.Create(0)` (D-9, line 491) and notes that the M17 `Max()+1` pattern is an inconsistency in older code that M-12 should not replicate. It also requires a LocalDB integration test (R-3, line 431) to confirm the sentinel works against real SQL Server — which is the right level of evidence to require.

### 2.2 — Rejected per-field Test mutators with DomainEvent (architectural violation)

Hermes's S1 line 45 specifies per-field mutators that emit a `TestChangedEvent`. But:
- `Test.Update` already exists in `Test.cs` and covers exactly the FR-M12-002 field set.
- A grep for `DomainEvent` / `INotification` / `MediatR` in `src/TopLab.Domain/` returns zero matches.
- Architecture §3 forbids Domain from referencing MediatR/EF/WPF packages.

So the Hermes plan is recommending a new architectural pattern (Domain events) that the project has never used and is architecturally prohibited from using. The Final Action Plan correctly identifies this and keeps the existing single-bulk `Test.Update` (D-2, line 484).

### 2.3 — Rejected the "superseded" versioning for ReferenceRange

Hermes's S2 line 52 says `UpdateReferenceRange` *"writes a NEW `ReferenceRange` row (versioning); old rows remain in the table for audit/lookup but are flagged as superseded"*. But:
- The `ReferenceRanges` table has no `SupersededAt`, no `Version`, no `ReplacedBy` column (verified in the migration).
- Adding such a column requires a migration. M-12 forbids migrations.
- The validation gate (b)/(c) on S2 of the Hermes plan asserts superseded semantics, which the schema cannot express.

The Final Action Plan's D-10 (line 492) uses in-place update, with the BR-05 freeze contract delegated to M-04 (the module that owns the migration authority on `PatientTest`). This is the only architecturally feasible choice.

### 2.4 — Rejected the `IUnitOfWork` / repository layer

Hermes's S2 line 53 and S3 line 59 reference "IUnitOfWork (M17)" and "repositories declared in S3". But:
- A grep for `IUnitOfWork` in `src/` returns zero matches.
- M17's `CreateUserCommandHandler` injects `IApplicationDbContext`, not `IUnitOfWork`.
- The Hermes plan is describing a layer that does not exist.

The Final Action Plan's D-4 (line 486) explicitly states "no repositories / no `IUnitOfWork` — handlers use `IApplicationDbContext` directly" with grep evidence. This is correct.

### 2.5 — Corrected the WPF MVVM library reference

Hermes's "Stack" header (line 25) and S4–S7 reference `CommunityToolkit.Mvvm`. But the project uses its own `ViewModelBase` / `RelayCommand` / `AsyncRelayCommand` under `src/TopLab.Presentation/Common/`. There is no NuGet reference to CommunityToolkit.Mvvm. Adding it would violate the "no new NuGet" rule.

The Final Action Plan line 40 names the actual classes used. This is a small but material correction: a coding agent would otherwise look for a package that does not exist.

### 2.6 — Surfaced the validator-DI latent defect (R-1)

The Hermes plan says "FluentValidation in `Application/Validation`" (S2, line 53) as if validators are wired up. The Final Action Plan's R-1 (line 429) shows that no `AddValidatorsFromAssembly` (or any equivalent) registration exists in the solution. The `ValidationBehavior` takes `IValidator<TRequest>?` (nullable), so the validators the Hermes plan would write are never resolved at runtime.

This is a project-wide defect (M17 and M22 have the same problem), but the Hermes plan's M-12 validation gates are untrustworthy until this is fixed. The Final Action Plan's S4 step 4 verifies the gap and prescribes the fix.

### 2.7 — Added an explicit search-by-group axis (FR-M12-001)

FR-M12-001 says "searchable by **test name, containing group, or test number**". The Hermes plan's S2 gate (d) tests "name/reportName/barcode contains '42'" — omitting the "containing group" axis. The Final Action Plan line 183 explicitly includes `TestGroup.Name.Contains(term)` and notes that "Prior draft `Docs/OpenCode/M-12.md` omitted the group-name axis its own CR-4 was meant to satisfy; this plan fixes it."

This is the kind of cross-axis gap that the Hermes plan's own cross-review pass (CR-4) introduced but did not propagate into the S2 validation gate. The Final Action Plan catches it.

### 2.8 — Added `WorkGroupLogItem` constructor visibility pre-check explicitly

Both plans reference the pre-check, but the Final Action Plan makes it an explicit S1 verification step with the exact grep command (`grep -rn "new WorkGroupLogItem(" src tests`) and the expected result (0 matches). Hermes's S1 (line 46) mentions the pre-check in prose but does not specify the command or the expected count as a gate. A coding agent reading the Hermes plan has to figure this out; the Final Action Plan has the test-verifiable criterion.

### 2.9 — Added file-by-file change map (§8)

The Hermes plan describes 8 slices but does not enumerate the 18 new files and 11 modified files. The Final Action Plan §8 lists every new file path, every modified file path, and 11 files that are explicitly *not* to be modified. This is materially more useful for a coding agent.

### 2.10 — Added edge cases and risks tables (§13, §14)

The Hermes plan's validation gates test happy paths and a small number of explicit assertions. The Final Action Plan §13 has 14 edge cases (E-1 to E-14) and §14 has 10 risks (R-1 to R-10), each with a handling/mitigation column. This is not a "more thorough prose" point — it is the difference between a plan that predicts what will go wrong and a plan that does not.

### 2.11 — Required a no-op infrastructure slice (S4)

The Hermes plan's S3 conflates "infrastructure" with "Application write surface + repositories". The Final Action Plan's S4 is a dedicated slice that:
- verifies no pending EF model changes,
- asserts no snapshot entity is registered in the EF model,
- performs the R-1 validator-DI runtime check.

This is the kind of slice that catches "the model looks fine on paper but fails at first SaveChangesAsync" issues. The Hermes plan's S3 does not have an equivalent gate.

### 2.12 — Recommended single-agent execution order (§16)

The Hermes plan's "Validation Gate" sections imply a sequencing but do not recommend a single-agent order. The Final Action Plan's §16 explicitly recommends: S1 → S2 → S3 → S4 → S5 → S7 → S6 → S8 — and gives the rationale for the non-DAG order (S7 before S6, so the gate can be wired as soon as the catalog exists, enabling end-to-end manual smoke earlier). This is the kind of recommendation a coding agent needs.

---

## Part 3 — What the Hermes plan got right that the Final Action Plan missed or got wrong

The Hermes plan has several substantive contributions that the Final Action Plan does not surface or under-weights:

### 3.1 — The `TestChangedEvent` per-field audit trail is genuinely valuable (but architecturally infeasible)

The Hermes plan's recommendation to use per-field mutators with named `TestChangedEvent` (S1, line 45) is *not* just a preference — it is a real audit-trail improvement over bulk `Test.Update`. If the project had a Domain-events mechanism (e.g., a `IDomainEvent` interface + a simple in-process dispatcher), this would be the right choice. The Final Action Plan rejects it as "architecturally infeasible" without acknowledging the trade-off. A real architect would note that adding a small in-process event dispatcher is a 50-line change that is itself "in scope" for M-12 if the owner values the finer audit trail. The Final Action Plan does not give the owner that option — it presents D-2 as a closed decision.

This is a real gap, not a deal-breaker. The Final Action Plan is correct that no such mechanism exists today; it would have been more useful to record "this is the cost of using bulk Update — a future change could introduce in-process Domain events and migrate to per-field mutators" as R-X.

### 3.2 — The human-confirmation-before-commit policy

The Hermes M-12 plan was produced in the context of a prior task that established a "human-confirmation-before-commit" policy (the loop-engineering skill). The plan does not re-state the policy in every slice, but it inherits it from the workflow. The Final Action Plan is a standalone document and does not reference this policy at all. A coding agent reading the Final Action Plan would be free to commit at the end of each slice without owner confirmation.

This is a workflow-context gap, not a plan defect. But if the M-12 implementation is meant to be carried out by an AI coding agent under the same governance as the prior work, the Final Action Plan should at least link to the `loop-engineering` skill or the human-confirmation policy.

### 3.3 — The Hermes plan's structure (slice-by-slice, validation gate per slice) is more accessible for an AI agent

The Final Action Plan is a 506-line document with 16 sections and 18 sub-decisions. It is well-evidenced, but it is dense. The Hermes plan is a 136-line document with 8 slices, each carrying its own validation gate. For an AI coding agent that has to sequence through the slices, the Hermes plan is more navigable.

This is a stylistic point, not a substantive one. The Final Action Plan is correct to be detailed; the Hermes plan is correct to be brief. A useful artifact would be a 1-page index that maps the Final Action Plan's sections to a slice-by-slice execution script.

### 3.4 — The Hermes plan's "no code" convention

The Hermes plan stays at the plan level (no C# snippets, no XAML snippets, no FluentValidation examples). The Final Action Plan includes inline code snippets (e.g., lines 437-471 for validators, lines 644-666 for ViewModels, lines 814-823 for DataTemplates). The Hermes plan's "no code" convention is a discipline that the Final Action Plan breaks. Whether this is a defect depends on the audience: for an AI agent that has to generate the code, snippets are useful; for a human reviewer who wants to read the plan without reading code, the Hermes convention is cleaner.

The Final Action Plan's snippets are correct and well-chosen, but a stricter "plan only" convention would have made the file more reviewable.

### 3.5 — The Hermes plan's "Open Decisions" section is more honest about uncertainty

The Hermes plan's "Open Decisions Requiring Owner Approval" section (§Q1, §Q2) presents the PatientTitle-scope and Test-update-pattern decisions as open, with explicit recommendations but explicit "Status: Open" markers. The Final Action Plan's analogous decisions (D-1, D-2, D-9, D-10, D-11) are all "resolved by evidence" with citations. The Final Action Plan is correct that the codebase's `Top_Lab_Master_Tracking_Sheet.md` and Architecture Blueprint §3 resolve these — but the resolutions are presented as closed when the owner's preference is the actual deciding factor. The Hermes plan's "Open" framing is more honest about who owns the decision.

### 3.6 — The Hermes plan's CR-4 (numeric search) is more explicitly detailed

The Hermes plan's CR-4 (cross-review changelog) describes the numeric-vs-text branch with explicit pseudo-code: *"the handler attempts `int.TryParse(SearchTerm)` — on success, the search includes `t.Id.Value == parsedInt` joined with OR"*. The Final Action Plan (line 183) describes the same logic but more concisely. For a coding agent that has to write the LINQ expression, the Hermes plan's example is more directly usable. The Final's correct observation that the OpenCode draft omitted the "containing group" axis is good, but the Final's own description of the numeric branch is shorter than the Hermes one.

### 3.7 — The Hermes plan's note on the `Max()` race condition

The Hermes plan's CR-2 says: "The repositories expose `NextIdAsync()` for the handlers." The Final Action Plan correctly identifies the `Max()+1` race condition as a problem and prescribes `Create(0)`. But the Hermes plan's note that the `Max()` strategy "can fail in race conditions" is a real concern that the Final Action Plan does not surface in the same way. The Final says only "do NOT copy the `Max()+1` pattern" — the Hermes plan's deeper concern (concurrent inserts producing duplicate IDs) is implicit in the Final's R-3 risk.

---

## Part 4 — Risks, gaps, and inconsistencies in the Final Action Plan

The Final Action Plan is materially better than the Hermes plan, but it is not without risks. The following items should be flagged before execution:

### 4.1 — `Test.Update` is preserved as-is; the finer-audit-trail cost is silent

The Final Action Plan's D-2 (line 484) preserves the existing single-bulk `Test.Update`. This means every edit of a test field goes through one mutator that accepts all 8 fields. The audit interceptor records the `Modified` event and increments `ModificationCount` once. The audit log says "Test X was modified at time T by user Y" — but not "Test X's `Name` was changed from 'A' to 'B'". If the owner later needs per-field audit (e.g., to investigate who changed a price), this information is not available.

This is a real cost. The Final Action Plan does not record it as a risk. Recommend: add R-11 "per-field audit granularity lost" with a mitigation of "record before/after in audit log if M-19 Statistics or a regulatory audit requires it" — but this would require a schema change, which is out of scope for M-12.

### 4.2 — The validator-DI fix (R-1) is conditional but the M-12 validation gates are not

The Final Action Plan's S4 step 4 verifies the validator-DI gap and prescribes `AddValidatorsFromAssembly` as the fix. But if the gap is confirmed, the fix is a one-line change in `src/TopLab.Application/DependencyInjection.cs` that affects M17 and M22 as well as M-12. The Final Action Plan correctly says "this also silently benefits M17/M22" — but a coding agent reading the M-12 plan should not change M17/M22 behavior without explicit owner approval. The Final Action Plan does not require the owner to approve this cross-cutting change.

Recommend: add an explicit pre-flight check ("if R-1 is confirmed, ask the owner before applying the fix, because it affects M17/M22 tests too").

### 4.3 — The LocalDB integration test (R-3) is described as "optional" via skip-when-unavailable

The Final Action Plan line 392 says the LocalDB-guarded identity test is "skipped gracefully when no LocalDB is available". This is honest about environment constraints, but it leaves the door open to a CI that never runs the test. If the CI build agent does not have LocalDB, the sentinel-id behavior is never actually tested. The Hermes plan's `Max()+1` strategy at least works deterministically with the InMemory provider (because the test asserts `test.Id.Value == 1` which is what the handler passes in, regardless of the DB).

Recommend: require the LocalDB integration test to be green before declaring M-12 Done. If the CI cannot run it, fail the build with a clear message.

### 4.4 — `ViewModelBase` is referenced but not documented in the Final Action Plan

The Final Action Plan line 40 names `ViewModelBase`, `RelayCommand`, `AsyncRelayCommand` under `src/TopLab.Presentation/Common/`. The 6 ViewModels (S5, S6) and the S7 shell-wiring change use these classes. But the Final Action Plan does not specify which methods of `ViewModelBase` are used (`SetProperty`, `RaisePropertyChanged`, `OnPropertyChanged`) or how `RelayCommand` and `AsyncRelayCommand` differ. A coding agent would have to read `ViewModelBase.cs` and the precedent `UserManagementViewModel.cs` to know.

This is a minor gap. The Hermes plan has the same gap (it says "RelayCommand" without specifying). Both plans assume the implementer reads the precedent.

### 4.5 — The Final Action Plan does not specify what the `IDialogService` interface looks like for `ShowConfirmationAsync`

The Final Action Plan line 245 says the Reference Ranges editor "Delete shows `ShowConfirmationAsync` with copy explaining historical results are unaffected (BR-05/FR-M12-006)". But the existing `IDialogService` in `src/TopLab.Presentation/Common/Dialogs/` is not enumerated. Does it have `ShowConfirmationAsync`? If not, M-12 has to add it. The Final Action Plan does not flag this.

Recommend: explicit pre-flight check that `IDialogService.ShowConfirmationAsync` exists, or add the method to the interface and document the M-19 deviation in the handoff.

### 4.6 — The `TestGroupsViewModel` does not include delete (E-9); the Final says this is fine but does not surface the FK SetNull behavior to the user

The Final Action Plan line 231 says "**No delete** (E-9: no PRD requirement; FK is SetNull so deletion semantics would need an owner decision — recorded as an open point, default = not offered)". The owner sees a Test Groups screen with no delete button. The owner sees a test whose `TestGroupId` becomes NULL (because the group was deleted by a different user in another session, or by direct DB access). The Final Action Plan does not specify how the catalog screen surfaces a test whose group is null.

This is a minor UX gap. Recommend: in the catalog, when a test's group is null, show "(no group)" rather than an empty cell. The Final Action Plan's DTO `TestSummaryDto` has `TestGroupId?` and `TestGroupName?` — the existing structure supports this.

### 4.7 — The Final Action Plan's S4 step 4 (R-1) is a check, not a fix

The Final Action Plan says "if null → add `AddValidatorsFromAssembly` in `AddApplication` + regression test". This is correct, but it conflates "diagnose" with "fix". A coding agent reading this might:
- Diagnose, find null, add the fix, add a test.
- Diagnose, find null, *decide not to fix* because of cross-cutting concerns, and leave the M-12 validators unresolved.

The Final Action Plan should say "if null → fix is mandatory before any M-12 validator test can be trusted to pass; the fix affects M17/M22 also; record this in the handoff". Recommend: rephrase as a hard precondition, not an optional mitigation.

### 4.8 — The 506-line document is hard to navigate

The Final Action Plan has 16 numbered sections, two appendices, and 18+ inline code snippets. A coding agent that needs to find the exact file to create for a specific slice will spend non-trivial time scrolling. A 1-page table-of-contents with file paths per slice would make this more usable. The Hermes plan's 8-slice structure is easier to navigate, even if the content is less rich.

This is a stylistic point. The Final Action Plan's detail is its strength; a TOC would not weaken it.

### 4.9 — The Final Action Plan's "R-1: validators never execute" claim should be flagged more loudly

The Final Action Plan's R-1 (line 429) is a **strongly supported conclusion, not yet runtime-proven**. The current M-12 validation gates (and M17's, M22's) cannot be trusted until this is fixed. If a coding agent executes S2/S3 in order, writes validators, runs the test suite, sees all green, and concludes "validators are wired up correctly" — the agent would be wrong, because the tests construct behaviors manually and bypass the DI registration gap. The R-1 is a pre-flight check that must be elevated to a hard prerequisite, not a "verification step" in S4.

Recommend: renumber R-1 to R-0 and require it as the first step in §16 (pre-flight), not step 4 of S4.

### 4.10 — The Final Action Plan's atomic `SaveWorkGroupLogItems` is presented as M-12's choice, but the OpenCode plan's separate `AddWorkGroupLogItem`/`RemoveWorkGroupLogItem` is a valid alternative

The Final Action Plan's S3 (line 200) uses `SaveWorkGroupLogItemsCommand` (atomic replace) with a precedent in `SaveUserPermissionsCommandHandler`. The Hermes plan's Q2-style open question on Test updates aside, the Final does not present the atomic-vs-delta choice as an open decision for the owner. The OpenCode plan (line 363-369) explicitly defined `SaveWorkGroupLogItems` with the same atomic semantics. The Hermes plan used separate `Add`/`Remove`. Both are defensible.

This is a minor gap. The Final Action Plan's choice is the safer one (atomicity, single transaction). But the owner should be asked. Recommend: add to D-X "atomic work-group items replace is the default; if the owner prefers delta semantics, add separate AddItem/RemoveItem commands" and link to the OpenCode draft for reference.

---

## Part 5 — Summary

| Aspect | Hermes M-12 | OpenCode M-12 | Final Action Plan |
|---|---|---|---|
| Line count | 136 | 1,144 | 506 |
| Bytes | 30,840 | 56,486 | 71,816 |
| Architectural assumptions verified against code | 4 of 8 (IUnitOfWork, CommunityToolkit.Mvvm, EF.Functions.Like, Max()+1) | Partial (some C# snippets show correct usage but the surrounding prose is wrong) | All major assumptions verified (D-1 to D-11) |
| Critical errors | 4 (Max()+1, DomainEvent, IUnitOfWork, WPF lib) | 1–2 (DomainEvent, maybe WPF lib) | 0 (all rejected with evidence) |
| Test strategy alignment | High (S8 specifies the audit-interceptor verification) | High (Test Strategy §3.4 respected) | Highest (every clause cited, with explicit R-1, R-3, R-4, R-5) |
| Navigability | Excellent (8 slices, 1 page each) | Poor (1,144 lines, file-by-file detail inline) | Good (16 sections, 18 file lists) |
| Edge-case coverage | Implicit (in validation gates) | Moderate (inline in slice specs) | Excellent (14 edge cases, 10 risks) |
| Plan-only discipline (no code) | Yes | No (snippets) | No (snippets) |
| Honest about uncertainty | High (Q1, Q2 explicit open decisions) | Moderate | Moderate (D-1 to D-11 presented as resolved; U-1 to U-5 in Appendix B) |

**Final verdict on the original question (a):** The Final Action Plan is **materially better** than the Hermes M-12 plan. The Hermes plan is well-structured and has good governance hooks (human-confirmation-before-commit, slice-by-slice validation gates), but it is grounded in four factually incorrect architectural premises that the Final Action Plan rejects with cited evidence. Executing the Hermes plan as written would produce code that does not compile (CommunityToolkit.Mvvm is not a package; `IUnitOfWork` is not an interface), code that violates the project's architecture (Domain events), and code that creates database-identity collisions (`Max()+1` on `IDENTITY(1,1)` columns). The Final Action Plan catches all of these before any code is written.

**Final verdict on the original question (b):** The Final Action Plan got right, that the Hermes plan missed, 18 specific items (rows 1–18 in Part 1). The most consequential are: rejecting the `Max()+1` ID strategy, rejecting the DomainEvent per-field mutator pattern, rejecting the `IUnitOfWork` layer, and surfacing the validator-DI latent defect.

**Final verdict on the original question (c):** The Hermes plan got right, that the Final Action Plan missed or under-weighted, 7 items (Part 3). The most consequential are: the audit-trail cost of bulk `Test.Update` is silent in the Final, the human-confirmation-before-commit policy is not referenced in the Final, and the Final's per-decision "resolved by evidence" framing hides that some decisions (D-2 in particular) are actually owner preferences.

**Final verdict on the original question (d):** The Final Action Plan has 10 risks and gaps to flag (Part 4). The most important are: R-1 should be elevated from a "verification step in S4" to a "pre-flight prerequisite" (otherwise M-12 validators will not work); the LocalDB integration test must be required (not skippable) for declaring M-12 Done; the `Test.Update` bulk-mutator decision loses per-field audit granularity and this should be recorded in the handoff; and the cross-cutting validator-DI fix affects M17/M22 and should require owner approval before being applied.

**Recommendation to the owner:** Adopt the Final Action Plan. The Hermes M-12 plan should be retired or, at most, retained as a high-level index document (8-slice structure, governance hooks) that points to the Final Action Plan as the operational source of truth. The Final Action Plan is the artifact an implementer should follow; the Hermes plan is the artifact a reviewer should consult for the workflow governance (human-confirmation-before-commit, validation gates, open-decision framing) that the Final Action Plan does not re-state.

---

## Part 6 — Specific items the Final Action Plan should add to fully supersede the Hermes plan

If the owner wishes to make the Final Action Plan a complete drop-in replacement, recommend the following additions:

1. **Pre-flight section (step 0 of §16) elevated from a checklist to a hard prerequisite:** the R-1 validator-DI check, the WorkGroupLogItem call-site grep, and the LocalDB availability check. All three must pass before S1 begins.

2. **Explicit reference to the human-confirmation-before-commit policy:** a one-line note in §6 ("every slice ends with a drafted commit message and a STOP for owner approval; the coding agent does not run `git commit` or `git push`"). This is workflow governance, not plan content, but it is the difference between an M-12 implementation that respects the prior task's policy and one that does not.

3. **Open-decision section (Appendix C, parallel to Appendix B's uncertainties):** PatientTitle-scope, single-default PatientTitle (D-11), Test/TestGroup delete (E-9), and `Test.Update` per-field-vs-bulk (D-2) presented as owner decisions with explicit recommendations, not as "resolved by evidence" with the evidence being the project's own prior documentation (which the owner can change).

4. **A 1-page slice-by-slice execution index** at the top of the file, mapping each of the 8 slices to its 3–5 key actions, the files it touches, and its exit criteria. This is the "Hermes-style" structure the Hermes plan had; the Final Action Plan has the detail but not the index.

5. **A risk named R-11: per-field audit granularity lost.** The Final Action Plan's D-2 (bulk `Test.Update`) loses the per-field audit trail. This is a real cost. Recording it as a risk (with mitigation "add a future in-process Domain-event mechanism when regulatory audit requires it") makes the cost visible.

---

*End of comparative review. This document is a new file at `Docs/Hermes/M-12-Comparative-Review.md`. Neither `Docs/Hermes/M-12.md` nor `Docs/Final Action Plan for Functional Unit 12.md` was modified in any way during the production of this review.*
