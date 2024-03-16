namespace VariousTests.Pipelines.Interceptable
{
    class Pipeline<TInput, TOutput>
    {
        private readonly List<Type> stepTypes;
        private readonly IServiceProvider serviceProvider;

        public Pipeline(List<Type> stepTypes, IServiceProvider serviceProvider)
        {
            this.stepTypes = stepTypes;
            this.serviceProvider = serviceProvider;
        }

        public virtual async ValueTask<TOutput> Execute(TInput input, CancellationToken cancellationToken)
        {
            dynamic nextInput = input;
            foreach(var stepType in stepTypes)
            {
                var step = serviceProvider.GetService(stepType) as IStep ?? throw new InvalidOperationException("Pipeline step not registered.");

                nextInput = await step.Process(nextInput, cancellationToken);
            }

            return (TOutput)nextInput!;
        }
    }
}
