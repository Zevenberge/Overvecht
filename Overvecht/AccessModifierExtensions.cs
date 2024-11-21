namespace Overvecht;

internal static class AccessModifierExtensions
{
    public static string ToCSharpAccessModifier(this AccessModifier accessModifier)
        => accessModifier switch {
            AccessModifier.Public => "public",
            AccessModifier.Private => "private",
            _ => "",
        };
}