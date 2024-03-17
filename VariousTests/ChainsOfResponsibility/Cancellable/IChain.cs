namespace VariousTests.ChainsOfResponsibility.Cancellable
{
    interface IChain
    {
        ValueTask<string> Handle(int request, CancellationToken cancellationToken);
    }
}
