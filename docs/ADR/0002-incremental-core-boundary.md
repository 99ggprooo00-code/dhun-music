# ADR-0002: Incremental Core Boundary

## Status
Accepted

## Context

The current `Dhun.Core` project is not a pure domain assembly. It currently contains domain models alongside EF Core/SQLite persistence, metadata helpers, HTTP infrastructure, and application services. A clean architecture target is still valuable, but moving all of these concerns in one migration would create a large regression surface for the local player.

## Decision

Separate the architecture incrementally instead of performing a big-bang project split.

The target boundaries are:

```text
Presentation (WinUI)
        |
Application/use cases
        |
Domain + source contracts
        |
Infrastructure (database, filesystem, metadata, playback, HTTP)
```

During the transition:

1. New source-neutral domain contracts must not introduce new infrastructure dependencies.
2. New local-library behavior should be exposed through interfaces where a future online source could reasonably need the same capability.
3. Existing EF Core/metadata/HTTP code may remain in `Dhun.Core` temporarily when moving it would be unnecessarily disruptive.
4. Each extraction must be accompanied by tests and a CI build before the next extraction.
5. Do not change database schema solely to satisfy the architectural ideal. Source-aware persistence is deferred until the mapping model is proven by local tests and a real provider requirement.

## Consequences

### Positive

- Reduces architectural risk without stopping feature development.
- Keeps the local player stable while boundaries are improved.
- Makes future online-provider work less invasive.
- Gives each refactor a small, reviewable failure surface.

### Negative

- `Dhun.Core` remains temporarily broader than its final responsibility.
- The architecture will be transitional for several milestones.
- Some dependencies cannot be removed until their consumers are extracted.

## Rejected alternative

A complete `Dhun.Core` -> `Dhun.Domain`/`Dhun.Application`/`Dhun.Infrastructure` rewrite was rejected for now because the current project already contains a mature local-player implementation and a substantial EF Core migration history. The rewrite would create unnecessary risk before the local library behavior has been hardened.

## Exit criteria

This ADR can be retired when:

- domain/source contracts compile without infrastructure packages;
- application use cases no longer require EF Core directly;
- persistence, filesystem/metadata, playback, and HTTP implementations live behind infrastructure boundaries;
- tests can exercise domain behavior without WinUI or SQLite;
- the existing database migrations and local-player regression suite remain green.
