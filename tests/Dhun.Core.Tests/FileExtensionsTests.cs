using FluentAssertions;
using Dhun.Core.Constants;
using Xunit;

namespace Dhun.Core.Tests;

public class FileExtensionsTests
{
    [Theory]
    [InlineData(".mp3")]
    [InlineData(".flac")]
    [InlineData(".aac")]
    [InlineData(".m4a")]
    [InlineData(".ogg")]
    [InlineData(".opus")]
    [InlineData(".wav")]
    [InlineData(".wma")]
    [InlineData(".aiff")]
    [InlineData(".ape")]
    [InlineData(".webm")]
    [InlineData(".mpc")]
    [InlineData(".mpp")]
    [InlineData(".AA")]
    public void MusicFileExtensions_ContainsVerifiedPlaybackFormats(string extension)
    {
        FileExtensions.MusicFileExtensions.Should().Contain(extension);
    }

    [Theory]
    [InlineData(".aax")]
    [InlineData(".m4p")]
    [InlineData(".m2v")]
    [InlineData(".mpv")]
    public void MusicFileExtensions_DoesNotAdvertiseEncryptedOrVideoOnlyFormats(string extension)
    {
        FileExtensions.MusicFileExtensions.Should().NotContain(extension);
    }
}
