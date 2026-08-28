namespace Dhun.Core.Utils;

/// <summary>
///     Ordering strategies used when a queue is shuffled.
/// </summary>
public enum QueueShuffleStrategy
{
    /// <summary>
    ///     Uniform pseudo-random permutation. Adjacent items may share the same grouping key.
    /// </summary>
    Default = 0,

    /// <summary>
    ///     Smart shuffle: tries hard to avoid placing items with the same grouping key (typically the
    ///     primary artist) back to back while keeping the order unpredictable.
    /// </summary>
    SmartNoAdjacentDuplicates = 1
}

/// <summary>
///     An item that can participate in a queue shuffle.
/// </summary>
/// <param name="Id">Stable identity of the queued entity. Must be unique within one shuffle call.</param>
/// <param name="GroupingKey">
///     Optional key used by <see cref="QueueShuffleStrategy.SmartNoAdjacentDuplicates" /> to keep related
///     items (same artist, same album, ...) apart. Items without a key behave as one group.
/// </param>
public readonly record struct QueueShuffleItem(Guid Id, string? GroupingKey = null);

/// <summary>
///     Pure queue-shuffle helper. It contains no playback, database or UI dependencies so that queue
///     ordering semantics can be unit tested and later reused by the local and any future online queue.
/// </summary>
/// <remarks>
///     Intended integration point: <c>MusicPlaybackService.GenerateShuffledQueue()</c> during the local
///     player regression milestone. The service currently owns the shuffled queue structure; this helper
///     only supplies the ordering algorithm.
/// </remarks>
public static class QueueShuffler
{
    /// <summary>
    ///     Deterministically shuffles <paramref name="items" /> for a given seed, optionally spreading
    ///     same-key items apart. Repeating the call with the same items and seed reproduces the same order,
    ///     which keeps resume-after-restart behavior stable.
    /// </summary>
    public static IReadOnlyList<QueueShuffleItem> Shuffle(
        IEnumerable<QueueShuffleItem> items,
        QueueShuffleStrategy strategy = QueueShuffleStrategy.Default,
        int seed = 0)
    {
        ArgumentNullException.ThrowIfNull(items);

        var pool = items as IReadOnlyList<QueueShuffleItem> ?? items.ToList();
        if (pool.Count <= 1)
            return pool.ToArray();

        var rng = seed == 0 ? Random.Shared : new Random(seed);
        var buffer = new List<QueueShuffleItem>(pool);
        FisherYatesShuffle(buffer, rng);

        return strategy == QueueShuffleStrategy.SmartNoAdjacentDuplicates
            ? SpreadAdjacentGroups(buffer)
            : buffer;
    }

    private static void FisherYatesShuffle(List<QueueShuffleItem> list, Random rng)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    ///     Packs the shuffled list into buckets keyed by grouping key and interleaves the largest
    ///     remaining bucket first. This is the classic "deal round-robin from the most frequent group"
    ///     construction: it yields no adjacent duplicates whenever the largest bucket size is at most
    ///     ceil(n / 2), and degrades gracefully (a duplicate is accepted) when that is mathematically
    ///     impossible, for example ten songs from one artist.
    /// </summary>
    private static IReadOnlyList<QueueShuffleItem> SpreadAdjacentGroups(List<QueueShuffleItem> shuffled)
    {
        var buckets = shuffled
            .GroupBy(item => item.GroupingKey ?? string.Empty)
            .OrderByDescending(group => group.Count())
            .Select(group => new List<QueueShuffleItem>(group))
            .ToList();

        var result = new List<QueueShuffleItem>(shuffled.Count);
        while (buckets.Count > 0)
        {
            foreach (var bucket in buckets)
            {
                result.Add(bucket[^1]);
                bucket.RemoveAt(bucket.Count - 1);
            }

            buckets.RemoveAll(bucket => bucket.Count == 0);
        }

        return result;
    }
}
