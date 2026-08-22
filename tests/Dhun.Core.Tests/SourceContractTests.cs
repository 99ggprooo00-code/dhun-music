using Dhun.Core.Sources;
using Xunit;

namespace Dhun.Core.Tests;

public sealed class SourceContractTests
{
    [Fact]
    public void LocalIdentity_IsStableAndValid()
    {
        var identity = SourceIdentity.Local("song-123");

        Assert.True(identity.IsValid);
        Assert.Equal(MusicSourceKind.Local, identity.Kind);
        Assert.Equal("song-123", identity.ExternalId);
        Assert.Equal("Local:song-123", identity.ToString());
    }

    [Fact]
    public void YouTubeIdentity_IsProviderScoped()
    {
        var identity = SourceIdentity.YouTube("video-123");

        Assert.True(identity.IsValid);
        Assert.Equal(MusicSourceKind.YouTube, identity.Kind);
        Assert.Equal("video-123", identity.ExternalId);
        Assert.Equal("YouTube:video-123", identity.ToString());
    }

    [Fact]
    public void SourceCapabilities_AreComposable()
    {
        var capabilities = MusicSourceCapabilities.Search |
                           MusicSourceCapabilities.Catalog |
                           MusicSourceCapabilities.Lyrics;

        Assert.True(capabilities.HasFlag(MusicSourceCapabilities.Search));
        Assert.True(capabilities.HasFlag(MusicSourceCapabilities.Catalog));
        Assert.True(capabilities.HasFlag(MusicSourceCapabilities.Lyrics));
        Assert.False(capabilities.HasFlag(MusicSourceCapabilities.Playback));
    }

    [Fact]
    public void SearchQuery_DefaultsAreSafeForUiUse()
    {
        var query = new SourceSearchQuery("daft punk");

        Assert.Equal(SourceSearchType.All, query.Type);
        Assert.Equal(25, query.Limit);
        Assert.Null(query.ContinuationToken);
    }
}
