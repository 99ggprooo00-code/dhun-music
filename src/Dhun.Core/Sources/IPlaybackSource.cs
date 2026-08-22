namespace Dhun.Core.Sources;

/// <summary>
/// Resolves a source-owned track for playback without exposing provider-specific
/// transport details to the UI or queue.
/// </summary>
public interface IPlaybackSource
{
    Task<PlaybackSourceResult> PrepareAsync(
        SourceIdentity track,
        CancellationToken cancellationToken = default);
}

public sealed record PlaybackSourceResult(
    bool IsAvailable,
    MediaAvailability Availability,
    Uri? PlaybackUri = null,
    string? Error = null);
