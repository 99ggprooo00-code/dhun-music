using Dhun.Core.Sources.Local;

namespace Dhun.Core.Tests;

public sealed class LocalTrackReconciliationTests
{
    [Fact]
    public void SameIdentityDifferentPath_IsUpdatePath()
    {
        var existing = Snapshot("abc", @"C:\Music\old.flac", 100, 1);
        var discovered = Snapshot("abc", @"C:\Music\new.flac", 100, 1);

        var result = LocalTrackReconciliation.Plan([existing], [discovered]);

        var decision = Assert.Single(result);
        Assert.Equal(LocalTrackReconciliationAction.UpdatePath, decision.Action);
        Assert.Equal(existing.Path, decision.ExistingPath);
        Assert.Equal(discovered.Path, decision.CurrentPath);
    }

    [Fact]
    public void SameIdentitySamePathUnchanged_IsNoChange()
    {
        var existing = Snapshot("abc", @"C:\Music\song.flac", 100, 1);

        var result = LocalTrackReconciliation.Plan([existing], [existing]);

        Assert.Equal(LocalTrackReconciliationAction.NoChange, Assert.Single(result).Action);
    }

    [Fact]
    public void ChangedFileMetadata_IsUpdateMetadata()
    {
        var existing = Snapshot("abc", @"C:\Music\song.flac", 100, 1);
        var discovered = Snapshot("abc", @"C:\Music\song.flac", 101, 2);

        var result = LocalTrackReconciliation.Plan([existing], [discovered]);

        Assert.Equal(LocalTrackReconciliationAction.UpdateMetadata, Assert.Single(result).Action);
    }

    [Fact]
    public void NewIdentity_IsAdd()
    {
        var result = LocalTrackReconciliation.Plan([], [Snapshot("new", @"C:\Music\new.mp3", 10, 1)]);

        var decision = Assert.Single(result);
        Assert.Equal(LocalTrackReconciliationAction.Add, decision.Action);
        Assert.Null(decision.ExistingPath);
    }

    [Fact]
    public void MissingIdentity_IsRemove()
    {
        var result = LocalTrackReconciliation.Plan([Snapshot("old", @"C:\Music\old.mp3", 10, 1)], []);

        var decision = Assert.Single(result);
        Assert.Equal(LocalTrackReconciliationAction.Remove, decision.Action);
        Assert.Null(decision.CurrentPath);
    }

    [Fact]
    public void DuplicateIdentity_UsesDeterministicPath()
    {
        var discovered = new[]
        {
            Snapshot("same", @"C:\Music\z.mp3", 10, 1),
            Snapshot("same", @"C:\Music\a.mp3", 10, 1)
        };

        var result = LocalTrackReconciliation.Plan([], discovered);

        var decision = Assert.Single(result);
        Assert.Equal(LocalTrackReconciliationAction.Add, decision.Action);
        Assert.Equal(@"C:\Music\a.mp3", decision.CurrentPath);
    }

    [Fact]
    public void InvalidSnapshots_AreIgnored()
    {
        var result = LocalTrackReconciliation.Plan(
            [Snapshot("", @"C:\Music\bad.mp3", 10, 1)],
            [Snapshot("valid", @"C:\Music\ok.mp3", 10, 1)]);

        var decisions = result.ToArray();
        Assert.Single(decisions);
        Assert.Contains(decisions, x => x.Action == LocalTrackReconciliationAction.Add);
    }

    private static LocalTrackSnapshot Snapshot(string identity, string path, long length, long seconds) =>
        new(identity, path, length, DateTimeOffset.UnixEpoch.AddSeconds(seconds));
}
