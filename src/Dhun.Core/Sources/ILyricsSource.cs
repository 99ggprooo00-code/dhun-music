namespace Dhun.Core.Sources;

/// <summary>Provides lyrics for source-owned tracks.</summary>
public interface ILyricsSource
{
    Task<SourceLyrics?> GetLyricsAsync(
        SourceIdentity track,
        CancellationToken cancellationToken = default);
}

public sealed record SourceLyrics(
    string? PlainText,
    string? SyncedText);
