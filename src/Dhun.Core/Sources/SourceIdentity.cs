namespace Dhun.Core.Sources;

/// <summary>
/// Stable identity of an entity inside a music source. Provider IDs are kept out of UI and persistence DTOs.
/// </summary>
public readonly record struct SourceIdentity(MusicSourceKind Kind, string ExternalId)
{
    public bool IsValid => !string.IsNullOrWhiteSpace(ExternalId);

    public static SourceIdentity Local(string stableId) => new(MusicSourceKind.Local, stableId);
    public static SourceIdentity YouTube(string videoId) => new(MusicSourceKind.YouTube, videoId);

    public override string ToString() => $"{Kind}:{ExternalId}";
}
