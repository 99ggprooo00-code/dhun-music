# Security policy

DHUN Native is under active foundation development and has not published a stable release.

## Reporting a vulnerability

Open a private GitHub security advisory for the repository rather than a public issue when a report involves credential handling, arbitrary file access, code execution, update integrity, package signing, OAuth tokens, or private user data.

Do not include real API keys, OAuth tokens, passwords, library databases, or private file paths in public reports.

## Current security boundaries

- Local library and playback data remain on the device.
- Online account integration is not enabled during the native-foundation milestone.
- Upstream private deployment and signing credentials are not inherited.
- Optional Discord, Last.fm and ListenBrainz code remains disabled by default and will be audited before any DHUN release.
- YouTube integration must use an isolated provider boundary and visible policy-compliant playback.
- Unsigned development packages must be clearly marked as previews.

## Supported versions

No stable version is supported yet. The version remains `0.1.0` during development.
