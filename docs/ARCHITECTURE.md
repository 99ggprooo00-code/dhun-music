# DHUN architecture

## Product rule: local first

DHUN is a Windows-native local music player with optional online sources. Local playback, library integrity, privacy, startup reliability, and Windows integration must remain fully usable when every online provider is unavailable.

Online music is an extension of the player—not its foundation.

## Current architecture and target

DHUN already has a substantial native implementation in `Dhun.Core`. That project currently combines domain models, EF Core/SQLite persistence, metadata helpers, HTTP utilities, and application services. It is therefore **not yet a pure domain assembly**.

The target is a clean boundary, but the project will reach it incrementally rather than through a risky rewrite. See ADR-0002 for the transition strategy.

```text
Current

Dhun.WinUI
    |
    v
Dhun.Core
    +-- domain/source contracts
    +-- persistence
    +-- metadata/filesystem services
    +-- application services
    +-- HTTP utilities

Target

Dhun.WinUI
    |
    v
Application / use cases
    |
    v
Domain + source contracts
    |
    +------ Infrastructure
             +-- EF Core/SQLite
             +-- filesystem/metadata
             +-- LibVLCSharp
             +-- HTTP/provider clients
```

## Dependency rules

1. `Dhun.Core` must not depend on WinUI.
2. New source-neutral domain contracts must not introduce new infrastructure dependencies.
3. Local entities may expose local paths only through local-source types.
4. Online provider DTOs must not leak into queue, playlist, history, or UI state.
5. Application code should map source DTOs into source-neutral domain models.
6. WinUI depends on abstractions, not provider implementations.
7. Local playback must never depend on an online API, browser runtime, OAuth token, or remote service.
8. Online failures must degrade to a visible unavailable state without damaging the local queue or database.
9. Existing infrastructure in `Dhun.Core` may remain temporarily while it is being extracted behind tested boundaries.

## Source-neutral identity

Every online or local entity has a stable source identity:

```text
SourceIdentity
├── Kind       Local | YouTube | FutureProvider
└── ExternalId provider-specific stable ID
```

The initial source contracts deliberately avoid changing existing EF entities. Database migration will follow after the contracts and mapping tests stabilize.

## Track model direction

```text
MusicTrack
├── InternalId
├── SourceIdentity
├── Title
├── Artists
├── Album
├── Duration
├── Artwork
├── Availability
├── Explicit
├── TrackNumber / DiscNumber
├── Local playback reference (local source only)
└── Online playback reference (online source only)
```

Queue, playlists, favorites and history operate on internal IDs plus source identity. They must not branch on YouTube-specific or path-specific fields.

## Source interfaces

- `IMusicSource` — identity and capability declaration
- `ISearchSource` — source search returning normalized results
- `ICatalogSource` — artist, album and playlist retrieval
- `ILyricsSource` — plain/synchronized lyric retrieval
- `IPlaybackSource` — source-specific preparation behind a common playback contract

Capabilities are explicit so the UI does not display actions a source cannot perform.

## Playback boundary

### Local

LibVLCSharp remains the local engine. It owns broad format support, ReplayGain, equalizer, gapless behavior, local queue preparation, device integration and playback state.

### Online

The first online implementation must use isolated catalog APIs and visible policy-compliant playback. DHUN will not implement hidden playback, media extraction, ad removal, downloading, or access-control bypassing.

Local and online playback report a common state to the application layer:

```text
Idle · Loading · Ready · Playing · Paused · Buffering · Ended · Error
```

## Persistence

EF Core + SQLite remains the authoritative local library store. The source-aware migration is intentionally deferred until mapping tests prove the model.

When it is justified, the migration sequence is:

1. Introduce source-neutral contracts.
2. Add source kind and source ID columns with local defaults.
3. Backfill all existing local records.
4. Add uniqueness constraints by source identity.
5. Extend queue and playlist records.
6. Add online cache tables separately from user library tables.

Online cache eviction must never delete user-created playlists, favorites, or history.

## Security and privacy

- No credentials are committed to source control.
- Tokens use Windows credential storage.
- Online account connection is optional.
- Logs remain local unless the user explicitly exports them.
- Diagnostics must redact user paths and secrets before sharing.
- Provider code receives the minimum required scopes.
- Build and release identities belong to DHUN, not upstream Nagi infrastructure.

## Testing gates

Every architectural increment requires:

- Core unit tests
- Migration tests when schema changes
- Queue/playlist persistence tests
- Provider contract tests with mocked responses
- x64 native publish
- EXE installer build
- silent install/uninstall test
- packaged 20-second startup smoke test

Real hardware remains required for audio output, Bluetooth, media keys, tray, sleep/wake, removable drive and multi-device verification.

## Branch policy

- `main` contains reviewed, CI-passing increments.
- Architecture and schema work is developed on focused branches.
- Changes enter through pull requests.
- The current architecture branch is intentionally kept focused until the local-player gates are proven.
