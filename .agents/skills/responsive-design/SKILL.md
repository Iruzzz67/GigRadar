---
name: responsive-design
description: Ensure GigRadar UI works comfortably across desktop, tablet, and mobile/.NET MAUI surfaces.
---

# Responsive Design

## Principles
- Mobile-first interaction decisions.
- Touch targets should be comfortable and consistent with platform conventions.
- Avoid horizontal scrolling for primary content.
- Cards and lists should reflow rather than merely shrink.
- Forms should minimize keyboard burden.
- Keep critical actions visible without excessive scrolling.

## Implementation
Before changing layout:
1. Inspect the current UI framework and existing responsive patterns.
2. Reuse established components/tokens.
3. Define behavior for narrow, medium, and wide layouts.
4. Check long names, long event titles, missing images, and localization-like text expansion.

## Validation
Test representative states:
- normal content
- empty list
- long content
- loading
- validation error
- network error
- small viewport
