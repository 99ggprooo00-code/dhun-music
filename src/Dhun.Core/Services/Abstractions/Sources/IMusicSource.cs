using Dhun.Core.Sources;

namespace Dhun.Core.Services.Abstractions.Sources;

public interface IMusicSource
{
    MusicSourceKind Kind { get; }
    string DisplayName { get; }
    MusicSourceCapabilities Capabilities { get; }
}

public interface ISearchSource : IMusicSource
{
    Task<SourceSearchPage> SearchAsync(SourceSearchQuery query, CancellationToken cancellationToken = default);
}

public interface ICatalogSource : IMusicSource
{
    Task<SourceArtist?> GetArtistAsync(SourceIdentity identity, CancellationToken cancellationToken = default);
    Task<SourceAlbum?> GetAlbumAsync(SourceIdentity identity, CancellationToken cancellationToken = default);
    Task<SourcePlaylist?> GetPlaylistAsync(SourceIdentity identity, CancellationToken cancellationToken = default);
}

public interface ILyricsSource : IMusicSource
{
    Task<string?> GetLyricsAsync(SourceTrack track, CancellationToken cancellationToken = default);
}

public interface IPlaybackSource : IMusicSource
{
    Task<PlaybackPreparation> PrepareAsync(SourceTrack track, CancellationToken cancellationToken = default);
}

public sealed record PlaybackPreparation(
    SourceIdentity Identity,
    MediaAvailability Availability,
    Uri? PlaybackUri,
    bool RequiresVisibleHost,
    string? FailureReason = null);
