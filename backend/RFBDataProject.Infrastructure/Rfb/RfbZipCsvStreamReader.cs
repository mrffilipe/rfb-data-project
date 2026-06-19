using System.IO.Compression;

namespace RFBDataProject.Infrastructure.Rfb;

public sealed class RfbZipCsvStreamReader
{
    public static async Task<Stream> OpenSingleCsvEntryAsync(Stream zipStream, CancellationToken ct = default)
    {
        var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
        var entry = archive.Entries.FirstOrDefault(e => !e.FullName.EndsWith('/'))
            ?? throw new InvalidOperationException("ZIP archive contains no CSV entry.");

        var entryStream = await entry.OpenAsync(ct);
        return new EntryStreamWrapper(entryStream, archive);
    }

    private sealed class EntryStreamWrapper : Stream
    {
        private readonly Stream _entry;
        private readonly ZipArchive _archive;
        private bool _disposed;

        public EntryStreamWrapper(Stream entry, ZipArchive archive)
        {
            _entry = entry;
            _archive = archive;
        }

        public override bool CanRead => _entry.CanRead;
        public override bool CanSeek => _entry.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _entry.Length;
        public override long Position { get => _entry.Position; set => _entry.Position = value; }
        public override void Flush() => _entry.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _entry.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _entry.Seek(offset, origin);
        public override void SetLength(long value) => _entry.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _entry.Dispose();
                _archive.Dispose();
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
