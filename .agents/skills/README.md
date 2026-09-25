# GigRadar Agent Skills

Project-specific Skills for Freebuff/agent workflows.

## Structure

```text
.agents/
└── skills/
    ├── gigradar-context/
    ├── ui-ux/
    ├── artist-experience/
    ├── eo-experience/
    ├── responsive-design/
    ├── accessibility/
    ├── security/
    ├── testing/
    └── code-review/
```

## Recommended usage

Start broad:

```text
/skill:gigradar-context
```

Then combine with the task:

```text
/skill:ui-ux
Redesign the Artist dashboard without breaking existing API contracts.
```

```text
/skill:eo-experience
Improve the EO event creation and applicant management flow.
```

```text
/skill:security
Audit authentication, token handling, and role-based authorization.
```

```text
/skill:code-review
Review my latest changes for correctness, security, UX, and tests.
```

## Working rule

Skills are guidance, not permission to invent architecture. Always inspect the existing GigRadar implementation and documentation before making changes.
