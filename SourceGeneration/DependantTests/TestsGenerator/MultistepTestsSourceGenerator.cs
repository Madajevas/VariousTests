#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System;
using System.CodeDom.Compiler;
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
                .Where(static m => m is not null);

            context.RegisterSourceOutput(
                testClassDeclarations.Combine(context.CompilationProvider),
                static (spc, pair) =>
                {
                    var (target, compilation) = pair;
                    var (classSymbol, classSyntax) = target.Value;
                    GenerateShadowTestClass(spc, classSymbol, classSyntax, compilation);
                });
        }

        private static bool HasAttributes(SyntaxNode node) =>
            node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0;

        private static (INamedTypeSymbol, ClassDeclarationSyntax)? GetSemanticTarget(GeneratorSyntaxContext context)
        {
            if (!(context.Node is ClassDeclarationSyntax testClassDeclaration) || !(context.SemanticModel.GetDeclaredSymbol(testClassDeclaration) is INamedTypeSymbol symbol))
            {
                return null;
            }

            if (context.SemanticModel.Compilation.GetTypeByMetadataName("TestsGenerator.Abstractions.MultistepAttribute") is not INamedTypeSymbol multistepAttribute)
            {
                return null;
            }

            return symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, multistepAttribute)) ? (symbol, testClassDeclaration) : null;
        }

        private static void GenerateShadowTestClass(SourceProductionContext context, INamedTypeSymbol classSymbol, ClassDeclarationSyntax classSyntax, Compilation compilation)
        {
            var methodsInOrder = classSyntax.Members
                .OfType<MethodDeclarationSyntax>()
                .Select(methodSyntax => methodSyntax.Identifier.Text)
                .ToArray();
            var tests = classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.GetAttributes().Any(a => a.AttributeClass.Name.Equals("MultistepParticipantAttribute")))
                .OrderBy(t => methodsInOrder.IndexOf(t.Name))
                .ToArray();

            bool skipGeneration = false;
            var availableForInjection = new OrderedDictionary<ITypeSymbol, IMethodSymbol>(new AsyncOrNotSymbolEqualityComparer());
            var dependencyGraph = new OrderedDictionary<IMethodSymbol, OrderedDictionary<IParameterSymbol, IMethodSymbol>>(SymbolEqualityComparer.Default);
            foreach (var test in tests)
            {
                dependencyGraph[test] = new OrderedDictionary<IParameterSymbol, IMethodSymbol>(SymbolEqualityComparer.Default);

                foreach (var parameter in test.Parameters)
                {
                    if (!availableForInjection.TryGetValue(parameter.Type, out var dependency))
                    {
                        skipGeneration = true;
                        var descriptor = new DiagnosticDescriptor("TG001", "Missing parameter source", "Missing test generating this parameter", "TestsGenerator", DiagnosticSeverity.Error, true);
                        Diagnostic diagnostic = Diagnostic.Create(descriptor, parameter.Locations[0]);
                        context.ReportDiagnostic(diagnostic);
                        continue;
                    }
                    availableForInjection.Remove(parameter.Type);

                    dependencyGraph[test].Add(parameter, dependency);
                }

                if (!test.ReturnsVoid)
                {
                    if (availableForInjection.ContainsKey(test.ReturnType))
                    {
                        skipGeneration = true;
                        var descriptor = new DiagnosticDescriptor("TG002", "There is not consumed param of this type", "Consume already produced parameter first", "TestsGenerator", DiagnosticSeverity.Error, true);
                        Diagnostic diagnostic = Diagnostic.Create(descriptor, test.Locations[0]);
                        context.ReportDiagnostic(diagnostic);
                        continue;
                    }

                    availableForInjection.Add(test.ReturnType, test);
                }
            }
            foreach (var availableValue in availableForInjection)
            {
                var descriptor = new DiagnosticDescriptor("TG003", "Returned value is not consumed", "Returned value is not consumed", "TestsGenerator", DiagnosticSeverity.Warning, true);
                Diagnostic diagnostic = Diagnostic.Create(descriptor, availableValue.Value.Locations[0]);
                context.ReportDiagnostic(diagnostic);
            }

            if (skipGeneration)
            {
                return;
            }


            var ns = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

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

            var keys = dependencyGraph.Keys;
            for (var i = 0; i < keys.Length; i++)
            {
                var test = keys[i];

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
                    var parameters = dependencyGraph[test].Keys;
                    for (var j = 0; j < parameters.Length; j++)
                    {
                        var parameter = parameters[j];
                        var dependency = dependencyGraph[test][parameter];

                        writer.Write($"returnedFrom{dependency.Name}");
                        if (j < parameters.Length - 1)
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
    }
}
