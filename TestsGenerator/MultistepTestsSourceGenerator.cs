using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestsGenerator
{
    [Generator]
    public class MultistepTestsSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(
                ctx => ctx.AddSource("MultistepAttribute.g.cs", SourceText.From(MultistepAttribute.Code, Encoding.UTF8)));
            context.RegisterPostInitializationOutput(
                ctx => ctx.AddSource("MultistepParticipantAttribute.g.cs", SourceText.From(MultistepParticipant.Code, Encoding.UTF8)));

            var testClassDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => HasAttributes(s), // quick filter
                    transform: static (ctx, _) => GetSemanticTarget(ctx)) // get symbol
                .Where(static m => m is not null)!;

            context.RegisterSourceOutput(
                testClassDeclarations.Combine(context.CompilationProvider),
                static (spc, pair) =>
                {
                    var (classSymbol, compilation) = pair;
                    GenerateShadowTestClass(spc, classSymbol!, compilation);
                });
        }

        private static bool HasAttributes(SyntaxNode node) =>
            node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0;

        private static INamedTypeSymbol? GetSemanticTarget(GeneratorSyntaxContext context)
        {
            if (!(context.Node is ClassDeclarationSyntax testClassDeclaration) || !(context.SemanticModel.GetDeclaredSymbol(testClassDeclaration) is INamedTypeSymbol symbol))
            {
                return null;
            }

            if (context.SemanticModel.Compilation.GetTypeByMetadataName("TestsGenerator.Abstractions.MultistepAttribute") is not INamedTypeSymbol multistepAttribute)
            {
                return null;
            }

            return symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, multistepAttribute)) ? symbol : null;
        }

        private static bool CompareTypes(ITypeSymbol left, ITypeSymbol right)
        {
            if (SymbolEqualityComparer.Default.Equals(left, right))
            {
                return true;
            }

            if (left is INamedTypeSymbol leftNs && leftNs.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
            {
                return SymbolEqualityComparer.Default.Equals(leftNs.TypeArguments[0], right);
            }

            if (left is INamedTypeSymbol rightNs && rightNs.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
            {
                return SymbolEqualityComparer.Default.Equals(left, rightNs.TypeArguments[0]);
            }

            return false;
        }

        private static void GenerateShadowTestClass(SourceProductionContext context, INamedTypeSymbol classSymbol, Compilation compilation)
        {
            var ns = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

            var tests = classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.GetAttributes().Any(a => a.AttributeClass.Name.Equals("MultistepParticipantAttribute")))
                .OrderBy(t => t.Parameters.Count())
                .ToArray();
            var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var dependencyGraph = new Dictionary<IMethodSymbol, List<IMethodSymbol>>(SymbolEqualityComparer.Default);
            foreach (var test in tests)
            {
                dependencyGraph[test] = new List<IMethodSymbol>(test.Parameters.Length);
                foreach (var param in test.Parameters)
                {
                    //var dependency = tests.Where(t => SymbolEqualityComparer.Default.Equals(t.ReturnType, param.Type) || SymbolEqualityComparer.Default.Equals(t.ReturnType, taskType.Construct(param.Type)));
                    var dependency = tests.Where(t => CompareTypes(t.ReturnType, param.Type));
                    dependencyGraph[test].AddRange(dependency);
                }
            }

            var validationResults = Validate(dependencyGraph, compilation).ToArray();
            foreach (var diagnostic in validationResults)
            {
                context.ReportDiagnostic(diagnostic);
            }
            if (validationResults.Any())
            {
                return;
            }

            var sortedTests = TopologicalSort(dependencyGraph);

            using var writer = new IndentedTextWriter(new StringWriter(), "    ");

            writer.WriteLine("// <auto-generated>");
            writer.WriteLine();
            writer.WriteLine("using NUnit.Framework;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine();
            writer.WriteLine($"namespace {ns}");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"public partial class {className}");
            writer.WriteLine("{");
            writer.Indent++;

            for (var i = 0; i < sortedTests.Count; i++)
            {
                var test = tests[i];

                if (test.ReturnType is INamedTypeSymbol returnType)
                {
                    if (returnType.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task")
                    {
                        writer.WriteLine($"[Test, Order({i})]");
                        writer.WriteLine($"public Task {test.Name}Generated()");
                    }
                    else if (returnType.OriginalDefinition.ToDisplayString() == "void")
                    {
                        writer.WriteLine($"[Test, Order({i})]");
                        writer.WriteLine($"public void {test.Name}Generated()");
                    }
                    else if (returnType.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
                    {
                        writer.WriteLine($"private {returnType.TypeArguments[0].Name} returnedFrom{test.Name};");
                        writer.WriteLine($"[Test, Order({i})]");
                        writer.WriteLine($"public async Task {test.Name}Generated()");
                    }
                    else
                    {
                        writer.WriteLine($"private {test.ReturnType.Name} returnedFrom{test.Name};");
                        writer.WriteLine($"[Test, Order({i})]");
                        writer.WriteLine($"public void {test.Name}Generated()");
                    }
                    writer.WriteLine("{");
                    writer.Indent++;

                    if (returnType.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
                    {
                        writer.Write($"this.returnedFrom{test.Name} = await ");
                    }
                    else if (returnType.OriginalDefinition.ToDisplayString() != "void")
                    {
                        writer.Write($"this.returnedFrom{test.Name} = ");
                    }

                    writer.Write($"this.{test.Name}(");
                    var requires = dependencyGraph[test].ToDictionary(k => k.ReturnType, v => v, new AsyncOrNotSymbolEqualityComparer());
                    for (var j = 0; j < test.Parameters.Count(); j++)
                    {
                        var param = test.Parameters[j];
                        var requiredTest = requires[param.Type];
                        requires.Remove(param.Type);
                        writer.Write($"returnedFrom{requiredTest.Name}");
                        if (j < test.Parameters.Count() - 1)
                        {
                            writer.Write(", ");
                        }
                    }
                    writer.WriteLine(");");

                    writer.Indent--;
                    writer.WriteLine("}");
                    writer.WriteLine();

                    System.Diagnostics.Debugger.Break();
                }
            }

            writer.Indent--;
            writer.WriteLine("}"); // class
            writer.Indent--;
            writer.WriteLine("}"); // namespace

            context.AddSource($"{className}Multistep.Incremental.g.cs", SourceText.From(writer.InnerWriter.ToString(), Encoding.UTF8));
        }

        private static IEnumerable<Diagnostic> Validate(IReadOnlyDictionary<IMethodSymbol, List<IMethodSymbol>> dependencyGraph, Compilation compilation)
        {
            var taskType = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            foreach (var testToDependencies in dependencyGraph)
            {
                var test = testToDependencies.Key;
                var dependencies = testToDependencies.Value;

                if (test.Parameters.Length == 0)
                {
                    continue;
                }

                foreach (var parameter in test.Parameters)
                {
                    var requiredTests = dependencies.Where(d => CompareTypes(d.ReturnType, parameter.Type)).Take(2).ToArray();

                    if (requiredTests.Length == 0)
                    {
                        var descriptor = new DiagnosticDescriptor("TG001", "Missing parameter source", "Missing test generating this parameter", "TestsGenerator", DiagnosticSeverity.Error, true);
                        Diagnostic diagnostic = Diagnostic.Create(descriptor, parameter.Locations[0]);
                        yield return diagnostic;
                    }
                    else if (requiredTests.Length > 1)
                    {
                        var descriptor = new DiagnosticDescriptor("TG002", "Ambiguous parameter source", "Multiple tests generating this parameter", "TestsGenerator", DiagnosticSeverity.Error, true);
                        Diagnostic diagnostic = Diagnostic.Create(descriptor, parameter.Locations[0]);
                        yield return diagnostic;
                    }
                }
            }
        }

        private static List<IMethodSymbol> TopologicalSort(Dictionary<IMethodSymbol, List<IMethodSymbol>> graph)
        {
            var sorted = new List<IMethodSymbol>();
            var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var visiting = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

            void Visit(IMethodSymbol node)
            {
                if (visited.Contains(node)) return;
                if (visiting.Contains(node))
                {
                    throw new InvalidOperationException("Cyclic dependency detected in test methods.");
                }

                visiting.Add(node);
                foreach (var dependency in graph[node])
                {
                    Visit(dependency);
                }
                visiting.Remove(node);
                visited.Add(node);
                sorted.Add(node);
            }

            foreach (var node in graph.Keys)
            {
                Visit(node);
            }

            return sorted;
        }
    }
}
