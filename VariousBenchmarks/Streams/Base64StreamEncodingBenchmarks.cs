using Various.Streams;

namespace VariousBenchmarks.Streams
{
    [MemoryDiagnoser]
    [SimpleJob(iterationCount: 5)]
    public class Base64StreamEncodingBenchmarks
    {
        private Stream source = null!;

        [Params(1024, 64 * 1024, 1024 * 1024)]
        public int Size { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            source = new MemoryStreamInDisguise();
            var bytes = new byte[Size];
            Random.Shared.NextBytes(bytes);
            source.Write(bytes);
        }

        [GlobalCleanup]
        public void Cleanup() => source.Dispose();

        [Benchmark(Baseline = true)]
        public void RegularConvertToBase64()
        {
            source.Position = 0;

            using var ms = new MemoryStream();
            source.CopyTo(ms);
            if (!ms.TryGetBuffer(out var buffer))
            {
                throw new InvalidCastException("failed to get buffer");
            }

            var base64 = Convert.ToBase64String(buffer);

            using var writer = new StreamWriter(Stream.Null);
            writer.Write(base64);
        }

        [Benchmark]
        public void UsingBase64Stream_1024()
        {
            source.Position = 0;

            using var base64Stream = Base64Stream.CreateForEncoding(Stream.Null);
            source.CopyTo(base64Stream, 1024);
        }

        [Benchmark]
        public void UsingBase64Stream_4096()
        {
            source.Position = 0;

            using var base64Stream = Base64Stream.CreateForEncoding(Stream.Null);
            source.CopyTo(base64Stream, 4096);
        }

        [Benchmark]
        public void UsingBase64Stream_81920()
        {
            source.Position = 0;

            using var base64Stream = Base64Stream.CreateForEncoding(Stream.Null);
            source.CopyTo(base64Stream, 81920);
        }
    }
}
