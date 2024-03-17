namespace VariousTests.ChainsOfResponsibility.Cancellable
{
    class ChainManager : IChain
    {
        private readonly Queue<Type> handlerTypes;
        private readonly IServiceProvider serviceProvider;

        public ChainManager(Queue<Type> handlerTypes, IServiceProvider serviceProvider)
        {
            this.handlerTypes = handlerTypes;
            this.serviceProvider = serviceProvider;
        }

        public ValueTask<string> Handle(int request, CancellationToken cancellationToken)
        {
            while (handlerTypes.TryDequeue(out var handlerType))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var handler = CreateHandler(handlerType);
                return handler.Handle(request, cancellationToken);
            }

            throw new System.Diagnostics.UnreachableException("Chain was empty or last chain element injected another elements needlessly.");
        }

        private IChain CreateHandler(Type handlerType)
        {
            var constructor = handlerType.GetConstructors().Single();
            var parameters = constructor.GetParameters()
                .Select(p => p.ParameterType.IsAssignableFrom(this.GetType()) ? this : serviceProvider.GetService(p.ParameterType))
                .ToArray();

            return (IChain)constructor.Invoke(parameters);
        }
    }
}
