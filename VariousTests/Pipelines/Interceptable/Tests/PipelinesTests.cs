using Microsoft.Extensions.DependencyInjection;

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

        private Pipeline<int, string> pipeline = null!;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton<DoublePipelineStep>();
            services.AddSingleton<ToStringPipelineStep>();
            var provider = services.BuildServiceProvider();

            var builder = Pipeline.BeginBuilder<int>(provider)
                .AddStep<DoublePipelineStep, long>()
                .AddStep<ToStringPipelineStep, string>();
            pipeline = builder.Build();
        }

        [Test]
        public void PipelineBuilder_BuildsPipelineThatInvokesAllSteps()
        {
            var result = pipeline.Execute(11);

            Assert.That(result, Is.EqualTo("22"));
        }

        [Test]
        public void InvokingSamePipelineAgain_ProducesCorrectResult()
        {
            pipeline.Execute(11);
            var result = pipeline.Execute(2);

            Assert.That(result, Is.EqualTo("4"));
        }
    }
}
