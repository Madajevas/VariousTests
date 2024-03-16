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

        public virtual TOutput Execute(TInput input)
        {
            dynamic nextInput = input;
            foreach(var stepType in stepTypes)
            {
                var step = serviceProvider.GetService(stepType) as IStep ?? throw new InvalidOperationException("Pipeline step not registered.");

                nextInput = step.Process(nextInput);
            }

            return (TOutput)nextInput!;
        }
    }
}
