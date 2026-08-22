namespace Dhun.Core.Sources;

/// <summary>
/// Identity and capability declaration for a music source.
/// Capability-specific interfaces should be used for operations.
/// </summary>
public interface IMusicSource
{
    string Id { get; }
    string DisplayName { get; }
    MusicSourceKind Kind { get; }
    MusicSourceCapabilities Capabilities { get; }
}
