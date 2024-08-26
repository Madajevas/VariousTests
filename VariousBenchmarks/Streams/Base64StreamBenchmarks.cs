using System.Text;

using Various.Streams;

namespace VariousBenchmarks.Streams
{
    internal class VoidStream : Stream
    {
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) { }
    }

    [MemoryDiagnoser]
    [SimpleJob(iterationCount: 10)]
    public class Base64StreamBenchmarks
    {
        private Stream source;
        private Stream target;

        [Params(1024, 64 * 1024, 1024 * 1024)]
        public int Size { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new MemoryStream();
            var bytes = new byte[Size];
            Random.Shared.NextBytes(bytes);
            source.Write(bytes);
        }

        [GlobalCleanup]
        public void Cleanup() => source.Dispose();

        [IterationSetup]
        public void IterationSetup() => target = new VoidStream();

        [Benchmark(Baseline = true)]
        public void RegularConvertToBase64()
        {
            source.Position = 0;

            using var ms = new MemoryStream();
            source.CopyTo(ms);

            var base64 = Convert.ToBase64String(ms.ToArray());

            using var writer = new StreamWriter(target);
            writer.Write(Encoding.UTF8.GetBytes(base64));
        }

        [Benchmark]
        public void UsingBase64Stream()
        {
            source.Position = 0;

            using var base64Stream = Base64Stream.CreateForEncoding(target);
            source.CopyTo(base64Stream);
        }
    }
}
