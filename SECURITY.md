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
- YouTube integration must use an isolated provider boundary and visible policy-compliant playback;
  provider download/extraction/background-playback capabilities are permanently out of scope
  (see `docs/ADR/0004-lawful-provider-first-boundary.md`).
- Unsigned development packages must be clearly marked as previews.

## Crash diagnostics and telemetry

- DHUN has no telemetry: no analytics endpoint, no automatic crash upload, no silent reporting of any kind.
- Crash details are shown to the user in an in-app dialog and may be written only to the local log directory.
  Nothing leaves the device unless the user explicitly copies or saves the report.
- The crash report content is passed through `CrashReportRedactor` before display or export. It removes
  Windows user-profile paths and common credential shapes (JSON/`key=value` secrets such as API keys,
  subscription keys and session keys, bearer tokens, and recognizable Google/GitHub/Slack token formats),
  while keeping exception types, messages and stack frames intact for diagnosis.
- If redaction ever has to be bypassed for debugging, that must be a deliberate user action on the local
  log file; it must never be a code path that sends unredacted data anywhere.
- The live local log (`Logs/log.txt`) is owned by the user and is not rewritten; redaction applies to
  anything presented for sharing.

## Automated security controls

- GitHub CodeQL scans C# on pull requests, main, and a weekly schedule.
- Dependabot monitors NuGet and GitHub Actions dependencies.
- Pull requests receive a high-severity dependency review gate.
- OpenSSF Scorecard runs weekly and publishes SARIF to GitHub code scanning.
- Core tests publish local coverage and TRX artifacts; no source or user data is sent to an external coverage service.

## Supported versions

No stable version is supported yet. `v1.0.0` is a prerelease until real-device regression and signing requirements are completed.
