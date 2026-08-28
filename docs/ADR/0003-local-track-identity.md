# ADR-0003: Stable local track identity

- **Status:** Accepted
- **Date:** 2026-08-22
- **Scope:** Local library reconciliation

## Context

The current `Song` model stores a filesystem path and the library scanner currently reconciles files primarily by path and modification time. A path is a location, not an identity: users can rename tracks, move albums, reorganize folders, change drive letters, or switch between mapped and canonical paths.

Playlists, favorites, history, ratings, and queue state should survive those operations whenever the underlying audio content is the same.

## Decision

Introduce a content-based local track fingerprint as a **reconciliation key**, while retaining the existing database `Song.Id` as the application's primary entity identity.

The initial fingerprint is SHA-256 of the audio file bytes. It is exposed through `LocalTrackIdentity` and is intentionally separate from the common `SourceTrack` model.

The scanner will use the fingerprint progressively rather than hashing every file on every scan:

1. Use the existing path + modification metadata fast path first.
2. When a file disappears and a plausible new file appears, use fingerprints to reconnect the existing `Song` row.
3. Persist the fingerprint on `Song` only as part of a reviewed database migration.
4. Keep duplicate-content policy separate from identity calculation.

## Consequences

### Positive

- Renames and moves can preserve the existing `Song.Id`.
- Playlists, favorites, history, ratings, and queue references can survive file relocation.
- The common source model remains provider-neutral.
- SHA-256 provides deterministic reconciliation across sessions and machines.

### Trade-offs

- Hashing is more expensive than comparing path and modification time.
- Identical byte-for-byte files have the same fingerprint and therefore require a separate duplicate policy.
- Persisting fingerprints requires a database migration and scanner reconciliation logic.

## Non-goals

- Do not replace `Song.Id` with the fingerprint.
- Do not hash every file during every normal scan.
- Do not make the online source model depend on local fingerprints.

## Acceptance criteria

Before the database migration is merged:

- unchanged files avoid hashing on normal incremental scans;
- moved/renamed files can be matched by fingerprint;
- missing/unreachable folders cannot trigger destructive reconciliation;
- duplicate-content behavior is explicitly tested;
- cancellation is honored during hashing;
- the migration is backward-compatible with existing libraries.
