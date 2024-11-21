using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Overvecht
{
    [Generator]
    public class AllArgsConstructorGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(AllArgsConstructorAttribute).FullName!,
                predicate: static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
                transform: static (context, cancellationToken) =>
                {
                    if (context.TargetNode is ClassDeclarationSyntax @class)
                    {
                        var properties = @class.Members.OfType<PropertyDeclarationSyntax>()
                            .Select(p => new PropertyModel(p.Type.ToString(), p.Identifier.ToString()))
                            .ToArray();
                        var @namespace = @class.Parent as BaseNamespaceDeclarationSyntax;
                        var namespaceName = @namespace?.Name.GetText().ToString() ?? "";

                        return new ClassModel(namespaceName, @class.Identifier.ToString(), properties);
                    }
                    return null;
                }
            );

            context.RegisterSourceOutput(pipeline, static (context, model) =>
            {
                if(model == null) return;
                var sourceText = SourceText.From($$"""
                namespace {{model.Namespace}};
                partial class {{model.ClassName}}
                {
                    public {{model.ClassName}}(
                        {{string.Join(", ", model.Properties.Select(
                                p => $"{p.Type} c_{p.Name}"
                            ))}}
                    )
                    {
                        {{
                                string.Join("\r\n", model.Properties.Select(p => $"{p.Name} = c_{p.Name};"))
                            }}
                    }
                }
                """, Encoding.UTF8);

                context.AddSource($"{model.ClassName}.AllArgs.g.cs", sourceText);
            });
        }
    }

    internal record ClassModel(string Namespace, string ClassName, PropertyModel[] Properties);
    internal record PropertyModel(string Type, string Name);
}