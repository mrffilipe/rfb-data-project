using System.Runtime.CompilerServices;
using System.Text;

namespace RFBDataProject.Infrastructure.Ingestion.Parsing;

public static class RfbCsvParser
{
    private static readonly Encoding Latin1 = Encoding.GetEncoding("ISO-8859-1");

    public static async IAsyncEnumerable<string[]> ParseAsync(
        Stream stream,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var sanitized = new Latin1SanitizingStream(stream);
        using var reader = new StreamReader(sanitized, Latin1, detectEncodingFromByteOrderMarks: false);

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);
            if (line is null)
                yield break;

            if (line.Length == 0)
                continue;

            yield return ParseLine(line);
        }
    }

    internal static string[] ParseLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ';' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        fields.Add(current.ToString());
        return fields.ToArray();
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
