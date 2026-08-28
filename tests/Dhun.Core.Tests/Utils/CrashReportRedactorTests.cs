using System.Text;
using Dhun.Core.Utils;
using FluentAssertions;

namespace Dhun.Core.Tests.Utils;

public sealed class CrashReportRedactorTests
{
    [Fact]
    public void Redact_HidesJsonConfiguredApiKeys()
    {
        var input = """
                    {"DhunApiServer":{"Url":"https://example.test","ApiKey":"super-secret-value-123"}}
                    """;

        var output = CrashReportRedactor.Redact(input);

        output.Should().NotContain("super-secret-value-123");
        output.Should().Contain("[REDACTED]");
        output.Should().Contain("https://example.test", "non-secret URLs must survive for diagnosis");
    }

    [Fact]
    public void Redact_HidesAssignmentAndQuerySecrets()
    {
        var input = """
                    api_key = abcdef0123456789
                    apikey=deadbeefdeadbeef&format=json
                    ?sk=5f4dcc3b5aa765d61d8327deb882cf99&lang=en
                    """;

        var output = CrashReportRedactor.Redact(input);

        output.Should().NotContain("abcdef0123456789");
        output.Should().NotContain("deadbeefdeadbeef");
        output.Should().NotContain("5f4dcc3b5aa765d61d8327deb882cf99");
        output.Should().Contain("format=json", "unrelated parameters must stay visible");
        output.Should().Contain("lang=en");
    }

    [Fact]
    public void Redact_HidesBearerTokensAndKnownKeyFormats()
    {
        var input = """
                    Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.payload.signature
                    request key=AIzaSyCqW3Z9fakefakefakefakefakefa12345
                    github token gho_1234567890abcdefghijABCDEFGHIJ1234567890
                    oauth ya29.a0ARadOmFakeFakeFake-fake123
                    """;

        var output = CrashReportRedactor.Redact(input);

        output.Should().NotContain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9");
        output.Should().NotContain("AIzaSyCqW3Z9");
        output.Should().NotContain("gho_1234567890");
        output.Should().NotContain("ya29.a0ARadOm");
        output.Should().Contain("[REDACTED]");
        output.Should().Contain("[REDACTED]");
        
    }

    [Fact]
    public void Redact_HidesUserProfilePaths()
    {
        var input = """
                    at Dhun.Core.Services.Implementations.LibraryService.Scan(C:\Users\Ramesh\AppData\Local\Dhun\library.db)
                    GET https://cdn.example/cover?path=C:/Users/Ramesh/Music/album/track.flac
                    """;

        var output = CrashReportRedactor.Redact(input);

        output.Should().NotContain("Ramesh");
        output.Should().Contain("[USER]");
        output.Should().Contain("library.db", "the non-personal remainder of the path stays diagnosable");
    }

    [Fact]
    public void Redact_KeepsStackFramesAndMessageText()
    {
        var input = """
                    System.InvalidOperationException: Sequence contains no elements
                       at Dhun.Core.Services.Implementations.MusicPlaybackService.NextAsync()
                       at Dhun.WinUI.ViewModels.PlayerViewModel.OnTimerTick()
                    """;

        var output = CrashReportRedactor.Redact(input);

        output.Should().Be(input, "plain diagnostic text must not be modified");
    }

    [Fact]
    public void Redact_KeepsNonProfileDrivePaths()
    {
        var input = "Could not read D:\\Music\\album disc2\\track 01.flac";

        var output = CrashReportRedactor.Redact(input);

        output.Should().Be(input, "user-owned music folders outside the profile are kept for diagnosis");
    }

    [Fact]
    public void Redact_DoesNotFalsePositiveOnWordsContainingKeyNames()
    {
        var input = """
                    monkey=curious turnkey=solution donkey=stubborn
                    [KeyInfo] artist=The Keys
                    """;

        var output = CrashReportRedactor.Redact(input);

        output.Should().Contain("monkey=curious");
        output.Should().Contain("turnkey=solution");
        output.Should().Contain("donkey=stubborn");
        output.Should().Contain("artist=The", "ordinary text is preserved (values with spaces are not assignment-shaped)");
    }

    [Fact]
    public void Redact_TruncatesToRecentTail()
    {
        var input = string.Concat(Enumerable.Repeat("log line with filler content\n", 10_000));

        var output = CrashReportRedactor.Redact(input, maxCharacters: 2_048);

        output.Should().StartWith("[... earlier log truncated ...]");
        output.Length.Should().BeLessThanOrEqualTo(2_048 + "[... earlier log truncated ...]\n".Length);
        output.Should().EndWith("log line with filler content\n");
    }

    [Fact]
    public void Redact_HandlesNullAndEmpty()
    {
        CrashReportRedactor.Redact(null).Should().BeEmpty();
        CrashReportRedactor.Redact(string.Empty).Should().BeEmpty();
    }

    [Fact]
    public void Redact_HandlesLargeInputInBoundedTime()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 20_000; i++)
            sb.Append("info: playback state changed api_key=").Append(i).Append(" secret=\"x\" C:\\Users\\Someone\\AppData\\Local\\n");
        var input = sb.ToString();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var output = CrashReportRedactor.Redact(input, maxCharacters: CrashReportRedactor.DefaultMaxReportCharacters);
        sw.Stop();

        output.Should().NotContain("C:\\Users\\Someone");
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10), "crash handling must not hang on large logs");
    }
}
