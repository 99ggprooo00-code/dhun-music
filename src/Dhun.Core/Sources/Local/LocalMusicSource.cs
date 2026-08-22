using Dhun.Core.Models;

namespace Dhun.Core.Sources.Local;

/// <summary>
/// Adapts DHUN's existing local-library model to the source-neutral music contracts.
/// Persistence and filesystem access remain outside this adapter.
/// </summary>
public sealed class LocalMusicSource : IMusicSource, ISearchSource
{
    private readonly Func<SourceIdentity, CancellationToken, Task<Song?>> _getSongAsync;
    private readonly Func<SourceSearchQuery, CancellationToken, Task<IReadOnlyList<Song>>> _searchAsync;
    private readonly Func<string, bool> _isFileAvailable;

    public LocalMusicSource(
        Func<SourceIdentity, CancellationToken, Task<Song?>> getSongAsync,
        Func<SourceSearchQuery, CancellationToken, Task<IReadOnlyList<Song>>> searchAsync,
        Func<string, bool> isFileAvailable)
    {
        _getSongAsync = getSongAsync ?? throw new ArgumentNullException(nameof(getSongAsync));
        _searchAsync = searchAsync ?? throw new ArgumentNullException(nameof(searchAsync));
        _isFileAvailable = isFileAvailable ?? throw new ArgumentNullException(nameof(isFileAvailable));
    }

    public string Id => "local";
    public string DisplayName => "Local music";
    public MusicSourceKind Kind => MusicSourceKind.Local;

    // Only advertise capabilities that are fully implemented by this adapter.
    public MusicSourceCapabilities Capabilities => MusicSourceCapabilities.Search;

    public async Task<SourceTrack?> GetTrackAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (identity.Kind != MusicSourceKind.Local || !identity.IsValid)
        {
            return null;
        }

        var song = await _getSongAsync(identity, cancellationToken).ConfigureAwait(false);
        return song is null ? null : LocalSourceMapper.ToTrack(song, _isFileAvailable);
    }

    public Task<SourceAlbum?> GetAlbumAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default)
    {
        // Album retrieval will be wired to the existing album repository in the library phase.
        return Task.FromResult<SourceAlbum?>(null);
    }

    public Task<SourceArtist?> GetArtistAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default)
    {
        // Artist retrieval will be wired to the existing artist repository in the library phase.
        return Task.FromResult<SourceArtist?>(null);
    }

    public Task<SourcePlaylist?> GetPlaylistAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default)
    {
        // Playlist retrieval will be wired after the local track mapping is stable.
        return Task.FromResult<SourcePlaylist?>(null);
    }

    public async Task<SourceSearchPage> SearchAsync(
        SourceSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query.Text);

        var limit = Math.Clamp(query.Limit, 1, 100);
        var songs = await _searchAsync(query with { Limit = limit }, cancellationToken)
            .ConfigureAwait(false);

        return new SourceSearchPage(
            songs.Select(song => LocalSourceMapper.ToTrack(song, _isFileAvailable)).ToArray(),
            Array.Empty<SourceAlbum>(),
            Array.Empty<SourceArtist>(),
            Array.Empty<SourcePlaylist>(),
            null);
    }
}

internal static class LocalSourceMapper
{
    public static SourceTrack ToTrack(Song song, Func<string, bool> isFileAvailable)
    {
        ArgumentNullException.ThrowIfNull(song);
        ArgumentNullException.ThrowIfNull(isFileAvailable);

        var artists = song.SongArtists
            .OrderBy(a => a.Order)
            .Select(a => a.Artist)
            .Where(artist => artist is not null)
            .Select(artist => new SourceArtist(
                SourceIdentity.Local($"artist:{artist!.Id:N}"),
                artist.Name))
            .ToArray();

        if (artists.Length == 0)
        {
            artists = [new SourceArtist(
                SourceIdentity.Local("artist:unknown"),
                Artist.UnknownArtistName)];
        }

        var artwork = song.Album?.CoverArtUri ?? song.AlbumArtUriFromTrack;
        SourceAlbum? album = song.Album is null
            ? null
            : new SourceAlbum(
                SourceIdentity.Local($"album:{song.Album.Id:N}"),
                song.Album.Title,
                artists,
                TryCreateUri(artwork),
                song.Album.Year ?? song.Year);

        return new SourceTrack(
            SourceIdentity.Local($"song:{song.Id:N}"),
            song.Title,
            artists,
            song.Duration,
            isFileAvailable(song.FilePath) ? MediaAvailability.Available : MediaAvailability.Unavailable,
            album,
            TryCreateUri(artwork),
            false,
            song.TrackNumber,
            song.DiscNumber);
    }

    private static Uri? TryCreateUri(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;
    }
}
