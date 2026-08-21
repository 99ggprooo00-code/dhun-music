# Native implementation map

## Nagi foundation

### Keep initially

- `src/Nagi.Core/Data` — EF Core/SQLite library database and migrations
- local library scanner and metadata pipeline
- LibVLCSharp playback service and queue
- playlists and smart playlists
- lyrics parsing and synchronized lyrics
- equalizer and ReplayGain
- mini-player, tray, SMTC/media controls and file associations
- logging, crash recovery and updater abstractions
- tests under `tests/Nagi.Core.Tests`

### Modify in tested stages

- visible product branding and assets
- internal namespaces/project names (`Nagi.*` → `Dhun.*`)
- application navigation and Fluent UI information architecture
- unified `Song`/source model for local and online entries
- queue and playlist entities to support multiple sources
- online source interfaces and official YouTube player host
- privacy disclosures and update endpoints
- release/signing workflow owned by DHUN

### Remove or disable

- upstream Azure Functions deployment
- upstream private API-key service configuration
- upstream SignPath/release identities
- upstream Crowdin and sponsor media
- Discord/Last.fm/ListenBrainz integrations until reviewed and explicitly re-enabled
- any feature that requires upstream private secrets

## SimpMusic

### Study

- online source boundaries
- artist/album/playlist catalog mapping
- desktop streaming UX
- lyrics matching using title/artist/duration

### Do not import wholesale

- Compose UI and Android platform layers
- hidden playback or media extraction code
- crash/analytics integrations

## Metrolist

### Study

- online library organization
- queue/radio behavior
- lyrics provider contracts
- account-sync UX

### Exclude

- Android-specific services and UI
- restricted/proprietary provider modules
- media extraction and ad-bypass behavior

## ViMusic

UX reference only: simplicity, compact navigation, queue and lyrics philosophy.

## Delivery gates

1. Native foundation builds and tests on clean Windows CI.
2. Dependency/license blockers are removed.
3. Visible DHUN rebrand builds without changing internal namespaces.
4. Upstream secret-dependent services are disabled.
5. Local player behavior is regression-tested.
6. Only then import the native tree into the main `dhun-music` branch.
7. Add online source contracts before online implementation.
