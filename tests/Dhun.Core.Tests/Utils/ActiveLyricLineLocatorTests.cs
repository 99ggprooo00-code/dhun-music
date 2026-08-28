using Dhun.Core.Models.Lyrics;
using Dhun.Core.Utils;
using FluentAssertions;

namespace Dhun.Core.Tests.Utils;

public sealed class ActiveLyricLineLocatorTests
{
    private static IReadOnlyList<LyricLine> Lines(params double[] startSeconds) =>
        startSeconds.Select((s, i) => new LyricLine(TimeSpan.FromSeconds(s), $"line-{i}")).ToList();

    [Fact]
    public void FindActiveIndex_ReturnsLatestStartedLine()
    {
        var lines = Lines(0, 5, 10, 15);

        ActiveLyricLineLocator.FindActiveIndex(lines, TimeSpan.FromSeconds(11.2)).Should().Be(2);
    }

    [Fact]
    public void FindActiveIndex_BeforeFirstLine_HasNoActiveLine()
    {
        var lines = Lines(3.5, 8, 12);

        ActiveLyricLineLocator.FindActiveIndex(lines, TimeSpan.FromSeconds(1)).Should().Be(-1);
    }

    [Fact]
    public void FindActiveIndex_AtExactTimestamp_ActivatesThatLine()
    {
        var lines = Lines(0, 5, 10);

        ActiveLyricLineLocator.FindActiveIndex(lines, TimeSpan.FromSeconds(5)).Should().Be(1);
    }

    [Fact]
    public void FindActiveIndex_EmptyOrNull_ReturnsMinusOne()
    {
        ActiveLyricLineLocator.FindActiveIndex(Array.Empty<LyricLine>(), TimeSpan.Zero).Should().Be(-1);
        ActiveLyricLineLocator.FindActiveIndex(null!, TimeSpan.Zero).Should().Be(-1);
    }

    [Fact]
    public void FindActiveIndex_WithHint_KeepsSameLineWithoutRescan()
    {
        var lines = Lines(0, 5, 10, 15);
        var hint = 2;

        // Position still inside line 2: hint must be returned untouched.
        ActiveLyricLineLocator.FindActiveIndex(lines, TimeSpan.FromSeconds(12), ref hint).Should().Be(2);
        hint.Should().Be(2);

        // Continuous playback advances exactly one line.
        ActiveLyricLineLocator.FindActiveIndex(lines, TimeSpan.FromSeconds(15), ref hint).Should().Be(3);
        hint.Should().Be(3);
    }

    [Fact]
    public void FindActiveIndex_WithWrongHint_RecoversViaBinarySearch()
    {
        var lines = Lines(0, 5, 10, 15, 20);
        var hint = 4;

        // Simulate a backwards seek: hint points far ahead of the actual position.
        ActiveLyricLineLocator.FindActiveIndex(lines, TimeSpan.FromSeconds(6), ref hint).Should().Be(1);
        hint.Should().Be(1);
    }

    [Fact]
    public void FindActiveIndex_WithHint_BeforeFirstLine_RewindsHintToStart()
    {
        var lines = Lines(4, 8, 12);
        var hint = 2;

        ActiveLyricLineLocator.FindActiveIndex(lines, TimeSpan.FromSeconds(2), ref hint).Should().Be(-1);
        hint.Should().Be(0);
    }

    [Fact]
    public void FindActiveIndex_MatchesLargeLyricList_Exhaustively()
    {
        var lines = Lines(Enumerable.Range(0, 500).Select(i => i * 2.0).ToArray());

        for (var probe = 0; probe < 500; probe++)
        {
            var position = TimeSpan.FromSeconds(probe * 2.0 + 1);
            var index = ActiveLyricLineLocator.FindActiveIndex(lines, position);

            lines[index].StartTime.Should().BeLessThanOrEqualTo(position);

            var next = index + 1 < lines.Count ? lines[index + 1].StartTime : TimeSpan.MaxValue;
            position.Should().BeLessThan(next);
        }
    }
}
