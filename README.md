# DHUN Native

A Windows-first, privacy-oriented music player combining a serious local library with a staged online-music architecture.

> **Status:** native foundation work in progress. The product version remains `0.1.0` until the native migration and core feature set are complete.

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

DHUN will retain and rebrand the proven Windows infrastructure while adding an isolated online source layer and a unified model for local and online tracks.

Planned source boundary:

```text
DHUN UI / Application Core
├── Local source (Nagi/LibVLC foundation)
└── Online source (official visible YouTube playback first)
```

DHUN will not implement hidden YouTube playback, ad removal, stream extraction, or access-control bypassing.

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

The previous Electron implementation is preserved separately in the `dhun-music` repository under branch `legacy/electron-player-v2` and tag `electron-preview-archive-2026-08-22`.
