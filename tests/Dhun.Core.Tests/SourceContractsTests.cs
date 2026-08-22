using Dhun.Core.Sources;
using FluentAssertions;
using Xunit;

namespace Dhun.Core.Tests;

public class SourceContractsTests
{
    [Fact]
    public void SourceIdentity_DistinguishesProvidersWithSameExternalId()
    {
        var local = SourceIdentity.Local("same-id");
        var youtube = SourceIdentity.YouTube("same-id");

        local.Should().NotBe(youtube);
        local.ToString().Should().Be("Local:same-id");
        youtube.ToString().Should().Be("YouTube:same-id");
    }

    [Fact]
    public void SourceTrack_RepresentsLocalOrOnlineContentWithoutProviderSpecificFields()
    {
        var artist = new SourceArtist(SourceIdentity.YouTube("channel-1"), "Artist");
        var track = new SourceTrack(
            SourceIdentity.YouTube("video-1"),
            "Song",
            [artist],
            TimeSpan.FromMinutes(3),
            MediaAvailability.Available);

        track.Identity.Kind.Should().Be(MusicSourceKind.YouTube);
        track.Duration.Should().Be(TimeSpan.FromMinutes(3));
        track.Artists.Should().ContainSingle().Which.Name.Should().Be("Artist");
    }

    [Fact]
    public void Capabilities_AreExplicitAndComposable()
    {
        var capabilities = MusicSourceCapabilities.Search |
                           MusicSourceCapabilities.Catalog |
                           MusicSourceCapabilities.Playback;

        capabilities.Should().HaveFlag(MusicSourceCapabilities.Search);
        capabilities.Should().HaveFlag(MusicSourceCapabilities.Playback);
        capabilities.Should().NotHaveFlag(MusicSourceCapabilities.PlaylistWrite);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyExternalIdentity_IsInvalid(string externalId)
    {
        new SourceIdentity(MusicSourceKind.Local, externalId).IsValid.Should().BeFalse();
    }
}
