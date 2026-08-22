# DHUN Native roadmap

DHUN is being developed as a **native Windows local music player first**. Online music remains a future extension behind source-neutral contracts.

The native foundation already provides .NET 10, WinUI 3, MVVM, EF Core/SQLite, LibVLCSharp, ATL metadata extraction, local library scanning, queue, playlists, smart playlists, history, synchronized lyrics, EQ, ReplayGain, mini-player, tray, media controls, file associations and startup integration. The current focus is turning that foundation into a reliable daily-driver local player.

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
- [ ] Review `LangVersion=preview` and remove it unless required
- [x] Add architecture tests where practical
- [ ] Decide source-aware database migration after mapping tests stabilize

## Milestone 1 — Local library reliability

- [ ] Real-device library scan regression
- [ ] Incremental scanning and cancellation
- [ ] File move/rename detection
- [ ] Missing/corrupt-file handling
- [ ] Track identity independent of display path
- [ ] Verify MP3, FLAC, AAC, M4A, OGG, Opus, WAV, WMA, AIFF and APE
- [ ] Metadata normalization regression corpus
- [ ] Embedded artwork and artwork-cache regression
- [ ] 10k-track performance test
- [ ] 50k-track performance test
- [ ] DHUN-owned crash diagnostics and privacy controls

## Milestone 2 — Playback and queue reliability

- [ ] Deterministic playback state transitions
- [ ] Queue add/remove/reorder/clear
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

- [ ] Review current official YouTube capabilities and terms
- [ ] Determine supported catalog/search/authentication flows
- [ ] Determine supported visible playback model
- [ ] Prototype only within supported/provider-compliant boundaries
- [ ] Keep provider implementation isolated from local/LibVLC playback

No hidden playback, stream extraction, ad removal, downloading, or access-control bypassing.

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
