namespace VariousTests.Pipelines.Interceptable
{
    interface IStep
    {
        ValueTask<dynamic> Process(dynamic input, CancellationToken cancellationToken);
    }

    interface IStep<TInput, TOutput> : IStep
    {
        async ValueTask<dynamic> IStep.Process(dynamic input, CancellationToken cancellationToken) => await Process(input, cancellationToken);

        ValueTask<TOutput> Process(TInput input, CancellationToken cancellationToken);
    }
}
