namespace Dhun.Core.Sources;

/// <summary>
/// Resolves registered music sources by their stable source identifier.
/// The registry is deliberately small so source discovery does not leak into
/// the UI, database, or individual provider implementations.
/// </summary>
public sealed class MusicSourceRegistry
{
    private readonly Dictionary<string, IMusicSource> _sources = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<IMusicSource> Sources => _sources.Values.ToArray();

    public void Register(IMusicSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(source.Id))
        {
            throw new ArgumentException("A music source must have a non-empty identifier.", nameof(source));
        }

        if (!_sources.TryAdd(source.Id, source))
        {
            throw new InvalidOperationException($"A music source with id '{source.Id}' is already registered.");
        }
    }

    public bool TryGet(string id, out IMusicSource? source)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            source = null;
            return false;
        }

        return _sources.TryGetValue(id, out source);
    }

    public IMusicSource GetRequired(string id)
    {
        if (TryGet(id, out var source))
        {
            return source;
        }

        throw new KeyNotFoundException($"No music source with id '{id}' is registered.");
    }

    public bool Remove(string id) => !string.IsNullOrWhiteSpace(id) && _sources.Remove(id);
}
