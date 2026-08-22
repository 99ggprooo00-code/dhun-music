namespace Dhun.Core.Sources;

/// <summary>
/// Catalog operations exposed by sources that advertise the Catalog capability.
/// </summary>
public interface ICatalogSource
{
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
