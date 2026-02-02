using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TestsGenerator.Tests
{
    public class DiagnosticsTests
    {
        [Test]
        public async Task Generator_ReportsDiagnosticsOnMissingParameters()
        {
            var source = """
                using TestsGenerator.Abstractions;

                [Multistep]
                internal partial class MissingParamTest
                {
                    [MultistepParticipant]
                    public void MissingParam(int missing) {}
                }
                """;
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            IEnumerable<PortableExecutableReference> references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            };
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName: "Tests", new[] { syntaxTree }, references);

            var generator = new CustomSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            var results = driver.GetRunResult();

            await Verifier.Verify(results)
                .UseDirectory("Snapshots")
                .ToTask();
        }
    }
}
