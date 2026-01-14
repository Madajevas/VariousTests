using System.Buffers;

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
                var bytesToTake = count * 4 / 3;
                var chunk = ArrayPool<char>.Shared.Rent(bytesToTake / sizeof(char));

                var read = reader.Read(chunk, 0, chunk.Length);
                var decoded = Convert.FromBase64CharArray(chunk, 0, read);
                decoded.CopyTo(buffer, 0);

                ArrayPool<char>.Shared.Return(chunk);

                return decoded.Length;
            }

            protected override void Dispose(bool disposing) => reader.Dispose();

            public override bool CanWrite => false;

            public override bool CanSeek => false;
        }
    }
}
