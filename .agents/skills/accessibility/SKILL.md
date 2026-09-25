---
name: accessibility
description: Audit and improve GigRadar accessibility without changing product behavior unnecessarily.
---

# Accessibility

## Checklist
- Every interactive control has an accessible name.
- Form fields have labels and useful validation text.
- Color is never the sole carrier of status.
- Text remains readable at larger sizes.
- Focus/selection states are visible.
- Navigation order is logical.
- Images have meaningful alternatives when informative; decorative images are ignored by assistive tech where supported.
- Error messages identify the affected field and explain how to fix it.
- Touch targets are large enough and not crowded.
- Avoid animations that cause discomfort; respect platform reduced-motion settings where available.

## Role safety
Accessibility improvements must not accidentally bypass role-based visibility or authorization.
