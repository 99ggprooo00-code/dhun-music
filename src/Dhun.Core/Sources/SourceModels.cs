namespace Dhun.Core.Sources;

public sealed record SourceArtist(SourceIdentity Identity, string Name, Uri? Artwork = null);

public sealed record SourceAlbum(
    SourceIdentity Identity,
    string Title,
    IReadOnlyList<SourceArtist> Artists,
    Uri? Artwork = null,
    int? Year = null);

public sealed record SourceTrack(
    SourceIdentity Identity,
    string Title,
    IReadOnlyList<SourceArtist> Artists,
    TimeSpan Duration,
    MediaAvailability Availability,
    SourceAlbum? Album = null,
    Uri? Artwork = null,
    bool IsExplicit = false,
    int? TrackNumber = null,
    int? DiscNumber = null);

public sealed record SourcePlaylist(
    SourceIdentity Identity,
    string Name,
    string? Description,
    Uri? Artwork,
    int? TrackCount);

public enum SourceSearchType
{
    All = 0,
    Track = 1,
    Album = 2,
    Artist = 3,
    Playlist = 4
}

public sealed record SourceSearchQuery(
    string Text,
    SourceSearchType Type = SourceSearchType.All,
    int Limit = 25,
    string? ContinuationToken = null);

public sealed record SourceSearchPage(
    IReadOnlyList<SourceTrack> Tracks,
    IReadOnlyList<SourceAlbum> Albums,
    IReadOnlyList<SourceArtist> Artists,
    IReadOnlyList<SourcePlaylist> Playlists,
    string? ContinuationToken = null);
