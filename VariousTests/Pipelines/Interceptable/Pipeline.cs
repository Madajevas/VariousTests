namespace VariousTests.Pipelines.Interceptable
{
    internal class Pipeline<TInput, TOutput>
    {
        private readonly Queue<Type> stepTypes;

        public Pipeline(Queue<Type> stepTypes)
        {
            this.stepTypes = stepTypes;
        }

        public TOutput Execute(TInput input)
        {
            dynamic nextInput = input;
            while (stepTypes.TryDequeue(out var stepType))
            {
                var step = CreateHandler(stepType);

                nextInput = step.Process(nextInput);
            }

            return (TOutput)nextInput!;
        }

        private IStep CreateHandler(Type stepType)
        {
            var constructor = stepType.GetConstructors().Single(); // single constructor or die

            return (IStep)constructor.Invoke(Array.Empty<object>());
        }
    }
}
