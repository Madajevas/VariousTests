namespace VariousTests.Pipelines.Interceptable.Tests
{
    internal class PipelinesTests
    {
        class DoublePipelineStep : IStep<int, long>
        {
            public long Process(int input) => input * 2;
        }

        class ToStringPipelineStep : IStep<long, string>
        {
            public string Process(long input) => input.ToString();
        }

        [Test]
        public void PipelineBuilder_BuildsPipelineThatInvokesAllSteps()
        {
            var builder = Pipeline.BeginBuilder<int>()
                .AddStep<DoublePipelineStep, long>()
                .AddStep<ToStringPipelineStep, string>();
            Pipeline<int, string> pipeline = builder.Build();

            var result = pipeline.Execute(11);

            Assert.That(result, Is.EqualTo("22"));
        }
    }
}
