using System.Security.Cryptography;

namespace Dhun.Core.Sources.Local;

/// <summary>
/// Computes a stable fingerprint for a local audio file.
/// The fingerprint is content-based, so moving or renaming a file does not change its identity.
/// </summary>
/// <remarks>
/// The fingerprint is deliberately separate from the database <c>Song.Id</c>. It is a reconciliation
/// key that can be used by the library scanner to reconnect an existing song after a move/rename.
/// Identical byte-for-byte files intentionally produce the same fingerprint and should therefore be
/// handled by the library's duplicate policy rather than being treated as different content.
/// </remarks>
public static class LocalTrackIdentity
{
    public const int HashBufferSize = 1024 * 128;

    public static async Task<string> ComputeAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            HashBufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return await ComputeAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<string> ComputeAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (!content.CanRead)
            throw new ArgumentException("The stream must be readable.", nameof(content));

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[HashBufferSize];

        while (true)
        {
            var bytesRead = await content.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
                break;

            hash.AppendData(buffer, 0, bytesRead);
        }

        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }
}
