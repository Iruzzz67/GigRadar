---
name: testing
description: Build and validate reliable tests for GigRadar API and mobile/client behavior.
---

# GigRadar Testing

## Test layers
- Unit tests for business rules and pure logic.
- Integration tests for API endpoints, auth, authorization, persistence, and serialization.
- UI/client tests for critical Artist and EO flows where the existing stack supports them.

## High-value scenarios
Authentication:
- valid login
- invalid credentials
- expired/invalid token
- refresh behavior
- logout

Authorization:
- Artist cannot perform EO-only operations.
- EO cannot perform Artist-only operations.
- Users cannot access another user's protected resources without permission.

Artist:
- browse/search gigs
- view details
- submit application
- duplicate application handling
- status changes

EO:
- create/update/publish event
- applicant review
- unauthorized event access
- event lifecycle validation

## Test quality
- Prefer deterministic tests.
- Avoid brittle timing and UI selectors.
- Test failure paths, not only happy paths.
- Keep tests aligned with actual domain states and contracts.
