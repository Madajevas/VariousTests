using System.Buffers;
using System.Buffers.Text;

namespace Various.Streams
{
    public partial class Base64Stream
    {
        sealed class EncodingStream : NothingSupportedStream
        {
            private readonly StreamWriter writer;
            private ArrayBufferWriter<byte> buffered;

            public EncodingStream(Stream output)
            {
                this.writer = new StreamWriter(output);
                this.buffered = new ArrayBufferWriter<byte>();
            }

            public override bool CanWrite => true;

            public override void Write(byte[] buffer, int offset, int count)
            {
                buffered.Write(buffer.AsSpan(offset, count));
                var maxLength = (buffered.WrittenCount / 3) * 3;

                if (maxLength == 0)
                {
                    return;
                }

                var chunk = ArrayPool<byte>.Shared.Rent(maxLength);
                try
                {
                    buffered.WrittenSpan.Slice(0, maxLength).CopyTo(chunk);
                    writer.Write(Convert.ToBase64String(chunk, 0, maxLength));
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(chunk);
                }

                var unreadBytes = buffered.WrittenCount - maxLength;
                if (unreadBytes > 0)
                {
                    Span<byte> remainder = stackalloc byte[unreadBytes];
                    buffered.WrittenSpan.Slice(maxLength, unreadBytes).CopyTo(remainder);
                    buffered.Clear();
                    buffered.Write(remainder);
                    Console.WriteLine();
                }
                else
                {
                    buffered.Clear();
                }
            }

            public override void Flush()
            {
                writer.Write(Convert.ToBase64String(buffered.WrittenSpan));
                writer.Flush();
            }

            protected override void Dispose(bool disposing) => writer.Dispose();

            public override bool CanRead => false;

            public override bool CanSeek => false;
        }
    }
}
