using System;

namespace VariousTests.StateMachines
{
    internal class ManualVsGeneratedTests
    {
        [Test]
        public void ManualImplementation_ProducesCorrectBehaviour()
        {
            List<IDisposable> disposables = new List<IDisposable>();

            var enumerable = new EnumeratorOfDisposables();
            using var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                disposables.Add(enumerator.Current);
            }

            Assert.That(disposables.Count, Is.EqualTo(2));
            foreach (var disposable in disposables)
            {
                disposable.Received().Dispose();
            }
        }

        [Test]
        public void ManualImplementation_WhenConsumingCodeThrows_StillDisposesOfCreatedResources()
        {
            IDisposable disposable = null!;

            try
            {
                var enumerable = new EnumeratorOfDisposables();
                using var enumerator = enumerable.GetEnumerator();
                enumerator.MoveNext();
                disposable = enumerator.Current;
                throw new Exception("Failed to process in some way");
            }
            catch { }

            disposable.Received().Dispose();
        }
    }
}
