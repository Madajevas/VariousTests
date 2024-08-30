using System.Buffers;

namespace Various.Streams
{
    public partial class Base64Stream
    {
        sealed class DecodingStream : NothingSupportedStream
        {
            private readonly StreamReader reader;
            private readonly Queue<byte> buffered;

            public DecodingStream(Stream source, bool leaveOpen)
            {
                this.reader = new StreamReader(source, leaveOpen: leaveOpen);
                this.buffered = new Queue<byte>();
            }

            public override bool CanRead => true;

            public override int Read(byte[] buffer, int offset, int count)
            {
                var internalBuffer = ArrayPool<char>.Shared.Rent(4 * 512);
                while (buffered.Count < count)
                {
                    var read = reader.Read(internalBuffer, 0, internalBuffer.Length);
                    foreach (var decodedByte in Convert.FromBase64CharArray(internalBuffer, 0, read))
                    {
                        buffered.Enqueue(decodedByte);
                    }

                    if (read < internalBuffer.Length)
                    {
                        break;
                    }
                }
                ArrayPool<char>.Shared.Return(internalBuffer);

                int i = 0;
                while (buffered.Count > 0 && i < count)
                {
                    buffer[i + offset] = buffered.Dequeue();
                    i++;
                }

                return i;
            }

            protected override void Dispose(bool disposing) => reader.Dispose();

            public override bool CanWrite => false;

            public override bool CanSeek => false;
        }
    }
}
