namespace VariousTests.ChainsOfResponsibility.Cancellable
{
    class ChainBuilder
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Queue<Type> handlerTypes;

        public ChainBuilder(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.handlerTypes = new Queue<Type>();
        }

        public ChainBuilder Chain<THandler>() where THandler : IChain
        {
            handlerTypes.Enqueue(typeof(THandler));

            return this;
        }

        public IChain Build()
        {
            return new ChainInvoker(handlerTypes, serviceProvider);
        }
    }
}
