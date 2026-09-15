# Loop Engineering — Memory File

- **Module:** Case Tracking, Audit & Traceability (P/T) (M-10)
- **Module Number:** M-10
- **Source Plan:** Docs/OpenCode/M-10.md
- **Date Created:** 2026-09-15
- **Total Slices:** 3
- **Current Slice:** S3 — done (module complete)
- **Current Branch:** main
- **Author:** loop-engineering skill (execution carried out by the executing agent per owner authorization; stage-10 auto local commit authorized by owner, never push)

---

## Module Summary

Delivers the restricted P/T audit & traceability read surface over the already-complete physical schema (zero migration, zero Domain change): the gated P view (`GetPatientAuditQuery` — registering user, modification count, last modifier, payment-receiving users incl. voided operations, soft-deleted patients included) and the gated T view (`GetPatientTestAuditQuery` — entered/reviewed/printed+count/delivered with users and UTC times). Both queries carry `IAuthorizedRequest` with `PT_AUDIT_ACCESS` (seeded id=13; absolute-permission bypass via the pipeline — FR-M17-008). Name resolution via a Users dictionary with raw-id fallback. Zero Presentation content. Done means: S1 P view plus tests, S2 T view plus tests, S3 zero-drift gate plus validator registration plus ADR-0043 plus tracking flip plus handoff plus full-suite green plus coverage floors.

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
- Stop threshold: 5 consecutive failures for the same reason.
- Execution order: strictly sequential S1 -> S2 -> S3, no parallel slices.
- Stage 7 gate: the plan's textual exit criteria (build/test/grep/model-assertion) replaces any standard UI journey — M-10 has no UI.
- Git: automatic LOCAL commit after each verified slice (no confirmation pause), on the CURRENT branch (main), NEVER create a new branch, NEVER push to any remote. Commit message format: `[M-10] Slice N/3: <slice title> — loop-engineering`.
- The ONLY normal stopping point (no report needed) is full completion of every slice in M-10's plan.

## Slice Validation Gates (from plan)

| Slice | Gate ID | Gate Description | How to Verify |
|-------|---------|------------------|---------------|
| 1 | VG-01 | P view: `src/TopLab.Application` builds zero/zero; S1 handler + authorization tests green (all P fields, receivers distinct/ordered, voided included, unknown patient NotFound, soft-deleted patient returned, raw-id fallback, verbatim denial + absolute bypass); zero `Persistence/**` diff (grep gate); Application S1 footprint coverage ≥ 80%. **Migration: none required by this slice** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 2 | VG-02 | T view: Application builds zero/zero; S2 tests green (full lifecycle, partial lifecycle nulls, PrintCount passthrough, unknown test NotFound, raw-id fallback, authorization theory complete); zero `Persistence/**` diff (grep gate); Application S2 footprint coverage ≥ 80%. **Migration: none required by this slice** | `dotnet build src/TopLab.Application`; `dotnet test tests/TopLab.Application.Tests`; grep gate |
| 3 | VG-03 | Zero-drift + close-out: Release build zero/zero; full suite green (`-m:1`); `dotnet ef migrations has-pending-model-changes` → no changes; snapshot unchanged; validator-registration extension green; coverage floors met or waived; ADR-0043 appended; M10 tracking row flipped; `Handoff_M10.md` per template; zero writes / zero Domain changes / zero Presentation content (grep gates) | `dotnet build TopLab.sln -c Release`; `dotnet test TopLab.sln -m:1`; coverage report; diff inspection |

---

## Slice Index

| # | Slice Title | Status | Validation Gate |
|---|-------------|--------|-----------------|
| 1 | Application read surface: the P view (patient-record audit) | [x] Done | VG-01 ✅ |
| 2 | Application read surface: the T view (per-test lifecycle audit) | [x] Done | VG-02 ✅ |
| 3 | Tests + zero-drift proof + close-out | [x] Done | VG-03 ✅ |

---

## Slice 1: Application read surface: the P view (patient-record audit)

- **Goal:** Ship the `PT_AUDIT_ACCESS`-gated `GetPatientAuditQuery` with the full P field set, the receiver rollup (voided included), dictionary name resolution with raw-id fallback, and the authorization theory shell.
- **Touches:** `src/TopLab.Application/Features/AuditAndTraceability/Common/AuditDtos.cs` (create); `.../Common/AuditAccessPolicy.cs` (create); `.../Queries/GetPatientAudit/` (3 files, create); `tests/TopLab.Application.Tests/Features/AuditAndTraceability/GetPatientAuditQueryHandlerTests.cs` (create); `.../AuditAuthorizationTests.cs` (create)
- **Validation Gate:** VG-01 — Application build zero/zero; S1 tests green; zero-Persistence grep gate; coverage ≥ 80%. Migration: none.

### 10-Stage Progress (Slice 1)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln` 0/0; `dotnet test TopLab.sln` full suite green (390+149+1157) before touching anything.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S1 re-read; SD-10-1…SD-10-9; verbatim Appendix A messages.
- [x] **Stage 3 — File Analysis:** `Patient.cs` audit columns, `PaymentOperation.cs:19-23`, `AuditableEntity.cs`, `AuthorizationBehavior.cs:34-35`, `PermissionConfiguration.cs:30`, `PatientBillingReader.cs:67-84` (dictionary name-resolution + raw-id fallback precedent), `FakeApplicationDbContext`/`FakeCurrentUserService`.
- [x] **Stage 4 — Planning:** DTOs → access policy → query + validator → handler → 2 test classes, encoded in this checklist.
- [x] **Stage 5 — Execution:** Implemented per plan (5 Application files + 2 test classes; receiver rollup = latest `OperationAtUtc` per receiver, ascending order per U1/U2).
- [x] **Stage 6 — Post-Execution Verification:** `dotnet build src/TopLab.Application` 0/0; S1 filter green (10/10); full Application suite green (1167/1167).
- [x] **Stage 7 — Validation Gate:** VG-01 PASS — build 0/0; tests green; zero `Persistence/**` + zero `Domain/**` diff (grep gates); S1 footprint coverage 54/58 = 93.1% ≥ 80%; migration none.
- [x] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 2: Application read surface: the T view (per-test lifecycle audit)

- **Goal:** Ship the gated `GetPatientTestAuditQuery` with the full lifecycle attribution, partial-lifecycle nulls, and `PrintCount`; complete the authorization theory.
- **Touches:** `src/TopLab.Application/Features/AuditAndTraceability/Common/AuditDtos.cs` (modify: add `PatientTestAuditDto`); `.../Queries/GetPatientTestAudit/` (3 files, create); `tests/TopLab.Application.Tests/Features/AuditAndTraceability/GetPatientTestAuditQueryHandlerTests.cs` (create)
- **Validation Gate:** VG-02 — Application build zero/zero; S2 tests green; zero-Persistence grep gate; coverage ≥ 80%. Migration: none.

### 10-Stage Progress (Slice 2)

- [x] **Stage 1 — Pre-Execution Verification:** build 0/0 + full suite green (390+149+1167) before touching anything.
- [x] **Stage 2 — Deep Understanding:** Plan §5 S2 re-read; SD-10-6; null-mapping rules.
- [x] **Stage 3 — File Analysis:** `PatientTest.cs` lifecycle columns + mutators; S1 DTO file; name-dictionary union pattern; `Test` catalog name precedent (`PatientBillingReader.cs:50-62`).
- [x] **Stage 4 — Planning:** DTO addition → query + validator → handler → test class; complete the theory.
- [x] **Stage 5 — Execution:** Implemented per plan (DTO extended; 3 T-view files; 1 test class; auth theory completed with T-query denial + absolute-bypass facts).
- [x] **Stage 6 — Post-Execution Verification:** build 0/0; S2 filter green (17/17); full Application suite green (1174/1174).
- [x] **Stage 7 — Validation Gate:** VG-02 PASS — build 0/0; tests green; zero `Persistence/**` + zero `Domain/**` diff (grep gates); footprint coverage 125/133 = 94.0% ≥ 80%; migration none.
- [x] **Stage 8 — Documentation Update:** This checklist + evidence recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Slice 3: Tests + zero-drift proof + close-out

- **Goal:** Prove zero drift, extend validator registration, pass coverage/slopwatch, and close out (ADR-0043, tracking flip, handoff).
- **Touches:** `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` (extend); `Docs/Source/Top_Lab_ADR.md` (append ADR-0043 — reconfirm max ADR at execution); `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` (flip M10 row + dated change-log row); `Docs/Handoff_M10.md` (create per template)
- **Validation Gate:** VG-03 — Release build zero/zero; full suite green; zero-drift proven; docs committed per convention; zero writes/Domain/Presentation content (grep gates).

### 10-Stage Progress (Slice 3)

- [x] **Stage 1 — Pre-Execution Verification:** `dotnet build TopLab.sln -c Release` 0/0 before touching anything.
- [x] **Stage 2 — Deep Understanding:** Plan §6 S3 re-read; drift → stop + addendum (never silent migration); ADR-0043 contents; close-out convention.
- [x] **Stage 3 — File Analysis:** `ValidatorRegistrationTests` per-module theory pattern; ADR max reconfirmed at ADR-0042 → ADR-0043; M10 tracking row (line 81) + §6 block + §9 log format; `Handoff_M16.md` structure precedent.
- [x] **Stage 4 — Planning:** Drift gate first → validator-reg extension → Release full suite → ADR → tracking → handoff.
- [x] **Stage 5 — Execution:** Implemented per plan (drift clean; M10 registration theory 2 cases; ADR-0043; tracking flip + §6 + §9; `Handoff_M10.md`).
- [x] **Stage 6 — Post-Execution Verification:** Release build 0/0; Release full suite green `-m:1` (390+1176+149 = 1715); drift gate → no changes; snapshot untouched.
- [x] **Stage 7 — Validation Gate:** VG-03 PASS — Release 0/0; full suite green; zero-drift proven; validator-reg extension green (2/2); Slopwatch 0 issues on touched dirs; ADR-0043 + tracking flip + handoff present; zero writes / zero Domain / zero Presentation (grep gates).
- [x] **Stage 8 — Documentation Update:** ADR-0043 appended; M10 row flipped 🟩 Done + §6 block + dated §9 row; `Docs/Handoff_M10.md` created per template; this checklist recorded.
- [x] **Stage 9 — Memory Status Update:** "Current Status" updated; module close-out recorded.
- [x] **Stage 10 — Git Commit (authorized local):** See Execution Log.

---

## Current Status

- Overall: 3/3 slices done — MODULE COMPLETE
- Slice 1 — Application read surface: the P view: [x] Done (VG-01 pass 2026-09-15)
- Slice 2 — Application read surface: the T view: [x] Done (VG-02 pass 2026-09-15)
- Slice 3 — Tests + zero-drift proof + close-out: [x] Done (VG-03 pass 2026-09-15)

## Execution Log

| Date (YYYY-MM-DD) | Slice | Stage | Action | Result | Commit |
|-------------------|-------|-------|--------|--------|--------|
| 2026-09-15 | 0 | — | Memory file created | OK | — |
| 2026-09-15 | 1 | 1–7 | S1 P view: 5 Application files + 2 test classes; build 0/0; 10/10 S1 tests + 1167 full suite green; zero Persistence/Domain diff; coverage 93.1% (VG-01 PASS) | OK | — |
| 2026-09-15 | 2 | 1–7 | S2 T view: DTO extended + 3 query files + 1 test class, auth theory completed; build 0/0; 17/17 + 1174 full suite green; zero Persistence/Domain diff; coverage 94.0% (VG-02 PASS) | OK | — |
| 2026-09-15 | 3 | 1–7 | S3 close-out: drift clean (no changes); M10 validator-reg theory 2/2; Release 0/0 + full suite 1715 green (-m:1); Slopwatch 0 issues; ADR-0043 + tracking flip + Handoff_M10 (VG-03 PASS) | OK | — |

## Stop Report (append only if a stop condition triggers)
