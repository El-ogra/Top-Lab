---
name: loop-engineering
description: "Use this skill for: (1) PLANNING — when user says 'خطط لـ', 'ابدأ التخطيط', 'صمم خطة تنفيذ', or any request to CREATE a plan before execution; (2) EXECUTION — when user asks to execute an implementation plan file in slices — including phrases like 'loop engineering', 'هندسة الحلقات', 'هندسه الحلقات', 'بروتوكول هندسة الحلقات', 'loop engineering protocol', 'loop engineering for [filename].md', 'apply loop engineering to [filename].md', or any request that mentions a .md plan file together with 'slice' / 'شريحة' / 'حلقة'. For planning: uses multi-agent workflow (planner + critic) to produce validated plan. For execution: reads the plan, generates a 10-stage loop-tracking memory file and an execution prompt. Do NOT use for: ad-hoc one-off scripts, design-only discussions without implementation, or pure documentation work without code."
license: MIT
compatibility: opencode
---

# Loop Engineering

Implements a 11-stage process: Phase 0 (multi-agent planning) followed by a 10-stage iterative loop for executing a software module plan in slices. Every slice is verified before and after execution. Failure to verify halts the loop.

## When to Activate

This skill activates automatically when the user's message matches the trigger phrases in the frontmatter description. When activated:

### For Planning (Phase 0)
If the user wants to CREATE a plan (not execute an existing one):
1. Extract the module name from the message.
2. Follow the planning workflow in `references/planning-phase.md`.
3. Guide the user through requirements → draft → critique → decisions → final plan.

### For Execution (Stages 1-10)
If the user wants to EXECUTE an existing plan:
1. Extract the plan filename (.md) from the message.
2. Locate the plan file at `docs/<filename>`, then `<project_root>/<filename>`, then any `docs/` subfolder.
3. If not found, output exactly: `لم يتم العثور على ملف الخطة [الاسم] في مجلد docs/ أو جذر المشروع. تحقق من اسم الملف وحاول مرة أخرى.`
4. Read the plan. Parse module name, module number, slice list, total count, validation gates.

## Phase 0 — Multi-Agent Planning (Optional Pre-Step)

Before the 10-stage execution loop, you can create a plan using multi-agent workflow:

### Planning Workflow
1. **Requirements Gathering** — Human provides module details
2. **Planner Agent** — Creates initial implementation plan
3. **Critic Agent** — Reviews and improves the plan
4. **Human Decisions** — Resolves open items
5. **Final Plan** — Clean plan ready for execution

For full details, read `references/planning-phase.md`.

### Quick Planning Commands
- `خطط لـ [اسم الوحدة]` → Start Phase 0
- `ابدأ التخطيط` → Start Phase 0
- `صمم خطة تنفيذ` → Start Phase 0

### After Planning Completes
Once the final plan is at `docs/[module-name]-plan.md`, execution begins:
- `لوب إنجنيرنغ [module-name]-plan.md` → Start Stage 1

---

## Anti-Hallucination Guard

If the plan file is empty, contains only headers, or has no identifiable slices, output:
`ملف الخطة فارغ أو لا يحتوي على شرائح قابلة للتحديد. يرجى تقديم خطة تحتوي على 3 شرائح على الأقل.`
Do NOT generate a default memory file. Do NOT infer slices silently.

## Slices Fallback — 6-Rule Heuristic

If the plan has no explicit slice numbering, apply these 6 rules IN ORDER to infer 3–7 slices:

1. **User-visible end-to-end test** — Can a user complete one full action? That is one slice.
2. **Architectural layer crossing** — Must touch UI + API + DB (or full stack). If not, split further.
3. **Database-first vs API-first vs UI-first** — Pick the layer the plan emphasizes.
4. **Risk-ordered delivery** — Highest-risk slice is slice 1.
5. **3–7 slice cap** — If >7, merge adjacent low-risk slices. If <3, split the largest.
6. **Independence** — Each slice deployable/testable without later slices.

When inferred, the memory file header MUST include:
`# Inferred Slices: [N] (via vertical-slicing heuristic)` and `# Note: Slice boundaries inferred by applying 6-rule heuristic. Review and adjust before execution.`

## Generate Two Artifacts (Strict Order)

Produce EXACTLY two fenced code blocks, in this order, with NOTHING between them:

1. **First code block** — the complete memory file content in English.
2. **Second code block** — the complete execution prompt.

After both blocks, you MAY add up to 5 lines in Arabic summarizing how to use them.

## The 10-Stage Loop (overview)

For each slice, execute these 10 stages in order:

1. **Pre-Execution Verification** — Build + tests pass with zero errors and zero warnings.
2. **Deep Understanding** — Internalize requirements, inputs, outputs, behavior.
3. **File Analysis** — Identify and inspect every file the slice touches.
4. **Planning** — Write a step-by-step execution plan.
5. **Execution** — Implement the slice.
6. **Post-Execution Verification** — Build + tests pass again.
7. **Validation Gate** — Pass the slice's validation gate (if any).
8. **Documentation Update** — Mark every checkbox in this slice as [x].
9. **Memory Status Update** — Update the "Current Status" line.
10. **Git Commit (Draft + Human Confirmation)** — Stage changes, draft exact commit message per template, then STOP and request explicit human confirmation before any `git commit` or `git push`; never auto-commit/push.

For detailed per-stage instructions, read `references/10-stage-loop.md`.

## Strict Policies

### Branch Policy
- Work ONLY on the current local branch (`git branch --show-current`).
- NEVER create new branches. NEVER push to any remote on your own.
- At Stage 10: stage changes (`git add`), draft the exact commit message per template, then STOP and request explicit human confirmation before `git commit` or `git push` is executed. No implicit or automatic commit/push under any condition.
- Push, when confirmed by human, targets current LOCAL branch only.

### Stop Conditions
| Condition | Threshold |
|---|---|
| Build failure | 4 consecutive times → STOP and submit status report |
| Same error unfixed | 4 consecutive times → STOP and submit status report |
| Test failure | 4 consecutive times → STOP and submit status report |
| Same file modification failure | 4 consecutive times → STOP and submit status report |

Status report format: slice number, failing action, attempt count, last error, affected files.

For full details, read `references/stop-conditions.md`.

## Anti-Patterns to Avoid

DO NOT:
- Skip any of the 10 stages, even if "unnecessary."
- Combine two stages into one.
- Mark a stage complete before it actually is.
- Downgrade "zero warnings" to "warnings are OK."
- Push to remote even if the user asks.
- Truncate the Current Status section with `...` or "and so on."
- Generate a memory file when the plan is empty/unparseable.

For more, read `references/anti-patterns.md`.

## Critical Reminders

- The memory file MUST be entirely in English. Date in ISO 8601 (`YYYY-MM-DD`).
- Slice count in the memory file MUST equal slice count in the plan.
- All checkboxes unfilled `- [ ]`. The implementing agent fills them.
- The Current Status section MUST contain all N+1 lines.
- Zero placeholders. Zero truncation. Copy-pasteable with one click.

## Composition With Other Skills

- `code-review` skill: invoke after Stage 10 of each slice.
- `test-generation` skill: invoke during Stage 3.
- `security-audit` skill: invoke between Stage 6 and Stage 7.

If another skill's instructions contradict loop-engineering's rules, loop-engineering's rules win.

## Reference Files (read on demand)

- `references/planning-phase.md` — Phase 0 multi-agent planning workflow.
- `references/memory-file-template.md` — full B1-B5 structure.
- `references/10-stage-loop.md` — detailed per-stage procedure.
- `references/execution-prompt.md` — full C1-C10 template.
- `references/anti-patterns.md` — specific failure modes.
- `references/stop-conditions.md` — detailed stop rules with examples.

Begin now by reading the plan file referenced in the user's message and producing the two code blocks.
