using Microsoft.Extensions.DependencyInjection;


namespace VariousTests.Collections.Composite.DependencyInjection.CompositePattern
{
    internal static class CompositeDependencyInjection
    {
        public static ICompositeBuilder<TAbstraction> Compose<TAbstraction>(this IServiceCollection services)
        {
            return new CompositeBuilder<TAbstraction>(services);
        }
    }
}
