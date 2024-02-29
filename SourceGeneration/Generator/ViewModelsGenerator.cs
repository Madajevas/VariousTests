using Generator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Immutable;
using System.Linq;

namespace Generator
{
    [Generator]
    public class ViewModelsGenerator : ISourceGenerator
    {
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
