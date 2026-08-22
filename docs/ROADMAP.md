# DHUN Native roadmap

The native foundation is published as a `v1.0.0` prerelease. Milestones continue to describe engineering progress, and promotion to a stable release depends on real-device regression testing.

## Milestone 0 — Native foundation

- [x] Fork Nagi with upstream relationship intact
- [x] Preserve Electron prototype in a separate archive repository
- [x] Verify clean upstream test/build behavior
- [x] Identify proprietary/build-key dependency blocker
- [x] Replace ImageSharp 4 with MIT-licensed SkiaSharp
- [x] Verify core tests and x64 WinUI compile on clean Windows CI
- [x] Apply visible DHUN branding and Windows package identity
- [x] Replace application artwork
- [x] Remove upstream Azure, signing, benchmark, translation and release infrastructure
- [x] Preserve GPL provenance and dependency notices
- [x] Rename internal projects and namespaces from `Nagi.*` to `Dhun.*`
- [x] Generate a sideloadable unsigned MSIX for foundation testing
- [x] Install the unsigned MSIX in Windows CI and verify a 20-second native startup window

## Milestone 1 — Local Windows player regression

- [ ] Verify library scan on a real Windows device
- [ ] Verify MP3, FLAC, AAC, M4A, OGG, Opus, WAV, WMA, AIFF and APE
- [ ] Verify queue, shuffle and repeat
- [ ] Verify playlists and smart playlists
- [ ] Verify embedded and LRC lyrics
- [ ] Verify equalizer and ReplayGain
- [ ] Verify mini-player and always-on-top
- [ ] Verify system tray and SMTC/media keys
- [ ] Verify file associations and startup task
- [x] Remove or hide integrations that depend on upstream credentials
- [ ] Add DHUN-owned crash diagnostics and privacy controls

## Milestone 2 — DHUN Fluent redesign

- [ ] Home
- [ ] Explore
- [ ] Unified Search
- [ ] Library sections: songs, albums, artists, genres and folders
- [ ] Playlist experience
- [ ] Persistent bottom player
- [ ] Full Now Playing page
- [ ] Lyrics and queue panels
- [ ] Compact and mini-player modes
- [ ] Dynamic artwork theme
- [ ] Keyboard-first navigation

## Milestone 3 — Online source contracts

- [ ] Define source-neutral `Song`, `Album`, `Artist` and `Playlist` models
- [ ] Extend database entities with source and source ID
- [ ] Define catalog/search/home/lyrics/playback source interfaces
- [ ] Implement official YouTube catalog provider
- [ ] Implement visible official YouTube playback host
- [ ] Add artist, album and playlist pages
- [ ] Add charts, moods and genres
- [ ] Add recommendations and radio
- [ ] Keep online provider isolated from local/LibVLC playback

## Milestone 4 — Unified music

- [ ] Mixed local/online queue
- [ ] Mixed local/online playlists
- [ ] Unified favorites
- [ ] Unified history and statistics
- [ ] Unified global search
- [ ] Availability and embeddability state
- [ ] Source-aware offline/cache behavior

## Milestone 5 — Advanced local audio

- [ ] Gapless regression suite
- [ ] Crossfade controls
- [ ] ReplayGain/normalization UX
- [ ] 10-band EQ and preamp
- [ ] Pitch and tempo
- [ ] Silence skipping
- [ ] Output-device selection
- [ ] Optional WASAPI/exclusive-mode feasibility review

## Milestone 6 — Distribution

- [ ] DHUN-owned package identity and upgrade path
- [ ] Privacy policy and Google/API disclosures
- [ ] Dependency license report
- [ ] Reproducible x64 and ARM64 builds
- [ ] Code signing or open-source signing approval
- [ ] MSIX installer
- [ ] Update feed owned by DHUN
- [ ] First stable release only after all required milestone gates pass
