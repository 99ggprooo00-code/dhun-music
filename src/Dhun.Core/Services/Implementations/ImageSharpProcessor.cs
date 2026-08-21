using System.Collections.Concurrent;
using System.Security.Cryptography;
using MaterialColorUtilities.Palettes;
using MaterialColorUtilities.Schemes;
using MaterialColorUtilities.Utils;
using Microsoft.Extensions.Logging;
using Dhun.Core.Helpers;
using Dhun.Core.Services.Abstractions;
using SkiaSharp;

namespace Dhun.Core.Services.Implementations;

/// <summary>
/// GPL-compatible image processor backed by MIT-licensed SkiaSharp.
/// The class name is retained temporarily to avoid a broad DI migration during foundation validation.
/// </summary>
public class ImageSharpProcessor : IImageProcessor
{
    private const int CachedImageMaxDimension = 600;
    private const int ColorExtractionDimension = 112;
    private readonly string _albumArtStoragePath;
    private readonly IFileSystemService _fileSystem;
    private readonly ILogger<ImageSharpProcessor> _logger;
    private readonly ConcurrentDictionary<string, Lazy<Task<string>>> _inFlightSaves = new();

    public ImageSharpProcessor(IPathConfiguration pathConfig, IFileSystemService fileSystem, ILogger<ImageSharpProcessor> logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger;
        _albumArtStoragePath = pathConfig.AlbumArtCachePath;
        try { _fileSystem.CreateDirectory(_albumArtStoragePath); }
        catch (Exception ex) { _logger.LogCritical(ex, "Failed to create Album Art directory at '{AlbumArtPath}'.", _albumArtStoragePath); }
    }

    public async Task<(string? uri, string? lightSwatchId, string? darkSwatchId)> SaveCoverArtAndExtractColorsAsync(byte[] pictureData)
    {
        if (pictureData.Length == 0) return (null, null, null);
        try
        {
            var contentHash = Convert.ToHexString(SHA256.HashData(pictureData)).ToLowerInvariant();
            var existing = FindCachedFileByHash(contentHash);
            if (existing != null)
            {
                var colors = ParseColorsFromFilename(existing);
                return (existing, colors.lightHex, colors.darkHex);
            }
            var lazy = _inFlightSaves.GetOrAdd(contentHash, _ => new Lazy<Task<string>>(() => ProcessAndSaveAsync(contentHash, pictureData)));
            try
            {
                var path = await lazy.Value.ConfigureAwait(false);
                var colors = ParseColorsFromFilename(path);
                return (path, colors.lightHex, colors.darkHex);
            }
            finally { _inFlightSaves.TryRemove(contentHash, out _); }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save cover art and extract colors.");
            return (null, null, null);
        }
    }

    private string? FindCachedFileByHash(string hash)
    {
        try { return _fileSystem.GetFiles(_albumArtStoragePath, $"{hash}.*.fetched.jpg").FirstOrDefault(); }
        catch (Exception ex) { _logger.LogDebug(ex, "Error searching for cached file with hash {Hash}", hash); return null; }
    }

    private static (string? lightHex, string? darkHex) ParseColorsFromFilename(string filePath)
    {
        try
        {
            var filename = Path.GetFileNameWithoutExtension(filePath);
            if (filename.EndsWith(".fetched", StringComparison.OrdinalIgnoreCase)) filename = filename[..^8];
            var parts = filename.Split('.');
            if (parts.Length >= 3) return (parts[^2], parts[^1]);
        }
        catch { }
        return (null, null);
    }

    private async Task<string> ProcessAndSaveAsync(string hash, byte[] data)
    {
        var existing = FindCachedFileByHash(hash);
        if (existing != null) return existing;
        using var source = SKBitmap.Decode(data) ?? throw new InvalidDataException("Unsupported image data");
        using var cached = ResizeToFit(source, CachedImageMaxDimension);
        var (light, dark) = ExtractColors(cached);
        var filename = $"{hash}.{light ?? "000000"}.{dark ?? "000000"}.fetched.jpg";
        var fullPath = _fileSystem.Combine(_albumArtStoragePath, filename);
        using var image = SKImage.FromBitmap(cached);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        var tempPath = fullPath + ".tmp";
        await _fileSystem.WriteAllBytesAsync(tempPath, encoded.ToArray()).ConfigureAwait(false);
        try { _fileSystem.MoveFile(tempPath, fullPath, false); }
        catch (IOException)
        {
            try { _fileSystem.DeleteFile(tempPath); }
            catch (Exception ex) { _logger.LogDebug(ex, "Failed to clean up temp file {TempPath}", tempPath); }
        }
        return fullPath;
    }

    private (string? lightHex, string? darkHex) ExtractColors(SKBitmap source)
    {
        try
        {
            using var sample = ResizeToFit(source, ColorExtractionDimension);
            var pixels = new uint[sample.Width * sample.Height];
            var index = 0;
            for (var y = 0; y < sample.Height; y++)
            for (var x = 0; x < sample.Width; x++)
            {
                var color = sample.GetPixel(x, y);
                pixels[index++] = ((uint)color.Alpha << 24) | ((uint)color.Red << 16) | ((uint)color.Green << 8) | color.Blue;
            }
            var seed = ImageUtils.ColorsFromImage(pixels).FirstOrDefault();
            if (seed == default) return (null, null);
            var palette = CorePalette.ContentOf(seed);
            var light = new LightSchemeMapper().Map(palette);
            var dark = new DarkSchemeMapper().Map(palette);
            return ((light.Primary & 0x00FFFFFF).ToString("x6"), (dark.Primary & 0x00FFFFFF).ToString("x6"));
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to extract colors from album art."); return (null, null); }
    }

    public Task<byte[]> ProcessImageBytesAsync(byte[] imageData, int maxDimension = 600)
    {
        if (imageData.Length == 0 || maxDimension <= 0) return Task.FromResult(imageData);
        try
        {
            using var source = SKBitmap.Decode(imageData);
            if (source == null || (source.Width <= maxDimension && source.Height <= maxDimension)) return Task.FromResult(imageData);
            using var resized = ResizeToFit(source, maxDimension);
            using var image = SKImage.FromBitmap(resized);
            using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            return Task.FromResult(encoded.ToArray());
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to process image, returning original bytes."); return Task.FromResult(imageData); }
    }

    private static SKBitmap ResizeToFit(SKBitmap source, int maxDimension)
    {
        var scale = Math.Min(1d, (double)maxDimension / Math.Max(source.Width, source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));
        if (width == source.Width && height == source.Height) return source.Copy();
        return source.Resize(new SKImageInfo(width, height), SKFilterQuality.High) ?? throw new InvalidOperationException("Image resize failed");
    }
}
