---
name: code-review
description: Perform a structured production-quality code review of GigRadar changes.
---

# GigRadar Code Review

Review in this order:

## 1. Correctness
- Does the change satisfy the requested behavior?
- Are edge cases handled?
- Are API/client contracts consistent?

## 2. Security
- Authentication and authorization correct?
- Any IDOR/BOLA?
- Secrets/tokens exposed?
- Unsafe input, file, URL, or logging behavior?

## 3. Data integrity
- Validation and state transitions correct?
- Null/empty handling safe?
- Concurrency or duplicate-submission issues?

## 4. UX
- Artist and EO role flows remain clear.
- Loading, empty, error, and success states are covered.
- Responsive/accessibility regressions avoided.

## 5. Maintainability
- Reuse existing abstractions.
- Avoid unnecessary duplication.
- Keep naming and architecture consistent.
- Avoid overengineering.

## 6. Testing
- Existing tests still pass.
- New behavior has appropriate coverage.

## Review output
Report findings by severity:
- **P0 Critical** — security/data-loss/outage risk.
- **P1 High** — major correctness/security issue.
- **P2 Medium** — meaningful defect or maintainability issue.
- **P3 Low** — minor improvement.

For each finding include:
- file/path
- exact problem
- impact
- concrete fix

If no findings exist, say so and list what was verified.
