using Microsoft.Identity.Client;

using System.Buffers;
using System.Diagnostics;

namespace Various.Streams
{
    public partial class Base64Stream
    {
        sealed class DecodingStream : NothingSupportedStream
        {
            private readonly StreamReader reader;

            public DecodingStream(Stream source, bool leaveOpen)
            {
                this.reader = new StreamReader(source, leaveOpen: leaveOpen);
            }

            public override bool CanRead => true;

            public override int Read(byte[] buffer, int offset, int count)
            {
                var approxBytesToTake = count * 4 / 3;
                var charsToTake = approxBytesToTake / sizeof(char);
                var chunk = ArrayPool<char>.Shared.Rent(charsToTake);
                var decoded = ArrayPool<byte>.Shared.Rent(count);

                var read = reader.Read(chunk, 0, chunk.Length);
                var status = Convert.TryFromBase64Chars(chunk.AsSpan(0, read), decoded, out var bytesWritten);
                Debug.Assert(status);
                decoded.AsSpan(0, bytesWritten).CopyTo(buffer);

                ArrayPool<byte>.Shared.Return(decoded);
                ArrayPool<char>.Shared.Return(chunk);

                return bytesWritten;
            }

            protected override void Dispose(bool disposing) => reader.Dispose();

            public override bool CanWrite => false;

            public override bool CanSeek => false;
        }
    }
}
