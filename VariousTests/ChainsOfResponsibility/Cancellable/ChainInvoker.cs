namespace VariousTests.ChainsOfResponsibility.Cancellable
{
    class ChainInvoker : IChain
    {
        private readonly Queue<Type> handlerTypes;
        private readonly IServiceProvider serviceProvider;

        public ChainInvoker(Queue<Type> handlerTypes, IServiceProvider serviceProvider)
        {
            this.handlerTypes = handlerTypes;
            this.serviceProvider = serviceProvider;
        }

        public ValueTask<string> Handle(int request, CancellationToken cancellationToken)
        {
            var manager = new ChainManager(new Queue<Type>(handlerTypes), serviceProvider);

            return manager.Handle(request, cancellationToken);
        }
    }
}
