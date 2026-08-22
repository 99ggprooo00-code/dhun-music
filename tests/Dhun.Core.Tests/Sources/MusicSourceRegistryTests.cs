using Dhun.Core.Sources;
using FluentAssertions;
using NSubstitute;

namespace Dhun.Core.Tests.Sources;

public sealed class MusicSourceRegistryTests
{
    [Fact]
    public void Register_ShouldExposeSourceById()
    {
        var source = Substitute.For<IMusicSource>();
        source.Id.Returns("local");

        var registry = new MusicSourceRegistry();
        registry.Register(source);

        registry.GetRequired("LOCAL").Should().BeSameAs(source);
        registry.Sources.Should().ContainSingle().Which.Should().BeSameAs(source);
    }

    [Fact]
    public void Register_ShouldRejectDuplicateIds_IgnoringCase()
    {
        var first = Substitute.For<IMusicSource>();
        first.Id.Returns("local");
        var second = Substitute.For<IMusicSource>();
        second.Id.Returns("LOCAL");

        var registry = new MusicSourceRegistry();
        registry.Register(first);

        var act = () => registry.Register(second);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*local*");
    }

    [Fact]
    public void Register_ShouldRejectBlankId()
    {
        var source = Substitute.For<IMusicSource>();
        source.Id.Returns(" ");

        var act = () => new MusicSourceRegistry().Register(source);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryGet_ShouldReturnFalseForUnknownSource()
    {
        var registry = new MusicSourceRegistry();

        registry.TryGet("missing", out var source).Should().BeFalse();
        source.Should().BeNull();
    }

    [Fact]
    public void Remove_ShouldOnlyRemoveExistingSource()
    {
        var source = Substitute.For<IMusicSource>();
        source.Id.Returns("local");
        var registry = new MusicSourceRegistry();
        registry.Register(source);

        registry.Remove("LOCAL").Should().BeTrue();
        registry.Remove("LOCAL").Should().BeFalse();
        registry.Sources.Should().BeEmpty();
    }
}
