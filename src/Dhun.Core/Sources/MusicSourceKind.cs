namespace Dhun.Core.Sources;

/// <summary>Identifies the system that owns an entity.</summary>
public enum MusicSourceKind
{
    Local = 0,
    YouTube = 1
}

[Flags]
public enum MusicSourceCapabilities
{
    None = 0,
    Search = 1 << 0,
    Catalog = 1 << 1,
    Lyrics = 1 << 2,
    Playback = 1 << 3,
    UserLibrary = 1 << 4,
    PlaylistWrite = 1 << 5
}

public enum MediaAvailability
{
    Unknown = 0,
    Available = 1,
    Unavailable = 2,
    Restricted = 3,
    Offline = 4
}
