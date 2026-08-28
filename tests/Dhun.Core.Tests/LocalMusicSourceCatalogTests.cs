using Dhun.Core.Models;
using Dhun.Core.Sources;
using Dhun.Core.Sources.Local;
using FluentAssertions;

namespace Dhun.Core.Tests;

public sealed class LocalMusicSourceCatalogTests
{
    [Fact]
    public async Task Catalog_adapters_are_advertised_only_when_all_are_available()
    {
        var source = CreateSource(
            (_, _) => Task.FromResult<Album?>(new Album { Title = "Album" }),
            (_, _) => Task.FromResult<Artist?>(new Artist { Name = "Artist" }),
            (_, _) => Task.FromResult<Playlist?>(new Playlist { Name = "Playlist" }));

        source.Capabilities.Should().HaveFlag(MusicSourceCapabilities.Search);
        source.Capabilities.Should().HaveFlag(MusicSourceCapabilities.Catalog);

        var album = await source.GetAlbumAsync(SourceIdentity.Local("album:1"));
        var artist = await source.GetArtistAsync(SourceIdentity.Local("artist:2"));
        var playlist = await source.GetPlaylistAsync(SourceIdentity.Local("playlist:3"));

        album.Should().NotBeNull();
        album!.Title.Should().Be("Album");
        album.Identity.Should().Be(SourceIdentity.Local("album:00000000000000000000000000000000"));
        artist.Should().NotBeNull();
        artist!.Name.Should().Be("Artist");
        playlist.Should().NotBeNull();
        playlist!.Name.Should().Be("Playlist");
        playlist.TrackCount.Should().Be(0);
    }

    [Fact]
    public async Task Catalog_adapters_reject_non_local_identities()
    {
        var called = false;
        var source = CreateSource(
            (_, _) =>
            {
                called = true;
                return Task.FromResult<Album?>(new Album());
            },
            (_, _) => Task.FromResult<Artist?>(new Artist()),
            (_, _) => Task.FromResult<Playlist?>(new Playlist()));

        var result = await source.GetAlbumAsync(SourceIdentity.YouTube("video"));

        result.Should().BeNull();
        called.Should().BeFalse();
    }

    [Fact]
    public async Task Catalog_mapping_preserves_artist_identity_and_remote_artwork()
    {
        var artist = new Artist
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Test Artist",
            RemoteImageUrl = "https://example.com/artist.jpg"
        };

        var source = CreateSource(
            (_, _) => Task.FromResult<Album?>(null),
            (_, _) => Task.FromResult<Artist?>(artist),
            (_, _) => Task.FromResult<Playlist?>(null));

        var result = await source.GetArtistAsync(SourceIdentity.Local("artist:test"));

        result.Should().NotBeNull();
        result!.Identity.Should().Be(SourceIdentity.Local("artist:22222222222222222222222222222222"));
        result.Name.Should().Be("Test Artist");
        result.Artwork.Should().Be(new Uri("https://example.com/artist.jpg"));
    }

    private static LocalMusicSource CreateSource(
        Func<SourceIdentity, CancellationToken, Task<Album?>> getAlbum,
        Func<SourceIdentity, CancellationToken, Task<Artist?>> getArtist,
        Func<SourceIdentity, CancellationToken, Task<Playlist?>> getPlaylist)
    {
        return new LocalMusicSource(
            (_, _) => Task.FromResult<Song?>(null),
            (_, _) => Task.FromResult<IReadOnlyList<Song>>([]),
            _ => true,
            getAlbum,
            getArtist,
            getPlaylist);
    }
}
