using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Various.Streams
{
    public class Base64Stream(Stream source) : Stream
    {
        public override bool CanRead => source.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => source.CanWrite;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        private byte[] remainder = new byte[2];
        private int remainderLen = 0;
        public override void Write(byte[] buffer, int offset, int count)
        {
            // also use remainder
            var amountToWrite = count - (count % 3);
            using var writer = new StreamWriter(source); // save before
            writer.Write(Convert.ToBase64String(buffer, offset, amountToWrite));

            remainderLen = count - amountToWrite;

            if (remainderLen > 0)
            {
                remainder[0] = buffer[amountToWrite];
            }
            if (remainderLen > 1)
            {
                remainder[1] = buffer[amountToWrite + 1];
            }
        }
    }
}
