using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Overvecht
{
    [Generator]
    public class BuilderGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                            fullyQualifiedMetadataName: typeof(BuilderAttribute).FullName!,
                            predicate: static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
                            transform: static (context, cancellationToken) =>
                            {
                                return context.TargetNode.BuildClassModel();
                            }
                        );

            context.RegisterSourceOutput(pipeline, GenerateSourceCode);
        }

        private static void GenerateSourceCode(SourceProductionContext context, ClassModel? model)
        {
            if (model == null) return;
            var sourceText = SourceText.From($$"""
                namespace {{model.Namespace}};
                partial class {{model.ClassName}}
                {
                    public class Builder
                    {
                        {{
                            string.Join("\t\t\n", model.Properties.Select(p => 
                                $"private {p.Type} m_{p.Name}; public Builder With{p.Name}({p.Type} value) {{ m_{p.Name} = value; return this; }}"
                            ))
                        }}

                        public {{model.ClassName}} Build()
                        {
                            return new(
                                {{ string.Join(", ", model.Properties.Select(p => $"m_{p.Name}")) }}
                            );
                        }
                    }
                }
                """, Encoding.UTF8);

            context.AddSource($"{model.Namespace}.{model.ClassName}.Builder.g.cs", sourceText);
        }
    }
}
