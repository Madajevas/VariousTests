namespace VariousTests.Pipelines.Interceptable
{
    class Pipeline
    {
        public static PipelineBuilder<TInput> BeginBuilder<TInput>(IServiceProvider serviceProvider) =>
            new PipelineBuilder<TInput>(serviceProvider);
    }

    class PipelineBuilder<TInput>
    {
        private readonly IServiceProvider serviceProvider;
        private List<Type> stepTypes;

        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            this.stepTypes = new List<Type>();
            this.serviceProvider = serviceProvider;
        }

        public PipelineBuilder<TInput, TOutput> AddStep<TStep, TOutput>() where TStep : IStep<TInput, TOutput>
        {
            stepTypes.Add(typeof(TStep));

            return new PipelineBuilder<TInput, TOutput>(stepTypes, serviceProvider);
        }
    }

    internal class PipelineBuilder<TFirst, TInput>
    {
        private readonly List<Type> stepTypes;
        private readonly IServiceProvider serviceProvider;

        public PipelineBuilder(List<Type> stepTypes, IServiceProvider serviceProvider)
        {
            this.stepTypes = stepTypes;
            this.serviceProvider = serviceProvider;
        }

        public PipelineBuilder<TFirst, TOutput> AddStep<TStep, TOutput>() where TStep : IStep<TInput, TOutput>
        {
            stepTypes.Add(typeof(TStep));

            return new PipelineBuilder<TFirst, TOutput>(stepTypes, serviceProvider);
        }

        public Pipeline<TFirst, TInput> Build()
        {
            return new Pipeline<TFirst, TInput>(stepTypes, serviceProvider);
        }
    }
}
