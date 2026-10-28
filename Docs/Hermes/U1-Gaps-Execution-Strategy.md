# U1-Gaps Execution Strategy — Agent 1 (Hermes)

**Date:** 2026-10-28
**Scope:** Unit 1 functional-parity gaps U1-GAP-002 … U1-GAP-016 (15 items). U1-GAP-001 is complete (build 0/0, 3725/3727 tests green, migration `20261028142027_AddPatientFollowUpFlag` applied).
**Status:** STRATEGY ONLY — no code modified, no migration applied.

---

## 0. Where we are

- Gap 1 (durable visit follow-up flag) is fully delivered: migration applied, build 0 warnings/0 errors, 3725 passed / 2 skipped (Docker), commit pending owner action.
- 15 gaps remain. Of them: **7 require an owner decision**, **4 require a migration** (conditional), **8 are pure code** (no decision, no migration).
- Owner is the final authority on product/domain. Owner personally performs every `git commit` and `git push`. Each slice stops for an explicit "go".

---

## 1. Gap classification (from the audit + inventory)

| # | Gap | Title | Decision? | Migration? | Coupling group |
|---|---|---|---|---|---|
| 002 | Primary sample type applied to ordered tests | Yes | Yes | — |
| 003 | Surcharge entry convention in cashiering panel | Yes | No | — |
| 004 | Discount % ↔ discount amount sync | No | No | — |
| 005 | Cash-receipt print-once enforcement | Yes | Conditional | — |
| 006 | Unfinished-results filter on patient search | No | No | Search filters |
| 007 | Unverified-results filter on patient search | No | No | Search filters |
| 008 | Unprinted-results filter on patient search | No | No | Search filters |
| 009 | Explicit search-mode selector on patient search | No | No | Search filters |
| 010 | Full test-catalogue browser for registration | No | No | Test catalog |
| 011 | One-click ordering of a master test group | No | No | Test catalog |
| 012 | Undelivered-results count indicator in shell | No | No | — |
| 013 | Registration-statistics quick action on patient editor | Yes | No | — |
| 014 | Explicit main-menu navigation on patient editor | Yes | No | — |
| 015 | Patient identification slip printing | Yes | Conditional | — |
| 016 | Remembered sign-in identity | Yes | Conditional | — |

**Owner-decision gaps:** 002, 003, 005, 013, 014, 015, 016 (7).
**Migration-gated gaps:** 002, 005, 015, 016 (4).
**Pure-code gaps:** 004, 006, 007, 008, 009, 010, 011, 012 (8).

---

## 2. Recommended execution order — 3 phases

### Phase 1 — Owner decision session (no code, one sitting)

Resolve all 7 decisions **before** any code is written. Each decision is presented as a short matrix (options, migration impact, recommendation, reasoning) exactly as gap-1's report did for its single decision. Batched into one session so execution never stalls mid-slice waiting for an answer.

Decisions to settle:
- 002 — how the primary sample type is applied to ordered tests (and whether it needs a new column).
- 003 — surcharge entry convention in the cashiering panel.
- 005 — print-once enforcement scope (conditional migration).
- 013 — registration-statistics quick action placement.
- 014 — main-menu navigation action placement.
- 015 — patient identification slip printing (conditional migration).
- 016 — remembered sign-in identity storage (conditional migration).

**Output of Phase 1:** a one-page "Decisions locked" note in `Docs/Hermes/` recording each choice + the resulting migration flag. No code.

### Phase 2 — Pure-code gaps (8 gaps, no decisions, no migrations)

Execute these first because they are unblocked and low-risk. Group the coupled ones so a shared contract changes once:

- **Slice group A — Search filters (006+007+008+009).** They all extend the same global search contract (`Text, Page, PageSize` → add filter flags + search-mode selector). One Application slice (query + handler + validator + tests) + one Presentation slice (view + viewmodel + tests). Two commits, not eight.
- **Slice group B — Test catalog (010+011).** Full catalogue browser + one-click master-group ordering on the registration screen. One Application slice + one Presentation slice.
- **Slice 004 — Discount % ↔ amount sync.** Standalone (ViewModel + tests).
- **Slice 012 — Undelivered-results count indicator in shell.** Standalone (small; touches the shell VM + a count query).

**Verification depth for Phase 2:** streamlined — a 5–10 claim spot-check per gap (file paths, permission codes, migration need) instead of the full 40+ claim table. The full gap-1-style report is reserved for migration-gated or permission-gated gaps.

### Phase 3 — Decision gaps (after Phase 1)

Order: no-migration decision gaps first, then migration-gated ones.

- 003, 013, 014 (no migration) — one slice each.
- 002, 005, 015, 016 (migration/conditional) — each gets a migration plan sub-slice (EF migration + snapshot check + `has-pending-model-changes` gate) before the code slice, mirroring gap-1's `AddPatientFollowUpFlag` flow.

---

## 3. Verification strategy (compressed)

For every gap, before implementing:

1. **Spot-check the load-bearing claims** from the audit/inventory against source: file paths, line references, permission codes, migration need, existing DTOs. (5–10 claims, not 40+.)
2. **Flag divergences** in a short note (like gap-1's "DIVERGENCES" section) — these are the real risk.
3. **Hard gate per slice:** `dotnet build TopLab.sln` 0 warnings/0 errors + `dotnet test TopLab.sln` 0 failed (Docker-skipped tests stay skipped; no slice depends on SQL Server).
4. **Migration-gated gaps additionally:** `dotnet ef migrations has-pending-model-changes` clean after the migration, snapshot reflects the new column, precedent pattern (bit, nullable:false, defaultValue:false) followed.

Full atomic claim tables are produced only for gaps that (a) add a migration, (b) change a permission gate, or (c) touch `Patient`/`PatientTest` domain invariants. The 8 pure-code gaps use the spot-check.

---

## 4. Atomic-slice discipline (unchanged)

- Each slice: implement → build 0/0 → tests green → local commit `بعد تنفيذ F <gap>-S<n>` → **STOP and wait for explicit "go"**.
- Do not chain slices unprompted.
- Owner performs every `git commit` and `git push`. The executing agent commits nothing and pushes nothing without an explicit instruction.
- Coupled grouping (search filters, test catalog) is allowed because the changes are genuinely interdependent (one shared contract), not because slices are being chained.

**Flexibility option for the owner:** the owner may issue "go for wave A" instead of "go" per slice; in that case the agent runs the wave's slices sequentially with a stop+commit between each, reporting after each. This keeps atomicity while reducing round-trips.

---

## 5. Isolation / head-to-head

- This agent (Hermes) writes its outputs under `Docs/Hermes/`. The peer (OpenCode) writes under `Docs/OpenCode/`. Strict isolation during comparison: no cross-folder reads; shared spec files live in `Docs/Source/` (and the audit/inventory files).
- The peer's doc-analysis output (shared by the owner) is treated as a lead, not as verified truth — every claim is re-checked against source before implementation, as gap-1 did.
- Throwaway test modules (if any) use the M-99 convention.

---

## 6. Risks to watch (cross-cutting)

- **Positional record growth** breaks call sites (gap-1 pattern: `StatisticsAuthorizationTests` had 3 sites). Any query record change must find all construction sites before the build gate.
- **Layering test** (`PresentationLayeringTests`) asserts an exact offender list — no Infrastructure `using` in ViewModels.
- **OrphanedViewModelTests** — new screens need a `NavigateTo<>` caller; sections inside an existing screen (like search filters) do not.
- **ValidatorRegistrationTests** — a new validator not in the theory data is silently never resolved.
- **Docker-dependent tests** stay skipped; never treat a skip as a failure.
- **EF snapshot drift** on migration gaps — verify `has-pending-model-changes` is clean.

---

## 7. Proposed next action

1. Owner reviews this strategy and either approves or adjusts the phase order.
2. If approved: Phase 1 decision session — I present the 7 decision matrices; owner picks.
3. Then Phase 2 begins with the search-filters wave (highest user-visible value, lowest risk).

No code is written until Phase 1 decisions are locked.