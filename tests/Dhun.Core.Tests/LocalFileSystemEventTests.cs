using System.IO;
using Dhun.Core.Sources.Local;

namespace Dhun.Core.Tests;

public sealed class LocalFileSystemEventTests
{
    [Fact]
    public void CreatedFile_IsAdded()
    {
        var result = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(WatcherChangeTypes.Created, @"C:\Music\song.flac"));

        Assert.NotNull(result);
        Assert.Equal(LocalFileChangeKind.Added, result.Kind);
        Assert.Equal(@"C:\Music\song.flac", result.Path);
        Assert.Null(result.OldPath);
    }

    [Fact]
    public void ChangedFile_IsModified()
    {
        var result = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(WatcherChangeTypes.Changed, @"C:\Music\song.flac"));

        Assert.NotNull(result);
        Assert.Equal(LocalFileChangeKind.Modified, result.Kind);
    }

    [Fact]
    public void DeletedFile_IsRemoved()
    {
        var result = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(WatcherChangeTypes.Deleted, @"C:\Music\song.flac"));

        Assert.NotNull(result);
        Assert.Equal(LocalFileChangeKind.Removed, result.Kind);
    }

    [Fact]
    public void RenamedFile_IsMoved()
    {
        var result = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(
                WatcherChangeTypes.Renamed,
                @"C:\Music\new.flac",
                @"C:\Music\old.flac"));

        Assert.NotNull(result);
        Assert.Equal(LocalFileChangeKind.Moved, result.Kind);
        Assert.Equal(@"C:\Music\new.flac", result.Path);
        Assert.Equal(@"C:\Music\old.flac", result.OldPath);
    }

    [Fact]
    public void RenamedWithoutOldPath_IsIgnored()
    {
        var result = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(WatcherChangeTypes.Renamed, @"C:\Music\new.flac"));

        Assert.Null(result);
    }

    [Fact]
    public void DirectoryEvents_AreIgnored()
    {
        var result = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(
                WatcherChangeTypes.Created,
                @"C:\Music\Album",
                IsDirectory: true));

        Assert.Null(result);
    }

    [Fact]
    public void BlankPaths_AreIgnored()
    {
        Assert.Null(LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(WatcherChangeTypes.Changed, "   ")));
    }

    [Fact]
    public void SamePathRename_IsIgnored()
    {
        var result = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(
                WatcherChangeTypes.Renamed,
                @"C:\Music\Song.flac",
                @"c:\music\song.flac"));

        Assert.Null(result);
    }

    [Fact]
    public void CombinedFlags_UseRenameThenDeleteThenCreateThenChangePrecedence()
    {
        var renamed = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(
                WatcherChangeTypes.Renamed | WatcherChangeTypes.Changed,
                @"C:\Music\new.flac",
                @"C:\Music\old.flac"));
        Assert.NotNull(renamed);
        Assert.Equal(LocalFileChangeKind.Moved, renamed.Kind);

        var deleted = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(
                WatcherChangeTypes.Deleted | WatcherChangeTypes.Changed,
                @"C:\Music\song.flac"));
        Assert.NotNull(deleted);
        Assert.Equal(LocalFileChangeKind.Removed, deleted.Kind);

        var created = LocalFileSystemEventNormalizer.Normalize(
            new LocalFileSystemEvent(
                WatcherChangeTypes.Created | WatcherChangeTypes.Changed,
                @"C:\Music\song.flac"));
        Assert.NotNull(created);
        Assert.Equal(LocalFileChangeKind.Added, created.Kind);
    }
}
