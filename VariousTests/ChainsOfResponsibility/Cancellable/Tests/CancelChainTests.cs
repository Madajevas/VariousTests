using Microsoft.Extensions.DependencyInjection;

namespace VariousTests.ChainsOfResponsibility.Cancellable.Tests
{
    internal class CancelChainTests
    {
        private IChain chain;
        private TaskCompletionSource firstHandlerBlocker;

        class FirstHandler : IChain
        {
            private readonly IChain next;
            private readonly Task waiter;

            public FirstHandler(IChain next, Task waiter)
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

        class SecondHandler : IChain
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

        [Test, Timeout(10_000)]
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

            Assert.That(() => chain.Handle(404, cancellationTokenSource.Token), Throws.Nothing);
            Assert.That(() => chain.Handle(404, cancellationTokenSource.Token), Throws.Nothing);
        }
    }
}
