using Microsoft.Extensions.DependencyInjection;

namespace VariousTests.ChainsOfResponsibility.Cancellable.Tests
{
    internal class CancelChainTests
    {
        private IChainHandler chain;
        private TaskCompletionSource firstHandlerBlocker;

        class FirstHandler : IChainHandler
        {
            private readonly IChainHandler next;
            private readonly Task waiter;

            public FirstHandler(IChainHandler next, Task waiter)
            {
                this.next = next;
                this.waiter = waiter;
            }

            public async ValueTask<string> Handle(int request, CancellationToken cancellationToken)
            {
                await waiter;
                return await next.Handle(request, cancellationToken);
            }
        }

        class SecondHandler : IChainHandler
        {
            public ValueTask<string> Handle(int request, CancellationToken cancellationToken)
            {
                return ValueTask.FromResult("second handler return");
            }
        }

        [SetUp]
        public void SetUp()
        {
            firstHandlerBlocker = new TaskCompletionSource();

            var services = new ServiceCollection();
            services.AddSingleton(firstHandlerBlocker.Task);
            var provider = services.BuildServiceProvider();

            var builder = new ChainBuilder(provider);
            builder.Chain<FirstHandler>()
                .Chain<SecondHandler>();

            chain = builder.Build();
        }

        [Test]
        public void WhenOperationIsCancelled_OperationCanceledExceptionIsThrown()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var resultTask = chain.Handle(404, cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();

            firstHandlerBlocker.SetResult();        // unblock first handler after cancelling

            Assert.That(() => resultTask, Throws.InstanceOf<OperationCanceledException>());
        }

        [Test]
        public void WhenSameChainIsInvokedMultipleTimes_ItAlwaysWorks()
        {
            firstHandlerBlocker.SetResult();
            var cancellationTokenSource = new CancellationTokenSource();

            chain.Handle(404, cancellationTokenSource.Token).AsTask().WaitAsync(cancellationTokenSource.Token);

            Assert.That(() => chain.Handle(404, cancellationTokenSource.Token), Throws.Nothing);
            Assert.That(() => chain.Handle(404, cancellationTokenSource.Token), Throws.Nothing);
        }

        [Test]
        public void DisposingCanceller_CancelsChainExecution()
        {
            var cancellableChain = chain as ICancellableChain;

            var (resultTask, canceller) = cancellableChain!.Handle(42);
            canceller.Dispose();

            firstHandlerBlocker.SetResult();

            Assert.That(() => resultTask, Throws.InstanceOf<OperationCanceledException>());
        }
    }
}
