namespace Dhun.Core.Sources.Local;

/// <summary>
/// Semantic change observed in a local music library. This model intentionally does not expose
/// FileSystemWatcher so the rest of the library can remain independent of the Windows event API.
/// </summary>
public enum LocalFileChangeKind
{
    Added,
    Modified,
    Removed,
    Moved
}

/// <summary>
/// A normalized local-file change ready for debounce/coalescing and later reconciliation.
/// <paramref name="Path"/> is the current path; <paramref name="OldPath"/> is populated only for moves.
/// </summary>
public sealed record LocalFileChange(
    LocalFileChangeKind Kind,
    string Path,
    string? OldPath = null);

/// <summary>
/// Raw filesystem notification data supplied by a watcher adapter.
/// </summary>
public readonly record struct LocalFileSystemEvent(
    WatcherChangeTypes ChangeType,
    string? FullPath,
    string? OldFullPath = null,
    bool IsDirectory = false);

/// <summary>
/// Converts raw filesystem notifications into stable, music-file-oriented events.
/// Invalid or directory notifications are ignored rather than allowed to poison the scan queue.
/// </summary>
public static class LocalFileSystemEventNormalizer
{
    public static LocalFileChange? Normalize(LocalFileSystemEvent notification)
    {
        if (notification.IsDirectory || string.IsNullOrWhiteSpace(notification.FullPath))
            return null;

        var path = notification.FullPath.Trim();

        if ((notification.ChangeType & WatcherChangeTypes.Renamed) != 0)
        {
            if (string.IsNullOrWhiteSpace(notification.OldFullPath))
                return null;

            var oldPath = notification.OldFullPath.Trim();
            if (string.Equals(oldPath, path, StringComparison.OrdinalIgnoreCase))
                return null;

            return new LocalFileChange(LocalFileChangeKind.Moved, path, oldPath);
        }

        if ((notification.ChangeType & WatcherChangeTypes.Deleted) != 0)
            return new LocalFileChange(LocalFileChangeKind.Removed, path);

        if ((notification.ChangeType & WatcherChangeTypes.Created) != 0)
            return new LocalFileChange(LocalFileChangeKind.Added, path);

        if ((notification.ChangeType & WatcherChangeTypes.Changed) != 0)
            return new LocalFileChange(LocalFileChangeKind.Modified, path);

        return null;
    }
}
