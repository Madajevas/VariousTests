using MockQueryable;
using MockQueryable.NSubstitute;

using Various.Collections.Composite.CompositeQueryable;

namespace VariousTests.Collections.Composite.CompositeQueryable
{
    public class CompositeQueryableTests
    {
        [Test]
        public void ProvidedWithTwoCollections_AppliesFilteringToBoth()
        {
            var q1 = new[] { new DummyEntity { Id = 0 }, new DummyEntity { Id = 1 } }.BuildMock();
            var q2 = new[] { new DummyEntity { Id = 1 }, new DummyEntity { Id = 2 } }.BuildMock();
            var composable = new CompositeQueryable<DummyEntity>(q1, q2);

            var list = composable
                .Where(i => i.Id > 0)
                .Where(i => i.Id < 2)
                .ToList();

            Assert.That(list, Is.All.Matches<DummyEntity>(e => e.Id == 1));
        }

        [Test]
        public void ProvidedWithTwoCollections_CanRetrievePropertiesOfCollectionElements()
        {
            var q1 = new[] { new DummyEntity { Id = 0 }, new DummyEntity { Id = 1 } }.BuildMock();
            var q2 = new[] { new DummyEntity { Id = 1 }, new DummyEntity { Id = 2 } }.BuildMock();
            var composable = new CompositeQueryable<DummyEntity>(q1, q2);

            var list = composable
                .Select(i => i.Id)
                .ToList();

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 1, 2 }));
        }

        [Test, Explicit]
        public void ProvidedWithCollectionsContainingDuplicates_CanFilterThoseOut()
        {
            var q1 = new[] { new DummyEntity { Id = 0 }, new DummyEntity { Id = 1 } }.BuildMock();
            var q2 = new[] { new DummyEntity { Id = 1 }, new DummyEntity { Id = 2 } }.BuildMock();
            var composable = new CompositeQueryable<DummyEntity>(q1, q2);

            var list = composable
                .Select(i => i.Id)
                .Distinct()
                .ToList();

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 2 })); // nope. only works on single underlying collection
        }

        [Test, Explicit]
        public void SkipTakeOperations_WorksOnQueryablesAsAWhole()
        {
            var q1 = new[] { new DummyEntity { Id = 0 }, new DummyEntity { Id = 1 } }.BuildMock();
            var q2 = new[] { new DummyEntity { Id = 2 }, new DummyEntity { Id = 3 } }.BuildMock();
            var composable = new CompositeQueryable<DummyEntity>(q1, q2);

            var list = composable.Skip(1).Take(2).Select(i => i.Id).ToList();

            Assert.That(list, Is.EqualTo(new[] { 1, 2 }));  // nope. gets second item of each queryable
        }

        [Test, Explicit]
        public void GroupBy_CanGroupItemsOfMultipleQueryables()
        {
            var q1 = new[] { new DummyEntity { Id = 1 } }.BuildMock();
            var q2 = new[] { new DummyEntity { Id = 1 } }.BuildMock();
            var composable = new CompositeQueryable<DummyEntity>(q1, q2);

            var grouped = composable.GroupBy(i => i.Id).ToList();

            Assert.That(grouped.Count, Is.EqualTo(1)); // nope. items in collections are grouped separately
        }


        [Test]
        public void ProvidedWithUnorderedItemsInBothCollections_CanOrderThem()
        {
            var q1 = new[] { new DummyEntity { Id = 1 }, new DummyEntity { Id = 0 } }.BuildMock();
            var q2 = new[] { new DummyEntity { Id = 3 }, new DummyEntity { Id = 2 } }.BuildMock();
            var composable = new CompositeQueryable<DummyEntity>(q1, q2);

            var list = composable
                .Select(i => i.Id)
                .OrderBy(i => i)
                .ToList();

            Assert.That(list, Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }
    }

    class DummyEntity
    {
        public int Id { get; set; }
    }
}
