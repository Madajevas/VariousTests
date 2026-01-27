using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Linq;

namespace TestsGenerator
{
    public class CustomSyntaxReceiver : ISyntaxReceiver
    {
        public List<MethodDeclarationSyntax> CandidateMethods { get; } = new();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is MethodDeclarationSyntax methodDeclaration
                && methodDeclaration.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == "MyCustomAttribute")))
            {
                CandidateMethods.Add(methodDeclaration);
            }
        }
    }
}
