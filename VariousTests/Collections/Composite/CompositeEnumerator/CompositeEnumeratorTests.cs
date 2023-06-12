using Various.Collections.Composite.CompositeEnumerator;

namespace VariousTests.Collections.Composite.CompositeEnumerator
{
    public class CompositeEnumeratorTests
    {
        [Test]
        public void MoveNext_ProvidedWithTwoCollections_EnumeratesBoth()
        {
            var collection1 = new[] { 1, 2, 3 };
            var collection2 = new[] { 4, 5, 6 };
            var enumerator = new CompositeEnumerator<int>(collection1, collection2);
            var enumeratedItems = new List<int>();

            while (enumerator.MoveNext())
            {
                enumeratedItems.Add(enumerator.Current);
            }

            Assert.That(enumeratedItems, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
        }

        [Test]
        public void Reset_StartsEnumerationFromFirstCollection()
        {
            var collection1 = new[] { 1 };
            var collection2 = new[] { 4 };
            var enumerator = new CompositeEnumerator<int>(collection1, collection2);
            enumerator.MoveNext();
            Assert.That(enumerator.Current, Is.EqualTo(1));

            enumerator.Reset();

            enumerator.MoveNext();
            Assert.That(enumerator.Current, Is.EqualTo(1));
        }
    }
}
