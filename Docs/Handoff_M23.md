# Top-Lab — Handoff Document M-23

## نظام توب لاب — تسليم جلسة عمل (Module 23 — Utilities / Tools)

---

## 1. Session Header (Required)

| Field | Value |
|---|---|
| Handoff document ID | `Handoff_2026-09-15_M23-utilities-tools` |
| Session date (UTC) | 2026-09-15 |
| Session start (UTC) | 2026-09-15 |
| Session end (UTC) | 2026-09-15 |
| Outgoing agent / contributor | Local coding agent (Top-Lab) |
| Incoming agent / contributor (if known) | TBD |
| Module ID (`Mxx` or `Fx`) | M-23 |
| Module name | Utilities (Tools) |
| Wave | 10 |
| Feature folder(s) touched | `src/TopLab.Domain/Utilities/**`, `src/TopLab.Application/Features/Utilities/**`, `src/TopLab.Application/Common/Interfaces/IPurchasesListStore.cs`, `IPhoneBookStore.cs`, `src/TopLab.Infrastructure/Services/Json*.cs`, `src/TopLab.Infrastructure/DependencyInjection.cs`, `tests/**` |
| Layers touched | Domain + Application + Infrastructure (Services + DI only) + tests + close-out docs |
| Branch name | `main` (local-only commits; no branch switching, no pushes) |
| Pull request URL (if opened) | None |
| Baseline commit at session start | `770d451` (M-20 close-out HEAD) |
| Final commit at session end | `[M-23] Slice 4/4: Tests + zero-drift proof + close-out — loop-engineering` |

---

## 2. Session Objective (Required)

Implement Module 23 **Utilities (Tools)** end-to-end in the four slices S1–S4 of `Docs/OpenCode/M-23.md`. Deliver three pure Domain computation services, four ungated Application queries (conversion, calculation, stopwatch, Test Library), and two workstation-local JSON-backed lists (Purchases, Phone Book) behind Application ports, with zero EF migration. Close out with zero-drift proof, validator registration, ADR-0047, tracking flip, and this handoff. Backend only — no Presentation content.

---

## 3. Achievements This Session (Required)

- **S1 — Domain computation services + tests** — Implementation Complete — `MeasurementUnitConverter` (explicit pair table, unknown pair throws), `ArithmeticCalculator` (recursive-descent parser; division-by-zero `CalculatorException`), `StopwatchCalculator`; 47 Domain tests. Commit `1fd8263`.
- **S2 — Application queries + Test Library** — Implementation Complete — `ConvertMeasurementUnit`, `EvaluateCalculation`, `ComputeStopwatchElapsed` (provider-supplied end), `GetTestLibrary` (dictionary group names, unknown group → «مجموعة التحاليل غير موجودة.»); 15 Application tests. Also stabilized a pre-existing midnight-UTC flake in SampleCollection tests (unrelated to M-23; required for G0). Commit `2ac7ea9`.
- **S3 — Purchases list + phone book** — Implementation Complete — `IPurchasesListStore`/`IPhoneBookStore` ports; add/remove/toggle + list queries; `JsonPurchasesListStore`/`JsonPhoneBookStore` (atomic write, missing-file-empty, malformed → Unexpected); two DI singleton registrations; 23 Application + 6 Infrastructure tests. Commit `07a22b0`.
- **S4 — Zero-drift + close-out** — Implementation Complete — drift gate clean; M23 validator-registration theory (11 cases); ADR-0047; tracking M23 + Wave 10 flipped; `Handoff_M23.md`.

---

## 4. State of the Codebase at Handoff (Required)

### 4.1 Build
- Solution builds locally: Yes. Errors: 0. Warnings: 0.

### 4.2 Tests
- All existing tests still pass: Yes.
- New tests: S1 +47 Domain; S2 +15 Application; S3 +23 Application + 6 Infrastructure; S4 +11 validator-registration cases.
- Tests currently failing: none.
- Coverage: per-slice footprint gates under the M-11/M-14/M-19/M-20 waiver posture.

### 4.3 Migrations
- New EF Core migration(s) added: **None**.
- `has-pending-model-changes` at close-out: No changes — zero drift. Snapshot unchanged.

### 4.4 Dependency Injection wiring
- New registration: two singletons in `Infrastructure.DependencyInjection` (`IPurchasesListStore` → `JsonPurchasesListStore`, `IPhoneBookStore` → `JsonPhoneBookStore`).

### 4.5 Configuration
- New configuration keys: none.
- `PermissionConfiguration.cs` seed: **none** — utilities are ungated (SD-23-3).

---

## 5. Work In Progress (Required)

None.

---

## 6. Contracts / API Surface (Required)

| Member | Kind | Gate |
|---|---|---|
| `ConvertMeasurementUnitQuery(Value, FromUnit, ToUnit)` → `Result<ConversionResultDto>` | Query | none |
| `EvaluateCalculationQuery(Expression)` → `Result<CalculationResultDto>` | Query | none |
| `ComputeStopwatchElapsedQuery(StartUtc, EndUtc?)` → `Result<StopwatchElapsedDto>` | Query | none |
| `GetTestLibraryQuery(NameFilter?, TestGroupId?)` → `Result<IReadOnlyList<TestLibraryEntryDto>>` | Query | none |
| `AddPurchaseItemCommand(Text)` / `RemovePurchaseItemCommand(Id)` / `TogglePurchaseItemDoneCommand(Id)` → `Result` | Commands | none |
| `GetPurchasesListQuery()` → `Result<IReadOnlyList<PurchaseItemDto>>` | Query | none |
| `AddPhoneBookEntryCommand(Name, Phone, Notes?)` / `RemovePhoneBookEntryCommand(Id)` → `Result` | Commands | none |
| `GetPhoneBookQuery()` → `Result<IReadOnlyList<PhoneBookEntryDto>>` | Query | none |

Ports: `IPurchasesListStore`, `IPhoneBookStore` (Application Common Interfaces).

---

## 7. Notes for the Incoming Agent (Required)

- **SD-23-3:** Entire surface is ungated. Do not add `IAuthorizedRequest` unless a future plan explicitly requires it.
- **SD-23-2:** List persistence is workstation-local JSON under `%ProgramData%\TopLab` — never EF, never a business table.
- **SD-23-9:** Calculator is a hand-written recursive-descent parser. Never introduce `DataTable.Compute` or dynamic evaluation.
- **SD-23-11:** Tools Phone Book never references `Patient` or patient phone numbers.
- **SD-23-6/7:** Image Library and Shortcut Library backends are settled exclusions.
- **Frozen messages:** «زوج الوحدات غير مدعوم.», «التعبير الحسابي غير صالح.», «لا يمكن القسمة على صفر.», «وقت النهاية يسبق وقت البداية.», «مجموعة التحاليل غير موجودة.», «نص البند مطلوب.», «الاسم مطلوب.», «رقم الهاتف مطلوب.»
- **Zero Presentation content.** Tools navigation/UI is a Presentation concern.

---

## 8. Artifacts / Attachments (Optional)

- Plan: `Docs/OpenCode/M-23.md`
- Memory: `Docs/OpenCode/M-23-memory.md`
- ADR: `Docs/Source/Top_Lab_ADR.md` → ADR-0047
- Tracking: `Docs/Source/Top_Lab_Master_Tracking_Sheet.md` M23 row + Wave 10 + §9 change-log

---

## 9. Acceptance / Sign-off (Optional)

| Criterion | Status |
|---|---|
| Build 0 errors / 0 warnings | Yes |
| Full suite green | Yes |
| Zero EF migration / zero drift proven | Yes |
| Zero Presentation / zero business-schema change | Yes |
| ADR-0047 + tracking flip + handoff present | Yes |
| Local commits only, never pushed | Yes |

---

*End of handoff.*
