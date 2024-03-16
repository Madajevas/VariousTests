namespace VariousTests.Pipelines.Interceptable
{
    class Pipeline
    {
        public static PipelineBuilder<TInput> BeginBuilder<TInput>() =>
            new PipelineBuilder<TInput>();
    }

    class PipelineBuilder<TInput>
    {
        private List<Type> stepTypes;

        public PipelineBuilder()
        {
            this.stepTypes = new List<Type>();
        }

        public PipelineBuilder<TInput, TOutput> AddStep<TStep, TOutput>() where TStep : IStep<TInput, TOutput>
        {
            stepTypes.Add(typeof(TStep));

            return new PipelineBuilder<TInput, TOutput>(stepTypes);
        }
    }

    internal class PipelineBuilder<TFirst, TInput>
    {
        private readonly List<Type> stepTypes;

        public PipelineBuilder(List<Type> stepTypes)
        {
            this.stepTypes = stepTypes;
        }

        public PipelineBuilder<TFirst, TOutput> AddStep<TStep, TOutput>() where TStep : IStep<TInput, TOutput>
        {
            stepTypes.Add(typeof(TStep));

            return new PipelineBuilder<TFirst, TOutput>(stepTypes);
        }

        public Pipeline<TFirst, TInput> Build()
        {
            return new Pipeline<TFirst, TInput>(stepTypes);
        }
    }
}
