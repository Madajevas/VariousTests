using Microsoft.Extensions.DependencyInjection;

namespace VariousTests.Collections.Composite.DependencyInjection.CompositePattern
{
    public class CompositeTests
    {
        private Composite? composite;

        [SetUp]
        public void SetUp()
        {
            var composite = new ServiceCollection()
                .Compose<IComposite>()
                .Of<ImplementationOne>()
                .Of<ImplementationTwo>()
                .Into<Composite>()
                .BuildServiceProvider()
                .GetService<IComposite>();

            var services = new ServiceCollection();

            services
                .AddSingleton<DependencyOne>()
                .AddSingleton<DependencyTwo>()
                .Compose<IComposite>()
                    .Of<ImplementationOne>()
                    .Of<ImplementationTwo>()
                    .Into<Composite>();

            var provider = services.BuildServiceProvider();

            this.composite = provider.GetService<IComposite>() as Composite;
        }

        [Test]
        public void BuiltComposite_CanBeResolved()
        {
            Assert.That(composite, Is.Not.Null);
        }

        [Test]
        public void ExtraCompositeDependency_IsInjectedSuccessfully()
        {
            Assert.That(composite!.DependencyTwo, Is.Not.Null);
        }

        [Test]
        public void Components_AreInjectedCorrectly()
        {
            var components = composite!.Components.ToArray();

            Assert.That(components[0], Is.InstanceOf<ImplementationOne>());
            Assert.That(components[1], Is.InstanceOf<ImplementationTwo>());
        }

        [Test]
        public void InjectedComponents_MayHaveDependenciesOfTheirOwn()
        {
            var implementationOne = composite!.Components.First() as ImplementationOne;

            Assert.That(implementationOne!.DependencyOne, Is.Not.Null);
        }

        [Test]
        public void BlogExample()
        {
            var services = new ServiceCollection();

            services
                .AddSingleton<DependencyOne>()
                .AddSingleton<DependencyTwo>()
                .Compose<IComposite>()
                    .Of<ImplementationOne>()
                    .Of<ImplementationTwo>()
                    .Into<Composite>();
            var provider = services.BuildServiceProvider();

            var composite = provider.GetService<IComposite>() as Composite;
            Assert.That(composite, Is.Not.Null);
            var components = composite.Components.ToArray();
            Assert.That(components[0], Is.InstanceOf<ImplementationOne>());
            Assert.That(components[1], Is.InstanceOf<ImplementationTwo>());
            Assert.That(composite.DependencyTwo, Is.Not.Null);
            Assert.That((components[0] as ImplementationOne)!.DependencyOne, Is.Not.Null);
        }
    }

    class DependencyOne { }
    class DependencyTwo { }

    interface IComposite { }

    class ImplementationOne : IComposite
    {
        public DependencyOne DependencyOne { get; }

        public ImplementationOne(DependencyOne dependencyOne)
        {
            DependencyOne = dependencyOne;
        }
    }
    class ImplementationTwo : IComposite { }
    class Composite : IComposite
    {
        public IEnumerable<IComposite> Components { get; set; }
        public DependencyTwo DependencyTwo { get; }

        public Composite(IEnumerable<IComposite> components, DependencyTwo dependencyTwo)
        {
            Components = components;
            DependencyTwo = dependencyTwo;
        }
    }
}
