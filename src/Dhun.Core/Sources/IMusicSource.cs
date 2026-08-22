namespace Dhun.Core.Sources;

/// <summary>
/// Common catalog contract implemented by a music source.
/// Capabilities that a source does not advertise should not be invoked.
/// </summary>
public interface IMusicSource
{
    string Id { get; }
    string DisplayName { get; }
    MusicSourceKind Kind { get; }
    MusicSourceCapabilities Capabilities { get; }

    Task<SourceTrack?> GetTrackAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default);

    Task<SourceAlbum?> GetAlbumAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default);

    Task<SourceArtist?> GetArtistAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default);

    Task<SourcePlaylist?> GetPlaylistAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default);
}
