using Various.Streams;

namespace VariousBenchmarks.Streams
{
    [MemoryDiagnoser]
    [SimpleJob(iterationCount: 10)]
    public class Base64StreamDecodingBenchmarks
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

            using (var writer = new StreamWriter(source, leaveOpen: true))
            {
                writer.Write(Convert.ToBase64String(bytes));
            }
        }

        [GlobalCleanup]
        public void Cleanup() => source.Dispose();

        [Benchmark(Baseline = true)]
        public void RegularConvertFromBase64()
        {
            source.Position = 0;

            using (var reader = new StreamReader(source, leaveOpen: true))
            {
                var decoded = Convert.FromBase64String(reader.ReadToEnd());
                Stream.Null.Write(decoded);
            }
        }

        [Benchmark]
        public void UsingBase64Stream()
        {
            source.Position = 0;

            using var base64Stream = Base64Stream.CreateForDecoding(source, leaveOpen: true);
            base64Stream.CopyTo(Stream.Null);
        }
    }
}
