# ADR-0004: Provider-first architecture and lawful audio boundary

- **Status:** Accepted
- **Date:** 2026-08-28
- **Scope:** Product architecture, online-source policy, feature planning

## Context

The reference projects for DHUN's product direction (Metrolist, VIVI, similar clients) derive much of their
capability from unofficial YouTube extraction: InnerTube endpoints, signature/cipher deciphering, stream
separation, proxying, downloads and background playback. It would be technically possible to clone that
pipeline into DHUN first. There are three independent reasons not to:

1. **Platform policy.** The YouTube API Services developer policies state that API clients must not
   download, import, backup, cache or store copies of YouTube audiovisual content without prior written
   approval, must not make content available for offline playback, and must not separate the audio track or
   enable background playback of content. YouTube's own Terms of Service similarly restrict downloading,
   modifying, and circumventing technical protections. A product built on those assumptions can be forced
   offline at any time.
2. **Fragility.** Unofficial extraction endpoints and player-signature algorithms change without notice.
   When they break, every feature built on top of them breaks simultaneously, including the parts of the
   application that have nothing to do with the provider.
3. **Product identity.** DHUN's stated product is "the best native Windows local music player, with
   optional online music", not "another YouTube client". Architecture should encode that, not fight it.

## Decision

Build the entire product architecture around a provider abstraction first, with legal/local audio as the
reference workload. Provider adapters are added one at a time, after the local player gate passes.

```text
Local files ··┐
Licensed /····┤
own audio ····┤
Public-domain├──► provider interface (Dhun.Core.Sources)
(archive,     │            │
 Jamendo, ...)─┘            ▼
Official API          Music domain
(metadata only)   ┌────────┼────────┐
                  ▼        ▼        ▼
               Search   Library  Player
                  └────────┼────────┘
                           ▼
                     App UI layer
```

1. The application layer never contains provider-specific parsing, endpoint knowledge, or
   provider-conditional behavior. Provider DTOs stop inside their adapter (already enforced by the
   source-neutral contracts and the `MusicSourceRegistry`).
2. **Local and user-owned audio is the reference workload.** All P0 features must be fully implementable
   against `LocalMusicSource` alone, offline.
3. Online providers are lawful and replaceable: official/authorized APIs, permissively licensed catalogs,
   or public-domain material. A provider must be removable without touching the player.
4. When an online provider is added, it must satisfy all of: visible playback (no hidden windows, no
   headless media surfaces), no download/caching/storage of provider media, no audio/video separation, no
   background playback of provider streams, no ad or access-control circumvention, failures degrade to a
   visible unavailable state, and the local queue/library is never modified by provider failure.
5. Metadata-only online usage (catalog matching, art, lyrics lookup) is a separate concern from playback
   and is governed by each provider's own terms.

## Prohibited patterns

These must not appear in DHUN, on any branch, regardless of how convenient a fix seems:

- Stream URL extraction relying on deciphering player signatures or anti-bot protections.
- Extracting, separating, or re-wrapping the audio track of provider video.
- Downloading or persisting provider media for "offline mode". DHUN's offline scope is user-owned files,
  licensed content, and legitimately cached metadata (artwork, lyrics, catalog data) — not provider audio.
- Background/hidden-window playback of provider content.
- Ad blocking, rate-limit evasion, account sharing, or any access-control bypass.
- Dependence on unofficial endpoints that can change without notice as a hard requirement for core UX.

## Consequences

### Positive

- A provider outage or policy change can never take the product down; it only disables one optional tile.
- No legal posture depends on interpreting a third party's scraping tolerance.
- The local player matures at its own pace and ships behind a real quality gate.
- Each future provider is independently testable and independently removable.

### Negative

- Features some users expect from unofficial clients (offline YouTube, background YouTube, downloads) will
  not exist in DHUN. This is intentional and should be communicated, not worked around.
- The catalog of lawful online sources is smaller and less convenient than unofficial YouTube.

## Rejected alternatives

1. **Clone the unofficial extraction pipeline first, add local playback later.** Rejected: this is the
   single change most likely to be reverted by an upstream policy or endpoint change, and it inverts the
   product priority.
2. **Ship two binaries (local player + "online" fork).** Rejected: duplicated player, queue, library and
   Windows integration; the split does not reduce legal or maintenance risk.
3. **Provider-agnostic façade over an extraction core.** Rejected: the façade would sit on top of the very
   dependency this ADR removes.

## Verification

- Architecture tests assert `Dhun.Core` does not reference WinUI and that source adapters stay behind contracts.
- Every feature added before the local-player gate must be demonstrable with a folder of MP3/FLAC files and
  no network.
