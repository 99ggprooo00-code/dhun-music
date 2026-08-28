using Dhun.Core.Models;

namespace Dhun.Core.Sources.Local;

/// <summary>
/// Adapts DHUN's existing local-library model to the source-neutral music contracts.
/// Persistence and filesystem access remain outside this adapter.
/// </summary>
public sealed class LocalMusicSource : IMusicSource, ICatalogSource, ISearchSource
{
    private readonly Func<SourceIdentity, CancellationToken, Task<Song?>> _getSongAsync;
    private readonly Func<SourceSearchQuery, CancellationToken, Task<IReadOnlyList<Song>>> _searchAsync;
    private readonly Func<string, bool> _isFileAvailable;
    private readonly Func<SourceIdentity, CancellationToken, Task<Album?>>? _getAlbumAsync;
    private readonly Func<SourceIdentity, CancellationToken, Task<Artist?>>? _getArtistAsync;
    private readonly Func<SourceIdentity, CancellationToken, Task<Playlist?>>? _getPlaylistAsync;

    public LocalMusicSource(
        Func<SourceIdentity, CancellationToken, Task<Song?>> getSongAsync,
        Func<SourceSearchQuery, CancellationToken, Task<IReadOnlyList<Song>>> searchAsync,
        Func<string, bool> isFileAvailable)
        : this(getSongAsync, searchAsync, isFileAvailable, null, null, null)
    {
    }

    public LocalMusicSource(
        Func<SourceIdentity, CancellationToken, Task<Song?>> getSongAsync,
        Func<SourceSearchQuery, CancellationToken, Task<IReadOnlyList<Song>>> searchAsync,
        Func<string, bool> isFileAvailable,
        Func<SourceIdentity, CancellationToken, Task<Album?>> getAlbumAsync,
        Func<SourceIdentity, CancellationToken, Task<Artist?>> getArtistAsync,
        Func<SourceIdentity, CancellationToken, Task<Playlist?>> getPlaylistAsync)
    {
        _getSongAsync = getSongAsync ?? throw new ArgumentNullException(nameof(getSongAsync));
        _searchAsync = searchAsync ?? throw new ArgumentNullException(nameof(searchAsync));
        _isFileAvailable = isFileAvailable ?? throw new ArgumentNullException(nameof(isFileAvailable));
        _getAlbumAsync = getAlbumAsync;
        _getArtistAsync = getArtistAsync;
        _getPlaylistAsync = getPlaylistAsync;
    }

    public string Id => "local";
    public string DisplayName => "Local music";
    public MusicSourceKind Kind => MusicSourceKind.Local;

    // Catalog is advertised only when all three catalog operations have an adapter.
    public MusicSourceCapabilities Capabilities =>
        MusicSourceCapabilities.Search |
        (_getAlbumAsync is not null && _getArtistAsync is not null && _getPlaylistAsync is not null
            ? MusicSourceCapabilities.Catalog
            : MusicSourceCapabilities.None);

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

    public async Task<SourceAlbum?> GetAlbumAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (!CanRead(identity, _getAlbumAsync))
        {
            return null;
        }

        var album = await _getAlbumAsync!(identity, cancellationToken).ConfigureAwait(false);
        return album is null ? null : LocalSourceMapper.ToAlbum(album);
    }

    public async Task<SourceArtist?> GetArtistAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (!CanRead(identity, _getArtistAsync))
        {
            return null;
        }

        var artist = await _getArtistAsync!(identity, cancellationToken).ConfigureAwait(false);
        return artist is null ? null : LocalSourceMapper.ToArtist(artist);
    }

    public async Task<SourcePlaylist?> GetPlaylistAsync(
        SourceIdentity identity,
        CancellationToken cancellationToken = default)
    {
        if (!CanRead(identity, _getPlaylistAsync))
        {
            return null;
        }

        var playlist = await _getPlaylistAsync!(identity, cancellationToken).ConfigureAwait(false);
        return playlist is null ? null : LocalSourceMapper.ToPlaylist(playlist);
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

    private static bool CanRead<T>(
        SourceIdentity identity,
        Func<SourceIdentity, CancellationToken, Task<T?>>? getter)
        where T : class =>
        identity.Kind == MusicSourceKind.Local && identity.IsValid && getter is not null;
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
            .Select(artist => ToArtist(artist!))
            .ToArray();

        if (artists.Length == 0)
        {
            artists = [new SourceArtist(
                SourceIdentity.Local("artist:unknown"),
                Artist.UnknownArtistName)];
        }

        var artwork = song.Album?.CoverArtUri ?? song.AlbumArtUriFromTrack;
        SourceAlbum? album = song.Album is null ? null : ToAlbum(song.Album);

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

    public static SourceAlbum ToAlbum(Album album)
    {
        ArgumentNullException.ThrowIfNull(album);

        var artists = album.AlbumArtists
            .OrderBy(a => a.Order)
            .Select(a => a.Artist)
            .Where(artist => artist is not null)
            .Select(artist => ToArtist(artist!))
            .ToArray();

        if (artists.Length == 0)
        {
            artists = [new SourceArtist(
                SourceIdentity.Local("artist:unknown"),
                Artist.UnknownArtistName)];
        }

        return new SourceAlbum(
            SourceIdentity.Local($"album:{album.Id:N}"),
            album.Title,
            artists,
            TryCreateUri(album.CoverArtUri),
            album.Year);
    }

    public static SourceArtist ToArtist(Artist artist)
    {
        ArgumentNullException.ThrowIfNull(artist);
        return new SourceArtist(
            SourceIdentity.Local($"artist:{artist.Id:N}"),
            artist.Name,
            TryCreateUri(artist.ImageUri));
    }

    public static SourcePlaylist ToPlaylist(Playlist playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);
        return new SourcePlaylist(
            SourceIdentity.Local($"playlist:{playlist.Id:N}"),
            playlist.Name,
            playlist.Description,
            TryCreateUri(playlist.CoverImageUri),
            playlist.PlaylistSongs.Count);
    }

    private static Uri? TryCreateUri(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;
    }
}
