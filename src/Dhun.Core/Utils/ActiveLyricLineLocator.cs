using Dhun.Core.Models.Lyrics;

namespace Dhun.Core.Utils;

/// <summary>
///     Locates the lyric line that should be active for a playback position.
/// </summary>
/// <remarks>
///     Synchronization rules:
///     <list type="bullet">
///         <item>Lines must be sorted by start time (guaranteed by <see cref="ParsedLrc" />).</item>
///         <item>The active line is the latest line whose start time is at or before the position.</item>
///         <item>Before the first timestamp no line is active; the UI shows the intro/placeholder state.</item>
///         <item>Lookup is a binary search, so per-frame synchronization stays O(log n) for any lyric size.</item>
///         <item>The hinted overload short-circuits the common "still on the same line" and
///         "advanced by exactly one line" cases to O(1) during continuous playback.</item>
///     </list>
/// </remarks>
public static class ActiveLyricLineLocator
{
    /// <summary>
    ///     Returns the index of the active line for <paramref name="position" />,
    ///     or <c>-1</c> when the collection is empty or the position precedes the first line.
    /// </summary>
    public static int FindActiveIndex(IReadOnlyList<LyricLine> lines, TimeSpan position)
    {
        if (lines is null || lines.Count == 0)
            return -1;

        var low = 0;
        var high = lines.Count - 1;
        var latestMatchIndex = -1;

        while (low <= high)
        {
            var mid = low + (high - low) / 2;
            if (lines[mid].StartTime <= position)
            {
                latestMatchIndex = mid;
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return latestMatchIndex;
    }

    /// <summary>
    ///     Hinted overload for per-frame playback synchronization. When <paramref name="hintIndex" />
    ///     (the previous frame's active index) already covers the position, it is returned without
    ///     searching; otherwise the binary search runs and the hint is updated.
    /// </summary>
    public static int FindActiveIndex(
        IReadOnlyList<LyricLine> lines,
        TimeSpan position,
        ref int hintIndex)
    {
        if (lines is null || lines.Count == 0)
        {
            hintIndex = -1;
            return -1;
        }

        if (hintIndex >= 0 && hintIndex < lines.Count)
        {
            var nextStart = hintIndex + 1 < lines.Count ? lines[hintIndex + 1].StartTime : TimeSpan.MaxValue;
            if (lines[hintIndex].StartTime <= position && position < nextStart)
                return hintIndex;
        }

        var index = FindActiveIndex(lines, position);

        // Preserve the historical service contract: when the position precedes every line,
        // the hint rewinds to the first line so continuous playback recovers without a re-search.
        hintIndex = index >= 0 ? index : 0;

        return index;
    }
}
