using Dhun.Core.Utils;
using FluentAssertions;

namespace Dhun.Core.Tests.Utils;

public sealed class QueueShufflerTests
{
    private static List<QueueShuffleItem> Items(int count, string key) =>
        Enumerable.Range(0, count).Select(_ => new QueueShuffleItem(Guid.NewGuid(), key)).ToList();

    [Fact]
    public void Shuffle_PreservesAllItemsExactlyOnce()
    {
        var input = Items(50, "artist-a").Concat(Items(30, "artist-b")).Concat(Items(20, "artist-c")).ToList();

        var result = QueueShuffler.Shuffle(input, QueueShuffleStrategy.SmartNoAdjacentDuplicates, seed: 42);

        result.Should().HaveCount(input.Count);
        result.Select(item => item.Id).Should().BeEquivalentTo(input.Select(item => item.Id));
    }

    [Fact]
    public void Shuffle_IsDeterministicForSameSeedAndInput()
    {
        var input = Items(40, "artist-a").Concat(Items(40, "artist-b")).ToList();

        var first = QueueShuffler.Shuffle(input, QueueShuffleStrategy.SmartNoAdjacentDuplicates, seed: 7);
        var second = QueueShuffler.Shuffle(input, QueueShuffleStrategy.SmartNoAdjacentDuplicates, seed: 7);

        first.Select(item => item.Id).Should().Equal(second.Select(item => item.Id));
    }

    [Fact]
    public void SmartShuffle_AvoidsAdjacentSameKey_WhenFeasible()
    {
        var input = Items(35, "artist-a").Concat(Items(35, "artist-b")).Concat(Items(30, "artist-c")).ToList();

        var result = QueueShuffler.Shuffle(input, QueueShuffleStrategy.SmartNoAdjacentDuplicates, seed: 123);

        result.Zip(result.Skip(1))
            .Where(pair => pair.First.GroupingKey == pair.Second.GroupingKey)
            .Should()
            .BeEmpty("smart shuffle must not place the same artist back to back when packing is feasible");
    }

    [Fact]
    public void SmartShuffle_DegradesGracefully_WhenSpreadingIsImpossible()
    {
        var input = Items(10, "artist-solo").ToList();

        var result = QueueShuffler.Shuffle(input, QueueShuffleStrategy.SmartNoAdjacentDuplicates, seed: 5);

        // Ten songs from one artist cannot be separated; the strategy must not throw or drop items.
        result.Should().HaveCount(10);
        result.Should().OnlyContain(item => item.GroupingKey == "artist-solo");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Shuffle_HandlesTrivialQueues(int count)
    {
        var input = Items(count, "artist-a");

        var result = QueueShuffler.Shuffle(input, QueueShuffleStrategy.SmartNoAdjacentDuplicates, seed: 1);

        result.Should().HaveCount(count);
        result.Select(item => item.Id).Should().Equal(input.Select(item => item.Id));
    }

    [Fact]
    public void DefaultShuffle_KeepsItemsWithoutKeys()
    {
        var input = Enumerable.Range(0, 25).Select(_ => new QueueShuffleItem(Guid.NewGuid())).ToList();

        var result = QueueShuffler.Shuffle(input, QueueShuffleStrategy.Default, seed: 3);

        result.Select(item => item.Id).Should().BeEquivalentTo(input.Select(item => item.Id));
    }

    [Fact]
    public void Shuffle_RejectsNullItems()
    {
        var act = () => QueueShuffler.Shuffle(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
