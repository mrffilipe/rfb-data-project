using System.Text;
using RFBDataProject.Domain.Repositories;
using RFBDataProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace RFBDataProject.Infrastructure.Ingestion;

public sealed class CnpjBulkCopyService : ICnpjBulkLoader
{
    private static readonly Encoding Latin1 = Encoding.GetEncoding("ISO-8859-1");
    private readonly ApplicationDbContext _context;

    public CnpjBulkCopyService(ApplicationDbContext context) => _context = context;

    public async Task LoadCsvStreamAsync(string targetTable, Stream csvStream, CancellationToken ct = default)
    {
        var columns = RfbTableColumns.GetColumns(targetTable);
        var columnList = string.Join(", ", columns.Select(c => $"\"{c}\""));

        await using var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync(ct);

        await using var writer = await conn.BeginTextImportAsync(
            $"COPY \"{targetTable}\" ({columnList}) FROM STDIN WITH (FORMAT csv, DELIMITER ';', QUOTE '\"', NULL '')",
            ct);

        await using var sanitized = new Latin1SanitizingStream(csvStream);
        using var reader = new StreamReader(sanitized, Latin1, detectEncodingFromByteOrderMarks: false);

        var buffer = new char[81920];
        int read;
        while ((read = await reader.ReadAsync(buffer, ct)) > 0)
            await writer.WriteAsync(buffer.AsMemory(0, read), ct);
    }

    private sealed class Latin1SanitizingStream : Stream
    {
        private readonly Stream _source;
        private readonly byte[] _readBuffer = new byte[8 * 1024 * 1024];
        private readonly Queue<byte> _pending = new();

        public Latin1SanitizingStream(Stream source) => _source = source;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var filled = 0;
            while (filled < count && _pending.Count > 0)
            {
                buffer[offset + filled] = _pending.Dequeue();
                filled++;
            }

            if (filled == count)
                return filled;

            var read = _source.Read(_readBuffer, 0, Math.Min(_readBuffer.Length, Math.Max(4096, count - filled)));
            if (read == 0 && filled == 0)
                return 0;

            for (var i = 0; i < read; i++)
            {
                var b = _readBuffer[i];
                if (b == 0)
                    continue;

                if (filled < count)
                    buffer[offset + filled++] = b;
                else
                    _pending.Enqueue(b);
            }

            return filled;
        }
    }
}
