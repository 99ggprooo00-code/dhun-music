# Reference products: verified feature map and corrections

Reference products are studied for architecture and product ideas. Their implementations are not templates,
and none of them defines what DHUN is allowed to ship. Facts below were re-checked against upstream
repositories and policies on 2026-08-28; if a claim here goes stale, this file is corrected, not quietly
rewritten.

## Status of the references (as of 2026-08-28)

| Reference | Verified current state | What DHUN actually takes from it |
|-----------|------------------------|----------------------------------|
| Metrolist | The Android app is in **maintenance mode** while the project migrates to **Kotlin Multiplatform**; the current line ships playback, background playback, downloads/cache, lyrics, EQ, account sync, local playlists and "listen together". | Product feature vocabulary, player-state separation, how downloads/cache and listening rooms are modeled in a UI. Nothing extraction-related. |
| VIVI | Active Material 3 Android client: dynamic colors from artwork, animated visuals, karaoke-style lyrics, EQ, offline downloads, Android Auto, local/privacy-first storage. Its build is **not** a simple Kotlin project — the setup involves Java 21, Go, protobuf and NDK/native components. | Evidence that a local-first, privacy-posed player can be expressive; dynamic theming and karaoke lyrics interaction design. Its complexity warns us not to underestimate "simple-looking" features. |
| WAVE | Claims about a specific InnerTube/proxy/FLAC architecture in circulation **could not be independently verified**; treated as unconfirmed. | No architectural assumptions at all. |
| Nagi (our base) | Active C#/.NET WinUI local player; we fork it under GPL and keep upstream provenance in `UPSTREAM.md`. | The local playback engine being hardened in Milestones 1–6. |

**Correction to earlier planning documents:** earlier roadmap drafts described Metrolist as an actively
growing YouTube client we could mirror. That is wrong: its current upstream direction is a KMP rewrite, its
Android app is bug-fix only, and its headline features (downloads, background playback of YouTube audio) are
precisely the ones DHUN cannot ship with provider content (see `docs/ADR/0004-lawful-provider-first-boundary.md`).
Study its screens and state model; do not plan DHUN's roadmap as a checklist of its provider features.

## YouTube policy constraints (verified 2026-08-28)

From the YouTube API Services developer policies, section on handling data/content, and the YouTube Terms:

- API clients must not download, import, backup, cache or store copies of YouTube audiovisual content
  without prior written approval.
- Content may not be made available for offline playback.
- Audio may not be separated from video; background playback of content is not enabled.
- Terms restrict downloading/modifying content and circumventing technical protections.

Sources: <https://developers.google.com/youtube/terms/developer-policies> and
<https://www.youtube.com/static?template=terms>.

Consequence for DHUN: **there is no compliant "offline YouTube" or "YouTube as background audio" feature
to build.** Any online integration DHUN ships later is metadata + visible official playback only, or comes
from a provider whose license explicitly permits what we build.

## DHUN feature map (P0 → P3)

P0 — must work, fully local, before any online code:

- library scan/watch with stable identity (ADR-0003)
- play/pause/seek/next/previous, volume, queue (add/remove/reorder)
- playlists, favorites, history
- embedded + LRC lyrics (sync located via `ActiveLyricLineLocator`)
- metadata/artwork robustness (missing art, bad tags, unicode paths)
- keyboard-first navigation, SMTC/media keys
- crash diagnostics with redaction, local-only unless user exports

P1 — very important, still local:

- shuffle/repeat semantics including artist-aware smart shuffle (`QueueShuffler`)
- gapless, crossfade, ReplayGain, EQ presets, sleep timer
- smart playlists (rules engine), search across songs/albums/artists/genres/folders
- listening statistics (local), resume-session, multi-device file watching

P2 — premium quality:

- dynamic artwork theme, mini-player, visualizer, karaoke-style word-synced lyrics
- romanization/translation UX, collaborative playlist export, QR/deep-link sharing of local playlists
  (metadata only)

P3 — expansion (each requires its own ADR first):

- lawful online providers behind `IMusicSource` (official metadata APIs, licensed catalogs)
- optional sync backend for playlists/favorites (user data only, never provider media)
- ARM64, packaging/store options

## Definition of done (applies to every feature above)

A feature is done only when all are true:

1. It works with a folder of permitted/local audio files and **no network**.
2. It survives app restart (state persisted and rebuilt correctly).
3. It has loading / empty / error / retry states in the UI.
4. Bad input (missing file, zero duration, missing tags, huge library) degrades safely.
5. It has at least one Core-level automated test, and CI is green.
6. It leaks no credentials, tokens or full user paths into logs, crash reports or exports.
7. Accessibility: keyboard reachable, focus states, screen-reader labels.
