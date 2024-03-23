namespace VariousTests.ChainsOfResponsibility.Cancellable
{
    interface ICancellableChain
    {
        (ValueTask<string> ResultTask, IDisposable Canceller) Handle(int request);
    }

    class ChainInvoker : IChainHandler, ICancellableChain
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

        public (ValueTask<string> ResultTask, IDisposable Canceller) Handle(int request)
        {
            var cts = new CancellationTokenSource();

            return (Handle(request, cts.Token), cts);
        }
    }

    class ChainCanceller : IDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource;

        public ChainCanceller(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
