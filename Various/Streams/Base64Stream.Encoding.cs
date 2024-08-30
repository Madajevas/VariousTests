namespace Various.Streams
{
    public partial class Base64Stream
    {
        sealed class EncodingStream : NothingSupportedStream
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
        }
    }
}
