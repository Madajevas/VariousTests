using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.ObjectPool;

using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Generator
{
    [Generator]
    public class ViewModelsGenerator : ISourceGenerator
    {
        // private ObjectPool<StringBuilder> stringBuilderPool;
        
        public ViewModelsGenerator()
        {
            // this.stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // System.Diagnostics.Debugger.Break();

            var targetTypeTracker = context.SyntaxContextReceiver as TargetTypeTracker;
            foreach (var typeNode in targetTypeTracker.DiscoveredViewModelTypes)
            {
                var typeNodeSymbol = context.Compilation
                    .GetSemanticModel(typeNode.Item1.SyntaxTree)
                    .GetDeclaredSymbol(typeNode.Item1);
                var sourceType = typeNodeSymbol.GetAttributes().Single().AttributeClass.TypeArguments.Single();
                var members = sourceType.GetMembers().Where(m => m.Kind == SymbolKind.Field).ToImmutableArray();
                var entityClassNamespace = typeNodeSymbol.ContainingNamespace?.ToDisplayString() ?? "???";

                // var builder = stringBuilderPool.Get();
                var builder = new StringBuilder();

                builder.AppendLine($"namespace {entityClassNamespace}");
                builder.AppendLine("{");

                builder.AppendLine($"\tpublic partial class {typeNodeSymbol.Name}");
                builder.AppendLine("\t{");

                builder.AppendLine($"\t\tpublic enum {sourceType.Name}ViewModel");
                builder.AppendLine("\t\t{");
                foreach (var member in members)
                {
                    builder.AppendLine($"\t\t\t{member.Name},");
                }
                builder.AppendLine("\t\t}");

                builder.AppendLine("\t}");

                builder.AppendLine("}");

                context.AddSource(typeNodeSymbol.Name, SourceText.From(builder.ToString(), Encoding.UTF8));

                // stringBuilderPool.Return(builder);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new TargetTypeTracker());
        }
    }
}

public class TargetTypeTracker : ISyntaxContextReceiver
{
    public IImmutableList<(TypeDeclarationSyntax, AttributeSyntax)> DiscoveredViewModelTypes =
        ImmutableList.Create<(TypeDeclarationSyntax, AttributeSyntax)>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not TypeDeclarationSyntax typeDeclarationSyntax)
        {
            return;
        }

        var all = typeDeclarationSyntax.AttributeLists.SelectMany(listSyntax => listSyntax.Attributes).ToArray();
        var generateAttribute = typeDeclarationSyntax.AttributeLists
            .SelectMany(listSyntax => listSyntax.Attributes)
            .Where(attributeSyntax => attributeSyntax.Name.ToString().StartsWith("GenerateViewModel"))
            .SingleOrDefault();

        if (generateAttribute != null)
        {
            DiscoveredViewModelTypes = DiscoveredViewModelTypes.Add((typeDeclarationSyntax, generateAttribute));
        }
    }
}
