# DHUN Native

A Windows-first, privacy-oriented music player combining a serious local library with a staged online-music architecture.

> **Status:** `v1.0.0` native preview. The Windows foundation passes automated tests and packaged startup checks, but remains unsigned and awaits broader real-device regression testing.

## Native foundation

DHUN Native is a GPL-3.0 fork of [Nagi](https://github.com/Anthonyy232/Nagi), currently based on upstream revision `60f593ce1a54315fe1247d7fd0a3d89bdca768eb`.

The native stack provides:

- C# and .NET 10
- WinUI 3 and Windows App SDK
- MVVM with CommunityToolkit
- EF Core and SQLite
- LibVLCSharp local playback
- ATL metadata extraction
- local library scanning, queue, playlists, smart playlists and history
- synchronized lyrics
- equalizer and ReplayGain
- mini-player, system tray and Windows media controls
- file associations, startup integration and native Fluent UI

## DHUN direction

DHUN is **provider-first and local-first**: the full product architecture is built around source-neutral
contracts and legal/local audio, and online providers are added one at a time later, each isolated behind an
adapter. The whole roadmap ordering is recorded in
[`docs/ADR/0004-lawful-provider-first-boundary.md`](docs/ADR/0004-lawful-provider-first-boundary.md);
verified reference-product status and the P0–P3 feature map live in
[`docs/UPSTREAM-FEATURE-MAP.md`](docs/UPSTREAM-FEATURE-MAP.md).

Planned source boundary:

```text
Dhun.Core.Sources (provider interface)
├── Local source (Nagi/LibVLC foundation) — the reference workload, fully offline
├── Future lawful providers (licensed / public-domain catalogs) — one adapter each
└── Official metadata APIs — catalog/lyrics matching only, visible playback only
```

DHUN will not implement stream or signature extraction, provider downloads/"offline mode", audio
separation, hidden or background provider playback, ad removal, or access-control bypassing — these are
permanently out of scope, not deferred.

## Build

Requirements:

- Windows 10/11
- Visual Studio with Windows App SDK tooling, or GitHub Actions Windows runner
- .NET SDK 10.0.101 or compatible feature band
- FFmpeg for the test suite

```powershell
dotnet test tests/Dhun.Core.Tests/Dhun.Core.Tests.csproj -c Release
dotnet restore src/Dhun.WinUI/Dhun.WinUI.csproj
dotnet publish src/Dhun.WinUI/Dhun.WinUI.csproj -c Release -r win-x64 -p:Platform=x64 -p:GenerateAppxPackageOnBuild=false
```

## Licensing

DHUN Native is licensed under GPL-3.0. See `LICENSE`, `UPSTREAM.md`, and `THIRD_PARTY.md`.

The previous Electron implementation is preserved separately in
[`99ggprooo00-code/dhun-music-electron-archive`](https://github.com/99ggprooo00-code/dhun-music-electron-archive)
under branch `legacy/electron-player-v2`.
