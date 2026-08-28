# Architecture decision records

This directory holds DHUN's ADRs. One decision per file, immutable once accepted; supersede instead of edit.

## Index

| ID | Title | Status |
|----|-------|--------|
| [0001](0001-local-first-source-architecture.md) | Local-first, source-neutral architecture | Accepted |
| [0002](0002-incremental-core-boundary.md) | Incremental Core boundary (transitional `Dhun.Core`) | Accepted |
| [0003](0003-local-track-identity.md) | Stable local track identity (content fingerprint) | Accepted |
| [0004](0004-lawful-provider-first-boundary.md) | Provider-first architecture and lawful audio boundary | Accepted |

## Template

```markdown
# ADR-NNNN: Title

- **Status:** Proposed | Accepted | Superseded by ADR-XXXX
- **Date:** YYYY-MM-DD
- **Scope:** subsystem(s) affected

## Context
What pressure, risk or disagreement makes a decision necessary now.

## Decision
The decision, in imperative sentences. Diagrams encouraged. Rules must be checkable.

## Consequences
Positive and negative, honestly stated.

## Rejected alternatives
Each alternative and the reason it was rejected.
```

## Rules

1. Any change to dependency direction, persistence model, provider policy, release process, or anything a
   future contributor would plausibly "fix" differently requires an ADR.
2. An ADR that describes a rule enforced by code should link the tests that enforce it.
3. Numbering is append-only.
