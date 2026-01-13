using System.Buffers;

namespace Various.Streams
{
    public partial class Base64Stream
    {
        sealed class EncodingStream : NothingSupportedStream
        {
            private readonly StreamWriter writer;

            private byte[] remainder = new byte[2];
            private int remainderCount;

            public EncodingStream(Stream output)
            {
                this.writer = new StreamWriter(output);
            }

            public override bool CanWrite => true;

            public override void Write(byte[] buffer, int offset, int count)
            {
                var bytesToTake = ((remainderCount + count) / 3) * 3;
                if (bytesToTake > 0)
                {
                    var chunk = ArrayPool<byte>.Shared.Rent(bytesToTake);

                    remainder.AsSpan(0, remainderCount).CopyTo(chunk);
                    buffer.AsSpan(offset, bytesToTake - remainderCount).CopyTo(chunk.AsSpan(remainderCount));
                    writer.Write(Convert.ToBase64String(chunk, 0, bytesToTake));

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
                writer.Write(Convert.ToBase64String(remainder.AsSpan(0, remainderCount)));
                writer.Flush();
            }

            protected override void Dispose(bool disposing) => writer.Dispose();

            public override bool CanRead => false;

            public override bool CanSeek => false;
        }
    }
}
