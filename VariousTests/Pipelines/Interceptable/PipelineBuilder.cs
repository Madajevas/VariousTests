namespace VariousTests.Pipelines.Interceptable
{
    internal class InitialPipelineBuilder<TInput>
    {
        private Queue<Type> stepTypes;

        public static InitialPipelineBuilder<TInput> Create() =>
            new InitialPipelineBuilder<TInput>();

        private InitialPipelineBuilder()
        {
            this.stepTypes = new Queue<Type>();
        }

        public NextPipelineBuilder<TInput, TOutput> AddStep<TStep, TOutput>() where TStep : IStep<TInput, TOutput>
        {
            stepTypes.Enqueue(typeof(TStep));

            return new NextPipelineBuilder<TInput, TOutput>(stepTypes);
        }
    }

    internal class NextPipelineBuilder<TFirst, TInput>
    {
        private readonly Queue<Type> stepTypes;

        public NextPipelineBuilder(Queue<Type> stepTypes)
        {
            this.stepTypes = stepTypes;
        }

        public NextPipelineBuilder<TFirst, TOutput> AddStep<TStep, TOutput>() where TStep : IStep<TInput, TOutput>
        {
            stepTypes.Enqueue(typeof(TStep));

            return new NextPipelineBuilder<TFirst, TOutput>(stepTypes);
        }

        public Pipeline<TFirst, TInput> Build()
        {
            return new Pipeline<TFirst, TInput>(stepTypes);
        }
    }
}
