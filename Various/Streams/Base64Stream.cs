using System.Diagnostics;

namespace Various.Streams
{
    public sealed class EncodingStream : Stream
    {
        private readonly Stream output;
        private readonly StreamWriter writer;

        public EncodingStream(Stream output)
        {
            this.output = output;
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

        private Base64Stream(Stream next)
        {
            this.next = next;
        }

        public override bool CanRead => throw new NotSupportedException();

        public override bool CanSeek => throw new NotSupportedException();

        public override bool CanWrite => next is BufferedStream;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() => next.Flush();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => next.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            next.Flush();
            next.Dispose();
        }
    }
}
