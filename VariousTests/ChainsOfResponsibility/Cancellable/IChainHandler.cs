namespace VariousTests.ChainsOfResponsibility.Cancellable
{
    interface IChainHandler
    {
        ValueTask<string> Handle(int request, CancellationToken cancellationToken);
    }
}
