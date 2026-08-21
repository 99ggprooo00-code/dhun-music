# Contributing to DHUN Native

DHUN Native is a GPL-3.0 Windows music player derived from Nagi. Contributions must preserve upstream attribution and follow the staged native roadmap.

## Before coding

1. Read `UPSTREAM.md`, `THIRD_PARTY.md`, `SECURITY.md`, and `docs/NATIVE_IMPLEMENTATION_MAP.md`.
2. Use an existing milestone issue or open a narrowly scoped proposal.
3. Do not add private API keys, certificates, tokens, account cookies, or proprietary modules.
4. Do not implement hidden YouTube playback, stream extraction, ad removal, downloading, or access-control bypass.

## Verification

At minimum:

```powershell
dotnet test tests/Dhun.Core.Tests/Dhun.Core.Tests.csproj -c Release
dotnet restore src/Dhun.WinUI/Dhun.WinUI.csproj
dotnet publish src/Dhun.WinUI/Dhun.WinUI.csproj -c Release -r win-x64 -p:Platform=x64 -p:GenerateAppxPackageOnBuild=false
```

Changes to playback, storage, scanning, lyrics, queue or metadata require tests. Windows packaging must pass before merging.

## Licensing

By contributing, you agree that your contribution is provided under GPL-3.0. Copied or adapted code must include exact source revision, license and modification notes in `THIRD_PARTY.md`.
