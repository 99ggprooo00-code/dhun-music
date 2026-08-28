namespace Dhun.Core.Sources.Local;

/// <summary>
/// Describes the known state of a local track without requiring a database entity.
/// </summary>
public sealed record LocalTrackSnapshot(
    string Identity,
    string Path,
    long Length,
    DateTimeOffset LastWriteTimeUtc);

public enum LocalTrackReconciliationAction
{
    Add,
    UpdatePath,
    UpdateMetadata,
    Remove,
    NoChange
}

/// <summary>
/// A deterministic filesystem-to-library reconciliation decision.
/// The planner is intentionally side-effect free; callers own persistence and file I/O.
/// </summary>
public sealed record LocalTrackReconciliationDecision(
    LocalTrackReconciliationAction Action,
    string Identity,
    string? ExistingPath,
    string? CurrentPath);

/// <summary>
/// Plans local-library changes by stable content identity instead of filesystem path.
/// </summary>
public static class LocalTrackReconciliation
{
    public static IReadOnlyList<LocalTrackReconciliationDecision> Plan(
        IEnumerable<LocalTrackSnapshot> existing,
        IEnumerable<LocalTrackSnapshot> discovered)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(discovered);

        var existingByIdentity = existing
            .Where(IsUsable)
            .GroupBy(x => x.Identity, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.OrderBy(v => v.Path, StringComparer.OrdinalIgnoreCase).First(), StringComparer.Ordinal);

        var discoveredByIdentity = discovered
            .Where(IsUsable)
            .GroupBy(x => x.Identity, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.OrderBy(v => v.Path, StringComparer.OrdinalIgnoreCase).First(), StringComparer.Ordinal);

        var decisions = new List<LocalTrackReconciliationDecision>();

        foreach (var item in discoveredByIdentity.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            if (!existingByIdentity.TryGetValue(item.Key, out var old))
            {
                decisions.Add(new(LocalTrackReconciliationAction.Add, item.Key, null, item.Value.Path));
                continue;
            }

            if (!PathsEqual(old.Path, item.Value.Path))
            {
                decisions.Add(new(LocalTrackReconciliationAction.UpdatePath, item.Key, old.Path, item.Value.Path));
            }
            else if (old.Length != item.Value.Length || old.LastWriteTimeUtc != item.Value.LastWriteTimeUtc)
            {
                decisions.Add(new(LocalTrackReconciliationAction.UpdateMetadata, item.Key, old.Path, item.Value.Path));
            }
            else
            {
                decisions.Add(new(LocalTrackReconciliationAction.NoChange, item.Key, old.Path, item.Value.Path));
            }
        }

        foreach (var item in existingByIdentity.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            if (!discoveredByIdentity.ContainsKey(item.Key))
            {
                decisions.Add(new(LocalTrackReconciliationAction.Remove, item.Key, item.Value.Path, null));
            }
        }

        return decisions;
    }

    private static bool IsUsable(LocalTrackSnapshot snapshot) =>
        !string.IsNullOrWhiteSpace(snapshot.Identity) && !string.IsNullOrWhiteSpace(snapshot.Path);

    private static bool PathsEqual(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
