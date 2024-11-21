namespace Overvecht;


[AttributeUsage(AttributeTargets.Class)]
public class AllArgsConstructorAttribute : Attribute
{
    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;
}
