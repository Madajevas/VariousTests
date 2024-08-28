using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Various.Streams
{
    public sealed class DecodingStream : Stream
    {
        private readonly StreamReader reader;
        private readonly Queue<byte> buffered;
        private readonly Stream source;

        public DecodingStream(Stream source)
        {
            this.reader = new StreamReader(source);
            this.buffered = new Queue<byte>();
            this.source = source;
        }

        public override bool CanRead => true;

        public override int Read(byte[] buffer, int offset, int count)
        {

            var buff = new char[40];
            while (buffered.Count < count)
            {
                var read = reader.Read(buff, 0, buff.Length);
                foreach(var b in Convert.FromBase64CharArray(buff, 0, read))
                {
                    buffered.Enqueue(b);
                }

                // var read = source.Read(buff, 0, buff.Length);
                // var a = Encoding.UTF8.GetString(buff, 0, read);
                // foreach (var b in Convert.FromBase64CharArray(a, 0, a.Length))
                // {
                //     buffered.Enqueue(b);
                // }
                //for (var j = 0; j < read; j++)
                //{
                //    buffered.Enqueue(buff[j]);
                //}

                if (read < buff.Length)
                {
                    break;
                }
            }


            int i = 0;
            while (buffered.Count > 0 && i < count)
            {
                buffer[i + offset] = buffered.Dequeue();
                i++;
            }

            return i;


            //var chars = new char[3 * 1024];
            //var read = reader.ReadBlock(chars);
            //var decoded = Convert.FromBase64CharArray(chars, 0, read);

            //decoded.CopyTo(buffer, offset);
        }

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

        public static Base64Stream CreateForDecoding(Stream input)
        {
            var decodingStream = new DecodingStream(input);
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
            next.Flush();
            next.Dispose();
        }
    }
}
