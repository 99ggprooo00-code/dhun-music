# ADR-0001: Local-first, source-neutral architecture

## Status
Accepted

## Context

DHUN's immediate product goal is a reliable native Windows local music player. The longer-term vision includes online music, but online integration is technically and operationally more volatile than local playback.

The existing native foundation already contains local library, metadata, SQLite/EF Core, LibVLCSharp playback, playlists, lyrics, ReplayGain, equalizer, tray and Windows media-control functionality.

## Decision

1. Local playback is the primary product milestone.
2. Online providers are optional source adapters.
3. Core models and application workflows remain source-neutral.
4. Provider IDs and DTOs remain inside source boundaries.
5. Local database migration to source-aware identity happens only after contracts and mapping tests stabilize.
6. No provider implementation is introduced merely to satisfy the architecture; interfaces must remain useful and minimal.

## Consequences

### Positive

- Local functionality can reach a stable release independently.
- YouTube/provider complexity does not infect queue, playlist, history, or UI code.
- Future providers can be evaluated independently.
- Offline privacy remains a product feature.

### Negative

- Some abstractions exist before the second provider exists.
- A later database migration will be required for fully unified local/online persistence.
- Source capability handling adds some application complexity.

## Rejected alternatives

### Online-first
Rejected because provider constraints would dictate the architecture before DHUN's local experience is mature.

### Separate applications
Rejected because it would duplicate the player, queue, playlists and Windows integration.

### Provider-specific models throughout the app
Rejected because it would make a future provider replacement or addition expensive.
