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

## Automated security controls

- GitHub CodeQL scans C# on pull requests, main, and a weekly schedule.
- Dependabot monitors NuGet and GitHub Actions dependencies.
- Pull requests receive a high-severity dependency review gate.
- OpenSSF Scorecard runs weekly and publishes SARIF to GitHub code scanning.
- Core tests publish local coverage and TRX artifacts; no source or user data is sent to an external coverage service.

## Supported versions

No stable version is supported yet. `v1.0.0` is a prerelease until real-device regression and signing requirements are completed.
