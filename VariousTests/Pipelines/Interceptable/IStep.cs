namespace VariousTests.Pipelines.Interceptable
{
    interface IStep
    {
        dynamic Process(dynamic input);
    }

    interface IStep<TInput, TOutput> : IStep
    {
        dynamic IStep.Process(dynamic input) => Process(input);

        TOutput Process(TInput input);
    }
}
