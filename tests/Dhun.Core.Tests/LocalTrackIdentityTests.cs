using System.Text;
using Dhun.Core.Sources.Local;

namespace Dhun.Core.Tests;

public sealed class LocalTrackIdentityTests
{
    [Fact]
    public async Task SameContent_ProducesSameFingerprint()
    {
        await using var first = new MemoryStream(Encoding.UTF8.GetBytes("dhun-test-audio"));
        await using var second = new MemoryStream(Encoding.UTF8.GetBytes("dhun-test-audio"));

        var firstFingerprint = await LocalTrackIdentity.ComputeAsync(first);
        var secondFingerprint = await LocalTrackIdentity.ComputeAsync(second);

        Assert.Equal(firstFingerprint, secondFingerprint);
        Assert.Equal(64, firstFingerprint.Length);
        Assert.All(firstFingerprint, c => Assert.True(Uri.IsHexDigit(c)));
    }

    [Fact]
    public async Task DifferentContent_ProducesDifferentFingerprint()
    {
        await using var first = new MemoryStream(Encoding.UTF8.GetBytes("dhun-test-audio-a"));
        await using var second = new MemoryStream(Encoding.UTF8.GetBytes("dhun-test-audio-b"));

        var firstFingerprint = await LocalTrackIdentity.ComputeAsync(first);
        var secondFingerprint = await LocalTrackIdentity.ComputeAsync(second);

        Assert.NotEqual(firstFingerprint, secondFingerprint);
    }

    [Fact]
    public async Task FilePathOverload_ProducesContentFingerprint()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"dhun-identity-{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllTextAsync(filePath, "dhun-test-audio");

            var fingerprint = await LocalTrackIdentity.ComputeAsync(filePath);

            await using var expectedContent = new MemoryStream(Encoding.UTF8.GetBytes("dhun-test-audio"));
            var expected = await LocalTrackIdentity.ComputeAsync(expectedContent);
            Assert.Equal(expected, fingerprint);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ComputeAsync_ObservesCancellation()
    {
        await using var stream = new SlowStream(Encoding.UTF8.GetBytes(new string('x', 1024)));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            LocalTrackIdentity.ComputeAsync(stream, cancellation.Token));
    }

    [Fact]
    public async Task ComputeAsync_RejectsUnreadableStream()
    {
        await using var stream = new WriteOnlyStream();

        await Assert.ThrowsAsync<ArgumentException>(() => LocalTrackIdentity.ComputeAsync(stream));
    }

    private sealed class SlowStream(byte[] data) : MemoryStream(data)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return base.ReadAsync(buffer, cancellationToken);
        }
    }

    private sealed class WriteOnlyStream : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get; set; }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) { }
    }
}
