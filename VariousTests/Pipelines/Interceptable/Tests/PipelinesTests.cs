using Microsoft.Extensions.DependencyInjection;

namespace VariousTests.Pipelines.Interceptable.Tests
{
    internal class PipelinesTests
    {
        class DoublePipelineStep : IStep<int, long>
        {
            public ValueTask<long> Process(int input, CancellationToken cancellationToken) => ValueTask.FromResult(input * 2L);
        }

        class ToStringPipelineStep : IStep<long, string>
        {
            public ValueTask<string> Process(long input, CancellationToken cancellationToken) => ValueTask.FromResult(input.ToString());
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
        public async Task PipelineBuilder_BuildsPipelineThatInvokesAllSteps()
        {
            var result = await pipeline.Execute(11, default);

            Assert.That(result, Is.EqualTo("22"));
        }

        [Test]
        public async Task InvokingSamePipelineAgain_ProducesCorrectResult()
        {
            await pipeline.Execute(11, default);
            var result = await pipeline.Execute(2, default);

            Assert.That(result, Is.EqualTo("4"));
        }
    }
}
