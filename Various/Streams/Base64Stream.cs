using System.Buffers;

namespace Various.Streams
{
    public sealed class DecodingStream : Stream
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
                foreach(var decodedByte in Convert.FromBase64CharArray(internalBuffer, 0, read))
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

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    public sealed class EncodingStream : Stream
    {
        private readonly StreamWriter writer;

        public EncodingStream(Stream output)
        {
            this.writer = new StreamWriter(output);
        }

        public override bool CanWrite => true;

        public override void Write(byte[] buffer, int offset, int count)
        {
            writer.Write(Convert.ToBase64String(buffer, offset, count));
        }

        public override void Flush() => writer.Flush();

        protected override void Dispose(bool disposing) => writer.Dispose();

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();
    }

    public sealed class Base64Stream : Stream
    {
        private Stream next;

        public static Base64Stream CreateForEncoding(Stream output)
        {
            var encodingStream = new EncodingStream(output);
            var bufferedStream = new BufferedStream(encodingStream, 3 * 1024);

            return new Base64Stream(bufferedStream);
        }

        public static Base64Stream CreateForDecoding(Stream input, bool leaveOpen = false)
        {
            var decodingStream = new DecodingStream(input, leaveOpen);
            return new Base64Stream(decodingStream);
        }

        private Base64Stream(Stream next)
        {
            this.next = next;
        }

        public override bool CanRead => next is DecodingStream;
        public override int Read(byte[] buffer, int offset, int count) => next.Read(buffer, offset, count);


        public override bool CanWrite => next is BufferedStream;
        public override void Write(byte[] buffer, int offset, int count) => next.Write(buffer, offset, count);
        public override void Flush() => next.Flush();


        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            (next as EncodingStream)?.Flush();
            next.Dispose();
        }
    }
}
