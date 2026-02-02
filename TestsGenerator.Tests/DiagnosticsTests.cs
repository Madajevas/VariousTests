using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TestsGenerator.Tests
{
    public class DiagnosticsTests
    {
        [Test]
        public async Task Generator_WhenThereAreMissingDependencies_ReportsDiagnostics()
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
            IEnumerable<PortableExecutableReference> references =
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ];
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName: "Tests", [syntaxTree], references);

            var generator = new CustomSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            var results = driver.GetRunResult().Diagnostics;

            await Verifier.Verify(results)
                .UseDirectory("Snapshots")
                .ToTask();
        }

        [Test]
        public async Task Generator_WhenThereIsAnAmbiguityInDependencies_ReportsDiagnostics()
        {
            var source = """
                using TestsGenerator.Abstractions;

                [Multistep]
                internal partial class MissingParamTest
                {
                    [MultistepParticipant]
                    public int ParamProvider1() => 1;

                    [MultistepParticipant]
                    public int ParamProvider2() => 2;

                    [MultistepParticipant]
                    public void AmbiguousParam(int ambiguous) {}
                }
                """;
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            IEnumerable<PortableExecutableReference> references =
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            ];
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName: "Tests", [syntaxTree], references);

            var generator = new CustomSourceGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            var results = driver.GetRunResult().Diagnostics;

            await Verifier.Verify(results)
                .UseDirectory("Snapshots")
                .ToTask();
        }
    }
}
