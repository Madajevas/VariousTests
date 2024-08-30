namespace Various.Streams
{
    // just to have less code..
    public abstract class NothingSupportedStream : Stream
    {
        public override bool CanRead => throw new NotSupportedException();

        public override bool CanSeek => throw new NotSupportedException();

        public override bool CanWrite => throw new NotSupportedException();

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
