---
name: gigradar-context
description: Project context and guardrails for GigRadar. Use this skill before making broad architectural, UX, or cross-platform changes.
---

# GigRadar Context

GigRadar is a marketplace/workflow platform connecting **Artists** with **Event Organizers (EO)**.

## Core product goals
- Help Artists discover relevant gigs and present professional profiles.
- Help EOs discover Artists, create/manage events, and handle applications.
- Keep flows simple, trustworthy, mobile-friendly, and consistent across API and .NET MAUI/mobile surfaces.

## Repository landmarks
- `GigRadarApi/` — backend/API.
- `GigRadarMobile/` — mobile client.
- `GigRadarLauncher/` — launcher/supporting project.
- Root documentation includes design system, role system, mobile, GPS/location, and project recap documents.

## Before changing code
1. Inspect the existing implementation and related documentation.
2. Preserve existing business rules unless the task explicitly changes them.
3. Avoid inventing endpoints, models, roles, or permissions.
4. Check both Artist and EO impact for shared UI/domain changes.
5. Prefer small, reviewable changes over broad rewrites.
6. Keep API contracts and client models synchronized.

## Definition of done
- Existing behavior is preserved unless intentionally changed.
- Relevant tests/build checks are run or their absence is reported.
- UI changes are responsive and accessible.
- Security-sensitive data is not logged or exposed.
