using Microsoft.CodeAnalysis;

namespace Generator
{
    [Generator]
    public class ViewModelsGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Code generation goes here
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}
