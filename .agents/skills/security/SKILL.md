---
name: security
description: Security review for GigRadar authentication, authorization, credentials, tokens, API boundaries, and sensitive data.
---

# GigRadar Security

## Priorities
- Authentication establishes identity; authorization enforces role/resource access.
- Never trust role/permission values supplied only by the client.
- Validate authorization server-side for every protected operation.
- Do not hardcode secrets, API keys, passwords, signing keys, or tokens.
- Never commit `.env`/secret files.
- Do not log access tokens, refresh tokens, passwords, authorization headers, or sensitive personal data.
- Store tokens using platform-appropriate secure storage on clients; inspect existing implementation before changing it.
- Use HTTPS/TLS in production.
- Validate user-controlled input at API boundaries.
- Protect file uploads and URLs against path traversal, unsafe content, and unauthorized access.
- Avoid leaking whether sensitive accounts/resources exist when the API should not reveal that information.

## Review procedure
1. Trace login/token issuance.
2. Trace token attachment to requests.
3. Trace refresh/logout/revocation behavior.
4. Trace role checks on API endpoints.
5. Check object-level authorization (IDOR/BOLA).
6. Check sensitive data exposure in DTOs, logs, errors, and client storage.
7. Recommend fixes that fit the existing architecture.

Do not weaken authentication or authorization to make a feature easier to implement.
