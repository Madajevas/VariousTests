using Microsoft.Identity.Client;

using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;

namespace Various.Streams
{
    public partial class Base64Stream
    {
        sealed class EncodingStream(Stream output) : NothingSupportedStream
        {
            private byte[] remainder = new byte[2];
            private int remainderCount;

            public override bool CanWrite => true;

            public override void Write(byte[] buffer, int offset, int count)
            {
                var bytesToTake = ((remainderCount + count) / 3) * 3;
                if (bytesToTake > 0)
                {
                    var chunk = ArrayPool<byte>.Shared.Rent(bytesToTake);
                    var encodedBytes = (int)(count * 1.5);
                    var encoded = ArrayPool<byte>.Shared.Rent(encodedBytes);

                    remainder.AsSpan(0, remainderCount).CopyTo(chunk);
                    buffer.AsSpan(offset, bytesToTake - remainderCount).CopyTo(chunk.AsSpan(remainderCount));

                    var status = Base64.EncodeToUtf8(
                        bytes: chunk.AsSpan(0, bytesToTake),
                        utf8: encoded.AsSpan(0, encodedBytes),
                        out var _,
                        out var bytesWritten,
                        isFinalBlock: false);
                    Debug.Assert(status == OperationStatus.Done);
                    output.Write(encoded, 0, bytesWritten);

                    ArrayPool<byte>.Shared.Return(encoded);
                    ArrayPool<byte>.Shared.Return(chunk);

                    remainderCount = count - bytesToTake + remainderCount;
                    buffer.AsSpan(offset + count - remainderCount, remainderCount).CopyTo(remainder);
                }
                else
                {
                    // this branch will execute only when bytesToTake <= 2 so remainder array will never overflow
                    buffer.AsSpan(offset, count).CopyTo(remainder.AsSpan(remainderCount));
                    remainderCount += count;
                }
            }

            public override void Flush()
            {
                Span<byte> encoded = stackalloc byte[4];
                var status = Base64.EncodeToUtf8(
                    bytes: remainder.AsSpan(0, remainderCount),
                    utf8: encoded,
                    out var _,
                    out var bytesWritten,
                    isFinalBlock: true);
                output.Write(encoded.Slice(0, bytesWritten));

                output.Flush();
            }

            protected override void Dispose(bool disposing) => output.Dispose();

            public override bool CanRead => false;

            public override bool CanSeek => false;
        }
    }
}
