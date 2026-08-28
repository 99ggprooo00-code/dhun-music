# DHUN Implementation Status

> Working document for the provider-first, local-first implementation program.

## Current baseline

- Repository: `99ggprooo00-code/dhun-music`
- Base branch: `main`
- Working branch: `codex/local-source-hardening`
- Existing architecture is retained; changes are incremental.

## Milestone A — Local source hardening

### Contracts
- [x] Source identity and source-neutral models exist.
- [x] Source registry exists.
- [x] Local music source exists.
- [ ] Complete album mapping.
- [ ] Complete artist mapping.
- [ ] Complete playlist mapping.
- [ ] Define explicit unsupported-capability behavior.

### Identity
- [x] Local track identity/fingerprint foundation exists.
- [ ] Add/verify source-aware identity persistence migration.
- [ ] Verify moved/renamed files preserve logical identity.
- [ ] Verify duplicate-content policy.

### Library reconciliation
- [ ] Define filesystem event normalization.
- [ ] Debounce/coalesce filesystem events.
- [ ] Reconcile create/change/delete/rename/move.
- [ ] Recover from events received while the app is closed.
- [ ] Handle unavailable drives and network shares without crashing.

### Playback
- [ ] Connect queue semantics to the production playback path.
- [ ] Verify shuffle/repeat behavior after queue edits.
- [ ] Verify rapid next/previous and seek recovery.
- [ ] Verify zero-duration and corrupt-file recovery.

### Verification
- [ ] Run automated tests.
- [ ] Run Windows CI.
- [ ] Complete physical-device regression matrix in issue #12.

## Rules

1. Do not rewrite the existing player wholesale.
2. Keep local playback fully functional without network access.
3. Providers must be replaceable adapters behind stable contracts.
4. Do not implement unofficial extraction, audio separation, access-control bypass, or unauthorized offline/background playback.
5. Every production behavior change gets a regression test.
6. Distinguish implemented, CI-verified, and physical-device-verified status.
