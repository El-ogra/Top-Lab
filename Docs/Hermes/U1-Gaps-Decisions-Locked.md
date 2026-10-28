# U1-Gaps — Decisions Locked (Phase 1 complete)

**Date:** 2026-10-28
**Status:** Phase 1 (owner decisions) COMPLETE. All 7 decision gaps resolved.
**Producer:** Agent 1 (Hermes), recording owner's final choices.

---

## Locked decisions

| Gap | Title | Decision | Migration |
|---|---|---|---|
| U1-GAP-002 | Primary sample type applied to ordered tests | **Option 1 — Pure Default** (applies only to tests ordered after it is set; no re-projection of existing tests) | Yes — new column on `Patients` (bit, nullable:false, default false) |
| U1-GAP-003 | Surcharge entry convention in cashiering panel | **Option 1 — Retain `+` prefix** (negative discount remains invalid) | No |
| U1-GAP-005 | Cash-receipt print-once enforcement | **Option 1 — Record emissions + audit trail** (new `ReceiptEmissions` table; repeat print requires explicit authorised re-print) | Yes — new `ReceiptEmissions` table (following `InvoiceIssues` precedent) |
| U1-GAP-013 | Registration-statistics quick action on patient editor | **Option 1 — In-Place Summary Panel** (compact panel/dialog on the registration screen, not navigation) | No |
| U1-GAP-014 | Explicit main-menu navigation action on patient editor | **Option 2 — Rely on Shell Ribbon** (no new control; ribbon is the single affordance) | No |
| U1-GAP-015 | Patient identification slip printing | **Option 2 — Paper ID Slip in scope** (reuse existing slip pipeline; plastic cards excluded) | No (slip itself). Geometry question (4in×2in hard-coded vs A5) to be settled before impl. |
| U1-GAP-016 | Remembered sign-in identity | **Option 1 — Workstation-Local Config** (`%ProgramData%\TopLab\appsettings.json`) | No |

---

## Consequences for execution order

- **Migration-gated gaps (Phase 3, after pure-code phase):** U1-GAP-002, U1-GAP-005 (each requires a migration plan sub-slice).
- **No-migration decision gaps (Phase 3):** U1-GAP-003, U1-GAP-013, U1-GAP-014, U1-GAP-015, U1-GAP-016 — straight code slices.
- **Pure-code gaps (Phase 2, unblocked now):** U1-GAP-004, U1-GAP-006, U1-GAP-007, U1-GAP-008, U1-GAP-009, U1-GAP-010, U1-GAP-011, U1-GAP-012.

## Open detail before implementation

- U1-GAP-015: confirm slip geometry (reuse 4in×2in tube-label geometry vs A5 report slip) before the slice. Recommendation: A5 (existing GroupPatientSlipPdfWriter) since it already carries name + LabId + barcode.

---

## Next step

Phase 2 begins with the **search-filters wave** (U1-GAP-006 + 007 + 008 + 009): one Application slice (extend `SearchPatientsGlobalQuery` tail with 4 params + predicates + validator + tests) + one Presentation slice (tri-state controls + viewmodel + tests). No migration. Atomic slices, stop for "go".

Awaiting owner "go" to start Slice 006-009-S1 (Application).