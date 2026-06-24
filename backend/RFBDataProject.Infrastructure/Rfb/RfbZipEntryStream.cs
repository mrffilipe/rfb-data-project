using SharpCompress.Readers;

namespace RFBDataProject.Infrastructure.Rfb;

public static class RfbZipEntryStream
{
    public static Stream OpenFirstCsvEntry(Stream zipStream)
    {
        var reader = ReaderFactory.Open(zipStream, new ReaderOptions { LeaveStreamOpen = false });
        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory)
                continue;

            var entryStream = reader.OpenEntryStream();
            return new EntryStreamWrapper(entryStream, reader);
        }

        throw new InvalidOperationException("ZIP archive contains no CSV entry.");
    }

    private sealed class EntryStreamWrapper : Stream
    {
        private readonly Stream _entry;
        private readonly IReader _reader;
        private bool _disposed;

        public EntryStreamWrapper(Stream entry, IReader reader)
        {
            _entry = entry;
            _reader = reader;
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
                _reader.Dispose();
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}
