---
name: ui-ux
description: Redesign and review GigRadar UI/UX for Artists and Event Organizers with a modern, comfortable, consistent experience.
---

# GigRadar UI/UX

## Goals
Create a polished product experience for two primary roles:
- **Artist:** discover gigs, manage profile/portfolio, apply, track applications.
- **EO:** create events, discover artists, review applicants, manage bookings.

## Review checklist
- Clear hierarchy and scannable content.
- One obvious primary action per screen.
- Consistent spacing, typography, cards, buttons, forms, empty/loading/error states.
- Strong mobile usability: thumb-friendly targets, concise navigation, minimal typing.
- Accessible contrast, labels, focus states, semantic controls, and readable text.
- Avoid decorative UI that competes with task completion.
- Use the design system in `GIGRADAR_APP_SPEC.md` (Lampiran A) as the source of truth before inventing tokens.

## UX flows to inspect
Artist:
login → onboarding → profile → browse gigs → gig detail → apply → application status → notifications.

EO:
login → onboarding → dashboard → create event → publish → discover artists → applicant review → booking/event management.

## Output expectations
When asked to redesign:
1. Identify current UX problems from code.
2. Propose a concrete improved hierarchy/flow.
3. Implement the smallest coherent set of changes.
4. Verify desktop/mobile behavior where applicable.
5. Do not replace working architecture merely for visual preference.
