using BenchmarkDotNet.Engines;

using System.Collections;

using Various.Collections.Composite.CompositeEnumerator;

namespace VariousBenchmarks.Collections.Composite.CompositeEnumerator
{
    [MemoryDiagnoser]
    public class CompositeEnumeratorBenchmarks
    {
        private static int[] collection1 = new[] { 1, 2, 4 };
        private static int[] collection2 = new[] { 5, 6, 7 };

        private readonly Consumer consumer = new Consumer();

        [Benchmark]
        public IEnumerator CreateCompositeEnumerator() => new CompositeEnumerator<int>(collection1, collection2);

        [Benchmark]
        public IEnumerator CreateConcatEnumerator() => collection1.Concat(collection2).GetEnumerator();

        [Benchmark]
        public void IterateCompositeEnumerator()
        {
            var enumerator = CreateCompositeEnumerator();

            while (enumerator.MoveNext())
            {
                consumer.Consume(enumerator.Current);
            }
        }

        [Benchmark]
        public void IterateConcatEnumerator()
        {
            var enumerator = CreateConcatEnumerator();
            while (enumerator.MoveNext())
            {
                consumer.Consume(enumerator.Current);
            }
        }
    }
}
