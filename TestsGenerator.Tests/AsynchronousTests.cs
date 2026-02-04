using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TestsGenerator.Tests
{
    internal class AsynchronousTests
    {
        [Test]
        public async Task Generator_WhenTestAsyncTestReturns_AwaitsForValueBeforeInjectingToNextTest()
        {
            var source = """
                using System.Threading.Tasks;
                using TestsGenerator.Abstractions;

                namespace TestsGenerator.Tests;

                [Multistep]
                internal partial class AsyncProducedParamTest
                {
                    [MultistepParticipant]
                    public Task<int> ProduceAsync() => Task.FromResult(1);

                    [MultistepParticipant]
                    public void Receiver(int value) {}
                }
                """;
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            IEnumerable<PortableExecutableReference> references =
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location)
            ];
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName: "Tests", [syntaxTree], references);

            var generator = new CustomSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            var results = driver.GetRunResult().Results.Single().GeneratedSources.Single(s => s.HintName == "AsyncProducedParamTestMultistep.Incremental.g.cs");

            await Verifier.Verify(results)
                .UseDirectory("Snapshots")
                .ToTask();
        }
    }
}
