using Dhun.Core.Models;
using Dhun.Core.Sources;
using Dhun.Core.Sources.Local;
using FluentAssertions;

namespace Dhun.Core.Tests;

public sealed class LocalMusicSourceTests
{
    [Fact]
    public async Task GetTrackAsync_rejects_non_local_identity()
    {
        var source = CreateSource();

        var result = await source.GetTrackAsync(SourceIdentity.YouTube("video"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTrackAsync_maps_existing_local_song()
    {
        var song = new Song
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Test Track",
            Duration = TimeSpan.FromMinutes(3),
            FilePath = @"C:\music\dhun-test-track.mp3",
            FolderId = Guid.NewGuid()
        };

        var source = new LocalMusicSource(
            (identity, _) => Task.FromResult<Song?>(
                identity == SourceIdentity.Local($"song:{song.Id:N}") ? song : null),
            (_, _) => Task.FromResult<IReadOnlyList<Song>>([song]),
            path => path == song.FilePath);

        var result = await source.GetTrackAsync(SourceIdentity.Local($"song:{song.Id:N}"));

        result.Should().NotBeNull();
        result!.Identity.Should().Be(SourceIdentity.Local($"song:{song.Id:N}"));
        result.Title.Should().Be("Test Track");
        result.Duration.Should().Be(TimeSpan.FromMinutes(3));
        result.Availability.Should().Be(MediaAvailability.Available);
        result.Artists.Should().ContainSingle(a => a.Name == Artist.UnknownArtistName);
    }

    [Fact]
    public async Task GetTrackAsync_reports_unavailable_when_filesystem_says_missing()
    {
        var song = new Song
        {
            Id = Guid.NewGuid(),
            Title = "Missing Track",
            FilePath = @"C:\music\missing.mp3",
            FolderId = Guid.NewGuid()
        };

        var source = new LocalMusicSource(
            (_, _) => Task.FromResult<Song?>(song),
            (_, _) => Task.FromResult<IReadOnlyList<Song>>([]),
            _ => false);

        var result = await source.GetTrackAsync(SourceIdentity.Local($"song:{song.Id:N}"));

        result.Should().NotBeNull();
        result!.Availability.Should().Be(MediaAvailability.Unavailable);
    }

    [Fact]
    public async Task SearchAsync_clamps_limit_and_maps_results()
    {
        SourceSearchQuery? received = null;
        var song = new Song
        {
            Title = "Search Result",
            Duration = TimeSpan.FromSeconds(90),
            FilePath = @"C:\music\result.mp3",
            FolderId = Guid.NewGuid()
        };

        var source = new LocalMusicSource(
            (_, _) => Task.FromResult<Song?>(song),
            (query, _) =>
            {
                received = query;
                return Task.FromResult<IReadOnlyList<Song>>([song]);
            },
            _ => true);

        var result = await source.SearchAsync(new SourceSearchQuery("result", Limit: 500));

        received.Should().NotBeNull();
        received!.Limit.Should().Be(100);
        result.Tracks.Should().ContainSingle(t => t.Title == "Search Result");
    }

    [Fact]
    public async Task SearchAsync_rejects_blank_query()
    {
        var source = CreateSource();

        var action = () => source.SearchAsync(new SourceSearchQuery("  "));

        await action.Should().ThrowAsync<ArgumentException>();
    }

    private static LocalMusicSource CreateSource()
    {
        return new LocalMusicSource(
            (_, _) => Task.FromResult<Song?>(null),
            (_, _) => Task.FromResult<IReadOnlyList<Song>>([]),
            _ => false);
    }
}
