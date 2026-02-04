using Microsoft.CodeAnalysis;

using System.Collections.Generic;

namespace TestsGenerator
{
    class AsyncOrNotSymbolEqualityComparer/*(Compilation compilation)*/ : IEqualityComparer<ISymbol?>
    {
        // private readonly INamedTypeSymbol genericTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

        public bool Equals(ISymbol x, ISymbol y)
        {
            if (SymbolEqualityComparer.Default.Equals(x, y))
            {
                return true;
            }

            if (x is INamedTypeSymbol xNs && xNs.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
            {
                return SymbolEqualityComparer.Default.Equals(xNs.TypeArguments[0], y);
            }

            if (y is INamedTypeSymbol yNs && yNs.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
            {
                return SymbolEqualityComparer.Default.Equals(y, yNs.TypeArguments[0]);
            }

            return false;
        }

        public int GetHashCode(ISymbol obj)
        {
            if (obj is INamedTypeSymbol objNs && objNs.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
            {
                return SymbolEqualityComparer.Default.GetHashCode(objNs.TypeArguments[0]);
            }

            return SymbolEqualityComparer.Default.GetHashCode(obj);
        }
    }
}
