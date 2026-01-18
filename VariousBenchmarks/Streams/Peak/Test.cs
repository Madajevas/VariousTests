using System.Buffers;
using System.Drawing;

int size = int.Parse(args[0]);

//foreach (var arg in args)
//{
//    Console.Write(arg);
//    Console.Write(' ');
//}
//Console.WriteLine();

var source = new MemoryStream();
var bytes = new byte[size];
Random.Shared.NextBytes(bytes);
source.Write(bytes);
source.Position = 0;

if (args.Contains("dry"))
{
    return;
}

using var ms = new MemoryStream();
source.CopyTo(ms);
if (!ms.TryGetBuffer(out var buffer))
{
    throw new InvalidCastException("failed to get buffer");
}

var base64 = Convert.ToBase64String(buffer);

using var writer = new StreamWriter(Stream.Null);
writer.Write(base64);

// Console.WriteLine(base64);


file sealed class EncodingStream(Stream output) : NothingSupportedStream
{
    private readonly StreamWriter writer = new StreamWriter(output);

    private byte[] remainder = new byte[2];
    private int remainderCount;

    public override bool CanWrite => true;

    public override void Write(byte[] buffer, int offset, int count)
    {
        var bytesToTake = ((remainderCount + count) / 3) * 3;
        if (bytesToTake > 0)
        {
            var chunk = ArrayPool<byte>.Shared.Rent(bytesToTake);

            remainder.AsSpan(0, remainderCount).CopyTo(chunk);
            buffer.AsSpan(offset, bytesToTake - remainderCount).CopyTo(chunk.AsSpan(remainderCount));
            writer.Write(Convert.ToBase64String(chunk, 0, bytesToTake));

            ArrayPool<byte>.Shared.Return(chunk);

            remainderCount = count - bytesToTake + remainderCount;
            buffer.AsSpan(offset + count - remainderCount, remainderCount).CopyTo(remainder);
        }
        else
        {
            // this branch will execute only when bytesToTake <= 2 so remainder array will never overflow
            buffer.AsSpan(offset, count).CopyTo(remainder.AsSpan(remainderCount));
            remainderCount += count;
        }
    }

    public override void Flush()
    {
        writer.Write(Convert.ToBase64String(remainder.AsSpan(0, remainderCount)));
        writer.Flush();
    }

    protected override void Dispose(bool disposing) => writer.Dispose();

    public override bool CanRead => false;

    public override bool CanSeek => false;
}

file abstract class NothingSupportedStream : Stream
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

