using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Overvecht.Tests;

public class AllArgsConstructorTest 
{
    [Fact]
    public void Test1()
    {
        var testCode = """
        namespace Foo;
        using Overvecht;
        [AllArgsConstructor]
        public partial class Bar
        {
            public string Baz { get; }
            public int Xam { get; }
        }

        public static class Program
        {
            public static void Main(string[] args)
            {
                var bar = new Bar("Aaa", 42);
                System.Console.WriteLine(bar);
            }
        }
        """;
        Run(testCode);
    }

    private void Run(string source)
    {
        var ast = CSharpSyntaxTree.ParseText(source);
        var reference = MetadataReference.CreateFromFile(typeof(AllArgsConstructorAttribute).Assembly.Location);
        var compilation = CSharpCompilation.Create("SourceGeneratorTests", [ast], [reference],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var generator = new AllArgsConstructorGenerator();
        CSharpGeneratorDriver.Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation,
                out var outputCompilation,
                out var diagnostics);
    }
}

