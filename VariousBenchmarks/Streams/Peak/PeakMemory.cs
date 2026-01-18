using Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate;
using Microsoft.IO;

using Various.Streams;

namespace VariousBenchmarks.Streams.Peak
{
    internal static class PeakMemory
    {
        public static IEnumerable<long> Run(int iterations, int size)
        {
            var source = new MemoryStreamInDisguise();
            var bytes = new byte[size];
            Random.Shared.NextBytes(bytes);
            source.Write(bytes);

            for (int i = 0; i < iterations; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect(GC.MaxGeneration);

                long beforeMemory = GC.GetTotalMemory(true);


                Optimized(source);


                long afterMemory = GC.GetTotalMemory(false);
                yield return afterMemory - beforeMemory;
            }
        }

        private static void Optimized(MemoryStream source)
        {
            source.Position = 0;

            using var base64Stream = Base64Stream.CreateForEncoding(Stream.Null);
            source.CopyTo(base64Stream, 4096);
        }


        private static void Regular(MemoryStream source)
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
    }
}
