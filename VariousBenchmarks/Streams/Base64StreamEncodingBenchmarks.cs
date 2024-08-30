using System.Text;

using Various.Streams;

namespace VariousBenchmarks.Streams
{
    [MemoryDiagnoser]
    [SimpleJob(iterationCount: 10)]
    public class Base64StreamEncodingBenchmarks
    {
        private Stream source = null!;

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

        [Benchmark(Baseline = true)]
        public void RegularConvertToBase64()
        {
            source.Position = 0;

            using var ms = new MemoryStream();
            source.CopyTo(ms);

            var base64 = Convert.ToBase64String(ms.ToArray());

            using var writer = new StreamWriter(Stream.Null);
            writer.Write(base64);
        }

        [Benchmark]
        public void UsingBase64Stream()
        {
            source.Position = 0;

            using var base64Stream = Base64Stream.CreateForEncoding(Stream.Null);
            source.CopyTo(base64Stream);
        }
    }
}
