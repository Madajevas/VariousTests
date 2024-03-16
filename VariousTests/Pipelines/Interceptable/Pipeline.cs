namespace VariousTests.Pipelines.Interceptable
{
    class Pipeline<TInput, TOutput>
    {
        private readonly List<Type> stepTypes;

        public Pipeline(List<Type> stepTypes)
        {
            this.stepTypes = stepTypes;
        }

        public virtual TOutput Execute(TInput input)
        {
            dynamic nextInput = input;
            foreach(var stepType in stepTypes)
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
