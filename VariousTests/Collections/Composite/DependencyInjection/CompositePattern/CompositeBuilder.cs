using Microsoft.Extensions.DependencyInjection;

namespace VariousTests.Collections.Composite.DependencyInjection.CompositePattern
{
    interface IEmptyCompositeBuilder<TAbstraction>
    {
        ICompositeBuilder<TAbstraction> Of<TImplementation>()
            where TImplementation : class, TAbstraction;
    }

    interface ICompositeBuilder<TAbstraction> : IEmptyCompositeBuilder<TAbstraction>
    {
        IServiceCollection Into<TImplementation>()
            where TImplementation : class, TAbstraction;
    }

    class CompositeBuilder<TAbstraction> : ICompositeBuilder<TAbstraction>
    {
        private readonly IServiceCollection services;
        private readonly IList<Type> components;

        public CompositeBuilder(IServiceCollection services)
        {
            this.services = services;
            this.components = new List<Type>();
        }

        public ICompositeBuilder<TAbstraction> Of<TImplementation>()
            where TImplementation : class, TAbstraction
        {
            services.AddSingleton<TImplementation>();
            components.Add(typeof(TImplementation));

            return this;
        }

        public IServiceCollection Into<TImplementation>()
            where TImplementation : class, TAbstraction
        {
            var compositeDescriptor = new ServiceDescriptor(
                serviceType: typeof(TAbstraction),
                factory: provider => BuildComposite<TImplementation>(provider)!,
                lifetime: ServiceLifetime.Singleton);
            services.Add(compositeDescriptor);

            return services;
        }

        private TAbstraction BuildComposite<TImplementation>(IServiceProvider provider)
        {
            var constructor = typeof(TImplementation).GetConstructors().Single();
            if (!constructor.GetParameters().Any(p => p.ParameterType == typeof(IEnumerable<TAbstraction>)))
            {
                throw new InvalidOperationException($"{typeof(TAbstraction)} composite constructor must accept {typeof(IEnumerable<TAbstraction>)} as parameter");
            }

            var parameters = constructor.GetParameters().Select(
                p => p.ParameterType == typeof(IEnumerable<TAbstraction>)
                    ? /* 3.4 */components.Select(type => (TAbstraction)provider.GetService(type)!)
                    : provider.GetService(p.ParameterType));

            return (TAbstraction)constructor.Invoke(parameters.ToArray());
        }
    }
}
