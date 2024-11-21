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
            AddAllArgsConstructorAttribute(context);
            AddBuilderAttribute(context);
        }

        private static void AddAllArgsConstructorAttribute(IncrementalGeneratorInitializationContext context)
        {
            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                            fullyQualifiedMetadataName: typeof(AllArgsConstructorAttribute).FullName!,
                            predicate: static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
                            transform: static (context, cancellationToken) =>
                            {
                                var classModel = context.TargetNode.BuildClassModel();
                                if (classModel == null) return null;
                                return new AllArgsConstructorModel(classModel, AccessModifier.Public);
                            }
                        );

            context.RegisterSourceOutput(pipeline, GenerateSourceCode);
        }

        private static void AddBuilderAttribute(IncrementalGeneratorInitializationContext context)
        {
            var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
                            fullyQualifiedMetadataName: typeof(BuilderAttribute).FullName!,
                            predicate: static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
                            transform: static (context, cancellationToken) =>
                            {
                                var classModel = context.TargetNode.BuildClassModel();
                                if (classModel == null) return null;
                                return new AllArgsConstructorModel(classModel, AccessModifier.Private);
                            }
                        );

            context.RegisterSourceOutput(pipeline, GenerateSourceCode);
        }

        private static void GenerateSourceCode(SourceProductionContext context, AllArgsConstructorModel? model)
        {
            if (model == null) return;
            var sourceText = SourceText.From($$"""
                namespace {{model.Class.Namespace}};
                partial class {{model.Class.ClassName}}
                {
                    {{model.AccessModifier.ToCSharpAccessModifier()}} {{model.Class.ClassName}}(
                        {{string.Join(", ", model.Class.Properties.Select(
                            p => $"{p.Type} c_{p.Name}"
                        ))}}
                    )
                    {
                        {{string.Join("\r\n", model.Class.Properties.Select(p => $"{p.Name} = c_{p.Name};"))}}
                    }
                }
                """, Encoding.UTF8);

            context.AddSource($"{model.Class.Namespace}.{model.Class.ClassName}.AllArgs.g.cs", sourceText);
        }
    }

    internal record AllArgsConstructorModel(ClassModel Class, AccessModifier AccessModifier);

}
internal static class SyntaxNodeExtensions
{
    internal static ClassModel? BuildClassModel(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax @class)
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
}