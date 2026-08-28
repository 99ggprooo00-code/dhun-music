# DHUN Native roadmap

DHUN is being developed as a **native Windows local music player first**. Online music remains a future extension behind source-neutral contracts.

The native foundation already provides .NET 10, WinUI 3, MVVM, EF Core/SQLite, LibVLCSharp, ATL metadata extraction, local library scanning, queue, playlists, smart playlists, history, synchronized lyrics, EQ, ReplayGain, mini-player, tray, media controls, file associations and startup integration. The current focus is turning that foundation into a reliable daily-driver local player.

The order of work is fixed by [`docs/ADR/0004-lawful-provider-first-boundary.md`](ADR/0004-lawful-provider-first-boundary.md): the provider abstraction and local/legal audio come first, and each online provider is added independently, later, and only if lawful. **The first milestone is not a YouTube pipeline.** Reference products (Metrolist, VIVI) are studied for architecture and product ideas only; their verified current status and the feature map DHUN derives from them live in [`docs/UPSTREAM-FEATURE-MAP.md`](UPSTREAM-FEATURE-MAP.md).

## Feature priority contract

P0 — must work, fully local, before any online code ships (Milestones 1–3):

- library scan/watch with stable identity; play/pause/seek/next/previous; queue add/remove/reorder/clear
- playlists, favorites, history; embedded + LRC lyrics; keyboard-first navigation; SMTC/media keys
- crash diagnostics with redaction (local-only unless the user exports)

P1 — very important, still local (Milestones 2–5):

- shuffle/repeat semantics including artist-aware smart shuffle (`QueueShuffler` in Core)
- gapless, crossfade, ReplayGain, EQ, sleep timer; smart playlists; unified local search
- listening statistics; resume-after-restart; folder watching

P2 — premium quality (Milestone 5):

- dynamic artwork theme, visualizer, karaoke-style word-synced lyrics, romanization UX, mini-player polish

P3 — expansion, each item requires its own ADR first (Milestones 7–10):

- lawful online providers behind the source contracts; optional user-data sync; ARM64; packaging

Every P0/P1 feature is only "done" when it passes the definition-of-done checklist in
[`docs/UPSTREAM-FEATURE-MAP.md`](UPSTREAM-FEATURE-MAP.md): works offline with local files, survives restart,
handles bad input, has a Core test, and leaks no secrets or user paths into logs.

## Milestone 0 — Architecture stabilization

- [x] Fork Nagi with upstream relationship intact
- [x] Preserve Electron prototype separately
- [x] Preserve GPL provenance and dependency notices
- [x] Native DHUN branding/package identity
- [x] Source identity primitives
- [x] Source catalog/search models
- [x] Minimal catalog, search, lyrics and playback source contracts
- [x] Document local-first architecture
- [x] Record local-first architecture decision
- [x] Audit Core dependencies for UI/provider leakage and record the transitional boundary strategy
- [x] Review `LangVersion=preview` and remove it unless required (landed in #10: `latest`, 0 warnings)
- [x] Add architecture tests where practical
- [ ] Decide source-aware database migration after mapping tests stabilize

## Milestone 1 — Local library reliability

- [ ] Real-device library scan regression
- [ ] Incremental scanning and cancellation
- [x] Move/rename + change detection core (`LocalFileSystemEventNormalizer`, deterministic `LocalTrackReconciliation.Plan` via #14; watcher + persistence wiring still pending)
- [ ] Wire reconciliation planner into the library scanner and DB writes
- [ ] Missing/corrupt-file handling
- [ ] Track identity independent of display path
- [ ] Verify MP3, FLAC, AAC, M4A, OGG, Opus, WAV, WMA, AIFF and APE
- [ ] Metadata normalization regression corpus
- [ ] Embedded artwork and artwork-cache regression
- [ ] 10k-track performance test
- [ ] 50k-track performance test
- [x] DHUN-owned crash diagnostics and privacy controls (redaction + DHUN report URL landed via #10; no telemetry, local-only export)

## Milestone 2 — Playback and queue reliability

- [ ] Deterministic playback state transitions
- [ ] Queue add/remove/reorder/clear
- [x] Pure, testable shuffle ordering core (`QueueShuffler`, deterministic seeds, smart no-adjacent-artist mode)
- [ ] Wire `QueueShuffler` into `MusicPlaybackService.GenerateShuffledQueue()` behind a setting
- [ ] Shuffle and repeat semantics
- [ ] Resume playback
- [ ] Playback error recovery
- [ ] Audio-device change handling
- [ ] Sleep/wake handling
- [ ] Long-playback regression suite
- [ ] Queue persistence tests
- [ ] Playlist persistence tests

## Milestone 3 — Daily-driver library experience

- [ ] Unified local search
- [ ] Songs, albums, artists, genres and folders
- [ ] Playlist UX
- [ ] Favorites
- [ ] History and recently played
- [ ] Recently added
- [ ] Persistent bottom player
- [ ] Full Now Playing page
- [ ] Lyrics and queue panels
- [ ] Keyboard-first navigation
- [ ] Compact/mini-player modes

## Milestone 4 — Advanced local audio

- [ ] Embedded and LRC lyrics regression
- [ ] ReplayGain UX
- [ ] Equalizer UX
- [ ] Gapless regression suite
- [ ] Crossfade
- [ ] Smart playlists
- [ ] Folder watching
- [ ] Drag/drop import and queueing
- [ ] Output-device selection
- [ ] Optional WASAPI/exclusive-mode feasibility review

## Milestone 5 — Native Windows polish

- [ ] Real-device SMTC/media-key validation
- [ ] Notifications
- [ ] System tray validation
- [ ] File associations
- [ ] Startup task validation
- [ ] Dynamic artwork theme
- [ ] Memory/performance profiling
- [ ] Accessibility and keyboard navigation review

## Milestone 6 — Hardening and local 1.0 gate

- [ ] Clean x64 build
- [ ] Packaged install/uninstall test
- [ ] 20-second startup smoke test
- [ ] Real-device regression pass
- [ ] Corrupt media test corpus
- [ ] Unicode/path-length test corpus
- [ ] Bluetooth/device switching test
- [ ] Sleep/wake test
- [ ] Privacy review
- [ ] Dependency/license review
- [ ] Release documentation

**Gate:** DHUN is not promoted as a stable local-player release until the above regression gates pass.

## Milestone 7 — Online source architecture

- [x] Define source-neutral Song/Album/Artist/Playlist models
- [x] Define source identity and capabilities
- [x] Define catalog/search/lyrics/playback interfaces
- [ ] Add source-aware persistence after migration design is validated
- [x] Add provider registry/availability handling
- [ ] Add source contract test suite

## Milestone 8 — Online provider research

Findings are recorded in [`docs/UPSTREAM-FEATURE-MAP.md`](UPSTREAM-FEATURE-MAP.md). Verified status changes the research scope:

- [x] Review current reference-project status — Metrolist's Android app is in maintenance mode while the project migrates to Kotlin Multiplatform; it is a feature-vocabulary reference, not an implementation template.
- [x] Review current official YouTube capabilities and terms — the YouTube API Services policies prohibit downloading/caching/storing audiovisual content, offline playback, audio separation and background playback of content; the Terms restrict downloading, modifying and circumventing protections.
- [ ] **Permanently out of scope:** unofficial stream/signature extraction, YouTube downloads/"offline mode", background or hidden-window playback of provider media, ad or access-control circumvention (see ADR-0004).
- [ ] Determine supported catalog/search/authentication flows for each candidate provider individually
- [ ] Determine supported visible playback model (embed/official surfaces only)
- [ ] Prototype only within supported/provider-compliant boundaries
- [ ] Keep provider implementation isolated from local/LibVLC playback
- [ ] Offline scope stays "user-owned and licensed content only": any future download manager manages local files, licensed catalogs and public-domain sources, never provider media

## Milestone 9 — Unified music

- [ ] Mixed local/online queue
- [ ] Mixed local/online playlists
- [ ] Unified favorites
- [ ] Unified history/statistics
- [ ] Unified global search
- [ ] Availability/restriction state
- [ ] Source-aware cache behavior

## Milestone 10 — Distribution

- [ ] DHUN-owned upgrade path
- [ ] Privacy/API disclosures
- [ ] Dependency license report
- [ ] Reproducible x64 and ARM64 builds where supported
- [ ] Code signing or open-source signing path
- [ ] MSIX installer
- [ ] DHUN-owned update feed
- [ ] Stable release only after required gates pass
