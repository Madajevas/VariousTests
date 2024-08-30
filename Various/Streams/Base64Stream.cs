using System.Buffers;

namespace Various.Streams
{
    public sealed partial class Base64Stream : NothingSupportedStream
    {
        private Stream next;

        public static Base64Stream CreateForEncoding(Stream output)
        {
            var encodingStream = new EncodingStream(output);
            var bufferedStream = new BufferedStream(encodingStream, bufferSize: 3 * 1024);

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

        public override bool CanRead => next.CanRead;
        public override int Read(byte[] buffer, int offset, int count) => next.Read(buffer, offset, count);


        public override bool CanWrite => next.CanWrite;
        public override void Write(byte[] buffer, int offset, int count) => next.Write(buffer, offset, count);
        public override void Flush() => next.Flush();

        public override bool CanSeek => false;

        protected override void Dispose(bool disposing) => next.Dispose();
    }
}
