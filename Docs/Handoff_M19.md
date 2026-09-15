# Top-Lab — Handoff Document M-19

## نظام توب لاب — تسليم جلسة عمل (Module 19 — Statistics)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-15_M19-statistics` |
| Session date (UTC) | 2026-09-15 |
| Session start (UTC) | 2026-09-15 |
| Session end (UTC) | 2026-09-15 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-19 |
| Module name | Statistics |
| Wave | 8 |
| Feature folder(s) touched | `src/TopLab.Application/Features/Statistics/**`, `tests/TopLab.Application.Tests/Features/Statistics/`, `tests/TopLab.Application.Tests/DependencyInjection/ValidatorRegistrationTests.cs` |
| Layers touched | Application + tests + close-out docs only (zero Domain, zero Infrastructure, zero Presentation) |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `6dd1604` (live `main` HEAD after M-18 + Arabic note) |
| Final commit at session end | `[M-19] Slice 4/4: Tests + zero-drift proof + close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 19 **Statistics** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-19.md`. Deliver four `STATISTICS`-gated (seeded id=12), read-only Application projections over the existing schema: patient counts classified by sex / referral entity / account type with optional monthly breakdown (soft-deleted excluded); test counts per test and per test group with optional group filter; sent-out statistics per destination lab with totals via the existing `SentOutAccountCalculator` only; and user productivity from `PatientTest` attribution timestamps with no M-18 coupling. Zero Domain change, zero migration, zero Presentation. S4 proves zero drift, extends validator registration, appends ADR-0045, flips the tracking sheet, and creates this handoff. Local-only commits; no remote pushes.

---

## 3. Achievements This Session (Required)

- **S1 — Patient statistics + authorization theory shell** — Implementation Complete — `StatisticsDtos` + `StatisticsAccessPolicy.Statistics`; `GetPatientCountStatisticsQuery` (`IAuthorizedRequest` → `STATISTICS`) with handler (half-open UTC period on `RegistrationDateUtc`, default-today, sex/referral/account-type classifications, optional monthly + month×sex cross-buckets per U1, soft-deleted excluded, null-referral «بدون جهة إحالة», raw-id fallback) + validator (`From > To` frozen message); handler tests (13) + `StatisticsAuthorizationTests` theory shell (denial verbatim + absolute bypass). Commit `c68e25c`.
- **S2 — Test statistics + theory extension** — Implementation Complete — DTOs `TestCountDto`/`TestGroupCountDto`/`TestCountStatisticsDto`; `GetTestCountStatisticsQuery` (period on `PatientTest.CreatedAtUtc`, per-test + per-group counts, optional `TestGroupId` filter, unknown group → `مجموعة التحاليل غير موجودة.`, deleted-patient exclusion) + validator; handler tests (8) + theory extended. Commit `cf8566e`.
- **S3 — Sent-out + user-productivity statistics** — Implementation Complete — DTOs `SentOutLabStatisticsDto`/`SentOutStatisticsDto`/`UserProductivityDto`/`UserProductivityStatisticsDto`; `GetSentOutStatisticsQuery` (period on `SentAtUtc`, per-lab counts + `SentOutAccountCalculator` totals only — grep-gated, optional lab filter, unknown lab → `الجهة الخارجية غير موجودة.`); `GetUserProductivityStatisticsQuery` (four attribution counts each on its own timestamp, zero-activity omitted, deleted user → raw id, no M-18 coupling); 2 handler-test classes (13) + theory complete for all four queries. Commit `9defb1b`.
- **S4 — Zero-drift proof + close-out** — Implementation Complete — `has-pending-model-changes` → no changes; snapshot untouched; M19 validator-registration theory (4 cases); Release/full suite green; ADR-0045 appended; tracking sheet M19 row flipped 🟩 Done + Wave 8 summary + dated §9 change-log row; `Handoff_M19.md` created.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build

- Solution builds locally: Yes.
- Errors: 0. Warnings: 0.

### 4.2 Tests

- All existing tests still pass: Yes.
- Full Application suite after S4: **1272 green** (1222 prior + 46 new Statistics handler/auth tests + 4 validator-registration cases). Full solution suite **1839** = 413 Domain + 1272 Application + 154 Infrastructure.
- New tests: S1 +15 (13 handler + 2 auth); S2 +8 handler + theory extension; S3 +13 handler + auth completion; S4 +4 validator-registration cases.
- Tests currently failing: none.
- Coverage: per-slice footprint gates applied under the M-11/M-14 waiver posture (module touches a subset of Application files; whole-project floors inapplicable). All new handler/validator/policy/DTO paths exercised by the S1–S3 tests.

### 4.3 Migrations

- New EF Core migration(s) added: **None**.
- `has-pending-model-changes` at close-out: No changes — zero drift. Snapshot unchanged.
- Any manual schema change made outside a migration: No.

### 4.4 Dependency Injection wiring

- New registration: none (handlers take the existing `IApplicationDbContext`/`IDateTimeProvider`; MediatR + validator assembly scan cover the new types).
- Validators: 4 new query validators resolve via the existing assembly scan; resolution pinned by the M19 validator-registration theory; no DI wiring change.
- Composition-root changes (`App.xaml.cs`): none.

### 4.5 Configuration

- New application configuration keys added: none.
- Changes to `.editorconfig` or solution-level configuration: none.
- Changes to the `PermissionConfiguration.cs` seed: **none** — `STATISTICS` (id=12) already seeded; reused as-is (SD-19-2).

---

## 5. Work In Progress (Required — mark "None" if none)

None.

---

## 6. Contracts / API Surface (Required)

Four MediatR queries, all `IAuthorizedRequest<Result<…>>` with `RequiredPermissionCode => "STATISTICS"`:

| Query | Parameters | Result DTO |
|---|---|---|
| `GetPatientCountStatisticsQuery` | `DateOnly? From, DateOnly? To, bool BySex, bool ByReferralEntity, bool ByAccountType, bool GroupByMonth` | `PatientCountStatisticsDto` |
| `GetTestCountStatisticsQuery` | `DateOnly? From, DateOnly? To, int? TestGroupId` | `TestCountStatisticsDto` |
| `GetSentOutStatisticsQuery` | `DateOnly? From, DateOnly? To, int? ExternalLabEntityId` | `SentOutStatisticsDto` |
| `GetUserProductivityStatisticsQuery` | `DateOnly? From, DateOnly? To, int? UserId` | `UserProductivityStatisticsDto` |

Shared DTOs in `Features/Statistics/Common/StatisticsDtos.cs`. Access policy: `StatisticsAccessPolicy.Statistics = "STATISTICS"`.

---

## 7. Notes for the Incoming Agent (Required)

- **SD-19-2 gating:** All four queries require `STATISTICS` (id=12). Absolute-permission users bypass via the existing `AuthorizationBehavior`. Do not add a second gate in handlers.
- **SD-19-3 soft-delete:** Soft-deleted patients are **excluded** from patient and test statistics (operational reporting). This is a deliberate contrast with M-10 audit (which includes them).
- **SD-19-4 productivity source:** Productivity is self-contained from `PatientTest` attributions. **Do not couple to M-18 attendance data.** Printed count uses `LastPrintedByUserId` with summed `PrintCount` (reprints attributed to the last printer — U2).
- **SD-19-5 calculator:** Sent-out financial totals must come only from `SentOutAccountCalculator`. Never restate `Cost − Paid` in Application (grep gate).
- **Periods:** All four queries share DateTime half-open UTC bounds, inclusive calendar days, default-today when omitted, `From > To` → «بداية الفترة يجب ألا تتجاوز نهايتها.».
- **Appendix A frozen messages:** inverted period; «بدون جهة إحالة»; «مجموعة التحاليل غير موجودة.»; «الجهة الخارجية غير موجودة.»; pipeline Forbidden «أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام».
- **Statistics printing** is out of scope (later Reporting concern). No Presentation content was added.
- **No Domain change, no migration, no `PermissionConfiguration` change** — keep it that way unless a new plan explicitly requires otherwise.

---

## 8. Artifacts / Attachments (Optional)

- Plan: `Docs/OpenCode/M-19.md`
- Memory: `Docs/OpenCode/M-19-memory.md`
- ADR: `Docs/Source/Top_Lab_ADR.md` → ADR-0045
- Tracking: `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` M19 row + §9 change-log

---

## 9. Acceptance / Sign-off (Optional)

| Criterion | Status |
|---|---|
| Release/full build 0 errors / 0 warnings | Yes |
| Full suite green | Yes — **1839** |
| Zero migration / zero drift proven | Yes (`has-pending-model-changes` → no changes) |
| Zero Domain / zero Persistence / zero Presentation content | Yes |
| ADR-0045 + tracking flip + handoff present | Yes |
| Local commits only, never pushed | Yes |

---

*End of handoff.*
